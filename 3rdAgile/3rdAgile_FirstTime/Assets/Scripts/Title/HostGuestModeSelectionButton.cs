// -----------------------------------------------------------------------------------
// ホストモード、ゲストモードそれぞれボタンを押したときの処理
// BasicSpawner.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using Fusion;
using TMPro;
using UnityEngine;

public class HostGuestModeSelectionButton : MonoBehaviour
{
    [SerializeField] private TMP_InputField roomNameInput = null;
    [SerializeField] private NetworkGameStarter networkRunner = null;

    private void Start()
    {
        Debug.Log("ｋｋｋ");
        roomNameInput.enabled = false;
    }

    // ホストモードのUIのスタートボタンにアタッチするメソッド
    public void HostModeStartButton()
    {
        roomNameInput.enabled = true;

        string roomName = roomNameInput.text;
        // ホストとしてゲームを開始する
        // GameMode.Host を指定すると、このプレイヤーがセッションのホストになる
        networkRunner.StartGame(Fusion.GameMode.Host, roomName);
    }

    // ゲストモードのUIのスタートボタンにアタッチするメソッド
    public void GuestModeStartButton()
    {
        roomNameInput.enabled = true;

        string roomName = roomNameInput.text;
        // ホストとしてゲームを開始する
        // GameMode.Host を指定すると、このプレイヤーがセッションのホストになる
        networkRunner.StartGame(GameMode.Client, roomName);
    }
}
