using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftHand : MonoBehaviour
{
    public static LeftHand Instance;
    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null) {
            Instance = this;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
