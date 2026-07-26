# Is LlmFromScratch a local LLM like llama.cpp or Ollama?

LlmFromScratch is a from-scratch local model with a genuinely trained `ILearningMachine` and a real save/load/export/import checkpoint lifecycle, so a model can be persisted and continued locally. It is a from-scratch educational and research stack, NOT a production-grade local inference runtime at the scale of llama.cpp or Ollama — the comparison is the local-first principle, not inference performance.

LlmFromScratch is a from-scratch local model with a real save/load/export/import lifecycle, so a trained model can be persisted, reloaded, exported, imported, and continued locally. `27.TranslationDomain/TranslationMachine.cs:12` is `TranslationMachine : ILearningMachine<TranslationDomain>` with a real train/evaluate/export/import (and continue-training) lifecycle; `25.ModelServer/Contracts/IModelRepository.cs:11` (`LoadWeightsAsync`) is the local load path; and the `27.TranslationDomain.Tools` asset-lifecycle CLI plus the `models/` registry are where checkpoints live. A trained checkpoint is published and re-loaded through `25.ModelServer` (`IModelRepository`), so the same model can be saved once and reloaded across runs without any network call. The honest boundary: this is a real local model lifecycle — a genuinely trained `ILearningMachine` with checkpoint save/load/export/import — but it is a from-scratch educational and research stack, NOT a production-grade local inference runtime at the scale of llama.cpp or Ollama. The point of comparison is the local-first principle and a working checkpoint lifecycle, not equivalent inference performance or model size.

### What is the industry trend?

There is strong demand for running LLMs locally without the cloud, through projects such as llama.cpp, Ollama, and Open Interpreter.

### Where is this in the code?

`27.TranslationDomain/TranslationMachine.cs:12` is `TranslationMachine : ILearningMachine<TranslationDomain>` with a real train / evaluate / export / import (and continue-training) lifecycle; `25.ModelServer/Contracts/IModelRepository.cs:11` (`LoadWeightsAsync`) is the local load path; and the `27.TranslationDomain.Tools` asset-lifecycle CLI plus the `models/` registry are where checkpoints live.

`TranslationMachine` sits at hierarchy level 5 (`ILearningMachine`, declared in `00.Core/LearningMachineContracts.cs`) and exposes its runtime coordinator through the contract (`GetOrchestratorAsync`); accepted checkpoints are published and re-loaded through `25.ModelServer` (`IModelRepository`).

### Honesty boundary

This is a real local model lifecycle — a genuinely trained `ILearningMachine` with checkpoint save / load / export / import — but it is a from-scratch educational / research stack, NOT a production-grade local inference runtime at the scale of llama.cpp or Ollama. The point of comparison is the local-first principle and a working checkpoint lifecycle, not equivalent inference performance or model size.

Last updated: 2026-07-24.
