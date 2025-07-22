using System.Collections;
using System.Collections.Generic;
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
    Defeated,
}

public class EnemyShipManager : MonoBehaviour
{
    private Vector2[,] restingLocationArray; // This holds the list of coordinates that a ship can be positioned at.
    private LocationState[,] statusArray; // This holds the information about whether or not that position has been assigned or not. 

    private float shipSpacing = 1f;
    public Transform center;
    public SpaceEnemyHealth enemy;

    private bool coroutineAllowed = true;
    private bool spawningGroup = false;

    private WaveState waveState;
    [SerializeField] EnemyFormation[] formations;
    private EnemyFormation formation;
    private bool allEnemiesDefeated = false;
    private List<SpaceEnemyHealth> availableEnemyList;
    private List<SpaceEnemyHealth> attackingEnemyList;

    // I need some way to keep track of all enemies that are spawned. 
    // 

    private void Awake()
    {
        availableEnemyList = new List<SpaceEnemyHealth>();
        attackingEnemyList = new List<SpaceEnemyHealth>();
        waveState = WaveState.Spawning;
        InitializeLocationArray(12, 5);
        // SpawnEnemyAtEachLocation();
    }

    private void Start()
    {
        // here we should choose a formation
        // for now we'll just use the test formation
    }

    private void FixedUpdate()
    {
        // Cleanup any defeated enemies
        DeadEnemyCleanup();
        // Check if the enemy manager needs to start a new behavior
        if (coroutineAllowed) { StartNewBehavior(); }
    }

    // INITIALIZATION HELPER FUNCTIONS ============================================================

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

    // UPDATE HELPER FUNCTIONS ====================================================================

    private void StartNewBehavior()
    {
        // Don't want to allow a new coroutine until all the enemies from the last group have settled or died
        // Also don't want to allow new spawning coroutines once all of the enemies have been spawned. From there a new attack coroutine should be called instead.
        switch (waveState)
        {
            case WaveState.Spawning:
                // Choose an enemy formation
                formation = ChooseEnemyFormation();
                // Spawn the enemy formation
                StartCoroutine(SpawnEnemyFormation(formation));
                break;
            case WaveState.Attacking:
                // Start the attacking coroutine. Decisions will be made from within this behavior.
                StartCoroutine(AttackMode());
                break;
            case WaveState.Defeated:
                // TODO: Implement me!
                // celebrate the player's victory
                // prepare for the next wave
                break;
        }
    }

    private void DeadEnemyCleanup()
    {
        // Remove destroyed objects from the lists
        availableEnemyList.RemoveAll(s => s == null);
        attackingEnemyList.RemoveAll(s => s == null);

        // If the manager is done spawning enemies (in attack mode) and the enemy list is empty, signal that all enemies have been defeated.
        if (availableEnemyList.Count == 0 && attackingEnemyList.Count == 0 && waveState == WaveState.Attacking)
        {
            allEnemiesDefeated = true;
        }
    }

    // ENEMY SPAWNING FUNCTIONS ===================================================================

    private EnemyFormation ChooseEnemyFormation()
    {
        // Choose an enemy formation at complete random.
        // TODO: Classify formations based on difficulty and use that to weight the choice
        int choice = Random.Range(0, formations.Length);
        // Debug.Log("There are " + formations.Length + " to choose from. Formation #" + choice + " was chosen.");
        return formations[choice];
    }

    // This is the control function for spawning the entire formation of enemies
    private IEnumerator SpawnEnemyFormation(EnemyFormation formation)
    {
        coroutineAllowed = false;

        // Cycle through each enemy group in the formation and spawn each of them in order
        foreach (EnemyGroup enemyGroup in formation.enemyGroups)
        {
            // Starts the coroutine to spawn in this group of enemies
            StartCoroutine(SpawnEnemyGroup(enemyGroup));

            // Locks this coroutine and prevents it from spawning new enemy groups before the current group has finished spawning. 
            while (spawningGroup) { yield return new WaitForEndOfFrame(); }

            // Before 1. [spawning the next wave] or 2. [releasing control to the attack behavior] I need to make sure all LIVING enemies have reached the formation.
            while (AllEnemiesAreInFormation() == false) { yield return new WaitForSecondsRealtime(0.25f); } // Doesn't need to be checked every frame

            // Gives some breathing room before the next wave spawns or the attack behavior begins.
            yield return new WaitForSecondsRealtime(0.5f);
        }

        waveState = WaveState.Attacking;
        coroutineAllowed = true;
    }

