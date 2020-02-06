using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestBoxScript : MonoBehaviour {

    [SerializeField] private GameObject pUp;
    [SerializeField] private GameObject coin;
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private LayerMask playerLayer;

    [Header("Box settings")]
    [SerializeField] private bool isCoin;
    [SerializeField] private int coins;
    [SerializeField] private float collisionTolorance;

    [Header("Score settings")]
    [SerializeField] private int coinsValue;

    private bool isTaken;
    private SpriteRenderer spriteRend;

    private void Start() {
        isTaken = false;
        spriteRend = GetComponent<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if(((1 << other.gameObject.layer) & playerLayer) != 0 && transform.position.y > other.gameObject.transform.position.y + collisionTolorance) {
            PlayerManager pm = other.gameObject.GetComponent<PlayerManager>();

            if(isCoin && coins > 0) {
                Instantiate(coin, transform.position, Quaternion.identity);
                pm.addScore(coinsValue);
                coins--;

                if(coins <= 0)
                    spriteRend.sprite = emptySprite;
            }

            if(!isCoin && !isTaken) {
                Instantiate(pUp, transform.position, Quaternion.identity);
                isTaken = true;
                spriteRend.sprite = emptySprite;
            } 
        }
    }

}
