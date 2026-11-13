// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TensorCore.Init;

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using TensorCore.Randomness;

// Алгоритмы инициализации весов нейронной сети.
// Правильная инициализация предотвращает затухание/взрыв градиентов.
public static class Initializers
{
    // Фиксированный seed для воспроизводимости экспериментов
    private static readonly Random Rng = new(42);
    private static readonly object RngSync = new();
    // AsyncLocal привязывает seed к текущей логической операции: соседние тесты
    // и запросы не делят scoped state, а вложенный using может вернуть прежний
    // контекст без глобального reset process-wide генератора.
    private static readonly AsyncLocal<ScopedDeterministicRandomContext?> ScopedRng = new();

    public static IDisposable UseDeterministicSeed(ulong rootSeed)
    {
        var scope = new DeterministicSeedScope(ScopedRng.Value, rootSeed);
        ScopedRng.Value = scope.Context;
        return scope;
    }

    // Kaiming Uniform (He 2015) — для сетей с ReLU.
    // Диапазон: [-sqrt(6/fan_in), +sqrt(6/fan_in)]
    public static float[] KaimingUniform(int rows, int cols)
    {
        var data   = new float[rows * cols];
        float bound = MathF.Sqrt(6.0f / cols); // fan_in = cols

        var scopedRng = CreateScopedRandom(
            "kaiming_uniform",
            rows,
            cols,
            variant: 0);
        if (scopedRng is not null)
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = (float)(scopedRng.NextUnitDouble() * 2 * bound - bound);
        }
        else
        {
            lock (RngSync)
            {
                for (int i = 0; i < data.Length; i++)
                    data[i] = (float)(Rng.NextDouble() * 2 * bound - bound);
            }
        }

        return data;
    }

    public static float[] KaimingUniform(
        int rows,
        int cols,
        ulong deterministicSeed)
    {
        var data = new float[rows * cols];
        float bound = MathF.Sqrt(6.0f / cols);
        var rng = new SplitMix64Random(deterministicSeed);
        for (int i = 0; i < data.Length; i++)
            data[i] = (float)(rng.NextUnitDouble() * 2 * bound - bound);
        return data;
    }

    // Нормальная инициализация N(0, std²) через Box-Muller преобразование.
    // Используется в LoRA для матрицы A.
    public static float[] Normal(int rows, int cols, float std = 0.02f)
    {
        var data = new float[rows * cols];

        var scopedRng = CreateScopedRandom(
            "normal",
            rows,
            cols,
            BitConverter.SingleToInt32Bits(std));
        if (scopedRng is not null)
        {
            for (int i = 0; i < data.Length; i += 2)
            {
                // Box-Muller: два равномерных → два стандартно нормальных
                double u1    = 1.0 - scopedRng.NextUnitDouble(); // избегаем log(0)
                double u2    = 1.0 - scopedRng.NextUnitDouble();
                double r     = Math.Sqrt(-2.0 * Math.Log(u1));
                double theta = 2.0 * Math.PI * u2;

                data[i] = (float)(r * Math.Cos(theta) * std);
                if (i + 1 < data.Length)
                    data[i + 1] = (float)(r * Math.Sin(theta) * std);
            }
        }
        else
        {
            lock (RngSync)
            {
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
            }
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

    private static SplitMix64Random? CreateScopedRandom(
        string algorithm,
        int rows,
        int cols,
        int variant)
    {
        var context = ScopedRng.Value;
        return context is null
            ? null
            : new SplitMix64Random(context.NextSeed(algorithm, rows, cols, variant));
    }

    private sealed class ScopedDeterministicRandomContext(ulong rootSeed)
    {
        private long _streamOrdinal;

        public ulong NextSeed(
            string algorithm,
            int rows,
            int cols,
            int variant)
        {
            // Interlocked делает shared AsyncLocal-frame безопасным, если код
            // случайно начнёт несколько построений модели из одного scope.
            var ordinal = Interlocked.Increment(ref _streamOrdinal) - 1L;
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            AppendFrame(hash, "tensorcore.initializers.deterministic-scope.v1");
            AppendUInt64(hash, rootSeed);
            AppendInt64(hash, ordinal);
            AppendFrame(hash, algorithm);
            AppendInt32(hash, rows);
            AppendInt32(hash, cols);
            AppendInt32(hash, variant);
            var digest = hash.GetHashAndReset();
            return BinaryPrimitives.ReadUInt64BigEndian(digest);
        }
    }

    private sealed class DeterministicSeedScope : IDisposable
    {
        private readonly ScopedDeterministicRandomContext? _previous;
        private bool _disposed;

        public DeterministicSeedScope(
            ScopedDeterministicRandomContext? previous,
            ulong rootSeed)
        {
            _previous = previous;
            Context = new ScopedDeterministicRandomContext(rootSeed);
        }

        public ScopedDeterministicRandomContext Context { get; }

        public void Dispose()
        {
            if (_disposed)
                return;

            // Неправильный порядок Dispose ломает вложенную семантику; лучше
            // остановиться явно, чем молча оставить чужой deterministic scope.
            if (!ReferenceEquals(ScopedRng.Value, Context))
            {
                throw new InvalidOperationException(
                    "TensorCore deterministic initializer scope was disposed out of order.");
            }

            ScopedRng.Value = _previous;
            _disposed = true;
        }
    }

    private static void AppendFrame(IncrementalHash hash, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        AppendInt32(hash, bytes.Length);
        hash.AppendData(bytes);
    }

    private static void AppendInt32(IncrementalHash hash, int value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(buffer, value);
        hash.AppendData(buffer);
    }

    private static void AppendInt64(IncrementalHash hash, long value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64BigEndian(buffer, value);
        hash.AppendData(buffer);
    }

    private static void AppendUInt64(IncrementalHash hash, ulong value)
    {
        Span<byte> buffer = stackalloc byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64BigEndian(buffer, value);
        hash.AppendData(buffer);
    }
}
