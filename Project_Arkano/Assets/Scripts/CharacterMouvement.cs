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


        private Vector3 m_directionInput;
        private Vector3 m_direction;

        private bool m_isJumping;
        private Vector3 m_jumpDirection;
        #region InputPlayer

        public void MouvementInput(InputAction.CallbackContext ctx)
        {
            if (ctx.performed) m_directionInput = ctx.ReadValue<Vector2>();
            if (ctx.canceled) m_directionInput = Vector2.zero;
        }

        public void JumpInput(InputAction.CallbackContext ctx)
        {
            if (ctx.started) Jump();
        }


        #endregion

        public void Update()
        {
            if (!m_isJumping) AvatarMouvement();
            
        }

        #region Deplacement

        private void AvatarMouvement()
        {
            OrientedMoveDirection();
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
            if (IsGrounded(-transform.up,ref hit, 1.1f))
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
            direction.x = Mathf.Round(direction.x);
            direction.y = Mathf.Round(direction.y);
            return direction;
        }

        private void AddInputToDirection()
        {
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
            if(m_isJumping)
            {
                AvatarJump();
            }
        }

        private void AvatarJump()
        {
            RaycastHit hit = new RaycastHit();
            if (!IsGrounded(transform.up,ref hit,jumpSpeed * 2*Time.deltaTime))
            {
                transform.position = transform.position + m_jumpDirection * jumpSpeed * Time.deltaTime;
            }
            else
            {
                m_isJumping = false;
                float angle = Vector3.SignedAngle(transform.up, hit.normal, Vector3.forward);
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z + angle);
                transform.position = transform.position  + (hit.point-transform.position).normalized * ((hit.point - transform.position).magnitude -1) ;
            }
        }

        private void Jump()
        {
            m_isJumping = true;
            m_jumpDirection = IsJumpDirectionValid();
            m_jumpDirection = EightDirectionTransformation(m_jumpDirection);
            float angle = Vector3.SignedAngle(m_jumpDirection.normalized,transform.up, -transform.forward);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z + angle);
        }

        private Vector3 IsJumpDirectionValid()
        {
            return m_directionInput == Vector3.zero ? transform.up : m_directionInput;
        }

        #endregion

        private bool IsGrounded(Vector3 direction, ref RaycastHit hit, float distance)
        {
            return Physics.Raycast(transform.position, direction, out hit, distance);
        }
    }
}