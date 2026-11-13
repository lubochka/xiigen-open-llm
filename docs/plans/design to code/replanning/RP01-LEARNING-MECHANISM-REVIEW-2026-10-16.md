# RP01 · проверка DPO/LoRA/kNN по опубликованным артефактам (16.10.2026)

**Статус: SOURCE_AND_ARTIFACT_REVIEW; no new native experiment; DRAFT independent Astra UltraCode plan review.**

Актуальные материалы опубликованы **не в новой параллельной очереди**, а в исправленном действующем RP01:
- [Полный научно-технический аудит, риски, архитектура, 7 предметных доменов и точные источники](Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md)
- [Текущий RP01 стартовый промпт, supersedes issued Main500](Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-EXECUTE-NOW.md)
- [Пять самостоятельных детальных задач REC01–REC05](Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/recovery/README.md)
- [Действующая README RP01](Kiro-Xiigen-RP01-Learning-First-Slice-Pack/README.md)
- [Старые B500 tasks как immutable исследовательский протокол](Kiro-Xiigen-RP01-Learning-First-Slice-Pack/tasks/batch500/README.md)

## Краткий итог

1. **DPO/LoRA: реальное изменение состояния работает**, 16 адаптеров меняются, 39 базовых тензоров/замороженный reference не меняются, checkpoint reopens. Forward даёт изменение вероятностей, но исходный B0 **random untrained**, с 629-токенным C# vocab и LoRA только q/v. На этих фактических 500×2 экспериментах полезного свободного навыка не обнаружено (C# 0/3→0/3, ENRU 0/4→0/4). Причина пока не изолирована.
2. **Simulation DPO quality:** core C#500 structurally 500 unique triples из 25 semantic families по20; все 500 `csharp_runtime_verified=false`. ENRU500 structurally 500 unique triples, 20 categories×25; independent single CODEX/Astra-like review packet states 500/500 accepted and 0 ambiguous, но независимость semantic families UNKNOWN. Данные — `simulation`, не Qwen. Offline importer всегда маркирует side author `DeterministicGenerator`, независимо от способа создания Astra симуляции: attribution needs reconciliation.
3. **DPO objective по форме близка к корректной:** masked response SUM probabilities, frozen reference, β0.1, signs `oneHot-softmax` and AdamW 1e-5; новый tiny numerical directional gradient/8–32 TRAIN overfit всё ещё требуется. Действующий offline CLI разрешает 1 calibration или 500 main, **не** произвольные 8–32: REC02 отдельно требует самый маленький безопасный diagnostic admission.
4. **Semantic learning/kNN:** `KnnLogicalUnit` использует unordered hashed BPE/predicate counts, `Search(query,_k)`, выбирает `hits[0]`; dynamic semantic k/learned reranker этим не доказаны. Отдельный translation law-guard до ограничения кандидатов улучшает на 6 открытых кейсах precision, но снижает coverage; это deterministic hybrid, не DPO learning. `DpoTrainer.VectorSignal` использует только **количество активных slots**, не смысл координат; feature→LoRA semantic transposition пока не реализована именно в этом маршруте.
5. **Speed/measurement:** ENRU native training ~2330 секунд и evaluation ~183 секунд, process peak ~2.43GB; новые phase timers появились позднее; SIMD MatMul microbenchmark не означает whole-PC gain. Ни full Windows/test coverage, ни завышенный `+2/12` не нужны для следующего научного шага.

**Next executable owner:** REC02 tiny response-mask/gradient/LoRA diagnostic с небольшой отдельной accepted simulation DPO; одновременно read-only REC01/REC03 и RP02 law-contrasts. При непрохождении tiny gradient stop big training; при работающем preference signal, но incoherent free output, сравнить supervised competence warm-up vs DPO-only before a new staged500. Не повторять замороженные старые опыты.

**Source scope:** GitHub `llm_mvp_core/codex/dpo-implementation` (reviewed at `83590a462b2f8aedc9d3ab4cb775193eb9c25ce5`); `xiigen-planning/llm_plan3` 25 R rounds, `previous rounds thinking results.md`, WorkLeft/RAG and historical RP01. Текущая GitHub публикация — **план и кодовый аудит, без выполнения нового Student training или независимого Astra scientific rerun**.
