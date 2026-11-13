# RP04 V6 — ближайшее настоящее обучение, не повтор программы проверок

**16 октября 2026 · V6 / EX revision.** Это актуальный старт существующего пакета из **12 физических заданий**. Не новый slice, не V7-кампания и не дополнительные Rxx rounds. Статус публикации — PLANNING_DRAFT / AUTHOR_REVIEW, не выполненные code/training/science и не независимый CODEX ACCEPT.

[README](README.md) → этот старт → [task-local AGENTS](tasks/AGENTS.md) → **одна полная текущая карточка**. [Execution board](EXECUTION-BOARD.md) сохраняет IDs/owners/dependencies; его историческая очередь не требует повторять достаточные текущие receipts. PACKAGE-MANIFEST task IDs/count неизменны.

**Непосредственно исправлены 16 октября:** [01B — E3 update](tasks/RP04.01B.md), [01C — E3 ordinary use](tasks/RP04.01C.md), [02B — E4 update](tasks/RP04.02B.md), [02C — E4 ordinary use](tasks/RP04.02C.md), [03B — независимый экзамен](tasks/RP04.03B.md). [01A](tasks/RP04.01A.md) сохраняет bounded-dispatch prompt. Остальные карточки не объявляются заново полностью проверенными этой редакцией.

[Атлас EX01–EX04: причины, источники, примеры и фальсификаторы](../RP01-RP05-REPAIR-ATLAS-2026-10-15/17-RP04-CONTEXT-AND-INVOCATION-REPAIR-2026-10-16.md) дополняет существующие F17–F30 и [BD01–BD04](../RP01-RP05-REPAIR-ATLAS-2026-10-15/13-RP04-BOUNDED-DISPATCH-REVIEW-2026-10-15.md), не заменяет их и не создаёт четыре новые задачи. Изменены executable instructions, не только обзор.

## 1. Выбирать по фактическому состоянию

| Что сейчас действительно отсутствует | Карточка | Достаточный ближайший результат |
|---|---|---|
| Ordinary caller, source или typed OutputRef seam | 00A адресно | Source→caller→real TOutput; не новый полный inventory |
| Реальный writer/reader/objective/checkpoint owner | 00B адресно | Один implementable owner contract, не duplicate trainer |
| Два observed lawful E3 результата для same input | 01A | Full-clean comparable alternatives и stable IDs; CONFIG_ONLY допустим как узкий первый урок |
| Новый valid signal и TRAIN_ONE grant, update ещё не сделан | 01B | Один numerical E3 update, coherent C0/U и первый reader |
| E3 update уже есть, ordinary use ещё не доказано | 01C | Одна cold C0/U пара одного готового domain; повтор optimizer запрещён |
| E4 representation/update/consumer ещё нет | Нужная 02A/02B/02C | Настоящие directed typed order-2/3, state и actual consumer; E3/RP01 не prerequisite |
| Есть достаточный ordinary-use receipt любого lane | 03B | Выбранный независимый вопрос; затем два домена и расширение, не обе ветки сразу |
| Реальному recipient нужна response-DPO pair | 03A | Chinese обе стороны + отдельный CODEX exact-pair judge; source-observed route/motif не обязан ждать |
| Есть consented feedback и compatible accepted parent | 04A | Новый lawful signal с journal/rollback и удержанием навыков, без новой UI prerequisites |
| Нужна независимая приёмка exact candidate | 05A | Настоящий отдельный CODEX review receipt, не самопроверка автора |

**State-first:** открыть existing journal и reuse sufficient compatible receipts. Applied/awaiting-evaluation не повторять под новым pair/update ID. Unknown attempt разрешается его прежним owner. C0 — прежнее совместимое обученное состояние на исправленном code без ЭТОГО update; U — один attributable update. Accepted P сохранять для восстановления; historical P-run не обязателен для каждого C0/U.

