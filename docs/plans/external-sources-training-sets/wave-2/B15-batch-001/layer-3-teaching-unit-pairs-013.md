# Layer 3 — Teaching Unit Pairs (013)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: teaching_unit

## TU-013-A — OrderFulfillmentService: does "dispatch" fire API_CONTRACT?

A: "Build the order fulfillment service that dispatches orders to the carrier." Does API_CONTRACT fire?
B: No. "dispatch" alone does not fire API_CONTRACT. API_CONTRACT fires on: integrate, wire up, connect (to external system), or "contract" keyword. "Dispatch" is an action verb within IMPLEMENT scope. Only ["IMPLEMENT"] fires.

## TU-013-B — OrderFulfillmentService: does "carrier" fire API_CONTRACT?

A: "Build the order fulfillment service for carrier dispatch." Does "carrier" fire API_CONTRACT?
B: No. "carrier" is a domain noun; it does not fire API_CONTRACT. API_CONTRACT fires on verbs (integrate/wire up/connect) or the keyword "contract". Domain nouns referencing external parties ("carrier", "vendor", "gateway") do not independently fire API_CONTRACT.

## TU-013-C — OrderFulfillmentService: when does API_CONTRACT fire for carrier integration?

A: "Build the order fulfillment service and integrate with the carrier dispatch API."
B: API_CONTRACT fires on "integrate". The routing produces two decisions: ImplementationFlow41 (IMPLEMENT) + APIContractFlow33 (API_CONTRACT). The word "API" in "carrier dispatch API" reinforces the API_CONTRACT context but "integrate" is the trigger.

## TU-013-D — Fabric-first in FulfillmentService: ICarrierClient

A: OrderFulfillmentService needs to call the carrier. Should it use new CarrierApiClient() or inject ICarrierClient?
B: Inject ICarrierClient via constructor (fabric-first). Rule 2: external dependencies resolve through interfaces. Never new-up injectable dependencies inside a service class. OperationResult<T> is returned from all public methods (Rule 1).

## TU-013-E — OrderFulfillmentService: OperationResult<T> on what methods?

A: Which methods of OrderFulfillmentService must return OperationResult<T>?
B: All public methods. Rule 1 is absolute: every public method returns OperationResult<T>. Private methods may return raw types. The fulfillment service public surface: StartFulfillment, TrackDispatch, CancelDispatch — all return OperationResult<T>.
