// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Text.Json;
using LlmFromScratch.Tests;
using TensorCore.Layers;

// Приёмка переноса «LogSoftmax быстрее работает по последней оси» (строка очереди
// под восьмым номером раздела работ, строка 217 реестра исправлений по опытам;
// блок измерений строки 674–688; карточка переноса сокращения строки нормы,
// разделы про восстановление обученного старта и про учебную проверку).
//
// Что здесь доказывается человеческими словами. Нормализация длины вектора
// складывает числа по последней оси. Прежний код для КАЖДОГО слагаемого собирал
// массив координат и переводил его обратно в плоский номер. Но при сохранённой
// последней оси нужные числа уже лежат в памяти подряд: строка выходной клетки
// начинается с её номера, умноженного на длину строки. Новый узкий путь читает
// эту строку напрямую. Порядок сложения прежний, поэтому сумма обязана выйти
// бит в бит той же; убраны только вычисления адресов.
//
// Что закрывает именно эта приёмка, и что НЕ закрывает. Строка очереди называет
// один пробел: восстановление назначенного обученного начала и полного состояния
// и четыре настоящих учебных обновления на нём. Поэтому здесь нет проверок
// границ ветви без сохранения оси, нет проверки отдельной границы обратного
// распространения и нет ресурсной панели — это другие части карточки.
//
// Откуда независимый эталон, и почему он независим. Донор семейства записал
// показания своей панели, и его собственная пометка ветви говорит: «исходная
// семантика плеча A; непрерывное суммирование последней оси выключено». Опыт
// записал два плеча: в плече A фактор при обучении был выключен, в плече B
// включён. Текущий продукт идёт узким путём ВСЕГДА, в том числе на
// диагностических чтениях. Значит совпадение чисел — это сравнение двух разных
// путей на одном обученном состоянии, а эталон взят из ранее сохранённых
// записей, а не из этого прогона.
//
// Осторожность, без которой приёмка была бы ложной. Веса и состояние
// оптимизатора назначенного донора побайтово совпадают с донором другого
// семейства: оба выращены из одних зёрен на одних данных при нулевом отсеве.
// Проверка, опирающаяся только на веса, прошла бы и с чужим донором. Поэтому
// ниже отдельно закреплены три признака, которые делают донор назначенным:
// отпечаток файла случайного состояния, область случайного потока адаптеров и
// отпечатки опознавательных строк. И отдельно доказано, что значения чужого
// семейства проверка отвергает.
//
// Расход на руку: четыре настоящих учебных обновления, четыре учебных прохода
// вперёд и четыре назад внутри них, восемь диагностических чтений. Порождения
// текста, предпочтений и внешних вызовов нет.
public sealed class RmsRowReductionLearningTests
{
    // Журнал прогона: измеренные расхождения и расписки попадают в вывод
    // проверки, а не остаются только в голове исполнителя.
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    public RmsRowReductionLearningTests(Xunit.Abstractions.ITestOutputHelper output) => _output = output;

