﻿namespace BioImage
{
    partial class ROIManager
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ROIManager));
            this.roiView = new System.Windows.Forms.ListView();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.rBox = new System.Windows.Forms.NumericUpDown();
            this.gBox = new System.Windows.Forms.NumericUpDown();
            this.bBox = new System.Windows.Forms.NumericUpDown();
            this.tBox = new System.Windows.Forms.NumericUpDown();
            this.cBox = new System.Windows.Forms.NumericUpDown();
            this.zBox = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.yBox = new System.Windows.Forms.NumericUpDown();
            this.xBox = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.hBox = new System.Windows.Forms.NumericUpDown();
            this.wBox = new System.Windows.Forms.NumericUpDown();
            this.typeBox = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.showBoundsBox = new System.Windows.Forms.CheckBox();
            this.showTextBox = new System.Windows.Forms.CheckBox();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label7 = new System.Windows.Forms.Label();
            this.imageNameLabel = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.idBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.sBox = new System.Windows.Forms.NumericUpDown();
            this.exportToCSVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAllToCSVToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateBut = new System.Windows.Forms.Button();
            this.addButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.rBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.zBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.yBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.xBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.hBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.wBox)).BeginInit();
            this.contextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sBox)).BeginInit();
            this.SuspendLayout();
            // 
            // roiView
            // 
            this.roiView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.roiView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(91)))), ((int)(((byte)(138)))));
            this.roiView.ForeColor = System.Drawing.Color.White;
            this.roiView.HideSelection = false;
            this.roiView.Location = new System.Drawing.Point(-1, 0);
            this.roiView.MultiSelect = false;
            this.roiView.Name = "roiView";
            this.roiView.Size = new System.Drawing.Size(221, 338);
            this.roiView.TabIndex = 0;
            this.roiView.UseCompatibleStateImageBehavior = false;
            this.roiView.View = System.Windows.Forms.View.List;
            this.roiView.SelectedIndexChanged += new System.EventHandler(this.roiView_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(226, 139);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Stroke Color RGB";
            // 
            // rBox
            // 
            this.rBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.rBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(91)))), ((int)(((byte)(138)))));
            this.rBox.ForeColor = System.Drawing.Color.White;
            this.rBox.Location = new System.Drawing.Point(229, 155);
            this.rBox.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.rBox.Name = "rBox";
            this.rBox.Size = new System.Drawing.Size(48, 20);
            this.rBox.TabIndex = 2;
            this.rBox.ValueChanged += new System.EventHandler(this.rBox_ValueChanged);
            // 
            // gBox
            // 
            this.gBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(91)))), ((int)(((byte)(138)))));
            this.gBox.ForeColor = System.Drawing.Color.White;
            this.gBox.Location = new System.Drawing.Point(283, 155);
            this.gBox.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.gBox.Name = "gBox";
            this.gBox.Size = new System.Drawing.Size(48, 20);
            this.gBox.TabIndex = 3;
            this.gBox.ValueChanged += new System.EventHandler(this.gBox_ValueChanged);
            // 
            // bBox
            // 
            this.bBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(91)))), ((int)(((byte)(138)))));
            this.bBox.ForeColor = System.Drawing.Color.White;
            this.bBox.Location = new System.Drawing.Point(337, 155);
            this.bBox.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.bBox.Name = "bBox";
            this.bBox.Size = new System.Drawing.Size(48, 20);
            this.bBox.TabIndex = 4;
            this.bBox.ValueChanged += new System.EventHandler(this.bBox_ValueChanged);
            // 
            // tBox
            // 
            this.tBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.tBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(91)))), ((int)(((byte)(138)))));
            this.tBox.ForeColor = System.Drawing.Color.White;
            this.tBox.Location = new System.Drawing.Point(337, 115);
            this.tBox.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.tBox.Name = "tBox";
            this.tBox.Size = new System.Drawing.Size(48, 20);
            this.tBox.TabIndex = 8;
            this.tBox.ValueChanged += new System.EventHandler(this.tBox_ValueChanged);
            // 
            // cBox
            // 
            this.cBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(91)))), ((int)(((byte)(138)))));
            this.cBox.ForeColor = System.Drawing.Color.White;
            this.cBox.Location = new System.Drawing.Point(283, 115);
            this.cBox.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.cBox.Name = "cBox";
            this.cBox.Size = new System.Drawing.Size(48, 20);
            this.cBox.TabIndex = 7;
            this.cBox.ValueChanged += new System.EventHandler(this.cBox_ValueChanged);
            // 
            // zBox
            // 
            this.zBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.zBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(91)))), ((int)(((byte)(138)))));
            this.zBox.ForeColor = System.Drawing.Color.White;
            this.zBox.Location = new System.Drawing.Point(229, 115);
            this.zBox.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.zBox.Name = "zBox";
            this.zBox.Size = new System.Drawing.Size(48, 20);
            this.zBox.TabIndex = 6;
            this.zBox.ValueChanged += new System.EventHandler(this.zBox_ValueChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(226, 99);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(28, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "ZCT";
            // 
            // yBox
            // 
            this.yBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.yBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(91)))), ((int)(((byte)(138)))));
            this.yBox.ForeColor = System.Drawing.Color.White;
            this.yBox.Location = new System.Drawing.Point(325, 48);
            this.yBox.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.yBox.Name = "yBox";
            this.yBox.Size = new System.Drawing.Size(60, 20);
            this.yBox.TabIndex = 10;
            this.yBox.ValueChanged += new System.EventHandler(this.yBox_ValueChanged);
            // 
            // xBox
            // 
            this.xBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.xBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(91)))), ((int)(((byte)(138)))));
            this.xBox.ForeColor = System.Drawing.Color.White;
            this.xBox.Location = new System.Drawing.Point(263, 48);
            this.xBox.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.xBox.Name = "xBox";
            this.xBox.Size = new System.Drawing.Size(56, 20);
            this.xBox.TabIndex = 9;
            this.xBox.ValueChanged += new System.EventHandler(this.xBox_ValueChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(230, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "X, Y:";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(225, 76);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "W, H:";
            // 
            // hBox
            // 
            this.hBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.hBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(91)))), ((int)(((byte)(138)))));
            this.hBox.ForeColor = System.Drawing.Color.White;
            this.hBox.Location = new System.Drawing.Point(325, 74);
            this.hBox.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.hBox.Name = "hBox";
            this.hBox.Size = new System.Drawing.Size(60, 20);
            this.hBox.TabIndex = 13;
            this.hBox.ValueChanged += new System.EventHandler(this.hBox_ValueChanged);
            // 
            // wBox
            // 
            this.wBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.wBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(91)))), ((int)(((byte)(138)))));
            this.wBox.ForeColor = System.Drawing.Color.White;
            this.wBox.Location = new System.Drawing.Point(263, 74);
            this.wBox.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.wBox.Name = "wBox";
            this.wBox.Size = new System.Drawing.Size(56, 20);
            this.wBox.TabIndex = 12;
            this.wBox.ValueChanged += new System.EventHandler(this.wBox_ValueChanged);
            // 
            // typeBox
            // 
            this.typeBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.typeBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(91)))), ((int)(((byte)(138)))));
            this.typeBox.ForeColor = System.Drawing.Color.White;
            this.typeBox.FormattingEnabled = true;
            this.typeBox.Location = new System.Drawing.Point(283, 181);
            this.typeBox.Name = "typeBox";
            this.typeBox.Size = new System.Drawing.Size(103, 21);
            this.typeBox.TabIndex = 15;
            this.typeBox.SelectedIndexChanged += new System.EventHandler(this.typeBox_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(244, 184);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(34, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Type:";
            // 
            // textBox
            // 
            this.textBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(91)))), ((int)(((byte)(138)))));
            this.textBox.ForeColor = System.Drawing.Color.White;
            this.textBox.Location = new System.Drawing.Point(283, 206);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(103, 20);
            this.textBox.TabIndex = 17;
            this.textBox.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(244, 209);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(31, 13);
            this.label6.TabIndex = 18;
            this.label6.Text = "Text:";
            // 
            // showBoundsBox
            // 
            this.showBoundsBox.AutoSize = true;
            this.showBoundsBox.ForeColor = System.Drawing.Color.White;
            this.showBoundsBox.Location = new System.Drawing.Point(229, 261);
            this.showBoundsBox.Name = "showBoundsBox";
            this.showBoundsBox.Size = new System.Drawing.Size(92, 17);
            this.showBoundsBox.TabIndex = 19;
            this.showBoundsBox.Text = "Show Bounds";
            this.showBoundsBox.UseVisualStyleBackColor = true;
            // 
            // showTextBox
            // 
            this.showTextBox.AutoSize = true;
            this.showTextBox.ForeColor = System.Drawing.Color.White;
            this.showTextBox.Location = new System.Drawing.Point(318, 261);
            this.showTextBox.Name = "showTextBox";
            this.showTextBox.Size = new System.Drawing.Size(77, 17);
            this.showTextBox.TabIndex = 20;
            this.showTextBox.Text = "Show Text";
            this.showTextBox.UseVisualStyleBackColor = true;
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem,
            this.exportToCSVToolStripMenuItem,
            this.exportAllToCSVToolStripMenuItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(164, 70);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.ForeColor = System.Drawing.Color.White;
            this.label7.Location = new System.Drawing.Point(233, 9);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(67, 13);
            this.label7.TabIndex = 22;
            this.label7.Text = "Image Name";
            // 
            // imageNameLabel
            // 
            this.imageNameLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.imageNameLabel.AutoSize = true;
            this.imageNameLabel.ForeColor = System.Drawing.Color.White;
            this.imageNameLabel.Location = new System.Drawing.Point(238, 27);
            this.imageNameLabel.Name = "imageNameLabel";
            this.imageNameLabel.Size = new System.Drawing.Size(0, 13);
            this.imageNameLabel.TabIndex = 23;
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.ForeColor = System.Drawing.Color.White;
            this.label8.Location = new System.Drawing.Point(246, 232);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(21, 13);
            this.label8.TabIndex = 24;
            this.label8.Text = "ID:";
            // 
            // idBox
            // 
            this.idBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(91)))), ((int)(((byte)(138)))));
            this.idBox.ForeColor = System.Drawing.Color.White;
            this.idBox.Location = new System.Drawing.Point(283, 229);
            this.idBox.Name = "idBox";
            this.idBox.Size = new System.Drawing.Size(103, 20);
            this.idBox.TabIndex = 25;
            this.idBox.TextChanged += new System.EventHandler(this.idBox_TextChanged);
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.ForeColor = System.Drawing.Color.White;
            this.label9.Location = new System.Drawing.Point(293, 286);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(39, 13);
            this.label9.TabIndex = 27;
            this.label9.Text = "Series:";
            // 
            // sBox
            // 
            this.sBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(49)))), ((int)(((byte)(91)))), ((int)(((byte)(138)))));
            this.sBox.ForeColor = System.Drawing.Color.White;
            this.sBox.Location = new System.Drawing.Point(338, 284);
            this.sBox.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.sBox.Name = "sBox";
            this.sBox.Size = new System.Drawing.Size(48, 20);
            this.sBox.TabIndex = 26;
            this.sBox.ValueChanged += new System.EventHandler(this.sBox_ValueChanged);
            // 
            // exportToCSVToolStripMenuItem
            // 
            this.exportToCSVToolStripMenuItem.Name = "exportToCSVToolStripMenuItem";
            this.exportToCSVToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.exportToCSVToolStripMenuItem.Text = "Export to CSV";
            // 
            // exportAllToCSVToolStripMenuItem
            // 
            this.exportAllToCSVToolStripMenuItem.Name = "exportAllToCSVToolStripMenuItem";
            this.exportAllToCSVToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            this.exportAllToCSVToolStripMenuItem.Text = "Export All to CSV";
            // 
            // updateBut
            // 
            this.updateBut.Location = new System.Drawing.Point(229, 308);
            this.updateBut.Name = "updateBut";
            this.updateBut.Size = new System.Drawing.Size(75, 25);
            this.updateBut.TabIndex = 28;
            this.updateBut.Text = "Update";
            this.updateBut.UseVisualStyleBackColor = true;
            this.updateBut.Click += new System.EventHandler(this.updateBut_Click);
            // 
            // addButton
            // 
            this.addButton.Location = new System.Drawing.Point(312, 308);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(75, 25);
            this.addButton.TabIndex = 29;
            this.addButton.Text = "Add";
            this.addButton.UseVisualStyleBackColor = true;
            this.addButton.Click += new System.EventHandler(this.addButton_Click);
            // 
            // ROIManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(122)))), ((int)(((byte)(156)))));
            this.ClientSize = new System.Drawing.Size(397, 339);
            this.Controls.Add(this.addButton);
            this.Controls.Add(this.updateBut);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.sBox);
            this.Controls.Add(this.idBox);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.imageNameLabel);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.showTextBox);
            this.Controls.Add(this.showBoundsBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.typeBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.hBox);
            this.Controls.Add(this.wBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.yBox);
            this.Controls.Add(this.xBox);
            this.Controls.Add(this.tBox);
            this.Controls.Add(this.cBox);
            this.Controls.Add(this.zBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.bBox);
            this.Controls.Add(this.gBox);
            this.Controls.Add(this.rBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.roiView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ROIManager";
            this.Text = "ROI Manager";
            this.Activated += new System.EventHandler(this.ROIManager_Activated);
            ((System.ComponentModel.ISupportInitialize)(this.rBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.zBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.yBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.xBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.hBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.wBox)).EndInit();
            this.contextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView roiView;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown rBox;
        private System.Windows.Forms.NumericUpDown gBox;
        private System.Windows.Forms.NumericUpDown bBox;
        private System.Windows.Forms.NumericUpDown tBox;
        private System.Windows.Forms.NumericUpDown cBox;
        private System.Windows.Forms.NumericUpDown zBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown yBox;
        private System.Windows.Forms.NumericUpDown xBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown hBox;
        private System.Windows.Forms.NumericUpDown wBox;
        private System.Windows.Forms.ComboBox typeBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox showBoundsBox;
        private System.Windows.Forms.CheckBox showTextBox;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label imageNameLabel;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox idBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.NumericUpDown sBox;
        private System.Windows.Forms.ToolStripMenuItem exportToCSVToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAllToCSVToolStripMenuItem;
        private System.Windows.Forms.Button updateBut;
        private System.Windows.Forms.Button addButton;
    }
}