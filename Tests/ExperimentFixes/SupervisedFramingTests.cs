// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DPO;
using DPO.Models;
using TensorCore.Layers;
using Tokenizer;

/// <summary>
/// Проверки переноса «обучение и чтение ответа используют одинаковый
/// разделитель». Ни одного обучающего обновления здесь не выполняется: это
/// договорные проверки формата, границы ответа и права на остановку, плюс
/// защита формата прямого сравнения предпочтений (DPO — Direct Preference
/// Optimization), который разделителя не получает.
/// </summary>
public sealed class SupervisedFramingTests
{
    /// <summary>
    /// Писатель обучения обязан получить ровно те же знаки, что раньше давал
    /// один вызов словаря на склейке «запрос, перевод строки, ответ». Шпион на
    /// действующем исполнителе учебного шага видит фактические знаки и вес и
    /// не вызывает родной тренер, поэтому ни одного настоящего обновления весов
    /// не происходит. Это проверка подключения формата, а не доказательство
    /// обучения.
    /// </summary>
    [Fact]
    public async Task JointLfEncoding_EqualsIncrementalTrainerInput()
    {
        using var fixture = new TinyTrainingFixtureCL();
        var spy = new RecordingSupervisedStepExecutorSep();
        var trainer = new IncrementalTrainer(
            new NullSessionStorageSep(),
            fixture.CreateFactory(),
            null,
            new StableDpoTrainingExecutor(),
            spy,
            fixture.AdapterDirectory);

        // Одна учебная строка и пустой список пар предпочтений: путь обучения
        // с учителем проходит, путь прямого сравнения предпочтений не вызывается.
        var result = await trainer.TrainAsync(
            "BUG",
            [fixture.Example("BUG", "prompt", "response", 0.75d)],
            []);

        // Шпион не двигает веса, поэтому тренер честно отказывается публиковать
        // переходник: именно этот отказ доказывает, что настоящих обновлений нет.
        Assert.Equal("NO_ADAPTER_UPDATE", result.ErrorCode);
        var captured = Assert.Single(spy.CapturedTokenSequences);
        var tokenizer = LoadFixtureTokenizer(fixture);
        // Ожидаемые знаки берутся прежним способом: один вызов словаря.
        var expected = tokenizer.Encode("prompt\nresponse");
        Assert.Equal(expected, captured);
        // Тот же список знаков обязан выдавать и единственный владелец формата.
        Assert.Equal(
            expected,
            SupervisedTextSequence.EncodeJointLf(tokenizer, "prompt", "response"));
        // Вес учебной строки передаётся в исполнителя без изменения.
        Assert.Equal(0.75f, Assert.Single(spy.CapturedWeights));
        // Имя формата зафиксировано: по нему чтения узнают свой профиль.
        Assert.Equal("sft-joint-lf-v1", SupervisedTextSequence.ProfileVersion);
        // Разделитель — ровно один перевод строки.
        Assert.Equal("\n", SupervisedTextSequence.Separator);
    }

    /// <summary>
    /// Границу ответа нельзя назвать, если словарь слил знак через стык запроса
    /// и ответа. Диагностика обязана отказаться словами
    /// response_boundary_unresolved и не отбрасывать учебную строку: полные
    /// знаки для обучения с учителем остаются доступными.
    /// </summary>
    [Fact]
    public void BoundaryMergedByBpe_IsNotScoredAsResponse()
    {
        var tokenizer = new BpeTokenizer();
        // Словарь обучается только на переводах строки: единственное слияние
        // возможно внутри пробельного куска, ровно поперёк стыка.
        tokenizer.Train("\n\n\n\n", 257);
        const string prompt = "p";
        const string response = "\n\nb";

        var span = SupervisedTextSequence.ResolveResponseSpan(
            tokenizer,
            prompt,
            response);

        // Отказ назван точным словом и не превращается в тихое «не выучено».
        Assert.False(span.IsSuccess);
        Assert.Equal("response_boundary_unresolved", span.ErrorCode);
        // Читатель ответа повторяет тот же отказ, потому что берёт границу там же.
        var prepared = SupervisedResponseReadout.Prepare(
            tokenizer,
            prompt,
            response,
            SupervisedResponseReadout.SupervisedJointLfProfile);
        Assert.False(prepared.IsSuccess);
        Assert.Equal("response_boundary_unresolved", prepared.ErrorCode);
        // Обучение на этой строке по-прежнему возможно: знаки никуда не делись.
        var joint = SupervisedTextSequence.EncodeJointLf(tokenizer, prompt, response);
        Assert.True(joint.Count >= 2);
        Assert.Equal(tokenizer.Encode("p\n\n\nb"), joint);
        // Знаки запроса с разделителем действительно не являются началом строки.
        var promptWithSeparator = tokenizer.Encode("p\n");
        Assert.NotEqual(
            promptWithSeparator,
            joint.Take(promptWithSeparator.Count).ToList());
    }

