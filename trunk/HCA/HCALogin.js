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

var TS_SERVER_ADDR = "203.65.114.5";//"192.168.0.75";//
var TS_SERVER_PORT = "80";//"801";//

var HCA_OCSP_URL = "http://hcaocsp.nat.gov.tw/cgi-bin/OCSP/ocsp_server.exe";
var MOICA_OCSP_URL = "http://moica.nat.gov.tw/cgi-bin/OCSP/ocsp_server.exe";

var OID_SUBJECT_DIRECTORY_ATTRIBUTE = "2.5.29.9";
var OID_SUBJECT_TYPE = "2.16.886.1.100.2.1";
var OID_HOP = "2.16.886.1.100.3.2.21";
var OID_HPC = "2.16.886.1.100.3.1.7";

var CARD_TYPE_HOP = 0;
var CARD_TYPE_HPC = 1;

var m_hModule = 0, m_hSession = 0, m_hSOSession = 0, m_error = 0, m_cardVer = -1, m_cardType = -1;

function checkModule() {
    if (m_hModule == 0) {
        return "請先初始化HCA密碼模組";
    }
    return "";
}
function checkSession() {
    if (m_hSession == 0) {
        return "請先開啟憑證SESSION";
    }
    return "";
}

function InitModule() {
    m_hModule = hca.ATL_InitModule(HCAPKCS11_MODULE, "");
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        return "初始化HCA密碼模組失敗: " + GetErrorMessage(m_error);
    }
    return "";
}

// 開啟憑證Session (同時驗證 PIN 碼)
// 成功回傳 true, 失敗(含PIN碼錯誤)回傳 false
function InitSession(pin) {

    if (checkModule() < 0) {
        return "";
    }

    if (!pin) {
        return "請輸入憑證PIN碼";
    }

    m_hSession = hca.ATL_InitSession(m_hModule, CKF_RW_SESSION | CKF_SERIAL_SESSION, pin);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        return "開啟憑證Session失敗: " + GetErrorMessage(m_error);
    }

    return "";
}

function CloseSession() {

    if (m_hSession == 0) {
        return "";
    }

    hca.ATL_CloseSession(m_hModule, m_hSession);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        return "關閉憑證Session失敗: " + GetErrorMessage(m_error) + "\n\n請重新啟動「HCA跨瀏覽器元件 ServiSign簽章元件服務」!!";
    }

    m_hSession = 0;
    return "";
}

function CloseModule() {

    if (m_hModule == 0) {
        return "";
    }

    if ((m_error = hca.ATL_CloseModule(m_hModule)) != 0) {
        return "關閉HCA密碼模組失敗: " + GetErrorMessage(m_error) + "\n\n請重新啟動「HCA跨瀏覽器元件 ServiSign簽章元件服務」!!";
    }

    m_hModule = 0;
    return "";
}

function GetBasic(data) {

    var msg = "";
    var rtn;

    if ((msg = checkModule()) != "") {
        return msg;
    }

    // 判斷卡片類型
    var type = hca.ATL_GetHCACardType(m_hModule);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        return "讀取卡片類型失敗: " + GetErrorMessage(m_error);
    }
    else {
        m_cardType = type;
    }

    // 讀取基本資料
    rtn = hca.ATL_GetHCABasicData(m_hModule);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        return "讀取基本資料失敗: " + GetErrorMessage(m_error);
    }
    else {
        var datas = rtn.toArray();
        if (m_cardType == CARD_TYPE_HPC) {
            data.type = "醫事人員卡";
            data.typeID = "0";
            data.醫事人員中文姓名 = datas[0];
            data.醫事人員英文姓名 = datas[1];
            data.醫事人員性別 = datas[2];
            data.醫事人員出生日期 = datas[3];
            data.醫事人員身份證號 = datas[4];
            data.醫事人員類別 = datas[5];
        }
        else {
            data.type = "醫事機構卡";
            data.typeID = "1";
            data.醫事機構代碼 = datas[0];
            data.醫事機構名稱 = datas[1];
            data.卡次 = datas[2];
            data.醫事機構種類 = datas[3];
            data.醫事機構類型 = datas[4];
            data.醫事機構地址 = datas[5];
            data.醫事機構聯絡電話 = datas[6];
            data.醫事機構擁有者 = datas[7];
        }

        console.log(data);
        return "";
    }
}

