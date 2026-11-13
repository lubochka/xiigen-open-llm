# R20 — проверенные разрывы и применённые исправления

**R20-CP4 · 13.10.2026 · адресный review/repair по прямому поручению пользователя.** Исправлены actual protocol, full R20 card и все пять own carriers, а не только новое приложение. Это документный результат. Actual training, external prompt dispatch и independent Astra review этой сессией не выполнялись. [SOURCES.csv](SOURCES.csv) содержит новое чтение; исходные U20/P20/K20/CARD-Rxx receipts сохранены побайтно в [SOURCES.CP3.csv](SOURCES.CP3.csv), blob `41ef8ac5e03c4c91c75de36d2e994b1aa0a73766`. Они остаются ATTRIBUTED прежнему R20, не становятся новым полным аудитом.

## Вывод для владельца

R20 уже имел полезные manifests, child examples и R06 consumer example. Разрыв был между этой основой и нынешним исполнением: active results всё ещё говорили R01–R19 и об отсутствии R19; карточка предлагала повторить уже сделанное и менять файлы R19; число строк/сценариев могло заменить доказательство содержательной доставки. Ремонт сохраняет базу, вводит action-specific readiness, актуальные R18 pre/post conditions и same-code N/U. Это делает следующую работу определённой, но не означает, что двадцать полных prompts уже собраны и доставлены.

### F20-01 — текущие входы и разрешение параллельности

**Исторически:** первый R20 исправлял предел R01–R17 на R01–R19. Та фраза в прежнем FINDINGS была устаревшим active claim, не последним поручением. **Сейчас:** R01–R20 могут стартовать независимо; R10 по-прежнему объединяет девять research inputs; 25 заданий сохраняют 10+5+5+5. Finalization зависит от конкретного принятого input, не от whole sibling DONE. Исправлено в protocol §§1–3, R20 card A, D20-01 и CHANGE-MAP R20-001. Старые counts не заявляются actual invocations.

### F20-02 — manifests не равны полным доставленным prompts

Первый автор структурно просмотрел CARD-R01…CARD-R25, не выполнил 25 полных source audits. Это ограничение сохранено. CP4 отдельно определяет ROUTE_RESOLVED, CONTENT_ASSEMBLED, PREPARATION_READY, FINAL_DECISION_READY, IMPLEMENTATION_AUTHORIZED и INVOCATION_OBSERVED. По каждому собираемому назначению требуются actual contents, source/skill depth, checkpoint, allowed next action, missing-input resolver и receipt; таблица §3 — manifest/документная граница всех двадцати стартов, не assembled content. Full R08 exception, R07 A–I/J, R11/R12/R13/**R14**/R15 repairs и current R18 A–D сохраняются. Actual external compiler/delivery по-прежнему NOT_OBSERVED.

### F20-03 — R19: историческое отсутствие больше не текущий blocker

Старый 404 из P20-19/RUN-LOG верен для прежнего снимка и сохраняется как historical evidence. На P `9ec4f8242234158609adbe3dd92ee50b45348201` R19 FINDINGS наблюдался, blob `faf41fc013108c64799cb55ad590e5c7f85f8c61`; START содержит маршрут R20, PARALLEL20 и границы R19. Это позволяет принять навигационную дельту, не объявляя R19 DONE/двадцать packets dispatched. Новый owner contract оставляет shared entrypoints R19; R20 пишет свои protocol/card/results и передаёт чужие section deltas, не whole-file replacements.

### F20-04 — архитектура сохранена как причинные механизмы

Unit domain/vector/output schemas и typed many→many associations; Flow corrections; Orchestrator lawful async paths/fan-out/fan-in/cancellation/full branch cost; Graph higher-order transfer с реальным edge authority. ILearningMachine сохраняется как training owner, Graph — L5 specialization, не новый trainer. По каждому стыку нужны law, learned state/signal/objective, owner/caller/consumer, intervention и falsifier. Domain tokens/encoder/projection и LoRA — разные контракты; k>1 при first-hit consumer не является multi-neighbor learning. These are preserved design requirements, not a new current-code audit or proof of implemented utility.

N/U contrast уточнён: frozen no-update N и updated U используют один corrected material/token/mask/stop/input-renderer/ordinary pipeline; engineering L→N не приписывается training. Frozen family-disjoint panels, domain mapping/unknown/mixed, laws, quality/time/retention/worst-domain/full PC и все attempts заменяют coverage как evidence learner gain. Требующиеся deterministic integrity/native safeguards сохраняются по риску, не превращаются в универсальный Windows gate первого урока.

### F20-05 — DPO роли и смысл pair сохранены

