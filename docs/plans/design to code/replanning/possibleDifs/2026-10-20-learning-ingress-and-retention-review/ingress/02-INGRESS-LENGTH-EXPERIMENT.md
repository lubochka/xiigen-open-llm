# INGRESS-LENGTH-01 — длинный supervised исходник не должен блокировать короткие уроки того же домена

**Статус: SOURCE_PREPARED_BUILD_UNVERIFIED / NOT_RUN.** Это карточка малого реального SFT-опыта с полным blueprint ниже. .NET в среде данного исследования отсутствовал; компиляция и изменение параметров не заявляются. Blueprint ещё требуется перенести в указанные два новых файла и собрать. Это не готовый ранее существовавший CLI. Старые эксперименты и `docs/plans/replanning-possible-diffs` не меняются.

## 1. Вопрос и обучающая причинность

Один `TrainingExample`, не помещающийся в `ContextLen + 1`, вызывает общий возврат из IncrementalTrainer до всех обновлений. Может ли предварительное **явное** разделение на допустимые и отложенные исходники восстановить полезное SFT на короткой части? [Точная цепь и границы](01-DATA-INGRESS-FINDINGS-AND-COVERAGE.md#ingress-length-01-цепочка-доказательств).

Меняем только admission-план перед неизменённым IncrementalTrainer. Сравниваем A с исходным списком и B с коротким активным списком и отдельным deferred manifest. Ограничение длины в тренере сохраняется. Никакого обрезания строки, переобучения tokenizer, изменения LoRA, маски loss или порядка примеров нет.

**Показатели успеха:** любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост accuracy не требуется. Рядом публикуются все изменения остальных показателей, включая ухудшения. В частности, ненулевой update сам по себе доказывает разблокировку, но не улучшение качества; NLL/response NLL на TRAIN и HELDOUT должны иметь отдельные вердикты. Два обученных коротких исходника из трёх не равны покрытию всего исходного домена.

## 2. Домен и точный бюджет

Предметный домен: программирование на C#; `CODE_Q` — ключ обучаемого адаптера. Профиль — SFT восстановления содержимого короткого C# файла по prompt вида `// file: One.cs`. Это буквально формат данных ProjectIndexer; не подменять его документационным ответом. Фикстура — [ingress-length.json](fixtures/ingress-length.json): два коротких TRAIN исходника, один заведомо длинный TRAIN исходник, два HELDOUT и один контроль другой конструкции. Контроль не называется retention прежнего навыка, если B0 не был ранее обучен.

Один общий исследовательский B0: модель 1 layer, hidden16, heads2, kvHeads1, FFN32, context64, vocabulary256. Byte BPE без merges (`Train(...,256)`) фиксируется **один раз** до A/B. Это упрощённый исследовательский профиль, а не восстановление существующего production checkpoint. После создания base сохраняется в `initial/base.bin`; обе руки загружают одни и те же bytes. Инициализацию базы не называть seeded, если доступный API это не гарантирует. SHA-256 B0 — идентификатор данного опыта. LoRA seeded отдельно, rank2, alpha4, dropout0, q/v; session factory создаёт одинаковые adapters. Первое измерение S0 делается после их установки.

У A ожидается 0 updates вследствие раннего возврата; у B максимум 2 реальных optimizer updates. По одной попытке на руку; `WarmupSteps=0`, фиксированная LR `0.0003`, weight decay0. Не менять LR после увиденного результата. Если обе руки не доходят до предусмотренной стадии — записать блокер, не заменять измерение Python-формулой. При повторении использовать три отдельно сохранённых B0 и указывать «три исходных состояния», не выдуманные три seed. Первый малый опыт этого не требует.

## 3. Где создать и как собрать — решения уже приняты

Команды выполняются из корня **существующего локального checkout llm_mvp_core**. Все inputs ниже приведены дословно; отдельный путь к planning-каталогу не нужен. `RUN_ROOT` располагается рядом с checkout. Команды создания ниже предназначены для отсутствующего `RUN_ROOT`. Если он уже есть, прочитать `results/run01/status.json` и сохранённые inputs/provenance: завершённый run анализировать без повтора, незавершённую подготовку продолжить в том же каталоге только при совпавших bytes и source SHA. При конфликте записать его вне прежних results и закончить подготовку; не перезаписывать, не удалять и не назначать автоматический attempt02. Timeout процесса не разрешает повторить уже выданные updates.

```bash
set -eu
CORE_SHA=ce890a48efda7f00d0ee040f2383cc2d06428ea6
RUN_ROOT="$(dirname "$PWD")/xiigen-ingress-2026-10-20-attempt01"
test ! -e "$RUN_ROOT"
mkdir -p "$RUN_ROOT/host" "$RUN_ROOT/inputs"
GIT_LFS_SKIP_SMUDGE=1 git worktree add --detach "$RUN_ROOT/source" "$CORE_SHA"
cat > "$RUN_ROOT/inputs/ingress-length.json" <<'INGRESS_FIXTURE_JSON'
{
  "Domain": "CODE_Q",
  "AdapterSeed": 20261020,
  "Model": {
    "VocabSize": 256,
    "ContextLen": 64,
    "HiddenDim": 16,
    "NumLayers": 1,
    "NumHeads": 2,
    "NumKvHeads": 1,
    "FfnHiddenDim": 32,
    "RopeTheta": 10000,
    "RmsNormEps": 0.00001
  },
  "Lora": {
    "Rank": 2,
    "Alpha": 4,
    "Dropout": 0,
    "TargetModules": ["q_proj", "v_proj"]
  },
  "Training": {
    "BatchSize": 1,
    "SeqLen": 64,
    "MaxLr": 0.0003,
    "MinLr": 0.0003,
    "WarmupSteps": 0,
    "TotalSteps": 2,
    "GradAccumSteps": 1,
    "GradClipNorm": 1,
    "WeightDecay": 0,
    "CheckpointEvery": 100,
    "LogEvery": 1
  },
  "Train": [
    {"Id": "train-one", "Split": "TRAIN", "FileName": "One.cs", "Source": "static int One()=>1;"},
    {"Id": "train-two", "Split": "TRAIN", "FileName": "Two.cs", "Source": "static int Two()=>2;"},
    {"Id": "train-long", "Split": "TRAIN_DEFERRED", "FileName": "Long.cs", "Source": "static class LongExample { static int One()=>1; static int Two()=>2; static int Three()=>3; static int Four()=>4; static int Five()=>5; static int Six()=>6; static int Seven()=>7; static int Eight()=>8; }"}
  ],
  "Evaluation": [
    {"Id": "heldout-three", "Split": "HELDOUT", "FileName": "Three.cs", "Source": "static int Three()=>3;"},
    {"Id": "heldout-four", "Split": "HELDOUT", "FileName": "Four.cs", "Source": "static int Four()=>4;"},
    {"Id": "control-branch", "Split": "OTHER_CONSTRUCTION_CONTROL", "FileName": "Pos.cs", "Source": "static bool Pos(int n)=>n>0;"}
  ]
}
INGRESS_FIXTURE_JSON
```

Создать **только** `$RUN_ROOT/host/IngressLengthProbe.csproj` и `$RUN_ROOT/host/Program.cs` из двух блоков ниже. Рабочие исходники original checkout и source worktree не править. При конфликте более свежего HEAD использовать закреплённый detached SHA; новый HEAD может быть отдельной последующей попыткой. Не копировать старые бинарники/C50 results. ProjectReference ниже использует настоящее дерево зависимостей net9 проекта; поддержка runtime и NuGet restore обязательна для build. [ContinuousLearning.csproj, 1–18][project] явно перечисляет зависимости; host не подставляет заглушку learner.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AssemblyName>IngressLengthProbe</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="../source/14.ContinuousLearning/ContinuousLearning.csproj" />
  </ItemGroup>
</Project>
```

Blueprint `Program.cs` использует публичные API: constructor IncrementalTrainer с factory, `IncrementalTrainingSession`, `LoraApplicator.ApplySeeded`, `Trainer`, `AdamW`, `TrainingSourceProvenance`, `LlmModel.Forward`. [Публичный trainer][trainer-constructor], [session][session], [factory][factory], [LoRA][lora], [source identity][source-identity]. `UnusedStorage` не моделирует обучение: в прочитанном IncrementalTrainer нет обращений к session storage; если обращение появится, опыт явно завершится ошибкой. Данные teacher/DPO не создаются и не подделываются.

```csharp
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using ContinuousLearning;
using LoRA;
using TensorCore.Core;
using Tokenizer;
using TrainingLoop;
using TrainingLoop.Optimizers;
using TransformerModel;
using TransformerModel.Config;

if (args.Length != 2)
    throw new ArgumentException("Usage: IngressLengthProbe <fixture.json> <new-output-dir>");
var json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, WriteIndented = true };
var spec = JsonSerializer.Deserialize<ProbeSpec>(File.ReadAllText(args[0]), json)!;
var output = Path.GetFullPath(args[1]);
if (Directory.Exists(output)) throw new IOException("Output already exists; preserve it.");
Directory.CreateDirectory(output);
var initial = Path.Combine(output, "initial");
Directory.CreateDirectory(initial);
var tokenizer = new BpeTokenizer();
tokenizer.Train(string.Join("\n", spec.Train.Select(r => r.Source)), 256);
var tokenizerPath = Path.Combine(initial, "tokenizer.json");
tokenizer.Save(tokenizerPath);
var basePath = Path.Combine(initial, "base.bin");
new LlmModel(spec.Model).SaveCheckpoint(basePath);
var baseSha = Hash(File.ReadAllBytes(basePath));
var tokenizerSha = Hash(File.ReadAllBytes(tokenizerPath));
string Text(ProbeRow r) => "// file: " + r.FileName + "\n" + r.Source;
int Length(ProbeRow r) => tokenizer.Encode(Text(r)).Count;
var active = spec.Train.Where(r => Length(r) >= 2 && Length(r) <= spec.Model.ContextLen + 1).ToArray();
var deferred = spec.Train.Except(active).ToArray();
if (active.Length != 2 || deferred.Length != 1)
    throw new InvalidOperationException("Fixture must admit exactly two and defer exactly one row.");
