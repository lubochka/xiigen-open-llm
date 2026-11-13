# R18 — первый содержательный учебный цикл: точная DPO-пара → Student → обычный ответ

**13.10.2026. Документальный результат R18, редакция 1.**

**Что подготовлено:** выбранный алгоритм, реальные точки изменения, четыре ограниченных implementation-пакета, предметный эксперимент и список оставшихся runtime-привязок. **Что не выполнено:** изменение core, реализация новых сервисов, запуск моделей/обучения/Windows/CI, независимая Astra экспертиза и измерение улучшения.

Статус: `IMPLEMENTATION_DESIGN_MATERIALIZED / RUNTIME_BINDING_OPEN / LEARNING_NOT_MEASURED`. Это не `FIRST_LESSON_EXECUTION_READY`: отсутствующие фактические checkpoint/profile/grant/threshold identities перечислены в §6, а не спрятаны в слове gate. Документ можно использовать для адресной реализации после отдельного разрешения на core work; разрешение писать planning на GitHub само не разрешает обучение или новые расходы.

## 1. Решение и границы

Первым остаётся **E1: существующий CodeDocumentation LlmModel**, а не Qwen, routing profile, kNN dictionary или новая модель с похожим именем. Исправляется минимальная связанная цепочка: точный материал → условная DPO objective → разрешённые параметры существующего Student → существующая попытка/checkpoint → генерация тем же ядром → сравнение на независимых задачах.

R18 обнаружил реальный генерационный kernel: `09.Inference/TextGenerator.cs`, используемый `CodeDocumentationSavedModelAssessmentService.AssessAsync`. Поэтому ordinary generation bridge имеет ограниченный дизайн и E1 не заменяется E2. **Bridge ниже проектируемый, не найденный готовый endpoint.** `CodeDocumentationRoutingFlow.ExecuteAsync` возвращает описание стадий и маршрута: выдавать его за сгенерированную документацию запрещено.

E2 domain-sensitive kNN/reranker, E3 Flow/Orchestrator и E4 Graph остаются самостоятельными экспериментами. Получение результата E1 не означает, что эти уровни научились. Не вводятся четыре новых trainer. Собственные R01–R17 carriers не переписываются; их полнота и независимость не повышаются.

### Закреплённые источники

- **P0:** `lubochka/xiigen-planning@36230b000d8237743038d5764178e48bcd78deb1` — основное чтение.
- **P1:** `lubochka/xiigen-planning@41e15b07a4fce8f930e9b2f2a27b021c649de93b` — поступивший R17 и обновлённая карточка R18. R12 DECISIONS в P1 имеет тот же blob `edfeeb3280927c187d60c69c669cc643091aa394`.
- **K:** `lubochka/llm_mvp_core@3c7f5f8c7809936e25579603c96ea6df8598fc40`.
- **D:** `llm mvp/design to code session`; **F:** `llm mvp/llm dynamic flow`; **V:** `llm mvp/visual studio & visual studio code continue`.

Исходные вопросы маршрутизированы непосредственно: D/WorkLeft-RAG-00 → D/WorkLeft-02 и INTEGRATION-01; V/RAG-06-llm-dynamic-flow → F/DECISIONS-LOCKED. Названия WorkLeft в этой строке сокращены только для чтения; точные пути находятся в SOURCES.csv. `original task.md` прочитан в части исходных четырёх ролей и many→many. Исторические provider shortlists и проценты из F не превращены в текущие разрешения или измерения.

Правила: [COMMON](run-prompts/COMMON.md), [общий контракт](COMMON-CONTRACT.md), [parallel join](PARALLEL-R01-R03-JOIN.md), [R17 evidence policy](R17-EVIDENCE-POLICY-2026-10-13.md), [выбор R12](results/R12/DECISIONS.md), [алгоритмические решения](COMPLETION-REPAIR-ALGORITHMS-2026-10-13.md), [доменные паспорта](COMPLETION-REPAIR-DOMAINS-2026-10-13.md).

## 2. Кодовая карта: наблюдение отдельно от изменения

Все K-пути ниже относятся к закреплённому K. Прочитанные диапазоны и blobs — в [SOURCES.csv](results/R18/SOURCES.csv). Строка является адресным trace, не обещанием полного аудита большого файла.