    [Fact]
    public void RestoredSft_PreservesLearningState()
    {
        // ── Шаг 1. Паспорт, байты донора и расписка о загруженных сборках ─────

        using var manifestDocument = RmsRowStateFixture.OpenManifest();
        var manifest = manifestDocument.RootElement;

        // Отпечатки донора сверяются ДО любого сравнения чисел: подмена
        // обученного старта обязана быть видна раньше, чем панель.
        var donor = RmsRowStateFixture.VerifyDonorAndGetPath(manifest);
        Assert.True(
            manifest.GetProperty("whole_file_sha256").EnumerateObject().Count() >= 8,
            "RMS_ROW_PINNED_FILE_SET_TOO_SMALL");

        // Какие именно байты продукта считали этот результат. Эта расписка —
        // то, что отличает доказанную чувствительность от заявленной.
        _output.WriteLine("Загруженные сборки продукта:");
        _output.WriteLine(RmsRowStateFixture.NativeAssemblyReceipt());

        // Донор объявил себя завершённым и квалифицированным по обучению.
        using var bundleDocument = JsonDocument.Parse(
            File.ReadAllBytes(Path.Combine(donor, "bundle.json")));
        var bundle = bundleDocument.RootElement;
        Assert.Equal("COMPLETE", bundle.GetProperty("status").GetString());
        Assert.True(bundle.GetProperty("learning_qualified").GetBoolean());
        Assert.Equal(
            manifest.GetProperty("restored_state_ru").GetProperty("optimizer_step").GetInt32(),
            bundle.GetProperty("optimizerStep").GetInt32());
        Assert.Equal("h64-rms-row", bundle.GetProperty("family_id").GetString());

        using var preparationDocument = JsonDocument.Parse(
            File.ReadAllBytes(Path.Combine(donor, "preparation.json")));
        var preparation = preparationDocument.RootElement;

        // Показания донора сняты при ВЫКЛЮЧЕННОМ проверяемом факторе. Без этого
        // сравнение было бы сравнением пути с самим собой и ничего не доказывало.
        using var sourceManifestDocument = JsonDocument.Parse(
            File.ReadAllBytes(Path.Combine(donor, "source-manifest.json")));
        Assert.True(sourceManifestDocument.RootElement
            .GetProperty("generatedProducer")
            .GetProperty("factorDisabledDuringPreparation")
            .GetBoolean());
        var goldDescription = manifest.GetProperty("independent_gold_ru");
        Assert.True(goldDescription.GetProperty("factor_disabled_during_preparation").GetBoolean());
        Assert.Equal(
            goldDescription.GetProperty("donor_branch_note").GetString(),
            preparation.GetProperty("branch").GetString());

        // ── Шаг 2. Назначенный донор, а не случайный с теми же весами ─────────

        var identity = manifest.GetProperty("identity_literals_ru");
        var topologySha = RmsRowStateFixture.Sha256Text(
            identity.GetProperty("topology_literal").GetString()!);
        var loraSha = RmsRowStateFixture.Sha256Text(
            identity.GetProperty("lora_literal").GetString()!);

        // Признак первый: отпечатки объявленных опознавательных строк.
        Assert.Equal(identity.GetProperty("expected_topology_sha256").GetString(), topologySha);
        Assert.Equal(identity.GetProperty("expected_lora_configuration_sha256").GetString(), loraSha);

        // Признак второй: отпечаток самого файла случайного состояния.
        var rngFileSha = RmsRowStateFixture.Sha256Hex(
            File.ReadAllBytes(Path.Combine(donor, "S0.rng.json")));
        Assert.Equal(
            manifest.GetProperty("whole_file_sha256").GetProperty("S0.rng.json").GetString(),
            rngFileSha);

        // Признак третий: область случайного потока адаптеров, объявленная донором.
        using var generationDocument = JsonDocument.Parse(
            File.ReadAllBytes(Path.Combine(donor, "generation.json")));
        var declaredStream = manifest.GetProperty("seeds_ru").GetProperty("stream_domain").GetString();
        Assert.Equal(declaredStream, generationDocument.RootElement.GetProperty("stream").GetString());

        // И доказательство, что проверка именно ОТВЕРГАЕТ чужое семейство, а не
        // просто принимает своё: все три признака обязаны отличаться от его
        // записанных значений. Веса при этом совпадают побайтово, поэтому без
        // этих трёх строк подмена донора прошла бы молча.
        var otherFamily = manifest.GetProperty("donor_is_not_distinguished_by_weights_ru")
            .GetProperty("other_family_marks");
        Assert.NotEqual(otherFamily.GetProperty("rng_file_sha256").GetString(), rngFileSha);
        Assert.NotEqual(otherFamily.GetProperty("stream_domain").GetString(), declaredStream);
        Assert.NotEqual(otherFamily.GetProperty("topology_sha256").GetString(), topologySha);
        Assert.NotEqual(otherFamily.GetProperty("lora_configuration_sha256").GetString(), loraSha);

        // ── Шаг 3. Восстановление обученного начала и полного состояния ───────

        var start = RmsRowStateFixture.Restore(manifest, donor);
        var restored = manifest.GetProperty("restored_state_ru");

        // Разделение обучаемого и замороженного состояния то же, что у опыта.
        Assert.Equal(restored.GetProperty("trainable_arrays").GetInt32(), start.Trainable.Count);
        Assert.Equal(restored.GetProperty("frozen_arrays").GetInt32(), start.Frozen.Count);
        Assert.Equal(restored.GetProperty("trainable_arrays").GetInt32(), start.Bindings.Count);

        // Восстановлено именно то обученное состояние, с которого начинался опыт:
        // отпечатки обучаемых и замороженных массивов считаются тем же правилом
        // и совпадают с записью опыта ДО его четырёх обновлений.
        var trainableBefore = RmsRowStateFixture.ParametersHash(start.Trainable);
        var frozenBefore = RmsRowStateFixture.ParametersHash(start.Frozen);
        Assert.Equal(restored.GetProperty("trainable_state_sha256").GetString(), trainableBefore);
        Assert.Equal(restored.GetProperty("frozen_state_sha256").GetString(), frozenBefore);

        // Полное состояние оптимизатора действительно ввезено, а не создано
        // заново: обратная выгрузка совпадает по шагу, набору связок, формам и
        // обоим накопленным моментам. Иначе «восстановлено» значило бы только
        // «не упало».
        var exported = start.Optimizer.ExportPortableState(start.Bindings);
        Assert.Equal(start.DonorOptimizerState.Step, exported.Step);
        Assert.Equal(
            start.DonorOptimizerState.Entries.Select(entry => entry.Key),
            exported.Entries.Select(entry => entry.Key));
        foreach (var expected in start.DonorOptimizerState.Entries)
        {
            var actual = exported.Entries.Single(entry => entry.Key == expected.Key);
            Assert.Equal(expected.Dimensions, actual.Dimensions);
            Assert.Equal(expected.FirstMoment, actual.FirstMoment);
            Assert.Equal(expected.SecondMoment, actual.SecondMoment);
        }

        // Состояние генератора случайных чисел восстановлено и сверено по полям
        // общим договором проекта, а не через текстовую запись: строка скрыла бы
        // расхождение курсора потока.
        Assert.Equal(
            restored.GetProperty("random_state_step").GetInt32(),
            start.DonorRandomState.RandomStateStep);
        Assert.Equal(topologySha, start.DonorRandomState.TopologySha256);
        Assert.Equal(loraSha, start.DonorRandomState.LoraConfigurationSha256);
        RandomStateFieldAssertions.AssertEqual(
            start.DonorRandomState,
            start.Model.CaptureRandomState(
                topologySha, loraSha, start.DonorRandomState.RandomStateStep));

        // ── Шаг 4. Эталон: два плеча опыта и их согласие между собой ──────────

        using var armADocument = RmsRowStateFixture.OpenSealedArm(manifest, "A");
        using var armBDocument = RmsRowStateFixture.OpenSealedArm(manifest, "B");
        var armA = armADocument.RootElement;
        var armB = armBDocument.RootElement;

        // Плечи отличаются ровно одним: включён ли проверяемый фактор при обучении.
        Assert.False(armA.GetProperty("mechanism")
            .GetProperty("contiguous_last_axis_sum_enabled").GetBoolean());
        Assert.True(armB.GetProperty("mechanism")
            .GetProperty("contiguous_last_axis_sum_enabled").GetBoolean());
        Assert.Equal(
            goldDescription.GetProperty("arms").GetProperty("A")
                .GetProperty("contiguous_last_axis_sum_enabled").GetBoolean(),
            armA.GetProperty("mechanism").GetProperty("contiguous_last_axis_sum_enabled").GetBoolean());
        Assert.Equal(
            goldDescription.GetProperty("arms").GetProperty("B")
                .GetProperty("contiguous_last_axis_sum_enabled").GetBoolean(),
            armB.GetProperty("mechanism").GetProperty("contiguous_last_axis_sum_enabled").GetBoolean());

        // Эталон не однорукий: оба плеча согласны по значениям, поэтому
        // сравнение ниже не зависит от того, какое плечо взято за эталон.
        var trainBeforeField = goldDescription.GetProperty("train_panel_before_field").GetString()!;
        var trainAfterField = goldDescription.GetProperty("train_panel_after_field").GetString()!;
        var examBeforeField = goldDescription.GetProperty("exam_panel_before_field").GetString()!;
        var examAfterField = goldDescription.GetProperty("exam_panel_after_field").GetString()!;

        foreach (var field in new[] { trainBeforeField, trainAfterField, examBeforeField, examAfterField })
        {
            var fromA = RmsRowStateFixture.RecordedPanel(armA, field);
            var fromB = RmsRowStateFixture.RecordedPanel(armB, field);
            Assert.Equal(fromA.Keys.OrderBy(key => key, StringComparer.Ordinal), fromB.Keys.OrderBy(key => key, StringComparer.Ordinal));
            foreach (var key in fromA.Keys) AssertSameReadout(fromA[key], fromB[key], $"плечи A и B, панель {field}, строка «{key}»");
        }

        var goldLosses = armA.GetProperty("training_losses").EnumerateArray()
            .Select(value => value.GetSingle()).ToArray();
        var lossesFromB = armB.GetProperty("training_losses").EnumerateArray()
            .Select(value => value.GetSingle()).ToArray();
        Assert.Equal(
            goldLosses.Select(BitConverter.SingleToInt32Bits),
            lossesFromB.Select(BitConverter.SingleToInt32Bits));

        // Записанные в паспорте потери сверяются с запечатанным плечом: рукописная
        // правка паспорта изменила бы эталон молча, и это обязано быть видно.
        Assert.Equal(
            goldLosses.Select(BitConverter.SingleToInt32Bits),
            manifest.GetProperty("training_ru").GetProperty("online_losses").EnumerateArray()
                .Select(value => BitConverter.SingleToInt32Bits(value.GetSingle())));

        // ── Шаг 4А. Сопоставление восстановленного состояния ПО ИМЕНАМ ────────
        //
        // Зачем это отдельно от отпечатков всего набора выше. Те отпечатки
        // считаются в порядке обхода модели и не говорят, КАКОЕ имя несёт какие
        // значения: при переименовании или перестановке массивов они либо молча
        // краснеют без указания виновного, либо вообще не различают, какой из
        // массивов подменён. Независимая запись донора перечисляет свои
        // обучаемые массивы по именам, её байты закреплены паспортом и уже
        // сверены выше, поэтому сверка идёт по именам, а не по порядку обхода.
        using var authorityDocument = JsonDocument.Parse(
            File.ReadAllBytes(Path.Combine(donor, "authority.json")));
        var authority = authorityDocument.RootElement;

        // Имена, которые донор объявил своими обучаемыми массивами.
        var declaredNames = authority.GetProperty("namedBindings").EnumerateArray()
            .Select(value => value.GetString()!)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        // Имена берутся из настоящих записей контрольной точки продукта — тем же
        // путём, которым продукт сохраняет и загружает модель, а не из порядка
        // перечисления параметров.
        var checkpointEntries = start.Model.GetCheckpointParameterEntries();
        var trainableByName = checkpointEntries
            .Where(entry => entry.Parameter.Value.RequiresGrad)
            .ToDictionary(entry => entry.Key, entry => entry.Parameter, StringComparer.Ordinal);
        var frozenByName = checkpointEntries
            .Where(entry => !entry.Parameter.Value.RequiresGrad)
            .ToDictionary(entry => entry.Key, entry => entry.Parameter, StringComparer.Ordinal);

        // Набор обучаемых имён совпадает с независимой записью донора целиком:
        // ни одного лишнего и ни одного пропущенного.
        Assert.Equal(
            declaredNames,
            trainableByName.Keys.OrderBy(name => name, StringComparer.Ordinal).ToArray());

        // Замороженные массивы не носят имён обучаемых. Без этого утверждения
        // «не изменилось» и «изменилось» ниже могли бы относиться к одному и
        // тому же массиву.
        Assert.Empty(frozenByName.Keys
            .Where(name => declaredNames.Contains(name, StringComparer.Ordinal)));
        Assert.Equal(restored.GetProperty("frozen_arrays").GetInt32(), frozenByName.Count);

        // Форма каждого обучаемого массива совпадает с формой, записанной под
        // ТЕМ ЖЕ именем в файле состояния оптимизатора донора. Это отдельный
        // файл донора, а не проверяемый объект и не продуктовый сериализатор.
        foreach (var donorEntry in start.DonorOptimizerState.Entries)
        {
            Assert.True(
                trainableByName.ContainsKey(donorEntry.Key),
                "RMS_ROW_DONOR_NAMED_ARRAY_NOT_RESTORED:" + donorEntry.Key);
            Assert.Equal(
                donorEntry.Dimensions,
                trainableByName[donorEntry.Key].Value.Shape.Dimensions);
        }

        // И значения: отпечаток обучаемых массивов, взятых В ПОРЯДКЕ ИМЁН,
        // совпадает с отпечатком, который записало запечатанное плечо опыта до
        // своих четырёх обновлений. Эталон взят из запечатанного файла опыта, а
        // не из паспорта, не из проверяемого объекта и не из продуктового
        // сериализатора. Оба отпечатка печатаются в журнал, поэтому расхождение
        // порядка имён и порядка обхода будет видно, а не спрятано.
        var trainableByNameHash = RmsRowStateFixture.ParametersHash(
            trainableByName.OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => pair.Value));
        _output.WriteLine(
            $"Отпечаток обучаемого состояния до обучения: в порядке имён "
            + $"{trainableByNameHash}, в порядке обхода модели {trainableBefore}.");
        Assert.Equal(armA.GetProperty("trainable_before_sha256").GetString(), trainableByNameHash);