File.WriteAllText(Path.Combine(output, "admission-plan.json"), JsonSerializer.Serialize(new {
    baseSha, tokenizerSha, originalCount = spec.Train.Length,
    originalTokens = spec.Train.Sum(Length), admittedCount = active.Length,
    admittedTokens = active.Sum(Length), deferredCount = deferred.Length,
    deferredTokens = deferred.Sum(Length), rows = spec.Train.Select(r => new {
        r.Id, sourceSha = Hash(Encoding.UTF8.GetBytes(r.Source)), encodedLength = Length(r),
        disposition = active.Contains(r) ? "ACTIVE" : "DEFERRED_CONTEXT_LENGTH",
        r.Source
    })
}, json));

var results = new List<object>();
var scoreRows = new List<object>();
string? commonInitialHash = null;
foreach (var arm in new[] { "A-full", "B-bounded" })
{
    var rows = arm == "A-full" ? spec.Train : active;
    var armDir = Path.Combine(output, arm);
    Directory.CreateDirectory(armDir);
    var sessionFactory = new FrozenFactory(spec, tokenizer, basePath, baseSha, tokenizerSha);
    var model = sessionFactory.Session.Model;
    var beforeHash = StateHash(model);
    commonInitialHash ??= beforeHash;
    if (beforeHash != commonInitialHash) throw new InvalidOperationException("A/B S0 mismatch.");
    ScoreAll("S0-before");
    if (StateHash(model) != beforeHash) throw new InvalidOperationException("Scorer mutated state.");
    var examples = rows.Select(r => new TrainingExample(
        "// file: " + r.FileName, r.Source, spec.Domain, 1.0,
        DateTimeOffset.Parse("2026-10-20T00:00:00Z"),
        TrainingSourceProvenance.CreateSourceId("fixture/" + r.FileName, r.Source))).ToArray();
    var trainer = new IncrementalTrainer(new UnusedStorage(), sessionFactory,
        Path.Combine(armDir, "adapters"), spec.Lora);
    var clock = Stopwatch.StartNew();
    var result = await trainer.TrainAsync(spec.Domain, examples, Array.Empty<DpoPair>());
    clock.Stop();
    var afterHash = StateHash(model);
    ScoreAll("S1-after");
    if (StateHash(model) != afterHash) throw new InvalidOperationException("Scorer mutated state.");
    var bindings = model.GetCheckpointParameterEntries()
        .Select(e => new AdamWParameterBinding(e.Key, e.Parameter)).ToArray();
    var optimizerState = sessionFactory.Session.Optimizer.ExportPortableState(bindings);
    var steps = optimizerState.Step;
    File.WriteAllBytes(Path.Combine(armDir, "optimizer-after.bin"), optimizerState.ToBytes());
    results.Add(new { arm, result.IsSuccess, result.ErrorCode,
        submittedRows = rows.Length, actualOptimizerSteps = steps,
        actualTargets = result.IsSuccess ? (int?)rows.Sum(r => Length(r) - 1)
            : steps == 0 && beforeHash == afterHash ? 0 : null,
        rejectedBatchRows = result.IsSuccess ? (int?)0
            : steps == 0 && beforeHash == afterHash ? rows.Length : null,
        notUpdatedOriginalRows = result.IsSuccess ? (int?)(spec.Train.Length - rows.Length)
            : steps == 0 && beforeHash == afterHash ? spec.Train.Length : null,
        executionStatus = result.IsSuccess ? "COMPLETED"
            : steps == 0 && beforeHash == afterHash ? "FAILED_NO_STATE_CHANGE"
            : "UNKNOWN_AFTER_PARTIAL_FAILURE",
        deferredRows = arm == "B-bounded" ? deferred.Length : 0,
        beforeHash, afterHash, elapsedMs = clock.Elapsed.TotalMilliseconds });
    File.WriteAllText(Path.Combine(output, "per-row.json"), JsonSerializer.Serialize(scoreRows, json));
    File.WriteAllText(Path.Combine(output, "arms.json"), JsonSerializer.Serialize(results, json));
    if (arm == "A-full" && (result.ErrorCode != "TRAINING_EXAMPLE_CONTEXT_EXCEEDED"
                            || steps != 0 || beforeHash != afterHash))
        throw new InvalidOperationException("A did not reproduce the specified admission failure.");
    if (arm == "B-bounded" && (!result.IsSuccess || steps != active.Length
                               || beforeHash == afterHash))
        throw new InvalidOperationException("B did not perform the specified real updates: " + result.ErrorCode);

    void ScoreAll(string phase)
    {
        foreach (var row in spec.Train.Concat(spec.Evaluation))
        {
            var ids = tokenizer.Encode(Text(row));
            if (ids.Count < 2 || ids.Count > spec.Model.ContextLen + 1)
            {
                scoreRows.Add(new { arm, phase, row.Id, row.Split, status = "NOT_SCORED_OVER_CONTEXT",
                    encodedLength = ids.Count, deferred = true });
                continue;
            }
            var promptCount = tokenizer.Encode("// file: " + row.FileName + "\n").Count;
            var score = Score(model, ids, promptCount);
            scoreRows.Add(new { arm, phase, row.Id, row.Split, status = "MEASURED",
                encodedLength = ids.Count, sourceSha = Hash(Encoding.UTF8.GetBytes(row.Source)), score });
        }
    }
}
File.WriteAllText(Path.Combine(output, "status.json"), JsonSerializer.Serialize(new {
    status = "MEASURED_REQUIRES_METRIC_REVIEW", baseSha, tokenizerSha,
    note = "No metric success conclusion is inferred from nonzero updates alone."
}, json));

