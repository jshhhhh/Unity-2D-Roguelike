using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//클래스와 클래스 멤버들을 만들 때 기능을 완성하지 않아도 되고,
//상속을 통해서 파생 클래스에서 구현함
public abstract class MovingObject : MonoBehaviour
{
    //오브젝트를 움직이게 할 시간 단위
    public float moveTime = 0.1f;
    //충돌이 일어났는지 체크할 장소
    public LayerMask blockingLayer;

    private BoxCollider2D boxCollider;
    //움직일 유닛의 rigidbody2d 컴포넌트의 레퍼런스 저장
    private Rigidbody2D rb2D;
    //움직인을 더 효율적으로 계산?
    private float inverseMoveTime;
    //Is the object currently moving.
    public bool isMoving;

    public Player thePlayer;

    //protected virtual 함수: 자식 클래스가 덮어써서 재정의 가능(오버라이드)
    protected virtual void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        //moveTime의 역수 -> 나누기 대신 곱하기로 효율적인 계산
        inverseMoveTime = 1f / moveTime;
        thePlayer = FindObjectOfType<Player>();
    }

    //out: 입력을 레퍼런스로 받음(Move 함수가 두 개 이상의 값을 리턴하기 위해)
    protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
    {
        Vector2 start = transform.position;
        //입력받은 방향값들을 기반으로 끝나는 위치 계산
        Vector2 end = start + new Vector2(xDir, yDir);

        //Ray를 사용할 때 자기 자신에 부딪치지 않게
        boxCollider.enabled = false;
        //시작지점에서 끝지점까지의 라인을 가져옴(BlockingLayer와의 충돌 검사)
        hit = Physics2D.Linecast(start, end, blockingLayer);
        boxCollider.enabled = true;

        //뭔가 부딪쳤는지 체크
        //null이면 이동 가능
        if (hit.transform == null && !isMoving)
        {
            StartCoroutine(SmoothMovement(end));
            //이동했다는 뜻
            return true;
        }

        //이동 실패했다는 뜻
        return false;
    }

    //한 공간에서 다른 곳으로 옮기는 데 사용되는 코루틴
    //end: 어디로 이동할 건지 표시
    protected IEnumerator SmoothMovement(Vector3 end)
    {
        //The object is now moving.
        isMoving = true;

        //end와 현재 위치의 차이 벡터에 sqrMagintude로 거리를 구함
        //Magintude: 벡터 길이, sqrMagnitude: 벡터 길이 제곱
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

        //float.Epsilon: 0에 가까운 작은 수
        while (sqrRemainingDistance > float.Epsilon)
        {
            //Vector3.MoveTowards: 현재 포인트를 직선상의 목표 포인트로 이동시킴
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
            //찾은 새 지점으로 이동
            rb2D.MovePosition(newPosition);
            //이동 이후의 남은 거리 다시 계산
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            //루프를 갱신하기 전 다음 프레임을 기다림
            yield return null;
        }

        //Make sure the object is exactly at the end of its movement.
        rb2D.MovePosition(end);

        //The object is no longer moving.
        isMoving = false;
        thePlayer.consumeFood = true;
    }

    //일반형(Generic) 입력 T는 막혔을 때 컴포넌트 타입을 가리키기 위해 사용
    //적: 상대->플레이어, 플레이어: 상대->벽
    //where T : Component -> T는 컴포넌트 타입(제약조건)
    protected virtual void AttemptMove<T>(int xDir, int yDir)
        where T : Component
    {
        RaycastHit2D hit;
        //이동 성공: true, 실패: false 반환
        bool canMove = Move(xDir, yDir, out hit);

        //Move에서 부딪친 transform이 null인지 확인
        if (hit.transform == null)
            //Move에서 라인캐스트가 다른 것과 부딪치지 않았다면 리턴 이후 코드를 실행하지 않음
            return;

        //무언가와 부딪쳤다면, 충돌한 오브젝트의 컴포넌트의 레퍼런스를 T타입의 컴포넌트에 할당
        T hitComponent = hit.transform.GetComponent<T>();

        //움직이던 오브젝트가 막혔고, 상호작용할 수 있는 오브젝트와 충돌함
        if (!canMove && hitComponent != null)
        {
            OnCantMove(hitComponent);
        }
    }

    //일반형(Generic) 입력 T를 T형의 component라는 변수로써 받아옴
    //추상메서드이므로 불완전하고, 자식 클래스의 함수로 기능 완성
    protected abstract void OnCantMove<T>(T component)
        where T : Component;
}
