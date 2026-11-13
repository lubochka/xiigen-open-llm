# QUALITY-02 score-scale contract alignment repair

## Итог

Выполнен static-only repair для QUALITY-02 scale contract mismatch. Canonical scale выбрана как `0..4`, потому что QUALITY-01 `quality-rubric.json` уже является источником рубрики, содержит явные anchors `0`..`4` и layer success поля `score_4_success`. Минимально-инвазивный путь - выровнять QUALITY-02 contract/checklist/dry-run references под существующую QUALITY-01 rubric, а не добавлять новый неописанный top anchor в QUALITY-01.

## Что изменено

- Active QUALITY-02 prep packet `20260618-231757`: future scoring contract теперь использует `score_scale=0..4` и поле `score_0_to_4`; dry-run rubric check ожидает scale `0..4`.
- Legacy QUALITY-02 prep packet `20260618-095502`: static scorer contract, schema examples, dry-run plan, preflight docs and review wording выровнены под `0..4` / `score_0_to_4`.
- State next action обновлен на `QUALITY-02-EVAL-ONLY-RUNNER-DRY-RUN-VALIDATION-02`.

## Границы

Actual eval/scoring/runtime/external AI/training/DPO/checkpoint mutation не выполнялись. Score rows, comparison outputs and quality claims не создавались. Claim locks сохранены false. Git add/commit/push не выполнялись; parent commits after review.

## Проверки

- JSON parse changed JSON artifacts and state: PASS.
- JSONL parse changed score-output-schema.jsonl: PASS, 12 lines.
- git diff --check по разрешенным folders и state: PASS.
- Scope guard: PASS, только разрешенные QUALITY-01/QUALITY-02/repair artifact folders и state.
- Search affected QUALITY-01/QUALITY-02 artifact folders for legacy inconsistent scale terms: PASS, remaining hits = 0.

## Следующий шаг

`QUALITY-02-EVAL-ONLY-RUNNER-DRY-RUN-VALIDATION-02` - rerun dry-run validation only, без actual scoring и без quality claims.