        // Отпечаток КАЖДОГО массива по отдельности, снятый до обучения. Без него
        // утверждение «обучаемое состояние изменилось» опирается на один
        // отпечаток всего набора и прошло бы даже тогда, когда обновился только
        // один адаптерный массив из четырёх.
        var trainableHashByNameBefore = trainableByName.ToDictionary(
            pair => pair.Key,
            pair => RmsRowStateFixture.ParametersHash(new[] { pair.Value }),
            StringComparer.Ordinal);
        var frozenHashByNameBefore = frozenByName.ToDictionary(
            pair => pair.Key,
            pair => RmsRowStateFixture.ParametersHash(new[] { pair.Value }),
            StringComparer.Ordinal);

        // ── Шаг 5. Показания ДО обучения через узкий путь ─────────────────────

        var rows = manifest.GetProperty("panel_rows_ru");
        var panelIds = new[] { "T1", "T2", "E1", "E2" };
        var diagnosticForwards = 0;

        var before = new Dictionary<string, RmsRowReadout>(StringComparer.Ordinal);
        foreach (var id in panelIds)
        {
            var row = rows.GetProperty(id);
            before[id] = RmsRowStateFixture.Observe(
                start.Model,
                row.GetProperty("prompt").GetString()!,
                row.GetProperty("answer").GetString()!);
            diagnosticForwards++;
        }

