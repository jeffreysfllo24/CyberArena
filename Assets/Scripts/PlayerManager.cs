using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//the "using Mirror" assembly reference is required on any script that involves networking
using Mirror;
using UnityEngine.UI;
using TMPro;

//the PlayerManager is the main controller script that can act as Server, Client, and Host (Server/Client). Like all network scripts, it must derive from NetworkBehaviour (instead of the standard MonoBehaviour)
public class PlayerManager : NetworkBehaviour
{
    // Cards
    public List<GameObject> DefenceCards;
    public List<GameObject> AssetCards;
    public List<GameObject> AttackCards;

    // Board Locations
    public GameObject SpaceArea1; public GameObject SpaceArea2; public GameObject SpaceArea3; public GameObject SpaceArea4; public GameObject SpaceArea5; public GameObject SpaceArea6; public GameObject SpaceArea7; public GameObject SpaceArea8;

    public GameObject EnemyArea1; public GameObject EnemyArea2; public GameObject EnemyArea3; public GameObject EnemyArea4; public GameObject EnemyArea5; public GameObject EnemyArea6; public GameObject EnemyArea7; public GameObject EnemyArea8;
    public GameObject PlayerArea;
    public GameObject EnemyArea;
    public GameObject AssetArea;
    public GameObject DefenceArea;
    public GameObject EnemyAssetArea;
    public GameObject EnemyDefenceArea;

    private int playerScore = 0;
    private int enemyScore = 0;
    public TextMeshProUGUI PlayerScoreText;
    public TextMeshProUGUI EnemyScoreText;
    public TextMeshProUGUI TurnCounterText;

    //the cards List represents our deck of cards
    List<GameObject> cards = new List<GameObject>();

    List<GameObject> safeAreaList = new List<GameObject>();

    List<GameObject> enemyAreaList = new List<GameObject>();
    public bool isPlayerTurn = false; // Boolean indiciating whether it is the current players turn

    public override void OnStartClient()
    {
        base.OnStartClient();

        PlayerArea = GameObject.Find("PlayerArea");
        EnemyArea = GameObject.Find("EnemyArea");
        AssetArea = GameObject.Find("AssetArea");
        DefenceArea = GameObject.Find("DefenceArea");
        EnemyAssetArea = GameObject.Find("EnemyAssetArea");
        EnemyDefenceArea = GameObject.Find("EnemyDefenceArea");

        GameObject Hud = GameObject.Find("HUD");
        PlayerScoreText = Hud.transform.Find("PlayerScore").GetComponent<TextMeshProUGUI>();
        EnemyScoreText = Hud.transform.Find("EnemyScore").GetComponent<TextMeshProUGUI>();
        TurnCounterText = Hud.transform.Find("TurnCounter").GetComponent<TextMeshProUGUI>();

        setAreas();

        if (isServer) {
            isPlayerTurn = true;
        }
        updateTurnStatus(isPlayerTurn);
        
        CmdUpdatePlayersConnected();
    }

    void setAreas() {
        SpaceArea1 = GameObject.Find("SpaceArea (1)");
        safeAreaList.Add(SpaceArea1);

        SpaceArea2 = GameObject.Find("SpaceArea (2)");
        safeAreaList.Add(SpaceArea2);

        SpaceArea3 = GameObject.Find("SpaceArea (3)");
        safeAreaList.Add(SpaceArea3);
        SpaceArea4 = GameObject.Find("SpaceArea (4)");
        safeAreaList.Add(SpaceArea4);
        SpaceArea5 = GameObject.Find("SpaceArea (5)");
        safeAreaList.Add(SpaceArea5);
        SpaceArea6 = GameObject.Find("SpaceArea (6)");
        safeAreaList.Add(SpaceArea6);
        SpaceArea7 = GameObject.Find("SpaceArea (7)");
        safeAreaList.Add(SpaceArea7);
        SpaceArea8 = GameObject.Find("SpaceArea (8)");
        safeAreaList.Add(SpaceArea8);

        EnemyArea1 = GameObject.Find("EnemyArea (1)");
        enemyAreaList.Add(EnemyArea1);
        EnemyArea2 = GameObject.Find("EnemyArea (2)");
        enemyAreaList.Add(EnemyArea2);
        EnemyArea3 = GameObject.Find("EnemyArea (3)");
        enemyAreaList.Add(EnemyArea3);
        EnemyArea4 = GameObject.Find("EnemyArea (4)");
        enemyAreaList.Add(EnemyArea4);
        EnemyArea5 = GameObject.Find("EnemyArea (5)");
        enemyAreaList.Add(EnemyArea5);
        EnemyArea6 = GameObject.Find("EnemyArea (6)");
        enemyAreaList.Add(EnemyArea6);
        EnemyArea7 = GameObject.Find("EnemyArea (7)");
        enemyAreaList.Add(EnemyArea7);
        EnemyArea8 = GameObject.Find("EnemyArea (8)");
        enemyAreaList.Add(EnemyArea8);
    }

