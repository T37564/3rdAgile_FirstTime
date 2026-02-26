using UnityEngine;
using Fusion;
using Network.Player;

public class Guardian : NetworkBehaviour
{
    private const float DetectionRadius = 5f;

    [SerializeField] private LayerMask playerLayer;

    private Collider[] hitCollider = new Collider[4];

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            Debug.Log("Guardian spawned with state authority.");
        }
        else
        {
            Debug.Log("Guardian spawned without state authority.");
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        var hits = Runner.GetPhysicsScene().OverlapSphere(
            transform.position,
            DetectionRadius,
            hitCollider,
            playerLayer,
            QueryTriggerInteraction.Collide
            );

        for (int i = 0; i < hits; i++)
        {
            var player = hitCollider[i].GetComponent<PlayerController>();
            if (player != null)
            {
                // ここでプレイヤーのダメージ(死亡)処理を呼び出す。
                player.TakeDamage();
            }
        }
    }
}
