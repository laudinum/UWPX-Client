using Data_Manager2.Classes;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;
using XMPP_API.Classes;

namespace Push_App_Server.Classes
{
    public class PushManager
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--


        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 19/11/2017 Created [Fabian Sauter]
        /// </history>
        public PushManager()
        {
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        public static void init()
        {
            //ConnectionHandler.INSTANCE.ClientConnected += INSTANCE_ClientConnected;
        }

        public static async Task<DataWriter> connectAndSendChannelAsync(XMPPClient client)
        {
            DataWriter dW = new DataWriter(client);
            await dW.connectAndSendAsync();
            return dW;
        }

        public static async Task<PushNotificationChannel> requestPushChannelAsync()
        {
            DataWriter dw = new DataWriter(null);
            return await dw.requestChannelAsync();
        }

        #endregion

        #region --Misc Methods (Private)--


        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--
        private static void INSTANCE_ClientConnected(ConnectionHandler handler, Data_Manager.Classes.Events.ClientConnectedEventArgs args)
        {
            Task.Run(async () =>
            {
                DataWriter dW = new DataWriter(args.CLIENT);
                await dW.connectAndSendAsync();
            });
        }

        #endregion
    }
}
