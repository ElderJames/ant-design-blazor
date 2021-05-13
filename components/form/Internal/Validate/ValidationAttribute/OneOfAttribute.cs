﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;

namespace AntDesign.Internal
{
    internal class OneOfAttribute : ValidationAttribute
    {
        internal object[] Values { get; set; }

        internal OneOfAttribute(object[] values) : base("The field {0} should be one of {1}")// TODO: localizable
        {
            Values = values;
        }

        public override string FormatErrorMessage(string name)
        {
            return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, JsonSerializer.Serialize(Values));
        }

        public override bool IsValid(object value)
        {
            if (value is Array)
            {
                return false;
            }

            foreach (var v in Values)
            {
                if (v.Equals(value))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
