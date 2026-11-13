# AST-SIB-CACHE-01 — стоимость sibling-признаков внутри обучения Structural KNN

**Статус 20.10.2026:** `DESIGN_READY_NOT_RUN`. Это новая карточка-кандидат; реализация эксперимента и обучение в текущем документном проходе не выполняются. Все численные результаты — `NOT_MEASURED`. Старые эксперименты, raw и карточки сохраняются.

## 1. Задача, приоритет и правила исполнения

Проверить, уменьшает ли однократное вычисление позиций детей одного AST-родителя время и выделения памяти **настоящего `StructuralKnnPredictor.TrainFromSource`**, сохраняя все обученные пары и ответы. Это учебная стоимость C#-структурной модели, а не отдельный benchmark списка/словаря. Эксперимент не заявляет объяснение качества RP01 до собственного результата.

Приоритет в registry: **после конкретных блокеров фактического обучения и конфликтов их карточек; в группе новых учебных улучшений; перед проверками без доказанной связи с обучением**. Точная новая последовательность для общего document registry: `ROPE-TRAIN-TABLE-01 → AST-SIB-CACHE-01 → NG01 → IDENTITY-M1-01 → KN-ROUTER-LEARNED-KEY-01 → END`; последние два — learning-dependent readers, не причина отложить direct learners. Исходные защищённые номера16/32 сохраняются. Если назначен только AST, его `next_id=END`: другие ID автоматически не запускаются. Это порядок будущей программы, **не разрешение запуска в текущем документном проходе**. Исторические успехи и уже измеренные кандидаты не надо перезапускать ради этой карточки.

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и домену; рост точности для этого не обязателен. В отчёте рядом показать изменения остальных показателей, включая ухудшения.** Для этого кандидата полезны уменьшение allocated bytes полного обучения и уменьшение времени полного обучения. Изменение только размера JSON, числа строк отчёта или скорости отдельного `ChildNodes` без обучения не является результатом карточки.

**Обязательное правило stop/next.** При явном «следующий эксперимент» немедленно прекратить прежнюю задачу и перейти к явно назначенному следующему ID: не дописывать прежний отчёт/registry, не делать дополнительные review, hash checks, tool calls, запуски, тесты или cleanup предыдущего ID. При явном «закончить немедленно / остановись» немедленно прекратить действия и новые запуски; если собственный процесс ещё работает, допустимо только необходимое прекращение этого процесса и его дочерних процессов, затем никаких новых report/hash/cleanup действий. Кратко сообщить состояние уже выполненного по доступному контексту. Не повторять описанное пользователем поведение многократного продолжения предыдущей задачи после её смены. До запуска процессам задаются конечные таймауты; timeout не разрешает продолжать работу после stop. Если обучение технически не началось, его уже зафиксированный статус — `TRAINING_NOT_STARTED`, не отрицательный результат гипотезы; оставшиеся `status.json`/логи сохраняются.

## 2. Первичные доказательства и новая часть

Source pin: `lubochka/llm_mvp_core@0b76ab950d82534f5904005dda096545505ae307`. Машиночитаемый паспорт — [ast-sib-cache-source-list.json](evidence/ast-sib-cache-source-list.json); общий порядок — [04-REGISTRY.md](04-REGISTRY.md).

