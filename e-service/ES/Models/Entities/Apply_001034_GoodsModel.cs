using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace ES.Models.Entities
{
    /// <summary>
    /// APPLY_001034_GOODS 危險性醫療儀器進口申請作業 貨品資料
    /// </summary>
    public class Apply_001034_GoodsModel
    {
        /// <summary>
        /// 案件編號
        /// </summary>
        [Display(Name = "案件編號")]
        public string APP_ID { get; set; }
        /// <summary>
        /// 項目序號
        /// </summary>
        [Display(Name = "項目序號")]
        public int? SRL_NO { get; set; }
        /// <summary>
        /// 貨品類別代碼
        /// </summary>
        [Display(Name = "貨品類別（規格）")]
        public string GOODS_TYPE_ID { get; set; }
        /// <summary>
        /// 貨品類別名稱
        /// </summary>
        [Display(Name = "貨品類別名稱")]
        public string GOODS_TYPE { get; set; }
        /// <summary>
        /// 貨品代碼
        /// </summary>
        [Display(Name = "貨品代碼")]
        public string GOODS_ID { get; set; }
        /// <summary>
        /// 貨品名稱
        /// </summary>
        [Display(Name = "貨品名稱")]
        public string GOODS_NAME { get; set; }
        /// <summary>
        /// 申請數量
        /// </summary>
        [Display(Name = "申請數量")]
        public int? APPLY_CNT { get; set; }
        /// <summary>
        /// 數量單位代號
        /// </summary>
        [Display(Name = "貨品單位")]
        public string GOODS_UNIT_ID { get; set; }
        /// <summary>
        /// 數量單位名稱
        /// </summary>
        [Display(Name = "數量單位名稱")]
        public string GOODS_UNIT { get; set; }
        /// <summary>
        /// 貨品型號
        /// </summary>
        [Display(Name = "型號")]
        public string GOODS_MODEL { get; set; }
        /// <summary>
        /// 貨品規格
        /// </summary>
        [Display(Name = "規格")]
        public string GOODS_SPEC_1 { get; set; }
        /// <summary>
        /// 貨品規格2
        /// </summary>
        [Display(Name = "summary")]
        public string GOODS_SPEC_2 { get; set; }
        /// <summary>
        /// 貨品牌名
        /// </summary>
        [Display(Name = "牌名")]        
        public string GOODS_BRAND { get; set; }
        /// <summary>
        /// 貨品輔助描述
        /// </summary>
        [Display(Name = "貨品輔助描述")]        
        public string GOODS_DESC { get; set; }
        /// <summary>
        /// 檢附文件類型01
        /// </summary>
        [Display(Name = "檢附文件類型一")]
        public string DOC_TYP_01 { get; set; }
        /// <summary>
        /// 檢附文件字號01
        /// </summary>
        [Display(Name = "檢附文件字號一")]
        public string DOC_COD_01 { get; set; }
        /// <summary>
        /// 檢附文件說明01
        /// </summary>
        [Display(Name = "檢附文件說明一")]
        public string DOC_TXT_01 { get; set; }
        /// <summary>
        /// 檢附文件類型02
        /// </summary>
        [Display(Name = "檢附文件類型二")]
        public string DOC_TYP_02 { get; set; }
        /// <summary>
        /// 檢附文件字號02
        /// </summary>
        [Display(Name = "檢附文件字號二")]
        public string DOC_COD_02 { get; set; }
        /// <summary>
        /// 檢附文件說明02
        /// </summary>
        [Display(Name = "檢附文件說明二")]
        public string DOC_TXT_02 { get; set; }
        /// <summary>
        /// 檢附文件類型03
        /// </summary>
        [Display(Name = "檢附文件類型三")]
        public string DOC_TYP_03 { get; set; }
        /// <summary>
        /// 檢附文件字號03
        /// </summary>
        [Display(Name = "檢附文件字號三")]
        public string DOC_COD_03 { get; set; }
        /// <summary>
        /// 檢附文件說明03
        /// </summary>
        [Display(Name = "檢附文件說明三")]
        public string DOC_TXT_03 { get; set; }
        /// <summary>
        /// 檢附文件類型04
        /// </summary>
        [Display(Name = "檢附文件類型四")]
        public string DOC_TYP_04 { get; set; }
        /// <summary>
        /// 檢附文件字號04
        /// </summary>
        [Display(Name = "檢附文件字號四")]
        public string DOC_COD_04 { get; set; }
        /// <summary>
        /// 檢附文件說明04
        /// </summary>
        [Display(Name = "檢附文件說明四")]
        public string DOC_TXT_04 { get; set; }
        /// <summary>
        /// 檢附文件類型05
        /// </summary>
        [Display(Name = "檢附文件類型五")]
        public string DOC_TYP_05 { get; set; }
        /// <summary>
        /// 檢附文件字號05
        /// </summary>
        [Display(Name = "檢附文件字號五")]
        public string DOC_COD_05 { get; set; }
        /// <summary>
        /// 檢附文件說明05
        /// </summary>
        [Display(Name = "檢附文件說明五")]
        public string DOC_TXT_05 { get; set; }
        /// <summary>
        /// 檢附文件類型06
        /// </summary>
        [Display(Name = "檢附文件類型六")]
        public string DOC_TYP_06 { get; set; }
        /// <summary>
        /// 檢附文件字號06
        /// </summary>
        [Display(Name = "檢附文件字號六")]
        public string DOC_COD_06 { get; set; }
        /// <summary>
        /// 檢附文件說明06
        /// </summary>
        [Display(Name = "檢附文件說明六")]
        public string DOC_TXT_06 { get; set; }
        /// <summary>
        /// 檢附文件類型07
        /// </summary>
        [Display(Name = "檢附文件類型七")]
        public string DOC_TYP_07 { get; set; }
        /// <summary>
        /// 檢附文件字號07
        /// </summary>
        [Display(Name = "檢附文件字號七")]
        public string DOC_COD_07 { get; set; }
        /// <summary>
        /// 檢附文件說明07
        /// </summary>
        [Display(Name = "檢附文件說明七")]
        public string DOC_TXT_07 { get; set; }
        /// <summary>
        /// 檢附文件類型08
        /// </summary>
        [Display(Name = "檢附文件類型八")]
        public string DOC_TYP_08 { get; set; }
        /// <summary>
        /// 檢附文件字號08
        /// </summary>
        [Display(Name = "檢附文件字號八")]
        public string DOC_COD_08 { get; set; }
        /// <summary>
        /// 檢附文件說明08
        /// </summary>
        [Display(Name = "檢附文件說明八")]
        public string DOC_TXT_08 { get; set; }
        /// <summary>
        /// 檢附文件類型09
        /// </summary>
        [Display(Name = "檢附文件類型九")]
        public string DOC_TYP_09 { get; set; }
        /// <summary>
        /// 檢附文件字號09
        /// </summary>
        [Display(Name = "檢附文件字號九")]
        public string DOC_COD_09 { get; set; }
        /// <summary>
        /// 檢附文件說明09
        /// </summary>
        [Display(Name = "檢附文件說明九")]
        public string DOC_TXT_09 { get; set; }
        /// <summary>
        /// 檢附文件類型10
        /// </summary>
        [Display(Name = "檢附文件類型十")]
        public string DOC_TYP_10 { get; set; }
        /// <summary>
        /// 檢附文件字號10
        /// </summary>
        [Display(Name = "檢附文件字號十")]
        public string DOC_COD_10 { get; set; }
        /// <summary>
        /// 檢附文件說明10
        /// </summary>
        [Display(Name = "檢附文件說明十")]
        public string DOC_TXT_10 { get; set; }
        /// <summary>
        /// 檢附文件類型11
        /// </summary>
        [Display(Name = "檢附文件類型十一")]
        public string DOC_TYP_11 { get; set; }
        /// <summary>
        /// 檢附文件字號11
        /// </summary>
        [Display(Name = "檢附文件字號十一")]
        public string DOC_COD_11 { get; set; }
        /// <summary>
        /// 檢附文件說明11
        /// </summary>
        [Display(Name = "檢附文件說明十一")]
        public string DOC_TXT_11 { get; set; }
        /// <summary>
        /// 檢附文件類型12
        /// </summary>
        [Display(Name = "檢附文件類型十二")]
        public string DOC_TYP_12 { get; set; }
        /// <summary>
        /// 檢附文件字號12
        /// </summary>
        [Display(Name = "檢附文件字號十二")]
        public string DOC_COD_12 { get; set; }
        /// <summary>
        /// 檢附文件說明12
        /// </summary>
        [Display(Name = "檢附文件說明十二")]
        public string DOC_TXT_12 { get; set; }
        /// <summary>
        /// 檢附文件類型13
        /// </summary>
        [Display(Name = "檢附文件類型十三")]
        public string DOC_TYP_13 { get; set; }
        /// <summary>
        /// 檢附文件字號13
        /// </summary>
        [Display(Name = "檢附文件字號十三")]
        public string DOC_COD_13 { get; set; }
        /// <summary>
        /// 檢附文件說明13
        /// </summary>
        [Display(Name = "檢附文件說明十三")]
        public string DOC_TXT_13 { get; set; }
        /// <summary>
        /// 檢附文件類型14
        /// </summary>
        [Display(Name = "檢附文件類型十四")]
        public string DOC_TYP_14 { get; set; }
        /// <summary>
        /// 檢附文件字號14
        /// </summary>
        [Display(Name = "檢附文件字號十四")]
        public string DOC_COD_14 { get; set; }
        /// <summary>
        /// 檢附文件說明14
        /// </summary>
        [Display(Name = "檢附文件說明十四")]
        public string DOC_TXT_14 { get; set; }
        /// <summary>
        /// 檢附文件類型15
        /// </summary>
        [Display(Name = "檢附文件類型十五")]
        public string DOC_TYP_15 { get; set; }
        /// <summary>
        /// 檢附文件字號15
        /// </summary>
        [Display(Name = "檢附文件字號十五")]
        public string DOC_COD_15 { get; set; }
        /// <summary>
        /// 檢附文件說明15
        /// </summary>
        [Display(Name = "檢附文件說明十五")]
        public string DOC_TXT_15 { get; set; }
        /// <summary>
        /// 檢附文件類型016
        /// </summary>
        [Display(Name = "檢附文件類型十六")]
        public string DOC_TYP_16 { get; set; }
        /// <summary>
        /// 檢附文件字號16
        /// </summary>
        [Display(Name = "檢附文件字號十六")]
        public string DOC_COD_16 { get; set; }
        /// <summary>
        /// 檢附文件說明16
        /// </summary>
        [Display(Name = "檢附文件說明十六")]
        public string DOC_TXT_16 { get; set; }
        /// <summary>
        /// 檢附文件類型17
        /// </summary>
        [Display(Name = "檢附文件類型十七")]
        public string DOC_TYP_17 { get; set; }
        /// <summary>
        /// 檢附文件字號17
        /// </summary>
        [Display(Name = "檢附文件字號十七")]
        public string DOC_COD_17 { get; set; }
        /// <summary>
        /// 檢附文件說明17
        /// </summary>
        [Display(Name = "檢附文件說明十七")]
        public string DOC_TXT_17 { get; set; }
        /// <summary>
        /// 檢附文件類型18
        /// </summary>
        [Display(Name = "檢附文件類型十八")]
        public string DOC_TYP_18 { get; set; }
        /// <summary>
        /// 檢附文件字號18
        /// </summary>
        [Display(Name = "檢附文件字號十八")]
        public string DOC_COD_18 { get; set; }
        /// <summary>
        /// 檢附文件說明18
        /// </summary>
        [Display(Name = "檢附文件說明十八")]
        public string DOC_TXT_18 { get; set; }
        /// <summary>
        /// 檢附文件類型19
        /// </summary>
        [Display(Name = "檢附文件類型十九")]
        public string DOC_TYP_19 { get; set; }
        /// <summary>
        /// 檢附文件字號19
        /// </summary>
        [Display(Name = "檢附文件字號十九")]        
        public string DOC_COD_19 { get; set; }
        /// <summary>
        /// 檢附文件說明19
        /// </summary>
        [Display(Name = "檢附文件說明十九")]
        public string DOC_TXT_19 { get; set; }
        /// <summary>
        /// 
        /// </summary>
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