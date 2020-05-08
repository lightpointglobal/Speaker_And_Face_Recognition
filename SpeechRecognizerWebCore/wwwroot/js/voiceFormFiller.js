window.VoiceFormFiller = (function() {
    function voiceFormFiller(recorder, form) {
        var module = this;
        var processRequest = function(node) {
            if (!node) return;
            var currentRequest = node.data;
            module.config.speechUtterance.text = currentRequest.text;
            speechSynthesis.speak(module.config.speechUtterance);
            var phraseRecognized = function(phrase) {
                recorder.endOfCommandTimeoutCallbacks.remove(phraseRecognized);

                if (currentRequest.type === "date") {
                    $.ajax({
                        type: "POST",
                        url: "/Home/ParseDate",
                        data: { date: phrase },
                        success: function(result) {
                            if (result.success === 1) {
                                $(currentRequest.input).val(result.date);
                                setTimeout(function() {
                                        if (node.next)
                                            processRequest(node.next);
                                        else
                                            $(form).submit();
                                    },
                                    module.config.pauseTime);
                            } else {
                                module.config.speechUtterance.text = "Не удалось распознать дату";
                                speechSynthesis.speak(module.config.speechUtterance);

                                setTimeout(function() {
                                        processRequest(node);
                                    },
                                    module.config.pauseTime);
                                return;
                            }
                        }
                    });
                } else {
                    $(currentRequest.input).val(phrase);
                    setTimeout(function () {
                        if (node.next)
                            processRequest(node.next);
                        else
                            $(form).submit();
                    }, module.config.pauseTime);
                }
            };

            recorder.endOfCommandTimeoutCallbacks.add(phraseRecognized);
        };
        var speechSynthesisUtterance = new SpeechSynthesisUtterance();
        speechSynthesisUtterance.rate = 2;
        Object.assign(module, {
            form: form,
            fieldRequests: new LinkedList(),
            config: {
                pauseTime: 1000,
                speechUtterance: speechSynthesisUtterance
            },
            addFieldRequest: function (r) {
                module.fieldRequests.insertAtEnd(r);
            },
            start: function () {
                setTimeout(function () {
                    if (module.fieldRequests.head)
                        processRequest(module.fieldRequests.head);
                    else
                        $(form).submit();
                }, module.config.pauseTime);
            }
        });
    }

    return voiceFormFiller;
})();
