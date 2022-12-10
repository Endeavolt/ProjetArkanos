
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class CharacterMouvement : MonoBehaviour
    {

        [Header("Control")]
        public bool ActiveAnalogigDirection;
        public float speed = 5;
        public float jumpSpeed = 25;
        public float shakeTime = .5f;
        [Header("Debug")]
        public bool activeDebug = false;

        public LayerMask layer;
        public LayerMask hitScanLayerMask;

        public Vector3 m_directionInput;
        public Vector3 m_directionJump;
        private Vector3 m_direction;

        public bool m_isJumping;
        private Vector3 m_jumpDirection;
        private bool m_rightSide = false;

        private float m_debugJumpTime;
        private float m_debugJumpDistance;
        private Vector3 m_debugJumpDir;
        
        private CameraShake m_cameraShake;
        private CapsuleCollider m_capsuleCollider;
        private CharacterShoot m_characterShoot;
        private CharacterJump m_characterJump;
        private BallBehavior m_ballBehavior;
        private bool m_HasExtraJump;
        [HideInInspector]
        public General.HitScanStrikeManager hitScanStrikeManager;
        public float hitScanWaitTime = 0.3f;
       

        public void Start()
        {
            m_characterShoot = GetComponent<CharacterShoot>();
            m_capsuleCollider = GetComponent<CapsuleCollider>();
            m_characterJump = GetComponent<CharacterJump>();
        }

        #region InputPlayer

        public void MouvementInput(InputAction.CallbackContext ctx)
        {
            if (ctx.performed) m_directionJump = ctx.ReadValue<Vector2>();
            if (ctx.canceled) m_directionJump = ctx.ReadValue<Vector2>();
            m_directionInput = m_directionJump;
        }


        #endregion

        public void Update()
        {
            if (!m_characterJump.isJumping)
            {
                AvatarMouvement();
                AvatarOrientation();

            }
        }

        #region Deplacement

        private void AvatarMouvement()
        {
            OrientedMoveDirection();
            m_directionInput = EightDirectionTransformation(m_directionInput);
            m_direction = EightDirectionTransformation(m_direction);
            AddInputToDirection();
            if (!IsObstacle())
            {
                Move();
            }
        }

        private void OrientedMoveDirection()
        {
            RaycastHit hit = new RaycastHit();
            if (IsGrounded(-transform.up, ref hit, 1.1f))
            {
                float angle = GetNormalAngle(hit.normal);
                m_direction = ChangeMovementDirection(angle);
                m_direction.Normalize();
            }

        }
        private Vector3 ChangeMovementDirection(float angle)
        {
            return new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle));
        }

        private bool IsObstacle()
        {
            Debug.DrawRay(transform.position, m_direction * 0.5f, Color.red);
            return Physics.Raycast(transform.position, m_direction.normalized, 0.5f);
        }

        private float GetNormalAngle(Vector3 normal)
        {
            float angle = Vector3.Angle(normal, Vector3.up);
            if (angle >= 180.0f) angle = 0.0f;
            return angle;
        }

        private Vector3 EightDirectionTransformation(Vector3 direction)
        {

            float xSign = Mathf.Sign(direction.x);
            float ySign = Mathf.Sign(direction.y);
            direction.x = Mathf.Abs(direction.x) > (Mathf.PI / 8.0f) ? xSign * 1 : 0;
            direction.y = Mathf.Abs(direction.y) > (Mathf.PI / 8.0f) ? ySign * 1 : 0;


            direction.Normalize();
            return direction;
        }

        private void AddInputToDirection()
        {
            m_directionInput.x = Mathf.Abs(m_directionInput.x) < 1.0f ? 0 : m_directionInput.x;
            m_directionInput.y = Mathf.Abs(m_directionInput.y) < 1.0f ? 0 : m_directionInput.y;

            m_direction.x *= m_directionInput.x;
            m_direction.y *= m_directionInput.y;
        }
        private void Move()
        {
            transform.position += m_direction.normalized * speed * Time.deltaTime;
        }
        #endregion

        #region Orientation

        private void AvatarOrientation()
        {
            if (m_rightSide != IsRightSide())
            {
                m_rightSide = IsRightSide();
                if (m_rightSide)
                {
                    TurnAvatar(180);

                }
                if (!m_rightSide)
                {
                    TurnAvatar(180);
                }
            }
        }

        public bool IsRightSide()
        {
            return (transform.position.x <= 2);
        }

        private void TurnAvatar(float angle)
        {
            transform.rotation = Quaternion.AngleAxis(angle, transform.up) * transform.rotation;
        }

        #endregion

        private bool IsGrounded(Vector3 direction, ref RaycastHit hit, float distance)
        {
            return Physics.Raycast(transform.position, direction, out hit, distance, layer);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Debug.DrawRay(transform.position, new Vector3(m_debugJumpDir.x, m_debugJumpDir.y, 0).normalized * 100);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position + Vector3.up * 0.5f, 0.1f);
            Gizmos.DrawSphere(transform.position + -Vector3.up * 0.5f, 0.1f);
            //Gizmos.DrawMesh(m_meshFilter.mesh, transform.position + new Vector3(m_debugJumpDir.x, m_debugJumpDir.y, 0).normalized * 10.0f) ;

        }
    }
}