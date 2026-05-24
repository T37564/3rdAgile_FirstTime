using Fusion;
using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class GenerateEnemyType
{
    public GameObject enemyObject;
}

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] public GenerateEnemyType[] generateEnemys;

    [SerializeField] private int generateCount = 0;

    [SerializeField] Transform[] spawnTransform;

    [SerializeField] private NetworkRunner runner;

    //private void Start()
    //{
    //    runner = FindAnyObjectByType<NetworkRunner>();

    //    Debug.Log(runner.IsRunning);
    //    GenerateEnemy();
    //}

    private IEnumerator Start()
    {
        // Runnerが起動するまで待機
        yield return new WaitUntil(() => runner.IsRunning);

        Debug.Log("Runner Start");
        // Host/Serverだけ生成
        if (!runner.IsServer) yield break;

        GenerateEnemy();
    }



    private void GenerateEnemy()
    {
        Debug.Log(runner);
        Debug.Log(generateEnemys[0].enemyObject);
        Debug.Log(spawnTransform.Length);

        for (int i = 0; i < generateCount; i++)
        {
            Debug.Log(spawnTransform[i]);
            runner.Spawn(generateEnemys[i].enemyObject, 
                spawnTransform[i].position, spawnTransform[i].rotation);
        }
    }
}
