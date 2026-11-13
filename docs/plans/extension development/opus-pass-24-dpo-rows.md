# Pass P24 — DPO: реальные принятые строки/датасеты на диске (accepted rows/data)

Фокус прохода: НЕ код тренера (P05) и НЕ чекпойнт-заявки (P05/P21/P22/P23), а сами
DPO-строки/датасеты НА ДИСКЕ для трёх расширений (prompt-to-task,
orchestration-reasoning, skill-selection) — их РЕАЛЬНЫЕ количества, происхождение
(принятые из рантайма/фидбэка vs авторские/синтетические) и honesty-флаги.

## Что реально лежит на диске (bounded git ls-files + wc -l + чтение карточек)

Датасеты трёх extension-машин (`training-data/smart-skills/semantic-quality/*` и
второй корень `training-data/skill-selection/*`), измеренные `wc -l`:

| Датасет (путь) | строк | назначение | honesty-флаги в карточке/сплите |
|---|---|---|---|
| seed-v1/smart-skills-dpo-seed.jsonl | 8 | seed (PTT 3, OR 3, SS 2) | `PASS_SEED_DATASET_CARD_STAGED_ONLY`, `training_allowed:false`, `seed_smoke_only:true`, `model_quality_claims_blocked:true`, `source_disjoint_...:false` |
| prompt-to-task-expanded-v1/...-dpo.jsonl | 12 | PTT train8/heldout4 | rows: `training_allowed=0/12`, `semantic_quality_claimed=0/12`, `chosen_rejected_pair_present=12/12`, `materialized_only=3/12` |
| orchestration-reasoning-expanded-v1/...-dpo.jsonl | 12 | OR train8/heldout4 | split: `training_allowed:false`, `verification_scope: material readiness only; not model training`; rows: `chosen_rejected_pair_present=0/12` |
| skill-selection-training-v1/...q5-04r-semantic-training.jsonl | 38 | SS (smart-skills корень) | (сирота — см. GAP-5) |
| skill-selection-heldout-v1/skill-selection-heldout.jsonl | 12 | SS held-out | — |
| skill-selection-inventory-v1/skill-inventory.jsonl | 175 | инвентарь скиллов (НЕ DPO-пары) | — |
| cross-project-transfer-fixtures-v1/cross-project-fixtures.jsonl | 8 | fixtures | — |
| **training-data/skill-selection/skill-selection-v1/corpus-snapshot.jsonl** | **88** | SS **авторитетный** корпус (его берёт runner) | corpus-pointer: `row_count:88`, `training_pair_count:75`, `raw_private_corpus_committed:false`, `adaptive_private_rows_included:false` |

Adaptive/online DPO-сигналы (`.xiigen/*/adaptive-dpo-signals.jsonl`), `wc -l`:

| файл | строк |
|---|---|
| artifacts/adaptive-learning/PTT-AD-02-probe/.xiigen/prompt-to-task/adaptive-dpo-signals.jsonl | 1 |
| artifacts/adaptive-learning/OR-AD-02-probe/.xiigen/orchestration-reasoning/adaptive-dpo-signals.jsonl | 1 |
| artifacts/smart-skills-quality-uplift/q6/.../.xiigen/orchestration-reasoning/adaptive-dpo-signals.jsonl | 1 |
| training-data/.../q6-04-.../restore-import/.xiigen/orchestration-reasoning/adaptive-dpo-signals.jsonl | 1 |
| **skill-selection adaptive-dpo-signals** | **0 файлов** |

Всего adaptive-dpo-signals.jsonl во всём репо = **4 файла, каждый по 1 строке**;
skill-selection — ноль. Содержимое единственной строки — синтетический probe:
`signalId:"ptt-ad-02-probe-signal-001"`, `chosenStructuredTaskRef` /
`rejectedStructuredTaskRef` = ref-плейсхолдеры, `containsRawPromptText:false`,
`semanticQualityClaimed:false`.

## Сверка «заявленных» количеств с диском

- skill-selection runner-report: `eligible_pair_count:75` → `adapter_routed_pair_count:900`
  → `trained_pair_count:900`, при 12 `training_adapters` (75 × 12 = 900). На диске
  корпус = 88 строк / 75 eligible. Значит «900 обученных пар» = те же 75 пар,
  размноженные по 12 адаптерам, а не 900 различных принятых строк.
- prompt-to-task promoted checkpoint (P21): `dpoPairCount:8` + `heldOutRowCount:4`
  = 12 строк expanded-датасета (train 8 / held-out 4) — сходится с диском.
- orchestration-reasoning-v1: 12 строк на диске, но **runner-report отсутствует**
  (подтверждение P22 gap-5 уже на уровне данных: split сам говорит «material
  readiness only; not model training»).

## Происхождение строк

Строки — АВТОРСКИЕ «materialized» примеры, не принятые из рантайма/фидбэка.
Карточка seed прямо: source_refs → `artifacts/dpo-quality/smart-skills-dpo-materialized.jsonl`,
роль «reviewed materialized seed rows copied exactly to seed JSONL». Схема строки
несёт `chosen_ref`/`chosen_summary`/`rejected_ref`/`rejected_summary`,
`approved_behavior`/`rejected_near_miss_behavior` — авторская near-miss-таксономия
по ссылке, а не захваченные chosen/rejected выходы модели.

## Дедуп

Не повторяю: P05 gap-1 (чекпойнт=словарь; упомянул `dpoPairCount:8`/`trained_pair_count:900`
как содержимое ЧЕКПОЙНТА, без подсчёта датасетов на диске и без флага 12× inflation),
P21 gap-3/gap-4 (prompt-to-task circular held-out / «12 patterns > 8 pairs» на уровне
чекпойнта/паттернов), P22 gap-2/gap-5 (orchestration trainer игнорит rejected в КОДЕ /
нет runner-report), P23 gap-2 («held_out_accuracy:1 circular, eligible 75→900» на уровне
eval). Здесь — новые дыры именно на уровне ДАННЫХ НА ДИСКЕ: количества, inflation,
schema-pair-absence, near-zero online-accepted, divergent roots.

## Новые дыры — см. таблицу в мастере P24 (5 шт.)
