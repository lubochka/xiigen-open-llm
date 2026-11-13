// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>
//
// Постоянное покрытие инварианта «память ответов эталона создаётся на любом
// пути создания обучателя предпочтений» (работа R1 плана ремонта 98 из папки
// docs/plans/replanning-possible-diffs/fixes-based-on-experiments, находка
// A29-01 аудита 97).
//
// Что здесь проверяется по-человечески. Обучатель предпочтений держит рядом с
// собой маленькую память ответов замороженного эталона: эталон не меняется,
// поэтому на уже заданный вопрос он вернёт те же два числа, и повторный проход
// через него не нужен. Когда обучение продолжают с сохранённого состояния, эта
// память обязана очиститься: прежние ответы считались для другого состояния и
// новому ходу не принадлежат.
//
// Что было сломано. Память создавалась только в закрытом конструкторе. Один из
// публичных конструкторов её не создавал вовсе — поле оставалось пустым. Пока
// очистки после успешного восстановления не было, пустоту никто не трогал.
// После того как очистку добавили, обычный публичный обучатель перестал
// переживать собственное же восстановление: обращение к пустому полю давало
// ошибку, обработчик уходил в откат, откат обращался к тому же пустому полю
// снова, и вместо честного результата возвращался вторичный сбой отката.
//
// Как это доказывается. Таблица всех шести способов создать обучатель: три
// публичных конструктора, два внутренних создателя и общий закрытый
// конструктор; у каждого читается фактическая ёмкость памяти ответов. Затем
// обычный публичный обучатель проходит четыре сценария восстановления и
// попытку изменить веса без разрешения сервера.
//
// Бюджет этой проверки. Ни одного учебного шага, ни одного прямого прохода, ни
// одного обратного прохода, ни одного обновления оптимизатора. Проверяется
// только договор создания и восстановления. Прежний прогретый сценарий памяти
// ответов живёт отдельно в `Tests/DpoReferenceScoreCacheTests.cs`, у него своя
// ненулевая учебная доза, и он сюда не переносится и с этим не смешивается.
//
// Чего это покрытие НЕ доказывает. Оно не измеряет качество обучения, не
// проверяет попадания и промахи прогретой памяти, не проверяет продолжение с
// диска и не утверждает, что исходный дефект останавливал какой-либо живой
// рабочий процесс: аудит этого не устанавливал, и проверка этого не заявляет.

namespace LlmFromScratch.Tests.ExperimentFixes;

using DPO;
using DPO.Models;
using LoRA;
using TensorCore.Core;
using Tokenizer;
using TrainingLoop.Optimizers;
using TransformerModel;
using TransformerModel.Config;

/// <summary>Одна строка таблицы способов создать обучатель предпочтений.</summary>
/// <param name="Family">Человеческое имя семейства создания.</param>
/// <param name="AssignedCapacity">Ёмкость памяти ответов, назначенная этому пути.</param>
public sealed record ReferenceCacheConstructorFamily(string Family, int AssignedCapacity);

public sealed class DpoReferenceCacheConstructorLifecycleTests
{
    // Самая маленькая пригодная модель: словарь байтов, короткий контекст, один слой.
    private static readonly ModelConfig Config = new()
    {
        VocabSize = 256, ContextLen = 8, HiddenDim = 8,
        NumLayers = 1, NumHeads = 2, NumKvHeads = 1, FfnHiddenDim = 16
    };

