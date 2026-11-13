# Instruction-Tuning Orchestration Pilot Runbook
# Version: 1.0 - 2026-05-11

Purpose: use accepted broad instruction/chat datasets to improve XIIGen's
handling of complex orchestration prompts without turning generic chat answers
into architecture authority.

Primary source gate:
`external sources learning 2/INSTRUCTION-TUNING-SOURCE-ASSESSMENT-v1.md`

Builder:
`external sources learning 2/build_instruction_tuning_orchestration_pilot.py`

---

## Why This Exists

The current XIIGen teaching focus is orchestration of complex tasks:

- route a raw user request into code-backed predicates
- preserve multiple branches when a prompt implies multiple sub-phases
- ask for missing constraints before execution
- decompose broad tasks into phases, skills, evidence, and tests
- load governance/planning skills before implementation skills

OpenAssistant-style datasets are useful because they contain real human prompt
shapes and multi-turn ambiguity. They are not useful as source-of-truth answers
for XIIGen architecture.

Therefore the pilot uses only the user/prompter side as A-side style material.
The chosen side must be adjudicated against XIIGen contracts.

---

## Accepted First Sources

Use only:

- OpenAssistant OASST1 - Apache-2.0
- OpenAssistant OASST2 - Apache-2.0

Do not use in the first run:

- Dolly 15k - CC-BY-SA-3.0 quarantine
- Alpaca - non-commercial/research-only block
- ShareGPT - provenance/license block by default
- WizardLM/Evol-Instruct - method only unless provenance is reviewed
- UltraChat - review required
- FLAN raw rows - method-only until per-task licensing is recorded

---

## Download Inputs

When network access is available, download:

```powershell
Invoke-WebRequest `
  -Uri "https://huggingface.co/datasets/OpenAssistant/oasst1/resolve/main/2023-04-12_oasst_all.messages.jsonl.gz" `
  -OutFile "external sources learning 2/instruction-tuning-pilot/source/oasst1.messages.jsonl.gz"

Invoke-WebRequest `
  -Uri "https://huggingface.co/datasets/OpenAssistant/oasst2/resolve/main/2023-11-05_oasst2_all.messages.jsonl.gz" `
  -OutFile "external sources learning 2/instruction-tuning-pilot/source/oasst2.messages.jsonl.gz"
```

If direct download times out, download the same two files manually from the
Hugging Face dataset file browser and place them in the `source/` directory.

---

## Build Candidate Rows

```powershell
python "external sources learning 2/build_instruction_tuning_orchestration_pilot.py" `
  --input "external sources learning 2/instruction-tuning-pilot/source/oasst1.messages.jsonl.gz" `
  --input "external sources learning 2/instruction-tuning-pilot/source/oasst2.messages.jsonl.gz" `
  --output-dir "external sources learning 2/instruction-tuning-pilot" `
  --limit 64
```

Outputs:

- `oasst-orchestration-pilot-candidates.jsonl`
- `oasst-orchestration-pilot-summary.md`

All rows are `model_review_required`, not train-ready.

---

## Candidate Row Contract

Each candidate row must contain:

- `external_role: "a_side_style_only"`
- `source_license_state: "apache-2.0"`
- `source_dataset`
- `source_id`
- `pair_kind`
- `internal_contract_reference`
- `a_side`
- `chosen_candidate`
- `rejected_candidate`
- `review_instruction`
- `quality_status: "model_review_required"`

Assistant answers from OASST are not used.

---

## Model-Led Adjudication Prompt

For each candidate row:

```text
You are adjudicating an instruction-tuning prompt-surface candidate for XIIGen.

Input:
- A-side prompt from accepted external dataset.
- Candidate pair family.
- Candidate chosen template.
- Candidate rejected template.
- Internal contract reference.

Task:
1. Decide whether the A-side genuinely fits the pair family.
2. If it fits, rewrite the chosen side as a specific XIIGen orchestration answer.
3. Rewrite the rejected side as a plausible wrong answer with a specific [WRONG: ...] consequence.
4. Preserve external_role=a_side_style_only.
5. Do not use the external assistant answer as authority.
6. Reject the row if the prompt is generic, unsafe, personal, impossible to map to orchestration, or requires domain facts not present in XIIGen contracts.

Output JSON:
{
  "decision": "accept_row" | "repair_row" | "reject_row",
  "pair_kind": "...",
  "a_side": "...",
  "chosen": "...",
  "rejected": "...",
  "reason": "...",
  "internal_contract_reference": "...",
  "source_license_state": "...",
  "external_role": "a_side_style_only",
  "quality_status": "train_ready" | "repair_required" | "rejected"
}
```

---

## Target Distribution for 64 Rows

- 20 IT-ORCH-01 root intent to routing compiler
- 16 IT-ORCH-02 multi-sub-phase routing
- 12 IT-ORCH-03 clarification / knowledge-edge
- 10 IT-ORCH-04 complex task decomposition
- 6 IT-ORCH-05 skill activation / load order

The deterministic builder only approximates this distribution. The model-led
adjudication step is responsible for accepting, repairing, or rejecting rows.

---

## Acceptance Gates Before Training

The adjudicated file can enter standalone ablation only if:

- no blocked source rows
- no ShareGPT/WizardLM/UltraChat/Alpaca/Dolly rows in train-ready
- `chosen != rejected`
- no mojibake in A/chosen/rejected
- every rejected side has a specific `[WRONG: ...]` consequence
- every chosen side cites a code-backed/internal contract
- no external assistant answer is copied as chosen
- at least 16 and at most 64 rows in the first ablation

After standalone ablation:

- positive reward margin required
- combo gate against V246 required
- reject the batch if V246 accuracy or margin regresses
