# RP01 — новый режим BATCH500; прежние карточки только по назначению

**Для новой работы после поручения15.10.2026 не проходить старую очередь ниже.** Открыть [единый dispatcher](../../RP01-EXECUTE-NOW.md), [контракт BATCH500](../../RP01-OFFLINE-TEACHER-BATCH500.md) и [пять новых подробных карточек](../batch500/README.md). Получить latest handoff действующего исполнителя; не повторять его работу или старый optimizer.

## Действующее соответствие задач

| Старый вход | Новый default для будущего опыта |
|---|---|
| 02B/S/A/J — создание пары | [B500.01 DATA](../batch500/RP01.B500.01-DATA.md): CODEX Astra S → Astra simulated BOTH author → отдельный CODEX Astra J; первые50, затем500 |
| 00B/01A — admission/material gap | [B500.02 ADMISSION](../batch500/RP01.B500.02-ADMISSION.md): offline exact shard без local Qwen author/evaluator |
| 01B/03A/T0/T1 — обучение | [B500.03 TRAIN](../batch500/RP01.B500.03-TRAIN.md): короткий TRAIN probe1–4, real batch/continuation,0/50/100/250/500 |
| 02A/03A/T2 — научная оценка | [B500.04 EXAM](../batch500/RP01.B500.04-EXAM.md): independent Q_law/critical/uncertainty, малый semantic gain вместо +2/12 |
| 03B/04A–E — домены/retention/hybrid | [B500.05 DOMAINS](../batch500/RP01.B500.05-DOMAINS.md): EN→RU consumer500, retention, один causal policy contrast |

S выполняет человеческую роль выбора; A честно маркирует `ASTRA_SIMULATED_TEACHER`, не Chinese/Qwen; J и E — разные назначения CODEX `astra 6 ultracode` с реальными receipts. Actual Student локален и не симулируется. Новый режим не наследует Chinese-only schema, local evaluator или обязательный60-slot C0 prefix. Его runtime adapter должен быть реально реализован, а не объявлен существующим.

## Сохранённые старые source/recovery карточки

Исторические bodies остаются полезны для exact code references и прежнего frozen run. Их old first-root/+2/12/5-repeat/Chinese-only инструкции не управляют BATCH500. Ни одна из16 карточек не является новой обязательной gate.

| Reference | Когда читать |
|---|---|
| [00A](RP01.00A.md) | Один недостающий primary/source-law вопрос, не полный RAG audit |
| [00B](RP01.00B.md) | Неизвестный actual legacy journal/consumer |
| [00C](RP01.00C.md) | Конкретное consequential решение или candidate review |
| [01A](RP01.01A.md) | Proven exact pair/mask defect |
| [01B](RP01.01B.md) | Proven objective/LoRA/journal defect; current B500 contract выше |
| [01C](RP01.01C.md) | Действительно принятые weights не доходят до cold consumer |
| [02A](RP01.02A.md) | Frozen legacy exam bindings, не новый default exam |
| [02B](RP01.02B.md) | Исторический настоящий Chinese authoring или separately assigned future provider integration |
| [03A](RP01.03A.md) | Только реальное адресное recovery/finishing прежнего one-pair run |
| [03B](RP01.03B.md) | Сохранённые feedback/continuation research references |
| [04A](RP01.04A.md) | RP02 domain/representation/LoRA semantic handoff |
| [04B](RP01.04B.md) | RP02 law-conditioned kNN/path handoff |
| [04C](RP01.04C.md) | RP03 Unit many→many/Flow correction |
| [04D](RP01.04D.md) | RP04 async ownership/higher-order graph |
| [04E](RP01.04E.md) | PC/causal hybrid references |
| [05A](RP01.05A.md) | Exact candidate independent review в заявленном scope |

Старые stage prompts в [children](../children/README.md) не назначаются по номеру. Complete negative historical run сохраняется без изменения verdict; unresolved in-flight effect восстанавливает actual journal owner. Старый native/scientific publication не обходится новым exploratory outcome. Research continuation isolated candidate и canonical serving promotion — разные полномочия.

READ_ONLY/REUSE_NO_CHANGE→0 product tests. Только изменённый deterministic риск получает ближайшую integrity проверку. Learning/domain/PC claims требуют своих реальных measurements. Новые tasks имеют состояние SPECIFIED_NOT_EXECUTED до returned execution evidence; публикация этого index не означает PAIRS_ADMITTED, STATE_CHANGED или QUALITY_GAIN.
