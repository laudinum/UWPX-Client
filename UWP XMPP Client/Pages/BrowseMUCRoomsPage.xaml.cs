﻿using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using UWP_XMPP_Client.Classes;
using UWP_XMPP_Client.DataTemplates;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using XMPP_API.Classes;
using XMPP_API.Classes.Network.XML.Messages;
using XMPP_API.Classes.Network.XML.Messages.XEP_0030;

namespace UWP_XMPP_Client.Pages
{
    public sealed partial class BrowseMUCRoomsPage : Page
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public XMPPClient Client
        {
            get { return (XMPPClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }
        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("Client", typeof(XMPPClient), typeof(BrowseMUCRoomsPage), null);

        public string Server
        {
            get { return (string)GetValue(ServerProperty); }
            set { SetValue(ServerProperty, value); }
        }
        public static readonly DependencyProperty ServerProperty = DependencyProperty.Register("Server", typeof(string), typeof(BrowseMUCRoomsPage), null);

        private MessageResponseHelper<DiscoResponseMessage> messageResponseHelper;
        private CustomObservableCollection<MUCRoomTemplate> rooms;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 04/01/2018 Created [Fabian Sauter]
        /// </history>
        public BrowseMUCRoomsPage()
        {
            SystemNavigationManager.GetForCurrentView().BackRequested += BrowseMUCRoomsPage_BackRequested;
            this.messageResponseHelper = null;
            this.rooms = new CustomObservableCollection<MUCRoomTemplate>();
            this.InitializeComponent();
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        public MasterDetailsView getMasterDetailsView()
        {
            return masterDetail_pnl;
        }

        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--


        #endregion

        #region --Misc Methods (Private)--
        private void sendDisco()
        {
            if (Client != null)
            {
                main_grid.Visibility = Visibility.Collapsed;
                loading_grid.Visibility = Visibility.Visible;
                noneFound_notification.Dismiss();

                messageResponseHelper = new MessageResponseHelper<DiscoResponseMessage>(Client, onMessage, onTimeout);
                DiscoRequestMessage disco = new DiscoRequestMessage(Client.getXMPPAccount().getIdDomainAndResource(), Server, DiscoType.ITEMS);
                messageResponseHelper.start(disco);
            }
        }

        private bool onMessage(DiscoResponseMessage disco)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => showResultDisco(disco as DiscoResponseMessage)).AsTask();
            return true;
        }

        private void onTimeout()
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => showResultDisco(null)).AsTask();
        }

        private void showResultDisco(DiscoResponseMessage disco)
        {
            rooms.Clear();
            messageResponseHelper?.Dispose();
            messageResponseHelper = null;

            if (disco == null || disco.ITEMS == null || disco.ITEMS.Count <= 0)
            {
                // Show non found in app notification:
                noneFound_notification.Show("None found. Please retry!", 0);
                return;
            }
            else
            {
                foreach (DiscoItem i in disco.ITEMS)
                {
                    rooms.Add(new MUCRoomTemplate()
                    {
                        client = Client,
                        roomInfo = new MUCRoomInfo()
                        {
                            jid = i.JID ?? "",
                            name = i.NAME ?? (i.JID ?? "")
                        }
                    });
                }
            }

            loading_grid.Visibility = Visibility.Collapsed;
            main_grid.Visibility = Visibility.Visible;
        }

        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UiUtils.setBackgroundImage(backgroundImage_img);
        }

        private void BrowseMUCRoomsPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                return;
            }
            if (rootFrame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is BrowseMUCNavigationParameter)
            {
                BrowseMUCNavigationParameter parameter = e.Parameter as BrowseMUCNavigationParameter;
                Client = parameter.client;
                Server = parameter.server;

                sendDisco();
            }
        }

        private void refresh_btn_Click(object sender, RoutedEventArgs e)
        {
            sendDisco();
        }
        #endregion
    }
}
