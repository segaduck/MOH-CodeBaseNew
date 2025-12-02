using ES.Commons;
using ES.DataLayers;
using ES.Models.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Omu.ValueInjecter;
using ES.Services;

namespace ES.Models.ViewModels
{
    public partial class Apply_005014ViewModel
    {
        public Apply_005014ViewModel()
        {
            this.Apply = new Apply_005014ApplyModel();
            this.Detail = new Apply_005014DetailModel();
            this.ApplyItems = new List<Apply_005014_ItemExt>();
            this.ApplyItems2 = new List<Apply_005014_Item2Ext>();
            this.ErrataList = new List<ErrataModel>();
            this.IsEditable = true;
            this.ApplyNoticeList = new List<TblAPPLY_NOTICE>();
        }

        /// <summary>
        /// 編輯模式
        /// </summary>
        public bool IsEditable { get; set; }

        /// <summary>
        /// 案件補件狀態, true 則前台使用者可編輯須補正欄位
        /// </summary>
        public bool IsErrata()
        {
            bool res = false;
            ShareDAO dao = new ShareDAO();
            if (this.Apply != null && (this.Apply.FLOW_CD == "2" || this.Apply.FLOW_CD == "4"))
            {
                if (dao.CalculationDocDate("005014", APP_ID))
                {
                    //已過補件期限
                    res = false;
                }
                else
                {
                    res = true;
                }
            }
            return res;
        }

        public string APP_ID { get; set; }
        public Apply_005014ApplyModel Apply { get; set; }
        public Apply_005014DetailModel Detail { get; set; }

        //public string CONTACT_TEL_Zip { get; set; }
        //public string CONTACT_TEL_Phone { get; set; }
        //public string CONTACT_TEL_Num { get; set; }
        //public string CONTACT_TEL { get; set; }

        public string ADDR_ZIP { get; set; }
        public string ADDR_ZIP_ADDR { get; set; }
        public string ADDR_ZIP_DETAIL { get; set; }

        public string TEL_Zip { get; set; }
        public string TEL_Phone { get; set; }
        public string TEL_Num { get; set; }
        public string TEL { get; set; }

        public string CNT_TEL_Zip { get; set; }
        public string CNT_TEL_Phone { get; set; }
        public string CNT_TEL_Num { get; set; }
        public string CNT_TEL { get; set; }

        /// <summary>
        /// 行動電話
        /// </summary>
        public string MOBILE { get; set; }

        public string EMAIL_1 { get; set; }
        public string EMAIL_2 { get; set; }
        public string EMAIL_3 { get; set; }
        public string EMAIL { get; set; }

        public IList<Apply_005014_ItemExt> ApplyItems { get; set; }
        public IList<Apply_005014_Item2Ext> ApplyItems2 { get; set; }
        public string ApplyItemsJson { get; set; }
        public string ApplyItems2Json { get; set; }
        public IList<HttpPostedFileBase> UploadFiles { get; set; }
        public IList<Apply_005014_FILExt> FileList { get; set; }
        public IList<ErrataModel> ErrataList { get; set; }
        public IList<TblAPPLY_NOTICE> ApplyNoticeList { get; set; }
        public IList<TblAPPLY_NOTICE> CurrentNoticeList { get; set; }

        public void GetApplyData(string appId)
        {
            this.APP_ID = appId;
            ApplyDAO dao = new ApplyDAO();
            ApplyModel apy = dao.GetRow(new ApplyModel { APP_ID = appId });
            if (apy != null)
            {
                this.Apply = (Apply_005014ApplyModel)new Apply_005014ApplyModel().InjectFrom(apy);
                this.Apply.MOBILE = apy.MOBILE;
                var adm = dao.GetRow(new AdminModel { ACC_NO = this.Apply.PRO_ACC });
                if (adm != null)
                {
                    this.Apply.PRO_ACC_NAME = adm.NAME;
                }
                else
                {
                    this.Apply.PRO_ACC_NAME = this.Apply.PRO_ACC;
                }
                // 查詢負責人姓名
                if (string.IsNullOrEmpty(this.Apply.CHR_NAME))
                {
                    var chr = dao.GetRow(new TblMEMBER { ACC_NO = this.Apply.ACC_NO });
                    if (chr != null)
                    {
                        this.Apply.CHR_NAME = chr.CHR_NAME.TONotNullString();
                    }
                }
                // 申請日期
                if (this.Apply.APP_TIME==null)
                {
                    this.Apply.APP_TIME = this.Apply.ADD_TIME;
                }
            }

            Apply_005014 apy0514 = dao.GetRow(new Apply_005014 { APP_ID = appId });
            if (apy0514 != null)
            {
                this.Detail = (Apply_005014DetailModel)new Apply_005014DetailModel().InjectFrom(apy0514);

                var itemGrids = dao.GetRowList(new Apply_005014_Item { APP_ID = appId });
                // 中藥材作為食品使用者 其他
                var itemType2and3 = itemGrids.Where(x => x.ITEM_TYPE.TOInt32() > 1).ToList();
                if (itemType2and3 != null && itemType2and3.Count > 0)
                {
                    foreach (var item in itemType2and3)
                    {
                        var row = (Apply_005014_ItemExt)new Apply_005014_ItemExt().InjectFrom(item);
                        this.ApplyItems.Add(row);
                    }
                }
                // 中藥材萃取物作為食品原料者
                var itemtype1 = itemGrids.Where(x => x.ITEM_TYPE.TOInt32() == 1).ToList();
                if (itemtype1 != null && itemtype1.Count > 0)
                {
                    foreach (var item in itemtype1)
                    {
                        var row = (Apply_005014_Item2Ext)new Apply_005014_Item2Ext().InjectFrom(item);
                        this.ApplyItems2.Add(row);
                    }
                }
                //  備註
                this.Detail.Remark = new Apply_005014_REMARKExt();
                this.Detail.Remark.InjectFrom(dao.GetRow(new Apply_005014_REMARK { APP_ID = appId }));
                if (this.Detail.Remark != null)
                {
                    this.Detail.Remark.checkboxR1 = this.Detail.Remark.REMARK1 == "Y" ? true : false;
                    this.Detail.Remark.checkboxR2 = this.Detail.Remark.REMARK2 == "Y" ? true : false;
                    this.Detail.Remark.checkboxR3 = this.Detail.Remark.REMARK3_1 == "Y" ? true : false;
                }

            }

            // ApplyItems...
            this.FileList = new List<Apply_005014_FILExt>();

            IList<Apply_005014_FILE> fileList = dao.GetRowList(new Apply_005014_FILE { APP_ID = appId });
            if (fileList != null && fileList.Count > 0)
            {
                foreach (var f in fileList)
                {
                    Apply_005014_FILExt fext = (Apply_005014_FILExt)new Apply_005014_FILExt().InjectFrom(f);
                    this.FileList.Add(fext);
                }
            }

            var noticeList = dao.GetRowList(new TblAPPLY_NOTICE { APP_ID = appId }).OrderByDescending(x => x.FREQUENCY).ToList();
            if (noticeList != null && noticeList.Count > 0)
            {
                var maxSeq = noticeList[0].FREQUENCY;
                this.ApplyNoticeList = noticeList.Where(x => x.FREQUENCY == maxSeq).ToList();
            }

            this.AfterLoad();
        }

