using UnityEngine;
using UnityEngine.UI;

namespace NobleConnect.Examples
{
    public class TextFromFile : MonoBehaviour
    {
        public TextAsset TextFile;

        void OnValidate()
        {
            GetComponent<Text>().text = TextFile.text;
        }
    }
}
