var cabHCA = "HCAAPIATL.cab#Version=1,0,19,711";
var cabHCAHPC = "HCACSAPIATL.cab#version=1,1,19,506";
var classidHCA = "CLSID:97E2BA41-8E68-4FCA-AEC6-DAD248400C20";
var classidHCAHPC = "CLSID:DB5437DD-75D1-478B-9F93-F9188D17B96B";
var isInsActiveX = true;

var GET_UTF8_DN = 1;
var HCAPKCS11_MODULE = "HCAPKCS11.dll"
var CKF_RW_SESSION = 0x00000002;
var CKF_SERIAL_SESSION = 0x00000004;

var CKM_MD5 = 0x00000210;
var CKM_SHA_1 = 0x00000220;
var CKM_SHA224 = 0x00000255;
var CKM_SHA256 = 0x00000250;
var CKM_SHA384 = 0x00000260;
var CKM_SHA512 = 0x00000270;

var CKM_RSA_PKCS = 0x00000001;
var CKM_SHA1_RSA_PKCS = 0x00000006;

var CKM_SHA256_RSA_PKCS = 0x00000040
var CKM_SHA384_RSA_PKCS = 0x00000041
var CKM_SHA512_RSA_PKCS = 0x00000042
var CKM_SHA224_RSA_PKCS = 0x00000046

var CKM_DES3_ECB = 0x00000132;
var CKM_DES3_CBC = 0x00000133;
var DES3_KEY_LENGTH = 24;

var GET_POLICY_ID = 0;
var GET_MSG_IMPRINT = 1;
var GET_SERIAL = 2;
var GET_TIME = 3;
var GET_VERSION = 4;
var GET_ACCURACY = 5;
var GET_NONCE = 6;

var CARD_TYPE_HOP = 0;
var CARD_TYPE_HPC = 1;

var m_hModule = 0,
    m_hSession = 0,
    m_hSOSession = 0,
    m_error = 0,
    m_cardVer = -1,
    m_cardType = -1,
    m_IsPortOpened = false,
    b64msg = "Q2hhbmdpbmcgSW5mb3JtYXRpb24gVGVjaG5vbG9neSBJbmMu",
    b64sig,
    cs_error,
    CAcert,
    Sigcert,
    GRCAcert,
    cs64cipher;
var reader_type;
var env_message;
var hca;
var hcacs;

function RunActiveX(type) {
    var eleObj = document.getElementById("xCab");
    while (eleObj.firstChild) {
        eleObj.removeChild(eleObj.firstChild);
    }
    switch (type) {
        case "pcsc":
            {
                eleObj.innerHTML = SetObjectTag(cabHCA, classidHCA, "hca");
                hca = document.getElementById("hca");
                reader_type = "axpcsc";
            }
            break;
        case "cs":
            {
                eleObj.innerHTML = SetObjectTag(cabHCAHPC, classidHCAHPC, "hcacs");
                hcacs = document.getElementById("hcacs");
                reader_type = "axcs";
            }
            break;
    }
    return CheckLoadActiveXState(type, hca, hcacs);
}

function RunHCAServiSign(type) {
    switch (type) {
        case "pcsc":
            {
                reader_type = "svpcsc";
                hca = getHCAAPISVIAdapterObj();
            }
            break;
        case "cs":
            {
                reader_type = "svcs";
                hcacs = getHCACSAPISVIAdapterObj();
            }
            break;
    }
    return CheckLoadServiSignState(type, hca, hcacs);
}
//type:pcsc(一般讀卡機)/cs(健保讀卡機)
function CheckEnvAndLoad(type) {
    var result = false;
    var browserName = getBrowser();
    var osName = getOS();
    var browserVersion = getVersion();
    if (osName == "Windows") {
        if (browserName == "Explorer") {
            if (parseInt(browserVersion.replace(/\..*$/, '')) > 9) {
                if (!RunHCAServiSign(type)) {
                    //blockAlert("元件載入失敗，請確認已安裝並執行HCAServiSign元件。","提示訊息", function () {  location.href = '/Home/Index'; } );
                    //Set_envmsg("元件載入失敗，請確認已安裝並執行HCAServiSign元件。");
                }
                else
                    isInsActiveX = false;
            }
            else
                RunActiveX(type);
        }
        else if (browserName == "Chrome" || browserName == "Firefox" || browserName == "Edge") {
            if (!RunHCAServiSign(type)) {
                //blockAlert("元件載入失敗，請確認已安裝並執行HCAServiSign元件。", "提示訊息",function () {  location.href = '/Home/Index'; } );
                //Set_envmsg("元件載入失敗，請確認已安裝並執行HCAServiSign元件。");
            }
            else
                isInsActiveX = false;
        }
        else
            blockAlert("不支援的瀏覽器" + browserName + "，請使用下列瀏覽器登入：\nGoogle Chrome、\nFirefox、\nMicrosoft Edge、\nInternet Explorer", "提示訊息",function () {  location.href = '/Home/Index'; } );
            // Set_envmsg("不支援的瀏覽器" + browserName + "，請使用下列瀏覽器登入：\nGoogle Chrome、\nFirefox、\nMicrosoft Edge、\nInternet Explorer");
    } else {
        blockAlert("請使用Windows作業系統，並執行HCAServiSign元件","提示訊息", function () {  location.href = '/Home/Index'; } );
        //Set_envmsg("請使用Windows作業系統，並執行HCAServiSign元件");
    }
    return result;
}

