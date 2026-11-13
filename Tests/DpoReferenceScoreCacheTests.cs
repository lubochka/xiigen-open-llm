// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using System.Globalization;
using ContinuousLearning;
using DPO;
using DPO.Models;
using LoRA;
using Tokenizer;
using TrainingLoop.Optimizers;
using TransformerModel;
using TransformerModel.Config;
using Xunit.Abstractions;

// FIX-LC transfer, plan 13-dpo-reference-cache.md, from LC-REF-CACHE.
//
// The reference model in preference training is frozen. Asked about the same
// pair twice it must return the same two numbers, because nothing about it has
// changed. The trainer nevertheless ran a full forward pass through it every
// time. Remembering the last two answers removes those passes.
//
// The thing that has to be true, and is checked here: the remembered answer
// produces exactly the same step as recomputing it. A cache that changed the
// result would be a silent corruption of training, not an optimisation.
//
// Tiny engineering fixtures only: no published training set, no checkpoint, no
// learning claim.
public sealed class DpoReferenceScoreCacheTests
{
    private static readonly ModelConfig Config = new()
    {
        VocabSize = 256, ContextLen = 8, HiddenDim = 8,
        NumLayers = 1, NumHeads = 2, NumKvHeads = 1, FfnHiddenDim = 16
    };

    [Fact]
    public void RememberedReferenceScoresProduceTheSameStepAsRecomputingThem()
    {
        using var roots = new TempRoots();
        var withoutCache = Trainer(Fixture(), roots.Next("plain"), capacity: 0);
        var withCache = Trainer(Fixture(), roots.Next("cached"), capacity: 2);

        // Step one teaches from two pairs; step two repeats the same two pairs.
        var plainStepOne = withoutCache.Trainer.StepVector(Batches(withoutCache.Fixture.Tokenizer), 1);
        var cachedStepOne = withCache.Trainer.StepVector(Batches(withCache.Fixture.Tokenizer), 1);
        var plainStepTwo = withoutCache.Trainer.StepVector(Batches(withoutCache.Fixture.Tokenizer), 2);
        var cachedStepTwo = withCache.Trainer.StepVector(Batches(withCache.Fixture.Tokenizer), 2);

        // The whole point: identical numbers on both steps.
        AssertSameMetrics(plainStepOne, cachedStepOne);
        AssertSameMetrics(plainStepTwo, cachedStepTwo);
    }

    [Fact]
    public void TheSecondStepStopsAskingTheFrozenReferenceAgain()
    {
        using var roots = new TempRoots();
        var plain = Trainer(Fixture(), roots.Next("plain"), capacity: 0);
        var cached = Trainer(Fixture(), roots.Next("cached"), capacity: 2);

        var plainFirst = new DpoStepVectorTiming();
        var cachedFirst = new DpoStepVectorTiming();
        plain.Trainer.StepVector(Batches(plain.Fixture.Tokenizer), 1, plainFirst);
        cached.Trainer.StepVector(Batches(cached.Fixture.Tokenizer), 1, cachedFirst);

        var plainSecond = new DpoStepVectorTiming();
        var cachedSecond = new DpoStepVectorTiming();
        plain.Trainer.StepVector(Batches(plain.Fixture.Tokenizer), 2, plainSecond);
        cached.Trainer.StepVector(Batches(cached.Fixture.Tokenizer), 2, cachedSecond);

        // Two pairs, two sides each: four reference passes on the first step in
        // both arms, because nothing is remembered yet.
        Assert.Equal(4L, plainFirst.Snapshot().ReferenceForward.CompletedCalls);
        Assert.Equal(4L, cachedFirst.Snapshot().ReferenceForward.CompletedCalls);

        // On the second step the arm without memory asks all four times again;
        // the arm with memory asks none. That difference is the whole saving.
        Assert.Equal(4L, plainSecond.Snapshot().ReferenceForward.CompletedCalls);
        Assert.Equal(0L, cachedSecond.Snapshot().ReferenceForward.CompletedCalls);

        // The trainable side is recomputed every step in both arms: the policy
        // changes, so its scores cannot be remembered.
        Assert.Equal(4L, plainSecond.Snapshot().PolicyForward.CompletedCalls);
        Assert.Equal(4L, cachedSecond.Snapshot().PolicyForward.CompletedCalls);

        var counters = cached.Trainer.ReferenceScoreCacheCounters;
        Assert.Equal(2, counters.Capacity);
        Assert.Equal(2, counters.Hits);
        Assert.Equal(2, counters.Misses);
        Assert.Equal(2, counters.Stores);
        Assert.Equal(0, plain.Trainer.ReferenceScoreCacheCounters.Capacity);
        Assert.Equal(0, plain.Trainer.ReferenceScoreCacheCounters.Lookups);
    }

    [Fact]
    public void TimingPhasesStillCountOnlyRealCalls()
    {
        using var roots = new TempRoots();
        var cached = Trainer(Fixture(), roots.Next("cached"), capacity: 2);

        var first = new DpoStepVectorTiming();
        cached.Trainer.StepVector(Batches(cached.Fixture.Tokenizer), 1, first);
        var second = new DpoStepVectorTiming();
        cached.Trainer.StepVector(Batches(cached.Fixture.Tokenizer), 2, second);

        // A remembered answer is not a model call, so it must not open the
        // reference phase at all — started and completed stay equal and no
        // phase is left half-open.
        foreach (var timing in new[] { first, second })
        {
            var phases = timing.Snapshot();
            Assert.Equal(phases.ReferenceForward.StartedCalls, phases.ReferenceForward.CompletedCalls);
            Assert.Equal(phases.PolicyForward.StartedCalls, phases.PolicyForward.CompletedCalls);
            Assert.Equal(0L, phases.ReferenceForward.IncompleteCalls);
        }

        Assert.Equal(0L, second.Snapshot().ReferenceForward.StartedCalls);
    }

