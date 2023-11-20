using Flax.Build;

public class FacepunchSteamworksEditorTarget : GameProjectEditorTarget
{
    /// <inheritdoc />
    public override void Init()
    {
        base.Init();

        // Reference the modules for editor
        Modules.Add("FacepunchSteamworks");
        Modules.Add("FacepunchSteamworksEditor");
    }
}
