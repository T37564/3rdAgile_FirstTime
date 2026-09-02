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
    [SerializeField] public GenerateEnemyType[] generateEnemys;

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

        for (int i = 0; i < generateCount; i++)
        {
            NetworkObject enemyPrefab = generateEnemys[i].enemyPrefabObject;

            if (enemyPrefab == null)
            {
                Debug.LogError($"{i}番目の敵PrefabはNULLです");
                continue;
            }

            Vector3 spawnPosition = enemyObjectPlace.GetRandomPosition(generateEnemys[i].stageTypes);

            runner.Spawn(enemyPrefab, spawnPosition, Quaternion.identity, null, (runner, spawnedEnemy) =>
            {
                NavMeshAgent agent = spawnedEnemy.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    Debug.Log(
                        $"Spawn直後: {spawnedEnemy.transform.position}"
                    );

                    if (NavMesh.SamplePosition(
                        spawnedEnemy.transform.position,
                        out NavMeshHit hit,
                        2.0f,
                        NavMesh.AllAreas))
                    {
                        agent.Warp(hit.position);

                        Debug.Log(
                            $"NavMesh上へWarp: {hit.position}"
                        );
                    }
                    else
                    {
                        Debug.LogWarning(
                            "Spawn位置の近くにNavMeshがありません"
                        );
                    }
                }
            });

            Debug.Log(
    $"【敵Spawn後】" +
    $"名前={enemyPrefab.name}, " +
    $"位置={enemyPrefab.transform.position}, " +
    $"指定位置={spawnPosition}"
);
            //StartCoroutine(CheckEnemyPosition(spawnedEnemy, spawnPosition));
        }
    }

    private IEnumerator CheckEnemyPosition(NetworkObject enemy,Vector3 spawnPosition)
    {
        yield return null;

        Debug.Log($"【1フレーム後】" + $"実際={enemy.transform.position}, " + $"指定={spawnPosition}");

        //if (enemy.transform.position != spawnPosition)
        //{
        //    enemy.transform.position = spawnPosition;

        //    Debug.Log(
        //        $"【位置修正】" +
        //        $"修正後={enemy.transform.position}"
        //    );
        //}
    }
}
