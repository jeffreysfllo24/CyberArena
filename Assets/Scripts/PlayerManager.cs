using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//the "using Mirror" assembly reference is required on any script that involves networking
using Mirror;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

//the PlayerManager is the main controller script that can act as Server, Client, and Host (Server/Client). Like all network scripts, it must derive from NetworkBehaviour (instead of the standard MonoBehaviour)
namespace MirrorBasics {

    [RequireComponent (typeof (NetworkMatch))]
    public class PlayerManager : NetworkBehaviour
    {
    // Lobby Making Variables -----------------------------
    public static PlayerManager localPlayer;
    [SyncVar] public string matchID;
    [SyncVar] public int playerIndex;

    NetworkMatch networkMatch;

    [SyncVar] public Match currentMatch;

    [SerializeField] GameObject playerLobbyUI;

    Guid netIDGuid;

    // Cards -----------------------------
    public List<GameObject> DefenceCards;
    public List<GameObject> AssetCards;
    public List<GameObject> AttackCards;

    // Board Locations
    public GameObject SpaceArea1; public GameObject SpaceArea2; public GameObject SpaceArea3; public GameObject SpaceArea4; public GameObject SpaceArea5; public GameObject SpaceArea6;

    public GameObject EnemyArea1; public GameObject EnemyArea2; public GameObject EnemyArea3; public GameObject EnemyArea4; public GameObject EnemyArea5; public GameObject EnemyArea6;
    public GameObject PlayerHandArea;
    public GameObject EnemyHandArea;
    public GameObject PlayerCardArea;
    public GameObject EnemyCardArea;

    private int playerScore = 0;
    private int enemyScore = 0;
    public TextMeshProUGUI PlayerScoreText;
    public TextMeshProUGUI EnemyScoreText;
    public TextMeshProUGUI TurnCounterText;

    public GameObject ResetButton;

    //the cards List represents our deck of cards
    List<GameObject> cards = new List<GameObject>();

    List<GameObject> safeAreaList = new List<GameObject>();

    List<GameObject> enemyAreaList = new List<GameObject>();
    public bool isPlayerTurn = false; // Boolean indiciating whether it is the current players turn

    public override void OnStartClient()
    {
        Debug.Log("onStartClient Callled");

        base.OnStartClient();

        if (isLocalPlayer) {
            localPlayer = this;
        } else {
            Debug.Log ($"Spawning other player UI Prefab");
            playerLobbyUI = UILobby.instance.SpawnPlayerUIPrefab (this);
        }
    }

    public override void OnStopClient () {
        Debug.Log ($"Client Stopped");
        ClientDisconnect ();
    }

    public override void OnStopServer () {
        Debug.Log ($"Client Stopped on Server");
        ServerDisconnect ();
    }

    [ClientRpc]
    public void RpcPopulateGameObjects() {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager pm = networkIdentity.GetComponent<PlayerManager>();

        pm.PlayerHandArea = GameObject.Find("PlayerHandArea");
        pm.EnemyHandArea = GameObject.Find("EnemyHandArea");
        pm.PlayerCardArea = GameObject.Find("PlayerCardArea");
        pm.EnemyCardArea = GameObject.Find("EnemyCardArea");

        GameObject Hud = GameObject.Find("HUD");
        pm.PlayerScoreText = Hud.transform.Find("PlayerScore").GetComponent<TextMeshProUGUI>();
        pm.EnemyScoreText = Hud.transform.Find("EnemyScore").GetComponent<TextMeshProUGUI>();
        pm.TurnCounterText = Hud.transform.Find("TurnCounter").GetComponent<TextMeshProUGUI>();
        pm.ResetButton = GameObject.Find("ResetButton");
        pm.ResetButton.transform.localScale = new Vector3(0, 0, 0);

        setAreas();

        if (isServer) {
            pm.isPlayerTurn = true;
        }

        updateTurnStatus(pm.isPlayerTurn);
        // CmdUpdatePlayersConnected();
    }

