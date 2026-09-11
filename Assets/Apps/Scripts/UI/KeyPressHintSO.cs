using System.Collections.Generic;
using UnityEngine;

namespace CoLab.UI
{
    [CreateAssetMenu(fileName = "New KeyPress Hint", menuName = "CoLab/UI/Key Press Hint")]
    public class KeyPressHintSO : ScriptableObject
    {
        public string keyID;
        public string keyLabel;
        public Sprite keySprite;

        public static KeyPressHintSO GetKeyFromList(string key, List<KeyPressHintSO> keyPressHints)
        {
            foreach (KeyPressHintSO keyPressHintSo in keyPressHints)
            {
                if (keyPressHintSo.keyID == key)
                {
                    return keyPressHintSo;
                }
            }

            return null;
        }
    }
}