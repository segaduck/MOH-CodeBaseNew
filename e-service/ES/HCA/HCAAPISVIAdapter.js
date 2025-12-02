
// Generate at 2020-03-10 13:29:41
var ServiSignErrorCode = 61001;

String.prototype.replaceAll = function(search, replacement) {
    var target = this;
    return target.split(search).join(replacement);
};

function getHCAAPISVIAdapterObj() {
	
	//如有任何參數問題，請參考第 10, 11點
	//https://redmine.changingtec.com/redmine/projects/servisign/wiki/%E5%95%8F%E9%A1%8C%E8%88%87%E8%A7%A3%E7%AD%94

	//最低 ServiSign 版本 0.0.0.0 = 預設不開啟
	var minServiSignVersion = "0.0.0.0"
	//最低元件版本 0.0.0.0 = 預設不開啟
	var minLibVersion = "0.0.0.0"

	//底層元件 Adapter 名稱
	var LibName = ""
	//底層元件 Adapter 所在位置
	var LibDir = ""
	//Path ID
	var PathID = "6BD4EA2493354E298DD0B09B72A5EA59"

	//最低不需更新安裝包版本
	var MinimalInstallerVersion = "1.0.20.0310"
	var MinimalInstallerVersion_Mac = ""
	var MinimalInstallerVersion_Linux = ""
	//JS 模板版本號 ServiSign 主程式會做相容性版本確認
	var JSVersion = "1.0.18.1219"

	//Tab 以分頁做為轉換模式，行為模式與 ActiveX 元件相同
	var ServiSignTabMode_Tab = 			0x0
	//Browser 模式則是以瀏覽器為單位，整個瀏覽器關掉才會釋放元件
	var ServiSignTabMode_Browser = 		0x1

	var ServiSignTabMode = ServiSignTabMode_Tab

	//預設不使用 cookie 儲存 Try port 的結果
	var useCookieTryPort = false
	
	//路徑保護
	//DataObj.pfxpath_servisignflag = true
    //DataObj.pfxpath = pfxpath.replace(/\\/g, "*")
	
	//開啟元件 UI 置頂功能
	//DataObj.topuidetect = true
	//開啟全系統 UI 置頂功能
	//DataObj.topuialldetect = true
	//開啟非同步功能
	//DataObj.asynchronously = true
	
	//資料斜線保護功能
	//DataObj.XXXXX_ServiSignSlashFlag = true
	//DataObj.XXXXX = inputXXXXXX.replaceAll("\\", "==ServiSignSlash==")
	//資料 Base64 保護功能
	//DataObj.XXXXX_ServiSignBase64Flag = true
	//DataObj.XXXXX = Base64Encode(XXXXX)
	//使用主線程呼叫 API
	//DataObj.ServiSignMainThread = true
	//變數保護功能
	//目前已經針對 pin, pw 跟 pass 等關鍵字顯示時自動遮蔽內容
	//如果想要額外遮蔽可在此調整:D
	//DataObj.XXXXX_ServiSignHide = true
	//回傳時可以隱藏 log  
	//DataObj.hide_result = true

	
	function ServiSignConnectError() {
		//ServiSign_RTN_CONNECTION_ERROR
		ServiSignErrorCode = 61006
	}
	function ServiSignLoadComponentError() {
		//Handing load component error
    }
	function ServiSignDisconnectError() {
		//Handing disconnect error
		ServiSignErrorCode = 61015
    }
    
    var VersionCompare_Error = 			0x00
	var VersionCompare_Bigger =			0x01
	var VersionCompare_Smaller = 		0x02
	var VersionCompare_Same = 			0x03

	function ServiSignLog(LogMessage) {
		console.log("[ServiSign Log] " + LogMessage)
	}
	function Sleep(milliseconds) {
		var start = new Date().getTime()
		for (var i = 0; i < 1e7; i++) {
			if ((new Date().getTime() - start) > milliseconds){
				break
			}
		}
	}
	function Base64Encode(input) {
		return encodeURIComponent(window.btoa(unescape(encodeURIComponent(input))))
	}
	function Base64Decode(input) {
		if(input == undefined) return ""
		return decodeURIComponent(escape(window.atob(input)))
	}
	function FixArray(inputArray) {
		ServiSignLog("FixArray: " + inputArray)
		var array = inputArray.split(";")

		// while(array.indexOf("") != -1){
		// 	var EmptyIndex = array.indexOf("")
		// 	array.splice(EmptyIndex, 1)
		// }
		
		array.toArray = function(){
			return this
		}
		array.splice(-1, 1);
		return array
	}
	function KeepAlive(DataObj) {
		var temp = ""
		DataObj.KeepAlive = true
		DataObj.topuidetect = true
		DataObj.ServiSignMainThread = true
		do{
			temp = ServiSignObj.Send(DataObj)
		}while(temp == "heartbeat")
		return temp
	}
	function isServiSignErrorCode(input) {
		var ErrorCode = parseInt(input) || 0
		return (61000 < ErrorCode && ErrorCode < 61999)
	}
	function getCookie(CookieName) {
		var TargetName = CookieName + "="
		var CookieArray = document.cookie.split(';')
		for (var i = 0; i < CookieArray.length; i++) {
			var CookieElement = CookieArray[i]
			while (CookieElement.charAt(0) == ' ') {
				CookieElement = CookieElement.substring(1)
			}
			if (CookieElement.indexOf(TargetName) == 0) {
				return CookieElement.substring(TargetName.length, CookieElement.length)
			}
		}
		return ""
	}
	function VersionCompare(VersionA, VersionB) {
		var iVersionA = parseInt(VersionA.replaceAll(",","").replaceAll(".","").replaceAll(" ","")) || 0
		var iVersionB = parseInt(VersionB.replaceAll(",","").replaceAll(".","").replaceAll(" ","")) || 0
		
		if(iVersionA == 0 || iVersionB == 0) return VersionCompare_Error
		if(iVersionA > iVersionB) return VersionCompare_Bigger
		if(iVersionA < iVersionB) return VersionCompare_Smaller
		return VersionCompare_Same
	}
	function BrowserDetection() {
		var sBrowser, sUsrAg = navigator.userAgent

		if(sUsrAg.indexOf("Edge") > -1){
			sBrowser = "Edge"
		} else if(sUsrAg.indexOf("Chrome") > -1) {
			sBrowser = "Chrome"
		} else if (sUsrAg.indexOf("Safari") > -1) {
			sBrowser = "Safari"
		} else if (sUsrAg.indexOf("Opera") > -1) {
			sBrowser = "Opera"
		} else if (sUsrAg.indexOf("Firefox") > -1) {
			sBrowser = "Firefox"
		} else if (sUsrAg.indexOf("MSIE") > -1 || sUsrAg.indexOf("Trident/7.0") > -1) {
			sBrowser = "Internet Explorer"
		} else {
			sBrowser = "unknown"
		}
		
		return sBrowser
	}
	function detectOS() {
		// https://stackoverflow.com/questions/38241480/detect-macos-ios-windows-android-and-linux-os-with-js
		var userAgent = window.navigator.userAgent,
			platform = window.navigator.platform,
			macosPlatforms = ['Macintosh', 'MacIntel', 'MacPPC', 'Mac68K'],
			windowsPlatforms = ['Win32', 'Win64', 'Windows', 'WinCE'],
			os = null;
	  
		if (macosPlatforms.indexOf(platform) !== -1) {
		  	os = 'Mac'
		}
		else if (windowsPlatforms.indexOf(platform) !== -1) {
		  	os = 'Windows'
		}
		else if (!os && /Linux/.test(platform)) {
		  	os = 'Linux'
		}
	  
		return os
	}
	function verifyURL(input){
		
		if(input.indexOf("alert(") >= 0){
			return ""
		}

		if(input.indexOf('https://localhost:') != 0 && input.indexOf('https://127.0.0.1:') != 0){
			return ""
		}

		return encodeURI(input)
	}
	function getServiSignObj() {
		var portList = [56445, 56545, 56645]
        var InstallerName = "HCAServiSignAdapterSetup"
		
		var Url_Part1_DNS = 'https://localhost:'
		var Url_Part1_IP = 'https://127.0.0.1:'
		var SessionID =""
		var ServiSignUrl = ""

		var realServiSignVersion = ""
		var realLibVersion = ""
		var realInstallerVersion = ""
		var CurrenOSMinimalInstallerVersion = null
		
		var needUpdate = false
		var Browser = ""
		var OS = null

		var ServiSignObjResultObj = undefined

		var CallbackFunction = []
		var CallbackFunctionIndex = 0
		var isCloseAsynchronously = false
		
		var ServiSignObj = 
		{
			clearServiSignCallback : function(){
				ServiSignLog("CallbackFunction empty")
				CallbackFunction = []
				CallbackFunctionIndex = 0
			},
			setServiSignCallback : function(InputFunction){
				if(typeof InputFunction != "function"){
					CallbackFunction.splice(CallbackFunctionIndex + 1, 0, function(){return ;})
				}
				else{
					CallbackFunction.splice(CallbackFunctionIndex + 1, 0, InputFunction)
				}
			},
			sendData : function(url, DataObj){
				var XMLHttpRequestAsynchronously = (CallbackFunction.length != 0)
				var xhr = new XMLHttpRequest()
				
				try{
					if(verifyURL(url) == ""){
						return undefined
					}
					xhr.open('MagicMethodA|POST|MagicMethodB'.split('|')[1], verifyURL(url), XMLHttpRequestAsynchronously)
				}
				catch(err) {
					return undefined
				}
				xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded")
				var onLoadHandler = function(event){
					try {
						ServiSignObjResultObj = JSON.parse(this.responseText)
					}
					catch(err) {
						return undefined
					}

					if(XMLHttpRequestAsynchronously){
						CallbackFunction[CallbackFunctionIndex++](Base64Decode(ServiSignObjResultObj.result), this.ServiSignUrl);
						
						if(CallbackFunctionIndex == CallbackFunction.length){
							ServiSignObj.clearServiSignCallback();
						}
					}
				}
				xhr.onload = onLoadHandler

				DataObj.comname = LibName
				DataObj.libdir = LibDir
				if(ServiSignTabMode == ServiSignTabMode_Tab){
					DataObj.sessionid = SessionID
				}
				else if(ServiSignTabMode == ServiSignTabMode_Browser){
					DataObj.sessionid = Browser
				}
				DataObj.ServiSignTabMode = ServiSignTabMode
				DataObj.ServiSignBrowser = Browser
				DataObj.JSVersion = JSVersion
				DataObj.minlibversion = minLibVersion
				DataObj.minserverversion = minServiSignVersion
				DataObj.InstallerName = InstallerName
				DataObj.PathID = PathID
				DataObj.ServiSignJSGenTime = "2020-03-10 13:29:41"
				
				// For new version form 20190416
				DataObj.AdapterJsonKeyCheck = true				

				var readyDataObj = "Parameter=" + Base64Encode(JSON.stringify(DataObj))
				
				try {
					xhr.send(readyDataObj)
				}
				catch(err) {
					var header = xhr.getResponseHeader("via")
					if (header) {
						ServiSignErrorCode = 61014
					}
					else{
						ServiSignDisconnectError()
					}
					return undefined
				}
				return ServiSignObjResultObj
			},
			TryPort : function() {
				var resultObj
				var DataObj = new Object()
				var ErrorCode = 0

				OS = detectOS()
				Browser = BrowserDetection()

				ServiSignLog(OS)
				ServiSignLog(Browser)

				// DataObj.functionname = "ServiSignEcho"
				DataObj.functionname = "Echo"
				// if(useCookieTryPort){
				// 	ServiSignUrl = getCookie("ServiSignUrl")
				// }
				// else{
				// 	ServiSignUrl = ""
				// }
				ServiSignUrl = ""

				if(ServiSignUrl == "" || ServiSignUrl == "fail") {
					var EchoSuccess = false
					for (var i = 0; i < portList.length; i++) {
						var url
						if(OS == "Mac"){
							url = Url_Part1_DNS + (portList[i] - 2000)
						}
						else{
							url = Url_Part1_DNS + portList[i]
						}

						ServiSignLog("Echo URL: " + url)

						resultObj = this.sendData(url, DataObj)
						if(resultObj != undefined){
							
							ErrorCode = Base64Decode(resultObj.result)
				
							if(ErrorCode != "" && ErrorCode != "0") {
								ServiSignLog("Error code: " + ErrorCode)
								ServiSignErrorCode = parseInt(ErrorCode)
								continue
							}
							ServiSignUrl = url
							EchoSuccess = true
							break
						}
						if(ServiSignErrorCode == 61014){
							// 使用了 Proxy
							// if(useCookieTryPort){
							// 	document.cookie = "ServiSignUrl=fail"
							// }
							ServiSignLog("Using proxy")
							return false
						}
					}
					if(!EchoSuccess){
						for (var i = 0; i < portList.length; i++) {
							var url
							if(OS == "Mac"){
								url = Url_Part1_IP + (portList[i] - 2000)
							}
							else{
								url = Url_Part1_IP + portList[i]
							}

							ServiSignLog("Echo URL: " + url)

							resultObj = this.sendData(url, DataObj)
							if(resultObj != undefined){
								
								ErrorCode = Base64Decode(resultObj.result)
					
								if(ErrorCode != "" && ErrorCode != "0") {
									ServiSignLog("Error code: " + ErrorCode)
									continue
								}
								ServiSignUrl = url
								break
							}
							if(ServiSignErrorCode == 61014){
								// 使用了 Proxy
								// if(useCookieTryPort){
								// 	document.cookie = "ServiSignUrl=fail"
								// }
								ServiSignLog("Using proxy")
								return false
							}
						}
					}
				}
				else{
					ServiSignLog("Echo URL: " + url)
					resultObj = this.sendData(ServiSignUrl, DataObj)
				}

				if(resultObj == undefined){
					// if(useCookieTryPort){
					// 	document.cookie = "ServiSignUrl=fail"
					// }
					ServiSignConnectError()
					return false
				}
				
				ServiSignErrorCode = parseInt(ErrorCode) || 0
				if(ServiSignErrorCode == 0){
					// if(useCookieTryPort){
					// 	document.cookie = "ServiSignUrl=" + ServiSignUrl
					// }
				}
				else{
					// if(useCookieTryPort){
					// 	document.cookie = "ServiSignUrl=fail"
					// }
					return false
				}
				
				realServiSignVersion = Base64Decode(resultObj.ServerVersion)
				realLibVersion = Base64Decode(resultObj.LibVersion)
				realInstallerVersion = Base64Decode(resultObj.InstallerVersion)
				
				if(SessionID == "") {
					SessionID = Base64Decode(resultObj.SessionID)
				}

				if(OS == "Windows"){
					CurrenOSMinimalInstallerVersion = MinimalInstallerVersion
				}
				else if(OS == "Mac"){
					CurrenOSMinimalInstallerVersion = MinimalInstallerVersion_Mac
				}
				else if(OS == "Linux"){
					CurrenOSMinimalInstallerVersion = MinimalInstallerVersion_Linux
				}
				
				needUpdate = (VersionCompare(CurrenOSMinimalInstallerVersion, realInstallerVersion) == VersionCompare_Bigger || VersionCompare(CurrenOSMinimalInstallerVersion, realInstallerVersion) == VersionCompare_Error)
				
				ServiSignErrorCode = 0
				return true
			},
			Send : function(DataObj) {
				DataObj.comname = LibName
				var resultObj = this.sendData(ServiSignUrl, DataObj)
				try {
					return Base64Decode(resultObj.result)
				}
				catch(err) {
					return ""
				}
			},
			getCGServiSignVersion : function() {
				return realServiSignVersion
			},
			getLibVersion : function() {
				return realLibVersion
			},
			getInstallerVersion : function() {
				return realInstallerVersion
			},
			getMinimalInstallerVersion : function() {
				return CurrenOSMinimalInstallerVersion
			},
			ServiSigninit : function() {
				var XMLHttpRequestSupported = typeof new XMLHttpRequest().responseType === 'string'
				
				if(!XMLHttpRequestSupported) {
					alert("This Browser does NOT support XMLHttpRequest")
					return false
				}
				return this.TryPort()
			},
			ServiSignRelease : function() {
				if(ServiSignErrorCode != 0){
					return
				}

				var empty_func = function() { return undefined; };
				this.setServiSignCallback(empty_func)

				var DataObj = new Object()
				DataObj.functionname = "ServiSignRelease"
				DataObj.minlibversion = minLibVersion
				this.Send(DataObj)

				realServiSignVersion = ""
				realLibVersion = ""
				realInstallerVersion = ""
				CurrenOSMinimalInstallerVersion = null
				
				SessionID = ""
				needUpdate = false
				ServiSignErrorCode = 0
			},
			ServiSignForceRelease : function() {
				var DataObj = new Object()
				DataObj.functionname = "ServiSignForceRelease"
				DataObj.minlibversion = minLibVersion
				this.Send(DataObj)

				realServiSignVersion = ""
				realLibVersion = ""
				realInstallerVersion = ""
				CurrenOSMinimalInstallerVersion = null
				
				SessionID = ""
				needUpdate = false
				ServiSignErrorCode = 0
			},
			needUpdateInstaller : function() {
				return needUpdate
			},
			setServiSignValue : function(channel, domain_list, timeout, value){
				var DataObj = new Object()
				DataObj.functionname = "setServiSignValue"
				DataObj.channel = channel
				DataObj.domain_list = domain_list
				DataObj.timeout = timeout
				DataObj.value = value
				DataObj.value_ServiSignHide = true
				return this.Send(DataObj)
			},
			getServiSignValue : function(channel){
				var DataObj = new Object()
				DataObj.functionname = "getServiSignValue"
				DataObj.channel = channel
				DataObj.hide_result = true
				return this.Send(DataObj)
			}
		}
		return ServiSignObj
	}
	var ServiSignObj = getServiSignObj()
	var ServiSignInterface = 
	{
		GetServiSignVersion : function() {
			return ServiSignObj.getCGServiSignVersion()
		},
		GetLibVersion : function() {
			return ServiSignObj.getLibVersion()
		},
		GetInstallerVersion : function() {
			return ServiSignObj.getInstallerVersion()
		},
		GetMinimalInstallerVersion : function() {
			return ServiSignObj.getMinimalInstallerVersion()
		},
		SetServiSignCallback : function(InputFunction) {
			ServiSignObj.setServiSignCallback(InputFunction)
		},
		NeedUpdateInstaller : function() {
			return ServiSignObj.needUpdateInstaller()
		},
		ServiSignForceRelease : function() {
			return ServiSignObj.ServiSignForceRelease()
		},
		SetServiSignValue : function(channel, domain_list, timeout, value){
			return ServiSignObj.setServiSignValue(channel, domain_list, timeout, value)
		},
		GetServiSignValue : function(channel){
			return ServiSignObj.getServiSignValue(channel)
		},
		GetFakeErrorCode : function(){
			return 0;
		},
		ATL_InitModule : function(moduleName, initArgs){
			var DataObj = new Object()
			DataObj.functionname = "ATL_InitModule"
			DataObj.moduleName_ServiSignBase64Flag = true
			DataObj.moduleName = Base64Encode(moduleName)
			DataObj.initArgs_ServiSignBase64Flag = true
			DataObj.initArgs = Base64Encode(initArgs)
			DataObj.ServiSignFunctionIndex = 0

			return ServiSignObj.Send(DataObj)
		},
		ATL_CloseModule : function(ulModuleHandle){
			var DataObj = new Object()
			DataObj.functionname = "ATL_CloseModule"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ServiSignFunctionIndex = 1

			return ServiSignObj.Send(DataObj)
		},
		ATL_InitSession : function(ulModuleHandle, iFlags, userPin){
			var DataObj = new Object()
			DataObj.functionname = "ATL_InitSession"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.iFlags = iFlags
			DataObj.userPin_ServiSignBase64Flag = true
			DataObj.userPin = Base64Encode(userPin)
			DataObj.ServiSignFunctionIndex = 2

			return ServiSignObj.Send(DataObj)
		},
		ATL_CloseSession : function(ulModuleHandle, ulSessionHandle){
			var DataObj = new Object()
			DataObj.functionname = "ATL_CloseSession"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ulSessionHandle = ulSessionHandle
			DataObj.ServiSignFunctionIndex = 3

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetCardHCACardVersion : function(ulModuleHandle){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetCardHCACardVersion"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ServiSignFunctionIndex = 4

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetKeyObjectHandle : function(ulModuleHandle, ulSessionHandle, iKeyType, key_id, param){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetKeyObjectHandle"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ulSessionHandle = ulSessionHandle
			DataObj.iKeyType = iKeyType
			DataObj.key_id_ServiSignBase64Flag = true
			DataObj.key_id = Base64Encode(key_id)
			DataObj.param_ServiSignBase64Flag = true
			DataObj.param = Base64Encode(param)
			DataObj.ServiSignFunctionIndex = 5

			return ServiSignObj.Send(DataObj)
		},
		ATL_DeleteKeyObject : function(ulModuleHandle, ulSessionHandle, ulKeyObjectHandle){
			var DataObj = new Object()
			DataObj.functionname = "ATL_DeleteKeyObject"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ulSessionHandle = ulSessionHandle
			DataObj.ulKeyObjectHandle = ulKeyObjectHandle
			DataObj.ServiSignFunctionIndex = 6

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetCertificateFromGPKICard : function(ulModuleHandle, ulSessionHandle, iCertId, readerName){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetCertificateFromGPKICard"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ulSessionHandle = ulSessionHandle
			DataObj.iCertId = iCertId
			DataObj.readerName_ServiSignBase64Flag = true
			DataObj.readerName = Base64Encode(readerName)
			DataObj.ServiSignFunctionIndex = 7

			return ServiSignObj.Send(DataObj)
		},
		ATL_HashFunction : function(ulModuleHandle, ulSessionHandle, ulAlgorithm, b64Data){
			var DataObj = new Object()
			DataObj.functionname = "ATL_HashFunction"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ulSessionHandle = ulSessionHandle
			DataObj.ulAlgorithm = ulAlgorithm
			DataObj.b64Data_ServiSignBase64Flag = true
			DataObj.b64Data = Base64Encode(b64Data)
			DataObj.ServiSignFunctionIndex = 8

			return ServiSignObj.Send(DataObj)
		},
		ATL_MakeSignature : function(ulModuleHandle, ulSessionHandle, ulAlgorithm, b64Data, ulPvKeyObject){
			var DataObj = new Object()
			DataObj.functionname = "ATL_MakeSignature"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ulSessionHandle = ulSessionHandle
			DataObj.ulAlgorithm = ulAlgorithm
			DataObj.b64Data_ServiSignBase64Flag = true
			DataObj.b64Data = Base64Encode(b64Data)
			DataObj.ulPvKeyObject = ulPvKeyObject
			DataObj.ServiSignFunctionIndex = 9

			return ServiSignObj.Send(DataObj)
		},
		ATL_VerifySignature : function(ulModuleHandle, ulSessionHandle, ulAlgorithm, b64Data, ulPubKeyObject, b64Sig){
			var DataObj = new Object()
			DataObj.functionname = "ATL_VerifySignature"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ulSessionHandle = ulSessionHandle
			DataObj.ulAlgorithm = ulAlgorithm
			DataObj.b64Data_ServiSignBase64Flag = true
			DataObj.b64Data = Base64Encode(b64Data)
			DataObj.ulPubKeyObject = ulPubKeyObject
			DataObj.b64Sig_ServiSignBase64Flag = true
			DataObj.b64Sig = Base64Encode(b64Sig)
			DataObj.ServiSignFunctionIndex = 10

			return ServiSignObj.Send(DataObj)
		},
		ATL_VerifySignatureByCert : function(b64Cert, ulAlgorithm, b64Data, b64Sig){
			var DataObj = new Object()
			DataObj.functionname = "ATL_VerifySignatureByCert"
			DataObj.b64Cert_ServiSignBase64Flag = true
			DataObj.b64Cert = Base64Encode(b64Cert)
			DataObj.ulAlgorithm = ulAlgorithm
			DataObj.b64Data_ServiSignBase64Flag = true
			DataObj.b64Data = Base64Encode(b64Data)
			DataObj.b64Sig_ServiSignBase64Flag = true
			DataObj.b64Sig = Base64Encode(b64Sig)
			DataObj.ServiSignFunctionIndex = 11

			return ServiSignObj.Send(DataObj)
		},
		ATL_PublicKeyEncryption : function(ulModuleHandle, ulSessionHandle, ulAlgorithm, b64Data, ulPubKeyObject){
			var DataObj = new Object()
			DataObj.functionname = "ATL_PublicKeyEncryption"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ulSessionHandle = ulSessionHandle
			DataObj.ulAlgorithm = ulAlgorithm
			DataObj.b64Data_ServiSignBase64Flag = true
			DataObj.b64Data = Base64Encode(b64Data)
			DataObj.ulPubKeyObject = ulPubKeyObject
			DataObj.ServiSignFunctionIndex = 12

			return ServiSignObj.Send(DataObj)
		},
		ATL_PublicKeyEncryptionByCert : function(b64Cert, ulAlgorithm, b64Data){
			var DataObj = new Object()
			DataObj.functionname = "ATL_PublicKeyEncryptionByCert"
			DataObj.b64Cert_ServiSignBase64Flag = true
			DataObj.b64Cert = Base64Encode(b64Cert)
			DataObj.ulAlgorithm = ulAlgorithm
			DataObj.b64Data_ServiSignBase64Flag = true
			DataObj.b64Data = Base64Encode(b64Data)
			DataObj.ServiSignFunctionIndex = 13

			return ServiSignObj.Send(DataObj)
		},
		ATL_PrivateKeyDecryption : function(ulModuleHandle, ulSessionHandle, ulAlgorithm, b64Cipher, ulPvKeyObject){
			var DataObj = new Object()
			DataObj.functionname = "ATL_PrivateKeyDecryption"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ulSessionHandle = ulSessionHandle
			DataObj.ulAlgorithm = ulAlgorithm
			DataObj.b64Cipher_ServiSignBase64Flag = true
			DataObj.b64Cipher = Base64Encode(b64Cipher)
			DataObj.ulPvKeyObject = ulPvKeyObject
			DataObj.ServiSignFunctionIndex = 14

			return ServiSignObj.Send(DataObj)
		},
		ATL_GenerateSessionKey : function(length){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GenerateSessionKey"
			DataObj.length = length
			DataObj.ServiSignFunctionIndex = 15

			return ServiSignObj.Send(DataObj)
		},
		ATL_SymEncryptionAlg : function(ulModuleHandle, ulSessionHandle, ulAlgorithm, b64Data, hexIv, ulSsKeyObject){
			var DataObj = new Object()
			DataObj.functionname = "ATL_SymEncryptionAlg"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ulSessionHandle = ulSessionHandle
			DataObj.ulAlgorithm = ulAlgorithm
			DataObj.b64Data_ServiSignBase64Flag = true
			DataObj.b64Data = Base64Encode(b64Data)
			DataObj.hexIv_ServiSignBase64Flag = true
			DataObj.hexIv = Base64Encode(hexIv)
			DataObj.ulSsKeyObject = ulSsKeyObject
			DataObj.ServiSignFunctionIndex = 16

			return ServiSignObj.Send(DataObj)
		},
		ATL_SymDecryptionAlg : function(ulModuleHandle, ulSessionHandle, ulAlgorithm, b64Cipher, hexIv, ulSsKeyObject){
			var DataObj = new Object()
			DataObj.functionname = "ATL_SymDecryptionAlg"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ulSessionHandle = ulSessionHandle
			DataObj.ulAlgorithm = ulAlgorithm
			DataObj.b64Cipher_ServiSignBase64Flag = true
			DataObj.b64Cipher = Base64Encode(b64Cipher)
			DataObj.hexIv_ServiSignBase64Flag = true
			DataObj.hexIv = Base64Encode(hexIv)
			DataObj.ulSsKeyObject = ulSsKeyObject
			DataObj.ServiSignFunctionIndex = 17

			return ServiSignObj.Send(DataObj)
		},
		ATL_VerifyCertSignature : function(cert, caCert){
			var DataObj = new Object()
			DataObj.functionname = "ATL_VerifyCertSignature"
			DataObj.cert_ServiSignBase64Flag = true
			DataObj.cert = Base64Encode(cert)
			DataObj.caCert_ServiSignBase64Flag = true
			DataObj.caCert = Base64Encode(caCert)
			DataObj.ServiSignFunctionIndex = 18

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetSerialNumber : function(b64Cert){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetSerialNumber"
			DataObj.b64Cert_ServiSignBase64Flag = true
			DataObj.b64Cert = Base64Encode(b64Cert)
			DataObj.ServiSignFunctionIndex = 19

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetCertIssuerDN : function(b64Cert){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetCertIssuerDN"
			DataObj.b64Cert_ServiSignBase64Flag = true
			DataObj.b64Cert = Base64Encode(b64Cert)
			DataObj.ServiSignFunctionIndex = 20

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetCertSubjectDN : function(b64Cert){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetCertSubjectDN"
			DataObj.b64Cert_ServiSignBase64Flag = true
			DataObj.b64Cert = Base64Encode(b64Cert)
			DataObj.ServiSignFunctionIndex = 21

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetLDAPTypeCertIssuerDN : function(b64Cert){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetLDAPTypeCertIssuerDN"
			DataObj.b64Cert_ServiSignBase64Flag = true
			DataObj.b64Cert = Base64Encode(b64Cert)
			DataObj.ServiSignFunctionIndex = 22

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetLDAPTypeCertSubjectDN : function(b64Cert){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetLDAPTypeCertSubjectDN"
			DataObj.b64Cert_ServiSignBase64Flag = true
			DataObj.b64Cert = Base64Encode(b64Cert)
			DataObj.ServiSignFunctionIndex = 23

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetRDNFromDN : function(dn, codePage, iFlags, rdn){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetRDNFromDN"
			DataObj.dn_ServiSignBase64Flag = true
			DataObj.dn = Base64Encode(dn)
			DataObj.codePage_ServiSignBase64Flag = true
			DataObj.codePage = Base64Encode(codePage)
			DataObj.iFlags = iFlags
			DataObj.rdn_ServiSignBase64Flag = true
			DataObj.rdn = Base64Encode(rdn)
			DataObj.ServiSignFunctionIndex = 24

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetCertNotBefore : function(b64Cert){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetCertNotBefore"
			DataObj.b64Cert_ServiSignBase64Flag = true
			DataObj.b64Cert = Base64Encode(b64Cert)
			DataObj.ServiSignFunctionIndex = 25

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetCertNotAfter : function(b64Cert){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetCertNotAfter"
			DataObj.b64Cert_ServiSignBase64Flag = true
			DataObj.b64Cert = Base64Encode(b64Cert)
			DataObj.ServiSignFunctionIndex = 26

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetExtension : function(b64Cert, oid){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetExtension"
			DataObj.b64Cert_ServiSignBase64Flag = true
			DataObj.b64Cert = Base64Encode(b64Cert)
			DataObj.oid_ServiSignBase64Flag = true
			DataObj.oid = Base64Encode(oid)
			DataObj.ServiSignFunctionIndex = 27

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetSubjectDirectoryAttributes : function(b64Cert, oid){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetSubjectDirectoryAttributes"
			DataObj.b64Cert_ServiSignBase64Flag = true
			DataObj.b64Cert = Base64Encode(b64Cert)
			DataObj.oid_ServiSignBase64Flag = true
			DataObj.oid = Base64Encode(oid)
			DataObj.ServiSignFunctionIndex = 28

			return ServiSignObj.Send(DataObj)
		},
		ATL_ExtractOIDFromDerCode : function(b64DerCode){
			var DataObj = new Object()
			DataObj.functionname = "ATL_ExtractOIDFromDerCode"
			DataObj.b64DerCode_ServiSignBase64Flag = true
			DataObj.b64DerCode = Base64Encode(b64DerCode)
			DataObj.ServiSignFunctionIndex = 29

			return ServiSignObj.Send(DataObj)
		},
		ATL_VerifyCRLSignature : function(b64Crl, b64CACert){
			var DataObj = new Object()
			DataObj.functionname = "ATL_VerifyCRLSignature"
			DataObj.b64Crl_ServiSignBase64Flag = true
			DataObj.b64Crl = Base64Encode(b64Crl)
			DataObj.b64CACert_ServiSignBase64Flag = true
			DataObj.b64CACert = Base64Encode(b64CACert)
			DataObj.ServiSignFunctionIndex = 30

			return ServiSignObj.Send(DataObj)
		},
		ATL_SearchCRL_GetCertStatus : function(b64Crl, certSN){
			var DataObj = new Object()
			DataObj.functionname = "ATL_SearchCRL_GetCertStatus"
			DataObj.b64Crl_ServiSignBase64Flag = true
			DataObj.b64Crl = Base64Encode(b64Crl)
			DataObj.certSN_ServiSignBase64Flag = true
			DataObj.certSN = Base64Encode(certSN)
			DataObj.ServiSignFunctionIndex = 31

			return ServiSignObj.Send(DataObj)
		},
		ATL_SearchCRL_GetRevokedReason : function(b64Crl, certSN){
			var DataObj = new Object()
			DataObj.functionname = "ATL_SearchCRL_GetRevokedReason"
			DataObj.b64Crl_ServiSignBase64Flag = true
			DataObj.b64Crl = Base64Encode(b64Crl)
			DataObj.certSN_ServiSignBase64Flag = true
			DataObj.certSN = Base64Encode(certSN)
			DataObj.ServiSignFunctionIndex = 32

			return ServiSignObj.Send(DataObj)
		},
		ATL_SearchCRL_GetRevokedDate : function(b64Crl, certSN){
			var DataObj = new Object()
			DataObj.functionname = "ATL_SearchCRL_GetRevokedDate"
			DataObj.b64Crl_ServiSignBase64Flag = true
			DataObj.b64Crl = Base64Encode(b64Crl)
			DataObj.certSN_ServiSignBase64Flag = true
			DataObj.certSN = Base64Encode(certSN)
			DataObj.ServiSignFunctionIndex = 33

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetCRLInfo_GetIssuerDN : function(b64Crl){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetCRLInfo_GetIssuerDN"
			DataObj.b64Crl_ServiSignBase64Flag = true
			DataObj.b64Crl = Base64Encode(b64Crl)
			DataObj.ServiSignFunctionIndex = 34

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetCRLInfo_GetThisUpdate : function(b64Crl){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetCRLInfo_GetThisUpdate"
			DataObj.b64Crl_ServiSignBase64Flag = true
			DataObj.b64Crl = Base64Encode(b64Crl)
			DataObj.ServiSignFunctionIndex = 35

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetCRLInfo_GetNextUpdate : function(b64Crl){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetCRLInfo_GetNextUpdate"
			DataObj.b64Crl_ServiSignBase64Flag = true
			DataObj.b64Crl = Base64Encode(b64Crl)
			DataObj.ServiSignFunctionIndex = 36

			return ServiSignObj.Send(DataObj)
		},
		ATL_BuildTobesignedOCSPRequest : function(certSN, hexNonce, b64ReqIssuerCert){
			var DataObj = new Object()
			DataObj.functionname = "ATL_BuildTobesignedOCSPRequest"
			DataObj.certSN_ServiSignBase64Flag = true
			DataObj.certSN = Base64Encode(certSN)
			DataObj.hexNonce_ServiSignBase64Flag = true
			DataObj.hexNonce = Base64Encode(hexNonce)
			DataObj.b64ReqIssuerCert_ServiSignBase64Flag = true
			DataObj.b64ReqIssuerCert = Base64Encode(b64ReqIssuerCert)
			DataObj.ServiSignFunctionIndex = 37

			return ServiSignObj.Send(DataObj)
		},
		ATL_TSQuery : function(hostname, hostport, b64Data){
			var DataObj = new Object()
			DataObj.functionname = "ATL_TSQuery"
			DataObj.hostname_ServiSignBase64Flag = true
			DataObj.hostname = Base64Encode(hostname)
			DataObj.hostport_ServiSignBase64Flag = true
			DataObj.hostport = Base64Encode(hostport)
			DataObj.b64Data_ServiSignBase64Flag = true
			DataObj.b64Data = Base64Encode(b64Data)
			DataObj.ServiSignFunctionIndex = 38

			return ServiSignObj.Send(DataObj)
		},
		ATL_TSVerify : function(b64Data, b64TSRes){
			var DataObj = new Object()
			DataObj.functionname = "ATL_TSVerify"
			DataObj.b64Data_ServiSignBase64Flag = true
			DataObj.b64Data = Base64Encode(b64Data)
			DataObj.b64TSRes_ServiSignBase64Flag = true
			DataObj.b64TSRes = Base64Encode(b64TSRes)
			DataObj.ServiSignFunctionIndex = 39

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetTSInfo : function(b64TSRes, param){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetTSInfo"
			DataObj.b64TSRes_ServiSignBase64Flag = true
			DataObj.b64TSRes = Base64Encode(b64TSRes)
			DataObj.param = param
			DataObj.ServiSignFunctionIndex = 40

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetCertFromLDAP : function(uniqueID, email, cn, ou, serialNumber, fullDN){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetCertFromLDAP"
			DataObj.uniqueID_ServiSignBase64Flag = true
			DataObj.uniqueID = Base64Encode(uniqueID)
			DataObj.email_ServiSignBase64Flag = true
			DataObj.email = Base64Encode(email)
			DataObj.cn_ServiSignBase64Flag = true
			DataObj.cn = Base64Encode(cn)
			DataObj.ou_ServiSignBase64Flag = true
			DataObj.ou = Base64Encode(ou)
			DataObj.serialNumber_ServiSignBase64Flag = true
			DataObj.serialNumber = Base64Encode(serialNumber)
			DataObj.fullDN_ServiSignBase64Flag = true
			DataObj.fullDN = Base64Encode(fullDN)
			DataObj.ServiSignFunctionIndex = 41

			return FixArray(ServiSignObj.Send(DataObj))
		},
		ATL_GetCardType : function(ulModuleHandle){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetCardType"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ServiSignFunctionIndex = 42

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetBasicData : function(ulModuleHandle){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetBasicData"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ServiSignFunctionIndex = 43

			return FixArray(ServiSignObj.Send(DataObj))
		},
		ATL_GetCardSN : function(){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetCardSN"
			DataObj.ServiSignFunctionIndex = 44

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetCardInfo : function(){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetCardInfo"
			DataObj.ServiSignFunctionIndex = 45

			return FixArray(ServiSignObj.Send(DataObj))
		},
		ATL_GetErrorCode : function(){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetErrorCode"
			DataObj.ServiSignFunctionIndex = 46

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetDataRecord : function(ulModuleHandle, pin, recordName){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetDataRecord"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.pin_ServiSignBase64Flag = true
			DataObj.pin = Base64Encode(pin)
			DataObj.recordName_ServiSignBase64Flag = true
			DataObj.recordName = Base64Encode(recordName)
			DataObj.ServiSignFunctionIndex = 47

			return ServiSignObj.Send(DataObj)
		},
		ATL_QueryOCSfromSignedOCSPRequest : function(certSNs, b64TbsOcspReq, b64TbsReqSig, b64ReqIssuerCert, b64SenderCert, b64ServerCert, serverUrl, porxyName, proxyPort){
			var DataObj = new Object()
			DataObj.functionname = "ATL_QueryOCSfromSignedOCSPRequest"
			DataObj.certSNs_ServiSignBase64Flag = true
			DataObj.certSNs = Base64Encode(certSNs)
			DataObj.b64TbsOcspReq_ServiSignBase64Flag = true
			DataObj.b64TbsOcspReq = Base64Encode(b64TbsOcspReq)
			DataObj.b64TbsReqSig_ServiSignBase64Flag = true
			DataObj.b64TbsReqSig = Base64Encode(b64TbsReqSig)
			DataObj.b64ReqIssuerCert_ServiSignBase64Flag = true
			DataObj.b64ReqIssuerCert = Base64Encode(b64ReqIssuerCert)
			DataObj.b64SenderCert_ServiSignBase64Flag = true
			DataObj.b64SenderCert = Base64Encode(b64SenderCert)
			DataObj.b64ServerCert_ServiSignBase64Flag = true
			DataObj.b64ServerCert = Base64Encode(b64ServerCert)
			DataObj.serverUrl_ServiSignBase64Flag = true
			DataObj.serverUrl = Base64Encode(serverUrl)
			DataObj.porxyName_ServiSignBase64Flag = true
			DataObj.porxyName = Base64Encode(porxyName)
			DataObj.proxyPort = proxyPort
			DataObj.ServiSignFunctionIndex = 48

			return ServiSignObj.Send(DataObj)
		},
		ATL_OCSPGetResponseStatus : function(idx){
			var DataObj = new Object()
			DataObj.functionname = "ATL_OCSPGetResponseStatus"
			DataObj.idx = idx
			DataObj.ServiSignFunctionIndex = 49

			return ServiSignObj.Send(DataObj)
		},
		ATL_OCSPGetCertStatus : function(idx){
			var DataObj = new Object()
			DataObj.functionname = "ATL_OCSPGetCertStatus"
			DataObj.idx = idx
			DataObj.ServiSignFunctionIndex = 50

			return ServiSignObj.Send(DataObj)
		},
		ATL_OCSPGetRevokeReason : function(idx){
			var DataObj = new Object()
			DataObj.functionname = "ATL_OCSPGetRevokeReason"
			DataObj.idx = idx
			DataObj.ServiSignFunctionIndex = 51

			return ServiSignObj.Send(DataObj)
		},
		ATL_Util_ReadFileToBase64 : function(filename){
			var DataObj = new Object()
			DataObj.functionname = "ATL_Util_ReadFileToBase64"
			DataObj.filename_ServiSignBase64Flag = true
			DataObj.filename = Base64Encode(filename)
			DataObj.ServiSignFunctionIndex = 52

			return ServiSignObj.Send(DataObj)
		},
		ATL_Util_WriteFileFromBase64 : function(filename, b64Data){
			var DataObj = new Object()
			DataObj.functionname = "ATL_Util_WriteFileFromBase64"
			DataObj.filename_ServiSignBase64Flag = true
			DataObj.filename = Base64Encode(filename)
			DataObj.b64Data_ServiSignBase64Flag = true
			DataObj.b64Data = Base64Encode(b64Data)
			DataObj.ServiSignFunctionIndex = 53

			return ServiSignObj.Send(DataObj)
		},
		ATL_SetCodePage : function(codePage){
			var DataObj = new Object()
			DataObj.functionname = "ATL_SetCodePage"
			DataObj.codePage = codePage
			DataObj.ServiSignFunctionIndex = 54

			return parseInt(ServiSignObj.Send(DataObj))
		},
		ATL_GetCertExtension : function(b64Cert){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetCertExtension"
			DataObj.b64Cert_ServiSignBase64Flag = true
			DataObj.b64Cert = Base64Encode(b64Cert)
			DataObj.ServiSignFunctionIndex = 55

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetCertPublicKey : function(b64Cert){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetCertPublicKey"
			DataObj.b64Cert_ServiSignBase64Flag = true
			DataObj.b64Cert = Base64Encode(b64Cert)
			DataObj.ServiSignFunctionIndex = 56

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetCertKeyUsage : function(b64Cert){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetCertKeyUsage"
			DataObj.b64Cert_ServiSignBase64Flag = true
			DataObj.b64Cert = Base64Encode(b64Cert)
			DataObj.ServiSignFunctionIndex = 57

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetHCACardType : function(ulModuleHandle){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetHCACardType"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ServiSignFunctionIndex = 58

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetHCABasicData : function(ulModuleHandle){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetHCABasicData"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ServiSignFunctionIndex = 59

			return FixArray(ServiSignObj.Send(DataObj))
		},
		ATL_GetHCACardSN : function(ulModuleHandle){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetHCACardSN"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ServiSignFunctionIndex = 60

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetHCACardInfo : function(ulModuleHandle){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetHCACardInfo"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ServiSignFunctionIndex = 61

			return FixArray(ServiSignObj.Send(DataObj))
		},
		ATL_GetHCAKeyLength : function(ulModuleHandle){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetHCAKeyLength"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ServiSignFunctionIndex = 62

			return ServiSignObj.Send(DataObj)
		},
		ATL_MakeSignatureEx : function(ulModuleHandle, ulSessionHandle, ulAlgorithm, b64Data, ulPvKeyObject){
			var DataObj = new Object()
			DataObj.functionname = "ATL_MakeSignatureEx"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ulSessionHandle = ulSessionHandle
			DataObj.ulAlgorithm = ulAlgorithm
			DataObj.b64Data_ServiSignBase64Flag = true
			DataObj.b64Data = Base64Encode(b64Data)
			DataObj.ulPvKeyObject = ulPvKeyObject
			DataObj.ServiSignFunctionIndex = 63

			return ServiSignObj.Send(DataObj)
		},
		ATL_InitSOSession : function(ulModuleHandle, iFlags, cardPuk){
			var DataObj = new Object()
			DataObj.functionname = "ATL_InitSOSession"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.iFlags = iFlags
			DataObj.cardPuk_ServiSignBase64Flag = true
			DataObj.cardPuk = Base64Encode(cardPuk)
			DataObj.ServiSignFunctionIndex = 64

			return ServiSignObj.Send(DataObj)
		},
		ATL_UnblockUserPIN : function(ulModuleHandle, ulSessionHandle, newUserPin){
			var DataObj = new Object()
			DataObj.functionname = "ATL_UnblockUserPIN"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ulSessionHandle = ulSessionHandle
			DataObj.newUserPin_ServiSignBase64Flag = true
			DataObj.newUserPin = Base64Encode(newUserPin)
			DataObj.ServiSignFunctionIndex = 65

			return ServiSignObj.Send(DataObj)
		},
		ATL_MakeSignatureEx2 : function(ulModuleHandle, ulSessionHandle, ulAlgorithm, ulEndianness, b64Data, ulPvKeyObject){
			var DataObj = new Object()
			DataObj.functionname = "ATL_MakeSignatureEx2"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ulSessionHandle = ulSessionHandle
			DataObj.ulAlgorithm = ulAlgorithm
			DataObj.ulEndianness = ulEndianness
			DataObj.b64Data_ServiSignBase64Flag = true
			DataObj.b64Data = Base64Encode(b64Data)
			DataObj.ulPvKeyObject = ulPvKeyObject
			DataObj.ServiSignFunctionIndex = 66

			return ServiSignObj.Send(DataObj)
		},
		ATL_GetHCACertType : function(ulModuleHandle){
			var DataObj = new Object()
			DataObj.functionname = "ATL_GetHCACertType"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ServiSignFunctionIndex = 67

			return ServiSignObj.Send(DataObj)
		},
		ATL_VerifySignatureByCert2 : function(b64Cert, b64Data, b64Sig){
			var DataObj = new Object()
			DataObj.functionname = "ATL_VerifySignatureByCert2"
			DataObj.b64Cert_ServiSignBase64Flag = true
			DataObj.b64Cert = Base64Encode(b64Cert)
			DataObj.b64Data_ServiSignBase64Flag = true
			DataObj.b64Data = Base64Encode(b64Data)
			DataObj.b64Sig_ServiSignBase64Flag = true
			DataObj.b64Sig = Base64Encode(b64Sig)
			DataObj.ServiSignFunctionIndex = 68

			return ServiSignObj.Send(DataObj)
		},
		ATL_CryptCertGetSignatureAlgorithm : function(b64Cert, flag){
			var DataObj = new Object()
			DataObj.functionname = "ATL_CryptCertGetSignatureAlgorithm"
			DataObj.b64Cert_ServiSignBase64Flag = true
			DataObj.b64Cert = Base64Encode(b64Cert)
			DataObj.flag = flag
			DataObj.ServiSignFunctionIndex = 69

			return ServiSignObj.Send(DataObj)
		},
		ATL_CryptSignatureGetAlgorithm : function(b64Cert, b64Sig, flags){
			var DataObj = new Object()
			DataObj.functionname = "ATL_CryptSignatureGetAlgorithm"
			DataObj.b64Cert_ServiSignBase64Flag = true
			DataObj.b64Cert = Base64Encode(b64Cert)
			DataObj.b64Sig_ServiSignBase64Flag = true
			DataObj.b64Sig = Base64Encode(b64Sig)
			DataObj.flags = flags
			DataObj.ServiSignFunctionIndex = 70

			return ServiSignObj.Send(DataObj)
		},
		ATL_CloseModuleEx : function(ulModuleHandle, iFlag){
			var DataObj = new Object()
			DataObj.functionname = "ATL_CloseModuleEx"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.iFlag = iFlag
			DataObj.ServiSignFunctionIndex = 71

			return ServiSignObj.Send(DataObj)
		},
		ATL_SignWithoutHash : function(ulModuleHandle, ulSessionHandle, b64Data, ulPvKeyObject){
			var DataObj = new Object()
			DataObj.functionname = "ATL_SignWithoutHash"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ulSessionHandle = ulSessionHandle
			DataObj.b64Data_ServiSignBase64Flag = true
			DataObj.b64Data = Base64Encode(b64Data)
			DataObj.ulPvKeyObject = ulPvKeyObject
			DataObj.ServiSignFunctionIndex = 72

			return ServiSignObj.Send(DataObj)
		},
		ATL_ChangeUserPIN : function(ulModuleHandle, ulSessionHandle, newUserPin){
			var DataObj = new Object()
			DataObj.functionname = "ATL_ChangeUserPIN"
			DataObj.ulModuleHandle = ulModuleHandle
			DataObj.ulSessionHandle = ulSessionHandle
			DataObj.newUserPin_ServiSignBase64Flag = true
			DataObj.newUserPin = Base64Encode(newUserPin)
			DataObj.ServiSignFunctionIndex = 73

			return ServiSignObj.Send(DataObj)
		}
	}

	window.addEventListener("beforeunload", function (e) {
		if(ServiSignTabMode == ServiSignTabMode_Tab){
			ServiSignObj.ServiSignRelease()
		}
		else if(ServiSignTabMode == ServiSignTabMode_Browser){
			//Do nothing
		}
	})
	if(!ServiSignObj.ServiSigninit()){
		ServiSignLoadComponentError()
		return undefined
	}
	
	return ServiSignInterface
}

function ServiSign_GetErrorCode(){
	return ServiSignErrorCode
}
