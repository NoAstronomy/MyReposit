using System;
using System.Collections;
using System.Collections.Generic;
using FightDATA;
using UnityEngine;

namespace r_BloodDATA
{
    public delegate bool BloodDelegate(object[] args);

    public class BloodManager
    {
        public CurveGrowData.CurveGrowManager curveGrowManager;
        public container container;
        public bloodControll bloodControll;
       // public bloodVisualize bloodVisualize;
        public bloodAttack bloodAttack;
        public FightDATA.WeaponManager weaponManager;

        public BloodManager(GameObject blood)
        {
            container = new container(this, blood);
            bloodControll = new bloodControll(this);
           // bloodVisualize = new bloodVisualize(this);
            bloodAttack = new bloodAttack(this);
            weaponManager = new FightDATA.WeaponManager(blood);
            
            weaponManager.container.hitPos = new GameObject("WeaponManager").transform;
        }

        public event BloodDelegate OnAttack;
        public event BloodDelegate OnAttackDefault;
        public event BloodDelegate TryAttack;
        public event BloodDelegate OnActiveCursor;
        public event BloodDelegate OnCrystalize;

        public bool CallBack(EventType type, object[] args)
        {
            switch (type)
            {
                case EventType.OnAttack:
                    if (OnAttack != null)
                        return OnAttack.Invoke(args);
                    else return false;

                case EventType.OnAttackDefault:
                    if (OnAttackDefault != null)
                        return OnAttackDefault.Invoke(args);
                    else return false;

                case EventType.TryAttack:
                    if (TryAttack != null)
                        return TryAttack.Invoke(args);
                    else return false;

                case EventType.OnActiveCursor:
                    if (OnActiveCursor != null)
                        return OnActiveCursor.Invoke(args);
                    else return false;

                case EventType.OnCrystalize:
                    if (OnCrystalize != null)
                        return OnCrystalize.Invoke(args);
                    else return false;
            }

            return false;
        }

        public enum EventType
        {
            OnAttack,
            OnAttackDefault,
            TryAttack,
            OnActiveCursor,
            OnCrystalize
        }
    }

    public class container
    {
        public ControllDATA.ControllManager controllManager;
        private BloodManager link;
        public Transform transform;
        public Transform player;
        public Rigidbody2D rigidbody;
        public GameObject splashParticles;
        public BoxCollider2D collider;
        public UnitHost unitHost;

        public BarDATA barData;

        public float bloodSize = .5f;
        public float splashInterval;

        public bool isActiveCursor;
        public bool isCristal;
        public bool isLiquid;
        public bool isAttack;
        public bool isAbsorptionHP;
        public bool isAbsorptionMP;
        public bool isAddNewBars;

        //public bool currentAttack;
        public bool currentSplash;
        public bool currentActiveCursor;
        public bool currentActiveBody;
        public bool currentDefaultAttack;
        public bool currentTentaclesAttack;
        public bool currentAttackBuffer;
        public bool currentAbsorptionHP;
        public bool currentAbsorptionMP;
        public bool currentAccumulationHP;
        public bool currentAccumulationMP;

        //public bool ctrlPressed;
        public bool pkmPressed;
        public bool pkmPressed2;
        public bool lkmPressed;
        public bool qPressed;
        public bool ePressed;
        public bool sPressed;
        public bool wPressed;
        public bool tabPressed;

        public float widthMax;
        public float widthMin;
        public float heigthMax;
        public float heigthMin;

        public float defaultAttackSpeed;
        public bool canAttack;

        public List<Vector3> bloodBallSpawnPoint;

