# SKILLSELECTION-CANDIDATE-P22-ZERO-DIAGNOSE-AND-ONE-FIX-06

## Вывод
Вердикт: **NEEDS_FIX**. Решение: **B**.

Candidate в P22 тесте означает checkpoint `artifacts/smart-skills-semantic-quality-completion/p17e-skillselection-lifecycle-import-export/production-export/skill-selection-checkpoint.json`. Это P17E production-export из P17D component lifecycle path, не common/promoted semantic checkpoint.

Почему `candidate_expected_match_count=0`: обе accepted P22 строки получили `candidateSelectedSkillIds=[]`, `candidateRequiresSecondPass=true`, `candidateConfidence=0`, evidence `[NO_TRAINED_STATE_AVAILABLE, SEMANTIC_SKILL_SELECTION_PIPELINE_BOUNDARY]`. В candidate checkpoint `candidatePositionWeights={}`; semantic-unit decision для этих P22 запросов не сработал. Поэтому fallback честно возвращает no trained state, а не ошибочный skill.

## Evidence
- Before broader rerun: tests 15/15 pass, `accepted_rows=2`, `common_expected_match_count=2`, `candidate_expected_match_count=0`.
- Anti-leakage controls true: True; no candidate-position final authority: True; no training/checkpoint mutation/promotion/semantic claim.
- Candidate checkpoint has 19 trained pair ids; common checkpoint has 900 trained pair ids.
- Candidate skillSelection profiles: `claim_verification, code_review, planning, planning_dpo_authoring, trainable_architecture_decision`.
- Candidate has no `test_integrity` profile, so P22 row `P22-SS-RUNTIME-E2E-MATRIX-001` cannot pass this candidate as-is.
- P17E report states component lifecycle evidence only and no semantic quality/readiness/common/final path mutation.

## Почему не A
A не применен: быстрый fix потребовал бы мутировать существующий P17E production-export evidence artifact или подогнать expected/output. Это было бы скрытым ослаблением доказательства. Нужен отдельный test-owned candidate/local learnability fixture.

## Boundary correction
Common semantic P22 gate и candidate/local learnability gate нужно разделять явно. Это boundary correction, не quality downgrade: common checkpoint получил expected matches, а candidate/local learnability для этих P22 rows остается недоказанным и не превращается в PASS.

## Next smallest bounded action
SKILLSELECTION-CANDIDATE-P22-LOCAL-LEARNABILITY-FIXTURE-07: create a test-owned candidate/local learnability fixture or checkpoint path for the two P22 accepted rows, generated through the SkillSelection training/runtime path, with source-disjoint controls preserved; rerun focused P22 and broader gate requiring candidate_expected_match_count > 0 and anti-leakage controls true, without production promotion or locator update.

## Non-claims
Не заявлено: product readiness, semantic quality readiness, token savings, training success, checkpoint promotion, locator update, candidate fix.

## Honesty block
```text
claim_scope: Diagnosis of P22 candidate_expected_match_count=0 blocker; no product/code/checkpoint fix claimed.
evidence_scope: Read P22 test evidence, prior broader rerun report/eval/runtime rows, P17D/P17E lifecycle artifacts, and candidate/common checkpoint summaries; wrote new evidence packet only.
count_claimed: 1 diagnosis packet; 0 dotnet tests in this order; 0 production/test code edits; before metrics: tests 15/15 pass, P22 accepted_rows=2, common_expected_match_count=2, candidate_expected_match_count=0.
independent_evidence_count: 6
subagent_id_or_NONE: Nash
downgrade_needed: yes: decision B keeps broader gate PARTIAL/NEEDS_FIX for candidate/local learnability; no product readiness or candidate fix is claimed.
```
