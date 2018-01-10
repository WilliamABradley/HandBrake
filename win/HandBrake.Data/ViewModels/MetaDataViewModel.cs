﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MetaDataViewModel.cs" company="HandBrake Project (http://handbrake.fr)">
//   This file is part of the HandBrake source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The Meta Data Tab
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandBrake.ViewModels
{
    using System;

    using HandBrake.EventArgs;
    using HandBrake.Services.Encode.Model;
    using HandBrake.Services.Encode.Model.Models;
    using HandBrake.Services.Interfaces;
    using HandBrake.Services.Presets.Model;
    using HandBrake.Services.Scan.Model;
    using HandBrake.ViewModels.Interfaces;

    /// <summary>
    /// The meta data view model.
    /// </summary>
    public class MetaDataViewModel : ViewModelBase, IMetaDataViewModel
    {
        private EncodeTask task;
        private MetaData metaData;

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaDataViewModel"/> class.
        /// </summary>
        /// <param name="userSettingService">
        /// The user Setting Service.
        /// </param>
        public MetaDataViewModel(IUserSettingService userSettingService)
        {
            this.Task = new EncodeTask();
        }

        public event EventHandler<TabStatusEventArgs> TabStatusChanged;

        /// <summary>
        /// The Current Job
        /// </summary>
        public EncodeTask Task
        {
            get
            {
                return this.task;
            }
            set
            {
                this.task = value;

                if (this.task != null)
                {
                    this.MetaData = this.task.MetaData;
                }

                this.NotifyOfPropertyChange(() => this.Task);
            }
        }

        /// <summary>
        /// Gets or sets the meta data.
        /// </summary>
        public MetaData MetaData
        {
            get
            {
                return this.metaData;
            }
            set
            {
                this.metaData = value;
                this.NotifyOfPropertyChange(() => this.MetaData);
            }
        }

        /// <summary>
        /// Setup the window after a scan.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="selectedTitle">
        /// The selected title.
        /// </param>
        /// <param name="currentPreset">
        /// The Current preset
        /// </param>
        /// <param name="encodeTask">
        /// The task.
        /// </param>
        public void SetSource(Source source, Title selectedTitle, Preset currentPreset, EncodeTask encodeTask)
        {
            this.Task = encodeTask;
        }

        /// <summary>
        /// Set the selected preset
        /// </summary>
        /// <param name="preset">
        /// The preset.
        /// </param>
        /// <param name="encodeTask">
        /// The task.
        /// </param>
        public void SetPreset(Preset preset, EncodeTask encodeTask)
        {
            this.Task = encodeTask;
        }

        /// <summary>
        /// Update all the UI controls based on the encode task passed in.
        /// </summary>
        /// <param name="encodeTask">
        /// The task.
        /// </param>
        public void UpdateTask(EncodeTask encodeTask)
        {
            this.Task = encodeTask;
        }

        public bool MatchesPreset(Preset preset)
        {
            return true;
        }
    }
}