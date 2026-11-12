using System;
using System.Diagnostics;
using System.Threading;

namespace ThreadingTasks
{
    class Program
    {
        private static object locker = new object();
        private static int counter = 0;

        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Многопоточность ");
                Console.WriteLine("1. Базовое управление потоками (Join)");
                Console.WriteLine("2. Фоновые потоки и Таймер");
                Console.WriteLine("3. Борьба с гонкой данных (lock)");
                Console.WriteLine("4. Игра 'Успел, не успел'");
                Console.WriteLine("0. Выход");
                Console.Write("Выберите задачу: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": Task1(); break;
                    case "2": Task2(); break;
                    case "3": Task3(); break;
                    case "4": Task4(); break;
                    case "0": return;
                    default: Console.WriteLine("Неверный ввод."); break;
                }

                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }
        }

        static void Task1()
        {
            Console.WriteLine("\nЗадача 1");
            Thread threadA = new Thread(new ParameterizedThreadStart(ThreadWorker));
            Thread threadB = new Thread(new ParameterizedThreadStart(ThreadWorker));

            threadA.Start("Поток А");
            threadB.Start("Поток Б");

            threadA.Join();
            threadB.Join();

            Console.WriteLine("Все потоки завершили работу!");
        }

        static void ThreadWorker(object name)
        {
            string threadName = (string)name;
            for (int i = 1; i <= 5; i++)
            {
                Console.WriteLine($"[{threadName}] Шаг: {i}");
                Thread.Sleep(500);
            }
        }

        static void Task2()
        {
            Console.WriteLine("\nЗадача 2");

            TimerCallback timerCallback = new TimerCallback(ShowTime);
            Timer timer = new Timer(timerCallback, null, 1000, 2000);

            Thread bgThread = new Thread(() =>
            {
                while (true)
                {
                    Console.WriteLine("Фоновый поток работает...");
                    Thread.Sleep(300);
                }
            });
            bgThread.IsBackground = true;
            bgThread.Start();

            Console.WriteLine("Нажмите Enter для выхода...");
            Console.ReadLine();
        }

        static void ShowTime(object state)
        {
            Console.WriteLine($"Текущее время: {DateTime.Now:HH:mm:ss}");
        }

        static void Task3()
        {
            Console.WriteLine("\nЗадача 3");
            counter = 0;

            Thread[] threads = new Thread[5];
            for (int i = 0; i < 5; i++)
            {
                threads[i] = new Thread(IncrementCounter);
                threads[i].Start();
            }

            foreach (Thread t in threads)
            {
                t.Join();
            }

            Console.WriteLine($"Итоговое значение counter: {counter}");
        }

        static void IncrementCounter()
        {
            for (int i = 0; i < 100000; i++)
            {
                lock (locker)
                {
                    counter++;
                }
            }
        }

        static void Task4()
        {
            Console.WriteLine("\nЗадача 4");
            Random rnd = new Random();
            int delay = rnd.Next(2000, 5000);
            Console.WriteLine("Приготовьтесь...");
            Thread.Sleep(delay);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Жми!");
            Console.ResetColor();

            Stopwatch sw = Stopwatch.StartNew();

            Thread timeoutThread = new Thread(() =>
            {
                Thread.Sleep(3000);
                Console.WriteLine("\nВы не успели! Слишком долго.");
                Environment.Exit(0);
            });
            timeoutThread.IsBackground = true;
            timeoutThread.Start();

            Console.ReadKey(true);
            sw.Stop();

            Console.WriteLine($"Ваше время реакции: {sw.ElapsedMilliseconds} миллисекунд");
        }
    }
}