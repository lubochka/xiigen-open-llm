// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Text.Json;
using AgentInfrastructure.Logical;
using ProjectWikiDomain;

// Что здесь проверяется, по-человечески.
//
// У домена «план работ по проекту» есть две дороги, по которым чужое обученное состояние попадает
// в живую машину: загрузка передаточной записи (обычная выгрузка и обратная загрузка) и загрузка
// контрольной точки (в том числе из файла на диске). Опасность: в контрольной точке лежит не
// только число точности, но и ЗАЯВЛЕНИЕ «порог качества пройден». Если такому заявлению поверить,
// машина объявит себя прошедшей порог, не имея на это чисел, и дальше будет подписывать свои
// ссылки на контрольную точку и на паспорт модели как проверенные оценкой, а точность экспорта
// выдавать как настоящую.
//
// Что обязан гарантировать продукт, и что именно проверяет этот файл:
// 1. Свидетельство заслона проверяется ДО любого изменения живого состояния: при отказе прежние
//    ответы, ссылки, обученные хранилища и прежний заслон машины остаются целы.
// 2. Заявленная пройденность, не подтверждённая собственными числами, отвергается с точной
//    причиной, названной словами, а не только общим кодом.
// 3. Точность обязана быть измеренным числом (ни «не-число», ни бесконечность) и лежать в отрезке
//    от нуля до единицы; действующий порог — ноль целых семьдесят пять сотых.
// 4. Дорога передаточной записи имеет право НАЗНАЧИТЬ пройденность по числам; дорога контрольной
//    точки — только ЗАПРЕТИТЬ: сохранённый флаг сам пройденности не даёт.
// 5. Заявление смыслового качества при загрузке не переносится ни на одной дороге.
//
// Честная граница этого файла. Механизм защиты в продукте уже написан; это приёмка, а не починка.
// На дороге передаточной записи отказ СЕГОДНЯ недостижим через открытый вход: создатель кандидата
// строит его без заявленной пройденности, а валидатор отказывает только заявленной и
// неподтверждённой. Поэтому на этой дороге здесь проверяется назначение пройденности по числам, а
// не ветка отказа — так и сказано в самом продукте рядом с вызовом валидатора.
//
// Чего этот файл НЕ доказывает. Он не проверяет качество обучения, не измеряет метрики и не
// доказывает, что модель чему-то научилась: обучение здесь нужно только как непустое живое
// состояние, по которому видно, менялось ли оно после отказа. Конечное бессмысленное число
// (например порядка 1e308) остаётся измеренным по общему правилу домена, выходит за отрезок от
// нуля до единицы и потому отвергается диапазоном, а не измеренностью, — отдельного запрета
// бессмысленного диапазона здесь нет.
//
// Соседний файл проверок «доверенный порог маршрутизации» (RoutingGateTrustedThresholdTests)
// частично пересекается с этим по дороге контрольной точки: там уже есть случаи точности одна
// десятая, «не-числа» и честной точности восемь десятых. Пересечение сохранено намеренно: тот файл
// принадлежит другой строке работ (доверенные пороги маршрутизации) и сам помечен как
// непрогнанный, а здесь закрываются именно границы порога, право назначения, выход за диапазон,
// файловая дорога и удержание прежних ответов, которых там нет.
public sealed class OrchestrationPlanGateLoadAcceptanceTests
{
    // Действующий порог приёмки записан здесь отдельным числом, а не берётся у продукта: если
    // проверка спросит порог у самого продукта, она послушно последует за испорченным порогом и
    // перестанет что-либо доказывать.
    private const double ActiveAccuracyThreshold = 0.75;

    // Подсказка, по которой у обученной машины спрашивается обычный ответ. Та же, что в учебной
    // паре, чтобы близость к обученному примеру была заведомо здоровой.
    private const string PlanPrompt = "continue implementation";

    // Вторая подсказка, заведомо не совпадающая с учебной: так в снимке живого состояния видно не
    // только попадание в обученный пример, но и ответ на незнакомую просьбу.
    private const string UnseenPrompt = "собери документацию по проекту";

