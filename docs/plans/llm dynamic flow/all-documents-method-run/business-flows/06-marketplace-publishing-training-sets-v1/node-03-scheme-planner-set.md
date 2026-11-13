# Node-03 - Scheme Planner Set

## Purpose

Runtime A is the full set of domain translations from Node-02.
Runtime B is an ordered and parallel plan with wait states, recovery paths, and review points.

## Pair 3.1 - Full marketplace publishing plan

A: The translated domains say that a verified seller creates an item with inventory, pricing, media, and target audience; the listing becomes active; audience profiling, marketplace-post generation, and cooperator scoring run; the post is distributed to friends, groups, and cooperators; notifications and analytics complete the flow.

B: Plan: first validate seller status and item details. Store the item with inventory, pricing, media, target audience, and availability. Create the active listing only after the item is valid. Start three branches: audience profiling, marketplace-post generation, and cooperator matching. Cooperator matching uses audience profile when available and must not classify competitors as partnership opportunities. After marketplace post and audience results exist, distribute friend product cards, group marketplace posts, and cooperator partnership cards. Send high-synergy cooperator notifications. Record marketplace analytics. Review seller verification, inventory consistency, pricing audit, duplicate listing, cooperator spam, feed-card correctness, and compliance.

Pair 3.1 - Phrasing B

A: The translated descriptions require item creation, listing activation, audience analysis, promotional content, partner matching, multi-audience feed cards, notifications, and measurement.

B: Plan: item state comes before listing state. Listing state comes before promotion. Profiling, content generation, and cooperator discovery run as separate branches. Distribution waits for the content and recipient outputs it needs, then uses different cards for friends, groups, and cooperators.

## Pair 3.2 - Limited-time offer

A: The translated domains say the listing has an expiry timestamp, time-sensitive notifications, countdown feed cards, and auto-removal after expiry.

B: Plan: store the item and expiry timestamp. Publish the listing with limited-time offer status. Generate feed cards with countdown language. Mark notifications as time-sensitive. Schedule expiry handling that removes the offer from active feeds when the timestamp passes. Review time handling, offer removal, and stale notification risk.

Pair 3.2 - Phrasing B

A: The translated descriptions say the offer is temporary and must disappear after the deadline.

B: Plan: keep expiry as part of the listing state, show urgency in cards and notifications while valid, and remove or mark expired content when time runs out.

## Pair 3.3 - Pre-order item

A: The translated domains say availability is pre-order with restock date, social post shows coming-soon badge, cooperator matching still runs, and listing converts when stock arrives.

B: Plan: store the listing as pre-order instead of in-stock. Generate coming-soon promotional content. Run cooperator matching for partnership discussions even before stock exists. Do not treat pre-order as purchase-ready inventory. When stock arrives, convert the listing to regular active availability. Review stock display and purchase eligibility.

Pair 3.3 - Phrasing B

A: The translated descriptions say the item is not available now but should still be marketed and matched for partnerships.

B: Plan: separate promotional readiness from fulfillment readiness. The marketplace can show the coming-soon item and match cooperators, but purchase behavior waits for stock.

## Pair 3.4 - Service listing

A: The translated domains say the item is a service, has no inventory tracking, uses a service description and booking call to action, and cooperator matching focuses on service complement.

B: Plan: create a service listing without stock count. Generate a service-format feed card with booking call to action. Run cooperator matching using service complementarity rather than physical product bundle assumptions. Skip inventory depletion handling. Review service-card format and no-stock behavior.

Pair 3.4 - Phrasing B

A: The translated descriptions say the listing sells a service instead of a physical product.

B: Plan: the listing remains commercial, but availability is not measured by stock. Distribution uses booking-focused cards, and partner matching looks for complementary services.

## Pair 3.5 - Wholesale pricing

A: The translated domains say the listing has multiple quantity-based pricing tiers, cooperator feeds show wholesale pricing, and group marketplace posts show a volume discount badge.

B: Plan: store all quantity tiers. Generate friend, group, and cooperator card variants. Show wholesale pricing specifically to cooperators. Show volume discount badge in group marketplace posts. Review price-display accuracy and audience-specific pricing.

Pair 3.5 - Phrasing B

A: The translated descriptions say different audiences need different price presentations for bulk buying.

B: Plan: keep pricing tiers attached to the listing and choose the correct display per audience: group users see volume discount, cooperators see wholesale partnership pricing, and friends see the friend discount when defined.

## Pair 3.6 - Inventory depleted during promotion

A: The translated domains say stock reaches zero while promotion is active; remove from active feeds, show sold-out badge on existing feed entries, and keep cooperator partnerships active for restock.

B: Plan: detect inventory depletion. Change listing availability to sold out. Stop active promotion and remove active feed placements where appropriate. Update visible existing entries with sold-out status. Keep cooperator discussions open for restock. Review stock consistency and buyer-facing display.

Pair 3.6 - Phrasing B

A: The translated descriptions say a listing can sell out after promotion starts.

B: Plan: feed distribution must respond to inventory state. The item is no longer actively purchasable, but existing context can remain visible as sold out and partnership work can continue toward restock.

## Pair 3.7 - Competing cooperator

A: The translated domains say a competing product should receive synergy score zero, never be shown as a partnership opportunity, but may still appear as a regular marketplace post if otherwise relevant.

B: Plan: during cooperator scoring, classify competing products as zero synergy. Exclude them from partnership cards and cooperator notifications. Keep normal marketplace relevance separate so the listing can still appear as a standard marketplace post if other rules allow. Review competitor exclusion.

Pair 3.7 - Phrasing B

A: The translated descriptions say competitors are not partners even if they are otherwise in the market.

B: Plan: partnership matching and general marketplace visibility are different paths. A competitor is blocked from partnership opportunity format while remaining eligible only for ordinary marketplace exposure.

## Pair 3.8 - Seller deactivates account

A: The translated domains say seller deactivation cancels active listings, removes them from feeds, notifies pending cooperators, and refunds pre-orders.

B: Plan: detect seller deactivation. Cancel every active listing from that seller. Remove marketplace feed entries. Notify pending cooperator discussions that the seller is inactive. Start refund handling for pre-orders. Review listing cancellation, feed removal, and buyer protection.

Pair 3.8 - Phrasing B

A: The translated descriptions say seller status can invalidate all marketplace activity.

B: Plan: seller eligibility is a continuing rule, not just a creation rule. If the seller becomes inactive, listing visibility, feed promotion, partnerships, and pre-orders must be cleaned up.