    /// <summary>
    /// Разбор границы обязан совпадать со сдвигом родного тренера: по входу с
    /// номером i предсказывается знак с номером i + 1, а ответными считаются
    /// цели начиная с номера «длина запроса с разделителем минус один».
    /// Условие «номер не меньше длины запроса» снова потеряло бы первый знак
    /// ответа — именно эту ошибку и чинит перенос. Ни одного вызова модели.
    /// </summary>
    [Fact]
    public void ResponseSpan_StartsAtFirstResponseTargetAndMatchesTrainerShift()
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("tiny tokenizer corpus", 256);
        const string prompt = "// file: Abs.cs";
        const string response = "static int Abs(int n)=>n<0?-n:n;";

        var span = SupervisedTextSequence.ResolveResponseSpan(
            tokenizer,
            prompt,
            response);

        Assert.True(span.IsSuccess, $"{span.ErrorCode}: {span.ErrorMessage}");
        var value = span.Value!;
        var full = tokenizer.Encode("// file: Abs.cs\nstatic int Abs(int n)=>n<0?-n:n;");
        var promptWithSeparator = tokenizer.Encode("// file: Abs.cs\n");
        // Читаются те же знаки, что уходят в обучение.
        Assert.Equal(full, value.FullSequenceIds);
        // Граница ответа равна длине запроса вместе с разделителем.
        Assert.Equal(promptWithSeparator.Count, value.PromptWithSeparatorTokenCount);
        // Знаменатель равен числу ответных знаков: длина строки минус граница.
        Assert.Equal(
            full.Count - promptWithSeparator.Count,
            value.ResponseTargetCount);
        // Первая ответная цель стоит на номере «граница минус один».
        Assert.Equal(
            promptWithSeparator.Count - 1,
            value.ResponseTargetIndices[0]);
        // Последняя ответная цель — последняя сдвинутая позиция строки.
        Assert.Equal(full.Count - 2, value.ResponseTargetIndices[^1]);
        // Сдвиг входов и целей дословно совпадает со сдвигом родного тренера.
        Assert.Equal(full.Count - 1, value.ShiftedInputIds.Count);
        for (var index = 0; index < value.ShiftedInputIds.Count; index++)
        {
            Assert.Equal(full[index], value.ShiftedInputIds[index]);
            Assert.Equal(full[index + 1], value.ShiftedTargetIds[index]);
        }