| Кодовый источник | Точная опора | Сейчас | Что поручается |
|---|---|---|---|
| K/14.ContinuousLearning/Training/DpoTrainingExecution.cs:TryBuildLogicalRequest, 360–560 | `LogicalDpoTrainingUnit.PrepareExecution(preferences, request.Tokenizer)` | Rich pair переносится в preference; raw authorship остаётся отдельным допуском | Передать и сохранить versioned material contract без потери Domain/context/author identity |
| K/18.CapabilityTraining/LlmIngredientLogicalUnits.cs:PrepareExecution, 1510–1710 | `new DpoVectorBatch(BuildBatch(tokenizer, ownedPreference), vector, 1f)` | Подготовка копирует vector и ограничивает размеры | Подготовить неизменяемые token IDs, response boundaries, masks и material digest до первой мутации |
| Тот же файл:BuildBatch/Compact, 2100–EOF | `return compact.Length <= 480 ? compact : compact[..480];` | Whitespace схлопывается, каждый текст обрезается; raw passport этого не отменяет | Удалить Compact именно из выбранного train-material path; overflow отклонять, а не прятать |
| K/01.Tokenizer/BpeTokenizer.cs:Encode/Decode, 1–240 | `public List<int> Encode(string text)` | Byte-BPE; Encode не отдаёт offsets; EosToken = последнему vocabulary ID | Сохранить vocabulary; определить сегментированную пару без угадывания joint-BPE boundary; отдельно проверить stop token |
| K/11.DPO/Models/DpoBatch.cs, весь файл | `ChosenMask`, `RejectedMask` | Маски обозначены как real-token/attention; отдельной response mask нет | Добавить отдельные loss masks и versioned objective identity; legacy inference не переопределять |
| K/11.DPO/DpoTrainer.cs:ComputeLogProbs/BackwardLogProbs, 550–820 | `return cnt > 0f ? sum / cnt : 0f;` | Mean по prompt+answer; backward тоже делит на count | Отдельная response-only sum ветка forward/backward для выбранной objective |
| Тот же файл:EvaluateAverageNextTokenLogProbability | `return ComputeLogProbs(logits, ids, mask);` | Публичный average inference scorer | Сохранить семантику и callers; не заменять общий helper глобально |
| Тот же файл:StepVector/VectorSignal | `var scaledWeight = vectorWeight * vectorSignal;` | VectorSignal считает активные координаты, не кодирует смысл output; reported loss не содержит этот multiplier | Weighting заморозить; отдельно записывать raw DPO loss и фактически оптимизируемый weighted loss |
| Тот же файл:Step/StepVector | `_policy.Parameters().Where(parameter => parameter.Value.RequiresGrad)` | Optimizer получает фактически активные параметры | Разрешённые активные ключи сравнивать с manifest/installed adapter objects; не выводить freeze из имени LoRA |
| K/18.CapabilityTraining/LlmIngredientLogicalUnits.cs, 1050–1260 и 1810–2030 | `LoraApplicator.ApplyFreshSeededPair` / `CreateDpoTrainer` | Существуют seeded topology, отдельная reference и existing trainer | Переиспользовать, не добавлять альтернативный цикл или скрытую новую инициализацию |
| K/26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.cs, 430–660 | `ResumeQualityDecisionFromExistingAttemptAsync` / `ReopenTrialCheckpointFromAttempt` | Проверяет already-applied, возобновляет оценку, создаёт baseline, выполняет существующий executor, переоткрывает trial | Сохранить lifecycle; связать новую objective/material identity с тем же attempt и quality decision |
| K/26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedModelCheckpointStartService.cs:PrepareStart, 1–240 | `LoadLatestCompatible` | Проверенный START из repository checkpoint; ошибку не заменяет случайным cold start | Для serving закрепить выбранный accepted manifest и проверить те же bytes при загрузке; не просто взять самый большой trial step |
| K/14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentService.cs:AssessAsync, 1–245 | `new TextGenerator(request.Model, request.Tokenizer)` | Настоящая генерация уже существует в экзамене; оценщик здесь Qwen client | Переиспользовать generation kernel, но не закрытый корпус и не Qwen как неподтверждённую Astra экспертизу |
| K/09.Inference/TextGenerator.cs:GenerateCore, весь файл | `_model.ForwardWithCache(...)` | Реальный prefill/decode, seed/cancellation, context guard, отдельный KV-cache | Добавить узкий versioned detailed-generation overload для нового consumer; старые overloads сохранить |
| K/26.TrainableRouter/Training/CodeDocumentationRoutingFlow.cs:ExecuteAsync, начало файла | `value.Stages.Select(stage => ...stage.StageTag...stage.SymbolId...)` | Route/stages, не Student documentation text | Не выдавать readiness receipt за выученный ответ; сохранить старый контракт |
| K/25.ModelServer/Program.cs:ConfigureLocalLoopbackRoutesCore, 575–840 | `app.MapCodeDocumentationSavedModelAssessment();` | В прочитанной map есть assessment/training/preflight; ordinary bridge не найден | Добавить отдельный read-only generation route в том же host; это предлагаемый endpoint |
| K/25.ModelServer/Program.cs, 1–270 | `@"C:\Projects\llm_mvp_core"` | Owner-pinned local trusted root | Не объявлять любой Linux clone готовой заменой; не превращать локальную зависимость в blanket GitHub Windows gate |

## 3. Законченный целевой алгоритм

### 3.1. Pair material v2: не потерять то, чему учим

Одна DPO-пара содержит один frozen prompt x и два ответа y+ и y− **на этот же x**. Domain/project/stack, author provenance, права, lesson identity, judge decision и tokenizer identity — часть связанного контекста. Изменённые текст или контекст не наследуют старое assessment. TIE/BOTH_BAD/UNJUDGEABLE идут в журнал без optimizer admission.

Для первого пути выбирается **segmented-byte-BPE-v2**: `p = Encode(x)`, `a = Encode(y)`, `ids = concat(p,a)`. Это явная версия кодирования, а не предположение, что Encode(x+y) имеет тот же prefix. В serving используется ровно `Encode(x)`, поэтому prefix policy/reference/ordinary generator совпадает. Не добавлять пробел, newline, system instruction или EOS после оценки пары. Используемый prompt-template должен быть выбран до authoring и входить в x.

Перед подготовкой мутации проверить: x и оба ответа непусты; UTF-8 без недопустимых surrogate replacements; Decode(p) и Decode(a) возвращают исходные UTF-8 bytes; IDs в диапазоне vocabulary и точно представимы tensor storage; tokenizer/manifest/schema identities совместимы; pair count для S04 равен одному; полный encoded budget укладывается в действующий V2 capacity и ContextLen. Различие raw ответов, исчезнувшее в token material, — `MATERIAL_CONTRAST_LOST`, не успешный урок.