        public container(BloodManager bloodManager, GameObject blood)
        {
            link = bloodManager;
            controllManager = GameObject.FindObjectOfType<PlayerController>().controllManager;
            player = controllManager.container.transform;
            transform = blood.transform;
            rigidbody = blood.GetComponent<Rigidbody2D>();
            unitHost = controllManager.container.unitHost;
            widthMin = GeneratorScreenData.SCREEN_WIDTH * 0.1f;
            widthMax = GeneratorScreenData.SCREEN_WIDTH * 0.9f;
            heigthMin = GeneratorScreenData.SCREEN_HEIGTH * 0.1f;
            heigthMax = GeneratorScreenData.SCREEN_HEIGTH * 0.9f;
            collider = blood.GetComponent<BoxCollider2D>();
            bloodBallSpawnPoint = new List<Vector3>();

            bloodBallSpawnPoint.Add(new Vector3(2, 2));
            bloodBallSpawnPoint.Add(new Vector3(2, 1));
            bloodBallSpawnPoint.Add(new Vector3(2, 0));
            bloodBallSpawnPoint.Add(new Vector3(1, 3));
            bloodBallSpawnPoint.Add(new Vector3(0, 3));
            bloodBallSpawnPoint.Add(new Vector3(-1, 3));
            bloodBallSpawnPoint.Add(new Vector3(-2, 2));
            bloodBallSpawnPoint.Add(new Vector3(-2, 1));
            bloodBallSpawnPoint.Add(new Vector3(-2, 0));
            
        }
    }

    public class bloodControll
    {
        private BloodManager link;

        public bloodControll(BloodManager bloodManager)
        {
            link = bloodManager;
        }


        private void BtnPressed()
        {
            link.container.isActiveCursor = (link.container.pkmPressed) 
                                            ? true 
                                            : false;


            if (link.container.controllManager.container.currentDash)
                return;

            if (link.container.controllManager.container.currentSlideOnWall)
                return;

            if (link.container.controllManager.container.currentSlowDown)
                return;

            if (link.container.controllManager.container.currentRoll)
                return;

            if (link.container.controllManager.container.currentGrab)
                return;

            if (link.container.qPressed)
                link.container.isCristal = true;

            if (link.container.controllManager.container.currentJump)
                return;

            if (link.container.controllManager.container.currentJumpOfWall)
                return;

            if (link.container.controllManager.container.currentFallenJump)
                return;

            if (link.container.controllManager.container.currentReactionToAttack)
                return;

            if (!link.container.controllManager.container.isOnGround)
                return;

            if (link.container.sPressed)
                link.container.isAbsorptionHP = true;

            if (link.container.wPressed)
                link.container.isAbsorptionMP = true;

            if (link.container.tabPressed)
                link.container.isAddNewBars = true;


        }

        public void Blood()
        {
            UpdateBooleanVar();
            BtnPressed();
            ActiveCursor();
            ActiveBody();
            CristalizationBlood(link.container.player.GetComponent<BoxCollider2D>());
            LiquidBlood(link.container.player.GetComponent<BoxCollider2D>());
            AbsorptionBlood();
            AddNewBars();
            CanActionInGame(!link.container.isAbsorptionHP && !link.container.isAbsorptionMP);
        }

        public void UpdateBooleanVar()
        {
            link.container.isAbsorptionHP = false;
            link.container.isAbsorptionMP = false;
            link.container.isAddNewBars = false;
          //  Debug.Log(link.container.currentDefaultAttack);
            link.container.currentSplash = false;
            link.container.currentAccumulationMP = false;
            link.container.currentAccumulationHP = false;
        }

        public void CanActionInGame(bool value)
        {
            link.container.controllManager.container.canMoveInGame = value;
            link.container.controllManager.container.canDashInGame = value;
            link.container.controllManager.container.canRollInGame = value;
            link.container.controllManager.container.canSlideInGame = value;
            link.container.controllManager.container.canGrabInGame = value;
            link.container.controllManager.container.canJumpInGame = value;
            link.container.controllManager.container.canJumpOfWallInGame = value;
            link.container.controllManager.container.canFallenJumpInGame = value;


        }

