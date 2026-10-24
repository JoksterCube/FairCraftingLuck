using System.Collections.Generic;
using static JoksterCube.FairCraftingLuck.Settings.Constants.Plugin;

namespace JoksterCube.FairCraftingLuck.Common.Networking;

// The mod is optional on both sides. A player without it is never kicked, and a modded player can join
// an unmodded server. When both sides have it, the versions must match exactly.
internal static class RpcHandlers
{
    internal static readonly HashSet<ZRpc> ValidatedPeers = [];
    private static bool _serverVersionReceived;

    // True on a server, and on a client once a server with this mod has answered the version check.
    internal static bool ServerHasMod => ZNet.instance && (ZNet.instance.IsServer() || _serverVersionReceived);

    internal static void BeginServerVersionCheck()
    {
        _serverVersionReceived = false;
        Plugin.ConnectionError = string.Empty;
    }

    internal static void ResetServerVersionCheck() => _serverVersionReceived = false;

    internal static void RPC_VersionCheck(ZRpc rpc, ZPackage pkg)
    {
        string version = pkg.ReadString();
        var instance = ZNet.instance;
        if (!instance) return;

        var isServer = instance.IsServer();
        if (!isServer)
            _serverVersionReceived = true;

        Plugin.ModLogger.LogInfo($"Version check, local: {ModVersion}, remote: {version}");
        if (version != ModVersion)
        {
            Plugin.ConnectionError = $"{ModName} Installed: {ModVersion}\n Needed: {version}";
            if (!isServer) return;
            Plugin.ModLogger.LogWarning($"Peer ({rpc.GetPeerHostName()}) has incompatible version, disconnecting...");
            rpc.Invoke("Error", 3);
        }
        else if (!isServer)
        {
            Plugin.ModLogger.LogInfo($"Server uses {ModName} {ModVersion}; its crafting luck settings apply.");
        }
        else
        {
            Plugin.ModLogger.LogInfo($"Adding peer ({rpc.GetPeerHostName()}) to validated list");
            ValidatedPeers.Add(rpc);
        }
    }
}
