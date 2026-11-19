using System;
using System.Linq;
using System.Threading.Tasks;

namespace SpPracticeParallelPart1
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Выберите задание (1-5) или 0 для выхода:");
                Console.WriteLine("1 - Отображение времени (Task)");
                Console.WriteLine("2 - Простые числа 0-1000 (Task)");
                Console.WriteLine("3 - Простые числа с границами (Task)");
                Console.WriteLine("4 - Поиск Min/Max/Avg/Sum (Task[])");
                Console.WriteLine("5 - Работа с массивом (Continuation)");

                string choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1": Task1(); break;
                    case "2": Task2(); break;
                    case "3": Task3(); break;
                    case "4": Task4(); break;
                    case "5": Task5(); break;
                    case "0": return;
                    default: Console.WriteLine("Неверный выбор."); break;
                }

                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }
        }

        static void Task1()
        {
            Console.WriteLine("Способ 1: Task.Start()");
            Task t1 = new Task(() => Console.WriteLine($"Время: {DateTime.Now}"));
            t1.Start();
            t1.Wait();

            Console.WriteLine("\nСпособ 2: Task.Factory.StartNew()");
            Task t2 = Task.Factory.StartNew(() => Console.WriteLine($"Время: {DateTime.Now}"));
            t2.Wait();

            Console.WriteLine("\nСпособ 3: Task.Run()");
            Task t3 = Task.Run(() => Console.WriteLine($"Время: {DateTime.Now}"));
            t3.Wait();
        }

        static void Task2()
        {
            Console.WriteLine("Простые числа от 0 до 1000:");
            Task t = new Task(() =>
            {
                for (int i = 0; i <= 1000; i++)
                {
                    if (IsPrime(i)) Console.Write($"{i} ");
                }
                Console.WriteLine();
            });
            t.Start();
            t.Wait();
        }

        static void Task3()
        {
            Console.Write("Введите начало диапазона: ");
            int start = int.Parse(Console.ReadLine());
            Console.Write("Введите конец диапазона: ");
            int end = int.Parse(Console.ReadLine());

            int count = 0;
            Task t = new Task(() =>
            {
                if (start > end) { int tmp = start; start = end; end = tmp; }
                for (int i = start; i <= end; i++)
                {
                    if (IsPrime(i))
                    {
                        Console.Write($"{i} ");
                        count++;
                    }
                }
                Console.WriteLine();
            });
            t.Start();
            t.Wait();

            Console.WriteLine($"Количество простых чисел: {count}");
        }

        static void Task4()
        {
            Random rnd = new Random();
            int[] arr = new int[1000];
            for (int i = 0; i < arr.Length; i++) arr[i] = rnd.Next(-1000, 1000);

            int min = 0, max = 0, sum = 0;
            double avg = 0;

            Task[] tasks = new Task[4];

            tasks[0] = Task.Run(() => min = arr.Min());
            tasks[1] = Task.Run(() => max = arr.Max());
            tasks[2] = Task.Run(() => sum = arr.Sum());
            tasks[3] = Task.Run(() => avg = arr.Average());

            Task.WaitAll(tasks);

            Console.WriteLine($"Минимум: {min}");
            Console.WriteLine($"Максимум: {max}");
            Console.WriteLine($"Сумма: {sum}");
            Console.WriteLine($"Среднее: {avg:F4}");
        }

        static void Task5()
        {
            int[] array = { 5, 3, 8, 1, 9, 5, 2, 8, 1, 7, 4, 3 };
            Console.WriteLine($"Исходный массив: {string.Join(", ", array)}");

            int target = 7;

            Task<int[]> removeDuplicates = Task.Run(() =>
            {
                int[] unique = array.Distinct().ToArray();
                Console.WriteLine($"После удаления дублей: {string.Join(", ", unique)}");
                return unique;
            });

            Task<int[]> sortTask = removeDuplicates.ContinueWith(t =>
            {
                int[] sorted = t.Result;
                Array.Sort(sorted);
                Console.WriteLine($"После сортировки: {string.Join(", ", sorted)}");
                return sorted;
            });

            Task searchTask = sortTask.ContinueWith(t =>
            {
                int[] sorted = t.Result;
                int index = Array.BinarySearch(sorted, target);
                if (index >= 0)
                    Console.WriteLine($"Число {target} найдено на индексе {index}");
                else
                    Console.WriteLine($"Число {target} не найдено");
            });

            searchTask.Wait();
        }

        static bool IsPrime(int n)
        {
            if (n < 2) return false;
            for (int i = 2; i <= Math.Sqrt(n); i++)
                if (n % i == 0) return false;
            return true;
        }
    }
}