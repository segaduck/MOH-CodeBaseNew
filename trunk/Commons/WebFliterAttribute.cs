using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using EECOnline.Commons;
using Turbo.DataLayer;
using System.Reflection;

namespace EECOnline.Commons
{
    /// <summary>
    /// Web檢核Attribute名稱
    /// </summary>
    public class WebFliterAttribute : Attribute
    {
        public WebFliter WFliter { get; set; }

        public int Size { get; set; }

        public string Name { get; set; }
    }

    /// <summary>
    /// 檢核格式項目類別
    /// </summary>
    public enum WebFliter
    {
        Default,

        /// <summary>
        /// 日期
        /// </summary>
        DATE,

        /// <summary>
        /// 身分證格式
        /// </summary>
        IDNO,

        /// <summary>
        /// 性別
        /// </summary>
        SEX,

        /// <summary>
        /// 是否申請
        /// </summary>
        APPLYSTATUS,

        /// <summary>
        /// 5+N類別
        /// </summary>
        FIVE_N,

        /// <summary>
        /// 系統別
        /// </summary>
        SYSID,


        /// <summary>
        /// 分署別
        /// </summary>
        BRANCHID,


        /// <summary>
        /// 請假類別
        /// </summary>
        VACSTATUS,

        /// <summary>
        /// 離退訓情況
        /// </summary>
        LEAVESTATUS,

        /// <summary>
        /// 處理資料模式(I/U)
        /// </summary>
        PROCESS,

        /// <summary>
        /// 處理資料模式(I/U/D)
        /// </summary>
        PROCESS_ALL,
    }
}
