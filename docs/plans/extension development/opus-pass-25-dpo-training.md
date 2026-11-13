# Pass P25 — DPO: реальный исполненный training-run (loss/reward-margin), fresh-load, continue-training

Фокус прохода: не КОД тренера (P05) и не СТРОКИ датасета на диске (P24), а **факт реального исполненного обучения с числовыми метриками** — что показывают персистированные runner-report'ы, траектория loss/reward-margin по шагам, и что реально произошло в fresh-load / continue-training прогонах (числовой прирост vs no-op). Все чтения bounded (git ls-files + чтение конкретных JSON), крупные файлы не читались целиком.

## Что реально есть на диске (мета-факты)

- **Реальный градиентный DPO действительно запускался**, но только для юнитов домена `42.ProjectWikiDomain` (`code-trace-builder-unit`, `negative-dpo-generator`), не для трёх extension-машин. На диске: **4263** `.bin`, **25488** `dpo_step_*.json`, **88** section22 runner-report'ов, десятки прогонов под `43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-15..2026-05-19-*` (батчи code-trace 001–007+, negative-dpo-generator 001–003, варианты masks/rejected/invariants).
- Три extension-машины (`prompt-to-task`, `orchestration-reasoning`, `skill-selection`) — это словарь/лексический счётчик (доказано P21/P22/P23); их промоутнутые ассеты в `models/*` НЕ несут DPO-метрик (см. G4). То есть: там, где есть настоящий градиентный DPO — это wiki-домен, отсоединённый от расширений (P05 gap-2); там, где расширения — настоящего DPO-обучения нет вовсе.

## G1 — Единственный настоящий градиентный DPO-run РЕГРЕССИРОВАЛ (loss вырос, margin упал)

Прочитаны первый и последний шаг эталонного 50-step прогона `code-trace-builder-flow-batch-001-code-trace-builder-unit-20260515143246`:

- `dpo_step_000001.json`: `Loss 0.68713164`, `RewardMargin 0.012084006`, `Accuracy 1`
- `dpo_step_000050.json`: `Loss 0.69122493`, `RewardMargin 0.0038708304`, `Accuracy 0.6`

DPO-loss при случайной инициализации = `-log σ(0) = ln 2 ≈ 0.693147`. За 50 шагов loss не упал, а **вырос** (0.68713 → 0.69122, т.е. пошёл ОБРАТНО к 0.6931), reward-margin **сжался в ~3 раза** (0.01208 → 0.00387), train-accuracy упала 1 → 0.6. Агрегатный section22-report того же прогона:
`weighted_accuracy 1`, `reward_margin 0.005325`, `loss 0.690509`, **`heldout_accuracy 0.56`** (чуть выше монетки), **`heldout_loss 0.691449`** (≈ ln 2, т.е. на held-out модель не научилась ничему), `heldout_reward_margin 0.00345`, `dpo_accuracy 0.66`. Отдельно от P05 gap-3 (там причина — хеш-токенайзер/игрушечная 1-слойная модель): здесь **сами исполненные числа** доказывают, что прогон не обучил (loss растёт, margin коллапсирует, held-out ≈ случайность), независимо от объяснения «почему».

## G2 — Ни один из 88 исполненных runner-report'ов не считает delta улучшения (before/after)

Прогон по всем 88 `section22.json`:
- `loss_delta` / `reward_margin_delta` / `weighted_accuracy_delta` = **UNKNOWN во всех 88** (0 не-UNKNOWN значений).
- section22-report явно перечисляет `missing_runner_metrics: ["reward_margin_delta","loss_delta"]`.
- `completion_claim_allowed: false` — **во всех 88** report'ах.

Т.е. системно: ни один исполненный прогон не вычисляет прирост «после обучения минус до обучения». Утверждение «обучение улучшило модель» нигде численно не подтверждено, и сам раннер это фиксирует (delta отсутствуют, completion запрещён). Это отдельно от G1 (абсолютные числа одного прогона) — здесь отсутствие ЛЮБОЙ delta-метрики по всему корпусу прогонов.

## G3 — Held-out accuracy по прогонам разбросана от 0 до 1 с массой на/ниже монетки

Распределение `heldout_accuracy` по 88 report'ам (top): `1.0`×10, **`UNKNOWN`×9**, `0.5`×6, `0.4`×5, `0.64`×4, `0.62`×4, `0.52`×4, `0.42`×4, `0.56`×3, `0.68`×3, а также `0.36`,`0.32`,`0.18`,`0.16`,`0.0`,`0.3`,`0.26`… Большая доля прогонов имеет held-out **на уровне или ниже случайных 0.5**. «1.0»-случаи — это крошечные held-out наборы (`heldOutRowCount ≈ 4`), не независимая генерализация. Стабильного held-out-прироста нет; held-out ведёт себя как шум. Отдельно от G1 (один прогон) — здесь распределение по всему корпусу.

## G4 — Промоутнутые extension-ассеты не несут DPO loss/reward-margin; у prompt-to-task runner-report НЕТ вовсе (коррекция P22 gap-5)