    [Fact]
    public void ADifferentPairIsADifferentQuestion()
    {
        using var roots = new TempRoots();
        var cached = Trainer(Fixture(), roots.Next("cached"), capacity: 2);

        cached.Trainer.StepVector(Batches(cached.Fixture.Tokenizer), 1);
        var other = new DpoStepVectorTiming();
        cached.Trainer.StepVector(OtherBatches(cached.Fixture.Tokenizer), 2, other);

        // Unseen pairs must go to the reference, all four sides of them.
        Assert.Equal(4L, other.Snapshot().ReferenceForward.CompletedCalls);
        var counters = cached.Trainer.ReferenceScoreCacheCounters;
        Assert.Equal(0, counters.Hits);
        Assert.Equal(4, counters.Misses);
    }

    [Fact]
    public void CapacityHoldsExactlyTwoPairsAndEvictsTheOldest()
    {
        using var roots = new TempRoots();
        var cached = Trainer(Fixture(), roots.Next("cached"), capacity: 2);

        // Two pairs fill the memory, two more push them out, and the first two
        // then have to be asked again.
        cached.Trainer.StepVector(Batches(cached.Fixture.Tokenizer), 1);
        cached.Trainer.StepVector(OtherBatches(cached.Fixture.Tokenizer), 2);
        var back = new DpoStepVectorTiming();
        cached.Trainer.StepVector(Batches(cached.Fixture.Tokenizer), 3, back);

        Assert.Equal(4L, back.Snapshot().ReferenceForward.CompletedCalls);
        Assert.Equal(0, cached.Trainer.ReferenceScoreCacheCounters.Hits);
    }

    [Fact]
    public void CapacityOutsideTheMeasuredChoiceIsRefused()
    {
        using var roots = new TempRoots();
        var fixture = Fixture();
        // Only the two measured values are allowed; anything else is a guess,
        // and a guess about capacity is a separate decision after a measurement.
        Assert.Throws<ArgumentOutOfRangeException>(() => Trainer(fixture, roots.Next("bad"), capacity: 5));
    }

    [Fact]
    public void TheWholeStateAfterTwoStepsIsTheSameWithAndWithoutTheCache()
    {
        // Repair R4 of the 27.10 transfer audit, LC row. Every other case here
        // compares the four numbers a step reports. Equal numbers are not equal
        // state: a cache hit that returned before the optimizer ran would leave
        // the loss exactly as it is and simply skip the last update. What is
        // compared below is the state itself — the trained weights, the frozen
        // reference, the optimizer moments with its step counter, and the
        // random state.
        using var roots = new TempRoots();
        var plain = Trainer(Fixture(), roots.Next("plain"), capacity: 0);
        var cached = Trainer(Fixture(), roots.Next("cached"), capacity: 2);

        var referenceAtStart = Values(plain.Fixture.Reference);
        var policyAtStart = Values(plain.Fixture.Policy);

        plain.Trainer.StepVector(Batches(plain.Fixture.Tokenizer), 1);
        cached.Trainer.StepVector(Batches(cached.Fixture.Tokenizer), 1);
        plain.Trainer.StepVector(Batches(plain.Fixture.Tokenizer), 2);
        cached.Trainer.StepVector(Batches(cached.Fixture.Tokenizer), 2);

        // Training really moved the policy; otherwise everything below would be
        // trivially equal and would prove nothing.
        var plainPolicy = Values(plain.Fixture.Policy);
        Assert.NotEqual(policyAtStart, plainPolicy);

        // The trained weights agree value for value.
        Assert.Equal(plainPolicy, Values(cached.Fixture.Policy));

        // The frozen side is frozen in both arms, not merely equal to each other.
        Assert.Equal(referenceAtStart, Values(plain.Fixture.Reference));
        Assert.Equal(referenceAtStart, Values(cached.Fixture.Reference));

        // The optimizer carries the moments and the step counter. Pinning the
        // counter at two is what catches a skipped last update.
        var plainOptimizer = plain.Trainer.ExportOptimizerState();
        var cachedOptimizer = cached.Trainer.ExportOptimizerState();
        Assert.Equal(2, plainOptimizer.Step);
        Assert.Equal(2, cachedOptimizer.Step);
        Assert.Equal(plainOptimizer.ToBytes(), cachedOptimizer.ToBytes());

        // And the random state, which is part of the mechanism rather than a
        // by-product: a different draw would mean a different next step.
        //
        // Сравниваются поля снимка, а не его текстовая запись. Текстовая запись
        // печатает у списка записей только имя его типа, поэтому прежнее
        // сравнение через ToString не видело ни позиции потока, ни зёрен, ни
        // самого набора ключей — и пропускало любую подмену внутри записей.
        // Договор сравнения один на весь проект проверок: RandomStateFieldAssertions.
        RandomStateFieldAssertions.AssertEqual(
            plain.Fixture.Policy.CaptureRandomState(plain.Topology, plain.Identity, 2),
            cached.Fixture.Policy.CaptureRandomState(cached.Topology, cached.Identity, 2));
    }

