# R18 — первый учебный цикл без круговых допусков и потери архитектуры

**13.10.2026 · редакция 2 · адресное исправление R18 по поручению Лубы.**

Мы сохраняем существующего CodeDocumentation Student, его trainer, попытки и checkpoint. Исправляем путь, по которому смысл одной оценённой пары достигает параметров Student, а результат — обычного пользовательского запроса. Затем на независимых заданиях выясняем, стало ли решение точнее и сколько это стоило. Отрицательный результат допустим; новый Windows run или процент coverage не заменяет этот ответ.

**Сейчас изменён план, не продукт.** Состояние: `EDITORIAL_REPAIR_PUBLISHED_DRAFT / RUNTIME_BINDING_OPEN / LEARNING_NOT_MEASURED`. Независимая проверка нового candidate через CODEX/GPT-6 Astra UltraCode — `NOT_PERFORMED`. Подготовка алгоритма не означает `FIRST_LESSON_EXECUTION_READY`. Эта публикация не выполняет A–D, не разрешает расходы, обучение, core patch, Actions, build или тесты.

[Предыдущая редакция](R18-FIRST-LEARNING-IMPLEMENTATION-WORK-ORDER.v1.md) сохранена байт-в-байт; её blob — `af505592907a43dc2022e22854e2b71f23a7e276`. [Разбор исправлений и источников](R18-REVIEW-REPAIR-2026-10-13.md) отделяет новое чтение от унаследованных результатов. [Исторические carriers R18](results/R18/FINDINGS.md) не переписаны и не повышены в статусе. При конфликте поручений R18 использовать эту редакцию и [действующую карточку](run-prompts/R18.md), а не исполнять обе версии.

## 0. Один эффективный пакет, а не ещё один круг планирования

Текущий editor исправляет только R18 и его вход. Будущий dispatcher сначала определяет режим: **документная подготовка**, **отдельно разрешённая реализация**, **разрешённое наблюдение**, **разрешённый учебный эффект**. Он не передаёт worker одновременно запрет всех core edits из planning COMMON и команду реализовать код. Для перепланирования [COMMON](run-prompts/COMMON.md) включается один раз; для будущего product work его неизменяемые правила сохраняются, а разрешённые эффекты задаются конкретным действующим поручением. GitHub write permission не является training grant.

Готовый packet содержит выбранный A, B, C или D целиком, необходимые §3/§5/§6, current own checkpoint, точные source/skill excerpts, pinned refs, allowed files/actions, решения и конечную передачу. Не посылать только «прочитай весь replanning». Полные меняемые файлы и actual callers/consumers читаются до product diff; исторический excerpt не заменяет такое чтение. Если equivalent bridge уже реализован на новом core ref — reuse и минимальная дельта, не второй сервис.

R01–R17 могут начать параллельно; R18 принимает достаточные частичные результаты и поздние смысловые поправки. Отсутствующий общий DONE не препятствие чтению или подготовке независимого фрагмента. Изменённый blob влияет только на связанные требования/пакеты. Более поздний сторонний RUN-LOG не сбрасывает весь visited set. Финализация конкретного вмешательства всё равно требует его source/state/objective/consumer/experiment/review prerequisites. Более широкие разрешённые dispatcher starts в других документах не превращаются в дополнительные вызовы или training concurrency этого R18.

## 1. Что выбрано, что сохранено

Первый опыт — **E1-CODEDOC-CONDITIONAL-DPO-v2**, существующий XIIGEN CodeDocumentation LlmModel. R12-ARCH-2 условно выбрал этот путь; прежняя надпись R12 REEXECUTION_REQUIRED в чужом историческом абзаце не отменяет прочитанный integrated design. Но R12 design не подтверждает installed runtime. R13 имеет действующий repair override, а independent R15 review отсутствует: их статусы здесь не исправляются самоназначением.

E1 изменяет только допущенную группу project-LoRA параметров Student. Исправления material/objective/generation сначала образуют одну версию кода, общую всем arms; сравнение parent с trial после этого не выдаётся за отдельное измерение эффективности каждого code fix. Teacher, Student, DPO reference, pair judge и post-update evaluator — разные роли. Новый trainer, новая machine, tokenizer rewrite или полный Graph redesign перед первым уроком не нужны.

Исходные роли сохраняются: **Unit** — domain/input-vector/output schema и typed many→many; **Flow** — отношения и коррекции; **Orchestrator** — lawful async combinations/order/budget; **DynamicGraph** — паттерны разных порядков и перенос композиции. Реальные ILogical* сопоставляются по поведению, а не переименовываются из-за опечатки. Train остаётся у ILearningMachine; Graph — существующая L5 specialization, не шестой уровень. E1 не доказывает обучение трёх других ролей, mapper, kNN или semantic projection.

Сохраняются TensorCore/shared base/project LoRA, StackDetector-before-normalization, semantic order/compression, BM25+kNN, positive/negative/warning memory, final arbiter, обязательные skills вне top-k и source-bound ограничения SmartSkills/helper. Правильный deterministic ответ может быть chosen. Randomness не является признаком обучения, а детерминированный forward с обученными весами не опровергает обучение.

## 2. Контекст и проверенная кодовая опора

**K:** `lubochka/llm_mvp_core@3c7f5f8c7809936e25579603c96ea6df8598fc40`; branch head повторно проверен при repair. **P:** `lubochka/xiigen-planning:llm_plan3`; pre-publication pin `0115dd8538657fb74e5fd26dd95ea8a2e2cdd146`. D = `llm mvp/design to code session`; F = `llm mvp/llm dynamic flow`; V = `llm mvp/visual studio & visual studio code continue`. Старые P0/P1 receipts сохраняют свои pins, не переписываются новым ref.

