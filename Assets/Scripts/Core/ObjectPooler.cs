// Assets/_Scripts/Core/ObjectPooler.cs
using UnityEngine;
using System.Collections.Generic;

public enum PoolObjectType { Ball, Brick, ExplosionEffect, CoinEffect /* ... 기타 등등 */ }

[System.Serializable]
public class ObjectPoolItem
{
    public PoolObjectType type;
    public GameObject objectToPool;
    public int amountToPool;
    public bool shouldExpand = true; // 풀이 비었을 때 확장할지 여부
}

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;
    public List<ObjectPoolItem> itemsToPool;
    private Dictionary<PoolObjectType, List<GameObject>> pooledObjects;

    void Awake()
    {
        Instance = this;
        pooledObjects = new Dictionary<PoolObjectType, List<GameObject>>();

        foreach (ObjectPoolItem item in itemsToPool)
        {
            List<GameObject> objectList = new List<GameObject>();
            for (int i = 0; i < item.amountToPool; i++)
            {
                GameObject obj = Instantiate(item.objectToPool);
                obj.SetActive(false);
                objectList.Add(obj);
            }
            pooledObjects.Add(item.type, objectList);
        }
    }

    public GameObject GetPooledObject(PoolObjectType type)
    {
        if (!pooledObjects.ContainsKey(type))
        {
            Debug.LogError($"Pool for type {type} doesn't exist.");
            return null;
        }

        List<GameObject> objectList = pooledObjects[type];
        for (int i = 0; i < objectList.Count; i++)
        {
            if (!objectList[i].activeInHierarchy)
            {
                return objectList[i];
            }
        }

        // 확장 가능한 경우 새로 생성
        ObjectPoolItem item = itemsToPool.Find(x => x.type == type);
        if (item != null && item.shouldExpand)
        {
            GameObject obj = Instantiate(item.objectToPool);
            obj.SetActive(false);
            objectList.Add(obj);
            Debug.LogWarning($"Pool for {type} expanded.");
            return obj;
        }

        Debug.LogWarning($"Pool for {type} is empty and cannot expand.");
        return null;
    }

    public void ReturnToPool(GameObject obj, PoolObjectType type) // 사용하지 않을 가능성 높음. 각 객체가 SetActive(false)로 처리
    {
         if (!pooledObjects.ContainsKey(type))
        {
            Debug.LogWarning($"Trying to return object of type {type} but pool doesn't exist. Destroying.");
            Destroy(obj);
            return;
        }
        obj.SetActive(false);
        // pooledObjects[type].Add(obj); // 리스트에 다시 추가할 필요는 없음. 이미 리스트에 존재.
    }
}