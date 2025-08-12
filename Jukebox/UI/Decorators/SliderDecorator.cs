using JukeboxCore.Assets;
using SettingsMenu.Components;
using UnityEngine;
using UnityEngine.UI;
using static Jukebox.Utils.ReflectionUtils;

namespace Jukebox.UI.Decorators
{
    public class SliderDecorator: UIDecorator<SliderDecorator>
    {
        [AddressableAsset("Assets/Prefabs/UI/Settings/Elements/Slider Button.prefab", typeof(GameObject))]
        private static GameObject sliderTemplate;

        [SerializeField]
        public float minValue;
        
        [SerializeField]
        public float maxValue;
        
        [SerializeField]
        public SliderValueToTextConfig sliderConfig;

        protected override GameObject Template() => sliderTemplate;

        protected override void Init()
        {
            var slider = GetComponentInChildren<Slider>();
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            
            var sliderValueToText = GetPrivate<SliderValueToText>(GetComponentInChildren<SettingsSlider>(),
                typeof(SettingsSlider), "sliderValueToText");
            sliderValueToText.ConfigureFrom(sliderConfig);
        }
    }
}