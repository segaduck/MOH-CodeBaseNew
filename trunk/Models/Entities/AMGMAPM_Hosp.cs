using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    public class TblAMGMAPM_Hosp : IDBRow
    {
        [IdentityDBField]
        public int? gmapm_id { get; set; }

        [Display(Name = "群組編碼")]
        public int? grp_id { get; set; }

        [Display(Name = "清單第一層")]

        public string sysid { get; set; }

        [Display(Name = "清單第二層")]
        public string modules { get; set; }

        [Display(Name = "清單第三層")]
        public string submodules { get; set; }

        [Display(Name = "編號")]
        public string prgid { get; set; }

        [Display(Name = "新增權限")]
        public string prg_i { get; set; }

        [Display(Name = "修改權限")]
        public string prg_u { get; set; }

        [Display(Name = "刪除權限")]
        public string prg_d { get; set; }

        [Display(Name = "查詢權限")]
        public string prg_q { get; set; }

        [Display(Name = "列印權限")]
        public string prg_p { get; set; }

        [Display(Name = "編輯者帳號")]
        public string moduser { get; set; }

        [Display(Name = "編輯者姓名")]
        public string modusername { get; set; }

        [Display(Name = "編輯時間")]
        public string modtime { get; set; }

        [Display(Name = "編輯IP")]
        public string modip { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.AMGMAPM_Hosp;
        }
    }
}