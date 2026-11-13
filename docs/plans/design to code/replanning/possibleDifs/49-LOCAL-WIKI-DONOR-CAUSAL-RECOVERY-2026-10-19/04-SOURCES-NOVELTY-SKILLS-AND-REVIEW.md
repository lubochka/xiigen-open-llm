# 49/04. Provenance, WorkLeft RAG, сравнение с агентами и review work orders

**19 октября 2026.** Выводы в пакете 49 основаны на целевых первичных фактах; обзор сотен исследований использован для предотвращения повторов, но **полное построчное чтение всех 400+ файлов, каждый commit в history и каждый private checkpoint не заявлены**.

## 1. Git/SHA evidence ledger — от чего происходит каждая проверка

| Claim | Первичный источник / версия | Прямо проверено в этой сессии | Граница |
|---|---|---|---|
| Donor positive CodeTrace DPO heldout 0.92, published | [source run 064][ct_result] (ветка local_wiki_planning, HEAD 219d7ba89dc3ef43e03e17f1461ad96154d3dd7a) | Прочитан loop result JSON: 50+50, primary1, DPO .92, status published | Не free-generation |
| Donor CodeTrace разнообразен | [100 JSONL CodeTrace][ct_pairs] | Полный parse100: distinct prompts 100, chosen100, rejected100; 50+50; exact cross-split prompt duplicates0 | Не подтверждает independence of semantic families |
| Donor QueryPlan published, DPO 1.00 | [source run 134][qp_result] | Прочитан loop result JSON | Не продуктовый semantic score |
| QueryPlan DPO side-collapse | [100 JSONL QueryPlan][qp_pairs] | Полный parse100: 8 distinct prompt each half, 0 exact train/heldout overlap, 1 unique chosen и 1 unique rejected на все100 | Conditioned inference NOT_RUN |
| Donor RequestRouter .66 and NOT published | [source run092][rr_result] | Прочитан loop result JSON: trained_not_published, warning below .75 | Не текущий регрессионный run |
| CodeTrace material regression | [old CodeTrace pipeline][ct_old] blob 233d30ab87010e9b8375555e50d5f1772ed15d45 vs [current][ct_new] blob e4a6ca484f43998fbb0cb7040cbbc56df7579afa | Донор ToDpoMetricPair(pair.A,Chosen,Rejected); цель принимает только RejectedType | Только названный metric pipeline |
| Router material regression + CODE_TRACE contamination | [old router pipeline][rr_old] blob 55484cd1eb8eceba0cbd90756e48cbc6bb9c34c3 vs [current][rr_new] blob 1de590d8172848a152e33218b4734d7018db3f42 | Смотрели полностью оба метода и их calls: model-visible triplet в current не зависит от prompt/chosen/rejected | Отдельный lexical model использует Prompt |
| Semantic fail-closed | [current runner][rr_runner] и [current Route model][route_model] | semanticQualityClaimed=false, PublishAllowed=false; RouteForEvaluation не равен Route | Gate может быть правильно спроектирован |
| Фактический ordinary training router | [old orchestrator][orch_old] blob 07dd59b03b347ca349365cc7c8e814edec80877a; [current][orch_new] blob b2f9fdc4c1693e9f1def0528ac13fc2db5274f0c | _router.Route(prompt) в обоих, специализированный обучаемый model имеет иной Unit/consumer | Уже было в donor, не новая регрессия |
| RP01 — другой learner | [scientific experiment][rp01] и [program][rp01prog] | Проверены exact pair material, сохранённая модель, отдельная DPO q/v LoRA цепь; прямого вызова Wiki CodeTrace pipeline в просмотренном Run нет | Полное call-graph search всего core не выполнено |
| Registry не гарантирует user quality | [donor registry next action][registry] | registry_entry_count426, accepted35, rejected183, experimental182, blocked26; next target NONE, new batch disabled | Это статус asset custody, не accuracy |

**Additional source-only reproducible ceiling:** donor CodeTrace 100 raw rows содержат 5 аннотированных rejected-type семейств (missing_callee, missing_value_flow, wrong_caller, invented_callee, incomplete_trace), по 20 строк в каждом. В нынешней CodeTrace project metric projection используется только RejectedType. Поэтому при отображении этих 100 случаев в такую функцию максимум **5 разных model-visible triples** (не считая служебных PairId), против 100 raw троек. Это алгебраический ceiling при условии совпадения извлечённого type, **не запущенная native projection и не измеренный output loss**. Машинный source-only receipt: [05](05-SOURCE-ONLY-CODETRACE-COLLAPSE.json).

## 2. Что уже есть у других агентов — без присвоения «новизны»