static object Score(LlmModel model, IReadOnlyList<int> ids, int promptTokenCount)
{
    int n = ids.Count - 1, v = model.Config.VocabSize, correct = 0, responseCount = 0;
    double sum = 0, responseSum = 0;
    var tensor = new Tensor(ids.Take(n).Select(x => (float)x).ToArray(), new TensorShape(1, n));
    var logits = model.Forward(tensor, training: false).Data;
    for (int t = 0; t < n; t++)
    {
        int start = t * v, best = 0;
        for (int j = 1; j < v; j++) if (logits[start + j] > logits[start + best]) best = j;
        double maximum = logits[start + best], expSum = 0;
        for (int j = 0; j < v; j++) expSum += Math.Exp(logits[start + j] - maximum);
        double loss = maximum + Math.Log(expSum) - logits[start + ids[t + 1]];
        if (!double.IsFinite(loss)) throw new InvalidOperationException("Nonfinite score.");
        sum += loss;
        if (best == ids[t + 1]) correct++;
        if (t + 1 >= promptTokenCount) { responseSum += loss; responseCount++; }
    }
    return new { targetCount = n, nllSum = sum, nllMean = sum / n,
        correctTokens = correct, tokenAccuracy = (double)correct / n,
        responseTargetCount = responseCount, responseNllSum = responseSum,
        responseNllMean = responseSum / responseCount };
}
static string StateHash(LlmModel model)
{
    using var stream = new MemoryStream();
    using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        foreach (var entry in model.GetCheckpointParameterEntries().OrderBy(e => e.Key, StringComparer.Ordinal))
        {
            writer.Write(entry.Key);
            writer.Write(entry.Parameter.Value.Data.Length);
            foreach (var value in entry.Parameter.Value.Data) writer.Write(BitConverter.SingleToInt32Bits(value));
        }
    return Hash(stream.ToArray());
}
static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

