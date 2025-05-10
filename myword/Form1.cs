using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System;
using Word = Microsoft.Office.Interop.Word;
using Microsoft.VisualBasic;
using System.Collections.Generic;

namespace TextEditor
{
    public partial class MainForm : Form
    {
        private List<(int Start, int Length)> _lastHighlights = new List<(int, int)>();
        public MainForm()
        {
            InitializeComponent();
            InitializeFontComboBox();
            InitializeSizeComboBox();

            // Обработчики событий
            menuOpen.Click += menuOpen_Click;
            menuSave.Click += menuSave_Click;
            menuSaveAs.Click += menuSaveAs_Click;
            menuExit.Click += (s, e) => Close();

            menuFont.Click += menuFont_Click;
            menuTextColor.Click += menuTextColor_Click;
            menuBackColor.Click += menuBackColor_Click;

            menuAlignLeft.Click += menuAlignLeft_Click;
            menuAlignCenter.Click += menuAlignCenter_Click;
            menuAlignRight.Click += menuAlignRight_Click;

            btnOpen.Click += menuOpen_Click;
            btnSave.Click += menuSave_Click;
            btnFont.Click += menuFont_Click;
            btnTextColor.Click += menuTextColor_Click;
            btnBackColor.Click += menuBackColor_Click;
            btnBold.Click += btnBold_Click;
            btnItalic.Click += btnItalic_Click;
            btnUnderline.Click += btnUnderline_Click;
            btnAlighLeft.Click += menuAlignLeft_Click;
            btnAlignCenter.Click += menuAlignCenter_Click;
            btnAlignRight.Click += menuAlignRight_Click;

            comboFont.SelectedIndexChanged += ComboFont_SelectedIndexChanged;
            comboSize.SelectedIndexChanged += ComboSize_SelectedIndexChanged;

            openFileDialog.Filter = "Все поддерживаемые файлы (*.rtf;*.txt;*.doc;*.docx)|*.rtf;*.txt;*.doc;*.docx|RTF файлы (*.rtf)|*.rtf|Текстовые файлы (*.txt)|*.txt|Word файлы (*.doc;*.docx)|*.doc;*.docx|Все файлы (*.*)|*.*";
            openFileDialog.FilterIndex = 1;

        }

        private void InitializeSizeComboBox()
        {
            comboSize.Items.AddRange(new object[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 });
            comboSize.SelectedIndex = 2;
        }

