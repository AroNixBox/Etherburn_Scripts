using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ShaderControl {
    public class DissolveControl : MonoBehaviour {
        [SerializeField] AnimationCurve dissolveCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] float dissolveDuration = 1f;
        [SerializeField] DissolveMode startMode = DissolveMode.Dissolve;
        [SerializeField] List<Material> materials;

        DissolveMode _currentMode;
        static readonly int DissolveAmount = Shader.PropertyToID("_DissolveAmount");

        [Button]
        void SaveMaterials() {
            materials.Clear();
            if (TryGetComponent(out MeshRenderer ownRenderer)) {
                materials.AddRange(ownRenderer.sharedMaterials);
            }
        }

        [Button]
        void SaveChildMaterials() {
            materials.Clear();
            var renderers = GetComponentsInChildren<MeshRenderer>();
            foreach (var childRenderer in renderers) {
                materials.AddRange(childRenderer.sharedMaterials);
            }
        }

        void Start() {
            SetDissolveMode(startMode);
        }

        void OnValidate() {
            if(Application.isPlaying) { return; }
            
            SetDissolveMode(startMode);
        }

        void SetDissolveMode(DissolveMode dissolveMode) {
            _currentMode = dissolveMode;
            var startAmount = GetDissolveMode(_currentMode);
            foreach (var material in materials) {
                if (material != null) {
                    material.SetFloat(DissolveAmount, startAmount);
                }
            }
        }

        [Button(ButtonSizes.Gigantic)]
        public void ChangeDissolveMode() {
            var dissolveMode = _currentMode == DissolveMode.Dissolve ? DissolveMode.Materialize : DissolveMode.Dissolve;
            _ = LerpDissolve(dissolveMode);
        }

        public async Task ChangeDissolveMode(DissolveMode dissolveMode) {
            if (_currentMode == dissolveMode) { return; }
            await LerpDissolve(dissolveMode);
        }

        async Task LerpDissolve(DissolveMode dissolveMode) {
            var startAmount = GetDissolveMode(_currentMode);
            var endAmount = GetDissolveMode(dissolveMode);
            var elapsedTime = 0f;

            while (elapsedTime < dissolveDuration) {
                elapsedTime += Time.deltaTime;
                var t = elapsedTime / dissolveDuration;
                var dissolveAmount = Mathf.Lerp(startAmount, endAmount, dissolveCurve.Evaluate(t));
                foreach (var material in materials) {
                    if (material != null) {
                        material.SetFloat(DissolveAmount, dissolveAmount);
                    }
                }
                await Task.Yield();
            }
            SetDissolveMode(dissolveMode);
        }

        int GetDissolveMode(DissolveMode dissolveMode) {
            return dissolveMode == DissolveMode.Dissolve ? 1 : 0;
        }

        public enum DissolveMode {
            Dissolve,
            Materialize
        }
    }
}