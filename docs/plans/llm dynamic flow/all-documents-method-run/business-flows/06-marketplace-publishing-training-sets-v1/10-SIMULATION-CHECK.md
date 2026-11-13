# 10 - Simulation Check

## Source Being Simulated

`C:\Users\lubao\Documents\xiigen\business flows\06-marketplace-publishing.md`

The simulation checks whether these node sets can recreate the marketplace-publishing flow without using the source document as runtime input.

## Test Prompt 1 - Full Marketplace Listing

Runtime user prompt:

"A verified seller lists a product with pricing, inventory, media, and target audience. Publish it, create a marketplace post, profile buyers, find cooperators, distribute to friends and groups with discounts, notify high-synergy partners, and report performance."

Expected behavior:

- Node-01 returns item creation, inventory, pricing, listing lifecycle, audience profiling, marketplace-post generation, cooperator scoring, friend and group distribution, feed cards, notifications, and analytics.
- Node-02 writes exact marketplace-domain statements for each label.
- Node-03 stores the item before publishing the listing and keeps profiling, content generation, and cooperator matching as separate branches.
- Node-04 processes item state, listing state, audience profile, post content, cooperator scores, friend/group audiences, feed-card variants, and analytics.
- Node-05 applies item storage, listing activation, branch outputs, synergy scoring, multi-format feed distribution, notifications, and analytics.
- Node-07 selects seller, inventory, pricing, cooperator, feed-card, compliance, and analytics arbiters.
- Node-08 rejects unverified sellers, duplicate products, wrong pricing, stock fraud, competitor partnership cards, cooperator spam, and missing metrics.
- Node-09 returns one answer describing the listing, promotion, partnership matching, distribution, notifications, and measurements.

Pass condition:

The simulated flow must include the three parallel branches after listing creation and must use different feed formats for friends, groups, and cooperators.

## Test Prompt 2 - Competing Cooperator

Runtime user prompt:

"A business with a competing product appears in the cooperator scan. Do not show it a partnership opportunity."

Expected behavior:

- Node-01 returns cooperator synergy scoring and marketplace safety.
- Node-02 translates competitor handling into synergy-zero partnership exclusion.
- Node-03 keeps regular marketplace visibility separate from partnership opportunity.
- Node-05 applies competitor exclusion from cooperator cards.
- Node-08 rejects any partnership card sent to a competitor.
- Node-09 explains that competitors are not cooperators.

Pass condition:

The result must not produce a partnership opportunity card for the competitor.

## Test Prompt 3 - Inventory Depleted

Runtime user prompt:

"The listing reaches zero stock while the marketplace post is still being promoted."

Expected behavior:

- Node-01 returns inventory and availability, feed distribution, marketplace safety.
- Node-03 plans sold-out status, active-feed removal, sold-out badge, and restock cooperator continuity.
- Node-05 updates feeds and listing availability.
- Node-08 rejects purchasable feed cards after stock reaches zero.
- Node-09 explains sold-out handling.

Pass condition:

The simulation must not keep promoting the item as available.

## Test Prompt 4 - Limited-Time Offer

Runtime user prompt:

"Publish a marketplace listing with an expiry time, countdown feed cards, urgent notifications, and automatic removal after expiry."

Expected behavior:

- Node-01 returns limited-time offers, listing lifecycle, feed cards, notifications.
- Node-03 keeps expiry attached to listing state and plans removal.
- Node-05 applies countdown and expiry cleanup.
- Node-08 checks stale offer removal.
- Node-09 explains temporary-offer handling.

Pass condition:

The output must include removal after expiry.

## Test Prompt 5 - Seller Deactivated

Runtime user prompt:

"The seller deactivates their account while listings and pre-orders are active."

Expected behavior:

- Node-01 returns listing lifecycle, inventory and availability, notifications, consumer protection.
- Node-03 cancels listings, removes feeds, notifies cooperators, and starts pre-order refunds.
- Node-05 applies cleanup.
- Node-08 rejects active listings from inactive seller.
- Node-09 explains inactive-seller cleanup.

Pass condition:

The simulation must not leave active listings visible.

## Missing-Training Detector

If the simulation publishes a listing before item storage and seller verification, add Node-03, Node-04, Node-05, and Node-08 creation-order pairs.

If profiling, post generation, and cooperator scoring collapse into one step, add Node-03 branch pairs.

If synergy scoring omits weights or treats competitors as partners, add Node-02, Node-05, and Node-08 cooperator pairs.

If friends, groups, and cooperators receive the same generic feed card, add Node-04, Node-05, and Node-09 card-format pairs.

If sold-out listings remain purchasable, add Node-04, Node-05, and Node-08 inventory pairs.

If limited-time offers never expire, add Node-03 and Node-08 expiry pairs.

If transaction terms, return policy, or tax transparency disappear from purchase-facing results, add Node-07 and Node-08 compliance pairs.

