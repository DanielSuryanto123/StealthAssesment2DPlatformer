using System;
using CoLab.Telemetry;
using UnityEngine;
using UnityEngine.UI;

namespace CoLab.UI
{
    public class PlayerIDWidget : Widget
    {
        [SerializeField] private InputField player1IDInputField;
        [SerializeField] private InputField player2IDInputField;
        private Button startGameButton;

        private new void Awake()
        {
            base.Awake();
            
            startGameButton = GetComponentInChildren(typeof(Button)) as Button;
        }

        private void StartGame()
        {
            
        }

        private void IsValidID()
        {
            throw new NotImplementedException();
        }

        private void OnEnable()
        {
            startGameButton.onClick.AddListener(StartGame);
        }

        private void OnDisable()
        {
            startGameButton.onClick.RemoveListener(StartGame);
        }
    }
}
