﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShellView.xaml.cs" company="HandBrake Project (http://handbrake.fr)">
//   This file is part of the HandBrake source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Interaction logic for ShellView.xaml
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandBrake.Views
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Input;

    using Caliburn.Micro;
    using HandBrake.Commands;
    using HandBrake.Services;
    using HandBrake.Services.Interfaces;
    using HandBrake.Utilities;
    using HandBrake.ViewModels.Interfaces;

    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShellView"/> class.
        /// </summary>
        public ShellView()
        {
            this.InitializeComponent();

            IUserSettingService userSettingService = IoC.Get<IUserSettingService>();
            bool minimiseToTray = userSettingService.GetUserSetting<bool>(UserSettingConstants.MainWindowMinimize);

            if (minimiseToTray)
            {
                NotificationService.Register(this);
                this.StateChanged += this.ShellViewStateChanged;
            }

            this.InputBindings.Add(new InputBinding(new ProcessShortcutCommand(new KeyGesture(Key.S, ModifierKeys.Control)), new KeyGesture(Key.S, ModifierKeys.Control))); // Start Encode
            this.InputBindings.Add(new InputBinding(new ProcessShortcutCommand(new KeyGesture(Key.K, ModifierKeys.Control)), new KeyGesture(Key.K, ModifierKeys.Control))); // Stop Encode
            this.InputBindings.Add(new InputBinding(new ProcessShortcutCommand(new KeyGesture(Key.L, ModifierKeys.Control)), new KeyGesture(Key.L, ModifierKeys.Control))); // Open Log Window
            this.InputBindings.Add(new InputBinding(new ProcessShortcutCommand(new KeyGesture(Key.Q, ModifierKeys.Control)), new KeyGesture(Key.Q, ModifierKeys.Control))); // Open Queue Window
            this.InputBindings.Add(new InputBinding(new ProcessShortcutCommand(new KeyGesture(Key.A, ModifierKeys.Control)), new KeyGesture(Key.A, ModifierKeys.Control))); // Add to Queue
            this.InputBindings.Add(new InputBinding(new ProcessShortcutCommand(new KeyGesture(Key.O, ModifierKeys.Control)), new KeyGesture(Key.O, ModifierKeys.Control))); // File Scan
            this.InputBindings.Add(new InputBinding(new ProcessShortcutCommand(new KeyGesture(Key.O, ModifierKeys.Alt)), new KeyGesture(Key.O, ModifierKeys.Alt)));     // Scan Window
            this.InputBindings.Add(new InputBinding(new ProcessShortcutCommand(new KeyGesture(Key.O, ModifierKeys.Control | ModifierKeys.Shift)), new KeyGesture(Key.O, ModifierKeys.Control | ModifierKeys.Shift))); // Scan a Folder
            this.InputBindings.Add(new InputBinding(new ProcessShortcutCommand(new KeyGesture(Key.G, ModifierKeys.Control | ModifierKeys.Shift)), new KeyGesture(Key.G, ModifierKeys.Control | ModifierKeys.Shift))); // Garbage Colleciton
            this.InputBindings.Add(new InputBinding(new ProcessShortcutCommand(new KeyGesture(Key.F1, ModifierKeys.None)), new KeyGesture(Key.F1, ModifierKeys.None))); // Help

            // Enable Windows Taskbar progress indication.
            if (this.TaskbarItemInfo == null)
            {
                this.TaskbarItemInfo = TaskBarService.WindowsTaskbar;
            }
        }

        /// <summary>
        /// Check with the user before closing.
        /// </summary>
        /// <param name="e">
        /// The CancelEventArgs.
        /// </param>
        protected override void OnClosing(CancelEventArgs e)
        {
            IShellViewModel shellViewModel = this.DataContext as IShellViewModel;

            if (shellViewModel != null)
            {
                bool canClose = shellViewModel.CanClose();
                if (!canClose)
                {
                    e.Cancel = true;
                }
            }

            NotificationService.UnRegister();

            base.OnClosing(e);
        }

        /// <summary>
        /// The shell view state changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void ShellViewStateChanged(object sender, EventArgs e)
        {
            if (NotificationService.Registered)
            {
                if (this.WindowState == WindowState.Minimized)
                {
                    this.ShowInTaskbar = false;
                    NotificationService.ChangeVisibility(true);
                }
                else if (this.WindowState == WindowState.Normal)
                {
                    NotificationService.ChangeVisibility(false);
                    this.ShowInTaskbar = true;
                }
            }
        }
    }
}