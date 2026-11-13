// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Reflection;
using System.Text.Json;
using DPO;
using DPO.Models;
using TensorCore.Core;
using TransformerModel;
using TransformerModel.Config;

namespace LlmFromScratch.Tests;

// Тесты фиксируют контракт допуска и включенную дверь тренера: legacy данные
// читаются для аудита, но trainer больше не обучается без admission паспорта.
public sealed class DpoTrainerPairAdmissionTests : IDisposable
{
    // Минимальная модель нужна для регрессии старого Step-пути без дорогого runtime.
    private static readonly ModelConfig SmallCfg = new()
    {
        VocabSize = 64,
        ContextLen = 8,
        HiddenDim = 16,
        NumLayers = 1,
        NumHeads = 2,
        NumKvHeads = 1,
        FfnHiddenDim = 32
    };

    private readonly string _checkpointDir = Path.Combine(
        Path.GetTempPath(),
        $"dpo-pair-admission-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_checkpointDir))
            Directory.Delete(_checkpointDir, recursive: true);
    }

    [Fact]
    public void CreateAdmitted_PreservesExactStringsAuthorsAndIssuer()
    {
        var question = "  question\r\nwith spaces  ";
        var chosen = "\tchosen answer\n";
        var rejected = "rejected answer  ";

        var admitted = DpoTrainerPairCustoms.CreateAdmitted(
            question,
            chosen,
            rejected,
            DpoPairSideAuthorKind.User,
            DpoPairSideAuthorKind.OpenTeacher,
            "translation-bridge");

        // Сырые строки проверяются на точное равенство: фабрика не имеет права
        // trim-ить или нормализовать payload, из которого строится fingerprint.
        Assert.Equal(question, admitted.Pair.Prompt);
        Assert.Equal(chosen, admitted.Pair.Chosen);
        Assert.Equal(rejected, admitted.Pair.Rejected);

        // Авторство сторон хранится в admission отдельно от бедной DpoPair.
        Assert.Equal(DpoPairSideAuthorKind.User, admitted.Admission.ChosenAuthor);
        Assert.Equal(
            DpoPairSideAuthorKind.OpenTeacher,
            admitted.Admission.RejectedAuthor);
        Assert.Equal("translation-bridge", admitted.Admission.IssuerName);
        Assert.False(admitted.Admission.HasPaidExternalModelSide);
    }

    [Fact]
    public void CreateAdmitted_KnownInputHasFixedContentFingerprint()
    {
        var admitted = DpoTrainerPairCustoms.CreateAdmitted(
            "question",
            "chosen",
            "rejected",
            DpoPairSideAuthorKind.User,
            DpoPairSideAuthorKind.DeterministicGenerator,
            "unit-test-customs");

        // Это независимый фиксированный вектор для схемы
        // DPO_TRAINER_PAIR_ADMISSION_CONTENT_SHA256_V1 с UTF-8 длиной каждого поля.
        Assert.Equal(
            "1c87f0f936fbeeddca8df6e826d62b3b9118083ba4d6248ca8c552ebf56e7ea5",
            admitted.Admission.ContentFingerprintSha256);
    }

    [Fact]
    public void CreateAdmitted_TextFieldMutationsChangeFingerprintAndRepeatIsStable()
    {
        var baseline = DpoTrainerPairCustoms.CreateAdmitted(
            "prompt",
            "chosen",
            "rejected",
            DpoPairSideAuthorKind.User,
            DpoPairSideAuthorKind.LocalModel,
            "unit-test-customs");
        var repeat = DpoTrainerPairCustoms.CreateAdmitted(
            "prompt",
            "chosen",
            "rejected",
            DpoPairSideAuthorKind.User,
            DpoPairSideAuthorKind.LocalModel,
            "unit-test-customs");

        // Повтор доказывает стабильность fingerprint для тех же трех raw строк.
        Assert.Equal(
            baseline.Admission.ContentFingerprintSha256,
            repeat.Admission.ContentFingerprintSha256);

        // Каждая роль входит в hash отдельно: мутация prompt/chosen/rejected
        // должна менять fingerprint и не может потеряться в склейке строк.
        Assert.NotEqual(
            baseline.Admission.ContentFingerprintSha256,
            FingerprintFor("prompt!", "chosen", "rejected"));
        Assert.NotEqual(
            baseline.Admission.ContentFingerprintSha256,
            FingerprintFor("prompt", "chosen!", "rejected"));
        Assert.NotEqual(
            baseline.Admission.ContentFingerprintSha256,
            FingerprintFor("prompt", "chosen", "rejected!"));
    }

