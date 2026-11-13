# User-Adaptive-Only Continuation Map

Run: `quality-controls-derived-permissive-source-review-20260618-084753`

Rows that may proceed later only in a user-local/adaptive path if policy and approval allow: **7/12 scenarios** and **21/54 pair rows**.

Rows that remain evidence-only now: **5/12 scenarios** and **33/54 pair rows**.

Non-common constraints:

- No common training and no common checkpoint mutation.
- No current training, model, checkpoint, locator, or target mutation from this packet.
- User-local/adaptive continuation needs explicit approval, user-local store manifest, DPO/source/material review, redaction/provenance, and lifecycle gates if training later occurs.
- Evidence-only rows are review/control evidence only unless a separate packet changes their route.
- No strict QUALITY-03, common readiness, token savings, semantic readiness, learnability, target promotion, or locator update claim is opened here.

The complete row list is in `user-adaptive-only-continuation-map.json` and `user-adaptive-only-continuation-map.jsonl`.
