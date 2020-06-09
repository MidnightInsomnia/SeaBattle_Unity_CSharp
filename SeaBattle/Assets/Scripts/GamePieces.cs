using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePieces : MonoBehaviour
{
    //Список картинок для смены на желаемую букву/цифру
    public Sprite[] imgs;

    //Индекс текущей картинки
    public int imgIndex = 0;

    public bool HidePiece = false;

    //Метод смены картинок, проверяется каждый кадр
    void ChangeImgs()
    {
        if(imgs.Length > imgIndex)
        {
            if((HidePiece) && (imgIndex == 1))
            {
                GetComponent<SpriteRenderer>().sprite = imgs[0];
            }
            else
            {
                //Передача картинки в параметр sprite блока Sprite Renderer в Unity
                GetComponent<SpriteRenderer>().sprite = imgs[imgIndex];
            }            
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        ChangeImgs();
    }

    // Update is called once per frame
    void Update()
    {
        ChangeImgs();
    }
}
