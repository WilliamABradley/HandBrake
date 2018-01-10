﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VideoEncoderConverter.cs" company="HandBrake Project (http://handbrake.fr)">
//   This file is part of the HandBrake source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Video Encoder Converter
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandBrake.Converters.Video
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Data;

    using HandBrake.CoreLibrary.Interop;
    using HandBrake.CoreLibrary.Interop.Model.Encoding;

    using HandBrake.Utilities;

    using EncodeTask = HandBrake.Services.Encode.Model.EncodeTask;
    using OutputFormat = HandBrake.Services.Encode.Model.Models.OutputFormat;
    using SystemInfo = HandBrake.CoreLibrary.Utilities.SystemInfo;

    /// <summary>
    /// Video Encoder Converter
    /// </summary>
    public class VideoEncoderConverter : IMultiValueConverter
    {
        /// <summary>
        /// Gets a list of Video encoders OR returns the string name of an encoder depending on the input.
        /// </summary>
        /// <param name="values">
        /// The values.
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
        /// IEnumberable VideoEncoder or String encoder name.
        /// </returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count() == 2)
            {
                List<VideoEncoder> encoders = EnumHelper<VideoEncoder>.GetEnumList().ToList();
                EncodeTask task = values[1] as EncodeTask;

                if (HandBrakeEncoderHelpers.VideoEncoders.All(a => a.ShortName != EnumHelper<VideoEncoder>.GetShortName(VideoEncoder.X264_10)))
                {
                    encoders.Remove(VideoEncoder.X264_10);
                }

                if (HandBrakeEncoderHelpers.VideoEncoders.All(a => a.ShortName != EnumHelper<VideoEncoder>.GetShortName(VideoEncoder.X265_10)))
                {
                    encoders.Remove(VideoEncoder.X265_10);
                }

                if (HandBrakeEncoderHelpers.VideoEncoders.All(a => a.ShortName != EnumHelper<VideoEncoder>.GetShortName(VideoEncoder.X265_12)))
                {
                    encoders.Remove(VideoEncoder.X265_12);
                }

                if (task != null && task.OutputFormat != OutputFormat.Mkv)
                {
                    encoders.Remove(VideoEncoder.Theora);
                    encoders.Remove(VideoEncoder.VP8);
                    encoders.Remove(VideoEncoder.VP9);
                }

                if (!SystemInfo.IsQsvAvailableH264)
                {
                    encoders.Remove(VideoEncoder.QuickSync);
                }

                if (!SystemInfo.IsQsvAvailableH265)
                {
                    encoders.Remove(VideoEncoder.QuickSyncH265);
                    encoders.Remove(VideoEncoder.QuickSyncH26510b);
                }
                else if (!SystemInfo.IsQsvAvailableH26510bit)
                {
                    encoders.Remove(VideoEncoder.QuickSyncH26510b);
                }

                return EnumHelper<VideoEncoder>.GetEnumDisplayValuesSubset(encoders);
            }

            if (values[0].GetType() == typeof(VideoEncoder))
            {
                return EnumHelper<VideoEncoder>.GetDisplay((VideoEncoder)values[0]);
            }

            return null;
        }

        /// <summary>
        /// Convert from a string name, to enum value.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetTypes">
        /// The target types.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// Returns the video encoder enum item.
        /// </returns>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            string name = value as string;
            if (!string.IsNullOrEmpty(name))
            {
                return new object[] { EnumHelper<VideoEncoder>.GetValue(name) };
            }

            return null;
        }
    }
}