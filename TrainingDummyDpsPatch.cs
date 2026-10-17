using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace RandomBullshit;

[HarmonyPatch(typeof(Character), "Start")]
internal static class TrainingDummyDpsDisplayPatch
{
    private static void Postfix(Character __instance)
    {
        if (!IsTrainingDummy(__instance))
        {
            return;
        }

        var piece = __instance.GetComponent<Piece>();
        if (piece != null)
        {
            // Player.RemovePiece checks this field before calling Piece.CanBeRemoved().
            piece.m_canBeRemoved = true;
        }

        if (__instance.GetComponent<TrainingDummyDpsDisplay>() == null)
        {
            __instance.gameObject.AddComponent<TrainingDummyDpsDisplay>();
        }

        if (__instance.GetComponent<TrainingDummyRegeneration>() == null)
        {
            __instance.gameObject.AddComponent<TrainingDummyRegeneration>();
        }
    }

    internal static bool IsTrainingDummy(Character character)
    {
        return IsTwigDummy(character)
            || IsTwmDummy(character);
    }

    internal static bool IsTwmDummy(Character character)
    {
        return character.GetComponent<TwmTrainingDummyMarker>() != null
            || character.gameObject.name.StartsWith(
                TrainingWithMexicanitoPiece.PrefabName,
                StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTwigDummy(Character character)
    {
        return character.gameObject.name.StartsWith("piece_TrainingDummy", StringComparison.OrdinalIgnoreCase);
    }
}

[HarmonyPatch(typeof(Character), "SetHealth", new Type[] { typeof(float) })]
internal static class TwmDummyPreventDeathPatch
{
    private static void Prefix(Character __instance, ref float __0)
    {
        if (TrainingDummyDpsDisplayPatch.IsTrainingDummy(__instance)
            && !TrainingDummyRemovalContext.IsRemovingPiece
            && __0 <= 0f)
        {
            __0 = 1f;
        }
    }
}

[HarmonyPatch(typeof(Piece), "CanBeRemoved")]
internal static class TrainingDummyCanBeRemovedPatch
{
    private static bool Prefix(Piece __instance, ref bool __result)
    {
        var character = __instance.GetComponent<Character>();
        if (character == null || !TrainingDummyDpsDisplayPatch.IsTrainingDummy(character))
        {
            return true;
        }

        __result = true;
        return false;
    }
}

[HarmonyPatch(typeof(Player), "RemovePiece")]
internal static class TrainingDummyPlayerRemovalContextPatch
{
    private static void Prefix(out bool __state)
    {
        __state = TrainingDummyRemovalContext.IsRemovingPiece;
        TrainingDummyRemovalContext.IsRemovingPiece = true;
    }

    private static void Postfix(bool __state)
    {
        TrainingDummyRemovalContext.IsRemovingPiece = __state;
    }
}

[HarmonyPatch(typeof(WearNTear), "RPC_Remove", new Type[] { typeof(long), typeof(bool) })]
internal static class TrainingDummyWearNTearRemovalContextPatch
{
    private static void Prefix(WearNTear __instance, out bool __state)
    {
        __state = TrainingDummyRemovalContext.IsRemovingPiece;
        var character = __instance.GetComponent<Character>();
        if (character != null && TrainingDummyDpsDisplayPatch.IsTrainingDummy(character))
        {
            TrainingDummyRemovalContext.IsRemovingPiece = true;
        }
    }

    private static void Postfix(bool __state)
    {
        TrainingDummyRemovalContext.IsRemovingPiece = __state;
    }
}

internal static class TrainingDummyRemovalContext
{
    internal static bool IsRemovingPiece { get; set; }
}

[HarmonyPatch(typeof(Destructible), "Destroy", new Type[] { typeof(HitData) })]
internal static class TrainingDummyPreventDamageDestructionPatch
{
    private static bool Prefix(Destructible __instance, HitData __0)
    {
        var character = __instance.GetComponent<Character>();
        return character == null
            || !TrainingDummyDpsDisplayPatch.IsTrainingDummy(character)
            || TrainingDummyRemovalContext.IsRemovingPiece;
    }
}

[HarmonyPatch(typeof(WearNTear), "Destroy", new Type[] { typeof(HitData), typeof(bool) })]
internal static class TrainingDummyPreventDamageWearNTearDestructionPatch
{
    private static bool Prefix(WearNTear __instance, HitData __0)
    {
        var character = __instance.GetComponent<Character>();
        return character == null
            || !TrainingDummyDpsDisplayPatch.IsTrainingDummy(character)
            || TrainingDummyRemovalContext.IsRemovingPiece;
    }
}

[HarmonyPatch(typeof(Piece), "DropResources", new Type[] { typeof(HitData) })]
internal static class TrainingDummyDropResourcesPatch
{
    private static bool Prefix(Piece __instance, HitData __0)
    {
        var character = __instance.GetComponent<Character>();
        return character == null
            || !TrainingDummyDpsDisplayPatch.IsTrainingDummy(character)
            || TrainingDummyRemovalContext.IsRemovingPiece;
    }
}

[HarmonyPatch(typeof(Character), "RPC_Damage")]
internal static class TrainingDummyCharacterDpsPatch
{
    private static bool Prefix(Character __instance, out float __state)
    {
        if (!TrainingDummyDpsDisplayPatch.IsTrainingDummy(__instance))
        {
            __state = float.NaN;
            return true;
        }

        __state = __instance.GetHealth();
        return true;
    }

    private static void Postfix(Character __instance, float __state)
    {
        if (float.IsNaN(__state))
        {
            return;
        }

        var damage = __state - __instance.GetHealth();
        if (damage <= 0f)
        {
            return;
        }

        var display = __instance.GetComponent<TrainingDummyDpsDisplay>();
        if (display == null)
        {
            display = __instance.gameObject.AddComponent<TrainingDummyDpsDisplay>();
        }

        display.RecordDamage(damage);

        var regeneration = __instance.GetComponent<TrainingDummyRegeneration>();
        if (regeneration != null)
        {
            regeneration.RecordDamage();
        }
    }
}

internal sealed class TrainingDummyRegeneration : MonoBehaviour
{
    private const float RegenerationDelaySeconds = 5f;

    private Character _character;
    private float _lastDamageAt = float.NegativeInfinity;

    private void Awake()
    {
        _character = GetComponent<Character>();
    }

    internal void RecordDamage()
    {
        _lastDamageAt = Time.time;
    }

    private void Update()
    {
        if (float.IsNegativeInfinity(_lastDamageAt)
            || Time.time - _lastDamageAt < RegenerationDelaySeconds)
        {
            return;
        }

        var wearNTear = GetComponent<WearNTear>();
        if (wearNTear != null)
        {
            wearNTear.Repair();
        }

        _character.SetHealth(_character.GetMaxHealth());
        _lastDamageAt = float.NegativeInfinity;
    }
}

internal sealed class TrainingDummyDpsDisplay : MonoBehaviour
{
    private const float DpsWindowSeconds = 5f;
    private const float IdleResetSeconds = 3f;
    private const float DisplayTimeoutSeconds = 5f;
    private const float RefreshIntervalSeconds = 0.1f;
    private const float MinimumAverageSeconds = 1f;

    private readonly Queue<DamageSample> _samples = new Queue<DamageSample>();
    private TextMesh _label;
    private float _totalDamage;
    private float _windowStartedAt;
    private float _lastDamageAt = float.NegativeInfinity;
    private float _lastActivityAt;
    private float _nextRefreshAt;

    private void Awake()
    {
        _lastActivityAt = Time.time;
        var font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        if (font == null)
        {
            RandomBullshitPlugin.LogError("Could not create the T.W.I.G. DPS label because Unity's built-in font could not be loaded.");
            return;
        }

        var labelObject = new GameObject("RandomBullshit.TwigDps");
        labelObject.transform.SetParent(transform, false);
        labelObject.transform.localPosition = Vector3.up * 3f;
        _label = labelObject.AddComponent<TextMesh>();
        _label.font = font;
        _label.fontSize = 64;
        _label.characterSize = 0.06f;
        _label.anchor = TextAnchor.MiddleCenter;
        _label.alignment = TextAlignment.Center;
        _label.color = new Color(1f, 0.92f, 0.68f);
        _label.text = "0";
        RandomBullshitPlugin.LogInfo("Attached DPS label to a training dummy.");
    }

    private void LateUpdate()
    {
        if (_label == null)
        {
            return;
        }

        var camera = Camera.main;
        if (camera != null)
        {
            _label.transform.rotation = camera.transform.rotation;
        }
    }

    private void Update()
    {
        var now = Time.time;
        if (now - _lastDamageAt >= IdleResetSeconds && !float.IsNegativeInfinity(_lastDamageAt))
        {
            _samples.Clear();
            _totalDamage = 0f;
            _lastDamageAt = float.NegativeInfinity;
            _windowStartedAt = now;
        }

        while (_samples.Count > 0 && now - _samples.Peek().Time > DpsWindowSeconds)
        {
            _totalDamage -= _samples.Dequeue().Amount;
        }

        if (_label != null && now >= _nextRefreshAt)
        {
            _label.text = $"{GetDps(now):F0}";
            _label.gameObject.SetActive(now - _lastActivityAt < DisplayTimeoutSeconds);
            _nextRefreshAt = now + RefreshIntervalSeconds;
        }
    }

    internal void RecordDamage(float amount)
    {
        var now = Time.time;
        if (now - _lastDamageAt >= IdleResetSeconds)
        {
            _samples.Clear();
            _totalDamage = 0f;
            _windowStartedAt = now;
        }

        _samples.Enqueue(new DamageSample(now, amount));
        _totalDamage += amount;
        _lastDamageAt = now;
        _lastActivityAt = now;
    }

    private float GetDps(float now)
    {
        if (_lastDamageAt == float.NegativeInfinity)
        {
            return 0f;
        }

        var elapsed = now - _windowStartedAt;
        var window = Mathf.Min(DpsWindowSeconds, Mathf.Max(MinimumAverageSeconds, elapsed));
        return _totalDamage / window;
    }

    private readonly struct DamageSample
    {
        internal DamageSample(float time, float amount)
        {
            Time = time;
            Amount = amount;
        }

        internal float Time { get; }

        internal float Amount { get; }
    }
}
