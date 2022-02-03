using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MovingObject
{
    //벽을 부술 때 적용될 데미지
    public int wallDamage = 1;
    //음식을 먹을 때 스코어에 더해질 점수
    public int pointsPerFood = 10;
    public int pointsPerSoda = 20;
    public float restartLevelDelay = 1f;

    //애니메이터 컴포넌트의 레퍼런스 가져와 저장
    private Animator animator;
    //해당 레벨의 플레이어 스코어 저장
    private int food;


    //Player의 Start를 구현
    protected override void Start()
    {
        animator = GetComponent<Animator>();

        //해당 레벨 동안 음식 점수를 관리
        food = GameManager.instance.playerFoodPoints;

        //부모 클래스의 Start() 실행
        base.Start();
    }

    //오브젝트가 비활성화되는 순간 호출되는 함수
    private void OnDisable()
    {
        //레벨이 바뀔 때마다 게임매니저로 다시 저장
        GameManager.instance.playerFoodPoints = food;
    }

    // Update is called once per frame
    void Update()
    {
        //현재 플레이어의 턴인지 체크
        if(!GameManager.instance.playersTurn)
            //턴이 아니라면 return으로 이하 코드 실행 방지
            return;
        
        //수평 혹은 수직으로 움직이는 방향을 1 또는 -1로 저장해서 사용
        int horizontal = 0;
        int vertical = 0;

        //우: 1, 좌: -1 반환
        horizontal = (int)Input.GetAxisRaw("Horizontal");
        //상: 1, 하: -1 반환
        vertical = (int)Input.GetAxisRaw("Vertical");

        //대각선 움직임 방지
        if(horizontal != 0)
            vertical  = 0;
        
        //플레이어가 움직이려 한다면
        if(horizontal != 0 || vertical != 0)
            //<T>: 상호작용할 오브젝트가 될 일반형 입력 -> 함수를 호출할 때 상호작용할 컴포넌트 특정 가능
            //일반형 변수 Wall -> 플레이어가 상호작용할 수 있는 벽에 대면할지도 모른다는 의미
            AttemptMove<Wall>(horizontal, vertical);
    }

    //일반형 입력 T를 받음 -> 움직이는 오브젝트가 마주칠 대상의 컴포넌트의 타입을 가리킴
    //x방향, y방향(int) 받음
    protected override void AttemptMove<T>(int xDir, int yDir)
    {
        //움직일 때마다 음식 점수 1 잃음
        food--;

        //부모 클래스의 AttemptMove 호출
        //일반형 입력 T와 함께 정수형 x방향, y방항을 입력으로 넣어줌
        base.AttemptMove<T>(xDir, yDir);

        //Move함수에서 이루어진 라인캐스트 충돌 결과의 레퍼런스를 가져올 변수 선언
        RaycastHit2D hit;

        //움직이면서 음식 점수를 잃기 때문에 체크
        CheckIfGameOver();

        //플레이어의 턴이 끝났음을 알림
        GameManager.instance.playersTurn = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //충돌한 오브젝트의 태그 체크
        if(other.tag == "Exit")
        {
            //restartLevelDelay: 1초?
            //1초간 정지 후 레벨 다시 시작
            Invoke("Restart", restartLevelDelay);
            enabled = false;
        }
        else if(other.tag == "Food")
        {
            //점수 추가 후 오브젝트 비활성화
            food += pointsPerFood;
            other.gameObject.SetActive(false);
        }
        else if(other.tag == "Soda")
        {
            //점수 추가 후 오브젝트 비활성화
            food += pointsPerSoda;
            other.gameObject.SetActive(false);
        }
    }

    //MovingObject에서 선언한 추상 함수 내부 구현
    protected override void OnCantMove<T>(T component)
    {
        //입력으로 받은 component 값을 wall로 변환
        Wall hitWall = component as Wall;
        //때린 벽의 DamageWall 함수 호출
        //플레이어가 벽에 얼마나 데미지를 줄지 알리기 위해 wallDamage를 입력
        hitWall.DamageWall(wallDamage);
        //애니메이터 트리거 발동
        animator.SetTrigger("playerChop");
    }

    //출구 오브젝트와 충동했을 때 레벨을 다시 로드
    private void Restart()
    {
        //마지막에 로드된 씬을 로드한다는 의미(유일한 씬인 main)
        //다른 레벨을 로드하기 위해 다른 씬을 불러올 필요X(레벨을 스크립트로 생성)
        //Application.LoadLevel(Application.loadedLevel);
        SceneManager.GetActiveScene();
    }

    //적이 플레이어를 공격할 때 호출
    //loss: 플레이어가 잃는 점수
    public void LoseFood(int loss)
    {
        //애니메이터 트리거 발동
        animator.SetTrigger("playerHit");
        //loss만큼 점수 손실
        food -= loss;
        //게임이 끝날 만큼의 점수를 잃었는지 체크
        CheckIfGameOver();
    }

    private void CheckIfGameOver()
    {
        if(food == 0)
            GameManager.instance.GameOver();
    }
}
