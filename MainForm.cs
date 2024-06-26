﻿using LogicCircuits.Elements;
using LogicCircuits.Elements.Gates;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace LogicCircuits
{
    public partial class MainForm : Form
    {
        private GateInfo[] gateInfos;
        private List<IElement> draft;

        public MainForm()
        {
            InitializeComponent();
            gateInfos = new GateInfo[9] { Elements.Gates.Buffer.GetInfo(), NOT.GetInfo(), AND.GetInfo(),
                OR.GetInfo(), NAND.GetInfo(), NOR.GetInfo(), XOR.GetInfo(), XNOR.GetInfo(), IMPLY.GetInfo()};
            draft = new List<IElement>();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Render();
        }

        private void Render(Graphics g = null)
        {
            if (g == null) g = panelCanvas.CreateGraphics();

            g.Clear(Color.LightGray);
            panelCanvas.Controls.Clear();

            int width = panelCanvas.Width, height = panelCanvas.Height;

            for (int i = -2; i < height / 30 + 2; i++)
            {
                for (int j = -2; j < width / 30 + 2; j++)
                {
                    g.FillRectangle(Brushes.Black, j * 30 - 12, i * 30 - 18, 2, 1);
                }
            }

            int gateWidth = 70, gateHeight = 40;//coef 0.575
            for (int i = 0; i < draft.Count; i++)
            {
                g.DrawImage(draft[i].Diagram, draft[i].Location.X - gateWidth / 2, draft[i].Location.Y - gateHeight / 2, gateWidth, gateHeight);

                PictureBox removeButton = new PictureBox
                {
                    Tag = draft[i],
                    Size = new Size(10, 10),
                    Image = Properties.Resources.close,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Location = new Point(draft[i].Location.X - gateWidth / 4, draft[i].Location.Y - 4 * gateHeight / 5),
                };
                toolTipMenu.SetToolTip(removeButton, "Видалити вентиль");
                removeButton.Click += (sender, e) =>
                {
                    draft.Remove((sender as Control).Tag as IElement);
                    elementMoveable = false;
                    Cursor = Cursors.Default;
                    Render();
                };
                panelCanvas.Controls.Add(removeButton);


                PictureBox moveButton = new PictureBox
                {
                    Tag = draft[i],
                    Size = new Size(10, 10),
                    Image = Properties.Resources.move,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Location = new Point(draft[i].Location.X - 2 * gateWidth / 5, draft[i].Location.Y - 4 * gateHeight / 5),
                };
                toolTipMenu.SetToolTip(moveButton, "Перемістити вентиль");
                moveButton.Click += (sender, e) =>
                {
                    if (!elementMoveable)
                    {
                        elementMoveable = true;
                        moveableElement = (sender as Control).Tag as IElement;
                        Cursor = Cursors.NoMove2D;
                    }
                    else
                    {
                        if ((sender as Control).Tag as IElement == moveableElement)
                        {
                            elementMoveable = false;
                            Cursor = Cursors.Default;
                        }
                        else
                        {
                            moveableElement = (sender as Control).Tag as IElement;
                        }
                    }
                };
                panelCanvas.Controls.Add(moveButton);


                PictureBox connectButton = new PictureBox
                {
                    Tag = draft[i],
                    Size = new Size(15, 15),
                    Image = Properties.Resources.connect,
                    SizeMode = PictureBoxSizeMode.Zoom,
                };
                Point connLocation = new Point();
                if (draft[i] is AND || draft[i] is NAND)
                    connLocation = new Point(draft[i].Location.X - 2 * gateWidth / 6, draft[i].Location.Y - 1 * gateHeight / 5);
                else if (draft[i] is OR || draft[i] is NOR)
                    connLocation = new Point(draft[i].Location.X - 2 * gateWidth / 7, draft[i].Location.Y - 1 * gateHeight / 5);
                else if (draft[i] is XOR || draft[i] is XNOR)
                    connLocation = new Point(draft[i].Location.X - 2 * gateWidth / 9, draft[i].Location.Y - 1 * gateHeight / 5);
                else if (draft[i] is IMPLY)
                    connLocation = new Point(draft[i].Location.X - 2 * gateWidth / 8, draft[i].Location.Y - 1 * gateHeight / 5);
                else if (draft[i] is Elements.Gates.Buffer || draft[i] is NOT)
                    connLocation = new Point(draft[i].Location.X - 2 * gateWidth / 5, draft[i].Location.Y - 1 * gateHeight / 5);

                connectButton.Location = connLocation;
                toolTipMenu.SetToolTip(connectButton, "Приєднати вентиль або вихідний сигнал");
                connectButton.Click += (sender, e) =>
                {
                    if (!elementConnectable)
                    {
                        elementConnectable = true;
                        connectableElement = (sender as Control).Tag as IElement;
                        Cursor = Cursors.Hand;
                    }
                    else
                    {
                        elementConnectable = false;
                        Cursor = Cursors.Default;

                        IElement current = (sender as Control).Tag as IElement;

                        if (connectableElement.Location.X < current.Location.X)
                        {
                            if (connectableElement is IOutputContainingElement outputting)
                                if (current is IInputContainingElement inputting)
                                    outputting.Connect(inputting).ToString();
                            Render();
                        }
                        if (current.Location.X <= connectableElement.Location.X)
                        {
                            if (current is IOutputContainingElement outputting)
                                if (connectableElement is IInputContainingElement inputting)
                                    outputting.Connect(inputting).ToString();
                            Render();
                        }
                    }
                };
                panelCanvas.Controls.Add(connectButton);


                if (draft[i] is IInputContainingElement element2)
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    int inputs = element2.Inputs.Count;
                    if (inputs != 0)
                    {
                        Point[] points1 = new Point[inputs];
                        Point[] points2 = new Point[inputs];

                        IOutputContainingElement[] copy = new IOutputContainingElement[inputs];
                        element2.Inputs.CopyTo(copy);
                        List<IOutputContainingElement> sortedList = copy.ToList();
                        sortedList.Sort((IOutputContainingElement i1, IOutputContainingElement i2) => i1.Location.Y < i2.Location.Y ? -1 : 1);

                        for (int k = 0; k < inputs; k++)
                            points1[k] = new Point(sortedList[k].Location.X + gateWidth / 2 - 1, sortedList[k].Location.Y - 1);

                        int inputsArea = gateHeight / 7 * 5;
                        int gap = inputsArea / (inputs + 1);

                        for (int k = 1; k < inputs + 1; k++)
                            points2[k - 1] = new Point(element2.Location.X - gateWidth / 2, element2.Location.Y - inputsArea / 2 + gap * k);

                        if (element2 is OR || element2 is NOR || element2 is XOR || element2 is XNOR || element2 is IMPLY)
                            for (int k = 0; k < inputs; k++)
                                points2[k].X += 8;

                        for (int k = 0; k < inputs; k++)
                            g.DrawLine(new Pen(Color.Black, 2f), points1[k], points2[k]);
                    }
                }
            }
        }

        private void PanelCanvasClick(object sender, EventArgs e)
        {
            if (gateSelected)
            {
                gateSelected = false;
                Cursor = Cursors.Default;
                for (int i = 0; i < panelGates.Controls.Count; i++)
                    if (panelGates.Controls[i].Controls[0].Tag.ToString() == selectedGate.ToString())
                    {
                        panelGates.Controls[i].BackColor = SystemColors.Control; break;
                    }
                AddGate(selectedGate);
            }
            if (elementMoveable)
            {
                elementMoveable = false;
                Cursor = Cursors.Default;
                moveableElement.Location = panelCanvas.PointToClient(Cursor.Position);
                Render();
            }
        }

        private void AddGate(int tag)
        {
            IGate gate = null;
            switch (tag)
            {
                case 1:
                    gate = new Elements.Gates.Buffer();
                    break;
                case 2:
                    gate = new NOT();
                    break;
                case 3:
                    gate = new AND();
                    break;
                case 4:
                    gate = new OR();
                    break;
                case 5:
                    gate = new NAND();
                    break;
                case 6:
                    gate = new NOR();
                    break;
                case 7:
                    gate = new XOR();
                    break;
                case 8:
                    gate = new XNOR();
                    break;
                case 9:
                    gate = new IMPLY();
                    break;
            }

            int width = panelCanvas.Width, height = panelCanvas.Height;
            gate.Location = panelCanvas.PointToClient(Cursor.Position);

            draft.Add(gate);
            Render();
        }

        private bool gateSelected = false;
        private int selectedGate = -1;

        private bool elementMoveable = false;
        private IElement moveableElement = null;

        private bool elementConnectable = false;
        private IElement connectableElement = null;
        private void GatesToolsClicked(object sender, EventArgs e)
        {
            int current = int.Parse((sender as Control).Tag.ToString());

            if (!gateSelected)
            {
                gateSelected = true;
                selectedGate = current;
                Cursor = Cursors.Cross;
                (sender as Control).Parent.BackColor = Color.LightGray;
            }
            else
            {
                if (current == selectedGate)
                {
                    gateSelected = false;
                    Cursor = Cursors.Default;
                    (sender as Control).Parent.BackColor = SystemColors.Control;
                }
                else
                {
                    for (int i = 0; i < panelGates.Controls.Count; i++)
                        if (panelGates.Controls[i].Controls[0].Tag.ToString() == selectedGate.ToString())
                        {
                            panelGates.Controls[i].BackColor = SystemColors.Control; break;
                        }
                    selectedGate = current;
                    (sender as Control).Parent.BackColor = Color.LightGray;
                }
            }
        }

        private void MenuButtonsMouseEnter(object sender, EventArgs e)
        {
            (sender as Control).Parent.BackColor = Color.LightGray;

            object tag = (sender as Control).Tag;
            if (tag != null && int.TryParse(tag.ToString(), out int gate))
            {
                labelGateName.Text = gateInfos[gate - 1].Name;
                pictureBoxFormula.Image = gateInfos[gate - 1].Formula;
                pictureBoxDiagram.Image = gateInfos[gate - 1].Diagram;
                pictureBoxGateTable.Image = gateInfos[gate - 1].TruthTable;
            }
        }

        private void MenuButtonsMouseLeave(object sender, EventArgs e)
        {
            if (!(gateSelected && (sender as Control).Tag.ToString() == selectedGate.ToString()))
                (sender as Control).Parent.BackColor = SystemColors.Control;

            labelGateName.Text = "<Назва вентиля>";
            if (pictureBoxFormula.Image != null) pictureBoxFormula.Image = null;
            if (pictureBoxDiagram != null) pictureBoxDiagram.Image = null;
            if (pictureBoxGateTable != null) pictureBoxGateTable.Image = null;
        }

        protected override void WndProc(ref Message m) //prevents redrawing caused by alt
        {
            if (m.Msg == 0x128) return;
            base.WndProc(ref m);
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            Render();
        }
    }
}
