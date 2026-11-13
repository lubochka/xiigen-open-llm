// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests.ExperimentFixes;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LoRA;
using TensorCore.Core;
using TensorCore.Init;
using TensorCore.Layers;
using TrainingLoop;
using TrainingLoop.Optimizers;
using TransformerModel;
using TransformerModel.Config;

// Общая оснастка приёмки переноса «LogSoftmax быстрее работает по последней оси».
//
// Здесь находится только то, что названо карточкой переноса как общее: загрузчик
// обученного начального состояния, чтение сохранённых данных и учёт расхода.
// Копий проверяемого ядра и копий обучателя здесь нет и быть не должно: приёмка
// обязана ходить тем же продуктовым путём, что и обычный вызывающий.
//
// Два приёма, которые задают устройство этого файла.
// Первый: обученные веса не копируются в проверки. Паспорт ссылается на один
// общий неизменяемый обученный старт и закрепляет его байты отпечатками, которые
// сверяются ДО любого сравнения чисел. Поэтому молчаливая подмена донора видна, а
// хранилище не растёт копиями весов по триста килобайт на каждую проверку.
// Второй: сравнение идёт побитово, а не допуском. При допуске расхождение можно
// спрятать, ослабив допуск; при сравнении представлений чисел — нельзя.

// Одно прочитанное показание панели. Поля повторяют те, что записали донор и
// сохранённые результаты опыта, поэтому сравнение идёт полем в поле.
internal sealed record RmsRowReadout(
    string Prompt,
    int GoldId,
    int PredictedId,
    bool Exact,
    double Nll,
    int GoldRank,
    int ArgmaxOrderRank,
    string LastLogitsBase64);

// Восстановленное обученное начало вместе со всем, что нужно для его проверки.
internal sealed record RmsRowRestoredStart(
    LlmModel Model,
    IReadOnlyList<AdamWParameterBinding> Bindings,
    AdamW Optimizer,
    Trainer Trainer,
    AdamWPortableState DonorOptimizerState,
    LlmModelRandomStateSnapshot DonorRandomState,
    string TopologySha256,
    string LoraConfigurationSha256,
    IReadOnlyList<Parameter> Trainable,
    IReadOnlyList<Parameter> Frozen,
    IReadOnlyList<int[]> TrainSequences);

internal static class RmsRowStateFixture
{
    // Опознавательное имя паспорта этой приёмки.
    public const string FixtureId = "RmsRowReduce01";

    // ── Паспорт и байты донора ──────────────────────────────────────────────

    // Паспорт лежит в хранилище, а не в папке сборки: путь не зависит от того,
    // откуда запущен прогон.
    public static JsonDocument OpenManifest()
    {
        var path = Path.Combine(
            RepositoryRoot(), "Tests", "Fixtures", "ExperimentFixes", "v1", FixtureId, "manifest.json");
        if (!File.Exists(path))
            throw new FileNotFoundException("RMS_ROW_FIXTURE_MANIFEST_NOT_FOUND:" + path, path);
        return JsonDocument.Parse(File.ReadAllBytes(path));
    }

    // Донор остаётся на своём месте: проверка читает его, но не дублирует.
    // Несовпадение отпечатка, отсутствующий файл или указатель вместо весов —
    // это конкретный дефект постановки до первого прохода модели, а не повод
    // подобрать другую модель.
    public static string VerifyDonorAndGetPath(JsonElement manifest)
    {
        var donor = Path.Combine(
            RepositoryRoot(),
            manifest.GetProperty("donor_relative_path").GetString()!
                .Replace('/', Path.DirectorySeparatorChar));
        if (!Directory.Exists(donor))
            throw new DirectoryNotFoundException("RMS_ROW_DONOR_DIRECTORY_NOT_FOUND:" + donor);

        // Каждый закреплённый файл сверяется целиком по содержимому.
        foreach (var pinned in manifest.GetProperty("whole_file_sha256").EnumerateObject())
        {
            var path = Path.Combine(donor, pinned.Name);
            if (!File.Exists(path))
                throw new FileNotFoundException("RMS_ROW_DONOR_FILE_NOT_FOUND:" + path, path);

            var bytes = File.ReadAllBytes(path);
            var actual = Sha256Hex(bytes);
            if (actual != pinned.Value.GetString())
            {
                throw new InvalidOperationException(
                    $"RMS_ROW_DONOR_FINGERPRINT_MISMATCH:{pinned.Name}:ожидался "
                    + $"{pinned.Value.GetString()}, прочитан {actual}");
            }
        }

        // Объявленные размеры тоже сверяются: указатель хранилища больших файлов
        // вместо настоящих весов весит иначе и обязан быть замечен.
        foreach (var declared in manifest.GetProperty("file_bytes").EnumerateObject())
        {
            var length = new FileInfo(Path.Combine(donor, declared.Name)).Length;
            if (length != declared.Value.GetInt64())
            {
                throw new InvalidOperationException(
                    $"RMS_ROW_DONOR_SIZE_MISMATCH:{declared.Name}:объявлено "
                    + $"{declared.Value.GetInt64()}, прочитано {length}");
            }
        }

        return donor;
    }

