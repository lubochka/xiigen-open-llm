# Layer 0-B — Router Pairs (060)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-060-001 — Complete routing reference: Round 3 TEST routing invariant

A: Does TEST ever route to 5.1 for any Round 3 topic (AdminPanel through WASM)?
B: No. All Round 3 topics are browser-based UI interaction flows. TEST routes to 5.3 for all 24 Round 3 files. 5.1 requires: pure function/service logic + "unit test" explicit + no UI vocabulary. No Round 3 topic meets this criterion.

---

## C0B-060-002 — Complete routing reference: UI routing invariant across all 60 topics

A: Has UI ever routed to anything other than 1.5 in B39?
B: No. UI → 1.5 is an absolute invariant. All 60 topic files in B39 (001–060) confirm: UI vocabulary → UIInteractionFlow15 (1.5). No exceptions exist.

---

## C0B-060-003 — Complete routing reference: QA routing decision tree

A: State the complete QA routing decision tree.
B: 1. Does "QA" appear as a two-letter abbreviation? No → QA predicate does not fire → no QA flow. Yes → 2. Does the request contain "before release", "before staging", "before production", "pre-production", "staging deployment", "release cut", "production deploy"? Yes → QA → 7.1 (ReleaseGateFlow71). No → QA → 7.3 (QAGatingFlow73).

---

## C0B-060-004 — Complete routing reference: when does 4.1 appear in AllSubPhaseIds?

A: In all 60 B39 topics, when does BackendServiceFlow41 (4.1) appear in AllSubPhaseIds?
B: Only when IMPLEMENT fires. IMPLEMENT fires on: build, implement, create, add [feature noun]. 4.1 appears in AllSubPhaseIds when and only when IMPLEMENT fires. In the 60 files reviewed, IMPLEMENT fires in examples containing: build the [component/page/feature], implement [feature/integration/service], create the [form/component], add a [feature noun]. All confirmed across Rounds 1, 2, and 3.

---

## C0B-060-005 — Complete routing reference: sub-phase ID complete map

A: State the complete sub-phase ID to FlowId mapping for B39.
B:
  1.5 → UIInteractionFlow15 (UI predicate)
  4.1 → BackendServiceFlow41 (IMPLEMENT predicate)
  5.1 → UnitIntegrationTestingFlow51 (TEST predicate, unit context — not used in Rounds 1-3)
  5.3 → E2ETestingFlow53 (TEST predicate, E2E/browser context — all 60 files)
  6.2 → DocumentationFlow62 (DOCUMENT predicate)
  7.1 → ReleaseGateFlow71 (QA predicate, release vocabulary)
  7.3 → QAGatingFlow73 (QA predicate, acceptance context — default)
  2.1 → ArchitectureDesignFlow21 (ARCHITECTURE predicate)
