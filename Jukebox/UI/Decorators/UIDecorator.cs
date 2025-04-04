using UnityEngine;

namespace Jukebox.UI.Decorators
{
    public abstract class UIDecorator<T> : MonoBehaviour where T: MonoBehaviour
    {
        [SerializeField]
        public GameObject hint;

        protected GameObject control;

        protected abstract GameObject Template();
        
        protected abstract void Init();
        
        public void Start()
        {
            control = Instantiate(Template(), transform);
            control.transform.position = transform.position;
            Destroy(hint);
            Init();
            Destroy(GetComponent<T>());
        }
    }
}