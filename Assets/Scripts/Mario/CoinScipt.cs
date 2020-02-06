using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinScipt : MonoBehaviour {

    [SerializeField] private float killTime;
    [SerializeField] private float moveSpeed;

    void Start() {
        Destroy(gameObject, killTime);
    }

    void FixedUpdate() {
        transform.position += Vector3.up * moveSpeed;
    }
}
