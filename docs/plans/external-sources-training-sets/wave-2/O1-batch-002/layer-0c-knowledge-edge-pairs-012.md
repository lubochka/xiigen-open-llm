# Layer 0-C — Knowledge Edge Pairs (012)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## O1b-0C-012-001 — Complete knowledge_edge decision table: colloquial phrases

A: O1b knowledge_edge decision table — always knowledge_edge

B: | Phrase | Why knowledge_edge |
   |--------|-------------------|
   | "client portal is down" | "portal", "down" not in lexicon |
   | "access denied" | colloquial error phrase, not lexicon term |
   | "permission denied" | UI/OS error message, not technical requirement |
   | "login is broken" | "login" explicitly excluded from SECURITY |
   | "can't log in" | "log in" not in lexicon |
   | "users getting kicked out" | colloquial, no lexicon word |
   | "not authorized" | status phrase, not technical requirement |
   | "users losing access" | colloquial, "access" alone not in lexicon as predicate |

---

## O1b-0C-012-002 — Complete knowledge_edge decision table: SECURITY keywords

A: O1b SECURITY fire table

B: | Term | Fires | Why |
   |------|-------|-----|
   | oauth | SECURITY | explicit lexicon entry |
   | token | SECURITY | explicit lexicon entry |
   | jwt | SECURITY | explicit lexicon entry |
   | session | SECURITY | explicit lexicon entry |
   | credential | SECURITY | explicit lexicon entry |
   | authentication / auth | SECURITY | explicit lexicon entry |
   | certificate | SECURITY | explicit lexicon entry |
   | encrypt / encryption | SECURITY | explicit lexicon entry |
   | tls / ssl | SECURITY | explicit lexicon entry |
   | login | NONE | explicitly excluded |
   | sign in / sign out | NONE | not in lexicon |

---

## O1b-0C-012-003 — Complete knowledge_edge decision table: AUTHORIZATION keywords

A: O1b AUTHORIZATION fire table

B: | Term | Fires | Why |
   |------|-------|-----|
   | scope | AUTHORIZATION | explicit lexicon entry (software context) |
   | tenant | AUTHORIZATION | explicit lexicon entry |
   | grant | AUTHORIZATION | explicit lexicon entry |
   | revoke | AUTHORIZATION | explicit lexicon entry |
   | principal | AUTHORIZATION | explicit lexicon entry |
   | service account | AUTHORIZATION | maps to principal/account with scope |
   | role / rbac | AUTHORIZATION | explicit lexicon entry |
   | permission | AUTHORIZATION | in technical requirement context |
   | permission denied | NONE | colloquial error phrase |
   | access denied | NONE | colloquial error phrase |

---

## O1b-0C-012-004 — knowledge_edge classification rule

A: O1b knowledge_edge rule stated as axiom

B: AXIOM: A prompt is classified as knowledge_edge if and only if zero predicate lexicon keywords are found in the prompt (after accounting for contextual disambiguation). One lexicon keyword present → valid routing, never knowledge_edge. The classification is binary: either a keyword fires or the prompt is unroutable.

---

## O1b-0C-012-005 — knowledge_edge corpus use

A: What happens to knowledge_edge prompts in O1b training?

B: knowledge_edge prompts are: (1) logged to the unroutable corpus for corpus review. (2) NOT routed to any sub-phase. (3) NOT used to start a nine-node pipeline. (4) Used as negative examples in training to teach the model what NOT to classify as SECURITY or AUTHORIZATION. They are valuable training data precisely because they demonstrate the absence of predicates.
