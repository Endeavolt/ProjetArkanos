using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX; //Add ball trail change gradient
using FMOD.Studio;
using FMODUnity;
using UnityEngine.InputSystem;

public class BallBehavior : MonoBehaviour
{

    public GameManager gameManager;
    public PlayerID currentPlayerID;
    public Vector3 center;
    public float speed = 10;
    public bool isDestroy = false;
    public bool isStop = false;

    [SerializeField]
    private int m_score;
    public Vector3 m_direction; /* misc*/
    private MeshRenderer m_ballRenderer;
    public int lastPlayerID;
    public int consecustiveHit;

    private VisualEffect m_trailVfx;//Add ball trail change gradient
    public EventInstance instance_Bound;
    [EventRef]
    public string instance_Bound_Attribution;

    public PlayerInput playerInput;
    public LayerMask layerMask;
    private Vector3 normal;
    private Vector3 prevDir;
    private Vector3 lastPoint;
    // Start is called before the first frame update
    void Start()
    {
        playerInput = FindObjectOfType<PlayerInput>();
        //playerInput.actions["ResetBall"].performed += BallRepos;
        m_trailVfx = GetComponentInChildren<VisualEffect>(); //Add ball trail change gradient
        m_ballRenderer = GetComponent<MeshRenderer>();
        transform.position = center;
        InitDirection();
        ChangeBallColor(5);
    }

    // Update is called once per frame
    void Update()
    {
        DetectCollision();
        Move();
    }

    public void Move()
    {
        if (!isStop)
            transform.position = transform.position + m_direction.normalized * speed * Time.deltaTime;
    }

    public int GetBallScore() { return m_score; }

    private void DetectCollision()
    {
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(transform.position + m_direction.normalized * transform.localScale.x, m_direction.normalized, out hit, speed  * Time.deltaTime, layerMask))
        {
            DestroyBall();
            if (!isDestroy)
            {
                if (hit.collider.tag == "Wall")
                {
                    WallHitSound();
                    WallReflect(hit);
                    AddBallScore(1);
                    ChangeBallColor(5);
                }
            }


        }
        CheckOverlapWall(m_direction);
    }

    private void WallHitSound()
    {
        consecustiveHit += 1;
        if (!instance_Bound.isValid())
        {
            instance_Bound = RuntimeManager.CreateInstance(instance_Bound_Attribution);
        }
        if (consecustiveHit < 3)
        {
            instance_Bound.setParameterByName("ConsecutiveHit", consecustiveHit);
        }
        else
        {
            instance_Bound.setParameterByName("ConsecutiveHit", 3);
        }
        instance_Bound.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject.transform));
        instance_Bound.start();
        //instance_Bound.release();
    }

    private void WallReflect(RaycastHit hit)
    {
        normal = hit.normal;
        prevDir = m_direction;
        m_direction = Vector3.Reflect(m_direction.normalized, hit.normal);
        lastPoint = hit.point;
       // CheckOverlapWall(prevDir);
        DetectCollision();
        m_direction.Normalize();
    }
    private void CheckOverlapWall(Vector3 direction)
    {
      
        RaycastHit hit = new RaycastHit();
        RaycastHit hit2 = new RaycastHit();
        Vector3 dir = new Vector3(direction.normalized.x, 0, 0);
        Vector3 dir2 = new Vector3(0, direction.normalized.y, 0);
        if (Physics.Raycast(transform.position, dir.normalized, out hit, transform.localScale.x / 2, layerMask) && Physics.Raycast(transform.position, dir2.normalized, out hit2, transform.localScale.x / 2, layerMask))
        {
            float diff = transform.localScale.x - (hit.point.x - transform.position.x);
            float diff2 = transform.localScale.y - (hit2.point.y - transform.position.y);
            transform.position = new Vector3(transform.position.x + diff, transform.position.y + diff2, transform.position.z);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, m_direction.normalized * speed * Time.deltaTime);
    }


    private void AddBallScore(int score)
    {
        m_score += score;
        gameManager.ChangeBallScore(m_score);
    }
    private void DestroyBall()
    {
        if (isDestroy)
        {
            gameManager.gameScore.AddScore(1, (int)currentPlayerID);
            Destroy(gameObject);
        }
    }
    private void InitDirection()
    {
        int isRight = Random.Range(0, 2);
        float xDir = isRight == 0 ? -1.0f : 1.0f;
        m_direction = new Vector3(xDir, 0, 0);
    }

    public void Strike(Vector3 direction, PlayerID strikerID, float ratio)
    {
        if (lastPlayerID != (int)strikerID)
        {
            lastPlayerID = (int)strikerID;
            consecustiveHit = 0;
        }
        m_direction = direction.normalized;
        currentPlayerID = strikerID;
        speed *= Mathf.Lerp(1.0f, 1.2f, ratio);
        ChangeBallColor((int)strikerID);
    }

    private void ChangeBallColor(int id)
    {
        if (id == 5)
        {
            m_ballRenderer.material.color = Color.white;
            m_trailVfx.SetGradient("Balltrail_Gradient", gameManager.m_playerAsset.playerHitColorsGradient[4]);
            return;
        }
        //m_ballRenderer.material.color = gameManager.playerColor[id]; //Add ball trail change gradient
        m_trailVfx.SetGradient("Balltrail_Gradient", gameManager.m_playerAsset.playerHitColorsGradient[id]); //Add ball trail change gradient
    }

    private void BallRepos(InputAction.CallbackContext ctx)
    {
        transform.position = new Vector3(0, 0, -2.28f);
        speed = 10;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(lastPoint, normal * 5);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(lastPoint, -prevDir * 5);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(lastPoint, m_direction * 5);
    }
}
