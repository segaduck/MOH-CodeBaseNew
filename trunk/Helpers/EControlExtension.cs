using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.ModelBinding;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using EECOnline.Commons;
using EECOnline.Helpers;
using EECOnline.Services;
using Turbo.Commons;

namespace Turbo.Helpers
{
    /// <summary>
    /// HTML 控項 產生輔助方法類別(快速產生)
    /// </summary>
    public static class EControlExtension
    {
        /// <summary>
        /// HTML 控項，節儉版面及快速產製使用，將需CustomLabelFor與其他共項元件產生部分一行解決
        /// </summary>
        /// <param name="expression">Model 欄位的 Lambda 表達式物件</param>
        /// <param name="Control">其他共項元件</param>
        /// <returns></returns>
        public static MvcHtmlString EControlForTurbo<TModel>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, string>> expression, MvcHtmlString Control)
        {
            // HTML標籤
            StringBuilder sb = new StringBuilder();
            sb.Append("<div class='form-group'>");
            sb.Append(htmlHelper.CustomLabelFor(expression, new { @class = "main-label col-sm-2" }));
            sb.Append("<div class='col-sm-10 form-inline'>");
            if (Control != null) sb.Append(Control);
            sb.Append("</div>");
            sb.Append("</div>");

            return MvcHtmlString.Create(sb.ToString());
        }

