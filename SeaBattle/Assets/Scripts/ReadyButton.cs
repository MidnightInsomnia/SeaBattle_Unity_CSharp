using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReadyButton : MonoBehaviour
{
    //Объект с полем редактора
    public GameObject PlayerField;
    //Свойство для сокращения доступа к компоненту
    public GameField PlayerFieldControl { get; set; }
    
    public Button btn;
    // Start is called before the first frame update
    void Start()
    {
        //Инициализация псевдонима для команды
        PlayerFieldControl = PlayerField.GetComponent<GameField>();
    }

    // Update is called once per frame
    void Update()
    {
        //Если на поле выставлены все корабли из ангара кнопка START становится активной
        if (PlayerFieldControl.ShipsAlive() == 20)
        {
            btn.interactable = true;
        }
        else
        {            
            btn.interactable = false;
        }            
    }
}
