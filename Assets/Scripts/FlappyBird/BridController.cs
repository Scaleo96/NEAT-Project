using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BridController : MonoBehaviour {

    [SerializeField] private float flapForce;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private LayerMask pointCollet;
    [SerializeField] private Text text;
    [SerializeField] private bool isAI;
    [SerializeField] private float aiActionDeley;

    private Rigidbody2D rb2d;
    private NEATAgent agent;
    private float cameraBounds;
    private float currenAliveTime;
    private int currentScore;


    private void Start() {
        rb2d = GetComponent<Rigidbody2D>();
        agent = GetComponent<NEATAgent>();
        cameraBounds = Camera.main.orthographicSize;
        currentScore = 0;
        currenAliveTime = 0;
        text.text = "Score: " + currentScore;

        if(isAI) {
            StartCoroutine(AIFlap());
            StartCoroutine(Counter());
        }
    }

    private void Update() {
        if(CheckDeath()) {
            if(isAI) {
                agent.SetFitness(currentScore + (int)Mathf.Ceil(currenAliveTime));
                agent.TrainingOver();
                StopAllCoroutines();
            } else {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        if(!isAI && Input.GetKeyDown(KeyCode.UpArrow)) 
            Flap();
    }

    private IEnumerator Counter() {
        while(true) {
            currenAliveTime += 1;
            agent.SetFitness(currentScore + (int)Mathf.Ceil(currenAliveTime));
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator AIFlap() {
        while(true) {
            if(agent.GetOutputs()[0] >= 0.75f)
                Flap();
            yield return new WaitForSeconds(aiActionDeley);
        }

    }

    private void FixedUpdate() {
        Rotate();
    }

    private bool CheckDeath() {
        if(rb2d.position.y <= -cameraBounds || rb2d.position.y >= cameraBounds) {
            return true;
        }

        return false;
    }

    private void Flap() {
        rb2d.velocity = new Vector2(rb2d.velocity.x, 0);
        rb2d.AddForce(Vector2.up * flapForce, ForceMode2D.Impulse);
        rb2d.rotation = -45;
    }

    private void Rotate() {
        rb2d.rotation = Mathf.MoveTowards(rb2d.rotation, -180, rotationSpeed * Mathf.Abs(rb2d.velocity.y) * 0.25f);
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if(isAI) {
            agent.SetFitness(currentScore + (int)Mathf.Ceil(currenAliveTime));
            agent.TrainingOver();
                            StopAllCoroutines();
        } else {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if( ((1 << other.gameObject.layer) & pointCollet) != 0) {
            currentScore++;
            text.text = "Score: " + currentScore;
            agent.SetFitness(currentScore + (int)Mathf.Ceil(currenAliveTime));
        }
    }

}
