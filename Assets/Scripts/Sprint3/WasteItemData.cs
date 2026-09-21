using UnityEngine;
using ReCiclo.Sprint4;

namespace ReCiclo.Sprint3
{
    [CreateAssetMenu(fileName = "NewWasteItemData", menuName = "ReCiclo/Waste Item Data")]
    public class WasteItemData : ScriptableObject
    {
        public string itemName;
        public WasteCategory category;
        public int basePoints = 100;
        public Sprite itemSprite;
        public Color tintColor = Color.white;
        [TextArea] public string educationalFact;
    }
}
