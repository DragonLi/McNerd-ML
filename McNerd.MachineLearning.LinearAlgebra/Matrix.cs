﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McNerd.MachineLearning.LinearAlgebra
{
    /// <summary>
    /// Describe which dimension of a Matrix to work with.
    /// </summary>
    public enum MatrixDimensions { Auto, Rows, Columns }

    public class Matrix : IEnumerable
    {
        #region Delegates
        /// <summary>
        /// General purpose delegate for processing a number and giving
        /// a result.
        /// </summary>
        /// <param name="a">The number to process.</param>
        /// <returns>The result of performing an operation on the number.</returns>
        public delegate double ProcessNumber(double a);

        /// <summary>
        /// General purpose delegate for processing two numbers and giving
        /// a result.
        /// </summary>
        /// <param name="a">The first number to process.</param>
        /// <param name="b">The second number to process.</param>
        /// <returns>The result of performing an operation on both inputs.</returns>
        public delegate double ProcessNumbers(double a, double b);

        /// <summary>
        /// General purpose delegate for processing a Matrix and giving
        /// a result.
        /// </summary>
        /// <param name="a">The Matrix to process.</param>
        /// <returns>The result of performing an operation on the Matrix.</returns>
        public delegate double ProcessMatrix(Matrix a);

        #endregion

        #region Private Fields
        /// <summary>
        /// Storage array for the Matrix data.
        /// </summary>
        double[] data;

        /// <summary>
        /// Dimensions of the Matrix
        /// </summary>
        int rows, columns;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor to create a new Matrix while specifying the number of
        /// rows and columns.
        /// </summary>
        /// <param name="rows">The number of rows to initialise the Matrix with.</param>
        /// <param name="cols">The number of columns to initialise the Matrix with.</param>
        public Matrix(int rows, int columns)
        {
            this.rows = rows;
            this.columns = columns;
            data = new double[rows * columns];
        }

        /// <summary>
        /// Constructor to create a new square Matrix.
        /// </summary>
        /// <param name="dimensions">The number of rows and columns to initialise the
        /// Matrix with. There will be an equal number of rows and columns.</param>
        public Matrix(int dimensions) : this(dimensions, dimensions)
        {
        }

        public Matrix(double[,] array) : this(array.GetLength(0), array.GetLength(1))
        {
            int index = 0;
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    data[index++] = array[row, column];
                }
            }
        }

        public Matrix(Matrix m) : this(m.Rows, m.Columns)
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = m.data[i];
        }
        #endregion

        #region Indexers
        /// <summary>
        /// Indexer to easily access a specific location in this Matrix.
        /// </summary>
        /// <param name="row">The row of the Matrix location to access.</param>
        /// <param name="column">The column of the Matrix location to access.</param>
        /// <returns>The value stored at the given row/column location.</returns>
        /// <remarks>Matrices are zero-indexed.</remarks>
        public double this[int row, int column]
        {
            get { return data[(row * Columns) + column]; }
            set { data[(row * Columns) + column] = value; }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Indicates whether or not this Matrix row and column dimensions are equal.
        /// </summary>
        public bool IsSquare => rows == columns;

        /// <summary>
        /// Get the dimensions of this Matrix in a single-dimensional array of the form
        /// [rows,columns].
        /// </summary>
        public int[] Dimensions => new int[] { rows, columns };

        /// <summary>
        /// Get the number of rows in this Matrix.
        /// </summary>
        public int Rows => rows;

        /// <summary>
        /// Get the number of columns in this Matrix.
        /// </summary>
        public int Columns => columns;

        /// <summary>
        /// Get the transposed version of this Matrix (swap rows and columns)
        /// </summary>
        public Matrix Transpose
        {
            get
            {
                Matrix t = new Matrix(Columns, Rows);
                for (int index = 0; index < data.Length; index++)
                {
                    int i = index / Rows;
                    int j = index % Rows;
                    t.data[index] = data[(Columns * j) + i];
                }
                return t;
            }
        }

        /// <summary>
        /// Calculate the inverse of this Matrix.
        /// </summary>
        public Matrix Inverse
        {
            get
            {
                if (!IsSquare)
                    throw new InvalidMatrixDimensionsException("Inverse requires a Matrix to be square.");
                Matrix MResult = Matrix.Identity(Rows);

                for (int diagonal = 0; diagonal < Rows; diagonal++)
                {
                    double diagonalValue = this[diagonal, diagonal];

                    // Ensure the diagonal value is not zero by swapping another row if necessary.
                    if (diagonalValue == 0)
                    {
                        for (int i = 0; i < Rows; i++)
                        {
                            if (i != diagonal && this[i, diagonal] != 0 && this[diagonal, i] != 0)
                            {
                                this.SwapRows(diagonal, i);
                                MResult.SwapRows(diagonal, i);
                                diagonalValue = this[diagonal, diagonal];
                                break;
                            }
                        }
                        if (diagonalValue == 0)
                            throw new NonInvertibleMatrixException("This Matrix is not invertible");
                    }

                    int lineValueIndex = diagonal;
                    int itemIndex = 0;
                    int diagonalIndex = diagonal * this.Columns;

                    for (int row = 0; row < Rows; row++)
                    {
                        if (row != diagonal)
                        {
                            double lineValue = this.data[lineValueIndex];
                            for (int column = 0; column < Columns; column++)
                            {
                                int diagonalColumnIndex = diagonalIndex + column;
                                this.data[itemIndex] = (this.data[itemIndex] * diagonalValue) - (this.data[diagonalColumnIndex] * lineValue);
                                MResult.data[itemIndex] = (MResult.data[itemIndex] * diagonalValue) - (MResult.data[diagonalColumnIndex] * lineValue);
                                itemIndex++;
                            }
                        }
                        else
                        {
                            itemIndex += this.Columns;
                        }
                        lineValueIndex += this.Columns;
                    }
                }

                // By now all the rows should be filled in...
                int indexResult = 0;
                int indexThis = 0;

                for (int i = 0; i < Rows; i++)
                {
                    double divisor = this.data[indexThis];
                    indexThis += this.Columns + 1;

                    for (int j = 0; j < Columns; j++)
                    {
                        MResult.data[indexResult++] /= divisor;
                    }
                }

                return MResult;
            }
        }

        /// <summary>
        /// Determine if this Matrix is a Magic Square.
        /// </summary>
        public bool IsMagic
        {
            get
            {
                if (Columns != Rows) return false;
                if (Columns == 2) return false;

                double sum = 0;
                double diagonalSum = 0;
                double reverseDiagonalSum = 0;
                var usedElements = new Dictionary<double, double>();

                for (int i = 0; i < Rows; i++)
                {
                    double rowSum = 0;
                    double columnSum = 0;

                    for (int j = 0; j < Columns; j++)
                    {
                        double thisElement = this[i, j];

                        // If we're reading the first row, we just calculate what all the other
                        // rows and columns should add up to.
                        if (i == 0)
                        {
                            sum += thisElement;
                        }
                        rowSum += thisElement;
                        columnSum += this[j, i];

                        if (!usedElements.ContainsKey(thisElement))
                            usedElements[thisElement] = 1;
                        else
                            return false; // This Matrix does not contain distinct numbers
                    }
                    if (rowSum != sum || columnSum != sum) return false;

                    diagonalSum += this[i, i];
                    reverseDiagonalSum += this[i, Columns - i - 1];
                }

                return reverseDiagonalSum == sum && diagonalSum == sum;
            }
        }

        /// <summary>
        /// Calculate the sum of all elements in this Matrix.
        /// </summary>
        public double SumAllElements
        {
            get
            {
                double output = 0;
                foreach(double element in this)
                {
                    output += element;
                }
                return output;
            }
        }

        /// <summary>
        /// Return a Matrix representing all the columns transformed into one
        /// long column. i.e. : column 1, followed by column 2, etc.
        /// </summary>
        public Matrix Unrolled
        {
            get
            {
                Matrix tm = this.Transpose;
                Matrix output = new Matrix(this.data.Length, 1);

                for (int i = 0; i < tm.data.Length; i++)
                {
                    output.data[i] = tm.data[i];
                }
                return output;
            }
        }
        #endregion

        #region Operations
        /// <summary>
        /// Add two matrices together.
        /// </summary>
        /// <param name="m1">The first Matrix to add.</param>
        /// <param name="m2">The second Matrix to add.</param>
        /// <returns>The result of adding the two matrices together.</returns>
        /// <exception cref="InvalidMatrixDimensionsException">Thrown when both matrices have
        /// different dimensions.</exception>
        public static Matrix operator +(Matrix m1, Matrix m2)
        {
            if (m1.HasSameDimensions(m2))
            {
                Matrix output = new Matrix(m1.rows, m1.columns);
                for (int i = 0; i < m1.data.Length; i++)
                {
                    output.data[i] = m1.data[i] + m2.data[i];
                }
                return output;
            }
            else
            {
                throw new InvalidMatrixDimensionsException("Cannot add two Matrix objects whose dimensions do not match.");
            }
        }

        /// <summary>
        /// Add a number to each element in a Matrix.
        /// </summary>
        /// <param name="number">The number to add to each element in a Matrix.</param>
        /// <param name="m">The Matrix to add numbers to.</param>
        /// <returns>The result of adding the number to each element in a Matrix.</returns>
        public static Matrix operator +(double scalar, Matrix m)
        {
            Matrix output = new Matrix(m.rows, m.columns);
            for (int i = 0; i < m.data.Length; i++)
            {
                output.data[i] = scalar + m.data[i];
            }
            return output;
        }

        /// <summary>
        /// Add a number to each element in a Matrix.
        /// </summary>
        /// <param name="number">The number to add to each element in a Matrix.</param>
        /// <param name="m">The Matrix to add numbers to.</param>
        /// <returns>The result of adding the number to each element in a Matrix.</returns>
        public static Matrix operator +(Matrix m, double scalar)
        {
            return scalar + m;
        }

        /// <summary>
        /// Unary negative operator.
        /// </summary>
        /// <param name="m1">The Matrix to negate.</param>
        /// <returns>The result of negating every element in the given Matrix.</returns>
        public static Matrix operator -(Matrix m1)
        {
            Matrix output = new Matrix(m1.rows, m1.columns);
            for (int i = 0; i < m1.data.Length; i++)
            {
                output.data[i] = -m1.data[i];
            }
            return output;
        }

        /// <summary>
        /// Subtract one Matrix from another.
        /// </summary>
        /// <param name="m1">The first Matrix to subtract from.</param>
        /// <param name="m2">The second Matrix to subtract from the first.</param>
        /// <returns>The result of subtracting the second Matrix from the first.</returns>
        /// <exception cref="InvalidMatrixDimensionsException">Thrown when both matrices have
        /// different dimensions.</exception>
        public static Matrix operator -(Matrix m1, Matrix m2)
        {
            if (m1.HasSameDimensions(m2))
            {
                Matrix output = new Matrix(m1.rows, m1.columns);
                for (int i = 0; i < m1.data.Length; i++)
                {
                    output.data[i] = m1.data[i] - m2.data[i];
                }
                return output;
            }
            else
            {
                throw new InvalidMatrixDimensionsException("Cannot subtract two Matrix objects whose dimensions do not match.");
            }
        }

        /// <summary>
        /// Subtract each element in a Matrix from a number.
        /// </summary>
        /// <param name="number">The number to subtract each element in a Matrix from.</param>
        /// <param name="m">The Matrix to subtract from the number.</param>
        /// <returns>The result of subracting each element from a given number.</returns>
        public static Matrix operator -(double scalar, Matrix m)
        {
            Matrix output = new Matrix(m.rows, m.columns);
            for (int i = 0; i < m.data.Length; i++)
            {
                output.data[i] = scalar - m.data[i];
            }
            return output;
        }

        /// <summary>
        /// Subtract a number from each element in a Matrix.
        /// </summary>
        /// <param name="number">The number to subtract from each element in a Matrix.</param>
        /// <param name="m">The Matrix to subtract the number from.</param>
        /// <returns>The result of subracting a number from each element in a given Matrix.</returns>
        public static Matrix operator -(Matrix m, double scalar)
        {
            Matrix output = new Matrix(m.rows, m.columns);
            for (int i = 0; i < m.data.Length; i++)
            {
                output.data[i] = m.data[i] - scalar;
            }
            return output;
        }


        /// <summary>
        /// Multiply two matrices together.
        /// </summary>
        /// <param name="m1">An nxm dimension Matrix.</param>
        /// <param name="m2">An mxp dimension Matrix.</param>
        /// <returns>An nxp Matrix that is the product of m1 and m2.</returns>
        /// <exception cref="InvalidMatrixDimensionsException">Thrown when the number of columns in the
        /// first Matrix don't match the number of rows in the second Matrix.</exception>
        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            if (m1.columns == m2.rows)
            {
                Matrix output = new Matrix(m1.rows, m2.columns);
                Parallel.For(0, m1.rows, i => MultiplyRow(i, m1, m2, ref output));
                return output;
            }
            else
            {
                throw new InvalidMatrixDimensionsException("Multiplication cannot be performed on matrices with these dimensions.");
            }
        }

        /// <summary>
        /// Scalar multiplication of a Matrix.
        /// </summary>
        /// <param name="scalar">The scalar value to multiply each element of the Matrix by.</param>
        /// <param name="m">The Matrix to apply multiplication to.</param>
        /// <returns>A Matrix representing the scalar multiplication of scalar * m.</returns>
        public static Matrix operator *(double scalar, Matrix m)
        {
            Matrix output = new Matrix(m.rows, m.columns);
            //for (int i = 0; i < m.data.Length; i++)
            //    output.data[i] = m.data[i] * scalar;
            //Parallel.For(0, m.data.Length, i => { output.data[i] = scalar * m.data[i]; });
            Parallel.For(0, m.rows, i => MultiplyRow(i, m, scalar, ref output));
            return output;
        }

        /// <summary>
        /// Scalar multiplication of a Matrix.
        /// </summary>
        /// <param name="m">The Matrix to apply multiplication to.</param>
        /// <param name="scalar">The scalar value to multiply each element of the Matrix by.</param>
        /// <returns>A Matrix representing the scalar multiplication of m * scalar.</returns>
        public static Matrix operator *(Matrix m, double scalar)
        {
            // Same as above, but ensuring commutativity - i.e. (s * m) == (m * s).
            return scalar * m;
        }

        /// <summary>
        /// Scalar division of a Matrix.
        /// </summary>
        /// <param name="scalar">The scalar value to divide each element of the Matrix by.</param>
        /// <param name="m">The Matrix to apply division to.</param>
        /// <returns>A Matrix representing the scalar division of scalar / m.</returns>
        public static Matrix operator /(double scalar, Matrix m)
        {
            Matrix output = new Matrix(m.rows, m.columns);
            Parallel.For(0, m.rows, i => DivideScalarByRow(i, m, scalar, ref output));
            return output;
        }

        /// <summary>
        /// Scalar division of a Matrix.
        /// </summary>
        /// <param name="m">The Matrix to apply division to.</param>
        /// <param name="scalar">The scalar value to division each element of the Matrix by.</param>
        /// <returns>A Matrix representing the scalar division of m / scalar.</returns>
        public static Matrix operator /(Matrix m, double scalar)
        {
            Matrix output = new Matrix(m.rows, m.columns);
            Parallel.For(0, m.rows, i => DivideRow(i, m, scalar, ref output));
            return output;
        }

        /// <summary>
        /// Override the == operator to compare Matrix values.
        /// </summary>
        /// <param name="m1">The first Matrix to compare.</param>
        /// <param name="m2">The second Matrix to compare.</param>
        /// <returns>True if the values of both matrices match.</returns>
        public static bool operator ==(Matrix m1, Matrix m2)
        {
            return m1.Equals(m2);
        }

        /// <summary>
        /// Override the != operator to compare Matrix values.
        /// </summary>
        /// <param name="m1">The first Matrix to compare.</param>
        /// <param name="m2">The second Matrix to compare.</param>
        /// <returns>True if the values of both matrices differ.</returns>
        public static bool operator !=(Matrix m1, Matrix m2)
        {
            return !(m1 == m2);
        }

        /// <summary>
        /// Create a Matrix truth table containing 1 and 0 representing
        /// values that are equal to the given scalar.
        /// </summary>
        /// <param name="m">The Matrix to compare to the scalar value.</param>
        /// <param name="scalar">A scalar value used for comparison.</param>
        /// <returns>A Matrix with 1s and 0s representing true and false for the
        /// comparison.</returns>
        public static Matrix operator ==(Matrix m, double scalar)
        {
            Matrix output = new Matrix(m.rows, m.columns);
            Parallel.For(0, m.rows, i => EqualToRow(i, m, scalar, ref output));
            return output;
        }


        /// <summary>
        /// Create a Matrix truth table containing 1 and 0 representing
        /// values that are not equal to the given scalar.
        /// </summary>
        /// <param name="m">The Matrix to compare to the scalar value.</param>
        /// <param name="scalar">A scalar value used for comparison.</param>
        /// <returns>A Matrix with 1s and 0s representing true and false for the
        /// comparison.</returns>
        public static Matrix operator !=(Matrix m, double scalar)
        {
            Matrix output = new Matrix(m.rows, m.columns);
            Parallel.For(0, m.rows, i => NotEqualToRow(i, m, scalar, ref output));
            return output;
        }

        /// <summary>
        /// Create a Matrix truth table containing 1 and 0 representing
        /// values that are less than the given scalar.
        /// </summary>
        /// <param name="m">The Matrix to compare to the scalar value.</param>
        /// <param name="scalar">A scalar value used for comparison.</param>
        /// <returns>A Matrix with 1s and 0s representing true and false for the
        /// comparison.</returns>
        public static Matrix operator <(Matrix m, double scalar)
        {
            Matrix output = new Matrix(m.rows, m.columns);
            Parallel.For(0, m.rows, i => LessThanRow(i, m, scalar, ref output));
            return output;
        }

        /// <summary>
        /// Create a Matrix truth table containing 1 and 0 representing
        /// values that are greater than the given scalar.
        /// </summary>
        /// <param name="m">The Matrix to compare to the scalar value.</param>
        /// <param name="scalar">A scalar value used for comparison.</param>
        /// <returns>A Matrix with 1s and 0s representing true and false for the
        /// comparison.</returns>
        public static Matrix operator >(Matrix m, double scalar)
        {
            Matrix output = new Matrix(m.rows, m.columns);
            Parallel.For(0, m.rows, i => GreaterThanRow(i, m, scalar, ref output));
            return output;
        }

        /// <summary>
        /// Create a Matrix truth table containing 1 and 0 representing
        /// values that are less than or equal to the given scalar.
        /// </summary>
        /// <param name="m">The Matrix to compare to the scalar value.</param>
        /// <param name="scalar">A scalar value used for comparison.</param>
        /// <returns>A Matrix with 1s and 0s representing true and false for the
        /// comparison.</returns>
        public static Matrix operator <=(Matrix m, double scalar)
        {
            Matrix output = new Matrix(m.rows, m.columns);
            Parallel.For(0, m.rows, i => LessThanOrEqualToRow(i, m, scalar, ref output));
            return output;
        }

        /// <summary>
        /// Create a Matrix truth table containing 1 and 0 representing
        /// values that are greater than or equal to the given scalar.
        /// </summary>
        /// <param name="m">The Matrix to compare to the scalar value.</param>
        /// <param name="scalar">A scalar value used for comparison.</param>
        /// <returns>A Matrix with 1s and 0s representing true and false for the
        /// comparison.</returns>
        public static Matrix operator >=(Matrix m, double scalar)
        {
            Matrix output = new Matrix(m.rows, m.columns);
            Parallel.For(0, m.rows, i => GreaterThanOrEqualToRow(i, m, scalar, ref output));
            return output;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Indicates if this Matrix has the same dimensions as another supplied Matrix.
        /// </summary>
        /// <param name="other">Another Matrix to compare this instance to.</param>
        /// <returns>true if both matrices have the same dimensions. Otherwise, false.</returns>
        public bool HasSameDimensions(Matrix other)
        {
            return (this.rows == other.rows) && (this.columns == other.columns);
        }

        /// <summary>
        /// Override the Object.Equals method to compare Matrix values.
        /// </summary>
        /// <param name="obj">The object to compare to this Matrix.</param>
        /// <returns>True if obj is a Matrix, and its values match the current
        /// Matrix values.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Matrix m = obj as Matrix;
            if (object.ReferenceEquals(null, m)) return false;
            if (ReferenceEquals(this, m)) return true;

            if (!this.HasSameDimensions(m)) return false;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    if (this[row, column] != m[row, column]) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Compare this Matrix with a second Matrix by value.
        /// </summary>
        /// <param name="m">The Matrix to compare to this one.</param>
        /// <returns>True if both matrices contain the same values.</returns>
        public bool Equals(Matrix m)
        {
            if (object.ReferenceEquals(null, m)) return false;
            if (ReferenceEquals(this, m)) return true;

            if (!this.HasSameDimensions(m)) return false;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    if (this[row, column] != m[row, column]) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Override the default hash code.
        /// </summary>
        /// <returns>A bitwise XOR based on rows and columns of this Matrix.</returns>
        public override int GetHashCode()
        {
            return rows ^ columns;
        }

        /// <summary>
        /// Convert this Matrix to a string.
        /// </summary>
        /// <returns>A string representation of this Matrix.</returns>
        /// <remarks>All elements are rounded to two decimal places.</remarks>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            int index = 0;
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    sb.AppendFormat("{0:0.00} ", data[index++]);
                }
                sb.Append("\n");
            }
            return sb.ToString();
        }

        #region Private row/column operations
        /// <summary>
        /// Calculate a single row result of multiplying two matrices.
        /// </summary>
        /// <param name="row">The zero-indexed row to calculate.</param>
        /// <param name="m1">The first Matrix to multiply.</param>
        /// <param name="m2">The second Matrix to multiply.</param>
        /// <param name="output">The Matrix to store the results in.</param>
        private static void MultiplyRow(int row, Matrix m1, Matrix m2, ref Matrix output)
        {
            int m1_index = row * m1.columns;
            int m2_index;

            for (int column = 0; column < output.Columns; column++)
            {
                double result = 0;
                m2_index = column;

                for (int i = 0; i < m1.Columns; i++)
                {
                    result += m1.data[m1_index + i] * m2.data[m2_index];
                    m2_index += m2.columns;
                }

                output[row, column] = result;

            }
        }

        /// <summary>
        /// Calculate the results of multiplying each element in a Matrix
        /// row by a scalar value.
        /// </summary>
        /// <param name="row">The zero-indexed row to calculate.</param>
        /// <param name="m">The Matrix to multiply by a scalar value.</param>
        /// <param name="scalar">The scalar value to multiply the Matrix by.</param>
        /// <param name="output">The Matrix that contains the results of multiplying the input
        /// Matrix by a scalar value.</param>
        private static void MultiplyRow(int row, Matrix m, double scalar, ref Matrix output)
        {
            int m_index = row * m.columns;

            for (int i = m_index; i < m_index + output.Columns; i++)
            {
                output.data[i] = scalar * m.data[i];
            }
        }

        /// <summary>
        /// Calculate a single row result of multiplying two matrices.
        /// </summary>
        /// <param name="row">The zero-indexed row from m1 to calculate.</param>
        /// <param name="m1">The first Matrix to multiply.</param>
        /// <param name="m2">The second Matrix to multiply.</param>
        /// <param name="output">The Matrix to store the results in.</param>
        private static void MultiplyByTransposedRow(int row, Matrix m1, Matrix m2, ref Matrix output)
        {
            int m1_index = row * m1.columns;
            int output_index = row * output.Columns;
            int m2_index = 0;

            for (int column = 0; column < output.Columns; column++)
            {
                double result = 0;

                for (int i = 0; i < m1.Columns; i++)
                {
                    result += m1.data[m1_index + i] * m2.data[m2_index++];
                }

                output.data[output_index++] = result;
            }
        }

        /// <summary>
        /// Calculate a single row result of multiplying row transposed into a column, by a corresponding
        /// row in a second Matrix.
        /// </summary>
        /// <param name="column">The zero-indexed row from m1 to transpose into a column.</param>
        /// <param name="m1">The first Matrix to multiply.</param>
        /// <param name="m2">The second Matrix to multiply.</param>
        /// <param name="output">The Matrix to store the results in.</param>
        private static void MultiplyByTransposedColumn(int column, Matrix m1, Matrix m2, ref Matrix output)
        {
            int output_index = column * output.Columns;
            int m1_index, m2_index;

            for (int m2Col = 0; m2Col < m2.Columns; m2Col++)
            {
                double result = 0;
                m1_index = column;
                m2_index = m2Col;

                for (int m2Row = 0; m2Row < m2.Rows; m2Row++)
                {
                    result += m1.data[m1_index] * m2.data[m2_index];
                    m1_index += m1.Columns;
                    m2_index += m2.Columns;
                }

                output.data[output_index++] = result;
            }
        }

        /// <summary>
        /// Calculate a single row result of multiplying one Matrix with its transpose.
        /// </summary>
        /// <param name="row">The zero-indexed row from m1 to calculate.</param>
        /// <param name="column">The zero-indexed column from m2 to calculate.</param>
        /// <param name="m1">The Matrix to multiply with its transpose.</param>
        /// <param name="output">The Matrix to store the results in.</param>
        private static void MultiplyByTransposedRow(int row, Matrix m1, ref Matrix output)
        {
            int m1_index = row * m1.columns;
            int output_index = row * output.Columns;
            int m2_index = 0;

            for (int column = 0; column < output.Columns; column++)
            {
                double result = 0;

                for (int i = 0; i < m1.Columns; i++)
                {
                    result += m1.data[m1_index + i] * m1.data[m2_index++];
                }

                output.data[output_index++] = result;
            }
        }

        /// <summary>
        /// Calculate a single row result of multiplying row transposed into a column, by a corresponding
        /// row in the original Matrix.
        /// </summary>
        /// <param name="column">The zero-indexed row from m1 to transpose into a column.</param>
        /// <param name="m1">The Matrix to transpose and multiply by the original.</param>
        /// <param name="output">The Matrix to store the results in.</param>
        private static void MultiplyByTransposedColumn(int column, Matrix m1, ref Matrix output)
        {
            MultiplyByTransposedColumn(column, m1, m1, ref output);
        }

        /// <summary>
        /// Calculate the results of dividing each element in a Matrix
        /// row by a scalar value.
        /// </summary>
        /// <param name="row">The zero-indexed row to calculate.</param>
        /// <param name="m">The Matrix to divide by a scalar value.</param>
        /// <param name="scalar">The scalar value to divide the Matrix by.</param>
        /// <param name="output">The Matrix that contains the results of dividing the input
        /// Matrix by a scalar value.</param>
        private static void DivideRow(int row, Matrix m, double scalar, ref Matrix output)
        {
            int m_index = row * m.columns;

            for (int i = m_index; i < m_index + output.Columns; i++)
            {
                output.data[i] = m.data[i] / scalar;
            }
        }

        /// <summary>
        /// Calculate the results of dividing a scalar value by each element
        /// in a Matrix.
        /// </summary>
        /// <param name="row">The zero-indexed row to calculate.</param>
        /// <param name="m">The Matrix to divide into a scalar value.</param>
        /// <param name="scalar">The scalar value to divide by the Matrix elements.</param>
        /// <param name="output">The Matrix that contains the results of dividing the scalar
        /// value by each element in the Matrix.</param>
        private static void DivideScalarByRow(int row, Matrix m, double scalar, ref Matrix output)
        {
            int m_index = row * m.columns;

            for (int i = m_index; i < m_index + output.Columns; i++)
            {
                output.data[i] = scalar / m.data[i];
            }
        }

        /// <summary>
        /// Calculate if each element in a row is greater than or equal to the given value, creating
        /// a 1.0 if true, or 0.0 otherwise.
        /// </summary>
        /// <param name="row">The zero-indexed row to calculate.</param>
        /// <param name="m">The Matrix to compare to the scalar value.</param>
        /// <param name="scalar">The scalar value to compare to the Matrix elements.</param>
        /// <param name="output">The Matrix that contains the comparison results.</param>
        private static void GreaterThanOrEqualToRow(int row, Matrix m, double scalar, ref Matrix output)
        {
            int m_index = row * m.columns;

            for (int i = m_index; i < m_index + output.Columns; i++)
            {
                output.data[i] = m.data[i] >= scalar ? 1.0 : 0.0;
            }
        }

        /// <summary>
        /// Calculate if each element in a row is greater than the given value, creating
        /// a 1.0 if true, or 0.0 otherwise.
        /// </summary>
        /// <param name="row">The zero-indexed row to calculate.</param>
        /// <param name="m">The Matrix to compare to the scalar value.</param>
        /// <param name="scalar">The scalar value to compare to the Matrix elements.</param>
        /// <param name="output">The Matrix that contains the comparison results.</param>
        private static void GreaterThanRow(int row, Matrix m, double scalar, ref Matrix output)
        {
            int m_index = row * m.columns;

            for (int i = m_index; i < m_index + output.Columns; i++)
            {
                output.data[i] = m.data[i] > scalar ? 1.0 : 0.0;
            }
        }

        /// <summary>
        /// Calculate if each element in a row is less than or equal to the given value, creating
        /// a 1.0 if true, or 0.0 otherwise.
        /// </summary>
        /// <param name="row">The zero-indexed row to calculate.</param>
        /// <param name="m">The Matrix to compare to the scalar value.</param>
        /// <param name="scalar">The scalar value to compare to the Matrix elements.</param>
        /// <param name="output">The Matrix that contains the comparison results.</param>
        private static void LessThanOrEqualToRow(int row, Matrix m, double scalar, ref Matrix output)
        {
            int m_index = row * m.columns;

            for (int i = m_index; i < m_index + output.Columns; i++)
            {
                output.data[i] = m.data[i] <= scalar ? 1.0 : 0.0;
            }
        }

        /// <summary>
        /// Calculate if each element in a row is less than the given value, creating
        /// a 1.0 if true, or 0.0 otherwise.
        /// </summary>
        /// <param name="row">The zero-indexed row to calculate.</param>
        /// <param name="m">The Matrix to compare to the scalar value.</param>
        /// <param name="scalar">The scalar value to compare to the Matrix elements.</param>
        /// <param name="output">The Matrix that contains the comparison results.</param>
        private static void LessThanRow(int row, Matrix m, double scalar, ref Matrix output)
        {
            int m_index = row * m.columns;

            for (int i = m_index; i < m_index + output.Columns; i++)
            {
                output.data[i] = m.data[i] < scalar ? 1.0 : 0.0;
            }
        }

        /// <summary>
        /// Calculate if each element in a row is equal to the given value, creating
        /// a 1.0 if true, or 0.0 otherwise.
        /// </summary>
        /// <param name="row">The zero-indexed row to calculate.</param>
        /// <param name="m">The Matrix to compare to the scalar value.</param>
        /// <param name="scalar">The scalar value to compare to the Matrix elements.</param>
        /// <param name="output">The Matrix that contains the comparison results.</param>
        private static void EqualToRow(int row, Matrix m, double scalar, ref Matrix output)
        {
            int m_index = row * m.columns;

            for (int i = m_index; i < m_index + output.Columns; i++)
            {
                output.data[i] = m.data[i] == scalar ? 1.0 : 0.0;
            }
        }

        /// <summary>
        /// Calculate if each element in a row is not equal to the given value, creating
        /// a 1.0 if true, or 0.0 otherwise.
        /// </summary>
        /// <param name="row">The zero-indexed row to calculate.</param>
        /// <param name="m">The Matrix to compare to the scalar value.</param>
        /// <param name="scalar">The scalar value to compare to the Matrix elements.</param>
        /// <param name="output">The Matrix that contains the comparison results.</param>
        private static void NotEqualToRow(int row, Matrix m, double scalar, ref Matrix output)
        {
            int m_index = row * m.columns;

            for (int i = m_index; i < m_index + output.Columns; i++)
            {
                output.data[i] = m.data[i] != scalar ? 1.0 : 0.0;
            }
        }

        #endregion

        #region Multiplying Transpose
        /// <summary>
        /// Multiply one Matrix by the transpose of the other.
        /// </summary>
        /// <param name="m1">The first Matrix to multiply.</param>
        /// <param name="m2">The Matrix to transpose and multiply.</param>
        /// <returns>The result of multiplying m1 with m2.Transpose.</returns>
        public static Matrix MultiplyByTranspose(Matrix m1, Matrix m2)
        {
            if (m1.Columns == m2.Columns)
            {
                Matrix output = new Matrix(m1.Rows, m2.Rows);
                Parallel.For(0, m1.Rows, i => MultiplyByTransposedRow(i, m1, m2, ref output));
                return output;
            }
            else
            {
                throw new InvalidMatrixDimensionsException("Multiplication cannot be performed on matrices with these dimensions.");
            }
        }

        /// <summary>
        /// Multiply a Matrix by its transpose.
        /// </summary>
        /// <param name="m1">The Matrix to multiply by its transpose.</param>
        /// <returns>The result of multiplying m1 with its transpose.</returns>
        public static Matrix MultiplyByTranspose(Matrix m1)
        {
            Matrix output = new Matrix(m1.Rows, m1.Rows);
            Parallel.For(0, m1.Rows, i => MultiplyByTransposedRow(i, m1, ref output));
            return output;
        }

        /// <summary>
        /// Multiply the Transpose of one Matrix by another Matrix.
        /// </summary>
        /// <param name="m1">The Matrix to transpose and multiply.</param>
        /// <param name="m2">The Matrix to multiply the Transpose of m1 by.</param>
        /// <returns>The result of multiplying m1.Transpose with m2.</returns>
        public static Matrix MultiplyTransposeBy(Matrix m1, Matrix m2)
        {
            if (m1.Rows == m2.Rows)
            {
                Matrix output = new Matrix(m1.Columns, m2.Columns);
                Parallel.For(0, m1.Columns, i => MultiplyByTransposedColumn(i, m1, m2, ref output));
                return output;
            }
            else
            {
                throw new InvalidMatrixDimensionsException("Multiplication cannot be performed on matrices with these dimensions.");
            }
        }

        /// <summary>
        /// Multiply the Transpose of a Matrix by the original Matrix.
        /// </summary>
        /// <param name="m1">The Matrix to transpose, and multiply by itself.</param>
        /// <returns>The result of multiplying m1.Transpose with m1.</returns>
        public static Matrix MultiplyTransposeBy(Matrix m1)
        {
            Matrix output = new Matrix(m1.Columns, m1.Columns);
            Parallel.For(0, m1.Columns, i => MultiplyByTransposedColumn(i, m1, ref output));
            return output;
        }
        #endregion

        #region Row / Column methods

        /// <summary>
        /// Swap two rows in this Matrix.
        /// </summary>
        /// <param name="row1">The first row to swap.</param>
        /// <param name="row2">The second row to swap.</param>
        public void SwapRows(int row1, int row2)
        {
            double[] tmp = new double[Columns];
            int indexRow1 = row1 * Columns;
            int indexRow2 = row2 * Columns;

            if (indexRow1 > data.Length || indexRow2 > data.Length)
                throw new IndexOutOfRangeException("SwapRow method called with non-existent rows.");

            for (int i = 0; i < Columns; i++)
            {
                tmp[i] = data[indexRow1 + i];
                data[indexRow1 + i] = data[indexRow2 + i];
                data[indexRow2 + i] = tmp[i];
            }
        }

        /// <summary>
        /// Join two Matrix objects together, side by side, or one above another.
        /// </summary>
        /// <param name="m1">The first Matrix to join.</param>
        /// <param name="m2">The second Matrix to join.</param>
        /// <param name="dimension">The dimensions to join them on.</param>
        /// <returns>A new Matrix containing both the original Matrices joined together.</returns>
        public static Matrix Join(Matrix m1, Matrix m2, MatrixDimensions dimension = MatrixDimensions.Auto)
        {
            Matrix result = null;
            switch (dimension)
            {
                case MatrixDimensions.Columns:
                    if (m1.Rows != m2.Rows)
                        throw new InvalidMatrixDimensionsException("Matrices cannot be joined as they don't have the same number of rows.");
                    result = new Matrix(m1.Rows, m1.Columns + m2.Columns);
                    int index = 0, indexM1 = 0, indexM2 = 0;
                    for (int row = 0; row < m1.Rows; row++)
                    {
                        for (int column = 0; column < m1.Columns; column++)
                        {
                            result.data[index++] = m1.data[indexM1++];
                        }
                        for (int column = 0; column < m2.Columns; column++)
                        {
                            result.data[index++] = m2.data[indexM2++];
                        }
                    }
                    break;
                case MatrixDimensions.Rows:
                    if (m1.Columns != m2.Columns)
                        throw new InvalidMatrixDimensionsException("Matrices cannot be joined as they don't have the same number of columns.");
                    result = new Matrix(m1.Rows + m2.Rows, m1.Columns);
                    for (int i = 0; i < m1.data.Length; i++)
                    {
                        result.data[i] = m1.data[i];
                    }
                    for (int i = 0; i < m2.data.Length; i++)
                    {
                        result.data[i + m1.data.Length] = m2.data[i];
                    }
                    break;
                case MatrixDimensions.Auto:
                    if (m1.Rows == m2.Rows)
                        goto case MatrixDimensions.Columns;
                    else
                        goto case MatrixDimensions.Rows;
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// Add a column of a particular value at the start of a given Matrix.
        /// </summary>
        /// <param name="m1">The Matrix to add the identity column to.</param>
        /// <param name="identityValue">The identity value, default 1.</param>
        /// <returns>A new Matrix with an addition column at the start.</returns>
        public static Matrix AddIdentityColumn(Matrix m1, double identityValue = 1)
        {
            Matrix m2 = new Matrix(m1.Rows, m1.Columns + 1);

            int dataIndex1 = 0;
            int dataIndex2 = 0;

            for (int row = 0; row < m1.Rows; row++)
            {
                m2.data[dataIndex2++] = identityValue;
                for (int column = 0; column < m1.Columns; column++)
                {
                    m2.data[dataIndex2++] = m1.data[dataIndex1++];
                }
            }

            return m2;
        }

        /// <summary>
        /// Extract a row from this Matrix.
        /// </summary>
        /// <param name="row">The zero-index row to extract.</param>
        /// <returns>A row-vector form Matrix.</returns>
        public Matrix GetRow(int row)
        {
            if (row >= this.Rows)
                throw new IndexOutOfRangeException("The requested row is out of range");
            Matrix result = new Matrix(1, this.Columns);

            int index = row * this.Columns;
            for (int i = 0; i < this.Columns; i++)
            {
                result.data[i] = this.data[index + i];
            }

            return result;
        }

        /// <summary>
        /// Replace a given row with the contents of the first row in another
        /// Matrix.
        /// </summary>
        /// <param name="row">The row to change.</param>
        /// <param name="m">The Matrix to get new values from.</param>
        public void SetRow(int row, Matrix m)
        {
            if (row >= this.Rows)
                throw new IndexOutOfRangeException("The requested row is out of range");
            int index = row * this.Columns;
            for (int i = 0; i < Math.Min(this.Columns, m.Columns); i++)
            {
                this.data[index + i] = m.data[i];
            }
        }

        /// <summary>
        /// Extract a column from this Matrix.
        /// </summary>
        /// <param name="column">The zero-index column to extract.</param>
        /// <returns>A column-vector form Matrix.</returns>
        public Matrix GetColumn(int column)
        {
            if (column >= this.Columns)
                throw new IndexOutOfRangeException("The requested column is out of range");
            Matrix result = new Matrix(this.Rows, 1);

            int index = column;
            for (int i = 0; i < this.Rows; i++)
            {
                result.data[i] = this.data[index];
                index += this.Columns;
            }

            return result;
        }

        /// <summary>
        /// Remove a given column from a Matrix.
        /// </summary>
        /// <param name="column">The zero-based index of the column to remove.</param>
        /// <returns>A new Matrix containing the contents of the original Matrix
        /// without the specified column.</returns>
        public Matrix RemoveColumn(int column = 0)
        {
            if (column >= this.Columns)
                throw new IndexOutOfRangeException("The requested column is out of range");
            if (this.Columns == 1)
                throw new InvalidMatrixDimensionsException("Cannot remove column as there is only one column to begin with");

            Matrix result = new Matrix(this.Rows, this.Columns-1);
            int dataIndex = 0, resultIndex = 0;
            for (int i = 0; i < this.Rows; i++)
            {
                for (int j = 0; j < this.Columns; j++)
                {
                    if (j != column)
                    {
                        result.data[resultIndex++] = this.data[dataIndex++];
                    }
                    else
                    {
                        dataIndex++;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Expands two columns in a Matrix to a series of polynomial features.
        /// </summary>
        /// <param name="column1">The zero-index of the first column to expand.</param>
        /// <param name="column2">The zero-index of the second column to expand.</param>
        /// <param name="degree">The number of polynomial degrees to expand the columns to.</param>
        /// <returns>A copy of the original Matrix with column1 replaced with X1, X2, X1^2,
        /// X2^2, X1*X2, X1*X2^2, etc</returns>
        public Matrix ExpandPolynomials(int column1, int column2, int degree)
        {
            if (column1 >= this.Columns || column2 >= this.Columns)
                throw new IndexOutOfRangeException("A requested column is out of range");

            int outputColumns = (((degree + 1) * (degree + 2)) / 2);
            outputColumns += (this.Columns - 2);
            Matrix result = new Matrix(this.Rows, outputColumns);

            for (int row = 0; row < this.Rows; row++)
            {
                int colIndex = 0;
                double value1 = this[row, column1];
                double value2 = this[row, column2];

                for (int col = 0; col < this.Columns; col++)
                {
                    if (col == column1)
                    {
                        for (int i = 0; i <= degree; i++)
                        {
                            for (int j = 0; j <= i; j++)
                            {
                                double component1 = Math.Pow(value1, (double)(i - j));
                                double component2 = Math.Pow(value2, (double)j);
                                result[row, colIndex++] = component1 * component2;
                            }
                        }
                    }
                    else if (col != column2)
                    {
                        result[row, colIndex++] = this[row, col];
                    }
                }
            }

            return result;
        }
        #endregion

        /// <summary>
        /// Fills the Matrix with a given number.
        /// </summary>
        /// <param name="number">The number to assign to every element in the Matrix.</param>
        public void Fill(double number)
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = number;
        }

        #region Matrix creation methods
        /// <summary>
        /// Create an identity Matrix
        /// </summary>
        /// <param name="dimensions">The number of rows and columns for this Matrix.</param>
        /// <returns>A square Matrix with zeros everywhere, except for the main diagonal which is filled with ones.</returns>
        public static Matrix Identity(int dimensions)
        {
            Matrix Midentity = new Matrix(dimensions, dimensions);
            int index = 0;
            while (index < Midentity.data.Length)
            {
                Midentity.data[index] = 1;
                index += dimensions + 1;
            }

            return Midentity;
        }

        /// <summary>
        /// Create a rows*columns size Matrix filled with 1's.
        /// </summary>
        /// <param name="rows">The number of rows to initialise the Matrix with.</param>
        /// <param name="columns">The number of columns to initialise the Matrix with.</param>
        /// <returns>A Matrix object filled with 1's.</returns>
        public static Matrix Ones(int rows, int columns)
        {
            Matrix result = new Matrix(rows, columns);
            result.Fill(1.0);
            return result;
        }

        /// <summary>
        /// Create a square Matrix filled with 1's.
        /// </summary>
        /// <param name="dimensions">The number of rows and columns to initialise the
        /// Matrix with. There will be an equal number of rows and columns.</param>
        /// <returns>A square Matrix object filled with 1's.</returns>
        public static Matrix Ones(int dimension)
        {
            Matrix result = new Matrix(dimension);
            result.Fill(1.0);
            return result;
        }

        /// <summary>
        /// Create a Magic Square for odd-numbered dimensions greater than 1.
        /// </summary>
        /// <param name="dimension">The dimension to use to create the Magic Square.</param>
        /// <returns>A Magic Square of the required dimensions.</returns>
        private static Matrix MagicSquareOdd(int dimension)
        {
            if (dimension <= 1)
                throw new InvalidMatrixDimensionsException("Dimensions must be greater than or equal to one.");

            if (dimension % 2 != 1)
                throw new InvalidMatrixDimensionsException("Dimensions must be an odd number.");

            Matrix output = new Matrix(dimension, dimension);

            // Set the first value and initialize current position to
            // halfway across the first row.
            int startColumn = dimension >> 1;
            int startRow = 0;
            output[startRow, startColumn] = 1;

            // Keep moving up and to the right until all squares are filled
            int newRow, newColumn;

            for (int i = 2; i <= dimension * dimension; i++)
            {
                newRow = startRow - 1; newColumn = startColumn + 1;
                if (newRow < 0) newRow = dimension - 1;
                if (newColumn >= dimension) newColumn = 0;

                if (output[newRow, newColumn] > 0)
                {
                    while (output[startRow, startColumn] > 0)
                    {
                        startRow++;
                        if (startRow >= dimension) startRow = 0;
                    }
                }
                else
                {
                    startRow = newRow; startColumn = newColumn;
                }
                output[startRow, startColumn] = i;
            }
            return output;
        }

        /// <summary>
        /// Create a Magic Square, where the numbers of each row, column and diagonal
        /// add up to the same number.
        /// </summary>
        /// <param name="dimensions">The number of rows and columns to initialise the
        /// Matrix with. There will be an equal number of rows and columns.</param>
        /// <returns>A Magic Square Matrix.</returns>
        public static Matrix Magic(int dimension)
        {
            // Handle special cases first
            if (dimension == 1)
                return Matrix.Identity(1);
            if (dimension == 2)
                throw new InvalidMatrixDimensionsException("A Magic Square cannot have a dimension of 2");

            // Handle odd-numbered dimensions first
            if (dimension % 2 == 1)
            {
                return (MagicSquareOdd(dimension));
            }

            // Handle 'doubly-even' dimensions (divisible by 4)
            if (dimension % 4 == 0)
            {
                Matrix doubleEven = new Matrix(dimension, dimension);

                double index = 1;
                for (int i = 0, r = dimension - 1; i < dimension; i++, r--)
                {
                    for (int j = 0, c = dimension - 1; j < dimension; j++, c--)
                    {
                        // Fill in the diagonals
                        if (i == j || (j + i + 1) == dimension)
                        {
                            doubleEven[i, j] = index;
                        }
                        else
                        {
                            // Otherwise, fill in diagonally opposite element
                            doubleEven[r, c] = index;
                        }
                        index++;
                    }
                }

                return doubleEven;
            }

            // Other even dimensions (divisible by 2, but not 4) using Strachey's method.
            int k = dimension;
            int n = (k - 2) / 4;
            int qDimension = k / 2;
            int qElementCount = qDimension * qDimension;
            Matrix A = MagicSquareOdd(qDimension);
            Matrix B = Matrix.ElementAdd(A, qElementCount);
            Matrix C = Matrix.ElementAdd(B, qElementCount);
            Matrix D = Matrix.ElementAdd(C, qElementCount);

            // Exchange first n columns in A with n columns in D
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < A.Rows; j++)
                {
                    double tmp = D[j, i];
                    D[j, i] = A[j, i];
                    A[j, i] = tmp;
                }
            }

            // Exchange right-most n-1 columns in C with B.
            for (int i = C.Columns - n + 1; i < C.Columns; i++)
            {
                for (int j = 0; j < A.Rows; j++)
                {
                    double tmp = C[j, i];
                    C[j, i] = B[j, i];
                    B[j, i] = tmp;
                }
            }

            // Exchange Middle and Centre-Middle squares in D with A
            int middle = (qDimension / 2);
            double swap = D[middle, 0]; D[middle, 0] = A[middle, 0]; A[middle, 0] = swap;
            swap = D[middle, middle]; D[middle, middle] = A[middle, middle]; A[middle, middle] = swap;


            Matrix row1 = Matrix.Join(A, C, MatrixDimensions.Columns);
            Matrix row2 = Matrix.Join(D, B, MatrixDimensions.Columns);

            Matrix output = Matrix.Join(row1, row2, MatrixDimensions.Rows);


            return output;
        }

        /// <summary>
        /// Create a Matrix filled with random numbers between 0.0 and 1.0
        /// </summary>
        /// <param name="rows">The number of rows to initialise the Matrix with.</param>
        /// <param name="columns">The number of columns to initialise the Matrix with.</param>
        /// <param name="seed">A number used to calculate a starting value for the pseudo-random
        /// number sequence.</param>
        /// <returns>A new Matrix filled with random numbers between 0.0 and 1.0</returns>
        public static Matrix Rand(int rows, int columns, int seed)
        {
            Matrix output = new Matrix(rows, columns);
            Random x = new Random(seed);

            for (int i = 0; i < output.data.Length; i++)
                output.data[i] = x.NextDouble();

            return output;
        }

        /// <summary>
        /// Create a Matrix filled with random numbers between 0.0 and 1.0
        /// </summary>
        /// <param name="rows">The number of rows to initialise the Matrix with.</param>
        /// <param name="columns">The number of columns to initialise the Matrix with.</param>
        /// <returns>A new Matrix filled with random numbers between 0.0 and 1.0</returns>
        public static Matrix Rand(int rows, int columns)
        {
            return Rand(rows, columns, (int)DateTime.Now.Ticks & 0x0000FFFF);
        }
        #endregion

        #region Element Operations
        /// <summary>
        /// Run a given operation on every element of a Matrix.
        /// </summary>
        /// <param name="m">The Matrix to operate on.</param>
        /// <param name="number">The value to use in each operation.</param>
        /// <param name="operation">The delegate method to operate with.</param>
        /// <returns>A new Matrix with the original elements operated on appropriately.</returns>
        public static Matrix ElementOperation(Matrix m, double number, ProcessNumbers operation)
        {
            Matrix result = new Matrix(m.Rows, m.Columns);
            for (int i = 0; i < result.data.Length; i++)
                result.data[i] = operation(m.data[i], number);

            return result;
        }

        /// <summary>
        /// Run a given operation on every corresponding element in two Matrix
        /// objects with the same dimensions.
        /// </summary>
        /// <param name="m1">The first Matrix to operate on.</param>
        /// <param name="m2">The second Matrix to operate on.</param>
        /// <param name="operation">The delegate method to operate with.</param>
        /// <returns>A new Matrix with each element from both input Matrix objects
        /// operated on appropriately.</returns>
        public static Matrix ElementOperation(Matrix m1, Matrix m2, ProcessNumbers operation)
        {
            if (m1 == null || m2 == null)
                throw new ArgumentNullException("ElementOperation cannot accept null Matrix objects");
            if (m1.Columns != m2.Columns && m1.Rows != m2.Rows)
                throw new InvalidMatrixDimensionsException("ElementOperation requires both Matrix objects to have the same dimensions");

            Matrix result = new LinearAlgebra.Matrix(m1.Rows, m1.Columns);

            if (m1.HasSameDimensions(m2))
            {
                for (int i = 0; i < result.data.Length; i++)
                    result.data[i] = operation(m1.data[i], m2.data[i]);
            }
            else
            {
                // Matrix/Vector operations.
                if (m1.Columns == m2.Columns)
                {
                    int index = 0;
                    for (int i = 0; i < result.data.Length; i++)
                    {
                        result.data[i] = operation(m1.data[i], m2.data[index++]);
                        if (index == m1.Columns) index = 0;
                    }
                }
                else
                {
                    // same number of rows
                    int index = 0;
                    for (int i = 0; i < result.Rows; i++)
                    {
                        for (int j=0; j < result.Columns; j++)
                        {
                            result.data[index] = operation(m1.data[index], m2.data[i]);
                            index++;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Run a given operation on every element of a Matrix.
        /// </summary>
        /// <param name="m">The Matrix to operate on.</param>
        /// <param name="operation">The delegate method to operate with.</param>
        /// <returns>A new Matrix with the original elements operated on appropriately.</returns>
        public static Matrix ElementOperation(Matrix m, ProcessNumber operation)
        {
            Matrix result = new Matrix(m.Rows, m.Columns);
            for (int i = 0; i < result.data.Length; i++)
                result.data[i] = operation(m.data[i]);

            return result;
        }

        #region Specific implementations of ElementOperation (scalars)
        /// <summary>
        /// Add a fixed number to each element in a given Matrix.
        /// </summary>
        /// <param name="m">The Matrix to process.</param>
        /// <param name="number">The number to add to each Matrix element.</param>
        /// <returns>A new Matrix containing elements added to the given number.</returns>
        public static Matrix ElementAdd(Matrix m, double number)
        {
            return ElementOperation(m, number, (x, y) => x + y);
        }

        /// <summary>
        /// Subtract a fixed number from each element in a given Matrix.
        /// </summary>
        /// <param name="m">The Matrix to process.</param>
        /// <param name="number">The number to subract from each Matrix element.</param>
        /// <returns>A new Matrix containing elements subtracted by the given number.</returns>
        public static Matrix ElementSubtract(Matrix m, double number)
        {
            return ElementOperation(m, number, (x, y) => x - y);
        }

        /// <summary>
        /// Multiply each element in a given Matrix by a fixed number.
        /// </summary>
        /// <param name="m">The Matrix to process.</param>
        /// <param name="number">The number to multiply each Matrix element by.</param>
        /// <returns>A new Matrix containing elements multiplied by the given number.</returns>
        public static Matrix ElementMultiply(Matrix m, double number)
        {
            return ElementOperation(m, number, (x, y) => x * y);
        }

        /// <summary>
        /// Divide each element in a given Matrix by a fixed number.
        /// </summary>
        /// <param name="m">The Matrix to process.</param>
        /// <param name="number">The number to divide each Matrix element by.</param>
        /// <returns>A new Matrix containing elements divided by the given number.</returns>
        public static Matrix ElementDivide(Matrix m, double number)
        {
            return ElementOperation(m, number, (x, y) => x / y);
        }

        /// <summary>
        /// Raise each element in a given Matrix by an exponent.
        /// </summary>
        /// <param name="m">The Matrix to process.</param>
        /// <param name="exponent">The exponent to raise each Matrix element by.</param>
        /// <returns>A new Matrix containing elements raised to the power of the given exponent.</returns>
        public static Matrix ElementPower(Matrix m, double exponent)
        {
            return ElementOperation(m, exponent, (x, y) => Math.Pow(x, y));
        }

        /// <summary>
        /// Get the square root of each element in a given Matrix.
        /// </summary>
        /// <param name="m">The Matrix to process.</param>
        /// <returns>A new Matrix containing elements that are the square roots of the
        /// original Matrix elements.</returns>
        public static Matrix ElementSqrt(Matrix m)
        {
            return ElementOperation(m, (x) => Math.Sqrt(x));
        }

        /// <summary>
        /// Get the absolute value of all Matrix elements.
        /// </summary>
        /// <param name="m">The Matrix to process.</param>
        /// <returns>A new Matrix containing the absolute value of all elements.</returns>
        public static Matrix ElementAbs(Matrix m)
        {
            return ElementOperation(m, (x) => Math.Abs(x));
        }

        /// <summary>
        /// Calculate e to the power of m for each element in m.
        /// </summary>
        /// <param name="m">The Matrix to process.</param>
        /// <returns>A Matrix containing elements that are e ^ m for all elements
        /// in the original Matrix m.</returns>
        public static Matrix ElementExp(Matrix m)
        {
            return ElementOperation(m, (x) => Math.Pow(Math.E, x));
        }

        /// <summary>
        /// Calculate the natural logarithm for each element in m.
        /// </summary>
        /// <param name="m">The Matrix to process.</param>
        /// <returns>A Matrix containing elements a that are e ^ a = m for all elements
        /// in the original Matrix m.</returns>
        public static Matrix ElementLog(Matrix m)
        {
            return ElementOperation(m, (x) => Math.Log(x));
        }

        #endregion

        #region Specific implementations of ElementOperation (matrices)
        /// <summary>
        /// Add the corresponding elements in two Matrix objects with the same dimensions.
        /// </summary>
        /// <param name="m1">The first Matrix to process.</param>
        /// <param name="m2">The second Matrix to add values to the first.</param>
        /// <returns>A new Matrix containing elements added from both input Matrix objects.</returns>
        public static Matrix ElementAdd(Matrix m1, Matrix m2)
        {
            return ElementOperation(m1, m2, (x, y) => x + y);
        }

        /// <summary>
        /// Subtract the corresponding elements in two Matrix objects with the same dimensions.
        /// </summary>
        /// <param name="m1">The first Matrix to process.</param>
        /// <param name="m2">The second Matrix to subtract values from the first.</param>
        /// <returns>A new Matrix containing elements subtracted from both input Matrix objects.</returns>
        public static Matrix ElementSubtract(Matrix m1, Matrix m2)
        {
            return ElementOperation(m1, m2, (x, y) => x - y);
        }

        /// <summary>
        /// Multiply the corresponding elements in two Matrix objects with the same dimensions.
        /// </summary>
        /// <param name="m1">The first Matrix to process.</param>
        /// <param name="m2">The second Matrix to multiply values from the first.</param>
        /// <returns>A new Matrix containing elements multiplied from both input Matrix objects.</returns>
        public static Matrix ElementMultiply(Matrix m1, Matrix m2)
        {
            return ElementOperation(m1, m2, (x, y) => x * y);
        }

        /// <summary>
        /// Divide the corresponding elements in two Matrix objects with the same dimensions.
        /// </summary>
        /// <param name="m1">The first Matrix to process.</param>
        /// <param name="m2">The second Matrix to divide values from the first.</param>
        /// <returns>A new Matrix containing elements divided from both input Matrix objects.</returns>
        public static Matrix ElementDivide(Matrix m1, Matrix m2)
        {
            return ElementOperation(m1, m2, (x, y) => x / y);
        }
        #endregion
        #endregion

        #region Dimension Operations
        /// <summary>
        /// Run a given operation on all elements in a particular dimension to reduce that dimension
        /// to a single row or column.
        /// </summary>
        /// <param name="m">The Matrix to operate on.</param>
        /// <param name="dimension">Indicate whether to operate on rows or columns.</param>
        /// <param name="operation">The delegate method to operate with.</param>
        /// <returns>A Matrix populated with the results of performing the given operation.</returns>
        /// <remarks>If the current Matrix is a row or column vector, then a 1*1 Matrix
        /// will be returned, regardless of which dimension is chosen. If the dimension is
        /// set to 'Auto', then the first non-singleton dimension is chosen. If no singleton
        /// dimension exists, then columns are used as the default.</remarks>
        public static Matrix ReduceDimension(Matrix m, MatrixDimensions dimension, ProcessNumbers operation)
        {
            Matrix result = null;

            // Process calculations
            switch (dimension)
            {
                case MatrixDimensions.Auto:
                    // Inspired by Octave, 'Auto' will process the first non-singleton dimension.
                    if (m.Rows == 1 || m.Columns == 1)
                    {
                        result = new Matrix(1, 1);
                        for (int i = 0; i < m.data.Length; i++)
                            result.data[0] = operation(result.data[0], m.data[i]);
                        return result;
                    }
                    else
                    {
                        // No singleton case? Let's go with columns.
                        goto case MatrixDimensions.Columns; // goto?? Haven't used one in years, and it feels good!!!!
                    }
                case MatrixDimensions.Columns:
                    result = new Matrix(1, m.Columns);
                    for (int i = 0; i < m.data.Length; i += m.Columns)
                        for (int j = 0; j < m.Columns; j++)
                            result.data[j] = operation(result.data[j], m.data[i + j]);
                    break;
                case MatrixDimensions.Rows:
                    result = new Matrix(m.Rows, 1);
                    int index = 0;
                    for (int i = 0; i < m.Rows; i++)
                        for (int j = 0; j < m.Columns; j++)
                            result.data[i] = operation(result.data[i], m.data[index++]);
                    break;
                default:
                    break;
            }

            return result;
        }

        #region Specific implementations of ReduceDimension
        /// <summary>
        /// Sum all elements along a specified dimension.
        /// </summary>
        /// <param name="m">The Matrix whose elements need to be added together.</param>
        /// <param name="dimension">The dimension (row or column) to process.</param>
        /// <returns>A 1*n or n*1 Matrix containing the sum of each element along the
        /// processed dimension.</returns>
        public static Matrix Sum(Matrix m, MatrixDimensions dimension = MatrixDimensions.Auto)
        {
            return ReduceDimension(m, dimension, (x, y) => x + y);
        }
        #endregion

        /// <summary>
        /// Run a set of operations on all elements in a particular dimension to reduce that dimension
        /// to a single row, and then perform an aggregate operation to produce a statistical 
        /// </summary>
        /// <param name="m">The Matrix to operate on.</param>
        /// <param name="dimension">Indicate whether to operate on rows or columns.</param>
        /// <param name="operation">The delegate method to operate with.</param>
        /// <remarks>If the current Matrix is a row or column vector, then a 1*1 Matrix
        /// will be returned, regardless of which dimension is chosen. If the dimension is
        /// set to 'Auto', then the first non-singleton dimension is chosen. If no singleton
        /// dimension exists, then columns are used as the default.</remarks>
        public static Matrix StatisticalReduce(Matrix m, MatrixDimensions dimension, ProcessMatrix operation)
        {
            Matrix result = null;

            switch (dimension)
            {
                case MatrixDimensions.Auto:
                    if (m.Rows == 1)
                    {
                        result = new Matrix(1, 1);
                        result.data[0] = operation(m);
                        return result;
                    }
                    else if (m.Columns == 1)
                    {
                        result = new Matrix(1, 1);
                        result.data[0] = operation(m);
                        return result;
                    }
                    else
                    {
                        // No singleton case? Let's go with columns.
                        goto case MatrixDimensions.Columns;
                    }
                case MatrixDimensions.Columns:
                    result = new Matrix(1, m.Columns);
                    for (int i = 0; i < m.Columns; i++)
                        result.data[i] = operation(m.GetColumn(i));
                    break;
                case MatrixDimensions.Rows:
                    result = new Matrix(m.Rows, 1);
                    for (int i = 0; i < m.Rows; i++)
                        result.data[i] = operation(m.GetRow(i));
                    break;
                default:
                    break;
            }

            return result;
        }

        #region Specific implementations of StatisticalReduce
        /// <summary>
        /// Get the mean value of all elements in each dimension of a given Matrix.
        /// </summary>
        /// <param name="m">The Matrix whose elements need to be averaged.</param>
        /// <param name="dimension">The dimension (row or column) to process.</param>
        /// <returns>A 1*n or n*1 Matrix containing the mean of each element along the
        /// processed dimension.</returns>
        public static Matrix Mean(Matrix m, MatrixDimensions dimension = MatrixDimensions.Auto)
        {
            return StatisticalReduce(m, dimension, (x) => x.data.Average());
        }

        /// <summary>
        /// Get the mean value of all elements squared in each dimension of a given Matrix.
        /// </summary>
        /// <param name="m">The Matrix whose squared elements need to be averaged.</param>
        /// <param name="dimension">The dimension (row or column) to process.</param>
        /// <returns>A 1*n or n*1 Matrix containing the mean square of each element along the
        /// processed dimension.</returns>
        public static Matrix MeanSquare(Matrix m, MatrixDimensions dimension = MatrixDimensions.Auto)
        {
            return StatisticalReduce(m, dimension, GetMeanSquare);
        }
        private static double GetMeanSquare(Matrix m)
        {
            double result = 0.0;
            foreach (double element in m)
            {
                result += Math.Pow(element, 2);
            }
            result /= m.data.Count();
            return result;
        }

        /// <summary>
        /// Get the maximum value of all elements in each dimension of a given Matrix.
        /// </summary>
        /// <param name="m">The Matrix to find the maximum value from.</param>
        /// <param name="dimension">The dimension (row or column) to process.</param>
        /// <returns>A 1*n or n*1 Matrix containing the maximum of each element along the
        /// processed dimension.</returns>
        public static Matrix Max(Matrix m, MatrixDimensions dimension = MatrixDimensions.Auto)
        {
            return StatisticalReduce(m, dimension, (x) => x.data.Max());
        }

        /// <summary>
        /// Get the minimum value of all elements in each dimension of a given Matrix.
        /// </summary>
        /// <param name="m">The Matrix to find the minimum value from.</param>
        /// <param name="dimension">The dimension (row or column) to process.</param>
        /// <returns>A 1*n or n*1 Matrix containing the minimum of each element along the
        /// processed dimension.</returns>
        public static Matrix Min(Matrix m, MatrixDimensions dimension = MatrixDimensions.Auto)
        {
            return StatisticalReduce(m, dimension, (x) => x.data.Min());
        }

        /// <summary>
        /// Find the index of the maximum value along a given dimension.
        /// </summary>
        /// <param name="m">The Matrix to find the maximum value index from.</param>
        /// <param name="dimension">The dimension (row or column) to process.</param>
        /// <returns>A 1*n or n*1 Matrix containing the maximum index of each element along the
        /// processed dimension.</returns>
        public static Matrix MaxIndex(Matrix m, MatrixDimensions dimension = MatrixDimensions.Auto)
        {
            return StatisticalReduce(m, dimension, (x) => GetMaxIndex(x));
        }
        private static int GetMaxIndex(Matrix m)
        {
            int maxIndex = 0;
            double maxValue = m.data[0];
            for (int i=0; i<m.data.Length; i++)
            {
                if (m.data[i] > maxValue)
                {
                    maxValue = m.data[i];
                    maxIndex = i;
                }
            }
            return maxIndex;
        }

        /// <summary>
        /// Find the index of the minimum value along a given dimension.
        /// </summary>
        /// <param name="m">The Matrix to find the minimum value index from.</param>
        /// <param name="dimension">The dimension (row or column) to process.</param>
        /// <returns>A 1*n or n*1 Matrix containing the minimum index of each element along the
        /// processed dimension.</returns>
        public static Matrix MinIndex(Matrix m, MatrixDimensions dimension = MatrixDimensions.Auto)
        {
            return StatisticalReduce(m, dimension, (x) => GetMinIndex(x));
        }
        private static int GetMinIndex(Matrix m)
        {
            int minIndex = 0;
            double minValue = m.data[0];
            for (int i = 0; i < m.data.Length; i++)
            {
                if (m.data[i] < minValue)
                {
                    minValue = m.data[i];
                    minIndex = i;
                }
            }
            return minIndex;
        }

        /// <summary>
        /// Get the range of values of all elements in each dimension of a given Matrix.
        /// </summary>
        /// <param name="m">The Matrix to find the range of.</param>
        /// <param name="dimension">The dimension (row or column) to process.</param>
        /// <returns>A 1*n or n*1 Matrix containing the range of each element along the
        /// processed dimension.</returns>
        public static Matrix Range(Matrix m, MatrixDimensions dimension = MatrixDimensions.Auto)
        {
            return StatisticalReduce(m, dimension, (x) => x.data.Max() - x.data.Min());
        }

        /// <summary>
        /// Get the interquartile range of values of all elements in each dimension of a given Matrix.
        /// </summary>
        /// <param name="m">The Matrix to find the interquartile range of.</param>
        /// <param name="dimension">The dimension (row or column) to process.</param>
        /// <returns>A 1*n or n*1 Matrix containing the interquartile range of each element along the
        /// processed dimension.</returns>
        public static Matrix IQR(Matrix m, MatrixDimensions dimension = MatrixDimensions.Auto)
        {
            return StatisticalReduce(m, dimension, (x) => GetQuartile3(x) - GetQuartile1(x) );
        }

        /// <summary>
        /// Get the median value of all elements in each dimension of a given Matrix.
        /// </summary>
        /// <param name="m">The Matrix to find the median of.</param>
        /// <param name="dimension">The dimension (row or column) to process.</param>
        /// <returns>A 1*n or n*1 Matrix containing the median of each element along the
        /// processed dimension.</returns>
        public static Matrix Median(Matrix m, MatrixDimensions dimension = MatrixDimensions.Auto)
        {
            return StatisticalReduce(m, dimension, GetMedian);
        }
        private static double GetMedian(Matrix vector)
        {
            if (vector.data.Length == 1)
                return vector.data[0];
            if (vector.data.Length == 2)
                return (vector.data[0] + vector.data[1]) / 2;

            List<double> data = vector.data.ToList(); data.Sort();
            int index = data.Count / 2;
            if (data.Count % 2 != 0)
                return data[index];
            else
                return (data[index] + data[index + 1]) / 2;
        }

        /// <summary>
        /// Get the first quartile value of all elements in each dimension of a given Matrix.
        /// </summary>
        /// <param name="m">The Matrix to find the first quartile of.</param>
        /// <param name="dimension">The dimension (row or column) to process.</param>
        /// <returns>A 1*n or n*1 Matrix containing the first quartile of each element along the
        /// processed dimension.</returns>
        public static Matrix Quartile1(Matrix m, MatrixDimensions dimension = MatrixDimensions.Auto)
        {
            return StatisticalReduce(m, dimension, GetQuartile1);
        }
        private static double GetQuartile1(Matrix vector)
        {
            if (vector.data.Length == 1)
                return vector.data[0];

            List<double> data = vector.data.ToList(); data.Sort();
            // Handle even number of elements
            if (data.Count % 2 == 0)
            {
                int upperBound = data.Count / 2;
                int index = upperBound / 2;
                if (upperBound % 2 != 0)
                    return data[index];
                else
                    return (data[index - 1] + data[index]) / 2;
            }
            // Handle 4n+1 number of elements
            if ((data.Count - 1) % 4 == 0)
            {
                int n = (data.Count - 1) / 4;
                return ((data[n - 1] * 0.25) + (data[n] * 0.75));
            }
            // Handle 4n+3 number of elements
            if ((data.Count - 3) % 4 == 0)
            {
                int n = (data.Count - 3) / 4;
                return ((data[n] * 0.75) + (data[n + 1] * 0.25));
            }

            return 0.0;
        }

        /// <summary>
        /// Get the third quartile value of all elements in each dimension of a given Matrix.
        /// </summary>
        /// <param name="m">The Matrix to find the third quartile of.</param>
        /// <param name="dimension">The dimension (row or column) to process.</param>
        /// <returns>A 1*n or n*1 Matrix containing the third quartile of each element along the
        /// processed dimension.</returns>
        public static Matrix Quartile3(Matrix m, MatrixDimensions dimension = MatrixDimensions.Auto)
        {
            return StatisticalReduce(m, dimension, GetQuartile3);
        }
        private static double GetQuartile3(Matrix vector)
        {
            if (vector.data.Length == 1)
                return vector.data[0];

            List<double> data = vector.data.ToList(); data.Sort();
            // Handle even number of elements
            if (data.Count % 2 == 0)
            {
                int upperBound = data.Count / 2;
                int index = upperBound / 2;
                if (upperBound % 2 != 0)
                    return data[index + upperBound];
                else
                    return (data[index + upperBound - 1] + data[index + upperBound]) / 2;
            }
            // Handle 4n+1 number of elements
            if ((data.Count - 1) % 4 == 0)
            {
                int n = (data.Count - 1) / 4;
                int index = 3 * n;
                return ((data[index] * 0.75) + (data[index + 1] * 0.25));
            }
            // Handle 4n+3 number of elements
            if ((data.Count - 3) % 4 == 0)
            {
                int n = (data.Count - 3) / 4;
                int index = 3 * n;
                return ((data[index + 1] * 0.25) + (data[index + 2] * 0.75));
            }

            return 0.0;
        }

        /// <summary>
        /// Get the mode from all elements in each dimension of a given Matrix.
        /// </summary>
        /// <param name="m">The Matrix to find the mode of.</param>
        /// <param name="dimension">The dimension (row or column) to process.</param>
        /// <returns>A 1*n or n*1 Matrix containing the mode of each element along the
        /// processed dimension.</returns>
        public static Matrix Mode(Matrix m, MatrixDimensions dimension = MatrixDimensions.Auto)
        {
            return StatisticalReduce(m, dimension, GetMode);
        }
        private static double GetMode(Matrix vector)
        {
            if (vector.data.Length == 1)
                return vector.data[0];

            // Group all the elements by identical values, and find the maximum
            // count. Then return the minimum value that matches the maximum count.
            var groups = vector.data.GroupBy(element => element);
            int maxCount = groups.Max(group => group.Count());
            var modeValues = groups.Where(group => (group.Count() == maxCount));

            if (modeValues.Count() == 1)
                return modeValues.First().Key;

            // For consistency with Octave, if there are multiple modes, return
            // the one with the smallest value.
            double minValue = modeValues.First().Key;
            foreach (var modeValue in modeValues)
            {
                if (modeValue.Key < minValue)
                    minValue = modeValue.Key;
            }

            return minValue;
        }

        /// <summary>
        /// Calculate the variance of the elements in a given Matrix.
        /// </summary>
        /// <param name="m">The Matrix to find the variance of.</param>
        /// <param name="dimension">The dimension (row or column) to process.</param>
        /// <returns>A 1*n or n*1 Matrix containing the variance of each element along the
        /// processed dimension.</returns>
        public static Matrix Variance(Matrix m, MatrixDimensions dimension = MatrixDimensions.Auto)
        {
            return StatisticalReduce(m, dimension, GetVariance);
        }
        private static double GetVariance(Matrix vector)
        {
            double mean = vector.data.Average();
            double result = 0.0;
            int n = vector.data.Count();

            foreach(double element in vector)
            {
                result += Math.Pow(element - mean, 2);
            }

            return result/(n-1);
        }


        /// <summary>
        /// Calculate the standard deviation of values of all elements in each dimension of a given Matrix.
        /// </summary>
        /// <param name="m">The Matrix to find the standard deviation of.</param>
        /// <param name="dimension">The dimension (row or column) to process.</param>
        /// <returns>A 1*n or n*1 Matrix containing the standard deviation of each element along the
        /// processed dimension.</returns>
        public static Matrix StandardDeviation(Matrix m, MatrixDimensions dimension = MatrixDimensions.Auto)
        {
            return StatisticalReduce(m, dimension, (x) => Math.Sqrt(GetVariance(x)));
        }

        #endregion

        /// <summary>
        /// Extract a new Matrix from an existing one, filling in each column sequentially.
        /// </summary>
        /// <param name="m">The Matrix to extract data from.</param>
        /// <param name="startingIndex">The zero-based starting index of the Matrix to start
        /// extracting data from.</param>
        /// <param name="rows">The number of rows in the reshaped Matrix.</param>
        /// <param name="columns">The number of columns in the reshaped Matrix.</param>
        /// <returns>A new Matrix based on the given dimensions.</returns>
        public static Matrix Reshape(Matrix m, int startingIndex, int rows, int columns)
        {
            if (m.data.Length < (startingIndex + (rows * columns)))
                throw new InvalidMatrixDimensionsException("There are not enough elements to reshape the Matrix.");

            Matrix output = new Matrix(rows, columns);

            int dataIndex = startingIndex;
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    output[j, i] = m.data[dataIndex++];
                }
            }
            return output;
        }
        #endregion

        #endregion

        /// <summary>
        /// Implement the GetEnumerator method to run against the data array.
        /// </summary>
        /// <returns>Returns an enumerator for the data array.</returns>
        public IEnumerator GetEnumerator()
        {
            return data.GetEnumerator();
        }

    }

    /// <summary>
    /// Custom exception for Matrix operations using incorrect dimensions.
    /// </summary>
    public class InvalidMatrixDimensionsException : InvalidOperationException
    {
        public InvalidMatrixDimensionsException()
        {
        }

        public InvalidMatrixDimensionsException(string message)
            : base(message)
        {
        }

        public InvalidMatrixDimensionsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    /// <summary>
    /// Custom excepction for Matrix operations that require invertible matrices. 
    /// </summary>
    public class NonInvertibleMatrixException : InvalidOperationException
    {
        public NonInvertibleMatrixException()
        {
        }

        public NonInvertibleMatrixException(string message)
            : base(message)
        {
        }

        public NonInvertibleMatrixException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
