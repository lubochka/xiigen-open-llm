# Layer 3 — Teaching Unit Pairs (009)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: teaching_unit

## TU-009 — Fabric-first pattern in B15 service design

Rule: All B15 services inject dependencies through interfaces (fabric-first), not concrete types.

Good (fabric-first):
public OrderService(IOrderRepository repository, IPaymentGatewayClient gateway)

Bad (concrete coupling):
public OrderService() { _gateway = new StripePaymentGateway(); }

Why: Fabric-first enables testing, swapping implementations, and following the Dependency Inversion Principle.

Common B15 interfaces: IOrderRepository, IPaymentGatewayClient, ICarrierClient, IEventBus, INotificationDispatcher, IRefundPolicy, ITaxRuleProvider, ICurrencyConverter

All services that require session persistence inherit BaseService(ISessionStorage storage).
