using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX; //Add ball trail change gradient
using FMOD.Studio;
using FMODUnity;
public class BallBehavior : MonoBehaviour
{

    public GameManager gameManager;
    public PlayerID currentPlayerID;
    public Vector3 center;
    public float speed = 10;
    public bool isDestroy = false;

    [SerializeField]
    private int m_score;
    private Vector3 m_direction;
    private MeshRenderer m_ballRenderer;
    public int lastPlayerID;
    public int consecustiveHit;

    private VisualEffect m_trailVfx;//Add ball trail change gradient
    public EventInstance instance_Bound;
    [EventRef]
    public string instance_Bound_Attribution;
    // Start is called before the first frame update
    void Start()
    {
        m_trailVfx = GetComponentInChildren<VisualEffect>(); //Add ball trail change gradient
        m_ballRenderer = GetComponent<MeshRenderer>();
        transform.position = center;
        InitDirection();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    public void FixedUpdate()
    {
        DetectCollision();
    }

    public void Move()
    {
        transform.position = transform.position + m_direction.normalized * speed * Time.deltaTime;
    }

    public int GetBallScore() { return m_score; }

    private void DetectCollision()
    {
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(transform.position, m_direction.normalized, out hit, speed * 2 * Time.deltaTime))
        {
            DestroyBall();
            if (!isDestroy)
            {
                if (hit.collider.tag == "Wall")
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
                    m_direction = Vector3.Reflect(m_direction.normalized, hit.normal);
                    m_direction.Normalize();
                    AddBallScore(1);
                    ChangeBallColor(5);
                }
            }


        }
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

    public void Strike(Vector3 direction, PlayerID strikerID)
    {
        if(lastPlayerID != (int)strikerID)
        {
            lastPlayerID = (int)strikerID;
            consecustiveHit = 0;
        }
        m_direction = direction.normalized;
        currentPlayerID = strikerID;
        ChangeBallColor((int)strikerID);
    }

    private void ChangeBallColor(int id)
    {
        if (id == 5)
        {
            m_ballRenderer.material.color = Color.white;
            return;
        }
        //m_ballRenderer.material.color = gameManager.playerColor[id]; //Add ball trail change gradient
        m_trailVfx.SetGradient("Balltrail_Gradient", gameManager.m_playerAsset.playerHitColorsGradient[id]); //Add ball trail change gradient
    }


}
