using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace WinFormsApp2
{
    public class MainForm : Form
    {
        public MainForm()
        {
            Text = "Практика. Модуль 1. Синхронизация";
            Width = 900;
            Height = 700;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new System.Drawing.Size(800, 600);

            TabControl tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add(CreateTask1Page());
            tabs.TabPages.Add(CreateTask2Page());
            tabs.TabPages.Add(CreateTask3Page());

            Controls.Add(tabs);
        }

        private TabPage CreateTask1Page()
        {
            TabPage page = new TabPage("Задание 1. События");
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Button btn = new Button { Text = "Запустить", Width = 120, Height = 40, Anchor = AnchorStyles.Left | AnchorStyles.Top };
            RichTextBox rtb = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true };

            btn.Click += (s, e) =>
            {
                rtb.Clear();
                btn.Enabled = false;

                int[] numbers = new int[1000];
                ManualResetEvent mre = new ManualResetEvent(false);

                Thread generator = new Thread(() =>
                {
                    Random rnd = new Random();
                    for (int i = 0; i < numbers.Length; i++) numbers[i] = rnd.Next(0, 5001);
                    AppendLog(rtb, "Генерация завершена.");
                    mre.Set();
                });

                Thread tMax = new Thread(() => { mre.WaitOne(); AppendLog(rtb, $"Максимум: {numbers.Max()}"); });
                Thread tMin = new Thread(() => { mre.WaitOne(); AppendLog(rtb, $"Минимум: {numbers.Min()}"); });
                Thread tAvg = new Thread(() => { mre.WaitOne(); AppendLog(rtb, $"Среднее: {numbers.Average():F4}"); });

                Thread waiter = new Thread(() =>
                {
                    generator.Start();
                    tMax.Start();
                    tMin.Start();
                    tAvg.Start();
                    generator.Join(); tMax.Join(); tMin.Join(); tAvg.Join();
                    if (btn.IsHandleCreated) btn.Invoke(new Action(() => btn.Enabled = true));
                });
                waiter.IsBackground = true;
                waiter.Start();
            };

            layout.Controls.Add(btn, 0, 0);
            layout.Controls.Add(rtb, 0, 1);
            page.Controls.Add(layout);
            return page;
        }

        private TabPage CreateTask2Page()
        {
            TabPage page = new TabPage("Задание 2. Критические секции");
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            FlowLayoutPanel topPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            Label lbl = new Label { Text = "Путь к файлу:", AutoSize = true, Margin = new Padding(0, 5, 0, 0) };
            TextBox txtPath = new TextBox { Width = 500 };
            topPanel.Controls.Add(lbl);
            topPanel.Controls.Add(txtPath);

            Button btn = new Button { Text = "Запустить", Width = 120, Height = 40, Margin = new Padding(10) };
            RichTextBox rtb = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true };

            btn.Click += (s, e) =>
            {
                string path = txtPath.Text;
                if (!File.Exists(path)) { MessageBox.Show("Файл не найден!"); return; }
                rtb.Clear();
                btn.Enabled = false;

                ManualResetEvent mre = new ManualResetEvent(false);
                object lockObj = new object();

                Thread reader = new Thread(() =>
                {
                    string text = File.ReadAllText(path);
                    int count = text.Split(new char[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries).Length;
                    AppendLog(rtb, $"Количество предложений: {count}");
                    mre.Set();
                });

                Thread modifier = new Thread(() =>
                {
                    mre.WaitOne();
                    lock (lockObj)
                    {
                        string text = File.ReadAllText(path);
                        File.WriteAllText(path, text.Replace('!', '#'));
                        AppendLog(rtb, "Файл успешно модифицирован (! заменено на #).");
                    }
                });

                Thread waiter = new Thread(() =>
                {
                    reader.Start();
                    modifier.Start();
                    reader.Join();
                    modifier.Join();
                    if (btn.IsHandleCreated) btn.Invoke(new Action(() => btn.Enabled = true));
                });
                waiter.IsBackground = true;
                waiter.Start();
            };

            layout.Controls.Add(topPanel, 0, 0);
            layout.Controls.Add(btn, 0, 1);
            layout.Controls.Add(rtb, 0, 2);
            page.Controls.Add(layout);
            return page;
        }

        private TabPage CreateTask3Page()
        {
            TabPage page = new TabPage("Задание 3. Критические секции");
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            FlowLayoutPanel topPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            Label lbl = new Label { Text = "Искомое число:", AutoSize = true, Margin = new Padding(0, 5, 0, 0) };
            TextBox txtNumber = new TextBox { Width = 100, Text = "42" };
            topPanel.Controls.Add(lbl);
            topPanel.Controls.Add(txtNumber);

            Button btn = new Button { Text = "Запустить", Width = 120, Height = 40, Margin = new Padding(10) };
            RichTextBox rtb = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true };

            btn.Click += (s, e) =>
            {
                if (!int.TryParse(txtNumber.Text, out int target)) { MessageBox.Show("Введите корректное число!"); return; }
                rtb.Clear();
                btn.Enabled = false;

                int[] data = { 9, 3, 7, 1, 5, 8, 2, 6, 4, 0, 15, 12, 18, 11, 14, 13, 17, 16, 19, 10 };
                ManualResetEvent mre = new ManualResetEvent(false);
                object lockObj = new object();

                Thread sorter = new Thread(() =>
                {
                    Array.Sort(data);
                    AppendLog(rtb, $"Массив отсортирован: {string.Join(", ", data)}");
                    mre.Set();
                });

                Thread searcher = new Thread(() =>
                {
                    mre.WaitOne();
                    lock (lockObj)
                    {
                        int index = Array.BinarySearch(data, target);
                        if (index >= 0) AppendLog(rtb, $"Число {target} найдено на индексе {index}.");
                        else AppendLog(rtb, $"Число {target} не найдено в массиве.");
                    }
                });

                Thread waiter = new Thread(() =>
                {
                    sorter.Start();
                    searcher.Start();
                    sorter.Join();
                    searcher.Join();
                    if (btn.IsHandleCreated) btn.Invoke(new Action(() => btn.Enabled = true));
                });
                waiter.IsBackground = true;
                waiter.Start();
            };

            layout.Controls.Add(topPanel, 0, 0);
            layout.Controls.Add(btn, 0, 1);
            layout.Controls.Add(rtb, 0, 2);
            page.Controls.Add(layout);
            return page;
        }

        private void AppendLog(RichTextBox rtb, string message)
        {
            if (rtb.IsHandleCreated)
                rtb.Invoke(new Action(() => rtb.AppendText(message + "\n")));
        }
    }
}