using Fusion;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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

    [SerializeField] private EnemyObjectPlace enemyObjectPlace;

    //[SerializeField] private NetworkRunner runner;

    //private void Start()
    //{
    //    runner = FindAnyObjectByType<NetworkRunner>();

    //    Debug.Log(runner.IsRunning);
    //    GenerateEnemy();
    //}

    private void Start()
    {
        // Runnerが起動するまで待機
        //yield return new WaitUntil(() => runner.IsRunning);

        Debug.Log("Runner Start");
        // Host/Serverだけ生成
        //if (!runner.IsServer) yield break;

        GenerateEnemy();
    }



    private void GenerateEnemy()
    {
        for (int i = 0; i < generateCount; i++)
        {
            Vector3 generatePosition=enemyObjectPlace.GetRandomPosition();

            Instantiate(generateEnemys[i].enemyObject,generatePosition,Quaternion.identity);
            NavMeshAgent agent = GetComponent<NavMeshAgent>();

            Debug.Log(agent.isOnNavMesh);
        }
    }
}
