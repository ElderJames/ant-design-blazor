﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace AntDesign
{
    public class AutoCompleteSearch : Search, IAutoCompleteInput
    {

        [CascadingParameter]
        public IAutoCompleteRef AutoComplete { get; set; }

        [CascadingParameter(Name = "OverlayTriggerContext")]
        public ForwardRef OverlayTriggerContext
        {
            get { return WrapperRefBack; }
            set { WrapperRefBack = value; }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (AutoComplete != null) AutoComplete?.SetInputComponent(this);
        }

        internal override async Task OnFocusAsync(FocusEventArgs e)
        {
            if (AutoComplete != null) await AutoComplete?.InputFocus(e);

            await base.OnFocusAsync(e);

        }

        protected override async Task OnkeyDownAsync(KeyboardEventArgs args)
        {
            await base.OnkeyDownAsync(args);

            if (AutoComplete != null) await AutoComplete?.InputKeyDown(args);
        }


        protected override async void OnInputAsync(ChangeEventArgs args)
        {
            base.OnInputAsync(args);

            if (AutoComplete != null) await AutoComplete?.InputInput(args);
        }


        #region IAutoCompleteInput

        public void SetValue(object value)
        {
            this.CurrentValue = value?.ToString();
        }

        #endregion
    }
}
