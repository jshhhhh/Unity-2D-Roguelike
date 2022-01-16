using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{
    public GameObject gameManager;

    // Start is called before the first frame update
    void Awake()
    {
        //인스턴스화돼있지 않다면 프리팹으로부터 하나 인스턴스화해옴
        if(GameManager.instance == null)
            Instantiate(gameManager);
    }
}