    // Родственный обход: ни один способ создать обучатель не должен оставлять
    // память ответов эталона пустой. Таблица перечисляет все шесть путей, а не
    // только тот, на котором дефект заметили.
    [Fact]
    public void EveryConstructorFamily_CreatesReferenceScoreCacheWithItsAssignedCapacity()
    {
        // Временные каталоги результатов: два пути создают каталог сразу.
        using var roots = new TempRoots();

        // Семейство 1: короткий публичный конструктор, который перенаправляет вызов.
        var delegatingPublic = new DpoTrainer(
            PlainModel(), PlainModel(), roots.Next("delegating-public"), DpoTrainer.DefaultLearningRate);

        // Семейство 2: публичный конструктор с наблюдателем публикации — то самое
        // место, где память ответов раньше не создавалась вовсе.
        var observerPublic = new DpoTrainer(
            PlainModel(), PlainModel(), roots.Next("observer-public"),
            DpoTrainer.DefaultLearningRate, publicationObserver: null);

        // Семейство 3: публичный конструктор с разрешением сервера (путь legacy):
        // он перенаправляет вызов в общий закрытый конструктор без получателя.
        var admittedLegacy = new DpoTrainer(
            PlainModel(), PlainModel(), roots.Next("admitted-legacy"),
            DpoMutationAdmissionCapability.Issue(new TestLease()));

        // Семейство 4: внутренний создатель подготовленного продолжения — тот же
        // закрытый конструктор, но без получателя и с отложенным каталогом.
        var preparedContinuation = DpoTrainer.CreateForPreparedContinuation(
            PlainModel(), PlainModel(), roots.Next("prepared-continuation"),
            DpoMutationAdmissionCapability.Issue(new TestLease()),
            DpoTrainer.DefaultLearningRate, null, null, null);

        // Семейство 5: внутренний создатель проектного получателя с выключенной
        // памятью ответов — это плечо сравнения, а не сломанная память.
        var projectWithoutCache = ProjectTrainer(roots.Next("project-off"), capacity: 0);

        // Семейство 6: тот же создатель с назначенной ёмкостью два — ровно той,
        // которая измерена в опыте и принята как рабочая.
        var projectWithCache = ProjectTrainer(
            roots.Next("project-on"), DpoTrainer.DefaultReferenceScoreCacheCapacity);

        // Фактические ёмкости читаются у созданных объектов, а не предполагаются.
        var observed = new[]
        {
            new ReferenceCacheConstructorFamily(
                "короткий публичный конструктор", delegatingPublic.ReferenceScoreCacheCounters.Capacity),
            new ReferenceCacheConstructorFamily(
                "публичный конструктор с наблюдателем", observerPublic.ReferenceScoreCacheCounters.Capacity),
            new ReferenceCacheConstructorFamily(
                "публичный конструктор с разрешением", admittedLegacy.ReferenceScoreCacheCounters.Capacity),
            new ReferenceCacheConstructorFamily(
                "подготовленное продолжение", preparedContinuation.ReferenceScoreCacheCounters.Capacity),
            new ReferenceCacheConstructorFamily(
                "проектный получатель, память выключена", projectWithoutCache.ReferenceScoreCacheCounters.Capacity),
            new ReferenceCacheConstructorFamily(
                "проектный получатель, память включена", projectWithCache.ReferenceScoreCacheCounters.Capacity)
        };

        // Ожидание выписано буквально: пять путей без получателя обучения дают
        // ноль, и только путь с получателем и назначенной ёмкостью даёт два.
        var expected = new[]
        {
            new ReferenceCacheConstructorFamily("короткий публичный конструктор", 0),
            new ReferenceCacheConstructorFamily("публичный конструктор с наблюдателем", 0),
            new ReferenceCacheConstructorFamily("публичный конструктор с разрешением", 0),
            new ReferenceCacheConstructorFamily("подготовленное продолжение", 0),
            new ReferenceCacheConstructorFamily("проектный получатель, память выключена", 0),
            new ReferenceCacheConstructorFamily("проектный получатель, память включена", 2)
        };

        // Сравнение идёт одной таблицей: пропуск любой строки будет виден сразу.
        Assert.Equal(expected, observed);

        // Ни один из шести путей не успел ничего спросить у эталона и ничего запомнить.
        foreach (var trainer in new[]
                 {
                     delegatingPublic, observerPublic, admittedLegacy,
                     preparedContinuation, projectWithoutCache, projectWithCache
                 })
        {
            var counters = trainer.ReferenceScoreCacheCounters;
            Assert.Equal((0, 0, 0, 0), (counters.Lookups, counters.Hits, counters.Misses, counters.Stores));
        }
    }

