/**
 * Purpose: Implementation of grapple hook ability mechanics and indicators
 * 
 * @author Sam Mellor
 **/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AGrapple : MonoBehaviour, Ability
{
    //States of the Grapple ability
    private enum GrappleState
    {
        Ready,              //Grapple unfired, ready to be used
        Firing,             //Grapple is moving towards target
        Connected,          //Grapple is connected, player is swinging
        Cooldown            //Player is not swinging, forced time-out
    }

    //Wrapping states of the Grapple
    private enum Wrapping
    {
        Wrap,               //Grapple will attach and wrap around corners it swings past
        UnWrap,             //Grapple unwrapping from previously wrapped corners
        Float               //Grapple will not swing player
    }

    //Serialized
    [SerializeField]
    private float maxGrappleDistance = 10.0f;
    [SerializeField]
    private float SwingSpeed = 20.0f;
    [SerializeField]
    private float GrappleCooldown = 0.2f;
    [SerializeField]
    private float ThrowTime = 0.2f;
    [SerializeField]
    private Sprite GrappleSprite;
    [SerializeField]
    private bool DebugRays = false;

    private GrappleState _grappleState;

    private bool _groundReset;

    private float _grappleTimer;        //Variable used as a timer in each state
    private float _swingDir;            //1 for right, -1 for left

    private bool _specialFloatExit = false;

    //Rays
    private RaycastHit2D _rayCastGrapple;
    private bool _grappleSuccess;

    //Wrapping State
    private Wrapping _wrap;

    //Storing wrapping points + radii for unwrap
    private float _radius;
    private List<float> _radii;

    private Vector3 curPivot;
    private Vector3 initPivot;
    private List<Vector3> pivotPoints;

    //Grapple Indicator
    [SerializeField]
    private GameObject Indicator;
    private SpriteRenderer indicatorRenderer;

    // Grapple Render
    private GameObject grappleSprite;
    private SpriteRenderer grappleSpriteRender;
    private Vector3 _grappSpriteMove;
    private LineRenderer _lineRender;

    [SerializeField]
    private Color RopeColor;

    void Awake()
    {
        //Core grapple
        _grappleState = GrappleState.Ready;
        _grappleSuccess = false;

        _grappleTimer = ThrowTime;

        //Wrapping
        _wrap = Wrapping.Wrap;

        _radii = new List<float>();
        pivotPoints = new List<Vector3>();

        //Indicator + Sprite
        Indicator = Instantiate(Indicator, transform.position, transform.rotation);
        indicatorRenderer = Indicator.GetComponentInChildren<SpriteRenderer>();
        indicatorRenderer.enabled = false;

        grappleSprite = new GameObject("GrappleSprite");
        grappleSpriteRender = grappleSprite.AddComponent<SpriteRenderer>();
        grappleSpriteRender.sortingLayerName = "UI";
        grappleSpriteRender.sprite = GrappleSprite;
        grappleSpriteRender.enabled = false;

        _lineRender = gameObject.AddComponent<LineRenderer>();
        _lineRender.sortingOrder = 1;
        _lineRender.startWidth = 0.2f;
        _lineRender.endWidth = 0.2f;
        _lineRender.enabled = false;

        _lineRender.material = new Material(Shader.Find("Sprites/Default"));
        _lineRender.startColor = RopeColor;
        _lineRender.endColor = RopeColor;
    }

    //Set variables for ability entrance
    public void EnterAbility()
    {
        _grappleState = GrappleState.Ready;
        _grappleTimer = ThrowTime;
        _grappleSuccess = false;

        indicatorRenderer.enabled = true;
    }

    //Set variables for ability exit
    public void ExitAbility()
    {
        //Clear wrapping points
        pivotPoints.Clear();
        _radii.Clear();

        //Hide Indicators
        _lineRender.positionCount = 0;
        _lineRender.enabled = false;
        grappleSpriteRender.enabled = false;

        _grappleState = GrappleState.Ready;
        indicatorRenderer.enabled = false;
    }

    //Execution of Grapple ability
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
        switch (_grappleState)
        {
            #region Ready State
            case GrappleState.Ready:

                //Raycast in direction of mouse position from player, to grapple distance
                _rayCastGrapple = Physics2D.Raycast(controller.transform.position, controller.PlayerToMouseDir(), maxGrappleDistance, LayerMask.GetMask("Default"));

                if (_rayCastGrapple)
                {
                    //Reveal indicator sprite at cast hit
                    grappleSpriteRender.enabled = true;
                    grappleSprite.transform.position = _rayCastGrapple.point;
                }
                else
                {
                    //Hide indicator sprite
                    grappleSpriteRender.enabled = false;
                }

                //On input
                if (InputController.Instance.Ability.Down)
                {
                    indicatorRenderer.enabled = false;

                    if (_rayCastGrapple)
                    {
                        //set initial grappling point and radius
                        initPivot = _rayCastGrapple.point;
                        curPivot = initPivot;
                        _radius = Vector3.Distance(controller.transform.position, initPivot);

                        _grappleSuccess = true;

                        //Grapple cannot be a success on death blocks
                        if (_rayCastGrapple.transform.CompareTag("Respawn"))
                        {
                            _grappleSuccess = false;
                        }
                    }
                    else
                    {
                        //Set dummy point and radius for grapple anim
                        _radius = maxGrappleDistance;
                        initPivot = controller.PlayerToMouseDir() * maxGrappleDistance;
                        initPivot += controller.transform.position;
                        _grappleSuccess = false;
                    }

                    //Set swing direction
                    if (initPivot.x >= controller.transform.position.x)
                    {
                        _swingDir = 1f;
                    }
                    else
                    {
                        _swingDir = -1f;
                    }

                    //Init grapple sprite movmement during Firing
                    grappleSprite.transform.position = controller.transform.position;
                    grappleSpriteRender.enabled = true;
                    _grappSpriteMove = (initPivot - controller.transform.position).normalized * _radius / ThrowTime;

                    //Init line render for grapple indicator
                    _lineRender.enabled = true;
                    _lineRender.positionCount = 2;
                    _lineRender.SetPosition(0, grappleSprite.transform.position);
                    _lineRender.SetPosition(1, controller.transform.position);

                    //Switch state
                    _grappleState = GrappleState.Firing;
                }
                break;
            #endregion
            #region Firing State
            case GrappleState.Firing:


                if (_grappleTimer > 0)
                {
                    _grappleTimer -= Time.deltaTime;

                    //Move sprite along direction
                    grappleSprite.transform.position += _grappSpriteMove * Time.deltaTime;

                    //Update line render to follow grapple sprite and player pos
                    _lineRender.SetPosition(0, grappleSprite.transform.position);
                    _lineRender.SetPosition(1, controller.transform.position);
                }
                else
                {
                    //If player is grounded at end of firing, do not swing
                    if (controller.grounded)
                    {
                        _grappleSuccess = false;
                    }

                    if (_grappleSuccess)
                    {
                        //Clear line render
                        _lineRender.positionCount = 0;

                        //If player is above grapple point, wrapping state Floats; otherwise wrapping state wraps
                        _wrap = Wrapping.Wrap;
                        if (initPivot.y <= controller.transform.position.y)
                        {
                            
                            _wrap = Wrapping.Float;
                            //Special case where float will exit into wrap
                            _specialFloatExit = true;
                            //reverse swing direction to swing correct direction when wrap state kicks in
                            _swingDir *= -1;
                        }

                        _grappleTimer = GrappleCooldown;

                        //Set Line Points
                        pivotPoints.Add(initPivot);
                        pivotPoints.Add(controller.transform.position);

                        SetLinePos();

                        _radius = Vector3.Distance(controller.transform.position, initPivot);
                        _radii.Add(_radius);

                        //Indicators
                        _lineRender.enabled = true;
                        grappleSpriteRender.enabled = true;

                        //State
                        _grappleState = GrappleState.Connected;
                    }
                    else
                    {
                        ResetGrapple();
                    }
                }
                break;
            #endregion
            #region Connected State
            case GrappleState.Connected:

                //Always wrap and unwrap, swing by ?


                
                //Switch on swinging states
                switch (_wrap)
                {
                    #region Wrap
                    case Wrapping.Wrap:

                        //Move the player
                        controller.SetVelocity(Swing(controller) * SwingSpeed);

                        //Wrap around obstructing obstacles
                        RaycastHit2D pivotHit = Physics2D.Linecast(controller.transform.position, curPivot, LayerMask.GetMask("Default"));
                        if (pivotHit)
                        {
                            if (new Vector3(pivotHit.point.x, pivotHit.point.y, 0f) != curPivot)
                            {
                                curPivot = pivotHit.point;
                                _radius = Vector3.Distance(controller.transform.position, curPivot);
                                _radii.Add(_radius);
                                pivotPoints.Insert(pivotPoints.Count - 1, pivotHit.point);
                                SetLinePos();
                            }
                        }

                        //always exits to float when player goes above latest wrap point
                        if (controller.transform.position.y > curPivot.y)
                        {
                            _wrap = Wrapping.Float;
                        }
                        break;
                    #endregion
                    #region Unwrap
                    case Wrapping.UnWrap:

                        //Move the player
                        controller.SetVelocity(Swing(controller) * SwingSpeed);

                        //_pivotPoints.Count is adjusted by -2 to ignore: initial grapple point and player position

                        //If we have extra pivot points
                        if (pivotPoints.Count > 2)
                        {
                                //direction between last and second last
                            Vector3 aPivot = pivotPoints[pivotPoints.Count - 2];
                            Vector3 bPivot = pivotPoints[pivotPoints.Count - 3];

                            Vector3 pivDirection = (aPivot - bPivot).normalized;
                            Vector3 plaDirection = (controller.transform.position - aPivot).normalized;
                            //2D cross product
                            float cross = pivDirection.x * plaDirection.y - pivDirection.y * plaDirection.x;

                            if ((cross > 0 && _swingDir > 0) || (cross < 0 && _swingDir < 0))
                            {
                                //drop point
                                pivotPoints.RemoveAt(pivotPoints.Count - 2);
                                curPivot = pivotPoints[pivotPoints.Count - 2];
                                _radii.RemoveAt(_radii.Count - 1);
                                _radius = _radii[_radii.Count - 1];

                                SetLinePos();
                            }
                        }

                        //Based on swing direction, enter wrap state when passing the pivot point
                        if (_swingDir < 0)
                        {
                            if (controller.transform.position.x < pivotPoints[0].x)
                            {
                                _wrap = Wrapping.Wrap;
                            }
                        }
                        else
                        {
                            if (controller.transform.position.x > pivotPoints[0].x)
                            {
                                _wrap = Wrapping.Wrap;
                            }
                        }
                     
                        break;
                    #endregion
                    #region Float
                    case Wrapping.Float:

                        //Clamp player x movement while attached in float
                        float wallmax = curPivot.x + _radius;
                        float wallmin = curPivot.x - _radius;

                        //overwrite player movement to clamp
                        Vector2 move = controller.velocity * Time.deltaTime;
                        if (controller.transform.position.x + move.x < wallmin || controller.transform.position.x + move.x > wallmax)
                        {
                            controller.SetHorizontalVelocity(0);
                        }

                        //Passing over top pivot cuts line
                        if ((_swingDir > 0 && controller.transform.position.x < pivotPoints[0].x) || (_swingDir < 0 && controller.transform.position.x > pivotPoints[0].x))
                        {
                            _lineRender.enabled = false;
                            grappleSpriteRender.enabled = false;
                            _grappleState = GrappleState.Cooldown;
                        }

                        //Exits to UnWrap except for special case
                        if (controller.transform.position.y < curPivot.y)
                        {
                            _swingDir *= -1;
                            _wrap = Wrapping.UnWrap;

                            if (_specialFloatExit)
                            {
                                _wrap = Wrapping.Wrap;
                                _specialFloatExit = false;
                            }
                        }
                        break;
                        #endregion
                }
                

                //Update player linerender point
                _lineRender.SetPosition(pivotPoints.Count - 1, controller.transform.position);
                //_lineRender.SetPosition(_pivotPoints.Count, controller.transform.position);

                //Cancel grapple swing
                //
                if (controller.grounded || InputController.Instance.Jump.Down || InputController.Instance.Ability.Down || controller.sidecollide || controller.ceiling)
                {
                    _lineRender.enabled = false;
                    grappleSpriteRender.enabled = false;
                    _grappleState = GrappleState.Cooldown;
                }

                break;

            #endregion
            #region Cooldown State
            case GrappleState.Cooldown:
                if (_grappleTimer > 0)
                {
                    _grappleTimer -= Time.deltaTime;
                }
                else
                {
                    ResetGrapple();
                }
                break;
                #endregion
        }
    }

    /*
     * Calculates swing trajectory
     */
    private Vector3 Swing(PlayerController controller)
    {
        
        //Perpendicular to Attached Point
        Vector2 perpendicular = new Vector2(curPivot.x, curPivot.y) - new Vector2(controller.transform.position.x, controller.transform.position.y);
        Vector2 move = (Vector2.Perpendicular(perpendicular) * -_swingDir).normalized;

        //Adjust for Drift (i.e. pull player in to fixed distance)
        //Calculate Direction back to Pivot Point
        Vector3 distAdjust = curPivot - (new Vector3(move.x, move.y, 0f) + controller.transform.position);

        //Calculate Actual Point
        Vector3 pointToActual = -distAdjust.normalized * _radius;

        //Direction from Player to Actual Point
        Vector3 actualMove = pointToActual + curPivot - controller.transform.position;

        if (DebugRays)
        {
            Debug.DrawRay(controller.transform.position, new Vector3(move.x, move.y, 0f), Color.cyan);
            Debug.DrawRay(new Vector3(move.x, move.y, 0f) + controller.transform.position, distAdjust.normalized, Color.green);
            Debug.DrawRay(curPivot, pointToActual.normalized, Color.yellow);
            Debug.DrawRay(controller.transform.position, actualMove * SwingSpeed, Color.white);
        }
      
        return actualMove;
    }

    /*
     * Resets grapple variables for state switch
     */
    private void ResetGrapple()
    {
        //Clear points
        pivotPoints.Clear();
        _radii.Clear();

        _grappleTimer = ThrowTime;

        //Indicators
        _lineRender.enabled = false;
        indicatorRenderer.enabled = true;
        grappleSpriteRender.enabled = false;

        _grappleState = GrappleState.Ready;
    }

    /*
     * Set line renderer points and positions
     */
    private void SetLinePos()
    {
        _lineRender.positionCount = pivotPoints.Count;
        _lineRender.SetPositions(pivotPoints.ToArray());
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