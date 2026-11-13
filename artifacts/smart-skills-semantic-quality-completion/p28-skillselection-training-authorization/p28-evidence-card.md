# P28 evidence card

## Что сделано

P28 проверил авторизацию candidate-only SkillSelection repair training и подготовил следующий безопасный non-training work order.

Точный вердикт: training сейчас не авторизован.

## Evidence

- Current P28 work order требует сначала authorization verification и прямо говорит считать training запрещенным, если active sources не разрешают его явно.
- State держит `training_authorized=false`, `training_allowed=false`, `eval_allowed=false`.
- P27 training authorization ledger держит `trainingAllowed=false`, `candidateTrainingAllowed=false`, `commonTrainingAllowed=false`, `notUsedForTrainingYet=true`.
- P27 parent review принял P27 только как checked repair dataset candidate.
- P27 rows все еще candidate/evidence: они local-private, not used for training yet, common reuse требует отдельного approval.
- Active plan требует source-disjoint, ablation, checkpoint rewrite, fresh-load, continue-training и no-claim gates.

## Артефакты

- `p28-training-authorization-verdict.json`
- `p28-training-authorization-verdict.md`
- `p28-next-safe-work-order.json`
- `p28-next-safe-work-order.md`
- `p28-review-cycles.jsonl`
- `p28-self-check.json`
- `p28-evidence-card.md`

## Следующее безопасное действие

P29 должен быть non-training gate-design slice: source-disjoint split design, training authorization matrix, batch card, leakage gates и future P30 gate list. P29 не должен запускать training или менять model files.

## Validation

- JSON/JSONL parse: passed, 4 JSON files, 1 JSONL file, 7 JSONL rows.
- UTF-8/no mojibake scan: passed after repairing the P28 state quote encoding.
- No-overclaim scan: passed.
- `git diff --check`: passed.
- `git diff --exit-code -- models/skill-selection/skill-selection-v1`: passed.

## No-overclaim

P28 не обучал модель, не запускал eval, не менял checkpoints, не делал promotion, не заменял common model и не заявлял semantic quality или SkillSelection completion.
