// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using ExperimentsV2;

// Что здесь проверяется, по-человечески.
//
// Это перенос исправления опыта сто двадцать четыре — договор различителя (строка кандидата в
// общем списке необходимых изменений, docs/plans/replanning-possible-diffs/required-changes.md).
// Основание разрешения: явная логическая ошибка диагностики.
//
// Что было неправильно, по двум сторонам сразу. Прежний различитель узнавал договор по
// БУКВАЛЬНОЙ подстроке имени базового класса: требуемое имя стояло постоянной строкой в коде и
// сравнивалось подстрокой с текстом договора, а фактическое — подстрокой с вольными текстовыми
// полями пакета. У подстроки две тихие беды. Составное имя: класс от `BaseServiceAdapter`
// содержит подстроку `BaseService`, и различитель отвечает «договор соблюдён» уверенно и
// НЕПРАВИЛЬНО. Переименование: если договор требует `ProjectService`, постоянная `BaseService`
// не совпадёт никогда, и различитель отвечает «не соблюдён» — опять уверенно и неправильно, и
// опять молча.
//
// Что стало. Требуемое имя приходит из договора времени работы, фактическое — из списка
// объявленных базовых типов, и сравниваются ЛИЧНОСТИ: точное совпадение имени. Когда одной из
// сторон нет, ответ — честное «по личностям решить нельзя», и догадка ответом не становится.
//
// Обученная память — сигнал, а не власть. Это требование обязательное, и его причина измерена:
// плечо, обученное на неверной метке, воспроизвело неверный ответ с уверенностью 1,0000. Ниже
// есть отдельный случай, где память говорит неверно с уверенностью 1,0000, а решение всё равно
// верное — именно потому, что память не единственная власть.
//
// Прежние правила воспроизведены функциями ОТ ВХОДА, и зависимость от входа показана вторым
// входом. Прогона не было: прямые команды сборки и прогона тестов запрещены политикой
// разрешений этой среды, я их не запускал и зелёными не называю.
public sealed class BaseContractIdentityDiscriminatorTests
{
    /// <summary>Требуемая личность базового типа в историческом договоре.</summary>
    private const string RequiredBaseService = "BaseService";

    /// <summary>Требуемая личность после переименования договора.</summary>
    private const string RequiredProjectService = "ProjectService";

    // ── Прежние правила, воспроизведённые для доказательства красноты ───────────