    void updateTurnStatus(bool isPlayerTurnVal) {
        if (isPlayerTurnVal) {
            GameObject.Find("Button").GetComponentInChildren<Text>().text = "End Turn";
        } else {
            GameObject.Find("Button").GetComponentInChildren<Text>().text = "Enemy Turn";
        }
    }

    //when the server starts, store Card1 and Card2 in the cards deck. Note that server-only methods require the [Server] attribute immediately preceding them!
    [Server]
    public override void OnStartServer()
    {
        cards.AddRange(DefenceCards);
        cards.AddRange(AssetCards);
        cards.AddRange(AttackCards);
    }
    
    //Commands are methods requested by Clients to run on the Server, and require the [Command] attribute immediately preceding them. CmdDealCards() is called by the DrawCards script attached to the client Button
    [Command]
    public void CmdDealCards(int n)
    {
        //(5x) Spawn a random card from the cards deck on the Server, assigning authority over it to the Client that requested the Command. Then run RpcShowCard() and indicate that this card was "Dealt"
        for (int i = 0; i < n; i++)
        {
            GameObject card = Instantiate(cards[Random.Range(0, cards.Count)], new Vector2(0, 0), Quaternion.identity);
            NetworkServer.Spawn(card, connectionToClient);
            RpcShowCard(card, "Dealt", "");
        }
    }

    //PlayCard() is called by the DragDrop script when a card is placed in the DropZone, and requests CmdPlayCard() from the Server
    public void PlayCard(GameObject card, string playAreaName)
    {
        CmdPlayCard(card, playAreaName);
    }

    //CmdPlayCard() uses the same logic as CmdDealCards() in rendering cards on all Clients, except that it specifies that the card has been "Played" rather than "Dealt"
    [Command]
    void CmdPlayCard(GameObject card, string playAreaName)
    {
        RpcShowCard(card, "Played", playAreaName);
    }

    //UpdateTurnsPlayed() is run only by the Server, finding the Server-only GameManager game object and incrementing the relevant variable
    [Server]
    void UpdateTurnsPlayed()
    {
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.UpdateTurnsPlayed();
        RpcUpdateTurnCounter(gm.TurnsPlayed);
        
    }

