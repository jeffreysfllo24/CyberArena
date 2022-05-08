using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

//this script demonstrates the usage of a SyncVar to synchronize a variable across the network
namespace MirrorBasics {
    public class IncrementClick : NetworkBehaviour
    {
        //SyncVar declaration requires the [SyncVar] attribute immediately preceding it!
        [SyncVar]
        public int NumberOfClicks = 0;

        //IncrementClicks() is called by the PointerDown event in the Event Trigger component attached to this gameobject
        public void IncrementClicks()
        {
            //locate the PlayerManager within this client and request the server to run CmdIncrementClick(), passing in this gameobject
            var networkIdentity = new NobleConnect.Mirror.NobleClient();
            PlayerManager pm = networkIdentity.connection.identity.GetComponent<PlayerManager>();
            pm.CmdIncrementClick(gameObject);
        }
    }
}