    // ── Дорога контрольной точки: заявленная пройденность обязана подтверждаться числами ────────

    [Fact]
    public void CheckpointClaimingPassage_WithAccuracyFarBelowThreshold_IsRefusedWithTheNamedReason()
    {
        // Живая машина с непустым обученным состоянием и честным пройденным заслоном: именно её
        // ответы и ссылки обязаны остаться целыми после отказа.
        var machine = TrainedMachineWithHonestPassedGate(0.90, "eval-honest-live");
        // Снимок всего наблюдаемого снаружи состояния до попытки загрузки.
        var before = LiveStateSnapshot(machine);

        // Чужая контрольная точка: пустое обученное состояние, другая версия и заслон, который
        // заявляет пройденный порог при точности одна десятая.
        var hostile = ForeignCheckpointWithGate(Gate("eval-low", 0.10, claimsPassage: true));

        var imported = machine.ImportCheckpoint(hostile);

        // Отказ, а не живое состояние.
        Assert.False(imported.IsSuccess);
        // Код отказа — именно тот открытый код, по которому вызывающий отличает эту причину.
        Assert.Equal(OrchestrationPlanMachine.ImportedGateContradictsEvidenceErrorCode, imported.ErrorCode);
        // Причина названа словами «ниже действующего порога». Сравниваем по словам, а не по записи
        // числа: запись числа зависит от культуры среды, слова — нет.
        Assert.Contains("is below the active threshold", imported.ErrorMessage);
        // Прежние ответы, ссылки, обученные хранилища и прежний заслон не изменились.
        Assert.Equal(before, LiveStateSnapshot(machine));
    }

    [Fact]
    public void CheckpointClaimingPassage_JustUnderTheThreshold_IsRefused()
    {
        // Граница снизу: точность на одну тысячную ниже порога. Это самый близкий к успеху отказ,
        // поэтому он и проверяется отдельно от заведомо низкой точности.
        var machine = TrainedMachineWithHonestPassedGate(0.90, "eval-honest-live");
        var before = LiveStateSnapshot(machine);

        var hostile = ForeignCheckpointWithGate(Gate("eval-just-under", 0.749, claimsPassage: true));

        var imported = machine.ImportCheckpoint(hostile);

        // Отказ с тем же кодом и той же названной причиной: число ниже порога.
        Assert.False(imported.IsSuccess);
        Assert.Equal(OrchestrationPlanMachine.ImportedGateContradictsEvidenceErrorCode, imported.ErrorCode);
        Assert.Contains("is below the active threshold", imported.ErrorMessage);
        // Живое состояние цело.
        Assert.Equal(before, LiveStateSnapshot(machine));
    }

    [Fact]
    public async Task CheckpointClaimingPassage_ExactlyAtTheThreshold_IsAcceptedAndTheSemanticQualityClaimIsDropped()
    {
        // Положительный контроль и граница сверху: ровно порог обязан проходить, иначе проверка
        // была бы просто запретом работы. Эта проверка зелёная и до, и после любого ослабления
        // порога — её падение означало бы, что приёмка закрыла честный прогон.
        var machine = new OrchestrationPlanMachine();
        // Заслон ровно на пороге, и вместе с ним заявлено смысловое качество — чтобы было видно,
        // что именно это заявление при загрузке снимается.
        var honest = machine.ExportCheckpoint() with
        {
            LastEvaluationGate = Gate("eval-at-threshold", ActiveAccuracyThreshold, claimsPassage: true, claimsSemanticQuality: true)
        };

        var imported = machine.ImportCheckpoint(honest);

        // Загрузка принята.
        Assert.True(imported.IsSuccess, imported.ErrorMessage);
        // Пройденность истинна: числа её подтверждают.
        Assert.NotNull(machine.LastEvaluationGate);
        Assert.True(machine.LastEvaluationGate!.PassesGate);
        // Точность сохранена той же, что пришла.
        Assert.Equal(ActiveAccuracyThreshold, machine.LastEvaluationGate.Accuracy);
        // А заявление смыслового качества снято: его при загрузке не переносят.
        Assert.False(machine.LastEvaluationGate.SemanticQualityClaimed);
        // Принятая оценка видна наружу обычной выгрузкой: и точностью, и подписанными ссылками.
        var export = await machine.ExportAsync();
        Assert.True(export.IsSuccess, export.ErrorMessage);
        Assert.Equal(ActiveAccuracyThreshold, export.Value!.AccuracyAtExport);
        Assert.Contains("eval/eval-at-threshold", export.Value.ManifestRef);
    }

