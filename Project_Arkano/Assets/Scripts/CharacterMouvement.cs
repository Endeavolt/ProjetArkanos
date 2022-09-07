using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class CharacterMouvement : MonoBehaviour
    {
        public float speed;

        private Vector3 m_directionInput;
        private Vector3 m_direction;

        public void MouvementInput(InputAction.CallbackContext ctx)
        {
            if (ctx.performed) m_directionInput = ctx.ReadValue<Vector2>();
            if (ctx.canceled) m_directionInput = Vector2.zero;
        }

        public void Update()

        {
            OrientedMoveDirection();
            EightDirectionTransformation();
            Move();
        }


        private void OrientedMoveDirection()
        {
            RaycastHit hit = new RaycastHit();
            if (IsGrounded(hit))
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

        private float GetNormalAngle(Vector3 normal)
        {
            float angle = Vector3.Angle(normal, Vector3.up);
            if (angle >= 180.0f) angle = 0.0f;
            return angle;
        }

        private bool IsGrounded(RaycastHit hit)
        {
            return Physics.Raycast(transform.position, -transform.up, out hit, 1.1f);
        }

        private void EightDirectionTransformation()
        {
            m_direction.x = Mathf.Round(m_direction.x);
            m_direction.y = Mathf.Round(m_direction.y);
         
        }
        private void Move()
        {
            m_direction.x *= m_directionInput.x;
            m_direction.y *= m_directionInput.y;
            transform.position += m_direction.normalized * speed * Time.deltaTime;
        }
    }

}