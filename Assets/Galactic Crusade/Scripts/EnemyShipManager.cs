using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

// https://gamedev.stackexchange.com/questions/82811/implementing-galaga-style-enemy-behavior-in-unity

// Terminology
    // A WAVE of enemies is the entire fleet spawned that wave. A wave must be eliminated entirely before the next wave can spawn. 
    // A GROUP of enemies is each selection of ships that spawn together in a consistent pattern. Many groups make up a wave.

public enum LocationState
{
    Available, // No ship has been assigned here yet
    Assigned, // A ship has been assigned this location
}

public class EnemyShipManager : MonoBehaviour
{
    private Vector2[,] locationArray; // This holds the list of coordinates that a ship can be positioned at.
    private LocationState[,] statusArray; // This holds the information about whether or not that position has been assigned or not. 

    private float shipSpacing = 1.5f;
    public Transform center;
    public SpaceEnemyHealth enemy;
    public Route entranceRoute;
    public Route attackRoute;

    private bool coroutineAllowed = true;

    // Let's start off with an 8 x 4 array within a standard space of 12 units by 6 units

    private void Awake()
    {
        FillLocationArray(8, 4);
        // SpawnEnemyAtEachLocation();
    }

    private void FixedUpdate()
    {
        if (coroutineAllowed)
        {
            StartCoroutine(CreateGroupOfEnemies());
        }
    }

    private IEnumerator CreateGroupOfEnemies()
    {
        coroutineAllowed = false;

        // Here we determine the following:
            // what enemies to make
            // how many of them to make
            // What entrance route they should take
            // what formation they should assemble in (aka resting locations)

        SpaceEnemyHealth enemyToSpawn = enemy;
        int enemyQuantity = 1;
        Route eRoute = entranceRoute;
        Route aRoute = attackRoute;

        // Helper variables for the loop
        float spawnSpeed = 0.5f;
        int enemiesSpawned = 0;
        float spawnTimer = 0f;
        
        while (enemiesSpawned < enemyQuantity)
        {
            spawnTimer += Time.deltaTime;

            if (spawnTimer > spawnSpeed)
            {
                SpawnEnemy(enemy, eRoute, aRoute);
                enemiesSpawned += 1;
                spawnTimer = 0f;
            }

            yield return new WaitForEndOfFrame();
        }

        // coroutineAllowed = true;
    }

    private Vector2 AssignFirstFreeLocation()
    {
        for (int i = 0; i < locationArray.GetLength(0); i++)
        {
            for (int j = 0; j < locationArray.GetLength(1); j++)
            {
                if (statusArray[i, j] == LocationState.Available)
                {
                    statusArray[i, j] = LocationState.Assigned;
                    return locationArray[i, j] + new Vector2(center.transform.position.x, center.transform.position.y);
                }
            }
        }

        return Vector2.zero;
    }

    private void FillLocationArray(int xLength, int yLength)
    {
        locationArray = new Vector2[8, 4];
        statusArray = new LocationState[8, 4];

        float xOffset = -((shipSpacing * locationArray.GetLength(0) / 2) - (shipSpacing / 2));
        float yOffset = -((shipSpacing * locationArray.GetLength(1) / 2) - (shipSpacing / 2));

        for (int i = 0; i < locationArray.GetLength(0); i++)
        {
            for (int j = 0; j < locationArray.GetLength(1); j++)
            {
                float x, y;

                x = xOffset + (shipSpacing * i);
                y = yOffset + (shipSpacing * j);

                locationArray[i, j] = new Vector2(x, y);
                statusArray[i, j] = LocationState.Available;
            }
        }
    }

    // This function will be invoked whenever a coroutine needs to create an enemy.
    // It is told what enemy prefab will be used and what entrance route to take
    private void SpawnEnemy(SpaceEnemyHealth enemy, Route entranceRoute, Route attackRoute)
    {
        SpaceEnemyHealth newEnemy = Instantiate(enemy, this.transform);
        newEnemy.GetComponent<SpaceEnemyLogic>().SetEntranceRoute(entranceRoute);
        newEnemy.GetComponent<SpaceEnemyLogic>().SetAttackRoute(attackRoute);
        newEnemy.GetComponent<SpaceEnemyLogic>().SetRestingLocation(AssignFirstFreeLocation());
    }

    private void SpawnEnemyAtEachLocation()
    {
        for (int i = 0; i < locationArray.GetLength(0); i++)
        {
            for (int j = 0; j < locationArray.GetLength(1); j++)
            {
                SpaceEnemyHealth newEnemy = Instantiate(enemy, this.transform);
                newEnemy.transform.localPosition = locationArray[i, j];
            }
        }
    }
}