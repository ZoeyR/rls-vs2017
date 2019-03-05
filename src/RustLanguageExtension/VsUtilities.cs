// <copyright file="VsUtilities.cs" company="Daniel Griffen">
// Copyright (c) Daniel Griffen. All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RustLanguageExtension
{
    using System;
    using Microsoft;
    using Microsoft.VisualStudio.Imaging;
    using Microsoft.VisualStudio.Imaging.Interop;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TaskStatusCenter;
    using Microsoft.VisualStudio.Threading;
    using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// Utility class for interacting with VS.
    /// </summary>
    internal static class VsUtilities
    {
        /// <summary>
        /// Event handler delegate type for info bar events.
        /// </summary>
        /// <param name="source">The object that produced the event.</param>
        /// <param name="e">Arguments for the info bar event.</param>
        public delegate void InfoBarEventHandler(object source, InfoBarEventArgs e);

        /// <summary>
        /// Create a task in the VS task status center
        /// </summary>
        /// <param name="title">The name of the task.</param>
        /// <param name="serviceProvider">VS async service provider.</param>
        /// <param name="task">The task that the item in the task status center is tracking.</param>
        /// <returns>A task that completes once the task is displayed in the status center.</returns>
        public static async Task CreateTaskAsync(string title, IAsyncServiceProvider serviceProvider, Task task)
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

        /// <summary>
        /// Shows a basic infobar notification.
        /// </summary>
        /// <param name="notification">The text of the notification.</param>
        /// <param name="serviceProvider">VS async service provider.</param>
        /// <returns>A task that completes once the infobar is displayed.</returns>
        public static async Task ShowNotoficationAsync(string notification, IAsyncServiceProvider serviceProvider)
        {
            var infoBar = new VsUtilities.InfoBar(notification);
            await VsUtilities.ShowInfoBarAsync(infoBar, serviceProvider);
        }

        /// <summary>
        /// Displays an <see cref="InfoBar"/> in VS.
        /// </summary>
        /// <param name="infoBar">The InfoBar object to display.</param>
        /// <param name="serviceProvider">VS async service provider.</param>
        /// <returns>A task that completes once the infobar is displayed.</returns>
        public static async Task ShowInfoBarAsync(InfoBar infoBar, IAsyncServiceProvider serviceProvider)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            IVsInfoBarUIFactory infoBarUIFactory = await serviceProvider.GetServiceAsync(typeof(SVsInfoBarUIFactory)) as IVsInfoBarUIFactory;
            Assumes.Present(infoBarUIFactory);

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

        /// <summary>
        /// Managed object for managing VS info bars.
        /// </summary>
        public class InfoBar : IVsInfoBar
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="InfoBar"/> class.
            /// </summary>
            /// <param name="text">Text to display in the info bar.</param>
            /// <param name="buttons">An array of buttons to place after the info bar text.</param>
            public InfoBar(string text, params InfoBarButton[] buttons)
            {
                this.TextSpans = new InfoBarTextSpanCollection(new InfoBarTextSpan(text));
                this.ActionItems = new InfoBarActionItemCollection(buttons);
            }

            /// <summary>
            /// Event that fires when the info bar is closed.
            /// </summary>
            public event Action OnClosed;

            /// <inheritdoc/>
            public ImageMoniker Image => KnownMonikers.StatusInformation;

            /// <inheritdoc/>
            public bool IsCloseButtonVisible => true;

            /// <inheritdoc/>
            public IVsInfoBarTextSpanCollection TextSpans
            {
                get;
                set;
            }

            /// <inheritdoc/>
            public IVsInfoBarActionItemCollection ActionItems
            {
                get;
                set;
            }

            /// <summary>
            /// Call to fire the <see cref="OnClosed"/> event.
            /// </summary>
            public void Close()
            {
                if (this.OnClosed != null)
                {
                    this.OnClosed();
                }
            }
        }

        /// <summary>
        /// Managed object representing buttons in an info bar.
        /// </summary>
        public class InfoBarButton : InfoBarActionItem
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="InfoBarButton"/> class.
            /// </summary>
            /// <param name="text">Text to display in the button.</param>
            public InfoBarButton(string text)
                : base(text, null)
            {
            }

            /// <summary>
            /// Event that fires when the button is clicked.
            /// </summary>
            public event InfoBarEventHandler OnClick;

            /// <inheritdoc/>
            public override bool IsButton => true;

            /// <summary>
            /// Call to fire the <see cref="OnClick"/> event.
            /// </summary>
            /// <param name="e">Event arguments.</param>
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