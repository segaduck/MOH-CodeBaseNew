var cabfileATL="HCAAPIATL.cab#Version=1,0,19,510";
var cabfileATL64="HCAAPIATLx64.cab#Version=1,0,19,510";
var classidATL="CLSID:97E2BA41-8E68-4FCA-AEC6-DAD248400C20";
var classidATL64="CLSID:97E2BA41-8E68-4FCA-AEC6-DAD248400C20";
	
	//=以下需做設定=======================================================================================
	MetaTag="<meta http-equiv=\"X-UA-Compatible\" content=\"IE=EmulateIE7;requiresActiveX=true\"/>";
	
	function RunActiveX()	{ //載入ActiveX
	
		document.write(MetaTag);
		if(window.navigator.platform=="Win32"){
			console.log("Win32");
			document.write(""+SetObjectTag(cabfileATL,classidATL,"hca"));
		}else if(window.navigator.platform=="Win64"){
			console.log("Win64");
			document.write(""+SetObjectTag(cabfileATL64,classidATL64,"hca"));
		}		
		//console.log("元件載入成功");
	}
	
	function RunServiSign(){ //載入ServiSign
		//console.log("Windows");
		hca = getHCAAPISVIAdapterObj();
		CheckLoadServiSignState(hca, "hca");
	}
	
	/*
	function RunServiSignonMac(){  //載入MacServiSign
		//console.log("Mac");
		PKIOBJ = getMacPKIObj();
		CheckLoadServiSignState(PKIOBJ, "PKIOBJ");
	}
	*/
	
	function CheckBrowserAndLoad(){  //判斷Browser後運行元件
		//JavaApplet Object Call
		var browserName = getBrowser();
		var osName = detectOS();	//Windows 7, 8, 8.1, 10, Vista, XP
		var osName = getOS();	//Windows, Mac, Linux
		var browserVersion = getVersion();
		console.log("HCA作業環境: " + osName + " " + browserName);
		if (osName == "Windows") {	//Windows, Linux, Mac
			if(browserName == "Explorer"){ 				
				//console.log("Explorer");
				RunServiSign(); //ServiSign only
				if(ServiSignErrorCode != 0)
					RunActiveX(); //for IE ActiveX
			}
			else if(browserName == "Chrome" ){				
				RunServiSign();
			}
			else if(browserName == "Firefox"){
				RunServiSign();
			}
			else if(browserName == "Edge"){
				RunServiSign();
			}
			else if(browserName == "Opera"){
				RunServiSign();
			}
			else if(browserName == "Safari"){
				RunServiSign();
			}
		}else if(osName == "Mac"){
			if(browserName == "Explorer"){ 				
				RunServiSign();
			}
			else if(browserName == "Chrome" ){				
				RunServiSign();
			}
			else if(browserName == "Firefox"){
				RunServiSign();
			}
			else if(browserName == "Edge"){
				RunServiSign();
			}
			else if(browserName == "Opera"){
				RunServiSign();
			}
			else if(browserName == "Safari"){
				RunServiSign();
			}
		}else {    //不認識的作業系統不支援
			console.log("請使用Windows/Mac作業系統，並執行ServiSign簽章元件");
			return_component = "Unknown";		
		}
	}

	//=以上需做設定=======================================================================================
	
	var BrowserDetect = {
	init: function () {
		this.browser = this.searchString(this.dataBrowser) || "An unknown browser";
		this.version = this.searchVersion(navigator.userAgent)
			|| this.searchVersion(navigator.appVersion)
			|| "an unknown version";
		this.OS = this.searchString(this.dataOS) || "an unknown OS";
	},
	searchString: function (data) {
		for (var i=0;i<data.length;i++)	{
			var dataString = data[i].string;
			var dataProp = data[i].prop;
			this.versionSearchString = data[i].versionSearch || data[i].identity;
			if (dataString) {
				if (dataString.indexOf(data[i].subString) != -1)
					return data[i].identity;
			}
			else if (dataProp)
				return data[i].identity;
		}
	},
	searchVersion: function (dataString) {
		var index = dataString.indexOf(this.versionSearchString);
		if (index == -1) return;
		var tmp = dataString.substring(index+this.versionSearchString.length+1);
		index = tmp.indexOf(" ");
		if( index == -1) index = tmp.length;
		return tmp.substring(0, index);
	},
	dataBrowser: [
		{
			string: navigator.userAgent,
			subString: "Chrome",
			identity: "Chrome"
		},
		{ 	string: navigator.userAgent,
			subString: "OmniWeb",
			versionSearch: "OmniWeb/",
			identity: "OmniWeb"
		},
		{
			string: navigator.vendor,
			subString: "Apple",
			identity: "Safari",
			versionSearch: "Version"
		},
		{
			prop: window.opera,
			identity: "Opera"
		},
		{
			string: navigator.vendor,
			subString: "iCab",
			identity: "iCab"
		},
		{
			string: navigator.vendor,
			subString: "KDE",
			identity: "Konqueror"
		},
		{
			string: navigator.userAgent,
			subString: "Firefox",
			identity: "Firefox"
		},
		{
			string: navigator.vendor,
			subString: "Camino",
			identity: "Camino"
		},
		{		// for newer Netscapes (6+)
			string: navigator.userAgent,
			subString: "Netscape",
			identity: "Netscape"
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
		},
		{
			string: navigator.userAgent,
			subString: "Gecko",
			identity: "Mozilla",
			versionSearch: "rv"
		},
		{ 		// for older Netscapes (4-)
			string: navigator.userAgent,
			subString: "Mozilla",
			identity: "Netscape",
			versionSearch: "Mozilla"
		}
	],
	dataOS : [
		{
			string: navigator.platform,
			subString: "Win",
			identity: "Windows"
		},
		{
			string: navigator.platform,
			subString: "Mac",
			identity: "Mac"
		},
		{
			   string: navigator.userAgent,
			   subString: "iPhone",
			   identity: "iPhone/iPod"
	    },
		{
			string: navigator.platform,
			subString: "Linux",
			identity: "Linux"
		}
	]

};
	
	BrowserDetect.init();
	//document.write('Browser: '+BrowserDetect.browser+'<br>version: '+BrowserDetect.version+'<br>OS: '+BrowserDetect.OS); 

	//CheckBrowser();

	function getBrowser(){
		//console.log("Browser: " + BrowserDetect.browser);
		return BrowserDetect.browser;
	}
	
	function getVersion(){
		
		return BrowserDetect.version;
	}
	
	function getOS(){
		//console.log("OS: " + BrowserDetect.OS);
		return BrowserDetect.OS;
	}

	function detectOS() {
		
		var os, ua = navigator.userAgent;
		if (ua.match(/Win(dows )?NT 6\.1/)) {
			os = "Windows 7";               // Windows 7
		}
		else if (ua.match(/Win(dows )?NT 6\.0/)) {
			os = "Windows Vista";               // Windows Vista
		}
		else if (ua.match(/Win(dows )?NT 5\.2/)) {
			os = "Windows Server 2003";         // Windows Server 2003
		}
		else if (ua.match(/Win(dows )?(NT 5\.1|XP)/)) {
			os = "Windows XP";              // Windows XP
		}
		else if (ua.match(/Win(dows )? (9x 4\.90|ME)/)) {
			os = "Windows ME";              // Windows ME
		}
		else if (ua.match(/Win(dows )?(NT 5\.0|2000)/)) {
			os = "Windows 2000";                // Windows 2000
		}
		else if (ua.match(/Win(dows )?98/)) {
			os = "Windows 98";              // Windows 98
		}
		else if (ua.match(/Win(dows )?NT( 4\.0)?/)) {
			os = "Windows NT 4.0";              // Windows NT 4.0
		}
		else if (ua.match(/Win(dows )?95/)) {
			os = "Windows 95";              // Windows 95
		}
		else if (ua.match(/Mac|PPC/)) {
			os = "Mac OS";                  // Macintosh
		}
		else if (ua.match(/Linux/)) {
			os = "Linux";                   // Linux
		}
		else if (ua.match(/(Free|Net|Open)BSD/)) {
			os = RegExp.$1 + "BSD";             // BSD
		}
		else if (ua.match(/SunOS/)) {
			os = "Solaris";                 // Solaris
		}
		else {
			os = "Unknown";                 // Other OS
		}
		return os;
	}
		
	function fnJSLanguageVer(sWordC, sWordE){  //回傳中文錯誤代碼訊息
		return sWordC;
	}

	function SetObjectTag(CodeBase,ClassID,idName){//若使用ActiveX  利用此產生Object Tag
		
		var ObjectString = "" +
		"<object CLASSID='" + ClassID + "' " +
		"CODEBASE='" + CodeBase + "' " +
		"ID='"+idName+"' onerror='installActiveXError()'" +
		"width='0' " +
		"height='0' "+
		">" +
		"</object>" ;  
		//console.log(ObjectString);
		return ObjectString;
	}

	function getCookie(cname){ //取得Cookie中的"hasServiSign="資訊
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

	function CheckLoadServiSignState(LoadState, LoadObjectName){ //檢查ServiSign載入狀況	
	ServiSignErrorCode = 61001;
			if (LoadState != undefined) {//ServiSign載入成功
			    console.log(LoadObjectName + " ServiSign簽章元件載入成功");
			}
			else {  //ServiSign載入失敗
			    console.log("ServiSign簽章元件載入失敗，錯誤代碼:" + GetErrorMessage(ServiSignErrorCode));
			}
	}

	/*function CheckServiSignInstalled(){  //檢查是否安裝ServiSign 1/0
		var InstalledServiSign = 0;
		var DownloadURL="";
		var checkLoad="";
		if (getCookie("hasServiSign") == ""){
			checkLoad = LinkServisignAdapter();		
			if (checkLoad != undefined) {
					document.cookie = "hasServiSign=0";
					InstalledServiSign = 1;
				}
				else {//未安裝ServiSign
					document.cookie = "hasServiSign=-1";
					if(DownloadURL == ""){
						console.log("1請安裝ServiSign簽章元件才可進行交易，如果已安裝元件，請重新啟動瀏覽器再試一次");
					}else{
						//window.location = "" + DownloadURL;
						console.log("1 " + DownloadURL);
					}
				}
		}
		else if (getCookie("hasServiSign") == "0"){
			InstalledServiSign = 1;
			
		}else if (getCookie("hasServiSign") == "-1"){
			checkLoad = undefined;
			if(DownloadURL == ""){
				console.log("2請安裝ServiSign簽章元件才可進行交易，如果已安裝元件，請重新啟動瀏覽器再試一次");
			}else{
				//window.location = "" + DownloadURL;
				console.log("2");
			}
		}else{
			if(DownloadURL == ""){
				console.log("安裝資訊異常，如果已安裝元件，請重新啟動瀏覽器再試一次");
			}else{
				//window.location = "" + DownloadURL;
				console.log("3");
			}
		}
		
		return InstalledServiSign;
	}
*/

	function installActiveXError()	{
		console.log( "[ActiveX] 載入失敗");
	}
