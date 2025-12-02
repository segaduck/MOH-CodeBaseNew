using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Commons;
using System.Web.Routing;
using ES.Services;

namespace ES.Areas.Admin.Models
{
    public class CaseModel
    {

        public virtual int NowPage { get; set; }

        [Display(Name = "申辦編號")]
        public virtual String APP_ID_BEGIN { get; set; }

        [Display(Name = "申辦編號")]
        public virtual String APP_ID_END { get; set; }

        public virtual DateTime? APP_TIME_BEGIN { get; set; }
        [Display(Name = "申辦日期")]
        public virtual string APP_TIME_BEGIN_AD { get; set; }

        public virtual DateTime? APP_TIME_END { get; set; }
        [Display(Name = "申辦日期")]
        public virtual string APP_TIME_END_AD { get; set; }

        [Display(Name = "公文文號")]
        public virtual string MOHW_CASE_NO { get; set; }

        [Display(Name = "處理進度")]
        public virtual string FLOW_CD { get; set; }

        [Display(Name = "身份字號/統編")]
        public virtual string IDN { get; set; }

        [Display(Name = "排序欄位")]
        public virtual string OderByCol { get; set; }

        [Display(Name = "排序方法")]
        public virtual string SortAZ { get; set; }

        [Display(Name = "結案狀態")]
        public virtual string CLOSE_MK { get; set; }

        [Display(Name = "許可證字號(字)")]
        public virtual string LIC_CD { get; set; }

        [Display(Name = "許可證字號(號)")]
        public virtual string LIC_NUM { get; set; }

        [Display(Name = "藥廠編號")]
        public virtual string MF_CD { get; set; }

        [Display(Name = "製造廠名稱")]
        public virtual string MF_NAME { get; set; }

        [Display(Name = "品名(中英文)")]
        public virtual string DRUG_NAME { get; set; }

        [Display(Name = "成分")]
        public virtual string INGR_NAME { get; set; }

        [Display(Name = "效能")]
        public virtual string EFFICACY { get; set; }

        [Display(Name = "適應症")]
        public virtual string INDIOCATION { get; set; }

        [Display(Name = "劑型")]
        public virtual string DOSAGE_FORM { get; set; }

        [Display(Name = "承辦人")]
        public virtual string PRO_ACC { get; set; }


        [Display(Name = "申請項目")]
        public virtual string Apply_Item { get; set; }

        public string IS_HOME_PAGE { get; set; }

        public string UNIT_CD { get; set; }

        public string FLOW_CD_ITEM { get; set; }

        [Display(Name = "申請人姓名")]
        public virtual string AP_NAME { get; set; }
    }

    public class CaseQueryModel
    {
        public virtual string ActionType { get; set; }

        public virtual string APP_ID { get; set; }

        public virtual string SRV_ID { get; set; }

        public virtual DateTime? APP_TIME { get; set; }

        public virtual DateTime? APP_EXT_DATE { get; set; }

        public virtual string NAME { get; set; }

        public virtual string PRO_ACC { get; set; }

        public virtual int? PRO_UNIT_CD { get; set; }

        public virtual string FLOW_CD { get; set; }

        public virtual string IDN { get; set; }

        public virtual DateTime? APP_STR_DATE { get; set; }

        public virtual string PAY_POINT { get; set; }

        public virtual string PAY_METHOD { get; set; }

        public virtual string MOHW_CASE_NO { get; set; }

        public virtual int? PAY_A_FEE { get; set; }

        public virtual int? PAY_A_PAID { get; set; }

        public virtual int? PAY_C_FEE { get; set; }

        public virtual int? UNIT_CD { get; set; }

        public virtual string SRV_NAME { get; set; }

        public virtual string ACC_NAME { get; set; }

        public virtual Boolean CASE_BACK_MK { get; set; }

        public virtual Boolean PAY_BACK_MK { get; set; }

        public virtual string CASE_STATUS { get; set; }

        public virtual string Apply_Item { get; set; }

    }

    public class CaseQueryExcelModel
    {
        /// <summary>
        /// 申請編號
        /// </summary>
        public virtual string APP_ID { get; set; }

        /// <summary>
        /// 申請日期
        /// </summary>
        public virtual DateTime? APP_TIME { get; set; }

        /// <summary>
        /// 處理進度
        /// </summary>
        public virtual string CASE_STATUS { get; set; }

        /// <summary>
        /// 許可證字號(字)
        /// </summary>
        public virtual string LIC_CD { get; set; }

        /// <summary>
        /// 許可證字號(號)
        /// </summary>
        public virtual string LIC_NUM { get; set; }


        /// <summary>
        /// 藥廠編號
        /// </summary>
        public virtual string MF_CD { get; set; }

        /// <summary>
        /// 製造廠名稱
        /// </summary>
        public virtual string NAME { get; set; }

        /// <summary>
        /// 中文品名
        /// </summary>
        public virtual string DRUG_NAME { get; set; }

        /// <summary>
        /// 英文品名
        /// </summary>
        public virtual string DRUG_NAME_E { get; set; }

        /// <summary>
        /// 主成分
        /// </summary>
        public virtual string PC_NAME { get; set; }

        /// <summary>
        /// 副成分
        /// </summary>
        public virtual string DI_NAME { get; set; }

        /// <summary>
        /// 效能
        /// </summary>
        public virtual string EFFICACY { get; set; }

        /// <summary>
        /// 適應症
        /// </summary>
        public virtual string INDIOCATION { get; set; }

        /// <summary>
        /// 劑型
        /// </summary>
        public virtual string DOSAGE_FORM { get; set; }

        /// <summary>
        /// 承辦人
        /// </summary>
        public virtual string ACC_NAME { get; set; }

        public string WhereController
        {
            get
            {
                string url = RouteTable.Routes.GetRouteData(new HttpContextWrapper(HttpContext.Current)).TONotNullString();

                return url;
            }
            set { string url = RouteTable.Routes.GetRouteData(new HttpContextWrapper(HttpContext.Current)).TONotNullString(); }
        }
    }
}