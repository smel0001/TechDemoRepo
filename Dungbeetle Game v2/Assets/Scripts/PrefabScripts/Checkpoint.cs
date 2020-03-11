/**
 * @author: Sam Mellor
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            Respawn playerRespawn = col.gameObject.GetComponent<Respawn>();
            playerRespawn.CheckPointTrigger(transform.position);
        }
    }
}
