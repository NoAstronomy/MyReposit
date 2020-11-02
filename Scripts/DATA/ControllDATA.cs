using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Experimental.XR;
using UnityEngine.Profiling;

namespace ControllDATA
{
    public partial class ControllManager
    {
        public container container;
        public raycastRealization raycastRealization;
        public movementRealization movementRealization;

        public ControllManager(GameObject me)
        {
            movementRealization = new movementRealization(this);
            container = new container(this, me);
            container.water = new WaterWorkManager(me);
            container.water.container.succ_radius = 4;
            raycastRealization = new raycastRealization(this);
        }
    }

    public partial class container
    {
        private ControllManager link;

        // public FluidAreaDATA.FluidAreaSimulation fluidAreaSimulation;

        public GeneratorGrabPoint generatorGrabPoint;

        public WaterWorkManager water;
        public LiquidSolver.VolumeClass currentVolume;
        public LiquidSolver.VolumeClass currentVolumeAlways;
        public LiquidSolver.VolumeClass[] currentVolumeByRadius;

        public float currentVolumeHeight;

        public UnitHost unitHost;
        public UnitStats stats;
        public Transform transform;
        public Rigidbody2D rigidbody;
        public BoxCollider2D bodyCollider;

        public Vector2 colliderStandSize;
        public Vector2 colliderStandOffset;

        public Vector2 leftStepPos;
        public Vector2 rightStepPos;
        public Vector2 posBeforeJump;

        public Vector2 posInTileMap;

        public bool isLeftStep;
        public bool isRightStep;
        public bool isOnGround;
        public bool isOnCeiling;
        public bool isLegsOnGround;
        public bool isJump;
        public bool moveUp;

        public float jumpTime = 0;

        public float groundDistance = 1f;
        public float horizontalAxes;
        public float horizontalOld;
        public float horizontalForAnimation;

        public float gravity = 1f;
        public float jumpCoef;
        public float rollCoef = 2f;
        public float rollDistanseCoef = 7f;
        public float dashSpeed = 12f;
        public float dashDistance = 6f;
        public float jumpDistance = 3.2f;
        public float grabScale = 0.5f;
        public float jumpOfWallXCoef = 2;

        public float rollSpeed = 6f;
        public float JumpSpeed = 6f;
        public float swimSpeed = 1f;
        public float movementSpeed;

        public float jumpOfWallInterval;
        public float grabInterval;
        public float movementInterval;
        public float jumpInterval;
        public float descentPlatformJumpInterval;
        public float fallenJumpInterval;
        public float afterRollInterval;
        public float afterGrabInterval;
        public float jumpDelayBeforeInterval;
        public float jumpDelayAfterInterval;
        public float slideInterval;

        #region KEY_PRESSED

        // Нажата ли клавиша

        public float horizontal;
        public bool jumpHeld;
        public bool isDash;
        public bool isRoll;
        public bool jumpPressed;
        public bool isGrab;
        public bool isSwim;
        public bool isBounceOfWall;
        public bool isSlideOnWall;
        public bool isJumpOfWall;
        public bool isDoubleJump;
        public bool isFallenJump;
        public bool isLanding;
        public bool isJumping;
        public bool isHorizontalMove;
        public bool isFallAfterJump;
        public bool unGrab;
        public bool breakDust;
        #endregion

        #region CAN_USE_IN_GAME

        public bool canGrabInGame;
        public bool canDashInGame;
        public bool canRollInGame;
        public bool canSlideInGame;
        public bool canAirDashInGame;
        public bool canDoubleJumpInGame;
        public bool canMoveInGame;
        public bool canJumpInGame;
        public bool canJumpOfWallInGame;
        public bool canFallenJumpInGame;

        #endregion

        #region CURRENT_ACTION

        public bool currentGrab;
        public bool currentJump;
        public bool currentDoubleJump;
        public bool currentFallenJump;
        public bool currentRoll;
        public bool currentDash;
        public bool currentAirDash;
        public bool currentMovement;
        public bool currentReactionToAttack;
        public bool currentJumpOfWall;
        public bool currentSlideOnWall;
        public bool currentSlowDown;
        public bool currentDelayAfterJump;
        public bool currentDelayBeforeJump;
        #endregion


        public GeneratorGrabPoint.GrabPoint actualGrabPoint;

        public container(ControllManager controllManager, GameObject me)
        {
            link = controllManager;
            unitHost = me.GetComponent<UnitHost>();

            generatorGrabPoint = GameObject.FindObjectOfType<GeneratorGrabPoint>();
            transform = me.transform;
            rigidbody = me.GetComponent<Rigidbody2D>();
            bodyCollider = me.GetComponent<BoxCollider2D>();

            colliderStandSize = bodyCollider.size;
            colliderStandOffset = bodyCollider.offset;

            canGrabInGame = true;
            canDashInGame = true;
            canAirDashInGame = false;
            canRollInGame = true;
            canSlideInGame = true;
            canDoubleJumpInGame = false;
            canMoveInGame = true;
            canJumpInGame = true;
            canJumpOfWallInGame = true;
            canFallenJumpInGame = true;

            unitHost.unit.OnHit += (args) =>
            {
                link.movementRealization.ReactionToAttack((UnitDATA.UnitHitInfo)args[0]);
            };

            OnEndDash += (args) =>
            {
                link.movementRealization.SlowDown();

            };
            unitHost.StartCoroutine(update());
        }

        #region DELEGATE

        public delegate void controll_delegate(object[] args);

        public event controll_delegate OnDodge;
        public event controll_delegate OnEndDodge;
        public event controll_delegate OnEndDash;
        public event controll_delegate OnGrab;
        public event controll_delegate OnUnGrab;
        public event controll_delegate OnAttack1;
        public event controll_delegate OnAttack2;
        public event controll_delegate OnJump;
        public event controll_delegate OnSlide;
        public event controll_delegate OnUnSlide;
        public event controll_delegate OnattakMeLow;
        public event controll_delegate OnattakMeHard;
        public event controll_delegate OnAttack;
        public event controll_delegate OnLand;

        public void CallBack(EventType callType, object[] args)
        {
            switch (callType)
            {
                case EventType.OnDodge:
                    OnDodge?.Invoke(args);
                    break;

                case EventType.OnEndDodge:
                    OnEndDodge?.Invoke(args);
                    break;

                case EventType.OnEndDash:
                    OnEndDash?.Invoke(args);
                    break;

                case EventType.OnAttack:
                    OnAttack?.Invoke(args);
                    break;

                case EventType.OnJump:
                    OnJump?.Invoke(args);
                    break;

                case EventType.OnGrab:
                    OnGrab?.Invoke(args);
                    break;

                case EventType.OnUnGrab:
                    OnUnGrab?.Invoke(args);
                    break;

                case EventType.OnSlide:
                    OnSlide?.Invoke(args);
                    break;

                case EventType.OnUnSlide:
                    OnUnSlide?.Invoke(args);
                    break;

                case EventType.OnattakMeLow:
                    OnattakMeLow?.Invoke(args);
                    break;

                case EventType.OnattakMeHard:
                    OnattakMeHard?.Invoke(args);
                    break;

                case EventType.OnLand:
                    OnLand?.Invoke(args);
                    break;
            }
        }

