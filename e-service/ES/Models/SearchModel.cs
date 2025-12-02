using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Models
{
    public class SearchModel
    {
        [Required(ErrorMessage="請輸入查詢條件")]
        [Display(Name = "案件名稱")]
        public string Name { get; set; }

        [Display(Name = "頁數")]
        public int NowPage { get; set; }
    }
}