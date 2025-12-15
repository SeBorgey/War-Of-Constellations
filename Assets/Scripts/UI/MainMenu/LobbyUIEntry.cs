
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class LobbyUIEntry : Selectable
    {
        [SerializeField] private TMP_Text lobbyNameText;
        [SerializeField] private TMP_Text playersText;
        private int iD = -1;

        public int ID => iD;
        public Action<int> OnSelected;

        public void Initialize(string name, int players, int capacity, int entryID)
        {
            lobbyNameText.text = name;
            playersText.text = players + "/" + capacity;
            iD = entryID;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            OnSelected?.Invoke(iD);
            targetGraphic.color = Color.yellow;
        }

        public void Desellect()
        {
            targetGraphic.color = Color.white;
        }

    }
}
