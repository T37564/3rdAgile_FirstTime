using Fusion;
using UnityEngine;

public class RoomEntranceController : NetworkBehaviour
{
    [System.Flags]
    public enum EntranceDirection
    {
        None = 0,
        North = 1 << 0,
        South = 1 << 1,
        East = 1 << 2,
        West = 1 << 3
    }

    [System.Serializable]
    private class EntrancePair
    {
        public GameObject entrance;
        public GameObject wall;

        public void SetOpen(bool isOpen)
        {
            if (entrance != null)
                entrance.SetActive(isOpen);

            if (wall != null)
                wall.SetActive(!isOpen);
        }
    }

    [Header("Entrance Objects")]
    [SerializeField] private EntrancePair north;
    [SerializeField] private EntrancePair south;
    [SerializeField] private EntrancePair east;
    [SerializeField] private EntrancePair west;

    [Networked]
    public int OpenDirections { get; set; }

    private int appliedDirections = -1;

    public override void Spawned()
    {
        ApplyEntrances();
    }

    public override void Render()
    {
        if (appliedDirections == OpenDirections)
            return;

        ApplyEntrances();
    }

    public void SetOpenDirections(EntranceDirection directions)
    {
        if (!Object.HasStateAuthority)
            return;

        OpenDirections = (int)directions;

        ApplyEntrances();
    }

    private void ApplyEntrances()
    {
        EntranceDirection directions =
            (EntranceDirection)OpenDirections;

        north.SetOpen(
            (directions & EntranceDirection.North) != 0);

        south.SetOpen(
            (directions & EntranceDirection.South) != 0);

        east.SetOpen(
            (directions & EntranceDirection.East) != 0);

        west.SetOpen(
            (directions & EntranceDirection.West) != 0);

        appliedDirections = OpenDirections;
    }
}