Прямой смысловой маршрут этого repair: D/`original task.md` (четыре роли) → D/`09.09.26WorkLeft-RAG-00-usage-guide.md` → D/`09.09.26WorkLeft-02-teaching-chinese-model.md` (Student/урок/no-repeat/NO_TRAINING_NEEDED) → R10/R11/R12/R13/R15/R17 → конкретные методы K. Original provider shortlist не текущий grant. Прежний R18 дополнительно проследил WorkLeft-INTEGRATION-01 и V/RAG-06→F/DECISIONS-LOCKED: эти findings переиспользуются как attributed, не выдаются за новый полный обход F/V. Все S00–S10, WorkLeft/RAG roots, DTC и IDE остаются в программе.

| K: файл / диапазон нового чтения | Наблюдение, не выполненный опыт | Решение и consumer |
|---|---|---|
| `18.CapabilityTraining/LlmIngredientLogicalUnits.cs`, 2250–EOF, BuildBatch/Compact | `Encode($"{Compact(preference.Prompt)} {Compact(preference.Chosen)}")`; whitespace/480 меняют материал после raw admission | A: immutable exact material; B обязан потреблять response masks |
| Тот же файл, 2100–2225 | Проверяет независимость policy/reference storage и keys; prepared continuation сравнивает состояния до freeze | B/C reuse; actual selected key set ещё привязать |
| `11.DPO/DpoTrainer.cs`, 90–245 | Конструктор без mutation capability допускает чтение/оценку, но не update | Раздельные read-only/mutation admission, не новый обход |
| Тот же файл, 560–720 | VectorSignal масштабирует gradient; score full-sequence mean; policy training:true, reference:false; optimizer фильтрует RequiresGrad | B: response sum и соответствующий backward; сохранить публичный average API; режимы/RNG явно фиксировать |
| `09.Inference/TextGenerator.cs`, весь файл | Encode(prompt), ForwardWithCache, sampler, seed/cancellation, отдельный cache, EOS/length guard | C: тот же kernel, versioned detailed overload; не второй генератор |
| `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentService.cs`, 1–165,175–300,430–565 | AssessAsync уже генерирует; GenerateStudentAnswer передаёт `entry.OwnerInput` | C: общий exact OwnerInput и kernel; evaluator/corpus остаются отдельно |
| `26.TrainableRouter/TrainableRouter.csproj`, весь | Reference на `14.ContinuousLearning` | Нельзя делать обратную ссылку 14→26 ради общего сервиса |
| `14.ContinuousLearning/ContinuousLearning.csproj`, весь | Reference на `09.Inference`, 03,21, но не 26 | Shared generation facade помещается в 14, обе стороны вызывают его вниз |
| `26.TrainableRouter/Training/CodeDocumentationRoutingFlow.cs`, 1–135 | Route/source/version/transit/stages, не сгенерированный текст | Старый контракт не переименовывается в Student answer |
| `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedModelCheckpointStartService.cs`, 1–265 | LoadLatestCompatible; только clean missing + ColdStartPolicy.IsAuthorized допускают явный cold start; ParentManifest=null | C: сохранить обе ветки, не требовать несуществующий accepted parent и не делать silent fallback |
| `03.TransformerModel/LlmModel.cs`, 1–145 | BaseParameters, canonical checkpoint entries/adapter slots | B: actual key binding; имя LoRA не доказательство адаптерного получателя |

Унаследованные code anchors прежнего R18: `14.ContinuousLearning/Training/DpoTrainingExecution.cs:TryBuildLogicalRequest`; `11.DPO/Models/DpoBatch.cs`; `01.Tokenizer/BpeTokenizer.cs`; `10.LoRA/LoraApplicator.cs`; `26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.cs:ResumeQualityDecisionFromExistingAttemptAsync/ReopenTrialCheckpointFromAttempt`; `25.ModelServer/Program.cs:ConfigureLocalLoopbackRoutesCore` и protected bootstrap 80–91. Точные прежние ranges/blobs — [SOURCES.csv](results/R18/SOURCES.csv). Before patch проверить full affected files/current callers; этой таблицей полный аудит не заявлен.

## 3. Решённый алгоритм

### 3.1. Один вход и faithful material

**Выбранный framing первого узкого API — `owner-input-verbatim-v1`: x есть неизменный OwnerInput, без автоматического шаблона, пробела или сериализации после judgment.** Это новое явное соглашение поверх реально найденного assessment input, а не утверждение о готовом ordinary endpoint. Source/domain/project/stack/output schema и association IDs связываются с x до authoring. Модель видит только x; существенное доменное условие, нужное модели, должно уже содержаться в x. Metadata не считается скрытым conditioning.

Предлагаемый `CodeDocumentationGenerationInput` содержит RequestId, exact OwnerInput, RepresentationId и server-validated source/domain context. Client не назначает себе права, selected model или truth labels. Existing CodeDocumentationRequest и RoutingFlow остаются прежним routing API. Его подключение к новому consumer требует versioned adapter с явным соответствием authoritative source/context→x; два независимо меняемых CodeText/OwnerInput не допускаются. При несоответствии — `INPUT_CONTRACT_MISMATCH`, не guessed rendering. Первый публичный путь принимает обычный пользовательский exact input через разрешённый local scope, а не закрытый corpus entry. Расширение structured rendering не скрывается в этом первом эксперименте.

Teacher пишет обе final стороны y+ и y− для одного x. CODEX с фактически разрешённой Astra UltraCode invocation выбирает case и отдельно судит точные стороны/context. Weak Student output — допустимый weakness context, не обязательный rejected. Preferred side и pair_usable — разные verdicts; TIE/BOTH_BAD/UNJUDGEABLE не forced preference. Judge не редактирует ответы под старым Chinese authorship. Изменённые bytes/context требуют нового assessment и действующих прав.

Material version сохраняется **`segmented-byte-BPE-v2`**: p=Encode(x), a=Encode(y), ids=concat(p,a). Не предполагать, что Encode(x+y) имеет тот же prefix. Не добавлять пробел, newline, instruction или EOS после оценки. RealTokenMask отмечает реальные позиции, ResponseLossMask — targets j, где prefixLength≤j<realLength. Logits[j−1] предсказывают ids[j]; первый response token использует последний prompt logit. Padding и prompt не прямые response targets, но prompt остаётся conditioning и может участвовать в градиентной цепочке.

