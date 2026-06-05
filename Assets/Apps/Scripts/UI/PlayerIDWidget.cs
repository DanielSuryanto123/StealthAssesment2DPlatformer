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
            string player1ID = player1IDInputField.text;
            string player2ID = player2IDInputField.text;

            string errorMessagePlayer1 = StringValidator.ValidateID(player1ID);
            string errorMessagePlayer2 = StringValidator.ValidateID(player2ID);

            if (errorMessagePlayer1 != "" || errorMessagePlayer2 != "")
            {
                return;
            }
            
            AnalyticsManager.Instance.StartSession(player1ID, player2ID);
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
