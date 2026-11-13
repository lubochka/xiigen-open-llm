# Center-Phase Quota Batch 001 - CB4 Intake Ledger
# Date: 2026-05-05

---

## Purpose

Record the license-cleared CB4 external-source pair corpus as usable material
for Wave 1 / center-phase corpus expansion.

This intake covers:

```text
primary_sub_phase_id: 3.4-security-and-authorization-model
primary_phase_id: phase-3-technical-design
batch_id: CB4
pair_prefix: CB4-3.4-
```

---

## Source Authority

The user confirmed the license was checked for the CB4 material.

Manifest evidence:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\wave-1\WAVE-1-STAGE-1-ROUND-MANIFEST-2026-05-05.md
```

Source classes recorded in the manifest:

| Source | License class | Corpus use |
|---|---|---|
| OWASP ASVS 5.0.0 | CC BY-SA 4.0 | allowed with attribution |
| NIST SP 800-218 SSDF v1.1 | Public domain US | allowed |
| CWE Top 25 2023 | MITRE public-domain style use noted by manifest | allowed |
| IETF RFCs | IETF Trust provisions | allowed |

The batch is accepted as Tier 2 external-source material for sub-phase 3.4.

---

## Artifact Location

```text
C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\wave-1\CB4-stage-3-pairs\
```

All 53 expected markdown files named by the user and manifest are present.

---

## Current Corpus Counts

| Count type | Value | Evidence |
|---|---:|---|
| total pair instances | 892 | manifest and user confirmation |
| total base pairs | 446 | manifest and user confirmation |
| original Action 2 instances | 120 | `CB4-STAGE-3-GATE-v1.md` |
| original Action 2 base pairs | 60 | `CB4-STAGE-3-GATE-v1.md` |
| Action 3 additional instances | 784 | manifest |
| Action 3 additional base pairs | 386 | manifest |
| original compiled training split | 100 training / 20 test instances | `CB4-DATASET-COMPILATION-v1.md` |
| additional Action 3 status | training material candidate | manifest |

Important distinction:

These are pair instances and base pairs, not 446 separate source documents.
They count toward logical-unit training material for sub-phase 3.4, but the
v4 document quota ledger must still keep source-document counts separate from
pair-instance counts.

---

## File Families

| Family | Files | Sequence coverage | Purpose |
|---|---:|---|---|
| Node 08 arbiter sets | 18 | 001-060 and 061-446 slices | arbiter PASS/REPROCESS/CONCERN training |
| Pipeline chain sets | 11 | 073-080 and 151-428 slices | full 9-node trace depth coverage |
| Cross-source sets | 10 | 097-106 and 189-438 slices | multi-territory combinations |
| Original node sets | 9 | 001-060 slices | all nine node roles |
| Node extension files | 5 | 081-170 slices | nodes 01-07/09 extension coverage |

---

## Node Coverage

The CB4 set covers all nine dynamic-flow node roles:

| Node | Role | Evidence |
|---|---|---|
| 01 | domain splitter | original set plus ext1 combined file |
| 02 | domain translator | original set plus ext1 combined file |
| 03 | scheme planner | original set plus ext1 combined file and pipeline chains |
| 04 | domain processor | original set plus ext1 |
| 05 | applier | original set plus ext1 |
| 06 | result translator | original set plus ext1 |
| 07 | arbiter selector | original set plus ext1 |
| 08 | arbiter units | original set plus ext1 through ext17 |
| 09 | formatter | original set plus ext1 |

---

## Stratum Status

Manifest stratum summary after Action 3:

```text
Stratum A: approximately 24 percent
Stratum B: approximately 23 percent
Stratum C: approximately 37 percent
Stratum D: approximately 16 percent
```

Interpretation:

- Stratum C is no longer the 0 percent gap recorded in the original Action 2
  gate.
- Action 3 materially improves depth-level reasoning coverage.
- Stratum D remains present but no longer dominates the corpus the way it did
  in the original 120-instance set.

---

## Promotion Status

| Surface | Status |
|---|---|
| License/source authority | accepted from user confirmation and manifest |
| File inventory | passed, 53 expected files present |
| Node coverage | passed, all nine nodes represented |
| Pair-instance availability | passed, 892 instances available |
| Sub-phase mapping | accepted for 3.4 security and authorization model |
| v4 document quota | not complete |
| Stage 5 orchestrator learning test | not started |

---

## Next Gate

The next gate for this material is not source-authority review. That is now
closed for CB4.

The next gate is:

```text
Stage 5 - security-verification-3.4 orchestrator learning test
```

Required next work:

1. register a `security-verification-3.4` logical flow;
2. connect the nine-node security pipeline;
3. use CB4 pairs as the training/conformance material;
4. run S1 PASS, S2 REPROCESS, and S3 CONCERN oracle scenarios;
5. verify mandatory checks from `OWASP-ASVS-HUMAN-RESULT-STANDARDS-v1.md`.

Until Stage 5 passes, CB4 is accepted corpus material but not a proven
orchestrator-learning success.
