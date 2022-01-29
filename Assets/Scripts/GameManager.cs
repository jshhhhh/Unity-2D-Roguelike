using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //싱글톤(클래스 바깥에서도 접근 가능하기 위해 public)
    //static으로 선언된 변수의 값을 공유함
    public static GameManager instance = null;
    public BoardManager boardScript;
    public int playerFoodPoints = 100;
    [HideInInspector]public bool playersTurn = true;

    private int level = 3;

    // Start is called before the first frame update
    void Awake()
    {
        #region Singleton
        if (instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            //이 오브젝트를 다른 씬을 불러올 때마다 파괴시키지 말라는 명령어
            DontDestroyOnLoad(this.gameObject);
            instance = this;
        }
        #endregion Singleton

        boardScript = GetComponent<BoardManager>();
        InitGame();
    }

    void InitGame()
    {
        boardScript.SetUpScene(level);
    }

    public void GameOver()
    {
        //비활성화
        enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
