using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using HarmonyLib;
using SpaceCraft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TPCA.Archipelago.Enums;
using TPCA.Core;

namespace TPCA.Archipelago
{
    internal class ArchipelagoClient
    {
        private const string MinArchipelagoVersion = "0.6.2";
        private const string GameName = "The Planet Crafter";

        public ArchipelagoState ArchipelagoState { get; private set; }
        public bool IsConnected => session?.Socket.Connected ?? false;

        private ArchipelagoSession session;

        private bool isAttemptingConnection;

        public LoginResult TryConnect()
        {
            if (IsConnected || isAttemptingConnection)
            {
                return new LoginFailure("Already trying to connect or already connected");
            }

            ArchipelagoState = ArchipelagoState.Connecting; // Not even sure this will work withy an async version of TryConnectAndLogin

            if (string.IsNullOrWhiteSpace(Plugin.State.Host))
            {
                ArchipelagoState = ArchipelagoState.ConnectionError;
                return new LoginFailure("Host Name is null or empty");
            }

            if (string.IsNullOrWhiteSpace(Plugin.State.PlayerName))
            {
                ArchipelagoState = ArchipelagoState.ConnectionError;
                return new LoginFailure("Player Name is null or empty");
            }

            try
            {
                session = ArchipelagoSessionFactory.CreateSession(Plugin.State.Host);
                SetupSession();
            }
            catch (Exception ex)
            {
                ArchipelagoState = ArchipelagoState.ConnectionError;
                return new LoginFailure($"Cannot create archipelago session: {ex}");
            }

            LoginResult loginResult;
            isAttemptingConnection = true;

            try
            {
                loginResult = session.TryConnectAndLogin(GameName, Plugin.State.PlayerName, ItemsHandlingFlags.AllItems, new Version(MinArchipelagoVersion), tags: null, uuid: null, Plugin.State.Password, requestSlotData: true);
            }
            catch (Exception ex)
            {
                loginResult = new LoginFailure(ex.ToString());
            }

            if (loginResult is LoginFailure loginFailure)
            {
                isAttemptingConnection = false;
                session = null;
                ArchipelagoState = ArchipelagoState.ConnectionError;
                return new LoginFailure($"Cannot connect to AP server: {string.Join("\n", loginFailure.Errors)}");
            }

            Plugin.Log.LogInfo($"Successfully connected to {Plugin.State.Host} as {Plugin.State.PlayerName}");

            ArchipelagoState = ArchipelagoState.Connected;

            return loginResult as LoginSuccessful;
        }

        public void OnConnect(LoginSuccessful login)
        {
            if (Plugin.State.Guid != Plugin.ArchipelagoClient.GetServerGuid())
            {
                Plugin.Log.LogWarning($"{nameof(OnConnect)} => Connecting a save where the GUID doesn't match with the server automatically connected to, please check the informations of the server you are connected to!");
            }

            Plugin.State.SlotData = new SlotData(login.SlotData);
            Plugin.State.ItemByLocations = ScoutAllLocations();

            isAttemptingConnection = false;   
        }

        private void SetupSession()
        {
            session.Socket.ErrorReceived += SessionErrorReceived;
            session.Socket.SocketOpened += SessionSocketOpened;
            session.Socket.SocketClosed += SessionSocketClosed;
            session.Items.ItemReceived += SessionItemReceived;
            session.MessageLog.OnMessageReceived += SessionOnMessageReceived;
        }

        public void Disconnect()
        {
            if (!IsConnected)
            {
                return;
            }

            isAttemptingConnection = false;

            Plugin.Log.LogDebug("Disconnecting ...");
            Task.Run(() => { _ = session.Socket.DisconnectAsync(); }).Wait();
            Plugin.Log.LogDebug("Disconnected");

            session = null;

            ArchipelagoState = ArchipelagoState.NotConnected;
        }

        private void SessionErrorReceived(Exception ex, string message)
        {
            Plugin.Log.LogError(message);
            if (ex != null)
            {
                Plugin.Log.LogError(ex.ToString());
            }

            ArchipelagoState = ArchipelagoState.ConnectionError;

            Disconnect();
        }

        private void SessionSocketOpened()
        {
            ArchipelagoState = ArchipelagoState.Connected;
        }

        private void SessionSocketClosed(string reason)
        {
            Plugin.Log.LogError($"Connection to Archipelago lost: {reason}");

            ArchipelagoState = ArchipelagoState.NotConnected;

            Disconnect();
        }

