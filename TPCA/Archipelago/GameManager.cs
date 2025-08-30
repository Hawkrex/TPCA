using Newtonsoft.Json;
using SpaceCraft;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TPCA.Archipelago
{
    internal static class GameManager
    {
        public static Queue<ApItemInfo> IncomingItems { get; } = new();
        public static Queue<ApItemInfo> IncomingMessages { get; } = new();

        public static List<Group> AllGroups { get; set; }

        public static string DeathSource { get; private set; }

        private static bool saveNew;
        private static bool saveLoaded;
        private static bool saveInitialized;

        private static HashSet<Group> previouslyKnowUnlockedGroups = new HashSet<Group>();

        public static void SetupNewSave()
        {
            saveNew = true;
            saveLoaded = true;
            Plugin.State.ClearSave();
        }

        public static void LoadSave()
        {
            if (saveLoaded)
            {
                return;
            }

            saveNew = false;
            saveLoaded = true;

            Plugin.State.CheckedLocations ??= [];

            Plugin.State.IsValid = true;

            if (!Plugin.ArchipelagoClient.IsConnected)
            {
                Plugin.UpdateConnectionInfo(Plugin.State.Uri, Plugin.State.PlayerName, Plugin.State.Password);
                Plugin.ArchipelagoClient.Connect();
            }

            InitializeSave(false);
            _ = ConnectSave();
        }

        public static void InitializeSave(bool isNew)
        {
            if (saveInitialized)
            {
                return;
            }

            // Init save with options from Plugin.State.SlotData

            saveInitialized = true;
        }

        public static bool ConnectSave()
        {
            saveNew = true;
            /*if (!saveLoaded)
            {
                Plugin.Log.LogError("Save not loaded");
                return true;
            }

            if (!saveValid)
            {
                Plugin.Log.LogError("Save not valid");
                Plugin.ArchipelagoClient.Disconnect();
                return false;
            }*/

            if (!Plugin.ArchipelagoClient.IsConnected)
            {
                Plugin.Log.LogError("Not connected");
                return true;
            }

            string seed = Plugin.State.Seed;
            var slotData = Plugin.State.SlotData;
            bool isNew = saveNew;

            if (saveNew)
            {
                Plugin.Log.LogInfo("Save new");
                saveNew = false;
                Plugin.State.IsValid = true;

                UpdateSaveData();
            }
            else if (seed != Plugin.State.Seed)
            {
                Plugin.Log.LogError("Mismatched seed detected. Did you load the right save?");
                Plugin.ArchipelagoClient.Disconnect();
            }
            else
            {
                Plugin.Log.LogInfo("UpdateConnectionInfo");
                Plugin.UpdateConnectionInfo();
            }

            Plugin.Log.LogInfo("Init save ...");
            Plugin.ArchipelagoClient.SyncLocations(Plugin.State.CheckedLocations);
            InitializeSave(isNew);
            Plugin.Log.LogInfo("Init save done");

            return true;
        }

        public static void UpdateSaveData()
        {
            if (!Plugin.State.IsValid)
            {
                return;
            }

            var data = JsonConvert.SerializeObject(Plugin.State);
            // TODO SaveManager.SaveObject(SaveObjectId, data, SaveRoomId);
        }

        public static void ExitSave()
        {
            saveNew = false;
            saveLoaded = false;
            saveInitialized = false;
            Plugin.State.ClearConnection();
            Plugin.State.ClearSave();
            // TODO SaveManager.SaveObject(SaveObjectId, "", SaveRoomId);
        }

        public static void Update()
        {
            CheckForLocationsUnlocked();

            if (CanGetItem() && IncomingItems.TryDequeue(out var item))
            {
                if (item.Index < Plugin.State.ItemIndex)
                {
                    Plugin.Log.LogInfo($"Ignoring previously obtained item {item.Name}");
                }
                else
                {
                    Plugin.Log.LogInfo($"Obtained item {item.Name}");
                    Plugin.State.ItemIndex++;
                    var display = SendItem(item);
                }
            }
        }

        internal static bool hasInited = false;

        private static void CheckForLocationsUnlocked()
        {
            foreach (var unitType in (DataConfig.WorldUnitType[])Enum.GetValues(typeof(DataConfig.WorldUnitType)))
            {
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

                foreach (var group in groupsUnlocked)
                {
                    if (!previouslyKnowUnlockedGroups.Contains(group))
                    {
                        if (hasInited)
                        {
                            Plugin.Log.LogInfo($"SendLocation {group.id}");
                            Plugin.ArchipelagoClient.SendLocation(group.id);
                        }

                        previouslyKnowUnlockedGroups.Add(group);
                    }
                }
            }

            hasInited = true;
        }

        public static bool SendItem(ApItemInfo item)
        {
            Group groupToSend = AllGroups.FirstOrDefault(x => x.id == item.Name);

            if (groupToSend != null)
            {
                // Boolean passed to the prefix patch to not skip the real method
                Plugin.DontPrefix = true;
                UnlockedGroupsHandler.Instance.UnlockGroupGlobally(groupToSend);
                Plugin.DontPrefix = false;
            }

            return false;
        }

        public static bool CanGetItem()
        {
            // TODO
            return true;// Plugin.State.IsValid;
        }

        public static bool CanDisplayMessage()
        {
            // TODO
            return false;
        }

        public static bool CanBeKilled()
        {
            // TODO
            return false;
        }

        public static void ReceiveDeath(string source)
        {
            DeathSource = source;
        }
    }
}
