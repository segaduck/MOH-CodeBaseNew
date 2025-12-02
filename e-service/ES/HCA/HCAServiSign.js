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
        alert("請先初始化密碼模組");
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

function InitModule(form) {
    debugger
    m_hModule = hca.ATL_InitModule(HCAPKCS11_MODULE, "");
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("初始化失敗:" + GetErrorMessage(m_error));
    }
    else {
        alert("初始化成功");
    }
}

function checkNotBlank(strField) {
    if (strField.value.length == 0) {
        return -1;
    }
    return 1;
}

function InitSession(form) {

    if (checkModule() < 0) {
        return;
    }

    if (!form.T1.value) {
        alert("請輸入PIN碼");
        return;
    }

    m_hSession = hca.ATL_InitSession(m_hModule, CKF_RW_SESSION | CKF_SERIAL_SESSION, form.T1.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("開啟失敗:" + GetErrorMessage(m_error));
    }
    else {
        alert("開啟成功");
    }
}

function InitSOSession(form) {

    if (checkModule() < 0) {
        return;
    }

    if (checkSessionClose() < 0) {
        return;
    }

    if (checkNotBlank(form.T2) < 0) {
        alert("請輸入PUK碼")
        return;
    }

    m_hSOSession = hca.ATL_InitSOSession(m_hModule, CKF_RW_SESSION | CKF_SERIAL_SESSION, form.T2.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("開啟失敗:" + GetErrorMessage(m_error));
    }
    else {
        alert("開啟成功");
    }
}

function CloseSession(form) {

    if (m_hSession == 0) {
        return;
    }

    hca.ATL_CloseSession(m_hModule, m_hSession);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("關閉失敗:" + GetErrorMessage(m_error));
    }
    else {
        alert("關閉成功");
    }
    m_hSession = 0;
}

function CloseSOSession(form) {

    if (m_hSOSession == 0) {
        return;
    }

    hca.ATL_CloseSession(m_hModule, m_hSOSession);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("關閉失敗:" + GetErrorMessage(m_error));
    }
    else {
        alert("關閉成功");
    }
    m_hSOSession = 0;
}

function CloseModule(form) {

    if (m_hModule == 0) {
        return;
    }

    hca.ATL_CloseModule(m_hModule);
    alert("關閉成功");
}

function CloseModuleEx(form) {

    if (m_hModule == 0) {
        return;
    }

    hca.ATL_CloseModuleEx(m_hModule, 0);
    alert("關閉成功");
}

function UnblockUserPIN(form) {

    var rtn;
    if (checkModule() < 0) {
        return;
    }

    if (checkSOSession() < 0) {
        return;
    }

    if (checkNotBlank(form.T3) < 0) {
        alert("請輸入新PIN碼")
        return;
    }

    rtn = hca.ATL_UnblockUserPIN(m_hModule, m_hSOSession, form.T3.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("解鎖失敗:" + GetErrorMessage(m_error));
    }
    else {
        alert("解鎖成功");
    }
}

function GetCardVersion(form) {

    var ver;
    if (checkModule() < 0) {
        return;
    }

    ver = hca.ATL_GetCardHCACardVersion(m_hModule);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("操作失敗:" + GetErrorMessage(m_error));
    }
    else {
        alert("卡片版本為:" + ver);
        m_cardVer = ver;
    }
}

function GetCardType(form, justreturn) {

    var type;
    if (checkModule() < 0) {
        return;
    }

    type = hca.ATL_GetHCACardType(m_hModule);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("操作失敗:" +GetErrorMessage( m_error));
    }
    else {
        m_cardType = type;
        if (!justreturn) {
            if (type == CARD_TYPE_HOP) {
                alert("醫事機構卡");
            }
            else {
                alert("醫事人員卡");
            }
        }
    }
}

function GetBasic(form) {

    var rtn;
    if (checkModule() < 0) {
        return;
    }
    if (checkCardVer() < 0) {
        return;
    }
    if (m_cardType == -1) {
        GetCardType(form, true);
    }

    rtn = hca.ATL_GetHCABasicData(m_hModule);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("操作失敗:" + GetErrorMessage(m_error));
    }
    else {
        var msg = "";
        var datas = rtn.toArray();
        if (m_cardType == CARD_TYPE_HPC) {
            msg += "醫事人員卡\n";
            msg += "醫事人員中文姓名:" + datas[0] + "\n";
            msg += "醫事人員英文姓名:" + datas[1] + "\n";
            msg += "醫事人員性別:" + datas[2] + "\n";
            msg += "醫事人員出生日期:" + datas[3] + "\n";
            msg += "醫事人員身份證號:" + datas[4] + "\n";
            msg += "醫事人員類別:" + datas[5] + "\n";
        }
        else {
            msg += "醫事機構卡\n";
            msg += "醫事機構代碼:" + datas[0] + "\n";
            msg += "醫事機構名稱:" + datas[1] + "\n";
            msg += "卡次:" + datas[2] + "\n";
            msg += "醫事機構種類:" + datas[3] + "\n";
            msg += "醫事機構類型:" + datas[4] + "\n";
            msg += "醫事機構地址:" + datas[5] + "\n";
            msg += "醫事機構聯絡電話:" + datas[6] + "\n";
            msg += "醫事機構擁有者:" + datas[7] + "\n";
        }

        alert(msg);
    }
}

