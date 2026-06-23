using Fusion;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class ReturnButtonUI : NetworkBehaviour
{
    [Header("ローディング時に表示するUI")]
    [SerializeField] private GameObject loadingCnavas = null;



    private NetworkRunner runner = null;

    public Button hostButton = null;
    public Button clientButton = null;

    public VisualElement messageLog = null;

    private void OnEnable()
    {
        runner = NetworkGameStarter.Instance.networkRunner;

        if (runner == null) return;

        // ホストのみ
        if (runner.IsServer)
        {
            hostButton.style.display = DisplayStyle.Flex;
        }
        else// ゲストのみ
        {
            clientButton.style.display = DisplayStyle.Flex;
        }

        // イベント登録
        hostButton.clicked += HostClickedRooDisbanded;
        clientButton.clicked += ClientClickedRooDisbanded;
    }

    private void OnDisable()
    {
        // イベント登録解除
        hostButton.clicked -= HostClickedRooDisbanded;
        clientButton.clicked -= ClientClickedRooDisbanded;
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_DisbandRoom()
    {
        _ = DisbandRoom();
    }

    private async Task DisbandRoom()
    {
        // ホストのみ
        if (runner.IsServer)
        {
            loadingCnavas.SetActive(true);

            await Task.Delay(5000);
        }
        else// ゲストのみ
        {
            messageLog.style.display = DisplayStyle.Flex;

            await Task.Delay(2000);

            loadingCnavas.SetActive(true);

            await Task.Delay(2000);
        }

        NetworkGameStarter.Instance.ShutdownRunner();
    }

    private async Task DisbandRoomClient()
    {
        loadingCnavas.SetActive(true);

        await Task.Delay(2000);

        NetworkGameStarter.Instance.ShutdownRunner();
    }


    /// <summary>
    /// ホストがチーム解散ボタンを押した際に発動
    /// </summary>
    private void HostClickedRooDisbanded()
    {
        RPC_DisbandRoom();
    }

    /// <summary>
    /// クライアントがチームを抜けるボタンを押した際に発動
    /// </summary>
    private void ClientClickedRooDisbanded()
    {
        DisbandRoomClient();
    }
}
