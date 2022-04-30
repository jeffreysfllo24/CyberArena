using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CategoryManager : MonoBehaviour {
    public enum Category {
        Wireless,
        Account,
        Information,
        Device
    }

    // Singleton https://gamedev.stackexchange.com/questions/116009/in-unity-how-do-i-correctly-implement-the-singleton-pattern
    private static CategoryManager _instance;
    public static CategoryManager Instance { get { return _instance; } }

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    }

    public Sprite wireless;

    public Sprite spriteForCategory(Category category) {
        switch (category) {
            case Category.Wireless:
                return wireless;
            default:
                return wireless;
        }
    }
}
