window.videoRecorder = (function() {
    return function videoRecorder(cfg) {
        var _this = this;

        Object.assign(this, cfg);

        this.recording = false;
        this.record = function () {
            if (_this.recording) return;

            if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
                navigator.mediaDevices.getUserMedia({ video: true }).then(function (stream) {
                    _this.inputStream = stream;
                    _this.outputElement.srcObject = stream;
                    _this.outputElement.play();
                    _this.recording = true;
                });
            }
        };

        this.stop = function () {
            if (!_this.recording) return;

            _this.inputStream = null;
            _this.outputElement.pause();
            _this.outputElement.srcObject = null;
            _this.recording = false;
        };

        this.takeSnapshot = function() {
            _this.snapshotCanvas.getContext("2d").drawImage(_this.outputElement, 0, 0, 333, 250);
            return new Promise(resolve => {
                _this.snapshotCanvas.toBlob(resolve, "image/png");
            });
        };

        return _this;
    };
})();