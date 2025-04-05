using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Data;
using Game.Gameplay;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class QuestPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _stepDescription;
        [SerializeField] private QuestChoiceUI _choicePrefab;
        [SerializeField] private Transform _choicesTransform;
        [SerializeField] private RectTransform _panelRect;

        private IGameController _gameController;
        private Quest _quest;

        public async UniTask<QuestChoice> RunQuest(Quest quest, IGameController gameController)
        {
            _quest = quest;
            _gameController = gameController;
            
            CleanUp();

            gameObject.SetActive(true);
            
            _panelRect.localScale = Vector3.zero;
            await _panelRect.DOScale(Vector3.one, 0.5f).ToUniTask();
            
            var choice = await RunStep(quest.StartingStep);
            while (choice.NextSteps.Count > 0)
            {
                choice = await RunStep(choice.SelectNextStep());
            }
            
            await _panelRect.DOScale(Vector3.zero, 0.5f).ToUniTask();

            return choice;
        }
        
        private async UniTask<QuestChoice> RunStep(QuestStep step)
        {
            CleanUp();
            
            _stepDescription.text = step.StepDescription;

            QuestChoice selectedChoice = null;
            foreach (var questChoice in step.Choices)
            {
                var choiceUI = Instantiate(_choicePrefab, _choicesTransform);
                choiceUI.Setup(questChoice, choice => selectedChoice = choice);
            }

            while (selectedChoice == null)
            {
                await UniTask.DelayFrame(1);
            }

            return selectedChoice;
        }

        private void CleanUp()
        {
            _stepDescription.text = string.Empty;
            
            var count = _choicesTransform.childCount;
            for (int i = count - 1; i >= 0; i--)
            {
                Destroy(_choicesTransform.GetChild(i).gameObject);
            }
        }
    }
}
