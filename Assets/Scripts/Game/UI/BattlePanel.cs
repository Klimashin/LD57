using System;
using System.Collections.Generic;
using BrunoMikoski.AnimationSequencer;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Core;
using Game.Data;
using Game.Gameplay;
using Reflex.Attributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.UI
{
    public class BattlePanel : MonoBehaviour
    {
        [SerializeField] private Transform _characterTransform;
        [SerializeField] private Transform _monsterTransform;
        [SerializeField] private float _delayBetweenTurns = 0.5f;
        [SerializeField] private float _battleStartDelay = 1f;
        [SerializeField] private float _battleEndDelay = 1f;
        [SerializeField] private RectTransform _panelRect;
        [SerializeField] private AnimationSequencerController _battleStartupAnimation;
        [SerializeField] private List<AudioClip> _hitSfx;

        private MonsterData _monsterData;
        private IGameController _gameController;
        private BattleUnit _characterModel;
        private BattleUnit _monsterModel;
        private SoundManager _soundManager;

        [Inject]
        private void Inject(SoundManager soundManager)
        {
            _soundManager = soundManager;
        }

        public async UniTask RunBattle(MonsterData monster, IGameController gameController)
        {
            _monsterData = monster;
            _gameController = gameController;
            
            Setup();

            gameObject.SetActive(true);
            
            _panelRect.localScale = Vector3.zero;
            await _panelRect.DOScale(Vector3.one, 0.5f).ToUniTask();

            await Battle();
            
            gameObject.SetActive(false);
        }

        private async UniTask Battle()
        {
            _battleStartupAnimation.gameObject.SetActive(true);
            
            await UniTask.Delay(TimeSpan.FromSeconds(_battleStartDelay));
            
            _battleStartupAnimation.gameObject.SetActive(false);
            
            bool charactersTurn = true;
            while (_characterModel.IsAlive && _monsterModel.IsAlive)
            {
                if (charactersTurn)
                {
                    await _characterModel.AttackRoutine(damage =>
                    {
                        int sfxIndex = Random.Range(0, _hitSfx.Count);
                        _soundManager.PlayOneShot(_hitSfx[sfxIndex]);
                        _monsterModel.TakeDamage(damage);
                    });
                }
                else
                {
                    await _monsterModel.AttackRoutine(damage =>
                    {
                        int sfxIndex = Random.Range(0, _hitSfx.Count);
                        _soundManager.PlayOneShot(_hitSfx[sfxIndex]);
                        _characterModel.TakeDamage(damage);
                    });
                }

                await UniTask.Delay(TimeSpan.FromSeconds(_delayBetweenTurns));

                charactersTurn = !charactersTurn;
            }

            if (_characterModel.IsAlive)
            {
                _monsterModel.FadeOut();
            }
            else
            {
                _characterModel.FadeOut();
            }
            
            await UniTask.Delay(TimeSpan.FromSeconds(_battleEndDelay));

            int remainingCharHp = _characterModel.Hp;
            _gameController.SetResource(PlayerResources.Health, remainingCharHp);
            
            await _panelRect.DOScale(Vector3.zero, 0.5f).ToUniTask();
        }

        private void Setup()
        {
            CleanUp();

            var characterUnit = Instantiate(_gameController.CharacterBattlePrefab, _characterTransform);
            var monsterUnit = Instantiate(_monsterData.Prefab, _monsterTransform);
            
            characterUnit.Initialize(_gameController.GetResource(PlayerResources.Health), _gameController.GetResource(PlayerResources.Attack), false);
            monsterUnit.Initialize(_monsterData.Hp, _monsterData.Attack, true);
            
            _characterModel = characterUnit;
            _monsterModel = monsterUnit;
        }

        private void CleanUp()
        {
            if (_characterTransform.childCount > 0)
            {
                Destroy(_characterTransform.GetChild(0).gameObject);
            }
            
            if (_monsterTransform.childCount > 0)
            {
                Destroy(_monsterTransform.GetChild(0).gameObject);
            }
        }
    }
}