    // Перенос раздела 4 адресного плана исправления переносов от 29 октября (файл 95).
    //
    // Откат уже сбрасывал память ответов эталона: после восстановления продолжения
    // это уже не тот же ход обучения, и прежние ответы ему не принадлежат. Успешная
    // ветвь того же восстановления этого не делала и возвращала успех с прогретой
    // памятью. Следующий шаг тогда брал ответы, посчитанные для другого состояния.
    //
    // Проверяется фазовая граница: тот же владелец прогревается, принимает свой
    // разрешённый снимок, и следующий шаг обязан снова спросить эталон.
    [Fact]
    public void AcceptedContinuationRestore_OnWarmedSameOwner_RequiresReferenceMiss()
    {
        using var roots = new TempRoots();
        var fixture = Fixture();
        var recipient = DpoProjectLoraRecipient.Capture(fixture.Policy, fixture.Reference, fixture.Adapters, fixture.Lora);
        var topology = LoraRandomStateIdentity.ComputeTopologySha256(Config, fixture.Lora, fixture.Seeds);
        var identity = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(fixture.Lora, fixture.Seeds);
        var profile = DpoExecutionProfile.Capture(Batches(fixture.Tokenizer)[0].Batch, recipient.Bindings);

        // Снимок и доказательство делаются до прогрева: это действительно допустимое
        // сохранённое состояние того же получателя, а не пустая заглушка.
        var savedRandom = fixture.Policy.CaptureRandomState(topology, identity, 1);
        var savedOptimizer = SavedOptimizerSnapshot(recipient);
        var proof = DpoContinuationProof.IssueVerified(
            profile, savedOptimizer, savedRandom, "kdp-lc-accepted-restore", () => true);

        // Владелец один на всю проверку: подменять его новым экземпляром нельзя,
        // потому что у нового память ответов и так пуста и ничего бы не доказала.
        var owner = DpoTrainer.CreateForProjectLora(
            fixture.Policy, fixture.Reference, roots.Next("warmed"),
            DpoMutationAdmissionCapability.Issue(new TestLease()),
            recipient, profile, proof, null,
            step => fixture.Policy.CaptureRandomState(topology, identity, step),
            state => fixture.Policy.RestoreRandomState(state, new(topology, identity)),
            2);

        // Прогрев: первый шаг спрашивает эталон о двух парах, второй берёт их из памяти.
        var warmUp = new DpoStepVectorTiming();
        owner.StepVector(Batches(fixture.Tokenizer), 1, warmUp);
        var warmHit = new DpoStepVectorTiming();
        owner.StepVector(Batches(fixture.Tokenizer), 2, warmHit);

        Assert.Equal(4L, warmUp.Snapshot().ReferenceForward.CompletedCalls);
        Assert.Equal(0L, warmHit.Snapshot().ReferenceForward.CompletedCalls);
        var warmed = owner.ReferenceScoreCacheCounters;
        Assert.Equal(2, warmed.Hits);

        // Состояние до восстановления заведомо другое, иначе «принятое восстановление»
        // ничего бы не меняло и проверка была бы пустой.
        var beforeRestore = owner.ExportOptimizerState();
        Assert.Equal(2, beforeRestore.Step);
        Assert.True(beforeRestore.Entries.Count > savedOptimizer.Entries.Count);

        // Разрешённое восстановление того же владельца принимается.
        Assert.True(owner.TryRestoreContinuationState(savedRandom, savedOptimizer));

        // Оптимизатор сверяется по полям: шаг, набор связок и оба момента.
        var afterRestore = owner.ExportOptimizerState();
        Assert.Equal(savedOptimizer.Step, afterRestore.Step);
        Assert.Equal(
            savedOptimizer.Entries.Select(entry => entry.Key),
            afterRestore.Entries.Select(entry => entry.Key));
        foreach (var expected in savedOptimizer.Entries)
        {
            var actual = afterRestore.Entries.Single(entry => entry.Key == expected.Key);
            Assert.Equal(expected.Dimensions, actual.Dimensions);
            Assert.Equal(expected.FirstMoment, actual.FirstMoment);
            Assert.Equal(expected.SecondMoment, actual.SecondMoment);
        }

        // Состояние генератора случайных чисел сверяется по полям записей, а не
        // через ToString: строка скрыла бы расхождение одного курсора. Договор
        // сравнения здесь тот же самый общий, что и выше по файлу, — второго
        // несовместимого договора в проекте проверок быть не должно.
        var randomAfterRestore = fixture.Policy.CaptureRandomState(topology, identity, savedRandom.RandomStateStep);
        RandomStateFieldAssertions.AssertEqual(savedRandom, randomAfterRestore);

        // Замороженный эталон остался замороженным: восстановление его не трогало.
        Assert.All(fixture.Reference.Parameters(), parameter => Assert.False(parameter.Value.RequiresGrad));

        // Третий шаг тех же двух пар обязан снова спросить эталон: прежние ответы
        // принадлежали другому состоянию и больше не действуют.
        var afterRestoreStep = new DpoStepVectorTiming();
        owner.StepVector(Batches(fixture.Tokenizer), 3, afterRestoreStep);
        Assert.Equal(4L, afterRestoreStep.Snapshot().ReferenceForward.CompletedCalls);

        // Счётчики сравниваются дельтами: сами они накапливаются за всю жизнь владельца.
        var afterThird = owner.ReferenceScoreCacheCounters;
        Assert.Equal(2, afterThird.Misses - warmed.Misses);
        Assert.Equal(0, afterThird.Hits - warmed.Hits);
        Assert.Equal(2, afterThird.Stores - warmed.Stores);

        // Доза ровно та, что назначена разделом 4: три шага, шесть предъявлений пар,
        // восемь обращений к эталону, двенадцать к обучаемой модели и столько же
        // обратных проходов, три обновления оптимизатора.
        var phases = new[] { warmUp.Snapshot(), warmHit.Snapshot(), afterRestoreStep.Snapshot() };
        Assert.Equal(6L, phases.Sum(phase => phase.VectorBatchesCompleted));
        Assert.Equal(8L, phases.Sum(phase => phase.ReferenceForward.CompletedCalls));
        Assert.Equal(12L, phases.Sum(phase => phase.PolicyForward.CompletedCalls));
        Assert.Equal(12L, phases.Sum(phase => phase.Backward.CompletedCalls));
        Assert.Equal(3L, phases.Sum(phase => phase.ApplyOptimizer.CompletedCalls));
    }

