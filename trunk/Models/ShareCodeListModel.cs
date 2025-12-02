using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Turbo.Commons;
using Turbo.DataLayer;
using EECOnline.Commons;
using EECOnline.DataLayers;
using System.Collections;
using EECOnline.Models.Entities;
using EECOnline.Services;

namespace EECOnline.Models
{
    /// <summary>
    /// 共用代碼統一模型(所有下拉選單放置)
    /// </summary>
    public class ShareCodeListModel
    {
        #region 常用(是否、停/使用、....)

        /// <summary>
        /// 否 / 是
        /// </summary>
        public IList<SelectListItem> YorN_list
        {
            get
            {
                var dictionary = new Dictionary<string, string> {
                    {"1", "是"},
                    {"0", "否"}
                };

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 停 / 使用
        /// </summary>
        public IList<SelectListItem> StoporStart_list
        {
            get
            {
                var dictionary = new Dictionary<string, string> {
                    {"1", "使用"},
                    {"0", "停用"},
                    {"2", "帳號須變更密碼"},
                };

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 停 / 使用
        /// </summary>
        public IList<SelectListItem> StartorEed_list
        {
            get
            {
                var dictionary = new Dictionary<string, string> {
                    {"1", "使用"},
                    {"0", "停用"},
                };

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 停 / 使用
        /// </summary>
        public IList<SelectListItem> UorS_list
        {
            get
            {
                var dictionary = new Dictionary<string, string> {
                    {"", "請選擇"},
                    {"1", "使用"},
                    {"0", "停用"}
                };

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 性別
        /// </summary>
        public IList<SelectListItem> sex_list
        {
            get
            {
                var dictionary = new Dictionary<string, string> {
                    {"1", "男"},
                    {"2", "女"},
                    {"3", "不透漏"}
                };

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        public IList<SelectListItem> sex_list2
        {
            get
            {
                var dictionary = new Dictionary<string, string> {
                    { "M", "男" },
                    { "F", "女" },
                };
                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        #endregion

        /// <summary>
        /// 取得縣市(前台申辦案件)
        /// </summary>
        public IList<SelectListItem> E_srv_city_list(object parms)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.E_SRV_CITY, parms);
            KeyMapModel item = new KeyMapModel();
            var newlist = list;
            item.TEXT = "請選擇";
            item.CODE = "";
            newlist.Add(item);
            newlist = newlist.OrderBy(m => m.CODE).ToList();
            return MyCommonUtil.ConvertSelItems(newlist);
        }

        /// <summary>
        /// 取得縣市(前台申辦案件)
        /// </summary>
        public IList<SelectListItem> E_srv_city_parm_list(object parms)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.srv_city_parm, parms);
            KeyMapModel item = new KeyMapModel();
            var newlist = list;
            item.TEXT = "請選擇";
            item.CODE = "";
            newlist.Insert(0, item);
            return MyCommonUtil.ConvertSelItems(newlist);
        }

        /// <summary>
        /// 取得縣市轄區(前台申辦案件)
        /// </summary>
        public IList<SelectListItem> E_srv_city_s_parm_list(object parms)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.srv_city_s_parm, parms);
            KeyMapModel item = new KeyMapModel();
            var newlist = list;
            item.TEXT = "請選擇";
            item.CODE = "";
            newlist.Add(item);
            newlist = newlist.OrderBy(m => m.CODE).ToList();
            return MyCommonUtil.ConvertSelItems(newlist);
        }

        /// <summary>
        /// 系統名稱清單
        /// </summary>
        public IList<SelectListItem> sysid_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.sysid);
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 模組名稱清單(根據SYSID)
        /// </summary>
        public IList<SelectListItem> modules_list(object parms)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.modules, parms);
            return MyCommonUtil.ConvertSelItems(list);
        }

        /// <summary>
        /// 取得單位資料
        /// </summary>
        public IList<SelectListItem> unit_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.unit);
                KeyMapModel item = new KeyMapModel();
                var newlist = list;
                item.TEXT = "請選擇";
                item.CODE = "";
                newlist.Add(item);
                newlist = newlist.OrderBy(m => m.CODE).ToList();
                return MyCommonUtil.ConvertSelItems(newlist);
            }
        }