    // This is called before the script starts the attack behavior. Doesn't account for any enemies that might be in attack mode.
    private bool AllEnemiesAreInFormation()
    {
        foreach (SpaceEnemyHealth enemy in availableEnemyList)
        {
            if (enemy.GetComponent<SpaceEnemyLogic>().GetShipState() == EnemyShipState.Resting) continue;
            else return false;
        }
        return true;
    }

    private IEnumerator SpawnEnemyGroup(EnemyGroup enemyGroup)
    {
        spawningGroup = true;

        SpaceEnemyHealth enemyToSpawn = enemy;
        SpaceEnemyLogic enemyLogic = enemyToSpawn.GetComponent<SpaceEnemyLogic>();
        int enemyQuantity = enemyGroup.groupCoordinates.Length;

        // Decide which of the ship's available routes should be chosen for entering and attacking
        int eChoice = Random.Range(0, enemyLogic.entranceRoutes.Count);
        int aChoice = Random.Range(0, enemyLogic.attackRoutes.Count);
        Route eRoute = enemyLogic.entranceRoutes[eChoice];
        Route aRoute = enemyLogic.attackRoutes[aChoice];

        // Helper variables for the loop
        float spawnSpeed = 0.5f;
        int enemiesSpawned = 0;
        float spawnTimer = 0f;

        while (enemiesSpawned < enemyQuantity)
        {
            spawnTimer += Time.deltaTime;

            if (spawnTimer > spawnSpeed)
            {
                SpawnEnemy(enemyToSpawn, eRoute, aRoute, enemyGroup.groupCoordinates[enemiesSpawned]);
                enemiesSpawned += 1;
                spawnTimer = 0f;
            }

            yield return new WaitForEndOfFrame();
        }

        spawningGroup = false;
    }

    // This function will be invoked whenever a coroutine needs to create an enemy. It is told what enemy prefab will be used and what entrance route to take
    private void SpawnEnemy(SpaceEnemyHealth enemy, Route entranceRoute, Route attackRoute, Vector2 arrayPosition)
    {
        SpaceEnemyHealth newEnemy = Instantiate(enemy, new Vector3(1000f, 1000f, 0f), new Quaternion(0f, 0f, 0f, 0f), this.transform);
        newEnemy.GetComponent<SpaceEnemyLogic>().SetEntranceRoute(entranceRoute);
        newEnemy.GetComponent<SpaceEnemyLogic>().SetAttackRoute(attackRoute);
        newEnemy.GetComponent<SpaceEnemyLogic>().SetRestingLocation(restingLocationArray[(int)arrayPosition.x, (int)arrayPosition.y] + new Vector2(center.transform.position.x, center.transform.position.y));
        newEnemy.GetComponent<SpaceEnemyLogic>().SetEnemyShipManager(this);
        availableEnemyList.Add(newEnemy);
    }

    // ATTACK FUNCTIONS ===========================================================================

    private IEnumerator AttackMode()
    {
        coroutineAllowed = false;
        float timeBetweenAttacks = 5f;
        float timer = timeBetweenAttacks;

        while (allEnemiesDefeated == false)
        {
            timer += Time.deltaTime;
            if (timer > timeBetweenAttacks)
            {
                SendEnemyToAttack();
                // send an enemy to attack
                // if the enemy sent to attack is defeated the timer should be increased
                timer = 0f;
            }


            yield return new WaitForEndOfFrame();
        }

        waveState = WaveState.Defeated;
        coroutineAllowed = true;
    }

    private void SendEnemyToAttack()
    {
        int choice = Random.Range(0, availableEnemyList.Count);
        SpaceEnemyHealth chosenEnemy = availableEnemyList[choice];
        chosenEnemy.GetComponent<SpaceEnemyLogic>().SetAttackSignal(true);
        availableEnemyList.Remove(chosenEnemy);
        attackingEnemyList.Add(chosenEnemy);
    }

    // HELPER FUNCTIONS TO RECIEVE COMMUNCATIONS FROM SPAWNED ENEMY SHIPS

    public void MakeShipAvailable(SpaceEnemyHealth enemy)
    {
        attackingEnemyList.Remove(enemy);
        availableEnemyList.Add(enemy);
    }

    // LEFTOVER TESTING FUNCTIONS. COULD STILL BE USEFUL.

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