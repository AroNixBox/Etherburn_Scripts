using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace UI.Menu {
    public class AudioSettings : MonoBehaviour{
        [SerializeField] AudioMixer audioMixer;
        [SerializeField] Slider volumeSlider;
        [SerializeField] MixerGroupParameter mixerGroupParameter;
        
        const float MinVolume = 0.0001f;
        const float MaxVolume = 1f;

        void Start() {
            // Make sure minvalue is not 0
            volumeSlider.minValue = MinVolume;
            volumeSlider.maxValue = MaxVolume;

            // Get the current volume from the audio mixer
            audioMixer.GetFloat(_mixerGroupParameters[mixerGroupParameter], out var currentVolume);
            // Convert the volume from a logarithmic scale to a linear scale
            var linearVolume = Mathf.Pow(10, currentVolume / 20);
            volumeSlider.value = linearVolume;

            // If the volume is approximately MinVolume, set it to MaxVolume
            if (Mathf.Approximately(linearVolume, MinVolume)) {
                volumeSlider.value = MaxVolume;
            }

            volumeSlider.onValueChanged.AddListener(SetVolume);

            SetVolume(volumeSlider.value);
        }

        void SetVolume(float volume) {
            // Get the string parameter name from the dictionary
            var volumeParameter = _mixerGroupParameters[mixerGroupParameter];
            // Clamp the volume to a minimum value to avoid large negative values
            var clampedVolume = Mathf.Clamp(volume, 0.0001f, 1f);
            // Convert the volume to a logarithmic scale
            var volumeValue = Mathf.Log10(clampedVolume) * 20;
            audioMixer.SetFloat(volumeParameter, volumeValue);
        }

        enum MixerGroupParameter {
            None,
            Master,
            Sfx,
            Music
        }

        readonly Dictionary<MixerGroupParameter, string> _mixerGroupParameters = new() {
            {MixerGroupParameter.Master, "Master_Volume"},
            {MixerGroupParameter.Sfx, "Sfx_Volume"},
            {MixerGroupParameter.Music, "Music_Volume"}
        };
    }
}
