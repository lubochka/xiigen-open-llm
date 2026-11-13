// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using DPO;

// Что здесь проверяется, по-человечески.
//
// Это продуктовый кусок по находке опыта сто двадцать один о допуске к изменению живых весов
// (запись 117 в едином реестре переносов,
// docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md).
//
// Что было сломано. На границе изменения весов не было объявленного владельца вовсе. Кто
// получатель — решал тот, кто передал модель: попросив обучить один проект и передав модель
// другого, вызывающий получал обновление весов другого проекта без единого возражения. Опыт
// измерил это прямо: правильный адрес получателя 0 из 1 до правки и 1 из 1 после, отпечаток
// весов в обоих мирах одинаковый, расхождение продуктового измерителя ровно ноль — отличался
// ТОЛЬКО владелец, и поймал это проверяющий по отпечаткам, а не продукт.
//
// Что стало. Владельца объявляет ТА СТОРОНА, которая выдаёт возможность изменения; отпечаток
// фактически изменяемого набора весов считает сам обучатель из привязок получателя и личности
// замороженного эталона; учёт пар «владелец ↔ набор весов» живёт внутри выданной возможности.
// Противоречие объявлений отказывается названным кодом через то же исключение допуска, что и
// прежде.
//
// О прежнем правиле границы. До этой правки граница принимала ЛЮБУЮ пару «владелец и набор
// весов», потому что владельца на ней не было вовсе. Это поведение САМО ПО СЕБЕ константа, и я
// говорю это прямо: входа ему не придумывается, воспроизведение записано постоянным ответом и
// сравнивается с наблюдённым ответом действующей границы. Выдумывать ему вход значило бы
// изображать зависимость, которой у прежнего правила не было.
//
// Чего эти проверки НЕ доказывают, и я этого не заявляю. Внутренний доступ к сборке обучения
// выдаётся по ИМЕНИ сборки, поэтому под разрешённым именем можно выдать себе возможность и
// объявить любого владельца. Первая проверка ниже воспроизводит именно этот обход и показывает,
// что он открыт. Сверка владельца ловит противоречие объявлений над одним набором весов; она
// НЕ делает имя сборки проверяемой личностью, и переименованием та дыра не чинится. Её закрытие
// — продуктовое решение владельца проекта, названное в записи 117 реестра.
//
// Прогона не было: прямые команды сборки и прогона тестов запрещены политикой разрешений этой
// среды, я их не запускал и зелёными не называю.
public sealed class DpoMutationOwnerIdentityTests
{
    /// <summary>Отпечаток набора весов первого проекта: строка вида «эталон|ключи привязок».</summary>
    private const string WeightsOfProjectAlpha = "ref-alpha-0001|blocks.0.lora_a|blocks.0.lora_b";

    /// <summary>Отпечаток набора весов второго проекта.</summary>
    private const string WeightsOfProjectBeta = "ref-beta-0002|blocks.0.lora_a|blocks.0.lora_b";

    /// <summary>Имя первого проекта, объявляемое выдающей стороной.</summary>
    private const string OwnerAlpha = "projectAlpha";

    /// <summary>Имя второго проекта.</summary>
    private const string OwnerBeta = "projectBeta";

    /// <summary>
    /// ПРЕЖНЕЕ правило границы: она принимала любую пару «владелец и набор весов», потому что
    /// объявленного владельца на ней не было вовсе. Это поведение само по себе КОНСТАНТА —
    /// именно тем оно и плохо, что ни от чего не зависело, — поэтому входа ему не придумывается
    /// и заявления «на том же входе» про него нет. Воспроизведено только для сравнения.
    /// </summary>
    private const bool PriorBoundaryAcceptedAnyOwnerOverAnyWeightsIsConstant = true;

    // ── Случай 1: действующий обход, воспроизведённый прямо ─────────────────────

    [Fact]
    public void AnyAssemblyWithAnAllowedName_CanStillIssueItselfTheCapability_AndThisTestIsTheProof()
    {
        // Эта сборка проверок НЕ является сервером моделей продукта и вообще не является ни
        // одной из трёх продуктовых частей. Тем не менее она сама выдаёт себе возможность
        // изменения весов и получает разрешение — ровно потому, что её ИМЯ входит в список
        // разрешённых имён сборок в файле проекта обучения предпочтению.
        var capability = DpoMutationAdmissionCapability.Issue(new SelfIssuedPermitSource());

        using var permit = capability.AcquireMutationPermit();
        Assert.NotNull(permit);

        // Это и есть доказательство обхода: допуск держится на имени, а не на личности. Так
        // делали три линии опытов подряд. Моя сверка владельца этого НЕ закрывает, и ниже
        // показано ровно то, что она закрывает.
        Assert.Null(capability.DeclaredOwnerId); // Владелец при такой выдаче даже не объявлен.
    }

    // ── Случай 2: два владельца над одним набором весов ─────────────────────────

