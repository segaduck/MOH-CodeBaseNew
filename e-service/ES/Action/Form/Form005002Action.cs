using ES.Utils;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace ES.Action.Form
{
    public class Form005002Action : FormBaseWordAction
    {
        public Form005002Action(SqlConnection conn)
        {
            this.conn = conn;
        }

        public Form005002Action(SqlConnection conn, SqlTransaction tran)
        {
            this.conn = conn;
            this.tran = tran;
        }

        public Dictionary<string, object> GetData(String id) 
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"
                SELECT A.SRV_ID, A.APP_ID, A.APP_TIME, A.NAME AS MF_NAME, A.ENAME AS MF_ENAME,
                    B.MF_ADDR, B.MF_ADDR_E, B.DRUG_NAME, B.DRUG_NAME_E, B.MF_CONT, B.MF_CONT_E,B.INDIOCATION, B.INDIOCATION_E,
                    B.EFFICACY, B.EFFICACY_E, B.PC_SCALE_1, B.PC_SCALE_1E, B.PC_SCALE_2E, B.PC_SCALE_21, B.PC_SCALE_22, B.PC_SCALE_23, B.PC_SCALE_24,
                    (SELECT TOP 1 CODE_DESC FROM CODE_CD WHERE CODE_CD = B.DOSAGE_FORM AND CODE_KIND = 'F5_DOSAGE_FORM') AS DOSAGE_FORM,
                    --(SELECT TOP 1 CODE_DESC FROM CODE_CD WHERE CODE_CD = B.DOSAGE_FORM_E AND CODE_KIND = 'F5_DOSAGE_FORM_E') AS DOSAGE_FORM_E,
                    DOSAGE_FORM_E,
                    (SELECT TOP 1 CODE_DESC FROM CODE_CD WHERE CODE_CD = B.LIC_CD AND CODE_KIND = 'F5_LIC_CD') + '第' + B.LIC_NUM + '號' AS LIC_NUM,
                    --(SELECT TOP 1 CODE_DESC FROM CODE_CD WHERE CODE_CD = B.LIC_CD_E AND CODE_KIND = 'F5_LIC_CD_E') + '-' + B.LIC_NUM_E AS LIC_NUM_E,
                    isnull('',(SELECT TOP 1 CODE_DESC FROM CODE_CD WHERE CODE_CD = B.LIC_CD_E AND CODE_KIND = 'F5_LIC_CD_E') + '-') + B.LIC_NUM_E AS LIC_NUM_E,
                    CONVERT(VARCHAR, (CONVERT(INT, CONVERT(VARCHAR, ISSUE_DATE, 112)) - 19110000)) AS ISSUE_DATE,
                    CONVERT(VARCHAR, ISSUE_DATE, 110) AS ISSUE_DATE_E,
                    CONVERT(VARCHAR, (CONVERT(INT, CONVERT(VARCHAR, EXPIR_DATE, 112)) - 19110000)) AS EXPIR_DATE,
                    CONVERT(VARCHAR, EXPIR_DATE, 110) AS EXPIR_DATE_E,DRUG_ABROAD_NAME,DRUG_ABROAD_NAME_E,MF_REMARK
                FROM APPLY A, APPLY_005002 B
                WHERE A.APP_ID = B.APP_ID
                    AND A.APP_ID = @APP_ID
            ";

            args.Add("APP_ID", id);

            return GetData(querySQL, args);
        }

        public List<Dictionary<string, object>> GetList1(string id)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"SELECT * FROM APPLY_005002_PC WHERE APP_ID = @APP_ID";

            args.Add("APP_ID", id);

            return GetList(querySQL, args);
        }

        public List<Dictionary<string, object>> GetList2(string id)
        {
            Dictionary<string, object> args = new Dictionary<string, object>();

            string querySQL = @"SELECT * FROM APPLY_005002_DI WHERE APP_ID = @APP_ID";

            args.Add("APP_ID", id);

            return GetList(querySQL, args);
        }

        public byte[] GetApplyWord(string id)
        {
            Dictionary<string, object> data = GetData(id);
            List<Dictionary<string, object>> list1 = GetList1(id);
            List<Dictionary<string, object>> list2 = GetList2(id);

            string date = data["ISSUE_DATE"].ToString();
            data["ISSUE_DATE"] = "民國" + date.Substring(0, date.Length - 4) + "年" + date.Substring(date.Length - 4, 2) + "月" + date.Substring(date.Length - 2) + "日";

            //data["EXPIR_DATE"] = "民國" + date2.Substring(0, date2.Length - 4) + "年" + date2.Substring(date2.Length - 4, 2) + "月" + date2.Substring(date2.Length - 2) + "日";
            date = data["EXPIR_DATE"].ToString();
            data["Expiration_Date_ColName"] = "";
            if (!String.IsNullOrEmpty(data["EXPIR_DATE"].ToString()))
            {
                data["Expiration_Date_ColName"] = "有效日期：";
                data["EXPIR_DATE"] = "民國" + date.Substring(0, date.Length - 4) + "年" + date.Substring(date.Length - 4, 2) + "月" + date.Substring(date.Length - 2) + "日";
            }

            data["Expiration_E_ColName"] = "";
            if (!String.IsNullOrEmpty(data["EXPIR_DATE_E"].ToString()))
                data["Expiration_E_ColName"] = "Expiration Date：";

            //外銷用品 中/英文
            string FileNameTag = "";
            data["DRUG_ABROAD_DATA"] = "";
            if (!String.IsNullOrEmpty(data["DRUG_ABROAD_NAME"].ToString()) && String.IsNullOrEmpty(data["DRUG_ABROAD_NAME_E"].ToString()))
            {
                data["DRUG_ABROAD_DATA"] = "外銷藥品名稱：" + data["DRUG_ABROAD_NAME"].ToString();
                FileNameTag = "_C";
            }

            if (String.IsNullOrEmpty(data["DRUG_ABROAD_NAME"].ToString()) && !String.IsNullOrEmpty(data["DRUG_ABROAD_NAME_E"].ToString()))
            {
                data["DRUG_ABROAD_DATA"] = "Product Name For Export：" + data["DRUG_ABROAD_NAME_E"].ToString();
                FileNameTag = "_C";
            }

            if (!String.IsNullOrEmpty(data["DRUG_ABROAD_NAME"].ToString()) && !String.IsNullOrEmpty(data["DRUG_ABROAD_NAME_E"].ToString()))
                FileNameTag = "_A";

            data.Add("CONT", GetCont(data, list1, list2));
            
            data.Add("MARK", AddRow(data, "MF_REMARK"));

            //data.Add("DRUG_ABROAD", AddRow(data, "DRUG_ABROAD_NAME"));

            string path = DataUtils.GetConfig("FOLDER_TEMPLATE") + data["SRV_ID"];

            string templateFile = path + "/外銷證明空白表格_0000912000"+ FileNameTag +".docx";
            string tempFile = path + "/" + new DateTime().ToString("yyyyMMddHHmmssfff") + ".docx";

            return MarkDocx(tempFile, templateFile, data);
        }

        private string GetCont(Dictionary<string, object> data, List<Dictionary<string, object>> list1, List<Dictionary<string, object>> list2)
        {
            StringBuilder sb = new StringBuilder();

            string font = "<w:pPr><w:spacing w:line=\"240\" w:lineRule=\"exact\"/><w:rPr><w:sz w:val=\"16\"/><w:szCs w:val=\"16\"/></w:rPr></w:pPr>";
            string fontTbL = "<w:tc><w:tcPr><w:tcW w:w=\"0\" w:type=\"auto\"/></w:tcPr><w:p><w:pPr><w:spacing w:line=\"240\" w:lineRule=\"exact\"/></w:pPr><w:r><w:rPr><w:sz w:val=\"16\"/><w:szCs w:val=\"16\"/><w:rFonts w:ascii=\"Times New Roman\" w:eastAsia=\"新細明體\" /></w:rPr><w:t>";
            string fontTbR = "<w:tc><w:tcPr><w:tcW w:w=\"0\" w:type=\"auto\"/></w:tcPr><w:p><w:pPr><w:spacing w:line=\"240\" w:lineRule=\"exact\"/><w:jc w:val=\"right\"/></w:pPr><w:r><w:rPr><w:sz w:val=\"16\"/><w:szCs w:val=\"16\"/><w:rFonts w:ascii=\"Times New Roman\" w:eastAsia=\"新細明體\" /></w:rPr><w:t>";
            string fontTb14 = "<w:tc><w:tcPr><w:tcW w:w=\"0\" w:type=\"auto\"/></w:tcPr><w:p><w:pPr><w:spacing w:line=\"240\" w:lineRule=\"exact\"/></w:pPr><w:r><w:rPr><w:sz w:val=\"14\"/><w:szCs w:val=\"14\"/><w:rFonts w:ascii=\"Times New Roman\" w:eastAsia=\"新細明體\" /></w:rPr><w:t>";
            string fontTbCName = "<w:tc><w:tcPr><w:tcW w:w=\"17%\" w:type=\"pct\"/></w:tcPr><w:p><w:pPr><w:spacing w:line=\"240\" w:lineRule=\"exact\"/></w:pPr><w:r><w:rPr><w:sz w:val=\"14\"/><w:szCs w:val=\"14\"/><w:rFonts w:ascii=\"Times New Roman\" w:eastAsia=\"新細明體\" /></w:rPr><w:t>";
            string fontTbEName = "<w:tc><w:tcPr><w:tcW w:w=\"22%\" w:type=\"pct\"/></w:tcPr><w:p><w:pPr><w:spacing w:line=\"240\" w:lineRule=\"exact\"/></w:pPr><w:r><w:rPr><w:sz w:val=\"14\"/><w:szCs w:val=\"14\"/><w:rFonts w:ascii=\"Times New Roman\" w:eastAsia=\"新細明體\" /></w:rPr><w:t>";
            string fontTbCount = "<w:tc><w:tcPr><w:tcW w:w=\"5%\" w:type=\"pct\"/></w:tcPr><w:p><w:pPr><w:spacing w:line=\"240\" w:lineRule=\"exact\"/><w:jc w:val=\"right\"/></w:pPr><w:r><w:rPr><w:sz w:val=\"14\"/><w:szCs w:val=\"14\"/><w:rFonts w:ascii=\"Times New Roman\" w:eastAsia=\"新細明體\" /></w:rPr><w:t>";
            string fontTbUnit = "<w:tc><w:tcPr><w:tcW w:w=\"4%\" w:type=\"pct\"/></w:tcPr><w:p><w:pPr><w:spacing w:line=\"240\" w:lineRule=\"exact\"/></w:pPr><w:r><w:rPr><w:sz w:val=\"14\"/><w:szCs w:val=\"14\"/><w:rFonts w:ascii=\"Times New Roman\" w:eastAsia=\"新細明體\" /></w:rPr><w:t>";
            string fontTbSpace = "<w:tc><w:tcPr><w:tcW w:w=\"4%\" w:type=\"pct\"/></w:tcPr><w:p><w:pPr><w:spacing w:line=\"240\" w:lineRule=\"exact\"/></w:pPr><w:r><w:rPr><w:sz w:val=\"14\"/><w:szCs w:val=\"14\"/><w:rFonts w:ascii=\"Times New Roman\" w:eastAsia=\"新細明體\" /></w:rPr><w:t>";

            string fontTbE = "</w:t></w:r></w:p></w:tc>";

            sb.Append("<w:p>").Append(font).Append("<w:t>處　方：").Append(data["MF_CONT"]).Append("：</w:t></w:p>");
            sb.Append("<w:p>").Append(font).Append("<w:t>Formula：").Append(data["MF_CONT_E"]).Append("：</w:t></w:p>");
            
            sb.Append("<w:tbl>");
            sb.Append("<w:tblPr><w:tblW w:type=\"pct\" w:w=\"100%\"/><w:tblStyle w:val=\"TableGrid\" /></w:tblPr>");
            sb.Append("<w:tblGrid><w:gridCol /><w:gridCol /><w:gridCol /><w:gridCol /></w:tblGrid>");

            int n = 2;
            int row = Convert.ToInt16(Math.Ceiling(double.Parse(list1.Count().ToString()) / 2));
            int col = 0;
            //if (list1.Count() + list2.Count() > 10) n = 2;
            //if (list1.Count() + list2.Count() > 20) n = 3;
            //主成分
            for (int i = 0; i < row; i++)
            {
                if ((i * 2) + 1 > list1.Count)
                    break;
                sb.Append("<w:tr>");
                for (int j = 0; j < n; j++)
                {
                    col = i == 0 && j == 0 ? 0 : j == 0 ? i : i + row;
                    if (col >= list1.Count)
                        break;

                    sb.Append(fontTbCName).Append(list1[col]["PC_NAME"]).Append(fontTbE);
                    sb.Append(fontTbEName).Append(list1[col]["PC_ENAME"]).Append(fontTbE);
                    sb.Append(fontTbCount).Append(list1[col]["PC_CONT"]).Append(fontTbE);
                    sb.Append(fontTbUnit).Append(list1[col]["PC_UNIT"]).Append(fontTbE);
                    

                    if (j == 0)
                        sb.Append(fontTbSpace).Append("").Append(fontTbE);
                }
                if (list1.Count == 1 && i == 0)//只有1筆資料時，右邊要塞空，表格才不會跑掉
                {
                    sb.Append(fontTbCName).Append("").Append(fontTbE);
                    sb.Append(fontTbEName).Append("").Append(fontTbE);
                    sb.Append(fontTbCount).Append("").Append(fontTbE);
                    sb.Append(fontTbUnit).Append("").Append(fontTbE);
                }
                sb.Append("</w:tr>");
            }
            //浸藥膏--------------
            if (!String.IsNullOrEmpty(data["PC_SCALE_1"].ToString()))
            {
                sb.Append("<w:tr>").Append(fontTb14).Append("<w:gridSpan  w:val='9'/>").Append("以上生藥製成進膏" + data["PC_SCALE_1"].ToString());
                if (!String.IsNullOrEmpty(data["PC_SCALE_21"].ToString()) || !String.IsNullOrEmpty(data["PC_SCALE_22"].ToString()) || !String.IsNullOrEmpty(data["PC_SCALE_23"].ToString()) || !String.IsNullOrEmpty(data["PC_SCALE_24"].ToString()))
                    sb.Append("(生藥與浸膏比例" + data["PC_SCALE_21"].ToString() + ":" + data["PC_SCALE_22"].ToString() + "=" + data["PC_SCALE_23"].ToString() + ":" + data["PC_SCALE_24"].ToString() + ")");
                sb.Append(fontTbE).Append("</w:tr>");
            }

            if (!String.IsNullOrEmpty(data["PC_SCALE_1E"].ToString()))
                sb.Append("<w:tr>").Append(fontTb14).Append("<w:gridSpan  w:val='9'/>").Append(data["PC_SCALE_1E"].ToString()).Append(data["PC_SCALE_2E"].ToString()).Append(fontTbE).Append("</w:tr>");
            //---------------------                


            //副成份
            row = Convert.ToInt16(Math.Ceiling(double.Parse(list1.Count().ToString()) / 2));
            col = 0;
            for (int i = 0; i < row; i++)
            {
                if ((i * 2) + 1 > list2.Count)
                    break;
                sb.Append("<w:tr>");
                for (int j = 0; j < n; j++)
                {
                    col = i == 0 && j == 0 ? 0 : j == 0 ? i : i + row;
                    if (col >= list2.Count)
                        break;

                    sb.Append(fontTbCName).Append(list2[col]["DI_NAME"]).Append(fontTbE);
                    sb.Append(fontTbEName).Append(list2[col]["DI_ENAME"]).Append(fontTbE);
                    sb.Append(fontTbCount).Append(list2[i]["DI_CONT"]).Append(fontTbE);
                    sb.Append(fontTbUnit).Append(list2[i]["DI_UNIT"]).Append(fontTbE);

                    if (j == 0)
                        sb.Append(fontTbSpace).Append("").Append(fontTbE);
                }
                if (list2.Count == 1 && i == 0)//只有1筆資料時，右邊要塞空，表格才不會跑掉
                {
                    sb.Append(fontTbCName).Append("").Append(fontTbE);
                    sb.Append(fontTbEName).Append("").Append(fontTbE);
                    sb.Append(fontTbCount).Append("").Append(fontTbE);
                    sb.Append(fontTbUnit).Append("").Append(fontTbE);
                }
                sb.Append("</w:tr>");
            }

            sb.Append("</w:tbl>");

            return sb.ToString();
            //int n = 2;
            ////if (list1.Count() + list2.Count() > 10) n = 2;
            ////if (list1.Count() + list2.Count() > 20) n = 3;

            //for (int i = 0; i < list1.Count(); i++)
            //{
            //    if (i == 0 || (i % n == 0))
            //    {
            //        sb.Append("<w:tr>");
            //    }

            //    sb.Append(fontTbL).Append(list1[i]["PC_NAME"]).Append(fontTbE);
            //    sb.Append(fontTbL).Append(list1[i]["PC_ENAME"]).Append(fontTbE);
            //    sb.Append(fontTbR).Append(list1[i]["PC_CONT"]).Append(" ").Append(list1[i]["PC_UNIT"]).Append(fontTbE);

            //    if (i % n == n - 1)
            //    {
            //        sb.Append("</w:tr>");
            //    }
            //}

            //for (int i = 0; i < list2.Count(); i++)
            //{
            //    if ((i + list1.Count()) % n == 0)
            //    {
            //        sb.Append("<w:tr>");
            //    }

            //    sb.Append(fontTbL).Append(list2[i]["DI_NAME"]).Append(fontTbE);
            //    sb.Append(fontTbL).Append(list2[i]["DI_ENAME"]).Append(fontTbE);
            //    sb.Append(fontTbR).Append(list2[i]["DI_CONT"]).Append(" ").Append(list2[i]["DI_UNIT"]).Append(fontTbE);

            //    if (i == list2.Count() - 1 || (i + list1.Count()) % n == n - 1)
            //    {
            //        sb.Append("</w:tr>");
            //    }
            //}

            //sb.Append("</w:tbl>");

            //return sb.ToString();
        }


        private string AddRow(Dictionary<string, object> data, string key)
        {
            StringBuilder sb = new StringBuilder();
            if (!String.IsNullOrEmpty(key))
            {
                if (key == "MF_REMARK")
                {
                    string fontTbL = "<w:tc><w:tcPr><w:tcW w:w=\"0\" w:type=\"auto\"/></w:tcPr><w:p><w:pPr><w:spacing w:line=\"240\" w:lineRule=\"exact\"/></w:pPr><w:r><w:rPr><w:sz w:val=\"16\"/><w:szCs w:val=\"16\"/><w:rFonts w:ascii=\"Times New Roman\" w:eastAsia=\"新細明體\" /></w:rPr><w:t>";
                    string fontTbE = "</w:t></w:r></w:p></w:tc>";

                    sb.Append("<w:tbl>");
                    sb.Append("<w:tblPr><w:tblStyle w:val=\"TableGrid\" /></w:tblPr>");
                    sb.Append("<w:tblGrid><w:gridCol /><w:gridCol /><w:gridCol /><w:gridCol /></w:tblGrid>");

                    //備註
                    if (!String.IsNullOrEmpty(data[key].ToString()))
                        sb.Append("<w:tr>").Append(fontTbL).Append(data[key].ToString()).Append(fontTbE).Append("</w:tr>");

                    sb.Append("</w:tbl>");
                }
                else
                {
                    if (!String.IsNullOrEmpty(data["DRUG_ABROAD_NAME"].ToString()))
                    {
                        sb.Append("<w:br/><w:p w:rsidR='00566178' w:rsidRDefault='00566178' w:rsidP='00566178'><w:pPr><w:adjustRightInd w:val='0'/><w:snapToGrid w:val='0'/><w:ind w:rightChars='2' w:right='5'/><w:rPr><w:sz w:val='16'/></w:rPr></w:pPr>");
                        sb.Append("<w:r><w:rPr><w:rFonts w:hint='eastAsia'/><w:sz w:val='16'/></w:rPr><w:t>外銷藥品名稱：</w:t></w:r><w:r w:rsidR='00950613'><w:rPr><w:rFonts w:hint='eastAsia'/><w:sz w:val='16'/></w:rPr><w:t xml:space='preserve'>         </w:t></w:r>");
                        sb.Append("<w:r w:rsidR='00950613' w:rsidRPr='00566178'><w:rPr><w:sz w:val='16'/><w:szCs w:val='16'/></w:rPr><w:t>" + data["DRUG_ABROAD_NAME"].ToString() + "</w:t></w:r>");
                        sb.Append("</w:p>");
                    }

                    if (!String.IsNullOrEmpty(data["DRUG_ABROAD_NAME_E"].ToString()))
                    {
                        sb.Append("<w:p w:rsidR='00566178' w:rsidRDefault='00566178' w:rsidP='00566178'><w:pPr><w:adjustRightInd w:val='0'/><w:snapToGrid w:val='0'/><w:ind w:rightChars='2' w:right='5'/><w:rPr><w:sz w:val='16'/></w:rPr></w:pPr>");
                        sb.Append("<w:r><w:rPr><w:rFonts w:hint='eastAsia'/><w:sz w:val='16'/></w:rPr><w:t>Product Name For Export：</w:t></w:r><w:r w:rsidR='00950613'><w:rPr><w:rFonts w:hint='eastAsia'/><w:sz w:val='16'/></w:rPr><w:t xml:space='preserve'></w:t></w:r>");
                        sb.Append("<w:r w:rsidR='00950613' w:rsidRPr='00566178'><w:rPr><w:sz w:val='16'/><w:szCs w:val='16'/></w:rPr><w:t>" + data["DRUG_ABROAD_NAME_E"].ToString() + "</w:t></w:r>");
                        sb.Append("</w:p>");
                    }
                }
            }

            return sb.ToString();
        }
    }
}