# ПРОТОКОЛ ИЗВЛЕЧЕНИЯ ОРКЕСТРАЦИИ — v2

## ГЛАВНОЕ ПРАВИЛО

> Извлечение оркестрации — это не «что входит в план».  
> Это «как цель пользователя превращается в контролируемый, проверяемый путь к работающему результату».

**Применение:** Когда есть пакет документов (ZIP, набор md-файлов, сессионные файлы, вложения) и нужно объяснить оркестрацию — конкретный маршрут решения задачи на человеческом языке.

**Источники:** REAL-CHAT-ORCHESTRATION-CHAINS-v2.md, USER-ORCHESTRATION-SCENARIOS-FROM-EPISODES-1000-v8.md, ORCHESTRATION-EXTRACTION-PROTOCOL-v1.md, PROTOCOL-ORCHESTRATION-EXTRACTION-FROM-INPUTS-AND-PLAN-v1.md, разборы FLOW-16 → FLOW-24.

---

### ОБЯЗАТЕЛЬНЫЕ ВЫХОДЫ ПРИ ТЕСТИРОВАНИИ НОВОГО ФЛОУ

> Протокол без оркестрации — это анализ без результата. Оркестрация — ПЕРВИЧНЫЙ выход. Всё остальное — вторичное.

При тестировании нового FLOW обязательны ОБА выхода, в этом порядке:

**1. Оркестрация (PRIMARY) — формат ШАГ 8-9:**
```
ЗАЧЕМ → КАК ВЫГЛЯДИТ → КАК УСТРОЕНО → В КАКОМ ПОРЯДКЕ ДЕЛАЛИ
```
- Обязательна всегда, для каждого FLOW без исключений
- Для Тип E (training data package) — частичная оркестрация: описать то, что извлечено, явно отметить что недоступно
- Нельзя пропустить даже когда «протокол уже понятен» или «файлы знакомы»
- Нельзя заменить списком багов или таблицей артефактов

**2. Расширение протокола (SECONDARY):**
- Новые паттерны, типы багов, типы входных данных — только то, чего не было раньше
- Добавлять ПОСЛЕ написания оркестрации, не вместо неё

**Ошибка FLOW-36/FLOW-38 (зафиксирован паттерн):**
Была написана расширение протокола без оркестрации → пользователь не получил основной результат. Симптом: «ты анализировал файлы, но ничего не рассказал». Исправление: оркестрация всегда идёт первой, даже если она частичная.

---

## ШАГ 1 — ПРИЁМ И КЛАССИФИКАЦИЯ МАТЕРИАЛА

### Список полученного

```
Что получено:
- [файл/архив 1] — тип + роль
- [файл/архив 2] — тип + роль
...
```

### Классификация источников

| Тип источника | Что означает | Как использовать |
|---|---|---|
| USER_PROMPT | Задача которая запустила работу | Определяет цель |
| INPUT_FILES | Исходный материал до планирования | Домен, ограничения, бизнес-описание |
| PRODUCED_FILES | Файлы созданные процессом планирования | Что процесс произвёл |
| STATE_FILE | Текущий прогресс / завершение / валидация | Завершено ли, что следующее |
| CANONICAL_MERGED_DOC | Текущая принятая истина системы | Приоритет над input-файлами |
| PLAN_FILE | Человеческий или implementation-план | Предполагаемый путь выполнения |
| VALIDATION_FILE | Доказательство или отчёт об ошибках | Подтверждены ли заявленные результаты |
| RAW_SESSION | История разговора | Только для понимания как родился план |
| GENERATED_OUTPUT | Предыдущий ответ модели | Свидетельство; проверять по источнику |

**Порядок доверия (убывающий):**
1. Текущая инструкция пользователя
2. Конкретный протокол/скилл указанный пользователем
3. Состояние / канонические merged-документы
4. Произведённые артефакты планирования
5. Input-файлы
6. История сессии
7. Прежние сгенерированные сводки

### Обязательный пакет

Никогда не извлекать оркестрацию из одного файла если существуют input/context файлы.

Необходимый пакет:
```
1. Запрос пользователя или источник задачи
2. Input-файлы / input-ZIP
3. Произведённые файлы / produced-ZIP
4. State-файл
5. Финальный план или implementation-план
6. Validation-отчёт (если есть)
```

### Определить «единственный источник правды» для этого FLOW

До чтения остального — найти один файл который содержит ВЕСЬ FLOW: функционал + ренумерацию + фазы + примеры + постсостояние.

Признаки:
- Называется FLOW{N}_P0_PLAN.md или FLOW{N}_MASTER_PLAN.md
- Содержит NO-CODE EXPLANATION + RENUMBERING MAP + PHASE BREAKDOWN в одном файле
- Остальные файлы пакета — дополнения к нему, не самостоятельные источники

Если найден → читать целиком прежде чем открывать другие файлы.

**Порядок чтения если нет явного источника правды:**
1. CANONICAL_MERGED_DOC (SESSION_STATE_MERGE, ENGINE_ARCHITECTURE_MERGED)
2. PLAN_FILE (MASTER_EXECUTION_PLAN, FLOW{N}_P0_PLAN)
3. VALIDATION_FILE
4. INPUT_FILES

---

## ШАГ 2 — СКАНИРОВАНИЕ СТРУКТУРЫ

Из главного файла выписать четыре вещи и больше ничего:

| Что | Откуда | Что именно |
|-----|--------|------------|
| Задача | Заголовок / цель | В чём проблема которую решает этот флоу |
| Что уже есть | Backward compatibility / Prior state | Что существовало до (цифры, версии, диапазоны) |
| Что добавляется | Gap analysis / New artifacts | Что нового (конкретно: F631-F640, не «10 фабрик») |
| Фазы | Phase map / DAG overview | Этапы по порядку |

**Стоп.** Всё остальное — детали. К ним вернёмся на шаге 4.

Проверка готовности: можно ли написать одно предложение?
```
Мы строим [X] который позволяет [кому] делать [что].
```
Если предложение не формулируется — остановиться. Оркестрация ещё не понята.

---

## ШАГ 3 — СИНХРОНИЗАЦИЯ С АКТУАЛЬНЫМ СОСТОЯНИЕМ

### RENUMBER_MAP

Если есть файлы ренумерации (RENUMBER_MAP.json, renumber_map.md):
- Выписать отображение старых номеров на новые
- Все дальнейшие ссылки — только по НОВЫМ номерам
- Никогда не смешивать старые и новые ID без явного объяснения ренумерации

### SESSION_STATE: двойная роль

SESSION_STATE несёт **двойную роль**:

**Роль 1 — Граница:** последний завершённый артефакт (F___, T___, SK___)
- Всё ниже — уже существует, не трогать
- Всё выше — новое, именно это описываем

**Роль 2 — Zone Decomposition Table:** зоны A–I с привязкой к семействам фабрик
- Это Layer 2 материал, фиксировать при первом чтении
- Не ждать ENGINE_ARCHITECTURE

### SESSION_STATE: многоблочный формат (критично)

SESSION_STATE — единственный из 7 канонических файлов который **дополняется, а не перезаписывается** при каждом FLOW-merge. Это значит:

- **Первый заголовок файла может быть устаревшим** — он остался от раннего FLOW, не был обновлён
- Счётчики в начале файла могут показывать состояние двух-трёх флоу назад
- **Правда = самый поздний блок** с заголовком «Post FLOW-N» или «SYSTEM STATE (Post FLOW-N)»

**Алгоритм чтения SESSION_STATE:**
```
1. Прочитать конец файла (последние 5000 символов) — там самый свежий блок
2. Найти самый поздний «SYSTEM STATE» с наибольшим N в «Post FLOW-N»
3. Только из этого блока брать глобальные счётчики (F___, T___, CF___, SK___)
4. Первый заголовок файла — НЕ источник правды о текущем состоянии
```

**Эволюция этого бага по флоу (паттерн):**

