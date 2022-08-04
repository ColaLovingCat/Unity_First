using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionController : MonoBehaviour
{
    public GameObject collectedEffect;
    public string ItemType;
    public int ItemScore = 100;

    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider2D;

    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider2D = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "player")
        {
            _spriteRenderer.enabled = false;
            _collider2D.enabled = false;
            //
            Debug.Log(ItemType);
            switch (ItemType)
            {
                case "Score":
                    {
                        GameController.Instance.totalScore += ItemScore;
                        GameController.Instance.UpdateTotalScore();
                        break;
                    }
                case "Health":
                    {
                        break;
                    }
                case "Bullet":
                    {
                        break;
                    }
            }
            //
            Destroy(gameObject, 0.2f);
        }
    }
}
