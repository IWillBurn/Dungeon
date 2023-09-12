using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EntitiesKeys
{
    NONE = -1,
    PLAYER = 0,
}

public class EntitiesDictionary : MonoBehaviour
{
    [SerializeField] GameObject[] dictionary;
    public GameObject GetByKey(EntitiesKeys key)
    {
        return dictionary[(int)key];
    }
}
