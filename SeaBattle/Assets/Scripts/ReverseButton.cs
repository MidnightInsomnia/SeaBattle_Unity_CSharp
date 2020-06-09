using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReverseButton : MonoBehaviour
{
    //Изображение в виде направления добавляется через редактор
    public GameObject Arrow;        

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {     
        //Если выбрано вертикальное направление изображение стрелки разворачивается вверх
        if (GameField.Direction == true)
        {
            Arrow.transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        //И в исходное положение, если положение горизонтальное
        else
        {
            Arrow.transform.rotation = Quaternion.Euler(0, 0, 0);
        }                
    }    
}
