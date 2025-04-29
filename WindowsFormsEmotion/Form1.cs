using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave; // NuGet: NAudio
using Newtonsoft.Json;// NuGet: Install-Package Newtonsoft.Json

namespace WindowsFormsEmotion
{

    public partial class Form1 : Form
    {
        private string audioFilePath = null;
        private List<EmotionData> emotionDataList = new List<EmotionData>();
        private WaveOutEvent waveOut;
        private string[] availableEmotions = { "Позитив", "Грусть", "Злость", "Нейтральная" };  // Список доступных эмоций

        public Form1()
        {
            InitializeComponent();

            SetTheme(Themes.Light);

            InitializeDataGridView();

            pictureBoxStatus.Visible = false; // Скрываем значок статуса
            panelEmotions.Visible = false;  // Скрываем панель с эмоциями
        }

        private void InitializeDataGridView()
        {
            //  Настраиваем DataGridView после InitializeComponent()
            dataGridViewEmotions.AutoGenerateColumns = false;  // Отключаем автоматическое создание столбцов
            dataGridViewEmotions.AllowUserToAddRows = false;
            dataGridViewEmotions.AllowUserToDeleteRows = false;
            dataGridViewEmotions.ReadOnly = false;
            dataGridViewEmotions.EditMode = DataGridViewEditMode.EditOnEnter;

            DataGridViewTextBoxColumn time_startColumn = new DataGridViewTextBoxColumn();
            time_startColumn.DataPropertyName = "Time_start";
            time_startColumn.HeaderText = "Время начало";
            time_startColumn.ReadOnly = true;
            dataGridViewEmotions.Columns.Add(time_startColumn);

            DataGridViewTextBoxColumn time_endColumn = new DataGridViewTextBoxColumn();
            time_endColumn.DataPropertyName = "Time_end";
            time_endColumn.HeaderText = "Время окончания";
            time_endColumn.ReadOnly = true;
            dataGridViewEmotions.Columns.Add(time_endColumn);

            DataGridViewTextBoxColumn emotionColumn = new DataGridViewTextBoxColumn();
            emotionColumn.DataPropertyName = "Emotion";
            emotionColumn.HeaderText = "Эмоция";
            emotionColumn.ReadOnly = true;
            dataGridViewEmotions.Columns.Add(emotionColumn);

            DataGridViewCheckBoxColumn matchColumn = new DataGridViewCheckBoxColumn();
            matchColumn.DataPropertyName = "IsSelected";
            matchColumn.HeaderText = "Соответствие";
            dataGridViewEmotions.Columns.Add(matchColumn);

            DataGridViewComboBoxColumn correctEmotionColumn = new DataGridViewComboBoxColumn();
            correctEmotionColumn.DataPropertyName = "CorrectEmotion";
            correctEmotionColumn.HeaderText = "Верная эмоция";
            correctEmotionColumn.DataSource = availableEmotions; // Задаем список доступных эмоций
            correctEmotionColumn.DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox;  //  Стиль отображения
            dataGridViewEmotions.Columns.Add(correctEmotionColumn);

        }
        private void dataGridViewEmotions_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowIndex = e.RowIndex;
            int columnIndex = e.ColumnIndex;

            if (rowIndex >= 0 && columnIndex == 3)
            {
                // Обработка нажатия на CheckBox
                emotionDataList[rowIndex].IsSelected = !emotionDataList[rowIndex].IsSelected;

                // Если CheckBox выбран, устанавливаем значение CorrectEmotion в значение Emotion по умолчанию,
                // в противном случае сбрасываем CorrectEmotion в пустую строку
                if (emotionDataList[rowIndex].IsSelected)
                {
                    emotionDataList[rowIndex].CorrectEmotion = emotionDataList[rowIndex].Emotion;
                }
                else
                {
                    emotionDataList[rowIndex].CorrectEmotion = "";
                }

            }

            UpdateDataGridView();
            dataGridViewEmotions.CurrentCell = dataGridViewEmotions.Rows[rowIndex].Cells[columnIndex];
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Audio Files (*.wav;*.mp3)|*.wav;*.mp3"; // Выбираем только аудиофайлы
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                pictureBoxStatus.Image = null;
                emotionDataList.Clear();
                UpdateDataGridView();
                panelEmotions.Controls.Clear();

