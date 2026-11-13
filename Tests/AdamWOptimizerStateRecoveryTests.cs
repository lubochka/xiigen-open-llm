// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using TensorCore.Core;
using System.Buffers.Binary;
using TensorCore.Layers;
using TrainingLoop.Optimizers;
using Xunit;

// Проверка одной семьи изменяемого состояния обучения: накопленные оценки
// градиентов оптимизатора и счётчик шага. До этого слайса они существовали
// только в памяти процесса, поэтому после перезапуска урок продолжался бы с
// обнулённой памятью оптимизатора.
//
// Граница честности: здесь проверяется сохранение и повторное открытие
// состояния через байты на диске, а не качество обучения модели и не перезапуск
// между процессами операционной системы.
public sealed class AdamWOptimizerStateRecoveryTests
{
    // Один шаг оптимизатора по заданным параметрам, чтобы у него появились
    // накопленные оценки градиентов и ненулевой счётчик шага.
    private static void RunOneStep(AdamW optimizer, IReadOnlyList<Parameter> parameters)
    {
        foreach (var parameter in parameters)
        {
            var gradient = new Tensor(parameter.Value.Shape);
            for (var index = 0; index < gradient.Data.Length; index++)
                gradient.Data[index] = 0.25f + (index * 0.125f);
            parameter.Value.Grad = gradient;
        }

        optimizer.Step(parameters, 0.001f);
    }

