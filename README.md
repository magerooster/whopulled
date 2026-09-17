# WhoPulled

WhoPulled identifies the player who first engages a configured overworld hunt and reports the result in chat. B, A, and S ranks can be enabled or disabled in the plugin configuration window.

## How To Use

### Prerequisites

WhoPulled assumes all the following prerequisites are met:

* XIVLauncher, FINAL FANTASY XIV, and Dalamud have all been installed and the game has been run with Dalamud at least once.
* XIVLauncher is installed to its default directories and configurations.
  * If a custom path is required for Dalamud's dev directory, it must be set with the `DALAMUD_HOME` environment variable.
* A .NET Core 8 SDK has been installed and configured, or is otherwise available. (In most cases, the IDE will take care of this.)

### Building

1. Open up `SamplePlugin.slnx` in your C# editor of choice (likely [Visual Studio](https://visualstudio.microsoft.com) or [JetBrains Rider](https://www.jetbrains.com/rider/)).
2. Build the solution. By default, this will build a `Debug` build, but you can switch to `Release` in your IDE.
3. The resulting plugin can be found at `SamplePlugin/bin/x64/Debug/WhoPulled.dll` (or `Release` if appropriate.)

### Activating in-game

#### Local development

1. Build the project in Debug or Release mode.
2. Open `/xlsettings` and go to `Experimental`.
3. Add the full path to `SamplePlugin/bin/x64/Debug` under `Dev Plugin Locations`.
4. Open `/xlplugins`, then enable `WhoPulled` under `Dev Tools > Installed Dev Plugins`.

#### Custom repository

1. Open `/xlsettings` and go to `Experimental`.
2. Add this URL under `Custom Plugin Repositories`:

   `https://raw.githubusercontent.com/magerooster/whopulled/main/pluginmaster.json`

3. Open `/xlplugins` and refresh the plugin repositories.
4. Search for `WhoPulled` and select `Install` or `Update`.

After a new release, refresh the repositories before checking for updates.

### Plugin metadata

Dalamud loads `WhoPulled.json` next to the DLL and uses it for plugin metadata.
