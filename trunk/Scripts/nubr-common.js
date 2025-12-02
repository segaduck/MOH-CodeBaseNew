$(document).ready(function () {
    //自動插入前端檢核訊息欄位
    //$("input:text").each(function () {
    //    var name = $(this).attr("name") + "";

    //    if ($(this).attr("data-val") != "true") {
    //        return;
    //    }
    //    跳過日期
    //    if (isEmpty(name) || name.endsWith("_AD_TW")) {
    //        return;
    //    }

    //    var vlidateMsg = $("<span/>");
    //    vlidateMsg.attr("data-valmsg-for", name);
    //    vlidateMsg.attr("data-valmsg-replace", "true");
    //    vlidateMsg.insertAfter($(this));
    //});

    //訊息方塊設定
    if (typeof toastr != 'undefined') {
        //if (toastr !== undefined) {
        toastr.options = {
            "positionClass": "toast-top-center",
        }
    }

    //Highcharts 設定
    if (typeof Highcharts != 'undefined') {
        Highcharts.theme = {
            //移除 Highcharts 版權標示
            credits: {
                enabled: false
            },
            lang: {
                printChart: "列印圖表",
                downloadJPEG: "下載為 JPEG 檔案 (圖檔)",
                downloadPDF: "下載為 PDF 檔案",
                downloadPNG: "下載為 PNG 檔案 (圖檔)",
                downloadSVG: "下載為 SVG 檔案 (向量圖檔)",
                drillUpText: "返回 {series.name}"
            }
        };
        Highcharts.setOptions(Highcharts.theme);
    }

    //立體效果
    //Highcharts.getOptions().colors = Highcharts.map(Highcharts.getOptions().colors, function (color) {
    //    return {
    //        radialGradient: {
    //            cx: 0.5,
    //            cy: 0.3,
    //            r: 0.7
    //        },
    //        stops: [
    //            [0, color],
    //            [1, Highcharts.Color(color).brighten(-0.3).get('rgb')] // darken
    //        ]
    //    };
    //});
});

//====================
//startsWith
//====================
if (!String.prototype.startsWith) {
    String.prototype.startsWith = function (searchString, position) {
        position = position || 0;
        return this.substr(position, searchString.length) === searchString;
    };
}
//====================
//endsWith
//====================
if (!String.prototype.endsWith) {
    String.prototype.endsWith = function (searchString, position) {
        var subjectString = this.toString();
        if (typeof position !== 'number' || !isFinite(position) || Math.floor(position) !== position || position > subjectString.length) {
            position = subjectString.length;
        }
        position -= searchString.length;
        var lastIndex = subjectString.lastIndexOf(searchString, position);
        return lastIndex !== -1 && lastIndex === position;
    };
}

//====================
//replaceAll
//====================
if (!String.prototype.replaceAll) {
    String.prototype.replaceAll = function (search, replacement) {
        var target = this;
        return target.replace(new RegExp(search, 'g'), replacement);
    };
}

//====================
//左補0
//====================
$.strPad = function (i, l, s) {
    var o = i.toString();
    if (!s) {
        s = '0';
    }
    while (o.length < l) {
        o = s + o;
    }
    return o;
};

//====================
//右補
//====================
$.strRightPad = function (s, c, n) {
    if (!s || !c || s.length >= n) {
        return s;
    }
    var max = (n - s.length) / c.length;
    for (var i = 0; i < max; i++) {
        s += c;
    }
    return s;
}

//====================
//判斷為空
//====================
function isEmpty(str) {
    if (typeof (str) === 'undefined' || str === null || $.trim(str) === "") {
        return true;
    }
    return false;
}

//====================
//取得亂數
//====================
function getRndNum(maxNum, minNum) {
    return Math.floor(Math.random() * (maxNum - minNum + 1)) + minNum;
}

//====================
//chunkString
//====================
function chunkString(str, length) {
    return str.match(new RegExp('.{1,' + length + '}', 'g'));
}

//====================
//textWarp
//====================
function textWarp(txt, length) {
    if (length === undefined || length <= 0) {
        length = 10;
    }
    if (txt === undefined || txt.length <= length) {
        return txt;
    }

    return chunkString(txt, length).join("<br/>");
}

//====================
//比對 map 中是否有指定 key
//====================
function mapCntains(map, key) {
    for (var currkey in map) {
        if (currkey == key) {
            return true;
        }
    }
    return false;
}