        /// <summary>
        /// 儲存申請資料
        /// </summary>
        public void SaveApply(ModelStateDictionary state)
        {
            this.BeforeSave();

            ApplyDAO dao = new ApplyDAO();

            if (string.IsNullOrEmpty(Apply.APP_ID))
            {
                this.Apply.FLOW_CD = "0";
            }

            switch (this.Apply.FLOW_CD)
            {
                case "0":  // 新申請
                    this.Validate(state);
                    if (!state.IsValid) { throw new Exception("儲存檢核錯誤"); }

                    this.Apply.FLOW_CD = "1";
                    this.Apply.APP_EXT_DATE = DateTime.Now.AddDays(5);
                    dao.AppendApply005014(this);
                    dao.SendMail_005014(this);
                    break;

                case "2":  // 原為通知補件->補件收件
                    if (!string.IsNullOrEmpty(this.Apply.APP_ID))
                    {
                        this.Apply.FLOW_CD = "3";
                        this.Apply.APP_EXT_DATE = DateTime.Now.AddDays(5);
                        dao.Update_Apply005014Front(this);
                    }
                    break;
            }
        }

        public void GetApplyItemList()
        {
            if (this.APP_ID != null)
            {
                ShareDAO dao = new ShareDAO();
                IList<Apply_005014_Item> itemList = dao.GetRowList(new Apply_005014_Item { APP_ID = this.APP_ID });
                IList<Apply_005014_ItemExt> extList = itemList.Where(x => x.ITEM_TYPE.TOInt32() > 1)
                    .Select(x => new Apply_005014_ItemExt().InjectFrom(x) as Apply_005014_ItemExt).ToList();
                this.ApplyItems = extList;

                var noticeList = dao.GetRowList(new TblAPPLY_NOTICE { APP_ID = this.APP_ID }).OrderByDescending(x => x.FREQUENCY).ToList();
                if (noticeList != null && noticeList.Count > 0)
                {
                    var maxSeq = noticeList[0].FREQUENCY;
                    this.ApplyNoticeList = noticeList.Where(x => x.FREQUENCY == maxSeq).ToList();
                }

                if (this.ApplyItems != null)
                {
                    foreach (var item in this.ApplyItems)
                    {
                        item.ITEM_TYPE_E = this.GetApplyItemNoticeField("ApplyItems_ITEM_TYPE_E", item.ITEM);
                        item.COMMODITIES_E = this.GetApplyItemNoticeField("ApplyItems_COMMODITIES_E", item.ITEM);
                        item.QTY_E = this.GetApplyItemNoticeField("ApplyItems_QTY_E", item.ITEM);
                        item.UNIT_E = this.GetApplyItemNoticeField("ApplyItems_UNIT_E", item.ITEM);
                        item.SPECQTY_E = this.GetApplyItemNoticeField("ApplyItems_SPECQTY_E", item.ITEM);
                        item.SPECUNIT_E = this.GetApplyItemNoticeField("ApplyItems_SPECUNIT_E", item.ITEM);
                        item.PORCTYPE_E = this.GetApplyItemNoticeField("ApplyItems_PORCTYPE_E", item.ITEM);
                        item.COMMODTYPE_E = this.GetApplyItemNoticeField("ApplyItems_COMMODTYPE_E", item.ITEM);
                        item.AFF1_EXPORT_COUNTRY_E = this.GetApplyItemNoticeField("ApplyItems_AFF1_EXPORT_COUNTRY_E", item.ITEM);
                        item.AFF1_SHEET_NO_E = this.GetApplyItemNoticeField("ApplyItems_AFF1_SHEET_NO_E", item.ITEM);
                        item.AFF1_IMPORT_COUNTRY_E = this.GetApplyItemNoticeField("ApplyItems_AFF1_IMPORT_COUNTRY_E", item.ITEM);
                        item.AFF2_EXPORT_COUNTRY_E = this.GetApplyItemNoticeField("ApplyItems_AFF2_EXPORT_COUNTRY_E", item.ITEM);
                        item.AFF2_AMOUNT_NAME_E = this.GetApplyItemNoticeField("ApplyItems_AFF2_AMOUNT_NAME_E", item.ITEM);
                        item.AFF2_AMOUNT_E = this.GetApplyItemNoticeField("ApplyItems_AFF2_AMOUNT_E", item.ITEM);
                        item.AFF2_SHEET_NO_E = this.GetApplyItemNoticeField("ApplyItems_AFF2_SHEET_NO_E", item.ITEM);
                        item.AFF2_IMPORT_COUNTRY_E = this.GetApplyItemNoticeField("ApplyItems_AFF2_IMPORT_COUNTRY_E", item.ITEM);
                    }
                }
            }
            else
            {
                Apply_005014_ItemExt item = new Apply_005014_ItemExt { ID = DateTime.Now.Ticks.ToString() };
                this.ApplyItems.Add(item);
            }
        }
        public void GetApplyItem2List()
        {
            if (this.APP_ID != null)
            {
                ShareDAO dao = new ShareDAO();
                IList<Apply_005014_Item> itemList = dao.GetRowList(new Apply_005014_Item { APP_ID = this.APP_ID });
                IList<Apply_005014_Item2Ext> ext2List = itemList.Where(x => x.ITEM_TYPE.TOInt32() == 1)
                    .Select(x => new Apply_005014_Item2Ext().InjectFrom(x) as Apply_005014_Item2Ext).ToList();
                this.ApplyItems2 = ext2List;

                var noticeList = dao.GetRowList(new TblAPPLY_NOTICE { APP_ID = this.APP_ID }).OrderByDescending(x => x.FREQUENCY).ToList();
                if (noticeList != null && noticeList.Count > 0)
                {
                    var maxSeq = noticeList[0].FREQUENCY;
                    this.ApplyNoticeList = noticeList.Where(x => x.FREQUENCY == maxSeq).ToList();
                }

                if (this.ApplyItems2 != null)
                {
                    foreach (var item in this.ApplyItems2)
                    {
                        item.ITEM_TYPE_E = this.GetApplyItemNoticeField("ApplyItems2_ITEM_TYPE_E", item.ITEM);
                        item.COMMODITIES_E = this.GetApplyItemNoticeField("ApplyItems2_COMMODITIES_E", item.ITEM);
                        item.QTY_E = this.GetApplyItemNoticeField("ApplyItems2_QTY_E", item.ITEM);
                        item.UNIT_E = this.GetApplyItemNoticeField("ApplyItems2_UNIT_E", item.ITEM);
                        item.SPECQTY_E = this.GetApplyItemNoticeField("ApplyItems2_SPECQTY_E", item.ITEM);
                        item.SPECUNIT_E = this.GetApplyItemNoticeField("ApplyItems2_SPECUNIT_E", item.ITEM);
                        item.PORCTYPE_E = this.GetApplyItemNoticeField("ApplyItems2_PORCTYPE_E", item.ITEM);
                        item.COMMODTYPE_E = this.GetApplyItemNoticeField("ApplyItems2_COMMODTYPE_E", item.ITEM);
                        item.AFF1_EXPORT_COUNTRY_E = this.GetApplyItemNoticeField("ApplyItems2_AFF1_EXPORT_COUNTRY_E", item.ITEM);
                        item.AFF1_SHEET_NO_E = this.GetApplyItemNoticeField("ApplyItems2_AFF1_SHEET_NO_E", item.ITEM);
                        item.AFF1_IMPORT_COUNTRY_E = this.GetApplyItemNoticeField("ApplyItems2_AFF1_IMPORT_COUNTRY_E", item.ITEM);
                        item.AFF2_EXPORT_COUNTRY_E = this.GetApplyItemNoticeField("ApplyItems2_AFF2_EXPORT_COUNTRY_E", item.ITEM);
                        item.AFF2_AMOUNT_NAME_E = this.GetApplyItemNoticeField("ApplyItems2_AFF2_AMOUNT_NAME_E", item.ITEM);
                        item.AFF2_AMOUNT_E = this.GetApplyItemNoticeField("ApplyItems2_AFF2_AMOUNT_E", item.ITEM);
                        item.AFF2_SHEET_NO_E = this.GetApplyItemNoticeField("ApplyItems2_AFF2_SHEET_NO_E", item.ITEM);
                        item.AFF2_IMPORT_COUNTRY_E = this.GetApplyItemNoticeField("ApplyItems2_AFF2_IMPORT_COUNTRY_E", item.ITEM);
                    }
                }
            }
            else
            {
                Apply_005014_Item2Ext item2 = new Apply_005014_Item2Ext { ID = DateTime.Now.Ticks.ToString() };
                this.ApplyItems2.Add(item2);
            }
        }

