using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttractedTo : MonoBehaviour
{
    Rigidbody _rigidbody;
    public Transform attractedTo;
    public float maxMagnitude, strengthOfAttraction;
    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (attractedTo)
        {
            Vector3 direction = attractedTo.position - transform.position;
            _rigidbody.AddForce(strengthOfAttraction * direction);
            if (_rigidbody.velocity.magnitude > maxMagnitude)
            {
                _rigidbody.velocity = _rigidbody.velocity.normalized * maxMagnitude;
            }
        }
    }
}
