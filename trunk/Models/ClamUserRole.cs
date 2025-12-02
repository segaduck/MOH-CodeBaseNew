using System;

namespace EECOnline.Models
{
    /// <summary>
    /// 使用者單位群組角色
    /// </summary>
    public class ClamUserRole
    {
        /// <summary>
        /// </summary>
        public string EXAMKIND { get; set; }

        /// <summary>
        /// 檢定類別名稱
        /// </summary>
        public string EXAMKIND_NAME { get; set; }

        /// <summary>
        /// 群組角色代碼
        /// </summary>
        public string ROLE { get; set; }

        /// <summary>
        /// 群組角色名稱
        /// </summary>
        public string ROLE_NAME { get; set; }
    }
}