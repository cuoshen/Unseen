using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum Direction 
{
    LEFT_UP, RIGHT_UP, LEFT, RIGHT, LEFT_DOWN, RIHGT_DOWN, UP, DOWN 
}

public class move : MonoBehaviour
{
    private float movespeed = 2f;
    private float diveSpeed = .5f;


    CharacterController cc;
    private Vector3 m_camRot;
    private Transform m_camTransform;
    private Transform m_transform;
    
    public float m_rotateSpeed = 1;
    private bool dived = false;
    private characterController cc;

    // Start is called before the first frame update
    void Start()
    {
        
        m_camTransform = Camera.main.transform;
        m_transform = GetComponent<Transform>();
        
    }


    void Control()
    {
        if (Input.GetMouseButton(0))
        {
            
            float rh = Input.GetAxis("Mouse X");
            float rv = Input.GetAxis("Mouse Y");

            // rotate camera
            m_camRot.x -= rv * m_rotateSpeed;
            m_camRot.y += rh * m_rotateSpeed;

        }
        m_camTransform.eulerAngles = m_camRot;

        // change the player position angle so that it aligns with camera
        Vector3 camrot = m_camTransform.eulerAngles;
        camrot.x = 0; camrot.z = 0;
        m_transform.eulerAngles = camrot;



        float xm = 0, ym = 0, zm = 0;
        if (Input.GetKeyUp(KeyCode.Space))
        {
            dived = !dived;
        }

        if (dived)
        {
            ym -= diveSpeed * Time.deltaTime;
        }

        // test up
        if (!dived && Input.GetKey(KeyCode.F))
        {
            ym += movespeed * Time.deltaTime;
        }



        if (Input.GetKey(KeyCode.W))
        {
            zm += movespeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            zm -= movespeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.A))
        {
            xm -= movespeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            xm += movespeed * Time.deltaTime;
        }
        m_transform.Translate(new Vector3(xm, ym, zm), Space.Self);
    }
        // Update is called once per frame
    void Update()
    {

        Control();
    }
}