    [ClientRpc]
    void RpcUpdateTurnCounter(int turn)
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager pm = networkIdentity.GetComponent<PlayerManager>();
        // a display turn includes player one and player two taking a turn
        int displayTurn = 1 + turn/2;
        pm.TurnCounterText.text = "Turn: " + displayTurn + "/10";
        Debug.Log("Turns Played: " + turn);
    }

    //CmdUpdatePlayersConnected() find the GameManager game object and incrementing the number of players connected
    [Command]
    void CmdUpdatePlayersConnected()
    {
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        gm.CmdUpdatePlayerConnected();
    }

    [ClientRpc]
    void RpcIncrementClients(string message)
    {
        Debug.Log(message);
    }

    //RpcLogToClients demonstrates how to request all clients to log a message to their respective consoles
    [ClientRpc]
    void RpcLogToClients(string message)
    {
        Debug.Log(message);
    }

    //ClientRpcs are methods requested by the Server to run on all Clients, and require the [ClientRpc] attribute immediately preceding them
    [ClientRpc]
    void RpcShowCard(GameObject card, string type, string playAreaName)
    {
        Debug.Log(playAreaName);

        //if the card has been "Dealt," determine whether this Client has authority over it, and send it either to the PlayerArea or EnemyArea, accordingly. For the latter, flip it so the player can't see the front!
        if (type == "Dealt"){
            if (hasAuthority)
            {
                card.transform.SetParent(PlayerArea.transform, false);
            }
            else
            {
                card.transform.SetParent(EnemyArea.transform, false);
                card.GetComponent<CardFlipper>().Flip();
            }
        }
        // The card has been played check if it is an attack card
        else if (card.tag == "Attack") {
            if (hasAuthority) {
                Transform child = null;
                if (playAreaName == "EnemyArea (1)") {
                    child = EnemyArea1.transform.GetChild(EnemyArea1.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (2)") {
                    child = EnemyArea2.transform.GetChild(EnemyArea2.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (3)") {
                    child = EnemyArea3.transform.GetChild(EnemyArea3.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (4)") {
                    child = EnemyArea4.transform.GetChild(EnemyArea4.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (5)") {
                    child = EnemyArea5.transform.GetChild(EnemyArea5.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (6)") {
                    child = EnemyArea6.transform.GetChild(EnemyArea6.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (7)") {
                    child = EnemyArea7.transform.GetChild(EnemyArea7.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (8)") {
                    child = EnemyArea8.transform.GetChild(EnemyArea8.transform.childCount - 1);
                }

                if (child != null) {
                    // Remove asset or defence as well as the used up attack card
                    Destroy(child.gameObject);
                    Destroy(card);
                    Debug.Log("Child succesfully removed for authority drop zone" + playAreaName);
                } else {
                    Debug.Log("Child is null for drop zone" + playAreaName);
                }

            } else {
                Transform child = null;
                if (playAreaName == "EnemyArea (1)") {
                    child = SpaceArea1.transform.GetChild(SpaceArea1.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (2)") {
                    child = SpaceArea2.transform.GetChild(SpaceArea2.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (3)") {
                    child = SpaceArea3.transform.GetChild(SpaceArea3.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (4)") {
                    child = SpaceArea4.transform.GetChild(SpaceArea4.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (5)") {
                    child = SpaceArea5.transform.GetChild(SpaceArea5.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (6)") {
                    child = SpaceArea6.transform.GetChild(SpaceArea6.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (7)") {
                    child = SpaceArea7.transform.GetChild(SpaceArea7.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (8)") {
                    child = SpaceArea8.transform.GetChild(SpaceArea8.transform.childCount - 1);
                }
                if (child != null) {
                    // Remove asset or defence as well as the used up attack card
                    Destroy(child.gameObject);
                    Destroy(card);
                    Debug.Log("Child succesfully removed for no authority drop zone" + playAreaName);
                } else {
                    Debug.Log("Child is null for no authority drop zone" + playAreaName);
                }
            }
        }
        //if the card has been "Played," and is not an attack card send it to the DropZone. If this Client doesn't have authority over it, flip it so the player can now see the front!
        else if (type == "Played")
        {
            if (hasAuthority) {
                if (playAreaName == "SpaceArea (1)") {
                    card.transform.SetParent(SpaceArea1.transform, false);
                } else if (playAreaName == "SpaceArea (2)") {
                    card.transform.SetParent(SpaceArea2.transform, false);
                } else if (playAreaName == "SpaceArea (3)") {
                    card.transform.SetParent(SpaceArea3.transform, false);
                } else if (playAreaName == "SpaceArea (4)") {
                    card.transform.SetParent(SpaceArea4.transform, false);
                } else if (playAreaName == "SpaceArea (5)") {
                    card.transform.SetParent(SpaceArea5.transform, false);
                } else if (playAreaName == "SpaceArea (6)") {
                    card.transform.SetParent(SpaceArea6.transform, false);
                } else if (playAreaName == "SpaceArea (7)") {
                    card.transform.SetParent(SpaceArea7.transform, false);
                } else if (playAreaName == "SpaceArea (8)") {
                    card.transform.SetParent(SpaceArea8.transform, false);
                }
            } else {
                if (playAreaName == "SpaceArea (1)") {
                    card.transform.SetParent(EnemyArea1.transform, false);
                } else if (playAreaName == "SpaceArea (2)") {
                    card.transform.SetParent(EnemyArea2.transform, false);
                } else if (playAreaName == "SpaceArea (3)") {
                    card.transform.SetParent(EnemyArea3.transform, false);
                } else if (playAreaName == "SpaceArea (4)") {
                    card.transform.SetParent(EnemyArea4.transform, false);
                } else if (playAreaName == "SpaceArea (5)") {
                    card.transform.SetParent(EnemyArea5.transform, false);
                } else if (playAreaName == "SpaceArea (6)") {
                    card.transform.SetParent(EnemyArea6.transform, false);
                } else if (playAreaName == "SpaceArea (7)") {
                    card.transform.SetParent(EnemyArea7.transform, false);
                } else if (playAreaName == "SpaceArea (8)") {
                    card.transform.SetParent(EnemyArea8.transform, false);
                }
            }

            if (!hasAuthority)
            {
                card.GetComponent<CardFlipper>().Flip();
            }
        }
    }

    //RPCInitiateGame() is called from the server-only GameManager game object when BOTH players have connected.
    [ClientRpc]
    public void RpcInitiateGame()
    {
        Debug.Log("Game Initiated");

        // Deal 5 cards to each client/player
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager pm = networkIdentity.GetComponent<PlayerManager>();
        pm.CmdDealCards(5);
    }

    [Command]
    public void CmdSwitchTurns()
    {
        RpcSwitchTurns();
        UpdateTurnsPlayed();
    }

    //RpcSwitchTurns() is called when a player ends their turn, each client will flip its turn
    [ClientRpc]
    public void RpcSwitchTurns()
    {
        Debug.Log("RpcSwitchTurns Called");
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager pm = networkIdentity.GetComponent<PlayerManager>();

        // calculate score
        if (pm.isPlayerTurn) {
            Debug.Log("Number of assets: " + AssetArea.transform.childCount);
            foreach(var safeArea in safeAreaList)
            {
                pm.playerScore += (safeArea.transform.childCount > 0) ? 1:0;
            }
            pm.PlayerScoreText.text = pm.playerScore.ToString();
        } else {
            Debug.Log("Number of enemy assets: " + AssetArea.transform.childCount);
            foreach(var enemyArea in enemyAreaList)
            {
                pm.enemyScore += (enemyArea.transform.childCount > 0) ? 1:0;
            }
            pm.EnemyScoreText.text = pm.enemyScore.ToString();
        }

        pm.isPlayerTurn = !pm.isPlayerTurn;
        updateTurnStatus(pm.isPlayerTurn);

        if (pm.isPlayerTurn) { // draw card if starting turn
            pm.CmdDealCards(1);
        }
    }

    //CmdTargetSelfCard() is called by the TargetClick script if the Client hasAuthority over the gameobject that was clicked
    [Command]
    public void CmdTargetSelfCard()
    {
        TargetSelfCard();
    }

    //CmdTargetOtherCard is called by the TargetClick script if the Client does not hasAuthority (err...haveAuthority?!?) over the gameobject that was clicked
    [Command]
    public void CmdTargetOtherCard(GameObject target)
    {
        NetworkIdentity opponentIdentity = target.GetComponent<NetworkIdentity>();
        TargetOtherCard(opponentIdentity.connectionToClient);
    }

    //TargetRpcs are methods requested by the Server to run on a target Client. If no NetworkConnection is specified as the first parameter, the Server will assume you're targeting the Client that hasAuthority over the gameobject
    [TargetRpc]
    void TargetSelfCard()
    {
        Debug.Log("Targeted by self!");
    }

    [TargetRpc]
    void TargetOtherCard(NetworkConnection target)
    {
        Debug.Log("Targeted by other!");
    }

    //CmdIncrementClick() is called by the IncrementClick script
    [Command]
    public void CmdIncrementClick(GameObject card)
    {
        RpcIncrementClick(card);
    }

    //RpcIncrementClick() is called on all clients to increment the NumberOfClicks SyncVar within the IncrementClick script and log it to the debugger to demonstrate that it's working
    [ClientRpc]
    void RpcIncrementClick(GameObject card)
    {
        card.GetComponent<IncrementClick>().NumberOfClicks++;
        Debug.Log("This card has been clicked " + card.GetComponent<IncrementClick>().NumberOfClicks + " times!");
    }
}
