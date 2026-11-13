# QUALITY-03 non-inter-rater proof producer

Это локальный deterministic producer proof-data. Он не является semantic scorer, learning proof или product readiness gate.

- total proof refs: 63
- PASS до работы: 9
- non-inter-rater PASS после producer: 0/54
- ожидаемый proof PASS после producer плюс carried inter-rater: 9/63
- runner-ready rows: 0
- quality-pass rows: 0

## По категориям
- held_out_split: PASS 0/9; RISK/DEFER 9/9
- source_disjoint: PASS 0/9; RISK/DEFER 9/9
- nearest_neighbour_similarity_risk: PASS 0/9; RISK/DEFER 9/9
- ablation: PASS 0/9; RISK/DEFER 9/9
- no_leakage: PASS 0/9; RISK/DEFER 9/9
- no_memorization: PASS 0/9; RISK/DEFER 9/9

## Что это доказывает
This proves only that local deterministic proof producers created consumable QUALITY-03 proof-contract inputs from local files.

## Чего это не доказывает
This does not prove semantic quality, token savings, learnability, product readiness, training readiness, target promotion, locator updates, or final readiness.

## CLI examples
```powershell
dotnet run --project 43.SmartSkillsDomain.Tools/Quality03NonInterRaterProofProducers/Quality03NonInterRaterProofProducers.csproj -- --evidence-pack <evidence-pack> --source-proof-contract-pack <proof-pack> --output-dir <new-quality-scoring-dir>
dotnet run --project 43.SmartSkillsDomain.Tools/Quality03ProofContractRunner/Quality03ProofContractRunner.csproj -- --evidence-pack <evidence-pack> --proof-contract-pack <new-quality-scoring-dir> --second-rater-packet <authorized-second-rater-packet-dir-or-file> --output-dir <runner-output-dir>
```

Следующий bounded work order: QUALITY-03-NON-INTER-RATER-SOURCE-AUTHORITY-AND-PROOF-BUNDLE-10D: close approved source/corpus/held-out/raw-private authority first, then assemble a real proof bundle only if local approval/provenance exists.
