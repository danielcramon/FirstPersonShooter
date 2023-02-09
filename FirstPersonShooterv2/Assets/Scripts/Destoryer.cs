using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destoryer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(KillObject), 5f);
    }

    void KillObject()
    {
        Destroy(gameObject);
    }
}
