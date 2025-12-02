using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Commons;

namespace ES.Models
{
    /// <summary>
    /// 申辦服務
    /// </summary>
    public class ServiceLstViewModel
    {
        public ServiceLstViewModel()
        {

        }

        /// <summary>
        /// 申辦服務頁籤(1:申辦類型/2:業務單位別)
        /// </summary>
        [Display(Name = "申辦服務頁籤")]
        public string ACT_TYPE { set; get; }

        /// <summary>
        /// 申辦服務代碼(申辦類型)(業務單位別)
        /// </summary>
        [Display(Name = "申辦服務代碼")]
        public int LST_ID { set; get; }

        public IList<ServiceLstGridModel> Grid { get; set; }
    }

    /// <summary>
    /// 申辦服務詳細項目
    /// </summary>
    public class ServiceLstGridModel
    {
        /// <summary>
        /// 申辦服務-案件名稱-申請項目名稱
        /// </summary>
        [Display(Name = "申辦服務")]
        [Control(Mode = Control.Lable, block_toggle_group = 1)]
        public string SRV_ID_NAME { get; set; }

        /// <summary>
        /// 申請項目代碼
        /// </summary>
        [Display(Name = "申請項目代碼")]
        public string SRV_ID { get; set; }

        public string SEQ_NO { get; set; }
    }
}