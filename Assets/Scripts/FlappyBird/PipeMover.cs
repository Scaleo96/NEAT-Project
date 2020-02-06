using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeMover : MonoBehaviour {

    [SerializeField] private float speed;
    public Transform destoryPos { get; set; }

    private void FixedUpdate() {
        transform.position += Vector3.left * speed * Time.deltaTime;

        if(transform.position.x < destoryPos.position.x)
            Destroy(gameObject);
    }

}
