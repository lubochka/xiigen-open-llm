# QUALITY-03-NON-INTER-RATER-PROOF-DATA-AND-RUNNERS-10C

## Цель

Получить реальные, runner-потребляемые PASS/APPROVED proof artifacts для шести non-inter-rater категорий по 9 QUALITY-03 строкам или честно зафиксировать no-pass gap с точным недостающим источником.

## Стартовая правда

- Blocking proof refs: 9/63 PASS.
- Non-inter-rater refs: 0/54 PASS, 54/54 DEFER/RISK.
- Runner-ready rows: 0/9.
- Quality-pass rows: 0/9.
- 10B artifact root: artifacts/extension-runtime-token-quality-learnability/quality-scoring/quality-scoring-20260619-162921

## Исполняемые шаги

1. Создать или найти approved-held-out-source-corpus-manifest.json, который принимает Quality03ProofContractRunner: approved=true, non-deferred status, approved train source ref, approved held-out split ref, approved raw-private corpus ref, per-row approved assignment for all 9 rows.
2. Доказать source/license/provenance partition: классифицировать каждый источник, исключить raw external AI output как training target, оставить external AI output только diagnostic evidence.
3. Построить source-disjoint proof: source_disjoint_check_executed=true, source_disjoint_pass=true, overlap counts zero, approved vendor trace exclusion.
4. Построить approved NN index over approved train/adaptive/private corpus: index built/contract present, top_k non-empty, max_similarity <= 0.82.
5. Выполнить ablation runner controls: exact prompt lookup, filename/BM25 route, candidate order, nearest-only retrieval; runner executed; shortcut_only=false.
6. Выполнить no-leakage/no-memorization replay probes over approved raw/private refs. Учесть текущие риски: rows 002/005/008 fragment=28, rows 003/006 fragment=29, rows 004/007 fragment=30, row 009 fragment=29, row 001 lacks scenario prompt text.
7. Только после этого создать proof bundle root с quality-03-non-inter-rater-proof-bundle.json и 54 per-row proof artifact JSON files, затем запустить producer и proof runner, записав before/after counts.

## Если данных недостаточно

Не спрашивать Luba в рамках worker. Создать no-pass quarantine/gap packet, где указать точный отсутствующий файл или approval/source/provenance/ref, из-за которого категория и строка не могут стать PASS.

## Запреты

No git stage/commit/push/branch/checkout. No external AI/web/model calls. No training/DPO/checkpoint/model/locator mutation. No false PASS. No raw external AI output as training target. Universal orchestration rule extraction only unless category explicitly requires host/runtime proof.
