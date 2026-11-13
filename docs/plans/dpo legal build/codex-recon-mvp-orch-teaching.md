# Рекон: Обучение оркестратора (DPO/teaching plan) — для плана ILearningDynamicGraph

Источник: каталог `C:/Users/lubao/Documents/xiigen/llm mvp/orchestrator teaching plan`.
Вопрос: как оркестратор обучается (план DPO/обучения).
Роль: рекон-аналитик (только чтение + запись этого файла). Это evidence для плана, не разрешение на реализацию.

---

## 1. Что это и где (точные ключевые пути)

Каталог `orchestrator teaching plan` — это набор планов/гайдов для пошагового обучения адаптивного оркестратора (`AdaptiveLogicalOrchestrator`) на DPO-парах из реальных XIIGen-сессий. Это **планы/гайды (.md)**, а не код (код лежит в `C:/Projects/llm_mvp_core`, эти файлы только ссылаются на него).

Прочитанные релевантные файлы (3–8, защита от перегруза):
- `C:/Users/lubao/Documents/xiigen/llm mvp/orchestrator teaching plan/ORCHESTRATOR-TEACHING-PLAN-v2.md` — главный план «одна пара за раз» (актуальная версия, заменяет v1).
- `C:/Users/lubao/Documents/xiigen/llm mvp/orchestrator teaching plan/ORCHESTRATOR-TEACHING-PLAN-v1.md` — предыдущая версия (для дельты).
- `C:/Users/lubao/Documents/xiigen/llm mvp/orchestrator teaching plan/XIIGEN-DRIVEN-SELF-TEACHING-GUIDANCE-v1.md` — как из XIIGen-сессии получить строки обучения по слоям (Layer 0-A … Layer 3) и прогнать end-to-end.
- `C:/Users/lubao/Documents/xiigen/llm mvp/orchestrator teaching plan/TEACHING-MEASUREMENT-GUIDANCE-v3.md` — как измерять и принимать строку (кумулятивное обучение, continue-training из чекпойнта).

Остальные файлы каталога (не читались детально, отмечены в «Пробелах»): `ORCHESTRATOR-TEACHING-PLAN-v1.md` (прочитан), `XIIGEN-EXPORT-TEACHING-ADDENDUM-v1/v2.md`, `TEACHING-MEASUREMENT-GUIDANCE-v1/v2.md`, `XIIGEN-EXPORT-1000-CASE-GENERATION-GUIDANCE-v1.md`, `CLAUDE-EXPORT-DEVELOPMENT-CYCLE-BUNDLE-MAP-v1.md`, `CLAUDE-EXPORT-FIRST-TEACHING-CYCLE-RESULT-v1.md`, `EXECUTOR-PROMPT-REGISTRY-PERSISTENCE.md`.

Внешние авторитеты, на которые ссылаются планы (НЕ в этом каталоге, доисследовать отдельно): `DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v2.md`, `EXAMINATION-METHOD-ORCHESTRATION-v7.md`, `EXTERNAL-SOURCE-REGISTRY-v6.md`.

---

## 2. Как устроено/работает (кратко)

**Парадигма — «одна пара за раз» (one row at a time), не batch.**
План учит одну DPO-пару, прогоняет живой prompt через все узлы flow, замеряет CPU/RAM/диск/время, проверяет verdict, решает promoted/held/rejected, и только потом берёт следующую пару (ORCHESTRATOR-TEACHING-PLAN-v2 §«What this plan is»).

