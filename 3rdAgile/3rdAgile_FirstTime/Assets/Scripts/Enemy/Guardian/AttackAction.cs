using Network.Player;
using UnityEngine;
using UnityEngine.AI;
using Fusion;

public enum EnemyState
{
    move,// 敵が移動できるかの状態
    attackReady,//攻撃前の予備動作
    attack,//攻撃中
    moveCoolDown // 敵が攻撃後の硬直状態
}
public class AttackAction : NetworkBehaviour
{
    [SerializeField] private GuardianController guardianController;

    [SerializeField] private NavMeshAgent navMeshAgent;

    [Header("攻撃距離")]
    [SerializeField] private float attackDistance = 0.0f;

    [Header("予備動作時間")]
    [SerializeField] private float attackReadyTime = 0.0f;

    [Header("攻撃後硬直")]
    [SerializeField] private float cooldownTime = 0.0f;

    [Header("プレイヤーに与えるダメージ")]
    [SerializeField] private int attackDamage = 0;

    private float timer = 0.0f;

    private EnemyState enemyState;

    [SerializeField] private GuardianWanderingArea guardianWanderingArea;

    private Vector3 WanderingPoint;

    // 徘徊する場所に到着したかの判定
    public bool isArrival = false;

    //[Networked] private TickTimer attackTimer { get; set; }

    public override void Spawned()
    {
        enemyState = EnemyState.move;

        guardianWanderingArea.FindWanderingGround();
    }

    private void Update()
    {
        if (Object == null) return;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        switch (enemyState)
        {
            case EnemyState.move:
                MoveState();
                break;

            case EnemyState.attackReady:
                AttackReadyState();
                break;

            case EnemyState.attack:
                AttackState();
                break;

            case EnemyState.moveCoolDown:
                CooldownState();
                break;
        }
    }

    private void Wandering()
    {
        if (guardianWanderingArea == null) return;

        // 現在地から目的地までの距離が一定以下の場合
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= 0.3f)
        {
            // 新しい座標を取得してその座標に向かう
            WanderingPoint = guardianWanderingArea.GetRandomPoint();
            navMeshAgent.SetDestination(WanderingPoint);
        }
    }

    private void MoveState()
    {
        navMeshAgent.isStopped = false;

        Wandering();

        // プレイヤーとの距離が攻撃距離以下になったら攻撃準備状態に移行
        if (guardianController.currentDistance <= attackDistance)
        {
            Debug.Log("攻撃準備");
            enemyState = EnemyState.attackReady;
            timer = attackReadyTime;
            navMeshAgent.isStopped = true;
        }

    }

    private void AttackReadyState()
    {
        timer -= Time.deltaTime;
        Debug.Log("攻撃予備動作: " + timer);
        transform.LookAt(guardianController.currentPlayer);

        if(guardianController.currentDistance >= attackDistance)
        {
            navMeshAgent.isStopped = false;
            enemyState = EnemyState.move;
        }

        // 予備動作時間が経過したら攻撃状態に移行
        if (timer <= 0)
        {
            Debug.Log("攻撃実行");
            enemyState = EnemyState.attack;
            //attackTimer = TickTimer.CreateFromSeconds(Runner, cooldownTime);
        }
    }

    private void AttackState()
    {
        // 攻撃の実装
        // 攻撃が完了したらクールダウン状態に移行
        //if (attackTimer.Expired(Runner))
        //{
        //    enemyState = EnemyState.moveCoolDown;
        //    attackTimer = TickTimer.CreateFromSeconds(Runner, cooldownTime);
        //}
        Debug.Log("攻撃");

        PlayerController playerController = guardianController.currentPlayer.GetComponent<PlayerController>();

        if (playerController != null)
        {
            playerController.TakeDamage(attackDamage);
            Debug.Log("ぶった");
        }
        enemyState = EnemyState.moveCoolDown;
        timer = cooldownTime;
    }

    private void CooldownState()
    {
        timer -= Time.deltaTime;
        //Debug.Log("攻撃終了クールダウン");
        if (timer <= 0)
        {
            Debug.Log("徘徊に移行");
            enemyState = EnemyState.move;
        }
    }
}
