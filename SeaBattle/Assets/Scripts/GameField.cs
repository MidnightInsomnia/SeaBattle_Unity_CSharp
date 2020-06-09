using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public class GameField : MonoBehaviour
{
    //***********************************************
    public GameObject MainGame; //Главный скрипт игры
    //***********************************************

    public GameObject eLetters, eNums, eField, EState;    

    //Карта в которую будем копировать данные о расстановке кораблей игрока
    public GameObject MapDestination;

    public bool HideShip = false;

    //Статическая переменная для переключения между режимом редактора(расстановки кораблей) и режимом боя
    public static bool GameReady = false;

    //Положение корабля на поле (Вертикальное, горизонтальное)
    public static bool Direction;

    //Вид выбранного корабля
    int SelectedShip = 4;

    //Список букв
    GameObject[] Letters;

    //Список цифр
    GameObject[] Nums;
    
    //Поле игры
    GameObject[,] Field;

    public struct TestCoord
    {
        public int X, Y;
    }

    public struct Ship
    {
        public TestCoord[] ShipCoord;
    }

    public List<Ship> ListShip = new List<Ship>();

    //Переменная для отключения статуса попадания по истечению указанного времени
    int Time = 800, DeltaTime = 0;

    //Размер поля
    int fieldLength = 10;

    //Количество кораблей на поле
    public int[] ShipsCount = {0, 4, 3, 2, 1 };

    //Создаём функцию копирования поля игры
    public void CopyField()
    {
        if(MapDestination != null)
        {
            //Перебираем всё поле
            //цикл отрисовки поля по Y
            for(int Y = 0; Y < fieldLength; Y++)
            {
                //цикл отрисовки поля по X
                for(int X = 0; X < fieldLength; X++)
                {
                    //читаем что записано в поле (картинки из массива imgs), которое собираемся копировать и записываем в другое поле
                    MapDestination.GetComponent<GameField>().Field[X, Y].GetComponent<GamePieces>().imgIndex = Field[X, Y].GetComponent<GamePieces>().imgIndex;
                }
            }

            //Обнуляем список кораблей
            MapDestination.GetComponent<GameField>().ListShip.Clear();

            //Записываем сгенерированные корабли
            MapDestination.GetComponent<GameField>().ListShip.AddRange(ListShip);
        }
    }


    //Функция возвращает правду, если есть корабли в ангаре
    bool CountShips()
    {
        //Переменная для подсчёта кораблей
        int Amount = 0;
        
        //Суммирование значений
        foreach(int Ship in ShipsCount)
        {
            Amount += Ship;
        }

        //Если сумма кораблей не равна 0, значит ещё есть что ставить на поле
        if (Amount != 0)
        {
            return true;
        }

        //Если сумма равна 0, значит корабли закончились
        return false;
    }

    //Метод удаления последнего поставленного корабля
    public void ClearLastShip()
    {                        
        //шаблон количества кораблей
        int[] ShipsMeta = { 0, 4, 3, 2, 1 };
        
        //Если корабли данного вида в полном составе, 
        //то вид меняется на предыдущий и в ангар возвращается уже корабль предыдущего вида
        if (ShipsCount[SelectedShip] == ShipsMeta[SelectedShip])
        {
            SelectedShip++;
            ShipsCount[SelectedShip]++;
        }            
        else
        {
            ShipsCount[SelectedShip]++;
        }

        //Из списка кораблей берётся последний добавленный
        Ship DeckToDelete = new Ship();
        DeckToDelete = ListShip.Last();
        //Цикл проходится по координатам всех палуб в последнем корабле, меняет спрайты поля
        foreach (var deck in DeckToDelete.ShipCoord)
        {
            Field[deck.X, deck.Y].GetComponent<GamePieces>().imgIndex = 0;
        }
        //Удаляем последний корабль
        ListShip.Remove(ListShip.Last());
                       
    }

    //Метод отчистки поля игры
    public void ClearField()
    {
        //Возвращаем корабли в ангар
        ShipsCount = new int[] { 0, 4, 3, 2, 1 }; //записываем количество кораблей

        SelectedShip = 4;
        //Очистка списка кораблей
        ListShip.Clear();

        //Цикл отрисовки поля по оси Y
        for(int Y = 0; Y < fieldLength; Y++)
        {
            //Цикл отрисовки поля по оси X
            for(int X = 0; X < fieldLength; X++)
            {
                //Сброс всех значений поля на стандартные
                Field[X, Y].GetComponent<GamePieces>().imgIndex = 0;
            }
        }
    }    

    public void EnterOwnShip(int X, int Y)
    {
        int Dir;
        //Переменная направления 
        if (Direction) Dir = 0;
        else Dir = 1;
        if(MainGame.GetComponent<MainGame>().GameMode == 2)
        {
            if (EnterDeck(SelectedShip, Dir, X, Y))
            {
                //Если получилось установить корабль, убираем его из ангара
                ShipsCount[SelectedShip]--;

                //Если кораблей такого типа не осталось - переходим к следующему виду
                if (ShipsCount[SelectedShip] == 0)
                {
                    //смена типа корабля (количества палуб)
                    SelectedShip--;
                    //Field[X, Y].GetComponent<GamePieces>().imgIndex = 0;
                }
            }
        }                
    }
    //Метод случайной расстановки кораблей. 
    //Алгоритм начинает расстановку с самого большого корабля
    //и заканчивает самым маленьким
    public void EnterRandomShip()
    {
        ClearField();
        //Номер выбранного корабля
        SelectedShip = 4;

        //Координаты по которым будет ставится корабль
        int X, Y;
        //Положение корабля на поле (Вертикальное, горизонтальное)
        int Direction;

        while (CountShips())
        {
            //Получаем 2 координаты по которым будем ставить корабль
            //Метод не включает последнее число [0;10) по этому 10, а не 9
            X = UnityEngine.Random.Range(0, 10); //позиция по X
            Y = UnityEngine.Random.Range(0, 10); //позиция по Y
            //Получаем направления 
            Direction = UnityEngine.Random.Range(0, 2);

            if(EnterDeck(SelectedShip, Direction, X, Y))
            {
                //Если получилось установить корабль, убираем его из ангара
                ShipsCount[SelectedShip]--;

                //Если кораблей такого типа не осталось - переходим к следующему виду
                if(ShipsCount[SelectedShip] == 0)
                {
                    //смена типа корабля (количества палуб)
                    SelectedShip--;
                    //Field[X, Y].GetComponent<GamePieces>().imgIndex = 0;
                }
            }
        }

    }    

    //Метод отрисовки игрового поля
    void CreateField()
    {
        //Запись координат игрового объекта GameMap, который является точкой отсчёта
        Vector3 StartPoze = transform.position;

        float XX = StartPoze.x + 1;
        float YY = StartPoze.y - 1;

        //Массив с буквами
        Letters = new GameObject[fieldLength];
        //Массив с цифрами
        Nums = new GameObject[fieldLength];

        //Отрисовка координат на игровом поле
        for(int Label = 0; Label < fieldLength; Label++)
        {
            //Установка букв
            Letters[Label] = Instantiate(eLetters);
            Letters[Label].transform.position = new Vector3(XX, StartPoze.y, StartPoze.z);
            Letters[Label].GetComponent<GamePieces>().imgIndex = Label;
            XX++;
            //Установка цифр
            Nums[Label] = Instantiate(eNums);
            Nums[Label].transform.position = new Vector3(StartPoze.x,YY, StartPoze.z);
            Nums[Label].GetComponent<GamePieces>().imgIndex = Label;
            YY--;
        }

        XX = StartPoze.x + 1;
        YY = StartPoze.y - 1;

        Field = new GameObject[fieldLength, fieldLength];


        //цикл отрисовки игрового поля по Y
        for (int Y = 0; Y < fieldLength; Y++)
        {
            //Отрисовка поля по X
            for(int X = 0; X< fieldLength; X++)
            {
                Field[X, Y] = Instantiate(eField);
                Field[X, Y].GetComponent<GamePieces>().imgIndex = 0;
                Field[X, Y].GetComponent<GamePieces>().HidePiece = HideShip;

                Field[X, Y].transform.position = new Vector3(XX, YY, StartPoze.z);

                Field[X, Y].transform.position = new Vector3(XX, YY, StartPoze.z);
                //Если поле принадлежит игроку, то не размещаем корабли автоматом
                if(GameReady == false)
                {
                    Field[X, Y].GetComponent<FieldClick>().FieldOwner = this.gameObject;
                }
                else
                {
                    if (HideShip)
                    {
                    Field[X, Y].GetComponent<FieldClick>().FieldOwner = this.gameObject;
                    }
                }


                Field[X, Y].GetComponent<FieldClick>().CoordX = X;
                Field[X, Y].GetComponent<FieldClick>().CoordY = Y;

                XX++;
            }
            XX = StartPoze.x + 1;
            YY--;
        }

    }

    bool TestDeckEnter(int X, int Y)
    {
        //Проверка на верную расстановку кораблей
        if((X > -1) && (Y > -1) && (X < 10) && (Y < 10))
        {
            //Массив координат палубы для проверки того, как далеко игрок ставит новый корабль от предыдущего
            //|?|?|?|
            //|?|X|?|
            //|?|?|?|
            int[] XX = new int[9], YY = new int[9];

            //Координаты вокруг выбранного поля
            //-------------------------------------------------------------------------------------------------
            /*|*/XX[0] = X + 1;     /*|*/ XX[1] = X;        /*|*/ XX[2] = X - 1; /*|*/
            /*|*/YY[0] = Y + 1;     /*|*/ YY[1] = Y + 1;    /*|*/ YY[2] = Y + 1; /*|*/
            //-------------------------------------------------------------------------------------------------
            /*|*/XX[3] = X + 1;     /*|*/ XX[4] = X;        /*|*/ XX[5] = X - 1; /*|*/
            /*|*/YY[3] = Y;         /*|*/ YY[4] = Y;        /*|*/ YY[5] = Y;     /*|*/
            //-------------------------------------------------------------------------------------------------
            /*|*/XX[6] = X + 1;     /*|*/ XX[7] = X;        /*|*/ XX[8] = X - 1; /*|*/
            /*|*/YY[6] = Y - 1;     /*|*/ YY[7] = Y - 1;    /*|*/ YY[8] = Y - 1; /*|*/
            //-------------------------------------------------------------------------------------------------


            //Проверка на выход за границы поля
            for(int I = 0; I < 9; I++)
            {
                if((XX[I] > -1) && (YY[I] > -1) && (XX[I] < 10) && (YY[I] < 10))
                {
                    //Проверка на доступность поля
                    if (Field[XX[I], YY[I]].GetComponent<GamePieces>().imgIndex != 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        return false;
    }

    //Проверка на возможность установки палуб в определённом направлении
    //ShipType - тип корабля и количество палуб
    //XD YD    - задание смещения по осям проверки
    //X Y      - координаты начальной точки установки корабля

    TestCoord[] TestEnterShipDirect(int ShipType, int XD, int YD, int X, int Y)
    {
        //Массив для вывода результата (корректных координат)
        TestCoord[] ResultCoord = new TestCoord[ShipType];

        //Проверка в указанную сторону на длинну корабля
        for( int P = 0; P < ShipType; P++)
        {
            //Проверка на возможность установки палубы
            if(TestDeckEnter(X, Y))
            {
                //Записываем результат
                ResultCoord[P].X = X;
                ResultCoord[P].Y = Y;
            }//Если проверка упирается в стену
            else
            {
                //Остановка проверки и возврат null значения
                return null;
            }

            //Смещение проверки в указанном направлении
            X += XD;
            Y += YD;
        }
        //В случае окончания работы проверочных циклов возвращается результат
        return ResultCoord;
    }

    //Передаём в метод какой корабль мы хотим поставить в указанном месте. Возвращает список координат либо null если места нет (зависит от кол-ва палуб
    //ShipType - тип корабля и количество палуб
    //Direction - направление: 0 - вертикаль, 1 - горизонталь
    //X Y - координаты начальной точки установки корабля

    TestCoord[] TestEnterShip(int ShipType, int Direction, int X, int Y)
    {
        //Создание массива координат с результатами проверки
        TestCoord[] ResultCoord = new TestCoord[ShipType];
        //Проверка, можно ли поставить корабль в указанной точке
        if (TestDeckEnter(X, Y))
        {
            //Выбор направления
            switch (Direction)
            {
                case 0:
                    //Попытка установки палубы в положительном направлении по оси X
                    ResultCoord = TestEnterShipDirect(ShipType, 1, 0, X, Y);

                    //Попытка установить корабль в обратном направлении
                    if (ResultCoord == null) 
                    {
                       ResultCoord = TestEnterShipDirect(ShipType, -1, 0, X, Y);
                    }
                    break;
                case 1:
                    //Попытка установки палубы в положительном направлении по оси Y
                    ResultCoord = TestEnterShipDirect(ShipType, 0, 1, X, Y);
                    //Попытка установить корабль в обратном направлении
                    if (ResultCoord == null) 
                    {
                        ResultCoord = TestEnterShipDirect(ShipType, 0, -1, X, Y);
                    }                    

                    break;
                
            }

            //Возврат результата
            return ResultCoord;
        }        
        //В случае неудачи возвращаем null
        return null;
    }

    //Функция установки корабля в указанную ячейку поля
    bool EnterDeck(int ShipType, int Direction, int X, int Y)
    {
        //Получаем координаты для установки корабля
        TestCoord[] P = TestEnterShip(ShipType, Direction, X, Y);

        //Если установка корабля возможна, то ставим его
        if (P != null)
        {
            //Если в списке будут координаты то поставить корабль в указанное место
            foreach(TestCoord Test in P)
            {
                Field[Test.X, Test.Y].GetComponent<GamePieces>().imgIndex = 1;
            }

            Ship Deck;

            //Сохраняем координаты корабля
            Deck.ShipCoord = P;

            //Сохраняем корабль в список
            ListShip.Add(Deck);

            //Если удалось поставить корабль возвращаем true
            return true;
        }
        return false;
    }



    // Start is called before the first frame update
    void Start()
    {
        //Создание поля при запуске
        CreateField();        
        if (HideShip)
        {
            EnterRandomShip();
        }        
    }

    // Update is called once per frame
    void Update()
    {
        DeltaTime++;

        if (DeltaTime > Time)
        {
            if (EState != null) EState.GetComponent<GamePieces>().imgIndex = 0;
            DeltaTime = 0;
        }
    }

    //Ячейка на которую нажали передаёт ссылку на объект в FieldOwner
    public void WhoClick(int X, int Y)
    {
        //Если идёт стадия редактирования то при нажатии на поле
        //Метод ставит корабль
        if(GameReady == false)
        {
            if (CountShips()) { EnterOwnShip(X, Y); }            
        }
        //Если идёт стадия боя, при нажатии на поле производится выстрел
        else
        {
            //Ограничение на клик по собственному полю
            if (MainGame != null && HideShip == true)
            {
                MainGame.GetComponent<MainGame>().UserClick(X, Y);
            }
        }
    }

    //функция получения параметра блока,
    //чтобы ИИ мог искать оставшуюся часть корабля в случае попадания
    public int GetIndexBlock(int X, int Y)
    {
        return Field[X, Y].GetComponent<GamePieces>().imgIndex;
    }

    //Метод индикации статуса выстрела 
    public bool Shoot(int X, int Y)
    {
        //обнуление статуса выстрела
        if (EState != null)
        {
            EState.GetComponent<GamePieces>().imgIndex = 0;
        }

        int SelectedField = Field[X, Y].GetComponent<GamePieces>().imgIndex;
        bool Result = false;
        if(SelectedField != 2 && SelectedField != 3)
        {
            switch (SelectedField)
            {
                //Промах
                case 0:
                    Field[X, Y].GetComponent<GamePieces>().imgIndex = 2;
                    Result = false;

                    //Индикатор промаха
                    if (EState != null) EState.GetComponent<GamePieces>().imgIndex = 3;
                    break;

                //Попадание
                case 1:
                    Field[X, Y].GetComponent<GamePieces>().imgIndex = 3;
                    Result = true;

                    if (ShootCheck(X, Y))
                    {
                        //Индикация уничтожения корабля
                        if (EState != null) EState.GetComponent<GamePieces>().imgIndex = 1;
                    }
                    else
                    {
                        //Индикация ранения
                        if (EState != null) EState.GetComponent<GamePieces>().imgIndex = 2;
                    }

                    break;

            }
            return Result;
        }

        return true;                
    }

    //Функция проверки попадания по кораблю
    bool ShootCheck(int X, int Y)
    {
        bool Result = false;

        //Перебираем корабли и смотрим в какой из них мы попали
        foreach(Ship Test in ListShip)
        {
            //перебираем палубы корабля и проверяем попали ли мы в неё
            foreach( TestCoord deck in Test.ShipCoord)
            {
                //Сравниваем координаты выстрела с координатой палубы
                if((deck.X == X) && (deck.Y == Y))
                {
                    //Объявляем переменную подсчёта попаданий
                    int HitCounter = 0;

                    foreach(TestCoord HitDeck in Test.ShipCoord)
                    {
                        //Проверяем что записано в поле по данным координатам
                        int TestBlock = Field[HitDeck.X, HitDeck.Y].GetComponent<GamePieces>().imgIndex;

                        //Если записана цифра 3(картинка с попаданием в префабе), то подсчитываем число ранений
                        if (TestBlock == 3) HitCounter++;
                    }
                    //Если количество палуб равно количеству попаданий, значит корабль уничтожен
                    if(HitCounter == Test.ShipCoord.Length)
                    {
                        //Если уничтожен возвращаем true
                        Result = true;
                    }
                    else
                    {
                        //Если корабль ещё цел, возвращаем false
                        Result = false;
                    }
                    //Завершаем цикл и возвращаем результат
                    return Result;
                }
            }
        }

        return Result;
    }

    //Метод для подсчёта живых кораблей
    public int ShipsAlive()
    {
        //Количество целых палуб
        int AliveDeckCounter = 0;

        //Проверка списка кораблей
        foreach(Ship Test in ListShip)
        {
            //перебираем палубы корабля и проверяем жива ли она
            foreach(TestCoord Deck in Test.ShipCoord)
            {
                int TestBlock = Field[Deck.X, Deck.Y].GetComponent<GamePieces>().imgIndex;

                //Если в поле imgIndex записан индекс 1, значит палуба целая
                if (TestBlock == 1) AliveDeckCounter++;
            }
        }

        //Возврат кол-ва целых палуб
        return AliveDeckCounter;
    }
}
