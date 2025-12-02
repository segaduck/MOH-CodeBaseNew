using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;
using Newtonsoft.Json;
using ES.DataLayers;
using System.Web.Mvc;
using Newtonsoft.Json.Converters;
using ES.Commons;

namespace ES.Models.ViewModels
{
    /// <summary>
    /// 外銷證明書 
    /// </summary>
    public class Apply_005002ViewModel
    {
        public string APP_ID { get; set; }
        public ApplyModel Apply { get; set; }
        public Apply_005002Model Detail { get; set; }
        public APPLY_PAY Pay { get; set; }
        public IList<ErrataModel> ErrataList { get; set; }

        public Apply_005002FieldModel AnoField { get; set; }

        /// <summary>
        /// 新設定的案件狀態
        /// </summary>
        public string NewFlowCd { get; set; }

        /// <summary>
        /// 成份內容 
        /// </summary>
        public IList<Apply_005002_DiModel> IngredientList { get; set; }

        /// <summary>
        /// 賦形劑
        /// </summary>
        public IList<Apply_005002_PcModel> ExcipientList { get; set; }

        /// <summary>
        /// 外銷品名
        /// </summary>
        public IList<Apply_005002_DOSAGE_FORM> DosageFormList { get; set; }

        public Apply_FileModel ApplyFile_1 { get; set; }
        public Apply_FileModel ApplyFile_2 { get; set; }
        public Apply_FileModel ApplyFile_3 { get; set; }
        public HttpPostedFileBase UploadFile_1 { get; set; }
        public HttpPostedFileBase UploadFile_2 { get; set; }
        public HttpPostedFileBase UploadFile_3 { get; set; }

        public string ModelJson { get; set; }

        public Apply_005002ViewModel()
        {
            this.Apply = new ApplyModel();
            this.Detail = new Apply_005002Model();
            this.Pay = new APPLY_PAY();
            this.IngredientList = new List<Apply_005002_DiModel>();
            this.ExcipientList = new List<Apply_005002_PcModel>();
            this.DosageFormList = new List<Apply_005002_DOSAGE_FORM>();
            this.ErrataList = new List<ErrataModel>();
            this.ApplyFile_1 = new Apply_FileModel();
            this.ApplyFile_2 = new Apply_FileModel();
            this.ApplyFile_3 = new Apply_FileModel();
            this.AnoField = new Apply_005002FieldModel();
        }

        public void GetApplyData()
        {
            if (this.APP_ID != null)
            {
                ApplyDAO dao = new ApplyDAO();
                this.Apply = dao.GetRow(new ApplyModel { APP_ID = this.APP_ID });
                this.Detail = dao.GetRow(new Apply_005002Model { APP_ID = this.APP_ID });
                this.Pay = dao.GetRow(new APPLY_PAY { APP_ID = this.APP_ID });
                this.IngredientList = dao.GetRowList(new Apply_005002_DiModel { APP_ID = this.APP_ID });
                this.ExcipientList = dao.GetRowList(new Apply_005002_PcModel { APP_ID = this.APP_ID });
                this.DosageFormList = dao.GetRowList(new Apply_005002_DOSAGE_FORM { APPLY_ID = this.APP_ID });
                this.ApplyFile_1 = dao.GetRow(new Apply_FileModel { APP_ID = this.APP_ID, FILE_NO = 1 });
                this.ApplyFile_2 = dao.GetRow(new Apply_FileModel { APP_ID = this.APP_ID, FILE_NO = 2 });
                this.ApplyFile_3 = dao.GetRow(new Apply_FileModel { APP_ID = this.APP_ID, FILE_NO = 3 });

                if (this.Apply != null)
                {
                    this.Apply.MAILBODY = null;
                }

                IList<TblAPPLY_NOTICE> noticeList = dao.GetRowList(new TblAPPLY_NOTICE { APP_ID = APP_ID })
                    .OrderByDescending(x => x.FREQUENCY)
                    .ToList();
                if (noticeList != null && noticeList.Count > 0)
                {
                    var maxfreq = noticeList[0].FREQUENCY;
                    this.ErrataList = noticeList
                        .Where(x => x.FREQUENCY.Value == maxfreq)
                        .Select(x => new ErrataModel
                        {
                            IsSel = x.ISADDYN == "Y",
                            Name = x.Field,
                            Note = x.NOTE
                        })
                        .ToList();
                }
            }
            else
            {
                this.Apply = new ApplyModel();
                this.Detail = new Apply_005002Model();
                this.Pay = new APPLY_PAY();
                this.IngredientList = new List<Apply_005002_DiModel>();
                this.ExcipientList = new List<Apply_005002_PcModel>();
                this.DosageFormList = new List<Apply_005002_DOSAGE_FORM>();

                this.ApplyFile_1 = new Apply_FileModel();
                if (this.UploadFile_1 != null)
                {
                    this.ApplyFile_1.FILENAME = this.UploadFile_1.FileName;
                }

                this.ApplyFile_2 = new Apply_FileModel();
                if (this.UploadFile_2 != null)
                {
                    this.ApplyFile_2.FILENAME = this.UploadFile_2.FileName;
                }

                this.ApplyFile_3 = new Apply_FileModel();
                if (this.UploadFile_2 != null)
                {
                    this.ApplyFile_2.FILENAME = this.UploadFile_2.FileName;
                }
            }
        }

