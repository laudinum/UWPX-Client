using System;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using UWPX_UI_Context.Classes.DataContext.Controls;
using UWPX_UI_Context.Classes.DataTemplates;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace UWPX_UI.Controls.Chat
{
    public sealed partial class ChatMessageListControl: UserControl
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public ChatDataTemplate Chat
        {
            get => (ChatDataTemplate)GetValue(ChatProperty);
            set => SetValue(ChatProperty, value);
        }
        public static readonly DependencyProperty ChatProperty = DependencyProperty.Register(nameof(ChatDataTemplate), typeof(ChatDataTemplate), typeof(ChatMessageListControl), new PropertyMetadata(null, ChatPropertyChanged));

        public bool IsDummy
        {
            get => (bool)GetValue(IsDummyProperty);
            set => SetValue(IsDummyProperty, value);
        }
        public static readonly DependencyProperty IsDummyProperty = DependencyProperty.Register(nameof(IsDummy), typeof(bool), typeof(ChatMessageListControl), new PropertyMetadata(false, OnIsDummyChanged));

        public readonly ChatMessageListControlContext VIEW_MODEL = new ChatMessageListControlContext();

        private ScrollViewer messagesScrollViewer = null;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        public ChatMessageListControl()
        {
            InitializeComponent();
            RegisterPointerWheelChanged();
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--


        #endregion

        #region --Misc Methods (Private)--
        private void UpdateView(DependencyPropertyChangedEventArgs args)
        {
            ChatDataTemplate oldChat = args.OldValue is ChatDataTemplate ? args.OldValue as ChatDataTemplate : null;
            ChatDataTemplate newChat = args.NewValue is ChatDataTemplate ? args.NewValue as ChatDataTemplate : null;
            VIEW_MODEL.UpdateView(oldChat, newChat);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/14534796/isupportincrementalloading-from-bottom-to-top
        /// </summary>
        private void RegisterPointerWheelChanged()
        {
            DependencyObject borderO = VisualTreeHelper.GetChild(messages_listView, 0);
            if (borderO is Border border)
            {
                /*border.Add
                DependencyObject scrollViewer = VisualTreeHelper.GetChild(border, 0);
                if (scrollViewer is ScrollViewer scroll)
                {

                }*/
            }
            if (messagesScrollViewer is null)
            {
                throw new InvalidOperationException("ScrollViewer not found.");
            }
        }

        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--
        private static void ChatPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            if (d is ChatMessageListControl control)
            {
                control.UpdateView(args);
            }
        }

        private static void OnIsDummyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ChatMessageListControl control)
            {
                control.VIEW_MODEL.MODEL.IsDummy = e.NewValue is bool b && b;
            }
        }

        #endregion

        private double desiredVerticalOffset = 0;
        private void Messages_listView_PointerWheelChanged(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            messagesScrollViewer = VisualTree.FindDescendant<ScrollViewer>(messages_listView);
            PointerPoint mousePosition = e.GetCurrentPoint(sender as UIElement);
            int delta = mousePosition.Properties.MouseWheelDelta;

            // calculate desiredOffset
            desiredVerticalOffset += delta;

            // limit desiredOffset.
            desiredVerticalOffset = desiredVerticalOffset < 0 ? 0 : desiredVerticalOffset;
            desiredVerticalOffset = desiredVerticalOffset > messagesScrollViewer.ScrollableHeight ? messagesScrollViewer.ScrollableHeight : desiredVerticalOffset;

            if (delta < 0 || delta > 0)
            {
                messagesScrollViewer.ChangeView(null, desiredVerticalOffset, null, false);
                e.Handled = true;
            }
        }
    }
}
