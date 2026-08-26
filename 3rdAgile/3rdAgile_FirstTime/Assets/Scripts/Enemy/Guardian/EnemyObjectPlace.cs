using NUnit.Framework;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class EnemyObjectPlace : MonoBehaviour
{
    public List<BoxCollider> enemyGroundColliders = new();

    private void OnEnable()
    {
        StageSpawner.OnMapGenerated += RegisterGrounds;
    }

    private void OnDisable()
    {
        StageSpawner.OnMapGenerated -= RegisterGrounds;
    }

    public void RegisterGrounds()
    {
        enemyGroundColliders.Clear();

        //StageTypeKinds[] stages = FindObjectsByType<StageTypeKinds>(FindObjectsSortMode.None);

        //stageTypeKinds.AddRange(stages);

        GameObject[] grounds = GameObject.FindGameObjectsWithTag("EnemyGround");

        foreach (GameObject ground in grounds)
        {
            BoxCollider box = ground.GetComponent<BoxCollider>();

            NavMeshSurface surface = ground.GetComponent<NavMeshSurface>();
            //surface.BuildNavMesh();

            if (box != null)
            {
                enemyGroundColliders.Add(box);
            }
        }
    }

    public Vector3 GetRandomPosition()
    {
        if (enemyGroundColliders.Count == 0)
        {
            Debug.LogError("EnemyGroundÇ™Ç†ÇËÇ‹ÇπÇÒ");
            return Vector3.zero;
        }

        BoxCollider box = enemyGroundColliders[Random.Range(0, enemyGroundColliders.Count)];

        Bounds bounds = box.bounds;

        float x = Random.Range(bounds.min.x + 0.5f, bounds.max.x - 0.5f);
        float z = Random.Range(bounds.min.z + 0.5f, bounds.max.z - 0.5f);

        // Colliderè„ïtãﬂÇÃç¿ïWÇçÏÇÈ
        Vector3 randomPosition = new Vector3(x, bounds.max.y + 1.0f, z);
        // ãﬂÇ≠ÇÃNavMeshè„ÇÃç¿ïWÇéÊìæ
        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        //return new Vector3(x, bounds.max.y + 1f, z);
        return randomPosition;
    }
}
