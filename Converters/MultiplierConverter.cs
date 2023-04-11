﻿// MultiplierConverter.cs
using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace ToucanUI.Converters
{
    public class MultiplierConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue && parameter is string stringParameter &&
                double.TryParse(stringParameter, NumberStyles.Any, CultureInfo.InvariantCulture, out double multiplier))
            {
                return doubleValue * multiplier;
            }

            return AvaloniaProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