    [Fact]
    public void CreateAdmitted_PaidExternalModelOnEitherSideDerivesPaidFlag()
    {
        var chosenPaid = DpoTrainerPairCustoms.CreateAdmitted(
            "prompt",
            "chosen",
            "rejected",
            DpoPairSideAuthorKind.PaidExternalModel,
            DpoPairSideAuthorKind.User,
            "unit-test-customs");
        var rejectedPaid = DpoTrainerPairCustoms.CreateAdmitted(
            "prompt",
            "chosen",
            "rejected",
            DpoPairSideAuthorKind.OpenTeacher,
            DpoPairSideAuthorKind.PaidExternalModel,
            "unit-test-customs");
        var noPaidSide = DpoTrainerPairCustoms.CreateAdmitted(
            "prompt",
            "chosen",
            "rejected",
            DpoPairSideAuthorKind.OpenTeacher,
            DpoPairSideAuthorKind.LocalModel,
            "unit-test-customs");

        // Вызывающий код не передает отдельный bool: admission выводит
        // флаг платной стороны из enum-значений обеих сторон.
        Assert.True(chosenPaid.Admission.HasPaidExternalModelSide);
        Assert.True(rejectedPaid.Admission.HasPaidExternalModelSide);
        Assert.False(noPaidSide.Admission.HasPaidExternalModelSide);
        Assert.Equal(
            "DPO_PAIR_ADMISSION_MISSING",
            DpoTrainerPairAdmission.MissingErrorCode);
        Assert.Equal(
            "DPO_PAIR_ADMISSION_PAID_MODEL_SIDE",
            DpoTrainerPairAdmission.PaidExternalModelSideErrorCode);
    }

