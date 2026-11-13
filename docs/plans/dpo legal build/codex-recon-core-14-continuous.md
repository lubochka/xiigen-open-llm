# Рекон: 14.ContinuousLearning — DpoPair и онлайн/continuous обучение (DPO, continue-training)

Сбор evidence для плана ILearningDynamicGraph. Только реально найденные пути; код прочитан, не исполнялся.

## Что это и где (точные ключевые пути)

Проект `14.ContinuousLearning` (assembly/namespace `ContinuousLearning`, .NET 9, csproj `C:/Projects/llm_mvp_core/14.ContinuousLearning/ContinuousLearning.csproj`). Назначение: накопить из проекта и из фидбека пользователя обучающие данные, провести инкрементальный LoRA-проход на домен и зарегистрировать адаптер. Файлы (все прочитаны, кроме Indexer — взят из контракта):

- `14.ContinuousLearning/Core/Core.cs` — записи-данные: `TrainingExample`, **`DpoPair`** (стр. 16-21), `FeedbackSignal {Accepted,Edited,Rejected}`, `FeedbackEvent`, `ModelAdapter`.
- `14.ContinuousLearning/Fabric/Interfaces.cs` — контракты: `IProjectIndexer`, `IDomainDatasetCache`, `IFeedbackRouter`, `IIncrementalTrainer`, `IDomainModelRegistry`, `IContinuousLearningPipeline`.
- `14.ContinuousLearning/Feedback/FeedbackRouter.cs` — превращает `FeedbackEvent` в `TrainingExample`/**`DpoPair`** (источник DPO-пар).
- `14.ContinuousLearning/Training/IncrementalTrainer.cs` — должен делать LoRA + DPO проход; **сейчас заглушка** (стр. 48-52).
- `14.ContinuousLearning/Pipeline/ContinuousLearningPipeline.cs` — оркестрация цикла index→cache→DPO→train→register→flush.
- `14.ContinuousLearning/Registry/DomainModelRegistry.cs` — in-memory реестр адаптеров по домену.
- `14.ContinuousLearning/Cache/DomainDatasetCache.cs` — in-memory + JSON-на-домен хранилище `TrainingExample`.
- `14.ContinuousLearning/Indexer/ProjectIndexer.cs` — сканер кода (не читан целиком; по контракту инжектит `ISemanticCompiler29` из 29).

## Как устроено / работает (кратко)

`DpoPair` (Core.cs:16-21) — чистая запись: `Prompt, Chosen, Rejected, Domain, CapturedAt`. Никакой логики/обучения на ней нет. Поле `Chosen` несёт фактический сигнал предпочтения; для REJECTED ставится плейсхолдер-строка `"[no preferred response]"`.

Источник DPO-пар — `FeedbackRouter.Route` (FeedbackRouter.cs:31-44, switch по сигналу):
- ACCEPTED → позитивный `TrainingExample` (weight 1.0) в кэш; пара НЕ создаётся.
- EDITED → `DpoPair(Chosen=UserCorrection, Rejected=ModelResponse)` + позитивный пример из правки (стр. 67-83).
- REJECTED → `DpoPair(Chosen="[no preferred response]", Rejected=ModelResponse)` (стр. 86-95).
Пары копятся в приватном `List<DpoPair> _dpoPairs` (стр. 24) и отдаются через `GetDpoPairs()`. Список **только в памяти**: нет Flush/persist DPO-пар, нет чтения их обратно при старте. Есть статическая `ComputeMaxWeight(total, numDomains, rare)` (D-09, динамический вес против катастрофического забывания) — но её результат в маршрутизации не применяется (вес всегда 1.0).

Цикл `ContinuousLearningPipeline.RunCycleAsync` (Pipeline.cs:42-91): индекс кода → `cache.Add` → `router.GetDpoPairs()` → по каждому домену `trainer.TrainAsync(domain, domainExamples, dpoPairs)` → `registry.Register(adapter)` → `cache.FlushAsync()`. То есть DPO-пары прокидываются в тренер, но один и тот же набор пар передаётся всем доменам (фильтрации `DpoPair.Domain` по домену нет).

`IncrementalTrainer.TrainAsync` (IncrementalTrainer.cs:38-53) — **ЗАГЛУШКА (stub)**: проверяет пустой домен/примеры, затем `path = Path.Combine(_adapterDir, "{domain}.lora")`, `await Task.CompletedTask`, `return Ok(path)`. НИ `LoraApplicator.Apply`, НИ `DpoTrainer.Step` НЕ вызываются. Комментарий (стр. 5-15, 48-49) явно делегирует реальную привязку LoRA/DPO в `16.AutonomousLearning.DataPipeline`, где конструируется живая `TransformerModel.LlmModel`. `DefaultConfig` (rank=8, alpha=16, q_proj/v_proj) задан, но не используется в этом проекте.

Персистентность/continue-training:
- `DomainDatasetCache.FlushAsync` (Cache.cs:59-70) реально пишет JSON по домену через `ICodeWriter`. Но `Add` чтения с диска при старте нет — кэш каждый раз пустой в памяти.
- `DomainModelRegistry` (Registry.cs) — **только in-memory** `Dictionary`. `Register/GetAdapter/ListAll` работают по словарю. Поле `_writer` (ICodeWriter) внедрено, но НИГДЕ не используется — комментарий «optionally persisted to registry.json» не реализован. Метода загрузки реестра нет.
- `ModelAdapter` хранит только `AdapterPath` (строку) — реального файла адаптера не создаётся (тренер заглушка).

## Релевантность к ILearningDynamicGraph (DPO / обучение / граф / оркестрация / измерение / teacher-provenance)

- DPO-формат: `DpoPair` (Prompt/Chosen/Rejected/Domain/CapturedAt) — пригодный кандидат-контракт пары для авто-DPO графа; но он завязан на «домены кода» (CODE_Q/BUG/PLAN/ARCH/REFACTOR, см. Core.cs:9), а не на узлы/рёбра ILearningUnit/Flow/Orchestrator/DynamicGraph. Для графа нужен другой ключ провенанса (узел-источник, ребро, роль), которого тут нет.
- Авто-построение DPO: `FeedbackRouter` — рабочий пример «сигнал → пара» (EDITED=chosen vs rejected, REJECTED=avoid). Это шаблон для генерации пар, НО только из ручного фидбека пользователя; авто-построения пар из несогласия узлов графа / арбитра здесь нет.
- Применение DPO / continue-training: фактически ОТСУТСТВУЕТ в этом проекте (тренер — заглушка; нет load-checkpoint→continue-training; реестр без persist/fresh-load). Для плана это «дыра-зависимость»: настоящий DPO/LoRA-проход живёт в `16.AutonomousLearning` (внешняя точка), а онлайн-цикл здесь — каркас.
- Оркестрация/измерение: `ContinuousLearningPipeline` — это последовательный конвейер (не граф, не fan-out/fan-in). Нет численных метрик (accuracy/loss/reward-margin/cpu_ms/ram_mb), нет held-out — измерения цикла нет.
- teacher/provenance: НЕ применимо здесь. Источник «chosen» — правка живого пользователя (адаптивная нога), не teacher-модель. Легального teacher-gate/provenance-полей (teacher_access_mode/model_id/license) в коде нет; при заимствовании в граф их надо вводить отдельно.

## Классификация уровня (по trainable-architecture-decision-SKILL.md)

Аллоуэд-уровни: ILearningMachine / ILogicalOrchestrator / ILogicalFlow / ILogicalUnit / ILearningMachinePair. Здесь НИ ОДИН класс не реализует эти уровни — это отдельная доменная подсистема с собственными интерфейсами.

- `DpoPair`, `TrainingExample`, `FeedbackEvent`, `ModelAdapter` — записи-данные (DTO). Не уровень и не хелпер; просто контракты данных.
- `FeedbackRouter` — внутренний детерминистический хелпер (switch ACCEPTED/EDITED/REJECTED → фиксированные правила построения пары). Не trainable-уровень: классификатор/маршрутизатор по enum, без обученного состояния. По скилу — «scorer/selector/router … не архитектурный паттерн», допустим только как internal detail. Роль для графа: генератор кандидат-пар (candidate generation), НЕ runtime-авторитет. Вердикт: **OK_INTERNAL_DETAIL** (честно помечен как роутер фидбека).
- `IncrementalTrainer.TrainAsync` — заявлен trainable («LoRA fine-tuning / DPO»), но это **stub** (см. Layer 5 generated-code-review: статус-путь без реального обучения, no-op training, не меняет обученного состояния). Вердикт по скилу: **REPORT_CLAIM_TOO_STRONG / FAIL Layer 5** — называть «обучением» нельзя, пока не подключены `LoraApplicator`/`DpoTrainer` и нет численных метрик. Это честный каркас (`PASS_AS_HONEST_UNTRAINED_SCAFFOLD`) при условии, что отчёт явно говорит «не обучает, делегировано в 16».
- `DomainDatasetCache` — внутренний хелпер хранения (in-memory + JSON write). Не уровень. Внутренняя деталь.
- `DomainModelRegistry` — внутренний хелпер-реестр (in-memory словарь). Не уровень; persist/fresh-load не реализованы — частичная заглушка.
- `ContinuousLearningPipeline` — последовательный конвейер. Похож по форме на оркестрацию, но НЕ `ILogicalOrchestrator` (не реализует контракт, нет узлов/рёбер/fan-out, нет trainable-авторитета на шагах). По графовому гейту скила это не DynamicGraph. Классификация: internal orchestration helper / каркас-конвейер.

Скрытых детерминистических «шорткатов под видом обучения» на runtime-авторитете нет, потому что обучения нет вовсе — риск в обратном: заявить готовый онлайн-DPO там, где только каркас.

## Пробелы / что доисследовать

- `Indexer/ProjectIndexer.cs` не прочитан целиком (инжект `ISemanticCompiler29`); для вопроса DPO/continue-training он второстепенен, но при заимствовании источника примеров — дочитать.
- Реальная привязка LoRA/DPO и continue-training находится в `16.AutonomousLearning` (DataPipeline) — вне области рекона; нужен отдельный рекон, чтобы понять, как `DpoPair`/`TrainingExample` реально попадают в `DpoTrainer.Step`/`LoraApplicator.Apply` и есть ли там load-checkpoint→continue-training и метрики.
- Нет тестов в области (в Glob тесты 14.* не найдены) — проверить, есть ли тест-проект, доказывающий поведение `FeedbackRouter`/pipeline (для двух-PASS статуса untrained-scaffold нужны тесты честности).
- Не подтверждены: persist/fresh-load DPO-пар (их вообще нет на диске), фильтрация пар по `DpoPair.Domain` в цикле, применение `ComputeMaxWeight` к весам.
- Контракты `OperationResult<T>` здесь из `AgentInfrastructure` (CLAUDE.md namespaces) — учесть при кросс-сборочном заимствовании в граф.
