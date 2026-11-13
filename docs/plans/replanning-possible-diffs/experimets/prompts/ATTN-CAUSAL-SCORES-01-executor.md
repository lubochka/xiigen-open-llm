# Промпт автору будущего малого опыта ATTN-CAUSAL-SCORES-01

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход перед новым аудитом или заданием

Обязателен [договор120](../../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

До действий прочитать выбранную карточку целиком и её локальную секцию120, source/raw и фактический маршрут producer→ordinary caller→learner→consumer; local HEAD, published SHA и dirty bytes различаются. Списки навыков ниже — дополнительные, не освобождение от полного свежего стека120. Исторические Tier3/vocab/split/UI/Git/model правила не расширяют нынешний паспорт и авторизацию.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.

**Сохранение постановки:** exact recorded result не повторять, old NOT_RUN не выше текущей Q-строки. Полный outer/nested бюджет и независимый gold остаются у карточки; пересчёт raw/структурный guard сTrain0 не заменяет обязательный учебный цикл и не получает фиктивный Train. Команды, API и фильтры не додумывать исполнителю. Вход обязателен также в передаваемом промпте выбранного ID.
<!-- EXECUTOR-CONTEXT-120:END -->


<!-- BEGIN shared-start-binding-2026-10-21 -->
## Сохранённый старт: h64-causal-scores/v1

**Действующий consumer-контракт:** [общее восстановление33](../33-shared-learned-starts.md), family `h64-causal-scores`. Этот раздел заменяет повторную common preparation во всех командах и промптах ниже. Consumer выполняет **0 common updates,0 дополнительных common readouts**; исходные arm updates/readouts и единственный фактор сохраняются.

- Данные этого семейства: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-causal-scores/v1/data.source.jsonl` и `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-causal-scores/v1/data.material.json`. Заданный источник: `C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-21-learning-delta-26332500/neural/ATTN-CAUSAL-SCORES-01.md`; рецепт: T+,T- bytes noLF;200cycles;stream=frozen-transpose-probe-v1;CS01-private-identities.
- Читать только `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-causal-scores/v1/bundle.json`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-causal-scores/v1/S0.weights.bin`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-causal-scores/v1/S0.optimizer.bin`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-causal-scores/v1/S0.rng.json`, `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-causal-scores/v1/preparation.json`; generation/tokenizer/authority/source manifest — там же по33. Missing/partial/unqualified → конкретный SHARED_START_UNAVAILABLE, без самостоятельного обучения или другого checkpoint.
- Единственный producer: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01`, A-семантика [назначенной карточки](../cards/ATTN-CAUSAL-SCORES-01.md); его конечный бюджет 400updates/404Forward/400Backward оплачивается один раз **отдельно**. Existing source для экспорта: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01/a001/results/common`.
- Будущий экспорт без model calls: `pwsh -NoProfile -File "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-causal-scores/v1/Export-Existing.ps1" -ContractPath "C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/reconciliation/shared-learned-starts-2026-10-21.json"`.
- Будущий однократный producer, только если он прямо включён в порученную программу: `pwsh -NoProfile -File "C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-causal-scores/v1/Prepare.ps1" -ContractPath "C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/reconciliation/shared-learned-starts-2026-10-21.json"`. Это заданный интерфейс, ещё не реализованный этим документным проходом; consumer не вызывает его автоматически.
- Восстановление: S0 weights/RNG и полный SFT optimizer (m/v/step) из bundle. Проверить named weights/keys/shapes, полный optimizer state и native RNG identity; construction/ApplySeeded только **до** load. Только DPO consumer заново создаёт native admission/lease после load, без переноса из producer; SFT-only MASK/MOMENT/H64/SILU/CS01 не создают DPO lease.
- Единственный consumer entry остаётся `powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01/Run.ps1 -Attempt a001`; его будущий wrapper вместо common Train читает fixed shared bundle, сохраняет его path/digest в study и восстанавливает A/B. R=`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01/a001/results`; в R/common разрешена неизменная входная копия shared snapshot для прежних внутренних путей. Она не создаёт preparation.claim/новые losses/updates. Reuse cost — чтение/restore, historical producer cost отдельно. Нельзя задним числом переписать claim уже выданного опыта.

Старые common-рецепты, методы подготовки и суммарные1000/400+arm budgets ниже теперь описывают **только однократного producer плюс consumer**, а не расход каждого consumer. Старый child `--prepare-learned`/prepare не вызывать из consumer wrapper. Все tools и ожидание — по31; никаких exporters/manifest/scripts «пока ждём». Артефакты и scripts здесь не созданы и не проверены запуском.
**Сохранение оплаченного state:** сначала независимо записать назначенные weights, optimizer, RNG и имеющуюся qualification; затем typed validation/readback; COMPLETE последним по33. Ошибка проверки weights не должна пропускать запись остальных уже доступных состояний. Новых Train/Forward и периодических снимков не добавлять; partial сохранять.

**Точный weights API для этой карточки:** `cfg` — фиксированный ModelConfig этой карточки; LoRA rank4, targets строго `q_proj,v_proj`. Producer: `model.SaveCheckpointToStream(stream)` на новом `FileStream(path,FileMode.CreateNew,FileAccess.ReadWrite,FileShare.None)`, изначально Length=0/Position=0; `Flush(flushToDisk:true)`. Затем независимо записать остальные state/receipt по33 и только после этого открыть сохранённый weights для чтения. Установить Position=0; `var checkedArtifact = LlmModel.ValidateFullCheckpointArtifact(stream,cfg,new FullCheckpointLoraTopology(4,new[]{"q_proj","v_proj"}))`; требовать `checkedArtifact.IsSuccess && checkedArtifact.Value==true`. Если в конкретном рецепте уже назначен maximumArtifactBytes, передать его неизменным (CS01:8388608). Перегрузка без topology недопустима для этого LoRA snapshot. Перед native `freshModel.LoadCheckpoint(stream)` снова Position=0 либо открыть новый read stream; construction/ApplySeeded только до load. Проверить назначенные named keys/shapes/value bits. Равный размер файла, повторное чтение самого файла или снижение loss не доказывают равенство потерянной live memory. В consumed/recovery случае такое равенство помечать UNKNOWN, не придумывать. Эта запись уточняет уже назначенные операции, не добавляет model calls.

<!-- END shared-start-binding-2026-10-21 -->


<!-- BEGIN mandatory-learning-and-recovery-2026-10-21 -->
## Обязательные правила для карточки и любого её промпта

**Найти tool и дождаться своей операции — обязанность исполнителя.** До выдачи команды прочитать [контракт31](../31-tool-discovery-and-wait.md) и указанные там реальные skills/references; по каталогу текущей среды найти launch и wait/resume/status, прочитать их схему и сохранить provider, handle, start time, H/R и deadline. Не выдумывать Kiro API по Codex-примеру, не просить пользователя выбирать инструмент или запускать команду вместо исполнителя.

**Ожидание не равно бесполезному polling.** Одна выдача entry; затем штатный bounded wait/resume того же session/cell/process ID до terminal exit или исходного experiment deadline. Нормальный wait можно повторять при silent/running. Два быстрых неблокирующих чтения без прогресса требуют перейти в wait; если он неисправен/ответы противоречат — независимое чтение H/R/claim/exit и своего PID+start. Не вводить Start-Sleep в занятый терминал. Предел одного ожидания≤60с — не предел эксперимента; yield/RPC timeout не доказывает exit/kill/нулевой расход. После диагностики живой процесс продолжают ждать. Отсутствие log не доказывает, что запуска не было. Не выдавать entry заново «синхронно на25минут», не подбирать results-NN, не удалять claim/partial, не сокращать TRAIN/steps до двух минут ради tool. При UNKNOWN сохранять неизвестный расход и разбирать существующий запуск. STOP/NEXT Любы немедленны.

**Не завершать ход на ожидании и не придумывать работу.** ISSUED/WAITING → штатный блокирующий/event wait исходного handle → terminal/exit → чтение назначенных raw → отчёт. Финальное «жду уведомления / доложу потом» до результата запрещено; обещание auto-notification не доказывает возобновление. Не создавать фоновый waiter/monitor/watchdog, циклы polling, progress-события каждые N шагов, summary scripts, manifests или другую работу «пока ждём»; не запускать агентов или следующую карточку. Обычное ожидание не требует заполнения паузы работой. Соблюдать provider no-poll/no-sleep. Проверить настоящий wait/continuation в каталоге до launch; при отсутствии доказать конкретный блокер, сохранить handle и INCOMPLETE, без выдуманного API или relaunch. STOP/NEXT Любы немедленны. Полный порядок и исключения — [контракт31](../31-tool-discovery-and-wait.md).

**Без действительного обучения задача не выполнена.** Каждая новая экспериментальная программа, включая diagnostic/reader/restore, содержит назначенный реальный учебный участок: original файл/метод, learner → изменённое состояние → обычный consumer, точные TRAIN/EXAM, измерения до/после и конечный бюджет. Обученный S0 является стартом, но не заменяет этот участок. Ноль updates допустим у контрольного плеча или reader после общего Train, но не у всей новой программы. KNN/BPE действительно обучают память/правила даже без нейронных updates. Счётчик, loss, hash и случайный верный токен не доказывают навык; 0→0 не является учебным результатом, частичные токены не объявляются целым ответом. Исторический безучебный результат сохранить; повтор получает отдельный назначенный рецепт и новый output. Выполненные стадии той же попытки после сбоя отчёта не переобучать ради счётчика. Полный охват и адресные рецепты — [37](../37-full-catalogue-learning-coverage.md); конкретные данные и бюджет брать из своей карточки, не придумывать.

**0 → 0, отсутствие обучения или неполное A/B — не завершение эксперимента.** Раздельно записать execution_status, learning_validity и hypothesis_status; при невалидности учебная цель OPEN. Сохранить сырые свидетельства и продолжить разрешённое исправление постановки, а не только написать «невалидно» и отчитаться об успехе. Не искать другой checkpoint/модель/host, не менять seed/фактор, не увеличивать steps до PASS: точный исправленный рецепт назначает автор карточки до новой попытки. Старые опыты не удалять. Поздняя ошибка сохранения не обнуляет состоявшиеся updates; неизвестное число — null, невыполненное плечо — «не измерено».

Любое полезное измеренное улучшение в **валидном учебном сравнении** — успех своей метрики/домена; рост accuracy необязателен, регрессии рядом. Диагностика невалидной постановки не закрывает учебную цель. Default-модель исполнителя без override; точные H/R/entry ниже. Отчёт — русский R/report.md. Текущая правка документов не разрешает runtime. Прямые STOP/NEXT Любы немедленны и при открытой цели.

Полный конечный порядок диагностики и критерии: [контракт30](../30-failure-recovery-and-learning-validity.md). При конфликте со старым текстом ниже действует этот блок.
**Состав данных ограничивает вывод.** До запуска автор задаёт точные TRAIN/EXAM/CONTROL, покрытие классов, роли отрицательных примеров, семьи/пересечения и конкретный контроль постоянного ответа. Одноклассовый TRAIN и выигрыш на его единственной метке не доказывают различение маршрутов; updates/confidence не заменяют этот контроль. Разные опыты не устанавливают причину регрессии состава данных. Измерять любой исход, не предписывать ухудшение; набор и budget исполнитель не меняет сам. Полное обязательное правило — [состав данных и допустимый вывод](../32-training-set-and-inference-contract.md).

**Сохранённый учебный результат используется без повторного выбора.** В уже разрешённой фазе исполнения пригодные сохранённые веса/обученное состояние имеют приоритет: восстановить их и продолжить назначенный опыт, не спрашивая «A: использовать результат или B: обучить заново». Поздняя ошибка проверки, сериализации или отчёта сама по себе не основание повторять Train. Сначала прочитать актуальные receipts и установить, не выполнены ли recovery/плечи уже; состоявшуюся работу не переиздавать. Проверять только необходимую для следующего шага совместимость и учебную квалификацию, используя имеющиеся данные; недостающие показания получать лишь по явно ограниченному recovery-рецепту, без optimizer updates и без притворного восстановления отсутствующих moments/RNG. Если пригодность доказана, выбрать reuse автоматически. Если объективно отсутствует необходимое состояние, назвать конкретный технический пробел; не предлагать повторное обучение «для чистоты», не просить пользователя снять guard и не выдавать размер файла/падение loss за доказательство выученного навыка. Текущая документная фаза запусков не разрешает.

**Реализация не отменяет контракт.** «Код не читает RNG/optimizer/проверку» означает отклонение, а не разрешение его игнорировать. До следующего model call исправить реализацию в уже разрешённом scope либо применить только явно назначенный в карточке совместимый режим восстановления; не придумывать исключение, не просить снять guard и не переносить weights-only DPO-исключение на SFT continuation.

<!-- END mandatory-learning-and-recovery-2026-10-21 -->


**AUTHOR_WORK_ORDER_ONLY / NOT_IMPLEMENTED / NOT_RUN / NO_MEASURED_GAIN.** Этот текст — документ для будущего отдельно назначенного автора. Его публикация не запускает experiment и не означает, что runner уже существует. Текущая задача аудитора — документация.

## CURRENT TASK

Когда получишь отдельное назначение **авторства ATTN-CAUSAL-SCORES-01**, реализуй только описанный в [карточке](../cards/ATTN-CAUSAL-SCORES-01.md) source-copy host. Модель — такая же, как default модель твоей сессии, без override. Верни конкретные файлы, manifest, one-factor.diff, ограниченную проверку сборки и runtime `NOT_RUN`. После отдельного назначения исполнения используй ровно будущую entry command карточки, один attempt `a001`, restore shared S0 без preparation и A→B. Другой model, checkpoint, dataset, бюджет или место исполнитель не придумывает.

**NOT THE TASK:** prod/core исправления, protected docs, старые опыты, полный CI, product tests, multi-seed/scale-up, создание DPO-пар внешней моделью, kNN/Unit/Flow, свободная Generate, перенос результата в production. Защищённую `docs/plans/replanning-possible-diffs` нельзя менять ни ради карточки, ни ради результата. При удачном измерении только report и candidate для решения Любы.

**EVIDENCE-ONLY:** [audit](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-21-learning-delta-26332500/neural/01-neural-causal-score-audit.md>), исходный core `26332500f76ea8345256c73ecfd44c0023b7e883`; planning novelty snapshot `14308ff181c5a93d837d92b85b2f35c41b863565`. Найденная работа полного QK перед causal mask подтверждена source, полезное ускорение пока не измерялось. Старый LEARN-SANITY показывает пригодность общей маленькой acquisition fixture, но его JSON не является готовым checkpoint.

## Required skills и контекст

Прочитай назначенные разделы pinned `docs/skills/architect-behavior-classifier-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `experiment-design-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `generated-code-review-SKILL.md`, `human-communication-GLOSSARY.md`. Пользовательский экспериментальный бюджет и запреты выше общих production gates, чужих model overrides, scale-up и многоразовых прогонов.

Прочти целиком карточку, [36 source-copy inputs](../../2026-10-21-learning-delta-integration/CS01-SOURCE-COPY-INPUTS.json) и native `CausalSelfAttention.cs`, `TensorOps.cs`, `SimdOps.cs`, `Trainer.cs`, `LlmModel.cs`, `LlmModelRandomState.cs`, `AdamW.cs/AdamWPortableState.cs`, `LoraLinear.cs/LoraApplicator.cs` из указанного core. Не искать новый набор вместо фиксированного ниже. Новизна ограничена 976+406 Markdown и ближайшими семантическими соседями; не писать «никем никогда не найдено».

## Implementation details: один фактор

A считает весь `Q @ Kᵀ`; B только `col<=row` внутри **ordinary square causal forward**, сохраняет native dot summation order (SIMD chunks и scalar remainder), transpose/allocation/shape-validation/cancellation boundaries, unchanged mask+softmax и **полный исходный backward**. Четырёхмерный narrow метод не является generic MatMul. Точные API: внутри класса `TensorOps` — `public static Tensor MatMulCausalScores(Tensor a, Tensor b, CancellationToken cancellationToken)`; в `SimdOps` — `internal static void MatVecRowCausalUnpublished(float[] a, int aOffset, int aRows, int aCols, float[] transposedB, int transposedBOffset, int bCols, float[] result, int resultOffset, CancellationToken cancellationToken)`. Wrapper делает native validation и вызывает **private static** `MatVecRowCausalCore` с теми же аргументами. `TensorOps` вызывает только internal wrapper, не private core. KV cache/past path не меняется. State, data, objective, clip, optimizer, RNG одинаковы. Не добавлять PAD/transpose/GQA/head/SiLU/softmax оптимизации вторым фактором.

E=`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01`.
J=`C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/CS01/Runner.csproj`.
New files: `E/Program.cs`, `E/Run.ps1`, `E/compare.py`, J, `E/src/<manifest paths>`. Копировать bytes из native pinned core, не исторический LFT01 altered Linear. J буквально соответствует XML в разделе2 карточки: `<Project>` без Sdk-атрибута → ImportDirectoryBuildProps/Targets=false и paths → explicit Sdk.props → TargetFramework=net9.0, OutputType=Exe, AssemblyName=ATTN-CAUSAL-SCORES-01, **ImplicitUsings=enable, Nullable=enable, LangVersion=13.0**, EnableDefaultCompileItems=false → explicit Compile36+Program → Sdk.targets. Нет product ProjectReference. Единственные изменяемые native **копии**: CausalSelfAttention, TensorOps, SimdOps. Manifest и narrow diff показывают все изменения. Build проверяет только authored host; не обучает и не даёт статуса trained.

Сейчас этой команды нет как проверенного runner. Автор обязан реализовать её буквально; запуск разрешён только отдельным заданием исполнения:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01/Run.ps1 -Attempt a001
```

Остальные пять внутренних CLI, paths, deadline и no-overwrite lifecycle исчерпывающе заданы в разделах2,6,8 карточки; не заменять их общим dispatcher. До build root claim; shared input immutable, consumer preparation claim не создаёт; до arm restore training claim; existing partial/result/claim — точный отказ `EXISTING_ATTEMPT`, без overwrite/delete/auto-a002.

## Настоящее обучение, фиксированный dataset и before

Тема — **язык программирования**, задача sign of short C# subtraction. `+` и `−` — два класса одной темы, не два домена. V256/H64/L1/Q4/KV2/FF176/Context64, base62001, q/v LoRA r4 alpha8 dropout0 seed62002, stream `frozen-transpose-probe-v1`. Byte IDs UTF8 без merge/LF/BOS/EOS/PAD. Native Trainer + AdamW(.9,.95,epsilon1e−8,decay0), constantLR.001, Warmup0, TotalSteps404, GradAccum1, Clip1, weight1.

| Split | Prompt | Answer |
|---|---|---|
| T1 | `C# result sign: 9-4 =` | `+` |
| T2 | `C# result sign: 4-9 =` | `-` |
| E1 | `C# result sign: 8-3 =` | `+` |
| E2 | `C# result sign: 3-8 =` | `-` |

Только отдельный producer (не этот consumer): общий W0, score T1,T2, затем400 native SFT updates в порядке T1,T2×200, затем T1,T2. Требуется хотя бы одна named TRAIN row false→true (полный one-byte answer). 0→0 или отсутствие acquisition — **нет валидного learning experiment**, не запускать A/B и не выдавать NLL/margin за его замену. 1/2 разрешает узкий вывод о приобретённой строке/cost, но не общем законе: constant-label baseline тоже1/2. EXAM не используется для updates или выбора состояния. Не обещать, что 400 обязательно достаточно; при неуспехе зафиксировать точный статус, не наращивать steps.

S0 сохраняется с named full weights, всеми4 AdamW m/v entries+step400 и полным native RNG. Restore — настоящий, не одинаковый seed. Точные API: `SaveCheckpointToStream` с CreateNew; typed `ValidateFullCheckpointArtifact(stream,cfg,new FullCheckpointLoraTopology(4,["q_proj","v_proj"]),...)`, require IsSuccess&&Value; generic overload base-only недопустима. Restore optimizer через `ExportPortableState/ToBytes/FromBytes/TryImportPortableState`, повторный export битово совпадает по key/shape/m/v/step. RNG — полный `LlmModelRandomStateSnapshot` частным JSON+native RestoreRandomState, fixed identities из карточки; не DPO writer для неканонического имени файла.

Каждый arm начинает из того же qualified S0: before score T1,T2,E1,E2; четыре native updates T1,T2,T1,T2; after та же панель. A dense, B bounded. Consumer:8 updates/8Backward/24Forward (8training+16diagnostic),168targets, common0; отдельный producer400updates/400Backward/404Forward,8400targets; сумма408updates/408Backward/428Forward,8568targets; DPO/Generate/teacher/BPETrain/warmup/retries=0. Native source фактор измеряется **внутри настоящего обучения**, не самостоятельным stopwatch ядра. Счётчики training QK: A7056/B3696 при4calls,T21,H4; это расчёт клеток, не измеренный success.

## Измерения, результат и запрет тихих изменений

TRAIN allocations/elapsed/CPU/GC только вокруг4 real updates на arm; common400 costs отдельно. Initial/post model, moments+step, RNG и full logits должны быть bit-identical A/B; одинаковы4loss bits; frozen unchanged, trainable actually changes. При parity fail не вводить post-hoc tolerance и не переносить diff в продукт. Сохранить наблюдение и ограничение attribution.

На каждой из4строк A0/A1/B0/B1: predicted ID, full one-byte exact, answer NLL, strict rank и tie rank, full logits bits. Данные readout одинаковы; не заменять before/after разными примерами. Показать learning deltas A1−A0/B1−B0 и resource delta B−A отдельно. PeakWorkingSet лишь context. Метрики/хеши/comparator берут уже сохранённые Tensor/state, никаких extra Forward.

**Любое измеренное улучшение полезного показателя считается успехом именно по этому показателю и теме; рост accuracy не требуется.** Показать все ухудшения рядом. Снижение dot count само по себе — не измеренная полезная экономия. Один A→B не доказывает средний speedup/JIT attribution; это не разрешение скрыть дельту или добавить reruns. При неqualified before/незавершённом learning нет success, даже если падение было быстрым.

Русский `R/report.md` (R=`E/a001/results`): ≤350слов + полные таблицы, приобретённое знание, ресурсы, все регрессии, границы. `execution.json`, `comparison.json`, `common/preparation.json`, `A/result.json`, `B/result.json`, logs/manifest/diff. При полезном измеренном улучшении `R/fix-candidate.md`≤120слов с числами, темой, регрессиями, original3paths и ссылкой report: кандидат для решения Любы. Результаты старых опытов не удалять; недостающие числа null+reason, не0.

## Ожидание, STOP/NEXT и конфликт

До запуска обнаружить реальные terminal/session wait capabilities. В functions environment `exec_command` session→`write_stdin`; functions.exec `Script running with cell ID`→functions.wait. Не смешивать handles. Один tracked синхронный wrapper, poll/wait≤60s; consumer deadline1800s (build180/A120/B120/report60; preparation0), producer deadline отдельно по33. Yield/тишина/exit=-1 без завершённого receipt не повод объявить timeout, читать преждевременно файл или запускать дубликат. No detached Start-Process/Start-Sleep surrogate. Если инструмента нет — точный `EXECUTION_TOOL_UNAVAILABLE`.

**Запрещённое поведение из пользовательского примера:** после прямого NEXT нельзя продолжать прежний опыт, дописывать его report/required-changes, коммитить, считать hashes, искать дополнительные причины или перезапускать. Немедленно принять названный stable ID; номер в старой карточке не authority. После STOP/«закончить немедленно» прекратить опыт. Если собственный процесс ещё активен, разрешена только необходимая отмена через уже известный handle/PID и подтверждение прекращения тем же handle; не оставлять обучение работающим из-за blanket no-tool запрета. Не трогать чужие процессы; никаких relaunch/report/hash/commit или нового поиска. Затем короткий фактический статус; неподтверждённую отмену назвать прямо. При NEXT сначала прекратить свою прежнюю работу в этой же узкой границе, затем принять названный ID. Не выполнять tool calls ради оформления прошлого. Цитата старого диалога — evidence, не live instruction повторить старую работу.

Конфликт source/API/fixtures/paths — `AUTHOR_CONFLICT` с конкретной строкой; не подменять model или data. Приоритет registry training-unblock→card conflicts→назначенные updates→этот direct TRAIN cost factor→нерелевантные обучению. Обычное завершение одного ID не даёт auto-NEXT без диспетчеризации.

## Named checks и return packet

| Check | WRONG | CORRECT |
|---|---|---|
| CS01-FACTOR | Игрушечный benchmark MatMul или одновременно PAD+triangle | Одна ветка в native SFT forward, одинаковые real updates |
| CS01-BEFORE | Random model,4updates,0→0, margin объявлен успехом | Отдельный producer acquisition400 с false→true; consumer только полный shared restore, затем A/B |
| CS01-PARITY | Меньше ms при иных weights/moments, тихая новая tolerance | Все numeric/state comparisons и регрессии, parity как обязательное условие эквивалентного cost результата |
| CS01-WAIT | Yield объявлен падением и запущена вторая копия | Сохранённый handle и ожидание того же process до receipt |
| CS01-NEXT | После NEXT ещё один отчёт/commit прошлого опыта | Немедленный переход к текущему explicit stable ID |

Author return packet: ID; exact created E/J paths; source/one-factor diff; build status (если назначен); no runtime claims; conflicts/NOT_RUN; next assigned action. Runtime return packet, только при отдельном исполнении: actual counts; qualified acquisition; source/state parity; полный table before/after и cost; все regressions; candidate path or no measured gain; scope limits. `claim_scope=one small neural training cost factor`, `evidence_scope=source-only until executed`, `independent_evidence_count` не увеличивать от числа самопроверок.

## Связь с реестром добавления

Stable ID: `ATTN-CAUSAL-SCORES-01`. [Локальный реестр](../../2026-10-20-card-reconciliation/0ea-execution-contract.json), [приоритеты](../cards/00-execution-index.md#next-contract). Следующий ID общего маршрута: [FILE-STATIC-RECEIVER-01](../cards/FILE-STATIC-RECEIVER-01.md) (№54; не автоматическое разрешение запуска). Это документный маршрут, не разрешение автоматически запускать следующий опыт; текущий canonical ID и прямое указание пользователя имеют приоритет.

Модель исполнителя — текущая модель сессии **по умолчанию**, без override модели или настроек reasoning.


## Действующее назначение после принятия

Карточка принята в каноническую документную очередь; источники пакета1212 остаются историей её разработки. Текущий write scope автора карточек — только `C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs`; описанные E/J и команда — будущая фаза, не разрешённая этим документным поручением. Исполнитель готового опыта использует назначенные H/R/entry, модель текущей сессии по умолчанию. Общий [индекс](../cards/00-execution-index.md#next-contract) определяет маршрут: ATTN-CAUSAL-SCORES-01 → [FILE-STATIC-RECEIVER-01](../cards/FILE-STATIC-RECEIVER-01.md). Внутренний порядок не отменяет прямой STOP/NEXT. Вся подготовка средств измерения — до launch; в WAITING только штатное ожидание исходного handle. Никаких новых scripts/manifests/мониторов «пока ждём». После terminal — существующие raw и отчёт. Кандидат и required-changes ведутся по общему правилу00 только в разрешённом scope, без ремонта старого опыта после NEXT.

> **Дождаться результата в этом задании:** после ISSUED/WAITING следующий шаг — штатный блокирующий/event wait исходного handle, после terminal — exit и raw, затем отчёт. Не заканчивать ход фразой «жду/доложу потом» и не считать обещание уведомления доказанным continuation. Не создавать дополнительный фоновый waiter/monitor, polling-loop, summary script, manifest или другую работу «пока ждём», не запускать агентов/следующий опыт. Соблюдать no-poll/no-sleep провайдера; discovery и подтверждённый wait из контракта31, без выдуманных tools. Доказанный внешний блокер — INCOMPLETE с сохранённым handle, без relaunch. STOP/NEXT Любы немедленны.

> **Без лишнего выбора о повторном обучении:** пригодный сохранённый учебный результат использовать автоматически. Не предлагать заново обучить модель из-за поздней ошибки validation/записи; не спрашивать, предпочитает ли пользователь сохранить уже оплаченный результат. Сначала проверить актуальные receipts: уже выполненные recovery и A/B не повторять. Минимальная проверка пригодности остаётся; отсутствие необходимого состояния фиксируется честно. Отсутствующая в коде обязательная проверка или RNG/optimizer restore — дефект реализации, а не разрешение снять условие; использовать только явно назначенный совместимый режим карточки.
