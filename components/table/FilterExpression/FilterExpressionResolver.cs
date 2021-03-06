﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace AntDesign.FilterExpression
{
    public class FilterExpressionResolver<T>
    {
        private readonly StringFilterExpression _stringFilter = new StringFilterExpression();
        private readonly NumberFilterExpression _numberFilter = new NumberFilterExpression(typeof(T));
        private readonly DateFilterExpression _dateFilter = new DateFilterExpression();

        public FilterExpressionResolver()
        {
        }

        public IFilterExpression GetFilterExpression()
        {
            if (THelper.IsNumericType<T>())
            {
                return _numberFilter;
            }
            else
            {
                if (THelper.GetUnderlyingType<T>() == typeof(DateTime))
                {
                    return _dateFilter;
                }
                else
                {
                    if (THelper.GetUnderlyingType<T>() == typeof(string))
                    {
                        return _stringFilter;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }
    }
}
