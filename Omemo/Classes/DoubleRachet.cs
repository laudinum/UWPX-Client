﻿using System;
using System.Collections.Generic;
using System.Linq;
using Omemo.Classes.Exceptions;
using Omemo.Classes.Keys;
using Omemo.Classes.Messages;

namespace Omemo.Classes
{
    public class DoubleRachet
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        private readonly IdentityKeyPair OWN_IDENTITY_KEY;
        private readonly IOmemoStorage STORAGE;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        public DoubleRachet(IdentityKeyPair ownIdentityKey, IOmemoStorage storage)
        {
            OWN_IDENTITY_KEY = ownIdentityKey;
            STORAGE = storage;
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        /// <summary>
        /// Encrypts the given plaintext message and returns the result.
        /// </summary>
        /// <param name="msg">The plain text message to encrypt.</param>
        /// <returns>A tuple consisting out of the cipher text and key-HMAC combination (Tuple[cipherText, keyHmac]).</returns>
        public Tuple<byte[], byte[]> EncryptMessasge(byte[] msg)
        {
            byte[] key = KeyHelper.GenerateSymetricKey();
            // 32 byte (256 bit) of salt. Initialized with 0s.
            byte[] hkdfOutput = CryptoUtils.HkdfSha256(key, new byte[32], "OMEMO Payload", 80);
            CryptoUtils.SplitKey(hkdfOutput, out byte[] encKey, out byte[] authKey, out byte[] iv);
            byte[] cipherText = CryptoUtils.Aes256CbcEncrypt(encKey, iv, msg);
            byte[] hmac = CryptoUtils.HmacSha256(authKey, cipherText);
            hmac = CryptoUtils.Truncate(hmac, 16);
            byte[] keyHmac = CryptoUtils.Concat(key, hmac);
            return new Tuple<byte[], byte[]>(cipherText, keyHmac);
        }

        /// <summary>
        /// Encrypts the given <paramref name="keyHmac"/> combination for all given devices and returns the resulting messages.
        /// Uses the given <see cref="OmemoSession"/> for encrypting.
        /// Calls <see cref="IOmemoStorage.StoreSession(OmemoProtocolAddress, OmemoSession)"/> on completion for each session.
        /// </summary>
        /// <param name="keyHmac">The key HMAC combination, that should be encrypted.</param>
        /// <param name="devices">A collection of devices, we should encrypt the message for.</param>
        /// <returns>A collection of encrypted messages for each device grouped by their bare JID (List[Tuple[bareJid, List[Tuple[deviceId, IOmemoMessage]]]]).</returns>
        public List<Tuple<string, List<Tuple<uint, IOmemoMessage>>>> EncryptKeyHmacForDevices(byte[] keyHmac, List<OmemoDeviceGroup> devices)
        {
            List<Tuple<string, List<Tuple<uint, IOmemoMessage>>>> msgs = new List<Tuple<string, List<Tuple<uint, IOmemoMessage>>>>();
            foreach (OmemoDeviceGroup group in devices)
            {
                List<Tuple<uint, IOmemoMessage>> groupMsgs = new List<Tuple<uint, IOmemoMessage>>();
                foreach (KeyValuePair<uint, OmemoSession> device in group.SESSIONS)
                {
                    OmemoSession session = device.Value;
                    OmemoAuthenticatedMessage authMsg = EncryptKeyHmacForDevices(keyHmac, device.Value, session.assData);

                    // To account for lost and out-of-order messages during the key exchange, OmemoKeyExchange structures are sent until a response by the recipient confirms that the key exchange was successfully completed.
                    if (session.nS == 0 || session.nR == 0)
                    {
                        OmemoKeyExchangeMessage kexMsg = new OmemoKeyExchangeMessage(session.preKeyId, session.signedPreKeyId, OWN_IDENTITY_KEY.pubKey, session.ek, authMsg);
                        groupMsgs.Add(new Tuple<uint, IOmemoMessage>(device.Key, kexMsg));
                    }
                    else
                    {
                        groupMsgs.Add(new Tuple<uint, IOmemoMessage>(device.Key, authMsg));
                    }

                    // Store the changed session:
                    STORAGE.StoreSession(new OmemoProtocolAddress(group.BARE_JID, device.Key), session);
                }
                msgs.Add(new Tuple<string, List<Tuple<uint, IOmemoMessage>>>(group.BARE_JID, groupMsgs));
            }
            return msgs;
        }

        /// <summary>
        /// Tries to decrypt the given <paramref name="cipherContent"/>.
        /// </summary>
        /// <param name="authMsg">The <see cref="OmemoAuthenticatedMessage"/> that should be used for decrypting the <paramref name="cipherContent"/>.</param>
        /// <param name="session">The <see cref="OmemoSession"/> that should be used for decryption.</param>
        /// <param name="cipherContent">The cipher text that should be decrypted.</param>
        /// <returns>On success the plain text for the given <paramref name="cipherContent"/>.</returns>
        public byte[] DecryptMessage(OmemoAuthenticatedMessage authMsg, OmemoSession session, byte[] cipherContent)
        {
            byte[] keyHmac = DecryptKeyHmacForDevice(authMsg, session);
            byte[] key = new byte[32];
            Buffer.BlockCopy(keyHmac, 0, key, 0, key.Length);
            byte[] hmacRef = new byte[16];
            Buffer.BlockCopy(keyHmac, key.Length, hmacRef, 0, hmacRef.Length);

            // 32 byte (256 bit) of salt. Initialized with 0s.
            byte[] hkdfOutput = CryptoUtils.HkdfSha256(key, new byte[32], "OMEMO Payload", 80);
            CryptoUtils.SplitKey(hkdfOutput, out byte[] encKey, out byte[] authKey, out byte[] iv);
            byte[] hmac = CryptoUtils.HmacSha256(authKey, cipherContent);
            hmac = CryptoUtils.Truncate(hmac, 16);
            if (!hmacRef.SequenceEqual(hmac))
            {
                throw new OmemoException("Failed to decrypt. HMAC does not match.");
            }
            return CryptoUtils.Aes256CbcDecrypt(encKey, iv, cipherContent);
        }

        #endregion

        #region --Misc Methods (Private)--
        /// <summary>
        /// Encrypts the given key and HMAC concatenation and returns the result.
        /// </summary>
        /// <param name="keyHmac">The key, HMAC concatenation result.</param>
        /// <param name="session">The <see cref="OmemoSession"/> between the sender and receiver.</param>
        /// <param name="assData">Encode(IK_A) || Encode(IK_B) => Concatenation of Alices and Bobs public part of their identity key.</param>
        private OmemoAuthenticatedMessage EncryptKeyHmacForDevices(byte[] keyHmac, OmemoSession session, byte[] assData)
        {
            byte[] mk = LibSignalUtils.KDF_CK(session.ckS, 0x01);
            session.ckS = LibSignalUtils.KDF_CK(session.ckS, 0x02);
            OmemoMessage omemoMessage = new OmemoMessage(session);
            ++session.nS;
            // 32 byte (256 bit) of salt. Initialized with 0s.
            byte[] hkdfOutput = CryptoUtils.HkdfSha256(mk, new byte[32], "OMEMO Message Key Material", 80);
            CryptoUtils.SplitKey(hkdfOutput, out byte[] encKey, out byte[] authKey, out byte[] iv);
            omemoMessage.cipherText = CryptoUtils.Aes256CbcEncrypt(encKey, iv, keyHmac);
            byte[] omemoMessageBytes = omemoMessage.ToByteArray();
            byte[] hmacInput = CryptoUtils.Concat(assData, omemoMessageBytes);
            byte[] hmacResult = CryptoUtils.HmacSha256(authKey, hmacInput);
            byte[] hmacTruncated = CryptoUtils.Truncate(hmacResult, 16);
            return new OmemoAuthenticatedMessage(hmacTruncated, omemoMessageBytes);
        }

        /// <summary>
        /// Tries a skipped message key for decrypting the key and HMAC concatenation (<see cref="OmemoMessage.cipherText"/>).
        /// </summary>
        /// <param name="msg">The <see cref="OmemoMessage"/> containing the key and HMAC concatenation (<see cref="OmemoMessage.cipherText"/>).</param>
        /// <param name="msgHmac">The HMAC of the <paramref name="msg"/>.</param>
        /// <param name="session">The <see cref="OmemoSession"/> that should be used for decryption.</param>
        /// <returns>key || HMAC on success, else null.</returns>
        private byte[] TrySkippedMessageKeys(OmemoMessage msg, byte[] msgHmac, OmemoSession session)
        {
            byte[] mk = session.MK_SKIPPED.GetMessagekey(msg.DH, msg.N);
            return !(mk is null) ? DecryptKeyHmacForDevice(mk, msg, msgHmac, session.assData) : null;
        }

        /// <summary>
        /// Skippes receiving message keys <paramref name="until"/>.
        /// </summary>
        /// <param name="session">The <see cref="OmemoSession"/> the keys should be skipped for.</param>
        /// <param name="until">Until which key should be skipped.</param>
        private void SkipMessageKeys(OmemoSession session, uint until)
        {
            if (session.nR + OmemoSession.MAX_SKIP < until)
            {
                throw new OmemoException("Failed to decrypt. Would skip to many message keys from " + session.nR + " to " + until + ", which is more than " + OmemoSession.MAX_SKIP + '.');
            }
            if (!(session.ckR is null))
            {
                while (session.nR < until)
                {
                    byte[] mk = LibSignalUtils.KDF_CK(session.ckR, 0x01);
                    session.ckR = LibSignalUtils.KDF_CK(session.ckR, 0x02);
                    session.MK_SKIPPED.SetMessageKey(session.dhR.pubKey, session.nR, mk);
                    ++session.nR;
                }
            }
        }

        /// <summary>
        /// Decrypts the key and HMAC concatenation (<see cref="OmemoMessage.cipherText"/>) with the given <paramref name="mk"/> and returns the result.
        /// </summary>
        /// <param name="mk">The message key that should be used for decryption.</param>
        /// <param name="msg">The <see cref="OmemoMessage"/> containing the key and HMAC concatenation (<see cref="OmemoMessage.cipherText"/>).</param>
        /// <param name="msgHmac">The HMAC of the <paramref name="msg"/>.</param>
        /// <param name="assData">Encode(IK_A) || Encode(IK_B) => Concatenation of Alices and Bobs public part of their identity key.</param>
        /// <returns>key || HMAC</returns>
        private byte[] DecryptKeyHmacForDevice(byte[] mk, OmemoMessage msg, byte[] msgHmac, byte[] assData)
        {
            // 32 byte (256 bit) of salt. Initialized with 0s.
            byte[] hkdfOutput = CryptoUtils.HkdfSha256(mk, new byte[32], "OMEMO Message Key Material", 80);
            CryptoUtils.SplitKey(hkdfOutput, out byte[] encKey, out byte[] authKey, out byte[] iv);
            byte[] hmacInput = CryptoUtils.Concat(assData, msg.ToByteArray());
            byte[] hmacResult = CryptoUtils.HmacSha256(authKey, hmacInput);
            byte[] hmacTruncated = CryptoUtils.Truncate(hmacResult, 16);
            if (!hmacTruncated.SequenceEqual(msgHmac))
            {
                throw new OmemoException("Failed to decrypt. HMAC of OmemoMessage does not match.");
            }
            return CryptoUtils.Aes256CbcDecrypt(encKey, iv, msg.cipherText);
        }

        /// <summary>
        /// Decrypts the key and HMAC concatenation (<see cref="OmemoMessage.cipherText"/>) with the given <paramref name="session"/>.
        /// </summary>
        /// <param name="authMsg">The <see cref="OmemoAuthenticatedMessage"/> containing the key and HMAC.</param>
        /// <param name="session">The <see cref="OmemoSession"/> that should be used for decryption.</param>
        /// <returns>key || HMAC</returns>
        private byte[] DecryptKeyHmacForDevice(OmemoAuthenticatedMessage authMsg, OmemoSession session)
        {
            OmemoMessage msg = new OmemoMessage(authMsg.OMEMO_MESSAGE);
            byte[] plainText = TrySkippedMessageKeys(msg, authMsg.HMAC, session);
            if (!(plainText is null))
            {
                return plainText;
            }

            if (session.dhR is null || !session.dhR.pubKey.Equals(msg.DH))
            {
                SkipMessageKeys(session, msg.PN);
                session.InitDhRatchet(msg);
            }
            SkipMessageKeys(session, msg.N);
            byte[] mk = LibSignalUtils.KDF_CK(session.ckR, 0x01);
            session.ckR = LibSignalUtils.KDF_CK(session.ckR, 0x02);
            ++session.nR;
            return DecryptKeyHmacForDevice(mk, msg, authMsg.HMAC, session.assData);
        }

        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}