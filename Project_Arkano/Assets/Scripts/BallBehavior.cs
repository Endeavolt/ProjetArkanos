using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Start is called before the first frame update
    void Start()
    {
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
        m_ballRenderer.material.color = gameManager.playerColor[id];
    }


}
