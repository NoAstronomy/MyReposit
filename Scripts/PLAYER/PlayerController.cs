using System.Collections;
using System.Collections.Generic;
using AnimationData;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public ControllDATA.ControllManager controllManager;
    public AnimationData.AnimationManager animManager;
    public AnimationData.AnimManagerWithBlood animManagerWithBlood;
    public BloodController bloodController;
    public UnitStats stats;
    public WeaponDATABASE weaponStats;
    public FightDATA.WeaponManager weaponManager;
    //public ControllDATA.NeckerchiefManager neckerchiefManager;

    //public GameObject sharf;
    void Awake()
    {
        controllManager = new ControllDATA.ControllManager(gameObject);
    }
    void Start()
    {

        controllManager.container.jumpDelayAfterInterval = 0;
        controllManager.container.jumpDelayBeforeInterval = 0;
        controllManager.container.dashSpeed = 4.2f;
        bloodController = GameObject.FindObjectOfType<BloodController>();
        controllManager.container.posBeforeJump = transform.position;

        animManager = new AnimationData.AnimationManager(gameObject, controllManager);
        animManagerWithBlood = new AnimManagerWithBlood(animManager, bloodController.bloodManager);
        controllManager.container.isRoll = Input.GetKeyDown(KeyCode.LeftShift);
        controllManager.container.stats = stats;
        // neckerchiefManager = new ControllDATA.NeckerchiefManager(gameObject, sharf);
        weaponManager = new FightDATA.WeaponManager(gameObject);
        weaponManager.current.UseWeaponFromPrefab(weaponStats);

        animManager.OnAttack += (args) =>
        {
            weaponManager.current.Strike(args);
        };
    }

    void Update()
    {
        SetHpToZero();
        ProcessInputs();

        if (!bloodController.bloodManager.container.isActiveCursor)
            controllManager.container.isDash = Input.GetKeyDown(KeyCode.LeftShift);

        if (bloodController.bloodManager.container.isActiveCursor)
            controllManager.container.isRoll = Input.GetKeyDown(KeyCode.LeftShift);

        controllManager.container.isSwim = false;
        controllManager.container.isSwim = !bloodController.bloodManager.container.isActiveCursor
                                           ? true
                                           : controllManager.container.isSwim;

        controllManager.container.isSwim = controllManager.container.currentGrab
                                           ? false
                                           : controllManager.container.isSwim;

        if (controllManager.container.canRollInGame)
            controllManager.movementRealization.Roll(new Vector2(transform.localScale.x, 0), stats.rollSpeedCurve);

        if (controllManager.container.canDashInGame)
            controllManager.movementRealization.Dash(new Vector2(transform.localScale.x, 0));

        //neckerchiefManager.controll.Update();
    }

    void FixedUpdate()
    {
        controllManager.movementRealization.Jumping();
        controllManager.movementRealization.Movement(stats.movementSpeed);
    }

    public bool isKillZone = false;
    public void SetHpToZero()
    {
        if (bloodController.bloodManager.container.unitHost.unit.container.nowHP <= 0 && isKillZone)
        {
            AddFullHp();
            controllManager.movementRealization.GoToBeforeJumpPoint();
            isKillZone = false;
            return;
        }

        if (isKillZone)
            bloodController.bloodManager.container.unitHost.unit.controll.Hurt = 0.5f * Time.deltaTime;
    }

    public void AddFullHp()
    {
        bloodController.bloodManager.container.unitHost.unit.container.nowHP = 1;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "KillZone")
        {
            isKillZone = true;
        }
    }

    public float hor;
    private float horLerpTime;

    void ProcessInputs()
    {
        horLerpTime = 10f;
        // hor = 0;
        //   hor = Input.GetAxis("Horizontal");
        /*  hor = (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                ? -1
                : ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                   ? 1
                   : 0
                  );*/

        /*   if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
           {
               hor -= horLerpTime * Time.deltaTime;
           }
           else 
           if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
           {
               hor += horLerpTime * Time.deltaTime;
           }
           else
           {
               hor = 0;
           }*/

        if (controllManager.container.canMoveInGame)
            hor = (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                  ? Mathf.Lerp(hor, -1, horLerpTime * Time.deltaTime)
                  : ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                     ? Mathf.Lerp(hor, 1, horLerpTime * Time.deltaTime)
                     : Mathf.Lerp(hor, 0, 2f * horLerpTime * Time.deltaTime)
                    );
        else hor = 0;

        // if (controllManager.container.currentRoll)
        //hor = 0;

        // hor = Mathf.Clamp(hor, -1f, 1f);
        // Debug.Log(hor);
        //hor = (Mathf.Abs(hor) < 0.2f && Mathf.Abs(hor) > 0) ? 1f * transform.localScale.x : hor;
        //Debug.Log(Input.GetAxis("Horizontal"));

        controllManager.container.isHorizontalMove = Input.GetKey(KeyCode.D)
                           || Input.GetKey(KeyCode.RightArrow)
                           || Input.GetKey(KeyCode.A)
                           || Input.GetKey(KeyCode.LeftArrow);

        //Input.SetAxes("Horizontal")

        controllManager.movementRealization.ChangeHorizontal(hor);
        //controllManager.movementRealization.ChangeHorizontal(Input.GetAxis("Horizontal"));


        controllManager.container.jumpHeld = Input.GetKey(KeyCode.Space)
                                             || Input.GetKey(KeyCode.UpArrow);

        controllManager.container.canDashInGame = (Input.GetKeyDown(KeyCode.F1))
                                                  ? ((controllManager.container.canDashInGame == true)
                                                     ? false
                                                     : true)
                                                  : controllManager.container.canDashInGame;

        controllManager.container.canRollInGame = (Input.GetKeyDown(KeyCode.F2))
                                                  ? ((controllManager.container.canRollInGame == true)
                                                     ? false
                                                     : true)
                                                  : controllManager.container.canRollInGame;

        controllManager.container.canGrabInGame = (Input.GetKeyDown(KeyCode.F3))
                                                  ? ((controllManager.container.canGrabInGame == true)
                                                     ? false
                                                     : true)
                                                  : controllManager.container.canGrabInGame;

        controllManager.container.canSlideInGame = (Input.GetKeyDown(KeyCode.F4))
                                                   ? ((controllManager.container.canSlideInGame == true)
                                                      ? false
                                                      : true)
                                                   : controllManager.container.canSlideInGame;

        controllManager.container.canAirDashInGame = (Input.GetKeyDown(KeyCode.F5))
                                                     ? ((controllManager.container.canAirDashInGame == true)
                                                        ? false
                                                        : true)
                                                     : controllManager.container.canAirDashInGame;

        controllManager.container.canDoubleJumpInGame = (Input.GetKeyDown(KeyCode.F6))
                                                        ? ((controllManager.container.canDoubleJumpInGame == true)
                                                           ? false
                                                           : true)
                                                        : controllManager.container.canDoubleJumpInGame;


    }

}
