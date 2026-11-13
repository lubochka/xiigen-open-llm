# Layer 0-A — Compiler Pairs (022)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-022-001 — ARCHITECTURE+SECURITY: VPC topology

A: "Design the VPC (Virtual Private Cloud) network topology — public and private subnet boundaries, NAT (Network Address Translation) gateway placement, and inter-VPC peering."
  "design" → ARCHITECTURE | "VPC network topology" → ARCHITECTURE ("topology", "network") | "subnet boundaries" → ARCHITECTURE ("boundary") | "NAT gateway placement" → ARCHITECTURE ("deployment")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "VPC", "NAT", "VPC peering" → platform/network vocabulary, gap

---

## B14-compiler-022-002 — ARCHITECTURE+SECURITY: zero-trust network

A: "Architect the zero-trust network topology — deny-all NetworkPolicy baseline, PrivateLink service boundaries, and mTLS (mutual TLS) enforcement."
  "architect" → ARCHITECTURE | "zero-trust network topology" → ARCHITECTURE ("topology", "network") + SECURITY ("zero-trust") | "PrivateLink service boundaries" → ARCHITECTURE ("boundary") | "mTLS enforcement" → SECURITY ("tls")
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY"]
  Gap: "PrivateLink", "NetworkPolicy" → platform vocabulary, gap

---

## B14-compiler-022-003 — ARCHITECTURE+IMPLEMENT+SECURITY: VPC security config

A: "Design the VPC security topology and implement the security group rules and TLS (Transport Layer Security) certificate configuration."
  "design" → ARCHITECTURE | "VPC security topology" → ARCHITECTURE + SECURITY | "implement" → IMPLEMENT | "security group rules" → SECURITY | "TLS certificate configuration" → SECURITY ("tls", "certificate")
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "SECURITY"]
  Gap: "VPC", "security group rules" → platform vocabulary, gap

---

## B14-compiler-022-004 — ARCHITECTURE+DOCUMENT: network architecture ADR

A: "Define the network isolation architecture and document the egress control decisions."
  "define" → ARCHITECTURE | "network isolation architecture" → ARCHITECTURE ("architecture", "network") | "document" → DOCUMENT | "egress control decisions" → ARCHITECTURE + SECURITY
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT", "SECURITY"]
  Gap: "egress control" → network vocabulary, gap

---

## B14-compiler-022-005 — gap: "open port 443 on the security group"

A: "Open port 443 on the production security group."
  "open" → gap (operational firewall action) | "security group" → gap | "port 443" → gap
  CompiledPredicateIds: []
  Teaching: "open port" is operational gap. To fire SECURITY: "implement the TLS ingress security configuration for the network layer."
