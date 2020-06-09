using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



////Получаем компонент камера
//cam = GetComponent<Camera>();
////задаём дальность обзора камеры
//cam.orthographicSize = 8;
////Задаём координаты для камеры
//this.transform.position = new Vector3(0, 0, -10);                                                    


public class MainGame : MonoBehaviour
{    
    //Список со страницами Canvas
    public List<GameObject> Pages = new List<GameObject>();
    //Имена имеющихся страниц
    enum Page
    {
        MainMenu,
        DifficultyMenu,
        Redactor,
        WinScreen,
        LoseScreen
    }
    
    //Уровни сложности
    enum difficulty
    {
        Easy,
        Middle,
        Hard
    }
    //Режим игры    
    public int GameMode { get; set; } = 0;

    public GameObject PlayerField, ComputerField, Player;    
    public GameObject CurrentPage { get; set; }

    Camera cam;
    
    //уровень сложности 0 - easy, 1 - middle 2 - hard    
    public int Difficulty { get; set; }
    public GameField PlayerFieldControl { get; set; } 

    //Переключатель, определяющий кто ходит в данный момент
    //если true - ход игрока
    //если false - ход PC
    bool whoseMove = true;

    //************************************************************
    //*********ОБРАБОТЧИКИ НАЖАТИЯ КНОПОК ИЗ CANVAS***************
    //************************************************************


    //MAIN MENU

    //Кнопка выхода
    public void ExitPressed()
    {
        Application.Quit();
    }    

    //DIFFICULTY

    //Смена игрового режима при выборе сложности
    public void DifficultButton()
    {        
        GameMode = 2;
    }    

    //REDACTOR

    //Переход обратно на страницу выбора сложности
    public void RedactorBackButton()
    {        
        PlayerFieldControl.ClearField();        
        GameMode = 1;     
    }   
    

    //Кнопка автоматической расстановки кораблей в редакторе
    public void RedactorAutoButton() 
    {        
        PlayerFieldControl.EnterRandomShip();
    }

    //Ручная смена расстановки кораблей    
    public void RedactorReverseButton()
    {
        //Переключение флага с информацией о направлении
        GameField.Direction = !GameField.Direction;
    }                                                                            

    //Очистка поля от кораблей
    public void RedactorClearAllButton()
    {
        PlayerFieldControl.ClearField();
    }       
    
    //Вызов метода удаления последнего поставленного на поле редактора корабля
    public void RedactorClearLastButton()
    {
        //Если на поле есть корабли - попытка очистить поле
        if (PlayerFieldControl.ShipsAlive() != 0)
        {
            PlayerFieldControl.ClearLastShip();
        }
    }


    //Кнопка готовности и перехода к следующему этапу
    public void RedactorStartButton()
    {        
        //Если все корабли из ангара расставлены
        if (PlayerFieldControl.ShipsAlive() == 20)
        {
            //Отключение страницы редактора
            Pages[(int)Page.Redactor].SetActive(false);            
            //смена игрового режима
            GameMode = 3;
            //Получаем компонент камера
            cam = GetComponent<Camera>();
            //задаём дальность обзора камеры
            cam.orthographicSize = 11;
            //Задаём координаты поля BattleScreen для камеры
            this.transform.position = new Vector3(52.88f, -3.02f, -10); 
            //Смена режима расстановки кораблей на режим боя
            GameField.GameReady = true;
            //Копирование кораблей с поля редактора на поле игрока
            PlayerField.GetComponent<GameField>().CopyField();
            //Очистка поля редактора перед выходом для повторной игры
            PlayerFieldControl.ClearField();
            //Заполнение поля компьютера кораблями
            ComputerField.GetComponent<GameField>().EnterRandomShip();            
        }
    }                                                                                             

    //Кнопка на странице проигрыша/выйгрыша
    public void FinalButtonPressed()
    {
        //В случае победы и затем нажатия кнопки "Меню" отключается страница WinScreen
        if (GameMode == 4)
        {
            Pages[(int)Page.WinScreen].SetActive(false);            
        }
        //В случае поражения и нажатия кнопки "Меню" отключается страница LoseScreen
        if(GameMode == 5)
        {
            Pages[(int)Page.LoseScreen].SetActive(false);
        }
        //Возврат к изначальному этапу игры
        GameMode = 0;
        //Включение страницы Main Menu
        Pages[(int)Page.MainMenu].SetActive(true);                       
    }


    //************************************************************
    //******************AI И ЛОГИКА ИГРЫ**************************
    //************************************************************


