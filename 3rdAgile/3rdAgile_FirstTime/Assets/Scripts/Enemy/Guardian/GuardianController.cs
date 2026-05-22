using Fusion;
using UnityEngine;
using UnityEngine.AI;

public class GuardianController : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent navMeshAgent;

    [Header("í«è]Ç∑ÇÈëŒè€ÇÃTransform")]
    [SerializeField] Transform[] players;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        navMeshAgent.SetDestination(transform.position);
    }
}
