# CB4 Dataset Compilation
# Stage 4 — Dataset compilation record
# Batch: CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Wave 1
# Compilation date: 2026-05-05

---

## Section 1 — Inputs

60 base pairs × 2 phrasings = 120 pair instances
Sequence IDs: CB4-3.4-001 through CB4-3.4-060 (plus Phrasing B for each)
Source files: wave-1/CB4-stage-3-pairs/ (9 node files)

---

## Section 2 — Annotations applied

### Annotation 1 — Sequence ID

Format: CB4-3.4-[SEQ] where SEQ is zero-padded 3-digit integer 001–060.
Phrasing B instances append " — Phrasing B" to the base pair ID.
Applied: at production time (encoded in pair headers in node files).

### Annotation 2 — License class

| Sequences | Source | License | Attribution required |
|-----------|--------|---------|---------------------|
| 001, 005, 009, 013–015, 021–025, 029–031, 034–037, 041–043, 046–047, 051–053, 056–058 | OWASP ASVS 5.0.0 | CC BY-SA 4.0 | Yes — "Based on OWASP ASVS 5.0.0, CC BY-SA 4.0" |
| 003, 007, 011, 019, 026, 033, 038, 043, 049, 055, 060 | NIST SP 800-218 SSDF v1.1 | Public domain (NIST) | No |
| 002, 006, 010, 016–017, 027, 032, 039, 044–045, 048, 054, 059 | CWE Top 25 2023 | Public domain (MITRE) | No |
| 004, 008, 012, 018, 028, 034, 040, 050 | IETF RFCs 7519/6749/7636/8446 | IETF Trust (pre-5378) | No |
| 020 | Multi-source (OWASP/IETF/NIST) | Most restrictive: CC BY-SA 4.0 | Yes |

### Annotation 3 — Attribution

Attribution block appended to all pairs from OWASP-ASVS source:
> Source: OWASP Application Security Verification Standard 5.0.0 (ASVS 5.0.0),
> licensed under Creative Commons Attribution-ShareAlike 4.0 International (CC BY-SA 4.0).
> https://owasp.org/www-project-application-security-verification-standard/

Attribution block for multi-source pair (020):
> Sources: OWASP ASVS 5.0.0 (CC BY-SA 4.0), IETF RFC 7519 (IETF Trust pre-5378),
> NIST SP 800-218 SSDF v1.1 (public domain). Most restrictive license: CC BY-SA 4.0.

### Annotation 4 — Anonymisation flag

All 120 instances: ANONYMISED = N/A.
Rationale: all pair content derives from published standards and hypothetical system
descriptions modelled on those standards. No real system names, real user IDs, or real
organisational identifiers appear in the content.

---

## Section 3 — Stratified split

### Stratum assignments

| Stratum | Definition | Base pair count | % |
|---------|-----------|----------------|---|
| A — Domain breadth | Pairs demonstrating range across security domains | ~18 | 30% |
| B — Team context | Pairs in realistic team/product context | ~15 | 25% |
| C — Depth level | Pairs requiring multi-step reasoning | 0 | 0% |
| D — Failure/edge cases | Pairs training against failure modes | ~27 | 45% |

**Note on Stratum C gap:** Stage 3 Action 2 (gap closure) prioritised Stratum D coverage
because all four source gap catalogues named failure modes as the primary training
contribution. Stratum C pairs (depth-level reasoning) are planned for Stage 3 Action 3
(coverage extension), to be executed after Stage 5 validation.

### Training/test split

Total: 120 instances.
Training set: 100 instances (83%).
Test set: 20 instances (17%).

**Test set selection criteria:** one Phrasing B instance per source per verdict type
(PASS/REPROCESS/CONCERN). Specifically:
  - OWASP-ASVS: 001-PB, 005-PB, 009-PB, 013-PB, 030-PB (5 instances)
  - CWE-TOP25: 002-PB, 006-PB, 010-PB, 016-PB, 032-PB (5 instances)
  - NIST-SSDF: 003-PB, 007-PB, 011-PB, 019-PB, 049-PB (5 instances)
  - IETF-RFCS: 004-PB, 008-PB, 012-PB, 018-PB, 050-PB (5 instances)
Total test: 20 instances (one per source per key verdict, all Phrasing B).

All 60 base pair originals are in the training set.
All 60 remaining Phrasing B instances are in the training set (40 training + 60 PB = 100).

---

## Section 4 — Wave 1 membership

Batch CB4 is the first batch in Wave 1, Sub-phase 3.4 (Security and Authorization Model).

| Field | Value |
|-------|-------|
| Wave | Wave 1 |
| Sub-phase | 3.4 — Security and Authorization Model |
| Pipeline phase at production | Phase 64 — InMemoryLogicalOrchestrator, T34 flow |
| Baseline at production | 1160 passing tests |
| Pair prefix | CB4-3.4- |
| Total base pairs | 60 |
| Total instances | 120 |
| Sources | OWASP ASVS 5.0.0, NIST SP 800-218 SSDF v1.1, CWE Top 25 2023, IETF RFCs |
| Nodes covered | 1, 2, 3, 4, 5, 6, 7, 8, 9 (all nine) |

---

## Section 5 — Registry update

File updated: external-sources-training-sets/00-INDEX.md
Entry added: CB4 row in Batch Registry table.
License classes added: CC BY-SA 4.0 (OWASP), Public domain NIST, Public domain MITRE,
IETF Trust pre-5378.

---

## Section 6 — Next stage

Stage 5 — Orchestrator learning test.
The security verification flow (scenario ID: security-verification-3.4) must be
registered in InMemoryLogicalOrchestrator. The nine-node pipeline must be implemented
and connected. CB4 pairs serve as the training set for the learning test.

Stage 5 prerequisite: the three oracle scenarios (S1 PASS, S2 REPROCESS, S3 CONCERN
from OWASP-ASVS-EXECUTOR-CONTRACT-v1.md) must produce outputs matching the mandatory
checks in OWASP-ASVS-HUMAN-RESULT-STANDARDS-v1.md.

Stratum C gap remediation (Stage 3 Action 3) is deferred until after Stage 5 validation
confirms which training gaps remain.
