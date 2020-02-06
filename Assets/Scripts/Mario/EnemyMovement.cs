using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour {

    [SerializeField] private bool faceLeft;
    [SerializeField] private float speed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float rayLen;
    [SerializeField] private LayerMask ground;

    private Rigidbody2D rb2d;
    public bool isActive { get; set; }

    void Start() {
        rb2d = GetComponent<Rigidbody2D>();
        ChangeDriection();
        isActive = false;
    }

    private void FixedUpdate() {
        if(isActive) {
            Collider2D cast = Physics2D.OverlapArea(rb2d.position, rb2d.position + Vector2.right * transform.localScale.x * rayLen, ground);
            Debug.DrawRay(rb2d.position, Vector2.right * transform.localScale.x * rayLen, Color.green);

            if(cast != null) {
                faceLeft = !faceLeft;
                ChangeDriection();
            }

            if(Mathf.Abs(rb2d.velocity.x) < maxSpeed)
                rb2d.AddForce(Vector2.right * transform.localScale.x * speed);
        }
    }

    private void ChangeDriection() {
        transform.localScale = faceLeft ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1);
    }

}