    private static Parameter CreateParameter(int length, float seedValue)
    {
        var tensor = new Tensor(new TensorShape(length));
        for (var index = 0; index < length; index++)
            tensor.Data[index] = seedValue + index;
        return new Parameter(tensor);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PortableState_SameCountDifferentDimensions_RejectsAtomically(bool matrix)
    {
        var source = new Parameter(new Tensor(new TensorShape(2, 3)));
        var target = new Parameter(new Tensor(matrix ? new TensorShape(3, 2) : new TensorShape(6)));
        Array.Fill(target.Value.Data, 4f);
        var sourceOptimizer = new AdamW();
        var targetOptimizer = new AdamW();
        RunOneStep(sourceOptimizer, new[] { source });
        RunOneStep(targetOptimizer, new[] { target });
        RunOneStep(targetOptimizer, new[] { target });
        var sourceBindings = new[] { new AdamWParameterBinding("matrix.weight", source) };
        var targetBindings = new[] { new AdamWParameterBinding("matrix.weight", target) };
        var reopened = AdamWPortableState.FromBytes(sourceOptimizer.ExportPortableState(sourceBindings).ToBytes());
        Assert.NotNull(reopened);
        var before = targetOptimizer.ExportPortableState(targetBindings).ToBytes();
        var weights = target.Value.Data.ToArray();
        var gradient = target.Grad!;
        var gradientData = gradient.Data.ToArray();
        var requiresGrad = target.Value.RequiresGrad;

        Assert.False(targetOptimizer.TryImportPortableState(targetBindings, reopened));

        Assert.Equal(before, targetOptimizer.ExportPortableState(targetBindings).ToBytes());
        Assert.Equal(weights, target.Value.Data);
        Assert.Same(gradient, target.Grad);
        Assert.Equal(gradientData, target.Grad!.Data);
        Assert.Equal(requiresGrad, target.Value.RequiresGrad);
    }

    [Fact]
    public void PortableState_WrittenToDiskAndReopened_RestoresMomentsAndStepExactly()
    {
        var firstParameter = CreateParameter(4, 1f);
        var secondParameter = CreateParameter(3, 5f);
        var bindings = new[]
        {
            new AdamWParameterBinding("layer.0.weight", firstParameter),
            new AdamWParameterBinding("layer.1.bias", secondParameter),
        };

        var optimizer = new AdamW();
        RunOneStep(optimizer, new[] { firstParameter, secondParameter });

        var exported = optimizer.ExportPortableState(bindings);
        Assert.Equal(2, exported.Entries.Count);
        Assert.True(exported.Step > 0);

        var path = Path.Combine(
            Path.GetTempPath(),
            "xiigen-adamw-state-" + Guid.NewGuid().ToString("N") + ".bin");
        try
        {
            File.WriteAllBytes(path, exported.ToBytes());

            // Новый оптимизатор моделирует процесс после перезапуска: у него нет
            // никакой памяти о предыдущем уроке.
            var restarted = new AdamW();
            var reopened = AdamWPortableState.FromBytes(File.ReadAllBytes(path));
            Assert.NotNull(reopened);
            Assert.True(restarted.TryImportPortableState(bindings, reopened!));

            var reexported = restarted.ExportPortableState(bindings);
            Assert.Equal(exported.Step, reexported.Step);
            Assert.Equal(exported.ToBytes(), reexported.ToBytes());
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void PortableState_LaterShapeMismatch_PreservesEarlierStateAndNextUpdate()
    {
        var source = new[] { Matrix(1f, 2, 3), Matrix(2f, 2, 3) };
        var target = new[] { Matrix(7f, 2, 3), Matrix(8f, 3, 2) };
        var control = new[] { Matrix(7f, 2, 3), Matrix(8f, 3, 2) };
        var sourceOptimizer = new AdamW();
        var targetOptimizer = new AdamW();
        var controlOptimizer = new AdamW();
        RunOneStep(sourceOptimizer, source);
        for (var i = 0; i < 2; i++)
        {
            RunOneStep(targetOptimizer, target);
            RunOneStep(controlOptimizer, control);
        }
        var incoming = sourceOptimizer.ExportPortableState(Bind(source));
        var before = targetOptimizer.ExportPortableState(Bind(target)).ToBytes();
        var weights = target.Select(p => p.Value.Data.ToArray()).ToArray();
        var gradients = target.Select(p => p.Grad!).ToArray();
        var gradientData = gradients.Select(g => g.Data.ToArray()).ToArray();

        Assert.False(targetOptimizer.TryImportPortableState(Bind(target), incoming));

        Assert.Equal(before, targetOptimizer.ExportPortableState(Bind(target)).ToBytes());
        for (var i = 0; i < target.Length; i++)
        {
            Assert.Equal(weights[i], target[i].Value.Data);
            Assert.Same(gradients[i], target[i].Grad);
            Assert.Equal(gradientData[i], target[i].Grad!.Data);
        }
        RunOneStep(targetOptimizer, target);
        RunOneStep(controlOptimizer, control);
        for (var i = 0; i < target.Length; i++)
            Assert.Equal(control[i].Value.Data, target[i].Value.Data);
        Assert.Equal(controlOptimizer.ExportPortableState(Bind(control)).ToBytes(),
            targetOptimizer.ExportPortableState(Bind(target)).ToBytes());
    }

    [Fact]
    public void PortableState_DimensionsDiskRoundTrip_ContinuesIdentically()
    {
        var source = new[] { Matrix(1f, 2, 3), Matrix(5f, 1, 2, 3) };
        var optimizer = new AdamW();
        RunOneStep(optimizer, source);
        var path = Path.Combine(Path.GetTempPath(), "xiigen-adamw-shape-" + Guid.NewGuid().ToString("N") + ".bin");
        Assert.False(File.Exists(path));
        try
        {
            File.WriteAllBytes(path, optimizer.ExportPortableState(Bind(source)).ToBytes());
            var reopened = AdamWPortableState.FromBytes(File.ReadAllBytes(path));
            Assert.NotNull(reopened);
            Assert.Equal(2, reopened.FormatVersion);
            var resumed = source.Select(p => new Parameter(new Tensor(p.Value.Data.ToArray(),
                new TensorShape(p.Value.Shape.Dimensions)))).ToArray();
            var restarted = new AdamW();
            Assert.True(restarted.TryImportPortableState(Bind(resumed).Reverse().ToArray(), reopened));
            for (var i = 0; i < source.Length; i++)
                Assert.Equal(source[i].Value.Shape.Dimensions, reopened.Entries[i].Dimensions);
            RunOneStep(optimizer, source);
            RunOneStep(restarted, resumed);
            for (var i = 0; i < source.Length; i++)
                Assert.Equal(source[i].Value.Data, resumed[i].Value.Data);
            Assert.Equal(optimizer.ExportPortableState(Bind(source)).ToBytes(),
                restarted.ExportPortableState(Bind(resumed)).ToBytes());
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
            Assert.False(File.Exists(path));
        }
    }

    [Fact]
    public void PortableState_SnapshotCollectionsDoNotAlias()
    {
        var dimensions = new[] { 2, 3 };
        var first = Enumerable.Repeat(0.1f, 6).ToArray();
        var second = Enumerable.Repeat(0.2f, 6).ToArray();
        var entry = new AdamWPortableParameterState("p0", dimensions, first, second);
        var entries = new[] { entry };
        var state = new AdamWPortableState(2, 1, entries);
        var before = state.ToBytes();
        var parameter = Matrix(4f, 2, 3);
        var optimizer = new AdamW();
        Assert.True(optimizer.TryImportPortableState(Bind([parameter]), state));
        dimensions[0] = 3;
        first[0] = 99f;
        second[0] = 88f;
        entries[0] = new AdamWPortableParameterState("other", [1], [0f], [0f]);
        Assert.Equal(before, state.ToBytes());
        Assert.Equal(before, optimizer.ExportPortableState(Bind([parameter])).ToBytes());
        Assert.Throws<NotSupportedException>(() => ((IList<int>)entry.Dimensions)[0] = 9);
        Assert.Throws<NotSupportedException>(() => ((IList<float>)entry.FirstMoment)[0] = 9f);
        Assert.Throws<NotSupportedException>(() => ((IList<float>)entry.SecondMoment)[0] = 9f);
        Assert.Throws<NotSupportedException>(() => ((IList<AdamWPortableParameterState>)state.Entries)[0] = entries[0]);
        var exported = optimizer.ExportPortableState(Bind([parameter]));
        parameter.Value.Shape.Dimensions[0] = 6;
        Assert.Equal(new[] { 2, 3 }, exported.Entries[0].Dimensions);
        var reopened = AdamWPortableState.FromBytes(before)!;
        Array.Clear(before);
        Assert.Equal(state.ToBytes(), reopened.ToBytes());
    }

    [Theory]
    [InlineData(new int[] { })]
    [InlineData(new[] { 0 })]
    [InlineData(new[] { -1 })]
    [InlineData(new[] { 65536, 65536 })]
    [InlineData(new[] { 2, 2 })]
    public void PortableState_InvalidDimensions_ConstructorRejects(int[] dimensions)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new AdamWPortableParameterState("p", dimensions, new float[6], new float[6]));
        Assert.Contains("ADAMW_PORTABLE_SHAPE_INVALID", exception.Message);
    }

    [Theory]
    [InlineData(new[] { 1 })]
    [InlineData(new[] { 1, 1 })]
    [InlineData(new[] { 1, 2, 3 })]
    public void PortableState_ValidDimensions_RoundTrip(int[] dimensions)
    {
        var count = dimensions.Aggregate(1, (a, b) => a * b);
        var state = new AdamWPortableState(2, 1,
            [new AdamWPortableParameterState("p", dimensions, new float[count], new float[count])]);
        Assert.Equal(dimensions, AdamWPortableState.FromBytes(state.ToBytes())!.Entries[0].Dimensions);
    }

    [Fact]
    public void PortableState_InvalidDimensionsOrFrame_FailsClosed()
    {
        var valid = new AdamWPortableState(2, 1,
            [new AdamWPortableParameterState("p", [2, 3], new float[6], new float[6])]).ToBytes();
        // Header20 + keyLength4 + key1 -> rank25, dims29/33, momentLength37.
        foreach (var (offset, value) in new (int, int)[]
        {
            (8, 1), (8, 99), (12, -1), (16, -1), (16, 1_000_001), (16, 1_000_000),
            (20, 0), (20, 1025), (25, 0), (25, -1), (25, int.MaxValue),
            (29, 0), (29, -1), (29, 4), (37, 0), (37, int.MaxValue), (37, 1_000_000_000)
        })
        {
            var corrupt = valid.ToArray();
            BinaryPrimitives.WriteInt32LittleEndian(corrupt.AsSpan(offset, 4), value);
            Assert.Null(AdamWPortableState.FromBytes(corrupt));
        }
        var overflow = valid.ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(overflow.AsSpan(29, 4), 65536);
        BinaryPrimitives.WriteInt32LittleEndian(overflow.AsSpan(33, 4), 65536);
        Assert.Null(AdamWPortableState.FromBytes(overflow));
        for (var length = 0; length < valid.Length; length++)
            Assert.Null(AdamWPortableState.FromBytes(valid.AsSpan(0, length)));
        Assert.Null(AdamWPortableState.FromBytes(valid.Concat(new byte[] { 0 }).ToArray()));
        var duplicate = valid.Concat(valid.AsSpan(20).ToArray()).ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(duplicate.AsSpan(16, 4), 2);
        Assert.Null(AdamWPortableState.FromBytes(duplicate));
        Assert.Throws<ArgumentException>(() => new AdamWPortableState(2, -1, []));
        Assert.Throws<ArgumentException>(() => new AdamWPortableState(1, 1, []));
        Assert.Throws<ArgumentException>(() =>
            new AdamWPortableParameterState("p", [1], [], []));
        Assert.Throws<ArgumentException>(() =>
            new AdamWPortableParameterState("p", [1], [0f], []));
        Assert.Throws<ArgumentException>(() => new AdamWPortableState(2, 1,
            [new AdamWPortableParameterState(new string('x', 1025), [1], [0f], [0f])]));
    }

    [Fact]
    public void PortableState_SparseAndKeyContractsRemainStable()
    {
        var source = Matrix(1f, 2, 3);
        var lazy = Matrix(2f, 6);
        var optimizer = new AdamW();
        var bindings = Bind([source, lazy]);
        var empty = AdamWPortableState.FromBytes(optimizer.ExportPortableState(bindings).ToBytes())!;
        Assert.Empty(empty.Entries);
        Assert.Equal(0, empty.Step);
        Assert.True(new AdamW().TryImportPortableState(bindings, empty));
        RunOneStep(optimizer, [source]);
        var sparse = optimizer.ExportPortableState(bindings);
        Assert.Single(sparse.Entries);
        var restored = new AdamW();
        Assert.True(restored.TryImportPortableState(bindings, sparse));
        var before = restored.ExportPortableState(bindings).ToBytes();
        foreach (var invalid in new AdamWParameterBinding[][]
        {
            [], [new("other", source)], [new("P0", source)], [new("p0", source), new("p0", lazy)],
            [new("p0", source), new("other", source)], [null!], [new("", source)], [new("p0", null!)]
        })
        {
            Assert.False(restored.TryImportPortableState(invalid, sparse));
            Assert.Equal(before, restored.ExportPortableState(bindings).ToBytes());
        }
        Assert.Throws<ArgumentException>(() => new AdamWPortableState(2, 1,
            [sparse.Entries[0], sparse.Entries[0]]));
        source.Value.Shape.Dimensions[0] = 3; // stale cached TotalElements must not excuse this.
        Assert.False(restored.TryImportPortableState(bindings, sparse));
        Assert.Throws<InvalidOperationException>(() => restored.ExportPortableState(bindings));
        source.Value.Shape.Dimensions[0] = 2;
        Assert.Equal(before, restored.ExportPortableState(bindings).ToBytes());
    }

    private static Parameter Matrix(float value, params int[] dimensions)
    {
        var parameter = new Parameter(new Tensor(new TensorShape(dimensions)));
        Array.Fill(parameter.Value.Data, value);
        return parameter;
    }

    private static AdamWParameterBinding[] Bind(Parameter[] parameters)
        => parameters.Select((p, i) => new AdamWParameterBinding("p" + i, p)).ToArray();

    [Fact]
    public void PortableState_MissingFile_LeavesRestartedOptimizerWithoutHiddenFreshStart()
    {
        var parameter = CreateParameter(4, 1f);
        var bindings = new[] { new AdamWParameterBinding("layer.0.weight", parameter) };

        var path = Path.Combine(
            Path.GetTempPath(),
            "xiigen-adamw-missing-" + Guid.NewGuid().ToString("N") + ".bin");
        Assert.False(File.Exists(path));

        // Отсутствие файла обязано быть видимым отказом на стороне вызывающего
        // кода, а не молчаливым продолжением со свежим оптимизатором.
        Assert.Throws<FileNotFoundException>(() => File.ReadAllBytes(path));

        var restarted = new AdamW();
        var exportedFromEmpty = restarted.ExportPortableState(bindings);
        Assert.Empty(exportedFromEmpty.Entries);
        Assert.Equal(0, exportedFromEmpty.Step);
    }

    [Fact]
    public void PortableState_CorruptBytes_FailClosedWithoutPartialState()
    {
        var parameter = CreateParameter(4, 1f);
        var bindings = new[] { new AdamWParameterBinding("layer.0.weight", parameter) };
        var optimizer = new AdamW();
        RunOneStep(optimizer, new[] { parameter });
        var bytes = optimizer.ExportPortableState(bindings).ToBytes();

        // Порча заголовка.
        var wrongMagic = bytes.ToArray();
        wrongMagic[0] = (byte)(wrongMagic[0] ^ 0xFF);
        Assert.Null(AdamWPortableState.FromBytes(wrongMagic));

        // Обрезанный файл.
        Assert.Null(AdamWPortableState.FromBytes(bytes.AsSpan(0, bytes.Length - 4)));

        // Лишние байты в конце.
        var withTrailer = bytes.Concat(new byte[] { 7, 7, 7, 7 }).ToArray();
        Assert.Null(AdamWPortableState.FromBytes(withTrailer));

        // Пустой файл.
        Assert.Null(AdamWPortableState.FromBytes(Array.Empty<byte>()));
    }

    [Fact]
    public void PortableState_ShapeMismatchOnRestart_RefusesImportAndKeepsOptimizerUntouched()
    {
        var originalParameter = CreateParameter(4, 1f);
        var originalBindings = new[]
        {
            new AdamWParameterBinding("layer.0.weight", originalParameter),
        };

        var optimizer = new AdamW();
        RunOneStep(optimizer, new[] { originalParameter });
        var exported = optimizer.ExportPortableState(originalBindings);

        // После перезапуска модель имеет параметр другой длины под тем же именем.
        var incompatibleParameter = CreateParameter(6, 1f);
        var incompatibleBindings = new[]
        {
            new AdamWParameterBinding("layer.0.weight", incompatibleParameter),
        };

        var restarted = new AdamW();
        Assert.False(restarted.TryImportPortableState(incompatibleBindings, exported));

        // Отказ не должен оставить частично перенесённое состояние.
        var afterRefusal = restarted.ExportPortableState(incompatibleBindings);
        Assert.Empty(afterRefusal.Entries);
        Assert.Equal(0, afterRefusal.Step);
    }

    [Fact]
    public void PortableState_UnknownParameterName_RefusesImport()
    {
        var parameter = CreateParameter(4, 1f);
        var bindings = new[] { new AdamWParameterBinding("layer.0.weight", parameter) };
        var optimizer = new AdamW();
        RunOneStep(optimizer, new[] { parameter });
        var exported = optimizer.ExportPortableState(bindings);

        // Имя параметра из состояния не разрешается в текущей модели.
        var renamedParameter = CreateParameter(4, 1f);
        var renamedBindings = new[]
        {
            new AdamWParameterBinding("layer.0.renamed-weight", renamedParameter),
        };

        var restarted = new AdamW();
        Assert.False(restarted.TryImportPortableState(renamedBindings, exported));
    }

    [Fact]
    public void PortableState_DuplicateBindingKey_RefusesImport()
    {
        var firstParameter = CreateParameter(4, 1f);
        var secondParameter = CreateParameter(4, 9f);
        var validBindings = new[]
        {
            new AdamWParameterBinding("layer.0.weight", firstParameter),
        };

        var optimizer = new AdamW();
        RunOneStep(optimizer, new[] { firstParameter });
        var exported = optimizer.ExportPortableState(validBindings);

        var duplicateBindings = new[]
        {
            new AdamWParameterBinding("layer.0.weight", firstParameter),
            new AdamWParameterBinding("layer.0.weight", secondParameter),
        };

        var restarted = new AdamW();
        Assert.False(restarted.TryImportPortableState(duplicateBindings, exported));
    }
}
