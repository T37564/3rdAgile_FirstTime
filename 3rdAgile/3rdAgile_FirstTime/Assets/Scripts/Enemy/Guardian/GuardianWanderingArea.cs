using UnityEngine;
using Fusion;

public class GuardianWanderingArea : NetworkBehaviour
{

    [Header("œpœj‚·‚éÛ‚ÌŒü‚©‚¤À•W")]
    [SerializeField] private Transform[] wanderingPosition;

    
    public Transform GetRandomPoint()
    {
        int index = Random.Range(0, wanderingPosition.Length);

        return wanderingPosition[index];
    }
}
