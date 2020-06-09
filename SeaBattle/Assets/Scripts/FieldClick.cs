using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldClick : MonoBehaviour
{
    //Ссылка на объект игрового поля. Нужно чтобы понять на чать какого именно поля нажали
    public GameObject FieldOwner = null;    
    //Позиция ячейки на поле
    public int CoordX, CoordY;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Обработчик нажатия
    void OnMouseDown()
    {
        //Проверка наличии ссылки на поле
        if (FieldOwner != null)
        {            
            FieldOwner.GetComponent<GameField>().WhoClick(CoordX, CoordY);
        }
    }
}
