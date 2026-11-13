# IETF-RFCS — Experiment Evidence
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
'security-verification-3.4'. All nine nodes produce no output.

---

## Section 3 — Comparison against oracle

All mandatory checks fail (structural gap).

IETF-RFCS specific critical mandatory checks failed:
- S2-M01: Node 8 verdict must be REPROCESS for HS256 trust boundary — NOT PRODUCED
- S2-M03: Cross-service forgery capability must be named — NOT PRODUCED
- S3-M01: Node 8 verdict must be CONCERN (not REPROCESS) for MAY clause — NOT PRODUCED

---

## Section 4 — Verdict per scenario

S1: CONCERN (structural)
S2: CONCERN (structural)
S3: CONCERN (structural)

---

## Section 5 — Gap catalogue

All nine nodes: structural gap.

**Gap-RFC-01 (primary):** Node 8 — RFC-JWT-Arbiter not registered.
The primary training contribution: RFC MUST/MAY normative keyword calibration
and trust boundary violation detection for HS256 shared key deployments.

**Gap-RFC-02 (primary):** Node 8 — RFC-OAuth2-Arbiter not registered.
The OAuth 2.0 protocol conformance arbiter (RFC 6749, RFC 7636) does not exist.

**Gap-RFC-03:** Node 7 — RFC arbiter selection not registered.

**Gaps-RFC-04 through -10:** Nodes 1–6, 9 — same structural absence.

---

## Section 6 — Next action

Stage 3 Action 2 — gap closure pass. No existing RFC pairs to correct.

Primary pairs to produce:
1. Node 8 RFC-JWT-Arbiter: S1 PASS with RFC section references
2. Node 8 RFC-JWT-Arbiter: S2 REPROCESS — RFC 7519 §8 trust boundary violation
3. Node 8 RFC-JWT-Arbiter: S3 CONCERN — RFC 7519 §4.1.4 MAY clause (not MUST)
4. Node 7: RFC-JWT-Arbiter selection when JWT vocabulary present
5. Cross-source references: IETF-RFCS pairs must cite corresponding ASVS IDs
   (RFC 7636 §4.2 → ASVS 10.4.6, RFC 7519 §7.2 → ASVS 9.1.1)
