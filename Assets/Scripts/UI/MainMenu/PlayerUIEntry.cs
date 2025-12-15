
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PlayerUIEntry : Selectable
    {
        [SerializeField] private TMP_Text playerNameText;
        private int iD = -1;
        public int ID => iD;

        public void Initialize(string name, int entryID)
        {
            playerNameText.text = name;
            iD = entryID;
            //switch (iD)
            //{
            //    case 0:
            //        targetGraphic.color = Color.green;
            //        break;
            //    case 1:
            //        targetGraphic.color = Color.blue;
            //        break;
            //    case 2:
            //        targetGraphic.color = Color.red;
            //        break;
            //    case 3:
            //        targetGraphic.color = Color.yellow;
            //        break;

            //}
        }

    }
}

