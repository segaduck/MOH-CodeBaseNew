using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Commons;
using ES.DataLayers;
using System.Collections;
using ES.Services;
using System.Globalization;

namespace ES.Models
{
    /// <summary>
    /// 共用代碼統一模型(所有下拉選單放置)
    /// </summary>
    public class ShareCodeListModel
    {
        /// <summary>
        /// 範例1:使用CodeMap產生 KeyMapModel屬性{CODE:"",TEXT:""}
        /// </summary>
        //public IList<SelectListItem> OPCD_list
        //{
        //    get
        //    {
        //        MyKeyMapDAO dao = new MyKeyMapDAO();
        //        IList<KeyMapModel> list = dao.GetCodeMapList();
        //        return MyCommonUtil.ConvertSelItems(list);
        //    }
        //}

        /// <summary>
        /// 範例3:產生Checkboxlist/普通
        /// </summary>
        //public IList<CheckBoxListItem> ABC_list
        //{
        //    get
        //    {
        //        MyKeyMapDAO dao = new MyKeyMapDAO();
        //        IList<KeyMapModel> list = dao.GetCodeMapList(Commons.StaticCodeMap.CodeMap.OPECOM);

        //        return MyCommonUtil.ConvertCheckBoxItems(list);
        //    }
        //}

        /// <summary>
        /// 範例2:直接產生使用Dictionary/普通
        /// </summary>
        //public IList<SelectListItem> Sample_list
        //{
        //    get
        //    {
        //        var dictionary = new Dictionary<string, string> {
        //            {"Y", "是"},
        //            {"N", "否"}
        //        };

        //        return MyCommonUtil.ConvertSelItems(dictionary);
        //    }
        //}

