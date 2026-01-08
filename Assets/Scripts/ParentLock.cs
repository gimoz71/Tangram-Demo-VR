using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParentLock : MonoBehaviour
{

    Rigidbody rb;
    public Text time;
    public Text text;
    public float snapTime = 2;

    public AudioSource lockedPit;

    public AudioClip pitOn;

    private float dropTimer;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (time)
        {
            time.text = "" + dropTimer;
        }
    }

    void  OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "pit")
        {

        if (text) { 
            text.text = "oggetto" + collider.gameObject.name;
        }
            rb.isKinematic = true;
            
            Debug.Log("ATTACHED");

            //lockedPit.PlayOneShot(pitOn, 1f);
        }
    }

    private void OnTriggerStay(Collider collider)
    {
        if (collider.gameObject.tag == "pit")
        {

            dropTimer += Time.deltaTime / (snapTime / 2);

            rb.isKinematic = dropTimer > 1;

            if (dropTimer > 1)
            {
                //transform.parent = snapTo;
                //transform.parent = collider.gameObject.transform;
                transform.position = collider.gameObject.transform.position;
                transform.rotation = collider.gameObject.transform.rotation;
                
            }
            else
            {
                float t = Mathf.Pow(35, dropTimer);

                rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * 4);
                if (rb.useGravity)
                    rb.AddForce(-Physics.gravity);

                transform.position = Vector3.Lerp(transform.position, collider.gameObject.transform.position, Time.fixedDeltaTime * t * 3);
                transform.rotation = Quaternion.Slerp(transform.rotation, collider.gameObject.transform.rotation, Time.fixedDeltaTime * t * 2);
            }
           
            Debug.Log("LOCKED");
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.tag == "pit")
        {
            dropTimer = 0;
            if (text)
            {
                text.text = "DETACHED";
            }
            rb.isKinematic = false;
            //transform.parent = null;
            //transform.position = collider.gameObject.transform.position;
            Debug.Log("DETACHED");
        }
    }
}
