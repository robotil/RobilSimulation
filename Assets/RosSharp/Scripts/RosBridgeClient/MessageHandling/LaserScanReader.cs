
﻿/*
© Siemens AG, 2018-2019
Author: Berkay Alp Cakal (berkay_alp.cakal.ct@siemens.com)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
<http://www.apache.org/licenses/LICENSE-2.0>.
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using System.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine.UI;

namespace RosSharp.RosBridgeClient
{
   
    
    public class LaserScanReader : MonoBehaviour {
        
        //rp lidar node in the simulation containes a single measurment of distance and intensity. In order to work with the real sensor we will need to change the struct to match the real message from the rplidar.
        //angle and distance of 32 measurments
        public class rplidar__data{
            //distance im m
            public float[] dist = new float[400];
            //float quality = new float[32];
            //angle in degrees
            public float[] angle = new float[400];
        }

        public LaserScanPublisher LaserScanPublisher;
        public Transform  SensorRotator;
        //angle resolution set by the user
        public float angle_res;
        // start angle of the scan set by the user[deg]
        public float MinAngle;
        //end angle of the scan set by the user[deg]
        public float MaxAngle;
        
        private Messages.Sensor.LaserScan message;
        //When this array is filled with the distances data of the scans, we send the data message with the publisher
        private rplidar__data lidar_data;
        private int measurmensts_counter;
        //RP Lidar A2 time for a single measurment
        private float measurment_time = 0.00025f;
        //calculated according to the angle resolution set by the user
        private float rotationFrequency;
        //calculate the number of laser samples that should be taken in fixedupdate time according to the measurment time
        private int SamplesPerPhysStep;
        private float horCurrentAngle;
        private NativeArray<RaycastHit> results;
        private NativeArray<RaycastCommand> commands; 
        private float[] azimuth;
        //private LaserScanVisualizer[] laserScanVisualizers;

        

        //**check this***/
        //time between measurements [seconds] - if your scanner is moving, this will be used in interpolating position of 3d points
        private float time_increment;
        //*******check this and also missing in our code msg********/
        //time between scans [seconds]
        private float scan_time;
        //# intensity data [device-specific units].  If your device does not provide intensities, please leave the array empty
        private float[] intensities;

        public void Start()
        {
            lidar_data = new rplidar__data();
            message = new Messages.Sensor.LaserScan();
            message.ranges = new float[lidar_data.dist.Length];
            //we don't get intensities but need to send an empty array
            intensities = new float[lidar_data.dist.Length];
            MinAngle = MinAngle < 0 ? 360 + MinAngle : MinAngle;
            MaxAngle = MaxAngle < 0 ? 360 + MaxAngle : MaxAngle;
            rotationFrequency = 1f / ((MaxAngle - MinAngle) / angle_res * measurment_time);
            SamplesPerPhysStep = Mathf.RoundToInt(Time.fixedDeltaTime / measurment_time); 
            //results = new NativeArray<RaycastHit>(SamplesPerPhysStep, Allocator.TempJob);
            //commands = new NativeArray<RaycastCommand>(SamplesPerPhysStep, Allocator.TempJob);
            horCurrentAngle = MinAngle;
            SensorRotator.localEulerAngles = new Vector3(0, horCurrentAngle, 0);
            azimuth = new float[SamplesPerPhysStep];

            //*****check this - i think it's the total time for tis scan. maybe need to measure the time since start filling the array until finish */
            scan_time = lidar_data.dist.Length * measurment_time;
            //*****check this - i think it's the time for each laser scan */
            time_increment = measurment_time;
            //Debug.Log("Min Angle " + MinAngle + " MaxAgle " + MaxAngle + " rotationFreq " + rotationFrequency + " SamplesPerPhysStep " + SamplesPerPhysStep + " horCurrentAngle " + horCurrentAngle);
        }

         private void FixedUpdate()
        {        
          RaycastJobs();  // uing the multithread (jobs) raycast function
        }

       private void RaycastJobs()
        {
            var results = new NativeArray<RaycastHit>(SamplesPerPhysStep, Allocator.TempJob);
            var commands = new NativeArray<RaycastCommand>(SamplesPerPhysStep, Allocator.TempJob);
            int i;

            if (horCurrentAngle >= MaxAngle && horCurrentAngle < MinAngle)
            { //completed horizontal scan
                horCurrentAngle = MinAngle;
                SensorRotator.localEulerAngles = new Vector3(0, MinAngle, 0);
            }
            if (horCurrentAngle >= 360)
            { //completed horizontal scan
                horCurrentAngle = 0;
                SensorRotator.localEulerAngles = new Vector3(0, 0, 0);
            }

            Vector3 scanerPos = SensorRotator.position;//  Vector3 ScanerLinearStep = ScannerVel * Time.fixedTime;
            int add_val = 0;
            float start_ang = horCurrentAngle;
            for (i = 0; i < SamplesPerPhysStep; i++)
            {
                //Debug.Log("horCurrentAngle " + horCurrentAngle + " max " + _maxAngle + " min " + _minAngle);
                if (horCurrentAngle >= MaxAngle && horCurrentAngle < MinAngle)
                {
                    start_ang = MinAngle;
                    SensorRotator.localEulerAngles = new Vector3(0, MinAngle, 0);
                    add_val = 0;
                }
                if (horCurrentAngle >= 360)
                {
                    start_ang = 0;
                    SensorRotator.localEulerAngles = new Vector3(0, 0, 0);
                    add_val = 0;
                }

                commands[i] = new RaycastCommand(scanerPos, SensorRotator.TransformDirection(Vector3.forward), 6);

                //if (DrawLidarDebug)
                //Debug.DrawRay(scanerPos, SensorRotator.TransformDirection(Vector3.forward) * 6, Color.green);
                
                add_val++;
                azimuth[i] = horCurrentAngle;
                //Debug.Log (horCurrentAngle);
                horCurrentAngle = start_ang + add_val * angle_res;
                SensorRotator.localEulerAngles = new Vector3(0, horCurrentAngle, 0);
            }


            // Schedule the batch of raycasts
            JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, 8, default(JobHandle));// m_ResWidth * ConfigRef.Channels
            // Wait for the batch processing job to complete
            handle.Complete();

            // commands.Dispose();

            for (int loc = 0; loc < results.Length; loc++)
            {
                var raycastHit = results[loc];
             
                if (raycastHit.collider != null){

                
                    lidar_data.dist[measurmensts_counter] = raycastHit.distance;
                       //Debug.Log("raycastHit" + raycastHit.distance);
                }
                else
                {
                     lidar_data.dist[measurmensts_counter] = 0;
                     //Debug.Log("raycastHit" + raycastHit.distance);
                }
                lidar_data.angle[measurmensts_counter] = azimuth[loc];
                if(measurmensts_counter == lidar_data.dist.Length -1){
                    measurmensts_counter = 0;
                    //for(int j = 0; j < 32; j++){
                        //Debug.Log(lidar_data.dist[j]);
                    //}
                    FillLaserScanMessage();
                    LaserScanPublisher.PublishMessage(message);
                }
                else{
                    measurmensts_counter++;
                }
            }
            results.Dispose();
            commands.Dispose();

        }

        private void FillLaserScanMessage(){
            //message.header.Update();
            message.angle_min       = lidar_data.angle[0] * Mathf.Deg2Rad;
            message.angle_max       = lidar_data.angle[lidar_data.dist.Length -1] * Mathf.Deg2Rad;
            message.angle_increment = angle_res * Mathf.Deg2Rad;
            message.time_increment  = time_increment;
            message.scan_time = scan_time;
            message.range_min       = 0.15f;
            message.range_max       = 6f;
            Array.Copy(lidar_data.dist, message.ranges, lidar_data.dist.Length); 
            //message.intensities     = intensities;
        }
    }
}
               

    
