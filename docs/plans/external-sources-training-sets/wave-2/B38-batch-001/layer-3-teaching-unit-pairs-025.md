# Layer 3 — Teaching Unit Pairs (025)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B38-TU-025-001 — Teaching summary: 025-036 versioning and cadence batch

Scenarios 025-036 cover SemanticVersioning (SemVer), CalendarVersioning (CalVer), build cadence (nightly, weekly, monthly, quarterly), hotfix tagging, and patch releases.

Key curriculum additions:
025 — SemVer: version noun context; build fires IMPLEMENT; approve fires QA
026 — RC/PreReleaseTag: tag is context; sign-off fires QA; approval = QA alone
027 — Nightly/weekly: schedule is gap; build fires IMPLEMENT; validate fires QA
028 — Monthly/quarterly: criteria fires DOCUMENT; four-predicate quarterly gate
029 — Hotfix/patch: hotfix/patch are context nouns; QA alone for approval
030-036 — Extension reference files

---

## B38-TU-025-002 — 12 teaching points for 025-036

1. "SemVer", "CalVer", "version number", "version tag" are context nouns — no predicate.
2. "Build" fires IMPLEMENT (rule 6) in any versioning build sentence.
3. "Sign-off" fires QA (rule 23) for all cadence scenarios.
4. "Criteria" fires DOCUMENT (rule 3) for all version qualifiers.
5. "Audit" co-fires QA+SECURITY (rule 19) in all cadence contexts.
6. "Rollout" is gap everywhere.
7. "Architecture" in release system design fires ARCHITECTURE.
8. TransitionType = feature_addition iff IMPLEMENT in PredicateSet.
9. QA is always last (absolute).
10. IMPLEMENT consolidates: "service" + "build" in same sentence = one IMPLEMENT.
11. "Pre-release tag", "hotfix tag", "patch release" are context nouns.
12. Five-predicate maximum: ["QA","IMPLEMENT","DOCUMENT","SECURITY","ARCHITECTURE"].

---

## B38-TU-025-003 — Quick scenario lookup: 025-036

| Scenario | Example input (abbreviated) | PredicateSet |
|---|---|---|
| SemVer build + approve | "Build versioning service and approve." | ["QA","IMPLEMENT"] |
| CalVer plan only | "Plan the calendar versioning scheme." | [] |
| RC sign-off | "Approve the RC pre-release tag." | ["QA"] |
| Nightly build validate | "Build nightly service and validate." | ["QA","IMPLEMENT"] |
| Weekly process doc | "Document the weekly release criteria." | ["DOCUMENT"] |
| Monthly gate | "Document monthly criteria and sign off." | ["QA","DOCUMENT"] |
| Quarterly four-predicate | "Review service, audit security, document criteria, approve." | ["QA","IMPLEMENT","DOCUMENT","SECURITY"] |
| Hotfix approve | "Approve the hotfix." | ["QA"] |
| Patch build + sign-off | "Build patch service and sign off." | ["QA","IMPLEMENT"] |