function GetCert(cert) {
    var msg = "";

    if ((msg = checkModule()) != "") {
        return msg;
    }
    if ((msg = checkSession()) != "") {
        return msg;
    }

    // 固定讀取憑證1 (簽章憑證)
    var b64cert = hca.ATL_GetCertificateFromGPKICard(m_hModule, m_hSession, 1, "");
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        return "取簽章憑證失敗: " + GetErrorMessage(m_error);
    }

    cert.b64 = b64cert;

    // 讀取各欄位

    // SN 憑證序號
    cert.SN = hca.ATL_GetSerialNumber(b64cert);

    // Subject_CN
    var subject, subject_cn;
    if (GET_UTF8_DN < 0) {
        subject = hca.ATL_GetLDAPTypeCertSubjectDN(b64cert);
    } else {
        subject = hca.ATL_GetCertSubjectDN(b64cert);
    }
    subject_cn = hca.ATL_GetRDNFromDN(subject, "MS950", 0, "CN");
    if (subject_cn != null) {
        cert.Subject_CN = subject_cn;
    }
    else {
        cert.Subject_CN = "(none)";
    }
    
    // Subject_E
    var subject_e;
    if (GET_UTF8_DN < 0) {
        subject = hca.ATL_GetLDAPTypeCertSubjectDN(b64cert);
    } else {
        subject = hca.ATL_GetCertSubjectDN(b64cert);
    }
    subject_e = hca.ATL_GetRDNFromDN(subject, "MS950", 0, "Email");
    if (subject_e != null) {
        cert.Subject_E = subject_e;
    }
    else {
        cert.Subject_E = "(none)";
    }

    // UTCIssueDate
    cert.UTCIssueDate = hca.ATL_GetCertNotBefore(b64cert);

    // UTCExpiredDate
    cert.UTCExpiredDate = hca.ATL_GetCertNotAfter(b64cert);

    return "";
}


// 驗證醫事憑證 PIN 
// 驗證成功會呼叫 success callback function 傳入下列資訊:
//   1.卡片記載的基本資料(Object)
//   2.憑證資料(Object)
// 若驗證失敗, 會呼叫 fail callback function 傳入:
//   1.錯誤訊息
function VerifyHCA(pin, success, fail) {
    var msg;
    // 初始化密碼模組
    if ((msg = InitModule()) != "") {
        failCallbackOrAlert(fail, msg);
        return;
    }
    // 開啟憑證Session (同時驗證 PIN 碼)
    if ((msg = InitSession(pin)) != "") {
        failCallbackOrAlert(fail, msg);
        return;
    }

    // PIN 碼驗證成功

    // 讀取卡片基本資料
    var data = {};
    if ((msg = GetBasic(data)) != "") {
        failCallbackOrAlert(fail, msg);
        return;
    }

    // 讀取憑證
    var cert = {};
    if ((msg = GetCert(cert)) != "") {
        failCallbackOrAlert(fail, msg);
        return;
    }

    // 關閉憑證Session (重要)
    if ((msg = CloseSession()) != "") {
        failCallbackOrAlert(fail, msg);
        return;
    }
    // 關閉HCA密碼模組 (重要)
    if ((msg = CloseModule()) != "") {
        failCallbackOrAlert(fail, msg);
        return;
    }

    // success
    if (success && typeof success === "function") {
        success(data, cert);
    }
}

function failCallbackOrAlert(callback, msg) {
    if (callback && typeof callback === "function") {
        callback(msg);
    }
}

