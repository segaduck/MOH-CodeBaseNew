using System;
using System.Reflection;

namespace EECOnline.Commons
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

        /// <summary>
        /// div 響應式大小
        /// </summary>
        public string col { get; set; }
        

        #region 同一個form裡面(同一行)
        /// <summary>
        /// 放置於同一個欄位
        /// </summary>
        public int group { get; set; }

        /// <summary>
        /// 設定group_form Id
        /// </summary>
        public string form_id { get; set; }

        /// <summary>
        /// 設定group_form class
        /// </summary>
        public string form_class { get; set; }
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

        /// <summary>
        /// 配合Bootstrap3給縮合內樣式
        /// </summary>
        public string block_class { get; set; }

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
        /// 分隔線
        /// 沒填參數無class
        /// </summary>
        public string hr { get; set; }

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

        /// <summary>
        /// 小圖檢視預覽名稱
        /// </summary>
        public string HoverFileName { get; set; }

        /// <summary>
        /// 圖片必填
        /// </summary>
        public string Required { get; set; }

        /// <summary>
        /// 顯示執業縣市
        /// </summary>
        public string h5 { get; set; }
        
        /// <summary>
        /// 下方警語(紅字)
        /// </summary>
        public string AlterMsg { get; set; }

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
        /// 輸入框
        /// </summary>
        Model,

        /// <summary>
        /// 輸入框
        /// </summary>
        TextBox,

        /// <summary>
        /// 密碼輸入框
        /// </summary>
        PassWord,

        /// <summary>
        /// 伸縮輸入框
        /// </summary>
        TextArea,

        /// <summary>
        /// 下拉選單
        /// </summary>
        DropDownList,

        /// <summary>
        /// 下拉選單(select)
        /// </summary>
        DropDownList2,

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
        /// 文字
        /// </summary>
        Label,

        /// <summary>
        /// 單位
        /// </summary>
        UNIT,

        /// <summary>
        /// 承辦人
        /// </summary>
        OPERAT,

        /// <summary>
        /// 最新消息
        /// </summary>
        ENEWS,

        /// <summary>
        /// 系統計畫別(套用簡易查詢視窗)
        /// </summary>
        PLAN,

        /// <summary>
        /// 行政區+地址代碼
        /// </summary>
        ZIP_CO_FULL,

        /// <summary>
        /// 行政區+地址代碼
        /// </summary>
        ZIP_CO_DROP_FULL,

        /// <summary>
        /// 行政區+地址代碼(三碼)
        /// </summary>
        ZIP_CO_DROP_THREE,

        /// <summary>
        /// 行政區+地址代碼(三碼)+道路街+巷弄號
        /// </summary>
        ZIP_CO_DROP_THREE2,

        /// <summary>
        /// 行政區代碼
        /// </summary>
        ZIP_CO,
        
        /// <summary>
        /// 檔案上傳
        /// </summary>
        EFILE,

        /// <summary>
        /// 群組
        /// </summary>
        GRP,

        /// <summary>
        /// 檔案上傳
        /// </summary>
        FileUpload,

        /// <summary>
        /// 檔案下載
        /// </summary>
        FileDownload,

        /// <summary>
        /// 積分列表
        /// </summary>
        IntegralList,

        /// <summary>
        /// 執業縣市
        /// </summary>
        apy_city_02_nm,

        /// <summary>
        /// 執業機構代碼
        /// </summary>
        AGENCY,

        /// <summary>
        /// 
        /// </summary>
        AGENCY1,

        /// <summary>
        /// 執業機構代碼(歇業後執業)
        /// </summary>
        AGENCY2,

        /// <summary>
        /// 
        /// </summary>
        ImageHover,

        /// <summary>
        /// 
        /// </summary>
        ImageHoverList,
    }
}
