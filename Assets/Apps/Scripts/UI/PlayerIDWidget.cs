using System;
using CoLab.Telemetry;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CoLab.UI
{
    public class PlayerIDWidget : Widget
    {
        [SerializeField] private TMP_InputField player1IDInputField;
        [SerializeField] private TMP_InputField player2IDInputField;
        private Button startGameButton;

        // Raised only after the IDs passed validation and the analytics session was started.
        public event Action SessionStarted;

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
                Debug.Log(errorMessagePlayer1 + "\n" + errorMessagePlayer2);
                return;
            }
            
            AnalyticsManager.Instance.StartSession(player1ID, player2ID);
            SessionStarted?.Invoke();
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