    void setAreas() {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager pm = networkIdentity.GetComponent<PlayerManager>();

        pm.SpaceArea1 = GameObject.Find("SpaceArea (1)");
        pm.safeAreaList.Add(pm.SpaceArea1);

        pm.SpaceArea2 = GameObject.Find("SpaceArea (2)");
        pm.safeAreaList.Add(pm.SpaceArea2);

        pm.SpaceArea3 = GameObject.Find("SpaceArea (3)");
        pm.safeAreaList.Add(pm.SpaceArea3);
        pm.SpaceArea4 = GameObject.Find("SpaceArea (4)");
        pm.safeAreaList.Add(pm.SpaceArea4);
        pm.SpaceArea5 = GameObject.Find("SpaceArea (5)");
        pm.safeAreaList.Add(pm.SpaceArea5);
        pm.SpaceArea6 = GameObject.Find("SpaceArea (6)");
        pm.safeAreaList.Add(pm.SpaceArea6);

        pm.EnemyArea1 = GameObject.Find("EnemyArea (1)");
        pm.enemyAreaList.Add(pm.EnemyArea1);
        pm.EnemyArea2 = GameObject.Find("EnemyArea (2)");
        pm.enemyAreaList.Add(pm.EnemyArea2);
        pm.EnemyArea3 = GameObject.Find("EnemyArea (3)");
        pm.enemyAreaList.Add(pm.EnemyArea3);
        pm.EnemyArea4 = GameObject.Find("EnemyArea (4)");
        pm.enemyAreaList.Add(pm.EnemyArea4);
        pm.EnemyArea5 = GameObject.Find("EnemyArea (5)");
        pm.enemyAreaList.Add(pm.EnemyArea5);
        pm.EnemyArea6 = GameObject.Find("EnemyArea (6)");
        pm.enemyAreaList.Add(pm.EnemyArea6);
    }

    void updateTurnStatus(bool isPlayerTurnVal) {
        if (isPlayerTurnVal) {
            GameObject.Find("Button").GetComponentInChildren<TextMeshProUGUI>().text = "End Turn";
        } else {
            GameObject.Find("Button").GetComponentInChildren<TextMeshProUGUI>().text = "Enemy Turn";
        }
    }

    void RpcUpdateEndGameText(int playerScore, int enemyScore) {     
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager pm = networkIdentity.GetComponent<PlayerManager>();

        if (pm.ResetButton == null){
            Debug.Log("Reset Button Null in update endgame text");
            return;
        }

        if (playerScore > enemyScore) {
            pm.ResetButton.transform.Find("EndGameText").GetComponentInChildren<Text>().text = "Congratulations You Win!";
        } else if (enemyScore > playerScore) {
            pm.ResetButton.transform.Find("EndGameText").GetComponentInChildren<Text>().text = "You Lost, Better Luck Next Time!";
        } else {
            pm.ResetButton.transform.Find("EndGameText").GetComponentInChildren<Text>().text = "It's a Tie!";
        }

        pm.ResetButton.transform.localScale = new Vector3(1, 1, 1);
    }

    void Awake () {
        networkMatch = GetComponent<NetworkMatch> ();
    }