        public enum EventType
        {
            OnDodge,
            OnEndDodge,
            OnEndDash,
            OnAttack,
            OnJump,
            OnGrab,
            OnUnGrab,
            OnSlide,
            OnUnSlide,
            OnattakMeLow,
            OnattakMeHard,
            OnLand
        }

        public TypeGrab typeGrab;
        public enum TypeGrab
        {
            OneTile,
            AnyTIle
        }

        

        #endregion

        IEnumerator update()
        {
            yield return null;
            // fluidAreaSimulation = FluidAreaDATA.FluidArea_Composite.fluid_dictionary["Dust"];
            while (true)
            {
                isOnGround = false;

               /* if (!link.container.canMoveInGame)
                {
                    link.container.horizontal = 0;
                    link.container.movementSpeed = 0;
                    link.container.rigidbody.velocity = new Vector2(0, link.container.rigidbody.velocity.y);
                }*/

                link.raycastRealization.CastingAndChecking();

                isDash = false;
                isRoll = false;

                link.movementRealization.FlipScaleX();
                currentVolume = water.settings.GetCurrentWater;
                currentVolumeAlways = water.settings.GetCurrentVolume;
                currentVolumeByRadius = water.settings.GetCurrentWaterByRadius;

                if (currentVolume.GetGlobalHeight > 0)
                    currentVolumeHeight = currentVolume.GetGlobalHeight;
                else
                    currentVolumeHeight = Mathf.Lerp(
                        currentVolumeHeight,
                        currentVolume.GetGlobalHeight,
                        20f * Time.deltaTime
                        );

                horizontalOld = movementSpeed;

                link.container.movementSpeed = (movementInterval == 0)
                                               ? 0
                                               : link.container.movementSpeed;

                moveUp = (currentJump
                          || currentDoubleJump
                          || currentFallenJump
                          || currentJumpOfWall)
                         ? true
                         : false;

                if (moveUp)
                    isOnGround = false;

                if (isSwim)
                {
                    swimSpeed = 1;
                    water.settings.UpdateWaterUndeground(currentVolume);
                }
                else
                {
                    water.settings.DiscardWaterUndeground(currentVolume);

                    if (currentVolume.height > 0 &&
                        currentVolume.Water.position.y + currentVolume.Water.localScale.y - 0.25f >
                        transform.position.y)
                        swimSpeed = 0.5f;
                    else
                        swimSpeed = 1;
                }

                if (currentVolume.height != 0)
                    currentVolume.Pressure += horizontalOld * 0.15f;

                jumpOfWallInterval = (jumpOfWallInterval <= 0)
                                     ? 0
                                     : (jumpOfWallInterval - Time.deltaTime);

                movementInterval = (movementInterval <= 0)
                                   ? 0
                                   : (movementInterval - Time.deltaTime);

                afterRollInterval = (afterRollInterval <= 0)
                                   ? 0
                                   : (afterRollInterval - Time.deltaTime);

                grabInterval = (grabInterval <= 0)
                               ? 0
                               : (grabInterval - Time.deltaTime);

                afterGrabInterval = (afterGrabInterval <= 0)
                               ? 0
                               : (afterGrabInterval - Time.deltaTime);

                descentPlatformJumpInterval = (descentPlatformJumpInterval <= 0)
                               ? 0
                               : (descentPlatformJumpInterval - Time.deltaTime);

                jumpInterval = (jumpInterval <= 0)
                               ? 0
                               : jumpInterval - Time.deltaTime;

                slideInterval = (slideInterval <= 0)
                               ? 0
                               : slideInterval - Time.deltaTime;

                if (canGrabInGame)
                {
                    link.movementRealization.GrabPlatformCorner();
                    link.movementRealization.StayInGrabPoint();
                }

                if (canSlideInGame)
                {
                    link.movementRealization.SlidingOnWall();
                    link.movementRealization.UpdateSlideInterlval();
                }

                rigidbody.velocity = new Vector2(
                    rigidbody.velocity.x,
                    Mathf.Clamp(
                        rigidbody.velocity.y,
                        -14,
                        14)
                    );

                link.movementRealization.UpdateJumpInterval();

                currentMovement = (movementSpeed == 0) ? false : true;

                isJumping = (isFallAfterJump || isFallenJump) ? true : isJumping;

                isLanding = (isJumping && isOnGround) ? true : isLanding;

              //  link.movementRealization.Landing();
               // link.movementRealization.Grabbing();
               // link.movementRealization.Sliding();
                   

                yield return null;
            }
        }
    }

    public partial class raycastRealization
    {
        private ControllManager link;

        public raycastRealization(ControllManager controllManager)
        {
            link = controllManager;
        }

        #region RAYCAST

        private RaycastHit2D Raycast(Vector2 offset, Vector2 rayDirection, float length, LayerMask mask)
        {
            Vector2 pos = link.container.transform.position;
            RaycastHit2D hit = Physics2D.Raycast(pos + offset, rayDirection, length, mask);

            Color color = hit ? Color.red : Color.green;
            Debug.DrawRay(pos + offset, rayDirection * length, color);

            return hit;
        }

        private RaycastHit2D Raycast(Vector2 offset, Vector2 rayDirection, float length)
        {
            Vector2 pos = link.container.transform.position;
            RaycastHit2D hit = Physics2D.Raycast(pos + offset, rayDirection, length);
            Color color = hit ? Color.red : Color.green;
            Debug.DrawRay(pos + offset, rayDirection * length, color);

            return hit;
        }

        private RaycastHit2D Linecast(Vector3 start, Vector3 end, LayerMask mask)
        {
            RaycastHit2D hit = Physics2D.Linecast(start, end, mask);

            Color color = hit ? Color.red : Color.green;
            Debug.DrawLine(start, end, color);

            return hit;
        }

        #endregion

        #region ENVIROMENT_DETECTION

        public bool LineCastOnWall()
        {
            RaycastHit2D wallCheck = Linecast(
                link.container.transform.position + new Vector3(
                    (link.container.colliderStandSize.x / 2f +
                     link.container.colliderStandOffset.x) * link.container.transform.localScale.x,
                    link.container.colliderStandSize.y),
                link.container.transform.position + new Vector3(
                    (link.container.colliderStandSize.x / 2f +
                     link.container.colliderStandOffset.x) * link.container.transform.localScale.x,
                    link.container.colliderStandSize.y) +
                Vector3.right * link.container.transform.localScale.x * 0.1f,
                ~(LayerMask.GetMask("Player") + LayerMask.GetMask("Ignore Raycast") + LayerMask.GetMask("Sharf"))
            );

            RaycastHit2D wallCheckLow = Linecast(
               link.container.transform.position + new Vector3(
                   (link.container.colliderStandSize.x / 2f +
                    link.container.colliderStandOffset.x) * link.container.transform.localScale.x,
                   link.container.colliderStandSize.y / 2),
               link.container.transform.position + new Vector3(
                   (link.container.colliderStandSize.x / 2f +
                    link.container.colliderStandOffset.x) * link.container.transform.localScale.x,
                   link.container.colliderStandSize.y / 2) + Vector3.right * link.container.transform.localScale.x * 0.1f,
               ~(LayerMask.GetMask("Player") + LayerMask.GetMask("Ignore Raycast") + LayerMask.GetMask("Sharf"))
           );

            if (wallCheck.collider != null &&
                wallCheck.collider.tag == "Platform")
                link.container.canSlideInGame = true;
            else link.container.canSlideInGame = false;

            return (wallCheck.collider != null && wallCheckLow.collider != null);
        }