Прочитаны и учтены существующие top-level [master synthesis][prior_master], [00-LANGUAGE-TRANSLATE recovery][prior_lang], каталог 47, [48 новые конкретные гипотезы][prior48] и **только для сравнения, без записи**, protected [Core28 CodeTrace][core28], [Core39 early CodeTrace][core39], [Core76 WikiTrace DPO input erosion][core76], [Core74 cross-study][core74]. Есть и 47/48 вложенные пакеты с многофакторными KNN/translation гипотезами.

- **НЕ новое:** DPO short signal/marker shortcut как класс, neural output head vs q/v LoRA, старые E09/E34 несопоставимые denominators, EOS, deterministic consumer, metadata checkpoint, learned-vs-fixed kNN, replay, teacher rights, source RAG/source lineage. Эти темы связаны с уже записанными prior docs.
- **Новая предметная добавка D49-01:** сравнение *двух веток конкретного ToDpoMetricPair* с **поздним** CodeTrace batch064 (а не прежним batch001) + источник разнообразия 100/100 + ceiling 5 при пяти типах ошибок.
- **Новая предметная добавка D49-02:** явное CODE_TRACE_DPO/PREFERRED_CODE_TRACE_OUTPUT ×12 внутри *Request Router*, полное отсутствие Prompt/ExpectedRouteKind в его DPO scorer, хотя отдельный learner их читает. Проверяется пара противоположных target routes при общем rejectedType.
- **Новая предметная добавка N49-05:** конкретная диспропорция archived QueryPlan batch134 — 100% DPO heldout при **1/1 уникальных сторонах** и 8/8 prompt variants. Это вопрос условности плана, а не обвинение в exact overlap.
- **Важное непереоткрытое:** нынешний fail-closed gate при semanticQualityClaimed=false и обычный static router. Оба являются точными consumers/source facts; статус «впервые обнаруженный дефект» им не даётся.

Доказанный пробел в подготовке **не означает доказанную причину деградации RP01** без matched source, recipient и standard quality metric. Ключевая первоочередная задача — быстро провести причинное ограниченное сравнение 49/03. Если существуют другие файлы с аналогичным содержанием, использовать их как дополнительную ссылку, не создавать ещё один конкурирующий алгоритм.

## 3. Навигация по RAG и skills

Реально прочитаны руководства [WorkLeft-RAG-00 usage][rag_usage], [WorkLeft-RAG-MASTER-INDEX][rag_master], [WorkLeft-RAG-01 DPO legal][rag_dpo] и [RP01-CURRENT-CODE-MAP][rpmap]. Это **карта к исходникам**, не доказательство работы модели. Любая ссылка на архивное решение дополнительно сверяется с current caller и точным artifact.

Применены специализированные skill entrypoints, реальный текст которых прочитан по состоянию current branch:
- **Architect**: [architect-behavior-classifier-SKILL][sk_arch], разграничивает живой запрос / архив, actual vs claimed capability.
- **Planning**: [phase-planning-SKILL][sk_plan], ограничивает scope, просит exact inputs/output/authority.
- **Planning review**: [plan-review-SKILL][sk_review], требует candidate-bound evidence и отдельной приёмки.
- **Code review**: [generated-code-review-SKILL][sk_code], просит фактический call chain и exact SHA, не вывеску PASS.
- **Prompt engineer**: [prompt-patch-authoring-SKILL][sk_prompt], один bottleneck за цикл.
- **Human communication**: [human-communication-GLOSSARY][sk_human], «домен» — тема обучения, не вид алгоритма, язык/направление и unit-level — отдельные поля.
- **Learning audit / experiment design**: [learning-path-audit-SKILL][sk_learn] и [experiment-design-SKILL][sk_exp], проверяют FIRES/COMPLETE/CORRUPTION и фиксацию baseline/enhanced/metric/negative control.

Эти инструкции **применены к построению документа**, но не являются доказательством запуска независимых sub-agents. Настоящий independent review: NOT_PERFORMED.

## 4. Выполнимый наряд исполнителю (после публикации research пакета)

**Входы:** donor run064 и run134 raw JSONL, donor/current методы CodeTraceBuilderTrainingPipeline и RequestRoutingTrainingPipeline, exact source/branch SHA, ближайшие 16/8/12 C# train/dev/exam cases по семействам, checkpoints/rights отдельно, текущая система legal admission. **Запреты:** без изменений protected core/plans/replanning-possible-diffs; без ослабления guardrail; без изменения EXAM после оценки; без эмуляции neural output rule engine; не поднимать полный Windows CI.

