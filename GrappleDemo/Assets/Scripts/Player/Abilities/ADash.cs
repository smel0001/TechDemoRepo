/**
 * Purpose: Implementation of dash ability mechanics and indicators
 * 
 * @author Sam Mellor
 **/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ADash : MonoBehaviour, Ability
{
    //States of the Dash
    private enum DashState
    {
        Ready,              //Dash unfired, ready to be used
        Dashing,            //Player currently dashing
        Cooldown            //Player not dashing forced timeout
    }

    [SerializeField]
    private float DashSpeed = 10f;
    [SerializeField]
    private float DashLength = 0.2f;
    [SerializeField]
    private float DashCD = 0.5f;

    private DashState _dashState;
    private float _dashTime;
    private Vector2 _dashDir;
    private float _initHorizontalV = 0f;

    private bool _groundReset;

    //Indicators
    [SerializeField]
    private GameObject Indicator;
    private SpriteRenderer indicatorRenderer;


    void Awake()
    {
        Indicator = Instantiate(Indicator, transform.position, transform.rotation);
        indicatorRenderer = Indicator.GetComponentInChildren<SpriteRenderer>();
        indicatorRenderer.enabled = false;
    }

    //Set variables for ability entrance
    public void EnterAbility()
    {
        _dashState = DashState.Ready;
        _dashTime = DashLength;
        _dashDir = Vector2.zero;

        indicatorRenderer.enabled = true;
    }

    //Set variables for ability exit
    public void ExitAbility()
    {
        _dashState = DashState.Ready;
        _dashTime = DashLength;

        indicatorRenderer.enabled = false;
    }

    //Execution of Dash ability
    public void Activate(PlayerController controller)
    {

        //TODO functionise indicator updates
        #region Update Indicator
        Vector2 mousedir = controller.PlayerToMouseDir();
        float angle = Mathf.Atan2(mousedir.y, mousedir.x) * Mathf.Rad2Deg;

        Indicator.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        Indicator.transform.position = controller.transform.position;
        #endregion

        //Switch on core state
        switch (_dashState)
        {
            #region Ready
            case DashState.Ready:
                if (_groundReset)
                {
                    if (InputController.Instance.Ability.Down)
                    {
                        //remove current vertical velocity
                        if (controller.velocity.y < 0f)
                        {
                            controller.SetVerticalVelocity(0f);
                        }

                        //save horizontal velocity
                        _initHorizontalV = controller.velocity.x;

                        _dashDir = controller.PlayerToMouseDir();
                        _dashState = DashState.Dashing;
                        _groundReset = false;
                    }
                }
                break;
            #endregion
            #region Dashing
            case DashState.Dashing:
                if (_dashTime > 0)
                {
                    //Dash vector
                    Vector2 dashmove = _dashDir * DashSpeed;

                    //set vertical
                    controller.SetVerticalVelocity(dashmove.y);
                    //set horizontal additively
                    controller.SetHorizontalVelocity(dashmove.x + _initHorizontalV);

                    _dashTime -= Time.deltaTime;
                }
                else
                {
                    _dashTime = DashCD;
                    _dashState = DashState.Cooldown;
                }
                break;
            #endregion
            #region Cooldown
            case DashState.Cooldown:
                if (_dashTime > 0)
                {
                    _dashTime -= Time.deltaTime;
                }
                else
                {
                    _dashTime = DashLength;
                    _dashState = DashState.Ready;
                }
                break;
            #endregion
        }
    }


    public void GroundCheck(PlayerController controller)
    {
        if (controller.grounded)
        {
            _groundReset = true;
        }
    }

    public void DeathReset() {}
}


