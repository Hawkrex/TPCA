using SpaceCraft;
using System;
using System.Collections.Generic;
using System.Linq;
using TPCA_Debug.Exporters;
using TPCA_Debug.Patches;
using UnityEngine;

namespace TPCA_Debug
{
    internal static class Debug
    {
        internal static string Location;

        internal static List<Group> AllGroups;

        internal static bool DontPrefix;

        private static bool itemsGiverToggle;
        private static Rect itemsGiverRect = new Rect(50, 50, 500, 300);

        private static bool progressionValuesModifiersToggle;
        private static Rect progressionValuesModifiersRect = new Rect(550, 50, 500, 350);

        private static bool debugToggle;
        private static Rect debugRect = new Rect(1000, 50, 300, 400);

        private static GUIStyle windowStyle;
        private static GUIStyle boxStyle;
        private static GUIStyle toggleStyle;
        private static GUIStyle textFieldStyle;
        private static GUIStyle buttonStyle;

        internal static float CurrentTerraformationValue;
        internal static bool FreezeCurrentTerraformation;
        private static float desiredTerraformationValue;

        internal static float CurrentOxygenValue;
        internal static bool FreezeCurrentOxygen;
        private static float desiredOxygenValue;

        internal static float CurrentHeatValue;
        internal static bool FreezeCurrentHeat;
        private static float desiredHeatValue;

        internal static float CurrentPressureValue;
        internal static bool FreezeCurrentPressure;
        private static float desiredPressureValue;

        internal static float CurrentBiomassValue;
        internal static bool FreezeCurrentBiomass;
        private static float desiredBiomassValue;

        internal static float CurrentPlantsValue;
        internal static bool FreezeCurrentPlants;
        private static float desiredPlantsValue;

        internal static float CurrentInsectsValue;
        internal static bool FreezeCurrentInsects;
        private static float desiredInsectsValue;

        internal static float CurrentAnimalsValue;
        internal static bool FreezeCurrentAnimals;
        private static float desiredAnimalsValue;

        internal static void OnGUI()
        {
            windowStyle = new GUIStyle(GUI.skin.window);
            windowStyle.normal.textColor = Color.white;

            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = MakeTex(2, 2, Color.black);

            toggleStyle = new GUIStyle(GUI.skin.toggle);
            toggleStyle.normal.textColor = Color.white;

            textFieldStyle = new GUIStyle(GUI.skin.textField);
            textFieldStyle.fontSize = 24;
            textFieldStyle.normal.textColor = Color.white;

            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.normal.textColor = Color.white;

            GUI.BeginGroup(new(0, 0, Screen.width, 30));
            GUI.Box(new(0, 0, Screen.width, 30), string.Empty);

            GUILayout.BeginArea(new Rect(5, 5, Screen.width, 30));
            GUILayout.BeginHorizontal();

            if (itemsGiverToggle = GUILayout.Toggle(itemsGiverToggle, "Items giver", buttonStyle))
            {
                itemsGiverRect = GUI.Window(1, itemsGiverRect, MakeItemsGiver, "Items giver");
            }

            if (progressionValuesModifiersToggle = GUILayout.Toggle(progressionValuesModifiersToggle, "Progression values modifier", buttonStyle))
            {
                progressionValuesModifiersRect = GUI.Window(2, progressionValuesModifiersRect, MakeProgressionValuesModifiers, "Progression values modifier");
            }
#if DEBUG
            if (debugToggle = GUILayout.Toggle(debugToggle, "Debug", "button"))
            {
                debugRect = GUI.Window(3, debugRect, MakeDebugWindow, "Debug");
            }
#endif
            GUI.EndGroup();
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private static void MakeItemsGiver(int windowId)
        {
            GUI.Box(new Rect(0, 17, 500, 300), string.Empty);

            GUILayout.BeginArea(new Rect(10, 30, 480, 260));
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("DeparturePlatform", buttonStyle))
            {
                var pm = Managers.GetManager<PlayersManager>().GetActivePlayerController();
                var inv = pm.GetPlayerBackpack().GetInventory();
                var gr = GroupsHandler.GetGroupViaId("DeparturePlatform");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);
            }

            if (GUILayout.Button("Extraction rocket", buttonStyle))
            {
                var pm = Managers.GetManager<PlayersManager>().GetActivePlayerController();
                var inv = pm.GetPlayerBackpack().GetInventory();

                var gr = GroupsHandler.GetGroupViaId("Rod-alloy");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);

                gr = GroupsHandler.GetGroupViaId("RocketReactor");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);

