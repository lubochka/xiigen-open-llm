# Corpus Completion Ledger v3
# Date: 2026-05-05

---

## Answer

The full corpus v4 is still not complete.

New information since v2:

- CB4 external-source material is accepted as license-cleared.
- CB4 contributes 892 pair instances / 446 base pairs for sub-phase 3.4.
- All expected CB4 Stage 3 files are present.
- The next gate for CB4 is Stage 5 runtime orchestration, not source authority.

---

## Current Proven Surfaces

| Surface | Status |
|---|---|
| WDA-00 through WDA-06 deterministic suite | passing |
| WDA-01 real router seed proof | passing |
| 31 seed source rows | ready |
| CB4 pair corpus for 3.4 | accepted |
| CB4 Stage 5 runtime flow | not started |
| full v4 training document quota | not complete |
| full held-out quota | not complete |
| transition held-out quota | not complete |

---

## CB4 Intake

| Item | Value |
|---|---|
| primary sub-phase | 3.4 Security and authorization model |
| sources | OWASP ASVS 5.0.0, NIST SSDF, CWE Top 25, IETF RFCs |
| pair instances | 892 |
| base pairs | 446 |
| original train/test split | 100 training / 20 test instances |
| additional Action 3 material | 784 instances |
| node coverage | all nine dynamic-flow nodes |
| file inventory | 53 expected files present |
| source authority | accepted by user confirmation and manifest license classes |

---

## Why Corpus Is Still Open

CB4 is strong pair material, but v4 completion still requires:

```text
3,100 training documents
620 sub-phase held-out documents
60 transition held-out documents
trusted logical units
active sub-phase flows
regression suspension behavior
topology-intent classifier behavior
phase-context compiler and transition-gate proof
batch manifests and zips for every promoted corpus batch
```

CB4 helps with one sub-phase and one security-verification flow. It does not
complete the whole development-area corpus.

---

## Next Gate

Phase 84:

```text
CB4 Stage 5 - security-verification-3.4 flow registration
```

Required:

1. register `security-verification-3.4`;
2. run three OWASP ASVS oracle scenarios;
3. prove PASS, REPROCESS, and CONCERN verdicts;
4. preserve attribution;
5. keep WDA and full suite passing;
6. produce manifest and zip.