        public Vector2 GetNearWall()
        {
            RaycastHit2D wallCheckLow = Physics2D.Raycast(
                link.container.transform.position + new Vector3(
                    0,
                    link.container.colliderStandSize.y + 0.01f * link.container.jumpOfWallXCoef),
                new Vector2(link.container.transform.localScale.x, 0),
                1f,
                ~(LayerMask.GetMask("Player") + LayerMask.GetMask("Ignore Raycast") + LayerMask.GetMask("Sharf"))
            );
            //  if (wallCheckLow.collider != null)
            //  Debug.Log(wallCheckLow.collider.tag);

            if (wallCheckLow.collider != null)
                return wallCheckLow.point + new Vector2(0, 0.01f * link.container.jumpOfWallXCoef);

            return Vector2.zero;
        }

        public bool CastOnCeiling()
        {
            RaycastHit2D leftCheck = Raycast(
                new Vector2(
                    -link.container.colliderStandSize.x / 2f +
                    (link.container.colliderStandOffset.x) * link.container.transform.localScale.x,
                    link.container.colliderStandSize.y * 1.1f),
                Vector2.up,
                link.container.groundDistance * 0.1f,
                ~(LayerMask.GetMask("Player") + LayerMask.GetMask("Ignore Raycast") + LayerMask.GetMask("Sharf"))
            );

            RaycastHit2D rightCheck = Raycast(
                new Vector2(
                    link.container.colliderStandSize.x / 2f +
                    (link.container.colliderStandOffset.x) * link.container.transform.localScale.x,
                    link.container.colliderStandSize.y * 1.1f),
                Vector2.up,
                link.container.groundDistance * 0.1f,
                ~(LayerMask.GetMask("Player") + LayerMask.GetMask("Ignore Raycast") + LayerMask.GetMask("Sharf"))
            );

            return (leftCheck.collider != null || rightCheck.collider != null);
        }

        public bool CheckSlideOnGround()
        {
            RaycastHit2D leftCheck = Raycast(
                new Vector2(
                    -link.container.colliderStandSize.x / 2f + (link.container.colliderStandOffset.x) * link.container.transform.localScale.x,
                    -0.05f),
                Vector2.down * 2,
                link.container.groundDistance * 0.5f,
                ~(LayerMask.GetMask("Player") + LayerMask.GetMask("Ignore Raycast") + LayerMask.GetMask("Sharf"))
            );

            RaycastHit2D rightCheck = Raycast(
                new Vector2(
                    link.container.colliderStandSize.x / 2f + (link.container.colliderStandOffset.x) * link.container.transform.localScale.x,
                    -0.05f),
                Vector2.down * 2,
                link.container.groundDistance * 0.5f,
                ~(LayerMask.GetMask("Player") + LayerMask.GetMask("Ignore Raycast") + LayerMask.GetMask("Sharf"))
            );

            return (leftCheck.collider != null || rightCheck.collider != null);
        }

        public void CastingAndChecking()
        {
            RaycastHit2D leftCheck = Raycast(
                new Vector2(
                    -link.container.colliderStandSize.x / 2f + (link.container.colliderStandOffset.x) * link.container.transform.localScale.x,
                    0),
                Vector2.down,
                link.container.groundDistance * 0.1f + link.container.currentVolumeHeight * 1.3f,
                ~(LayerMask.GetMask("Player") + LayerMask.GetMask("Sharf") + LayerMask.GetMask("Ignore Raycast"))
            );

            RaycastHit2D rightCheck = Raycast(
                new Vector2(
                    link.container.colliderStandSize.x / 2f + (link.container.colliderStandOffset.x) * link.container.transform.localScale.x,
                    0),
                Vector2.down,
                link.container.groundDistance * 0.1f + link.container.currentVolumeHeight * 1.3f,
                ~(LayerMask.GetMask("Player") + LayerMask.GetMask("Sharf") + LayerMask.GetMask("Ignore Raycast"))
            );

            link.container.isLeftStep = (leftCheck.collider != null);
            link.container.isRightStep = (rightCheck.collider != null);


            link.container.isOnGround = (leftCheck.collider != null ||
                                         rightCheck.collider != null);

        }

        public bool CastRayOnFeet()
        {
            RaycastHit2D feetCheck = Raycast(
                new Vector2(
                    -link.container.colliderStandSize.x / 2f + (link.container.colliderStandOffset.x) * link.container.transform.localScale.x,
                    0),
                new Vector2(link.container.transform.localScale.x, 0),
                link.container.groundDistance * 1f,
                ~(LayerMask.GetMask("Player") + LayerMask.GetMask("Sharf") + LayerMask.GetMask("Ignore Raycast"))
            );

            // Debug.Log(feetCheck.collider != null);
            return feetCheck.collider != null;
        }
        #endregion

    }

    public partial class movementRealization
    {
        private ControllManager link;

        public movementRealization(ControllManager controllManager)
        {
            link = controllManager;

           // WorldSaveLogic.OnWorldLoad += on_world_load;
        }

        #region MOVEMENT

        public void Movement(float movementSpeed)
        {
          //  Debug.Log(link.container.horizontal);
          //  Debug.Log(link.container.rigidbody.velocity.x < 0);
            if (link.container.currentSlideOnWall)
                return;

            if (link.container.currentGrab)
                return;

            if (link.container.currentDash)
                return;

            if (link.container.currentRoll)
                return;

            if (link.container.currentReactionToAttack)
                return;

            if (link.container.currentSlowDown)
                return;

            if (link.container.currentDelayAfterJump)
                return;

            if (link.container.currentDelayBeforeJump)
                return;

           // if (!link.container.canMoveInGame)
             //   return;

            if (link.container.canMoveInGame)
            {
                if (Mathf.Abs(link.container.horizontal) > 0.1f)
                {
                    link.container.movementSpeed = movementSpeed;
                    link.container.movementInterval = 0.025f;
                }


                link.container.rigidbody.velocity = new Vector2(
                    link.container.horizontal * movementSpeed * link.container.swimSpeed,
                    link.container.rigidbody.velocity.y
                );
            }
            else
            {
                link.container.movementSpeed = 0;
                link.container.rigidbody.velocity = new Vector2(0f, link.container.rigidbody.velocity.y);
            }

        }

