window.chromeSpeechRecognititon = (function() {
    return function chromeSpeechRecognition(cfg) {
        var _this = this;

        this.endOfCommandTimeoutCallbacks = $.Callbacks();

        this.lastRecognized = "";

        if (cfg && cfg.endOfCommandTimeoutCallbacks)
            for (var i = 0; i < cfg.endOfCommandTimeoutCallbacks.length; i++)
                this.endOfCommandTimeoutCallbacks.add(cfg.endOfCommandTimeoutCallbacks[i]);

        var recognition;
        try {
            recognition = new webkitSpeechRecognition();
        } catch (e) {
            recognition = Object;
        }
        recognition.lang = "ru";
        recognition.continuous = true;
        recognition.interimResults = false;
        recognition.onresult = function (event) {
            var txtRec = "";
            for (var i = event.resultIndex; i < event.results.length; ++i) {
                txtRec += event.results[i][0].transcript;
            }
            console.log(txtRec);
            _this.lastRecognized = txtRec;
            _this.endOfCommandTimeoutCallbacks && _this.endOfCommandTimeoutCallbacks.fire(_this.lastRecognized);
        };

        recognition.onend = function () {
            if (_this.recognition.stopping)
                _this.recognition.stopping = false;
            else
               recognition.start();
        };

        this.recognition = recognition;

        this.emulateRecognized = function (message) {
            _this.lastRecognized = message;
            _this.endOfCommandTimeoutCallbacks && _this.endOfCommandTimeoutCallbacks.fire(_this.lastRecognized);
        };

        this.start = function() {
            _this.recognition.start();
        };

        this.stop = function () {
            _this.recognition.stopping = true;
            _this.recognition.stop();
        };

        return _this;
    };
})();