                gr = GroupsHandler.GetGroupViaId("Rod-osmium");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);

                gr = GroupsHandler.GetGroupViaId("Obsidian");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);

                gr = GroupsHandler.GetGroupViaId("SolarQuartz");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);

                gr = GroupsHandler.GetGroupViaId("BalzarQuartz");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);
            }

            if (GUILayout.Button("10 keys", buttonStyle))
            {
                var pm = Managers.GetManager<PlayersManager>().GetActivePlayerController();
                var inv = pm.GetPlayerBackpack().GetInventory();

                for (int i = 0; i < 10; i++)
                {
                    var gr = GroupsHandler.GetGroupViaId("WardenKey");
                    InventoriesHandler.Instance.AddItemToInventory(gr, inv);
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Starter pack", buttonStyle))
            {
                var pm = Managers.GetManager<PlayersManager>().GetActivePlayerController();
                var inv = pm.GetPlayerBackpack().GetInventory();
                var gr = GroupsHandler.GetGroupViaId("EquipmentIncrease4");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);

                gr = GroupsHandler.GetGroupViaId("MultiBuild");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);

                gr = GroupsHandler.GetGroupViaId("Backpack7");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);

                gr = GroupsHandler.GetGroupViaId("BootsSpeed3");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);

                gr = GroupsHandler.GetGroupViaId("OxygenTank5");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);

                gr = GroupsHandler.GetGroupViaId("Jetpack5");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);

                gr = GroupsHandler.GetGroupViaId("ScreenEnergy");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);

                gr = GroupsHandler.GetGroupViaId("ScreenUnlockables");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Microchip", buttonStyle))
            {
                var pm = Managers.GetManager<PlayersManager>().GetActivePlayerController();
                var inv = pm.GetPlayerBackpack().GetInventory();
                var gr = GroupsHandler.GetGroupViaId("BlueprintT1"); // Every tier of microchip blueprint
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);
            }

            if (GUILayout.Button("Fusion generator", buttonStyle))
            {
                var pm = Managers.GetManager<PlayersManager>().GetActivePlayerController();
                var inv = pm.GetPlayerBackpack().GetInventory();
                var gr = GroupsHandler.GetGroupViaId("EnergyGenerator6");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);
            }

            if (GUILayout.Button("Ore Extractor 3", buttonStyle))
            {
                var pm = Managers.GetManager<PlayersManager>().GetActivePlayerController();
                var inv = pm.GetPlayerBackpack().GetInventory();
                var gr = GroupsHandler.GetGroupViaId("OreExtractor3");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Iron", buttonStyle))
            {
                var pm = Managers.GetManager<PlayersManager>().GetActivePlayerController();
                var inv = pm.GetPlayerBackpack().GetInventory();
                var gr = GroupsHandler.GetGroupViaId("Iron");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);
            }

            if (GUILayout.Button("Titanium", buttonStyle))
            {
                var pm = Managers.GetManager<PlayersManager>().GetActivePlayerController();
                var inv = pm.GetPlayerBackpack().GetInventory();
                var gr = GroupsHandler.GetGroupViaId("Titanium");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);
            }

            if (GUILayout.Button("Aluminium", buttonStyle))
            {
                var pm = Managers.GetManager<PlayersManager>().GetActivePlayerController();
                var inv = pm.GetPlayerBackpack().GetInventory();
                var gr = GroupsHandler.GetGroupViaId("Aluminium");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);
            }

            if (GUILayout.Button("Silicon", buttonStyle))
            {
                var pm = Managers.GetManager<PlayersManager>().GetActivePlayerController();
                var inv = pm.GetPlayerBackpack().GetInventory();
                var gr = GroupsHandler.GetGroupViaId("Silicon");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);
            }

            if (GUILayout.Button("Cobalt", buttonStyle))
            {
                var pm = Managers.GetManager<PlayersManager>().GetActivePlayerController();
                var inv = pm.GetPlayerBackpack().GetInventory();
                var gr = GroupsHandler.GetGroupViaId("Cobalt");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);
            }

            if (GUILayout.Button("Magnesium", buttonStyle))
            {
                var pm = Managers.GetManager<PlayersManager>().GetActivePlayerController();
                var inv = pm.GetPlayerBackpack().GetInventory();
                var gr = GroupsHandler.GetGroupViaId("Magnesium");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);
            }

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Iridium", buttonStyle))
            {
                var pm = Managers.GetManager<PlayersManager>().GetActivePlayerController();
                var inv = pm.GetPlayerBackpack().GetInventory();
                var gr = GroupsHandler.GetGroupViaId("Iridium");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);
            }

            if (GUILayout.Button("Uranium", buttonStyle))
            {
                var pm = Managers.GetManager<PlayersManager>().GetActivePlayerController();
                var inv = pm.GetPlayerBackpack().GetInventory();
                var gr = GroupsHandler.GetGroupViaId("Uranim");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);
            }

            if (GUILayout.Button("Alloy", buttonStyle))
            {
                var pm = Managers.GetManager<PlayersManager>().GetActivePlayerController();
                var inv = pm.GetPlayerBackpack().GetInventory();
                var gr = GroupsHandler.GetGroupViaId("Alloy");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);
            }

            if (GUILayout.Button("Osmium", buttonStyle))
            {
                var pm = Managers.GetManager<PlayersManager>().GetActivePlayerController();
                var inv = pm.GetPlayerBackpack().GetInventory();
                var gr = GroupsHandler.GetGroupViaId("Osmium");
                InventoriesHandler.Instance.AddItemToInventory(gr, inv);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            GUI.DragWindow();
        }

        private static void MakeProgressionValuesModifiers(int windowId)
        {
            GUI.Box(new Rect(0, 17, 500, 350), string.Empty);

            float margin = 10;

            float toggleX = margin;
            float toggleWidth = 20 + 120;
            float inputBoxX = toggleX + toggleWidth + margin;
            float inputBoxWidth = 260;
            float buttonX = inputBoxX + inputBoxWidth + margin;
            float buttonWidth = 60;

            float terraformationY = 20 + margin;
            float oxygenY = terraformationY + 30 + margin;
            float heatY = oxygenY + 30 + margin;
            float pressureY = heatY + 30 + margin;
            float biomassY = pressureY + 30 + margin;
            float plantsY = biomassY + 30 + margin;
            float insectsY = plantsY + 30 + margin;
            float animalsY = insectsY + 30 + margin;

            // Terraformation
            FreezeCurrentTerraformation = GUI.Toggle(new Rect(toggleX, terraformationY, toggleWidth, 20), FreezeCurrentTerraformation, "Terraformation freeze", toggleStyle);

            if (!FreezeCurrentTerraformation)
            {
                desiredTerraformationValue = CurrentTerraformationValue;
            }

            desiredTerraformationValue = Convert.ToSingle(GUI.TextField(new Rect(inputBoxX, terraformationY, inputBoxWidth, 34), desiredTerraformationValue.ToString(), textFieldStyle));

            if (GUI.Button(new Rect(buttonX, terraformationY, buttonWidth, 34), "Apply", buttonStyle))
            {
                WorldUnitsPlanetPatches.SendNewValue(DataConfig.WorldUnitType.Terraformation, desiredTerraformationValue);
            }

            // Oxygen
            FreezeCurrentOxygen = GUI.Toggle(new Rect(toggleX, oxygenY, toggleWidth, 20), FreezeCurrentOxygen, "Oxygen freeze", toggleStyle);

            if (!FreezeCurrentOxygen)
            {
                desiredOxygenValue = CurrentOxygenValue;
            }

            desiredOxygenValue = Convert.ToSingle(GUI.TextField(new Rect(inputBoxX, oxygenY, inputBoxWidth, 34), desiredOxygenValue.ToString(), textFieldStyle));

            if (GUI.Button(new Rect(buttonX, oxygenY, buttonWidth, 34), "Apply", buttonStyle))
            {
                WorldUnitsPlanetPatches.SendNewValue(DataConfig.WorldUnitType.Oxygen, desiredOxygenValue);
            }

            // Heat
            FreezeCurrentHeat = GUI.Toggle(new Rect(toggleX, heatY, toggleWidth, 20), FreezeCurrentHeat, "Heat freeze", toggleStyle);

            if (!FreezeCurrentHeat)
            {
                desiredHeatValue = CurrentHeatValue;
            }

            desiredHeatValue = Convert.ToSingle(GUI.TextField(new Rect(inputBoxX, heatY, inputBoxWidth, 34), desiredHeatValue.ToString(), textFieldStyle));

            if (GUI.Button(new Rect(buttonX, heatY, buttonWidth, 34), "Apply", buttonStyle))
            {
                WorldUnitsPlanetPatches.SendNewValue(DataConfig.WorldUnitType.Heat, desiredHeatValue);
            }

            // Pressure
            FreezeCurrentPressure = GUI.Toggle(new Rect(toggleX, pressureY, toggleWidth, 20), FreezeCurrentPressure, "Pressure freeze", toggleStyle);

            if (!FreezeCurrentPressure)
            {
                desiredPressureValue = CurrentPressureValue;
            }

            desiredPressureValue = Convert.ToSingle(GUI.TextField(new Rect(inputBoxX, pressureY, inputBoxWidth, 34), desiredPressureValue.ToString(), textFieldStyle));

            if (GUI.Button(new Rect(buttonX, pressureY, buttonWidth, 34), "Apply", buttonStyle))
            {
                WorldUnitsPlanetPatches.SendNewValue(DataConfig.WorldUnitType.Pressure, desiredPressureValue);
            }

            // Biomass
            FreezeCurrentBiomass = GUI.Toggle(new Rect(toggleX, biomassY, toggleWidth, 20), FreezeCurrentBiomass, "Biomass freeze", toggleStyle);

            if (!FreezeCurrentBiomass)
            {
                desiredBiomassValue = CurrentBiomassValue;
            }

            desiredBiomassValue = Convert.ToSingle(GUI.TextField(new Rect(inputBoxX, biomassY, inputBoxWidth, 34), desiredBiomassValue.ToString(), textFieldStyle));

            if (GUI.Button(new Rect(buttonX, biomassY, buttonWidth, 34), "Apply", buttonStyle))
            {
                WorldUnitsPlanetPatches.SendNewValue(DataConfig.WorldUnitType.Biomass, desiredBiomassValue);
            }

            // Plants
            FreezeCurrentPlants = GUI.Toggle(new Rect(toggleX, plantsY, toggleWidth, 20), FreezeCurrentPlants, "Plants freeze", toggleStyle);

            if (!FreezeCurrentPlants)
            {
                desiredPlantsValue = CurrentPlantsValue;
            }

            desiredPlantsValue = Convert.ToSingle(GUI.TextField(new Rect(inputBoxX, plantsY, inputBoxWidth, 34), desiredPlantsValue.ToString(), textFieldStyle));

            if (GUI.Button(new Rect(buttonX, plantsY, buttonWidth, 34), "Apply", buttonStyle))
            {
                WorldUnitsPlanetPatches.SendNewValue(DataConfig.WorldUnitType.Plants, desiredPlantsValue);
            }

            // Insects
            FreezeCurrentInsects = GUI.Toggle(new Rect(toggleX, insectsY, toggleWidth, 20), FreezeCurrentInsects, "Insects freeze", toggleStyle);

            if (!FreezeCurrentInsects)
            {
                desiredInsectsValue = CurrentInsectsValue;
            }

            desiredInsectsValue = Convert.ToSingle(GUI.TextField(new Rect(inputBoxX, insectsY, inputBoxWidth, 34), desiredInsectsValue.ToString(), textFieldStyle));

            if (GUI.Button(new Rect(buttonX, insectsY, buttonWidth, 34), "Apply", buttonStyle))
            {
                WorldUnitsPlanetPatches.SendNewValue(DataConfig.WorldUnitType.Insects, desiredInsectsValue);
            }

            // Animals
            FreezeCurrentAnimals = GUI.Toggle(new Rect(toggleX, animalsY, toggleWidth, 20), FreezeCurrentAnimals, "Animals freeze", toggleStyle);

            if (!FreezeCurrentAnimals)
            {
                desiredAnimalsValue = CurrentAnimalsValue;
            }

            desiredAnimalsValue = Convert.ToSingle(GUI.TextField(new Rect(inputBoxX, animalsY, inputBoxWidth, 34), desiredAnimalsValue.ToString(), textFieldStyle));

            if (GUI.Button(new Rect(buttonX, animalsY, buttonWidth, 34), "Apply", buttonStyle))
            {
                WorldUnitsPlanetPatches.SendNewValue(DataConfig.WorldUnitType.Animals, desiredAnimalsValue);
            }

            GUI.DragWindow();
        }

        private static void MakeDebugWindow(int windowId)
        {
            GUI.Label(new(4, 4, 300, 20), "Archipelago debug menu");

            GUI.Label(new(4, 28, 300, 20), $"Received item : {Location}");

            var pressedButton = GUI.Button(new(4, 52, 300, 20), "Send PortalGenerator1");
            if (pressedButton)
            {
                SendItem("PortalGenerator1");
            }

            var pressedButton2 = GUI.Button(new(4, 76, 300, 20), "Send FlowerPot2");
            if (pressedButton2)
            {
                SendItem("FlowerPot2");
            }

            var exportGroupDataButton = GUI.Button(new(4, 100, 300, 20), "Export Group Datas to json");
            if (exportGroupDataButton)
            {
                JsonExporter.ExportGroupsDatas(AllGroups);
            }

            var exportArchipelagoDatasButton = GUI.Button(new(4, 124, 300, 20), "Export Group Datas to Archipelago");
            if (exportArchipelagoDatasButton)
            {
                ArchipelagoExporter.ExportGroupsDatas(AllGroups);
            }

            var exportArchipelagoDataNamesButton = GUI.Button(new(4, 148, 300, 20), "Export Group Data names to plain text");
            if (exportArchipelagoDataNamesButton)
            {
                PlainTextExporter.ExportGroupsDataNames(AllGroups);
            }

            GUI.DragWindow();
        }

        private static void SendItem(string groupId)
        {
            var groupToSend = AllGroups.FirstOrDefault(x => x.id == groupId);

            if (groupToSend != null)
            {
                // Boolean passed to the prefix patch to not skip the real method
                DontPrefix = true;
                UnlockedGroupsHandler.Instance.UnlockGroupGlobally(groupToSend);
                DontPrefix = false;
            }
        }
    }
}