        /// <summary>
        /// HTML 控項，節儉版面及快速產製使用，將需CustomLabelFor與其他共項元件產生部分一行解決
        /// </summary>
        /// <param name="expression">Model 欄位的 Lambda 表達式物件</param>
        /// <param name="Control">其他共項元件</param>
        /// <param name="IsNew">明細專用，需配合Attr.IsNew</param>
        /// <param name="Model_tag">明細專用，若有子類別，需將子類別名稱鍵入，便可達到二階層</param>
        /// <returns></returns>
        public static MvcHtmlString EControlForTurbo<TModel, T>(this HtmlHelper<TModel> htmlHelper, T Model, bool IsNew = true)
        {
            StringBuilder sb = new StringBuilder();
            // 設定ControlAttribute清單
            IList<EECOnline.Commons.ControlAttribute> ca = new List<EECOnline.Commons.ControlAttribute>();
            // 搜尋該ViewModel屬性(畫面Model請選擇第二層Model(FormModel,DetailModel等...)為主)
            foreach (var pi in Model.GetType().GetProperties())
            {
                // 取得屬性自定義控件狀態
                var attr = pi.GetCustomAttribute<EECOnline.Commons.ControlAttribute>();
                if (attr != null)
                {
                    attr.pi = pi;
                    ca.Add(attr);
                }
            }

            // 算出最大的group數目，讓其餘未有加group屬性保證為單一不重複
            int max_group_value = ca.Select(m => m.group).Max();
            foreach (var c in ca)
            {
                ++max_group_value;
                if (c.group == 0)
                { c.group = max_group_value; }
            }

            // 依據group分類
            var Ca_Block_Group = ca.OrderBy(m => m.block_toggle_group).GroupBy(m => m.block_toggle_group).Select(m => m.ToList()).ToList();

            foreach (var ca_Group in Ca_Block_Group)
            {
                // 依據block_group分類
                var Ca_Block_in_Group = ca_Group.GroupBy(m => m.block_group).Select(m => m.ToList()).ToList();

                foreach (var ca_Group_in in Ca_Block_in_Group)
                {
                    // 依據group分類
                    var Ca_Group = ca_Group_in.GroupBy(m => m.group).Select(m => m.ToList()).ToList();

                    foreach (var Ca in Ca_Group)
                    {
                        // label 
                        var col_sm = "col-sm-4";

                        // 分隔線
                        var hr = Ca.Where(m => m.hr != null).ToList();
                        var hr_Id = hr.ToCount() > 0 ? hr.FirstOrDefault().hr : "";
                        if (hr.ToCount() > 0) sb.Append("<hr class='" + hr_Id + "'>");

                        // div ID
                        var Ca_form_id = Ca.Where(m => m.form_id.TONotNullString() != "").ToList();
                        var form_Id = Ca_form_id.ToCount() > 0 ? Ca_form_id.FirstOrDefault().form_id : "";

                        // HIDDEN的情況下不要有欄位顯示
                        var hd = Ca.Where(m => m.Mode == Control.Hidden).ToList();

                        // 顯示執業縣市
                        var h5 = Ca.Where(m => m.h5 != null).ToList();
                        var h5_Id = h5.ToCount() > 0 ? hr.FirstOrDefault().h5 : "";

                        if (h5.ToCount() > 0) sb.Append("<h5 id=\"h5_id\">" + h5_Id + " </h5>");

                        if (hd.ToCount() > 0) sb.Append("<div style='display:none'>");
                        else sb.Append("<div class='form-row form-group' id='" + form_Id + "'>");

                        foreach (var attr in Ca)
                        {
                            // 生成控制項

                            // Detail視窗專用
                            // 關閉控項狀態(平常控項使用)
                            var IsReadOnly = attr.IsReadOnly ? attr.IsReadOnly : (attr.IsOpenNew && !IsNew);
                            // 關閉控項狀態(特殊控項使用)
                            //var DisabledString = "pointer-events:none;;background:#DDDDDD";
                            var DisabledString = "pointer-events:none;";
                            var IsDisabled = attr.IsReadOnly ? DisabledString : (attr.IsOpenNew && !IsNew) ? DisabledString : "";

                            // property轉換成expression
                            var property = attr.pi;
                            var target = Expression.Parameter(typeof(TModel));
                            var getPropertyValue = Expression.Property(target, property);
                            if (property.PropertyType.Name == "Boolean")
                            {

                                var expression = Expression.Lambda<Func<TModel, Boolean>>(getPropertyValue, target);

                                // 建立 Hidden 控件
                                if (attr.Mode == Control.Hidden)
                                {
                                    sb.Append(htmlHelper.HiddenFor(expression));
                                }
                            }
                            // 建立 CheckBoxList控件
                            else if (attr.Mode == Control.CheckBoxList)
                            {
                                var expression = Expression.Lambda<Func<TModel, IList<object>>>(getPropertyValue, target);
                                var plist = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_list").FirstOrDefault();
                                var value = plist.GetValue(Model);
                                var safeValue = (IList<CheckBoxListItem>)value;
                                sb.Append("<div class='col-sm-2'>");
                                sb.Append(htmlHelper.CustomLabelFor(expression, new { @class = "form-label" }));
                                sb.Append("</div>");
                                sb.Append("<div class='" + col_sm + " form-inline'>");
                                sb.Append(htmlHelper.CheckBoxListFor(expression, safeValue, new { style = IsDisabled }));
                                sb.Append("</div>");
                            }
                            // 建立 FILEGRID控件
                            else if (attr.Mode == Control.EFILE)
                            {
                                var expression = Expression.Lambda<Func<TModel, object>>(getPropertyValue, target);

                                sb.Append(htmlHelper.EditorFor(expression));
                            }
                            else
                            {
                                var expression = Expression.Lambda<Func<TModel, object>>(getPropertyValue, target);

                                // 建立 Hidden 控件
                                if (attr.Mode == Control.Hidden)
                                {
                                    sb.Append(htmlHelper.HiddenFor(expression));
                                }
                                // 建立 PassWord控件
                                else if (attr.Mode == Control.PassWord)
                                {
                                    if (!IsReadOnly)
                                    {
                                        var piText = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_REPEAT").FirstOrDefault();
                                        // property轉換成expression
                                        var getPropertyValue_REPEAT = Expression.Property(target, piText);
                                        var expressionREPEAT = Expression.Lambda<Func<TModel, object>>(getPropertyValue_REPEAT, target);

                                        // 第一次輸入
                                        sb.Append(htmlHelper.CustomLabelFor(expression, new { @class = "col-sm-2 control-label label-set" }));
                                        sb.Append("<div class='col-sm-4 form-inline'>");
                                        sb.Append(htmlHelper.PasswordFor(expression, new { @class = "form-control", size = attr.size, maxlength = attr.maxlength, placeholder = attr.placeholder }));
                                        sb.Append("</div>");
                                        // 第二次輸入
                                        sb.Append(htmlHelper.CustomLabelFor(expressionREPEAT, new { @class = "col-sm-2 control-label label-set" }));
                                        sb.Append("<div class='col-sm-4 form-inline'>");
                                        sb.Append(htmlHelper.PasswordFor(expressionREPEAT, new { @class = "form-control", size = attr.size, maxlength = attr.maxlength, placeholder = attr.placeholder }));
                                        sb.Append("<span style='color:red;display:none' id='PwdError'></span>");
                                        sb.Append("</div>");

                                        // JS區塊
                                        var Id1 = attr.pi.Name;
                                        var Id2 = attr.pi.Name + "_REPEAT";
                                        sb.Append("<script type='text/javascript'>");
                                        sb.Append("$(document).ready(function () {");
                                        sb.Append("var Id1 = $('#" + Id1 + "');");
                                        sb.Append("var Id2 = $('#" + Id2 + "');");
                                        sb.Append("var PwdError = $('#PwdError');");

                                        sb.Append("Id1.on('change', function(){");
                                        sb.Append("PwdError.removeAttr('style');");


                                        // 驗證特殊字元
                                        //sb.Append("var checkPwd = /^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*\\W).{8,12}$/ ;");
                                        sb.Append("if(/^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*\\W).{8,16}$/.test(Id1.val())){");
                                        sb.Append("PwdError.attr('style','color:green;');");
                                        sb.Append("PwdError.text('');");
                                        sb.Append("$('button').removeAttr('disabled');");

                                        // 驗證是否相同
                                        sb.Append("if(Id1.val()==Id2.val()){");
                                        sb.Append("PwdError.attr('style','color:green;');");
                                        sb.Append("PwdError.text('密碼相同');");
                                        sb.Append("$('button').removeAttr('disabled');");
                                        sb.Append("}");
                                        sb.Append("else{");
                                        sb.Append("PwdError.attr('style','color:red;');");
                                        sb.Append("PwdError.text('密碼不相同，請檢查');");
                                        sb.Append("$('button').attr('disabled','disabled');");
                                        sb.Append("};");

                                        sb.Append("}");
                                        sb.Append("else{");
                                        sb.Append("PwdError.attr('style','color:red;');");
                                        sb.Append("PwdError.text('密碼必為8-16碼，至少有1個數字、1個英字大寫、1個特殊符號');");
                                        sb.Append("$('button').attr('disabled','disabled');");
                                        sb.Append("};");
                                        sb.Append("});");

                                        sb.Append("Id2.on('change', function(){");
                                        sb.Append("PwdError.removeAttr('style');");



                                        // 驗證特殊字元
                                        //sb.Append("var checkPwd = /^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*\\W).{8,16}$/;");
                                        sb.Append("if(/^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*\\W).{8,16}$/.test(Id1.val())){");
                                        sb.Append("PwdError.attr('style','color:green;');");
                                        sb.Append("PwdError.text('');");
                                        sb.Append("$('button').removeAttr('disabled');");

                                        // 驗證是否相同
                                        sb.Append("if(Id1.val()==Id2.val()){");
                                        sb.Append("PwdError.attr('style','color:green;');");
                                        sb.Append("PwdError.text('密碼相同');");
                                        sb.Append("$('button').removeAttr('disabled');");
                                        sb.Append("}");
                                        sb.Append("else{");
                                        sb.Append("PwdError.attr('style','color:red;');");
                                        sb.Append("PwdError.text('密碼不相同，請檢查');");
                                        sb.Append("$('button').attr('disabled','disabled');");
                                        sb.Append("};");

                                        sb.Append("}");
                                        sb.Append("else{");
                                        sb.Append("PwdError.attr('style','color:red;');");
                                        sb.Append("PwdError.text('密碼必為8-16碼，至少有1個數字、1個英字大寫、1個特殊符號');");
                                        sb.Append("$('button').attr('disabled','disabled');");
                                        sb.Append("};");
                                        sb.Append("});");
                                        sb.Append("});");
                                        sb.Append("</script>");
                                    }
                                }
                                else
                                {
                                    // FileUpload
                                    if (attr.Mode == Control.FileUpload)
                                    {
                                        if (attr.Required == "Y") sb.Append(htmlHelper.CustomLabelFor(expression, true, new { @class = "form-label col-sm-2" }));
                                        else sb.Append(htmlHelper.CustomLabelFor(expression, new { @class = "form-label col-sm-2" }));

                                        sb.Append("<div class='col-sm-10'>");
                                    }
                                    
                                    // IntegralList
                                    else if (attr.Mode == Control.IntegralList)
                                    {
                                        var di_style = IsReadOnly ? "style='display:none'" : "";

                                        sb.Append("<div class='col-sm-3' " + di_style + ">");
                                        sb.Append(htmlHelper.CustomLabelFor(expression, new { @class = "form-label" }));
                                        sb.Append("</div>");
                                        sb.Append("<div class='col-sm-9' " + di_style + ">");
                                    }
                                    // ZIP_CO
                                    else if (attr.Mode == Control.ZIP_CO || attr.Mode == Control.ZIP_CO_FULL || attr.Mode == Control.ZIP_CO_DROP_FULL || attr.Mode == Control.ZIP_CO_DROP_THREE || attr.Mode == Control.AGENCY || attr.Mode == Control.AGENCY1)
                                    {
                                        sb.Append("<div class='col-sm-2'>");
                                        sb.Append(htmlHelper.CustomLabelFor(expression, new { @class = "form-label" }));
                                        sb.Append("</div>");
                                        sb.Append("<div class='col-sm-10'>");
                                    }
                                    // ImageHoverList
                                    else if (attr.Mode == Control.ImageHoverList)
                                    {
                                    }
                                    else
                                    {
                                        sb.Append("<div class='col-sm-2'>");
                                        sb.Append(htmlHelper.CustomLabelFor(expression, new { @class = "form-label" }));
                                        sb.Append("</div>");

                                        if (string.IsNullOrEmpty(attr.col))
                                        {
                                            sb.Append("<div class='" + col_sm + "'>");
                                        }
                                        else
                                        {
                                            sb.Append("<div class='col-sm-" + attr.col + "'>");
                                        }
                                    }
                                    // 建立 TextBox 控件
                                    if (attr.Mode == Control.TextBox) sb.Append(htmlHelper.TextBoxFor(expression, new { @class = "form-control", size = attr.size, maxlength = attr.maxlength, placeholder = attr.placeholder }, IsReadOnly));

                                    // 建立 TextArea 控件
                                    if (attr.Mode == Control.TextArea) sb.Append(htmlHelper.TextAreaFor(expression, attr.rows, attr.columns, new { @class = "form-control", maxlength = attr.maxlength, style = IsDisabled }));

                                    // 建立 DatePicker 控件
                                    if (attr.Mode == Control.DatePicker) sb.Append(htmlHelper.DatePickerTWFor(expression, new { placeholder = "yyy/mm/dd", style = IsDisabled, onblur = attr.onblur }));

                                    // 建立 DropDownList控件
                                    if (attr.Mode == Control.DropDownList)
                                    {
                                        var plist = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_list").FirstOrDefault();
                                        var value = plist.GetValue(Model);
                                        var safeValue = (IList<SelectListItem>)value;
                                        sb.Append(htmlHelper.DropDownListFor(expression, safeValue, new { @class = "form-normal", style = IsDisabled }));
                                    }

                                    // 建立 DropDownList2控件
                                    if (attr.Mode == Control.DropDownList2)
                                    {
                                        var plist = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_list").FirstOrDefault();
                                        var value = plist.GetValue(Model);
                                        var safeValue = (IList<SelectListItem>)value;
                                        sb.Append(htmlHelper.Select2ForTurbo(expression, safeValue, IsReadOnly));
                                    }

                                    // 建立 RadioGroup控件
                                    if (attr.Mode == Control.RadioGroup)
                                    {
                                        var plist = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_list").FirstOrDefault();
                                        var value = plist.GetValue(Model);
                                        var safeValue = (IList<SelectListItem>)value;
                                        sb.Append(htmlHelper.ERadioGroupFor(expression, safeValue, new { style = IsDisabled, onblur = attr.onblur }));
                                    }

                                    // 建立 Label 控件
                                    if (attr.Mode == Control.Label) sb.Append(htmlHelper.LabelForTurbo(expression));

                                    // 建立 ZIP_CO控件
                                    if (attr.Mode == Control.ZIP_CO)
                                    {
                                        var piText = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_TEXT").FirstOrDefault();
                                        // property轉換成expression
                                        var getPropertyValue_TEXT = Expression.Property(target, piText);
                                        var expressionTEXT = Expression.Lambda<Func<TModel, object>>(getPropertyValue_TEXT, target);

                                        sb.Append(htmlHelper.ZIP_COForTurbo(expression, expressionTEXT, IsReadOnly));
                                    }

                                    // 建立 ZIP_CO_FULL控件
                                    if (attr.Mode == Control.ZIP_CO_FULL)
                                    {
                                        var piText = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_TEXT").FirstOrDefault();

                                        // property轉換成expression
                                        var getPropertyValue_TEXT = Expression.Property(target, piText);
                                        var expressionTEXT = Expression.Lambda<Func<TModel, object>>(getPropertyValue_TEXT, target);

                                        var piADDR = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_ADDR").FirstOrDefault();

                                        // property轉換成expression
                                        var getPropertyValue_ADDR = Expression.Property(target, piADDR);
                                        var expressionADDR = Expression.Lambda<Func<TModel, object>>(getPropertyValue_ADDR, target);

                                        sb.Append(htmlHelper.ZIP_CO_FULLForTurbo(expression, expressionTEXT, expressionADDR, IsReadOnly));
                                    }

                                    // 建立 ZIP_CO_DROP_FULL控件
                                    if (attr.Mode == Control.ZIP_CO_DROP_FULL)
                                    {
                                        var piADDR = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_ADDR").FirstOrDefault();

                                        // property轉換成expression
                                        var getPropertyValue_ADDR = Expression.Property(target, piADDR);
                                        var expressionADDR = Expression.Lambda<Func<TModel, object>>(getPropertyValue_ADDR, target);

                                        sb.Append(htmlHelper.ZIP_CO_DropForTurbo(expression, expressionADDR, IsReadOnly));
                                    }

                                    // 建立 ZIP_CO_DROP_THREE控件
                                    if (attr.Mode == Control.ZIP_CO_DROP_THREE)
                                    {
                                        var piADDR = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_ADDR").FirstOrDefault();

                                        // property轉換成expression
                                        var getPropertyValue_ADDR = Expression.Property(target, piADDR);
                                        var expressionADDR = Expression.Lambda<Func<TModel, object>>(getPropertyValue_ADDR, target);

                                        sb.Append(htmlHelper.ZIP_CO_ThreeDropForTurbo(expression, expressionADDR, IsReadOnly));
                                    }

                                    // 建立 ZIP_CO_DROP_THREE1控件
                                    if (attr.Mode == Control.ZIP_CO_DROP_THREE2)
                                    {
                                        var piADDR = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_ADDR").FirstOrDefault();
                                        var piADDR2 = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_ADDR2").FirstOrDefault();

                                        // property轉換成expression
                                        var getPropertyValue_ADDR = Expression.Property(target, piADDR);
                                        var expressionADDR = Expression.Lambda<Func<TModel, object>>(getPropertyValue_ADDR, target);
                                        var getPropertyValue_ADDR2 = Expression.Property(target, piADDR2);
                                        var expressionADDR2 = Expression.Lambda<Func<TModel, object>>(getPropertyValue_ADDR2, target);


                                        sb.Append(htmlHelper.ZIP_CO_ThreeDropForTurbo2(expression, expressionADDR, expressionADDR2, IsReadOnly));
                                    }

                                    // 建立 FileUpload 控件
                                    if (attr.Mode == Control.FileUpload)
                                    {
                                        sb.Append(htmlHelper.FileUploadForTurbo(expression, false, false, null, Convert.ToString(attr.LimitFileType), Convert.ToString(attr.MaxFileSize), Convert.ToString(attr.UploadDesc), Convert.ToString(attr.Required)));

                                        var piCheck = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_CHECK").FirstOrDefault();
                                        // property轉換成expression
                                        var getPropertyValue_Check = Expression.Property(target, piCheck);
                                        var expressionCheck = Expression.Lambda<Func<TModel, object>>(getPropertyValue_Check, target);

                                        sb.Append("<div style=\"display:none\">");
                                        sb.Append(htmlHelper.TextBoxFor(expressionCheck, new { @name = ExpressionHelper.GetExpressionText(expressionCheck), @id = ExpressionHelper.GetExpressionText(expressionCheck) }));
                                        sb.Append("</div>");
                                    }

                                    // 建立 IntegralList 控件
                                    if (attr.Mode == Control.IntegralList)
                                    {
                                        sb.Append(htmlHelper.IntegralListForTurbo(expression, IsReadOnly));
                                    }

                                    // 建立 AGENCY控件
                                    if (attr.Mode == Control.AGENCY)
                                    {
                                        var piText = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_TEXT").FirstOrDefault();
                                        // property轉換成expression
                                        var getPropertyValue_TEXT = Expression.Property(target, piText);
                                        var expressionTEXT = Expression.Lambda<Func<TModel, object>>(getPropertyValue_TEXT, target);

                                        sb.Append(htmlHelper.AGENCYForTurbo(expression, expressionTEXT, IsReadOnly));
                                    }

                                    // 建立 AGENCY1控件
                                    if (attr.Mode == Control.AGENCY1)
                                    {
                                        var piText = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_TEXT").FirstOrDefault();
                                        // property轉換成expression
                                        var getPropertyValue_TEXT = Expression.Property(target, piText);
                                        var expressionTEXT = Expression.Lambda<Func<TModel, object>>(getPropertyValue_TEXT, target);

                                        var piHidden = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_HIDDEN").FirstOrDefault();
                                        // property轉換成expression
                                        var getPropertyValue_HIDDEN = Expression.Property(target, piHidden);
                                        var expressionHIDDEN = Expression.Lambda<Func<TModel, object>>(getPropertyValue_HIDDEN, target);

                                        sb.Append(htmlHelper.AGENCY1ForTurbo(expression, expressionTEXT, expressionHIDDEN, IsReadOnly));
                                    }

                                    // 建立 AGENCY2控件
                                    if (attr.Mode == Control.AGENCY2)
                                    {
                                        var piText = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_TEXT").FirstOrDefault();
                                        // property轉換成expression
                                        var getPropertyValue_TEXT = Expression.Property(target, piText);
                                        var expressionTEXT = Expression.Lambda<Func<TModel, object>>(getPropertyValue_TEXT, target);

                                        var piHidden = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_HIDDEN").FirstOrDefault();
                                        // property轉換成expression
                                        var getPropertyValue_HIDDEN = Expression.Property(target, piHidden);
                                        var expressionHIDDEN = Expression.Lambda<Func<TModel, object>>(getPropertyValue_HIDDEN, target);

                                        sb.Append(htmlHelper.AGENCY1ForTurbo(expression, expressionTEXT, expressionHIDDEN, IsReadOnly, true));
                                    }

                                    // 建立 ImageHover 控件
                                    if (attr.Mode == Control.ImageHover)
                                    {
                                        sb.Append(htmlHelper.ImageHoverForTurbo(expression));
                                    }

                                    // 建立 ImageHover 控件
                                    if (attr.Mode == Control.ImageHoverList)
                                    {
                                        sb.Append(htmlHelper.ImageHoverListForTurbo(expression));
                                    }

                                    if (attr.AlterMsg.TONotNullString() != "")
                                    {
                                        sb.Append("<font style='color:red'>" + attr.AlterMsg.TONotNullString() + "</font>");
                                    }

                                    if (attr.Mode != Control.ImageHoverList)
                                    {
                                        sb.Append("</div>");
                                    }
                                }
                            }
                        }
                        sb.Append("</div>");
                    }
                }

            }

            return MvcHtmlString.Create(sb.ToString());
        }
    }
}