        public void GetFileList()
        {
            ShareDAO dao = new ShareDAO();
            if (!string.IsNullOrEmpty(this.APP_ID))
            {
                this.FileList = new List<Apply_005014_FILExt>();
                var fileList = dao.GetRowList(new Apply_005014_FILE { APP_ID = this.APP_ID });
                foreach (var file in fileList)
                {
                    var fext = new Apply_005014_FILExt();
                    fext.FILE_E = file.FILE_E;
                    fext.FILE_ID = file.FILE_ID;
                    fext.APP_ID = file.APP_ID;
                    this.FileList.Add(fext);
                }
            }

            var noticeList = dao.GetRowList(new TblAPPLY_NOTICE { APP_ID = this.APP_ID }).OrderByDescending(x => x.FREQUENCY).ToList();
            if (noticeList != null && noticeList.Count > 0)
            {
                var maxSeq = noticeList[0].FREQUENCY;
                this.ApplyNoticeList = noticeList.Where(x => x.FREQUENCY == maxSeq).ToList();
            }

            if (this.FileList != null)
            {
                for (int i = 0; i < this.FileList.Count; i++)
                {
                    var file = this.FileList[i];
                    file.FILE_E = this.GetApplyFileNoticeField("FILE", i + 1);
                }
            }

        }

