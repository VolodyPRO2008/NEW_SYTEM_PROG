using System;
using MyLibrary;

namespace ClientApp
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
                Console.WriteLine("1 - Информационное сообщение");
                Console.WriteLine("2 - Проверка на высокосный год");
                Console.WriteLine("3 - Max, Min, Sum трех чисел");
                Console.WriteLine("4 - Факториал, простое, четное, нечетное");
                Console.WriteLine("5 - Работа с дробями");

                string choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        InfoModule.ShowMessage("Работа с DLL выполнена успешно!");
                        break;
                    case "2":
                        Console.Write("Введите год: ");
                        int year = int.Parse(Console.ReadLine());
                        Console.WriteLine(DateModule.IsLeapYear(year) ? "Высокосный" : "Не высокосный");
                        break;
                    case "3":
                        Console.Write("Введите три числа через пробел: ");
                        string[] parts = Console.ReadLine().Split(' ');
                        int a = int.Parse(parts[0]), b = int.Parse(parts[1]), c = int.Parse(parts[2]);
                        Console.WriteLine($"Max: {MathModule.Max(a, b, c)}");
                        Console.WriteLine($"Min: {MathModule.Min(a, b, c)}");
                        Console.WriteLine($"Sum: {MathModule.Sum(a, b, c)}");
                        break;
                    case "4":
                        Console.Write("Введите число: ");
                        int n = int.Parse(Console.ReadLine());
                        Console.WriteLine($"Факториал: {NumberModule.Factorial(n)}");
                        Console.WriteLine($"Простое: {(NumberModule.IsPrime(n) ? "Да" : "Нет")}");
                        Console.WriteLine($"Четное: {(NumberModule.IsEven(n) ? "Да" : "Нет")}");
                        Console.WriteLine($"Нечетное: {(NumberModule.IsOdd(n) ? "Да" : "Нет")}");
                        break;
                    case "5":
                        Fraction f1 = new Fraction(1, 2);
                        Fraction f2 = new Fraction(1, 3);
                        Console.WriteLine($"Дробь 1: {f1}");
                        Console.WriteLine($"Дробь 2: {f2}");
                        Console.WriteLine($"Сложение: {f1.Add(f2)}");
                        Console.WriteLine($"Вычитание: {f1.Subtract(f2)}");
                        Console.WriteLine($"Умножение: {f1.Multiply(f2)}");
                        Console.WriteLine($"Деление: {f1.Divide(f2)}");
                        break;
                    case "0": return;
                    default: Console.WriteLine("Неверный выбор."); break;
                }

                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }
        }
    }
}