        private void AbsorptionBlood()
        {
            
            //Debug.Log("S pressed" + link.container.isAbsorptionHP);

            if (link.container.isAbsorptionHP)
            {
              /*  if (link.container.unitHost.unit.container.nowHP >= 1)
                {
                    for (int i = 0; i < link.container.controllManager.container.currentVolumeByRadius.Length; i++)
                    {
                        if (link.container.controllManager.container.currentVolumeByRadius[i].Liquid < 0.1f)
                            continue;
                    }

                                      
                }*/

                for (int i = 0; i < link.container.controllManager.container.currentVolumeByRadius.Length; i++)
                {
                    //if(link.container.controllManager.container.currentVolumeByRadius[i].Liquid < 0.1f)
                      //  continue;
                    link.container.unitHost.unit.controll.Heal = 0.3f * Time.deltaTime;
                    link.container.currentAccumulationHP = true;
                    
                   // SuccParticle_Logic.Go_Succ();
                    link.container.controllManager.container.currentVolumeByRadius[i].Liquid -= 40f * Time.deltaTime;
                }
            }
            else if (link.container.isAbsorptionMP)
            {

               // Debug.Log("W pressed" + link.container.isAbsorptionMP);

                if (!link.container.isActiveCursor)
                {

                    if (link.container.barData.mana[link.container.barData.currentMpBarBack].mpBack != 1)
                    {
                        link.container.unitHost.unit.controll.Hurt = 0.3f * Time.deltaTime;
                    }
                    else
                    {                      
                        link.container.splashInterval += 0.2f * Time.deltaTime;

                        if (link.container.splashInterval > 0.1f)
                        {                          
                                link.container.currentSplash = true;
                            Debug.Log("SPL");
                                link.container.unitHost.unit.controll.Hurt = 0.3f * Time.deltaTime;
                        }
                    }
                    link.container.currentAccumulationMP = true;
                    link.container.barData.GetMPBack(0.7f);
                }
                else
                {                    
                    for (int i = 0; i < link.container.controllManager.container.currentVolumeByRadius.Length; i++)
                    {
                        if (link.container.controllManager.container.currentVolumeByRadius[i].Liquid < 0)
                            continue;

                        link.container.controllManager.container.currentVolumeByRadius[i].Liquid -= 40f * Time.deltaTime;
                        //SuccParticle_Logic.Go_Succ();
                    }
                        
                }

                           
            } else if (!link.container.isAbsorptionHP || !link.container.isAbsorptionMP)
            {
                link.container.splashInterval = 0;
                //SuccParticle_Logic.Stop_Succ();
            }

        }

        private void AddNewBars()
        {
            if(link.container.isAddNewBars)
                link.container.barData.AddNewBars();
        }

        private void LiquidBlood(BoxCollider2D ignoreCol)
        {
            if (link.container.isCristal)
                return;

            if (link.container.isLiquid)
                return;
            
                link.container.rigidbody.bodyType = RigidbodyType2D.Dynamic;
                link.CallBack(BloodManager.EventType.OnActiveCursor, new object[] { (int)1 });
                Physics2D.IgnoreCollision(
                    link.container.collider,
                    ignoreCol,
                    true
                );
                link.container.isLiquid = true;
         
        }

        private void CristalizationBlood(BoxCollider2D ignoreCol)
        {
            if (!link.container.isCristal)
                return;


                // Debug.Log(1);
            if(!link.container.isActiveCursor)
                return;

           
            
            if (link.container.barData.mana[0].mpBack <= 0 && link.container.isCristal)
            {
              //  link.bloodVisualize.bloodParticle.Stop();
              //  link.bloodVisualize.bloodParticle.Clear();
                link.container.isCristal = false;
                link.container.isActiveCursor = false;
                link.container.pkmPressed = false;
                link.container.pkmPressed2 = true;
                return;
            }

            if (link.container.isLiquid)
            {
                // Debug.Log(1);
                link.CallBack(BloodManager.EventType.OnCrystalize, new object[1]);
                link.container.isLiquid = false;
            }

            link.container.barData.DownMPBack(0.5f);
            //Debug.Log("Cristal");
            link.container.rigidbody.bodyType = RigidbodyType2D.Static;

            Physics2D.IgnoreCollision(
                link.container.collider,
                ignoreCol,
                false
            );

            //link.bloodVisualize.BloodPointCristalBox();
            //link.container.pkmPressed = false;
        }

        private Vector3 pos;
        private float posX;
        private float posY;
        private Vector3 offset_screen_original;
        private Vector3 offset_screen;
        private Vector3 screen_sealed_pos;

