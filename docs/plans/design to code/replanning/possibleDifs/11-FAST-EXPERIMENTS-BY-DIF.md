# 11 — Мини-эксперименты для всех 44 гипотез possibleDifs

**16.10.2026 · READY_AS_EXPERIMENT_PLAN / NOT_RUN.** Это дополнение к [01-PRIORITIZED-DIFFERENCES](01-PRIORITIZED-DIFFERENCES.md) и [08-CAUSAL-DIAGNOSTICS](08-CAUSAL-DIAGNOSTICS.md): короткий A/B с заранее замороженными критериями вместо нового 500-run, Windows CI или подгонки тестов.

## Стандарт одной проверки

Каждую гипотезу проверять по четырём независимым воротам:
1. **Подтверждённое отличие** — actual path:method:SHA, что изменено в representation/objective/consumer.
2. **Вызов на неудачном RP01** — actual trace, иначе только LOCAL_DIFFERENCE и невозможен вывод о причине нулевой генерации RP01.
3. **Контроль одного фактора** — одинаковые модели, train/heldout cases, source families, decoder, admissible laws и сравнимый бюджет A/B; difference явно названа до запуска.
4. **Изменение качества** — paired heldout outputs **реального consumer** с независимой рубрикой, критическими ошибками и abstain, а не только loss/weights/checksums.

Маленькая проверка даёт **диагностический эффект**, а не статистически надёжную сертификацию на 95%. Для новой DPO пары сохранять настоящие teacher/judge роли, однократное применение и разрешённое происхождение; старые C#500, ENRU500 и их frozen панели **не переобучать/не переоценивать**.

## M01 — Cold B0 (DIF-01; RP01)
**Сначала без обучения:** 4 новых открытых запроса на B0 и C50, same tokenizer/settings, записать free response, logprob выбранных вариантов и длины. Затем **отдельный dev-only warm-up опыт** на 8 коротких chosen-only supervised train examples, 8 новых heldout, A=same B0/no-update, B=same B0+short SFT. Сравнить first-token/whole-answer correctness и способность overfit train. Это проверяет наличие языковой предпосылки, не качество DPO. Сравнение DPO-only и warm→DPO проводить **позже** только с новыми разрешёнными парами и owner approval для A/B one-pair/no-repeat; нельзя запускать один применённый pair ID второй раз. Stop при отсутствии basic overfit.

## M02 — Coordinate semantics/TrainingVectorBuilder (DIF-02,03,16,17,24,37; RP01/RP02/RP18)
**Нулевой стоимостью Student:** построить три 60D vectors одинаковой sparsity, но различным расположением/знаком/величиной; оценить текущий VectorSignal. Он должен вернуть одинаковый scalar, если исходный код не изменён. Далее отдельный consumer-test: 8 train + 8 unseen law-conflict pairs, A=локальные root/hashed coordinates, B=shared versioned order+law feature, **тот же** KNN/solver. Primary: real output correctness. Равный scalar доказывает только механическую инвариантность, не повышение качества от B. Если StepVector не читает координаты, пересчёт координат без нового recipient не изменит их градиент.

