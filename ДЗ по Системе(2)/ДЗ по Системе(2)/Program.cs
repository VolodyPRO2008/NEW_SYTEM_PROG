using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ProcessManagement
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                RunChildProcess(args);
                return;
            }

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Модуль 2. Процессы");
                Console.WriteLine("1. Запуск дочернего процесса (ожидание и код завершения)");
                Console.WriteLine("2. Запуск процесса (ожидание или принудительное завершение)");
                Console.WriteLine("3. Калькулятор через аргументы командной строки");
                Console.WriteLine("4. Поиск слова в файле через аргументы командной строки");
                Console.WriteLine("0. Выход");
                Console.Write("Выберите задание: ");

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
            Console.WriteLine("\nЗадание 1");
            Console.WriteLine("Запускаем дочерний процесс (notepad.exe)...");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "notepad.exe",
                UseShellExecute = false
            };

            using (Process process = Process.Start(startInfo))
            {
                Console.WriteLine("Ожидаем закрытия Блокнота...");
                process.WaitForExit();
                Console.WriteLine($"Процесс завершен. Код завершения: {process.ExitCode}");
            }
        }

        static void Task2()
        {
            Console.WriteLine("\nЗадание 2");
            Console.WriteLine("Запускаем дочерний процесс (notepad.exe)...");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "notepad.exe",
                UseShellExecute = false
            };

            using (Process process = Process.Start(startInfo))
            {
                Console.WriteLine("Выберите действие:");
                Console.WriteLine("1 - Дождаться завершения");
                Console.WriteLine("2 - Принудительно завершить (Kill)");
                Console.Write("Ваш выбор: ");
                string action = Console.ReadLine();

                if (action == "1")
                {
                    Console.WriteLine("Ожидаем закрытия Блокнота...");
                    process.WaitForExit();
                    Console.WriteLine($"Процесс завершен. Код завершения: {process.ExitCode}");
                }
                else if (action == "2")
                {
                    Console.WriteLine("Принудительное завершение процесса...");
                    process.Kill();
                    process.WaitForExit();
                    Console.WriteLine("Процесс был принудительно завершен.");
                }
            }
        }

        static void Task3()
        {
            Console.WriteLine("\nЗадание 3");
            Console.Write("Введите первое число: ");
            string num1 = Console.ReadLine();
            Console.Write("Введите второе число: ");
            string num2 = Console.ReadLine();
            Console.Write("Введите операцию (+, -, *, /): ");
            string op = Console.ReadLine();

            string currentExePath = Process.GetCurrentProcess().MainModule.FileName;

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = currentExePath,
                Arguments = $"\"{num1}\" \"{num2}\" \"{op}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                Console.WriteLine("Результат работы дочернего процесса:");
                Console.WriteLine(output);
            }
        }

        static void Task4()
        {
            Console.WriteLine("\nЗадание 4");
            Console.Write("Введите путь к папке: ");
            string folderPath = Console.ReadLine();
            Console.Write("Введите слово для поиска: ");
            string searchWord = Console.ReadLine();

            string currentExePath = Process.GetCurrentProcess().MainModule.FileName;

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = currentExePath,
                Arguments = $"\"{folderPath}\" \"{searchWord}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                Console.WriteLine("Результат работы дочернего процесса:");
                Console.WriteLine(output);
            }
        }

        static void RunChildProcess(string[] args)
        {
            if (args.Length == 3)
            {
                double a = double.Parse(args[0]);
                double b = double.Parse(args[1]);
                string op = args[2];
                double result = 0;

                switch (op)
                {
                    case "+": result = a + b; break;
                    case "-": result = a - b; break;
                    case "*": result = a * b; break;
                    case "/":
                        if (b != 0) result = a / b;
                        else Console.WriteLine("Ошибка: деление на ноль");
                        break;
                }

                Console.WriteLine($"Получены аргументы: {a}, {b}, операция '{op}'");
                Console.WriteLine($"Результат: {result}");
            }
            else if (args.Length == 2)
            {
                string folderPath = args[0];
                string word = args[1];
                int totalCount = 0;

                Console.WriteLine($"Поиск слова '{word}' в папке: {folderPath}");

                if (Directory.Exists(folderPath))
                {
                    string[] files = Directory.GetFiles(folderPath, "*.txt", SearchOption.AllDirectories);

                    foreach (string file in files)
                    {
                        string content = File.ReadAllText(file);
                        int count = CountOccurrences(content, word);
                        totalCount += count;
                    }

                    Console.WriteLine($"Общее количество вхождений: {totalCount}");
                }
                else
                {
                    Console.WriteLine("Указанная папка не существует.");
                }
            }
        }

        static int CountOccurrences(string text, string word)
        {
            int count = 0;
            int index = 0;

            while ((index = text.IndexOf(word, index, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                count++;
                index += word.Length;
            }

            return count;
        }
    }
}