function GetSN(form) {

    if (checkModule() < 0) {
        return;
    }
    var sn;
    sn = hca.ATL_GetHCACardSN(m_hModule);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("操作失敗:" + GetErrorMessage(m_error));
    }
    else {
        alert("卡片序號:" + sn);
    }
}

function GetInfo(form) {

    if (checkModule() < 0) {
        return;
    }
    var rtn;
    rtn = hca.ATL_GetHCACardInfo(m_hModule);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("操作失敗:" + GetErrorMessage(m_error));
    }
    else {
        var infos = rtn.toArray();

        var msg = "";
        if (infos[0] == "0") {
            msg += "卡片不支援產生RSA金鑰\n";
        }
        else {
            msg += "卡片支援產生RSA金鑰\n";
        }

        msg += "卡片發行日期:" + infos[1] + "\n";
        msg += "卡片有效日期啟始日:" + infos[2].substr(0, 7) + "\n";
        msg += "卡片有效日期結束日:" + infos[2].substr(7, 7) + "\n";

        if (infos[3] == "1") {
            msg += "卡片狀態:已開卡" + "\n";
        }
        else {
            msg += "卡片狀態:未開卡" + "\n";
        }

        if (infos[4] == "0") {
            msg += "Card on use\n";
        }
        else {
            msg += "Card termination\n";
        }

        alert(msg);
    }
}

function GetKeyLen(form) {

    if (checkModule() < 0) {
        return;
    }
    var rtn;
    rtn = hca.ATL_GetHCAKeyLength(m_hModule);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("操作失敗:" + GetErrorMessage(m_error));
    }
    else {
        alert("金鑰長度:" + (rtn) + " bytes");
    }
}

function GetCertType(form) {

    if (checkModule() < 0) {
        return;
    }
    var rtn;
    rtn = hca.ATL_GetHCACertType(m_hModule);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("操作失敗:" + GetErrorMessage(m_error));
    }
    else {
        if (rtn == 2) {
            alert("憑證類型:2(SHA256)");
        }
        else if (rtn == 1) {
            alert("憑證類型:1(SHA1)");
        }
        else {
            alert("憑證類型:" + rtn + "(Unknown)");
        }
    }
}

function GetCert(form) {

    if (checkModule() < 0) {
        return;
    }
    if (checkSession() < 0) {
        return;
    }

    var cacert, cert1, cert2;
    cacert = hca.ATL_GetCertificateFromGPKICard(m_hModule, m_hSession, 0, "");
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("取CA憑證失敗:" + GetErrorMessage(m_error));
    }
    else {
        var err = hca.ATL_Util_WriteFileFromBase64(form.T10.value, cacert);
        if (err != 0) {
            alert("存檔失敗:" + err);
        }
        else {
            alert("取得CA憑證成功");
        }
    }

    cert1 = hca.ATL_GetCertificateFromGPKICard(m_hModule, m_hSession, 1, "");
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("取簽章憑證失敗:" + GetErrorMessage(m_error));
    }
    else {
        var err = hca.ATL_Util_WriteFileFromBase64(form.T8.value, cert1);
        if (err != 0) {
            alert("存檔失敗:" + err);
        }
        else {
            alert("取得簽章憑證成功");
        }
    }

    cert2 = hca.ATL_GetCertificateFromGPKICard(m_hModule, m_hSession, 2, "");
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("取加密憑證失敗:" + GetErrorMessage(m_error));
    }
    else {
        var err = hca.ATL_Util_WriteFileFromBase64(form.T9.value, cert2);
        if (err != 0) {
            alert("存檔失敗:" + err);
        }
        else {
            alert("取得加密憑證成功");
        }
    }
}

function Sign(form) {

    if (checkModule() < 0) {
        return;
    }
    if (checkSession() < 0) {
        return;
    }

    var b64msg, hPriKey = 0, b64sig;
    b64msg = hca.ATL_Util_ReadFileToBase64(form.T4.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("檔案讀取失敗");
        return;
    }

    hPriKey = hca.ATL_GetKeyObjectHandle(m_hModule, m_hSession, 0, "", "1");
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("取私鑰失敗:" + GetErrorMessage(m_error));
        return;
    }

    //	b64sig = hca.ATL_MakeSignature(m_hModule, m_hSession, CKM_SHA1_RSA_PKCS, b64msg, hPriKey);
    b64sig = hca.ATL_MakeSignatureEx2(m_hModule, m_hSession, CKM_SHA256_RSA_PKCS, 0, b64msg, hPriKey);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("簽章失敗:" + GetErrorMessage(m_error));
    }
    else {
        var err = hca.ATL_Util_WriteFileFromBase64(form.T5.value, b64sig);
        if (err != 0) {
            alert("寫入簽章檔失敗:" + err);
        }
        else {
            alert("簽章完成\r\n");
        }
    }

    if (hPriKey != 0) {
        hca.ATL_DeleteKeyObject(m_hModule, m_hSession, hPriKey);
    }
}

