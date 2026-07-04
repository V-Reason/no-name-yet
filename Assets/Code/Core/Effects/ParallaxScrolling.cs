using UnityEngine;

public class ParallaxScrolling : MonoBehaviour
{
    [Header("视差系数")]
    [SerializeField] private float parallaxEffect = 1.0f;
    [Header("锁定Y轴")]
    [SerializeField] private bool freezeY = false;

    [Header("需要手动设置引用")]
    [Tooltip("相机位")]
    [SerializeField] private GameObject cam;

    private float begCamX;  // 相机原始位置
    private float begCamY;
    private float begBgX;   // 背景原始位置
    private float begBgY;

    // 循环偏移量
    private float loopOffsetX;
    private float spriteLength;

    void Start()
    {
        begCamX = cam.transform.position.x;
        begCamY = cam.transform.position.y;
        begBgX = transform.position.x;
        begBgY = transform.position.y;
        loopOffsetX = 0;
        spriteLength = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        Vector2 currCamPos = cam.transform.position;
        Vector2 currGgPos = transform.position;

        float camDeltaX = currCamPos.x - begCamX;
        float camDeltaY = currCamPos.y - begCamY;
        float parallaxMove = camDeltaX * parallaxEffect;
        if (!freezeY)
            transform.position = new Vector3(begBgX + parallaxMove + loopOffsetX, begBgY + camDeltaY);
        else
            transform.position = new Vector3(begBgX + parallaxMove + loopOffsetX, begBgY);


        float relativeMove = camDeltaX * (1 - parallaxEffect);
        if (relativeMove > loopOffsetX + spriteLength)
        {
            loopOffsetX += spriteLength;
        }
        else if (relativeMove < loopOffsetX - spriteLength)
        {
            loopOffsetX -= spriteLength;
        }
    }
}