    //when the server starts, store Card1 and Card2 in the cards deck. Note that server-only methods require the [Server] attribute immediately preceding them!
    [Server]
    public override void OnStartServer()
    {
        netIDGuid = netId.ToString ().ToGuid ();
        networkMatch.matchId = netIDGuid;

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
            GameObject card = Instantiate(cards[UnityEngine.Random.Range(0, cards.Count)], new Vector2(0, 0), Quaternion.identity);
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

    [Command]
    public void ReplayGame(){
        Debug.Log("Replay Game called in player manager");
        RpcResetGame();
        CmdGameManagerResetTurns();
        CmdGameManagerInitiateGame();
    }

    [ClientRpc]
    void RpcUpdateTurnCounter(int turn)
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager pm = networkIdentity.GetComponent<PlayerManager>();
        // a display turn includes player one and player two taking a turn
        int displayTurn = 1 + turn/2;

        if (displayTurn > 10) {
            RpcUpdateEndGameText(pm.playerScore, pm.enemyScore);
            return;
        }

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
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager pm = networkIdentity.GetComponent<PlayerManager>();

        //if the card has been "Dealt," determine whether this Client has authority over it, and send it either to the PlayerHandArea or EnemyArea, accordingly. For the latter, flip it so the player can't see the front!
        if (type == "Dealt"){
            if (hasAuthority)
            {
                card.transform.SetParent(pm.PlayerHandArea.transform, false);
            }
            else
            {
                card.transform.SetParent(pm.EnemyHandArea.transform, false);
                card.GetComponent<CardFlipper>().Flip();
            }
        }
        // The card has been played check if it is an attack card
        else if (card.tag == "Attack") {
            if (hasAuthority) {
                Transform child = null;
                if (playAreaName == "EnemyArea (1)") {
                    child = pm.EnemyArea1.transform.GetChild(pm.EnemyArea1.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (2)") {
                    child = pm.EnemyArea2.transform.GetChild(pm.EnemyArea2.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (3)") {
                    child = pm.EnemyArea3.transform.GetChild(pm.EnemyArea3.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (4)") {
                    child = pm.EnemyArea4.transform.GetChild(pm.EnemyArea4.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (5)") {
                    child = pm.EnemyArea5.transform.GetChild(pm.EnemyArea5.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (6)") {
                    child = pm.EnemyArea6.transform.GetChild(pm.EnemyArea6.transform.childCount - 1);
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
                    child = pm.SpaceArea1.transform.GetChild(pm.SpaceArea1.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (2)") {
                    child = pm.SpaceArea2.transform.GetChild(pm.SpaceArea2.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (3)") {
                    child = pm.SpaceArea3.transform.GetChild(pm.SpaceArea3.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (4)") {
                    child = pm.SpaceArea4.transform.GetChild(pm.SpaceArea4.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (5)") {
                    child = pm.SpaceArea5.transform.GetChild(pm.SpaceArea5.transform.childCount - 1);
                } else if (playAreaName == "EnemyArea (6)") {
                    child = pm.SpaceArea6.transform.GetChild(pm.SpaceArea6.transform.childCount - 1);
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
                    card.transform.SetParent(pm.SpaceArea1.transform, false);
                } else if (playAreaName == "SpaceArea (2)") {
                    card.transform.SetParent(pm.SpaceArea2.transform, false);
                } else if (playAreaName == "SpaceArea (3)") {
                    card.transform.SetParent(pm.SpaceArea3.transform, false);
                } else if (playAreaName == "SpaceArea (4)") {
                    card.transform.SetParent(pm.SpaceArea4.transform, false);
                } else if (playAreaName == "SpaceArea (5)") {
                    card.transform.SetParent(pm.SpaceArea5.transform, false);
                } else if (playAreaName == "SpaceArea (6)") {
                    card.transform.SetParent(pm.SpaceArea6.transform, false);
                }
            } else {
                if (playAreaName == "SpaceArea (1)") {
                    card.transform.SetParent(pm.EnemyArea1.transform, false);
                } else if (playAreaName == "SpaceArea (2)") {
                    card.transform.SetParent(pm.EnemyArea2.transform, false);
                } else if (playAreaName == "SpaceArea (3)") {
                    card.transform.SetParent(pm.EnemyArea3.transform, false);
                } else if (playAreaName == "SpaceArea (4)") {
                    card.transform.SetParent(pm.EnemyArea4.transform, false);
                } else if (playAreaName == "SpaceArea (5)") {
                    card.transform.SetParent(pm.EnemyArea5.transform, false);
                } else if (playAreaName == "SpaceArea (6)") {
                    card.transform.SetParent(pm.EnemyArea6.transform, false);
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
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager pm = networkIdentity.GetComponent<PlayerManager>();

        Debug.Log("Jeffrey: Game Initiated");

        if(pm.ResetButton != null) {
            Debug.Log("Client Start Reset Button not Null");
            pm.ResetButton.transform.localScale = new Vector3(0, 0, 0);
        } else {
            Debug.Log("Client Start Reset Button is Null");
        }
        
        // Deal 5 cards to each client/player
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
            foreach(var safeArea in pm.safeAreaList)
            {
                pm.playerScore += (safeArea.transform.childCount > 0) ? 1:0;
            }
            pm.PlayerScoreText.text = pm.playerScore.ToString();
        } else {
            foreach(var enemyArea in pm.enemyAreaList)
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

    [ClientRpc]
    void RpcResetGame() {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager pm = networkIdentity.GetComponent<PlayerManager>();

        // Delete all cards in safe areas, enemy area, player dealt cards and enemy dealt cards
        deleteListChildren(pm.safeAreaList);
        deleteListChildren(pm.enemyAreaList);
        
        foreach (Transform child in pm.PlayerHandArea.transform) {
            Destroy(child.gameObject);
        }

        foreach (Transform child in pm.EnemyHandArea.transform) {
            Destroy(child.gameObject);
        }

        // Reset the turn and score
        resetPlayerScoresAndTurn();

        // Set active turn and tell Game Manager to initiate game
        if (isServer) {
            pm.isPlayerTurn = true;
        } else {
            pm.isPlayerTurn = false;
        }

        updateTurnStatus(pm.isPlayerTurn);

    }

    void deleteListChildren( List<GameObject> list) {
        foreach(var gameObject in list) {
            foreach (Transform child in gameObject.transform) {
                Destroy(child.gameObject);
            }
        }
    }

    void resetPlayerScoresAndTurn() {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager pm = networkIdentity.GetComponent<PlayerManager>();

        pm.playerScore = 0;
        pm.PlayerScoreText.text = "0";
        pm.enemyScore = 0;
        pm.EnemyScoreText.text = "0";

        pm.TurnCounterText.text = "Turn: 1/10";
    }

    [Server]
    void CmdGameManagerResetTurns() {
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (gm != null) {
            Debug.Log("Game manager in reset is not null");
            gm.ResetTurnsPlayed();
        } else {
            Debug.Log("Game manager in reset is null");
        }
    }

    [Server]
    void CmdGameManagerInitiateGame() {
        GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (gm != null) {
            Debug.Log("Game manager in initiate game is not null");
            gm.initiateGame();
        } else {
            Debug.Log("Game manager in initiate game is null");
        }
    }

    /* 
        HOST MATCH
    */

    public void HostGame (bool publicMatch) {
        string matchID = MatchMaker.GetRandomMatchID ();
        CmdHostGame (matchID, publicMatch);
    }

    [Command]
    void CmdHostGame (string _matchID, bool publicMatch) {
        matchID = _matchID;
        if (MatchMaker.instance.HostGame (_matchID, this, publicMatch, out playerIndex)) {
            Debug.Log ($"<color=green>Game hosted successfully</color>");
            networkMatch.matchId = _matchID.ToGuid ();
            TargetHostGame (true, _matchID, playerIndex);
        } else {
            Debug.Log ($"<color=red>Game hosted failed</color>");
            TargetHostGame (false, _matchID, playerIndex);
        }
    }

    [TargetRpc]
    void TargetHostGame (bool success, string _matchID, int _playerIndex) {
        playerIndex = _playerIndex;
        matchID = _matchID;
        Debug.Log ($"MatchID: {matchID} == {_matchID}");
        UILobby.instance.HostSuccess (success, _matchID);
    }

    /* 
        JOIN MATCH
    */

    public void JoinGame (string _inputID) {
        CmdJoinGame (_inputID);
    }

    [Command]
    void CmdJoinGame (string _matchID) {
        matchID = _matchID;
        if (MatchMaker.instance.JoinGame (_matchID, this, out playerIndex)) {
            Debug.Log ($"<color=green>Game Joined successfully</color>");
            networkMatch.matchId = _matchID.ToGuid ();
            TargetJoinGame (true, _matchID, playerIndex);

            //Host
            if (isServer && playerLobbyUI != null) {
                playerLobbyUI.SetActive (true);
            }
        } else {
            Debug.Log ($"<color=red>Game Joined failed</color>");
            TargetJoinGame (false, _matchID, playerIndex);
        }
    }

    [TargetRpc]
    void TargetJoinGame (bool success, string _matchID, int _playerIndex) {
        playerIndex = _playerIndex;
        matchID = _matchID;
        Debug.Log ($"MatchID: {matchID} == {_matchID}");
        UILobby.instance.JoinSuccess (success, _matchID);
    }

    /* 
        DISCONNECT
    */

    public void DisconnectGame () {
        CmdDisconnectGame ();
    }

    [Command]
    void CmdDisconnectGame () {
        ServerDisconnect ();
    }

    void ServerDisconnect () {
        MatchMaker.instance.PlayerDisconnected (this, matchID);
        RpcDisconnectGame ();
        networkMatch.matchId = netIDGuid;
    }

    [ClientRpc]
    void RpcDisconnectGame () {
        ClientDisconnect ();
    }

    void ClientDisconnect () {
        if (playerLobbyUI != null) {
            if (!isServer) {
                Destroy (playerLobbyUI);
            } else {
                playerLobbyUI.SetActive (false);
            }
        }
    }

    /* 
        SEARCH MATCH
    */

    public void SearchGame () {
        // CmdSearchGame ();
    }

    // [Command]
    // void CmdSearchGame () {
    //     if (MatchMaker.instance.SearchGame (this, out playerIndex, out matchID)) {
    //         Debug.Log ($"<color=green>Game Found Successfully</color>");
    //         networkMatch.matchId = matchID.ToGuid ();
    //         TargetSearchGame (true, matchID, playerIndex);

    //         //Host
    //         if (isServer && playerLobbyUI != null) {
    //             playerLobbyUI.SetActive (true);
    //         }
    //     } else {
    //         Debug.Log ($"<color=red>Game Search Failed</color>");
    //         TargetSearchGame (false, matchID, playerIndex);
    //     }
    // }

    // [TargetRpc]
    // void TargetSearchGame (bool success, string _matchID, int _playerIndex) {
    //     playerIndex = _playerIndex;
    //     matchID = _matchID;
    //     Debug.Log ($"MatchID: {matchID} == {_matchID} | {success}");
    //     UILobby.instance.SearchGameSuccess (success, _matchID);
    // }

    /* 
        MATCH PLAYERS
    */

    [Server]
    public void PlayerCountUpdated (int playerCount) {
        TargetPlayerCountUpdated (playerCount);
    }

    [TargetRpc]
    void TargetPlayerCountUpdated (int playerCount) {
        if (playerCount > 1) {
            UILobby.instance.SetStartButtonActive(true);
        } else {
            UILobby.instance.SetStartButtonActive(false);
        }
    }

    /* 
        BEGIN MATCH
    */

    public void BeginGame () {
        CmdBeginGame ();
    }

    [Command]
    void CmdBeginGame () {
        MatchMaker.instance.BeginGame (matchID);
        Debug.Log ($"<color=red>Game Beginning</color>");
    }

    public void StartGame () { //Server
        TargetBeginGame ();
    }

    [TargetRpc]
    void TargetBeginGame () {
        Debug.Log ($"MatchID: {matchID} | Beginning");

        //Additively load game scene
        SceneManager.LoadScene(0, LoadSceneMode.Additive);
    }

    }
}
