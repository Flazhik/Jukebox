using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Jukebox.UI.Elements
{
    public class CounterButton : Button
    {
        public event Action OnDown; 
        public event Action OnUp;
        public event Action OnExit;
        public bool pressed;
        
        public override void OnPointerDown(PointerEventData eventData)
        {
            pressed = true;
            base.OnPointerDown(eventData);
            OnDown?.Invoke();
        }
        
        public override void OnPointerUp(PointerEventData eventData)
        {
            pressed = false;
            base.OnPointerUp(eventData);
            OnUp?.Invoke();
        }
        
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            if (pressed)
                OnExit?.Invoke();

            pressed = false;
        }
    }
}