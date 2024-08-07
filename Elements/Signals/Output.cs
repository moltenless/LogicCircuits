﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogicCircuits.Elements
{
    public class Output : IInputContainingElement
    {
        public Point Location { get; set; }
        public Image Diagram { get; } = Properties.Resources._out;
        public string Name { get; } = null;
        public List<IOutputContainingElement> Inputs { get; set; } = new List<IOutputContainingElement>();
        public InputsMultiplicity InputsMultiplicity => InputsMultiplicity.Single;

        public List<Control> Controls { get; set; } = new List<Control>();

        public Output(string name)
        {
            Name = name;
        }

        public int CalculateOutput(List<(IElement, int outputResult)> register)
        {
            if (Inputs.Count != 1)
            {
                register.Add((this, -1));
                return -1;
            }

            int input = Inputs[0].CalculateOutput(register);
            if (input == -1)
            {
                register.Add((this, -1));
                return -1;
            }

            int output = input;
            register.Add((this, output));
            return output;
        }
    }
}