var BrowserDetect = {
    init: function () {
        this.browser = this.searchString(this.dataBrowser) || "An unknown browser";
        this.version = this.searchVersion(navigator.userAgent) ||
            this.searchVersion(navigator.appVersion) ||
            "an unknown version";
        this.OS = this.searchString(this.dataOS) || "an unknown OS";
    },
    searchString: function (data) {
        for (var i = 0; i < data.length; i++) {
            var dataString = data[i].string;
            var dataProp = data[i].prop;
            this.versionSearchString = data[i].versionSearch || data[i].identity;
            if (dataString) {
                if (dataString.indexOf(data[i].subString) != -1)
                    return data[i].identity;
            } else if (dataProp)
                return data[i].identity;
        }
    },
    searchVersion: function (dataString) {
        var index = dataString.indexOf(this.versionSearchString);
        if (index == -1) return;
        var tmp = dataString.substring(index + this.versionSearchString.length + 1);
        index = tmp.indexOf(" ");
        if (index == -1) index = tmp.length;
        return tmp.substring(0, index);
    },
    dataBrowser: [{
        string: navigator.userAgent,
        subString: "Chrome",
        identity: "Chrome"
    },
    {
        string: navigator.userAgent,
        subString: "Firefox",
        identity: "Firefox"
    },
    {
        string: navigator.userAgent,
        subString: "MSIE",
        identity: "Explorer",
        versionSearch: "MSIE"
    },
    {
        string: navigator.userAgent,
        subString: "Trident",
        identity: "Explorer",
        versionSearch: "rv"
    }
    ],
    dataOS: [{
        string: navigator.platform,
        subString: "Win",
        identity: "Windows"
    }]

};

BrowserDetect.init();

function getBrowser() {
    return BrowserDetect.browser;
}

function getVersion() {
    return BrowserDetect.version;
}

function getOS() {
    return BrowserDetect.OS;
}

function SetObjectTag(CodeBase, ClassID, idName) {
    var ObjectString = "" +
        "<object CLASSID='" + ClassID + "' " +
        "CODEBASE='" + CodeBase + "' " +
        "ID='" + idName + "' onerror='installActiveXError()'" +
        ">" +
        "</object>";
    return ObjectString;
}

