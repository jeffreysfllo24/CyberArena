using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManager : NetworkBehaviour
{
    //This simple GameManager script is attached to a Server-only game object, demonstrating how to implement game logic tracked by the Server
    public int TurnsPlayed = 0;
    private int playersConnected = 0;

    public void UpdateTurnsPlayed()
    {
        TurnsPlayed++;
    }

    public void CmdUpdatePlayerConnected()
    {
        playersConnected += 1;

        // Check if two players connected
        if (playersConnected > 1) {
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            PlayerManager pm = networkIdentity.GetComponent<PlayerManager>();
            pm.RpcInitiateGame();
        }
    }
}
