using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MirrorBasics {

    public class GameManager : NetworkBehaviour
    {
        //This simple GameManager script is attached to a Server-only game object, demonstrating how to implement game logic tracked by the Server
        public int TurnsPlayed = 0;
        private int playersConnected = 0;

        void Start () {
            Debug.Log("Game Manager Awake!");
            // awakeGameObjects();
        }

        public void UpdateTurnsPlayed()
        {
            TurnsPlayed++;
        }

        public void ResetTurnsPlayed()
        {
            TurnsPlayed = 0;
        }

        public void CmdUpdatePlayerConnected()
        {
            playersConnected += 1;

            // Check if two players connected
            if (playersConnected > 1) {
                awakeGameObjects();
            }
        }

        [Server]
        public void awakeGameObjects() {
            var networkIdentity = new NobleConnect.Mirror.NobleClient();
            PlayerManager pm = networkIdentity.connection.identity.GetComponent<PlayerManager>();
            pm.RpcPopulateGameObjects();
            pm.RpcInitiateGame();
        }

        public void initiateGame() {
            var networkIdentity = new NobleConnect.Mirror.NobleClient();
            PlayerManager pm = networkIdentity.connection.identity.GetComponent<PlayerManager>();
            pm.RpcInitiateGame();
        }
    }
}
