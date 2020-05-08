﻿
var Recorder = /** @class */ (function () {
    function Recorder(cfg) {
        var _this = this;
        if (cfg === void 0) { cfg = {}; }
        this.config = {
            bufferLen: 4096,
            numChannels: 1,
            mimeType: "audio/wav",
            sampleRate: 16000
        };
        this.recording = false;
        this.callbacks = {
            getBuffer: [],
            exportWAV: []
        };
        //@ts-ignore
        Object.assign(this.config, cfg);
        var self = {};
        this.worker = new InlineWorker(function () {
            var recLength = 0, recBuffers = [], sampleRate, numChannels;
            this.onmessage = function (e) {
                switch (e.data.command) {
                    case "init":
                        init(e.data.config);
                        break;
                    case "record":
                        record(e.data.buffer);
                        break;
                    case "exportWAV":
                        exportWAV(e.data.type);
                        break;
                    case "getBuffer":
                        getBuffer();
                        break;
                    case "clear":
                        clear();
                        break;
                }
            };
            function init(config) {
                sampleRate = config.sampleRate;
                numChannels = config.numChannels;
                initBuffers();
            }
            function record(inputBuffer) {
                for (var channel = 0; channel < numChannels; channel++) {
                    recBuffers[channel].push(inputBuffer[channel]);
                }
                recLength += inputBuffer[0].length;
            }
            function exportWAV(type) {
                var buffers = [];
                for (var channel = 0; channel < numChannels; channel++) {
                    buffers.push(mergeBuffers(recBuffers[channel], recLength));
                }
                var interleaved;
                if (numChannels === 2) {
                    interleaved = interleave(buffers[0], buffers[1]);
                }
                else {
                    interleaved = buffers[0];
                }
                var dataview = encodeWAV(interleaved);
                var audioBlob = new Blob([dataview], { type: type });
                this.postMessage({ command: "exportWAV", data: audioBlob });
            }
            function getBuffer() {
                var buffers = [];
                for (var channel = 0; channel < numChannels; channel++) {
                    buffers.push(mergeBuffers(recBuffers[channel], recLength));
                }
                this.postMessage({ command: "getBuffer", data: buffers });
            }
            function clear() {
                recLength = 0;
                recBuffers = [];
                initBuffers();
            }
            function initBuffers() {
                for (var channel = 0; channel < numChannels; channel++) {
                    recBuffers[channel] = [];
                }
            }
            function mergeBuffers(recBuffers, recLength) {
                var result = new Float32Array(recLength);
                var offset = 0;
                for (var i = 0; i < recBuffers.length; i++) {
                    result.set(recBuffers[i], offset);
                    offset += recBuffers[i].length;
                }
                return result;
            }
            function interleave(inputL, inputR) {
                var length = inputL.length + inputR.length;
                var result = new Float32Array(length);
                var index = 0, inputIndex = 0;
                while (index < length) {
                    result[index++] = inputL[inputIndex];
                    result[index++] = inputR[inputIndex];
                    inputIndex++;
                }
                return result;
            }
            function floatTo16BitPCM(output, offset, input) {
                for (var i = 0; i < input.length; i++, offset += 2) {
                    var s = Math.max(-1, Math.min(1, input[i]));
                    output.setInt16(offset, s < 0 ? s * 0x8000 : s * 0x7FFF, true);
                }
            }
            function writeString(view, offset, string) {
                for (var i = 0; i < string.length; i++) {
                    view.setUint8(offset + i, string.charCodeAt(i));
                }
            }
            function encodeWAV(samples) {
                var buffer = new ArrayBuffer(44 + samples.length * 2);
                var view = new DataView(buffer);
                debugger
                /* RIFF identifier */
                writeString(view, 0, "RIFF");
                /* RIFF chunk length */
                view.setUint32(4, 36 + samples.length * 2, true);
                /* RIFF type */
                writeString(view, 8, "WAVE");
                /* format chunk identifier */
                writeString(view, 12, "fmt ");
                /* format chunk length */
                view.setUint32(16, 16, true);
                /* sample format (raw) */
                view.setUint16(20, 1, true);
                /* channel count */
                view.setUint16(22, numChannels, true);
                /* sample rate */
                view.setUint32(24, sampleRate, true);
                /* byte rate (sample rate * block align) */
                view.setUint32(28, sampleRate * 2, true);
                /* block align (channel count * bytes per sample) */
                view.setUint16(32, numChannels * 2, true);
                /* bits per sample */
                view.setUint16(34, 16, true);
                /* data chunk identifier */
                writeString(view, 36, "data");
                /* data chunk length */
                view.setUint32(40, samples.length * 2, true);
                floatTo16BitPCM(view, 44, samples);
                return view;
            }
        }, self);
        this.worker.postMessage({
            command: "init",
            config: {
                sampleRate: this.config.sampleRate,
                numChannels: this.config.numChannels
            }
        });
        this.worker.onmessage = function (e) {
            var cb = _this.callbacks[e.data.command].pop();
            if (typeof cb == "function") {
                cb(e.data.data);
            }
        };
    }
    Recorder.prototype.recordInternal = function () {
        var _this = this;
        navigator.mediaDevices.getUserMedia({ audio: true }).then(function (stream) {
            //@ts-ignore
            _this.audioContext = new AudioContext({ sampleRate: _this.config.sampleRate });
            _this.inputStream &&
                _this.inputStream.mediaStream.getTracks().forEach(function (t) { t.stop(); });
            _this.inputStream = _this.audioContext.createMediaStreamSource(stream);
            _this.context = _this.inputStream.context;
            _this.node = (_this.context.createScriptProcessor || _this.context.createJavaScriptNode)
                .call(_this.context, _this.config.bufferLen, _this.config.numChannels, _this.config.numChannels);
            _this.node.onaudioprocess = function (e) {
                if (!_this.recording)
                    return;
                var buffer = [];
                for (var channel = 0; channel < _this.config.numChannels; channel++) {
                    buffer.push(e.inputBuffer.getChannelData(channel));
                }
                _this.worker.postMessage({
                    command: "record",
                    buffer: buffer
                });
            };
            _this.inputStream.connect(_this.node);
            _this.node.connect(_this.context.destination); //this should not be necessary
            _this.worker.postMessage({
                command: "init",
                config: {
                    sampleRate: _this.context.sampleRate,
                    numChannels: _this.config.numChannels
                }
            });
        });
    };
    Recorder.prototype.record = function () {
        this.recordInternal();
        this.recording = true;
    };
    Recorder.prototype.stop = function () {
        this.inputStream && this.inputStream.mediaStream.getTracks().forEach(function (t) { t.stop(); });
        this.recording = false;
    };
    Recorder.prototype.clear = function () {
        this.worker.postMessage({ command: "clear" });
    };
    Recorder.prototype.getBuffer = function (cb) {
        if (!cb)
            throw new Error("Callback not set");
        this.callbacks.getBuffer.push(cb);
        this.worker.postMessage({ command: "getBuffer" });
    };
    Recorder.prototype.exportWAV = function (cb, mimeType) {
        mimeType = mimeType || this.config.mimeType;
        if (!cb)
            throw new Error("Callback not set");
        this.callbacks.exportWAV.push(cb);
        this.worker.postMessage({
            command: "exportWAV",
            type: mimeType
        });
    };
    return Recorder;
}());