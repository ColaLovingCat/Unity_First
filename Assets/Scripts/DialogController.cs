using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogController : MonoBehaviour
{
    public GameObject dialog;

    private void OnTriggerEnter2D(Collider2D other)
    {
         if (other.tag == "player")
        {
            dialog.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "player")
        {
            dialog.SetActive(false);
        }
    }
}
