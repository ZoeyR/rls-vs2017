// <copyright file="VsUtilities.cs" company="Daniel Griffen">
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
    using Microsoft.VisualStudio.Imaging;
    using Microsoft.VisualStudio.Imaging.Interop;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TaskStatusCenter;
    using Microsoft.VisualStudio.Threading;

    public static class VsUtilities
    {
        public delegate void InfoBarEventHandler(object source, InfoBarEventArgs e);

        public static async System.Threading.Tasks.Task CreateTaskAsync<T>(string title, Microsoft.VisualStudio.Shell.IAsyncServiceProvider serviceProvider, Task<T> task)
        {
            var tsc = await serviceProvider.GetServiceAsync(typeof(SVsTaskStatusCenterService)) as IVsTaskStatusCenterService;

            if (tsc == null)
            {
                return;
            }

            var options = default(TaskHandlerOptions);
            options.Title = title;

            var data = default(TaskProgressData);
            data.PercentComplete = null;

            var handler = tsc.PreRegister(options, data);
            handler.RegisterTask(task);
        }

        public static async System.Threading.Tasks.Task ShowNotoficationAsync(string notification)
        {
            var infoBar = new VsUtilities.InfoBar(notification);
            await VsUtilities.ShowInfoBarAsync(infoBar);
        }

        public static async System.Threading.Tasks.Task ShowInfoBarAsync(InfoBar infoBar)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IVsInfoBarUIFactory infoBarUIFactory = await ServiceProvider.GetGlobalServiceAsync(typeof(SVsInfoBarUIFactory)) as IVsInfoBarUIFactory;
            var uiElement = infoBarUIFactory.CreateInfoBar(infoBar);

            IVsShell shell = await ServiceProvider.GetGlobalServiceAsync(typeof(SVsShell)) as IVsShell;
            shell.GetProperty((int)__VSSPROPID7.VSSPROPID_MainWindowInfoBarHost, out var host);
            if (host is IVsInfoBarHost infoBarHost)
            {
                var eventSink = new InfoBarEvents(infoBar, uiElement);
                uiElement.Advise(eventSink, out var cookie);
                eventSink.Cookie = cookie;

                infoBarHost.AddInfoBar(uiElement);
            }
        }

        public class InfoBar : IVsInfoBar
        {
            public InfoBar(string text, params InfoBarButton[] buttons)
            {
                this.TextSpans = new InfoBarTextSpanCollection(new InfoBarTextSpan(text));
                this.ActionItems = new InfoBarActionItemCollection(buttons);
            }

            public event Action OnClosed;

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

            public void Close()
            {
                if (this.OnClosed != null)
                {
                    this.OnClosed();
                }
            }
        }

        public class InfoBarButton : InfoBarActionItem
        {
            public InfoBarButton(string text, object actionContext = null)
                : base(text, actionContext)
            {
            }

            public event InfoBarEventHandler OnClick;

            public override bool IsButton => true;

            public void Click(InfoBarEventArgs e)
            {
                if (this.OnClick != null)
                {
                    this.OnClick(this, e);
                }
            }
        }

        private class InfoBarEvents : IVsInfoBarUIEvents
        {
            private uint cookie;
            private IVsInfoBarUIElement uiElement;
            private InfoBar infoBar;

            public InfoBarEvents(InfoBar infoBar, IVsInfoBarUIElement uiElement)
            {
                this.infoBar = infoBar;
                this.uiElement = uiElement;
            }

            public uint Cookie
            {
                set { this.cookie = value; }
            }

            public void OnClosed(IVsInfoBarUIElement infoBarUIElement)
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                this.infoBar.Close();
                this.uiElement.Unadvise(this.cookie);
            }

            public void OnActionItemClicked(IVsInfoBarUIElement infoBarUIElement, IVsInfoBarActionItem actionItem)
            {
                if (actionItem is InfoBarButton button)
                {
                    button.Click(new InfoBarEventArgs(this.uiElement, this.infoBar));
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

            public int Count => this.spans.Length;

            public IVsInfoBarTextSpan GetSpan(int index)
            {
                return this.spans[index];
            }
        }

        private class InfoBarActionItemCollection : IVsInfoBarActionItemCollection
        {
            private IVsInfoBarActionItem[] items;

            public InfoBarActionItemCollection(params IVsInfoBarActionItem[] items)
            {
                this.items = items;
            }

            public int Count => this.items.Length;

            public IVsInfoBarActionItem GetItem(int index)
            {
                return this.items[index];
            }
        }
    }
}