        private void ActiveCursor()
        {
            if ((link.container.pkmPressed && link.container.pkmPressed2))
            {
                
                link.container.isCristal = false;
                link.CallBack(BloodManager.EventType.OnActiveCursor, new object[] { (int)2 });
                link.container.controllManager.container.canGrabInGame = false;
                link.container.controllManager.container.canSlideInGame = false;

                link.container.controllManager.movementRealization.UnGrab();
                if (link.container.controllManager.container.currentSlideOnWall)
                    link.container.controllManager.movementRealization.UnSlide(1);
                // Debug.Log(2);
                // link.bloodVisualize.bloodParticle.Clear();
                // link.bloodVisualize.bloodParticle.Play();
                //link.container.transform.position = link.container.player.transform.position;
                bool isSpawn = false;
                for (int i = 0; i < link.container.bloodBallSpawnPoint.Count; i++)
                {
                    RaycastHit2D hit;
                    Vector2 origin = new Vector2(
                                         link.container.player.position.x,
                                         link.container.player.position.y)
                                     + new Vector2(
                                         (link.container.bloodBallSpawnPoint[i].x - link.container.bloodSize)
                                         * link.container.player.transform.localScale.x,
                                         (link.container.bloodBallSpawnPoint[i].y + link.container.bloodSize)
                                     ); 

                    Vector2 direction = new Vector2(
                                            link.container.player.transform.position.x,
                                            link.container.player.transform.position.y)
                                        + Vector2.up * 1.1f
                                        - origin;

                    //Debug.DrawRay(origin, direction);
                    
                    float distance = Vector2.Distance(direction + origin, origin)/2;

                    hit = Physics2D.CircleCast(
                        origin,
                        link.container.bloodSize,
                        direction,
                        distance,
                        ~(LayerMask.GetMask("Sharf") + LayerMask.GetMask("Player") +
                          LayerMask.GetMask("Ignore Raycast"))
                    );

                    if (hit.collider != null)
                    {
                       // Debug.Log(hit.collider.name + " " + i);
                        continue;
                    }
                    else
                    {
                        screen_sealed_pos = Camera.main.WorldToScreenPoint(
                            link.container.player.position + new Vector3(
                                (link.container.bloodBallSpawnPoint[i].x - link.container.bloodSize)
                                * link.container.player.transform.localScale.x,
                                (link.container.bloodBallSpawnPoint[i].y + link.container.bloodSize)
                            )
                        );

                        isSpawn = true;
                       // Debug.Log(i);
                        break;
                    }
                    
                }

                if (!isSpawn)
                {
                    screen_sealed_pos = Camera.main.WorldToScreenPoint(
                        link.container.player.position + new Vector3(0, 1) * 1.1f
                    );
                }


                
            }

          //  if (!link.container.isActiveCursor)
          //  {
                // screen_sealed_pos = new Vector3(GeneratorScreenData.SCREEN_WIDTH/2, GeneratorScreenData.SCREEN_HEIGTH/2);
                /*     screen_sealed_pos = Camera.main.WorldToScreenPoint(
                         link.container.transform.position
                         + new Vector3(
                             4f * link.container.controllManager.container.transform.localScale.x,
                             0.5f,
                             0
                         )
                     );*/

                /*    Vector2Int v2Int = Grapth.GrapthTools.GetInstance()
                                             .RoundToTileCoord(link.container.player.position.x,
                                                               link.container.player.position.y);



                    Vector2 v2 = Grapth.GrapthTools.GetInstance()
                                       .TileCoordToDecardCoord(v2Int.x,
                                                               v2Int.y
                                       );
                */
 

             //   return;
          //  }

            //if (GameObject.Find("Enemy") != null && link.bloodAttack.CheckDistToEnemy(GameObject.Find("Enemy").transform, 10))
               // link.bloodAttack.Attack(GameObject.Find("Enemy").transform, "0", 30);

           // if (!link.container.isCristal)
               // link.bloodVisualize.BloodPoint();
            //else
                
 
            //var y0 = GeneratorScreenData.SCREEN_HEIGTH;
            //var x0 = GeneratorScreenData.SCREEN_WIDTH;

            offset_screen = new Vector3(
                                GeneratorScreenData.SCREEN_WIDTH,
                                GeneratorScreenData.SCREEN_HEIGTH
                            ) / 2f;
            offset_screen *= .7f;
            offset_screen_original = new Vector3(
                                         GeneratorScreenData.SCREEN_WIDTH,
                                         GeneratorScreenData.SCREEN_HEIGTH
                                     ) / 2f;

            posX = (screen_sealed_pos.x - offset_screen_original.x) + Input.GetAxis("Mouse X") * 20;
            posY = (screen_sealed_pos.y - offset_screen_original.y) + Input.GetAxis("Mouse Y") * 20;

            posX = Mathf.Clamp(posX, -offset_screen.x, offset_screen.x);
           // posY = Mathf.Clamp(posY, -offset_screen.y, offset_screen.y);

            //posY = Mathf.Abs(posY) > offset_screen.y ? (offset_screen.y * (posY / Mathf.Abs(posY))) : posY;

            posY = Mathf.Abs(posY) > Mathf.Sqrt((1 - (posX * posX) / (offset_screen.x * offset_screen.x)) *
                                                offset_screen.y * offset_screen.y)
                   ? Mathf.Sqrt((1 - (posX * posX) / (offset_screen.x * offset_screen.x)) * offset_screen.y *
                                offset_screen.y) * (posY / Math.Abs(posY))
                   : posY;

          /*  posX = Mathf.Abs(posX) >= Mathf.Sqrt((1 - (posY * posY) / (offset_screen.y * offset_screen.y)) *
                                                offset_screen.x * offset_screen.x)
                   ? Mathf.Sqrt((1 - (posY * posY) / (offset_screen.y * offset_screen.y)) * offset_screen.x *
                                offset_screen.x) * (posX / Math.Abs(posX))
                   : posX;*/


            screen_sealed_pos = new Vector3(
                                    posX,
                                    posY)
                                + offset_screen_original;

            pos = Camera.main.ScreenToWorldPoint(
                new Vector3(
                    posX,
                    posY,
                    22) + offset_screen_original
            );
            //pos = link.container.transform.position;

            

           // link.container.rigidbody.MovePosition(pos);
            link.container.rigidbody.MovePosition(Vector3.Lerp(
                                                      link.container.transform.position,
                                                      pos,
                                                      15 * Time.deltaTime)
                                                  );

            link.bloodAttack.Attack(link.bloodAttack.actualEnemy, "0", 10);
                //Vector3.Lerp(
                   // link.container.transform.position,
                  //  pos,
                  //  15 * Time.deltaTime)
            //);
            
        }

