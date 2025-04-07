using System;
using System.Linq;
using HarmonyLib;
using UnityEngine.InputSystem;

namespace Jukebox.Patches
{
    [HarmonyPatch(typeof(InputManager))]
    public static class InputManagerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(InputManager), "Rebind")]
        public static bool InputManager_Rebind_Prefix(
            InputManager __instance,
            InputAction action,
            int? existingIndex,
            Action onComplete,
            Action onCancel,
            InputControlScheme scheme)
        {
            if (action.actionMap.name != "Jukebox")
                return true;
            
            __instance.WaitForButton(path =>
            {
                if (path == "<Mouse>/leftButton")
                {
                    onComplete?.Invoke();
                    return;
                }

                if (action.bindings.Any(binding => InputSystem.FindControl(binding.path) == InputSystem.FindControl(path)))
                {
                    onComplete?.Invoke();
                    return;
                }
                (!existingIndex.HasValue
                    ? action.AddBinding()
                    : action.ChangeBinding(existingIndex.GetValueOrDefault()))
                    .WithPath(path)
                    .WithGroup(scheme.bindingGroup);
                var actionModified = __instance.actionModified;
                actionModified?.Invoke(action);
                if (onComplete == null)
                    return;
                onComplete();
            }, () =>
            {
                onCancel?.Invoke();
            }, scheme.deviceRequirements.Select(requirement => requirement.controlPath).ToList());

            return false;
        }
    }
}