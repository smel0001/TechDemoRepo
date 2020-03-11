/**
 * Purpose: Handles player input detection
 * 
 * @author Sam Mellor
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    //Create instance
    public static InputController Instance
    {
        get { return s_Instance; }
    }
    protected static InputController s_Instance;

    //Control Scheme Encoding
    public InputButton Jump = new InputButton(KeyCode.Space);
    public InputButton Wheel = new InputButton(KeyCode.Mouse1);
    public InputButton Ability = new InputButton(KeyCode.Mouse0);
    public InputAxis Horizontal = new InputAxis(KeyCode.D, KeyCode.A);
    public InputMouse Mouse = new InputMouse();
    public InputScroll Scroll;

    private bool m_FixedUpdatePassed;

    [SerializeField]
    private float scrollScale = 0.1f;

    void Awake()
    {
        Scroll = new InputScroll(scrollScale);

        if (s_Instance == null)
        {
            s_Instance = this;
        }
    }

    void Update()
    {
        RefreshInputs();

        m_FixedUpdatePassed = false;
    }

    void FixedUpdate()
    {
        m_FixedUpdatePassed = true;
    }

    void RefreshInputs()
    {
        Jump.Refresh(m_FixedUpdatePassed);
        Wheel.Refresh(m_FixedUpdatePassed);
        Ability.Refresh(m_FixedUpdatePassed);
        Horizontal.Refresh();
        Mouse.Refresh();
        Scroll.Refresh();
    }

}


public class InputButton
{
    public KeyCode key;
    public bool Down;
    public bool Held;
    public bool Up;

    //track inputs between fixed updates
    bool m_AfterFixedUpdateDown;
    bool m_AfterFixedUpdateHeld;
    bool m_AfterFixedUpdateUp;

    //Constructor
    public InputButton(KeyCode key)
    {
        this.key = key;
    }

    //Update the Button
    public void Refresh(bool fixedUpdatePassed)
    {
        if (fixedUpdatePassed)
        {
            Down = Input.GetKeyDown(key);
            Held = Input.GetKey(key);
            Up = Input.GetKeyUp(key);

            m_AfterFixedUpdateDown = Down;
            m_AfterFixedUpdateHeld = Held;
            m_AfterFixedUpdateUp = Up;
        }

        Down = Input.GetKeyDown(key) || m_AfterFixedUpdateDown;
        Held = Input.GetKey(key) || m_AfterFixedUpdateHeld;
        Up = Input.GetKeyUp(key) || m_AfterFixedUpdateUp;

        m_AfterFixedUpdateDown |= Down;
        m_AfterFixedUpdateHeld |= Held;
        m_AfterFixedUpdateUp |= Up;
    }
}

public class InputAxis
{
    public KeyCode positive;
    public KeyCode negative;
    public float Value;

    //Constructor
    public InputAxis(KeyCode positive, KeyCode negative)
    {
        this.positive = positive;
        this.negative = negative;
    }

    //Update the Axis
    public void Refresh()
    {
        bool positiveHeld = false;
        bool negativeHeld = false;

        positiveHeld = Input.GetKey(positive);
        negativeHeld = Input.GetKey(negative);

        Value = 0f;
        if (positiveHeld & negativeHeld)
        {
            Value = 0f;
        }
        else if (positiveHeld)
        {
            Value = 1f;
        }
        else if (negativeHeld)
        {
            Value = -1f;
        }

    }
}

public class InputMouse
{
    public float mouseX;
    public float mouseY;
    public Vector2 mousePos;

    public void Refresh()
    {
        //Unity mouse position
        mouseX = Input.mousePosition.x;
        mouseY = Input.mousePosition.y;
        mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
    }
}

public class InputScroll
{
    private float scale;
    public float delta;

    public InputScroll(float _scale)
    {
        scale = _scale;
    }

    public void Refresh()
    {

        delta = Input.mouseScrollDelta.y;
    }
}