using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Areas.Admin.Models
{
    public class ServiceModel
    {
    }

    public class ServiceManagerActionModel
    {

    }

    public class ServiceCategoryModel
    {
        [Display(Name = "分類ID")]
        public int CategoryId { set; get; }

        [Display(Name = "父分類ID")]
        public int ParentId { set; get; }

        [Required(ErrorMessage = "請選擇所屬單位")]
        [Display(Name = "所屬單位")]
        public int UnitCode { set; get; }

        [Display(Name = "父分類名稱")]
        public string ParentName { set; get; }

        [Required(ErrorMessage = "請填寫分類名稱")]
        [Display(Name = "分類名稱")]
        public string Name { set; get; }

        [Display(Name = "層級")]
        public int Level { set; get; }

        [Required(ErrorMessage = "請填寫排序")]
        [Display(Name = "排序")]
        public int Seq { set; get; }

        [Display(Name = "異動者帳號")]
        public string UpdateAccount { set; get; }
    }

    public class ServiceEditModel
    {
        [Display(Name = "分類")]
        public string CategoryName { set; get; }

        [Display(Name = "分類ID")]
        public int CategoryId { set; get; }

        [Display(Name = "分類ID")]
        public int CategoryParentId { set; get; }

        [Display(Name = "案件編號")]
        public string ServiceId { set; get; }

        [Required(ErrorMessage = "請填寫排序")]
        [Display(Name = "排序")]
        public int Seq { set; get; }

        [Display(Name = "案件名稱")]
        public string Name { set; get; }

        [Display(Name = "案件說明")]
        public string Desc { set; get; }

        [Display(Name = "案件維護單位")]
        public int FixUnitCode { set; get; }

        [Display(Name = "分文負責人")]
        public string[] CaseMakerIds { get; set; }

        [Display(Name = "申請須知管理者")]
        public string PageMakerId { set; get; }

        [Display(Name = "書表下載管理者")]
        public string FileMakerId { set; get; }

        [Display(Name = "需要書表下載")]
        public bool DesignFileMark { set; get; }

        [Display(Name = "繳費時間")]
        public string PayPoint { set; get; }

        [Display(Name = "繳費時間")]
        public string PayPoint1 { set; get; }

        [Display(Name = "繳費時間")]
        public bool PayPoint2 { set; get; }

        [Display(Name = "繳費時間")]
        public bool PayPoint3 { set; get; }

        [Display(Name = "會計科目")]
        public string AccountName { set; get; }

        [Display(Name = "科目代碼")]
        public string AccountCode { set; get; }

        [Display(Name = "申請時繳費計算方式")]
        public string PayUnit { set; get; }

        [Display(Name = "申請時費用")]
        public int ApplyFee { set; get; }

        [Display(Name = "申請時計價的基本份數")]
        public int BaseNum { set; get; }

        [Display(Name = "申請時超過基本份數時，申請一份所需費用")]
        public int FeeExtra { set; get; }

        [Display(Name = "申請時費用")]
        public string ApplyFeeA { set; get; }

        [Display(Name = "申請時費用")]
        public string ApplyFeeB { set; get; }

        [Display(Name = "申請時計價的基本份數")]
        public string BaseNumB { set; get; }

        [Display(Name = "申請時超過基本份數時，申請一份所需費用")]
        public string FeeExtraB { set; get; }

        [Display(Name = "申請時費用")]
        public string ApplyFeeD { set; get; }

        [Display(Name = "申請時計價的基本份數")]
        public string BaseNumD { set; get; }

        [Display(Name = "申請時超過基本份數時，申請一份所需費用")]
        public string FeeExtraD { set; get; }

        [Display(Name = "申請時依資料欄位計價")]
        public string ApplyPayField { set; get; }

        [Display(Name = "審核後繳費計算方式")]
        public string ChkPayUnit { set; get; }

        [Display(Name = "審核後費用")]
        public int ChkApplyFee { set; get; }

        [Display(Name = "審核後計價的基本份數")]
        public int ChkBaseNum { set; get; }

        [Display(Name = "審核後超過基本份數時，申請一份所需費用")]
        public int ChkFeeExtra { set; get; }

        [Display(Name = "審核後費用")]
        public string ChkApplyFeeA { set; get; }

        [Display(Name = "審核後費用")]
        public string ChkApplyFeeB { set; get; }

        [Display(Name = "審核後計價的基本份數")]
        public string ChkBaseNumB { set; get; }

        [Display(Name = "審核後超過基本份數時，申請一份所需費用")]
        public string ChkFeeExtraB { set; get; }

        [Display(Name = "審核後費用")]
        public string ChkApplyFeeD { set; get; }

        [Display(Name = "審核後計價的基本份數")]
        public string ChkBaseNumD { set; get; }

        [Display(Name = "審核後超過基本份數時，申請一份所需費用")]
        public string ChkFeeExtraD { set; get; }

        [Display(Name = "繳費方式")]
        public string[] PayMethod { set; get; }

        [Display(Name = "繳費期限")]
        public bool PayDeadlineMark { set; get; }

        [Display(Name = "繳費期限")]
        public string PayDeadline { set; get; }

        [Display(Name = "處理期限")]
        public string ProDeadline { set; get; }

        [Display(Name = "轉申請資料")]
        public bool TranMisMark { set; get; }

        [Display(Name = "轉公文系統（以案案件）")]
        public bool TranArchiveMark { set; get; }

        [Display(Name = "公文代碼")]
        public string ArchiveCode { set; get; }

        [Display(Name = "研考會代碼")]
        public string RdecCode { set; get; }

        [Display(Name = "登入方式")]
        public string[] LoginType { set; get; }

        [Display(Name = "申辦資格")]
        public string[] ApplyTarget { set; get; }

        [Display(Name = "主題分類代碼")]
        public string ClassSubCode { set; get; }

        [Display(Name = "施政分類代碼")]
        public string ClassAdmCode { set; get; }

        [Display(Name = "服務分類代碼")]
        public string ClassSrvCode { set; get; }

        [Display(Name = "關鍵字")]
        public string KeyWord { set; get; }

        [Display(Name = "最後修改帳號")]
        public string UpdateAccount { set; get; }

        [Display(Name = "最後修改時間")]
        public string UpdateTime { set; get; }

        public int UnitCode { set; get; }

        [Display(Name = "單位代碼")]
        public string UnitSCode { set; get; }

        [Display(Name = "分文單位代碼")]
        public string[] AssignUnitCode { set; get; }

        [Display(Name = "信用卡繳費帳號")]
        public string PayAccount { set; get; }

        [Display(Name = "是否為通用表單設定")]
        public bool SharedMark { set; get; }

        [Display(Name = "表單設定")]
        public string FormID { set; get; }

        [Display(Name = "是否開放補件")]
        public bool ReUpdateMark { set; get; }

        public void Init()
        {
            this.PayPoint = this.PayPoint ?? "";
            if (this.PayPoint.Equals("A"))
            {
                this.PayPoint1 = "A";
            }
            else
            {
                this.PayPoint1 = "B";
            }

            if (this.PayPoint.Equals("B"))
            {
                this.PayPoint2 = true;
            }
            else
            {
                this.PayPoint2 = false;
            }

            if (this.PayPoint.Equals("C"))
            {
                this.PayPoint3 = true;
            }
            else
            {
                this.PayPoint3 = false;
            }

            if (this.PayPoint.Equals("D"))
            {
                this.PayPoint2 = true;
                this.PayPoint3 = true;
            }

            this.PayUnit = this.PayUnit ?? "";
            if (this.PayUnit.Equals("A"))
            {
                this.ApplyFeeA = this.ApplyFee.ToString();
            }
            else if (this.PayUnit.Equals("B"))
            {
                this.ApplyFeeB = this.ApplyFee.ToString();
                this.BaseNumB = this.BaseNum.ToString();
                this.FeeExtraB = this.FeeExtra.ToString();
            }
            else if (this.PayUnit.Equals("C"))
            {

            }
            else if (this.PayUnit.Equals("D"))
            {
                this.ApplyFeeD = this.ApplyFee.ToString();
                this.BaseNumD = this.BaseNum.ToString();
                this.FeeExtraD = this.FeeExtra.ToString();
            }

            if (this.ChkPayUnit.Equals("A"))
            {
                this.ChkApplyFeeA = this.ChkApplyFee.ToString();
            }
            else if (this.ChkPayUnit.Equals("B"))
            {
                this.ChkApplyFeeB = this.ChkApplyFee.ToString();
                this.ChkBaseNumB = this.ChkBaseNum.ToString();
                this.ChkFeeExtraB = this.ChkFeeExtra.ToString();
            }
            else if (this.ChkPayUnit.Equals("D"))
            {
                this.ChkApplyFeeD = this.ChkApplyFee.ToString();
                this.ChkBaseNumD = this.ChkBaseNum.ToString();
                this.ChkFeeExtraD = this.ChkFeeExtra.ToString();
            }

            if (!this.PayDeadline.Equals("0"))
            {
                this.PayDeadlineMark = true;
            }
        }

        public void SetModel()
        {
            this.PayPoint1 = this.PayPoint1 ?? "";
            if (this.PayPoint1.Equals("A"))
            {
                this.PayPoint = "A";
            }
            else if (this.PayPoint2 && this.PayPoint3)
            {
                this.PayPoint = "D";
            }
            else if (this.PayPoint3)
            {
                this.PayPoint = "C";
            }
            else if (this.PayPoint2)
            {
                this.PayPoint = "B";
            }
            else
            {
                this.PayPoint = "A";
            }

            this.PayUnit = this.PayUnit ?? "";
            if (!this.PayPoint.Equals("A"))
            {
                if (this.PayUnit.Equals("A"))
                {
                    this.ApplyFee = Int32.Parse(this.ApplyFeeA);
                    this.BaseNum = 0;
                    this.FeeExtra = 0;
                }
                else if (this.PayUnit.Equals("B"))
                {
                    this.ApplyFee = Int32.Parse(this.ApplyFeeB);
                    this.BaseNum = Int32.Parse(this.BaseNumB);
                    this.FeeExtra = Int32.Parse(this.FeeExtraB);
                }
                else if (this.PayUnit.Equals("D"))
                {
                    this.ApplyFee = Int32.Parse(this.ApplyFeeD);
                    this.BaseNum = Int32.Parse(this.BaseNumD);
                    this.FeeExtra = Int32.Parse(this.FeeExtraD);
                }
            }

            this.ChkPayUnit = this.ChkPayUnit ?? "";
            if (!this.PayPoint.Equals("A"))
            {
                if (this.ChkPayUnit.Equals("A"))
                {
                    this.ChkApplyFee = Int32.Parse(this.ChkApplyFeeA);
                    this.ChkBaseNum = 0;
                    this.ChkFeeExtra = 0;
                }
                else if (this.ChkPayUnit.Equals("B"))
                {
                    this.ChkApplyFee = Int32.Parse(this.ChkApplyFeeB);
                    this.ChkBaseNum = Int32.Parse(this.ChkBaseNumB);
                    this.ChkFeeExtra = Int32.Parse(this.ChkFeeExtraB);
                }
                else if (this.ChkPayUnit.Equals("D"))
                {
                    this.ChkApplyFee = Int32.Parse(this.ChkApplyFeeD);
                    this.ChkBaseNum = Int32.Parse(this.ChkBaseNumD);
                    this.ChkFeeExtra = Int32.Parse(this.ChkFeeExtraD);
                }
            }

            this.PayDeadline = this.PayDeadline ?? "";
            if (!this.PayDeadlineMark)
            {
                this.PayDeadline = "0";
            }
        }
    }

    public class ServiceBatchModel
    {
        /// <summary>
        /// 案件代碼
        /// </summary>
        public string[] ActionId { get; set; }

        /// <summary>
        /// 服務代碼
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// 動作類型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 異動者
        /// </summary>
        public string UpdateAccount { get; set; }
    }
}