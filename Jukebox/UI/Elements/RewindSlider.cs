using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Jukebox.UI.Elements
{
    public class RewindSlider : MonoBehaviour, IEndDragHandler, IDragHandler
    {
        public event Action<float> OnValueChanged;
        public bool beingDragged;
        
        [SerializeField]
        public Slider slider;
        
        public void OnEndDrag(PointerEventData eventData)
        {
            OnValueChanged?.Invoke(slider.value);
            beingDragged = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            beingDragged = true;
        }
    }
}