// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;

// Что здесь проверяется, по-человечески.
//
// Аудит иногда поправляет ТОЛКОВАНИЕ старого сохранённого результата: сырое число остаётся
// прежним, а вывод по нему оказывается слабее, чем звучал. Такую поправку нельзя применить
// «куда-нибудь»: применённая к другой попытке, она становится ложью о том опыте, которого никто
// не проверял.
//
// Что было до этой правки, по коду. Сами поправки существуют как сохранённый пакет аудита и уже
// несут опознание своего источника: хранилище, срез, путь, отпечаток точных байтов и указатель
// внутрь документа. А ЧИТАТЕЛЯ не существовало вовсе: во всём дереве слово «поправка
// утверждения» встречается только в файлах данных, ни в одном файле кода его нет, и сам пакет
// говорит это о себе прямо — у каждой из шести поправок стоит признак «читатель в работе не
// реализован», а правило чтения записано словами: «сверить отпечаток и путь источника и указатель
// внутрь документа; сырое значение оставить видимым; реализация читателя остаётся запланированной».
//
// Что стало. Читатель есть, и связь с источником у него неразделима по построению: опознание
// источника лежит ВНУТРИ поправки, создать поправку без полного опознания нельзя, и у читателя
// нет довода, который принимал бы толкование отдельно от источника. Отпечаток источника читатель
// считает заново из фактических байтов, а несовпадение среза называет конфликтом версии.
//
// О красном прогоне на прежней логике. Прежней логики не существует: читателя не было, поэтому
// запускать на нём нечего, и красным прогоном я это не называю. Честная граница такая: до этой
// правки поправку нельзя было применить ВООБЩЕ, а теперь её нельзя применить НЕ К СВОЕМУ
// источнику. Положительный контроль ниже доказывает, что законный путь при этом проходит.
//
// Чего это НЕ лечит, прямо. Это не подключение к построителю отчётов: такого построителя в дереве
// нет, и строка очереди сама называет его будущей работой. Поэтому поправка пока не доезжает до
// человеческого вывода сама — она только может быть прочитана правильно тем, кто её спросит.
// Исторические сырые данные этот код не меняет и обучения не начинает.
//
// Прогона не было: прямые команды сборки и прогона тестов запрещены политикой разрешений проекта,
// и обходить запрет оболочкой-посредником я не стал.
public sealed class ClaimCorrectionSourceBindingTests
{
    // Хранилище и срез взяты в той же форме, что в сохранённом пакете аудита.
    private const string Repository = "lubochka/llm_mvp_core";
    private const string SourceCommit = "e235f0971d6172f020822d42c61fe34a2819f9f8";
    private const string ForeignCommit = "0000000000000000000000000000000000000000";
    private const string SourcePath = "experiments-v2/RP01-MICRO/P10301/results/attempt-0001/metrics.json";
    private const string ForeignPath = "experiments-v2/RP01-MICRO/P10401/results/attempt-0001/metrics.json";

    // Указатель внутрь документа записан русскими именами частей — ровно как в пакете аудита.
    private const string Pointer = "/контрольная_точка_и_продолжение/продолжение_обучения/вывод";

    // Сырое значение источника и авторитетное толкование аудита: это РАЗНЫЕ вещи.
    private const string RawValueInSource = "обучение продолжено с контрольной точки";
    private const string AuthoritativeInterpretation = "продолжение обучения с контрольной точки НЕ ДОКАЗАНО этим прогоном";

    // ── Проверка 1: поправка чужого источника не применяется ──────────────────

    [Fact]
    public void CorrectionFromAnotherPath_IsRefusedAndNothingIsRead()
    {
        // Поправка составлена для одного файла, а предъявлен другой.
        var correction = HonestCorrection();
        var reading = ClaimCorrectionReader.Read(correction, ActualSource(path: ForeignPath));

        // Отказ приходит обычным конвертом с названным кодом.
        Assert.False(reading.IsSuccess);
        Assert.Equal(ClaimCorrectionReader.PathMismatchErrorCode, reading.ErrorCode);
        // Прочитанного наружу не выдано: применить поправку некуда.
        Assert.Null(reading.Value);
        // Причина называет оба пути, а не только код.
        Assert.Contains(SourcePath, reading.ErrorMessage);
        Assert.Contains(ForeignPath, reading.ErrorMessage);
    }