                audioFilePath = openFileDialog.FileName;
                textBoxFilePath.Text = audioFilePath;
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            saveFileDialog.Title = "Сохранить данные в файл JSON";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                SaveDataGridToJson(saveFileDialog.FileName);
            }
        }

        private void SaveDataGridToJson(string filePath)
        {
            try
            {
                // Преобразование данных в JSON.
                string jsonData = JsonConvert.SerializeObject(emotionDataList, Newtonsoft.Json.Formatting.Indented);

                // Сохранение JSON в файл.
                File.WriteAllText(filePath, jsonData);

                MessageBox.Show("Данные успешно сохранены в файл: " + filePath, "Сохранение данных", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void buttonRecognize_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(audioFilePath))
            {
                MessageBox.Show("Пожалуйста, выберите аудиофайл.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Показываем, что процесс начался
            pictureBoxStatus.Visible = true;
            pictureBoxStatus.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxStatus.Image = Properties.Resources.processing; // Проставьте свою иконку загрузки

            try
            {
                // Имитация работы модели (здесь будет реальная модель)
                await System.Threading.Tasks.Task.Delay(100); // Имитация долгого процесса


                // Добавляем данные об эмоциях.
                emotionDataList.Add(new EmotionData { Time_start = TimeSpan.FromSeconds(0), Time_end = TimeSpan.FromSeconds(3), Emotion = "Позитив", IsSelected = false, CorrectEmotion = "" });
                emotionDataList.Add(new EmotionData { Time_start = TimeSpan.FromSeconds(3.1), Time_end = TimeSpan.FromSeconds(6.2), Emotion = "Грусть", IsSelected = false, CorrectEmotion = "" });
                emotionDataList.Add(new EmotionData { Time_start = TimeSpan.FromSeconds(6.3), Time_end = TimeSpan.FromSeconds(9.1), Emotion = "Злость", IsSelected = false, CorrectEmotion = ""});
                emotionDataList.Add(new EmotionData { Time_start = TimeSpan.FromSeconds(9.2), Time_end = TimeSpan.FromSeconds(12.4), Emotion = "Нейтральная", IsSelected = false, CorrectEmotion = ""});
                emotionDataList.Add(new EmotionData { Time_start = TimeSpan.FromSeconds(12.5), Time_end = TimeSpan.FromSeconds(16), Emotion = "Позитив", IsSelected = false, CorrectEmotion = "" });

                UpdateDataGridView();
                pictureBoxStatus.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBoxStatus.Image = Properties.Resources.success;
                panelEmotions.Visible = true;


            }
            catch (Exception ex)
            {
                pictureBoxStatus.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBoxStatus.Image = Properties.Resources.error;  // Проставьте свою иконку ошибки
                MessageBox.Show($"Ошибка при распознавании: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void UpdateDataGridView()
        {
            // Создаем BindingList, чтобы DataGridView автоматически обновлялся
            BindingList<EmotionData> bindingList = new BindingList<EmotionData>(emotionDataList);
            dataGridViewEmotions.DataSource = bindingList;

        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(audioFilePath))
            {
                MessageBox.Show("Пожалуйста, выберите аудиофайл.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                if (waveOut != null && waveOut.PlaybackState == PlaybackState.Playing)
                {
                    waveOut.Stop();
                    waveOut.Dispose();
                    waveOut = null;
                    return;
                }

                waveOut = new WaveOutEvent();
                var audioFileReader = new AudioFileReader(audioFilePath);

                waveOut.Init(audioFileReader);
                waveOut.Play();
                waveOut.PlaybackStopped += WaveOut_PlaybackStopped;

                // Запускаем отдельный поток для обновления эмоций
                System.Threading.Tasks.Task.Run(() => PlayAudioWithEmotions(audioFileReader));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка воспроизведения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void WaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            // Здесь выполняем действия по завершению воспроизведения (например, переключение кнопки)
            if (waveOut != null)
            {
                waveOut.Dispose();
                waveOut = null;
            }
            this.Invoke((MethodInvoker)delegate
            {
                panelEmotions.Controls.Clear();
            });

        }

        private void PlayAudioWithEmotions(AudioFileReader audioFileReader)
        {

            long currentPosition = 0;
            while (currentPosition < audioFileReader.Length)
            {
                double currentTimeInSeconds = (double)currentPosition / audioFileReader.WaveFormat.AverageBytesPerSecond;
                TimeSpan currentTimeSpan = TimeSpan.FromSeconds(currentTimeInSeconds);

                var currentEmotion = emotionDataList
                    .Where(x => x.Time_start <= currentTimeSpan && x.Time_end >= currentTimeSpan)
                    .FirstOrDefault();

                if (currentEmotion != null)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        panelEmotions.Controls.Clear(); // Очищаем прошлые иконки
                        PictureBox pb = new PictureBox();
                        pb.Dock = DockStyle.Fill;
                        pb.SizeMode = PictureBoxSizeMode.Zoom;
                        switch (currentEmotion.Emotion.ToLower())
                        {
                            case "позитив":
                                pb.Image = Properties.Resources.happy;
                                break;
                            case "грусть":
                                pb.Image = Properties.Resources.sad;
                                break;
                            case "злость":
                                pb.Image = Properties.Resources.angry;
                                break;
                            case "нейтральная":
                                pb.Image = Properties.Resources.neutral;
                                break;
                            default:
                                pb.Image = null;
                                break;

                        }
                        panelEmotions.Controls.Add(pb); // Добавляем новую иконку
                    });
                }

                System.Threading.Thread.Sleep(100);
                currentPosition = audioFileReader.Position;
                if (waveOut == null || waveOut.PlaybackState != PlaybackState.Playing)
                    return;

            }
        }

        public class EmotionData
        {
            public TimeSpan Time_start { get; set; }
            public TimeSpan Time_end { get; set; }
            public string Emotion { get; set; }
            public bool IsSelected { get; set; }
            public string CorrectEmotion { get; set; }

            public EmotionData()
            {
                // Инициализация CorrectEmotion пустой строкой по умолчанию
                CorrectEmotion = "";
            }
        }

        public class Theme
        {
            public Color FormBackColor { get; set; }
            public Color ControlBackColor { get; set; }
            public Color ControlForeColor { get; set; }
            public Color ButtonBackColor { get; set; }
            public Color ButtonForeColor { get; set; }
            public Color DataGridViewBackColor { get; set; }
            public Color DataGridViewForeColor { get; set; }
            public Color DataGridViewHeaderBackColor { get; set; }
            public Color DataGridViewHeaderForeColor { get; set; }
            public Font ControlFont { get; set; } 
            public Font ButtonFont { get; set; } 
            public Font DataGridViewFont { get; set; }

        }

        public static class Themes
        {
            public static Theme Light { get; } = new Theme
            {
                FormBackColor = Color.LightBlue,
                ControlBackColor = Color.White,
                ControlForeColor = Color.Black,
                ButtonBackColor = Color.White,
                ButtonForeColor = Color.Black,
                DataGridViewBackColor = Color.White,
                DataGridViewForeColor = Color.Black,
                DataGridViewHeaderBackColor = Color.White,
                DataGridViewHeaderForeColor = Color.Black,

                ControlFont = new Font("Segoe UI", 9),
                ButtonFont = new Font("Segoe UI", 9, FontStyle.Bold),
                DataGridViewFont = new Font("Segoe UI", 9)
            };
 
        }

        private void SetTheme(Theme theme)
        {

            this.BackColor = theme.FormBackColor;

            labelFilePath.ForeColor = theme.ControlForeColor;
            labelFilePath.Font = theme.ControlFont;
            textBoxFilePath.BackColor = theme.ControlBackColor;
            textBoxFilePath.ForeColor = theme.ControlForeColor;
            textBoxFilePath.Font = theme.ControlFont;

            buttonBrowse.BackColor = theme.ButtonBackColor;
            buttonBrowse.ForeColor = theme.ButtonForeColor;
            buttonBrowse.Font = theme.ButtonFont;
            buttonRecognize.BackColor = theme.ButtonBackColor;
            buttonRecognize.ForeColor = theme.ButtonForeColor;
            buttonRecognize.Font = theme.ButtonFont;
            buttonPlay.BackColor = theme.ButtonBackColor;
            buttonPlay.ForeColor = theme.ButtonForeColor;
            buttonPlay.Font = theme.ButtonFont;
            buttonSave.BackColor = theme.ButtonBackColor;
            buttonSave.ForeColor = theme.ButtonForeColor;
            buttonSave.Font = theme.ButtonFont;

            dataGridViewEmotions.BackgroundColor = theme.DataGridViewBackColor;
            dataGridViewEmotions.ForeColor = theme.DataGridViewForeColor;
            dataGridViewEmotions.Font = theme.DataGridViewFont;
            dataGridViewEmotions.ColumnHeadersDefaultCellStyle.BackColor = theme.DataGridViewHeaderBackColor;
            dataGridViewEmotions.ColumnHeadersDefaultCellStyle.ForeColor = theme.DataGridViewHeaderForeColor;
            dataGridViewEmotions.ColumnHeadersDefaultCellStyle.Font = theme.DataGridViewFont;
            dataGridViewEmotions.EnableHeadersVisualStyles = false; // Важно для применения стилей к заголовкам

        }

    }
}
