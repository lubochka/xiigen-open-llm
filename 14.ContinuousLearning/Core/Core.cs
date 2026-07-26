// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

// ─── Training example ─────────────────────────────────────────────────────────
// One labelled sample stored in the domain dataset cache.
// weight: dynamic — computed by FeedbackRouter.ComputeMaxWeight (D-09, never hardcoded ×10).
public sealed record TrainingExample(
    string         Prompt,       // original user prompt or symbol identifier
    string         Response,     // model's response at capture time
    string         Domain,       // canonical D-04: CODE_Q / BUG / PLAN / ARCH / REFACTOR
    double         Weight,       // importance weight — 1.0 = standard, higher = rare domain
    DateTimeOffset CapturedAt);

// ─── DPO pair ─────────────────────────────────────────────────────────────────
// Chosen/rejected pair produced by FeedbackRouter for DPO fine-tuning.
// Created when user edits (Chosen=correction) or rejects (Chosen="[no preferred response]").
public sealed record DpoPair(
    string         Prompt,
    string         Chosen,       // preferred response — user's edit or placeholder
    string         Rejected,     // the model response the user did not accept
    string         Domain,
    DateTimeOffset CapturedAt);

// ─── Feedback signal ──────────────────────────────────────────────────────────
// Three signals a user can send about a model response.
public enum FeedbackSignal { Accepted, Edited, Rejected }

// ─── Feedback event ───────────────────────────────────────────────────────────
// Context needed by FeedbackRouter to convert one signal into a training record.
public sealed record FeedbackEvent(
    string         Prompt,
    string         ModelResponse,    // what the model produced
    string?        UserCorrection,   // non-null when Signal == Edited
    FeedbackSignal Signal,
    string         Domain);

// ─── Model adapter ────────────────────────────────────────────────────────────
// Represents one LoRA adapter file saved by IncrementalTrainer (~2 MB per adapter).
public sealed record ModelAdapter(
    string         Domain,            // which domain this adapter specialises for
    string         AdapterPath,       // relative path to the .lora file
    int            TrainingExamples,  // how many examples produced this adapter
    DateTimeOffset TrainedAt);
