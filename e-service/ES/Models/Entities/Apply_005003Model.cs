using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using ES.Commons;

namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class Apply_005003Model
    {
        /// <summary>
        /// 案件號碼
        /// </summary>
        [Display(Name = "案件號碼")]
        public string APP_ID { get; set; }

        /// <summary>
        /// EMAIL
        /// </summary>
        [Display(Name = "EMAIL")]
        //[Required]
        public string EMAIL { get; set; }

        /// <summary>
        /// *申辦份數
        /// </summary>
        [Display(Name = "申辦份數")]
        public int? COPIES { get; set; }
        /// <summary>
        /// ??地址-外銷國家-(暫用)
        /// </summary>
        [Display(Name = "外銷國家")]
        public string MF_ADDR { get; set; }

        /// <summary>
        /// *2A.1藥品許可證字號-字
        /// </summary>
        [Display(Name = "*2A.1藥品許可證字號")]
        public string F_2A_1_WORD { get; set; }
        /// <summary>
        /// *2A.1藥品許可證字號-號
        /// </summary>
        [Display(Name = "*2A.1藥品許可證字號")]
        public string F_2A_1_NUM { get; set; }
        /// <summary>
        /// 1.2本產品是否獲准在國內販售？
        /// </summary>
        [Display(Name = "1.2本產品是否獲准在國內販售？")]
        public string F_1_2 { get; set; }
        /// <summary>
        /// 1.藥品名稱
        /// </summary>
        [Display(Name = "1.藥品名稱")]
        public string F_1 { get; set; }
        /// <summary>
        /// *劑型
        /// </summary>
        [Display(Name = "劑型")]
        public string F_1_DF { get; set; }

        /// <summary>
        /// *1.1處方說明
        /// </summary>
        [Display(Name = "1.1處方說明")]
        public string F_1_1 { get; set; }
        /// <summary>
        /// 1.3本產品是否有在國內販售？
        /// </summary>
        [Display(Name = "1.3本產品是否有在國內販售？")]
        public string F_1_3 { get; set; }

        /// <summary>
        /// *2A.1核准日期
        /// </summary>
        [Display(Name = "2A.1核准日期")]
        public DateTime? F_2A_1_DATE { get; set; }

        /// <summary>
        /// 2A.2/2A.3.1/2B.2.1製造廠名稱
        /// </summary>
        [Display(Name = "2A.2/2A.3.1/2B.2.1製造廠名稱")]
        public string F_2A_3_1_NAME { get; set; }
        /// <summary>
        /// *2A.2/2A.3.1/2B.2.1製造廠地址
        /// </summary>
        [Display(Name = "2A.2/2A.3.1/2B.2.1製造廠地址")]
        public string F_2A_3_2_ADDR { get; set; }
        /// <summary>
        /// *是否為委託製造？
        /// </summary>
        [Display(Name = "是否為委託製造？")]
        public string F_2A_2_COMM { get; set; }
        /// <summary>
        /// *2A.2藥商名稱
        /// </summary>
        [Display(Name = "2A.2藥商名稱")]
        public string F_2A_2 { get; set; }
        /// <summary>
        /// *2A.2藥商地址
        /// </summary>
        [Display(Name = "2A.2藥商地址")]
        public string F_2A_2_ADDR { get; set; }

        /// <summary>
        /// *2A.3藥品許可證持有者之類別
        /// </summary>
        [Display(Name = "2A.3藥品許可證持有者之類別")]
        public string F_2A_3 { get; set; }
        /// <summary>
        /// *2A.4該藥品許可證是否有經認可之試驗佐證？
        /// </summary>
        [Display(Name = "")]
        public string F_2A_4 { get; set; }
        /// <summary>
        /// *2A.5所附產品資訊是否完整且與藥品許可證一致？
        /// </summary>
        [Display(Name = "*2A.5所附產品資訊是否完整且與藥品許可證一致？")]
        public string F_2A_5 { get; set; }

        /// <summary>
        /// *2A.6申請者是否為藥品許可證持有者？
        /// </summary>
        [Display(Name = "*2A.6申請者是否為藥品許可證持有者？")]
        public string F_2A_6 { get; set; }
        /// <summary>
        /// 2A.6/2B.1申請者之公司名稱
        /// </summary>
        [Display(Name = "2A.6/2B.1申請者之公司名稱")]
        public string F_2A_6_NAME { get; set; }
        /// <summary>
        /// 2A.6/2B.1申請者之公司地址
        /// </summary>
        [Display(Name = "2A.6/2B.1申請者之公司地址")]
        public string F_2A_6_ADDR { get; set; }

        /// <summary>
        /// *2B.2申請者之類別
        /// </summary>
        [Display(Name = "2B.2申請者之類別")]
        public string F_2B_2 { get; set; }
        /// <summary>
        /// *2B.3僅供外銷專用之原因
        /// </summary>
        [Display(Name = "2B.3僅供外銷專用之原因")]
        public string F_2B_3 { get; set; }
        /// <summary>
        /// 備註
        /// </summary>
        [Display(Name = "備註")]
        public string F_2B_3_REMARKS { get; set; }

        /// <summary>
        /// *3.製造廠是否定期接受本部之GMP查核？
        /// </summary>
        [Display(Name = "3.製造廠是否定期接受本部之GMP查核？")]
        public string F_3_0 { get; set; }
        /// <summary>
        /// *3.1接受定期查核之週期為何？
        /// </summary>
        [Display(Name = "*3.1接受定期查核之週期為何？")]
        public string F_3_1 { get; set; }
        /// <summary>
        /// 3.2申請案藥品許可證之劑型，是否經過本部查核？
        /// </summary>
        [Display(Name = "3.2申請案藥品許可證之劑型，是否經過本部查核？")]
        public string F_3_2 { get; set; }
        /// <summary>
        /// *3.3申請案藥品許可證之製造設備及製程，是否符合WHO建議之GMP規範？
        /// </summary>
        [Display(Name = "*3.3申請案藥品許可證之製造設備及製程，是否符合WHO建議之GMP規範？")]
        public string F_3_3 { get; set; }
        /// <summary>
        /// *4.申請者所提供之資訊，是否符合外銷對象對產品製造所有方面的標準？
        /// </summary>
        [Display(Name = "*4.申請者所提供之資訊，是否符合外銷對象對產品製造所有方面的標準？")]
        public string F_4 { get; set; }

        /// <summary>
        /// 附件名稱 500
        /// </summary>
        [Display(Name = "附件名稱")]
        public string ATTACH_1 { get; set; }

        /// <summary>
        /// 佐證文件採合併檔案
        /// </summary>
        [Display(Name = "佐證文件採合併檔案")]
        public string MERGEYN { get; set; } //varchar 1 允許
        /// <summary>
        /// 藥品許可證影本(正面)
        /// </summary>
        [Display(Name = "藥品許可證影本(正面)")]
        public string FILE_LICF { get; set; } //varchar 20 允許
        /// <summary>
        /// 藥品許可證影本(反面)
        /// </summary>
        [Display(Name = "藥品許可證影本(反面)")]
        public string FILE_LICB { get; set; } //varchar 20 允許
        /// <summary>
        /// 處方之中藥材中英文對照表。
        /// </summary>
        [Display(Name = "處方之中藥材中英文對照表。")]
        public string FILE_CHART { get; set; } //varchar 20 允許

        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "")]
        public string DEL_MK { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? DEL_TIME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DEL_FUN_CD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DEL_ACC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? UPD_TIME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UPD_FUN_CD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UPD_ACC { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? ADD_TIME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ADD_FUN_CD { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ADD_ACC { get; set; }

    }
}