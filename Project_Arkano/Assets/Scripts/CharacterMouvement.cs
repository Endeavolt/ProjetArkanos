using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class CharacterMouvement : MonoBehaviour
    {
        public float speed = 5;
        public float jumpSpeed = 25;
        [Header("Debug")]
        public bool activeDebug = false;

   

        private Vector3 m_directionInput;
        private Vector3 m_direction;

        public bool m_isJumping;
        private Vector3 m_jumpDirection;
        private bool m_rightSide = false;

        private float m_debugJumpTime;
        private float m_debugJumpDistance;
        private Vector3 m_debugJumpDir;
 

        #region InputPlayer

        public void MouvementInput(InputAction.CallbackContext ctx)
        {
            if (ctx.performed) m_directionInput = ctx.ReadValue<Vector2>();
            if (ctx.canceled) m_directionInput = Vector2.zero;
        }

        public void JumpInput(InputAction.CallbackContext ctx)
        {
            if (ctx.started && !m_isJumping) Jump();
        }


        #endregion

        public void Update()
        {
            if (!m_isJumping)
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
                m_isJumping = false;
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

        #region Jump

        private void FixedUpdate()
        {
            if (m_isJumping)
            {
                AvatarJump();
            }
        }

        private void AvatarJump()
        {
            RaycastHit hit = new RaycastHit();
            if (!IsGrounded(transform.up, ref hit, jumpSpeed * 2 * Time.deltaTime))
            {
                if (m_jumpDirection == Vector3.zero) m_isJumping = false;
                transform.position = transform.position + m_jumpDirection * jumpSpeed * Time.deltaTime;
                m_debugJumpDistance += jumpSpeed * Time.deltaTime;
                m_debugJumpTime += Time.deltaTime;
            }
            else
            {
                if (activeDebug) JumpDebug();
                m_isJumping = false;
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, 0, transform.rotation.eulerAngles.z);
                float angle = Vector3.SignedAngle(transform.up, hit.normal, Vector3.forward);
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z + angle);
                transform.position = transform.position + (hit.point - transform.position).normalized * ((hit.point - transform.position).magnitude - 1);
            }
        }

        private void Jump()
        {
            GlobalSoundManager.PlayOneShot(1, transform.position);
            m_isJumping = true;
            m_jumpDirection = IsJumpDirectionValid();
            m_jumpDirection = EightDirectionTransformation(m_jumpDirection);
            float angle = Vector3.SignedAngle(m_jumpDirection.normalized, transform.up, -transform.forward);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z + angle);
            m_debugJumpDir = m_jumpDirection;

        }

        private Vector3 IsJumpDirectionValid()
        {
            return m_directionInput == Vector3.zero ? transform.up : m_directionInput;
        }

        private void JumpDebug()
        {
            Debug.Log("<b>Jump Time :</b> <color=cyan>" + m_debugJumpTime + "</color> <b>Jump Distance :</b><color=cyan>" + m_debugJumpDistance + "</color><b> Jump Direction :</b><color=cyan>" + m_debugJumpDir + "</color>");
            m_debugJumpDistance = 0.0f;
            m_debugJumpTime = 0.0f;
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
            return Physics.Raycast(transform.position, direction, out hit, distance);
        }
    }
}