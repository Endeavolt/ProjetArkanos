using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{

    public class CharacterJump : MonoBehaviour, CharacterControlsInterface
    {

        [Header("Jump Parameeters")]
        public float jumpSpeed = 25.0f;
        public bool analogicInputDirection;

        [Header("Jump Collision")]
        public LayerMask layers;
        public LayerMask hitScanLayerMask;

        [Header("Infos & Debug")]
        public bool activeDebug;
        public bool isJumping;

        private bool _isControlActive = false;

        private Vector3 _inputDirection;
        private Vector3 _jumpDirection;
        private bool _hasExtraJump;

        private float m_debugJumpTime;
        private float m_debugJumpDistance;
        private Vector3 m_debugJumpDir;

        [HideInInspector]
        public General.HitScanStrikeManager hitScanStrikeManager;
        private CharacterShoot m_characterShoot;
        private CapsuleCollider m_capsuleCollider;


        public void Start()
        {
            m_characterShoot = GetComponent<CharacterShoot>();
            m_capsuleCollider = GetComponent<CapsuleCollider>();
        }

        public void FixedUpdate()
        {
            UpdateJump();
        }

        #region Management Functions
        public void ChangeControl(bool state)
        {
            _isControlActive = state;
        }

        #endregion

        /// <summary>
        /// Function bind to the jump input
        /// </summary>
        /// <param name="ctx"></param>
        public void JumpInputAction(InputAction.CallbackContext ctx)
        {
            if (ctx.started && _isControlActive)
            {
                Jump();
            }
        }

        public void MouvementInput(InputAction.CallbackContext ctx)
        {
            if (ctx.performed && _isControlActive) _inputDirection = ctx.ReadValue<Vector2>();
            if (ctx.canceled && _isControlActive) _inputDirection = ctx.ReadValue<Vector2>();
        }

        public void SetExtraJump()
        {
            _hasExtraJump = true;
        }

        private void Jump()
        {
            _jumpDirection = GetJumpDirection();
            if (IsHitScanIsValid())
            {
                ActivateHitScanStrike();
                return;
            }
            isJumping = true;
            PlayJumpSound();
            OrientateCharacterInJumpDirection();
            _hasExtraJump = false;
        }

        private void UpdateJump()
        {
            if (!isJumping) return;

            RaycastHit hit = new RaycastHit();
            if (!IsGrounded(transform.up, ref hit, jumpSpeed * 2 * Time.fixedDeltaTime))
            {
                JumpMovement();
            }
            else
            {
                isJumping = false;
                JumpDebug();
                JumpLanding(hit);

            }
        }

        private void JumpLanding(RaycastHit hit)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, 0, transform.rotation.eulerAngles.z );
            float angle = Vector3.SignedAngle(transform.up, hit.normal, Vector3.forward);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, 0, transform.rotation.eulerAngles.z + angle);
            transform.position = transform.position + (hit.point - transform.position).normalized * ((hit.point - transform.position).magnitude - 1);
        }

        private void JumpMovement()
        {
            transform.position = transform.position + _jumpDirection * jumpSpeed * Time.fixedDeltaTime;
            m_debugJumpDistance += jumpSpeed * Time.fixedDeltaTime;
            m_debugJumpTime += Time.fixedDeltaTime;
        }

        private bool IsGrounded(Vector3 direction, ref RaycastHit hit, float distance)
        {
            return Physics.Raycast(transform.position, direction, out hit, distance, layers);
        }

        private void OrientateCharacterInJumpDirection()
        {
            float angle = Vector3.SignedAngle(_jumpDirection.normalized, transform.up, -transform.forward);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z + angle);
        }
        private void PlayJumpSound()
        {
            GlobalSoundManager.PlayOneShot(1, transform.position);
        }
        private void JumpDebug()
        {
            if (!activeDebug) return;
            Debug.Log("<b>Jump Time :</b> <color=cyan>" + m_debugJumpTime + "</color> <b>Jump Distance :</b><color=cyan>" + m_debugJumpDistance + "</color><b> Jump Direction :</b><color=cyan>" + m_debugJumpDir + "</color>");
            m_debugJumpDistance = 0.0f;
            m_debugJumpTime = 0.0f;
        }

        #region HitscanStrike
        private void ActivateHitScanStrike()
        {
                hitScanStrikeManager.AddPlayer(m_characterShoot.GetShootDirection(), this.gameObject);
        }
        private bool IsHitScanStrike(ref RaycastHit hitInfo)
        {
            return Physics.CapsuleCast(transform.position + Vector3.up * -0.5f, transform.position + Vector3.up * 0.5f, m_capsuleCollider.radius, _jumpDirection, out hitInfo, 30, hitScanLayerMask);
        }

        private bool IsHitScanIsValid()
        {
            RaycastHit hitInfo = new RaycastHit();
            return IsHitScanStrike(ref hitInfo) && hitInfo.collider.tag == "Ball" && m_characterShoot.IsShooting();
        }
        public void FinishHitScan()
        {
            isJumping = true;
        }
        #endregion

        #region Jump Direction

        private Vector3 GetJumpDirection()
        {
            Vector3 jumpDirection = new Vector3();

            if (HasNoDirectionInput()) { jumpDirection = transform.up; }
            else { jumpDirection = _inputDirection; }

            jumpDirection.z = 0.0f;

            if (analogicInputDirection)
            {
                jumpDirection = EightDirectionTransformation(jumpDirection);
            }

            return jumpDirection.normalized;

        }
        private bool HasNoDirectionInput() { return _inputDirection == Vector3.zero; }
        private Vector3 EightDirectionTransformation(Vector3 direction)
        {
            float xSign = Mathf.Sign(direction.x);
            float ySign = Mathf.Sign(direction.y);
            direction.x = Mathf.Abs(direction.x) > (Mathf.PI / 8.0f) ? xSign * 1 : 0;
            direction.y = Mathf.Abs(direction.y) > (Mathf.PI / 8.0f) ? ySign * 1 : 0;
            return direction.normalized;
        }

        #endregion;
    }

}