//====================
//執行Ajax
//====================
//以ajax 傳送資料，並取回值 (後端需搭配AjaxResultStruct)
//url：目的url
//params：傳送資料 ,ex: {data1: "a" , data2: "b", , data3: "c"}
//completeMessage：完成後顯示的訊息
//_callbackFunc：完成後執行的方法
function executeAjax(url, params, completeMessage, _callbackFunc) {
    //鎖定畫面
    blockUI();
    debugTrace("executeAjax post data:");
    debugTrace(params);
    // post
    $.ajax({
        url: url,
        data: params,
        type: "POST",
        //dataType: 'json',
        success: function (result) {
            if (result.status) {
                if (_callbackFunc != undefined) {
                    if ($.isFunction(_callbackFunc)) {
                        _callbackFunc(result.data);
                    }
                }
                if (!isEmpty(completeMessage)) {
                    toastr.success(completeMessage);
                }
                unblockUI();
            } else {
                toastr.error("處理錯誤!<br/>" + result.message);
                unblockUI();
                return;
            }
        },
        error: function (xhr, ajaxOptions, thrownError) {
            toastr.error("網站錯誤, 請洽資訊人員!");
            console.log(xhr.status);
            console.log(thrownError);
            unblockUI();
        },
        complete: function () {
        }
    });
}

//以ajax 傳送資料，並得到回傳的 html (後端配合回傳 PartialView)
//url：目的url
//params：傳送資料 ,ex: {data1: "a" , data2: "b", , data3: "c"}
//completeMessage：完成後顯示的訊息
//_callbackFunc：完成後執行的方法
function ajaxPartialView(url, params, completeMessage, _callbackFunc) {
    //鎖定畫面
    blockUI();
    debugTrace("executeAjax post data:");
    debugTrace(params);
    // post
    $.ajax({
        url: url,
        data: params,
        type: "POST",
        //contentType: "application/html; charset=utf-8",
        //dataType: "html",
        success: function (result) {
            if (typeof result === 'object') {
                //回傳為AjaxResultStruct
                toastr.error("處理錯誤!<br/>" + result.message);
            } else if (_callbackFunc != undefined) {
                //回傳為 html
                if ($.isFunction(_callbackFunc)) {
                    _callbackFunc(result);
                }
                //成功訊息
                if (!isEmpty(completeMessage)) {
                    toastr.success(completeMessage);
                }
            }
            unblockUI();
        },
        error: function (xhr, ajaxOptions, thrownError) {
            toastr.error("網站錯誤, 請洽資訊人員!");
            console.log(xhr.status);
            console.log(thrownError);
            unblockUI();
        },
        complete: function () {
        }
    });
}

//====================
//popup
//====================
//顯示一個訊息視窗
function alertMsg(msg) {
    var html = "<div class='form-horizontal'>";
    html += "<div class='form-list'>"
    html += msg;
    html += "</div></div>"
    showPopWindowByHtml(html, 200, "訊息", 100);
}

var _popWindow = null;
function showPopWindowByHtml(html, width, title, height, _callbackFunc) {
    if (_popWindow != null) {
        _popWindow.dialog('destroy').remove();
    }

    _popWindow = $("#popupWindow");
    if (_popWindow.length == 0) {
        _popWindow = $("<div/>");
        _popWindow.attr("id", "popupWindow");
        _popWindow.appendTo($(document.body));
        _popWindow.css("overflow", "hidden");
    }

    _popWindow.html(html);

    var options = {
        autoOpen: true,
        modal: true,
        title: title || "",
        open: function (event, ui) {
            $('#popupWindow').dialog('option', 'position', 'center');
        },
        close: function () {
            _popWindow.dialog('destroy').remove();
            _popWindow = null;

            if (_callbackFunc != undefined) {
                if ($.isFunction(_callbackFunc)) {
                    _callbackFunc(result.data);
                }
            }
        }
    }
    if (!isEmpty(width)) {
        options.width = width;
    }
    if (isEmpty(height)) {
        options.height = height;
    }
    _popWindow.dialog(options);
}
//顯示 dialog
function showPopWindow(ID, width, title) {
    if (isEmpty(width)) {
        width = 600;
    }

    var dialog = $("#" + ID).dialog({
        autoOpen: true,
        modal: true,
        title: title || "",
        width: width,
        //height: height,
        maxWidth: $(window).width() - 40,
        maxHeight: $(window).height() - 20,
        //show: {
        //    effect: "clip",
        //    duration: 1000
        //},
        buttons: {
            "關閉": function () {
                $(this).dialog("close");
            }
        }
        , open: function (event, ui) {
            $("#" + ID).dialog('option', 'position', 'center');
        }
        //, close: function () {
        //}
    });

    return dialog;
}

