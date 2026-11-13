# R16 — review and repair findings (13.10.2026)

**Current documentary status: REPAIRED_REQUIREMENTS_DRAFT / EXACT_LEARNING_EXECUTION_NOT_READY / LEARNING_NOT_MEASURED.** R16 is an application of source-bound learning requirements, not an independent architecture selection, code implementation or product-learning result.

## Что оказалось неправильно в предыдущем исполнении R16

1. Действующий WorkLeft-02 teaching Chinese model и WorkLeft-INTEGRATION-01 — фактические входы будущих учебных задач — не были обновлены, несмотря на claim «active requirements applied». Прежний WorkLeft-02 описывал Teacher role слишком общо; это допускало повторное смешение CODEX/Chinese/Student/evaluator.
2. R16 изменил «Алгоритм создания ДПО.md» и «planning of testing.md», хотя оба маркированы как исторические источники, а run-prompts/R16 запрещал переписывать первый. Восстановлены R16-добавления из исторических файлов, текущая норма помещена в действующие WorkLeft.
3. R16 во время своего первого прохода не наблюдал R15. Теперь результаты R15 в дереве есть, но **только parent adversarial review NEEDS_FIX** для candidate c1a428a9f753bd97bb46544b02e47705d7416128. Отдельный CODEX GPT-6 Astra UltraCode independent review **NOT_PERFORMED**. «R15 absent» как текущее состояние устарело; «R15 PASS» было бы выдумкой.
4. R12 RUN-LOG остаётся REEXECUTION_REQUIRED до A–F hybrid synthesis. R13 RUN-LOG имеет STATUS OVERRIDE REPAIR_REQUIRED до новой populated first-card и readback. E1/DPO seam — кандидат, не автоматически выбранная архитектура/первый опыт. R14 фраза «R13 не missing» не устанавливает выполнение repair.
5. Предыдущее R16 CHANGE-MAP подтверждало readback маркеров, но не effective dispatcher/compiler propagation. До R20 нельзя выдавать документальные поправки за новый compiled S04/S06/S07 prompt или реализованный algorithm.

## Реальный документальный repair

- WorkLeft-02: exact delegated human-role выбор CODEX/GPT-6 Astra UltraCode; **обе финальные DPO стороны генерирует разрешённая внешняя открытая китайская модель**; отдельно CODEX/Astra оценивает exact pair; Student trainer и post-update evaluator отделены; provenance, one-pair/no-repeat, parent/no-update/holdout, модель результатов и ограничения readiness.
- WorkLeft-INTEGRATION-01: полная гибридная программа ILearningUnit (typed many→many), ILearningFlow (коррекция Unit), ILearningOrchestrator (async маршруты), ILearningDynamicGraph (higher-order перенос); deterministic laws + learned authority; E1–E4, domain laws/token/vector, dynamic kNN multi-neighbor, LoRA/projection, retention, PC feedback + cost.
- WorkLeft-00/RESEARCH-03: прямой статус R12/R13/R15 и запрет на преждевременное READY/selected seam.
- Original DPO и historical planning-of-testing: только отмена вставленного R16 «current precedence» блока, без изменения их исходных требований; active replacements находятся в WorkLeft.
- Исправлен run-prompts/R16: actual R15 input, отдельные подзадачи A–D, source/prompt/skills/good/bad/edge, live consumers, explicit readiness criteria.

## Граница того, что пока нельзя утверждать

R12 hybrid design и R13 filled first experiment остаются открытыми. R15 independent Astra review не выполнен. Нет фактического latest code-trace для выбранного first learner recipient/parameter group/ordinary consumer; R18 не должен подписать exact executable E1–E4 packet как READY. R17 должен отдельно изменить policy Windows-vs-learning evidence. R19 сохраняет все slice/extras/DTC/IDE обязательства. R20 проверяет и при необходимости меняет effective prompt/compiler consumers: этот R16 документально применён к WorkLeft, **не означает** изменения компилятора.

Никаких DPO generation, CODEX/Astra judgment, Student update, Windows/Actions, product tests, code, model weights, datasets или эмпирического domain improvement здесь не выполнялось. Показатели качества, оптимальная kNN, vector tokenization, LoRA transfer, PC fit = **NOT_MEASURED**.

## Порядок будущей функциональной проверки

Для E1/E2/E3/E4 требуются отдельные frozen lawful source families, one-factor contrast, baseline/challenger/no-update, проверка обычного consumer, независимые оценщики, retention/domain/worst-domain, время/полные ресурсы, prebound thresholds и IMPROVE/REGRESS/INCONCLUSIVE/INVALID_EVIDENCE. Детерминированные invariants/release safeguards проверяются по своему риску, test coverage не является доказательством обучения.

Статус публикации подтверждать отдельным GitHub remote readback; факт write не является semantic independent approval.
