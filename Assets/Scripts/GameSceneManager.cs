using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MirrorBasics {

    public class GameSceneManager : NetworkBehaviour
    {

        void Start () {
            Debug.Log("Game Scene Manager Awake!");
            gameManagerInitiateGame();
        }

        [Server]
        void gameManagerInitiateGame() {
            GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
            if (gm != null) {
                Debug.Log("Game manager in initiate game is not null");
                gm.awakeGameObjects();
            } else {
                Debug.Log("Game manager in initiate game is null");
            }
        }
       
    }
}
