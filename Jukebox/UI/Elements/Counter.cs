using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;

namespace Jukebox.UI.Elements
{
    public class Counter : MonoBehaviour
    {
        public int Value
        {
            get => value;
            set
            {
                this.value = value;
                textValue.text = Value.ToString();
                OnChanged?.Invoke(value);
            }
        }

        public event Action<int> OnChanged;

        [SerializeField]
        private TextMeshProUGUI textValue;

        [SerializeField]
        private CounterButton increase;

        [SerializeField]
        private CounterButton decrease;

        [SerializeField]
        private int minValue;
        
        [SerializeField]
        private int maxValue;
        
        private int tmpValue;
        private IEnumerator changeValueRoutine;
        private int value;

        public void Awake()
        {
            AddEvents(increase, v => tmpValue = ChangeValue(i => i + 1));
            AddEvents(decrease, v => tmpValue = ChangeValue(i => i - 1));
        }

        public void SetDefaultValue(int defaultValue)
        {
            Value = defaultValue;
            tmpValue = defaultValue;
            textValue.text = defaultValue.ToString();
        }

        private void AddEvents(CounterButton button, Action<int> callback)
        {
            button.OnDown += () =>
            {
                changeValueRoutine = ChangeValueRoutine(callback);
                StartCoroutine(changeValueRoutine);
            };
            button.OnUp += SaveValue;
            button.OnExit += SaveValue;
        }

        private void SaveValue()
        {
            if (changeValueRoutine != null)
                StopCoroutine(changeValueRoutine);
            
            Value = tmpValue;
            OnChanged?.Invoke(Value);
        }

        [SuppressMessage("ReSharper", "IteratorNeverReturns")]
        private IEnumerator ChangeValueRoutine(Action<int> callback)
        {
            callback(tmpValue);
            yield return new WaitForSecondsRealtime(0.3f);

            while (true)
            {
                callback(tmpValue);
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        private int ChangeValue(Func<int, int> operation)
        {
            var result = ValidateOperation(operation);
            textValue.text = result.ToString();
            return result;
        }

        private int ValidateOperation(Func<int, int> operation)
        {
            var result = operation(tmpValue);
            return result < minValue || result > maxValue ? tmpValue : result;
        }
    }
}