using System;
using FlaxEditor.Content.Settings;
using FlaxEngine;
using Steamworks;
using SettingsBase = FlaxEngine.SettingsBase;

namespace FacepunchSteamworks
{
    /// <summary>
    /// Facepunch steam settings.
    /// </summary>
    public class FacepunchSteamSettings : SettingsBase
    {
        public uint AppId = 0;
    }
    
    /// <summary>
    /// The sample game plugin.
    /// </summary>
    /// <seealso cref="FlaxEngine.GamePlugin" />
    public class FacepunchSteamworksPlugin : GamePlugin
    {
        /// <inheritdoc />
        public FacepunchSteamworksPlugin()
        {
            _description = new PluginDescription
            {
                Name = "FacepunchSteamworks",
                Category = "Other",
                Author = "",
                AuthorUrl = null,
                HomepageUrl = null,
                RepositoryUrl = "https://github.com/Tryibion/FlaxFacepunchSteamworks",
                Description = "This is an implementation of Facepunch.Steamworks library for the Flax Engine.",
                Version = new Version(),
                IsAlpha = false,
                IsBeta = false,
            };
        }

        /// <inheritdoc />
        public override void Initialize()
        {
            base.Initialize();

            var settings = Engine.GetCustomSettings("Steam").Instance as FacepunchSteamSettings;
            if (settings == null)
            {
                Debug.LogError("No steam settings found.");
                Engine.RequestExit();
            }
            else
            {
                Debug.Write(LogType.Info, $"Steam settings found. AppId = {settings.AppId}.");
            }
            
#if !FLAX_EDITOR
#if !BUILD_RELEASE
            Dispatch.OnDebugCallback += OnDebugCallback;
#endif
            if (SteamClient.RestartAppIfNecessary(settings.AppId))
            {
                Engine.RequestExit();
            }

            try
            {
                SteamClient.Init(settings.AppId, false);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Steamworks failed to init: {e}");
            }
            
            Debug.Log($"SteamClient Valid: {SteamClient.IsValid}");

            Scripting.Update += OnUpdate;
#endif
        }

        private void OnDebugCallback(CallbackType type, string message, bool server)
        {
            Debug.Log($"Type: {type}, Server: {server}, Message: {message}");
        }

        private void OnUpdate()
        {
            SteamClient.RunCallbacks();
        }

        /// <inheritdoc />
        public override void Deinitialize()
        {
            
#if !FLAX_EDITOR
            SteamClient.Shutdown();
            Scripting.Update -= OnUpdate;
            
#if !BUILD_RELEASE
            Dispatch.OnDebugCallback -= OnDebugCallback;
#endif
#endif
            base.Deinitialize();
        }
    }
}
