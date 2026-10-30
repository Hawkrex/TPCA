using Archipelago.MultiClient.Net;
using SpaceCraft;
using System;
using System.Collections.Generic;
using System.Linq;
using TPCA.Archipelago;
using TPCA.Archipelago.Enums;
using static SpaceCraft.MachineDeparturePlatform;

namespace TPCA.Core
{
    internal static class GameManager
    {
        public static Queue<ApItemInfo> IncomingItems { get; } = new();
        public static Queue<ApItemInfo> IncomingMessages { get; } = new();

        public static List<Group> AllGroups { get; set; }

        public static string DeathSource { get; private set; }

        private static HashSet<Group> previouslyKnowUnlockedGroups = new HashSet<Group>();

        internal static bool isInitiated = false;

        public static void CheckCompletion(DepartureType endingType)
        {
            Plugin.Log.LogDebug($"{nameof(CheckCompletion)} => Departure made with ending <{endingType}>");
            Plugin.Log.LogDebug($"{nameof(CheckCompletion)} => Goal was <{Plugin.State.SlotData.Goal}>");

            switch (endingType)
            {
                case DepartureType.EndingSentinel:
                    if (Plugin.State.SlotData.Goal == Goal.PrimeSentinel)
                    {
                        Plugin.ArchipelagoClient.SendCompletion();
                    }
                    break;
                case DepartureType.EndingRiley:
                    if (Plugin.State.SlotData.Goal == Goal.PrimeSmugglers)
                    {
                        Plugin.ArchipelagoClient.SendCompletion();
                    }
                    break;
                case DepartureType.EndingWardens:
                    if (Plugin.State.SlotData.Goal == Goal.PrimeWardens)
                    {
                        Plugin.ArchipelagoClient.SendCompletion();
                    }
                    break;
                case DepartureType.EndingHumbleSentinel:
                    break;
                case DepartureType.EndingHumbleStarform:
                    break;
                case DepartureType.EndingHumbleSolo:
                    break;
                case DepartureType.EndingToxicitySolo:
                    break;
                case DepartureType.EndingToxicityEternastra:
                    break;
                case DepartureType.EndingToxicityDassetex:
                    break;
            }
        }

        public static void LoadSave(string filename)
        {
            Plugin.State.SaveDatas = Plugin.ArchipelagoDatasBySaveNames[filename];

            Plugin.State.Host = Plugin.State.SaveDatas.Host;
            Plugin.State.PlayerName = Plugin.State.SaveDatas.PlayerName;
            Plugin.State.Password = Plugin.State.SaveDatas.Password;

            Plugin.State.ItemByLocations = Plugin.State.SaveDatas.Locations;

            var loginResult = Plugin.ArchipelagoClient.TryConnect(); // On game load
            if (loginResult is LoginSuccessful loginSuccessful)
            {
                if (Plugin.State.SaveDatas.Guid != Plugin.ArchipelagoClient.GetServerGuid())
                {
                    Plugin.Log.LogWarning($"{nameof(LoadSave)} => Loading a save where the GUID doesn't match with the server automatically connected to, please check the informations of the server you are connected to!");
                }

                Plugin.ArchipelagoClient.OnConnect(loginSuccessful);
                Plugin.Log.LogInfo($"{nameof(LoadSave)} => Automatic connection on save loading successful");
            }

            _ = ConnectSave();
        }

        public static bool ConnectSave()
        {
            if (!Plugin.ArchipelagoClient.IsConnected)
            {
                Plugin.Log.LogError($"{nameof(GameManager)}::{nameof(ConnectSave)} => Not connected");
                return true;
            }

            string seed = Plugin.State.Seed;

            if (seed != Plugin.State.Seed)
            {
                Plugin.Log.LogError($"{nameof(GameManager)}::{nameof(ConnectSave)} => Mismatched seed detected. Did you load the right save?");
                Plugin.ArchipelagoClient.Disconnect();
            }

            Plugin.Log.LogInfo($"{nameof(GameManager)}::{nameof(ConnectSave)} => Init save ...");
            Plugin.ArchipelagoClient.SyncLocations(Plugin.State.CheckedLocations);

            Plugin.Log.LogInfo($"{nameof(GameManager)}::{nameof(ConnectSave)} => Init save done");

            return true;
        }

        public static void Update()
        {
            CheckForLocationsUnlocked();

            // Check for items received
            if (IncomingItems.TryDequeue(out var item))
            {
                bool conditionToDo = false;
                if (conditionToDo)
                {
                    Plugin.Log.LogInfo($"{nameof(GameManager)}::{nameof(Update)} => Ignoring previously obtained item {item.Name}");
                }
                else
                {
                    Plugin.Log.LogInfo($"{nameof(GameManager)}::{nameof(Update)} => Obtained item {item.Name}");
                    var display = UnlockItem(item);
                }
            }
        }

        private static void CheckForLocationsUnlocked()
        {
            foreach (var unitType in (DataConfig.WorldUnitType[])Enum.GetValues(typeof(DataConfig.WorldUnitType)))
            {
                // Copy from UnlockingHandler.GetUnitUnlockablesGroupSegment
                var allGroups = GroupsHandler.GetAllGroups();

                var groupsUnlocked = new List<Group>();
                foreach (var group in allGroups)
                {
                    UnlockingInfos unlockingInfos = group.GetUnlockingInfos();

                    if (unlockingInfos.GetWorldUnit() != DataConfig.WorldUnitType.Null && unlockingInfos.GetUnlockingValue() > 0.0 && unlockingInfos.GetWorldUnit() == unitType)// && (!onlySpecificToCurrentPlanet || unlockingInfos.GetIsSpecificToCurrentPlanet()))
                    {
                        if (unlockingInfos.GetIsUnlocked(false))
                        {
                            groupsUnlocked.Add(group);
                        }
                    }
                }

                // Copy from AlertUnlockables.CheckIfNewUnlockableUnlocked
                foreach (var group in groupsUnlocked)
                {
                    if (!previouslyKnowUnlockedGroups.Contains(group))
                    {
                        if (isInitiated)
                        {
                            Plugin.Log.LogInfo($"{nameof(GameManager)}::{nameof(CheckForLocationsUnlocked)} => Unlocked blueprint location {group.id}");
                            Plugin.ArchipelagoClient.SendLocation(group);
                        }

                        previouslyKnowUnlockedGroups.Add(group);
                    }
                }
            }

            isInitiated = true;
        }

        public static bool UnlockItem(ApItemInfo item)
        {
            var groupToSend = AllGroups.FirstOrDefault(x => x.id == item.Name);
            Plugin.State.ItemByLocations[groupToSend.id] = item;

            if (groupToSend != null)
            {
                UnlockedGroupsHandler.Instance.UnlockGroupGlobally(groupToSend);
            }

            return false;
        }
    }
}