        private void ActiveBody()
        {
            if (!link.container.pkmPressed && link.container.pkmPressed2)
            {
                link.container.isCristal = false;
                link.CallBack(BloodManager.EventType.OnActiveCursor, new object[] { (int)1 });
                link.container.controllManager.container.canGrabInGame = true;
                link.container.controllManager.container.canSlideInGame = true;
               // link.bloodVisualize.bloodParticle.Clear();
               // link.bloodVisualize.bloodParticle.Play();
                //Debug.Log(3);
            }

            if (link.container.isActiveCursor)
                return;



            link.bloodAttack.FindEnemyWithDist();
           // if(link.bloodAttack.CheckDistToEnemy(link.bloodAttack.actualEnemy, 5))
            //Debug.Log(123);
            link.bloodAttack.Attack(link.bloodAttack.actualEnemy, "0", 10);
           // if (GameObject.Find("Enemy") != null &&
              //  link.bloodAttack.CheckDistToEnemy(GameObject.Find("Enemy").transform, 2))
            //{
            //    link.bloodAttack.Attack(GameObject.Find("Enemy").transform, "0", 5);
           // }

          //  link.bloodVisualize.BloodShield();

            link.container.transform.position = link.container.player.position  + Vector3.up/2;
        }
    } 

    public class bloodAttack
    {
        private BloodManager link;

