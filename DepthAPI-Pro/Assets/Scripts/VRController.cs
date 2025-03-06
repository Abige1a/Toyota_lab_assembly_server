using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

public class VRController : MonoBehaviour
{
    public Vector3 input;
    public bool allowRotation;
    public Vector3 rotationInput;

    public float moveRange = 1.0f;

    public int positionMode = 0;

    public Transform parent;

    public GameObject targetObject;
    public float targetMoveSpeed;
    public float targetRotateSpeed;

    [HideInInspector] public bool isSelected;
    private GameObject playerHand;

    public UnityRobotClient robotclient;




    // Start is called before the first frame update
    void Start()
    {
        isSelected = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isSelected)
        {
            if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
            {
                Vector3 movement = playerHand.transform.position - parent.position;
                if (movement.magnitude > moveRange)
                {
                    movement = movement.normalized * moveRange;
                }
                transform.position = parent.position + movement;
                if (allowRotation)
                {
                    transform.rotation = playerHand.transform.rotation;
                }
            }
            else
            {
                transform.position = parent.position;
                transform.rotation = parent.rotation;
            }
        }
        else
        {
            transform.position = parent.position;
            transform.rotation = parent.rotation;
        }
        input = (transform.position - parent.position) / moveRange;
        rotationInput = transform.rotation.eulerAngles;

        if(targetObject != null)
        {
            if (!allowRotation)
            {
                if(positionMode == 0){
                    targetObject.transform.position += input * targetMoveSpeed * Time.deltaTime;
                }
                else if(positionMode == 1){
                    targetObject.transform.position += new Vector3(input.x, 0.0f, 0.0f) * targetMoveSpeed * Time.deltaTime;
                }
                else if(positionMode == 2){
                    targetObject.transform.position += new Vector3(0.0f, input.y, 0.0f) * targetMoveSpeed * Time.deltaTime;
                }
                else if(positionMode == 3){
                    targetObject.transform.position += new Vector3(0.0f, 0.0f, input.z) * targetMoveSpeed * Time.deltaTime;
                }
            }
            else
            {
                float rotationAmount = targetRotateSpeed * Time.deltaTime;
                Quaternion rotationDelta = Quaternion.Euler(0, rotationInput.y * rotationAmount, 0);
                targetObject.transform.rotation *= rotationDelta;
            }
        }
        //double[] velocity = Rotation_transform(input, rotationInput);
    }

    private double[] Rotation_transform(Vector3 input, Vector3 rotationInput)
    {
        PoseMsg movement = new PoseMsg();
        movement.position =input.To<FLU>();

        if (rotationInput.x > 180f)
        {
            rotationInput.x -= 360f;
        }
        else if (rotationInput.x<=-180f)
        {
            rotationInput.x += 360f;
        }
        if (rotationInput.y > 180f)
        {
            rotationInput.y -= 360f;
        }
        else if (rotationInput.y <= -180f)
        {
            rotationInput.y += 360f;
        }
        if (rotationInput.z > 180f)
        {
            rotationInput.z -= 360f;
        }
        else if (rotationInput.z <= -180f)
        {
            rotationInput.z += 360f;
        }

        double[] velocity = new double[6];
        velocity[0] = movement.position.x;
        velocity[1] = movement.position.y;
        velocity[2] = movement.position.z;
        velocity[3] = rotationInput.x/90;
        velocity[4] = rotationInput.y/90;
        velocity[5] = rotationInput.z/90;


        return velocity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("PlayerHand"))
        {
            isSelected = true;
            playerHand = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("PlayerHand"))
        {
            isSelected = false;
            playerHand = null;
        }
    }
}
