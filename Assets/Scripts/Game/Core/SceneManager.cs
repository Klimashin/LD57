using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Reflex.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Game.Core
{
    public class SceneManager
    {
        private readonly IReadOnlyDictionary<string, AssetReference> _scenes;

        private readonly Dictionary<string, SceneInstance> _loadedScenes = new ();

        public SceneManager(IReadOnlyDictionary<string, AssetReference> scenes)
        {
            _scenes = scenes;
        }

        public async UniTask LoadScene(string name)
        {
            var sceneAsset = _scenes[name];
            var handle = Addressables.LoadSceneAsync(sceneAsset, LoadSceneMode.Additive, false);
            await handle.ToUniTask();
            _loadedScenes.Add(name, handle.Result);
            ReflexSceneManager.PreInstallScene(handle.Result.Scene, builder => {});
            await handle.Result.ActivateAsync();
        }

        public async UniTask UnloadScene(string name)
        {
            if (!_loadedScenes.TryGetValue(name, out var sceneHandle))
            {
                Debug.LogError($"Scene {name} is not loaded.");
                return;
            }
            
            AsyncOperationHandle<SceneInstance> handle = Addressables.UnloadSceneAsync(sceneHandle);
            await handle.ToUniTask();
        }
    }
}
