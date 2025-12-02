function changeBox(obj, subId, showValue) {
    if (obj.type == 'radio') {
        if (obj.value == showValue) {
            $('#' + subId).prop('disabled', false);
        } else {
            $('#' + subId).prop('disabled', true);
        }
    } else {
        if (obj.value == showValue) {
            if (obj.checked) {
                $('#' + subId).prop('disabled', false);
            } else {
                $('#' + subId).prop('disabled', true);
            }
        }
    }
}

function showTableBody(obj, id) {
    var flag = $('#tbody_' + id).is(':hidden');

    if (flag) {
        $('#tbody_' + id).show();
        obj.value = '隱藏';
    } else {
        $('#tbody_' + id).hide();
        obj.value = '顯示';
    }
}

function formSubmit(message, id) {
    if (message != "") {
        jAlert(message, function () { $("#" + id).focus(); });
    } else {
        jConfirm("確定要送出嗎？", function () {
            //if (getIEVersion() == 11) {
                $('#ActionForm').submit();
            //} else {
            //    window.open(getRootPath() + '', "Preview", "scrollbars=yes,resizable=yes,width=800,height=650");
            //    $('#ActionForm').submit(); 
            //}
        });
    }
}

function insertRow(id) {
    var jq_tr = $('#' + id + ' tr:eq(1)').clone();

    var checked = new Array();

    $(jq_tr).find("input:checked").each(function () {
        checked.push($(this).prop("id"));
    });
    $("#" + id).append("<tr>" + $(jq_tr).html() + "</tr>");

    $("#" + id).find('tr:last').each(function () {
        for (var i = 0; i < checked.length; i++) {
            $(this).find("#" + checked[0]).prop("checked", true);
        }
    });

    sortRow(id);
}

function deleteRow(obj, id) {
    $(obj).parents('tr').remove();
    sortRow(id);
}

function deleteAll(id) {
    $('#tbody_' + id + ' tr').each(function (i) {
        if (i > 1) {
            $(this).remove();
        }
    });
    $("#tbody_" + id + "_max").val("0");
}

function sortRow(id) {
    var len = $("#" + id).find("tr").length;

    for (var i = 2; i < len; i++) {
        $('#' + id + ' tr:eq(' + i + ')').each(function () {
            $(this).find("input").each(function () {
                var name = $(this).attr("name");
                var id = $(this).attr("id");
                if (typeof (name) !== "undefined") {
                    var new_name = name.substring(name, name.lastIndexOf("_") + 1) + (i - 1);
                    $(this).attr("id", id.replace(name, new_name));
                    $(this).attr("name", new_name);
                }
            });

            $(this).find("select").each(function () {
                var name = $(this).attr("name");
                var id = $(this).attr("id");
                if (typeof (name) !== "undefined") {
                    var new_name = name.substring(name, name.lastIndexOf("_") + 1) + (i - 1);
                    $(this).attr("id", id.replace(name, new_name));
                    $(this).attr("name", new_name);
                }
            });

            $(this).find("textarea").each(function () {
                var name = $(this).attr("name");
                var id = $(this).attr("id");
                if (typeof (name) !== "undefined") {
                    var new_name = name.substring(name, name.lastIndexOf("_") + 1) + (i - 1);
                    $(this).attr("id", id.replace(name, new_name));
                    $(this).attr("name", new_name);
                }
            });

            $(this).find('td:first').text(i - 1);
            //$(this).find('td:eq(1)').html("<input type=\"button\" value=\"刪除\" onclick=\"deleteRow(this, '" + id + "')\" />");
            $(this).find('td:eq(1)').html("<img src=\"../../Images/icon/cross-icone-5804-16.png\" alt=\"刪除\" title=\"刪除\" style=\"cursor:pointer;\" onclick=\"deleteRow(this, '" + id + "')\" />");
        });

        setDatepicker(id, i);
    }

    $("#" + id + "_max").val(len - 2);
    //alert("#" + id + "_max: " + $("#" + id + "_max").val());
}

function hiddenRow(obj, id, n) {
    var flag = $('#' + id + ' tr:eq(' + (n * 2 + 1) + ')').is(':hidden');
    if (flag) {
        obj.value = "隱藏";
        $('#' + id + ' tr:eq(' + (n * 2 + 1) + ')').show();
    } else {
        obj.value = "顯示";
        $('#' + id + ' tr:eq(' + (n * 2 + 1) + ')').hide();
    }
}

function formBaseCheck() {
    var message = "";
    var id = "";
    var obj = new Array();

    if ($("#BIRTHDAY").length > 0) {
        if ($("#BIRTHDAY").val() == "") {
            message += "請填寫生日<br/>";
            if (id == "") {
                id = "BIRTHDAY";
            }
        } else if (!isDate($("#BIRTHDAY").val())) {
            message += "生日格式錯誤<br/>";
            if (id == "") {
                id = "BIRTHDAY";
            }
        }
    }

    obj[0] = id;
    obj[1] = message;
    return obj;
}