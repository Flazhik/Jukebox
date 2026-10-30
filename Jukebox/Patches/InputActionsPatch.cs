using HarmonyLib;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using static JukeboxCore.Logging.JukeboxLogger;

namespace Jukebox.Patches;

[HarmonyPatch(typeof(InputActions))]
public static class InputActionsPatch
{
    private const string ActionMapName = "Jukebox";

    private static InputActionAsset _controls;

    public static void Load() =>
        _controls = Addressables.LoadAssetAsync<InputActionAsset>("Assets/Jukebox/JukeboxControls.inputactions")
            .WaitForCompletion();

    [HarmonyPostfix]
    [HarmonyPatch(MethodType.Constructor)]
    public static void InputActions_Constructor_Postfix(InputActions __instance)
    {
        if (__instance.asset.FindActionMap(ActionMapName) != null)
            return;

        MergeInputActionAssets(__instance);
    }    

    private static void MergeInputActionAssets(InputActions ukInputActions)
    {
        var jukeboxActionAssets = Resources.FindObjectsOfTypeAll(typeof(InputActionAsset));
        if (jukeboxActionAssets.Length == 0)
        {
            Log.LogError("Couldn't load InputActionAsset");
            return;
        }

        if (ukInputActions.asset.FindActionMap(ActionMapName) != null)
            return;

        ukInputActions.Disable();
        ukInputActions.asset.AddActionMap(InputActionMap.FromJson(_controls.ToJson())[0]);
        ukInputActions.Enable();
    }
}