        // Отпечаток последовательности непустой и имеет длину шестнадцати байт
        // по два знака: по нему отчёт доказывает, что читались те же знаки.
        Assert.Equal(64, value.SequenceDigestSha256.Length);
        // Диагностический контроль без разделителя даёт другой отпечаток.
        var control = SupervisedResponseReadout.Prepare(
            tokenizer,
            prompt,
            response,
            SupervisedResponseReadout.DiagnosticNoSeparatorProfile);
        Assert.True(control.IsSuccess, $"{control.ErrorCode}: {control.ErrorMessage}");
        Assert.NotEqual(
            value.SequenceDigestSha256,
            control.Value!.SequenceDigestSha256);
        // У контроля граница считается по знакам запроса без разделителя.
        Assert.Equal(
            tokenizer.Encode(prompt).Count,
            control.Value!.PromptBoundaryTokenCount);
    }

    /// <summary>
    /// Фактические учебные знаки сверяются до любого вызова модели: если
    /// читателю дали не те знаки, он отказывается, а не считает промахи.
    /// </summary>
    [Fact]
    public void Prepare_RejectsForeignActualTrainingIdsAndUnknownProfile()
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("tiny tokenizer corpus", 256);
        const string prompt = "prompt";
        const string response = "response";

        // Чужие знаки: намеренно подаём склейку без разделителя.
        var foreign = tokenizer.Encode(prompt).Concat(tokenizer.Encode(response)).ToList();
        var mismatched = SupervisedResponseReadout.Prepare(
            tokenizer,
            prompt,
            response,
            SupervisedResponseReadout.SupervisedJointLfProfile,
            foreign);
        Assert.False(mismatched.IsSuccess);
        Assert.Equal("SUPERVISED_READOUT_ACTUAL_IDS_MISMATCH", mismatched.ErrorCode);

        // Те же знаки, что отдаёт владелец формата, принимаются.
        var matched = SupervisedResponseReadout.Prepare(
            tokenizer,
            prompt,
            response,
            SupervisedResponseReadout.SupervisedJointLfProfile,
            SupervisedTextSequence.EncodeJointLf(tokenizer, prompt, response));
        Assert.True(matched.IsSuccess, $"{matched.ErrorCode}: {matched.ErrorMessage}");

        // Неизвестное имя профиля не подменяется молча ни одним из двух форматов.
        var unknown = SupervisedResponseReadout.Prepare(
            tokenizer,
            prompt,
            response,
            "some-other-profile");
        Assert.False(unknown.IsSuccess);
        Assert.Equal("SUPERVISED_READOUT_PROFILE_UNKNOWN", unknown.ErrorCode);
    }

    /// <summary>
    /// Диагностика остановки истинна только на профиле обучения, при ненулевом
    /// знаменателе и при полном совпадении всех ответных знаков. Чужой профиль
    /// и пустой знаменатель — отказы, а не «ещё не выучено». Ни одного вызова
    /// модели: читаются уже вычисленные результаты.
    /// </summary>
    [Fact]
    public void AcquisitionStop_RequiresSupervisedProfileNonEmptyDenominatorAndFullMatch()
    {
        // Полное совпадение на профиле обучения: остановка разрешена.
        var full = Readout(
            SupervisedResponseReadout.SupervisedJointLfProfile,
            correctTokens: 23,
            responseTargetCount: 23);
        var satisfied = SftAcquisitionStop.IsSatisfied(full);
        Assert.True(satisfied.IsSuccess, $"{satisfied.ErrorCode}: {satisfied.ErrorMessage}");
        Assert.True(satisfied.Value);

        // Один промах — остановка не разрешена, но это не отказ прибора.
        var partial = Readout(
            SupervisedResponseReadout.SupervisedJointLfProfile,
            correctTokens: 22,
            responseTargetCount: 23);
        var notSatisfied = SftAcquisitionStop.IsSatisfied(partial);
        Assert.True(notSatisfied.IsSuccess, $"{notSatisfied.ErrorCode}: {notSatisfied.ErrorMessage}");
        Assert.False(notSatisfied.Value);

        // Диагностический профиль без разделителя не управляет остановкой.
        var control = Readout(
            SupervisedResponseReadout.DiagnosticNoSeparatorProfile,
            correctTokens: 23,
            responseTargetCount: 23);
        var incompatible = SftAcquisitionStop.IsSatisfied(control);
        Assert.False(incompatible.IsSuccess);
        Assert.Equal("SFT_STOP_PROFILE_INCOMPATIBLE", incompatible.ErrorCode);

        // Пустой знаменатель — отказ, а не ноль правильных ответов.
        var empty = Readout(
            SupervisedResponseReadout.SupervisedJointLfProfile,
            correctTokens: 0,
            responseTargetCount: 0);
        var emptyDenominator = SftAcquisitionStop.IsSatisfied(empty);
        Assert.False(emptyDenominator.IsSuccess);
        Assert.Equal("SFT_STOP_EMPTY_DENOMINATOR", emptyDenominator.ErrorCode);
        // Доля правильных знаков при пустом знаменателе отсутствует, а не равна нулю.
        Assert.Null(empty.ResponseCorrectFraction);
        // При ненулевом знаменателе доля считается из тех же целых чисел.
        Assert.Equal(22d / 23d, partial.ResponseCorrectFraction!.Value, precision: 12);
    }

    /// <summary>
    /// Формат прямого сравнения предпочтений остаётся без разделителя. Новый
    /// единый формат обучения с учителем не вставляет перевод строки в
    /// подготовленную пару, не меняет маски и не меняет отпечаток пары.
    /// </summary>
    [Fact]
    public void ExactDpoFraming_RemainsNoSeparator()
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("abab abab abab abab", 260);
        const string prompt = "a";
        const string chosen = "b";
        const string rejected = "c";
        var batch = DpoExactPairMaterial.CreateBatch(
            tokenizer,
            prompt,
            chosen,
            rejected,
            DpoTrainerPairCustoms.CreateAdmitted(
                prompt,
                chosen,
                rejected,
                DpoPairSideAuthorKind.DeterministicGenerator,
                DpoPairSideAuthorKind.DeterministicGenerator,
                "fix-sep-exact-dpo-unchanged").Admission);

        // Имена версий формата и цели обучения по предпочтениям не изменились.
        Assert.Equal("segmented-byte-BPE-v2", DpoExactPairMaterial.Version);
        Assert.Equal(
            "prompt-response-segments-no-separator-v2",
            DpoExactPairMaterial.FramingVersion);
        Assert.Equal(
            "response-sum-softplus-signal-weighted-v3",
            DpoExactPairMaterial.ObjectiveVersion);
        Assert.Equal(DpoExactPairMaterial.Version, batch.MaterialVersion);
        // Знаки пары — это раздельные знаки запроса и ответа, склеенные встык.
        var segmented = tokenizer.Encode(prompt)
            .Concat(tokenizer.Encode(chosen))
            .ToArray();
        Assert.Equal(
            segmented.Select(static value => (float)value),
            batch.ChosenIds.Data);
        // Разделителя обучения с учителем в паре нет ни на одной позиции.
        var separatorIds = tokenizer.Encode(SupervisedTextSequence.Separator);
        Assert.DoesNotContain(
            (float)separatorIds[0],
            batch.ChosenIds.Data);
        // Отпечаток пары остаётся шестидесяти четырёх знаков и привязан к паре.
        Assert.Equal(64, batch.ExactMaterial!.DigestSha256.Length);
        // Длина запроса считается по знакам запроса без разделителя.
        Assert.Equal(
            tokenizer.Encode(prompt).Count,
            batch.ExactMaterial!.PromptLength);
    }

    // Небольшая сборка результата чтения для проверок предиката остановки.
    private static SupervisedResponseReadoutResult Readout(
        string profileVersion,
        int correctTokens,
        int responseTargetCount)
        => new(
            profileVersion,
            new string('0', 64),
            correctTokens,
            responseTargetCount,
            correctTokens == responseTargetCount && responseTargetCount > 0,
            -0.01d,
            0.01d,
            SupervisedResponseReadout.ResponseScoreAggregation,
            [],
            []);

    // Словарь учебной оснастки читается тем же файлом, что отдан тренеру.
    private static BpeTokenizer LoadFixtureTokenizer(TinyTrainingFixtureCL fixture)
    {
        using var stream = File.OpenRead(fixture.TokenizerPath);
        return BpeTokenizer.Load(stream);
    }
}

