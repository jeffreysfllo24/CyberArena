using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour {

	public Card card;

	public TextMeshProUGUI nameText;
	public TextMeshProUGUI hoverText;

	public Image artworkImage;
	public Image categoryImage;
	// TODO: set the colour

	// Use this for initialization
	void Start() {
		nameText.text = card.name;
		hoverText.text = card.description;
		artworkImage.sprite = card.artwork;
		categoryImage.sprite = CategoryManager.Instance.spriteForCategory(card.category);
	}
	
}
