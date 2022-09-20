using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

        private bool m_isStrikeUp = false;
        private bool m_isCharging = false;

        private float m_charginTimer = 0.0f;

        private CharacterMouvement m_characterMouvement;
        private PlayerInput m_playerInput;

        private void Start()
        {
            m_playerInput = GetComponent<PlayerInput>();
            m_characterMouvement = GetComponent<CharacterMouvement>();
        }

        public void StrikeUpInput(InputAction.CallbackContext ctx)
        {
            if (ctx.performed) ActiveCharging(true);
            if (ctx.canceled) LaunchStrike();
        }
        public void StrikeDownInput(InputAction.CallbackContext ctx)
        {
            if (ctx.performed) ActiveCharging(false);
            if (ctx.canceled) LaunchStrike();
        }

        private void Update()
        {
            if (m_isCharging)
            {
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
                Vector3 direction = Quaternion.AngleAxis(angle, -Vector3.forward) * Vector3.right;
                RaycastHit hit = new RaycastHit();
                Physics.Raycast(center, direction.normalized, out hit, 100.0f, layerMaskObjstacle);
                direction = hit.point - ballBehavior.transform.position;
                direction = new Vector3(direction.x, direction.y, 0);
                direction.Normalize();
                ballBehavior.Strike(direction, (PlayerID)m_playerInput.playerIndex);
            }
            m_charginTimer = 0;
            m_isCharging = false;
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

    }

}