        public string StreamToBase64(Stream stream)
        {
            string result = null;
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                result = System.Convert.ToBase64String(ms.ToArray());
            }
            return result;
        }

        public void AfterLoad()
        {
            if (this.Apply != null)
            {
                this.Apply.ADDR_ZIP = this.Apply.ADDR_CODE;
                this.Apply.ADDR_ZIP_DETAIL = this.Apply.ADDR;

                if (!string.IsNullOrEmpty(this.Apply.TEL))
                {
                    string[] telArr = this.Apply.TEL.Split(new char[] { '-', '#' });
                    this.TEL_Zip = telArr[0];
                    this.TEL_Phone = telArr[1];
                    this.TEL_Num = telArr[2];
                }

                if (!string.IsNullOrEmpty(this.Apply.CNT_TEL))
                {
                    string[] cntArr = this.Apply.CNT_TEL.Split(new char[] { '-', '#' });
                    this.CNT_TEL_Zip = cntArr[0];
                    this.CNT_TEL_Phone = cntArr[1];
                    this.CNT_TEL_Num = cntArr[2];
                }
            }

            if (this.Detail != null)
            {
                var dtl = this.Detail;

                dtl.AFFIDAVIT2_CHECKED = (dtl.AFFIDAVIT2 == "Y");

                if (string.IsNullOrWhiteSpace(dtl.EMAIL))
                {
                    dtl.EMAIL = this.EMAIL + "@" + ((string.IsNullOrWhiteSpace(this.EMAIL_1) || this.EMAIL_1 == "0") ? (string.IsNullOrWhiteSpace(this.EMAIL_2) ? EMAIL_3 : this.EMAIL_2) : new ShareCodeListModel().GetMailDomainList.Where(m => m.Value == this.EMAIL_1).FirstOrDefault().Text);
                }
                else
                {
                    this.EMAIL = this.Detail.EMAIL;
                    var emailArr = dtl.EMAIL.Split(new string[] { "@0", "@" }, StringSplitOptions.RemoveEmptyEntries);
                    EMAIL_1 = emailArr[0];
                    if (emailArr.ToCount() > 1)
                    {
                        var email1List = new ShareCodeListModel().GetMailDomainList.Where(m => m.Value == emailArr[1]).ToList();
                        if (email1List.ToCount() != 0)
                        {
                            EMAIL_1 = email1List.FirstOrDefault().Value;
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(emailArr[1]))
                            {
                                EMAIL_2 = "0";
                                EMAIL_3 = emailArr[1];
                            }
                        }
                    }
                }

                dtl.PRODUCTION_COUNTRY_E = this.GetNoticeField("PRODUCTION_COUNTRY_E");
                dtl.SELL_COUNTRY_E = this.GetNoticeField("SELL_COUNTRY_E");
                dtl.TRANSFER_COUNTRY_E = this.GetNoticeField("TRANSFER_COUNTRY_E");

                dtl.AFF1_COMMODITIES_E = this.GetNoticeField("AFF1_COMMODITIES_E");
                dtl.AFF1_QTY_E = this.GetNoticeField("AFF1_QTY_E");
                dtl.AFF1_NO_E = this.GetNoticeField("AFF1_NO_E");

                dtl.AFF2_COMMODITIES_E = this.GetNoticeField("AFF2_COMMODITIES_E");
                dtl.AFF2_QTY_E = this.GetNoticeField("AFF2_QTY_E");
                dtl.AFF2_NO_E = this.GetNoticeField("AFF2_NO_E");

                if (dtl.Remark != null)
                {
                    dtl.Remark.REMARK1_E = this.GetNoticeField("REMARK1_E");
                }
            }

