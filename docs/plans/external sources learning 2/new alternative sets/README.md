# Alternative Learning Set — Batch ES-2026-05-10-ALT

## Purpose

Parallel DPO learning set produced from **Families 9–14** added to
`EXTERNAL-SOURCE-REGISTRY-v7.md (v7.1, 2026-05-10)`. Coverage targets the
**config-driven multi-vertical platform** architecture described in
`Multimodel Orchestrator/module-architecture-extracted/DEFINITIVE_MODULE_ARCHITECTURE.md`
(Genie DNA: 21 modules × 15 site types, 3-layer Editor/Storage/Machine,
Strategy+Factory engines, JSON-in-Elasticsearch).

This set is independent of the original `new sets/` library — different ID
namespace, different sources, complementary domain coverage.

---

## License gate

Strict filter applied: **MIT and Apache 2.0 only.** Every row's source is
mapped in the registry under one of these families:

| Family | Theme | Sources | Licenses |
|---|---|---|---|
| 9 | Config-Driven Admin Platforms | strapi, adminjs, refine, keystone, payload, tinacms, appsmith | 5× MIT, 2× Apache 2.0 |
| 10 | Schema-Driven Form Engines | rjsf, formik, react-hook-form, zod, yup | 1× Apache 2.0, 4× MIT |
| 11 | Modular Commerce / Multi-Vertical | medusajs | MIT |
| 12 | Headless UI Primitives & Generic Engines | radix-ui, shadcn-ui, TanStack ×4, dnd-kit, plate, fluentui, primer, polaris | 11× MIT |
| 13 | Apache 2.0 Design Systems | carbon, spectrum-css, react-spectrum | 3× Apache 2.0 |
| 14 | Service Catalogue / Plugin / Visual Editor | backstage, excalidraw, tldraw | 2× Apache 2.0, 1× MIT |

Every row's `source_license_state = external_license_checked`.

---

## File namespace and ID convention

| File | ID prefix | Format mirror |
|---|---|---|
| BUG-Rows.md | BUG-A0001..BUG-Annnn | matches `new sets/BUG-Rows.md` |
| PLAN-Rows.md | PLAN-A0001.. | matches `new sets/PLAN-Rows.md` |
| ARCH-Rows.md | ARCH-A0001.. | matches `new sets/ARCH-Rows.md` |
| CODE_Q-Rows.md | CODEQ-A0001.. | matches `new sets/CODE_Q-Rows.md` |
| REFACTOR-Rows.md | REFACTOR-A0001.. | matches `new sets/REFACTOR-Rows.md` |

The `-A` suffix marks alternative-set rows so calibration runs can
disambiguate from the original library.

Row format (one line per pair):

```
[<ID> | <Type>/<overlap_ratio>/<transition_type>/<sub_phase>] <source> | A:"<prompt>" | B:<chosen> | P:<rejected_path> | R:[WRONG: <annotation>]
```

(CODE_Q drops `transition_type`; REFACTOR drops `sub_phase`.)

`<Type>`: A (adjacent sub-phase confusion), B (wrong dependency order),
C (wrong domain), D (ADR alternative), E (spec supersession), F (dispatchable
Gate B), J (mermaid topology).

`<overlap_ratio>` < 0.60 mandatory per
`DPO-PAIR-AUTHORING-GUIDANCE-v2.md` line 58.

---

## Domain coverage map

| Genie DNA module | Primary family | Secondary family |
|---|---|---|
| Dynamic Router (`menu_structure`) | 14 (backstage) | 12 (TanStack Router) |
| Home/Search Page (`view_definitions`) | 9 (strapi, payload) | 12 (TanStack Table) |
| Single View Page (`detail_definitions`) | 9 (refine, payload) | 12 (radix, shadcn) |
| Categories/Tags (`category_definitions`) | 9 (strapi, keystone) | — |
| Linked Items (`linked_items_configs`) | 9 (strapi populate) | 12 (TanStack Query) |
| Feed (`feed_configs`) | 9 (tinacms) | 12 (TanStack Query) |
| Profile (`profile_definitions`) | 9 (refine, keystone) | 12 (radix Form) |
| Shopping Cart (`cart_rules`) | 11 (medusa cart) | 9 (refine + medusa) |
| Availability (`availability_rules`) | 11 (medusa inventory) | — |
| Promotions (`promotion_definitions`) | 11 (medusa promotions) | — |
| Packages (`package_definitions`) | 11 (medusa product variants) | — |
| Coupons (`coupon_definitions`) | 11 (medusa discount) | — |
| Tab/Credit (`wallet_configs`) | 11 (medusa gift card) | — |
| Subscription (`subscription_plans`) | 11 (medusa subscriptions) | — |
| Adjustments (`adjustment_configs`) | 11 (medusa order edits) | — |
| Add Item Forms (`form_definitions`) | 10 (rjsf, react-hook-form, zod) | 12 (radix Form) |
| Chat (`chat_configs`) | 9 (refine + appsmith) | 12 (radix) |
| Service Request (`ticket_configs`) | 9 (refine, payload) | 12 (TanStack Form) |
| Cancellation/Refund (`cancellation_policies`) | 11 (medusa returns) | — |
| Invoices (`invoice_configs`) | 9 (payload), 11 (medusa) | — |
| Ad Monetization (`ad_configs`) | 9 (strapi blocks) | 12 (radix) |
| Style Maps (`style_maps`) | 13 (carbon, spectrum) | 12 (shadcn theme) |
| Editor / Visual Builder | 14 (excalidraw, tldraw) | 9 (appsmith, tinacms) |
| Plugin / Extension API | 14 (backstage) | 9 (strapi plugins) |
| Admin Layer 1 design language | 13 (carbon, spectrum) | 12 (fluentui, polaris, primer) |

---

## Pair quality rules

Universal rules from `EXTERNAL-SOURCE-REGISTRY-v7.md` apply unchanged:

1. No "Do not generate a capability" rejected template.
2. Every rejected response has a `[WRONG: ...]` annotation explaining the
   confusion (sub-phase/layer/concept).
3. Overlap ratio < 0.60 between A and B.
4. No source-syntax (CLI commands, schema literals) as A-side prompts —
   real product prompts describe behaviour, not syntax.
5. Every row maps to at least one runtime contract in the registry.

---

## Generation progress

```
BUG-Rows.md        : 4200 rows  (BUG-A0001..A4200)
PLAN-Rows.md       : 4200 rows  (PLAN-A0001..A4200)
ARCH-Rows.md       : 4200 rows  (ARCH-A0001..A4200)
CODE_Q-Rows.md     : 4200 rows  (CODEQ-A0001..A4200)
REFACTOR-Rows.md   : 4200 rows  (REFACTOR-A0001..A4200)
Total              : 21000 rows after batch 26 (200 added per file)
Target per domain  : ≥ 10000 rows (target exceeded; continued for breadth)
```

---

## Last Updated

2026-05-11 (batch 26 — 4200 rows per file, families 9–14, MIT/Apache-2.0 only).
