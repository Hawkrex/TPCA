using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TPCA.Datas;

namespace TPCA.Archipelago
{
    internal class ArchipelagoClient
    {
        private const string MinArchipelagoVersion = "0.6.2";

        public bool IsConnected => session?.Socket.Connected ?? false;

        private ArchipelagoSession session;
        private DeathLinkHandler deathLinkHandler;

        private bool isAttemptingConnection;

        public bool Connect()
        {
            if (IsConnected || isAttemptingConnection)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(Plugin.State.Uri))
            {
                Plugin.Log.LogWarning("Cannot connect to archipelago server: Host Name is null or empty");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Plugin.State.PlayerName))
            {
                Plugin.Log.LogWarning("Cannot connect to archipelago server: Player Name is null or empty");
                return false;
            }

            try
            {
                session = ArchipelagoSessionFactory.CreateSession(Plugin.State.Uri);
                SetupSession();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Cannot create archipelago session: {ex}");
                return false;
            }

            LoginResult loginResult;
            isAttemptingConnection = true;

            try
            {
                loginResult = session.TryConnectAndLogin("The Planet Crafter", Plugin.State.PlayerName, ItemsHandlingFlags.AllItems, new Version(MinArchipelagoVersion), tags: null, uuid: null, Plugin.State.Password, requestSlotData: true);
            }
            catch (Exception ex)
            {
                loginResult = new LoginFailure(ex.ToString());
            }

            if (loginResult is LoginFailure loginFailure)
            {
                isAttemptingConnection = false;
                Plugin.Log.LogError($"Cannot connect to archipelago server: {string.Join("\n", loginFailure.Errors)}");
                session = null;
                return false;
            }

            Plugin.Log.LogInfo($"Successfully connected to {Plugin.State.Uri} as {Plugin.State.PlayerName}");
            OnConnect(loginResult as LoginSuccessful);

            return true;
        }

        private void OnConnect(LoginSuccessful login)
        {
            if (!Plugin.State.SetupSession(login.SlotData, session.RoomState.Seed))
            {
                return;
            }

            deathLinkHandler = new DeathLinkHandler(session.CreateDeathLinkService(), Plugin.State.PlayerName, Plugin.State.SlotData.DeathLink);
            isAttemptingConnection = false;
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

            Plugin.Log.LogInfo($"Received item #{index}: {item.ItemId} - {itemName}");
            GameManager.IncomingItems.Enqueue(new()
            {
                Name = itemName,
                Index = index
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

            long locationId = session.Locations.GetLocationIdFromName("The Planet Crafter", locationName);
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
    }
}
