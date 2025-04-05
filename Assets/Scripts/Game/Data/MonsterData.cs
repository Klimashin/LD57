using Game.UI;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu]
    public class MonsterData : ScriptableObject
    {
        [SerializeField] private int _hp;
        [SerializeField] private int _attack;
        [SerializeField] private BattleUnit _prefab;

        public int Hp => _hp;
        public int Attack => _attack;
        public BattleUnit Prefab => _prefab;
    }
}