            if (this.ApplyItems != null)
            {
                foreach (var item in this.ApplyItems)
                {
                    item.ITEM_TYPE_E = this.GetApplyItemNoticeField("ApplyItems_ITEM_TYPE_E", item.ITEM);
                    item.COMMODITIES_E = this.GetApplyItemNoticeField("ApplyItems_COMMODITIES_E ", item.ITEM);
                    item.QTY_E = this.GetApplyItemNoticeField("ApplyItems_QTY_E", item.ITEM);
                    item.UNIT_E = this.GetApplyItemNoticeField("ApplyItems_UNIT_E", item.ITEM);
                    item.SPECQTY_E = this.GetApplyItemNoticeField("ApplyItems_SPECQTY_E", item.ITEM);
                    item.SPECUNIT_E = this.GetApplyItemNoticeField("ApplyItems_SPECUNIT_E", item.ITEM);
                    item.PORCTYPE_E = this.GetApplyItemNoticeField("ApplyItems_PORCTYPE_E", item.ITEM);
                    item.COMMODTYPE_E = this.GetApplyItemNoticeField("ApplyItems_COMMODTYPE_E", item.ITEM);
                    item.AFF1_EXPORT_COUNTRY_E = this.GetApplyItemNoticeField("ApplyItems_AFF1_EXPORT_COUNTRY_E", item.ITEM);
                    item.AFF1_SHEET_NO_E = this.GetApplyItemNoticeField("ApplyItems_AFF1_SHEET_NO_E", item.ITEM);
                    item.AFF1_IMPORT_COUNTRY_E = this.GetApplyItemNoticeField("ApplyItems_AFF1_IMPORT_COUNTRY_E", item.ITEM);
                    item.AFF2_EXPORT_COUNTRY_E = this.GetApplyItemNoticeField("ApplyItems_AFF2_EXPORT_COUNTRY_E", item.ITEM);
                    item.AFF2_AMOUNT_NAME_E = this.GetApplyItemNoticeField("ApplyItems_AFF2_AMOUNT_NAME_E", item.ITEM);
                    item.AFF2_AMOUNT_E = this.GetApplyItemNoticeField("ApplyItems_AFF2_AMOUNT_E", item.ITEM);
                    item.AFF2_SHEET_NO_E = this.GetApplyItemNoticeField("ApplyItems_AFF2_SHEET_NO_E", item.ITEM);
                    item.AFF2_IMPORT_COUNTRY_E = this.GetApplyItemNoticeField("ApplyItems_AFF2_IMPORT_COUNTRY_E", item.ITEM);

                }
            }
            if (this.ApplyItems2 != null)
            {
                foreach (var item in this.ApplyItems2)
                {
                    item.ITEM_TYPE_E = this.GetApplyItemNoticeField("ApplyItems2_ITEM_TYPE_E", item.ITEM);
                    item.COMMODITIES_E = this.GetApplyItemNoticeField("ApplyItems2_COMMODITIES_E ", item.ITEM);
                    item.QTY_E = this.GetApplyItemNoticeField("ApplyItems2_QTY_E", item.ITEM);
                    item.UNIT_E = this.GetApplyItemNoticeField("ApplyItems2_UNIT_E", item.ITEM);
                    item.SPECQTY_E = this.GetApplyItemNoticeField("ApplyItems2_SPECQTY_E", item.ITEM);
                    item.SPECUNIT_E = this.GetApplyItemNoticeField("ApplyItems2_SPECUNIT_E", item.ITEM);
                    item.PORCTYPE_E = this.GetApplyItemNoticeField("ApplyItems2_PORCTYPE_E", item.ITEM);
                    item.COMMODTYPE_E = this.GetApplyItemNoticeField("ApplyItems2_COMMODTYPE_E", item.ITEM);
                    item.AFF1_EXPORT_COUNTRY_E = this.GetApplyItemNoticeField("ApplyItems2_AFF1_EXPORT_COUNTRY_E", item.ITEM);
                    item.AFF1_SHEET_NO_E = this.GetApplyItemNoticeField("ApplyItems2_AFF1_SHEET_NO_E", item.ITEM);
                    item.AFF1_IMPORT_COUNTRY_E = this.GetApplyItemNoticeField("ApplyItems2_AFF1_IMPORT_COUNTRY_E", item.ITEM);
                    item.AFF2_EXPORT_COUNTRY_E = this.GetApplyItemNoticeField("ApplyItems2_AFF2_EXPORT_COUNTRY_E", item.ITEM);
                    item.AFF2_AMOUNT_NAME_E = this.GetApplyItemNoticeField("ApplyItems2_AFF2_AMOUNT_NAME_E", item.ITEM);
                    item.AFF2_AMOUNT_E = this.GetApplyItemNoticeField("ApplyItems2_AFF2_AMOUNT_E", item.ITEM);
                    item.AFF2_SHEET_NO_E = this.GetApplyItemNoticeField("ApplyItems2_AFF2_SHEET_NO_E", item.ITEM);
                    item.AFF2_IMPORT_COUNTRY_E = this.GetApplyItemNoticeField("ApplyItems2_AFF2_IMPORT_COUNTRY_E", item.ITEM);

                }
            }

