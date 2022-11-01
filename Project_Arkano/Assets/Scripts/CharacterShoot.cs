using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FMOD.Studio;
using FMODUnity;

namespace Player
{

    public class CharacterShoot : MonoBehaviour
    {

        public Vector3 center;
        public float chargeTime = 1.5f;
        public float boxSize = 5.0f;
        public LayerMask layerMaskObjstacle;

        [Header("Debug")]
        public bool activeDebug = false;

        public PlayerUI playerUI { set; private get; }

        private bool m_isStrikeUp = false;
        private bool m_isCharging = false;

        private float m_charginTimer = 0.0f;

        private CharacterMouvement m_characterMouvement;
        private PlayerInput m_playerInput;

        public EventInstance instance_Charging;
        [EventRef]
        public string instance_Charging_Attribution;
        public EventInstance instance_Hit;
        [EventRef]
        public string instance_Hit_Attribution;
        public GameObject vfxAnimate;
        private Animator m_BallAnimator;
        private void Start()
        {
            m_playerInput = GetComponent<PlayerInput>();
            m_characterMouvement = GetComponent<CharacterMouvement>();
        }

        public void StrikeUpInput(InputAction.CallbackContext ctx)
        {
            if (ctx.started)
            {
                StartInstance_Charging();
            }
            if (ctx.performed) ActiveCharging(true);
            if (ctx.canceled)
            {
                LaunchStrike();
                StopInstance_Charging();
            }
        }
        public void StrikeDownInput(InputAction.CallbackContext ctx)
        {
            if (ctx.started)
            {
                StartInstance_Charging();
            }
            if (ctx.performed) ActiveCharging(false);
            if (ctx.canceled)
            {
                LaunchStrike();
                StopInstance_Charging();
            }
        }

        private void Update()
        {
            if (m_isCharging)
            {
                playerUI.FillStrikeImage(m_charginTimer / chargeTime);
                m_charginTimer += Time.deltaTime;
            }
        }


        private void OnDrawGizmos()
        {
            if (activeDebug)
            {
                if (m_isCharging)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(transform.position, new Vector3(boxSize, boxSize, boxSize));
                }
            }
        }


        public void ActiveCharging(bool isUp)
        {
            m_isStrikeUp = isUp;
            m_isCharging = true;
        }

        public void LaunchStrike()
        {
            float angle = Mathf.Lerp(0, 90.0f, (float)m_charginTimer / chargeTime);
            angle = m_isStrikeUp ? angle * -1 : angle;
            BallBehavior ballBehavior = null;
            if (CheckBallCollison(ref ballBehavior))
            {
                if (!m_characterMouvement.IsRightSide())
                {
                    angle *= -1.0f;
                    angle += 180;
                }
                if(!instance_Hit.isValid())
                {
                    instance_Hit = RuntimeManager.CreateInstance(instance_Hit_Attribution);
                }
                instance_Hit.setParameterByName("ChargeRate", m_charginTimer / chargeTime);
                instance_Hit.start();
                instance_Hit.release();
                //instance_Hit.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                //instance_Hit.release();
                Vector3 direction = Quaternion.AngleAxis(angle, -Vector3.forward) * Vector3.right;
                RaycastHit hit = new RaycastHit();
                Physics.Raycast(center, direction.normalized, out hit, 100.0f, layerMaskObjstacle);
                direction = hit.point - ballBehavior.transform.position;
                direction = new Vector3(direction.x, direction.y, 0);
                Vector3 trueDirection = direction;
                direction.Normalize();
                ballBehavior.Strike(direction, (PlayerID)m_playerInput.playerIndex, trueDirection);
                GameObject vfx_obj = Instantiate(vfxAnimate, new Vector3(ballBehavior.transform.position.x, ballBehavior.transform.position.y, -5) , ballBehavior.transform.rotation);
                StartCoroutine(ballBehavior.StartSfx(0.4f));
                ballBehavior.DeformEffect();
            }
            m_charginTimer = 0;
            m_isCharging = false;
            playerUI.FillStrikeImage(m_charginTimer / chargeTime);
        }

        public bool CheckBallCollison(ref BallBehavior ballBehavior)
        {
            Collider[] colliders = Physics.OverlapBox(transform.position, new Vector3(boxSize / 2, boxSize / 2, boxSize / 2));
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].tag == "Ball")
                {
                    ballBehavior = colliders[i].GetComponent<BallBehavior>();
                    return true;
                }
            }
            return false;
        }
        public void StartInstance_Charging()
        {
            if(!instance_Charging.isValid())
            {
                instance_Charging = RuntimeManager.CreateInstance(instance_Charging_Attribution);
                instance_Charging.start();
                instance_Charging.release();
            }

        }

        public void StopInstance_Charging()
        {

            instance_Charging.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            instance_Charging.release();
        }

    }
    

}