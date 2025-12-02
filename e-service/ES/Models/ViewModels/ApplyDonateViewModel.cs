using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Models.Entities;

namespace ES.Models.ViewModels
{
    /// <summary>
    /// 賑災專戶
    /// </summary>
    public class ApplyDonateViewModel
    {
        public ApplyDonateViewModel()
        {
            this.Detail = new ApplyDonateDetailModel();
        }
        public List<ApplyDonateGridModel> Grid { get; set; }
        public ApplyDonateDetailModel Detail { get; set; }
    }
    public class ApplyDonateGridModel : TblAPPLY_DONATE
    {
        /// <summary>
        /// 附件下載
        /// </summary>
        public List<string> FileGrid { get; set; }
    }
    public class ApplyDonateDetailModel : TblAPPLY_DONATE
    {
        /// <summary>
        /// 檔案上傳
        /// </summary>
        public List<string> FileList { get; set; }
    }
}