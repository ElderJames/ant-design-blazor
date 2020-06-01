﻿using System;

#pragma warning disable 1591
// ReSharper disable once CheckNamespace

namespace AntDesign
{
    public enum SelectMode
    {
        Default,
        Tags,
        Multiple
    }

    public static class SelectModeExtensions
    {
        public const string Tags = "tags";
        public const string Multiple = "multiple";
        private const StringComparison Comparison = StringComparison.OrdinalIgnoreCase;

        public static SelectMode ToSelectMode(this string mode)
        {
            if (Tags.Equals(mode, Comparison))
            {
                return SelectMode.Tags;
            }

            return Multiple.Equals(mode, Comparison) ? SelectMode.Multiple : SelectMode.Default;
        }
    }
}
