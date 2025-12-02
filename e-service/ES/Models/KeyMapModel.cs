using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ES.Models
{
    /// <summary>
    /// 系統共用代碼 Model
    /// </summary>
    public class KeyMapModel
    {
        /// <summary>
        /// 代碼
        /// </summary>
        public string CODE { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string TEXT { get; set; }

        /// <summary>
        /// 此項目在下拉選單中是否為選取狀態
        /// </summary>
        public bool Selected { get; set; }
    }
}