    // Вторая из трёх фаз того же раздела 4 уже закрыта проверкой выше. Здесь
    // закрывается ПЕРВАЯ: отказ допуска происходит ДО любой мутации, и прогретая
    // память ответов эталона при таком отказе остаётся неприкосновенной.
    //
    // Почему это важно по-человечески. Если бы очистка стояла раньше проверок,
    // любой отклонённый запрос на продолжение молча обесценивал бы законную
    // память владельца: ответы эталона пришлось бы считать заново, хотя ни одно
    // состояние не изменилось. Отклонённый вызов не имеет права ничего стоить.
    //
    // Отказ берётся ровно тем отрицательным примером, который назван в плане:
    // всё прежнее, изменён только один курсор потока. Это копия уже снятого
    // снимка, а не новая учебная траектория.
    //
    // Доза: три шага, шесть предъявлений пар, четыре обращения к эталону,
    // двенадцать к обучаемой модели, двенадцать обратных проходов, три
    // обновления оптимизатора. Сам отклонённый вызов не добавляет ни одного
    // прохода и ни одного обновления.
    [Fact]
    public void RejectedContinuationRestore_OnWarmedSameOwner_LeavesTheRememberedAnswersUntouched()
    {
        using var roots = new TempRoots();
        var fixture = Fixture();
        var recipient = DpoProjectLoraRecipient.Capture(fixture.Policy, fixture.Reference, fixture.Adapters, fixture.Lora);
        var topology = LoraRandomStateIdentity.ComputeTopologySha256(Config, fixture.Lora, fixture.Seeds);
        var identity = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(fixture.Lora, fixture.Seeds);
        var profile = DpoExecutionProfile.Capture(Batches(fixture.Tokenizer)[0].Batch, recipient.Bindings);

        var savedRandom = fixture.Policy.CaptureRandomState(topology, identity, 1);
        var savedOptimizer = SavedOptimizerSnapshot(recipient);
        var proof = DpoContinuationProof.IssueVerified(
            profile, savedOptimizer, savedRandom, "kdp-lc-rejected-restore", () => true);

        var owner = DpoTrainer.CreateForProjectLora(
            fixture.Policy, fixture.Reference, roots.Next("warmed-rejected"),
            DpoMutationAdmissionCapability.Issue(new TestLease()),
            recipient, profile, proof, null,
            step => fixture.Policy.CaptureRandomState(topology, identity, step),
            state => fixture.Policy.RestoreRandomState(state, new(topology, identity)),
            2);

        // Прогрев: первый шаг спрашивает эталон о двух парах, второй берёт их из памяти.
        var warmUp = new DpoStepVectorTiming();
        owner.StepVector(Batches(fixture.Tokenizer), 1, warmUp);
        var warmHit = new DpoStepVectorTiming();
        owner.StepVector(Batches(fixture.Tokenizer), 2, warmHit);
        Assert.Equal(4L, warmUp.Snapshot().ReferenceForward.CompletedCalls);
        Assert.Equal(0L, warmHit.Snapshot().ReferenceForward.CompletedCalls);
        var warmed = owner.ReferenceScoreCacheCounters;
        Assert.Equal(2, warmed.Hits);

        // Копия сохранённого снимка с одним изменённым курсором потока.
        var rejectedCopy = savedRandom with
        {
            Entries = savedRandom.Entries
                .Select((entry, index) => index == 0
                    ? entry with { ConsumedCursor = entry.ConsumedCursor + 1 }
                    : entry)
                .ToList()
        };

        // Та же подмена, которую обязан назвать общий сравнитель состояния: это
        // связывает отрицательный пример отказа с типизированным сравнением, а
        // не оставляет их двумя несвязанными утверждениями.
        Assert.Contains(
            RandomStateFieldAssertions.Differences(savedRandom, rejectedCopy),
            difference => difference.Contains("израсходованная позиция потока", StringComparison.Ordinal));

        // Состояние до отклонённого вызова: его нельзя тронуть.
        var optimizerBeforeRejection = owner.ExportOptimizerState().ToBytes();
        var policyBeforeRejection = Values(fixture.Policy);

        // Допуск обязан отказать именно отказом договора продолжения.
        Assert.Throws<DpoMutationAdmissionException>(
            () => owner.TryRestoreContinuationState(rejectedCopy, savedOptimizer));

        // Отказ случился до мутации: ни веса, ни оптимизатор не двигались.
        Assert.Equal(optimizerBeforeRejection, owner.ExportOptimizerState().ToBytes());
        Assert.Equal(policyBeforeRejection, Values(fixture.Policy));
        Assert.All(fixture.Reference.Parameters(), parameter => Assert.False(parameter.Value.RequiresGrad));

        // Главное: прогретая память жива. Третий шаг тех же двух пар обязан снова
        // попасть в память и не спросить эталон ни разу.
        var afterRejection = new DpoStepVectorTiming();
        owner.StepVector(Batches(fixture.Tokenizer), 3, afterRejection);
        Assert.Equal(0L, afterRejection.Snapshot().ReferenceForward.CompletedCalls);

        // Счётчики сравниваются дельтами: два попадания и ни одного промаха.
        var afterThird = owner.ReferenceScoreCacheCounters;
        Assert.Equal(2, afterThird.Hits - warmed.Hits);
        Assert.Equal(0, afterThird.Misses - warmed.Misses);
        Assert.Equal(0, afterThird.Stores - warmed.Stores);

        // Назначенная доза, посчитанная по фактическим фазам.
        var phases = new[] { warmUp.Snapshot(), warmHit.Snapshot(), afterRejection.Snapshot() };
        Assert.Equal(6L, phases.Sum(phase => phase.VectorBatchesCompleted));
        Assert.Equal(4L, phases.Sum(phase => phase.ReferenceForward.CompletedCalls));
        Assert.Equal(12L, phases.Sum(phase => phase.PolicyForward.CompletedCalls));
        Assert.Equal(12L, phases.Sum(phase => phase.Backward.CompletedCalls));
        Assert.Equal(3L, phases.Sum(phase => phase.ApplyOptimizer.CompletedCalls));
    }

