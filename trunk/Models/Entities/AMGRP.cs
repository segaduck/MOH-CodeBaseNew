using System;
using System.ComponentModel.DataAnnotations;
using EECOnline.Commons;
using Turbo.DataLayer;

namespace EECOnline.Models.Entities
{
    /// <summary>
    /// 角色群組
    /// </summary>
    public class TblAMGRP : IDBRow
    {
        /// <summary>
        /// PK
        /// </summary>
        [IdentityDBField]
        public int? grp_id { get; set; }

        /// <summary>
        /// 群組名稱
        /// </summary>
        [Display(Name = "群組名稱")]
        public string grpname { get; set; }

        /// <summary>
        /// 群組狀態
        /// 0 停用
        /// 1 啟用
        /// </summary>
        [Display(Name = "群組狀態")]
        public string grp_status { get; set; }

        /// <summary>
        /// 所屬單位名稱
        /// </summary>
        [Display(Name = "所屬單位名稱")]
        public string unit_cd { get; set; }

        /// <summary>
        /// 編輯者帳號
        /// </summary>
        [Display(Name = "編輯者帳號")]
        public string moduser { get; set; }

        /// <summary>
        /// 編輯者姓名
        /// </summary>
        [Display(Name = "編輯者姓名")]
        public string modusername { get; set; }

        /// <summary>
        /// 編輯時間
        /// </summary>
        [Display(Name = "編輯時間")]
        public string modtime { get; set; }

        /// <summary>
        /// 編輯IP
        /// </summary>
        [Display(Name = "編輯IP")]
        public string modip { get; set; }

        /// <summary>
        /// 刪除註記
        /// </summary>
        [Display(Name = "刪除註記")]
        public string del_mk { get; set; }
        public DBRowTableName GetTableName()
        {
            return StaticCodeMap.TableName.AMGRP;
        }
    }
}