using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    protected Animator _animator;
    protected AudioSource _deathAudio;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        _animator = GetComponent<Animator>();
        _deathAudio = GetComponent<AudioSource>();
    }

    public void Death()
    {
        Destroy(gameObject);
    }

    public void Jumpon()
    {
        _deathAudio.Play();
        _animator.SetTrigger("triDeath");
    }
}
