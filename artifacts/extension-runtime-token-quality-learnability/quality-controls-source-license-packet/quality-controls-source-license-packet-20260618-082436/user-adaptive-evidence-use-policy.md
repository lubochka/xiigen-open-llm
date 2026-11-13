# User-Adaptive / Evidence Use Policy

Да, часть текущих private/session rows можно сохранить как future `USER_ADAPTIVE_ONLY` без common training, но только с жесткими ограничениями.

- Future USER_ADAPTIVE_ONLY scope: 7/12 scenarios, 21/54 pair rows.
- EVIDENCE_ONLY scope: 5/12 scenarios, 33/54 pair rows.
- COMMON_ALLOWED scope: 0/12 scenarios, 0/54 pair rows.

## Limits

- Only under user-local/private root such as .xiigen or another approved user root.
- Requires explicit later approval, user-local store manifest, privacy/redaction refs, and DPO/material-quality review before any training/eval mutation.
- Must not update common datasets, common checkpoints, locator, or product readiness.
- Must keep raw private/session text out of shared reports; shared packets need redacted refs plus hashes.
- Held-out/source-disjoint and ablation controls remain required before any learnability claim.

## Evidence-Only Limits

- May support review/control explanation and gap tracking only.
- Does not authorize training, eval mutation, common acceptance, or train/held-out split.
- Project-local rows need a separately filled packet or newly derived permissive/open row before common use.

Ни одна строка сейчас не разрешает training/eval mutation, common dataset update, checkpoint update, locator update или readiness claim.
