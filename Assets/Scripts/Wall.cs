using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    //벽을 한 번 때렸을 때 보여줄 스프라이트
    public Sprite dmgSprite;
    //벽의 히트 포인트(내구도)
    public int hp = 4;
    public AudioClip chopSound1;
    public AudioClip chopSound2;

    private SpriteRenderer spriteRenderer;

    private Player thePlayer;

    // Start is called before the first frame update
    void Awake()
    {
        //레퍼런스 가져옴
        spriteRenderer = GetComponent<SpriteRenderer>();
        thePlayer = FindObjectOfType<Player>();
    }

    public void DamageWall(int loss)
    {
        //벽을 칠 때마다 음식 감소
        thePlayer.consumeFood = true;
        SoundManager.instance.RandomizeSfx(chopSound1,chopSound2);
        //스프라이트를 교체해서 시각적인 변화
        spriteRenderer.sprite = dmgSprite;
        //남은 체력을 loss만큼 감소
        hp -= loss;

        if(hp <= 0)
            gameObject.SetActive(false);
    }
}
