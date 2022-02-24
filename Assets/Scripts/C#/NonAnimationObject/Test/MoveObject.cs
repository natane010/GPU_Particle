using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    public bool isMove;
    public bool isRotate;

    Vector3 a;
    [SerializeField]float xSpeed;
    [SerializeField]float ySpeed;
    [SerializeField]float zSpeed;
    Quaternion b;
    [SerializeField] float qSpeed;

    [SerializeField] Rigidbody rb;

    private void Start()
    {
        a = Vector3.zero;
        b = Quaternion.identity;
    }
    // Update is called once per frame
    void Update()
    {
        var dir = Vector3.zero;
        a.y += Time.deltaTime * xSpeed;
        a.x += Time.deltaTime * ySpeed;
        a.z += Time.deltaTime * zSpeed;
        //gameObject.transform.eulerAngles = a;
        if(isRotate)
        b = Quaternion.Euler(a * qSpeed);

        this.gameObject.transform.rotation = b;

        if (Input.GetKey(KeyCode.RightArrow))
        {
            //this.transform.position += new Vector3(1, 0, 0) * Time.deltaTime;
            dir = new Vector3(1, 0, 0);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            //this.transform.position -= new Vector3(1, 0, 0) * Time.deltaTime;
            dir = new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            //this.transform.position += new Vector3(0, 0, 1) * Time.deltaTime;
            dir = new Vector3(0, 0, 1);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            //this.transform.position -= new Vector3(0, 0, 1) * Time.deltaTime;
            dir = new Vector3(0, 0, -1);
        }
        if (Input.GetKey(KeyCode.S))
        {
            rb.velocity = Vector3.zero;
        }
        rb.AddForce(dir * xSpeed);
    }
}