    [Fact]
    public void CorrectionFromAnotherCommit_IsRefusedAsAnExplicitVersionConflict()
    {
        // Тот же файл, но другая его версия. Правило аудита требует именно явного конфликта
        // версии, а не тихого применения поправки к другой попытке.
        var reading = ClaimCorrectionReader.Read(HonestCorrection(), ActualSource(commit: ForeignCommit));

        Assert.False(reading.IsSuccess);
        Assert.Equal(ClaimCorrectionReader.CommitConflictErrorCode, reading.ErrorCode);
        Assert.Null(reading.Value);
        // Причина называет конфликт версии словами, а не только кодом.
        Assert.Contains("конфликт версии", reading.ErrorMessage);
    }

    [Fact]
    public void CorrectionWhoseSourceContentChanged_IsRefusedByTheRecomputedDigest()
    {
        // Хранилище, путь и срез совпадают, но содержимое документа другое. Отпечаток считается
        // заново из фактических байтов, поэтому подмена видна.
        var changedContent = SourceDocument(RawValueInSource + " (изменено)");
        var reading = ClaimCorrectionReader.Read(HonestCorrection(), ActualSource(content: changedContent));

        Assert.False(reading.IsSuccess);
        Assert.Equal(ClaimCorrectionReader.ContentMismatchErrorCode, reading.ErrorCode);
        Assert.Null(reading.Value);
    }

    // ── Проверка 2: поправка без опознания источника до читателя не доходит ───

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CorrectionWithoutSourceDigest_IsNotEvenCreated(string emptyDigest)
    {
        // Опознание источника неполное: отпечатка нет.
        var created = ClaimCorrection.Create(
            "C01",
            new ClaimCorrectionSource(Repository, SourceCommit, SourcePath, emptyDigest, Pointer),
            "обучение продолжено",
            "NOT_PROVEN",
            AuthoritativeInterpretation,
            "аудит пересчитал вывод");

        // Поправка не создана вовсе, поэтому читателю её передать нечем.
        Assert.False(created.IsSuccess);
        Assert.Equal(ClaimCorrection.SourceIdentityRequiredErrorCode, created.ErrorCode);
        Assert.Null(created.Value);
    }

    [Fact]
    public void CorrectionWithoutPointer_IsNotEvenCreated()
    {
        // Указателя внутрь документа нет: поправлять нечего и некуда.
        var created = ClaimCorrection.Create(
            "C02",
            new ClaimCorrectionSource(Repository, SourceCommit, SourcePath, DigestOfHonestSource(), ""),
            "обучение продолжено",
            "NOT_PROVEN",
            AuthoritativeInterpretation,
            "аудит пересчитал вывод");

        Assert.False(created.IsSuccess);
        Assert.Equal(ClaimCorrection.SourceIdentityRequiredErrorCode, created.ErrorCode);
        Assert.Null(created.Value);
    }

    [Fact]
    public void PackageRowWithoutSourceIdentity_NeverBecomesACorrection()
    {
        // Строка пакета аудита без опознания источника: загрузчик проходит через ту же
        // единственную проверку, поэтому дыра не возвращается с другой стороны.
        var package = """
        {
          "schema": "post107-claim-corrections-v1",
          "corrections": [
            {
              "id": "C09",
              "repository": "lubochka/llm_mvp_core",
              "source_path": "experiments-v2/RP01-MICRO/P10301/results/attempt-0001/metrics.json",
              "json_pointer": "/вывод",
              "interpretation_status": "NOT_PROVEN",
              "authoritative_interpretation": "не доказано"
            }
          ]
        }
        """;

        var loaded = ClaimCorrectionLoader.Load(package);

        // Пакет отвергнут целиком: поправкой без источника эта строка не стала.
        Assert.False(loaded.IsSuccess);
        Assert.Equal(ClaimCorrection.SourceIdentityRequiredErrorCode, loaded.ErrorCode);
        Assert.Null(loaded.Value);
    }

    // ── Проверка 3: законный путь проходит, сырое значение остаётся видимым ───

    [Fact]
    public void HonestCorrectionOnItsOwnSource_IsReadWithRawValueAndInterpretationSideBySide()
    {
        // Положительный контроль: та же поправка на своём источнике.
        var reading = ClaimCorrectionReader.Read(HonestCorrection(), ActualSource());

        Assert.True(reading.IsSuccess, reading.ErrorMessage);
        var value = reading.Value!;
        // Сырое значение прочитано из самого документа по указателю поправки.
        Assert.Equal(RawValueInSource, value.RawValue);
        // Авторитетное толкование лежит РЯДОМ и отличается от сырого значения.
        Assert.Equal(AuthoritativeInterpretation, value.AuthoritativeInterpretation);
        Assert.NotEqual(value.RawValue, value.AuthoritativeInterpretation);
        // Состояние толкования названо словами.
        Assert.Equal("NOT_PROVEN", value.InterpretationStatus);
        // Прочитанное несёт опознание своего источника: видно, к чему оно относится.
        Assert.Equal(SourcePath, value.Source.SourcePath);
        Assert.Equal(SourceCommit, value.Source.SourceCommit);
    }

