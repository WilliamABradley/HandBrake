﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SubtitleBehaviourConverter.cs" company="HandBrake Project (http://handbrake.fr)">
//   This file is part of the HandBrake source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Subtitle Behaviour Converter
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandBrake.Converters.Subtitles
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    using HandBrake.Model.Subtitles;
    using HandBrake.Utilities;

    /// <summary>
    /// Subtitle Behaviour Converter
    /// </summary>
    public class SubtitleBehaviourConverter : IValueConverter
    {
        /// <summary>
        /// The convert.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value.GetType() == typeof(BindingList<SubtitleBehaviourModes>))
            {
                return
                    new BindingList<string>(
                        EnumHelper<SubtitleBehaviourModes>.GetEnumDisplayValues(typeof(SubtitleBehaviourModes)).ToList());
            }

            if (value != null && value.GetType() == typeof(SubtitleBehaviourModes))
            {
                return EnumHelper<SubtitleBehaviourModes>.GetDisplay((SubtitleBehaviourModes)value);
            }

            return null;
        }

        /// <summary>
        /// The convert back.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = value as string;
            if (!string.IsNullOrEmpty(name))
            {
                return EnumHelper<SubtitleBehaviourModes>.GetValue(name);
            }

            return null;
        }
    }
}