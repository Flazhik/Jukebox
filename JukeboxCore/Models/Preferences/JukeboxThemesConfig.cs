using System.Collections.Generic;
using Newtonsoft.Json;

namespace JukeboxCore.Models.Preferences
{
    [JsonObject(MemberSerialization.OptIn)]
    public class JukeboxThemesConfig
    {
        [JsonProperty("calmThemeConfig")]
        public CalmThemeConfig calmTheme = new();

        public class CalmThemeConfig
        {
            [JsonProperty("specialEnemies", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public Dictionary<EnemyType, int> specialEnemies =
                new()
                {
                    { EnemyType.Sisyphus, 1 },
                    { EnemyType.Mindflayer, 1 },
                    { EnemyType.Ferryman, 1 },
                    { EnemyType.HideousMass, 1 },
                    { EnemyType.Swordsmachine, 2 }
                };
            
            [JsonProperty("specialRadiantEnemies", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public Dictionary<EnemyType, int> specialRadiantEnemies =
                new()
                {
                    { EnemyType.Sisyphus, 0 },
                    { EnemyType.Mindflayer, 0 },
                    { EnemyType.Ferryman, 0 },
                    { EnemyType.HideousMass, 0 },
                    { EnemyType.Swordsmachine, 1 }
                };
        }
        
        public JukeboxThemesConfig Equalize()
        {
            foreach (var enemy in calmTheme.specialEnemies)
            {
                if (!calmTheme.specialRadiantEnemies.ContainsKey(enemy.Key))
                    calmTheme.specialRadiantEnemies.Add(enemy.Key, 0);

                if (calmTheme.specialRadiantEnemies[enemy.Key] > enemy.Value)
                    calmTheme.specialRadiantEnemies[enemy.Key] = enemy.Value;
            }

            return this;
        }
    }
}