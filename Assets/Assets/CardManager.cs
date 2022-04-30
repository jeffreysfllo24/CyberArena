using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour {
    // Singleton https://gamedev.stackexchange.com/questions/116009/in-unity-how-do-i-correctly-implement-the-singleton-pattern
    // singleton isn't working
    // private static CardManager _instance;
    // public static CardManager Instance { get { return _instance; } }

    // private void Awake() {
    //     if (_instance != null && _instance != this) {
    //         Destroy(this.gameObject);
    //     } else {
    //         _instance = this;
    //     }
    // }

    // TODO: figure out how to use array instead
    public Card smartphone;
    public Card encryption;

    public List<Card> getCards() {
        List<Card> cards = new List<Card>();
        cards.Add(smartphone);
        cards.Add(encryption);
        return cards;
    }
}
