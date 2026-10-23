using Fusion;
using Network.Player;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    move,// 敵が移動できるかの状態
    attackReady,//攻撃前の予備動作
    attack,//攻撃中
    moveCoolDown // 敵が攻撃後の硬直状態
}
public class AttackAction : NetworkBehaviour
{
    // プレイヤーに与えるダメージ
    private readonly int ATTACK_DAMAGE = 1;

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

    [Header("攻撃範囲オブジェクト")]
    [SerializeField] private Renderer attackRangeObject;

    [Networked] public NetworkBool isVisible { get; set; } = false;

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
        // 開始時に攻撃範囲オブジェクトを非表示にする
        if (attackRangeObject != null)
        {
            attackRangeObject.enabled = false;
        }

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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcGuardianAttackAnimation()
    {
        animator.SetTrigger("IsAttack");
    }

    /// <summary>
    /// ホスト側とクライアント側で攻撃範囲オブジェクトの表示・非表示を同期する処理
    /// </summary>
    /// <param name="visible"></param>
    [Rpc(RpcSources.StateAuthority,RpcTargets.All)]
    private void RpcSetVisibleAttackRange(bool visible)
    {
        if (attackRangeObject != null)
        {
            attackRangeObject.enabled = visible;
        }
    }

    /// <summary>
    /// 攻撃範囲内にいる、攻撃可能なプレイヤーを取得する
    /// </summary>
    private List<PlayerController> GetAttackablePlayers()
    {
        // 攻撃範囲内のプレイヤーを格納するリスト
        List<PlayerController> playersInRange = new List<PlayerController>();

        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject playerObject in playerObjects)
        {
            if (playerObject == null) continue;

            PlayerController playerController = playerObject.GetComponent<PlayerController>();

            if (playerController == null) continue;

            // スタートエリア内のプレイヤーには攻撃しない
            if (playerController.IsInStartArea) continue;

            // 敵とプレイヤーの距離を計算
            float distance = Vector3.Distance(transform.position, playerObject.transform.position);

            // 攻撃範囲内ならリストに追加
            if (distance <= attackDistance)
            {
                playersInRange.Add(playerController);
            }
        }
        return playersInRange;
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

        // 攻撃範囲内のプレイヤーを取得
        List<PlayerController> playersInRange = GetAttackablePlayers();

        // プレイヤーがスタートエリア内なら攻撃しない
        if (guardianController.currentPlayer != null)
        {
            PlayerController playerController =
                guardianController.currentPlayer.GetComponent<PlayerController>();

            if (playerController != null && playerController.IsInStartArea)
            {
                return;
            }
        }

        // 攻撃範囲内にプレイヤーがいれば攻撃準備へ移行
        if (playersInRange.Count > 0)
        {
            Debug.Log($"攻撃準備：対象プレイヤー数 = {playersInRange.Count}");
            attackTimer = TickTimer.CreateFromSeconds(Runner, attackReadyTime);

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

        // 攻撃範囲オブジェクトを表示して攻撃範囲を可視化する
        RpcSetVisibleAttackRange(true);

        // 攻撃のターゲットにしているプレイヤーの方を見る
        transform.LookAt(guardianController.currentPlayer);

        isMoveNetworked = false;
        isIdleNetworked = true;

        // 攻撃対象のプレイヤーを取得
        PlayerController playerController = null;

        // 攻撃範囲内のプレイヤーを再確認
        List<PlayerController> playersInRange = GetAttackablePlayers();

        if (guardianController.currentPlayer != null)
        {
            playerController =
                guardianController.currentPlayer.GetComponent<PlayerController>();
        }

        // プレイヤーがスタートエリア内に入ったら攻撃をキャンセル
        if (playerController != null && playerController.IsInStartArea)
        {
            Debug.Log("プレイヤーがスタートエリアに入ったため、攻撃をキャンセル");
            RpcSetVisibleAttackRange(false);
            navMeshAgent.isStopped = false;
            enemyState = EnemyState.move;

            return;
        }

        // 予備動作中プレイヤーが一定以上の距離から離れたら徘徊する
        // 誰も範囲内にいなければ攻撃をキャンセル
        if (playersInRange.Count == 0)
        {
            Debug.Log("攻撃範囲内にプレイヤーがいないため、攻撃をキャンセル");

            RpcSetVisibleAttackRange(false);
            navMeshAgent.isStopped = false;
            enemyState = EnemyState.move;

            return;
        }

        // 予備動作時間が経過したら攻撃状態に移行
        if (attackTimer.Expired(Runner))
        {
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
        RpcGuardianAttackAnimation();
    }

    public void AttackAnimationFinished()
    {
        if (!Object.HasStateAuthority) return;

        // 攻撃が命中する時点で範囲内のプレイヤーを取得
        List<PlayerController> playersInRange = GetAttackablePlayers();

        // 攻撃範囲内のプレイヤーにダメージを与える
        foreach (PlayerController player in playersInRange)
        {
            if (player == null) continue;

            player.TakeDamage(ATTACK_DAMAGE);

            Debug.Log($"攻撃命中：{player.gameObject.name}");
        }

        Debug.Log($"攻撃終了：命中人数 = {playersInRange.Count}");

        attackStarted = false;
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
        RpcSetVisibleAttackRange(false);
        
        if (attackTimer.Expired(Runner))
        {
            enemyState = EnemyState.move;
        }
    }

    public void SetObjectVisible(bool visible)
    {
        isVisible = visible;
    }
}
