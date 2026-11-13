# QUALITY-03-NON-INTER-RATER-SOURCE-EVIDENCE-SUPPLY-AND-APPROVED-SPLIT-10G

## Цель
Дать 10G точный вход для закрытия разрыва 10F: сейчас локально нет доказанного source/license/consent approval, поэтому proof bundle запрещен.

## Текущий факт 10F
- PASS остается 9/63.
- Non-inter-rater остается 0/54.
- Runner-ready остается 0/9.
- Quality-pass остается 0/9.
- Все 9 row assignments остаются DEFERRED_NOT_APPROVED.
- Proof bundle не создан, producer/runner не запускались.

## Что нужно принести или авторить в 10G
1. Accepted source/license/consent packet для каждого source_ref/doc_id/source_hash текущих 9 строк.
2. approved-train-source-corpus-manifest.json с approved=true только для common-training источников с доказанными license_ref/license_hash/consent.
3. approved-held-out-split-manifest.json с нулевым overlap по source_ref, doc_id, source_hash, path, title, prompt hash, correction text, nearest-neighbour source.
4. approved-raw-private-corpus-ref.json только replay-diagnostic/no-training, с approved paths/hashes и vendor trace exclusions.
5. quality-03-row-assignment-approval.jsonl: ровно 9 строк, все approved_assignment не NONE/DEFER.
6. После этого row-specific NN, ablation, no-leakage, no-memorization controls, proof bundle и 54 proof artifacts.
7. Только после валидного proof bundle запуск NonInterRaterProofProducer и Quality03ProofContractRunner.

## Запрещено
No git stage/commit/push/branch/checkout. No external AI/web/model calls unless Luba gives a fresh explicit exception. No training/DPO/checkpoint/model/locator mutation. No raw external AI output as training target. No false approval or false PASS.
