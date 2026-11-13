namespace TensorCore.Init;

// Алгоритмы инициализации весов нейронной сети.
// Правильная инициализация предотвращает затухание/взрыв градиентов.
public static class Initializers
{
    // Фиксированный seed для воспроизводимости экспериментов
    private static readonly Random Rng = new(42);

    // Kaiming Uniform (He 2015) — для сетей с ReLU.
    // Диапазон: [-sqrt(6/fan_in), +sqrt(6/fan_in)]
    public static float[] KaimingUniform(int rows, int cols)
    {
        var data   = new float[rows * cols];
        float bound = MathF.Sqrt(6.0f / cols); // fan_in = cols

        for (int i = 0; i < data.Length; i++)
            data[i] = (float)(Rng.NextDouble() * 2 * bound - bound);

        return data;
    }

    // Нормальная инициализация N(0, std²) через Box-Muller преобразование.
    // Используется в LoRA для матрицы A.
    public static float[] Normal(int rows, int cols, float std = 0.02f)
    {
        var data = new float[rows * cols];

        for (int i = 0; i < data.Length; i += 2)
        {
            // Box-Muller: два равномерных → два стандартно нормальных
            double u1    = 1.0 - Rng.NextDouble(); // избегаем log(0)
            double u2    = 1.0 - Rng.NextDouble();
            double r     = Math.Sqrt(-2.0 * Math.Log(u1));
            double theta = 2.0 * Math.PI * u2;

            data[i] = (float)(r * Math.Cos(theta) * std);
            if (i + 1 < data.Length)
                data[i + 1] = (float)(r * Math.Sin(theta) * std);
        }

        return data;
    }

    // Нулевая инициализация — используется в LoRA для матрицы B
    // (адаптер начинает как нулевое добавление к базовой модели)
    public static float[] Zeros(int rows, int cols) => new float[rows * cols];

    // Инициализация единицами — используется для нормализационных весов (γ=1, β=0)
    public static float[] Ones(int count)
    {
        var data = new float[count];
        Array.Fill(data, 1.0f);
        return data;
    }
}
