# Layer 0-C — Knowledge Edge Pairs (016)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E016-001 — "threat model" fires SECURITY in audit context; "architecture diagram" alone is gap

A-fires: "Review the threat model for the OrderService — are all STRIDE (Spoofing, Tampering, Repudiation, Information Disclosure, Denial of Service, Elevation of Privilege) categories addressed?"
  "threat model" → SECURITY (security action context); "STRIDE" → SECURITY; "review" + threat model co-occurrence → SECURITY
  CompiledPredicateIds: ["SECURITY"]

A-gap: "Draw the threat model diagram for the new microservice."
  "draw" + "diagram" → DOCUMENT? Yes if "document" verb is used. "draw a diagram" → borderline but no strong predicate.
  CompiledPredicateIds: ["DOCUMENT"] — "draw/create diagram" maps to DOCUMENT, not SECURITY.

---

## E016-002 — "attack vector" fires SECURITY; "feature vector" is gap

A-fires: "Enumerate all attack vectors in the authentication flow — focus on session hijacking and token replay."
  "attack vectors" → SECURITY; "session hijacking" → SECURITY (session trigger); "token replay" → SECURITY (token trigger)
  CompiledPredicateIds: ["SECURITY"]

A-gap: "Define the feature vector for the recommendation engine training."
  "feature vector" → gap (ML context, not security)
  CompiledPredicateIds: []

---

## E016-003 — "STRIDE" fires SECURITY; "DFD" (Data Flow Diagram) alone is gap

A: "Complete the STRIDE threat model for the PaymentService using the data flow diagrams."
  "STRIDE" → SECURITY; "threat model" → SECURITY; "data flow diagrams" → context (not independent predicate)
  CompiledPredicateIds: ["SECURITY"]
  Teaching: STRIDE is a threat modeling framework — its name fires SECURITY. DFD (Data Flow Diagram) alone is gap.

---

## E016-004 — "risk assessment" is gap; "vulnerability assessment" fires SECURITY

A-gap: "Conduct a risk assessment for the new payment feature before release."
  "risk assessment" → gap; "conduct" → gap
  CompiledPredicateIds: [] → knowledge_edge
  Gap ID: gap-security-risk-vocabulary-001

A-fires: "Conduct a vulnerability assessment of the payment feature before release."
  "vulnerability assessment" → SECURITY; "conduct" + vulnerability context → SECURITY
  CompiledPredicateIds: ["SECURITY"]
  Teaching: "risk" vocabulary is gap. "vulnerability" vocabulary fires SECURITY.

---

## E016-005 — "privilege escalation" fires BOTH SECURITY and AUTHORIZATION

A: "Audit the AdminService for privilege escalation paths — can a user-role account access admin-only endpoints?"
  "audit" → SECURITY; "privilege escalation" → SECURITY + AUTHORIZATION
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"]
  Teaching: "privilege escalation" is an explicit dual-predicate trigger per predicate rules.
