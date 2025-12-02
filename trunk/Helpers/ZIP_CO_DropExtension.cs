using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using EECOnline.Models;
using EECOnline.Services;

namespace Turbo.Helpers
{
    /// <summary>
    /// HTML 行政區 輸入框產生輔助方法類別
    /// </summary>
    public static class ZIP_CO_DropExtension
    {
        /// <summary>HTML 行政區單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="expressionTEXT">TEXT(名稱)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString ZIP_CO_DropForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression,
            Expression<Func<TModel, object>> expressionTEXT,
            bool IsReadOnly = false,
            object htmlAttributes = null, bool enabled = true)
        {
            ShareCodeListModel list = new ShareCodeListModel();

            //HTML 標籤的 id 與 name 屬性值
            var name = ExpressionHelper.GetExpressionText(expression);
            var EXPname = name;
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value = Convert.ToString(metadata.Model).ToLower();

            var propertyName = templateInfo.GetFullHtmlFieldName(name);
            var propertyId = templateInfo.GetFullHtmlFieldId(propertyName);

            //HTML 標籤的 id 與 name 屬性值
            var name_text = ExpressionHelper.GetExpressionText(expressionTEXT);
            var metadata_text = ModelMetadata.FromLambdaExpression(expressionTEXT, htmlHelper.ViewData);
            var templateInfo_text = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value_text = Convert.ToString(metadata.Model).ToLower();

            var propertyName_text = templateInfo.GetFullHtmlFieldName(name_text);
            var propertyId_text = templateInfo.GetFullHtmlFieldId(propertyName_text);

            // HTML標籤
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n<div class='input-group'>");
            sb.Append(htmlHelper.TextBoxFor(expression, new { @class = "form-normal", size = 10, maxlength = 5, placeholder = "郵遞區號" }, IsReadOnly).ToHtmlString());

            if (!IsReadOnly)
            {
                sb.Append("<span class=' mr-2'>");
                sb.Append(htmlHelper.DropDownList(propertyId + "_City", list.Zip_City_list, new { @class = "form-normal", onchange = "do" + EXPname + "TownChange()" }).ToHtmlString());
                Hashtable zc = new Hashtable();
                zc["CityCode"] = "";
                sb.Append(htmlHelper.DropDownList(propertyId + "_Town", list.Zip_Town_list(zc), new { @class = "form-normal", onchange = "do" + EXPname + "RoadChange()" }).ToHtmlString());
                Hashtable zt = new Hashtable();
                zt["TownCode"] = "";
                sb.Append(htmlHelper.DropDownList(propertyId + "_Road", list.Zip_Road_list(zt), new { @class = "form-normal", onchange = "do" + EXPname + "ZipChange()" }).ToHtmlString());
                sb.Append("</span>");
            }
            else
            {
                sb.Append("<span class='input-group-btn mr-2'>");
                sb.Append(htmlHelper.DropDownList(propertyId + "_City", list.Zip_City_list, new { @class = "form-normal", onchange = "do" + EXPname + "TownChange()", disabled = "disabled" }).ToHtmlString());
                Hashtable zc = new Hashtable();
                zc["CityCode"] = "";
                sb.Append(htmlHelper.DropDownList(propertyId + "_Town", list.Zip_Town_list(zc), new { @class = "form-normal", onchange = "do" + EXPname + "RoadChange()", disabled = "disabled" }).ToHtmlString());
                Hashtable zt = new Hashtable();
                zt["TownCode"] = "";
                sb.Append(htmlHelper.DropDownList(propertyId + "_Road", list.Zip_Road_list(zt), new { @class = "form-normal", onchange = "do" + EXPname + "ZipChange()", disabled = "disabled" }).ToHtmlString());
                sb.Append("</span>");
            }
            sb.Append(htmlHelper.TextBoxFor(expressionTEXT, new { size = "40", @class = "form-normal", placeholder = "地址詳細資料" }, IsReadOnly).ToHtmlString());
            sb.Append("</div>");

