using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using ES.Utils;
using System.Text;
using ES.Action;
using ES.Areas.Admin.Models;

namespace ES.Areas.Admin.Action
{
    public class DocumentExportAction : BaseAction
    {
        public List<DocumentExportModel.DocumentModel> GetDocumentData()
        {
            List<DocumentExportModel.DocumentModel> li = new List<DocumentExportModel.DocumentModel>();
            SqlConnection conn = DataUtils.GetConnection();
            conn.Open();
            SqlCommand com = conn.CreateCommand(); ;
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(@"
                    select AD.APP_ID,AD.SRV_NO,UNIT_NAME,ADDRESS,NAME,TEL,FAX,MAIL,'申請'+ISNULL(DRUG_NAME,'')+ISNULL(LIC_NUM,'')+SUBJECT as SUBJECT,CAPTION1,CAPTION2+CONVERT(VARCHAR,APP_COUNT)+'份' AS CAPTION2,CAPTION3+CONVERT(VARCHAR,AMOUNT)+'元整'+CASE WHEN PAYMETHOD=1 THEN '支票乙紙' WHEN PAYMETHOD=2 THEN '匯票乙紙' ELSE '現金' END AS CAPTION3,OTHER1 as CAPTION4,OTHER2 as CAPTION5,OTHER3 as CAPTION6,OTHER4 as CAPTION7,OTHER5 as CAPTION8
from APPLY_DOCUMENT ad inner join DOCUMENT_BASE db on ad.SRV_NO = db.SRV_ID  where isnull(DOC_NO,'')='' and APP_ID is not null
                ");
                com.CommandText = sb.ToString(); ;

                using (SqlDataReader sr = com.ExecuteReader())
                {
                    while (sr.Read())
                    {

                        DocumentExportModel.DocumentModel dem = new DocumentExportModel.DocumentModel();
                        dem.APP_ID = sr["APP_ID"].ToString();
                        dem.SRV_ID = sr["SRV_NO"].ToString();
                        dem.UNIT_NAME = sr["UNIT_NAME"].ToString();
                        dem.ADDRESS = sr["ADDRESS"].ToString();
                        dem.NAME = sr["NAME"].ToString();
                        dem.TEL = sr["TEL"].ToString();
                        dem.FAX = sr["FAX"].ToString();
                        dem.MAIL = sr["MAIL"].ToString();
                        dem.SUBJECT = sr["SUBJECT"].ToString();
                        dem.CAPTION1 = sr["CAPTION1"].ToString();
                        dem.CAPTION2 = sr["CAPTION2"].ToString();
                        dem.CAPTION3 = sr["CAPTION3"].ToString();
                        dem.CAPTION4 = sr["CAPTION4"].ToString();
                        dem.CAPTION5 = sr["CAPTION5"].ToString();
                        dem.CAPTION6 = sr["CAPTION6"].ToString();
                        dem.CAPTION7 = sr["CAPTION7"].ToString();
                        dem.CAPTION8 = sr["CAPTION8"].ToString();

                        li.Add(dem);
                    }
                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                String a = ex.Message;
                logger.Warn(ex.Message, ex);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
                com.Dispose();
            }

            return li == null ? new List<DocumentExportModel.DocumentModel>() : li;
        }

        public string GetATTACH_1(string APP_ID, string SRV_ID)
        {
            var result = string.Empty;
            try
            {
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand com = conn.CreateCommand())
                    {
                        string Sql = " select ATTACH_1 FROM APPLY_" + SRV_ID + " where APP_ID=@APP_ID ";
                        DataUtils.AddParameters(com, "APP_ID", APP_ID);
                        com.CommandText = Sql;

                        result = com.ExecuteScalar().ToString();
                    }
                    conn.Close();
                    conn.Dispose();
                }
            }
            catch
            {
            }
            return result;
        }

        public int GetDocumentId()
        {
            var result = 0;
            try
            {
                using (SqlConnection conn = DataUtils.GetConnection())
                {
                    conn.Open();
                    using (SqlCommand com = conn.CreateCommand())
                    {
                        string Sql = @" select isnull(max(substring(DOC_NO,8,7)),'0')
from APPLY_DOCUMENT 
where substring(DOC_NO,5,3)= substring(convert(varchar,GETDATE(),102),1,4)-1911";

                        com.CommandText = Sql;

                        result = int.Parse(com.ExecuteScalar().ToString());
                    }
                    conn.Close();
                    conn.Dispose();
                }
            }
            catch
            {
            }
            return result;
        }

        public int UpdataDocumentId(string DocumentId, string APP_ID)
        {
            var result = 0;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand com = conn.CreateCommand())
                {
                    string Sql = @" Update APPLY_DOCUMENT Set DOC_NO = @DOC_NO WHERE APP_ID=@APP_ID ";
                    DataUtils.AddParameters(com, "DOC_NO", DocumentId);
                    DataUtils.AddParameters(com, "APP_ID", APP_ID);

                    com.CommandText = Sql;

                    result = com.ExecuteNonQuery();
                }
                conn.Close();
                conn.Dispose();
            }
            return result;
        }

        public int UpdataClearDocumentId(string DocumentId)
        {
            var result = 0;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand com = conn.CreateCommand())
                {
                    string Sql = @" Update APPLY_DOCUMENT Set DOC_NO = NULL WHERE DOC_NO=@DOC_NO ";
                    DataUtils.AddParameters(com, "DOC_NO", DocumentId);

                    com.CommandText = Sql;

                    result = com.ExecuteNonQuery();
                }
                conn.Close();
                conn.Dispose();
            }
            return result;
        }




        public int UpdataDocumentId(DocumentExportModel.DocumentSet model)
        {
            var result = 0;
            using (SqlConnection conn = DataUtils.GetConnection())
            {
                conn.Open();
                using (SqlCommand com = conn.CreateCommand())
                {
                    string Sql = @" Update SETUP Set SETUP_VAL = @PATH WHERE SETUP_CD='DOWNLOAD_DOCUMENT_PATH';Update SETUP Set SETUP_VAL = @MAIL WHERE SETUP_CD='DOWNLOAD_DOCUMENT_MAIL';";
                    DataUtils.AddParameters(com, "PATH", model.path);
                    DataUtils.AddParameters(com, "MAIL", model.mail);

                    com.CommandText = Sql;

                    result = com.ExecuteNonQuery();
                }
                conn.Close();
                conn.Dispose();
            }
            return result;
        }
    }
}