using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ES.Commons
{
    /// <summary>
    /// 用來配 Html.CheckBoxListFor() 使用的checkbox顯示項目定義
    /// </summary>
    public class CheckBoxListItem
    {
        /// <summary>
        /// Checkbox 的 value 值
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// 接在 Checkbox 後面的顯示文字 
        /// </summary>
        public string DisplayText { get; private set; }

        /// <summary>
        /// 該項目是否為選取狀態
        /// </summary>
        public bool Checked { get; private set; }


        /// <summary>
        /// 帶參數的建構子
        /// </summary>
        /// <param name="value"></param>
        /// <param name="displayText"></param>
        /// <param name="isChecked"></param>
        public CheckBoxListItem(string value, string displayText, bool isChecked)
        {
            this.Value = value;
            this.DisplayText = displayText;
            this.Checked = isChecked;
        }

        public override string ToString()
        {
            return string.Format("CheckBoxListItem: {0}:{1}:{2}", this.Value, this.DisplayText, this.Checked);
        }
    }
}