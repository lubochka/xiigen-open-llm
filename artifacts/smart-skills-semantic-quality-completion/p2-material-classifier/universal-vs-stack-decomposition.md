# P2 Universal-vs-Stack Decomposition - repair

Этот файл объясняет, что можно рассматривать как переносимую orchestration boundary, а что остается stack/project-specific evidence. После parent repair этот artifact также фиксирует демоции: docker docs, test code, tool rules and translation stack plans не становятся accepted только из-за имени файла, папки или категории. Все строки остаются not training-ready и не являются DPO rows.

## D-NAMED-DOC07956-001

Источник: `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\all-documents-method-run\full-corpus-unique-training-sets\doc-07956-xiigen-universal-orchestrator-plan\`

Универсально: Prompt раскладывается на независимые ответственности; ветви ведутся отдельно, merge делается после evidence/review, а failed fragment возвращается на repair без сброса уже принятых частей.

Исключено как stack/project-specific: Исключены XIIGen branding, doc-07956 folder, node filenames, exact node numbers, v12/genie labels и source title as shortcut.

Chosen shape: Для multi-domain задачи сохранить отдельные ветви, review до финального ответа и targeted repair только failed fragment.

Rejected near miss: Схлопнуть все домены в один happy path, сделать review после formatting или принять source-document line как runtime input.

Required ablation: Удалить folder/title/node labels; решение должно держаться на separate branches + pre-format review + targeted repair, а не на doc-07956 или node labels.

## D-NAMED-SKILLS-INVENTORY-001

Источник: `C:\Users\lubao\Documents\xiigen\llm mvp\skills loading as an ai extension\skills-inventory-universal-vs-project-specific.md`

Универсально: Skill registry, session router, arbiter/evidence discipline, portability labels and conflict policy are reusable AI-extension core.

Исключено как stack/project-specific: Исключены .NET/C#/xUnit/OperationResult, concrete subproject map, Windows Forms, exact commands, branch/commit conventions and local artifact paths.

Chosen shape: Разделять universal skill-loading/session/evidence discipline и adapter layer, который подставляет stack/project rules.

Rejected near miss: Зашить llm_mvp_core/.NET project map into universal engine или объявить каждый skill universal без adapter boundary.

Required ablation: Удалить .NET, C#, XIIGen, OperationResult, docs\skills path and skill filenames; переносимое reasoning должно сохраниться.

## D-NAMED-UNIVERSAL-DEPENDENCIES-001

Источник: `C:\Users\lubao\Documents\xiigen\llm mvp\llm_mvp_core-extracted\llm_mvp_core-main\28.SemanticNlp\Algorithms\UniversalDependencies.cs`

Универсально: Единицы могут иметь root decision, dependent evidence/gate children and relation labels; order/review строится из отношений, а не из поверхности файла.

Исключено как stack/project-specific: Исключены C# namespace/classes, ConlluParser/RuleBasedParser mechanics, CoNLL-U fields, relation token syntax, OperationResult/storage/idempotency details.

Chosen shape: Определить главный decision node, dependent evidence/gates and relation type before sequencing/review.

Rejected near miss: Выбрать route по filename UniversalDependencies.cs, method names GetRoot/GetHead или C# parser mechanics.

Required ablation: Удалить .cs filename, class/method names, namespace, CoNLL-U syntax and relation tokens; должен остаться generalized dependency/relation prompt.

## D-NAMED-UNIVERSAL-DEPENDENCIES-CACHE-001

Источник: `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\source-examination-cache\llm_mvp_core-main\llm_mvp_core-main\28.SemanticNlp\Algorithms\UniversalDependencies.cs`

Универсально: Identical content is one source family, not two independent training examples; source-disjoint split must respect duplicate family.

Исключено как stack/project-specific: Исключены cache path, extracted repo path, C# mechanics and file-copy identity as learning signal.

Chosen shape: Treat duplicate copy as same source family and require source-disjoint split by family, not by path string.

Rejected near miss: Count extracted and cache copies as two independent held-out sources or train on one and eval on the duplicate.

Required ablation: Path/hash duplicate controls must prove no duplicate-family leakage.

## D-NAMED-NINE-NODE-FLOW-001

Источник: `C:\Users\lubao\Documents\xiigen\llm mvp\llm_mvp_core-extracted\llm_mvp_core-main\21.AgentInfrastructure\Logical\UniversalNineNodeDevelopmentFlow.cs`

Универсально: Оркестрация может идти через chain: split, translate, plan, process, apply, translate result, select arbiters, arbitrate, format; evidence carried through stations and review occurs before final formatting/claim.

Исключено как stack/project-specific: Исключены C# records/classes, exact nine-node ids, singleton SharedNodeUnits, Project15Bridge, SemanticCompiler29, metrics keys, async/envelope mechanics.

Chosen shape: Выбрать phase/gate chain with evidence carried forward and review before final claim.

Rejected near miss: Hardcode exactly nine nodes, call Project15Bridge, or claim singleton execution proves semantic quality.

Required ablation: Удалить class/file/node names, Project15Bridge/SemanticCompiler29 and exact ids; success must survive renamed phases and non-C# examples.

## D-NAMED-FLOW-SINGLETON-TESTS-001

Источник: `C:\Users\lubao\Documents\xiigen\llm mvp\llm_mvp_core-extracted\llm_mvp_core-main\Tests\UniversalFlowSingletonTests.cs`

Универсально: Reusable orchestration assets need evidence of no state bleed across concurrent branches/batches, dedup of duplicate work and independent assertions before a readiness claim.

Исключено как stack/project-specific: Исключены xUnit/[Fact]/Assert, temp checkpoint root, concrete subphase ids, class/test names, singleton identity mechanics and exactly-nine assertion as product rule.

Chosen shape: Readiness plan requires isolation/dedup/no-cross-contamination evidence before claim.

Rejected near miss: Accept because a test name says universal or learn singleton shared node as universal behavior.

Required ablation: Удалить test names, Assert/xUnit, run-id strings, subphase ids and temp paths; row must still test evidence isolation, not C# test mechanics.

## D-P1-S-BASIC-PROMPT-PTT-001

Источник: `C:\Users\lubao\Documents\xiigen\basic prompt.txt`

Универсально: Prompt превращается в проверяемый task contract: цель, ограничения, запреты, artifacts, validation, examples and recovery/state.

Исключено как stack/project-specific: Исключены local paths, filenames, host/tool mechanics and project-stack command recipes.

Chosen shape: Выделить requested artifact, allowed verification and final-response boundaries before execution.

Rejected near miss: Пересказать prompt свободным текстом или выбрать действие по filename/path without task contract.

Required ablation: Paraphrase held-out должен пройти без exact prompt lookup, source path and filename.

## D-P1-S-PHASE-PATCH-ORCH-001

Источник: `C:\Users\lubao\Documents\xiigen\Wordpress integration\Worpdpress updated plugin\update skills between phases\PATCH-phase-transition-protocol-SKILL.md`

Универсально: Phase boundaries cannot be collapsed: review/fix evidence, state/docs update and next-phase planning/approval are separate gates.

Исключено как stack/project-specific: Исключены WordPress path, plugin folder mechanics, PowerShell/host details, exact patch commands and title/filename shortcut.

Chosen shape: Proceed only after accepted evidence; next phase remains bounded and separately authorized.

Rejected near miss: Commit/push before review or treat protocol patch/plugin path as implementation done.

Required ablation: Path/title/WordPress/plugin/static phase keywords must fail; decision must rely on gate-before-next-phase boundary.

## D-DEMOTED-CLINERULES-001

Источник: `C:\Users\lubao\Documents\xiigen\.clinerules`

Универсально: A rules file can be evidence that constraints/gates exist, but this row was not content-verified enough for common training acceptance.

Исключено как stack/project-specific: Tool-specific settings, local project rules, command recipes and filename .clinerules are excluded.

Chosen shape: Keep as evidence_only until a future manual decomposition names a content-supported stack-free boundary.

Rejected near miss: Promote because filename is .clinerules or because the folder suggests rules.

Required ablation: Filename/path/tool-config controls must fail before any future acceptance.

## D-DEMOTED-CURSORRULES-001

Источник: `C:\Users\lubao\Documents\xiigen\.cursorrules`

Универсально: A tool rules artifact may contain useful evidence, but this row is not accepted without content support summary.

Исключено как stack/project-specific: Cursor/IDE mechanics, local stack rules, commands and filename .cursorrules are excluded.

Chosen shape: Keep as evidence_only; future acceptance requires manual stack-free boundary from content.

Rejected near miss: Route by Cursor filename or copy IDE recipe.

Required ablation: Controls по .cursorrules/Cursor keywords must fail.

## D-DEMOTED-PROMPTS-001

Источник: `C:\Users\lubao\Documents\xiigen\PROMPTS.md`

Универсально: Prompt collections can be useful evidence, but generic prompt-family acceptance is not enough after repair.

Исключено как stack/project-specific: Implementation prompt details, stack actions, filenames and project recipes are excluded.

Chosen shape: Keep as evidence_only until a manually checked no-code task-contract boundary is written.

Rejected near miss: Promote because filename says PROMPTS or because entries look phase-like.

Required ablation: Regex/title controls must fail; future row must pass paraphrase-family held-out.

## D-DEMOTED-TRANSLATION-PLAN-001

Источник: `C:\Users\lubao\Documents\xiigen\XIIGEN_NESTJS_REACT_TRANSLATION_PLAN.md`

Универсально: A translation plan may contain planning/gate methodology, but this row is not accepted because the reusable no-code methodology was not explicitly separated in the candidate row.

Исключено как stack/project-specific: NestJS, React, translation stack details, implementation recipes and project names are excluded.

Chosen shape: Evidence_only until a future row states exact reusable planning/gate/DPO method and exact excluded stack details.

Rejected near miss: Promote because title says plan or because phases exist near NestJS/React details.

Required ablation: NestJS/React/title controls must fail; future accepted row must survive with only no-code methodology.

## D-DEMOTED-DOCKER-COMPOSE-001

Источник: `C:\Users\lubao\Documents\xiigen\Wordpress integration\Worpdpress updated plugin\debug skills\xiigen-new-skills-final\docker-debugger\references\docker-compose.md`

Универсально: No accepted universal orchestration boundary was verified from this content in P2 repair.

Исключено как stack/project-specific: Docker Compose services, ports, environment, volumes, docker compose up/down/logs and command syntax are stack execution details.

Chosen shape: Classify as evidence_only, not PromptToTask/OrchestrationReasoning candidate.

Rejected near miss: Promote docker docs because they live under skills/debugger references or mention orchestration-like service composition.

Required ablation: Docker/compose/service/port/volume command controls must fail.

## D-DEMOTED-PHASE-TEST-CJS-001

Источник: `C:\Users\lubao\Documents\xiigen\Wordpress integration\get-shit-done-main\get-shit-done-main\tests\phase.test.cjs`

Универсально: No accepted common orchestration boundary was verified from this test code in P2 repair.

Исключено как stack/project-specific: Node test code, phase directory sorting mechanics, assertions, filenames and local package behavior are excluded.

Chosen shape: Classify as evidence_only unless a future manual decomposition extracts a stack-free phase-order rule from content.

Rejected near miss: Promote because file name contains phase or because tests imply ordering.

Required ablation: tests/phase filename, Node syntax and sorting mechanics controls must fail.

## No-claim boundary

Этот decomposition artifact не заявляет semantic quality, не создает DPO rows и не разрешает training. Он только фиксирует evidence boundary для parent review P2.