        /// <summary>
        /// 加入會員_性別
        /// </summary>
        public IList<SelectListItem> SEX_list
        {
            get
            {
                var dictionary = new Dictionary<string, string> {
                    {"", "請選擇"},
                    {"M", "男"},
                    {"F", "女"},
                    {"O", "其他"}
                };

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 性別(含其他)
        /// </summary>
        public IList<SelectListItem> SEX2_list
        {
            get
            {
                var dictionary = new Dictionary<string, string> {
                    {"M", "男"},
                    {"F", "女"},
                    {"O", "其他"}
                };

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 申請角色
        /// </summary>
        public IList<SelectListItem> APP_ROLEList
        {
            get
            {
                var dictionary = new Dictionary<string, string> {
                    {"0", "自辦"},
                    {"1", "代理"},
                };

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 代理人
        /// </summary>
        public IList<SelectListItem> A_AGENTList
        {
            get
            {
                var dictionary = new Dictionary<string, string> {
                    {"0", "自然人"},
                    {"1", "法人"},
                };

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 國家清單
        /// </summary>
        public IList<SelectListItem> CountryList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetPort2MapList(); //dao.GetCountryMapList();
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 國家清單 優先順序
        /// </summary>
        public IList<SelectListItem> Country2List
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCountry2MapList();
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 查詢 2A.1藥品許可證字號
        /// </summary>
        public IList<KeyMapModel> PList2A1_CODE
        {
            get
            {
                //衛署成製字(DOH-OM)、衛署藥製字(DOH-PM)、衛部成製字(MOHW-OM)、衛部藥製字(MOHW-PM)。
                //MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = new List<KeyMapModel>();
                list.Add(new KeyMapModel() { CODE = "DOH-OM", TEXT = "衛署成製字" });
                list.Add(new KeyMapModel() { CODE = "DOH-PM", TEXT = "衛署藥製字" });
                list.Add(new KeyMapModel() { CODE = "MOHW-OM", TEXT = "衛部成製字" });
                list.Add(new KeyMapModel() { CODE = "MOHW-PM", TEXT = "衛部藥製字" });
                //list.Add(new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                //list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return list;
            }
        }

        /// <summary>
        /// 2B.3僅供外銷專用之原因
        /// </summary>
        public IList<KeyMapModel> PList2B3_CODE
        {
            get
            {
                //衛署成製字(DOH-OM)、衛署藥製字(DOH-PM)、衛部成製字(MOHW-OM)、衛部藥製字(MOHW-PM)。
                //MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = new List<KeyMapModel>();
                list.Add(new KeyMapModel() { CODE = "1", TEXT = "1.Not required. (沒有被要求)" });
                list.Add(new KeyMapModel() { CODE = "2", TEXT = "2.Not requested. (沒有需求)" });
                list.Add(new KeyMapModel() { CODE = "3", TEXT = "3.Under  consideration. (尚在申請中)" });
                list.Add(new KeyMapModel() { CODE = "4", TEXT = "4.Refused. (不被核准)" });
                //list.Add(new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                //list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return list;
            }
        }

        public IList<KeyMapModel> PList2B3_ECODE
        {
            get
            {
                //衛署成製字(DOH-OM)、衛署藥製字(DOH-PM)、衛部成製字(MOHW-OM)、衛部藥製字(MOHW-PM)。
                //MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = new List<KeyMapModel>();
                list.Add(new KeyMapModel() { CODE = "1", TEXT = "Not required." });
                list.Add(new KeyMapModel() { CODE = "2", TEXT = "Not requested." });
                list.Add(new KeyMapModel() { CODE = "3", TEXT = "Under consideration." });
                list.Add(new KeyMapModel() { CODE = "4", TEXT = "Refused." });
                //list.Add(new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                //list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return list;
            }
        }

        /// <summary>
        /// 查詢 2A.1藥品許可證字號
        /// </summary>
        public IList<SelectListItem> PList2A1
        {
            get
            {
                IList<SelectListItem> list = new List<SelectListItem>();
                foreach (KeyMapModel item in PList2A1_CODE)
                {
                    list.Add(new SelectListItem() { Value = item.CODE, Text = item.TEXT });
                }
                list.Insert(0, new SelectListItem() { Value = "", Text = "請選擇" });
                return list;
            }
        }

        /// <summary>
        /// 2B.3僅供外銷專用之原因
        /// </summary>
        public IList<SelectListItem> PList2B3
        {
            get
            {
                IList<SelectListItem> list = new List<SelectListItem>();
                foreach (KeyMapModel item in PList2B3_CODE)
                {
                    list.Add(new SelectListItem() { Value = item.CODE, Text = item.TEXT });
                }
                list.Insert(0, new SelectListItem() { Value = "", Text = "請選擇" });
                return list;
            }
        }


        /// <summary>
        /// 查詢 藥商許可執照字號
        /// </summary>
        public IList<SelectListItem> PList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeList("F5_LIC_CD");
                //list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 查詢 藥商許可執照字號(英文)
        /// </summary>
        public IList<SelectListItem> PList_E
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetPLList_EN();
                //list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 港口
        /// </summary>
        public IList<SelectListItem> PortList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetPortMapList();
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 港口 優先順序
        /// </summary>
        public IList<SelectListItem> Port2List
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetPort2MapList();
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 岸口
        /// </summary>
        public IList<SelectListItem> HarborList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetHarborMapList();
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 進口關
        /// </summary>
        public IList<SelectListItem> LocalPortList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetLocalPortList();
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 申請用途
        /// </summary>
        public IList<SelectListItem> GetAPP_USEList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeList("F1_APP_USE_1");
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 申請用途(生殖細胞+非感染)
        /// </summary>
        public IList<SelectListItem> GetAPP_USE3List
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeList("F1_APP_USE_3");
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 核發方式
        /// </summary>
        public IList<SelectListItem> GetCONF_TYPEList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeList("F1_CONF_TYPE_1");
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }
        /// <summary>
        /// 貨品類別（規格）
        /// </summary>
        public IList<SelectListItem> GetGOODS_TYPEList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeList("F1_GOODS_TYPE_1");
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 貨品類別（規格） 生殖細胞
        /// </summary>
        public IList<SelectListItem> GetGOODS_TYPE3List
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeList("F1_GOODS_TYPE_3");
                list =  list.Where(x => x.CODE.TOInt32() == 300).ToList();
                list.FirstOrDefault().TEXT = "取自委託人之生殖細胞或胚胎";
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 貨品類別（規格）非感染性
        /// </summary>
        public IList<SelectListItem> GetGOODS_TYPEList2
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeList("F1_GOODS_TYPE_2");
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 貨品單位
        /// </summary>
        public IList<SelectListItem> GetGOODS_UNIT_IDList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeList("F1_GOODS_UNIT_1");
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// E-mail Domain
        /// </summary>
        public IList<SelectListItem> GetMailDomainList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = new List<KeyMapModel>();
                list.Add(new KeyMapModel() { CODE = "1", TEXT = "gmail.com" });
                list.Add(new KeyMapModel() { CODE = "2", TEXT = "yahoo.com.tw" });
                list.Add(new KeyMapModel() { CODE = "3", TEXT = "outlook.com" });
                list.Add(new KeyMapModel() { CODE = "0", TEXT = "自訂" });
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// E-mail Domain
        /// </summary>
        public IList<SelectListItem> GetMailDomainList1
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = new List<KeyMapModel>();
                list.Add(new KeyMapModel() { CODE = "gmail.com", TEXT = "gmail.com" });
                list.Add(new KeyMapModel() { CODE = "yahoo.com.tw", TEXT = "yahoo.com.tw" });
                list.Add(new KeyMapModel() { CODE = "outlook.com", TEXT = "outlook.com" });
                list.Add(new KeyMapModel() { CODE = "0", TEXT = "自訂" });
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 申辦份數(1-10)
        /// </summary>
        public IList<SelectListItem> GetPayCountSelect
        {
            get
            {
                //MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<SelectListItem> list = new List<SelectListItem>();
                for (int ix = 1; ix <= 10; ix++)
                {
                    SelectListItem tmp1 = new SelectListItem() { Value = ix.ToString(), Text = ix.ToString() };
                    list.Add(tmp1);
                }
                return list;//MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 3.1接受定期查核之週期為何
        /// </summary>
        public IList<SelectListItem> PListF31
        {
            get
            {
                //MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<SelectListItem> list = new List<SelectListItem>();
                for (int ix = 1; ix <= 10; ix++)
                {
                    string tmpV = string.Format("{0}", ix);
                    SelectListItem tmp1 = new SelectListItem() { Value = tmpV, Text = tmpV };
                    list.Add(tmp1);
                }
                return list;//MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// GMP 申辦案件類別
        /// </summary>
        public IList<SelectListItem> GetF005004ApplyType
        {
            get
            {
                IList<KeyMapModel> list = new List<KeyMapModel>();
                list.Add(new KeyMapModel() { CODE = "新申請", TEXT = "新申請" });
                list.Add(new KeyMapModel() { CODE = "遺失補發", TEXT = "遺失補發" });
                list.Add(new KeyMapModel() { CODE = "汙損換發", TEXT = "汙損換發" });

                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 是否
        /// </summary>
        public IList<SelectListItem> YorN_list
        {
            get
            {
                var dictionary = new Dictionary<string, string> {
                    {"Y", "是"},
                    {"N", "否"}
                };
                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 查詢 藥劑代碼
        /// </summary>
        public IList<SelectListItem> tra_con_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeList("F5_GMP_FORM");
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 查詢 醫師司案件狀態（只查詢要設定的狀態）
        /// </summary>
        public IList<SelectListItem> GetStatuListForDEP
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetDEPCaseStatusList("4");
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 查詢 國健署專科護理師案件狀態
        /// </summary>
        public IList<SelectListItem> GetStatuListForNurse
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetStatuListForNurse("55");
                return MyCommonUtil.ConvertSelItems(list);
            }
        }


        /// <summary>
        /// 查詢 醫師司案件狀態
        /// </summary>
        public IList<SelectListItem> GetStatuListForUnitCD4
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCaseStatusList("4");
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 查詢 護理及健康照護司案件狀態
        /// </summary>
        public IList<SelectListItem> GetStatuListForUnitCD55
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCaseStatusList("55");
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 查詢 中醫藥司案件狀態
        /// </summary>
        public IList<SelectListItem> GetStatuListForUnitCD7
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCaseStatusList("7");
                //list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 查詢 中醫藥司案件狀態(CODE_MEMO)
        /// </summary>
        public IList<SelectListItem> GetStatuMemoList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCaseMemoStatusList("7");
                //list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 查詢 社工司案件狀態
        /// </summary>
        public IList<SelectListItem> GetStatuListForUnitCD8
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCaseStatusList("8");
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                // 低收入戶專用狀態
                list.Remove(list.Where(m => m.CODE == "10").FirstOrDefault());
                list.Remove(list.Where(m => m.CODE == "8").FirstOrDefault());
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 查詢 檔案應用申請專用 012001 010001
        /// </summary>
        public IList<SelectListItem> GetStatuListForUnitCD8_2
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCaseStatusList("8");
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                // 檔案應用申請專用
                list.Remove(list.Where(m => m.CODE == "10").FirstOrDefault());
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 查詢 低收入戶專用
        /// </summary>
        public IList<SelectListItem> GetStatuListForUnitCD8_1
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCaseStatusList("8");
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                list.Remove(list.Where(m => m.CODE == "4").FirstOrDefault());
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 查詢 生殖細胞案件狀態
        /// </summary>
        public IList<SelectListItem> GetStatuListForUnitCD11
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCaseStatusPCDList("11");
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }
        /// <summary>
        /// 查詢 案件狀態 新收案件 收件成功 (法規會)
        /// </summary>
        public IList<SelectListItem> GetStatuListForUnitCD13
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCaseStatusPCDList("13");
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 查詢 案件狀態 新收案件 收件成功 (爭審會)
        /// </summary>
        public IList<SelectListItem> GetStatuListForUnitCD14
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCaseStatusPCDList("14");
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 查詢 案件狀態 新收案件 處理中 退件結案 結案 (國審會)
        /// </summary>
        public IList<SelectListItem> GetStatuListForUnitCD15
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCaseStatusPCDList("15");
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 科別
        /// </summary>
        public IList<SelectListItem> GetDivisionList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetDivisionList001007();
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 證書類別
        /// </summary>
        public IList<SelectListItem> GetDivisionCertList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetDivisionCertList();
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        public IList<SelectListItem> GetLicNumCertList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetLicNumCertList();
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        public IList<SelectListItem> GetLicNumCertMDList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetLicNumCertMDList();
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        public IList<SelectListItem> GetLicNumCertMDByPCDList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetLicNumCertMDByPCDList();
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        public IList<SelectListItem> GetCERTCATEList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCERTCATEList001009();
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        public IList<SelectListItem> GetCERTCATEByPCDList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCERTCATEByPCDList();
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        public IList<SelectListItem> GetCERTCATEMDByPCDList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCERTCATEMDByPCDList();
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 核發單位
        /// </summary>
        public IList<SelectListItem> GetLicIssueDeptList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetLicIssueDeptList();
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 證書(專科醫師證書補（換）發)
        /// </summary>
        public IList<SelectListItem> GetLicNumList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetLicNumList();
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 補(換)發原因
        /// </summary>
        public IList<SelectListItem> GetAction_ResList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeList("F1_ACTION_RES_1");
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 申請種類
        /// </summary>
        public IList<SelectListItem> ISMEET_list
        {
            get
            {
                var dictionary = new Dictionary<string, string> {
                    {"1", "符合第一款(國內社會工作實務經驗五年以上，有中央主管機關審查合格之證明文件。)<br />"},
                    {"2", "符合第二款(外國社會工作師證書及國內社會工作實務經驗一年以上，有中央主管機關審查合格之證明文件。)<br />"},
                    {"3", "符合第三款(曾任公立或依法立案之私立專科以上學校講師三年以上、助理教授或副教授二年以上、教授一年以上，<br /> &nbsp;&nbsp;講授前條第一項所列學科至少二科，並有國內社會工作實務經驗一年以上，有中央主管機關審查合格之證明文件。)"}
                };

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 學歷證明
        /// </summary>
        public IList<SelectListItem> EDUCATION_list
        {
            get
            {
                var dictionary = new Dictionary<string, string> {
                    {"1", "1.中華民國一百零二年起，經考選部依「<a href=\"https://wwwc.moex.gov.tw/main/ExamLaws/wfrmExamLaws.aspx?kind=3&menu_id=320&laws_id=88\" target=\"_blank\">專技人員社會工作師考試應考資格第五條審議通過並公告名單</a>」所列學校之畢業證書影本<br />"},
                    {"2", "2.報考專門職業及技術人員高等考試社會工作師考試之考試通知書影本<br />"},
                    {"3", "3.經教育部承認之國外專科以上社會工作相關科、系、組、所、學位學程畢業證書影本<br />"}
                };

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 年
        /// </summary>
        public IList<SelectListItem> YEAR_list
        {
            get
            {
                var dictionary = new Dictionary<string, string>();
                var nowyear = HelperUtil.DateTimeToTwString(DateTime.Now).Substring(0, 3);
                var yearlst = new List<string>();
                for (int i = nowyear.TOInt32(); i >= 30; i--)
                {
                    dictionary.Add(i.ToString(), i.ToString());
                }

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 年(含空白資料版)
        /// </summary>
        public IList<SelectListItem> YEAR_list_Blank
        {
            get
            {
                var dictionary = new Dictionary<string, string>();
                var nowyear = HelperUtil.DateTimeToTwString(DateTime.Now).Substring(0, 3);
                var yearlst = new List<string>();
                dictionary.Add("", "請選擇");
                for (int i = nowyear.TOInt32(); i >= 30; i--)
                {
                    dictionary.Add(i.ToString(), i.ToString());
                }

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 月
        /// </summary>
        public IList<SelectListItem> MONTH_list
        {
            get
            {
                var dictionary = new Dictionary<string, string>();
                for (int i = 1; i <= 12; i++)
                {
                    dictionary.Add(i.ToString(), i.ToString());
                }

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 月
        /// </summary>
        public IList<SelectListItem> MONTH_list_Blank
        {
            get
            {
                var dictionary = new Dictionary<string, string>();
                dictionary.Add("", "請選擇");
                for (int i = 1; i <= 12; i++)
                {
                    dictionary.Add(i.ToString(), i.ToString());
                }

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 日
        /// </summary>
        public IList<SelectListItem> Day_list
        {
            get
            {
                var dictionary = new Dictionary<string, string>();
                for (int i = 1; i <= 31; i++)
                {
                    dictionary.Add(i.ToString(), i.ToString());
                }

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 核發用_申請份數
        /// </summary>
        public IList<SelectListItem> GetAPPLY_NUM
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = new List<KeyMapModel>();
                list.Add(new KeyMapModel() { CODE = "1", TEXT = "1" });
                list.Add(new KeyMapModel() { CODE = "2", TEXT = "2" });
                list.Add(new KeyMapModel() { CODE = "3", TEXT = "3" });
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 核發用_考試年度
        /// </summary>
        public IList<SelectListItem> GetAPPLY_YEAR
        {
            get
            {
                var dictionary = new Dictionary<string, string>();
                var nowyear = DateTime.Now.Year - 1911;
                var yearlst = new List<string>();
                var det = nowyear - 86;
                for (int i = 0; i <= det; i++)
                {
                    dictionary.Add((nowyear - i).ToString(), (nowyear - i).ToString());
                }

                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 中醫藥司 劑型
        /// </summary>
        public IList<SelectListItem> GetDosageFormList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeList("F5_DOSAGE_FORM");
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 中醫藥司 劑型
        /// </summary>
        public IList<SelectListItem> GetCommoditiesTypeList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeList("F5_COMMODITIES_TYPE");
                //list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 申辦類型
        /// </summary>
        public IList<SelectListItem> IM_EXPORT_list
        {
            get
            {
                IList<KeyMapModel> list = new List<KeyMapModel>();
                list.Add(new KeyMapModel() { CODE = "0", TEXT = "輸入" });
                list.Add(new KeyMapModel() { CODE = "1", TEXT = "輸出" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 信用卡捐款 具名捐款  匿名捐款
        /// </summary>
        public IList<SelectListItem> PUBLIC_MK_list
        {
            get
            {
                IList<KeyMapModel> list = new List<KeyMapModel>();
                list.Add(new KeyMapModel() { CODE = "Y", TEXT = "具名捐款" });
                list.Add(new KeyMapModel() { CODE = "N", TEXT = "匿名捐款" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        public IList<SelectListItem> DONATE_PAY_list
        {
            get
            {
                IList<KeyMapModel> list = new List<KeyMapModel>();
                list.Add(new KeyMapModel() { CODE = "C", TEXT = "信用卡線上刷卡" });
                list.Add(new KeyMapModel() { CODE = "S", TEXT = "超商條碼捐款" });
                list.Add(new KeyMapModel() { CODE = "T", TEXT = "WebATM" });
                list.Add(new KeyMapModel() { CODE = "L", TEXT = "LinePay" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        public IList<SelectListItem> REC_TITLE_CD_list
        {
            get
            {
                IList<KeyMapModel> list = new List<KeyMapModel>();
                list.Add(new KeyMapModel() { CODE = "", TEXT = "不需要收據" });
                list.Add(new KeyMapModel() { CODE = "1", TEXT = "單次紙本收據" });
                list.Add(new KeyMapModel() { CODE = "2", TEXT = "年度紙本收據" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }
        public IList<SelectListItem> APPLYDONATE_list
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetDonateCodeList();
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }
        /// <summary>
        /// 工作型態_清單
        /// </summary>
        public IList<CheckBoxListItem> CHECKNO_checkbox_list
        {
            get
            {
                var dictionary = new Dictionary<string, string> {
                    {"1"," 閱覽、抄錄 "},
                    {"2"," 複製"},
                };
                return MyCommonUtil.ConvertCheckBoxItems(dictionary);
            }
        }

        /// <summary>
        /// 工作型態_清單
        /// </summary>
        public IList<CheckBoxListItem> CHECKNO_checkbox_list1
        {
            get
            {
                var dictionary = new Dictionary<string, string> {
                    {"3"," 歷史考證"},
                    {"4"," 學術研究"},
                    {"5"," 事證稽憑"},
                    {"6"," 業務參考"},
                    {"7"," 權益保障"},
                    {"8"," 其他"},
                };
                return MyCommonUtil.ConvertCheckBoxItems(dictionary);
            }
        }

        /// <summary>
        /// 證書開立證明格式_清單(001008)
        /// </summary>
        public IList<SelectListItem> CERT_TYPECD_list
        {
            get
            {
                IList<KeyMapModel> list = new List<KeyMapModel>();
                list.Add(new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                list.Add(new KeyMapModel() { CODE = "1", TEXT = "分別開立" });
                list.Add(new KeyMapModel() { CODE = "2", TEXT = "整併一張" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 專科類別清單
        /// </summary>
        public IList<SelectListItem> SPECIALIST_TYPE_list
        {
            get
            {
                var dictionary = new Dictionary<string, string> {
                    { "1","醫務專科" },
                    { "2","心理衛生專科" },
                    { "3","兒童、少年、婦女及家庭專科" },
                    { "4","老人專科" },
                    { "5","身心障礙專科" }
                };
                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 專科社會工作師證書換發（更名或污損） 申請狀態清單
        /// </summary>
        public IList<SelectListItem> APPLY_TYPE_34_list
        {
            get
            {
                var dictionary = new Dictionary<string, string>
                {
                    { "3","換發(更名)" },
                    { "4","換發(汙損)" },
                };
                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }
        /// <summary>
        /// 專科社會工作師證書 申請狀態清單
        /// </summary>
        public IList<SelectListItem> APPLY_TYPE_list
        {
            get
            {
                var dictionary = new Dictionary<string, string>
                {
                    { "1","核發" },
                    { "2","補發(遺失)" },
                    { "3","換發(更名)" },
                    { "4","換發(汙損)" },
                };
                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }
        /// <summary>
        /// 專科社會工作師證書 考試年度清單
        /// </summary>
        public IList<SelectListItem> TEST_YEAR_list
        {
            get
            {
                IDictionary<string, string> IDic = new Dictionary<string, string>();
                var NowYear = HelperUtil.TransToTwYear(DateTime.Now, "").Substring(0, 3).TOInt32();
                for (var i = 0; i < 16; i++)
                {
                    IDic.Add((NowYear - i).ToString(), (NowYear - i).ToString());
                }
                return MyCommonUtil.ConvertSelItems(IDic);
            }
        }
        /// <summary>
        /// 專科社會工作師證書 申請狀態清單
        /// </summary>
        public IList<SelectListItem> SEX_CD_list
        {
            get
            {
                var dictionary = new Dictionary<string, string>
                {
                    { "M","男" },
                    { "F","女" },
                    { "O","其他" }
                };
                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 2B.3僅供外銷專用之原因-F_2B_3
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public string Get_PList2B3_TEXT(string val)
        {
            string rst = "";
            if (val == null) { return rst; }
            if (string.IsNullOrEmpty(val)) { return rst; }
            ShareCodeListModel dropdown = new ShareCodeListModel();
            foreach (var item in dropdown.PList2B3_CODE)
            {
                if (item.CODE.Equals(val))
                {
                    rst = item.TEXT;
                    return rst; //break;
                }
            }
            return rst;
        }

        public string Get_PList2B3_ENG(string val)
        {
            string rst = "";
            if (val == null) { return rst; }
            if (string.IsNullOrEmpty(val)) { return rst; }
            ShareCodeListModel dropdown = new ShareCodeListModel();
            foreach (var item in dropdown.PList2B3_ECODE)
            {
                if (item.CODE.Equals(val))
                {
                    rst = item.TEXT;
                    return rst;//break;
                }
            }
            return rst;
        }

        public string Get_PList2B2_TEXT(string val)
        {
            string rst = "";
            if (val == null) { return rst; }
            if (string.IsNullOrEmpty(val)) { return rst; }
            if (val.Equals("A")) { rst = "A、製造廠"; }
            if (val.Equals("B")) { rst = "B、僅包裝和(或)貼標"; }
            if (val.Equals("C")) { rst = "C、以上皆非。"; }
            return rst;
        }

        public string Get_PList2A3_TEXT(string val)
        {
            string rst = "";
            if (val == null) { return rst; }
            if (string.IsNullOrEmpty(val)) { return rst; }
            if (val.Equals("A")) { rst = "A、同時也是製造廠"; }
            if (val.Equals("B")) { rst = "B、僅包裝和(或)貼標"; }
            if (val.Equals("C")) { rst = "C、以上皆非。"; }
            return rst;
        }

        public string Get_YesNo_TEXT(string val)
        {
            string rst = "";
            if (val == null) { return rst; }
            if (string.IsNullOrEmpty(val)) { return rst; }
            if (val.Equals("Y")) { rst = "Yes."; }
            if (val.Equals("N")) { rst = "No."; }
            return rst;
        }

        public string Get_DATE_US_TEXT(DateTime? date1)
        {
            string rst = "";
            if (!date1.HasValue) { return rst; }
            string tmp1 = date1.Value.ToString(" MMM. dd, yyyy", CultureInfo.CreateSpecificCulture("en-US"));
            rst = string.Format("; Issue date:{0}", tmp1);
            return rst;
        }

        /// <summary>
        /// 中文
        /// </summary>
        /// <param name="WORD"></param>
        /// <param name="NUM"></param>
        /// <returns></returns>
        public string Get_F2A1_TEXT(string WORD, string NUM)
        {
            string rst = "";
            string txt = WORD.TONotNullString();
            foreach (KeyMapModel item in PList2A1_CODE)
            {
                if (item.CODE.Equals(txt)) { txt = item.TEXT; }
            }
            rst = string.Format("{0}-第{1}號", txt, NUM);
            return rst;
        }

        /// <summary>
        /// 英文
        /// </summary>
        /// <param name="WORD"></param>
        /// <param name="NUM"></param>
        /// <returns></returns>
        public string Get_F2A1_ENG(string WORD, string NUM)
        {
            string rst = "";
            rst = string.Format("{0}-{1}", WORD, NUM);
            return rst;
        }

        /// <summary>
        /// 單位
        /// </summary>
        public IList<SelectListItem> GetUnitList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetUnitList();
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 處理進度(以單位)
        /// </summary>
        public IList<SelectListItem> GetFlowCDListByUnit(string unit_cd)
        {
            MyKeyMapDAO dao = new MyKeyMapDAO();
            IList<KeyMapModel> list = dao.GetFlowCDListByUnit(unit_cd);
            list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
            return MyCommonUtil.ConvertSelItems(list); 
        }

        /// <summary>
        /// 防疫旅館國家清單
        /// </summary>
        public IList<SelectListItem> FlyPayCountryList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetFlyPayCountryMapList();
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 包裝單位 5013,5014
        /// </summary>
        public IList<SelectListItem> Getvw_PACK_UNITList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeList("F5_vw_PACK_UNIT");
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }
        /// <summary>
        /// 劑型代碼 5013,5014
        /// </summary>
        public IList<SelectListItem> Getvw_DRUG_FORMList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeList("F5_vw_DRUG_FORM");
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }
        /// <summary>
        /// 數量單位 5013,5014
        /// </summary>
        public IList<SelectListItem> Getvw_PACKList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeMemoList("F5_vw_PACK");
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }
        /// <summary>
        /// 國家代碼 5013,5014
        /// </summary>
        public IList<SelectListItem> Getvw_NATIONList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetCodeList("F5_vw_NATION");
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        /// <summary>
        /// 5013,5014 產品類別 sample_proc_type_code
        /// </summary>
        public IList<SelectListItem> PorcType_list
        {
            get
            {
                var dictionary = new Dictionary<string, string>
                {
                    { "0","中藥材" },
                    { "1","中藥製劑" },
                };
                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }


        /// <summary>
        /// 防疫旅館 春節專案
        /// </summary>
        public IList<SelectListItem> SprSection_list
        {
            get
            {
                var dictionary = new Dictionary<string, string>
                {
                    { "","請選擇" },
                    { "1","北" },
                    { "2","中" },
                    { "3","南" },
                };
                return MyCommonUtil.ConvertSelItems(dictionary);
            }
        }

        /// <summary>
        /// 進口關清單 005014
        /// </summary>
        public IList<SelectListItem> ImportList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = dao.GetImportMapList();
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        public IList<SelectListItem> GetUnitSubTypeList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = new List<KeyMapModel>();
                list.Add(new KeyMapModel() { CODE = "1", TEXT = "身心障礙福利機構/團體" });
                list.Add(new KeyMapModel() { CODE = "2", TEXT = "老人及長期照顧福利機構/團體" });
                list.Add(new KeyMapModel() { CODE = "3", TEXT = "兒少、婦女及家庭福利機構/團體" });
                list.Add(new KeyMapModel() { CODE = "4", TEXT = "學校" });
                list.Add(new KeyMapModel() { CODE = "5", TEXT = "矯正機關" });
                list.Add(new KeyMapModel() { CODE = "6", TEXT = "社工師事務所及公會/協會/學會" });
                list.Add(new KeyMapModel() { CODE = "7", TEXT = "其他（非屬上述類別）" });
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }

        public IList<SelectListItem> GetCntTypeList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = new List<KeyMapModel>();
                list.Add(new KeyMapModel() { CODE = "1", TEXT = "總會/事務所統一推薦" });
                list.Add(new KeyMapModel() { CODE = "2", TEXT = "單位無分會/分部/分事務所" });
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }
        public IList<SelectListItem> GetUnitTypeList
        {
            get
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                IList<KeyMapModel> list = new List<KeyMapModel>();
                list.Add(new KeyMapModel() { CODE = "1", TEXT = "1.\t公部門〔直轄市、縣（市）政府（含社會局處、勞工局處、教育局處及衛生局等相關單位）及所屬之社會福利機構、本部等其他中央部會及其所屬社會福利機構、公立大專校院等〕" });
                list.Add(new KeyMapModel() { CODE = "2", TEXT = "2.\t私部門〔各私立社會福利機構、長期照顧服務機構、團體、大專校院〕" });
                list.Add(new KeyMapModel() { CODE = "3", TEXT = "3.\t公私立醫事機構" });
                list.Insert(0, new KeyMapModel() { CODE = "", TEXT = "請選擇" });
                return MyCommonUtil.ConvertSelItems(list);
            }
        }
    }
}