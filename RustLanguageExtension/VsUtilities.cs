using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskStatusCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RustLanguageExtension
{
    public static class VsUtilities
    {
        public static async Task<ITaskHandler> CreateTask(string title)
        {
            var tsc = await ServiceProvider.GetGlobalServiceAsync(typeof(SVsTaskStatusCenterService)) as IVsTaskStatusCenterService;

            var options = default(TaskHandlerOptions);
            options.Title = title;
            options.ActionsAfterCompletion = CompletionActions.RetainAndNotifyOnRanToCompletion;

            var data = default(TaskProgressData);
            data.PercentComplete = null;

            return tsc.PreRegister(options, data);
        }

        public static async System.Threading.Tasks.Task ShowInfoBar(string text)
        {
            var infoBar = new InfoBarModel(text, KnownMonikers.StatusInformation, isCloseButtonVisible: true);

            IVsInfoBarUIFactory infoBarUIFactory = await ServiceProvider.GetGlobalServiceAsync(typeof(SVsInfoBarUIFactory)) as IVsInfoBarUIFactory;
            var uiElement = infoBarUIFactory.CreateInfoBar(infoBar);

            IVsShell shell = await ServiceProvider.GetGlobalServiceAsync(typeof(SVsShell)) as IVsShell;
            shell.GetProperty((int)__VSSPROPID7.VSSPROPID_MainWindowInfoBarHost, out var host);
            if (host is IVsInfoBarHost infoBarHost)
            {
                infoBarHost.AddInfoBar(uiElement);
            }
        }
    }
}
