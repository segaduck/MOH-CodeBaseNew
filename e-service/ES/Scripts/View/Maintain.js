function SetNext(val, rootUrl) {
    $.ajax({
        cache: false,
        type: "post",
        url: rootUrl + 'Pay/GetSC_IDoption',
        dataType: "json",
        data: {
            UNIT_CD: val
        },
        success: function (result) {
            $('#sel_SC_PID').empty()
            $('#sel_SC_PID').append("<option value=''>請選擇</option>");
            for (var i = 0; i < result.length; i++) {
                $('#sel_SC_PID').append("<option value='" + result[i].VALUE + "'>" + result[i].NAME + "</option>");
            }
        },
        error: function () {
            alert("目前伺服器無法連結！");
        }
    });
}

function UpdateTime(rootUrl) {
    var val = "";
    $("input[name='chb_app_id']").each(
        function () {
            if ($(this).prop("checked")) {
                val += $(this).val() + ",";
            }
        }
    );
    if (val == "") {
        alert("請勾選入帳資料!");
        return;
    }
    $.ajax({
        cache: false,
        type: "post",
        url: rootUrl + 'Pay/SetSETTLE_DATE',
        dataType: "json",
        data: {
            APP_IDS: val
        },
        success: function (result) {
            alert(result.VALUE);
            $("#ActionForm").submit();
        },
        error: function () {
            alert("目前伺服器無法連結！");
        }
    });
}

function SetNextAgain(val, rootUrl) {
    $.ajax({
        cache: false,
        type: "post",
        url: rootUrl + 'Pay/GetSRV_IDoption',
        dataType: "json",
        data: {
            SC_ID: val
        },
        success: function (result) {
            $('#sel_SRV_ID').empty()
            $('#sel_SRV_ID').append("<option value=''>請選擇</option>");
            for (var i = 0; i < result.length; i++) {
                $('#sel_SRV_ID').append("<option value='" + result[i].VALUE + "'>" + result[i].NAME + "</option>");
            }
        },
        error: function () {
            alert("目前伺服器無法連結！");
        }
    });
}

function CheckCardStatus(applyId) {
    $.ajax({
        cache: false,
        type: "post",
        //url: getRootPath() + '/Admin/Pay/CheckCardStatus',
        url: getRootPath() + '/BACKMIN/Pay/CheckCardStatus?applyId=' + applyId,
        dataType: "json",
        data: { applyId: applyId },
        success: function (result) {
            window.location = window.location;
        },
        error: function () {
            jAlert("目前伺服器無法連結！");
        }
    });
}

function doSearchEC(typeEC) {
    console.log(typeEC);
    $('#submit_type').val(typeEC);
    $('#ActionForm').submit();
}