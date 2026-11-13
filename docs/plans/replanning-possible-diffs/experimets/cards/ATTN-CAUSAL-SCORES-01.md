# ATTN-CAUSAL-SCORES-01 — не считать будущие QK scores внутри настоящего обучения

<!-- EXECUTOR-CONTEXT-120:START -->
## Вход и самопроверка перед новым заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Адресная граница CORE:** Kernel parity проверить на назначенном реальном learning пути: независимые tensors/gradients/updates и ненулевой правильный readout. Отрицательный контроль обнаруживает возврат ошибки, положительный сохраняет lawful путь; isolated0→0 не качество обучения.

**Исходный адрес из паспорта:** `03.TransformerModel/Layers/CausalSelfAttention.cs`; роль этого файла и фактически достигнутый native/copy путь сверить по собственному runner карточки. Примеры120 не добавляют чужие fixtures/пороги к её набору.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->

<!-- BEGIN result-navigation-2026-10-27 -->
**Учёт исполнения27.10.2026 — RESULT_RECORDED:** [отчёт 1](<C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01/a001/results/report.md>).
Это ссылки на сохранённые результаты, не новый прогон и не общий вердикт успеха всей карточки. Нижний предварительный протокол сохранён; его старое NOT_RUN не отменяет найденный run. Более поздняя новая рецептура не считается выполненной лишь из-за исторического отчёта. Повтор исходного сценария этим обновлением не назначен.

