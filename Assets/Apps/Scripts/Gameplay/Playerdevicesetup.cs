using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Forces this player's PlayerInput to a specific Control Scheme + device at startup.
/// This solves the classic local-co-op problem where two PlayerInput components
/// both try to auto-claim the same physical keyboard, leaving one player with
/// no input at all.
///
/// Setup:
/// - Attach alongside PlayerInput on each player GameObject.
/// - Set "Control Scheme Name" to match exactly what you named it in the
///   Input Actions editor (e.g. "P1Keyboard", "P2Keyboard", or "Gamepad").
/// - Leave "Gamepad Index" at 0 unless you have more than one controller
///   and need to assign a specific one.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class PlayerDeviceSetup : MonoBehaviour
{
    [Tooltip("Must match a Control Scheme name exactly as defined in your .inputactions asset.")]
    [SerializeField] private string controlSchemeName = "P1Keyboard";

    [Tooltip("Only used when the control scheme requires a Gamepad. Index into Gamepad.all if you support more than one controller.")]
    [SerializeField] private int gamepadIndex = 0;

    private PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        ApplyControlScheme();
    }

    private void ApplyControlScheme()
    {
        if (controlSchemeName.ToLower().Contains("gamepad"))
        {
            if (Gamepad.all.Count > gamepadIndex)
            {
                playerInput.SwitchCurrentControlScheme(controlSchemeName, Gamepad.all[gamepadIndex]);
            }
            else
            {
                Debug.LogWarning($"[PlayerDeviceSetup] No gamepad found at index {gamepadIndex} for {gameObject.name}. " +
                                  "Falling back to whatever PlayerInput auto-assigned.");
            }
        }
        else
        {
            // Both keyboard schemes (P1Keyboard / P2Keyboard) share the same physical
            // Keyboard device — the split happens in the binding groups you set up
            // in the Input Actions editor, not in which device object is paired.
            if (Keyboard.current != null)
            {
                playerInput.SwitchCurrentControlScheme(controlSchemeName, Keyboard.current);
            }
            else
            {
                Debug.LogWarning($"[PlayerDeviceSetup] No keyboard detected for {gameObject.name}.");
            }
        }
    }
}