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
using NAudio.Wave;
using Newtonsoft.Json;
using System.Diagnostics;

namespace WindowsFormsEmotion
{

    public partial class Form1 : Form
    {
        private string pythonScriptPath = "C:\\diplom\\GigaAM\\emotion_analysis.py"; //Укажите свой путь до скрипта
        string outputDirectory = "C:\\diplom\\GigaAM"; 
        private string audioFilePath = null;
        private WaveOutEvent waveOut;
        private AudioFileReader audioFile;
        private bool isPaused = false;

        private List<EmotionData> emotionDataList = new List<EmotionData>();
        private string[] availableEmotions = { "Позитив", "Грусть", "Злость", "Нейтральная" };  // Список доступных эмоций

        private static readonly Dictionary<string, string> EmotionMap = new Dictionary<string, string>()
    {
        {"neutral", "Нейтральная"},
        {"angry", "Злость"},
        {"sad", "Грусть"},
        {"positive", "Позитив"}
    };

        public Form1()
        {
            InitializeComponent();

            SetTheme(Themes.Light);

            InitializeDataGridView();

            DoubleBuffered = true;
            panelEmotions.Visible = false;  // Скрываем панель с эмоциями
        }

        private void Form1_Load(object sender, EventArgs e)
        {
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
            time_startColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells; //  Автоматическое изменение размера по содержимому
            dataGridViewEmotions.Columns.Add(time_startColumn);

            DataGridViewTextBoxColumn time_endColumn = new DataGridViewTextBoxColumn();
            time_endColumn.DataPropertyName = "Time_end";
            time_endColumn.HeaderText = "Время окончания";
            time_endColumn.ReadOnly = true;
            time_endColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells; //  Автоматическое изменение размера по содержимому
            dataGridViewEmotions.Columns.Add(time_endColumn);

            DataGridViewTextBoxColumn emotion2probColumn = new DataGridViewTextBoxColumn();
            emotion2probColumn.DataPropertyName = "Emotion2prob";
            emotion2probColumn.HeaderText = "Вероятности эмоций";
            emotion2probColumn.ReadOnly = true;
            emotion2probColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; // Занимает все доступное место
            emotion2probColumn.DefaultCellStyle.WrapMode = DataGridViewTriState.True; // Включаем перенос текста
            dataGridViewEmotions.Columns.Add(emotion2probColumn);

            DataGridViewTextBoxColumn emotionColumn = new DataGridViewTextBoxColumn();
            emotionColumn.DataPropertyName = "Emotion";
            emotionColumn.HeaderText = "Эмоция";
            emotionColumn.ReadOnly = true;
            emotionColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells; //  Автоматическое изменение размера по содержимому
            dataGridViewEmotions.Columns.Add(emotionColumn);

            DataGridViewCheckBoxColumn matchColumn = new DataGridViewCheckBoxColumn();
            matchColumn.DataPropertyName = "IsSelected";
            matchColumn.HeaderText = "Соответствие";
            matchColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells; //  Автоматическое изменение размера по содержимому
            dataGridViewEmotions.Columns.Add(matchColumn);

            DataGridViewComboBoxColumn correctEmotionColumn = new DataGridViewComboBoxColumn();
            correctEmotionColumn.DataPropertyName = "CorrectEmotion";
            correctEmotionColumn.HeaderText = "Верная эмоция";
            correctEmotionColumn.DataSource = availableEmotions; // Задаем список доступных эмоций
            correctEmotionColumn.DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox;  //  Стиль отображения
            correctEmotionColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells; //  Автоматическое изменение размера по содержимому
            dataGridViewEmotions.Columns.Add(correctEmotionColumn);

            dataGridViewEmotions.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells; // Автоматическое изменение высоты строк
        }

        private void dataGridViewEmotions_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowIndex = e.RowIndex;
            int columnIndex = e.ColumnIndex;

