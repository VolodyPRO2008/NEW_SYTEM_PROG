using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace WinFormsApp2
{
    public class MainForm : Form
    {
        private TextBox txtStart;
        private TextBox txtEnd;
        private TextBox txtThreads;
        private Button btnTask6;
        private RichTextBox rtbOutput6;
        private Button btnTask7;
        private RichTextBox rtbOutput7;
        private Button btnTask15;
        private RichTextBox rtbOutput15;
        private int[] data7;

        public MainForm()
        {
            Text = "Практика. Модуль 1. Все задания";
            Width = 900;
            Height = 700;

            TabControl tabs = new TabControl { Left = 10, Top = 10, Width = 860, Height = 640 };

            TabPage page15 = new TabPage("Задания 1-5");
            btnTask15 = new Button { Text = "Запустить задания 1-5", Left = 10, Top = 10, Width = 220, Height = 30 };
            rtbOutput15 = new RichTextBox { Left = 10, Top = 50, Width = 820, Height = 540, ReadOnly = true };
            btnTask15.Click += BtnTask15_Click;
            page15.Controls.Add(btnTask15);
            page15.Controls.Add(rtbOutput15);

            TabPage page6 = new TabPage("Задание 6");
            Label lblStart = new Label { Text = "Начало:", Left = 10, Top = 15, Width = 60 };
            txtStart = new TextBox { Left = 75, Top = 10, Width = 80, Text = "0" };
            Label lblEnd = new Label { Text = "Конец:", Left = 165, Top = 15, Width = 60 };
            txtEnd = new TextBox { Left = 230, Top = 10, Width = 80, Text = "50" };
            Label lblThreads = new Label { Text = "Потоков:", Left = 320, Top = 15, Width = 70 };
            txtThreads = new TextBox { Left = 395, Top = 10, Width = 60, Text = "1" };
            btnTask6 = new Button { Text = "Запуск", Left = 470, Top = 8, Width = 100 };
            rtbOutput6 = new RichTextBox { Left = 10, Top = 45, Width = 820, Height = 545, ReadOnly = true };
            btnTask6.Click += BtnTask6_Click;
            page6.Controls.Add(lblStart);
            page6.Controls.Add(txtStart);
            page6.Controls.Add(lblEnd);
            page6.Controls.Add(txtEnd);
            page6.Controls.Add(lblThreads);
            page6.Controls.Add(txtThreads);
            page6.Controls.Add(btnTask6);
            page6.Controls.Add(rtbOutput6);

            TabPage page7 = new TabPage("Задание 7");
            btnTask7 = new Button { Text = "Сгенерировать и вычислить", Left = 10, Top = 10, Width = 220, Height = 30 };
            rtbOutput7 = new RichTextBox { Left = 10, Top = 50, Width = 820, Height = 540, ReadOnly = true };
            btnTask7.Click += BtnTask7_Click;
            page7.Controls.Add(btnTask7);
            page7.Controls.Add(rtbOutput7);

            tabs.TabPages.Add(page15);
            tabs.TabPages.Add(page6);
            tabs.TabPages.Add(page7);
            Controls.Add(tabs);
        }

        private void BtnTask15_Click(object sender, EventArgs e)
        {
            rtbOutput15.Clear();
            btnTask15.Enabled = false;

            Thread worker = new Thread(() =>
            {
                Random rnd = new Random();
                Action<string> log = msg =>
                {
                    if (rtbOutput15.IsHandleCreated)
                        rtbOutput15.Invoke(new Action(() => rtbOutput15.AppendText(msg + "\n")));
                };

                log("Задание 1");
                Thread t1 = new Thread(() => PrintRange(0, 50, log));
                t1.Start();
                t1.Join();

                log("Задание 2");
                log("Используем диапазон 10..30 для демонстрации");
                Thread t2 = new Thread(() => PrintRange(10, 30, log));
                t2.Start();
                t2.Join();

                log("Задание 3");
                log("Используем диапазон 1..30 и 3 потока");
                RunMultipleThreads(1, 30, 3, log);

                log("Задание 4 и 5");
                int[] data = GenerateArray(10000);
                int max = 0, min = 0;
                double avg = 0;
                Thread tMax = new Thread(() => max = data.Max());
                Thread tMin = new Thread(() => min = data.Min());
                Thread tAvg = new Thread(() => avg = data.Average());
                tMax.Start(); tMin.Start(); tAvg.Start();
                tMax.Join(); tMin.Join(); tAvg.Join();
                log($"Максимум: {max}");
                log($"Минимум: {min}");
                log($"Среднее: {avg:F4}");

                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "result.txt");
                Thread tFile = new Thread(() => WriteToFile(filePath, data, max, min, avg));
                tFile.Start();
                tFile.Join();
                log($"Результаты записаны в файл: {filePath}");

                if (btnTask15.IsHandleCreated)
                    btnTask15.Invoke(new Action(() => btnTask15.Enabled = true));
            });
            worker.IsBackground = true;
            worker.Start();
        }

        private void PrintRange(int start, int end, Action<string> log)
        {
            if (start > end)
            {
                int tmp = start; start = end; end = tmp;
            }
            for (int i = start; i <= end; i++)
                log($"[{Thread.CurrentThread.ManagedThreadId}] {i}");
        }

        private void RunMultipleThreads(int start, int end, int threadsCount, Action<string> log)
        {
            if (threadsCount <= 0) threadsCount = 1;
            if (start > end)
            {
                int tmp = start; start = end; end = tmp;
            }
            int total = end - start + 1;
            int perThread = (int)Math.Ceiling(total / (double)threadsCount);
            List<Thread> threads = new List<Thread>();
            for (int i = 0; i < threadsCount; i++)
            {
                int from = start + i * perThread;
                int to = Math.Min(from + perThread - 1, end);
                if (from > end) break;
                Thread t = new Thread(() => PrintRange(from, to, log));
                threads.Add(t);
                t.Start();
            }
            foreach (var t in threads) t.Join();
        }

        private int[] GenerateArray(int size)
        {
            Random rnd = new Random();
            int[] arr = new int[size];
            for (int i = 0; i < size; i++) arr[i] = rnd.Next(0, 100000);
            return arr;
        }

        private void WriteToFile(string path, int[] data, int max, int min, double avg)
        {
            using (StreamWriter sw = new StreamWriter(path, false))
            {
                sw.WriteLine("Набор чисел:");
                sw.WriteLine(string.Join(", ", data));
                sw.WriteLine($"Максимум: {max}");
                sw.WriteLine($"Минимум: {min}");
                sw.WriteLine($"Среднее: {avg:F4}");
            }
        }

        private void BtnTask6_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtStart.Text, out int start) ||
                !int.TryParse(txtEnd.Text, out int end) ||
                !int.TryParse(txtThreads.Text, out int threadsCount))
            {
                MessageBox.Show("Введите корректные числа");
                return;
            }

            if (start > end)
            {
                int tmp = start; start = end; end = tmp;
            }
            if (threadsCount <= 0) threadsCount = 1;

            rtbOutput6.Clear();
            btnTask6.Enabled = false;

            int total = end - start + 1;
            int perThread = (int)Math.Ceiling(total / (double)threadsCount);
            int[] counters = new int[] { threadsCount };

            for (int i = 0; i < threadsCount; i++)
            {
                int from = start + i * perThread;
                int to = Math.Min(from + perThread - 1, end);
                if (from > end) break;
                Thread t = new Thread(() => PrintRangeUI(from, to, counters));
                t.IsBackground = true;
                t.Start();
            }
        }

        private void PrintRangeUI(int from, int to, int[] counters)
        {
            for (int i = from; i <= to; i++)
            {
                if (rtbOutput6.IsHandleCreated)
                    rtbOutput6.Invoke(new Action(() => rtbOutput6.AppendText($"[{Thread.CurrentThread.ManagedThreadId}] {i}\n")));
            }
            if (Interlocked.Decrement(ref counters[0]) == 0)
            {
                if (btnTask6.IsHandleCreated)
                    btnTask6.Invoke(new Action(() => btnTask6.Enabled = true));
            }
        }

        private void BtnTask7_Click(object sender, EventArgs e)
        {
            rtbOutput7.Clear();
            btnTask7.Enabled = false;

            Thread worker = new Thread(() =>
            {
                Random rnd = new Random();
                data7 = new int[10000];
                for (int i = 0; i < data7.Length; i++) data7[i] = rnd.Next(0, 100000);

                int max = 0, min = 0;
                double avg = 0;

                Thread tMax = new Thread(() => max = data7.Max());
                Thread tMin = new Thread(() => min = data7.Min());
                Thread tAvg = new Thread(() => avg = data7.Average());
                Thread tFile = new Thread(() => SaveToFile7(max, min, avg));

                tMax.Start(); tMin.Start(); tAvg.Start();
                tMax.Join(); tMin.Join(); tAvg.Join();
                tFile.Start(); tFile.Join();

                int localMax = max, localMin = min;
                double localAvg = avg;

                if (rtbOutput7.IsHandleCreated)
                {
                    rtbOutput7.Invoke(new Action(() =>
                    {
                        rtbOutput7.AppendText($"Максимум: {localMax}\n");
                        rtbOutput7.AppendText($"Минимум: {localMin}\n");
                        rtbOutput7.AppendText($"Среднее: {localAvg:F4}\n");
                        rtbOutput7.AppendText("Данные сохранены в result.txt\n");
                        btnTask7.Enabled = true;
                    }));
                }
            });
            worker.IsBackground = true;
            worker.Start();
        }

        private void SaveToFile7(int max, int min, double avg)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "result.txt");
            using (StreamWriter sw = new StreamWriter(path, false))
            {
                sw.WriteLine("Набор чисел:");
                sw.WriteLine(string.Join(", ", data7));
                sw.WriteLine($"Максимум: {max}");
                sw.WriteLine($"Минимум: {min}");
                sw.WriteLine($"Среднее: {avg:F4}");
            }
        }
    }
}