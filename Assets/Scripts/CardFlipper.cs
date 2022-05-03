using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardFlipper : MonoBehaviour
{
    public void Flip()
    {
        GameObject cardBack = gameObject.transform.Find("CardBack").gameObject;
        cardBack.SetActive(!cardBack.activeSelf);
    }
}