Если overflow, возвращается отказ без optimizer/teacher retry. Исполнитель не увеличивает capacity произвольно. Отдельное создание более короткого полноценного урока требует нового разрешённого authoring и нового exact-pair assessment; это не обрезанная старая пара.

Для chosen/rejected можно сохранить существующее правое padding до общего T. `RealTokenMask` отмечает реальные позиции, **ResponseLossMask** — только индексы j с `prefixLength <= j < realLength`. В causal score logits[j−1] предсказывают ids[j]; первая response позиция использует последний prompt logit. Padding и prompt не являются response targets. Prompt остаётся контекстом, поэтому отсутствие его прямого target loss НЕ означает запрет всех градиентов через его hidden states.

Prepared material копирует массивы, хранит версии и digest от raw/context/tokenizer/framing/IDs/masks/lengths. Перед mutation используется тот же snapshot, а не повторный Encode изменяемых caller fields. Raw admission и prepared digest выполняют разные роли: digest не выдаёт права на обучение. Изменение authoritative source после admission приводит к refusal/recovery согласно текущему effect boundary.

**EOS не выдумывать.** В прочитанном BPE `EosToken` — последний ID, без доказанного отдельного special-token договора. v2 не добавляет искусственный EOS в lesson. Новый detailed-generation overload получает optional stop ID только из проверенного manifest; без такого ID работает до фиксированного maxNewTokens и возвращает `LENGTH_LIMIT`/явный незавершённый результат. Нельзя молча переименовать обычный BPE token в EOS, расширить vocabulary или выдать обрезанный ответ за полный. Legacy Generate сохраняет старое поведение; все arms нового эксперимента используют один v2 generation contract.

### 3.2. Objective и семантический получатель

Для каждой response последовательности вычисляется `ell(y|x) = sum_j log p(ids[j] | ids[0..j−1])` только по ResponseLossMask. Затем:

```text
m = beta * ((ell_policy(y+|x) - ell_reference(y+|x))
          - (ell_policy(y−|x) - ell_reference(y−|x)))
base_loss = softplus(−m)
a = frozen_existing_VectorSignal(teaching_vector)  # для одной пары
optimized_loss = a * base_loss
grad_m = a * (sigmoid(m) − 1)
chosen_upstream = +beta * grad_m
rejected_upstream = −beta * grad_m
```

Это выбранная response-conditional DPO с зафиксированным существующим sample multiplier. Beta и optimizer settings берутся из exact existing settings; на K Beta=0.1, DefaultLearningRate=1e−5. Эти константы — наблюдение кода, не новый тюнинг под удачный результат. Forward и backward используют одну response mask и **не делят на число tokens**. Стабильный softplus/log-sigmoid не должен терять смысл на больших margins. При `policy=reference`, m=0 ожидаемо; это не причина выбросить валидную пару. Нулевой фактический gradient либо отсутствие measurable gain фиксируются, а не исправляются фиктивным изменением веса.

`EvaluateAverageNextTokenLogProbability` остаётся average API. Legacy diagnostic/evaluation callers сохраняют прежнюю семантику; mutating v2 входы обязаны иметь явную objective/material version. Missing response mask у mutating v2 не заменяется all-ones. Старые неисполненные batches не объявляются автоматически совместимыми: producer мигрируется адресно, неподдержанный caller получает честный отказ до effects. Это конкретная migration requirement, не требование предварительно переписать весь продукт.

Для reporting оставить прежний raw loss отдельно и добавить optimized_loss, multiplier, response-token counts, margin, trainable-key digest и objective version. Не сравнивать числа старого full-sequence mean с новым sum как доказательство улучшения.

**Получатель:** существующий repository-owned CodeDocumentation PolicyModel. В выбранном LoRA-only первом вмешательстве разрешены A/B именно установленных project adapters, соответствующих canonical target modules и manifest. Base parameters заморожены; reference целиком заморожена и имеет независимое storage. Получить реальные checkpoint keys от installed objects/`GetCheckpointParameterEntries()`, сравнить с `RequiresGrad` group; пустая/лишняя/неразрешённая группа — refusal. До этого нельзя заявлять, что current код уже adapter-only. Не угадывать model rank, hidden size, adapter IDs или checkpoint по имени папки.

TrainingVector в StepVector пока лишь множитель: новая semantic tokenization/projection/kNN не приписывается ему. LoRA — параметризация адаптации; `P_domain: source-space → target-space` остаётся отдельным последующим договором с decoder, labels и consumer.

### 3.3. Existing state lifecycle, не второй trainer

```text
read current attempt/journal → resolve already-applied/unknown effects
→ bind accepted parent + tokenizer + topology + optimizer/RNG identities
→ freeze operational examination and all controls
→ select/author/assess one new lawful pair under existing roles
→ prepare exact immutable v2 material
→ isolated trial: existing executor → LogicalDpoTrainingUnit → DpoTrainer
→ persist trial + optimizer/RNG/full identity using existing publication owners
→ reopen trial; examine before changing serving selection
→ IMPROVE and all admission/retention/cost conditions: existing accept path
→ otherwise retain parent; preserve rejected/inconclusive attempt
→ cold ordinary generation from the actually accepted local identity
```

Снимок включает weights/base/adapters, optimizer moments/step, RNG/dropout, counters, objective/material/tokenizer/topology/schema identities и parent/attempt association. Поскольку E1 не меняет kNN/graph, их manifests фиксируются как frozen, а не бессмысленно переобучаются или пересобираются. Если они изменились фактически, E1 contrast invalid и нужен согласованный snapshot/migration. Измерять память этих состояний всё равно обязательно.

