
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

        private CapsuleCollider m_capsuleCollider;
        private CharacterShoot m_characterShoot;
        private MeshFilter m_meshFilter;
        private BallBehavior m_ballBehavior;
        public float hitScanWaitTime = 0.3f;


        public void Start()
        {
            m_characterShoot = GetComponent<CharacterShoot>();
            m_capsuleCollider = GetComponent<CapsuleCollider>();
            m_meshFilter = GetComponent<MeshFilter>();
        }

        #region InputPlayer

        public void MouvementInput(InputAction.CallbackContext ctx)
        {
            if (ctx.performed) m_directionJump = ctx.ReadValue<Vector2>();
            if (ctx.canceled) m_directionJump = ctx.ReadValue<Vector2>();
            m_directionInput = m_directionJump;
        }

        public void JumpInput(InputAction.CallbackContext ctx)
        {
            if (ctx.started && !m_isJumping) Jump();
        }


        #endregion

        public void Update()
        {
            m_debugJumpDir = IsJumpDirectionValid();

            Debug.Log(IsJumpDirectionValid());
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
                Debug.Log(m_jumpDirection);
            }
            else
            {
                if (activeDebug) JumpDebug();
                m_isJumping = false;
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, 0, transform.rotation.eulerAngles.z);
                float angle = Vector3.SignedAngle(transform.up, hit.normal, Vector3.forward);
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z + angle);
                transform.position = transform.position + (hit.point - transform.position).normalized * ((hit.point - transform.position).magnitude - 1);
                Debug.Log(m_jumpDirection);
            }
        }

        private void Jump()
        {
            GlobalSoundManager.PlayOneShot(1, transform.position);

            m_jumpDirection = IsJumpDirectionValid();
            if (!ActiveAnalogigDirection)
            {
                m_jumpDirection = EightDirectionTransformation(m_jumpDirection);
            }
            m_jumpDirection = m_jumpDirection.normalized;
            float angle = Vector3.SignedAngle(m_jumpDirection.normalized, transform.up, -transform.forward);

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z + angle);
            SetupHitScanStrike();

        }

        private void SetupHitScanStrike()
        {
            RaycastHit hitInfo = new RaycastHit();
            if (IsHitScanStrike(ref hitInfo) && hitInfo.collider.tag =="Ball")
            {
                Vector3 dirRepostion = transform.position.x > hitInfo.transform.position.x ? -Vector3.right : Vector3.right;
                Vector3 playerPos = hitInfo.transform.position + dirRepostion * hitInfo.transform.localScale.x;
                transform.position = playerPos;
                m_ballBehavior = hitInfo.collider.GetComponent<BallBehavior>();
                m_ballBehavior.isStop = true;
                
                StartCoroutine(HitScanStrinking());
            }
            else
            {
                m_isJumping = true;
            }
        }

        private IEnumerator HitScanStrinking()
        {
            yield return new WaitForSeconds(hitScanWaitTime);
            m_ballBehavior.isStop = false;
            m_characterShoot.LaunchStrike();
            m_isJumping = true;
        }

        private bool IsHitScanStrike(ref RaycastHit hitInfo)
        {
            return Physics.CapsuleCast(transform.position + Vector3.up * -0.5f, transform.position + Vector3.up * 0.5f, m_capsuleCollider.radius, m_jumpDirection,out hitInfo, 30, hitScanLayerMask) ;
        }

        private Vector3 IsJumpDirectionValid()
        {
            return m_directionJump == Vector3.zero ? transform.up : m_directionJump;
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
            return Physics.Raycast(transform.position, direction, out hit, distance, layer);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Debug.DrawRay(transform.position, new Vector3(m_debugJumpDir.x, m_debugJumpDir.y, 0).normalized * 100);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position + Vector3.up * 0.5f, 0.1f);
            Gizmos.DrawSphere(transform.position + -Vector3.up * 0.5f, 0.1f);
            Gizmos.DrawMesh(m_meshFilter.mesh, transform.position + new Vector3(m_debugJumpDir.x, m_debugJumpDir.y, 0).normalized * 10.0f) ;

        }
    }
}