using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotDriver : MonoBehaviour
{
   public List<AxleInfo> axleInfos;
   public float maxMotorTorque;
   public float maxSteeringAngle;

    public void FixedUpdate()
    {
        float currentMotorTorque = maxMotorTorque * Input.GetAxis("Vertical");
        float currentSteeringAngle = maxSteeringAngle * Input.GetAxis("Horizontal");
        
        foreach (AxleInfo axle in axleInfos)
        {
            if (axle.attachedToMotor){
                axle.rightWheel.motorTorque = currentMotorTorque;
                axle.leftWheel.motorTorque = currentMotorTorque;
            }

            if (axle.attachToSteering)
            {
                axle.rightWheel.motorTorque = currentSteeringAngle;
                axle.leftWheel.motorTorque = currentSteeringAngle;
            }
        }
    }
}

[System.Serializable]

public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool attachedToMotor;
    public bool attachToSteering;
}