    /// <summary>
    /// ПРЕЖНЕЕ правило узнавания фактической стороны: вхождение подстроки требуемого имени в
    /// текст. Настоящая функция от входа: ответ считается из поданного текста и поданной
    /// подстроки. Воспроизведено здесь только для сравнения.
    /// </summary>
    private static bool PriorSubstringRecognition(string text, string requiredLiteral)
        => text.Contains(requiredLiteral, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// ПРЕЖНЕЕ правило требуемой стороны: буквальная постоянная `BaseService` сравнивалась
    /// подстрокой с текстом договора, поэтому переименованный договор не совпадал никогда.
    /// Настоящая функция от входа: ответ считается из поданного текста договора.
    /// Воспроизведено здесь только для сравнения.
    /// </summary>
    private static bool PriorHardcodedRequirementMatches(string contractText)
        => contractText.Contains("BaseService", StringComparison.OrdinalIgnoreCase);

    // ── Случай 1: составное имя базового типа ───────────────────────────────────

    [Fact]
    public void CompositeBaseTypeName_FooledThePriorSubstringRule_AndIsRefusedByIdentity()
    {
        // Класс унаследован от ДРУГОГО типа, в имени которого требуемое имя лишь содержится.
        var observed = new[] { "BaseServiceAdapter" };

        // Прежнее правило на том же входе отвечает НЕВЕРНО: подстрока входит, значит «да».
        Assert.True(PriorSubstringRecognition(observed[0], RequiredBaseService));

        // Действующее правило отвечает верно: совпадения ЛИЧНОСТИ нет.
        var verdict = BaseContractIdentityDiscriminator.Decide(RequiredBaseService, observed);
        Assert.False(verdict.Satisfied);
        Assert.Equal(BaseContractIdentityDiscriminator.MissingDecision, verdict.Decision);
        Assert.True(verdict.DecidedByIdentity);

        // Зависимость прежнего правила от входа: на имени без требуемой подстроки его ответ другой.
        Assert.False(PriorSubstringRecognition("ProjectService", RequiredBaseService));
    }

    // ── Случай 2: переименованный договор ──────────────────────────────────────

    [Fact]
    public void RenamedContract_FooledThePriorHardcodedRequirement_AndIsAnsweredByIdentity()
    {
        // Договор времени работы требует другое имя, и класс именно от него и унаследован.
        var observed = new[] { "ProjectService" };

        // Прежнее правило на том же входе отвечает НЕВЕРНО: буквальной постоянной здесь нет,
        // поэтому требование считалось несуществующим и потери — нулевыми.
        Assert.False(PriorHardcodedRequirementMatches("inherits ProjectService"));
        // Зависимость от входа: на прежнем имени договора его ответ другой.
        Assert.True(PriorHardcodedRequirementMatches("inherits BaseService"));

        // Действующее правило отвечает верно: требуемое имя взято из договора, личности совпали.
        var verdict = BaseContractIdentityDiscriminator.Decide(RequiredProjectService, observed);
        Assert.True(verdict.Satisfied);
        Assert.Equal(BaseContractIdentityDiscriminator.SatisfiedDecision, verdict.Decision);
        Assert.True(verdict.DecidedByIdentity);
    }

    // ── Случай 3: память отвечает неверно с полной уверенностью ─────────────────

    [Fact]
    public void LearnedMemoryWrongWithFullConfidence_DoesNotDecide_AndTheAnswerStaysCorrect()
    {
        // Память уверена и неправа: говорит «договор соблюдён» с уверенностью 1,0000, хотя
        // унаследован другой тип. Это ровно измеренный опытом случай.
        var wrongMemory = new LearnedCoverageSignal(MemorySaysSatisfied: true, Coverage: 1.0, Confidence: 1.0);
        var observed = new[] { "BaseServiceAdapter" };

        var verdict = BaseContractIdentityDiscriminator.Decide(RequiredBaseService, observed, wrongMemory);

        // Решение верное, несмотря на полную уверенность памяти: решали личности.
        Assert.False(verdict.Satisfied);
        Assert.True(verdict.DecidedByIdentity);
        // Сигнал памяти записан рядом целиком — он виден читателю, но ответом не стал.
        Assert.True(verdict.LearnedSignal.MemorySaysSatisfied);
        Assert.Equal(1.0, verdict.LearnedSignal.Confidence);
        // Ответ памяти и ответ решения прямо противоположны, и это зафиксировано.
        Assert.NotEqual(verdict.LearnedSignal.MemorySaysSatisfied, verdict.Satisfied);

        // Обратная половина того же правила: память уверенно отрицает, а личности совпадают —
        // решение всё равно по личностям.
        var denyingMemory = new LearnedCoverageSignal(false, 1.0, 1.0);
        var satisfied = BaseContractIdentityDiscriminator.Decide(
            RequiredBaseService, new[] { "BaseService" }, denyingMemory);
        Assert.True(satisfied.Satisfied);
        Assert.NotEqual(satisfied.LearnedSignal.MemorySaysSatisfied, satisfied.Satisfied);
    }

    // ── Случай 4: решить по личностям нельзя — честный отказ, а не догадка ──────

    [Fact]
    public void WithoutObservedBaseTypes_TheAnswerIsAnHonestRefusal_NotTheMemoryGuess()
    {
        // Список объявленных базовых типов не наблюдался, а память уверенно говорит «соблюдён».
        var confidentMemory = new LearnedCoverageSignal(true, 1.0, 1.0);

        var verdict = BaseContractIdentityDiscriminator.Decide(RequiredBaseService, null, confidentMemory);

        // Ответ — честное «решить нельзя», и догадка памяти ответом не стала.
        Assert.Equal(BaseContractIdentityDiscriminator.UndecidableDecision, verdict.Decision);
        Assert.Equal(BaseContractIdentityDiscriminator.ObservationMissingErrorCode, verdict.ErrorCode);
        Assert.False(verdict.DecidedByIdentity);
        Assert.False(verdict.Satisfied);
        // Но сигнал сохранён: читатель видит, что память говорила и с какой уверенностью.
        Assert.True(verdict.LearnedSignal.MemorySaysSatisfied);

        // И когда договор не назвал требуемое имя — тоже честный отказ со своим кодом.
        var noRequirement = BaseContractIdentityDiscriminator.Decide("   ", new[] { "BaseService" });
        Assert.Equal(BaseContractIdentityDiscriminator.UndecidableDecision, noRequirement.Decision);
        Assert.Equal(BaseContractIdentityDiscriminator.RequirementMissingErrorCode, noRequirement.ErrorCode);
    }

    // ── Случай 5: приведение личности названо и проверено ──────────────────────

    [Fact]
    public void IdentityNormalization_IgnoresNamespaceAndGenerics_ButNotCase()
    {
        // Уточнение пространства имён личность не меняет: имя типа то же.
        Assert.True(BaseContractIdentityDiscriminator
            .Decide(RequiredBaseService, new[] { "AgentInfrastructure.BaseService" }).Satisfied);

        // Уточнение общими параметрами тоже: `BaseService<T>` — тот же базовый договор.
        Assert.True(BaseContractIdentityDiscriminator
            .Decide(RequiredBaseService, new[] { "BaseService<TDomain>" }).Satisfied);

        // А регистр личность МЕНЯЕТ: имена типов в этом языке различаются регистром, и
        // «baseservice» не является тем же типом.
        Assert.False(BaseContractIdentityDiscriminator
            .Decide(RequiredBaseService, new[] { "baseservice" }).Satisfied);

        // Прежнее правило подстроки здесь отвечало бы «да» на всех трёх входах, включая
        // последний: оно сравнивало без учёта регистра и без границ имени.
        Assert.True(PriorSubstringRecognition("baseservice", RequiredBaseService));
    }

    // ── Случай 6: положительный контроль обычного совпадения ──────────────────

    [Fact]
    public void PlainMatchingBaseType_IsSatisfiedByIdentity_AndThePriorRuleAgreedHere()
    {
        var observed = new[] { "SomeOther", "BaseService" };

        // Положительный контроль: на честном входе прежнее и действующее правило согласны, и
        // правка этот случай не ломает.
        Assert.True(PriorSubstringRecognition(observed[1], RequiredBaseService));

        var verdict = BaseContractIdentityDiscriminator.Decide(RequiredBaseService, observed);
        Assert.True(verdict.Satisfied);
        Assert.True(verdict.DecidedByIdentity);
        Assert.Equal(string.Empty, verdict.ErrorCode);
    }
}
