using System.Collections;
//적을 계속 추적하기 위해 사용할 리스트 자료 구조 사용
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    //턴 사이에 게임이 얼마 동안 대기할지 나타내는 변수
    public float turnDelay = 0.1f;
    //싱글톤(클래스 바깥에서도 접근 가능하기 위해 public)
    //static으로 선언된 변수의 값을 공유함
    public static GameManager instance = null;
    public BoardManager boardScript;
    public int playerFoodPoints = 100;
    [HideInInspector]public bool playersTurn = true;

    private int level = 3;
    //적들의 위치 추적, 움직임 명령
    private List<Enemy> enemies;
    //
    private bool enemiesMoving;

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

        //Enemy 타입의 리스트 동적 할당
        enemies = new List<Enemy>();
        boardScript = GetComponent<BoardManager>();
        InitGame();
    }

    void InitGame()
    {
        //게임이 시작될 때 적 리스트 초기화
        //이전 레벨에서의 적들 전부 정리
        enemies.Clear();
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
        //enemiesMoving: 적이 이동 중
        if(playersTurn || enemiesMoving)
            //아래 코드 실행하지 않음
            return;
        
        StartCoroutine(MoveEnemies());
    }

    //적들이 자신을 게임 매니저에 등록하도록 해서 게임 매니저가 적들이 움직이도록 명령할 수 있게 함
    public void AddEnemyToLisy(Enemy scripts)
    {
        enemies.Add(scripts);
    }

    //연속적으로 한 번에 하나씩 적을 옮기는 데 사용
    IEnumerator MoveEnemies()
    {
        enemiesMoving = true;
        //turnDelay: 0.1초
        yield return new WaitForSeconds(turnDelay);
        //적들이 아무도 없는지(첫 레벨인지)
        //적 리스트 길이를 enemies.Count로 체크
        if(enemies.Count == 0)
        {
            //대기하는 적이 없어도 일단 플레이어가 기다리도록
            yield return new WaitForSeconds(turnDelay);
        }

        //적 리스트만큼 루프
        for(int i = 0; i < enemies.Count; i++)
        {
            //MoveEnemy: 적들이 움직이도록 명령
            enemies[i].MoveEnemy();
            //moveTime: 0.1초
            yield return new WaitForSeconds(enemies[i].moveTime);
        }

        playersTurn = true;
        enemiesMoving = false;
    }
}
