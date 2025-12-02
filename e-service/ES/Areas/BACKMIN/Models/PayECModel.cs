using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ES.Areas.Admin.Models
{
    public class PayECModel
    {
        public PayECModel()
        {
            this.queryModel = new MaintainECModel();
        }
        public MaintainECModel queryModel { get; set; }
        public string submit_type { get; set; }
    }

    public class MaintainECModel
    {
        public string viewAPP_ID { get; set; }
        public virtual int NowPage { get; set; }

        [Display(Name = "申辦編號")]
        public virtual string APP_ID_BEGIN { get; set; }

        [Display(Name = "申辦編號")]
        public virtual string APP_ID_END { get; set; }

        [Display(Name = "申辦日期")]
        public virtual DateTime? APP_TIME_BEGIN { get; set; }

        [Display(Name = "申辦日期")]
        public virtual DateTime? APP_TIME_END { get; set; }

        [Display(Name = "案件類別")]
        public virtual string SC_PID { get; set; }

        [Display(Name = "案件類別")]
        public virtual string SRV_ID { get; set; }

        [Display(Name ="請款狀態")]
        public virtual string RSP_STATUS { get; set; }
    }

    public class RspModel
    {
        public string A_001_010_10 { get; set; } //特店代號
        public string B_011_018_8 { get; set; } //端末機代號
        public string C_019_058_40 { get; set; } //訂單編號
        public string D_059_077_19 { get; set; } //空白
        public string E_078_085_8 { get; set; } //交易金額
        public string F_086_093_8 { get; set; } //授權碼
        public string G_094_095_2 { get; set; } //交易碼 01退貨 02請款
        public string H_096_103_8 { get; set; } //交易日期YYYYMMDD
        public string I_104_119_16 { get; set; } //使用者自訂欄位
        public string J_120_159_40 { get; set; } //卡人資訊
        public string K_160_165_6 { get; set; } //帳單處理日期YYMMDD(西曆)
        public string L_166_168_3 { get; set; } //回應碼 00為請款成功
        public string M_169_184_16 { get; set; } //回應訊息
        public string N_185_190_6 { get; set; } //Batch and seq. No.
        public string O_191_191_1 { get; set; } //分期手續費型態或紅利扣抵型態
        public string P_192_193_2 { get; set; } //分期數
        public string Q_194_201_8 { get; set; } //首期金額
        public string R_202_209_8 { get; set; } //每期金額
        public string S_210_215_6 { get; set; } //分期交易手續費
        public string T_216_223_8 { get; set; } //本次扣抵點數
        public string U_224_224_1 { get; set; } //餘額正負號
        public string V_225_232_8 { get; set; } //卡人點數餘額
        public string W_233_242_10 { get; set; } //卡人自付金額
        public string X_243_250_8 { get; set; } //付款日
        public string Y_251_251_1 { get; set; } //3D認證結果
        public string Z_252_252_1 { get; set; } //是否為國外卡
        public string ZA_253_253_1 { get; set; } //系統處理檔案
        public string ZB_254_270_17 { get; set; } //預留

    }
}