/*
 * 判斷 CheckBox 或 Radio 是否有被選取
 */
function LibIsChecked(name) {
    // alert($("input[name='" + name + "']:checked").length + " / " + $("input[name='ACTION_RES']").length);
    if ($("input[name='" + name + "']:checked").length == 0) {
        return false;
    }
    else {
        return true;
    }
}

function jAlert(message, fun) {
    if ($("#dialog-alert").length == 0) {
        $("body").append("<div id=\"dialog-alert\"></div>");
    }

    var text = "<div class='ui-icon ui-icon-alert' style='float: left; margin: 0px 7px 20px 0px;'></div><div style='float: left; width: 90%;'>" + message + "</div>";

    $("#dialog-alert").prop("title", "訊息");
    $("#dialog-alert").html(text);

    $("#dialog-alert").dialog({
        modal: true,
        buttons: {
            確定: function () {
                $(this).dialog("close");
                if (typeof (fun) != "undefined") {
                    fun();
                }
            }
        }
    });
}

function jShowDialog(title, message) {
    if ($("#dialog-show").length == 0) {
        $("body").append("<div id=\"dialog-show\"></div>");
    }

    $("#dialog-show").prop("title", title);
    $("#dialog-show").html(message);

    $("#dialog-show").dialog({
        modal: true
    });
}

function jConfirm(message, fun, fun2) {
    if ($("#dialog-confirm").length == 0) {
        $("body").append("<div id=\"dialog-confirm\"></div>");
    }

    var text = "<div class='ui-icon ui-icon-alert' style='float: left; margin: 0px 7px 20px 0px;'></div><div style='float: left; width: 90%;'>" + message + "</div>";

    $("#dialog-confirm").prop("title", "確認");
    $("#dialog-confirm").html(text);

    $("#dialog-confirm").dialog({
        modal: true,
        buttons: {
            確定: function () {
                $(this).dialog("close");
                if (typeof (fun) != "undefined") {
                    fun();
                }
            },
            取消: function () {
                $(this).dialog("close");
                if (typeof (fun2) != "undefined") {
                    fun2();
                }
            }
        }
    });
}

// 是否為日期格式
function isDate(value) {
    var re = /^\d{4}\/\d{2}\/\d{2}$/

    if (!re.test(value)) {
        return false;
    }

    var tmpy = "";
    var tmpm = "";
    var tmpd = "";

    tmpy = tmpy + value.substring(0, 4);
    tmpm = tmpm + value.substring(5, 7);
    tmpd = tmpd + value.substring(8, 10);

    year = new String(tmpy);
    month = new String(tmpm);
    day = new String(tmpd)

    if ((tmpy.length != 4) || (tmpm.length > 3) || (tmpd.length > 3)) return false;
    if (!((1 <= month) && (12 >= month) && (31 >= day) && (1 <= day))) return false;
    if (!((year % 4) == 0) && (month == 2) && (day == 29)) return false;
    if ((month <= 7) && ((month % 2) == 0) && (day >= 31)) return false;
    if ((month >= 8) && ((month % 2) == 1) && (day >= 31)) return false;
    if ((month == 2) && (day == 30)) return false;

    return true;
}

// 是否為數字
function isNumber(value) {
    var re = /^[\d]*$/
    return re.test(value);
}

// 是否為數字，可帶小數
function isFloat(value) {
    var re = /^\d+(\.\d+)?$/
    return re.test(value);
}

function ajaxLoader(el, options) {
    // Becomes this.options
    var defaults = {
        bgColor: '#fff',
        duration: 800,
        opacity: 0.7,
        classOveride: false
    }
    this.options = jQuery.extend(defaults, options);
    this.container = $(el);
    this.init = function () {
        var container = this.container;
        // Delete any other loaders
        this.remove();
        // Create the overlay
        var overlay = $('<div></div>').css({
            'background-color': this.options.bgColor,
            'opacity': this.options.opacity,
            'width': container.width(),
            'height': container.height(),
            'position': 'absolute',
            'top': '0px',
            'left': '0px',
            'z-index': 99999
        }).addClass('ajax_overlay');
        // add an overiding class name to set new loader style
        if (this.options.classOveride) {
            overlay.addClass(this.options.classOveride);
        }
        // insert overlay and loader into DOM
        container.append(
            overlay.append($('<div></div>').addClass('ajax_loader')).fadeIn(this.options.duration)
        );
    };
    this.remove = function () {
        var overlay = this.container.children(".ajax_overlay");
        if (overlay.length) {
            overlay.fadeOut(this.options.classOveride, function () {
                overlay.remove();
            });
        }
    }
    this.init();
}

function showLoad() {
    var options = {
        bgColor: '#fff',
        duration: 800,
        opacity: 0.7,
        classOveride: false
    }
    var block;
    //$("body").live('click', function () {
        block = new ajaxLoader(this, options);
    //});

}

function getRootPath() {
    var path = location.pathname.match("/e-service");
    return (path == null) ? "" : path;
}

var updateFileExt = new Array();
updateFileExt[42] = "jpg/jpeg/bmp/png/gif/tif";
updateFileExt[43] = "doc/docx/gif/jpg/jpeg/tif";
updateFileExt[44] = "doc/docx/pdf/gif/jpg/jpeg/tif";

function chkUpdateFileExt(type, id) {
    var ext = $("#" + id).val().split("\\").pop().split(".").pop().toLowerCase();

    try {
        return (updateFileExt[type].indexOf(ext) != -1)
    } catch (ex) { }

    return false;
}

function countLength(stringToCount)  
{ 
    //計算有幾個全型字、中文字...  
    var c = stringToCount.match(/[^ -~]/g);  
    return stringToCount.length + (c ? c.length : 0);
}

function getIEVersion() {
    var rv = -1;
    if (navigator.appName == 'Microsoft Internet Explorer') {
        var ua = navigator.userAgent;
        var re = new RegExp("MSIE ([0-9]{1,}[\.0-9]{0,})");
        if (re.exec(ua) != null)
            rv = parseFloat(RegExp.$1);
    }
    else if (navigator.appName == 'Netscape') {
        var ua = navigator.userAgent;
        var re = new RegExp("Trident/.*rv:([0-9]{1,}[\.0-9]{0,})");  //for IE 11
        if (re.exec(ua) != null)
            rv = parseFloat(RegExp.$1);
    }
    return rv;
}

$(function () {
    $.datepicker.setDefaults({ dateFormat: 'yy/mm/dd', changeYear: true, changeMonth: true, showButtonPanel: true, yearRange: '1900:+10', showOn: 'both', buttonImageOnly: true, buttonImage: getRootPath() + '/Images/icon/calendar.png' });
});