    [Fact]
    public void TwoDeclaredOwnersOverOneWeightSet_AreRefusedByTheNamedCode()
    {
        // Прежнее правило приняло бы обе пары: оно принимало любую.
        Assert.True(PriorBoundaryAcceptedAnyOwnerOverAnyWeightsIsConstant);

        var ledger = new DpoMutationOwnerLedger();

        // Первое объявление закрепляет пару и проходит: сравнивать ещё не с чем.
        var first = ledger.RequireOwnerMatchesWeights(OwnerAlpha, WeightsOfProjectAlpha);
        Assert.True(first.Admitted, first.Human);

        // Второй владелец на ТЕХ ЖЕ весах — отказ своим кодом. Это измеренный дефект: урок,
        // объявленный для одного проекта, ложился на веса другого.
        var second = ledger.RequireOwnerMatchesWeights(OwnerBeta, WeightsOfProjectAlpha);
        Assert.False(second.Admitted);
        Assert.Equal(DpoMutationOwnerLedger.WeightsOwnerChangedErrorCode, second.ErrorCode);

        // Ответ действующей границы противоположен ответу прежнего правила на том же материале.
        Assert.NotEqual(PriorBoundaryAcceptedAnyOwnerOverAnyWeightsIsConstant, second.Admitted);
    }

    // ── Случай 3: один владелец над двумя разными наборами весов ────────────────

    [Fact]
    public void OneDeclaredOwnerOverTwoWeightSets_IsRefusedByTheNamedCode()
    {
        Assert.True(PriorBoundaryAcceptedAnyOwnerOverAnyWeightsIsConstant);

        var ledger = new DpoMutationOwnerLedger();
        Assert.True(ledger.RequireOwnerMatchesWeights(OwnerAlpha, WeightsOfProjectAlpha).Admitted);

        // Тот же владелец, другой набор весов — обратная половина той же лжи об адресе.
        var moved = ledger.RequireOwnerMatchesWeights(OwnerAlpha, WeightsOfProjectBeta);
        Assert.False(moved.Admitted);
        Assert.Equal(DpoMutationOwnerLedger.OwnerWeightsChangedErrorCode, moved.ErrorCode);
        Assert.NotEqual(PriorBoundaryAcceptedAnyOwnerOverAnyWeightsIsConstant, moved.Admitted);
    }

    // ── Случай 4: честный повтор проходит столько раз, сколько нужно ────────────

    [Fact]
    public void HonestRepeatOfTheSameOwnerOverTheSameWeights_IsAdmittedEveryTime()
    {
        var ledger = new DpoMutationOwnerLedger();

        // Положительный контроль: учёт не мешает обычному обучению одного проекта.
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var verdict = ledger.RequireOwnerMatchesWeights(OwnerAlpha, WeightsOfProjectAlpha);
            Assert.True(verdict.Admitted, verdict.Human);
            Assert.Equal(string.Empty, verdict.ErrorCode);
        }

        // И второй проект на СВОИХ весах тоже законен: отказ не огульный.
        var other = ledger.RequireOwnerMatchesWeights(OwnerBeta, WeightsOfProjectBeta);
        Assert.True(other.Admitted, other.Human);
    }

    // ── Случай 5: пустой отпечаток набора весов не проходит молча ───────────────

    [Fact]
    public void EmptyWeightsIdentity_IsRefusedInsteadOfSilentlyPassing()
    {
        var ledger = new DpoMutationOwnerLedger();

        var refused = ledger.RequireOwnerMatchesWeights(OwnerAlpha, "   ");
        Assert.False(refused.Admitted);
        Assert.Equal(DpoMutationOwnerLedger.EmptyWeightsIdentityErrorCode, refused.ErrorCode);
    }

    // ── Случай 6: граница возможности отказывает тем же исключением допуска ─────

    [Fact]
    public void TheCapabilityBoundaryRefusesWithTheAdmissionException_AndKeepsTheUndeclaredPath()
    {
        // Возможность, выданная С объявленным владельцем: сверка работает.
        var declared = DpoMutationAdmissionCapability.Issue(new SelfIssuedPermitSource(), OwnerAlpha);
        Assert.Equal(OwnerAlpha, declared.DeclaredOwnerId);

        // Первое обращение закрепляет набор весов этого владельца.
        declared.RequireOwnerMatchesWeights(WeightsOfProjectAlpha);

        // Другой набор весов под тем же объявленным владельцем — отказ тем же исключением
        // допуска, что и прежде, но с новой названной причиной.
        var error = Assert.Throws<DpoMutationAdmissionException>(
            () => declared.RequireOwnerMatchesWeights(WeightsOfProjectBeta));
        Assert.Equal(DpoMutationOwnerLedger.OwnerWeightsChangedErrorCode, error.ErrorCode);

        // Возможность, выданная БЕЗ объявленного владельца: прежний путь сохранён целиком, ни
        // один нынешний вызывающий от этой правки не ломается.
        var undeclared = DpoMutationAdmissionCapability.Issue(new SelfIssuedPermitSource());
        Assert.Null(undeclared.DeclaredOwnerId);
        undeclared.RequireOwnerMatchesWeights(WeightsOfProjectAlpha); // Отказа нет.
        undeclared.RequireOwnerMatchesWeights(WeightsOfProjectBeta);  // И на другом наборе тоже.
    }

    // ── Оснастка ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Источник разрешения, выдающий его самому себе. Он существует здесь не как образец
    /// правильной архитектуры, а как воспроизведение действующего обхода: сборка проверок может
    /// это сделать только потому, что её имя внесено в список разрешённых имён сборок.
    /// </summary>
    private sealed class SelfIssuedPermitSource : IDpoMutationAdmissionIssuer
    {
        /// <summary>Выдаёт разрешение-пустышку: для сверки владельца важен сам факт выдачи.</summary>
        public IDisposable AcquireDpoTrainerMutationPermit()
            => new CancellationTokenSource();
    }
}
