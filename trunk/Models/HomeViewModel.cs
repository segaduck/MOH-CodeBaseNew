using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Turbo.DataLayer;
using EECOnline.Commons;
using EECOnline.DataLayers;
using EECOnline.Services;
using EECOnline.Models.Entities;
using Turbo.Commons;
using Omu.ValueInjecter;
using System.Collections;

namespace EECOnline.Models
{
    /// <summary>
    /// 首頁
    /// </summary>
    public class HomeViewModel
    {
        public HomeViewModel() { this.TempDatas = new Hashtable(); }

        /// <summary>News</summary>
        public NewsModel News { get; set; }

        /// <summary>
        /// 流程進度管控：   <br/>
        /// 1: 憑證登入     <br/>
        /// 2: 線上申辦說明  <br/>
        /// 3: 填寫申請資料  <br/>
        /// 4: 申請確認     <br/>
        /// 5: 申請結束畫面  <br/>
        /// </summary>
        public string ProcessStep { get; set; }

        /// <summary>使用何種方式登入<br/>
        /// 1: 自然人憑證登入<br/>
        /// 2: 行動自然人憑證登入<br/>
        /// 3: 身分證字號＋健保卡<br/>
        /// </summary>
        public string UserLoginTab { get; set; }

        /// <summary>登入 申請訂單用 Model</summary>
        public LoginModel Login { get; set; }

        /// <summary>申請訂單用 Model</summary>
        public LoginApplyModel LoginApply { get; set; }

        /// <summary>登入 查詢申請的訂單用 Model</summary>
        public SearchModel Search { get; set; }

        /// <summary>查詢申請的訂單用 Model</summary>
        public SearchApplyModel SearchApply { get; set; }

        /// <summary>管理申請的訂單用 Model</summary>
        public SearchApplyDetailModel SearchApplyDetail { get; set; }

        public Hashtable TempDatas { get; set; }

        public SuccessPageModel SuccessPage { get; set; }
    }

    public class NewsModel : PagingResultsViewModel
    {
        public IList<NewsGridModel> Grid { get; set; }
    }

    public class NewsGridModel : TblENEWS
    {
        public string showdates_Text
        {
            get
            {
                if (this.showdates.TONotNullString() == "") return "";
                var tmpDT = DateTime.ParseExact(this.showdates, "yyyyMMdd", null);
                return tmpDT.AddYears(-1911).ToString("yyy/MM/dd");
            }
        }