function getCookie(cname) {
    var name = cname + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

function CheckLoadServiSignState(type, hcaState, hcacsState) {
    hideMask();
    if (type == "pcsc" && hcaState != undefined) {
        //setEnvTag('<span class="badge badge-success">HCA元件安裝成功</span>');
        //return true;
    }
    else if (type == "cs" && hcacsState != undefined) {
        //setEnvTag('<span class="badge badge-success">HCACS元件安裝成功</span>');
        //return true;
    }
    else if (hcaState == undefined && hcacsState == undefined) {
        blockAlert('尚未安裝元件或未啟動ServiSign元件</span><span>＞請啟動HCAServiSign服務或<a href=\"https://hca.nat.gov.tw/download/HCAServiSignAdapterSetup_1.0.20.0310.exe\">按此下載</a>HCA跨瀏覽器元件',"提示訊息", function () { location.href = '/Home/Index'; });
        //setEnvTag('<span class="badge badge-danger">尚未安裝元件或未啟動ServiSign元件</span><span>＞請啟動HCAServiSign服務或<a href=\"../Download/HCAServiSignAdapterSetup_1.0.19.0725.exe\">按此下載</a>HCA跨瀏覽器元件</span>');
        //return false;
    }
}

function CheckLoadActiveXState(type, hcaState, hcacsState) {
    hideMask();
    //window.onload = function () {
    if (type == "pcsc" && hcaState != undefined) {
        try {
            hcaState.ATL_GetErrorCode();
        }
        catch (e) {
            blockAlert('尚未安裝ActiveX元件</span><span>＞請加入信任網站、取消勾選「ActiveX篩選」，並執行元件（<font color=red><b>執行完畢後請重新整理此頁</b></font>）',"提示訊息", function () {  location.href = '/Home/Index'; } );
            //setEnvTag('<span class="badge badge-danger">尚未安裝ActiveX元件</span><span>＞請加入信任網站、取消勾選「ActiveX篩選」，並執行元件（<font color=red><b>執行完畢後請重新整理此頁</b></font>）</span>');
            //return false;
        }
        //setEnvTag('<span class="badge badge-success">HCA ActiveX元件安裝成功</span>');
        //return true;
    }
    else if (type == "cs" && hcacsState != undefined) {
        blockAlert('僅能使用一般讀卡機 !',"提示訊息", function () {  location.href = '/Home/Index'; } );
        //try {
        //    hcacs.GetErrorCode();
        //}
        //catch (e) {
        //    setEnvTag('<span class="badge badge-danger">尚未安裝ActiveX元件</span><span>＞請加入信任網站、取消勾選「ActiveX篩選」，並執行元件（<font color=red><b>執行完畢後請重新整理此頁</b></font>）</span>');
        //    return false;
        //}
        //setEnvTag('<span class="badge badge-success">HCACS ActiveX元件安裝成功</span>');
        return false;
    }
    else {
        blockAlert('尚未安裝元件ActiveX元件</span><span>＞請加入信任網站、取消勾選「ActiveX篩選」，並執行元件（<font color=red><b>執行完畢後請重新整理此頁</b></font>）',"提示訊息", function () {  location.href = '/Home/Index'; } );
        //setEnvTag('<span class="badge badge-danger">尚未安裝元件ActiveX元件</span><span>＞請加入信任網站、取消勾選「ActiveX篩選」，並執行元件（<font color=red><b>執行完畢後請重新整理此頁</b></font>）</span>');
        return false;
    }
    //};
}

function installActiveXError() {
    isInsActiveX = false;
    blockAlert('尚未安裝ActiveX元件</span><span>＞請加入信任網站、將ActiveX開啟，並執行元件（執行完畢後請重整此頁）', "提示訊息",function () {  location.href = '/Home/Index'; } );
    //setEnvTag('<span class="badge badge-danger">尚未安裝ActiveX元件</span><span>＞請加入信任網站、將ActiveX開啟，並執行元件（執行完畢後請重整此頁）</span>');
}

function GetSN() {
    var result;
    switch (reader_type) {
        case "axpcsc":
        case "svpcsc": {
            result = hca.ATL_GetHCACardSN(m_hModule);
            m_error = hca.ATL_GetErrorCode();
            if (parseInt(m_error) != 0) {
                blockAlert("操作失敗:[" + m_error + "] 請確認您的環境已設定成功且卡片以正確置入", "提示訊息",function () {  location.href = '/Home/Index'; } );
                //return "操作失敗:[" + m_error + "] 請確認您的環境已設定成功且卡片以正確置入";
            }
        }
            break;
        case "axcs":
        case "svcs": {
            result = hcacs.HCA_GetCardSN();
            var errCode = hcacs.GetErrorCode();
            if (parseInt(errCode) != 0) {
                blockAlert("取卡號失敗。錯誤代碼：" + errCode, function () {  location.href = '/Home/Index'; });
                //return "取卡號失敗。錯誤代碼：" + errCode;
            }
        }
            break;
        default:
            blockAlert("載入讀卡機失敗，請確認讀卡機及卡片是否正確安插？讀卡機驅動程式是否安裝正確？","提示訊息", function () {  location.href = '/Home/Index'; });
            //return "載入讀卡機失敗，請確認讀卡機及卡片是否正確安插？讀卡機驅動程式是否安裝正確？";
            break;
    }
    return result;
}

function checkModule() {
    if (m_hModule == 0) {
        return -1;
    }
    return 1;
}

function checkSession() {
    if (m_hSession == 0) {
        alert("請先開啟SESSION");
        return -1;
    }
    return 1;
}

function checkSOSession() {
    if (m_hSOSession == 0) {
        alert("請先開啟SO SESSION");
        return -1;
    }
    return 1;
}

function checkModuleClose() {
    if (m_hModule != 0) {
        alert("請先關閉密碼模組");
        return -1;
    }
    return 1;
}

function checkSessionClose() {
    if (m_hSession != 0) {
        alert("請先關閉SESSION");
        return -1;
    }
    return 1;
}

function checkSOSessionClose() {
    if (m_hSOSession != 0) {
        alert("請先關閉SO SESSION");
        return -1;
    }
    return 1;
}

function checkCardVer() {
    if (m_cardVer == -1) {
        alert("請先取得卡片版本");
        return -1;
    }
    return 1;
}

function checkCardType() {
    if (m_cardType == -1) {
        alert("請先取得卡片種類");
        return -1;
    }
    return 1;
}

function InitModule() {
    m_hModule = hca.ATL_InitModule(HCAPKCS11_MODULE, "");

    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        return "初始化失敗:" + m_error;
    } else {
        return "初始化成功";
    }
}