    // Запечатанные результаты опыта — это эталон для четырёх учебных обновлений.
    // Их байты тоже закреплены, иначе эталон можно было бы молча переписать.
    public static JsonDocument OpenSealedArm(JsonElement manifest, string arm)
    {
        var gold = manifest.GetProperty("independent_gold_ru");
        var armEntry = gold.GetProperty("arms").GetProperty(arm);
        var path = Path.Combine(
            RepositoryRoot(),
            gold.GetProperty("experiment_results_relative_root").GetString()!
                .Replace('/', Path.DirectorySeparatorChar),
            armEntry.GetProperty("file").GetString()!.Replace('/', Path.DirectorySeparatorChar));

        if (!File.Exists(path))
            throw new FileNotFoundException("RMS_ROW_SEALED_ARM_NOT_FOUND:" + path, path);

        var bytes = File.ReadAllBytes(path);
        var actual = Sha256Hex(bytes);
        if (actual != armEntry.GetProperty("sha256").GetString())
        {
            throw new InvalidOperationException(
                $"RMS_ROW_SEALED_ARM_FINGERPRINT_MISMATCH:{arm}:ожидался "
                + $"{armEntry.GetProperty("sha256").GetString()}, прочитан {actual}");
        }

        return JsonDocument.Parse(bytes);
    }

    // Записанная панель по опознавательному имени поля, разложенная по подсказке.
    // Ключ — подсказка, а не номер в массиве: порядок записи не должен молча
    // переставлять строки местами.
    public static IReadOnlyDictionary<string, RmsRowReadout> RecordedPanel(
        JsonElement root, string field)
        => root.GetProperty(field).EnumerateArray().ToDictionary(
            row => row.GetProperty("prompt").GetString()!,
            row => new RmsRowReadout(
                row.GetProperty("prompt").GetString()!,
                row.GetProperty("gold_token_id").GetInt32(),
                row.GetProperty("predicted_token_id").GetInt32(),
                row.GetProperty("teacher_forced_exact").GetBoolean(),
                row.GetProperty("target_nll").GetDouble(),
                row.GetProperty("gold_rank").GetInt32(),
                row.GetProperty("argmax_order_rank").GetInt32(),
                Convert.ToBase64String(FloatBits(
                    row.GetProperty("last_logits").EnumerateArray()
                        .Select(value => value.GetSingle()).ToArray()))),
            StringComparer.Ordinal);

    // ── Восстановление назначенного обученного начала ───────────────────────

