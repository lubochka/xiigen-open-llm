// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Text.Json;
using TranslationDomain;

// Что здесь проверяется, по-человечески.
//
// Это починка поломки сборки, опубликованной в удалённой ветке: счётчик публикаций версии
// состояния объявлен ДЛИННЫМ целым, а три места читали его как обычное целое, и в C# длинное в
// обычное неявно не преобразуется. Проект не собирался.
//
// Почему это не косметика. Если бы преобразование дописали приведением, оно молча усекало бы
// номер после предела обычного целого: две разные публикации получили бы один номер, и условная
// публикация по номеру версии перестала бы их различать. Молчаливое усечение здесь хуже отказа
// сборки, поэтому читатели расширены до типа самой величины, а не приведены к обычному целому.
//
// Эти проверки закрепляют починку так, чтобы сужение типа обратно немедленно падало: тип
// открытого свойства версии и типы полей записи проигравшего кандидата проверяются напрямую, а
// значение выше предела обычного целого проходит настоящий продуктовый путь записи текста и
// читается обратно без потери.
//
// Отдельно закреплено, что уже записанные файлы читаются как прежде: прежнее небольшое значение
// пишется и читается тем же единственным правилом текста и остаётся тем же числом, а в самом
// тексте остаётся обычным числом без кавычек — то есть формат записи не менялся.
//
// Прогона не было, и это главная честная граница этой починки: сборка в этой среде отказывает по
// разрешениям, компилятором ничего не проверено, доказательство — разбор типов по каждому месту
// и эти написанные проверки. Зелёной сборкой я ничего не называю.
public sealed class StateVersionWideningTests
{
    // ── Случай 1: типы закреплены, сужение обратно упадёт ──────────────────────

    [Fact]
    public void StateVersionReaders_AreDeclaredAsLong_SoNarrowingBackWouldFailHere()
    {
        // Открытое свойство номера версии обязано отдавать длинное целое: оно читает счётчик
        // публикаций, объявленный длинным.
        var property = typeof(TranslationMachine).GetProperty(nameof(TranslationMachine.CurrentStateVersion));
        Assert.NotNull(property);
        Assert.Equal(typeof(long), property!.PropertyType);

        // Поле основы в записи кандидата — длинное целое.
        var baseField = typeof(TranslationLostCandidateArtifact)
            .GetProperty(nameof(TranslationLostCandidateArtifact.BaseStateVersionNumber));
        Assert.NotNull(baseField);
        Assert.Equal(typeof(long), baseField!.PropertyType);

        // Поле победившей версии — длинное целое с возможным пустым значением.
        var winningField = typeof(TranslationLostCandidateArtifact)
            .GetProperty(nameof(TranslationLostCandidateArtifact.WinningStateVersionNumber));
        Assert.NotNull(winningField);
        Assert.Equal(typeof(long?), winningField!.PropertyType);

        // Проверяемый указатель на запись несёт те же типы: иначе ссылка и запись разошлись бы.
        var refBase = typeof(TranslationLostCandidateArtifactRef)
            .GetProperty(nameof(TranslationLostCandidateArtifactRef.BaseStateVersionNumber));
        var refWinning = typeof(TranslationLostCandidateArtifactRef)
            .GetProperty(nameof(TranslationLostCandidateArtifactRef.WinningStateVersionNumber));
        Assert.Equal(typeof(long), refBase!.PropertyType);
        Assert.Equal(typeof(long?), refWinning!.PropertyType);
    }

    // ── Случай 2: номер выше предела обычного целого проходит без потери ───────

    [Fact]
    public void AVersionNumberAboveIntRange_SurvivesTheRealTextRoundTrip()
    {
        // Номер заведомо больше предела обычного целого: именно его прежний тип потерял бы.
        var hugeBase = (long)int.MaxValue + 7;
        var hugeWinning = (long)int.MaxValue + 9;

        var artifact = ArtifactWith(hugeBase, hugeWinning);

        // Запись и чтение идут ЕДИНСТВЕННЫМ продуктовым правилом текста, а не самодельным.
        var json = JsonSerializer.Serialize(artifact, TranslationCheckpointJsonOptions.Standard);
        var restored = JsonSerializer.Deserialize<TranslationLostCandidateArtifact>(
            json, TranslationCheckpointJsonOptions.Standard);

        Assert.NotNull(restored);
        Assert.Equal(hugeBase, restored!.BaseStateVersionNumber);
        Assert.NotNull(restored.WinningStateVersionNumber);
        // Сравнение идёт со значением, а не с возможным пустым: типы обеих сторон длинные.
        Assert.Equal(hugeWinning, restored.WinningStateVersionNumber!.Value);

        // И прямое доказательство, что потери нет именно из-за предела: при обычном целом эти
        // числа не представимы.
        Assert.True(hugeBase > int.MaxValue);
        Assert.True(hugeWinning > int.MaxValue);
    }

