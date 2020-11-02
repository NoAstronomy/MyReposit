using System.Collections;
using System.Collections.Generic;
using CurveGrowData;
using r_BloodDATA;
using UnityEditor;
using UnityEngine;

public class BloodController : MonoBehaviour
{
    //public GameObject player;
    [Header("Default Attack")]
    public float defaultAttackSpeed;

   // public ParticleSystem splashParticles;
    private int currentTentacle;
    private int tentacle;
    //private int side;
    //private int tentacleMod;

    public List<ScriptableObject> tentacles;

    [Header("Tentacle")]
    public CurveGrowManager cManager;
    public pointInfoCollection pCollection;
    public Vector3 startPointBias;
    public Vector3 endPointBias;
    public float timeBeforeDecay;
    public float growTimer;
    public float decayTimer;
    public float lrWidthMult;
    public float shaderSpeedPlus;
    public float shaderSpeedMinus;
    public AnimationCurve growCurve;
    public AnimationCurve decayCurve;
    public AnimationCurve endWidthCurve;
    public Material material;
 

    [Header("Blood")]
    public r_BloodDATA.BloodManager bloodManager;
   // public ParticleSystem bloodParticle;
    public WeaponDATABASE weapon;
    private List<ParticleCollisionEvent> particlesCollision;

   // [Header("Blood Size")]
   // [Range(0, 1)]
   // private float bloodSize;

    void Awake()
    {
        bloodManager = new r_BloodDATA.BloodManager(gameObject);  
    }

    void Start()
    {
        Cursor.visible = false;
        currentTentacle = -1;
       /* foreach (var a in bloodManager.container.player.GetComponentsInChildren<Transform>())
        {
            if (a.name == "SPL")
                splashParticles = a.GetComponent<ParticleSystem>();
        }

        splashParticles.Stop();*/

        cManager = new CurveGrowManager(gameObject);

        //bloodManager.container.splashParticles = splashParticles;
        particlesCollision = new List<ParticleCollisionEvent>();

        bloodManager.weaponManager.current.UseWeaponFromPrefab(weapon);
        bloodManager.bloodAttack.AddAllEnemy();


        bloodManager.TryAttack += (args) =>
        {
            if (((List<UnitHost>) args[0]).Count == 0)
                return false;
            return true;
        };

        bloodManager.OnAttack += (args) =>
        {
            if (((List<UnitHost>)args[0]).Count == 0)
                return false;

            currentTentacle = (currentTentacle < 2) ? currentTentacle + 1 : 0;
            //int side = (currentTentacle < 11) ? currentTentacle + 1 : 0;

            switch (currentTentacle)
            {
                case 0:
                    tentacle = Random.Range(0, 3);
                    break;

                case 1:
                    tentacle = Random.Range(4, 7);
                    break;

                case 2:
                    tentacle = Random.Range(8, 11);
                    break;
            }
            //Debug.Log("currentTentacle = " + currentTentacle);
            //int tentacleMod = Random.Range(1, 4);
           // Debug.Log("tentacleMod = " + tentacleMod);
           // tentacle = tentacleMod * currentTentacle - 1;
          //  Debug.Log("tentacle = " + tentacle);

            List <UnitHost> unitHost = (List<UnitHost>) args[0];

            Vector3 offset = Vector3.zero;
            //Debug.Log("unit pos =" + offset);
            /* + new Vector3(
                 0.25f * Random.value * (Random.value > 0.5
                                         ? -1
                                         : 1),
                 0.25f * Random.value * (Random.value > 0.5
                                         ? -1
                                         : 1)
             );*/

            cManager.CurveGrowRealizer.shootTentacle(
                new Tentacle(
                    (Tentacle_Params)tentacles[tentacle], 
                    gameObject.transform,
                    unitHost[0].transform,
                    startPointBias,
                    offset,
                    timeBeforeDecay,
                    growTimer,
                    decayTimer,
                    1f,
                    shaderSpeedMinus,
                    shaderSpeedPlus,
                    0,
                    material
                    )
                );
            return true;
        };

        /* bloodManager.OnAttackDefault += (args) =>
        {
            if (((List<UnitHost>)args[0]).Count == 0)
                return false;
           // Debug.Log(123);

             currentTentacle = (currentTentacle < 2) ? currentTentacle + 1 : 0;

             List<UnitHost> unitHost = (List<UnitHost>)args[0];

             Vector3 offset = Vector3.zero;
             //Debug.Log("unit pos =" + offset);
                             // + new Vector3(
                              //    0.25f * Random.value * (Random.value > 0.5
                                                  //        ? -1
                                                 //         : 1),
                                  //0.25f * Random.value * (Random.value > 0.5
                                       //                   ? -1
                                      //                    : 1)
                             // );

             cManager.CurveGrowRealizer.shootTentacle(
                 new Tentacle(
                     (Tentacle_Params)tentacles[currentTentacle],
                     gameObject.transform,
                     unitHost[0].transform,
                     startPointBias,
                     offset,
                     timeBeforeDecay,
                     growTimer,
                     decayTimer,
                     1f,
                     material
                 )
             );
             //Debug.Break();
            return true;
        };*/
    }

    void Update()
    {
       // bloodManager.container.bloodSize = bloodSize;
        SumBtnPressed();
        //bloodParticle.transform.position = gameObject.transform.position;
        bloodManager.bloodControll.Blood();
        //UpdateSplashPartiles();
        bloodManager.container.defaultAttackSpeed = defaultAttackSpeed;
    }

    void FixedUpdate()
    {
    }

    public void SumBtnPressed()
    {
        //bloodManager.container.ctrlPressed = Input.GetKeyDown(KeyCode.LeftControl);
        bloodManager.container.lkmPressed = Input.GetKeyDown(KeyCode.Mouse0);
        bloodManager.container.pkmPressed2 = Input.GetKeyDown(KeyCode.Mouse1);


        bloodManager.container.pkmPressed = (Input.GetKeyDown(KeyCode.Mouse1))
                                                  ? ((bloodManager.container.pkmPressed == true)
                                                     ? false
                                                     : true)
                                                  : bloodManager.container.pkmPressed;

       // if(Input.GetKeyDown(KeyCode.Mouse0))
            //Debug.Log("CLICK");

        bloodManager.container.qPressed = Input.GetKeyDown(KeyCode.Q);
        bloodManager.container.ePressed = Input.GetKeyDown(KeyCode.E);
        bloodManager.container.sPressed = Input.GetKey(KeyCode.S);
        bloodManager.container.wPressed = Input.GetKey(KeyCode.W);
        bloodManager.container.tabPressed = Input.GetKeyDown(KeyCode.Tab);
    }

   /* public void UpdateSplashPartiles()
    {
        if (bloodManager.container.currentSplash)
            splashParticles.Play();
        else
            splashParticles.Stop();
    }*/

}
