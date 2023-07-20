using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSphereController : MonoBehaviour
{
    public Vector3 spawnRadii = new Vector3(10, 2.5f, 2.5f);
    private List<WaterSphereProperties> currWaterSpheres = new List<WaterSphereProperties>();
    [SerializeField] WaterSpherePool spherePool;
    public Vector3 initialSize = Vector3.one;
    public bool useJobs = false;
    [SerializeField, Range(-9.8f, 9.8f)] private float g = -9.8f; //m/s

    public int numSpawnPerUpdate = 100;

    public int numSpheres;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        int checkNum = 0;

        while (currWaterSpheres.Count < spherePool.maxSpawnAmount && (checkNum++ < numSpawnPerUpdate))
        {
            WaterSphereProperties newSphere = spherePool.getSphere();
            Vector3 origin = transform.position;
            float x = origin.x - spawnRadii.x + (spawnRadii.x * 2 * Random.value);
            float y = origin.y - spawnRadii.y + (spawnRadii.y * 2 * Random.value);
            float z = origin.z - spawnRadii.z + (spawnRadii.z * 2 * Random.value);
            newSphere.transform.position = new Vector3(x, y, z);
            newSphere.transform.localScale = initialSize;
            currWaterSpheres.Add(newSphere);
        }

        updateSpherePositions();

        removeUnecessarySpheres();
        numSpheres = currWaterSpheres.Count;
    }

    private void updateSpherePositions()
    {
        if (!useJobs)
        {
            //Old way
            foreach (WaterSphereProperties currSphere in currWaterSpheres)
            {

                Transform sphereTrans = currSphere.transform;
                //Update position using velocity
                Vector3 currVelocity = currSphere.getVelocity();

                Vector3 newPos = sphereTrans.position + (Time.deltaTime * currVelocity);
                sphereTrans.position = newPos;

                //Update acceleration and velocity
                Vector3 currAccel = currSphere.getAcceleration();
                float newAccel = currAccel.y + (g * Time.deltaTime);
                float newYVel = currVelocity.y + (newAccel * Time.deltaTime);

                currSphere.updateAcceleration(currAccel.x, newAccel, currAccel.z);
                currSphere.updateVelocity(currVelocity.x, newYVel, currVelocity.z);

            }

        }
        else
        {

        }
    }

    private void removeUnecessarySpheres()
    {
        for (int i = currWaterSpheres.Count - 1; i >= 0; i--)
        {
            Transform t = currWaterSpheres[i].transform;
            if (t.position.y < 0)
            {
                spherePool.returnSphere(currWaterSpheres[i]);
                currWaterSpheres.RemoveAt(i);
            }
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