**DPO-механизм (реальный путь, подтверждён код-ревью в v2 §Harness 3 и Step 2-3):**
- A-сторона пары → `DpoPair.Prompt`; B-сторона (правильная) → `Chosen`; намеренно неверная B → `Rejected`. Токенизация `BpeTokenizer.Encode()`.
- LoRA: `LoraApplicator.Apply(model, LoraConfig{ Rank=4, Alpha=2f → Scaling=0.5, TargetModules=["q_proj","v_proj"] })`. Базовые веса замораживаются (`RequiresGrad=false`), обновляются только адаптеры `LoraLinear` (`lora_a`, `lora_b` через `RegisterParameter`).
- Один шаг: `new DpoTrainer(policyModel, referenceModel, checkpointDir).Step(batch, step:rowIndex)`. AdamW пропускает замороженные веса (`Grad==null`). `model.SaveCheckpoint(path)` пишет полный чекпойнт включая веса адаптеров.
- v3 уточняет фактический дефолт: текущий `DefaultLogicalCapabilityTrainer` использует крошечный **Rank 2** (не 4), Alpha 4.

**5 обучаемых компонентов (каждый — `ILogicalUnit<TInput,TOutput>`)** (v2 §«The five components»):
| Компонент | Контракт | Роль |
|---|---|---|
| Prompt routing input compiler | `ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>` | Layer 0 — prompt → вход роутера |
| Multi-sub-phase router | `ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>` | Layer 0 — какие sub-phase затронуты |
| Domain splitter | `ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>` | Node 01 |
| Domain translator | `ILogicalUnit<DomainPacket, TranslatedDomainStatement>` | Node 02 |
| Scheme planner | `ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>` | Node 03 |

**Trust-машина / отбор обученного юнита:** обученный юнит регистрируется в `InMemoryLogicalUnitRegistry` со статусом `Probationary`. Фабрики (например `SecurityVerification34FlowFactory`) выбирают его через `ResolveLatest` только при флаге `UseLearnedUnitWhenAvailable=true`. Решение: verdict верный + margin≥0 → `promoted`; verdict верный, но confidence упал → `held`; verdict неверный → `Rejected` (откат на детерминированный fallback). 5 подряд `promoted` → `Trusted`.

**Кумулятивное обучение (v3 — главное требование):** runner НЕ создаёт новый оркестратор на каждую строку. Один долгоживущий topology/flow/unit-registry + trainer + orchestrator на весь bundle. После строки N — ретест 2-3 предыдущих принятых строк (regression window). Повторно затронутая sub-phase должна **загрузить активный чекпойнт и продолжить обучение** (continue-training), а не учиться с нуля → цепочка версий v1→v2→v3 (`ParentUnitId`, `ParentPolicyCheckpointPath`, `TrainingStepCount`). `ResolveLatest` выбирает новейшую не-Rejected версию.

**Цикл из XIIGen-сессии (GUIDANCE-v1, 5 стадий):** (1) прочитать и размапить сессию (PROMPTS/DEPENDENCIES/DECISIONS/ARBITERS); (2) зарегистрировать новые topology-записи (`AddOrUpdateSubPhase`/`AddOrUpdateTransition`); (3) сгенерировать строки обучения по слоям (Layer 0-A compiler → 0-B router → 1 transition → Node 01-09 → Layer 3 correction), upstream-перед-downstream; (4) учить каждый `ILogicalUnit` по одной строке; (5) end-to-end верификация полного оркестратора. Жёсткое правило: A-сторона = язык разработчика (prompt), B-сторона = реальный runtime-объект (как в `LogicalUnitResult.Output`), не описание.

**Измерения/леджер:** `TeachingRowRecord` (ресурсы тренировки/инференса, verdict, confidence before/after, margin, decision). JSONL-леджер `…/teaching-ledger/…jsonl`, одна строка на пару, без перезаписи. Stopping rules: 3 подряд rejected → стоп раунда; row > 10 000 ms → `REVIEW_ROW`; working-set delta > 200 MB → стоп; `LoraAdapterBytes` > 2 MB (D-05) → Rank 2 и рестарт от последней promoted-строки.

---

## 3. Релевантность к ILearningDynamicGraph (DPO/обучение/граф/оркестрация/измерение/teacher-provenance)

Это **прямой и высоко-релевантный прецедент** авто-построения DPO и обучения иерархии `ILogicalUnit`/`ILogicalOrchestrator` — то, что планируется для ILearningDynamicGraph.

