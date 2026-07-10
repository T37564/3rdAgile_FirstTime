using UnityEngine;

public class EnemySpawerNoNetwork : MonoBehaviour
{
    [SerializeField] public GameObject enemyObject;

    private GameObject guardianRoom;

    private GameObject guardianRoomInstance;


    private Transform guardianRoomTransform;

    private bool acquisitionRoom = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!acquisitionRoom)
        {
            GameObject enemyRoom = GameObject.FindWithTag("EnemyRoom");
            //Vector3 roomPosition = guardianRoomInstance.transform.position;
        }
    }

    private void SpawnRoom(GameObject roomPrefab,Vector3 position)
    {
        GameObject room = Instantiate(roomPrefab, position, Quaternion.identity);

        if (roomPrefab == enemyObject)
        {
            guardianRoomInstance = room;
        }
    }
}
