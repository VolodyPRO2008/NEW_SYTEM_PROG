using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ProcessManagementLab
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine(" Лабораторная работа: Управление процессами ");
                Console.WriteLine("1. Мониторинг текущего процесса и Топ-5 по памяти");
                Console.WriteLine("2. Запуск дочернего процесса (ping) и чтение вывода");
                Console.WriteLine("3. Запуск notepad.exe с тайм-аутом и Kill()");
                Console.WriteLine("0. Выход");
                Console.Write("Выберите задание: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": Task1(); break;
                    case "2": Task2(); break;
                    case "3": Task3(); break;
                    case "0": return;
                    default: Console.WriteLine("Неверный ввод."); break;
                }

                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }
        }


        static void Task1()
        {
            Console.WriteLine("\n Задание 1");


            Process currentProcess = Process.GetCurrentProcess();
            Console.WriteLine($"[Текущий процесс]");
            Console.WriteLine($"Имя: {currentProcess.ProcessName}");
            Console.WriteLine($"ID: {currentProcess.Id}");
            Console.WriteLine($"Время запуска: {currentProcess.StartTime}");
            Console.WriteLine($"Используемая физическая память: {currentProcess.WorkingSet64 / 1024 / 1024} MB");
            Console.WriteLine($"Количество потоков: {currentProcess.Threads.Count}");
            Console.WriteLine(new string('-', 40));


            Console.WriteLine("[Топ-5 процессов по памяти]");
            Process[] allProcesses = Process.GetProcesses();


            var top5 = allProcesses
                .OrderByDescending(p => p.WorkingSet64)
                .Take(5);

            foreach (var p in top5)
            {
                Console.WriteLine($"Имя: {p.ProcessName,-25} | Память: {p.WorkingSet64 / 1024 / 1024} MB");
            }
        }

        static void Task2()
        {
            Console.WriteLine("\n--- Задание 2 ---");
            Console.WriteLine("Запуск 'ping 8.8.8.8 -n 4'...");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c ping 8.8.8.8 -n 4",
                UseShellExecute = false,   
                RedirectStandardOutput = true, 
                CreateNoWindow = true         
            };

            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                Console.WriteLine(output);


                process.WaitForExit();
                Console.WriteLine("Процесс завершен.");
            }
        }


        static void Task3()
        {
            Console.WriteLine("\n Задание 3 ");
            Console.WriteLine("Запуск 'notepad.exe'...");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "notepad.exe",
                UseShellExecute = true 
            };

            using (Process process = Process.Start(startInfo))
            {
                Console.WriteLine($"Процесс запущен, ID: {process.Id}. Ожидаем 3 секунды...");


                if (!process.WaitForExit(3000))
                {
                    Console.WriteLine("Процесс не завершился за 3 секунды. Принудительное завершение (Kill)...");
                    process.Kill();
                    process.WaitForExit(); 
                    Console.WriteLine("Процесс убит.");
                }
                else
                {
                    Console.WriteLine("Процесс завершился сам.");
                }
            }
        }
    }
}