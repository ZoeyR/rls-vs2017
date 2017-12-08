﻿using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
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

        public static async System.Threading.Tasks.Task ShowInfoBar(InfoBar infoBar)
        {
            IVsInfoBarUIFactory infoBarUIFactory = await ServiceProvider.GetGlobalServiceAsync(typeof(SVsInfoBarUIFactory)) as IVsInfoBarUIFactory;
            var uiElement = infoBarUIFactory.CreateInfoBar(infoBar);

            IVsShell shell = await ServiceProvider.GetGlobalServiceAsync(typeof(SVsShell)) as IVsShell;
            shell.GetProperty((int)__VSSPROPID7.VSSPROPID_MainWindowInfoBarHost, out var host);
            if (host is IVsInfoBarHost infoBarHost)
            {
                infoBarHost.AddInfoBar(uiElement);
            }
        }

        public class InfoBar : IVsInfoBar
        {
            public ImageMoniker Image => KnownMonikers.StatusInformation;

            public bool IsCloseButtonVisible => true;

            public IVsInfoBarTextSpanCollection TextSpans
            {
                get;
                set;
            }

            public IVsInfoBarActionItemCollection ActionItems
            {
                get;
                set;
            }

            public event Action OnClosed;

            public InfoBar(string text, params InfoBarButton[] buttons)
            {
                TextSpans = new InfoBarTextSpanCollection(new InfoBarTextSpan(text));
                ActionItems = new InfoBarActionItemCollection(buttons);
            }

            public void Close()
            {
                this.OnClosed();
            }
        }

        public delegate void InfoBarEventHandler(object source);
        public class InfoBarButton : InfoBarActionItem
        {
            public event InfoBarEventHandler OnClick;

            protected InfoBarButton(string text, object actionContext = null) : base(text, actionContext)
            {
            }

            public void Click()
            {
                this.OnClick(this);
            }

            public override bool IsButton => true;
        }

        private class InfoBarEvents : IVsInfoBarUIEvents
        {
            private uint cookie;
            private IVsInfoBarUIElement uiElement;
            private InfoBar infoBar;

            public InfoBarEvents(InfoBar infoBar, uint cookie, IVsInfoBarUIElement uiElement)
            {
                this.cookie = cookie;
                this.uiElement = uiElement;
            }
            public void OnClosed(IVsInfoBarUIElement infoBarUIElement)
            {
                this.infoBar.Close();
                uiElement.Unadvise(this.cookie);
            }

            public void OnActionItemClicked(IVsInfoBarUIElement infoBarUIElement, IVsInfoBarActionItem actionItem)
            {
                if (actionItem is InfoBarButton button)
                {
                    button.Click();
                }
            }
        }

        private class InfoBarTextSpanCollection : IVsInfoBarTextSpanCollection
        {
            private IVsInfoBarTextSpan[] spans;
            public InfoBarTextSpanCollection(params IVsInfoBarTextSpan[] spans)
            {
                this.spans = spans;
            }
            public IVsInfoBarTextSpan GetSpan(int index)
            {
                return this.spans[index];
            }

            public int Count => this.spans.Length;
        }

        private class InfoBarActionItemCollection : IVsInfoBarActionItemCollection
        {
            private IVsInfoBarActionItem[] items;
            public InfoBarActionItemCollection(params IVsInfoBarActionItem[] items)
            {
                this.items = items;
            }

            public IVsInfoBarActionItem GetItem(int index)
            {
                return this.items[index];
            }

            public int Count => this.items.Length;
        }

    }
}