        /// <summary>
        /// 案件補件狀態, true 則前台使用者鎖住編輯欄位
        /// </summary>
        public bool IsReadonly()
        {
            bool res = false;
            ShareDAO dao = new ShareDAO();
            if (this.Apply != null && (this.Apply.FLOW_CD == "2" || this.Apply.FLOW_CD == "4"))
            {
                if (dao.CalculationDocDate("005002", APP_ID))
                {
                    res = true;  //已過補件期限
                }
                else
                {
                    res = false;
                }
            }
            return res;
        }

        /// <summary>
        /// 已過補件期限 
        /// </summary>
        /// <returns></returns>
        public bool IsOverDue()
        {
            bool res = false;
            ShareDAO dao = new ShareDAO();
            if (dao.CalculationDocDate("005002", APP_ID))
            {
                res = true;
            }
            return res;
        }

        /// <summary>
        /// 儲存新申請資料
        /// </summary>
        public void SaveApply(ModelStateDictionary state)
        {
            this.Validate(state);
            this.Apply.FLOW_CD = "1"; // 新申請收件
            ApplyDAO dao = new ApplyDAO();
            dao.AppendApply005002(this);
            dao.SendMail_005002Notice(this);
        }

        /// <summary>
        /// 補件重新送件
        /// </summary>
        /// <param name="state"></param>
        public void SaveResend(ModelStateDictionary state)
        {
            this.Validate(state);
            ApplyDAO dao = new ApplyDAO();
            dao.UpdateApply005002(this);
            dao.SendMail_005002Resend(this);
        }

        public void BindData()
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
            this.ApplyFile_1 = model.ApplyFile_1;
            this.ApplyFile_2 = model.ApplyFile_2;
            this.ApplyFile_3 = model.ApplyFile_3;
        }

        public void Validate(ModelStateDictionary state)
        {
            // 申辦份數為必要項
            if (!this.Detail.MF_COPIES.HasValue)
            {
                state.AddModelError("MF_COPIES", "申辦份數為必要項");
            }

            // 公司名稱為必要項
            if (string.IsNullOrEmpty(this.Apply.NAME))
            {
                state.AddModelError("Apply_NAME", "公司名稱為必要項");
            }

            // 聯絡人為必要項
            if (string.IsNullOrEmpty(this.Apply.CNT_NAME))
            {
                state.AddModelError("CNT_NAME", "聯絡人為必要項");
            }

            // 電話為必要項
            if (string.IsNullOrEmpty(this.Apply.TEL))
            {
                state.AddModelError("CNT_TEL", "電話為必要項");
            }

            // E-MAIL為必要項
            if (string.IsNullOrEmpty(this.Detail.EMAIL))
            {
                state.AddModelError("EMAIL", "E-MAIL為必要項");
            }

            // 藥品許可證字號(中英文)為必要項
            if (string.IsNullOrEmpty(this.Detail.LIC_CD))
            {
                state.AddModelError("LIC_CD", "藥品許可證字號(中文)為必要項");
            }

            if (string.IsNullOrEmpty(this.Detail.LIC_CD_E))
            {
                state.AddModelError("LIC_CD_E", "藥品許可證字號(英文)為必要項");
            }

            // 製造廠名稱(中英文)為必要項
            if (string.IsNullOrEmpty(this.Detail.MF_CNT_NAME))
            {
                state.AddModelError("MF_CNT_NAME", "製造廠名稱(中文)為必要項");
            }
            if (string.IsNullOrEmpty(this.Detail.MF_CNT_NAME_E))
            {
                state.AddModelError("MF_CNT_NAME_E", "製造廠名稱(英文)為必要項");
            }

            // 製造廠地址為必要項
            if (string.IsNullOrEmpty(this.Detail.MF_ADDR))
            {
                state.AddModelError("MF_ADDR", "製造廠地址(中文)為必要項");
            }
            if (string.IsNullOrEmpty(this.Detail.MF_ADDR_E))
            {
                state.AddModelError("MF_ADDR_E", "製造廠地址(英文)為必要項");
            }

            // 藥品名稱(中文)為必要項
            if (string.IsNullOrEmpty(this.Detail.DRUG_NAME))
            {
                state.AddModelError("DRUG_NAME", "藥品名稱(中文)為必要項");
            }
            if (string.IsNullOrEmpty(this.Detail.DRUG_NAME_E))
            {
                state.AddModelError("DRUG_NAME_E", "藥品名稱(英文)為必要項");
            }

            //// 劑型(中英文)為必要項
            if (string.IsNullOrEmpty(this.Detail.DOSAGE_FORM))
            {
                state.AddModelError("DOSAGE_FORM", "劑型(中文)為必要項");
            }
            if (string.IsNullOrEmpty(this.Detail.DOSAGE_FORM_E))
            {
                state.AddModelError("DOSAGE_FORM_E", "劑型(英文)為必要項");
            }

            // 核准日期(中文)為必要項
            if (!this.Detail.ISSUE_DATE.HasValue)
            {
                state.AddModelError("ISSUE_DATE", "核准日期(中文)為必要項");
            }

            // 處方說明(中英文)為必要項
            if (string.IsNullOrEmpty(this.Detail.MF_CONT))
            {
                state.AddModelError("MF_CONT", "處方說明(中文)為必要項");
            }
            if (string.IsNullOrEmpty(this.Detail.MF_CONT_E))
            {
                state.AddModelError("MF_CONT_E", "處方說明(英文)為必要項");
            }

        }

    }

    public class ErrataModel
    {
        public string Name { get; set; }
        public bool IsSel { get; set; }
        public string Note { get; set; }
    }

    public class Apply_005002FieldModel
    {
        /// <summary>
        /// 公文日期
        /// </summary>
        public string MOHW_CASE_DATE { get; set; }
    }

}