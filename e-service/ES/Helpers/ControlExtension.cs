using ES.Commons;
using ES.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace ES.Helpers
{
    /// <summary>
    /// HTML 控項 產生輔助方法類別(快速產生)
    /// </summary>
    public static class ControlExtension
    {

        #region 前端
        /// <summary>
        /// HTML 控項，節儉版面及快速產製使用，將需CustomLabelFor與其他共項元件產生部分一行解決
        /// </summary>
        /// <param name="expression">Model 欄位的 Lambda 表達式物件</param>
        /// <param name="Control">其他共項元件</param>
        /// <param name="IsNew">明細專用，需配合Attr.IsNew</param>
        /// <param name="Model_tag">明細專用，若有子類別，需將子類別名稱鍵入，便可達到二階層</param>
        /// <returns></returns>
        public static MvcHtmlString FrontControlForTurbo<TModel, T>(this HtmlHelper<TModel> htmlHelper, T Model, bool IsNew = true)
        {
            // HTML標籤

            StringBuilder sb = new StringBuilder();
            // 設定ControlAttribute清單
            IList<ES.Commons.ControlAttribute> ca = new List<ES.Commons.ControlAttribute>();
            // 搜尋該ViewModel屬性(畫面Model請選擇第二層Model(FormModel,DetailModel等...)為主)
            foreach (var pi in Model.GetType().GetProperties())
            {
                // 取得屬性自定義控件狀態
                var attr = pi.GetCustomAttribute<ES.Commons.ControlAttribute>();
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
            var Ca_Block_Group = ca.GroupBy(m => m.block_toggle_group).Select(m => m.ToList()).ToList();

            foreach (var ca_Group in Ca_Block_Group)
            {
                var Ca_blockDiv_id = ca_Group.Where(m => m.block_BIG_id.TONotNullString() != "").ToList();
                var blockIDivd = Ca_blockDiv_id.ToCount() > 0 ? Ca_blockDiv_id.FirstOrDefault().block_BIG_id : "";

                if (blockIDivd != "")
                {
                    sb.Append("<div id='" + blockIDivd + "' >");
                }

                var Ca_block_id = ca_Group.Where(m => m.block_toggle_id.TONotNullString() != "").ToList();
                var blockId = Ca_block_id.ToCount() > 0 ? Ca_block_id.FirstOrDefault().block_toggle_id : "";

                // 是否顯示區塊
                var Ca_toggle = ca_Group.Where(m => m.block_toggle).ToList();
                if (Ca_toggle.ToCount() > 0)
                {
                    sb.Append("<h1 class='form-title  title-bg-y'>" + Ca_toggle.FirstOrDefault().toggle_name + "</h1>");
                    sb.Append(" <div class='form-set'>");
                }

                // 依據block_group分類
                var Ca_Block_in_Group = ca_Group.GroupBy(m => m.block_group).Select(m => m.ToList()).ToList();

                foreach (var ca_Group_in in Ca_Block_in_Group)
                {
                    var ca_Group_in_id = ca_Group_in.Where(m => m.block_id.TONotNullString() != "").ToList();
                    var InBlockId = ca_Group_in_id.ToCount() > 0 ? ca_Group_in_id.FirstOrDefault().block_id : "";

                    sb.Append("<div id='" + InBlockId + "'>");

                    // 依據group分類
                    var Ca_Group = ca_Group_in.GroupBy(m => m.group).Select(m => m.ToList()).ToList();

                    foreach (var Ca in Ca_Group)
                    {
                        var CaCount = Ca.ToCount();
                        // 計算Css賦予(NormalController) 1:col-sm-10 / 2: col-sm-4 / else:col-sm-2 
                        var col_sm = "col-sm-2";
                        switch (CaCount)
                        {
                            case 1:
                                col_sm = "col-sm-10";
                                break;
                            case 2:
                                col_sm = "col-sm-4";
                                break;
                        }

                        var Ca_form_id = Ca.Where(m => m.form_id.TONotNullString() != "").ToList();
                        var form_Id = Ca_form_id.ToCount() > 0 ? Ca_form_id.FirstOrDefault().form_id : "";
                        try
                        {
                            if (Ca[0].Mode.TONotNullString() != "Goods") sb.Append("<div class='form-group form-inline' id='" + form_Id + "'>");
                        }
                        catch (Exception)
                        {
                            sb.Append("<div class='form-group form-inline' id='" + form_Id + "'>");
                        }

                        foreach (var attr in Ca)
                        {
                            // 生成控制項

                            // Detail視窗專用
                            // 關閉控項狀態(平常控項使用)
                            var IsReadOnly = attr.IsReadOnly ? attr.IsReadOnly : (attr.IsOpenNew && !IsNew);
                            // 關閉控項狀態(特殊控項使用)
                            var DisabledString = "pointer-events:none;background:#DDDDDD";
                            var IsDisabled = attr.IsReadOnly ? DisabledString : (attr.IsOpenNew && !IsNew) ? DisabledString : "";

                            // property轉換成expression
                            var property = attr.pi;

                            var target = Expression.Parameter(typeof(TModel));
                            var getPropertyValue = Expression.Property(target, property);
                            //if (property.PropertyType.Name == "Boolean")
                            //{

                            //    var expression = Expression.Lambda<Func<TModel, Boolean>>(getPropertyValue, target);

                            //    // 建立 Hidden 控件
                            //    if (attr.Mode == Control.Hidden)
                            //    {
                            //        sb.Append(htmlHelper.HiddenFor(expression));
                            //    }

                            //}
                            if (attr.Mode == Control.Goods)
                            {
                                var expression = Expression.Lambda<Func<TModel, object>>(getPropertyValue, target);

                                sb.Append(htmlHelper.EditorFor(expression, attr.EditorViewName));
                            }
                            // 建立 CheckBoxList控件
                            else if (attr.Mode == Control.CheckBoxList)
                            {
                                var expression = Expression.Lambda<Func<TModel, IList<object>>>(getPropertyValue, target);
                                var plist = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_list").FirstOrDefault();
                                var value = plist.GetValue(Model);
                                var safeValue = (IList<CheckBoxListItem>)value;
                                sb.Append(htmlHelper.CustomLabelFor(expression, new { @class = "col-sm-2 step-label" }));
                                sb.Append("<div class='" + col_sm + "'>");
                                sb.Append(htmlHelper.CheckBoxListFor(expression, safeValue, new { style = IsDisabled }));
                                sb.Append("</div>");
                            }
                            // 建立 CheckBox 控件
                            else if (attr.Mode == Control.CheckBox)
                            {
                                var expression = Expression.Lambda<Func<TModel, Boolean>>(getPropertyValue, target);
                                sb.Append(htmlHelper.CustomLabelFor(expression, new { @class = "col-sm-2 step-label" }));
                                sb.Append("<div class='" + col_sm + "'>");


                                sb.Append(htmlHelper.CheckBoxFor(expression, new { onchange = attr.onChangeFun, style = IsDisabled }).ToHtmlString());
                                sb.Append(attr.checkBoxWord);

                                sb.Append("</div>");
                            }
                            else
                            {

                                var expression = Expression.Lambda<Func<TModel, object>>(getPropertyValue, target);

                                // 建立 Hidden 控件
                                if (attr.Mode == Control.Hidden)
                                {
                                    sb.Append(htmlHelper.HiddenFor(expression));
                                }
                                else
                                {
                                    sb.Append(htmlHelper.CustomLabelFor(expression, new { @class = "col-sm-2 step-label" }));
                                    sb.Append("<div class='" + col_sm + "'>");

                                    // 建立 Lable 控件
                                    if (attr.Mode == Control.Lable)
                                    {
                                        var value = property.GetValue(Model);
                                        sb.Append("<p class='form-control-static'>"+value+"</p>");
                                        sb.Append(htmlHelper.HiddenFor(expression));
                                    }

                                    // 建立 TextBox 控件
                                    if (attr.Mode == Control.TextBox) sb.Append(htmlHelper.TextBoxFor(expression, new { @class = "form-control", size = attr.size, maxlength = attr.maxlength, placeholder = attr.placeholder }, IsReadOnly));



                                    // 建立 TextArea 控件
                                    if (attr.Mode == Control.TextArea) sb.Append(htmlHelper.TextAreaFor(expression, attr.rows, attr.columns, new { @class = "form-control", maxlength = attr.maxlength, style = IsDisabled }));

                                    // 建立 DatePicker 控件
                                    if (attr.Mode == Control.DatePicker) sb.Append(htmlHelper.DatePickerTWFor(expression, new { placeholder = "年/月/日", style = IsDisabled, onblur = attr.onblur }));

                                    // 建立 DropDownList控件
                                    if (attr.Mode == Control.DropDownList)
                                    {
                                        var plist = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_list").FirstOrDefault();
                                        var value = plist.GetValue(Model);
                                        var safeValue = (IList<SelectListItem>)value;
                                        sb.Append(htmlHelper.DropDownListFor(expression, safeValue, new { @class = "form-control formbar-bg", style = IsDisabled }));
                                    }


                                    // 建立 RadioGroup控件
                                    if (attr.Mode == Control.RadioGroup)
                                    {
                                        var plist = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_list").FirstOrDefault();
                                        var value = plist.GetValue(Model);
                                        var safeValue = (IList<SelectListItem>)value;
                                        sb.Append(htmlHelper.RadioGroupFor(expression, safeValue, new { style = IsDisabled }));
                                    }

                                    // 建立 Tel 控件
                                    if (attr.Mode == Control.Tel) sb.Append(htmlHelper.TelForTurbo(expression, new { @class = "form-control", size = attr.size, maxlength = attr.maxlength, placeholder = attr.placeholder, style = IsDisabled }, IsReadOnly));

                                    // 建立 Email 控件
                                    if (attr.Mode == Control.EMAIL) sb.Append(htmlHelper.EmailForTurbo(expression, new { @class = "form-control", size = attr.size, maxlength = attr.maxlength, placeholder = attr.placeholder, style = IsDisabled }, IsReadOnly));

                                    // 建立 FileUpload 控件
                                    if (attr.Mode == Control.FileUpload)
                                    {
                                        if (IsReadOnly)
                                        {
                                            var FileNameText = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_FILENAME").FirstOrDefault();
                                            var getPropertyValue_FileNameText = Expression.Property(target, FileNameText);
                                            var expression_FileNameText = Expression.Lambda<Func<TModel, object>>(getPropertyValue_FileNameText, target);
                                            sb.Append(htmlHelper.FrontFileDLForTurbo(expression_FileNameText));
                                        }
                                        else
                                        {
                                            if (attr.MaxFileSize.TONotNullString() == "" && attr.LimitFileType.TONotNullString() == "")
                                            {
                                                sb.Append(htmlHelper.FileUploadForTurbo(expression, false));
                                            }
                                            else
                                            {
                                                sb.Append(htmlHelper.FileUploadForTurbo(expression, false,false,null,Convert.ToString(attr.LimitFileType),Convert.ToString(attr.MaxFileSize),Convert.ToString(attr.UploadDesc)));
                                            }
                                        }
                                    }

                                    // 建立 FileUpload 控件
                                    if (attr.Mode == Control.FileUploadAppDoc)
                                    {
                                        if (IsReadOnly)
                                        {
                                            var FileNameText = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_FILENAME").FirstOrDefault();
                                            var getPropertyValue_FileNameText = Expression.Property(target, FileNameText);
                                            var expression_FileNameText = Expression.Lambda<Func<TModel, object>>(getPropertyValue_FileNameText, target);

                                            sb.Append(htmlHelper.FileUploadForTurbo(expression, false, true));
                                            sb.Append(htmlHelper.FontFileDLForTurbo(expression_FileNameText, "N"));
                                        }
                                        else
                                        {
                                            sb.Append(htmlHelper.FileUploadForTurbo(expression, false));
                                        }
                                    }

                                    // 建立 Log 控件
                                    if (attr.Mode == Control.Log)
                                    {
                                        var pi = Model.GetType().GetProperties().Where(m => m.Name == "APP_ID").FirstOrDefault();
                                        var piVal = pi.GetValue(Model).TONotNullString();
                                        sb.Append(htmlHelper.LogForTurbo(piVal, attr.LogSchema));
                                    }



                                    // 建立 ImageHover 控件
                                    if (attr.Mode == Control.ImageHover) sb.Append(htmlHelper.ImageHoverForTurbo(expression, attr.HoverFileName));

                                    // 建立 Addr 控件
                                    if (attr.Mode == Control.ADDR)
                                    {
                                        var piText = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_ADDR").FirstOrDefault();
                                        var getPropertyValue_TEXT = Expression.Property(target, piText);
                                        var expressionTEXT = Expression.Lambda<Func<TModel, object>>(getPropertyValue_TEXT, target);

                                        var piDetailText = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_DETAIL").FirstOrDefault();
                                        var getPropertyValue_DetailTEXT = Expression.Property(target, piDetailText);
                                        var expressionDetailTEXT = Expression.Lambda<Func<TModel, object>>(getPropertyValue_DetailTEXT, target);

                                        sb.Append(htmlHelper.AddrForTurbo(expression, expressionTEXT, expressionDetailTEXT, IsReadOnly));
                                    }

                                    // 建立 Addr 控件
                                    if (attr.Mode == Control.CountryPort)
                                    {
                                        var plist = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_list").FirstOrDefault();
                                        var value = plist.GetValue(Model);
                                        var safeValue = (IList<SelectListItem>)value;

                                        var portText = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_PORT").FirstOrDefault();
                                        var getPropertyValue_portText = Expression.Property(target, portText);
                                        var expressionPortTEXT = Expression.Lambda<Func<TModel, object>>(getPropertyValue_portText, target);

                                        //var portlist = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_PORT_list").FirstOrDefault();
                                        //var portValue = plist.GetValue(Model);
                                        var portSafeValue = new List<SelectListItem>();

                                        sb.Append(htmlHelper.CountryPortForTurbo(expression, safeValue, expressionPortTEXT, portSafeValue, IsReadOnly));
                                    }

                                    // 建立 ABCNUM控件
                                    if (attr.Mode == Control.ABCNUM)
                                    {
                                        //================= 精子
                                        // 取自
                                        var aNameText = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "A_NAME").FirstOrDefault();
                                        var getPropertyValue_aNameText = Expression.Property(target, aNameText);
                                        var expression_aNameText = Expression.Lambda<Func<TModel, object>>(getPropertyValue_aNameText, target);
                                        // 數量(管)
                                        var aNum1Text = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "A_NUM1").FirstOrDefault();
                                        var getPropertyValue_aNum1Text = Expression.Property(target, aNum1Text);
                                        var expression_aNum1Text = Expression.Lambda<Func<TModel, object>>(getPropertyValue_aNum1Text, target);
                                        // 取得日期
                                        var aDateText = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "A_DATE_AD").FirstOrDefault();
                                        var getPropertyValue_aDateText = Expression.Property(target, aDateText);
                                        var expression_aDateText = Expression.Lambda<Func<TModel, object>>(getPropertyValue_aDateText, target);

                                        //================= 卵子
                                        // 取自
                                        var bNameText = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "B_NAME").FirstOrDefault();
                                        var getPropertyValue_bNameText = Expression.Property(target, bNameText);
                                        var expression_bNameText = Expression.Lambda<Func<TModel, object>>(getPropertyValue_bNameText, target);
                                        // 數量(個)
                                        var bNum1Text = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "B_NUM1").FirstOrDefault();
                                        var getPropertyValue_bNum1Text = Expression.Property(target, bNum1Text);
                                        var expression_bNum1Text = Expression.Lambda<Func<TModel, object>>(getPropertyValue_bNum1Text, target);
                                        // 數量(個)
                                        var bNum2Text = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "B_NUM2").FirstOrDefault();
                                        var getPropertyValue_bNum2Text = Expression.Property(target, bNum1Text);
                                        var expression_bNum2Text = Expression.Lambda<Func<TModel, object>>(getPropertyValue_bNum2Text, target);
                                        // 取得日期
                                        var bDateText = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "B_DATE_AD").FirstOrDefault();
                                        var getPropertyValue_bDateText = Expression.Property(target, bDateText);
                                        var expression_bDateText = Expression.Lambda<Func<TModel, object>>(getPropertyValue_bDateText, target);

                                        //================= 胚胎
                                        // 取自
                                        var cName1Text = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "C_NAME1").FirstOrDefault();
                                        var getPropertyValue_cName1Text = Expression.Property(target, cName1Text);
                                        var expression_cName1Text = Expression.Lambda<Func<TModel, object>>(getPropertyValue_cName1Text, target);
                                        // 配偶
                                        var cName2Text = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "C_NAME2").FirstOrDefault();
                                        var getPropertyValue_cName2Text = Expression.Property(target, cName2Text);
                                        var expression_cName2Text = Expression.Lambda<Func<TModel, object>>(getPropertyValue_cName2Text, target);
                                        // 數量(個)
                                        var cNum1Text = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "C_NUM1").FirstOrDefault();
                                        var getPropertyValue_cNum1Text = Expression.Property(target, cNum1Text);
                                        var expression_cNum1Text = Expression.Lambda<Func<TModel, object>>(getPropertyValue_cNum1Text, target);
                                        // 形成日期
                                        var cDateText = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "C_DATE_AD").FirstOrDefault();
                                        var getPropertyValue_cDateText = Expression.Property(target, cDateText);
                                        var expression_cDateText = Expression.Lambda<Func<TModel, object>>(getPropertyValue_cDateText, target);
                                        // 胚胎日
                                        var cDayText = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "C_DAY").FirstOrDefault();
                                        var getPropertyValue_cDayText = Expression.Property(target, cDayText);
                                        var expression_cDayText = Expression.Lambda<Func<TModel, object>>(getPropertyValue_cDayText, target);


                                        sb.Append(htmlHelper.ABCNUMForTurbo(
                                            expression_aNameText,
                                            expression_aNum1Text,
                                            expression_aDateText,
                                            expression_bNameText,
                                            expression_bNum1Text,
                                            expression_bNum2Text,
                                            expression_bDateText,
                                            expression_cName1Text,
                                            expression_cName2Text,
                                            expression_cNum1Text,
                                            expression_cDateText,
                                            expression_cDayText,
                                            new { style = IsDisabled }, IsReadOnly));
                                    }

                                    // 建立 DonateAmount 控件
                                    if (attr.Mode == Control.DonateAmount) sb.Append(htmlHelper.DonateAmountForTurbo(expression, new { @class = "form-control", size = attr.size, maxlength = attr.maxlength, placeholder = attr.placeholder, style = IsDisabled }, IsReadOnly));

                                    if (attr.Notes.TONotNullString() != "")
                                    {
                                        sb.Append("<br/>");
                                        sb.Append(attr.Notes);
                                    }
                                    if (attr.Link.TONotNullString() != "")
                                    {
                                        sb.Append("<br/>");
                                        sb.Append("<a href='" + attr.LinkHref + "' target='_blank' title='" + attr.Link + "'>" + attr.Link + "</a> ");
                                    }

                                    sb.Append("</div>");
                                }
                            }
                        }

                        try
                        {
                            if (Ca[0].Mode.TONotNullString() != "Goods") sb.Append("</div>");
                        }
                        catch (Exception)
                        {
                            sb.Append("</div>");
                        }

                    }

                    sb.Append("</div>");
                }

                if (Ca_toggle.ToCount() > 0)
                {
                    sb.Append("</div>");
                }

                if (blockIDivd != "")
                {
                    sb.Append("</div>");
                }
            }

            return MvcHtmlString.Create(sb.ToString());
        }
        #endregion

        #region 後端
        /// <summary>
        /// HTML 控項，節儉版面及快速產製使用，將需CustomLabelFor與其他共項元件產生部分一行解決
        /// </summary>
        /// <param name="expression">Model 欄位的 Lambda 表達式物件</param>
        /// <param name="Control">其他共項元件</param>
        /// <param name="IsNew">明細專用，需配合Attr.IsNew</param>
        /// <param name="Model_tag">明細專用，若有子類別，需將子類別名稱鍵入，便可達到二階層</param>
        /// <returns></returns>
        public static MvcHtmlString ControlForTurbo<TModel, T>(this HtmlHelper<TModel> htmlHelper, T Model, bool IsNew = true)
        {
            // HTML標籤
            //var Id_tag = Model_tag == "" ? Model_tag : Model_tag + "_";
            //var Name_tag = Model_tag == "" ? Model_tag : Model_tag + ".";

            StringBuilder sb = new StringBuilder();
            // 設定ControlAttribute清單
            IList<ES.Commons.ControlAttribute> ca = new List<ES.Commons.ControlAttribute>();
            // 搜尋該ViewModel屬性(畫面Model請選擇第二層Model(FormModel,DetailModel等...)為主)
            foreach (var pi in Model.GetType().GetProperties())
            {
                // 取得屬性自定義控件狀態
                var attr = pi.GetCustomAttribute<ES.Commons.ControlAttribute>();
                if (attr != null)
                {
                    attr.pi = pi;
                    ca.Add(attr);
                }
            }

            // 算出最大的Block_group數目，讓其餘未有加group屬性保證為單一不重複
            //int max_Bgroup_value = ca.Select(m => m.block_group).Max();
            //foreach (var c in ca)
            //{
            //    ++max_Bgroup_value;
            //    if (c.block_group == 0)
            //    { c.block_group = max_Bgroup_value; }
            //}

            // 算出最大的group數目，讓其餘未有加group屬性保證為單一不重複
            int max_group_value = ca.Select(m => m.group).Max();
            foreach (var c in ca)
            {
                ++max_group_value;
                if (c.group == 0)
                { c.group = max_group_value; }
            }

            // 依據group分類
            var Ca_Block_Group = ca.GroupBy(m => m.block_toggle_group).Select(m => m.ToList()).ToList();

            foreach (var ca_Group in Ca_Block_Group)
            {
                var Ca_blockDiv_id = ca_Group.Where(m => m.block_BIG_id.TONotNullString() != "").ToList();
                var blockIDivd = Ca_blockDiv_id.ToCount() > 0 ? Ca_blockDiv_id.FirstOrDefault().block_BIG_id : "";

                if (blockIDivd != "")
                {
                    sb.Append("<div id='" + blockIDivd + "' >");
                }

                var Ca_block_id = ca_Group.Where(m => m.block_toggle_id.TONotNullString() != "").ToList();
                var blockId = Ca_block_id.ToCount() > 0 ? Ca_block_id.FirstOrDefault().block_toggle_id : "";

                // 是否顯示縮合
                var Ca_toggle = ca_Group.Where(m => m.block_toggle).ToList();
                var toggle_class = "";
                if (Ca_toggle.ToCount() > 0)
                {
                    toggle_class = "panel-collapse collapse";

                    sb.Append("<div class='panel panel-info'>");

                    sb.Append("<div class='panel-heading'>");

                    sb.Append("<h4 class='panel-title' data-toggle='collapse' data-parent='#" + blockId + "' href = '#" + blockId + "'> ");
                    sb.Append("<a>");
                    sb.Append(Ca_toggle.FirstOrDefault().toggle_name + "(點擊展開)");
                    sb.Append("</a>");
                    sb.Append("</h4>");

                    sb.Append("</div>");
                }

                sb.Append("<div id='" + blockId + "' class='" + toggle_class + "'>");

                if (Ca_toggle.ToCount() > 0)
                {
                    sb.Append(" <div class='panel-body'>");
                }

                // 依據block_group分類
                var Ca_Block_in_Group = ca_Group.GroupBy(m => m.block_group).Select(m => m.ToList()).ToList();

                foreach (var ca_Group_in in Ca_Block_in_Group)
                {
                    var ca_Group_in_id = ca_Group_in.Where(m => m.block_id.TONotNullString() != "").ToList();
                    var InBlockId = ca_Group_in_id.ToCount() > 0 ? ca_Group_in_id.FirstOrDefault().block_id : "";

                    sb.Append("<div id='" + InBlockId + "'>");

                    // 依據group分類
                    var Ca_Group = ca_Group_in.GroupBy(m => m.group).Select(m => m.ToList()).ToList();

                    foreach (var Ca in Ca_Group)
                    {
                        var CaCount = Ca.ToCount();
                        // 計算Css賦予(NormalController) 1:col-sm-10 / 2: col-sm-4 / else:col-sm-2 
                        var col_sm = "col-sm-2";
                        switch (CaCount)
                        {
                            case 1:
                                col_sm = "col-sm-10";
                                break;
                            case 2:
                                col_sm = "col-sm-4";
                                break;
                        }

                        var Ca_form_id = Ca.Where(m => m.form_id.TONotNullString() != "").ToList();
                        var form_Id = Ca_form_id.ToCount() > 0 ? Ca_form_id.FirstOrDefault().form_id : "";

                        sb.Append("<div class='form-group' id='" + form_Id + "'>");

                        foreach (var attr in Ca)
                        {
                            // 生成控制項

                            // Detail視窗專用
                            // 關閉控項狀態(平常控項使用)
                            var IsReadOnly = attr.IsReadOnly ? attr.IsReadOnly : (attr.IsOpenNew && !IsNew);
                            // 關閉控項狀態(特殊控項使用)
                            var DisabledString = "pointer-events:none;background:#DDDDDD";
                            var IsDisabled = attr.IsReadOnly ? DisabledString : (attr.IsOpenNew && !IsNew) ? DisabledString : "";

                            // property轉換成expression
                            var property = attr.pi;
                            var target = Expression.Parameter(typeof(TModel));
                            var getPropertyValue = Expression.Property(target, property);
                            if (attr.Mode == Control.Goods)
                            {
                                var expression = Expression.Lambda<Func<TModel, object>>(getPropertyValue, target);

                                sb.Append(htmlHelper.EditorFor(expression, attr.EditorViewName));
                            }
                            if (attr.Mode == Control.ZipButton)
                            {
                                var pi = Model.GetType().GetProperties().Where(m => m.Name == "APP_ID").FirstOrDefault();
                                var piVal = pi.GetValue(Model).TONotNullString();
                                sb.Append(htmlHelper.ZipButtonForTurbo(piVal, attr.CaseName));
                            }
                            // 建立 CheckBoxList控件
                            else if (attr.Mode == Control.CheckBoxList)
                            {
                                var expression = Expression.Lambda<Func<TModel, IList<object>>>(getPropertyValue, target);
                                var plist = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_list").FirstOrDefault();
                                var value = plist.GetValue(Model);
                                var safeValue = (IList<CheckBoxListItem>)value;
                                sb.Append(htmlHelper.CustomLabelFor(expression, new { @class = "col-sm-2 main-label" }));
                                sb.Append("<div class='" + col_sm + "'>");
                                sb.Append(htmlHelper.CheckBoxListFor(expression, safeValue, new { style = IsDisabled }));
                                sb.Append("</div>");
                            }
                            // 建立 CheckBox 控件
                            else if (attr.Mode == Control.CheckBox)
                            {
                                var expression = Expression.Lambda<Func<TModel, Boolean>>(getPropertyValue, target);
                                sb.Append(htmlHelper.CustomLabelFor(expression, new { @class = "col-sm-2 main-label" }));
                                sb.Append("<div class='" + col_sm + "'>");


                                sb.Append(htmlHelper.CheckBoxFor(expression, new { onchange = attr.onChangeFun, style = IsDisabled }).ToHtmlString());
                                sb.Append(attr.checkBoxWord);

                                sb.Append("</div>");
                            }
                            else
                            {

                                var expression = Expression.Lambda<Func<TModel, object>>(getPropertyValue, target);

                                // 建立 Hidden 控件
                                if (attr.Mode == Control.Hidden)
                                {
                                    sb.Append(htmlHelper.HiddenFor(expression));
                                }
                                else
                                {
                                    sb.Append(htmlHelper.CustomLabelFor(expression, new { @class = "col-sm-2 main-label" }));
                                    sb.Append("<div class='" + col_sm + "'>");

                                    // 建立 Lable 控件
                                    if (attr.Mode == Control.Lable)
                                    {
                                        var value = property.GetValue(Model);
                                        sb.Append(value);
                                        sb.Append(htmlHelper.HiddenFor(expression));
                                    }

                                    // 建立 TextBox 控件
                                    if (attr.Mode == Control.TextBox) sb.Append(htmlHelper.TextBoxFor(expression, new { @class = "form-control", size = attr.size, maxlength = attr.maxlength, placeholder = attr.placeholder }, IsReadOnly));



                                    // 建立 TextArea 控件
                                    if (attr.Mode == Control.TextArea) sb.Append(htmlHelper.TextAreaFor(expression, attr.rows, attr.columns, new { @class = "form-control", maxlength = attr.maxlength, style = IsDisabled }));

                                    // 建立 DatePicker 控件
                                    if (attr.Mode == Control.DatePicker) sb.Append(htmlHelper.DatePickerTWFor(expression, new { placeholder = "年/月/日", style = IsDisabled, onblur = attr.onblur }));

                                    // 建立 DropDownList控件
                                    if (attr.Mode == Control.DropDownList)
                                    {
                                        var plist = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_list").FirstOrDefault();
                                        var value = plist.GetValue(Model);
                                        var safeValue = (IList<SelectListItem>)value;
                                        sb.Append(htmlHelper.DropDownListFor(expression, safeValue, new { @class = "form-control", style = IsDisabled }));
                                    }


                                    // 建立 RadioGroup控件
                                    if (attr.Mode == Control.RadioGroup)
                                    {
                                        var plist = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_list").FirstOrDefault();
                                        var value = plist.GetValue(Model);
                                        var safeValue = (IList<SelectListItem>)value;
                                        sb.Append(htmlHelper.RadioGroupFor(expression, safeValue, new { style = IsDisabled }));
                                    }

                                    // 建立 Tel 控件
                                    if (attr.Mode == Control.Tel) sb.Append(htmlHelper.TelForTurbo(expression, new { @class = "form-control", size = attr.size, maxlength = attr.maxlength, placeholder = attr.placeholder, style = IsDisabled }, IsReadOnly));

                                    // 建立 Email 控件
                                    if (attr.Mode == Control.EMAIL) sb.Append(htmlHelper.EmailForTurbo(expression, new { @class = "form-control", size = attr.size, maxlength = attr.maxlength, placeholder = attr.placeholder, style = IsDisabled }, IsReadOnly));

                                    // 建立 FileUpload 控件
                                    if (attr.Mode == Control.FileUpload)
                                    {
                                        if (IsReadOnly)
                                        {
                                            var FileNameText = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_FILENAME").FirstOrDefault();
                                            var getPropertyValue_FileNameText = Expression.Property(target, FileNameText);
                                            var expression_FileNameText = Expression.Lambda<Func<TModel, object>>(getPropertyValue_FileNameText, target);

                                            sb.Append(htmlHelper.FrontFileDLForTurbo(expression_FileNameText));
                                        }
                                        else
                                        {
                                            sb.Append(htmlHelper.FileUploadForTurbo(expression, false));
                                            
                                        }
                                    }

                                    // 建立 Log 控件
                                    if (attr.Mode == Control.Log)
                                    {
                                        var pi = Model.GetType().GetProperties().Where(m => m.Name == "APP_ID").FirstOrDefault();
                                        var piVal = pi.GetValue(Model).TONotNullString();
                                        sb.Append(htmlHelper.LogForTurbo(piVal, attr.LogSchema));
                                    }



                                    // 建立 ImageHover 控件
                                    if (attr.Mode == Control.ImageHover) sb.Append(htmlHelper.ImageHoverForTurbo(expression, attr.HoverFileName));

                                    // 建立 Addr 控件
                                    if (attr.Mode == Control.ADDR)
                                    {
                                        var piText = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_ADDR").FirstOrDefault();
                                        var getPropertyValue_TEXT = Expression.Property(target, piText);
                                        var expressionTEXT = Expression.Lambda<Func<TModel, object>>(getPropertyValue_TEXT, target);

                                        var piDetailText = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_DETAIL").FirstOrDefault();
                                        var getPropertyValue_DetailTEXT = Expression.Property(target, piDetailText);
                                        var expressionDetailTEXT = Expression.Lambda<Func<TModel, object>>(getPropertyValue_DetailTEXT, target);

                                        sb.Append(htmlHelper.AddrForTurbo(expression, expressionTEXT, expressionDetailTEXT, IsReadOnly));
                                    }

                                    // 建立 Addr 控件
                                    if (attr.Mode == Control.CountryPort)
                                    {
                                        var plist = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_list").FirstOrDefault();
                                        var value = plist.GetValue(Model);
                                        var safeValue = (IList<SelectListItem>)value;

                                        var portText = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_PORT").FirstOrDefault();
                                        var getPropertyValue_portText = Expression.Property(target, portText);
                                        var expressionPortTEXT = Expression.Lambda<Func<TModel, object>>(getPropertyValue_portText, target);

                                        //var portlist = Model.GetType().GetProperties().Where(m => m.Name == attr.pi.Name + "_PORT_list").FirstOrDefault();
                                        //var portValue = plist.GetValue(Model);
                                        var portSafeValue = new List<SelectListItem>();

                                        sb.Append(htmlHelper.CountryPortForTurbo(expression, safeValue, expressionPortTEXT, portSafeValue, IsReadOnly));
                                    }

                                    // 建立 ABCNUM控件
                                    if (attr.Mode == Control.ABCNUM)
                                    {
                                        //================= 精子
                                        // 取自
                                        var aNameText = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "A_NAME").FirstOrDefault();
                                        var getPropertyValue_aNameText = Expression.Property(target, aNameText);
                                        var expression_aNameText = Expression.Lambda<Func<TModel, object>>(getPropertyValue_aNameText, target);
                                        // 數量(管)
                                        var aNum1Text = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "A_NUM1").FirstOrDefault();
                                        var getPropertyValue_aNum1Text = Expression.Property(target, aNum1Text);
                                        var expression_aNum1Text = Expression.Lambda<Func<TModel, object>>(getPropertyValue_aNum1Text, target);
                                        // 取得日期
                                        var aDateText = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "A_DATE_AD").FirstOrDefault();
                                        var getPropertyValue_aDateText = Expression.Property(target, aDateText);
                                        var expression_aDateText = Expression.Lambda<Func<TModel, object>>(getPropertyValue_aDateText, target);

                                        //================= 卵子
                                        // 取自
                                        var bNameText = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "B_NAME").FirstOrDefault();
                                        var getPropertyValue_bNameText = Expression.Property(target, bNameText);
                                        var expression_bNameText = Expression.Lambda<Func<TModel, object>>(getPropertyValue_bNameText, target);
                                        // 數量(個)
                                        var bNum1Text = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "B_NUM1").FirstOrDefault();
                                        var getPropertyValue_bNum1Text = Expression.Property(target, bNum1Text);
                                        var expression_bNum1Text = Expression.Lambda<Func<TModel, object>>(getPropertyValue_bNum1Text, target);
                                        // 數量(個)
                                        var bNum2Text = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "B_NUM2").FirstOrDefault();
                                        var getPropertyValue_bNum2Text = Expression.Property(target, bNum1Text);
                                        var expression_bNum2Text = Expression.Lambda<Func<TModel, object>>(getPropertyValue_bNum2Text, target);
                                        // 取得日期
                                        var bDateText = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "B_DATE_AD").FirstOrDefault();
                                        var getPropertyValue_bDateText = Expression.Property(target, bDateText);
                                        var expression_bDateText = Expression.Lambda<Func<TModel, object>>(getPropertyValue_bDateText, target);

                                        //================= 胚胎
                                        // 取自
                                        var cName1Text = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "C_NAME1").FirstOrDefault();
                                        var getPropertyValue_cName1Text = Expression.Property(target, cName1Text);
                                        var expression_cName1Text = Expression.Lambda<Func<TModel, object>>(getPropertyValue_cName1Text, target);
                                        // 配偶
                                        var cName2Text = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "C_NAME2").FirstOrDefault();
                                        var getPropertyValue_cName2Text = Expression.Property(target, cName2Text);
                                        var expression_cName2Text = Expression.Lambda<Func<TModel, object>>(getPropertyValue_cName2Text, target);
                                        // 數量(個)
                                        var cNum1Text = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "C_NUM1").FirstOrDefault();
                                        var getPropertyValue_cNum1Text = Expression.Property(target, cNum1Text);
                                        var expression_cNum1Text = Expression.Lambda<Func<TModel, object>>(getPropertyValue_cNum1Text, target);
                                        // 形成日期
                                        var cDateText = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "C_DATE_AD").FirstOrDefault();
                                        var getPropertyValue_cDateText = Expression.Property(target, cDateText);
                                        var expression_cDateText = Expression.Lambda<Func<TModel, object>>(getPropertyValue_cDateText, target);
                                        // 胚胎日
                                        var cDayText = Model.GetType().GetProperties().Where(m => m.Name == attr.fontWord + "C_DAY").FirstOrDefault();
                                        var getPropertyValue_cDayText = Expression.Property(target, cDayText);
                                        var expression_cDayText = Expression.Lambda<Func<TModel, object>>(getPropertyValue_cDayText, target);


                                        sb.Append(htmlHelper.ABCNUMForTurbo(
                                            expression_aNameText,
                                            expression_aNum1Text,
                                            expression_aDateText,
                                            expression_bNameText,
                                            expression_bNum1Text,
                                            expression_bNum2Text,
                                            expression_bDateText,
                                            expression_cName1Text,
                                            expression_cName2Text,
                                            expression_cNum1Text,
                                            expression_cDateText,
                                            expression_cDayText,
                                            new { style = IsDisabled }, IsReadOnly));
                                    }
                                    if (attr.Notes.TONotNullString() != "")
                                    {
                                        sb.Append("<br/>");
                                        sb.Append(attr.Notes);
                                    }
                                    if (attr.Link.TONotNullString() != "")
                                    {
                                        sb.Append("<br/>");
                                        sb.Append("<a href='" + attr.LinkHref + "' title='" + attr.Link + "'>" + attr.Link + "</a> ");
                                    }
                                    sb.Append("</div>");
                                }
                            }
                        }


                        sb.Append("</div>");
                    }

                    sb.Append("</div>");
                }



                sb.Append("</div>");
                if (Ca_toggle.ToCount() > 0)
                {
                    sb.Append("</div>");
                    sb.Append("</div>");
                }

                if (blockIDivd != "")
                {
                    sb.Append("</div>");
                }
            }

            return MvcHtmlString.Create(sb.ToString());
        }
        #endregion
    }
}