**Первое использование состояния:** один runnable domain достаточен для ADOPTION_OBSERVED_SINGLE_DOMAIN. 01C/02C могут переиспользовать достаточный trace из 01B/02B. Второй domain, static-order ablations, historical comparator и новая cancel/retry-кампания не блокируют first adoption. Domain-quality/retention experiments принадлежат 03B; deterministic probes — только владельцу реально изменённого риска. Это снятие дублирования, не отмена функционального объёма.

## 2. Копируемый prompt родителя исполнителю

> Выполни только назначенную существующую RP04 карточку и её текущую фазу. В контекст обязательно включи ПОЛНЫЙ current task packet, exact input receipts и относящиеся source excerpts; этот старт не заменяет подробную карточку. CURRENT TASK, NOT THE TASK, grants, allowed files/commands и expected return задай явно. Цель — ближайший разрешённый numerical learning effect либо его отсутствующий ordinary-reader/scientific evidence. Не начинать R01–R25 заново, не создавать ещё один trainer и не расширять Windows test coverage.
>
> Сначала проверь существующий signal/update journal, state и receipts. Не повторять уже сделанный update. Missing caller/owner resolve адресно через 00A/00B; готовые первичные источники и unchanged semantic dependencies reuse, не перечитывать весь архив. P=lubochka/xiigen-planning@llm_plan3; K=lubochka/llm_mvp_core@codex/dpo-implementation. Зафиксируй HEAD/blobs/actual read depth; прочитай affected code целиком и его current callers/typed consumers до изменения.
>
> Примени настоящие sections skills из карточки: архитектура owner/signal/state/consumer, phase planning, plan/code review, prompt specificity, human-readable evidence. Не выдавай skill name за чтение и не выполняй исторические 30/50-pass или blanket coverage как новые prerequisites. RAG→WorkLeft→primary G/F/V должен объяснять решающий claim. D/original task.md сохраняет typed many→many Unit, learned Flow corrections, async Orchestrator и higher-order Graph; WorkLeft RESEARCH-03 — exact pair, one-use, rollback и independent evaluation. Source table не полный аудит репозитория.
>
> RP02 domain laws/semantic roles/coreference/AST vectors и dynamic kNN нельзя заменять raw cosine или hits[0]. Используй versioned owner-provided representation; если она не выражает нужный закон, resolve эту зависимость либо возьми другой разрешённый runnable case, а не игнорируй закон. RP03 Unit/Flow и opposite lane frozen в isolated RP04 сравнении. LoRA сама по себе не tokenizer: нужен actual project transform→vector→consumer.
>
> Обозначь intervention: CONFIG_PREF/RESULT_RANK/DISPATCH_ORDER/SUBSET/actual STOP либо genuine structural route/motif. AxisSweep flags не multi-hop paths; capped ordinal prefix не всё domain space; stable CombinationId не dispatch ordinal; post-result ranking не экономит уже executed candidates. Clean batch не заменяет inner legality/final arbiter. Hard law/rights/types/effects veto не обходится learned score.
>
> Для contextual ranking проверяй совместные context×plan/motif features или nonlinear joint scorer. Linear concatenation [c(x),a(plan)] сокращает c(x) в pairwise margin и может выучить лишь global ranking. Narrow configuration lesson можно выполнить сразу и назвать честно; context-reversal panel позже ограничивает DOMAIN_ADAPTIVE claim, а не блокирует первый update. Не создавать второй tokenizer или огромный tensor product.
>
> Для каждого C0/U `(case,condition,repeat,attempt)` создавай fresh one-shot invocation wrapper штатной factory, сохраняя нужные learned weights. Проверяй mutable runtime scope: fresh wrapper не обязательно state isolation. Не снимай guard ALREADY_STARTED и не обнуляй модель при fresh factory. Prepared scenario/legacy pre-route/fallback и реально выполненный target-reader/hash записывай отдельно. Хороший legacy ответ не доказательство E3/E4 state use.
>
> First learning: новый lawful observed informative signal→actual numeric theta old/new→coherent parent/C0/U→same-entry cold ordinary reader→materialized requested TOutput/final arbiter. Одного готового domain достаточно; sufficient receipt reuse. Checkpoint/hash/loss != gain; loaded != used; selected target != executed target. Trial не production promotion. Grants на code/train/providers/effects должны реально существовать; уже выданное разрешение повторно не спрашивай.
>
> После first adoption 03B проводит один ближайший independent вопрос: fixed-pool rank, fresh-pool reach, dispatch cost или context reversal. Затем двухдоменный пилот (C# и EN→RU предпочтительно; замена с явным UNEXAMINED), шире SQL/ID-SYNTH/DTC/mixed и реальные B0/B1/B2. Source families независимы, T не используется для отладки или обучения. Общий user denominator сохраняет failures/bypass; target-used subset отдельный. Отдельно mapping/reach/rank/lawful success/refusal, latency и ВСЕ branch/resource/state costs, missing sensors NA.
>
> CODEX с astra 6 ultracode выполняет HUMAN case/choice selection. Разрешённая внешняя открытая китайская модель пишет ОБЕ final chosen/rejected. ДРУГОЙ CODEX с astra 6 ultracode оценивает unchanged exact pair. Post-update examiner и final exact-SHA reviewer — отдельные actual invocations с receipts. Source-observed route/motif objective не standard response DPO автоматически. RP01 synthetic-dev exception не переносить в RP04 молча.
>
> Source/doc/science/NO_CHANGE: ноль новых product tests. При реальном deterministic code change reuse только smallest relevant existing law/identity/cancel/once-only/rollback probe и нужную touched-project compilation. Не выполнять каждый пример как обязательную suite; не удалять guards и не retune expected stochastic answers. Native/release verification остаётся отдельной delivery обязанностью, не критерием обученного качества.
>
> Верни complete task receipt: source/skills/grants, actual owner/reader/typed output, diff или reuse, GOOD/BAD/EDGE, numeric state или точный gap, stable IDs/invocation/runtime scope/route_class, raw domain/cost/denominators либо NOT_MEASURED, minimum probe/zero, actual model/review receipts либо NOT_INVOKED/NOT_PERFORMED, exact commit/blob readback, falsifier и один next owner. Не называй документ обучением или author review независимой приёмкой.

## 3. Примеры и ошибки

**GOOD:** один valid CONFIG_PREF update реально прочитан штатным cold consumer на C# case; narrow claim назван, 03B получает raw output для independent exam. **BAD:** повторить этот урок ради закрытия новой карточки, требовать заранее все домены или выдать unused checkpoint за gain. **EDGE:** stale law/coordinate, ambiguous referent/schema, revoked consent или unknown prior attempt — сохранить compatible parent и resolve только соответствующий seam.

SQL NullGuard→Bind — условная scenario dependency, не универсальный закон; nullable parameter preparation может быть lawful. EN→RU agreement следует реальному referent/role, не любому предыдущему feminine noun. Timeout не termination uncooperative child; deliberately hanging in-process job не нужен. При Windows SAC использовать только подтверждённый supported CORE_ONLY entry с IDE_NOT_VERIFIED, не mock и не повтор Windows Actions.

## 4. Публикация и продолжение

Все прежние функциональные owners, 12 task IDs, 13 optional child prompts, E3/E4 независимость, domain-law kNN, Unit/Flow, DPO roles, higher-order/feedback/PC/DTC obligations сохраняются. Child packet использовать только для oversized seam: exact materialized sources/skills/rights/examples/return, read-only evidence, без overlapping writer и нового gate.

SHA-protected task-scoped Git writes сохраняют concurrent работу. Readback после публикации обязателен. Эта ревизия меняет план и prompts; code, data, weights, training, domain gains и independent Astra acceptance не выполнялись.
