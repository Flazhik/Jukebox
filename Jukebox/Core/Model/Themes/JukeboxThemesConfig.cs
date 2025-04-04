using System.Collections.Generic;
using Newtonsoft.Json;

namespace Jukebox.Core.Model.Themes
{
    [JsonObject(MemberSerialization.OptIn)]
    public class JukeboxThemesConfig
    {
        [JsonProperty("calmThemeConfig")] public CalmThemeConfig CalmTheme = new();

        public class CalmThemeConfig
        {
            [JsonProperty("specialEnemies", ObjectCreationHandling = ObjectCreationHandling.Replace)]

            public Dictionary<EnemyType, int> SpecialEnemies =
                new()
                {
                    { EnemyType.Sisyphus, 1 },
                    { EnemyType.Mindflayer, 1 },
                    { EnemyType.Ferryman, 1 },
                    { EnemyType.HideousMass, 1 },
                    { EnemyType.Swordsmachine, 2 }
                };
        }
    }
}