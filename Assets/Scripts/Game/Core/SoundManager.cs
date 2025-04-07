using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Core
{
    public class SoundManager
    {
        private readonly AudioMixer _audioMixer;
        private readonly string _volumeVariableName;
        private readonly string _musicGroupName;
        private readonly string _sfxGroupName;
        private readonly IReadOnlyDictionary<string, AudioClip> _audioDictionary;

        public SoundManager(
            AudioMixer audioMixer,
            IReadOnlyDictionary<string, AudioClip> audioDictionary,
            string volumeVariableName = "MasterVolume",
            string musicGroupName = "Music",
            string sfxGroupName = "Sfx"
        ) {
            _audioMixer = audioMixer;
            _volumeVariableName = volumeVariableName;
            _audioDictionary = audioDictionary;
            _musicGroupName = musicGroupName;
            _sfxGroupName = sfxGroupName;
        }

        private float Volume { get; set; } = 1;

        public void SetVolume(float v)
        {
            Volume = v;
            var volumeLvl = v > float.Epsilon ? 20 * Mathf.Log10(Volume) : -144f;
            _audioMixer.SetFloat(_volumeVariableName, volumeLvl);
        }

        public float GetVolume()
        {
            return Volume;
        }

        public bool IsPlayingClip(string clipName)
        {
            if (_musicAudio == null)
            {
                return false;
            }

            return _audioDictionary.TryGetValue(clipName, out var clip) && _musicAudio.clip == clip;
        }
    
        private AudioSource _musicAudio;
        public void PlayMusicClip(string clipName, float fadeTime = 0.5f)
        {
            if (!_audioDictionary.TryGetValue(clipName, out var clip))
            {
                Debug.LogError($"Missing clip with name {clipName}");
            }

            if (_musicAudio == null)
            {
                _musicAudio = (new GameObject()).AddComponent<AudioSource>();
                Object.DontDestroyOnLoad(_musicAudio);
                _musicAudio.outputAudioMixerGroup = _audioMixer.FindMatchingGroups(_musicGroupName)[0];
            }

            _musicAudio.DOFade(0f, 0.5f)
                .OnComplete(() =>
                {
                    _musicAudio.clip = clip;
                    _musicAudio.loop = true;
                    _musicAudio.Play();

                    _musicAudio.DOFade(1f, fadeTime);
                });
        }

        public void FadeCurrentMusic(float fadeTime)
        {
            if (_musicAudio == null)
            {
                return;
            }

            _musicAudio.DOFade(0f, fadeTime);
        }

        private readonly List<AudioSource> _oneShotAudio = new ();
        public void PlayOneShot(AudioClip clip, float pitch = 1f)
        {
            var oneShotAudio = _oneShotAudio.Find(source => source.isPlaying == false);
            if (oneShotAudio == null)
            {
                oneShotAudio = (new GameObject()).AddComponent<AudioSource>();
                Object.DontDestroyOnLoad(oneShotAudio);
                oneShotAudio.outputAudioMixerGroup = _audioMixer.FindMatchingGroups(_sfxGroupName)[0];
                _oneShotAudio.Add(oneShotAudio);
            }

            oneShotAudio.pitch = pitch;
            oneShotAudio.PlayOneShot(clip, 1f);
        }
    }
}
