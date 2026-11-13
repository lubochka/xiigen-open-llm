// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure.Logical;

// Что здесь проверяется, по-человечески.
//
// Чтобы честно сказать «модель умеет обобщать», экзаменационные строки не должны встречаться в
// обучении — ни теми же байтами, ни под новым именем. Сторож происхождения материала проверяет
// это по ФАКТИЧЕСКИМ строкам до любого обучения.
//
// Два дефекта опытного проверяющего, которые здесь закреплены как отказы. Опытный проверяющий
// из опыта сто двенадцатого (`experiments-v2/RP01-MICRO/P11201/src/MaterialLedgerVerifier.cs`)
// искал совпадение по ПРЕДЪЯВЛЕННЫМ полям отпечатка (строки 61 и 64), ни разу не пересчитывая
// их из содержания, и при неоднозначности брал первую запись (строки 37 и 42). На таких входах
// он выдавал приём. Здесь оба входа обязаны давать названный отказ.
//
// О «красном прогоне на прежней логике». У этих файлов прежней логики не существует: сторож
// написан заново, и запускать на нём нечего. Прежним механизмом был опытный проверяющий, и на
// входах проверок «подделанный отпечаток» и «неоднозначная семья» он давал ПРИЁМ — именно это
// расхождение здесь и закреплено. Утверждать красный прогон на несуществующем предшественнике я
// не могу и не утверждаю.
//
// Чего эти проверки НЕ доказывают. Они не доказывают роста качества: на внешнем экзамене опыта
// оба плеча дали два верных ответа из четырёх, ровно как постоянный ответ, и один класс строк
// просел с двух из двух до нуля из двух. Они не доказывают независимость ПОСЛЕ превращения
// текста в числа для модели: источник-непересекающийся не означает непересекающийся после
// кодирования, и в расписке это поле честно стоит как «не измерено». Они проверяют сам сторож, а
// не его подключение к обучателю: подключение закрыто обзором кода и сборкой, но не прогоном.
public sealed class CapabilityMaterialLineageTests
{
    // ── Замороженный доверенный источник ──────────────────────────────────────

    // Строки источника: три учебные, одна промежуточной проверки и одна экзаменационная.
    private static IReadOnlyList<CapabilitySourceRow> HonestSourceRows() =>
    [
        new("s1", "g1", CapabilityMaterialRole.Train, "Review the code before merging", "ok"),
        new("s2", "g2", CapabilityMaterialRole.Train, "Build the project before testing", "ok"),
        new("s3", "g3", CapabilityMaterialRole.Train, "Run the tests before pushing", "ok"),
        new("s4", "g4", CapabilityMaterialRole.Dev, "Check the result before returning", "ok"),
        new("s5", "g5", CapabilityMaterialRole.Test, "Before implementing code, review it", "ok")
    ];

    // Договор оценки с выбранным приведением текста и политикой независимых семей.
    private static CapabilityEvaluationMaterial Material(
        IReadOnlyList<CapabilitySourceRow>? sourceRows = null) =>
        new(
            ProfileId: "p112-family-disjoint",
            SourceManifestSha256: "0000000000000000000000000000000000000000000000000000000000000000",
            SourceRows: sourceRows ?? HonestSourceRows(),
            NormalizationPolicy: CapabilityMaterialNormalizationPolicy.LowerCaseAndPunctuationStripped,
            IntersectionPolicy: CapabilityMaterialIntersectionPolicy.IndependentFamilies);

    // ── Честная основа: целые семьи по разные стороны границы ─────────────────

    [Fact]
    public void WholeFamiliesWithActualOwnedRows_AreAdmittedWithRecomputedCounts()
    {
        var material = Material();
        // Фактические строки повторяют источник, и их отпечатки посчитаны честно.
        var actual = material.SourceRows.Select(row => ActualOf(row, row.Content)).ToArray();

        var admission = CapabilityMaterialLineageGuard.ValidateBeforeLearning(material, actual);

        Assert.True(admission.IsSuccess, admission.ErrorMessage);
        // Счётчики в расписке пересчитаны сторожем, а не взяты из заявки.
        Assert.Equal(5, admission.Value!.CheckedRowCount);
        Assert.Equal(3, admission.Value.TrainRowCount);
        Assert.Equal(1, admission.Value.DevRowCount);
        Assert.Equal(1, admission.Value.TestRowCount);
        // Семьи обучения и отложенного материала не пересекаются.
        Assert.Equal(["g1", "g2", "g3"], admission.Value.TrainFamilies);
        Assert.Equal(["g4", "g5"], admission.Value.HeldOutFamilies);
        // Независимость после кодирования текста для модели здесь не измерялась, и так и написано.
        Assert.Equal(CapabilityMaterialAdmissionReceipt.NotMeasured, admission.Value.ModelVisibleCollisionCheck);
    }

