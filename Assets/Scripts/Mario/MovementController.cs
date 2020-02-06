using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour {

    [Header("General settings")]
    [SerializeField] private float speed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float raycastLength;
    [SerializeField] private LayerMask ground;

    [Header("AI settings")]
    [SerializeField] private bool isAI;

    private Rigidbody2D rb2d;
    private NEATAgent agent;

    private bool jump;

    void Start() {
        rb2d = GetComponent<Rigidbody2D>();
        agent = GetComponent<NEATAgent>();
        jump = false;
    }

    void Update() {

        if(isAI) {
            if(agent.GetNEAT() != null) {
                float[] output = agent.GetOutputs();

                float dir = output[0];

                if(dir != 0)
                    Move(dir);
                else if(rb2d.velocity.y < 0.1f)
                    rb2d.velocity = new Vector2(0, rb2d.velocity.y);

                if(output[1] > 0.5f)
                    jump = true;
            }
        } else {

            float dir = Input.GetAxis("Horizontal");

            if(dir != 0)
                Move(dir);
            else if(rb2d.velocity.y < 0.1f)
               rb2d.velocity = new Vector2(0, rb2d.velocity.y);

            if(Input.GetButton("Jump"))
                jump = true;
                
        }
    }

    private void FixedUpdate() {
        if(jump) 
            Jump();
    }

    private void Move(float dir) {
        transform.localScale = new Vector3(Mathf.Sign(dir), 1, 1);

        if( rb2d.velocity.x < maxSpeed && dir > 0 || rb2d.velocity.x > -maxSpeed && dir < 0)
            rb2d.AddForce(Vector2.right * dir * speed, ForceMode2D.Force);
    }

    private void Jump() {
        RaycastHit2D cast = Physics2D.Raycast(rb2d.position, Vector2.down * raycastLength, raycastLength, ground);

        if(cast.collider != null)
            rb2d.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        jump = false;
    }
}
