# opus-pass-28 — Semantic quality reality (repaired held-out / source-disjoint eval + numeric results)

Проход P28 из 50. Фокус: РЕАЛЬНОЕ семантическое качество трёх расширений — «починенный»
held-out / source-disjoint eval и его ЧИСЛОВЫЕ результаты. Оценка ведётся по живому ядру
`C:\Projects\llm_mvp_core` и по фактическим артефактам под
`artifacts/extension-runtime-token-quality-learnability/*`, а не по копиям пакета
`extension development`. Дедуп: НЕ повторяю per-machine held-out проходы P21 (prompt-to-task
циркулярный held-out, alias-словарь, ablation-no-op, тривиальные пороги), P22 (task→plan
словарный разрыв, stub-тренер, отсутствие runner-report), P23 (skill-selection
`NO_TRUE_SOURCE_DISJOINT_RUNTIME_EVAL_FOUND`, циркулярный accuracy:1, лексический bag-of-words),
P25 (исполненные DPO-числа: регрессия, UNKNOWN-delta, held-out ≈ шум — только wiki-домен),
P27 (token savings). P28 — про КРОСС-МАШИННЫЙ семантический QA-слой QUALITY-03 / QSG-24..27:
сам механизм измерения качества расширений, его acceptance и числовые выходы.

## Что проверено (bounded reads)

- Код QA-слоя: `43.SmartSkillsDomain/Quality03ProofContractRunner.cs` (955+, целиком по срезам),
  `Quality03ProofContractRunnerContracts.cs`, `Quality03NonInterRaterProofProducer.cs` (наличие),
  `ExtensionEvalOnlyQualityRunner.cs` / `ExtensionQualityScoring.cs` (перечитаны из P27 по
  части входов качества). Вызыватели `Quality03ProofContractRunner` = только
  `43.SmartSkillsDomain.Tools/Quality03ProofContractRunner/Program.cs` + xUnit-тесты (продового
  host-вызывателя нет).
- Артефакты приёмки: `artifacts/extension-runtime-token-quality-learnability/parent-review-QUALITY-03.json`,
  `quality-comparison-report.json` (96 rows), `quality-scores-adaptive.jsonl` /
  `quality-scores-baseline.jsonl` (по 96 строк).
- «Починенные» source-disjoint/held-out прогоны:
  `phase-04-prompt-to-task-universal-contract/20260623-100538/source-disjoint-eval-summary.json`
  и `.jsonl`; `qsg24-real-dpo-source-disjoint-eval/{quality-gates.json,material-only-held-out-ablation-eval.json,source-disjoint-split.json,summary.md}`;
  `qsg25/qsg26/qsg27-*/source-disjoint-check.json`.
- Состояние proof-contract очереди:
  `quality-03-runner-ready-input-ref-acquire-or-defer-no-scoring/.../acquire-defer-matrix.json`
  (counts 63/9/45/9), плюс родственные `quality-03-*-no-scoring` / `*-acquire-or-defer` /
  `*-approval-acquisition` каталоги.
- Грепы: `semantic_quality_claimed:true` по всему extension-runtime дереву → только
  `original_claim_flags` в provenance-repair-пассах, которые ИСПРАВЛЯЮТ флаг на `false`
  (`corrected_claim_flags.semantic_quality_claimed=false`); файлов с
  `semantic_quality_claimed:false` — 2321.

## Центральный вывод

Реального ИЗМЕРЕННОГО семантического качества трёх расширений нет. QA-слой качества построен
двухъярусно и на обоих ярусах не производит числа качества:

