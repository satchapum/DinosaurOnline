using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundMoveScript : MonoBehaviour
{
    [SerializeField] GameObject ground_1;
    [SerializeField] GameObject ground_2;

    [SerializeField] float speed;

    private Rigidbody rbGround_1;
    private Rigidbody rbGround_2;

    private void Start()
    {
        rbGround_1 = ground_1.GetComponent<Rigidbody>();
        rbGround_2 = ground_2.GetComponent<Rigidbody>();
    }
    void FixedUpdate()
    {
        rbGround_1.MovePosition(rbGround_1.position + new Vector3(-1, 0, 0) * Time.deltaTime * speed);
        rbGround_2.MovePosition(rbGround_2.position + new Vector3(-1, 0, 0) * Time.deltaTime * speed);

        if (ground_1.transform.position == new Vector3(-70,0,0))
        {
            gameObject.transform.position = new Vector3(70, 0, 0);
        }
        else if(ground_2.transform.position == new Vector3(-70, 0, 0))
        {
            gameObject.transform.position = new Vector3(70, 0, 0);
        }
    }
}
