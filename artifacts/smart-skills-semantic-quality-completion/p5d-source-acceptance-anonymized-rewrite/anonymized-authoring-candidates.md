# P5D anonymized authoring candidates

Статус: это accepted named-reference authoring candidates для будущей anonymized authoring phase, не training material. Все строки ниже остаются `training_ready=false`; они не разрешают training, eval, checkpoint rewrite или semantic-quality claim. Каждая строка теперь несет source-acceptance metadata прямо в JSONL, чтобы downstream authoring не мог случайно повысить ее без parent review.

## 1. Universal core vs adapter

Было опасно: исходная строка могла научить модель выбирать по названию документа, именам skill-файлов, локальному стеку, командам и project map.

Что удалили: пути, имена файлов, названия языков и фреймворков, команды, branch/commit conventions, локальные artifact paths и skill-name clues.

Универсальная граница: common orchestration отвечает только за переносимую дисциплину - режим, требования, evidence scope, review boundary and completion boundary. Project adapter подставляет локальную policy отдельно.

Chosen rewrite: разделить reusable discipline и adapter policy; не переносить project commands into common engine.

Rejected rewrite: объявить локальные adapter rules универсальными и зашить project map/commands в common behavior.

Ablation: подсказки по file title, skill names, stack markers, command markers and project-map markers должны провалиться. Решение должно держаться на конфликте core-vs-adapter в другом домене.

Remaining risks: row target is both, поэтому future authoring must split PromptToTask and OrchestrationReasoning variants instead of duplicating the same evidence; analysis-state still says todo, so parent review must accept named-reference use before materialization; rejected example can become too generic if it is not close enough.

## 2. Multi-branch review and targeted repair

Было опасно: источник имел folder id, title, node labels, node numbers and branded phrases, которые могли стать shortcut вместо orchestration reasoning.

Что удалили: folder/title/node labels, numbers, branded phrases, source-document-line framing and package structure.

Универсальная граница: сложная задача раскладывается на независимые ветки; каждая ветка собирает evidence; review выбран до final formatting; merge происходит только после review; при провале чинится только сбойная ветка.

Chosen rewrite: keep branches separate, review before final formatting, and repair only failed branch.

Rejected rewrite: collapse all branches into one path, review too late, or restart all work after one failed branch.

Ablation: folder/title/node-label/order controls должны провалиться. Решение должно зависеть от branch separation, evidence-before-merge and targeted repair.

Remaining risks: near-duplicate risk with station-chain flow remains, so future split should use this row for multi-branch targeted repair, not station count; analysis-state still says todo; chosen/rejected text must not use package title or node labels.

## 3. Root decision and dependent gates

Было опасно: строка была code-source heavy: filename, language, namespace, class/method names, parser mechanics and syntax labels могли полностью объяснить expected answer.

Что удалили: расширение исходника, путь, язык реализации, namespace, class/method names, parser mechanics, grammar labels, relation tokens from source syntax and project details.

Универсальная граница: orchestration сначала находит главное решение, затем привязывает к нему dependent evidence, gates and review obligations; sequencing следует отношениям между элементами, а не поверхности источника.

Chosen rewrite: root decision plus dependent evidence/gates/reviews before sequencing.

Rejected rewrite: surface-order route or source-term route without dependency boundary.

Ablation: file-extension, class/method, namespace-like, grammar-label and source-title controls должны провалиться; не-кодовый пример должен сохранять тот же выбор.

Remaining risks: future authoring must not mention code/source family as a clue; relationship wording can become too abstract unless paired with a concrete non-code task example; analysis-state still says todo.

## 4. Isolation before reusable-asset claim

Было опасно: строка пришла из test-source и могла научить, что test framework labels or assertion words are readiness proof.

Что удалили: test framework markers, assertion words, test method names, run ids, temp paths, singleton/shared implementation labels and exact-count assertions.

Универсальная граница: reusable orchestration asset нельзя считать готовым, пока не проверены isolation across parallel branches, no state bleed, deduplication and independent evidence before claim.

Chosen rewrite: require isolation, deduplication and no-cross-contamination evidence before readiness claim.

Rejected rewrite: accept reusable/shared asset as ready after one reassuring smoke check or source label.

Ablation: test-label, assertion-word, run-id, temp-path and exact-count controls должны провалиться; выбранная граница должна работать в не-test scenario.

Remaining risks: future authoring must not say test pass equals readiness; analysis-state still says todo; rejected row needs a clear near miss where evidence isolation is skipped but the source/test label looks reassuring.