            // Script標籤
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            StringBuilder sb_script = new StringBuilder();
            sb_script.Append("<script type='text/javascript'>");
            sb_script.Append("$(document).ready(function () {");
            sb_script.Append("$('#" + propertyId + "').on('blur', do" + EXPname + "NameGet);");
            sb_script.Append("do" + EXPname + "NameGet();");
            sb_script.Append("});");

            //顯示行政區代碼(ZIP_CO)資料選取對話框
            sb_script.Append("function do" + EXPname + "TownChange(townCode) {");
            sb_script.Append("var _zc = $('#" + propertyId + "_City');");
            sb_script.Append("var _zt = $('#" + propertyId + "_Town');");
            sb_script.Append("var _zr = $('#" + propertyId + "_Road');");
            sb_script.Append("var url = '" + url.Action("GetTownList", "Ajax", new { area = "" }) + "';");
            sb_script.Append("var data = { 'CityCode': _zc.val() };");
            sb_script.Append("ajaxLoadMore(url, data, function(resp) {");
            sb_script.Append("_zt.empty();");
            sb_script.Append("_zt.append('<option selected>請選擇</option>');");
            sb_script.Append("_zr.empty();");
            sb_script.Append("_zr.append('<option selected>請選擇</option>');");

            sb_script.Append("$.each(resp.data, function (i, item) {");
            sb_script.Append("if (resp.data != '') {");
            sb_script.Append("if (townCode == item.CODE) {");
            sb_script.Append("_zt.append('<option value=' + item.CODE + ' selected>' + item.TEXT + '</option>');");
            sb_script.Append("}");
            sb_script.Append("else {");
            sb_script.Append("_zt.append('<option value=' + item.CODE + '>' + item.TEXT + '</option>');");
            sb_script.Append("}");

            sb_script.Append("}");
            sb_script.Append("});");

            sb_script.Append("});");
            sb_script.Append("}");

            //顯示行政區代碼(ZIP_CO)資料選取對話框
            sb_script.Append("function do" + EXPname + "RoadChange(roadCode) {");
            sb_script.Append("var _zt = $('#" + propertyId + "_Town');");
            sb_script.Append("var _zr = $('#" + propertyId + "_Road');");
            sb_script.Append("var url = '" + url.Action("GetRoadList", "Ajax", new { area = "" }) + "';");
            sb_script.Append("var data = { 'TownCode': _zt.val() };");
            sb_script.Append("ajaxLoadMore(url, data, function(resp) {");
            sb_script.Append("_zr.empty();");
            sb_script.Append("_zr.append('<option selected>請選擇</option>');");

            sb_script.Append("$.each(resp.data, function (i, item) {");
            sb_script.Append("if (resp.data != '') {");
            sb_script.Append("if (roadCode == item.CODE) {");
            sb_script.Append("_zr.append('<option value=' + item.CODE + ' selected>' + item.TEXT + '</option>');");
            sb_script.Append("}");
            sb_script.Append("else {");
            sb_script.Append("_zr.append('<option value=' + item.CODE + '>' + item.TEXT + '</option>');");
            sb_script.Append("}");
            
            sb_script.Append("}");
            sb_script.Append("});");

            sb_script.Append("});");
            sb_script.Append("}");
            
            //顯示行政區代碼(ZIP_CO)資料選取對話框
            sb_script.Append("function do" + EXPname + "ZipChange() {");
            sb_script.Append("var _zr = $('#" + propertyId + "_Road');");
            sb_script.Append("var _zo = $('#" + propertyId + "');");
            sb_script.Append("_zo.val(_zr.val().substr(0,3));");
            sb_script.Append("}");
            
            //取得行政區代碼對應的規格名稱。
            sb_script.Append("function do" + EXPname + "NameGet() {");
            sb_script.Append("var code = $('#" + propertyId + "');");
            sb_script.Append("var _zc = $('#" + propertyId + "_City');");
            sb_script.Append("var _zt = $('#" + propertyId + "_Town');");
            sb_script.Append("var _zr = $('#" + propertyId + "_Road');");
            sb_script.Append("var data = { 'url': '" + url.Action("GetZipCo", "Ajax", new { area = "" }) + "',");
            sb_script.Append("'arg': { 'CODE': code.val() },");
            sb_script.Append("'msg': '查無該郵遞區號詳細資料!!',};");