function CloseModule() {

    if (m_hModule == 0) {
        return "成功";
    }
    hca.ATL_CloseModule(m_hModule);
    return "成功";
}
function CloseModuleEx() {

    if (m_hModule == 0) {
        return "成功";
    }
    hca.ATL_CloseModuleEx(m_hModule, 0);
    return "成功";
}

function InitSession(pinValue) {

    if (checkModule() < 0) {
        return;
    }

    m_hSession = hca.ATL_InitSession(m_hModule, CKF_RW_SESSION | CKF_SERIAL_SESSION, pinValue);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        switch (m_error) {
            case 9039:
                alert("開啟失敗：" + m_error + "，PIN碼驗證錯誤，請確認您輸入的PIN碼是否正確。");
                break;
            case 9043:
                alert("開啟失敗：" + m_error + "，PIN碼已鎖定，請至HCA2.0網站進行解鎖後再進行操作。");
                break;
            default:
                alert("開啟失敗：" + m_error + "，PIN碼驗證失敗，請確認您輸入的PIN碼是否正確。");
                break;
        }
    } else {
        return "開啟成功";
    }
}

function CloseSession() {

    if (m_hSession == 0) {
        return;
    }

    hca.ATL_CloseSession(m_hModule, m_hSession);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        return "關閉失敗:" + m_error;
    } else {
        return "關閉成功";
    }
    m_hSession = 0;
}

function InitSOSession(pukCode) {

    if (checkModule() < 0) {
        return false;
    }

    if (checkSessionClose() < 0) {
        return false;
    }

    if (pukCode == undefined || pukCode == "") {
        alert("請輸入PUK碼")
        return false;
    }

    m_hSOSession = hca.ATL_InitSOSession(m_hModule, CKF_RW_SESSION | CKF_SERIAL_SESSION, pukCode);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("開啟失敗:" + m_error);
        return false;
    } else {
        return true;
    }
}

function CloseSOSession() {

    if (m_hSOSession == 0) {
        return;
    }

    hca.ATL_CloseSession(m_hModule, m_hSOSession);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        return "關閉失敗:" + m_error;
    } else {
        return "關閉成功";
    }
    m_hSOSession = 0;
}

function GetCardVersion() {

    var ver;
    if (checkModule() < 0) {
        return;
    }

    ver = hca.ATL_GetCardHCACardVersion(m_hModule);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        return ("操作失敗:" + m_error);
    } else {
        m_cardVer = ver;
        return ("卡片版本為:" + ver);
    }
}

function GetCardType() {

    var type;
    if (checkModule() < 0) {
        return;
    }

    type = hca.ATL_GetHCACardType(m_hModule);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        return "操作失敗:" + m_error;
    }
    else {
        m_cardType = type;
        if (type == CARD_TYPE_HOP) {
            return "醫事機構卡";
        }
        else {
            return "醫事人員卡";
        }

    }
}

function GetBasic() {

    var rtn;
    if (checkModule() < 0) {
        return "請先初始化密碼模組";
    }
    if (checkCardVer() < 0) {
        return "請先取得卡片版本";
    }
    if (checkCardType() < 0) {
        return "請先取得卡片種類";
    }

    rtn = hca.ATL_GetHCABasicData(m_hModule);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        return "操作失敗:" + m_error;
    }
    else {
        var datas = rtn.toArray();
        var msg = "";
        if (m_cardType == CARD_TYPE_HPC) {
            msg += "醫事人員中文姓名:" + datas[0] + "<br>";
            msg += "醫事人員英文姓名:" + datas[1] + "<br>";
            msg += "醫事人員性別:" + datas[2] + "<br>";
            msg += "醫事人員出生日期:" + datas[3] + "<br>";
            msg += "醫事人員身份證號:" + datas[4] + "<br>";
            msg += "醫事人員類別:" + datas[5] + "<br>";
        }
        else {
            msg += "醫事機構代碼:" + datas[0] + "<br>";
            msg += "醫事機構名稱:" + datas[1] + "<br>";
            msg += "卡次:" + datas[2] + "<br>";
            msg += "醫事機構種類:" + datas[3] + "<br>";
            msg += "醫事機構類型:" + datas[4] + "<br>";
            msg += "醫事機構地址:" + datas[5] + "<br>";
            msg += "醫事機構聯絡電話:" + datas[6] + "<br>";
            msg += "醫事機構擁有者:" + datas[7] + "<br>";
        }
        return msg;
    }
}

