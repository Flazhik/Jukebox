using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Jukebox.UI.Elements;

public class RewindSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] public Slider slider;
    [NonSerialized] public bool beingDragged;

    public void OnPointerDown(PointerEventData eventData)
    {
        beingDragged = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        beingDragged = false;
        OnValueChanged?.Invoke(slider.value);
    }

    public event Action<float> OnValueChanged;
}