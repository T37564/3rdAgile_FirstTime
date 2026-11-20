using Fusion;
using UnityEngine;
using Network.Player;

public class DungeonMapPlayerMarker : MonoBehaviour
{
    [Header("Map References")]
    [SerializeField] private DungeonMapGenerater mapGenerator;

    [SerializeField] private RectTransform playerMarker;

    private Transform player;

    private void Start()
    {
        // 先にSpawnされたプレイヤーを取得する
        if (player == null)
        {
            FindLocalPlayer();
        }

        if (playerMarker != null)
        {
            playerMarker.gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (mapGenerator == null || playerMarker == null) return;

        // プレイヤーがまだ見つからない場合
        if (player == null)
        {
            FindLocalPlayer();

            if (player == null)
            {
                playerMarker.gameObject.SetActive(false);
                return;
            }
        }

        // マップ生成完了待ち
        if (!mapGenerator.generated)
        {
            playerMarker.gameObject.SetActive(false);
            return;
        }

        if (!playerMarker.gameObject.activeSelf)
        {
            playerMarker.gameObject.SetActive(true);
        }

        // 位置更新
        playerMarker.anchoredPosition = mapGenerator.WorldToMapPosition(player.position);

        // 向き更新
        playerMarker.localRotation = Quaternion.Euler( 0f, 0f, -player.eulerAngles.y);
    }

    public void SetPlayer(Transform target)
    {
        player = target;
    }

    private void FindLocalPlayer()
    {
        PlayerController[] controllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (PlayerController controller in controllers)
        {
            NetworkObject networkObject = controller.Object;

            if (networkObject == null || !networkObject.IsValid) continue;

            if (!networkObject.HasInputAuthority) continue;

            SetPlayer(controller.transform);
            return;
        }
    }
}