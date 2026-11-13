namespace MatrixAlgorithms;

// DenseMatrix is a thin wrapper around a flat double array (a one-dimensional array
// that stores all matrix entries one row after another).
// "Dense" (плотная матрица) means every entry is stored, including zeros.
// This contrasts with sparse matrices (Family 2) where only non-zero entries are kept.
// DenseMatrix does NOT contain any algorithm — it is pure storage with index arithmetic.
public sealed class DenseMatrix
{
    // The flat backing array. Row r, column c is stored at index r * Columns + c.
    // "Row-major order" means row 0 occupies positions 0..Columns-1,
    // row 1 occupies positions Columns..2*Columns-1, and so on.
    private readonly double[] _data;

    // The number of rows in this matrix.
    public int Rows    { get; }

    // The number of columns in this matrix.
    public int Columns { get; }

    // Allocate a new matrix filled with zeros (the .NET default for double[]).
    public DenseMatrix(int rows, int columns)
    {
        // Validate that the dimensions are positive to catch programming bugs early.
        if (rows <= 0 || columns <= 0)
            throw new ArgumentOutOfRangeException(
                $"Matrix dimensions must be positive. Got {rows} rows × {columns} columns.");

        // Allocate enough space for every entry; .NET initialises all entries to 0.0.
        _data   = new double[rows * columns];
        Rows    = rows;
        Columns = columns;
    }

    // Create a matrix that shares an existing flat array without copying it.
    // The caller is responsible for ensuring the array length equals rows × columns.
    public DenseMatrix(double[] existingData, int rows, int columns)
    {
        // Guard against an array that is too short for the claimed dimensions.
        if (existingData.Length < rows * columns)
            throw new ArgumentException(
                $"Array length {existingData.Length} is smaller than {rows} × {columns} = {rows * columns}.");

        _data   = existingData;
        Rows    = rows;
        Columns = columns;
    }

    // Get or set the entry at the given row (строка) and column (столбец).
    // Translates the two-dimensional coordinates to the flat array index.
    public double this[int row, int column]
    {
        get => _data[row * Columns + column];
        set => _data[row * Columns + column] = value;
    }

    // Return the backing flat array for algorithms that need direct access to the storage.
    // The caller receives the actual array, not a copy — changes affect the matrix.
    public double[] AsArray() => _data;

    // Copy a complete row into a new one-dimensional array.
    // Used by decomposition algorithms that need to read a row without modifying the matrix.
    public double[] GetRow(int rowIndex)
    {
        // Allocate a fresh array so the caller can modify it safely.
        var row = new double[Columns];
        // Copy exactly Columns values starting at the position where the row begins.
        Array.Copy(_data, rowIndex * Columns, row, 0, Columns);
        return row;
    }

    // Copy a complete column into a new one-dimensional array.
    // Used by decomposition algorithms that process one column at a time.
    public double[] GetColumn(int columnIndex)
    {
        var col = new double[Rows];
        // Step through the backing array in strides of Columns to collect column entries.
        for (int r = 0; r < Rows; r++)
            col[r] = _data[r * Columns + columnIndex];
        return col;
    }

    // Swap two complete rows in-place.
    // Used by LU decomposition with partial pivoting (Гауссово исключение с перестановкой строк —
    // eliminating below the diagonal while putting the largest available entry on the diagonal first).
    public void SwapRows(int rowA, int rowB)
    {
        // If both row indices are the same there is nothing to do.
        if (rowA == rowB) return;

        for (int c = 0; c < Columns; c++)
        {
            // Hold the value from row A while we overwrite it.
            double temp = this[rowA, c];
            this[rowA, c] = this[rowB, c];
            this[rowB, c] = temp;
        }
    }

    // Return a full independent copy of this matrix.
    // Algorithms that must not modify the original data call Clone() first.
    public DenseMatrix Clone()
    {
        var copy = (double[])_data.Clone();
        return new DenseMatrix(copy, Rows, Columns);
    }

    // Multiply a column vector (столбец — одномерный массив длиной Columns) by this matrix.
    // Returns a new vector of length Rows: result[i] = sum_j this[i,j] * vector[j].
    public double[] MultiplyVector(double[] vector)
    {
        if (vector.Length != Columns)
            throw new ArgumentException(
                $"Vector length {vector.Length} does not match matrix columns {Columns}.");

        var result = new double[Rows];
        for (int r = 0; r < Rows; r++)
        {
            double sum = 0.0;
            for (int c = 0; c < Columns; c++)
                sum += this[r, c] * vector[c];
            result[r] = sum;
        }
        return result;
    }

    // Multiply the transpose of this matrix by a column vector.
    // Returns a vector of length Columns: result[j] = sum_i this[i,j] * vector[i].
    // Used in singular value decomposition power iteration to compute A^T * u.
    public double[] MultiplyTransposeVector(double[] vector)
    {
        if (vector.Length != Rows)
            throw new ArgumentException(
                $"Vector length {vector.Length} does not match matrix rows {Rows}.");

        var result = new double[Columns];
        for (int c = 0; c < Columns; c++)
        {
            double sum = 0.0;
            for (int r = 0; r < Rows; r++)
                sum += this[r, c] * vector[r];
            result[c] = sum;
        }
        return result;
    }
}