    // И ТРЕТЬЯ фаза того же раздела 4: сбой уже НАЧАТОГО восстановления. Здесь
    // граница допуска уже пройдена, состояние тронуто, поэтому прежний откат
    // обязан и вернуть состояние назад, и забыть ответы эталона.
    //
    // Почему это важно по-человечески. После начала восстановления никто не
    // знает, чьему состоянию принадлежат прежние ответы, поэтому держать их
    // нельзя: следующий шаг взял бы числа, посчитанные для другого состояния, и
    // это была бы тихая порча обучения, а не экономия.
    //
    // Сбой вносится в тот самый шов восстановления случайного состояния, который
    // создатель обучателя и так принимает снаружи: подделывать продуктовый код
    // для этого не нужно. Шов отказывает один раз, поэтому сам откат проходит
    // штатно, и наружу выходит исходная ошибка, а не вторичный сбой отката.
    //
    // Доза: три шага, шесть предъявлений пар, восемь обращений к эталону,
    // двенадцать к обучаемой модели, двенадцать обратных проходов, три
    // обновления оптимизатора.
    [Fact]
    public void FailureAfterRestoreBegan_RollsBackAndForgetsTheRememberedAnswers()
    {
        const string failureMarker = "KDP_LC_RESTORE_SEAM_FAILED_ON_PURPOSE";
        using var roots = new TempRoots();
        var fixture = Fixture();
        var recipient = DpoProjectLoraRecipient.Capture(fixture.Policy, fixture.Reference, fixture.Adapters, fixture.Lora);
        var topology = LoraRandomStateIdentity.ComputeTopologySha256(Config, fixture.Lora, fixture.Seeds);
        var identity = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(fixture.Lora, fixture.Seeds);
        var profile = DpoExecutionProfile.Capture(Batches(fixture.Tokenizer)[0].Batch, recipient.Bindings);

        var savedRandom = fixture.Policy.CaptureRandomState(topology, identity, 1);
        var savedOptimizer = SavedOptimizerSnapshot(recipient);
        var proof = DpoContinuationProof.IssueVerified(
            profile, savedOptimizer, savedRandom, "kdp-lc-failed-restore", () => true);

        // Шов восстановления отказывает ровно один раз, по команде проверки.
        var failNextRestore = false;
        var restoreCalls = 0;
        void Restore(LlmModelRandomStateSnapshot state)
        {
            restoreCalls++;
            if (failNextRestore)
            {
                // Один отказ: следующий вызов — это уже откат, он обязан пройти.
                failNextRestore = false;
                throw new InvalidOperationException(failureMarker);
            }

            fixture.Policy.RestoreRandomState(state, new(topology, identity));
        }

        var owner = DpoTrainer.CreateForProjectLora(
            fixture.Policy, fixture.Reference, roots.Next("warmed-failed"),
            DpoMutationAdmissionCapability.Issue(new TestLease()),
            recipient, profile, proof, null,
            step => fixture.Policy.CaptureRandomState(topology, identity, step),
            Restore,
            2);

        // Прогрев тот же: второй шаг уже берёт ответы из памяти.
        var warmUp = new DpoStepVectorTiming();
        owner.StepVector(Batches(fixture.Tokenizer), 1, warmUp);
        var warmHit = new DpoStepVectorTiming();
        owner.StepVector(Batches(fixture.Tokenizer), 2, warmHit);
        Assert.Equal(4L, warmUp.Snapshot().ReferenceForward.CompletedCalls);
        Assert.Equal(0L, warmHit.Snapshot().ReferenceForward.CompletedCalls);
        var warmed = owner.ReferenceScoreCacheCounters;
        Assert.Equal(2, warmed.Hits);

        var optimizerBeforeFailure = owner.ExportOptimizerState().ToBytes();

        // Разрешённое восстановление начинается и падает внутри шва.
        failNextRestore = true;
        var failure = Assert.Throws<InvalidOperationException>(
            () => owner.TryRestoreContinuationState(savedRandom, savedOptimizer));

        // Наружу вышла исходная ошибка: значит откат прошёл штатно и вторичного
        // сбоя отката не было. Шов вызван дважды: попытка и откат.
        Assert.Equal(failureMarker, failure.Message);
        Assert.Equal(2, restoreCalls);

        // Откат вернул прежнее состояние оптимизатора.
        Assert.Equal(optimizerBeforeFailure, owner.ExportOptimizerState().ToBytes());
        Assert.All(fixture.Reference.Parameters(), parameter => Assert.False(parameter.Value.RequiresGrad));

        // Главное: память ответов забыта. Третий шаг тех же двух пар обязан
        // спросить эталон все четыре раза заново.
        var afterFailure = new DpoStepVectorTiming();
        owner.StepVector(Batches(fixture.Tokenizer), 3, afterFailure);
        Assert.Equal(4L, afterFailure.Snapshot().ReferenceForward.CompletedCalls);

        // Счётчики дельтами: два промаха, ни одного попадания, две записи.
        var afterThird = owner.ReferenceScoreCacheCounters;
        Assert.Equal(2, afterThird.Misses - warmed.Misses);
        Assert.Equal(0, afterThird.Hits - warmed.Hits);
        Assert.Equal(2, afterThird.Stores - warmed.Stores);

        // Назначенная доза, посчитанная по фактическим фазам.
        var phases = new[] { warmUp.Snapshot(), warmHit.Snapshot(), afterFailure.Snapshot() };
        Assert.Equal(6L, phases.Sum(phase => phase.VectorBatchesCompleted));
        Assert.Equal(8L, phases.Sum(phase => phase.ReferenceForward.CompletedCalls));
        Assert.Equal(12L, phases.Sum(phase => phase.PolicyForward.CompletedCalls));
        Assert.Equal(12L, phases.Sum(phase => phase.Backward.CompletedCalls));
        Assert.Equal(3L, phases.Sum(phase => phase.ApplyOptimizer.CompletedCalls));
    }