    [Fact]
    public void PointerMissingInTheSource_IsRefusedWithTheExactPointerNamed()
    {
        // Источник свой, но нужного места в документе нет: поправлять нечего.
        var documentWithoutPointer = """{"итог": {"вывод": "другое место"}}""";
        var created = ClaimCorrection.Create(
            "C03",
            new ClaimCorrectionSource(Repository, SourceCommit, SourcePath, Sha256(documentWithoutPointer), Pointer),
            "обучение продолжено",
            "NOT_PROVEN",
            AuthoritativeInterpretation,
            "аудит пересчитал вывод");
        Assert.True(created.IsSuccess, created.ErrorMessage);

        var reading = ClaimCorrectionReader.Read(created.Value!, ActualSource(content: documentWithoutPointer));

        Assert.False(reading.IsSuccess);
        Assert.Equal(ClaimCorrectionReader.PointerNotFoundErrorCode, reading.ErrorCode);
        // Причина называет сам указатель, чтобы человек увидел, чего не нашлось.
        Assert.Contains(Pointer, reading.ErrorMessage);
    }

    [Fact]
    public void WellFormedPackage_LoadsCorrectionsThatCarryTheirSource()
    {
        // Пакет в той же форме, что сохранённый аудит: шесть полей опознания у каждой строки.
        var digest = DigestOfHonestSource();
        var package = $$"""
        {
          "schema": "post107-claim-corrections-v1",
          "scope": "Interpretation overlays; never changes historical raw or starts training",
          "source_commit": "{{SourceCommit}}",
          "corrections": [
            {
              "id": "C01",
              "repository": "{{Repository}}",
              "source_commit": "{{SourceCommit}}",
              "source_path": "{{SourcePath}}",
              "source_sha256": "{{digest}}",
              "json_pointer": "{{Pointer}}",
              "original_claim": "обучение продолжено",
              "interpretation_status": "NOT_PROVEN",
              "authoritative_interpretation": "{{AuthoritativeInterpretation}}",
              "reason": "аудит пересчитал вывод"
            }
          ]
        }
        """;

        var loaded = ClaimCorrectionLoader.Load(package);

        Assert.True(loaded.IsSuccess, loaded.ErrorMessage);
        var correction = Assert.Single(loaded.Value!);
        // Загруженная поправка несёт своё опознание источника целиком.
        Assert.Equal(Repository, correction.Source.Repository);
        Assert.Equal(SourceCommit, correction.Source.SourceCommit);
        Assert.Equal(SourcePath, correction.Source.SourcePath);
        Assert.Equal(digest, correction.Source.SourceSha256);
        Assert.Equal(Pointer, correction.Source.JsonPointer);

        // И она читается на своём источнике, то есть загрузчик и читатель согласованы.
        var reading = ClaimCorrectionReader.Read(correction, ActualSource());
        Assert.True(reading.IsSuccess, reading.ErrorMessage);
        Assert.Equal(RawValueInSource, reading.Value!.RawValue);
    }

    // ── Оснастка ──────────────────────────────────────────────────────────────

    // Документ источника в той же форме, что сохранённые измерения опыта: русские имена частей.
    private static string SourceDocument(string conclusion)
        => $$"""
        {
          "контрольная_точка_и_продолжение": {
            "продолжение_обучения": {
              "вывод": "{{conclusion}}"
            }
          }
        }
        """;

    // Отпечаток честного документа источника, посчитанный проверкой самостоятельно.
    private static string DigestOfHonestSource()
        => Sha256(SourceDocument(RawValueInSource));

    // Честная поправка: её опознание источника соответствует честному документу.
    private static ClaimCorrection HonestCorrection()
    {
        var created = ClaimCorrection.Create(
            "C01",
            new ClaimCorrectionSource(Repository, SourceCommit, SourcePath, DigestOfHonestSource(), Pointer),
            "обучение продолжено",
            "NOT_PROVEN",
            AuthoritativeInterpretation,
            "аудит пересчитал вывод");
        Assert.True(created.IsSuccess, created.ErrorMessage);
        return created.Value!;
    }

    // Фактический источник: по умолчанию честный, но любую его часть можно подменить.
    private static ActualClaimSource ActualSource(
        string? repository = null,
        string? commit = null,
        string? path = null,
        string? content = null)
        => new(
            repository ?? Repository,
            commit ?? SourceCommit,
            path ?? SourcePath,
            content ?? SourceDocument(RawValueInSource));

    // Sha256 считает отпечаток точных байтов текста так же, как это делает читатель.
    private static string Sha256(string value)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
}
