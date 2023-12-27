using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSystem : MonoBehaviour
{

    [SerializeField] float m_boundarySensativity;
    [SerializeField] float m_camSpeed;


    private void Start()
    {
        Vector3 playerPos = PlayerMovement.Instance.transform.position;
        playerPos.z = -10.0f;
        transform.position = playerPos;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 MousePosFromCenter = new Vector2(Input.mousePosition.x - Screen.width * 0.5f, Input.mousePosition.y - Screen.height * 0.5f);

        if (MousePosFromCenter.x >= Screen.width * 0.5f - m_boundarySensativity)
            transform.position = new Vector3(transform.position.x + Time.deltaTime * m_camSpeed, transform.position.y, transform.position.z);
        
        else if(MousePosFromCenter.x < -Screen.width * 0.5f  + m_boundarySensativity)
            transform.position = new Vector3(transform.position.x - Time.deltaTime * m_camSpeed, transform.position.y, transform.position.z);
        
        if( MousePosFromCenter.y >= Screen.height * 0.5f - m_boundarySensativity)
            transform.position = new Vector3(transform.position.x, transform.position.y + Time.deltaTime * m_camSpeed, transform.position.z);
        
        else if(MousePosFromCenter.y < -Screen.height * 0.5f + m_boundarySensativity)
            transform.position = new Vector3(transform.position.x, transform.position.y - Time.deltaTime * m_camSpeed, transform.position.z);
         

        if (Input.GetKey(KeyCode.Space) && PlayerMovement.Instance != null)
        {
            Vector3 playerPos = PlayerMovement.Instance.transform.position;
            playerPos.z = -10.0f;
            transform.position = playerPos;
        }

    }
}
