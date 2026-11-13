# C2 — Local LLMs (llama.cpp / Ollama / Open Interpreter)

**Trend.** There is strong demand for running LLMs locally without the cloud, through projects such as llama.cpp, Ollama, and Open Interpreter.

**How this project relates.** LlmFromScratch is a from-scratch local model with a real save / load / export / import lifecycle, so a trained model can be persisted, reloaded, exported, imported, and continued locally.

**Where in the code.** `27.TranslationDomain/TranslationMachine.cs:12` is `TranslationMachine : ILearningMachine<TranslationDomain>` with a real train / evaluate / export / import (and continue-training) lifecycle; `25.ModelServer/Contracts/IModelRepository.cs:11` (`LoadWeightsAsync`) is the local load path; and the `27.TranslationDomain.Tools` asset-lifecycle CLI plus the `models/` registry are where checkpoints live.

**Orchestration.** `TranslationMachine` sits at hierarchy level 5 (`ILearningMachine`, declared in `00.Core/LearningMachineContracts.cs`) and exposes its runtime coordinator through the contract (`GetOrchestratorAsync`); accepted checkpoints are published and re-loaded through `25.ModelServer` (`IModelRepository`).

**Honesty boundary.** This is a real local model lifecycle — a genuinely trained `ILearningMachine` with checkpoint save / load / export / import — but it is a from-scratch educational / research stack, NOT a production-grade local inference runtime at the scale of llama.cpp or Ollama. The point of comparison is the local-first principle and a working checkpoint lifecycle, not equivalent inference performance or model size.
