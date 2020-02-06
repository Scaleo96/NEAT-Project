using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerUp {

    private Sprite sprite;

    public PowerUp(Sprite image) {
        sprite = image;
    }

    public void Use(PlayerManager player) {
        player.GetComponent<SpriteRenderer>().sprite = sprite;
        player.AddHealth(1);
    }
}

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private int currentHealth = 1;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float collisionTolorance;
    [SerializeField] private float collisionForce;

    private NEATAgent agent;
    private Rigidbody2D rb2d;
    private Sprite orgSprite;
    private SpriteRenderer spriteRend;
    private PowerUp currentPowerup;
    private int score;

    private float distance;
    private Vector2 startPos;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NEATAgent>();
        rb2d = GetComponent<Rigidbody2D>();
        spriteRend = GetComponent<SpriteRenderer>();
        orgSprite = spriteRend.sprite;
        startPos = transform.position;
        distance = 0;
    }

    private void FixedUpdate() {
        distance = transform.position.x - startPos.x;
        agent.SetFitness(Mathf.FloorToInt(distance) + (score / 100));
    }

    public void PlayerHit()
    {
        currentHealth--;

        if(currentHealth < 2)
            spriteRend.sprite = orgSprite;

        if (currentHealth < 1)
        {
            PlayerDeath();
        }
    }

    public void PlayerDeath()
    {
        GetComponent<NEATAgent>().TrainingOver();
    }

    public void AddHealth(int amount) {
        currentHealth += amount;
    }

    public void SetPowerup(PowerUp pUp) {
        currentPowerup = pUp;
        pUp.Use(this);
    }

    public void addScore(int value) {
        score += value;
        scoreText.text = "Score: " + score.ToString();
        agent.SetFitness(Mathf.FloorToInt(distance) + (score / 100));
    }

    private void OnCollisionEnter2D(Collision2D other) {
        //collision whit enemy
        if(((1 << other.gameObject.layer) & enemyLayer) != 0) {

            if(other.transform.position.y + collisionTolorance < rb2d.position.y) {
                EnemyHealth eh = other.gameObject.GetComponent<EnemyHealth>();
                eh.DealDamage(1);
                addScore(eh.deathScore);

                Vector2 forceDir = Vector2.Reflect(rb2d.velocity.normalized, Vector2.up);
                rb2d.velocity = Vector2.zero;
                rb2d.AddForce(forceDir * collisionForce, ForceMode2D.Impulse);

                return;
            }

            PlayerHit();
        }           
    }
}
