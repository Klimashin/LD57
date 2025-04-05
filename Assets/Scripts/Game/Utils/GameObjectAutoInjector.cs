using Reflex.Extensions;
using Reflex.Injectors;
using UnityEngine;

public class GameObjectAutoInjector : MonoBehaviour
{
    private void Awake()
    {
        GameObjectInjector.InjectObject(gameObject, gameObject.scene.GetSceneContainer());
    }
}
