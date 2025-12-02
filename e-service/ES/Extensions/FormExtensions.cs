using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using log4net;
using ES.Utils;

namespace ES.Extensions
{
    public static class FormExtensions
    {
        private static readonly ILog logger = LogUtils.GetLogger();

        /// <summary>
        /// 建立表單畫面
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <returns></returns>
        public static MvcHtmlString BuildFormTable(this HtmlHelper htmlHelper)
        {
            List<Dictionary<string, string>> list = htmlHelper.ViewBag.List;
            StringBuilder sb = new StringBuilder();

            foreach (Dictionary<string, string> dict in list)
            {
                if (dict["TABLE_TYPE"].StartsWith("0"))
                {
                    #region 程式碼
                    
                    if (dict["TABLE_TYPE"].Equals("04"))
                    {
                        sb.Append("<table class=\"formTable1\" style=\"margin-top:-17px;\">");
                        sb.Append("<caption style=\"display: none;\">");
                    }
                    else
                    {
                        sb.Append("<table class=\"formTable1\">");
                        sb.Append("<caption style=\"padding: 0px 10px 0px 10px;\">");
                        
                    }
                    sb.Append("<div style=\"float: left; padding: 9px 0px 7px 0px; width: 520px;\">");
                    sb.Append(dict["TABLE_TITLE"]);
                    sb.Append("<span style=\"font-size: 12px;\">");
                    sb.Append(dict["TABLE_DESC"]);
                    sb.Append("</span>");
                    sb.Append("</div>");
                    sb.Append("<div style=\"float: right;\">");
                    sb.Append("<input type=\"button\" id=\"btnFormHid" + dict["TABLE_ID"] + "\" value=\"隱藏\" onclick=\"showTableBody(this, '" + dict["TABLE_ID"] + "')\" />");
                    sb.Append("</div>");
                    sb.Append("</caption>");
                    sb.Append("<tbody id=\"tbody_" + dict["TABLE_ID"] + "\">");
                    sb.Append(FormTableBuilder(htmlHelper, dict));
                    sb.Append("</tbody>");
                    sb.Append("</table>");
                    sb.Append("<br />"); 
                    #endregion
                }
                else if (dict["TABLE_TYPE"].StartsWith("1"))
                {
                    #region 程式碼
                    sb.Append("<table class=\"gridTable2\">");
                    sb.Append("<caption style=\"padding: 0px 10px 0px 10px;\">");
                    sb.Append("<div style=\"float: left; padding: 9px 0px 7px 0px; width: 450px;\">");
                    sb.Append(dict["TABLE_TITLE"]);
                    sb.Append("<span style=\"font-size: 12px;\">");
                    sb.Append(dict["TABLE_DESC"]);
                    sb.Append("</span>");
                    sb.Append("</div>");
                    sb.Append("<div style=\"float: right; padding: 5px 0px 0px 0px;\">");
                    if (dict["TABLE_TYPE"].Equals("11"))
                    {
                        //sb.Append("<input type=\"button\" value=\"新增資料\" onclick=\"insertRow('tbody_" + @dict["TABLE_ID"] + "');\" />");
                        sb.Append("<input type=\"button\" value=\"新增下方欄位\" onclick=\"insertRow('tbody_" + @dict["TABLE_ID"] + "');\" />");
                    }
                    else if (dict["TABLE_TYPE"].Equals("12"))
                    {
                        //sb.Append("<input type=\"button\" value=\"新增資料\" onclick=\"insertRow2('tbody_" + @dict["TABLE_ID"] + "');\" />");
                        sb.Append("<input type=\"button\" value=\"新增下方欄位\" onclick=\"insertRow2('tbody_" + @dict["TABLE_ID"] + "');\" />");
                    }
                    sb.Append("<input type=\"button\" value=\"全部刪除\" onclick=\"deleteAll('" + @dict["TABLE_ID"] + "');\" />");
                    sb.Append("<input type=\"button\" id=\"btnFormHid" + dict["TABLE_ID"] + "\" value=\"隱藏\" onclick=\"showTableBody(this, '" + dict["TABLE_ID"] + "')\" />");
                    sb.Append("<input type=\"hidden\" name=\"tbody_" + dict["TABLE_ID"] + "_max\" id=\"tbody_" + dict["TABLE_ID"] + "_max\" value=\"0\" />");
                    sb.Append("</div>");
                    sb.Append("</caption>");
                    sb.Append("<tbody id=\"tbody_" + @dict["TABLE_ID"] + "\">");
                    sb.Append(FormTableBuilder(htmlHelper, dict));
                    sb.Append("</tbody>");
                    sb.Append("</table>");
                    sb.Append("<br />"); 
                    #endregion
                }
            }

            return MvcHtmlString.Create(sb.ToString());
        }