До effects: strict UTF-8 roundtrip каждого сегмента; непустые x/y; valid и точно представимые tensor token IDs; tokenizer/schema/context/V2 capacity; один новый урок S04; смысловой контраст сохранён. Правое padding не меняет tokens. Подготовленные arrays detached от изменяемого caller, версии/lengths/masks/digest связываются с raw pair/context/framing/tokenizer. Digest не выдаёт прав на обучение. **Overflow — отказ до mutation, не увеличение 480, обрезка или скрытый teacher retry.** Более короткий полноценный урок — отдельное разрешённое authoring/assessment, не новый UUID старой пары.

Различать: semantic TIE; `MATERIAL_CONTRAST_LOST` из-за подготовки; допустимую пару с малым/нулевым initial margin. Последний не отменяет preference. Исчезнувший whitespace внутри literal — смысловая порча, не normalization. Реальные error handlers перечисляет A по caller; refusal не скрывает уже произошедший effect.

### 3.2. DPO objective и конкретные параметры

Для policy и frozen reference вычислять response-only **sum** ell(y|x), с одинаковыми masks/framing и согласованным backward:

```text
m = beta * ((ell_policy(y+|x) - ell_reference(y+|x))
          - (ell_policy(y-|x) - ell_reference(y-|x)))
base_loss = softplus(-m)
a = frozen_existing_VectorSignal(teaching_vector)  # single pair, normalized weight=1
optimized_loss = a * base_loss
grad_m = a * (sigmoid(m) - 1)
chosen_upstream = beta * grad_m
rejected_upstream = -beta * grad_m
```

Стабильные softplus/log-softmax; forward/backward не делят response contribution на count. `EvaluateAverageNextTokenLogProbability` остаётся average API с прежними callers. Не менять общий ComputeLogProbs глобально. Mutating v2 требует explicit objective/material version и response masks; unsupported old caller отклоняется до effects, не all-ones fallback. Producer/consumer migration адресная, не переписывание всех доменов.

На K наблюдаются beta=0.1, DefaultLearningRate=1e−5; это не новые experimental thresholds. Сохранить actual optimizer settings, VectorSignal и режимы первого вмешательства. Raw DPO loss, optimized loss, multiplier, response counts, margin, active-key digest и objective version сообщать отдельно. Old mean и new sum несопоставимы как learning gain.

**Получатель:** actual repository-owned CodeDocumentation policy, разрешённые A/B установленных project adapters. Keys брать от installed objects/`GetCheckpointParameterEntries`, сверить manifest, RequiresGrad и optimizer membership; base/ref frozen, policy/reference storage независимы. Нельзя объявлять adapter-only по имени класса, менять rank/LR под outcome или устанавливать второй trainer/freeze system. Warm/prepared continuation сохраняет exact optimizer/RNG state, не скрытый reset.

При численно одинаковых policy/reference forward distributions m=0 ожидаемо. Однако current policy вызывается с training:true, reference:false: одинаковые weights сами по себе не доказывают одинаковый stochastic forward. B фиксирует режимы/RNG и их фактическое влияние; seed label не proof. Не менять dropout/optimizer одновременно с выбранным фактором без новой версии контракта. Zero gradient/identical weights допускают честный результат, не фиктивную мутацию. TeachingVector здесь multiplier, LoRA — параметризация адаптации; semantic `P_domain(source→target)` остаётся отдельным договором, не свойством shape.

### 3.3. Состояние, cold start и принятие

```text
existing durable attempt/journal
  → already applied / unknown: recovery + evaluation, НИКАКОГО второго update
  → new lesson: определить допустимый исходный state
      accepted parent найден → verified loader + pinned manifest
      clean missing + applicable explicit cold-start authority
          → existing CreateColdStartModel + сохранённый baseline root
      иначе → отказ; corrupt/incompatible/ambiguous/unavailable не cold start
  → read-only baseline и frozen experiment controls
  → granted new exact pair → immutable material → один isolated update
  → existing trial persistence/reopen → independent examination
  → qualified accept либо keep-parent/keep-baseline
  → cold ordinary invocation выбранного состояния, отдельно от verdict
```

Cold-start ветка **условная**, не новое разрешение: проверить текущий S04 packet/ColdStartPolicy и scope. `ParentManifest=null` остаётся правдой; fresh baseline не relabel accepted checkpoint. До update baseline обязан иметь воспроизводимые state/identity/snapshot и допустимый isolated evaluator binding. Если текущий assessment passport принимает только accepted/trial, C добавляет узкое внутреннее baseline binding с явным source kind; не подделывает accepted manifest. При отсутствии authority остаётся `BASELINE_AUTHORIZATION_OPEN`, а не вечное «создайте accepted checkpoint до первого обучения».

Снимок: weights/base/adapters, optimizer moments/step, scheduler если применим, RNG/dropout/counters, topology/tokenizer/framing/objective/material/schema, parent/attempt association. Frozen kNN/index/graph/cache identities также фиксируются; их фактическое изменение invalidates E1 contrast. Выбранный serving instance не изменяется trial optimizer. Unknown update сначала reconcile; rollback pointer не стирает consumed-pair journal. При competing promotion сравнить accepted identity, не затирать новую ревизию и не повторять урок. Failed evaluator call не означает failed update.

### 3.4. Общая генерация без циклической зависимости проектов

**Все новые названия ниже — proposed design, не найденный implementation.**

