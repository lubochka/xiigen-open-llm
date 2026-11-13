# Layer 0-A — Compiler Pairs (036)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: compiler

---

## B38-0A-036-001 — Complete B38 QA reference: release cadence and versioning extension

This file is a complete QA reference summary for the 025-036 extension batch covering SemanticVersioning (SemVer), CalendarVersioning (CalVer), build cadence, and hotfix/patch release scenarios.

### 12 new scenarios covered (025-036)
| File | Primary topic | Key predicate pattern |
|---|---|---|
| 025 | SemanticVersioning | ["QA"] / ["QA","IMPLEMENT"] / ["DOCUMENT"] |
| 026 | PreReleaseTag / RCCandidate | ["QA"] / ["QA","IMPLEMENT"] / ["QA","DOCUMENT"] |
| 027 | NightlyBuild / WeeklyRelease | ["QA","IMPLEMENT"] / ["DOCUMENT"] |
| 028 | MonthlyRelease / QuarterlyRelease | ["QA","DOCUMENT"] / ["QA","IMPLEMENT","DOCUMENT","SECURITY"] |
| 029 | HotfixTag / PatchRelease | ["QA"] / ["QA","IMPLEMENT"] / ["QA","DOCUMENT"] / ["QA","SECURITY"] |
| 030 | QA reference (versioning) | Reference file |
| 031 | HotfixTag full set / NightlyBuild | ["QA","IMPLEMENT","DOCUMENT","SECURITY"] / ["DOCUMENT"] |
| 032 | SemVerBump / PreReleaseTag / CalVer | ["QA","IMPLEMENT"] / ["QA","SECURITY"] |
| 033 | MonthlyRelease arch / PatchRelease full | ["QA","ARCHITECTURE"] / ["QA","IMPLEMENT","DOCUMENT","SECURITY","ARCHITECTURE"] |
| 034 | QA reference (build cadence) | Reference file |
| 035 | HotfixTag arch / PatchRelease | ["QA","ARCHITECTURE"] / ["QA","IMPLEMENT","SECURITY"] |
| 036 | Complete QA reference (this file) | Reference file |

---

## B38-0A-036-002 — 12 teaching points for the 025-036 extension

1. "Semantic version", "calendar version", "version number" are context words — no predicate fires from them alone.
2. "Build" fires IMPLEMENT (rule 6) in code-authoring context; "nightly build" as a schedule noun does not fire.
3. "Sign-off" fires QA (rule 23) in all cadence contexts — nightly, weekly, monthly, quarterly.
4. "Criteria" fires DOCUMENT (rule 3) for all version qualifiers: patch, minor, major, RC, release.
5. "Audit" + "security" co-fires both QA and SECURITY (rule 19) in all release contexts.
6. "Rollout", "cutover", "go-live" remain gap vocabulary in cadence release sentences — no predicate.
7. "Architecture" in release system design fires ARCHITECTURE predicate.
8. TransitionType = feature_addition if and only if IMPLEMENT is in the predicate set — cadence noun alone never elevates.
9. QA is always last in any multi-predicate pipeline (rule 12 — absolute for all cadence scenarios).
10. IMPLEMENT consolidates: "service" + "build" in same sentence = one IMPLEMENT predicate.
11. "Pre-release tag", "hotfix tag", "patch release" are context nouns — no predicate fires from them directly.
12. Five-predicate maximum: ["QA","IMPLEMENT","DOCUMENT","SECURITY","ARCHITECTURE"] — achievable in complex version review sentences.

---

## B38-0A-036-003 — Anti-corruption reminders for versioning scenarios

v6-01 (QA suppression): QA must fire from review/approve/verify/sign-off even when version nouns (SemVer, CalVer, RC, tag) are co-present. Version nouns are context — never QA suppressors.

v6-02 (Gate B bypass): Not applicable to this batch (all deterministic_generic flows).

v6-03 (predicate collapse): ["QA","IMPLEMENT"] for a versioning scenario with "build" + "service" MUST produce DecisionCount: 2 (4.1 + 7.1). Not DecisionCount: 1.