1. **Ярус 1 — `Quality03ProofContractRunner`** — по собственной метке
   `local deterministic QA/proof-contract artifact checker; not semantic model, trainable unit,
   product readiness, or quality claim authority` (`:14`). Он лишь агрегирует ПРИСУТСТВИЕ 63
   blocking proof-ref строк (7 категорий × 9 rows). Поля отчёта `QualityPassRows:0` (`:304`),
   `RunnerScorerRun:false` (`:307`), `RunnerReadyEvalInputsCreated:false` (`:306`) зашиты;
   `WhatThisDoesNotProve` (`:316`) прямо отрицает semantic quality. Даже на 63/63 он лишь
   «unlocks» ОТДЕЛЬНЫЙ eval-only scorer (`:280`), который в этом раннере не запускается.
2. **Ярус 2 — фактический scorer** — на диске существует только как FIXTURE: 96-строчный
   `quality-comparison-report.json` — константный `baseline=adaptive=2/5`, `delta 0`,
   `verdict FAIL_INCONCLUSIVE_REVIEW`, `reviewer_id "QUALITY-03-fixture-comparison-reviewer"`,
   каждая из 4 rubric-слоёв (`prompt_to_task`/`orchestration_reasoning`/`skill_selection`/
   `final_output_usefulness`) с note «No real … output evidence was evaluated». Родительский
   review при этом `verdict: ACCEPTED_FOR_PHASE_COMMIT`, но `semantic_quality_claimed:false`,
   `quality_result_claimed:false`, и в limitations: «real evaluated layer outputs are still
   required; source-disjoint eval is still required; ablations are still required».

Очередь proof-contract застряла: `acquire-defer-matrix.json` (2026-06-21) считает
`pass 9 / missing 45 / deferred 9` из 63, `approvals 0/9`, `runner_ready 0/9`,
`quality_pass 0/9`. Проходят только 9 held_out_split-строк (авто-PASS при наличии corpus
authority), остальное — «no-scoring», заблокированное отсутствием человеческого одобрения
redacted eval-входов.

«Починенные» source-disjoint прогоны (phase-04, QSG-24..27) действительно есть и формально
`PASS`/`accuracy 1`/`source_disjoint true`, НО:
- phase-04: `baselineInternalOnly:true`, `semanticQualityClaimed:false` — прогон идёт по
  детерминированному baseline-keyword пути (P21), eval-семейства авторски написаны под те же
  границы taskType;
- QSG-24: `evaluator_kind: material_only_source_disjoint_row_quality_evaluator_not_model_runtime`,
  `staged_checkpoint_used_for_generation:false`, `real_training_or_continue_training_run:false`;
  «score» = бинарное покрытие материалом `without_adaptive=0 → with_adaptive=1` (тавтология:
  строка получает 1, когда авторский adaptive-материал под неё присутствует); gate
  `semantic_quality_claimed_false:true` ОБЯЗАН быть true, т.е. прогон проходит ИМЕННО потому,
  что качество НЕ заявляется.

Итог: ни один артефакт не несёт легитимного `semantic_quality_claimed:true` из реального
прогона модели; единственные числа качества — константный FAIL-fixture (2/5) и бинарное
покрытие материалом (0/1). Числового доказанного семантического качества нет.

## НОВЫЕ дыры (P28)

