using System;
using UnityEngine;

[Serializable]
public class SpawnData
{
    public float delay;
    public Enemy.EnemyType enemyType;
    public int point;
}

[Serializable]
public class WaveData
{
    public SpawnData[] enemies;
}

[Serializable]
public class StageData
{
    public WaveData[] waves;
}