        public bloodAttack(BloodManager bloodManager)
        {
            link = bloodManager;
            enemy = new List<Transform>();
            currentEnemies = new List<UnitHost>();


        }

        private List<Transform> enemy;

        public void AddAllEnemy()
        {
            
            foreach (var a in GameObject.FindObjectsOfType<UnitHost>())
            {
                if (a.name == "Enemy")
                    enemy.Add(a.transform);
                
               // Debug.Log(enemy.Count);
            }
        }

        private float minDist;
        public Transform actualEnemy;
        private int numberOfEnemy;
        public void FindEnemyWithDist()
        {
            if (actualEnemy != null)
                return;

           // Debug.Log(enemy.Count);
            if(enemy.Count < 1)
                return;

            minDist = Vector2.Distance(link.container.transform.position, enemy[0].position);
            for(int i = 0; i < enemy.Count; i++)
            {
                float minDist = Vector2.Distance(link.container.transform.position, enemy[i].position);
                if (minDist < this.minDist)
                    numberOfEnemy = i;

            }
            actualEnemy = enemy[numberOfEnemy];
            //Debug.Log(actualEnemy.position);
        }

        public Vector3 rayHitPoint;
        public bool CheckDistToEnemy(Transform enemy, float distance)
        {
            if (this.enemy.Count < 1)
                return false;
            //Debug.Log();
            if (Vector2.Distance(link.container.transform.position, enemy.position) > distance)
            {
                //actualEnemy = null;
                return false;
            }


            if (enemy == null)
                return false;



            RaycastHit2D hit = Physics2D.Raycast(
                link.container.transform.position,
                (enemy.position - link.container.transform.position).normalized,
                distance,
                ~(LayerMask.GetMask("Ignore Raycast") + LayerMask.GetMask("Player") + LayerMask.GetMask("Sharf"))
            );
            if (hit.collider == null)
            {
                //actualEnemy = null;
                return false;
            }

            rayHitPoint = hit.collider.transform.position;
            Debug.Log(hit.collider.name + " " + hit.collider.transform.position);
            bool b = (hit.collider != null &&
                      hit.collider.gameObject.transform == enemy.transform)
                     ? true
                     : false;

            Color color = (b) ? Color.green : Color.red;

            Debug.DrawRay(
                link.container.transform.position,
                (enemy.position - link.container.transform.position),
                color
            );

            return true;
        }

        public int typeAttack;
        public List<UnitHost> currentEnemies;
        public void Attack(Transform enemy, string attack, float timeSpeed)
        {
            if (this.enemy.Count < 1)
                return;

            if (!link.container.lkmPressed)
                return;

            //if (link.container.currentAttack)
            // return;

            //Debug.Log(321321);
            //if (rayHitPoint != this.actualEnemy.position)
            // return;
            //Debug.Log(rayHitPoint);

            link.weaponManager.container.hitPos.position = this.actualEnemy.transform.position;
            var actualEnemy = link.weaponManager.current.Get_HostUnderBox(attack);

            foreach (var a in actualEnemy)
            {
                if (a.gameObject.name == "Player")
                {
                    actualEnemy.Remove(a);
                    break;
                }

                currentEnemies.Add(a);
            }

            //Debug.Log(actualEnemy.Count + " " + actualEnemy[0].name);
            // link.container.currentAttack = true;

            // if (actualEnemy.Count < 1)
            // return;

            if (link.container.isActiveCursor)
            {
                TentaclesAttack(link.container.defaultAttackSpeed * 10, "0", currentEnemies);
            }
            else
            {
                //Debug.Log("ATTACK_DEAFAULT");
                DeafaultAttack(link.container.defaultAttackSpeed, "0", currentEnemies);
            }
        }

