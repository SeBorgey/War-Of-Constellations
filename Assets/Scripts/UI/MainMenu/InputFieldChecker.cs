using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InputFieldChecker : MonoBehaviour
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private List<Button> buttons;

        private void OnGUI()
        {
            bool haveText = inputField.text != "";

            foreach (var button in buttons)
            {
                button.interactable = haveText;
            }
        }
    }
}