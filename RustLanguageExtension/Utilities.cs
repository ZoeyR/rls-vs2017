using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RustLanguageExtension.VsUtilities;

namespace RustLanguageExtension
{
    internal class Utilities
    {
        public static async Task<bool> WaitForSingleButtonInfoBarAsync(InfoBar infoBar)
        {
            if (infoBar.ActionItems.Count != 1)
            {
                throw new ArgumentException($"{nameof(infoBar)} has more than one button element");
            }

            var completionSource = new TaskCompletionSource<bool>();

            var button = (InfoBarButton)infoBar.ActionItems.GetItem(0);
            button.OnClick += (source, e) =>
            {
                completionSource.SetResult(true);
                e.InfoBarUIElement.Close();
            };

            infoBar.OnClosed += () =>
            {
                completionSource.TrySetResult(false);
            };

            await VsUtilities.ShowInfoBar(infoBar);
            return await completionSource.Task;
        }
    }
}
