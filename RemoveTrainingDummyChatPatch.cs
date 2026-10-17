using System;
using HarmonyLib;
using UnityEngine;

namespace RandomBullshit;

[HarmonyPatch(typeof(Chat), "InputText")]
internal static class RemoveTrainingDummyChatPatch
{
    private const string Command = "/removedummy";
    private const float RemovalRange = 8f;

    private static bool Prefix(Chat __instance)
    {
        var input = __instance.m_input;
        if (input == null)
        {
            return true;
        }

        var text = input.text;
        if (string.IsNullOrEmpty(text)
            || !text.StartsWith(Command, StringComparison.OrdinalIgnoreCase)
            || (text.Length > Command.Length && !char.IsWhiteSpace(text[Command.Length])))
        {
            return true;
        }

        input.text = string.Empty;
        if (!string.IsNullOrWhiteSpace(text.Substring(Command.Length)))
        {
            __instance.AddString($"Usage: {Command}");
            return false;
        }

        RemoveNearestDummy(__instance);
        return false;
    }

    private static void RemoveNearestDummy(Terminal context)
    {
        if (Player.m_localPlayer == null)
        {
            context.AddString("Cannot remove a training dummy because no local player is available.");
            return;
        }

        if (ZNetScene.instance == null)
        {
            context.AddString("Cannot remove a training dummy because the network scene is unavailable.");
            return;
        }

        var playerPosition = Player.m_localPlayer.transform.position;
        Character nearestDummy = null;
        var nearestDistanceSquared = RemovalRange * RemovalRange;

        foreach (var character in UnityEngine.Object.FindObjectsByType<Character>(FindObjectsSortMode.None))
        {
            if (!TrainingDummyDpsDisplayPatch.IsTrainingDummy(character))
            {
                continue;
            }

            var distanceSquared = (character.transform.position - playerPosition).sqrMagnitude;
            if (distanceSquared > nearestDistanceSquared)
            {
                continue;
            }

            nearestDummy = character;
            nearestDistanceSquared = distanceSquared;
        }

        if (nearestDummy == null)
        {
            context.AddString($"No training dummy found within {RemovalRange:F0} meters.");
            return;
        }

        var networkView = nearestDummy.GetComponent<ZNetView>();
        if (networkView == null)
        {
            context.AddString("Cannot remove the training dummy because its network view is missing.");
            return;
        }

        if (!networkView.IsOwner())
        {
            context.AddString("Cannot remove the training dummy because your player does not own it.");
            return;
        }

        ZNetScene.instance.Destroy(nearestDummy.gameObject);
        context.AddString("Removed the nearest training dummy.");
    }
}
