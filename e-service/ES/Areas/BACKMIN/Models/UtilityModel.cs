using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Areas.Admin.Models
{
    public class UtilityModel
    {      
        [Display(Name = "LOG種類")]
        public virtual string TX_CATE_CD { get; set; }

        [Display(Name = "操作者ID")]
        public virtual string TX_LOGIN_ID { get; set; }

        [Display(Name = "操作者姓名")]
        public virtual string TX_LOGIN_NAME { get; set; }

        [Display(Name = "單位代碼")]
        public virtual int TX_UNIT_CD { get; set; }

        [Display(Name = "操作代碼")]
        public virtual int TX_TYPE { get; set; }

        [Display(Name = "敘述")]
        public virtual string TX_DESC { get; set; }

    }    
}