public sealed record ProbeRow(string Id, string Split, string FileName, string Source);
public sealed record ProbeSpec(string Domain, ulong AdapterSeed, ModelConfig Model,
    LoraConfig Lora, TrainingConfig Training, ProbeRow[] Train, ProbeRow[] Evaluation);
public sealed class FrozenFactory : IIncrementalTrainingSessionFactory
{
    public IncrementalTrainingSession Session { get; }
    private bool used;
    public FrozenFactory(ProbeSpec spec, BpeTokenizer tokenizer, string basePath,
        string baseSha, string tokenizerSha)
    {
        var model = new LlmModel(spec.Model);
        model.LoadCheckpoint(basePath);
        LoraApplicator.ApplySeeded(model, spec.Lora, spec.AdapterSeed, "ingress-probe");
        Session = new IncrementalTrainingSession(model, tokenizer, new Trainer(spec.Training),
            new AdamW(weightDecay: spec.Training.WeightDecay), baseSha, tokenizerSha,
            TrainingSourceProvenance.Identify(spec.Model));
    }
    public OperationResult<IncrementalTrainingSession> Create(string domain)
    {
        if (used) throw new InvalidOperationException("One session per arm.");
        used = true;
        return OperationResult<IncrementalTrainingSession>.Ok(Session);
    }
}
public sealed class UnusedStorage : ISessionStorage
{
    public Task<OperationResult<bool>> SaveAsync(string id, AgentState state) => throw new NotSupportedException();
    public Task<OperationResult<AgentState>> LoadAsync(string id) => throw new NotSupportedException();
    public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync() => throw new NotSupportedException();
}
```

Затем только этот host:

```bash
dotnet build "$RUN_ROOT/host/IngressLengthProbe.csproj" -c Release
dotnet run --project "$RUN_ROOT/host/IngressLengthProbe.csproj" -c Release --no-build -- "$RUN_ROOT/inputs/ingress-length.json" "$RUN_ROOT/results/run01"
```

В blueprint нет попытки использовать имя friend assembly, снять production authority или заменить реальный update stub-ом. При build failure записать `BUILD_BLOCKED` с точной ошибкой в новом `status.json`; чинить лишь новый host/его project wiring. Если требуется изменение original dependency, этот опыт остаётся заблокированным и выдаётся конкретный dependency blocker; production/full suite ради него не запускать. Текущее `SOURCE_PREPARED_BUILD_UNVERIFIED` нельзя автоматически повысить до READY по наличию code block.

Счётчик optimizer берётся из публичного `ExportPortableState(...).Step`; `CaptureState().Step` имеет internal visibility и для внешнего host не используется. Этот API проверен отдельно по [AdamW,188–227][adamw-export] и [публичному portable state][adamw-state]. Экспорт вынесен за измеряемый интервал training. Наличие такого exporter не выдаётся за дешёвую постоянную стоимость счётчика в продукте.

## 4. Как читать before/after правильно

По каждой строке фиксируются S0/S1 на одном input и одном наборе targets, teacher-forced logits при `training:false`, `nllMean`, `nllSum`, `responseNllMean`, token accuracy и числители/знаменатели. При byte256 нет BPE crossing-token неоднозначности границы; при замене tokenizer такой расчёт boundary требует отдельного доказательства и этот опыт уже не та же фикстура.

Показать для каждой метрики `A:S1−S0`, `B:S1−S0` и парную разность этих изменений. TRAIN, HELDOUT и контроль другой конструкции не смешивать. Снижение TRAIN NLL — успех усвоения TRAIN по этой метрике; оно не объявляется generalization. Снижение HELDOUT NLL — отдельный локальный успех. Рост ошибки на контрольной конструкции — отдельная регрессия, даже если TRAIN улучшился.

`admission-plan.json` показывает исходные три строки, IDs, исходники, длины и deferred причину. Для длинной строки `NOT_SCORED_OVER_CONTEXT` — **не нулевая ошибка**. Она не обучалась в B; в A не обучалась ни одна. A/B сравнивает доступность обновления при том же предложенном наборе данных, а не equal-compute эффективность двух learning kernels. `elapsedMs` относится к вызову trainer без scoring и не включает compiler/restore. Сравнивать runtime на 0 и 2 updates как «ускорение обучения» нельзя; дополнительно вычислить B время/update и время/target из явных числителей.

В текущий бюджет входит только указанная A/B попытка, максимум два настоящих updates. Дополнительный negative control без длинной строки здесь не назначен: приведённый Program намеренно требует один deferred input и отказ A. Такой контроль потребовал бы отдельного режима с загрузкой того же B0 и иными ожиданиями; его нельзя выполнять изменением fixture на месте. При неуспехе после ненулевых optimizer steps targets/необученные строки отмечаются `null` и `UNKNOWN_AFTER_PARTIAL_FAILURE`, а не ложным нулём; фактический step и hashes сохраняются.

## 5. Где чинить оригинал только после решения пользователя

Место возможного изменения: формирование training plan до transaction candidate/hash и mutation в `ContinuousLearningPipeline`/`IncrementalTrainer`. [Pipeline, 169–180][pipeline]; [IncrementalTrainer, 385–475][admission]. Сначала вычислить exact tokenizer/config-bound eligible и deferred manifests, затем связать admitted IDs с provenance и candidate hash. Не пропускать строки внутри уже захэшированного цикла, иначе `examples[index]`, `tokenSequences[index]`, source manifest и transaction identity могут разойтись. Разбиение длинных файлов на методы/окна — следующий отдельный эксперимент; не смешивать его с admission repair.

## 6. Промпт исполнителю

> Реализуй только новый host и выполни INGRESS-LENGTH-01 по blueprint и fixtures этой карточки в указанном isolated worktree. Сначала снимай блокер реального обучения; protected experiment folder и original production source не менять. Ни одна старая карточка не удаляется. Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост accuracy не требуется; рядом покажи все ухудшения и потерю/отложение покрытия. Получи реальные parameter before/after и optimizer steps через публичный IncrementalTrainer; parser-only/аналитический расчёт не заменяет опыт. DPO здесь не запускается: его блокировка остаётся source consequence. Если build не прошёл, запиши точный блокер; не называй код готовым или улучшение доказанным. После результатов решение о переносе изменения в production принимает пользователь.

[project]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/ContinuousLearning.csproj#L1-L18
[trainer-constructor]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Training/IncrementalTrainer.cs#L54-L68
[session]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Training/IncrementalTrainingSession.cs#L21-L32
[factory]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Fabric/Interfaces.cs#L195-L206
[lora]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/10.LoRA/LoraApplicator.cs#L79-L102
[source-identity]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Core/TrainingProvenance.cs#L71-L81
[pipeline]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Pipeline/ContinuousLearningPipeline.cs#L169-L180
[admission]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/14.ContinuousLearning/Training/IncrementalTrainer.cs#L385-L475
[adamw-export]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/04.TrainingLoop/Optimizers/AdamW.cs#L188-L227
[adamw-state]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/04.TrainingLoop/Optimizers/AdamWPortableState.cs#L15-L138
