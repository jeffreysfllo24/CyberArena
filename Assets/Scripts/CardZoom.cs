using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CardZoom : NetworkBehaviour
{
    //Canvas is located at runtime during Awake(), whereas ZoomCard is a simple card prefab located in the inspector
    public GameObject Canvas;
    public GameObject ZoomCard;

    //zoomCard is declared here for later usage, while zoomSprite is assigned during Awake() to store this gameobject's sprite
    private GameObject zoomCard;
    private Sprite zoomSprite;

    public void Awake()
    {
        Canvas = GameObject.Find("Main Canvas");
        zoomSprite = gameObject.GetComponent<Image>().sprite;
    }

    //OnHoverEnter() is called by the Pointer Enter event in the Event Trigger component attached to this gameobject
    public void OnHoverEnter()
    {
        //determine whether the client hasAuthority over this gameobject
        bool cardBeenPlayed = gameObject.GetComponent<DragDrop>().beenPlayed;
        Debug.Log("onHoverEnter" + cardBeenPlayed);
        if (!hasAuthority && !cardBeenPlayed) return;

        //if the client hasAuthority, create a new version of the card with the appropriate sprite
    
        int width = 240;
        int height = 344;
        int x = (Screen.width / 2) + (width / 2);
        int y = (Screen.height / 2) + (height / 2);

        zoomCard = Instantiate(ZoomCard, new Vector2(x,y), Quaternion.identity);
        zoomCard.GetComponent<Image>().sprite = zoomSprite;
        
        //make the card a child of the Canvas so that it is rendered on top of everything else
        zoomCard.transform.SetParent(Canvas.transform, true);

        //make the card bigger!
        RectTransform rect = zoomCard.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);
    }

    //OnHoverExit() is called by the Pointer Exit event (as well as Begin Drag) in the Event Trigger component attached to this gameobject
    public void OnHoverExit()
    {
        Destroy(zoomCard);
    }
}
