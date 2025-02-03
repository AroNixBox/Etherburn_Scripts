using UnityEngine;
using UnityEngine.UI;

namespace UI {
    [RequireComponent(typeof(Image))]
    public class ImageFillAmountMirror : MonoBehaviour {
        [SerializeField] Image mirrorFillImage;
        Image _image;

        void Start() {
            _image = GetComponent<Image>();
        }

        void Update() {
            _image.fillAmount = mirrorFillImage.fillAmount;
        }
    }
}