    // Главный контрпример аудита: обычный публичный обучатель отдаёт собственную
    // память оптимизатора и принимает её обратно. Эта ветвь обязана завершиться
    // успехом, а не вторичным сбоем отката на пустом поле.
    [Fact]
    public void PublicTrainer_AcceptedOwnOptimizerRestore_SucceedsWithoutRollbackFailure()
    {
        // Один временный каталог результатов на эту проверку.
        using var roots = new TempRoots();
        // Обучатель строится обычным публичным путём, без разрешения сервера.
        var policy = PlainModel();
        var trainer = new DpoTrainer(policy, PlainModel(), roots.Next("accepted-restore"),
            DpoTrainer.DefaultLearningRate, publicationObserver: null);
        // Снимок весов до восстановления: доказательство, что веса не тронуты.
        var weightsBefore = Weights(policy);

        // Своё же состояние оптимизатора — заведомо допустимый снимок.
        var ownState = trainer.ExportOptimizerState();

        // Восстановление того же состояния обязано быть принятым.
        Assert.True(trainer.TryRestoreContinuationState(null, ownState));

        // Память ответов осталась выключенной и исправной: её можно прочитать.
        Assert.Equal(0, trainer.ReferenceScoreCacheCounters.Capacity);
        // Счётчик шага оптимизатора вернулся к тому же значению, что был отдан.
        Assert.Equal(ownState.Step, trainer.ExportOptimizerState().Step);
        // Веса обучаемой модели от восстановления своего же состояния не изменились.
        Assert.Equal(weightsBefore, Weights(policy));
    }

    // Прежде допустимый пустой запрос восстановления проходит тем же местом и
    // поэтому тоже падал. Он обязан вернуть успех и ничего не сломать.
    [Fact]
    public void PublicTrainer_EmptyRestoreRequest_SucceedsWithoutRollbackFailure()
    {
        // Один временный каталог результатов на эту проверку.
        using var roots = new TempRoots();
        // Обычный публичный обучатель без разрешения сервера.
        var policy = PlainModel();
        var trainer = new DpoTrainer(policy, PlainModel(), roots.Next("empty-restore"),
            DpoTrainer.DefaultLearningRate, publicationObserver: null);
        // Снимок весов до вызова: пустой запрос не имеет права их трогать.
        var weightsBefore = Weights(policy);

        // Ни состояния генератора, ни состояния оптимизатора — прежний допустимый вызов.
        Assert.True(trainer.TryRestoreContinuationState(null, null));

        // Память ответов цела, веса целы.
        Assert.Equal(0, trainer.ReferenceScoreCacheCounters.Capacity);
        Assert.Equal(weightsBefore, Weights(policy));
    }

    // Неподходящий снимок обязан быть отвергнут честным «нет», а не превращаться
    // во вторичный сбой отката на пустой памяти ответов.
    [Fact]
    public void PublicTrainer_UnsuitableSnapshot_IsRejectedWithoutSecondaryRollbackFailure()
    {
        // Один временный каталог результатов на эту проверку.
        using var roots = new TempRoots();
        // Обычный публичный обучатель без разрешения сервера.
        var policy = PlainModel();
        var trainer = new DpoTrainer(policy, PlainModel(), roots.Next("unsuitable-snapshot"),
            DpoTrainer.DefaultLearningRate, publicationObserver: null);
        // Снимок весов до вызова: отвергнутое восстановление их не меняет.
        var weightsBefore = Weights(policy);

        // Снимок с именем параметра, которого у этой модели нет вовсе.
        var foreignState = new AdamWPortableState(
            AdamWPortableState.CurrentFormatVersion,
            7,
            [new AdamWPortableParameterState("не-наш-параметр", [2, 2], new float[4], new float[4])]);

        // Отказ обязан быть обычным «нет», без исключения и без кода сбоя отката.
        Assert.False(trainer.TryRestoreContinuationState(null, foreignState));

        // Откат вернул прежний счётчик шага, а не чужую семёрку.
        Assert.Equal(0, trainer.ExportOptimizerState().Step);
        // Память ответов цела, веса целы.
        Assert.Equal(0, trainer.ReferenceScoreCacheCounters.Capacity);
        Assert.Equal(weightsBefore, Weights(policy));
    }