    // Порядок шагов взят из карточки переноса дословно: построение с
    // детерминированным зерном, присоединение низкоранговых адаптеров в
    // назначенном случайном потоке, загрузка обученных весов, привязки
    // оптимизатора только по фактически присоединённым параметрам, ввоз полного
    // состояния оптимизатора с требованием номера шага, восстановление
    // состояния генератора случайных чисел и создание обучателя.
    public static RmsRowRestoredStart Restore(JsonElement manifest, string donorPath)
    {
        var model = BuildModel(manifest);
        var bindings = AdapterBindings(model, manifest);

        // Обученные веса читаются из общего донора потоком, без копии в проверку.
        using (var weights = new FileStream(
                   Path.Combine(donorPath, "S0.weights.bin"),
                   FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            weights.Position = 0;
            model.LoadCheckpoint(weights);
        }

        // Обучаемые и замороженные массивы разделяются до обучения тем же
        // признаком, что у хозяина опыта: пустой набор недопустим.
        var trainable = model.Parameters().Where(p => p.Value.RequiresGrad).ToArray();
        var frozen = model.Parameters().Where(p => !p.Value.RequiresGrad).ToArray();
        if (trainable.Length == 0 || frozen.Length == 0)
            throw new InvalidOperationException("RMS_ROW_MISSING_TRAINABLE_OR_FROZEN_STATE");

        // Оптимизатор создаётся с объявленной донором настройкой и получает
        // полное сохранённое состояние. Отказ ввоза — ошибка, а не тихий старт
        // с нулевыми накопленными оценками градиентов.
        var optimizerSettings = manifest.GetProperty("training_ru").GetProperty("optimizer");
        var optimizer = new AdamW(
            beta1: optimizerSettings.GetProperty("beta1").GetSingle(),
            beta2: optimizerSettings.GetProperty("beta2").GetSingle(),
            weightDecay: optimizerSettings.GetProperty("weightDecay").GetSingle(),
            eps: optimizerSettings.GetProperty("eps").GetSingle());

        var donorOptimizer = AdamWPortableState.FromBytes(
                File.ReadAllBytes(Path.Combine(donorPath, "S0.optimizer.bin")))
            ?? throw new InvalidOperationException("RMS_ROW_OPTIMIZER_STATE_UNREADABLE");

        var declaredStep = manifest.GetProperty("restored_state_ru")
            .GetProperty("optimizer_step").GetInt32();
        if (donorOptimizer.Step != declaredStep)
        {
            throw new InvalidOperationException(
                $"RMS_ROW_OPTIMIZER_STEP_MISMATCH:объявлен {declaredStep}, "
                + $"прочитан {donorOptimizer.Step}");
        }

        if (!optimizer.TryImportPortableState(bindings, donorOptimizer))
            throw new InvalidOperationException("RMS_ROW_OPTIMIZER_IMPORT_REFUSED");

        // Состояние генератора случайных чисел восстанавливается по тем же
        // опознавательным строкам, что объявил донор.
        var identity = manifest.GetProperty("identity_literals_ru");
        var topologySha = Sha256Text(identity.GetProperty("topology_literal").GetString()!);
        var loraSha = Sha256Text(identity.GetProperty("lora_literal").GetString()!);
        var donorRandom = JsonSerializer.Deserialize<LlmModelRandomStateSnapshot>(
                File.ReadAllBytes(Path.Combine(donorPath, "S0.rng.json")))
            ?? throw new InvalidOperationException("RMS_ROW_RANDOM_STATE_UNREADABLE");
        model.RestoreRandomState(donorRandom, new LlmModelRandomStateIdentity(topologySha, loraSha));

        // Обучатель собирается из объявленной паспортом настройки: если паспорт
        // поправить руками, изменится фактическое обучение, и записанные потери
        // перестанут совпадать. Настройка не может разойтись с проверкой молча.
        var trainerSettings = manifest.GetProperty("training_ru").GetProperty("trainer");
        var trainer = new Trainer(new TrainingConfig
        {
            BatchSize = trainerSettings.GetProperty("BatchSize").GetInt32(),
            SeqLen = trainerSettings.GetProperty("SeqLen").GetInt32(),
            WarmupSteps = trainerSettings.GetProperty("WarmupSteps").GetInt32(),
            TotalSteps = trainerSettings.GetProperty("TotalSteps").GetInt32(),
            MaxLr = trainerSettings.GetProperty("MaxLr").GetSingle(),
            MinLr = trainerSettings.GetProperty("MinLr").GetSingle(),
            GradClipNorm = trainerSettings.GetProperty("GradClipNorm").GetSingle(),
            GradAccumSteps = trainerSettings.GetProperty("GradAccumSteps").GetInt32(),
            WeightDecay = trainerSettings.GetProperty("WeightDecay").GetSingle(),
        });

        // Учебные последовательности — это байты подсказки и ответа подряд, без
        // разделителя и без маски ответа: обучение идёт по всей последовательности,
        // как в исходном опыте.
        var rows = manifest.GetProperty("panel_rows_ru");
        var sequences = manifest.GetProperty("training_ru").GetProperty("call_order")
            .EnumerateArray()
            .Select(id => rows.GetProperty(id.GetString()!))
            .Select(row => Bytes(
                row.GetProperty("prompt").GetString()! + row.GetProperty("answer").GetString()!))
            .ToArray();

        return new RmsRowRestoredStart(
            model, bindings, optimizer, trainer, donorOptimizer, donorRandom,
            topologySha, loraSha, trainable, frozen, sequences);
    }

    // Модель строится ровно того устройства, что объявил донор. Детерминированное
    // зерно охватывает только конструктор: после загрузки весов зёрна не влияют.
    private static LlmModel BuildModel(JsonElement manifest)
    {
        var model = manifest.GetProperty("model_ru");
        var seeds = manifest.GetProperty("seeds_ru");
        var lora = manifest.GetProperty("lora_ru");

        LlmModel built;
        using (Initializers.UseDeterministicSeed(seeds.GetProperty("construction_seed").GetUInt64()))
        {
            built = new LlmModel(new ModelConfig
            {
                VocabSize = model.GetProperty("VocabSize").GetInt32(),
                ContextLen = model.GetProperty("ContextLen").GetInt32(),
                HiddenDim = model.GetProperty("HiddenDim").GetInt32(),
                NumLayers = model.GetProperty("NumLayers").GetInt32(),
                NumHeads = model.GetProperty("NumHeads").GetInt32(),
                NumKvHeads = model.GetProperty("NumKvHeads").GetInt32(),
                FfnHiddenDim = model.GetProperty("FfnHiddenDim").GetInt32(),
                RopeTheta = model.GetProperty("RopeTheta").GetSingle(),
                RmsNormEps = model.GetProperty("RmsNormEps").GetSingle(),
            });
        }

        LoraApplicator.ApplySeeded(
            built,
            new LoraConfig
            {
                Rank = lora.GetProperty("Rank").GetInt32(),
                Alpha = lora.GetProperty("Alpha").GetSingle(),
                Dropout = lora.GetProperty("Dropout").GetSingle(),
                TargetModules = lora.GetProperty("TargetModules").EnumerateArray()
                    .Select(value => value.GetString()!).ToArray(),
            },
            seeds.GetProperty("lora_seed").GetUInt64(),
            seeds.GetProperty("stream_domain").GetString()!);

        return built;
    }

    // Привязки состояния оптимизатора строятся только по ФАКТИЧЕСКИ
    // присоединённым адаптерам назначенных проекций и по тождеству ссылок на их
    // параметры, а ключи берутся из настоящих записей контрольной точки и
    // упорядочиваются по порядку символов. Брать все параметры или другой
    // порядок нельзя: ввоз сохранённых накопленных оценок градиентов привязан
    // именно к этому набору и порядку.
    private static IReadOnlyList<AdamWParameterBinding> AdapterBindings(
        LlmModel model, JsonElement manifest)
    {
        var targets = manifest.GetProperty("lora_ru").GetProperty("TargetModules")
            .EnumerateArray().Select(value => value.GetString()!).ToArray();

        var attached = Enumerable.Range(0, model.Blocks.Count)
            .SelectMany(block => targets
                .Select(target => model.GetAttachedProjectionAdapter(block, target)))
            .OfType<LoraLinear>()
            .ToArray();

        var allowed = attached
            .SelectMany(adapter => adapter.AdapterParameters)
            .Select(entry => entry.Parameter)
            .ToHashSet(ReferenceEqualityComparer.Instance);

        var bindings = model.GetCheckpointParameterEntries()
            .Where(entry => allowed.Contains(entry.Parameter))
            .Select(entry => new AdamWParameterBinding(entry.Key, entry.Parameter))
            .OrderBy(binding => binding.Key, StringComparer.Ordinal)
            .ToArray();

        if (bindings.Length == 0)
            throw new InvalidOperationException("RMS_ROW_ADAPTER_BINDINGS_EMPTY");

        return bindings;
    }

    // ── Чтение одной строки панели ──────────────────────────────────────────

    // Один диагностический проход по подсказке без обновления. Соглашение
    // повторяет хозяина опыта дословно: на вход идут байты подсказки, читается
    // последняя позиция, наибольший берётся строгим сравнением, а сумма
    // показателей считается в двойной точности. Точность здесь существенна:
    // если считать сумму одинарной, мера ошибки разойдётся в девятом знаке при
    // тех же самых логитах, и сравнение станет ложно красным.
    public static RmsRowReadout Observe(LlmModel model, string prompt, string answer)
    {
        var ids = Bytes(prompt);
        var input = new Tensor(
            ids.Select(id => (float)id).ToArray(), new TensorShape(1, ids.Length));

        var logits = model.Forward(input, training: false);

        var vocab = model.Config.VocabSize;
        var offset = (ids.Length - 1) * vocab;
        var last = logits.Data.AsSpan(offset, vocab).ToArray();

        var max = double.NegativeInfinity;
        var best = 0;
        for (var id = 0; id < vocab; id++)
            if (last[id] > max) { max = last[id]; best = id; }

        double sum = 0;
        for (var id = 0; id < vocab; id++) sum += Math.Exp(last[id] - max);

        var gold = Bytes(answer)[0];
        var goldLogit = last[gold];
        var nll = max + Math.Log(sum) - goldLogit;

        // Строгий ранг считает только строго большие значения. Порядковый ранг
        // дополнительно разрешает ничьи меньшим номером, как записал опыт.
        var goldRank = 1;
        var argmaxOrderRank = 1;
        for (var id = 0; id < vocab; id++)
        {
            var value = last[id];
            if (value > goldLogit) { goldRank++; argmaxOrderRank++; }
            else if (value == goldLogit && id < gold) argmaxOrderRank++;
        }

        return new RmsRowReadout(
            prompt, gold, best, best == gold, nll, goldRank, argmaxOrderRank,
            Convert.ToBase64String(FloatBits(last)));
    }

    // ── Учёт состояния ─────────────────────────────────────────────────────

    // Отпечаток набора параметров тем же правилом, что у хозяина опыта: для
    // каждого параметра в объявленном порядке ранг, затем все размерности,
    // затем все значения двоичной записью.
    public static string ParametersHash(IEnumerable<Parameter> parameters)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        foreach (var parameter in parameters)
        {
            writer.Write(parameter.Value.Shape.Rank);
            foreach (var dimension in parameter.Value.Shape.Dimensions) writer.Write(dimension);
            foreach (var value in parameter.Value.Data) writer.Write(value);
        }

        writer.Flush();
        return Sha256Hex(stream.ToArray());
    }