1. `StructuralVectorExtractor.Extract`, [L54–59](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/12.SemanticCompiler/Prediction/StructuralKnn.cs#L54-L59), для каждого узла создаёт `node.Parent.ChildNodes().ToList()`, затем ищет узел через `IndexOf`.
2. Настоящий `TrainFromSource`, [L124–141](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/12.SemanticCompiler/Prediction/StructuralKnn.cs#L124-L141), парсит C#, обходит все соседние узлы, вызывает этот extractor и передаёт пары в `Train`. `Train` заменяет `_training` этими парами, [L117–120](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/12.SemanticCompiler/Prediction/StructuralKnn.cs#L117-L120).
3. `Predict` затем читает `_training`, считает прежнюю distance, берёт `k` соседей и голосует, [L145–167](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/12.SemanticCompiler/Prediction/StructuralKnn.cs#L145-L167). Здесь есть реальная запись и чтение выученной памяти.

Для родителя с `m` детьми, у которых извлекаются признаки, baseline повторяет материализацию списка примерно `m` раз и последовательно ищет позиции. Суммарная работа этого участка растёт как `m²`; это вывод из циклов, **не измеренные ms/bytes**. Область возможного выигрыша — широкие C# блоки, argument/initializer lists, generated code. У узких деревьев словари B могут стоить дороже; именно поэтому есть отдельный узкий профиль.

Ближайшее существующее покрытие — **P26.4** про использование `SiblingCount` в расстоянии, **P27.6** про `%200` codec и прежний [57/argument-gap](https://github.com/lubochka/xiigen-planning/blob/4b950599a04388941f56b41e634233ebb7dc17c2/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/57-ARGUMENT-AND-VERIFIER-LEARNING-GAPS-2026-10-19/01-structural-argument-gap.md). Здесь не меняется ни один признак, расстояние, аргументный ordinal, ограничение 255 или целевой label: меняется цена вычисления тех же данных во время native обучения. Адресный поиск по всем **734 planning Markdown и 330 protected Markdown** не нашёл прежнего опыта такой цены. Receipt и фактические совпадения: [ast-sibling-novelty-search.json](evidence/ast-sibling-novelty-search.json). Это адресная проверка новизны, а не утверждение, что весь смысл всех документов исчерпывающе доказан поиском строк.

## 3. Замкнутый паспорт: модель, пути, входы, зависимости

| Поле | Фиксированное значение |
|---|---|
| ID | `AST-SIB-CACHE-01` |
| Обучаемый механизм | Исходный `SemanticCompiler.Prediction.StructuralKnnPredictor`, `k=5` — его default. Начальная память пуста. Neural model, LoRA, DPO, токенизатор BPE и внешние teacher/judge здесь не участвуют |
| Модель исполнителя и reviewer | Та же фактически назначенная модель и настройки, что у соответствующего агента по умолчанию. Не переключать модель; в отчёте записать фактическую, не выдумывать имя |
| Язык карточки, промпта и итогового отчёта | Русский; ID, paths, C# и названия метрик сохраняются |
| Применяемые навыки | Точные файлы в `SourceCore/docs/skills/`: `architect-behavior-classifier-SKILL.md`, `learning-path-audit-SKILL.md`, `code-examination-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `plan-self-validation-SKILL.md`, `experiment-design-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `output-readability-gate-SKILL.md`, `human-communication-GLOSSARY.md`. Применить к одному native пути; правила пользователя выше старых skill gates |
| Source | `12.SemanticCompiler/Prediction/StructuralKnn.cs`, source blob `b3aa5d376be3f93423997674dc015cea7e9fbf58` |
| Native A / B | `<Core>/35.ExperimentsV2/Experiments/Rp01Micro/AST-SIB-CACHE-01/A.cs` и `B.cs` |
| Host | `<Core>/experiments-v2/RP01-MICRO/AST-SIB-CACHE-01/Runner.csproj`, `Program.cs`, `prepare.py`, `run.py` |
| Данные | В host: `data/train-narrow.cs`, `data/train-wide.cs`, `data/heldout.cs`; точная генерация ниже, одинаковые bytes обоих arms |
| Результаты | `<Planning>/llm mvp/design to code session/replanning/possibleDifs/runs/AST-SIB-CACHE-01/20261020-01/`; вложенные `nA`, `nB`, `wA`, `wB` |
| SourceCore / Core / Planning | `SourceCore` — корень уже открытого git checkout `llm_mvp_core`, только чтение pinned source. `Core` — фиксированный короткий Windows workspace **`C:\x\ast01`**, где лежат только experimental host/native copies, Git там не нужен. `Planning` — существующий checkout `xiigen-planning` рядом с **SourceCore**, в него добавляются результаты. Команды ниже не ищут других проектов |
| Framework / package | `net9.0`, `Microsoft.CodeAnalysis.CSharp` **4.12.0** — как в текущих [12 csproj](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/12.SemanticCompiler/SemanticCompiler.csproj#L3-L14) и [35 csproj](https://github.com/lubochka/llm_mvp_core/blob/0b76ab950d82534f5904005dda096545505ae307/35.ExperimentsV2/ExperimentsV2.csproj#L3-L14). `SyntacticRoleMapper` уже в копируемом файле; иных source dependencies нет |
| Бюджет | 4 `TrainFromSource`: narrow A, narrow B, wide B, wide A; 1 обучение в каждом свежем процессе. 0 warm-up, 0 повторных обучений, 0 внешних/LLM генераций. 16 frozen native query до и 16 после каждого обучения: всего 128 `Predict` |

Это полный рецепт подготовки отдельного micro-проекта; `Runner.csproj`, `prepare.py`, `Program.cs` и native копии пока не созданы. Ссылка на весь `ExperimentsV2.csproj` или `SemanticCompiler.csproj` не нужна. Получатель исправления после выбора пользователем — `12.SemanticCompiler/Prediction/StructuralKnn.cs::TrainFromSource/Extract`. Отдельный caller `SemanticCompilerFacade.TrainStructuralKnn` также повторяет extractor; его будущая интеграция потребует передавать аналогичный cache, созданный на один разобранный файл. **Данный опыт измеряет именно `TrainFromSource`; ускорение façade без отдельной интеграции не заявляется.**

Разрешённые экспериментальные исключения заданы заранее: один `Program.cs`, обычные DTO, `void`, `throw` и `try/catch` для запуска/фиксации ошибок, read-only reflection для snapshot вне измеренного Train. Они допустимы в этой изолированной experimental копии и не разрешают перенос таких исключений в production. Product DNA-процедуры, full CI, исправления продуктовых тестов и оформление production-внедрения не являются prerequisite этого опыта.

## 4. Единственное изменение B

`prepare.py` читает source bytes командой `git -C <SourceCore> show 0b76ab950d82534f5904005dda096545505ae307:12.SemanticCompiler/Prediction/StructuralKnn.cs` через `subprocess.check_output` (без shell), сохраняет original SHA/blob provenance и создаёт две копии **в коротком Core**. Команда никогда не ищет Git в `C:\x\ast01`. Только namespace меняется одинаково: `SemanticCompiler.Prediction` → `AstSibCache01.A` / `AstSibCache01.B`. A далее остаётся исходной. В B сделать следующие изменения:

- Добавить `SiblingFeatureCache`, живущий только в одном вызове `TrainFromSource` и одном синтаксическом дереве. Он хранит для каждого родителя его capped child count и для каждого ребёнка capped index; прежняя семантика `Math.Min(...,255)` и fallback index 0 сохраняется.
- Сохранить публичный `Extract(node,fileId=0)` без cache для всех существующих query callers; его исходное поведение остаётся. Внутренний overload принимает cache. В нём изменяется только блок L54–59, остальной расчёт вектора скопирован без изменения.
- В `TrainFromSource` после `nodes = …ToArray()` создать cache и передать его в вызов extractor. `Train(pairs)` выполняется как прежде. По возврату cache больше не хранится в модели или static state.

Однозначная реализация cache, которую использует внутренний overload B:

```csharp
internal sealed class SiblingFeatureCache
{
    private readonly Dictionary<SyntaxNode, byte> _counts = new();
    private readonly Dictionary<SyntaxNode, byte> _positions = new();

    internal (byte Position, byte Count) Get(SyntaxNode node)
    {
        if (node.Parent is not { } parent) return (0, 0);
        if (!_counts.TryGetValue(parent, out var count))
        {
            var children = parent.ChildNodes().ToArray();
            count = (byte)Math.Min(children.Length, 255);
            _counts.Add(parent, count);
            for (var i = 0; i < children.Length; i++)
                _positions.TryAdd(children[i], (byte)Math.Min(i, 255));
        }
        return (_positions.TryGetValue(node, out var index) ? index : (byte)0, count);
    }
}
```

Используется default equality `SyntaxNode`, как в исходном `List.IndexOf`; `TryAdd` сохраняет первое вхождение. Root, byte saturation, fileId, parent chain, depth, role mapping, `%200`, `_training` insertion order, `k`, distance, tie policy, seed и запросы не менять. Не оптимизировать заодно ancestors, parsing или сортировку KNN. Не добавлять global/static cache, pooling и удержание AST между вызовами.

## 5. Точные данные и before/after

Данные генерируются в `prepare.py` до измерений в UTF-8 без BOM, с LF. Для `profile=narrow`: `methodCount=128`, `width=8`; для `wide`: `methodCount=1`, `width=1024`. В обоих профилях ровно 1024 оператора `x += ...;`; число остальных AST-узлов различается и записывается, не считается одинаковым.

```python
def training_source(method_count, width):
    out = ['namespace AstTrain { public sealed class Lessons {']
    for m in range(method_count):
        out.append(f'public int M{m:03d}(int x) {{')
        for i in range(width):
            out.append(f'x += {m * width + i + 1};')
        out.append('return x; }')
    out.append('} }')
    return '\n'.join(out) + '\n'
```

`heldout.cs` имеет ровно следующий текст. Он не поступает в `TrainFromSource` и не используется для выбора размера cache/параметров B:

```csharp
namespace AstExam {
  public sealed class Questions {
    public int H(int x) {
      x += 2001;
      x -= 2002;
      if (x > 2003) { x += 2004; }
      return x;
    }
  }
}
```

До запуска зафиксировать SHA256 всех трёх файлов. Program один раз разбирает `heldout.cs`; из `GetRoot().DescendantNodes().ToArray()` берёт первые 16 пар `(node[i], node[i+1])`. Gold — `NodeTypeId(node[i+1])`, то есть существующий compact ID; сырые SyntaxKind сохранить рядом. Если пар меньше16, это ошибка подготовки и до обучения не запускать ни один arm. В каждом arm `Predict` получает исходный public extractor для query, без cache; это read path настоящей выученной модели.

До обучения сохранить `TrainingSize=0`, snapshot пустой `_training`, 16 predictions и `next_compact_id_accuracy_before`. После **одного** native `TrainFromSource(trainText,fileId:0)` сохранить полный ordered `_training`, 16 predictions и `next_compact_id_accuracy_after`. `_training` читается через reflection только для отчёта, вне timed section; не подставлять/дописывать пары через reflection. Для canonical snapshot записывать по очереди все 10 полей `StructuralVector` и `NextToken`, в фиксированном порядке полей/записей, одинаковом в A/B; SHA namespace-dependent JSON типов не использовать.

Это измеряет обучение структурной памяти и сохранность её вывода. `next_compact_id_accuracy` — корректность текущего compact codec на этой панели; она не равна компилируемости программы или смысловой точности свободного C#-ответа. Для EN/RU/HE/ES, SQL без C# AST и обычного порядка слов этот эксперимент не устанавливает полезности cache или новых структурных законов.

## 6. Измерения и формат результата

Timed section содержит **весь `TrainFromSource`**: Roslyn parse, создание nodes/pairs/cache, извлечение векторов и запись `_training`. Подготовка данных, сборка, процессный startup, сериализация snapshot и evaluation находятся вне секции. В текущем синхронном вызове измерять `Stopwatch.GetTimestamp()` и `GC.GetAllocatedBytesForCurrentThread()` до/после. Никаких ручных `GC.Collect()`; первый JIT, если он попал внутрь вызова, входит в отмеченный **cold training time**. Это один запуск, не оценка распределения latency. Меньшее время — полезное наблюдение этого запуска, без заявления устойчивого causal speedup; allocations и state показывают отдельно. Порядок narrow A→B / wide B→A фиксирован, но не является контролем порядка на одинаковых inputs: профили различаются.

| Показатель | Что записать для каждого профиля |
|---|---|
| `train_allocated_bytes` | A, B, `B-A`; меньше полезно. Это allocation volume, не peak RAM |
| `train_elapsed_ms` | A, B, `B-A`; меньше полезно. Полная секция, одинаковые границы |
| `training_calls` / `TrainingSize` | 1 на arm; before0, after фактический count. Не нулевое обучение не заменять фиктивным счётчиком |
| `trained_pairs_sha256` | Полный ordered state after; совпадение A/B и любые отличия каждого поля |
| `next_compact_id_accuracy` | A0→A1, B0→B1 и разность этих изменений на одной панели |
| `prediction_disagreements` | Число и конкретные query IDs с разными A1/B1 outputs |
| Остальные характеристики | Количество входных bytes, AST nodes, максимальная ширина родителя; runtime/package версии; build/exception status. Peak RSS, CPU time и semantic free-answer accuracy — `NOT_MEASURED`, если собственного измерителя нет |

Меньше allocations в wide при прежней accuracy — успех `train_allocated_bytes / C# wide AST`. Регрессия allocations в narrow или времени в любом профиле показывается рядом и ограничивает область переноса. Рост accuracy не обязателен и не задаёт порог допуска. Если state A/B различается, сохранить полезные измеренные изменения и все регрессии, но не писать «эквивалентная функция доказана»: нужен разбор точного первого расхождения без автоматического нового запуска. Если native обучение не завершилось либо один arm обучен на другом содержимом, сравнение стоимости равной работы не установлено; report содержит ошибки и фактические измерения, гипотеза остаётся без подтверждения.

Каждый из `nA/nB/wA/wB` содержит `metrics.json`, `state-before.json`, `state-after.json`, `predictions.json`, `status.json`, `stdout.txt`, `stderr.txt`. Корень attempt содержит `manifest.json`, `source-ab.diff`, `comparison.json`, `report.md`, `candidate-fix.md`. `report.md` по-русски — **до350 слов плюс таблицы** — начинается: «Меняли …; реально обучалось …; измерили …; остальные показатели …». Затем таблица всех метрик, положительные показатели отдельно по профилям, ухудшения/ограничения, первичные filenames и source pin. `candidate-fix.md` — **до120 слов** — указывает исходный receiver и ровно экспериментальный B diff; итог `CANDIDATE_FOR_USER_DECISION`, без автоматического production переноса. Пока не поступил STOP/NEXT, разрешён один readback сохранённых raw/summary для соответствия отчёту; он не добавляет обучений, предсказаний, тестов или повторов. При STOP/NEXT этот readback тоже прекращается.

## 7. Проект и команды без поиска scaffold

`Runner.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <AssemblyName>AstSibCache01</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.12.0" />
    <Compile Include="Program.cs" />
    <Compile Include="../../../35.ExperimentsV2/Experiments/Rp01Micro/AST-SIB-CACHE-01/A.cs" />
    <Compile Include="../../../35.ExperimentsV2/Experiments/Rp01Micro/AST-SIB-CACHE-01/B.cs" />
  </ItemGroup>
</Project>
```

Исполнитель создаёт `prepare.py`, `run.py` и `Program.cs` по этому рецепту в коротком Host. `prepare.py` создаёт только перечисленные native copies, проект, данные и manifest; отказывается перезаписывать существующие файлы с другими bytes. `Program.cs` поддерживает `--arm A|B --profile narrow|wide --output <directory>` и read-only режим `--compare <attempt>` для таблицы/русского отчёта. В compare нет `TrainFromSource`, новых inference или запуска процесса. Все raw writes — `FileMode.CreateNew`; существующий частичный attempt не удалять.

`run.py` — контроллер с фиксированной таблицей `phase→argv/timeout`: `build` вызывает только `dotnet build <Host>/Runner.csproj -c Release -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false`, лимит180с; `nA/nB/wB/wA` вызывают DLL `<Host>/bin/Release/net9.0/AstSibCache01.dll` с соответствующими `--arm`, `--profile`, `--output <Out>/<phase>`, лимит60с на каждый; `compare` вызывает ту же DLL `--compare <Out>`, лимит15с. `prepare.py` имеет общий лимит45с; его subprocess version/source reads также имеют finite timeout в остатке бюджета. **Сумма лимитов процессов —480с**; паузы между командами в эту сумму не входят, глобальный wall deadline не заявляется. Это потолки отдельных процессов, не назначенные дополнительные повторы. Python `subprocess` получает список аргументов без shell. Контроллер заранее захватывает stdout/stderr в перечисленные raw, при timeout прекращает только своё дерево процесса (`taskkill /PID <ownedPid> /T /F` на Windows), записывает `process-status.json` и возвращает ненулевой exit; автоматического повторного запуска нет. При explicit stop/next прекращение этого дерева — единственная допустимая техническая завершающая операция. Directory для arm может содержать эти process logs; прежние metrics/state/status не перезаписывать.

PowerShell, из корня уже открытого core checkout, после реализации только этого host:

```powershell
$SourceCore = (git rev-parse --show-toplevel).Trim()
$Core = 'C:\x\ast01'
$Planning = Join-Path (Split-Path $SourceCore -Parent) 'xiigen-planning'
$HostDir = Join-Path $Core 'experiments-v2/RP01-MICRO/AST-SIB-CACHE-01'
$Out = Join-Path $Planning 'llm mvp/design to code session/replanning/possibleDifs/runs/AST-SIB-CACHE-01/20261020-01'
python (Join-Path $HostDir 'prepare.py') --source-core $SourceCore --core $Core --planning $Planning --out $Out
if ($LASTEXITCODE -ne 0) { throw 'AST preparation failed; no training started.' }
function InvokeAstPhase([string]$Phase) {
    python (Join-Path $HostDir 'run.py') --host $HostDir --out $Out --phase $Phase
    if ($LASTEXITCODE -ne 0) { throw "AST phase $Phase failed; no automatic retry." }
}
InvokeAstPhase 'build'
InvokeAstPhase 'nA'
InvokeAstPhase 'nB'
InvokeAstPhase 'wB'
InvokeAstPhase 'wA'
InvokeAstPhase 'compare'
```

Исполнять следующую фазу только после успеха предыдущей; при неуспешной сборке обучение не начинать. Preflight `prepare.py` проверяет `SourceCore`/`Planning`, source blob, наличие .NET9/Python, полный путь каждого planned artifact <240 символов и отсутствие существующего attempt **до любого Train**. Package4.12.0 восстанавливается только обычной build-командой с её лимитом; отсутствие package — build failure, не отрицательный результат обучения. Все native/host/build пути уже находятся в коротком `C:\x\ast01`; полный repo path внутрь них не вкладывается. Если отдельный целевой planning output путь всё ещё длиннее240, фиксировать этот конкретный подготовительный blocker до обучения; не менять protected folder и не запускать опыт без места для его raw. После результата файл не надо переносить из тайной временной папки: raw сразу пишется в разрешённый `possibleDifs/runs` существующего Planning.

Конфликт существующего `20261020-01`: сохранить его целиком, **не создавать автоматически02/03 и не запускать обучение**. Сообщить конкретный existing-attempt status по имеющемуся контексту и перейти к следующему явно назначенному registry ID; если назначен только AST — `END`. Новый attempt может быть указан отдельным последующим заданием, а не выбран в попытке обойти уже завершённый/частичный запуск. Нельзя запускать повторно arm, уже завершивший обучение. Чужое изменение HEAD не означает повтор: source pin остаётся указанным. Если ровно нужный source/package недоступен после конкретной попытки, записать `PREPARATION_BLOCKED` с командой/ошибкой; не чинить production и не превращать это в отрицательный учебный результат. Не требовать нового GitHub разрешения, когда доступ уже авторизован пользователем.

## 8. Готовый промпт исполнителю

> Выполни только AST-SIB-CACHE-01 из этой карточки, если текущая задача уже перешла к реализации/запуску. В документном проходе ничего не запускай. Работай с моделью и настройками по умолчанию; отчёт по-русски. Применяй точные десять skill-файлов из паспорта с приоритетом свежих правил Любы. Source pin0b76, read-only SourceCore, короткий WorkCore C:\x\ast01, native A/B paths, net9/Roslyn4.12, три Compile файла, две формы учебного C# и фиксированная16-query панель уже заданы: не ищи другой проект, модель, данные или место отчёта. A копирует настоящий StructuralKnn; B добавляет только scoped sibling cache внутри TrainFromSource. Четыре native обучения суммарно, без прогрева/повторов/LLM-generation; одинаковые исходные bytes, k5, roles/parents/ordinal/caps/codec/voting. Соблюдай таймауты prepare45с/build180с/каждыйarm60с/compare15с и прекращай только своё process tree при timeout. Измерь полный TrainFromSource и его настоящую память до/после, не отдельный cache-loop. Любое измеренное полезное улучшение — успех по показателю и домену без требования роста accuracy; все остальные изменения и ухудшения покажи рядом. Single cold time не выдавай за устойчивый speedup, эквивалентность learned state не объявляй при расхождении. Собери только Runner.csproj; без full CI, Main500, production fixes, удаления старых экспериментов или автоматического commit/push. Protected docs/plans/replanning-possible-diffs остаётся read-only; результат — только разрешённый possibleDifs/runs существующего Planning. При конфликте используй ровно описанные правила, ничего старого не удаляй. Зафиксируй отчёт и candidate-fix для решения пользователя только пока эта задача активна; не продолжай цепочку внедрения сам. При команде «следующий» немедленно прекращай старую задачу и переходи к явно назначенному следующему ID: без дописывания её отчёта/registry, review, hash checks и новых запусков. При «остановись немедленно» прекращай все действия; допустимо только необходимое прекращение собственного активного процесса и его детей, затем никаких tool calls/cleanup. Завершённый удачный показатель не требует повторного опыта или исправления продуктовых тестов для признания успеха.

**Согласование параллельной дельты:** при поручении объединённой программы `AST.next_id=NG01`, затем `NG01.next_id=IDENTITY-M1-01`, затем KN-router и END. При одиночном AST по-прежнему END. Точная внешняя карточка NG01, её место и unchanged output — в [12-CONCURRENT-RECONCILIATION.md](12-CONCURRENT-RECONCILIATION.md). Это дополнение меняет только документный маршрут, не AST factor или бюджет.
