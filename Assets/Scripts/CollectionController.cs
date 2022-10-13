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
                case "Score": // ���ӻ���
                    {
                        GameController.Instance.totalScore += ItemScore;
                        GameController.Instance.RefreshGameInfos();
                        break;
                    }
                case "Health": // ����Ѫ��
                    {
                        GameController.Instance.UpdateHealth(ItemScore);
                        GameController.Instance.RefreshGameInfos();
                        break;
                    }
                case "Bullet": // ���ӵ�ҩ��
                    {
                        break;
                    }
            }
            // ������Ʒ
            Destroy(gameObject, 0.2f);
        }
    }
}
