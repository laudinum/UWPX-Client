using Data_Manager2.Classes;
using Push_App_Server.Classes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XMPP_API.Classes;

namespace UWP_XMPP_Client.Pages.SettingsPages
{
    public sealed partial class BackgroundTasksSettingsPage : Page
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        private ObservableCollection<string> accounts;
        private List<XMPPClient> clients;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 04/09/2017 Created [Fabian Sauter]
        /// </history>
        public BackgroundTasksSettingsPage()
        {
            this.accounts = new ObservableCollection<string>();
            this.clients = null;
            this.InitializeComponent();
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += AbstractBackRequestPage_BackRequested;
            loadAccounts();
            loadSettings();
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--


        #endregion

        #region --Misc Methods (Private)--
        private void loadSettings()
        {
            disablePush_tgls.IsOn = Settings.getSettingBoolean(SettingsConsts.DISABLE_PUSH);
            pushServer_tbx.Text = Consts.PUSH_SERVER_ADDRESS;
        }

        public void loadAccounts()
        {
            clients = ConnectionHandler.INSTANCE.getClients();
            if (clients != null)
            {
                accounts.Clear();
                foreach (XMPPClient c in clients)
                {
                    accounts.Add(c.getXMPPAccount().getIdAndDomain());
                }
            }
        }

        private async Task requestPushChannelAsync()
        {
            requestPushChannel_btn.IsEnabled = false;
            requestPushChannel_prgr.IsActive = true;
            requestPushChannel_prgr.Visibility = Visibility.Visible;
            info_notif.Dismiss();

            PushNotificationChannel channel = await PushManager.requestPushChannelAsync();
            showPushChannel(channel);

            if (channel == null)
            {
                info_notif.Show("Failed to request push channel! (channel == null)");
            }
            else
            {
                info_notif.Show("Successfully requested push channel!");
            }

            requestPushChannel_btn.IsEnabled = true;
            requestPushChannel_prgr.IsActive = false;
            requestPushChannel_prgr.Visibility = Visibility.Collapsed;
        }

        private void showPushChannel(PushNotificationChannel channel)
        {
            pushChannelUrl_tbx.Text = (channel?.Uri) ?? "";
            pushChannelExpirationDate_tbx.Text = (channel?.ExpirationTime.ToString()) ?? "";
        }

        private async Task sendToServerAsync()
        {
            sendToPushServer_btn.IsEnabled = false;
            sendToPushServer_prgr.IsActive = true;
            sendToPushServer_prgr.Visibility = Visibility.Visible;
            serverCert_tbx.Text = "";

            if(account_cbx.SelectedIndex >= 0 && account_cbx.SelectedIndex < clients.Count)
            {
                DataWriter dW = await PushManager.connectAndSendChannelAsync(clients[account_cbx.SelectedIndex]);

                serverCert_tbx.Text = dW.getCertificateInformation() ?? "";

                if (dW.success)
                {
                    info_notif.Show("Success! View the logs for more information.");
                }
                else
                {
                    info_notif.Show("Failed to connect to the push server! View the logs for more information.");
                }
            }
            else
            {
                info_notif.Show("Failed to send message to push server! Select an account first!");
            }

            sendToPushServer_btn.IsEnabled = true;
            sendToPushServer_prgr.IsActive = false;
            sendToPushServer_prgr.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--
        private void AbstractBackRequestPage_BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
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

        private void disablePush_tgls_Toggled(object sender, RoutedEventArgs e)
        {
            Settings.setSetting(SettingsConsts.DISABLE_PUSH, disablePush_tgls.IsOn);
        }

        private async void sendToPushServer_btn_Click(object sender, RoutedEventArgs e)
        {
            await sendToServerAsync();
        }

        private async void requestPushChannel_btn_Click(object sender, RoutedEventArgs e)
        {
            await requestPushChannelAsync();
        }

        private void account_cbx_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (account_cbx.Items.Count > 0)
            {
                account_cbx.SelectedIndex = 0;
            }
        }

        #endregion
    }
}
