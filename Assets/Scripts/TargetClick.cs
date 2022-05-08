using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

//this script demonstrates the usage of a TargetRpc to specify remote action on one client, rather than a ClientRpc, which would initiate actions on all clients
namespace MirrorBasics {
    public class TargetClick : NetworkBehaviour
    {

        //OnTargetClick() is called by the PointerDown event on the Event Trigger component attached to this gameobject
        public void OnTargetClick()
        {
            //locate the PlayerManager in this Client
            var networkIdentity = new NobleConnect.Mirror.NobleClient();
            PlayerManager pm = networkIdentity.connection.identity.GetComponent<PlayerManager>();

            //if this client hasAuthority over this gameobject, we don't need to pass in the gameobject to the server command. If it doesn't, we do!
            if (hasAuthority)
            {
                pm.CmdTargetSelfCard();
            }
            else
            {
                pm.CmdTargetOtherCard(gameObject);
            }
        }
    }
}