    // Допустимый сохранённый снимок оптимизатора того же получателя: настоящий ключ
    // связки и настоящая форма параметра, моменты нулевые.
    private static AdamWPortableState SavedOptimizerSnapshot(DpoProjectLoraRecipient recipient)
    {
        var binding = recipient.Bindings[0];
        var length = binding.Parameter.Value.Data.Length;
        return new AdamWPortableState(
            AdamWPortableState.CurrentFormatVersion,
            1,
            [new AdamWPortableParameterState(
                binding.Key,
                binding.Parameter.Value.Shape.Dimensions,
                new float[length],
                new float[length])]);
    }

    // Every parameter value of a model, in declaration order, as one flat list.
    private static List<float> Values(LlmModel model)
    {
        var values = new List<float>();
        foreach (var parameter in model.Parameters())
            values.AddRange(parameter.Value.Data);
        return values;
    }

    private static void AssertSameMetrics(DpoMetrics expected, DpoMetrics actual)
    {
        Assert.Equal(expected.Loss, actual.Loss, 6);
        Assert.Equal(expected.RewardMargin, actual.RewardMargin, 6);
        Assert.Equal(expected.ChosenReward, actual.ChosenReward, 6);
        Assert.Equal(expected.RejectedReward, actual.RejectedReward, 6);
    }

    private sealed record ModelFixture(
        LlmModel Policy, LlmModel Reference, BpeTokenizer Tokenizer,
        LoraConfig Lora, LoraPairRandomSeedAuthority Seeds, IReadOnlyList<LoraLinear> Adapters);

    private sealed record TrainerFixture(DpoTrainer Trainer, ModelFixture Fixture, string Topology, string Identity);

    private static ModelFixture Fixture()
    {
        var policy = PlainModel();
        var reference = PlainModel();
        var lora = new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] };
        var seeds = new LoraPairRandomSeedAuthority(73, "tiny.policy", "tiny.reference");
        var applied = LoraApplicator.ApplyFreshSeededPair(policy, reference, lora, seeds.RootInitializationSeed,
            seeds.PolicyDropoutStreamDomain, seeds.ReferenceDropoutStreamDomain);
        foreach (var parameter in reference.Parameters())
            parameter.Value.RequiresGrad = false;
        return new(policy, reference, Tokenizer(), lora, seeds, applied.FirstModelAdapters);
    }

    private static TrainerFixture Trainer(ModelFixture fixture, string path, int capacity)
    {
        var recipient = DpoProjectLoraRecipient.Capture(fixture.Policy, fixture.Reference, fixture.Adapters, fixture.Lora);
        var topology = LoraRandomStateIdentity.ComputeTopologySha256(Config, fixture.Lora, fixture.Seeds);
        var identity = LoraRandomStateIdentity.ComputeLoraConfigurationSha256(fixture.Lora, fixture.Seeds);
        var trainer = DpoTrainer.CreateForProjectLora(
            fixture.Policy, fixture.Reference, path,
            DpoMutationAdmissionCapability.Issue(new TestLease()),
            recipient,
            DpoExecutionProfile.Capture(Batches(fixture.Tokenizer)[0].Batch, recipient.Bindings),
            null, null,
            step => fixture.Policy.CaptureRandomState(topology, identity, step),
            state => fixture.Policy.RestoreRandomState(state, new(topology, identity)),
            capacity);
        return new(trainer, fixture, topology, identity);
    }

    private static IReadOnlyList<DpoVectorBatch> Batches(BpeTokenizer tokenizer)
        => Build(tokenizer, [("a", "b", "cc"), ("d", "ee", "f")]);

    private static IReadOnlyList<DpoVectorBatch> OtherBatches(BpeTokenizer tokenizer)
        => Build(tokenizer, [("g", "h", "ii"), ("j", "kk", "l")]);

    private static IReadOnlyList<DpoVectorBatch> Build(BpeTokenizer tokenizer, (string Prompt, string Chosen, string Rejected)[] rows)
        => rows.Select(row => new DpoVectorBatch(
            DpoExactPairMaterial.CreateBatch(tokenizer, row.Prompt, row.Chosen, row.Rejected,
                DpoTrainerPairCustoms.CreateAdmitted(row.Prompt, row.Chosen, row.Rejected,
                    DpoPairSideAuthorKind.DeterministicGenerator,
                    DpoPairSideAuthorKind.DeterministicGenerator, "simulation").Admission), [1f, 0f])).ToArray();

    private static BpeTokenizer Tokenizer()
    {
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("fixture", 256);
        return tokenizer;
    }

    private static LlmModel PlainModel()
    {
        var model = new LlmModel(Config);
        var random = new Random(73);
        foreach (var parameter in model.Parameters())
            for (var i = 0; i < parameter.Value.Data.Length; i++)
                parameter.Value.Data[i] = parameter.Value.Shape.Rank == 1 ? 1 : (float)(random.NextDouble() - .5) * .15f;
        return model;
    }


    private sealed class TestLease : IDpoMutationAdmissionIssuer
    {
        public IDisposable AcquireDpoTrainerMutationPermit() => new CancellationTokenSource();

        public void Dispose()
        {
        }
    }

    private sealed class TempRoots : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "kdp-lc-" + Guid.NewGuid().ToString("N"));

        public string Next(string name)
        {
            var path = Path.Combine(_root, name);
            Directory.CreateDirectory(path);
            return path;
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_root))
                    Directory.Delete(_root, recursive: true);
            }
            catch (IOException)
            {
                // Временный каталог опыта не должен ронять проверку.
            }
        }
    }
}

