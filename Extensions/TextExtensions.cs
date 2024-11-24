using TMPro;
using UnityEngine;

namespace Extensions {
    public static class TextExtensions {
        public static TextMeshPro CreateWorldText(string text, Transform parent, Vector3 localPosition, float cellSize, float fontSize, Color color, TextAlignmentOptions textAlignment, int sortingOrder = 0) {
            GameObject textGameObject = new GameObject("World Text", typeof(TextMeshPro));
            Transform transform = textGameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = localPosition;
            TextMeshPro textMeshPro = textGameObject.GetComponent<TextMeshPro>();
            textMeshPro.alignment = textAlignment;
            textMeshPro.text = text;
            textMeshPro.color = color;
            textMeshPro.textWrappingMode = TextWrappingModes.Normal;

            // Set the size of the TextMeshPro element to match the cell size
            textMeshPro.rectTransform.sizeDelta = new Vector2(cellSize, cellSize);
        
            // Set font size based on cell size
            textMeshPro.fontSize = fontSize;
        
            textMeshPro.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        
            textGameObject.transform.forward = -Vector3.up;
        
            return textMeshPro;
        }
    }
}
