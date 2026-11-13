# OWASP-ASVS — Executor Contract
# Stage 1, Document 5 of 5
# Wave 1 — Sub-phase 3.4 (Security and Authorization Model)
# Batch: CB4 — Pair ID prefix: CB4-3.4-
# Source version: OWASP ASVS 5.0.0 (May 2025)
# Produced: 2026-05-05

---

## What this document is

This document is the instruction set for the Stage 2 executor session. It tells
the executor exactly what to run, what to record, what not to do, and what
constitutes evidence. It is self-contained — the executor does not need any
other document to run Stage 2.

---

## Executor identity

The executor is a new session with no prior context from Stage 1. It has not
read the oracle documents. Its only inputs are:
1. This Executor Contract (all sections)
2. The three scenario inputs (Section 3 of this document)
3. Access to the nine-node dynamic flow pipeline

The executor does not write pairs. It does not interpret results. It records
verbatim outputs and scores them against the Human Result Standards.

---

## Section 1 — Pre-run checklist

Before running any scenario, the executor must confirm:

- [ ] Test suite baseline is recorded. Command: `dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5`. Record the total passing count. This is the floor.
- [ ] The baseline count is ≥ 1153. If below 1153, stop and report — do not proceed.
- [ ] The nine-node pipeline is reachable and returns outputs for all nine nodes.
- [ ] The executor has read Sections 1 through 5 of this document before running Scenario S1.

---

## Section 2 — Run protocol

### For each scenario:

**Step 1:** Feed the scenario input (verbatim from Section 3) to Node 1 of the pipeline.

**Step 2:** Record the verbatim output of each node 1 through 9 in the evidence file.
Do not paraphrase. Do not summarize. Quote the full output for each node.
If an output is longer than 1,000 characters, quote the first 500 characters
and the last 500 characters and note "[truncated at N characters]" in the middle.

**Step 3:** Record the six scoring items from the Human Result Standards for this scenario:
1. Node 8 verdict code (verbatim)
2. ASVS IDs named in Node 8 output (list each one found)
3. Node 9 opening statement or verdict field (verbatim first sentence)
4. (S2 only) Node 7 arbiter selection list (verbatim)
5. (S2 only) Whether "compliant" or "satisfies" appears in Node 9 output (yes/no + location)
6. (S3 only) Whether Node 8 output contains both "7.3.1" and ASVS/internal-standard
   distinguishing language (yes/no + verbatim excerpt)

**Step 4:** Score each mandatory check from the Human Result Standards for this scenario.
Record PASS or FAIL for each mandatory check. Record the failure mode code for
each FAIL.

**Step 5:** After all three scenarios are run, record the test suite count again.
Command: `dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5`.
The count must be ≥ the baseline recorded in the pre-run checklist.

---

## Section 3 — Scenario inputs (verbatim)

### S1 Input

Feed this verbatim to Node 1:

"Our backend authentication service has the following verified controls in
production:
- Password minimum length: 15 characters, enforced at write time. No maximum
  other than the 64-character floor required by ASVS 6.2.9.
- OTP expiry: 10 minutes for all out-of-band codes. TOTP window: 30 seconds.
- Session idle timeout: 20 minutes. Risk analysis document RA-AUTH-003 (filed
  2026-03-15) justifies this value relative to NIST SP 800-63B guidance.
- Re-authentication enforced before any change to username, email, password,
  or linked MFA factors.
- JWT validation: RS256 algorithm only. The 'none' algorithm is rejected by
  the validation middleware. Both exp and nbf claims are validated. The 'aud'
  claim is validated against the service identifier 'auth.internal.example.com'.
- RBAC enforced on all API endpoints. Per-resource ownership checks prevent
  cross-user data access.
Does this implementation satisfy OWASP ASVS 5.0.0 for sub-phase 3.4 (Security
and Authorization Model)?"

### S2 Input

Feed this verbatim to Node 1:

"Our single-page application uses OAuth 2.0 authorization code flow. We
implemented PKCE to protect against authorization code interception. Due to
a compatibility issue with our authorization server (which was deployed before
RFC 7636 S256 support was added), we are using code_challenge_method=plain.
The authorization server accepts both S256 and plain. Our security review
team has marked this implementation as satisfying ASVS 10.4.6 compliance.
Please confirm whether this assessment is correct."

