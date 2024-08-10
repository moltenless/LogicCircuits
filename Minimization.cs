﻿using LogicCircuits.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogicCircuits.Forms;

namespace LogicCircuits
{
    internal static class Minimization
    {
        public static string MinimizeQuineMcCluskey(List<(IElement, int outputResult)> registry)
        {
            //List<List<int>> truthTable = FormsBuilder.GetTruthTable(registry, out List<string> columnNames);
            //int cols = truthTable[0].Count;
            //int rows = truthTable.Count;

            List<List<int>> truthTable = new List<List<int>>
            {
                new List<int> { 0, 0, 0, 1 },
                new List<int> { 0, 0, 1, 1 },
                new List<int> { 0, 1, 0, 1 },
                new List<int> { 0, 1, 1, 0 },
                new List<int> { 1, 0, 0, 1 },
                new List<int> { 1, 0, 1, 1 },
                new List<int> { 1, 1, 0, 0 },
                new List<int> { 1, 1, 1, 1 }
            };
            List<string> columnNames = new List<string>
            {
                "X1", "X2", "X3", "Y"
            };
            int cols = truthTable[0].Count;
            int rows = truthTable.Count;

            List<List<int>> minterms = new List<List<int>>();
            for (int i = 0; i < rows; i++)
                if (truthTable[i][cols - 1] == 1)
                    minterms.Add(truthTable[i]);

            string prefix = $"{columnNames[cols - 1]}({columnNames[0]}";
            for (int i = 1; i < cols - 1; i++)
                prefix += $",{columnNames[i]}";
            prefix += ") = ";

            if (minterms.Count == 0) return prefix + "0";
            if (minterms.Count == rows) return prefix = "1";

            List<List<int>> currentImplicants = new List<List<int>>();
            for (int i = 0; i < minterms.Count; i++)
            {
                currentImplicants.Add(new List<int>());
                for (int j = 0; j < minterms[i].Count - 1; j++)
                    currentImplicants[currentImplicants.Count - 1].Add(minterms[i][j]);
            }

            List<List<int>> primeImplicants = new List<List<int>>();
            int termsLength = currentImplicants[0].Count;

            for (int j = 0; j < 5; j++)
            {
                List<List<int>> coveringNextImplicants = new List<List<int>>();

                for (int k = 0; k < currentImplicants.Count - 1; k++)
                {
                    int[] compared = currentImplicants[k].ToArray();
                    for (int g = k + 1; g < currentImplicants.Count; g++)
                    {
                        int[] comparing = currentImplicants[g].ToArray();
                        int differentValuesCounter = 0;
                        bool dontCaresMisplaced = false;
                        for (int h = 0; h < termsLength; h++)
                        {
                            if (compared[h] == 0 && comparing[h] == 1 || compared[h] == 1 && comparing[h] == 0)
                                differentValuesCounter++;
                            if (compared[h] == -1 && comparing[h] != -1 || compared[h] != -1 && comparing[h] == -1)
                                dontCaresMisplaced = true;
                        }
                        if (dontCaresMisplaced)
                            continue;

                        if (differentValuesCounter == 1)
                        {
                            coveringNextImplicants.Add(new List<int>());
                            for (int h = 0; h < termsLength; h++)
                                if (compared[h] == comparing[h])
                                    coveringNextImplicants[coveringNextImplicants.Count - 1].Add(compared[h]);
                                else
                                    coveringNextImplicants[coveringNextImplicants.Count - 1].Add(-1);
                        }
                    }
                }

                for (int k = 0; k < currentImplicants.Count; k++)
                {
                    bool prime = true;
                    for (int g = 0; g < coveringNextImplicants.Count; g++)
                    {
                        if (Covers(coveringNextImplicants[g], currentImplicants[k]))
                        {
                            prime = false;
                            break;
                        }
                    }

                    if (prime)
                        primeImplicants.Add(currentImplicants[k]);
                }

                string result = "";

                for (int i = 0; i < currentImplicants.Count; i++)
                {
                    for (int e = 0; e < currentImplicants[i].Count; e++)
                        result += currentImplicants[i][e] + "\t";
                    result += "\n";
                }
                result += "\n";
                for (int i = 0; i < coveringNextImplicants.Count; i++)
                {
                    for (int e = 0; e < coveringNextImplicants[i].Count; e++)
                        result += coveringNextImplicants[i][e] + "\t";
                    result += "\n";
                }
                result += "\n";
                for (int i = 0; i < primeImplicants.Count; i++)
                {
                    for (int e = 0; e < primeImplicants[i].Count; e++)
                        result += primeImplicants[i][e] + "\t";
                    result += "\n";
                }
                System.Windows.Forms.MessageBox.Show(result);

                currentImplicants = coveringNextImplicants;
            }



            string result1 = "";

            return result1;
        }

        public static bool Covers(List<int> implicant, List<int> targetTerm)
        {
            for (int i = 0; i < implicant.Count; i++)
            {
                if (implicant[i] == 0 && targetTerm[i] != 0)
                    return false;
                if (implicant[i] == 1 && targetTerm[i] != 1)
                    return false;
            }
            return true;
        }
    }
}