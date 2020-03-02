using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TdLib;
using static TdLib.TdApi;

namespace MSEdgeBot
{
    public class TDLibHost
    {
        private static TdClient _client;
        private static bool _authNeeded;
        private static bool _isReady;
        private static Chat _thisChat;

        private static readonly ManualResetEventSlim ResetEvent = new ManualResetEventSlim();
        private static readonly ManualResetEventSlim ReadyEvent = new ManualResetEventSlim();
        private static readonly ManualResetEventSlim UploadedEvent = new ManualResetEventSlim();

        public static TDLibHost TDLibHostBot { get; set; }

        public TDLibHost() => CreateInstance().GetAwaiter().GetResult();

        public async Task CreateInstance()
        {
            TDLibHostBot = this;

            _client = new TdClient();
            TdLog.SetVerbosityLevel(0);

            _client.UpdateReceived += async (sender, update) =>
            {
                switch (update)
                {
                    case Update.UpdateOption option:
                        await _client.ExecuteAsync(new SetOption
                        {
                            DataType = option.DataType,
                            Extra = option.Extra,
                            Name = option.Name,
                            Value = option.Value
                        });
                        break;
                    case Update.UpdateAuthorizationState updateAuthorizationState when updateAuthorizationState.AuthorizationState.GetType() == typeof(AuthorizationState.AuthorizationStateWaitTdlibParameters):
                        await _client.ExecuteAsync(new SetTdlibParameters
                        {
                            Parameters = new TdlibParameters
                            {
                                ApiId = SecretKeys.API_ID,
                                ApiHash = SecretKeys.API_HASH,
                                ApplicationVersion = "1.0.0",
                                DeviceModel = "PC",
                                SystemLanguageCode = "en",
                                SystemVersion = "Win 10.0",
                                DatabaseDirectory = Path.Combine(Directory.GetCurrentDirectory(), "tdlib"),
                                EnableStorageOptimizer = true,
                                FilesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "tdlib")
                            }
                        });
                        break;
                    case Update.UpdateAuthorizationState updateAuthorizationState when updateAuthorizationState.AuthorizationState.GetType() == typeof(AuthorizationState.AuthorizationStateWaitEncryptionKey):
                        await _client.ExecuteAsync(new CheckDatabaseEncryptionKey());
                        break;
                    case Update.UpdateAuthorizationState updateAuthorizationState when updateAuthorizationState.AuthorizationState.GetType() == typeof(AuthorizationState.AuthorizationStateWaitCode):
                    case Update.UpdateAuthorizationState updateAuthorizationState2 when updateAuthorizationState2.AuthorizationState.GetType() == typeof(AuthorizationState.AuthorizationStateWaitPhoneNumber):
                        _authNeeded = true;
                        ResetEvent.Set();
                        break;
                    case Update.UpdateUser updateUser:
                        break;
                    case Update.UpdateConnectionState updateConnectionState when updateConnectionState.State.GetType() == typeof(ConnectionState.ConnectionStateReady):
                        ResetEvent.Set();
                        ReadyEvent.Set();
                        break;

                    case Update.UpdateMessageSendFailed uwu:
                    case Update.UpdateMessageSendSucceeded uwu2:
                        //what happens ?
                        //BIG *RIP*
                        UploadedEvent.Set();
                        break;

                    default:
                        break;
                }
            };

#if !PRODUCTION
            await Cont(TelegramBotSettings.DEV_CHANNEL);
#else
            await Cont(TelegramBotSettings.PROD_CHANNEL);
#endif
        }

        private async Task Cont(string channelName)
        {
            ResetEvent.Wait();

            if (_authNeeded)
            {
                await _client.ExecuteAsync(new CheckAuthenticationBotToken 
                {
#if !PRODUCTION
                    Token = SecretKeys.DEV_KEY
#else
                    Token = SecretKeys.PROD_KEY 
#endif
                });
            }

            ReadyEvent.Wait();

            if (_thisChat == null)
                _thisChat = await _client.SearchPublicChatAsync(channelName);

            _isReady = true;
        }

        public async Task Close()
        {
            try
            {
                await _client.CloseAsync();
                _client.Dispose();
                _isReady = false;
                _authNeeded = false;
            }
            catch (Exception)
            {
                //ok
            }
        }

        public async Task UploadFileAndMessage(string pathFileToUpload, string messageHtml)
        {
            if (_isReady)
            {
                var capt = await _client.ParseTextEntitiesAsync(messageHtml, new TextParseMode.TextParseModeHTML());
                

                await _client.SendMessageAsync(_thisChat.Id, 0, new SendMessageOptions { DisableNotification = true }, 
                    null, new InputMessageContent.InputMessageDocument
                {
                    Document = new InputFile.InputFileLocal { Path = pathFileToUpload },
                    Caption = capt,
                });

                UploadedEvent.Wait();

                UploadedEvent.Reset();
            }
            else
            {
                throw new InvalidOperationException("Hmm, too early or not connected.");
            }
        }
    }
}