        public void TentaclesAttack(float timeSpeed, string attack, List<UnitHost> actualEnemy)
        {

            if (!link.container.isActiveCursor)
                return;

            if (link.container.controllManager.container.currentGrab)
                return;

            if (link.container.isAbsorptionMP)
                return;

            if (link.container.isAbsorptionHP)
                return;

            if (link.container.controllManager.container.currentSlideOnWall)
                return;

            if (link.container.controllManager.container.currentSlowDown)
                return;

            if (link.container.controllManager.container.currentDash)
                return;

            if (link.container.controllManager.container.currentRoll)
                return;

            if (link.container.controllManager.container.currentReactionToAttack)
                return;

            if (link.container.currentTentaclesAttack)
                return;

            if (!link.CallBack(BloodManager.EventType.TryAttack, new object[1] { actualEnemy }))
                return;

            link.CallBack(BloodManager.EventType.OnAttack, new object[1] { actualEnemy });
            link.weaponManager.current.Strike(attack, actualEnemy);
            link.container.unitHost.StartCoroutine(make_attackTentacles());
            // Debug.Log("tent");
        }

        public void DeafaultAttack(float timeInterval, string attack, List<UnitHost> actualEnemy)
        {

            if (link.container.isAttack)
            {
                link.container.isAttack = false;
            }   

            if (link.container.isActiveCursor)
                return;

            if (link.container.isAbsorptionMP)
                return;

            if (link.container.isAbsorptionHP)
                return;

            if (link.container.controllManager.container.currentGrab)
                return;

            if (link.container.controllManager.container.currentSlideOnWall)
                return;

            if (link.container.controllManager.container.currentSlowDown)
                return;

            if (link.container.controllManager.container.currentDash)
                return;

            if (link.container.controllManager.container.currentRoll)
                return;

            if (link.container.controllManager.container.currentReactionToAttack)
                return;

            if(link.container.currentDefaultAttack)
               return;

            //Debug.Log(typeAttack);

          // if (Make_attack_buffer == null && !link.container.isAttack)
             //   Make_attack_buffer = link.container.unitHost.StartCoroutine(make_attack_buffer());

            link.CallBack(BloodManager.EventType.OnAttackDefault, new object[2] {actualEnemy, typeAttack});
            //Debug.Log(link.container.isAttack);

            typeAttack = (typeAttack < 2) ? typeAttack + 1 : 0;
            //link.weaponManager.current.Strike(attack);
           // if (Make_attack_buffer == null)
                link.container.unitHost.StartCoroutine(make_attack(timeInterval));
        }
        IEnumerator make_attackTentacles()
        {
            float time = 0;

           link.container.currentTentaclesAttack = true;
            while (time <= 1)
            {
                if (!link.container.isActiveCursor)
                    break;

                time += 5 * Time.deltaTime;
                yield return null;
            }
           link.container.currentTentaclesAttack = false;
        }

        IEnumerator make_attack(float timeInterval)
        {
            float time = 0;

            link.container.currentDefaultAttack = true;
            while (time <= 1)
            {
                if (link.container.lkmPressed && time > 0.3f && !link.container.isAttack)
                    link.container.isAttack = true;

                if (link.container.controllManager.container.isOnGround)
                    link.container.controllManager.container.canMoveInGame = false;

               // link.CallBack(BloodManager.EventType.OnAttackDefault, new object[2] { actualEnemy, typeAttack });
                time += (1/timeInterval) * Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }

            link.container.currentDefaultAttack = false;

            if (link.container.isAttack)
            {
                //Debug.Log(00000);

                //link.container.currentDefaultAttack = false;
                
                DeafaultAttack(link.container.defaultAttackSpeed, "0", currentEnemies);
            }
 
           

            
            //Make_attack_buffer = null;
        }

    /*    private Coroutine Make_attack_buffer;

        IEnumerator make_attack_buffer()
        {
            float time = 0;

            link.container.currentAttackBuffer = true;
            while (time < 1f)
            {
                if (link.container.lkmPressed && time > 0.5f && !link.container.isAttack)
                    link.container.isAttack = true;
                Debug.Log(21323);
                time +=  4f*Time.deltaTime;
                yield return null;
            }

            if (link.container.isAttack)
            {
                Debug.Log(00000);
                DeafaultAttack(link.container.defaultAttackSpeed, "0", currentEnemies);
            }

            link.container.isAttack = false;
            link.container.currentAttackBuffer = false;
            Make_attack_buffer = null;
        }*/

    }
}

