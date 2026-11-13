# Node-07 - Arbiter Selector Set

## Purpose

Runtime A is the full collection of formal results R1...Rn plus flow context.
Runtime B is the selected review units.

## Pair 7.1 - Listing creation

A: Formal results show a new marketplace item and active listing.

B: Select seller-verification arbiter, required-listing-fields arbiter, fake-listing arbiter, duplicate-listing arbiter, and listing-status arbiter.

Pair 7.1 - Phrasing B

A: Formal results include seller identity, item details, pricing, inventory, and publication status.

B: Select seller eligibility review, item completeness review, duplicate-product review, and publication-state review.

## Pair 7.2 - Pricing and discounts

A: Formal results show pricing tiers, friend discounts, group discounts, cooperator bundle pricing, or price changes.

B: Select pricing-audit arbiter, discount-range arbiter, audience-specific-price arbiter, and price-change-history arbiter.

Pair 7.2 - Phrasing B

A: Formal results include commercial prices that vary by audience or quantity.

B: Select price transparency review, discount correctness review, and pricing-change review.

## Pair 7.3 - Inventory and availability

A: Formal results show stock counts, pre-order status, sold-out status, or inventory changes during promotion.

B: Select inventory-consistency arbiter, availability-display arbiter, sold-out-feed arbiter, and purchase-readiness arbiter.

Pair 7.3 - Phrasing B

A: Formal results include stock-sensitive marketplace visibility.

B: Select stock validation review, feed-availability review, and buyer-facing-state review.

## Pair 7.4 - Cooperator matching

A: Formal results show synergy scores, cooperation types, complementary products, competitors, or partnership notifications.

B: Select synergy-weight arbiter, competitor-exclusion arbiter, cooperation-threshold arbiter, and cooperator-spam arbiter.

Pair 7.4 - Phrasing B

A: Formal results include partnership opportunities from cooperator scoring.

B: Select partnership-fit review, score-factor review, competitor review, and notification-rate review.

## Pair 7.5 - Feed distribution

A: Formal results show friend, group, and cooperator feed cards.

B: Select card-format arbiter, audience-route arbiter, offer-display arbiter, and marketplace-enabled-group arbiter.

Pair 7.5 - Phrasing B

A: Formal results include multi-format feed placement for marketplace content.

B: Select feed-card correctness review, audience-specific formatting review, and group-permission review.

## Pair 7.6 - Limited-time, pre-order, service, or wholesale

A: Formal results include expiry, countdown, coming-soon state, service booking, or volume pricing.

B: Select special-listing-state arbiter, expiry-removal arbiter, availability-truth arbiter, and pricing-display arbiter.

Pair 7.6 - Phrasing B

A: Formal results show a listing that is not a normal in-stock product.

B: Select listing-type review, time-state review, stock-state review, and card-variant review.

## Pair 7.7 - Consumer and transaction protection

A: Formal results show transaction terms, return policy, tax transparency, pricing, or buyer-facing purchase readiness.

B: Select consumer-terms arbiter, return-policy arbiter, tax-transparency arbiter, and transaction-scope arbiter.

Pair 7.7 - Phrasing B

A: Formal results include commercial buying context.

B: Select buyer-protection review, marketplace-compliance review, and transaction-data-boundary review.

## Pair 7.8 - Analytics and seller intelligence

A: Formal results show marketplace metrics and seller suggestions.

B: Select analytics-coverage arbiter, seller-suggestion arbiter, sensitive-business-data arbiter, and search-throttling arbiter.

Pair 7.8 - Phrasing B

A: Formal results include performance measurement, trends, and marketplace intelligence.

B: Select metrics completeness review, seller-advice review, confidential-data review, and scraping-protection review.