    [Fact]
    public async Task CheckpointWithoutClaimedPassage_AndHighAccuracy_StaysNotPassed()
    {
        // Сердце разницы между дорогами: на дороге контрольной точки числа НЕ назначают
        // пройденность. Точность девять десятых заведомо выше порога, но пройденность не заявлена —
        // значит её и не появляется. Если бы эта дорога получила право назначать по числам, сюда
        // приехала бы пройденность, которую никто не подтверждал оценкой на этой машине.
        var machine = new OrchestrationPlanMachine();
        var honest = machine.ExportCheckpoint() with
        {
            LastEvaluationGate = Gate("eval-unclaimed", 0.90, claimsPassage: false, claimsSemanticQuality: true)
        };

        var imported = machine.ImportCheckpoint(honest);

        // Загрузка разрешена: честное непройденное состояние не запрещено.
        Assert.True(imported.IsSuccess, imported.ErrorMessage);
        // Но пройденности нет.
        Assert.NotNull(machine.LastEvaluationGate);
        Assert.False(machine.LastEvaluationGate!.PassesGate);
        // Заявление смыслового качества снято и здесь.
        Assert.False(machine.LastEvaluationGate.SemanticQualityClaimed);
        // Наружу это видно как отсутствие пройденного заслона: точность экспорта нулевая, а ссылки
        // подписаны непроверенной основой, а не опознанием оценки.
        var export = await machine.ExportAsync();
        Assert.True(export.IsSuccess, export.ErrorMessage);
        Assert.Equal(0.0, export.Value!.AccuracyAtExport);
        Assert.Contains("baseline_unverified", export.Value.CheckpointRef);
        Assert.Contains("baseline_unverified", export.Value.ManifestRef);
    }

    // Три значения, которые измерением не являются: «не-число» ложно в любом сравнении, а обе
    // бесконечности сравнение с порогом проходят или проваливают бессмысленно.
    [Theory]
    [InlineData("не-число", double.NaN)]
    [InlineData("плюс бесконечность", double.PositiveInfinity)]
    [InlineData("минус бесконечность", double.NegativeInfinity)]
    public void CheckpointClaimingPassage_WithUnmeasuredAccuracy_IsRefusedForNotBeingMeasured(
        string caseName,
        double unmeasuredAccuracy)
    {
        // Общее правило домена обязано называть такое значение неизмеренным — на нём стоит отказ.
        Assert.False(
            ProjectWikiMeasuredNumberRule.IsMeasured(unmeasuredAccuracy),
            $"Значение «{caseName}» не является измерением.");

        var machine = TrainedMachineWithHonestPassedGate(0.90, "eval-honest-live");
        var before = LiveStateSnapshot(machine);

        var hostile = ForeignCheckpointWithGate(Gate("eval-unmeasured", unmeasuredAccuracy, claimsPassage: true));

        var imported = machine.ImportCheckpoint(hostile);

        // Отказ, и причина названа именно неизмеренностью — не «ниже порога» и не «вне отрезка».
        Assert.False(imported.IsSuccess, $"Значение «{caseName}» обязано быть отвергнуто.");
        Assert.Equal(OrchestrationPlanMachine.ImportedGateContradictsEvidenceErrorCode, imported.ErrorCode);
        Assert.Contains("accuracy is not a measured number", imported.ErrorMessage);
        // Порядок причин важен: про порог здесь не говорится вовсе.
        Assert.DoesNotContain("is below the active threshold", imported.ErrorMessage);
        // Живое состояние цело.
        Assert.Equal(before, LiveStateSnapshot(machine));
    }

