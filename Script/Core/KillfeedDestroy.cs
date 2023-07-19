using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KillfeedDestroy : MonoBehaviour

{

    private void Start() {
        Invoke("Kill",2f);
    }

    void Kill()
    {

        Destroy(gameObject);
        
    }





}