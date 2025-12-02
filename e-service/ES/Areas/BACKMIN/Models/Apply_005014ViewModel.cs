using ES.Commons;
using ES.DataLayers;
using ES.Models.Entities;
using ES.Models.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ES.Areas.BACKMIN.Models
{
    public class Apply_005014ViewModel : ES.Models.ViewModels.Apply_005014ViewModel
    {
        public ControllerContext context;
        public IList<TblAPPLY_NOTICE> AllNoticeList;
        public IList<Apply_005014_FILE_Log> FileLogList;

        private Dictionary<string, string> FieldCodeMap;

        public Apply_005014ViewModel()
        {
            // 未調整
            this.SetupFieldMap();
        }

        public new void GetApplyData(string AppId)
        {
            base.GetApplyData(AppId);

            // 取得歷程
            ApplyDAO applyDao = new ApplyDAO();
            this.AllNoticeList = applyDao.GetRowList(new TblAPPLY_NOTICE { APP_ID = this.Apply.APP_ID })
                .OrderByDescending(x => x.FREQUENCY)
                .ToList();

            if (this.AllNoticeList != null && this.AllNoticeList.Count > 0)
            {
                foreach (var item in this.AllNoticeList)
                {
                    item.Field_NAME = this.FieldCodeMap.ContainsKey(item.Field) ? this.FieldCodeMap[item.Field] : "";
                }

                int maxval = this.AllNoticeList.Max(x => x.FREQUENCY).Value;
                this.ApplyNoticeList = AllNoticeList.Where(x => x.FREQUENCY == maxval).ToList();  // 取得最新一次的補正歷程
            }

            // 取得檔案上傳歷程
            this.FileLogList = applyDao
                .GetRowList(new Apply_005014_FILE_Log { APP_ID = AppId })
                .ToList();
        }

        public new void SaveApply(ModelStateDictionary state)
        {
            BackApplyDAO dao = new BackApplyDAO();

            if (!string.IsNullOrEmpty(this.Apply.APP_ID))
            {
                dao.Update_Apply005014(this);   // 儲存案件資料
                dao.SendMail_005014(this);      // 寄送郵件
            }
        }


        public new void BeforeSave()
        {
            base.BeforeSave();

            var dtl = this.Detail;
            List<TblAPPLY_NOTICE> errata = new List<TblAPPLY_NOTICE>();

            AddErrata(errata, "PRODUCTION_COUNTRY_E", dtl.PRODUCTION_COUNTRY_E, null);
            AddErrata(errata, "SELL_COUNTRY_E", dtl.SELL_COUNTRY_E, null);
            AddErrata(errata, "TRANSFER_COUNTRY_E", dtl.TRANSFER_COUNTRY_E, null);
            AddErrata(errata, "REMARK1_E", dtl.Remark.REMARK1_E, null);
            AddErrata(errata, "REMARK2_E", dtl.Remark.REMARK2_E, null);
            AddErrata(errata, "REMARK3_E", dtl.Remark.REMARK3_E, null);
            if(this.ApplyItems != null && this.ApplyItems.Count > 0)
            {
                for (int i = 0; i < this.ApplyItems.Count; i++)
                {
                    var item = this.ApplyItems[i];
                    AddErrata(errata, "ApplyItems_ITEM_TYPE_E", item.ITEM_TYPE_E, item.ITEM);
                    AddErrata(errata, "ApplyItems_COMMODITIES_E", item.COMMODITIES_E, item.ITEM);
                    AddErrata(errata, "ApplyItems_QTY_E", item.QTY_E, item.ITEM);
                    AddErrata(errata, "ApplyItems_UNIT_E", item.UNIT_E, item.ITEM);
                    AddErrata(errata, "ApplyItems_SPECQTY_E", item.SPECQTY_E, item.ITEM);
                    AddErrata(errata, "ApplyItems_SPECUNIT_E", item.SPECUNIT_E, item.ITEM);
                    AddErrata(errata, "ApplyItems_PORCTYPE_E", item.PORCTYPE_E, item.ITEM);
                    AddErrata(errata, "ApplyItems_COMMODTYPE_E", item.COMMODTYPE_E, item.ITEM);
                    AddErrata(errata, "ApplyItems_AFF1_EXPORT_COUNTRY_E", item.AFF1_EXPORT_COUNTRY_E, item.ITEM);
                    AddErrata(errata, "ApplyItems_AFF1_SHEET_NO_E", item.AFF1_SHEET_NO_E, item.ITEM);
                    AddErrata(errata, "ApplyItems_AFF1_IMPORT_COUNTRY_E", item.AFF1_IMPORT_COUNTRY_E, item.ITEM);
                }
            }
            if (this.ApplyItems2 != null && this.ApplyItems2.Count > 0)
            {
                for (int i = 0; i < this.ApplyItems2.Count; i++)
                {
                    var item = this.ApplyItems2[i];
                    AddErrata(errata, "ApplyItems2_ITEM_TYPE_E", item.ITEM_TYPE_E, item.ITEM);
                    AddErrata(errata, "ApplyItems2_COMMODITIES_E", item.COMMODITIES_E, item.ITEM);
                    AddErrata(errata, "ApplyItems2_QTY_E", item.QTY_E, item.ITEM);
                    AddErrata(errata, "ApplyItems2_UNIT_E", item.UNIT_E, item.ITEM);
                    AddErrata(errata, "ApplyItems2_SPECQTY_E", item.SPECQTY_E, item.ITEM);
                    AddErrata(errata, "ApplyItems2_SPECUNIT_E", item.SPECUNIT_E, item.ITEM);
                    AddErrata(errata, "ApplyItems2_PORCTYPE_E", item.PORCTYPE_E, item.ITEM);
                    AddErrata(errata, "ApplyItems2_COMMODTYPE_E", item.COMMODTYPE_E, item.ITEM);
                    AddErrata(errata, "ApplyItems2_AFF2_EXPORT_COUNTRY_E", item.AFF2_EXPORT_COUNTRY_E, item.ITEM);
                    AddErrata(errata, "ApplyItems2_AFF2_AMOUNT_NAME_E", item.AFF2_AMOUNT_NAME_E, item.ITEM);
                    AddErrata(errata, "ApplyItems2_AFF2_AMOUNT_E", item.AFF2_AMOUNT_E, item.ITEM);
                    AddErrata(errata, "ApplyItems2_AFF2_SHEET_NO_E", item.AFF2_SHEET_NO_E, item.ITEM);
                    AddErrata(errata, "ApplyItems2_AFF2_IMPORT_COUNTRY_E", item.AFF2_IMPORT_COUNTRY_E, item.ITEM);
                }
            }
            if (this.FileList != null)
            {
                for (int i = 0; i < this.FileList.Count; i++)
                {
                    var item = this.FileList[i];
                    AddErrata(errata, "FILE", item.FILE_E, i + 1);
                }
            }

            this.ApplyNoticeList = errata;

        }

        public void ResetErrataField()
        {
            if (this.Detail != null)
            {
                foreach (var prop in this.Detail.GetType().GetProperties())
                {
                    if (prop.Name.EndsWith("_E"))
                    {
                        prop.SetValue(this.Detail, null);
                    }
                }
            }

            if (this.ApplyItems != null && this.ApplyItems.Count > 0)
            {
                foreach (var item in this.ApplyItems)
                {
                    foreach (var prop in item.GetType().GetProperties())
                    {
                        if (prop.Name.EndsWith("_E"))
                        {
                            prop.SetValue(item, null);
                        }
                    }
                }
            }
            if (this.ApplyItems2 != null && this.ApplyItems2.Count > 0)
            {
                foreach (var item in this.ApplyItems2)
                {
                    foreach (var prop in item.GetType().GetProperties())
                    {
                        if (prop.Name.EndsWith("_E"))
                        {
                            prop.SetValue(item, null);
                        }
                    }
                }
            }
            if (this.FileList != null && this.FileList.Count > 0)
            {
                foreach (var item in this.FileList)
                {
                    item.FILE_E = null;
                }
            }
        }

        private void AddErrata(List<TblAPPLY_NOTICE> errata, string key, string item, int? srcNo)
        {
            DateTime now = DateTime.Now;
            if (!string.IsNullOrEmpty(item))
            {
                TblAPPLY_NOTICE model = new TblAPPLY_NOTICE();
                model.APP_ID = this.Apply.APP_ID;
                model.ADD_TIME = now;
                model.Field = key;
                model.ISADDYN = "Y";
                model.NOTE = item;

                if (srcNo.HasValue)
                {
                    model.SRC_NO = srcNo;
                }

                errata.Add(model);
            }
        }

        private void SetupFieldMap()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("PRODUCTION_COUNTRY_E", "生產國別");
            dict.Add("SELL_COUNTRY_E", "賣方國家");
            dict.Add("TRANSFER_COUNTRY_E", "起運國家");
            dict.Add("REMARK1_E", "備註1");
            dict.Add("REMARK2_E", "備註2");
            dict.Add("REMARK3_E", "備註3");
            
            dict.Add("ApplyItems_ITEM_TYPE_E", "一般申請項目-類型");
            dict.Add("ApplyItems_COMMODITIES_E", "一般申請項目-貨名");
            dict.Add("ApplyItems_QTY_E", "一般申請項目-申請數量");
            dict.Add("ApplyItems_UNIT_E", "一般申請項目-申請數量單位");
            dict.Add("ApplyItems_SPECQTY_E", "一般申請項目-規格數量");
            dict.Add("ApplyItems_SPECUNIT_E", "一般申請項目-規格數量單位");
            dict.Add("ApplyItems_PORCTYPE_E", "一般申請項目-產品類別");
            dict.Add("ApplyItems_COMMODTYPE_E", "一般申請項目-劑型");
            dict.Add("ApplyItems_AFF1_EXPORT_COUNTRY_E", "一般申請項目切結書-出口國");
            dict.Add("ApplyItems_AFF1_SHEET_NO_E", "一般申請項目切結書-報單號碼");
            dict.Add("ApplyItems_AFF1_IMPORT_COUNTRY_E", "一般申請項目切結書-進口關");

            dict.Add("ApplyItems2_ITEM_TYPE_E", "萃取物(提取物)申請項目-類型");
            dict.Add("ApplyItems2_COMMODITIES_E", "萃取物(提取物)申請項目-貨名");
            dict.Add("ApplyItems2_QTY_E", "萃取物(提取物)申請項目-申請數量");
            dict.Add("ApplyItems2_UNIT_E", "萃取物(提取物)申請項目-申請數量單位");
            dict.Add("ApplyItems2_SPECQTY_E", "萃取物(提取物)申請項目-規格數量");
            dict.Add("ApplyItems2_SPECUNIT_E", "萃取物(提取物)申請項目-規格數量單位");
            dict.Add("ApplyItems2_PORCTYPE_E", "萃取物(提取物)申請項目-產品類別");
            dict.Add("ApplyItems2_COMMODTYPE_E", "萃取物(提取物)申請項目-劑型");
            dict.Add("ApplyItems2_AFF2_EXPORT_COUNTRY_E", "萃取物(提取物)申請項目-出口國");
            dict.Add("ApplyItems2_AFF2_AMOUNT_NAME_E", "萃取物(提取物)申請項目-原中藥材名稱");
            dict.Add("ApplyItems2_AFF2_AMOUNT_E", "萃取物(提取物)申請項目-原中藥材含量");
            dict.Add("ApplyItems2_AFF2_SHEET_NO_E", "萃取物(提取物)申請項目-報單號碼");
            dict.Add("ApplyItems2_AFF2_IMPORT_COUNTRY_E", "萃取物(提取物)申請項目-進口關");

            dict.Add("FILE", "其他證明文件");

            this.FieldCodeMap = dict;
        }

    }
}