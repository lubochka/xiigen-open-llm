# QUALITY-03 code-fix work order 09K

Work order id: QUALITY-03-PROOF-CONTRACT-RUNNER-SECOND-RATER-PACKET-CLI-FIX-09K

Цель: добавить явный CLI-контракт `--second-rater-packet <dir>`, чтобы proof contract runner мог честно привязать authorized second-rater packet к категории `inter_rater_adjudication` без копирования или переименования файлов.

Текущая причина: runner принимает только `--evidence-pack`, `--proof-contract-pack`, `--output-dir`, `--run-id`, `--work-order-id`. Если запустить его сейчас, он прочитает старый `row-specific-independent-second-rater-adjudication.jsonl` из proof-contract pack, а не authorized packet `quality-scoring-20260619-135510`.

Allowed write scope:
- `43.SmartSkillsDomain.Tools/Quality03ProofContractRunner/Program.cs`
- `43.SmartSkillsDomain/Quality03ProofContractRunnerContracts.cs`
- `43.SmartSkillsDomain/Quality03ProofContractRunner.cs`
- `Tests/ExtensionQuality03ProofContractRunnerTests.cs`

Required change:
- Add optional `--second-rater-packet <dir>` to CLI usage and parsing.
- Add optional `SecondRaterPacketDirectory` to runner options.
- When supplied, load `per-row-authorized-second-rater-adjudication.jsonl` from that directory for `inter_rater_adjudication` instead of `proof_contract_pack/row-specific-independent-second-rater-adjudication.jsonl`.
- Validate 9 rows, unique row ids, row-id match to proof input rows, and existing pass/adjudication semantics.
- Keep concrete refs pointing at the authorized packet path.
- Preserve old behavior when `--second-rater-packet` is omitted.
- Report the binding mode in runner output/refusal output.

Required verification:
- `dotnet test Tests/LlmFromScratch.Tests.csproj --filter ExtensionQuality03ProofContractRunnerTests`
- Runner command after fix:

```powershell
dotnet run --project 43.SmartSkillsDomain.Tools/Quality03ProofContractRunner/Quality03ProofContractRunner.csproj -- --evidence-pack "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\quality-scoring\quality-scoring-20260619-120659" --proof-contract-pack "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\quality-scoring\quality-scoring-20260619-122536" --second-rater-packet "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\quality-scoring\quality-scoring-20260619-135510" --output-dir "C:\Projects\llm_mvp_core\artifacts\extension-runtime-token-quality-learnability\quality-scoring\quality-scoring-20260619-141113" --run-id "quality-scoring-20260619-141113" --work-order-id "QUALITY-03-PROOF-CONTRACT-RUNNER-RERUN-AUTHORIZED-SECOND-RATER-PACKET-09J"
```

Expected honest post-fix runner result with these unchanged packs: inter-rater category can become 9/9, proof PASS likely 9/63, runner-ready rows 0/9, quality-pass rows 0/9. This is not a quality, semantic, learnability, product, or readiness claim.

Forbidden: git stage/commit/push/branch inside the bounded implementation work order, external AI/model calls, training/DPO/checkpoint/model/locator mutation, and copy/rename hacks.