function Verify(form) {

    if (checkModule() < 0) {
        return;
    }
    if (checkSession() < 0) {
        return;
    }

    var b64msg, b64sig, hPubKey = 0;
    b64msg = hca.ATL_Util_ReadFileToBase64(form.T4.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("原文檔案讀取失敗");
        return;
    }

    b64sig = hca.ATL_Util_ReadFileToBase64(form.T5.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("簽章檔讀取失敗");
        return;
    }

    hPubKey = hca.ATL_GetKeyObjectHandle(m_hModule, m_hSession, 1, "", "1");
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("取公鑰失敗:" + GetErrorMessage(m_error));
        return;
    }

    //	m_error = hca.ATL_VerifySignature(m_hModule, m_hSession, CKM_SHA1_RSA_PKCS, b64msg, hPubKey, b64sig);
    m_error = hca.ATL_VerifySignature(m_hModule, m_hSession, CKM_SHA256_RSA_PKCS, b64msg, hPubKey, b64sig);
    if (m_error == 0) {
        alert("驗章成功");
    }
    else {
        alert("驗章失敗:" + GetErrorMessage(m_error));
    }

    if (hPubKey != 0) {
        hca.ATL_DeleteKeyObject(m_hModule, m_hSession, hPubKey);
    }
}

function VerifyByCertFile(form) {

    var b64cert, b64data, b64sig;

    b64data = hca.ATL_Util_ReadFileToBase64(form.T4.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("簽章原文檔讀取失敗");
        return;
    }

    b64sig = hca.ATL_Util_ReadFileToBase64(form.T5.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("簽章檔讀取失敗");
        return;
    }

    b64cert = hca.ATL_Util_ReadFileToBase64(form.T8.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("驗章憑證檔讀取失敗");
    }

    //	m_error = hca.ATL_VerifySignatureByCert(b64cert, CKM_SHA1_RSA_PKCS, b64data, b64sig);
    //	m_error = hca.ATL_VerifySignatureByCert(b64cert, CKM_SHA256_RSA_PKCS, b64data, b64sig);
    m_error = hca.ATL_VerifySignatureByCert2(b64cert, b64data, b64sig);
    if (m_error == 0) {
        alert("驗章成功");
    }
    else {
        alert("驗章失敗:" + GetErrorMessage(m_error));
    }
}


function Enc(form) {

    if (checkModule() < 0) {
        return;
    }
    if (checkSession() < 0) {
        return;
    }

    var b64msg, b64cipher, hPubKey = 0;

    b64msg = hca.ATL_Util_ReadFileToBase64(form.T4.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("資料檔讀取失敗\r\n");
        return;
    }

    hPubKey = hca.ATL_GetKeyObjectHandle(m_hModule, m_hSession, 1, "", "2");
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("取公鑰失敗:" + GetErrorMessage(m_error));
        return;
    }

    b64cipher = hca.ATL_PublicKeyEncryption(m_hModule, m_hSession, CKM_RSA_PKCS, b64msg, hPubKey);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("加密失敗:" + GetErrorMessage(m_error));
    }
    else {
        var err = hca.ATL_Util_WriteFileFromBase64(form.T6.value, b64cipher);
        if (err != 0) {
            alert("寫入加密檔失敗:" + err);
        }
        else {
            alert("加密完成");
        }
    }

    if (hPubKey != 0) {
        hca.ATL_DeleteKeyObject(m_hModule, m_hSession, hPubKey);
    }
}

function EncByCertFile(form) {

    var b64cert, b64data, b64cipher;

    b64cert = hca.ATL_Util_ReadFileToBase64(form.T9.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("加密憑證檔讀取失敗");
        return;
    }

    b64data = hca.ATL_Util_ReadFileToBase64(form.T4.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("簽章原文檔讀取失敗");
        return;
    }

    b64cipher = hca.ATL_PublicKeyEncryptionByCert(b64cert, CKM_RSA_PKCS, b64data);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("加密失敗:" + GetErrorMessage(m_error));
    }
    else {
        var err = hca.ATL_Util_WriteFileFromBase64(form.T6.value, b64cipher);
        if (err != 0) {
            alert("寫入加密檔失敗:" + err);
        }
        else {
            alert("加密完成");
        }
    }
}

function Dec(form) {

    if (checkModule() < 0) {
        return;
    }
    if (checkSession() < 0) {
        return;
    }

    var b64cipher, b64plain, hPriKey = 0;

    b64cipher = hca.ATL_Util_ReadFileToBase64(form.T6.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("資料加密檔讀取失敗");
        return;
    }

    hPriKey = hca.ATL_GetKeyObjectHandle(m_hModule, m_hSession, 0, "", "2");
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("取私鑰失敗:" + GetErrorMessage(m_error));
        return;
    }

    b64plain = hca.ATL_PrivateKeyDecryption(m_hModule, m_hSession, CKM_RSA_PKCS, b64cipher, hPriKey);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("解密失敗:" + GetErrorMessage(m_error));
    }
    else {
        var err = hca.ATL_Util_WriteFileFromBase64(form.T7.value, b64plain);
        if (err != 0) {
            alert("寫入解密檔失敗:" + err);
        }
        else {
            alert("解密完成");
        }
    }

    if (hPriKey != 0) {
        hca.ATL_DeleteKeyObject(m_hModule, m_hSession, hPriKey);
    }
}

function GetSignAlgo(form) {

    var b64cert, b64sig, algo;
    b64sig = hca.ATL_Util_ReadFileToBase64(form.T5.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("簽章檔讀取失敗");
        return;
    }

    b64cert = hca.ATL_Util_ReadFileToBase64(form.T8.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("驗章憑證檔讀取失敗");
    }

    algo = hca.ATL_CryptSignatureGetAlgorithm(b64cert, b64sig, 0);

    if ((m_error = hca.ATL_GetErrorCode()) == 0) {
        alert("簽章演算法為 " + algo);
    }
    else {
        alert("取簽章演算法失敗:" + GetErrorMessage(m_error));
    }
}

