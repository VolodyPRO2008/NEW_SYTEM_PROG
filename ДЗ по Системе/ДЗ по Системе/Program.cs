using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace WinApiTasks
{
    class Program
    {
 
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool SetWindowText(IntPtr hWnd, string lpString);


        const uint MB_OK = 0x0;
        const uint MB_YESNO = 0x4;
        const uint MB_ICONQUESTION = 0x20;
        const uint MB_ICONINFORMATION = 0x40;
        const uint MB_ICONERROR = 0x10;


        const int IDYES = 6;
        const int IDNO = 7;


        const uint WM_CLOSE = 0x0010;

        static void Main(string[] args)
        {
            Console.WriteLine("Выберите задание (1-4):");
            Console.WriteLine("1. Hello, World! через MessageBox");
            Console.WriteLine("2. Игра 'Угадай число'");
            Console.WriteLine("3. Закрыть Блокнот");
            Console.WriteLine("4. Время в заголовке Блокнота");
            Console.Write("Ваш выбор: ");

            string choice = Console.ReadLine();
            Console.Clear();

            switch (choice)
            {
                case "1":
                    Task1();
                    break;
                case "2":
                    Task2();
                    break;
                case "3":
                    Task3();
                    break;
                case "4":
                    Task4();
                    break;
                default:
                    Console.WriteLine("Неверный ввод.");
                    break;
            }

            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        static void Task1()
        {
            Console.WriteLine("Запуск Задания 1...");
            MessageBox(IntPtr.Zero, "Hello, World!", "Задание 1", MB_OK | MB_ICONINFORMATION);
        }

        static void Task2()
        {
            Console.WriteLine("Запуск Задания 2...");
            Random rnd = new Random();
            bool playAgain = true;

            while (playAgain)
            {
                int min = 0, max = 100;
                bool guessed = false;
                MessageBox(IntPtr.Zero, "Загадайте число от 0 до 100. Я попробую угадать!", "Игра", MB_OK);

                while (!guessed)
                {
                    int guess = rnd.Next(min, max + 1);
                    int result = MessageBox(IntPtr.Zero, $"Ваше число больше {guess}?", "Угадываю...", MB_YESNO | MB_ICONQUESTION);

                    if (result == IDYES)
                    {
                        min = guess + 1;
                    }
                    else
                    {
                        int check = MessageBox(IntPtr.Zero, $"Вы загадали число {guess}?", "Проверка", MB_YESNO | MB_ICONQUESTION);
                        if (check == IDYES)
                        {
                            MessageBox(IntPtr.Zero, "Ура! Я угадал!", "Победа", MB_OK | MB_ICONINFORMATION);
                            guessed = true;
                        }
                        else
                        {
                            max = guess - 1;
                        }
                    }

                    if (min > max)
                    {
                        MessageBox(IntPtr.Zero, "Вы жульничаете! Такого числа нет.", "Ошибка", MB_OK | MB_ICONERROR);
                        break;
                    }
                }

                int again = MessageBox(IntPtr.Zero, "Хотите сыграть еще раз?", "Повтор", MB_YESNO | MB_ICONQUESTION);
                if (again == IDNO) playAgain = false;
            }
        }

        static void Task3()
        {
            Console.WriteLine("Запуск Задания 3...");
            Console.WriteLine("Ищем Блокнот...");

            IntPtr hNotepad = FindWindow("Notepad", null);

            if (hNotepad != IntPtr.Zero)
            {
                Console.WriteLine("Блокнот найден! Отправляем команду закрытия...");
                SendMessage(hNotepad, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                Console.WriteLine("Команда отправлена.");
            }
            else
            {
                Console.WriteLine("Блокнот не запущен. Откройте его и попробуйте снова.");
            }
        }

        static void Task4()
        {
            Console.WriteLine("Запуск Задания 4...");
            Console.WriteLine("Ожидание Блокнота...");
            IntPtr hNotepad = IntPtr.Zero;

            while (hNotepad == IntPtr.Zero)
            {
                hNotepad = FindWindow("Notepad", null);
                Thread.Sleep(500);
            }

            Console.WriteLine("Блокнот найден! Обновляем время в заголовке. Нажмите любую клавишу в этом окне для остановки.");

            CancellationTokenSource cts = new CancellationTokenSource();

            Task.Run(() =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (FindWindow("Notepad", null) == IntPtr.Zero)
                    {
                        Console.WriteLine("Блокнот закрыт. Остановка.");
                        break;
                    }

                    string timeStr = "Time: " + DateTime.Now.ToString("HH:mm:ss");
                    SetWindowText(hNotepad, timeStr);
                    Thread.Sleep(1000);
                }
            });

            Console.ReadKey();
            cts.Cancel();
            Console.WriteLine("Обновление времени остановлено.");
        }
    }
}