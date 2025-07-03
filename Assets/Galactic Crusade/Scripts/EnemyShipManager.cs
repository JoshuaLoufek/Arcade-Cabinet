using System.Collections;
using System.Text.RegularExpressions;
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

public enum WaveState
{
    Spawning,
    Attacking,
}

public class EnemyShipManager : MonoBehaviour
{
    private Vector2[,] restingLocationArray; // This holds the list of coordinates that a ship can be positioned at.
    private LocationState[,] statusArray; // This holds the information about whether or not that position has been assigned or not. 

    private float shipSpacing = 1.5f;
    public Transform center;
    public SpaceEnemyHealth enemy;
    public Route entranceRoute;
    public Route attackRoute;

    private bool coroutineAllowed = true;
    private bool spawningGroup = false;

    private WaveState waveState;
    [SerializeField] EnemyFormation[] formations;


    private void Awake()
    {
        waveState = WaveState.Spawning;
        InitializeLocationArray(10, 5);
        // SpawnEnemyAtEachLocation();
    }

    private void Start()
    {
        // here we should choose a formation
        // for now we'll just use the test formation
    }

    private void FixedUpdate()
    {
        // Don't want to allow a new coroutine until all the enemies from the last group have settled or died
        // Also don't want to allow new spawning coroutines once all of the enemies have been spawned. From there a new attack coroutine should be called instead.
        if (coroutineAllowed)
            switch (waveState)
            {
                case WaveState.Spawning:
                    //StartCoroutine(SpawnGroupOfEnemies());
                    StartCoroutine(SpawnEnemyFormation(formations[0]));
                    break;
            }
    }

    private IEnumerator SpawnEnemyFormation(EnemyFormation formation)
    {
        coroutineAllowed = false;

        // Cycle through each enemy group in the formation and spawn each of them in order
        foreach (EnemyGroup enemyGroup in formation.enemyGroups)
        {
            // Starts the coroutine to spawn in this group of enemies
            StartCoroutine(SpawnEnemyGroup(enemyGroup));

            // Locks this coroutine from spawning new enemy groups before the current group has finished spawning. 
            while (spawningGroup)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        coroutineAllowed = true;
    }

    private IEnumerator SpawnEnemyGroup(EnemyGroup enemyGroup)
    {
        spawningGroup = true;

        SpaceEnemyHealth enemyToSpawn = enemy;
        int enemyQuantity = enemyGroup.groupCoordinates.Length;
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
                SpawnEnemy(enemyToSpawn, eRoute, aRoute);
                enemiesSpawned += 1;
                spawnTimer = 0f;
            }

            yield return new WaitForEndOfFrame();
        }

        spawningGroup = false;
    }

    private IEnumerator SpawnGroupOfEnemies()
    {
        coroutineAllowed = false;

        // Here we determine the following:
            // what enemies to make
            // how many of them to make
            // What entrance route they should take
            // what formation they should assemble in (in other words, what are their resting locations?)

        SpaceEnemyHealth enemyToSpawn = enemy;
        int enemyQuantity = 8;
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
                SpawnEnemy(enemyToSpawn, eRoute, aRoute);
                enemiesSpawned += 1;
                spawnTimer = 0f;
            }

            yield return new WaitForEndOfFrame();
        }

        // have a check here to determine if this is the last group of enemies that needed to be spawned. If so, change to attack mode.
        
        // coroutineAllowed = true;
    }

    private Vector2 AssignFirstFreeLocation()
    {
        for (int i = 0; i < restingLocationArray.GetLength(0); i++)
        {
            for (int j = 0; j < restingLocationArray.GetLength(1); j++)
            {
                if (statusArray[i, j] == LocationState.Available)
                {
                    statusArray[i, j] = LocationState.Assigned;
                    return restingLocationArray[i, j] + new Vector2(center.transform.position.x, center.transform.position.y);
                }
            }
        }

        return Vector2.zero;
    }

    private void InitializeLocationArray(int xLength, int yLength)
    {
        restingLocationArray = new Vector2[xLength, yLength];
        statusArray = new LocationState[xLength, yLength];

        float xOffset = -((shipSpacing * restingLocationArray.GetLength(0) / 2) - (shipSpacing / 2));
        float yOffset = -((shipSpacing * restingLocationArray.GetLength(1) / 2) - (shipSpacing / 2));

        for (int i = 0; i < restingLocationArray.GetLength(0); i++)
        {
            for (int j = 0; j < restingLocationArray.GetLength(1); j++)
            {
                float x, y;

                x = xOffset + (shipSpacing * i);
                y = yOffset + (shipSpacing * j);

                restingLocationArray[i, j] = new Vector2(x, y);
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
        for (int i = 0; i < restingLocationArray.GetLength(0); i++)
        {
            for (int j = 0; j < restingLocationArray.GetLength(1); j++)
            {
                SpaceEnemyHealth newEnemy = Instantiate(enemy, this.transform);
                newEnemy.transform.localPosition = restingLocationArray[i, j];
            }
        }
    }
}