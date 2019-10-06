using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Tilemap tilemap;

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
        x = Mathf.Clamp(player.position.x, tilemap.localBounds.min.x + cameraHalfSizeX, tilemap.localBounds.max.x - cameraHalfSizeX);
        y = Mathf.Clamp(player.position.y, tilemap.localBounds.min.y + cameraHalfSizeY, tilemap.localBounds.max.y - cameraHalfSizeY);
        transform.position = new Vector3(x, y, transform.position.z);
    }
}
