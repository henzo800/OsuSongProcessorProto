using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatController : MonoBehaviour
{
    public int hitTime;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        this.transform.position += new Vector3(-GameManager.instance.Velocity * Time.fixedDeltaTime, 0,0);
    }

    void Delete() {
        Destroy(this.gameObject);
    }
    
}
