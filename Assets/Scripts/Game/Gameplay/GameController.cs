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
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Game.Gameplay
{
    public class GameController : MonoBehaviour, IGameController
    {
        [SerializeField] private Transform _tilesTransform;
        [SerializeField] private FieldConfig _config;
        [SerializeField] private GameObject _characterMarkerPrefab;
        [SerializeField] private BattleUnit _characterBattlePrefab;
        [SerializeField] private float _characterMoveDuration = 0.5f;
        [SerializeField] private GameObject _questMarkerPrefab;
        [SerializeField] private GameObject _moveMarkerPrefab;
        
        public int Energy { get; private set; }
        public int Hp { get; private set; }
        public int Attack { get; private set; }
        public UnityEvent OnWin { get; private set; } = new();
        public UnityEvent OnLose { get; private set; } = new();
        public Vector2Int CharacterPosition { get; private set; }
        public Vector2Int FieldSize => _config.Size;
        public BattleUnit CharacterBattlePrefab => _characterBattlePrefab;
        public GameState State { get; private set; } = GameState.AwaitingInput;
        public FieldTileController CurrentHoveredTile { get; private set; }

        private readonly Dictionary<Vector2Int, FieldTileController> _tiles = new ();
        private GameObject _questMarker;
        private GameObject _characterMarker;
        private BattlePanel _battlePanel;
        private QuestPanel _questPanel;
        private GameObject _moveMarker;

        [Inject]
        private void Inject(BattlePanel battlePanel, QuestPanel questPanel)
        {
            _battlePanel = battlePanel;
            _questPanel = questPanel;
        }
        
        private void Start()
        {
            GenerateTiles();
            SetupGoals();
            GenerateEvents();
            InitializeCharacter();
        }

        private void Update()
        {
            HandleHover();
            
            if (State == GameState.AwaitingInput)
            {
                HandlePlayerInput();
            }
        }

        private void HandleHover()
        {
            if (Camera.main == null)
            {
                return;
            }

            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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
            await _characterMarker.transform.DOMove(CoordsIntoCharacterPosition(CharacterPosition), _characterMoveDuration).ToUniTask();

            await HandleTileEntrance(_tiles[CharacterPosition]);

            Energy -= _tiles[CharacterPosition].Config.Cost;
            Debug.Log($"NEW ENERGY LEVEL: {Energy.ToString()}");

            if (Energy < 0)
            {
                var damage = -Energy;
                Energy = 0;
                Hp -= damage;
            }

            if (CheckLoseCondition())
            {
                await OnGameLost();
            }
            else if (CheckWinCondition())
            {
                await OnGameWon();
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
            return CharacterPosition == _config.WinTile;
        }

        private async UniTask OnGameLost()
        {
            State = GameState.Lost;
            Debug.Log("GAME LOST");
            OnLose.Invoke();
        }
        
        private async UniTask OnGameWon()
        {
            State = GameState.Win;
            _questMarker.SetActive(false);
            Debug.Log("GAME WON");
            OnWin.Invoke();
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
            return characterCoords.x >= 0 && characterCoords.x < _config.Size.x 
                    && characterCoords.y >= 0 && characterCoords.y < _config.Size.y;
        }
        
        private bool ValidateMoveCoords(Vector2Int moveCoords)
        {
            bool insideMap = moveCoords.x >= 0
                   && moveCoords.x < _config.Size.x
                   && moveCoords.y >= 0
                   && moveCoords.y < _config.Size.y;

            bool validMove = (Mathf.Abs(moveCoords.x - CharacterPosition.x) == 1 && moveCoords.y == CharacterPosition.y)
                             || (Mathf.Abs(moveCoords.y - CharacterPosition.y) == 1 &&
                                 moveCoords.x == CharacterPosition.x);

            return insideMap && validMove;
        }

        private void InitializeCharacter()
        {
            _characterMarker = Instantiate(_characterMarkerPrefab);
            CharacterPosition = _config.CharacterInitialTile;
            _characterMarker.transform.position = CoordsIntoCharacterPosition(CharacterPosition);
            Energy = _config.CharacterBaseParams.Energy;
            Hp = _config.CharacterBaseParams.Hp;
            Attack = _config.CharacterBaseParams.Attack;
            _moveMarker = Instantiate(_moveMarkerPrefab);
            _moveMarker.gameObject.SetActive(false);
        }

        private Vector3 CoordsIntoCharacterPosition(Vector2Int coords)
        {
            return new (coords.x, coords.y + 0.3f, 0f);
        }
        
        private Vector3 CoordsIntoQuestMarkerPosition(Vector2Int coords)
        {
            return new (coords.x, coords.y + 0.3f, 0f);
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
                _tiles.Keys.Where(tileCoord => tileCoord != _config.CharacterInitialTile && tileCoord != _config.WinTile);

            foreach (var tile in filteredTiles)
            {
                List<EventConfig> filteredEvents = _config.Events.Where(e => e.Event.ValidateForTile(this, tile)).ToList();
                var eventConfig = SelectEvent(filteredEvents, _config.TileWithoutEventSpawnRate);
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
            int totalChance = _config.Tiles.Select(tileConfig => tileConfig.SpawnRate).Sum();
            
            for (int i = 0; i < _config.Size.x; i++)
            {
                for (int j = 0; j < _config.Size.y; j++)
                {
                    var tileCoord = new Vector2Int(i, j);
                    var tileConfig = SelectTile(_config.Tiles, totalChance);
                    var tile = Instantiate(tileConfig.TilePrefab);
                    SetupTile(tile, tileCoord, tileConfig);
                    _tiles.Add(tileCoord, tile);
                }
            }
        }

        private void SetupTile(FieldTileController tile, Vector2Int tileCoords, TileConfig tileConfig)
        {
            var tileTransform = tile.transform;
            tile.gameObject.name = $"{tile.gameObject.name}_{tileCoords.x.ToString()}_{tileCoords.y.ToString()}";
            tileTransform.SetParent(_tilesTransform);
            tileTransform.position = new Vector3(tileCoords.x, tileCoords.y, 0f);
            tile.Setup(tileCoords, tileConfig);
        }

        private void SetupGoals()
        {
            var questTile = _config.WinTile;
            _questMarker = Instantiate(_questMarkerPrefab, _tilesTransform);
            _questMarker.transform.position = CoordsIntoQuestMarkerPosition(questTile);
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
                
                default:
                    throw new Exception("Unknown res type set");
            }
        }

        public async UniTask ShuffleTiles()
        {
            var unblockedTiles = _tiles.Where(pair => pair.Key != CharacterPosition && pair.Key != _config.WinTile).ToList();
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
    public class FieldConfig
    {
        public Vector2Int Size = new (5, 5);
        public CharacterBaseParams CharacterBaseParams = new ();
        public Vector2Int CharacterInitialTile = new(0, 0);
        public List<TileConfig> Tiles;
        public Vector2Int WinTile = new(0, 0);
        public int TileWithoutEventSpawnRate = 1;
        public List<EventConfig> Events;
    }

    [Serializable]
    public class TileConfig
    {
        public FieldTileController TilePrefab;
        public int SpawnRate = 1;
        public int Cost = 1;
    }

    public class CharacterBaseParams
    {
        public int Energy = 20;
        public int Hp = 20;
        public int Attack = 2;
    }

    [Serializable]
    public class EventConfig
    {
        public TileEvent Event;
        public int SpawnRate = 1;
    }

    public enum GameState
    {
        AwaitingInput = 0,
        ProcessingTurn = 1,
        Win = 2,
        Lost = 3
    }

    public enum PlayerResources
    {
        Energy,
        Health
    }

    public interface IGameController
    {
        public int Energy { get; }
        public int Hp { get; }
        public int Attack { get; }

        public Vector2Int FieldSize { get; }
        public Vector2Int CharacterPosition { get; }
        public void AddResource(PlayerResources resType, int resCount);
        public void SetResource(PlayerResources resType, int resCount);
        public UniTask ShuffleTiles();
        public UniTask RotateTiles(Vector2Int center, bool clockwise);
        public BattleUnit CharacterBattlePrefab { get; }
        public UniTask Battle(MonsterData monster);
        public UniTask Quest(Quest quest);
    }
}
