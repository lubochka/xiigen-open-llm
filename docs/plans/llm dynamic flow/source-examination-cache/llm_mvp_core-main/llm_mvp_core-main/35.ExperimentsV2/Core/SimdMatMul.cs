using System.Numerics;

namespace ExperimentsV2;

// SimdMatMul реализует матрично-векторное умножение с использованием SIMD.
// Обрабатывает Vector<float>.Count (8 на AVX, 16 на AVX-512) элементов за раз.
// Для матриц размером >= 64×64 даёт 4-8× ускорение на современных процессорах.
public static class SimdMatMul
{
    // Умножить матрицу (rows × cols) на вектор (cols) → результат (rows).
    // matrix хранится row-major: matrix[row * cols + col].
    public static float[] MatVec(float[] matrix, float[] vector, int rows, int cols)
    {
        var result = new float[rows];
        int vSize  = Vector<float>.Count;

        for (int row = 0; row < rows; row++)
        {
            int   rowOffset = row * cols;
            float sum       = 0f;
            int   col       = 0;

            for (; col <= cols - vSize; col += vSize)
            {
                var mVec = new Vector<float>(matrix, rowOffset + col);
                var vVec = new Vector<float>(vector, col);
                sum += Vector.Dot(mVec, vVec);
            }

            for (; col < cols; col++)
                sum += matrix[rowOffset + col] * vector[col];

            result[row] = sum;
        }
        return result;
    }

    // Обычное умножение без SIMD — для сравнения
    public static float[] MatVecPlain(float[] matrix, float[] vector, int rows, int cols)
    {
        var result = new float[rows];
        for (int row = 0; row < rows; row++)
        {
            float sum = 0f;
            for (int col = 0; col < cols; col++)
                sum += matrix[row * cols + col] * vector[col];
            result[row] = sum;
        }
        return result;
    }
}
