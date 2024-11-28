using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ArrowPooler : MonoBehaviour
{
    public static ArrowPooler instance;

    // the prefab to be pooled
    public GameObject prefab;
    // the number of objects in the pool
    public int poolSize;
    // can the pools size be changed
    public bool expandable;

    public Text arrowQuantityText;

    private HashSet<GameObject> InUseObjects;
    private HashSet<GameObject> pooledObjects;

    // initialise the pool with 'poolSize' number of objects
    private void Awake()
    {
        instance = this;

        InUseObjects = new HashSet<GameObject>();
        pooledObjects = new HashSet<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GenerateNewObject();
        }

        AdjustArrowText();
    }

    // returns an object from the pool. if the pool is empty and expanding the pool is allowed then a new instance is instantiated
    public GameObject GetObject()
    {
        GameObject obj = pooledObjects.FirstOrDefault();

        if (obj == null && !expandable)
        {
            return null;
        }
        else if (obj == null)
        {
            obj = GenerateNewObject();
        }

        pooledObjects.Remove(obj);
        InUseObjects.Add(obj);
        obj.SetActive(true);

        AdjustArrowText();

        return obj;
    }

    // places an object back into the pool. (only works if the object came from the pool to begin with)
    public void ReturnObject(GameObject obj)
    {
        if (InUseObjects.Contains(obj))
        {
            obj.SetActive(false);
            InUseObjects.Remove(obj);
            pooledObjects.Add(obj);
        }
        AdjustArrowText();
    }

    // instantiates a new instance of the prefab and adds it to the pool
    private GameObject GenerateNewObject()
    {
        GameObject newObject = Instantiate(prefab);
        newObject.transform.parent = transform;
        newObject.SetActive(false);
        pooledObjects.Add(newObject);

        return newObject;
    }

    private void AdjustArrowText()
    {
        arrowQuantityText.text = pooledObjects.Count.ToString();
    }
}
