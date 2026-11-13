# MICRO-N33 — один маленький опыт, отделяющий настоящее улучшение ответа от изменения оценки

**Статус:** READY-TO-IMPLEMENT PROTOCOL; **NATIVE C# NOT RUN, STUDENT NOT RETRAINED, INDEPENDENT JUDGE NOT CALLED**. Дата 2026-10-16. Контроль source: `llm_mvp_core@32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7`. Этот файл — **план исполнения**, не result receipt.

## Цель, которую можно опровергнуть

Установить, что положительный эффект ранее работавшего trainable retrieval/Unit-пути **действительно меняет точность обычного ответа** и сохраняется после cold restore, а не возникает из подсчёта accuracy, ID пройденных тестов, имеющегося словаря или эвристического шаблона. Одновременно проверить новые N33 расхождения по статистике Unit и пределу вывода Student, не смешивая их с actual learning.

**Главный ответ эксперимента:** при одной и той же независимой постановке `HOLDOUT`, что выше после разрешённого маленького урока: `semantic_correct_count`, `critical_law_violations`, `old_case_retention`, `actual_final_human_answer`. Положительный `AccuracyAfter`/LoRA delta/PASS без этой метрики НЕ является успехом.

## Материал: два независимых узких проверочных блока

**A — настоящий trainable Unit / ordinary Flow.** Использовать реальные API:
- `new TranslationMachine()`, `TrainStructuredBatchAsync(TranslationLearningBatch)`, обычный `GetOrchestratorAsync` → `InlineTranslationOrchestrator` → `TranslationRoutingFlow` → `TrainedTranslationAdapter` → `ExecutorUnit` → `QualityVerifierUnit`.
- Сохранение: `SaveCheckpointAsync`, новый экземпляр `TranslationMachine.LoadCheckpointAsync`; original same state versus fresh restored.
- **Работающий proof fixture**, который можно переиспользовать без имитации: [Tests/TranslationDomainTests.cs L253–338](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/Tests/TranslationDomainTests.cs#L253-L338) применяет девять семантических/executor/verifier уроков и реально меняет три RU_EN пользовательских ответа. Для отдельного retrieval-положительного механизма [L1525–1554](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/Tests/TranslationDomainTests.cs#L1525-L1554) проверяет prefix-KNN без exact signature. Для слова из контекста [L368–405](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/Tests/TranslationDomainTests.cs#L368-L405) — wordmap fallback.
- **Важно:** эти существующие проверки доказывают возможность изменения ответа на использованных обучающих формулировках; это ещё не independent unseen generalization. Micro-N33 должен добавить законно размеченные source-family-disjoint cases. Нельзя скопировать ожидаемые значения из этих tests в «закрытую» панель.

**B — независимая оценка того же Student, без обновления весов.** Взятые неизменными уже сохранённые B0 и C50 кандидаты, один и тот же checkpoint SHA/tokenizer/prompt/settings `MaxNewTokens,Temperature,TopK,TopP,Seed`; варианты `MaxAnswerChars=1` и `MaxAnswerChars=500`. Использовать [CodeDocumentationStudentGeneration L61–76](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/14.ContinuousLearning/Training/CodeDocumentationStudentGeneration.cs#L61-L76) и сравнить simulation raw consumer с сервисным enforcement [SavedModelAssessment L548–577](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/14.ContinuousLearning/Training/CodeDocumentationSavedModelAssessmentService.cs#L548-L577). **Это measurement-parity probe, не новое DPO обучение.**

## Минимальный run plan (без Windows CI)

| Шаг | Состояние и контроль | Evidence, которую должен произвести исполнитель |
|---|---|---|
| 0. Freeze | 1 seed, current core SHA, compiler, machine IDs, source IDs, 1–2 law-families, 4–8 independently reviewed train examples per domain, 8 blind holdout per domain + 4 controls old/negation/order. Нет повторного применения старых DPO IDs. | `manifest.json`, legal pair/source provenance, split rationale, `input_sha256` / `heldout_sha256`; human reviewer fixed gold **before** running answers |
| 1. Pre-training | Fresh machine `M0`, `TranslationRoutingFlow` as normal; run identical candidate prompts including exact train prompts **separately marked as train-recall** and unseen law contrasts; capture actual text, chosen `AdapterId`, verifier verdict and top neighbour if reached. | `before.jsonl`, cold/bootstrap/heuristic trace, zero work claiming learning |
| 2. One small lesson | On new machine `M1`, train 4–8 **unique, permitted** examples using real `TranslationStructuredBatchBuilder.BuildGeneralBatch` or typed `TranslationLearningExample` from verified tests. Select **one feature path**: exact-source OR contextual wordmap OR prefix/structure KNN. Do NOT simultaneously change vectorization, verifier, tokenizer and KNN score. | `training-receipt.json`: exact UnitIds, mutated map/index counts, seed, train pair IDs, no claims from `AccuracyAfter` |
| 3. Same-input A/B | Compare identical prompts and decoding: A=M0, B=M1 actual ordinary Flow; include one near-but-different negation, two changed roles/names, source-family-disjoint holdout; human judge blind to A/B. Train-recall results report separately. | `matched-answers.jsonl`: all prompts, A/B text, whether learned recipient read, gold law, true/false correctness, false accept/reject, abstention and coverage |
| 4. Cold restore and prior skill | Save M1 using `SaveCheckpointAsync`; new process/load M2; compare M1 vs M2 exact output for same panel. Then, ONLY if first law learned, apply **one additional independent** lesson and retest old law + new law. | `restore-parity.json`, checkpoint/Unit-state SHA and original-versus-new old-case retention |
| 5. **N33-A denominator** | Without mutating M1, run heldout groups with **1 case of Unit X, 9 cases of Unit Y** under existing `MeasureHeldOutAccuracy`. Log group size, per-unit correct, macro avg, micro avg, **end-user Flow** correct. If synthetic labels must be controlled, expose this first only as evaluator arithmetic, not evidence of a real native translation gain. | `score-parity.json`, complete numerator/denominator/group weights plus real consumer count |
| 6. **N33-B length** | Without optimizer call, same existing Student and seed for both `MaxAnswerChars` values. Show raw `GenerateDetailed` IDs/text, `SettingsSha256` and disposition in each ordinary consumer. Only compare server admission after actual service invocation is confirmed. | `length-parity.json`: two raw outputs, SHA, `stopReason`, full UTF-16 char length and each consumer's accepted/refused status |

**Recommended implementation surface:** one disposable local .NET console project or a narrow research test in `llm_mvp_core` (outside protected `docs/plans/replanning-possible-diffs`); use the *existing* `TranslationDomainTests` helpers, don't rewrite training algorithm or manufacture admission capabilities. No GitHub Actions or Windows matrix. Run only the local short probe; if tools require a production-owned lawful DPO pair and external teacher is unavailable, defer the neural mutation arm rather than forge provenance. Store report/new hypotheses under planning/possibleDifs; never overwrite historical run JSON.

## Small concrete input composition, to be independently validated

Before running, ask authorised human-role CODEX/Astra to approve the task and exact expected semantics. For the **RU→EN** technical direction, one candidate train pair modeled after existing test source:

- Training source: `добавь проверку null в метод ValidateToken и не переименовывай ProcessQueue`.
- Positive translation example: `Add a null check to the ValidateToken method and do not rename ProcessQueue.`
- Distinct forbidden contrast: an output that changes `do not rename` into `rename`; this is a **law-contrast sentinel**, not an automatically accepted DPO rejected side.
- Blind source-family-disjoint queries must change the method/entity *and preserve the law*, plus a query that **intentionally requests** renaming so a naive negation-copy rule fails. Golden outputs are authored before inference by independent domain reviewers, not from the Student or deterministic template.

Second conditional block for EN→RU may use short copular polarity cases (e.g. `The door is open.` versus `The door is not open.`), but first confirm target listener `EN_RU` and [TranslationMemorySourceLaw](https://github.com/lubochka/llm_mvp_core/blob/32ac6e741ce4cfb2dd8ac024716e470f8d0f18f7/27.TranslationDomain/TranslationMemorySourceLaw.cs) only declares compatibility on its narrow typed clause; cannot extrapolate to every sentence.

**Domain relevance:** C# explanation AST needs structural source/operation rather than word order; ENRU/RUEN ordering and negation benefit from true clause-sensitive features; DSL `->` and SQL `NULL` require operator truth preservation; generic non-syntactic retrieval shouldn't be forced to pay for parsers. For broad claim, repeat AFTER positive pilot on >1 domain with separately judged gold.

## Predeclared acceptance and falsifiers

A genuine **Unit learning gain** is present only if:
- The trained ordinary Flow (B) fixes **at least 2/8 previously wrong independent heldout tasks** versus A with **zero additional critical law violations**, and improves or maintains *precision at comparable coverage*. Report the numerator even if this threshold is not reached; do not tune threshold after seeing answers.
- The result is tied by trace to a **training-mutated** word/phrase/KNN/verifier state rather than hardcoded bootstrap/template, and **restored M2** produces the same accepted semantic answers.
- No extra correct answers were copied from train into heldout, no gold is embedded in scorer, and domain judge is blind to arm. This is an exploratory minimum signal, not a 95% production claim. For 8 tasks, use per-case evidence rather than statistical significance rhetoric.

Falsifiers:
- B changes `AccuracyAfter` but not user responses → `SCORE_ONLY_NOT_LEARNING`.
- B changes training-prompt answers only; all unseen remain wrong → `MEMORIZATION_ONLY`; still a real learned exact-key memory, not generalization.
- B improves blind heldout but M2 after checkpoint changes the same answers → `RESTORE_PARITY_FAIL`, quality not durable.
- A and B have identical text despite distinct KNN/Unit state → `LEARNED_RECIPIENT_NOT_CONSUMED` (or state irrelevant to this panel).
- N33-A macro ≠ request-weighted micro but same ordinary answers → `METRIC_DENOMINATOR_DIFF` **not** quality improvement.
- N33-B raw generator unaffected by `MaxAnswerChars` but service accepts/refuses differently → `CONSUMER_POLICY_DIFF`; mark a regression only if the evaluation contract required parity and the same blind question changed grade.
- Source-native result disagrees with static analysis → correct this plan, retain the unexpected evidence, don't change rubrics to obtain PASS.

## Reviewer and reporting template

One **architect** traces `pair→typed Unit Train→checkpoint→ordinary Flow`, not source names alone. One **code reviewer** verifies exact methods, identity, isolation and checkpoint readback. One **experiment reviewer** enforces same questions/same denominators, negative controls and domain scope. **Prompt engineer** freezes semantic contrast, source/teacher legal provenance and examiner format. **Human communication** report must read:

> «На [N] независимых задачах trainable [Unit] изменил [K] пользовательских ответов, из них [C] стали правильными и [W] нарушают закон; при контрольном неизменном состоянии оценки различаются только по [причине]. Проверка fresh restore: [результат]. Это [да/нет] подтверждает причинное влияние отличия на качество именно [домен].»

No fictitious outcomes: `RESULTS_PENDING / NO_NATIVE_RUN` until an actual receipt exists. User's frozen RP01 `0/3`, `0/4`, and E09 historical panels are not overwritten.

<!-- CODEX-POST-ADMISSION-20261112-N33-METRIC:START -->
## Замороженная панель и обучение на её ошибке — приёмка от 12 ноября 2026

Источник конкретизации — [последующий аудит](codex-2026-11-12-post-admission-experiment-review.md) core `f089a7d55d359ff103d0cdd9ddfb26eee346a16b`. Исторический протокол выше не объявляется исполненным и автоматически не расширяется. Перед новым запуском проверить его current API/consumer и прежние budgets; формулировка READY 16 октября не заменяет эту проверку.

P58 показал границу, которую план N33 должен явно удержать: выбранная из EXAM ошибочная строка `e1` стала учебным corrective example, а единственный последующий правильный ответ всех A/B/C был exact-source повтором этой строки. Полезное усвоение 1/1 и остаток 0/7 нужно показывать рядом; прежнее 1/8 сохраняется как общий snapshot, но не как оценка восьми всё ещё отложенных задач. Приёмка N33 разделяет TRAIN recall, corrected-item recall, untouched-input score и family-disjoint score. Никакая одна из этих осей не обнуляет полезный результат другой. Исторический порог 2/8 относится к заявлению этого pilot о переносе; результат 1/1 на предъявленном материале обязан быть сохранён отдельно.

Исполнитель фиксирует exposure ledger до каждого ordinary output: RowId, input после реально используемой нормализации/tokenizer/signature, target, arm/checkpoint/phase, consumed training/correction IDs, был ли этот материал использован для подбора или feedback. Новое имя строки/проекта, изменённый target и новый буквальный текст не доказывают новый вход модели. Исправленная строка и эквивалентные input aliases выводятся из untouched denominator; fresh restore не делает знакомый input незнакомым. Выбор очередного урока разрешённым feedback остаётся отдельным от sealed final panel.

Различающие проверки ledger выполняются без Train: (1) correction для одной строки меняет 8 untouched на 7 untouched+1 corrected; (2) та же модельно видимая задача с другим RowId остаётся seen; (3) смена только target даёт seen-input/new-target; (4) genuinely новый input остаётся untouched; (5) пустой untouched набор получает UNMEASURED, denominator 0. Сохранённые actual тексты и gold при этой переклассификации неизменны. Для принятия нативного эффекта затем нужны действительные ordinary Flow readouts и свежий restore по назначенному малому бюджету, а не успешность Python ledger.

По P48 в двух плечах 12/16 совпадают, но три строки выиграны и три потеряны. Поэтому `matched-answers.jsonl` обязан давать gained/lost/unchanged с RowId и классом риска. Macro/micro равенство не заменяет парный разбор. По P49 единственный выигрыш нового проекта B находится на `c-f3-p2` (input3→target0), уже предъявленной new-TRAIN паре; это конкретное полезное усвоение, не unseen transfer.

Бюджет прикрепляется к точным исполняемым методам. Один helper/Fact и весь test class имеют разные суммы: текущие P58 105/56 против 261/104, P52 12/12 против 36/28 при полном успешном однократном исполнении. Эти числа лишь пример проверяемой ошибки области; в N33 вывести собственные суммы из discovery и реальных вызовов. Scorer-only блок B и арифметический блок шага5 не получают скрытых Train вызовов от широкого фильтра или общей fixture. Если дополнительный обучающий контроль действительно нужен, его доза называется в наряде заранее и входит в цену.
<!-- CODEX-POST-ADMISSION-20261112-N33-METRIC:END -->
