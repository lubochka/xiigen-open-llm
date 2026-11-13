# Layer 0-A — Compiler Pairs (043) — HotfixPipeline
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: compiler

---

## B28-compiler-043-001 — "hotfix" alone is gap

A: "Cut a hotfix branch and push to production."
  "cut a hotfix branch" → gap | "push to production" → gap
  Teaching: "hotfix" branch vocabulary is gap. "push" is also gap in release context.
  CompiledPredicateIds: []

---

## B28-compiler-043-002 — IMPLEMENT+QA for hotfix implementation

A: "Implement the hotfix for the payment timeout bug and QA the fix before deploying."
  "implement" → IMPLEMENT | "fix" → gap (not a predicate trigger) | "QA" → QA
  Teaching: "implement" fires IMPLEMENT. "fix" alone is not a predicate. QA fires on abbreviation.
  CompiledPredicateIds: ["IMPLEMENT", "QA"]

---

## B28-compiler-043-003 — IMPLEMENT+TEST+QA for hotfix with verification

A: "Build the hotfix patch service, add regression tests for the patched code path, and QA sign-off."
  "build" → IMPLEMENT | "service" → IMPLEMENT (dedup) | "add regression tests" → TEST | "QA sign-off" → QA
  Teaching: Build+service → IMPLEMENT. Test-writing verb → TEST. QA last.
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "QA"]

---

## B28-compiler-043-004 — "emergency fix" and "critical patch" are gap

A: "Deploy the emergency fix and apply the critical patch to production."
  "deploy" → gap | "emergency fix" → gap | "critical patch" → gap | "apply" → gap
  Teaching: Emergency and patch vocabulary is gap. "deploy" and "apply" do not fire predicates.
  CompiledPredicateIds: []

---

## B28-compiler-043-005 — SECURITY+TEST+QA for security hotfix

A: "Secure the CVE (Common Vulnerabilities and Exposures) hotfix endpoint, write tests for the patched vulnerability, and QA the security remediation."
  "secure" → SECURITY | "write tests" → TEST | "QA" → QA
  Teaching: Security hotfix context: "secure" → SECURITY. Test-writing → TEST. QA last.
  CompiledPredicateIds: ["SECURITY", "TEST", "QA"]
