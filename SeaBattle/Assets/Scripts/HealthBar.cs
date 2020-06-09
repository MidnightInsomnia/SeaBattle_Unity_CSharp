using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{

    public GameObject HealthPiece,      //Блок хранения внешнего вида ячеек поля
                      GameField;        //Функция, получающая от поля количество живых палуб

    //Панель, отображения количества живых палуб на поле
    GameObject[] healthBar = new GameObject[20];

    void CreateHealthBar()
    {
        //Получаем точку в которой будет создано поле
        Vector3 GetPositionOnScreen = this.transform.position;
        //Смещение относительно точки создания поля
        float DX = 0.5f;

        for( int I = 0; I < 20; I++)
        {
            //Создаём 1 ячейку здоровья
            healthBar[I] = Instantiate(HealthPiece) as GameObject;
            //Задаём ей позицию
            healthBar[I].transform.position = GetPositionOnScreen;
            //Смещаем позицию на указанное расстояние
            GetPositionOnScreen.x += DX;
        }
    }

    //Метод обновления шкалы здоровья
    void RefreshHealth()
    {
        int L = 0;
        //Обнуление
        for(int I = 0; I < 20; I++)
        {
            healthBar[I].GetComponent<GamePieces>().imgIndex = 0;
        }

        //Получение количества HitPoint-ов через ссылку на поле
        if(GameField != null)
        {
            L = GameField.GetComponent<GameField>().ShipsAlive();
        }        

        //Передача количества HP в Bar
        for(int I = 0; I < L; I++)
        {
            healthBar[I].GetComponent<GamePieces>().imgIndex = 1;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if(HealthPiece != null)
        {
            CreateHealthBar();
        }        
    }

    // Update is called once per frame
    void Update()
    {        
        if((GameField != null) && (HealthPiece != null))
        {
            RefreshHealth();
        }
    }
}