        // Главное утверждение первой половины: на восстановленном обученном
        // состоянии узкий путь даёт те же числа, что записало плечо с выключенным
        // фактором.
        var goldTrainBefore = RmsRowStateFixture.RecordedPanel(armA, trainBeforeField);
        var goldExamBefore = RmsRowStateFixture.RecordedPanel(armA, examBeforeField);
        AssertMatchesRecorded(before, goldTrainBefore, goldExamBefore, rows, panelIds, "до обучения");

        // И второе независимое свидетельство того же состояния: панель самого
        // донора. Два разных сохранённых источника согласны с наблюдённым.
        var donorStartPanel = RmsRowStateFixture.RecordedPanel(
            preparation, goldDescription.GetProperty("donor_gold_at_start_field").GetString()!);
        foreach (var id in new[] { "T1", "T2" })
        {
            var prompt = rows.GetProperty(id).GetProperty("prompt").GetString()!;
            AssertSameReadout(donorStartPanel[prompt], before[id], $"панель донора, строка «{id}»");
        }

        // ── Шаг 6. Четыре настоящих учебных обновления ───────────────────────

        var order = manifest.GetProperty("training_ru").GetProperty("call_order")
            .EnumerateArray().Select(value => value.GetString()!).ToArray();
        Assert.Equal(new[] { "T1", "T2", "T1", "T2" }, order);

