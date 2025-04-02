using System;
using System.IO;
using Flax.Build;
using Flax.Build.NativeCpp;

public class FacepunchSteamworks : GameModule
{
    /// <inheritdoc />
    public override void Setup(BuildOptions options)
    {
        base.Setup(options);

        // Here you can modify the build options for your game module
        // To reference another module use: options.PublicDependencies.Add("Audio");
        // To add C++ define use: options.PublicDefinitions.Add("COMPILE_WITH_FLAX");
        // To learn more see scripting documentation.
        BuildNativeCode = false;
        
        Tags["Network"] = string.Empty;
        options.PublicDependencies.Add("Networking");
        options.ScriptingAPI.SystemReferences.Add("System.Runtime");
        
        options.ScriptingAPI.IgnoreMissingDocumentationWarnings = true;

        var facepunchLibsPath = Path.Combine(FolderPath, "..", "..", "Content", "FacepunchLibs");
        switch (options.Platform.Target)
        {
            case TargetPlatform.Windows:
                options.ScriptingAPI.FileReferences.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Win64.dll"));
                options.DependencyFiles.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Win64.xml"));
                options.DependencyFiles.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Win64.pdb"));
                break;
            case TargetPlatform.Linux:
                options.ScriptingAPI.FileReferences.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Posix.dll"));
                options.DependencyFiles.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Posix.xml"));
                options.DependencyFiles.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Posix.pdb"));
                break;
            case TargetPlatform.Mac:
                options.ScriptingAPI.FileReferences.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Posix.dll"));
                options.DependencyFiles.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Posix.xml"));
                options.DependencyFiles.Add(Path.Combine(facepunchLibsPath, "Facepunch.Steamworks.Posix.pdb"));
                break;
            default: /*throw new InvalidPlatformException(options.Platform.Target)*/ break;
        }
        
    }
}