    // Расписка о фактически загруженных сборках продукта: место, отпечаток и
    // опознаватель сборки. Без неё нельзя сказать, какие именно байты считали
    // этот результат, а при проверке чувствительности это и есть главное.
    // Сборки берутся от самих используемых типов, а не по предполагаемому имени:
    // имя каталога проекта и простое имя сборки — разные вещи, и поиск по имени
    // каталога молча давал бы «не загружена» вместо расписки.
    public static string NativeAssemblyReceipt()
    {
        var targets = new (string Human, System.Reflection.Assembly Assembly)[]
        {
            ("ядро тензоров, где находится проверяемое суммирование", typeof(TensorOps).Assembly),
            ("модель, которая вызывает нормализацию", typeof(LlmModel).Assembly),
            ("обучатель, который делает учебные обновления", typeof(Trainer).Assembly),
        };

        var parts = new List<string>();
        foreach (var (human, assembly) in targets)
        {
            var name = assembly.GetName().Name ?? "БЕЗ_ИМЕНИ";
            if (string.IsNullOrEmpty(assembly.Location))
            {
                parts.Add($"{human} ({name}): расположение недоступно, отпечаток файла снять нельзя");
                continue;
            }

            var hash = Sha256Hex(File.ReadAllBytes(assembly.Location));
            var moduleId = assembly.ManifestModule.ModuleVersionId;
            parts.Add(
                $"{human} ({name}): файл {assembly.Location}, отпечаток {hash}, "
                + $"опознаватель сборки {moduleId}");
        }

        return string.Join(Environment.NewLine, parts);
    }

    // ── Мелкие помощники ───────────────────────────────────────────────────

    // Байты текста как номера лексем: кодировка UTF-8, без разбора на части,
    // без метки начала и конца.
    public static int[] Bytes(string text)
        => Encoding.UTF8.GetBytes(text).Select(value => (int)value).ToArray();

    // Биты чисел в том же порядке, в каком их записывали донор и опыт.
    public static byte[] FloatBits(IReadOnlyList<float> values)
    {
        var buffer = new byte[values.Count * 4];
        for (var index = 0; index < values.Count; index++)
            BitConverter.GetBytes(values[index]).CopyTo(buffer, index * 4);
        return buffer;
    }

    public static string Sha256Hex(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    // Отпечаток строки считается без метки порядка байтов, как у хозяина опыта.
    public static string Sha256Text(string literal)
        => Sha256Hex(new UTF8Encoding(false).GetBytes(literal));

    // Подъём от папки сборки до корня хранилища: путь не зависит от места запуска.
    public static string RepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "LlmFromScratch.sln"))) return current.FullName;
            current = current.Parent;
        }

        throw new DirectoryNotFoundException("RMS_ROW_REPOSITORY_ROOT_NOT_FOUND");
    }
}
