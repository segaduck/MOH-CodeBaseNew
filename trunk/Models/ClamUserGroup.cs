using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EECOnline.Models
{
    /// <summary>
    /// 使用者群組
    /// </summary>
    public class ClamUserGroup
    {
        /// <summary>
        /// 群組代碼
        /// </summary>
        public string GRPID { get; set; }

        /// <summary>
        /// 群組名稱
        /// </summary>
        public string GRPNAME { get; set; }
    }
}