using System;

namespace XMPP_API.Classes.Network
{
    public abstract class AbstractMessageParseHandler : IComparable
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        /// <summary>
        /// The priority of the parse handler.
        /// Higher values result in a higher priority.
        /// </summary>
        public readonly int PRIORITY;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--


        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        public int CompareTo(object obj)
        {
            if (obj is AbstractMessageParseHandler handler)
            {
                return PRIORITY.CompareTo(handler.PRIORITY);
            }
            return -1;
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
