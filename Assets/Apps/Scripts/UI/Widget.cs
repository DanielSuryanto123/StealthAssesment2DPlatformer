using UnityEngine;

namespace CoLab.UI
{
    public abstract class Widget : MonoBehaviour, IWidget
    {
        [SerializeField] protected bool _hideOnStart;
        
        protected Canvas _canvas;

        protected void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        protected void Start()
        {
            if (_hideOnStart) Hide();
            else Show();
        }

        public void Show()
        {
            if (_canvas is null)
            {
                Debug.LogError("Canvas not found");
                return;
            }
            _canvas.enabled = true;
        }

        public void Hide()
        {
            if (_canvas is null)
            {
                Debug.LogError("Canvas not found");
                return;
            }
            _canvas.enabled = false;
        }
    }
}