- `models/prompt-to-task/prompt-to-task-v1/` содержит только `manifest.json` + `prompt-to-task-v1-checkpoint.json`; `git ls-files | grep "prompt-to-task.*runner-report"` → **0**. Runner-report для prompt-to-task не существует нигде. Это **корректирует** скобочное утверждение P22 gap-5 («у `models/prompt-to-task` … runner-report есть») — его нет.
- `models/skill-selection/skill-selection-v1/runner-report.json` несёт `accuracy_at_export: 1`, `held_out_accuracy: 1` и **НИ ОДНОГО** ключа loss/reward/margin/delta (grep метрик → пусто).
- `models/orchestration-reasoning/...` runner-report отсутствует (уже зафиксировано P22 gap-5).

Trainable Unit pattern (CLAUDE.md) требует персистить runner-report с accuracy **и** loss-или-reward-margin. Ни один из трёх промоутнутых extension-ассетов не несёт DPO-оптимизационной метрики: skill-selection — только accuracy (это in-sample лексический счёт, P23), prompt-to-task и orchestration-reasoning — вообще без отчёта. «accuracy 1» ≠ доказанное DPO-обучение. Отдельно от P22 gap-5 (там только про отсутствие отчёта у orchestration; здесь — отсутствие DPO-метрик у ВСЕХ трёх + коррекция про prompt-to-task).

## G5 — Все исполненные continue-training пробы extension-машин = no-op со SKIP replay (before==after); единственный не-no-op — игрушечный q6 0→1

Прочитаны все `continue-probe` артефакты:
- `PTT-AD-04-continue-probe.json`: `no_op:true`, `metric_before:1 == metric_after:1`, reason «…already contains all package-trained signal ids; continue probe **skipped replay explicitly**».
- `OR-AD-04-continue-probe.json`: `no_op:true`, `metric_before:1 == metric_after:1`, тот же reason.
- `AD-07…/continue-probe-report.json`: `no_op_continuation:true`, `adaptive_checkpoint_sha256_before == …_after` (байт-в-байт), reason «No new correction DPO rows … **avoided replay**».
- `AD-09A…/continue-probe-report.json`: no-op, «No new ResultGap missing-pattern DPO rows…».
- `SS-13-continue-probe.json`: «deterministic **no-op** continuation … no new eligible pair ids».
- Единственный не-no-op: `…/q6/user-local-adaptive-test-profile/adaptive-continue-probe-report.json`: `metric_before:0 → metric_after:1`, `no_op:false` — но это тест-профиль с одним сигналом (бинарный флип 0→1), не числовой прирост на held-out.

Каждая персистированная continue-training-проба короткозамыкается в no_op и **намеренно пропускает replay** (в свежем профиле нет новых строк — прямая связь с P24 gap-4: online-принятых строк ≈ 0); before==after каждый раз. Значит код-путь continue-training-UPDATE ни разу не исполнялся на реальных строках; «continue-training» доказан только как loadability + пропуск. Отдельно от P05 gap-6 (там формулировка «нет числового прироста»): здесь сильнее — механизм **никогда не запускается** (replay skipped by design), а не «прирост не доказан».

## G6 — Fresh-load доказывает лишь loadability 2 паттернов и сам декларирует «no continue-training / no improvement»

`artifacts/authority-self-reflection/OR-04B-CONTINUE-TRAINING-FRESH-LOAD-01.json`:
`claim_scope`: «continued checkpoint fresh-load probe only; proves fresh-loaded machine can plan for one base pattern and the real delta pattern; **no continue-training, no host runtime, no semantic quality claim**»; `learned_pattern_count:4`, прогоняются ровно `base_pattern_id` + `delta_pattern_id` (2 паттерна), `checkpoint_unchanged_by_probe:true`, `semantic_quality_claimed:false`.

«Fresh-load improvement» выполнен только как loadability: свежезагруженный чекпойнт умеет выдать план для двух именованных паттернов, и сам артефакт отрицает continue-training/host-runtime/качество. Нет before/after сравнения свежезагруженной модели против пустой на held-out — «улучшение от загрузки» не измерено. Отдельно от прозы P21 (там prompt-to-task «байтовая переустановка») — здесь OrchestrationReasoning, self-declared load-only, охват 2 паттерна.

## Дедуп-заметка
НЕ повторяю: P05 gap-3 (хеш-токенайзер/toy-модель — причина), P05 gap-6 (общее «нет числового прироста adaptive»), P21 (alias-authority/ablation-noop/held-out-circular/continue=append код prompt-to-task), P22 gap-2/gap-5/gap-6 (orchestration trainer/нет runner-report/continue=append), P23 (лексический argmax/source-disjoint/нет ablation), P24 (авторские seed-строки/900=12×75/online≈0). Новое здесь — ИСПОЛНЕННЫЕ числовые результаты обучения (регрессия loss/margin, отсутствие delta во всех 88, held-out ≤ монетки), отсутствие DPO-метрик в промоутнутых ассетах (+коррекция prompt-to-task), и фактический no-op/skip-replay характер continue/fresh-load прогонов.