    GameField.TestCoord AIShoot()
    {
        GameField.TestCoord XY;

        XY.X = -1;
        XY.Y = -1;

        //Перебираем корабли и смотрим в каком из них есть палубы
        foreach(GameField.Ship Test in Player.GetComponent<GameField>().ListShip)
        {
            //Перебираем палубы корабля и проверяем попали ли мы в неё
            foreach(GameField.TestCoord Deck in Test.ShipCoord)
            {
                //Смотрим какой номер у палубы
                int Index = Player.GetComponent<GameField>().GetIndexBlock(Deck.X, Deck.Y);
                if(Index == 1)
                {
                    //вернём координаты целой палубы
                    return Deck;
                }
            }            
        }
        //Если перебрали и не нашли ни одной подходящей координаты вернём - 1, -1
        return XY;
    }

    int ShootCounter = 0;

    //Метод, описывающий ходы компьютера
    void ArtificialIntelligence()
    {
        //Частота гарантированного убийства палубы игрока
        int killDeckPeriod = 0;
        //Количество кораблей, которые нужно уничтожить игроку для активации алгоритма точных попаданий
        int shipsBeforeRage = 0;

        //Настройки для каждой сложности
        if (Difficulty == (int)difficulty.Easy) { killDeckPeriod = 5; shipsBeforeRage = 5; }
        if (Difficulty == (int)difficulty.Middle) { killDeckPeriod = 3; shipsBeforeRage = 7; }
        if (Difficulty == (int)difficulty.Hard) { killDeckPeriod = 1; shipsBeforeRage = 10; }
        //Проверка на возможность хода
        if (!whoseMove)
        {
            //если палуб больше половины, то выстрелы случайны
            int ShootX = Random.Range(0, 10);
            int ShootY = Random.Range(0, 10);

            //Вычисление количества уничтоженных палуб игроком
            int PC_Ship = ComputerField.GetComponent<GameField>().ShipsAlive();

            //Активация точных попаданий по палубам игрока
            if (PC_Ship < shipsBeforeRage)
            {
                if (ShootCounter == killDeckPeriod)
                {
                    //Стреляем по палубе
                    GameField.TestCoord XY = AIShoot();

                    if((XY.X >= 0) && (XY.Y >= 0))
                    {
                        ShootX = XY.X;
                        ShootY = XY.Y;
                    }
                    ShootCounter = 0;                    
                }
                else
                {
                    ShootCounter++;
                }
            }            
            //в случае промаха - передача хода игроку
            whoseMove = !Player.GetComponent<GameField>().Shoot(ShootX, ShootY);
        }
    }

    //Проверка победы
    void WinTest()
    {
        //Проверка количества живых палуб
        int PC_Ship = ComputerField.GetComponent<GameField>().ShipsAlive();
        int Player_Ship = Player.GetComponent<GameField>().ShipsAlive();
        
        //Если игрок уничтожил все вражеские корабли -победа
        if (PC_Ship == 0)
        {
            GameMode = 4;
            BackToUI(true);
        }
        //Если все корабли игрока уничтожены - проигрыш
        if (Player_Ship == 0)
        {
            GameMode = 5;
            BackToUI(false);
        }

    }

    //Метод для перехода обратно в область Canvas и активации нужных страниц
    public void BackToUI(bool win)
    {
        //Возврат к режиму редактирования
        GameField.GameReady = false;     
        //В случае победы активация WinScreen
        if (win)
        {
            Pages[(int)Page.WinScreen].SetActive(true);
        }
        //В случае поражения LoseScreen
        else
        {
            Pages[(int)Page.LoseScreen].SetActive(true);
        }

        cam = GetComponent<Camera>();
        //задаём дальность обзора камеры
        cam.orthographicSize = 8;
        //Задаём координаты для камеры
        this.transform.position = new Vector3(0, 0, -10);
    }
    //Когда игрок щёлкает по блоку мы получаем сообщение о том, куда он нажал
    public void UserClick(int X, int Y)
    {
        //Если не выйграл и ходит игрок, то ходим
        if (whoseMove)
        {
            //ходит игрок 
            //если он попал, то функция вернёт правду и ход останется за игроком
            //если он промахнулся, то ход передаётся PC

            whoseMove = ComputerField.GetComponent<GameField>().Shoot(X, Y);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //Инициализация псевдонима для ображения к компаненту
        PlayerFieldControl = PlayerField.GetComponent<GameField>();        
    }

    // Update is called once per frame
    void Update()
    {        
        //Проверка на победу
        if(GameMode == 3)
        {
            WinTest();
            //Если ход PC, вызываем метод с ИИ
            ArtificialIntelligence();
        }
    }
}
