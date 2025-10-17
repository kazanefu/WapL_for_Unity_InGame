using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadandCall : MonoBehaviour
{
    public GameObject interpreterobj;
    public WapLInterpreter interpreter;
    public GameObject Ifield;
    public GameObject Ofield;
    public bool Roop;
    // Start is called before the first frame update
    void Start()
    {
        interpreter = interpreterobj.GetComponent<WapLInterpreter>();
        Roop = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5) && Input.GetKey(KeyCode.D)&&Input.GetKey(KeyCode.F))
        {
            Roop = true;
        }
        if (Input.GetKeyDown(KeyCode.F4) && Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.F))
        {
            Roop = false;
        }
        if (Input.GetKeyDown(KeyCode.F5)&& Input.GetKey(KeyCode.R))
        {
            interpreter.ReadInput();
        }
        if (Input.GetKeyDown(KeyCode.F5) && Input.GetKey(KeyCode.D))
        {
            interpreter.RunCode();
        }
        if (Roop)
        {
            interpreter.RunCode();
        }
        if (Input.GetKeyDown(KeyCode.Escape) && Input.GetKey(KeyCode.D))
        {
            Ofield.SetActive(false);
            Ifield.SetActive(false);
        }
        if (Input.GetKeyDown(KeyCode.Escape) && Input.GetKey(KeyCode.R))
        {
            Ofield.SetActive(true);
            Ifield.SetActive(true);
        }
    }
}