    // Два измеренных, но негодных числа: доля верных ответов не может быть больше единицы и не
    // может быть отрицательной. Это не низкий результат, а негодное число.
    [Theory]
    [InlineData("больше единицы", 1.5)]
    [InlineData("отрицательная доля", -0.10)]
    public void CheckpointClaimingPassage_WithAccuracyOutsideTheZeroToOneRange_IsRefusedForRange(
        string caseName,
        double outOfRangeAccuracy)
    {
        // Такое число измерено — значит отказ обязан прийти именно от проверки диапазона.
        Assert.True(
            ProjectWikiMeasuredNumberRule.IsMeasured(outOfRangeAccuracy),
            $"Значение «{caseName}» конечно и считается измеренным.");

        var machine = TrainedMachineWithHonestPassedGate(0.90, "eval-honest-live");
        var before = LiveStateSnapshot(machine);

        var hostile = ForeignCheckpointWithGate(Gate("eval-out-of-range", outOfRangeAccuracy, claimsPassage: true));

        var imported = machine.ImportCheckpoint(hostile);

        // Отказ с причиной про выход за отрезок от нуля до единицы.
        Assert.False(imported.IsSuccess, $"Значение «{caseName}» обязано быть отвергнуто.");
        Assert.Equal(OrchestrationPlanMachine.ImportedGateContradictsEvidenceErrorCode, imported.ErrorCode);
        Assert.Contains("accuracy is outside the zero-to-one range", imported.ErrorMessage);
        // Ни измеренность, ни порог здесь не названы: причина одна и точная.
        Assert.DoesNotContain("accuracy is not a measured number", imported.ErrorMessage);
        Assert.DoesNotContain("is below the active threshold", imported.ErrorMessage);
        // Живое состояние цело.
        Assert.Equal(before, LiveStateSnapshot(machine));
    }

    // ── Дорога контрольной точки: тот же заслон через файл на диске ──────────────────────────────

    [Fact]
    public async Task FileCheckpointClaimingPassage_WithLowAccuracy_IsRefusedByTheFileRoad()
    {
        // Реестр прямо требует закрыть загрузку файловой контрольной точки, а не только загрузку
        // записи в памяти: в продукте это отдельный открытый вход, который читает чужой файл.
        var directory = CreateTemporaryDirectory();
        try
        {
            var path = Path.Combine(directory, "hostile-checkpoint.json");
            // Файл пишет не продукт, а сторона: так и выглядит чужая контрольная точка на диске.
            var hostile = ForeignCheckpointWithGate(Gate("eval-file-low", 0.10, claimsPassage: true));
            await File.WriteAllTextAsync(
                path,
                JsonSerializer.Serialize(hostile, OrchestrationPlanJsonOptions.Standard));

            var load = await OrchestrationPlanMachine.LoadCheckpointAsync(path);

            // Файловая дорога отказывает тем же кодом и той же названной причиной.
            Assert.False(load.IsSuccess);
            Assert.Equal(OrchestrationPlanMachine.ImportedGateContradictsEvidenceErrorCode, load.ErrorCode);
            Assert.Contains("is below the active threshold", load.ErrorMessage);
            // И никакой машины вызывающему не выдано: принимать нечего.
            Assert.Null(load.Value);
        }
        finally
        {
            // Временный каталог удаляется в том же прогоне, независимо от исхода проверки.
            DeleteTemporaryDirectory(directory);
        }
    }

