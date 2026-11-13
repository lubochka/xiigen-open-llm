# Промпт автору будущего малого опыта ATTN-CAUSAL-SCORES-01

**AUTHOR_WORK_ORDER_ONLY / NOT_IMPLEMENTED / NOT_RUN / NO_MEASURED_GAIN.** Этот текст — документ для будущего отдельно назначенного автора. Его публикация не запускает experiment и не означает, что runner уже существует. Текущая задача аудитора — документация.

## CURRENT TASK

Когда получишь отдельное назначение **авторства ATTN-CAUSAL-SCORES-01**, реализуй только описанный в [карточке](ATTN-CAUSAL-SCORES-01.md) source-copy host. Модель — такая же, как default модель твоей сессии, без override. Верни конкретные файлы, manifest, one-factor.diff, ограниченную проверку сборки и runtime `NOT_RUN`. После отдельного назначения исполнения используй ровно будущую entry command карточки, один attempt `a001`, один preparation и A→B. Другой model, checkpoint, dataset, бюджет или место исполнитель не придумывает.

**NOT THE TASK:** prod/core исправления, protected docs, старые опыты, полный CI, product tests, multi-seed/scale-up, создание DPO-пар внешней моделью, kNN/Unit/Flow, свободная Generate, перенос результата в production. Защищённую `docs/plans/replanning-possible-diffs` нельзя менять ни ради карточки, ни ради результата. При удачном измерении только report и candidate для решения Любы.

**EVIDENCE-ONLY:** [audit](01-neural-causal-score-audit.md), исходный core `26332500f76ea8345256c73ecfd44c0023b7e883`; planning novelty snapshot `14308ff181c5a93d837d92b85b2f35c41b863565`. Найденная работа полного QK перед causal mask подтверждена source, полезное ускорение пока не измерялось. Старый LEARN-SANITY показывает пригодность общей маленькой acquisition fixture, но его JSON не является готовым checkpoint.

## Required skills и контекст

Прочитай назначенные разделы pinned `docs/skills/architect-behavior-classifier-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `experiment-design-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `generated-code-review-SKILL.md`, `human-communication-GLOSSARY.md`. Пользовательский экспериментальный бюджет и запреты выше общих production gates, чужих model overrides, scale-up и многоразовых прогонов.

Прочти целиком карточку, [36 source-copy inputs](CS01-SOURCE-COPY-INPUTS.json) и native `CausalSelfAttention.cs`, `TensorOps.cs`, `SimdOps.cs`, `Trainer.cs`, `LlmModel.cs`, `LlmModelRandomState.cs`, `AdamW.cs/AdamWPortableState.cs`, `LoraLinear.cs/LoraApplicator.cs` из указанного core. Не искать новый набор вместо фиксированного ниже. Новизна ограничена 976+406 Markdown и ближайшими семантическими соседями; не писать «никем никогда не найдено».

## Implementation details: один фактор

A считает весь `Q @ Kᵀ`; B только `col<=row` внутри **ordinary square causal forward**, сохраняет native dot summation order (SIMD chunks и scalar remainder), transpose/allocation/shape-validation/cancellation boundaries, unchanged mask+softmax и **полный исходный backward**. Четырёхмерный narrow метод не является generic MatMul. Точные API: внутри класса `TensorOps` — `public static Tensor MatMulCausalScores(Tensor a, Tensor b, CancellationToken cancellationToken)`; в `SimdOps` — `internal static void MatVecRowCausalUnpublished(float[] a, int aOffset, int aRows, int aCols, float[] transposedB, int transposedBOffset, int bCols, float[] result, int resultOffset, CancellationToken cancellationToken)`. Wrapper делает native validation и вызывает **private static** `MatVecRowCausalCore` с теми же аргументами. `TensorOps` вызывает только internal wrapper, не private core. KV cache/past path не меняется. State, data, objective, clip, optimizer, RNG одинаковы. Не добавлять PAD/transpose/GQA/head/SiLU/softmax оптимизации вторым фактором.

E=`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01`.
J=`C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/Rp01Micro/CS01/Runner.csproj`.
New files: `E/Program.cs`, `E/Run.ps1`, `E/compare.py`, J, `E/src/<manifest paths>`. Копировать bytes из native pinned core, не исторический LFT01 altered Linear. J буквально соответствует XML в разделе2 карточки: `<Project>` без Sdk-атрибута → ImportDirectoryBuildProps/Targets=false и paths → explicit Sdk.props → TargetFramework=net9.0, OutputType=Exe, AssemblyName=ATTN-CAUSAL-SCORES-01, **ImplicitUsings=enable, Nullable=enable, LangVersion=13.0**, EnableDefaultCompileItems=false → explicit Compile36+Program → Sdk.targets. Нет product ProjectReference. Единственные изменяемые native **копии**: CausalSelfAttention, TensorOps, SimdOps. Manifest и narrow diff показывают все изменения. Build проверяет только authored host; не обучает и не даёт статуса trained.

