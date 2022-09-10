using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallBehavior : MonoBehaviour
{
    public Vector3 centre;
    public float speed = 10;
    public bool isDestroy = false;
    private Vector3 m_direction;

    // Start is called before the first frame update
    void Start()
    {
        transform.position = centre;
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
                }
            }


        }
    }

    private void DestroyBall()
    {
        if (isDestroy)
        {
            Destroy(gameObject);
        }
    }
    private void InitDirection()
    {
        int isRight = Random.Range(0, 2);
        float xDir = isRight == 0 ? -1.0f : 1.0f;
        m_direction = new Vector3(xDir, 0, 0);
    }

    public void Strike(Vector3 direction)
    {
        m_direction = direction.normalized;
    }


}