При неизвестном исходе optimizer нет разрешения повторить пару. Возобновляется quality/recovery по existing coordinator/journal, а не новая попытка с другим UUID. При concurrent parent promotion не затирать чужую accepted revision: compare identity, оставить trial в известном состоянии, отдельно решить re-evaluation. Failed assessment не означает failed update; rollback serving pointer не отменяет историю уже применённой пары.

### 3.4. Bounded ordinary consumer, спроектированный здесь

Предлагаемые, пока НЕ существующие носители:

- `26.TrainableRouter/Training/CodeDocumentationGenerationService.cs` — read-only domain service в существующем CodeDocumentation owner/project, не новый trainer.
- `25.ModelServer/ContinuousLearning/CodeDocumentationGenerationEndpoint.cs` — local read-only adapter.
- Proposed route: `POST /api/code-documentation/generate`. Это спецификация нового endpoint, **не готовая команда запуска**.

Вход service: existing CodeDocumentationRequest плюс server-owned immutable accepted-model handle, generation contract и scope. Выход: типизированный результат с RequestId/SymbolId, фактическим model/tokenizer/objective/generation identity, text/tokens, stop reason, law/source warnings и временем. При нескольких inputs сохранить associations; пустой output не сдвигает zip-связь следующего input. Ошибка/abstain — typed result, не fabricated documentation.

Endpoint принимает только разрешённый пользовательский запрос, не пути к весам, arbitrary trial ID, examiner references или training authority. Accepted identity выбирает действующий server-owned профиль; загрузчик проверяет bytes и compatibility. При fail-closed loader error нет silent cold start. `CodeDocumentationRoutingFlow.RouteDocumentationAsync` можно использовать для действующих scope/source/version/transit checks, но не переименовывать его старый ExecuteAsync result в generated answer.

Service вызывает существующий TextGenerator с одинаковым v2 prefix/stop/seed/settings. Добавляется ограниченный overload/result carrier в `09.Inference/TextGenerator.cs`; старые public overloads не меняются. Новый overload использует те же ForwardWithCache, sampler, cancellation и ContextLen guard; detail result не требует нового LLM stack. Assessment использует этот же service/kernel через **внутренний trusted isolated snapshot binding** для parent/control/trial. Этот binding не доступен обычному HTTP caller и не меняет production accepted selection до verdict. Экзамен остаётся владельцем закрытых reference answers; они не попадают в ordinary service.

После accept новый процесс обычным proposed endpoint получает принятое состояние и сообщает его identity. Совпадение hash без фактической генерации не закрывает adoption. Отказ от trial сохраняет ordinary parent. Адаптация, найденная только в прямом assessment helper, но не в ordinary request path, оставляет `ADOPTION_OPEN`.

## 4. Четыре полных дочерних поручения

Это **не четыре выполненных запуска** и не R26–R29. Они составляют назначаемую ближайшую реализацию; сейчас создан только документ. A и начальная часть C могут проектироваться параллельно после фиксации общего input contract; B принимает A; D подготавливает безопасные панели параллельно, но не запускает mutation до A+B+C и допусков. Объединение делает один owner по exact candidate SHA. При большом файле дочитать его частями, не сокращать предмет до удобного excerpt.

### R18-A — faithful pair → immutable model material

**Промпт исполнителю.** Исправь выбранный путь передачи одной разрешённой CodeDocumentation пары так, чтобы optimizer получил именно различие, оценённое judge. Реализуй segmented-byte-BPE-v2 из §3.1 в существующих владельцах. Не меняй teacher policy, не создавай пары, не тренируй tokenizer и не увеличивай capacity.

