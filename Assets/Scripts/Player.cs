using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : MovingObject
{
    //벽을 부술 때 적용될 데미지
    public int wallDamage = 1;
    //음식을 먹을 때 스코어에 더해질 점수
    public int pointsPerFood = 10;
    public int pointsPerSoda = 20;
    public float restartLevelDelay = 1f;
    public Text foodText;
    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip eatSound1;
    public AudioClip eatSound2;
    public AudioClip drinkSound1;
    public AudioClip drinkSound2;
    public AudioClip gameOverSound;

    //애니메이터 컴포넌트의 레퍼런스 가져와 저장
    private Animator animator;
    //해당 레벨의 플레이어 스코어 저장
    private int food;
    //플레이어의 터치가 시작되는 지점 저장
    //-Vector2.one: 스크린 밖의 위치를 의미
    //터치 입력이 있는지 없는지 확인
    //실제 터치가 이루어지기 전까진 거짓값으로 초기화
    private Vector2 touchOrigin = -Vector2.one;

    //움직일 때 음식을 소모할 수 있는 상태(1씩 감소시키기 위해)
    public bool consumeFood = true;


    //Player의 Start를 구현
    protected override void Start()
    {
        animator = GetComponent<Animator>();

        //해당 레벨 동안 음식 점수를 관리
        food = GameManager.instance.playerFoodPoints;

        foodText.text = "Food: " + food;

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

        //스탠드 얼론 빌드 또는 웹 플레이어 실행 체크
        #if UNITY_STANDALONE || UNITY_WEBPLAYER

        //우: 1, 좌: -1 반환
        horizontal = (int)Input.GetAxisRaw("Horizontal");
        //상: 1, 하: -1 반환
        vertical = (int)Input.GetAxisRaw("Vertical");

        //대각선 움직임 방지
        if(horizontal != 0)
            vertical  = 0;

        //IOS, 안드로이드, 윈도우8 기타 등등
        #else

            //하나 이상의 터치를 감지했을 때
            if(Input.touchCount > 0)
            {
                //처음 터치 지점 저장
                //첫 번째 터치 지점만 잡아내고, 나머지 터치 전부 무시(한 방향에 대한 한 손가락의 스와이핑만 지원)
                Touch myTouch = Input.touches[0];

                //'터치가 막 시작(Began)' 상태인지 체크
                if(myTouch.phase == TouchPhase.Began)
                {
                    touchOrigin = myTouch.position;
                }

                //'터치가 막 끝남(Ended))' 상태인지 체크, touchOrigin이 0보다 크거나 같은지 체크
                //touchOrigin을 -1로 초기화해놓음
                //터치가 이루어졌다는 의미(초기화했던 값이 바뀜)
                else if(myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
                {
                    Vector2 touchEnd = myTouch.position;
                    //두 터치의 차(이동의 방향 알 수 있음)
                    float x = touchEnd.x - touchOrigin.x;
                    float y = touchEnd.y - touchOrigin.y;
                    //계속해서 true 반복하지 않게 초기화
                    touchOrigin.x = -1;

                    //방향에서 터치가 좀 더 가로쪽인지 세로쪽인지 찾아야 함
                    //절대값 비교로 유저가 스와이프하련 방향 체크
                    if(Mathf.Abs(x) > Mathf.Abs(y))
                        horizontal = x > 0? 1 : -1;
                    else
                        vertical = y > 0? 1 : -1;
                }
            }

        #endif
        
        //플레이어가 움직이려 한다면
        if(horizontal != 0 || vertical != 0)
        {
            //<T>: 상호작용할 오브젝트가 될 일반형 입력 -> 함수를 호출할 때 상호작용할 컴포넌트 특정 가능
            //일반형 변수 Wall -> 플레이어가 상호작용할 수 있는 벽에 대면할지도 모른다는 의미
            AttemptMove<Wall>(horizontal, vertical);
        }
    }

    //일반형 입력 T를 받음 -> 움직이는 오브젝트가 마주칠 대상의 컴포넌트의 타입을 가리킴
    //x방향, y방향(int) 받음
    protected override void AttemptMove<T>(int xDir, int yDir)
    {
        //움직일 때마다 음식 점수 1 잃음
        if(consumeFood)
        {
            consumeFood = false;
            food--;
        }
        foodText.text = "Food: " + food;

        //부모 클래스의 AttemptMove 호출
        //일반형 입력 T와 함께 정수형 x방향, y방항을 입력으로 넣어줌
        base.AttemptMove<T>(xDir, yDir);

        //Move함수에서 이루어진 라인캐스트 충돌 결과의 레퍼런스를 가져올 변수 선언
        RaycastHit2D hit;
        //Move가 true를 리턴하는지(플레이어가 움직일 수 있는지)
        if(Move(xDir,yDir, out hit))
        {
            //두 개 중 하나의 이동 효과음 재생
            //params 키워드를 통해 콤마가 하나의 배열로 합쳐 입력됨
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
        }

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
            //restartLevelDelay: 1초
            //1초간 정지 후 레벨 다시 시작
            Invoke("Restart", restartLevelDelay);
            enabled = false;
        }
        else if(other.tag == "Food")
        {
            //점수 추가 후 오브젝트 비활성화
            food += pointsPerFood;
            //음식 오브젝트를 먹었을 때 메시지 표시
            foodText.text ="+" + pointsPerFood + " Food: " + food;
            SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
            other.gameObject.SetActive(false);
        }
        else if(other.tag == "Soda")
        {
            //점수 추가 후 오브젝트 비활성화
            food += pointsPerSoda;
            //소다 오브젝트를 먹었을 때 메시지 표시
            foodText.text ="+" + pointsPerSoda + " Food: " + food;
            SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
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
        Application.LoadLevel(Application.loadedLevel);
        //SceneManager.GetActiveScene();
    }

    //적이 플레이어를 공격할 때 호출
    //loss: 플레이어가 잃는 점수
    public void LoseFood(int loss)
    {
        //애니메이터 트리거 발동
        animator.SetTrigger("playerHit");
        //loss만큼 점수 손실
        food -= loss;
        //공격당했을 때 잃은 점수 메시지 표시
        foodText.text = "-" + loss + " Food: " + food;
        //게임이 끝날 만큼의 점수를 잃었는지 체크
        CheckIfGameOver();
    }

    private void CheckIfGameOver()
    {
        if(food <= 0)
        {
            //게임 오버 사운드 재생
            SoundManager.instance.PlaySingle(gameOverSound);
            //musicSource 재생 멈춤
            SoundManager.instance.musicSource.Stop();
            GameManager.instance.GameOver();
        }
    }
}
