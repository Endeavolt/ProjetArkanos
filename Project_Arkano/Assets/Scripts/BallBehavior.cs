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
    private VisualEffect m_haloVfx;
    private VisualEffect m_haloVfx2;
    public float distance_Generatehalo;
    private Animator m_BallAnimator;
    private GameObject m_ballObject;
    // Start is called before the first frame update


    void Start()
    {
        InitVfx();
        m_ballRenderer = GetComponent<MeshRenderer>();
        transform.position = center;
        InitDirection();
        m_ballObject = GameObject.Find("BallObject").gameObject;
    }

    void InitVfx()
    {
        VisualEffect[] vfxTab = GetComponentsInChildren<VisualEffect>();
        m_trailVfx = vfxTab[0]; //Add ball trail change gradient
        //m_haloVfx = vfxTab[1];
        //m_haloVfx2 = vfxTab[2];
        //Debug.Log(m_trailVfx.name + " = trail. " + m_haloVfx.name + " = halo.");
        m_BallAnimator = gameObject.GetComponent<Animator>();
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
        Vector3 lastPosition = transform.position;
        transform.position = transform.position + m_direction.normalized * speed * Time.deltaTime;
        float velocity = Vector3.Distance(lastPosition, transform.position);
        if(velocity > distance_Generatehalo / 10)
        {
            LaunchHalo();
            Debug.Log("Haloed");
        }
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

    public void Strike(Vector3 direction, PlayerID strikerID, Vector3 trueDirection)
    {
        if(lastPlayerID != (int)strikerID)
        {
            lastPlayerID = (int)strikerID;
            consecustiveHit = 0;
        }
        Vector3 newRotation = trueDirection;
        m_ballObject.transform.rotation = Quaternion.Euler(newRotation);
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


    public IEnumerator StartSfx(float time)
    {
        yield return new WaitForSeconds(time);
        speed *= 1.1f;
        //m_haloVfx.SendEvent("Launch");
        //Debug.Log("Launch vfx Halo");
    }

    public IEnumerator DeformBall()
    {
        yield return new WaitForSeconds(0.5f);
        m_BallAnimator.SetBool("ActiveDeform", false);
    }
    public void LaunchHalo()
    {        
        //m_haloVfx2.SendEvent("Launch");
    }

    public void DeformEffect()
    {
        m_BallAnimator.SetBool("ActiveDeform", true);
        StartCoroutine(DeformBall());
    }

}
