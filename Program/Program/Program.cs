using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadingHomework
{
    class Program
    {
        static int ticketsLeft = 100;
        static int fuelLevel = 0;

        static CancellationTokenSource cts = new CancellationTokenSource();

        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();

                cts = new CancellationTokenSource();

                Console.WriteLine("1. Диспетчер задач (Thread vs ThreadPool)");
                Console.WriteLine("2. Асинхронный калькулятор (APM)");
                Console.WriteLine("3. Касса кинотеатра (Синхронизация)");
                Console.WriteLine("4. Космический запуск (Комплексное)");
                Console.WriteLine("0. Выход");
                Console.Write("Выберите задание: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": Task1(); break;
                    case "2": Task2(); break;
                    case "3": Task3(); break;
                    case "4": Task4(); break;
                    case "0": return;
                    default: Console.WriteLine("Неверный ввод."); break;
                }


                cts.Cancel();

                Console.WriteLine("\nНажмите любую клавишу для продолжения");
                Console.ReadKey();
            }
        }

        static void Task1()
        {
            Console.WriteLine("Задание 1");
            var token = cts.Token;

            Thread monitorThread = new Thread(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    Console.WriteLine($"[Время] {DateTime.Now:HH:mm:ss}");

                    if (token.WaitHandle.WaitOne(1000)) break;
                }
            });
            monitorThread.IsBackground = true;
            monitorThread.Start();

            for (int i = 1; i <= 3; i++)
            {
                ThreadPool.QueueUserWorkItem(PoolTask, i);
            }

   
            Thread.Sleep(5000);
            Console.WriteLine("\n5 секунд истекли. Останавливаем мониторинг...");
        }

        static void PoolTask(object? state)
        {
            if (state is int taskId)
            {
                Console.WriteLine($"Задача №{taskId} выполняется в потоке {Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep(2000);
            }
        }

        static void Task2()
        {
            Console.WriteLine("Задание 2");

            Task.Run(() =>
            {
                Thread.Sleep(3000);
                double result = Math.Sqrt(25);
                Console.WriteLine($"\n[Callback] Результат вычисления: {result}");
                Console.WriteLine($"[Callback] Выполнялось в потоке: {Thread.CurrentThread.ManagedThreadId}");
            });

            Console.WriteLine("Расчет запущен. Главный поток свободен.");
            Console.ReadLine();
        }

        static void Task3()
        {
            Console.WriteLine("Задание 3");
            ticketsLeft = 100;

            Thread[] cashiers = new Thread[5];
            for (int i = 0; i < 5; i++)
            {
                cashiers[i] = new Thread(SellTickets);
                cashiers[i].Start(i + 1);
            }

            foreach (Thread t in cashiers)
            {
                t.Join();
            }
        }

        static void SellTickets(object? cashierId)
        {
            if (cashierId is int id)
            {
                while (ticketsLeft > 0)
                {
                    int currentTickets = Interlocked.Decrement(ref ticketsLeft);

                    if (currentTickets >= 0)
                    {
                        Console.WriteLine($"Кассир {id} продал билет. Осталось: {currentTickets}");
                    }
                    else
                    {
                        Interlocked.Increment(ref ticketsLeft);
                    }

                    Thread.Sleep(10);
                }
            }
        }

        static void Task4()
        {
            Console.WriteLine("Задание 4");
            fuelLevel = 0;
            var token = cts.Token;


            Thread telemetryThread = new Thread(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    Console.WriteLine("Телеметрия в норме...");
                    if (token.WaitHandle.WaitOne(1000)) break;
                }
            });
            telemetryThread.IsBackground = true;
            telemetryThread.Start();


            Task.Run(() =>
            {
                for (int i = 0; i < 4; i++)
                {
                    if (token.IsCancellationRequested) return;
                    Thread.Sleep(1000);
                }
                Console.WriteLine("\nТраектория рассчитана! К запуску готов");
            }, token);

            ThreadPool.QueueUserWorkItem(PumpFuel);


            Thread.Sleep(5000);

            int finalFuel = Interlocked.CompareExchange(ref fuelLevel, 0, 0);
            Console.WriteLine($"\nИтоговый уровень топлива: {finalFuel}");
            Console.WriteLine("Завершение симуляции запуска");
        }

        static void PumpFuel(object? state)
        {
            for (int i = 0; i < 10; i++)
            {
                Interlocked.Add(ref fuelLevel, 10);
                Thread.Sleep(500);
            }
        }
    }
}