        private void SessionItemReceived(IReceivedItemsHelper helper)
        {
            var item = helper.DequeueItem();
            string itemName = item.ItemName ?? item.ItemDisplayName;

            var player = item.Player;
            var playerName = player.Alias ?? player.Name ?? $"Player #{player.Slot}";

            Plugin.Log.LogInfo($"{nameof(SessionItemReceived)} => Received item {item.ItemId} - {itemName} from {playerName}");
            GameManager.IncomingItems.Enqueue(new()
            {
                Name = itemName,
                Flags = item.Flags,
                PlayerName = playerName,
                IsLocal = player == session.ConnectionInfo.Slot,
                IsTpcItem = true
            });
        }

        private void SessionOnMessageReceived(LogMessage message)
        {
            Plugin.Log.LogMessage(message);
        }

        public void SendLocation(Group group)
        {
            if (!IsConnected)
            {
                Plugin.Log.LogWarning($"Trying to send location {group.id} but there is no connection");
                return;
            }

            long locationId = session.Locations.GetLocationIdFromName(GameName, group.id);
            Plugin.Log.LogInfo($"Trying to send location {locationId}");
            _ = session.Locations.CompleteLocationChecksAsync(locationId);

            // Show popup with the item sent informations when
            var itemInfos = Plugin.State.ItemByLocations[group.id];
            if (!itemInfos.IsLocal)
            {
                SendPopup(new PopupData(group.GetImage(), $"Send item '{itemInfos.Name}' to {itemInfos.PlayerName}", 5f));
            }
        }

        public static void SendPopup(PopupData popupData)
        {
            Plugin.Log.LogDebug($"{nameof(SendPopup)} => Message : {popupData.text}");
            AccessTools.Method(typeof(PopupsHandler), "AddToList").Invoke(Managers.GetManager<PopupsHandler>(), [popupData]);
        }

        public Dictionary<string, ApItemInfo> ScoutAllLocations()
        {
            if (!IsConnected)
            {
                return null;
            }

            var scouts = session.Locations.ScoutLocationsAsync([.. session.Locations.AllLocations]).ContinueWith(task =>
            {
                Dictionary<string, ApItemInfo> itemByLocations = [];
                foreach (var entry in task.Result)
                {
                    var player = entry.Value.Player;
                    string playerName = player.Alias ?? player.Name ?? $"Player #{player.Slot}";

                    itemByLocations[session.Locations.GetLocationNameFromId(entry.Key)] = new ApItemInfo
                    {
                        Name = entry.Value.ItemDisplayName,
                        Flags = entry.Value.Flags,
                        PlayerName = playerName,
                        IsLocal = player == session.ConnectionInfo.Slot,
                        IsTpcItem = entry.Value.ItemDisplayName == "NoItem" ? false : entry.Value.ItemGame == GameName
                    };
                }
                return itemByLocations;
            });
            scouts.Wait();

            return scouts.Result;
        }

        public void SendCompletion()
        {
            if (!IsConnected)
            {
                return;
            }

            Plugin.Log.LogInfo($"{nameof(SendCompletion)} => Sending completion, GG!");

            session.SetGoalAchieved();
        }

        internal bool FetchGameDatas()
        {
            if (!IsConnected)
            {
                return false;
            }

            FetchGuid();
            FetchLocations();

            return true;
        }

        private void FetchGuid()
        {
            if (string.IsNullOrEmpty(session.RoomState.Seed))
            {
                Plugin.Log.LogWarning("Couldn't retrieve SeedName from the server, no GUID will be set!");
            }

            Plugin.State.Guid = session.RoomState.Seed;

            Plugin.Log.LogDebug($"Retrieved GUID <{Plugin.State.Guid}>");
        }

        private void FetchLocations()
        {
            Plugin.State.ItemByLocations = ScoutAllLocations();

            if (!Plugin.State.ItemByLocations.Any())
            {
                Plugin.Log.LogWarning("Couldn't retrieve Locations from the server, no Locations will be set!");
            }

            Plugin.Log.LogDebug($"Retrieved locations count <{Plugin.State.ItemByLocations.Count()}>");
        }

        internal string GetServerGuid()
        {
            return session.RoomState.Seed;
        }

        internal void NotifyGuidAlreadyExists()
        {
            ArchipelagoState = ArchipelagoState.ConnectedButGuidExists;
        }
    }
}