            sb_script.Append("if ($.trim(data.arg.CODE) == '') _zc.val('');");
            sb_script.Append("else {");

            sb_script.Append("ajaxLoadMore(data.url, data.arg, function(resp) {");
            sb_script.Append("if (resp.data != '') {");
            sb_script.Append("console.log(resp.data.CITYCODE);");
            sb_script.Append("_zc.val(resp.data.CITYCODE);");
            sb_script.Append("do" + EXPname + "TownChange(resp.data.TOWNCODE);");
            sb_script.Append("console.log(resp.data.TOWNCODE);");
            //sb_script.Append("setTimeout(_zt.val(resp.data.TOWNCODE), 1000);");
            //sb_script.Append("_zt.val(resp.data.TOWNCODE);");
            //sb_script.Append("do" + EXPname + "RoadChange(resp.data.ZIPCO);");
            sb_script.Append("setTimeout(function(){do" + EXPname + "RoadChange(resp.data.ZIPCO)}, 1000);");
            sb_script.Append("console.log(resp.data.ZIPCO);");
            sb_script.Append("code.val(resp.data.ZIPCO);");
            //sb_script.Append("_zr.val(resp.data.ZIPCO);");
            sb_script.Append("}");
            sb_script.Append("});");

            sb_script.Append("}");
            sb_script.Append("};");
            sb_script.Append("</script>");

            // 組成完整標籤
            sb.Append(sb_script);

            return MvcHtmlString.Create(sb.ToString());
        }


