using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using TPCA.Datas;
using UnityEngine;

namespace TPCA.Archipelago
{
    internal class ArchipelagoClient
    {
        private const string MinArchipelagoVersion = "0.6.2";
        private const string GameName = "The Planet Crafter";

        public bool IsConnected => session?.Socket.Connected ?? false;

        private ArchipelagoSession session;
        private DeathLinkHandler deathLinkHandler;

        private bool isAttemptingConnection;

        public LoginResult TryConnect()
        {
            if (IsConnected || isAttemptingConnection)
            {
                return new LoginFailure("Already trying to connect or already connected");
            }

            if (string.IsNullOrWhiteSpace(Plugin.State.Host))
            {
                return new LoginFailure("Host Name is null or empty");
            }

            if (string.IsNullOrWhiteSpace(Plugin.State.PlayerName))
            {
                return new LoginFailure("Player Name is null or empty");
            }

            try
            {
                session = ArchipelagoSessionFactory.CreateSession(Plugin.State.Host);
                SetupSession();
            }
            catch (Exception ex)
            {
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
                return new LoginFailure($"Cannot connect to AP server: {string.Join("\n", loginFailure.Errors)}");
            }

            Plugin.Log.LogInfo($"Successfully connected to {Plugin.State.Host} as {Plugin.State.PlayerName}");

            return loginResult as LoginSuccessful;
        }

        public bool Connect()
        {
            var loginResult = TryConnect();
            if (loginResult is LoginSuccessful loginSuccessful)
            {
                OnConnect(loginSuccessful);
                return true;
            }

            return false;
        }

        private void OnConnect(LoginSuccessful login)
        {
            if (!Plugin.State.SetupSession(login.SlotData, session.RoomState.Seed))
            {
                return;
            }

            deathLinkHandler = new DeathLinkHandler(session.CreateDeathLinkService(), Plugin.State.PlayerName, Plugin.State.SlotData.DeathLink);
            isAttemptingConnection = false;
            ScoutAllLocations();
        }

        private void SetupSession()
        {
            session.Socket.ErrorReceived += SessionErrorReceived;
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
            Task.Run(() => { _ = session.Socket.DisconnectAsync(); }).Wait();
            deathLinkHandler = null;
            session = null;
        }

        private void SessionErrorReceived(Exception ex, string message)
        {
            Plugin.Log.LogError(message);
            if (ex != null)
            {
                Plugin.Log.LogError(ex.ToString());
            }

            Disconnect();
        }

        private void SessionSocketClosed(string reason)
        {
            Plugin.Log.LogError($"Connection to Archipelago lost: {reason}");
            Disconnect();
        }

        private void SessionItemReceived(IReceivedItemsHelper helper)
        {
            int index = helper.Index - 1;
            var item = helper.DequeueItem();
            string itemName = item.ItemName;
            if (ApData.ItemNames.TryGetValue((ApItemId)item.ItemId, out var name))
            {
                itemName = name;
            }
            itemName ??= item.ItemDisplayName;

            var player = item.Player;
            var playerName = player.Alias ?? player.Name ?? $"Player #{player.Slot}";
            
            Plugin.Log.LogInfo($"{nameof(SessionItemReceived)} => Received item #{index}: {item.ItemId} - {itemName} from {playerName}");
            GameManager.IncomingItems.Enqueue(new()
            {
                Name = itemName,
                Index = index,
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

        public void SendLocation(string locationName)
        {
            if (!IsConnected)
            {
                Plugin.Log.LogWarning($"Trying to send location {locationName} but there is no connection");
                return;
            }

            long locationId = session.Locations.GetLocationIdFromName(GameName, locationName);
            Plugin.Log.LogWarning($"Trying to send location {locationId}");
            _ = session.Locations.CompleteLocationChecksAsync(locationId);
        }

        public bool SyncLocations(List<long> locations)
        {
            if (!IsConnected || locations == null || locations.Count == 0)
            {
                return false;
            }

            Plugin.Log.LogInfo($"Sending location checks: {string.Join(", ", locations)}");
            _ = session.Locations.CompleteLocationChecksAsync([.. locations]);
            return true;
        }

        public void ScoutAllLocations()
        {
            if (!IsConnected)
            {
                return;
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
                        IsTpcItem = entry.Value.ItemGame == GameName
                    };
                }
                return itemByLocations;
            });
            scouts.Wait();
            Plugin.State.ItemByLocations = scouts.Result;
        }

        public IEnumerable<string> GetLocationsNames()
        {
            return session.Locations.AllLocations.Select(x => session.Locations.GetLocationNameFromId(x));
        }

        public void SendCompletion()
        {
            if (!IsConnected)
            {
                return;
            }

            session.SetGoalAchieved();
        }

        public void SendDeath()
        {
            deathLinkHandler?.SendDeathLink();
        }

        public void ToggleDeathLink()
        {
            deathLinkHandler?.ToggleDeathLink();
        }

        public bool DeathLinkEnabled()
        {
            return deathLinkHandler.IsDeathLinkEnabled;
        }

        public void CheckForDeath()
        {
            if (GameManager.CanBeKilled())
            {
                deathLinkHandler?.KillPlayer();
            }
        }

        public void SendMessage(string message)
        {
            session?.Say(message);
        }

        internal bool CreateAndStoreGuid()
        {
            if (!IsConnected)
            {
                return false;
            }

            string guid = Guid.NewGuid().ToString();
            Plugin.State.SaveInfos = new ArchipelagoSaveInfos
            {
                Guid = guid,
                Host = Plugin.State.Host,
                PlayerName = Plugin.State.PlayerName,
                Password = Plugin.State.Password
            };
            session.DataStorage[$"{session.Players.ActivePlayer.Name}_Guid"] = Plugin.State.SaveInfos.Guid;
            Plugin.Log.LogDebug($"Saved GUID <{(string)session.DataStorage[$"{session.Players.ActivePlayer.Name}_Guid"]}> for player <{session.Players.ActivePlayer.Name}>");

            return true;
        }

        internal bool DoesGuidExist()
        {
            string savedGuid = session.DataStorage[$"{session.Players.ActivePlayer.Name}_Guid"];
            Plugin.Log.LogDebug($"GUID <{savedGuid}> exists for player <{session.Players.ActivePlayer.Name}>");

            return !string.IsNullOrEmpty(savedGuid);
        }
    }
}
