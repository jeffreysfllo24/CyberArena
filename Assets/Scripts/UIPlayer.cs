using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MirrorBasics {

    public class UIPlayer : MonoBehaviour {

        [SerializeField] Text text;
        PlayerManager player;

        public void SetPlayer (PlayerManager player) {
            if (player == null) {
                Debug.Log("Jeffrey SetPlayer: player is null");
            } else {
                Debug.Log("Jeffrey SetPlayer: player is not null");
            }
            this.player = player;
            text.text = "Player " + player.playerIndex.ToString ();
        }

    }
}