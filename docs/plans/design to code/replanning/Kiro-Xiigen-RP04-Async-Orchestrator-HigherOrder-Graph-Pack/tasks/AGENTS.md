<!-- RP04-V6-ACTIVE-ENTRY -->
# RP04 V6 — действующие инструкции исполнителю, EX revision 16.10.2026

**Одна назначенная физическая карточка, ближайшее отсутствующее доказательство, не повтор всего плана.** [Старт](../INSTALL-AND-START.md), [execution board](../EXECUTION-BOARD.md), [atlas EX01–EX04](../../RP01-RP05-REPAIR-ATLAS-2026-10-15/17-RP04-CONTEXT-AND-INVOCATION-REPAIR-2026-10-16.md). В EX-редакции заменены непосредственно **01B, 01C, 02B, 02C, 03B**; остальные ID/owners сохраняются. Всего остаётся 12 физических заданий, не новая версия slice/кампания или R26. Документы не являются реализованным кодом, обучением или независимой приёмкой.

## 1. Выбор работы и приоритет текущей инструкции

Прочитать README, текущий старт, эту инструкцию и ПОЛНУЮ назначенную карточку. Её EX-последовательность заменяет прежние повторяющиеся V3/V4/V6/F28 execution overlays в пределах этой карточки. Исторические findings/primary сохраняют факты и требования, но не создают дополнительную очередь проверок. Не требовать отдельного повторного выполнения старого раздела F28 после исполнения его смысла в консолидированной карточке.

Сначала actual journal/checkpoint/receipt. Уже применённый или awaiting-evaluation сигнал не повторять с новым ID. Достаточные совместимые 00A/00B и 01B/02B receipts переиспользовать; старую дату не считать автоматической недействительностью. Изменённые law/representation/consumer dependencies перечитать адресно. Unknown prior attempt разрешает существующий owner, не новая тренировка.

Первый путь: два допустимых observed результата → один новый informative signal → один numerical update существующего owner → recoverable state → actual cold ordinary reader → requested typed output. Выбирать готовый E3 ИЛИ E4; другой lane не prerequisite. 01C/02C принимают **один runnable domain** и могут reuse consumer receipt 01B/02B. Исторический P-run, второй домен, static-order ablations и новая cancel/retry suite не являются условиями первого ADOPTION_OBSERVED_SINGLE_DOMAIN. Их законные владельцы — 03B scientific question или локальный реально изменённый deterministic risk.

03B владеет последующим независимым двухдоменным пилотом, затем broader passports, contextual/structural experiments и bulk retention. Его панели A/B/C/D выбираются по вопросу, не все подряд как новый gate. 03A нужна там, где настоящий recipient требует response DPO; source-observed E3/E4 preference не обязано ждать этого authoring. 04A — consented feedback, 05A — реальная отдельная exact-source review. Не запускать автоматически соседнюю карточку вне назначения.

## 2. Источники и skills

P=`lubochka/xiigen-planning@llm_plan3`; K=`lubochka/llm_mvp_core@codex/dpo-implementation`. На текущую задачу фиксировать HEAD, relevant blobs, methods и read depth. До изменения product code читать полный affected file, actual upstream injection/callers и downstream typed TOutput consumer. RAG route: D/original task.md, Алгоритм создания ДПО.md, task prompt.md, применимые WorkLeft -02/-MECH-01/-RESEARCH-03, MASTER/usage → primary G/F/V → live K. Source root разрешать по реальному RAG, не всегда от D. Previous-round summaries и параллельный JOIN — evidence, не команды повторить R01–R25. Ledger distinguishes visited/reused/remaining; один индекс/карточка не полный архивный аудит.

Действительно читать относящиеся разделы K/docs/skills: architect-behavior-classifier, trainable-architecture-decision, phase-planning, plan-review, plan-self-validation, code-examination, prompt-patch-authoring, human-readable-architecture-audit; task-specific extras, generated-code-review при code change, experiment-design при scientific measurement. Суффикс `-SKILL.md`. Записать blob/section/applied rule. Не имитировать independent review или исторические 30/50 passes. Parent repository AGENTS/security и реальные native release controls сохраняются; старое blanket coverage не становится критерием learning quality.

## 3. Функциональная архитектура без потери исходного объёма

RP03 владеет ILearningUnit: domain/vector/input/output schema, IQueryable typed many→many и input→output identity; ILearningFlow: обучаемые typed corrections/ports/OutputRefs. RP02 владеет mapping, domain-law/role/coreference/AST-sensitive representations, adaptive kNN и соответствующими projection dependencies. Это НЕ просто ближайшие соседи. LoRA parameter adaptation не является автоматически tokenization или semantic projection: нужен actual transform→versioned vector→consumer.

