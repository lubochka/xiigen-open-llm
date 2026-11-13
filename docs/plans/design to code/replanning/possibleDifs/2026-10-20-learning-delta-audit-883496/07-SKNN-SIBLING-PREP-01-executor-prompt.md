# Передача существующего AST-SIB-CACHE-01; SKNN не запускается отдельно

**Сейчас только документирование.** Этот текст устраняет конфликт двух independently prepared recipes, не назначает runtime. SKNN-SIBLING-PREP-01 имеет статус `REFERENCE_ONLY` и не является отдельной гипотезой после публикации9d311.

## Единственный будущий промпт

При отдельном назначении этого фактора выполнить только **AST-SIB-CACHE-01**, S02, по [полной canonical карточке](https://github.com/lubochka/xiigen-planning/blob/9d311be43f4e108f6fa1cda1774549d54941c3c5/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/07-AST-SIB-CACHE-01.md). Источник, script, данные, before/after,4 TrainFromSource/128 Predict, WorkCore `C:/x/ast01`, .NET9/Roslyn4.12 и output `<Planning>/llm mvp/design to code session/replanning/possibleDifs/runs/AST-SIB-CACHE-01/20261020-01` уже назначены там. Не выбирать другой рецепт и не объединять его с2 facade Train/32 query из reference SKNN. Не создавать SKNN host. SourceCore и Planning вычисляются только заданным canonical способом; их поиск по другим каталогам не поручен.

Исполнитель и reviewer используют фактическую модель по умолчанию, без override. Отчёт русский. Точные skills в SourceCore/docs/skills: `architect-behavior-classifier-SKILL.md`, `learning-path-audit-SKILL.md`, `code-examination-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `plan-self-validation-SKILL.md`, `experiment-design-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `output-readability-gate-SKILL.md`, `human-communication-GLOSSARY.md`; правила пользователя выше старых product gates. Уже прочитанное не перечитывать ради оформления.

Точные prepare/build/nA/nB/wB/wA/compare команды — canonical §7; внешний script и его внутренние стадии не дублировать. Замерить полный настоящий TrainFromSource, state и чтение его16-query панели до/после. Бюджет128 Predict получается из4 arms×2states×16; никаких warmups/retries/additional generations. Любое полезное measured улучшение — успех оси/темы без обязательного accuracy gain; показывать все остальные изменения, включая ухудшения. Не объявлять single cold wall time устойчивым causal speedup.

Существующие results/claim сохранять. Ошибка подготовки до Train не является провалом обучения. Простые experimental exceptions/DTO/void разрешены; product changes, full CI, изменение protected и автоматическое внедрение не поручены. `report.md` и `candidate-fix.md` только в canonical result-root; один разрешённый readback без новых calls.

**Запрет поведения из инцидента:** «следующий/заканчивай и переходи» немедленно прекращает прежнюю задачу, включая дописывание report/registry, hashes, review, retry и commit. «Остановить всё» прекращает всё; допустимо только необходимое прекращение своего ещё активного процесса. Не возвращаться к прежнему опыту для завершения оформления.

В отдельно порученной полной программе **AST→NG01→BPE-REALIZED→IDENTITY→KN-ROUTER→END**. При одиночном AST после назначенной фазы END; не запускать соседние IDs автоматически. Ближайшим общим NEXT остаётся N62-R2.
