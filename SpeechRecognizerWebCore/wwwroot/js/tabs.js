$(function () {
    var activeClass = "active";
    $(".nav-tabs li").click(function () {
        var me = $(this);
        me.closest(".nav-tabs").find("li").each(function() {
            var li = $(this);
            li.removeClass(activeClass);
            $("#" + li.attr("for") + ".tab-pane").removeClass(activeClass);
            if (li === me) {
                li.addClass(activeClass);
                $("#" + li.attr("for") + ".tab-pane").removeClass(activeClass);
            }

        });


    });
});