    [Fact]
    public async Task FileCheckpointWithAnHonestPassedGate_IsLoadedByTheFileRoad()
    {
        // Положительный контроль файловой дороги: честная пройденная оценка обязана загружаться,
        // иначе защита просто закрыла бы законный путь.
        var directory = CreateTemporaryDirectory();
        try
        {
            var path = Path.Combine(directory, "honest-checkpoint.json");
            var honest = ForeignCheckpointWithGate(
                Gate("eval-file-honest", 0.80, claimsPassage: true, claimsSemanticQuality: true));
            await File.WriteAllTextAsync(
                path,
                JsonSerializer.Serialize(honest, OrchestrationPlanJsonOptions.Standard));

            var load = await OrchestrationPlanMachine.LoadCheckpointAsync(path);

            // Машина выдана, пройденность истинна, заявление смыслового качества снято.
            Assert.True(load.IsSuccess, load.ErrorMessage);
            Assert.NotNull(load.Value);
            var loaded = load.Value!.Machine;
            Assert.NotNull(loaded.LastEvaluationGate);
            Assert.True(loaded.LastEvaluationGate!.PassesGate);
            Assert.Equal(0.80, loaded.LastEvaluationGate.Accuracy);
            Assert.False(loaded.LastEvaluationGate.SemanticQualityClaimed);
        }
        finally
        {
            DeleteTemporaryDirectory(directory);
        }
    }

    // ── Дорога передаточной записи: обычный круг «вывоз → ввоз» ─────────────────────────────────

    [Fact]
    public async Task RealExportImportCircle_WithLowAccuracy_LeavesPassageFalse()
    {
        // Настоящий круг: машина-источник честно прогнала оценку с низким результатом, выгрузила
        // себя обычным членом выгрузки, и эта выгрузка загружается в другую машину.
        var source = new OrchestrationPlanMachine();
        var lowGate = source.ExportCheckpoint() with
        {
            LastEvaluationGate = Gate("eval-circle-low", 0.10, claimsPassage: false)
        };
        Assert.True(source.ImportCheckpoint(lowGate).IsSuccess);

        var exported = await source.ExportAsync();
        Assert.True(exported.IsSuccess, exported.ErrorMessage);

        var target = new OrchestrationPlanMachine();
        var imported = await target.ImportAsync(exported.Value!);

        // Загрузка прошла: честный низкий результат не запрещён.
        Assert.True(imported.IsSuccess, imported.ErrorMessage);
        // Но пройденности после ввоза нет, и точность экспорта нулевая.
        Assert.False(target.LastEvaluationGate is { PassesGate: true });
        var targetExport = await target.ExportAsync();
        Assert.True(targetExport.IsSuccess, targetExport.ErrorMessage);
        Assert.Equal(0.0, targetExport.Value!.AccuracyAtExport);
        Assert.Contains("baseline_unverified", targetExport.Value.ManifestRef);
    }

    [Fact]
    public async Task TransferRecordCarryingLowAccuracyWithEvaluationSignedRefs_DoesNotGrantPassage()
    {
        // Тот же низкий результат, но враждебнее: передаточная запись приходит уже с подписанными
        // опознанием оценки ссылками, то есть выглядит как проверенная. Прежний код объявлял
        // пройденным любой положительный результат, и такая запись давала пройденный порог.
        var machine = new OrchestrationPlanMachine();

        var imported = await machine.ImportAsync(TransferRecord(0.10, "eval-record-low"));

        // Загрузка прошла, но пройденности нет: её назначают только числа не ниже порога.
        Assert.True(imported.IsSuccess, imported.ErrorMessage);
        Assert.False(machine.LastEvaluationGate is { PassesGate: true });
        var export = await machine.ExportAsync();
        Assert.True(export.IsSuccess, export.ErrorMessage);
        Assert.Equal(0.0, export.Value!.AccuracyAtExport);
        Assert.Contains("baseline_unverified", export.Value.ManifestRef);
    }