    // ── Требование 1: предъявленный отпечаток сверяется с пересчитанным ───────

    [Fact]
    public void ChangedActualTextWithTheOldPresentedHash_IsRejectedBeforeLearning()
    {
        var material = Material();
        var honest = HonestSourceRows();

        // Фактический текст учебной строки изменён, а предъявленные отпечатки остались старыми —
        // ровно тот вход, на котором опытный проверяющий искал по заявке и принимал её.
        var tampered = new CapabilityActualMaterialRow(
            ExampleId: "e1",
            ClaimedSourceRowId: "s1",
            Role: CapabilityMaterialRole.Train,
            Content: "Review the code before merging AND SILENTLY ADDED TEXT",
            PresentedContentSha256: Sha256(honest[0].Content),
            PresentedNormalizedSha256: Sha256(NormalizeLikeGuard(honest[0].Content)));

        var actual = new[] { tampered }
            .Concat(honest.Skip(1).Select(row => ActualOf(row, row.Content)))
            .ToArray();

        var admission = CapabilityMaterialLineageGuard.ValidateBeforeLearning(material, actual);

        Assert.False(admission.IsSuccess);
        // Код отказа называет ровно эту причину, а не общее «материал плохой».
        Assert.Equal(
            CapabilityMaterialLineageGuard.PresentedDigestMismatchErrorCode,
            admission.ErrorCode);
        // Причина написана человеческими словами и называет виновную строку.
        Assert.Contains("e1", admission.ErrorMessage);
    }

    // ── Требование 2: неоднозначная семья — отказ, а не молчаливый выбор ──────

    [Fact]
    public void AmbiguousNormalizedFamilies_GiveTypedRefusalWithoutPickingTheFirstOne()
    {
        // Две строки источника РАЗНЫХ семей, совпадающие после приведения текста.
        var ambiguousSource = new CapabilitySourceRow[]
        {
            new("s1", "gA", CapabilityMaterialRole.Train, "Review the code", "ok"),
            new("s2", "gB", CapabilityMaterialRole.Train, "review the code.", "ok"),
            new("s3", "gC", CapabilityMaterialRole.Test, "Build the project", "ok")
        };
        var material = Material(ambiguousSource);

        // Третье написание того же текста: точных байтов такого в источнике нет, а после
        // приведения он совпадает сразу с двумя семьями.
        var actual = new[]
        {
            ActualOf(ambiguousSource[0], "Review the code!", claimedSourceRowId: "s1"),
            ActualOf(ambiguousSource[2], ambiguousSource[2].Content)
        };

        var admission = CapabilityMaterialLineageGuard.ValidateBeforeLearning(material, actual);

        Assert.False(admission.IsSuccess);
        // Отказ типизирован: это именно неоднозначность происхождения.
        Assert.Equal(
            CapabilityMaterialLineageGuard.AmbiguousContentLineageErrorCode,
            admission.ErrorCode);
        // В причине названы ОБЕ семьи: молчаливого выбора первой не произошло.
        Assert.Contains("gA", admission.ErrorMessage);
        Assert.Contains("gB", admission.ErrorMessage);
    }

    // ── Требование 3: запрещённое пересечение отвергается до обучения ─────────

