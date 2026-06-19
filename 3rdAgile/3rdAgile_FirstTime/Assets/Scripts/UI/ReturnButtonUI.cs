using Fusion;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class ReturnButtonUI : MonoBehaviour
{
    [Header("ローディング時に表示するUI")]
    [SerializeField] private GameObject loadingCnavas = null;



    private NetworkRunner runner = null;

    public Button hostButton = null;
    public Button clientButton = null;

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

    /// <summary>
    /// ホストがチーム解散ボタンを押した際に発動
    /// </summary>
    private async void HostClickedRooDisbanded()
    {


        // ローディングUIを表示する
        loadingCnavas.SetActive(true);

        await Task.Delay(5000);

        NetworkGameStarter.Instance.ShutdownRunner();
    }

    /// <summary>
    /// クライアントがチームを抜けるボタンを押した際に発動
    /// </summary>
    private void ClientClickedRooDisbanded()
    {
        NetworkGameStarter.Instance.ShutdownRunner();
    }
}
