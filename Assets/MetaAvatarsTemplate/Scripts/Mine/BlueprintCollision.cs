using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueprintCollision : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Blueprintpart")
        {
            //print("me " + gameObject.name + " im inside " + other.gameObject.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
