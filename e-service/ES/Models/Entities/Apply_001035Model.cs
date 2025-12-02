using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace ES.Models.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class Apply_001035Model
    {
        /// <summary>
        /// 
        /// </summary>
        public string APP_ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TAX_ORG_ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TAX_ORG_NAME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TAX_ORG_ENAME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TAX_ORG_ADDR { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TAX_ORG_EADDR { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TAX_ORG_MAN { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TAX_ORG_TEL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TAX_ORG_EMAIL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TAX_ORG_FAX { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IM_EXPORT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? DATE_S { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? DATE_E { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DEST_STATE_ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DEST_STATE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SELL_STATE_ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SELL_STATE { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string TRN_COUNTRY_ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TRN_PORT_ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TRN_PORT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BEG_COUNTRY_ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BEG_PORT_ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BEG_PORT { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SELL_NAME { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SELL_ADDR { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string APP_USE_ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string APP_USE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string USE_MARK { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CONF_TYPE_ID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CONF_TYPE { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件類型01")]
        public string DOC_TYP_01 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件敘述01")]
        public string DOC_COD_01 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件字號01")]
        public string DOC_TXT_01 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件類型02")]
        public string DOC_TYP_02 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件敘述02")]
        public string DOC_COD_02 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件字號02")]
        public string DOC_TXT_02 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件類型03")]
        public string DOC_TYP_03 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件敘述03")]
        public string DOC_TYP_03_SEL { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DOC_COD_03 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件字號03")]
        public string DOC_TXT_03 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件類型04")]
        public string DOC_TYP_04 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件敘述04")]
        public string DOC_COD_04 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件字號04")]
        public string DOC_TXT_04 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件類型05")]
        public string DOC_TYP_05 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件敘述05")]
        public string DOC_COD_05 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件字號05")]
        public string DOC_TXT_05 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件類型06")]
        public string DOC_TYP_06 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件敘述06")]
        public string DOC_COD_06 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件字號06")]
        public string DOC_TXT_06 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件類型07")]
        public string DOC_TYP_07 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件敘述07")]
        public string DOC_COD_07 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件字號07")]
        public string DOC_TXT_07 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件類型08")]
        public string DOC_TYP_08 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件敘述08")]
        public string DOC_COD_08 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Display(Name = "檢附文件字號08")]
        public string DOC_TXT_08 { get; set; }
        /// <summary>
        /// 檢附文件類型09
        /// </summary>
        [Display(Name = "檢附文件類型09")]
        public string DOC_TYP_09 { get; set; }
        /// <summary>          
        /// 檢附文件敘述09   
        /// </summary>   
        [Display(Name = "檢附文件敘述09")]
        public string DOC_COD_09 { get; set; }
        /// <summary>          
        /// 檢附文件字號09     
        /// </summary>   
        [Display(Name = "檢附文件字號09")]
        public string DOC_TXT_09 { get; set; }
        /// <summary>
        /// 檢附文件類型10
        /// </summary>
        [Display(Name = "檢附文件類型10")]
        public string DOC_TYP_10 { get; set; }
        /// <summary>        
        /// 檢附文件敘述10   
        /// </summary>      
        [Display(Name = "檢附文件敘述10")]
        public string DOC_COD_10 { get; set; }
        /// <summary>         
        /// 檢附文件字號10    
        /// </summary>        
        [Display(Name = "檢附文件字號10")]
        public string DOC_TXT_10 { get; set; }
        /// <summary>
        /// 檢附文件類型11
        /// </summary>
        [Display(Name = "檢附文件類型11")]
        public string DOC_TYP_11 { get; set; }
        /// <summary>         
        /// 檢附文件敘述11   
        /// </summary>      
        [Display(Name = "檢附文件敘述11")]
        public string DOC_COD_11 { get; set; }
        /// <summary>         
        /// 檢附文件字號11    
        /// </summary>      
        [Display(Name = "檢附文件字號11")]
        public string DOC_TXT_11 { get; set; }
        /// <summary>
        /// 檢附文件類型12
        /// </summary>
        [Display(Name = "檢附文件類型12")]
        public string DOC_TYP_12 { get; set; }
        /// <summary>         
        /// 檢附文件敘述12    
        /// </summary>     
        [Display(Name = "檢附文件敘述12")]
        public string DOC_COD_12 { get; set; }
        /// <summary>         
        /// 檢附文件字號12    
        /// </summary>        
        [Display(Name = "檢附文件字號12")]
        public string DOC_TXT_12 { get; set; }
        /// <summary>
        /// 檢附文件類型13
        /// </summary>
        [Display(Name = "檢附文件類型13")]
        public string DOC_TYP_13 { get; set; }
        /// <summary>         
        /// 檢附文件敘述13    
        /// </summary>     
        [Display(Name = "檢附文件敘述13")]
        public string DOC_COD_13 { get; set; }
        /// <summary>         
        /// 檢附文件字號13    
        /// </summary>  
        [Display(Name = "檢附文件字號13")]
        public string DOC_TXT_13 { get; set; }
        /// <summary>
        /// 檢附文件類型14
        /// </summary>
        [Display(Name = "檢附文件類型14")]
        public string DOC_TYP_14 { get; set; }
        /// <summary>         
        /// 檢附文件敘述14    
        /// </summary>    
        [Display(Name = "檢附文件敘述14")]
        public string DOC_COD_14 { get; set; }
        /// <summary>         
        /// 檢附文件字號14    
        /// </summary> 
        [Display(Name = "檢附文件字號14")]
        public string DOC_TXT_14 { get; set; }
        /// <summary>
        /// 檢附文件類型15
        /// </summary>
        [Display(Name = "檢附文件類型15")]
        public string DOC_TYP_15 { get; set; }
        /// <summary>         
        /// 檢附文件敘述15    
        /// </summary>     
        [Display(Name = "檢附文件敘述15")]
        public string DOC_COD_15 { get; set; }
        /// <summary>         
        /// 檢附文件字號15    
        /// </summary>       
        [Display(Name = "檢附文件字號15")]
        public string DOC_TXT_15 { get; set; }
        /// <summary>
        /// 檢附文件類型16
        /// </summary>
        [Display(Name = "檢附文件類型16")]
        public string DOC_TYP_16 { get; set; }
        /// <summary>         
        /// 檢附文件敘述16    
        /// </summary>     
        [Display(Name = "檢附文件敘述16")]
        public string DOC_COD_16 { get; set; }
        /// <summary>         
        /// 檢附文件字號16    
        /// </summary>     
        [Display(Name = "檢附文件字號16")]
        public string DOC_TXT_16 { get; set; }
        /// <summary>
        /// 檢附文件類型17
        /// </summary>
        [Display(Name = "檢附文件類型17")]
        public string DOC_TYP_17 { get; set; }
        /// <summary>         
        /// 檢附文件敘述17    
        /// </summary>  
        [Display(Name = "檢附文件敘述17")]
        public string DOC_COD_17 { get; set; }
        /// <summary>         
        /// 檢附文件字號17    
        /// </summary>   
        [Display(Name = "檢附文件字號17")]
        public string DOC_TXT_17 { get; set; }
        /// <summary>
        /// 檢附文件類型18
        /// </summary>
        [Display(Name = "檢附文件類型18")]
        public string DOC_TYP_18 { get; set; }
        /// <summary>         
        /// 檢附文件敘述18    
        /// </summary>     
        [Display(Name = "檢附文件敘述18")]
        public string DOC_COD_18 { get; set; }
        /// <summary>         
        /// 檢附文件字號18    
        /// </summary>    
        [Display(Name = "檢附文件字號18")]
        public string DOC_TXT_18 { get; set; }
        /// <summary>
        /// 檢附文件類型19
        /// </summary>
        [Display(Name = "檢附文件類型19")]
        public string DOC_TYP_19 { get; set; }
        /// <summary>         
        /// 檢附文件敘述19    
        /// </summary>        
        [Display(Name = "檢附文件敘述19")]
        public string DOC_COD_19 { get; set; }
        /// <summary>         
        /// 檢附文件字號19    
        /// </summary>   
        [Display(Name = "檢附文件字號19")]
        public string DOC_TXT_19 { get; set; }
        /// <summary>
        /// 檢附文件類型20
        /// </summary>
        [Display(Name = "檢附文件類型20")]
        public string DOC_TYP_20 { get; set; }
        /// <summary>         
        /// 檢附文件敘述20    
        /// </summary>
        [Display(Name = "檢附文件敘述20")]
        public string DOC_COD_20 { get; set; }
        /// <summary>         
        /// 檢附文件字號20    
        /// </summary> 
        [Display(Name = "檢附文件字號20")]
        public string DOC_TXT_20 { get; set; }
        /// <summary>
        /// 檢附文件類型21
        /// </summary>
        [Display(Name = "檢附文件類型21")]
        public string DOC_TYP_21 { get; set; }
        /// <summary>         
        /// 檢附文件敘述21    
        /// </summary>     
        [Display(Name = "檢附文件類型21")]
        public string DOC_COD_21 { get; set; }
        /// <summary>         
        /// 檢附文件字號21    
        /// </summary>    
        [Display(Name = "檢附文件類型21")]
        public string DOC_TXT_21 { get; set; }
        /// <summary>
        /// 檢附文件類型22
        /// </summary>
        [Display(Name = "檢附文件類型22")]
        public string DOC_TYP_22 { get; set; }
        /// <summary>         
        /// 檢附文件敘述22    
        /// </summary>     
        [Display(Name = "檢附文件敘述22")]
        public string DOC_COD_22 { get; set; }
        /// <summary>         
        /// 檢附文件字號22    
        /// </summary>   
        [Display(Name = "檢附文件字號22")]
        public string DOC_TXT_22 { get; set; }
        /// <summary>
        /// 檢附文件類型23
        /// </summary>
        [Display(Name = "檢附文件類型23")]
        public string DOC_TYP_23 { get; set; }
        /// <summary>         
        /// 檢附文件敘述23    
        /// </summary>    
        [Display(Name = "檢附文件敘述23")]
        public string DOC_COD_23 { get; set; }
        /// <summary>         
        /// 檢附文件字號23    
        /// </summary>       
        [Display(Name = "檢附文件字號23")]
        public string DOC_TXT_23 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? COPIES { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MDOD_APPNO { get; set; }
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