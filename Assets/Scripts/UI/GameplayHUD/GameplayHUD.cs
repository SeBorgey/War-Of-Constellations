using UnityEngine;
using TMPro;

namespace UI
{
    public class GameplayHUD : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _goldText;

        private void Start()
        {
            UpdateGold(0);
        }

        public void UpdateGold(int newAmount)
        {
            if (_goldText != null)
            {
                _goldText.text = $"Gold: {newAmount}";
            }
        }
    }
}
