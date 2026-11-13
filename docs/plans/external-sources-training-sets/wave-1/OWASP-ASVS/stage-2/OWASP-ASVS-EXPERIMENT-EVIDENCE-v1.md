# OWASP-ASVS — Experiment Evidence
# Stage 2, Document 1 of 1
# Wave 1 — Sub-phase 3.4 (Security and Authorization Model)
# Batch: CB4 — Pair ID prefix: CB4-3.4-
# Execution date: 2026-05-05
# Pipeline state: phase 64 — InMemoryLogicalOrchestrator, T34 flow only

---

## Section 1 — Baseline

Pre-run test suite count:
Command: `dotnet test Tests/LlmFromScratch.Tests.csproj --no-build 2>&1 | tail -5`
Result: `Passed! - Failed: 0, Passed: 1160, Skipped: 0, Total: 1160`
Baseline: **1160 passing, 0 failing.** This is the floor.

Post-run test suite count: 1160 passing, 0 failing. Baseline holds.

---

## Section 2 — Scenario traces

### Pipeline state at execution time

The LogicalOrchestratorAssistant.CreateDefault() method registers exactly one
flow: T34LogicalFlowAdapter with ScenarioId = "t34-phase-transition".

The security-verification scenarios require a scenario ID such as
"security-verification-3.4" or equivalent. No such scenario is registered.
The orchestrator's InMemoryLogicalOrchestrator.RunAsync() returns:
"No logical flow registered for scenario '{scenarioId}'."

No nine-node security pipeline exists. Nodes 1 through 9 produce no output
for any of the three OWASP-ASVS scenarios.

---

### S1 Input (verbatim)

"Our backend authentication service has the following verified controls in
production:
- Password minimum length: 15 characters, enforced at write time...
[Full input as specified in OWASP-ASVS-EXECUTOR-CONTRACT-v1.md Section 3 S1]"

**S1 Node 1 output:** [NO OUTPUT — no registered flow for scenario 'security-verification-3.4']
**S1 Node 2 output:** [NO OUTPUT]
**S1 Node 3 output:** [NO OUTPUT]
**S1 Node 4 output:** [NO OUTPUT]
**S1 Node 5 output:** [NO OUTPUT]
**S1 Node 6 output:** [NO OUTPUT]
**S1 Node 7 output:** [NO OUTPUT]
**S1 Node 8 output:** [NO OUTPUT]
**S1 Node 9 output:** [NO OUTPUT]

**Orchestrator response (verbatim):**
`OperationResult.Fail("No logical flow registered for scenario 'security-verification-3.4'.")`

---

### S2 Input (verbatim)

"Our single-page application uses OAuth 2.0 authorization code flow. We
implemented PKCE to protect against authorization code interception..."
[Full input as specified in OWASP-ASVS-EXECUTOR-CONTRACT-v1.md Section 3 S2]

**S2 Nodes 1–9:** [NO OUTPUT — same pipeline failure as S1]

**Orchestrator response (verbatim):**
`OperationResult.Fail("No logical flow registered for scenario 'security-verification-3.4'.")`

---

### S3 Input (verbatim)

"Our session management implementation uses a 20-minute idle timeout with a
documented risk analysis (RA-AUTH-003)..."
[Full input as specified in OWASP-ASVS-EXECUTOR-CONTRACT-v1.md Section 3 S3]

**S3 Nodes 1–9:** [NO OUTPUT — same pipeline failure as S1]

**Orchestrator response (verbatim):**
`OperationResult.Fail("No logical flow registered for scenario 'security-verification-3.4'.")`

---

## Section 3 — Comparison against oracle

All mandatory checks fail. Classification: structural gap for all nodes in all
scenarios.

| Check | Expected (oracle) | Actual | Result | Gap type |
|-------|------------------|--------|--------|----------|
| S1-M01 | Node 8 verdict: PASS | No output | FAIL | Structural |
| S1-M02 | ASVS IDs named in Node 8 | No output | FAIL | Structural |
| S1-M03 | Node 9 verdict: PASS | No output | FAIL | Structural |
| S1-M04 | No REPROCESS in Node 9 | No output | FAIL | Structural |
| S1-M05 | G-security-verification in Node 2 | No output | FAIL | Structural |
| S2-M01 | Node 8 verdict: REPROCESS | No output | FAIL | Structural |
| S2-M02 | ASVS 10.4.6 named | No output | FAIL | Structural |
| S2-M03 | Plain method violation described | No output | FAIL | Structural |
| S2-M04 | ASVS-V10-OAuth-Arbiter selected | No output | FAIL | Structural |
| S2-M05 | No "compliant" in Node 9 | No output | FAIL | Structural |
| S2-M06 | Remediation action in Node 9 | No output | FAIL | Structural |
| S3-M01 | Node 8 verdict: CONCERN | No output | FAIL | Structural |
| S3-M02 | ASVS 7.3.1 confirmed in Node 8 | No output | FAIL | Structural |
| S3-M03 | Internal standard vs ASVS distinction | No output | FAIL | Structural |
| S3-M04 | No REPROCESS in Node 9 | No output | FAIL | Structural |
| S3-M05 | Resolution path in Node 9 | No output | FAIL | Structural |