    [Fact]
    public async Task RealExportImportCircle_AtTheThreshold_GrantsPassageAndDropsTheSemanticQualityClaim()
    {
        // Настоящий круг на самой границе: источник имеет честный пройденный заслон ровно на пороге
        // и вместе с ним заявленное смысловое качество.
        var source = new OrchestrationPlanMachine();
        var honest = source.ExportCheckpoint() with
        {
            LastEvaluationGate = Gate(
                "eval-circle-threshold",
                ActiveAccuracyThreshold,
                claimsPassage: true,
                claimsSemanticQuality: true)
        };
        Assert.True(source.ImportCheckpoint(honest).IsSuccess);

        // Обычная выгрузка источника: она несёт точность и подписанные опознанием оценки ссылки.
        var exported = await source.ExportAsync();
        Assert.True(exported.IsSuccess, exported.ErrorMessage);
        Assert.Equal(ActiveAccuracyThreshold, exported.Value!.AccuracyAtExport);

        var target = new OrchestrationPlanMachine();
        var imported = await target.ImportAsync(exported.Value);

        // Пройденность назначена по числам — это право именно этой дороги.
        Assert.True(imported.IsSuccess, imported.ErrorMessage);
        Assert.NotNull(target.LastEvaluationGate);
        Assert.True(target.LastEvaluationGate!.PassesGate);
        Assert.Equal(ActiveAccuracyThreshold, target.LastEvaluationGate.Accuracy);
        // Заявление смыслового качества не переносится и здесь.
        Assert.False(target.LastEvaluationGate.SemanticQualityClaimed);
        // Наружу пройденность видна точностью и подписанной ссылкой.
        var targetExport = await target.ExportAsync();
        Assert.True(targetExport.IsSuccess, targetExport.ErrorMessage);
        Assert.Equal(ActiveAccuracyThreshold, targetExport.Value!.AccuracyAtExport);
        Assert.Contains("eval/eval-circle-threshold", targetExport.Value.ManifestRef);
    }

    // ── Удержание прежних ответов после каждого отказа ──────────────────────────────────────────

    [Fact]
    public void AfterEveryRefusedCheckpoint_ThePreviousAnswersAndRefsAreUnchanged()
    {
        // Прямое предписание реестра: подтвердить прежние ответы после отказа. Проверяется одна и
        // та же обученная машина против всех враждебных контрольных точек подряд — так видно, что
        // ни один отказ не оставил после себя следа и что отказы не портят состояние накопительно.
        var machine = TrainedMachineWithHonestPassedGate(0.90, "eval-honest-live");
        // Снимок берётся по живому состоянию: обычные ответы машины на две подсказки, обе её
        // ссылки, точность экспорта, прежний заслон, размеры обученных хранилищ и обученные пары.
        var before = LiveStateSnapshot(machine);

        // Все враждебные случаи: ниже порога, у самой границы, неизмеренные и вне отрезка.
        var hostileGates = new[]
        {
            Gate("eval-retention-low", 0.10, claimsPassage: true),
            Gate("eval-retention-just-under", 0.749, claimsPassage: true),
            Gate("eval-retention-nan", double.NaN, claimsPassage: true),
            Gate("eval-retention-positive-infinity", double.PositiveInfinity, claimsPassage: true),
            Gate("eval-retention-negative-infinity", double.NegativeInfinity, claimsPassage: true),
            Gate("eval-retention-above-one", 1.5, claimsPassage: true),
            Gate("eval-retention-negative", -0.10, claimsPassage: true)
        };

        foreach (var hostileGate in hostileGates)
        {
            // Враждебная контрольная точка несёт ПУСТОЕ обученное состояние и другую версию:
            // если бы состояние применялось до проверки, снимок изменился бы сразу в нескольких
            // местах — в версии, в ссылках, в размерах хранилищ и в тексте ответа.
            var hostile = ForeignCheckpointWithGate(hostileGate);

            var imported = machine.ImportCheckpoint(hostile);

            // Каждый случай обязан быть отказом с тем же открытым кодом.
            Assert.False(imported.IsSuccess, $"Заслон '{hostileGate.EvaluationId}' обязан быть отвергнут.");
            Assert.Equal(OrchestrationPlanMachine.ImportedGateContradictsEvidenceErrorCode, imported.ErrorCode);
            // И после каждого отказа живое состояние — то же самое.
            Assert.Equal(before, LiveStateSnapshot(machine));
        }

        // Отдельно и прямо: прежний заслон на месте, пройденность не потеряна, обученные пары целы.
        Assert.NotNull(machine.LastEvaluationGate);
        Assert.Equal("eval-honest-live", machine.LastEvaluationGate!.EvaluationId);
        Assert.True(machine.LastEvaluationGate.PassesGate);
        Assert.Equal(0.90, machine.LastEvaluationGate.Accuracy);
        Assert.NotEmpty(machine.TrainedPairIds);
        Assert.True(machine.Mode2KnnStoreSize > 0);
    }

