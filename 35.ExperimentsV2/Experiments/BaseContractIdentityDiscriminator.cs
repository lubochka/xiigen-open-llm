// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// Различитель договора базового типа: сравнение ЛИЧНОСТЕЙ вместо узнавания по подстроке.
//
// Назначение: перенос исправления опыта сто двадцать четыре (строка кандидата в общем списке
// необходимых изменений, docs/plans/replanning-possible-diffs/required-changes.md). Основание
// разрешения — явная логическая ошибка диагностики.
//
// ЧТО СЛОМАНО РОВНО, по двум сторонам сразу. Прежний различитель узнавал договор по БУКВАЛЬНОЙ
// подстроке имени базового класса: требуемое имя стояло в коде постоянной строкой и сравнивалось
// подстрокой с текстом договора, а фактическое — подстрокой с вольными текстовыми полями
// пакета. У подстроки две беды, и обе тихие:
//
// Первая — составное имя. Класс, унаследованный от `BaseServiceAdapter`, содержит подстроку
// `BaseService`, поэтому различитель отвечает «договор соблюдён» уверенно и НЕПРАВИЛЬНО: такого
// наследования нет.
//
// Вторая — переименование. Если договор требует `ProjectService`, буквальная постоянная
// `BaseService` не совпадёт никогда, и различитель отвечает «договор не соблюдён» или «потерь
// нет» — опять уверенно и неправильно, причём молча: громкой ошибки не возникает.
//
// ЧТО СТАЛО. Требуемое имя приходит из договора времени работы, фактическое — из списка
// объявленных базовых типов, и сравниваются ЛИЧНОСТИ: точное совпадение имени, а не вхождение
// подстроки. Когда одной из сторон нет, различитель честно отвечает «по личностям решить
// нельзя» и НЕ подменяет ответ догадкой.
//
// ОБУЧЕННАЯ ПАМЯТЬ — СИГНАЛ, А НЕ ВЛАСТЬ. Это требование обязательное, и вот его причина,
// измеренная опытом: плечо, обученное на неверной метке, воспроизвело неверный ответ с
// уверенностью 1,0000. Значит уверенность не может быть порогом доверия, а обученная память не
// может быть единственной властью над ответом. Здесь она входит в решение только как сигнал
// покрытия и уверенности: он записывается в вердикт, виден читателю и НИКОГДА не перебивает
// сравнение личностей. Когда личности сравнить нельзя, сигнал тоже не становится ответом —
// ответом становится честное «решить нельзя».
public static class BaseContractIdentityDiscriminator
{
    /// <summary>Договор соблюдён: требуемая личность найдена среди объявленных базовых типов.</summary>
    public const string SatisfiedDecision = "BASE_CONTRACT_SATISFIED";

    /// <summary>Договор не соблюдён: требуемой личности среди объявленных базовых типов нет.</summary>
    public const string MissingDecision = "BASE_CONTRACT_MISSING";

    /// <summary>По личностям решить нельзя: одной из сторон сравнения нет.</summary>
    public const string UndecidableDecision = "BASE_CONTRACT_UNDECIDABLE_BY_IDENTITY";

    /// <summary>Требуемая личность не названа договором времени работы.</summary>
    public const string RequirementMissingErrorCode = "BASE_CONTRACT_REQUIREMENT_NOT_DECLARED";

    /// <summary>Список объявленных базовых типов не наблюдался.</summary>
    public const string ObservationMissingErrorCode = "BASE_CONTRACT_BASE_TYPES_NOT_OBSERVED";

