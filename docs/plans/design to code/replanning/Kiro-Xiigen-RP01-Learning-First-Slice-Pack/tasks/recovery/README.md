# RP01 REC01–REC05 · Быстрый recovery после отрицательных C#500 + ENRU500

**16.10.2026 · CURRENT START INDEX · план, не новый тренировочный результат.** [Evidence-based audit](../../RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) → [текущий RP01 dispatcher](../../RP01-EXECUTE-NOW.md). Задачи совместимы с RP02–RP05. **Исторический BATCH500** (500 pairs per domain, 50 updates per campaign, no functional gain) завершён как отрицательный научный результат и не должен быть повторён; его старые карточки и экспериментальные протоколы остаются evidence-only.

| Карточка (полный самостоятельный промпт) | Самый ранний полезный результат | Можно параллельно? |
| --- | --- | --- |
| [REC01 — evidence + provenance](RP01.REC01-EVIDENCE.md) | Слой failure, exact evidence и один falsifier; 0 training calls | Да, read-only |
| [REC02 — objective + gradient + tiny overfit](RP01.REC02-DPO-GRADIENT.md) | Small TRAIN preference learning and loaded greedy decision or bounded failure; instrumented 8–32 pairs | Да при единственном владельце Student |
| [REC03 — cold-start / tokenizer / SFT](RP01.REC03-CAPABILITY.md) | Measured capacity prerequisite and fair tiny SFT/DPO contrast | Read-only/tokenization параллельно; weight updates serialized |
| [REC04 — hybrid domain/kNN consumer](RP01.REC04-HYBRID.md) | One law-specific consumer-visible precision/coverage tradeoff and RP02 handoff | Да, отдельный RP02 state owner |
| [REC05 — prospective multic-domain learning](RP01.REC05-LEARNING-LOOP.md) | Independent domain-law learning curve, time/RAM, verdict or bounded invalidation | Только после REC02/03 causal signal |

**Dispatch:** Начинать с REC02 tiny learning diagnostic параллельно с REC01 evidence; REC03 latency/tokenizer read-only и REC04 real consumer read-only могут идти одновременно. **Не ждать REC01 full completion, чтобы провести REC02**; если tiny mathematical/optimizer gradient sanity fails, остановить новые big batches и чинить seam, не объявлять learning completed. Training and checkpoint single-writer; separate owner/provenance for Astra development synthetic pairs and independent review; CODEX выполняет human choice и качество DPO на Astra 6 UltraCode. Actual model claim — только runtime receipt.

**Критерий:** Runnable changed mechanism + before/after effect + independent semantic domain-law observation + full cost; `0/3` и `0/4` не положительный outcome. No test-coverage quota or Windows CI. Limited deterministic checks при новых code risks сохранять; old frozen campaign evidence не трогать. `+2/12` не переносят. Recovery packages являются `DRAFT_PENDING_INDEPENDENT_ASTRA_REVIEW`, а не заявкой об исполненной проверке модели.

Результат вернуть в существующий RP01 owner/journal с exact repository HEAD, source paths, CODEX/Astra attestation, changed parameters, train pairs and semantic families, true updates, fresh ordinary Student/consumer behavior, stage costs, unresolved assumptions, next action, GitHub publication proof.