**Продолжение:** [единый индекс и новая очередь Q](00-execution-index.md#next-contract). Наличие открытых исправлений не означает отсутствия новых опытов. Перед любым повтором сверить exact ID/попытку с отчётом; не запускать уже завершённый run и не перезаписывать его output. При STOP/NEXT Любы немедленно прекратить предыдущую работу; не продолжать её отчёты, retries или проверки вместо нового ID.
<!-- END result-navigation-2026-10-27 -->



<!-- BEGIN shared-start-binding-2026-10-21 -->
## Сохранённый старт: h64-causal-scores/v1

**Действующий consumer-контракт:** [общее восстановление33](../33-shared-learned-starts.md), family `h64-causal-scores`. Этот раздел заменяет повторную common preparation во всех командах и промптах ниже. Consumer выполняет **0 common updates,0 дополнительных common readouts**; исходные arm updates/readouts и единственный фактор сохраняются.

- Данные этого семейства: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-causal-scores/v1/data.source.jsonl` и `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-causal-scores/v1/data.material.json`. Заданный источник: `C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-21-learning-delta-26332500/neural/ATTN-CAUSAL-SCORES-01.md`; рецепт: T+,T- bytes noLF;200cycles;stream=frozen-transpose-probe-v1;CS01-private-identities.
- Читать только `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-causal-scores/v1/bundle.json`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-causal-scores/v1/S0.weights.bin`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-causal-scores/v1/S0.optimizer.bin`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-causal-scores/v1/S0.rng.json`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-causal-scores/v1/preparation.json`; generation/tokenizer/authority/source manifest — там же по33. Missing/partial/unqualified → конкретный SHARED_START_UNAVAILABLE, без самостоятельного обучения или другого checkpoint.
- Единственный producer: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01`, A-семантика [назначенной карточки](ATTN-CAUSAL-SCORES-01.md); его конечный бюджет 400updates/404Forward/400Backward оплачивается один раз **отдельно**. Existing source для экспорта: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01/a001/results/common`.
- Будущий экспорт без model calls: `pwsh -NoProfile -File "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-causal-scores/v1/Export-Existing.ps1" -ContractPath "C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/reconciliation/shared-learned-starts-2026-10-21.json"`.
- Будущий однократный producer, только если он прямо включён в порученную программу: `pwsh -NoProfile -File "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-causal-scores/v1/Prepare.ps1" -ContractPath "C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/reconciliation/shared-learned-starts-2026-10-21.json"`. Это заданный интерфейс, ещё не реализованный этим документным проходом; consumer не вызывает его автоматически.
- Восстановление: S0 weights/RNG и полный SFT optimizer (m/v/step) из bundle. Проверить named weights/keys/shapes, полный optimizer state и native RNG identity; construction/ApplySeeded только **до** load. Только DPO consumer заново создаёт native admission/lease после load, без переноса из producer; SFT-only MASK/MOMENT/H64/SILU/CS01 не создают DPO lease.
- Единственный consumer entry остаётся `powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01/Run.ps1 -Attempt a001`; его будущий wrapper вместо common Train читает fixed shared bundle, сохраняет его path/digest в study и восстанавливает A/B. R=`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01/a001/results`; в R/common разрешена неизменная входная копия shared snapshot для прежних внутренних путей. Она не создаёт preparation.claim/новые losses/updates. Reuse cost — чтение/restore, historical producer cost отдельно. Нельзя задним числом переписать claim уже выданного опыта.

Старые common-рецепты, методы подготовки и суммарные1000/400+arm budgets ниже теперь описывают **только однократного producer плюс consumer**, а не расход каждого consumer. Старый child `--prepare-learned`/prepare не вызывать из consumer wrapper. Все tools и ожидание — по31; никаких exporters/manifest/scripts «пока ждём». Артефакты и scripts здесь не созданы и не проверены запуском.
**Сохранение оплаченного state:** сначала независимо записать назначенные weights, optimizer, RNG и имеющуюся qualification; затем typed validation/readback; COMPLETE последним по33. Ошибка проверки weights не должна пропускать запись остальных уже доступных состояний. Новых Train/Forward и периодических снимков не добавлять; partial сохранять.

**Точный weights API для этой карточки:** `cfg` — фиксированный ModelConfig этой карточки; LoRA rank4, targets строго `q_proj,v_proj`. Producer: `model.SaveCheckpointToStream(stream)` на новом `FileStream(path,FileMode.CreateNew,FileAccess.ReadWrite,FileShare.None)`, изначально Length=0/Position=0; `Flush(flushToDisk:true)`. Затем независимо записать остальные state/receipt по33 и только после этого открыть сохранённый weights для чтения. Установить Position=0; `var checkedArtifact = LlmModel.ValidateFullCheckpointArtifact(stream,cfg,new FullCheckpointLoraTopology(4,new[]{"q_proj","v_proj"}))`; требовать `checkedArtifact.IsSuccess && checkedArtifact.Value==true`. Если в конкретном рецепте уже назначен maximumArtifactBytes, передать его неизменным (CS01:8388608). Перегрузка без topology недопустима для этого LoRA snapshot. Перед native `freshModel.LoadCheckpoint(stream)` снова Position=0 либо открыть новый read stream; construction/ApplySeeded только до load. Проверить назначенные named keys/shapes/value bits. Равный размер файла, повторное чтение самого файла или снижение loss не доказывают равенство потерянной live memory. В consumed/recovery случае такое равенство помечать UNKNOWN, не придумывать. Эта запись уточняет уже назначенные операции, не добавляет model calls.

<!-- END shared-start-binding-2026-10-21 -->


**Маршрут общей порученной программы:** ATTN-CAUSAL-SCORES-01 → [FILE-STATIC-RECEIVER-01](FILE-STATIC-RECEIVER-01.md); для отдельного назначения одного ID — END. Текущий STOP/NEXT выше маршрута.

<!-- BEGIN mandatory-learning-and-recovery-2026-10-21 -->
## Обязательные правила для карточки и любого её промпта

**Найти tool и дождаться своей операции — обязанность исполнителя.** До выдачи команды прочитать [контракт31](../31-tool-discovery-and-wait.md) и указанные там реальные skills/references; по каталогу текущей среды найти launch и wait/resume/status, прочитать их схему и сохранить provider, handle, start time, H/R и deadline. Не выдумывать Kiro API по Codex-примеру, не просить пользователя выбирать инструмент или запускать команду вместо исполнителя.

**Ожидание не равно бесполезному polling.** Одна выдача entry; затем штатный bounded wait/resume того же session/cell/process ID до terminal exit или исходного experiment deadline. Нормальный wait можно повторять при silent/running. Два быстрых неблокирующих чтения без прогресса требуют перейти в wait; если он неисправен/ответы противоречат — независимое чтение H/R/claim/exit и своего PID+start. Не вводить Start-Sleep в занятый терминал. Предел одного ожидания≤60с — не предел эксперимента; yield/RPC timeout не доказывает exit/kill/нулевой расход. После диагностики живой процесс продолжают ждать. Отсутствие log не доказывает, что запуска не было. Не выдавать entry заново «синхронно на25минут», не подбирать results-NN, не удалять claim/partial, не сокращать TRAIN/steps до двух минут ради tool. При UNKNOWN сохранять неизвестный расход и разбирать существующий запуск. STOP/NEXT Любы немедленны.

**Не завершать ход на ожидании и не придумывать работу.** ISSUED/WAITING → штатный блокирующий/event wait исходного handle → terminal/exit → чтение назначенных raw → отчёт. Финальное «жду уведомления / доложу потом» до результата запрещено; обещание auto-notification не доказывает возобновление. Не создавать фоновый waiter/monitor/watchdog, циклы polling, progress-события каждые N шагов, summary scripts, manifests или другую работу «пока ждём»; не запускать агентов или следующую карточку. Обычное ожидание не требует заполнения паузы работой. Соблюдать provider no-poll/no-sleep. Проверить настоящий wait/continuation в каталоге до launch; при отсутствии доказать конкретный блокер, сохранить handle и INCOMPLETE, без выдуманного API или relaunch. STOP/NEXT Любы немедленны. Полный порядок и исключения — [контракт31](../31-tool-discovery-and-wait.md).

**Без действительного обучения задача не выполнена.** Для каждой новой экспериментальной программы, включая diagnostic/reader/restore, обязателен назначенный реальный учебный участок: original файл/метод, learner→изменённое состояние→обычный consumer, точные TRAIN/EXAM, before/after и конечный бюджет. Сохранённый обученный S0 является стартом, но не заменяет этот участок. Для retention/restore навык проверяется до вмешательства; для acquisition допустим cold start с приобретением поведения. Ноль обновлений допустим у отдельного контрольного плеча или reader после общего Train, но не у всей новой программы. Unit/BPE действительно обучают память/правила; neural updates=0 у них не означает отсутствие native Train. Updates, loss, hash и случайный верный токен сами по себе не доказывают навык; 0→0 не является учебным результатом, частичные токены не объявляются целым ответом. Исторический безучебный receipt сохранить; его повтор получает отдельный назначенный рецепт по оригиналу и новый output, а не считается завершённым новым опытом. Уже выполненный учебный участок этой же попытки не повторять ради счётчика после сбоя отчёта; продолжить его сохранённые stages. Отдельная unit-проверка готового receipt не является новым экспериментом обучения. Полный охват карточек и адресные рецепты — [37](../37-full-catalogue-learning-coverage.md).

**0 → 0, отсутствие обучения или неполное A/B — не завершение эксперимента.** Раздельно записать execution_status, learning_validity и hypothesis_status; при невалидности учебная цель OPEN. Сохранить сырые свидетельства и продолжить разрешённое исправление постановки, а не только написать «невалидно» и отчитаться об успехе. Не искать другой checkpoint/модель/host, не менять seed/фактор, не увеличивать steps до PASS: точный исправленный рецепт назначает автор карточки до новой попытки. Старые опыты не удалять. Поздняя ошибка сохранения не обнуляет состоявшиеся updates; неизвестное число — null, невыполненное плечо — «не измерено».

Любое полезное измеренное улучшение в **валидном учебном сравнении** — успех своей метрики/домена; рост accuracy необязателен, регрессии рядом. Диагностика невалидной постановки не закрывает учебную цель. Default-модель исполнителя без override; точные H/R/entry ниже. Отчёт — русский R/report.md. Текущая правка документов не разрешает runtime. Прямые STOP/NEXT Любы немедленны и при открытой цели.

Полный конечный порядок диагностики и критерии: [контракт30](../30-failure-recovery-and-learning-validity.md). При конфликте со старым текстом ниже действует этот блок.
**Состав данных ограничивает вывод.** До запуска автор задаёт точные TRAIN/EXAM/CONTROL, покрытие классов, роли отрицательных примеров, семьи/пересечения и конкретный контроль постоянного ответа. Одноклассовый TRAIN и выигрыш на его единственной метке не доказывают различение маршрутов; updates/confidence не заменяют этот контроль. Разные опыты не устанавливают причину регрессии состава данных. Измерять любой исход, не предписывать ухудшение; набор и budget исполнитель не меняет сам. Полное обязательное правило — [состав данных и допустимый вывод](../32-training-set-and-inference-contract.md).

**Сохранённый учебный результат используется без повторного выбора.** В уже разрешённой фазе исполнения пригодные сохранённые веса/обученное состояние имеют приоритет: восстановить их и продолжить назначенный опыт, не спрашивая «A: использовать результат или B: обучить заново». Поздняя ошибка проверки, сериализации или отчёта сама по себе не основание повторять Train. Сначала прочитать актуальные receipts и установить, не выполнены ли recovery/плечи уже; состоявшуюся работу не переиздавать. Проверять только необходимую для следующего шага совместимость и учебную квалификацию, используя имеющиеся данные; недостающие показания получать лишь по явно ограниченному recovery-рецепту, без optimizer updates и без притворного восстановления отсутствующих moments/RNG. Если пригодность доказана, выбрать reuse автоматически. Если объективно отсутствует необходимое состояние, назвать конкретный технический пробел; не предлагать повторное обучение «для чистоты», не просить пользователя снять guard и не выдавать размер файла/падение loss за доказательство выученного навыка. Текущая документная фаза запусков не разрешает.

**Реализация не отменяет контракт.** «Код не читает RNG/optimizer/проверку» означает отклонение, а не разрешение его игнорировать. До следующего model call исправить реализацию в уже разрешённом scope либо применить только явно назначенный в карточке совместимый режим восстановления; не придумывать исключение, не просить снять guard и не переносить weights-only DPO-исключение на SFT continuation.

<!-- END mandatory-learning-and-recovery-2026-10-21 -->


**Current status: AUTHOR_WORK_ORDER_ONLY / NOT_IMPLEMENTED / NOT_RUN / NO_MEASURED_GAIN.** Эта карточка задаёт будущий маленький эксперимент. Ни исходники host, ни checkpoint S0, ни результаты этим документом не созданы. Команды ниже — **будущий интерфейс, который автор должен реализовать**, а не проверенный запуск, доступный сейчас.

Evidence: [нейронный аудит](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-21-learning-delta-26332500/neural/01-neural-causal-score-audit.md>). Core `26332500f76ea8345256c73ecfd44c0023b7e883`; planning dedup `14308ff181c5a93d837d92b85b2f35c41b863565`. Новизна ограничена поиском 976+406 Markdown и сравнением ближайших факторов. Защищённый `docs/plans/replanning-possible-diffs` read-only.

## 1. Цель, приоритет и один фактор

Тема — **язык программирования**. Задача — знак результата короткого C# выражения. Learner — реальный native Transformer с q/v LoRA, SFT next-token CE и AdamW. Один block, два адаптерных slot, четыре trainable arrays; Unit/Flow-классификатор в этом опыте не используется. Это не DPO, не kNN и не искусственный классификатор по строковому правилу.

**Гипотеза:** при одинаковых приобретённых знаниях и одинаковых последующих обновлениях пропуск scores будущих causal позиций уменьшит стоимость TRAIN. Рост accuracy не обязателен. Изменение может оказаться бесполезным по времени: small T, неизменённые backward/allocations, JIT и прочие расходы сохраняются.

| A | B | Что одинаково |
|---|---|---|
| Native `Q @ Kᵀ` считает все столбцы | Только в обычном square causal QK forward вычисляются `column<=row`; верхние ячейки затем, как прежде, затираются mask | Веса S0, optimizer moments+step, RNG, model config, bytes, порядок, бюджет, loss, full backward, clipping, optimizer и readouts |

Приоритет: сначала существующие blockers настоящего обучения, затем конфликты карточек и назначенные пользователем обновления; этот прямой TRAIN cost-factor идёт после них и перед опытами, не связанными с обучением. Stable ID важнее номера строки. По завершении одного опыта самостоятельного NEXT нет: только явно выбранный пользователем/общим registry stable ID. Пользовательский приказ NEXT сразу заменяет текущую работу; STOP прекращает её.

## 2. Разрешённое место, автор и runtime

**Сейчас — только документация.** Будущий автор работает на доступной модели сессии по умолчанию, без override модели/reasoning. Его ограниченный результат — host source-copy и возврат автору registry; запуск требует отдельного задания на этот ID. Production integration не входит ни в авторство, ни в эксперимент.

Устанавливаются пути, а не предлагается искать их:

| Имя | Точный будущий путь |
|---|---|
| E — experiment artifact root | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01` |
| J — проект | `C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/CS01/Runner.csproj` |
| Entry point | `E/Program.cs` |
| Native source-copy | `E/src/<36 paths from CS01-SOURCE-COPY-INPUTS.json>` |
| Wrapper / comparator | `E/Run.ps1`, `E/compare.py` |
| Exact attempt root | `E/a001` |
| R — results | `E/a001/results` |
| Assembly | `E/a001/bin/ATTN-CAUSAL-SCORES-01.dll` |
| build intermediate | `E/a001/obj` |

На Windows это короткий `CS01`, а не полный ID в каждом вложенном каталоге. Автор проверяет максимальный нормализованный путь до build; если путь всё равно выходит за фактически поддерживаемый лимит, возвращает точный отказ. Не переименовывать каталог на лету и не трогать чужой старый run.

Автор берёт **ровно 36 файлов из корня pinned core**, по [CS01-SOURCE-COPY-INPUTS.json](../../2026-10-21-learning-delta-integration/CS01-SOURCE-COPY-INPUTS.json). Исторический LFT01 manifest даёт только список имён; его изменённый `Linear.cs` и старый `CoreCommit=62bcc...` не копируются. `Linear.cs` должен быть native current, без `ExperimentReuseFrozenTranspose`. До правки сохранить свежие blob/SHA256; после правки — source manifest и `one-factor.diff`. Коммитируемые источники продукта, все другие hosts, старые результаты и защищённая папка неизменны.

J создаётся с точным порядком SDK imports и явными настройками, поскольку родительские props отключены. Ниже **спецификация будущего csproj**, не созданный файл:

```xml
<Project>
  <PropertyGroup>
    <ImportDirectoryBuildProps>false</ImportDirectoryBuildProps>
    <ImportDirectoryBuildTargets>false</ImportDirectoryBuildTargets>
    <ArtifactRoot>$(MSBuildThisFileDirectory)../../../../experiments-v2/RP01-MICRO/CS01/</ArtifactRoot>
    <BaseOutputPath>$(ArtifactRoot)a001/bin/</BaseOutputPath>
    <BaseIntermediateOutputPath>$(ArtifactRoot)a001/obj/</BaseIntermediateOutputPath>
    <MSBuildProjectExtensionsPath>$(ArtifactRoot)a001/obj/</MSBuildProjectExtensionsPath>
  </PropertyGroup>
  <Import Project="Sdk.props" Sdk="Microsoft.NET.Sdk" />
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <OutputType>Exe</OutputType>
    <AssemblyName>ATTN-CAUSAL-SCORES-01</AssemblyName>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>13.0</LangVersion>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="$(ArtifactRoot)src/**/*.cs" />
    <Compile Include="$(ArtifactRoot)Program.cs" />
  </ItemGroup>
  <Import Project="Sdk.targets" Sdk="Microsoft.NET.Sdk" />
</Project>
```

Под `src` только36 manifest files. Нет product ProjectReference, пакетов внешнего ML и общего experiment dispatcher. Изменения фактора допустимы только в копиях `CausalSelfAttention.cs`, `TensorOps.cs`, `SimdOps.cs`. Program/wrapper/comparator — экспериментальная инфраструктура. Другие33 native files остаются bit-identical manifest.

## 3. Точный алгоритм B

1. В source-copy `CausalSelfAttention.ScaledDotProduct` добавить static experiment switch, false по умолчанию. Только строка вычисления scores выбирает native `TensorOps.MatMul(q,kT,ct)` в A или новый `TensorOps.MatMulCausalScores(q,kT,ct)` в B. Cached `ScaledDotProductWithPast` и все остальные MatMul не меняются.
2. Новый метод принимает только rank4 inputs `[B,H,T,D] × [B,H,D,T]`, одинаковые batch/head, `T>0`, конечные вычисляемые исходные параметры. Нельзя вызывать его как универсальное матричное умножение. Исходные shape/cancellation проверки и dense result layout сохраняются. Если потребитель уже не immediate scale+mask, автор обязан вернуть конфликт, а не расширять назначение метода.
3. Скопировать текущий `MatMul` путь в узкий метод: тот же `Transpose2D` B, тот же fresh dense result, те же batch offsets. Из `TensorOps` вызывать **internal static** `SimdOps.MatVecRowCausalUnpublished`, который сохраняет native `ValidateMatVecArguments` и делегирует своему **private static** `MatVecRowCausalCore`. Только в private core заменить предел `for (col=0; col<bCols; col++)` на `col<=row`. SIMD loop и scalar remainder для каждой оставшейся клетки совпадают с исходными **включая порядок накопления**. Верхние клетки fresh array остаются `+0`, затем unchanged scale и mask записывают −∞. Их не выдавать наружу как полное произведение.
4. Backward нового метода — без изменений из native `MatMul`: те же edges/RequiresGrad, gradA helper, gradB triple loop, accumulation, allocations. Не пропускать нулевые gradient products и не менять summation order. Mask backward продолжает передавать только lower triangle. Softmax и `weights @ V` остаются native.
5. Сохранить проверку cancellation на каждой строке и внутри длинного dot product, текущую семантику unpublished buffer. Не удалять production protections. Число progress events может уменьшиться с числом выполненных клеток; его не утверждать равным A. Progress/counters — не показатели усвоения.
6. В source-copy `ScaledDotProduct` общие для A/B scalar counters считают eligible forward calls, T, dense candidate dot count и actually computed dot count, один increment на вызов, без `Interlocked`/логирования по каждой клетке. В B computed=`B*H*T*(T+1)/2`; в A=`B*H*T*T`. Счётчик не заменяет измерение времени. Reset только вне измерительного окна. Доступ к q/k values для нового validation pass в timer не добавлять: finite проверяется по полученному state и сохранённым outputs; baseline и B сохраняют native runtime validations.

Точная граница доступности в будущей source-copy: `TensorOps` предоставляет `public static Tensor MatMulCausalScores(Tensor a, Tensor b, CancellationToken cancellationToken)`. `CausalSelfAttention` вызывает этот public метод. В `SimdOps` объявляются `internal static void MatVecRowCausalUnpublished(float[] a, int aOffset, int aRows, int aCols, float[] transposedB, int transposedBOffset, int bCols, float[] result, int resultOffset, CancellationToken cancellationToken)` и **private static** `MatVecRowCausalCore` с теми же параметрами/return void. `TensorOps` имеет доступ к internal wrapper в той же TensorCore assembly; прямого вызова private core из другого класса нет. Это повторяет границу existing `MatVecRowUnpublished` → `MatVecRowCore`, сохраняя unpublished-buffer ownership.

**Нельзя включать вторым фактором:** сокращение T/PAD, selected-head rows, иной tokenizer, transpose strides, GQA view, frozen weight cache, softmax fusion, новое выделение памяти, clipping, response-only SFT или иной scheduler. Не делать «референсную» NumPy/C# игрушку вместо native Trainer.

## 4. Model, tokenizer и фиксированный набор

| Параметр | Значение |
|---|---|
| Model | Vocab256, Context64, Hidden64, Layers1, Qheads4, KVheads2, HeadDim16, FFN176, RopeTheta10000, RmsNormEps1e−5 |
| Initialization | `Initializers.UseDeterministicSeed(62001UL)` только при construction |
| LoRA | `LoraApplicator.ApplySeeded`, rank4, alpha8, dropout0, targets `q_proj`,`v_proj`, seed62002UL |
| Stream | `frozen-transpose-probe-v1`; заимствована точная fixed preparation identity известной LEARN-SANITY fixture, а не её cached Linear patch |
| Optimizer | Native AdamW beta1=.9, beta2=.95, epsilon1e−8, weightDecay0 |
| Trainer | Native `TrainSupervisedNextTokenStep`; Batch1, SeqLen64, GradAccum1, Clip1, Warmup0, TotalSteps404, MaxLr=MinLr=.001, WeightDecay0; weight=1 |
| Token IDs | Exact UTF-8 bytes 0..255; никакого BPE Train, merge, LF, BOS, EOS, PAD или normalization |
| Source objective | Полная `Prompt+Answer`, native shift; ответ одна позиция из 21 targets, prompt loss намеренно одинаков в A/B |

| Split | ID | Prompt — точные bytes, без кавычек | Answer |
|---|---|---|---|
| TRAIN | T1 | `C# result sign: 9-4 =` | `+` |
| TRAIN | T2 | `C# result sign: 4-9 =` | `-` |
| EXAM | E1 | `C# result sign: 8-3 =` | `+` |
| EXAM | E2 | `C# result sign: 3-8 =` | `-` |

Каждый prompt 21 byte, combined training sequence 22 byte. `+` — ID43, `-` — ID45. Весь TRAIN одного предмета; положительный и отрицательный знак — разные классы внутри него. EXAM не участвует в updates или выборе checkpoint. Это очень близкие шаблоны одного семейства, поэтому результат не считается доказательством source-disjoint обобщения или умения писать C#. 1/2 совпадает с constant-label baseline; это обязательно показывается.

Не добавлять новые темы, отрицательные «домены», перемешивание, новые seed, задачи из чужих карточек или дополнительные balanced runs. Фиксированный ручной split содержит оба класса в TRAIN и EXAM; исполнять его буквально, а не автоматически запускать чужой StratifiedSplit или scale-up из общего skill.

## 5. Рецепт producer S0; consumer получает его из shared bundle

Готового проверенного S0 для этой новой карточки нет. Существующий `LEARN-SANITY-PROBE` служит обоснованием малой acquisition fixture, но его JSON не является weights+optimizer checkpoint. Этот документ **не обещает**, что 400 updates обязательно дадут новый правильный ответ на актуальной сборке.

Preparation использует A semantics. Построить W0 с указанными model/seeds/stream; один native prompt-only readout T1,T2 без update. Обучить ровно 200 циклов T1,T2 = **400 настоящих native updates**. Никаких промежуточных EXAM/readouts, раннего выбора лучшего S0 или наращивания бюджета. Затем второй native prompt-only T1,T2 readout. Сохранить оба массива exact/NLL/rank/logits, 400 losses, количество updates и реальное изменение trainable weights.

`learning_qualified=true` только если хотя бы одна из фиксированных T1/T2 перешла из W0 exact=false в S0 exact=true. Это приобретение полного однотокенного ответа на указанную строку. Для cost-проверки достаточно такой ограниченной acquisition; 1/2 не означает усвоение общего закона, 2/2 различимо с constant-label baseline. При 0→0, отсутствии такого перехода, NaN или отсутствии actual parameter write — `PREPARATION_COMPLETED_LEARNING_UNQUALIFIED`; A/B не запускаются, цель проверки открыта. Margin/NLL alone не переименовывают это в валидный минимальный learning experiment. Сохранить произошедшие затраты и причину, без повторов/автоNEXT.

Только назначенный producer после step400 сохраняет единый bundle в `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-causal-scores/v1`; consumer не пишет `R/common`:

- `S0.weights.bin`: native `SaveCheckpointToStream` на `FileStream(FileMode.CreateNew,FileAccess.ReadWrite,FileShare.None)`. Flush(flushToDisk:true); перед validation независимо записать optimizer/RNG/preparation по33. Затем открыть weights для чтения, Position=0 и вызвать **typed** `LlmModel.ValidateFullCheckpointArtifact(stream,cfg,new FullCheckpointLoraTopology(4,["q_proj","v_proj"]),maximumArtifactBytes:8388608)` и требовать `IsSuccess && Value==true`. Generic overload без topology — base-only и для этого файла не подходит. Сохранить полный именованный weight state в snapshot JSON для сравнения, не только hash. Обычный `SaveCheckpoint(path)` с overwrite здесь не нужен.
- `S0.optimizer.bin`: bindings из `GetCheckpointParameterEntries`, только actual attached q/v AdapterParameters по reference identity; четыре уникальных native key и shape. `ExportPortableState(bindings).ToBytes()` → CreateNew. `AdamWPortableState.FromBytes` читает этот формат. Step400; должны быть все четыре m/v entries, не выдуманные нулевые defaults.
- `S0.rng.json`: `JsonSerializer.SerializeToUtf8Bytes` полного native `LlmModelRandomStateSnapshot`, CreateNew. **Не** `RepositoryOwnedDpoRandomStateFile.WriteCreateNew` с частным именем. Topology identity = lowerhex SHA256 UTF8 literal `CS01|V256|H64|L1|Q4|KV2|FF176|C64|R4|q_proj,v_proj`. LoRA configuration identity = lowerhex SHA256 UTF8 literal `CS01|alpha8|dropout0|seed62002|frozen-transpose-probe-v1`. Эти две фиксированные private experiment identities не утверждают production topology authority. `CaptureRandomState(topologySha,loraSha,400)`; все поля snapshot сохраняются.
- `preparation.json`: core/source digest, config, fixture bytes, seeds/stream, completed_updates400, exact_by_id_W0/S0, acquired_ids, `learning_qualified`, failure_reason, 400 native losses, costs, всех actual run counters и refs на три артефакта. Недоступное число=`null` с причиной, не0.

У каждого A/B новый model: construction+ApplySeeded **до** load, затем native `LoadCheckpoint` и никаких повторных Initializers. Проверить все named weights, shapes, float bits с S0. Новый AdamW: `TryImportPortableState(bindings,FromBytes(...))==true`; повторный Export должен иметь точно те же key/shape/m/v bits и step400. Новый Trainer имеет local counter0, что допустимо при выбранном constant LR; optimizer step нельзя сбрасывать. `RestoreRandomState(snapshot,new LlmModelRandomStateIdentity(...))`; повторный Capture(step400) сравнивает FormatVersion/Algorithm/AdapterKey/StreamDomain/InitializationSeed/DropoutSeed/ConsumedCursor/оба identity hash/RandomStateStep. После construction никаких случайных sample/generate вызовов. В S0 и обоих плечах полный набор из четырёх trainable arrays, остальные frozen.

## 6. A/B и budget без скрытых вызовов

Consumer wrapper читает квалифицированный shared bundle h64-causal-scores/v1 без preparation, потом последовательно A, затем B. Каждый arm получает тот же S0, optimizer400 и RNG. В A switch=false, в B switch=true, включая before/after readout. Четыре строки панели T1,T2,E1,E2 на before, затем четыре updates T1,T2,T1,T2, затем та же панель after. После четвёртого native update optimizer step404. Полученные tensors сохраняются для расчёта всех метрик; ради дополнительной метрики ещё один Forward запрещён.

| Работа | Fixed максимум |
|---|---:|
| Только producer: common SFT updates/Forward/Backward | 400 / 400 / 400 |
| A SFT updates/Forward/Backward | 4 / 4 / 4 |
| B SFT updates/Forward/Backward | 4 / 4 / 4 |
| Только producer: common диагностические Forward | 4 — T1,T2 на W0 и S0 |
| Arm диагностические Forward | 16 — по четыре строки before/after A и B |
| Producer+consumer: optimizer updates / training targets | 408 / 8568 |
| Producer+consumer: model Forward / Backward | 428 / 408 |
| Только consumer: updates / targets / Forward / Backward | 8 / 168 / 24 / 8 |
| DPO / Generate / external teacher / new BPE Train | 0 / 0 / 0 / 0 |
| Seeds / retries / скрытый warmup / additional order B→A | один фиксированный набор / 0 / 0 / 0 |

TRAIN tensor shape всегда `[1,4,21,16]` для Q. Проверка reachability должна увидеть **4 ordinary score forwards** на arm. Расчёт ожидаемых QK dot counts: A=`4*4*21*21=7056`, B=`4*4*21*22/2=3696`, пропущено3360; это счёт математических клеток, не benchmark результата. Обе стороны имеют84 native training targets и4 updates. Readout counters и common counters отдельно.

Consumer runtime deadline1800s: build≤180s, preparation0, A≤120s, B≤120s, comparator/report≤60s; отдельный producer имеет deadline по33; сумма ограничена общим deadline. Deadline не равен короткому tool-call timeout. После yield продолжается ожидание **того же** process/session handle. Молчание в stdout не объявляется падением.

## 7. Измерения и смысл before/after

TRAIN cost — цена перехода A0→A1 и B0→B1, то есть четырёх реальных native updates. Это не «стоимость до=0». `Stopwatch`, `GC.GetAllocatedBytesForCurrentThread`, process CPU (`Process.Refresh` перед чтением), GC0/1/2 — строго вокруг четырёх вызовов native Trainer. Construction, load, snapshots, hashes, diagnostics и serialization снаружи. Никаких `GC.Collect`, hidden JIT warmup или extra forward.

Before/after diagnostic readout cost может быть отдельной строкой, но не заменяет TRAIN cost. PeakWorkingSet процесса писать отдельным context field: он не равен точной пиковой памяти train window. Common400 cost отдельный и никогда не приписывается A или B.

Для каждой T1/T2/E1/E2 на A0/A1/B0/B1 сохранить **тот же** prompt, gold ID, predicted ID, exact полного one-byte ответа, NLL=`logsumexp(last256logits)-gold_logit`, strict gold rank=`1+count(logit>gold)`, tie-ordered rank отдельно (`+count(equal & id<gold)`); argmax ties по minID. Полные logits всех21 позиций сохранить как float32 bits за cost window; last256 нужны отчёту. Новый forward для hashes/logits не разрешён.

Показать две разные дельты:

- `learning_delta_A=A1−A0` и `learning_delta_B=B1−B0` по точным правильным ответам, NLL, rank, retained acquired IDs. При разных знаках «лучше» явно указывается направление.
- `treatment_delta=B−A` для TRAIN allocations/time/CPU и, отдельно, `learning_delta_B−learning_delta_A`. Изменение loss в процессе SFT нельзя заменить сравнением разной панели до и после.

Эквивалентность этого фактора ожидается bit-exact на одной машине/runtime: A0=B0 и A1=B1 по model state, full logits, RNG, optimizer keys/m/v/step; четыре online loss bits равны; frozen weights неизменны; trainable weights реально изменились после step400; все values конечны. Comparator читает уже записанные arrays, не вызывает learner. Если bits расходятся, не менять post-hoc tolerance: зафиксировать `NUMERICAL_PARITY_FAILED` и все дельты; полезное измеренное изменение сохранится как наблюдение, но однофакторная семантическая эквивалентность не доказана.

**Правило успеха:** любое измеренное улучшение полезного показателя в валидном опыте считается успехом **по этому показателю и этой теме**; рост accuracy не обязателен. Рядом все ухудшения, включая точность, NLL/rank, время, CPU, allocations, GC и потерю приобретённой строки. Нового minimum-gain threshold нет. Одна A→B пара не доказывает воспроизводимый средний speedup или причину шума/JIT; это ограничение attribution, а не повод скрыть наблюдаемую дельту или самовольно повторять.

При неqualified S0 или незавершённых native updates не писать learning success, даже если отказ был быстрее или margins сдвинулись. Counter уменьшения числа dot products — механическое свойство, а не сам по себе полезный измеренный TRAIN gain.

<!-- BEGIN book-math-pass9-2026-10-23:causal-work -->
<a id="book-math-pass9-causal-work"></a>
## Математическое объяснение результата: не вычислять то, что маска исключит

**Дополнение 23.10.2026.** Этот раздел объясняет опубликованный `CS01/a001`; рецепт и бюджет карточки сохраняются. Первичные данные: [плечо A](https://github.com/lubochka/llm_mvp_core/blob/ff6ed114e49f275da50891b8dd13e9d9288a8692/experiments-v2/RP01-MICRO/CS01/a001/results/A/result.json), [плечо B](https://github.com/lubochka/llm_mvp_core/blob/ff6ed114e49f275da50891b8dd13e9d9288a8692/experiments-v2/RP01-MICRO/CS01/a001/results/B/result.json), [отчёт](https://github.com/lubochka/llm_mvp_core/blob/ff6ed114e49f275da50891b8dd13e9d9288a8692/experiments-v2/RP01-MICRO/CS01/a001/results/report.md).

**Пример для студента.** Первые три слова уже даны, и ученик должен предсказать четвёртое. Для этого представление третьей входной позиции может использовать позиции 1–3; четвёртая и пятая ещё недоступны и закрыты причинной маской. Если сразу пропустить сравнения с этими будущими позициями, остаются те же доступные подсказки и тот же урок, а промежуточных вычислений становится меньше.

**Математика книги и наше применение.** В [§9.8.1, печатные с. 287–288 / PDF с. 323–324](https://github.com/lubochka/xiigen-planning/blob/d7b1704fd681fe0855afeb08ca5234433b5f9f6c/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#display-9-softmax-jacobian) дан якобиан softmax:

$$
J=\operatorname{diag}(p)-pp^\top.
$$

В этом опыте [реальный вызов маски](https://github.com/lubochka/llm_mvp_core/blob/ff6ed114e49f275da50891b8dd13e9d9288a8692/experiments-v2/RP01-MICRO/CS01/src/03.TransformerModel/Layers/CausalSelfAttention.cs) перезаписывает будущие оценки значением $-\infty$. Для конечных допустимых оценок и непустой разрешённой части строки получаем

$$
\widetilde z_{ij}=\begin{cases}q_i^\top k_j/\sqrt d,&j\le i,\\-\infty,&j>i,\end{cases}
\qquad p_{ij}=0\quad(j>i).
$$

Наш вывод из формулы и маски: исключённый компонент даёт нулевую строку и столбец якобиана; кроме того, константа после перезаписи вообще не зависит от исходной оценки. Поэтому $\partial L/\partial z_{ij}=0$ при $j>i$. [Обратный проход самой маски](https://github.com/lubochka/llm_mvp_core/blob/ff6ed114e49f275da50891b8dd13e9d9288a8692/experiments-v2/RP01-MICRO/CS01/src/00.TensorCore/Core/TensorOps.cs) действительно передаёт градиент только разрешённым клеткам. Вычислять исходное скалярное произведение в закрытой клетке не требуется ни для ответа, ни для этого градиента.

**Откуда берётся экономия.** [Единственный фактор](https://github.com/lubochka/llm_mvp_core/blob/ff6ed114e49f275da50891b8dd13e9d9288a8692/experiments-v2/RP01-MICRO/CS01/a001/results/one-factor.diff) меняет предел прямого QK-цикла с всех столбцов на `col <= row`. Наш подсчёт работы для одной квадратной матрицы длины $n$:

$$
n^2\longrightarrow\sum_{i=1}^{n}i=\frac{n(n+1)}2,\qquad
N_{\mathrm{skip}}=\frac{n(n-1)}2.
$$

Например, при четырёх позициях нужны 10 сравнений вместо 16. Это относится к **прямому расчёту квадратных QK scores без KV-cache**. Плотный выходной массив и цикл обратного MatMul сохранены. Новый [§14.5, печатные с. 478–480 / PDF с. 514–516](https://github.com/lubochka/xiigen-planning/blob/d7b1704fd681fe0855afeb08ca5234433b5f9f6c/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0512-0515-ch14-sa-channels-and-cost-accounting.md#section-14-5) полезен здесь как правило раздельного учёта параметров, вычислений и расходов; приведённый треугольный подсчёт — наше применение к коду, не формула этого раздела книги.

**Что измерено на четырёх учебных обновлениях:**

| Показатель | A | B |
|---|---:|---:|
| Вычисленные QK-скалярные произведения, $n=21$, batch 1, 4 головы | 7 056 | 3 696 |
| Пропущенные произведения | 0 | 3 360, или 47.6% |
| Время четырёх обновлений | 110.3624 ms | 85.2964 ms |
| Выделенные байты в этом окне | 13 642 776 B | 13 642 776 B |
| Шаг оптимизатора до → после | 400 → 404 | 400 → 404 |

Сопоставление сырых A/B показывает одинаковые биты четырёх loss, одинаковые выходы до/после, итоговые веса и состояние оптимизатора. Панель ответов осталась 2/4 в обоих плечах до и после; новые правильные ответы этим фактором не получены. Подготовительные 400 обновлений общего старта не входят в эти четыре измеряемых шага.

**Почему время может улучшаться.** Убраны реально выполнявшиеся скалярные произведения, которые не влияли на обучение. В единственной холодной паре A/B наблюдалось уменьшение времени на 25.0660 ms, или 22.7%, при прежнем учебном результате. Плечи выполнялись в отдельных процессах; этот замер не устанавливает устойчивый процент ускорения. Сокращение на 47.6% относится к изменённому QK-циклу, а не ко всему attention, обратному проходу или полному обучению. Меньшее число уроков не проверялось.
<!-- END book-math-pass9-2026-10-23:causal-work -->

## 8. Будущий интерфейс команд — сейчас НЕ запускать

Автор реализует, а исполнитель использует следующую единственную entry command **после отдельного назначения исполнения ATTN-CAUSAL-SCORES-01**:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01/Run.ps1 -Attempt a001
```

Внутри wrapper — следующие будущие CLI, последовательно, не общий dispatcher:

```powershell
dotnet build C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/CS01/Runner.csproj -c Release --output C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01/a001/bin -p:ImportDirectoryBuildProps=false -p:ImportDirectoryBuildTargets=false -p:BaseIntermediateOutputPath=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01/a001/obj/ -p:MSBuildProjectExtensionsPath=C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01/a001/obj/
# Consumer: fixed shared bundle -> verified read-only input copy R/common; no prepare/Train here.
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01/a001/bin/ATTN-CAUSAL-SCORES-01.dll A code-sign C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01/a001/results
dotnet C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01/a001/bin/ATTN-CAUSAL-SCORES-01.dll B code-sign C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01/a001/results
python C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01/compare.py C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01/a001/results
```

`prepare`/`A`/`B` — новая CLI автора, **не** существующая проверенная функция. Direct arm without live wrapper claim отказывает. До build wrapper атомарно создаёт attempt claim с ID, nonce, PID, UTC start, pinned commit и registry stable ID, удерживает эксклюзивный file handle до окончания. До первого constructor preparation — собственный `preparation.claim`; A/B до restore — `training.claim` в своей папке. Existing attempt/claim/result/partial даёт `EXISTING_ATTEMPT`, без overwrite, удаления или auto-a002. Не повторять только из-за build.stdout/status.json или задержки файлов; проверить тот же receipt/process. Нестандартный каталог может назначить только новое явное задание автора registry.

Инструменты исполнитель обнаруживает по **реальным** metadata среды: локальный terminal/process API с return of session/process ID и соответствующий wait/poll. Для functions окружения `exec_command`→`write_stdin`, а если functions.exec вернул `Script running with cell ID`, используется functions.wait. Не путать эти два handle. В PowerShell запуск wrapper идёт синхронно в tracked terminal process; не `Start-Process` без receipt, не detached job, не `Start-Sleep` вместо ожидания результата. Poll/wait≤60s за один вызов, общий deadline выше; между ожиданиями краткий статус. Exit=-1 и yield не считать синонимами завершённого процесса. Нет tool capability — вернуть `EXECUTION_TOOL_UNAVAILABLE`, не сочинять таймаут и не менять budget.

## 9. Отчёт, фиксация кандидата и немедленный переход

Язык отчёта русский. `R/report.md`: до350 слов основного текста + полные таблицы. Порядок: что произошло с реальным обучением; какие именно полезные показатели улучшились и где ухудшились; почему результат ограничен этой темой/fixture; candidate decision. Таблица TRAIN cost A/B/Δ, таблицы T1/T2/E1/E2 A0/A1/B0/B1, acquisition W0→S0, actual call budget, parity и source identities обязательны. Links относительно R: `common/preparation.json`, `A/result.json`, `B/result.json`, `comparison.json`, `execution.json`, `one-factor.diff`, `source-copy-manifest.json`, logs.

`R/execution.json`: PID/handle, start/end/exit, stage durations, source/model/data identity, expected и actual counts, completed/partial, failure reason. `R/comparison.json` — сравнение сохранённых arrays/counters без learner. Существующие результаты не удалять, недостающие числа не восстанавливать по памяти.

Если валидный опыт показал хотя бы одно полезное измеренное улучшение — создать `R/fix-candidate.md`, до120 слов: **какой metric/domain выиграл**, численная дельта, все регрессии, точные три original code paths из раздела3, ссылка на report и статус «кандидат для решения Любы». Если улучшение не измерено — hypothesis остаётся open, candidate успеха не создаётся. Никаких автопереноса в prod, исправления product tests, полного CI или изменения protected directory.

**Обязательный запрет поведения из пользовательского примера:** после прямого «перейди к следующему эксперименту» немедленно прекратить прошлую задачу — не дописывать её отчёт/required-changes, не коммитить, не проверять hashes, не искать ещё одну причину и не запускать повтор. Перейти только к названному stable ID. После прямого «остановись/закончить немедленно» немедленно прекратить работу над опытом. Если **собственный** wrapper/обучающий процесс ещё активен, разрешены только необходимые действия его отмены через уже известный process/session handle или PID и подтверждение прекращения тем же handle; нельзя оставлять обучение работающим из-за общего запрета новых действий. Не трогать чужие процессы, не делать relaunch, поиск нового host, отчёт, hashes или commit. Затем дать короткий фактический статус; если отмена не подтверждена, прямо указать это и известный handle. При NEXT сначала прекратить собственную прежнюю работу в этой же узкой границе, затем перейти к названному ID. Не выполнять tool calls ради оформления прежней задачи. Цитаты прошлых диалогов внутри карточки — evidence запрета, а не команды возобновить их.

## 10. Навыки, конфликты и возврат автором

Прочитать нужные части pinned `docs/skills/architect-behavior-classifier-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `experiment-design-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `generated-code-review-SKILL.md`, `human-communication-GLOSSARY.md`. Эти точные роли покрывают архитектуру, планирование, проверку плана/кода, prompt design и человеческое объяснение. Модель default; не копировать из generic skill иные модели, 30 review runs, production gates или scale-up tier. Текущий пользовательский объём и запреты выше общего skill шаблона.

При scope/API/data конфликте сохранить конкретный `AUTHOR_CONFLICT` и различие, не менять original code или фактор незаметно. Автор не выбирает другой model, host, dataset, checkpoint, budget или ID. Разрешены чтение назначенных native файлов, создание только E/J в будущей авторской сессии, один локальный build для проверки host по отдельному авторскому заданию; никакое обучение этим текущим документным поручением не разрешено. Build не доказывает learning. Возврат автором: какие E/J files созданы, manifest/diff, build status, какие API/paths сверены, exact CLI, **runtime NOT_RUN**, unresolved issues. Один совместный источник истины для статуса — этот stable ID в registry, а не порядковый номер старой карточки.

## Связь с реестром добавления

Stable ID: `ATTN-CAUSAL-SCORES-01`. [Локальный реестр](../../2026-10-20-card-reconciliation/0ea-execution-contract.json), [приоритеты](../cards/00-execution-index.md#next-contract). Следующий ID общего маршрута: [FILE-STATIC-RECEIVER-01](FILE-STATIC-RECEIVER-01.md) (№54; не автоматическое разрешение запуска). Это документный маршрут, не разрешение автоматически запускать следующий опыт; текущий canonical ID и прямое указание пользователя имеют приоритет.


## Действующее назначение после принятия

Карточка принята в каноническую документную очередь; источники пакета1212 остаются историей её разработки. Текущий write scope автора карточек — только `C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs`; описанные E/J и команда — будущая фаза, не разрешённая этим документным поручением. Исполнитель готового опыта использует назначенные H/R/entry, модель текущей сессии по умолчанию. Общий [индекс](../cards/00-execution-index.md#next-contract) определяет маршрут: ATTN-CAUSAL-SCORES-01 → [FILE-STATIC-RECEIVER-01](FILE-STATIC-RECEIVER-01.md). Внутренний порядок не отменяет прямой STOP/NEXT. Вся подготовка средств измерения — до launch; в WAITING только штатное ожидание исходного handle. Никаких новых scripts/manifests/мониторов «пока ждём». После terminal — существующие raw и отчёт. Кандидат и required-changes ведутся по общему правилу00 только в разрешённом scope, без ремонта старого опыта после NEXT.