// Шпион учебного шага: сохраняет фактические знаки и вес и не вызывает родной
// тренер, поэтому ни одного настоящего обновления весов не происходит.
file sealed class RecordingSupervisedStepExecutorSep : ISupervisedTrainingStepExecutor
{
    public List<IReadOnlyList<int>> CapturedTokenSequences { get; } = [];

    public List<float> CapturedWeights { get; } = [];

    public float Execute(
        IncrementalTrainingSession session,
        IReadOnlyList<int> tokenIds,
        float weight,
        IReadOnlyList<Parameter> adapterParameters)
    {
        // Знаки сохраняются копией, чтобы позже их нельзя было подменить.
        CapturedTokenSequences.Add(tokenIds.ToArray());
        // Вес учебной строки сохраняется как есть.
        CapturedWeights.Add(weight);
        // Возвращается конечная потеря: тренер обязан принять её как конечное число.
        return 0.5f;
    }
}

// Хранилище сессий-заглушка: тренеру оно нужно только как обязательная зависимость.
file sealed class NullSessionStorageSep : ISessionStorage
{
    public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
        => Task.FromResult(OperationResult<bool>.Ok(true));

    public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
        => Task.FromResult(OperationResult<AgentState>.Fail(
            "Состояние сессии не сохраняется в этой проверке.",
            "NULL_STORAGE"));

    public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
        => Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok([]));
}
