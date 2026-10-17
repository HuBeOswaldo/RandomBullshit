using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace RandomBullshit;

internal static class TrainingWithMexicanitoPiece
{
    private const string VanillaPrefabName = "piece_TrainingDummy";
    internal const string PrefabName = "piece_TWMTrainingDummy";
    internal static GameObject Prefab { get; private set; }

    internal static void Register()
    {
        var prefab = PrefabManager.Instance.CreateClonedPrefab(PrefabName, VanillaPrefabName);
        if (prefab == null)
        {
            RandomBullshitPlugin.LogError($"Could not create TWM because the vanilla prefab '{VanillaPrefabName}' was not found.");
            return;
        }

        Prefab = prefab;
        prefab.AddComponent<TwmTrainingDummyMarker>();

        var piece = prefab.GetComponent<Piece>();
        if (piece == null)
        {
            RandomBullshitPlugin.LogError($"Could not register TWM because prefab '{VanillaPrefabName}' has no Piece component.");
            Object.Destroy(prefab);
            return;
        }

        piece.m_canBeRemoved = true;
        piece.m_name = "$piece_TWMTrainingDummy";
        piece.m_description = "A stationary training dummy for practicing attacks.";

        var character = prefab.GetComponent<Character>();
        if (character == null)
        {
            RandomBullshitPlugin.LogError("Could not set the TWM dummy's in-world name because its Character component was not found.");
            Object.Destroy(prefab);
            return;
        }

        character.m_name = "$piece_TWMTrainingDummy";
        if (prefab.GetComponent<TrainingDummyRegeneration>() == null)
        {
            prefab.AddComponent<TrainingDummyRegeneration>();
        }

        var aiComponents = prefab.GetComponentsInChildren<BaseAI>(true);
        foreach (var ai in aiComponents)
        {
            ai.enabled = false;
        }

        if (aiComponents.Length == 0)
        {
            RandomBullshitPlugin.LogInfo("TWM prefab has no AI components to disable.");
        }
        else
        {
            RandomBullshitPlugin.LogInfo($"Disabled {aiComponents.Length} AI component(s) on TWM.");
        }

        var customPiece = new CustomPiece(prefab, "Hammer", false);
        if (!PieceManager.Instance.AddPiece(customPiece))
        {
            RandomBullshitPlugin.LogError("Jötunn could not add TWM to the hammer build menu.");
            Object.Destroy(prefab);
            return;
        }

        RandomBullshitPlugin.LogInfo("Registered TWM (Training with Mexicanito) in the hammer build menu.");
    }
}

internal sealed class TwmTrainingDummyMarker : MonoBehaviour
{
}