        private void ComboFont_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboFont.SelectedItem != null && comboSize.SelectedItem != null)
            {
                float size = float.Parse(comboSize.SelectedItem.ToString());
                richTextBox.SelectionFont = new Font(comboFont.SelectedItem.ToString(), size);
            }
        }

        private void ComboSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboFont.SelectedItem != null && comboSize.SelectedItem != null)
            {
                float size = float.Parse(comboSize.SelectedItem.ToString());
                richTextBox.SelectionFont = new Font(comboFont.SelectedItem.ToString(), size);
            }
        }

        private void menuOpen_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string ext = Path.GetExtension(openFileDialog.FileName).ToLower();
                    switch (ext)
                    {
                        case ".rtf":
                            richTextBox.LoadFile(openFileDialog.FileName, RichTextBoxStreamType.RichText);
                            break;
                        case ".txt":
                            using (var reader = new StreamReader(openFileDialog.FileName, Encoding.UTF8))
                                richTextBox.Text = reader.ReadToEnd();
                            break;
                        case ".doc":
                        case ".docx":
                            OpenDocx(openFileDialog.FileName);
                            break;
                        default:
                            MessageBox.Show("Неподдерживаемый формат.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void menuSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(saveFileDialog.FileName))
                menuSaveAs_Click(sender, e);
            else
                SaveFile(saveFileDialog.FileName);
        }

        private void menuSaveAs_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                SaveFile(saveFileDialog.FileName);
            }
        }

        private void SaveFile(string fileName)
        {
            try
            {
                string ext = Path.GetExtension(fileName).ToLower();
                switch (ext)
                {
                    case ".rtf":
                        richTextBox.SaveFile(fileName, RichTextBoxStreamType.RichText);
                        break;
                    case ".txt":
                        using (var writer = new StreamWriter(fileName, false, Encoding.UTF8))
                            writer.Write(richTextBox.Text);
                        break;
                    case ".doc":
                    case ".docx":
                        SaveAsDocx(fileName);
                        break;
                    default:
                        MessageBox.Show("Неподдерживаемый формат для сохранения.");
                        break;
                }
                saveFileDialog.FileName = fileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveAsDocx(string fileName)
        {
            var wordApp = new Word.Application();
            var doc = wordApp.Documents.Add();
            try
            {
                doc.Content.Text = richTextBox.Text;
                doc.SaveAs2(fileName, Word.WdSaveFormat.wdFormatDocumentDefault);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении Word файла: {ex.Message}");
            }
            finally
            {
                doc.Close();
                wordApp.Quit();
            }
        }

        private void OpenDocx(string fileName)
        {
            var wordApp = new Word.Application();
            Word.Document doc = null;
            try
            {
                doc = wordApp.Documents.Open(fileName, ReadOnly: true);
                richTextBox.Text = doc.Content.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии Word файла: {ex.Message}");
            }
            finally
            {
                doc?.Close(false);
                wordApp.Quit();
            }
        }

        private void menuFont_Click(object sender, EventArgs e)
        {
            fontDialog.Font = richTextBox.SelectionFont;
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                richTextBox.SelectionFont = fontDialog.Font;
            }
        }

        private void menuTextColor_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                richTextBox.SelectionColor = colorDialog.Color;
            }
        }

        private void menuBackColor_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                richTextBox.SelectionBackColor = colorDialog.Color;
            }
        }

        private void menuAlignLeft_Click(object sender, EventArgs e)
        {
            richTextBox.SelectionAlignment = HorizontalAlignment.Left;
        }

        private void menuAlignCenter_Click(object sender, EventArgs e)
        {
            richTextBox.SelectionAlignment = HorizontalAlignment.Center;
        }

        private void menuAlignRight_Click(object sender, EventArgs e)
        {
            richTextBox.SelectionAlignment = HorizontalAlignment.Right;
        }

        private void ToggleFontStyle(FontStyle style)
        {
            if (richTextBox.SelectionFont != null)
            {
                Font currentFont = richTextBox.SelectionFont;
                FontStyle newStyle = currentFont.Style ^ style;
                richTextBox.SelectionFont = new Font(currentFont.FontFamily, currentFont.Size, newStyle);
            }
        }

        private void btnBold_Click(object sender, EventArgs e) => ToggleFontStyle(FontStyle.Bold);
        private void btnItalic_Click(object sender, EventArgs e) => ToggleFontStyle(FontStyle.Italic);
        private void btnUnderline_Click(object sender, EventArgs e) => ToggleFontStyle(FontStyle.Underline);

        private void InitializeFontComboBox()
        {
            foreach (FontFamily font in FontFamily.Families)
                comboFont.Items.Add(font.Name);
        }

        private void tsbSearchString_Click(object sender, EventArgs e)
        {
            string searchText = ShowSearchDialog();
            if (string.IsNullOrWhiteSpace(searchText))
                return;

            RemoveLastHighlights();
            var matches = FindTextMatches(richTextBox.Text, searchText);
            int lenSearchText = searchText.Length;
            HighlightMatches(matches, lenSearchText, Color.Yellow);
        }

        private string ShowSearchDialog()
        {
            using (var form = new Form())
            {
                form.Text = "Поиск в документе";
                form.Size = new Size(300, 150);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;

                var textBox = new TextBox { Width = 250, Location = new Point(20, 20) };
                var buttonOk = new Button { Text = "Найти", DialogResult = DialogResult.OK, Location = new Point(20, 60) };
                var buttonCancel = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel, Location = new Point(160, 60) };

                form.Controls.Add(textBox);
                form.Controls.Add(buttonOk);
                form.Controls.Add(buttonCancel);
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;

                return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;
            }
        }

        // Поиск Кнута Мориса Прата
        private List<int> FindTextMatches(string text, string pattern)
        {
            var matches = new List<int>();
            if (string.IsNullOrEmpty(pattern))
                return matches;

            int n = text.Length;
            int m = pattern.Length;
            int[] lps = ComputeLPSArray(pattern);

            int i = 0; // индекс для текста
            int j = 0; // индекс для паттерна
            while (i < n)
            {
                if (pattern[j] == text[i])
                {
                    j++;
                    i++;
                }

                if (j == m)
                {
                    matches.Add(i - j); // Добавляем позицию совпадения
                    j = lps[j - 1];
                }
                else if (i < n && pattern[j] != text[i])
                {
                    if (j != 0)
                        j = lps[j - 1];
                    else
                        i++;
                }
            }

            return matches;
        }
        private int[] ComputeLPSArray(string pattern)
        {
            int[] lps = new int[pattern.Length];
            int len = 0;
            int i = 1;
            while (i < pattern.Length)
            {
                if (pattern[i] == pattern[len])
                {
                    len++;
                    lps[i] = len;
                    i++;
                }
                else
                {
                    if (len != 0)
                    {
                        len = lps[len - 1];
                    }
                    else
                    {
                        lps[i] = 0;
                        i++;
                    }
                }
            }
            return lps;
        }

        private void HighlightMatches(List<int> matches, int length, Color highlightColor)
        {
            foreach (var start in matches)
            {
                richTextBox.Select(start, length);
                richTextBox.SelectionBackColor = highlightColor;
                _lastHighlights.Add((start, length)); // Запоминаем выделение
            }
        }

        private void RemoveLastHighlights()
        {
            foreach (var (start, length) in _lastHighlights)
            {
                richTextBox.Select(start, length);
                richTextBox.SelectionBackColor = richTextBox.BackColor;
            }
            _lastHighlights.Clear();
        }

        
    }
}
