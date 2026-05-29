using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "Game/UIAssetData")]
public class UIAssetData : ScriptableObject
{
    [Header("Title UI")]
    public VisualTreeAsset titleUI;

    [Header("VirtualKeyboard UI")]
    public VisualTreeAsset VirtualKeyboardUI;

    [Header("Lobby UI")]
    public VisualTreeAsset lobbyUI;

    [Header("MainGameScenes UI")]
    public VisualTreeAsset mainGameScenesUI;

    [Header("Score UI")]
    public VisualTreeAsset scoreUI;
}