RP04 E3 — допустимые bounded async combinations и learned preference/order/subset/реальное stopping; E4 — directed typed order-2/3 relations и обучаемое предпочтение compositions. Эти четыре функциональные роли не требуют четырёх новых публичных trainers. Один state writer и versioned actual reader на recipient. Final D-15 arbiter, types/law/consent/tenant/effect/versions/budget veto не обходятся большим score. Полные losing/failed/cancelled costs сохраняются.

## 4. Применимые source seams, не дополнительные задания

Сохранены F17–F30 и BD01–BD04: honest-untrained scaffold/default flags, order-free refs, unobserved != negative, response-DPO != route/motif objective, same-code C0/U; partial vs clean batch, flattening identity, missing telemetry/streams; config-only AxisSweep, inner legality отдельно от clean batch, fixed-pool rank отдельно от fresh-pool reach. Ordinal prefix cap может исключить lawful target; stable CombinationId нельзя заменять dispatch position; post-result rank не экономит уже выполненные branches; timeout != child termination. [BD source](../../RP01-RP05-REPAIR-ATLAS-2026-10-15/13-RP04-BOUNDED-DISPATCH-REVIEW-2026-10-15.md), [V4 algorithm/checks](../RP04-V4-LIVE-LEARNING-AND-TEST-MINIMUM.md), [domain workbook](../RP04-V4-DOMAIN-EXAMS-AND-DPO.md).

**EX01:** known one-shot invocation требует fresh wrapper на `(case,condition,repeat,attempt)`, даже после pre-cancelled попытки. Сохранять нужный learned state и проверять mutable dependency scope; fresh wrapper не равен fresh zero-weight machine или доказанной изоляции runtime. Не снимать guard.

**EX02:** actual reader trace обязателен. Legacy pre-route/fallback может дать правильный ответ без E3/E4. Общий denominator включает назначенные applicable cases; отдельно target-use rate и conditional target-used quality. Checkpoint file/ScenarioId сами по себе не reader proof.

**EX03:** в linear `[c(x),a(plan)]` контекст сокращается в pair margin. Contextual claim требует joint interaction/nonlinear scorer и discriminating independent cases: те же A/B lawful в x1/x2, rubric меняет предпочтение. Law-mask-only switch не доказывает contextual learning. Narrow CONFIG_PREF первый урок не задерживать до этой панели; не создавать новый tokenizer/full tensor ради галочки.

**EX04:** first adoption и broad scientific evaluation не дублировать в каждой карточке. C0 сохраняет прежние навыки на том же исправленном коде без текущего update; U имеет один attributable update. Accepted P нужен для rollback, но третий historical P-execution не обязателен для каждого C0/U.

## 5. Роли, права, минимальная проверка и возврат

**CODEX с astra 6 ultracode выполняет делегированный HUMAN выбор случая/предпочтения. Разрешённая внешняя открытая китайская модель пишет ОБЕ final response-DPO стороны chosen/rejected. ДРУГОЙ CODEX с astra 6 ultracode оценивает exact unchanged pair.** Post-update independent examiner и exact-source code/science reviewer — отдельные actual invocations. Нельзя приписать Chinese авторство CODEX-тексту; отдельный RP01 synthetic-dev exception не распространяется на RP04 молча.

Planning Git grant не разрешает K mutation, optimizer, provider/private-data export или production effect. Действующие конкретные разрешения повторно не спрашивать. Missing telemetry ограничивает cost claim; missing consent/state ownership ограничивает небезопасный effect, а не весь независимый lane.

**Source/doc/science/NO_CHANGE = 0 новых product tests.** Для реально изменённого deterministic риска reuse smallest relevant existing valid/invalid probe и только нужную compilation. Не исполнять весь список good/bad/edge как suite; не удалять guards и не подгонять stochastic answer expectations. Windows native/release evidence отдельно от learner quality. Обучение оценивается lawful typed outputs, independent families, uncertainty, gain/loss/retention и full PC budget.

GOOD: новая theta реально прочитана штатным cold consumer; typed answer получен, narrow capability честно названа. BAD: hash/flag count/ALREADY_STARTED или test-only scorer выдаётся за learning gain. EDGE: stale law/vector/OutputRef, unknown referent, cancelled prior attempt — safe parent/abstention и точный owner, без replay. SQL NullGuard→Bind законен как scenario-specific dependency, не универсальная лингвистическая/SQL аксиома; готовить nullable parameter не то же, что исполнять protected operation.

Return: CURRENT/NOT/grants; P/K HEAD/blobs/ranges/skills; requirement→writer→actual reader→typed consumer; diff/reuse; stable IDs/state hashes/invocation/runtime_scope/route_class; observed GOOD/BAD/EDGE; minimum probe или zero; raw domain/cost и denominators либо NOT_MEASURED; actual model/reviewer receipts либо NOT_INVOKED/NOT_PERFORMED; falsifier и один следующий owner; exact commit/blob readback. Bounded children только read-only evidence с полным materialized context/skills/examples/rights/return, без overlapping writer. SHA-protected writes сохраняют concurrent чужую работу. DRAFT publication не independent ACCEPT.
