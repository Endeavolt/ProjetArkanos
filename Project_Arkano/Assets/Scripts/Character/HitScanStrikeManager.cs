using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace General
{

    public class HitScanStrikeManager : MonoBehaviour
    {
        [Tooltip("Base time interaction")]
        [Header("Interactions Parameters")] public float interactionBaseTime = 0.25f;
        [Tooltip("Waiting Time after the ball is gone")] public float waitingTimeAfterInteraction = .2f;
        public Vector3[] playerPosition;
        public AnimationCurve powerSpeedBallCurve;
        public float maxSpeedForcePercent  =  .25f;
        [HideInInspector]
        public BallBehavior ball;
        [Tooltip("Distance minimum with walls for active the hit scan strike")] public float radiusOfStrike;
        public LayerMask layerMask;
        [Header("Debug Parameters")]
        public Vector3 center = new Vector3(1.05f, 1.28f, -2.23f);
        public bool debugActive = false;
        public Color[] positionColor = new Color[4];
        public float radiusPositionSphere = .3f;

        private List<Player.CharacterMouvement> m_playerScriptList = new List<Player.CharacterMouvement>();
        public List<Vector3> m_rawShootDirection = new List<Vector3>(4);
        private CameraShake m_cameraShake;
        private int m_currentPlayerInvolve;

        private float m_timerInteraction;
        private float m_timeInteraction;

        private bool m_bIsDuel = false;
        private bool m_isPhase3;




        public void Start()
        {
            m_cameraShake = Camera.main.GetComponent<CameraShake>();
        }

        public void AddPlayer(Vector3 direction, GameObject player)
        {
            if (m_isPhase3) return;
            if (m_currentPlayerInvolve == 0)
            {
                CheckBallPosIsAllow();
            }
            direction = direction.normalized;
            SetDirectionOrder(direction);
            player.transform.position = ball.transform.position  + playerPosition[m_currentPlayerInvolve];
            m_playerScriptList.Add(player.GetComponent<Player.CharacterMouvement>());
            m_currentPlayerInvolve++;
            m_timeInteraction = interactionBaseTime;
            m_timerInteraction = .0f;
            ball.isStop = true;
            if (m_currentPlayerInvolve >= 2) m_bIsDuel = true;
            if (m_currentPlayerInvolve == 1)
            {
                StartCoroutine(HitScanInteraction());
            }
        }
        private void SetDirectionOrder(Vector3 direction)
        {
            for (int i = 0; i < m_currentPlayerInvolve; i++)
            {
                if (direction.y > m_rawShootDirection[i].y)
                {
                    m_rawShootDirection.Insert(i, direction);
                    return;
                }
            }

            m_rawShootDirection.Insert(m_currentPlayerInvolve, direction);
        }
        private void ResetInteraction()
        {
            m_rawShootDirection.Clear();
            m_playerScriptList.Clear();
            m_currentPlayerInvolve = 0;
            m_timerInteraction = 0;
            m_timeInteraction = 0.0f;
            m_bIsDuel = false;
            m_isPhase3 = false;

        }
        private Vector3 FindShootDirection(float sign)
        {
            float baseAngle = sign * Vector3.Angle(Vector3.up, m_rawShootDirection[0]);
            float angleCount = 0.0f;
            for (int i = 1; i < m_currentPlayerInvolve; i++)
            {
                angleCount += sign * Vector3.Angle(m_rawShootDirection[0], m_rawShootDirection[i]);
            }

            angleCount /= m_currentPlayerInvolve;

            Vector3  dir = Quaternion.Euler(0.0f, 0.0f, baseAngle + angleCount) * Vector3.up;
            return dir.normalized;
        }
        IEnumerator HitScanInteraction()
        {

            while (m_timerInteraction < m_timeInteraction)
            {
                yield return Time.deltaTime;
                m_timerInteraction += Time.deltaTime;
            }

            m_isPhase3 = true;
            ShootBall();
            m_timerInteraction = .0f;


            while (m_timerInteraction < waitingTimeAfterInteraction)
            {
                yield return Time.deltaTime;
                m_timerInteraction += Time.deltaTime;
            }
            ReleasePlayer();
            ResetInteraction();
            yield return null;
        }


        private void CheckBallPosIsAllow()
        {
            ReplaceBall(Mathf.Sign(ball.m_direction.x) * ball.transform.right);
            ReplaceBall(-Mathf.Sign(ball.m_direction.x) * ball.transform.right);
            ReplaceBall(Mathf.Sign(ball.m_direction.y) * ball.transform.up);
            ReplaceBall(-Mathf.Sign(ball.m_direction.y) * ball.transform.up);
        }

        private void ReplaceBall(Vector3 direction)
        {
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ball.transform.position, direction, out hit, radiusOfStrike, layerMask))
            {
                ball.transform.position -= (ball.transform.position + direction * radiusOfStrike - hit.point);
            }
        }

        private void ShootBall()
        {
            Vector3 dir = FindShootDirection(1);
            int idBall = m_bIsDuel ? 5 : m_playerScriptList[0].GetComponent<Player.CharacterShoot>().GetPlayerID(); 
            ball.isStop = false;
            ball.Strike(dir, (PlayerID)idBall, 0f /*+ powerSpeedBallCurve.Evaluate(m_currentPlayerInvolve)* maxSpeedForcePercent*/);
            m_cameraShake.LaunchShakeEffect(interactionBaseTime, .5f);
        }
        private void ReleasePlayer()
        {

            for (int i = 0; i < m_currentPlayerInvolve; i++)
            {
                m_playerScriptList[i].FinishHitScan();
                if (m_bIsDuel) m_playerScriptList[i].SetExtraJump();
            }
        }

        #region  Debug Function
        public void OnDrawGizmos()
        {
            if (debugActive)
            {
                ShowPlayerPosition();
            }
        }
        private void ShowPlayerPosition()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(center, radiusPositionSphere);
            for (int i = 0; i < playerPosition.Length; i++)
            {

                    Gizmos.color = positionColor[i];
                    Gizmos.DrawSphere(center+ playerPosition[i], radiusPositionSphere);
            }
        }
        #endregion  

    }

}