**Контекст и навыки.** K/`docs/skills/trainable-architecture-decision-SKILL.md`, `code-examination-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `output-readability-gate-SKILL.md`; перед patch читать применимые current core инструкции, не переносить historical blanket QA. P/COMMON, R17 matrix и source rows K01–K04 в SOURCES. Прочитать полностью изменяемые DpoBatch и необходимые sections больших producer файлов, всех найденных mutating callers. Full-file review больших файлов завершить до product diff; excerpt R18 не его замена.

**RAG/primary.** D/`09.09.26WorkLeft-RAG-00-usage-guide.md` → `09.09.26WorkLeft-02-teaching-chinese-model.md` → `09.09.26WorkLeft-INTEGRATION-01-training-into-work-program.md`; source question: точная пара, авторство и запрет повторного update. Не перечитывать остальные архивы без нового disputed requirement.

**Allowed diff после отдельного core поручения:** `14.ContinuousLearning/Training/DpoTrainingExecution.cs`, `18.CapabilityTraining/LlmIngredientLogicalUnits.cs`, `11.DPO/Models/DpoBatch.cs`; маленький internal material carrier в этих существующих проектах при необходимости. `BpeTokenizer.cs` сначала только читать; изменение vocabulary/format не входит. Не редактировать datasets, веса, workflow, skills, unrelated domains.

**Решение:** заменить Compact в selected path; отдельно Encode exact prompt/response; validate UTF-8 roundtrip/ranges/capacity/contrast; создать real-token и response-loss masks с target offsets; detach inputs; привязать preparation к exact context/versions; fail before mutation при mismatch. Не делать фиктивный [0] ответ из пустой строки.

**Хороший пример:** длинные ответы отличаются важным условием после прежней 480-й позиции; либо различие дошло до tokens, либо весь урок явно отклонён по capacity. Whitespace внутри source literal сохраняется. **Плохой:** увеличить 480 до 4000, обрезать после judge, считать raw hash доказательством смысловой сохранности.

**Края:** multibyte/combining symbols, CRLF внутри literal, граница BPE, empty x/y, identical token sides, invalid token ID, mutable caller после await, stale tokenizer, prompt+answer на capacity boundary. Это иллюстрации, не approved teacher data.

**Проверка результата без coverage campaign:** code→material trace, несколько адресных before/after dumps только разрешённых DOCUMENT_EXAMPLE, ручное сопоставление UTF-8/IDs/masks/границы и отказа до effects. Не добавлять тестовый suite и не запускать training/Windows. Минимальная сборка затронутого проекта допустима только в последующем разрешённом core work scope; она не доказывает обучение.

**Выход:** patch, material-contract version, caller compatibility table, exact evidence и ограничение. **Приёмка:** все fields подготовлены без потерь и есть consuming B input, а не только новый DTO. **Передача B/C/D:** prepared version, prefix/response contract, token/stop identities и unsupported old callers. Один bounded correction cycle по замечаниям; далее явный blocker вместо loop-until-green.

### R18-B — conditional objective → конкретные trainable keys

**Промпт исполнителю.** В существующем DpoTrainer реализуй response-only sequence sum и согласованный backward по §3.2. Зафиксируй именно project-LoRA получателя существующего CodeDocumentation Student. Не создавай trainer, не подменяй neural learning изменением routing weights, не меняй public average scorer.

**Контекст/навыки:** результат A; R12 C/E/F, COMPLETION-REPAIR-ALGORITHMS §§3,8–9, R17 optimizer/learning rows; `trainable-architecture-decision`, `code-examination`, `plan-review`, `prompt-patch-authoring`, `output-readability-gate` из K/`docs/skills/` с суффиксом `-SKILL.md`. Математический primary: DPO §4 Eq.7 и implementation appendix; LoRA §4. Ссылки в §8.

**Source route:** WorkLeft-INTEGRATION-01 existing Student → `DpoTrainingExecution.TryBuildLogicalRequest` → `LogicalDpoTrainingUnit` → `DpoTrainer.StepVector`; `LoraApplicator` installed objects → checkpoint keys. Читать полные изменяемые methods и все affected callers; отдельную публичную inference ветку явно найти и сохранить.

**Allowed diff:** `11.DPO/DpoTrainer.cs`, result metrics carrier после подтверждения actual path, `18.CapabilityTraining/LlmIngredientLogicalUnits.cs`; A-owned DpoBatch менять только через agreed contract/join. LoraApplicator/Linear читать; править только при доказанном отсутствующем installation/freeze seam и отдельном минимальном patch. Не менять ModelConfig/rank/learning rate под найденный outcome.

**Решение:** отдельные response sum helpers, те же masks и отсутствие count division в backward; стабильный log-sigmoid; frozen VectorSignal и отдельный optimized loss; полный allowlist actual trainable keys; base/ref frozen independent; model-local trial, не shared serving instance. Legacy scorer сохраняется; v2 missing contracts fail closed до mutate.

**Хороший пример:** при длинном одинаковом prompt score отражает условное предпочтение ответа, а не разбавляется prompt length; policy=reference даёт m=0 и не объявляется invalid pair. **Плохой:** оставить backward mean при forward sum; заменить shared ComputeLogProbs и незаметно изменить обычный inference; назвать ненулевой hash успешным обучением.

**Края:** unequal response lengths, one response token, padding, extreme logits/margins, NaN, all-zero loss mask, no active adapter, unexpected base RequiresGrad, aliasing reference/policy, zero gradient, old objective checkpoint continuation. Смена objective требует новой runtime compatibility identity; optimizer continuation не сбрасывается молча.

**Проверка результата:** аналитическая трасса loss и знаков/масштаба derivative; до разрешённого update — фактический key manifest и независимость storage. В будущем допущенном diagnostic scope — ограниченное численное наблюдение objective/gradient, а не новый coverage suite; результат не означает Student utility. Не перезапускать одну пару для получения красивого loss.

**Выход:** patch, objective version/формула, trainable/frozen key receipt, backward correspondence, preserved public scorer contract. **Приёмка:** ни loss, ни recipient не оставлены на усмотрение следующего worker; concrete runtime key list либо подтверждён, либо назван blocked item G2. **Передача C/D:** exact candidate SHA, state identities, optimized/reporting metrics и zero-effect disposition.

### R18-C — existing checkpoint → read-only ordinary generation

**Промпт исполнителю.** Соедини реально существующие repository checkpoint/генератор и обычный CodeDocumentation запрос согласно §3.3–3.4. Верни настоящий текст Student и identity использованного состояния. Не строй новый lifecycle, не открывай trial weights внешнему caller и не называй routing stages документацией.

**Контекст/навыки:** A material contract, B state/objective contract, R12-F, WorkLeft-02/INTEGRATION-01, R17 state/host rows; K skills `trainable-architecture-decision`, `code-examination`, `phase-planning`, `plan-review`, `output-readability-gate`. До изменения service dependency проверить actual csproj references; отсутствие reference решается адресно в существующем project, а не новой платформой.

**Source route:** coordinator 430–660 → existing checkpoint StartService/loader → assessment service AssessAsync → TextGenerator.GenerateCore → ModelServer ConfigureLocalLoopbackRoutesCore. Отдельно прочитать routing Flow consumer; проверить caller/dependency tree, не повторять full RAG inventory.

**Allowed diff:** proposed files и изменения TextGenerator/Program из §3.4; существующий assessment service — только для общего generation kernel/input compatibility. Publication coordinator/loader/quality journal вначале reuse/read-only; изменять только необходимое objective/material compatibility/adoption binding, не переписывать custody и recovery. Сохранить legacy public Generate и RoutingFlow.ExecuteAsync.

**Решение:** server-owned accepted handle; pin manifest/version перед загрузкой и повторно проверить выбор; private isolated trial binding для examination; typed association-preserving result, explicit stop reason/cancellation; same v2 generator kernel; no corpus/reference leakage. После accept cold ordinary process должен реально вызвать модель принятой identity. После отказа продолжает parent. Endpoint путь выше — новый API design, не уже доступный маршрут.

**Хороший пример:** обычный запрос после accept возвращает Student text, model hash нового child и source associations; до accept публичный consumer остаётся на parent. **Плохой:** вернуть `training_run_id` и stages вместо текста; hot-swap trial в production для его оценки; загрузить adapter другого domain из-за одинакового shape.

**Края:** acceptance race, changed tokenizer, missing/corrupt accepted artifact, unauthorized client-supplied root, stale KV-cache, partial result after cancellation, native trusted-root mismatch, length-limit answer, evaluator generation отличается от ordinary serialization, restart после update с unknown outcome.

**Проверка результата:** trace request→scope→actual loader→TextGenerator→typed response; в последующем разрешённом runtime read-only generation из accepted и isolated trial, cold reload и fault/recovery observations. Никакого повторного optimizer ради проверки adoption. Windows требуется только для реального затронутого native/host свойства, не как универсальный экспериментальный барьер. Не обходить configured required checks.

**Выход:** minimal patch, proposed API contract, cold-adoption receipt/или конкретный gap, preserved lifecycle and native boundary table. **Приёмка:** один ordinary kernel с реальным caller, нет фальшивой генерации, нет serve-before-accept. **Передача D:** invocable method/API после реализации, exact startup/profile/source rights и observed state identities; придуманный curl до implementation не выдавать.

### R18-D — один причинный экзамен и передача в настоящие кампании

**Промпт исполнителю.** Подготовь и, только при отдельном разрешении, исполни E1-CODEDOC-CONDITIONAL-DPO-v2 по §5. Различи пригодность пары, actual update, ordinary adoption и measurable utility. Не измеряй «обучение» количеством тестов. Не запускай никакую модель, пока G1–G6 не закрыты actual receipts.

**Контекст/навыки:** A/B/C exact candidate, R13 FINDINGS с repair override, R17 matrix, seven domain passports, WorkLeft S04/S06/S07; K skills `trainable-architecture-decision`, `plan-review`, `output-readability-gate`, плюс фактически обнаруженные experiment/measurement skills — имена и loaded spans записать, не выдумывать load. Source question: одинаковые lawful task families и одинаковый ordinary consumer, а не новая универсальная benchmark framework.

**Allowed work:** existing assessment/quality pipeline instrumentation, server-owned evaluation configuration и authorised experiment artifacts; product files только из согласованного C/D patch list. Не публиковать закрытые case answers, веса, raw user input или teacher calls из documentary scope. Никакого нового coverage suite/Windows dispatch; actual required invariant/host evidence по R17 сохраняется.

**Решение:** зарегистрировать immutable card до outcome; operational weakness selection отдельно от untouched final; exact granted new pair; same parent/control memory and v2 generation; одна trial mutation через existing owner; evaluate isolated trial, сопоставить domain/family scores/noise/retention/time/full resources; verdict до promotion; cold ordinary observation после принятия. Aggregate report не подменяет raw protected evaluator evidence.

**Хороший пример:** pair valid, update observed, но новых nullable families Student не решил лучше: `INCONCLUSIVE` либо `REGRESS` по заранее заданному правилу, parent сохранён. **Плохой:** выбрать subset удачных ответов после просмотра; превратить два корректных варианта в forced preference; оценить своим teacher-автором и назвать independent Astra.

**Края:** student уже достигает разрешённого target; no suitable new source; judge TIE; low numerical margin; missing closed reference; evaluator disagreement; violated law; storage-only change; no-update control unexpectedly improves; budget exhaustion; unknown optimizer outcome. Ни один не запускает автоматический retry до удачи.

**Проверка/выход:** заполненная card, actual run IDs/versions, qualified verdict, protected evidence locations, public aggregate and adoption outcome. Все неисполненные метрики — NOT_MEASURED. Если фактического Astra UltraCode reviewer нет, статус `INDEPENDENT_ASTRA_REVIEW_NOT_PERFORMED`, не смена label у Qwen/client или текущего автора.

**Передача:** R19/R20 — реально готовая очередь и prompt binding; R22 — последующие E2/E3/E4 contrasts; R23 — фактический ресурсный предел/unknown recovery; S06.08B/S06.09C и S07 — learning curves новых уроков. Не добавлять автоматический R26.

## 5. Предметный первый эксперимент: E1-CODEDOC-CONDITIONAL-DPO-v2

### Гипотеза, задача и пример

**Предмет:** корректное объяснение C# кода с nullable/control-flow смыслом в поддерживаемом проектом language version. **Гипотеза:** один новый допустимый урок меняет project-adapter так, что обычный Student лучше сохраняет source-grounded semantic claims на других source families, без потери retention и недопустимой стоимости. **Опровержение/ограничение:** при одинаковом consumer и материале выигрыша за пределами шума нет, эффект только на учебном snippet, растут critical errors либо полезность существует только в oracle-domain и исчезает при обычном mapping.

DOCUMENT_EXAMPLE, NOT_APPROVED_FOR_TRAINING: для `int? Size(string? s) => s?.Length;` объяснение «при null возвращается null» противопоставляется «при null возвращается 0». Оба final ответа реального урока должен написать допущенный Chinese teacher на **один и тот же frozen source**, затем CODEX/Astra выбирает preference и отдельно оценивает exact pair. `s?.Length ?? 0` — другая входная задача, а не rejected side первой. Разные корректные перефразировки могут быть TIE. Новые legal source bytes не берутся из этого документа как готовая обучающая пара.

### Зафиксированные arms и атрибуция

Parent P: выбранный accepted checkpoint, исполняемый уже новым v2 кодом A/B/C. Control C0: fresh read-only instance P с теми же доступными allowed memory/index/graph/context и cost policy, **без optimizer**. Challenger T: ровно один admitted update от P; inference из переоткрытого isolated trial. Код, serializer, decoder, law rules, доступная память, retrieval pool и генерационные settings совпадают между arms. Улучшение от исправления Compact само не приписывается одному gradient step; все arms сравниваются после этой code migration.

No-update не обучается второй раз на той же паре. Ablation ветви с дополнительным применением одной пары запрещены текущим no-repeat. Pair quality и teacher/evaluator noise наблюдаются отдельно от Student score. Если arms различаются новым retrieval corpus, classifier или cache policy, результат не является чистым E1 эффектом.

### Панели, domain laws и измерения

Training source family отделена от operational development и confirmation families по происхождению/шаблону, не случайной строке. Rename одного snippet не независимый family. Operational weakness selection может предложить nullable тему; если слабость не подтверждается, не вынуждать ненужный урок. Untouched final certification panel не используется для выбора уроков, tuning или ежеблочного feedback.

Confirmation strata: nullable reference access; nullable value returns/chain; coalescing контрфактические исходники; retention на ранее доступных CodeDocumentation task families. Getter exception и скобки/граница conditional access — важные edge classes, а не обещание исчерпывающего покрытия C#. Конкретные allowed family IDs, N, версии и rubric поступают из подтверждённого server-owned corpus/profile и замораживаются до outcome. Не копировать references в planning или teacher prompt.

По каждой family записывать correct required claims, missing essential claims, invented/wrong-source claims и critical law violations. Семейства имеют заранее заданные веса; множество перефразировок не увеличивает вес исходного source. Дополнительно: oracle-domain vs predicted-domain, mixed/unknown abstention и downstream solver quality. E1 держит mapper fixed: улучшение solver не закрывает обучение mapper.

Для uncertainty использовать paired family-level comparison с заранее выбранным методом из существующего evaluation/noise profile. Численный meaningful-effect threshold, retention tolerance, sample size и budget берутся из accepted profile до результата. Они **не наблюдались в R18**; G4 блокирует RUN, а не заполняется универсальными 95%/0 или произвольным p-value. R18 не отменяет noise-aware product policy и не подменяет конфигурацию новым субъективным порогом.

Измерять wall time от получения permitted lesson input до qualified verdict и отдельно teacher authoring, pair assessment, material preparation, update, save/reopen, generation, evaluation, promotion/adoption. Для latency показывать распределение и N; p95 маленькой выборки не превращать в надёжный SLA. Peak RAM/VRAM — high-water mark всего локального процесса/дерева, а не сумма несинхронных максимумов. Учитывать все одновременно живые policy/reference/assessment copies, activations/logits, gradients/moments, tokenizer, index/graph/cache, snapshots и retained disk. Отдельно teacher/evaluator location, calls/tokens/cost и failed/cancelled work. Не утверждать, что адаптер поместился на ПК, если memory замерили только для A/B matrices.

### Stop, outcome и установка

`INVALID_EVIDENCE`: потеря contrast/lineage, leakage, unmatched arms, неверный recipient, неизвестные actual profile/cost/measurement identities. Это не REGRESS. `REGRESS`: валидное измеренное ухудшение по заранее принятой rubric/retention/law rule. `INCONCLUSIVE`: evidence валиден, но отличить эффект от шума/предела панели нельзя. `IMPROVE`: заданный meaningful/noise-aware критерий выполнен, law/retention/resource/admission сохранены. `NOT_MEASURED`: эксперимент не исполнялся.

При REGRESS/INCONCLUSIVE сохранить parent и полноценный результат. При exhausted budget остановиться, не добирать красивые случаи/повторы. При unknown update reconcile durable attempt прежде всего. Только допустимый IMPROVE + действующая authority допускает existing promotion; cold ordinary use отдельным фактом подтверждает adoption. Повторные экзамены для калибровки возможны только в зарегистрированном repeat budget без нового optimizer и без выбора лучшего повтора.

**Bulk:** S04 — первый один урок. Далее существующие S06.08B/S06.09C/S07 кампании: окна последовательных новых pair IDs, counts attempted/admitted/applied/accepted, cumulative time, family-level accuracy, retention, worst-domain и disk/memory growth. Bulk не минибатч replay. При смене panel-version сохранить separate series и bridge comparison, не рисовать непрерывный gain из разных denominators.

## 6. Точные оставшиеся runtime-привязки

| ID | Что отсутствует фактически | Где получить / действие без нового исследования всей программы | Что блокирует |
|---|---|---|---|
| G1 | Accepted checkpoint/model/domain/project/version и actual tokenizer/modelConfig | Existing server-owned selection → StartService/verified loader; сохранить manifest/hash и source trace. Исторический Qwen shortlist не выбирает Student | Реальный generation baseline и update; дизайн A/B можно читать/реализовывать в отдельном code scope |
| G2 | Полный installed adapter key allowlist, frozen base и optimizer/RNG continuation конкретного parent | B: installed objects → GetCheckpointParameterEntries, actual manifest and existing journal; не угадывать rank или keys | Mutation |
| G3 | Действующий material/teacher/judge grant, actual models/request identities, applicable idle/capacity/OP-65 permission | Current S04 server-owned selected packet/profile и текущий авторизованный запуск. Не выводить из права писать GitHub | Teacher/assessor calls и training effects |
| G4 | Конкретные lawful corpus/family IDs, N, effect/noise/retention thresholds, budgets и evaluator profile | R13 repair + действующая server-owned evaluation/noise selection; дать semantic receipt в D. Нет новой автоматической серии сбора | Scientific RUN/promotion; published design не fake filled runtime card |
| G5 | Реализованный ordinary v2 route/service и protected trial evaluation binding | C по точным proposed files §3.4; read-only invocation и cold-load evidence на candidate SHA | Adoption/utility claim, не поиск нового trainer |
| G6 | Actual host/hardware/resource envelope, applicable native evidence, независимый review | Локальный authorised S04 host, R17 property-specific map, R23 envelope, реальный R15/R25/Astra reviewer | Effects, PC/promotion claims по соответствующей зависимости |

Это конечный binding list. Missing source identity не повод перечитывать все R01–R17; missing code bridge решается C, а не выдаётся за внешнюю недоступность. R18 не имитирует закрытие R13 repair или independent review.

## 7. Сохранённая полная программа и semantic receipts

| Источник/решение | Что изменил R18 | Что не считается закрытым / следующий owner |
|---|---|---|
| R05 CP2 exact pair/objective | A убирает lossy material; B задаёт response sum и isolated public scorer | Реальный эффект D, не green fixture |
| R12-ARCH-2 + algorithm CR-D03/E/F | E1 выбран; weighting сохранён; actual trainable keys и ordinary bridge конкретизированы | E2 не подмена neural S04; learned projection отдельно |
| R13 useful E1–E4 + repair override | §5 предметно задаёт task, arms, invariants, measures, negative outcome; G4 честно выделяет actual missing identities | Чужой R13 статус не повышен; D/R22 принимают заполненные runtime values |
| R17 incoming P1 | Current local C: dependency отделена от GitHub native campaign; A/B/C/D имеют разные evidence risks и finite stop | Workflows/required status не изменены; skip-ci не обход required rules |
| Original Unit / Flow / Orchestrator / Graph | Typed associations и actual consumer обязательны; §3 не вводит четыре API-двойника или trainer | Flow: frozen Units + changed corrections; Orchestrator: frozen lower roles + lawful async paths; Graph: unseen higher-order compositions — R22 |
| F/D-04, D-07, D-10, D-15 | Development intents не subjects; stack до normalization; BM25+kNN и arbiter authority сохраняются | Domain mapping/kNN efficacy отдельные E2 contrasts, не свойство VectorSignal |
| F/D-06 historical replay vs current WorkLeft/S04 | Для текущего one-pair lifecycle действует explicit no-repeat; historical replay не запускается | Изменение replay policy только отдельным разрешённым решением, не молча в bulk |
| F/D-13 | Neighborhood/subject diversity не подменяется software coverage | R21/R22 domain breadth и semantic coverage |
| F/D-14 | Internal first, external single-use permission, idle learning, after/before preservation | Ни manual feedback, ни teacher label не отменяют arbiter/authority |
| All S00–S10 / WorkLeft / DTC / IDE | E1 — первая зависимо готовая цепочка, не сужение всей программы | R19 переносит roadmap; R20 compiler; native/remote S05 at actual delivery |
| Семь passports | C# first; EN↔RU, Orchestration, Figma→HTML/CSS, Angular, SQL и DPO Builder сохранены отдельно | R21 domain references; R22 experiments; R23 PC cost; R24 integration; R25 независимый review |

Для E2 использовать уже спроектированный lawful pool → domain/query feature score → fixed-k first comparison → typed multi-neighbor aggregator → solver. Затем отдельное изменение k/radius; HNSW остаётся candidate generator, nonmetric rerank не объявляется index metric. Для LoRA projection определить source/target coordinates и decoder прежде обучения. Для Flow учить associations/corrections при frozen Units; для Orchestrator учитывать все branches/cancels; для Graph — unseen compositions, не memorized path. Правильный deterministic ответ может быть chosen. Эти обязательства остаются в очереди, но не тормозят самостоятельный первый урок.

## 8. Источники научного контракта и граница проверки R18

- DPO, Rafailov et al., версия v3, §4 Eq.7 и appendix B: https://arxiv.org/html/2305.18290v3 . Использована только формула условного preference objective, не перенесено обещание эффективности XIIGEN.
- LoRA, Hu et al., версия v2, §4: https://arxiv.org/html/2106.09685v2 . Low-rank adaptation не является автоматически tokenizer или inverse semantic projection.
- Source-specific nullable example и остальные domain laws — COMPLETION-REPAIR-DOMAINS с первичными ссылками. Для execution сохранять фактическую language/stack version.

R18 выполнил адресное чтение и авторскую самопроверку, не полное повторное исследование R01–R17, не полный аудит каждого большого core файла и не независимую Astra проверку. Перед product patch каждый worker читает полные меняемые файлы и их actual callers. Никакие учебные данные/закрытые ответы/секреты/веса сюда не скопированы. Новые классы, overload и endpoint выше — explicit proposed design, не утверждение о существующей реализации.
