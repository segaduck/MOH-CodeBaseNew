
var gDebug = 0;

/*
 *  CGCAIPATL 部分元件參數定義
 */
 	var CGCAPI_FLAG_READFILE_DELETE				=	0x00100000;

	var CGCAPI_FLAG_SELCERT_MEMORY				=	0x00400000;

	var CGCAPI_FLAG_EXPORTABLE					=	0x00000001;
	var CGCAPI_FLAG_MEMORY						=	0x00400000;

	var CGCAPI_STORE_CU_FILE					=	0x00400000;

	var CG_KU_NON_REPUDIATION					= 	0x0040;
	var CG_KU_DIGITAL_SIGNATURE					=	0x0080;
	var CG_KU_KEY_ENCIPHERMENT					=	0x0020;
	var CG_KU_DATA_ENCIPHERMENT					=	0x0010;

	var CG_FLAG_DATA_APPEND_NULL				=	0x00040000;
	var CG_FLAG_SUBJECT_PARTIALMATCH			=	0x00080000;
	var CGCAPI_FLAG_SELCERT_MANUAL				=	0x00000001;
	var CGCAPI_FLAG_SELCERT_AUTO				=	0x00000002;
	var CGCAPI_FLAG_SELCERT_SELFAUTO			=	0x00000003;
	var CGCAPI_FLAG_SELCERT_AFTER 				=	0x00000004;
	var CGCAPI_FLAG_SELCERT_OLDEST				= 	0x00000008;
	var CGCAPI_FLAG_SELCERT_CHECKVALID 			=	0x00000010;	
	
	var CGCAPI_FLAG_ENABLE_EMPTY_PASSWORD		=	0x00001000;
	var CGCAPI_FLAG_SELCERT_USEIE				=	0x00002000;
	var CGCAPI_FLAG_USE_CAPTCHA					=	0x00008000;
	
	var CG_FLAG_CERT_ATTACHALL					= 	0x00000200;	

	var CG_FLAG_VERIFY_CONTENT_ONLY				=	0x00000000;
	var CG_FLAG_VERIFY_CERTCHAIN				=	0x00000001;
	var CG_FLAG_VERIFY_CRL						=	0x00000002;
	var CG_FLAG_VERIFY_FULL						=	0x00000003;


	var CG_FLAG_DETACHMSG						=	0x00004000;

	var CG_FLAG_SUBJECT_COMMON_NAME				=	0x00010000;
	var CG_FLAG_SUBJECT_RSA_EMAILADDR			=	0x00020000;
	var CG_FLAG_SUBJECT_ORGANIZATION_NAME 		= 0x00040000;
	var CG_FLAG_SUBJECT_ORGANIZATIONAL_UNIT_NAME = 0x00080000;
	var CG_FLAG_SUBJECT_COUNTRY_NAME			= 0x00100000;
	var CG_FLAG_SUBJECT_DEVICE_SERIAL_NUMBER	= 0x00200000;
	
	var CG_FLAG_DOHASH							= 0x00010000;
	var CG_FLAG_NOHASHOID						= 0x00020000;
	var CG_ALGOR_MD5							= 0x01;
	var CG_ALGOR_SHA1							= 0x02;
	var CG_ALGOR_SHA256							= 0x04;
	var CG_ALGOR_SHA384							= 0x05;
	var CG_ALGOR_SHA512							= 0x06;	
		
	var CG_FLAG_BASE64_ENCODE = 0x00001000;
	


function fnJSLanguageVer(sWordC, sWordE){
	return sWordC;
}



/*
 *  CGCAIPATL 部分元件錯誤代碼
 */
