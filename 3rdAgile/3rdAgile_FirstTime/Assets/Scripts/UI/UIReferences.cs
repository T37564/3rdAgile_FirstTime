using UnityEngine;

public class UIReferences : SingletonMonobehaviour<UIReferences>
{
    [SerializeField] private GameObject titleUI = null;
    [SerializeField] private GameObject virtualKeyboardUI = null;
    [SerializeField] private GameObject lobbyUI = null;
    [SerializeField] private GameObject loadingUI = null;

    public GameObject TitleUI => titleUI;
    public GameObject VirtualKeyboardUI => virtualKeyboardUI;
    public GameObject LobbyUI => lobbyUI;
    public GameObject LoadingUI => loadingUI;

}
