using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mirror;

public class CardZoom : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //Canvas is located at runtime during Awake(), whereas ZoomCard is a simple card prefab located in the inspector
    public GameObject Canvas;
    public GameObject ZoomCard;

    //zoomCard is declared here for later usage, while zoomSprite is assigned during Awake() to store this gameobject's sprite
    private GameObject zoomCard;

    float x1;
    float x2;
    float y1;
    float y2;

    public void Awake()
    {
        Canvas = GameObject.Find("Main Canvas");
    }

    //OnHoverEnter() is called by the Pointer Enter event in the Event Trigger component attached to this gameobject
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        // only create one zoom card at a time
        if (zoomCard != null) return;

        //determine whether the client hasAuthority over this gameobject
        bool cardBeenPlayed = gameObject.GetComponent<DragDrop>().beenPlayed;
        Debug.Log("onHoverEnter" + cardBeenPlayed);
        if (!hasAuthority && !cardBeenPlayed) return;

        //if the client hasAuthority, create a new version of the card with the appropriate sprite
    
        int width = 120;
        int height = 177;
        int x = (Screen.width / 2) + (width / 2);
        int y = (Screen.height / 2) + (height / 2);

        zoomCard = Instantiate(ZoomCard, new Vector2(x,y), Quaternion.identity);
        
        //make the card a child of the Canvas so that it is rendered on top of everything else
        zoomCard.transform.SetParent(Canvas.transform, true);
    }

    //OnHoverExit() is called by the Pointer Exit event (as well as Begin Drag) in the Event Trigger component attached to this gameobject
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        // Read card dimensions at runtime after instantiation
        if (x1 == 0.0f) {
            x1 = gameObject.transform.position.x - (GetComponent<BoxCollider2D>().size.x / 2);
            x2 = gameObject.transform.position.x + (GetComponent<BoxCollider2D>().size.x / 2);
            y1 = gameObject.transform.position.y - (GetComponent<BoxCollider2D>().size.y / 2);
            y2 = gameObject.transform.position.y + (GetComponent<BoxCollider2D>().size.y / 2);
        }

        // Don't destroy card if still within card bounds (aka hovering over child of card object)
        if (pointerEventData.position.x > x1 && pointerEventData.position.x < x2 &&
            pointerEventData.position.y > y1 && pointerEventData.position.y < y2) return;

        Destroy(zoomCard);
        zoomCard = null;
    }

    public void StartDrag()
    {
        // Cancel zoom when player picks up card
        Destroy(zoomCard);
        zoomCard = null;
    }
}
