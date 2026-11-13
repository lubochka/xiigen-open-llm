# Layer 2 — Nine-Node Flow Pairs (020)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: nine_node_flow (ImplementationFlow41 + SecurityAuditFlow)

## B15-nine-020 — AuditService (IMPLEMENT+SECURITY)

Request: "Build the audit service and secure the audit log against tampering using append-only storage and HMAC (Hash-based Message Authentication Code) signatures."

### Two flows: SecurityAuditFlow then ImplementationFlow41

**Node 1 — DomainSplitterUnit**: Target: AuditService (feature_addition) | A = SelectedLogicalTransition: TransitionType: feature_addition, SubPhaseIds: ["security", "4.1"], PredicateIds: ["IMPLEMENT", "SECURITY"] | SecurityAuditFlow runs first to define tamper-protection constraints; ImplementationFlow41 implements within those constraints.

**Node 2 — DomainTranslatorUnit (SECURITY)**: Gate B: security constraints for audit log: (1) Append-only storage — no update or delete operations on audit records; (2) HMAC signatures — each audit entry signed with HMAC-SHA256 using service-level secret key; (3) Signature verification — on log read, verify HMAC before returning record. Output: SecurityConstraintSpec.

**Node 3 — SchemePlannerUnit (SECURITY → IMPLEMENT)**: probationary_accepted — security constraints approved; IAuditRepository (append-only) interface defined; IHmacSigner interface defined.

**Node 4 — DomainProcessorUnit (IMPLEMENT)**: WorkBranches:
- AuditWriter: IAuditService.WriteEntry(AuditEntry) → OperationResult<AuditEntryId>; BR-AUDIT-001: entry must include EntityType, EntityId, OperationType, ActorId, Timestamp; BR-AUDIT-002: HMAC signature computed and stored with entry.
- AuditReader: IAuditService.GetEntries(AuditQuery) → OperationResult<AuditEntry[]>; BR-AUDIT-003: HMAC verified on read; tampered entries rejected with error code AUDIT_TAMPERED.
- AuditRepository: append-only IAuditRepository; no Update or Delete methods exposed.

**Node 5 — ApplierUnit**: Files generated: IAuditService.cs, AuditService.cs, IAuditRepository.cs, AuditRepository.cs (append-only), IHmacSigner.cs, HmacSigner.cs, AuditEntry.cs, AuditQuery.cs.

**Node 6 — ResultTranslatorUnit**: 8 files, 4 interfaces, 3 business rules, security constraint: append-only + HMAC. Test surface: 10 recommended cases (write + verify, tamper detection, query filtering).

**Node 7 — ArbiterSelectorUnit**: Arbiter: security-audit-arbiter then implementation-completeness-arbiter.

**Node 8 — ArbiterUnit**: Security verdict: PASS (append-only enforced at repository level; HMAC-SHA256 signing confirmed). Implementation verdict: PASS (IHmacSigner injected; fabric-first; OperationResult<T> confirmed).

**Node 9 — FormatterUnit**: Verdict: IMPLEMENT_APPROVED (IMPLEMENT+SECURITY) | TransitionType: feature_addition | Files: 8 | SecurityConstraints: append-only + HMAC | TestSurface: 10 cases
