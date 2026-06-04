using System;
using UnityEngine;
using UnityEngine.UI;

namespace CoLab.UI
{
    public class NewSessionWidget : Widget
    {
        [SerializeField] private Widget playerIDWidget;
        [SerializeField] private Button newSessionButton;

        private new void Awake()
        {
            base.Awake();

            newSessionButton = GetComponentInChildren<Button>();
            playerIDWidget = FindAnyObjectByType<PlayerIDWidget>();
        }

        private void OnEnable()
        {
            newSessionButton.onClick.AddListener(NewSession);
        }

        private void OnDisable()
        {
            newSessionButton.onClick.RemoveListener(NewSession);
        }

        public void NewSession()
        {
            playerIDWidget.Show();
            Hide();
        }
    }
}