        var weight = manifest.GetProperty("training_ru").GetProperty("weight").GetSingle();
        var observedLosses = new float[start.TrainSequences.Count];
        for (var step = 0; step < start.TrainSequences.Count; step++)
        {
            observedLosses[step] = start.Trainer.TrainSupervisedNextTokenStep(
                start.Model, start.Optimizer, start.TrainSequences[step], weight);
        }

        // Потери каждого вызова — это потеря ДО его собственного обновления, и
        // сверяются они побитово, а не допуском.
        Assert.Equal(goldLosses.Length, observedLosses.Length);
        for (var step = 0; step < goldLosses.Length; step++)
        {
            _output.WriteLine(
                $"обновление {step + 1}: потеря опыта {goldLosses[step]:R}, наблюдённая "
                + $"{observedLosses[step]:R}, расхождение {observedLosses[step] - goldLosses[step]:R}");
            Assert.Equal(
                BitConverter.SingleToInt32Bits(goldLosses[step]),
                BitConverter.SingleToInt32Bits(observedLosses[step]));
        }

        // Обновления настоящие: счётчик шагов оптимизатора прошёл от
        // четырёхсот к четырёмстам четырём.
        var afterTraining = start.Optimizer.ExportPortableState(start.Bindings);
        Assert.Equal(
            manifest.GetProperty("training_ru").GetProperty("optimizer_step_before").GetInt32(),
            start.DonorOptimizerState.Step);
        Assert.Equal(
            manifest.GetProperty("training_ru").GetProperty("optimizer_step_after").GetInt32(),
            afterTraining.Step);

