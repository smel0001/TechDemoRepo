/**
 * @author Sam Mellor
 **/

public interface Ability
{
    void EnterAbility();
    void ExitAbility();

    void Activate(PlayerController player);
    void GroundCheck(PlayerController player);
    void DeathReset();
}