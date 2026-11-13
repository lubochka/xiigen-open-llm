# Node-08 - Arbiter Units Set

## Purpose

Runtime A is one selected review responsibility and the formal result or result group to examine.
Runtime B is pass, concern, or reprocess instruction.

## Pair 8.1 - Seller verification

A: Review responsibility: seller-verification arbiter. Formal result: listing was published for a seller without verified seller status.

B: Reprocess. The source requires verified seller status before marketplace publishing. Send the listing-creation fragment back and require seller eligibility to be confirmed before publication.

Pair 8.1 - Phrasing B

A: Review responsibility: seller eligibility review. Formal result: active listing exists after seller deactivation.

B: Reprocess. Seller deactivation cancels active listings. Send the listing-lifecycle fragment back and require cancellation and feed removal.

## Pair 8.2 - Duplicate and fake listing

A: Review responsibility: duplicate-listing arbiter. Formal result: a new listing is more than ninety percent similar to the seller's existing listing.

B: Reprocess. The source says duplicates above ninety percent similarity should be blocked and the seller should be guided to update the existing listing. Send the item-creation fragment back.

Pair 8.2 - Phrasing B

A: Review responsibility: fake-listing review. Formal result: unverified listing content passes directly to feeds.

B: Concern. Marketplace publishing requires seller verification and content moderation. Require the result to show the review gate before distribution.

## Pair 8.3 - Pricing audit

A: Review responsibility: price-change-history arbiter. Formal result: price changed after cooperator agreement with no notice or grace period.

B: Reprocess. The source requires cooperators to be notified and given a forty-eight-hour grace period for existing bundle agreements. Send the pricing fragment back.

Pair 8.3 - Phrasing B

A: Review responsibility: discount-range review. Formal result: group member discount is forty percent.

B: Concern. The source defines group member discount as ten to fifteen percent. Require the discount result to be corrected or marked as outside the source-defined range.

## Pair 8.4 - Inventory fraud

A: Review responsibility: inventory-consistency arbiter. Formal result: feed cards continue to show purchasable stock after inventory reaches zero.

B: Reprocess. The source requires active feeds to be removed or marked sold out when inventory is depleted. Send the inventory and feed fragments back.

Pair 8.4 - Phrasing B

A: Review responsibility: purchase-readiness review. Formal result: pre-order item is displayed as in-stock purchase-ready.

B: Reprocess. Pre-order listings must show coming-soon or pre-order status until stock arrives. Send the listing-state fragment back.

## Pair 8.5 - Cooperator scoring

A: Review responsibility: competitor-exclusion arbiter. Formal result: a competing product receives a partnership opportunity card.

B: Reprocess. The source says competing products receive synergy score zero and must never be shown as partnership opportunities. Send the cooperator-scoring fragment back.

Pair 8.5 - Phrasing B

A: Review responsibility: synergy-weight review. Formal result: synergy score omits audience overlap and product complementarity.

B: Reprocess. The source defines the score factors and weights. Send the synergy fragment back and require all factors to be represented or explicitly unavailable.

## Pair 8.6 - Cooperator spam

A: Review responsibility: cooperator-spam arbiter. Formal result: one seller sends twenty partnership requests in a day.

B: Reprocess. The source limits partnership requests to five per day. Send the notification fragment back and require the excess requests to be blocked or delayed.

Pair 8.6 - Phrasing B

A: Review responsibility: notification-rate review. Formal result: all low-synergy businesses receive partnership notifications.

B: Concern. Partnership notifications should target qualified synergy results. Require low-synergy recipients to receive ordinary marketplace exposure or no cooperator notification.

## Pair 8.7 - Feed card correctness

A: Review responsibility: card-format arbiter. Formal result: cooperators receive a normal product card instead of a partnership opportunity card despite high synergy.

B: Reprocess. High-synergy cooperators should see partnership opportunity cards with audience overlap and collaboration suggestions. Send the feed-card fragment back.

Pair 8.7 - Phrasing B

A: Review responsibility: marketplace-enabled-group review. Formal result: group auto-post appears in a group that does not enable marketplace sections.

B: Reprocess. Group auto-posting must only occur in marketplace-enabled groups. Send the group-distribution fragment back.

## Pair 8.8 - Compliance and analytics

A: Review responsibility: consumer-terms arbiter. Formal result: marketplace purchase flow omits seller terms, return policy, or tax transparency.

B: Concern. The source says consumer protection laws apply and terms, return policy, and tax calculation transparency must be clear. Require buyer-facing terms to be added before completion is claimed.

Pair 8.8 - Phrasing B

A: Review responsibility: analytics-coverage review. Formal result: distribution completed but no listing inquiries, cooperator acceptance, discount redemption, or revenue metrics are recorded.

B: Reprocess. The source defines marketplace metrics for performance. Send the analytics fragment back and require the missing performance measures.

