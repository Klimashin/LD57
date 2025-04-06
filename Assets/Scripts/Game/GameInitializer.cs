using System;
using Game.Core;
using Game.Utils;
using Reflex.Core;
using UnityEngine;
using UnityEngine.Audio;

namespace Game
{
    public class GameInitializer : MonoBehaviour, IInstaller
    {
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private AudioDictionary _audioClips;
    
        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            var soundManager = new SoundManager(_audioMixer, _audioClips);
            containerBuilder.AddSingleton(soundManager);
        }
    }
    
    [Serializable] public class AudioDictionary : UnitySerializedDictionary<string, AudioClip> {}
}