    // Починка памяти ответов не смягчает допуск: публичный обучатель без
    // выданного сервером разрешения по-прежнему не может изменить веса.
    [Fact]
    public void PublicTrainer_WeightMutationWithoutAdmission_StaysForbidden()
    {
        // Один временный каталог результатов на эту проверку.
        using var roots = new TempRoots();
        // Путь каталога берётся заранее: после отказа он не должен появиться.
        var checkpointDirectory = Path.Combine(roots.Root, "mutation-refused");
        // Обычный публичный обучатель без разрешения сервера.
        var policy = PlainModel();
        var trainer = new DpoTrainer(policy, PlainModel(), checkpointDirectory,
            DpoTrainer.DefaultLearningRate, publicationObserver: null);
        // Снимок весов до попытки: отказ обязан оставить их побайтово теми же.
        var weightsBefore = Weights(policy);

        // Попытка учебного шага отвергается до прямого прохода и до любой мутации.
        var refusal = Assert.Throws<DpoMutationAdmissionException>(
            () => trainer.Step(AdmittedLegacyBatch(), step: 1));

        // Код отказа именно про отсутствующее разрешение сервера.
        Assert.Equal(DpoMutationAdmissionCapability.RequiredErrorCode, refusal.ErrorCode);
        // Веса не изменились, каталог результатов не создан.
        Assert.Equal(weightsBefore, Weights(policy));
        Assert.False(Directory.Exists(checkpointDirectory));
    }

    // Проектный обучатель собирается из настоящего получателя адаптеров и
    // настоящего профиля исполнения: ни одного вызова модели здесь нет.
    private static DpoTrainer ProjectTrainer(string checkpointDirectory, int capacity)
    {
        // Обучаемая и замороженная модели одного размера.
        var policy = PlainModel();
        var reference = PlainModel();
        // Маленькая настройка адаптеров: ровно та, что используется в соседней проверке.
        var lora = new LoraConfig { Rank = 2, Alpha = 4, Dropout = 0, TargetModules = ["q_proj", "v_proj"] };
        // Закреплённые зёрна: пара адаптеров создаётся воспроизводимо.
        var seeds = new LoraPairRandomSeedAuthority(73, "tiny.policy", "tiny.reference");
        // Создание пары адаптеров — подготовка, а не обучение.
        var applied = LoraApplicator.ApplyFreshSeededPair(policy, reference, lora, seeds.RootInitializationSeed,
            seeds.PolicyDropoutStreamDomain, seeds.ReferenceDropoutStreamDomain);
        // Эталон заморожен: его параметры не получают градиента.
        foreach (var parameter in reference.Parameters())
            parameter.Value.RequiresGrad = false;
        // Получатель обучения — именно эти адаптеры обучаемой модели.
        var recipient = DpoProjectLoraRecipient.Capture(policy, reference, applied.FirstModelAdapters, lora);
        // Профиль исполнения снимается с настоящей пары точного материала.
        var profile = DpoExecutionProfile.Capture(ExactVectorBatch().Batch, recipient.Bindings);
        // Создание обучателя проектного получателя с назначенной ёмкостью памяти.
        return DpoTrainer.CreateForProjectLora(
            policy, reference, checkpointDirectory,
            DpoMutationAdmissionCapability.Issue(new TestLease()),
            recipient, profile, null, null, null, null, capacity);
    }

    // Одна пара точного материала для снятия профиля исполнения.
    private static DpoVectorBatch ExactVectorBatch()
    {
        // Токенизатор обучается на крошечной строке: это подготовка данных.
        var tokenizer = new BpeTokenizer();
        tokenizer.Train("fixture", 256);
        // Допуск пары выдаётся обычным проектным способом.
        var admission = DpoTrainerPairCustoms.CreateAdmitted(
            "a", "b", "cc",
            DpoPairSideAuthorKind.DeterministicGenerator,
            DpoPairSideAuthorKind.DeterministicGenerator,
            "r1-constructor-lifecycle").Admission;
        // Готовая пара с весом обучения и весом сравнения.
        return new DpoVectorBatch(
            DpoExactPairMaterial.CreateBatch(tokenizer, "a", "b", "cc", admission), [1f, 0f]);
    }

