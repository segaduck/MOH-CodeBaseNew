using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EECOnline.Models.Entities;

namespace EECOnline.Models
{
    /// <summary>
    /// 使用者群組
    /// </summary>
    public class ClamUser : TblAMDBURM
    {
        /// <summary>
        /// 服務單位
        /// </summary>
        public string UNIT_NAME { get; set; }
    }
}