function GetCertAttr(form) {

    if (checkNotBlank(form.txtCertNum) < 0) {
        alert("請在[CertNum 0~2]欄位輸入屬性值", "資訊");
    }
    var lCertNum = parseInt(form.txtCertNum.value);

    if (checkNotBlank(form.txtCertAttr) < 0) {
        alert("請在[Attribute 1~16]欄位輸入屬性值", "資訊");
    }
    var lAttrFlag = parseInt(form.txtCertAttr.value);

    var b64cert;

    switch (lCertNum) {
        case 0: {
            b64cert = hca.ATL_Util_ReadFileToBase64(form.T10.value);
            if ((m_error = hca.ATL_GetErrorCode()) != 0) {
                alert("CA憑證檔讀取失敗");
                return;
            }
            break;
        }
        case 1: {
            b64cert = hca.ATL_Util_ReadFileToBase64(form.T8.value);
            if ((m_error = hca.ATL_GetErrorCode()) != 0) {
                alert("簽章憑證檔讀取失敗");
                return;
            }
            break;
        }
        case 2: {
            b64cert = hca.ATL_Util_ReadFileToBase64(form.T9.value);
            if ((m_error = hca.ATL_GetErrorCode()) != 0) {
                alert("加密憑證檔讀取失敗");
                return;
            }
            break;
        }
        default:
            alert("憑證請指定0:CA憑證, 1:驗章憑證, 2:加密憑證");
            return;
    }

    switch (lAttrFlag) {

        case 1: {
            var sn = hca.ATL_GetSerialNumber(b64cert);
            alert("憑證序號：" + sn);
            break;
        }
        case 2: {
            var issuer, issuer_c;
            if (GET_UTF8_DN < 0) {
                issuer = hca.ATL_GetLDAPTypeCertIssuerDN(b64cert);
            } else {
                issuer = hca.ATL_GetCertIssuerDN(b64cert);
            }
            issuer_c = hca.ATL_GetRDNFromDN(issuer, "MS950", 0, "C");
            if (issuer_c != null) {
                alert("Iusser_C：" + issuer_c);
            }
            else {
                str.Format("Iusser_C：(none)");
            }
            break;
        }
        case 3: {
            var issuer, issuer_o;
            if (GET_UTF8_DN < 0) {
                issuer = hca.ATL_GetLDAPTypeCertIssuerDN(b64cert);
            } else {
                issuer = hca.ATL_GetCertIssuerDN(b64cert);
            }
            issuer_o = hca.ATL_GetRDNFromDN(issuer, "MS950", 0, "O");
            if (issuer_o != null) {
                alert("Iusser_O：" + issuer_o);
            }
            else {
                alert("Iusser_O：(none)");
            }
            break;
        }
        case 4: {
            var issuer, issuer_ou;
            if (GET_UTF8_DN < 0) {
                issuer = hca.ATL_GetLDAPTypeCertIssuerDN(b64cert);
            } else {
                issuer = hca.ATL_GetCertIssuerDN(b64cert);
            }
            issuer_ou = hca.ATL_GetRDNFromDN(issuer, "MS950", 0, "OU");
            if (issuer_ou != null) {
                alert("Iusser_OU：" + issuer_ou);
            }
            else {
                alert("Iusser_OU：(none)");
            }
            break;
        }
        case 5: {
            var issuer, issuer_cn;
            if (GET_UTF8_DN < 0) {
                issuer = hca.ATL_GetLDAPTypeCertIssuerDN(b64cert);
            } else {
                issuer = hca.ATL_GetCertIssuerDN(b64cert);
            }
            issuer_cn = hca.ATL_GetRDNFromDN(issuer, "MS950", 0, "CN");
            if (issuer_cn != null) {
                alert("Iusser_CN：" + issuer_cn);
            }
            else {
                alert("Iusser_CN：(none)");
            }
            break;
        }
        case 6: {
            var issuer, issuer_e;
            if (GET_UTF8_DN < 0) {
                issuer = hca.ATL_GetLDAPTypeCertIssuerDN(b64cert);
            } else {
                issuer = hca.ATL_GetCertIssuerDN(b64cert);
            }
            issuer_e = hca.ATL_GetRDNFromDN(issuer, "MS950", 0, "Email");
            if (issuer_e != null) {
                alert("Iusser_E：" + issuer_e);
            }
            else {
                alert("Iusser_E：(none)");
            }
            break;
        }
        case 7: {
            var subject, subject_c;
            if (GET_UTF8_DN < 0) {
                subject = hca.ATL_GetLDAPTypeCertSubjectDN(b64cert);
            } else {
                subject = hca.ATL_GetCertSubjectDN(b64cert);
            }
            subject_c = hca.ATL_GetRDNFromDN(subject, "MS950", 0, "C");
            if (subject_c != null) {
                alert("Subject_C：" + subject_c);
            }
            else {
                alert("Subject_C：(none)");
            }
            break;
        }

        case 8: {
            var subject, subject_o;
            if (GET_UTF8_DN < 0) {
                subject = hca.ATL_GetLDAPTypeCertSubjectDN(b64cert);
            } else {
                subject = hca.ATL_GetCertSubjectDN(b64cert);
            }
            subject_o = hca.ATL_GetRDNFromDN(subject, "MS950", 0, "O");
            if (subject_o != null) {
                alert("Subject_O：" + subject_o);
            }
            else {
                alert("Subject_O：(none)");
            }
            break;
        }
        case 9: {
            var subject, subject_ou;
            if (GET_UTF8_DN < 0) {
                subject = hca.ATL_GetLDAPTypeCertSubjectDN(b64cert);
            } else {
                subject = hca.ATL_GetCertSubjectDN(b64cert);
            }
            subject_ou = hca.ATL_GetRDNFromDN(subject, "MS950", 0, "OU");
            if (subject_ou != null) {
                alert("Subject_OU：" + subject_ou);
            }
            else {
                alert("Subject_OU：(none)");
            }
            break;
        }
        case 10: {
            var subject, subject_cn;
            if (GET_UTF8_DN < 0) {
                subject = hca.ATL_GetLDAPTypeCertSubjectDN(b64cert);
            } else {
                subject = hca.ATL_GetCertSubjectDN(b64cert);
            }
            subject_cn = hca.ATL_GetRDNFromDN(subject, "MS950", 0, "CN");
            if (subject_cn != null) {
                alert("Subject_CN：" + subject_cn);
            }
            else {
                alert("Subject_CN：(none)");
            }
            break;
        }
        case 11: {
            var subject, subject_e;
            if (GET_UTF8_DN < 0) {
                subject = hca.ATL_GetLDAPTypeCertSubjectDN(b64cert);
            } else {
                subject = hca.ATL_GetCertSubjectDN(b64cert);
            }
            subject_e = hca.ATL_GetRDNFromDN(subject, "MS950", 0, "Email");
            if (subject_e != null) {
                alert("Subject_E：" + subject_e);
            }
            else {
                alert("Subject_E：(none)");
            }
            break;
        }
        case 12: {
            var notBefore = hca.ATL_GetCertNotBefore(b64cert);
            alert("UTCIssueDate：" + notBefore);
            break;
        }
        case 13: {
            var notAfter = hca.ATL_GetCertNotAfter(b64cert);
            alert("UTCExpiredDate：" + notAfter);
            break;
        }
        case 14: {
            var ext = hca.ATL_GetCertExtension(b64cert);
            alert("Extension：" + ext);
            break;
        }
        case 15: {
            var pubkey = hca.ATL_GetCertPublicKey(b64cert);
            alert("Public Key：" + pubkey);
            break;
        }
        case 16: {
            var ku = hca.ATL_GetCertKeyUsage(b64cert);
            alert("Key Usage：" + ku);
            break;
        }
        case 17: {
            var algo = hca.ATL_CryptCertGetSignatureAlgorithm(b64cert, 0);
            alert("Sign Algorithm：" + algo);
            break;
        }
        default:
            break;
    }
}

