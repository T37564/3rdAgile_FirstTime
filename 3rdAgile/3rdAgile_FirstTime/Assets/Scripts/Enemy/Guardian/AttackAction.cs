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

    [SerializeField] private Animator animator;

    // タイマーの秒数をリセットする変数
    private float resetTimerCount = 0.0f;

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
    
    private bool attackStarted = false;

    [Networked] private bool isMoveNetworked { get; set; } = false;

    [Networked] private bool isIdleNetworked { get; set; } = false;

    [Networked] private TickTimer attackTimer { get; set; }

    public override void Spawned()
    {
        enemyState = EnemyState.move;

        guardianWanderingArea.FindWanderingGround();
    }

    private void Update()
    {
        if (Object == null) return;

        //アニメーションがnullのとき処理をしない
        if (animator == null) return;

        // isMoveNetworked=trueのとき同期して全クライアントにアニメーションを実行する
        animator.SetBool("IsMove", isMoveNetworked);

        animator.SetBool("IsIdle", isIdleNetworked);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        // EnemyStateの状態によって行う処理を分ける
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

    [Rpc(RpcSources.StateAuthority,RpcTargets.All)]
    private void RpcGuardianAttackAnimation()
    {
        animator.SetTrigger("IsAttack");
    }

    /// <summary>
    /// 敵の徘徊する目的地を渡すメソッド
    /// </summary>
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

    /// <summary>
    /// 移動処理
    /// </summary>
    private void MoveState()
    {
        // 動きを止めない
        navMeshAgent.isStopped = false;

        // 徘徊する処理
        Wandering();
        isIdleNetworked = false;

        // 0.1f以上の速さで移動していたらisMoveNetworkedはtrueになる
        isMoveNetworked = navMeshAgent.velocity.magnitude > 0.1f;
        
        //Debug.Log(guardianController.currentDistance);

        // プレイヤーとの距離が攻撃距離以下になったら攻撃準備状態に移行
        if (guardianController.currentDistance <= attackDistance)
        {
            Debug.Log("攻撃準備");

            // 攻撃の予備動作のタイマーを開始させる
            attackTimer = TickTimer.CreateFromSeconds(Runner, attackReadyTime);
            //timer -= Time.deltaTime;
            
            enemyState = EnemyState.attackReady;
            navMeshAgent.isStopped = true;
        }

    }

    /// <summary>
    /// 攻撃の予備動作をする処理
    /// </summary>
    private void AttackReadyState()
    {
        navMeshAgent.isStopped = true;
        //Debug.Log("攻撃予備動作: " + Runner.Tick);
        Debug.Log("攻撃予備動作: " + attackTimer.RemainingTime(Runner));

        // 攻撃のターゲットにしているプレイヤーの方を見る
        //transform.LookAt(guardianController.currentPlayer);

        isMoveNetworked = false;
        isIdleNetworked = true;

        // 予備動作中プレイヤーが一定以上の距離から離れたら徘徊する
        if(guardianController.currentDistance >= attackDistance)
        {
            Debug.Log("徘徊に移行");
            navMeshAgent.isStopped = false;
            enemyState = EnemyState.move;
        }

        // 予備動作時間が経過したら攻撃状態に移行
        if (attackTimer.Expired(Runner))
        {
            Debug.Log("攻撃実行");
            enemyState = EnemyState.attack;
        }
    }

    /// <summary>
    /// プレイヤーに攻撃する処理
    /// </summary>
    private void AttackState()
    {
        if (attackStarted) return;
        attackStarted = true;

        // 攻撃の実装
        // 攻撃が完了したらクールダウン状態に移行
        Debug.Log("攻撃");
        RpcGuardianAttackAnimation();

        // 攻撃対象のプレイヤーのPlayerControllerを取得
        PlayerController playerController = guardianController.currentPlayer.GetComponent<PlayerController>();

        if (playerController != null)
        {
            // プレイヤーにダメージを与える
            playerController.TakeDamage(attackDamage);
            Debug.Log("ぶった");
        }
    }

    public void AttackAnimationFinished()
    {
        if (!Object.HasStateAuthority) return;

        attackStarted = false;
        Debug.Log("攻撃終了");
        // 攻撃後一定時間攻撃できないようにする
        enemyState = EnemyState.moveCoolDown;
       　
        // 攻撃後のクールダウンタイマーを開始させる
        attackTimer = TickTimer.CreateFromSeconds(Runner, cooldownTime);
    }

    /// <summary>
    /// 攻撃後のクールダウンを開始させるメソッド
    /// </summary>
    private void CooldownState()
    {
        isIdleNetworked = true;
        
        //timer -= Time.deltaTime;
        Debug.Log($"攻撃終了クールダウン " + attackTimer.RemainingTime(Runner));
        //if (timer <= 0)
        if(attackTimer.Expired(Runner))
        {
            Debug.Log("徘徊に移行");
            enemyState = EnemyState.move;
        }
    }
}