    // ── Оснастка ────────────────────────────────────────────────────────────────────────────────

    // Свидетельство заслона обычной формы: опознание оценки, число строк, точность, заявленная
    // пройденность, человеческая пометка, время и заявление смыслового качества.
    private static OrchestrationPlanEvaluationGate Gate(
        string evaluationId,
        double accuracy,
        bool claimsPassage,
        bool claimsSemanticQuality = false)
        => new(
            evaluationId,
            8,
            accuracy,
            claimsPassage,
            $"stored gate: accuracy={accuracy}, claims_passage={claimsPassage}",
            DateTimeOffset.UtcNow,
            claimsSemanticQuality);

    // Чужая контрольная точка: состояние СВЕЖЕЙ необученной машины (пустые хранилища, начальная
    // версия, нулевой счёт обучения) плюс подставленное свидетельство заслона. Пустота здесь
    // работает как различающий признак для проверки удержания состояния.
    private static OrchestrationPlanMachineCheckpoint ForeignCheckpointWithGate(
        OrchestrationPlanEvaluationGate gate)
        => new OrchestrationPlanMachine().ExportCheckpoint() with { LastEvaluationGate = gate };

    // Передаточная запись обычной формы с подписанными опознанием оценки ссылками: именно такую
    // запись принимает открытый вход загрузки передаточной записи.
    private static LearningMachineExport TransferRecord(double accuracy, string evaluationId)
        => new(
            OrchestrationPlanMachine.MachineId,
            OrchestrationPlanMachine.DomainTag,
            "0.2.0",
            $"{OrchestrationPlanMachine.MachineId}/0.2.0/eval/{evaluationId}/checkpoint",
            $"{OrchestrationPlanMachine.MachineId}/0.2.0/eval/{evaluationId}/manifest.json",
            accuracy,
            DateTimeOffset.UtcNow);

    // Живая машина с непустым обученным состоянием и честным пройденным заслоном. Обучение здесь
    // не измеряется и ничего не доказывает про качество: оно нужно только затем, чтобы у машины
    // были настоящие ответы и непустые хранилища, по которым виден любой след чужой загрузки.
    private static OrchestrationPlanMachine TrainedMachineWithHonestPassedGate(
        double accuracy,
        string evaluationId)
    {
        var machine = new OrchestrationPlanMachine();
        // Две учебные пары обобщающего режима: по ним машина потом отвечает на обычную подсказку.
        var batch = new OrchestrationTrainingBatch(
            "opm-gate-acceptance-batch",
            [
                TrainingPair("opm-gate-pair-01", "FLOW-33", PlanPrompt),
                TrainingPair("opm-gate-pair-02", "FLOW-34", "продолжить реализацию потока")
            ]);
        var trained = machine.TrainOrchestrationBatchAsync(batch).GetAwaiter().GetResult();
        Assert.True(trained.IsSuccess, trained.ErrorMessage);
        // Обучение сбрасывает прежний заслон, поэтому честный пройденный заслон ставится после
        // обучения — своей же собственной контрольной точкой, чтобы обученные пары остались целы.
        var honest = machine.ExportCheckpoint() with
        {
            LastEvaluationGate = Gate(evaluationId, accuracy, claimsPassage: true)
        };
        var imported = machine.ImportCheckpoint(honest);
        Assert.True(imported.IsSuccess, imported.ErrorMessage);
        return machine;
    }

