using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called before the first frame update
    public int damage;
    public bool isMelee;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            Destroy(gameObject, 3);

        }
        
    }
    void OnTriggerEnter(Collider other)
    {
        if (!isMelee&& other.gameObject.tag == "Wall")
        {
            Destroy(gameObject);
        }
        if(!isMelee && other.gameObject.tag == "Floor")
        {
            Destroy(gameObject, 3);
            
        }
       
    }

  
}
