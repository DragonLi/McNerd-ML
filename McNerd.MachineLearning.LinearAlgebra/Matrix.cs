﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McNerd.MachineLearning.LinearAlgebra
{
    /// <summary>
    /// Various special cases of matrix.
    /// </summary>
    public enum MatrixTypes { Zeros, Ones, Identity, Magic, Random }

    /// <summary>
    /// Describe which dimension of a matrix to work with.
    /// </summary>
    public enum MatrixDimensions { Auto, Rows, Columns }

    public class Matrix
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
        #endregion

        #region Private Fields
        /// <summary>
        /// Storage array for the matrix data.
        /// </summary>
        double[] data;

        /// <summary>
        /// Dimensions of the matrix
        /// </summary>
        int rows, columns;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor to create a new matrix while specifying the number of
        /// rows and columns.
        /// </summary>
        /// <param name="rows">The number of rows to initialise the matrix with.</param>
        /// <param name="cols">The number of columns to initialise the matrix with.</param>
        public Matrix(int rows, int columns)
        {
            this.rows = rows;
            this.columns = columns;
            data = new double[rows * columns];
        }

        /// <summary>
        /// Constructor to create a new square matrix.
        /// </summary>
        /// <param name="dimensions">The number of rows and columns to initialise the
        /// matrix with. There will be an equal number of rows and columns.</param>
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
        #endregion

        #region Indexers
        /// <summary>
        /// Indexer to easily access a specific location in this matrix.
        /// </summary>
        /// <param name="row">The row of the matrix location to access.</param>
        /// <param name="column">The column of the matrix location to access.</param>
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
        /// Indicates whether or not this matrix row and column dimensions are equal.
        /// </summary>
        public bool IsSquare => rows == columns;

        /// <summary>
        /// Get the dimensions of this matrix in a single-dimensional array of the form
        /// [rows,columns].
        /// </summary>
        public int[] Dimensions => new int[] { rows, columns };

        /// <summary>
        /// Get the number of rows in this matrix.
        /// </summary>
        public int Rows => rows;

        /// <summary>
        /// Get the number of columns in this matrix.
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
                Matrix inv = new Matrix(Rows, Columns);

                // TODO: Implement one of the many algorithms for this. Maybe start with a relatively simple one
                // like Gaussian Elimination? Improve on it later.

                return inv;
            }
        }
        #endregion

        #region Operations
        /// <summary>
        /// Add two matrices together.
        /// </summary>
        /// <param name="m1">The first matrix to add.</param>
        /// <param name="m2">The second matrix to add.</param>
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
        /// Subtract one matrix from another.
        /// </summary>
        /// <param name="m1">The first matrix to subtract from.</param>
        /// <param name="m2">The second matrix to subtract from the first.</param>
        /// <returns>The result of subtracting the second matrix from the first.</returns>
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
        /// Multiply two matrices together.
        /// </summary>
        /// <param name="m1">An nxm dimension matrix.</param>
        /// <param name="m2">An mxp dimension matrix.</param>
        /// <returns>An nxp Matrix that is the product of m1 and m2.</returns>
        /// <exception cref="InvalidMatrixDimensionsException">Thrown when the number of columns in the
        /// first matrix don't match the number of rows in the second matrix.</exception>
        public static Matrix operator *(Matrix m1, Matrix m2)
        {
            if (m1.columns == m2.rows)
            {
                Matrix output = new Matrix(m1.rows, m2.columns);
                Parallel.For(0, m1.rows, i => MultiplyRow(i, m1, m2, output));
                return output;
            }
            else
            {
                throw new InvalidMatrixDimensionsException("Multiplication cannot be performed on matrices with these dimensions.");
            }
        }

        /// <summary>
        /// Scalar multiplication of a matrix.
        /// </summary>
        /// <param name="scalar">The scalar value to multiply each element of the matrix by.</param>
        /// <param name="m">The matrix to apply multiplication to.</param>
        /// <returns>A matrix representing the scalar multiplication of scalar * m.</returns>
        public static Matrix operator *(double scalar, Matrix m)
        {
            Matrix output = new Matrix(m.rows, m.columns);
            //for (int i = 0; i < m.data.Length; i++)
            //    output.data[i] = m.data[i] * scalar;
            //Parallel.For(0, m.data.Length, i => { output.data[i] = scalar * m.data[i]; });
            Parallel.For(0, m.rows, i => MultiplyRow(i, m, scalar, output));
            return output;
        }

        /// <summary>
        /// Scalar multiplication of a matrix.
        /// </summary>
        /// <param name="m">The matrix to apply multiplication to.</param>
        /// <param name="scalar">The scalar value to multiply each element of the matrix by.</param>
        /// <returns>A matrix representing the scalar multiplication of scalar * m.</returns>
        public static Matrix operator *(Matrix m, double scalar)
        {
            // Same as above, but ensuring commutativity - i.e. (s * m) == (m * s).
            return scalar * m;
        }

        /// <summary>
        /// Override the == operator to compare matrix values.
        /// </summary>
        /// <param name="m1">The first matrix to compare.</param>
        /// <param name="m2">The second matrix to compare.</param>
        /// <returns>True if the values of both matrices match.</returns>
        public static bool operator ==(Matrix m1, Matrix m2)
        {
            return m1.Equals(m2);
        }

        /// <summary>
        /// Override the != operator to compare matrix values.
        /// </summary>
        /// <param name="m1">The first matrix to compare.</param>
        /// <param name="m2">The second matrix to compare.</param>
        /// <returns>True if the values of both matrices differ.</returns>
        public static bool operator !=(Matrix m1, Matrix m2)
        {
            return !(m1 == m2);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Indicates if this matrix has the same dimensions as another supplied matrix.
        /// </summary>
        /// <param name="other">Another matrix to compare this instance to.</param>
        /// <returns>true if both matrices have the same dimensions. Otherwise, false.</returns>
        public bool HasSameDimensions(Matrix other)
        {
            return (this.rows == other.rows) && (this.columns == other.columns);
        }

        /// <summary>
        /// Override the Object.Equals method to compare matrix values.
        /// </summary>
        /// <param name="obj">The object to compare to this matrix.</param>
        /// <returns>True if obj is a matrix, and its values match the current
        /// matrix values.</returns>
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
        /// Compare this matrix with a second matrix by value.
        /// </summary>
        /// <param name="m">The matrix to compare to this one.</param>
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
        /// <returns>A bitwise XOR based on rows and columns of this matrix.</returns>
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
                    sb.AppendFormat("{0:0.00}", data[index++]);
                }
                sb.Append("\n");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Calculate a single row result of multiplying two matrices.
        /// </summary>
        /// <param name="row">The zero-indexed row to calculate.</param>
        /// <param name="m1">The first matrix to multiply.</param>
        /// <param name="m2">The second matrix to multiply.</param>
        /// <param name="output">The matrix to store the results in.</param>
        private static void MultiplyRow(int row, Matrix m1, Matrix m2, Matrix output)
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
        /// Calculate the results of multiplying each element in a matrix
        /// row by a scalar value.
        /// </summary>
        /// <param name="row">The zero-indexed row to calculate.</param>
        /// <param name="m">The matrix to multiply by a scalar value.</param>
        /// <param name="scalar">The scalar value to multiply the matrix by.</param>
        /// <param name="output">The matrix that contains the results of multiplying the input
        /// matrix by a scalar value.</param>
        private static void MultiplyRow(int row, Matrix m, double scalar, Matrix output)
        {
            int m_index = row * m.columns;

            for (int i = m_index; i < m_index + output.Columns; i++)
            {
                output.data[i] = scalar * m.data[i];
            }
        }

        #region Matrix creation methods
        /// <summary>
        /// Create an identity matrix
        /// </summary>
        /// <param name="dimensions">The number of rows and columns for this matrix.</param>
        /// <returns>A square matrix with zeros everywhere, except for the main diagonal which is filled with ones.</returns>
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
        #endregion

        #region Element Operations
        /// <summary>
        /// Run a given operation on every element of a matrix.
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
            if (!m1.HasSameDimensions(m2))
                throw new InvalidMatrixDimensionsException("ElementOperation requires both Matrix objects to have the same dimensions");

            Matrix result = new LinearAlgebra.Matrix(m1.Rows, m1.Columns);

            for (int i = 0; i < result.data.Length; i++)
                result.data[i] = operation(m1.data[i], m2.data[i]);

            return result;
        }

        /// <summary>
        /// Run a given operation on every element of a matrix.
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
        /// Multiply each element in a given Matrix by a fixed number.
        /// </summary>
        /// <param name="m">The Matrix to process.</param>
        /// <param name="number">The number to multiply each Matrix element by.</param>
        /// <returns>A new Matrix containing elements multiplied by the given number.</returns>
        public static Matrix ElementSqrt(Matrix m)
        {
            return ElementOperation(m, (x) => Math.Sqrt(x));
        }

        /// <summary>
        /// Multiply each element in a given Matrix by a fixed number.
        /// </summary>
        /// <param name="m">The Matrix to process.</param>
        /// <param name="number">The number to multiply each Matrix element by.</param>
        /// <returns>A new Matrix containing elements multiplied by the given number.</returns>
        public static Matrix ElementAbs(Matrix m)
        {
            return ElementOperation(m, (x) => Math.Abs(x));
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
        /// <param name="m">The matrix to operate on.</param>
        /// <param name="dimension">Indicate whether to operate on rows or columns.</param>
        /// <param name="operation">The delegate method to operate with.</param>
        /// <returns>A matrix populated with the results of performing the given operation.</returns>
        /// <remarks>If the current matrix is a row or column vector, then a 1*1 matrix
        /// will be returned, regardless of which dimension is chosen. If the dimension is
        /// set to 'Auto', then the first non-singleton dimension is chosen. If no singleton
        /// dimension exists, then columns are used as the default.</remarks>
        public static Matrix ReduceDimension(Matrix m, MatrixDimensions dimension, ProcessNumbers operation)
        {
            Matrix result = null;

            // Process calculations
            switch(dimension)
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
        #endregion

        #endregion
    }

    /// <summary>
    /// Custom exception for matrix operations using incorrect dimensions.
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
    /// Custom excepction for matrix operations that require invertible matrices. 
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
