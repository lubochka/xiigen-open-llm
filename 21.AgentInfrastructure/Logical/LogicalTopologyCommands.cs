// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

// This base command represents one requested topology mutation.
public abstract record LogicalTopologyCommand(string Reason);

// This command asks the topology registry to add or update one logical domain.
public sealed record AddDomainCommand(
    LogicalDomain Domain,
    string Reason)
    : LogicalTopologyCommand(Reason);

// This command asks the topology registry to add or update one logical phase.
public sealed record AddPhaseCommand(
    LogicalPhase Phase,
    string Reason)
    : LogicalTopologyCommand(Reason);

// This command asks the topology registry to add or update one logical sub-phase.
public sealed record AddSubPhaseCommand(
    LogicalSubPhase SubPhase,
    string Reason)
    : LogicalTopologyCommand(Reason);

// This command asks the topology registry to add or update one logical transition.
public sealed record AddTransitionCommand(
    LogicalTransition Transition,
    string Reason)
    : LogicalTopologyCommand(Reason);
