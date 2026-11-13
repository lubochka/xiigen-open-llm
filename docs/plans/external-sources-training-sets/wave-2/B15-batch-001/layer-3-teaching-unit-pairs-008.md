# Layer 3 — Teaching Unit Pairs (008)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: teaching_unit

## TU-008 — OperationResult<T> in B15 service implementation

Rule: Every public method in every B15 service returns OperationResult<T>.

Never: throw exceptions for business logic
Never: return null
Never: return bool without context

In order processing:
- Order creation: OperationResult<OrderId>.Ok(orderId) or Fail("ORDER_VALIDATION_FAILED")
- Status update: OperationResult<OrderStatus>.Ok(status) or Fail("ORDER_NOT_FOUND")
- Payment processing: OperationResult<PaymentReceipt>.Ok(receipt) or Fail("PAYMENT_GATEWAY_ERROR")

The OperationResult pattern allows callers to handle failures without try/catch chains and without null checks.