- `14.ContinuousLearning/Training/CodeDocumentationStudentGeneration.cs`: общий read-only facade и versioned input/result carriers. Получает уже загруженную модель, tokenizer, exact x, server-owned identity, generation settings и cancellation. Не выбирает checkpoint, не знает evaluator references и не тренирует.
- `26.TrainableRouter/Training/CodeDocumentationGenerationService.cs`: ordinary wrapper. Проверяет source/domain/input contract, получает server-selected accepted handle через существующий owner/loader и вызывает facade в 14.
- `25.ModelServer/ContinuousLearning/CodeDocumentationGenerationEndpoint.cs`: local adapter, proposed `POST /api/code-documentation/generate`; existing Program связывает его с wrapper.
- `09.Inference/TextGenerator.cs`: ограниченный detailed overload над тем же prefill/decode/sampler, сохраняющий legacy overloads. Assessment в 14 вызывает общий facade напрямую, **не сервис 26**.

```text
ModelServer25 → ordinary wrapper26 → shared generation14 → TextGenerator09
                                  ↗
assessment14 + private trial/baseline binding
```

Действующая зависимость 26→14 сохраняется; **14→26 запрещена**. Нельзя вводить второй generator ради устранения цикла. Facade не содержит closed reference answers, teacher authoring или optimizer authority. Ordinary client не передаёт weight path/trial ID/repository root. Trial/baseline handle доступен только trusted internal evaluation path. Existing RoutingFlow.ExecuteAsync остаётся route/stages, не подставной documentation result.

Output: RequestId и сохранённые input/output association IDs, generated text/token information, фактические model/tokenizer/framing/generation versions, stop reason, source/law warnings и timings. Many→many не positional Zip: reordered/partial/cancelled/fan-in results сохраняют identity. При используемых OutputRefs нужен настоящий typed resolver со scope/version/lifetime, а не copied refs или IQueryable поверх candidate IDs. Это не требует Graph adapter для неиспользующего его E1.

Унаследованная EOS проблема сохраняется: последний vocabulary ID сам по себе не доказанный special token. Detailed v2 использует stop ID только из проверенного manifest; иначе fixed maxNewTokens и явный LENGTH_LIMIT/незавершённость. Не расширять vocabulary, не дописывать EOS в урок и не выдавать truncated answer за полный. Все arms используют один и тот же контракт, включая completion rubric. Cache не переиспользуется между model/framing identities. Error/abstain возвращаются типизированно, не fabricated text.

До promotion изучается isolated trial через тот же facade/input contract. После verdict новый ordinary process действительно генерирует выбранной accepted/retained identity. Hash или assessment-only generation не закрывают adoption. Если accepted state отсутствует и baseline не разрешён для serving, ordinary путь честно сообщает отсутствие готового состояния; experimental cold baseline не становится публичной production model автоматически.

## 4. Четыре ограниченных задания для модели

### Общая форма передачи A–D

Каждому worker dispatcher встраивает: CURRENT TASK/NOT THE TASK, actual mode/grant, assigned source/code excerpts с repo/ref/blob/span, loaded skill proof, current→target algorithm, allowed/forbidden files/actions, examples/edge, documentary acceptance и будущую scientific examination раздельно. Команды только обнаруженные в actual authorized packet; не придумывать CLI/options. Этот документ не разрешает `dotnet test`, training, Actions, paid calls, install/cleanup или обход configured required checks. После отдельного core поручения допустимы только перечисленные effects; минимальная сборка требует соответствующего разрешения и не является learning evidence.

Все дети return-only: diff/proposal + exact refs/read spans + algorithm/consumer trace + preserved contracts + evidence kind + unresolved item/blocked effect + next recipient. Дополнительно count_claimed=actual calls, subagent_id=actual или NONE при отсутствии вызова, independent_evidence_count=actual; никаких выдуманных execution IDs. Один bounded correction по конкретному замечанию, затем содержательный checkpoint и named остаток, не loop-until-green. Parent интегрирует пересекающиеся файлы последовательно и проверяет свежий blob. A и начальная C могут готовиться параллельно после фиксации input contract; B потребляет A; D готовит панели параллельно, mutation идёт только после собственных prerequisites.

### R18-A — assessed pair → immutable tokens/masks

**Промпт.** В существующем producer реализуй §3.1: optimizer должен получить именно содержательное различие judged pair, с тем же x и без Compact. Начни с active caller и всех mutating consumers; закончи реально потребляемым материалом, не DTO без B. В текущем planning mode верни точный diff design; product patch только по отдельному mode/grant.

