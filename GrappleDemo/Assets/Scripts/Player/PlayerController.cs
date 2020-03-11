/**
 * @author Sam Mellor
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private int numHorizontalRays = 5;
    [SerializeField]
    private int numVerticalRays = 5;
    [SerializeField]
    private float rayOffset = 0.1f;
    [SerializeField]
    private LayerMask PlatformsMask;

    private Transform _transform;
    private BoxCollider2D _collider;
    private Vector3 localScale;

    private Vector3 rayTopRight;
    private Vector3 rayBottomRight;
    private Vector3 rayBottomLeft;
    private Vector3 rayTopLeft;
    private RaycastHit2D raycastHit;
    private float horizontalRayGaps;
    private float verticalRayGaps;

    [System.NonSerialized]
    public Vector2 velocity;

    [System.NonSerialized]
    public bool grounded;
    [System.NonSerialized]
    public bool ceiling;
    [System.NonSerialized]
    public bool sidecollide;

    


    void Awake()
    {
        _transform = transform;
        _collider = GetComponent<BoxCollider2D>();
        localScale = transform.localScale;
        grounded = false;
        ceiling = false;
        sidecollide = false;

        horizontalRayGaps = (_collider.size.y * Mathf.Abs(localScale.y) - rayOffset) / (numHorizontalRays - 1);
        verticalRayGaps = (_collider.size.x * Mathf.Abs(localScale.x) - rayOffset) / (numVerticalRays - 1);
    }

    //Expectation is that this is called every frame in player
    public void Move()
    {
        grounded = false;
        ceiling = false;
        sidecollide = false;

        Vector2 deltaVelocity = velocity * Time.deltaTime;

        //Collision Cast
        RayOrigins();
        HorizontalRays(ref deltaVelocity);
        VerticalRays(ref deltaVelocity);

        _transform.Translate(deltaVelocity);

        if (grounded)
            velocity.y = 0;

        if (ceiling)
            if (velocity.y > 0)
                velocity.y = 0;
    }

    #region Rays
    void RayOrigins() {
        Vector2 size = new Vector2(_collider.size.x * Mathf.Abs(localScale.x), _collider.size.y * Mathf.Abs(localScale.y));

        rayTopRight = _transform.position + new Vector3(size.x/2, size.y/2, 0f);
        rayBottomRight = _transform.position + new Vector3(size.x/2, size.y/-2, 0f);
        rayBottomLeft = _transform.position + new Vector3(size.x/-2, size.y/-2, 0f);
        rayTopLeft = _transform.position + new Vector3(size.x/-2, size.y/2, 0f);
    }

    void HorizontalRays(ref Vector2 deltaVelocity) {
        //direction
        bool movingRight = true;
        if (deltaVelocity.x < 0f)
        {
            movingRight = false;
        }

        Vector3 rayOrigin = movingRight ? rayBottomRight : rayBottomLeft;
        Vector2 rayDir = movingRight ? Vector2.right : -Vector2.right;

        float rayDist = Mathf.Abs(deltaVelocity.x);

        rayOrigin.y += rayOffset/2;


        for (int i = 0; i < numHorizontalRays; i++) 
        {
            Vector3 ray = new Vector3 (rayOrigin.x, rayOrigin.y + i * horizontalRayGaps, 0f);

            //Debug.DrawRay(ray, rayDir * rayDist, Color.green);
            raycastHit = Physics2D.Raycast(ray, rayDir, rayDist, PlatformsMask);
            if (raycastHit)
            {
                //Shrink delta movement to new dist
                deltaVelocity.x = raycastHit.point.x - rayOrigin.x;

                sidecollide = true;
            }
        }
    }

    void VerticalRays(ref Vector2 deltaVelocity) {
        //direction
        bool movingUp = true;
        if (deltaVelocity.y < 0f)
        {
            movingUp = false;
        }

        Vector3 rayOrigin = movingUp ? rayTopRight : rayBottomRight;
        Vector2 rayDir = movingUp ? Vector2.up : -Vector2.up;

        float rayDist = Mathf.Abs(deltaVelocity.y);

        rayOrigin.x -= rayOffset / 2;
        rayOrigin.x += deltaVelocity.x;


        for (int i = 0; i < numVerticalRays; i++)
        {
            Vector3 ray = new Vector3(rayOrigin.x - i * verticalRayGaps, rayOrigin.y, 0f);

            raycastHit = Physics2D.Raycast(ray, rayDir, rayDist, PlatformsMask);
            if (raycastHit)
            {
                //Shrink delta movement to new dist
                deltaVelocity.y = raycastHit.point.y - rayOrigin.y;

                if (!movingUp)
                {
                    grounded = true;
                }
                else
                {
                    ceiling = true;
                }
            }
        }
    }
    #endregion

    #region VelocityMutators
    public void SetVelocity(Vector2 v)
    {
        velocity = v;
    }

    public void AddForce(Vector2 f)
    {
        velocity += f * Time.deltaTime;
    }

    public void SetHorizontalVelocity(float v)
    {
        velocity.x = v;
    }

    public void AddHorizontalForce(float f)
    {
        velocity.x += f * Time.deltaTime;
    }

    public void SetVerticalVelocity(float v)
    {
        velocity.y = v;
    }

    public void AddVerticalForce(float f)
    {
        velocity.y += f * Time.deltaTime;
    }

    public void ApplyGravity(float f)
    {
        velocity.y += f * Time.deltaTime;
    }

    public void HorizontalDecelerate(float f)
    {
        if (velocity.x > 0f)
        {
            velocity.x -= f * Time.deltaTime;
            if (velocity.x < 0f)
            {
                velocity.x = 0f;
            }
        }
        else if (velocity.x < 0f)
        {
            velocity.x += f * Time.deltaTime;
            if (velocity.x > 0f)
            {
                velocity.x = 0f;
            }
        }
    }
    #endregion


    /*
     * Get 2D vector that describes the direction from the player to the mouse position
     */
    public Vector2 PlayerToMouseDir()
    {
        Vector3 mouse = new Vector3(InputController.Instance.Mouse.mouseX, InputController.Instance.Mouse.mouseY, 0f);
        mouse = Camera.main.ScreenToWorldPoint(mouse);
        mouse.z = 0f;

        Vector3 dir = (mouse - transform.position).normalized;
        Vector2 dir2D = new Vector2(dir.x, dir.y);

        return dir2D;
    }
}
