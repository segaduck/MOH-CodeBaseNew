using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    public class TblAMGRP_Hosp : IDBRow
    {
        [IdentityDBField]
        public int? grp_id { get; set; }

        [Display(Name = "群組名稱")]
        public string grpname { get; set; }

        [Display(Name = "群組狀態")]
        public string grp_status { get; set; }

        [Display(Name = "所屬單位名稱")]
        public string unit_cd { get; set; }

        [Display(Name = "編輯者帳號")]
        public string moduser { get; set; }

        [Display(Name = "編輯者姓名")]
        public string modusername { get; set; }

        [Display(Name = "編輯時間")]
        public string modtime { get; set; }

        [Display(Name = "編輯IP")]
        public string modip { get; set; }

        [Display(Name = "刪除註記")]
        public string del_mk { get; set; }

        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.AMGRP_Hosp;
        }
    }
}