**Наряд A (CODEX executor, только при разрешённой runtime среде):** реализовать изолированные preflight/counterfactual вызовы real ToDpoMetricPair, логировать видимые токены и коллизии при одинаковом rejectedType. Не создавать новую модель для того, что можно сравнить адресными входами и текущим scorer. Затем один фактор raw-vs-type-only для EXP49-CT-PAIR, фиксированные budget/seed/teacher rights; результат — A/B score и настоящие accepted traces с сохранением отказов.

**Наряд B (независимый review, другой исполнитель):** открыть те же версии и артефакты; подтвердить source→recipient→consumer; повторно посчитать unique text families; удостоверить exact split/отсутствие утечки; проверить, что intervention меняет только один фактор и не включал hidden gold; проверить действительную пользовательскую correctness. Вердикт PASS только для named candidate SHA и конкретных измеренных cases, иначе NEEDS_FIX/INCONCLUSIVE. Особое внимание: у старого donor query planning стороны одинаковы, и score1.00 нельзя считать доказательством content-conditioned learner.

**Acceptance research-пакета (эта сессия):** имеются названные первичные URL и конкретные SHA, явно разделены confirmed code changes / prior known / new hypotheses, эксперименты воспроизводимы на малом объёме, есть отрицательные controls и границы claim, protected catalog untouched. **Acceptance learning quality (в будущем):** фактически запущенный C# A/B, реальный trained state, unseen semantic improvement, cold restore+ordinary reader, независимое принятие. Сейчас learning quality acceptance **НЕ ДОСТИГНУТА**.

## Source URLs
[ct_result]: https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-code-trace-builder-high-contrast-terminal-chain-promotion-confirmation-064/project-wiki-training-loop-result.json
[qp_result]: https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-134/project-wiki-training-loop-result.json
[rr_result]: https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-request-router-post-registry-selector-promotion-confirmation-092/project-wiki-training-loop-result.json
[ct_pairs]: https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-code-trace-builder-high-contrast-terminal-chain-promotion-confirmation-064/dpo-runs/code-trace-builder-unit/code-trace-builder-realistic-multihop-high-contrast-terminal-chain-promotion-confirmation-064-code-trace-builder-unit-20260524115856/dpo-pairs.jsonl
[qp_pairs]: https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/43.LearningLocalWikiData/artifacts/runner-metrics/2026-05-24-project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-134/dpo-runs/project-wiki-query-planning-flow/project-wiki-query-planning-angular-dotnet-binary-contrast-promotion-confirmation-134-project-wiki-query-planning-flow-20260524184949/dpo-pairs.jsonl
[rr_old]: https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingPipeline.cs
[rr_new]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingPipeline.cs
[ct_old]: https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/42.ProjectWikiDomain/CodeTraceBuilderTrainingPipeline.cs
[ct_new]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/CodeTraceBuilderTrainingPipeline.cs
[rr_runner]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainingRunner.cs
[route_model]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs
[orch_old]: https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/42.ProjectWikiDomain/ProjectWikiTrainingOrchestrator.cs
[orch_new]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/42.ProjectWikiDomain/ProjectWikiTrainingOrchestrator.cs
[rp01]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs
[rp01prog]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/tools/Rp01SimulationDpo/Program.cs
[registry]: https://github.com/lubochka/llm_mvp_core/blob/local_wiki_planning/43.LearningLocalWikiData/reports/PROJECT-WIKI-NEXT-TRAINING-ACTION.json
[prior_master]: https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/00-MASTER-SYNTHESIS-AND-CONFLICTS.md
[prior_lang]: https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/00-LANGUAGE-TRANSLATE-RECOVERY-FIRST-2026-10-19.md
[prior48]: https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/02-NEW-HYPOTHESES.md
[core28]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/28-code-trace-learning-donor.md
[core39]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/39-code-trace-remediation-lineage.md
[core76]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/76-wiki-trace-donor-input-loss-and-checkpoint-identity.md
[core74]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/74-helper-master-reconciliation-and-research-priorities.md
[rag_usage]: https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/09.09.26WorkLeft-RAG-00-usage-guide.md
[rag_master]: https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/09.09.26WorkLeft-RAG-MASTER-INDEX.md
[rag_dpo]: https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/09.09.26WorkLeft-RAG-01-dpo-legal-build.md
[rpmap]: https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-CURRENT-CODE-MAP.md
[sk_arch]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/architect-behavior-classifier-SKILL.md
[sk_plan]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/phase-planning-SKILL.md
[sk_review]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/plan-review-SKILL.md
[sk_code]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/generated-code-review-SKILL.md
[sk_prompt]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/prompt-patch-authoring-SKILL.md
[sk_human]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/human-communication-GLOSSARY.md
[sk_learn]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/learning-path-audit-SKILL.md
[sk_exp]: https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/skills/experiment-design-SKILL.md
