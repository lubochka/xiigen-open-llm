# N44 — Короткие причинные эксперименты: восстановление исторического качества и falsifiers

**Дата:** 2026-10-19. **Режим:** независимый GitHub SOURCE/RECEIPT AUDIT + DESIGN_ONLY для новых опытов. **Срезы:** `language_translate@f6032aff81f1f83de7c09cf9b9efe6b058dd12b9`, `PrepareToGPL@31346d8928be4469b01aaf313780e8f950ff91e2`, `codex/dpo-implementation@a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6`. **Целевая ветка документации:** `xiigen-planning@llm_plan3`. **Запрещено изменять:** `llm_mvp_core/docs/plans/replanning-possible-diffs/**`.

**Статус всех ниже:** `DESIGNED_NOT_RUN`. В этом исследовании запускались только GitHub source/receipt reads. Результаты 18.10 из core — **исторические исполненные** native runs и не должны повторно маркироваться как наши.

## Исходное правило и критерии

Единица учёта: `case_id, domain, exact prompt, source-language, target-language, pair ID, learned recipient, before state hash, after state hash, cold restore hash, ordinary output, verdict, latency, memory`. Для каждого заявленного эффекта: контроль A, вмешательство B, **один** factor, контрконтроль C; заранее фиксируются eval cases, labels, rubric, seeds, candidate pool и бюджет. Heldout не должен иметь дубликатов/семейных переносов train и не должен служить source обучения. Токенизатор, BPE identity, DPO-маска, LoRA baseline и optimizer не должны меняться в опыте по R1/R2. Если train не происходит, сказать «улучшение маршрутизации/потребления», а не «новая нейронная способность».

### Micro-A — R1 source-language contract, 18 синтетических запросов

