using System;
using System.Collections.Generic;
using UnityEngine;


public class Test : MonoBehaviour {

    void Start () {

        ULogger.Instance().Log("test", Color.gray);
        ULogger.Instance().Log("test", 15,Color.gray);
        ULogger.Instance().Log("test", 15, "FF0000");
    }
	

	void Update () {
	}
}