        /// <summary>
        /// 建立預覽畫面
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <returns></returns>
        public static MvcHtmlString BuildPreviewTable(this HtmlHelper htmlHelper)
        {
            List<Dictionary<string, string>> list = htmlHelper.ViewBag.List;
            StringBuilder sb = new StringBuilder();

            foreach (Dictionary<string, string> dict in list)
            {
                if (dict["TABLE_TYPE"].StartsWith("0"))
                {
                    #region 程式碼
                    if (dict["TABLE_TYPE"].Equals("04"))
                    {
                        sb.Append("<table class=\"formTable1\" style=\"margin-top:-17px;\">");
                        sb.Append("<caption style=\"display: none;\">");
                    }
                    else
                    {
                        sb.Append("<table class=\"formTable1\">");
                        sb.Append("<caption style=\"padding: 0px 10px 0px 10px;\">");

                    }
                    sb.Append("<div style=\"float: left; padding: 9px 0px 7px 0px; width: 600px;\">");
                    sb.Append(dict["TABLE_TITLE"]);
                    sb.Append("<span style=\"font-size: 12px;\">");
                    sb.Append(dict["TABLE_DESC"]);
                    sb.Append("</span>");
                    sb.Append("</div>");
                    sb.Append("<div style=\"float: right;\">");
                    sb.Append("<input type=\"button\" id=\"btnFormHid" + dict["TABLE_ID"] + "\" value=\"隱藏\" onclick=\"showTableBody(this, '" + dict["TABLE_ID"] + "')\" />");
                    sb.Append("</div>");
                    sb.Append("</caption>");
                    sb.Append("<tbody id=\"tbody_" + dict["TABLE_ID"] + "\">");
                    sb.Append(PreviewTableBuilder(htmlHelper, dict));
                    sb.Append("</tbody>");
                    sb.Append("</table>");
                    sb.Append("<br />");
                    #endregion
                }
                else if (dict["TABLE_TYPE"].StartsWith("1"))
                {
                    #region 程式碼
                    sb.Append("<table class=\"gridTable2\">");
                    sb.Append("<caption style=\"padding: 0px 10px 0px 10px;\">");
                    sb.Append("<div style=\"float: left; padding: 9px 0px 7px 0px; width: 600px;\">");
                    sb.Append(dict["TABLE_TITLE"]);
                    sb.Append("<span style=\"font-size: 12px;\">");
                    sb.Append(dict["TABLE_DESC"]);
                    sb.Append("</span>");
                    sb.Append("</div>");
                    sb.Append("<div style=\"float: right; padding: 5px 0px 0px 0px;\">");
                    sb.Append("<input type=\"button\" id=\"btnFormHid" + dict["TABLE_ID"] + "\" value=\"隱藏\" onclick=\"showTableBody(this, '" + dict["TABLE_ID"] + "')\" />");
                    sb.Append("<input type=\"hidden\" name=\"tbody_" + dict["TABLE_ID"] + "_max\" id=\"tbody_" + dict["TABLE_ID"] + "_max\" value=\"0\" />");
                    sb.Append("</div>");
                    sb.Append("</caption>");
                    sb.Append("<tbody id=\"tbody_" + @dict["TABLE_ID"] + "\">");
                    sb.Append(PreviewTableBuilder(htmlHelper, dict));
                    sb.Append("</tbody>");
                    sb.Append("</table>");
                    sb.Append("<br />");
                    #endregion
                }
            }

            return MvcHtmlString.Create(sb.ToString());
        }