function checkHCAAPI() {
    var msg = "";
    if (reader_type == undefined) {
        alert("請選擇讀卡機類別!");
        return;
    }
    var pincodeObj = document.getElementById("pincode");
    if (pincodeObj.value == "") {
        alert("請輸入PIN碼!");
        pincodeObj.focus();
        return;
    }

    if (isInsActiveX) {
        msg += "<li>安全模組環境 - ActiveX</li>";
    } else {
        msg += "<li>安全模組環境 - HCA跨瀏覽器元件</li>";
    }

    switch (reader_type) {
        case "axpcsc":
        case "svpcsc": {
            msg += "<li>初始化 - " + InitModule() + "</li>";
            msg += "<li>開啟SESSION - " + InitSession(pincodeObj.value) + "</li>";
            msg += "<li>卡片版本 - " + GetCardVersion() + "</li>";
            msg += "<li>卡片類別 - " + GetCardType() + "</li>";
            msg += "<li>卡片基本資料 - " + GetBasic() + "</li>";
            var cardSN = GetSN();
            var cardInfo = GetInfo();
            msg += "<li>卡片序號 - " + cardSN + "</li>";
            msg += "<li>卡片資訊 - " + cardInfo + "</li>";
            msg += "<li>簽章 - " + Sign() + "</li>";
            msg += "<li>驗章 - " + Verify() + "</li>";
            msg += "<li>公鑰加密 - " + Enc() + "</li>";
            msg += "<li>私鑰解密 - " + Dec() + "</li>";
            if (m_hSession > 0) msg += "<li>關閉SESSION - " + CloseSession() + "</li>";
            if (m_hModule > 0) msg += "<li>關閉密碼模組 - " + CloseModule() + "</li>";
        }
            break;
        case "axcs":
        case "svcs": {
            msg += "<li>開啟讀卡機通訊埠" + csOpenComPort() + "</li>";
            msg += "<li>卡片基本資料 - " + csGetBasicData() + "</li>";
            msg += "<li>卡片資訊 - " + csGetCardInfo() + "</li>";
            msg += "<li>卡片類別 - " + csGetCardType() + "</li>";
            msg += "<li>卡片序號 - " + csGetSerialNumber() + "</li>";
            msg += "<li>驗PIN碼 - " + csVerifyPIN(pincodeObj.value) + "</li>";
            msg += "<li>取憑證 - " + csGetCerts() + "</li>";
            msg += "<li>簽章 - " + csSignData() + "</li>";
            msg += "<li>驗章 - " + csVerifySign() + "</li>";
            msg += "<li>公鑰加密 - " + csPublicKeyEncrypt() + "</li>";
            msg += "<li>私鑰解密 - " + csPrivateKeyDecrypt() + "</li>";
            if (m_IsPortOpened) msg += "<li>關閉讀卡機通訊埠 - " + csCloseComPort() + "</li>";
        }
            break;
    }

    m_error = 0;
    cs_error = 0;
    if (msg != "") {
        msg = "<fieldset style='padding:5px'>"
            + "<legend>&nbsp;檢 測 結 果&nbsp;</legend>"
            + "<ul>" + msg + "</ul>"
            + "</fieldset>";
    }
    document.getElementById("HCAInfo").innerHTML = msg;
}

function GetInfo() {
    if (checkModule() < 0) {
        return "";
    }

    var msg = "";
    var rtn = hca.ATL_GetHCACardInfo(m_hModule);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        msg += "失敗: " + m_error;

    } else {
        var infos = rtn.toArray();

        if (parseInt(infos[0]) == 0) {
            msg += "卡片不支援產生RSA金鑰" + "<br>";
        } else {
            msg += "卡片支援產生RSA金鑰" + "<br>";
        }

        msg += "卡片發行日期: " + infos[1] + "<br>";
        msg += "卡片有效日期啟始日: " + infos[2].substr(0, 7) + "<br>";
        msg += "卡片有效日期結束日: " + infos[2].substr(7, 7) + "<br>";

        if (parseInt(infos[3]) == 1) {
            msg += "卡片狀態: 已開卡" + "<br>";
        } else {
            msg += "卡片狀態: 未開卡" + "<br>";
        }

        if (parseInt(infos[4]) == 0) {
            msg += "Card on use" + "<br>";
        } else {
            msg += "Card termination" + "<br>";
        }
    }
    return msg;
}

