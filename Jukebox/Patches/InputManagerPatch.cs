using System;
using System.IO;
using System.Linq;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine.InputSystem;

namespace Jukebox.Patches;

[HarmonyPatch(typeof(InputManager))]
public static class InputManagerPatch
{
    private const string ActionMapName = "Jukebox";
    
    private static FileInfo JukeboxBindingsFile => new(Path.Combine(PrefsManager.PrefsPath, "JukeboxBinds.json"));
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(InputManager), "Awake")]
    public static void InputManager_Awake_Postfix(InputManager __instance)
    {
        __instance.InputSource.Disable();
        if (JukeboxBindingsFile.Exists)
            JsonConvert.DeserializeObject<JsonBindingMap>(File.ReadAllText(JukeboxBindingsFile.FullName))
                .ApplyTo(__instance.InputSource.Actions.asset);
        __instance.UpgradeBindings();
        __instance.InputSource.Enable();
    }    
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(InputManager), "OnDisable")]
    public static void InputManager_OnDisable_Postfix(InputManager __instance)
    {
        SaveBindings(__instance, __instance.InputSource.Actions.asset);
    }
    
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

                if (action.bindings.Any(binding =>
                        InputSystem.FindControl(binding.path) == InputSystem.FindControl(path)))
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
            }, () => onCancel?.Invoke(),
            scheme.deviceRequirements.Select(static requirement => requirement.controlPath).ToList());

        return false;
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(InputManager), "SaveBindings")]
    public static void InputManager_SaveBindings_Postfix(InputManager __instance, InputActionAsset asset)
    {
        SaveBindings(__instance, asset);
    }

    private static void SaveBindings(InputManager instance, InputActionAsset asset) =>
        File.WriteAllText(JukeboxBindingsFile.FullName, JsonConvert.SerializeObject(
            BindingMapJson(asset, instance), Formatting.Indented));
    
    private static JsonBindingMap BindingMapJson(InputActionAsset asset, InputManager instance)
    {
        var scheme = instance.InputSource.Actions.KeyboardMouseScheme;
        var baseAsset = instance.defaultActions;
        
        var jsonBindingMap = new JsonBindingMap
        {
            controlScheme = scheme.bindingGroup
        };
        
        foreach (var a in from a in asset
                 let action = baseAsset.FindAction(a.id)
                 where action.actionMap.name == ActionMapName
                       && !a.IsActionEqual(action, scheme.bindingGroup)
                 select a)
            jsonBindingMap.AddAction(a);
        
        return jsonBindingMap;
    }
}