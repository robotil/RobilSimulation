using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using RosSharp.RosBridgeClient;

public class DataBillboard : MonoBehaviour
{
    public Text GPSCoordinates, XZpositions, XZDistance, Bearing;
    private NavSatFixPublisher nsf;
    private Double LatDegData, LonDegData, distData, brngData;
    Vector3 initPosData, tmpPosData;

    const double PI = 3.141592653589793238463;
    
    
    void Start()
    {
        nsf = GameObject.Find("RosConnector").GetComponent<NavSatFixPublisher>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        LatDegData = nsf.LatDeg;
        LonDegData = nsf.LonDeg;
        initPosData = nsf._init_pos;
        tmpPosData = nsf.tmpPos;
        distData = nsf.dist;
        brngData = nsf.brng;
        setGPStext();
    }

    private void setGPStext()
    {
        GPSCoordinates = GameObject.Find("GPS_coordinates_text").GetComponent<Text>(); 

        GPSCoordinates.text = "Latitude:  " + LatDegData + "    Longitude:   " + LonDegData;

        XZpositions = GameObject.Find("XZpositions").GetComponent<Text>();
        XZpositions.text = "init pos =  ( " + Math.Round(initPosData.x) + " , " + Math.Round(initPosData.z) + " )  ;  tmp pos =  ( " + tmpPosData.x + " , " + tmpPosData.z + " )";

        XZDistance = GameObject.Find("XZDistance").GetComponent<Text>();
        XZDistance.text = "Distance is:  " + distData;

        Bearing = GameObject.Find("Bearing").GetComponent<Text>();
        Bearing.text = "Bearing:  " + (brngData * (180 / PI));
    }
}
