# Local Derived Permissive Source Search

Run: `quality-controls-derived-permissive-source-review-20260618-084753`

No web was used. I searched the previous packet folder, previous quality-controls folders, the scenario pack, allowed local references, and manifest-referenced license folders.

Result: **NO_ACCEPTED_DERIVED_PERMISSIVE_SOURCE_PACKET_FOUND**.

Accepted derived permissive/open source packets found: **0**.

Reusable metadata candidates found but not accepted for current rows:

- `doc-05627-license`: local Apache License domain document. Not accepted because it is not bound to the current 12 scenario / 54 pair `source_ref/doc_id` rows.
- `doc-05649-license`: local LGPL license domain document. Not accepted because it is not a target MIT/Apache/open-permissive source packet and is not bound to current rows.

Counts: scenarios 12, pair rows 54, common allowed 0/12 and 0/54, user-adaptive-only 7/12 and 21/54, evidence-only 5/12 and 33/54.

What this proves: local search did not find an accepted permissive source/license packet for the current rows.

What this does not prove: quality improvement, token savings, learnability, semantic readiness, common readiness, strict QUALITY-03 readiness, target promotion, or locator update.