            if (rowIndex >= 0 && columnIndex == 4)
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
                LoadAudio(audioFilePath);
            }
        }
        private void LoadAudio(string filePath)
        {
            try
            {
                StopAudio();  // Сначала останавливаем, если что-то уже играет
                waveOut = new WaveOutEvent();
                audioFile = new AudioFileReader(filePath);
                waveOut.Init(audioFile);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке аудио: " + ex.Message);
            }
        }
        
        private async void buttonRecognize_Click(object sender, EventArgs e)
        {
            emotionDataList.Clear();
            UpdateDataGridView();

            if (string.IsNullOrEmpty(audioFilePath))
            {
                MessageBox.Show("Пожалуйста, выберите аудиофайл.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Показываем, что процесс начался
            pictureBoxStatus.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxStatus.Image = Properties.Resources.processing;

            await System.Threading.Tasks.Task.Delay(10);

            try
            {

                // Передаем путь к audioFilePath как аргумент в RunPythonScript
                List<EmotionSegment> emotionSegments = RunPythonScript(pythonScriptPath, audioFilePath, outputDirectory);

                if (emotionSegments == null || emotionSegments.Count == 0)
                {
                    pictureBoxStatus.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBoxStatus.Image = Properties.Resources.error;
                    MessageBox.Show("Ошибка: Не удалось распознать эмоции.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                foreach (var segment in emotionSegments)
                {                    
                    string emotion = ConvertEmotion(segment.emotion);

                    string emotion2prob = ConvertEmotionProbabilitiesToString(segment.emotion2prob);

                    // Добавляем данные об эмоциях.
                    emotionDataList.Add(new EmotionData
                    {
                        Time_start = segment.StartTime,
                        Time_end = segment.EndTime,
                        Emotion2prob = emotion2prob, 
                        Emotion = emotion,
                        IsSelected = false,
                        CorrectEmotion = ""
                    });

                }
                UpdateDataGridView();

                pictureBoxStatus.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBoxStatus.Image = Properties.Resources.success;
                panelEmotions.Visible = true;

            }
            catch (Exception ex)
            {
                pictureBoxStatus.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBoxStatus.Image = Properties.Resources.error;
                MessageBox.Show($"Ошибка при распознавании: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        public static List<EmotionSegment> RunPythonScript(string scriptPath, string audioFilePath, string outputDirectory)
        {
            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = "c:\\diplom\\GigaAM\\.venv\\Scripts\\python.exe", // Укажите свой путь к директории, в которой находится python.exe
                Arguments = $"\"{scriptPath}\" \"{audioFilePath}\" \"{outputDirectory}\"", // Аргументы: путь к скрипту, путь к аудиофайлу, путь к папке для сохранения сегментов аудиозаписи.
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            string result;

            using (Process process = new Process { StartInfo = start })
            {
                process.Start();
                using (StreamReader reader = new StreamReader(process.StandardOutput.BaseStream, Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                }
                process.WaitForExit();
            }
           
            try
            {
                // Используем Newtonsoft.Json для десериализации.
                return JsonConvert.DeserializeObject<List<EmotionSegment>>(result);
            }
            catch (JsonException ex)
            {
                // Обработка ошибок парсинга JSON
                MessageBox.Show($"Ошибка парсинга JSON: {ex.Message}. JSON: {result}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); // Добавлен вывод JSON для отладки
                return null;
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
                    StopAudio(); // Если уже играет, останавливаем
                    return;
                }

                if (waveOut == null || audioFile == null || audioFile.FileName != audioFilePath)
                {
                    StopAudio();
                    waveOut = new WaveOutEvent();
                    audioFile = new AudioFileReader(audioFilePath);
                    waveOut.Init(audioFile);
                }

                buttonPlay.Text = "Остановить";
                isPaused = !isPaused;
                waveOut.Play();
                waveOut.PlaybackStopped += WaveOut_PlaybackStopped;

                // Запускаем отдельный поток для обновления эмоций
                System.Threading.Tasks.Task.Run(() => PlayAudioWithEmotions(audioFile));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка воспроизведения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void WaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                panelEmotions.Controls.Clear();
            });
            StopAudio();
        }

        private void StopAudio()
        {
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.PlaybackStopped -= WaveOut_PlaybackStopped;
                waveOut.Dispose();
                waveOut = null;
            }

            if (audioFile != null)
            {
                audioFile.Position = 0; // Сбрасываем позицию
                audioFile.Dispose();
                audioFile = null;
            }

            isPaused = !isPaused;
            buttonPlay.Text = "Прослушать запись";
        }
        private void PlayAudioWithEmotions(AudioFileReader audioFileReader)
        {
            long currentPosition = 0;
            while (currentPosition < audioFileReader.Length)
            {
                if (isPaused)
                {
                    System.Threading.Thread.Sleep(50);  // Ждем, пока не будет снята пауза
                    continue;
                }

                double currentTimeInSeconds = (double)currentPosition / audioFileReader.WaveFormat.AverageBytesPerSecond;
                TimeSpan currentTimeSpan = TimeSpan.FromSeconds(currentTimeInSeconds);

                var currentEmotion = emotionDataList
                    .Where(x => x.Time_start <= currentTimeSpan && x.Time_end >= currentTimeSpan)
                    .FirstOrDefault();

                int rowIndexToHighlight = -1;
                if (currentEmotion != null)
                {
                    // Находим индекс строки в dataGridViewEmotions, соответствующий currentEmotion
                    rowIndexToHighlight = emotionDataList.IndexOf(emotionDataList.FirstOrDefault(x => x == currentEmotion));
                }

                this.Invoke((MethodInvoker)delegate
                {
                    panelEmotions.Controls.Clear();
                    PictureBox pb = new PictureBox();
                    pb.Dock = DockStyle.Fill;
                    pb.SizeMode = PictureBoxSizeMode.Zoom;
                    Image emotionImage = null;
                    switch (currentEmotion?.Emotion?.ToLower()) //  Безопасный доступ к свойству Emotion и использование null-condition оператора
                    {
                        case "позитив":
                            emotionImage = Properties.Resources.happy;
                            break;
                        case "грусть":
                            emotionImage = Properties.Resources.sad;
                            break;
                        case "злость":
                            emotionImage = Properties.Resources.angry;
                            break;
                        case "нейтральная":
                            emotionImage = Properties.Resources.neutral;
                            break;
                        default:
                            emotionImage = null;
                            break;
                    }
                    pb.Image = emotionImage;
                    panelEmotions.Controls.Add(pb);


                    // Выделение строки в DataGridView
                    if (rowIndexToHighlight >= 0 && rowIndexToHighlight < dataGridViewEmotions.Rows.Count)  // Проверка границ
                    {
                        dataGridViewEmotions.ClearSelection();
                        dataGridViewEmotions.Rows[rowIndexToHighlight].Selected = true;
                        dataGridViewEmotions.FirstDisplayedScrollingRowIndex = rowIndexToHighlight;  // Автоматическая прокрутка к текущей строке
                    }
                    else
                    {
                        dataGridViewEmotions.ClearSelection(); // Сброс выделения, если эмоция не найдена или индекс некорректен
                    }
                });

                System.Threading.Thread.Sleep(50); // Уменьшено для более плавной работы
                if (waveOut == null || waveOut.PlaybackState != PlaybackState.Playing || isPaused)
                    return;
                currentPosition = audioFileReader.Position;
           
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
        public class EmotionData
        {
            public TimeSpan Time_start { get; set; }
            public TimeSpan Time_end { get; set; }
            public string Emotion2prob { get; set; }
            public string Emotion { get; set; }
            public bool IsSelected { get; set; }
            public string CorrectEmotion { get; set; }

            public EmotionData()
            {
                // Инициализация CorrectEmotion пустой строкой по умолчанию
                CorrectEmotion = "";
            }
        }

        public class EmotionSegment
        {
            [JsonProperty("start_time")]
            public double start_time { get; set; }

            [JsonProperty("end_time")] 
            public double end_time { get; set; }

            [JsonProperty("emotion")]
            public string emotion { get; set; }

            [JsonProperty("emotion2prob")]
            public Dictionary<string, double> emotion2prob { get; set; }

            [JsonIgnore] 
            public TimeSpan StartTime => TimeSpan.FromSeconds(start_time);

            [JsonIgnore]
            public TimeSpan EndTime => TimeSpan.FromSeconds(end_time);
        }

        public static string ConvertEmotion(string englishEmotion)
        {
            if (string.IsNullOrEmpty(englishEmotion))
            {
                return "Не определено";
            }

            string lowerCaseEmotion = englishEmotion.ToLower(); // Преобразуем входную строку к нижнему регистру

            if (EmotionMap.ContainsKey(lowerCaseEmotion)) // Ищем в словаре по ключу в нижнем регистре
            {
                return EmotionMap[lowerCaseEmotion];
            }

            if (lowerCaseEmotion.Contains("neutral")) return EmotionMap["neutral"];
            if (lowerCaseEmotion.Contains("anger")) return EmotionMap["angry"];
            if (lowerCaseEmotion.Contains("sad")) return EmotionMap["sad"];
            if (lowerCaseEmotion.Contains("positive") || lowerCaseEmotion.Contains("happy")) return EmotionMap["positive"];

            return "Неизвестно";
        }

        private static string ConvertEmotionProbabilitiesToString(Dictionary<string, double> emotionProbabilities)
        {
            if (emotionProbabilities == null || emotionProbabilities.Count == 0)
            {
                return string.Empty;
            }

            var keyValuePairs = emotionProbabilities.Select(kvp => $"{ConvertEmotion(kvp.Key)}: {kvp.Value:F4}"); // F4 - формат для 4 знаков после запятой

            return string.Join(", ", keyValuePairs);
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