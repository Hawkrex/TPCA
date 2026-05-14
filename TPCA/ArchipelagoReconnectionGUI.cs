using TPCA.Archipelago.Enums;
using UnityEngine;

namespace TPCA
{
    internal static class ArchipelagoReconnectionGUI
    {
        private const string hostNameControlName = "HostName";
        private const string playerNameControlName = "PlayerName";
        private const string passwordControlName = "Password";

        internal static void OnGUI()
        {
            float margin = 10;

            int boxWidth = 400;
            int boxHeight = 250;

            int titleFontSize = 26;

            int labelFontSize = 18;
            float labelWidth = 120;
            float labelHeight = 24;

            float textFieldWidth = 260;
            float textFieldHeight = labelHeight + 4;

            var titleStyle = new GUIStyle(GUI.skin.box);
            titleStyle.fontSize = titleFontSize;
            titleStyle.normal.textColor = Color.white;
            titleStyle.alignment = TextAnchor.UpperCenter;

            var labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = labelFontSize;
            labelStyle.normal.textColor = Color.white;

            var textFieldStyle = new GUIStyle(GUI.skin.textField);
            textFieldStyle.fontSize = labelFontSize;
            textFieldStyle.normal.textColor = Color.white;

            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = labelFontSize;
            buttonStyle.normal.textColor = Color.white;

            var e = Event.current;
            string focusedControlName = GUI.GetNameOfFocusedControl();
            bool isEnterKeyPressed = e.type == EventType.KeyUp &&
                e.keyCode is KeyCode.KeypadEnter or KeyCode.Return &&
                focusedControlName is hostNameControlName or playerNameControlName or passwordControlName;

            // Frame
            GUI.BeginGroup(new(1920 - (boxWidth + 60), (1080 - boxHeight) / 2, boxWidth, boxHeight));
            GUI.Box(new(0, 0, boxWidth, boxHeight), "Archipelago Connection", titleStyle);

            // Host name
            float hostY = titleStyle.fontSize + 30;
            GUI.Label(new(margin, hostY, labelWidth, labelHeight), "Host", labelStyle);
            GUI.SetNextControlName(hostNameControlName);
            Plugin.State.Host = GUI.TextField(new(margin + labelWidth, hostY, textFieldWidth, textFieldHeight), Plugin.State.Host, textFieldStyle);

            // Player name
            float slotNameY = hostY + textFieldHeight + margin;
            GUI.Label(new(margin, slotNameY, labelWidth, labelHeight), "Player Name", labelStyle);
            GUI.SetNextControlName(playerNameControlName);
            Plugin.State.PlayerName = GUI.TextField(new(margin + labelWidth, slotNameY, textFieldWidth, textFieldHeight), Plugin.State.PlayerName, textFieldStyle);

            // Password (optional)
            float passwordY = slotNameY + textFieldHeight + margin;
            GUI.Label(new(margin, passwordY, labelWidth, labelHeight), "Password", labelStyle);
            GUI.SetNextControlName(passwordControlName);
            Plugin.State.Password = GUI.PasswordField(new(margin + labelWidth, passwordY, textFieldWidth, textFieldHeight), Plugin.State.Password, "*"[0], textFieldStyle);

            float connectionStateX = (boxWidth - 450) / 2;
            float connectionStateY = passwordY + margin + 10;
            string connectionState = string.Empty;

            switch (Plugin.ArchipelagoClient.ArchipelagoState)
            {
                case ArchipelagoState.NotConnected:
                    connectionState = "Not connected to AP server.";
                    labelStyle.normal.textColor = Color.yellow;
                    break;

                case ArchipelagoState.Connecting: // Need to rewrite TryConnectAndLogin in async for this to work
                    connectionState = "Connecting to AP server ...";
                    labelStyle.normal.textColor = Color.white;
                    break;

                case ArchipelagoState.ConnectionError:
                    connectionState = "Error connecting to AP server!";
                    labelStyle.normal.textColor = Color.red;
                    break;

                case ArchipelagoState.ConnectedButGuidExists:
                    connectionState = "Connected to AP server but a save has already been associated to this AP server and player!";
                    labelStyle.normal.textColor = Color.yellow;
                    break;

                case ArchipelagoState.Connected:
                    connectionState = "Connected to AP server.";
                    labelStyle.normal.textColor = Color.green;
                    break;
            }

            labelStyle.fontSize = 16;
            labelStyle.alignment = TextAnchor.LowerCenter;
            GUI.Label(new(connectionStateX, connectionStateY, 450, labelHeight * 2), connectionState, labelStyle);

            float connectionX = (boxWidth - 200) / 2;
            float connectionY = connectionStateY + labelHeight * 2;
            GUI.enabled = !Plugin.ArchipelagoClient.IsConnected;
            bool isButtonClicked = GUI.Button(new(connectionX, connectionY, 200, 40), "Connect", buttonStyle);
            GUI.enabled = true;

            if (isEnterKeyPressed || isButtonClicked)
            {
                Plugin.ArchipelagoClient.Connect();
            }

            GUI.EndGroup();
        }
    }
}
