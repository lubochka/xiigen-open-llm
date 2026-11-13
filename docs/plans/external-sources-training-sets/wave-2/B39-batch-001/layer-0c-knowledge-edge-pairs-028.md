# Layer 0-C — Knowledge Edge Pairs (028)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E028-001 — LoginFlowTest: gap vocabulary

A: Which words are gap in login test requests?
B: Gap: "run the login tests" ("run" is gap), "execute login spec" ("execute" is gap), "automate login with Playwright" ("automate" and "Playwright" are gap), "check login works" ("check" is gap), "configure SSO" ("configure" is gap for IMPLEMENT), "deploy the login service" ("deploy" is gap), "monitor login failures" ("monitor" is gap). "login" and "authentication" alone are domain descriptors, not predicate triggers.

---

## E028-002 — LoginFlowTest: "screen" fires UI

A: "Verify the login screen redirects to the dashboard screen." Does "dashboard screen" fire UI?
B: "login screen" → "screen" fires UI. "dashboard screen" → "screen" fires UI again, but deduplication applies: multiple "screen" occurrences → one UI predicate. "dashboard" alone is gap for UI, but "dashboard screen" contains "screen" which fires UI. Result: TEST ("verify") + UI ("screen" deduplicated). Note: "dashboard" alone → gap. "dashboard screen" → "screen" fires UI.

---

## E028-003 — LoginFlowTest: error banner vocabulary

A: Does "error banner" fire UI?
B: "error banner" → "banner" — is "banner" in the UI lexicon? The explicit UI lexicon in B39 does not list "banner" in the base definition, but contextually similar to toast/snackbar. Re-evaluation: the held-out examples (024) include "error banner" in context and it fires UI (C024-001 held-out: "ARIA roles" fires UI). Teaching: "banner" is used in role-based ARIA (role="banner") and is treated as UI vocabulary in B39. "error banner" → UI fires. Conservative rule: when in doubt, check for explicit lexicon words first.

---

## E028-004 — LoginFlowTest: "remember-me toggle" fires UI

A: "Assert the remember-me toggle retains state after page reload." What fires?
B: TEST ("assert" → TEST); UI ("remember-me toggle" → "toggle" fires UI; "page" → "page" fires UI; deduplicated). Result: [TEST, UI]. "toggle" and "page" are both in the UI lexicon.

---

## E028-005 — LoginFlowTest: "redirect" alone is gap

A: "Verify the login redirect goes to the correct URL." Does "redirect" fire UI?
B: No. "redirect" is not in the UI lexicon. "URL" is not in the UI lexicon. But "verify" fires TEST. Result: [TEST]. Routing: [5.3]. No UI sub-phase if no explicit UI vocabulary word is present. TransitionType: documentation.
