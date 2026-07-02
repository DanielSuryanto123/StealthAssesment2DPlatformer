using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoLab.UI
{
    public class KeyPressHintWidget : MonoBehaviour, IWidget
    {
        [SerializeField] private List<KeyPressHintSO> keyPressHints;
        
		private Canvas _canvas;
        private Image _keyImage;
        private TMP_Text _keyText;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        public void ShowKey(string key)
        {
            KeyPressHintSO keyPress = KeyPressHintSO.GetKeyFromList(key, keyPressHints);
            _keyImage.sprite = keyPress.keySprite;
            _keyText.text = keyPress.keyLabel;
        }

        public void Show()
        {
            _canvas.enabled = true;
        }

        public void Hide()
        {
            _canvas.enabled = false;
        }
    }
}
