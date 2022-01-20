using System.Collections;
//리스트 사용
using System.Collections.Generic;
using UnityEngine;
//직렬화 속성 사용
using System;
//
using Random = UnityEngine.Random;


public class BoardManager : MonoBehaviour
{
    public class Count
    {
        public int minimum;
        public int maximum;
        //생성자
        public Count(int min, int max)
        {
            minimum = min;
            maximum = max;
        }
    }

    //게임 보드를 위한 공간을 그림
    //8X8 사이즈의 게임 보드
    /*
    rows
     7
     6
     5
     4
     3
     2
     1
     0
    -1
      -1 0  1  2  3  4  5  6  7 column
    */
    //가장자리(이동 불가): -1행 -1열, columns + 1열, rows + 1행
    //내부 테두리: 0행 0열, columns열, rows행
    //테두리를 제외한 내부: 1행 1열, columns - 1열, rows - 1행
    public int columns = 8;
    public int rows = 8;
    //레벨마다 얼마나 많은 벽을 랜덤하게 생성할지 설정
    //최소 5개, 최대 9개의 벽 생성
    public Count wallCount = new Count(5, 9);
    //음식 개수 설정
    public Count foodCount = new Count(1, 5);
    //출구는 한 개만
    public GameObject exit;
    //나머지 오브젝트는 배열을 사용
    public GameObject[] floorTiles;
    public GameObject[] wallTiles;
    public GameObject[] foodTiles;
    public GameObject[] enemyTiles;
    public GameObject[] outerWallTiles;

    //하이어라키를 깨끗이 해놓기 위해 사용
    //많은 오브젝트를 소환하고 이 변수의 자식으로 모두 넣어줌
    private Transform boardHolder;
    //게임의 가능한 모든 다른 위치들을 추적하기 위해 사용
    //오브젝트가 해당 장소에 있는지 없는지도 추적
    private List <Vector3> gridPositions = new List<Vector3>();

    //리스트 초기화 함수
    void InitializeList()
    {
        //Clear() 함수로 초기화
        gridPositions.Clear();

        //x좌표(왼쪽부터)
        //floor 타일의 테두리를 남겨두기 위해 1을 뺌
        for(int x = 1; x < columns - 1; x++)
        {
            //y좌표(아래부터)
            for(int y = 1; y < rows - 1; y++)
            {
                //Vector3를 격자형 위치 리스트에 더함
                gridPositions.Add(new Vector3(x, y, 0f));
            }
        }
    }

    //바깥벽과 게임 보드의 바닥을 짓는 함수
    //바닥 타일들에 바깥 벽 타일을 배치
    void BoardSetup()
    {
        //Board의 transform 대입
        boardHolder = new GameObject ("Board").transform;

        //게임 보드의 가장자리를 포함하기 위한 범위 지정
        for(int x = -1; x < columns + 1; x++)
        {
            for(int y = -1; y < rows + 1; y++)
            {
                //0에서 floorTile 길이 사이에서 랜덤하게 골라온 floorTiles 오브젝트 배열의 인덱스를 대입(인스턴스화 준비)
                GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
                //가장자리일 경우(이동 불가 지역)
                if(x == -1 || x == columns || y == -1 || y == rows)
                    //바깥 벽 타일의 배열 중 고른 타일로 인스턴스화 준비
                    toInstantiate = outerWallTiles[Random.Range(0,  outerWallTiles.Length)];
                
                //인스턴스화하려는 오브젝트 할당
                //toInstantiate 프리펩을 현재 x, y 좌표에 회전값 없이 게임 오브젝트로 할당
                GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;

                //부모 오브젝트를 boardHolder로 설정
                instance.transform.SetParent(boardHolder);
            }
        }
    }

    //게임 보드 위에 랜덤 오브젝트(벽, 적, 파워 업 등) 배치
    Vector3 RandomPosition()
    {
        //gridPosition의 개수 범위 안에서 인덱스를 랜덤으로 생성
        int randomIndex = Random.Range(0, gridPositions.Count);
        //randomIndex에 있는 격자 위치 값을 넣어줌
        Vector3 randomPosition = gridPositions[randomIndex];
        //사용한 격자 위치를 리스트에서 제거
        //(같은 장소에 두 개 이상의 오브젝트 생성 방지)
        gridPositions.RemoveAt(randomIndex);
        //랜덤한 위치 값 리턴
        return randomPosition;
    }

    //선택한 장소에 실제로 타일 소환 함수
    void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
    {
        //objectCount로 주어진 오브젝트를 얼마나 스폰할지 조정
        int objectCount = Random.Range(minimum, maximum + 1);
        //objectCount만큼 오브젝트 소환
        for(int i = 0; i < objectCount; i++)
        {
            //랜덤 위치 가져옴
            Vector3 randomPosition = RandomPosition();
            //tileArray로부터 랜덤 값을 생성하여 넣음
            GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];
            //타일을 랜덤한 위치에 인스턴스화시킴
            Instantiate(tileChoice, randomPosition, Quaternion.identity);
        }
    }

    //레벨에 따라 배치
    public void SetUpScene(int level)
    {
        BoardSetup();
        InitializeList();
        //오브젝트 배치
        LayoutObjectAtRandom(wallTiles, wallCount.minimum, wallCount.maximum);
        LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);
        //Mathf.Log로 레벨 수에 따라 적 생성
        //Mathf.Log는 float형을 리턴, 정수형으로 변환
        //레벨2->적1, 레벨4->적2, 레벨8->적3...
        int enemyCount = (int)Mathf.Log(level, 2f);
        //적 배치(minimum값과 maximum값이 같음)
        LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);
        //출구 인스턴스화(언제나 레벨의 가장 위 오른쪽 구석에 위치)
        Instantiate(exit, new Vector3(columns - 1, rows - 1, 0f), Quaternion.identity);
    }
}
