using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenesManager : MonoBehaviour
{
    public static ScenesManager instance;
    public static bool canStart;

    // Start is called before the first frame update
    void Start()
    {
        canStart = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void SetCanStart(bool value)
    {
        canStart = value;
    }
}