        /// <summary>
        /// 取得單位資料1
        /// </summary>
        public IList<SelectListItem> unit_list1(object parms)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.unit1, parms);
            return MyCommonUtil.ConvertSelItems(list);
        }

        /// <summary>
        /// 取得功能群組
        /// </summary>
        public IList<SelectListItem> amgrp_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.amgrp);
                KeyMapModel item = new KeyMapModel();
                var newlist = list;
                item.TEXT = "請選擇";
                item.CODE = "";
                newlist.Add(item);
                newlist = newlist.OrderBy(m => m.CODE).ToList();
                return MyCommonUtil.ConvertSelItems(newlist);
            }
        }

        /// <summary>
        /// 取得案件
        /// </summary>
        public IList<SelectListItem> apply_list(object parms)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.apply, parms);
            KeyMapModel item = new KeyMapModel();
            var newlist = list;
            item.TEXT = "請選擇";
            item.CODE = "";
            newlist.Add(item);
            newlist = newlist.OrderBy(m => m.CODE).ToList();
            return MyCommonUtil.ConvertSelItems(newlist);
        }

        /// <summary>
        /// 取得縣市
        /// </summary>
        public IList<SelectListItem> srv_city_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.srv_city);
                KeyMapModel item = new KeyMapModel();
                var newlist = list;
                item.TEXT = "請選擇";
                item.CODE = "";
                newlist.Add(item);
                newlist = newlist.OrderBy(m => m.CODE).ToList();
                return MyCommonUtil.ConvertSelItems(newlist);
            }
        }

        /// <summary>
        /// 取得縣市_單位權限
        /// </summary>
        public IList<SelectListItem> srv_city_unit_list(object parms)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.srv_city_unit, parms);
            KeyMapModel item = new KeyMapModel();
            var newlist = list;
            item.TEXT = "請選擇";
            item.CODE = "";
            newlist.Add(item);
            newlist = newlist.OrderBy(m => m.CODE).ToList();
            return MyCommonUtil.ConvertSelItems(newlist);
        }

        /// <summary>
        /// 項目類別
        /// </summary>
        public IList<SelectListItem> slt_type_list
        {
            get
            {
                var dictionary = new Dictionary<string, string> {
                    {"0", "否"},
                    {"1", "網路申辦"},
                    {"2", "臨櫃申辦"},
                    {"3", "書表下載"},
                };

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 主旨
        /// </summary>
        public IList<SelectListItem> apply_subject_list
        {
            get
            {
                var dictionary = new Dictionary<string, string> {
                    {"", "請選擇"},
                    {"1", "應備證件"},
                    {"2", "申請方式"},
                    {"3", "繳費方式"},
                    {"4", "處理時限"},
                    {"5", "承辦單位"},
                    {"6", "備註"},
                    {"7", "相關連結"},
                    {"8", "檔案下載"},
                };

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 狀態所屬種類
        /// </summary>
        public IList<SelectListItem> applyhardtype_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.applyhardtype);
                KeyMapModel item = new KeyMapModel();
                var newlist = list;
                item.TEXT = "請選擇";
                item.CODE = "";
                newlist.Add(item);
                newlist = newlist.OrderBy(m => m.CODE).ToList();
                return MyCommonUtil.ConvertSelItems(newlist);
            }
        }

        /// <summary>
        /// 處理狀態
        /// </summary>
        public IList<SelectListItem> srv_status_list(object parms)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.srv_status, parms);
            KeyMapModel item = new KeyMapModel();
            var newlist = list;
            item.TEXT = "請選擇";
            item.CODE = "";
            newlist.Add(item);
            newlist = newlist.OrderBy(m => m.CODE).ToList();
            return MyCommonUtil.ConvertSelItems(newlist);
        }

        /// <summary>
        /// 處理狀態
        /// </summary>
        public IList<SelectListItem> srv_status_list1
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.srv_status1);
                KeyMapModel item = new KeyMapModel();
                var newlist = list;
                item.TEXT = "請選擇";
                item.CODE = "";
                newlist.Add(item);
                newlist = newlist.OrderBy(m => m.CODE).ToList();
                return MyCommonUtil.ConvertSelItems(newlist);
            }
        }

        /// <summary>
        /// 輸入方式
        /// </summary>
        public IList<SelectListItem> gettype_list
        {
            get
            {
                var dictionary = new Dictionary<string, string> {
                    {"1", "輸入地址"},
                    {"2", "顯示地址"},
                };

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 承辦人清單(分文專用)
        /// </summary>
        public IList<SelectListItem> apy_undertaker_list(object parms)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.apy_undertaker, parms);
            KeyMapModel item = new KeyMapModel();
            var newlist = list;
            item.TEXT = "請選擇";
            item.CODE = "";
            newlist.Add(item);
            newlist = newlist.OrderBy(m => m.CODE).ToList();
            return MyCommonUtil.ConvertSelItems(newlist);
        }

        /// <summary>
        /// 取得公告類型
        /// </summary>
        public IList<SelectListItem> enews_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.enews);
                KeyMapModel item = new KeyMapModel();
                var newlist = list;
                item.TEXT = "請選擇";
                item.CODE = "";
                newlist.Add(item);
                newlist = newlist.OrderBy(m => m.CODE).ToList();
                return MyCommonUtil.ConvertSelItems(newlist);
            }
        }

        /// <summary>
        /// 取得常見問題類型
        /// </summary>
        public IList<SelectListItem> code_name_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.code_name);
                KeyMapModel item = new KeyMapModel();
                var newlist = list;
                item.TEXT = "請選擇";
                item.CODE = "";
                newlist.Add(item);
                newlist = newlist.OrderBy(m => m.CODE).ToList();
                return MyCommonUtil.ConvertSelItems(newlist);
            }
        }

        /// <summary>
        /// 取得常見問題類型(不顯示停用)
        /// </summary>
        public IList<SelectListItem> code_name_list1
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.code_name1);
                KeyMapModel item = new KeyMapModel();
                var newlist = list;
                item.TEXT = "請選擇";
                item.CODE = "";
                newlist.Add(item);
                newlist = newlist.OrderBy(m => m.CODE).ToList();
                return MyCommonUtil.ConvertSelItems(newlist);
            }
        }

        /// <summary>
        /// 上架
        /// </summary>
        public IList<SelectListItem> status_list
        {
            get
            {
                var dictionary = new Dictionary<string, string> {
                    {"", "請選擇"},
                    {"1", "是"},
                    {"0", "否"},
                };

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 寄信狀態
        /// </summary>
        public IList<SelectListItem> mail_status_list
        {
            get
            {
                var dictionary = new Dictionary<string, string> {
                    {"", "請選擇"},
                    {"1", "成功"},
                    {"0", "失敗"},
                };

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 空白
        /// </summary>
        public IList<SelectListItem> Blank_list
        {
            get
            {
                var dictionary = new Dictionary<string, string> {
                   {"", "請選擇"},
                };

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 登記事項
        /// </summary>
        public IList<SelectListItem> Apy_change_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.Apy_change);
                KeyMapModel item = new KeyMapModel();
                var newlist = list;
                item.TEXT = "請選擇";
                item.CODE = "";
                newlist.Add(item);
                newlist = newlist.OrderBy(m => m.CODE).ToList();
                return MyCommonUtil.ConvertSelItems(newlist);
            }
        }

        /// <summary>
        /// 操作狀態
        /// (0: 查詢, 1: 新增, 2:修改, 3:刪除)
        /// </summary>
        public IList<SelectListItem> use_type_list
        {
            get
            {
                var dictionary = new Dictionary<string, string> {
                    {string.Empty, "請選擇"},
                    {"0", "查詢"},
                    {"1", "新增"},
                    {"2", "修改"},
                    {"3", "刪除"},
                };

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 操作功能
        /// (0:前臺, 1:後臺)
        /// </summary>
        public IList<SelectListItem> urlwhere_list
        {
            get
            {
                var dictionary = new Dictionary<string, string>
                {
                    { string.Empty, "請選擇"},
                    { "0", "前臺"},
                    { "1", "後臺"}
                };

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 信件功能
        /// (1: 忘記密碼, 2: 前臺案件申辦, 3: 後臺案件變更)
        /// </summary>
        public IList<SelectListItem> usedfunc_list
        {
            get
            {
                var dictionary = new Dictionary<string, string>
                {
                    { string.Empty, "請選擇"},
                    { "1", "忘記密碼"},
                    { "2", "前臺案件申辦"},
                    { "3", "後臺案件申辦"}
                };

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 病床種類
        /// </summary>
        public IList<SelectListItem> Bed_type_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.Bed_type);
                KeyMapModel item = new KeyMapModel();
                var newlist = list;
                item.TEXT = "請選擇";
                item.CODE = "";
                newlist.Add(item);
                newlist = newlist.OrderBy(m => m.CODE).ToList();
                return MyCommonUtil.ConvertSelItems(newlist);
            }
        }

        /// <summary>
        /// 病床類型
        /// </summary>
        public IList<SelectListItem> Bed_kind_list(object parms)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.Bed_kind, parms);
            KeyMapModel item = new KeyMapModel();
            var newlist = list;
            item.TEXT = "請選擇";
            item.CODE = "";
            newlist.Add(item);
            newlist = newlist.OrderBy(m => m.CODE).ToList();
            return MyCommonUtil.ConvertSelItems(newlist);
        }

        /// <summary>
        /// 縣市清單(地址)
        /// </summary>
        public IList<SelectListItem> Zip_City_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.Zip_City);
                KeyMapModel item = new KeyMapModel();
                var newlist = list;
                item.TEXT = "請選擇";
                item.CODE = "";
                newlist.Add(item);
                newlist = newlist.OrderBy(m => m.CODE).ToList();
                return MyCommonUtil.ConvertSelItems(newlist);
            }
        }

        /// <summary>
        /// 鄉鎮區清單(地址)
        /// </summary>
        public IList<SelectListItem> Zip_Town_list(object parms)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.Zip_Town, parms);
            KeyMapModel item = new KeyMapModel();
            var newlist = list;
            item.TEXT = "請選擇";
            item.CODE = "";
            newlist.Add(item);
            newlist = newlist.OrderBy(m => m.CODE).ToList();
            return MyCommonUtil.ConvertSelItems(newlist);
        }

        /// <summary>
        /// 街道清單(地址)
        /// </summary>
        public IList<SelectListItem> Zip_Road_list(object parms)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.Zip_Road, parms);
            KeyMapModel item = new KeyMapModel();
            var newlist = list;
            item.TEXT = "請選擇";
            item.CODE = "";
            newlist.Add(item);
            newlist = newlist.OrderBy(m => m.CODE).ToList();
            return MyCommonUtil.ConvertSelItems(newlist);
        }

        /// <summary>
        /// 取得縣市
        /// </summary>
        public IList<SelectListItem> srv_city_list1
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                KeyMapModel item = new KeyMapModel();
                IList<KeyMapModel> list = new List<KeyMapModel>();
                var newlist = list;
                item.TEXT = "請選擇";
                item.CODE = "";
                newlist.Add(item);
                item = new KeyMapModel();
                item.TEXT = "系統管理";
                item.CODE = "00";
                newlist.Add(item);
                var data = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.srv_city);
                foreach (var q in data)
                {
                    item = new KeyMapModel();
                    item.TEXT = q.TEXT;
                    item.CODE = q.CODE;
                    newlist.Add(item);
                }
                return MyCommonUtil.ConvertSelItems(newlist);
            }
        }

        /// <summary>
        /// 取得縣市轄區
        /// </summary>
        public IList<SelectListItem> srv_city_s_list(string parms)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            FrontDAO dao1 = new FrontDAO();
            KeyMapModel item = new KeyMapModel();
            IList<KeyMapModel> list = new List<KeyMapModel>();
            item.TEXT = "請選擇";
            item.CODE = "";
            list.Add(item);

            TblCODE cd = new TblCODE();
            cd.code_cd = parms.TONotNullString();
            cd.code_type = "CITYS";
            var data = dao1.GetRowList(cd);
            foreach (var q in data)
            {
                item = new KeyMapModel();
                item.TEXT = q.code_name;
                item.CODE = q.code_cd;
                list.Add(item);
            }
            return MyCommonUtil.ConvertSelItems(list);

        }

        /// <summary>
        /// 醫院清單
        /// </summary>
        /// <returns></returns>
        public IList<SelectListItem> Get_Hospital_list(bool isNeedBlank = false)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            var list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.Get_Hospital, null);
            if (isNeedBlank) list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "<請選擇>" });
            // 只顯示這些清單的醫院就好
            string[] JustOnly = {
                "1131010011H",  // 亞東
                "1317040011H",  // 中山醫院
                //"1317050017H",  // 中國醫藥
            };
            list = list.Where(x => JustOnly.Contains(x.CODE)).ToList();
            return MyCommonUtil.ConvertSelItems(list);
        }

        /// <summary>
        /// 病歷類型 (全顯示)
        /// </summary>
        /// <returns></returns>
        public IList<SelectListItem> Get_HIS_Type_AllList()
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            var list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.Get_HIS_Type_All, null);
            return MyCommonUtil.ConvertSelItems(list);
        }

        /// <summary>
        /// 病歷類型 (僅顯示有效期內，須傳入醫院代號)
        /// </summary>
        /// <returns></returns>
        public IList<SelectListItem> Get_HIS_Type_ValidList(string HospitalCode)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            Hashtable parmas = new Hashtable();
            parmas["hospital_code"] = HospitalCode;
            var list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.Get_HIS_Type_Valid, parmas);
            return MyCommonUtil.ConvertSelItems(list);
        }

        /// <summary>
        /// 病歷類型 取單價 (僅顯示有效期內，須傳入醫院代號)
        /// </summary>
        /// <returns></returns>
        public IList<SelectListItem> Get_HIS_Type_ValidList_Price(string HospitalCode)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            Hashtable parmas = new Hashtable();
            parmas["hospital_code"] = HospitalCode;
            var list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.Get_HIS_Type_Valid_Price, parmas);
            return MyCommonUtil.ConvertSelItems(list);
        }

        public IList<SelectListItem> Get_SearchFilter_list(bool isNeedBlank = false)
        {
            var dictionary = new Dictionary<string, string> {
                { "1", "一個月" },
                { "3", "三個月" },
                { "6", "六個月" },
            };
            var list = MyCommonUtil.ConvertSelItems(dictionary);
            if (isNeedBlank) list.Insert(0, new SelectListItem() { Value = "", Text = "<請選擇>" });
            return list;
        }

        /// <summary>
        /// EEC_ApplyDetailPrice.provide_status<br />代表有無取得 EEC base64 XML 檔案之狀態
        /// </summary>
        /// <returns></returns>
        public IList<SelectListItem> provide_status_list()
        {
            var dictionary = new Dictionary<string, string> {
                { "0", "尚未提供" },
                { "1", "已提供" },
                { "2", "無法提供" },
            };
            return MyCommonUtil.ConvertSelItems(dictionary);
        }

        /// <summary>
        /// 1: 自然人憑證登入<br />
        /// 2: 行動自然人憑證登入 (TW FidO)<br />
        /// 3: 身分證字號 + 健保卡
        /// </summary>
        /// <param name="isNeedBlank"></param>
        /// <returns></returns>
        public IList<SelectListItem> Get_login_type_list(bool isNeedBlank = false)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            var list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.Get_login_type, null);
            if (isNeedBlank) list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "<請選擇>" });
            return MyCommonUtil.ConvertSelItems(list);
        }

        /// <summary>
        /// 帳務月份列表 - 依照申請訂單 createdatetime 為主
        /// </summary>
        /// <param name="isNeedBlank"></param>
        /// <returns></returns>
        public IList<SelectListItem> Get_AccountingYM_list(bool isNeedBlank = false)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            var list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.Get_AccountingYM, null);
            if (isNeedBlank) list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "<請選擇>" });
            return MyCommonUtil.ConvertSelItems(list);
        }
    }
}