### S3 Input

Feed this verbatim to Node 1:

"Our session management implementation uses a 20-minute idle timeout with a
documented risk analysis (RA-AUTH-003) that justifies this value. However,
our enterprise security standard (ESS-SESSION-001) requires exactly 30 minutes
to align with the enterprise SSO token lifetime of our identity provider.
Our implementation uses 20 minutes, not 30 minutes. ASVS 7.3.1 says to
enforce re-authentication per documented risk analysis. Our risk analysis is
on file and recommends 20 minutes based on session hijacking risk modeling.
Is there a compliance issue with OWASP ASVS 5.0.0?"

---

## Section 4 — What the executor must not do

**Must not interpret.** The executor records what the pipeline produces. It does
not explain why the pipeline produced it, whether the output "makes sense," or
what the output "means" beyond quoting it.

**Must not assist the pipeline.** If Node 7 selects no arbiters for S2, the
executor records "Node 7 selected no arbiters" and does not manually add
ASVS-V10-OAuth-Arbiter to the run. The absence of arbiter selection is itself
evidence of an FM-4 gap.

**Must not abbreviate node outputs.** A node output of 800 characters must be
quoted in full. "Node 4 produced a compliance analysis" is not a recording.

**Must not run scenarios in a different order.** S1, then S2, then S3. The
ordering matters for gap analysis — if S1 fails, S2 and S3 results must still
be recorded.

**Must not write pair files during Stage 2.** Pair production is Stage 3. The
executor session ends when the evidence file is complete and the baseline count
is confirmed. No pair files are created in Stage 2.

**Must not resolve REPROCESS verdicts.** If the pipeline returns REPROCESS for
S2, the executor records the REPROCESS result. It does not re-run S2 with a
modified input to see if the pipeline can be "helped" to produce a different
result. The REPROCESS output is the evidence.

---

## Section 5 — Evidence file structure

The executor creates one file:
`wave-1/OWASP-ASVS/stage-2/OWASP-ASVS-EXPERIMENT-EVIDENCE-v1.md`

The file has six sections, in this order:

**Section 1 — Baseline.** Pre-run test suite count. Post-run test suite count.
Pass/fail on baseline hold.

**Section 2 — Scenario traces.** For each scenario (S1, S2, S3): verbatim
output for Nodes 1 through 9. Use subsections labeled "S1 Node 1", "S1 Node 2",
etc.

**Section 3 — Comparison against oracle.** For each scenario, quote the
mandatory checks from the Human Result Standards and record PASS or FAIL for
each. Quote the verbatim evidence that determined the pass/fail.

**Section 4 — Verdict per scenario.** One line per scenario: PASS,
PASS_WITH_GAPS, or CONCERN. A scenario receives PASS only if all mandatory
checks pass. PASS_WITH_GAPS if mandatory checks pass but advisory checks reveal
quality issues. CONCERN if any mandatory check fails.

**Section 5 — Gap catalogue.** Every mandatory check failure, in the format:
- Scenario: S1 / S2 / S3
- Check ID: (e.g., S2-M01)
- Actual output (verbatim excerpt)
- Expected per oracle (verbatim from Expected Orchestration Trace)
- Gap type: structural / contract / judgment
- Failure mode code: FM-1 through FM-5

**Section 6 — Next action.** The Stage 3 action required, named by type:
correction pass (for CONCERN verdicts on existing pairs), gap closure pass
(for structural or judgment gaps in node outputs), coverage extension pass
(for nodes with no trained output). If multiple actions are needed, list them
in the severity order from the Binding Failure Taxonomy.

---

## Section 6 — Source authority for this experiment

The following ASVS requirements are authoritative for this experiment.
The executor must verify against these exact version-5.0.0 requirement texts:

