using Fusion;
using UnityEngine;
using UnityEngine.AI;

public class GuardianController : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent navMeshAgent;

    private Transform players;

    private void Start()
    {
        // Playerタグのオブジェクトを探す
        GameObject target = GameObject.FindWithTag("Player");

        if (target != null)
        {
            players = target.transform;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        navMeshAgent.SetDestination(players.position);
    }

    private void FindNearestPlauer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float shortestDistance = Mathf.Infinity;
        Transform nearestPlayer = null;

        foreach(GameObject playerObject in players)
        {

        }
    }
}
