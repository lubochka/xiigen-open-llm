# NIST-SSDF — Experiment Evidence
# Stage 2, Document 1 of 1
# Wave 1 — Sub-phase 3.4 (Security and Authorization Model)
# Batch: CB4 — Pair ID prefix: CB4-3.4-
# Execution date: 2026-05-05
# Pipeline state: phase 64 — InMemoryLogicalOrchestrator, T34 flow only

---

## Section 1 — Baseline

Pre-run: 1160 passing, 0 failing. Floor: 1160.
Post-run: 1160 passing, 0 failing. Baseline holds.

---

## Section 2 — Scenario traces

Same pipeline failure as OWASP-ASVS: no registered flow for
'security-verification-3.4'. All nine nodes produce no output for S1, S2, S3.

Orchestrator response for all scenarios (verbatim):
`OperationResult.Fail("No logical flow registered for scenario 'security-verification-3.4'.")`

---

## Section 3 — Comparison against oracle

All mandatory checks fail (structural gap — no pipeline output). Full check
table not repeated; structure is identical to OWASP-ASVS evidence Section 3.

NIST-SSDF specific mandatory checks failed:
- S2-M03: Node 8 must state runtime test results (ZAP, ASVS) do not satisfy PW.2 — NOT PRODUCED
- S3-M01: Node 8 verdict must be CONCERN for post-implementation design review — NOT PRODUCED

---

## Section 4 — Verdict per scenario

S1: CONCERN (structural — all nodes absent)
S2: CONCERN (structural — all nodes absent)
S3: CONCERN (structural — all nodes absent)

---

## Section 5 — Gap catalogue

All nine nodes: structural gap — no security-verification flow registered.

**Gap-SSDF-01 (primary):** Node 8 — SSDF-PW-Arbiter not registered.
The core SSDF training signal is Node 8's ability to distinguish SSDF process
evidence (design artifacts) from ASVS runtime test results. This arbiter does
not exist. Priority: highest.

**Gap-SSDF-02 (primary):** Node 7 — SSDF arbiter selection not registered.
Node 7 has no rules for selecting SSDF-PW-Arbiter or SSDF-PO-Arbiter from
process-evidence vocabulary.

**Gaps-SSDF-03 through -09:** Nodes 1–6, 9 — same structural absence as
OWASP-ASVS gaps -01 through -06, -09.

---

## Section 6 — Next action

Stage 3 Action 2 — gap closure pass. No existing SSDF pairs to correct.

Primary pairs to produce:
1. Node 8 SSDF-PW-Arbiter: S1 PASS with PO.1, PW.1, PW.2, PW.8 named
2. Node 8 SSDF-PW-Arbiter: S2 REPROCESS (runtime test ≠ design review)
3. Node 8 SSDF-PW-Arbiter: S3 CONCERN (post-implementation timing gap)
4. Node 7: SSDF-PW-Arbiter selection from process vocabulary
