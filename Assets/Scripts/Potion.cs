using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : MonoBehaviour
{
    public AudioClip clip;
    GameObject target;

    // Start is called before the first frame update
    void Start()
    {
        if(target == null)
        {
            target = GameObject.FindGameObjectWithTag("Player");
        }
    }
    
    void Update() {
        // ワールドのy軸に沿って1秒間に90度回転
        transform.Rotate(new Vector3(0, 90, 0) * Time.deltaTime, Space.World);
    }
    
    void OnCollisionEnter(Collision collision)
    { 
        if (collision.gameObject.tag == "Player") {    
            target.GetComponent<FPSController>().TakePotion();
            AudioSource.PlayClipAtPoint(clip,transform.position);  
            Destroy(this.gameObject);
        }
    }
}
