using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    //벽을 한 번 때렸을 때 보여줄 스프라이트
    public Sprite dmgSprite;
    //벽의 히트 포인트(내구도)
    public int hp = 4;

    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Awake()
    {
        //레퍼런스 가져옴
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void DamageWall(int loss)
    {
        //스프라이트를 교체해서 시각적인 변화
        spriteRenderer.sprite = dmgSprite;
        //남은 체력을 loss만큼 감소
        hp -= loss;

        if(hp <= 0)
            gameObject.SetActive(false);
    }
}
