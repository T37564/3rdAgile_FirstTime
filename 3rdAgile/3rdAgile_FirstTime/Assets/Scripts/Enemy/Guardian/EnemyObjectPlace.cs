using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.AI.Navigation;

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

        GameObject[] grounds = GameObject.FindGameObjectsWithTag("EnemyGround");

        foreach (GameObject ground in grounds)
        {
            BoxCollider box = ground.GetComponent<BoxCollider>();

            NavMeshSurface surface = ground.GetComponent<NavMeshSurface>();
            surface.BuildNavMesh();

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
            Debug.LogError("EnemyGround‚ª‚ ‚è‚Ü‚¹‚ñ");
            return Vector3.zero;
        }

        BoxCollider box = enemyGroundColliders[Random.Range(0, enemyGroundColliders.Count)];

        Bounds bounds = box.bounds;

        float x = Random.Range(bounds.min.x + 0.5f, bounds.max.x - 0.5f);
        float z = Random.Range(bounds.min.z + 0.5f, bounds.max.z - 0.5f);

        return new Vector3(x, bounds.max.y + 1f, z);
    }
}
