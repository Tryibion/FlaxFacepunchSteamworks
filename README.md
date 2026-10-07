# Facepunch.Steamworks for the Flax Engine
This is a simple plugin implimentation of the [Facepunch.Steamworks](https://github.com/Facepunch/Facepunch.Steamworks) library inside of the Flax Engine. Version integrated is version 2.5.0.

Enjoy this plugin? Here is a link to donate on [ko-fi](https://ko-fi.com/tryibion).

## Installation
To add this plugin project to your game, follow the instructions in the [Flax Engine documentation](https://docs.flaxengine.com/manual/scripting/plugins/plugin-project.html#automated-git-cloning) for adding a plugin project automatically using git or manually.

## Setup
The settings are auto created for you in the `Content/Settings` folder. You can open them and change the AppID to your Steam AppID.

## Excluding the plugin
To exclude the library and the plugin code from your build, you can add `EXCLUDE_STEAMWORKS` to the list of custom defines in the build settings or the game cooker.

You can add code similar to this in your `Game.Build.cs` file under the `Setup` method.

```csharp
if (!Configuration.CustomDefines.Contains("EXCLUDE_STEAMWORKS"))
{
    options.PublicDependencies.Add("FacepunchSteamworks");
    options.ScriptingAPI.Defines.Add("EXCLUDE_STEAMWORKS");
}
```

This will exclude the plugin and add a preprocessor definition that you can wrap your API calls in using `#if !EXCLUDE_STEAMWORKS`.