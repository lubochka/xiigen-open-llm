# Layer 3 — Teaching-Unit Pairs (017)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: teaching-unit-pairs

---

## B37-L3-017-001 — Teaching: lessons-learned is gap vocabulary

**Concept:** Lessons-learned sessions (retrospectives) are B37 gap vocabulary.

Q: "Run the lessons-learned session." → predicates?
A: []. DomainId "project-management". knowledge_edge. SCL emitted.

Q: "Facilitate the retrospective." → predicates?
A: []. DomainId "project-management".

Q: "Review the lessons learned." → predicates?
A: [QA]. "review" fires QA. DomainId "software-development".

---

## B37-L3-017-002 — Teaching: "run" vs "review" boundary for sessions

| Token | Predicate | Notes |
|-------|----------|-------|
| run | None | "run" not in any predicate lexicon |
| facilitate | None | "facilitate" not in any predicate lexicon |
| conduct | None | "conduct" not in any predicate lexicon |
| hold | None | "hold" not in any predicate lexicon |
| review | QA | QA trigger (Rule 19) |
| validate | QA | QA trigger |

"Run/facilitate/conduct the lessons-learned session" → zero predicates.
"Review the lessons learned" → QA fires.

---

## B37-L3-017-003 — Teaching: lessons-learned action items compound

When lessons-learned is combined with action items and follow-up:

Q: "Track lessons-learned action items and follow up." → predicates?
A: []. All gap vocabulary: track (Rule 2), action items, follow-up. DomainId "project-management".

Gap IDs: manage-delivery-001, track-001, action-items-001, follow-up-001, accountability-001.

---

## B37-L3-017-004 — Teaching: "document" vs "share" for lessons learned

Q: "Share the lessons learned with the team." → predicates?
A: []. "share" not in any predicate lexicon. DomainId "project-management".

Q: "Document the lessons learned." → predicates?
A: [DOCUMENT]. "document" fires DOCUMENT. DomainId "software-development".

Q: "Share the lessons learned with stakeholders." → predicates?
A: [DOCUMENT]. "stakeholders" fires DOCUMENT. DomainId "software-development".

---

## B37-L3-017-005 — Teaching: lessons-learned exam

1. "Run lessons-learned session." → []
2. "Facilitate retrospective." → []
3. "Track action items from lessons." → []
4. "Share lessons with team." → []
5. "Review lessons learned." → [QA]
6. "Document lessons learned." → [DOCUMENT]
7. "Document + share with stakeholders." → [DOCUMENT]
8. "Define lessons-learned structure." → [DOCUMENT, ARCHITECTURE]
9. DomainId for question 1? → "project-management"
10. DomainId for question 8? → "software-development"
