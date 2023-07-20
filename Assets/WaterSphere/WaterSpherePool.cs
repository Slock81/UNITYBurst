using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class WaterSpherePool : MonoBehaviour
{

    [SerializeField] private WaterSphereProperties prefab;

    [SerializeField, Range(1, 40000)] public int maxSpawnAmount;
    private ObjectPool<WaterSphereProperties> pool;

    void Awake()
    {
        pool = new ObjectPool<WaterSphereProperties>(createSphere, OnTakeFromPool, OnReturnToPool, OnDestroyPooledObject, false, maxSpawnAmount, maxSpawnAmount);
    }

    private void OnDestroy()
    {
        pool.Dispose();
    }

    public WaterSphereProperties getSphere()
    {
        return pool.Get();
    }

    public void returnSphere(WaterSphereProperties p_sphere)
    {
        pool.Release(p_sphere);
    }


    /**Hidden from user Complexity **/
    WaterSphereProperties createSphere()
    {
        WaterSphereProperties newSphere = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        newSphere.transform.parent = transform;
        return newSphere;
    }



    void OnReturnToPool(WaterSphereProperties p_sphere)
    {
        p_sphere.reset();
        p_sphere.gameObject.SetActive(false);
    }

    void OnTakeFromPool(WaterSphereProperties p_sphere)
    {
        p_sphere.gameObject.SetActive(true);
    }

    void OnDestroyPooledObject(WaterSphereProperties p_sphere)
    {
        Destroy(p_sphere.gameObject);
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override string ToString()
    {
        return base.ToString();
    }
}