- **DPO-построение:** готовая схема «A=prompt, Chosen=правильный runtime-объект, Rejected=ближайший неверный из corruption/held-out или из детерминированного fallback». Прямой шаблон для авто-DPO графа.
- **Граф/оркестрация:** компоненты уже описаны как узлы потока (Layer 0 → Node 01..09 → Layer 3) с правилом «upstream B-side = downstream A-side» (Node 01 B → Node 02 A → Node 03 A) — это и есть рёбра DynamicGraph с контрактами вход/выход. Совпадает с Graph-Style/DynamicGraph Reconstruction Gate из скила (нужно явно выписать nodes/edges/контракты/кто владелец финального решения на ребре).
- **Измерение/lifecycle:** есть полный lifecycle обучаемого юнита — train data (XIIGen rows), learning signal (DPO margin), evaluation (verification verdict + regression window), export/checkpoint (`SaveCheckpoint`), locator/registry (`ResolveLatest`), fresh-load и continue-training (parent-chained checkpoints v3). Это закрывает требования lifecycle-evidence из скила.
- **Teacher-provenance (КРИТИЧНО для «dpo legal build»):** источник teacher/ground-truth здесь — **XIIGen-сессии = first-party** (`source_license_state: "first_party"`, GUIDANCE-v1). Это согласуется с Legal Teacher / Provenance Gate скила: first-party данные допустимы как обучающая цель. **Но** во многих XIIGen-сессиях участвует архитектор/ассистент — если ассистент был закрытой проприетарной моделью (Claude/OpenAI/Gemini), то «architect response» как `Chosen`/DPO-target подпадает под запрет distillation-teacher → `DATASET_REBUILD_REQUIRED`. Это надо проверить per-source (см. Пробелы).

---

## 4. Классификация (по правилу trainable-architecture-decision-SKILL.md)

Каталог содержит **планы**, не код; классифицирую архитектурные сущности, которые планы описывают.

- **Пять обучаемых компонентов (compiler, router, domain splitter/translator, scheme planner)** → допустимый уровень **`ILogicalUnit`**. Каждый имеет типизированные вход/выход, LoRA/DPO-обучение, чекпойнт, registry-locator, trust-state lifecycle. Это разрешённый trainable-уровень (скил §«only allowed trainable architecture levels»). Вердикт по скилу: **OK_ALLOWED_LEVEL** при наличии lifecycle-evidence.
- **`AdaptiveLogicalOrchestrator`** → уровень **`ILogicalOrchestrator`** (разрешённый). Маршрутизация по веткам и fan-out по sub-phase — оркестрация, допустима на этом уровне.
- **`InMemoryLogicalUnitRegistry.ResolveLatest` + trust-state (Probationary/Trusted/Rejected/Candidate)** → **внутренний детерминистический хелпер (locator/selection lifecycle)**, НЕ отдельный trainable-уровень. По скилу: `OK_INTERNAL_DETAIL` — выбор «новейшей не-Rejected версии» по `RegistrationOrder` детерминирован и честно помечен как механизм отбора версии, а не «семантический выбор». Это допустимо как деталь внутри оркестратора.
- **Compiler predicate lexicon (девять предикатов SECURITY/TEST/…) через сопоставление trigger-terms в prompt** → это **детерминистический keyword/regex-matching хелпер** (скил §3: «regex/keyword/pattern matching»). ВНИМАНИЕ для плана ILearningDynamicGraph: если в графе предикатное сопоставление будет финальным runtime-авторитетом маршрутизации там, где требуется обучаемый юнит, это `FORBIDDEN_RUNTIME_SHORTCUT`. В текущих планах оно используется в т.ч. как способ генерации B-side compiler-пар (corpus-labeling) и как baseline — это допустимо при честной пометке baseline/helper. Вердикт: **BASELINE_MUST_BE_LABELLED** (нельзя выдавать за семантику; для обучаемой замены нужен обученный `PromptRoutingInputCompilerUnit`, что планы и предусматривают).
- **LoRA/DPO (`LoraApplicator`, `DpoTrainer`)** → не отдельный уровень, а **механизм обучения внутри `ILogicalUnit`** (компонент модели). По скилу `OK_MODEL_COMPONENT` — роль названа честно.
- **Wrapper `ExecuteAsync`: Embed → cosine similarity по HNSW → highest-scoring candidate** (v2 Step 4) → HNSW/cosine — **компонент модели/retrieval внутри `ILogicalUnit`**, не самостоятельная архитектура. По скилу KNN/retrieval — audit-triggered, допустим как компонент: `OK_MODEL_COMPONENT`. Риск: если кандидат выбирается фактически точным реплеем обучающего prompt (exact-prompt lookup), это leakage — нужна ablation/permutation-проверка (в планах явной ablation нет — см. Пробелы).

