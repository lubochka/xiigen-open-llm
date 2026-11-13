# RP04 — проверка findings и применённые исправления EX01–EX04

**16 октября 2026 · SOURCE_REVIEW / PLANNING_DRAFT.** Это продолжение F17–F30 и BD01–BD04, не новый цикл R01–R25. В этой ревизии непосредственно заменены **пять действующих task prompts: 01B, 01C, 02B, 02C, 03B**, а также task-local AGENTS и startup. Всего в пакете остаётся 12 заданий. Код продукта, DPO, веса и обучение не изменены; отдельная CODEX Astra-экспертиза не выполнялась. [Действующий старт](../Kiro-Xiigen-RP04-Async-Orchestrator-HigherOrder-Graph-Pack/INSTALL-AND-START.md).

Первичная публикация наблюдений: [02a42e6](https://github.com/lubochka/xiigen-planning/commit/02a42e685a3082ee2272bfd808a8121ddadb2d22). Её прежнее «карточки ещё не исправлены» было промежуточным состоянием; таблица применённых изменений ниже фиксирует последующие реальные записи. Ни один DRAFT commit не является independent ACCEPT.

## 1. Что подтверждено, что уточнено, чего не проверяли

Прочитаны atlas F17–F21, F22–F27, F28–F30 и BD01–BD04; актуальные 01A/01B/01C/02B/02C/03B, SOURCE-AND-ARCHITECTURE, startup и tasks/AGENTS. Сохранены различия: scaffold не обучение; config не structural route; clean batch не inner law admission; unobserved не rejected; fixed-pool ranking не fresh-domain reach; result ranking не execution savings. **Это не новые находки и не заявленная повторная runtime-аттестация всех прежних утверждений.**

Новые результаты ревизии классифицированы точно:

| ID | Класс результата | Что меняется в работе |
|---|---|---|
| EX01 | Прямо проверенное следствие известного one-shot контракта; не новый code defect | Fresh invocation per observation, сохраняющий нужный learned state и изоляцию зависимостей |
| EX02 | Прямая source-локализация прежних F17/F21, не новый класс дефекта | Actual target-reader и bypass/legacy/fallback отдельно от общего успеха |
| EX03 | Формально обоснованный риск недоопределённого RP04 scorer; не доказанный runtime bug | Joint context×plan/motif representation и независимый context-reversal discriminator |
| EX04 | Подтверждённое дублирование/противоречие в прежних физических prompts | Один domain first adoption; broad exams и ablations только у 03B, не до каждого handoff |

**Scope:** bounded RP04 review с исходными/межслайсовыми источниками, не полный semantic audit всех slices, RAG и WorkLeft. Причины зависания Windows и предполагаемая подгонка тестов здесь не исследовались по workflow/test diffs и не объявляются доказанными.

## 2. Прямые источники и границы чтения

K snapshot: `lubochka/llm_mvp_core@58d12ecfe7af05e3221f984bf6c4c67d82fe3b96`; P HEAD при первой публикации: `87b1bd32767eceaf2934bdae3d0d3605a0416655`.

- [DynamicGraphCandidateInvocationOrchestrator.cs](https://github.com/lubochka/llm_mvp_core/blob/58d12ecfe7af05e3221f984bf6c4c67d82fe3b96/15.OrchestratorModel/Logical/DynamicGraphCandidateInvocationOrchestrator.cs), blob `b66c9b50df9be24b2619995e8101fcb41283d00d`, **строки 1–280**: one-shot `_started` перед cancelled check; legacy pre-route до candidate execution. Остальная часть файла и все его callers не объявляются прочитанными полностью этой ревизией.
- [DynamicGraphMachine.cs](https://github.com/lubochka/llm_mvp_core/blob/58d12ecfe7af05e3221f984bf6c4c67d82fe3b96/15.OrchestratorModel/Logical/DynamicGraphMachine.cs), blob `8149717b04d442e6e89bb61ab34fb608201ef369`, **полностью**: Train/Evaluate/Export/Import по-прежнему NOT_ACTIVATED. Это подтверждение существующего F17, не новая находка.
- До ремонта RP04.01B blob `eda35e27bd84080ef6fdb8a8cb61973b8744cc58`, 02B `10bbfa1300216edf5f7aad50cab4bc6f97ad7ffb`, 01C `0d79c4d812ea74019756c5f8d87b10553ca1ae98`, 02C `7a8e226731a596056ed5b5ad9477d8b55133cc55`, 03B `4472a2effc6784f0b3a6a4d37ce9cfdc19eb8068`: прочитаны тексты соответствующих карточек. Указания на новое состояние не ретроактивно меняют эти old blobs.
- [SOURCE-AND-ARCHITECTURE](../Kiro-Xiigen-RP04-Async-Orchestrator-HigherOrder-Graph-Pack/SOURCE-AND-ARCHITECTURE.md), blob `965627bb2df6fc26357de0e02d41e8b00617d7f0`: уже называл invocation bridge one-shot. Поэтому прежнюю промежуточную формулировку EX01 «новая граница» уточняем: новое — явная executable exam recipe и recovery, не открытие ранее неизвестного свойства.

**Реально посещённые RAG/primary:** D/WorkLeft-RAG-MASTER-INDEX начальные sections (запрошены 1–160), WorkLeft-RESEARCH-03 current R16 conformance/one-use/roles (1–145); D/original task.md (1–130: четыре роли и combinations); G/active-codex-execution-plan-06-async-combinations.md (1–140: ownership и отдельные caps); R/previous rounds thinking results.md (1–120, R07 repair/current scope); R/PARALLEL-R01-R03-JOIN.md (1–105, current concurrency override/owner receipts). У некоторых больших ответов tool preview был усечён; claim относится только к реально видимым начальным sections. Полные огромные архивы и не посещённые G/F/V документы не объявляются прочитанными. Source map направляет следующие scoped implementation tasks к их exact primary, а не создаёт общий барьер перечитывания.

**Skills применены по видимым sections:** trainable-architecture-decision 1–150; phase-planning 1–105; plan-review 1–90; code-examination 1–100; prompt-patch-authoring 1–115; human-readable-architecture-audit 1–125, все из K/docs/skills. Применение: bound claim к source, не смешивать future plan и implementation, один конкретный bottleneck, complete prompt/GOOD-BAD-EDGE, source before code, читаемое русское объяснение. Это не утверждение FULL_READ всего skill-stack, не 30/50 review passes и не независимый agent verdict.

## 3. EX01 — fresh invocation без сброса обученной модели

`RunAsync` делает `Interlocked.CompareExchange(ref _started,1,0)` до проверки cancellation. Повторный вызов того же объекта возвращает `DYNAMIC_GRAPH_CANDIDATE_INVOCATION_ALREADY_STARTED`; даже первая pre-cancelled попытка расходует wrapper. Это source fact, не наблюдённый сбой проведённого экзамена.

**Ремонт 01C/02C/03B:** каждый `(case_id,condition_id,repeat_id,attempt_id)` получает fresh invocation через штатную factory. Одинаковые ordinary entry/config/law/representation и соответствующие immutable C0/U states; C0 не fresh zero-weight model. Проверять mutable serving dependencies: fresh wrapper сам по себе не доказывает runtime isolation. Actual cold-load claim требует cold process либо действительно изолированного loaded snapshot.

**GOOD:** разные invocation IDs, одинаковая конфигурация, actual reader читает соответствующие state hashes. **BAD:** снять `_started` для зелёного bulk либо пересоздавать модель с нулевыми весами; в первом случае нарушен контракт, во втором U больше не U. **EDGE:** pre-cancelled attempt → fresh wrapper для допустимого нового observation, но effect/optimizer identity прежнего unresolved attempt разрешается отдельно. Новый invocation не право replay.

**Проверка/опровержение:** reuse существующий correct factory caller и C0/U ordinary trace, если уже соответствуют; иначе exact instance-scope seam. ALREADY_STARTED виден в общем availability denominator, но не semantic rejected pair/learner regression. Ноль новых tests за документацию; только relevant changed-risk evidence при реальном code edit.

## 4. EX02 — полезный legacy ответ не равен E3/E4 adoption

В прочитанном source `RunAsync` неадаптивные prepared scenarios возвращаются через `Assistant.RunPreparedAsync` до candidate execution. Это прямой пример старого риска «ordinary ответ не использует target», не новый security defect.

**Ремонт:** record prepared_scenario, invocation/runtime scope, actual_reader_method/hash, candidate launch count, final typed output и route_class: TARGET_USED, LEGACY_PRE_ROUTE, FALLBACK, TARGET_NOT_USED, UNKNOWN. Ни checkpoint file, ни ScenarioId сами по себе не actual reader proof.

**GOOD:** все назначенные applicable cases входят в общий user score; target-use rate и target-used conditional quality имеют отдельные N. **BAD:** убрать bypass/failures из общего знаменателя или credit ProjectWiki answer к theta. **EDGE:** C0/U реально выбирают разные paths → сохранить traces, назвать E2E intervention либо восстановить matched isolation, не принудительно подключать только U. Не ломать полезный pre-routing ради метрики.

**Опровержение:** существующий actual trace уже показывает target state read и typed final consumer — reuse. Владелец 01C/02C и 03B; 00A только при missing caller. Локальная source-структура не доказывает, что все hosts bypass learner.

## 5. EX03 — доменный контекст должен влиять на сравнение вариантов

Прежнее `phi_domain(x,plan)` допускает опасно слабую реализацию:

```text
phi(x,p) = [c(x), a(p)]
score(x,p) = wc·c(x) + wa·a(p)
score(x,p+) - score(x,p-) = wa·(a(p+) - a(p-))
```

Контекст сокращается в same-x linear preference. Такая модель не может разворачивать предпочтение одинаковых A/B от x, если обе альтернативы остаются допустимыми. Это математическое следствие выбранной формы, **не утверждение о найденном реально работающем таком learner**. Принцип context×action уже применим в RP02; здесь уточнён его обязательный смысл для RP04.

**Ремонт 01B/02B:** source-confirmed sparse joint features `psi(x,plan/motif)`, compact bilinear `c(x)^T W a(p)` либо nonlinear joint scorer; exact learned owner/gradient/coordinate schema. Это возможные минимальные design варианты, не обещание существующей implementation. Reuse RP02 role/coreference/AST/kNN evidence; не требовать полный tensor product, новый tokenizer или giant model. Hard-law mask отдельно и не обходим score.

**Экзамен D в 03B:** одинаковые stable A/B lawful в x1/x2, независимая rubric предпочитает A для x1 и B для x2; проверить actual ranking и перенос на новые source families. **BAD:** смена только из-за law-mask запрета названа contextual learner. **GOOD examples:** две lawful C# pure processing стратегии при разных input sizes и наблюдаемой quality/cost цели; SQL equivalent read-only strategies на разных distributions; EN→RU обе грамматически допустимые стратегии при явно разном requested register. Cost preference не выдумывать. Grammatical agreement/coreference остаются обязательными законами, но один veto неверного рода не доказывает contextual ranking.

**EDGE:** один lawful plan, tie, unclear register, unknown cost comparator → NO_VALID_CONTEXT_REVERSAL/TIE. Future answer/judge label не decision-time feature. Для первого честного global CONFIG_PREF update эта панель не prerequisite; ограничить claim, не задерживать полезный lesson. **Опровержение риска:** existing joint scorer + independent reversal evidence. Формула показывает возможность, а не gain; один виденный reversal не generalization.

## 6. EX04 — лишние повторные экзамены в adoption-карточках

**Подтверждённый prompt-level разрыв.** Старый 01C одновременно требовал P/C0/U cold starts, CODE+ID observations и cancel/retry probe; старый 02C — CODE/ID и static-order-1/fixed-order-3/learned comparison. Между тем 03B уже владеет независимым domain/causal exam, а startup разрешает первое learning/adoption до broad domain proof. Для простого consumer handoff это дублировало оценку и могло отодвигать урок.

**Применённый ремонт:** 01C/02C могут завершить ADOPTION_OBSERVED_SINGLE_DOMAIN по одному runnable diagnostic/DEV C0/U ordinary-use trace, reuse достаточного 01B/02B receipt. Historical P-run, второй domain и structural ablations принадлежат конкретному 03B вопросу; parent для rollback сохраняется всегда. Cancel/identity/rollback check выполняется лишь при соответствующем changed deterministic risk, не новая suite на каждый handoff. Cold-loader не отлаживать на sealed T.

**GOOD:** U уже записан → проверить его actual reader и typed answer → 03B; не ещё один update. **BAD:** ждать второй lane, семь доменов, все baseline runs или Windows green до first useful signal. **EDGE:** one-domain success недостаточен для broader claim; статус ограничен, 03B остаётся явным владельцем дальнейшего экзамена. Если typed output/rights/state unsafe — precise seam repair, не обход. **Опровержение:** accepted earlier sufficient receipt уже заменяет старые steps на реальном dispatcher — REUSE, однако физический prompt всё равно должен не требовать лишнего повторения.

## 7. Применённые физические изменения

| Файл | Что изменено | Commit / resulting blob |
|---|---|---|
| [01B](../Kiro-Xiigen-RP04-Async-Orchestrator-HigherOrder-Graph-Pack/tasks/RP04.01B.md) | State-first E3, joint context features, one signal/numeric update, identity, ранний reader | `ca9cf3056611bb387b5c44b5b722999e598f6ffd` / `cc2c30267d7ba55165c12a5db241e9fb6adb6c54` |
| [01C](../Kiro-Xiigen-RP04-Async-Orchestrator-HigherOrder-Graph-Pack/tasks/RP04.01C.md) | Один domain adoption; fresh invocation; actual reader/bypass; снято дублирование exam | `8690e94e2e211a57a42e13ec1583f299144da44a` / `afbabf93a60676ecdca7c37a3717f1d509f9393e` |
| [02B](../Kiro-Xiigen-RP04-Async-Orchestrator-HigherOrder-Graph-Pack/tasks/RP04.02B.md) | Directed contextual motif update, one owner/journal, no repeated lesson | `08c4342807b572de29d781dbf47386c69a2af257` / `859daff5a70c8ec07251198bf76167c3db4fc49a` |
| [02C](../Kiro-Xiigen-RP04-Async-Orchestrator-HigherOrder-Graph-Pack/tasks/RP04.02C.md) | Actual ordered execution→typed output; one-domain handoff, distinct invocation scopes | `c0f788ec5f3818b817bcd7775039509721427f51` / `36800e8c0ff1ddad7332f89afbfc51dc4fe2ae6f` |
| [03B](../Kiro-Xiigen-RP04-Async-Orchestrator-HigherOrder-Graph-Pack/tasks/RP04.03B.md) | Progressive multidomain panel; A/B/C/D; target-use denominators; bulk/retention/full PC | `ffd667c922e4b19d172a35b2f42d9a169d5452e1` / `80932697a55694ec3bdd0af5b7605218d6d72071` |
| [tasks/AGENTS](../Kiro-Xiigen-RP04-Async-Orchestrator-HigherOrder-Graph-Pack/tasks/AGENTS.md) | Устранены возврат старых gates и двусмысленность текущих EX sections | `2369d4d41dafdfbcc884f421624bc2da41f9e4ea` / `e1d71500dbb076464fc383ea45a2dcdbcd39cee7` |
| [INSTALL-AND-START](../Kiro-Xiigen-RP04-Async-Orchestrator-HigherOrder-Graph-Pack/INSTALL-AND-START.md) | State-based dispatcher и полный parent-to-worker старт с новыми rules | `fb381d4c0d59a7475efb05a19c7876bc21aa19a3` / `27e80595834d0e632b1be0ede9683d3d8c742b26` |

Каждая из пяти карточек содержит CURRENT/NOT, входы/grants, source+skills, точные шаги, GOOD/BAD/EDGE/почему/recovery, minimum checks, scientific scope, optional bounded child и return schema. Не заявляем переписывание всех 12 карточек. Write results дали приведённые commit/blob identities; итоговая повторная remote verification записывается отдельным receipt, не подменяется этим утверждением.

## 8. Научная и функциональная сохранность

Кратчайший путь: существующие source/owner receipts → готовый E3 ИЛИ E4 → два lawful observed outcomes → один новый informative signal → один numerical update → coherent checkpoint → fresh invocation с actual reader → typed ordinary output. Если update уже сделан — продолжать с него. Далее независимый двухдоменный pilot, C#/EN→RU/SQL и применимые остальные domains, реальные unique B0→B1→B2, retention/interference/time/resources. Нет mandatory Windows/coverage gate.

RP02 сохраняет laws/role-token/vector/dynamic-kNN; RP03 many→many/Flow; RP04 E3/E4; RP01 response DPO; LoRA/projection actual transform/consumer отдельно. На isolated C0/U freeze non-targets; hybrid D/L/H и interaction experiments после first lesson, с одинаковыми правами/безопасностью и matched PC budget. Затраты всех losers/cancel/retries, proposal/drain, cold-start/index/checkpoint/teacher/judge видимы; NA не zero. Raw law-correct typed outputs и независимые source families, а не test percentage, решают gain/regress/tie/inconclusive.

**Роли:** CODEX с **astra 6 ultracode** делает делегированный HUMAN выбор случая/предпочтения; разрешённая внешняя открытая китайская модель автор ОБЕИХ final chosen/rejected; ДРУГОЙ CODEX с **astra 6 ultracode** оценивает exact unchanged pair. Post-update examiner и final exact-source reviewer — отдельные реальные роли с receipts. RP01 synthetic-dev exception не распространяется на RP04 автоматически. Source-observed route/motif preference не равно standard response DPO и не требует бессмысленного ожидания response author.

**Предел выполненного:** planning/source author review + Git publication. `PRODUCT_CODE_UNCHANGED; NEW_PRODUCT_TESTS=0; TRAIN_NOT_RUN; DPO_NOT_GENERATED; DOMAIN_EXAMS_NOT_RUN; GAIN_NOT_MEASURED; INDEPENDENT_ASTRA_NOT_PERFORMED`. Произвольное удаление полезных deterministic guards/tests не предлагается. Реальная release/native verification остаётся separate delivery obligation. Чужие slices/results/weights/statuses не изменены.
