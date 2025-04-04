using System;
using System.Collections.Generic;
using Jukebox.Components;
using Jukebox.UI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Jukebox.UI.Themes
{
    public class ThemeSettingsMenu: MonoBehaviour
    {
        [SerializeField]
        private Counter threshold;

        [SerializeField]
        private GameObject enemyTemplate;
        
        [SerializeField]
        private List<AssetReferenceT<SpawnableObject>> enemies;

        private void Awake()
        {
            var themesManager = JukeboxThemesManager.Instance;
            threshold.SetDefaultValue(PrefsManager.Instance.GetIntLocal("jukebox.calmThemeEnemiesThreshold"));
            threshold.OnChanged += OnThresholdChanged;
            
            foreach (var enemyReference in enemies)
            {
                var handle = enemyReference.LoadAssetAsync();
                var enemy = handle.WaitForCompletion();
                Addressables.Release(handle);
                
                var entry = Instantiate(enemyTemplate, enemyTemplate.transform.parent);
                var icon = enemy.gridIcon;
                entry.transform.Find("Icon").GetComponentInChildren<Image>().sprite = icon;
                entry.transform.Find("Title").GetComponentInChildren<TMP_Text>().text = enemy.objectName;

                var counter = entry.GetComponentInChildren<Counter>();
                var toggle = entry.GetComponentInChildren<Toggle>();
                
                var thresholdForEnemy = themesManager.GetThresholdFor(enemy.enemyType);
                if (!thresholdForEnemy.HasValue)
                {
                    toggle.isOn = false;
                    counter.SetDefaultValue(0);
                }
                else
                    counter.SetDefaultValue(thresholdForEnemy.Value);

                counter.OnChanged += value =>
                {
                    toggle.isOn = true;
                    themesManager.SetThresholdFor(enemy.enemyType, value);
                };
                
                toggle.onValueChanged.AddListener(on =>
                {
                    if (on)
                        themesManager.SetThresholdFor(enemy.enemyType, counter.Value);
                    else
                        themesManager.DisableThresholdFor(enemy.enemyType);
                });

                entry.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            threshold.OnChanged -= OnThresholdChanged;
        }

        private static void OnThresholdChanged(int value) =>
            PrefsManager.Instance.SetIntLocal("jukebox.calmThemeEnemiesThreshold", value);
    }
}