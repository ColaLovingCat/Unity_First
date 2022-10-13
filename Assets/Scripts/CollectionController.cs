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
            switch (ItemType)
            {
                case "Score": // 增加积分
                    {
                        GameController.Instance.totalScore += ItemScore;
                        GameController.Instance.RefreshGameInfos();
                        break;
                    }
                case "Health": // 增加血量
                    {
                        GameController.Instance.UpdateHealth(ItemScore);
                        GameController.Instance.RefreshGameInfos();
                        break;
                    }
                case "Bullet": // 增加弹药量
                    {
                        break;
                    }
            }
            // 消除物品
            Destroy(gameObject, 0.2f);
        }
    }
}
