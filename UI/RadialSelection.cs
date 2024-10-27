using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class RadialSelection : MonoBehaviour {
        [Header("References")] 
        [SerializeField, Required] RadialImage radialSelectionPrefab;
        [SerializeField, Required] Transform radialSelectionsParent;
        [SerializeField] Transform weaponSelectionParent;
        [SerializeField] Transform weaponInformation;
        [SerializeField] TMP_Text weaponHeaderText;
        [SerializeField] TMP_Text weaponDescriptionText;
        [Header("Settings")]
        [SerializeField, Range(1, 30)] uint angleBetweenSelections = 10;

        // Can use this to animate a specific radial part
        readonly List<RadialPart> _radialParts = new ();
        int _numberOfOptions;

        int _selectedIndex;
        
        void Start() {
            // Disable the radial selection on start
            weaponSelectionParent.gameObject.SetActive(false);
        }
        public int GetSelectedIndex() => _selectedIndex;
        public void InitializeRadialParts(List<Player.Weapon.WeaponSO> weapons, int initialSelectedIndex) {
            _numberOfOptions = weapons.Count;
            for (var i = 0; i < _numberOfOptions; i++) {
                // Get the angle for each option, half the angle between selections
                // because we want to center the selection and the angle is on both sides
                var angle = i * 360f / _numberOfOptions - angleBetweenSelections * 0.5f;
                var radialPartEulerAngle = new Vector3(0, 0, angle);

                // Spawn one radial part
                var spawnedRadialPart = Instantiate(radialSelectionPrefab, radialSelectionsParent);
                spawnedRadialPart.transform.localEulerAngles = radialPartEulerAngle;

                // Set the "Size" of the radial part
                var radialPart = new RadialPart {
                    RadialImage = spawnedRadialPart.radialImage,
                    IconParentRect = spawnedRadialPart.centerRect,
                    Icon = spawnedRadialPart.iconImage
                };
                
                // Slice the image
                radialPart.RadialImage.fillAmount = 1f / _numberOfOptions - angleBetweenSelections / 360f;

                // Icon
                {
                    // Rotate the Icon around the center to match the middle of the slice
                    float halfFillAmountAngle =
                        radialPart.RadialImage.fillAmount * 180f; // 360 degrees * fillAmount / 2
                    radialPart.IconParentRect.localEulerAngles = new Vector3(0, 0, -halfFillAmountAngle);
                    spawnedRadialPart.iconImage.transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                    radialPart.Icon.sprite = weapons[i].weaponSprite;
                }

                // Store weapon information that radial part needs for display
                {
                    radialPart.WeaponName = weapons[i].weaponName;
                    radialPart.WeaponDescription = weapons[i].weaponDescription;
                }
                
                _radialParts.Add(radialPart);
            }
            
            // Set the initial selected index
            _selectedIndex = initialSelectedIndex;
        }
        
        public void EnableRadialSelection(bool enabled) => weaponSelectionParent.gameObject.SetActive(enabled);

        public void SelectRadialPart(Vector2 inputPosition, bool isMouseInput) {
            Vector2 localPosition;
            
            if (isMouseInput) {
                // Mouse: InverseTransformPoint to get the local position
                localPosition = radialSelectionsParent.InverseTransformPoint(inputPosition);
            } else {
                // For controller input, we normalize to get the direction
                localPosition = inputPosition.normalized;
            }

            // Angle in degrees from the position input of the player
            var angle = Mathf.Atan2(localPosition.y, localPosition.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f; // Don't want negative angles
            
            // Determine the selected index
            if (_numberOfOptions == 2) {
                // Edge Case if 2 Options
                _selectedIndex = angle is > 90 and < 270 
                    ? 1 
                    : 0;
            } else {
                // More than 2 Options
                // FloorToInt because we want to select the closest option
                _selectedIndex = Mathf.FloorToInt(angle / (360f / _numberOfOptions));
            }
            
            // Color the selected part
            for (int i = 0; i < _radialParts.Count; i++) {
                _radialParts[i].RadialImage.color = i == _selectedIndex 
                    ? Color.yellow 
                    : Color.white;
            }

            // Display the weapon information
            {
                weaponHeaderText.text = _radialParts[_selectedIndex].WeaponName;
                weaponDescriptionText.text = _radialParts[_selectedIndex].WeaponDescription;
            }
        }
        
        
        class RadialPart {
            public Image RadialImage;
            public RectTransform IconParentRect;
            public Image Icon;
            public string WeaponName;
            public string WeaponDescription;
        }
    }
}
