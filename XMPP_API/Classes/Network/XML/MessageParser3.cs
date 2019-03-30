using System.Collections;
using System.Collections.Generic;
using System.Xml;
using XMPP_API.Classes.Network.XML.Messages;

namespace XMPP_API.Classes.Network.XML
{
    public class MessageParser3
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public readonly MessageParserStats STATS;
        private readonly XmlReaderSettings READER_SETTINGS;

        private readonly ArrayList z;

        public delegate void RegisterMessageParserHandler(XMPPClient client, OmemoSessionBuildErrorEventArgs args);
        public event RegisterMessageParserHandler OnRegisterMessageParser;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        public MessageParser3()
        {
            this.STATS = new MessageParserStats();
            this.READER_SETTINGS = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment };
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        public List<AbstractMessage> parseMessages(ref string msg)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            List<AbstractMessage> messages = parseMessageInternal(ref msg);
            stopwatch.Stop();
            STATS.onMeasurement(stopwatch.ElapsedMilliseconds);
            return messages;
        }

        public void registerHandler()
        {

        }

        #endregion

        #region --Misc Methods (Private)--


        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}
