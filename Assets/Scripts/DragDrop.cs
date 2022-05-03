using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DragDrop : NetworkBehaviour
{
    //Canvas is assigned locally at runtime in Start(), whereas the rest are assigned contextually as this gameobject is dragged and dropped
    public GameObject Canvas;
    public PlayerManager PlayerManager;

    private bool isDragging = false;
    private bool isOverDropZone = false;
    private bool isDraggable = true;
    private GameObject dropZone;
    private GameObject startParent;
    private Vector2 startPosition;
    
    [SyncVar]
    public bool beenPlayed = false;

    private void Start()
    {
        Canvas = GameObject.Find("Main Canvas");
        
        //check whether this client hasAuthority to manipulate this gameobject
        if (!hasAuthority)
        {
            isDraggable = false;
        }
    }
    void Update()
    {
        //check every frame to see if this gameobject is being dragged. If it is, make it follow the mouse and set it as a child of the Canvas to render above everything else
        if (isDragging)
        {
            transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            transform.SetParent(Canvas.transform, true);
        }        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //in our scene, if this gameobject collides with something, it must be the dropzone, as specified in the layer collision matrix (cards are part of the "Cards" layer and the dropzone is part of the "DropZone" layer)
        dropZone = collision.gameObject;

        if (isOverDropZoneAreas(collision.gameObject)) {
            isOverDropZone = true;
        }

        Debug.Log("Entering Game Object" + dropZone.name);
    }

    private bool isOverDropZoneAreas(GameObject dropZone) {
        return (dropZone.name.StartsWith("EnemyArea") || dropZone.name.StartsWith("SpaceArea"));
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isOverDropZone = false;

        Debug.Log("Exiting Game Object" + collision.gameObject.name);
        dropZone = null;
    }

    //StartDrag() is called by the Begin Drag event in the Event Trigger component attached to this gameobject
    public void StartDrag()
    {
        //if the gameobject is draggable, store the parent and position of it so we know where to return it if it isn't put in a dropzone
        if (!isDraggable || !isPlayerTurn()) return;
        startParent = transform.parent.gameObject;
        startPosition = transform.position;
        isDragging = true;
    }

    //EndDrag() is called by the End Drag event in the Event Trigger component attached to this gameobject
    public void EndDrag()
    {
        // Debug.Log(gameObject.tag + " " + dropZone.name +  " " + startParent.name);
        Debug.Log("" + isDraggable + isOverDropZone);

        if (!isDraggable || !isPlayerTurn()) return;
        isDragging = false;
        
        // TODO add logic to determine if correct card type (defence, asset) is placed in correct area using game object name or property

        //if the gameobject is put in a dropzone, set it as a child of the dropzone and access the PlayerManager of this client to let the server know a card has been played
        if (isOverDropZone && isValidMove(gameObject.tag, startParent.name, dropZone))
        {
            transform.SetParent(dropZone.transform, false);
            isDraggable = false;
            beenPlayed = true;
            NetworkIdentity networkIdentity = NetworkClient.connection.identity;
            PlayerManager = networkIdentity.GetComponent<PlayerManager>();
            PlayerManager.PlayCard(gameObject, dropZone.name);
        }
        //otherwise, send it back from whence it came
        else
        {
            transform.position = startPosition;
            transform.SetParent(startParent.transform, false);
        }
    }

    private bool isValidMove(string tag, string startZone, GameObject endZone) {
        // Valid Moves
        // - Asset and Defence tag cards can be placed our players side zones
        // - A defence card must be placed ON an asset, hence an asset must be placed first in a zone then a defence card
        // - Maximum two cards per zone (first must be asset, second must be defence)

        if (tag == "Asset") {
            return endZone.transform.childCount == 0;
        } else if (tag == "Defence") {
            return endZone.transform.childCount == 1;
        } else if (tag == "Attack") {
            return false;
        }
        return false;
    }

    private bool isPlayerTurn()
    {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager = networkIdentity.GetComponent<PlayerManager>();
        Debug.Log(PlayerManager.isPlayerTurn);
        return PlayerManager.isPlayerTurn;
    }
}