function VerifyCert(form) {

    var b64cert, b64cacert;
    if (checkNotBlank(form.T11) < 0) {
        alert("請輸入欲驗證的憑證路徑");
        return;
    }

    b64cert = hca.ATL_Util_ReadFileToBase64(form.T11.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("欲驗證之憑證檔讀取失敗");
    }

    b64cacert = hca.ATL_Util_ReadFileToBase64(form.T10.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("CA憑證檔讀取失敗");
    }

    m_error = hca.ATL_VerifyCertSignature(b64cert, b64cacert);
    if (m_error != 0) {
        alert("驗證失敗:" + GetErrorMessage(m_error));
    }
    else {
        alert("驗證成功");
    }
}

function VerifyCRLSig(form) {

    if (checkNotBlank(form.T10) < 0) {
        alert("請輸入CA憑證檔路徑");
        return;
    }

    if (checkNotBlank(form.T12) < 0) {
        alert("請輸入CRL檔路徑");
        return;
    }

    var b64cacert, b64CRL;
    b64cacert = hca.ATL_Util_ReadFileToBase64(form.T10.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("CA憑證檔讀取失敗");
        return;
    }

    b64CRL = hca.ATL_Util_ReadFileToBase64(form.T12.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("CRL檔讀取失敗");
        return;
    }

    m_error = hca.ATL_VerifyCRLSignature(b64CRL, b64cacert);
    if (m_error != 0) {
        alert("驗證失敗:" + GetErrorMessage(m_error));
    }
    else {
        alert("驗證成功");
    }
}
function GetCRLInfo(form) {

    if (checkNotBlank(form.T12) < 0) {
        alert("請輸入CRL檔路徑");
        return;
    }

    var b64CRL, data;
    b64CRL = hca.ATL_Util_ReadFileToBase64(form.T12.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("CRL檔讀取失敗");
        return;
    }

    data = hca.ATL_GetCRLInfo_GetIssuerDN(b64CRL);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("取CRL簽發者DN失敗:" + GetErrorMessage(m_error));
    }
    else {
        alert("CRL簽發者DN為:" + data);
    }

    data = hca.ATL_GetCRLInfo_GetThisUpdate(b64CRL);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("取CRL有效日期失敗:" + GetErrorMessage(m_error));
    }
    else {
        alert("CRL有效日期為:" + data);
    }

    data = hca.ATL_GetCRLInfo_GetNextUpdate(b64CRL);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("取CRL下次更新時間失敗:" + GetErrorMessage(m_error));
    }
    else {
        alert("CRL下次更新時間為:" + data);
    }
}
function SearchCRL(form) {

    if (checkNotBlank(form.T12) < 0) {
        alert("請輸入CRL檔路徑");
        return;
    }
    if (checkNotBlank(form.T13) < 0) {
        alert("請輸入欲查詢的憑證序號(16進位格式)");
        return;
    }

    var b64CRL, status, revokeDate, reason;
    b64CRL = hca.ATL_Util_ReadFileToBase64(form.T12.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("CRL檔讀取失敗");
        return;
    }

    status = hca.ATL_SearchCRL_GetCertStatus(b64CRL, form.T13.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("驗CRL失敗:" + GetErrorMessage(m_error));
        return;
    }
    else {
        alert("憑證狀態為:" + status);
        if (status != 0) {
            revokeDate = hca.ATL_SearchCRL_GetRevokedDate("", form.T13.value);
            alert("撤銷日期為:" + revokeDate);
            reason = hca.ATL_SearchCRL_GetRevokedReason("", form.T13.value);
            alert("撤銷理由為:" + reason);
        }
    }
}

