using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

public class WaterSphereController : MonoBehaviour
{
    public Vector3 spawnRadii = new Vector3(10, 2.5f, 2.5f);
    private List<WaterSphereProperties> currWaterSpheres = new List<WaterSphereProperties>();
    [SerializeField] WaterSpherePool spherePool;
    public Vector3 initialSize = Vector3.one;
    public bool useJobs = false;
    [SerializeField, Range(-9.8f, 9.8f)] private float gravity = -9.8f; //m/s

    public int numSpawnPerUpdate = 100;

    public int numSpheres;

    // Start is called before the first frame update
    void Start()
    {

    }

    public double updateTime;
    public double returnTime;

    // Update is called once per frame
    void Update()
    {
        int checkNum = 0;

        while (currWaterSpheres.Count < spherePool.maxSpawnAmount && (checkNum++ < numSpawnPerUpdate))
        {
            WaterSphereProperties newSphere = spherePool.getSphere();
            Vector3 origin = transform.position;
            float x = origin.x - spawnRadii.x + (spawnRadii.x * 2 * UnityEngine.Random.value);
            float y = origin.y - spawnRadii.y + (spawnRadii.y * 2 * UnityEngine.Random.value);
            float z = origin.z - spawnRadii.z + (spawnRadii.z * 2 * UnityEngine.Random.value);
            newSphere.transform.position = new Vector3(x, y, z);
            newSphere.transform.localScale = initialSize;
            currWaterSpheres.Add(newSphere);
        }

        numSpheres = currWaterSpheres.Count;
        double updateStart = Time.realtimeSinceStartupAsDouble;
        updateSpherePositions();
        updateTime = 1000 * (Time.realtimeSinceStartupAsDouble - updateStart);



    }

    private void updateSpherePositions()
    {
        if (!useJobs)
        {
            //Old way
            for (int i = currWaterSpheres.Count - 1; i >= 0; i--)
            {
                WaterSphereProperties currSphere = currWaterSpheres[i];



                Transform sphereTrans = currSphere.transform;
                if (sphereTrans.position.y < 0)
                {
                    spherePool.returnSphere(currWaterSpheres[i]);
                    currWaterSpheres.RemoveAt(i);
                }
                else
                {
                    //Update position using velocity
                    Vector3 currVelocity = currSphere.getVelocity();

                    Vector3 newPos = sphereTrans.position + (Time.deltaTime * currVelocity);
                    sphereTrans.position = newPos;

                    //Update acceleration and velocity
                    Vector3 currAccel = currSphere.getAcceleration();
                    float newAccel = currAccel.y + (gravity * Time.deltaTime);
                    float newYVel = currVelocity.y + (newAccel * Time.deltaTime);

                    currSphere.updateAcceleration(currAccel.x, newAccel, currAccel.z);
                    currSphere.updateVelocity(currVelocity.x, newYVel, currVelocity.z);
                }
            }

        }
        else
        {
            NativeArray<float3> posArray = new NativeArray<float3>(numSpheres, Allocator.TempJob);
            NativeArray<float3> velArray = new NativeArray<float3>(numSpheres, Allocator.TempJob);
            NativeArray<float3> accelArray = new NativeArray<float3>(numSpheres, Allocator.TempJob);

            for (int i = 0; i < numSpheres; i++)
            {
                posArray[i] = currWaterSpheres[i].transform.position;
                velArray[i] = currWaterSpheres[i].getVelocity();
                accelArray[i] = currWaterSpheres[i].getAcceleration();
            }

            UpdateSpheresJob parrallelUpdate = new UpdateSpheresJob
            {
                g = gravity,
                dt = Time.deltaTime,
                positionArray = posArray,
                velocityArray = velArray,
                acclerationArray = accelArray
            };

            JobHandle jobHandle = parrallelUpdate.Schedule(numSpheres, numSpheres / 10);
            jobHandle.Complete();

            //Update original values
            for (int i = numSpheres - 1; i >= 0; i--)
            {
                float currY = posArray[i].y;
                if (currY < 0)
                {
                    spherePool.returnSphere(currWaterSpheres[i]);
                    currWaterSpheres.RemoveAt(i);
                }
                else
                {
                    currWaterSpheres[i].transform.position = posArray[i];
                    currWaterSpheres[i].updateVelocity(velArray[i].x, velArray[i].y, velArray[i].z);
                    currWaterSpheres[i].updateAcceleration(accelArray[i].x, accelArray[i].y, accelArray[i].z);
                }
            }

            posArray.Dispose();
            velArray.Dispose();
            accelArray.Dispose();
        }
    }

    //Data to execute jobs for struct


    [BurstCompile]
    public struct UpdateSpheresJob : IJobParallelFor
    {
        public NativeArray<float3> positionArray;
        public NativeArray<float3> acclerationArray;
        public NativeArray<float3> velocityArray;
        public float g;
        public float dt;
        public void Execute(int p_index)
        {

            positionArray[p_index] += new float3(0, dt * velocityArray[p_index].y, 0);

            //Update acceleration and velocity
            acclerationArray[p_index] += new float3(0, (g * dt), 0);
            velocityArray[p_index] += new float3(0, (acclerationArray[p_index].y * dt), 0);
        }
    }


    public bool doDrawGizmos = true;
    private void OnDrawGizmos()
    {
        if (!doDrawGizmos) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, spawnRadii * 2);
    }
}
