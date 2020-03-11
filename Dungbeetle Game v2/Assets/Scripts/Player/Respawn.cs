/**
 * @author: Sam Mellor
 **/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    [SerializeField]
    private Vector3 respawnPos = new Vector3(0, 0, 0);

    public void CheckPointTrigger(Vector3 checkPointPos)
    {
        respawnPos = checkPointPos;
    }

    public void RespawnCharacter()
    {
        transform.position = respawnPos;

        Player player = GetComponent<Player>();
        player.curAbility.DeathReset();
        player.curAbility.ExitAbility();
        player.curAbility.EnterAbility();

        PlayerController controller = GetComponent<PlayerController>();
        controller.SetVelocity(Vector2.zero);
    }
}
