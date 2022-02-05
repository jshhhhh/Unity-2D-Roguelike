using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingObject
{
    //적이 플레이어를 공격할 때 뺄셈할 음식 포인트
    public int playerDamage;

    private Animator animator;
    //플레이어 위치, 적이 어디로 향하는지 알려줌
    private Transform target;
    //적이 턴마다 움직이게 함
    private bool skipMove;
    public AudioClip enemyAttack1;
    public AudioClip enemyAttack2;

    //부모 클래스의 Start 함수를 재정의
    protected override void Start()
    {
        //Enemy 스크립트가 자기 스스로를 게임 매니저의 적 리스트에 더함
        //게임 매니저가 Enemy의 MoveEnemy 함수 호출 가능
        GameManager.instance.AddEnemyToLisy(this);
        animator = GetComponent<Animator>();
        //플레이어의 transform 저장
        target = GameObject.FindGameObjectWithTag("Player").transform;
        base.Start();
    }

    //입력형 T: 적이 상호작용할 것으로 예상되는 플레이어를 입력으로 함
    protected override void AttemptMove<T>(int xDir, int yDir)
    {
        //참이면 적 턴 스킵
        if(skipMove)
        {
            //턴이 돌아올 때만 움직일 수 있음
            skipMove = false;
            return;
        }

        //플레이어가 될 일반형 입력 T 입력
        //이동할 x방향, y방향을 함께 입력
        base.AttemptMove<T>(xDir, yDir);

        skipMove = true;
    }

    //적이 움직이려 할 때 GameManager에 의해 호출됨
    public void MoveEnemy()
    {
        int xDir = 0;
        int yDir = 0;

        //현재 transform 위치, target 위치 차이 체크(transform x좌표와 transform x좌표의 차이가 사실상 0인 앱실론보다 작은지 체크)
        //x좌표가 대충 같은지 체크(적과 플레이어가 같은 열에 속하는지)
        if(Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)
            //target y좌표가 transform y좌표보다 큰지 체크
            //참: 플레이어를 향해 위로 이동(1), 거짓: 플레이어를 향해 아래로 이동(-1)
            //수직 방향 이동
            yDir = target.position.y > transform.position.y ? 1 : -1;
        else
            //target x좌표가 transform x좌표보다 큰지 체크
            //수평 방향 이동
            //참: 오른쪽으로 이동(1), 거짓: 왼쪽으로 이동(-1)
            xDir = target.position.x > transform.position.x ? 1 : -1;

            //Player를 일반형 입력으로 넣고. x방향과 y방향도 입력
            AttemptMove<Player>(xDir, yDir)   ;         
    }

    //플레이어가 점거 중인 공간으로 적이 이동 시도할 때 호출됨
    //추상 함수로 선언된 MovingObject의 OnCantMove 재정의
    //플레이어를 가리킬 일반형 T
    //T 타입의 component 변수를 입력으로 받으
    protected override void OnCantMove<T>(T component)
    {
        //컴포넌트를 Player로 형변환 
        Player hitPlayer = component as Player;

        //데미지만큼 음식 점수를 잃음
        hitPlayer.LoseFood(playerDamage);
        
        animator.SetTrigger("enemyAttack");

        SoundManager.instance.RandomizeSfx(enemyAttack1, enemyAttack2);
    }
}
