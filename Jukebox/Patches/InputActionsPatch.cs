using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Jukebox.Patches
{
    [HarmonyPatch(typeof(InputActions))]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class InputActionsPatch
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

            var inputActionMap = jukeboxActionAssets
                .SelectMany(asset => ((InputActionAsset)asset).actionMaps)
                .FirstOrDefault(map => map.name.Equals("Jukebox"));
            
            if (inputActionMap == default)
            {
                Debug.LogError("Couldn't find an InputActionAsset for Jukebox");
                return;
            }

            var mineJson = inputActionMap.ToJson();
            ukInputActions.asset.AddActionMap(InputActionMap.FromJson(mineJson)[0]);
        }
    }
}