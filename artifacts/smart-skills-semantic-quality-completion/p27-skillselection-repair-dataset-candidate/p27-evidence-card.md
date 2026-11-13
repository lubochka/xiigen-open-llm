# P27 evidence card - SkillSelection repair dataset candidate

Вердикт: P27 artifacts готовы как dataset materialization evidence. Training authorization остается false.

## Линия доказательств

1. P24 создал 4 local-private runtime holdout rows: `e2e-test-matrix`, `no-product-decisions`, `output-skepticism`, `self-verification`.
2. P25 прогнал side-by-side comparison и получил 0/4 для common checkpoint и 0/4 для candidate checkpoint. Common выбирал соседние broad baseline skills, candidate возвращал пустой выбор из-за отсутствия trained state.
3. P26 разобрал эти 4 failure rows и написал 8 DPO repair draft rows: prompt-context и final-selection row на каждый провал.
4. P27 материализовал эти 8 rows в checked repair dataset candidate с quarantine/source-disjoint/training authorization evidence.

## Конкретные failure-to-row примеры

- `P24-SS-HOLDOUT-E2E-MATRIX-001`: ожидался `e2e-test-matrix`; common выбрал `implementation-integrity`, `plan-review`, `test-integrity`; candidate выбрал nothing. P27 rows учат выбирать coverage-matrix skill по признакам scenario matrix, success, idempotency, delayed behavior, recovery, boundary, security checks.
- `P24-SS-HOLDOUT-NO-PRODUCT-DECISION-001`: ожидался `no-product-decisions`; common выбрал `claim-verification`, `implementation-integrity`, `planning-session-startup`; candidate выбрал nothing. P27 rows учат отличать product/architecture choice от implementation.
- `P24-SS-HOLDOUT-OUTPUT-SKEPTICISM-001`: ожидался `output-skepticism`; common выбрал `plan-review`, `verification-before-completion`; candidate выбрал nothing. P27 rows учат выбирать claim-skepticism gate до reporting.
- `P24-SS-HOLDOUT-SELF-VERIFICATION-001`: ожидался `self-verification`; common выбрал `implementation-integrity`, `test-integrity`; candidate выбрал nothing. P27 rows учат post-change self-check boundary.

## Что P27 не доказывает

P27 не доказывает semantic quality, SkillSelection readiness, runtime promotion, common model update или checkpoint health. Он доказывает только то, что P26 repair draft rows стали проверяемым local-private dataset candidate и что они остаются quarantined until later authorization.

## Следующий безопасный шаг

Parent review должен принять или вернуть P27 artifacts. Если принято, parent делает commit/push boundary. Только отдельный будущий bounded slice может начинать candidate training и обязан пройти source-disjoint split, fresh-load, continue-training, export/import и ablation gates.
