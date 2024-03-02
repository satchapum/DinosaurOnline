using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GroundMoveScript : NetworkBehaviour
{
    [SerializeField] GameObject ground_1;
    [SerializeField] GameObject ground_2;

    [SerializeField] LoginManagerScript loginManager;

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
        if (loginManager.isTwoPlayerSpawning.Value)
        {
            rbGround_1.MovePosition(rbGround_1.position + new Vector3(-1, 0, 0) * Time.deltaTime * speed);
            rbGround_2.MovePosition(rbGround_2.position + new Vector3(-1, 0, 0) * Time.deltaTime * speed);

            Vector3 positionToOut = new Vector3(-120, 0, 7.1f);
            Vector3 positionToReset_1 = new Vector3(ground_2.transform.position.x + 115f, 0, 7.1f);
            Vector3 positionToReset_2 = new Vector3(ground_1.transform.position.x + 115f, 0, 7.1f);

            if (ground_1.transform.position.x <= positionToOut.x)
            {
                ground_1.transform.position = positionToReset_1;
            }
            else if (ground_2.transform.position.x <= positionToOut.x)
            {
                ground_2.transform.position = positionToReset_2;
            }
        }
    }
}
