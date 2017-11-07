using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        
        ULogger.Instance().Log("aaaaa", 15, "#ffe000");
        ULogger.Instance().Log("this", 10);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