| Component | Real-status | Evidence file:line | Gap | Closure steps |
|---|---|---|---|---|
| QUALITY-03 приёмочный quality-comparison (96 rows) | COPIED_DOC_ONLY / FIXTURE (accepted phase commit) | `quality-comparison-report.json` `verdict "PASS_REPORT_READY_NO_QUALITY_OR_TOKEN_CLAIM"`, 96 rows `baseline=adaptive=2`,`delta 0`,`FAIL_INCONCLUSIVE_REVIEW`; `quality-scores-adaptive.jsonl` `reviewer_id "QUALITY-03-fixture-comparison-reviewer"`,`inter_rater_status "single_reviewer_fixture_report_requires_later_real_eval"`, layer note «No real … output evidence was evaluated»; `parent-review-QUALITY-03.json` `verdict "ACCEPTED_FOR_PHASE_COMMIT"` + `semantic_quality_claimed:false` | Единственный shipped-числовой артефакт семантического качества 3 расширений — константный 2/5 FAIL-fixture с fixture-ревьюером; ни один реальный выход модели не оценён, но фаза принята под phase-commit. «PASS» относится к форме отчёта, не к качеству. | Пометить QUALITY-03 как fixture/scaffold, снять «accepted» с числового смысла; заменить fixture-ревьюера реальным per-pair scorer'ом с числами качества, гейтить phase-commit на не-fixture отчёте (>N реально оценённых rows, не FAIL_INCONCLUSIVE). |
| `Quality03ProofContractRunner` как «semantic quality gate» | BUILT_NOT_PROVEN / presence-checker (не измерение) | `Quality03ProofContractRunner.cs:14` self-label «not semantic model … or quality claim authority», `:304` `QualityPassRows:0`, `:306-307` `RunnerReadyEvalInputsCreated/RunnerScorerRun:false`, `:316` `WhatThisDoesNotProve` (отрицает semantic quality), `:280` на 63/63 лишь «Run the separately approved eval-only quality scorer»; вызыватели — `Tools/Quality03ProofContractRunner/Program.cs`+Tests (нет прод/host) | Ярус-1 «гейт качества» проверяет лишь ПРИСУТСТВИЕ 63 proof-ref JSONL-строк, а не измеряет качество; `QualityPassRows` зашит в 0; фактический scorer — отдельный несделанный шаг. «Semantic quality» гейт по построению не может выдать число качества. Отдельно от P21/P23 (per-machine held-out код). | Разделить и реализовать: (а) presence-checker оставить как support, (б) построить реально запускаемый per-pair scorer, который потребляет захваченные выходы модели и выдаёт числовое качество; гейтить readiness на числе, не на наличии proof-ref. |
| Очередь proof-contract застряла 9/63; `quality_pass 0/9`, `approvals 0/9` — eval-входы не одобрены (no-scoring) | STUB / blocked-on-human-eval-input-approval | `quality-03-runner-ready-input-ref-acquire-or-defer-no-scoring/.../acquire-defer-matrix.json` `counts {total 63, pass 9, missing 45, deferred 9, approvals "0/9", runner_ready "0/9", quality_pass "0/9"}`; вся ветка каталогов `quality-03-*-no-scoring` / `*-approval-acquisition` / `*-acquire-or-defer` | Из 63 blocking proof-refs проходят лишь 9 (авто-PASS held_out_split по наличию corpus authority); 45 missing, 9 deferred; ни одного одобренного redacted eval-входа (approvals 0/9) ⇒ scorer никогда не запускался (quality_pass 0/9). Семантический QA стоит на человеческом одобрении eval-входов, которое не получено. Ново (Luba-decision/eval-input блокер именно для качества). | Получить/сгенерировать одобренные redacted eval-входы (approvals ≥9/9), довести proof-refs до 63/63, запустить eval-only scorer; поднять `quality_pass` с 0; без одобренных входов честно держать статус BLOCKED, не «accepted». |
| «Починенный» source-disjoint семантический eval (QSG-24) | STUB / material-coverage тавтология, модель не запускалась | `qsg24-real-dpo-source-disjoint-eval/material-only-held-out-ablation-eval.json` `evaluator_kind "material_only_source_disjoint_row_quality_evaluator_not_model_runtime"`, `staged_checkpoint_used_for_generation:false`, `real_training_or_continue_training_run:false`, rows `without_adaptive_rows_score:0 → with_adaptive_rows_material_coverage_score:1`; `quality-gates.json` gate `semantic_quality_claimed_false:true` (обязан быть true), `passed:true`, `note "No accepted row is treated as common train-ready material … training approval and real checkpoint training remain closed"` | Заявленный «real DPO source-disjoint eval» не запускает модель: «качество» — бинарное покрытие авторским материалом (0→1 тавтологично присутствию adaptive-строки под held-out), а PASS требует, чтобы semantic quality НЕ заявлялось. Числовой прирост 0→1 — не семантическое качество. Отдельно от P21 gap-3 (prompt-to-task keyword-путь) — здесь QSG material-evaluator. | Заменить material-coverage evaluator на прогон ЗАГРУЖЕННОГО checkpoint (реальные выходы модели) на source-disjoint held-out; мерить качество выхода (rubric/inter-rater), а не присутствие материала; снять требование «claim=false» как условие PASS. |
| Нет продюсера реальных выходов 4 rubric-слоёв, питающего scorer | MISSING WIRING / no layer-output capture | `quality-scores-adaptive.jsonl` каждый из 4 `layer_scores[*].evidence_refs = [token-delta-report.json]` и note «No real {prompt_to_task/orchestration_reasoning/skill_selection/final_output_usefulness} output evidence was evaluated»; `ExtensionQualityScoring.Score` реально зовётся только `ExtensionEvalOnlyQualityRunner.cs` (P27) + Tests; нет компонента, снимающего per-pair выходы 4 слоёв в rubric-вход | 4-слойный rubric не имеет входного конвейера: слои ссылаются лишь на token-delta-report, реальных выходов prompt_to_task/orchestration/skill/final-output никто не захватывает и не подаёт в scorer ⇒ качество структурно не может быть оценено. Отдельно от P27 gap-5 (token↔quality subsystem join): тут отсутствует продюсер САМИХ layer-выходов. | Построить capture-harness: на каждый прогон каждого хоста фиксировать реальные выходы 4 слоёв per-pair; подать их как evidence в rubric-scorer; тест, что layer_scores получены из реальных выходов, а не из token-delta. |
| Inter-rater адъюдикация — только одиночный fixture-ревьюер; proof-ref, не quality-pass | MISSING / single-reviewer fixture | `Quality03ProofContractRunner.cs:27` `INTER_RATER_ADJUDICATION_PROOF_REF_ONLY_NOT_QUALITY_PASS`, `:37` категория `inter_rater_adjudication`; `quality-scores-adaptive.jsonl` `inter_rater_status "single_reviewer_fixture_report_requires_later_real_eval"`, `reviewer_id "QUALITY-03-fixture-comparison-reviewer"` (один ревьюер на все 96) | Требуемая независимая вторая оценка (inter-rater) не существует как реальная адъюдикация выходов: shipped-строки — одиночный fixture-ревьюер, а inter-rater-категория раннера по построению «proof-ref only, not quality pass». Согласованности двух независимых оценок реального качества нет. Ново. | Ввести реального независимого второго ревьюера/адъюдикатора на per-pair выходы (не fixture); считать inter-rater agreement (напр. Cohen κ) на непустом наборе; гейтить quality-pass на согласии, а не на присутствии proof-ref. |

## Числовая сводка P28

- `semantic_quality_claimed:true` из реального прогона: 0 файлов (все «true» — это исправляемые
  на `false` ошибочные флаги в provenance-repair; `false` — 2321 файл).
- QUALITY-03 96-строчный отчёт: 96/96 rows `FAIL_INCONCLUSIVE_REVIEW`, score 2/5 константа, 0 rows
  с реально оценённым выходом.
- Proof-contract очередь: 9/63 pass, `quality_pass 0/9`, `approvals 0/9`, `runner_ready 0/9`.
- QSG-24 held-out: `without_adaptive avg 0` → `with_adaptive material-coverage avg 1` (бинарная
  тавтология), модель не запускалась, `semantic_quality_claimed:false`.
- phase-04 source-disjoint: accuracy 1, `sourceDisjointPassed:true`, но `baselineInternalOnly:true`,
  `semanticQualityClaimed:false` (детерминированный baseline-путь).

Вывод: «repaired held-out/source-disjoint eval + numeric results» существует как форма (прогоны,
PASS-флаги, отчёты), но числа — это либо константный FAIL-fixture, либо бинарное покрытие
материалом; доказанного семантического качества модели нет, и весь измеряющий scorer заблокирован
на неполученном человеческом одобрении eval-входов.
