using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Data;
using Game.Gameplay.Events;
using Game.UI;
using JetBrains.Annotations;
using Reflex.Attributes;
using Reflex.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Game.Gameplay
{
    public class GameController : MonoBehaviour, IGameController
    {
        [SerializeField] private Transform _tilesTransform;
        [SerializeField] private GameObject _menuPanel;
        [SerializeField] private List<GameStage> _stages;
        [SerializeField] private GameObject _characterMarkerPrefab;
        [SerializeField] private BattleUnit _characterBattlePrefab;
        [SerializeField] private float _characterMoveDuration = 0.5f;
        [SerializeField] private GameObject _moveMarkerPrefab;
        [SerializeField] private CharacterBaseParams _characterBaseParams = new ();
        [SerializeField] private List<AudioSource> _gameplayTracks;
        [SerializeField, TextArea] private string _loseDescription;
        [SerializeField, TextArea] private string _winDescription;
        
        public int Energy { get; private set; }
        public int Hp { get; private set; }
        public int Attack { get; private set; }
        public UnityEvent OnWin { get; } = new();
        public UnityEvent OnLose { get; } = new();
        public Vector2Int CharacterPosition { get; private set; }
        public Vector2Int FieldSize => _currentStage.Size;
        public BattleUnit CharacterBattlePrefab => _characterBattlePrefab;
        public GameState State { get; private set; } = GameState.AwaitingInput;
        public FieldTileController CurrentHoveredTile { get; private set; }

        private readonly Dictionary<Vector2Int, FieldTileController> _tiles = new ();
        private GameObject _characterMarker;
        private BattlePanel _battlePanel;
        private QuestPanel _questPanel;
        private ResIncomePanel _resIncomePanel;
        private GameObject _moveMarker;
        private LoadingOverlay _loadingOverlay;
        private int _currentStageIndex = -1;
        private GameStage _currentStage;
        private Camera _camera;

        [Inject]
        private void Inject(BattlePanel battlePanel, QuestPanel questPanel, LoadingOverlay loadingOverlay, Camera camera, ResIncomePanel resIncomePanel)
        {
            _battlePanel = battlePanel;
            _questPanel = questPanel;
            _loadingOverlay = loadingOverlay;
            _camera = camera;
            _resIncomePanel = resIncomePanel;
        }
        
        private void Start()
        {
            ProgressToNextStage().Forget();
        }

        private async UniTask ProgressToNextStage()
        {
            State = GameState.Loading;
            
            bool isFirstStage = _currentStageIndex == -1;
            if (!isFirstStage)
            {
                await _loadingOverlay.ShowOverlay();
                CleanupField();
            }

            _currentStageIndex++;
            _currentStage = _stages[_currentStageIndex];
            _gameplayTracks[_currentStageIndex].mute = false;

            CameraSetup();
            GenerateTiles();
            GenerateEvents();
            InitializeCharacter();

            await _loadingOverlay.HideOverlay();
            
            State = GameState.AwaitingInput;
        }

        private void CameraSetup()
        {
            var camTransform = _camera.transform;
            camTransform.position = new Vector3(_currentStage.CameraConfig.Position.x,
                _currentStage.CameraConfig.Position.y, camTransform.position.z);
            _camera.orthographicSize = _currentStage.CameraConfig.Size;
        }

        private void CleanupField()
        {
            _tiles.Clear();
            int fieldChildCount = _tilesTransform.childCount;
            for (int i = fieldChildCount - 1; i >= 0; i--)
            {
                Destroy(_tilesTransform.GetChild(i).gameObject);
            }
        }

        private void Update()
        {
            HandleHover();
            
            switch (State)
            {
                case GameState.Menu when Input.GetKeyDown(KeyCode.Escape):
                    State = GameState.AwaitingInput;
                    _menuPanel.gameObject.SetActive(false);
                    return;
                case GameState.AwaitingInput when Input.GetKeyDown(KeyCode.Escape):
                    State = GameState.Menu;
                    _menuPanel.gameObject.SetActive(true);
                    return;
            }
            
            if (State == GameState.AwaitingInput)
            {
                HandlePlayerInput();
            }
        }

        private void HandleHover()
        {
            if (State == GameState.Loading)
            {
                return;
            }

            Vector2 mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);

            if (State == GameState.AwaitingInput && hit != null && hit.TryGetComponent<FieldTileController>(out var hitTile))
            {
                CurrentHoveredTile = hitTile;
                if (ValidateMoveCoords(CurrentHoveredTile.Coords))
                {
                    _moveMarker.gameObject.SetActive(true);
                    SetMoveMarkerPosition(CharacterPosition, CurrentHoveredTile.Coords);
                }
                else
                {
                    _moveMarker.gameObject.SetActive(false);
                }
            }
            else
            {
                CurrentHoveredTile = null;
                _moveMarker.gameObject.SetActive(false);
            }
        }

        private void HandlePlayerInput()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && CurrentHoveredTile != null && ValidateMoveCoords(CurrentHoveredTile.Coords))
            {
                HandleTurn(CurrentHoveredTile.Coords).Forget();
                return;
            }
            
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                var newCharCoords = CharacterPosition + new Vector2Int(-1, 0);
                if (ValidateMoveCoords(newCharCoords))
                {
                    HandleTurn(newCharCoords).Forget();
                }
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                var newCharCoords = CharacterPosition + new Vector2Int(1, 0);
                if (ValidateMoveCoords(newCharCoords))
                {
                    HandleTurn(newCharCoords).Forget();
                }
            } 
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                var newCharCoords = CharacterPosition + new Vector2Int(0, 1);
                if (ValidateMoveCoords(newCharCoords))
                {
                    HandleTurn(newCharCoords).Forget();
                }
            } 
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                var newCharCoords = CharacterPosition + new Vector2Int(0, -1);
                if (ValidateMoveCoords(newCharCoords))
                {
                    HandleTurn(newCharCoords).Forget();
                }
            }
        }

        private async UniTask HandleTurn(Vector2Int newCoords)
        {
            if (!ValidateCharacterCoords(newCoords))
            {
                Debug.LogError("Character coords are out of the map range");
                return;
            }

            State = GameState.ProcessingTurn;
            
            Debug.Log($"MOVING CHARACTER TO ({newCoords.x.ToString()}, {newCoords.y.ToString()}) ({_tiles[CharacterPosition].gameObject.name})");

            CharacterPosition = newCoords;
            await _characterMarker.transform.DOMove(_tiles[CharacterPosition].CharacterPosition, _characterMoveDuration).ToUniTask();

            await HandleTileEntrance(_tiles[CharacterPosition]);

            Energy -= _tiles[CharacterPosition].Cost;
            Debug.Log($"NEW ENERGY LEVEL: {Energy.ToString()}");

            if (Energy < 0)
            {
                var damage = -Energy;
                Energy = 0;
                Hp -= damage;
            }
            
            if (CheckWinCondition())
            {
                await OnStageCompleted();
            }
            else if (CheckLoseCondition())
            {
                await OnGameLost();
            }
            else
            {
                State = GameState.AwaitingInput;
            }
        }

        private bool CheckLoseCondition()
        {
            return Hp <= 0;
        }
        
        private bool CheckWinCondition()
        {
            return CharacterPosition == _currentStage.GoalTile;
        }

        private async UniTask OnGameLost()
        {
            State = GameState.Lost;
            
            await _resIncomePanel.Show(new List<ResourceEvent.ResourceChange>(), _loseDescription);
            
            OnLose.Invoke();
        }
        
        private async UniTask OnStageCompleted()
        {
            var stageGoalTile = _tiles[CharacterPosition];
            if (stageGoalTile.TryGetComponent<GoalTileController>(out var goalTile))
            {
                await goalTile.PlayAnimation(_characterMarker);
            }
            else if (stageGoalTile.TryGetComponent<EndGameTileController>(out var endGameTile))
            {
                await endGameTile.PlayAnimation();
            }
            
            if (_currentStageIndex >= _stages.Count - 1)
            {
                await _resIncomePanel.Show(new List<ResourceEvent.ResourceChange>(), _winDescription);
                await _loadingOverlay.ShowOverlay();
                var scene = SceneManager.LoadScene(2, new LoadSceneParameters(LoadSceneMode.Single));
                ReflexSceneManager.PreInstallScene(scene, builder => {});
            }
            else
            {
                await ProgressToNextStage();
            }
        }

        private async UniTask HandleTileEntrance(FieldTileController tile)
        {
            if (tile.TileEvent == null)
            {
                return;
            }

            var tileEvent = tile.TileEvent;
            tile.ConsumeTileEvent();
            await tileEvent.HandleEvent(this);
        }

        private bool ValidateCharacterCoords(Vector2Int characterCoords)
        {
            return characterCoords.x >= 0 && characterCoords.x < _currentStage.Size.x 
                    && characterCoords.y >= 0 && characterCoords.y < _currentStage.Size.y;
        }
        
        private bool ValidateMoveCoords(Vector2Int moveCoords)
        {
            bool insideMap = moveCoords.x >= 0
                   && moveCoords.x < _currentStage.Size.x
                   && moveCoords.y >= 0
                   && moveCoords.y < _currentStage.Size.y;

            bool validMove = (Mathf.Abs(moveCoords.x - CharacterPosition.x) == 1 && moveCoords.y == CharacterPosition.y)
                             || (Mathf.Abs(moveCoords.y - CharacterPosition.y) == 1 &&
                                 moveCoords.x == CharacterPosition.x);

            return insideMap && validMove && _tiles[moveCoords].Cost >= 0;
        }

        private void InitializeCharacter()
        {
            CharacterPosition = _currentStage.CharacterInitialTile;
            
            _characterMarker = Instantiate(_characterMarkerPrefab, _tilesTransform);
            _characterMarker.transform.position = _tiles[CharacterPosition].CharacterPosition;
            _moveMarker = Instantiate(_moveMarkerPrefab, _tilesTransform);
            _moveMarker.gameObject.SetActive(false);

            if (_currentStageIndex == 0)
            {
                Energy = _characterBaseParams.Energy;
                Hp = _characterBaseParams.Hp;
                Attack = _characterBaseParams.Attack;
            }
        }

        private void SetMoveMarkerPosition(Vector2Int from, Vector2Int to)
        {
            float xPoos = from.x + (to.x - from.x)/2f;
            float yPos = 0.5f + from.y + (to.y - from.y)/2f;
            _moveMarker.transform.position = new (xPoos, yPos, 0f);

            float moveMarkerRotation = 0f;
            if (to.y > from.y)
            {
                moveMarkerRotation = 0f;
            }
            else if (to.y < from.y)
            {
                moveMarkerRotation = 180f;
            }
            else if (to.x < from.x)
            {
                moveMarkerRotation = 90f;
            }
            else if (to.x > from.x)
            {
                moveMarkerRotation = -90f;
            }

            _moveMarker.transform.eulerAngles = new Vector3(0f, 0f, moveMarkerRotation);
        }

        private void GenerateEvents()
        {
            IEnumerable<Vector2Int> filteredTiles =
                _tiles.Keys.Where(tileCoord => tileCoord != _currentStage.CharacterInitialTile && tileCoord != _currentStage.GoalTile && _tiles[tileCoord].Cost >= 0);

            foreach (var tile in filteredTiles)
            {
                List<EventConfig> filteredEvents = _currentStage.Events.Where(e => e.Event.ValidateForTile(this, tile)).ToList();
                var eventConfig = SelectEvent(filteredEvents, _currentStage.TileWithoutEventSpawnRate);
                if (eventConfig != null)
                {
                    _tiles[tile].BindTileEvent(eventConfig.Event);
                }
            }
        }

        [CanBeNull]
        private EventConfig SelectEvent(List<EventConfig> eventsSet, int emptyEventChance)
        {
            int totalChance = eventsSet.Select(eventConfig => eventConfig.SpawnRate).Sum() + emptyEventChance;
            int cumulativeChance = emptyEventChance;
            int roll = Random.Range(0, totalChance);
            if (roll < cumulativeChance)
            {
                return null;
            }
            
            for (int i = 0; i < eventsSet.Count; i++)
            {
                cumulativeChance += eventsSet[i].SpawnRate;
                if (roll < cumulativeChance)
                {
                    return eventsSet[i];
                }
            }

            throw new Exception("Failed to select event based on roll");
        }

        private void GenerateTiles()
        {
            int totalChance = _currentStage.Tiles.Select(tileConfig => tileConfig.SpawnRate).Sum();

            for (int i = 0; i < _currentStage.Size.x; i++)
            {
                for (int j = 0; j < _currentStage.Size.y; j++)
                {
                    var tileCoord = new Vector2Int(i, j);
                    TileConfig tileConfig;
                    if (tileCoord.x == _currentStage.GoalTile.x && tileCoord.y == _currentStage.GoalTile.y)
                    {
                        tileConfig = _currentStage.GoalTileConfig;
                    }
                    else
                    {
                        TileConfig selection;
                        do
                        {
                            selection = SelectTile(_currentStage.Tiles, totalChance);
                        } while (!ValidateSelection(selection, tileCoord));

                        tileConfig = selection;
                    }

                    var tile = Instantiate(tileConfig.TilePrefab, _tilesTransform);
                    tile.gameObject.name = $"{tile.gameObject.name}_{tileCoord.x.ToString()}_{tileCoord.y.ToString()}";
                    tile.transform.position = new Vector3(tileCoord.x, tileCoord.y, 0f);
                    tile.Setup(tileCoord);
                    _tiles.Add(tileCoord, tile);
                }
            }
        }

        private bool ValidateSelection(TileConfig tileConfig, Vector2Int tileCoords)
        {
            if (tileConfig.TilePrefab.Cost < 0)
            {
                return tileCoords.x > 0 && tileCoords.y > 0 && tileCoords.x < _currentStage.Size.x - 1 &&
                       tileCoords.y < _currentStage.Size.y - 1;
            }

            return true;
        }

        private TileConfig SelectTile(List<TileConfig> tilesSet, int totalChance)
        {
            int roll = Random.Range(0, totalChance);
            int cumulativeChance = 0;
            for (int i = 0; i < tilesSet.Count; i++)
            {
                cumulativeChance += tilesSet[i].SpawnRate;
                if (roll < cumulativeChance)
                {
                    return tilesSet[i];
                }
            }

            throw new Exception("Failed to select tile based on roll");
        }

        public async UniTask HandleResourceChange(List<ResourceEvent.ResourceChange> resourceChanges, string description)
        {
            await _resIncomePanel.Show(resourceChanges, description);

            foreach (var resourceChange in resourceChanges)
            {
                AddResource(resourceChange.Type, resourceChange.Amount);
            }
        }

        public void AddResource(PlayerResources resType, int resCount)
        {
            switch (resType)
            {
                case PlayerResources.Energy:
                    Energy += resCount;
                    return;
                    
                case PlayerResources.Health:
                    Hp += resCount;
                    return;
                
                case PlayerResources.Attack:
                    Attack += resCount;
                    return;
                
                default:
                    throw new Exception("Unknown res type added");
            }
        }

        public int GetResource(PlayerResources resType)
        {
            switch (resType)
            {
                case PlayerResources.Energy:
                    return Energy;

                case PlayerResources.Health:
                    return Hp;
                
                case PlayerResources.Attack:
                    return Attack;
                
                default:
                    throw new Exception("Unknown res type added");
            }
        }
        
        public void SetResource(PlayerResources resType, int resCount)
        {
            switch (resType)
            {
                case PlayerResources.Energy:
                    Energy = resCount;
                    return;
                    
                case PlayerResources.Health:
                    Hp = resCount;
                    return;
                
                case PlayerResources.Attack:
                    Attack = resCount;
                    return;
                
                default:
                    throw new Exception("Unknown res type set");
            }
        }

        public async UniTask ShuffleTiles()
        {
            var unblockedTiles = _tiles.Where(pair => pair.Key != CharacterPosition && pair.Key != _currentStage.GoalTile).ToList();
            var tilePositions = unblockedTiles.Select(t => t.Key).ToList();
            var shuffledTiles = unblockedTiles.Select(t => t.Value).ToList();
            
            for (int i = shuffledTiles.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (shuffledTiles[i], shuffledTiles[j]) = (shuffledTiles[j], shuffledTiles[i]); // Swap tiles
            }

            List<UniTask> moveTasks = new();
            for (int i = 0; i < tilePositions.Count; i++)
            {
                var tile = shuffledTiles[i];
                var position = tilePositions[i];
                _tiles[position] = tile;
                var moveTask = tile.transform.DOMove(new Vector3(position.x, position.y, 0f), 1f).ToUniTask();
                moveTasks.Add(moveTask);
            }

            await UniTask.WhenAll(moveTasks);
        }

        public async UniTask RotateTiles(Vector2Int center, bool clockwise)
        {
            if (!(center.x > 0 && center.y > 0 && center.x < FieldSize.x - 1 && center.y < FieldSize.y - 1))
            {
                return;
            }

            List<Vector2Int> oldPositions = new List<Vector2Int>
            {
                new (center.x - 1, center.y - 1), // BL
                new (center.x,     center.y - 1), // BM
                new (center.x + 1, center.y - 1), //BR
                new (center.x + 1, center.y), // MR
                new (center.x + 1, center.y + 1), //TR
                new (center.x,     center.y + 1), //TM
                new (center.x - 1, center.y + 1), //TL
                new (center.x - 1, center.y), //ML
            };

            List<Vector2Int> newPositions = new();
            for (int i = 0; i < oldPositions.Count; i++)
            {
                int fromIndex = clockwise ? (i + 7) % oldPositions.Count : (i + 1) % oldPositions.Count;
                newPositions.Add(oldPositions[fromIndex]);
            }
            
            List<FieldTileController> affectedTiles = new();
            foreach (var t in oldPositions)
            {
                affectedTiles.Add(_tiles[t]);
            }
            
            List<UniTask> moveTasks = new();
            for (int i = 0; i < affectedTiles.Count; i++)
            {
                var tile = affectedTiles[i];
                var newPosition = newPositions[i];
                _tiles[newPosition] = tile;
                var moveTask = tile.transform.DOMove(new Vector3(newPosition.x, newPosition.y, 0f), 1f).ToUniTask();
                moveTasks.Add(moveTask);
            }

            await UniTask.WhenAll(moveTasks);
        }

        public async UniTask Battle(MonsterData monster)
        {
            await _battlePanel.RunBattle(monster, this);
        }

        public async UniTask Quest(Quest quest)
        {
            var resultChoice = await _questPanel.RunQuest(quest, this);
            if (resultChoice.ChoiceEvent != null)
            {
                await resultChoice.ChoiceEvent.HandleEvent(this);
            }
        }
    }

    [Serializable]
    public class CharacterBaseParams
    {
        public int Energy = 20;
        public int Hp = 20;
        public int Attack = 2;
    }

    public enum GameState
    {
        AwaitingInput = 0,
        ProcessingTurn = 1,
        Win = 2,
        Lost = 3,
        Loading = 4,
        Menu = 5
    }

    public enum PlayerResources
    {
        Energy,
        Health,
        Attack
    }

    public interface IGameController
    {
        public int GetResource(PlayerResources type);
        public Vector2Int FieldSize { get; }
        public Vector2Int CharacterPosition { get; }
        public UniTask HandleResourceChange(List<ResourceEvent.ResourceChange> resourceChanges, string description);
        public void SetResource(PlayerResources resType, int resCount);
        public UniTask ShuffleTiles();
        public UniTask RotateTiles(Vector2Int center, bool clockwise);
        public BattleUnit CharacterBattlePrefab { get; }
        public UniTask Battle(MonsterData monster);
        public UniTask Quest(Quest quest);
    }
}