    [Fact]
    public void DpoTrainerPairAdmission_ConstructorsDoNotExposeBooleanPaidSideParameter()
    {
        var constructors = typeof(DpoTrainerPairAdmission).GetConstructors(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Рефлексия проверяет и внутренний дружественный путь: ни текущая сборка,
        // ни ContinuousLearning не должны увидеть конструктор с ручным bool-флагом.
        Assert.NotEmpty(constructors);
        Assert.DoesNotContain(
            constructors,
            constructor => constructor
                .GetParameters()
                .Any(parameter => parameter.ParameterType == typeof(bool)));
    }

    [Fact]
    public void InternalConstructor_DerivesPaidFlagFromAuthors()
    {
        var chosenPaid = new DpoTrainerPairAdmission(
            DpoPairSideAuthorKind.PaidExternalModel,
            DpoPairSideAuthorKind.User,
            "prompt",
            "chosen",
            "rejected",
            "unit-test-customs");
        var rejectedPaid = new DpoTrainerPairAdmission(
            DpoPairSideAuthorKind.OpenTeacher,
            DpoPairSideAuthorKind.PaidExternalModel,
            "prompt",
            "chosen",
            "rejected",
            "unit-test-customs");
        var noPaidSide = new DpoTrainerPairAdmission(
            DpoPairSideAuthorKind.OpenTeacher,
            DpoPairSideAuthorKind.LocalModel,
            "prompt",
            "chosen",
            "rejected",
            "unit-test-customs");

        // Прямой internal-вызов не получает bool и не может рассинхронизировать
        // флаг: конструктор выводит его только из двух авторов.
        Assert.True(chosenPaid.HasPaidExternalModelSide);
        Assert.True(rejectedPaid.HasPaidExternalModelSide);
        Assert.False(noPaidSide.HasPaidExternalModelSide);
    }

    [Fact]
    public void InternalConstructor_RejectsUnknownAuthorsNullTextsAndBlankIssuer()
    {
        // Дружественная сборка не должна обходить публичную таможню: конструктор сам
        // проверяет авторов, исходные тексты для fingerprint и issuer.
        Assert.Throws<ArgumentOutOfRangeException>(() => new DpoTrainerPairAdmission(
            (DpoPairSideAuthorKind)999,
            DpoPairSideAuthorKind.OpenTeacher,
            "prompt",
            "chosen",
            "rejected",
            "unit-test-customs"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new DpoTrainerPairAdmission(
            DpoPairSideAuthorKind.User,
            (DpoPairSideAuthorKind)999,
            "prompt",
            "chosen",
            "rejected",
            "unit-test-customs"));
        Assert.Throws<ArgumentNullException>(() => new DpoTrainerPairAdmission(
            DpoPairSideAuthorKind.User,
            DpoPairSideAuthorKind.OpenTeacher,
            null!,
            "chosen",
            "rejected",
            "unit-test-customs"));
        Assert.Throws<ArgumentNullException>(() => new DpoTrainerPairAdmission(
            DpoPairSideAuthorKind.User,
            DpoPairSideAuthorKind.OpenTeacher,
            "prompt",
            null!,
            "rejected",
            "unit-test-customs"));
        Assert.Throws<ArgumentNullException>(() => new DpoTrainerPairAdmission(
            DpoPairSideAuthorKind.User,
            DpoPairSideAuthorKind.OpenTeacher,
            "prompt",
            "chosen",
            null!,
            "unit-test-customs"));
        Assert.Throws<ArgumentNullException>(() => new DpoTrainerPairAdmission(
            DpoPairSideAuthorKind.User,
            DpoPairSideAuthorKind.OpenTeacher,
            "prompt",
            "chosen",
            "rejected",
            null!));
        Assert.Throws<ArgumentException>(() => new DpoTrainerPairAdmission(
            DpoPairSideAuthorKind.User,
            DpoPairSideAuthorKind.OpenTeacher,
            "prompt",
            "chosen",
            "rejected",
            "   "));
    }

    [Fact]
    public void CreateAdmitted_NullTextRejectsBeforeFingerprinting()
    {
        // Значение null отличается от намеренно пустой строки; поэтому оно не допускается
        // к fingerprint, чтобы bridge не маскировал отсутствие поля.
        Assert.Throws<ArgumentNullException>(() => DpoTrainerPairCustoms.CreateAdmitted(
            null!,
            "chosen",
            "rejected",
            DpoPairSideAuthorKind.User,
            DpoPairSideAuthorKind.OpenTeacher,
            "unit-test-customs"));
        Assert.Throws<ArgumentNullException>(() => DpoTrainerPairCustoms.CreateAdmitted(
            "prompt",
            null!,
            "rejected",
            DpoPairSideAuthorKind.User,
            DpoPairSideAuthorKind.OpenTeacher,
            "unit-test-customs"));
        Assert.Throws<ArgumentNullException>(() => DpoTrainerPairCustoms.CreateAdmitted(
            "prompt",
            "chosen",
            null!,
            DpoPairSideAuthorKind.User,
            DpoPairSideAuthorKind.OpenTeacher,
            "unit-test-customs"));
    }

    [Fact]
    public void CreateAdmitted_UnknownAuthorAndEmptyIssuerReject()
    {
        // Пустой issuer падает закрыто: admission без подписавшей границы нельзя
        // связать с последующим аудитом происхождения.
        Assert.Throws<ArgumentException>(() => DpoTrainerPairCustoms.CreateAdmitted(
            "prompt",
            "chosen",
            "rejected",
            DpoPairSideAuthorKind.User,
            DpoPairSideAuthorKind.OpenTeacher,
            "   "));
        Assert.Throws<ArgumentNullException>(() => DpoTrainerPairCustoms.CreateAdmitted(
            "prompt",
            "chosen",
            "rejected",
            DpoPairSideAuthorKind.User,
            DpoPairSideAuthorKind.OpenTeacher,
            null!));

        // Неизвестный enum проверяется на каждой стороне отдельно: integer cast не
        // должен создавать шестой неописанный класс автора.
        Assert.Throws<ArgumentOutOfRangeException>(() => DpoTrainerPairCustoms.CreateAdmitted(
            "prompt",
            "chosen",
            "rejected",
            (DpoPairSideAuthorKind)999,
            DpoPairSideAuthorKind.OpenTeacher,
            "unit-test-customs"));
        Assert.Throws<ArgumentOutOfRangeException>(() => DpoTrainerPairCustoms.CreateAdmitted(
            "prompt",
            "chosen",
            "rejected",
            DpoPairSideAuthorKind.User,
            (DpoPairSideAuthorKind)999,
            "unit-test-customs"));
    }

    [Fact]
    public void DpoBatch_OptionalAdmissionRoundTripsWithoutTensorLoss()
    {
        var admitted = DpoTrainerPairCustoms.CreateAdmitted(
            "prompt",
            "chosen",
            "rejected",
            DpoPairSideAuthorKind.User,
            DpoPairSideAuthorKind.OpenTeacher,
            "unit-test-customs");
        var batch = MakeBatch([1, 2, 3], [1, 4, 3]) with
        {
            Admission = admitted.Admission
        };
        var copied = batch with { };

        // Слот переносится через record-copy как обычное init-свойство, не
        // перезаписывая tensor-поля существующего batch-контракта.
        Assert.Same(admitted.Admission, copied.Admission);
        Assert.Same(batch.ChosenIds, copied.ChosenIds);
        Assert.Same(batch.RejectedIds, copied.RejectedIds);
        Assert.Same(batch.ChosenMask, copied.ChosenMask);
        Assert.Same(batch.RejectedMask, copied.RejectedMask);
    }

    [Fact]
    public void LegacyThreeFieldDatasetAndOrdinaryTrainerStepFailsMissingAdmission()
    {
        var path = Path.GetTempFileName() + ".jsonl";
        try
        {
            File.WriteAllText(
                path,
                JsonSerializer.Serialize(new DpoPair
                {
                    Prompt = "legacy prompt",
                    Chosen = "legacy chosen",
                    Rejected = "legacy rejected"
                }));

            var dataset = DpoDataset.Load(path);
            var batch = MakeBatch([1, 2, 3], [1, 4, 3]);
            var trainer = CreateMutationAdmittedTrainer(
                new LlmModel(SmallCfg),
                new LlmModel(SmallCfg));

            // Legacy dataset остается читаемым, но тренерная дверь запрещает
            // обучение batch-а без admission даже при живом mutation permit.
            Assert.Equal(1, dataset.Count);
            Assert.Equal("legacy prompt", dataset[0].Prompt);
            Assert.Null(batch.Admission);
            var exception = Assert.Throws<DpoMutationAdmissionException>(() =>
                trainer.Step(batch, step: 12));
            Assert.Equal(DpoTrainerPairAdmission.MissingErrorCode, exception.ErrorCode);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void DpoDatasetLoad_LegacyRowsPreserveExactPairsAndNullAdmission()
    {
        var first = new DpoPair
        {
            Prompt = " legacy prompt\r\n",
            Chosen = "\tlegacy chosen ",
            Rejected = "legacy rejected\n"
        };
        var second = new DpoPair
        {
            Prompt = "second prompt",
            Chosen = "second chosen",
            Rejected = "second rejected"
        };

        var dataset = LoadJsonl(
            JsonSerializer.Serialize(first),
            "   ",
            JsonSerializer.Serialize(second));

        // Legacy JSONL не получает паспорт задним числом: порядок, Count и
        // бедный indexer остаются тем же совместимым контрактом.
        Assert.Equal(2, dataset.Count);
        Assert.Equal(first, dataset[0]);
        Assert.Equal(second, dataset[1]);
        Assert.Null(dataset.GetRow(0).Admission);
        Assert.Null(dataset.Rows[1].Admission);
    }

    [Fact]
    public void DpoDatasetRows_PublicRowsCannotDowncastAndMutatePrivateSnapshot()
    {
        var dataset = LoadJsonl(
            JsonSerializer.Serialize(new DpoPair
            {
                Prompt = "first prompt",
                Chosen = "first chosen",
                Rejected = "first rejected"
            }),
            JsonSerializer.Serialize(new DpoPair
            {
                Prompt = "second prompt",
                Chosen = "second chosen",
                Rejected = "second rejected"
            }));
        var rows = dataset.Rows;

        // Rows нужен для audit sidecar, но он не должен отдавать наружу List:
        // иначе caller смог бы переставить пары и admission после загрузки.
        Assert.IsNotType<List<DpoDatasetRow>>(rows);
        Assert.Throws<NotSupportedException>(
            () => ((IList<DpoDatasetRow>)rows).RemoveAt(0));
        Assert.Equal(2, dataset.Count);
        Assert.Equal("first prompt", dataset[0].Prompt);
        Assert.Equal("second prompt", dataset.GetRow(1).Pair.Prompt);
    }

    [Fact]
    public void DpoDatasetLoad_AdmittedRowsCarryAuthorsIssuerAndFingerprint()
    {
        var row = JsonSerializer.Serialize(new
        {
            Prompt = "admitted prompt",
            Chosen = "admitted chosen",
            Rejected = "admitted rejected",
            chosenAuthor = nameof(DpoPairSideAuthorKind.User),
            rejectedAuthor = nameof(DpoPairSideAuthorKind.OpenTeacher),
            admissionIssuerName = "dataset-loader-test"
        });

        var dataset = LoadJsonl(row);
        var record = dataset.GetRow(0);
        var expected = DpoTrainerPairCustoms.CreateAdmitted(
            "admitted prompt",
            "admitted chosen",
            "admitted rejected",
            DpoPairSideAuthorKind.User,
            DpoPairSideAuthorKind.OpenTeacher,
            "dataset-loader-test");

        // Admission строится только через DpoTrainerPairCustoms, поэтому
        // fingerprint совпадает с независимым customs-rebind тех же raw строк.
        Assert.Equal(expected.Pair, dataset[0]);
        Assert.NotNull(record.Admission);
        Assert.Equal(DpoPairSideAuthorKind.User, record.Admission.ChosenAuthor);
        Assert.Equal(
            DpoPairSideAuthorKind.OpenTeacher,
            record.Admission.RejectedAuthor);
        Assert.Equal("dataset-loader-test", record.Admission.IssuerName);
        Assert.Equal(
            expected.Admission.ContentFingerprintSha256,
            record.Admission.ContentFingerprintSha256);
    }

    [Fact]
    public void DpoDatasetLoad_DuplicateCoreAndAdmissionFieldsRejectClosed()
    {
        var duplicateRows = new[]
        {
            """{"Prompt":"p1","Prompt":"p2","Chosen":"c","Rejected":"r"}""",
            """{"Prompt":"p","Chosen":"c1","Chosen":"c2","Rejected":"r"}""",
            """{"Prompt":"p","Chosen":"c","Rejected":"r1","Rejected":"r2"}""",
            """{"Prompt":"p","Chosen":"c","Rejected":"r","chosenAuthor":"User","chosenAuthor":"OpenTeacher","rejectedAuthor":"User","admissionIssuerName":"dataset-loader-test"}""",
            """{"Prompt":"p","Chosen":"c","Rejected":"r","chosenAuthor":"User","rejectedAuthor":"User","rejectedAuthor":"OpenTeacher","admissionIssuerName":"dataset-loader-test"}""",
            """{"Prompt":"p","Chosen":"c","Rejected":"r","chosenAuthor":"User","rejectedAuthor":"OpenTeacher","admissionIssuerName":"dataset-loader-test","admissionIssuerName":"other-issuer"}"""
        };

        // JsonDocument хранит duplicate properties, поэтому loader обязан
        // перечислить объект сам, а не доверять TryGetProperty "последнему wins".
        foreach (var row in duplicateRows)
            AssertLoadThrows<InvalidOperationException>(row);
    }

    [Fact]
    public void DpoDatasetLoad_WrongCaseCoreAndAdmissionFieldsRejectClosed()
    {
        var wrongCaseRows = new[]
        {
            """{"prompt":"p","Chosen":"c","Rejected":"r"}""",
            """{"Prompt":"p","chosen":"c","Rejected":"r"}""",
            """{"Prompt":"p","Chosen":"c","rejected":"r"}""",
            """{"Prompt":"p","Chosen":"c","Rejected":"r","ChosenAuthor":"User","rejectedAuthor":"OpenTeacher","admissionIssuerName":"dataset-loader-test"}""",
            """{"Prompt":"p","Chosen":"c","Rejected":"r","chosenAuthor":"User","RejectedAuthor":"OpenTeacher","admissionIssuerName":"dataset-loader-test"}""",
            """{"Prompt":"p","Chosen":"c","Rejected":"r","chosenAuthor":"User","rejectedAuthor":"OpenTeacher","AdmissionIssuerName":"dataset-loader-test"}"""
        };

        // Политика exact-case закрывает silent downgrade: близкое имя поля - это
        // ошибка источника данных, а не unknown extension metadata.
        foreach (var row in wrongCaseRows)
            AssertLoadThrows<InvalidOperationException>(row);
    }

    [Fact]
    public void DpoDatasetLoad_WrongCaseAdmissionTrioCannotSilentlyBecomeLegacy()
    {
        const string row = """
            {"Prompt":"prompt","Chosen":"chosen","Rejected":"rejected","ChosenAuthor":"User","RejectedAuthor":"OpenTeacher","AdmissionIssuerName":"dataset-loader-test"}
            """;

        // Все три admission поля в неправильном case похожи на паспорт, поэтому
        // loader обязан отказать, а не проглотить их как unrelated legacy поля.
        AssertLoadThrows<InvalidOperationException>(row);
    }

    [Fact]
    public void DpoDatasetLoad_UnknownUnrelatedFieldsStayIgnored()
    {
        const string legacy = """
            {"Prompt":"legacy prompt","Chosen":"legacy chosen","Rejected":"legacy rejected","sourceNote":"audit-only"}
            """;
        const string admitted = """
            {"Prompt":"admitted prompt","Chosen":"admitted chosen","Rejected":"admitted rejected","chosenAuthor":"User","rejectedAuthor":"OpenTeacher","admissionIssuerName":"dataset-loader-test","sourceNote":"audit-only"}
            """;

        var dataset = LoadJsonl(legacy, admitted);

        // Неродственные extension поля не ломают совместимость; строгой остается
        // только case/duplicate политика для полей, которые влияют на пару и admission.
        Assert.Null(dataset.GetRow(0).Admission);
        Assert.NotNull(dataset.GetRow(1).Admission);
        Assert.Equal("legacy prompt", dataset[0].Prompt);
        Assert.Equal("admitted prompt", dataset[1].Prompt);
    }

    [Fact]
    public void DpoDatasetLoad_PartialUnknownAndCorruptAdmissionRowsRejectClosed()
    {
        var partial = JsonSerializer.Serialize(new
        {
            Prompt = "prompt",
            Chosen = "chosen",
            Rejected = "rejected",
            chosenAuthor = nameof(DpoPairSideAuthorKind.User)
        });
        var unknown = JsonSerializer.Serialize(new
        {
            Prompt = "prompt",
            Chosen = "chosen",
            Rejected = "rejected",
            chosenAuthor = "ClosedTeacherAlias",
            rejectedAuthor = nameof(DpoPairSideAuthorKind.User),
            admissionIssuerName = "dataset-loader-test"
        });
        var numeric = """
            {"Prompt":"prompt","Chosen":"chosen","Rejected":"rejected","chosenAuthor":1,"rejectedAuthor":"User","admissionIssuerName":"dataset-loader-test"}
            """;

        // Partial/unknown/numeric authorship падает до создания admission:
        // loader не угадывает автора и не мигрирует старую строку автоматически.
        AssertLoadThrows<InvalidOperationException>(partial);
        AssertLoadThrows<InvalidOperationException>(unknown);
        AssertLoadThrows<InvalidOperationException>(numeric);
        AssertLoadThrows<JsonException>("{");
        AssertLoadThrows<InvalidOperationException>("[]");
    }

    [Fact]
    public void DpoDatasetLoad_CountIndexerAndShuffleStayCompatibleWithAdmittedRows()
    {
        var rows = Enumerable.Range(0, 6)
            .Select(index => JsonSerializer.Serialize(new
            {
                Prompt = $"prompt-{index}",
                Chosen = $"chosen-{index}",
                Rejected = $"rejected-{index}",
                chosenAuthor = nameof(DpoPairSideAuthorKind.DeterministicGenerator),
                rejectedAuthor = nameof(DpoPairSideAuthorKind.DeterministicGenerator),
                admissionIssuerName = "dataset-loader-test"
            }))
            .ToArray();

        var dataset = LoadJsonl(rows);
        var order1 = dataset.Shuffle(seed: 127).Select(pair => pair.Prompt).ToArray();
        var order2 = dataset.Shuffle(seed: 127).Select(pair => pair.Prompt).ToArray();

        // Shuffle возвращает прежний IEnumerable<DpoPair>; sidecar admission
        // не меняет seed-дeтерминизм и не мутирует исходный порядок indexer.
        Assert.Equal(6, dataset.Count);
        Assert.Equal("prompt-0", dataset[0].Prompt);
        Assert.All(dataset.Rows, row => Assert.NotNull(row.Admission));
        Assert.Equal(order1, order2);
        Assert.Equal(
            Enumerable.Range(0, 6).Select(index => $"prompt-{index}").Order(),
            order1.Order());
        Assert.Equal("prompt-0", dataset[0].Prompt);
    }

    [Fact]
    public void DpoDatasetLoad_PaidExternalSideStaysReadableWithoutTrainableClaim()
    {
        var row = JsonSerializer.Serialize(new
        {
            Prompt = "paid prompt",
            Chosen = "paid chosen",
            Rejected = "paid rejected",
            chosenAuthor = nameof(DpoPairSideAuthorKind.PaidExternalModel),
            rejectedAuthor = nameof(DpoPairSideAuthorKind.User),
            admissionIssuerName = "dataset-loader-test"
        });

        var dataset = LoadJsonl(row);
        var admission = dataset.GetRow(0).Admission;

        // Paid side доступна для аудита/research, но dataset row не содержит
        // IsTrainable/Trainable флага и не открывает будущую дверь тренера.
        Assert.Equal("paid prompt", dataset[0].Prompt);
        Assert.NotNull(admission);
        Assert.True(admission.HasPaidExternalModelSide);
        Assert.DoesNotContain(
            typeof(DpoDatasetRow).GetProperties(),
            property => property.Name.Contains(
                "Trainable",
                StringComparison.OrdinalIgnoreCase));
    }

    private static string FingerprintFor(
        string prompt,
        string chosen,
        string rejected)
        => DpoTrainerPairCustoms
            .CreateAdmitted(
                prompt,
                chosen,
                rejected,
                DpoPairSideAuthorKind.User,
                DpoPairSideAuthorKind.LocalModel,
                "unit-test-customs")
            .Admission
            .ContentFingerprintSha256;

    private static DpoDataset LoadJsonl(params string[] lines)
    {
        var path = Path.GetTempFileName() + ".jsonl";
        try
        {
            File.WriteAllLines(path, lines);
            return DpoDataset.Load(path);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    private static void AssertLoadThrows<TException>(params string[] lines)
        where TException : Exception
    {
        var path = Path.GetTempFileName() + ".jsonl";
        try
        {
            File.WriteAllLines(path, lines);
            Assert.ThrowsAny<TException>(() => DpoDataset.Load(path));
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    private static DpoBatch MakeBatch(int[] chosenIds, int[] rejectedIds)
    {
        var cIds = chosenIds.Select(id => (float)id).ToArray();
        var rIds = rejectedIds.Select(id => (float)id).ToArray();
        var cMask = Enumerable.Repeat(1f, chosenIds.Length).ToArray();
        var rMask = Enumerable.Repeat(1f, rejectedIds.Length).ToArray();

        return new DpoBatch
        {
            ChosenIds = new Tensor(cIds, new TensorShape(1, chosenIds.Length)),
            RejectedIds = new Tensor(rIds, new TensorShape(1, rejectedIds.Length)),
            ChosenMask = new Tensor(cMask, new TensorShape(1, chosenIds.Length)),
            RejectedMask = new Tensor(rMask, new TensorShape(1, rejectedIds.Length))
        };
    }

    private DpoTrainer CreateMutationAdmittedTrainer(
        LlmModel policy,
        LlmModel reference)
        => new(
            policy,
            reference,
            _checkpointDir,
            DpoMutationAdmissionCapability.Issue(new TestDpoMutationAdmissionLease()));

    // Аренда выдает только разрешение на mutation для существующей двери тренера;
    // она не подменяет новый admission пары, который в этой фазе еще не применяется.
    private sealed class TestDpoMutationAdmissionLease
        : IDpoMutationAdmissionIssuer
    {
        IDisposable IDpoMutationAdmissionIssuer.AcquireDpoTrainerMutationPermit()
            => new CancellationTokenSource();
    }
}
