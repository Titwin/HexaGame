using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkSimulator : MonoBehaviour {

    [Header("RigidBody list")]
    public Rigidbody[] rigidbodies;

    [Header("Legs references")]
    public LegKinematic[] legs;

    [Header("State parameters")]
    public bool dead = false;
    public Vector3 speed;
    public float stepLength;
    public float swapLimit;

    private int swingGroup = 0;
    private int groupCount = 2;

    void Start ()
    {
		if(!dead)
        {
            foreach (Rigidbody rb in rigidbodies)
                rb.isKinematic = true;
        }
	}
	
	
	void Update ()
    {
        speed = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Vector3.Normalize(speed);
        speed *= 2;



        if (dead)
        {
            foreach (Rigidbody rb in rigidbodies)
                rb.isKinematic = false;
        }
        else
        {
            foreach (Rigidbody rb in rigidbodies)
                rb.isKinematic = true;

            if(speed != new Vector3(0,0,0))
            {
                for (int i=0; i< legs.Length; i++)
                {
                    if ((i%groupCount) == swingGroup)
                        legs[i].IKsetPosition(legs[i].startPosition - speed * 0.5f * stepLength);
                    else
                        legs[i].IKsetPosition(legs[i].getPosition() + speed * Time.deltaTime);
                }
                transform.position += Time.deltaTime * speed;

                float limit = 30.0f;
                for (int i = 0; i < legs.Length; i++)
                {
                    if ((i % groupCount) != swingGroup)
                    {
                        float l = legs[i].getLimitDistance(speed, stepLength);
                        if (l < limit)
                            limit = l;
                    }
                }
                if(limit < swapLimit)
                {
                    swingGroup++;
                    swingGroup %= groupCount;
                }

                float y = 0.0f;
                for (int i = 0; i < legs.Length; i++)
                {
                    if ((i % groupCount) != swingGroup)
                        y += legs[i].getPosition().y;
                }
                Vector3 p = transform.position;
                transform.position = new Vector3(p.x, -y / 2, p.z);
            }
            else
            {
                foreach (LegKinematic l in legs)
                    l.IKsetPosition(l.startPosition);
                swingGroup = 0;
            }
        }
	}
}
