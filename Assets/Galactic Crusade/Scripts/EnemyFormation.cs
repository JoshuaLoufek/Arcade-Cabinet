using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemyFormation : ScriptableObject
{
    public string formationName;

    // This represents the coordinate points on the ship grid where each group can be spawned.
    public List<EnemyGroup> enemyGroups;
}

[Serializable]
public class EnemyGroup
{
    public Vector2[] groupCoordinates;
}