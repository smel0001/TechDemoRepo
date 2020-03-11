/**
 * Purpose: Implementation of ability switching
 * 
 * @author Sam Mellor
 **/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySelect : MonoBehaviour
{
    //States of Slow
    private enum SlowState
    {
        Ready,      //Charged and ready to slow
        Active,     //Time is being slowed
        Charge      //Slow is recharging
    }

    [SerializeField]
    private float slowFactor = 0.4f;

    private float ScrollPoint = 0;
    private int oldSelect = 0;

    public float slowTimer = 1f;
    private float curSlowTimer;

    //Link to Player in Scene
    [SerializeField]
    private Player Player;
    private PlayerController _playerController;

    private List<Ability> _abilitiesT;

    //Indicators
    public Image HighlightImage;
    public Image SlowImage;

    public Slider slowSlider;
    public Image sliderFill;

    private SlowState slowstate = SlowState.Ready;


    void Awake()
    {
        _playerController = Player.GetComponent<PlayerController>();

        _abilitiesT = new List<Ability>();
        _abilitiesT.AddRange(GetComponents<Ability>());

        curSlowTimer = slowTimer;
    }

    private void Start()
    {
        Player.curAbility = _abilitiesT[0];
        Player.SetCurrentAbility(_abilitiesT[0]);

        SlowImage.enabled = false;
        slowSlider.value = slowTimer;
    }

    void Update()
    {
        //Select ability
        ScrollPoint += InputController.Instance.Scroll.delta * 0.5f;
        if (ScrollPoint < 0f)
        {
            ScrollPoint = 2.99f;
        }
        else if (ScrollPoint > 2.99f)
        {
            ScrollPoint = 0f;
        }

        if (Input.GetKeyDown("1"))
        {
            ScrollPoint = 0;
        }
        else if (Input.GetKeyDown("2"))
        {
            ScrollPoint = 1;
        }
        else if (Input.GetKeyDown("3"))
        {
            ScrollPoint = 2;
        }

        int roundedSelect = (int)Mathf.Floor(ScrollPoint);

        if (roundedSelect != oldSelect)
        {
            oldSelect = roundedSelect;
            switch (roundedSelect)
            {
                case 0:
                    HighlightImage.rectTransform.localPosition = new Vector3(-32f, 0f);
                    break;
                case 1:
                    HighlightImage.rectTransform.localPosition = new Vector3(0f, 0f);
                    break;
                case 2:
                    HighlightImage.rectTransform.localPosition = new Vector3(32f, 0f);
                    break;
            }

            Player.SetCurrentAbility(_abilitiesT[roundedSelect]);
        }

        //Slow
        switch(slowstate)
        {
            case SlowState.Ready:
                if (InputController.Instance.Wheel.Down)
                {
                    SlowImage.enabled = true;
                    Time.timeScale = slowFactor;
                    slowstate = SlowState.Active;
                }
                break;

            case SlowState.Active:
                curSlowTimer -= Time.deltaTime;
                slowSlider.value = curSlowTimer;
                if (InputController.Instance.Wheel.Up || curSlowTimer < 0f)
                {
                    SlowImage.enabled = false;
                    Time.timeScale = 1f;
                    sliderFill.color = Color.red;
                    slowstate = SlowState.Charge;
                }
                break;

            case SlowState.Charge:
                curSlowTimer += Time.deltaTime;
                slowSlider.value = curSlowTimer;
                if (curSlowTimer > slowTimer)
                {
                    curSlowTimer = slowTimer;
                    sliderFill.color = Color.green;
                    slowstate = SlowState.Ready;
                }
                break;
        }

        //Non selected update
        foreach (Ability ability in _abilitiesT)
        {
            ability.GroundCheck(_playerController);
        }
    }
}