    // ── Случай 3: уже записанные значения читаются как прежде ─────────────────

    [Fact]
    public void AnAlreadyRecordedSmallVersion_ReadsExactlyAsBefore_AndStaysAPlainNumber()
    {
        // Небольшое значение — такое, какое лежит в уже записанных файлах.
        var artifact = ArtifactWith(2, 3);

        var json = JsonSerializer.Serialize(artifact, TranslationCheckpointJsonOptions.Standard);

        // Формат не менялся: номер остаётся обычным числом без кавычек. Если бы расширение типа
        // меняло текст, прежние файлы перестали бы читаться — здесь видно, что не меняет.
        Assert.Contains("\"base_state_version_number\": 2", json);
        Assert.Contains("\"winning_state_version_number\": 3", json);

        var restored = JsonSerializer.Deserialize<TranslationLostCandidateArtifact>(
            json, TranslationCheckpointJsonOptions.Standard);

        Assert.NotNull(restored);
        // Числа названы длинными прямо в проверке: сравнение обычного целого с длинным не
        // вывелось бы как один тип, и это тоже часть доказательства расширения.
        Assert.Equal(2L, restored!.BaseStateVersionNumber);
        Assert.NotNull(restored.WinningStateVersionNumber);
        Assert.Equal(3L, restored.WinningStateVersionNumber!.Value);

        // Пустая победившая версия — отдельный смысл, и он тоже читается как прежде.
        var cancelled = ArtifactWith(2, null);
        var cancelledJson = JsonSerializer.Serialize(cancelled, TranslationCheckpointJsonOptions.Standard);
        var cancelledBack = JsonSerializer.Deserialize<TranslationLostCandidateArtifact>(
            cancelledJson, TranslationCheckpointJsonOptions.Standard);
        Assert.Null(cancelledBack!.WinningStateVersionNumber);
    }

    // ── Оснастка ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Запись кандидата с настоящей контрольной точкой состояния: она берётся у настоящей машины
    /// её же выгрузкой, чтобы путь записи текста был продуктовым целиком.
    /// </summary>
    private static TranslationLostCandidateArtifact ArtifactWith(long baseVersion, long? winningVersion)
    {
        var machine = new TranslationMachine();            // Настоящая машина области перевода.
        var exported = machine.ExportCheckpoint();          // Её собственная выгрузка состояния.
        Assert.True(exported.IsSuccess, exported.ErrorMessage);

        return new TranslationLostCandidateArtifact(
            SchemaVersion: 1,                               // Версия формата записи.
            MachineId: TranslationMachine.MachineId,        // Машина, которой принадлежит кандидат.
            DomainTag: TranslationMachine.DomainTag,        // Предметная область машины.
            BaseStateVersionNumber: baseVersion,            // Проверяемый номер основы.
            BaseSemanticVersion: "1.0.0",                   // Смысловой номер основы.
            WinningStateVersionNumber: winningVersion,      // Проверяемый номер победителя.
            CandidateBatchId: "version-widening",           // Партия обучения кандидата.
            CandidateSemanticVersion: "1.0.1",              // Смысловой номер кандидата.
            CandidateStateIdentity: "identity",             // Опознание состояния кандидата.
            CandidateCheckpoint: exported.Value!,           // Фактическое состояние из выгрузки.
            LineageExampleIdentities: [],                   // Происхождение учебных строк.
            Cost: new TranslationLostCandidateCost(1, 2, 3, 4), // Уже оплаченный расход.
            RefusalErrorCode: "STATE_VERSION_CHANGED_DURING_TRAINING", // Код отказа публикации.
            RefusalReason: "проверка расширения типа номера версии",    // Человеческая причина.
            BuiltAtUtc: DateTimeOffset.UnixEpoch);          // Постоянное время сборки записи.
    }
}