        /// <summary>HTML (三碼)行政區單選輸入框產生方法(包含script)。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="expressionTEXT">TEXT(名稱)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString ZIP_CO_ThreeDropForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression,
            Expression<Func<TModel, object>> expressionTEXT,
            bool IsReadOnly = false,
            object htmlAttributes = null, bool enabled = true)
        {
            ShareCodeListModel list = new ShareCodeListModel();

            //HTML 標籤的 id 與 name 屬性值
            var name = ExpressionHelper.GetExpressionText(expression);
            var EXPname = name;
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value = Convert.ToString(metadata.Model).ToLower();

            var propertyName = templateInfo.GetFullHtmlFieldName(name);
            var propertyId = templateInfo.GetFullHtmlFieldId(propertyName);

            //HTML 標籤的 id 與 name 屬性值
            var name_text = ExpressionHelper.GetExpressionText(expressionTEXT);
            var metadata_text = ModelMetadata.FromLambdaExpression(expressionTEXT, htmlHelper.ViewData);
            var templateInfo_text = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value_text = Convert.ToString(metadata.Model).ToLower();

            var propertyName_text = templateInfo.GetFullHtmlFieldName(name_text);
            var propertyId_text = templateInfo.GetFullHtmlFieldId(propertyName_text);

            // HTML標籤
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n<div class='input-group'>");
            sb.Append(htmlHelper.TextBoxFor(expression, new { @class = "form-normal", size = 10, maxlength = 5, placeholder = "郵遞區號" }, IsReadOnly).ToHtmlString());

            if (!IsReadOnly)
            {
                sb.Append("<span class=' mr-2'>");
                sb.Append(htmlHelper.DropDownList(propertyId + "_City", list.Zip_City_list, new { @class = "form-normal", onchange = "do" + EXPname + "TownChange()" }).ToHtmlString());
                Hashtable zc = new Hashtable();
                zc["CityCode"] = "";
                sb.Append(htmlHelper.DropDownList(propertyId + "_Town", list.Zip_Town_list(zc), new { @class = "form-normal", onchange = "do" + EXPname + "ZipChange()" }).ToHtmlString());
                //Hashtable zt = new Hashtable();
                //zt["TownCode"] = "";
                //sb.Append(htmlHelper.DropDownList(propertyId + "_Town", list.Zip_Town_list(zc), new { @class = "form-normal", onchange = "do" + EXPname + "RoadChange()" }).ToHtmlString());
                //Hashtable zt = new Hashtable();
                //zt["TownCode"] = "";
                //sb.Append(htmlHelper.DropDownList(propertyId + "_Road", list.Zip_Road_list(zt), new { @class = "form-normal", onchange = "do" + EXPname + "ZipChange()" }).ToHtmlString());
                sb.Append("</span>");
            }
            else
            {
                sb.Append("<span class='input-group-btn mr-2'>");
                sb.Append(htmlHelper.DropDownList(propertyId + "_City", list.Zip_City_list, new { @class = "form-normal", onchange = "do" + EXPname + "TownChange()", disabled = "disabled" }).ToHtmlString());
                Hashtable zc = new Hashtable();
                zc["CityCode"] = "";
                sb.Append(htmlHelper.DropDownList(propertyId + "_Town", list.Zip_Town_list(zc), new { @class = "form-normal", onchange = "do" + EXPname + "ZipChange()", disabled = "disabled" }).ToHtmlString());
                //Hashtable zt = new Hashtable();
                //zt["TownCode"] = "";
                //sb.Append(htmlHelper.DropDownList(propertyId + "_Town", list.Zip_Town_list(zc), new { @class = "form-normal", onchange = "do" + EXPname + "RoadChange()", disabled = "disabled" }).ToHtmlString());
                //Hashtable zt = new Hashtable();
                //zt["TownCode"] = "";
                //sb.Append(htmlHelper.DropDownList(propertyId + "_Road", list.Zip_Road_list(zt), new { @class = "form-normal", onchange = "do" + EXPname + "ZipChange()", disabled = "disabled" }).ToHtmlString());
                sb.Append("</span>");
            }
            sb.Append(htmlHelper.TextBoxFor(expressionTEXT, new { size = "40", @class = "form-normal", placeholder = "地址詳細資料" }, IsReadOnly).ToHtmlString());
            sb.Append("</div>");

            // Script標籤
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            StringBuilder sb_script = new StringBuilder();
            sb_script.Append("<script type='text/javascript'>");
            sb_script.Append("$(document).ready(function () {");
            sb_script.Append("$('#" + propertyId + "').on('blur', do" + EXPname + "NameGet);");
            sb_script.Append("do" + EXPname + "NameGet();");
            sb_script.Append("});");

            //顯示行政區代碼(ZIP_CO)區域資料下拉選單
            sb_script.Append("function do" + EXPname + "TownChange(townCode) {");
            sb_script.Append("var _zc = $('#" + propertyId + "_City');");
            sb_script.Append("var _zt = $('#" + propertyId + "_Town');");
            //sb_script.Append("var _zr = $('#" + propertyId + "_Road');");
            sb_script.Append("var url = '" + url.Action("GetTownList", "Ajax", new { area = "" }) + "';");
            sb_script.Append("var data = { 'CityCode': _zc.val() };");
            sb_script.Append("ajaxLoadMore(url, data, function(resp) {");
            sb_script.Append("_zt.empty();");
            sb_script.Append("_zt.append('<option selected>請選擇</option>');");
            //sb_script.Append("_zr.empty();");
            //sb_script.Append("_zr.append('<option selected>請選擇</option>');");

            sb_script.Append("$.each(resp.data, function (i, item) {");
            sb_script.Append("if (resp.data != '') {");
            sb_script.Append("if (townCode == item.CODE) {");
            sb_script.Append("_zt.append('<option value=' + item.CODE + ' selected>' + item.TEXT + '</option>');");
            sb_script.Append("}");
            sb_script.Append("else {");
            sb_script.Append("_zt.append('<option value=' + item.CODE + '>' + item.TEXT + '</option>');");
            sb_script.Append("}");

            sb_script.Append("}");
            sb_script.Append("});");

            sb_script.Append("});");
            sb_script.Append("}");

            //顯示行政區代碼(ZIP_CO)資料路名下拉選單
            //sb_script.Append("function do" + EXPname + "RoadChange(roadCode) {");
            //sb_script.Append("var _zt = $('#" + propertyId + "_Town');");
            ////sb_script.Append("var _zr = $('#" + propertyId + "_Road');");
            //sb_script.Append("var url = '" + url.Action("GetRoadList", "Ajax", new { area = "" }) + "';");
            //sb_script.Append("var data = { 'TownCode': _zt.val() };");
            //sb_script.Append("ajaxLoadMore(url, data, function(resp) {");
            ////sb_script.Append("_zr.empty();");
            ////sb_script.Append("_zr.append('<option selected>請選擇</option>');");

            //sb_script.Append("$.each(resp.data, function (i, item) {");
            //sb_script.Append("if (resp.data != '') {");
            //sb_script.Append("if (roadCode == item.CODE) {");
            ////sb_script.Append("_zr.append('<option value=' + item.CODE + ' selected>' + item.TEXT + '</option>');");
            //sb_script.Append("}");
            //sb_script.Append("else {");
            ////sb_script.Append("_zr.append('<option value=' + item.CODE + '>' + item.TEXT + '</option>');");
            //sb_script.Append("}");

            //sb_script.Append("}");
            //sb_script.Append("});");

            //sb_script.Append("});");
            //sb_script.Append("}");

            ////顯示行政區代碼(ZIP_CO)資料選取對話框
            //sb_script.Append("function do" + EXPname + "ZipChange() {");
            //sb_script.Append("var _zr = $('#" + propertyId + "_Road');");
            //sb_script.Append("var _zo = $('#" + propertyId + "');");
            //sb_script.Append("_zo.val(_zr.val());");
            //sb_script.Append("}");

            //顯示行政區代碼(ZIP_CO)資料選取對話框
            sb_script.Append("function do" + EXPname + "ZipChange() {");
            sb_script.Append("var _zt = $('#" + propertyId + "_Town');");
            sb_script.Append("var _zo = $('#" + propertyId + "');");
            sb_script.Append("_zo.val(_zt.val().substr(0,3));");
            sb_script.Append("}");

            //取得行政區代碼對應的規格名稱。
            sb_script.Append("function do" + EXPname + "NameGet() {");
            sb_script.Append("var code = $('#" + propertyId + "');");
            sb_script.Append("var _zc = $('#" + propertyId + "_City');");
            sb_script.Append("var _zt = $('#" + propertyId + "_Town');");
            //sb_script.Append("var _zr = $('#" + propertyId + "_Road');");
            sb_script.Append("var data = { 'url': '" + url.Action("GetZipCo_Three", "Ajax", new { area = "" }) + "',");
            sb_script.Append("'arg': { 'CODE': code.val() },");
            sb_script.Append("'msg': '查無該郵遞區號詳細資料!!',};");

            sb_script.Append("if ($.trim(data.arg.CODE) == '') _zc.val('');");
            sb_script.Append("else {");

            sb_script.Append("ajaxLoadMore(data.url, data.arg, function(resp) {");
            sb_script.Append("if (resp.data != '') {");
            sb_script.Append("console.log(resp.data.CITYCODE);");
            sb_script.Append("_zc.val(resp.data.CITYCODE);");
            sb_script.Append("do" + EXPname + "TownChange(resp.data.TOWNCODE);");
            sb_script.Append("console.log(resp.data.TOWNCODE);");
            //sb_script.Append("setTimeout(_zt.val(resp.data.TOWNCODE), 1000);");
            //sb_script.Append("_zt.val(resp.data.TOWNCODE);");
            //sb_script.Append("do" + EXPname + "RoadChange(resp.data.ZIPCO);");
            //sb_script.Append("setTimeout(function(){do" + EXPname + "RoadChange(resp.data.ZIPCO)}, 1000);");
            sb_script.Append("console.log(resp.data.ZIPCO);");
            sb_script.Append("code.val(resp.data.ZIPCO);");
            //sb_script.Append("_zr.val(resp.data.ZIPCO);");
            sb_script.Append("}");
            sb_script.Append("});");

            sb_script.Append("}");
            sb_script.Append("};");
            sb_script.Append("</script>");

            // 組成完整標籤
            sb.Append(sb_script);

            return MvcHtmlString.Create(sb.ToString());
        }


        /// <summary>HTML (三碼)行政區單選輸入框產生方法(包含script)，詳細資料細拆為─道路街、巷弄號。</summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper">HTML 輔助處理物件。</param>
        /// <param name="expression">(代號)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="expressionTEXT1">TEXT1(名稱)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="expressionTEXT2">TEXT2(名稱)Model 欄位的 Lambda 表達式物件。</param>
        /// <param name="htmlAttributes">要輸出的 HTML 屬性值集合。輸入 null 表示不需要。</param>
        /// <param name="enabled">是否啟用勾選框。（true: 啟用，false: 禁用）。預設 true。本參數僅適用在當 Model 欄位值不是布林值時，須自行寫程式來判斷 true 或 false 結果。</param>
        public static MvcHtmlString ZIP_CO_ThreeDropForTurbo2<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, object>> expression,
            Expression<Func<TModel, object>> expressionTEXT1,
            Expression<Func<TModel, object>> expressionTEXT2,
            bool IsReadOnly = false,
            object htmlAttributes = null, bool enabled = true)
        {
            ShareCodeListModel list = new ShareCodeListModel();

            //HTML 標籤的 id 與 name 屬性值
            var name = ExpressionHelper.GetExpressionText(expression);
            var EXPname = name;
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var templateInfo = htmlHelper.ViewContext.ViewData.TemplateInfo;
            var value = Convert.ToString(metadata.Model).ToLower();

            var propertyName = templateInfo.GetFullHtmlFieldName(name);
            var propertyId = templateInfo.GetFullHtmlFieldId(propertyName);

            //HTML 標籤的 id 與 name 屬性值
            var name_text1 = ExpressionHelper.GetExpressionText(expressionTEXT1);
            var metadata_text1 = ModelMetadata.FromLambdaExpression(expressionTEXT1, htmlHelper.ViewData);
            var propertyName_text1 = templateInfo.GetFullHtmlFieldName(name_text1);
            var propertyId_text1 = templateInfo.GetFullHtmlFieldId(propertyName_text1);

            var name_text2 = ExpressionHelper.GetExpressionText(expressionTEXT2);
            var metadata_text2 = ModelMetadata.FromLambdaExpression(expressionTEXT2, htmlHelper.ViewData);
            var propertyName_text2 = templateInfo.GetFullHtmlFieldName(name_text2);
            var propertyId_text2 = templateInfo.GetFullHtmlFieldId(propertyName_text2);

            //var templateInfo_text = htmlHelper.ViewContext.ViewData.TemplateInfo;
            //var value_text = Convert.ToString(metadata.Model).ToLower();
            
            // HTML標籤
            StringBuilder sb = new StringBuilder();
            sb.Append("\r\n<div class='input-group'>");
            sb.Append(htmlHelper.TextBoxFor(expression, new { @class = "form-normal", size = 10, maxlength = 5, placeholder = "郵遞區號" }, IsReadOnly).ToHtmlString());

            if (!IsReadOnly)
            {
                sb.Append("<span class=' mr-2'>");
                sb.Append(htmlHelper.DropDownList(propertyId + "_City", list.Zip_City_list, new { @class = "form-normal", onchange = "do" + EXPname + "TownChange()" }).ToHtmlString());
                Hashtable zc = new Hashtable();
                zc["CityCode"] = "";
                sb.Append(htmlHelper.DropDownList(propertyId + "_Town", list.Zip_Town_list(zc), new { @class = "form-normal", onchange = "do" + EXPname + "ZipChange()" }).ToHtmlString());
                sb.Append("</span>");
            }
            else
            {
                sb.Append("<span class='input-group-btn mr-2'>");
                sb.Append(htmlHelper.DropDownList(propertyId + "_City", list.Zip_City_list, new { @class = "form-normal", onchange = "do" + EXPname + "TownChange()", disabled = "disabled" }).ToHtmlString());
                Hashtable zc = new Hashtable();
                zc["CityCode"] = "";
                sb.Append(htmlHelper.DropDownList(propertyId + "_Town", list.Zip_Town_list(zc), new { @class = "form-normal", onchange = "do" + EXPname + "ZipChange()", disabled = "disabled" }).ToHtmlString());
                sb.Append("</span>");
            }
            sb.Append(htmlHelper.TextBoxFor(expressionTEXT1, new { size = "20", @class = "form-normal", placeholder = "地址道路街" }, IsReadOnly).ToHtmlString());
            sb.Append(htmlHelper.TextBoxFor(expressionTEXT2, new { size = "20", @class = "form-normal", placeholder = "地址巷弄號" }, IsReadOnly).ToHtmlString());
            sb.Append("</div>");

            // Script標籤
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            StringBuilder sb_script = new StringBuilder();
            sb_script.Append("<script type='text/javascript'>");
            sb_script.Append("$(document).ready(function () {");
            sb_script.Append("$('#" + propertyId + "').on('blur', do" + EXPname + "NameGet);");
            sb_script.Append("do" + EXPname + "NameGet();");
            sb_script.Append("});");

            //顯示行政區代碼(ZIP_CO)區域資料下拉選單
            sb_script.Append("function do" + EXPname + "TownChange(townCode) {");
            sb_script.Append("var _zc = $('#" + propertyId + "_City');");
            sb_script.Append("var _zt = $('#" + propertyId + "_Town');");
            sb_script.Append("var url = '" + url.Action("GetTownList", "Ajax", new { area = "" }) + "';");
            sb_script.Append("var data = { 'CityCode': _zc.val() };");
            sb_script.Append("ajaxLoadMore(url, data, function(resp) {");
            sb_script.Append("_zt.empty();");
            sb_script.Append("_zt.append('<option selected>請選擇</option>');");

            sb_script.Append("$.each(resp.data, function (i, item) {");
            sb_script.Append("if (resp.data != '') {");
            sb_script.Append("if (townCode == item.CODE) {");
            sb_script.Append("_zt.append('<option value=' + item.CODE + ' selected>' + item.TEXT + '</option>');");
            sb_script.Append("}");
            sb_script.Append("else {");
            sb_script.Append("_zt.append('<option value=' + item.CODE + '>' + item.TEXT + '</option>');");
            sb_script.Append("}");

            sb_script.Append("}");
            sb_script.Append("});");

            sb_script.Append("});");
            sb_script.Append("}");
            
            //顯示行政區代碼(ZIP_CO)資料選取對話框
            sb_script.Append("function do" + EXPname + "ZipChange() {");
            sb_script.Append("var _zt = $('#" + propertyId + "_Town');");
            sb_script.Append("var _zo = $('#" + propertyId + "');");
            sb_script.Append("_zo.val(_zt.val().substr(0,3));");
            sb_script.Append("}");

            //取得行政區代碼對應的規格名稱。
            sb_script.Append("function do" + EXPname + "NameGet() {");
            sb_script.Append("var code = $('#" + propertyId + "');");
            sb_script.Append("var _zc = $('#" + propertyId + "_City');");
            sb_script.Append("var _zt = $('#" + propertyId + "_Town');");
            sb_script.Append("var data = { 'url': '" + url.Action("GetZipCo_Three", "Ajax", new { area = "" }) + "',");
            sb_script.Append("'arg': { 'CODE': code.val() },");
            sb_script.Append("'msg': '查無該郵遞區號詳細資料!!',};");

            sb_script.Append("if ($.trim(data.arg.CODE) == '') _zc.val('');");
            sb_script.Append("else {");

            sb_script.Append("ajaxLoadMore(data.url, data.arg, function(resp) {");
            sb_script.Append("if (resp.data != '') {");
            sb_script.Append("console.log(resp.data.CITYCODE);");
            sb_script.Append("_zc.val(resp.data.CITYCODE);");
            sb_script.Append("do" + EXPname + "TownChange(resp.data.TOWNCODE);");
            sb_script.Append("console.log(resp.data.TOWNCODE);");
            sb_script.Append("console.log(resp.data.ZIPCO);");
            sb_script.Append("code.val(resp.data.ZIPCO);");
            sb_script.Append("}");
            sb_script.Append("});");

            sb_script.Append("}");
            sb_script.Append("};");
            sb_script.Append("</script>");

            // 組成完整標籤
            sb.Append(sb_script);

            return MvcHtmlString.Create(sb.ToString());
        }
    }
}