function GenSessionKey(form) {

    var b64Key = hca.ATL_GenerateSessionKey(DES3_KEY_LENGTH);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("產生SESSION KEY失敗:" + GetErrorMessage(m_error));
    }
    else {
        alert("產生SESSION KEY成功");
        form.T16.value = b64Key;
    }
}

function SymmEnc(form) {

    if (checkModule() < 0) {
        return;
    }
    if (checkSession() < 0) {
        return;
    }
    if (checkNotBlank(form.T16) < 0) {
        alert("請先產生SESSION KEY");
        return;
    }
    if (checkNotBlank(form.T17) < 0) {
        alert("請填入IV值");
        return;
    }

    var b64msg, hSnKey = 0, b64cipher;
    b64msg = hca.ATL_Util_ReadFileToBase64(form.T4.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("檔案讀取失敗");
        return;
    }

    hSnKey = hca.ATL_GetKeyObjectHandle(m_hModule, m_hSession, 2, "", form.T16.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("取SESSION KEY失敗:" + GetErrorMessage(m_error));
        return;
    }

    b64cipher = hca.ATL_SymEncryptionAlg(m_hModule, m_hSession, CKM_DES3_CBC, b64msg, form.T17.value, hSnKey);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("加密失敗:" + GetErrorMessage(m_error));
    }
    else {
        var err = hca.ATL_Util_WriteFileFromBase64(form.T14.value, b64cipher);
        if (err != 0) {
            alert("寫入加密檔失敗:" + err);
        }
        else {
            alert("加密完成\r\n");
        }
    }

    if (hSnKey != 0) {
        hca.ATL_DeleteKeyObject(m_hModule, m_hSession, hSnKey);
    }
}

function SymmDec(form) {
    if (checkModule() < 0) {
        return;
    }
    if (checkSession() < 0) {
        return;
    }
    if (checkNotBlank(form.T16) < 0) {
        alert("請先產生SESSION KEY");
        return;
    }
    if (checkNotBlank(form.T17) < 0) {
        alert("請填入IV值");
        return;
    }

    var b64msg, hSnKey = 0, b64cipher;
    b64cipher = hca.ATL_Util_ReadFileToBase64(form.T14.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("檔案讀取失敗");
        return;
    }

    hSnKey = hca.ATL_GetKeyObjectHandle(m_hModule, m_hSession, 2, "", form.T16.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("取SESSION KEY失敗:" + GetErrorMessage(m_error));
        return;
    }

    b64msg = hca.ATL_SymDecryptionAlg(m_hModule, m_hSession, CKM_DES3_CBC, b64cipher, form.T17.value, hSnKey);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("解密失敗:" + GetErrorMessage(m_error));
    }
    else {
        var err = hca.ATL_Util_WriteFileFromBase64(form.T15.value, b64msg);
        if (err != 0) {
            alert("寫入解密檔失敗:" + err);
        }
        else {
            alert("解密完成\r\n");
        }
    }

    if (hSnKey != 0) {
        hca.ATL_DeleteKeyObject(m_hModule, m_hSession, hSnKey);
    }
}

function Hash(form) {
    if (checkModule() < 0) {
        return;
    }
    if (checkSession() < 0) {
        return;
    }

    var b64msg, b64digest;
    b64msg = hca.ATL_Util_ReadFileToBase64(form.T4.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("檔案讀取失敗");
        return;
    }

    b64digest = hca.ATL_HashFunction(m_hModule, m_hSession, CKM_SHA_1, b64msg);
    //	b64digest = hca.ATL_HashFunction(m_hModule, m_hSession, CKM_SHA224, b64msg);
    //	b64digest = hca.ATL_HashFunction(m_hModule, m_hSession, CKM_SHA256, b64msg);
    //	b64digest = hca.ATL_HashFunction(m_hModule, m_hSession, CKM_SHA384, b64msg);
    //	b64digest = hca.ATL_HashFunction(m_hModule, m_hSession, CKM_SHA512, b64msg);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("產生HASH值失敗:" + GetErrorMessage(m_error));
    }
    else {
        var err = hca.ATL_Util_WriteFileFromBase64(form.T18.value, b64digest);
        if (err != 0) {
            alert("寫入檔案失敗:" + err);
        }
        else {
            alert("產生HASH值完成\r\n");
        }
    }
}