// Дешёвая приёмка общего договора сравнения состояния случайного потока
// (работа «Проверять состояние RNG, а не имя его коллекции», раздел пятый плана
// ремонта поздних переносов от 29 октября, файл 102).
//
// Что здесь проверяется. Сам помощник сравнения, а не обучение: помощник обязан
// быть чувствителен к подмене внутри записей снимка. Поэтому снимки собираются
// прямо из записей, без модели, без диска и без единого учебного шага.
//
// И отдельно ИЗМЕРЯЕТСЯ прежнее сравнение по тексту: те же самые подмены
// прогоняются через `ToString()` обоих снимков. Это измерение, а не
// предположение, — именно оно доказывает, что прежний дефект был настоящим.
//
// Чего эти проверки НЕ доказывают: они ничего не говорят о качестве обучения и
// не заявляют равенства расходуемой случайной траектории.
public sealed class RandomStateFieldComparisonSensitivityTests
{
    // Вывод проверки: измеренная таблица попадает в журнал прогона, а не только
    // в голову исполнителя.
    private readonly ITestOutputHelper _output;

    public RandomStateFieldComparisonSensitivityTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void AnUnchangedCopyPassesBothComparisons()
    {
        // Два отдельных экземпляра с одинаковыми значениями: сравнение по полям,
        // а не совпадение ссылок.
        var expected = Reference();
        var actual = Reference();

        // Новый помощник: ни одного расхождения и ни одного отказа.
        Assert.Empty(RandomStateFieldAssertions.Differences(expected, actual));
        RandomStateFieldAssertions.AssertEqual(expected, actual);

        // Прежнее сравнение по тексту на неизменённой копии тоже согласно —
        // поэтому этот случай сам по себе дефекта не показывает.
        Assert.Equal(expected.ToString(), actual.ToString());
    }

    [Fact]
    public void EverySubstitutionInsideTheEntriesIsRejectedByTheFieldComparison()
    {
        // Исходный снимок один на все подмены.
        var expected = Reference();
        _output.WriteLine("| подмена | расхождений у помощника | названное поле |");

        foreach (var (name, field, mutated) in Substitutions())
        {
            // Помощник обязан назвать хотя бы одно расхождение.
            var differences = RandomStateFieldAssertions.Differences(expected, mutated);
            Assert.NotEmpty(differences);
            // И притом именно то поле, которое подменили, а не случайное другое.
            Assert.Contains(differences, difference => difference.Contains(field, StringComparison.Ordinal));
            // Утверждающая обёртка обязана на этой подмене отказать, и отказ
            // обязан быть именно отказом договора, а не падением помощника.
            var failure = Assert.ThrowsAny<Exception>(
                () => RandomStateFieldAssertions.AssertEqual(expected, mutated));
            Assert.StartsWith("Состояние случайного потока различается по полям:", failure.Message, StringComparison.Ordinal);
            _output.WriteLine($"| {name} | {differences.Count} | {field} |");
        }
    }

    [Fact]
    public void TheOldTextComparisonMissedEveryOneOfThoseSubstitutions()
    {
        // Тот же исходный снимок и те же подмены, что и в проверке выше.
        var expected = Reference();
        // Текстовая запись печатает у списка записей только имя его типа, а не
        // содержимое: ни одного значения из самих записей в ней нет. Сама строка
        // печатается в журнал, чтобы измерение было видно, а не только заявлено.
        var referenceText = expected.ToString();
        _output.WriteLine($"текстовая запись снимка: {referenceText}");
        Assert.DoesNotContain(expected.Entries[0].AdapterKey, referenceText, StringComparison.Ordinal);
        Assert.DoesNotContain(expected.Entries[0].StreamDomain, referenceText, StringComparison.Ordinal);
        Assert.DoesNotContain(
            expected.Entries[0].ConsumedCursor.ToString(CultureInfo.InvariantCulture),
            referenceText,
            StringComparison.Ordinal);
        _output.WriteLine("| подмена | старое сравнение по тексту |");

        foreach (var (name, _, mutated) in Substitutions())
        {
            // Оснастка обязана быть честной: обе стороны измерения держат записи
            // в наборе того же вида, что и продуктовый снимок. Иначе в тексте
            // разошлись бы имена типов оснастки, а не состояние.
            Assert.Equal(expected.Entries.GetType(), mutated.Entries.GetType());
            // Измерение: прежнее сравнение считало подменённый снимок тем же самым.
            var oldComparisonSawThemAsEqual = string.Equals(
                expected.ToString(), mutated.ToString(), StringComparison.Ordinal);
            Assert.True(
                oldComparisonSawThemAsEqual,
                $"подмена «{name}»: прежнее сравнение по тексту её всё-таки поймало, запись измерения нужно поправить");
            _output.WriteLine($"| {name} | пропустило |");
        }
    }

