using System;
using UnityEngine;

[Serializable]
public class GenerateEnemy
{
    public GameObject enemyObject;
}

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] public GenerateEnemy generateEnemys;

    [SerializeField] private int generateCount = 0;

    [SerializeField] Transform[] spawnTransform;


    // Update is called once per frame
    private void Update()
    {
        GenerateEnemy();
    }

    private void GenerateEnemy()
    {
        for(int i = 0; i < generateCount; i++)
        {
            Instantiate(generateEnemys.enemyObject, spawnTransform[i]);
        }
    }
}