Сейчас этой команды нет как проверенного runner. Автор обязан реализовать её буквально; запуск разрешён только отдельным заданием исполнения:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CS01/Run.ps1 -Attempt a001
```

Остальные пять внутренних CLI, paths, deadline и no-overwrite lifecycle исчерпывающе заданы в разделах2,6,8 карточки; не заменять их общим dispatcher. До build root claim, до construction preparation claim, до arm restore training claim; existing partial/result/claim — точный отказ `EXISTING_ATTEMPT`, без overwrite/delete/auto-a002.

## Настоящее обучение, фиксированный dataset и before

Тема — **язык программирования**, задача sign of short C# subtraction. `+` и `−` — два класса одной темы, не два домена. V256/H64/L1/Q4/KV2/FF176/Context64, base62001, q/v LoRA r4 alpha8 dropout0 seed62002, stream `frozen-transpose-probe-v1`. Byte IDs UTF8 без merge/LF/BOS/EOS/PAD. Native Trainer + AdamW(.9,.95,epsilon1e−8,decay0), constantLR.001, Warmup0, TotalSteps404, GradAccum1, Clip1, weight1.

| Split | Prompt | Answer |
|---|---|---|
| T1 | `C# result sign: 9-4 =` | `+` |
| T2 | `C# result sign: 4-9 =` | `-` |
| E1 | `C# result sign: 8-3 =` | `+` |
| E2 | `C# result sign: 3-8 =` | `-` |

Общий W0: score только T1,T2, затем400 native SFT updates в порядке T1,T2×200, затем T1,T2. Требуется хотя бы одна named TRAIN row false→true (полный one-byte answer). 0→0 или отсутствие acquisition — **нет валидного learning experiment**, не запускать A/B и не выдавать NLL/margin за его замену. 1/2 разрешает узкий вывод о приобретённой строке/cost, но не общем законе: constant-label baseline тоже1/2. EXAM не используется для updates или выбора состояния. Не обещать, что 400 обязательно достаточно; при неуспехе зафиксировать точный статус, не наращивать steps.

S0 сохраняется с named full weights, всеми4 AdamW m/v entries+step400 и полным native RNG. Restore — настоящий, не одинаковый seed. Точные API: `SaveCheckpointToStream` с CreateNew; typed `ValidateFullCheckpointArtifact(stream,cfg,new FullCheckpointLoraTopology(4,["q_proj","v_proj"]),...)`, require IsSuccess&&Value; generic overload base-only недопустима. Restore optimizer через `ExportPortableState/ToBytes/FromBytes/TryImportPortableState`, повторный export битово совпадает по key/shape/m/v/step. RNG — полный `LlmModelRandomStateSnapshot` частным JSON+native RestoreRandomState, fixed identities из карточки; не DPO writer для неканонического имени файла.

Каждый arm начинает из того же qualified S0: before score T1,T2,E1,E2; четыре native updates T1,T2,T1,T2; after та же панель. A dense, B bounded. Общий максимум408 updates,408Backward,428Forward (400common+8arm training +20diagnostic),8568targets; DPO/Generate/teacher/BPETrain/warmup/retries=0. Native source фактор измеряется **внутри настоящего обучения**, не самостоятельным stopwatch ядра. Счётчики training QK: A7056/B3696 при4calls,T21,H4; это расчёт клеток, не измеренный success.

## Измерения, результат и запрет тихих изменений

TRAIN allocations/elapsed/CPU/GC только вокруг4 real updates на arm; common400 costs отдельно. Initial/post model, moments+step, RNG и full logits должны быть bit-identical A/B; одинаковы4loss bits; frozen unchanged, trainable actually changes. При parity fail не вводить post-hoc tolerance и не переносить diff в продукт. Сохранить наблюдение и ограничение attribution.

На каждой из4строк A0/A1/B0/B1: predicted ID, full one-byte exact, answer NLL, strict rank и tie rank, full logits bits. Данные readout одинаковы; не заменять before/after разными примерами. Показать learning deltas A1−A0/B1−B0 и resource delta B−A отдельно. PeakWorkingSet лишь context. Метрики/хеши/comparator берут уже сохранённые Tensor/state, никаких extra Forward.