        public void Movement2(float movementSpeed, float horizontal)
        {
            if (!link.container.canMoveInGame)
                return;

            if (!link.container.currentSlowDown)
                return;

            if (link.container.currentSlideOnWall)
                return;

            if (link.container.currentRoll)
                return;

            if (link.container.currentReactionToAttack)
                return;

            if (link.container.currentDelayAfterJump)
                return;

            if (link.container.currentDelayBeforeJump)
                return;

            if (Mathf.Abs(horizontal) > 0.1f)
            {
                link.container.movementSpeed = movementSpeed;
                link.container.movementInterval = 0.025f;
            }

            link.container.rigidbody.velocity = new Vector2(
                horizontal * movementSpeed * link.container.swimSpeed,
                link.container.rigidbody.velocity.y
            );
        }

        public void ChangeHorizontal(float horizontal)
        {
            if (link.container.jumpOfWallInterval > 0.1f)
                link.container.horizontal = 0;
            else
            {
                if (link.container.currentSlideOnWall)
                    return;

                if (link.container.currentDelayAfterJump)
                    return;

                if (link.container.currentDelayBeforeJump)
                    return;

                

                link.container.horizontal = horizontal;
                
            }
        }

        public void FlipScaleX()
        {
            if (!link.container.canMoveInGame)
                return;

            if (link.container.currentSlideOnWall)
                return;

            if (link.container.currentGrab)
                return;

            if (link.container.currentDash)
                return;

            if (link.container.currentRoll)
                return;

            if (link.container.currentReactionToAttack)
                return;

            if (link.container.currentJumpOfWall)
                return;

            if (link.container.currentDelayAfterJump)
                return;

            if (link.container.currentDelayBeforeJump)
                return;


            if (link.container.horizontal < 0 && link.container.transform.localScale.x > 0)
            {
                link.container.transform.localScale = new Vector3(
                    -1f,
                    link.container.transform.localScale.y,
                    link.container.transform.localScale.z
                );
            }
            else if (link.container.horizontal > 0 && link.container.transform.localScale.x < 0)
            {
                link.container.transform.localScale = new Vector3(
                    1f,
                    link.container.transform.localScale.y,
                    link.container.transform.localScale.z
                );
            }
        }
        #endregion

        public void SlowDown()
        {
            link.container.unitHost.StartCoroutine(make_slowDowm());
        }

