using UnityEngine;
using Fusion;

public class DeliveryBoxArrived : NetworkBehaviour
{
    // プレイヤーIDを保持するためのNetworkedプロパティ
    [Networked] public PlayerRef Carrier { get; set; }

    private void OnTriggerEnter(Collider other)
    {
        // ネットワークオブジェクトの状態を管理するための権限を確認
        if (!Object.HasStateAuthority)
        {
            return;
        }

        if(other.CompareTag("DeliveryBox"))
        {
            Delivered();
        }
    }

    private void Delivered()
    {
        // Carrier（プレイヤーID）から、そのプレイヤーの実体（GameObject）を取得
        var playerObject =Runner.GetPlayerObject(Carrier);

        Runner.Despawn(Object);
    }
}
