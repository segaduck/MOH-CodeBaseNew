using ES.Commons;
using ES.DataLayers;
using ES.Models.Entities;
using ES.Models.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ES.Areas.BACKMIN.Models
{
    public class Apply_005002ViewModel : ES.Models.ViewModels.Apply_005002ViewModel
    {
        public Apply_005002ViewModel()
        {
            this.SetupFileMap();
        }

        public Apply_FileModel ApplyFile_4 { get; set; }
        public Apply_FileModel ApplyFile_5 { get; set; }
        public HttpPostedFileBase UploadFile_4 { get; set; }
        public HttpPostedFileBase UploadFile_5 { get; set; }
        public IList<TblAPPLY_NOTICE> NoticeList { get; set; }
        public IList<Apply_File_LogModel> FileLogList { get; set; }

        public Dictionary<string, string> FieldNameMap;

        /// <summary>
        /// 承辦人姓名
        /// </summary>
        public string PRO_ACC_NAME { get; set; }

        /// <summary>
        ///  承辦人儲存案件資料
        /// </summary>
        public new void SaveApply(ModelStateDictionary state)
        {
            try
            {
                BackApplyDAO dao = new BackApplyDAO();
                this.Validate(state);
                this.BeforeSave();
                dao.SaveApply_005002(this);
                this.SendMail(dao);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        private void SendMail(BackApplyDAO dao)
        {
            string msg = null;
            if (this.Apply.FLOW_CD == "2" || this.Apply.FLOW_CD == "4")
            {
                msg = dao.SendMail_005002Notice(this);
            }
            else if (this.Apply.FLOW_CD == "0" || this.Apply.FLOW_CD == "20")
            {
                dao.CaseFinishMail_005002(this);
            }
        }

        /// <summary>
        /// 取得案件申請資料 
        /// </summary>
        public new void GetApplyData()
        {
            base.GetApplyData();
            ShareDAO dao = new ShareDAO();
            this.ApplyFile_4 = dao.GetRow(new Apply_FileModel { APP_ID = this.APP_ID, FILE_NO = 4 });
            this.ApplyFile_5 = dao.GetRow(new Apply_FileModel { APP_ID = this.APP_ID, FILE_NO = 5 });
            this.FileLogList = dao.GetRowList(new Apply_File_LogModel { APP_ID = this.APP_ID });
            this.NoticeList = dao.GetRowList(new TblAPPLY_NOTICE { APP_ID = this.Apply.APP_ID });   // 補件歷程
            this.AfterLoad();

            BackApplyDAO backDao = new BackApplyDAO();
            backDao.GetCaseDate_005002(this);   // 取得公文日期
        }

        private void AfterLoad()
        {
            ApplyDAO dao = new ApplyDAO();
            if (this.Apply != null)
            {
                this.Apply.MAILBODY = null;

                if (!string.IsNullOrEmpty(this.Apply.PRO_ACC))
                {
                    TblADMIN adm = dao.GetRow(new TblADMIN { ACC_NO = this.Apply.PRO_ACC });
                    if (adm != null)
                    {
                        this.PRO_ACC_NAME = adm.NAME;
                    }else
                    {
                        this.PRO_ACC_NAME = this.Apply.PRO_ACC;
                    }
                }

                this.NewFlowCd = this.Apply.FLOW_CD;

                // 後台無須個別欄位顯示補正資料
                if (this.ErrataList != null && this.ErrataList.Count > 0)
                {
                    this.ErrataList = new List<ErrataModel>();
                }
            }

            if (this.ApplyFile_4 == null)
            {
                this.ApplyFile_4 = new Apply_FileModel();
            }

            if (this.ApplyFile_5 == null)
            {
                this.ApplyFile_5 = new Apply_FileModel();
            }

            if (this.NoticeList != null)
            {
                foreach (var item in this.NoticeList)
                {
                    if (this.FieldNameMap.ContainsKey(item.Field))
                    {
                        item.Field_NAME = this.FieldNameMap[item.Field];
                    }
                }
            }

        }

        public new void BindData()
        {
            Apply_005002ViewModel model = JsonConvert.DeserializeObject<Apply_005002ViewModel>(this.ModelJson,
                new StringConverter(),
                new IntConverter(),
                new NullableDateTimeConverter());
            this.Apply = model.Apply;
            this.Detail = model.Detail;
            this.Pay = model.Pay;
            this.IngredientList = model.IngredientList;
            this.ExcipientList = model.ExcipientList;
            this.ErrataList = model.ErrataList;
            this.NewFlowCd = model.NewFlowCd;
            this.AnoField = model.AnoField;
        }

        public void BeforeSave()
        {
            this.Apply.FLOW_CD = this.NewFlowCd;
        }

        public void ResetErrataList()
        {
            ShareDAO dao = new ShareDAO();
            this.NoticeList = dao.GetRowList(new TblAPPLY_NOTICE { APP_ID = this.APP_ID })
                .Where(x => x.ISADDYN == "Y")
                .OrderByDescending(x => x.FREQUENCY)
                .ToList();

            foreach (var item in this.NoticeList)
            {
                item.Field_NAME = this.FieldNameMap.ContainsKey(item.Field) ? this.FieldNameMap[item.Field] : null;
            }
        }

        private void SetupFileMap()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("DOSAGE_FORM", "劑型(中文)");
            dict.Add("DOSAGE_FORM_E", "劑型(英文)");
            dict.Add("DRUG_ABROAD_NAME", "外銷品名(中文)");
            dict.Add("DRUG_ABROAD_NAME_E", "外銷品名(英文)");
            dict.Add("DRUG_NAME", "藥品名稱(中文)");
            dict.Add("DRUG_NAME_E", "藥品名稱(英文)");
            dict.Add("EFFICACY", "效能(中文)");
            dict.Add("EFFICACY_E", "效能(英文)");
            dict.Add("EXCIPIENT", "賦形劑");
            dict.Add("EXPIR_DATE", "有效日期(中文)");
            dict.Add("INDIOCATION", "適應症(中文)");
            dict.Add("INDIOCATION_E", "適應症(英文)");
            dict.Add("INGREDIENT", "成分內容");
            dict.Add("ISSUE_DATE", "核准日期(中文)");
            dict.Add("LIC", "藥品許可證");
            dict.Add("LIC_E", "藥品許可證英文名稱");
            dict.Add("MF_ADDR", "製造廠地址");
            dict.Add("MF_ADDR_E", "製造廠英文地址");
            dict.Add("MF_CNT_NAME", "製造廠名稱(中文)");
            dict.Add("MF_CNT_NAME_E", "製造廠名稱(英文)");
            dict.Add("MF_CONT", "處方說明(中文)");
            dict.Add("MF_CONT_E", "處方說明(英文)	");
            dict.Add("PC_SCALE", "濃縮製劑");
            dict.Add("UploadFile_1", "外銷專用藥品許可證(正面)");
            dict.Add("UploadFile_2", "外銷專用藥品許可證(反面)");
            dict.Add("UploadFile_3", "處方之中藥材中英文對照表	");
            this.FieldNameMap = dict;
        }

    }

}