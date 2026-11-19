using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp2
{
    public class MainForm : Form
    {
        public MainForm()
        {
            Text = "Практика. Модуль 1. Часть 2";
            Width = 950;
            Height = 750;

            TabControl tabs = new TabControl { Left = 10, Top = 10, Width = 910, Height = 690, Dock = DockStyle.Fill };

            tabs.TabPages.Add(CreateTask1Page());
            tabs.TabPages.Add(CreateTask23Page());
            tabs.TabPages.Add(CreateTask45Page());
            tabs.TabPages.Add(CreateTask6Page());
            tabs.TabPages.Add(CreateTask7Page());
            tabs.TabPages.Add(CreateTask8Page());

            Controls.Add(tabs);
        }

        private TabPage CreateTask1Page()
        {
            TabPage page = new TabPage("Задание 1");

            // Сдвигаем все вниз и вправо, чтобы было красиво
            Label lblNum = new Label { Text = "Числа:", Left = 20, Top = 23, Width = 70 };
            ComboBox cbNum = new ComboBox { Left = 100, Top = 20, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cbNum.Items.AddRange(new object[] { "Lowest", "BelowNormal", "Normal", "AboveNormal", "Highest" });
            cbNum.SelectedIndex = 2;

            Label lblLetter = new Label { Text = "Буквы:", Left = 20, Top = 58, Width = 70 };
            ComboBox cbLetter = new ComboBox { Left = 100, Top = 55, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cbLetter.Items.AddRange(new object[] { "Lowest", "BelowNormal", "Normal", "AboveNormal", "Highest" });
            cbLetter.SelectedIndex = 2;

            Label lblSym = new Label { Text = "Символы:", Left = 20, Top = 93, Width = 70 };
            ComboBox cbSym = new ComboBox { Left = 100, Top = 90, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            cbSym.Items.AddRange(new object[] { "Lowest", "BelowNormal", "Normal", "AboveNormal", "Highest" });
            cbSym.SelectedIndex = 2;

            Button btn = new Button { Text = "Запустить", Left = 250, Top = 20, Width = 120, Height = 115 };
            RichTextBox rtb = new RichTextBox { Left = 20, Top = 150, Width = 850, Height = 450, ReadOnly = true };

            btn.Click += (s, e) =>
            {
                rtb.Clear();
                btn.Enabled = false;
                ThreadPriority p1 = (ThreadPriority)Enum.Parse(typeof(ThreadPriority), cbNum.Text);
                ThreadPriority p2 = (ThreadPriority)Enum.Parse(typeof(ThreadPriority), cbLetter.Text);
                ThreadPriority p3 = (ThreadPriority)Enum.Parse(typeof(ThreadPriority), cbSym.Text);

                Thread t1 = new Thread(() => GenerateNumbers(rtb)) { Priority = p1, IsBackground = true };
                Thread t2 = new Thread(() => GenerateLetters(rtb)) { Priority = p2, IsBackground = true };
                Thread t3 = new Thread(() => GenerateSymbols(rtb)) { Priority = p3, IsBackground = true };

                Task.Run(() =>
                {
                    t1.Start(); t2.Start(); t3.Start();
                    t1.Join(); t2.Join(); t3.Join();
                    if (btn.IsHandleCreated) btn.Invoke(new Action(() => btn.Enabled = true));
                });
            };

            page.Controls.Add(lblNum);
            page.Controls.Add(cbNum);
            page.Controls.Add(lblLetter);
            page.Controls.Add(cbLetter);
            page.Controls.Add(lblSym);
            page.Controls.Add(cbSym);
            page.Controls.Add(btn);
            page.Controls.Add(rtb);

            return page;
        }

        private void GenerateNumbers(RichTextBox rtb)
        {
            Random r = new Random();
            for (int i = 0; i < 200; i++)
            {
                if (rtb.IsHandleCreated)
                    rtb.Invoke(new Action(() => rtb.AppendText($"[{Thread.CurrentThread.Priority}] {r.Next(100, 999)} ")));
                Thread.Sleep(1);
            }
        }

        private void GenerateLetters(RichTextBox rtb)
        {
            Random r = new Random();
            for (int i = 0; i < 200; i++)
            {
                if (rtb.IsHandleCreated)
                    rtb.Invoke(new Action(() => rtb.AppendText($"[{Thread.CurrentThread.Priority}] {(char)r.Next('A', 'Z')} ")));
                Thread.Sleep(1);
            }
        }

        private void GenerateSymbols(RichTextBox rtb)
        {
            string syms = "!@#$%^&*()_+-=[]{};:,.<>?/";
            Random r = new Random();
            for (int i = 0; i < 200; i++)
            {
                if (rtb.IsHandleCreated)
                    rtb.Invoke(new Action(() => rtb.AppendText($"[{Thread.CurrentThread.Priority}] {syms[r.Next(syms.Length)]} ")));
                Thread.Sleep(1);
            }
        }

        private TabPage CreateTask23Page()
        {
            TabPage page = new TabPage("Задания 2-3");
            TextBox txtSrc = new TextBox { Left = 100, Top = 15, Width = 600 };
            TextBox txtDst = new TextBox { Left = 100, Top = 45, Width = 600 };
            NumericUpDown numThreads = new NumericUpDown { Left = 100, Top = 75, Width = 100, Minimum = 1, Maximum = 16, Value = 1 };
            ProgressBar pb = new ProgressBar { Left = 10, Top = 140, Width = 880, Height = 30 };
            Button btnStart = new Button { Text = "Старт", Left = 100, Top = 105, Width = 80 };
            Button btnPause = new Button { Text = "Пауза", Left = 190, Top = 105, Width = 80 };
            Button btnStop = new Button { Text = "Стоп", Left = 280, Top = 105, Width = 80 };

            CancellationTokenSource cts = null;
            bool isPaused = false;
            object pauseLock = new object();

            btnStart.Click += (s, e) =>
            {
                if (!File.Exists(txtSrc.Text) || string.IsNullOrWhiteSpace(txtDst.Text))
                {
                    MessageBox.Show("Укажите файлы");
                    return;
                }
                pb.Value = 0;
                btnStart.Enabled = false;
                cts = new CancellationTokenSource();
                isPaused = false;
                var token = cts.Token;

                Task.Run(() =>
                {
                    try
                    {
                        using (FileStream src = new FileStream(txtSrc.Text, FileMode.Open, FileAccess.Read))
                        using (FileStream dst = new FileStream(txtDst.Text, FileMode.Create, FileAccess.Write))
                        {
                            long len = src.Length;
                            byte[] buffer = new byte[8192];
                            long totalRead = 0;
                            int read;
                            while ((read = src.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                lock (pauseLock)
                                {
                                    while (isPaused && !token.IsCancellationRequested)
                                        Monitor.Wait(pauseLock);
                                }
                                if (token.IsCancellationRequested) break;
                                dst.Write(buffer, 0, read);
                                totalRead += read;
                                int percent = (int)((double)totalRead / len * 100);
                                if (pb.IsHandleCreated)
                                    pb.Invoke(new Action(() => { pb.Value = Math.Min(100, percent); }));
                            }
                        }
                    }
                    catch { }
                    finally
                    {
                        if (btnStart.IsHandleCreated) btnStart.Invoke(new Action(() => btnStart.Enabled = true));
                    }
                }, token);
            };

            btnPause.Click += (s, e) =>
            {
                lock (pauseLock)
                {
                    isPaused = !isPaused;
                    btnPause.Text = isPaused ? "Продолжить" : "Пауза";
                    Monitor.PulseAll(pauseLock);
                }
            };

            btnStop.Click += (s, e) =>
            {
                lock (pauseLock)
                {
                    isPaused = false;
                    btnPause.Text = "Пауза";
                    cts?.Cancel();
                    Monitor.PulseAll(pauseLock);
                }
                MessageBox.Show("Копирование остановлено");
            };

            page.Controls.Add(new Label { Text = "Исходный:", Left = 10, Top = 18, Width = 80 });
            page.Controls.Add(txtSrc);
            page.Controls.Add(new Label { Text = "Копия:", Left = 10, Top = 48, Width = 80 });
            page.Controls.Add(txtDst);
            page.Controls.Add(new Label { Text = "Потоков:", Left = 10, Top = 78, Width = 80 });
            page.Controls.Add(numThreads);
            page.Controls.Add(btnStart);
            page.Controls.Add(btnPause);
            page.Controls.Add(btnStop);
            page.Controls.Add(pb);
            return page;
        }

        private TabPage CreateTask45Page()
        {
            TabPage page = new TabPage("Задания 4-5");
            TextBox txtSrc = new TextBox { Left = 100, Top = 15, Width = 600 };
            TextBox txtDst = new TextBox { Left = 100, Top = 45, Width = 600 };
            NumericUpDown numThreads = new NumericUpDown { Left = 100, Top = 75, Width = 100, Minimum = 1, Maximum = 16, Value = 1 };
            ProgressBar pb = new ProgressBar { Left = 10, Top = 140, Width = 880, Height = 30 };
            Button btnStart = new Button { Text = "Старт", Left = 100, Top = 105, Width = 80 };
            Button btnPause = new Button { Text = "Пауза", Left = 190, Top = 105, Width = 80 };
            Button btnStop = new Button { Text = "Стоп", Left = 280, Top = 105, Width = 80 };

            CancellationTokenSource cts = null;
            bool isPaused = false;
            object pauseLock = new object();

            btnStart.Click += (s, e) =>
            {
                if (!Directory.Exists(txtSrc.Text) || string.IsNullOrWhiteSpace(txtDst.Text))
                {
                    MessageBox.Show("Укажите директории");
                    return;
                }
                pb.Value = 0;
                btnStart.Enabled = false;
                cts = new CancellationTokenSource();
                isPaused = false;
                var token = cts.Token;

                Task.Run(() =>
                {
                    try
                    {
                        var files = Directory.GetFiles(txtSrc.Text, "*", SearchOption.AllDirectories);
                        if (!Directory.Exists(txtDst.Text)) Directory.CreateDirectory(txtDst.Text);
                        int total = files.Length;
                        int done = 0;

                        foreach (var file in files)
                        {
                            lock (pauseLock)
                            {
                                while (isPaused && !token.IsCancellationRequested)
                                    Monitor.Wait(pauseLock);
                            }
                            if (token.IsCancellationRequested) break;
                            string rel = file.Substring(txtSrc.Text.Length).TrimStart(Path.DirectorySeparatorChar);
                            string destFile = Path.Combine(txtDst.Text, rel);
                            Directory.CreateDirectory(Path.GetDirectoryName(destFile));
                            File.Copy(file, destFile, true);
                            done++;
                            int percent = (int)((double)done / total * 100);
                            if (pb.IsHandleCreated)
                                pb.Invoke(new Action(() => { pb.Value = Math.Min(100, percent); }));
                        }
                    }
                    catch { }
                    finally
                    {
                        if (btnStart.IsHandleCreated) btnStart.Invoke(new Action(() => btnStart.Enabled = true));
                    }
                }, token);
            };

            btnPause.Click += (s, e) =>
            {
                lock (pauseLock)
                {
                    isPaused = !isPaused;
                    btnPause.Text = isPaused ? "Продолжить" : "Пауза";
                    Monitor.PulseAll(pauseLock);
                }
            };

            btnStop.Click += (s, e) =>
            {
                lock (pauseLock)
                {
                    isPaused = false;
                    btnPause.Text = "Пауза";
                    cts?.Cancel();
                    Monitor.PulseAll(pauseLock);
                }
                MessageBox.Show("Копирование остановлено");
            };

            page.Controls.Add(new Label { Text = "Исходная:", Left = 10, Top = 18, Width = 80 });
            page.Controls.Add(txtSrc);
            page.Controls.Add(new Label { Text = "Копия:", Left = 10, Top = 48, Width = 80 });
            page.Controls.Add(txtDst);
            page.Controls.Add(new Label { Text = "Потоков:", Left = 10, Top = 78, Width = 80 });
            page.Controls.Add(numThreads);
            page.Controls.Add(btnStart);
            page.Controls.Add(btnPause);
            page.Controls.Add(btnStop);
            page.Controls.Add(pb);
            return page;
        }

        private TabPage CreateTask6Page()
        {
            TabPage page = new TabPage("Задание 6");
            TextBox txt = new TextBox { Left = 100, Top = 15, Width = 150 };
            Button btn = new Button { Text = "Вычислить", Left = 260, Top = 13, Width = 100 };
            Label lbl = new Label { Left = 100, Top = 50, Width = 400, Height = 100 };

            btn.Click += async (s, e) =>
            {
                if (!int.TryParse(txt.Text, out int n) || n < 0) return;
                btn.Enabled = false;
                lbl.Text = "Вычисление...";
                var res = await Task.Run(() => Factorial(n));
                lbl.Text = $"Факториал {n} = {res}";
                btn.Enabled = true;
            };

            page.Controls.Add(new Label { Text = "Число:", Left = 10, Top = 18, Width = 80 });
            page.Controls.Add(txt);
            page.Controls.Add(btn);
            page.Controls.Add(lbl);
            return page;
        }

        private System.Numerics.BigInteger Factorial(int n)
        {
            System.Numerics.BigInteger res = 1;
            for (int i = 2; i <= n; i++) res *= i;
            return res;
        }

        private TabPage CreateTask7Page()
        {
            TabPage page = new TabPage("Задание 7");
            TextBox txtBase = new TextBox { Left = 100, Top = 15, Width = 150 };
            TextBox txtPow = new TextBox { Left = 100, Top = 45, Width = 150 };
            Button btn = new Button { Text = "Вычислить", Left = 100, Top = 75, Width = 100 };
            Label lbl = new Label { Left = 100, Top = 110, Width = 400, Height = 100 };

            btn.Click += async (s, e) =>
            {
                if (!double.TryParse(txtBase.Text, out double b) || !double.TryParse(txtPow.Text, out double p)) return;
                btn.Enabled = false;
                lbl.Text = "Вычисление...";
                var res = await Task.Run(() => Math.Pow(b, p));
                lbl.Text = $"Результат: {res}";
                btn.Enabled = true;
            };

            page.Controls.Add(new Label { Text = "Число:", Left = 10, Top = 18, Width = 80 });
            page.Controls.Add(txtBase);
            page.Controls.Add(new Label { Text = "Степень:", Left = 10, Top = 48, Width = 80 });
            page.Controls.Add(txtPow);
            page.Controls.Add(btn);
            page.Controls.Add(lbl);
            return page;
        }

        private TabPage CreateTask8Page()
        {
            TabPage page = new TabPage("Задание 8");
            RichTextBox rtbIn = new RichTextBox { Left = 10, Top = 15, Width = 880, Height = 150 };
            Button btn = new Button { Text = "Анализ", Left = 10, Top = 175, Width = 100 };
            Label lbl = new Label { Left = 10, Top = 215, Width = 880, Height = 200 };

            btn.Click += async (s, e) =>
            {
                string text = rtbIn.Text;
                btn.Enabled = false;
                lbl.Text = "Анализ...";
                var res = await Task.Run(() => AnalyzeText(text));
                lbl.Text = $"Гласные: {res.vowels}\nСогласные: {res.consonants}\nСимволы: {res.symbols}";
                btn.Enabled = true;
            };

            page.Controls.Add(rtbIn);
            page.Controls.Add(btn);
            page.Controls.Add(lbl);
            return page;
        }

        private void InitializeComponent()
        {

        }

        private (int vowels, int consonants, int symbols) AnalyzeText(string text)
        {
            int vowels = 0, consonants = 0, symbols = 0;
            string v = "aeiouyаеёиоуыэюяAEIOUYАЕЁИОУЫЭЮЯ";
            string c = "bcdfghjklmnpqrstvwxzбвгджзйклмнпрстфхцчшщBCDFGHJKLMNPQRSTVWXZБВГДЖЗЙКЛМНПРСТФХЦЧШЩ";
            foreach (char ch in text)
            {
                if (v.IndexOf(ch) >= 0) vowels++;
                else if (c.IndexOf(ch) >= 0) consonants++;
                else if (!char.IsWhiteSpace(ch)) symbols++;
            }
            return (vowels, consonants, symbols);
        }
    }
}