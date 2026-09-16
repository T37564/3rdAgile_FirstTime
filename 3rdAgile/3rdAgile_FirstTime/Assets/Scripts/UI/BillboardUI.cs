using UnityEngine;

/// <summary>
/// UIをカメラ正面の角度に向けるためのクラス
/// </summary>
public class BillboardUI : MonoBehaviour
{
    private Camera mainCamera = null;

    /// <summary>
    /// メインカメラの取得
    /// </summary>
    private void Start()
    {
        mainCamera = Camera.main;
    }

    /// <summary>
    /// カメラの角度調整
    /// </summary>
    private void LateUpdate()
    {
        if (mainCamera == null) return;

        // カメラとUIの距離測定
        Vector3 direction = mainCamera.transform.position - transform.position;
        direction.y = 0;

        // UIの角度をカメラ正面に向ける
        transform.rotation = Quaternion.LookRotation(-direction);
    }
}
