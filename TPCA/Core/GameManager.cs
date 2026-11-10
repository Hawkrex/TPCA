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
        public static ArchipelagoData State { get; set; } = new ArchipelagoData();

        public static Queue<ApItemInfo> IncomingItems { get; } = new();

        public static List<Group> AllGroups { get; set; }

        private static HashSet<Group> previouslyKnownUnlockedGroups = new HashSet<Group>();

        public static void LoadSave(ArchipelagoData state)
        {
            State = state;

            var loginResult = Plugin.ArchipelagoClient.TryConnect(); // On game loaded
            if (loginResult is LoginSuccessful loginSuccessful)
            {
                Plugin.ArchipelagoClient.OnConnect(loginSuccessful);
                Plugin.Log.LogInfo($"{nameof(LoadSave)} => Automatic connection on save loading successful");
            }
        }

        public static void Update()
        {
            CheckForLocationsUnlocked();

            // Check for items received
            if (IncomingItems.TryDequeue(out var item))
            {
                Plugin.Log.LogInfo($"{nameof(GameManager)}::{nameof(Update)} => Obtained item {item.Name}");
                var display = UnlockItem(item);
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
                    if (!previouslyKnownUnlockedGroups.Contains(group))
                    {
                        Plugin.Log.LogInfo($"{nameof(GameManager)}::{nameof(CheckForLocationsUnlocked)} => Unlocked blueprint location {group.id}");
                        Plugin.ArchipelagoClient.SendLocation(group); // To test

                        previouslyKnownUnlockedGroups.Add(group);
                    }
                }
            }
        }

        public static bool UnlockItem(ApItemInfo item)
        {
            var groupToSend = AllGroups.FirstOrDefault(x => x.id == item.Name);
            State.ItemByLocations[groupToSend.id] = item;

            if (groupToSend != null)
            {
                UnlockedGroupsHandler.Instance.UnlockGroupGlobally(groupToSend);
            }

            return false;
        }

        public static void CheckCompletion(DepartureType endingType)
        {
            Plugin.Log.LogDebug($"{nameof(CheckCompletion)} => Departure made with ending <{endingType}>");
            Plugin.Log.LogDebug($"{nameof(CheckCompletion)} => Goal was <{State.SlotData.Goal}>");

            switch (endingType)
            {
                case DepartureType.EndingSentinel:
                    if (State.SlotData.Goal == Goal.PrimeSentinel)
                    {
                        Plugin.ArchipelagoClient.SendCompletion();
                    }
                    break;
                case DepartureType.EndingRiley:
                    if (State.SlotData.Goal == Goal.PrimeSmugglers)
                    {
                        Plugin.ArchipelagoClient.SendCompletion();
                    }
                    break;
                case DepartureType.EndingWardens:
                    if (State.SlotData.Goal == Goal.PrimeWardens)
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
    }
}
