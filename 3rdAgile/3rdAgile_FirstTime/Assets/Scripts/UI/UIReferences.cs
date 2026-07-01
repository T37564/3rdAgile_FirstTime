// -----------------------------------------------------------------------------------
// UIオブジェクトの参照を管理するクラス
// UIReferences.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using UnityEngine;

public class UIReferences : SingletonMonobehaviour<UIReferences>
{
    [Header("タイトル画面")]
    [SerializeField] private GameObject titleUI = null;

    [Header("仮想キーボード")]
    [SerializeField] private GameObject virtualKeyboardUI = null;

    [Header("ロビー画面")]
    [SerializeField] private GameObject lobbyUI = null;

    [Header("ロード画面")]
    [SerializeField] private GameObject loadingUI = null;

    public GameObject TitleUI => titleUI;
    public GameObject VirtualKeyboardUI => virtualKeyboardUI;
    public GameObject LobbyUI => lobbyUI;
    public GameObject LoadingUI => loadingUI;
}