    // Decide решает по личностям и записывает сигнал обученной памяти рядом, не отдавая ему
    // власти над ответом.
    //
    // requiredBaseTypeIdentity — требуемое имя из договора времени работы.
    // observedBaseTypeIdentities — фактические объявленные базовые типы разборщика.
    // learnedSignal — сигнал обученной памяти: её собственный ответ, покрытие и уверенность.
    public static BaseContractVerdict Decide(
        string? requiredBaseTypeIdentity,
        IReadOnlyList<string>? observedBaseTypeIdentities,
        LearnedCoverageSignal? learnedSignal = null)
    {
        var signal = learnedSignal ?? LearnedCoverageSignal.Absent; // Отсутствие сигнала — тоже состояние.

        if (string.IsNullOrWhiteSpace(requiredBaseTypeIdentity))
            // Договор не назвал требуемую личность: решать по личностям нечем.
            return new BaseContractVerdict(
                false, UndecidableDecision, RequirementMissingErrorCode, signal, false,
                "договор времени работы не назвал требуемый базовый тип: сравнение личностей невозможно");

        if (observedBaseTypeIdentities is null || observedBaseTypeIdentities.Count == 0)
            // Списка объявленных базовых типов нет: сравнивать не с чем, и догадка ответом не становится.
            return new BaseContractVerdict(
                false, UndecidableDecision, ObservationMissingErrorCode, signal, false,
                "список объявленных базовых типов не наблюдался: по личностям решить нельзя, " +
                "сигнал обученной памяти ответом не становится");

        // Сравнение ЛИЧНОСТЕЙ: точное совпадение имени, а не вхождение подстроки. Именно здесь
        // умирают обе прежние беды — составное имя больше не совпадает, а переименование
        // перестаёт быть невидимым, потому что требуемое имя берётся из договора, а не из
        // постоянной строки в коде.
        var satisfied = observedBaseTypeIdentities.Any(observed =>
            string.Equals(Identity(observed), Identity(requiredBaseTypeIdentity), StringComparison.Ordinal));

        return new BaseContractVerdict(
            satisfied,                                   // Ответ решён личностями.
            satisfied ? SatisfiedDecision : MissingDecision, // Названное решение.
            string.Empty,                                // Кода отказа нет: решение состоялось.
            signal,                                      // Сигнал обученной памяти записан рядом.
            true,                                        // Решение принято сравнением личностей.
            satisfied
                ? "требуемый базовый тип найден среди объявленных по точному совпадению личности"
                : "требуемого базового типа среди объявленных нет: совпадения личности не найдено");
    }

    // Identity приводит имя базового типа к сравниваемой личности.
    //
    // Что входит в приведение и почему ровно это. Убираются краевые пробелы и отбрасывается
    // уточнение пространства имён: личность типа — это его имя, а не путь к нему. Общие
    // параметры отбрасываются вместе со скобками, потому что `BaseService<T>` и `BaseService` —
    // один и тот же базовый договор с разным уточнением. Регистр НЕ игнорируется: имена типов в
    // этом языке различаются регистром, и «baseservice» не является тем же типом.
    private static string Identity(string name)
    {
        var trimmed = (name ?? string.Empty).Trim();          // Краевые пробелы личности не меняют.
        var withoutGenerics = trimmed.Split('<')[0];          // Уточнение общими параметрами отбрасывается.
        var lastDot = withoutGenerics.LastIndexOf('.');       // Граница уточнения пространства имён.
        return lastDot >= 0 && lastDot + 1 < withoutGenerics.Length
            ? withoutGenerics[(lastDot + 1)..]                // Остаётся само имя типа.
            : withoutGenerics;                                // Уточнения не было — имя как есть.
    }
}

/// <summary>
/// Сигнал обученной памяти: её собственный ответ, покрытие и уверенность. Он входит в вердикт
/// как наблюдение и НЕ имеет власти над решением — ровно потому, что опыт измерил неверный ответ
/// с уверенностью 1,0000.
/// </summary>
public sealed record LearnedCoverageSignal(
    bool MemorySaysSatisfied,  // Что отвечает обученная память сама по себе.
    double Coverage,           // Доля покрытия, которую память заявляет.
    double Confidence)         // Уверенность памяти; порогом доверия она не является.
{
    /// <summary>Сигнала нет: память ничего не говорит, и это отдельное состояние.</summary>
    public static LearnedCoverageSignal Absent { get; } = new(false, 0.0, 0.0);
}

/// <summary>Вердикт различителя договора базового типа.</summary>
public sealed record BaseContractVerdict(
    bool Satisfied,                    // Соблюдён ли договор по личностям.
    string Decision,                   // Названное решение: соблюдён, не соблюдён, решить нельзя.
    string ErrorCode,                  // Код причины, когда решить нельзя; иначе пустая строка.
    LearnedCoverageSignal LearnedSignal, // Сигнал обученной памяти, записанный рядом с решением.
    bool DecidedByIdentity,            // Решено ли сравнением личностей, а не чем-то ещё.
    string Human);                     // Человеческое объяснение решения.
