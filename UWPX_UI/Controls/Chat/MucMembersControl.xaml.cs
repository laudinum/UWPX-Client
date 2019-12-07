﻿using Data_Manager2.Classes.DBTables;
using UWPX_UI_Context.Classes.DataContext.Controls.Chat;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XMPP_API.Classes;

namespace UWPX_UI.Controls.Chat
{
    public sealed partial class MucMembersControl: UserControl
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public readonly MucMembersControlContext VIEW_MODEL = new MucMembersControlContext();

        public ChatTable Chat
        {
            get { return (ChatTable)GetValue(ChatProperty); }
            set { SetValue(ChatProperty, value); }
        }
        public static readonly DependencyProperty ChatProperty = DependencyProperty.Register(nameof(Chat), typeof(ChatTable), typeof(MucMembersControl), new PropertyMetadata(null, OnChatChanged));

        public MUCChatInfoTable MucInfo
        {
            get { return (MUCChatInfoTable)GetValue(MucInfoProperty); }
            set { SetValue(MucInfoProperty, value); }
        }
        public static readonly DependencyProperty MucInfoProperty = DependencyProperty.Register(nameof(MucInfo), typeof(MUCChatInfoTable), typeof(MucMembersControl), new PropertyMetadata(null, OnMucInfoChanged));

        public XMPPClient Client
        {
            get { return (XMPPClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }
        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register(nameof(Client), typeof(XMPPClient), typeof(MucMembersControl), new PropertyMetadata(null, OnClientChanged));

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        public MucMembersControl()
        {
            InitializeComponent();
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--


        #endregion

        #region --Misc Methods (Private)--
        private void UpdateView(DependencyPropertyChangedEventArgs e)
        {
            VIEW_MODEL.UpdateView(e);
        }

        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--
        private static void OnChatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MucMembersControl mucInfoControl)
            {
                mucInfoControl.UpdateView(e);
            }
        }

        private static void OnClientChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MucMembersControl mucInfoControl)
            {
                mucInfoControl.UpdateView(e);
            }
        }

        private static void OnMucInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MucMembersControl mucInfoControl)
            {
                mucInfoControl.UpdateView(e);
            }
        }

        #endregion
    }
}