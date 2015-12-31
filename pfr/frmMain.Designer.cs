namespace pfr
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
            this.tbDate = new System.Windows.Forms.DateTimePicker();
            this.dgv = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.mnuFiles = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOdb = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuXXI = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSofit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPrint = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuProtokol = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCard = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOrders = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOptions = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BottomToolStripPanel
            // 
            this.BottomToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.BottomToolStripPanel.Name = "BottomToolStripPanel";
            this.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.BottomToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.BottomToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // TopToolStripPanel
            // 
            this.TopToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.TopToolStripPanel.Name = "TopToolStripPanel";
            this.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.TopToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.TopToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // RightToolStripPanel
            // 
            this.RightToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.RightToolStripPanel.Name = "RightToolStripPanel";
            this.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.RightToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.RightToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // LeftToolStripPanel
            // 
            this.LeftToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.LeftToolStripPanel.Name = "LeftToolStripPanel";
            this.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.LeftToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            this.LeftToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // ContentPanel
            // 
            this.ContentPanel.AutoScroll = true;
            this.ContentPanel.Size = new System.Drawing.Size(918, 507);
            // 
            // tbDate
            // 
            this.tbDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.tbDate.Location = new System.Drawing.Point(12, 28);
            this.tbDate.Name = "tbDate";
            this.tbDate.Size = new System.Drawing.Size(88, 20);
            this.tbDate.TabIndex = 3;
            this.tbDate.ValueChanged += new System.EventHandler(this.tbDate_ValueChanged);
            // 
            // dgv
            // 
            this.dgv.AllowUserToAddRows = false;
            this.dgv.AllowUserToDeleteRows = false;
            this.dgv.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4,
            this.Column5});
            this.dgv.Location = new System.Drawing.Point(12, 54);
            this.dgv.Name = "dgv";
            this.dgv.ReadOnly = true;
            this.dgv.Size = new System.Drawing.Size(894, 465);
            this.dgv.TabIndex = 4;
            // 
            // Column1
            // 
            this.Column1.DataPropertyName = "DateReg";
            this.Column1.HeaderText = "Дата загрузки";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // Column2
            // 
            this.Column2.DataPropertyName = "DOffice";
            this.Column2.HeaderText = "Подразделение";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            // 
            // Column3
            // 
            this.Column3.DataPropertyName = "Fio";
            this.Column3.HeaderText = "ФИО";
            this.Column3.Name = "Column3";
            this.Column3.ReadOnly = true;
            this.Column3.Width = 300;
            // 
            // Column4
            // 
            this.Column4.DataPropertyName = "Acc";
            this.Column4.HeaderText = "Счет";
            this.Column4.Name = "Column4";
            this.Column4.ReadOnly = true;
            this.Column4.Width = 150;
            // 
            // Column5
            // 
            this.Column5.DataPropertyName = "Sm";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            dataGridViewCellStyle2.Format = "N2";
            dataGridViewCellStyle2.NullValue = null;
            this.Column5.DefaultCellStyle = dataGridViewCellStyle2;
            this.Column5.HeaderText = "Сумма";
            this.Column5.Name = "Column5";
            this.Column5.ReadOnly = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFiles,
            this.mnuOdb,
            this.mnuPrint,
            this.mnuOptions});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(918, 24);
            this.menuStrip1.TabIndex = 5;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // mnuFiles
            // 
            this.mnuFiles.Name = "mnuFiles";
            this.mnuFiles.Size = new System.Drawing.Size(57, 20);
            this.mnuFiles.Text = "Файлы";
            this.mnuFiles.Click += new System.EventHandler(this.mnuFiles_Click);
            // 
            // mnuOdb
            // 
            this.mnuOdb.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuXXI,
            this.mnuSofit});
            this.mnuOdb.Name = "mnuOdb";
            this.mnuOdb.Size = new System.Drawing.Size(70, 20);
            this.mnuOdb.Text = "Выгрузка";
            // 
            // mnuXXI
            // 
            this.mnuXXI.Name = "mnuXXI";
            this.mnuXXI.Size = new System.Drawing.Size(285, 22);
            this.mnuXXI.Text = "Документы в XXI век";
            this.mnuXXI.Click += new System.EventHandler(this.mnuOdb_Click);
            // 
            // mnuSofit
            // 
            this.mnuSofit.Name = "mnuSofit";
            this.mnuSofit.Size = new System.Drawing.Size(285, 22);
            this.mnuSofit.Text = "Файлы для Софит-Эмитент";
            this.mnuSofit.Click += new System.EventHandler(this.mnuSofit_Click);
            // 
            // mnuPrint
            // 
            this.mnuPrint.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuProtokol,
            this.mnuCard,
            this.mnuOrders});
            this.mnuPrint.Name = "mnuPrint";
            this.mnuPrint.Size = new System.Drawing.Size(58, 20);
            this.mnuPrint.Text = "Печать";
            // 
            // mnuProtokol
            // 
            this.mnuProtokol.Name = "mnuProtokol";
            this.mnuProtokol.Size = new System.Drawing.Size(289, 22);
            this.mnuProtokol.Text = "Протокол получения пенс.документов";
            this.mnuProtokol.Click += new System.EventHandler(this.mnuProtokol_Click);
            // 
            // mnuCard
            // 
            this.mnuCard.Name = "mnuCard";
            this.mnuCard.Size = new System.Drawing.Size(289, 22);
            this.mnuCard.Text = "Пополнение карточных счетов";
            this.mnuCard.Click += new System.EventHandler(this.mnuCard_Click);
            // 
            // mnuOrders
            // 
            this.mnuOrders.Name = "mnuOrders";
            this.mnuOrders.Size = new System.Drawing.Size(289, 22);
            this.mnuOrders.Text = "Сводные банковские ордера";
            this.mnuOrders.Click += new System.EventHandler(this.mnuOrders_Click);
            // 
            // mnuOptions
            // 
            this.mnuOptions.Name = "mnuOptions";
            this.mnuOptions.Size = new System.Drawing.Size(79, 20);
            this.mnuOptions.Text = "Настройки";
            this.mnuOptions.Click += new System.EventHandler(this.mnuOptions_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(918, 531);
            this.Controls.Add(this.dgv);
            this.Controls.Add(this.tbDate);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmMain";
            this.Text = "Обмен с ПФР";
            ((System.ComponentModel.ISupportInitialize)(this.dgv)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
        private System.Windows.Forms.ToolStripPanel TopToolStripPanel;
        private System.Windows.Forms.ToolStripPanel RightToolStripPanel;
        private System.Windows.Forms.ToolStripPanel LeftToolStripPanel;
        private System.Windows.Forms.ToolStripContentPanel ContentPanel;
        private System.Windows.Forms.DateTimePicker tbDate;
        private System.Windows.Forms.DataGridView dgv;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuOdb;
        private System.Windows.Forms.ToolStripMenuItem mnuOptions;
        private System.Windows.Forms.ToolStripMenuItem mnuXXI;
        private System.Windows.Forms.ToolStripMenuItem mnuSofit;
        private System.Windows.Forms.ToolStripMenuItem mnuPrint;
        private System.Windows.Forms.ToolStripMenuItem mnuProtokol;
        private System.Windows.Forms.ToolStripMenuItem mnuCard;
        private System.Windows.Forms.ToolStripMenuItem mnuFiles;
        private System.Windows.Forms.ToolStripMenuItem mnuOrders;
    }
}

