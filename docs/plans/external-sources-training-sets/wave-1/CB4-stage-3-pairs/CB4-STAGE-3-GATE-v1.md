# CB4 Stage 3 Gate Document
# Batch: CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Wave 1 — Stage 3 completion record
# Pair prefix: CB4-3.4-
# Production date: 2026-05-05

---

## Gate B — Stage 3 completion criteria

**Criterion 1:** 120 pair instances produced (60 base pairs × 2 phrasings).
**Result:** PASS. Sequence CB4-3.4-001 through CB4-3.4-060, each with Phrasing B.

**Criterion 2:** All nine nodes covered.
**Result:** PASS. Nine node files produced.

**Criterion 3:** All four sources represented in Node 8 (PASS/REPROCESS/CONCERN per source).
**Result:** PASS. Each source has three Node 8 base pairs (PASS, REPROCESS, CONCERN).

**Criterion 4:** Node 9 REPROCESS propagation trained.
**Result:** PASS. Pairs 030 and 032 explicitly train against FM-5 (FORMATTER_PREMATURE_RESOLUTION).

**Criterion 5:** Failure modes covered.
**Result:** PASS. FM-1/FM-4/FM-5 (OWASP-ASVS), FM-CWE-2/FM-CWE-4 (CWE-TOP25),
FM-SSDF-1/FM-SSDF-3 (NIST-SSDF), FM-RFC-3/FM-RFC-4 (IETF-RFCS) trained.

---

## Pair file index

| File | Node | Sequences | Base pairs | Instances |
|------|------|-----------|-----------|-----------|
| node-08-arbiter-set.md | 8 — Arbiter Units | 001–012 | 12 | 24 |
| node-07-arbiter-selector-set.md | 7 — Arbiter Selector | 013–020 | 8 | 16 |
| node-02-domain-translator-set.md | 2 — Domain Translator | 021–028 | 8 | 16 |
| node-09-formatter-set.md | 9 — Formatter | 029–034 | 6 | 12 |
| node-01-domain-splitter-set.md | 1 — Domain Splitter | 035–040 | 6 | 12 |
| node-03-scheme-planner-set.md | 3 — Scheme Planner | 041–045 | 5 | 10 |
| node-04-domain-processor-set.md | 4 — Domain Processor | 046–050 | 5 | 10 |
| node-05-applier-set.md | 5 — Applier | 051–055 | 5 | 10 |
| node-06-result-translator-set.md | 6 — Result Translator | 056–060 | 5 | 10 |
| **TOTAL** | 9 nodes | 001–060 | **60** | **120** |

---

## Source coverage

| Source | PASS pairs | REPROCESS pairs | CONCERN pairs | Notes |
|--------|-----------|----------------|--------------|-------|
| OWASP ASVS 5.0.0 | 001, 014, 029, 046, 056 | 005, 030, 047, 052, 057 | 009, 031, 053, 058 | V10.4.6 PKCE, V7.3.1 session |
| CWE Top 25 2023 | 002, 016, 039 | 006, 032, 048, 054, 059 | 010, 017 | CWE-862 IDOR, CWE-269 privilege |
| NIST SP 800-218 SSDF v1.1 | 003, 019, 038, 051 | 007 | 011, 033, 049, 055, 060 | PW.2 timing, PW.1/PW.8 satisfied |
| IETF RFCs (7519, 6749, 7636, 8446) | 004, 018, 034, 040 | 008, 050 | 012 | HS256 trust boundary, MAY clause |

---

## Stratum distribution

| Stratum | Pairs | % (approx) | Target |
|---------|-------|-----------|--------|
| A — Domain breadth | ~18 base | 30% | 30% |
| B — Team context | ~15 base | 25% | 25% |
| C — Depth level | ~0 base | 0% | 25% |
| D — Failure/edge cases | ~27 base | 45% | 20% |

**Note on Stratum C:** Stage 3 Action 2 (gap closure) prioritises failure mode coverage
(Stratum D). Stratum C depth-level pairs are targeted in Stage 3 Action 3 (coverage
extension). The Stratum D percentage is elevated because all four source gap catalogues
identified REPROCESS/CONCERN cases as the primary training contribution.

---

## Oracle document alignment

| Oracle check | Addressed in | Status |
|-------------|-------------|--------|
| OWASP S1-M01 Node 8 PASS | Pair 001 | COVERED |
| OWASP S2-M01 Node 8 REPROCESS | Pair 005 | COVERED |
| OWASP S2-M04 ASVS-V10-OAuth-Arbiter selected | Pair 013 | COVERED |
| OWASP S3-M01 Node 8 CONCERN | Pair 009 | COVERED |
| OWASP S3-M02 ASVS 7.3.1 confirmed | Pair 053/058 | COVERED |
| CWE S2-M01 Node 8 REPROCESS for CWE-862 | Pair 006 | COVERED |
| CWE S2-M02 CWE-862 named | Pair 006/032/048 | COVERED |
| CWE S3-M02 CWE-269 named for privilege scope | Pair 010/017 | COVERED |
| SSDF S2-M03 PW.2 runtime test ≠ design review | Pair 007/049 | COVERED |
| SSDF S3-M01 CONCERN for post-impl timing | Pair 011/033/055 | COVERED |
| RFC S2-M01 Node 8 REPROCESS for HS256 trust boundary | Pair 008/050 | COVERED |
| RFC S2-M03 Cross-service forgery capability named | Pair 008/050 | COVERED |
| RFC S3-M01 Node 8 CONCERN for MAY clause | Pair 012 | COVERED |

---

## Next action

Stage 4 — Dataset compilation.
- Annotate all 120 instances with: CB4-3.4- sequence ID, license class, attribution,
  anonymisation flag
- Apply stratified split: training set (100 instances) / test set (20 instances)
- Record wave 1 membership in batch registry
- Update EXTERNAL-SOURCES-REGISTRY with CB4 entry

Stage 3 Action 3 (coverage extension, Stratum C depth pairs) may be scheduled after
Stage 4 compilation confirms the current coverage gaps, particularly Stratum C (0% vs.
25% target).
