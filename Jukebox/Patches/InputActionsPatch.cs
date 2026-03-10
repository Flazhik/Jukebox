using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;

namespace Jukebox.Patches
{
    [HarmonyPatch(typeof(InputActions))]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public static class InputActionsPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(MethodType.Constructor)]
        public static void InputActions_Constructor_Postfix(InputActions __instance)
        {
            if (__instance.asset.FindActionMap("Jukebox") != null)
                return;
            
            MergeInputActionAssets(__instance);
        }

        private static void MergeInputActionAssets(InputActions ukInputActions)
        {
            var jukeboxActionAssets = UnityEngine.Resources.FindObjectsOfTypeAll(typeof(InputActionAsset));
            if (jukeboxActionAssets.Length == 0)
            {
                Debug.LogError("Couldn't load InputActionAsset");
                return;
            }

            var actionMap = Addressables.LoadAssetAsync<InputActionAsset>("Assets/Jukebox/JukeboxControls.inputactions")
                .WaitForCompletion();
                
            if (ukInputActions.asset.FindActionMap("Jukebox") != null)
                return;

            ukInputActions.Disable();
            ukInputActions.asset.AddActionMap(InputActionMap.FromJson(actionMap.ToJson())[0]);
            ukInputActions.Enable();
        }
    }
}