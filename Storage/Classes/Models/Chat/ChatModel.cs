﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Storage.Classes.Models.Account;
using Storage.Classes.Models.Omemo;
using XMPP_API.Classes;

namespace Storage.Classes.Models.Chat
{
    public class ChatModel: AbstractModel, IComparable
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        [NotMapped]
        private int _id;

        /// <summary>
        /// The bare JID of the chat/room e.g. 'coven@chat.shakespeare.lit'.
        /// </summary>
        [Required]
        public string bareJid
        {
            get => _bareJid;
            set => SetProperty(ref _bareJid, value);
        }
        [NotMapped]
        private string _bareJid;

        /// <summary>
        /// The bare JID of the account, the chat belongs to.
        /// </summary>
        [Required]
        public string accountBareJid
        {
            get => _accountBareJid;
            set => SetProperty(ref _accountBareJid, value);
        }
        [NotMapped]
        private string _accountBareJid;

        /// <summary>
        /// A custom name for the chat. Has to be null in case it's the same like the <see cref="bareJid"/>.
        /// </summary>
        public string customName
        {
            get => _customName;
            set => SetProperty(ref _customName, value);
        }
        [NotMapped]
        private string _customName;

        /// <summary>
        /// Whether there are chat messages for the chat available/the chat has been started.
        /// </summary>
        public bool isChatActive
        {
            get => _isChatActive;
            set => SetProperty(ref _isChatActive, value);
        }
        [NotMapped]
        private bool _isChatActive;

        /// <summary>
        /// The <see cref="DateTime"/> we had the last activity in this chat, like received/send a chat message.
        /// </summary>
        [Required]
        public DateTime lastActive
        {
            get => _lastActive;
            set => SetProperty(ref _lastActive, value);
        }
        [NotMapped]
        private DateTime _lastActive;

        /// <summary>
        /// Has the chat been muted by the user.
        /// </summary>
        [Required]
        public bool muted
        {
            get => _muted;
            set => SetProperty(ref _muted, value);
        }
        [NotMapped]
        private bool _muted;

        /// <summary>
        /// Presence subscription state e.g. 'both' or 'from'.
        /// </summary>
        public string subscription
        {
            get => _subscription;
            set => SetProperty(ref _subscription, value);
        }
        [NotMapped]
        private string _subscription;

        /// <summary>
        /// Is the chat part of the roster for one-to-one chats, or a part of the users bookmarks for MUCs.
        /// </summary>
        [Required]
        public bool inRoster
        {
            get => _inRoster;
            set => SetProperty(ref _inRoster, value);
        }
        [NotMapped]
        private bool _inRoster;


        /// <summary>
        /// True in case a presence subscription request to the targets presence is pending.
        /// </summary>
        [Required]
        public bool subscriptionRequested
        {
            get => _subscriptionRequested;
            set => SetProperty(ref _subscriptionRequested, value);
        }
        [NotMapped]
        private bool _subscriptionRequested;

        /// <summary>
        /// The status message for the chat.
        /// </summary>
        public string status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }
        [NotMapped]
        private string _status;

        /// <summary>
        /// The current chat presence e.g. online, dnd, xa, ...
        /// </summary>
        [Required]
        public Presence presence
        {
            get => _presence;
            set => SetProperty(ref _presence, value);
        }
        [NotMapped]
        private Presence _presence;

        /// <summary>
        /// The state of the chat (XEP-0085). Only interesting during runtime.
        /// </summary>
        [NotMapped]
        public string chatState
        {
            get => _chatState;
            set => SetProperty(ref _chatState, value);
        }
        [NotMapped]
        private string _chatState;

        /// <summary>
        /// The type of the chat e.g. MUC/MIX/...
        /// </summary>
        [Required]
        public ChatType chatType
        {
            get => _chatType;
            set => SetProperty(ref _chatType, value);
        }
        [NotMapped]
        private ChatType _chatType;

        /// <summary>
        /// Information about the state of OMEMO for this chat.
        /// </summary>
        [Required]
        public OmemoChatInformationModel omemoInfo
        {
            get => _omemoInfo;
            set => SetOmemoInfoProperty(value);
        }
        [NotMapped]
        private OmemoChatInformationModel _omemoInfo;


        /// <summary>
        /// The optional MUC information in case <see cref="chatType"/> == <seealso cref="ChatType.MUC"/>.
        /// </summary>
        [ForeignKey(nameof(muc) + "Id")]
        public MucInfoModel muc
        {
            get => _muc;
            set => SetMucProperty(value);
        }
        [NotMapped]
        private MucInfoModel _muc;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        public ChatModel() { }

        public ChatModel(string chatBareJid, AccountModel account)
        {
            bareJid = chatBareJid;
            accountBareJid = account.bareJid;
            isChatActive = false;
            lastActive = DateTime.Now;
            muted = false;
            subscription = null;
            inRoster = false;
            subscriptionRequested = false;
            status = null;
            presence = Presence.Unavailable;
            chatType = ChatType.CHAT;
            omemoInfo = new OmemoChatInformationModel(chatBareJid);

        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        private void SetOmemoInfoProperty(OmemoChatInformationModel value)
        {
            OmemoChatInformationModel old = _omemoInfo;
            if (SetProperty(ref _omemoInfo, value, nameof(omemoInfo)))
            {
                if (!(old is null))
                {
                    old.PropertyChanged -= OnOmemoPropertyChanged;
                }
                if (!(value is null))
                {
                    value.PropertyChanged += OnOmemoPropertyChanged;
                }
            }
        }

        private void SetMucProperty(MucInfoModel value)
        {
            MucInfoModel old = _muc;
            if (SetProperty(ref _muc, value, nameof(muc)))
            {
                if (!(old is null))
                {
                    old.PropertyChanged -= OnMucPropertyChanged;
                }
                if (!(value is null))
                {
                    value.PropertyChanged += OnMucPropertyChanged;
                }
            }
        }

        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        public int CompareTo(object obj)
        {
            return obj is ChatModel c ? lastActive.CompareTo(c.lastActive) : -1;
        }

        #endregion

        #region --Misc Methods (Private)--


        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--
        private void OnOmemoPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(nameof(omemoInfo) + '.' + e.PropertyName);
        }

        private void OnMucPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Do not propagate own events and prevent circular propagation:
            if (!e.PropertyName.StartsWith(nameof(MucInfoModel.chat)))
            {
                base.OnPropertyChanged(nameof(muc) + '.' + e.PropertyName);
            }
        }

        #endregion
    }
}
