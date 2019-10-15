using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleLogger : MonoBehaviour
{
    // Start is called before the first frame update

    private Quaternion vehicleTransfrom;
    private Quaternion ROSQuaternion;
    private Vector3 angles;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        vehicleTransfrom = transform.rotation;
        Debug.Log("Quaternion rotation is: X: " + vehicleTransfrom.x + " y: " + vehicleTransfrom.y + " z: " + vehicleTransfrom.z + " w: " + vehicleTransfrom.w);

        ROSQuaternion.x = -vehicleTransfrom.z;
        ROSQuaternion.y =  vehicleTransfrom.x;
        ROSQuaternion.z = -vehicleTransfrom.y;
        ROSQuaternion.w =  vehicleTransfrom.w;

        Debug.Log("ROS Quaternion rotation is: X: " + ROSQuaternion.x + " y: " + ROSQuaternion.y + " z: " + ROSQuaternion.z + " w: " + ROSQuaternion.w);

        angles = ROSQuaternion.eulerAngles;
        Debug.Log("Euler angles are Pitch (x): " + angles.y + " Yaw (y): " + (360-angles.z) + " Roll (z): " + (360-angles.x));
   
    }
}