        // ── Шаг 7. Показания ПОСЛЕ обучения ─────────────────────────────────

        var after = new Dictionary<string, RmsRowReadout>(StringComparer.Ordinal);
        foreach (var id in panelIds)
        {
            var row = rows.GetProperty(id);
            after[id] = RmsRowStateFixture.Observe(
                start.Model,
                row.GetProperty("prompt").GetString()!,
                row.GetProperty("answer").GetString()!);
            diagnosticForwards++;
        }

        var goldTrainAfter = RmsRowStateFixture.RecordedPanel(armA, trainAfterField);
        var goldExamAfter = RmsRowStateFixture.RecordedPanel(armA, examAfterField);
        AssertMatchesRecorded(after, goldTrainAfter, goldExamAfter, rows, panelIds, "после обучения");

        // ── Шаг 8. Что изменилось, а что обязано было остаться ───────────────

        // Замороженное состояние не тронуто, обучаемое изменилось. Оба факта
        // проверяются отпечатками ЭТОГО прогона, а не сверкой со старым
        // двоичным отпечатком: подгонять свежий прогон под старый отпечаток
        // запрещено.
        var trainableAfter = RmsRowStateFixture.ParametersHash(start.Trainable);
        var frozenAfter = RmsRowStateFixture.ParametersHash(start.Frozen);
        Assert.Equal(frozenBefore, frozenAfter);
        Assert.NotEqual(trainableBefore, trainableAfter);

