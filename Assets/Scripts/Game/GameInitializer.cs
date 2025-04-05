using System;
using Game.Core;
using Game.Utils;
using Reflex.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;

namespace Game
{
    public class GameInitializer : MonoBehaviour, IInstaller
    {
        [SerializeField] private ScenesDictionary _scenes;
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private AudioDictionary _audioClips;
    
        public void InstallBindings(ContainerBuilder containerBuilder)
        {
            var sceneManager = new SceneManager(_scenes);
            containerBuilder.AddSingleton(sceneManager);
            var soundManager = new SoundManager(_audioMixer, _audioClips);
            containerBuilder.AddSingleton(soundManager);
        }
    }
    
    [Serializable] public class ScenesDictionary : UnitySerializedDictionary<string, AssetReference> {}
    [Serializable] public class AudioDictionary : UnitySerializedDictionary<string, AudioClip> {}
}