            if (this.FileList != null)
            {
                for (int i = 0; i < this.FileList.Count; i++)
                {
                    var file = this.FileList[i];
                    file.FILE_E = this.GetApplyFileNoticeField("FILE", i + 1);
                }
            }
        }

        private string GetNoticeField(string filename)
        {
            string result = null;
            var notice = this.ApplyNoticeList.Where(x => x.Field == filename).FirstOrDefault();
            if (notice != null)
            {
                result = notice.NOTE;
            }
            return result;
        }

        private string GetApplyItemNoticeField(string filename, int? item)
        {
            string result = null;
            var notice = this.ApplyNoticeList
                .Where(x => x.Field == filename && x.SRC_NO == item)
                .FirstOrDefault();

            if (notice != null)
            {
                result = notice.NOTE;
            }
            return result;
        }

        private string GetApplyFileNoticeField(string filename, int? index)
        {
            string result = null;
            var notice = this.ApplyNoticeList
                .Where(x => x.Field == filename && x.SRC_NO == index)
                .FirstOrDefault();

            if (notice != null)
            {
                result = notice.NOTE;
            }
            return result;
        }


        public void BeforeSave()
        {
            this.ApplyItems = JsonConvert.DeserializeObject<IList<Apply_005014_ItemExt>>
                (this.ApplyItemsJson, new StringConverter(), new IntConverter());
            this.ApplyItems2 = JsonConvert.DeserializeObject<IList<Apply_005014_Item2Ext>>
                (this.ApplyItems2Json, new StringConverter(), new IntConverter());

            if (this.Apply != null)
            {
                this.Apply.ADDR_CODE = this.Apply.ADDR_ZIP;
                this.Apply.ADDR = this.Apply.ADDR_ZIP_DETAIL;
                this.Apply.MOBILE = this.Apply.MOBILE;
                this.Apply.TEL = string.Format("{0}-{1}#{2}", this.TEL_Zip, this.TEL_Phone, this.TEL_Num);
                this.Apply.CNT_TEL = string.Format("{0}-{1}#{2}", this.CNT_TEL_Zip, this.CNT_TEL_Phone, this.CNT_TEL_Num);

                if (this.Apply.TEL == "-#")
                {
                    this.Apply.TEL = null;
                }
                if (this.Apply.CNT_TEL == "-#")
                {
                    this.Apply.CNT_TEL = null;
                }
            }

            if (this.Detail != null)
            {
                var dtl = this.Detail;
                dtl.AFFIDAVIT2 = dtl.AFFIDAVIT2_CHECKED ? "Y" : "N";

                if (!string.IsNullOrWhiteSpace(EMAIL_1))
                {
                    dtl.EMAIL = string.Format("{0}@{1}{2}", this.EMAIL_1, this.EMAIL_2, this.EMAIL_3);
                }

                if (this.Detail.Remark != null)
                {
                    var mk = this.Detail.Remark;
                    mk.REMARK1 = mk.checkboxR1 ? "Y" : "N";
                    mk.REMARK2 = mk.checkboxR2 ? "Y" : "N";
                    mk.REMARK3_1 = mk.checkboxR3 ? "Y" : "N";
                }
            }
        }

        public void Preview()
        {
            this.BeforeSave();

            if (!string.IsNullOrEmpty(this.ApplyItemsJson))
            {
                this.ApplyItems = JsonConvert.DeserializeObject<IList<Apply_005014_ItemExt>>(this.ApplyItemsJson, new StringConverter(), new IntConverter());
            }
            if (!string.IsNullOrEmpty(this.ApplyItems2Json))
            {
                this.ApplyItems2 = JsonConvert.DeserializeObject<IList<Apply_005014_Item2Ext>>(this.ApplyItems2Json, new StringConverter(), new IntConverter());
            }

            this.FileList = new List<Apply_005014_FILExt>();
            if (this.UploadFiles != null)
            {
                ShareDAO dao = new ShareDAO();
                int i = 0;
                foreach (var file in this.UploadFiles)
                {
                    if (file != null)
                    {
                        Apply_005014_FILExt fitem = new Apply_005014_FILExt();
                        fitem.MIME = file.ContentType;
                        fitem.FILE_NAME = dao.PutFile("005014", file, (++i).ToString());
                        this.FileList.Add(fitem);
                    }
                }
            }
        }
        /// <summary>
        /// 取得 進口關 選單名稱
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public string GetPortText(string port)
        {
            string res = string.Empty;
            if (port != null)
            {
                MyKeyMapDAO dao = new MyKeyMapDAO();
                ShareCodeListModel options = new ShareCodeListModel();
                KeyMapModel item = dao.GetLocalPortList().Where(x => x.CODE == port).FirstOrDefault();  // 清單
                if (item != null)
                {
                    res = string.Format("({0}){1}", item.CODE, item.TEXT);
                }
            }

            return res;
        }

        public void Validate(ModelStateDictionary state, string applyitem = "")
        {
            var apy = this.Apply;
            var dtl = this.Detail;
            var remark = this.Detail.Remark;

            if (apy != null && dtl != null)
            {
                if (string.IsNullOrEmpty(apy.NAME))
                {
                    state.AddModelError("Apply_NAME", "公司名稱為必要項");
                }

                if (string.IsNullOrEmpty(apy.IDN))
                {
                    state.AddModelError("Apply_IDN", "統一編號為必要項");
                }

                if (string.IsNullOrEmpty(apy.CNT_NAME))
                {
                    state.AddModelError("Apply_CNT_NAME", "承辦人姓名為必要項");
                }

                if (string.IsNullOrEmpty(apy.CNT_TEL) || apy.CNT_TEL == "-#")
                {
                    state.AddModelError("CNT_TEL_Num", "連絡電話為必要項");
                }

                if (string.IsNullOrEmpty(apy.MOBILE))
                {
                    state.AddModelError("Apply_MOBILE", "行動電話為必要項");
                }

                if (string.IsNullOrEmpty(dtl.EMAIL))
                {
                    state.AddModelError("EMAIL_3", "E-MAIL為必要項");
                }

                if (string.IsNullOrEmpty(apy.ADDR))
                {
                    state.AddModelError("Apply_ADDR_ZIP_DETAIL", "地址為必要項");
                }

                if (string.IsNullOrEmpty(dtl.PRODUCTION_COUNTRY))
                {
                    state.AddModelError("Detail_PRODUCTION_COUNTRY", "生產國別為必要項");
                }

                if (string.IsNullOrEmpty(dtl.SELL_COUNTRY))
                {
                    state.AddModelError("Detail_SELL_COUNTRY", "賣方國家為必要項");
                }

                if (string.IsNullOrEmpty(dtl.TRANSFER_COUNTRY))
                {
                    state.AddModelError("Detail_TRANSFER_COUNTRY", "起運國家為必要項");
                }

                if (remark.REMARK1 == "Y" && string.IsNullOrEmpty(remark.REMARK1_ITEM1_COMMENT))
                {
                    state.AddModelError("Detail_Remark_REMARK1_E", "報單號碼為必要項");
                }
                if (remark.REMARK1 == "Y" && string.IsNullOrEmpty(remark.REMARK1_ITEM2))
                {
                    state.AddModelError("Detail_Remark_REMARK1_E", "申請H01用途為必要項");
                }
                else if (remark.REMARK1 == "Y" && remark.REMARK1_ITEM2 == "2" && string.IsNullOrEmpty(remark.REMARK1_ITEM2_COMMENT))
                {
                    state.AddModelError("Detail_Remark_REMARK1_E", "申請H01非中藥材用途為必要項");
                }
                if (remark.REMARK3_1 == "Y" && string.IsNullOrEmpty(remark.REMARK3_2))
                {
                    remark.REMARK3_2 = "1";
                    //state.AddModelError("Detail_Remark_REMARK1_E", "非中藥用途貨品進口為必要項");
                }
                if (remark.REMARK3_1 == "Y")
                {
                    switch (remark.REMARK3_2)
                    {
                        case "1":
                            if (string.IsNullOrEmpty(remark.REMARK3_2_COMMENT)) { state.AddModelError("Detail_Remark_REMARK1_E", "請輸入食品用途"); }
                            break;
                        case "2":
                            if (string.IsNullOrEmpty(remark.REMARK3_3_COMMENT)) { state.AddModelError("Detail_Remark_REMARK1_E", "請輸入研發用途"); }
                            break;
                        case "3":
                            if (string.IsNullOrEmpty(remark.REMARK3_4_COMMENT)) { state.AddModelError("Detail_Remark_REMARK1_E", "請輸入試製用途"); }
                            break;
                        case "4":
                            if (string.IsNullOrEmpty(remark.REMARK3_5_COMMENT)) { state.AddModelError("Detail_Remark_REMARK1_E", "請輸入\"其他\"用途"); }
                            break;
                    }
                }

                if (applyitem == "1" && this.ApplyItems2 != null && this.ApplyItems2.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(this.ApplyItems2.FirstOrDefault().COMMODITIES))
                    {
                        state.AddModelError("Detail_Remark_REMARK1_E", "一般申請項目、萃取物(提取物)項目，擇一填寫");
                    }
                }
                else if (applyitem == "2" && this.ApplyItems != null && this.ApplyItems.Count > 1)
                {
                    if (!string.IsNullOrWhiteSpace(this.ApplyItems.FirstOrDefault().COMMODITIES))
                    {
                        state.AddModelError("Detail_Remark_REMARK1_E", "一般申請項目、萃取物(提取物)項目，擇一填寫");
                    }
                }
                else if (applyitem == "undefined")
                {
                    state.AddModelError("Detail_Remark_REMARK1_E", "一般申請項目、萃取物(提取物)項目，擇一填寫");
                }
                if (this.ApplyItems != null && this.ApplyItems.Count > 0 && this.ApplyItems2 != null && this.ApplyItems2.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(this.ApplyItems2.FirstOrDefault().COMMODITIES) && !string.IsNullOrWhiteSpace(this.ApplyItems.FirstOrDefault().COMMODITIES))
                    {
                        state.AddModelError("Detail_Remark_REMARK1_E", "一般申請項目、萃取物(提取物)項目，擇一填寫");
                    }
                }
                if (applyitem == "1" && this.ApplyItems != null && this.ApplyItems.Count > 0)
                {
                    foreach (var item in this.ApplyItems)
                    {
                        if (!item.ITEM_TYPE.HasValue)
                        {
                            state.AddModelError(item.ID + "_ITEM_TYPE", "一般_類型為必要項");
                        }
                        if (string.IsNullOrEmpty(item.PORCTYPE))
                        {
                            state.AddModelError(item.ID + "_PORCTYPE", "一般_產品類別為必要項");
                        }
                        if (string.IsNullOrEmpty(item.COMMODITIES))
                        {
                            state.AddModelError(item.ID + "_COMMODITIES", "一般_貨名為必要項");
                        }
                        if (string.IsNullOrEmpty(item.COMMODTYPE))
                        {
                            state.AddModelError(item.ID + "_COMMODTYPE", "一般_劑型為必要項");
                        }
                        if (string.IsNullOrEmpty(item.QTY))
                        {
                            state.AddModelError(item.ID + "_QTY", "一般_申請數量為必要項");
                        }
                        if (string.IsNullOrEmpty(item.UNIT))
                        {
                            state.AddModelError(item.ID + "_UNIT", "一般_申請數量單位為必要項");
                        }
                        if (string.IsNullOrEmpty(item.SPECQTY))
                        {
                            state.AddModelError(item.ID + "_SPECQTY", "一般_規格數量為必要項");
                        }
                        if (String.IsNullOrEmpty(item.SPECUNIT))
                        {
                            state.AddModelError(item.ID + "_SPECUNIT", "一般_規格數量單位為必要項");
                        }
                        if (string.IsNullOrEmpty(item.AFF1_EXPORT_COUNTRY))
                        {
                            item.AFF1_EXPORT_COUNTRY = dtl.TRANSFER_COUNTRY;
                            //state.AddModelError(item.ID + "_AFF1_EXPORT_COUNTRY", "一般_出口國為必要項");
                        }
                        if (string.IsNullOrEmpty(item.AFF1_IMPORT_COUNTRY))
                        {
                            state.AddModelError(item.ID + "_AFF1_IMPORT_COUNTRY", "一般_進口關為必要項");
                        }
                    }
                }
                if (applyitem == "2" && this.ApplyItems2 != null && this.ApplyItems2.Count > 0)
                {
                    foreach (var item in this.ApplyItems2)
                    {
                        if (!item.ITEM_TYPE.HasValue)
                        {
                            state.AddModelError(item.ID + "_ITEM_TYPE", "萃取物_類型為必要項");
                        }
                        if (string.IsNullOrEmpty(item.PORCTYPE))
                        {
                            state.AddModelError(item.ID + "_PORCTYPE", "萃取物_產品類別為必要項");
                        }
                        if (string.IsNullOrEmpty(item.COMMODITIES))
                        {
                            state.AddModelError(item.ID + "_COMMODITIES", "萃取物_貨名為必要項");
                        }
                        if (string.IsNullOrEmpty(item.COMMODTYPE))
                        {
                            state.AddModelError(item.ID + "_COMMODTYPE", "萃取物_劑型為必要項");
                        }
                        if (string.IsNullOrEmpty(item.QTY))
                        {
                            state.AddModelError(item.ID + "_QTY", "萃取物_申請數量為必要項");
                        }
                        if (string.IsNullOrEmpty(item.UNIT))
                        {
                            state.AddModelError(item.ID + "_UNIT", "萃取物_申請數量單位為必要項");
                        }
                        if (string.IsNullOrEmpty(item.SPECQTY))
                        {
                            state.AddModelError(item.ID + "_SPECQTY", "萃取物_規格數量為必要項");
                        }
                        if (String.IsNullOrEmpty(item.SPECUNIT))
                        {
                            state.AddModelError(item.ID + "_SPECUNIT", "萃取物_規格數量單位為必要項");
                        }
                        if (string.IsNullOrEmpty(item.AFF2_EXPORT_COUNTRY))
                        {
                            item.AFF2_EXPORT_COUNTRY = dtl.TRANSFER_COUNTRY;
                            //state.AddModelError(item.ID + "_AFF2_EXPORT_COUNTRY", "萃取物_出口國為必要項");
                        }
                        if (string.IsNullOrEmpty(item.AFF2_AMOUNT_NAME))
                        {
                            state.AddModelError(item.ID + "_AFF2_AMOUNT_NAME", "萃取物_原中藥材名稱為必要項");
                        }
                        if (string.IsNullOrEmpty(item.AFF2_AMOUNT))
                        {
                            state.AddModelError(item.ID + "_AFF2_AMOUNT", "萃取物_原中藥材含量為必要項");
                        }
                        if (string.IsNullOrEmpty(item.AFF2_IMPORT_COUNTRY))
                        {
                            state.AddModelError(item.ID + "_AFF2_IMPORT_COUNTRY", "萃取物_進口關為必要項");
                        }
                    }
                }
            }
        }
    }

    public class Apply_005014ApplyModel : ApplyModel
    {
        public string CONTACT_TEL_Zip { get; set; }
        public string CONTACT_TEL_Phone { get; set; }
        public string CONTACT_TEL_Num { get; set; }

        public string ADDR_ZIP { get; set; }
        public string ADDR_ZIP_ADDR { get; set; }
        public string ADDR_ZIP_DETAIL { get; set; }

        public string TEL_Zip { get; set; }
        public string TEL_Phone { get; set; }
        public string TEL_Num { get; set; }

        public string CNT_TEL_Zip { get; set; }

        public string CNT_TEL_Phone { get; set; }

        public string CNT_TEL_Num { get; set; }

        public string PRO_ACC_NAME { get; set; }

    }

    public class Apply_005014DetailModel : Apply_005014
    {
        public Apply_005014DetailModel()
        {
            this.Remark = new Apply_005014_REMARKExt();
        }
        public bool AFFIDAVIT2_CHECKED { get; set; }

        public string Aff1ImportCountryL1 { get; set; }

        public string Aff2ImportCountryL1 { get; set; }

        public Apply_005014_REMARKExt Remark { get; set; }
    }

    public class Apply_005014_ItemExt : Apply_005014_Item
    {
        public string ID { get; set; }
        // 出口關 進口關中文名稱
        public string AFF1_IMPORT_NAME { get; set; }
        public string AFF1_EXPORT_NAME { get; set; }
    }

    public class Apply_005014_Item2Ext : Apply_005014_Item
    {
        public string ID { get; set; }
        // 出口關 進口關中文名稱
        public string AFF2_IMPORT_NAME { get; set; }
        public string AFF2_EXPORT_NAME { get; set; }
    }

    public class Apply_005014_REMARKExt : Apply_005014_REMARK
    {
        public bool checkboxR1 { get; set; }
        public bool checkboxR2 { get; set; }
        public bool checkboxR3 { get; set; }
    }
}