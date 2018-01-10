﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DialogService.cs" company="HandBrake Project (http://handbrake.fr)">
//   This file is part of the HandBrake source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   A Class for Creating Dialog Prompts.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace HandBrake.Utilities
{
    using System.Windows;

    using HandBrake.Model.Prompts;
    using HandBrake.Utilities.Interfaces;

    public class DialogService : IDialogService
    {
        public DialogResult Show(string message, string header, DialogButtonType buttons, DialogType type)
        {
            var buttontype = (MessageBoxButton)(int)buttons;
            var image = (MessageBoxImage)(int)type;

            var result = MessageBox.Show(message, header, buttontype, image);

            return (DialogResult)(int)result;
        }

        public void Show(string message, string title)
        {
            MessageBox.Show(message, title);
        }

        void IDialogService.Show(string message)
        {
            MessageBox.Show(message);
        }
    }
}