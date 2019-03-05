// <copyright file="Utilities.cs" company="Daniel Griffen">
// Copyright (c) Daniel Griffen. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RustLanguageExtension
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// General utility class for the extension.
    /// </summary>
    internal class Utilities
    {
        /// <summary>
        /// Displays an info bar with one button and waits until the user closes it or clicks the button.
        /// </summary>
        /// <param name="infoBar">The infobar to display.</param>
        /// <param name="serviceProvider">The async service provider.</param>
        /// <returns>A task that completes when the info bar has been closed or the button selected.
        /// The task completes with true if the button was pressed or false if the info bar was closed.</returns>
        public static async Task<bool> WaitForSingleButtonInfoBarAsync(VsUtilities.InfoBar infoBar, IAsyncServiceProvider serviceProvider)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (infoBar.ActionItems.Count != 1)
            {
                throw new ArgumentException($"{nameof(infoBar)} has more than one button element");
            }

            var completionSource = new TaskCompletionSource<bool>();

            var button = (VsUtilities.InfoBarButton)infoBar.ActionItems.GetItem(0);
            button.OnClick += (source, e) =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                completionSource.SetResult(true);
                e.InfoBarUIElement.Close();
            };

            infoBar.OnClosed += () =>
            {
                completionSource.TrySetResult(false);
            };

            await VsUtilities.ShowInfoBarAsync(infoBar, serviceProvider);
            return await completionSource.Task;
        }
    }
}