//簽章
function Sign() {
    if (checkModule() < 0) {
        return "請先初始化密碼模組";
    }
    if (checkSession() < 0) {
        return "請先開啟SESSION";
    }

    var hPriKey = hca.ATL_GetKeyObjectHandle(m_hModule, m_hSession, 0, "", "1");
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        return "取私鑰失敗: " + m_error;
    }

    var msg = "";
    b64sig = hca.ATL_MakeSignature(m_hModule, m_hSession, CKM_SHA1_RSA_PKCS, b64msg, hPriKey);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        msg += "失敗: " + m_error + "<br>";
    } else {
        msg += "成功" + "<br>";
    }

    if (hPriKey != 0) {
        hca.ATL_DeleteKeyObject(m_hModule, m_hSession, hPriKey);
    }
    //msg += b64sig + "<br>"; //DEBUG
    return msg;
}
//驗章
function Verify() {
    if (checkModule() < 0) {
        return "請先初始化密碼模組";
    }
    if (checkSession() < 0) {
        return "請先開啟SESSION";
    }

    var hPubKey = hca.ATL_GetKeyObjectHandle(m_hModule, m_hSession, 1, "", "1");
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        return "取公鑰失敗: " + m_error;
    }

    var msg = "";
    m_error = hca.ATL_VerifySignature(m_hModule, m_hSession, CKM_SHA1_RSA_PKCS, b64msg, hPubKey, b64sig);
    if (m_error == 0) {
        msg += "成功";
    } else {
        msg += "失敗: " + m_error;
    }

    if (hPubKey != 0) {
        hca.ATL_DeleteKeyObject(m_hModule, m_hSession, hPubKey);
    }
    return msg;
}
//公鑰加密
function Enc() {
    if (checkModule() < 0) {
        return "請先初始化密碼模組";
    }
    if (checkSession() < 0) {
        return "請先開啟SESSION";
    }

    var iCardType = hca.ATL_GetCardType(m_hModule);
    var dwCardVersion = hca.ATL_GetCardHCACardVersion(m_hModule);
    if (iCardType == 1 && dwCardVersion == 1) {
        return "1.0醫事人員卡無加密憑證";
    }

    var hPubKey = hca.ATL_GetKeyObjectHandle(m_hModule, m_hSession, 1, "", "2");
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        return "取公鑰失敗: " + m_error;
    }

    var msg = "";
    b64cipher = hca.ATL_PublicKeyEncryption(m_hModule, m_hSession, CKM_RSA_PKCS, b64msg, hPubKey);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        msg += "失敗: " + m_error + "<br>";
    } else {
        msg += "成功" + "<br>";
    }

    if (hPubKey != 0) {
        hca.ATL_DeleteKeyObject(m_hModule, m_hSession, hPubKey);
    }
    //msg += b64cipher + "<br>"; //DEBUG
    return msg;
}
//私鑰解密
function Dec() {
    if (checkModule() < 0) {
        return "請先初始化密碼模組";
    }
    if (checkSession() < 0) {
        return "請先開啟SESSION";
    }

    var iCardType = hca.ATL_GetCardType(m_hModule);
    var dwCardVersion = hca.ATL_GetCardHCACardVersion(m_hModule);
    if (iCardType == 1 && dwCardVersion == 1) {
        return "1.0醫事人員卡無加密憑證";
    }

    var b64plain, hPriKey = 0;
    hPriKey = hca.ATL_GetKeyObjectHandle(m_hModule, m_hSession, 0, "", "2");
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        return "取私鑰失敗: " + m_error;
    }

    var msg = "";
    b64plain = hca.ATL_PrivateKeyDecryption(m_hModule, m_hSession, CKM_RSA_PKCS, b64cipher, hPriKey);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        msg += "失敗: " + m_error + "<br>";
    } else {
        msg += "成功" + "<br>";
    }
    //msg += b64plain + "<br>"; //DEBUG
    if (hPriKey != 0) {
        hca.ATL_DeleteKeyObject(m_hModule, m_hSession, hPriKey);
    }
    return msg;
}

function csOpenComPort() {
    var msg = "";
    if ((hcacs.HCA_OpenCOMPort(0)) == 0)
        msg += "成功";
    else if ((hcacs.HCA_OpenCOMPort(1)) == 0)
        msg += "成功";
    else if ((hcacs.HCA_OpenCOMPort(2)) == 0)
        msg += "成功";
    else if ((hcacs.HCA_OpenCOMPort(3)) == 0)
        msg += "成功";
    else {
        msg += "失敗";
    }

    m_IsPortOpened = true;
    return msg;
}

function csGetBasicData() {
    if (!m_IsPortOpened) {
        return "讀卡機通訊埠尚未開啟";
    }
    var vBasicData = hcacs.HCA_GetBasicData().toArray();
    cs_error = hcacs.GetErrorCode();
    var msg = "";
    if (cs_error == 0) {
        var iCardType = hcacs.HCA_GetCardType();
        var dwCardVersion = hcacs.HCA_GetCardVersion();

        switch (parseInt(iCardType)) {
            case 0:
                msg += "<br>卡別: " + (dwCardVersion == 2 ? "[0]2.0醫事機構卡" : "[0]1.0醫事機構卡") + "<br>";
                msg += "醫事機構代碼: " + vBasicData[0] + "<br>";
                msg += "醫事機構名稱: " + vBasicData[1] + "<br>";
                msg += "卡次: " + vBasicData[2] + "<br>";
                msg += "醫事機構種類: " + vBasicData[3] + "<br>";
                msg += "醫事機構類型: " + vBasicData[4] + "<br>";
                msg += "醫事機構地址: " + vBasicData[5] + "<br>";
                msg += "醫事機構聯絡電話: " + vBasicData[6] + "<br>";
                msg += "醫事機構擁有者: " + vBasicData[7] + "<br>";
                break;

            case 1:
                msg += "<br>卡別: " + (dwCardVersion == 2 ? "[1]2.0醫事人員卡" : "[1]1.0醫事人員卡") + "<br>";
                msg += "醫事人員中文姓名: " + vBasicData[0] + "<br>";
                msg += "醫事人員英文姓名: " + vBasicData[1] + "<br>";
                msg += "醫事人員性別: " + vBasicData[2] + "<br>";
                msg += "醫事人員出生日期: " + vBasicData[3] + "<br>";
                msg += "醫事人員身份證號: " + vBasicData[4] + "<br>";
                msg += "醫事人員類別: " + vBasicData[5] + "<br>";
                break;

            default:
                msg += "未支援之卡別資料";
        }
    } else {
        msg += "失敗: " + cs_error;
    }
    return msg;
}

