using BepInEx;
using HarmonyLib;
using Jotunn;
using Jotunn.Entities;
using Jotunn.Managers;

namespace RandomBullshit;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInDependency(Main.ModGuid)]
public sealed class RandomBullshitPlugin : BaseUnityPlugin
{
    public const string PluginGuid = "com.randombullshit.mod";
    public const string PluginName = "RandomBullshit";
    public const string PluginVersion = "1.0.6";

    internal static RandomBullshitPlugin Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        new Harmony(PluginGuid).PatchAll(typeof(TrainingDummyCharacterDpsPatch).Assembly);
        var localization = LocalizationManager.Instance.GetLocalization();
        localization.AddTranslation("$piece_TWMTrainingDummy", "T.W.M");

        PrefabManager.OnVanillaPrefabsAvailable += RegisterTrainingWithMexicanito;
        Logger.LogInfo($"{PluginName} loaded. Type Mexicaniando!");
    }

    private void RegisterTrainingWithMexicanito()
    {
        PrefabManager.OnVanillaPrefabsAvailable -= RegisterTrainingWithMexicanito;
        TrainingWithMexicanitoPiece.Register();
    }

    internal static void LogInfo(string message)
    {
        Instance.Logger.LogInfo(message);
    }

    internal static void LogError(string message)
    {
        Instance.Logger.LogError(message);
    }
}
