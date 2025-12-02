using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ES.Models
{
    public class SAMPLE_BASModel
    {
        public class Req
        {
            public string SAMPLE_RDOCUT_NO { get; set; }
        }

        public class Res
        {
            public string E_AppID { get; set; }
            public string SAMPLE_RDOCUT_NO { get; set; }
            public string SAMPLE_RDATE { get; set; }
            public string ORIGIN_CN_CODE { get; set; }
            public string SHIP_PORT_CODE { get; set; }
            public string SELL_CN_CODE { get; set; }
            public string APPLY_IDN_BAN { get; set; }
            public string APPLY_IDN_NAME { get; set; }
            public string APPLY_IDN_PHONE { get; set; }
            public string APPLY_IDN_ADD { get; set; }
            public string CaseType { get; set; }
            // 錯誤代碼列表
            /*
             錯誤代碼	說明
0000	成功
0001	資料庫錯誤
0002	程式錯誤
0003	網路回傳錯誤
0005	系統別錯誤
0006	日期格式錯誤
0007	時數格式錯誤
0008	資料庫連線錯誤
0009	資料重複錯誤
0010	長度錯誤
0011	身分證格式錯誤
0012	性別格式錯誤
0013	未填入數值
                 */
            public string Description { get; set; }
        }


    }
}