function csGetCardInfo() {
    if (!m_IsPortOpened) {
        return "讀卡機通訊埠尚未開啟";
    }
    var vCardinfo = hcacs.HCA_GetCardInfo().toArray();
    cs_error = hcacs.GetErrorCode();
    var msg = "";
    if (cs_error == 0) {
        msg += "<br>卡片發行日期: " + vCardinfo[0] + "<br>";
        msg += "卡片有效日期啟始日: " + vCardinfo[1] + "<br>";
        msg += "卡片有效日期結束日: " + vCardinfo[2] + "<br>";
    } else {
        msg += "失敗: " + cs_error;
    }
    return msg;
}

function csGetCardType() {
    if (!m_IsPortOpened) {
        return "讀卡機通訊埠尚未開啟";
    }
    var iCardType = hcacs.HCA_GetCardType();
    cs_error = hcacs.GetErrorCode();
    if (cs_error == 0) {
        switch (parseInt(iCardType)) {
            case 0:
                return "[0]醫事機構卡。";
            case 1:
                return "[1]醫事人員卡。";
            default:
                return "未支援之卡別資料！";
        }
    } else {
        return "失敗: " + cs_error;
    }
}

function csGetSerialNumber() {
    if (!m_IsPortOpened) {
        return "讀卡機通訊埠尚未開啟";
    }
    var strSerialNumber = hcacs.HCA_GetCardSN();
    cs_error = hcacs.GetErrorCode();
    if (cs_error == 0) {
        return "卡片序號: " + strSerialNumber;
    } else {
        return "失敗: " + cs_error;
    }
}

function csVerifyPIN(pincode) {
    if (!m_IsPortOpened) {
        return "讀卡機通訊埠尚未開啟";
    }
    var pincode = document.getElementById("pincode").value;
    if (pincode.length == 6)
        cs_error = hcacs.HCA_VerifyPIN(pincode, 0);
    else
        cs_error = hcacs.HCA_VerifyPIN('', 1);

    if (cs_error == 0)
        return "成功";
    else
        return "失敗: " + cs_error;
}

function csSignData() {
    if (!m_IsPortOpened) {
        return "讀卡機通訊埠尚未開啟";
    }
    var begTime = new Date();
    cs64sig = hcacs.HCA_SignMessage(b64msg, 0);
    var endTime = new Date();
    cs_error = hcacs.GetErrorCode();
    var msg = "";
    if (cs_error == 0) {
        msg += "成功: 簽章時間 " + ((endTime.getTime() - begTime.getTime()) / 1000) + " 秒" + "<br>";
    } else {
        msg += "失敗: " + cs_error;
    }
    //msg += cs64sig + "<br>"; //DEBUG
    return msg;
}
//驗章
function csVerifySign() {
    if (!m_IsPortOpened) {
        return "讀卡機通訊埠尚未開啟";
    }
    cs_error = hcacs.HCA_LoadCert(Sigcert, 0);
    var msg = "";
    if (cs_error == 0) {
        //msg += "<br>載入憑證成功" + "<br>";
    } else {
        return "載入憑證失敗: " + cs_error;
    }
    cs_error = hcacs.HCA_VerifySignMessage(b64msg, cs64sig, 0);
    if (cs_error == 0) {
        msg += "成功" + "<br>";
    } else {
        msg += "失敗: " + cs_error + "<br>";
    }
    return msg;
}
//公鑰加密
function csPublicKeyEncrypt() {
    if (!m_IsPortOpened) {
        return "讀卡機通訊埠尚未開啟";
    }
    var iCardType = hcacs.HCA_GetCardType();
    var dwCardVersion = hcacs.HCA_GetCardVersion();
    if (iCardType == 1 && dwCardVersion == 1) {
        return "失敗: 無加密憑證";
    }
    cs_error = hcacs.HCA_LoadCert(Enccert, 0);
    var msg = "";
    if (cs_error == 0) {
        //msg += "<br>載入憑證成功" + "<br>";
    } else {
        return "載入憑證失敗: " + cs_error;
    }

    cs64cipher = hcacs.HCA_PublicEncrypt(b64msg, 0);
    cs_error = hcacs.GetErrorCode();
    if (cs_error == 0) {
        msg += "成功" + "<br>";
    } else {
        msg += "失敗: " + cs_error + "<br>";
    }
    //msg += cs64cipher + "<br>"; //DEBUG
    return msg;
}
//私鑰解密
function csPrivateKeyDecrypt() {
    if (!m_IsPortOpened) {
        return "讀卡機通訊埠尚未開啟";
    }
    var iCardType = hcacs.HCA_GetCardType();
    var dwCardVersion = hcacs.HCA_GetCardVersion();
    if (iCardType == 1 && dwCardVersion == 1) {
        return "失敗: 無加密憑證";
    }
    var plaintext = hcacs.HCA_PrivateDecrypt(cs64cipher, 0);
    cs_error = hcacs.GetErrorCode();
    var msg = "";
    if (cs_error == 0) {
        msg += "成功" + "<br>";
    } else {
        msg += "失敗: " + cs_error + "<br>";
    }
    return msg;
}

