using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using JoksterCube.FairCraftingLuck.Settings;
using ServerSync;
using static JoksterCube.FairCraftingLuck.Settings.Constants;
using static JoksterCube.FairCraftingLuck.Settings.Constants.Plugin;

namespace JoksterCube.FairCraftingLuck;

[BepInPlugin(ModGUID, ModName, ModVersion)]
public class Plugin : BaseUnityPlugin
{
    internal static readonly ManualLogSource ModLogger = BepInEx.Logging.Logger.CreateLogSource(ModName);
    internal static string ConnectionError = string.Empty;

    // ModRequired stays false: players without the mod may join a modded server, and a modded player
    // may join an unmodded server (local settings apply). When both have it, versions must match exactly.
    // Always locked: synced settings come only from the server's config file (see EnforceServerConfigPatch).
    internal static readonly ConfigSync ServerConfigSync = new(ModGUID)
    {
        DisplayName = ModName,
        CurrentVersion = ModVersion,
        MinimumRequiredVersion = ModVersion,
        IsLocked = true
    };

    private readonly string _configFileFullPath = Path.Combine(Paths.ConfigPath, ConfigFileName);

    private FileSystemWatcher? _configWatcher;
    private DateTime _lastConfigWrite;

    private readonly Harmony _harmony = new(ModGUID);

    private void Awake()
    {
        PluginConfig.Build(Config, ServerConfigSync);

        var assembly = Assembly.GetExecutingAssembly();
        _harmony.PatchAll(assembly);
        SetupWatcher();
    }

    private void OnDestroy()
    {
        _harmony.UnpatchSelf();
        _configWatcher?.Dispose();
        Config.Save();
    }

    private void SetupWatcher()
    {
        var watcher = new FileSystemWatcher(Paths.ConfigPath, ConfigFileName);
        watcher.Changed += ReadConfigValues;
        watcher.Created += ReadConfigValues;
        watcher.Renamed += ReadConfigValues;
        watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        _configWatcher = watcher;
        watcher.EnableRaisingEvents = true;
    }

    private void ReadConfigValues(object sender, FileSystemEventArgs e)
    {
        if (!File.Exists(_configFileFullPath)) return;
        // The watcher usually raises several events per save; reload once per actual write.
        DateTime lastWrite = File.GetLastWriteTimeUtc(_configFileFullPath);
        if (lastWrite == _lastConfigWrite) return;
        _lastConfigWrite = lastWrite;
        try
        {
            ModLogger.LogDebug(DebugMessages.ReadConfigCalled);
            Config.Reload();
        }
        catch
        {
            ModLogger.LogError(DebugMessages.ErrorLoadingConfig);
            ModLogger.LogError(DebugMessages.RequestCheckConfig);
        }
    }
}
