using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public BoxCollider2D boxCollider;

    private float aspect;
    private Camera thisCamera;

    private void Start()
    {
        thisCamera = GetComponent<Camera>();
        aspect = thisCamera.aspect;
    }

    void LateUpdate()
    {
        float x, y;
        float cameraHalfSizeY = thisCamera.orthographicSize;
        float cameraHalfSizeX = cameraHalfSizeY * aspect;
        x = Mathf.Clamp(player.position.x, boxCollider.bounds.min.x + cameraHalfSizeX, boxCollider.bounds.max.x - cameraHalfSizeX);
        y = Mathf.Clamp(player.position.y, boxCollider.bounds.min.y + cameraHalfSizeY, boxCollider.bounds.max.y - cameraHalfSizeY);
        transform.position = new Vector3(x, y, transform.position.z);
    }
}
