
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
        [SerializeField] private Selectable selectable;
        private int iD = -1;

        public Selectable Selectable => selectable;
        public int ID => iD;
        public Action<int> OnSelected;
        public Action<int> OnDeselected;

        public void Initialize(string name, int players, int capacity, int entryID)
        {
            lobbyNameText.text = name;
            playersText.text = players + "/" + capacity;
            iD = entryID;
        }

        override public void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            OnSelected?.Invoke(iD);
        }

        override public void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            OnDeselected?.Invoke(iD);
        }

    }
}
