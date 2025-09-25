# TPCA

This is the client for the **people who wants to play The Planet Crafter Archipelago**, you can find the [ap world](https://github.com/Hawkrex/Archipelago-ThePlanetCrafter) here for the **Archipelago server host**.

## Installation
- Go to this repo `releases` page
- Download the `The Planet Crafter.yaml` and the `TPCA.zip`
- Customize your `The Planet Crafter.yaml` with your favorite text editor and send it to your **Archipelago server host**
- Install `BepinEx` following the [guide available here](https://docs.bepinex.dev/articles/user_guide/installation/index.html)

At this step, you should have the `BepinEx` folder in your game folder and a `plugins` folder in the BepinEx folder. 

- You can now unzip the `TPCA.zip` into the `plugins` folder of BepinEx

Now you should have the following path `The Planet Crafter\BepInEx\plugins\TPCA` and inside the following files (latest release) :
- Archipelago.MultiClient.Net.dll
- ArchipelagoItem.png
- Newtonsoft.Json.dll
- TPCA.dll

## Connection to Archipelago

On your save file list menu, click on New game to enter the creation menu. Choose a nice name, the planet on which you want to start and the game settings like a classic save.
An `Archipelago Mode` checkbox have been added to let you create a classic save without deleting the TPCA client plugin, just uncheck it and click on Create. Otherwise, leave this checkbox checked and take a look at the `Archipelago Settings` menu.
Fill the `Host` ("archipelago.gg:[PortNumber]" if hosted from the site, or "localhost:[PortNumber]" if hosted on your computer (default port is 38281), or "XXX.XXX.XXX.XXX/[PortNumber]" if hosted on a private AP server), your `Player Name` and the `Password` if any is needed.
You are now ready to create your save, click on the `First Connection` button, be sure to have the green message "Connected to AP server." and then click on `Create`. Now you can load your game and start playing. You can leave the game at any moment and reload the save that will automatically reconnect you with the informations filled before.

### Troubleshooting

The `First Connection` button can produce the following messages :
- `Connected to AP server.` in green: everything is alright and you can create your save.
- `Connected to AP server but a save has already been associated to this AP server and player! Create a new save at your own risk!` in yellow: you should have an existing save already created on the AP server, you can always bypass it and create a new save, this new save will become the associated save with the AP server.
- `Error connecting to AP server!` in red: there's been a problem connecting to the AP server, double check the informations you filled in the Archipelago Settings menu and make sure the AP server is launched.
- `Please connect to AP server first!` in yellow: you clicked on Create before connecting to the AP server with the First Connection button.

## Debug

You can help by activating the debug logs in BepinEx :
- Go to the `BepinEx/config` folder, edit the `BepInEx.cfg` config file by finding the `[Logging.Disk]` tag, underneath you should find the parameter `LogLevels` : make it `LogLevels = All` to enable all levels of logs
- Then play and reproduce the bug you found if you can (or prepare this config before you play the first time)
- During your play, the `LogOutput.log` file is generated in real time in the `BepinEx` folder
- Create an issue with a nice title, a description of what happened in the game and a copy of the `LogOutput.log` file

## Technical functioning of the AP save (Boring programmer stuff)

When clicking on the First Connection button, the client tries to connect to the AP server. Then by clicking on the Create button, the client generates a GUID (unique identifier) for the specified Player Name that is saved in both the TPC save file and on the AP server. The Host, Player Name and Password are also saved in the TPC save file to be able to connect automatically when loading the save. Afterward, the client disconnect from the AP server.

When loading a save, the client tries to connect to the AP server with the informations in the TPC save file, then checks if the GUID in the save file match the GUID on the server, and finally launch the game.
If there is a problem with the GUID, user only gets a warning and can continue at their own risks. If there is a problem connecting to the AP server when loading a save, user gets a warning and will be asked to fill new AP server informations to connect to a AP server. Once again the client check the matching of GUID that the user can bypass if wanted. A new GUID is the generated and is saved to the TPC save file as well as the new server connection informations.