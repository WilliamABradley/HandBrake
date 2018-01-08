﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CanBurnSubtitleConverter.cs" company="HandBrake Project (http://handbrake.fr)">
// This file is part of the HandBrake source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Subtitle Behaviour Converter
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandBrake.Converters.Subtitles
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    using HandBrake.Services.Encode.Model;
    using HandBrake.Services.Encode.Model.Models;

    public class CanBurnSubtitleConverter : IMultiValueConverter
    {
     

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 1)
            {
                bool sourceTrackCanBurn = (bool)values[0];
                SubtitleType type = (SubtitleType)values[1];
                EncodeTask task = values[2] as EncodeTask;

                if (task != null && OutputFormat.Mp4.Equals(task.OutputFormat) && SubtitleType.PGS.Equals(type))
                {
                    return false;
                }

                return sourceTrackCanBurn;
            }

            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