        /// <summary>
        /// 建立表格
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="dict"></param>
        /// <returns></returns>
        private static string FormTableBuilder(this HtmlHelper htmlHelper, Dictionary<string, string> dict)
        {
            List<Dictionary<string, string>> list = htmlHelper.ViewBag.DictList[dict["TABLE_ID"]];

            StringBuilder sb = new StringBuilder();

            switch (dict["TABLE_TYPE"])
            {
                case "01":
                    #region 程式碼
                    sb.Append("<tr>");
                    sb.Append("<th>承辦單位</th>");
                    sb.Append("<td>衛生福利部");//.Append(htmlHelper.ViewBag.Dict["UNIT_NAME"]).Append("</td>");
                    if (dict["SRV_ID"].ToString() == "001038")
                        sb.Append("國民健康署").Append("</td>");
                    else
                        sb.Append(htmlHelper.ViewBag.Dict["UNIT_NAME"]).Append("</td>");
                    sb.Append("</tr>");

                    for (int i = 0; i < list.Count; i++)
                    {
                        if (!String.IsNullOrEmpty(list[i]["FIELD_TITLE"]))
                        {
                            sb.Append("<tr>");
                            sb.Append("<th");
                            if (!String.IsNullOrEmpty(list[i]["TITLE_STYLE"]))
                            {
                                sb.Append(" style=\"" + list[i]["TITLE_STYLE"] + "\"");
                            }
                            if (!String.IsNullOrEmpty(list[i]["TITLE_ATTR"]))
                            {
                                sb.Append(" " + list[i]["TITLE_ATTR"] + "");
                            }
                            sb.Append(">");
                            sb.Append(list[i]["TITLE_LEFT"]);
                            if (list[i]["REQ_MK"].Equals("Y"))
                            {
                                sb.Append(ShowRequired());
                            }
                            sb.Append(list[i]["FIELD_TITLE"]);
                            sb.Append(list[i]["TITLE_RIGHT"]);
                            sb.Append("</th>");
                            sb.Append("<td>");
                        }

                        sb.Append(FormField(list[i], (Dictionary<string, List<SelectListItem>>)htmlHelper.ViewBag.Item));

                        if (i == list.Count - 1 || !String.IsNullOrEmpty(list[i + 1]["FIELD_TITLE"]))
                        {
                            sb.Append("</td>");
                            sb.Append("</tr>");
                        }
                    }
                    break;
                    #endregion
                case "02":
                case "04":
                    #region 程式碼
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (!String.IsNullOrEmpty(list[i]["FIELD_TITLE"]))
                        {
                            sb.Append("<tr>");
                            sb.Append("<th");
                            if (!String.IsNullOrEmpty(list[i]["TITLE_STYLE"]))
                            {
                                sb.Append(" style=\"" + list[i]["TITLE_STYLE"] + "\"");
                            }
                            if (!String.IsNullOrEmpty(list[i]["TITLE_ATTR"]))
                            {
                                sb.Append(" " + list[i]["TITLE_ATTR"] + "");
                            }
                            sb.Append(">");
                            sb.Append(@list[i]["TITLE_LEFT"]);
                            if (list[i]["REQ_MK"].Equals("Y"))
                            {
                                sb.Append(ShowRequired());
                            }
                            sb.Append(list[i]["FIELD_TITLE"]);
                            sb.Append(list[i]["TITLE_RIGHT"]);
                            sb.Append("</th>");
                            sb.Append("<td>");
                        }

                        sb.Append(FormField(list[i], (Dictionary<string, List<SelectListItem>>)htmlHelper.ViewBag.Item));

                        if (i == list.Count - 1 || !String.IsNullOrEmpty(list[i + 1]["FIELD_TITLE"]))
                        {
                            sb.Append("</td>");
                            sb.Append("</tr>");
                        }
                    }
                    break;
                    #endregion
                case "03":
                    #region 程式碼
                    sb.Append("<tr>");
                    sb.Append("<th colspan=\"2\">承辦單位</th>");
                    sb.Append("<td>衛生福利部");//.Append(htmlHelper.ViewBag.Dict["UNIT_NAME"]).Append("</td>");
                    if (dict["SRV_ID"].ToString() == "001038")
                        sb.Append("國民健康署").Append("</td>");
                    else
                        sb.Append(htmlHelper.ViewBag.Dict["UNIT_NAME"]).Append("</td>");
                    sb.Append("</tr>");
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (!String.IsNullOrEmpty(list[i]["FIELD_TITLE"]))
                        {
                            sb.Append("<tr>");
                            sb.Append("<th");
                            if (!String.IsNullOrEmpty(list[i]["TITLE_STYLE"]))
                            {
                                sb.Append(" style=\"" + list[i]["TITLE_STYLE"] + "\"");
                            }
                            if (!String.IsNullOrEmpty(list[i]["TITLE_ATTR"]))
                            {
                                sb.Append(" " + list[i]["TITLE_ATTR"] + "");
                            }
                            sb.Append(">");
                            sb.Append(list[i]["TITLE_LEFT"]);
                            if (list[i]["REQ_MK"].Equals("Y"))
                            {
                                sb.Append(ShowRequired());
                            }
                            sb.Append(list[i]["FIELD_TITLE"]);
                            sb.Append(list[i]["TITLE_RIGHT"]);
                            sb.Append("</th>");
                            sb.Append("<td>");
                        }

                        sb.Append(FormField(list[i], (Dictionary<string, List<SelectListItem>>)htmlHelper.ViewBag.Item));

                        if (i == list.Count - 1 || !String.IsNullOrEmpty(list[i + 1]["FIELD_TITLE"]))
                        {
                            sb.Append("</td>");
                            sb.Append("</tr>");
                        }
                    }
                    break;
                    #endregion
                case "11":
                    #region 程式碼
                    sb.Append("<tr>");
                    sb.Append("<th style=\"width: 20px;\">");
                    sb.Append("</th>");
                    sb.Append("<th style=\"width: 20px;\"></th>");
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (!String.IsNullOrEmpty(list[i]["FIELD_TITLE"]))
                        {
                            if (!String.IsNullOrEmpty(list[i]["TITLE_STYLE"]))
                            {
                                sb.Append("<th style=\"" + list[i]["TITLE_STYLE"] + "\">");
                            }
                            else
                            {
                                sb.Append("<th>");
                            }
                            sb.Append(list[i]["TITLE_LEFT"]);
                            if (list[i]["REQ_MK"].Equals("Y"))
                            {
                                sb.Append(ShowRequired());
                            }
                            sb.Append(list[i]["FIELD_TITLE"]);
                            sb.Append(list[i]["TITLE_RIGHT"]);
                            sb.Append("</th>");
                        }
                    }
                    sb.Append("</tr>");
                    sb.Append("<tr style=\"display: none;\">");
                    sb.Append("<td style=\"text-align: right;\">0</td>");
                    sb.Append("<td></td>");
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (!String.IsNullOrEmpty(list[i]["FIELD_TITLE"]))
                        {
                            sb.Append("<td>");
                        }
                        sb.Append(FormField(list[i], (Dictionary<string, List<SelectListItem>>)htmlHelper.ViewBag.Item));
                        if (i == list.Count - 1 || !String.IsNullOrEmpty(list[i + 1]["FIELD_TITLE"]))
                        {
                            sb.Append("</td>");
                        }
                    }
                    sb.Append("</tr>");
                    break;
                    #endregion
                case "12":
                    #region 程式碼
                    sb.Append("<tr style=\"display: none;\">");
                    sb.Append("<th style=\"width: 20px;\">");
                    sb.Append("</th>");
                    sb.Append("<th style=\"width: 20px;\"></th>");
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (!String.IsNullOrEmpty(list[i]["FIELD_TITLE"]))
                        {
                            sb.Append("<th");
                            if (!String.IsNullOrEmpty(list[i]["TITLE_STYLE"]))
                            {
                                sb.Append(" style=\"" + list[i]["TITLE_STYLE"] + "\"");
                            }
                            if (!String.IsNullOrEmpty(list[i]["TITLE_ATTR"]))
                            {
                                sb.Append(" " + list[i]["TITLE_ATTR"] + "");
                            }
                            sb.Append(">");
                            sb.Append(@list[i]["TITLE_LEFT"]);
                            if (list[i]["REQ_MK"].Equals("Y"))
                            {
                                sb.Append(ShowRequired());
                            }
                            sb.Append(list[i]["FIELD_TITLE"]);
                            sb.Append(@list[i]["TITLE_RIGHT"]);
                            sb.Append("</th>");
                        }
                    }
                    sb.Append("</tr>");
                    sb.Append("<tr style=\"display: none;\">");
                    sb.Append("<td style=\"text-align: right;\">0</td>");
                    sb.Append("<td></td>");
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (!String.IsNullOrEmpty(list[i]["FIELD_TITLE"]))
                        {
                            sb.Append("<td>");
                        }
                        sb.Append(FormField(list[i], (Dictionary<string, List<SelectListItem>>)htmlHelper.ViewBag.Item));
                        if (i == list.Count - 1 || !String.IsNullOrEmpty(list[i + 1]["FIELD_TITLE"]))
                        {
                            sb.Append("</td>");
                        }
                    }
                    sb.Append("</tr>");
                    break;
                    #endregion
            }

            return sb.ToString();
        }

        /// <summary>
        /// 建立預覽表格
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static string PreviewTableBuilder(this HtmlHelper htmlHelper, Dictionary<string, string> dict)
        {
            List<Dictionary<string, string>> list = htmlHelper.ViewBag.DictList[dict["TABLE_ID"]];

            StringBuilder sb = new StringBuilder();

            switch (dict["TABLE_TYPE"])
            {
                case "01":
                    #region 程式碼
                    sb.Append("<tr>");
                    sb.Append("<th>承辦單位</th>");
                    sb.Append("<td>衛生福利部");//.Append(htmlHelper.ViewBag.Dict["UNIT_NAME"]).Append("</td>");
                    if (dict["SRV_ID"].ToString() == "001038")
                        sb.Append("國民健康署").Append("</td>");
                    else
                        sb.Append(htmlHelper.ViewBag.Dict["UNIT_NAME"]).Append("</td>");
                    sb.Append("</tr>");

                    for (int i = 0; i < list.Count; i++)
                    {
                        if (!String.IsNullOrEmpty(list[i]["FIELD_TITLE"]))
                        {
                            sb.Append("<tr>");
                            sb.Append("<th");
                            if (!String.IsNullOrEmpty(list[i]["TITLE_STYLE"]))
                            {
                                sb.Append(" style=\"" + list[i]["TITLE_STYLE"] + "\"");
                            }
                            if (!String.IsNullOrEmpty(list[i]["TITLE_ATTR"]))
                            {
                                sb.Append(" " + list[i]["TITLE_ATTR"] + "");
                            }
                            sb.Append(">");
                            sb.Append(list[i]["TITLE_LEFT"]);
                            if (list[i]["REQ_MK"].Equals("Y"))
                            {
                                sb.Append(ShowRequired());
                            }
                            sb.Append(list[i]["FIELD_TITLE"]);
                            sb.Append(list[i]["TITLE_RIGHT"]);
                            sb.Append("</th>");
                            sb.Append("<td>");
                        }

                        sb.Append(PreviewField(list[i], (Dictionary<string, List<SelectListItem>>)htmlHelper.ViewBag.Item, 0));

                        if (i == list.Count - 1 || !String.IsNullOrEmpty(list[i + 1]["FIELD_TITLE"]))
                        {
                            sb.Append("</td>");
                            sb.Append("</tr>");
                        }
                    }
                    break;
                    #endregion
                case "02":
                case "04":
                    #region 程式碼
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (!String.IsNullOrEmpty(list[i]["FIELD_TITLE"]))
                        {
                            sb.Append("<tr>");
                            sb.Append("<th");
                            if (!String.IsNullOrEmpty(list[i]["TITLE_STYLE"]))
                            {
                                sb.Append(" style=\"" + list[i]["TITLE_STYLE"] + "\"");
                            }
                            if (!String.IsNullOrEmpty(list[i]["TITLE_ATTR"]))
                            {
                                sb.Append(" " + list[i]["TITLE_ATTR"] + "");
                            }
                            sb.Append(">");
                            sb.Append(@list[i]["TITLE_LEFT"]);
                            if (list[i]["REQ_MK"].Equals("Y"))
                            {
                                sb.Append(ShowRequired());
                            }
                            sb.Append(list[i]["FIELD_TITLE"]);
                            sb.Append(list[i]["TITLE_RIGHT"]);
                            sb.Append("</th>");
                            sb.Append("<td>");
                        }

                        sb.Append(PreviewField(list[i], (Dictionary<string, List<SelectListItem>>)htmlHelper.ViewBag.Item, 0));

                        if (i == list.Count - 1 || !String.IsNullOrEmpty(list[i + 1]["FIELD_TITLE"]))
                        {
                            sb.Append("</td>");
                            sb.Append("</tr>");
                        }
                    }
                    break;
                    #endregion
                case "03":
                    #region 程式碼
                    sb.Append("<tr>");
                    sb.Append("<th colspan=\"2\">承辦單位</th>");
                    sb.Append("<td>衛生福利部");
                    if (dict["SRV_ID"].ToString() == "001038")
                        sb.Append("國民健康署").Append("</td>");
                    else
                        sb.Append(htmlHelper.ViewBag.Dict["UNIT_NAME"]).Append("</td>");
                    sb.Append("</tr>");
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (!String.IsNullOrEmpty(list[i]["FIELD_TITLE"]))
                        {
                            sb.Append("<tr>");
                            sb.Append("<th");
                            if (!String.IsNullOrEmpty(list[i]["TITLE_STYLE"]))
                            {
                                sb.Append(" style=\"" + list[i]["TITLE_STYLE"] + "\"");
                            }
                            if (!String.IsNullOrEmpty(list[i]["TITLE_ATTR"]))
                            {
                                sb.Append(" " + list[i]["TITLE_ATTR"] + "");
                            }
                            sb.Append(">");
                            sb.Append(list[i]["TITLE_LEFT"]);
                            if (list[i]["REQ_MK"].Equals("Y"))
                            {
                                sb.Append(ShowRequired());
                            }
                            sb.Append(list[i]["FIELD_TITLE"]);
                            sb.Append(list[i]["TITLE_RIGHT"]);
                            sb.Append("</th>");
                            sb.Append("<td>");
                        }

                        sb.Append(PreviewField(list[i], (Dictionary<string, List<SelectListItem>>)htmlHelper.ViewBag.Item, 0));

                        if (i == list.Count - 1 || !String.IsNullOrEmpty(list[i + 1]["FIELD_TITLE"]))
                        {
                            sb.Append("</td>");
                            sb.Append("</tr>");
                        }
                    }
                    break;
                    #endregion
                case "11":
                    #region 程式碼
                    sb.Append("<tr>");
                    sb.Append("<th style=\"width: 20px;\">");
                    sb.Append("</th>");
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (!String.IsNullOrEmpty(list[i]["FIELD_TITLE"]))
                        {
                            if (!String.IsNullOrEmpty(list[i]["TITLE_STYLE"]))
                            {
                                sb.Append("<th style=\"" + list[i]["TITLE_STYLE"] + "\">");
                            }
                            else
                            {
                                sb.Append("<th>");
                            }
                            sb.Append(list[i]["TITLE_LEFT"]);
                            if (list[i]["REQ_MK"].Equals("Y"))
                            {
                                sb.Append(ShowRequired());
                            }
                            sb.Append(list[i]["FIELD_TITLE"]);
                            sb.Append(list[i]["TITLE_RIGHT"]);
                            sb.Append("</th>");
                        }
                    }
                    sb.Append("</tr>");
                    
                    if (((Dictionary<string, List<Dictionary<string, object>>>)htmlHelper.ViewBag.DBList).ContainsKey(dict["TABLE_DB_NAME"]))
                    {
                        foreach (Dictionary<string, object> item in ((Dictionary<string, List<Dictionary<string, object>>>)htmlHelper.ViewBag.DBList)[dict["TABLE_DB_NAME"]])
                        {
                            sb.Append("<tr>");
                            sb.Append("<td style=\"text-align: right;\">").Append(item["SRL_NO"]).Append("</td>");
                            for (int i = 0; i < list.Count; i++)
                            {
                                if (!String.IsNullOrEmpty(list[i]["FIELD_TITLE"]))
                                {
                                    sb.Append("<td>");
                                }
                                if (!String.IsNullOrEmpty(list[i]["FIELD_NAME"]) && item.ContainsKey(list[i]["FIELD_NAME"]))
                                {
                                    //logger.Debug("EE-" + list[i]["FIELD_NAME"] + ": " + item[list[i]["FIELD_NAME"]]);

                                    if (item[list[i]["FIELD_NAME"]].GetType().ToString().Equals("System.DateTime"))
                                    {
                                        list[i]["FIELD_DEF"] = ((DateTime)item[list[i]["FIELD_NAME"]]).ToString("yyyy/MM/dd");
                                    }
                                    else
                                    {
                                        list[i]["FIELD_DEF"] = item[list[i]["FIELD_NAME"]].ToString();
                                    }
                                    //list[i]["FIELD_DEF"] = (string)item[list[i]["FIELD_NAME"]];
                                }
                                sb.Append(PreviewField(list[i], (Dictionary<string, List<SelectListItem>>)htmlHelper.ViewBag.Item, (int)item["SRL_NO"]));
                                if (i == list.Count - 1 || !String.IsNullOrEmpty(list[i + 1]["FIELD_TITLE"]))
                                {
                                    sb.Append("</td>");
                                }
                            }
                            sb.Append("</tr>");
                        }
                    }
                    
                    break;
                    #endregion
                case "12":
                    #region 程式碼
                    sb.Append("<tr style=\"display: none;\">");
                    sb.Append("<th style=\"width: 20px;\">");
                    sb.Append("</th>");
                    sb.Append("<th style=\"width: 50px;\"></th>");
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (!String.IsNullOrEmpty(list[i]["FIELD_TITLE"]))
                        {
                            sb.Append("<th");
                            if (!String.IsNullOrEmpty(list[i]["TITLE_STYLE"]))
                            {
                                sb.Append(" style=\"" + list[i]["TITLE_STYLE"] + "\"");
                            }
                            if (!String.IsNullOrEmpty(list[i]["TITLE_ATTR"]))
                            {
                                sb.Append(" " + list[i]["TITLE_ATTR"] + "");
                            }
                            sb.Append(">");
                            sb.Append(@list[i]["TITLE_LEFT"]);
                            if (list[i]["REQ_MK"].Equals("Y"))
                            {
                                sb.Append(ShowRequired());
                            }
                            sb.Append(list[i]["FIELD_TITLE"]);
                            sb.Append(@list[i]["TITLE_RIGHT"]);
                            sb.Append("</th>");
                        }
                    }
                    sb.Append("</tr>");
                    if (((Dictionary<string, List<Dictionary<string, object>>>)htmlHelper.ViewBag.DBList).ContainsKey(dict["TABLE_DB_NAME"]))
                    {
                        foreach (Dictionary<string, object> item in ((Dictionary<string, List<Dictionary<string, object>>>)htmlHelper.ViewBag.DBList)[dict["TABLE_DB_NAME"]])
                        {
                            sb.Append("<tr>");
                            sb.Append("<td style=\"text-align: right;\">").Append(item["SRL_NO"]).Append("</td>");
                            for (int i = 0; i < list.Count; i++)
                            {
                                if (!String.IsNullOrEmpty(list[i]["FIELD_TITLE"]))
                                {
                                    sb.Append("<td>");
                                }
                                if (!String.IsNullOrEmpty(list[i]["FIELD_NAME"]) && item.ContainsKey(list[i]["FIELD_NAME"]))
                                {
                                    //logger.Debug(list[i]["FIELD_NAME"] + ": " + item[list[i]["FIELD_NAME"]]);

                                    if (item[list[i]["FIELD_NAME"]].GetType().ToString().Equals("System.DateTime"))
                                    {
                                        list[i]["FIELD_DEF"] = ((DateTime)item[list[i]["FIELD_NAME"]]).ToString("yyyy/MM/dd");
                                    }
                                    else
                                    {
                                        list[i]["FIELD_DEF"] = item[list[i]["FIELD_NAME"]].ToString();
                                    }
                                    //list[i]["FIELD_DEF"] = (string)item[list[i]["FIELD_NAME"]];
                                }
                                sb.Append(PreviewField(list[i], (Dictionary<string, List<SelectListItem>>)htmlHelper.ViewBag.Item, (int)item["SRL_NO"]));
                                if (i == list.Count - 1 || !String.IsNullOrEmpty(list[i + 1]["FIELD_TITLE"]))
                                {
                                    sb.Append("</td>");
                                }
                            }
                            sb.Append("</tr>");
                        }
                    }
                    break;
                    #endregion
            }

            return sb.ToString();
        }

        /// <summary>
        /// 建立表格欄位
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <param name="dict"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        private static string FormField(Dictionary<string, string> dict, Dictionary<string, List<System.Web.Mvc.SelectListItem>> item)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(dict["FIELD_LEFT"]);

            switch (dict["FORM_FIELD_CD"])
            {
                case "0": // 空白
                    #region 程式碼
                    break;
                    #endregion
                case "1": // 單選 (RadioBox)
                    #region 程式碼
                    for (int i = 0; i < item[dict["FIELD_NAME"]].Count; i++)
                    {
                        sb.Append("<span>");
                        sb.Append("<input type=\"radio\" name=\"" + GetFieldName(dict, 0) + "\" id=\"" + GetFieldName(dict, 0) + "_" + i + "\" value=\"" + item[dict["FIELD_NAME"]][i].Value + "\" " + dict["FIELD_ATTR"] + " ");
                        if (item[dict["FIELD_NAME"]][i].Selected)
                        {
                            sb.Append(" checked=\"true\"");
                        }
                        if (!String.IsNullOrEmpty(dict["FIELD_ATTR"]))
                        {
                            sb.Append(" " + dict["FIELD_ATTR"] + "");
                        }
                        sb.Append("/>" + item[dict["FIELD_NAME"]][i].Text);
                        sb.Append("</span>");
                    }

                    break;
                    #endregion
                case "2": // 複選 (CheckBox)
                    #region 程式碼
                    for (int i = 0; i < item[dict["FIELD_NAME"]].Count; i++)
                    {
                        sb.Append("<span>");
                        sb.Append("<input type=\"checkbox\" name=\"" + GetFieldName(dict, 0) + "\" id=\"" + GetFieldName(dict, 0) + "_" + i + "\" value=\"" + item[dict["FIELD_NAME"]][i].Value + "\" " + dict["FIELD_ATTR"] + " ");
                        if (item[dict["FIELD_NAME"]][i].Selected)
                        {
                            sb.Append(" checked=\"checked\"");
                        }
                        if (!String.IsNullOrEmpty(dict["FIELD_ATTR"]))
                        {
                            sb.Append(" " + dict["FIELD_ATTR"] + "");
                        }
                        sb.Append("/>" + @item[dict["FIELD_NAME"]][i].Text);
                        sb.Append("</span>");
                    }

                    break;
                    #endregion
                case "3": // 勾選 (CheckBox)
                    #region 程式碼
                    sb.Append("<input type=\"checkbox\" name=\"" + GetFieldName(dict, 0) + "\" id=\"" + GetFieldName(dict, 0) + "\" " + dict["FIELD_ATTR"] + " ");

                    if (String.IsNullOrEmpty(dict["FIELD_VAL"]))
                    {
                        sb.Append("value=\"value\" ");
                    }
                    else
                    {
                        sb.Append("value=\"" + dict["FIELD_VAL"] + "\" ");
                    }

                    if (dict["FIELD_DEF"].Equals("Y"))
                    {
                        sb.Append("checked=\"checked\" ");
                    }
                    sb.Append("/>");
                    break;
                    #endregion
                case "21": // 下拉選單
                case "22": // 互動式下拉選單
                case "23": // 互動式下拉選單子項目
                case "24": // 下拉選單 (無空項目)
                    #region 程式碼
                    sb.Append("<span>");
                    sb.Append("<select name=\"" + GetFieldName(dict, 0) + "\" id=\"" + GetFieldName(dict, 0) + "\" " + dict["FIELD_ATTR"] + " ");

                    if (!String.IsNullOrEmpty(dict["FIELD_STYLE"]))
                    {
                        sb.Append(" style=\"" + dict["FIELD_STYLE"] + "\" ");
                    }

                    sb.Append(">");

                    for (int i = 0; i < item[dict["FIELD_NAME"]].Count; i++)
                    {
                        sb.Append("<option value=\"" + item[dict["FIELD_NAME"]][i].Value + "\" ");
                        if (item[dict["FIELD_NAME"]][i].Selected)
                        {
                            sb.Append("selected=\"selected\"");
                        }
                        sb.Append(">" + item[dict["FIELD_NAME"]][i].Text + "</option>");
                    }
                    sb.Append("</select>");
                    sb.Append("</span>");
                    break;
                    #endregion
                case "31": // Textarea row=2
                case "32": // Textarea row=3
                    #region 程式碼
                    sb.Append("<span>");
                    sb.Append("<textarea name=\"" + GetFieldName(dict, 0) + "\" id=\"" + GetFieldName(dict, 0) + "\" ");
                    if (!String.IsNullOrEmpty(dict["FIELD_STYLE"]))
                    {
                        sb.Append(" style=\"" + dict["FIELD_STYLE"] + "\" ");
                    }
                    if (dict["FORM_FIELD_CD"].Equals("31"))
                    {
                        sb.Append("rows=\"2'\" ");
                    }
                    else if (dict["FORM_FIELD_CD"].Equals("32"))
                    {
                        sb.Append("rows=\"3'\" ");
                    }
                    if (!String.IsNullOrEmpty(dict["FIELD_ATTR"]))
                    {
                        sb.Append(" " + dict["FIELD_ATTR"] + "");
                    }
                    sb.Append(">");
                    sb.Append(dict["FIELD_DEF"]);
                    sb.Append("</textarea>");
                    sb.Append("</span>");
                    break;
                    #endregion
                case "41": // 檔案上傳
                case "42": // 檔案上傳 (圖檔)
                case "43": // 檔案上傳 (doc/docx/gif/jpg/tif)
                case "44": // 檔案上傳 (doc/docx/pdf/gif/jpg/tif)
                    #region 程式碼
                    sb.Append("<span>");
                    sb.Append("<input type=\"file\" name=\"" + GetFieldName(dict, 0) + "\" id=\"" + GetFieldName(dict, 0) + "\" value=\"" + dict["FIELD_DEF"] + "\" ");
                    if (!String.IsNullOrEmpty(dict["FIELD_STYLE"]))
                    {
                        sb.Append(" style=\"" + dict["FIELD_STYLE"] + "\" ");
                    }
                    if (!String.IsNullOrEmpty(dict["FIELD_LIMIT"]))
                    {
                        sb.Append(" maxlength=\"" + dict["FIELD_LIMIT"] + "\" ");
                    }
                    if (dict["FORM_FIELD_CD"].Equals("52"))
                    {
                        sb.Append("readonly=\"readonly\"");
                    }
                    if (!String.IsNullOrEmpty(dict["FIELD_ATTR"]))
                    {
                        sb.Append(" " + dict["FIELD_ATTR"] + "");
                    }
                    sb.Append("/>");
                    sb.Append("</span>");
                    break;
                    #endregion
                case "105": // 隱藏欄位 (預覽時顯示)
                case "106": // 隱藏欄位
                    #region 程式碼
                    sb.Append("<input type=\"hidden\" name=\"" + GetFieldName(dict, 0) + "\" id=\"" + GetFieldName(dict, 0) + "\" value=\"" + dict["FIELD_DEF"] + "\" />");
                    break;
                    #endregion
                default:
                    #region 程式碼
                    sb.Append("<input type=\"text\" name=\"" + GetFieldName(dict, 0) + "\" id=\"" + GetFieldName(dict, 0) + "\" value=\"" + dict["FIELD_DEF"] + "\" ");
                    if (!String.IsNullOrEmpty(dict["FIELD_STYLE"]))
                    {
                        sb.Append(" style=\"" + dict["FIELD_STYLE"] + "\" ");
                    }
                    if (!String.IsNullOrEmpty(dict["FIELD_LIMIT"]))
                    {
                        sb.Append(" maxlength=\"" + dict["FIELD_LIMIT"] + "\" ");
                    }
                    if (dict["FORM_FIELD_CD"].Equals("102"))
                    {
                        sb.Append(" readonly=\"readonly\"");
                    }
                    if (dict["FORM_FIELD_CD"].Equals("103"))
                    {
                        sb.Append(" disabled=\"disabled\"");
                    }
                    if (!String.IsNullOrEmpty(dict["FIELD_ATTR"]))
                    {
                        sb.Append(" " + dict["FIELD_ATTR"] + "");
                    }
                    sb.Append("/>");
                    #endregion
                    break;
            }

            sb.Append(dict["FIELD_RIGHT"]);

            return sb.ToString();
        }

        private static string PreviewField(Dictionary<string, string> dict, Dictionary<string, List<System.Web.Mvc.SelectListItem>> item, int idx)
        {
            StringBuilder sb = new StringBuilder();

            if (dict["PREVIEW_MK"].Equals("Y"))
            {
                sb.Append(dict["FIELD_LEFT"]);

                //logger.Debug(GetFieldName(dict, idx) + ": " + dict["FIELD_DEF"]);

                switch (dict["FORM_FIELD_CD"])
                {
                    case "0": // 空白
                        #region 程式碼
                        break;
                        #endregion
                    case "1": // 單選 (RadioBox)
                        #region 程式碼
                        for (int i = 0; i < item[dict["FIELD_NAME"]].Count; i++)
                        {
                            //logger.Debug(GetFieldName(dict, idx) + " / " + item[dict["FIELD_NAME"]][i].Value);
                            sb.Append("<span>");
                            sb.Append("<input type=\"radio\" name=\"" + GetFieldName(dict, idx) + "\" id=\"" + GetFieldName(dict, idx) + "_" + i + "\" value=\"" + item[dict["FIELD_NAME"]][i].Value + "\" ");

                            //logger.Debug(item[dict["FIELD_NAME"]] + ":" + item[dict["FIELD_NAME"]]);

                            if (item[dict["FIELD_NAME"]][i].Value.Equals(dict["FIELD_DEF"]))
                            {
                                sb.Append(" checked=\"checked\"");
                            }
                            /*
                            if (@item[dict["FIELD_NAME"]][i].Selected)
                            {
                                sb.Append(" checked=\"checked\"");
                            }
                            */
                            if (!String.IsNullOrEmpty(dict["FIELD_ATTR"]))
                            {
                                sb.Append(" " + dict["FIELD_ATTR"] + "");
                            }
                            sb.Append("/>" + item[dict["FIELD_NAME"]][i].Text);
                            sb.Append("</span>");
                        }
                        break;
                        #endregion
                    case "2": // 複選 (CheckBox)
                        #region 程式碼
                        for (int i = 0; i < item[dict["FIELD_NAME"]].Count; i++)
                        {
                            sb.Append("<span>");
                            sb.Append("<input type=\"checkbox\" name=\"" + GetFieldName(dict, idx) + "\" id=\"" + GetFieldName(dict, idx) + "_" + i + "\" value=\"" + item[dict["FIELD_NAME"]][i].Value + "\" ");
                            if (@item[dict["FIELD_NAME"]][i].Selected)
                            {
                                sb.Append(" checked=\"checked\"");
                            }
                            if (!String.IsNullOrEmpty(dict["FIELD_ATTR"]))
                            {
                                sb.Append(" " + dict["FIELD_ATTR"] + "");
                            }
                            sb.Append("/>" + item[dict["FIELD_NAME"]][i].Text);
                            sb.Append("</span>");
                        }
                        break;
                        #endregion
                    case "3": // 勾選 (CheckBox)
                        #region 程式碼
                        sb.Append("<input type=\"checkbox\" name=\"" + GetFieldName(dict, idx) + "\" id=\"" + GetFieldName(dict, idx) + "\" value=\"Y\" ");
                        if (dict["FIELD_DEF"].Equals("Y"))
                        {
                            sb.Append("checked=\"checked\" ");
                        }
                        sb.Append("/>");
                        break;
                        #endregion
                    case "21": // 下拉選單
                    case "22": // 互動式下拉選單
                    case "23": // 互動式下拉選單子項目
                    case "24": // 下拉選單 (無空項目)
                        #region 程式碼
                        /*
                        sb.Append("<span>");
                        if (!String.IsNullOrEmpty(dict["FIELD_DEF"]))
                        {
                            for (int i = 0; i < item[dict["FIELD_NAME"]].Count; i++)
                            {
                                if (item[dict["FIELD_NAME"]][i].Value == dict["FIELD_DEF"])
                                {
                                    sb.Append(item[dict["FIELD_NAME"]][i].Text);
                                    sb.Append("<input type=\"hidden\" name=\"" + GetFieldName(dict, idx) + "\" id=\"" + GetFieldName(dict, idx) + "_" + i + "\" value=\"" + item[dict["FIELD_NAME"]][i].Value + "\" />");
                                }
                            }
                        }
                        sb.Append("</span>");
                        */
                        //logger.Debug("AA21: " + dict["FIELD_DEF"]);
                        sb.Append("<span>" + dict["FIELD_DEF"] + "</span>");
                        break;
                        #endregion
                    case "31": // Textarea row=2
                    case "32": // Textarea row=3
                        #region 程式碼
                        sb.Append("<span>");
                        if (!String.IsNullOrEmpty(dict["FIELD_DEF"]))
                        {
                            sb.Append(dict["FIELD_DEF"].Replace("\n", "<br/>"));
                        }
                        sb.Append("</span>");
                        break; 
                        #endregion
                    case "41": // 檔案上傳
                    case "42": // 檔案上傳 (圖檔)
                    case "43": // 檔案上傳 (doc/docx/gif/jpg/tif)
                    case "44": // 檔案上傳 (doc/docx/pdf/gif/jpg/tif)
                        #region 程式碼
                        sb.Append("<span>");

                        if (!String.IsNullOrEmpty(dict["FIELD_DEF"]))
                        {
                            string fileName = dict["FIELD_DEF"].Substring(dict["FIELD_DEF"].LastIndexOf("\\") + 1);

                            //logger.Debug("dict[\"FIELD_DEF\"]: " + dict["FIELD_DEF"]);

                            if (dict["FIELD_DEF"].StartsWith("Temp"))
                            {
                                //sb.Append("<a href=\"File?path=" + HttpUtility.UrlDecode(dict["FIELD_DEF"]) + "\">" + fileName + "</a>");
                                sb.Append("<a href=\"../File/Apply?path=" + HttpUtility.UrlDecode(DataUtils.ToBase64String(dict["FIELD_DEF"])) + "\">" + fileName + "</a>");
                            }
                            else
                            {
                                //sb.Append("<a href=\"../File?path=" + HttpUtility.UrlDecode(dict["FIELD_DEF"]) + "\">" + fileName + "</a>");
                                sb.Append("<a href=\"../../File/Apply?path=" + HttpUtility.UrlDecode(DataUtils.ToBase64String(dict["FIELD_DEF"])) + "\">" + fileName + "</a>");
                            }
                            
                        }
                        sb.Append("</span>");
                        break;
                        #endregion
                    case "106": // 隱藏欄位
                        #region 程式碼
                        sb.Append("<input type=\"hidden\" name=\"" + GetFieldName(dict, idx) + "\" id=\"" + GetFieldName(dict, idx) + "\" value=\"" + dict["FIELD_DEF"] + "\" />");
                        break;
                        #endregion
                    default:
                        #region 程式碼
                        sb.Append(dict["FIELD_DEF"]);
                        sb.Append("<input type=\"hidden\" name=\"" + GetFieldName(dict, idx) + "\" id=\"" + GetFieldName(dict, idx) + "\" value=\"" + dict["FIELD_DEF"] + "\" />");
                        break;
                        #endregion
                }

                sb.Append(dict["FIELD_RIGHT"]);
            }

            return sb.ToString();
        }


        /// <summary>
        /// 取得欄位名稱
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        private static string GetFieldName(Dictionary<string, string> dict, int idx)
        {
            if (dict["TABLE_TYPE"].Equals("11") || dict["TABLE_TYPE"].Equals("12"))
            {
                return @dict["FIELD_NAME"] + "_" + idx;
            }
            else
            {
                return @dict["FIELD_NAME"];
            }
        }

        /// <summary>
        /// 顯示必填
        /// </summary>
        /// <returns></returns>
        private static string ShowRequired()
        {
            return "<span class=\"formRequired\">*</span>";
        }
    }
}