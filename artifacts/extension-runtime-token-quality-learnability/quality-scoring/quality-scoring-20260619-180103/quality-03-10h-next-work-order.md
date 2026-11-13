# QUALITY-03-NON-INTER-RATER-EXPLICIT-SOURCE-LICENSE-CONSENT-AND-ROW-APPROVAL-PACKET-10H

## Цель
Получить явное source/license/consent approval для точных QUALITY-03 rows или снова честно зафиксировать no-pass. 10G проверил локальные пакеты и не нашёл доказанного approval: accepted_license_packets=0, license_proven_pair_rows=0, common_training_allowed_pair_rows=0, eligible split rows=0.

## Что нужно принести
1. Для каждого source_ref/doc_id/source_hash: source_license_ref, license_hash, license_class, consent/provenance, privacy_class, common_training_allowed или явный non-common route.
2. Для QUALITY-03-REVIEW-ROW-001: отсутствующий source_scenario_id/source_ref/doc_id/source_hash binding.
3. Approved train source corpus manifest только из permissive/explicitly approved universal material.
4. Approved held-out split с нулевым overlap по source_ref, doc_id, source_hash, path, title, prompt hash, correction text, nearest-neighbour source.
5. Approved raw-private replay-only corpus ref с path/hash и vendor trace exclusion; no training target use.
6. Ровно 9 approved row assignments.
7. Только после этого: 54 proof artifacts, NonInterRaterProofProducer, Quality03ProofContractRunner.

## Запрещено
No git stage/commit/push/branch/checkout. No external AI/web/model calls without fresh Luba exception. No training/DPO/checkpoint/model/locator mutation. No raw external AI output as training target. No self-approval. No false PASS.

## Верификация
JSON/JSONL parse; approval invariants; overlap check only if split exists; vendor exclusion check; producer/runner only after valid proof bundle; git diff --check; whitespace check new artifacts.
