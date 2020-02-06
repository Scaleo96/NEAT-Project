using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    [SerializeField] private Transform followObj;
    [SerializeField] private Vector2 CameraBoundsX;
    [SerializeField] private Vector2 CameraBoundsY;
    
    void Update() {
        float posX = Mathf.Clamp(followObj.transform.position.x, CameraBoundsX.x, CameraBoundsX.y);
        float posY = Mathf.Clamp(followObj.transform.position.y, CameraBoundsY.x, CameraBoundsY.y);
        transform.position = new Vector3(posX, posY, transform.position.z);
    }
}
