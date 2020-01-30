/*
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

namespace RosSharp.RosBridgeClient
{
    public class LaserScanReader : MonoBehaviour
    {
        private Ray[] rays;
        private RaycastHit[] raycastHits;
        private Vector3[] directions;
        private LaserScanVisualizer[] laserScanVisualizers;
        private float currentRayDirection = 0;
        private float lastRayDirection = 0;
        float NewTurnTime = 0;
        private float rayResolution = 0;

        

        public int samples = 0;
        public int rotationFrequency = 0;
        public int update_rate = 1800;
        public float angle_min = 0;
        public float angle_max = 6.28f;
        public float angle_increment = -0.0174533f;
        public float time_increment = 0;
        public float scan_time = 0;
        public float range_min = 0.12f;
        public float range_max = 3.5f;
        public float[] ranges;
        public float[] intensities;

        public void Start()
        {
            directions = new Vector3[samples];
            ranges = new float[samples];
            intensities = new float[samples];
            rays = new Ray[samples];
            raycastHits = new RaycastHit[samples];
            rayResolution = (float)360/samples;
            Debug.Log("ray resolution:  " + rayResolution);

        }

        public float[] Scan()
        {

            //Scan is called every fixed update which is 0.02 sec. in this
            
          RaycastJobs();  // uing the multithread (jobs) raycast function


            //laserScanVisualizers = GetComponents<LaserScanVisualizer>();
            //if (laserScanVisualizers != null)
            //Debug.Log("visualizer is activated");
            //    foreach (LaserScanVisualizer laserScanVisualizer in laserScanVisualizers)
              //      laserScanVisualizer.SetSensorData(gameObject.transform, directions, ranges, range_min, range_max);

            return ranges;
        }

       private void RaycastJobs()
        {

            //For one ray samples = 1
            rays = new Ray[samples];
            raycastHits = new RaycastHit[samples];
            ranges = new float[samples];
            directions = new Vector3[samples];
            currentRayDirection = 0;

            // the function Quaternion.euler input is in degrees

            // Perform a single raycast using RaycastCommand and wait for it to complete
            // Setup the command and result buffers
            var results = new NativeArray<RaycastHit>(samples, Allocator.TempJob);

            var commands = new NativeArray<RaycastCommand>(samples, Allocator.TempJob);

            float timeBeforeJob =  Time.realtimeSinceStartup;

            int i = 0;
            while (i  < samples)
            {
               currentRayDirection = i * rayResolution;
               commands[i] = new RaycastCommand(transform.position, Quaternion.Euler(new Vector3(0, currentRayDirection, 0)) * transform.forward);
               i++;     
            }

            Debug.Log("commands length:  " + commands.Length);
            // Schedule the batch of raycasts
            JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, 8, default(JobHandle));

            // Wait for the batch processing job to complete
            handle.Complete();

            //var sb = new StringBuilder();
            // Copy the result. If batchedHit.collider is null there was no hit
            
            
            for (int j=0; j<commands.Length; j++)
            {

                ranges[j] = results[j].distance;

              //  sb.Append(hit.distance +",");
              //  if(hit.collider!=null)
               // {
                   
                    
               // }
               // else
               // {
                    //DontHit
               // }
            }
            
            // Debug.Log("results are: " + sb.ToString());

            Debug.Log("Time After another set of rays (Job) :  " + (Time.realtimeSinceStartup - timeBeforeJob));
            // Dispose the buffers
            results.Dispose();
            commands.Dispose();
            
        }
    }

    
}