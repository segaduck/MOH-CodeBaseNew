namespace ES.Models
{
    public class CreditHPPModel
    {
        public CreditHPPModel()
        {
            this.ECConnetModel = new ECPG_ConnectModel();
            this.EncModel = new ECPG_EncryptModel();
            this.DecModel = new ECPG_DecryptModel();
        }
        public ECPG_ConnectModel ECConnetModel { get; set; }
        public ECPG_EncryptModel EncModel { get; set; }
        public ECPG_DecryptModel DecModel { get; set; }
    }

    /// <summary>
    /// 連線方式
    /// </summary>
    public class ECPG_ConnectModel
    {
        /// <summary>
        /// Internet使用者 setURL(DomainName,RequestURL)
        /// 專線使用者 setIP(IP,PORT,RequestURL)
        /// IBM JSSE使用者 apiClient.setProvider("com.ibm.jsse.JSSEProvider")
        /// setURL 環境          Domain Name                 Request URL
        /// 正式環境網址  nccnet-ec.nccc.com.tw       /merchant/HPPRequest
        /// 測試環境網址  nccnet-ectest.nccc.com.tw   /merchant/HPPRequest
        /// setIP 環境 中心提供IP port38181
        /// </summary>
        public string DomainName { get; set; }
        public string RequestURL { get; set; }
        public string ClientIP { get; set; }
        public string ClientPORT { get; set; }
        public string JsseProvider { get; set; }

    }
    /// <summary>
    /// HppPlusDotNetTls1.2-1.dll 加解密元件
    /// 交易參數設定 
    /// 授權交易 postTransaction() 
    /// -999:連線失敗
    /// -901:特店代號格式錯誤
    /// -902:端末機袋號格式錯誤
    /// -903:訂單編號格式錯誤
    /// -904:交易金額格式錯誤
    /// -905:3D回應網址格式錯誤
    /// -906:分期交易，請輸入分期期數
    /// -907:交易模式輸入錯誤
    /// -908:會員編號或會員TOKEN僅能擇一設定
    /// 2:執行完畢
    /// getRESPONSECODE()  "00" 取得交易金鑰 otherwise 錯誤訊息代碼
    /// getRESPONSEMSG() 回應訊息
    /// getKEY() 當 getRESPONSECODE()=”00”時，代表取得”交易金鑰”否則為錯誤訊息
    /// 授權取消交易 postCancel() [MerchantID,OrderID]
    /// -999:連線失敗
    /// -901:特店代號格式錯誤
    /// -903:訂單編號格式錯誤
    /// 授權結果查詢 postQuery() [MerchantID,OrderID] or [Key交易金鑰]
    /// </summary>
    public class ECPG_EncryptModel
    {
        /// <summary>
        /// *商店代號 setMERCHANTID() 特約商店代號(合約)
        /// </summary>
        public string MerchantID { get; set; }

        /// <summary>
        /// *訂單編號 setORDERID() EC 系統交易序號對應商店指派的「交易訂單編號」。必須唯一不可重複。不支援中文。
        /// </summary>
        public string OrderID { get; set; }

        /// <summary>
        /// *端末機代碼 setTERMINALID() 端末機代碼(合約)
        /// </summary>
        public string TerminalID { get; set; }

        /// <summary>
        /// 群組商店代號 setGroupMerchantID 1.卡號綁定特店適用 2.如使用群組商店綁定卡號，授權交易需同時傳送此欄位
        /// </summary>
        public string GroupMerchantID { get; set; }

        /// <summary>
        /// 會員 setMemberToken() 綁定後中心產生之會員 TOKEN 1. 卡號綁定特店適用 
        /// 2. 傳送交易時此欄位或MemberID 欄位其一須帶值且僅一欄位有值 3. 如此欄位有值則視為已綁定 
        /// 卡片管理功能注意事項請參照 7.5 Üny 綁卡應注意事項
        /// </summary>
        public string TOKENMemberToken { get; set; }

        /// <summary>
        /// 會員編號 setMemberID() 如該會員尚未綁定則傳送 MemberID  於交易完成後綁定該卡 1. 卡號綁定特店適用 
        /// 2. 傳送交易時此欄位或MemberToken 欄位其一須帶值且僅一欄位有值 3. 如此欄位有值則視為未綁定
        /// 卡片管理功能注意事項請參照 7.5 Üny 綁卡應注意事項
        /// </summary>
        public string MemberID { get; set; }

        /// <summary>
        /// 會員行動電話 setMobileNumber() 商店會員行動電話1. 卡號綁定特店適用
        /// 2. 如為此類特店則此欄位必填 3. 須為台灣地區之電話號碼,國外號碼將導致認證失敗 4. EX:0911234567
        /// </summary>
        public string MobileNumber { get; set; }

        /// <summary>
        /// *交易種類 setTRANSMODE() 0.一般交易 1.分期交易 2.紅利折抵交易
        /// </summary>
        public string TransMode { get; set; }

        /// <summary>
        /// *交易金額 setTRANSAMT() 需整數
        /// </summary>
        public string TransAmt { get; set; }

        /// <summary>
        /// *回應網址 setNotifyURL() 交易回覆網址必須為 https 網址，否則當授權結果回覆(POST)給特約商店時，可能會因為持卡人瀏覽器環境設定因素而遭到瀏覽器阻擋，導致商店無法確認交易狀態。
        /// </summary>
        public string NotifyURL { get; set; }

        /// <summary>
        /// 分期期數 setINSTALLMENT() 若為分期交易，該值不得為零。
        /// </summary>
        public string Installment { get; set; }

        /// <summary>
        /// CSS檔網址 setCSS_URL() 當特約商店使用簡易付款業且要改變樣式時，簡易付款頁將由指定之網址讀取css設定。
        /// </summary>
        public string CssUrl { get; set; }

        /// <summary>
        /// 金融銀行代碼 setBankNo() 當特約商店需要限制授權交易之發卡行時，請以BankNo參數指定發卡行代碼，超過一家銀行請以","區隔
        /// </summary>
        public string BankNo { get; set; }

        /// <summary>
        /// 設定付款頁 setTemplate() 參考2.3付款頁說明
        /// </summary>
        public string Template { get; set; }

        /// <summary>
        /// 自訂資料 setPrivateData() 特約商店自訂資料，其值請以 Name=Value & Name=Value…Name=Value 的格式(中間不可包含任何空格)組成字串
        /// </summary>
        public string PrivateData { get; set; }

        /// <summary>
        /// 身分驗證碼 setIDNUMBER() 1. 身分證字號或外僑居留證號(網路投保特店:參考上線注意事項 7.4) 
        /// 2. 身分證字號或外僑居留證號或統一編號(ON US 公務機構特店) 2.1:如為統一編號此欄位長度為 8
        /// </summary>
        public string IDNUMBER { get; set; }

        /// <summary>
        /// 手機號碼 setCh_Mobile() 網路投保特店適用
        /// </summary>
        public string CH_MOBILE { get; set; }

        /// <summary>
        /// 卡人生日 setCh_Birth() 網路投保特店適用
        /// </summary>
        public string CH_BIRTH { get; set; }

        /// <summary>
        /// 交易網址 setTransURL 特店交易網址
        /// </summary>
        public string TransURL { get; set; }

        /// <summary>
        /// 持卡人姓名 setCardholderName() 英文數字均可
        /// </summary>
        public string CardholderName { get; set; }

        /// <summary>
        /// 持卡人帳單地址-城市 setCity() 中文英文均可，1個中文字站3個字元空間。
        /// </summary>
        public string CardholderBillingAddressCity { get; set; }

        /// <summary>
        /// 持卡人帳單地址1 setAddress1() 中英文數字均可，超過50字元請續填下方地址2欄位
        /// </summary>
        public string CardholderBillingAddressLine1 { get; set; }

        /// <summary>
        /// 持卡人帳單地址2 setAddress2() 若地址 1 長度小於 50 字元時，此欄位不需傳送。
        /// </summary>
        public string CardholderBillingAddressLine2 { get; set; }

        /// <summary>
        /// 持卡人帳單地址3 setAddress3() 若地址 2 長度小於 50 字元時，此欄位不需傳送。
        /// </summary>
        public string CardholderBillingAddressLine3 { get; set; }

        /// <summary>
        /// 持卡人帳單地址郵遞區號 setPostalCode() 填入3碼郵遞區號
        /// </summary>
        public string CardholderBillingAddressPostalCode { get; set; }

        /// <summary>
        /// 持卡人電子信箱 setEmail() 字串254
        /// </summary>
        public string CardholderEmailAddress { get; set; }

        /// <summary>
        /// APILOG敏感性資料遮蔽 setLogMaskFlag() true/false 當啟用API程式紀錄時，設定此參數可另行遮蔽log中之敏感欄位。
        /// </summary>
        public string LogMaskFlag { get; set; }

        /// <summary>
        /// JAVAAPI程式訊息 setDebug() true/false 啟用/關閉API程式紀錄(System.out)交易相關電文訊息寫入Application Server系統log中。
        /// </summary>
        public string LogMaskFlag_Java { get; set; }

        /// <summary>
        /// .NET API COM+API 程式訊息 apiClient.LogFile="路徑\LogFileName" or apiClient.setLogFile("路徑\LogFileName")
        /// </summary>
        public string LogFileNameUrl { get; set; }
    }


    /// <summary>
    /// 授權結果查詢 postQuery() [MerchantID,OrderID] or [Key交易金鑰]
    /// </summary>
    public class ECPG_DecryptModel
    {
        /// <summary>
        /// 商店代號 getMERCHANTID()
        /// </summary>
        public string MerchantID { get; set; }
        /// <summary>
        /// 訂單編號 getORDERID() 交易訂單編號
        /// </summary>
        public string OrderID { get; set; }
        /// <summary>
        /// 端末機代號 getTERMINALID()
        /// </summary>
        public string TerminalID { get; set; }
        /// <summary>
        /// 會員Token getMemberToken()
        /// </summary>
        public string MemberToken { get; set; }
        /// <summary>
        /// 會員編號 getMemberID()
        /// </summary>
        public string MemberID { get; set; }
        /// <summary>
        /// 卡號 getPAN() 交易卡號(部分遮蔽)
        /// </summary>
        public string PAN { get; set; }
        /// <summary>
        /// 卡片到期日 getEXPIREDATE() YYMM
        /// </summary>
        public string EXPIREDATE { get; set; }
        /// <summary>
        /// 交易代碼 getTRANSCODE() 00.授權 01.取消
        /// </summary>
        public string TRANSCODE { get; set; }
        /// <summary>
        /// 交易類別 getTRANSMODE() 0.一般 1.分期 2.紅利
        /// </summary>
        public string TRANSMODE { get; set; }
        /// <summary>
        /// 交易金額 getTRANSAMT()
        /// </summary>
        public string TRANSAMT { get; set; }
        /// <summary>
        /// 交易日期 getTRANSDATE() YYYYMMDD
        /// </summary>
        public string TRANSDATE { get; set; }
        /// <summary>
        /// 交易時間 getTRANSTIME() HHMMSS
        /// </summary>
        public string TRANSTIME { get; set; }
        /// <summary>
        /// 授權碼 getAPPROVECODE()
        /// </summary>
        public string APPROVECODE { get; set; }
        /// <summary>
        /// 授權回應碼 getRESPONSECODE()
        /// </summary>
        public string RESPONSECODE { get; set; }
        /// <summary>
        /// 授權回應訊息 getRESPONSEMSG()
        /// </summary>
        public string RESPONSEMSG { get; set; }
        /// <summary>
        /// 分期期數 getINSTALLMENT()
        /// </summary>
        public string INSTALLMENT { get; set; }
        /// <summary>
        /// 分期手續費計價方式 getINSTALLMENTTYPE() E外加 I內合
        /// </summary>
        public string INSTALLMENTTYPE { get; set; }
        /// <summary>
        /// 首期金額 getFIRSTAMT()
        /// </summary>
        public string FIRSTAMT { get; set; }
        /// <summary>
        /// 每期金額 getEACHAMT()
        /// </summary>
        public string EACHAMT { get; set; }
        /// <summary>
        /// 分期手續費 getFEE()
        /// </summary>
        public string FEE { get; set; }
        /// <summary>
        /// 紅利折抵方式 getREDEEMTYPE() 1.全額 2.部分
        /// </summary>
        public string REDEEMTYPE { get; set; }
        /// <summary>
        /// 紅利折抵點數 getREDEEMUSED()
        /// </summary>
        public string REDEEMUSED { get; set; }
        /// <summary>
        /// 紅利餘額 getREDEEMBALANCE()
        /// </summary>
        public string REDEEMBALANCE { get; set; }
        /// <summary>
        /// 持卡人自付金額 getCREDITAMT()
        /// </summary>
        public string CREDITAMT { get; set; }
        /// <summary>
        /// 風險卡號註記 getRiskMark() Y表示風險卡號
        /// </summary>
        public string RiskMark { get; set; }
        /// <summary>
        /// 國外卡 getForeign() 為Visa,MasterCard,JCB卡時 Y國外卡,N國內卡 否則為空白
        /// </summary>
        public string Foreign { get; set; }
        /// <summary>
        /// 3D認證結果 getSecureStatus() Y成功 N失敗 B未執行3D認證 未啟用3D功能則空白
        /// </summary>
        public string SecureStatus { get; set; }
        /// <summary>
        /// 自訂資料 getPrivateData()
        /// </summary>
        public string PrivateData { get; set; }
    }
}