//====================
//blockUI
//====================
//覆寫 global 以加速畫面顯示
function blockUI() {
    $.blockUI({
        css: {
            message: $("<div>處理中，請稍後</div>"),
            border: 'none',
            padding: '15px',
            backgroundColor: '#000',
            '-webkit-border-radius': '10px',
            '-moz-border-radius': '10px',
            opacity: .5,
            color: '#fff'
        }
    });
}
function unblockUI() {
    $.unblockUI();
}

//====================
//toastr
//====================
function toastrReturnMsgStlye() {
    toastr.options = {
        "positionClass": "toast-top-full-width",
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    }
}
function toastrNormalStlye() {
    toastr.options = {
        "positionClass": "toast-top-center",
    }
}

//====================
//資料快篩 (對整列資料做過濾)
//====================
//1.tbodyID :table的tr欄位外面包"tbody" ,並賦予ID
//2.inputElmID : 輸入框的ID
function gridRowFilterHandler(tbodyID, inputElmID) {
    //資料快篩
    $("#" + inputElmID).keyup(function (e) {
        var ENTER_KEY = 13;
        var UP_ARROW_KEY = 38;
        var DOWN_ARROW_KEY = 40;
        var ESCAPE_KEY = 27;
        if (e.which !== UP_ARROW_KEY && e.which !== DOWN_ARROW_KEY && e.which !== ESCAPE_KEY) {
            var filterWord = $.trim($("#" + inputElmID).val().toUpperCase());
            var isFilterWordEmpty = isEmpty(filterWord);
            $("tbody#" + tbodyID + " > tr").each(function () {
                var rowContent = ($(this).text() + "").toUpperCase();
                if (!isFilterWordEmpty && !isEmpty(rowContent) && rowContent.indexOf(filterWord) < 0) {
                    $(this).hide();
                } else {
                    $(this).show();
                }
            });
        }
    });
}
//====================
//RadioButton 處理
//====================
//以 RadioButton name 的後綴為關鍵字取得值
function radioButton_GetValueBySuffix(elmName) {
    return $(":radio[name$='" + elmName + "']:checked").val();
}

//對 radioButton 設定選擇值
function radioButton_SetSelect(name, SelectdValue) {
    $(':radio[name="' + name + '"][value="' + SelectdValue + '"]').prop('checked', true);
}

//====================
//畫面欄位取值
//====================
//以ID的前綴取值 (無法取得 radio 和 checkbox)
function getInputDataByIDPrefix(preID) {
    var dataRow = {};
    $("input[id^='" + preID + "'], textarea").each(function () {
        var thisID = $(this).attr("id");
        thisID = thisID.substr(preID.length);
        dataRow[thisID] = $(this).val();
    });
    $("select[id^='" + preID + "']").each(function () {
        var thisID = $(this).attr("id");
        thisID = thisID.substr(preID.length);
        dataRow[thisID] = $(this).val();
    });
    return dataRow;
}
//====================
//其他
//====================
function getInputButton(text, onclick) {
    var input = $("<input />");
    input.attr("type", "button");
    input.attr("class", "btn btn-main");
    input.attr("value", text);
    input.attr("onclick", onclick);
    return input;
}

function lockScreen() {
    $("input,select,:radio,textarea").prop("disabled", true);
    $(":button, .input-group-btn, .uploadify-button").hide();
}

function lockAreaScreen(areaID) {
    var area = $("#" + areaID)
    if (area.length == 0) {
        return;
    }
    area.find("input,select,:radio,textarea").prop("disabled", true);
    area.find(":button, .input-group-btn").hide();
}

function virtualFromSubmit(url, params) {
    var form = $("form[name=RedirAfterBlockResult]");
    form.html("");

    for (var propertyName in params) {
        var input = document.createElement("input");
        $(input).attr("type", "hidden");
        $(input).attr("name", propertyName);
        $(input).attr("value", params[propertyName]);
        form.append(input);
    }
    form.attr("action", url);
    form.submit();
    return false;
}