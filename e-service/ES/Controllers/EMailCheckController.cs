using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ES.Models.ViewModels;
using ES.DataLayers;
using ES.Models.Entities;
using ES.Services;
using ES.Utils;

namespace ES.Controllers
{
    public class EMailCheckController : BaseNoMemberController
    {
        public ActionResult Check(string APP_ID)
        {
            Apply_010001CheckModel form = new Apply_010001CheckModel();
            ApplyDAO dao = new ApplyDAO();
            form.status = "N";
            if (!string.IsNullOrWhiteSpace(APP_ID))
            {
                if (APP_ID.Substring(8, 6) == "010001")
                {
                    TblAPPLY_010001 where = new TblAPPLY_010001();
                    where.APP_ID = APP_ID;
                    var temp = dao.GetRow(where);
                    var flagTime = temp.ADD_TIME?.ToString("yyyyMMddHHmm").TOInt64() + 2400;
                    var now = DateTime.Now.ToString("yyyyMMddHHmm").TOInt64();
                    if (flagTime - now <= 0)
                    {
                        //已超過期限
                        form.status = "N";
                    }
                    else
                    {
                        //期限內
                        form.status = "Y";

                        TblAPPLY_010001 newData = new TblAPPLY_010001();
                        newData.CHECK_FLAG = "Y";
                        using (SqlConnection conn = DataUtils.GetConnection())
                        {
                            conn.Open();
                            SqlTransaction tran = conn.BeginTransaction();
                            dao.Tran(conn, tran);
                            dao.Update(newData, where);
                            tran.Commit();
                            conn.Close();
                            conn.Dispose();
                        }
                    }
                }
                else if (APP_ID.Substring(8, 6) == "012001")
                {
                    TblAPPLY_012001 where = new TblAPPLY_012001();
                    where.APP_ID = APP_ID;
                    var temp = dao.GetRow(where);
                    var flagTime = temp.ADD_TIME?.ToString("yyyyMMddHHmm").TOInt64() + 2400;
                    var now = DateTime.Now.ToString("yyyyMMddHHmm").TOInt64();
                    if (flagTime - now <= 0)
                    {
                        //已超過期限
                        form.status = "N";
                    }
                    else
                    {
                        //期限內
                        form.status = "Y";

                        TblAPPLY_012001 newData = new TblAPPLY_012001();
                        newData.CHECK_FLAG = "Y";
                        using (SqlConnection conn = DataUtils.GetConnection())
                        {
                            conn.Open();
                            SqlTransaction tran = conn.BeginTransaction();
                            dao.Tran(conn, tran);
                            dao.Update(newData, where);
                            tran.Commit();
                            conn.Close();
                            conn.Dispose();
                        }
                    }
                }
            }
            return View("Done", form);
        }

    }
}
