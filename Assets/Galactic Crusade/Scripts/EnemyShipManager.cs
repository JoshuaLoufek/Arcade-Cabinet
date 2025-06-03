using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

// https://gamedev.stackexchange.com/questions/82811/implementing-galaga-style-enemy-behavior-in-unity

public class EnemyShipManager : MonoBehaviour
{
    private Vector2[,] locationArray;
    private float shipSpacing = 1.5f;
    public Transform center;
    public SpaceEnemy enemy;

    // Let's start off with an 8 x 4 array within a standard space of 12 units by 6 units

    private void Awake()
    {
        locationArray = new Vector2[8, 4];
        FillLocationArray();
        SpawnEnemyAtEachLocation();
    }

    private void FillLocationArray()
    {
        float xOffset = -((shipSpacing * locationArray.GetLength(0) / 2) - (shipSpacing / 2));
        float yOffset = -((shipSpacing * locationArray.GetLength(1) / 2) - (shipSpacing / 2));

        Debug.Log("yOffset: " + yOffset);

        for (int i = 0; i < locationArray.GetLength(0); i++)
        {
            for (int j = 0; j < locationArray.GetLength(1); j++)
            {
                float x, y;

                x = xOffset + (shipSpacing * i);
                y = yOffset + (shipSpacing * j);

                Debug.Log("X, Y: " + x + ", " + y);

                locationArray[i, j] = new Vector2(x, y);
            }
        }
    }

    private void SpawnEnemyAtEachLocation()
    {
        for (int i = 0; i < locationArray.GetLength(0); i++)
        {
            for (int j = 0; j < locationArray.GetLength(1); j++)
            {
                SpaceEnemy newEnemy = Instantiate(enemy, this.transform);
                newEnemy.transform.localPosition = locationArray[i, j];
            }
        }
    }
}
