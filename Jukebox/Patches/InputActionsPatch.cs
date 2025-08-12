using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UnityEngine;
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

            var actionsJson = @"{
                ""maps"": [
                    {
                        ""name"": ""Jukebox"",
                        ""id"": ""9dda5277-7734-4ec3-ad76-c8c0e5763d02"",
                        ""actions"": [
                            {
                                ""name"": ""Next Track"",
                                ""type"": ""Button"",
                                ""id"": ""09c701ba-74bc-4818-80a7-d1049e88ed97"",
                                ""expectedControlType"": ""Button"",
                                ""processors"": """",
                                ""interactions"": """",
                                ""initialStateCheck"": false
                            },
                            {
                                ""name"": ""Jukebox Menu"",
                                ""type"": ""Button"",
                                ""id"": ""2031a43b-3913-4384-a334-02669e72c30d"",
                                ""expectedControlType"": ""Button"",
                                ""processors"": """",
                                ""interactions"": """",
                                ""initialStateCheck"": false
                            },
                            {
                                ""name"": ""Disable Player"",
                                ""type"": ""Button"",
                                ""id"": ""3675ceba-90d0-46f5-b634-d6c13d208ff6"",
                                ""expectedControlType"": ""Button"",
                                ""processors"": """",
                                ""interactions"": """",
                                ""initialStateCheck"": false
                            },
                            {
                                ""name"": ""Playback Menu"",
                                ""type"": ""Button"",
                                ""id"": ""fa8430f8-91e7-454b-91f7-8b0147642ef9"",
                                ""expectedControlType"": ""Button"",
                                ""processors"": """",
                                ""interactions"": """",
                                ""initialStateCheck"": false
                            }
                        ],
                        ""bindings"": [
                            {
                                ""name"": """",
                                ""id"": ""8ec3310e-3f25-46a8-9a97-347d624fb616"",
                                ""path"": ""<Keyboard>/f3"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": ""Keyboard & Mouse"",
                                ""action"": ""Next Track"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": false
                            },
                            {
                                ""name"": """",
                                ""id"": ""b6df442f-c08f-4187-940a-9b684fa729c5"",
                                ""path"": ""<Keyboard>/f4"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": ""Keyboard & Mouse"",
                                ""action"": ""Jukebox Menu"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": false
                            },
                            {
                                ""name"": """",
                                ""id"": ""48676d48-feb2-406e-bc66-91f1aa7b11f1"",
                                ""path"": ""<Keyboard>/backquote"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": ""Keyboard & Mouse"",
                                ""action"": ""Playback Menu"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": false
                            },
                            {
                                ""name"": """",
                                ""id"": ""b86c9dc7-c6a1-4490-8aaa-bcb7dc1aa3cd"",
                                ""path"": ""<Keyboard>/f10"",
                                ""interactions"": """",
                                ""processors"": """",
                                ""groups"": ""Keyboard & Mouse"",
                                ""action"": ""Disable Player"",
                                ""isComposite"": false,
                                ""isPartOfComposite"": false
                            }
                        ]
                    }
                ]
            }";
            ukInputActions.asset.AddActionMap(InputActionMap.FromJson(actionsJson)[0]);
        }
    }
}