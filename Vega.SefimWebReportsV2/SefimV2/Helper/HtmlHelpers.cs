using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace SefimV2.Helper
{
    public static class HtmlHelpers
    {       
        #region HtmlHelpers        

        #region DatePickers

        public static MvcHtmlString CustomDateHelperFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, bool isZorunluMu = false, string placeholder = "Tarih Seçiniz", bool isDisabled = false, bool readOnly = false)
        {
            var fieldName = ExpressionHelper.GetExpressionText(expression);
            var fullBindingName = html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(fieldName);
            var fieldId = TagBuilder.CreateSanitizedId(fullBindingName);

            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);

            var value = metadata.Model;
            if (metadata.Model != null && (DateTime)value == default(DateTime))
            {
                value = null;
            }
            TagBuilder divTag = new TagBuilder("div");
            divTag.AddCssClass("input-group");

            TagBuilder inputTag = new TagBuilder("input");
            inputTag.Attributes.Add("type", "text");
            inputTag.Attributes.Add("data-toggle", "tooltip");
            inputTag.Attributes.Add("data-html", "true");
            inputTag.Attributes.Add("title", "gg/aa/yy <br/> 30/12/2016 <br/> Şeklinde giriniz");
            inputTag.Attributes.Add("trigger", "manual");
            inputTag.AddCssClass("form-control");
            inputTag.Attributes.Add("placeholder", placeholder);
            inputTag.Attributes.Add("autocomplete", "off");
            inputTag.Attributes.Add("name", fullBindingName);
            inputTag.Attributes.Add("id", fieldId);
            inputTag.Attributes.Add("value", value == null ? string.Empty : Convert.ToDateTime(value.ToString()).ToString("dd'/'MM'/'yyyy"));

            if (isDisabled)
            {
                inputTag.Attributes.Add("disabled", "true");
            }

            if (readOnly)
            {
                inputTag.Attributes.Add("readonly", "readonly");
            }
            else
            {
                inputTag.AddCssClass("datepicker");
            }


            if (!isZorunluMu)
            {
                var zorunlu = (expression.Body as MemberExpression).Member.IsDefined(typeof(RequiredAttribute), false);
                if (zorunlu)
                {
                    var validationAttributes = html.GetUnobtrusiveValidationAttributes(fullBindingName, metadata);

                    foreach (var key in validationAttributes.Keys)
                    {
                        inputTag.Attributes.Add(key, validationAttributes[key].ToString());
                    }
                }
            }
            else
            {

                inputTag.Attributes.Add("data-val", "true");
                inputTag.Attributes.Add("data-val-required", metadata.DisplayName + " alanı için bir tarih seçiniz.");
            }

            TagBuilder spanTag = new TagBuilder("span");
            spanTag.AddCssClass("input-group-addon fa fa-calendar");

            divTag.InnerHtml = inputTag.ToString(TagRenderMode.StartTag);
            divTag.InnerHtml += spanTag.ToString();

            return new MvcHtmlString(divTag.ToString());
        }

        #endregion DatePickers
                  
        #region CheckBox
        public static MvcHtmlString CheckBoxNullable<TModel, TProperty>(this HtmlHelper<TModel> h, Expression<Func<TModel, TProperty>> expression, bool isDisabled = false)
        {
            var fieldName = ExpressionHelper.GetExpressionText(expression);
            var fullBindingName = h.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(fieldName);
            var fieldId = TagBuilder.CreateSanitizedId(fullBindingName);

            var metadata = ModelMetadata.FromLambdaExpression(expression, h.ViewData);
            //var propName = metadata.PropertyName;
            var model = metadata.Model;
            var displayName = metadata.DisplayName;
            var container = metadata.Container;
            var isrequired = metadata.IsRequired;
            //string id = fullBindingName + "_Id";
            TagBuilder inputHiddenTag = new TagBuilder("input");

            TagBuilder inputTag = new TagBuilder("input");
            inputTag.AddCssClass("chk_tristate");
            inputTag.Attributes.Add("id", fieldId);
            inputTag.Attributes.Add("name", fullBindingName);
            inputTag.Attributes.Add("type", "checkbox");
            if (isDisabled)
            {
                inputTag.Attributes.Add("disabled", "disabled");
            }
            //string inputValue = container.GetType().GetProperty(propName).GetValue(container, null) == null ? "null" : container.GetType().GetProperty(propName).GetValue(container, null).ToString();
            if (model != null)
            {
                if ((bool)model == true)
                {
                    inputTag.Attributes.Add("value", "true");
                    inputTag.Attributes.Add("checked", "checked");
                }
                else
                {
                    inputTag.Attributes.Add("value", "true");

                    inputHiddenTag.Attributes.Add("type", "hidden");
                    inputHiddenTag.Attributes.Add("id", fieldId + "_hiddenValue");
                    inputHiddenTag.Attributes.Add("value", "false");
                }
            }
            else
            {
                inputTag.Attributes.Add("value", "null");
                inputTag.Attributes.Add("indeterminate", "indeterminate");
            }

            TagBuilder pTag = new TagBuilder("p");
            pTag.AddCssClass("output");
            pTag.Attributes.Add("id", fieldId + "_text");

            TagBuilder divTag = new TagBuilder("div");
            divTag.InnerHtml = inputTag.ToString();
            if (model != null)
            {
                if ((bool)model == false)
                {
                    divTag.InnerHtml += inputHiddenTag.ToString();
                }
            }
            divTag.InnerHtml += pTag.ToString();


            return new MvcHtmlString(divTag.ToString());
        }
        #endregion CheckBox
        
        #region DropDown
        public static MvcHtmlString DropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, IEnumerable<SelectListItem> selectList, object htmlAttributes, bool isZorunluMu)
        {
            var fieldName = ExpressionHelper.GetExpressionText(expression);
            var fullBindingName = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(fieldName);
            var fieldId = TagBuilder.CreateSanitizedId(fullBindingName);

            List<SelectListItem> list = selectList.ToList();
            if (isZorunluMu)
            {
                list.Insert(0, new SelectListItem { Text = "Seçiniz", Value = null });
            }
            else
            {
                list.Insert(0, new SelectListItem { Text = "Seçiniz", Value = "" });
            }
            return SelectExtensions.DropDownListFor(htmlHelper, expression, list, htmlAttributes);
        }

        private static MvcHtmlString DropDownForBoolEnumWithDefaultValue(ModelMetadata metadata, string fullBindingName, string fieldId, List<SelectListItem> list, bool? defaultSelectedValue)
        {
            TagBuilder selectTag = new TagBuilder("select");
            selectTag.AddCssClass("form-control first_open");
            selectTag.Attributes.Add("data-val", "true");
            selectTag.Attributes.Add("data-val-required", metadata.DisplayName + " alanı boş bırakılamaz");
            selectTag.Attributes.Add("name", fullBindingName);
            selectTag.Attributes.Add("id", fieldId);

            foreach (var item in list)
            {
                TagBuilder optionTag = new TagBuilder("option");
                optionTag.Attributes.Add("value", item.Value);
                optionTag.SetInnerText(item.Text);
                if ((item.Value == "" || item.Value == "0") && defaultSelectedValue == null)
                {
                    optionTag.Attributes.Add("selected", "selected");
                }
                else if (item.Value == "True" && defaultSelectedValue == true)
                {
                    optionTag.Attributes.Add("selected", "selected");
                }
                else if (item.Value == "False" && defaultSelectedValue == false)
                {
                    optionTag.Attributes.Add("selected", "selected");
                }
                selectTag.InnerHtml += optionTag.ToString();
            }
            return new MvcHtmlString(selectTag.ToString());
        }

        private static MvcHtmlString DropDownForBoolEnumWithMetaDataValue(ModelMetadata metadata, string fullBindingName, string fieldId, List<SelectListItem> list)
        {
            TagBuilder selectTag = new TagBuilder("select");
            selectTag.AddCssClass("form-control first_open");
            selectTag.Attributes.Add("data-val", "true");
            selectTag.Attributes.Add("data-val-required", metadata.DisplayName + " alanı boş bırakılamaz");
            selectTag.Attributes.Add("name", fullBindingName);
            selectTag.Attributes.Add("id", fieldId);

            foreach (var item in list)
            {
                TagBuilder optionTag = new TagBuilder("option");
                optionTag.Attributes.Add("value", item.Value);
                optionTag.SetInnerText(item.Text);

                if (metadata.ModelType.IsGenericType && metadata.ModelType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    if ((item.Value == "" || item.Value == "0") && metadata.Model == null)
                    {
                        optionTag.Attributes.Add("selected", "selected");
                    }
                    else if (item.Value == "True" &&  (bool)metadata.Model == true)
                    {
                        optionTag.Attributes.Add("selected", "selected");
                    }
                    else if (item.Value == "False" && (bool)metadata.Model == false)
                    {
                        optionTag.Attributes.Add("selected", "selected");
                    }
                    selectTag.InnerHtml += optionTag.ToString();
                }
                else
                {
                    if (item.Value == "True" && (bool)metadata.Model == true)
                    {
                        optionTag.Attributes.Add("selected", "selected");
                    }
                    else if (item.Value == "False" && (bool)metadata.Model == false)
                    {
                        optionTag.Attributes.Add("selected", "selected");
                    }
                    selectTag.InnerHtml += optionTag.ToString();
                }
            }
            return new MvcHtmlString(selectTag.ToString());
        }

        private static bool BoolEnumSayfaIlkDefaMiAciliyor(IDictionary<string, object> gelenhtmlAttributes, bool sayfaIlkDefaMiAciliyor)
        {
            foreach (var item in gelenhtmlAttributes)
            {
                if (item.Key == "boolEnumControlValue")
                {
                    if (item.Value != null && item.Value.ToString() == "0")
                    {
                        sayfaIlkDefaMiAciliyor = true;
                    }
                    else
                    {
                        sayfaIlkDefaMiAciliyor = false;
                    }
                }
            }
            return sayfaIlkDefaMiAciliyor;
        }
        
        #endregion DropDown
    
        #region Enum

        public static MvcHtmlString EnumDropdownWithDisplayNames<TModel, TEnum>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TEnum>> expression, string optionLabel, object htmlAttributes, List<byte> displayedEnumIdList = null, bool orderByName = true, bool dontUseUnset = false, bool valueSifirOlanDahilOlsunMu = false)
        {
            var type = typeof(TEnum);
            if (!type.IsEnum)
            {
                throw new InvalidOperationException();
            }

            var selectList = new List<SelectListItem>();
            var fields = type.GetFields();

            for (int i = 0; i < fields.Length; i++)
            {
                var attribute = Attribute.GetCustomAttribute(fields[i], typeof(DisplayAttribute)) as DisplayAttribute;
                if (attribute != null && ( valueSifirOlanDahilOlsunMu ? ((byte)fields[i].GetValue(null) != 0 || (byte)fields[i].GetValue(null) == 0) :  (byte)fields[i].GetValue(null) != 0))
                {
                    if (displayedEnumIdList == null)
                    {
                        selectList.Add(new SelectListItem { Text = attribute.Name, Value = ((byte)fields[i].GetValue(null)).ToString() });
                    }
                    else
                    {
                        if (displayedEnumIdList.Count() > 0 && displayedEnumIdList.Any(s => s == (byte)fields[i].GetValue(null)))
                            selectList.Add(new SelectListItem { Text = attribute.Name, Value = ((byte)fields[i].GetValue(null)).ToString() });
                    }
                }
            }

            if (orderByName)
            {
                //Select Listesi içerisinde yer alan enumlar alfabetık olarak sıralanır.
                selectList = selectList.OrderBy(s => s.Text).ToList();
            }

            MvcHtmlString htmlString = null;
            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            //PropertyInfo propertyInfo = typeof(TModel).GetProperties().Where(s => s.PropertyType == typeof(TEnum)).FirstOrDefault();
            PropertyInfo propertyInfo = metadata.ContainerType.GetProperties().Where(s => s.PropertyType == typeof(TEnum)).FirstOrDefault();
            Attribute requiredAttr = propertyInfo.GetCustomAttribute<RequiredAttribute>();

            if (metadata.Model != null)
            {
                var element = selectList.Where(x => x.Value == ((byte)metadata.Model).ToString()).FirstOrDefault();
                if (element != null)
                {
                    element.Selected = true;
                }

            }


            if (requiredAttr == null)
            {
                selectList.Insert(0, new SelectListItem { Text = optionLabel, Value = "0" });
                htmlString = htmlHelper.DropDownListFor(expression, selectList, htmlAttributes);
            }
            else
            {
                if (dontUseUnset)
                {
                    htmlString = htmlHelper.DropDownListFor(expression, selectList, htmlAttributes);
                }
                else
                {
                    htmlString = htmlHelper.DropDownListFor(expression, selectList, optionLabel, htmlAttributes);
                }
            }

            return htmlString;
        }

        #endregion
              
        #endregion HtmlHelpers                               
    }
}