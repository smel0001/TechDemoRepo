/**
 * @author Sam Mellor
 **/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACloud : MonoBehaviour, Ability
{
    //States of the Cloud ability
    private enum CloudState
    {
        Ready,
        Active,
        Cooldown
    }

    [SerializeField]
    private float CloudCD = 1f;
    [SerializeField]
    private float LifeTime = 3f;

    private CloudState _cloudState;

    //Indicators
    [SerializeField]
    private GameObject Indicator;
    private SpriteRenderer indicatorRenderer;
    private BoxCollider2D cloudCollider;

    private float _cloudTimer;          //Variable used as a timer in each state

    private bool _groundReset;


    void Awake()
    {
        Indicator = Instantiate(Indicator, transform.position, transform.rotation);
        indicatorRenderer = Indicator.GetComponent<SpriteRenderer>();
        cloudCollider = Indicator.GetComponent<BoxCollider2D>();
        indicatorRenderer.enabled = false;
        cloudCollider.enabled = false;

        _cloudTimer = LifeTime;

        _cloudState = CloudState.Ready;
    }

    public void EnterAbility() {}

    //Cloud stays active when ability exited
    public void ExitAbility()
    {  
        if (_cloudState == CloudState.Ready)
        {
            indicatorRenderer.enabled = false;
        }
    }

    public void Activate(PlayerController player)
    {

        switch(_cloudState)
        {
            case CloudState.Ready:

                //get mouse position
                Vector3 mousePos2D = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos2D.z = 0;
                //set cloud position to mouse
                Indicator.transform.position = mousePos2D;

                indicatorRenderer.enabled = true;
                //make indicator opaque when cloud isn't out
                indicatorRenderer.color = new Color(1, 1, 1, 0.5f);

                if (InputController.Instance.Ability.Down)
                {
                    _cloudState = CloudState.Active;
                }
                break;
        }
    }

    //Tracks when cloud should decay regardless of ability selection
    public void GroundCheck(PlayerController controller)
    {
        if (controller.grounded)
        {
            _groundReset = true;
        }

        switch (_cloudState)
        {
            #region Active
            case CloudState.Active:

                cloudCollider.enabled = true;
                indicatorRenderer.color = new Color(1, 1, 1, Mathf.Lerp(0, 1, (_cloudTimer / 3.0f)));

                if (_cloudTimer > 0)
                {
                    _cloudTimer -= Time.deltaTime;
                }
                else
                {
                    _cloudTimer = CloudCD;
                    _cloudState = CloudState.Cooldown;
                }
                break;
            #endregion
            #region Cooldown
            case CloudState.Cooldown:

                indicatorRenderer.enabled = false;
                cloudCollider.enabled = false;

                if (_cloudTimer > 0)
                {
                    _cloudTimer -= Time.deltaTime;
                }
                else
                {
                    _cloudTimer = LifeTime;
                    _cloudState = CloudState.Ready;
                }
                break;
            #endregion
        }
    }

    //reset cloud variables on player death
    public void DeathReset()
    {
        indicatorRenderer.enabled = false;
        cloudCollider.enabled = false;
        _cloudTimer = LifeTime;
        _cloudState = CloudState.Ready;
    }

}
