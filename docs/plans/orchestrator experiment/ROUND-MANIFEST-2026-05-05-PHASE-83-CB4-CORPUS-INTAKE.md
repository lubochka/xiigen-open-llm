# Round Manifest - Phase 83 CB4 Corpus Intake
# Date: 2026-05-05

---

## Purpose

Accept the license-cleared CB4 external-source pair material into the corpus
tracking path for Wave 1 / sub-phase 3.4, without incorrectly declaring corpus
v4 complete.

---

## Files Produced

- `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\CENTER-PHASE-QUOTA-BATCH-001-CB4-INTAKE-LEDGER.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\WAVE-1-QUOTA-LEDGER-v2.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\CB4-STAGE-5-SECURITY-FLOW-REGISTRATION-PLAN-v1.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\CORPUS-COMPLETION-LEDGER-v3.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\ROUND-MANIFEST-2026-05-05-PHASE-83-CB4-CORPUS-INTAKE.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator experiment\round-phase-83-cb4-corpus-intake-2026-05-05.zip`

---

## Evidence Read

- `external-sources-training-sets\wave-1\WAVE-1-STAGE-1-ROUND-MANIFEST-2026-05-05.md`
- `external-sources-training-sets\wave-1\CB4-DATASET-COMPILATION-v1.md`
- `external-sources-training-sets\wave-1\CB4-stage-3-pairs\CB4-STAGE-3-GATE-v1.md`
- CB4 Stage 3 pair folder inventory

---

## Structural Verification

Expected CB4 files checked:

```text
53/53 present
```

Key present files include:

- `node-08-arbiter-set-ext17.md`
- `node-pipeline-chain-set-ext10.md`
- `node-cross-source-set-ext9.md`
- `node-01-domain-splitter-set.md`
- `node-09-formatter-set.md`

---

## Corpus Intake Result

| Metric | Value |
|---|---:|
| accepted pair instances | 892 |
| accepted base pairs | 446 |
| original training instances | 100 |
| original test instances | 20 |
| additional training-material candidate instances | 784 |
| node roles covered | 9 |
| source families | 4 |
| target sub-phase | 3.4 |

---

## Verification

Focused WDA:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "WDA" --no-build
```

Result:

```text
Passed: 7, Failed: 0, Total: 7
```

No C# files changed in this phase.

---

## Finding

The CB4 material is accepted for corpus use under the user-confirmed license
review and manifest license classes.

The corpus is still not complete. CB4 is accepted material for one sub-phase,
not the full v4 corpus.

---

## Next Action

Start Phase 84:

```text
CB4 Stage 5 security-verification-3.4 flow registration
```

The Stage 5 implementation should start small: register a deterministic
`security-verification-3.4` logical flow, run the three OWASP ASVS oracle
scenarios, and prove PASS / REPROCESS / CONCERN verdict preservation before
attempting learned unit replacement.
