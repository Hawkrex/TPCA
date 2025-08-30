using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using System;
using System.Collections.Generic;

namespace TPCA.Archipelago
{
    internal class DeathLinkHandler
    {
        public bool IsDeathLinkEnabled { get; private set; }

        private readonly string playerName;
        private readonly DeathLinkService service;
        private readonly Queue<DeathLink> deathLinks = new();

        /// <summary>
        /// Instantiates our death link handler, sets up the hook for receiving death links, and enables death link if needed
        /// </summary>
        /// <param name="deathLinkService">The new DeathLinkService that our handler will use to send and receive death links</param>
        /// <param name="playerName">The player name</param>
        /// <param name="enableDeathLink">Whether we should enable death link or not on startup</param>
        public DeathLinkHandler(DeathLinkService deathLinkService, string playerName, bool enableDeathLink = false)
        {
            if (deathLinkService is null)
            {
                Plugin.Log.LogWarning("Cannot create DeathLinkHandler : DeathLinkService is null");
                return;
            }

            service = deathLinkService;
            service.OnDeathLinkReceived += DeathLinkReceived;

            if (string.IsNullOrEmpty(playerName))
            {
                Plugin.Log.LogWarning("Cannot create DeathLinkHandler : Player Name is null or empty");
                return;
            }

            this.playerName = playerName;

            IsDeathLinkEnabled = enableDeathLink;

            if (IsDeathLinkEnabled)
            {
                service.EnableDeathLink();
            }
        }

        /// <summary>
        /// Enables/disables death link
        /// </summary>
        public void ToggleDeathLink()
        {
            IsDeathLinkEnabled = !IsDeathLinkEnabled;

            if (IsDeathLinkEnabled)
            {
                service.EnableDeathLink();
            }
            else
            {
                service.DisableDeathLink();
            }
        }

        /// <summary>
        /// What happens when we receive a deathLink
        /// </summary>
        /// <param name="deathLink">Received Death Link object to handle</param>
        private void DeathLinkReceived(DeathLink deathLink)
        {
            deathLinks.Enqueue(deathLink);

            Plugin.Log.LogDebug(string.IsNullOrWhiteSpace(deathLink.Cause)
                ? $"Received Death Link from: {deathLink.Source}"
                : deathLink.Cause);
        }

        /// <summary>
        /// Can be called when in a valid state to kill the player, dequeuing and immediately killing the player with a
        /// message if we have a death link in the queue
        /// </summary>
        public void KillPlayer()
        {
            try
            {
                if (deathLinks.Count < 1)
                {
                    return;
                }

                var deathLink = deathLinks.Dequeue();
                // Text boxes have to be short, investigate using a dialogue box for showing full cause
                string cause = GetDeathLinkCause(deathLink);

                Plugin.Log.LogMessage(cause);
                GameManager.ReceiveDeath(cause);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(ex);
            }
        }

        /// <summary>
        /// Returns message for the player to see when a death link is received without a cause
        /// </summary>
        /// <param name="deathLink">death link object to get relevant info from</param>
        /// <returns>The death link cause</returns>
        private string GetDeathLinkCause(DeathLink deathLink)
        {
            return $"Death from {deathLink.Source}";
        }

        /// <summary>
        /// Called to send a death link to the multiworld
        /// </summary>
        public void SendDeathLink()
        {
            try
            {
                if (!IsDeathLinkEnabled)
                {
                    return;
                }

                Plugin.Log.LogMessage("Sharing your death...");

                // Add the cause here
                var linkToSend = new DeathLink(playerName);

                service.SendDeathLink(linkToSend);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(ex);
            }
        }
    }
}
