using System.Collections;
//적을 계속 추적하기 위해 사용할 리스트 자료 구조 사용
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    //레벨이 시작되기 전 초 단위로 대기할 시간
    public float levelStartDelay = 2f;
    //턴 사이에 게임이 얼마 동안 대기할지 나타내는 변수
    public float turnDelay = 0.1f;
    //싱글톤(클래스 바깥에서도 접근 가능하기 위해 public)
    //static으로 선언된 변수의 값을 공유함
    public static GameManager instance = null;
    public BoardManager boardScript;
    public int playerFoodPoints = 100;
    [HideInInspector] public bool playersTurn = true;

    //레벨 숫자를 표시할 텍스트(Day 1...)
    private Text levelText;
    //
    private GameObject levelImage;
    private int level = 1;
    //적들의 위치 추적, 움직임 명령
    private List<Enemy> enemies;
    //
    private bool enemiesMoving;
    //게임 보드를 만드는 중인지 체크, 만드는 중에는 플레이어 움직임 방지
    private bool doingSetup;

    public Player thePlayer;
    //public bool waitUntilPlayerStop = true;

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

        thePlayer = FindObjectOfType<Player>();
    }

    //유니티 API 기본 제공 함수
    //씬이 로드될 때마다 호출됨
    private void OnLevelWasLoaded(int index)
    {
        level++;

        InitGame();
    }

    //게임이 시작될 때 적 리스트 초기화
    //이전 레벨에서의 적들 전부 정리
    void InitGame()
    {
        //타이틀 카드가 뜨는 동안 플레이어는 움직일 수 없음
        levelImage = GameObject.Find("LevelImage");
        levelText = GameObject.Find("LevelText").GetComponent<Text>();
        levelText.text = "Day " + level;
        levelImage.SetActive(true);
        //이미지가 보여진 뒤 꺼질 때까지 2초(levelStartDelay)가 걸림
        Invoke("HideLevelImage", levelStartDelay);

        doingSetup = true;
        enemies.Clear();
        boardScript.SetUpScene(level);
    }

    //레벨을 시작할 준비가 됐을 때 levelImage를 끄는 함수, InitGame 함수에서 Invoke(지연 시간 후 함수 호출)
    private void HideLevelImage()
    {
        levelImage.SetActive(false);
        //플레이어가 움직일 수 있게
        doingSetup = false;
    }

    public void GameOver()
    {
        levelText.text = "After " + level + " days, you starved.";
        levelImage.SetActive(true);
        //비활성화
        enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        //enemiesMoving: 적이 이동 중
        if (playersTurn || enemiesMoving || doingSetup)
            //아래 코드 실행하지 않음
            return;
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
        if (enemies.Count == 0)
        {
            //대기하는 적이 없어도 일단 플레이어가 기다리도록
            yield return new WaitForSeconds(turnDelay);
        }

        //적 리스트만큼 루프
        for (int i = 0; i < enemies.Count; i++)
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
