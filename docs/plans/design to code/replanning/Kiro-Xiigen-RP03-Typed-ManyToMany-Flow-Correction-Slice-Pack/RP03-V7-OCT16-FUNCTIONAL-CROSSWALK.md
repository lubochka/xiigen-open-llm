# RP03 V7 — функциональная сохранность и границы минимальной проверки

16.10.2026. Карта связывает исходные ожидания с прежними 15 task IDs; не создаёт новые rounds. Это design crosswalk по прочитанным dispatcher/board/manifest/architecture и адресным карточкам, **не утверждение полного нового аудита всех сохранённых cards или всего Core**.

## Требование → владелец → предметное свидетельство

| Ожидание | Существующие owners | Как исследовать результат, а не покрытие строк |
|---|---|---|
| Все исходные domains/WorkLeft/RAG, не один S04 | 00A | Семь строк requirement→opened primary→actual code/UNKNOWN→owner; missing source не скрывается. |
| Реальные boundaries и consumer | 00B | Actual producer→CandidateId/OutputRef→typed resolver→next Unit; не считать hash payload. |
| Настоящий feedback и independent examination | 00C; children 00C-A/B | Source-family split, provenance/rights, real independent signal; exam labels не в trainer. |
| Unit many→many | 01A | q1→o1,o2; q2→o2,o3 — четыре ассоциации, typed schema/request provenance; не по enumeration order. |
| Materialized typed results | 01B | OutputRef resolves to actual compatible TOutput; wrong tenant/schema/version denied. |
| Unit output становится input следующего Unit | 01C | Actual next ExecuteAsync и output; handoff-only не выполненный шаг. |
| Изолировать обучение Unit от Flow | 01D optional | Реальный Unit W/state при frozen Flow; без смешанной causal attribution. |
| Flow real scorer / pre-outcome semantic features | 02A | Numeric W/reader/writer/training owner, immutable law/coordinate snapshot; CandidateId/order/label не признаки. |
| Ранний настоящий Flow урок | 02B | Lawful independent preference, formula/finite numeric delta, actual reader; score/rank/quality отдельно. |
| Durable trial и feedback once-only | 02D | Новый PID читает coherent W/schema/ledger/parent; replay/rollback; serving untouched. |
| Flow→typed next Unit и scoped ordinary adoption | 02C BUILD/JOIN/ADOPT | Сначала isolated real composition; затем accepted W/registry, actual Host; нет цикла exam/admission. |
| Proper DPO и роли | 03A optional / RP01 | CODEX Astra selection, external Chinese обе стороны, отдельный CODEX Astra judge, exact bytes, собственный Student objective/state. |
| Domain mapping / accuracy / path quality | 03B, RP02 | Same held-out cases C0/T; domain confusion, unknown/abstention, type/role/semantic mistakes и lawful end result. |
| Dynamic kNN / domain tokens / semantic coordinates | RP02 + 02A/03B join | Law-conditioned features/neighbor compatibility; separate one-factor arms, позже kNN×F1; обычное hits[0] не adequate design. |
| LoRA semantic projection | RP18 + 03A/03B interface | Coordinate/alignment identity и actual index consumer; Student LoRA ≠ embedding/token projection. |
| PC compactness and next feedback | 03C + RP05 | Real normal cold Host, whole-process memory/disk/time/cost, opt-in next event; не только размер W. |
| Async/higher-order patterns | RP04; RP03 compatible ports | Не потерять effects/cancellation/typed relationships; не задерживать первый RP03 update полной реализацией Graph. |
| Independent quality judgement | 04A; domain grader 03B | Exact candidate/artifacts, separate actual CODEX/Astra call, scope/uncertainty; авторская проверка не independent PASS. |

## Как разгрузить executor context без потери функциональности

Для выбранной карточки загружаются dispatcher relevant sections, одна active card, её actual inputs, 3–5 relevant skill sections и exact primary/code dependents. Не загружать старые V6/V7 appendices как дополнительные приказы. Если child получает domain exam, передать frozen manifest и **scope**, а не всё исследование R01–R25.

Все 15 IDs и 9 children сохранены. В этом ремонте непосредственно переписаны 02A/B/C/D, 03A/B/C и 00C-A. Другие восемь parent cards и восемь children сохранены; их полное новое semantic read/rewrite **не заявляется**. При обнаружении в сохранённом child old ordinary-only language использовать declared 03B trial scope и отметить Host NOT_ADOPTED, не фальсифицировать admission; deeper independent child review остаётся отдельным bounded остатком, не prerequisite первого update.

## Минимальная проверка — по реальному риску

READ_ONLY/source/planning/grading/coordination = **0 product tests**. Матрица ниже не список обязательных новых tests; owner выбирает только реально затронутую границу и сначала переиспользует existing evidence.

| Изменилось | Самое узкое достаточное инженерное свидетельство | Чего это не доказывает |
|---|---|---|
| Update formula/reader | Finite/sign numerical example и actual W reader | Domain generalization |
| Event application | Повтор actual event не даёт второй commit | Уникальность всего будущего потока без проверки lineage |
| Association/type/rights | Actual legal payload и затронутый denied case | Качество generated answer |
| Atomic trial state | Coherent checkpoint/new PID/recovery нужной ветки | Ordinary admission |
| Serving publisher | Accepted tuple transition и touched rollback/denial | Положительный gain без независимого exam |
| Domain algorithm | Independent family C0/T outcomes/law/uncertainty | Windows release readiness |

Доменный compiler/SQL engine/type/grammar oracle — допустимая проверка конкретного результата обучения. Он не превращает задачу в массовую learner unit-test coverage. Нельзя заменять independent domain grading одним флагом PASS.

## Source reading receipt этой ревизии

Прямо открыты: актуальные RP03 README/manifest/dispatcher/science/board/AGENTS, source architecture; 00A и восемь переписанных cards; N26–N31 atlas, исходный atlas verification receipt relevant sections; WorkLeft RAG usage guide opening и RESEARCH-03 current delta; `previous rounds thinking results.md` opening 1–65 (включая R07 status correction, не весь огромный файл); V/RAG-06 opening 1–80 → F/ARBITER-KNN-AUTHORITY-AMENDMENT opening 1–105. Исторические E51 и corpus counts не подтверждены этой сессией как runtime/current measurements.

Core непосредственно: `RegistryDynamicGraphEdgeAuthorityResolver.cs` целиком на K58d12; `DynamicGraphEdgeAuthorityExecutionPipeline.cs` ExecuteAsync от начала до request/guard dispatch в полученном фрагменте. Empty metadata подтверждено на этом call site; отсутствие всех out-of-band readers не доказано. Остальные Core paths в карточках — required future executor reads, не ложное заявление полного source audit.

Skills actually scoped-read: architect-behavior-classifier 1–100; phase-planning 1–120; plan-review 1–110; generated-code-review 1–100; prompt-patch-authoring 1–130; conversation-intent-and-self-reflection 1–110; trainable-architecture-decision 1–140. Другие навыки в карточках предписаны следующему исполнителю, не выданы за полностью прочитанные здесь.

**Граница:** документы исправлены; Core implementation/weights/model calls/training/real exams/PC и независимая Astra этим действием не выполнялись. Не доказана гипотеза test-fitting по истории; она не используется как факт для удаления safety checks.