| FLOW | Тип документального бага |
|------|--------------------------|
| FLOW-01 | **Document Alias Substitution (FLOW-39/36 паттерн, полная замена)**: RESTRUCTURED-ARCHITECTURE.md использует псевдонимы везде: «the user registration flow»=FLOW-01, «the multi-stack porting flow»=FLOW-37, «the learning loop flow»=FLOW-38, «the local model training flow»=FLOW-39, «the business onboarding flow»=FLOW-02, «the event creation flow»=FLOW-03(?), «the content management flow»=FLOW-04(?), «the programming interface»=API, «the search index»=Elasticsearch, «the retrieval system»=RAG — кросс-референция с canonical документами без таблицы перевода невозможна. **Multi-Version Document Proliferation без canonical marker**: 8+ версий одного flow (v2, v3, v4, replan v1/v2/v3, restructured) — ни один документ не помечен `"isCanonical": true` или supersedes-pointer; RESTRUCTURED-ARCHITECTURE.md (2026-03-24) фактически canonical но не объявлен явно. **Infrastructure Gate Count Mismatch**: grok analysis = «14 чеков (8+6)», RESTRUCTURED-ARCHITECTURE.md = «8 original + 5 new (checks 9–13) = 13 total» — расхождение 14 vs 13; canonical document = 13. **V9-003 Severity Inconsistency**: документ говорит V9-003 = «score-0 warning, not BUILD_FAILURE» на первом flow — но для FLOW-02 и далее это же условие должно быть BUILD_FAILURE; нет явного переходного правила. **Phase C conditionality implicit**: SESSION-C.md не содержит явного if/else gate («если нет patches — skip C → jump to D»); риск выполнения Phase C по умолчанию. **Новый тип флоу: Domain Business Flow (Post-Learning Infrastructure)** — первый domain flow после FLOW-37/38/39 ACTIVE; движок прибывает уже с calibration memory + distillation + shadow runs + V9 gate; 7-document execution architecture; LEARNING-HANDOFF.json к следующему flow; dogfood-first (XIIGen строит собственный Community Platform). |
| FLOW-02 | **Document Alias Substitution (pre-canonical era)**: `flow_id: the business onboarding flow` (не FLOW-02); prerequisite: `the user registration flow complete` (не FLOW-01); нет F/T/BFA-номеров; cross-референции без таблицы перевода невозможны. **Debounce Rule Hidden in Edge Cases**: правило «повторная обработка только через 5+ минут» описано только в Edge Cases (#7 — High-frequency questionnaire updates); в YAML/event chain отсутствует; для движка = runtime policy, должна быть явно в Flow DSL (per-subject lock на `(userId, FLOW-02)`). **Redis Eviction Gap**: 6 разных TTL-паттернов без документации Redis maxmemory-policy; при memory pressure → silent personalization regressions без видимых ошибок в API. **Новый тип флоу: Pre-Canonical Domain Business Flow** — functional spec + deep research без F/T/CF-номеров; параллельные ветки + explicit join; circuit breakers; MACHINE/FREEDOM разделение отсутствует явно. |
| FLOW-03 | **Document Alias Substitution**: `flow_id: the event creation flow`; нет F/T-номеров; pre-canonical format. **Phantom Consumer**: `PromotionCampaignCompleted → billing-service` в event_chain; billing-service НЕ перечислен в Services table, нет спецификации — phantom consumer без владельца. **Hidden Sequential Constraint**: YAML event chain: `EventCreated → analytics-service, matching-service, search-index-service` — выглядит параллельным; Business Flow section явно: Matching ждёт EventIndexed (Elasticsearch должен быть готов ДО matching query); sequential constraint невидим в YAML. **Event Naming Gap**: `EventMatchesCalculated` появляется в event_chain, но не в Happy Path steps — внутренний aggregation step без явного документирования. |
| FLOW-04 | **flow_id Semantic Mismatch (CRITICAL)**: `flow_id: the event attendance flow` присвоен файлу `04-post-publishing.md` (Post Publishing & Feed Distribution); «event attendance» семантически относится к посещению мероприятий, НЕ к публикации постов; исторический артефакт переименования без обновления alias; FLOW-03 document тоже ссылается на «the event attendance flow» для обозначения FLOW-04 → двойная путаница в cross-референции. **Duplicate BFA Section**: две идентичные секции «Business Flow Arbiter Integration» в одном документе (copy-paste артефакт). **Join Policy Hidden in Edge Cases**: join ALL_REQUIRED + TIMEOUT_THEN_FALLBACK только в Edge Cases (#2), не в YAML event chain; engine требует явный join mode с partial failure semantics. **InfluxDB Unique Infra**: Analytics Service требует InfluxDB (time-series) — уникальное требование FLOW-04; FLOW-02/03 используют только Elasticsearch; не задокументировано в infrastructure spec платформы. |
| FLOW-05 | **QuestionnaireAnswered vs QuestionnaireCompleted (Event Naming Split)**: два разных имени одного события — `QuestionnaireAnswered` в event_chain и `QuestionnaireCompleted` в тексте и sequence diagram; ни одно не зафиксировано canonical; нормализация обязательна до реализации (имя влияет на все consumer subscriptions). **Redis Pub/Sub vs Never-Lose-Points Conflict**: functional spec указывает Redis Pub/Sub (at-most-once) как основной транспорт; «никогда не терять очки/ачивки» несовместимо с at-most-once; deep research рекомендует Redis Streams для critical events; spec и реальные требования contradictory. **InfluxDB for Canonical Gamification State**: Gamification Service на InfluxDB (time-series с retention policies → данные могут удаляться); canonical state (текущие очки, уровень) требует durability; deep research: MongoDB (canonical) + InfluxDB (ledger) — не разделено в spec. |
| FLOW-06 | **Cooperator Service Identity Ambiguity (3 sources, 3 answers)**: functional spec = отдельный Cooperator Service; deep research = расширение Matching Service (Skill 47); engine deep research = отдельный Python + ML сервис; три источника дают три разных архитектурных ответа; требует explicit architectural decision до начала реализации. **Neo4j Undocumented Infra Dependency**: Connection Service требует Neo4j для graph queries (purchase affinity); Neo4j не упомянут как часть текущей инфраструктуры платформы ни в одном другом документе; значительная операционная добавка без explicit infra acceptance. **Dynamic Pricing Undefined**: «dynamic pricing adjustments» упомянуты как часть флоу без правил, триггеров или истории изменений; 48-hour cooperator grace period документирован, negotiation workflow нет. |
| FLOW-07 | **ML Model Undefined (Weight Adjustment)**: adjustment range -0.2 до +0.2 документирован; model type/feature set/versioning/deployment pipeline не определены; при отсутствии deployed model → adjustment = 0.0 (нейтральный), НЕ 0.5; нет fallback strategy в spec. **Purchase History Privacy Tension**: sub-weight proxy обязателен (aggregate score, никаких line-items в event payload); implementation constraint не специфицирован явно; требует explicit architectural решения. **Mutual Pending Auto-Accept Race Condition**: detect both + accept both + emit FriendRequestAccepted без race требует atomic операции; ни spec, ни deep research не описывают механизм предотвращения race (optimistic lock? distributed lock?). **Feed Service Durability Ambiguity**: Redis Cluster = cache-like или requires durability? При restart без persistence → feed теряется; deep research фиксирует вопрос без ответа; требует explicit решения до реализации. |
| FLOW-08 | **Research Artifact Without Base Spec**: FLOW-08 = только два research reports; нет файла `08-*.md` с user stories/event definitions/service inventory; это исследовательский артефакт, не production flow; findings поглощены FLOW-15 (Multi-Tenant Architecture). **Graduation Threshold Not Quantified**: рекомендация «hybrid bridge as default» без конкретных graduation criteria (N записей, compliance label = pci → graduate); конкретные пороги должны быть задокументированы в FLOW-15. **Report 1 vs Report 2 Duplication**: оба отчёта покрывают одни темы с небольшими вариациями; Report 2 = refinement; canonical = Report 2 (не объявлено явно). **Паттерн: Research Artifact Flow** — нет base spec; нет F/T/CF-номеров; нет implementation plan; только research findings + extracted MACHINE-правила; признак поглощения в downstream flow. |
| FLOW-09 | **Pre-Canonical Factory Number Collision (F244 в трёх флоу)**: FLOW-09 plan.md объявляет F244–F255 (Event Participation). FLOW-10 master plan объявляет F244–F287 (Tenant Control Plane + 6 families). FLOW-11 master plan также объявляет F244–F287 (Social Network, 6 families). Три документа claim F244 для РАЗНЫХ фабрик — pre-canonical era: central numbering registry отсутствовал; canonical numbers только в ENGINE_ARCHITECTURE_MERGED. **«the marketplace flow» Alias Conflict**: FLOW-09 называется «the marketplace flow» в source docs; FLOW-10 иногда тоже; один псевдоним на два разных flow в двух документах. **Engine Primitives 4 и 5 First Introduction**: EP-4 Saga Compensation Registry и EP-5 Idempotency Fabric впервые вводятся как named primitives; DNA Pattern 9 (Idempotency-First) первое появление как named pattern. **O(n²) Fan-Out without per-tenant quota**: ceiling (500 пар/batch) = FREEDOM-global; в multi-tenant контексте должен быть per-tenant, иначе крупный ивент одного тенанта блокирует очереди другого. |
| FLOW-10 | **Pre-Canonical Factory Number Collision (F244, второй конфликт)**: FLOW-10 объявляет F244–F287 (7 factory families: Tenant Control Plane → Notification Hub); тот же диапазон что FLOW-09 (F244–F255) и FLOW-11 (F244–F287); canonical numbers только в ENGINE_ARCHITECTURE_MERGED. **«payments flow» vs «marketplace flow» Alias Confusion**: FLOW-10 = «the payments flow» внутренне; иногда также «the marketplace flow» (тот же псевдоним что FLOW-09); два псевдонима для одного флоу, оба пересекаются с FLOW-09. **Task Type Offset +20 (Hidden Remap)**: P7a/P7b task type documents используют числа с offset +20 относительно P6a/P6b без объяснения; преднамеренный remap при merge, но без знания этого P7 нечитаем. **No SESSION_STATE**: 7 factory families (44 interfaces) введены без отдельного SESSION_STATE-файла; continuity только через ENGINE_ARCHITECTURE_MERGED. |
| FLOW-11 | **Triple Alias Collision «the marketplace flow» (КРИТИЧЕСКИЙ)**: FLOW-09 = «the marketplace flow» (Event Participation). FLOW-10 = иногда «the marketplace flow» (Shops). FLOW-11 = «the marketplace flow» = Flow 3 (Post + Media Publish) внутри документа. Один псевдоним на три разных flow в трёх разных документах — cross-референции полностью сломаны без явной таблицы перевода. **Pre-Canonical F244 Collision (третий документ)**: FLOW-11 также декларирует F244–F287 (Social Network families 27–32); тот же диапазон что FLOW-09 и FLOW-10. **Engine Primitive 2 Scope Expansion without Documentation**: EP-2 (Durable Timer) используется как async WAIT GATE для MediaReady сигнала — новая семантика; ранее EP-2 = только expiry timers; расширение семантики не задокументировано в EP-2 definition, только в task type spec. **DNA Pattern 9 First as Cross-Cutting Phase 0**: DNA-9 (Multi-Tenant Isolation Layer) добавляется в Phase 0 (до всех остальных фаз) — впервые cross-cutting DNA pattern = first priority; 396 compliance checks (44 factories × 9 patterns). |
| FLOW-12 | **Base Document is Q&A, Not Spec**: `12 - ERP systems.md` = Q&A сессия («Can you describe in modules what ERP system like SAP B1 contains?»); не flow-specification; Q&A использован как domain context → Factory interfaces F288–F303 через engine pattern (IDocumentChainService, ILedgerService, IThreeWayMatchService — прямые отображения из Q&A). **Two Chains in One Plan File**: MASTER_EXECUTION_PLAN содержит два независимых расширения: Multi-Tenant Control Plane (Families 27–29, F244–F271) + ERP Integration (Family 32, F288–F303); SESSION_STATE фиксирует пост-FLOW-12 = F1–F303 как один canonical state. **F11 Suffix Ambiguity**: суффикс `F11` в ENGINE_ARCHITECTURE_F11.md = Architecture Version 11 (НЕ FLOW-11); суффикс `F13` в SESSION_STATE_F13 = Flow Number; разные суффиксы означают разные вещи в разных файлах одного набора → путаница при поиске и recovery. |
| FLOW-13 | **Mixed File Suffixes (F11 vs F13)**: ENGINE_ARCHITECTURE_F11.md и MASTER_EXECUTION_PLAN_F11.md — суффикс F11 = Architecture Version; SESSION_STATE_F13.md и TASK_TYPES_CATALOG_F13.md — суффикс F13 = Flow Number; в одном наборе файлов суффикс означает разные вещи → потенциальный источник путаницы при recovery. **F288 Additive Merge (корректно)**: FLOW-12 ERP добавил F288–F303; FLOW-13 Finance также начинает с F288 в контексте своего canonical continuation; SESSION_STATE_F13 подтверждает pre-FLOW-13 baseline = F1–F287; F1–F329 = корректный суммарный диапазон после FLOW-13. **DNA Pattern 9 (SoD) First Consumption**: DNA-9 (Segregation of Duties) уже существует в движке до FLOW-13; FLOW-13 первым потребляет её в 10 task types через DNA compliance station; новые архетипы DURABLE_SAGA и HUMAN_GATE — первые в движке. **Domain Context Base Document**: аналогично FLOW-12, `13 - finance.md` = domain context, не flow-specification; domain concepts AP/AR/GL/Treasury транслированы в Factory interfaces через engine pattern. |
| FLOW-14 | **«werehouse» Typo in ALL 11 Filenames**: все 11 файлов FLOW-14 содержат «werehouse» вместо «warehouse» в именах файлов; внутри самих файлов слово написано правильно; поиск по `*warehouse*` НЕ находит эти файлы — нужен `*werehouse*`; критично для filename-based RAG indexing в XIIGen. **F329→F425 Intermediate Gap (~96 factories)**: SESSION_STATE_F13 = F1–F329; FLOW-14 baseline = F1–F425; delta +96 фабрик из промежуточных флоу, не входящих в набор source-docs; FLOW-14 хронологически позже чем порядковый номер 14 предполагает. **Partial Canonical Format (5/7 files)**: из 7 canonical файлов только 5 эквивалентов; UNIFIED_SOURCE_INDEX и SESSION_STATE отсутствуют как отдельные файлы; P7 (итоговый merge) не найден; P8_VALIDATION подтверждает 26/26 PASS — все фазы выполнены, но canonical files не сохранены. **«the notifications flow» Alias for Data Warehouse**: внутреннее имя = «the notifications flow» для Data Warehouse & Integration Engine; семантически не связаны; alias, вероятно, назначен при изначально другом scope или переиспользован из другого контекста. |
| FLOW-15 | **Dual-Segment Non-Contiguous Factory Range** — FLOW-15 охватывает два несмежных диапазона: Segment A (Family 27, F244–F260, MVP Builder core) и Segment B (Families 64–67, F493–F509, Monetization/Plugin/Sandbox extension); между ними ~232 фабрики (FLOW-16/17/18/19); оба сегмента называются «FLOW-15» без явного обозначения разрыва. **V-Number File Naming** — файлы FLOW-15 названы "15-build mvps_*.md" (не FLOW15_*.md), что разрушает стандартную схему поиска по имени. **Session-Isolation Renumbering Bug (ранняя форма)** — P4B input files написаны при стейл-базелайне (F466/Family 64); фактические P4B номера = F493/Family 64–67; явный renumber required. **4-Model Research Minimum MACHINE Rule** — DR21 устанавливает min=2 из 4 моделей как BUILD FAILURE; ни один CF rule не регистрирует это правило в BFA index; enforcement = document-only. **Managed Primitives Factory Set** — F252–F256 = пять разных фабрик для DB/Storage/Auth/Email/Payments; каждая независимо swappable; LIFO compensating rollback при partial failure (dependency provision order MACHINE). **Rollback via Engine Primitive 2** — IRollbackControllerService (F258) использует EP-2 (durable timer) для 7-day rollback window; window survives restarts; нет rollback без immutable snapshot (F257) — sequence enforced. **Paywall miss-default=deny (DD99)** — cache miss → F505 PG fallback → allow; brief false-negatives acceptable; never default-allow на cache failure — MACHINE. **CoW Sandbox Fork pattern** — F501–F504 Copy-on-Write: read checks sandbox first → live fallback; defers copy cost до фактической модификации; commit atomic via PG transaction. **Новый тип флоу: Multi-Tenant App Factory** — движок строит complete tenant apps: ConversationBuilder→IntentCompiler→ResearchConsensus(4-model)→AgentOrchestrator→CodegenPipeline→ManagedPrimitives(5 свапаемых)→VersionControl→Rollback(EP-2); POOLED→SCHEMA→DEDICATED graduation без обратной миграции; Template 38 dispatches on 4 entry events (Subscription/Plugin/Sandbox/Export). |
| FLOW-16 | **Source File Naming Mismatch** — ожидаемые файлы FLOW16_PLAN.md; реальные файлы = "16 - giant shop platforms deep research*.md" + training corpus run-124; ни одного FLOW16_PLAN.md не существует; первичный источник = Grok transcript (16 flow orchestrator.md). **EP-4/EP-5 без EP Enum Update документа** — FLOW-16 вводит EP-4 (Idempotency Key Registry) и EP-5 (Transactional Outbox) как новые Engine Primitives; нет отдельного "EP enum update" документа аналогичного тому как EP-1/2/3 были задокументированы; регистрация EP = в теле factory specs только. **Days-Long Saga Pattern (Cart-to-Order)** — T221 длится дни (не минуты); escrow state machine Created→Funded→Released span weeks; compensation = multi-step LIFO saga; нет документированного maximum saga duration timeout. **Validation 99/99** — Grok analysis утверждает 99/99 PASS; ни один документ не даёт расшифровку что именно 99 проверок покрывает; число несовпадает ни с BFA (14) ни с stress tests count. |
| FLOW-17 | **Session-Isolation Renumbering Bug (первый задокументированный P0-renumber-map)** — input files написаны для "multi-tenancy flow" starting at F466 (families 60–68); canonical baseline before FLOW-17 = post-FLOW-16 (F579+); offset: +114 factories, +15 families, +48 task types, +56 BFA rules, +45 stress tests, +34 skills, +30 DDs, +23 DRs, +10 templates; P0 plan содержит explicit полную таблицу ренумерования. **V17-Catalog/FLOW-17 Naming Confusion (системный)** — TASK_TYPES_CATALOG_V17.md покрывает "the event attendance flow / Post Publishing & Feed Distribution" (V66) — НЕ FLOW-17 Freelancer Marketplace; аналогично V18=Event Discovery/Gamification; V19=Group Management Marketplace — V-catalog numbers и FLOW numbers — совершенно разные системы нумерации в одной папке old/. **PHASE_17_STATE.json Mislabel** — этот файл = migration track Phase 17 (Node.js migration complete); НЕ FLOW-17 orchestration state; имя файла создаёт ложное впечатление. **REPUTATION archetype не зарегистрирован** — Grok analysis и Family 75 (immutable reputation journal) описывают REPUTATION как паттерн; в P0 plan только ESCROW_SAGA явно названа как новый archetype; REPUTATION = text-only, нет formal ContractArchetype enum entry. **Screenshots as External Refs (MACHINE)** — Family 81 Work Delivery: activity screenshots stored ONLY as external references (S3 path), никогда inline binary в DB; access gated to contract parties only (IR hardcoded); нет CF rule явно регистрирующего это в BFA cross-flow protection. **ESCROW_SAGA LIFO compensation** — milestone state machine (CREATED→FUNDED→RELEASED) + compensation при failure = LIFO saga; nesting: milestone компенсация внутри contract компенсации; compensation depth не задокументирована. |
| FLOW-18 | **Session-Isolation Renumbering Bug** — 18-* input files используют F466–F508 (DevOps/Flow Creation) при стейл-базелайне; canonical рефactor требует ренумерации до F631–F696 (delta+166 offset minimum). **IFlowDesignerService UI Fabric — отсутствует CF Rule** — FLOW-18 вводит критическое архитектурное правило «НИКОГДА не импортировать React/Angular напрямую — всё через IFlowDesignerService с resolution via config»; нет CF rule фиксирующего это в BFA registry; compliance = architectural intent only. **F692 IExternalGitSyncService — отсутствует CF Rule для "no provider SDK"** — "generic HTTP ONLY, never Octokit or GitLab SDK" — правило документировано в factory spec, нет BFA CF rule; будущие flows могут добавить Octokit без detection. **FLOW-18 vs FLOW-26 Meta-Flow Confusion** — оба называются «meta-flow»; FLOW-18 = USER-DRIVEN visual flow authoring (пользователь рисует, движок генерирует код); FLOW-26 = ENGINE-DRIVEN self-extension на CapabilityGapDetected; совершенно разные архетипы под одним ярлыком. **GENERATED→CORE Promotion Ladder** — 5 стадий (GENERATED→TESTED→INJECTED→MINIMAL→CORE); CORE = human approval gate; ST-176 утверждает «full cycle <1 hour» — achievable только для simple nodes; complex nodes с 5 factory deps требуют manual code review и могут занять дни. **OT/CRDT conflict resolution** — Family 89 (F668–F673) вводит operational transform / CRDT для 50-user concurrent editing; нет документированного fallback при OT convergence failure; ST-166 проверяет «no data loss» но не проверяет correctness of merged result. **Новый тип флоу: Visual Flow Authoring / User-Driven IDE** — пользователь описывает intent визуально; движок генерирует factories + task types + flow defs; UI Fabric resolves platform (React/Angular/Svelte/Native) via config; GENERATED→CORE ladder с human gate; OT/CRDT collaboration; AF-11 self-improvement feedback loop. |
| FLOW-19 | **Dual-Meaning "Phase 19" vs "FLOW-19"** — PHASE_19_CERTIFICATE.md = migration track Phase 19 (стек-миграция .NET/ReactNative → Node.js 20/TS5/NestJS10/React18); FLOW19_PLAN.md = CI/CD & DevOps Control Plane; оба документа в одной папке old/ с одинаковым числовым префиксом; критически разные сущности. **TASK_TYPES_CATALOG_V19.md Naming Confusion (продолжение V17/V18)** — покрывает "V72 Group Management Marketplace Publishing" — НЕ FLOW-19 CI/CD; V-catalog numbering vs FLOW numbering = параллельные несовместимые системы. **Session-Isolation Renumbering Bug** — input files F466–F502 (families 60–68); canonical baseline before FLOW-19 = post-FLOW-18 (F696); offset: +231 factories, +34 families. **IHallucinationDriftService (F727) Integration Gap** — Family 101 содержит IOtelCollectorAdapterService, IControlPlaneAuditService, IHallucinationDriftService; F727 (hallucination drift) — нет cross-reference к learning loop indices (xiigen-calibration-memory, xiigen-shadow-runs) или AF engine; potential orphan service. **ReadinessReport-as-Sole-Gate** — IReadinessReportService (F719) = SOLE gate для promotion; никакая другая approval не достаточна; 7-day DR drill rule для Tier-1 = MACHINE invariant; нет документированного что происходит при ReadinessReport expiry (>7 days). **IPipelineAdapterService abstraction** — CI provider = config choice (Jenkins/AzDO/GHA/GitLab all behind same interface); нет CF rule блокирующего hardcoded CI provider import в generated code. |
| FLOW-20 | **Dual Contract Namespace (T287–T306 + T625–T628)** — RUN-171 reconciliation показывает OLD contract block T287–T306 и NEW block T625–T628 оба зарегистрированы одновременно; reconciliation status = PARTIAL; 6 discrepancies; 18 из 20 task type processes = INTERNAL_ONLY. **V20 File Name Collision** — V20_STATE.md + V20_SKILLS_FACTORY.md + V20_RAG_INDEX.md в old/ относятся к XIIGen architecture version V20 (Code Injection Pipeline era, ~2026-02-23); FLOW20_PLAN.md = Sponsored Content + Graph API (~2026-02-27); "V20" = architecture version, "FLOW-20" = flow; SESSION_STATE_2026-03-01.md labels FLOW-20 как "Monitoring & Observability" в flow registry vs "Sponsored Content + Graph API" в plan — три разных имени для одного flow. **Political Dual-Gate completeness** — DD-168: два независимых ворот обязательны; neither gate substitutes for the other; нет документированного enforcement mechanism (CF rule или static check) — только DD text. **Consent-First Blocking Gate** — DD-167: consent evaluation ДО любой targeting; конкретный factory/task type = T193 + F525; но: нет документированного что происходит при consent service outage (fail-open vs fail-closed); риск GDPR нарушения при outage. **Stateless Auction Engine <50ms** — DD-166: Redis-only hot state; MACHINE latency bound; нет документированного fallback при Redis outage (auction fails → no ad delivery, или fallback auction, или no-fill); 118 фабрик — крупнейший одиночный FLOW; нет явной factory count justification. **Append-Only Spend Ledger** — DD-169: corrections = offset entries only; нет CF rule предотвращающего UPDATE на ledger rows; enforcement = architectural intent только. |
| FLOW-21 | **Variable T-Range in Plan File** — v4 plan использует `$T_START–$T_END` вместо конкретных T307–T326; реальные числа берутся из Wave 4 pre-allocation table (отдельный документ); риск: разные читатели могут резолвить переменные по-разному. **PLATFORM-ONLY Factory Count Mismatch** — 49 factories total = 45 INJECTABLE + 4 PLATFORM-ONLY (F867/F874/F897/F900); если code generator считает все 49 injectable → injection failures на PLATFORM-ONLY. **DWH Routing Cross-Flow Dependency without CF Rule** — "forms never write DWH directly, must go through the notifications flow" = runtime cross-flow dependency; нет CF rule в BFA registry; enforcement = text-only. **Document Alias Substitution** — "the notifications flow" (FLOW-14) не идентифицирован по номеру в документе. **Новый паттерн: Persist-Before-Emit + Anti-Spam-Before-Persist dual sequencing** — два nested stage-gate invariants (invariant 15 part 1 + part 2); оба проверяются через static code-level grep; нарушение = score-0. |
| FLOW-22 | **Gap #77 HARD BLOCKER carry-forward** — Named check registry externalization (160+ entries) должна быть решена до Phase A; но gap создан в FLOW-21 и перенесён в FLOW-22 без resolution; нет blocking gate в Phase A проверяющего gap #77 status. **Multi-Version Proliferation** — v2 + v4 в одной папке; v2 содержит 8 критических ошибок (E1–E8); нет explicit "supersedes" pointer; риск читать v2 как canonical. **ETag Conflict Cycle Limit** — cycle limit = 3 из FREEDOM config (E7); поведение при превышении лимита не задокументировано (infinite retry? DataProcessResult.Error?). **4 STACK_COUPLED vs 4 INCOMPATIBLE Labeling** — одни и те же 4 task types названы STACK_COUPLED в одной секции и INCOMPATIBLE (php-wordpress) в другой; разные классификации одного и того же набора в одном документе. **PG-FIRST sequential (DD-195)** — ES + PG persistence: PG first, ES async retry on failure; это означает ES может быть временно out-of-sync с PG; поисковые запросы против ES могут возвращать stale data; нет документированного компенсирующего механизма для ES-miss windows. |
| FLOW-23 | **BFA Range Typo** — header документа: CF-430–CF-451 (22 rules); authoritative: CF-430–CF-449 (20 rules); STATE.json требует флага `bfa_typo_resolved: true`; risk: система может зарегистрировать 22 CF slots вместо 20. **Document Alias Substitution** — canonical name "Visual Editor Extended (Canvas/Layout)"; Grok file называет "the form builder flow"; в prerequisite chain "the form builder flow" и "the publishing flow" — оба alias без FLOW-номера. **PLATFORM-ONLY Family Boundary Ambiguity** — F967–F970 (4 PLATFORM-ONLY) находятся в диапазоне Family 136 (Layout Engine) по F-номерам, но представляют cross-cutting concerns; какая family владеет этими фабриками неоднозначно. **DD-212 Multi-Model Consensus Algorithm Not Specified** — named check `code_export_af9_gate` проверяет threshold ≥0.8 fractional; но не документировано: 2-of-3 agreement или average-of-3 или min-of-3; разные interpretations дают разные pass/fail результаты. **Новый паттерн: Multi-Model Code Export Consensus** — Claude + GPT + Gemini параллельно; 2/3 consensus ≥0.8 fraction; retry с higher temperature до 3 циклов; единственный flow где code export quality = multi-model vote, не single-model score. |
| FLOW-24 | **T-Range Header Error** — header: T367–T388 (22 task types); canonical: T367–T374 (8 task types); CHECK 0 в infrastructure gate проверяет это numerically; риск: 14 «призрачных» task types T375–T388 могут быть случайно assigned другому flow. **DD Number Cross-Version Mismatch** — Grok analysis ссылается на "DD-182 Graph-RAG Neo4j" (из более раннего session state); реальные DD-числа FLOW-24 = DD-219–DD-228; reader может искать несуществующий DD-182. **Infrastructure Gaps M5/M6 — New Gap Series** — M5 (executionModel: "hybrid_sync_async" в generic-node-executor.ts) и M6 (FREEDOM_GATED SKIP_SUCCESS в decompose.handler.ts) требуют engine changes ДО FLOW-24; они в M-series (не в numeric gap 77–96 series); разные gap series = риск что один из них пропустят. **SafetyGateToken Generation Difficulty** — predicted score 0.40–0.58 cycle 1; expected 3 cycles; нет documented fallback если cycle 3 fails (план не указывает что делать при persistent generation failure). **Новый паттерн: SafetyGateToken Protocol** — F1002 производит cryptographic token; F1003 ТРЕБУЕТ его как type-level argument; impossible to skip at compile time; первый flow где safety gate = type constraint, не runtime check. **Новый паттерн: FREEDOM-Gated Skip** — missing domain pack config = task SKIPPED (step outcome SKIP_SUCCESS), не FAILED; первый flow с «skip, not fail» step outcome; требует engine support (M6). |
| FLOW-25 | Zone name swap — имена зон C и D переставлены в SESSION_STATE vs ENGINE_ARCHITECTURE |
| FLOW-25 | **Circular Self-Governance** — FLOW-25 BFA engine управляет changes к BFA rules; но сам FLOW-25 содержит 37 CF rules которые сам же должен governing; bootstrapping: если FLOW-25 ещё не ACTIVE, кто governing его собственные BFA rules? Нет явного исключения или ordering rule. **FORCE_PROCEED Permission not in CF Rule** — FORCE_PROCEED требует super-admin permission + min 50-char rationale (оба MACHINE); permission check = text requirement, нет explicit CF rule с violation detection pattern. **ArbiterRegistry ES Migration** — ARBITER-EXTENSION-PROTOCOL.md описывает migration in-memory→ES (xiigen-arbiters index) в Phase A; это architectural change к engine registry; нет rollback plan если ES migration fails mid-phase. **Новый паттерн: 8-State ArbitrationStateMachine** — IDLE→EXTRACTING→DETECTING→SEVERITY_AGGREGATING→PENDING_RESOLUTION|SKIP_ARBITRATION→APPLYING_RESOLUTION→RESOLVED|REJECTED|ERROR; persist-before-emit на КАЖДОМ переходе (score-0); первый state machine в flows с ветвящимися resolution paths. |
| FLOW-26 | **BFA Rule Count Discrepancy** — master table: 22 rules; section header count: другое число; authoritative = master table. **CORE = Human MACHINE — Text-only Constraint Risk** — COREHumanApprovalGate документирован как "cannot be bypassed by config"; нет explicit CF rule с violationCheck pattern; если только текст плана → риск bypass без detection. **FLOW-26↔FLOW-27 Mutual Dependency** — FLOW-26 использует FLOW-27 HumanInteractionGate (COREHumanApprovalGate, task type 22); FLOW-27 зависит от FLOW-26 как prerequisite; mutual dependency требует осторожного ordering при первой установке (bootstrap sequence). |
| FLOW-27 | **Multi-Artifact Differential Session-Isolation** — factory offset +28 (1 флоу = FLOW-26), DD offset +132 (≈10 флоу), DR offset +99 (≈8 флоу). Разные артефакты в одном input-пакете взяты из документов с совершенно разными историческими базелайнами: factory spec — актуальный (post-FLOW-25), DD/DR — копия из post-FLOW-10 документа. Merge plan явно описывает: "27-input authored at pre-self-developing-meta-flow baseline (post publishing flow numbering)". Renumbering выполнен корректно. Cross-validation 33/33 PASS. **Total Alias Substitution** сохраняется (тот же паттерн что FLOW-26). |
| FLOW-28 | **Multi-Artifact Differential Session-Isolation (нарастающий)** — factory offset +54 (2 флоу: FLOW-26+27 = 28+26), DD offset +142, DR offset +106. Паттерн DD/DR из старого базелайна сохраняется — одни авторы или один шаблонный документ. Renumbering выполнен корректно, ALL_PASS. Новый тип входных данных: **Тип F — внешний research report** (2 файла по мультитенантности поданы как отдельные .md, не в input.zip) — дополнительный контекст к implementation pack. |
| FLOW-29 | **Multi-Artifact Differential Session-Isolation (нормализация DD/DR)** — factory offset +101 (3 флоу = 28+26+47). DD offset +39, DR offset +29 — значительно меньше чем FLOW-27/28; авторы начали брать более свежий DD/DR базелайн. Тренд: factory offset растёт предсказуемо (+28, +54, +101), DD/DR offsets нормализуются. 72 фабрики — рекордный одиночный пакет. MERGED_COMPLETE. |
| FLOW-30 | **Reverse Session-Isolation для фабрик** — factory offset -27 (input assumed F1275, actual F1248; input опережал реальность). **Bidirectional Differential** внутри одного пакета: factories -27 vs stress tests +50 — разные секции взяты из документов в ПРОТИВОПОЛОЖНЫХ направлениях смещения. Первый случай отрицательного factory offset за всю историю флоу. Multiple validation rounds (result files 1/2/3) = stale headers и числовые исправления по ходу. Final: 6/6 ✅. |
| FLOW-26 | **Session-Isolation Renumbering (исправлен в merge plan)** — input files drafted at post-FLOW-22 baseline (F982). Canonical baseline at merge start = post-FLOW-25 (F1074). Offset: +93 factories, +14 families, +20 DRs. Merge plan явно документирует offset и выполняет renumbering — паттерн ИСПРАВЛЕН. **Total Alias Substitution в SESSION_STATE** — все T-номера → «a task type», все CF-номера → «an iron rule», все SK-номера → «a planning skill» в canonical STATE файле; только DD и DR числа сохранены. Severity выше чем обычная Document Alias Substitution — затронут canonical STATE, не только planning docs. **Incomplete Merge Packaging** — P6 (MASTER_EXECUTION_PLAN) и P7 (SESSION_STATE) не завершены; P8 (Cross-Validation) не запускалась. Выходной пакет упакован с 5/7 файлами — явный pending list в inventoried output. **Phase Ordering Deviation** — P4 и P5 выполнены в обратном порядке (plan: UNIFIED=P4, BFA=P5; execution: BFA=P4, UNIFIED=P5). Без явного объяснения отклонения. |
| FLOW-27 | Missing update — SESSION_STATE вообще не содержал FLOW-27, только pre-FLOW-27 числа |
| FLOW-28 | Stale header — SESSION_STATE обновлён в конце файла, но заголовок и начальный блок остались Post-FLOW-26 (глубина: 1 флоу) |
| FLOW-29 | Stale header — тот же паттерн что FLOW-28, заголовок по-прежнему Post-FLOW-26 (глубина: 3 флоу) |
| FLOW-30 | Cascading Stale Header — заголовок всё ещё Post-FLOW-26 (FLOW-26 = "the self-developing meta-flow"), FLOW-27/28/29/30 добавились без обновления (глубина: 4 флоу). Первый раз потребовался dedicated consolidation pass по всем 7 файлам |
| FLOW-31 | SESSION_STATE stale header ИСПРАВЛЕН после consolidation pass (глубина 0). **Session-Isolation Renumbering** — input files assumed baseline F1075 вместо реального F1271 (offset +196); 5 флоу пропущены (FLOW-26+27+28+29+30 = 28+26+47+72+23). **File Naming Cross-Session Confusion** — файлы «31 VALIDATION_REPORT.md», «31 CONSOLIDATION_REPORT.md», «31 FLOW EXECUTION_PLAN.md» являются консолидационными документами FLOW-30 (число «31» = номер pass сессии, не номер FLOW); реальные FLOW-31 документы: FLOW31_VALIDATION_REPORT.md, FLOW31_MERGE_MASTER_PLAN.md. **PHANTOM Archetype Bug** — ContractArchetype.MAPPING перечислен в оригинальном reference plan, никогда не добавлялся в enum (дублировал MODULE); удалён в corrected versions (1)(2)(3). **Test Baseline Conflict** — оригинальный plan: ≥3,513; корректный (post FLOW-26→30): ≥3,696; исправлен в Addendum. **BFA CF Range Collision** (FLOW-26/27) — 14 перекрывающихся CF rules в input; устранено renumbering pass. ALL CHECKS PASSED; 109,547 строк. |
| FLOW-32 | **Нет Session-Isolation Renumbering Bug** — стартовал с правильного F1339 (post-FLOW-31 canonical = F1338+1); сессия знала актуальный STATE. **Triple Multi-Vector Input** — первый флоу с тремя независимыми векторами в одном пакете: (1) domain flow delta (Sharable Flows marketplace engineering); (2) meta-system upgrade docs (V32 Enhanced Plan, SESSION_RAG_INDEX); (3) go-to-market research (MARKETPLACE-*.md — внешние платформы Salesforce/HubSpot/monday.com). Вектор 3 = бизнес-стратегия XIIGen как продукта, НЕ engineering delta; принять его за архитектурное содержание = описать неправильный FLOW. FLOW-32 добавляет внутренний template marketplace (F1339–F1418); внешние платформы описываются только в business research vector. |
| FLOW-33 | Session-Isolation Renumbering в предельной форме: input files assumed post-FLOW-17 (F630, 83 families) — реальный baseline post-FLOW-32 (F1418, 210 families). 15 флоу пропущено. Offset +788 factories, +127 families. Новый тип бага: **Merge Plan State Label Mislabel** — в FLOW33_MERGE_MASTER_PLAN.md лейбл "Post FLOW-32" правильный, но числа (F1-F1338) взяты из post-FLOW-31. Следствие: individual factory table показывает F631→F1339 (shift +708) вместо правильного F631→F1419 (shift +788). Ground truth: FLOW33_RENUMBER_MAP.json → `actual_prior_state: "Post FLOW-32 (F1-F1418)"`. Дополнительный баг: **Interface Naming Inconsistency** — 4 из 5 фабрик Family 212 имеют разные имена в ENGINE_ARCHITECTURE vs UNIFIED_SOURCE_INDEX; RENUMBER_MAP.json использует UNIFIED имена как canonical. |
| FLOW-34 | Session-Isolation: тот же post-FLOW-17 baseline (F630), реальный = post-FLOW-33 (F1428). 16 флоу пропущено, offset +798. Merge plan корректен (нет Label Mislabel — FLOW-34 правильно идентифицирует prior state). **Design Approach Inconsistency** (эволюция Interface Naming бага): Family 218 в ENGINE = generic adapters (IDataProcessResultAdapterService), в UNIFIED = per-language scaffolds (INodeSdkScaffoldService); это разные архитектурные подходы, не просто разные имена. Масштаб: 4 семьи (~20 фабрик). RENUMBER_MAP выбирает ENGINE-имена (противоположно FLOW-33 где выбрал UNIFIED). **Cross-Flow Compatibility Table с незаполненным baseline** — UNIFIED таблица проверяет только FLOW-01-17, пропускает FLOW-18–33. **Contradictory F641 NOTE**: "pre-F630 F641" — оксюморон (F641 > F630). |
| FLOW-35 | **Нет Session-Isolation Renumbering Bug** — план стартовал с правильного F1484 (post-FLOW-34 = F1483+1). Первый флоу в серии FLOW-33/34/35 без сессионной изоляции — написан в сессии знавшей актуальный canonical state. **Test Baseline Mismatch** — COMPREHENSIVE_PLAN: "~1,092 existing + ~105 net new = ~1,197 total"; факт (STATE_P5): прогрессия 668→842, итог 842 (+174 новых). Расхождение: план, вероятно, считал все-слои (server+RN+integration), STATE-файлы — только server suite; или baseline-оценка взята из устаревшего документа. **Family Assignment Discrepancy** — в таблице артефактов плана F1487-F1488 (ConfigBuilder, GrokProvider) неявно отнесены к Family 223; STATE_P5.json явно: Family 224 = F1487-F1490. Canonical = STATE_P5. **SDK Hygiene Gap (absent BFA rule)** — FLOW-35 исправляет два SDK-нарушения из FLOW-33 (мёртвый `google-generativeai`, provider-leaking OpenAI-обёртка для Grok), но не добавляет CF-правила предотвращающего аналогичные violations в будущих флоу. CF-789/CF-790 покрывают только secrets. **InMemorySecretsProvider без Factory ID** — план описывает 3 провайдера (AWS F1485, EnvVar F1486, InMemory), но InMemory существует только как файл без F-номера (test-only helper); несовместимо с паттерном регистрации других test-провайдеров. |
| FLOW-37 | **Premature Manual Implementation Bug (v1/v2 → revised)**: v1 и v2 объявляли `implementationMode: "manual"` для ВСЕХ фаз с обоснованием «circular dependency». Конкретное нарушение v2: создавал `CapabilityGapRepository.ts`, `TraceFailureAnalyzer.ts` как TypeScript-сервисы напрямую — обходит AF-pipeline (no judgment, no learning, PromptOps не может патчить). Circular dependency justification верна ТОЛЬКО для Phase A bootstrap, но ошибочно расширена на все фазы. Revised (2026-03-24): `implementationMode: "hybrid"`, `bootstrapBoundary: "Phase A only"`, `afPipelineStartsAt: "Phase B"`. **Artifact Ranges Not Pre-Allocated** — STATE.json содержит «⛔ REQUIRED — Run INFRA-audit 1» вместо реальных F/T/CF/SK диапазонов; FLOW-37 разрабатывался без загрузки canonical STATE. **Новый тип флоу: Engine Self-Awareness / Pre-Phase Gate** — движок добавляет инфраструктуру самодиагностики (capability-gate, trace analyzer, skill lifecycle); все capabilities = flow topologies, не TypeScript; bootstrap boundary = Architecture Decision 2. |
| FLOW-38 | **Premature Manual Implementation Bug (v2 → revised, тот же паттерн что FLOW-37)**: v2 объявляет `implementationMode: "manual"` для ВСЕГО FLOW-38 с обоснованием «circular dependency — AF pipeline cannot build the systems that improve AF pipeline quality». Конкретные нарушения v2: создаёт 6 TypeScript сервисов напрямую: `RagQualityEvolver.ts`, `DpoToRagPromoter.ts`, `CalibrationMemory.ts`, `PromptEvolutionStore.ts`, `KnownFixSeeder.ts`, `SkillEvolutionTrigger.ts`. Ключевое следствие: `DpoToRagPromoter.ts` использует `buildPatternContent(teachingPoint, conflictType)` — static template string; теги PROVEN_SOLUTION паттернов никогда не улучшаются; паттерны не находятся через retrieval → Loop 2 показывает NEVER_RETRIEVED. False Circular Dependency: justification верна только для Phase A, расширена на все фазы ошибочно. Revised (2026-03-24): `implementationMode: "hybrid"`, Phase A = только 2 ES indices + 5 FLOW_DESIGN patterns + event schemas, Phases B–F = AF-pipeline для 7 loop topologies. **Thresholds не в FREEDOM Config (v2)**: `private readonly DPO_THRESHOLD = 3` в `DpoToRagPromoter.ts`; `private readonly UPDATE_THRESHOLD = 3` в `SkillEvolutionTrigger.ts`; изменение требует recompile + redeploy; нельзя настроить per-tenant. Revised: все пороги в `flow38_learning_thresholds` FREEDOM config. **Loop 7 отсутствует в v2**: v2 описывает 6 петель, revised добавляет Loop 7 (`flow-design-learning` topology) — классификация PROMPT_FLAW / RAG_FLAW / DESIGN_FLAW; после 3 похожих DESIGN_FLAW создаёт новый FLOW_DESIGN паттерн. **Family Name Discrepancy**: v2 `"learning-loop-services"` (TypeScript-centric), revised `"learning-loop-flows"` (topology-centric). **Artifact Ranges Not Pre-Allocated**: `⛔ REQUIRED` вместо реальных чисел в обоих документах. **Новый тип флоу: Learning Loop Completion** — движок получает память между flows; 6+1 постоянно работающих топологий; каждый loop = flow topology (не TypeScript-сервис); петли сами обучаются через тот же механизм который реализуют. |
| FLOW-39 | **Document Alias Substitution (тот же паттерн что FLOW-36, полная замена)**: весь документ использует псевдонимы вместо технических ID: «the local model training flow»=FLOW-39, «the learning loop flow»=FLOW-38, «the multi-stack porting flow»=FLOW-37, «the user registration flow»=FLOW-40, «the friend request flow»=FLOW-44, «the skill injection station»=AF-4, «the code generation station»=AF-1, «the quality judge station»=AF-9, «the prompt operations station»=AF-11, «the preference learning signal triple»=DPO triple, «the retrieval system»=RAG, «the programming interface»=API, «the search index»=Elasticsearch. Кросс-референция с canonical документами требует таблицы перевода. **Artifact Ranges Not Pre-Allocated**: `⛔ REQUIRED — Run INFRA-audit 1` для T (6 task types), F (6 factories), CF (6 BFA rules), SK (1 skill), Family (1). **Economic Assumption Unverified**: утверждение «40 curriculum-ordered = 80 random в качестве» — pedagogical assumption, не empirically verified; риск: fine-tuning threshold 40 может оказаться недостаточным для некоторых archetypes. **Curriculum Tier Formula Hardcoding Risk**: flowC (curriculum-sequencer) tier boundaries определены inline в topology contract JSON (не только в FREEDOM config) — исправление требует re-injection топологии, а не просто изменения config. **Phase G as second manual phase**: документ декларирует `bootstrapBoundary: "Phase A only"`, но Phase G содержит manual работу (обновление `flow-implementation-guide-SKILL.md`); технически допустимо (skill file update, не TypeScript), но нетипично для hybrid pattern. **Chain Dependency Risk**: FLOW-39 Phase A предполагает 10 FLOW_DESIGN паттернов (5 из FLOW-37 + 5 из FLOW-38); если FLOW-37/38 Phase A неполные → FLOW-39 Phase A упадёт на pre-check. **Новый тип флоу: Flow Design Teaching / Local Model Curriculum** — движок учится проектировать свои own flow topologies (flow-design skills в RAG); local model получает curriculum-ordered обучение; V9 gate как mandatory teaching health check; shadow runs с нуля API стоимости; fine-tuning порог 40 → более ранний switch. |
| FLOW-36 | **Нет Session-Isolation Renumbering Bug для фабрик** — F1491/Family 225 совпадает с FLOW-35 canonical next. **Partial CRITICAL CORRECTION** — документ REQUIREMENTS-SUMMARY явно объявляет "CRITICAL CORRECTION" (переименовывает task types и BFA rules в безопасные диапазоны), но пропускает design records: указывает DR-240 как start, реальный FLOW-35 canonical next = DR-269. SELF-IMPLEMENTATION-PLAN ещё хуже: stress test start = 432 (коллизия! ST-499 уже занят), design record start = DR-243 (также занят). Объявленная коррекция создаёт ложную уверенность. **Phase Numbering Inconsistency** — SELF-IMPLEMENTATION-PLAN использует P0/P1/P2/P3/P4 (5 фаз). EXECUTION-PLAN использует A/B/C/D (4 фазы). Разные схемы нумерации для одного флоу, разные границы фаз, нет однозначного mapping. **Document Alias Substitution** — все технические идентификаторы заменены описательными псевдонимами: «the retrieval system» = RAG, «the search index» = Elasticsearch, «the feature registry flow» = FLOW-36, «the code generation station» = AF-1, «the meta-arbitration flow» = FLOW-35 (не совпадает с canonical name «Secrets Fabric + ConfigBuilder + Grok Provider»). Кросс-референция с каноническими документами невозможна без таблицы перевода. **Multi-Stack Document Mismatch** — ENGINE_ARCHITECTURE_MERGED.md описывает Python 3.12 + FastAPI; CLAUDE.md (в контексте FLOW-36) описывает NestJS 10 + TypeScript 5; CLAUDE.md проекта (актуальный) описывает .NET 9 / C# 13. Три разных стека в документах одного проекта. Test baselines не сравнимы: FLOW-35 STATE_P5 = 842 server tests (Python), FLOW-36 = 2,342 server tests (NestJS). |
| FLOW-40 | **Artifact Ranges Not Pre-Allocated**: canonical spec и v7 план используют `⛔ REQUIRED — Run INFRA-audit` вместо реальных числовых диапазонов для T/F/CF/Family; системный паттерн — документы пишутся без загрузки canonical STATE. **FlowEventBridge Archetype Ambiguity**: FlowEventBridge мог быть классифицирован как PROCESSING (он «обрабатывает» события); правильно DATA_PIPELINE — граница: DATA_PIPELINE = move + format, PROCESSING = compute + transform; FlowEventBridge перемещает и форматирует события между очередью и SSE-потоком, не вычисляет их. **Semantic Slug Enforcement (Rule 16) нарушено в v7**: canonical spec использует только `client-push` в code paths (правильно); v7 план использует `server/test/flow40/` с числом в тест-директориях (нарушение Rule 16, должно быть `server/test/client-push/`). **Phase A Bootstrap Boundary**: hybrid pattern устоявшийся (FLOW-37/38/39 precedent), Phase A manual (Wave -1 bootstrap — Infrastructure fabric), Phases B–D через AF-pipeline; для Wave -1 возникает вопрос «нужен ли AF-pipeline вообще» — ответ да, B–D не circular. **Новый тип флоу: Wave -1 Infrastructure / Client Push** — единый SSE-механизм для всех waiting-step flows; ISseConnectionPool fabric interface (8-я фабрика); InMemorySseConnectionPool default + Redis-backed swap; 3 task types (ROUTING/DATA_PIPELINE/SCHEDULED); Phase A done (commit a4cf6e7); B–D ждут user-registration ACTIVE. |
| FLOW-42 | **H19 Wrong CURRENT-STATE Attribution**: `CURRENT-STATE.json` содержит slug `rag-quality-graph` вместо правильного `miro-shape-heuristics`; 2 slug pair с некорректными ссылками; требует исправления в Phase B. **T-number Potential Conflict (FLOW-41 vs FLOW-42)**: FLOW-42 использует T584–T587 (по examination), FLOW-41 R1-STATE-INIT использует T587=CanvaTextReadAdapter + T588=CanvaTextWriteAndUI — T587 используется в обоих flows; если T-numbers глобальные → конфликт pre-allocation; EXTERNAL_REPO независимое пространство требует явной документации. **GAP-ARCH-01 системный (повторяется из FLOW-41)**: ADAPTATION archetype отсутствует в enum для обоих flows; одно исправление (~5 строк) разблокирует весь FLOW-34 fleet. **GAP-INFRA-01 системный (повторяется)**: `xiigen-decision-graph` + `xiigen-planning-decisions` fixtures отсутствуют; блокирует ALL flows с planning layer (100 строк JSON = разблокировать всё). **Implementation Status vs Planning Completeness Mismatch**: H20=PLANNING_COMPLETE, CHAIN_REVIEW=READY_FOR_EXECUTION — но пакет `adapters/miro/` не создан (H18=NOT_PACKAGED); FLOW-42 = самый детально спланированный неначатый flow. **FREEDOM Surface Validation Gap**: 10 configurable confidence thresholds (r1..r10) — без explicit bounds validation tenant может выставить threshold=0 (всё классифицируется как одна категория); bounds validation не документирована. **Новый тип флоу: Platform Adaptation with Heuristic Reasoning** — ADAPTATION archetype + spatial analysis + 10 tunable heuristics; FREEDOM surface 11 ключей (самая широкая в FLOW-34 fleet); 5 GAP-sessions (R1–R5) как обязательный pre-phase; standalone test strategy (не stub в server/); 16 arbiters (2x сложнее FT-C5). |
| FLOW-41 | **GAP-ARCH-01 — ADAPTATION archetype отсутствует в enum (blocking)**: ContractArchetype enum содержит 29+ archetypes без ADAPTATION; `taskTypeArchetypes` в STATE.json использует строку вне type system; `feedback.handler.ts` tierMap defaults к 3 вместо правильного 2 (ADAPTATION сравним по сложности с DATA_PIPELINE); нужно добавить в `archetypes.ts` + `feedback.handler.ts`; ~5 строк в 2 файлах. **GAP-INFRA-01 — fixture files отсутствуют (blocking для graph seeding)**: `xiigen-decision-graph` fixtures: 0 файлов; `xiigen-planning-decisions` fixtures: 0 файлов; это системный gap — блокирует ВСЕ flows с planning layer, не только FLOW-41; PC-1 gate ловит автоматически. **J1 — hardcoded STRUCTURAL check перед graph query (carry-forward)**: `bootstrap-cycle-router.ts` lines 42–53 hardcoded `if (params.score < 0.50) return STOP_STRUCTURAL` до graph query (line 63); любой archetype-specific override edge игнорируется при score < 0.50; системная проблема всех flows. **K1 systemic — 50 .code subfield refs в старых сессиях**: DpoValidationInput использует `modelComparison.chosen.model` (правильно), но 50 старых session files ссылаются на `chosen.code` (stale); FLOW-41 чист. **Event model adaptation**: C5 — standalone Canva App, не CloudEvents; event model CALLBACK (`@canva/design` onSelectionChange); Addition 16 требует адаптации (Canva SDK callback names, не CloudEvent names). **Shared ecosystem blocker**: @xiigen/plugin-sdk (Layer 1) не опубликован в npm — блокирует публикацию всей FLOW-34 экосистемы адаптеров (Figma + Canva + будущие). **Новый тип флоу: Platform Adaptation / Ecosystem Adapter** — EXTERNAL_REPO; ADAPTATION archetype (новый); 3-layer architecture (Layer 2 = CSS engine неизменен, Layer 3 = canva-adapter.ts единственное отличие); Mock Writer Injection pattern; C5-E equivalence proofs; npm package ready (H-18 ALREADY_PACKAGED, H-11 READY, H-5 CONSISTENT — 2nd in corpus after FLOW-34); 80+ session files. |
| ADAPTER-W1 | **Diff View Algorithm Not Documented**: W1 — единственный адаптер с Diff View (Generated CSS vs Native Webflow CSS); алгоритм определения «несовпадения» не задокументирован (строчное сравнение? нормализация единиц? числовая близость ±1px?); должен быть MACHINE decision ДО начала Сессии 4 (UI-сессия). **figma-types.d.ts Scope Mismatch**: C5 использует text shims (~20 строк); W1 требует ПОЛНЫЙ figma-types.d.ts (все layout-типы SceneNode); при копировании C5-подхода → layout-свойства дают тихие ошибки типов (компилятор не падает); Сессия 2 checklist должен явно различать scopes. **API Staleness Risk**: план написан до старта (current_session: 0, AWAITING SESSION-0 APPROVAL); Webflow Designer API может измениться; FC-12 validation — mandatory gate ПЕРЕД каждой сессией, не однократно при написании плана. **Silent Property Gap в roundtrip**: buildFigmaLikeNode() без явного маппинга свойства → свойство тихо отсутствует в generated CSS без ошибки; coverage gap обнаруживается только в Сессии 5 roundtrip-тестах; если Сессия 5 неполна → конвейер «тихо» теряет часть из 22+ свойств. **Новый паттерн: Diff View as Pipeline Validation Gate** — W1 уникален: native Webflow CSS = ground truth для конвейера FigmaToCode; Diff View превращает адаптер из «конвертора» в «валидатор точности конвейера»; первый адаптер с authoritative reference для сравнения (C5/M1 не имеют equivalent ground truth). |

**Алгоритм измерения глубины стейла:**
```
1. Найти заголовок системного блока → «Post FLOW-N»  → N_header
2. Найти последний FLOW в FLOW REGISTRY → N_last
3. Глубина = N_last - N_header
4. Глубина 0 = нет бага
5. Глубина 1 = одиночный stale header (FLOW-28 pattern)
6. Глубина ≥2 = cascading stale header — requires consolidation pass
```

Вывод: SESSION_STATE как append-only документ накапливает drift. Всегда читать самый поздний блок. Измерять глубину, не только факт стейла.

### Session-Isolation Renumbering Bug (новый тип бага, FLOW-31)

Если в input-файлах F/T/SK-номера НЕ совпадают с ожидаемыми от canonical state:

```
1. Взять input start: input_F_start = первый F-номер в input delta
2. Взять canonical end: canonical_F_end = последний F-номер в canonical STATE
3. Ожидаемый старт = canonical_F_end + 1
4. Если input_F_start ≠ ожидаемый старт → Session-Isolation Offset = ожидаемый - input_start
5. Симптом: input files генерировались в изолированной сессии без загрузки актуального STATE
6. Fix: Phase 1 renumbering script ПЕРЕД merge (паттерн FLOW31_MERGE_MASTER_PLAN)
```

**FLOW-31 пример:** input assumed F1075 (после the tenant lifecycle flow = 1074), реальный canonical baseline = F1271 (после FLOW-30 добавил F1248–F1270). Offset = +196 factories, +100 task types, +37 families и т.д. Потребовался полный renumbering pass всех 7 delta-файлов перед merge.

Этот баг **не виден в SESSION_STATE** — он в input файлах. Алгоритм: прочитать SESSION_STATE → найти canonical end numbers → сравнить с input start numbers → если расхождение → renumbering pass обязателен.

**FLOW-33 пример (предельная форма):** input assumed post-FLOW-17 (F630, 83 families) — canonical перед FLOW-33 = post-FLOW-32 (F1418, 210 families). 15 флоу пропущено. Offset: +788 factories, +127 families, +289 task types, +70 templates, +444 BFA rules, +291 stress tests. Fix: FLOW33_RENUMBER_MAP.json — авторитетное отображение, не таблицы merge plan.

**Правило масштаба gap:** сдвиг >500 по фабрикам = признак что несколько флоу полностью пропущены как baseline. Проверить RENUMBER_MAP.json явно перед принятием любых F-номеров из input.

### Merge Plan State Label Mislabel Bug (новый тип бага, FLOW-33)

Признак: лейбл "system_state_before" в merge plan называет правильный флоу (напр. "Post FLOW-32"), но числа внутри — из предыдущего флоу (post-FLOW-31). Следствие: individual factory renumber table в том же документе вычислен от неверного baseline → неверные canonical F-номера.

```
Алгоритм детекции:
1. Взять числа из system_state_before в merge plan → planned_end_F
2. Взять числа из RENUMBER_MAP.json → actual_prior_F
3. Если planned_end_F ≠ actual_prior_F → Label Mislabel
4. Проверить individual factory renumber table: F_old + (actual_prior_F - planned_prior_F) = истинный canonical
5. Ground truth всегда: RENUMBER_MAP.json > merge plan tables

Симптом в factory table:
  merge plan: F631 → F1339 (+708)
  canonical:  F631 → F1419 (+788)
  разница: actual_prior_F (F1418) - planned_end_F (F1338) = 80 = ровно один флоу
```

Fix: считать числа из RENUMBER_MAP.json как абсолютный источник правды. Merge plan tables — только ориентировочны.

### Interface Naming Inconsistency Bug (новый тип бага, FLOW-33)

Один и тот же factory может иметь **разные имена интерфейса** в ENGINE_ARCHITECTURE vs UNIFIED_SOURCE_INDEX vs RENUMBER_MAP.json внутри одного пакета. Это происходит когда input delta-файлы генерировались в разных сессиях или итерациях.

```
Алгоритм детекции:
1. Выписать имена интерфейсов из ENGINE_ARCHITECTURE для каждого F-номера
2. Выписать имена из UNIFIED_SOURCE_INDEX для тех же F-номеров
3. Сравнить построчно — любое несовпадение = Interface Naming Inconsistency
4. Проверить RENUMBER_MAP.json — он содержит canonical имена

Порядок доверия для имён интерфейсов:
  RENUMBER_MAP.json > UNIFIED_SOURCE_INDEX > ENGINE_ARCHITECTURE

FLOW-33 пример (Family 212 / F1425-F1428):
  ENGINE:  IArbitrationEngine, IContextPackBuilder, IRegressionImpactAnalyzer, IPromptPackService
  UNIFIED: IMultiArbiterService, IContextPackService, IRegressionImpactService, IPromptEvolutionService
  Canonical (MAP): IMultiArbiterService, IContextPackService, IRegressionImpactService, IPromptEvolutionService
```

Симптом: ENGINE_ARCHITECTURE содержит более «технические» имена (IArbitrationEngine), UNIFIED содержит более «семантические» (IMultiArbiterService). Обычно UNIFIED ближе к canonical — он пишется позже.

**FLOW-34 расширение: Design Approach Inconsistency (более глубокий тип):**

Если ENGINE и UNIFIED расходятся не только в именах но и в **структуре** — это Design Approach Inconsistency. Признак: одна семья в ENGINE проектирует N generic-сервисов, та же семья в UNIFIED проектирует N language/platform-specific-сервисов.

```
FLOW-34 пример (Family 218 / F1460-F1465):
  ENGINE: generic adapters
    F1460 = IMicroserviceSdkGeneratorService
    F1461 = IDataProcessResultAdapterService
    F1462 = IDynamicRoutingAdapterService
  UNIFIED: per-language scaffolds
    F1460 = INodeSdkScaffoldService
    F1461 = IGoSdkScaffoldService
    F1462 = IJavaSdkScaffoldService

RENUMBER_MAP canonical (F1460): IMicroserviceSdkGeneratorService  ← следует ENGINE

Вывод: RENUMBER_MAP не всегда выбирает UNIFIED. Порядок доверия:
  RENUMBER_MAP.json >> всё остальное
  (MAP может выбрать ENGINE в одном FLOW, UNIFIED в другом — проверять в каждом случае)
```

Когда масштаб расходится (4+ семьи, 20+ фабрик) — Design Approach Inconsistency означает что два source документа писались независимо и представляют разные design iterations. Canonical = то что RENUMBER_MAP зафиксировал.

**FLOW-34 дополнительный баг: Cross-Flow Compatibility Table с незаполненным baseline:**

Если compatibility table в input документах проверяет только FLOW-01 → FLOW-K (где K = baseline при написании документа), но реальный canonical имеет FLOW-K+1 → FLOW-N — таблица пропускает все конфликты с этими флоу.

```
Детекция:
1. Найти последнее FLOW-N в compatibility table
2. Сравнить с реальным предшественником из RENUMBER_MAP.json
3. Если N < реальный predecessor → таблица неполная на (predecessor - N) флоу
FLOW-34 пример: table checked FLOW-01–17 only; actual prior = FLOW-33; 16 flows missing
```

Это не критичный bug — merge plan всегда должен быть проверен независимо. Но фиксировать как "missing compatibility coverage" в секции bugs.

### Partial CRITICAL CORRECTION Bug (новый тип бага, FLOW-36)

Признак: документ явно объявляет «CRITICAL CORRECTION» или «renumbered to safe ranges» — но перечисляет только НЕКОТОРЫЕ типы артефактов, оставляя остальные в состоянии коллизии.

```
Алгоритм детекции:
1. Найти в документе явный «CRITICAL CORRECTION» или «renumbered» блок
2. Выписать артефакты которые были исправлены (напр. task types, BFA rules)
3. Выписать ВСЕ типы артефактов флоу (factories, families, task types, templates,
   BFA rules, stress tests, skills, DDs, DRs, EPs)
4. Для каждого типа НЕ упомянутого в correction: проверить числа относительно canonical STATE
5. Если любой непроверенный тип имеет коллизию → Partial CRITICAL CORRECTION Bug

FLOW-36 пример:
  Исправлено: task types (T567+), BFA rules (CF-791+)
  Пропущено:  design records — указан DR-240 start, canonical next = DR-269 → коллизия DR-240..268
              stress tests в SELF-IMPL-PLAN — указан ST-432 start, ST-499 уже занят → коллизия
```

Эффект: объявленная коррекция создаёт ложную уверенность — читатель предполагает что «всё исправлено», но часть артефактов остаётся в коллизионном диапазоне.

### Document Alias Substitution Bug (новый тип бага, FLOW-36)

Признак: технические идентификаторы (имена AF-станций, имена флоу, CF/SK/T-номера, имена сервисов) систематически заменены описательными псевдонимами по всему документу.

```
Алгоритм детекции:
1. Найти в документе фразы типа «the retrieval system», «the search index», «the code generation station»
2. Создать таблицу перевода: alias → canonical identifier
3. Проверить что все числа в alias-форме совпадают с canonical STATE
4. Проверить что alias-имена флоу совпадают с canonical именами

FLOW-36 примеры:
  «the retrieval system» = RAG
  «the search index» = Elasticsearch
  «the feature registry flow» = FLOW-36
  «the meta-arbitration flow» = FLOW-35 (canonical: «Secrets Fabric + ConfigBuilder + Grok Provider»)
  «the code generation station» = AF-1 Genesis
  «the quality judge station» = AF-9 Judge
  «a task type» = T567+ (число не указано)
  «an iron rule» = CF-791+ (число не указано)
```

Последствия:
- Кросс-референция с каноническими документами невозможна без таблицы перевода
- Числа могут дрейфовать между документами одного флоу
- Имена флоу-предшественников могут не совпадать с canonical именами
- Placeholder-форма ("a task type") скрывает что числа вообще не назначены

### Phase Numbering Inconsistency Bug (новый тип бага, FLOW-36)

Признак: один и тот же флоу описан в двух или более документах с разными схемами нумерации фаз.

```
Алгоритм детекции:
1. Найти все документы в input-пакете
2. Для каждого выписать схему нумерации: P0/P1/P2... или A/B/C/D или 1/2/3...
3. Сравнить границы фаз: одинаковые ли deliverables в каждой паре?
4. Если схемы разные → Phase Numbering Inconsistency

FLOW-36 пример:
  SELF-IMPLEMENTATION-PLAN: P0(INJECT) → P1(GENERATE) → P2(JUDGE) → P3(IMPROVE) → P4(LEARN)
  EXECUTION-PLAN: A(Wire Contracts) → B(Engine Generates + RAG Seeding) → C(Benchmark Runs) → D(Validate)
  Разные границы фаз: P0+P1 ≈ A+B, но P4(LEARN) в EXECUTION-PLAN нет отдельной фазы
```

Как читать: использовать EXECUTION-PLAN как source-of-truth для последовательности шагов (он детальнее), SELF-IMPLEMENTATION-PLAN как source-of-truth для концептуальных фаз (он объясняет почему).

### Multi-Artifact Differential Session-Isolation Bug (новый тип бага, FLOW-27)

Признак: разные артефакты в ОДНОМ input-пакете имеют РАЗНЫЕ смещения относительно canonical STATE. Это означает что разные секции input-документов были взяты из исторически разных базелайнов.

```
Алгоритм детекции:
1. Взять canonical STATE числа для каждого типа артефакта
2. Взять input start для каждого типа: F, T, CF, SK, DD, DR, template, stress test
3. Вычислить offset для каждого типа: canonical - input_start
4. Если offsets значительно РАЗНЫЕ → Multi-Artifact Differential Session-Isolation

FLOW-27 пример:
  Factory offset:  +28   (input F1075, canonical F1103) = 1 флоу drift
  DD offset:      +132   (input DD-130, canonical DD-262) = ≈10 флоу drift
  DR offset:       +99   (input DR-99, canonical DR-198) = ≈8 флоу drift

Диагноз: factory spec был актуализирован (автор знал про FLOW-26),
  но DD/DR были скопированы из старого шаблона без обновления.

Тест: если offset(DD) / offset(Factory) > 3 → высокая вероятность этого бага.
```

Паттерн нарастания по флоу:
- FLOW-27: factory +28, DD +132 (несоответствие 4.7x)
- FLOW-28: factory +54, DD +142 (несоответствие 2.6x)
- FLOW-29: factory +101, DD +39 (нормализация! несоответствие 0.4x — авторы обновили DD базелайн)

Вывод: если видим factory offset маленький + DD/DR offset огромный → DD/DR взяты из старого шаблонного документа.

### Reverse + Bidirectional Session-Isolation Bug (новый тип бага, FLOW-30)

Признак: offset для какого-то типа артефактов отрицательный (input опережает canonical) ИЛИ offset имеет разный знак для разных типов внутри одного пакета.

```
Алгоритм детекции:
1. Для каждого типа артефакта: offset = canonical_start - input_start
2. Если любой offset < 0 → Reverse Session-Isolation для этого типа
3. Если max(offset) и min(offset) имеют РАЗНЫЕ знаки → Bidirectional Differential

FLOW-30 пример:
  Factory:      input F1275, canonical F1248 → offset = -27 (ОТРИЦАТЕЛЬНЫЙ)
  Stress Tests: input ST-340, canonical ST-390 → offset = +50 (положительный)
  Разность знаков → Bidirectional

Интерпретация Reverse (offset < 0):
  Input ожидал БОЛЬШЕ артефактов чем реально существует.
  Возможные причины:
    a) Input взят из будущего состояния (планирование следующих флоу)
    b) Input использует нумерацию из черновика который не был заморожен
    c) Между разработкой input-пакета и merge часть запланированных флоу была отменена
```

Severity: ВЫСОКИЙ — обычный renumbering алгоритм (add offset) не применим при отрицательном смещении. Требует проверки каждого типа независимо. При отрицательном offset — проверить нет ли коллизии с уже существующими артефактами в canonical.

### Incomplete Merge Packaging Bug (новый тип бага, FLOW-26)

Признак: выходной пакет содержит часть canonical файлов с завершённым merge, а остальные — незавершены или отсутствуют. Merge был «запакован» до завершения всех фаз.

```
Алгоритм детекции:
1. Найти inventoried output файл или список выходных файлов
2. Проверить: сколько из 7 canonical файлов имеют статус COMPLETE
3. Если <7 — выписать какие фазы остались pending
4. Проверить: запускалась ли Phase N (Cross-Validation) — если нет, вся валидность pending

FLOW-26 пример:
  COMPLETE (5/7): ENGINE_ARCHITECTURE, TASK_TYPES_CATALOG, SKILLS_FACTORY_RAG,
                  V62_BFA_STRESS_TEST, UNIFIED_SOURCE_INDEX
  PENDING (2/7): MASTER_EXECUTION_PLAN (P6 не выполнена)
                 SESSION_STATE (P7 только next-starts-at — неполное обновление)
  СЛЕДСТВИЕ: P8 Cross-Validation не запускалась → согласованность 5 файлов не подтверждена
```

Последствия:
- 5 COMPLETE файлов могут содержать рассинхроны которые P8 обнаружила бы
- SESSION_STATE не обновлён → canonical next-numbers могут не совпадать с actual state в 5 файлах
- MASTER_EXECUTION_PLAN не обновлён → нет recovery commands для FLOW-26

При обнаружении: отметить в секции «Что поймал протокол» с явным pending list. Canonical числа для следующего FLOW брать из merge plan (POST-FLOW-N NUMBERS секция) или из inventoried output числа файлов.

### Phase Ordering Deviation Bug (новый тип бага, FLOW-26)

Признак: фазы выполнены в другом порядке чем описан в merge plan, без явного объяснения отклонения.

```
Алгоритм детекции:
1. Из merge plan: выписать plan_order = [P1→P2→...→P9]
2. Из inventoried output или session history: выписать actual_order
3. Сравнить каждую позицию
4. Если actual ≠ plan → Phase Ordering Deviation

FLOW-26 пример:
  Plan:    P4 = UNIFIED_SOURCE_INDEX → P5 = V62_BFA_STRESS_TEST
  Actual:  P4 = V62_BFA_STRESS_TEST → P5 = UNIFIED_SOURCE_INDEX
  Нет объяснения почему порядок изменён
```

Severity: обычно LOW если оба файла завершены корректно. Становится HIGH если:
- Более ранняя фаза зависит от данных более поздней (cross-reference corruption)
- Переставлены местами фазы с dependency (напр. SESSION_STATE перед BFA — SESSION_STATE должен быть последним)

### Total Alias Substitution в Canonical STATE (новый тип бага, FLOW-26)

Признак: в canonical SESSION_STATE файле ALL T-номера, CF-номера, SK-номера заменены generic-placeholders. Severity ВЫШЕ чем обычный Document Alias Substitution в planning docs.

```
Симптомы в SESSION_STATE:
  «a task type» вместо T375, T376, T388
  «an iron rule» вместо CF-473, CF-509
  «a planning skill» вместо SK-232, SK-250
  «a task type–a task type» вместо T375–T388
  «stress test 300–stress test 324» — числа сохранены (DD и DR тоже сохранены)

Проверить: есть ли отдельный файл или секция с маппингом placeholder → actual number?
Если нет → T/CF/SK числа для этого FLOW недоступны из STATE файла напрямую.

Источник actual чисел: искать в TASK_TYPES_CATALOG_MERGED (T-числа),
  V62_BFA_STRESS_TEST_MERGED (CF-числа), SKILLS_FACTORY_RAG_MERGED (SK-числа),
  или в merge plan RENUMBERING MAP.
```

Отличие от обычного Document Alias Substitution:
- Обычный DAS: в planning/execution docs; canonical docs содержат числа
- Total Alias Substitution: в самом STATE файле; числа нигде в STATE не доступны

### Premature Manual Implementation Bug (новый тип бага, FLOW-37)

Признак: флоу объявляет `implementationMode: "manual"` или создаёт TypeScript-сервисы напрямую для capabilities которые должны быть flow topologies — ссылаясь на «circular dependency», но эта логика применяется ко ВСЕМ фазам вместо только minimum bootstrap.

```
Алгоритм детекции:
1. Найти в input docs: implementationMode, Phase A steps, созданные файлы
2. Если implementationMode: "manual" для ВСЕХ фаз → проверить обоснование
3. Если обоснование = «circular dependency» → проверить: это верно ТОЛЬКО для bootstrap
4. Проверить Phase A: создаёт ли TypeScript-сервисы с бизнес-логикой?
   Разрешено в Phase A: ES index creation, initial seeding, topology contract JSON, ONE .md skill
   НЕ разрешено: CapabilityGapRepository.ts, TraceFailureAnalyzer.ts, любой сервис с if/else

FLOW-37 пример:
  v2 (неверно): implementationMode: "manual" + создаёт CapabilityGapRepository.ts напрямую
    → обходит AF-pipeline → no judgment, no learning, PromptOps не может патчить
  revised (верно): implementationMode: "hybrid" + bootstrapBoundary: "Phase A only"
    → Phase A = 8 bootstrap шагов (0 TypeScript сервисов)
    → Phases B–F = 7 flow topologies через AF-pipeline

Тест-вопрос: «Может ли этот TypeScript-сервис улучшиться через PromptOps?»
  Если нет → это должна быть flow topology, не TypeScript
```

Последствия:
- Self-awareness capabilities, написанные как TypeScript, накапливают 0 preference triples → не обучаются
- Повторяющийся capability gap не будет обнаружен как паттерн (нет trace analysis)
- При patch PromptOps не может изменить поведение сервиса

Признак revision: если в input-файлах есть несколько версий STATE.json и в новой появляется `"bootstrapBoundary"` + `"afPipelineStartsAt"` → предыдущие версии содержали этот баг.

### File Naming Cross-Session Confusion Bug (новый тип бага, FLOW-31)

Признак: файлы в output-пакете содержат число «N» в имени, но это число относится к номеру консолидационного прохода или сессии, не к номеру FLOW.

```
Алгоритм детекции:
1. Найти файлы с числом в имени: «31 VALIDATION_REPORT.md», «31 CONSOLIDATION_REPORT.md» etc.
2. Проверить СОДЕРЖИМОЕ: какой FLOW упоминается внутри в первых 30 строках?
3. Если содержимое относится к другому FLOW-M (M ≠ числу в имени) → File Naming Cross-Session Confusion
4. Найти «настоящие» файлы по паттерну: FLOW{N}_VALIDATION_REPORT.md / FLOW{N}_MERGE_MASTER_PLAN.md
   (паттерн с подчёркиванием и явным FLOW-префиксом = canonical naming)

FLOW-31 пример:
  «31 VALIDATION_REPORT.md»    — содержит данные FLOW-30 consolidation (tenant lifecycle flow)
  «31 CONSOLIDATION_REPORT.md» — содержит stale header fixes для FLOW-30
  «31 FLOW EXECUTION_PLAN.md»  — содержит recovery instructions: «следующий FLOW-31 стартует с F1271»
  
  Реальные FLOW-31 документы:
  FLOW31_VALIDATION_REPORT.md — ALL CHECKS PASSED, 109,547 строк, post-FLOW-31 state
  FLOW31_MERGE_MASTER_PLAN.md — 9 зон, offset +196, все фазы A–H
```

Причина: в конце долгой консолидационной сессии (FLOW-30 cleanup + FLOW-31 prep) последовательные артефакты получали инкрементальный счётчик сессии, не семантический FLOW-номер. «31» = «тридцать первый документ в этой сессии», не «документ FLOW-31».

Последствия:
- Принять FLOW-30 консолидацию за FLOW-31 данные → использовать устаревшие artifact counts
- Пропустить реальный FLOW-31 renumbering → сдвиг +196 не применён; backward compat collision
- Canonical state взят из неправильного файла (FLOW-30 end state ≠ FLOW-31 end state)

Правило: всегда проверять содержимое файла, а не только имя. «FLOW{N}_» prefix в имени = надёжный признак; пробел перед словом = подозрение.

### PHANTOM Archetype Bug (новый тип бага, FLOW-31)

Признак: в reference plan или planning doc перечислен новый тип/архетип/enum-значение, который **не существует ни в одном canonical файле** и не был добавлен в production enum.

```
Алгоритм детекции:
1. Найти в input/planning docs все упоминания «new ContractArchetype», «new enum value»,
   «добавляется тип», «новый архетип» etc.
2. Для каждого упоминания проверить:
   a. Есть ли он в ENGINE_ARCHITECTURE_MERGED enum section?
   b. Есть ли он в TASK_TYPES_CATALOG (task types использующие этот тип)?
   c. Есть ли он в corrected/final версиях плана (не только в оригинале)?
3. Если нет ни в одном canonical файле → PHANTOM Archetype

FLOW-31 пример:
  Оригинальный FLOW-31-REFERENCE-PLAN.md:
    ContractArchetype.MAPPING — «Figma screen → module mapping result»
  
  В enum в ENGINE_ARCHITECTURE_MERGED:
    PLAN, SCAFFOLD, COMPONENT, CONNECTOR, MODULE (существуют)
    MAPPING — ОТСУТСТВУЕТ
  
  В corrected versions (1)(2)(3):
    MAPPING убран — был семантическим дублёром MODULE
    MODULE уже охватывал все mapping artifacts
  
  Диагноз: planning doc написан до сверки с существующим enum;
    авторы позже обнаружили дублирование и убрали MAPPING
```

Последствия:
- Code generation пробует использовать PHANTOM тип → runtime exception в switch-statement
- BFA rules написанные под PHANTOM тип → никогда не срабатывают (мёртвые правила)
- Читатель corrected versions не видит MAPPING → путается «куда делся этот тип?»

Признак для раннего детектирования: если в planning doc упоминается новый тип который не упоминается ни в одном delta-файле ENGINE_ARCHITECTURE или TASK_TYPES → потенциальный PHANTOM. Проверить enum registry перед merge.

Правило: источник правды для enum — corrected/final версии плана + ENGINE_ARCHITECTURE_MERGED. Оригинальный reference plan при наличии corrected versions = black-listed draft.

### Типы input.zip — определить тип ПРЕЖДЕ чем использовать

**Тип A — оригинальные бизнес-требования** (как в FLOW-22)
- Признаки: читабельный текст, «что хочет пользователь», нет кода/фабрик, нет F-номеров
- Использовать: как Layer 1. Из files.zip — Layers 2–3.

**Тип B — старые delta-файлы предыдущего FLOW** (как в FLOW-23)
- Признаки: таблицы с F-номерами, поля MACHINE/FREEDOM/IRON RULES, нумерация другого FLOW
- Использовать: как стиль/шаблон описания. Layer 1 — из P0_PLAN/MASTER_PLAN в files.zip.

**Тип C — детальные delta-файлы того же FLOW** (как в FLOW-24)
- Признаки: файлы {N}_-_{название}_*.md, полные контракты фабрик/task types
- Использовать: как «deep dive» дополнение — когда в merged описание короткое. Числа — только из files.zip.

**Тип D — Agent-executable execution plan** (как в FLOW-36)
- Признаки: ⛔ STOP маркеры между фазами; «WHAT CLAUDE CODE RECEIVES»; deliverables в JSON; gate checks (server ≥ N, client ≥ N); NEVER/ALWAYS блоки с hard rules; нет ZIP-архивов; только .md файлы
- Использовать: как Layer 3 (implementation order) + Layer 2 (architecture decisions). Layer 1 извлекать из REQUIREMENTS-SUMMARY или секции "Why this task". Числа проверять против canonical STATE независимо — execution plans часто создаются в изолированных сессиях.
- Особенность: может использовать Document Alias Substitution — построить таблицу перевода перед анализом.

**Тип E — ML Training Data Package** (как в FLOW-38)
- Признаки: `00-INDEX.md` + `01-UML-FRAME.md` + `node-NN-*-set.md` + `10-SIMULATION-CHECK.md`; A/B пары для тренировки pipeline-узлов; source document REFERENCED но не включён; "Runtime A is..." / "Runtime B is..." структура; domain labels вместо технических ID
- НЕЛЬЗЯ извлечь: полное описание оркестрации, F-номера, task type contracts, MACHINE/FREEDOM split, BFA rules — эти данные в source document которого нет
- МОЖНО извлечь: имя флоу, домен, дату, stack target, implementation mode (manual/auto, P-номер, причина), prerequisite флоу, размер source document (косвенный признак масштаба), Edge Integrity Rule нарушения
- Порядок чтения: INDEX → UML-FRAME → SIMULATION-CHECK → node-01 (только для domain labels). Остальные node-файлы — только если нужны конкретные review patterns (node-08).
- Ключевой сигнал: если "implementation mode: manual" + "circular dependency" → флоу создаёт механизм который зависит от себя; manual = намеренный разрыв рекурсии
- Для полного теста: запросить source document (FLOW-38-LEARNING-LOOP-PLAN-v2.md)

**Тип F — Внешний Research Report как дополнение** (как в FLOW-28)
- Признаки: отдельные .md файлы с "deep-research", "report", "analysis" в имени; НЕ в input.zip и НЕ в files.zip; содержат Executive Summary + Architecture Recommendations + Risk Analysis; ссылаются на домен флоу но не на F/T/CF-числа
- Использовать: как Layer 1 (бизнес-требования) + дополнение к Layer 2 (design constraints). НЕ как источник artifact numbers
- Особенность: если таких файлов 2+ — они написаны с разных точек зрения (технической и бизнес-) → читать оба для полной картины проблемы
- Числа артефактов: ИГНОРИРОВАТЬ из этих файлов — берутся только из input.zip / files.zip / STATE файлов

**Порядок при двух ZIP-архивах:**
```
1. Финальное состояние: SESSION_STATE_MERGE.md — текущие счётчики и статус
2. P0 план / мастер-план
3. Канонические merged-файлы (architecture, task, BFA, skills)
4. Validation-отчёт
5. Только после этого — input-ZIP
```
Причина: финальный ZIP = что система считает правдой сейчас. Input-ZIP = откуда пришло. Читать input первым — риск принять устаревшую нумерацию за текущую.

### Validation report

Если есть VALIDATION.md или «N/N PASS»:
- **Никогда не писать «завершено» без confirmation из validation-файла**
- 99/99 PASS → система в заявленном состоянии
- Есть провалы → отметить что именно не завершено

---

## ШАГ 4 — ГЛУБОКОЕ ЧТЕНИЕ ПО ТИПАМ ФАЙЛОВ

### P0_PLAN (FLOW{N}_P0_PLAN.md)

Читать в строгом порядке именованных частей:

| Часть | Содержит | Используется для |
|-------|----------|-----------------|
| PART 1 — NO-CODE EXPLANATION | Функциональное описание, сравнение с предыдущим FLOW | Layer 1 |
| PART 2 — RENUMBERING MAP | Таблица старые → новые F/T/CF/SK | Шаг 3 |
| PART 3 — PHASE BREAKDOWN | 7 фаз, что делается, что получается, длительность | Layer 3 |
| PART 4 — PLAN VALIDATION | Требования, DNA-паттерны, backward compat | Шаг 9 |
| PART 5 — EXAMPLES | Положительные и отрицательные примеры стиля | Сохранение стиля |
| PART 6–7 — RAG / Numbers / Post-State | Итоговые счётчики, архитектурные решения | Финальное состояние |

P0_PLAN заменяет MASTER_EXECUTION_PLAN как главный файл.

### MASTER_EXECUTION_PLAN (если нет P0_PLAN)

Внутри всегда есть **человекочитаемая секция** — искать её первой:
- «No-code explanation» / «What We're Building» / «Business Context» / «Запрос»
- Или любой раздел без технических терминов в начале файла

Это **первичный источник для Layer 1**. Остальные разделы — технический план реализации.
Если секции нет — посмотреть в input.zip (Тип A).

### ENGINE_ARCHITECTURE

Для каждого нового компонента выписать:
- **Зачем он** — одно предложение, проблема которую закрывает
- **Что делает конкретно** — логика, не методы («читает сентинел → проверяет → пишет фазу»)
- **Куда смотрит** — конкретная система (ES, Redis, Neo4j) + что читает/пишет
- **FABRIC RESOLUTION** — через какой Fabric работает (DATABASE/QUEUE/AI/RAG FABRIC)
- **Зависит от** — что должно быть готово до него

Не выписывать: сигнатуры методов, DNA-номера, названия классов — если не объясняют логику.

### SESSION_STATE

Выписать:
- COMPLETE / DONE — фон, не трогать
- IN_PROGRESS / следующая фаза — точка входа оркестрации
- Диапазоны ID — ориентиры для «что новое»
- Zone Decomposition Table (если есть) — сразу Layer 2

**Кросс-проверка зон (обязательно):**

При чтении Zone Decomposition Table — сразу записать ожидаемый маппинг:
```
Family NNN → Зона X — [ожидаемое имя] — ожидаемые фабрики: FYYY–FZZZ
```
После чтения ENGINE_ARCHITECTURE_MERGED — для каждого семейства:
1. Найти первую фабрику в указанном диапазоне
2. Проверить имя заголовка FAMILY NNN в ENGINE_ARCHITECTURE
3. Если имя не совпадает с SESSION_STATE → **документационный баг**

**Пример бага (FLOW-25):**
- SESSION_STATE говорит: Family 149 = Context Aggregation (F1043–F1049)
- ENGINE_ARCHITECTURE показывает: `FAMILY 149 — Arbitration Orchestrator` (F1043 = IArbitrationStateMachineService)
- Фабрики верны, имена зон переставлены — этот рассинхрон нужно зафиксировать

Все найденные несоответствия — в секцию **«Что поймал протокол»** финального вывода.

### TASK_TYPES_CATALOG

Для каждого task type читать:
- **ENTRY** — что запускает (событие, API вызов, триггер) → строит пользовательский сценарий
- **PURPOSE** — зачем он существует → строит пользовательский сценарий

Из ENTRY+PURPOSE строится раздел «Как это выглядит для пользователя».
Полный контракт (DNA, iron rules, quality gates) — для Layer 3, не для пользовательского описания.

Искать секции **MACHINE / FREEDOM**:
- **MACHINE** = что нельзя менять: consent-гейт, server-side grading, append-only ledger, tenantId
- **FREEDOM** = что настраивается через ES-конфиги: длительность, пороги, модель AI, quiet hours

MACHINE/FREEDOM → в раздел «Железные гарантии» вывода.

### BFA / CF-правила

Выписать только CRITICAL и HIGH:
```
[Номер]: [Что запрещено] → [Что ломается если нарушить]
```

### История / исследование (deep research, сессионные записи)

Читать только для поиска «красной нити». Конкретно искать:
- Какая последовательность повторяется без изменений?
- Что в поздних версиях заменило раннюю? (брать позднее)
- Что было явно отброшено как ошибочное?

Для длинных сессионных файлов извлечь:
```
1. Исходная проблема пользователя
2. Ключевые коррекции от пользователя
3. Архитектурные концепции которые пережили коррекции
4. Артефакты которые стали каноническими
5. Текущее состояние после слияния/ренумерации
6. Финальная красная нить
```

**Multi-Document Deep Research — не путать с Multi-Vector Input** (FLOW-33 паттерн):

Если input-ZIP содержит 5-10+ файлов вида `*deep research N.md` — это НЕ отдельные векторы оркестрации. Это **контекстные документы одного вектора**, разбитые по темам. Отличия:

| Признак | Deep Research Context | Отдельный вектор |
|---------|-----------------------|-----------------|
| Содержит F-номера? | Нет (концептуальный фреймворк) | Да (delta-артефакты) |
| Priority в merge plan | P2 / Context | P1 / Source |
| Извлекать delta? | Нет — только концепции | Да — в отдельный раздел оркестрации |

Правило: количество input-файлов ≠ количество оркестрационных векторов. Классифицировать каждый по содержанию, не по имени файла.

### Multi-Vector Input — несколько независимых векторов в одном пакете (FLOW-31+)

Если в zip-пакете обнаруживаются документы ДВУХ или БОЛЕЕ разных видов:
- **Domain flow artifacts** — delta файлы для 7 canonical files (ENGINE_ARCHITECTURE delta, BFA rules, task types, skills)
- **Meta-system upgrade documents** — файлы вида `ENHANCED_PLAN.md`, `V{N}_STATE.md`, `SKILLS_FACTORY.md`, `SESSION_RAG_INDEX.md` без F-номеров, описывающие КАК строить систему (approach libraries, genesis methodology, auto-heal protocols)
- **Go-to-market / business research** — файлы MARKETPLACE-*.md, STATE-*.json, strategy docs — описывают бизнес-стратегию (куда публиковать продукт, какие внешние платформы), НЕ что делает движок

Матрица определения что есть что:

| Признак | Вектор |
|---------|--------|
| F-номера, task types, BFA rules, DNA patterns | Domain flow delta |
| V{N}_STATE.md, ENHANCED_PLAN.md, SKILLS_FACTORY.md | Meta-system upgrade |
| MARKETPLACE-*.md, TRAFFIC-*.md, STATE-*.json с внешними платформами | Go-to-market research |

**Правило:** описывать каждый вектор ОТДЕЛЬНО. Для оркестрации — только domain flow delta. Meta-system docs объясняют HOW team builds XIIGen. Go-to-market docs — бизнес-стратегия, не архитектура.

Эволюция паттерна:
- FLOW-31: Dual — domain (Design Intelligence) + meta-system (V31 Enhanced Plan)
- FLOW-32: Triple — domain (Sharable Flows Marketplace) + meta-system (V32 Unified) + go-to-market research
- FLOW-33: Single — domain only (Self-Building Bootstrap Engine). 8 deep research docs = context (P2 priority), не отдельный вектор. Research docs объясняют conceptual framework (MAPE-K, PromptOps, GraphRAG) но не содержат delta-артефактов. Важно: количество входных документов ≠ количество векторов.

### Несколько уровней в одном пакете (миграция + новый FLOW)

Два разных уровня — не сливать в один нарратив:
- **Уровень миграции**: что изменилось в существующем (что переехало, stack-neutral)
- **Уровень нового FLOW**: что добавилось (новые фабрики, task types, паттерны)

Описывать по очереди, показать связь: «Phase 19 завершила переход стека, после чего FLOW-19 добавил DevOps Control Plane».

### Система которая сама себя строит (история реализации)

Если флоу описывает систему которая и строится и работает — найти «историю реализации»:
```
Что написано вручную один раз?
Что после этого становится данными?
Что запускает первое выполнение?
Какой процесс повторяется для каждого юнита/семейства?
Что предотвращает принятие плохого результата?
Где хранится прогресс? Как человек видит и одобряет?
```

### Несколько файлов по одной теме (_FACTORIES, _TASK_TYPES, _BFA, _SKILLS)

Сначала главный (MASTER_EXECUTION_PLAN или файл без суффикса).
Остальные — разделы одного плана, читать как дополнения.
Искать повторяющийся паттерн — структуру которая используется во всех фазах/флоу.

---

## ШАГ 5 — ПОИСК КРАСНОЙ НИТИ И ПОВТОРЯЮЩЕГОСЯ ПАТТЕРНА

### Красная нить

Главная последовательность от первого события до конечного результата — позвоночник, не все шаги.

**Как найти:**
1. Взять все фазы из всех прочитанных файлов
2. Найти последовательность которая встречается в большинстве файлов в одном порядке
3. Убрать всё что является деталью одного шага (retry, ветвления, edge cases)
4. Оставить только переходы состояния системы

**Формат:**
```
[Стартовое событие] → [Состояние 1] → [Состояние 2] → ... → [Конечное состояние]
```

Для длинных сессий красная нить НЕ написана в одном месте — синтезировать из коррекций разговора, произведённых плановых файлов, текущего состояния, canonical merged-документов, validation-отчётов.

### Три обязательных слоя для сложных систем

```
Слой 1: ЧТО ХОЧЕТ ПОЛЬЗОВАТЕЛЬ
  → Функциональное/продуктовое описание
  → Без архитектуры, без зон, без фабрик
  → «Рекламодатель хочет: аукцион <50мс, таргетинг только с согласием»

Слой 2: ТЕХНИЧЕСКИЙ ДИЗАЙН (МОСТ)
  Три последовательных моста:

  Мост A — Таблица зон:
    Формат: | Зона | Семейство | Что решает (бизнес → тех) |
    ОБЯЗАТЕЛЬНО после таблицы: принцип деления одним предложением:
    «Зоны созданы по принципу жизненного цикла формы»
    «Зоны разделены по актору (пользователь / система / внешний сервис)»
    Без принципа → зоны кажутся произвольными

  Мост B — Зона → конкретные фабрики:
    Каждая зона → семейство → диапазон F-номеров
    Источник: ENGINE_ARCHITECTURE_MERGED.md

  Мост C — Design Decisions (явный перевод):
    Формат: «[Бизнес-требование]» → DD-NNN: [Архитектурное решение]
    Типы DD по категории:
    - Хранение:     «Формы гибкие» → DD-180: Form schema в Elasticsearch, не в реляционной БД
    - Порядок:      «Нет потерь» → DD-184: Persist-before-emit
    - Изоляция:     «Антиспам надёжен» → DD-183: Anti-spam как отдельный fabric gate
    - Паттерн:      «Рецепты как Zapier» → DD-185: Recipe DAG использует тот же движок что Flow Engine
    - Наследование: «Всё изолировано» → DD-191: Forms MT наследует модели из FLOW-08+14
    Источник: UNIFIED_SOURCE_INDEX_MERGED.md

Слой 3: РЕАЛИЗАЦИЯ
  → Task types, фабрики, BFA-правила, стресс-тесты
  → Всё вытекает из Слоя 2
```

Прыжок через слой = неправильный вывод.

**Зоны для продуктовых флоу должны следовать жизненному циклу пользователя:**

Визуальный редактор: `создание/редактирование → моделирование → авторинг → публикация → SEO/routing → медиа → workspace`

Формы и автоматизация: `создание формы → runtime → сбор submissions → lifecycle записей → уведомления → интеграции → рецепты → коннекторы → изоляция`

Обучение / тьютор: `онбординг → согласие+профиль → диагностика → карта знаний → урок → safety → календарь → квиз → геймификация → адаптация → восстановление`

Если зоны не следуют lifecycle — объяснить почему. Если объяснения нет → мост неполный.

### Engine Primitive / Cross-Cutting Capability

Если флоу добавляет фундаментальный механизм под все флоу (новый тип узла, универсальный scheduler, runtime visibility):

**1. Backward compat матрица** — явно перечислить что не сломалось:
```
| Существующий артефакт | Влияние от EP-N | Механизм |
|----------------------|-----------------|---------|
| FLOW-01 – FLOW-26    | Нет влияния — EP-N добавляется снизу | DI injection |
| T1–T412              | Не изменены. Новые factories инжектируются через DI | |
| waitFor[] в flow JSON | Уже существовал — теперь интерпретируется | Zero migration |
```

**2. Zero-migration check** — что движок переиспользует без изменений схемы:
- Ключи из flow JSON которые уже были (`waitFor[]`, `dependsOn[]`, `nodeType`)
- Existing fabric providers которые подключаются без нового кода (`INotificationProvider`, `IDatabaseService`)
- DI-совместимость: новые фабрики инжектируются в те же контейнеры

**3. Провайдер-reuse** — отметить явно:
```
Переиспользовано: [F68 INotificationProvider, Skill 04/05, MicroserviceBase]
Новое: [только то что действительно ново]
```
Если EP добавляет новый провайдер — объяснить почему существующий не подошёл.

**4. Двойная проверка обновления состояния** (критично для этого типа):
- Validation ✅ в completion report проверяет только что АРТЕФАКТЫ (F-номера, T-номера) попали в файлы
- Validation НЕ проверяет что SESSION_STATE изменил заголовок, обновил FLOW REGISTRY и счётчики
- Читать SESSION_STATE отдельно и проверить: (a) заголовок «Post FLOW-N», (b) FLOW-N есть в реестре, (c) глобальные счётчики соответствуют post-FLOW-N состоянию

**Пример бага (FLOW-27):** Completion report сказал SESSION_STATE ✅. Но SESSION_STATE.md остался с заголовком «Post FLOW-26» и FLOW-27 не попал в FLOW REGISTRY — валидация считала ссылки на F-номера в файле, но не читала сам заголовок и таблицу.

### Governance / Cross-Flow Gate (арбитр)

Если флоу является governance gate — перехватывает изменения в уже существующих флоу:

**1. Cross-flow protection matrix** — таблица какие флоу защищены:
```
| Существующий флоу | Что защищает | Через какое BFA-правило |
|-------------------|-------------|------------------------|
| FLOW-08 Payment   | CRITICAL: изменения блокируются всегда | CF-478 |
| FLOW-18 Visual Flow | User-created flows обязаны проходить gate | CF-508 |
```

**2. Static-before-AI ordering** — всегда документировать порядок и полномочия:
```
Статика первой: [что проверяют детерминированные правила] — быстро, без AI
AI вторым: [что ищет семантический анализ] — только повышает severity, никогда не понижает
```
Если AI может понизить severity — это архитектурный дефект.

**3. Escape hatch constraints** — для каждого bypass-механизма (FORCE_PROCEED и аналогов):
```
Требует: [какие права нужны]
Логирует: [постоянный audit trail, не удаляется]
Всё равно заблокировано: [что нельзя обойти даже через escape hatch]
```
Пример: FLOW-24 School Pack — FORCE_PROCEED заблокирован полностью, не только ограничен.

**4. Blast radius chain** — путь от изменения до последствий:
```
[Изменение сущности/API/схемы]
  → Dependency index lookup → прямые зависимости
  → Транзитивный обход → скрытые зависимости
  → Severity aggregation (static) → AI semantic check
  → Impact report → Human decision (REFACTOR / REJECT / COMPAT_WRAPPER / FORCE_PROCEED)
```

**5. Durable session guarantee** — arbitration state machine должна переживать сбои:
- Persist-before-emit: сохранить состояние до публикации события
- Timeout не auto-approves — escalates
- TenantId обязателен во всех BFA queries

### Self-Learning Infrastructure Layer

Если флоу не добавляет новый бизнес-домен, а **делает существующую AI-инфраструктуру адаптивной** через feedback, versioning, bandit routing, или experiment-driven promotion:

**1. Замкнутость feedback loop** — убедиться что петля закрыта от конца до начала:
```
[Действие пользователя / результат запроса]
  → T_ingest (сбор сигнала, idempotency key)
  → агрегатор (за окно: N запросов или T времени)
  → T_update (политика обновляется async, предыдущая архивируется)
  → T_route (следующий запрос уже использует новую политику)
```
Каждое звено должно быть явно описано. Пропущенное звено = данные собираются но ни на что не влияют.

**2. MACHINE-инварианты под самообучением** — бандитная система может "выучить" нежелательное поведение. Проверить:
- Arm structure (комбинация dimensions) — invariant, не configurable: если dims меняются без миграции → CF HALT или REJECT
- Budget enforcement — FAIL, не silent truncation: CF-правило CRITICAL REJECT (иначе billing/безопасность)
- Policy update — ASYNC ONLY: синхронное обновление политики нарушает изоляцию запросов
- Cross-tenant data — у каждого граф-запроса tenantId: отдельный CF CRITICAL REJECT

**3. Promotion pipeline completeness** — путь от "experimental" до "production":
```
GENERATED → INJECTED → MINIMAL → CORE  (promotion ladder шаги)
```
Проверить: есть ли CF-правило REJECT на каждый skip шага. Если шаги не защищены BFA — возможна несанкционированная промоция.

**4. Improvement suggestions: human gate** — самообучающаяся система предлагает изменения. Убедиться что:
- Suggestions NOT auto-applied: явный human-gate (Control Plane action, или EP-level gate)
- Если gate implicit (через UI/Control Plane) — отметить как architecturally weaker чем explicit BFA REJECT
- Сравнить с FLOW-27 EP-6 pattern: там Redis SETNX claim + BFA rule — это сильная форма
- Если только текст плана ("never auto-applied") без CF-правила → архитектурное наблюдение

### Design Intelligence Layer (FLOW-31 паттерн)

Если флоу добавляет **восприятие структурного артефакта** (Figma, ERD, документ) вместо обработки пользовательских действий:

**1. Mandatory IR pipeline** — конвейер обязателен целиком:
```
DesignIR (структурный) → ScreenSemanticsIR (per-unit, с evidence map) → SystemModelIR (глобальный вывод)
```
Каждый слой: хранится в ES отдельно, независимо queryable. Средний слой пропускать нельзя — в нём создаётся evidence map, без которой iron rule «каждый claim требует ≥1 evidence» неисполним.

**2. Evidence-anchored confidence** — LLM self-report downweighted:
```
confidence = evidence_count × 0.4 + evidence_quality × 0.4 + llm_self_confidence × 0.2
```
Если LLM self-report весит >20% — проверить, не введён ли bias к overconfidence. Evidence quality: component name match > text label match > layout pattern match.

**3. "Don't know" design rule** — система никогда не угадывает:
- Элементы с confidence < threshold → UNCLASSIFIED, surfaced пользователю
- Ambiguous top-2 кандидаты (разница < порог) → PAUSE + user selection
- Проверить: есть ли iron rule которая запрещает auto-assign ниже порога? Если нет — архитектурное наблюдение

**4. External ontology check** — если AI mapping использует внешнюю матрицу (module-architecture, ERD schema, etc.):
- Кто определяет ontology? LLM или authoritative external source?
- Если LLM redefines at runtime → риск drift от canonical
- Правильный паттерн: F_loader_service загружает matrix в ES startup; LLM retrieves from it, never redefines
- Iron rule: read-only на ontology graphs в runtime

### Meta-Flow / Self-Extension (FLOW-33 паттерн)

Если флоу даёт движку инфраструктуру **для самостоятельной реализации самого себя** — bootstrap + implement-family loop + arbiter consensus:

**1. Idempotent Bootstrap State Machine** — проверить три инварианта:
```
a. Sentinel-before-run: CheckBootstrapStatus читает sentinel ПЕРЕД любым действием; версия >= target → skip
b. Outbox pattern: фаза пишется в ES ДО emit события (нарушение = потеря прогресса при сбое)
c. Smoke gate: BootstrapCompleted НЕЛЬЗЯ выдать без прохождения smoke test (CF-rule CRITICAL)
```

**2. Two-Layer GraphRAG Seeding Sequence** — строгий порядок:
```
Layer 1 (Catalog Graph — топология): Family→Factory→Method→TaskType→Skill→Template
  ↓  ПОЛНОСТЬЮ завершён (CF CRITICAL)
Layer 2 (Impact Graph — динамика): ProviderImpl→TestSuite→FlowDef прикрепляются к существующим узлам
```
Почему нельзя параллельно: Layer 2 edges (ProviderImpl IMPLEMENTS Method) требуют чтобы Method-узлы уже существовали. Seed Layer 2 первым = orphaned edges.

**3. 5-Arbiter Consensus** — строгий порядок и правила:
```
Coverage → Security → DNA Compliance → Testing → BFA Dependencies
  ↑         ↑          ↑               ↑          ↑
  все 5 должны пройти; нет "большинства"
  порядок = MACHINE (нельзя переставить)
  max iterations = MACHINE (нельзя превысить → эскалация к человеку)
  failure = exact trace → regeneration (не generic retry)
```
Проверить: failure trace идёт обратно в генератор со структурированными полями? Если "попробуй ещё раз" без trace — архитектурное наблюдение.

**4. ContextPack TTL** — стейл контекст отравляет генерацию:
- Есть ли CF-правило CRITICAL/HIGH которое rejections стейл пакет?
- TTL хранится как MACHINE-значение или FREEDOM? Если FREEDOM — проверить default
- Долгая сессия (>TTL) = автоматический rebuild ContextPack, не продолжение со старым

**5. Backward Compatibility Fence** — явный BFA CRITICAL:
```
CF-rule: все F1-F{prior_last} READ-ONLY; F{new_start}+ добавляют
Проверить: CF-rule с severity CRITICAL в BFA для read-only fence?
Если нет — backward compat гарантируется только намерением, не enforcement
```
FLOW-33 первый который делает это explicit BFA rule (CF-743: "Backward Compatibility Fence F1-F1418 Read-Only").

### Multi-Language Code Generation / Canonical Spec + Adapter (FLOW-34 паттерн)

Если флоу добавляет генерацию одного артефакта в N языковых/фреймворковых стеках через Canonical Spec:

**1. Canonical-before-Variant Triple Check:**
```
a. Все три договора присутствуют: OpenAPI 3.1 + CloudEvents 1.0 + JSON Schema 2020-12
   (при отсутствии хотя бы одного — CF CRITICAL, variant generation не запускается)
b. Spec version заморожена до старта генерации — нельзя менять пока хоть один вариант в progress
c. Golden test vectors подготовлены до variant generation — conformance runner использует только их
```

**2. P0 → P1∥P2 → P3 → P4 порядок:**
```
P0 (canonical extraction) — SEQUENTIAL, обязателен первым
P1 (server variants) ∥ P2 (client variants) — PARALLEL, оба стартуют после P0_COMPLETE
P3 (WordPress, conditional) — отдельная ветка, только если WP targets заказаны
P4 (Graph RAG) — Phase B, ТОЛЬКО после всех вариантов достигли INJECTED
```
Проверить: есть ли CF-правило что P4 не стартует раньше INJECTED? Если нет — Graph RAG может seeded с неполными данными.

**3. WordPress двойная проверка:**
- Plugin MUST NOT contain business logic (CF-правило, не просто рекомендация)
- Theme MUST NOT contain REST endpoints или business logic
- permission_callback на каждом REST endpoint = BUILD FAILURE при отсутствии
- Оба target-а используют один общий Security Gate (T262/CF-783)

**4. Cross-language DNA Compliance Matrix:**
```
Для каждого серверного языка обязательны 5 invariants:
  1. DataProcessResult<T> эквивалент (Result<T,E> в Rust, Either<Error,T> или similar в других)
  2. ParseDocument → map/dict only (никаких typed models)
  3. Tenant scope propagation на каждой storage/query операции
  4. DynamicController эквивалент (один generic router через config)
  5. W3C TraceContext (OpenTelemetry) propagation
Если язык не может выразить один из 5 — это architectural blocker, не FREEDOM.
```

**5. No-Secrets audit scope:**
Проверить что No-Secrets gate применяется КО ВСЕМ слоям: client bundles, wp_options, server adapters, SDK artifacts, packaging manifests. Если любой слой выведен из-под gate — архитектурное наблюдение.

### Infrastructure Bug Fix / SDK Hygiene Correction (FLOW-35 паттерн)

Если флоу **не добавляет бизнес-домен**, а исправляет нарушения в существующем коде и добавляет инфраструктурный фабрик (не flow-specific, а cross-cutting для всей системы):

**1. Phase 0 Mandatory Prerequisite** — bug fix обязан быть первой фазой:
```
Симптом: существующий провайдер/сервис использует мёртвый или нарушающий принципы SDK
Правило: Phase 0 = исправление этого нарушения, НОЛЬ новых артефактов
  → нельзя добавлять нового провайдера пока существующий сломан
  → нельзя начинать Phase 1+ пока forbidden import list не обновлён
Проверить: обновлён ли список forbidden imports чтобы не допустить откат?
  (пример: google.generativeai должен остаться в forbidden даже после миграции на google.genai)
```

**2. Два типа SDK-нарушений которые требуют фазы 0:**
```
a. Dead Package / EOL SDK:
   Признак: пакет официально снят с поддержки (repo renamed, EOL notice, archived)
   Fix: migration to current SDK, ноль изменений в публичном контракте (IAiProvider)
   Валидация: все тесты перезаписаны с новыми моками, assertions идентичны

b. Provider-Leaking / Cross-Provider SDK Import:
   Признак: import SDK провайдера A для вызова провайдера B (напр. openai для xAI/Grok)
   Fix: native SDK провайдера B, без зависимости от SDK A
   Валидация: forbidden imports включает оба пакета (старый cross-import + новый native)
```

**3. 7-й фабрик: паттерн распознавания "ещё один фабрик vs новый бизнес-флоу":**
```
New Fabric (fabric-first infrastructure):
  - Интерфейс добавляется в fabrics/interfaces.py (общий файл интерфейсов)
  - Используется ВСЕМИ флоу через резолвер (не специфичен для одного домена)
  - Инициализируется в bootstrap sequence ДО domain flows
  - Семейство = Infrastructure Family (не domain family)
  - Нет flow template в FlowOrchestrator (fabric ≠ flow)

New Domain Flow:
  - Добавляет бизнес-логику пользователя или нового процесса
  - Есть flow template (JSON DAG для FlowOrchestrator)
  - Domain-specific families (не глобальный bootstrap)
```

**4. ConfigBuilder / Secret Reference Resolution — проверки:**
```
a. Reference синтаксис MACHINE (не меняется): $secret:path/to/key, $env:VAR_NAME
b. Resolution order MACHINE (exact match → error; нет parent path guessing)
c. Cache invalidation MACHINE (TTL-based); TTL duration = FREEDOM
d. resolver НИКОГДА не видит $secret: ссылки — только resolved dict
e. Проверить: есть ли Iron Rule "NEVER log resolved secret values"?
   Если нет в BFA — только намерение, не enforcement
```

**5. Bootstrap Sequence Order как MACHINE-инвариант:**
```
Фиксированный порядок: Secrets → Config → Database → Queue → AI → RAG → Flow Engine
  ↓ каждый шаг = pre-condition для следующего
Проверить:
  - Secrets failure → ABORT (не DEGRADED): конфиг без секретов = сломанный конфиг
  - Config failure → ABORT (не DEGRADED): фабрики без resolved config = неизвестное состояние
  - Database/Queue/AI failure → DEGRADED (не ABORT): система частично работает
  - Порядок = MACHINE? Или можно переставить через config FREEDOM?
Если порядок FREEDOM — архитектурное наблюдение (зависимости нарушаются)
```

**6. SDK Hygiene BFA Gap — архитектурное наблюдение:**
Если флоу исправляет SDK-нарушения но не добавляет CF-правила предотвращающего их повторение:
- Нет CF CRITICAL: "FORBIDDEN import of package with EOL status"
- Нет CF CRITICAL: "FORBIDDEN cross-provider SDK import (import SDK A to call provider B)"
Это "text-only constraint" — violation может повториться в следующем флоу без gate.
Рекомендация: добавить в forbidden imports check в fabric tests (автоматизированная проверка > BFA text).

### Self-Implementing Engine Workload / Engine as First Customer (FLOW-36 паттерн)

Если флоу использует **сам движок** для генерации инфраструктурных компонентов этого же движка — движок становится своим первым заказчиком:

**1. Two-Track Architecture — строго разделить:**
```
Track A (человек / Claude Code):
  - Инфраструктура: Docker, ES setup, pip install, Ollama pull
  - EngineContracts: первый набор создаётся вручную (Phase 0 bootstrap)
  - Начальные промпты: первая версия пишется вручную
  Критерий: «XIIGen не генерирует Docker configs или pip install commands»

Track B (XIIGen AF pipeline):
  - Python/code generation: все .py файлы через AF-1 Genesis
  - Validation: через AF-9 Judge + 5 арбитров
  - Improvement loop: через PromptOps на низких scores
  Критерий: AF pipeline — это то для чего он построен; убирает человека из кода
```
Смешение треков = архитектурное наблюдение (человек пишет код который должен генерировать движок).

**2. Gradual Autonomy Phases — проверить gradient:**
```
Phase A: 0% — Human injects contracts + prompts; engine only accepts contracts
Phase B: ~30% — Engine picks model based on first data; human reviews output
Phase C: ~70% — Engine selects from accumulated data; runs benchmarks itself
Phase D: ~90% — Engine validates its own output across N dimensions
```
Проверить: каждая фаза действительно менее зависима от человека чем предыдущая?
Если Phase D требует столько же human review как Phase A — gradient не работает.

**3. Phase 0 Bootstrap Paradox:**
```
Проблема: движок не умеет декомпозировать «сделай benchmark» в task types (оркестрация слаба)
Решение: вручную написать первый набор EngineContracts — дать движку с чем работать
Паттерн: первая итерация = INJECTED (человеком), последующие = движком через PromptOps
Проверить: явно ли отмечено что Phase 0 = bootstrap exception, не нормальный рабочий режим?
```
Это не противоречие архитектуре — это bootstrapping pattern (системы не могут запуститься из ничего).

**4. Meta-Loop Integrity:**
```
Движок генерирует benchmark tool → benchmark tool оценивает модели → модели включают тот который генерировал
Проверить:
  a. Есть ли human gate между Phase 1 (generation) и Phase 4 (training data)?
     Если generated code → training data без validation → circularity corrupts training
  b. Кто определяет первый quality threshold (≥80 для training data)?
     Если threshold устанавливает сам генератор — self-referential bias
  c. FLOW-36 решение: Luba validates initial generated benchmark перед training data capture
     Это корректное решение — external validator разрывает петлю
```

**5. Cross-Document Artifact Number Verification (критично для этого типа):**
```
Execution plans часто создаются в нескольких итерациях → inconsistent numbers
Алгоритм:
  1. Найти все artifact number claims в ВСЕХ документах пакета
  2. Для каждого типа создать таблицу: document → claimed start number
  3. Сравнить с canonical STATE
  4. Если «CRITICAL CORRECTION» упомянута — проверить ВСЕ типы, не только исправленные
  5. Stress tests и design records особенно часто пропускаются при частичных коррекциях
```

### LLM Dynamic Flow / Meta-Pipeline Training Data (FLOW-38 паттерн)

Если входной пакет содержит **A/B training data для 9-node pipeline** вместо реального плана флоу:

**1. Диагностика: это training data или план?**
```
Признаки training data пакета:
  - Файлы: 00-INDEX.md + 01-UML-FRAME.md + node-NN-*-set.md + 10-SIMULATION-CHECK.md
  - Структура: "Runtime A is..." / "Runtime B is..." в каждом файле
  - Содержимое B-ответов: generic ("The X responsibility preserves the source meaning...")
  - Нет F-номеров, BFA rules, фабрик, task types
  - INDEX содержит "Inventory type: not listed in attached MVP inventory"

Action: запросить source document. Без него → partial analysis only.
```

**2. Что МОЖНО извлечь из training data пакета:**
```
Из INDEX: имя, домен, дата, размер source, stack target, implementation mode
Из UML-FRAME: архитектура pipeline (9 узлов с ролями)
Из SIMULATION-CHECK: что "должно сломаться" — список anti-patterns
Из node-01 domain labels: тематические домены флоу (user intent, decision trail, etc.)
Из node-08 arbiter units: типы review concerns (missing evidence, unclear dependency, etc.)
```

**3. 9-Node LLM Dynamic Flow — архитектура:**
```
Node 1: Domain Splitter — user prompt → array of domain labels
Node 2: Domain Translator — one fragment + label → formal domain statement
Node 3: Scheme Planner — all formal statements → ordered/parallel work pieces
Node 4: Domain Processor — one work piece → complete processed result
Node 5: Applier — one processed piece → concrete outcome
Node 6: Result Translator — one formal result → readable result
Node 7: Arbiter Selector — result collection → required reviewers
Node 8: Arbiter Units — review responsibility → pass/concern/reprocess
Node 9: Formatter — approved translated results → one coherent answer

Ключевые свойства: ТОЛЬКО approved results достигают Formatter; арбитры могут
вернуть ТОЛЬКО проваленный фрагмент (не всё); параллельные ветки до merge;
wait state перед merge; recovery route для re-processing
```

**4. Edge Integrity Rule — ключевой инвариант пакета:**
```
"No package may train a node as if it receives source-document lines at runtime."
Смысл: runtime input ≠ source document строка; каждая A-B пара должна иметь
runtime-shape (как реальный запрос пользователя), не document-shape (как строки плана)

Проверить: B-ответы начинаются с "The user intent responsibility preserves..." →
это generic шаблон. Edge Integrity нарушена если B = дословная цитата source doc.

Нарушение = training data defect (обучит pipeline принимать markdown как user input)
```

**5. Circular Dependency Pattern:**
```
Признак: "implementation mode: manual" + "circular dependency"
Смысл: флоу создаёт механизм который зависит от этого же механизма
  FLOW-38 example: learning loop COMPLETION → training loop trains on FLOW-38 → 
    training data derived from FLOW-38 plan → план создаёт тот же loop
Manual = намеренный разрыв: человек вручную создаёт первую итерацию

Проверить:
  - Есть ли явное "see justification" упоминание? Если нет — circular dep не задокументирован
  - Есть ли prerequisite с "flow-N active"? Prerequisite = dependency на уже живущий флоу
  - Если prerequisite не верифицирован → deployment blocker
```

### BUILD TIME vs RUNTIME

Если система и строится и работает — описывать оба слоя отдельно:
- **BUILD TIME**: как движок строит систему (фазы, AF-станции, генерация, слияние)
- **RUNTIME**: как готовая система работает (триггеры, Flow Templates, события, таймеры)

Внутренние движки генерации:
```
INVENTORY (собирает контекст, паттерны, ограничения)
→ SYNTHESIS (генерирует реализацию)
→ JUDGMENT (проверяет: AF-станции, BFA, DNA, iron rules, quality gates, стресс-тесты)
```
Нельзя сводить к «AI генерирует код» — оркестрация = разделение сбора, генерации и проверки.

### Повторяющийся паттерн и скелет

**Паттерн XIIGen FLOW** (повторяется в каждом флоу):
```
мастер-план → фазы P0–Pn → Save Point → Recovery Command → AF-станции → Merge → SESSION_STATE
```
Описать один раз, конкретный флоу — как экземпляр шаблона.

**Скелет перед текстом:** прежде чем писать вывод, выписать скелет блоков. Скелет не идёт в вывод — инструмент синтеза.

### Порядок зон = UX-маршрут

Зоны описывать в порядке в котором пользователь с ними сталкивается, не по алфавиту и не по номерам семейств.

### Функциональная группировка

Когда компонентов много (10+ станций, 20+ фабрик) — группировать по функции:
- Плохо: «AF-1, AF-2, AF-3, AF-4, AF-5, AF-6...»
- Хорошо: «INVENTORY (AF-2,4) → SYNTHESIS (AF-1,5) → JUDGMENT (AF-6..9, AF-10, AF-11)»

---

## ШАГ 6 — РАСПОЗНАВАНИЕ ТИПА ФЛОУ

После нахождения красной нити — определить к какому типу относится флоу. Каждый тип требует дополнительных правил:

| Тип флоу | Признаки | Дополнительно |
|---|---|---|
| **Длинная сессия** | Большой сессионный файл + ZIP-ы | Извлечь: исходная проблема → ключевые коррекции → выжившие концепции → канонические артефакты → финальная красная нить |
| **Адаптер / порт** | «adapt», «port», «plugin»; сравниваются платформы | 3 вещи: что даёт хост-платформа / что уже умеет движок / что нового для этой платформы. Несколько адаптеров → обязательная таблица сравнения |
| **Multi-Language Code Generation / Canonical Spec + Adapter** | «translate to alternatives», «multi-stack», «canonical spec», «cross-language»; много целевых стеков (5+); WordPress как packaging target | Пять вещей: (1) **Canonical-before-Variant gate** — canonical spec заморожена ДО любой генерации (CF CRITICAL); OpenAPI+CloudEvents+JSON Schema — все три обязательны; (2) **Параллельность P1+P2** — server variants (P1) и client variants (P2) параллельно, но P0 (canonical extraction) строго sequential первым; (3) **WordPress = два отдельных target-а** — Plugin (behaviors+admin+blocks) и Theme (styling+templates); нет business logic в WP targets; (4) **Graph RAG — Phase B после regular library** — graph seeding только после P0-P3 завершены, только после всех вариантов INJECTED; параллельный Graph RAG = architectural defect; (5) **No-Secrets как cross-cutting MACHINE** — ни в client bundles, ни в wp_options, ни в server adapters — единое MACHINE-правило через все варианты, не per-target FREEDOM |
| **Мульти-фича платформа** | Несколько несвязанных возможностей сразу | Разделить: definition-time (как создаются task types, BFA, skills) vs runtime (как события маршрутизируются через flow templates) |
| **Системный FLOW** | Flow registry с предыдущими флоу | 2 уровня: (1) системный паттерн FLOW-01→FLOW-N, (2) что конкретно добавляет этот флоу |
| **Marketplace / финансовый** | Деньги, контракты, escrow, репутация | Сначала runtime lifecycle (job → proposal → contract → escrow → dispute → payout). Защита денег — не детали, а оркестрация. Обязательно: idempotency, append-only ledger, KYC gates |
| **Template / App Store Marketplace** | Цифровые артефакты (flow DAGs, конфиги, стратегии), не физические товары; издатель/потребитель; install modes; revenue share | Три вещи: (1) **Logic/Data Separation** — marketplace обменивается ЛОГИКОЙ, НИКОГДА ДАННЫМИ (эмбеддинги, секреты, документы не экспортируются); (2) **Ownership Model** — три режима (Snapshot/Linked/Fork); при Linked обновление от издателя = mandatory BFA revalidation у потребителя, нельзя skip; (3) **Supply chain integrity** — artifact signing (unsigned = нельзя publish в public), SBOM, SLSA provenance; human review gate для fraud (никаких auto-suspensions) |
| **Миграция + новый FLOW** | Сертификат миграции + files нового флоу | Два уровня отдельно: миграция базелайна + новые возможности. Не сливать. Что изменилось для ВСЕГО будущего vs что добавил этот конкретный флоу |
| **Регулируемая платформа** | Advertising, Graph API, consent, billing | consent → targeting → auction (latency) → attribution → quotas → billing ledger → policy. INVENTORY/SYNTHESIS/JUDGMENT — три движка, не «AI генерирует» |
| **No-code / формы** | drag-and-drop, submissions, recipes | User-facing lifecycle до зон: форма → submission → notification → integration → рецепты. Зоны = lifecycle |
| **Визуальный редактор / CMS** | Редактор страниц, content types, publishing | Пользовательский journey: создание → моделирование → авторинг → публикация → design tokens → медиа → workspace |
| **Расширение предыдущего флоу** | «extends FLOW-X» или «FLOW-X Extended» | Начать с: что уже было → что добавляется (before/after story). P0_PLAN = source of truth. Old input = исторический паттерн |
| **Обучение / тьютор / календарь** | Адаптивное обучение, квизы, domain packs | Замкнутый цикл: онбординг → диагностика → урок → квиз → геймификация → адаптация → восстановление. Consent и safety — ключевые гарантии, не детали |
| **Governance / Cross-Flow Gate** | Перехватывает изменения во всех предыдущих флоу, impact analysis, blast radius, human arbitration | Cross-flow protection matrix → static-before-AI ordering → escape hatch constraints → durable session → cross-tenant guard |
| **Meta-Flow / Self-Extension** | Движок использует себя для роста: генерация, валидация, ассимиляция новых возможностей. Признаки FLOW-33-типа: bootstrap kernel + implement-family loop + 5-arbiter consensus | Пять вещей: (1) **Idempotent Bootstrap** — state machine с sentinel в ES; каждая фаза пишется ДО emit события (outbox); повторный запуск безопасен; (2) **Two-Layer GraphRAG Seeding Sequence** — Слой 1 (топология) ОБЯЗАН быть полным до начала Слоя 2 (embeddings); CF-правило CRITICAL на нарушение порядка; (3) **5-Arbiter Consensus** — все 5 арбитров должны пройти (нет "большинства"), порядок Coverage→Security→DNA→Testing→BFA MACHINE, max iterations MACHINE, failure trace подаётся обратно генератору; (4) **ContextPack TTL** — context для генерации имеет максимальное время жизни (4h в FLOW-33); арбитры REJECT стейл; (5) **Backward Compatibility Fence** — явное BFA-правило CRITICAL: все ранее существующие фабрики read-only; попытка перезаписать = немедленный откат. Рекурсивный тип: строит то же что исполняет |
| **Proto-Meta-Flow / First Self-Extension** | Первый флоу где движок начинает расширять себя. Признаки FLOW-26-типа: (a) триггер = CapabilityGapDetected (FlowOrchestrator не нашёл фабрику); (b) SelfBuildRun state machine с outbox + idempotency; (c) Genesis Loop с bounded retries (3–5) + human escalation; (d) Promotion Ladder (≥4 стадий, CORE = human gate MACHINE); (e) GitOps assimilation (ветка + PR + CI + hot-reload). НЕ имеет 5-arbiter consensus (появится в FLOW-33). Это отличает Proto-Meta-Flow от Meta-Flow / Self-Extension | Шесть вещей: (1) **Reuse-before-Generate gate** — COPY/ADAPT/REWRITE evaluation MACHINE; нельзя пропустить перед каждым genesis; (2) **Contract-before-Code** — factory spec + BFA draft регистрируются ДО запуска Genesis Loop; нарушение = architectural defect; (3) **Evidence Bundle completeness** — 12 обязательных секций (factorySpec, taskTypeContract, eventSchemas, BFA draft, codeBundle, unitTests, e2eTests, DNA matrix, securityScan, sandboxLog, flowTemplate, promotionRecommendation); missing DNA matrix или BFA reg = BUILD FAILURE; (4) **E2E с оригинальным запросом** — НЕ синтетические тесты; тестировать именно тот сценарий который обнаружил gap; (5) **BFA gate в двух точках** — при открытии PR и при CI gate; backward compat failure against cross-flow governance = BUILD FAILURE, no override; (6) **CORE = human MACHINE** — проверить: есть ли явный CF-правило REJECT или это текст плана; если только текст — "text-only constraint" |
| **Engine Primitive / Cross-Cutting** | Добавляет новый тип узла или фундаментальный механизм под ВСЕ флоу; все шаблоны наследуют автоматически | Проверить 4 вещи: (1) backward compat явно перечислен — что не сломалось, (2) zero-migration: что уже было в flow JSON / DI, (3) все существующие провайдеры переиспользованы, (4) ENGINE_ARCHITECTURE + SESSION_STATE оба обновлены — валидация ✅ не гарантирует это |
| **Self-Learning Infrastructure** | Не добавляет новый бизнес-домен — делает AI-инфраструктуру адаптивной для ВСЕХ существующих флоу: feedback loops, bandit routing, versioned experiments, promotion gates | Три вещи: (1) петля обратной связи замкнута — feedback → reward → policy update → следующий запрос; (2) MACHINE vs FREEDOM под самообучением: arm structure invariant, budget enforcement FAIL (не silent truncation), policy update ASYNC ONLY; (3) improvement suggestions: auto-applied или требуют human gate — проверить есть ли CF-правило REJECT, или только текст плана |
| **Design Intelligence / Perceptual** | Вход = структурный артефакт (Figma, ERD, diagram), не user action; AI используется для perception (понять что изображено), а не для генерации; внешняя ontology как mapping source, а не LLM-generated knowledge | Три вещи: (1) **Mandatory IR pipeline** — N последовательных трансформаций, каждая independent queryable (хранится в ES отдельно); средний слой нельзя пропустить — в нём создаётся evidence map для iron rules нижнего уровня; (2) **Evidence-anchored confidence** — confidence НЕ из LLM self-report; LLM self-score весит ≤20%, остальное — verifiable evidence (nodeId, компонент, текст); (3) **"Don't know" > "wrong"** — unclassified элементы НИКОГДА не auto-assigned; ambiguous (top-2 в пределах порога) → пауза и выбор пользователя; регрессия learning loop → rollback + human gate |
| **Infrastructure Bug Fix / New Fabric** | Не добавляет бизнес-домен. Признаки FLOW-35-типа: (a) Phase 0 = pure bug fix (0 новых артефактов); (b) исправляет нарушения в SHIPPED коде предыдущего флоу (EOL пакет, cross-provider SDK); (c) добавляет cross-cutting fabric (не domain-specific flow); (d) нет RENUMBER_MAP (сессия знала правильный canonical state). Признаки: "SDK migration", "dead package", "native SDK", новый fabric в fabrics/interfaces.py, bootstrap sequence в порядке dependencies | Шесть вещей: (1) **Phase 0 prerequisite** — bug fix обязан быть отдельной фазой (НОЛЬ артефактов) ДО любой feature; forbidden import list обновляется в Phase 0 чтобы предотвратить откат; (2) **Два типа SDK-нарушений**: Dead Package (EOL/archived repo) и Provider-Leaking (import SDK A чтобы звонить провайдеру B); оба требуют исправления до добавления нового провайдера; (3) **New Fabric vs New Domain Flow** — fabric добавляется в fabrics/interfaces.py и инициализируется в bootstrap ДО domain flows; нет flow template в FlowOrchestrator; (4) **ConfigBuilder reference syntax** — $secret:path и $env:VAR = MACHINE; TTL, provider selection = FREEDOM; resolver НИКОГДА не видит $secret: в final config; (5) **Bootstrap order = MACHINE invariant** — Secrets→Config→DB→Queue→AI→RAG→Flow — фиксирован зависимостями; Secrets/Config failure = ABORT; остальные = DEGRADED; (6) **SDK Hygiene BFA Gap** — проверить есть ли CF-правило против будущих EOL/cross-provider violations; если нет — "text-only constraint", архитектурное наблюдение |
| **Engine Self-Awareness / Pre-Phase Gate** | Движок добавляет инфраструктуру самодиагностики: capability-gap detection, trace failure analysis, skill lifecycle. Признаки FLOW-37-типа: (a) prerequisite = предыдущий domain flow ACTIVE; (b) `implementationMode: "hybrid"` с явным `bootstrapBoundary: "Phase A only"` и `afPipelineStartsAt: "Phase B"`; (c) все capabilities = flow topologies (не TypeScript-сервисы); (d) bootstrap boundary задокументирован как Architecture Decision (ADR); (e) skills dual-format (.md + RAG FLOW_DESIGN pattern); (f) gap signals → PromptOps loop | Четыре вещи: (1) **Bootstrap boundary check** — Phase A создаёт только: ES index + initial seeding + topology contract + ONE bootstrap skill; если Phase A создаёт TypeScript-сервис с логикой → Premature Manual Implementation Bug (v2 паттерн FLOW-37); (2) **Circular dependency scope check** — `implementationMode: "manual"` с обоснованием «circular dependency» верно ТОЛЬКО для absolute minimum bootstrap; если расширено на все фазы → anti-pattern; правильно: Phase A minimum, далее AF-pipeline; (3) **Skills dual-format** — каждый skill: `.md` файл (для Claude Code) + FLOW_DESIGN RAG-паттерн в `xiigen-rag-patterns` (для skill injection station); отсутствие RAG-паттерна = неполный skill, injection station не найдёт его; (4) **Downstream learning loop connection** — проверить: подключена ли `gap-signal-capture` topology к PromptOps / training data pipeline; если нет — self-awareness capabilities не улучшаются (text-only improvement) |
| **Self-Implementing Engine Workload** | Движок использует свой собственный AF pipeline для генерации своей же инфраструктуры. Признаки FLOW-36-типа: Phase 0 = manual bootstrap (человек пишет contracts + prompts); следующие фазы = движок генерирует Python/code через AF-1; Judge→Improve loop повышает quality score over time; meta-loop (generated tool benchmarks the models that generated it). Входные документы: Тип D (agent-executable plan) с ⛔ STOP маркерами. Может использовать Document Alias Substitution вместо технических ID | Пять вещей: (1) **Two-Track split** — Track A (человек) = только инфраструктура (Docker/ES/config), не код; Track B (движок) = весь code generation; смешение = architectural observation; (2) **Phase 0 bootstrap exception** — первые contracts + prompts вручную; явно отмечено как исключение, не норма; (3) **Meta-Loop human gate** — generated code НЕ становится training data без external validator; иначе self-referential bias; проверить: есть ли явный approval gate между generation и training capture?; (4) **Gradual Autonomy gradient** — каждая фаза реально менее зависима от человека (0%→30%→70%→90%); если последняя фаза требует столько же human review — gradient сломан; (5) **Cross-Document Artifact Numbers** — execution plans часто многоитерационны; «CRITICAL CORRECTION» в одном документе не гарантирует исправление в других; проверить каждый тип артефакта в каждом документе против canonical STATE |
| **Learning Loop Completion** | Движок получает долгосрочную память: DPO→RAG promotion, RAG quality evolution, calibration memory, known-fix seeder. Признаки FLOW-38-типа: (a) `implementationMode: "hybrid"` с `bootstrapBoundary: "Phase A only"`; (b) все 6+ loop capabilities = flow topologies через AF-pipeline; (c) петли обучения сами идут через тот же pipeline который реализуют; (d) event-driven triggering (не blocking calls из main pipeline); (e) FREEDOM config для ВСЕХ порогов (dpoPromotionThreshold, qualityScoreMinSamples, skillUpdateTriggerCount); (f) AI-generated pattern content (не template strings) | Четыре вещи: (1) **Loops must learn themselves** — если loop topology генерирует text/content через template string → Premature Manual Implementation Bug; text generation в loops = `ai-generate.handler` для возможности PromptPatch; (2) **False Circular Dependency check** — `implementationMode: "manual"` для ВСЕГО флоу с обоснованием «circular dependency» = anti-pattern; circular dependency верно только для Phase A bootstrap минимума (ES indices + FLOW_DESIGN seeds + event schemas); (3) **Event-driven decoupling** — loop triggers = события от main pipeline (phase.b.completed, training.triple.stored); main pipeline НИКОГДА не ждёт loop completion; failure в loop не блокирует генерацию; (4) **FLOW_DESIGN patterns accumulate** — FLOW-37 Phase A seeded 5, FLOW-38 Phase A adds 5 more; проверить: каждый loop добавляет соответствующие FLOW_DESIGN patterns в RAG (не только создаёт TypeScript) |
| **Flow Design Teaching / Local Model Curriculum** | Движок учится проектировать flow topologies (flow-design skills в RAG) + local model получает curriculum-ordered обучение с V9 teaching health gate. Признаки FLOW-39-типа: (a) `implementationMode: "hybrid"`, `bootstrapBoundary: "Phase A only"`; (b) Phase A засевает flow-design skills как RAG patterns с `curriculumTier` (1–5); (c) shadow runs mandatory (zero API cost); (d) V9 gate = mandatory teaching health check на каждом phase gate; (e) knowledge distillation: 3 code-free rules из каждого passing run; (f) fine-tuning threshold = curriculum-ordered count (40), не random count (80) | Пять вещей: (1) **Flow-vs-Service Gate (Step 0.5)** — каждая capability должна пройти Q1–Q4 test с записью `capabilityRouting[]` в STATE.json с reasoning; только Q4 bootstrap-list YES = MANUAL; (2) **V9 gate = BUILD_FAILURE для teaching health** — если DISTILLED_RULE не создан за phase, или DPO triple без chosen.code (gap item 08), или domain query returns 0 → phase completion BLOCKED; (3) **Shadow run output isolation** — local model output НИКОГДА не входит в main generation pipeline; shadow = measurement only; switch recommendation только при gap < threshold for 3 consecutive runs; (4) **Curriculum tier distribution** — Tier 1 (foundation) должен накапливаться быстрее чем Tier 5; проверить tier distribution в xiigen-local-curriculum, не только count; (5) **Fine-tuning milestone math** — 40 curriculum-ordered triples target = architecture decision с reasoning (педагогически эквивалентны 80 random); проверить: fine-tuning threshold в FREEDOM config, не hardcoded; switch recommendation = event → human gate, не auto-switch |
| **Domain Business Flow (Post-Learning Infrastructure)** | Первый domain flow строящийся ПОСЛЕ FLOW-37/38/39 ACTIVE. Движок прибывает уже «умным»: calibration memory, KNOWN_FIX в RAG, V9 gate, distillation pipeline, shadow runs, curriculum sequencer. Признаки FLOW-01-типа: (a) prerequisite: FLOW-37+38+39 ACTIVE (проверяются checks 9–13 в infrastructure gate); (b) 7-document execution architecture (Gate + A/B/C/D/E/F + Reference); (c) Phase B = самая изменённая (автоматические: distillation + shadow + compression); (d) V9 gate как BUILD_FAILURE на каждом phase gate; (e) Phase F создаёт LEARNING-HANDOFF.json для следующего flow в цепочке; (f) догфуд-стратегия (движок строит собственный продукт) | Пять вещей: (1) **Infrastructure Gate enrichment check** — checks 9–13 должны быть PASS ДО Phase A; особенно: ≥16 FLOW_DESIGN patterns с curriculumTier, 4 FLOW-39 ES indices, localModelEndpoint configured; если нет → STOP; (2) **Phase C conditionality gate** — SESSION-C.md должен иметь явный if/else: «если Phase B без PromptPatch → skip C → go to D»; отсутствие явного gate = риск unnecessary execution; (3) **V9-003 severity graduation** — на первом flow V9-003 = warning (qualityScore < 0.60 expected); на N+1 flows V9-003 должно стать BUILD_FAILURE; проверить есть ли transition rule в документации или только implicit; (4) **LEARNING-HANDOFF.json completeness** — проверить что handoff содержит: distilledRules + dpoTriples + shadowRunSummary + calibrationRecords + forNextFlowPlanning; без этого следующий flow стартует с нуля вместо baseline; (5) **Mock-factory adoption check** — Phase D должен использовать `createFabricMock()` (FLOW-37 pattern), НЕ ad-hoc mocks; ad-hoc мок для `storeDocument` без tenantId маскирует tenant scope isolation нарушения |
| **Pre-Canonical Domain Business Flow** | Первое поколение бизнес-флоу, разработанных до создания 7-файлового canonical формата. Признаки FLOW-02..FLOW-09 типа: (a) только functional spec (.md) + deep research (.md) — нет ENGINE_ARCHITECTURE, SESSION_STATE, TASK_TYPES_CATALOG как отдельных файлов; (b) нет F-номеров, T-номеров, BFA CF-номеров — только текстовые псевдонимы; (c) `flow_id: the {name} flow` вместо FLOW-N; (d) параллельные ветки + explicit join/convergence; (e) circuit breakers + graceful degradation (optional branches); (f) pre-canonical factory numbering — F244+ в каждом документе независимо (не глобальная последовательность); (g) MACHINE/FREEDOM разделение только implied; (h) Research Artifact вариант: нет базового spec-файла вообще (FLOW-08 паттерн — только research reports). | Пять вещей: (1) **Pre-canonical numbering alert** — F-номера в source docs НЕ часть глобальной последовательности; FLOW-09, FLOW-10, FLOW-11 все начинают с F244 для разных фабрик; canonical numbers только в ENGINE_ARCHITECTURE_MERGED; никогда не принимать F-номер из pre-canonical doc как canonical без верификации в MERGED; (2) **Alias table first** — строить таблицу перевода псевдонимов `the {name} flow → FLOW-N` ПЕРЕД любым cross-flow анализом; все prerequisite и dependency ссылки — текстовые; без таблицы кросс-референции сломаны; при alias collision (один псевдоним на несколько flows) — решать через контекст; (3) **Edge Cases as hidden engine requirements** — важные engine-level ограничения (debounce window, join policy ALL_REQUIRED/TIMEOUT_THEN_FALLBACK, retry semantics, sequencing constraints) документированы ТОЛЬКО в разделе Edge Cases, не в YAML/event chain; обязательно сканировать Edge Cases явно на engine-level constraints перед разработкой — они не дублируются в YAML; (4) **Engine Primitives inference** — EP-4 (Saga Compensation), EP-5 (Idempotency Fabric) могут быть implicitly required без явного именования в этих flows; инферировать из multi-step transactional patterns и data consistency requirements; explicit naming появится только в FLOW-09+; (5) **Research Artifact check** — проверить наличие базового flow-spec файла; если источники = только research reports без service inventory/event definitions/user stories → это Research Artifact (FLOW-08 паттерн); найти downstream flow который имплементировал findings (FLOW-08 → FLOW-15); не строить implementation plan на research artifact напрямую |
| **Wave -1 Infrastructure / Client Push** | Инфраструктурный флоу нулевого уровня — реализуется ДО всех user-facing flows; устраняет системный анти-паттерн (polling hell). Признаки FLOW-40-типа: (a) Wave -1 — нет prerequisite flows, запускается первым; (b) вводит новый fabric-интерфейс (ISseConnectionPool — 8-я фабрика); (c) Phase A manual bootstrap (fabric interface + default in-memory impl), Phases B–D через AF-pipeline; (d) Phases B–D ждут конкретный user-facing flow ACTIVE (для integration test scenario); (e) три task type с разными archetypes (ROUTING + DATA_PIPELINE + SCHEDULED); (f) 2 BFA rules: infrastructure domain MAY NOT emit в user-домены + tenant scope isolation | Четыре вещи: (1) **Fabric interface vs service** — ISseConnectionPool это контракт, не реализация; в Phase A создаётся только interface + default impl (InMemoryPool для dev/CI); production impl (Redis-backed) = отдельный swap; если Phase A создаёт сразу production impl — нарушение single-concern Phase A; (2) **DATA_PIPELINE vs PROCESSING для event bridge** — FlowEventBridge = DATA_PIPELINE (move + format), не PROCESSING (compute + transform); если event bridge классифицирован как PROCESSING → неправильный curriculum tier и архетип-специфичные тесты; (3) **BFA static grep enforcement** — CF rule «MAY NOT emit in user domains» = `violationCheck: "static-grep"`; это не runtime check; проверить что в spec явно указан тип проверки (static-grep, не тест-assertion); (4) **User-facing flow dependency для integration tests** — Phases B–D могут быть написаны через AF-pipeline, но integration test P-5 (SSE + реальный event) требует ACTIVE user registration flow; проверить что dependency задокументирована в STATE.json, не только в тексте плана |
| **Platform Adaptation with Heuristic Reasoning** | EXTERNAL_REPO флоу; расширяет Platform Adaptation паттерн (FLOW-41) до адаптера с собственной heuristic logic + spatial analysis. Признаки FLOW-42-типа: (a) ADAPTATION archetype (новый); (b) Layer 1 толстый (содержит N эвристик с confidence scoring, не только маппинг); (c) spatial/positional analysis в Layer 1 (containment, layout inference); (d) широкая FREEDOM surface (≥10 configurable thresholds — per-tenant tuning); (e) 5 sequential GAP-sessions как mandatory pre-phase (R1–R5); (f) standalone npm package test strategy (не server/ стубы); (g) 16+ arbiters (2x больше чем simple adapter) | Пять вещей: (1) **Layer 1 thickness check** — если Layer 1 содержит только маппинг (translateA→B) это Platform Adapter (FLOW-41); если содержит классификацию + spatial reasoning + heuristics → Platform Adaptation with Heuristic Reasoning (FLOW-42); разница влияет на test count (20+ только на classifier) и arbiter count (16 vs 8); (2) **Heuristic threshold bounds validation** — все N configurable thresholds должны иметь bounds документированные в FREEDOM config schema; отсутствие bounds = tenant может выставить 0/1 → degenerate behaviour; (3) **GAP-sessions dependency chain verification** — R1→R2→R3→R4→R5 строго ordered; R4 зависит от R2 (fixtures); R5 зависит от R3 (archetype in enum); нарушение порядка = broken named-checks или graph seeding на неправильные индексы; (4) **T-number namespace check** — при EXTERNAL_REPO: проверить является ли T-namespace глобальным или per-repo; если глобальный → проверить нет ли конфликта с другими flows в том же диапазоне; (5) **Spatial analysis test coverage** — edge cases: overlapping elements (partial containment), nested 3+ levels, zero-size elements; если тесты только «happy path» containment → SpatialAnalyzer не верифицирован на production data |
| **Multi-Tenant App Factory / Managed Primitives Platform** | Движок строит ПОЛНЫЕ tenant-приложения, а не только оркестрирует процессы. Признаки FLOW-15-типа: (a) ConversationBuilder→IntentCompiler→ResearchConsensus→AgentOrchestrator→CodegenPipeline→ManagedPrimitives chain; (b) Managed Primitives = factory-backed (5+ отдельных интерфейсов: DB/Storage/Auth/Email/Payments — каждый свапаемый); (c) multi-tenant isolation graduation (POOLED→SCHEMA→DEDICATED, нет backward migration); (d) rollback = immutable snapshot (EP-1) + EP-2 durable timer (7-day window); (e) 4-model research consensus (min 2 MACHINE = BUILD FAILURE); (f) CoW sandbox fork + subscription state machine + paywall gate (miss=deny) | Пять вещей: (1) **Managed Primitives provision order** — provision in dependency order (Auth до Email, DB до Storage); compensation = LIFO rollback — нет partial-provisioned app; (2) **4-model research min check** — min 2 из N моделей = MACHINE; если только 1 модель успела до timeout — ResearchFailed emitted, НЕ single-model accepted; (3) **Tenant isolation graduation** — POOLED→SCHEMA→DEDICATED; no backward migration (MACHINE); graduation требует: active AgentRun check + migration lock + cache invalidation; (4) **Rollback window = EP-2** — timer стартует ПОСЛЕ successful pre-rollback snapshot; timer survives restarts; нет snapshot = нет rollback (enforce в F257→F258 sequence); (5) **Paywall cache miss = deny** — кратковременный false-negative acceptable; default-allow при cache miss = MACHINE violation; audit every enforcement decision |
| **Visual Flow Authoring / User-Driven IDE** | Пользователь описывает intent визуально, движок генерирует factories/code. Отличается от Meta-Flow (FLOW-26): не engine-driven на gap detection, а user-driven через visual canvas. Признаки FLOW-18-типа: (a) IFlowDesignerService resolves to React/Angular/Svelte/Native via config (NEVER direct import); (b) GENERATED→TESTED→INJECTED→MINIMAL→CORE promotion ladder; (c) OT/CRDT conflict resolution для collaborative editing; (d) AST-level DNA validation на generated code; (e) hot code injection (FREEDOM: hot/cold/canary); (f) AF-11 self-improvement feedback loop; (g) Marketplace + fork lineage | Пять вещей: (1) **UI Fabric enforcement** — IFlowDesignerService = platform resolver; ни одна генерация кода не должна иметь `import React` / `import { Component } from '@angular/core'` напрямую; если нет CF rule — «text-only constraint»; (2) **GENERATED→CORE sequence** — CORE = human gate MACHINE; ST-176 (full cycle <1 hour) возможен только для простых nodes; complex nodes требуют manual review; проверить нет ли auto-CORE без human gate; (3) **OT/CRDT edge cases** — concurrent edit merge correctness (не только «no data loss»); deadlock-free merge path; fallback при convergence failure; (4) **External Git Sync SDK constraint** — никакого Octokit/GitLab SDK; generic HTTP only; если нет CF rule — drift risk; (5) **Self-improvement AF-11 connection** — AF-11 Prompt Ops должен получать generation outcomes; проверить есть ли явная event subscription (не poll); отсутствие = self-improvement не улучшается |
| **DevOps Control Plane / Self-Hosting Infrastructure** | Движок учится управлять собственным и tenant deployment. Признаки FLOW-19-типа: (a) CMDB = component descriptor YAML from Git webhooks (infra as code); (b) Environment Factory (local/PR-ephemeral/cloud через единый интерфейс); (c) IPipelineAdapterService = canonical contract (Jenkins/AzDO/GHA/GitLab normalised); (d) ReadinessReport = SOLE deployment gate; (e) DR drill mandatory — Tier-1: 7-day maximum interval или нельзя promote; (f) secrets in Git = BUILD FAILURE; IaC без compensation plan = BUILD FAILURE | Пять вещей: (1) **ReadinessReport completeness** — один gate для всей deployment chain; ReadinessReport должен охватывать: config-check + smoke + integration + performance + DR drill evidence; missing любого = FAIL; (2) **IPipelineAdapterService as config** — CI provider = config choice, не hardcoded import; если generated code имеет `import jenkins_client` → SDK hygiene violation (аналог F692 в FLOW-18); (3) **DR drill evidence immutability** — IDrillEvidenceService (F723) = WORM; restore drill completion = дата + SHA + duration + pass/fail; нет evidence = нет promotion, даже если drill прошёл; (4) **IHallucinationDriftService connection** — F727 = observability для AI output drift; проверить есть ли explicit subscription к learning loop (xiigen-calibration-memory); если нет — мониторинг AI drift не соединён с feedback; (5) **Tenant lifecycle saga idempotency** — onboarding/offboarding = multi-step saga; каждый шаг idempotent; audit logs survive offboarding (MACHINE); storage bindings survive in read-only mode для data retention |
| **Platform Adaptation / Ecosystem Adapter** | EXTERNAL_REPO флоу; адаптирует существующий pipeline под новый SDK/платформу без изменения Layer 2 (ядро). Признаки FLOW-41-типа: (a) ADAPTATION archetype (новый — может отсутствовать в enum); (b) 3-layer architecture: Layer 1 (shared SDK), Layer 2 (unchanged core engine), Layer 3 (thin adapter = единственное отличие); (c) Mock Writer Injection pattern (writer = `(payload) => Promise<void>` injected, не hardcoded); (d) C5-E equivalence proof тесты (платформа A == платформа B → одинаковый output из Layer 2); (e) npm package ready; (f) CALLBACK event model (не CloudEvents) | Шесть вещей: (1) **GAP-ARCH-01 check** — проверить есть ли ADAPTATION в `ContractArchetype` enum + `tierMap` в `feedback.handler.ts`; если нет → STATE.json использует string не из type system, tierMap defaults к 3 (неправильно, ADAPTATION = tier 2 = DATA_PIPELINE complexity); (2) **GAP-INFRA-01 check** — проверить наличие `xiigen-decision-graph` + `xiigen-planning-decisions` fixture JSON файлов в `fixtures/`; их отсутствие блокирует graph seeding для ВСЕХ flows с planning layer (не только адаптера); (3) **Layer 2 purity test** — Layer 2 (CSS engine) НИКОГДА не должен содержать import из platform SDK (ни Figma, ни Canva); если содержит → architectural violation; Layer 2 должен принимать только SharedElement[] / SharedStyle[] типы; (4) **Mock Writer Injection enforcement** — `writeToCanvas(enhanced, writer)` pattern: production writer = real SDK call; test writer = mock; если бизнес-логика напрямую импортирует `@canva/design.setContent` → нарушение; (5) **Equivalence proofs coverage** — C5-E тесты должны доказывать что один и тот же SharedElement из разных платформ даёт идентичный CSS output; если тесты только проверяют «адаптер не падает» (без сравнения с референсом Figma) → неполная проверка; (6) **Full-layout adapter scope check (W1 pattern)** — если адаптер обрабатывает layout-свойства (не только text), требуется ПОЛНЫЙ figma-types.d.ts (все SceneNode layout-типы), не text shims; при использовании text shims от C5 в layout-адаптере → тихие ошибки типов без compiler failures; если адаптер имеет Diff View (generated vs native CSS comparison) — алгоритм определения несовпадения (pixel-exact / unit normalization / ±N tolerance) = MACHINE decision, задокументировать ДО UI-сессии |

---

## ШАГ 7 — ОПРЕДЕЛЕНИЕ ГРАНИЦ ЗАДАЧИ

Перед синтезом ответить на три вопроса:

**1. Что именно просит эта задача?**
Не «что написано в файлах», а что конкретно нужно: построить (что?), запустить (что?), объяснить (что?).

**2. Что НЕ входит в задачу?**
Что соседнее / похожее, но другое — нельзя тащить в оркестрацию.

**3. Что нужно подтвердить до начала работы?**
Главный артефакт на входе — если его нет, остановиться и спросить.

---

## BEST PRACTICES FROM HIGH-QUALITY ORCHESTRATIONS (grok orchestrator folder)

After studying the strongest examples in `claude-sessions\grok  orchestrator` (especially 27, 33, 37, 38, 39, 41, 42), the following patterns consistently produce the most valuable, reusable scenarios:

### Pattern 1: Strong Before/After Framing
The best files start with a very clear "До FLOW-X / После FLOW-X" contrast. This is more powerful than just describing what the flow does.

**Recommendation:** Always include a short "До / После" block when the flow introduces a new capability or changes how the engine works.

### Pattern 2: Explicit "Why this revision" for key decisions
High-quality files don't just state the final architecture — they explain what was wrong in v1/v2 and why it was revised (e.g., "implementationMode: manual" extended too far → changed to hybrid with clear bootstrapBoundary).

**Recommendation:** When a plan contains revisions (especially around manual vs AF-pipeline boundaries), explicitly document the reasoning for the change.

### Pattern 3: Clear Manual vs Pipeline Boundary
The strongest orchestrations precisely define:
- What **must** be done manually in Phase A (and why the engine cannot do it itself).
- Where the AF-pipeline takes over.
- What the bootstrapBoundary actually protects.

**Recommendation:** For any flow that touches self-implementation or meta-capabilities, make the "manual bootstrap boundary" extremely explicit.

### Pattern 4: Memorable Analogies + Concrete Impact
Files like 33 use powerful analogies ("план строительства города") and then immediately ground them in what the user actually experiences.

**Recommendation:** Use 1 strong analogy per major new concept, then show the direct user/system impact.

### Pattern 5: Deep bug work stays mostly in protocol, not in the main text
In the best public-facing orchestrations, heavy bug analysis is summarized or moved to protocol extensions. The main text stays readable and focused on value.

**Recommendation:** In Forensic Mode, do the full bug hunting (Rule F2), but keep the main orchestration narrative clean. Put detailed bug findings in a dedicated "Что поймал протокол" section or directly into protocol updates.

### New Mandatory Section in High-Quality Output (for Library scenarios)
When producing scenarios for the permanent library, include these elements (inspired by the strongest files in the grok orchestrator folder):

1. Clear "До / После" framing (2-4 sentences)
2. Explicit explanation of the key architectural revision/decision and *why* it was made
3. Precise definition of the Manual Bootstrap Boundary (if the flow touches self-extension)
4. One strong, memorable analogy (optional but highly effective)
5. Short "Что это даёт" / impact paragraph for the user or the engine

These elements dramatically increase reusability and understanding.

---

## FORENSIC / HIGH-FIDELITY EXTRACTION MODE (Mandatory for Library Building)

**Purpose:** This mode exists because some models (including Grok) require stronger forcing functions than Claude to reach equivalent depth on forensic document analysis tasks.

When the goal is to produce high-quality, reusable orchestration scenarios for the library (not quick internal understanding), **Forensic Mode is mandatory**.

### When to activate Forensic Mode
- Building the permanent `orchestration-scenarios/` library
- User explicitly requests "Claude-level" or "protocol-max" quality
- The session involves canonical merged documents (ENGINE_ARCHITECTURE_MERGED, etc.)
- Previous normal-mode extraction was rejected as too shallow

### Mandatory Rules in Forensic Mode

**Rule F1 — Minimum Three Full Passes**
Before writing any orchestration, the analyst must perform at least three complete, documented passes over the material:
1. Pass 1: Normal understanding (what the flow does)
2. Pass 2: Bug-hunting pass (explicitly using the full Bug Catalogue below + new inconsistencies)
3. Pass 3: Grounding pass (locate and sample the actual canonical merged files on disk, not just the session transcript)

Each pass must be noted in the final "Как я анализировал" section.

**Rule F2 — Mandatory Bug Catalogue Check (before writing orchestration)**
The following bugs must be explicitly checked for (even if "nothing was found"). This list is the current minimum:

- Session-Isolation Renumbering (normal + Reverse + Bidirectional)
- Document Alias Substitution (normal + Total Alias Substitution in canonical STATE)
- Incomplete Merge Packaging
- Phase Ordering Deviation
- Stale Headers / Cascading Stale Headers in SESSION_STATE
- Premature Manual Implementation
- Cross-Flow Compatibility Table with incomplete baseline
- Any new inconsistencies between inventoried output vs actual files on disk

For each bug type: state "Checked — found / not found" + evidence.

**Rule F3 — Real Documents on Disk > Session Transcript**
- Never write the final orchestration using only the session transcript.
- You must locate the actual input files mentioned (Architecture.docx, XIIGEN_*_MERGED.md, FLOW-N-REFERENCE-PLAN.md, etc.) in the filesystem.
- Sample at least the headers and one key section of the largest canonical merged documents.
- Record the real paths used.

**Rule F4 — Grok Compensation Rules (specific for this model)**
When the analyst is Grok:
- Deliberately slow down. Do not synthesize early.
- After Pass 2, force yourself to find at least 2–3 new inconsistencies that were not obvious on first reading.
- Before writing the orchestration, do a "pedantry pass": look specifically for places where you want to be elegant/high-level and instead force more concrete artifact references and bug mentions.
- If you feel the urge to "make it nice and readable too early" — treat it as a warning sign that you are skipping forensic depth.

**Rule F5 — Protocol Extension is Expected Output**
In Forensic Mode, extending the protocol (adding new bug types, new flow categories, stronger rules) is not optional decoration — it is part of the deliverable when new patterns are discovered.

**Rule F6 — Architectural Shift Clarity (Claude-level requirement)**
Before writing the main orchestration, explicitly answer:
- What fundamental limitation of the previous system does this flow remove?
- What new class of problems can the engine now solve that it couldn't before?
- What new class of risks or responsibilities does this introduce?

This must be written in plain language in the final report (usually in section 1 or 2).

**Rule F7 — Manual Boundary Precision (Claude-level requirement)**
For any flow that touches self-implementation, meta-capabilities, or automation of previously manual work, the report **must** contain a clear, unambiguous statement of:
- Exactly what must still be done manually (and why the engine cannot do it yet)
- The precise boundary (e.g., "bootstrapBoundary = Phase A only")
- The justification for that boundary

Vague statements like "Phase A is manual for now" are not acceptable in Forensic Mode.

**Rule F8 — High-Signal Narrative Priority**
In the final orchestration report:
- Prioritize insight over completeness.
- The reader should understand *why this flow matters* for the long-term nature of the system within the first 30% of the document.
- Detailed bug lists, artifact tables, and forensic findings should be moved to a dedicated "Protocol Findings" or "Appendix" section unless they directly illustrate the architectural shift.

This is one of the main reasons many web versions feel more impressive — they protect the narrative clarity.

---

## СИМУЛЯЦИОННЫЕ ПАТТЕРНЫ — RUNTIME ARCHITECTURAL INVARIANTS

*Источник: обработка 20 высокоприоритетных сессий из `claude-sessions/Uncategorized/`, май 2026.*  
*Эти паттерны невидимы в статических flow-документах — они проявляются только при SIMULATION или Phase A execution.*

---

### 5-ЭТАПНЫЙ PIPELINE РАЗРАБОТКИ ФЛОУ (MACHINE — строго sequential)

```
FLOW-PREP (19 раундов) → SIMULATION (35 раундов) → GAP-PREP → GAP-TRANSLATE → Claude Code
```

**Нельзя запускать параллельно. Нельзя пропускать этапы.**

| Этап | Что делает | Блокировано до |
|------|-----------|---------------|
| FLOW-PREP | Готовит session files (R0–R19) | — |
| SIMULATION | Gap discovery до Phase A (Groups E/A/B/C/D) | FLOW-PREP complete |
| GAP-PREP | Классифицирует gaps в gap list | SIMULATION complete |
| GAP-TRANSLATE | Конвертирует gaps в Claude Code fix sessions | GAP-PREP complete |
| Claude Code | Выполняет fixes | GAP-TRANSLATE complete |

**FLOW-PREP Раунды:**
R0=STATE init | R1=Graph seeding | R2=NODE grades | R3=Infra gates | R4=CloudEvent verify | R5=Named checks | R6=ANALYTICS archetypes | R7=Phase B routing | R8=Phase C calibration | R9=Phase F edges | R10=Arbiter promotion | R11=Genesis triples | R12=NODE correlation | R13=Cross-flow deps | R14=Learning handoff | R15=VALIDATED confirm | R16–R19=Wave gate

**SIMULATION Gap Classes:** H (infrastructure) | J (graph/routing) | K (interface) | L (persistence) | M (RAG/curriculum) | N (named checks) | A/B/G (scenario-specific)

---

### H0 CADENCE — MACHINE ПРАВИЛО

**Одна сессия = один файл.** После каждого раунда: present → ⛔ STOP → ждать явного "continue."  
**Нарушение** = немедленная коррекция. Это не preference — это session protocol.

**Session file naming:** `FLOW-{XX}-{STAGE}-R{N}.md` — canonical.  
**НИКОГДА:** `SESSION-` prefix, копирование имён из uploaded templates.  
**Причина drift:** context loss после conversation compaction → регрессия к template naming. Фиксируется в session memory явно.

---

### КРИТИЧЕСКИЕ SIMULATION BUGS (системные классы)

| Gap | Тип | Описание | Флоу |
|-----|-----|---------|------|
| **DPO Triple Type Mismatch** | E-11 | `determineDpoQuality` ожидает `chosen: {code: string}`, `feedback.handler` пишет `chosen: string` → все тройки INVALID навсегда | FLOW-12 |
| **BootstrapCycleRouter Not Injected** | E-1 | Registered in DI но не injected в `GenericNodeExecutor` → весь graph-informed routing bypassed | FLOW-12 |
| **ROUTING_DECISION Never Written** | E-2 | Phase F graph learning iterates empty list → graph learning не работает | FLOW-12 |
| **Fixture Index Name Mismatch** | E-7 | Fixture создаёт не тот index name vs. что запрашивает service | FLOW-12 |
| **figmaNodes API Boundary Discard** | B3 | figmaNodes discarded at API boundary → Figma-to-Code value proposition structurally unfulfilled | FLOW-11 |
| **Learning Feedback Loop Broken (4 места)** | B | PersistentFeedbackStore in-memory only; human feedback never reaches prompt; ratings not connected to passed flags; evolved prompts never applied | FLOW-11 |
| **Empty Constraints CONVERGENCE FAIL** | — | Empty `constraints[]` array в NODE triggers CONVERGENCE_MISSING_CONSTRAINTS | FLOW-01 |
| **Named Checks Missing in NAMED_CHECKS** | E-6/N | Check IDs referenced in contracts absent from NAMED_CHECKS registry | FLOW-12 |

**Для каждого из этих bugs:** проверить при extraction из любого flow с соответствующими компонентами.

---

### 8 ENTRY TYPES (полная taxonomy)

| Тип | Описание |
|-----|---------|
| HTTP | Synchronous REST endpoint |
| EVENT | Async event consumer |
| DUAL | HTTP + Event dual entry |
| INLINE | Инлайн выполнение в родительском flow |
| INLINE-PURE | Инлайн без side effects |
| SCHEDULED | Cron/timer triggered |
| REQUEST_RESPONSE | Synchronous request-response (не REST) |
| HYBRID_SYNC_ASYNC | Sync accept + async processing |

**Ранее документировалось 4–5 типов. Полная taxonomy = 8.**

---

### 4 MACHINE PATTERN TYPES

| Тип | Описание |
|-----|---------|
| `constant` | Значение фиксировано, FREEDOM config не меняет |
| `formula` | Вычисляется детерминированно из inputs |
| `ordering` | Порядок шагов фиксирован (sequence constraint) |
| `state_machine` | Конечный автомат с MACHINE-переходами |

---

### NODE PRIMITIVE — ОСНОВНОЙ ПРИМИТИВ ЦИКЛА

| Поле | Тип | Назначение |
|------|-----|-----------|
| `structure` | object | Структурное описание что строится |
| `intent` | string | Цель/намерение узла |
| `constraints` | array | **НИКОГДА пустой** → CONVERGENCE_MISSING_CONSTRAINTS |
| `quality` | number | Порог качества (≥ 0.85 = MACHINE) |

**Вердикты:** LEAF (терминальный, идёт в Cycle 4) | EXPAND (рекурсивный, порождает sub-flow)

**4-Cycle Chain Pipeline:**
1. Cycle 1: Intent → Plan (PlannerHandler + Plan Reviewer)
2. Cycle 2: Plan Step → NODE (3-model parallel, blind judging, challenger isolation)
3. Cycle 3: Depth Decision (LEAF/EXPAND, max depth = 3 — MACHINE)
4. Cycle 4: Leaf NODE → Executor (AF pipeline)
5. Cycle 5 (Meta): Meta-Arbiter на grade < 0.85

**3 Teaching Channels:** CODE_REASONING | DESIGN_REASONING | CONVERGENCE_REASONING

---

### ARCHITECTURAL INVARIANTS ИЗ СЕССИЙ

| Правило | Источник | Категория |
|---------|---------|----------|
| `scope_isolation` count ≥ **4** (one per node), НИКОГДА ≥ 1 | Xiigen value proposition | MACHINE |
| **CF-POLICY-01**: absent `knowledgeScope` defaults to PRIVATE (never GLOBAL) | FLOW-01 testing | MACHINE |
| `flowId.keyword` (не `planId.keyword`) для DESIGN_REASONING RAG queries | Dev platform roadmap | MACHINE |
| `taskTypeCycleBudgets` — pre-populated в gate STATE.json, НИКОГДА `{}` | Dev platform roadmap | MACHINE |
| SEED-N+4 всегда с `&size=30` | Value proposition | MACHINE |
| Rollback = `upsertEdge` напрямую (governance override, bypasses decay window) | Value proposition | MACHINE |
| **Commit Blast Radius**: GOVERNANCE + SOURCE нельзя смешивать | Figma skills | MACHINE |
| **D-34-1**: FLOW-34 handles MODE-B-thin only; enterprise = FlowBundle guidance, not runtime | Self-learning | MACHINE |
| **COLD START fallback**: по мере роста corpus → upgrade reference doc | Dev platform | MACHINE |

---

### PATTERN TRANSFER RATES — МЕТРИКА

| Тип паттернов | Transfer Rate | Примечание |
|--------------|--------------|-----------|
| Structural (SETNX, DNA-8, MACHINE ordering, PLATFORM-ONLY) | ~100% | Cross-flow invariants |
| FLOW-12 (subscriptions) | 85% | Наиболее переиспользуемый |
| Средний domain flow | 50–70% | Зависит от domain similarity |
| FLOW-34 (meta-generation) | 33% | Fundamentally different category |
| Domain-specific patterns | ~0% | Правильно, не должны переноситься |

**Значение:** При планировании нового flow → сначала проверять structural patterns (100% transfer), затем close-domain flows. Meta-flows требуют отдельного analysis.

---

### АРХЕТИП TEMPLATE REGISTRY — ЭВОЛЮЦИЯ

| После флоу | Размер |
|-----------|--------|
| Through Wave 3 (FLOW-01..FLOW-13) | 20 архетипов |
| After FLOW-14 (ETL) | 21 |
| After FLOW-17 (marketplace) | 22 |
| After FLOW-19 (DevOps → REQUEST_RESPONSE) | 23 |
| After FLOW-20 | **33** |

**Новые архетипы требуют:** обновления `ContractArchetype` enum + `tierMap` в `feedback.handler.ts` + SIMULATION re-run для flows использующих новый архетип.

---

### FLOW REFERENCE PLAN — ОБЯЗАТЕЛЬНЫЕ СЕКЦИИ

Из обработки FLOW-18 через FLOW-34: следующие секции системно отсутствовали в pre-review планах:

| Секция | Критичность |
|--------|------------|
| ARBITER COUPLING ANNOTATIONS table | HIGH — без неё нельзя проверить arbiter wiring |
| machineFreedom guidance | HIGH — MACHINE vs FREEDOM constraints не явные |
| STATE.json version label | MEDIUM — v2 label на v3 документе |
| downstream + collisions fields | MEDIUM |
| promotionLevel в Phase B metrics | MEDIUM |
| WHAT CHANGED section | LOW |
| SESSION-0 CHECKLIST | LOW |
| KEY FACTS section | LOW |
| Wave assignment (не collapsed) | MEDIUM |
| Executable bash в Phase B loops (не prose) | HIGH |

**Flow-specific structural notes:**
- FLOW-24: last user-facing flow, only 8 task types (smallest AF batch)
- FLOW-32: PLATFORM-ONLY count в spec ≠ в list — проверять
- FLOW-34: intentional placeholder IDs (T-[+0]) — NOT bugs, preserve them

---

### PLANNING SKILLS LIBRARY — МИНИМАЛЬНЫЙ НАБОР ДЛЯ SIMULATION

12 обязательных skills загружать ПЕРЕД каждой simulation сессией:

| SK | Навык | Когда |
|----|-------|-------|
| — | HOW-TO-USE-SKILLS | До всего |
| SK-441 | simulation-protocol | Любой simulation step |
| SK-432 | root-cause-ladder | Любой gap analysis |
| SK-434 | solution-scope-gate | Session design |
| SK-442 | arbiter-panel-design | Sessions с арбитрами |
| SK-443 | session-file-authoring | Написание session files |
| SK-510 | four-tier-decision | Decision classification |
| SK-511 | graph-entity-schema | Graph entity design |
| SK-512 | confidence-lifecycle | Confidence seeding |
| — | plan-review | Перед delivery |
| — | flow-implementation-guide | Перед session structure |
| — | flow-restructure | Перед session structure |

**Версия v2.8.0:** 86 skills across 4 layers. Загружать по имени файла, не по SK-числу.

---

### XIIGEN PRODUCT VARIANTS — IMPACT НА FLOW EXTRACTION

| Вариант | Режим | Что значит для протокола |
|---------|-------|------------------------|
| xiigen-saas | MODE-A | Flows имеют multi-tenant isolation requirements |
| xiigen-oss | MODE-B-full | Flows самодостаточны, без hosted platform |
| xiigen-enterprise | MODE-B-full | FLOW-34 = FlowBundle guidance, не runtime |
| xiigen-lean | MODE-B-full | Python + React Native stack |

**При анализе flow**: проверить указан ли `productScope` ("client-capability" vs "xiigen-capability") — влияет на Feature Registry architecture.

**Rule F9 — Ruthless Narrative Purity (Critical for "Purer" Reports)**
The single biggest reason some reports feel dramatically cleaner and more valuable is **deliberate removal of noise** from the main text.

Key practices observed in the highest-quality examples:
- Remove most F-numbers, T-numbers, IR-XXXX, and specific factory IDs from the main narrative (mention counts and families instead).
- Do not list every iron rule or design record inline.
- Keep the main report focused on logic, sequence, and principles.
- Move raw technical details to an appendix or "raw analysis" section when the user asks for it.

**Rule F10 — Separation of Concerns (Report vs Analysis Process)**
- The main orchestration report should be written for a human reader who wants understanding and insight.
- The detailed "how I analyzed the files" process (file inventory, cross-referencing, bug hunting steps) should only be shown when explicitly requested, and even then preferably in a separate section or as a follow-up.
- Never pollute the primary narrative with the forensic process unless it directly serves the reader's understanding.

**Rule F11 — Recommended High-Purity Structure (Proven in Top Reports)**
For the cleanest, most reusable scenarios, follow this proven flow:

1. Strong visionary / problem framing (what fundamental limitation is being solved).
2. High-level orchestration overview (mental model + zones).
3. Step-by-step functional description (what actually happens in each stage).
4. Key design decisions and principles (especially the "why" behind hard constraints and revisions).
5. Grounded reality (what was actually built in the session + major findings).

Only after the above, add "Protocol Findings / Bugs Discovered" if relevant.

This structure consistently produces reports that feel significantly "purer" and more insightful.

### Pre-Orchestration Validation Checklist (must be passed before writing ШАГ 8)

Before moving to synthesis, confirm:
- [ ] Real canonical merged files located and sampled on disk
- [ ] All known bug types from the catalogue explicitly checked
- [ ] At least 3 full passes completed and documented
- [ ] Any new bug patterns discovered have been added to the protocol
- [ ] Numbers in the orchestration match either the merge plan or the actual files on disk (not just the session summary)

Failure to pass this checklist means the extraction is not yet in Forensic Mode.

---

## ШАГ 8 — СИНТЕЗ

### Нарративный порядок

Перед написанием выстроить описание в строгом порядке:

```
ЗАЧЕМ → КАК ВЫГЛЯДИТ → КАК УСТРОЕНО → В КАКОМ ПОРЯДКЕ ДЕЛАЛИ
```

| Часть | Вопрос | Источник | Слой |
|-------|--------|----------|------|
| ЗАЧЕМ | Какую проблему решает | MASTER_PLAN no-code + input Layer 1 | Layer 1 |
| КАК ВЫГЛЯДИТ | Сценарий для пользователя | Task Types ENTRY+PURPOSE + зоны в UX-порядке | Layer 1–2 |
| КАК УСТРОЕНО | Технические решения, принципы | DD/DR + MACHINE/FREEDOM | Layer 2 |
| В КАКОМ ПОРЯДКЕ ДЕЛАЛИ | Фазы/зоны + save points | MASTER_EXECUTION_PLAN / P0_PLAN PART 3 | Layer 3 |

Читатель который не понимает ЗАЧЕМ — не поймёт КАК. Не перепрыгивать через часть.

### Правило упрощения

Убрать: технический жаргон который не меняет понимание (сигнатуры, DNA-номера, названия классов).

Оставить: принципы поведения (idempotent, resumable, fabric-first, human gate) — они объясняют ПОЧЕМУ система устроена именно так.

Тест: «Если убрать это слово, изменится ли понимание?» — если нет, убрать.

### Осознанный список удалённого

| Что убрать | Почему | Что оставить вместо |
|------------|--------|---------------------|
| Таблицы фабрик и F-номера в теле текста | Детали реализации, не смысл | Упомянуть количество («44 фабрики») |
| Технический жаргон где есть простое слово | Потеря читателя | «saga с откатом» вместо «compensating transaction» |
| Повторяющийся контент из разных файлов | Лишний объём | Только самую свежую полную версию из files.zip |
| Граничные случаи в основном потоке | Засоряют красную нить | Вынести в «Ключевые ограничения» |

Это выбор уровня детализации. Детали остаются доступными: «если хочешь, могу углубить X».

### Объяснение процесса по запросу

Когда пользователь спрашивает «как ты анализировал» — ответить той же дисциплиной:
```
1. Что получено на входе
2. Какие файлы — исходный input, какие — произведённые/текущее состояние
3. Как просканировал главные блоки
4. Как синхронизировал актуальные номера/состояние
5. Как извлёк красную нить
6. Что сознательно отфильтровал как низкоуровневый шум
7. Как проверил согласованность перед написанием ответа
```

### Запрещённые формулировки

| Запрещено | Почему | Правильно |
|-----------|--------|-----------|
| «infrastructure-discovery-skill: найти файлы» | Описывает инструмент, не действие | «Открыть styles.ts, выпиши каждую функцию которая берёт node property» |
| «G0-G7: планирование» | Описывает фазу фреймворка | «Проверить что диапазон F631-F640 не пересекается с F1-F630» |
| «Валидировать план» | Нет объекта, нет результата | «Сверить 7 документов плана с оракулом → если пересечение — откат» |
| «Проанализировать входящие данные» | Непонятно что делать | «Прочитать ENGINE_ARCHITECTURE.md, выписать: зачем + что делает + куда пишет» |

### Структура шага

```
N. [Глагол действия] [конкретный объект] → [измеримый результат]
```

Работающие глаголы: открыть, прочитать, выписать, проверить, сравнить, записать, запустить, заблокировать, вернуть.  
Нерабочие: проанализировать, обработать, использовать, применить, рассмотреть.

---

## ШАГ 9 — ФОРМАТ ВЫВОДА

```markdown
# Оркестрация: [Название]

## Что мы строим
[1-2 предложения: что строим, для кого, зачем. Без жаргона.]

## Что это даёт
[5-7 конкретных возможностей. Без F-номеров.]

## Главная новая возможность
[Что нового/сложного именно здесь — чего не было раньше.]

## [Аналогия]
[Одна метафора которая объясняет систему без технических слов.]

## Как работает движок (контекст) — необязательно
[Только когда FLOW строит целую платформу и читатель может не знать XIIGen.
1. Из каких fabrics состоит движок (одной строкой каждый)
2. DNA-правила как принципы (не запреты)
3. Что такое фабрика / task type
НЕ добавлять если FLOW расширяет отдельный домен — контекст уже известен]

## Архитектурный мост — только для систем с зонной декомпозицией

**Принцип деления:** «Зоны созданы по принципу X»

| Зона | Семейство | Что решает (бизнес → тех) |
|------|-----------|---------------------------|
| A    | 119       | ... |

**Ключевые Design Decisions (3-5 штук):**
- «[Бизнес-требование]» → DD-NNN: [Архитектурное правило]

## Как это выглядит для пользователя
[Пошаговый сценарий по зонам в UX-порядке.
Каждый шаг строится из ENTRY+PURPOSE task types.]

## Как [система] строит это (BUILD TIME)

| Фаза | Что делается | Что получается | Продолжительность |
|------|--------------|----------------|-------------------|
| P0   | Планирование, RAG-индекс | FLOW{N}_P0_PLAN.md | 10 мин |
| P1   | ENGINE_ARCHITECTURE | N фабрик + M семейств | 20 мин |
| P2   | TASK_TYPES_CATALOG | K task types + шаблоны | 25 мин |
| P3   | SKILLS_FACTORY_RAG | навыки SK-XXX–SK-YYY | 15 мин |
| P4   | BFA_STRESS_TEST | CF-правила + стресс-тесты | 20 мин |
| P5   | UNIFIED_SOURCE_INDEX | DD + DR + кросс-ссылки | 15 мин |
| P6   | MASTER_EXECUTION_PLAN | единый план выполнения | 10 мин |
| P7   | SESSION_STATE | обновление глобальных счётчиков | 5 мин |

[AF-конвейер: AF-1 генерирует → AF-4 ищет паттерны → AF-5 мульти-модель → AF-6..9 проверяют → AF-10 объединяет → AF-11 учится]

## Как работает готовая система (RUNTIME) — только если применимо
[Flow Templates — DAG'и бизнес-процессов: Название → Шаг1 → ... → Результат]
[Триггеры: API / очередь / таймеры]
[Повторяющийся loop: Цикл (для каждой X) ├── A ├── B └── C]

## Основной бизнес-процесс
[Главный процесс на человеческом языке, без ID. 7-10 шагов.]

## Железные гарантии
[Правила как ОБЕЩАНИЯ системы — что пользователь ПОЛУЧАЕТ.
Формат: [Что гарантировано] — [за счёт чего]
Пример: «Деньги не спишутся дважды — за счёт idempotency key на уровне БД»
MACHINE из task types → сюда. Запреты переформулировать от позитива.]

## Новые паттерны (архетипы) — если применимо
[НАЗВАНИЕ_ПАТТЕРНА — одно предложение что это и почему важно]

## Почему такая оркестрация работает — необязательно
[Зачем так лучше хаоса. Аргументы от ценности, короткие пули.]

## Итоговое состояние
[Цифры из SESSION_STATE: F___ / T___ / CF___ / SK___, статус завершения]

## Что поймал протокол — обязательная секция
[Документальные несоответствия обнаруженные при анализе. Три категории:]

**Рассинхроны между файлами:**
- Family NNN: SESSION_STATE говорит «[Зона X]», ENGINE_ARCHITECTURE показывает «[Зона Y]»
- Фабрики верны, имена переставлены → editorial bug

**Редакционные артефакты:**
- Старые заголовки FLOW-N в merged-документах (остались после copy-paste)
- Устаревшие template ID внутри JSON-примеров
- Нумерационные коллизии (input-нумерация смешана с canonical)

**Архитектурные наблюдения:**
- Что в документах описано как FREEDOM, но выглядит как MACHINE (или наоборот)
- Отсутствующие cross-ссылки между DD и factory

Если несоответствий нет — написать явно: «Документальных несоответствий не обнаружено».
Не пропускать секцию даже если всё в порядке — отсутствие проблем тоже результат.

## Что дальше
[Следующий разрешённый шаг — только в пределах доказательств]

## [Оффер]
Хочешь, могу: [вариант 1 / вариант 2 / вариант 3]
```

---

## ШАГ 10 — ПРОВЕРКА ПЕРЕД ОТПРАВКОЙ

### Главный вопрос

> **Оркестрация по этапам правильно построена и ничего не упускает?**

Всё остальное — следствие этого вопроса.

### Чеклист полноты цепочки этапов

**Цепочка замкнута:**
- [ ] Есть ли явный первый триггер? («Пользователь отправил форму», «Система обнаружила gap», «Разработчик предложил изменение»)
- [ ] Каждый этап производит что-то конкретное — что нужно следующему?
- [ ] Последний этап даёт финальный результат — понятно ли что это результат?
- [ ] Нет ли «слепого пятна» — этапа который подразумевается, но не описан?

**Каждый этап честен:**
- [ ] Этап делает то, что про него написано? (не «контекст» описывает arbitration, а arbitration описывает arbitration — как в баге FLOW-25)
- [ ] Если этап говорит «проверяет», «блокирует», «генерирует» — это реально происходит, не просто объявлено?

**Гарантии настоящие:**
- [ ] Каждая MACHINE-гарантия действительно не обходится — нет другого места в описании где она молча отменяется?
- [ ] Escape hatch (FORCE_PROCEED и подобные) описан полностью: что даёт, что логирует, что всё равно заблокировано?
- [ ] Если есть «всегда CRITICAL» — это действительно всегда, или есть условия?

**Картина полная:**
- [ ] Описаны ли все флоу/системы которые этот флоу затрагивает (защищает, ломает, расширяет)?
- [ ] Описано ли что происходит при сбое / timeout / retry exhaust — или только happy path?
- [ ] Если флоу управляет другими флоу — перечислены ли они явно?

**Читабельность:**
- [ ] Написано ли «Мы строим X который позволяет Y делать Z»? — если нет, оркестрация не понята
- [ ] Читается ли как рассказ, не как выжимка таблиц?
- [ ] Написано ли «завершено» без доказательства? → убрать или добавить ссылку на validation

**Документальные несоответствия:**
- [ ] Имена зон в описании совпадают с тем что эти зоны реально делают? (не только с тем как называются в SESSION_STATE)
- [ ] Глобальные счётчики взяты из САМОГО ПОЗДНЕГО блока SESSION_STATE, а не из заголовка файла?
- [ ] Этот флоу присутствует в FLOW REGISTRY в самом позднем блоке SESSION_STATE?
- [ ] Если первый заголовок файла — устаревший FLOW-N, найден ли более поздний блок в конце файла?
- [ ] Если completion report говорит ✅ на SESSION_STATE — это проверено чтением конца файла, а не принято на веру
- [ ] Есть ли секция «Что поймал протокол» — обязательна, даже если пустая

### Симуляция — проиграть цепочку

Взять первый триггер и мысленно пройти по этапам до конца:
```
[Триггер] → [Этап 1: что получил, что произвёл] → [Этап 2: ...] → ... → [Результат]
```
Если на любом переходе непонятно что входит или что выходит — этот переход не описан.

Если хотя бы один вопрос «нет» → найти пропущенный этап или уточнить существующий.

### Чеклист отклонения

Отклонить если любое из следующего верно:

**Стартовые проблемы:**
- Начинается с номеров, файлов, классов вместо человеческого продукта
- Нельзя написать одно предложение «Мы строим X»
- Перечисляет компоненты без объяснения процесса между ними

**Проблемы содержания:**
- Не называет главную новую возможность
- Не объясняет что из чего получается
- Не показывает как проверяется результат
- Не упоминает текущее состояние
- Игнорирует input-файлы или произведённое состояние

**Проблемы этапов:**
- Цепочка этапов обрывается — непонятно что происходит после этапа N
- Этап описан не тем что делает (зона A описывает работу зоны B)
- Happy path без упоминания что происходит при сбое / retry / timeout
- Гарантия заявлена, но в другом месте молча обходится

**Проблемы источников:**
- Описание зоны противоречит тому что эта зона реально делает в ENGINE_ARCHITECTURE
- Говорит «завершено» без validation-доказательства
- Игнорирует историю сессии когда оркестрация родилась из множества коррекций
- Не может назвать роль каждого крупного файла
- Не говорит что было сознательно отфильтровано

**Проблемы специальных типов:**
- Адаптеры: нет объяснения что переиспользуется и что новое для платформы; нет таблицы сравнения для нескольких адаптеров
- Мульти-фича: не разделена definition-time от runtime оркестрации
- Event-driven: нет ни одного конкретного пути события от действия к результату
- Системные флоу: не объяснён повторяющийся паттерн release train
- Опущены save points, recovery commands, canonical merge, SESSION_STATE update
- Сообщаются глобальные счётчики без объяснения что конкретно добавляет флоу

**Проблемы финансовых / контрактных:**
- Нет runtime lifecycle до factories/task IDs
- Idempotency, ledger, escrow, KYC, evidence capture, timers, dispute — побочные детали вместо гарантий

**Проблемы миграции:**
- Миграция базелайна и новая возможность флоу слиты в один нарратив
- Не объяснено почему смена стека меняет что можно предполагать в будущем

**Проблемы регулируемых платформ:**
- Нет: consent, targeting, auction latency, attribution, quotas, billing, policy, tenant isolation
- INVENTORY/SYNTHESIS/JUDGMENT свёрнуты в «AI генерирует код»
- Freedom/config значения скрыты как hardcoded детали

**Проблемы форм / no-code:**
- Нет user/data lifecycle до зон и семейств
- Зоны не объяснены почему следуют lifecycle

**Проблемы визуального редактора:**
- Нет пользовательского journey authoring/publishing от начала до конца

**Проблемы расширений:**
- Флоу описан как standalone без baseline+delta истории
- Старые ID input-файлов использованы без ренумерации

**Проблемы обучения / тьютора:**
- Нет замкнутого цикла: diagnosis → teach → test → reward → adapt → schedule → recover
- Consent, safety, calendar sync, grading, gamification, adaptive planning — отсутствуют

---

## АНТИ-ПАТТЕРНЫ

**Описание формата вместо содержания:**
> «Применить planning-skill с фазами G0-G7»
→ Описание инструмента. Нужно что происходит внутри.

**Перечисление артефактов без действий:**
> «F631:IBootstrapService, F632:IPlanBundleIngestionService...»
→ Реестр. Нужно что они делают и в каком порядке.

**Один большой шаг без декомпозиции:**
> «Реализовать семейство через 5 арбитров»
→ Разбить: загрузить спецификацию → собрать ContextPack → запустить генерацию → прогнать 5 арбитров параллельно → если <4/5, retry → если ≥4/5, продвинуть в INJECTED.

**Шаг без результата:**
> «Проанализировать deep research файлы»
→ «Прочитать deep research 2-8.md, выписать: главный цикл bootstrap, где пишется сентинел, что при краше»

**Номера из старых версий:**
> «F1419-F1428» когда SESSION_STATE говорит F631-F640
→ Сверить с SESSION_STATE и RENUMBER_MAP прежде чем использовать.

**Нет «первого нового» — всё «добавлено»:**
> «Добавлены фабрики, добавлены задачи, добавлены правила...»
→ Выделить что появилось ВПЕРВЫЕ в этом флоу и не было раньше.

**Прозой то что лучше таблицей:**
> «Первый тип делает X, второй Y, третий Z...»
→ Если компонентов 3+ → таблица: Зона | Что делаем | Результат | Почему важно

**Слияние двух уровней:**
> «FLOW-19 добавил DevOps CI/CD и переехал на Node.js...»
→ Две разные истории. Описать по очереди, показать связь.

**Нет аналогии — только техника:**
> Текст содержит только «фабрика», «fabric», «arbiter», «DAG»
→ Добавить хотя бы одно сравнение с реальным миром.

**DD перечислены без перевода:**
> «Design Decisions: DD-180, DD-181, DD-183...»
→ «"Никаких потерянных данных" → DD-184: Persist-before-emit» — тогда понятно ПОЧЕМУ.

**Зоны без принципа:**
> «Система разбита на 9 зон: A (Forms), B (Runtime)...»
→ Добавить: «Зоны следуют lifecycle: создание (A,B) → сбор данных (C,D) → доставка (E,F) → автоматизация (G,H) → изоляция (I)»

**Расширение флоу без baseline:**
> «FLOW-23 добавляет Visual Editor Extended...»
→ Начать с: что уже было в FLOW-22, что добавляет FLOW-23, почему это не standalone.

**Первый заголовок SESSION_STATE принят за актуальное состояние:**
> «SESSION_STATE показывает Post FLOW-26, значит мы на FLOW-26»
→ SESSION_STATE дополняется, а не перезаписывается. Первый заголовок устаревший. Читать конец файла — там самый поздний блок с реальными счётчиками.

**Stale header накапливается через несколько флоу (Cascading Stale Header):**
> «Заголовок SESSION_STATE отстаёт на 4 флоу — FLOW-27/28/29/30 добавились, заголовок всё ещё "Post FLOW-26"»
→ Стейл-заголовок не всегда на один флоу. Проверить ГЛУБИНУ отставания: взять заголовок системного блока → взять последний флоу в FLOW REGISTRY → разница = сколько флоу никогда не обновляли заголовок. При глубине ≥2 это cascading stale header — требует dedicated consolidation pass по всем 7 canonical файлам, не точечного патча.

**Completion report ✅ принят как истина без проверки SESSION_STATE:**
> «FLOW-27 завершён — completion report показывает 33/33 PASS»
→ Validation считала ссылки на F-номера в файлах. Но SESSION_STATE может сохранять заголовок предыдущего флоу и не иметь этого флоу в FLOW REGISTRY. Проверять SESSION_STATE независимо: заголовок + реестр + счётчики.

**Engine Primitive описан как ещё один домен:**
> «FLOW-27 добавляет Human Interaction Gate для управления задачами»
→ EP-N не домен и не фича — это новая шестерня в двигателе. Объяснить: что теперь умеет каждый флоу автоматически, что до EP было невозможно, что не сломалось в FLOW-01–FLOW-N.

**«AI генерирует код» вместо INVENTORY/SYNTHESIS/JUDGMENT:**
> «AF-станции используют AI для генерации»
→ Три отдельных движка: сбор контекста → генерация → независимая проверка. Оркестрация = их разделение.

**MACHINE-инвариант защищён только текстом плана, не BFA-правилом:**
> «Improvement suggestions никогда не применяются автоматически» (написано в MASTER_EXECUTION_PLAN P5.3)
→ Текст плана — описание намерения, не enforcement. Проверить: есть ли CF-правило REJECT которое БЛОКИРУЕТ нарушение. Если constraint живёт только в тексте — отметить как "text-only constraint" в секции архитектурных наблюдений. Усиленная версия: EP-level human gate (FLOW-27 EP-6) или явный CF REJECT с CRITICAL severity.

**Dual-session output принят как единый нарратив:**
> «FLOW-31 добавляет Design Intelligence Engine и формализует Reflexion Loop в prompt station»
→ Это два разных слоя. Design Intelligence — domain flow (что движок умеет делать). Reflexion Loop — meta-system upgrade документ (как система строится командой). Описывать раздельно. Meta-system docs (V31_ENHANCED_PLAN.md, V31_STATE.md) не входят в описание оркестрации — это методологические документы разработки, не engineered flow.

**Go-to-market research принята за архитектуру движка:**
> «FLOW-32 добавляет поддержку Salesforce AppExchange, HubSpot marketplace и monday.com»
→ Это go-to-market research (куда публиковать XIIGen как продукт), а не что движок умеет делать. FLOW-32 добавляет внутренний template marketplace (F1339–F1418). Внешние платформы = бизнес-стратегия из MARKETPLACE-*.md, отдельный вектор входных данных.

**Session-Isolation Renumbering Bug — принят за «нормальные» числа:**
> «Input файлы содержат F1075–F1142 — значит это правильный диапазон для этого FLOW»
→ Сначала проверить canonical STATE. Если canonical end = F1270, а input start = F1075 — это session-isolation offset +196. Input files разрабатывались в изолированной сессии без загрузки актуального STATE. Использовать input числа напрямую = backward compat collision. Нужен renumbering pass Phase 1 перед merge.

---

## ПАТТЕРНЫ MERGED ARCHITECTURE — 7 CANONICAL FILES

Все флоу XIIGen аккумулируются в 7 canonical reference файлов. Нарушение gapless numbering = BUILD FAILURE.

### 7 Mandatory Merged Files
| Файл | Содержимое |
|------|-----------|
| `SKILLS_FACTORY_RAG_MERGED` | Skill patterns SK-1..SK-N |
| `UNIFIED_SOURCE_INDEX_MERGED` | Source index: concept→factory mapping, event chains, MACHINE/FREEDOM |
| `V62_BFA_STRESS_TEST_MERGED` | BFA conflict rules CF-1..CF-N + stress tests ST-1..ST-N |
| `TASK_TYPES_CATALOG_MERGED` | Task type contracts T1..T-N (full engine contract format) |
| `ENGINE_ARCHITECTURE_MERGED` | Design decisions DD-1..DD-N, design records DR-1..DR-N |
| `SESSION_STATE_MERGE` | Global session state tracker across all flows |
| `MASTER_EXECUTION_PLAN_MERGED` | Execution plan, flow templates, phase structure |

**Gapless Numbering Invariant:** Каждый флоу начинается с `next_free` числа в каждой серии. Пропуск или повтор = backward compatibility collision.

**Session-Isolation Renumbering Bug (распространённый):**
> Input files содержат числа с меньшим диапазоном → были разработаны без актуального STATE.json.
→ Обнаруживается сравнением `canonical STATE.end` vs `input.start`. Нужен renumbering pass перед merge.

### Полный Формат Task Type Contract (обязателен)
```
TASK TYPE: T-N — Semantic Name
ARCHETYPE: type
ENTRY: trigger conditions
PURPOSE: что выполняет
DISTINCT FROM: отличия от похожих типов
FACTORY DEPENDENCIES: F-N → via CreateAsync()
FABRIC RESOLUTION: F-N → DATABASE FABRIC (provider)
AF CONFIGURATION: AF-1 генерирует, AF-6 review, AF-9 judge
BFA VALIDATION: CF-N checks
MACHINE/FREEDOM: что фиксировано, что конфигурируемо
IRON RULES: нарушение = BUILD FAILURE
QUALITY GATES: что проверяет AF-9
```
One-line stub вместо полного формата = немедленное отклонение плана.

---

## ПАТТЕРНЫ FREEDOM MACHINE & IMPLEMENTATION MODES

### Freedom Machine Philosophy
**The One Rule:** Если бизнес-пользователи могут захотеть изменить что-либо — оно ДОЛЖНО быть динамическим.

**5-Generation Platform Evolution:**
| Gen | Stack | "Машина" |
|-----|-------|---------|
| 1 | WordPress | HTML/template layer |
| 2 | .NET/Elasticsearch | Business logic layer |
| 3 | Microservices | Service topology layer |
| 4 | AI Agents | Generation patterns layer |
| 5 | Meta-fabric | Architecture decisions layer |

Каждое поколение превращает предыдущий динамический слой в статичную машину.

### Genesis Prompts = AF-1 INPUT (не инструкции для Claude Code)
**Системный gap (skills v7 и ранее):** Genesis prompts обрабатывались как инструкции для Claude Code.  
**Правильно:** Genesis prompts = AF-1 pipeline input. XIIGen генерирует код сам.

Признаки ошибки в плане:
- `SESSION files reference create_file` (вместо `afPipeline.run()`)
- Отсутствие `implementationMode: "af-pipeline"` в STATE.json
- Phases описывают Claude Code actions вместо INJECT → GENERATE → JUDGE → INTEGRATE → PROMOTE

### V0-MODE + V0-SCOPE Pre-Flight Checks (Skills v8)
Добавлены к flow-completeness-checker v1.5 (33 items):
- **V0-MODE**: Confirms `implementationMode` declared in STATE.json
- **V0-SCOPE**: Confirms scope discipline at routing level (не только content level)
- Q0 в planning-session-startup: "Who writes the code? AI pipeline or Claude Code directly?"

### FLOW-00.3: Cost Pipeline — Типичные Breaks
Три независимые места где cost tracking ломается:
1. `Dispatcher` вычисляет USD cost но **никогда не вызывает** `CostTracker.record()`
2. AF pipeline имеет **нулевые ссылки** на cost/tokens/budget
3. AF-11 feedback записывает quality scores но **не cost metrics** → `SpendGovernor` нефункционален

Паттерн: infrastructure flows для починки таких breaks (FLOW-00.1, FLOW-00.2, FLOW-00.3) вставляются в execution order до основных flows.

---

## ПАТТЕРНЫ SESSION PROTOCOL — START GATE & H0

### Обязательный SESSION-START GATE
Перед любым Round 1 output без исключений:
1. Физически прочитать (не назвать) все required skills — confirm каждый с SK# + одна фраза
2. Явно описать H0 conflicts в таблице (не молча резолвить)
3. Подтвердить scope target (разные флоу = разные task type sets)
4. Получить явное одобрение до перехода PLANNING → SIMULATION

### H0 Conflict Resolution Table (обязательна в session prompt)
| Конфликт | Default | Override | Authoritative Source |
|----------|---------|----------|---------------------|
| Stop cadence | Once per Planning | After EVERY round | User explicit instruction |
| Skill loading | Named = adequate | Physically read + confirmed | Session protocol |
| Phase structure | Single phase | PLANNING gate SIMULATION | User approval |

### Anti-Pattern List (обязательный раздел)
Session prompt должен содержать явный список запрещённых поведений:
- ❌ Начинать Round 1 до подтверждения всех skills
- ❌ Молча резолвить H0 конфликты
- ❌ Цеплять несколько rounds без ⛔ STOP
- ❌ Называть skill как "loaded" без физического чтения файла
- ❌ Валидировать FC gates inline вместо dedicated round
- ❌ Execution-focused naming (SESSION-A2-NODE) вместо teaching-focused (SESSION-TEACH-A0)

### Layer 1 vs Layer 2 Distinction
- **Layer 1**: Execution scripts (SESSION-A, SESSION-B files) — инструмент
- **Layer 2**: Teaching/planning content — что XIIGen должен знать как решать задачи — ЦЕЛЬ
- НИКОГДА не путать: Layer 2 = цель. Layer 1 = средство достижения Layer 2.

### FC Gate Validation Protocol
- FC-1..FC-31 plan review gates
- FC gates валидируются в **отдельном dedicated round** — НИКОГДА inline per file
- Причина: inline validation пропускает cross-file inconsistencies

---

## ПАТТЕРНЫ AMENDMENT PROTOCOL & IRON RULES SAFETY

### Принцип: Amendments, не Rewrites
**НИКОГДА** не переписывать утверждённые plan documents.  
**ВСЕГДА** добавлять numbered amendments к существующим документам.

Формат amendment:
```
### Amendment A-N: [Title]
**Status:** MUST-FIX | NICE-TO-HAVE
**Original text:** (цитата проблемного места)
**Corrected approach:** (исправление)
**Rationale:** (почему original подход неверен)
```

### ironRules Parallel-Field Safety Pattern
```typescript
// НЕПРАВИЛЬНО — ломает 140+ existing contracts:
ironRules: IronRuleSpec[]  // type change

// ПРАВИЛЬНО — backward compatible:
ironRules: string[]          // оригинальное поле НЕИЗМЕННО
ironRulesStructured?: IronRuleSpec[]  // новое поле параллельно
```
**Правило:** Тип `ironRules: string[]` НИКОГДА не меняется. Это breaking change.

### archetype Field Safety Pattern
```typescript
// НЕПРАВИЛЬНО — ломает enum usages:
archetype: string  // слишком широко

// НЕПРАВИЛЬНО — ломает open extension:
archetype: ContractArchetype  // слишком узко (pure enum)

// ПРАВИЛЬНО — union type с downgraded validation:
archetype: ContractArchetype | string  // union
// Validation: downgrade error → warning для неизвестных значений
```

### Pre-Allocation Session for Wave N+1
Перед началом Wave N+1 flows (группа flows одного wave):
1. Провести **Wave pre-allocation session** для всей группы (e.g., FLOW-03..FLOW-08)
2. Распределить non-overlapping Family/CF ranges заранее
3. Начинать реализацию только после согласования ranges

### LIFO Compensation Order
Паттерн FLOW-05: когда нужно откатить несколько операций с зависимостями:
- **WRONG**: strict temporal LIFO (последнее выполненное — первое откатываемое)
- **RIGHT**: dependency-driven order (откат в порядке dependency graph)
- Пример: `revokePoints → revokeBadges → resetStreak` (не обратный порядок выполнения)

---

## ПАТТЕРНЫ KNOWLEDGE GOVERNANCE & REVIEW GATES

### Knowledge Scope Default
- Default scope = **PRIVATE** (не MODULE, не GLOBAL)
- PRIVATE → MODULE: только если явно указано с rationale
- MODULE → GLOBAL: только copy-on-adopt с обоснованием
- **CF-POLICY-01**: отсутствие `knowledgeScope` → применять PRIVATE

### Portability Gap Layer (Classes A/B/C)
| Class | Тип зависимости |
|-------|----------------|
| A | RAG dependencies (поиск в ES, document retrieval) |
| B | Graph edge dependencies (decision-graph edges) |
| C | Prompt version dependencies (конкретная версия промпта) |

### Gate 0i "DIG IN DOCS FIRST"
Перед поднятием BLOCK:
1. Поднять потенциальный concern (internal)
2. **Search project knowledge** — docs, design files, session files
3. Найдено → withdraw concern, document resolution
4. Не найдено → escalate as BLOCK

**Запрещено**: поднимать BLOCK за отсутствие artif-файлов на Architect stage (artif-файлы ожидаются только на Planning/Implementation stage).

### Review Stage Hierarchy
| Stage | Что ожидается | Что НЕ ожидается |
|-------|--------------|-----------------|
| ARCHITECT | Flows, design artifacts | EngineContract, arbiterConfig |
| PLANNING | EngineContract, session files | Complete implementation |
| IMPLEMENTATION | Code, tests | Architectural decisions |

### 5-step i18n Translation Pipeline Pattern (FLOW-48)
```
1. tenant cache check → 2. policy gate → 3. marketplace probe →
4. delta translation (только delta между tenant и marketplace) →
5. full translation (если marketplace не имеет)
```
AI keys routing:
- Tenant-specific content → tenant AI credentials
- Marketplace-shared content → master tenant AI credentials

---

## ПАТТЕРНЫ ENGINE RUNTIME — КОМПИЛЯТОР И DATA CONTRACTS

### "XIIGen is a Compiler" Mental Model
> XIIGen — это компилятор. Принимает большую проблему, разбивает на стадии, каждая стадия производит output для следующей. Стадия с неправильной формой output — **молча corrupts всё downstream**.

**Правило перед изменением любого файла/модуля:**
1. Что эта стадия получает (input shape)?
2. Что она ДОЛЖНА произвести (output shape)?
3. Кто это потребляет?
4. Что потребителю нужно?
Если consumer requirement неизвестен — **halt и запросить** перед продолжением.

### Pipeline Data Contracts
| Stage | Receives | Must Produce | Critical Consumer Need |
|-------|----------|-------------|----------------------|
| Cycle 1 (Planner) | userIntent + constraints[] | Plan steps: abstract, domain-specific, one responsibility, no tech names | Шаги должны вынуждать 3 модели делать РАЗНЫЕ выборы — generic steps → convergence на Round 1 → useless DPO |
| Cycle 2 (Convergence) | One plan step + domain context | NODE: structure {I/O, events}, intent, step-specific constraints, quality target | Constraints STEP-SPECIFIC, не generic DNA rules — generic = немедленная convergence |
| Cycle 3 | NODE spec | LEAF or EXPAND (depth ≤ 3) | Deterministic routing |
| Cycle 4 | LEAF NODE | Implementation | Implementable spec |

### Live Engine Run — Типичные Первоначальные Баги
| Баг | Проявление | Корень |
|-----|-----------|-------|
| Mock Mode = 1.00 | Grade всегда 1.00, real providers не запускаются | `AI_PROVIDER` env var = MOCK MODE — нужны `AI_GEMINI_PROVIDER`, `AI_OPENAI_PROVIDER`, `AI_JUDGE_PROVIDER` |
| Cycle 1→2 Handoff | 10 plan steps произведены, 0 Cycle 2 rounds выполнено | Handoff controller не передаёт steps в queue / не инжектирован |
| CONTEXT_INSUFFICIENT loop | convergence.handler продолжает без достаточного контекста | Handler должен применять `CONTEXT_INSUFFICIENT` протокол к СЕБЕ перед генерацией |

### Flows-as-Nodes Unified Mental Model
```
Flow      = NODE (input shape → output shape → consumer)
Skill     = NODE (pattern → guidance → consumer skill)
Arbiter   = evaluator-NODE (grades other nodes)
Judge     = top-level orchestrator NODE
```
Не иерархия — composed node graph. Правильный mental model для engine design и session design.

---

## ПАТТЕРНЫ АРХИТЕКТУРНОГО РЕФРЕЙМИНГА — 5 GAPS

### From Code Generator to Universal Domain Engine
**Текущее:** XIIGen = NestJS code generator + disconnected learning infrastructure  
**Целевое:** Universal domain understanding engine

**The Missing Primitive:**
```typescript
NODE = {
  structure:   // I/O, events, dependencies
  intent:      // what this step accomplishes  
  constraints: // step-specific (NOT generic DNA rules)
  quality:     // what 0.95 looks like for THIS step
}
```
NODE создаётся через multi-model convergence ДО любой генерации кода.

### 5 Gaps (обнаружены через 16-step architectural journey)
| Gap | Current Behavior | Designed Behavior |
|-----|-----------------|------------------|
| G1 NODE | Не существует | Multi-model convergence перед кодом |
| G2 RAG | Используется как database | Semantic retrieval + graph navigation |
| G3 Arbiters | Работают uniformly | Contextual (scenario-specific) |
| G4 Planning DPO | Planning decisions не captured | Каждое planning decision = DPO triple |
| G5 Self-Context | System не может получить контекст сам | External context acquisition protocol |

### Cross-NODE Dependency Resolution
Downstream models могут запрашивать верифицированные факты из upstream NODEs.  
**Pattern:** NODE.A.quality.result → NODE.B.constraints.verified_fact

### Business Arbiter
Арбитр работающий across system boundaries: multiple repos + REST contracts + queue schemas.  
Обнаруживает cross-system logical flaws невидимые при single-repo analysis.

### Concurrent Agent Merge Problem
Два Claude Code агента на разных branches → T-number collision при merge.  
**Resolution:** Провести renumbering pass ПЕРЕД merge. Сначала загрузить canonical STATE, затем map conflicting ranges.

---

## ПАТТЕРНЫ GOVERNANCE — GR-001 & ARCHITECT RULES

### GR-001: Zero Tech Debt Golden Rule
> **"Zero tech debt = zero tech debt. Full stop. No pre-existing exceptions."**

Любое отклонение — создание долга. Нет исключений для "legacy", "pre-existing", "temporary".

### ARCHITECT Rules A-1..A-4
| Rule | Trigger | Required Behavior |
|------|---------|------------------|
| **A-1** | Session start | Никаких deliverables без явного одобрения плана |
| **A-2** | "Make a plan" | PLANNING standard: написать .md файл + present_files + stop |
| **A-3** | Negative feedback из любого источника | Нельзя игнорировать — включая source documents |
| **A-4** | "Produce md united plan" | Написать .md + present_files + stop (НЕ продолжать) |

**ARCHITECT Mode Drift (системный антипаттерн):**
> Сессия производит deliverables без одобрения плана, игнорирует negative feedback из source documents.
→ Поднимать ARCH-DRIFT если сессия производит код/файлы до явного "proceed" от архитектора.

### 4 Systemic Governance Gaps (G-AUTH-01..12, G-NDJSON-01..4, G-PROTO-01, G-ARCH-01)
1. **G-AUTH**: HTTP controllers без `@UseGuards`/`@Roles` (найдено в 12 skill files)
2. **G-NDJSON**: `fixtures/arbiters/{slug}-arbiters.bulk.ndjson` — нет type definitions ни в одном skill
3. **G-PROTO**: PORTABILITY-TEST-PROTOCOL-v2.0, AUTH-PLAN-v3.0, PFM-v2.9 не referenced в DoD gates
4. **G-ARCH**: Sessions producing deliverables без plan approval

### Issue Rejection Pattern
- External reviewer заявляет concern (e.g., "API keys plaintext")
- **НЕЛЬЗЯ** принимать без верификации: проверить source code напрямую
- Если код уже решает проблему (e.g., AES-256-GCM шифрование) — отклонить issue с evidence
- **Правило**: External reviewer claims ≠ ground truth. Always verify through source.

---

## ПАТТЕРНЫ AUDIT INTEGRITY & PROCESS VERIFICATION

### Fabricated Names Anti-Pattern
Распространённый баг при работе с collision analysis: task type names могут быть fabricated (не существуют в codebase).  
**Правило:** Всегда верифицировать имена через `grep` в source files перед документированием.  
**Пример:** `MLOptimizer`, `PatternParser` документированы как существующие; grep показал их нет; реальные имена: `PatternExtractorService`, `BenchmarkRunnerService`, `PatternIndexerService`.

### INFRA-AUDIT-1 Before Renumbering
Перед любым renumbering pass:
1. Запустить INFRA-AUDIT-1 scan: grep по всем source files для нахождения HIGHEST assigned numbers
2. Только после этого определять renumbering targets
3. Proposed targets T574-576 могут быть уже assigned — всегда проверять

### Process Self-Verification (Carries Same Rigor as Generated Code)
> Тот же rigour что применяется к generated code ДОЛЖЕН применяться к session management.

Признаки нарушения:
- Fixes marked COMPLETE в ISSUE INVENTORY без grep verification
- Один и тот же failure pattern в S-1, S-6, S-7 (recurring regression)
- Review-fix-re-review loop = quality mechanism (должен быть: process catches errors)

**Structural guard:** Post-fix verification step (Step 7 в FOUND-ISSUE PROTOCOL) обязателен:
1. Claimed fix applied
2. Run verification grep
3. Confirm pattern не появляется в output
4. ONLY THEN mark as COMPLETE

### Supersession Document Tracking
При наличии V2 и V3 документов одного типа:
- Явно отмечать supersession relationship
- Различать: что документ говорит vs что authoritative version говорит при конфликте
- "V3 recommended with two fixes" = precision required; "looks good" = unacceptable review

### Authoritative Event Names vs Wrong Names
При конфликте master plan ↔ preparation guide → **master plan всегда authoritative**.  
Пример (FLOW-02):
```
WRONG (guide):   ProfileEnriched, MatchConfirmed, OnboardingAnnounced
RIGHT (master):  ProfileEnrichmentCompleted, MatchCompleted, OnboardingCompleted
```
Перед production session file → верифицировать event names через master plan.

### Carry-Forward Registry Pattern
Tracking pattern для unresolved issues across sessions:
- C-{N}: Code-level issues
- S-{N}: Session management issues  
- K-{N}: Knowledge/documentation issues
**Решение накопления:** Shorter session cycles = fewer carry-forwards. Если registry > 10 items → stop, consolidate.

---

## НОВЫЕ ПАТТЕРНЫ ИЗ АНАЛИЗА КОРПУСА СЕССИЙ (батчи 01-07)

> Источник: SESSION-ANALYSIS.md Части 25-35. Добавлено 2026-05-28. 63 новых паттерна к существующим 56.

### Архитектурные инварианты движка

- **Fabric-First Five-Layer Hierarchy** — обязательный якорь промпта: L0=Fabrics, L1=Factory, L2=Engine Contracts, L3=AF Stations, L4=Guardrails, L5=Flow as Output. Проект — это движок на L0-L4; флоу — его выход, не архитектура.
- **Version Drift Detection** — метрика engine-reference-count: V71=0 engine refs = полная потеря концепции движка. Recovery: corrected prompt с явной иерархией fabric-first перед любым design-work.
- **Fabric-First UI Mandate** — UI-подход должен быть fabric-first независимо от платформы (React Native, web). Архитектурный инвариант, не технологический выбор.
- **Native Provider SDK Rule** — запрещено использовать SDK одного провайдера для вызова другого. DD-358: xai_sdk для Grok (не OpenAI base_url wrapper); DD-359: google-genai (не google-generativeai). Нарушение = provider-leaking против fabric-first.

### Инфраструктурные паттерны

- **DNA-7: W3C Trace Context** — 7-й DNA-паттерн. Trace ID должен propagate через ВСЕ inter-service calls включая queue events (не только HTTP). Инициализируется на входе флоу; обязателен в каждом downstream событии, логе, AI-вызове. Расширение Queue Fabric.
- **Secrets Fabric как 7-й Fabric Interface** — ISecretsService следует тому же registry/resolver/factory паттерну что и 6 других fabrics. ConfigBuilder резолвит `$secret:path` и `$env:VAR` в nested config dicts. Iron rule: никогда не логировать secret values; list_secrets никогда не возвращает значения.
- **Main→Consumed→Archive→DLQ** — каждый inter-service вызов идёт через queue (никогда прямой HTTP). Стадии: Main → Consumed → Archive → DLQ. Redis Streams с consumer groups.
- **Transactional Outbox для Event Reliability** — события пишутся в outbox atomically с бизнес-транзакцией, relay публикует в queue. Предотвращает event loss при сбое между DB write и queue publish. Отличается от стандартного Redis Streams паттерна.

### Flow Development паттерны

- **Two-Document Deliverable per Flow** — каждое изучение флоу производит 2 документа: (1) SESSION-FLOW-XX-MASTER.md (только curl/jq/node -e, никогда pseudo-code; $T_START/$T_END из STATE.json); (2) FLOW-XX-MISSING-INFRASTRUCTURE.md (gap analysis с Tier A + Tier B). Gate condition: псевдокод в gate scripts = НЕИСПОЛНИМО.
- **Gate Script Executability Rule** — gate scripts должны использовать ТОЛЬКО curl/jq/node -e команды. Блоки вида `/* ES client */`, top-level await в node -e = Claude Code не может выполнить. SK-419 требует 8 обязательных полей на event declaration (не 3).
- **Infrastructure Investment Convergence** — каждый последующий флоу требует меньше новой инфраструктуры. Измеренная траектория: FLOW-15 ~5 дней → FLOW-18 ~15 дней → FLOW-21 ~2 дня → FLOW-28 ~1 день (zero code changes). Это ожидаемый результат кумулятивной инвестиционной стратегии.
- **Completed-But-Unmerged Extension Gap** — расширение флоу может быть полностью завершено в отдельном документе, но никогда не слито в 7 canonical files. Правильная последовательность: сначала слить все completed-but-unmerged, потом выполнять новые extension phases. Нарушение ломает gapless numbering invariant.
- **Engine Counter Advancement Protocol** — каждый флоу продвигает глобальные счётчики F/T/CF/ST/SK/DD/DR. Next-flow start points обязательны в SESSION_STATE. Нарушение последовательности = corruption backward compatibility.

### Self-Building Engine паттерны

- **T58 Self-Extension Contract** — task type T58 генерирует новые task types (META archetype). Принимает capability description; производит factory interfaces + AF configurations + BFA rules + iron rules + quality gates. Замыкает петлю self-building engine.
- **AWAITING_DECISION + 5 Mandatory Design Questions** — новое lifecycle state блокирует code generation. Каждый task type должен ответить на 5 вопросов перед design review: (1) external dependencies, (2) required user decisions, (3) output validation, (4) gate chain enforcement, (5) provider fallback behavior. Отсутствие ответа = design review failure.

### Governance & Testing

- **FC Failure Class Taxonomy FC-1..FC-12** — формальная таксономия: FC-1 count drift, FC-5 registry completeness, FC-6 external dependencies, FC-8 format, FC-10 incomplete propagation (критичный: two-field fixes должны propagate ко ВСЕМ местам одновременно), FC-12 gate questions. H0 может waive FC-8 citing direct authority. Batch resolution: все issues за один pass.
- **Negative Bypass Tests** — каждое MACHINE rule должно иметь negative bypass test подтверждающий что rule нельзя обойти. Без bypass tests MACHINE rules существуют только в документации.
- **Moderation GATE Three-Outcome** — approved / rejected / human review. Обязателен перед любой downstream distribution контента. Три outcome-паттерн = переиспользуемый для любого validation gate.

---

## ПРИМЕНИМОСТЬ

**Применяется для:**
- ZIP-пакетов планов (REFERENCE-PLAN + SESSION-*.md)
- Сессионных файлов с историей работы
- Наборов архитектурных документов без единого главного файла
- Любого задания «объясни что здесь происходит» по документам

**Не применяется для:**
- Генерации кода (это planning-skill + genesis AF-1)
- Написания новых планов с нуля
- Ответов на конкретные технические вопросы без документов
