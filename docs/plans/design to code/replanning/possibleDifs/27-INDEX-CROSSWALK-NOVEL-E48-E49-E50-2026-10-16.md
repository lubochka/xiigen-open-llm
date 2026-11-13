# N27 — независимая сверка E48/E49/E50 c RP01 и карта причинных следующих шагов

**2026-10-16 | SOURCE-BASED INVESTIGATION | NO_NATIVE_LEARNING_RUN | NO_INDEPENDENT_CODEX_ASTRA_REVIEW.** Дополнение к 148 уже существовавшим карточкам possibleDifs, а не замена или переименованная версия DIF-01…44. Core docs/plans/replanning-possible-diffs строго READ ONLY.

## Ссылки

- [N27-A: E48 низкобюджетный warm-start + отсутствие настоящего сохранённого transfer](27-NEW-E48-MISREAD-WARM-START-AND-MISSING-CHECKPOINT-2026-10-16.md)
- [N27-B: E50 premature closure/false treatment contrast + E49 consumer divergence](27-NEW-E50-PREMATURE-GRAPH-CLOSURE-FALSE-PASS-2026-10-16.md)
- [MICRO-N27: 3-arm E48 и арифметический graph shadow E50](27-MICRO-LOW-BUDGET-TRANSFER-AND-GRAPH-CLOSURE-2026-10-16.md)

## Матрица причин, не смешивать разные обучающиеся объекты

| Класс | Код/архив | Уже известное | Новый вклад N27 / следующий опыт |
| --- | --- | --- | --- |
| База/optimizer/LoRA | [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) | B0 random, rank8 q/v only; 50 updates/домен; free C#0/3 и ENRU0/4 | проверить настоящую семантическую полезность *на ранней точке* после отдельного persistent warm-state |
| Dataset/DPO | [DIF register](01-PRIORITIZED-DIFFERENCES.md) | 500 строк ≠ 500 independent families; prompt/admission/text masks; simulated teacher | E48 не заменяет DPO validation, с NEXT experiment не смешивать authentic teacher quality |
| KNN/semantic features | [N26 full lineage](26-INDEPENDENT-FULL-GAP-REGISTER-AND-HISTORICAL-LEARNING-LINEAGE-2026-10-16.md) | matrix→KNN donor lineage, kNN first-hit, false rank/normalization, rule tags | E49 не доказано проходит из Jaccard profiles в production transition consumer; нужен вызов и comparison |
| Units/Flow/Graph | [E49 source](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E49OrchestratorExpander.cs) / [current graph](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/17.DynamicDomains/DynamicDomainGraph.cs) | важность typed Unit и actual consumer | синтетическое экспериментальное domain inference заменено reachability, и E50 не сообщает устойчивую learning convergence |
| State/checkpoints | [E48 source](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/Experiments/E48DomainBootstrapper.cs) | checkpoint bit-preservation vs real adopted behavior | source pretrain object не участвовал в enhanced arm — нельзя ссылаться на E48 как доказательство исправного restore |
| Метрика/научная проверка | [E48 архив](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E48-DOMAIN-BOOTSTRAPPER/results/run-2026-05-01-001.json) / [E50 архив](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/35.ExperimentsV2/results/E50-DOMAIN-GRAPH-CLOSURE/results/run-2026-05-01-001.json) | не смешивать relative improvements, heldout correctness и optimizer activity | первый опыт показывает эффект 14%→34% при N56, теряемый на финальной метрике; второй создал 75.22% из avg vs final одной траектории |

## Priority и явная область причинности

**P0 — не ждать E48/E50 перед диагностикой основного RP01:** проверять random-B0 prerequisite, DPO semantic validity, numerical gradient/LoRA, exact tokenizer/stop-token и zero-valued learned quality; сравнивать на реальном Student и ordinary consumer. Уже подробно покрыто [existing master synthesis](00-MASTER-SYNTHESIS-AND-CONFLICTS.md), [DIF01-44](01-PRIORITIZED-DIFFERENCES.md), [RP01 recovery](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md).

**P1 — N27-A:** быстро проверить реальный state transfer/low-budget learning, потому что он имеет конкретный исторический conditional advantage, который финальный FAIL скрыл. Успех по E48 semantic heldout не гарантирует улучшение LoRA Student; нужен одинаковый consumer.

**P2 — N27-B/E49:** сначала статически опровергнуть claims успешной graph closure; только если найден actual ordinary caller, проводить quality A/B с profile donor. **Не заставлять все 500 DPO пройти через graph closure, если code path отсутствует.**

## RAG / соседние ветки / источники

У проекта несколько сохранённых видов обучения. Branch archaeology следует продолжать по [11-BRANCH-ARCHAEOLOGY](11-BRANCH-ARCHAEOLOGY-AND-LEARNING-PROOFS.md), [12-MATRIX-TO-KNN](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md), [26-FULL-LINEAGE](26-INDEPENDENT-FULL-GAP-REGISTER-AND-HISTORICAL-LEARNING-LINEAGE-2026-10-16.md) с обязательной цепью OLD formula/experiment → exact Git migration → NEW callable function → ordinary consumer → SAME evaluation families. Исследование E48/E50 НЕ объявляет найденным «тот самый» перенесённый matrix→KNN experiment: точный donor всё ещё OPEN. Проверка соседних DesignToCode/external_domains/language_translate/orchestrator-extensions нужна для расширения coverage, но N27 конкретно опирается на сохранённые E48–E50 sources текущей core ветки, не на утверждение о diff соседних branches.

## Independent review / human & teacher contract

CODEX на Astra 6 UltraCode выполняет роль человека: выбирает обучающие случаи и независимо оценивает их качество. **Внешняя открытая китайская модель создаёт обе стороны chosen/rejected для реального DPO**; нынешняя RP01 simulation явно использует иной simulated/deterministic source, это не реализация Chinese teacher. CODEX отдельно оценивает собственные Student outputs на новых скрытых задачах. Ни E48, ни E50 arithmetic или документ не создают DPO и не заменяют эти вызовы.

**Verification-before-completion:** Markdown source links pinned blob identities, три new docs readback после Git publication, 0 изменений core repo, native executed=false, no uplift claim. Для эксперимента требуется отдельное одобрение запуска от владельца, архивы не переписывать.