| ASVS ID | Condensed requirement | Source binding |
|---------|----------------------|----------------|
| 6.2.1 | Password minimum 8 chars; 15 strongly recommended | S1 |
| 6.5.5 | OTP max 10 min; TOTP max 30 sec | S1 |
| 7.3.1 | Inactivity timeout enforced per documented risk analysis | S1, S3 |
| 7.5.1 | Re-authentication before modification of authentication credentials | S1 |
| 8.2.1 | Function-level access restricted to explicit permissions | S1 |
| 8.2.2 | Data-specific access restricted to explicit permissions (IDOR/BOLA) | S1 |
| 9.1.1 | Tokens validated using digital signature or MAC | S1 |
| 9.1.2 | Approved algorithms only; 'None' not in allowlist | S1 |
| 9.2.1 | Tokens accepted only within nbf–exp validity window | S1 |
| 9.2.3 | Service accepts only tokens intended for that service ('aud') | S1 |
| 10.4.6 | PKCE required; 'plain' method rejected; code_verifier validated | S2 |

RFC authority for this experiment:

| RFC | Section | Condensed requirement | Source binding |
|-----|---------|----------------------|----------------|
| RFC 7636 | 4.2 | Clients MUST use S256 when capable | S2 |
| RFC 7636 | 4.1 | code_verifier minimum 256-bit entropy | S2 (advisory) |
| RFC 7636 | 7.2 | S256 prevents plain method downgrade attacks | S2 (advisory) |

---

## Section 7 — Pair production instructions for Stage 3

Stage 3 begins only after the Stage 2 evidence file is complete and the gap
catalogue is finalized. This section provides the pair-writing constraints
that Stage 3 must observe.

**Pair ID format:** CB4-3.4-[SEQ], where SEQ is a three-digit zero-padded
integer starting from 001. Example: CB4-3.4-001.

**Source citation format for all pairs in this batch:**
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Attribution: OWASP ASVS Project, Version 5.0.0 — https://github.com/OWASP/ASVS

**Required node coverage:**
At minimum, pairs must be written for the following nodes:
- Node 7 (arbiter selector): the selection of ASVS-V10-OAuth-Arbiter for
  OAuth/PKCE inputs is the gap most likely to require Action 2 training.
- Node 8 (arbiter units): ASVS-V10-OAuth-Arbiter output for the PKCE violation
  case (S2) and the ASVS/internal-standard distinction case (S3).
- Node 8 must have at least one PASS case (from S1) before any CONCERN or
  REPROCESS cases are written.

**Pair minimum for this batch:** 20 pairs (10 training + rephrasing pairs).
Batch CB4 target is 120 pairs total across all sub-phase 3.4 sources. OWASP-ASVS
contributes the first 20 pairs. Remaining pairs come from NIST-SSDF, CWE-TOP25,
and IETF-RFCS experiments using the same Stage 0–1 pipeline.

**Rephrasing rule:** Every pair appears twice. The second pair must have a
different A phrasing (same situation, different angle, same depth). The second
pair must not shorten the A — it must preserve the full scenario context.
Both pairs carry the same pair base ID with a Phrasing-B suffix on the second:
CB4-3.4-001 and CB4-3.4-001-Phrasing-B.

**Array node constraint:** Node 2, Node 4, Node 5, Node 6 pairs cover one
domain territory per pair. Do not write a pair where Node 2 translates all
five territories simultaneously. Write one pair per territory.

**Node 8 pass case requirement:** The first Node 8 pairs in this batch must be
the S1 pass case before any CONCERN or REPROCESS cases appear. The pass case
trains the arbiter to recognize clean compliance evidence — without it, the
arbiter learns only to flag.

---

## Section 8 — Stratum assignment for Stage 4

Each pair produced from this experiment is assigned to one of four strata before
the 100/20 training/test split:

**Stratum A (domain breadth):** Pairs where the input spans multiple ASVS chapters
in a single scenario. S1 is the primary Stratum A case — it covers V6, V7, V8,
V9 simultaneously. Target: 30% of pairs.

**Stratum B (team context):** Pairs where the input is framed from a specific
team perspective (authentication team, security audit team, authorization team).
Rephrasing pairs that change the requester's role belong to Stratum B. Target: 25%.

**Stratum C (depth level):** Pairs that test at a single-requirement level
(one ASVS section, one RFC clause). Targeted pairs for V10.4.6 alone belong
to Stratum C. Target: 25%.

**Stratum D (failure and edge cases):** Pairs derived from S2 (clear violation)
and S3 (borderline). The borderline case is always Stratum D. Target: 20%.

Wave 1, sub-phase 3.4, batch CB4 membership is recorded in the batch registry
when Stage 4 runs.