    // Минимальная допущенная пара прежнего формата для проверки отказа допуска.
    private static DpoBatch AdmittedLegacyBatch()
    {
        // Три разрешённых идентификатора на каждой стороне пары.
        float[] chosen = [1f, 2f, 3f];
        float[] rejected = [1f, 4f, 3f];
        // Маски полностью заполнены: все позиции участвуют.
        float[] chosenMask = [1f, 1f, 1f];
        float[] rejectedMask = [1f, 1f, 1f];
        // Пара собирается с обычным выданным допуском.
        return new DpoBatch
        {
            ChosenIds = new Tensor(chosen, new TensorShape(1, 3)),
            RejectedIds = new Tensor(rejected, new TensorShape(1, 3)),
            ChosenMask = new Tensor(chosenMask, new TensorShape(1, 3)),
            RejectedMask = new Tensor(rejectedMask, new TensorShape(1, 3)),
            Admission = DpoTrainerPairCustoms.CreateAdmitted(
                "r1 prompt", "r1 chosen", "r1 rejected",
                DpoPairSideAuthorKind.User,
                DpoPairSideAuthorKind.OpenTeacher,
                "r1-constructor-lifecycle").Admission
        };
    }

    // Все числа модели в порядке объявления одним плоским списком.
    private static List<float> Weights(LlmModel model)
    {
        // Список собирается по параметрам, чтобы сравнение было побайтовым по значению.
        var values = new List<float>();
        foreach (var parameter in model.Parameters())
            values.AddRange(parameter.Value.Data);
        return values;
    }

    // Маленькая модель с воспроизводимым заполнением чисел.
    private static LlmModel PlainModel()
    {
        // Создание модели заданной конфигурации.
        var model = new LlmModel(Config);
        // Закреплённое зерно: одинаковые числа при каждом прогоне.
        var random = new Random(73);
        // Векторы смещений заполняются единицей, матрицы — малыми числами вокруг нуля.
        foreach (var parameter in model.Parameters())
            for (var i = 0; i < parameter.Value.Data.Length; i++)
                parameter.Value.Data[i] = parameter.Value.Shape.Rank == 1 ? 1 : (float)(random.NextDouble() - .5) * .15f;
        return model;
    }

    // Выданное сервером разрешение в проверке: настоящий внешний допуск здесь не нужен.
    private sealed class TestLease : IDpoMutationAdmissionIssuer
    {
        // Разрешение выдаётся как обычный освобождаемый объект.
        public IDisposable AcquireDpoTrainerMutationPermit() => new CancellationTokenSource();

        // Собственных ресурсов у этой выдачи нет.
        public void Dispose()
        {
        }
    }

    // Временные каталоги результатов, которые убираются после проверки.
    private sealed class TempRoots : IDisposable
    {
        // Общий корень всех каталогов одной проверки.
        public string Root { get; } =
            Path.Combine(Path.GetTempPath(), "r1-ctor-" + Guid.NewGuid().ToString("N"));

        // Очередной именованный каталог внутри общего корня.
        public string Next(string name)
        {
            // Полный путь собирается от общего корня.
            var path = Path.Combine(Root, name);
            // Каталог создаётся заранее: конструктор может ожидать его существования.
            Directory.CreateDirectory(path);
            return path;
        }

        // Уборка общего корня после проверки.
        public void Dispose()
        {
            try
            {
                // Корень удаляется целиком вместе со всем содержимым.
                if (Directory.Exists(Root))
                    Directory.Delete(Root, recursive: true);
            }
            catch (IOException)
            {
                // Временный каталог не должен ронять проверку.
            }
        }
    }
}
