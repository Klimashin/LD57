using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class BattleUnit : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _hp;
        [SerializeField] private TextMeshProUGUI _attack;

        private float _attackAnimationDuration = 1f;
        
        public int Hp { get; private set; }
        public int Attack { get; private set; }
        public bool IsAlive => Hp > 0;
        public bool IsEnemy { get; private set; }

        public void Initialize(int hp, int attack, bool isEnemy)
        {
            Hp = hp;
            Attack = attack;
            IsEnemy = isEnemy;

            _hp.text = Hp.ToString();
            _attack.text = Attack.ToString();
        }

        public async UniTask AttackRoutine(Action<int> damageCallback)
        {
            var animationSeq = DOTween.Sequence().SetAutoKill(true);

            animationSeq
                .Append(_image.rectTransform.DOMoveX(transform.position.x + (IsEnemy ? -200f : 200f), _attackAnimationDuration / 2f))
                .Join(_image.rectTransform.DORotate(IsEnemy ? new Vector3(0f, 0f, 20f) : new Vector3(0f, 0f, -20f), _attackAnimationDuration / 2f))
                .AppendCallback(() => damageCallback.Invoke(Attack))
                .SetLoops(2, LoopType.Yoyo);

            await animationSeq.ToUniTask();
            
            animationSeq.Kill();
        }

        public void TakeDamage(int damage)
        {
            Hp -= damage;
            Hp = Mathf.Max(0, Hp);
            _hp.text = Hp.ToString();
            _image.rectTransform.DOShakePosition(0.3f, Vector3.one * 15f, 10);
        }

        public void FadeOut()
        {
            _image.DOFade(0f, 0.5f);
        }
    }
}