**Контекст/skills/RAG.** `trainable-architecture-decision-SKILL.md`, `code-examination-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `output-readability-gate-SKILL.md` в K/docs/skills. WorkLeft-RAG-00→WorkLeft-02, original role/input clause, R12-C01/C02, R05 CP2 и поздний FS-D05; читать относящиеся spans. Код: DpoTrainingExecution.TryBuildLogicalRequest→LogicalDpoTrainingUnit.PrepareExecution/BuildBatch→DpoBatch. Full changed-file/caller read до diff, не вся библиотека.

**Allowed future files:** `14.ContinuousLearning/Training/DpoTrainingExecution.cs`, `18.CapabilityTraining/LlmIngredientLogicalUnits.cs`, `11.DPO/Models/DpoBatch.cs`; узкий internal immutable carrier в этих проектах. BpeTokenizer читать, vocabulary/codebook/weights не менять. Shared input contract §3.4 согласовать с C, не параллельно редактировать один файл.

**Действия.** Зафиксируй x/framing/context до authoring; удали Compact из selected train path; segmented encode, byte roundtrip, limits/range/contrast; separate masks и j−1 target mapping; detached copies; material version/digest и точный отказ до effect. Выясни actual handler unsupported legacy caller, пустых sides и caller mutation после await. Не подменяй empty response токеном [0].

**Хорошо:** отличие после 480-й позиции сохранено либо целый урок отказан по capacity; пробелы source literal не исчезли. **Плохо:** поднять 480 до 4000, silently trim после judge или считать raw hash доказательством faithful tokens. **Края:** UTF-8/combining characters, CRLF/literals, BPE segment boundary, identical token sides, stale tokenizer, fractional/out-of-range IDs, exact capacity, empty x/y, alias mutable arrays.

**Проверка/выход.** Documentary source→material→consumer walkthrough с несколькими явно обозначенными DOCUMENT_EXAMPLE, ручная сверка bytes/IDs/lengths/masks и эффекта refusal; не suite/coverage. Future permitted diagnostic может наблюдать этот же путь без optimizer/teacher. Return: proposed/actual patch, version, caller compatibility, material evidence, ограничения. Completion: B принимает именно эти masks/context, C/D тот же framing. Никакого объявления learned gain. Один correction cycle, затем точный unresolved caller/span и его blocked effect.

### R18-B — response objective → разрешённые Student keys

**Промпт.** Реализуй отдельную mutating-v2 response sum и соответствующий backward в существующем DpoTrainer по §3.2. Свяжи обновление с actual installed project-LoRA Student; не routing weights, не новый trainer и не global replacement average scorer. A уже определил материал; не выбирай потерянный loss contract заново.

**Контекст/skills/RAG.** A exact output; R12-C02/E01/F01, R15R-04, R17 optimizer/Student rows; `trainable-architecture-decision`, `code-examination`, `plan-review`, `prompt-patch-authoring`, `output-readability-gate` (полные имена с `-SKILL.md`). WorkLeft integration existing Student→LogicalDpoTrainingUnit→StepVector; LoRA installed objects→GetCheckpointParameterEntries. DPO §4 Eq.7 и LoRA §4 как mathematical primary, не обещание efficacy.

**Allowed future files:** `11.DPO/DpoTrainer.cs`, найденный actual metrics carrier, selected sections `18.CapabilityTraining/LlmIngredientLogicalUnits.cs`; DpoBatch только через A join. LoraApplicator/Linear сначала read/reuse; менять только доказанный missing seam с явным дополнительным file grant. Нельзя менять rank, beta, LR, tokenizer, dataset или public inference API под исход опыта.

**Действия.** Привяжи new masks/version; stable response logsum и backward без count division; raw/optimized metrics; frozen multiplier; actual key allowlist и independent reference. Прочитай training-mode/RNG path, не полагайся на одинаковые weights как гарантию одинакового stochastic score. Выдели allowed continuation несовместимости objective version; не обнуляй optimizer незаметно.

**Хорошо:** первая response позиция получает правильный upstream, prompt не target, но conditioning остаётся; legacy average caller неизменён. **Плохо:** forward sum/backward mean; назвать ненулевой checkpoint hash learning; forced rejection m=0. **Края:** unequal response length, one token, all-zero mask, NaN/extreme logits, policy/reference alias, extra base RequiresGrad, пустые adapter keys, warm-start flags, identical weights, old objective optimizer state.

**Проверка/выход.** Аналитическая проверка знаков/масштаба derivative, caller trace и exact trainable/frozen manifest. Future authorized diagnostic — ограниченное численное наблюдение derivative/storage без optimizer; не повтор урока и не coverage campaign. Return: patch/design, objective/math/mode contract, backward correspondence, actual/OPEN key receipt, preserved API, state compatibility и exact candidate. Передача C/D; missing actual keys блокирует mutation, а не вычисление проекта patch. Один bounded correction, без нового freeze framework.

### R18-C — допустимый baseline → обычная генерация выбранного state

**Промпт.** Свяжи existing checkpoint lifecycle с ordinary CodeDocumentation input по §3.3–3.4. Сохрани authorised cold-start branch и честные identities. Построй общий facade в 14 поверх TextGenerator09, wrapper в 26 и local adapter в 25, без обратной ссылки 14→26. Assessment и ordinary request должны получать одинаковый x/generation contract, но не общие закрытые ответы или полномочия.

**Контекст/skills/RAG.** A framing; B objective/state; R12-F; WorkLeft-RAG-00→WorkLeft-02/INTEGRATION-01; R17 state/native risk. `trainable-architecture-decision`, `code-examination`, `phase-planning`, `plan-review`, `human-readable-architecture-audit`, `output-readability-gate`. Code chain: StartService/loader + coordinator→assessment→TextGenerator; routing Flow и обе csproj dependencies обязательно прочитать. Parent/attempt source kinds не выводить из folder names.

**Allowed future files:** proposed §3.4 files, `09.Inference/TextGenerator.cs`, `14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentService.cs`, scoped ModelServer Program route registration. Existing StartService/coordinator/loader — reuse; только необходимое version/baseline/adoption binding с exact reason и file grant. Не менять protected `C:\Projects\llm_mvp_core`, existing Generate/ExecuteAsync semantics или custody/recovery целиком.

**Действия.** Verified accepted selection или authorised clean baseline; immutable server handle с validated bytes/version; source admission exact OwnerInput; shared facade; trusted isolated trial/baseline only internally; identity-safe cache/results; comparison-before-promotion; cold ordinary invocation после решения. Protected startup остаётся host constraint, не повод universal CI run. Reordered/empty/cancelled results не ломают association. Ordinary source scope не становится permissive только потому, что новый API read-only.

**Хорошо:** accepted parent продолжает обслуживать запросы во время isolated trial; после отказа сохраняется; после accept cold process генерирует actual selected model. **Плохо:** вернуть stages или run_id вместо текста; клиент выбирает trial path; assessment14 импортирует26; corrupt checkpoint заменён random model; accepted passport подделан для cold start. **Края:** clean missing без grant, cold-start prohibited конкретным S04, unavailable/incompatible bytes, acceptance race, stale tokenizer/cache, LENGTH_LIMIT, wrong-domain adapter same shape, changed source meaning, cancelled fan-in, unknown previous update.

**Проверка/выход.** Document walkthrough обеих start branches и dependency DAG, request→source→loader→facade→kernel→typed result. Future read-only observation при §6 допуске проверяет загрузку и обычный parent answer **до первого update**; post-verdict cold adoption отдельно. Return: minimal patch/design, input/API schema, manifest/source-kind mapping, dependency/caller map, native boundary, G5-pre/G5-post receipts либо gaps. Полная cold adoption не prerequisite собственного первого вызова. Запуск API до его реализации не выдавать готовым curl/CLI.

### R18-D — причинный экзамен, отрицательный результат и следующий bulk

**Промпт.** Скомпилируй §5 в frozen runtime card и исполни только разрешённую стадию §6. Не требуй все G1–G6 перед любой моделью: preconditions должны предшествовать своему effect, observations появляются после него. Exact request IDs заранее неизвестны; actual grant/model/profile должны быть известны. Не превращай общий R13 repair в повтор всех исследований или в пустую ссылку вместо concrete experiment.

**Контекст/skills/RAG.** A/B/C candidate и receipts; current R13 repair override/experiment compiler; R17 matrix; WorkLeft S04/S06/S07; семь baseline passports и only applicable nullable laws. `trainable-architecture-decision`, `phase-planning`, `plan-review`, `output-readability-gate`; дополнительные measurement skills сначала найти и действительно прочитать. Не заявлять invocation Astra по надписи в файле.

**Allowed future work:** existing assessment/quality pipeline instrumentation и server-owned configuration/experiment artifacts в явно разрешённом scope. Не публиковать exam answers, raw user data, pair/weights или делать teacher calls по documentary grant. Не создавать test suite/benchmark framework/Actions dispatch. Existing required native/state safeguards сохраняются property-specific.

**Действия.** Определи исходный accepted либо разрешённый cold baseline; отдели operational selection от untouched final. Frozen task-family rubric/controls/budget до candidate outcomes; valid permit→actual teacher response→separate exact judge→prepared material→one mutation→isolated comparison→decision→ordinary adoption. Запиши observed IDs после действия, а unknown outcome передай existing recovery. Сравни качество/retention/время/полную цену, не только loss. Новый feedback проходит права, смысл, novelty/conflicts и новую pair admission.

**Хорошо:** валидная pair применена один раз, но independent families не стали лучше — INCONCLUSIVE/REGRESS по frozen rule, parent сохранён. Достаточный baseline даёт NO_TRAINING_NEEDED с нулём teacher/pair/update. **Плохо:** выбирать удачные cases post-hoc, повторять pair до выигрыша, называть собственную оценку независимой Astra, ждать будущий response ID до вызова. **Края:** TIE/BOTH_BAD, исчерпанный budget, no lawful source, evaluator disagreement, low margin, no-update control gain, незавершённый output, missing ground truth, budget breach, unknown mutation, actual acceptance race.

**Проверка/выход.** Filled card и exact pending fields, actual observations отдельно, one-pair lifecycle, raw protected evidence refs, aggregate by family/domain, cost/uncertainty, qualified verdict, promotion и adoption как разные fields. Без запуска все метрики NOT_MEASURED. Передать R19/R20 очередь и compiler bindings, R22 последующие E2/E3/E4, R23 PC/cost envelope, R25 exact candidate review. Ни A–D, ни этот список не дополнительные R26–R29 или выполненные model calls.

## 5. Конкретный первый эксперимент и расширение по bulk

**Предмет:** source-grounded объяснение C# nullable/control-flow в реально поддерживаемой language version. **Гипотеза:** один новый допустимый урок улучшает conditional Student ответы на других source families без недопустимых law errors, забывания и стоимости. **Falsifier:** эффект только на уроке; нет различимого выигрыша; растут ошибки/затраты; oracle-domain выигрыш исчезает на обычном mapping. One-pair отсутствие gain не доказывает невозможность всех гибридных алгоритмов, но это законченный информативный исход данного вмешательства.

**DOCUMENT_EXAMPLE, NOT_APPROVED_FOR_TRAINING:** `int? Size(string? s) => s?.Length;`: «при null возвращается null» лучше «при null возвращается 0». `s?.Length ?? 0` — другой x, не rejected side первой пары. Две корректные формулировки могут быть TIE. Реальные final sides пишет permitted Chinese teacher на lawful отдельном источнике, не копирует этот документ или closed exam. Domain law сверяется с исходным кодом и versioned primary, не по уверенности judge.

**Arms.** P — exact admissible исходный state, accepted или явно разрешённый initial baseline, уже исполняемый общим v2 кодом. C0 — отдельный read-only экземпляр того же P без optimizer с одинаковой доступной memory/retrieval/cache policy. T — ровно один admitted update от P, после persistence/reopen. Same source semantics, serializer/framing, tokenizer, output contract, modes/settings, rubric/panel и cost envelope. Не менять mapper/k/radius/codebook одновременно с весами. Сравнение старого и нового кода — отдельная mechanism диагностика, не causal proof одного DPO шага.

**Разделение источников.** Training, operational development и held-out families разделяются по происхождению/шаблону, не random row или rename. Для pre/post брать одинаковую frozen measurement panel; её повторное использование для выбора следующего урока означает operational, не untouched final. Отдельный финальный panel не используется для teaching/tuning и не выдаёт уроки после каждого bulk. При смене panel/rubric отдельная серия и bridge comparison с обеими states, не склеенный gain.

**Strata первого сравнения:** nullable reference access; nullable value returns/chains; coalescing counterfactual inputs; conditional-access parentheses/getter-exception edge classes; retention на уже доступных CodeDocumentation families. Это не исчерпывающий C# syllabus. Конкретные lawful corpus/family IDs, N, язык/версия, family weights, evaluator, effect/noise/retention rules и budgets берутся из действующего approved profile и фиксируются до T outcomes. При отсутствии selection D формирует decision-ready profile proposal по данным baseline/noise и действующему owner process; не притворяется, что значения уже найдены, и не оставляет их без owner в «future gate». Нерешённая policy блокирует dependent scientific mutation/decision, не чтение и разработку A/C.

**Измерения.** По независимым families: correct essential claims, missing claims, invented/wrong-source claims, critical law violations, abstention/completion; per-domain и worst-domain отдельно от среднего. Domain mapping: oracle versus predicted domain, mixed/unknown confusion/abstention и downstream quality; mapper в E1 frozen, solver gain не mapper learning. Uncertainty — paired family-level сравнение по заранее выбранному noise profile; одинаковые source paraphrases не independent N. Judge receives blinded/counterbalanced answers по frozen protocol; source-law adjudication и disagreement budget отделены от teacher. Actual отдельная Astra экспертиза проверяет механизм/направление и evidence, не заменяет raw measurements или ground truth. Requested model name без actual invocation не независимость.

**Время и PC.** Wall time от разрешённого input до qualified outcome; отдельно authoring/judgment/preparation/update/save/reopen/generation/evaluation/promotion/adoption. Latency distribution с N, без надёжного p95-SLA из нескольких наблюдений. RAM/VRAM high-water всего одновременно живого process tree; base/policy/reference/assessment copies, adapters, optimizer/gradients/activations/logits/KV, encoder/tokenizer, index/graph/cache, snapshots/retained disk, reindex и failed/cancelled branches. Несинхронные component peaks не складывать в measured total. Teacher/judge/evaluator location/calls/tokens/cost учитываются даже при external compute. Предварительный resource admission и оценка размера идут до действия; фактический peak — после действия. Недостаток memory останавливает опыт, не требует сперва успешно обучиться для доказательства допуска.

**Исходы.** INVALID_EVIDENCE: потерянный contrast/lineage, leakage, mismatched arms, wrong recipient, неизвестные существенные measurement conditions. REGRESS: валидно измеренное ухудшение. INCONCLUSIVE: валидный опыт не различает эффект и шум. IMPROVE: meaningful effect по frozen rule, соблюдены law/retention/resource/admission. NOT_MEASURED: не выполнялось. NO_TRAINING_NEEDED — отдельная ветка достаточного baseline, не произошедшее обучение. Promotion только после допустимого verdict и действующей authority; keep-parent/keep-baseline при отрицательном результате. Повторные экзамены возможны лишь в заранее допущенном budget и без нового update; не выбирать лучший повтор.

**Bulk** — окно последовательных новых уроков в существующих S06.08B/S06.09C/S07, не replay mini-batch. По окнам attempted/admitted/applied/accepted, time, family accuracy/uncertainty/retention/worst domain, state growth. Старый pair_id или та же пара с новым UUID не новый урок. User feedback проверяется по source rights/meaning/novelty/conflicts; пользовательская поправка не автоматически truth. Compactness доказывается сравнением quality-at-budget/quality–time–memory, а не количеством файлов/параметров одного adapter. Best observed path не global optimum.

## 6. G1–G6 без взаимной блокировки

Сохраняются ID, но разделены **предварительное условие** и **наблюдение после действия**. Никаких автоматических разрешений эта матрица не выдаёт.

| Группа | Что известно до соответствующего действия | Что появляется после действия / какой claim ограничен |
|---|---|---|
| G1 — baseline | Exact domain/project/model/config/tokenizer, selected accepted identity; либо clean missing и applicable explicit cold-start authority. Источник: existing server selection/StartService | Verified loaded baseline/state receipt. Accepted manifest не выдумывается для initial root. Без G1 нельзя real baseline/update; A/B code design продолжаются |
| G2 — recipient/state | Actual installed key allowlist, frozen base/ref, independent storage, optimizer/RNG continuation, durable no-repeat/recovery; loader inspection возможен в read-only stage | Actual one-update/weights/optimizer/journal evidence после разрешённой мутации; не prerequisite этой же мутации |
| G3 — effects | Expected actual model/profile identities, material/source rights, grant/expiry, budget, applicable S04 idle/capacity/OP-65 authority до соответствующего call | Observed teacher/assessor request IDs и exact returned bytes после вызова; exact judgment/material admission до optimizer. Future IDs не вход authoring |
| G4 — examination | Lawful family partitions, rubric/meaningful-effect/noise/retention rules, N, evaluator and repeat budget зарегистрированы до candidate outcome; отсутствующие решения имеют D/R13 owner | Scores/uncertainty/qualified outcome после измерения. No numeric profile значит dependent RUN/decision не READY, не запрет read-only profile inspection |
| G5 — consumer | Реализованный shared facade/input contract/isolated trial binding и admissible read-only invocation path; verified parent ordinary behavior до update | G5-post: cold accepted/retained generation после verdict. Её нельзя требовать перед G5-pre; no adoption claim без фактического ordinary output |
| G6 — environment/review | Permitted actual host, conservative resource envelope, applicable property-specific native evidence, exact-candidate independent review в той стадии, где его требует current policy | Measured whole peak/cost и post-state adoption после действий; независимый review receipt только от реального reviewer. Global release proof не prerequisite математической подготовки |

**Порядок разрешённых стадий:**

1. **PLAN / CODE AUTHORING:** reads, source reconciliation и assigned planning publication сейчас. Отдельный core grant позволяет A/B/C patch; ни actual teacher request IDs, ни post-update gain не условие написания кода.
2. **READ-ONLY INSPECTION:** отдельный local read/inference grant + applicable source/privacy/native controls + preliminary resource admission. Получить loaded baseline/keys/profile facts. Никакого optimizer, teacher call или production promotion.
3. **BASELINE / BRIDGE OBSERVATION:** implemented C path, G1, applicable G3 read/evaluation authority, G4 выбранной панели и G6 scope. Измерить parent/allowed initial baseline и ordinary contract. Post-trial adoption ещё не существует и не требуется. При запрете cold baseline остановить только эту ветку.
4. **AUTHORING / PAIR ASSESSMENT:** pre-call G3 и bounded budget; затем actual provenance, separate exact-pair verdict, A material. No forced pair при NO_TRAINING_NEEDED/TIE/no lawful source.
5. **ONE MUTATION:** G1/G2 + immutable admitted material + G3 mutation grant + frozen G4 + implemented G5-pre isolated/ordinary path + relevant G6 review/resource/native safeguards. Не требовать будущую T metric или G5-post.
6. **ISOLATED EXAMINATION:** actual persisted T and baseline, matched G4 controls и evaluation grant; verdict до public selection. UNKNOWN mutation сначала existing recovery, не новая пара.
7. **PROMOTION / COLD ADOPTION:** qualified result+policy+accepted-identity compare → existing accept/retain → отдельный ordinary cold observation. Результат adoption может быть OPEN/failed при completed exam, не fake full success.
8. **REMOTE / NATIVE RELEASE:** S05/delivery права, custody и exact applicable required checks отдельно. Не отключать workflows/statuses; новый GitHub Windows run не default prerequisite каждого урока. Local C: bootstrap сохраняется.

## 7. Что E1 не закрывает: содержательная передача широкой программы

| Механизм / owner | Конкретное последующее вмешательство | Что заморожено и чем опровергается успех |
|---|---|---|
| Domain mapping / R21–R22 | Domain tuple отдельно от intent/project/stack; lawful routing с unknown/mixed; signal supervised/preference по реальным labels | Frozen solver; oracle/predicted downstream comparison, confusion/calibration/abstain. Solver gain не mapping gain |
| Domain tokens/vectors / R21–R22 | Versioned law-sensitive IR/codebook/encoder; meaning-preserving transformations и actual consumers | Frozen relevant solver/retrieval; near-miss laws и out-of-family end-task. Shape/hash/byte count недостаточны |
| Dynamic kNN / R22 | Deterministic eligible pool → BM25/ANN candidates → domain/query learned rerank → typed multi-neighbor aggregation → solver → arbiter. Сначала fixed-k rank/consumer intervention, затем отдельно k/radius budget policy | Same corpus/context/compute; exact-search diagnostic vs ANN. HNSW не произвольная nonmetric function; k>1 с first-hit output не fusion. Wrong-domain hit/unchanged consumer опровергает заявленный механизм |
| LoRA/result projection / R22 | Actual low-rank recipient отдельно от P_domain source coordinates→target tokens/vector; named labels/objective/decoder/ordinary consumer и version/reindex/rollback | Frozen unrelated base/policies; semantic decoding/downstream quality + full cost. Small adapter или одинаковый dimension не доказывают projection |
| Unit many→many / R22 | Typed input/output schema, associations, requested output type, actual output resolver | Reordered/partial/fan-in identity и lawful end-task, не candidate-count. E1 only selected Student lesson |
| Flow / R22, E3 | Учить relation/correction state с конкретным credit recipient; не менять все Units вместе | Frozen Units; held-out compositions, misassociated correction и no ordinary adoption — falsifiers |
| Orchestrator / R22, E3 | Learned lawful path/branch budget среди допустимых combinations с deterministic dependency order | Frozen lower roles; feasible deterministic/no-update baselines, success/time/стоимость всех branches/cancels. «Лучший из найденных» не global best |
| DynamicGraph / R22, E4 | Higher-order pattern state, learning signal/update owner, saved policy→preload→ordinary use | Frozen lower policies; unseen compositions/transfer/retention. Current declared graph без activated training не trained graph |
| Hybrid efficiency / R22–R23 | Matched-budget deterministic-only, learned-only и hybrid comparisons там, где доступны lawful comparable arms; missing arm явно ограничивает claim | Same family/law/quality envelope, все ресурсы; replay запрещённого урока не ablation. Interaction effect не выводится из суммы isolated gains |
| Continuous feedback / R22–R23 | New lawful lessons, conflicts/novelty/authority, bounded memory/index/graph growth и full rollback | Stable operational anchors + untouched final, old abilities/PC growth; best bulk selection или скрытый replay invalid |

Семь baseline subjects сохранены: EN↔RU; orchestration; Figma→HTML/CSS; C#; Angular; SQL documentation; DPO Builder для остальных. Например, law-sensitive distinctions — перевод с отрицанием/согласованием; order/cancellation; layout constraints; nullable behavior; framework lifecycle; SQL NULL/join/duplicates; same-input valid preferences у Builder. Это направления examination, не уже verified data. Дополнительные исходные languages/intents/домены, DTC и IDE не удаляются.

R19 принимает dependency queue всех S00–S10/extras и three delivery lines; R20 компилирует effective packets с mode-specific permissions; R21 источники/паспорта; R22 эксперименты выше; R23 PC/retention budget; R24 интеграционные consumers; R25 independent exact-candidate closure. Named owner/trigger сохраняет deferred work, не объявляет её DONE. 25 assignments остаются 10+5+5+5; A–D не четыре добавленных research rounds. Extra call допускается только по конкретному незакрытому вопросу с полноценным packet и отдельным реальным счётом.

## 8. Приёмка R18 и научные опоры

Документная приёмка: воспроизвести positive, negative и edge walkthroughs A–D; проверить stage DAG на отсутствие cycle; различить current code/proposed methods; пройти source→requirement→selected solution→consumer→falsifier; убедиться, что neither blanket coverage nor vague future gate заменяет решение. Exact current model/profile/key/permit/panel numbers остаются фактическими binding fields §6, не выдумываются. Факты в них ищутся адресно, unresolved policy оформляется на owner; не новый полный обход R01–R17.

Независимый reviewer получает exact final candidate/diff, [repair findings и review packet](R18-REVIEW-REPAIR-2026-10-13.md), original roles, relevant R12/R13/R15/R17 и прямые K methods. Он не пишет собственный candidate, не наследует старый PASS и может вернуть NEEDS_FIX. Авторская проверка этой редакции не отдельный Astra вызов.

Математические references: DPO (Rafailov et al.), §4 Eq.7, https://arxiv.org/html/2305.18290v3 ; LoRA (Hu et al.), §4, https://arxiv.org/html/2106.09685v2 . Они обосновывают objective/параметризацию, не эффективность XIIGEN. Доменные законы берутся из versioned primary routes [domain passports](COMPLETION-REPAIR-DOMAINS-2026-10-13.md). Actual measurements, full-code audit, core implementation и independent review этой публикацией не выполнены.
