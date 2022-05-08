using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MirrorBasics {

    public class DrawCards : NetworkBehaviour
    {

        //OnClick() is called by the OnClick() event within the Button component
        public void OnClick()
        {
            //locate the PlayerManager in this Client and request the Server to deal cards
            var networkIdentity = new NobleConnect.Mirror.NobleClient();
            PlayerManager pm = networkIdentity.connection.identity.GetComponent<PlayerManager>();

            // Only if it is the players turn can they end it
            if (pm.isPlayerTurn) {
                pm.CmdSwitchTurns();
            }
        }

    }
}