    [Fact]
    public void ExamContentAppearingInTrainUnderANewName_IsRejectedBeforeLearning()
    {
        // Источник, в котором экзаменационная строка присутствует и как учебная: это и есть
        // запрещённое пересечение содержания по явно заданной политике.
        var overlappingSource = new CapabilitySourceRow[]
        {
            new("s1", "g1", CapabilityMaterialRole.Train, "Review the code before merging", "ok"),
            new("s5train", "g5", CapabilityMaterialRole.Train, "Before implementing code, review it", "ok"),
            new("s5test", "g5", CapabilityMaterialRole.Test, "Before implementing code, review it", "ok")
        };
        var material = Material(overlappingSource);

        // Фактические строки честные: отпечатки верные, связь со строками источника верная.
        var actual = new[]
        {
            ActualOf(overlappingSource[0], overlappingSource[0].Content),
            ActualOf(overlappingSource[1], overlappingSource[1].Content),
            ActualOf(overlappingSource[2], overlappingSource[2].Content)
        };

        var admission = CapabilityMaterialLineageGuard.ValidateBeforeLearning(material, actual);

        Assert.False(admission.IsSuccess);
        // Отказ именно про пересечение содержания, а не про отпечаток: форма материала верна.
        Assert.Equal(
            CapabilityMaterialLineageGuard.ForbiddenContentOverlapErrorCode,
            admission.ErrorCode);
    }

    [Fact]
    public void SameFamilyOnBothSidesOfTheBorder_IsRejectedEvenWhenTheRowsDiffer()
    {
        // Строки разные, но происхождение одно: независимой оценки на таком разбиении нет.
        // Это плечо «по строкам» из опыта: пересечение семей там законно равнялось двум, и для
        // продуктовой оценки независимости оно становится запретом.
        var sharedFamilySource = new CapabilitySourceRow[]
        {
            new("s1", "g1", CapabilityMaterialRole.Train, "Review the code before merging", "ok"),
            new("s2", "g3", CapabilityMaterialRole.Train, "Run the tests before pushing", "ok"),
            new("s3", "g3", CapabilityMaterialRole.Test, "Run every test before pushing again", "ok")
        };
        var material = Material(sharedFamilySource);

        var actual = sharedFamilySource.Select(row => ActualOf(row, row.Content)).ToArray();

        var admission = CapabilityMaterialLineageGuard.ValidateBeforeLearning(material, actual);

        Assert.False(admission.IsSuccess);
        // Отказ называет пересечение семей и саму семью.
        Assert.Equal(
            CapabilityMaterialLineageGuard.ForbiddenFamilyOverlapErrorCode,
            admission.ErrorCode);
        Assert.Contains("g3", admission.ErrorMessage);
    }

    // ── Обучаемой единице уходит только обучение ──────────────────────────────

    [Fact]
    public void OnlyTrainRows_AreHandedToTheLearner()
    {
        var material = Material();
        var actual = material.SourceRows.Select(row => ActualOf(row, row.Content)).ToArray();

        var trainingRows = CapabilityMaterialLineageGuard.SelectTrainingRows(material, actual);

        Assert.True(trainingRows.IsSuccess, trainingRows.ErrorMessage);
        // Три учебные строки и ни одной отложенной.
        Assert.Equal(3, trainingRows.Value!.Count);
        Assert.Equal(["s1", "s2", "s3"], trainingRows.Value.Select(row => row.ClaimedSourceRowId));
        // Экзаменационная строка обучаемой единице не достаётся ни под каким именем.
        Assert.DoesNotContain(trainingRows.Value, row => row.ClaimedSourceRowId == "s5");
    }

    // ── Оснастка ──────────────────────────────────────────────────────────────

    // ActualOf собирает фактическую строку с ЧЕСТНО посчитанными отпечатками заданного текста.
    private static CapabilityActualMaterialRow ActualOf(
        CapabilitySourceRow sourceRow,
        string content,
        string? claimedSourceRowId = null)
        => new(
            ExampleId: "example-" + (claimedSourceRowId ?? sourceRow.SourceRowId),
            ClaimedSourceRowId: claimedSourceRowId ?? sourceRow.SourceRowId,
            Role: sourceRow.Role,
            Content: content,
            PresentedContentSha256: Sha256(content),
            PresentedNormalizedSha256: Sha256(NormalizeLikeGuard(content)));

    // Sha256 считает отпечаток точных байтов текста так же, как это делает сторож.
    private static string Sha256(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    // NormalizeLikeGuard повторяет выбранное приведение текста: нижний регистр, знаки препинания
    // убраны, пробелы сжаты. Повторено здесь намеренно, чтобы проверка готовила вход сама и не
    // зависела от внутренностей сторожа.
    private static string NormalizeLikeGuard(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            if (char.IsPunctuation(character) || char.IsSymbol(character)) continue;
            builder.Append(char.IsWhiteSpace(character) ? ' ' : char.ToLowerInvariant(character));
        }

        return string.Join(' ', builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
