using UnityEngine;
using static MessageHud;

namespace JoksterCube.FairCraftingLuck.Common;

internal static class PlayerExtensions
{
    internal static void Message(MessageType messageType, string message, Sprite? icon = null)
    {
        if (Player.m_localPlayer) Player.m_localPlayer.Message(messageType, message, 0, icon);
    }

    internal static void FormatedMessage(MessageType messageType, string message, params object[] args) =>
        Message(messageType, string.Format(message, args));

    internal static void FormatedCenterMessage(string message, params object[] args) =>
        FormatedMessage(MessageType.Center, message, args);

    internal static void FormatedTopLeftMessage(string message, params object[] args) =>
        FormatedMessage(MessageType.TopLeft, message, args);
}
