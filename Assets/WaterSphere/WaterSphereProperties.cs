using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSphereProperties : MonoBehaviour
{
    private Vector3 velocity;
    private Vector3 accceleration;

    public Vector3 getVelocity()
    {
        return velocity;
    }

    public Vector3 getAcceleration()
    {
        return accceleration;
    }

    public void updateVelocity(float p_x, float p_y, float p_z)
    {
        velocity.x = p_x;
        velocity.y = p_y;
        velocity.z = p_z;
    }
    public void updateAcceleration(float p_x, float p_y, float p_z)
    {
        accceleration.x = p_x;
        accceleration.y = p_y;
        accceleration.z = p_z;
    }

    public void reset()
    {
        velocity = Vector3.zero;
        accceleration = Vector3.zero;
    }
}
