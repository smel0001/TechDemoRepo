/**
 * Purpose: Dummy empty ability
 * 
 * @author: Sam Mellor
 **/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AEmpty : MonoBehaviour, Ability
{

    public void EnterAbility()
    {
    }

    public void ExitAbility()
    {
    }

    public void Activate(PlayerController player)
    {
    }

    public void GroundCheck(PlayerController controller)
    {
    }

    public void DeathReset()
    {}
}