По непосредственно прочитанному current WorkLeft-02 внешняя разрешённая открытая китайская модель пишет обе final стороны; CODEX/Astra отдельно выбирает урок как human delegate и отдельно оценивает exact pair; Student и post-update evaluator отдельны. Exact bytes/semantics, meaningful rejected, preferred_side versus pair_usable, TIE/BOTH_BAD/UNJUDGEABLE и one-pair/no-repeat передаются в packet. Correct deterministic answer не negative по происхождению. LoRA/update, DPO objective и tokenizer не подменяют друг друга. Вызова какой-либо из этих ролей в CP4 не было.

### F20-06 — повторные проверки ограничены изменившейся семантикой

25 existing manifests и 12 failure families не объявлены новой программой/тестами. Protocol §6 адресно уточняет late/current R19, R18 pre/post, source-rule/status conflicts, N/U, overflow и shared ownership; R19 twenty DOCUMENT_EXAMPLE cases остаются attributed documentary examples. Прежнее указание R20 заново пройти всё заменено NO_CHANGE receipt для принятого и review только зависимых deltas. При count/content несоответствии понижается claim, а не запускается новая бесконечная skill-repair петля.

### F20-07 — E1 и R18: не предсказывать результаты будущих действий

Прочитан R18-REVIEW-REPAIR-2026-10-13 §§1–2. Existing A–D и найденный ранее TextGenerator переиспользуются; old blanket all-G1–G6-before-any-call не возвращается через R20. Grant/expected model нужны до call; request IDs — наблюдение после; exact assessed pair — до mutation; G5-pre и G5-post различаются. Conditional authorized clean-missing baseline не равен разрешению обойти corrupt state/S04 prohibition. Common facade direction и owner-input renderer — attributed R18 design, не выполненная реализация R20. E2 kNN, E3 Flow/Orchestrator, E4 Graph остаются самостоятельными; E1 не закрывает весь гибрид.

### F20-08 — восстановлен смысл primary, не только ссылка RAG

Прочитан V/RAG-06 opening и F/DECISIONS-LOCKED целиком в возвращённом коротком файле. RAG предупреждает о старом CODE-ABSENT; D-06 Replay исторического F не активируется вопреки нынешнему S04 no-repeat; D-13 coverage говорит о subject density/predicate profiles, не unit-test проценте; D-15 не позволяет kNN/priority/budget обойти arbiter. Исторические числа compression/accuracy/adapter size не принимаются как current measurements. В WorkLeft рядом с действующим DPO role contract есть старые sibling-status формулировки: правило и статус разрешаются отдельно. Actual current status берётся у своего владельца, не угадывается по шапке.

### F20-09 — active scope, история и предметный handoff

CP4 не меняет START/README/AGENTS/PLAN/COMMON/JOIN, WorkLeft, R18, core, skills, tests/workflows, данные или weights. Original protocol сохранён тем же blob в R20-EFFECTIVE-PACKET-DISPATCH.CP3.md, active protocol исправлен в прежнем пути; исходный source ledger также сохранён побайтно. IDs F20-01…07, D20-01…09 и CHANGE-MAP R20-001…011 остаются, уточнения стоят рядом с неверными действующими утверждениями. RUN-LOG отделяет работу нынешнего интегратора от первых R20/CP3.

## Что действительно рассмотрено и что не закрыто

Новый review: current R20 card/results/protocol по адресным sections; PARALLEL20 и repair-card R20; START/JOIN/R19 opening/current authority; R18 repair; direct original roles, WorkLeft role/learning contract, один RAG→primary route; шесть адресных skill excerpts; первые исторические passages previous rounds; primary DPO/LoRA abstracts. Полный corpus/все WorkLeft/все 25 full cards и actual current core code этим repair не перечитывались.

**Опубликованный результат:** согласованный documentary repair и подробное назначение следующей работы R20; preparation/final/action boundaries для 25 manifests, meaningful examples/edge/falsifiers и обновлённые own carriers. **Не заявляется:** двадцать/двадцать пять CONTENT_ASSEMBLED packets, actual external invocation, закрытие sibling research, independent plan PASS, runtime binding всех G1–G6, observed learning или PC savings. Сборка конкретного packet получает отдельный content/dispatch receipt; это не новый исследовательский раунд и не причина перезапускать R01–R19.

**Передача:** R19 — адресная adoption CP4 содержимого по существующему route, без смены START; R18 — сохранённая актуальная action-specific lifecycle semantics; R21/R22/R23/R24/R25 — curriculum, causal study, PC/feedback, ordinary delivery и independent exact-candidate examination по прежним назначениям. Получатели не считаются уже принявшими эту новую редакцию без own receipt.
