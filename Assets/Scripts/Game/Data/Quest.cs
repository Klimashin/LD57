using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Quests/Quest")]
    public class Quest : ScriptableObject
    {
        public string QuestName;
        public QuestStep StartingStep;
    }
}
