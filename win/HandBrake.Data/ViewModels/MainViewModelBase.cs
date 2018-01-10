﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModelBase.cs" company="HandBrake Project (http://handbrake.fr)">
//   This file is part of the HandBrake source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   HandBrakes Main Window
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandBrake.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Caliburn.Micro;
    using HandBrake;
    using HandBrake.Common;
    using HandBrake.CoreLibrary.Interop;
    using HandBrake.EventArgs;
    using HandBrake.Factories;
    using HandBrake.Helpers;
    using HandBrake.Model;
    using HandBrake.Model.Audio;
    using HandBrake.Model.Prompts;
    using HandBrake.Model.Subtitles;
    using HandBrake.Properties;
    using HandBrake.Services.Encode.EventArgs;
    using HandBrake.Services.Encode.Model;
    using HandBrake.Services.Encode.Model.Models;
    using HandBrake.Services.Interfaces;
    using HandBrake.Services.Presets.Interfaces;
    using HandBrake.Services.Presets.Model;
    using HandBrake.Services.Queue.Interfaces;
    using HandBrake.Services.Queue.Model;
    using HandBrake.Services.Scan.EventArgs;
    using HandBrake.Services.Scan.Interfaces;
    using HandBrake.Services.Scan.Model;
    using HandBrake.Startup;
    using HandBrake.Utilities;
    using HandBrake.Utilities.Interfaces;
    using HandBrake.ViewModels.Interfaces;
    using PlatformBindings;
    using PlatformBindings.Common;
    using PlatformBindings.Models.FileSystem;
    using Action = System.Action;
    using Execute = Caliburn.Micro.Execute;
    using LogManager = HandBrake.Helpers.LogManager;

    /// <summary>
    /// HandBrakes Main Window
    /// </summary>
    public class MainViewModelBase : ViewModelBase, IMainViewModel
    {
        #region Private Variables and Services

        protected readonly IQueueProcessor queueProcessor;
        protected readonly IPresetService presetService;
        protected readonly IErrorService errorService;
        protected readonly IUserSettingService userSettingService;
        protected readonly IScan scanService;
        protected readonly IDialogService dialogService;
        protected readonly IViewManager viewManager;
        protected readonly INotificationService notificationManager;
        protected readonly ISystemInfo systemInfo;
        protected readonly LauncherServiceBase launcher;
        protected readonly ITaskBarService taskbarService;

        private string windowName;
        private string sourceLabel;
        private string statusLabel;
        private string programStatusLabel;
        private Source scannedSource;
        private Title selectedTitle;
        private string duration;
        private bool isEncoding;
        private bool showStatusWindow;
        private Preset selectedPreset;
        private EncodeTask queueEditTask;
        private int lastEncodePercentage;
        private bool isPresetPanelShowing;
        private bool showSourceSelection;
        private BindingList<SourceMenuItem> drives;
        private bool canPause;
        private bool showAlertWindow;
        private string alertWindowHeader;
        private string alertWindowText;
        private bool hasSource;
        private bool isSettingPreset;
        private IPresetObject selectedPresetCategory;
        private bool isModifiedPreset;

        #endregion Private Variables and Services

        /// <summary>
        /// Initializes a new instance of the <see cref="MainViewModelBase"/> class.
        /// The viewmodel for HandBrakes main window.
        /// </summary>
        /// <param name="userSettingService">
        /// The User Setting Service
        /// </param>
        /// <param name="scanService">
        /// The scan Service.
        /// </param>
        /// <param name="presetService">
        /// The preset Service.
        /// </param>
        /// <param name="errorService">
        /// The Error Service
        /// </param>
        /// <param name="viewManager">
        /// The View Manager.
        /// </param>
        /// <param name="notificationManager">
        /// The Notification Manager.
        /// </param>
        /// <param name="queueProcessor">
        /// The queue processor.
        /// </param>
        /// <param name="whenDoneService">
        /// The when Done Service.
        /// *** Leave in Constructor. ***
        /// </param>
        /// <param name="pictureSettingsViewModel">
        /// The picture Settings View Model.
        /// </param>
        /// <param name="videoViewModel">
        /// The video View Model.
        /// </param>
        /// <param name="summaryViewModel">
        /// The summary view model.
        /// </param>
        /// <param name="filtersViewModel">
        /// The filters View Model.
        /// </param>
        /// <param name="audioViewModel">
        /// The audio View Model.
        /// </param>
        /// <param name="subtitlesViewModel">
        /// The subtitles View Model.
        /// </param>
        /// <param name="advancedViewModel">
        /// The advanced View Model.
        /// </param>
        /// <param name="chaptersViewModel">
        /// The chapters View Model.
        /// </param>
        /// <param name="staticPreviewViewModel">
        /// The static Preview View Model.
        /// </param>
        /// <param name="queueViewModel">
        /// The queue View Model.
        /// </param>
        /// <param name="metaDataViewModel">
        /// The Meta Data View Model
        /// </param>
        /// <param name="systemInfo">
        /// System Information.
        /// </param>
        /// <param name="launcher">
        /// The Launcher.
        /// </param>
        /// <param name="taskbarService">
        /// Manipulates the TaskBar.
        /// </param>
        public MainViewModelBase(IUserSettingService userSettingService, IScan scanService, IPresetService presetService,
            IErrorService errorService, IViewManager viewManager, INotificationService notificationManager, IQueueProcessor queueProcessor,
            IPrePostActionService whenDoneService, IPictureSettingsViewModel pictureSettingsViewModel, IVideoViewModel videoViewModel, ISummaryViewModel summaryViewModel,
            IFiltersViewModel filtersViewModel, IAudioViewModel audioViewModel, ISubtitlesViewModel subtitlesViewModel,
            IX264ViewModel advancedViewModel, IChaptersViewModel chaptersViewModel, IStaticPreviewViewModel staticPreviewViewModel,
            IQueueViewModel queueViewModel, IMetaDataViewModel metaDataViewModel, ISystemInfo systemInfo, LauncherServiceBase launcher, ITaskBarService taskbarService)
        {
            this.scanService = scanService;
            this.presetService = presetService;
            this.errorService = errorService;
            this.QueueViewModel = queueViewModel;
            this.userSettingService = userSettingService;
            this.queueProcessor = queueProcessor;
            this.viewManager = viewManager;
            this.notificationManager = notificationManager;
            this.systemInfo = systemInfo;
            this.launcher = launcher;
            this.taskbarService = taskbarService;

            this.SummaryViewModel = summaryViewModel;
            this.PictureSettingsViewModel = pictureSettingsViewModel;
            this.VideoViewModel = videoViewModel;
            this.MetaDataViewModel = metaDataViewModel;
            this.FiltersViewModel = filtersViewModel;
            this.AudioViewModel = audioViewModel;
            this.SubtitleViewModel = subtitlesViewModel;
            this.ChaptersViewModel = chaptersViewModel;
            this.AdvancedViewModel = advancedViewModel;
            this.StaticPreviewViewModel = staticPreviewViewModel;

            // Setup Properties
            this.WindowTitle = Resources.HandBrake_Title;
            this.CurrentTask = new EncodeTask();
            this.CurrentTask.PropertyChanged += this.CurrentTask_PropertyChanged;
            this.ScannedSource = new Source();
            this.HasSource = false;

            // Setup Events
            this.scanService.ScanStarted += this.ScanStared;
            this.scanService.ScanCompleted += this.ScanCompleted;
            this.scanService.ScanStatusChanged += this.ScanStatusChanged;
            this.queueProcessor.JobProcessingStarted += this.QueueProcessorJobProcessingStarted;
            this.queueProcessor.QueueCompleted += this.QueueCompleted;
            this.queueProcessor.QueueChanged += this.QueueChanged;
            this.queueProcessor.QueuePaused += this.QueueProcessor_QueuePaused;
            this.queueProcessor.EncodeService.EncodeStatusChanged += this.EncodeStatusChanged;
            this.userSettingService.SettingChanged += this.UserSettingServiceSettingChanged;

            this.PresetsCategories = new BindingList<IPresetObject>();
            this.Drives = new BindingList<SourceMenuItem>();

            LogManager.Init();
            HandBrakeInstanceManager.Init();
        }

        #region View Model Properties

        /// <summary>
        /// Gets or sets PictureSettingsViewModel.
        /// </summary>
        public IPictureSettingsViewModel PictureSettingsViewModel { get; set; }

        /// <summary>
        /// Gets or sets AudioViewModel.
        /// </summary>
        public IAudioViewModel AudioViewModel { get; set; }

        /// <summary>
        /// Gets or sets SubtitleViewModel.
        /// </summary>
        public ISubtitlesViewModel SubtitleViewModel { get; set; }

        /// <summary>
        /// Gets or sets ChaptersViewModel.
        /// </summary>
        public IChaptersViewModel ChaptersViewModel { get; set; }

        /// <summary>
        /// Gets or sets AdvancedViewModel.
        /// </summary>
        public IX264ViewModel AdvancedViewModel { get; set; }

        /// <summary>
        /// Gets or sets VideoViewModel.
        /// </summary>
        public IVideoViewModel VideoViewModel { get; set; }

        /// <summary>
        /// Gets or sets FiltersViewModel.
        /// </summary>
        public IFiltersViewModel FiltersViewModel { get; set; }

        /// <summary>
        /// Gets or sets the queue view model.
        /// </summary>
        public IQueueViewModel QueueViewModel { get; set; }

        /// <summary>
        /// Gets or sets the static preview view model.
        /// </summary>
        public IStaticPreviewViewModel StaticPreviewViewModel { get; set; }

        /// <summary>
        /// Gets or sets the The MetaData View Model
        /// </summary>
        public IMetaDataViewModel MetaDataViewModel { get; set; }

        public ISummaryViewModel SummaryViewModel { get; set; }

        #endregion View Model Properties

        #region Properties

        /// <summary>
        /// Gets or sets TestProperty.
        /// </summary>
        public string WindowTitle
        {
            get
            {
                return this.windowName;
            }

            set
            {
                if (!Equals(this.windowName, value))
                {
                    this.windowName = value;
                    this.NotifyOfPropertyChange(() => this.WindowTitle);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Program Status Toolbar Label
        /// This indicates the status of HandBrake
        /// </summary>
        public string ProgramStatusLabel
        {
            get
            {
                return string.IsNullOrEmpty(this.programStatusLabel) ? Resources.State_Ready : this.programStatusLabel;
            }

            set
            {
                if (!Equals(this.programStatusLabel, value))
                {
                    this.programStatusLabel = value;
                    this.NotifyOfPropertyChange(() => this.ProgramStatusLabel);
                }
            }
        }

        /// <summary>
        /// Gets or sets the Program Status Toolbar Label
        /// This indicates the status of HandBrake
        /// </summary>
        public string StatusLabel
        {
            get
            {
                return string.IsNullOrEmpty(this.statusLabel) ? Resources.State_Ready : this.statusLabel;
            }

            set
            {
                if (!Equals(this.statusLabel, value))
                {
                    this.statusLabel = value;
                    this.NotifyOfPropertyChange(() => this.StatusLabel);
                }
            }
        }

        /// <summary>
        /// Gets or sets Presets.
        /// </summary>
        public IEnumerable<IPresetObject> PresetsCategories { get; set; }

        public IPresetObject SelectedPresetCategory
        {
            get
            {
                return this.selectedPresetCategory;
            }
            set
            {
                if (!object.Equals(this.selectedPresetCategory, value))
                {
                    this.selectedPresetCategory = value;
                    this.NotifyOfPropertyChange(() => this.SelectedPresetCategory);
                    this.NotifyOfPropertyChange(() => this.CategoryPresets);
                }
            }
        }

        public IEnumerable<Preset> CategoryPresets
        {
            get
            {
                PresetDisplayCategory category = this.SelectedPresetCategory as PresetDisplayCategory;
                if (category != null && category.Presets != null)
                {
                    if (!category.Presets.Contains(this.SelectedPreset))
                    {
                        this.SelectedPreset = category.Presets.FirstOrDefault();
                    }

                    return new BindingList<Preset>(category.Presets);
                }

                this.SelectedPreset = null;
                return new BindingList<Preset>();
            }
        }

        /// <summary>
        /// Gets or sets SelectedPreset.
        /// </summary>
        public Preset SelectedPreset
        {
            get
            {
                return this.selectedPreset;
            }

            set
            {
                if (!object.Equals(this.selectedPreset, value))
                {
                    this.selectedPreset = value;
                    this.NotifyOfPropertyChange(() => this.SelectedPreset);

                    if (value != null)
                    {
                        this.PresetSelect(value);
                    }
                }
            }
        }

        public bool IsModifiedPreset
        {
            get
            {
                return this.isModifiedPreset;
            }
            set
            {
                if (value == this.isModifiedPreset) return;
                this.isModifiedPreset = value;
                this.NotifyOfPropertyChange();
            }
        }

        public void TrickPresetDisplayUpdate()
        {
            this.NotifyOfPropertyChange(() => this.SelectedPreset);
            this.selectedPreset.IsSelected = true;
        }

        /// <summary>
        /// Gets or sets The Current Encode Task that the user is building
        /// </summary>
        public EncodeTask CurrentTask { get; set; }

        /// <summary>
        /// Gets or sets the Last Scanned Source
        /// This object contains information about the scanned source.
        /// </summary>
        public Source ScannedSource
        {
            get
            {
                return this.scannedSource;
            }

            set
            {
                this.scannedSource = value;
                this.NotifyOfPropertyChange(() => ScannedSource);
            }
        }

        /// <summary>
        /// Gets or sets the title specific scan.
        /// </summary>
        public int TitleSpecificScan { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the encode serivce supports pausing.
        /// </summary>
        public bool CanPause
        {
            get
            {
                return this.canPause;
            }
            set
            {
                if (value.Equals(this.canPause))
                {
                    return;
                }
                this.canPause = value;
                this.NotifyOfPropertyChange(() => this.CanPause);
            }
        }

        /// <summary>
        /// Gets or sets the Source Label
        /// This indicates the status of scans.
        /// </summary>
        public string SourceLabel
        {
            get
            {
                return string.IsNullOrEmpty(this.sourceLabel) ? Resources.Main_SelectSource : this.sourceLabel;
            }

            set
            {
                if (!Equals(this.sourceLabel, value))
                {
                    this.sourceLabel = value;
                    this.NotifyOfPropertyChange(() => SourceLabel);
                }
            }
        }

        /// <summary>
        /// Gets SourceName.
        /// </summary>
        public string SourceName
        {
            get
            {
                // Sanity Check
                if (ScannedSource == null || ScannedSource.ScanPath == null)
                {
                    return string.Empty;
                }

                // The title that is selected has a source name. This means it's part of a batch scan.
                if (selectedTitle != null && !string.IsNullOrEmpty(selectedTitle.SourceName) && !selectedTitle.SourceName.EndsWith("\\"))
                {
                    return Path.GetFileNameWithoutExtension(selectedTitle.SourceName);
                }

                // Check if we have a Folder, if so, check if it's a DVD / Bluray drive and get the label.
                if (ScannedSource.ScanPath.EndsWith("\\"))
                {
                    foreach (DriveInformation item in DriveUtilities.GetDrives())
                    {
                        if (item.RootDirectory.Contains(this.ScannedSource.ScanPath.Replace("\\\\", "\\")))
                        {
                            return item.VolumeLabel;
                        }
                    }
                }

                if (Path.GetFileNameWithoutExtension(this.ScannedSource.ScanPath) != "VIDEO_TS")
                    return Path.GetFileNameWithoutExtension(this.ScannedSource.ScanPath);

                return Path.GetFileNameWithoutExtension(Path.GetDirectoryName(this.ScannedSource.ScanPath));
            }
        }

        /// <summary>
        /// Gets RangeMode.
        /// </summary>
        public IEnumerable<PointToPointMode> RangeMode
        {
            get
            {
                return new List<PointToPointMode>
                    {
                        PointToPointMode.Chapters, PointToPointMode.Seconds, PointToPointMode.Frames
                    };
            }
        }

        /// <summary>
        /// Gets a value indicating whether ShowTextEntryForPointToPointMode.
        /// </summary>
        public bool ShowTextEntryForPointToPointMode
        {
            get
            {
                return this.SelectedPointToPoint != PointToPointMode.Chapters;
            }
        }

        /// <summary>
        /// Gets StartEndRangeItems.
        /// </summary>
        public IEnumerable<int> StartEndRangeItems
        {
            get
            {
                if (this.SelectedTitle == null)
                {
                    return null;
                }

                return this.SelectedTitle.Chapters.Select(item => item.ChapterNumber).Select(dummy => dummy).ToList();
            }
        }

        /// <summary>
        /// Gets Angles.
        /// </summary>
        public IEnumerable<int> Angles
        {
            get
            {
                if (this.SelectedTitle == null)
                {
                    return null;
                }

                List<int> items = new List<int>();
                for (int i = 1; i <= this.selectedTitle.AngleCount; i++)
                {
                    items.Add(i);
                }
                return items;
            }
        }

        /// <summary>
        /// Gets or sets Duration.
        /// </summary>
        public string Duration
        {
            get
            {
                return string.IsNullOrEmpty(duration) ? "--:--:--" : duration;
            }
            set
            {
                duration = value;
                this.NotifyOfPropertyChange(() => Duration);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether IsEncoding.
        /// </summary>
        public bool IsEncoding
        {
            get
            {
                return this.isEncoding;
            }

            set
            {
                this.isEncoding = value;
                this.CanPause = value;
                this.NotifyOfPropertyChange(() => this.IsEncoding);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether ShowStatusWindow.
        /// </summary>
        public bool ShowStatusWindow
        {
            get
            {
                return this.showStatusWindow;
            }

            set
            {
                this.showStatusWindow = value;
                this.NotifyOfPropertyChange(() => this.ShowStatusWindow);
            }
        }

        /// <summary>
        /// Gets RangeMode.
        /// </summary>
        public IEnumerable<OutputFormat> OutputFormats
        {
            get
            {
                return new List<OutputFormat>
                    {
                         OutputFormat.Mp4, OutputFormat.Mkv
                    };
            }
        }

        /// <summary>
        /// Gets or sets Destination.
        /// </summary>
        public FileData Destination
        {
            get
            {
                return this.CurrentTask.Destination;
            }
            set
            {
                if (!Equals(this.CurrentTask.Destination, value))
                {
                    if (value != null)
                    {
                        string ext = string.Empty;
                        try
                        {
                            ext = Path.GetExtension(value.Path);
                            if (FileHelper.FilePathHasInvalidChars(value.Path))
                            {
                                this.errorService.ShowMessageBox(Resources.Main_InvalidDestination, Resources.Error, DialogButtonType.OK, DialogType.Error);
                                return;
                            }
                        }
                        catch (Exception exc)
                        {
                            this.errorService.ShowError(Resources.Main_InvalidDestination, string.Empty, value + Environment.NewLine + exc);
                            return;
                        }

                        this.CurrentTask.Destination = value;
                        this.NotifyOfPropertyChange(() => this.Destination);

                        switch (ext)
                        {
                            case ".mkv":
                                this.SummaryViewModel.SetContainer(OutputFormat.Mkv);
                                break;

                            case ".mp4":
                            case ".m4v":
                                this.SummaryViewModel.SetContainer(OutputFormat.Mp4);
                                break;
                        }
                    }
                    else
                    {
                        this.CurrentTask.Destination = null;
                        this.NotifyOfPropertyChange(() => this.Destination);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets SelectedTitle.
        /// </summary>
        public Title SelectedTitle
        {
            get
            {
                return this.selectedTitle;
            }
            set
            {
                if (!Equals(this.selectedTitle, value))
                {
                    this.selectedTitle = value;

                    if (this.selectedTitle == null)
                    {
                        return;
                    }

                    // Use the Path on the Title, or the Source Scan path if one doesn't exist.
                    this.SourceLabel = this.SourceName;
                    this.CurrentTask.Source = !string.IsNullOrEmpty(this.selectedTitle.SourceName) ? this.selectedTitle.SourceName : this.ScannedSource.ScanPath;
                    this.CurrentTask.Title = value.TitleNumber;
                    this.NotifyOfPropertyChange(() => this.StartEndRangeItems);
                    this.NotifyOfPropertyChange(() => this.SelectedTitle);
                    this.NotifyOfPropertyChange(() => this.Angles);
                    this.NotifyOfPropertyChange(() => this.SourceInfo);

                    // Default the Start and End Point dropdowns
                    this.SelectedStartPoint = 1;
                    this.SelectedEndPoint = this.selectedTitle.Chapters != null &&
                                            this.selectedTitle.Chapters.Count != 0
                                                ? this.selectedTitle.Chapters.Last().ChapterNumber
                                                : 1;

                    this.SelectedPointToPoint = PointToPointMode.Chapters;
                    this.SelectedAngle = 1;

                    if (this.userSettingService.GetUserSetting<bool>(UserSettingConstants.AutoNaming))
                    {
                        if (this.userSettingService.GetUserSetting<string>(UserSettingConstants.AutoNameFormat) != null)
                        {
                            // TODO: Verify Valid
                            this.Destination.File = AppServices.Current?.IO?.CreateFile(AutoNameHelper.AutoName(this.CurrentTask, this.SourceName, this.selectedPreset))?.Result;
                        }
                    }
                    this.NotifyOfPropertyChange(() => this.CurrentTask);

                    this.Duration = this.DurationCalculation();

                    // Setup the tab controls
                    this.SetupTabs();
                }
            }
        }

        /// <summary>
        /// Gets or sets SelectedAngle.
        /// </summary>
        public int SelectedAngle
        {
            get
            {
                return this.CurrentTask.StartPoint;
            }

            set
            {
                this.CurrentTask.Angle = value;
                this.NotifyOfPropertyChange(() => this.SelectedAngle);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether is timespan range.
        /// </summary>
        public bool IsTimespanRange { get; set; }

        /// <summary>
        /// Gets or sets SelectedStartPoint.
        /// </summary>
        public int SelectedStartPoint
        {
            get
            {
                return this.CurrentTask.StartPoint;
            }
            set
            {
                this.CurrentTask.StartPoint = value;
                this.NotifyOfPropertyChange(() => this.SelectedStartPoint);
                this.Duration = this.DurationCalculation();

                if (this.userSettingService.GetUserSetting<bool>(UserSettingConstants.AutoNaming) && this.ScannedSource.ScanPath != null)
                {
                    if (this.SelectedPointToPoint == PointToPointMode.Chapters && this.userSettingService.GetUserSetting<string>(UserSettingConstants.AutoNameFormat) != null &&
                        this.userSettingService.GetUserSetting<string>(UserSettingConstants.AutoNameFormat).Contains(Constants.Chapters))
                    {
                        // TODO: Verify Valid
                        this.Destination.File = AppServices.Current?.IO?.CreateFile(AutoNameHelper.AutoName(this.CurrentTask, this.SourceName, this.selectedPreset))?.Result;
                    }
                }

                if (this.SelectedStartPoint > this.SelectedEndPoint)
                {
                    this.SelectedEndPoint = this.SelectedStartPoint;
                }
            }
        }

        /// <summary>
        /// Gets or sets SelectedEndPoint.
        /// </summary>
        public int SelectedEndPoint
        {
            get
            {
                return this.CurrentTask.EndPoint;
            }
            set
            {
                this.CurrentTask.EndPoint = value;
                this.NotifyOfPropertyChange(() => this.SelectedEndPoint);
                this.Duration = this.DurationCalculation();

                if (this.SelectedPointToPoint == PointToPointMode.Chapters && this.userSettingService.GetUserSetting<string>(UserSettingConstants.AutoNameFormat) != null &&
                    this.userSettingService.GetUserSetting<string>(UserSettingConstants.AutoNameFormat).Contains(Constants.Chapters))
                {
                    // TODO: Verify Valid
                    this.Destination.File = AppServices.Current?.IO?.CreateFile(AutoNameHelper.AutoName(this.CurrentTask, this.SourceName, this.selectedPreset))?.Result;
                }

                if (this.SelectedStartPoint > this.SelectedEndPoint && this.SelectedPointToPoint == PointToPointMode.Chapters)
                {
                    this.SelectedStartPoint = this.SelectedEndPoint;
                }
            }
        }

        /// <summary>
        /// Gets or sets SelectedPointToPoint.
        /// </summary>
        public PointToPointMode SelectedPointToPoint
        {
            get
            {
                return this.CurrentTask.PointToPointMode;
            }
            set
            {
                this.CurrentTask.PointToPointMode = value;
                this.NotifyOfPropertyChange(() => SelectedPointToPoint);
                this.NotifyOfPropertyChange(() => ShowTextEntryForPointToPointMode);

                if (value == PointToPointMode.Chapters && this.SelectedTitle != null)
                {
                    if (this.selectedTitle == null)
                    {
                        return;
                    }

                    this.SelectedStartPoint = 1;
                    this.SelectedEndPoint = selectedTitle.Chapters != null && selectedTitle.Chapters.Count > 0 ? selectedTitle.Chapters.Last().ChapterNumber : 1;
                }
                else if (value == PointToPointMode.Seconds)
                {
                    if (this.selectedTitle == null)
                    {
                        return;
                    }

                    this.SelectedStartPoint = 0;

                    int timeInSeconds;
                    if (int.TryParse(Math.Round(selectedTitle.Duration.TotalSeconds, 0).ToString(CultureInfo.InvariantCulture), out timeInSeconds))
                    {
                        this.SelectedEndPoint = timeInSeconds;
                    }

                    this.IsTimespanRange = true;
                    this.NotifyOfPropertyChange(() => this.IsTimespanRange);
                }
                else
                {
                    if (this.selectedTitle == null)
                    {
                        return;
                    }

                    // Note this does not account for VFR. It's only a guesstimate.
                    double estimatedTotalFrames = selectedTitle.Fps * selectedTitle.Duration.TotalSeconds;

                    this.SelectedStartPoint = 0;
                    int totalFrames;
                    if (int.TryParse(Math.Round(estimatedTotalFrames, 0).ToString(CultureInfo.InvariantCulture), out totalFrames))
                    {
                        this.SelectedEndPoint = totalFrames;
                    }

                    this.IsTimespanRange = false;
                    this.NotifyOfPropertyChange(() => this.IsTimespanRange);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether show advanced tab.
        /// </summary>
        public bool ShowAdvancedTab
        {
            get
            {
                return this.userSettingService.GetUserSetting<bool>(UserSettingConstants.ShowAdvancedTab);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether is preset panel showing.
        /// </summary>
        public bool IsPresetPanelShowing
        {
            get
            {
                return this.isPresetPanelShowing;
            }
            set
            {
                if (!Equals(this.isPresetPanelShowing, value))
                {
                    this.isPresetPanelShowing = value;
                    this.NotifyOfPropertyChange(() => this.IsPresetPanelShowing);

                    // Save the setting if it has changed.
                    if (this.userSettingService.GetUserSetting<bool>(UserSettingConstants.ShowPresetPanel) != value)
                    {
                        this.userSettingService.SetUserSetting(UserSettingConstants.ShowPresetPanel, value);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating progress percentage.
        /// </summary>
        public int ProgressPercentage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether show source selection.
        /// </summary>
        public bool ShowSourceSelection
        {
            get
            {
                return this.showSourceSelection;
            }
            set
            {
                if (value.Equals(this.showSourceSelection))
                {
                    return;
                }
                this.showSourceSelection = value;
                this.NotifyOfPropertyChange(() => this.ShowSourceSelection);

                // Refresh the drives.
                if (this.showSourceSelection && systemInfo.SupportsOpticalDrive)
                {
                    this.Drives.Clear();
                    foreach (SourceMenuItem menuItem in from item in DriveUtilities.GetDrives()
                                                        let driveInformation = item
                                                        select new SourceMenuItem
                                                        {
                                                            Text = string.Format("{0} ({1})", item.RootDirectory, item.VolumeLabel),
                                                            DriveLetter = item.RootDirectory,
                                                            Action = () => this.ProcessDrive(driveInformation),
                                                            Tag = item,
                                                            IsDrive = true,
                                                        })
                    {
                        this.Drives.Add(menuItem);
                    }

                    this.TitleSpecificScan = 0;
                    this.NotifyOfPropertyChange(() => this.HasOpticalDrives);
                    this.NotifyOfPropertyChange(() => this.TitleSpecificScan);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether there are any Optical Drives.
        /// </summary>
        public bool HasOpticalDrives
        {
            get
            {
                return systemInfo.SupportsOpticalDrive && this.Drives.Any();
            }
        }

        /// <summary>
        /// Gets or sets the drives.
        /// </summary>
        public BindingList<SourceMenuItem> Drives
        {
            get
            {
                return this.drives;
            }
            set
            {
                if (Equals(value, this.drives))
                {
                    return;
                }

                this.drives = value;
                this.NotifyOfPropertyChange(() => this.Drives);
            }
        }

        /// <summary>
        /// Gets the cancel action.
        /// </summary>
        public Action CancelAction
        {
            get
            {
                return this.CancelScan;
            }
        }

        /// <summary>
        /// Action for the status window.
        /// </summary>
        public Action OpenLogWindowAction
        {
            get
            {
                return this.OpenLogWindow;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether show alert window.
        /// </summary>
        public bool ShowAlertWindow
        {
            get
            {
                return this.showAlertWindow;
            }
            set
            {
                if (value.Equals(this.showAlertWindow))
                {
                    return;
                }
                this.showAlertWindow = value;
                this.NotifyOfPropertyChange(() => this.ShowAlertWindow);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether alert window header.
        /// </summary>
        public string AlertWindowHeader
        {
            get
            {
                return this.alertWindowHeader;
            }
            set
            {
                if (value == this.alertWindowHeader)
                {
                    return;
                }
                this.alertWindowHeader = value;
                this.NotifyOfPropertyChange(() => this.AlertWindowHeader);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether alert window text.
        /// </summary>
        public string AlertWindowText
        {
            get
            {
                return this.alertWindowText;
            }
            set
            {
                if (value == this.alertWindowText)
                {
                    return;
                }
                this.alertWindowText = value;
                this.NotifyOfPropertyChange(() => this.AlertWindowText);
            }
        }

        /// <summary>
        /// Gets the alert window close.
        /// </summary>
        public Action AlertWindowClose
        {
            get
            {
                return this.CloseAlertWindow;
            }
        }

        /// <summary>
        /// Gets the add to queue label.
        /// </summary>
        public string QueueLabel
        {
            get
            {
                return string.Format(Resources.Main_QueueLabel, this.queueProcessor.Count > 0 ? string.Format(" ({0})", this.queueProcessor.Count) : string.Empty);
            }
        }

        /// <summary>
        /// Gets the start label.
        /// </summary>
        public string StartLabel
        {
            get
            {
                return this.queueProcessor.Count > 0 ? Resources.Main_StartQueue : Resources.Main_Start;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether has source.
        /// </summary>
        public bool HasSource
        {
            get
            {
                return this.hasSource;
            }

            set
            {
                if (value.Equals(this.hasSource))
                {
                    return;
                }

                this.hasSource = value;
                this.NotifyOfPropertyChange(() => this.HasSource);
            }
        }

        /// <summary>
        /// Flag to indicate if the queue is showing on the main view. (I.e  inline queue display)
        /// </summary>
        public bool IsQueueShowingInLine { get; set; } = false;

        public string SourceInfo
        {
            get
            {
                if (this.SelectedTitle != null)
                {
                    int parW = this.SelectedTitle.ParVal.Width;
                    int parH = this.SelectedTitle.ParVal.Height;
                    int displayW = this.SelectedTitle.Resolution.Width * parW / parH;

                    return string.Format("{0}x{1} ({2}x{3}), {4} FPS, {5} {6}, {7} {8}",
                        this.SelectedTitle.Resolution.Width,
                        this.SelectedTitle.Resolution.Height,
                        displayW,
                        this.SelectedTitle.Resolution.Height,
                        Math.Round(this.SelectedTitle.Fps, 2),
                        this.SelectedTitle.AudioTracks.Count,
                        ResourcesUI.MainView_AudioTrackCount,
                        this.SelectedTitle.Subtitles.Count,
                        ResourcesUI.MainView_SubtitleTracksCount);
                }

                return string.Empty;
            }
        }

        #endregion Properties

        #region Load and Shutdown Handling

        /// <summary>
        /// Initialise this view model.
        /// </summary>
        public override void OnLoad()
        {
            // Show or Hide the Preset Panel.
            this.IsPresetPanelShowing = this.userSettingService.GetUserSetting<bool>(UserSettingConstants.ShowPresetPanel);

            // Setup the presets.
            this.presetService.Load();
            this.PresetsCategories = this.presetService.Presets;
            this.NotifyOfPropertyChange(() => this.PresetsCategories);
            this.presetService.LoadCategoryStates();

            this.SummaryViewModel.OutputFormatChanged += this.SummaryViewModel_OutputFormatChanged;

            // Queue Recovery
            bool queueRecovered = QueueRecoveryHelper.RecoverQueue(this.queueProcessor, this.errorService, StartupOptions.AutoRestartQueue);

            // If the queue is not recovered, show the source selection window by default.
            if (!queueRecovered)
            {
                this.ShowSourceSelection = true;
            }
            else
            {
                this.HasSource = true; // Enable the GUI. Needed for in-line queue.
            }

            // If the user has enabled --auto-start-queue, start the queue.
            if (StartupOptions.AutoRestartQueue && !this.queueProcessor.IsProcessing && this.queueProcessor.Count > 0)
            {
                this.queueProcessor.Start(this.userSettingService.GetUserSetting<bool>(UserSettingConstants.ClearCompletedFromQueue));
            }

            // Preset Selection
            this.SetDefaultPreset();

            // Reset WhenDone if necessary.
            if (this.userSettingService.GetUserSetting<bool>(UserSettingConstants.ResetWhenDoneAction))
            {
                this.WhenDone("Do nothing");
            }

            // Log Cleaning
            if (this.userSettingService.GetUserSetting<bool>(UserSettingConstants.ClearOldLogs))
            {
                Thread clearLog = new Thread(() => GeneralUtilities.ClearLogFiles(30));
                clearLog.Start();
            }

            this.PictureSettingsViewModel.TabStatusChanged += this.TabStatusChanged;
            this.VideoViewModel.TabStatusChanged += this.TabStatusChanged;
            this.FiltersViewModel.TabStatusChanged += this.TabStatusChanged;
            this.AudioViewModel.TabStatusChanged += this.TabStatusChanged;
            this.SubtitleViewModel.TabStatusChanged += this.TabStatusChanged;
            this.ChaptersViewModel.TabStatusChanged += this.TabStatusChanged;
            this.AdvancedViewModel.TabStatusChanged += this.TabStatusChanged;
            this.MetaDataViewModel.TabStatusChanged += this.TabStatusChanged;
            this.SummaryViewModel.TabStatusChanged += this.TabStatusChanged;
        }

        private void SummaryViewModel_OutputFormatChanged(object sender, OutputFormatChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Extension))
            {
                var newname = Path.ChangeExtension(this.Destination.Path, e.Extension);
                var result = AsyncHelpers.GetThreadedResult(() => this.Destination.File.RenameAsync(newname));

                // updates path.
                this.Destination.Path = this.Destination.File.Path;
            }

            this.VideoViewModel.RefreshTask();
            this.AudioViewModel.RefreshTask();
        }

        /// <summary>
        /// Shutdown this View
        /// </summary>
        public void Shutdown()
        {
            // Shutdown Service
            this.queueProcessor.Stop();
            this.presetService.SaveCategoryStates();

            // Unsubscribe from Events.
            this.scanService.ScanStarted -= this.ScanStared;
            this.scanService.ScanCompleted -= this.ScanCompleted;
            this.scanService.ScanStatusChanged -= this.ScanStatusChanged;
            this.queueProcessor.QueuePaused -= this.QueueProcessor_QueuePaused;
            this.queueProcessor.QueueCompleted -= this.QueueCompleted;
            this.queueProcessor.QueueChanged -= this.QueueChanged;

            this.queueProcessor.JobProcessingStarted -= this.QueueProcessorJobProcessingStarted;
            this.queueProcessor.EncodeService.EncodeStatusChanged -= this.EncodeStatusChanged;
            this.userSettingService.SettingChanged -= this.UserSettingServiceSettingChanged;

            this.SummaryViewModel.OutputFormatChanged -= this.SummaryViewModel_OutputFormatChanged;

            // Tab status events
            this.PictureSettingsViewModel.TabStatusChanged -= this.TabStatusChanged;
            this.VideoViewModel.TabStatusChanged -= this.TabStatusChanged;
            this.FiltersViewModel.TabStatusChanged -= this.TabStatusChanged;
            this.AudioViewModel.TabStatusChanged -= this.TabStatusChanged;
            this.SubtitleViewModel.TabStatusChanged -= this.TabStatusChanged;
            this.ChaptersViewModel.TabStatusChanged -= this.TabStatusChanged;
            this.AdvancedViewModel.TabStatusChanged -= this.TabStatusChanged;
            this.MetaDataViewModel.TabStatusChanged -= this.TabStatusChanged;
            this.SummaryViewModel.TabStatusChanged -= this.TabStatusChanged;
        }

        #endregion Load and Shutdown Handling

        #region Menu and Taskbar

        /// <summary>
        /// Open the About Window
        /// </summary>
        public void OpenAboutApplication()
        {
            this.viewManager.OpenOptions(OptionsTab.About);
        }

        /// <summary>
        /// Open the Options Window
        /// </summary>
        public void OpenOptionsWindow()
        {
            IShellViewModel shellViewModel = IoC.Get<IShellViewModel>();
            shellViewModel.DisplayWindow(ShellWindow.OptionsWindow);
        }

        /// <summary>
        /// Open the Log Window
        /// </summary>
        public void OpenLogWindow()
        {
            if (this.viewManager.SupportsWindow)
            {
                this.viewManager.ShowWindow<ILogViewModel>();
            }
        }

        /// <summary>
        /// Open the Queue Window.
        /// </summary>
        public void OpenQueueWindow()
        {
            var supportsWindow = viewManager.SupportsWindow;

            bool showQueueInline = supportsWindow ? this.userSettingService.GetUserSetting<bool>(UserSettingConstants.ShowQueueInline) : true;
            showQueueInline = supportsWindow ? false : true; // Disabled until it's evaluated.

            if (showQueueInline)
            {
                this.IsQueueShowingInLine = !this.IsQueueShowingInLine;
                if (this.IsQueueShowingInLine)
                {
                    this.QueueViewModel.Activate();
                }
                else
                {
                    this.QueueViewModel.Deactivate();
                }
                this.NotifyOfPropertyChange(() => this.IsQueueShowingInLine);
            }
            else
            {
                this.IsQueueShowingInLine = false;
                this.NotifyOfPropertyChange(() => this.IsQueueShowingInLine);

                this.viewManager.ShowWindow<IQueueViewModel>();
            }
        }

        /// <summary>
        /// Open the Queue Window.
        /// </summary>
        public void OpenPreviewWindow()
        {
            if (!string.IsNullOrEmpty(this.CurrentTask.Source) && this.viewManager.SupportsWindow)
            {
                this.StaticPreviewViewModel.IsOpen = true;
                this.StaticPreviewViewModel.UpdatePreviewFrame(this.CurrentTask, this.ScannedSource);
                this.viewManager.ShowWindow(this.StaticPreviewViewModel);
            }
        }

        /// <summary>
        /// Launch the Help pages.
        /// </summary>
        public void LaunchHelp()
        {
            try
            {
                AppServices.Current.UI.OpenLink(new Uri("https://handbrake.fr/docs"));
            }
            catch (Exception exc)
            {
                this.errorService.ShowError(Resources.Main_UnableToLoadHelpMessage, Resources.Main_UnableToLoadHelpSolution, exc);
            }
        }

        /// <summary>
        /// Add the current task to the queue.
        /// </summary>
        /// <returns>
        /// True if added, false if error.
        /// </returns>
        public bool AddToQueue()
        {
            if (this.ScannedSource == null || string.IsNullOrEmpty(this.ScannedSource.ScanPath) || this.ScannedSource.Titles.Count == 0)
            {
                this.errorService.ShowMessageBox(Resources.Main_ScanSource, Resources.Error, DialogButtonType.OK, DialogType.Error);
                return false;
            }

            if (this.CurrentTask.Destination == null)
            {
                this.errorService.ShowMessageBox(Resources.Main_SetDestination, Resources.Error, DialogButtonType.OK, DialogType.Error);
                return false;
            }

            if (!DirectoryUtilities.IsWritable(Path.GetDirectoryName(this.CurrentTask.Destination.Path), true, this.errorService))
            {
                this.errorService.ShowMessageBox(Resources.Main_NoPermissionsOrMissingDirectory, Resources.Error, DialogButtonType.OK, DialogType.Error);
                return false;
            }

            // Sanity check the filename
            if (!string.IsNullOrEmpty(this.Destination.Path) && FileHelper.FilePathHasInvalidChars(this.Destination.Path))
            {
                this.errorService.ShowMessageBox(Resources.Main_InvalidDestination, Resources.Error, DialogButtonType.OK, DialogType.Error);
                this.NotifyOfPropertyChange(() => this.Destination);
                return false;
            }

            if (this.Destination.Path == this.ScannedSource.ScanPath)
            {
                this.errorService.ShowMessageBox(Resources.Main_SourceDestinationMatchError, Resources.Error, DialogButtonType.OK, DialogType.Error);
                this.Destination = null;
                return false;
            }

            if (this.scannedSource != null && !string.IsNullOrEmpty(this.scannedSource.ScanPath) && this.Destination.Path.ToLower() == this.scannedSource.ScanPath.ToLower())
            {
                this.errorService.ShowMessageBox(Resources.Main_MatchingFileOverwriteWarning, Resources.Error, DialogButtonType.OK, DialogType.Error);
                return false;
            }

            QueueTask task = new QueueTask(new EncodeTask(this.CurrentTask), HBConfigurationFactory.Create(), this.ScannedSource.ScanPath);

            if (!this.queueProcessor.CheckForDestinationPathDuplicates(task.Task.Destination.Path))
            {
                this.queueProcessor.Add(task);
            }
            else
            {
                this.errorService.ShowMessageBox(Resources.Main_DuplicateDestinationOnQueue, Resources.Error, DialogButtonType.OK, DialogType.Warning);
                return false;
            }

            if (!this.IsEncoding)
            {
                this.ProgramStatusLabel = string.Format(Resources.Main_XEncodesPending, this.queueProcessor.Count);
            }

            return true;
        }

        /// <summary>
        /// Add all Items to the queue
        /// </summary>
        public void AddAllToQueue()
        {
            if (this.ScannedSource == null || this.ScannedSource.Titles == null || this.ScannedSource.Titles.Count == 0)
            {
                this.errorService.ShowMessageBox(Resources.Main_ScanSource, Resources.Error, DialogButtonType.OK, DialogType.Error);
                return;
            }

            if (!AutoNameHelper.IsAutonamingEnabled())
            {
                this.errorService.ShowMessageBox(Resources.Main_TurnOnAutoFileNaming, Resources.Error, DialogButtonType.OK, DialogType.Error);
                return;
            }

            if (this.CurrentTask != null && this.CurrentTask.SubtitleTracks != null && this.CurrentTask.SubtitleTracks.Count > 0)
            {
                if (this.SubtitleViewModel.SubtitleBehaviours == null || this.SubtitleViewModel.SubtitleBehaviours.SelectedBehaviour == SubtitleBehaviourModes.None)
                {
                    var result = this.errorService.ShowMessageBox(
                        Resources.Main_AutoAdd_AudioAndSubWarning,
                        Resources.Warning,
                        DialogButtonType.YesNo,
                        DialogType.Warning);

                    if (result == DialogResult.No)
                    {
                        return;
                    }
                }
            }

            foreach (Title title in this.ScannedSource.Titles)
            {
                this.SelectedTitle = title;
                this.AddToQueue();
            }
        }

        /// <summary>
        /// The add selection to queue.
        /// </summary>
        public void AddSelectionToQueue()
        {
            if (this.ScannedSource == null || this.ScannedSource.Titles == null || this.ScannedSource.Titles.Count == 0)
            {
                this.errorService.ShowMessageBox(Resources.Main_ScanSource, Resources.Error, DialogButtonType.OK, DialogType.Error);
                return;
            }

            if (!AutoNameHelper.IsAutonamingEnabled())
            {
                this.errorService.ShowMessageBox(Resources.Main_TurnOnAutoFileNaming, Resources.Error, DialogButtonType.OK, DialogType.Error);
                return;
            }

            IQueueSelectionViewModel viewModel = IoC.Get<IQueueSelectionViewModel>();

            viewModel.Setup(this.ScannedSource, this.SourceName, (tasks) =>
            {
                foreach (SelectionTitle title in tasks)
                {
                    this.SelectedTitle = title.Title;
                    this.AddToQueue();
                }
            }, this.selectedPreset);

            if (this.viewManager.SupportsWindow)
            {
                this.viewManager.ShowWindow(viewModel);
            }
        }

        /// <summary>
        /// Folder Scan
        /// </summary>
        public void FolderScan()
        {
            AppServices.Current?.IO?.Pickers?.PickFolder()?.ContinueWith(task =>
            {
                var folder = task.Result;
                if (folder != null)
                {
                    this.StartScan(folder.Path, this.TitleSpecificScan);
                }
            });
        }

        /// <summary>
        /// File Scan
        /// </summary>
        public void FileScan()
        {
            AppServices.Current?.IO?.Pickers?.PickFile()?.ContinueOnUIThread(task =>
            {
                var file = task.Result;
                if (file != null)
                {
                    this.StartScan(file.Path, this.TitleSpecificScan);
                }
            });
        }

        /// <summary>
        /// Cancel a Scan
        /// </summary>
        public void CancelScan()
        {
            this.ShowStatusWindow = false;
            this.scanService.Cancel();
        }

        /// <summary>
        /// Start an Encode
        /// </summary>
        public void StartEncode()
        {
            Task.Run(() =>
            {
                if (this.queueProcessor.IsProcessing)
                {
                    this.IsEncoding = true;
                    return;
                }

                // Check if we already have jobs, and if we do, just start the queue.
                if (this.queueProcessor.Count != 0 || this.queueProcessor.EncodeService.IsPasued)
                {
                    if (this.queueProcessor.EncodeService.IsPasued)
                    {
                        this.IsEncoding = true;
                    }

                    this.queueProcessor.Start(this.userSettingService.GetUserSetting<bool>(UserSettingConstants.ClearCompletedFromQueue));
                    return;
                }

                // Otherwise, perform Santiy Checking then add to the queue and start if everything is ok.
                if (this.SelectedTitle == null)
                {
                    this.errorService.ShowMessageBox(Resources.Main_ScanSource, Resources.Error, DialogButtonType.OK, DialogType.Error);
                    return;
                }

                if (this.Destination == null)
                {
                    this.errorService.ShowMessageBox(Resources.Main_ChooseDestination, Resources.Error, DialogButtonType.OK, DialogType.Error);
                    return;
                }

                if (!DriveUtilities.HasMinimumDiskSpace(
                    this.Destination.Path,
                    this.userSettingService.GetUserSetting<long>(UserSettingConstants.PauseOnLowDiskspaceLevel)))
                {
                    this.errorService.ShowMessageBox(Resources.Main_LowDiskspace, Resources.Error, DialogButtonType.OK, DialogType.Error);
                    return;
                }

                if (this.scannedSource != null && !string.IsNullOrEmpty(this.scannedSource.ScanPath) && this.Destination.Path.ToLower() == this.scannedSource.ScanPath.ToLower())
                {
                    this.errorService.ShowMessageBox(Resources.Main_MatchingFileOverwriteWarning, Resources.Error, DialogButtonType.OK, DialogType.Error);
                    return;
                }

                if (File.Exists(this.Destination.Path))
                {
                    var result = this.errorService.ShowMessageBox(Resources.Main_DestinationOverwrite, Resources.Question, DialogButtonType.YesNo, DialogType.Question);
                    if (result == DialogResult.No)
                    {
                        return;
                    }
                }

                // Create the Queue Task and Start Processing
                if (this.AddToQueue())
                {
                    this.IsEncoding = true;
                    this.queueProcessor.Start(this.userSettingService.GetUserSetting<bool>(UserSettingConstants.ClearCompletedFromQueue));
                }
            });
        }

        /// <summary>
        /// Edit a Queue Task
        /// </summary>
        /// <param name="task">
        /// The task.
        /// </param>
        public void EditQueueJob(EncodeTask task)
        {
            // Rescan the source to make sure it's still valid
            this.queueEditTask = task;
            this.scanService.Scan(task.Source, task.Title, QueueEditAction, HBConfigurationFactory.Create());
        }

        /// <summary>
        /// Pause an Encode
        /// </summary>
        public void PauseEncode()
        {
            this.queueProcessor.PauseEncode();
            this.IsEncoding = false;
        }

        /// <summary>
        /// Stop an Encode.
        /// </summary>
        public void StopEncode()
        {
            this.queueProcessor.Stop();
        }

        /// <summary>
        /// Shutdown the Application
        /// </summary>
        public void ExitApplication()
        {
            var systemState = IoC.Get<ISystemStateService>();
            systemState.Quit();
        }

        /// <summary>
        /// The select source window.
        /// </summary>
        public void SelectSourceWindow()
        {
            ShowSourceSelection = !ShowSourceSelection;
        }

        /// <summary>
        /// The close source selection.
        /// </summary>
        public void CloseSourceSelection()
        {
            this.ShowSourceSelection = false;
        }

        /// <summary>
        /// The close alert window.
        /// </summary>
        public void CloseAlertWindow()
        {
            this.ShowAlertWindow = false;
            this.AlertWindowText = string.Empty;
            this.AlertWindowHeader = string.Empty;
        }

        /// <summary>
        /// Pass on the "When Done" Action to the queue view model.
        /// </summary>
        /// <param name="action">action</param>
        public void WhenDone(string action)
        {
            this.QueueViewModel?.WhenDone(action, true);
        }

        #endregion Menu and Taskbar

        #region Main Window Public Methods

        /// <summary>
        /// The Destination Path
        /// </summary>
        public void BrowseDestination()
        {
            string mp4 = ".mp4", m4v = ".m4v", mkv = ".mkv";

            var properties = new FileSavePickerProperties();

            properties.FileTypes.Add(mp4);
            properties.FileTypes.Add(m4v);
            properties.FileTypes.Add(mkv);

            string extension = this.CurrentTask.Destination != null ? Path.GetExtension(this.CurrentTask.Destination.Path) : null;
            var defaultext = this.CurrentTask.Destination != null && !string.IsNullOrEmpty(extension) ?
                extension : (this.CurrentTask.OutputFormat == OutputFormat.Mkv ? mkv : mp4);

            properties.DefaultFileExtension = defaultext;

            if (this.CurrentTask != null && this.CurrentTask.Destination != null)
            {
                if (Directory.Exists(Path.GetDirectoryName(this.CurrentTask.Destination.Path)))
                {
                    properties.SuggestedStorageItem = this.CurrentTask.Destination.File;
                }
            }

            AppServices.Current?.IO?.Pickers?.SaveFile(properties)?.ContinueWith(task =>
            {
                var file = task.Result;
                if (file == null)
                {
                    return;
                }

                if (file != null)
                {
                    if (file.Path == this.ScannedSource.ScanPath)
                    {
                        this.errorService.ShowMessageBox(Resources.Main_SourceDestinationMatchError, Resources.Error, DialogButtonType.OK, DialogType.Error);
                        this.Destination = null;
                        return;
                    }

                    this.Destination = new FileData(file);

                    // Set the Extension Dropdown. This will also set Mp4/m4v correctly.
                    switch (Path.GetExtension(file.Path))
                    {
                        case ".mkv":
                            this.SummaryViewModel.SetContainer(OutputFormat.Mkv);
                            break;

                        case ".mp4":
                        case ".m4v":
                            this.SummaryViewModel.SetContainer(OutputFormat.Mp4);
                            break;
                    }

                    this.NotifyOfPropertyChange(() => this.CurrentTask);
                }
            });
        }

        /// <summary>
        /// The open destination directory.
        /// </summary>
        public void OpenDestinationDirectory()
        {
            if (this.Destination != null)
            {
                try
                {
                    string directory = Path.GetDirectoryName(this.Destination.Path);
                    this.launcher.OpenDirectory(directory);
                }
                catch (Exception exc)
                {
                    this.errorService.ShowError(Resources.MainViewModel_UnableToLaunchDestDir, Resources.MainViewModel_UnableToLaunchDestDirSolution, exc);
                }
            }
        }

        /// <summary>
        /// Add a Preset
        /// </summary>
        public void PresetAdd()
        {
            IAddPresetViewModel presetViewModel = IoC.Get<IAddPresetViewModel>();
            presetViewModel.Setup(this.CurrentTask, this.SelectedTitle, this.AudioViewModel.AudioBehaviours, this.SubtitleViewModel.SubtitleBehaviours);
            this.viewManager.ShowDialog(presetViewModel);

            this.NotifyOfPropertyChange(() => this.PresetsCategories);
            this.NotifyOfPropertyChange(() => this.CategoryPresets);
        }

        /// <summary>
        /// Update a selected preset.
        /// </summary>
        public void PresetUpdate()
        {
            if (this.SelectedPreset == null)
            {
                this.errorService.ShowMessageBox(
                    Resources.Main_SelectPresetForUpdate, Resources.Main_NoPresetSelected, DialogButtonType.OK, DialogType.Warning);

                return;
            }

            if (this.selectedPreset.IsBuildIn)
            {
                this.errorService.ShowMessageBox(
                    Resources.Main_NoUpdateOfBuiltInPresets, Resources.Main_NoPresetSelected, DialogButtonType.OK, DialogType.Warning);
                return;
            }

            if (this.errorService.ShowMessageBox(Resources.Main_PresetUpdateConfrimation, Resources.AreYouSure, DialogButtonType.YesNo, DialogType.Question) == DialogResult.Yes)
            {
                this.selectedPreset.Update(new EncodeTask(this.CurrentTask), new AudioBehaviours(this.AudioViewModel.AudioBehaviours), new SubtitleBehaviours(this.SubtitleViewModel.SubtitleBehaviours));
                this.presetService.Update(this.selectedPreset);

                this.errorService.ShowMessageBox(
                        Resources.Main_PresetUpdated, Resources.Updated, DialogButtonType.OK, DialogType.Information);
            }
        }

        /// <summary>
        /// Manage the current Preset
        /// </summary>
        public void PresetManage()
        {
            if (this.SelectedPreset == null)
            {
                this.errorService.ShowMessageBox(
                    Resources.Main_SelectPresetForUpdate, Resources.Main_NoPresetSelected, DialogButtonType.OK, DialogType.Warning);

                return;
            }

            if (this.selectedPreset.IsBuildIn)
            {
                this.errorService.ShowMessageBox(
                    Resources.Main_NoUpdateOfBuiltInPresets, Resources.Main_NoPresetSelected, DialogButtonType.OK, DialogType.Warning);
                return;
            }

            IManagePresetViewModel presetViewModel = IoC.Get<IManagePresetViewModel>();
            presetViewModel.Setup(this.selectedPreset);
            this.viewManager.ShowDialog(presetViewModel);
            Preset preset = presetViewModel.Preset;

            this.NotifyOfPropertyChange(() => this.CategoryPresets);
            this.selectedPreset = preset; // Reselect the preset
            this.NotifyOfPropertyChange(() => this.SelectedPreset);
        }

        /// <summary>
        /// Remove a Preset
        /// </summary>
        public void PresetRemove()
        {
            if (this.selectedPreset != null)
            {
                if (this.selectedPreset.IsDefault)
                {
                    this.errorService.ShowMessageBox(
                      Resources.MainViewModel_CanNotDeleteDefaultPreset,
                      Resources.Warning,
                      DialogButtonType.OK,
                      DialogType.Information);

                    return;
                }

                var result =
                this.errorService.ShowMessageBox(
                   Resources.MainViewModel_PresetRemove_AreYouSure + this.selectedPreset.Name + " ?",
                   Resources.Question,
                   DialogButtonType.YesNo,
                   DialogType.Question);

                if (result == DialogResult.No)
                {
                    return;
                }

                this.presetService.Remove(this.selectedPreset);
                this.NotifyOfPropertyChange(() => this.CategoryPresets);
                this.SelectedPreset = this.CategoryPresets.FirstOrDefault();
            }
            else
            {
                this.dialogService.Show(Resources.Main_SelectPreset, Resources.Warning, DialogButtonType.OK, DialogType.Warning);
            }
        }

        /// <summary>
        /// Set a default preset
        /// </summary>
        public void PresetSetDefault()
        {
            if (this.selectedPreset != null)
            {
                this.presetService.SetDefault(this.selectedPreset);
                this.dialogService.Show(string.Format(Resources.Main_NewDefaultPreset, this.selectedPreset.Name), Resources.Main_Presets, DialogButtonType.OK, DialogType.Information);
            }
            else
            {
                this.dialogService.Show(Resources.Main_SelectPreset, Resources.Warning, DialogButtonType.OK, DialogType.Warning);
            }
        }

        /// <summary>
        /// Import a Preset
        /// </summary>
        public void PresetImport()
        {
            var properties = new FilePickerProperties();
            properties.FileTypes.Add(".json");
            properties.FileTypes.Add(".plist");

            AppServices.Current?.IO?.Pickers?.PickFile(properties)?.ContinueWith(task =>
            {
                var file = task.Result;
                if (file != null)
                {
                    this.presetService.Import(file);
                    this.NotifyOfPropertyChange(() => this.CategoryPresets);
                }
            });
        }

        /// <summary>
        /// Export a Preset
        /// </summary>
        public void PresetExport()
        {
            if (this.selectedPreset != null && !this.selectedPreset.IsBuildIn)
            {
                var properties = new FileSavePickerProperties();
                properties.FileTypes.Add(".json");

                AppServices.Current?.IO?.Pickers?.SaveFile(properties)?.ContinueWith(task =>
                {
                    var file = task.Result;
                    if (file != null)
                    {
                        this.presetService.Export(file, this.selectedPreset, HBConfigurationFactory.Create());
                    }
                });
            }
            else
            {
                this.dialogService.Show(Resources.Main_SelectPreset, Resources.Warning, DialogButtonType.OK, DialogType.Warning);
            }
        }

        /// <summary>
        /// Reset built-in presets
        /// </summary>
        public void PresetReset()
        {
            this.presetService.UpdateBuiltInPresets();

            this.NotifyOfPropertyChange(() => this.PresetsCategories);
            this.NotifyOfPropertyChange(() => this.CategoryPresets);

            this.SetDefaultPreset();

            this.errorService.ShowMessageBox(Resources.Presets_ResetComplete, Resources.Presets_ResetHeader, DialogButtonType.OK, DialogType.Information);
        }

        public void PresetSelect()
        {
            this.PresetSelect(this.SelectedPreset);
        }

        /// <summary>
        /// The preset select.
        /// </summary>
        /// <param name="tag">
        /// The tag.
        /// </param>
        public void PresetSelect(object tag)
        {
            Preset preset = tag as Preset;
            if (preset != null)
            {
                if (this.SelectedPresetCategory == null || this.SelectedPresetCategory.Category != preset.Category)
                {
                    this.SelectedPresetCategory = this.PresetsCategories.FirstOrDefault(c => c.Category == preset.Category);
                }

                this.selectedPreset = preset;
                this.NotifyOfPropertyChange(() => this.SelectedPreset);

                this.presetService.SetSelected(this.selectedPreset);

                if (this.selectedPreset != null)
                {
                    // Tab Settings
                    this.isSettingPreset = true;
                    this.IsModifiedPreset = false;
                    this.PictureSettingsViewModel.SetPreset(this.selectedPreset, this.CurrentTask);
                    this.VideoViewModel.SetPreset(this.selectedPreset, this.CurrentTask);
                    this.FiltersViewModel.SetPreset(this.selectedPreset, this.CurrentTask);
                    this.AudioViewModel.SetPreset(this.selectedPreset, this.CurrentTask);
                    this.SubtitleViewModel.SetPreset(this.selectedPreset, this.CurrentTask);
                    this.ChaptersViewModel.SetPreset(this.selectedPreset, this.CurrentTask);
                    this.AdvancedViewModel.SetPreset(this.selectedPreset, this.CurrentTask);
                    this.MetaDataViewModel.SetPreset(this.selectedPreset, this.CurrentTask);
                    this.SummaryViewModel.SetPreset(this.selectedPreset, this.CurrentTask);
                    this.isSettingPreset = false;
                }
            }
        }

        /// <summary>
        /// Start a Scan
        /// </summary>
        /// <param name="path">
        /// The file/folder path.
        /// </param>
        /// <param name="title">
        /// The title.
        /// </param>
        public void StartScan(string path, int title)
        {
            if (path != null)
            {
                ShowSourceSelection = false;
                this.scanService.Scan(path, title, null, HBConfigurationFactory.Create());
            }
        }

        /// <summary>
        /// The process drive.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        public void ProcessDrive(object item)
        {
            if (item != null)
            {
                if (item.GetType() == typeof(DriveInformation))
                {
                    this.StartScan(((DriveInformation)item).RootDirectory, 0);
                }
                else if (item.GetType() == typeof(SourceMenuItem))
                {
                    DriveInformation driveInfo = ((SourceMenuItem)item).Tag as DriveInformation;
                    if (driveInfo != null)
                    {
                        this.StartScan(driveInfo.RootDirectory, this.TitleSpecificScan);
                    }

                    this.ShowSourceSelection = false;
                }
            }
        }

        #endregion Main Window Public Methods

        #region Private Methods

        /// <summary>
        /// Update all the UI Components to allow the user to edit their previous settings.
        /// </summary>
        /// <param name="successful">
        /// The successful.
        /// </param>
        /// <param name="scannedSource">
        /// The scanned Source.
        /// </param>
        private void QueueEditAction(bool successful, Source scannedSource)
        {
            /* TODO Fix this. */
            Execute.OnUIThread(() =>
            {
                // Copy all the Scan data into the UI
                scannedSource.CopyTo(this.ScannedSource);
                this.NotifyOfPropertyChange(() => this.ScannedSource);
                this.NotifyOfPropertyChange(() => this.ScannedSource.Titles);

                // Select the Users Title
                this.SelectedTitle = this.ScannedSource.Titles.FirstOrDefault();
                this.CurrentTask = new EncodeTask(queueEditTask);
                this.NotifyOfPropertyChange(() => this.CurrentTask);
                this.HasSource = true;

                // Update the Main Window
                this.NotifyOfPropertyChange(() => this.Destination);
                this.SelectedAngle = this.CurrentTask.Angle;
                int start = this.CurrentTask.StartPoint;
                int end = this.CurrentTask.EndPoint;
                this.SelectedPointToPoint = this.CurrentTask.PointToPointMode; // Force reset.
                this.SelectedStartPoint = start;
                this.SelectedEndPoint = end;

                // Update the Tab Controls
                this.PictureSettingsViewModel.UpdateTask(this.CurrentTask);
                this.VideoViewModel.UpdateTask(this.CurrentTask);
                this.FiltersViewModel.UpdateTask(this.CurrentTask);
                this.AudioViewModel.UpdateTask(this.CurrentTask);
                this.SubtitleViewModel.UpdateTask(this.CurrentTask);
                this.ChaptersViewModel.UpdateTask(this.CurrentTask);
                this.AdvancedViewModel.UpdateTask(this.CurrentTask);
                this.MetaDataViewModel.UpdateTask(this.CurrentTask);
                this.SummaryViewModel.UpdateTask(this.CurrentTask);

                // Cleanup
                this.ShowStatusWindow = false;
                this.SourceLabel = this.SourceName;
                this.StatusLabel = Resources.Main_ScanCompleted;
            });
        }

        /// <summary>
        /// Setup the UI tabs. Passes in any relevant models for setup.
        /// </summary>
        private void SetupTabs()
        {
            // Setup the Tabs
            if (this.selectedTitle != null)
            {
                this.isSettingPreset = true;
                this.PictureSettingsViewModel.SetSource(this.ScannedSource, this.SelectedTitle, this.selectedPreset, this.CurrentTask);
                this.VideoViewModel.SetSource(this.ScannedSource, this.SelectedTitle, this.selectedPreset, this.CurrentTask);
                this.FiltersViewModel.SetSource(this.ScannedSource, this.SelectedTitle, this.selectedPreset, this.CurrentTask);
                this.AudioViewModel.SetSource(this.ScannedSource, this.SelectedTitle, this.selectedPreset, this.CurrentTask);
                this.SubtitleViewModel.SetSource(this.ScannedSource, this.SelectedTitle, this.selectedPreset, this.CurrentTask);
                this.ChaptersViewModel.SetSource(this.ScannedSource, this.SelectedTitle, this.selectedPreset, this.CurrentTask);
                this.AdvancedViewModel.SetSource(this.ScannedSource, this.SelectedTitle, this.selectedPreset, this.CurrentTask);
                this.MetaDataViewModel.SetSource(this.ScannedSource, this.SelectedTitle, this.selectedPreset, this.CurrentTask);
                this.SummaryViewModel.SetSource(this.ScannedSource, this.SelectedTitle, this.selectedPreset, this.CurrentTask);
                this.isSettingPreset = false;
            }
        }

        private void TabStatusChanged(object sender, TabStatusEventArgs e)
        {
            if (this.isSettingPreset)
            {
                return; // Don't process this when we are setting up.
            }

            bool matchesPreset = this.PictureSettingsViewModel.MatchesPreset(this.selectedPreset);

            if (!this.SummaryViewModel.MatchesPreset(this.selectedPreset))
            {
                matchesPreset = false;
            }

            if (!this.PictureSettingsViewModel.MatchesPreset(this.selectedPreset))
            {
                matchesPreset = false;
            }

            if (!this.VideoViewModel.MatchesPreset(this.selectedPreset))
            {
                matchesPreset = false;
            }

            if (!this.FiltersViewModel.MatchesPreset(this.selectedPreset))
            {
                matchesPreset = false;
            }

            if (!this.AudioViewModel.MatchesPreset(this.selectedPreset))
            {
                matchesPreset = false;
            }

            if (!this.SubtitleViewModel.MatchesPreset(this.selectedPreset))
            {
                matchesPreset = false;
            }

            if (!this.ChaptersViewModel.MatchesPreset(this.selectedPreset))
            {
                matchesPreset = false;
            }

            if (!this.AdvancedViewModel.MatchesPreset(this.selectedPreset))
            {
                matchesPreset = false;
            }

            if (!this.MetaDataViewModel.MatchesPreset(this.selectedPreset))
            {
                matchesPreset = false;
            }

            this.IsModifiedPreset = !matchesPreset;
        }

        /// <summary>
        /// Calculate the duration between the end and start point
        /// </summary>
        /// <returns>
        /// The duration calculation.
        /// </returns>
        private string DurationCalculation()
        {
            if (this.selectedTitle == null)
            {
                return "--:--:--";
            }

            double startEndDuration = this.SelectedEndPoint - this.SelectedStartPoint;
            TimeSpan output;

            switch (this.SelectedPointToPoint)
            {
                case PointToPointMode.Chapters:
                    output = this.SelectedTitle.CalculateDuration(this.SelectedStartPoint, this.SelectedEndPoint);
                    return string.Format("{0:00}:{1:00}:{2:00}", output.Hours, output.Minutes, output.Seconds);

                case PointToPointMode.Seconds:
                    output = TimeSpan.FromSeconds(startEndDuration);
                    return string.Format("{0:00}:{1:00}:{2:00}", output.Hours, output.Minutes, output.Seconds);

                case PointToPointMode.Frames:
                    startEndDuration = startEndDuration / selectedTitle.Fps;
                    output = TimeSpan.FromSeconds(Math.Round(startEndDuration, 2));
                    return string.Format("{0:00}:{1:00}:{2:00}", output.Hours, output.Minutes, output.Seconds);
            }

            return "--:--:--";
        }

        /// <summary>
        /// The open alert window.
        /// </summary>
        /// <param name="header">
        /// The header.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        private void OpenAlertWindow(string header, string message)
        {
            this.ShowAlertWindow = true;
            this.AlertWindowHeader = header;
            this.AlertWindowText = message;
        }

        private void SetDefaultPreset()
        {
            // Preset Selection
            if (this.presetService.DefaultPreset != null)
            {
                PresetDisplayCategory category =
                    (PresetDisplayCategory)this.PresetsCategories.FirstOrDefault(
                        p => p.Category == this.presetService.DefaultPreset.Category);

                this.SelectedPresetCategory = category;
                this.SelectedPreset = this.presetService.DefaultPreset;
            }
        }

        #endregion Private Methods

        #region Event Handlers

        /// <summary>
        /// Handle the Scan Status Changed Event.
        /// </summary>
        /// <param name="sender">
        /// The Sender
        /// </param>
        /// <param name="e">
        /// The EventArgs
        /// </param>
        private void ScanStatusChanged(object sender, ScanProgressEventArgs e)
        {
            this.SourceLabel = string.Format(Resources.Main_ScanningTitleXOfY, e.CurrentTitle, e.Titles, e.Percentage);
            this.StatusLabel = string.Format(Resources.Main_ScanningTitleXOfY, e.CurrentTitle, e.Titles, e.Percentage);
        }

        /// <summary>
        /// Handle the Scan Completed Event
        /// </summary>
        /// <param name="sender">
        /// The Sender
        /// </param>
        /// <param name="e">
        /// The EventArgs
        /// </param>
        private void ScanCompleted(object sender, ScanCompletedEventArgs e)
        {
            this.ShowStatusWindow = false;

            if (e.ScannedSource != null && !e.Cancelled)
            {
                if (this.ScannedSource == null)
                {
                    this.ScannedSource = new Source();
                }
                e.ScannedSource.CopyTo(this.ScannedSource);
            }
            else
            {
                this.ScannedSource = null;
            }

            Execute.OnUIThread(() =>
            {
                if (e.Successful && this.ScannedSource != null)
                {
                    this.NotifyOfPropertyChange(() => this.ScannedSource);
                    this.NotifyOfPropertyChange(() => this.ScannedSource.Titles);
                    this.HasSource = true;
                    this.SelectedTitle = this.ScannedSource.Titles.FirstOrDefault(t => t.MainTitle) ?? this.ScannedSource.Titles.FirstOrDefault();
                }
                else if (!e.Cancelled)
                {
                    this.OpenAlertWindow(Resources.Main_ScanNoTitlesFound, Resources.Main_ScanNoTitlesFoundMessage);
                }

                if (e.Successful)
                {
                    this.SourceLabel = this.SourceName;
                    this.StatusLabel = Resources.Main_ScanCompleted;
                }
                else if (e.Cancelled)
                {
                    this.SourceLabel = Resources.Main_ScanCancelled;
                    this.StatusLabel = Resources.Main_ScanCancelled;
                }
                else
                {
                    this.SourceLabel = Resources.Main_ScanFailled_CheckLog;
                    this.StatusLabel = Resources.Main_ScanFailled_CheckLog;
                }
            });
        }

        /// <summary>
        /// Handle the Scan Started Event
        /// </summary>
        /// <param name="sender">
        /// The Sender
        /// </param>
        /// <param name="e">
        /// The EventArgs
        /// </param>
        private void ScanStared(object sender, EventArgs e)
        {
            Execute.OnUIThread(
                () =>
                {
                    this.StatusLabel = Resources.Main_ScanningPleaseWait;
                    this.ShowStatusWindow = true;
                });
        }

        /// <summary>
        /// The Encode Status has changed Handler
        /// </summary>
        /// <param name="sender">
        /// The Sender
        /// </param>
        /// <param name="e">
        /// The Encode Progress Event Args
        /// </param>
        private void EncodeStatusChanged(object sender, EncodeProgressEventArgs e)
        {
            int percent;
            int.TryParse(
                Math.Round(e.PercentComplete).ToString(CultureInfo.InvariantCulture),
                out percent);

            Execute.OnUIThread(
                () =>
                {
                    if (this.queueProcessor.EncodeService.IsEncoding)
                    {
                        string jobsPending = string.Format(Resources.Main_JobsPending_addon, this.queueProcessor.Count);
                        if (e.IsSubtitleScan)
                        {
                            this.ProgramStatusLabel = string.Format(Resources.MainViewModel_EncodeStatusChanged_SubScan_StatusLabel,
                                e.Task,
                                e.TaskCount,
                                e.PercentComplete,
                                e.EstimatedTimeLeft,
                                e.ElapsedTime,
                                jobsPending);
                        }
                        else if (e.IsMuxing)
                        {
                            this.ProgramStatusLabel = ResourcesUI.MainView_Muxing;
                        }
                        else if (e.IsSearching)
                        {
                            this.ProgramStatusLabel = string.Format(ResourcesUI.MainView_ProgressStatusWithTask, ResourcesUI.MainView_Searching, e.PercentComplete, e.EstimatedTimeLeft, jobsPending);
                        }
                        else
                        {
                            this.ProgramStatusLabel =
                            string.Format(Resources.MainViewModel_EncodeStatusChanged_StatusLabel,
                                e.Task,
                                e.TaskCount,
                                e.PercentComplete,
                                e.CurrentFrameRate,
                                e.AverageFrameRate,
                                e.EstimatedTimeLeft,
                                e.ElapsedTime,
                                jobsPending);
                        }

                        if (lastEncodePercentage != percent && this.taskbarService != null)
                        {
                            this.taskbarService.SetTaskBarProgress(percent);
                        }

                        lastEncodePercentage = percent;
                        this.ProgressPercentage = percent;
                        this.NotifyOfPropertyChange(() => ProgressPercentage);

                        if (this.userSettingService.GetUserSetting<bool>(UserSettingConstants.ShowStatusInTitleBar))
                        {
                            this.WindowTitle = string.Format(Resources.WindowTitleStatus, Resources.HandBrake_Title, this.ProgressPercentage, e.Task, e.TaskCount);
                            var notification = string.Format(Resources.TaskTrayStatusTitle, Resources.HandBrake_Title, this.ProgressPercentage, e.Task, e.TaskCount, e.EstimatedTimeLeft);
                            this.notificationManager.Notify(notification);
                        }
                    }
                    else
                    {
                        this.ProgramStatusLabel = Resources.Main_QueueFinished;
                        this.IsEncoding = false;
                        this.WindowTitle = Resources.HandBrake_Title;
                        this.notificationManager.Notify(this.WindowTitle);

                        if (taskbarService != null)
                        {
                            taskbarService.DisableTaskBarProgress();
                        }
                    }
                });
        }

        /// <summary>
        /// Handle the Queue Starting Event
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void QueueProcessorJobProcessingStarted(object sender, QueueProgressEventArgs e)
        {
            Execute.OnUIThread(
               () =>
               {
                   this.ProgramStatusLabel = Resources.Main_PreparingToEncode;
                   this.IsEncoding = true;
               });
        }

        /// <summary>
        /// The Queue has completed handler
        /// </summary>
        /// <param name="sender">
        /// The Sender
        /// </param>
        /// <param name="e">
        /// The EventArgs
        /// </param>
        private void QueueCompleted(object sender, EventArgs e)
        {
            this.IsEncoding = false;

            Execute.OnUIThread(
                () =>
                {
                    string errorDesc = string.Empty;
                    if (this.queueProcessor.ErrorCount > 0)
                    {
                        errorDesc += string.Format(Resources.Main_QueueFinishedErrors, this.queueProcessor.ErrorCount);
                    }

                    this.ProgramStatusLabel = Resources.Main_QueueFinished + errorDesc;
                    this.WindowTitle = Resources.HandBrake_Title;
                    this.notificationManager.Notify(this.WindowTitle);

                    if (this.taskbarService != null)
                    {
                        this.taskbarService.DisableTaskBarProgress();
                    }
                });
        }

        /// <summary>
        /// The queue changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The EventArgs.
        /// </param>
        private void QueueChanged(object sender, EventArgs e)
        {
            Execute.OnUIThread(
              () =>
              {
                  this.ProgramStatusLabel = string.Format(Resources.Main_XEncodesPending, this.queueProcessor.Count);
                  this.NotifyOfPropertyChange(() => this.QueueLabel);
                  this.NotifyOfPropertyChange(() => this.StartLabel);
              });
        }

        private void QueueProcessor_QueuePaused(object sender, EventArgs e)
        {
            Execute.OnUIThread(
                () =>
                {
                    this.ProgramStatusLabel = Resources.Main_QueuePaused;
                    this.NotifyOfPropertyChange(() => this.QueueLabel);
                    this.NotifyOfPropertyChange(() => this.StartLabel);
                });
        }

        /// <summary>
        /// Allows the main window to respond to setting changes.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void UserSettingServiceSettingChanged(object sender, SettingChangedEventArgs e)
        {
            switch (e.Key)
            {
                case UserSettingConstants.ShowAdvancedTab:
                    this.NotifyOfPropertyChange(() => this.ShowAdvancedTab);
                    break;

                case UserSettingConstants.WhenCompleteAction:
                    this.QueueViewModel.WhenDone(this.userSettingService.GetUserSetting<string>(UserSettingConstants.WhenCompleteAction), false);
                    break;
            }
        }

        /// <summary>
        /// Handle the property changed event of the encode task.
        /// Allows the main window to respond to changes.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void CurrentTask_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == UserSettingConstants.ShowAdvancedTab)
            {
                this.NotifyOfPropertyChange(() => this.ShowAdvancedTab);
            }
        }

        #endregion Event Handlers
    }
}