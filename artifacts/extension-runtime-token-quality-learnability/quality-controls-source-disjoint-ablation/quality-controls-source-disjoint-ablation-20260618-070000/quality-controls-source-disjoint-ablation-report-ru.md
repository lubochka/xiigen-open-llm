# QUALITY-CONTROLS-SOURCE-DISJOINT-ABLATION — отчёт

Run: `quality-controls-source-disjoint-ablation-20260618-070000`.

## Что сделано

Создан repair artifact pack для текущих 12 сценариев и 54 pair rows из QUALITY-03 rerun. Все строки учтены: у каждой есть `scenario_id`, `pair_id`, `prompt_hash`, ссылки на текущие QUALITY/TOKEN артефакты и явный статус контроля.

Я не заполнял `source_ref`, `doc_id`, license или train-vs-held-out классификацию из названий сценариев или догадок. В локальных артефактах есть только `source_basis` и `universalization_rule`; это полезное объяснение происхождения сценария, но не строгая ссылка на источник, документ, лицензию и partition.

## Что доказано локально

- 12/12 сценариев из `scenario-pack-token-quality-v1.jsonl` прочитаны.
- 54/54 pair rows из текущего QUALITY-03 rerun учтены.
- Для каждой строки есть `scenario_id`, `pair_id` и `prompt_hash`.
- Pair integrity в предыдущем rerun был 54/54 PASS.
- Текущие артефакты фиксируют отсутствие external AI task solving/training/model mutation в этой цепочке.
- Universal-vs-stack-specific split теперь явный: common quality controls относятся к универсальной семантике task/orchestration/skill, а Claude/Cursor/Codex остаются только labels host matrix, не source authority.

## Чего не хватает

- `source_ref`: отсутствует для 12/12 сценариев и 54/54 pair rows.
- `doc_id`: отсутствует для 12/12 сценариев и 54/54 pair rows.
- `license_class`: отсутствует для 12/12 сценариев и 54/54 pair rows.
- `source_disjoint_group` и `train_or_heldout`: отсутствуют для 12/12 сценариев и 54/54 pair rows.
- No-leakage: нет source partition или nearest-neighbour/source-overlap отчёта для текущих 54 строк.
- No-memorization: нет held-out source-disjoint eval и exact replay контроля для текущих 54 строк.
- Ablation: нет текущего target-disabled/asset-shuffled/common-only rerun для этих 54 pair rows.

## Влияние на QUALITY-03 rerun

QUALITY-03 нельзя честно rerun-ить как strict PASS прямо сейчас. Repair pack переводит проблему из расплывчатого inconclusive в конкретный статус по каждой строке: локальная идентичность пары и prompt hash есть, но строгая source-disjoint/no-leakage/no-memorization/ablation часть заблокирована отсутствующей metadata/control evidence.

Поэтому claims остаются закрыты: token savings, quality improvement, semantic quality, learnability, live-host savings и final readiness не открываются. Дополнительно token-savings gate уже фиксирует, что adaptive total tokens выше baseline на +48 heuristic tokens, так что savings claim закрыт и по качественному gate, и по observed token delta.

## Следующее действие

Следующая фаза должна заполнить missing source metadata из approved references: source ledger с `source_ref/doc_id/license_class`, train-vs-held-out partition, source overlap/nearest-neighbour report, no-memorization report и ablation report. Только после этого имеет смысл снова запускать QUALITY-03 для strict controls.