    [Fact]
    public void TheSameEntriesInAnotherOrderAreAcceptedBecauseIdentityIsTheAdapterKey()
    {
        // Это единственное место, где договор по ключу мягче прежнего сравнения
        // по месту в массиве, и оно здесь названо прямо, а не умолчано: тот же
        // набор записей, переставленный местами, считается тем же состоянием.
        // Перестановка не меняет ни одного поля ни одной записи, а тождество
        // записи задаёт ключ адаптера, а не её позиция.
        var expected = Reference();
        var swapped = WithEntries(expected, expected.Entries[1], expected.Entries[0]);

        // Порядок записей действительно другой, иначе проверка была бы пустой.
        Assert.NotEqual(expected.Entries[0].AdapterKey, swapped.Entries[0].AdapterKey);
        // И при этом расхождений состояния нет.
        Assert.Empty(RandomStateFieldAssertions.Differences(expected, swapped));
    }

    [Fact]
    public void ADuplicatedAdapterKeyIsRejectedBecauseEntryIdentityBecomesAmbiguous()
    {
        // Возмещение за ту единственную мягкость: сравнение по месту в массиве
        // повтор ключа пропускало молча, а договор по ключу обязан его отвергнуть —
        // при повторе ключа тождество записи перестаёт быть однозначным.
        var expected = Reference();
        var duplicated = WithEntries(expected, expected.Entries[0], expected.Entries[0]);

        var differences = RandomStateFieldAssertions.Differences(expected, duplicated);
        Assert.Contains(differences, difference => difference.Contains("неоднозначно", StringComparison.Ordinal));
        _output.WriteLine($"повтор ключа адаптера: расхождений {differences.Count}");
    }

    /// <summary>Исходный снимок: заголовок и две записи двух установленных адаптеров.</summary>
    private static LlmModelRandomStateSnapshot Reference() => new(
        LlmModelRandomStateSnapshot.CurrentFormatVersion,
        LlmModelRandomStateSnapshot.CurrentAlgorithm,
        Entries(
            new LlmModelRandomStateEntry("blocks.0.attn.q_proj", "policy.dropout", 11UL, 22UL, 33UL),
            new LlmModelRandomStateEntry("blocks.0.attn.v_proj", "policy.dropout", 44UL, 55UL, 66UL)),
        "topology-sha256-of-the-fixture",
        "lora-configuration-sha256-of-the-fixture",
        2);

    /// <summary>Снимок с тем же заголовком и другим набором записей.</summary>
    private static LlmModelRandomStateSnapshot WithEntries(
        LlmModelRandomStateSnapshot source,
        params LlmModelRandomStateEntry[] entries)
        => source with { Entries = Entries(entries) };

    /// <summary>
    /// Набор записей ровно того же вида, какой строит продуктовый снимок:
    /// `LlmModel.CaptureRandomState` складывает записи в `List`. Это важно для
    /// измерения прежнего сравнения по тексту: текстовая запись печатает у
    /// списка ИМЯ ЕГО ТИПА, поэтому у обеих сторон измерения тип набора обязан
    /// совпадать с продуктовым. Иначе разошлись бы имена типов оснастки, а не
    /// состояния, и измерение показало бы несуществующую чувствительность.
    /// </summary>
    private static IReadOnlyList<LlmModelRandomStateEntry> Entries(
        params LlmModelRandomStateEntry[] entries)
        => new List<LlmModelRandomStateEntry>(entries);

    /// <summary>
    /// Пять названных нарядом случаев подмены: позиция потока, зерно (двумя
    /// отдельными зёрнами), область потока, ключ адаптера, добавленная и
    /// удалённая запись.
    /// </summary>
    private static IReadOnlyList<(string Name, string Field, LlmModelRandomStateSnapshot Mutated)> Substitutions()
    {
        // Исходные записи, от которых считаются подмены.
        var reference = Reference();
        var first = reference.Entries[0];
        var second = reference.Entries[1];
        return
        [
            ("изменена израсходованная позиция потока", "израсходованная позиция потока",
                WithEntries(reference, first with { ConsumedCursor = first.ConsumedCursor + 1 }, second)),
            ("изменено зерно начальной установки", "зерно начальной установки",
                WithEntries(reference, first with { InitializationSeed = first.InitializationSeed + 1 }, second)),
            ("изменено зерно отсева", "зерно отсева",
                WithEntries(reference, first with { DropoutSeed = first.DropoutSeed + 1 }, second)),
            ("изменена область потока", "область потока",
                WithEntries(reference, first with { StreamDomain = first.StreamDomain + ".changed" }, second)),
            ("изменён ключ адаптера", "отсутствует в полученном состоянии",
                WithEntries(reference, first with { AdapterKey = first.AdapterKey + ".changed" }, second)),
            ("добавлена запись", "число записей",
                WithEntries(reference, first, second,
                    new LlmModelRandomStateEntry("blocks.1.attn.q_proj", "policy.dropout", 77UL, 88UL, 99UL))),
            ("удалена запись", "число записей", WithEntries(reference, first))
        ];
    }
}
