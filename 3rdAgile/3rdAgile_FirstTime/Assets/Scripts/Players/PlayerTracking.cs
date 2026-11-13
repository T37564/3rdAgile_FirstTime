using UnityEngine;
using Network.Player;

public class PlayerTracking : MonoBehaviour
{
    // TPS視点のオフセット
    private readonly Vector3 TPS_OFFSET = new Vector3(0f, 5f, -7f);

    // FPS視点のオフセット
    private readonly Vector3 FPS_OFFSET = new Vector3(0f, 1.6f, 0f);

    [Header("自分のプレイヤーのTransform")]
    [SerializeField] private Transform target = null;

    [Header("視点切り替えにかかる時間")]
    [SerializeField] private float transitionDuration = 0.5f;

    [SerializeField] private float mouseSensitivity = 0.1f;

    private float pitch = 0f;

    // 現在の視点
    private bool isFPS = false;

    // 視点切り替えの補間値（0 = TPS、1 = FPS）
    private float viewBlend = 0f;

    private PlayerInputHandler inputHandler = null;

    /// <summary>
    /// 追従対象を設定
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        UnregisterInputEvent();

        target = newTarget;

        isFPS = false;
        viewBlend = 0f;

        if (target == null) return;

        inputHandler = target.GetComponent<PlayerInputHandler>();

        if (inputHandler != null)
            inputHandler.SetCamera(transform);

        RegisterInputEvent();
    }

    /// <summary>
    /// イベント登録
    /// </summary>
    private void RegisterInputEvent()
    {
        if (inputHandler == null) return;

        inputHandler.OnViewSwitch += SwitchView;
    }

    /// <summary>
    /// イベント登録解除
    /// </summary>
    private void UnregisterInputEvent()
    {
        if (inputHandler == null) return;

        inputHandler.OnViewSwitch -= SwitchView;
        inputHandler = null;
    }

    /// <summary>
    /// 視点切り替え
    /// </summary>
    private void SwitchView()
    {
        isFPS = !isFPS;

        if(isFPS)
        {
            pitch = 0f;
        }

        inputHandler?.SetViewMode(isFPS);

        Cursor.lockState = isFPS ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isFPS;
    }

    private void Update()
    {
        if (!isFPS || inputHandler == null) return;

        Vector2 look = inputHandler.LookInput;

        pitch -= look.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -80f, 80f);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 目標の補間値
        float targetBlend = isFPS ? 1f : 0f;

        // 一定速度で補間値を変化させる
        viewBlend = Mathf.MoveTowards(viewBlend, targetBlend, Time.deltaTime / Mathf.Max(transitionDuration, 0.001f));

        // TPS視点の位置
        Vector3 tpsPosition = target.position + TPS_OFFSET;

        // FPS視点の位置
        Vector3 fpsPosition = target.TransformPoint(FPS_OFFSET);

        // TPS視点の回転
        Quaternion tpsRotation = Quaternion.LookRotation(target.position - tpsPosition, Vector3.up);

        // FPS視点の回転
        Quaternion fpsRotation = target.rotation * Quaternion.Euler(pitch, 0f, 0f);

        // 位置を補間
        transform.position = Vector3.Lerp(tpsPosition, fpsPosition, viewBlend);

        // 回転を補間
        transform.rotation = Quaternion.Slerp(tpsRotation, fpsRotation, viewBlend);
    }

    private void OnDestroy()
    {
        UnregisterInputEvent();
    }
}