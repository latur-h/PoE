using PoE.dlls.Style;

namespace PoE
{
    partial class Main
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
            components = new System.ComponentModel.Container();
            tabControl_Main = new FlatTabControl();
            tabPage_Main = new TabPage();
            label_Active = new Label();
            checkBox_Flask5 = new CheckBox();
            textBox_Flask1 = new FlatTextBox();
            checkBox_Flask4 = new CheckBox();
            checkBox_Flask3 = new CheckBox();
            checkBox_Flask2 = new CheckBox();
            checkBox_Flask1 = new CheckBox();
            label_Key = new Label();
            textBox_Flask5 = new FlatTextBox();
            textBox_Flask4 = new FlatTextBox();
            textBox_Flask3 = new FlatTextBox();
            textBox_Flask2 = new FlatTextBox();
            groupBox_Flask5 = new FlatGroupBox();
            slider_Flask5 = new Slider();
            label_Flask5_Slider = new Label();
            groupBox_Flask4 = new FlatGroupBox();
            slider_Flask4 = new Slider();
            label_Flask4_Slider = new Label();
            groupBox_Flask3 = new FlatGroupBox();
            slider_Flask3 = new Slider();
            label_Flask3_Slider = new Label();
            groupBox_Flask2 = new FlatGroupBox();
            slider_Flask2 = new Slider();
            label_Flask2_Slider = new Label();
            groupBox_Flask1 = new FlatGroupBox();
            slider_Flask1 = new Slider();
            label_Flask1_Slider = new Label();
            comboBox_Flask5 = new FlatComboBox();
            comboBox_Flask4 = new FlatComboBox();
            comboBox_Flask3 = new FlatComboBox();
            comboBox_Flask2 = new FlatComboBox();
            label_Percent = new Label();
            label_FlaskType = new Label();
            comboBox_Flask1 = new FlatComboBox();
            tabPage_Gamble = new TabPage();
            label_GambleType = new Label();
            comboBox_GambleType = new FlatComboBox();
            tabPage_Orbs = new TabPage();
            tabPage_Settings = new TabPage();
            groupBox_FlaskSettings = new FlatGroupBox();
            label_FlaskTinctureCooldown = new Label();
            textBox_FlaskTinctureCooldown = new FlatTextBox();
            label_FlaskUtilityCooldown = new Label();
            textBox_FlaskUtilityCooldown = new FlatTextBox();
            label_FlaskHpMpCooldown = new Label();
            textBox_FlaskHpMpCooldown = new FlatTextBox();
            label_FlaskKeyPressDelay = new Label();
            textBox_FlaskKeyPressDelay = new FlatTextBox();
            label_FlaskDelay = new Label();
            textBox_FlaskDelay = new FlatTextBox();
            label_FlaskStopKey = new Label();
            label_FlaskDrinkKey = new Label();
            label_FlaskRegisterKey = new Label();
            textBox_FlaskStopKey = new FlatTextBox();
            textBox_FlaskDrinkKey = new FlatTextBox();
            textBox_FlaskRegisterKey = new FlatTextBox();
            groupBox_GambleSettings = new FlatGroupBox();
            label_GambleSpeed = new Label();
            textBox_GambleSpeed = new FlatTextBox();
            label_GamblerDelay = new Label();
            textBox_GamblerDelay = new FlatTextBox();
            label_GamblerStopKey = new Label();
            label_GamblerStartKey = new Label();
            label_GamblerGetCoorinatesKey = new Label();
            textBox_GamblerStopKey = new FlatTextBox();
            textBox_GamblerStartKey = new FlatTextBox();
            textBox_GamblerGetCoordinatesKey = new FlatTextBox();
            tabPage_Logs = new TabPage();
            toolTip_Settings = new ToolTip(components);
            tabControl_Main.SuspendLayout();
            tabPage_Main.SuspendLayout();
            groupBox_Flask5.SuspendLayout();
            groupBox_Flask4.SuspendLayout();
            groupBox_Flask3.SuspendLayout();
            groupBox_Flask2.SuspendLayout();
            groupBox_Flask1.SuspendLayout();
            tabPage_Gamble.SuspendLayout();
            tabPage_Settings.SuspendLayout();
            groupBox_FlaskSettings.SuspendLayout();
            groupBox_GambleSettings.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl_Main
            // 
            tabControl_Main.Controls.Add(tabPage_Main);
            tabControl_Main.Controls.Add(tabPage_Gamble);
            tabControl_Main.Controls.Add(tabPage_Orbs);
            tabControl_Main.Controls.Add(tabPage_Settings);
            tabControl_Main.Controls.Add(tabPage_Logs);
            tabControl_Main.Dock = DockStyle.Fill;
            tabControl_Main.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabControl_Main.ItemSize = new Size(100, 32);
            tabControl_Main.Location = new Point(0, 0);
            tabControl_Main.Name = "tabControl_Main";
            tabControl_Main.SelectedIndex = 0;
            tabControl_Main.Size = new Size(744, 451);
            tabControl_Main.SizeMode = TabSizeMode.Fixed;
            tabControl_Main.TabIndex = 6;
            // 
            // tabPage_Main
            // 
            tabPage_Main.Controls.Add(label_Active);
            tabPage_Main.Controls.Add(label_Key);
            tabPage_Main.Controls.Add(label_Percent);
            tabPage_Main.Controls.Add(label_FlaskType);
            tabPage_Main.Controls.Add(groupBox_Flask5);
            tabPage_Main.Controls.Add(groupBox_Flask4);
            tabPage_Main.Controls.Add(groupBox_Flask3);
            tabPage_Main.Controls.Add(groupBox_Flask2);
            tabPage_Main.Controls.Add(groupBox_Flask1);
            tabPage_Main.Location = new Point(4, 36);
            tabPage_Main.Name = "tabPage_Main";
            tabPage_Main.Padding = new Padding(3);
            tabPage_Main.Size = new Size(736, 411);
            tabPage_Main.TabIndex = 0;
            tabPage_Main.Text = "Main";
            tabPage_Main.UseVisualStyleBackColor = true;
            // 
            // label_Active
            // 
            label_Active.AutoSize = true;
            label_Active.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_Active.Location = new Point(9, 23);
            label_Active.Name = "label_Active";
            label_Active.Size = new Size(52, 21);
            label_Active.TabIndex = 41;
            label_Active.Text = "Active";
            // 
            // checkBox_Flask5
            // 
            checkBox_Flask5.AutoSize = true;
            checkBox_Flask5.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            checkBox_Flask5.Location = new Point(36, 22);
            checkBox_Flask5.Name = "checkBox_Flask5";
            checkBox_Flask5.Size = new Size(15, 25);
            checkBox_Flask5.TabIndex = 40;
            checkBox_Flask5.UseVisualStyleBackColor = true;
            // 
            // textBox_Flask1
            // 
            textBox_Flask1.BackColor = Color.Transparent;
            textBox_Flask1.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox_Flask1.Location = new Point(6, 56);
            textBox_Flask1.Margin = new Padding(0);
            textBox_Flask1.Name = "textBox_Flask1";
            textBox_Flask1.Size = new Size(78, 30);
            textBox_Flask1.TabIndex = 30;
            textBox_Flask1.TextAlign = HorizontalAlignment.Center;
            // 
            // checkBox_Flask4
            // 
            checkBox_Flask4.AutoSize = true;
            checkBox_Flask4.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            checkBox_Flask4.Location = new Point(36, 22);
            checkBox_Flask4.Name = "checkBox_Flask4";
            checkBox_Flask4.Size = new Size(15, 25);
            checkBox_Flask4.TabIndex = 39;
            checkBox_Flask4.UseVisualStyleBackColor = true;
            // 
            // checkBox_Flask3
            // 
            checkBox_Flask3.AutoSize = true;
            checkBox_Flask3.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            checkBox_Flask3.Location = new Point(36, 22);
            checkBox_Flask3.Name = "checkBox_Flask3";
            checkBox_Flask3.Size = new Size(15, 25);
            checkBox_Flask3.TabIndex = 38;
            checkBox_Flask3.UseVisualStyleBackColor = true;
            // 
            // checkBox_Flask2
            // 
            checkBox_Flask2.AutoSize = true;
            checkBox_Flask2.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            checkBox_Flask2.Location = new Point(36, 22);
            checkBox_Flask2.Name = "checkBox_Flask2";
            checkBox_Flask2.Size = new Size(15, 25);
            checkBox_Flask2.TabIndex = 37;
            checkBox_Flask2.UseVisualStyleBackColor = true;
            // 
            // checkBox_Flask1
            // 
            checkBox_Flask1.AutoSize = true;
            checkBox_Flask1.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            checkBox_Flask1.Location = new Point(36, 22);
            checkBox_Flask1.Name = "checkBox_Flask1";
            checkBox_Flask1.Size = new Size(15, 25);
            checkBox_Flask1.TabIndex = 36;
            checkBox_Flask1.UseVisualStyleBackColor = true;
            // 
            // label_Key
            // 
            label_Key.AutoSize = true;
            label_Key.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_Key.Location = new Point(9, 58);
            label_Key.Name = "label_Key";
            label_Key.Size = new Size(35, 21);
            label_Key.TabIndex = 35;
            label_Key.Text = "Key";
            // 
            // textBox_Flask5
            // 
            textBox_Flask5.BackColor = Color.Transparent;
            textBox_Flask5.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox_Flask5.Location = new Point(6, 56);
            textBox_Flask5.Name = "textBox_Flask5";
            textBox_Flask5.Size = new Size(78, 30);
            textBox_Flask5.TabIndex = 34;
            textBox_Flask5.TextAlign = HorizontalAlignment.Center;
            // 
            // textBox_Flask4
            // 
            textBox_Flask4.BackColor = Color.Transparent;
            textBox_Flask4.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox_Flask4.Location = new Point(6, 56);
            textBox_Flask4.Name = "textBox_Flask4";
            textBox_Flask4.Size = new Size(78, 30);
            textBox_Flask4.TabIndex = 33;
            textBox_Flask4.TextAlign = HorizontalAlignment.Center;
            // 
            // textBox_Flask3
            // 
            textBox_Flask3.BackColor = Color.Transparent;
            textBox_Flask3.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox_Flask3.Location = new Point(6, 56);
            textBox_Flask3.Name = "textBox_Flask3";
            textBox_Flask3.Size = new Size(78, 30);
            textBox_Flask3.TabIndex = 32;
            textBox_Flask3.TextAlign = HorizontalAlignment.Center;
            // 
            // textBox_Flask2
            // 
            textBox_Flask2.BackColor = Color.Transparent;
            textBox_Flask2.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox_Flask2.Location = new Point(6, 56);
            textBox_Flask2.Name = "textBox_Flask2";
            textBox_Flask2.Size = new Size(78, 30);
            textBox_Flask2.TabIndex = 31;
            textBox_Flask2.TextAlign = HorizontalAlignment.Center;
            // 
            // groupBox_Flask5
            // 
            groupBox_Flask5.Controls.Add(checkBox_Flask5);
            groupBox_Flask5.Controls.Add(textBox_Flask5);
            groupBox_Flask5.Controls.Add(comboBox_Flask5);
            groupBox_Flask5.Controls.Add(slider_Flask5);
            groupBox_Flask5.Controls.Add(label_Flask5_Slider);
            groupBox_Flask5.Location = new Point(477, 8);
            groupBox_Flask5.Name = "groupBox_Flask5";
            groupBox_Flask5.Size = new Size(90, 380);
            groupBox_Flask5.TabIndex = 14;
            groupBox_Flask5.TabStop = false;
            groupBox_Flask5.Text = "5";
            // 
            // slider_Flask5
            // 
            slider_Flask5.Location = new Point(29, 132);
            slider_Flask5.Name = "slider_Flask5";
            slider_Flask5.Size = new Size(32, 200);
            slider_Flask5.TabIndex = 24;
            // 
            // label_Flask5_Slider
            // 
            label_Flask5_Slider.AutoSize = true;
            label_Flask5_Slider.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_Flask5_Slider.Location = new Point(35, 338);
            label_Flask5_Slider.Name = "label_Flask5_Slider";
            label_Flask5_Slider.Size = new Size(19, 21);
            label_Flask5_Slider.TabIndex = 25;
            label_Flask5_Slider.Text = "0";
            // 
            // groupBox_Flask4
            // 
            groupBox_Flask4.Controls.Add(checkBox_Flask4);
            groupBox_Flask4.Controls.Add(textBox_Flask4);
            groupBox_Flask4.Controls.Add(comboBox_Flask4);
            groupBox_Flask4.Controls.Add(slider_Flask4);
            groupBox_Flask4.Controls.Add(label_Flask4_Slider);
            groupBox_Flask4.Location = new Point(382, 8);
            groupBox_Flask4.Name = "groupBox_Flask4";
            groupBox_Flask4.Size = new Size(90, 380);
            groupBox_Flask4.TabIndex = 29;
            groupBox_Flask4.TabStop = false;
            groupBox_Flask4.Text = "4";
            // 
            // slider_Flask4
            // 
            slider_Flask4.Location = new Point(29, 132);
            slider_Flask4.Name = "slider_Flask4";
            slider_Flask4.Size = new Size(32, 200);
            slider_Flask4.TabIndex = 20;
            // 
            // label_Flask4_Slider
            // 
            label_Flask4_Slider.AutoSize = true;
            label_Flask4_Slider.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_Flask4_Slider.Location = new Point(35, 338);
            label_Flask4_Slider.Name = "label_Flask4_Slider";
            label_Flask4_Slider.Size = new Size(19, 21);
            label_Flask4_Slider.TabIndex = 21;
            label_Flask4_Slider.Text = "0";
            // 
            // groupBox_Flask3
            // 
            groupBox_Flask3.Controls.Add(checkBox_Flask3);
            groupBox_Flask3.Controls.Add(textBox_Flask3);
            groupBox_Flask3.Controls.Add(comboBox_Flask3);
            groupBox_Flask3.Controls.Add(slider_Flask3);
            groupBox_Flask3.Controls.Add(label_Flask3_Slider);
            groupBox_Flask3.Location = new Point(287, 8);
            groupBox_Flask3.Name = "groupBox_Flask3";
            groupBox_Flask3.Size = new Size(90, 380);
            groupBox_Flask3.TabIndex = 28;
            groupBox_Flask3.TabStop = false;
            groupBox_Flask3.Text = "3";
            // 
            // slider_Flask3
            // 
            slider_Flask3.Location = new Point(29, 132);
            slider_Flask3.Name = "slider_Flask3";
            slider_Flask3.Size = new Size(32, 200);
            slider_Flask3.TabIndex = 16;
            // 
            // label_Flask3_Slider
            // 
            label_Flask3_Slider.AutoSize = true;
            label_Flask3_Slider.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_Flask3_Slider.Location = new Point(35, 338);
            label_Flask3_Slider.Name = "label_Flask3_Slider";
            label_Flask3_Slider.Size = new Size(19, 21);
            label_Flask3_Slider.TabIndex = 17;
            label_Flask3_Slider.Text = "0";
            // 
            // groupBox_Flask2
            // 
            groupBox_Flask2.Controls.Add(checkBox_Flask2);
            groupBox_Flask2.Controls.Add(textBox_Flask2);
            groupBox_Flask2.Controls.Add(comboBox_Flask2);
            groupBox_Flask2.Controls.Add(slider_Flask2);
            groupBox_Flask2.Controls.Add(label_Flask2_Slider);
            groupBox_Flask2.Location = new Point(191, 8);
            groupBox_Flask2.Name = "groupBox_Flask2";
            groupBox_Flask2.Size = new Size(90, 380);
            groupBox_Flask2.TabIndex = 27;
            groupBox_Flask2.TabStop = false;
            groupBox_Flask2.Text = "2";
            // 
            // slider_Flask2
            // 
            slider_Flask2.Location = new Point(29, 132);
            slider_Flask2.Name = "slider_Flask2";
            slider_Flask2.Size = new Size(32, 200);
            slider_Flask2.TabIndex = 12;
            // 
            // label_Flask2_Slider
            // 
            label_Flask2_Slider.AutoSize = true;
            label_Flask2_Slider.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_Flask2_Slider.Location = new Point(35, 338);
            label_Flask2_Slider.Name = "label_Flask2_Slider";
            label_Flask2_Slider.Size = new Size(19, 21);
            label_Flask2_Slider.TabIndex = 13;
            label_Flask2_Slider.Text = "0";
            // 
            // groupBox_Flask1
            // 
            groupBox_Flask1.Controls.Add(checkBox_Flask1);
            groupBox_Flask1.Controls.Add(textBox_Flask1);
            groupBox_Flask1.Controls.Add(comboBox_Flask1);
            groupBox_Flask1.Controls.Add(slider_Flask1);
            groupBox_Flask1.Controls.Add(label_Flask1_Slider);
            groupBox_Flask1.Location = new Point(92, 8);
            groupBox_Flask1.Name = "groupBox_Flask1";
            groupBox_Flask1.Size = new Size(90, 380);
            groupBox_Flask1.TabIndex = 26;
            groupBox_Flask1.TabStop = false;
            groupBox_Flask1.Text = "1";
            // 
            // slider_Flask1
            // 
            slider_Flask1.Location = new Point(29, 132);
            slider_Flask1.Name = "slider_Flask1";
            slider_Flask1.Size = new Size(32, 200);
            slider_Flask1.TabIndex = 8;
            // 
            // label_Flask1_Slider
            // 
            label_Flask1_Slider.AutoSize = true;
            label_Flask1_Slider.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_Flask1_Slider.Location = new Point(35, 338);
            label_Flask1_Slider.Name = "label_Flask1_Slider";
            label_Flask1_Slider.Size = new Size(19, 21);
            label_Flask1_Slider.TabIndex = 9;
            label_Flask1_Slider.Text = "0";
            // 
            // comboBox_Flask5
            // 
            comboBox_Flask5.DrawMode = DrawMode.OwnerDrawFixed;
            comboBox_Flask5.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_Flask5.FlatStyle = FlatStyle.Flat;
            comboBox_Flask5.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            comboBox_Flask5.FormattingEnabled = true;
            comboBox_Flask5.Location = new Point(6, 94);
            comboBox_Flask5.Name = "comboBox_Flask5";
            comboBox_Flask5.Size = new Size(78, 30);
            comboBox_Flask5.TabIndex = 22;
            // 
            // comboBox_Flask4
            // 
            comboBox_Flask4.DrawMode = DrawMode.OwnerDrawFixed;
            comboBox_Flask4.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_Flask4.FlatStyle = FlatStyle.Flat;
            comboBox_Flask4.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            comboBox_Flask4.FormattingEnabled = true;
            comboBox_Flask4.Location = new Point(6, 94);
            comboBox_Flask4.Name = "comboBox_Flask4";
            comboBox_Flask4.Size = new Size(78, 30);
            comboBox_Flask4.TabIndex = 18;
            // 
            // comboBox_Flask3
            // 
            comboBox_Flask3.DrawMode = DrawMode.OwnerDrawFixed;
            comboBox_Flask3.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_Flask3.FlatStyle = FlatStyle.Flat;
            comboBox_Flask3.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            comboBox_Flask3.FormattingEnabled = true;
            comboBox_Flask3.Location = new Point(6, 94);
            comboBox_Flask3.Name = "comboBox_Flask3";
            comboBox_Flask3.Size = new Size(78, 30);
            comboBox_Flask3.TabIndex = 14;
            // 
            // comboBox_Flask2
            // 
            comboBox_Flask2.DrawMode = DrawMode.OwnerDrawFixed;
            comboBox_Flask2.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_Flask2.FlatStyle = FlatStyle.Flat;
            comboBox_Flask2.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            comboBox_Flask2.FormattingEnabled = true;
            comboBox_Flask2.Location = new Point(6, 94);
            comboBox_Flask2.Name = "comboBox_Flask2";
            comboBox_Flask2.Size = new Size(78, 30);
            comboBox_Flask2.TabIndex = 10;
            // 
            // label_Percent
            // 
            label_Percent.AutoSize = true;
            label_Percent.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_Percent.Location = new Point(9, 146);
            label_Percent.Name = "label_Percent";
            label_Percent.Size = new Size(61, 21);
            label_Percent.TabIndex = 3;
            label_Percent.Text = "Percent";
            // 
            // label_FlaskType
            // 
            label_FlaskType.AutoSize = true;
            label_FlaskType.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_FlaskType.Location = new Point(9, 101);
            label_FlaskType.Name = "label_FlaskType";
            label_FlaskType.Size = new Size(79, 21);
            label_FlaskType.TabIndex = 1;
            label_FlaskType.Text = "Flask type";
            // 
            // comboBox_Flask1
            // 
            comboBox_Flask1.DrawMode = DrawMode.OwnerDrawFixed;
            comboBox_Flask1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_Flask1.FlatStyle = FlatStyle.Flat;
            comboBox_Flask1.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            comboBox_Flask1.FormattingEnabled = true;
            comboBox_Flask1.Location = new Point(6, 94);
            comboBox_Flask1.Name = "comboBox_Flask1";
            comboBox_Flask1.Size = new Size(78, 30);
            comboBox_Flask1.TabIndex = 0;
            // 
            // tabPage_Gamble
            // 
            tabPage_Gamble.Controls.Add(label_GambleType);
            tabPage_Gamble.Controls.Add(comboBox_GambleType);
            tabPage_Gamble.Location = new Point(4, 36);
            tabPage_Gamble.Name = "tabPage_Gamble";
            tabPage_Gamble.Size = new Size(736, 411);
            tabPage_Gamble.TabIndex = 3;
            tabPage_Gamble.Text = "Gamble";
            tabPage_Gamble.UseVisualStyleBackColor = true;
            // 
            // label_GambleType
            // 
            label_GambleType.AutoSize = true;
            label_GambleType.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_GambleType.Location = new Point(7, 10);
            label_GambleType.Name = "label_GambleType";
            label_GambleType.Size = new Size(104, 21);
            label_GambleType.TabIndex = 40;
            label_GambleType.Text = "Gamble type:";
            // 
            // comboBox_GambleType
            // 
            comboBox_GambleType.DrawMode = DrawMode.OwnerDrawFixed;
            comboBox_GambleType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_GambleType.FlatStyle = FlatStyle.Flat;
            comboBox_GambleType.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            comboBox_GambleType.FormattingEnabled = true;
            comboBox_GambleType.Location = new Point(119, 7);
            comboBox_GambleType.Name = "comboBox_GambleType";
            comboBox_GambleType.Size = new Size(180, 30);
            comboBox_GambleType.TabIndex = 0;
            // 
            // tabPage_Orbs
            // 
            tabPage_Orbs.Location = new Point(4, 36);
            tabPage_Orbs.Name = "tabPage_Orbs";
            tabPage_Orbs.Size = new Size(736, 411);
            tabPage_Orbs.TabIndex = 4;
            tabPage_Orbs.Text = "Orbs";
            tabPage_Orbs.UseVisualStyleBackColor = true;
            // 
            // tabPage_Settings
            // 
            tabPage_Settings.Controls.Add(groupBox_FlaskSettings);
            tabPage_Settings.Controls.Add(groupBox_GambleSettings);
            tabPage_Settings.Location = new Point(4, 36);
            tabPage_Settings.Name = "tabPage_Settings";
            tabPage_Settings.Padding = new Padding(3);
            tabPage_Settings.Size = new Size(732, 397);
            tabPage_Settings.TabIndex = 1;
            tabPage_Settings.Text = "Settings";
            tabPage_Settings.UseVisualStyleBackColor = true;
            // 
            // groupBox_FlaskSettings
            // 
            groupBox_FlaskSettings.Controls.Add(label_FlaskTinctureCooldown);
            groupBox_FlaskSettings.Controls.Add(textBox_FlaskTinctureCooldown);
            groupBox_FlaskSettings.Controls.Add(label_FlaskUtilityCooldown);
            groupBox_FlaskSettings.Controls.Add(textBox_FlaskUtilityCooldown);
            groupBox_FlaskSettings.Controls.Add(label_FlaskHpMpCooldown);
            groupBox_FlaskSettings.Controls.Add(textBox_FlaskHpMpCooldown);
            groupBox_FlaskSettings.Controls.Add(label_FlaskKeyPressDelay);
            groupBox_FlaskSettings.Controls.Add(textBox_FlaskKeyPressDelay);
            groupBox_FlaskSettings.Controls.Add(label_FlaskDelay);
            groupBox_FlaskSettings.Controls.Add(textBox_FlaskDelay);
            groupBox_FlaskSettings.Controls.Add(label_FlaskStopKey);
            groupBox_FlaskSettings.Controls.Add(label_FlaskDrinkKey);
            groupBox_FlaskSettings.Controls.Add(label_FlaskRegisterKey);
            groupBox_FlaskSettings.Controls.Add(textBox_FlaskStopKey);
            groupBox_FlaskSettings.Controls.Add(textBox_FlaskDrinkKey);
            groupBox_FlaskSettings.Controls.Add(textBox_FlaskRegisterKey);
            groupBox_FlaskSettings.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            groupBox_FlaskSettings.Location = new Point(7, 175);
            groupBox_FlaskSettings.Name = "groupBox_FlaskSettings";
            groupBox_FlaskSettings.Size = new Size(603, 158);
            groupBox_FlaskSettings.TabIndex = 59;
            groupBox_FlaskSettings.TabStop = false;
            groupBox_FlaskSettings.Text = "Flasks";
            // 
            // label_FlaskTinctureCooldown
            // 
            label_FlaskTinctureCooldown.AutoSize = true;
            label_FlaskTinctureCooldown.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_FlaskTinctureCooldown.Location = new Point(452, 78);
            label_FlaskTinctureCooldown.Name = "label_FlaskTinctureCooldown";
            label_FlaskTinctureCooldown.Size = new Size(66, 21);
            label_FlaskTinctureCooldown.TabIndex = 15;
            label_FlaskTinctureCooldown.Text = "Tincture";
            // 
            // textBox_FlaskTinctureCooldown
            // 
            textBox_FlaskTinctureCooldown.BackColor = Color.Transparent;
            textBox_FlaskTinctureCooldown.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox_FlaskTinctureCooldown.Location = new Point(452, 102);
            textBox_FlaskTinctureCooldown.Name = "textBox_FlaskTinctureCooldown";
            textBox_FlaskTinctureCooldown.Size = new Size(85, 30);
            textBox_FlaskTinctureCooldown.TabIndex = 14;
            textBox_FlaskTinctureCooldown.TextAlign = HorizontalAlignment.Center;
            // 
            // label_FlaskUtilityCooldown
            // 
            label_FlaskUtilityCooldown.AutoSize = true;
            label_FlaskUtilityCooldown.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_FlaskUtilityCooldown.Location = new Point(338, 78);
            label_FlaskUtilityCooldown.Name = "label_FlaskUtilityCooldown";
            label_FlaskUtilityCooldown.Size = new Size(51, 21);
            label_FlaskUtilityCooldown.TabIndex = 13;
            label_FlaskUtilityCooldown.Text = "Utility";
            // 
            // textBox_FlaskUtilityCooldown
            // 
            textBox_FlaskUtilityCooldown.BackColor = Color.Transparent;
            textBox_FlaskUtilityCooldown.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox_FlaskUtilityCooldown.Location = new Point(338, 102);
            textBox_FlaskUtilityCooldown.Name = "textBox_FlaskUtilityCooldown";
            textBox_FlaskUtilityCooldown.Size = new Size(85, 30);
            textBox_FlaskUtilityCooldown.TabIndex = 12;
            textBox_FlaskUtilityCooldown.TextAlign = HorizontalAlignment.Center;
            // 
            // label_FlaskHpMpCooldown
            // 
            label_FlaskHpMpCooldown.AutoSize = true;
            label_FlaskHpMpCooldown.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_FlaskHpMpCooldown.Location = new Point(224, 78);
            label_FlaskHpMpCooldown.Name = "label_FlaskHpMpCooldown";
            label_FlaskHpMpCooldown.Size = new Size(59, 21);
            label_FlaskHpMpCooldown.TabIndex = 11;
            label_FlaskHpMpCooldown.Text = "HP/MP";
            // 
            // textBox_FlaskHpMpCooldown
            // 
            textBox_FlaskHpMpCooldown.BackColor = Color.Transparent;
            textBox_FlaskHpMpCooldown.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox_FlaskHpMpCooldown.Location = new Point(224, 102);
            textBox_FlaskHpMpCooldown.Name = "textBox_FlaskHpMpCooldown";
            textBox_FlaskHpMpCooldown.Size = new Size(85, 30);
            textBox_FlaskHpMpCooldown.TabIndex = 10;
            textBox_FlaskHpMpCooldown.TextAlign = HorizontalAlignment.Center;
            // 
            // label_FlaskKeyPressDelay
            // 
            label_FlaskKeyPressDelay.AutoSize = true;
            label_FlaskKeyPressDelay.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_FlaskKeyPressDelay.Location = new Point(110, 78);
            label_FlaskKeyPressDelay.Name = "label_FlaskKeyPressDelay";
            label_FlaskKeyPressDelay.Size = new Size(35, 21);
            label_FlaskKeyPressDelay.TabIndex = 9;
            label_FlaskKeyPressDelay.Text = "Key";
            // 
            // textBox_FlaskKeyPressDelay
            // 
            textBox_FlaskKeyPressDelay.BackColor = Color.Transparent;
            textBox_FlaskKeyPressDelay.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox_FlaskKeyPressDelay.Location = new Point(110, 102);
            textBox_FlaskKeyPressDelay.Name = "textBox_FlaskKeyPressDelay";
            textBox_FlaskKeyPressDelay.Size = new Size(85, 30);
            textBox_FlaskKeyPressDelay.TabIndex = 8;
            textBox_FlaskKeyPressDelay.TextAlign = HorizontalAlignment.Center;
            // 
            // label_FlaskDelay
            // 
            label_FlaskDelay.AutoSize = true;
            label_FlaskDelay.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_FlaskDelay.Location = new Point(452, 22);
            label_FlaskDelay.Name = "label_FlaskDelay";
            label_FlaskDelay.Size = new Size(35, 21);
            label_FlaskDelay.TabIndex = 7;
            label_FlaskDelay.Text = "Poll";
            // 
            // textBox_FlaskDelay
            // 
            textBox_FlaskDelay.BackColor = Color.Transparent;
            textBox_FlaskDelay.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox_FlaskDelay.Location = new Point(452, 46);
            textBox_FlaskDelay.Name = "textBox_FlaskDelay";
            textBox_FlaskDelay.Size = new Size(85, 30);
            textBox_FlaskDelay.TabIndex = 6;
            textBox_FlaskDelay.TextAlign = HorizontalAlignment.Center;
            // 
            // label_FlaskStopKey
            // 
            label_FlaskStopKey.AutoSize = true;
            label_FlaskStopKey.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_FlaskStopKey.Location = new Point(338, 22);
            label_FlaskStopKey.Name = "label_FlaskStopKey";
            label_FlaskStopKey.Size = new Size(41, 21);
            label_FlaskStopKey.TabIndex = 5;
            label_FlaskStopKey.Text = "Stop";
            // 
            // label_FlaskDrinkKey
            // 
            label_FlaskDrinkKey.AutoSize = true;
            label_FlaskDrinkKey.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_FlaskDrinkKey.Location = new Point(224, 22);
            label_FlaskDrinkKey.Name = "label_FlaskDrinkKey";
            label_FlaskDrinkKey.Size = new Size(48, 21);
            label_FlaskDrinkKey.TabIndex = 4;
            label_FlaskDrinkKey.Text = "Drink";
            // 
            // label_FlaskRegisterKey
            // 
            label_FlaskRegisterKey.AutoSize = true;
            label_FlaskRegisterKey.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_FlaskRegisterKey.Location = new Point(110, 22);
            label_FlaskRegisterKey.Name = "label_FlaskRegisterKey";
            label_FlaskRegisterKey.Size = new Size(67, 21);
            label_FlaskRegisterKey.TabIndex = 3;
            label_FlaskRegisterKey.Text = "Register";
            // 
            // textBox_FlaskStopKey
            // 
            textBox_FlaskStopKey.BackColor = Color.Transparent;
            textBox_FlaskStopKey.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox_FlaskStopKey.Location = new Point(338, 46);
            textBox_FlaskStopKey.Name = "textBox_FlaskStopKey";
            textBox_FlaskStopKey.Size = new Size(85, 30);
            textBox_FlaskStopKey.TabIndex = 2;
            textBox_FlaskStopKey.TextAlign = HorizontalAlignment.Center;
            // 
            // textBox_FlaskDrinkKey
            // 
            textBox_FlaskDrinkKey.BackColor = Color.Transparent;
            textBox_FlaskDrinkKey.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox_FlaskDrinkKey.Location = new Point(224, 46);
            textBox_FlaskDrinkKey.Name = "textBox_FlaskDrinkKey";
            textBox_FlaskDrinkKey.Size = new Size(85, 30);
            textBox_FlaskDrinkKey.TabIndex = 1;
            textBox_FlaskDrinkKey.TextAlign = HorizontalAlignment.Center;
            // 
            // textBox_FlaskRegisterKey
            // 
            textBox_FlaskRegisterKey.BackColor = Color.Transparent;
            textBox_FlaskRegisterKey.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox_FlaskRegisterKey.Location = new Point(110, 46);
            textBox_FlaskRegisterKey.Name = "textBox_FlaskRegisterKey";
            textBox_FlaskRegisterKey.Size = new Size(85, 30);
            textBox_FlaskRegisterKey.TabIndex = 0;
            textBox_FlaskRegisterKey.TextAlign = HorizontalAlignment.Center;
            // 
            // groupBox_GambleSettings
            // 
            groupBox_GambleSettings.Controls.Add(label_GambleSpeed);
            groupBox_GambleSettings.Controls.Add(textBox_GambleSpeed);
            groupBox_GambleSettings.Controls.Add(label_GamblerDelay);
            groupBox_GambleSettings.Controls.Add(textBox_GamblerDelay);
            groupBox_GambleSettings.Controls.Add(label_GamblerStopKey);
            groupBox_GambleSettings.Controls.Add(label_GamblerStartKey);
            groupBox_GambleSettings.Controls.Add(label_GamblerGetCoorinatesKey);
            groupBox_GambleSettings.Controls.Add(textBox_GamblerStopKey);
            groupBox_GambleSettings.Controls.Add(textBox_GamblerStartKey);
            groupBox_GambleSettings.Controls.Add(textBox_GamblerGetCoordinatesKey);
            groupBox_GambleSettings.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            groupBox_GambleSettings.Location = new Point(7, 7);
            groupBox_GambleSettings.Name = "groupBox_GambleSettings";
            groupBox_GambleSettings.Size = new Size(603, 158);
            groupBox_GambleSettings.TabIndex = 58;
            groupBox_GambleSettings.TabStop = false;
            groupBox_GambleSettings.Text = "Gamble";
            // 
            // label_GambleSpeed
            // 
            label_GambleSpeed.AutoSize = true;
            label_GambleSpeed.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_GambleSpeed.Location = new Point(110, 78);
            label_GambleSpeed.Name = "label_GambleSpeed";
            label_GambleSpeed.Size = new Size(53, 21);
            label_GambleSpeed.TabIndex = 9;
            label_GambleSpeed.Text = "Speed";
            // 
            // textBox_GambleSpeed
            // 
            textBox_GambleSpeed.BackColor = Color.Transparent;
            textBox_GambleSpeed.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox_GambleSpeed.Location = new Point(110, 102);
            textBox_GambleSpeed.Name = "textBox_GambleSpeed";
            textBox_GambleSpeed.Size = new Size(85, 30);
            textBox_GambleSpeed.TabIndex = 8;
            textBox_GambleSpeed.TextAlign = HorizontalAlignment.Center;
            // 
            // label_GamblerDelay
            // 
            label_GamblerDelay.AutoSize = true;
            label_GamblerDelay.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_GamblerDelay.Location = new Point(452, 22);
            label_GamblerDelay.Name = "label_GamblerDelay";
            label_GamblerDelay.Size = new Size(49, 21);
            label_GamblerDelay.TabIndex = 7;
            label_GamblerDelay.Text = "Delay";
            // 
            // textBox_GamblerDelay
            // 
            textBox_GamblerDelay.BackColor = Color.Transparent;
            textBox_GamblerDelay.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox_GamblerDelay.Location = new Point(452, 46);
            textBox_GamblerDelay.Name = "textBox_GamblerDelay";
            textBox_GamblerDelay.Size = new Size(85, 30);
            textBox_GamblerDelay.TabIndex = 6;
            textBox_GamblerDelay.TextAlign = HorizontalAlignment.Center;
            // 
            // label_GamblerStopKey
            // 
            label_GamblerStopKey.AutoSize = true;
            label_GamblerStopKey.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_GamblerStopKey.Location = new Point(338, 22);
            label_GamblerStopKey.Name = "label_GamblerStopKey";
            label_GamblerStopKey.Size = new Size(41, 21);
            label_GamblerStopKey.TabIndex = 5;
            label_GamblerStopKey.Text = "Stop";
            // 
            // label_GamblerStartKey
            // 
            label_GamblerStartKey.AutoSize = true;
            label_GamblerStartKey.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_GamblerStartKey.Location = new Point(224, 22);
            label_GamblerStartKey.Name = "label_GamblerStartKey";
            label_GamblerStartKey.Size = new Size(42, 21);
            label_GamblerStartKey.TabIndex = 4;
            label_GamblerStartKey.Text = "Start";
            // 
            // label_GamblerGetCoorinatesKey
            // 
            label_GamblerGetCoorinatesKey.AutoSize = true;
            label_GamblerGetCoorinatesKey.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label_GamblerGetCoorinatesKey.Location = new Point(110, 22);
            label_GamblerGetCoorinatesKey.Name = "label_GamblerGetCoorinatesKey";
            label_GamblerGetCoorinatesKey.Size = new Size(72, 21);
            label_GamblerGetCoorinatesKey.TabIndex = 3;
            label_GamblerGetCoorinatesKey.Text = "Possition";
            // 
            // textBox_GamblerStopKey
            // 
            textBox_GamblerStopKey.BackColor = Color.Transparent;
            textBox_GamblerStopKey.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox_GamblerStopKey.Location = new Point(338, 46);
            textBox_GamblerStopKey.Name = "textBox_GamblerStopKey";
            textBox_GamblerStopKey.Size = new Size(85, 30);
            textBox_GamblerStopKey.TabIndex = 2;
            textBox_GamblerStopKey.TextAlign = HorizontalAlignment.Center;
            // 
            // textBox_GamblerStartKey
            // 
            textBox_GamblerStartKey.BackColor = Color.Transparent;
            textBox_GamblerStartKey.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox_GamblerStartKey.Location = new Point(224, 46);
            textBox_GamblerStartKey.Name = "textBox_GamblerStartKey";
            textBox_GamblerStartKey.Size = new Size(85, 30);
            textBox_GamblerStartKey.TabIndex = 1;
            textBox_GamblerStartKey.TextAlign = HorizontalAlignment.Center;
            // 
            // textBox_GamblerGetCoordinatesKey
            // 
            textBox_GamblerGetCoordinatesKey.BackColor = Color.Transparent;
            textBox_GamblerGetCoordinatesKey.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox_GamblerGetCoordinatesKey.Location = new Point(110, 46);
            textBox_GamblerGetCoordinatesKey.Name = "textBox_GamblerGetCoordinatesKey";
            textBox_GamblerGetCoordinatesKey.Size = new Size(85, 30);
            textBox_GamblerGetCoordinatesKey.TabIndex = 0;
            textBox_GamblerGetCoordinatesKey.TextAlign = HorizontalAlignment.Center;
            // 
            // tabPage_Logs
            // 
            tabPage_Logs.Location = new Point(4, 36);
            tabPage_Logs.Name = "tabPage_Logs";
            tabPage_Logs.Size = new Size(732, 397);
            tabPage_Logs.TabIndex = 2;
            tabPage_Logs.Text = "Logs";
            tabPage_Logs.UseVisualStyleBackColor = true;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(744, 451);
            Controls.Add(tabControl_Main);
            MinimumSize = new Size(760, 490);
            Name = "Main";
            Text = "PoE";
            FormClosing += Main_FormClosing;
            Load += Main_Load;
            tabControl_Main.ResumeLayout(false);
            tabPage_Main.ResumeLayout(false);
            tabPage_Main.PerformLayout();
            groupBox_Flask5.ResumeLayout(false);
            groupBox_Flask5.PerformLayout();
            groupBox_Flask4.ResumeLayout(false);
            groupBox_Flask4.PerformLayout();
            groupBox_Flask3.ResumeLayout(false);
            groupBox_Flask3.PerformLayout();
            groupBox_Flask2.ResumeLayout(false);
            groupBox_Flask2.PerformLayout();
            groupBox_Flask1.ResumeLayout(false);
            groupBox_Flask1.PerformLayout();
            tabPage_Gamble.ResumeLayout(false);
            tabPage_Gamble.PerformLayout();
            tabPage_Settings.ResumeLayout(false);
            groupBox_FlaskSettings.ResumeLayout(false);
            groupBox_FlaskSettings.PerformLayout();
            groupBox_GambleSettings.ResumeLayout(false);
            groupBox_GambleSettings.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private FlatTabControl tabControl_Main;
        private TabPage tabPage_Main;
        private TabPage tabPage_Settings;
        private TabPage tabPage_Logs;
        private FlatComboBox comboBox_Flask1;
        private Label label_Percent;
        private Label label_FlaskType;
        private Slider slider_Flask1;
        private Label label_Flask1_Slider;
        private Label label_Flask5_Slider;
        private Slider slider_Flask5;
        private FlatComboBox comboBox_Flask5;
        private Label label_Flask4_Slider;
        private Slider slider_Flask4;
        private FlatComboBox comboBox_Flask4;
        private Label label_Flask3_Slider;
        private Slider slider_Flask3;
        private FlatComboBox comboBox_Flask3;
        private Label label_Flask2_Slider;
        private Slider slider_Flask2;
        private FlatComboBox comboBox_Flask2;
        private FlatGroupBox groupBox_Flask1;
        private FlatGroupBox groupBox_Flask2;
        private FlatGroupBox groupBox_Flask5;
        private FlatGroupBox groupBox_Flask4;
        private FlatGroupBox groupBox_Flask3;
        private FlatTextBox textBox_Flask1;
        private FlatTextBox textBox_Flask5;
        private FlatTextBox textBox_Flask4;
        private FlatTextBox textBox_Flask3;
        private FlatTextBox textBox_Flask2;
        private Label label_Key;
        private CheckBox checkBox_Flask1;
        private CheckBox checkBox_Flask2;
        private CheckBox checkBox_Flask5;
        private CheckBox checkBox_Flask4;
        private CheckBox checkBox_Flask3;
        private Label label_Active;
        private TabPage tabPage_Gamble;
        private TabPage tabPage_Orbs;
        private FlatComboBox comboBox_GambleType;
        private Label label_GambleType;
        private FlatGroupBox groupBox_GambleSettings;
        private FlatGroupBox groupBox_FlaskSettings;
        private FlatTextBox textBox_GamblerGetCoordinatesKey;
        private FlatTextBox textBox_GamblerStartKey;
        private FlatTextBox textBox_GamblerStopKey;
        private Label label_GamblerStopKey;
        private Label label_GamblerStartKey;
        private Label label_GamblerGetCoorinatesKey;
        private Label label_GamblerDelay;
        private FlatTextBox textBox_GamblerDelay;
        private Label label_GambleSpeed;
        private FlatTextBox textBox_GambleSpeed;
        private Label label_FlaskRegisterKey;
        private FlatTextBox textBox_FlaskRegisterKey;
        private Label label_FlaskDrinkKey;
        private FlatTextBox textBox_FlaskDrinkKey;
        private Label label_FlaskStopKey;
        private FlatTextBox textBox_FlaskStopKey;
        private Label label_FlaskDelay;
        private FlatTextBox textBox_FlaskDelay;
        private Label label_FlaskKeyPressDelay;
        private FlatTextBox textBox_FlaskKeyPressDelay;
        private Label label_FlaskHpMpCooldown;
        private FlatTextBox textBox_FlaskHpMpCooldown;
        private Label label_FlaskUtilityCooldown;
        private FlatTextBox textBox_FlaskUtilityCooldown;
        private Label label_FlaskTinctureCooldown;
        private FlatTextBox textBox_FlaskTinctureCooldown;
        private ToolTip toolTip_Settings;
    }
}