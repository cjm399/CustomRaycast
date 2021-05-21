using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float ogCamSpeed = .1f;
    public float fastCamSpeed = 1f;

    private float currCamSpeed;
    private bool isFast = false;
    void Update()
    {
        Vector3 moveDir = Vector3.zero;
        if(Input.GetKey(KeyCode.W))
        {
            moveDir += transform.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveDir += -transform.right;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDir += -transform.forward;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDir += transform.right;
        }
        if (Input.GetKey(KeyCode.E))
        {
            moveDir += transform.up;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            moveDir += -transform.up;
        }
        isFast = Input.GetKey(KeyCode.LeftShift);
        currCamSpeed = (isFast ? fastCamSpeed : ogCamSpeed);
        moveDir = moveDir.normalized * currCamSpeed;
        transform.position += moveDir;
    }
}
