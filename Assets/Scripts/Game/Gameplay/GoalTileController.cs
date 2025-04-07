using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Game.Gameplay
{
    public class GoalTileController : FieldTileController
    {
        [SerializeField] private Transform _liftTransform;
        [SerializeField] private float _liftAnimationDelay = 0.5f;
        [SerializeField] private float _liftAnimationDuration = 1.5f;
        
        public async UniTask PlayAnimation(GameObject character)
        {
            character.transform.SetParent(_liftTransform);

            await _liftTransform.DOLocalMoveY(-0.5f, _liftAnimationDuration).ToUniTask();
        }
    }
}
