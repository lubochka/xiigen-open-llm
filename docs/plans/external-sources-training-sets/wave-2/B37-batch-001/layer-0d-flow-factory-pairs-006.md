# Layer 0-D — Flow Factory Pairs (006)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: flow-factory-pairs

---

## B37-0D-006-001 — IMPLEMENT factory: BackendServiceFlow41 (build in managing sentence)

**Input:** "Manage the delivery of the authentication service and build the access control module."

**Compiler output:** ["IMPLEMENT"]

**Flow factory decision:**
- IMPLEMENT (service, build, module) → BackendServiceFlow41 (sub-phase 4.1)
- DomainId: "software-development"
- Factory: BackendServiceFlow41

**Gap warnings forwarded:**
- gap-project-managing-manage-delivery-001

**Factory note:** BackendServiceFlow41 is selected because IMPLEMENT fired on software artifacts (service, module). The "manage the delivery" framing is managing vocabulary — gap. The factory processes the build/service/module work, not the managing work.

---

## B37-0D-006-002 — IMPLEMENT factory: "create" in managing sentence

**Input:** "Create the status tracking module and integrate it with the delivery dashboard."

**Compiler output:** ["IMPLEMENT"]

**Flow factory decision:**
- IMPLEMENT (create, module) → BackendServiceFlow41
- DomainId: "software-development"

**Gap warnings:**
- gap-project-managing-track-status-001
- gap-project-managing-manage-delivery-001

---

## B37-0D-006-003 — IMPLEMENT factory: "write" fires even in documentation-adjacent sentence

**Input:** "Write the escalation tracking script and deploy it to the project servers."

**Compiler output:** ["IMPLEMENT"]

**Flow factory decision:**
- IMPLEMENT (write) → BackendServiceFlow41
- "tracking" → gap (track family)
- "escalation" → gap (Rule 13)
- "script" → not a predicate trigger but IMPLEMENT already fired via "write"
- DomainId: "software-development"

**Gap warnings:**
- gap-project-managing-escalation-001
- gap-project-managing-track-status-001

---

## B37-0D-006-004 — IMPLEMENT + DOCUMENT factory: build + document combined

**Input:** "Build the escalation service module and document the acceptance criteria."

**Compiler output:** ["IMPLEMENT", "DOCUMENT"]

**Flow factory decision:**
- IMPLEMENT (build, service, module) → BackendServiceFlow41
- DOCUMENT (document, criteria) → DocumentationFlow
- Parallel: BackendServiceFlow41 + DocumentationFlow
- DomainId: "software-development"

**Gap warnings:**
- gap-project-managing-escalation-001
- Also: "acceptance" fires TEST on its own → but "acceptance criteria" here: "acceptance" is TEST, "criteria" is DOCUMENT — full result would be ["TEST", "IMPLEMENT", "DOCUMENT"]

Corrected analysis:
- "acceptance" → TEST fires (Rule 2: acceptance is in TEST lexicon)
- "build" → IMPLEMENT fires
- "service" → IMPLEMENT fires
- "module" → IMPLEMENT fires
- "document" → DOCUMENT fires
- "criteria" → DOCUMENT fires

Corrected result: ["TEST", "IMPLEMENT", "DOCUMENT"]

Factory: TestFlow5x + BackendServiceFlow41 + DocumentationFlow

---

## B37-0D-006-005 — IMPLEMENT factory in project managing context: summary

**IMPLEMENT fires in B37 sentences when:**
1. "build the [service/module]" — build + artifact
2. "create the [tracking/status] module" — create + module
3. "implement the [feature/service]" — implement + artifact
4. "write the [script/service]" — write + artifact
5. "[authentication/escalation] service" — "service" token alone

**Each case routes to BackendServiceFlow41 regardless of the managing context surrounding the IMPLEMENT-triggering tokens.**

**IMPLEMENT does NOT fire on:**
- "delivery service" when "service" means business/operational service (ambiguous; lexical rule applies)
- "manage the service" — "manage" is gap; "service" fires IMPLEMENT (lexical rule applies regardless)
