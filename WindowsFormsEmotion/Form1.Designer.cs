namespace WindowsFormsEmotion
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelFilePath = new System.Windows.Forms.Label();
            this.textBoxFilePath = new System.Windows.Forms.TextBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.buttonRecognize = new System.Windows.Forms.Button();
            this.dataGridViewEmotions = new System.Windows.Forms.DataGridView();
            this.buttonPlay = new System.Windows.Forms.Button();
            this.panelEmotions = new System.Windows.Forms.Panel();
            this.buttonSave = new System.Windows.Forms.Button();
            this.pictureBoxStatus = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewEmotions)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStatus)).BeginInit();
            this.SuspendLayout();
            // 
            // labelFilePath
            // 
            this.labelFilePath.AutoSize = true;
            this.labelFilePath.Location = new System.Drawing.Point(0, 14);
            this.labelFilePath.Name = "labelFilePath";
            this.labelFilePath.Size = new System.Drawing.Size(98, 16);
            this.labelFilePath.TabIndex = 0;
            this.labelFilePath.Text = "Путь к файлу:";
            // 
            // textBoxFilePath
            // 
            this.textBoxFilePath.Location = new System.Drawing.Point(106, 12);
            this.textBoxFilePath.Name = "textBoxFilePath";
            this.textBoxFilePath.Size = new System.Drawing.Size(374, 22);
            this.textBoxFilePath.TabIndex = 1;
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Location = new System.Drawing.Point(486, 12);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(134, 30);
            this.buttonBrowse.TabIndex = 2;
            this.buttonBrowse.Text = "Выбрать файл";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // buttonRecognize
            // 
            this.buttonRecognize.Location = new System.Drawing.Point(628, 12);
            this.buttonRecognize.Name = "buttonRecognize";
            this.buttonRecognize.Size = new System.Drawing.Size(190, 30);
            this.buttonRecognize.TabIndex = 3;
            this.buttonRecognize.Text = "Распознать эмоции";
            this.buttonRecognize.UseVisualStyleBackColor = true;
            this.buttonRecognize.Click += new System.EventHandler(this.buttonRecognize_Click);
            // 
            // dataGridViewEmotions
            // 
            this.dataGridViewEmotions.AllowUserToAddRows = false;
            this.dataGridViewEmotions.AllowUserToDeleteRows = false;
            this.dataGridViewEmotions.AllowUserToOrderColumns = true;
            this.dataGridViewEmotions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewEmotions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewEmotions.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dataGridViewEmotions.Location = new System.Drawing.Point(12, 79);
            this.dataGridViewEmotions.Name = "dataGridViewEmotions";
            this.dataGridViewEmotions.ReadOnly = true;
            this.dataGridViewEmotions.RowHeadersWidth = 51;
            this.dataGridViewEmotions.RowTemplate.Height = 24;
            this.dataGridViewEmotions.Size = new System.Drawing.Size(803, 422);
            this.dataGridViewEmotions.TabIndex = 5;
            this.dataGridViewEmotions.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewEmotions_CellContentClick);
            // 
            // buttonPlay
            // 
            this.buttonPlay.Location = new System.Drawing.Point(629, 43);
            this.buttonPlay.Name = "buttonPlay";
            this.buttonPlay.Size = new System.Drawing.Size(189, 30);
            this.buttonPlay.TabIndex = 6;
            this.buttonPlay.Text = "Прослушать запись";
            this.buttonPlay.UseVisualStyleBackColor = true;
            this.buttonPlay.Click += new System.EventHandler(this.buttonPlay_Click);
            // 
            // panelEmotions
            // 
            this.panelEmotions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelEmotions.Location = new System.Drawing.Point(888, 14);
            this.panelEmotions.Name = "panelEmotions";
            this.panelEmotions.Size = new System.Drawing.Size(114, 104);
            this.panelEmotions.TabIndex = 7;
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(485, 43);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(134, 30);
            this.buttonSave.TabIndex = 8;
            this.buttonSave.Text = "Сохранить";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // pictureBoxStatus
            // 
            this.pictureBoxStatus.Location = new System.Drawing.Point(825, 14);
            this.pictureBoxStatus.Name = "pictureBoxStatus";
            this.pictureBoxStatus.Size = new System.Drawing.Size(35, 35);
            this.pictureBoxStatus.TabIndex = 4;
            this.pictureBoxStatus.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1066, 600);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.panelEmotions);
            this.Controls.Add(this.buttonPlay);
            this.Controls.Add(this.dataGridViewEmotions);
            this.Controls.Add(this.pictureBoxStatus);
            this.Controls.Add(this.buttonRecognize);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.textBoxFilePath);
            this.Controls.Add(this.labelFilePath);
            this.Name = "Form1";
            this.Text = "Распознование эмоций";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewEmotions)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxStatus)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelFilePath;
        private System.Windows.Forms.TextBox textBoxFilePath;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonRecognize;
        private System.Windows.Forms.PictureBox pictureBoxStatus;
        private System.Windows.Forms.DataGridView dataGridViewEmotions;
        private System.Windows.Forms.Button buttonPlay;
        private System.Windows.Forms.Panel panelEmotions;
    }
}

