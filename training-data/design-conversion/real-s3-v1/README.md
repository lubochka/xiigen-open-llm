# DesignConversion real-s3-v1 intake scaffold

## Контур честности

claim_scope: scaffold для будущего приема real-evidence bundle по DesignConversion S3. Это не product readiness, не training run, не held-out result, не upload.

evidence_scope: здесь лежат только JSON Schema placeholders для будущих source manifest, DPO rows, held-out measurements и checkpoint manifest. Реальных source assets, DPO строк, trained weights или held-out результатов в этом scaffold нет.

count_claimed: 4 schema-файла и этот README.

independent_evidence_count: 0 real evidence records.

subagent_id_or_NONE: NONE.

downgrade_needed: yes. The evidence supports only: ingestible schema scaffold exists.

real_trained_weights=not_proven

real_held_out_measurement=not_proven

real_dpo_or_visual_evidence=not_proven

promotion_upload=blocked

placeholder_not_real=true

## Что сюда можно добавлять

Этот каталог предназначен только для маленького real-evidence intake bundle в `training-data/design-conversion/real-s3-v1/`.

Заполнять bundle нужно так:

1. `source-manifest.schema.json` описывает будущий `source-manifest.json`: реальный source design id или redacted source ref, source kind, design tree hash/ref, screenshots, provenance, license/authority и privacy guard.
2. `dpo-rows.schema.json` описывает одну будущую строку `dpo-rows.jsonl`: unit/domain/source/result metadata, chosen/rejected safe refs, teacher labels, teaching point, curriculum tier и validation outcome.
3. `heldout-measurements.schema.json` описывает будущий `heldout-measurements.json`: real held-out metrics, anti-leakage proof, ablation proof, runtime/cost budgets и measurement gate status.
4. `checkpoint-manifest.schema.json` описывает будущий `checkpoint-manifest.json`: checkpoint ref/hash, learned weights evidence, fresh-load probe, continue-training probe, train/held-out row counts и upload status.

## Что сейчас отсутствует

- Нет реального source bundle с лицензией/authority и provenance.
- Нет реальных DPO/visual rows.
- Нет real held-out measurement.
- Нет real trained weights.
- Нет fresh-load evidence для real weights.
- Нет continue-training evidence для real weights.
- Нет promotion/upload.

## Правила заполнения

- Не добавлять secrets, raw user ids, raw prompts или абсолютные пути за пределами этого bundle.
- Использовать bundle-local refs, redacted refs и SHA-256 hashes.
- Source split должен быть source-disjoint: один source identity не должен попадать одновременно в train и held-out.
- Любой пример или заглушка должен иметь `placeholder_not_real=true` и не должен проходить production measurement gate как real evidence.
- `promotion_upload=blocked` остается верным, пока real checkpoint, real held-out measurement и все gate evidence не доказаны отдельно.