**Любое измеренное улучшение полезного показателя считается успехом именно по этому показателю и теме; рост accuracy не требуется.** Показать все ухудшения рядом. Снижение dot count само по себе — не измеренная полезная экономия. Один A→B не доказывает средний speedup/JIT attribution; это не разрешение скрыть дельту или добавить reruns. При неqualified before/незавершённом learning нет success, даже если падение было быстрым.

Русский `R/report.md` (R=`E/a001/results`): ≤350слов + полные таблицы, приобретённое знание, ресурсы, все регрессии, границы. `execution.json`, `comparison.json`, `common/preparation.json`, `A/result.json`, `B/result.json`, logs/manifest/diff. При полезном измеренном улучшении `R/fix-candidate.md`≤120слов с числами, темой, регрессиями, original3paths и ссылкой report: кандидат для решения Любы. Результаты старых опытов не удалять; недостающие числа null+reason, не0.

## Ожидание, STOP/NEXT и конфликт

До запуска обнаружить реальные terminal/session wait capabilities. В functions environment `exec_command` session→`write_stdin`; functions.exec `Script running with cell ID`→functions.wait. Не смешивать handles. Один tracked синхронный wrapper, poll/wait≤60s; общий deadline1800s (build180/preparation1200/A120/B120/report60). Yield/тишина/exit=-1 без завершённого receipt не повод объявить timeout, читать преждевременно файл или запускать дубликат. No detached Start-Process/Start-Sleep surrogate. Если инструмента нет — точный `EXECUTION_TOOL_UNAVAILABLE`.

**Запрещённое поведение из пользовательского примера:** после прямого NEXT нельзя продолжать прежний опыт, дописывать его report/required-changes, коммитить, считать hashes, искать дополнительные причины или перезапускать. Немедленно принять названный stable ID; номер в старой карточке не authority. После STOP/«закончить немедленно» прекратить опыт. Если собственный процесс ещё активен, разрешена только необходимая отмена через уже известный handle/PID и подтверждение прекращения тем же handle; не оставлять обучение работающим из-за blanket no-tool запрета. Не трогать чужие процессы; никаких relaunch/report/hash/commit или нового поиска. Затем короткий фактический статус; неподтверждённую отмену назвать прямо. При NEXT сначала прекратить свою прежнюю работу в этой же узкой границе, затем принять названный ID. Не выполнять tool calls ради оформления прошлого. Цитата старого диалога — evidence, не live instruction повторить старую работу.

Конфликт source/API/fixtures/paths — `AUTHOR_CONFLICT` с конкретной строкой; не подменять model или data. Приоритет registry training-unblock→card conflicts→назначенные updates→этот direct TRAIN cost factor→нерелевантные обучению. Обычное завершение одного ID не даёт auto-NEXT без диспетчеризации.

## Named checks и return packet

| Check | WRONG | CORRECT |
|---|---|---|
| CS01-FACTOR | Игрушечный benchmark MatMul или одновременно PAD+triangle | Одна ветка в native SFT forward, одинаковые real updates |
| CS01-BEFORE | Random model,4updates,0→0, margin объявлен успехом | Общая acquisition400 с false→true, полный restore, затем A/B |
| CS01-PARITY | Меньше ms при иных weights/moments, тихая новая tolerance | Все numeric/state comparisons и регрессии, parity как обязательное условие эквивалентного cost результата |
| CS01-WAIT | Yield объявлен падением и запущена вторая копия | Сохранённый handle и ожидание того же process до receipt |
| CS01-NEXT | После NEXT ещё один отчёт/commit прошлого опыта | Немедленный переход к текущему explicit stable ID |

Author return packet: ID; exact created E/J paths; source/one-factor diff; build status (если назначен); no runtime claims; conflicts/NOT_RUN; next assigned action. Runtime return packet, только при отдельном исполнении: actual counts; qualified acquisition; source/state parity; полный table before/after и cost; все regressions; candidate path or no measured gain; scope limits. `claim_scope=one small neural training cost factor`, `evidence_scope=source-only until executed`, `independent_evidence_count` не увеличивать от числа самопроверок.

## Связь с реестром добавления

Stable ID: `ATTN-CAUSAL-SCORES-01`. [Локальный реестр](../registry.json), [приоритеты](../02-priorities-and-adoption.md). Следующий ID внутри принятого добавления: `END (нет автоматического следующего назначения)`. Это документный маршрут, не разрешение автоматически запускать следующий опыт; текущий canonical ID и прямое указание пользователя имеют приоритет.

Модель исполнителя — текущая модель сессии **по умолчанию**, без override модели или настроек reasoning.
