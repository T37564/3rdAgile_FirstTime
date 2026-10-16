using Fusion;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class GenerateEnemyType
{
    // 生成する敵のPrefab
    public NetworkObject enemyPrefabObject;

    // 出現するステージの種類
    public StageTypeKinds stageTypes;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("生成する敵の種類")]
    [SerializeField] public GenerateEnemyType[] generateEnemys;

    [Header("生成する敵の数")]
    [SerializeField] private int generateCount = 0;

    [SerializeField] private EnemyObjectPlace enemyObjectPlace;

    private NetworkRunner runner;

    private void Awake()
    {
        runner = FindAnyObjectByType<NetworkRunner>();

        if (runner == null)
        {
            Debug.LogError("NetworkRunnerが見つかりません");
            return;
        }

        //GenerateEnemy();
    }

    private void OnEnable()
    {
        StageSpawner.OnNavMeshGenerated += GenerateEnemy;
    }

    private void OnDisable()
    {
        StageSpawner.OnNavMeshGenerated -= GenerateEnemy;
    }

    private void OnNavMeshGenerated()
    {
        StartCoroutine(GenerateEnemyAfterReady());
    }

    private IEnumerator GenerateEnemyAfterReady()
    {
        yield return null;

        Physics.SyncTransforms();

        GenerateEnemy();
    }

    private void GenerateEnemy()
    {
        // runnerが取得できていない場合
        if (runner == null)
        {
            Debug.LogError("NetworkRunnerが取得できていません");
            return;
        }

        // ホスト以外は生成しない
        if (!runner.IsServer)
        {
            return;
        }

        // 生成する敵の数だけループ
        for (int i = 0; i < generateCount; i++)
        {
            // 生成する敵の種類をランダムに選択
            int enemyIndex = UnityEngine.Random.Range(0, generateEnemys.Length);

            // 生成する敵の種類を取得
            GenerateEnemyType enemyType = generateEnemys[enemyIndex];

            // 生成する敵のPrefabを取得
            NetworkObject enemyPrefab = enemyType.enemyPrefabObject;

            if (enemyType == null)
            {
                Debug.LogError($"{i}番目の敵PrefabはNULLです");
                continue;
            }

            //Vector3 spawnPosition = enemyObjectPlace.GetRandomPosition(generateEnemys[i].stageTypes);
            Vector3 spawnPosition = enemyObjectPlace.GetRandomPosition(enemyType.stageTypes);

            // NavMesh上に敵を配置させる
            NetworkObject spawnedEnemy = runner.Spawn(enemyPrefab, spawnPosition, Quaternion.identity, null, (runner, spawnedEnemy) =>
            {
                NavMeshAgent agent = spawnedEnemy.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    Debug.Log(
                        $"Spawn直後: {spawnedEnemy.transform.position}"
                    );

                    if (NavMesh.SamplePosition(spawnedEnemy. transform. position, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
                    {
                        agent.Warp(hit.position);
                    }
                    else
                    {
                        Debug.LogWarning("Spawn位置の近くにNavMeshがありません");
                    }
                }
            });
        }
    }
}
