using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Speech.Synthesis;

namespace Tests;

[TestClass]
public class AttackStats
{
    [TestMethod]
    public void CalculateAttackStats()
    {
        int LIMIT = 10000;
        int[] VANILLA = new int[] { 2, 3, 4, 6, 9, 12, 18, 24 };
        int[] current = new int[8];

        List<int> attacks1 = new List<int>();
        List<int> attacks2 = new List<int>();
        List<int> attacks3 = new List<int>();
        List<int> attacks4 = new List<int>();
        List<int> attacks5 = new List<int>();
        List<int> attacks6 = new List<int>();
        List<int> attacks7 = new List<int>();
        List<int> attacks8 = new List<int>();

        Random RNG = new Random();

        for(int monteCarloIteration = 1; monteCarloIteration <= LIMIT; monteCarloIteration++)
        {
            for (int i = 0; i < 8; i++)
            {
                int minAtk = (int)Math.Ceiling(VANILLA[i] - VANILLA[i] * .333);
                int maxAtk = (int)(VANILLA[i] + VANILLA[i] * .5);
                int next = VANILLA[i];

                next = RNG.Next(minAtk, maxAtk);

                if (i == 0)
                {
                    current[i] = Math.Max(next, 2);
                }
                else
                {
                    if (next < current[i - 1])
                    {
                        current[i] = current[i - 1];
                    }
                    else
                    {
                        current[i] = next;
                    }
                }
            }
            attacks1.Add(current[0]);
            attacks2.Add(current[1]);
            attacks3.Add(current[2]);
            attacks4.Add(current[3]);
            attacks5.Add(current[4]);
            attacks6.Add(current[5]);
            attacks7.Add(current[6]);
            attacks8.Add(current[7]);
        }

        Console.WriteLine("Attack 1: " + attacks1.Average());
        Console.WriteLine("Attack 2: " + attacks2.Average());
        Console.WriteLine("Attack 3: " + attacks3.Average());
        Console.WriteLine("Attack 4: " + attacks4.Average());
        Console.WriteLine("Attack 5: " + attacks5.Average());
        Console.WriteLine("Attack 6: " + attacks6.Average());
        Console.WriteLine("Attack 7: " + attacks7.Average());
        Console.WriteLine("Attack 8: " + attacks8.Average());
    }

    [TestMethod]
    public void CalculateAdjustedAttackStats()
    {
        int LIMIT = 10000;
        double[] VANILLA = new double[] { 2, 3, 4, 6, 9, 12, 18, 24 };
        int[] current = new int[8];

        List<int> attacks1 = new List<int>();
        List<int> attacks2 = new List<int>();
        List<int> attacks3 = new List<int>();
        List<int> attacks4 = new List<int>();
        List<int> attacks5 = new List<int>();
        List<int> attacks6 = new List<int>();
        List<int> attacks7 = new List<int>();
        List<int> attacks8 = new List<int>();

        Random RNG = new Random();

        for (int monteCarloIteration = 1; monteCarloIteration <= LIMIT; monteCarloIteration++)
        {
            for (int i = 0; i < 8; i++)
            {
                double minAtk = VANILLA[i] - VANILLA[i] * .333;
                double maxAtk = VANILLA[i] + VANILLA[i] * .5;
                double next = VANILLA[i];

                next = RNG.NextDouble() * (maxAtk - minAtk) + minAtk;

                if (i == 0)
                {
                    current[i] = (int)Math.Round(Math.Max(next, 2));
                }
                else
                {
                    if (next < current[i - 1])
                    {
                        current[i] = current[i - 1];
                    }
                    else
                    {
                        current[i] = (int)Math.Round(next);
                    }
                }
                current[i] = (int)Math.Min(current[i], maxAtk);
                current[i] = (int)Math.Max(current[i], minAtk);
            }
            attacks1.Add(current[0]);
            attacks2.Add(current[1]);
            attacks3.Add(current[2]);
            attacks4.Add(current[3]);
            attacks5.Add(current[4]);
            attacks6.Add(current[5]);
            attacks7.Add(current[6]);
            attacks8.Add(current[7]);
        }

        Console.WriteLine(attacks1.Average());
        Console.WriteLine(attacks2.Average());
        Console.WriteLine(attacks3.Average());
        Console.WriteLine(attacks4.Average());
        Console.WriteLine(attacks5.Average());
        Console.WriteLine(attacks6.Average());
        Console.WriteLine(attacks7.Average());
        Console.WriteLine(attacks8.Average());
    }
}