function Hash2(form) {
    if (checkModule() < 0) {
        return;
    }
    if (checkSession() < 0) {
        return;
    }

    var b64msg, b64digest;
    b64msg = hca.ATL_Util_ReadFileToBase64(form.T4.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("檔案讀取失敗");
        return;
    }

    //	b64digest = hca.ATL_HashFunction(m_hModule, m_hSession, CKM_SHA_1, b64msg);
    //	b64digest = hca.ATL_HashFunction(m_hModule, m_hSession, CKM_SHA224, b64msg);
    b64digest = hca.ATL_HashFunction(m_hModule, m_hSession, CKM_SHA256, b64msg);
    //	b64digest = hca.ATL_HashFunction(m_hModule, m_hSession, CKM_SHA384, b64msg);
    //	b64digest = hca.ATL_HashFunction(m_hModule, m_hSession, CKM_SHA512, b64msg);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("產生HASH值失敗:" + GetErrorMessage(m_error));
    }
    else {
        var err = hca.ATL_Util_WriteFileFromBase64(form.T18.value, b64digest);
        if (err != 0) {
            alert("寫入檔案失敗:" + err);
        }
        else {
            alert("產生HASH值完成\r\n");
        }
    }
}

function DoTSQuery(form) {

    if (checkNotBlank(form.T5) < 0) {
        alert("找不到簽章檔");
        return;
    }

    var b64sig;
    b64sig = hca.ATL_Util_ReadFileToBase64(form.T5.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("簽章檔讀取失敗");
        return;
    }


    var ts = hca.ATL_TSQuery(TS_SERVER_ADDR, TS_SERVER_PORT, b64sig);
    if (hca.ATL_GetErrorCode() != 0) {
        alert("ATL_TSQuery fail " + hca.ATL_GetErrorCode());
        return;
    }
    else {
        var err = hca.ATL_Util_WriteFileFromBase64(form.T22.value, ts);
        if (err != 0) {
            alert("寫入檔案失敗:" + err);
        }
        else {
            alert("TIMESTAMP QUERY成功\r\n");
        }
    }
}

function DoTSVerify(form) {

    if (checkNotBlank(form.T5) < 0) {
        alert("找不到簽章檔");
        return;
    }

    if (checkNotBlank(form.T22) < 0) {
        alert("找不到TS簽章檔");
        return;
    }

    var b64sig, b64ts;
    b64sig = hca.ATL_Util_ReadFileToBase64(form.T5.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("簽章檔讀取失敗");
        return;
    }
    b64ts = hca.ATL_Util_ReadFileToBase64(form.T22.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("TS簽章檔讀取失敗");
        return;
    }

    var rtn = hca.ATL_TSVerify(b64sig, b64ts);
    if (rtn == 0) {
        alert("TS驗證成功");
    }
    else {
        alert("TS驗證失敗:" + rtn);
    }
}

function DoGetTSInfo(form, param) {

    if (checkNotBlank(form.T22) < 0) {
        alert("找不到TS簽章檔");
        return;
    }

    var b64ts;
    b64ts = hca.ATL_Util_ReadFileToBase64(form.T22.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("TS簽章檔讀取失敗");
        return;
    }

    var iField = parseInt(param);
    var info = hca.ATL_GetTSINFO(b64ts, iField);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("取TS資訊失敗: " + m_error);
    }
    else {
        alert(info);
    }
}

function GenTbsOCSPReq(form) {

    if (checkNotBlank(form.T20) < 0) {
        alert("請輸入憑證序號");
        return;
    }
    var b64req, b64cacert;
    b64cacert = hca.ATL_Util_ReadFileToBase64(form.T10.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("CA憑證檔讀取失敗");
        return;
    }

    b64req = hca.ATL_BuildTobesignedOCSPRequest(form.T20.value, form.T21.value, b64cacert);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("產生OCSP失敗:" + GetErrorMessage(m_error));
    }
    else {
        var err = hca.ATL_Util_WriteFileFromBase64(form.T19.value, b64req);
        if (err != 0) {
            alert("寫入檔案失敗:" + err);
        }
        else {
            alert("產生OCSP請求檔完成\r\n");
        }
    }
}

function GetSNCnts(Sns) {
    return Sns.split(",").length;
}

