using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField] private PlayerController playerToFollow;
    [SerializeField] private Vector2 offset;

    // Update is called once per frame
    void Update()
    {
        Vector3 playerPos = playerToFollow.transform.position;
        Vector2 movementDirection = playerToFollow.MovementDir;

        playerPos.z = -10;

        if(movementDirection.y != 0) playerPos.y += movementDirection.y > 0 ? offset.y : -offset.y;
        if(movementDirection.x != 0) playerPos.x += movementDirection.x > 0 ? offset.x : -offset.x;

        transform.position = playerPos;
    }
}