**Вопрос:** доходит ли реальный raw-text запрос до обученного переводческого consumer без заранее известного исходного языка? **Источники:** [old R1](https://github.com/lubochka/llm_mvp_core/blob/language_translate/21.AgentInfrastructure/Logical/TranslationRoutingFlow.cs#L21-L104) vs [new ResolveDirection](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationRoutingFlow.cs#L133-L157). **Вход:** [18 hand-authored eval-only cases](44-ROUTING-CAUSAL-FIXTURE-2026-10-19.json), 6 RU→EN, 6 HE→EN, 6 EN→RU, medical/legal/technical/cultural/personal/general. Сведения `gold_source_lang` хранятся у evaluator, **не в raw-text подсказке**.

**Arms:** A — актуальный реальный host с заполненными `SourceLanguage/TopicDomain`; B — реальный host с `SourceLanguage` отсутствующим или пустым, **без нового classifier**; C — тот же B, но с **неавторитетной** старой R1 script-evidence helper, который предлагает SourceLanguage до current Flow. Все arms используют *один и тот же* сохранённый trained translation Unit/checkpoint, если он реально доступен. Если без исходного языка пользовательский host не принимает такой запрос — записать `caller_rejects_missing_language`, это и есть измеряемый boundary.

**Метрики:** delivery-to-trained-recipient rate, правильное направление, `SOURCE_LANGUAGE_REQUIRED` rate, quality только для actually translated outputs, abstain/hallucinations, p50 wall ms. **Условие причинности:** C лучше B по *обычному ответу* на ≥1 новом случае при том же learned recipient, не ухудшая язык на отрицательных контролях; отдельно указать числитель/знаменатель 18. **Фальсификатор:** все 18 cases всегда приходят в host с correct SourceLanguage; B не является поддерживаемым user path; или C меняет route, но не ответ/качество. **Риск:** mixed-script «The Russian Federation» и имена на латинице допускают ошибку script heuristic; от таких эффектов нужна явная неопределённость/отказ.

### Micro-B — R2 TopicDomain contribution

**Вопрос:** меняет ли contextual topic реальный **learned** outcome, а не просто поле в metrics? **Источники:** [old R2](https://github.com/lubochka/llm_mvp_core/blob/language_translate/21.AgentInfrastructure/Logical/TranslationTopicRoutingFlow.cs#L209-L260), [new Flow metric pass-through](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationRoutingFlow.cs#L99-L114), [current Executor Unit](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L264-L293).

**Arms:** A — same question/current caller `TopicDomain=general`; B — gold human domain `medical/legal/technical`; C — old R2 proposed topic, не использовать его детерминированный ответ как перевод. Первая фаза **без обучения**: trace target Unit, lookup ranks, selected pair IDs and ordinary answer; вторая только если topic реально меняет selection — 3–6 verified *same prompt, different legitimate context* DPO pairs on learned translator's own store, then cold ordinary eval. **Метрики:** source-domain confusion, sensitive-sense accuracy, negation/role preservation, wrong-domain contamination; отдельная проверка с domain-words только в цитате. **Фальсификатор:** all A/B/C identical selected learned state & responses — R2 restoration doesn't explain poor translation. **Область:** EN/RU specialized language; HE support только при реально существующем trained store; не переносить на AST-код.

### Micro-C — Rejected-adapter hard-negative provenance

**Вопрос:** были ли явные старые negative adapter candidates значимы для DPO, а не просто для routing audit? **Источник:** [old decision fields](https://github.com/lubochka/llm_mvp_core/blob/language_translate/00.Core/TranslationRoutingContracts.cs#L39-L66) vs [new result](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationContracts.cs#L56-L67). **Нулевой этап / обрыв:** grep/call graph ровно `RejectedAdapters → pair materializer → DpoDataset → learner` в старом коде. Если нет пути — записать `NOT_HISTORICAL_TRAINING_PATH`, на этом **не создавать** кросс-доменные negative pairs.

**Только при наличии исторического consumer:** заморозить 12 admissible same-prompt contrast examples, направления RU/EN/HE (4×3). Arms A — current origin candidate sourcing; B — same proposed candidates with **wrong adapter** as rejected when independently verified; C — random rejected with same count, tokenizer and cost. Проверить `chosen/rejected` semantic and model provenance, pair admission/legal gate, 1–3 bounded training updates **в правильного recipient** и heldout free answers. Метрики: accepted meaningful pairs, label inversion, preference score shift, answer gain and leakage. **Фальсификатор:** historical negatives были только trace; либо улучшился margin без изменения свободного ответа. Никогда не обучать от rejected, если он не был независимо признан ошибкой.

### Micro-D — старые успехи, три отдельные A/B (P0)

| Тест | Baseline vs вмешательство | Домен | Измерить / остановить если... |
|---|---|---|---|
| D1 Translation checkpoint and memory | direct current old 4-dir cold load vs versioned 4→6 migration, then exact same learned corpus state | RU↔EN, HE↔EN (ES only empty new dirs) | restore success, history/memory identities, 6–12 **new** semantic answers. Если stored memory не достигнет consumer — сначала исправить bridge, не обучать LoRA |
| D2 Contextual E26 | current word-map vs real POS-neighbor/context-conditioned trained retrieval with identical lessons | EN/RU polysemy; general vs specialized | sense accuracy, negative controls homographs, no false 100% claim on tiny original 15 |
| D3 Structural E09 | current C# consumer vs same consumer with E09 structural kNN candidate and model selection only; keep corpus split | C# AST code-to-doc / optional SQL AST separately | family-disjoint top1 and **final code-doc semantics**; prevent 763.5ms old enhanced from masked perf regression |
| D4 Replay E43 | no replay vs 177 prior replay-equivalent examples, explicitly exclude cross-file pseudo-bigrams | domain retention | old retained and new accuracy, no cross-family leakage, same predictor and lexicon |

**Сначала D1** (возвращает выученную память, не зависит от новой модели), затем D2/D3 по нужному продукту, D4 после наличия ненулевого learned skill; не запускать все микротесты массовой Windows CI.

### Micro-E — новое отрицательное свидетельство 18.10 не повторять как невыполненное

Источник [отчёт 8 DPO updates](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/report.md): legacy fractional-mask fixed finite differences, **0/24→0/24, 24 текста идентичны**. **Вывод:** `GRADIENT_COMPONENT_CORRECT` и `SEMANTIC_QUALITY_NO_OBSERVED_GAIN` одновременно. Следующий осмысленный neural test — не ремейк mask-fix, а recipient-specific input-conditional learning с независимым heldout и знакомыми negative controls: frozen B0 DPO 8, same pretrained/warm base SFT→DPO (только если legally allowed), learned translation memory (отдельный algorithm owner). Результаты не сводить к одной метрике. Сослаться на существующий RP01 plan и отдельно фиксировать любые новые права доступа к data; не копировать прежние неразрешённые corpora.

## Acceptance и формат результата исполнителя

```text
case_id; branch_sha; source_sha; domain; train_family; eval_family;
owner_type; chosen_pair_id; rejected_pair_id; tokenizer_fingerprint;
pre_state_hash; post_state_hash; restored_state_hash;
training_updates; consumer_route; candidate_origin; output_before; output_after;
independent_judge_verdict; error_code; latency_ms; cost; data_authority;
claim_type=[ROUTING|RETRIEVAL|LEARNED_MEMORY|NEURAL]; status=[CONFIRMED|FALSIFIED|INCONCLUSIVE]
```

**Good:** «после cold restore тот же Unit на 2/12 ранее неизвестных корректно различил медицинское/техническое значение; 0 новых false positive; исходный корпус и judge frozen». **Bad:** «прошёл тест DPO / поправили вес / updated SHA, значит качество улучшилось». Непокрытые источники (старый external corpus/branch/real host) явно `NOT_VERIFIED`.

## Skill и источник-специфический контроль

Применены при проектировании [architecture classifier](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/architect-behavior-classifier-SKILL.md), [phase planning](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/phase-planning-SKILL.md), [plan review](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/plan-review-SKILL.md), [code examination](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/code-examination-SKILL.md), [claim verification](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/claim-verification-SKILL.md), [prompt patch](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/prompt-patch-authoring-SKILL.md), [human readability](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/human-readable-architecture-audit-SKILL.md), [learning-path](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/learning-path-audit-SKILL.md), [experiment-design](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/experiment-design-SKILL.md), [translation reality](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/translation-domain-reality-check-SKILL.md), RAG WorkLeft [master](../../09.09.26WorkLeft-RAG-MASTER-INDEX.md) и [RP01 source map](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/SOURCE-AND-ARCHITECTURE.md). **Не подменять исторический README указателем на новый документ; использовать только additive preface.**
