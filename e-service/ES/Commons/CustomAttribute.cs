using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using ES.Commons;
using System.Reflection;

namespace ES.Commons
{
    /// <summary>
    /// 控制項Attribute名稱
    /// </summary>
    public class ControlAttribute : Attribute
    {
        /// <summary>
        /// 控項類別
        /// </summary>
        public Control Mode { get; set; }

        /// <summary>
        /// 屬性
        /// </summary>
        public PropertyInfo pi { get; set; }

        /// <summary>
        /// 控制項狀態
        /// </summary>
        public object HtmlAttribute { get; set; }

        /// <summary>
        /// 控制項NewOrModify情況
        /// </summary>
        public bool IsOpenNew { get; set; }

        /// <summary>
        /// 控制項ReadOnly情況
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// 寬度
        /// </summary>
        public string size { get; set; }

        /// <summary>
        /// 列數(TextArea專用)
        /// </summary>
        public int rows { get; set; }

        /// <summary>
        /// 行數(TextArea專用)
        /// </summary>
        public int columns { get; set; }

        /// <summary>
        /// 文字(最大長度)
        /// </summary>
        public string maxlength { get; set; }

        /// <summary>
        /// 控制項提示
        /// </summary>
        public string placeholder { get; set; }

        /// <summary>
        /// 觸發事件
        /// </summary>
        public string onblur { get; set; }

        #region 同一個form裡面(同一行)
        /// <summary>
        /// 放置於同一個欄位
        /// </summary>
        public int group { get; set; }

        /// <summary>
        /// 設定group_form Id
        /// </summary>
        public string form_id { get; set; }
        #endregion
        #region 同一個toggle_block裡面(同一區塊)

        /// <summary>
        /// 放置於同一個區塊
        /// </summary>
        public int block_toggle_group { get; set; }

        /// <summary>
        /// 放置於同一個區塊
        /// </summary>
        public string block_toggle_id { get; set; }

        /// <summary>
        /// 是否展示縮合區塊
        /// </summary>
        public bool block_toggle { get; set; }

        /// <summary>
        /// 縮合名稱
        /// </summary>
        public string toggle_name { get; set; }

        #endregion
        #region 同一個block裡面(同一區塊)

        /// <summary>
        /// 放置於同一個區塊
        /// </summary>
        public int block_group { get; set; }

        /// <summary>
        /// 放置於同一個區塊
        /// </summary>
        public string block_id { get; set; }

        #endregion
        #region 最外層的Block DIV(包在最外面的)
        /// <summary>
        /// 放置於同一個區塊
        /// </summary>
        public string block_BIG_id { get; set; }
        #endregion

        /// <summary>
        /// 前綴字
        /// </summary>
        public string fontWord { get; set; }

        /// <summary>
        /// 勾選框後方文字
        /// </summary>
        public string checkBoxWord { get; set; }

        /// <summary>
        /// 客製化元件VIEW名稱(放在EditorTemplates)
        /// </summary>
        public string EditorViewName { get; set; }

        /// <summary>
        /// 小圖檢視預覽名稱
        /// </summary>
        public string HoverFileName { get; set; }

        /// <summary>
        /// onChange事件
        /// </summary>
        public string onChangeFun { get; set; }

        /// <summary>
        /// Log所需的Schema
        /// </summary>
        public string LogSchema { get; set; }

        /// <summary>
        /// 打包下載名稱
        /// </summary>
        public string CaseName { get; set; }

        /// <summary>
        /// 每個輸入屬性下方備註
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// 每個輸入屬性超連結
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// 每個輸入屬性超連結網址
        /// </summary>
        public string LinkHref { get; set; }

        /// <summary>
        /// 限制檔案類型(0:預設限制型態 / 1:pdf及圖檔型態)
        /// </summary>
        public string LimitFileType { get; set; }

        /// <summary>
        /// 上傳容量限制(MB)
        /// </summary>
        public string MaxFileSize { get; set; }

        /// <summary>
        /// 上傳物件描述文字
        /// </summary>
        public string UploadDesc { get; set; }
    }

    /// <summary>
    /// 控項類別
    /// </summary>
    public enum Control
    {
        /// <summary>
        /// 隱藏元件
        /// </summary>
        Hidden,

        /// <summary>
        /// 文字
        /// </summary>
        Lable,

        /// <summary>
        /// 輸入框
        /// </summary>
        TextBox,

        /// <summary>
        /// 伸縮輸入框
        /// </summary>
        TextArea,

        /// <summary>
        /// 下拉選單
        /// </summary>
        DropDownList,

        /// <summary>
        /// 單選
        /// </summary>
        Radio,

        /// <summary>
        /// 單選群組
        /// </summary>
        RadioGroup,

        /// <summary>
        /// 檢核
        /// </summary>
        CheckBox,

        /// <summary>
        /// 多選群組
        /// </summary>
        CheckBoxList,

        /// <summary>
        /// 日期輸入框
        /// </summary> 
        DatePicker,

        /// <summary>
        /// 電話輸入框
        /// </summary>
        Tel,

        /// <summary>
        /// 郵件輸入框
        /// </summary>
        EMAIL,

        /// <summary>
        /// 地址輸入框
        /// </summary>
        ADDR,

        /// <summary>
        /// 輸入生殖細胞或胚胎數量
        /// </summary>
        ABCNUM,

        /// <summary>
        /// 港口-岸口
        /// </summary>
        CountryPort,

        /// <summary>
        /// 檔案上傳
        /// </summary>
        FileUpload,

        /// <summary>
        /// 檔案上傳(補件)
        /// </summary>
        FileUploadAppDoc,

        /// <summary>
        /// 貨品資料
        /// </summary>
        Goods,

        /// <summary>
        /// 預覽小圖
        /// </summary>
        ImageHover,

        /// <summary>
        /// 捐款金額
        /// </summary>
        DonateAmount,

        /// <summary>
        /// 案件歷程
        /// </summary>
        Log,

        /// <summary>
        /// 打包下載
        /// </summary>
        ZipButton,

    }

    /// <summary>
    /// 用來標註 Controller Action 為允許未登入狀態下被 Invoke
    /// <para><seealso cref="ES.Controllers.BaseController"/> OnActionExecuting 會參考到這個標示</para>
    /// </summary>
    public class AllowAnonymousAttribute : Attribute {

    }
}
