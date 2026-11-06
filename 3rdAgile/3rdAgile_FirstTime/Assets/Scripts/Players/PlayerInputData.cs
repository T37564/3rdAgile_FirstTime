//=========================================================================
// 制作者：スズキ
//=========================================================================

using UnityEngine;
using Fusion;

namespace Network.Player
{
    public struct PlayerInputData : INetworkInput
    {
        public Vector2 move;

        // ボタンが押されているか
        public NetworkBool interactPressed;

        // 長押しが成立しているか
        public NetworkBool interactHeld;
    }
}