function QueryOCSP(form) {

    if (checkNotBlank(form.T20) < 0) {
        alert("請輸入憑證序號");
        return;
    }

    var b64OCSPReq, b64cacert;
    b64OCSPReq = hca.ATL_Util_ReadFileToBase64(form.T19.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("OCSP請求檔讀取失敗");
        return;
    }
    b64cacert = hca.ATL_Util_ReadFileToBase64(form.T10.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("CA憑證檔讀取失敗");
        return;
    }

    m_error = hca.ATL_QueryOCSfromSignedOCSPRequest(form.T20.value, b64OCSPReq, "", b64cacert, "", "", HCA_OCSP_URL, "", 0);

    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("查詢OCSP失敗:" + GetErrorMessage(m_error));
    }
    else {
        var cnts = GetSNCnts(form.T20.value);
        var i;
        for (i = 0; i < cnts; i++) {
            var str = "第" + (i + 1) + "張憑證查詢結果\n";
            var res_status = hca.ATL_OCSPGetResponseStatus(i);
            if (res_status != 0) {
                str += "OCSP回應狀態錯誤:" + res_status;
                alert(str);
                continue;
            }
            else {
                var cert_status = hca.ATL_OCSPGetCertStatus(i);
                if (cert_status == 0) {
                    str += "憑證狀態為正常";
                    alert(str);
                    continue;
                }
                else if (cert_status == 1) {
                    str += "憑證狀態為已撤銷\n";
                    var reason = hca.ATL_OCSPGetRevokeReason(i);
                    str += "撤銷理由為:";
                    str += reason;
                    alert(str);
                    continue;
                }
                else if (cert_status == 2) {
                    str += "憑證不存在";
                    alert(str);
                    continue;
                }
                else {
                    str += "憑證狀態為不明:" + cert_status;
                    alert(str);
                    continue;
                }
            }
        }
    }
}

function DownloadCert(form) {

    var certs = hca.ATL_GetCertFromLDAP(form.T23.value, form.T24.value, form.T25.value, form.T26.value, form.T27.value, "");
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("下載憑證失敗:" + GetErrorMessage(m_error));
    }
    else {
        var x509certs = certs.toArray();
        alert("共有 " + x509certs.length + " 張 X.509 憑證");
        for (i = 0; i < x509certs.length; i++) {
            var strX509Cert = x509certs[i];
            var msg = "[第" + (i + 1) + "張] 憑證\n" + strX509Cert;
            alert(msg);
            hca.ATL_Util_WriteFileFromBase64("D:/Cert" + (i + 1) + ".cer", strX509Cert);

        }
    }
}

function GetCertExt(form) {

    if (checkNotBlank(form.T28) < 0) {
        alert("請輸入憑證路徑");
        return;
    }

    var b64cert, b64ext, b64attr, oid;
    b64cert = hca.ATL_Util_ReadFileToBase64(form.T28.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("憑證讀取失敗");
        return;
    }

    b64ext = hca.ATL_GetExtension(b64cert, OID_SUBJECT_DIRECTORY_ATTRIBUTE);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("讀取SubjectDirectoryAttribute Extension失敗:" + GetErrorMessage(m_error));
        return;
    }

    b64attr = hca.ATL_GetSubjectDirectoryAttributes(b64cert, OID_SUBJECT_TYPE);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("讀取SubjectDirectoryAttribute中屬性失敗:" + GetErrorMessage(m_error));
        return;
    }

    oid = hca.ATL_ExtractOIDFromDerCode(b64attr);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("判斷OID失敗:" + GetErrorMessage(m_error));
        return;
    }

    if ((oid.length = OID_HPC.length) && (oid.indexOf(OID_HPC) != -1)) {
        alert("此為醫事人員憑證");
    }
    else if ((oid.length = OID_HOP.length) && (oid.indexOf(OID_HOP) != -1)) {
        alert("此為醫事機構憑證");
    }
    else {
        alert("非HCA憑證");
    }
}

function SignWithoutHash(form) {
    if (checkModule() < 0) {
        return;
    }
    if (checkSession() < 0) {
        return;
    }

    var b64hash, b64sign, hPriKey = 0;

    hPriKey = hca.ATL_GetKeyObjectHandle(m_hModule, m_hSession, 0, "", "1");
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("取私鑰失敗:" + GetErrorMessage(m_error));
        return;
    }

    //測試原文:Test String 123
    //SHA1 Base64:2jmj7l5rSw0yVb/vlWAYkK/YBwk=
    //SHA256 Base64:UfAarftHcNP/HwmWf0kKofjv4XUNaxsukD69sxthLSI=
    b64hash = "2jmj7l5rSw0yVb/vlWAYkK/YBwk=";
    //b64hash = "UfAarftHcNP/HwmWf0kKofjv4XUNaxsukD69sxthLSI=";

    b64sign = hca.ATL_SignWithoutHash(m_hModule, m_hSession, b64hash, hPriKey);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("簽章失敗:" + GetErrorMessage(m_error));
    }
    else {
        var err = hca.ATL_Util_WriteFileFromBase64(form.T29.value, b64sign);
        if (err != 0) {
            alert("寫入簽章檔失敗:" + err);
        }
        else {
            alert("簽章完成");
        }
    }

    if (hPriKey != 0) {
        hca.ATL_DeleteKeyObject(m_hModule, m_hSession, hPriKey);
    }

}

function ChangeUserPin(form) {

    var rtn;
    if (checkModule() < 0) {
        return;
    }

    if (checkSession() < 0) {
        return;
    }

    if (checkNotBlank(form.T3) < 0) {
        alert("請輸入新PIN碼")
        return;
    }

    rtn = hca.ATL_ChangeUserPIN(m_hModule, m_hSession, form.T3.value);
    if ((m_error = hca.ATL_GetErrorCode()) != 0) {
        alert("修改失敗:" + GetErrorMessage(m_error));
    }
    else {
        alert("修改成功");
    }
}

function Test(form) {

    var time1 = new Date();
    InitModule(form);
    InitSession(form);
    Sign(form);
    CloseSession(form);
    CloseModule(form);
    var time2 = new Date(); alert(time2.getTime() - time1.getTime());
}