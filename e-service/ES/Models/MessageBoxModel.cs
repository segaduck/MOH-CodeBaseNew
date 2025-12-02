using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ES.Models
{
    public class MessageBoxModel
    {
        public MessageBoxModel(string message, int history)
        {
            this.Message = message;
            this.History = history;
        }

        public MessageBoxModel(string action, string control, string message)
        {
            this.Action = action;
            this.Control = control;
            this.Message = message;
        }

        public MessageBoxModel(string action, string control, string message, string title)
        {
            this.Action = action;
            this.Control = control;
            this.Message = message;
            this.Title = title;
        }

        /// <summary>
        /// 控制項
        /// </summary>
        public string Control { get; set; }

        /// <summary>
        /// 動作項
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// 上一頁
        /// </summary>
        public int History { get; set; }

        /// <summary>
        /// 訊息內容
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 訊息抬頭Title
        /// </summary>
        public string Title { get; set; }

    }
}