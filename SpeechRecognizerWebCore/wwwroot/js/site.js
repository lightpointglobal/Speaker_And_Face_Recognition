function addParamToUrl(url, key, value) {
    value = encodeURI(value);
    url += url.indexOf("?") > -1 ? "&" : "?";
    url += key + "=" + value;
    return url;
}

function stringToDate(str) {
    var ret;
    $.ajax({
        type: "POST",
        url: "/Home/ParseDate",
        data: { date: str },
        success: function (result) {
            if (result.success === 1)
                ret = result.date;
        },
        async: false
    });
    return ret;
}

$(function () {
    /*$("input[type=date]").flatpickr({
        altInput: true,
        altFormat: "d.m.Y",
        dateFormat: "Y-m-d",
        "locale": "ru"
    });*/
});