---

## Section 4 — Verdict per scenario

**S1:** CONCERN — structural gap (all nine nodes absent)
**S2:** CONCERN — structural gap (all nine nodes absent)
**S3:** CONCERN — structural gap (all nine nodes absent)

Note: the oracle uses PASS/PASS_WITH_GAPS/CONCERN for scenario verdicts. All
three scenarios receive CONCERN because the pipeline produces no output — the
absence of any output is a structural failure, not a pass or a qualified pass.

---

## Section 5 — Gap catalogue

All gaps are structural gaps (content entirely absent, not wrong). Every gap
is attributed to the same root cause: no security-verification flow registered
in the pipeline.

**Gap-ASVS-01:** Node 1 — Domain Splitter
- All scenarios: no domain territories produced
- Gap type: structural
- Root cause: no registered flow for security-verification-3.4

**Gap-ASVS-02:** Node 2 — Domain Translator
- All scenarios: no domain labels produced
- Gap type: structural
- Root cause: Node 1 produces no input for Node 2

**Gap-ASVS-03:** Node 3 — Scheme Planner
- All scenarios: no scheme plan produced
- Gap type: structural
- Root cause: no flow registered

**Gap-ASVS-04:** Node 4 — Domain Processor
- All scenarios: no domain processing output
- Gap type: structural
- Root cause: no flow registered

**Gap-ASVS-05:** Node 5 — Applier
- All scenarios: no application result
- Gap type: structural
- Root cause: no flow registered

**Gap-ASVS-06:** Node 6 — Result Translator
- All scenarios: no translated result
- Gap type: structural
- Root cause: no flow registered

**Gap-ASVS-07:** Node 7 — Arbiter Selector
- All scenarios: no arbiter selected
- Gap type: structural
- Root cause: no flow registered; arbiters ASVS-V6-Auth-Arbiter,
  ASVS-V7-Session-Arbiter, ASVS-V8-AuthZ-Arbiter, ASVS-V9-Token-Arbiter,
  ASVS-V10-OAuth-Arbiter are not registered

**Gap-ASVS-08:** Node 8 — Arbiter Units
- All scenarios: no arbiter verdicts produced
- Gap type: structural
- Root cause: no arbiters registered, no flow registered
- Priority note: the core coverage-illusion, claim-boundary, and PKCE-violation
  training pairs all live in this node — this is the highest-priority gap in
  the batch

**Gap-ASVS-09:** Node 9 — Formatter
- All scenarios: no formatted result
- Gap type: structural
- Root cause: no flow registered

---

## Section 6 — Next action

**Action required: Stage 3 Action 2 — Gap closure pass.**

There are no existing security-verification pairs to correct (Action 1 does not
apply — the correction pass applies only when existing pairs have CONCERN verdicts).
All gaps are structural — content must be written from scratch.

Stage 3 Action 2 produces pairs for all nine nodes following the Examination
Method Final rules:
- Node 8 pass case (S1 verdict PASS) first
- Then Node 8 REPROCESS case (S2 PKCE violation)
- Then Node 8 CONCERN case (S3 session timeout)
- Then Node 7 arbiter selection pairs
- Then Node 2 domain translation pairs (one per territory)
- Then remaining nodes in coverage order

**Action 3 (coverage extension) follows Action 2 after all S1-S3 gaps are
closed.** Action 3 produces pairs for nodes that had no trained output — in this
case all nine nodes, but it starts from the same oracle definitions used in
Action 2 and extends coverage to additional scenarios not covered by S1-S3.

**Batch CB4 target:** 120 pairs total for sub-phase 3.4 across all four sources.
OWASP-ASVS contributes the first 20 pairs minimum. Remaining 100 pairs from
NIST-SSDF, CWE-TOP25, and IETF-RFCS experiments.