        // То же самое, но ПО ИМЕНАМ: изменился КАЖДЫЙ из четырёх названных
        // адаптерных массивов, и не изменился НИ ОДИН названный замороженный.
        // Отпечаток всего набора выше этого различить не может: ему достаточно
        // одного изменившегося массива, поэтому обновление одного адаптера из
        // четырёх прошло бы молча.
        foreach (var pair in trainableByName.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            var trainedHash = RmsRowStateFixture.ParametersHash(new[] { pair.Value });
            _output.WriteLine(
                $"обучаемый массив «{pair.Key}»: до обучения "
                + $"{trainableHashByNameBefore[pair.Key]}, после {trainedHash}");
            Assert.NotEqual(trainableHashByNameBefore[pair.Key], trainedHash);
        }

        foreach (var pair in frozenByName.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            Assert.Equal(
                frozenHashByNameBefore[pair.Key],
                RmsRowStateFixture.ParametersHash(new[] { pair.Value }));
        }

        // Сравнение с историческим отпечатком печатается как сведение о
        // происхождении и условием прохождения не является.
        var provenance = manifest.GetProperty("provenance_only_ru")
            .GetProperty("trainable_after_sha256").GetString();
        _output.WriteLine(
            "Происхождение, не условие прохождения: исторический отпечаток обучаемого "
            + $"состояния после обучения {provenance}, наблюдённый {trainableAfter}, "
            + $"совпадение {(provenance == trainableAfter ? "да" : "нет")}.");

        // ── Шаг 9. Защита от пустой проверки ────────────────────────────────

        // Восстановлено именно ОБУЧЕННОЕ состояние, а не чистая модель: до
        // четырёхсот обновлений донора первая учебная строка была неверна и
        // правильный ответ стоял далеко, после них она верна и первая.
        var donorFreshPanel = RmsRowStateFixture.RecordedPanel(
            preparation,
            goldDescription.GetProperty("donor_gold_before_donor_training_field").GetString()!);
        var t1Prompt = rows.GetProperty("T1").GetProperty("prompt").GetString()!;
        Assert.False(donorFreshPanel[t1Prompt].Exact);
        Assert.True(donorStartPanel[t1Prompt].Exact);
        Assert.True(donorFreshPanel[t1Prompt].GoldRank > donorStartPanel[t1Prompt].GoldRank);
        Assert.Equal(1, before["T1"].GoldRank);
        Assert.True(before["T1"].Exact);

        // И наблюдённые числа отличаются от необученных: если бы веса не
        // загрузились, панель совпала бы с состоянием до обучения донора.
        Assert.NotEqual(donorFreshPanel[t1Prompt].LastLogitsBase64, before["T1"].LastLogitsBase64);

        // Обучение действительно что-то изменило в ответах, а не только в весах.
        Assert.NotEqual(before["T1"].LastLogitsBase64, after["T1"].LastLogitsBase64);

        // ── Шаг 10. Никакого завышения результата ───────────────────────────

        // Приобретённая строка сохранена, но правило знака не освоено: вторая
        // учебная строка неверна и до, и после, учебная доля остаётся одна из
        // двух, экзаменационная тоже одна из двух. Это равно простому
        // постоянному ответу, и успехом по качеству не является.
        Assert.False(before["T2"].Exact);
        Assert.False(after["T2"].Exact);
        Assert.Equal(1, new[] { before["T1"], before["T2"] }.Count(row => row.Exact));
        Assert.Equal(1, new[] { after["T1"], after["T2"] }.Count(row => row.Exact));
        Assert.Equal(1, new[] { before["E1"], before["E2"] }.Count(row => row.Exact));
        Assert.Equal(1, new[] { after["E1"], after["E2"] }.Count(row => row.Exact));

