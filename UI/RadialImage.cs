using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class RadialImage : MonoBehaviour {
        public Image radialImage;
        // Workaround: Image is placed with an offset so we only need to rotate the center
        public RectTransform centerRect;
        public Image iconImage;
    }
}