    // Учебная пара обобщающего режима настоящей формы домена, без выдуманных полей.
    private static OrchestrationTrainingPair TrainingPair(string pairId, string flowRef, string prompt)
        => new(
            pairId,
            PairMode.Mode2Generalization,
            prompt,
            new OrchestrationPlanChosen(
                flowRef,
                $"План реализации потока {flowRef}",
                $"docs/flows/{flowRef}.md",
                $"ТЕЛО ПЛАНА {flowRef}"),
            "Отклонённый план пропускает проверку результата.",
            GenerationRouteMode.Mode2Generalization,
            BoundarySource.Classifier,
            true,
            IntentKind.ImplementationExecutionRequest,
            flowRef,
            $"opm-gate-bundle-{flowRef}",
            false);

    // Снимок всего, что видно снаружи у живой машины: сами ответы на две подсказки, версия, обе
    // ссылки, точность экспорта, прежний заслон, счёт обучения, размеры обученных хранилищ и
    // список обученных пар. Этого достаточно, чтобы увидеть ЛЮБОЕ изменение живого состояния,
    // а не только изменение ссылок.
    private static string LiveStateSnapshot(OrchestrationPlanMachine machine)
    {
        // Обычные ответы машины: именно они и есть «прежние ответы», которые обязаны уцелеть.
        var knownAnswer = PlanTextFor(machine, PlanPrompt);
        var unseenAnswer = PlanTextFor(machine, UnseenPrompt);
        // Обычная выгрузка: версия, обе ссылки и точность экспорта.
        var export = machine.ExportAsync().GetAwaiter().GetResult();
        Assert.True(export.IsSuccess, export.ErrorMessage);
        var exported = export.Value!;
        // Прежний заслон записывается словами, чтобы в сравнении было видно, что именно поменялось.
        var gate = machine.LastEvaluationGate is null
            ? "заслона нет"
            : string.Join(
                ";",
                machine.LastEvaluationGate.EvaluationId,
                Number(machine.LastEvaluationGate.Accuracy),
                machine.LastEvaluationGate.PassesGate,
                machine.LastEvaluationGate.SemanticQualityClaimed);
        // Всё вместе — одна строка: сравнение строк даёт понятную разницу в отчёте о падении.
        return string.Join(
            "|",
            exported.Version,
            exported.CheckpointRef,
            exported.ManifestRef,
            Number(exported.AccuracyAtExport),
            gate,
            Number(machine.TrainingProgressScore),
            machine.Mode1FlowGroundingKnnSize,
            machine.Mode2KnnStoreSize,
            machine.Mode3KnnStoreSize,
            string.Join(",", machine.TrainedPairIds.Order(StringComparer.Ordinal)),
            knownAnswer,
            unseenAnswer);
    }

    // Обычный ответ машины на подсказку через её открытый вход построения плана.
    private static string PlanTextFor(OrchestrationPlanMachine machine, string prompt)
    {
        var generated = machine.GeneratePlanAsync(prompt).GetAwaiter().GetResult();
        Assert.True(generated.IsSuccess, generated.ErrorMessage);
        return generated.Value!.PlanText;
    }

    // Запись числа в снимке не зависит от культуры среды: иначе сравнение «до и после» могло бы
    // разойтись из-за запятой вместо точки, а не из-за поведения продукта.
    private static string Number(double value)
        => value.ToString("R", System.Globalization.CultureInfo.InvariantCulture);

    // Временный каталог для файловой дороги создаётся ВНЕ хранилища проекта и удаляется в том же
    // прогоне: следов в рабочем каталоге не остаётся.
    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "opm-gate-load-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    // Удаление временного каталога вместе с содержимым; отсутствие каталога проверяется здесь же.
    private static void DeleteTemporaryDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
        Assert.False(Directory.Exists(path));
    }
}
