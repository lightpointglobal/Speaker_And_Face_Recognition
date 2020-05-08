var commandRecognizer = commandRecognizer || {};

commandRecognizer.addCommandListener = function (grammars, ignorePunctuationMarks, callback) {
    commandRecognizer.commandListeners.push({ grammars, ignorePunctuationMarks, callback });
};
commandRecognizer.commandListeners = [];

commandRecognizer.removePunctuationMaks = function(str) {
    return str.replace("!", "").replace("?", "").replace(".", "").replace(",", "");
};
commandRecognizer.recognize = function (commandStr) {
    var tempStr = $.trim(commandStr.toLowerCase());
    for (var i = 0; i < commandRecognizer.commandListeners.length; i++) {
        var listener = commandRecognizer.commandListeners[i];

        var tempStr1 = listener.ignorePunctuationMarks ? commandRecognizer.removePunctuationMaks(tempStr) : tempStr;

        for (var j = 0; j < listener.grammars.length; j++) {
            if (tempStr1.startsWith(listener.grammars[j])) {
                listener.callback && listener.callback(tempStr1, listener.grammars[j]);
            }
        }

    }
};