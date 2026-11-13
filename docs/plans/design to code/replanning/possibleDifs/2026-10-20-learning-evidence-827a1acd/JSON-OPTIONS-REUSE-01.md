# JSON-OPTIONS-REUSE-01 — стоимость десериализации в настоящем обучении Executor

**Статус:** `PLANNED_NOT_RUN / SOURCE_COPY_HOST_AUTHORED_UNBUILT`. Это новая узкая cost-гипотеза семейства P125. В этом документном аудите обучение и сборка не выполняются. Полный source-copy host задан в [приложении](JSON-OPTIONS-REUSE-01-HOST.md); он написан, но ещё не собран и не проверен native запуском. Домен: **Перевод EN→RU, technical, синтетический существующий fixture P125**.

## 1. Вопрос и факт в коде

Можно ли уменьшить выделение managed memory на **весь реальный batch8 обучения**, повторно используя неизменные `JsonSerializerOptions` только в `ExecutorUnit.TrainOnExample`, при том же выученном состоянии и тех же обычных ответах?

В [TranslationTrainedUnits.cs:404–455](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain/TranslationTrainedUnits.cs#L404-L455) строка408 создаёт новый options и converter на каждый урок. Затем метод десериализует `ExecutorInput/ExecutorOutput` и пишет exact-source, phrase/word maps и общий KNN. [TrainStructuredBatchAsync:107–145](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain/TranslationMachine.cs#L107-L145) вызывает метод для каждого обучающего примера.

Полезный эффект пока **гипотеза**. Не измерены ни экономия памяти, ни ускорение. Этот фактор не объявлен доказанной причиной снижения semantic quality RP01.

**Новизна относительно корпуса:** в799planningMD+339protectedMD на зафиксированных снимках найдено использование JsonSerializerOptions в сопутствующих snippets, но не сравнение его lifetime внутри Executor training. Это не N62: тот меняет training context другого learner. На более позднем core `827a1acdbef95d220886e54f9d36bce0b405c8cd` появились новые docs; эта карточка сохраняет воспроизводимый code/data pin669 и не присваивает новые результаты его старому снимку.

## 2. Получатель обучения и проверяемая цепочка

`existing batch8 → TranslationMachine.TrainStructuredBatchAsync → ExecutorUnit.TrainOnExample → exact/phrase/shared-KNN state → GetOrchestratorAsync().RunAsync → TranslationRoutingFlow → TrainedTranslationAdapter → learned tokenizer / executor / QualityVerifier.Verify → обычный output/verdict`.

[Обычный consumer, TranslationDirectionAdapters.cs:117–142](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/27.TranslationDomain/TranslationDirectionAdapters.cs#L117-L142). Измеряется обучение настоящей памяти и её применение; микробенчмарк одного JsonSerializer отдельно от обучения сюда не допускается. Neural optimizer отсутствует: neural updates0, neural layers N/A, external model calls0. По исходнику одна машина содержит8Units и1Flow; обучается только `executor-en_ru`.

## 3. Единственное различие A/B

| Поле | A | B |
|---|---|---|
| Код | Девять закреплённых исходников без изменений | Те же девять исходников; один метод использует повторные options |
| Train data / IDs / order | Исходный `training-batch-B.json`,8уроков | Тот же файл побайтно |
| Изначальная память | Fresh TranslationMachine, не checkpoint | То же |
| Обучение | Один TrainStructuredBatchAsync(batch8) | Один такой же вызов |
| Evaluation | Одна фиксированная24-case панель до и после | Та же |
| Изменённый участок | `var opts = new JsonSerializerOptions ...` | `var opts = TrainingJsonPolicy.Options;` |
| Остальная логика | Без изменений | Без изменений |

В B добавляется **только внутри ExecutorUnit** приватный holder:

```csharp
private static class TrainingJsonPolicy
{
    internal static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };
    static TrainingJsonPolicy() { }
}
```

Явный static constructor исключает `beforefieldinit`: создание options приходится на первое обращение **внутри первого измеряемого TrainOnExample**, а не переносится в setup/warmup. Конфигурация, JSON DTO, converters, данные и порядок уроков одинаковы. Ни tokenizer/verifier TrainOnExample, ни MeasureAccuracy, ни builder не меняются. После первого использования options не модифицируются. Никакого cache текстов, десериализованных уроков или ответов модели нет.

**Конкретный будущий ремонт оригинала:** вынести lifetime options в этот приватный holder и заменить только строку408 `ExecutorUnit.TrainOnExample`; production изменение рассматривается пользователем после результата. Полный pipeline, тесты продукта и CI сейчас не исправляются.

## 4. Точное место, источники и границы изоляции

Авторские файлы из приложения сохраняются исполнителем без изменений:

`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JSON-OPTIONS-REUSE-01/host/prepare.py`  
`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JSON-OPTIONS-REUSE-01/host/Program.cs`  
`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JSON-OPTIONS-REUSE-01/host/compare.py`

`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JSON-OPTIONS-REUSE-01/host/run.py` — готовая авторская обвязка всех шести шагов; ещё не исполнялась.

**Короткий execution root:** `C:/xi/e/jo1`. Компиляция A/B, bin/obj, dependency copies, data и results находятся только там. Не использовать длинный protected result-path в имени компилируемого проекта. При существующем `C:/xi/e/jo1` → `BLOCKED_EXISTING_OUTPUT`: сохранить содержимое и передать точный путь; не удалять старый опыт и не подбирать самостоятельно другой ID.

**Read-only inputs:** core git objects669 и уже существующий `C:/Projects/llm_mvp_core/27.TranslationDomain/bin/Release/net9.0`. Protected `docs/plans/replanning-possible-diffs` только читается. Checkout, production source, tests, csproj проекта35 и его dispatcher не изменяются.

### Девять фактически компилируемых исходников

Все пути относительны `27.TranslationDomain/`, commit669. `prepare.py` читает их через `git show`, сверяет Git blob SHA и фиксирует SHA256:

| Файл | Git blob SHA |
|---|---|
| TranslationMachine.cs | fd0e736de3739236b4dde2d6b9d000f5ebae8416 |
| TranslationTrainedUnits.cs | 1742abfc9fb617dfc66e4d618e0c169da01a6ccc |
| TranslationUnitContracts.cs | 0f88d0d9cc3a62d2b2f8b0b9321142a4a4c1b0d8 |
| TranslationContracts.cs | 9ccff62783661fab24131e206b42280fe4cdc063 |
| TranslationCheckpointContracts.cs | 3879805b0504df7718adb2760e86aa6b4c3b13ab |
| TranslationDirectionAdapters.cs | ce139d39cfab36b7e919c31398cacde225b952d5 |
| TranslationRoutingFlow.cs | ae9da139a68b9e6d6cfd655425b0327053e06f0d |
| TranslationSemanticTokenizer.cs | ee6d81c7933fa33a9c57a2111990019ec55e5dab |
| TranslationMemorySourceLaw.cs | ba841f4f1839d504444d214feb73c80a4cc24f32 |

Compile items: ровно эти9копий + один общий Program.cs. Reference: копии уже существующих dependency DLL из указанного Release-каталога, **исключая production27.TranslationDomain.dll**, поскольку domain source компилируется в экспериментальный exe. ProjectReference на продукт, проект35 или12 отсутствует. `ImportDirectoryBuildProps/Targets=false` выставлены до SDK imports. A/B имеют одинаковые build settings и отдельные процессы.

Manifest сохраняет sourceCommit, sourceblob/SHA256, data blobs/SHA256, native original paths/SHA256, candidate source SHA256, все четыре host-файла, два project-файла, scorer и one-factor.diff. Перед первым обучением native host проверяет DLL/PDB identity исходной27assembly и PDB checksums всех9baseline source files; отдельно проверяет собственный `JsonOptionsA/B` assembly, его PDB и checksums9исходников+Program. SHA фактических bin dependencies и уже загруженных assemblies сравниваются с manifest до TRAIN, после TRAIN и после evaluation, вне измеренного окна. Production27assembly загружаться не должна. Полная source provenance всех transitive dependencies не заявляется.

Если существующий baseline DLL/PDB отсутствует, не соответствует9исходникам или dependency copy изменена — `BLOCKED_ENVIRONMENT`, **zero additional training**. Не собирать весь продукт и не чинить тесты для обхода блокировки. Сохранить preflight evidence и перейти по NEXT. Этот статус не означает неудачу механизма обучения.

## 5. Данные и независимое оценивание

Источник всех входов:

`docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/` на commit669:

- [training-batch-B.json](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/training-batch-B.json) — ровно8готовых native lessons, `sourceId=simulation`, все `executor-en_ru`, не heldout.
- [eval-inputs.json](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/eval-inputs.json) — вся прежняя панель24без gold:8exact,3near,8composition,5edge.
- [panel-gold.json](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/panel-gold.json) — frozen accepted phrases/rubric.
- [score.cjs](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/score.cjs) — frozen assess().

Единственное изменение экспериментальной копии scorer: два списка плеч `['A','B','C']` становятся `['A','B']`, поскольку здесь оба плеча обучаются и третьего нет. Rubric, normalize, assess() и24cases не меняются. Gold читается только scorer после сохранения всех96native outputs; native host его не открывает. Никаких новых DPO вызовов: исходные уроки честно названы синтетическими, а не приписаны внешней модели.

Исторический P125 показал19/24 после8уроков, включая3ошибочно принятых edge-ответа; [первичный отчёт](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md#L1-L59). Это baseline evidence, а не ожидаемый обязательный результат нового запуска. При новых числах записать именно новые outputs.

## 6. Бюджет и команда запуска

**Только после назначения этой строки реестра на выполнение.** Текущее задание — документация, не запуск. Модель исполнителя — текущая модель его сессии по умолчанию, без override; не путать её с памятью Student. Минимальный host, простые DTO, exceptions и try/catch допустимы для эксперимента.

Максимум:2малые сборки isolated host;2native process starts, A затем B;2batch8=16memory lessons;96ordinary outputs=2arms×2phases×24;0neural steps;0external model calls;0cold-load;0warmup/reverse order/repeat. Один scorer и один comparison. Не запускать B после неуспешного A; не считать incomplete run полноценным A/B.

Авторский `run.py` выполняет подготовку, build A, build B, A, B, comparison строго последовательно. **Единственная команда:**

```powershell
C:/Python314/python.exe C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/JSON-OPTIONS-REUSE-01/host/run.py
```

Используется уже известный `C:/Python314/python.exe`; нет поиска другого Python. Если этот executable/Windows, git, dotnet9 или Node недоступны — `BLOCKED_ENVIRONMENT`, без установки и обхода. Wrapper не допускает существующий execution root или `host/attempt-jo1`. Последний — точно назначенный staging-каталог только для логов подготовки, не для компиляции.

Обвязка записывает stdout/stderr и exit каждого из6шагов в `host/attempt-jo1/`, затем копирует logs и `command-receipts.json` в `C:/xi/e/jo1/result/`, когда этот root уже создан. Даже preparation failure сохраняет собственные stdout/stderr/receipt в staging. Поля receipt: argv, cwd, startUtc, finishUtc, exit, wallMs, timeoutSeconds, stdoutPath, stderrPath, status/error. Любой nonzero/timeout заканчивает попытку: B не запускается после failed A. Нет удаления, автоматического retry или перехода на другой host.

Внутренние команды `run.py`, отдельное ручное повторение не требуется:

```text
C:/Python314/python.exe <assigned-host>/prepare.py --repo C:/Projects/llm_mvp_core --out C:/xi/e/jo1
dotnet build C:/xi/e/jo1/A/Probe.csproj -c Release --nologo
dotnet build C:/xi/e/jo1/B/Probe.csproj -c Release --nologo
dotnet C:/xi/e/jo1/A/bin/Release/net9.0/JsonOptionsA.dll C:/xi/e/jo1 A
dotnet C:/xi/e/jo1/B/bin/Release/net9.0/JsonOptionsB.dll C:/xi/e/jo1 B
C:/Python314/python.exe C:/xi/e/jo1/compare.py --root C:/xi/e/jo1
```

`<assigned-host>` здесь означает единственный полный host-path из единственной команды; в authored run.py placeholder отсутствует. Таймауты: prepare30s, каждый build120s, каждый native process60s, compare30s. Wrapper завершает только собственное дочернее дерево при timeout/interrupt. После прямого пользовательского stop не продолжает receipt/report; существующие artifacts остаются. При native failure число завершённых updates берётся из сохранившегося native receipt либо остаётся неизвестным, не придумывается0.

`compare.py` после96native outputs проверяет frozen data/host hashes и копии фактически читаемых gold/eval, затем сам вызывает ровно один `node C:/xi/e/jo1/result/score.cjs`. Native Program открывает только training-batch и eval-inputs, **не gold**. Не запускать scorer вторично. Все сборки изолированы; source factor и бюджет не меняются.

## 7. Что именно измеряется

**Первичная ось:** `trainingThreadAllocatedBytes` — allocation между непосредственным входом/выходом одного реального `TrainStructuredBatchAsync`. Pinned implementation синхронный и возвращает completed Task; host проверяет completed + sameManagedThread. Это managed allocation, не RAM peak.

Рядом: `trainingProcessAllocatedBytes`, training wall и process CPU deltas, GC0/1/2, общий lifetime allocated/process peak, source/checkpoint bytes, правильные/принятые/ошибочно принятые outputs и все rubric errors. Цена подготовки и build отдельно от цены обучения. VRAM и neural layers N/A; стоимость модели-исполнителя, whole-PC/group memory не измеряется и не выдаётся за0.

Для A/B сохранить initial/before/trained/after checkpoints. Сравнить:

- A_initial=B_initial; A_trained=B_trained по каноническому state hash.
- before→trained меняется memory state в обоих плечах; RowsTrained сам по себе не доказательство.
- `exact/phrase/word/surface/KNN` counts и contents; frozen tokenizer/verifier/other directions.
- Ordinary queries не меняют полный canonical state; memory hash дополнительно сохранён отдельно. After output/route/verdict/confidence/metrics/warnings A/B совпадают либо различия выписаны.
- В обоих плечах тот же Verify и Translate consumer реально вызван ordinary Flow; не заменять ответы gold.

Из canonical state equality исключены **только** `ExportedAtUtc` и operational fields `TrainingHistory[].CpuMs/PeakRamMb/ElapsedMs`. Все исходные checkpoints остаются без вырезания этих полей; runtime costs сравниваются отдельно. Batch IDs, learned entries, version, readiness, history learning fields и identities сохраняются.

Comparator пишет A0,A1,B0,B1,ΔA,ΔB,ΔB−ΔA по каждому bucket/quality metric; неопределённые значения остаются null. `exactOptimizationSupported` требует initial/trained equality, реальной записи памяти, неизменных остальных owners, полного state без query mutations и output parity. При нарушении causal attribution=INVALID_COMPARISON, но наблюдаемые полезные deltas сохраняются. При валидном контрасте allocation attribution ограничен этой парой; wall/CPU/GC attribution=NOT_ESTABLISHED.

В wall/CPU одна холодная точка на плечо; наблюдаемый выигрыш честно записать как локальный полезный delta, **causal attribution=NOT_ESTABLISHED**. Не объяснять заранее JIT/порядком/шумом и не назначать повторы для признания локального результата.

## 8. Успех, ухудшения и кандидат в исправления

**Любое измеренное улучшение полезного показателя считается успехом по этому показателю и этому домену. Рост accuracy не обязателен; нет порога5%, требования статистической значимости или дополнительного прогона.** Все изменения остальных метрик, включая ухудшения, показываются рядом. Если состояние/outputs различаются, не скрывать выигрыш cost, но отдельно отметить semantic regression/confounding и не называть factor semantics-preserving.

Не подменять физический выигрыш изменением счётчика. При нулевом измеренном выигрыше `NO_MEASURED_GAIN`; при environment/failure `PARTIAL/BLOCKED`, не «обучение хуже».

Один русский `C:/xi/e/jo1/result/report.md`: вопрос; original method/patch; реальные команды и код SHA; данные/домен; actual learning state before/after;24ответа с gold для каждого плеча/фазы (таблица или linkedraw + ключевые differences); все cost/quality deltas и gains/losses; ограничения; точный original fix; решение `CANDIDATE_ONLY_USER_DECISION`. Если есть полезный измеренный delta, добавить `C:/xi/e/jo1/result/CANDIDATE.md` до120слов с метрикой, domain, ухудшениями и ссылкой на report. **Не менять protected required-changes.md и не внедрять production fix.** До выполнения эти файлы не заполнять вымышленными результатами.

Использовать skills из `docs/skills/`: intent-to-plan; plan-self-validation; generated-code-review; trainable-architecture-decision; experiment-design; prompt-patch-authoring; human-readable-architecture-audit и human-communication-GLOSSARY. Роль этих skills — точный scope, source→state→consumer, понятный русский отчёт. Старые product gates не требуют полный lifecycle для экспериментальной копии.

## 9. Порядок и немедленное переключение

Место в основном реестре: после blockers настоящего обучения и разрешения конфликтов в уже назначенных карточках; до unrelated инфраструктурных проверок. Эта строка не запускается при текущем документном аудите. В полной назначенной программе [этого реестра](04-execution-registry.md) после строки №40 **NEXT = `IDENTITY-M1-01`, строка №41**. Исполнитель не выбирает следующий опыт. При поручении только JSON-OPTIONS-REUSE-01 завершить один этот ID: `NEXT=END`, не запускать следующий самостоятельно.

Исторический инцидент зафиксирован словами пользователя: «четыре раза вместо переходя к следущему эксперименту продолжаешь прошлую задачу!!!» и «Закончить немедленно!!!!!!». Это свидетельство прошлого нарушения, не текущий stop документного аудита.

**Запрет обязателен и для этой карточки, и для каждого её prompt.** После «заканчивай и переходи к следующему» немедленно прекратить прошлый опыт: не дописывать report/review, не повторять запуск, не чинить тесты, не коммитить, не «улучшать» прежнюю задачу. Сразу выполнять прямо назначенный NEXT. После «остановить всё» немедленно прекратить любую работу и tool calls. Сохранённые raw/partial не удалять; их неполнота не оправдывает продолжение. Если native start упал до Train, честно trainingCalls=0; имеющиеся output files не удалять ради retry.

**Приоритет конфликтов:** текущая прямая команда пользователя → назначенная строка реестра/данная карточка → исторические общие правила. Нельзя из документного поручения вывести разрешение на runtime, а из успеха метрики — разрешение на production promotion.

