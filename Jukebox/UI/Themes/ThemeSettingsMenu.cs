using System.Collections.Generic;
using Jukebox.Components;
using Jukebox.UI.Elements;
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
        private GameObject regularEnemies;
        
        [SerializeField]
        private GameObject radiantEnemies;
        
        [SerializeField]
        private Button[] tabButtons;

        [SerializeField]
        private GameObject activeTab;
        
        [SerializeField]
        private List<AssetReferenceT<SpawnableObject>> enemies;

        private void Awake()
        {
            var themesManager = JukeboxThemesManager.Instance;
            threshold.SetDefaultValue(PrefsManager.Instance.GetIntLocal("jukebox.calmThemeEnemiesThreshold"));
            threshold.OnChanged += OnThresholdChanged;
            
            foreach (var button in tabButtons)
            {
                button.onClick.AddListener(() =>
                {
                    activeTab.transform.SetParent(button.transform);
                    activeTab.transform.SetSiblingIndex(0);
                    activeTab.transform.localPosition = Vector3.zero;
                });
            }
            
            foreach (var enemyReference in enemies)
            {
                var handle = enemyReference.LoadAssetAsync();
                var enemy = handle.WaitForCompletion();
                Addressables.Release(handle);

                var respectiveSettings = new Dictionary<bool, ThemeEnemySettings>();
                foreach (var tab in new [] { regularEnemies, radiantEnemies })
                {
                    var entry = Instantiate(enemyTemplate, tab.transform);
                    var radiant = tab == radiantEnemies;
                    
                    var icon = enemy.gridIcon;
                    var enemySettings = entry.GetComponent<ThemeEnemySettings>();
                    enemySettings.icon.sprite = icon;
                    enemySettings.title.text = radiant ? $"Radiant {enemy.objectName}" : enemy.objectName;
                    enemySettings.radianceBg.SetActive(radiant);

                    if (themesManager.GetThresholdFor(enemy.enemyType, radiant) is var enemyThreshold
                        && !enemyThreshold.HasValue)
                        enemySettings.counter.SetDefaultValue(0);
                    else
                        enemySettings.counter.SetDefaultValue(enemyThreshold.Value);

                    enemySettings.toggle.isOn = enemyThreshold.HasValue;
                    enemySettings.counter.OnChanged += value =>
                    {
                        enemySettings.toggle.isOn = true;
                        themesManager.SetThresholdFor(enemy.enemyType, value, radiant);
                    };
                    enemySettings.toggle.onValueChanged.AddListener(on =>
                    {
                        if (on)
                            themesManager.SetThresholdFor(enemy.enemyType, enemySettings.counter.Value, radiant);
                        else
                            themesManager.DisableThresholdFor(enemy.enemyType, radiant);
                    });
                    respectiveSettings[radiant] = enemySettings;
                    entry.SetActive(true);
                }

                respectiveSettings[false].counter.OnChanged += value =>
                {
                    if (themesManager.GetThresholdFor(enemy.enemyType, true) is var enemyThreshold
                        && !enemyThreshold.HasValue || enemyThreshold.Value > value)
                    {
                        themesManager.SetThresholdFor(enemy.enemyType, value, true);
                        respectiveSettings[true].toggle.isOn = true;
                        respectiveSettings[true].counter.Value = value;
                    }
                };
                
                respectiveSettings[false].toggle.onValueChanged.AddListener(on =>
                {
                    if (!on)
                        return;

                    if (themesManager.GetThresholdFor(enemy.enemyType, true) is not null)
                        return;
                    
                    respectiveSettings[true].toggle.isOn = true;
                    respectiveSettings[true].counter.Value = respectiveSettings[false].counter.Value;
                });
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