﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using McNerd.MachineLearning.LinearAlgebra;

namespace ConsoleTester
{
    /// <summary>
    /// This console app is just for playing around with the code.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            bool Exit = false;
            WriteCommands();

            while(!Exit)
            {
                Exit = GetCommand();
            }
        }

        static bool GetCommand()
        {
            bool IsExit = false;

            ConsoleKeyInfo key = Console.ReadKey(true);

            switch (key.KeyChar)
            {
                case '1':
                    Console.Clear();
                    LinearRegressionDemo();
                    WriteCommands();
                    break;
                case 'x':
                    IsExit = true;
                    break;
                default:
                    break;
            }

            return IsExit;
        }

        static void LinearRegressionDemo()
        {
            WriteH1("Linear Regression");

            #region Compute Cost
            WriteH2("Cost Functions");
            #region Test Cost Function A
            Matrix X = new Matrix(new double[,] {
                { 2.0, 1.0, 3.0 },
                { 7.0, 1.0, 9.0 },
                { 1.0, 8.0, 1.0 },
                { 3.0, 7.0, 4.0 }
            });
            Matrix y = new Matrix(new double[,] {
                { 2.0 },
                { 5.0 },
                { 5.0 },
                { 6.0 }
            });
            Matrix theta = new Matrix(new double[,] {
                { 0.4 },
                { 0.6 },
                { 0.8 }
            });

            double cost = LinearRegression.ComputeCost(X, y, theta);

            // Aiming for a result around 5.295
            Console.WriteLine("Target: 5.295    Actual: {0}", cost);
            #endregion

            #region Test Cost Function B
            X = new Matrix(new double[,] {
                { 1.0, 2.0 },
                { 1.0, 3.0 },
                { 1.0, 4.0 },
                { 1.0, 5.0 }
            });
            y = new Matrix(new double[,] {
                { 7.0 },
                { 6.0 },
                { 5.0 },
                { 4.0 }
            });
            theta = new Matrix(new double[,] {
                { 0.1 },
                { 0.2 }
            });

            cost = LinearRegression.ComputeCost(X, y, theta);

            // Aiming for a result around 11.945
            Console.WriteLine("Target: 11.945   Actual: {0}", cost);
            #endregion

            #region Test Cost Function C
            X = new Matrix(new double[,] {
                { 1.0, 2.0, 3.0 },
                { 1.0, 3.0, 4.0 },
                { 1.0, 4.0, 5.0 },
                { 1.0, 5.0, 6.0 }
            });
            y = new Matrix(new double[,] {
                { 7.0 },
                { 6.0 },
                { 5.0 },
                { 4.0 }
            });
            theta = new Matrix(new double[,] {
                { 0.1 },
                { 0.2 },
                { 0.3 }
            });

            cost = LinearRegression.ComputeCost(X, y, theta);

            // Aiming for a result around 7.0175
            Console.WriteLine("Target: 7.0175   Actual: {0}", cost);
            #endregion
            #endregion

            #region Gradient Descent
            WriteH2("Gradient Descent");

            #region Gradient Descent A
            X = new Matrix(new double[,] {
                { 2.0, 1.0, 3.0 },
                { 7.0, 1.0, 9.0 },
                { 1.0, 8.0, 1.0 },
                { 3.0, 7.0, 4.0 }
            });

            y = new Matrix(new double[,] {
                { 2.0 },
                { 5.0 },
                { 5.0 },
                { 6.0 }
            });

            theta = new Matrix(3, 1);
            Matrix result = LinearRegression.GradientDescent(X, y, theta, 0.01, 100);

            Console.Write("Target: 0.23; 0.56; 0.31;   Actual: ");
            Console.WriteLine(result.ToString().Replace(" \n", "; "));
            #endregion

            #region Gradient Descent B
            X = new Matrix(new double[,] {
                { 1.0, 5.0 },
                { 1.0, 2.0 },
                { 1.0, 4.0 },
                { 1.0, 5.0 }
            });

            y = new Matrix(new double[,] {
                { 1.0 },
                { 6.0 },
                { 4.0 },
                { 2.0 }
            });

            theta = new Matrix(2, 1);
            result = LinearRegression.GradientDescent(X, y, theta, 0.01, 1000);

            Console.Write("Target: 5.2; -0.57;   Actual: ");
            Console.WriteLine(result.ToString().Replace(" \n", "; "));
            #endregion

            #region Gradient Descent C
            // Starting from a non-zero theta
            X = new Matrix(new double[,] {
                { 1.0, 5.0 },
                { 1.0, 2.0 }
            });

            y = new Matrix(new double[,] {
                { 1.0 },
                { 6.0 }
            });

            theta = new Matrix(new double[,] { { 0.5 }, { 0.5 } });
            result = LinearRegression.GradientDescent(X, y, theta, 0.1, 10);

            Console.Write("Target: 1.7; 0.19;    Actual: ");
            Console.WriteLine(result.ToString().Replace(" \n", "; "));
            #endregion

            #endregion

            #region Feature Normalization
            WriteH2("Feature Normalization");

            #region Feature Normalization A
            X = new Matrix(new double[,] {
                { 1.0 },
                { 2.0 },
                { 3.0 }
            });
            result = LinearRegression.FeatureNormalization(X);

            Console.Write("Target: -1.0; 0.0; 1.0;    Actual: ");
            Console.WriteLine(result.ToString().Replace(" \n", "; "));
            #endregion

            #region Feature Normalization B
            X = Matrix.Magic(3);
            result = LinearRegression.FeatureNormalization(X);

            Console.Write("Target: 1.13 -1.00 0.38; -0.76 0.00 0.76; -0.38 1.00 -1.13;\nActual: ");
            Console.WriteLine(result.ToString().Replace(" \n", "; "));
            #endregion

            #region Feature Normalization C
            X = Matrix.Magic(3);
            X = Matrix.Join(Matrix.Ones(1, 3) * -1, X, MatrixDimensions.Rows);
            result = LinearRegression.FeatureNormalization(X);

            Console.Write("Target: -1.21 -1.01 -1.21; 1.21 -0.56 0.67; -0.14 0.34 0.95; 0.14 1.24 -0.41;\nActual: ");
            Console.WriteLine(result.ToString().Replace(" \n", "; "));
            #endregion

            #endregion

            #region Normal Equation
            WriteH2("Normal Equation");

            // This gives the same answer as Gradient Descent A above, if GD is allowed
            // to iterate enough times.
            X = new Matrix(new double[,] {
                { 2.0, 1.0, 3.0 },
                { 7.0, 1.0, 9.0 },
                { 1.0, 8.0, 1.0 },
                { 3.0, 7.0, 4.0 }
            });

            y = new Matrix(new double[,] {
                { 2.0 },
                { 5.0 },
                { 5.0 },
                { 6.0 }
            });

            theta = new Matrix(2, 1);
            Matrix thetaNormal = LinearRegression.NormalEquation(X, y);

            Console.Write("Target: 0.008; 0.568; 0.486;   Actual: ");
            Console.WriteLine(thetaNormal.ToString().Replace(" \n", "; "));
            #endregion
        }

        static void WriteCommands()
        {
            ConsoleColor fc = Console.ForegroundColor;
            ConsoleColor bc = Console.BackgroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.BackgroundColor = ConsoleColor.Gray;

            Console.Write("\n1:Linear Regression  x:Exit");

            Console.ForegroundColor = fc;
            Console.BackgroundColor = bc;
        }

        static void WriteH1(string s)
        {
            ConsoleColor c = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s.ToUpper());
            Console.ForegroundColor = c;
        }

        static void WriteH2(string s)
        {
            ConsoleColor c = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(s);
            //Console.WriteLine(new String('-', 75));
            Console.ForegroundColor = c;
        }
    }
}