        // Приобретённые донором строки — ровно те, что он объявил.
        Assert.Equal(
            goldDescription.GetProperty("acquired_ids").EnumerateArray()
                .Select(value => value.GetString()).ToArray(),
            preparation.GetProperty("acquired_ids").EnumerateArray()
                .Select(value => value.GetString()).ToArray());

        // ── Шаг 11. Учёт расхода ────────────────────────────────────────────

        // Расход совпал с объявленным: четыре обновления и восемь чтений, ни
        // одного лишнего прохода и ни одного порождения текста.
        var budget = manifest.GetProperty("budget_ru");
        Assert.Equal(budget.GetProperty("diagnostic_forward").GetInt32(), diagnosticForwards);
        Assert.Equal(budget.GetProperty("optimizer_updates").GetInt32(), observedLosses.Length);
        Assert.Equal(budget.GetProperty("training_forward").GetInt32(), start.TrainSequences.Count);
        Assert.Equal(0, budget.GetProperty("generate").GetInt32());
        Assert.Equal(0, budget.GetProperty("dpo_steps").GetInt32());
        Assert.Equal(0, budget.GetProperty("external_teacher_calls").GetInt32());

        _output.WriteLine(
            $"Расход прогона: учебных обновлений {observedLosses.Length}, "
            + $"диагностических чтений {diagnosticForwards}, порождений текста 0.");
    }

    // ── Помощники сравнения ─────────────────────────────────────────────────

    // Наблюдённая панель против записанной: учебные строки берутся из учебной
    // панели записи, экзаменационные — из экзаменационной.
    private void AssertMatchesRecorded(
        IReadOnlyDictionary<string, RmsRowReadout> observed,
        IReadOnlyDictionary<string, RmsRowReadout> recordedTrain,
        IReadOnlyDictionary<string, RmsRowReadout> recordedExam,
        JsonElement rows,
        IReadOnlyList<string> panelIds,
        string stage)
    {
        foreach (var id in panelIds)
        {
            var row = rows.GetProperty(id);
            var prompt = row.GetProperty("prompt").GetString()!;
            var split = row.GetProperty("split").GetString()!;
            var source = split == "TRAIN" ? recordedTrain : recordedExam;

            Assert.True(source.ContainsKey(prompt), $"RMS_ROW_RECORDED_ROW_NOT_FOUND:{stage}:{id}");
            AssertSameReadout(source[prompt], observed[id], $"{stage}, строка «{id}»");
        }
    }

    // Одно показание против другого: сначала логиты, потом всё остальное.
    // Порядок здесь часть доказательства. Если когда-нибудь разойдётся мера
    // ошибки ниже, логиты к этому моменту уже доказаны одинаковыми, и причина
    // будет в арифметике экзаменатора, а не в проверяемом суммировании.
    private void AssertSameReadout(RmsRowReadout expected, RmsRowReadout actual, string what)
    {
        Assert.Equal(expected.LastLogitsBase64, actual.LastLogitsBase64);
        Assert.Equal(expected.GoldId, actual.GoldId);
        Assert.Equal(expected.PredictedId, actual.PredictedId);
        Assert.Equal(expected.Exact, actual.Exact);
        Assert.Equal(expected.GoldRank, actual.GoldRank);
        Assert.Equal(expected.ArgmaxOrderRank, actual.ArgmaxOrderRank);

        // Мера ошибки сверяется ПОБИТОВО по двойной точности, а не допуском: при
        // допуске отказ можно спрятать, ослабив допуск, при сравнении
        // представлений — нельзя. Фактическое расхождение печатается в журнал.
        var expectedBits = BitConverter.DoubleToInt64Bits(expected.Nll);
        var actualBits = BitConverter.DoubleToInt64Bits(actual.Nll);
        _output.WriteLine(
            $"{what}: мера ошибки записи {expected.Nll:R}, наблюдённая {actual.Nll:R}, "
            + $"расхождение {actual.Nll - expected.Nll:R}, разница представлений {actualBits - expectedBits}");
        Assert.Equal(expectedBits, actualBits);
    }
}