function GetErrorMessage(ErrCode) { //將錯誤代碼轉換為人看得懂的訊息
	var msg = "";

	var iErrCode = parseInt(ErrCode);
	if (isNaN(iErrCode)) { iErrCode = ErrCode; }

	switch(iErrCode){

		case 0:
			msg += fnJSLanguageVer('完成', 'Success ');
			break;
		case 13:
			msg += fnJSLanguageVer("[" + "5001" + '] 一般性錯誤，請確認讀卡機有接上，或是卡片重新拔插再試一次', "[" + "5001" + '] general error ');
			break;
		case 5001:
			msg += fnJSLanguageVer("[" + ErrCode + '] 一般性錯誤', "[" + ErrCode + '] general error ');
			break;
		case 5002:
			msg += fnJSLanguageVer("[" + ErrCode + '] 記憶體配置錯誤',"[" + ErrCode + '] Memory Allocation Error');
			break;
		case 5003:
			msg += fnJSLanguageVer("[" + ErrCode + '] Buffer too small',"[" + ErrCode + '] Buffer too small');
			break;
		case 5005:
			msg += fnJSLanguageVer("[" + ErrCode + '] 參數錯誤',"[" + ErrCode + '] Invalid parameter');
			break;
		case 5006:
			msg += fnJSLanguageVer("[" + ErrCode + '] Invalid handle',"[" + ErrCode + '] Invalid handle');
			break;
		case 5007:
			msg += fnJSLanguageVer("[" + ErrCode + '] 元件已過期',"[" + ErrCode + '] TrialVersion Library is expired');
			break;
		case 5008:
			msg += fnJSLanguageVer("[" + ErrCode + '] Base64 Encoding/Decoding Error',"[" + ErrCode + '] Base64 Encoding/Decoding Error');
			break;

		case 5010:
			msg += fnJSLanguageVer("[" + ErrCode + '] 找不到符合憑證',"[" + ErrCode + '] certificate not found');
			break;
		case 5011:
			msg += fnJSLanguageVer("[" + ErrCode + '] 憑證已過期',"[" + ErrCode + '] Certicate Expired');
			break;
		case 5012:
			msg += fnJSLanguageVer("[" + ErrCode + '] 憑證尚未有效',"[" + ErrCode + '] Certificate can not be used now');
			break;

		case 5014:
			msg += fnJSLanguageVer("[" + ErrCode + '] 憑證主旨比對錯誤',"[" + ErrCode + '] Certificate subject not match');
			break;

		case 5015:
			msg += fnJSLanguageVer("[" + ErrCode + '] 找不到憑證發行者',"[" + ErrCode + '] Unable to find certificate issuer');
			break;

		case 5016:
			msg += fnJSLanguageVer("[" + ErrCode + '] 憑證簽章值無效',"[" + ErrCode + '] Certificate signature is invalid');
			break;

		case 5017:
			msg += fnJSLanguageVer("[" + ErrCode + '] 錯誤的金鑰使用方式',"[" + ErrCode + '] Invalid ertificate keyusage');
			break;

		case 5020:
		case 5021:
		case 5022:
		case 5023:
		case 5024:
		case 5025:
		case 5026:
		case 5028:
			msg += fnJSLanguageVer("[" + ErrCode + '] 憑證已撤銷',"[" + ErrCode + '] Certificate is revoked');
			break;

		case 5030:
			msg += fnJSLanguageVer("[" + ErrCode + '] CRL 已過期',"[" + ErrCode + '] CRL expired.');
			break;
		case 5031:
			msg += fnJSLanguageVer("[" + ErrCode + '] CRL 尚未有效',"[" + ErrCode + '] CRL not yet valid.');
			break;
		case 5032:
			msg += fnJSLanguageVer("[" + ErrCode + '] 找不到 CRL ',"[" + ErrCode + '] CRL not found.');
			break;
		case 5034:
			msg += fnJSLanguageVer("[" + ErrCode + '] CRL 的簽章值錯誤 ',"[" + ErrCode + '] CRL signature invalid.');
			break;
		case 5036:
			msg += fnJSLanguageVer("[" + ErrCode + '] 資料的簽章值錯誤 ',"[" + ErrCode + '] Invalid data signature.');
			break;
		case 5037:
			msg += fnJSLanguageVer("[" + ErrCode + '] 簽章的原文錯誤 ',"[" + ErrCode + '] Content not match.');
			break;
		case 5038:
			msg += fnJSLanguageVer("[" + ErrCode + '] 圖形驗證碼錯誤 ',"[" + ErrCode + '] Incorrect captcha.');
			break;
		case 5040:
			msg += fnJSLanguageVer("[" + ErrCode + '] 錯誤的憑證格式 ',"[" + ErrCode + '] Incorrect Certificate format.');
			break;

		case 5041:
			msg += fnJSLanguageVer("[" + ErrCode + '] 錯誤的 CRL 格式 ',"[" + ErrCode + '] Incorrect CRL format.');
			break;

		case 5042:
			msg += fnJSLanguageVer("[" + ErrCode + '] 錯誤的 PKCS#7 格式 ',"[" + ErrCode + '] Incorrect PKCS7 format.');
			break;

		case 5050:
			msg += fnJSLanguageVer("[" + ErrCode + '] 找不到指定物件 ',"[" + ErrCode + '] CG_RTN_OBJ_NOT_FOUND.');
			break;
		case 5071:
			msg += fnJSLanguageVer("[" + ErrCode + '] 密碼不正確 ',"[" + ErrCode + '] CG_RTN_PASSWD_INVALID.');
			break;

		case 5204:
			msg += fnJSLanguageVer("[" + ErrCode + '] 找不到私密金鑰 ',"[" + ErrCode + '] CG_RTN_OBJ_NOT_FOUND.');
			break;
		case 5205:
			msg += fnJSLanguageVer("[" + ErrCode + '] 憑證無法匯出 ',"[" + ErrCode + '] CGCAPI_RTN_UNEXPORTABLE.');
			break;
		case 5206:
			msg += fnJSLanguageVer("[" + ErrCode + '] 權限不足 ',"[" + ErrCode + '] CGCAPI_RTN_STORE_ACCESSDENY.');
			break;
		case 5902:
			msg += fnJSLanguageVer("[" + ErrCode + '] 找不到檔案 ',"[" + ErrCode + '] CG_RTN_FILE_NOT_FOUND.');
			break;
		case 5906:
			msg += fnJSLanguageVer("[" + ErrCode + '] 沒有權限存取 ',"[" + ErrCode + '] CG_RTN_ACCESS_DENIED.');
			break;

		// PKCS#11 return code
		case 9005:
			msg += fnJSLanguageVer("[" + ErrCode + '] 此 PKCS#11 不支援此 Function ',"[" + ErrCode + '] CGP11_RTN_OBJECT_NOT_EXIST.');
			break;
		case 9006:
			msg += fnJSLanguageVer("[" + ErrCode + '] PKCS#11 參數錯誤 ',"[" + ErrCode + '] CGP11_RTN_ARGUMENTS_BAD.');
			break;
		case 9039:
		case 9040:
			msg += fnJSLanguageVer("[" + ErrCode + '] PKCS#11 Pin 碼錯誤 ',"[" + ErrCode + '] CGP11_RTN_PIN_INCORRECT.');
			break;
		case 9043:
			msg += fnJSLanguageVer("[" + ErrCode + '] PKCS#11 Pin Lock ',"[" + ErrCode + '] CGP11_RTN_PIN_INCORRECT.');
			break;


		case 9100:
			msg += fnJSLanguageVer("[" + ErrCode + '] 物件不存在 ',"[" + ErrCode + '] CGP11_RTN_OBJECT_NOT_EXIST.');
			break;

		case 9100:
			msg += fnJSLanguageVer("[" + ErrCode + '] 物件不存在 ',"[" + ErrCode + '] CGP11_RTN_OBJECT_NOT_EXIST.');
			break;
		case 9101:
			msg += fnJSLanguageVer("[" + ErrCode + '] 物件已存在 ',"[" + ErrCode + '] CGP11_RTN_OBJECT_EXIST.');
			break;
		case 9102:
			msg += fnJSLanguageVer("[" + ErrCode + '] 物件發生問題(可能是因為一個以上) ',"[" + ErrCode + '] CGP11_RTN_OBJECT_HAS_PROBLEM.');
			break;

		case 9110:
		case 9111:
			msg += fnJSLanguageVer("[" + ErrCode + '] Load Library 失敗 ',"[" + ErrCode + '] CGP11_RTN_LIBRARY_NOT_LOAD.');
			break;

		case 9112:
			msg += fnJSLanguageVer("[" + ErrCode + '] 找不到 slot ',"[" + ErrCode + '] CGP11_RTN_SLOT_NOT_FOUND.');
			break;
			
		case 61001:
			msg += fnJSLanguageVer("[" + ErrCode + '] 一般性錯誤，ServiSign主程式-未安裝完成，請重新安裝試試看.', "[" + ErrCode + '] General Error, ');
			break;
		case 61003:
			msg += fnJSLanguageVer("[" + ErrCode + '] 字元轉換錯誤，請洽服務窗口協調廠商調整程式.', "[" + ErrCode + '] Library atoi transfer Error. ');
			break;	
		case 61004:
			msg += fnJSLanguageVer("[" + ErrCode + '] 字元轉換錯誤 字元為空，請洽服務窗口協調廠商調整程式',"[" + ErrCode + '] Library atoi transfer Error, character is null. ');
			break;	
		case 61005:
			msg += fnJSLanguageVer("[" + ErrCode + '] 參數錯誤，請洽工程師',"[" + ErrCode + '] Paramater Error ');
			break;
		case 61006:
			msg += fnJSLanguageVer("[" + ErrCode + '] 請檢查Changingtec憑證是否匯入憑證庫成功',"[" + ErrCode + '] Please Check Changingtec CA Certificate in your CertStore. ');
			break;			
		case 61007:
			msg += fnJSLanguageVer("[" + ErrCode + '] 元件已過期', "[" + ErrCode + '] Component was Expired. ');
			break;			
		case 61008:
			msg += fnJSLanguageVer("[" + ErrCode + '] SSL連線請使用TLS1.1以上連線方式',"[" + ErrCode + '] Please Check your SSL Protocol is higher than TLS 1.1. ');
			break;		
		case 61201:
			msg += fnJSLanguageVer("[" + ErrCode + '] 元件版本格式錯誤',"[" + ErrCode + '] Library version format Error. ');
			break;	
		case 61202:
			msg += fnJSLanguageVer("[" + ErrCode + '] ServiSign Adapter程式版本錯誤，請洽工程師',"[" + ErrCode + '] JavaScript Adapter version Error. ');
			break;		
		case 61203:
			msg += fnJSLanguageVer("[" + ErrCode + '] ServiSign Adapter所述版本錯誤，請洽工程師',"[" + ErrCode + '] ServiSign JS written version Error. ');
			break;		
		case 61204:
			msg += fnJSLanguageVer("[" + ErrCode + '] ServiSign Library版本錯誤，請洽工程師',"[" + ErrCode + '] ServiSign library version Error. ');
			break;				
		case 61902:
			msg += fnJSLanguageVer("[" + ErrCode + '] 找不到對應的LibName，請洽工程師',"[" + ErrCode + '] LibName Not Found. ');
			break;					
		case 61904:
			msg += fnJSLanguageVer("[" + ErrCode + '] 錯誤的Service路徑，不是打到Localhost',"[" + ErrCode + '] Bad Net Path, Service is not from localhost. ');
			break;			
		case 61905:
			msg += fnJSLanguageVer("[" + ErrCode + '] Service初始化失敗，請重新啟動電腦。',"[" + ErrCode + '] ServiSign initialize fail, Please restart your PC. ');
			break;	
		case 61906:
			msg += fnJSLanguageVer("[" + ErrCode + '] 元件無法存取。',"[" + ErrCode + '] ServiSign Access Denied. ');
			break;	
		case 61908:
			msg += fnJSLanguageVer("[" + ErrCode + '] 元件尚未認證。',"[" + ErrCode + '] Component was not Authorized. ');
			break;	



		default:
			msg += fnJSLanguageVer('其他錯誤，請參考元件手冊: (',  'Unknown Error, please reference document: (') + ErrCode + ") " ;
		  	return msg;

	}

	return msg;
}
/*
if (navigator.platform != "Win32" && navigator.platform !="Win64") {
	alert("Only Windows System is supported");
}
*/
var isInstalled = true;
function installActiveXError(){
	isInstalled = false;
<!--
//    document.faqax.submit();
//-->

	
}

