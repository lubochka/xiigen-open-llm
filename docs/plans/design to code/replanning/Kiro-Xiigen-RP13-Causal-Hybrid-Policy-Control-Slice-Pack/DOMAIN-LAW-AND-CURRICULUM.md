# RP13 — предметные законы, семантика и curriculum для выбора причины

## Семь исходных направлений, не подменять друг другом

| Domain/source (WorkLeft-RESEARCH-01 / R08 / R21) | Hard law examples; verify exact primary | Пример причинного противопоставления |
|---|---|---|
| Перевод EN↔RU | смысл, направление, терминологические якоря, неоднозначность, protected tokens | tokenizer потерял отрицание vs Student плохой перевод при сохранённом токене |
| Оркестрация | типизированный переход, срок/await, idempotency, lawful fan-in, cancellation | policy выбрала дорогой graph route vs executor нарушил cancellation |
| Figma→HTML+CSS | source-design provenance, hierarchy, screen sizes, accessibility, asset constraints | vector неверно распознал компонент vs renderer исказил известный layout |
| C# documentation | actual signature/member/namespace, версионность, behaviour/callers | retrieved wrong method vs Student придумал несуществующий API |
| Angular documentation | stack/version, component/module/signal/router scope | classifier потерял Angular version vs doc writer hallucinated route |
| SQL documentation | dialect/schema/constraints, join & NULL semantics, source validity | incorrect domain dialect vs right retrieval but wrong SQL explanation |
| DPO Builder for six | weakness provenance, meaningful preference, rights, exact chosen/rejected authorship, no leakage | policy дала слабую weak-signal задачу vs exact pair rejected as BOTH_BAD |

Дополнительные домены, смешанные задачи и `UNKNOWN` исследовать явно, но не объявлять закрытие всех семи по одному C# примеру. Для первого дешёвого baseline — C# и EN→RU, третий контраст SQL; остальные получают паспорта и later measured arms, а не фиктивные «7/7».

## Минимальный паспорт per domain×intent

`subject+direction+stack_version; source authority/date/license; candidate labels and allowed multi-label; immutable hard laws vs empirical soft assumptions; legal operations; requested output; meaningful token units+semantic features+vector schema/version; appropriate k policy/neighbor relevance+counterexamples; typed Unit/Flow relations; async/graph alternatives; Student/LoRA target; abstain policy; evaluation independent rubric; class of wrong but plausible outputs; source family group; forbidden leakage; reviewer qualification`.

Заморозить настоящие TRAIN/OP_DEV/CERT по **семействам происхождения**, не random row-per-row; per-domain law version, novel family, known vs unknown, mixed/negative, ties/abstention. Все labels/validity labels — независимые источники или компетентный эксперт, не generation after examining T.

## Демонстрация GOOD / BAD / EDGE (гипотезы, не результаты)

- **GOOD C#:** два допустимых document lookup paths; exact API + correct version и скромная memory cost; learned selector picks best **among enumerated permissible**; after local lawful feedback chooses cheaper path on sealed new families, confirmed separately.
- **BAD SQL:** test puts dialect name in both training and cert through paraphrases, optimizer scores 100%; это leakage, outcome `INVALID_EVIDENCE`.
- **EDGE multilingual:** `“не удаляй”`/`“do not delete”`, mixed C#+RU, unresolved SQL dialect, untrusted source, no lawful candidate, both candidates tied. Hard-law abort or calibrated abstention может быть правильнее, чем confident but invalid path.

До reveal treatment результатов определить oracle allowed domain sets, lawful path candidate universe, rubric denominator/N, cost weights and budget. 95% — дальняя цель из WorkLeft, не численно подтверждённый факт этого пакета и не универсальный ранний gate.
