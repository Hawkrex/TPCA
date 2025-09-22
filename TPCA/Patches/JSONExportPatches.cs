using HarmonyLib;
using Newtonsoft.Json;
using SpaceCraft;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TPCA.Patches
{
    [HarmonyPatch(typeof(JSONExport))]
    internal static class JSONExportPatches
    {
        public static Dictionary<string, ArchipelagoSaveInfos> ArchipelagoInfosByNames = new();

        [HarmonyPatch(nameof(JSONExport.SaveToJson))]
        [HarmonyPostfix]
        public static void SaveToJson_Postfix(string _saveFileName)
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
                Plugin.Log.LogDebug($"{nameof(SaveToJson_Postfix)} => Archipelago mode deactivated");
                return;
            }

            SaveArchipelagoSaveInfos(_saveFileName);
        }

        [HarmonyPatch(nameof(JSONExport.CreateNewSaveFile))]
        [HarmonyPrefix]
        public static void CreateNewSaveFile_Prefix(JsonableGameState gameSettings)
        {
            Plugin.Log.LogDebug($"{nameof(CreateNewSaveFile_Prefix)} => OK");
            if (Plugin.ArchipelagoModeDeactivated)
            {
                Plugin.Log.LogDebug($"{nameof(CreateNewSaveFile_Prefix)} => Archipelago mode deactivated");
                return;
            }

            Plugin.Log.LogDebug($"{nameof(CreateNewSaveFile_Prefix)} => Archipelago mode activated");

            gameSettings.hasPlayedIntro = true;
        }

        [HarmonyPatch(nameof(JSONExport.CreateNewSaveFile))]
        [HarmonyPostfix]
        public static void CreateNewSaveFile_Postfix(string saveFileName)
        {
            if (Plugin.ArchipelagoModeDeactivated)
            {
                Plugin.Log.LogDebug($"{nameof(CreateNewSaveFile_Postfix)} => Archipelago mode deactivated");
                return;
            }

            Plugin.Log.LogDebug($"{nameof(CreateNewSaveFile_Prefix)} => Archipelago mode activated");

            SaveArchipelagoSaveInfos(saveFileName);
        }

        private static void SaveArchipelagoSaveInfos(string _saveFileName)
        {
            Plugin.Log.LogDebug($"{nameof(SaveArchipelagoSaveInfos)} => Save file <{_saveFileName}>");

            string infosJson = JsonConvert.SerializeObject(Plugin.State.SaveInfos);
            Plugin.Log.LogDebug($"{nameof(SaveArchipelagoSaveInfos)} => Infos to save <{infosJson}>");

            string filePath = GetWorldStateSaveFilePath(_saveFileName);

            try
            {
                File.AppendAllText(filePath, "\r\n");
                File.AppendAllText(filePath, infosJson);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"{nameof(SaveArchipelagoSaveInfos)} => Error saving archipelago infos into save <{_saveFileName}> : {ex}");
            }
        }

        [HarmonyPatch(nameof(JSONExport.LoadFromJson))]
        [HarmonyPostfix]
        public static void LoadFromJson_Postfix(string _saveFileName)
        {
            if (ArchipelagoInfosByNames.ContainsKey(_saveFileName))
            {
                Plugin.Log.LogDebug($"{nameof(LoadFromJson_Postfix)} => Infos already loaded for save <{_saveFileName}>");
                return;
            }

            string filePath = GetWorldStateSaveFilePath(_saveFileName);
            string lastLine = File.ReadLines(filePath).Last();

            if (lastLine != "@") // Normal game save end with a line containing only a "@"
            {
                var infos = JsonConvert.DeserializeObject<ArchipelagoSaveInfos>(lastLine);
                ArchipelagoInfosByNames[_saveFileName] = infos;
            }
        }

        private static string GetWorldStateSaveFilePath(string _saveFileName)
        {
            return string.Format("{0}/{1}.json", Application.persistentDataPath, _saveFileName);
        }
    }
}
