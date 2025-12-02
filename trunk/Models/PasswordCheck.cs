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
    public class PasswordCheckModel
    {
        /// <summary>
        /// 長度至少8字元
        /// </summary>
        public bool Chk_1 { get; set; }

        /// <summary>
        /// 包含小寫英文字母
        /// </summary>
        public bool Chk_2 { get; set; }

        /// <summary>
        /// 包含大寫英文字母
        /// </summary>
        public bool Chk_3 { get; set; }

        /// <summary>
        /// 包含數字
        /// </summary>
        public bool Chk_4 { get; set; }

        /// <summary>
        /// 不含空白字元及特殊字元
        /// </summary>
        public bool Chk_5 { get; set; }

        /// <summary>
        /// 密碼與重複密碼不相符
        /// </summary>
        public bool Chk_6 { get; set; }

        /// <summary>
        /// 變更後密碼不得與前一次密碼相同
        /// </summary>
        public bool Chk_7 { get; set; }

        /// <summary>
        /// 變更後密碼不得與帳號相同
        /// </summary>
        public bool Chk_8 { get; set; }
    }
}