        public string newstype_Text
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                if (this.newstype.TONotNullString() == "") return "";
                return list.enews_list.Where(x => x.Value == this.newstype).FirstOrDefault().Text;
            }
        }
    }

    public class NewsDetailModel : TblENEWS
    {
        public string showdates_Text
        {
            get
            {
                if (this.showdates.TONotNullString() == "") return "";
                var tmpDT = DateTime.ParseExact(this.showdates, "yyyyMMdd", null);
                return tmpDT.AddYears(-1911).ToString("yyy/MM/dd");
            }
        }

        public string newstype_Text
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                if (this.newstype.TONotNullString() == "") return "";
                return list.enews_list.Where(x => x.Value == this.newstype).FirstOrDefault().Text;
            }
        }

        public IList<TblEFILE> Files { get; set; }
    }

    public class LoginModel
    {
        public string user_name { get; set; }

        /// <summary>登入1：身分證字號</summary>
        public string user_idno { get; set; }

        /// <summary>登入1：出生年月日</summary>
        public string user_birthday { get; set; }

        /// <summary>登入1：自然人憑證 PIN 碼</summary>
        public string user_pincode { get; set; }

        /// <summary>登入1：電子郵件Email</summary>
        public string user_email1 { get; set; }

        /// <summary>登入1：認證碼</summary>
        public string ValidateCode1 { get; set; }

        /// <summary>登入1：certData.subjectID (自然人憑證登入暫存用)</summary>
        public string certData_subjectID { get; set; }

        /// <summary>登入2：身分證字號</summary>
        public string user_idno1 { get; set; }

        /// <summary>登入2：出生年月日</summary>
        public string user_birthday1 { get; set; }

        /// <summary>登入2：電子郵件Email</summary>
        public string user_email2 { get; set; }

        /// <summary>登入2：認證碼</summary>
        public string ValidateCode2 { get; set; }

        #region 行動自然人憑證驗證參數

        /// <summary>
        /// 交易序號(GUID)
        /// </summary>
        public string transaction_id { get; set; }

        /// <summary>
        /// 操作代碼(ATH/SIGN)
        /// </summary>
        public string op_code { get; set; }

        /// <summary>
        /// SP服務識別代碼20230926140227945383
        /// </summary>
        public string sp_service_id { get; set; }

        /// <summary>
        /// SP驗證碼 AES_GCM_HEX(SHA256_HEX(transaction_id+sp_service_id+op_code+hint+sign_data)))
        /// </summary>
        public string sp_checksum { get; set; }

        /// <summary>
        /// 提示訊息
        /// </summary>
        public string hint { get; set; }

        /// <summary>
        /// 簽章類型(不必填)
        /// </summary>
        public string sign_type { get; set; }

        /// <summary>
        /// 待簽資料(不必填)
        /// </summary>
        public string sign_data { get; set; }

        /// <summary>
        /// 待簽資料之編碼(不必填)
        /// </summary>
        public string tbs_encoding { get; set; }

        /// <summary>
        /// HASH格式(不必填)
        /// </summary>
        public string hash_algorithm { get; set; }

        /// <summary>
        /// 操作時限(不必填)預設60秒
        /// </summary>
        public string time_limit { get; set; }


        #endregion

        /// <summary>登入3：身分證字號</summary>
        public string user_idno2 { get; set; }

        /// <summary>登入3：出生年月日</summary>
        public string user_birthday2 { get; set; }

        /// <summary>登入3：電子郵件Email</summary>
        public string user_email3 { get; set; }

        /// <summary>登入3：認證碼</summary>
        public string ValidateCode3 { get; set; }
    }

    public class LoginApplyModel : TblEEC_Apply
    {
        [NotDBField]
        public string user_email { get; set; }

        [NotDBField]
        public int? user_birthday_Y
        {
            get
            {
                if (this.user_birthday.TONotNullString() == "") return null;
                DateTime tmpBirth = DateTime.ParseExact(this.user_birthday, "yyyyMMdd", null);
                return tmpBirth.Year - 1911;
            }
        }

        [NotDBField]
        public int? user_birthday_M
        {
            get
            {
                if (this.user_birthday.TONotNullString() == "") return null;
                DateTime tmpBirth = DateTime.ParseExact(this.user_birthday, "yyyyMMdd", null);
                return tmpBirth.Month;
            }
        }

        [NotDBField]
        public int? user_birthday_D
        {
            get
            {
                if (this.user_birthday.TONotNullString() == "") return null;
                DateTime tmpBirth = DateTime.ParseExact(this.user_birthday, "yyyyMMdd", null);
                return tmpBirth.Day;
            }
        }

        [NotDBField]
        public string createdatetime_Text
        {
            get
            {
                if (this.createdatetime.TONotNullString() == "") return "";
                var tmpDT = DateTime.ParseExact(this.createdatetime.TONotNullString(), "yyyy/MM/dd HH:mm:ss", null);
                tmpDT = tmpDT.AddYears(-1911);
                return tmpDT.ToString("yyy 年 MM 月 dd 日 HH:mm:ss");
            }
        }

        [NotDBField]
        public string createdatetime_Text_NoTime
        {
            get
            {
                if (this.createdatetime.TONotNullString() == "") return "";
                var tmpDT = DateTime.ParseExact(this.createdatetime.TONotNullString(), "yyyy/MM/dd HH:mm:ss", null);
                tmpDT = tmpDT.AddYears(-1911);
                return tmpDT.ToString("yyy 年 MM 月 dd 日");
            }
        }

        [NotDBField]
        public string user_idno_Text
        {
            get
            {
                var Result = this.user_idno.TONotNullString().ToCharArray();
                for (int i = 3; i <= Result.Length - 3; i++) Result[i] = '*';
                return new string(Result);
            }
        }

        [NotDBField]
        public string user_name_Text
        {
            get
            {
                var Result = this.user_name.TONotNullString().ToCharArray();
                if (Result.Length == 2)
                    Result[1] = '*';
                else
                    for (int i = 1; i <= Result.Length - 2; i++) Result[i] = '*';
                return new string(Result);
            }
        }

        /// <summary>明細刪除暫存用</summary>
        public string ApplyDetail_DelIdx { get; set; }

        /// <summary>明細付款暫存用</summary>
        //public string ApplyDetail_PayIdx { get; set; }

        /// <summary>申請內容的各筆 一間醫院一筆</summary>
        public IList<LoginApplyDetailModel> ApplyDetail { get; set; }
    }

    public class HisTypeModel
    {
        public HisTypeModel() { this.ApiQueryIndexData = new Utils.Hospital_Common_Api.apiQueryIndexModel(); }
        public string his_type { get; set; }
        public string his_type_name { get; set; }
        public int? price { get; set; }
        public string ec_date { get; set; }
        public string ec_note { get; set; }
        public string ec_dateText { get; set; }
        public string ec_dept { get; set; }
        public string ec_doctor { get; set; }
        public string ec_docType { get; set; }
        public string ec_system { get; set; }
        public EECOnline.Utils.Hospital_Common_Api.apiQueryIndexModel ApiQueryIndexData { get; set; }
    }

    public class LoginApplyDetailModel : TblEEC_ApplyDetail
    {
        private IList<TblEEC_Hospital_Api> HospApiList { get; set; }

        public LoginApplyDetailModel()
        {
            FrontDAO dao = new FrontDAO();
            this.HospApiList = dao.GetRowList(new TblEEC_Hospital_Api());
        }

        [NotDBField]
        public string user_birthday { get; set; }

        [NotDBField]
        public IList<SelectListItem> hospital_code_list
        {
            get
            {
                ShareCodeListModel list = new ShareCodeListModel();
                return list.Get_Hospital_list(true);
            }
        }

        [NotDBField]
        public string his_range1_AD
        {
            get
            {
                if (string.IsNullOrEmpty(his_range1)) { return null; }
                else
                {
                    return HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(his_range1, ""));  // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                his_range1 = HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(value), "");  // YYYMMDD 民國年 使用者看到
            }
        }

        [NotDBField]
        public string his_range2_AD
        {
            get
            {
                if (string.IsNullOrEmpty(his_range2)) { return null; }
                else
                {
                    return HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(his_range2, ""));  // YYYYMMDD 回傳給系統
                }
            }
            set
            {
                his_range2 = HelperUtil.DateTimeToString(HelperUtil.TransToDateTime(value), "");  // YYYMMDD 民國年 使用者看到
            }
        }

        public string[] his_types_SHOW
        {
            get
            {
                if (this.his_types != null)
                    return this.his_types.Replace("'", "").Split(',');
                else
                    return new string[0];
            }
            set
            {
                if (value != null)
                    this.his_types = string.Join(",", value.ToList());
            }
        }

        public IList<CheckBoxListItem> his_types_SHOW_list
        {
            get
            {
                List<CheckBoxListItem> Result = new List<CheckBoxListItem>();
                foreach (var row in this.HisTypes_List)
                {
                    if (this.hospital_code == "1131010011H")
                    {
                        // 亞東
                        var tmpObj = new EECOnline.Utils.Hospital_FarEastern_Api.Api_A1ResultModel();
                        tmpObj.InjectFrom(row);
                        Result.Add(new CheckBoxListItem(
                            row.his_type,
                            row.his_type_name + EECOnline.Utils.Hospital_FarEastern_Api.Api_A1_Remark(tmpObj),
                            false
                       ));
                    }
                    if (this.hospital_code == "1317040011H")
                    {
                        // 中山醫
                        var tmpObj = new EECOnline.Utils.Hospital_csh_Api.Api_A1ResultModel();
                        tmpObj.InjectFrom(row);
                        Result.Add(new CheckBoxListItem(
                            row.his_type,
                            row.his_type_name + EECOnline.Utils.Hospital_csh_Api.Api_A1_Remark(tmpObj),
                            false
                       ));
                    }
                    if (this.hospital_code == "1317050017H")
                    {
                        // 中國醫

                    }
                }
                return Result;
            }
        }

        public IList<HisTypeModel> HisTypes_List
        {
            get
            {
                List<HisTypeModel> Result = new List<HisTypeModel>();
                if (this.hospital_code.TONotNullString() == "" || this.his_range1.TONotNullString() == "" || this.his_range2.TONotNullString() == "") return Result;
                if (this.hospital_code == "1131010011H")
                {
                    // 亞東
                    var findApi = this.HospApiList.Where(x => x.hospital_code == this.hospital_code && x.hospital_apikey == "A1").ToList();
                    if (findApi.ToCount() == 1)
                    {
                        int tmpPrice = 0;
                        Result = EECOnline.Utils.Hospital_FarEastern_Api.Api_A1(
                            findApi.FirstOrDefault().hospital_domain,
                            this.user_idno,
                            this.user_birthday,
                            this.his_range1,
                            this.his_range2
                        ).Select(x => new HisTypeModel()
                        {
                            his_type = x.ec_no,
                            his_type_name = x.ec_name,
                            price = int.TryParse(x.ec_price, out tmpPrice) ? x.ec_price.TOInt32() : 0,
                            ec_date = x.ec_date,
                            ec_dateText = x.ec_dateText,
                            ec_note = x.ec_note,
                            ec_dept = x.ec_dept,
                            ec_doctor = x.ec_doctor,
                            ec_docType = x.ec_docType,
                            ec_system = x.ec_system,
                        }).ToList();
                    }
                    return Result;
                }
                else
                if (this.hospital_code == "1317040011H")
                {
                    // 中山醫
                    int tmpPrice = 0;
                    Result = EECOnline.Utils.Hospital_csh_Api.Api_A1(
                        this.user_idno,
                        this.user_birthday,
                        this.his_range1,
                        this.his_range2
                    ).Select(x => new HisTypeModel()
                    {
                        his_type = x.ec_no,
                        his_type_name = x.ec_name,
                        price = int.TryParse(x.ec_price, out tmpPrice) ? x.ec_price.TOInt32() : 0,
                        ec_date = x.ec_date,
                        ec_dateText = x.ec_dateText,
                        ec_note = x.ec_note,
                        ec_dept = x.ec_dept,
                        ec_doctor = x.ec_doctor,
                        ec_docType = x.ec_docType,
                        ec_system = x.ec_system,
                    }).ToList();
                    return Result;
                }
                else
                if (this.hospital_code == "1317050017H")
                {
                    // 中國醫
                    return Result;
                }
                else
                {
                    // 找不到時，才抓自己系統的
                    // 取得 API 病歷資料
                    //var apiToken = EECOnline.Utils.Hospital_Common_Api.GetLoginToken(ConfigModel.LoginUser, ConfigModel.LoginPwd);
                    //var apiDatas = EECOnline.Utils.Hospital_Common_Api.GetQueryIndex(this.user_idno, this.his_range1, this.his_range2, apiToken);
                    // 取得 系統 病歷類型設定
                    var sclm = new ShareCodeListModel();
                    var list1 = sclm.Get_HIS_Type_ValidList(this.hospital_code);
                    var list2 = sclm.Get_HIS_Type_ValidList_Price(this.hospital_code);
                    foreach (var row in list1)
                    {
                        // 檢查 API 有無相符資料
                        //var findApiData = apiDatas.Where(x => x.HospitalId == this.hospital_code && x.TemplateId == row.Value).ToList();
                        //if (findApiData.ToCount() <= 0) continue;
                        // 塞入回傳
                        //foreach (var apiItem in findApiData)
                        //{
                        var tmpPrice = 0;
                        var tmpObj = list2.Where(x => x.Value == row.Value).FirstOrDefault();
                        if (tmpObj != null) int.TryParse(tmpObj.Text, out tmpPrice);
                        var tmpItem = new HisTypeModel()
                        {
                            his_type = row.Value,
                            his_type_name = row.Text,
                            price = tmpPrice,
                        };
                        //tmpItem.ApiQueryIndexData.InjectFrom(apiItem);
                        Result.Add(tmpItem);
                        //}
                    }
                    return Result;
                }
            }
        }

        [NotDBField]
        public string pay_deadline_Text
        {
            get
            {
                if (this.pay_deadline.TONotNullString() == "") return "";
                var tmpDT = DateTime.ParseExact(this.pay_deadline.TONotNullString(), "yyyy/MM/dd HH:mm:ss", null);
                tmpDT = tmpDT.AddYears(-1911);
                return tmpDT.ToString("yyy 年 MM 月 dd 日 HH:mm:ss");
            }
        }

        [NotDBField]
        public string payed_datetime_Text
        {
            get
            {
                if (this.payed_datetime.TONotNullString() == "") return "";
                var tmpDT = DateTime.ParseExact(this.payed_datetime.TONotNullString(), "yyyy/MM/dd HH:mm:ss", null);
                tmpDT = tmpDT.AddYears(-1911);
                return tmpDT.ToString("yyy 年 MM 月 dd 日 HH:mm:ss");
            }
        }

        /// <summary>申請內容的各筆醫院 其中的費用明細</summary>
        public IList<LoginApplyDetailPriceModel> ApplyDetailPrice { get; set; }
    }

    public class LoginApplyDetailPriceModel : TblEEC_ApplyDetailPrice
    {
        public LoginApplyDetailPriceModel() { this.ApiData = new TblEEC_ApplyDetailPrice_ApiData(); }

        /// <summary>紀錄 EEC_ApplyDetailPrice 資料對應的 API 資料用 (僅共通病歷代號使用，非特殊醫院代號使用)</summary>
        public TblEEC_ApplyDetailPrice_ApiData ApiData { get; set; }

        public string Get_Api_A1_Remark
        {
            get
            {
                var tmpObj = new EECOnline.Utils.Hospital_FarEastern_Api.Api_A1ResultModel();
                tmpObj.InjectFrom(this);
                return EECOnline.Utils.Hospital_FarEastern_Api.Api_A1_Remark(tmpObj);
            }
        }
    }

    public class SearchModel
    {
        /// <summary>登入1：自然人憑證登入 - 自然人憑證 PIN 碼</summary>
        public string user_idno4Last { get; set; }
        /// <summary>登入1：自然人憑證登入 - 自然人憑證 PIN 碼</summary>
        public string user_pincode { get; set; }

        /// <summary>登入1：自然人憑證登入 - 自然人憑證 PIN 碼</summary>
        public string ValidateCode1 { get; set; }

        /// <summary>登入2：行動自然人憑證登入 - 身分證字號</summary>
        public string user_idno1 { get; set; }

        /// <summary>登入1：自然人憑證登入 - 自然人憑證 PIN 碼</summary>
        public string ValidateCode2 { get; set; }

        /// <summary>登入2：行動自然人憑證登入 - 出生年月日</summary>
        public int? user_birthday_Y { get; set; }

        /// <summary>登入2：行動自然人憑證登入 - 出生年月日</summary>
        public int? user_birthday_M { get; set; }

        /// <summary>登入2：行動自然人憑證登入 - 出生年月日</summary>
        public int? user_birthday_D { get; set; }

        /// <summary>登入3：姓名</summary>
        public string user_name { get; set; }

        /// <summary>登入3：身分證字號＋健保卡 - 身分證字號</summary>
        public string user_idno2 { get; set; }

        /// <summary>登入1：自然人憑證登入 - 自然人憑證 PIN 碼</summary>
        public string ValidateCode3 { get; set; }

        /// <summary>登入3：身分證字號＋健保卡 - 健保卡註冊密碼</summary>
        public string user_cardpwd { get; set; }

        /// <summary>登入1：自然人憑證登入 - 自然人憑證 PIN 碼</summary>
        public string user_birthday2 { get; set; }
    }

    public class SearchApplyModel
    {
        public string user_idno { get; set; }
        public string user_name { get; set; }
        public string user_name_Text
        {
            get
            {
                var Result = this.user_name.TONotNullString().ToCharArray();
                if (Result.Length == 2)
                    Result[1] = '*';
                else
                    for (int i = 1; i <= Result.Length - 2; i++) Result[i] = '*';
                return new string(Result);
            }
        }
        public string Search1Filter { get; set; }
        public string Search2Filter { get; set; }
        public string Search3Filter { get; set; }
        public IList<SelectListItem> Search1Filter_list { get { return new ShareCodeListModel().Get_SearchFilter_list(true); } }
        public IList<SelectListItem> Search2Filter_list { get { return new ShareCodeListModel().Get_SearchFilter_list(true); } }
        public IList<SelectListItem> Search3Filter_list { get { return new ShareCodeListModel().Get_SearchFilter_list(true); } }
        public IList<SearchGridModel> SearchGrid1 { get; set; }
        public IList<SearchGridModel> SearchGrid2 { get; set; }
        public IList<SearchGridModel> SearchGrid3 { get; set; }

        /// <summary>作用中的頁籤 (1: 近期訂單, 2: 交易中訂單, 3: 歷史訂單)</summary>
        public string ActiveGridTab { get; set; }

        /// <summary>點擊查詢的訂單編號 (apply_no_sub)</summary>
        public string DetailApplyNo { get; set; }

        /// <summary>點擊刪除的訂單編號 (apply_no_sub)</summary>
        public string DeleteApplyNo { get; set; }
    }

    public class SearchGridModel : TblEEC_ApplyDetail
    {
        private IList<TblEEC_Hospital_Api> HospApiList { get; set; }

        public SearchGridModel()
        {
            FrontDAO dao = new FrontDAO();
            this.HospApiList = dao.GetRowList(new TblEEC_Hospital_Api());
        }

        [NotDBField]
        public string user_birthday { get; set; }

        [NotDBField]
        private List<TblEEC_ApplyDetailPrice> TheData_ApplyDetails
        {
            get
            {
                // 是屬於特殊醫院代號，要用他的 API 去抓項目 (現在不動態抓 API 了，直接拿 DB 存的去顯示)
                var Result = new List<TblEEC_ApplyDetailPrice>();
                if (!string.IsNullOrEmpty(this.apply_no_sub))
                {
                    Result = new FrontDAO().GetRowList(new TblEEC_ApplyDetailPrice() { apply_no_sub = this.apply_no_sub })
                        .Where(x => x.his_type.TONotNullString() != "").ToList();
                }
                return Result;
            }
        }

        [NotDBField]
        public string his_types_Text
        {
            get
            {
                List<string> rtnList = new List<string>();
                if (this.hospital_code.TONotNullString() == "" || this.his_range1.TONotNullString() == "" || this.his_range2.TONotNullString() == "" || this.his_types.TONotNullString() == "") return "";
                if (this.hospital_code == "1131010011H")
                {
                    // 亞東
                    var findApi = this.HospApiList.Where(x => x.hospital_code == this.hospital_code && x.hospital_apikey == "A1").ToList();
                    if (findApi.ToCount() == 1)
                    {
                        foreach (var row in this.TheData_ApplyDetails)
                        {
                            var tmpObj = new EECOnline.Utils.Hospital_FarEastern_Api.Api_A1ResultModel();
                            tmpObj.InjectFrom(row);
                            tmpObj.ec_no = row.his_type;
                            tmpObj.ec_name = row.his_type_name;
                            rtnList.Add(row.his_type_name + EECOnline.Utils.Hospital_FarEastern_Api.Api_A1_Remark(tmpObj));
                        }
                    }
                }
                else
                if (this.hospital_code == "1317040011H")
                {
                    // 中山醫
                    foreach (var row in this.TheData_ApplyDetails)
                    {
                        var tmpObj = new EECOnline.Utils.Hospital_csh_Api.Api_A1ResultModel();
                        tmpObj.InjectFrom(row);
                        tmpObj.ec_no = row.his_type;
                        tmpObj.ec_name = row.his_type_name;
                        rtnList.Add(row.his_type_name + EECOnline.Utils.Hospital_csh_Api.Api_A1_Remark(tmpObj));
                    }
                }
                else
                if (this.hospital_code == "1317050017H")
                {
                    // 中國醫

                }
                else
                {
                    // 找不到時，才抓自己系統的
                    rtnList.AddRange(
                        new ShareCodeListModel().Get_HIS_Type_AllList()
                        .Where(x => this.his_types.Contains(x.Value))
                        .Select(x => x.Text)
                    );
                }
                return string.Join("<br />", rtnList);
            }
        }

        [NotDBField]
        public int? price_sum { get; set; }
        [NotDBField]
        public int? price_ck { get; set; }
        [NotDBField]
        public string createdatetime { get; set; }

        [NotDBField]
        public string createdatetime_Text
        {
            get
            {
                if (this.createdatetime.TONotNullString() == "") return "";
                var tmpDT = DateTime.ParseExact(this.createdatetime.TONotNullString(), "yyyy/MM/dd HH:mm:ss", null);
                tmpDT = tmpDT.AddYears(-1911);
                return tmpDT.ToString("yyy/MM/dd HH:mm:ss");
            }
        }

        [NotDBField]
        public string pay_deadline_Text
        {
            get
            {
                if (this.pay_deadline.TONotNullString() == "") return "";
                var tmpDT = DateTime.ParseExact(this.pay_deadline.TONotNullString(), "yyyy/MM/dd HH:mm:ss", null);
                tmpDT = tmpDT.AddYears(-1911);
                return tmpDT.ToString("yyy/MM/dd HH:mm:ss");
            }
        }

        [NotDBField]
        public string payed_datetime_Text
        {
            get
            {
                if (this.payed_datetime.TONotNullString() == "") return "";
                var tmpDT = DateTime.ParseExact(this.payed_datetime.TONotNullString(), "yyyy/MM/dd HH:mm:ss", null);
                tmpDT = tmpDT.AddYears(-1911);
                return tmpDT.ToString("yyy/MM/dd HH:mm:ss");
            }
        }
    }

    public class SearchApplyDetailModel
    {
        #region 記錄查詢頁欄位

        public string user_idno { get; set; }

        public string user_name { get; set; }

        public string user_name_Text
        {
            get
            {
                var Result = this.user_name.TONotNullString().ToCharArray();
                if (Result.Length == 2)
                    Result[1] = '*';
                else
                    for (int i = 1; i <= Result.Length - 2; i++) Result[i] = '*';
                return new string(Result);
            }
        }

        public string Search1Filter { get; set; }

        public string Search2Filter { get; set; }

        public string Search3Filter { get; set; }

        public string ActiveGridTab { get; set; }

        #endregion

        public long? keyid { get; set; }

        public string apply_no { get; set; }

        public string apply_no_sub { get; set; }

        public string hospital_code { get; set; }

        public string hospital_name { get; set; }

        public string his_types { get; set; }

        public string pay_deadline { get; set; }

        public string payed { get; set; }

        public string payed_datetime { get; set; }

        public string createdatetime { get; set; }

        [NotDBField]
        public string createdatetime_Text_NoTime
        {
            get
            {
                if (this.createdatetime.TONotNullString() == "") return "";
                var tmpDT = DateTime.ParseExact(this.createdatetime.TONotNullString(), "yyyy/MM/dd HH:mm:ss", null);
                tmpDT = tmpDT.AddYears(-1911);
                return tmpDT.ToString("yyy 年 MM 月 dd 日");
            }
        }

        [NotDBField]
        public string pay_deadline_Text
        {
            get
            {
                if (this.pay_deadline.TONotNullString() == "") return "";
                var tmpDT = DateTime.ParseExact(this.pay_deadline.TONotNullString(), "yyyy/MM/dd HH:mm:ss", null);
                tmpDT = tmpDT.AddYears(-1911);
                return tmpDT.ToString("yyy 年 MM 月 dd 日 HH:mm:ss");
            }
        }

        [NotDBField]
        public string payed_datetime_Text
        {
            get
            {
                if (this.payed_datetime.TONotNullString() == "") return "";
                var tmpDT = DateTime.ParseExact(this.payed_datetime.TONotNullString(), "yyyy/MM/dd HH:mm:ss", null);
                tmpDT = tmpDT.AddYears(-1911);
                return tmpDT.ToString("yyy 年 MM 月 dd 日 HH:mm:ss");
            }
        }

        /// <summary>記錄是哪一筆按下瀏覽病歷的 keyid</summary>
        public string HisView_keyid { get; set; }

        public IList<SearchApplyDetailPriceModel> DetailPrice { get; set; }
    }

    public class SearchApplyDetailPriceModel : TblEEC_ApplyDetailPrice
    {
        /// <summary>
        /// 檢查是否可以下載病歷檔案
        /// </summary>
        public bool CheckHisViewReady
        {
            get
            {
                FrontDAO dao = new FrontDAO();
                // 檢查該筆資料，是否屬於 特殊醫院別
                if (this.hospital_code == "1131010011H")
                {
                    // 亞東
                    if (this.ec_success_yn.TONotNullString() != "成功") return false;
                    var getSETUP = dao.GetRow(new TblSETUP() { setup_cd = "Hospital_FarEastern_Api", del_mk = "N" });
                    if (getSETUP != null)
                    {
                        var hisPath = getSETUP.setup_val.TONotNullString() + this.ec_fileName.TONotNullString();
                        if (!System.IO.File.Exists(hisPath)) return false;
                        return true;
                    }
                }
                else
                if (this.hospital_code == "1317040011H")
                {
                    // 中山醫
                    if (this.ec_success_yn.TONotNullString() != "成功") return false;
                    var getSETUP = dao.GetRow(new TblSETUP() { setup_cd = "Hospital_csh_Api", del_mk = "N" });
                    if (getSETUP != null)
                    {
                        var hisPath = getSETUP.setup_val.TONotNullString() + this.ec_fileName.TONotNullString();
                        if (!System.IO.File.Exists(hisPath)) return false;
                        return true;
                    }
                }
                else
                if (this.hospital_code == "1317050017H")
                {
                    // 中國醫

                }
                else
                {
                    // 是 一般 EEC
                    var getApiData = dao.GetRow(new TblEEC_ApplyDetailPrice_ApiData() { master_keyid = this.keyid });
                    if (getApiData == null) return false;
                    if (getApiData.Report_HTML.TONotNullString() == "")
                    {
                        if (this.provide_bin.TONotNullString() != "")
                        {
                            // 看看後台有沒有手動上傳檔案，有的話，就讓他下載
                            return true;
                        }
                        else
                        {
                            // 如果無文件，則立刻去 EEC 抓一次
                            // 1. 取得 API Token
                            //var apiToken = EECOnline.Utils.Hospital_Common_Api.GetLoginToken(ConfigModel.LoginUser, ConfigModel.LoginPwd);
                            // 2. 取得 病歷檔案(base64) 然後存入 DB
                            //EECOnline.Utils.Hospital_Common_Api.GetQueryContent_SaveIntoDB(apiToken, getApiData.Guid, getApiData.PatientIdNo, getApiData.AccessionNum, getApiData.HospitalId, getApiData.TemplateId);
                            // 3. 抓出 DB 的 病歷檔案(base64) 然後轉成 HTML 然後存入 DB
                            EECOnline.Utils.Hospital_Common_Api.TransXMLtoHTML(this.his_type, getApiData.Guid, getApiData.PatientIdNo, getApiData.AccessionNum, getApiData.HospitalId, getApiData.TemplateId);
                            // 再驗一次
                            var getApiData2 = dao.GetRow(new TblEEC_ApplyDetailPrice_ApiData() { master_keyid = this.keyid });
                            if (getApiData2 == null) return false;
                            if (getApiData2.Report_HTML.TONotNullString() == "") return false;
                        }
                    }
                    return true;
                }
                return false;
            }
        }

        public string Get_Api_A1_Remark
        {
            get
            {
                var tmpObj = new EECOnline.Utils.Hospital_FarEastern_Api.Api_A1ResultModel();
                tmpObj.InjectFrom(this);
                return EECOnline.Utils.Hospital_FarEastern_Api.Api_A1_Remark(tmpObj);
            }
        }
    }

    public class SuccessPageModel
    {
        public string apply_no_sub { get; set; }
        public string hospital_code { get; set; }
        public string hospital_name { get; set; }
    }

    public class ContactUsModel : TblContactUs
    {
        public string ValidateCode { get; set; }

        public IList<SelectListItem> Type_list
        {
            get
            {
                var list = new List<SelectListItem>();
                list.Add(new SelectListItem() { Value = "", Text = "<請選擇>" });
                list.Add(new SelectListItem() { Value = "線上申辦相關", Text = "線上申辦相關" });
                list.Add(new SelectListItem() { Value = "線上付款相關", Text = "線上付款相關" });
                list.Add(new SelectListItem() { Value = "進度查詢相關", Text = "進度查詢相關" });
                list.Add(new SelectListItem() { Value = "下載病歷相關", Text = "下載病歷相關" });
                list.Add(new SelectListItem() { Value = "其他", Text = "其他" });
                return list;
            }
        }
    }

    /// <summary>中山醫 合庫 授權交易輸出欄位</summary>
    public class Success_csh_AuthRespModel
    {
        /// <summary>授權結果狀態。</summary>
        public string status { get; set; }

        /// <summary>錯誤代碼。</summary>
        public string errcode { get; set; }

        /// <summary>交易授權碼。</summary>
        public string authCode { get; set; }

        /// <summary>授權金額(臺幣金額整數)。</summary>
        public string authAmt { get; set; }

        /// <summary>
        /// 交易訂單編號。
        /// 信用卡交易，最大長度19位。
        /// 銀聯卡交易，最小長度8位，最大長度19位。
        /// 消費扣款交易，最大長度16位。
        /// </summary>
        public string lidm { get; set; }

        /// <summary>交易追蹤碼(此處回傳xid非3D網路交易序號)。</summary>
        public string xid { get; set; }

        /// <summary>幣別(901)。</summary>
        public string currency { get; set; }

        /// <summary>幣值指數(小數點三位)。</summary>
        public string amtExp { get; set; }

        /// <summary>網站特店自訂代碼(請注意merID與MerchantID不同)。</summary>
        public string merID { get; set; }

        /// <summary>
        /// 特店網站或公司名稱，僅供顯示。
        /// 銀聯交易限定僅能為英、數字、空白及『-』，最大長度25位。
        /// </summary>
        public string MerchantName { get; set; }

        /// <summary>授權失敗原因說明。</summary>
        public string errDesc { get; set; }

        /// <summary>持卡者交易的信用卡號末四碼。</summary>
        public string lastPan4 { get; set; }

        /// <summary>持卡者交易的信用卡別，如VISA、MasterCard、JCB。</summary>
        public string cardBrand { get; set; }

        /// <summary>遮罩後之信用卡卡號。</summary>
        public string pan { get; set; }

        /// <summary>
        /// 授權處理回應時間，格式為YYYYMMDDHHMMSS。
        /// 銀聯交易處理回應時間格式為YYYY/MM/DDHH:MM:SS。
        /// </summary>
        public string authRespTime { get; set; }

        /// <summary>交易類別碼。</summary>
        public string PayType { get; set; }

        /// <summary>分期交易之期數。</summary>
        public string PeriodNum { get; set; }

        /// <summary>分期交易之首期分期金額，含2位小數。</summary>
        public string DownPayments { get; set; }

        /// <summary>分期交易之每期金額，含2位小數。</summary>
        public string InstallmentPayments { get; set; }

        /// <summary>紅利交易活動代碼。</summary>
        public string BonusActionCode { get; set; }

        /// <summary>
        /// 紅利折抵方式。
        /// 0表示依發卡行決定(預設)。
        /// 1表示不折抵。
        /// 2表示全數折抵。
        /// 3表示部份折抵。
        /// </summary>
        public string BonusDesc { get; set; }

        /// <summary>紅利交易授權結果(00為成功)。</summary>
        public string BonusRespCode { get; set; }

        /// <summary>
        /// 紅利剩餘點數註記。
        /// P表示紅利餘額點數為正數。
        /// N表示紅利餘額點數為負數。
        /// </summary>
        public string BonusSign { get; set; }

        /// <summary>紅利餘額點數。</summary>
        public string BonusBalance { get; set; }

        /// <summary>扣抵紅利點數。</summary>
        public string BonusDeduct { get; set; }

        /// <summary>紅利扣抵後交易金額，含2位小數。</summary>
        public string BonusDebuctAmt { get; set; }

        /// <summary>交易驗證碼。</summary>
        public string respToken { get; set; }

        /// <summary>交易日期。</summary>
        public string txnDateLocal { get; set; }

        /// <summary>交易時間。</summary>
        public string txnTimeLocal { get; set; }

        /// <summary>系統追蹤號碼。</summary>
        public string Srrn { get; set; }

        /// <summary>
        /// 回應碼。
        /// 銀聯交易的此欄位值為00表示成功。
        /// </summary>
        public string respCode { get; set; }

        /// <summary>回應結果描述。</summary>
        public string respMsg { get; set; }

        /// <summary>由銀聯回覆之交易識別值。</summary>
        public string qid { get; set; }
    }
}