Источник: [TrainingVectorBuilder](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/23.SemanticCompilerAdapter/TrainingVectorBuilder.cs#L14-L91), [DpoTrainer.VectorSignal](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L1159-L1166).

## M03 — Exact DPO gradient, batch count, LoRA (DIF-05,09,21,23,33; RP01)
**Быстрый gate до любого bulk:** проверить актуальность exact-v2 ResponseMask, frozen reference и только разрешённых q/v LoRA keys. На маленькой новой dev-only разрешённой паре — analytical vs finite-difference directional gradient по одному LoRA parameter, тот же loss и dropout=0. Если ошибка знака/маски — STOP и targeted fix. Если успешно, не более 8 новых уникальных train pairs с коротким train-only overfit и 8 других heldout; acceptance только по actual semantic output, а не margin. Не повторять старые DPO pairs. Rank/target module эксперимент — только после доказанного bottleneck.

Источник: [DpoTrainer](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L832-L1119).

## M04 — DPO teacher, semantic quality, families (DIF-04,18,19,20,41; RP01)
**Без обучения:** 4 C# + 4 ENRU **открытых** примера по непересекающимся смысловым семействам, независимо проверить chosen/rejected, C# compile/runtime и ENRU negation/actor. Считать VALID / TIE / BOTH_BAD / UNJUDGEABLE, длины и source truth. Программные 500 simulation строк не считать 500 независимыми навыками; если нет фактической внешней разрешённой китайской модели, не выдавать данные за external teacher output. Чтобы доказать влияние качества пары на Student, позднее нужен отдельный *разрешённый* A/B tiny training с независимым heldout: сама проверка содержания не доказывает learning gain.

## M05 — KNN, порядок и law-aware выбор (DIF-07,13,14,15,35,36,44; RP02)
**ПОЛНЫЙ ПРОТОКОЛ УЖЕ ГОТОВ:** [11A-KNN A/B](11A-MICRO-KNN-ORDER-CAUSAL-PILOT.md) + [16 train / 16 heldout fixture](11A-MICRO-KNN-ORDER-FIXTURE.json). А — настоящий KnnLogicalUnit raw hashed BPE histogram; B — тот же Unit с единственным order-aware input feature. Primary B≥12/16 и не менее +4/16 correct, плюс label-flip negative control и 3 ANN repeats. Не говорить, что KNN виновен в RP01 direct DPO, если RP01 этот Unit не вызывает. Adaptive-k / law filter / negative warning / abstain — **отдельные** однофакторные опыты при положительном сигнале, не всё за один патч.

## M06 — Trainable Unit/Flow/Orchestrator/Graph (DIF-08,25,26,27,28,29,38; RP03/RP04)
**8 коротких typed heldout association cases:** фиксированные Units/inputs, A=старый static Flow mapping, B=один changed **learned edge/Unit recipient** при том же solver и budget. Primary: exact правильные OutputRefs→payload и recipient associations/8. Если нет runtime trainable state+consumer — **MISSING_RECIPIENT**, а не придуманный успех. Отдельно, после этого, проверить два безопасных Orchestrator branches или unseen Graph composition; не использовать production side effects. Прямая генерация RP01 не обязана вызывать этот путь.

## M07 — Checkpoint/load/serving (DIF-31,32; RP01)
**0 training, 2–3 prompts:** identical policy in-memory → saved → restored → cold new process. Fixed tokenizer+LoRA keys/RNG/decoder. Ожидается exact parity logits/selected tokens (или явно обоснованная численная tolerance), иначе source of mismatch. На обычном user consumer проверять только при принятой модели; RESEARCH_ONLY_NOT_ADOPTED ≠ ошибка восстановления. Это fidelity, а не quality gain.

## M08 — Исторические E09/E34 controls (дополнительное расхождение)
**0 neural training:** E09 baseline bigram и structural KNN оценить на одинаковых val indices и global AST-kind labels (original per-file labels отдельно); E34 tuned alpha использовать только вне final val, затем BM25 и fusion на **одних** heldout examples, не 18 против 9. Метрическая проверка истинного исторического gain нужна прежде port старого механизма. [Точный source audit](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md).

## M09 — Tokenizer/decoder/greedy decision (DIF-06,10,22,39; RP01)
**0 training:** 4–8 открытых prompts, C0/C50, сравнить train vs Generate framing, BPE token roundtrip, EOS ID/stop rule, первые logits и top2 gap, max token limits, free string. Если хочется изменить stopping policy, сделать **изолированный** A/B decode с теми же weights, без изменения checkpoint/tokenizer; measure valid free answer count, а не число сдвинутых logits.

## M10 — Preference vs actual answer (DIF-11; RP01)
**0 training:** одна небольшая открытая panel из 8 задач (C# nullable / ENRU negation / SQL NULL — отдельные domain rubrics), одинаковый decoder, before vs frozen trained research candidate. Primary task correctness/8 + critical errors + abstain; pair margin отдельно. При 0→0 и изменённых весах verdict **NO_SEMANTIC_GAIN_OBSERVED**. Панель из 8 не является окончательным blind certification.

## M11 — Replay, feedback, domain transfer (DIF-12,30,34,40,43; RP05)
**Начать только при nonzero old-domain competence**. Иначе 0→0 не retention. После законного нового урока A=no-replay, B=authorised policy, same fixed old/new heldout, no repeat of applied pairs, no heuristic ×10 hardcoding, full cost. Если текущие права запрещают повтор — NOT_AUTHORIZED и STOP, а не скрытый replay.

## M12 — Cost (DIF-42; RP01 performance)
Одна короткая profile-specific попытка без общего Windows CI: reference/policy forward, backward, optimizer, serialization, wall RSS и полный собственный CPU cost. Primary utility per full wall/cost **только при measured semantic quality gain**. Вложенные таймеры не суммировать повторно. Speedup без качества не закрывает training issue.

## Карта первых экспериментов на каждую из 44 позиций

| DIF IDs из 01 | Первое различающее действие |
|---|---|
| 01 | M01 |
| 02,03,16,17,24,37 | M02 |
| 04,18,19,20,41 | M04 |
| 05,09,21,23,33 | M03 |
| 06,10,22,39 | M09 |
| 07,13,14,15,35,36,44 | M05 |
| 08,25,26,27,28,29,38 | M06 |
| 11 | M10 |
| 12,30,34,40,43 | M11 |
| 31,32 | M07 |
| 42 | M12 |
| Исторические E09/E34 | M08 |

**Covered IDs: 01–44 без пропусков.** Это 12 разновидностей будущей проверки, НЕ 44 выполненных запуска. Один эксперимент не должен смешивать два изменяемых механизма.

## Минимальный возвращаемый результат

Один receipt для одного run: experiment_id, DIF IDs, actual core/planning SHA, fixture hash, hypothesis/falsifier, affected current consumer, A/B one-factor delta, train/heldout sources and family IDs, A/B case-level outputs, paired correct changes, critical/abstain, wall+peak own-process memory, unchanged reference/heldout provenance, actual call into RP01 yes/no, verdict LOCAL_GAIN/NO_GAIN/REGRESS/INCONCLUSIVE/INVALID_EXPERIMENT/NOT_RUN/NOT_AUTHORIZED, next one owner.

**Skills:** архитектор (trainable-architecture-decision + human-readable-architecture-audit), phase-planning, plan-review, code-examination, experiment-design, improvement-measurement, prompt-patch-authoring, root-cause-tracing, data-flow-integrity, conversation-intent-and-self-reflection — читать оригиналы в [llm_mvp_core/docs/skills](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/docs/skills). Отчёт отличает CODE от HYPOTHESIS и не заявляет работу эксперимента без log/real output.
