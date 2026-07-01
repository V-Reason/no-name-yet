using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    private Dictionary<string, Queue<GameObject>> poolDict = new Dictionary<string, Queue<GameObject>>();

    public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        string key = prefab.name;
        GameObject obj;

        if (poolDict.ContainsKey(key) && poolDict[key].Count > 0)
        {
            obj = poolDict[key].Dequeue();
            obj.transform.position = pos;
            obj.transform.rotation = rot;
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab, pos, rot);
            obj.name = key;
        }
        // obj.OnSpawn();
        return obj;
    }

    public void Despawn(GameObject obj)
    {
        string key = obj.name;
        if (!poolDict.ContainsKey(key))
            poolDict.Add(key, new Queue<GameObject>());

        obj.SetActive(false);
        poolDict[key].Enqueue(obj);
        // obj.OnDespawn();
    }

    public void Clear() => poolDict.Clear();
}

// 可选
// public interface IPoolable
// {
//     void OnSpawn();
//     void OnDespawn();
// }