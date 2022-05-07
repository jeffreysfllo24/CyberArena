﻿using NobleConnect.Mirror;
using UnityEngine;
using Mirror;

namespace NobleConnect.Examples.Mirror
{
    // Example implementation of NobleNetworkManager
    // Look at ExampleNetworkHUD for more information on how to use it. 
    public class ExampleMirrorNobleNetworkManager : NobleNetworkManager
    {
        public override void OnClientConnect()
        {
            base.OnClientConnect();
            Debug.Log("Client connected.");
        }

        public override void OnClientDisconnect() 
        {
            base.OnClientDisconnect();
            Debug.Log("Client disconnected.");
        }

        // public override void OnServerConnect(NetworkConnectionToClient conn)
        // {
        //     base.OnServerConnect(conn);
        //     Debug.Log("Server received a client connection.");
        // }

        // public override void OnServerDisconnect(NetworkConnectionToClient conn)
        // {
        //     base.OnServerDisconnect(conn);
        //     Debug.Log("Server lost a client.");
        // }

        // OnServerPrepared is called when the host is listening and has received 
        // their HostEndPoint from the NobleConnect service.
        // Use this HostEndPoint on the client in order to connect to the host.
        // Typically you would use a matchmaking system to pass the HostEndPoint to the client.
        // Look at the Match Up Example for one way to do it. Match Up comes free with any paid plan. 
        public override void OnServerPrepared(string hostAddress, ushort hostPort)
        {
            // Get your HostEndPoint here. 
            Debug.Log("Hosting at: " + hostAddress + ":" + hostPort);
        }
    }
}