Итоговая классификация уровня каталога: набор планов корректно ложится на разрешённую иерархию (`ILogicalOrchestrator` → `ILogicalFlow`(узлы) → `ILogicalUnit`); детерминистические части (predicate-lexicon, ResolveLatest/trust) — внутренние хелперы, требующие честной пометки baseline/locator, а не статуса «семантический trainable».

---

## 5. Пробелы / что доисследовать

1. **Teacher-provenance per XIIGen-сессия (блокер для legal-build):** планы помечают XIIGen как `first_party`, но не проверяют, не является ли «architect response», используемый как `Chosen`/DPO-target, выводом закрытой модели (Claude/OpenAI/Gemini). Если да — по скилу `DATASET_REBUILD_REQUIRED`. Доисследовать происхождение «architect response» в реальных сессиях `C:/Users/lubao/Documents/xiigen/`.
2. **Не прочитаны (защита от перегруза):** `XIIGEN-EXPORT-TEACHING-ADDENDUM-v1/v2.md`, `XIIGEN-EXPORT-1000-CASE-GENERATION-GUIDANCE-v1.md`, `CLAUDE-EXPORT-*` (две штуки), `EXECUTOR-PROMPT-REGISTRY-PERSISTENCE.md`, `TEACHING-MEASUREMENT-GUIDANCE-v1/v2.md`. Особенно `1000-CASE-GENERATION` и `CLAUDE-EXPORT-FIRST-TEACHING-CYCLE-RESULT` важны для понимания масштаба и фактических результатов teacher-источников.
3. **Внешние авторитеты вне каталога:** `EXAMINATION-METHOD-ORCHESTRATION-v7.md` (формат пар по слоям), `DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v2.md` (каноническая иерархия flow), `EXTERNAL-SOURCE-REGISTRY-v6.md` (лицензии внешних источников). Без них нельзя финально подтвердить контракты узлов и лицензионный гейт.
4. **Граничные проверки leakage/ablation:** в планах нет явных permutation-invariance / no-exact-prompt-replay тестов для wrapper'а (Embed+HNSW). Для ILearningDynamicGraph нужно добавить ablation-гейт (правило скила про KNN как support-evidence, а не финальный авторитет).
5. **Соответствие коду (на момент планов 2026-05-05):** планы описывают сигнатуры (`DpoTrainer.Step`, `LoraApplicator.Apply`, `InMemoryLogicalUnitRegistry`, `AdaptiveLogicalOrchestrator`) — нужно сверить с фактическим `C:/Projects/llm_mvp_core/15.OrchestratorModel` и тренером (`DefaultLogicalCapabilityTrainer`), т.к. v3 уже фиксирует расхождение (Rank 2 vs декларированный Rank 4 в v2/v1).
6. **DynamicGraph-реконструкция:** планы описывают линейную цепочку слоёв, но не many-to-many/fan-in/async-рёбра в терминах Graph-Style Gate скила — для ILearningDynamicGraph эту часть надо выписать заново (nodes/edges/multi-output contracts/кто владелец решения на каждом ребре).
