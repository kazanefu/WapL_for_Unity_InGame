using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadandCall : MonoBehaviour
{
    public GameObject interpreterobj;
    public WapLInterpreter interpreter;
    // Start is called before the first frame update
    void Start()
    {
        interpreter = interpreterobj.GetComponent<WapLInterpreter>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5)&& Input.GetKey(KeyCode.R))
        {
            interpreter.ReadInput();
        }
        if (Input.GetKeyDown(KeyCode.F5) && Input.GetKey(KeyCode.D))
        {
            interpreter.RunCode();
        }
    }
}
