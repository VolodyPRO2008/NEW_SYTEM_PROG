using System;
using System.Collections.Generic;
using System.Threading;

namespace SpPracticeModule2Console
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Выберите задание (1-6) или 0 для выхода:");
                Console.WriteLine("1 - Единственный экземпляр (Mutex)");
                Console.WriteLine("2 - Защита счетчика (Mutex)");
                Console.WriteLine("3 - Ограничение потоков (Semaphore)");
                Console.WriteLine("4 - Производитель-потребитель (Semaphore + lock)");
                Console.WriteLine("5 - Стартовый сигнал (ManualResetEvent)");
                Console.WriteLine("6 - Поочередный вывод (AutoResetEvent)");

                string choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1": Task1(); break;
                    case "2": Task2(); break;
                    case "3": Task3(); break;
                    case "4": Task4(); break;
                    case "5": Task5(); break;
                    case "6": Task6(); break;
                    case "0": return;
                    default: Console.WriteLine("Неверный выбор."); break;
                }

                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }
        }

        static void Task1()
        {
            try
            {
                Mutex.OpenExisting("MY_MUTEX");
                Console.WriteLine("Приложение уже запущено!");
                return;
            }
            catch (WaitHandleCannotBeOpenedException)
            {
            }

            using (Mutex mutex = new Mutex(true, "MY_MUTEX"))
            {
                Console.WriteLine("Приложение работает");
                Console.WriteLine("Нажмите любую клавишу для выхода...");
                Console.ReadKey();
                mutex.ReleaseMutex();
            }
        }

        static void Task2()
        {
            MutexCounter counter = new MutexCounter();
            Thread[] threads = new Thread[5];

            for (int i = 0; i < 5; i++)
            {
                threads[i] = new Thread(() =>
                {
                    for (int j = 0; j < 1000000; j++)
                    {
                        counter.Increment();
                    }
                });
                threads[i].Start();
            }

            foreach (var t in threads) t.Join();
            Console.WriteLine($"Итоговое значение: {counter.Count}");
        }

        static void Task3()
        {
            Semaphore sem = new Semaphore(3, 3);
            int totalTasks = 6;
            int completedTasks = 0;
            object lockObj = new object();

            for (int i = 0; i < totalTasks; i++)
            {
                int taskId = i;
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    bool acquired = false;
                    while (!acquired)
                    {
                        acquired = sem.WaitOne(500);
                        if (!acquired)
                        {
                            Console.WriteLine($"[Задача {taskId}] Таймаут ожидания, повтор...");
                        }
                    }

                    try
                    {
                        Console.WriteLine($"[Задача {taskId}] Поток {Thread.CurrentThread.ManagedThreadId} начал работу.");
                        Thread.Sleep(2000);
                        Console.WriteLine($"[Задача {taskId}] Поток {Thread.CurrentThread.ManagedThreadId} завершил работу.");
                    }
                    finally
                    {
                        sem.Release();
                    }

                    lock (lockObj)
                    {
                        completedTasks++;
                    }
                });
            }

            while (true)
            {
                lock (lockObj)
                {
                    if (completedTasks == totalTasks) break;
                }
                Thread.Sleep(100);
            }
        }

        static void Task4()
        {
            Queue<int> queue = new Queue<int>();
            Semaphore empty = new Semaphore(5, 5);
            Semaphore full = new Semaphore(0, 5);
            object lockObj = new object();
            int totalItems = 10;
            int consumedItems = 0;

            Thread producer = new Thread(() =>
            {
                for (int i = 1; i <= totalItems; i++)
                {
                    empty.WaitOne();
                    lock (lockObj)
                    {
                        queue.Enqueue(i);
                        Console.WriteLine($"Производитель добавил: {i} (в очереди: {queue.Count})");
                    }
                    full.Release();
                    Thread.Sleep(200);
                }
            });

            Action consumerAction = () =>
            {
                while (true)
                {
                    lock (lockObj)
                    {
                        if (consumedItems >= totalItems) break;
                    }

                    full.WaitOne();
                    lock (lockObj)
                    {
                        if (queue.Count > 0)
                        {
                            int item = queue.Dequeue();
                            Console.WriteLine($"Потребитель {Thread.CurrentThread.ManagedThreadId} забрал: {item} (в очереди: {queue.Count})");
                            consumedItems++;
                        }
                    }
                    empty.Release();
                    Thread.Sleep(300);
                }
            };

            Thread c1 = new Thread(() => consumerAction());
            Thread c2 = new Thread(() => consumerAction());

            producer.Start(); c1.Start(); c2.Start();
            producer.Join(); c1.Join(); c2.Join();
            Console.WriteLine("Обработка завершена.");
        }

        static void Task5()
        {
            ManualResetEvent startEvent = new ManualResetEvent(false);
            Thread[] threads = new Thread[10];

            for (int i = 0; i < 10; i++)
            {
                int id = i;
                threads[i] = new Thread(() =>
                {
                    startEvent.WaitOne();
                    Console.WriteLine($"Поток {id} стартовал");
                });
                threads[i].Start();
            }

            Console.WriteLine("Ожидание 2 секунды...");
            Thread.Sleep(2000);
            Console.WriteLine("Отправка сигнала!");
            startEvent.Set();

            foreach (var t in threads) t.Join();
        }

        static void Task6()
        {
            AutoResetEvent event1 = new AutoResetEvent(true);
            AutoResetEvent event2 = new AutoResetEvent(false);

            Thread t1 = new Thread(() =>
            {
                for (int i = 1; i <= 9; i += 2)
                {
                    event1.WaitOne();
                    Console.WriteLine(i);
                    event2.Set();
                }
            });

            Thread t2 = new Thread(() =>
            {
                for (int i = 2; i <= 10; i += 2)
                {
                    event2.WaitOne();
                    Console.WriteLine(i);
                    event1.Set();
                }
            });

            t1.Start(); t2.Start();
            t1.Join(); t2.Join();
        }
    }

    class MutexCounter
    {
        private int count = 0;
        private Mutex mutex = new Mutex();

        public int Count => count;

        public void Increment()
        {
            mutex.WaitOne();
            try
            {
                count++;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
    }
}