        IEnumerator make_slowDowm()
        {
            link.container.currentSlowDown = true;
            Vector2 direction = new Vector2(1, 0);
            Vector2 pos = link.container.transform.position;
            Vector2 endPos = pos + direction.normalized * link.container.transform.localScale.x;
            Vector2 slowDownDir = endPos - pos;
            float time = 0;
            while (time <= 1)
            {
                Movement2(link.container.stats.movementSpeed, link.container.transform.localScale.x * 1.2f);

                if (link.container.isOnGround)
                {
                    /* link.movementRealization.SpawnDustParticles(
                         new Vector3(0.3f * link.container.transform.localScale.x, -0.01f, 0),
                         Random.Range(0.3f, 0.4f),
                         1500
                     );

                     link.movementRealization.PushDustParticles(
                         new Vector3(0.3f * link.container.transform.localScale.x, 0f, 0), 2,
                         new Vector3(
                             0.0001f * link.container.transform.localScale.x,
                             Random.Range(0.00001f, 0.00003f),
                             0)
                     );*/
                }

                time += 5 * Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
            link.container.currentSlowDown = false;
        }

        #region DUST
        /* public void PushDustParticles(Vector3 offset, float radius, Vector3 direction)
         {
             FluidAreaDATA.FluidArea_Composite.fluid_dictionary["Dust"].PushParticle(
                 link.container.transform.position + offset,
                 radius,
                 direction
             );

         }

         public void SpawnDustParticles(Vector3 offset, float radius, float count)
         {
             FluidAreaDATA.FluidArea_Composite.fluid_dictionary["Dust"].SpawnParticle(
                 link.container.transform.position + offset,
                 radius,
                 count
             );
         }*/

        /*public IEnumerator make_dust(Vector3 offsetSpawn,
                                     float radiusSpawn,
                                     float countSpawn,
                                     Vector3 offsetPush,
                                     float radiusPush,
                                     Vector3 directionPush)
        {
            float time = 0;
            while (time < 1)
            {
                if (link.container.breakDust)
                    break;

                link.movementRealization.SpawnDustParticles(
                    offsetSpawn,
                    radiusSpawn,
                    countSpawn
                );

                link.movementRealization.PushDustParticles(
                    offsetPush,
                    radiusPush,
                    directionPush
                );

                time += 15 * Time.deltaTime;
                yield return null;
            }

            link.container.breakDust = false;
        }    */



        public void Landing()
        {
            if (!(link.container.isLanding && link.container.isJumping && link.container.isOnGround))
                return;

            if (link.container.currentGrab)
                return;

            if (link.container.currentVolumeHeight > 0.01f)
                return;

            link.container.isJumping = false;
            link.container.isLanding = false;
            link.container.CallBack(container.EventType.OnLand, new object[1]);
            // Debug.Log(123);
            link.container.unitHost.StartCoroutine(make_delayAfterJump());
            //Random.Range(0.1f, 0.2f) + Mathf.Abs(link.container.rigidbody.velocity.y) * 0.05f
            /* link.container.unitHost.StartCoroutine(
                 make_dustLanding()

             );   */
        }

        private IEnumerator make_delayAfterJump()
        {
            float time = 0;
            while (time < 1)
            {
                link.container.currentDelayAfterJump = true;
                link.container.horizontal = 0;
                link.container.rigidbody.velocity = new Vector2(0, 0);
                link.container.horizontal = 0;
                time += (1 / link.container.jumpDelayAfterInterval) * Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
            link.container.currentDelayAfterJump = false;
        }

        /* private IEnumerator make_dustLanding()
         {
             float time = 0;
            // float radius = Mathf.Abs(link.container.rigidbody.velocity.y) * 0.05f;
             //float dir = link.container.transform.localScale.x;
             while (time < 1)
             {

                 if (link.container.isRightStep && link.container.isLeftStep)
                 {
                     if (link.container.isOnGround)
                     {
                         link.movementRealization.SpawnDustParticles(
                             new Vector3(
                                 (link.container.colliderStandSize.x + link.container.colliderStandOffset.x) * 0.7f,
                                 0,
                                 0),
                             0.06f,
                             30000
                         );

                         link.movementRealization.SpawnDustParticles(
                             new Vector3(
                                 -(link.container.colliderStandSize.x + link.container.colliderStandOffset.x) * 0.7f,
                                 0,
                                 0),
                             0.06f,
                             30000
                         );

                         link.movementRealization.SpawnDustParticles(
                             new Vector3(
                                 0,
                                 0,
                                 0),
                             0.06f,
                             30000
                         );
                     }


                     link.movementRealization.PushDustParticles(
                         new Vector3(0.1f, -0.1f, 0),
                         0.5f,
                         new Vector3(
                             0.0004f * time,
                             -0.00001f,
                             0)
                     );




                     link.movementRealization.PushDustParticles(
                         new Vector3(-0.1f, -0.1f, 0),
                         0.5f,
                         new Vector3(
                             -0.0004f * time,
                             -0.00001f,
                             0)
                     );


                     link.movementRealization.PushDustParticles(
                         new Vector3(0.1f, -0.1f, 0),
                         0.5f,
                         new Vector3(
                             0,
                             -0.00001f,
                             0)
                     );
                 }
                 else
                 {

                     if (link.container.isRightStep)
                     {
                         link.movementRealization.SpawnDustParticles(
                             new Vector3(
                                 (link.container.colliderStandSize.x + link.container.colliderStandOffset.x) * 0.7f,
                                 0,
                                 0),
                             0.06f,
                             30000
                         );

                         link.movementRealization.PushDustParticles(
                             new Vector3(0.1f, -0.1f, 0),
                             0.5f,
                             new Vector3(
                                 0.0002f * time,
                                 -0.00001f,
                                 0)
                         );

                     }

                     if (link.container.isLeftStep)
                     {
                         link.movementRealization.SpawnDustParticles(
                             new Vector3(
                                 -(link.container.colliderStandSize.x + link.container.colliderStandOffset.x) *
                                 0.7f,
                                 0,
                                 0),
                             0.06f,
                             30000
                         );


                         link.movementRealization.PushDustParticles(
                             new Vector3(-0.1f, -0.1f, 0),
                             0.5f,
                             new Vector3(
                                 -0.0002f * time,
                                 -0.00001f,
                                 0)
                         );
                     }
                 }

                 time += 4 * Time.deltaTime;
                 yield return null;
             }

             link.container.breakDust = false;
         }*/

        // private Coroutine GrabDust;
        public void Grabbing()
        {
            if (!link.container.currentGrab)
            {
                //  GrabDust = null;
                return;
            }

            // if (GrabDust != null)
            //   return;

            /*   GrabDust = link.container.unitHost.StartCoroutine(
                   make_dust(
                       new Vector3(
                           0.3f * link.container.transform.localScale.x, 
                           link.container.colliderStandSize.y * 0.75f, 
                           0),
                       Random.Range(0.3f, 0.3f),
                       2000,
                       new Vector3(
                           0.3f * link.container.transform.localScale.x,
                           link.container.colliderStandSize.y * 0.75f,
                           0),
                       2,
                       new Vector3(
                           0,
                           Random.Range(-0.00001f, 0.00001f),
                           0)
                   )
               );*/
        }

        //private Coroutine SlideDust;
        public void Sliding()
        {
            if (!link.container.currentSlideOnWall)
            {
                return;
            }

            /* link.movementRealization.SpawnDustParticles(
                 new Vector3(0.3f * link.container.transform.localScale.x,
                             link.container.colliderStandSize.y * 0.75f,
                             0),
                 Random.Range(0.1f, 0.2f),
                 20000
             );

             link.movementRealization.PushDustParticles(
                 new Vector3(-link.container.transform.localScale.x, 0, 0),
                 10,
                 new Vector3(
                     -0.000003f * link.container.transform.localScale.x,
                     -0.000004f,
                     0)
             );  */
        }


        #endregion

        #region JUMPS

        // private float jumpDelay = 0;
        //private int jumpCount = 0;
        private bool currentJumpDelay;
        public void UpdateJumpInterval()
        {
            link.container.jumpInterval = (link.container.isOnGround && !link.container.currentJump && !currentJumpDelay)
                                          ? link.container.jumpInterval
                                          : 0.1f;

            link.container.jumpInterval = (link.container.currentJump) ? 0 : link.container.jumpInterval;

            link.container.isFallAfterJump = (link.container.isOnGround)
                              ? false
                              : link.container.isFallAfterJump;

            link.container.isFallenJump = (link.container.canFallenJumpInGame &&
                                           !link.container.isFallAfterJump
                                           && !link.container.isOnGround
                                           && !link.container.currentJump
                                           && !link.container.currentJumpOfWall
                                           && !link.container.currentSlideOnWall
                                           && !link.container.currentFallenJump
                                           && !link.container.isJumpOfWall)
                                          ? true
                                          : false;

            link.container.descentPlatformJumpInterval = (link.container.isOnGround
                                           && !link.container.currentJump)
                                          ? 0.1f
                                          : link.container.descentPlatformJumpInterval;

            link.container.descentPlatformJumpInterval = (link.container.currentJump)
                                          ? 0
                                          : link.container.descentPlatformJumpInterval;

            link.container.isJump = (link.container.canJumpInGame && link.container.descentPlatformJumpInterval > 0 && !currentJumpDelay)
                                    ? true
                                    : false;

            link.container.jumpPressed = (link.container.isJumpOfWall
                                          && !link.container.jumpHeld)
                                         ? true
                                         : link.container.jumpPressed;

            link.container.jumpPressed = (link.container.currentJumpOfWall
                                          || !link.container.currentSlideOnWall)
                                         ? false
                                         : link.container.jumpPressed;


        }

        public void Jumping()
        {

            if (link.container.currentRoll)
                return;

            if (link.container.currentReactionToAttack)
                return;

            if (!link.container.jumpHeld)
                return;

            if (link.raycastRealization.CastOnCeiling())
                return;

            if (link.container.currentDelayAfterJump)
                return;

            if (link.container.currentDelayBeforeJump)
                return;

            if (link.container.isJump && !link.container.currentJump && link.container.jumpInterval <= 0)
                link.container.unitHost.StartCoroutine(make_jump());
            else if (link.container.isJumpOfWall && !link.container.currentJumpOfWall && link.container.jumpPressed)
                link.container.unitHost.StartCoroutine(make_jumpOfWall());
            else if (link.container.isFallenJump && !link.container.currentFallenJump && !link.container.isJump)
                link.container.unitHost.StartCoroutine(make_fallenJump());
        }

        IEnumerator make_JumpDelayBefore()
        {
            float time = 0;
            while (time < 1 && !link.container.currentDelayAfterJump)
            {
                link.container.currentDelayBeforeJump = true;
                link.container.horizontal = 0;
                link.container.rigidbody.velocity = new Vector2(0, 0);
                time += (1 / link.container.jumpDelayBeforeInterval) * Time.deltaTime;
                Debug.Log(33211);
                yield return new WaitForFixedUpdate();
            }
            link.container.currentDelayBeforeJump = false;
        }

        IEnumerator make_jump()
        {
            float p = 0;
            while (p < 1 && !link.container.currentDelayAfterJump)
            {
                link.container.currentDelayBeforeJump = true;
                link.container.horizontal = 0;
                link.container.rigidbody.velocity = new Vector2(0, 0);
                p += (1 / link.container.jumpDelayBeforeInterval) * Time.deltaTime;
                //Debug.Log(33211);
                yield return new WaitForFixedUpdate();
            }

            link.container.currentDelayBeforeJump = false;
            //Debug.Log(2321);
            //link.container.unitHost.StartCoroutine(make_JumpDelayBefore());
            link.container.moveUp = true;
            float time = 0;
            link.container.isOnGround = false;
            float jumpCoef = 0;
            float jumpStartPos;
            float jumpStartEndPos;
            jumpStartPos = link.container.transform.position.y;
            jumpStartEndPos = jumpStartPos + link.container.jumpDistance;

            link.container.posBeforeJump = new Vector2(
                link.container.transform.position.x,
                link.container.transform.position.y
            );

            link.container.CallBack(container.EventType.OnJump, new object[] { (int)1 });

            link.container.currentJump = true;

            link.container.isSlideOnWall = true;

            link.container.rigidbody.gravityScale = 0;

            if (link.container.currentGrab)
                UnGrab();

            float timeMinCoef = 0.25f;
            timeMinCoef = (link.container.unGrab)
                          ? 0.5f
                          : 0.25f;

            while (time < 1f)
            {

                if (!(link.container.jumpHeld || (time < timeMinCoef)))
                    break;

                if (link.raycastRealization.CastOnCeiling())
                    break;

                link.container.isOnGround = false;
                link.container.rigidbody.velocity = new Vector2(
                    link.container.rigidbody.velocity.x,
                    0);

                link.container.rigidbody.MovePosition(
                    new Vector2(
                        link.container.rigidbody.position.x +
                        link.container.rigidbody.velocity.x * Time.deltaTime,
                        jumpStartPos * (1 - jumpCoef) + jumpStartEndPos * jumpCoef
                    )
                );

                jumpCoef = link.container.stats.jumpCurve.Evaluate(time);

                time += Time.deltaTime * 3f;
                yield return new WaitForFixedUpdate();
            }

            link.container.unGrab = (link.container.unGrab == true) ? false : link.container.unGrab;
            link.container.rigidbody.gravityScale = 1;
            link.container.currentJump = false;
            link.container.isFallAfterJump = true;
            link.container.isDoubleJump = true;
            link.container.moveUp = false;
        }

        /* IEnumerator make_Doublejump()
        {
            float time = 0f;
            float jumpCoef = 0f;

            float jumpStartPos;
            float jumpStartEndPos;

            link.container.CallBack(container.EventType.OnJump, new object[] { (int)2 });

            link.container.isDoubleJump = false;
            link.container.currentDoubleJump = true;

            jumpStartPos = link.container.transform.position.y;
            jumpStartEndPos = jumpStartPos + link.container.jumpDistance;

            link.container.rigidbody.gravityScale = 0;

            link.container.isSlideOnWall = true;

            while (time < 0.95f && link.container.jumpHeld)
            {
               if (link.raycastRealization.CastOnCeiling())
                   break;

                if (link.container.currentSlideOnWall)
                   break;

               // if (link.container.currentSlideOnWall)
                 //   break;

                link.container.isOnGround = false;
                link.container.rigidbody.velocity = new Vector2(
                    link.container.rigidbody.velocity.x,
                    0);
                link.container.rigidbody.MovePosition(
                    new Vector2(
                        link.container.rigidbody.position.x +
                        link.container.rigidbody.velocity.x * Time.fixedDeltaTime,
                        jumpStartPos * (1 - jumpCoef) + jumpStartEndPos * jumpCoef
                    )
                );

                jumpCoef = link.container.stats.doubleJumpCurve.Evaluate(time);

                time += Time.deltaTime * 2f;

                yield return new WaitForFixedUpdate();
            }

            link.container.rigidbody.gravityScale = 1;
            link.container.currentDoubleJump = false;

        }*/

        IEnumerator make_fallenJump()
        {
            float time = 0;
            float jumpCoef = 0;

            float jumpStartPos;
            float jumpStartEndPos;
            link.container.CallBack(container.EventType.OnJump, new object[] { (int)2 });
            link.container.isOnGround = false;
            link.container.currentFallenJump = true;
            link.container.isDoubleJump = false;

            jumpStartPos = link.container.transform.position.y;
            jumpStartEndPos = jumpStartPos + link.container.jumpDistance;
            link.container.rigidbody.gravityScale = 0;
            link.container.isSlideOnWall = true;

            while (time < 1f && (link.container.jumpHeld || time < 0.25f))
            {
                if (link.raycastRealization.CastOnCeiling())
                    break;

                if (link.container.currentSlideOnWall)
                    break;

                if (link.raycastRealization.LineCastOnWall())
                    break;

                link.container.rigidbody.velocity = new Vector2(
                    link.container.rigidbody.velocity.x,
                    0);

                link.container.isOnGround = false;

                link.container.rigidbody.MovePosition(
                    new Vector2(
                        link.container.rigidbody.position.x +
                        link.container.rigidbody.velocity.x * Time.deltaTime,
                        jumpStartPos * (1 - jumpCoef) + jumpStartEndPos * jumpCoef
                    )
                );

                jumpCoef = link.container.stats.jumpCurve.Evaluate(time);

                time += Time.deltaTime * 2.5f;

                yield return new WaitForFixedUpdate();
            }
            link.container.isFallAfterJump = true;
            link.container.rigidbody.gravityScale = 1;
            link.container.currentFallenJump = false;

        }


        IEnumerator make_jumpOfWall()
        {
            float time = 0;
            float jumpCoefSide = 0;
            float jumpCoefUp = 0;
            float jumpStartPos;
            float jumpStartEndPos;
            link.container.isOnGround = false;
            link.container.CallBack(container.EventType.OnJump, new object[] { (int)4 });

            link.container.currentJumpOfWall = true;
            link.container.isJumpOfWall = false;
            link.container.isDoubleJump = false;

            link.container.posBeforeJump = new Vector2(
                link.container.transform.position.x,
                link.container.transform.position.y
            );

            float mysacleX = link.container.transform.localScale.x;
            //Debug.Log("j of w");
            jumpStartPos = link.container.transform.position.y;
            jumpStartEndPos = jumpStartPos + link.container.jumpDistance * 0.6f;

            link.container.rigidbody.gravityScale = 0;

            UnSlide(-1);

            link.container.isSlideOnWall = true;

            while (time < 1f)
            {
                if (link.raycastRealization.CastOnCeiling())
                    break;

                if (link.raycastRealization.LineCastOnWall())
                    break;
                link.container.isOnGround = false;
                link.container.horizontal = -mysacleX;

                link.container.rigidbody.velocity = new Vector2(
                    link.container.rigidbody.velocity.x,
                    0
                );

                link.container.rigidbody.MovePosition(
                    new Vector2(
                        link.container.rigidbody.position.x + jumpCoefSide * -mysacleX * link.container.jumpOfWallXCoef,
                        jumpStartPos * (1 - jumpCoefUp) + jumpStartEndPos * jumpCoefUp
                    )
                );

                jumpCoefSide = link.container.stats.jumpOfWallCurveSide.Evaluate(time) * 0.05f;
                jumpCoefUp = link.container.stats.jumpOfWallCurveUp.Evaluate(time) * 1.5f;

                time += Time.fixedDeltaTime * 4f;
                yield return new WaitForFixedUpdate();
            }

            link.container.isFallAfterJump = true;
            link.container.currentJumpOfWall = false;
            link.container.rigidbody.gravityScale = 1;
        }

        #endregion

        #region DASHES

        public void Dash(Vector2 direction)
        {
            if (link.container.currentDash)
                return;

            if (!link.container.isOnGround)
                return;

            if (link.container.currentJump)
                return;

            if (link.container.currentFallenJump)
                return;

            if (link.container.currentRoll)
                return;

            if (link.container.currentReactionToAttack)
                return;

            if (!link.container.isDash)
                return;

            if (link.container.currentSlideOnWall)
                return;

            if (link.container.currentGrab)
                return;


            link.container.unitHost.StartCoroutine(make_dash(direction));
        }

        private Vector2 currentDashPoint;

        IEnumerator make_dash(Vector2 direction)
        {
            link.container.currentDash = true;
            link.container.CallBack(container.EventType.OnDodge, new object[] { (int)1 });
            Vector2 startPosition = link.container.transform.position;
            startPosition += direction * 0.2f;
            Vector2 dashPoint = startPosition + direction.normalized * link.container.dashDistance;
            float speed = link.container.dashSpeed;
            float time = 0f;
            int lastFrame = -1;

            /*link.container.unitHost.StartCoroutine(
                make_dust(
                    new Vector3(0.3f * link.container.transform.localScale.x, 0.01f, 0),
                    Random.Range(0.3f, 0.4f),
                    3000,
                    new Vector3(0, 0.01f, 0),
                    2,
                    new Vector3(
                        0,
                        Random.Range(-0.0001f, 0.0001f),
                        0)
                )
            );*/

            while (time <= 1f)
            {
                if (link.container.jumpHeld)
                    break;

                link.container.rigidbody.velocity = new Vector2(
                      link.container.rigidbody.velocity.x,
                      link.container.rigidbody.velocity.y
                  );

                link.container.rigidbody.MovePosition(
                    startPosition * (1f - time) +
                    dashPoint * time
                );

                if (Mathf.RoundToInt((5 * time)) != lastFrame)
                {
                    lastFrame = Mathf.RoundToInt((5 * time));
                    SpriteSequencer.CreateDashSequence(
                        link.container.rigidbody.position + new Vector2(0f, 0.5f),
                        lastFrame,
                        link.container.transform.localScale * 0.4f
                    );
                }

                time += speed * Time.deltaTime;

                yield return new WaitForFixedUpdate();
            }
            link.container.currentDash = false;
            link.container.CallBack(container.EventType.OnEndDodge, new object[] { (int)1 });
            if (link.container.isOnGround)
                link.container.CallBack(container.EventType.OnEndDash, new object[] { (int)1 });
            else
                link.container.CallBack(container.EventType.OnEndDash, new object[] { (int)2 });
        }

        public void AirDash(Vector2 direction)
        {

        }

        #endregion

        #region ROLL

        public void Roll(Vector2 direction, AnimationCurve rollSpeedCurve)
        {
            if (!link.container.isRoll)
                return;

            if (!link.container.isOnGround)
                return;

            if (link.container.currentRoll)
                return;

            if (link.container.currentDash)
                return;

            if (link.container.currentReactionToAttack)
                return;

            if (link.container.currentJump)
                return;

            link.container.currentRoll = true;
            link.container.unitHost.StartCoroutine(make_rolling(direction, rollSpeedCurve));
        }

        IEnumerator make_rolling(Vector2 direction, AnimationCurve rollSpeedCurve)
        {
            link.container.CallBack(container.EventType.OnDodge, new object[] { (int)2 });

            Vector2 startPoint = link.container.transform.position;
            Vector2 rollPoint = startPoint + direction.normalized * link.container.rollDistanseCoef;
            Vector2 rollDirection = rollPoint - startPoint;

            float time = 0;
            float rollCoef = 1;
            Vector3 curPos = Vector3.zero;
            while (time <= 1f)
            {

                rollCoef = rollSpeedCurve.Evaluate(time);
                link.container.rigidbody.velocity = new Vector2(
                    rollDirection.x * rollCoef,
                    link.container.rigidbody.velocity.y
                );

                curPos = link.container.transform.position;
                time += 2f * Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
            link.container.rigidbody.velocity = new Vector2(0, link.container.rigidbody.velocity.y);
            link.container.horizontal = 0;

            link.container.currentRoll = false;
            link.container.CallBack(container.EventType.OnEndDodge, new object[] { (int)2 });

        }

        #endregion

        #region SLIDING

        public void UpdateSlideInterlval()
        {
            link.container.isSlideOnWall = (link.container.slideInterval < 0.1f)
                                           ? true
                                           : link.container.isSlideOnWall;

            // Debug.Log(link.container.slideInterval);
        }

        public void SlidingOnWall()
        {
            if (!link.raycastRealization.LineCastOnWall())
                return;

            if (!link.container.canSlideInGame)
                return;

            if (!link.container.isSlideOnWall)
                return;

            if (link.container.currentJump)
                return;

            if (link.container.currentDoubleJump)
                return;

            if (link.container.currentFallenJump)
                return;

            if (link.container.jumpOfWallInterval > 0.1f)
                return;

            if (link.raycastRealization.CheckSlideOnGround())
                return;

            if (!link.raycastRealization.LineCastOnWall())
                return;

            if (!link.container.isSwim)
                return;

            if (link.container.currentReactionToAttack)
                return;

            if (link.container.currentRoll)
                return;

            if (link.container.currentDash)
                return;

            if (link.container.currentGrab)
                return;

            if (make_slideOnWall != null)
                link.container.unitHost.StopCoroutine(make_slideOnWall);

            make_slideOnWall = link.container.unitHost.StartCoroutine(make_SlideOnWall());

        }

        private Coroutine make_slideOnWall;

        IEnumerator make_SlideOnWall()
        {
            link.container.currentSlideOnWall = true;
            float time = 0;

            link.container.CallBack(container.EventType.OnSlide, new object[] { (int)1 });
            correct_slide_position();

            while (time < 1)
            {
                if (!link.container.canSlideInGame)
                    break;

                if (!link.raycastRealization.LineCastOnWall())
                    break;

                if (link.container.currentGrab)
                    break;

                if (link.raycastRealization.CheckSlideOnGround())
                    break;

                link.container.isJumpOfWall = (link.container.canJumpOfWallInGame)? true : false;
                link.container.isSlideOnWall = false;
                link.container.rigidbody.velocity = Vector2.zero;
                link.container.isOnGround = false;
                link.container.horizontal = 0;
                link.container.slideInterval = 0.5f;

                time += Time.deltaTime;
                yield return null;
            }

            link.container.isJumpOfWall = false;
            link.container.currentSlideOnWall = false;
            link.container.rigidbody.gravityScale = 1;
            //link.container.unitHost.StopCoroutine(make_slideOnWall);
        }

        void correct_slide_position()
        {
            Vector3 offset = link.raycastRealization.GetNearWall();

            link.container.transform.position = new Vector3(
                offset.x - link.container.transform.localScale.x
                * (link.container.colliderStandSize.x / 2f
                   + link.container.colliderStandOffset.x + 0.02f * link.container.jumpOfWallXCoef),
                link.container.transform.position.y
            );
        }

        public void UnSlide(float direction)
        {
            link.container.jumpOfWallInterval = 0.3f;

            if (!link.container.currentSlideOnWall)
                return;

            if (make_slideOnWall != null)
                link.container.unitHost.StopCoroutine(make_slideOnWall);

            link.container.transform.localScale = new Vector3(direction * link.container.transform.localScale.x, 1, 1);
            link.container.CallBack(container.EventType.OnUnSlide, new object[1]);
            link.container.isOnGround = false;
            link.container.moveUp = true;
            link.container.currentSlideOnWall = false;
        }

        #endregion

        #region GRAB

        private Vector2 grabPos;
        public void GrabPlatformCorner()
        {
            if (link.container.isOnGround)
                return;

            if (link.container.isOnGround && link.container.grabInterval < 0.2f)
                link.container.grabInterval = 0.2f;

            if (link.container.currentGrab && !link.container.isOnGround)
            {
                link.container.isOnGround = true;
                return;
            }

            if (link.container.isOnGround
                && link.container.currentJump)
                return;

            if (link.container.grabInterval > 0)
                return;

            if (link.container.currentJump)
                return;

            if (link.container.currentDoubleJump)
                return;

            if (link.container.currentJumpOfWall)
                return;

            if (link.container.currentSlideOnWall)
                return;

            if (link.container.currentFallenJump)
                return;

            if (!link.container.isSwim)
                return;

            if (link.container.currentGrab)
                return;

            if (link.container.currentReactionToAttack)
                return;

            if (link.container.currentRoll)
                return;

            if (link.container.currentDash)
                return;

            var grablist = link.container.generatorGrabPoint.getPointByX(
                link.container.generatorGrabPoint.GetX(link.container.transform.position.x),
                link.container.transform.localScale.x > 0);

            Vector3 offset = new Vector2(
                (link.container.colliderStandSize.x / 2 + link.container.colliderStandOffset.x) * link.container.transform.localScale.x,
                link.container.colliderStandSize.y
            );

            Vector2 myPos = link.container.transform.position + offset;

            link.container.actualGrabPoint = null;
            foreach (var a in grablist)
            {
                Vector2 dist =
                    new Vector2(
                        a.position.x,
                        a.position.y) +
                    new Vector2(
                        link.container.grabScale * -link.container.transform.localScale.x,
                        link.container.grabScale
                    );

                if (Vector2.Distance(dist, myPos) <= link.container.grabScale + 0.01f)
                {
                    link.container.actualGrabPoint = a;
                    link.container.currentGrab = true;
                }
            }
        }



        public void StayInGrabPoint()
        {
            if (link.container.actualGrabPoint == null)
            {
                link.container.rigidbody.gravityScale = 1;
                return;
            }

            Vector3 offset = new Vector2(
                (link.container.colliderStandSize.x + link.container.grabScale) * -link.container.transform.localScale.x,
                -link.container.colliderStandSize.y + link.container.grabScale
            );

            grabPos = link.container.actualGrabPoint.position + offset;
            if (link.raycastRealization.CastRayOnFeet())
            {
                link.container.typeGrab = container.TypeGrab.AnyTIle;
            }
            else
            {
                link.container.typeGrab = container.TypeGrab.OneTile;
            }
            // Debug.Log("type_grab" + link.container.typeGrab);

            if (link.container.transform.position.y < grabPos.y - 0.15f)
            {
                UnGrab();
                return;
            }

            link.container.rigidbody.gravityScale = 0;

            if (link.container.actualGrabPoint.isDynamic)
            {
                link.container.transform.SetParent(link.container.actualGrabPoint.transform);
                link.container.transform.position = link.container.actualGrabPoint.transform.position + new Vector3(
                                                        -link.container.grabScale * link.container.transform.localScale.x -
                                                        link.container.transform.localScale.x / 3,
                                                        -link.container.colliderStandSize.y + link.container.grabScale
                                                    );
                link.container.rigidbody.velocity = new Vector2(0, 0);
            }
            else
            {
                if (GrabToCorner == null)
                {
                    link.container.rigidbody.velocity = new Vector2(0, 0);
                    GrabToCorner = link.container.unitHost.StartCoroutine(make_grabToCorner());

                }
            }
        }

       /* void on_world_load(object[] args)
        {
            if (GrabToCorner != null)
                link.container.unitHost.StopCoroutine(GrabToCorner);
            GrabToCorner = null;
        }*/

        private Coroutine GrabToCorner;
        IEnumerator make_grabToCorner()
        {
            Vector3 grabPosV3 = new Vector3(
                grabPos.x,
                grabPos.y,
                link.container.transform.position.z
                );
            float time = 0;
            Vector3 startPos = link.container.transform.position;
            while (time <= 1)
            {
                // link.container.rigidbody.velocity = new Vector2(link.container.rigidbody.velocity.x, 0);
                link.container.rigidbody.gravityScale = 0;
                link.container.rigidbody.MovePosition(
                    new Vector3(
                        grabPosV3.x,
                        startPos.y * (1 - time) + grabPosV3.y * time,
                        link.container.transform.position.z
                    )
                );

                time += 10 * Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }

            link.container.transform.position = new Vector3(
                link.container.transform.position.x, grabPosV3.y,
                link.container.transform.position.z
                );
            link.container.CallBack(container.EventType.OnGrab, new object[1] { (int)1 });
        }

        public void UnGrab()
        {
            if (!link.container.currentGrab)
                return;

            if (link.container.actualGrabPoint.isDynamic)
                link.container.transform.SetParent(null);

            link.container.unGrab = true;
            link.container.horizontal = 0;
            link.container.currentGrab = false;
            link.container.CallBack(container.EventType.OnUnGrab, new object[] { (int)1 });
            link.container.actualGrabPoint = null;
            link.container.afterGrabInterval = 0.3f;
            GrabToCorner = null;
        }

        #endregion

        #region REACTION_TO_ATTACK

        public void ReactionToAttack(UnitDATA.UnitHitInfo unitHitInfo)
        {
            link.container.unitHost.StartCoroutine(make_reactionToAttack(unitHitInfo));
        }

        IEnumerator make_reactionToAttack(UnitDATA.UnitHitInfo unitHitInfo)
        {
            link.container.CallBack(container.EventType.OnattakMeLow, new object[] { (int)1 });
            link.container.currentReactionToAttack = true;
            link.container.rigidbody.gravityScale = 0;
            Vector2 startPos = link.container.transform.position;
            Vector2 endPos = startPos + new Vector2(
                                 link.container.transform.localScale.x * (unitHitInfo.isFromBack ? 1 : -1),
                                 0.3f) * (int)(unitHitInfo.hitType + 1);

            float time = 0;
            while (time < 1)
            {
                link.container.rigidbody.MovePosition(startPos * (1 - time) + endPos * time);
                time += 5 * Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
            link.container.rigidbody.gravityScale = 1;
            link.container.currentReactionToAttack = false;
        }

        #endregion

        public void GoToBeforeJumpPoint()
        {
            link.container.transform.position = link.container.posBeforeJump;
        }

    }

}
