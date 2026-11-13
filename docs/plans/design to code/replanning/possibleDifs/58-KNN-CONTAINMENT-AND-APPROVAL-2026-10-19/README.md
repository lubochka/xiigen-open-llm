# N58 — Различия обучения: контекст kNN, историческое доказательство, корректность опытов и личная проверка

**19.10.2026 — WRITE: только xiigen-planning/llm_plan3/.../possibleDifs/58-*.** Защищённый путь **llm_mvp_core/docs/plans/replanning-possible-diffs/** строго READ ONLY. **SOURCE_AUDIT / DESIGN_ONLY; новые .NET запуски, optimizer updates, генерации, сборка, обучение и выводы об изменении качества: 0.** Все новые протоколы: **PENDING_LUBA_REVIEW / NOT_AUTHORIZED**.

**Проверяемый срез:** core [codex/dpo-implementation@eb05219](https://github.com/lubochka/llm_mvp_core/tree/eb05219c27663835ce927c7bf97cf1274c07cddb), planning [llm_plan3@3a8919d](https://github.com/lubochka/xiigen-planning/tree/3a8919daaa53c9bab8f4d6134f560009839530a4) перед записью. При запуске требуется новая фиксация HEAD, source SHA и начальных данных. Веточные доноры: [PrepareToGPL@31346d8](https://github.com/lubochka/llm_mvp_core/tree/31346d8928be4469b01aaf313780e8f950ff91e2), [DesignToCode-clean@e346b88](https://github.com/lubochka/llm_mvp_core/tree/e346b88a3cc1fcce48aefa626bfc7b00509eefd6), [language_translate@f6032a](https://github.com/lubochka/llm_mvp_core/tree/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9), [external_domains@3036a01](https://github.com/lubochka/llm_mvp_core/tree/3036a01acec938f0b83fa4ffbda26202cc35d639), [orchestrator-extensions@b293045](https://github.com/lubochka/llm_mvp_core/tree/b293045bd89b1eb96d3e32fedd341f1a4170adbc), [local_wiki_planning@219d7ba](https://github.com/lubochka/llm_mvp_core/tree/219d7ba89dc3ef43e03e17f1461ad96154d3dd7a). Наличие ветки само по себе не подтверждает научный эффект.

## Человеческий вывод

У XIIGEN есть подтверждённое **обучение памяти Unit** через структурированные уроки и ordinary Flow: опубликованные 8 уроков EN→RU дали **0/24→19/24** (P125, 18.10.2026), а не улучшение neural LoRA. При этом было 22/24 принятых ответов, включая две явные ошибки и один неоднозначный; новых независимых лексических семейств не проверяли. Старый neural RP01 B0+DPO **не получил улучшения свободного ответа**: C# 0/3→0/3, EN→RU 0/4→0/4. Ложная эквивалентность двух результатов недопустима.

**Переупакованный PRIOR_ART N58-K1/K2 (не новая находка):** текущая мера source-overlap EN→RU использует **множества слов, знаменатель только запрос**, удаляет not/no, и вызывается в ordinary kNN; ограниченный SourceLaw не покрывает императивы. Из этого следует кодовый риск неверного принятия соседа по команде с лишним действием или противоположной полярностью. **Это ещё не доказанный end-to-end дефект:** prior exact/phrase/word-map и несовпадение signature могут сделать ветвь недостижимой. Нужен сначала reachability trace, только потом честное сопоставление на 2 уроках / 4 экзаменационных запросах. Отдельных claims о нейронном Student не делаем.

## Содержимое

- [01 — доказательства, положительное историческое обучение и техническая карта различий](01-EVIDENCE-AND-SCOPE.md).
- [02 — узкая новая гипотеза K1/K2, кодовые строки, доменные границы и контрдоказательства](02-KNN-CONTAINMENT-AND-NEGATION-HYPOTHESES.md).
- [03 — два небольших A/B, полностью заданный стартовый пакет, имя модели, навыки, входы, ожидаемые результаты, слепая оценка](03-TWO-MICROEXPERIMENTS-PENDING-APPROVAL.md).
- [04 — независимая проверка защищённого каталога: что уже покрыто, что условно, что реально измерено](04-PROTECTED-EXPERIMENT-COVERAGE-REVIEW.md).
- [05 — протокол личной проверки, готовое поручение CODEX/исполнителю, негативные контроли и критерии evidence](05-HUMAN-GATE-AND-EXECUTOR-PROMPT.md).
- [06 — новая узкая кандидат-гипотеза K3: safe/unsafe и valid/invalid распознаны SourceLaw, но сравниваются как неизвестные; малый A/B](06-NEW-PREDICATE-COMPLEMENT-LAW-AND-SMALL-EXPERIMENT.md).

**Запрет запуска:** ни один текст этого пакета не означает согласие на изменение продукта или начало эксперимента. По текущей команде разрешено только создать исследования в planning. Требуется прямое одобрение Любы на конкретный version/hash карточки до исполнения. Если runner/source branch не подтверждён, результат остаётся CONDITIONAL, а не фиктивным PASS.

**Приоритет новизны:** K1/K2 уже обнаружены в N27/N37; это две уточнённые верификационные карточки. K3 — отдельный новый кандидат (предикатные противоположности внутри закрытого словаря SourceLaw). K1/K2 — точные сочетания нескольких конкретных source-факторов в `SourceTokenSimilarity`, не заявление о первом открытии общей проблемы kNN, потери отрицания, неполных фраз или низкой уверенности. По просмотренным P54, P55, P56, P57, P60, P81, P93, P107, P117, P125 и N27/N55/N56/N57 эти компоненты частично известны; все 493 entry и 470 subcase заново по смыслу не прочитаны. Возможную найденную позднее точную дубликацию отметить как PRIOR_ART и не переприсваивать.
