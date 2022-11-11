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
        public enum StrikeType
        {
            Normal,
            HitScanStrike,
        }

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
        private CameraShake m_cameraShake;

        public EventInstance instance_Charging;
        [EventRef]
        public string instance_Charging_Attribution;
        public EventInstance instance_Hit;
        [EventRef]
        public string instance_Hit_Attribution;
        private void Start()
        {
            m_playerInput = GetComponent<PlayerInput>();
            m_characterMouvement = GetComponent<CharacterMouvement>();
            m_cameraShake = Camera.main.GetComponent<CameraShake>();
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

        public bool IsShooting() { return m_isCharging; }
        private void Update()
        {
            if (m_isCharging && !m_characterMouvement.m_isJumping)
            {
                playerUI.FillStrikeImage(m_charginTimer / chargeTime);
                m_charginTimer += Time.deltaTime;
                playerUI.aimShootFeedback.transform.position = GetShootUIPos(transform.position);
            }
            else
            {
                ResetStrike();
                playerUI.aimShootFeedback.transform.position = GetShootUIPos(transform.position);
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

        public void ResetStrike()
        {
            m_charginTimer = 0;
            playerUI.FillStrikeImage(m_charginTimer / chargeTime);
        }
        public void LaunchStrike(StrikeType isHitScanStrike = StrikeType.Normal)
        {
            BallBehavior ballBehavior = null;

            if (CheckBallCollison(ref ballBehavior))
            {
                if (isHitScanStrike == StrikeType.Normal && m_characterMouvement.m_isJumping) return;
                float angle = GetShootAngle();
                Vector3 direction = GetShootDirection(angle, ballBehavior.transform.position); ;
                ballBehavior.Strike(direction, (PlayerID)m_playerInput.playerIndex, m_charginTimer / chargeTime);
                ShootSound();
            }
            m_charginTimer = 0;
            m_isCharging = false;

        }

        public int GetPlayerID()
        {
            return m_playerInput.playerIndex;
        }

        #region Shoot function
        private float GetShootAngle()
        {
            float angle = Mathf.Lerp(0, 90.0f, (float)m_charginTimer / chargeTime);
            angle = m_isStrikeUp ? angle * -1 : angle;
            if (!m_characterMouvement.IsRightSide())
            {
                angle *= -1.0f;
                angle += 180;
            }
            return angle;
        }

        private Vector3 GetShootDestination(float angle)
        {
            Vector3 direction = Quaternion.AngleAxis(angle, -Vector3.forward) * Vector3.right;
            RaycastHit hit = new RaycastHit();
            Physics.Raycast(center, direction.normalized, out hit, 100.0f, layerMaskObjstacle);
            return hit.point;
        }
        private Vector3 GetShootDirection(float angle, Vector3 position)
        {
            Vector3 destination = GetShootDestination(angle);
            Vector3 direction = destination - position;
            direction = new Vector3(direction.x, direction.y, 0);
            return direction.normalized;

        }

        private Vector3 GetShootUIPos(Vector3 position)
        {
            float angle = GetShootAngle();
            Vector3 dest = GetShootDestination(angle);
            dest.z = 0;
            Vector3 direction = position - dest;
            Debug.DrawRay(transform.position, -direction);
            direction.z = 0;
            float angleDir = Vector3.SignedAngle(direction, Vector3.right, Vector3.right);
            Vector3 positionUI = dest + direction.normalized * (Mathf.Abs(center.x - dest.x) / Mathf.Abs(Mathf.Cos(angleDir * Mathf.Deg2Rad)));
            positionUI.z = -5.0f;
            return positionUI;

        }

        public Vector3 GetShootDirection()
        {
            float angle = GetShootAngle();
            Vector3 dest = GetShootDestination(angle);
            return dest;
        }
        #endregion

        #region Sound Functions
        private void ShootSound()
        {
            if (!instance_Hit.isValid())
            {
                instance_Hit = RuntimeManager.CreateInstance(instance_Hit_Attribution);
            }
            instance_Hit.setParameterByName("ChargeRate", m_charginTimer / chargeTime);
            instance_Hit.start();
            instance_Hit.release();
            //instance_Hit.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            //instance_Hit.release();
        }
        #endregion

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
            if (!instance_Charging.isValid())
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