function csGetCerts() {
    if (!m_IsPortOpened) {
        return "讀卡機通訊埠尚未開啟";
    }
    CAcert = hcacs.HCA_GetCert(0);
    cs_error = hcacs.GetErrorCode();
    var msg = "<br>";
    if (cs_error == 0) {
        msg += "取CA憑證成功: 憑證大小 " + CAcert.length + " Byte。" + "<br>";
    } else {
        msg += "取CA憑證失敗: " + cs_error + "<br>";
    }

    Sigcert = hcacs.HCA_GetCert(1);
    cs_error = hcacs.GetErrorCode();
    if (cs_error == 0) {
        msg += "取簽章憑證成功: 憑證大小 " + Sigcert.length + " Byte。" + "<br>";
    } else {
        msg += "取簽章憑證失敗: " + cs_error + "<br>";
    }

    var iCardType = hcacs.HCA_GetCardType();
    var dwCardVersion = hcacs.HCA_GetCardVersion();
    if (iCardType == 1 && dwCardVersion == 1) {
        msg += "取加密憑證失敗: 1.0醫事人員卡無加密憑證" + "<br>";
    } else {
        Enccert = hcacs.HCA_GetCert(2);
        cs_error = hcacs.GetErrorCode();
        if (cs_error == 0) {
            msg += "取加密憑證成功: 憑證大小 " + Enccert.length + " Byte。" + "<br>";
        } else {
            msg += "取加密憑證失敗: " + cs_error + "<br>";
        }
    }

    if (dwCardVersion == 1) {
        msg += "取GRCA憑證失敗: 1.0卡片無GRCA憑證" + "<br>";
    } else {
        GRCAcert = hcacs.HCA_GetCert(3);
        cs_error = hcacs.GetErrorCode();
        if (cs_error == 0) {
            msg += "取GRCA憑證成功: 憑證大小 " + GRCAcert.length + " Byte。" + "<br>";
        } else {
            msg += "取GRCA憑證失敗: " + cs_error + "<br>";
        }
    }
    return msg;
}

function csCloseComPort() {
    var msg = "";
    if (!m_IsPortOpened) {
        return "未開啟通訊埠";
    }
    cs_error = hcacs.HCA_CloseCOMPort();
    if (cs_error == 0) {
        msg += "成功";
        m_IsPortOpened = false;
    } else {
        msg += "失敗: " + cs_error;
    }
    return msg;;
}
function Set_envmsg(msg) {
    //先清空再給值
    env_message = undefined;
    env_message = msg;
}
var hideMask = function () {
    // $('#mask').fadeOut(300).remove();
    $('#mask').fadeOut(300);
};
var viewMask = function (str) {
    var mask = '<div id="mask"></div>';
    var txt = $('<div id="maskTxt">' + str + '</div>').css({
        'text-align': 'center',
        'padding-top': '20%',
        'color': 'white',
        'font-size': '25px'
    });
    if ($('#mask').length <= 0) {
        $('body').append(mask);
    }
    $('#mask').css({
        'position': 'fixed',
        'width': '100%',
        'height': '100%',
        'background': ' rgba(0,0,0,.7)',
        'top': '0',
        'left': '0',
        'display': 'none',
        'z-index': '100'
    });

    if ($('#maskTxt').length <= 0) { $('#mask').append(txt); }
    $('#mask').fadeIn(300);
};

function setEnvTag(tag) {
    document.getElementById('EnvTag').innerHTML = tag;
}