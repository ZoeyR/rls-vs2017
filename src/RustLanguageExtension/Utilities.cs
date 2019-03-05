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

    internal class Utilities
    {
        public static async Task<bool> WaitForSingleButtonInfoBarAsync(VsUtilities.InfoBar infoBar)
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

            await VsUtilities.ShowInfoBarAsync(infoBar);
            return await completionSource.Task;
        }
    }
}
