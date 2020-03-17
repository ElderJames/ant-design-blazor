﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AntBlazor
{
    /// <summary>
    ///
    /// </summary>
    public class AntInputBase : AntInputComponentBase<string>
    {
        protected const string InputMarkup = "<input class=\"@ClassMapper.Class\" style=\"@Style\" @attributes=\"Attributes\" Id=\"@Id\" type=\"@_type\" placeholder=\"@placeholder\" value=\"@Value\" @onchange=\"OnChangeAsync\" @onkeypress=\"OnPressEnterAsync\" @oninput=\"OnInputAsync\" />";

        protected const string PrefixCls = "ant-input";

        protected bool _allowClear;
        protected string _wrapperClass;
        protected string _groupClass;
        protected string _clearIconClass;
        protected ElementReference inputEl { get; set; }
        protected string _type = "text";

        [Parameter]
        public RenderFragment addOnBefore { get; set; }

        [Parameter]
        public RenderFragment addOnAfter { get; set; }

        [Parameter]
        public string size { get; set; } = AntInputSize.Default;

        [Parameter]
        public string placeholder { get; set; }

        [Parameter]
        public string defaultValue { get; set; }

        [Parameter]
        public int maxLength { get; set; } = -1;

        [Parameter]
        public RenderFragment prefix { get; set; }

        [Parameter]
        public RenderFragment suffix { get; set; }

        [Parameter]
        public EventCallback<ChangeEventArgs> onChange { get; set; }

        [Parameter]
        public EventCallback<KeyboardEventArgs> onPressEnter { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (!string.IsNullOrEmpty(defaultValue) && string.IsNullOrEmpty(Value))
            {
                Value = defaultValue;
            }

            SetClasses();
        }

        protected virtual void SetClasses()
        {
            ClassMapper.Clear()
                .Add($"{PrefixCls}")
                .If($"{PrefixCls}-lg", () => size == AntInputSize.Large)
                .If($"{PrefixCls}-sm", () => size == AntInputSize.Small);

            if (Attributes is null)
            {
                Attributes = new System.Collections.Generic.Dictionary<string, object>();
            }

            if (maxLength >= 0)
            {
                Attributes?.Add("maxlength", maxLength);
            }

            if (Attributes.ContainsKey("disabled"))
            {
                // TODO: disable element
                _wrapperClass = string.Join(" ", _wrapperClass, $"{PrefixCls}-affix-wrapper-disabled");
                ClassMapper.Add($"{PrefixCls}-disabled");
            }

            if (Attributes.ContainsKey("allowClear"))
            {
                _allowClear = true;
                _clearIconClass = $"{PrefixCls}-clear-icon";
                ToggleClearBtn();
            }

            if (size == AntInputSize.Large)
            {
                _wrapperClass = string.Join(" ", _wrapperClass, $"{PrefixCls}-affix-wrapper-lg");
                _groupClass = string.Join(" ", _groupClass, $"{PrefixCls}-group-wrapper-lg");
            }
            else if (size == AntInputSize.Small)
            {
                _wrapperClass = string.Join(" ", _wrapperClass, $"{PrefixCls}-affix-wrapper-sm");
                _groupClass = string.Join(" ", _groupClass, $"{PrefixCls}-group-wrapper-sm");
            }
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            SetClasses();
        }

        public override Task SetParametersAsync(ParameterView parameters)
        {
            return base.SetParametersAsync(parameters);
        }

        protected async Task OnChangeAsync(ChangeEventArgs args)
        {
            if (onChange.HasDelegate)
            {
                await onChange.InvokeAsync(args);
            }
        }

        protected async Task OnPressEnterAsync(KeyboardEventArgs args)
        {
            if (args.Code == "Enter" && onPressEnter.HasDelegate)
            {
                await onPressEnter.InvokeAsync(args);
            }
        }

        private void ToggleClearBtn()
        {
            if (string.IsNullOrEmpty(Value))
            {
                suffix = null;
                StateHasChanged();
            }
            else
            {
                suffix = BuildAntIcon("close-circle");
                StateHasChanged();
            }
        }

        /// <summary>
        /// Invoked when user add/remove content
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        protected virtual Task OnInputAsync(ChangeEventArgs args)
        {
            // AntInputComponentBase.Value will be empty, use args.Value
            Value = args.Value.ToString();
            if (_allowClear)
            {
                ToggleClearBtn();
            }

            return Task.CompletedTask;
        }

        protected virtual void ClearContent()
        {
            if (_allowClear)
            {
                Value = string.Empty;
                ToggleClearBtn();
            }
        }

        protected virtual RenderFragment BuildAntIcon(string icon, Dictionary<string, object> attrDict = null)
        {
            return new RenderFragment((builder) =>
            {
                int i = 0;
                builder.OpenComponent<AntIcon>(i++);
                builder.AddAttribute(i++, "type", icon);
                if (attrDict != null)
                {
                    foreach (var pair in attrDict)
                    {
                        builder.AddAttribute(i++, pair.Key, pair.Value);
                    }
                }
                builder.CloseComponent();
            });
        }
    }
}