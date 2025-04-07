using System;
using System.Collections.Generic;
using Game.Gameplay;
using Game.Gameplay.Events;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu]
    public class GameStage : ScriptableObject
    {
        public Vector2Int Size = new (5, 5);
        public Vector2Int CharacterInitialTile = new(0, 0);
        public List<TileConfig> Tiles;
        public TileConfig GoalTileConfig;
        public Vector2Int GoalTile = new(0, 0);
        public int TileWithoutEventSpawnRate = 1;
        public List<EventConfig> Events;
        public CameraConfig CameraConfig;
    }
    
    [Serializable]
    public class TileConfig
    {
        public FieldTileController TilePrefab;
        public int SpawnRate = 1;
        public int Cost = 1;
    }
    
    [Serializable]
    public class EventConfig
    {
        public TileEvent Event;
        public int SpawnRate = 1;
    }
    
    [Serializable]
    public class CameraConfig
    {
        public Vector2 Position;
        public float Size = 3;
    }
}
