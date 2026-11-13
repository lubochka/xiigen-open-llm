# Pass P15 — Cursor: сокращение токенов (row12) + guard качества

Честный фрейм: этот проход оценивает РЕАЛЬНОЕ состояние механизма «сокращение
токенов» (row12) и «guard качества (non-regression)» для расширения Cursor —
по живому коду и артефактам, а не по copy/status пакета. Дедуп: не повторяю
P01 phase-13 («token STUB/NEEDS_FIX, reduction не доказана», фазовый),
P06 gap-2/3/4 (пакетный `token-savings-cursor-row12-20260628-153837/measurement.json`
synthetic no-send, baseline≡adaptive по MD5, harness BLOCKED, пустые логи),
P13 (Cursor entrypoint/transport), P14 (конвейер prompt→task→skills).

## Что проверено (bounded reads)

Cursor host-репо `C:\Projects\cursor-xiigen-smart-skills`:
- `scripts/run-frdp-row12-host-visible.ps1` (лаунчер Extension Host row12);
- `test/host-qa/FRDP-row12-hostVisibleRuntimeProofQA.js` (row12 QA-раннер);
- `reports/TOKEN-03-gap-repair-20260617-015255/cursor-gap-repair-summary.json`
  + `cursor-gap-repair-rows.jsonl` (14 строк baseline/adaptive);
- `reports/TOKEN-03-full-hosts-20260617-012704/token03-extensionHostFullPackQA.js`;
- `docs/CURSOR-DOCS-FINAL-COVERAGE-TOKEN-QUALITY-NONHOST.md`;
- `src/extension.ts` (grep token/quality).

Core `C:\Projects\llm_mvp_core\43.SmartSkillsDomain`:
- `TokenTraceAccounting.cs` (весь);
- `ExtensionQualityScoring.cs` (весь);
- `XiigenExtensionApiService.cs` (grep token/quality/token_mode);
- callers-грепы `TokenTraceAccounting`/`ExtensionQualityScoring`/`ExtensionEvalOnlyQualityRunner`.

Реальная измерительная программа row12:
- `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-token-savings-baseline/measurement-20260625T004435/Program.cs` (весь).

## Ключевой вывод P15

В отличие от того, как это описано в мастере (P01/P06: «token = STUB / synthetic
no-send»), на УРОВНЕ КОДА в core РЕАЛЬНО существует движок учёта токенов
(`TokenTraceAccounting`) и рубричный скорер качества (`ExtensionQualityScoring`).
Но:
1. движок считает **эвристические** токены (слова/пунктуация), а не токены
   реальной модели/хоста (`trace != savings`), и **не вызывается** ни границей
   расширения (`XiigenExtensionApiService`), ни `extension.ts`, ни `core-api.ps1`
   — только тестами и одной одноразовой `Program.cs`;
2. эксперимент baseline↔adaptive **не имеет независимой переменной**: обе ветви
   зовут одно и то же ядро с одним и тем же полным промптом; `mode`/`token_mode`
   — пассивная метка, которую ядро игнорирует ⇒ `token_delta` структурно 0;
3. 14-assertion quality-guard row12 **не может пройти по построению**, потому что
   ядро возвращает not-ready-границу (пустой `SelectedSkills`, нет структурных
   секций task/plan/skills); единственный исполненный замер вернул
   `MEASUREMENT_NEEDS_FIX` и рекомендовал НЕ засчитывать row12;
4. переиспользуемый quality-скорер — **человеческая рубрика, eval-only**, не
   подключён к рантайму; а `TokenTraceAccounting.EvaluateQualityGate` всегда
   проваливается, потому что `CreateHeuristicTrace` жёстко пишет null в
   `QualityScore`/`QualityRegression`.

## Детальные находки (file:line)

### 1. Движок учёта токенов существует, но эвристический и отсоединён

`TokenTraceAccounting.cs`:
- `:60-62` `HeuristicTokenizerName="xiigen-heuristic-word-punctuation"`,
  `HeuristicAccountingType="heuristic"`;
- `:101-127` `CountHeuristicTokens` — считает переходы «слово/пунктуация»,
  это НЕ токенизатор модели/хоста;
- `:66-99` `CreateHeuristicTrace` возвращает `QualityScore:null`,
  `QualityRegression:null`, `TokenSavingsClaimed:false`;
- `:135-177` `ValidateBaselineAdaptivePair` ТРЕБУЕТ одинаковый `prompt_hash`
  (`:146`) и ЗАПРЕЩАЕТ `TokenSavingsClaimed` в этой схеме-фазе (`:161-164`).

Callers `TokenTraceAccounting` (git grep): только
`Tests/ExtensionTokenTraceAccountingTests.cs` и одноразовая
`docs/plans/evidence/.../measurement-20260625T004435/Program.cs`.
`git grep TokenTraceAccounting -- 43.SmartSkillsDomain/XiigenExtensionApiService.cs`
→ 0. `grep TokenTraceAccounting` в `cursor-xiigen-smart-skills/src` и в
`tools/xiigen-extension-core-api.ps1` → 0. В `XiigenExtensionApiService.cs`
слова «token» встречаются только как строковые константы
`semantic_quality_claimed`/`tokenSavingsClaimed:false` — ни одного `CountTokens`
/ токенизатора. Значит: поставляемый Cursor-путь row12 (`FRDP-row12`,
`TOKEN-03`) не эмитит ни одного числа токенов.

### 2. baseline↔adaptive без независимой переменной ⇒ delta структурно 0

`measurement-20260625T004435/Program.cs`:
- `:66-67` `var baseline = InvokeMode("baseline", service); var adaptive = InvokeMode("adaptive", service);`
- `InvokeMode` (`:247-393`) для обеих веток строит `XiigenExtensionRequest` с
  ОДНИМ И ТЕМ ЖЕ `promptText` (`:261`), `method` (`:47`), `RequestedSkills`/
  `AvailableSkills` (`:274-275`); различие ТОЛЬКО `HostFacts["token_mode"]=mode`
  (`:269`) и `CheckSetInputs["token_mode"]` (`:281`);
- ядро `XiigenExtensionApiService` не ветвится на `token_mode`
  (`grep token_mode` в сервисе → 0);
- `:179-180` `observed_candidate_delta = adaptive.total - baseline.total`,
  `token_reduction_observed = adaptive < baseline` → фактически 0/false, так как
  `VisibleAnswerText = response.ChatDisplayText` — константа
  «Core xiigen boundary executed…» в обеих ветвях.

Тот же дефект в host-QA `TOKEN-03-full-hosts`:
`token03-extensionHostFullPackQA.js:5` цикл
`for(const mode of ["baseline_common_model_only","adaptive_common_plus_user_model"])`,
но вызов `vscode.commands.executeCommand('xiigen.smartSkills.select',{prompt:scenario.prompt_text,method})`
получает ТОЛЬКО `{prompt, method}` — `mode` в команду не передаётся, пишется
лишь в `rows[].mode`. Поэтому baseline и adaptive — идентичные вызовы; в
`TOKEN-03-gap-repair` это видно как совпадающий `StructuredTaskRefs`-хэш и
`SelectedSkills:[]` в обеих модах каждого сценария. Заголовки обоих раннеров
жёстко пишут `token_savings_claim_allowed:false`, `quality_non_regression_passed:false`,
`adaptive_effect_claimed:false`.

Вывод: эксперимент редукции требует baseline БЕЗ xiigen-контекста vs adaptive С
внедрённым/сжатым xiigen-контекстом (РАЗНЫЙ размер входа). Текущие раннеры шлют
идентичный вход дважды ⇒ delta может быть только 0; adaptive-нога без эффекта.

### 3. 14-assertion quality-guard row12 не проходит по построению

`measurement-20260625T004435/Program.cs`:
- `:546-562` 14 ассертов требуют, чтобы РЕАЛЬНЫЙ ответ ядра содержал marker,
  scenario_id, `task_id`/`task_title`/`constraints`/`acceptance_gates`, три
  упорядоченных шага, per-step verification gate, `SelectedSkills.Count>0`,
  выбранные скиллы из allowed-набора, секции references/non_claims,
  раскрытие детерминистских shortcut-границ;
- реальный ответ ядра — not-ready граница:
  `skill_selection_evidence=NO_AVAILABLE_SKILLS_PROVIDED`,
  `prompt_to_task_status=loaded_state_no_matching_learned_pattern`,
  `SelectedSkills:[]` (см. TOKEN-03 coreResponse);
- поэтому ассерты 3-14 падают; отчёт `:210-215`:
  «The 14-item quality guard does not pass because the actual compact core
  response does not preserve the marker/scenario/required structured sections»;
- `:97-104` `finalStatus = MEASUREMENT_NEEDS_FIX`;
- `:217-222` counter-recommendation: «Do not increment passed scenario count for
  row 12 … keep in needs-fix/blocked».

Значит совместный проход «редукция токенов + non-regression качества» невозможен,
пока ядро — not-ready-стаб (P02/P10), не выдающий структурного task/plan/skills.
Guard стоит вниз по потоку от стаба.

### 4. Переиспользуемый quality-скорер — человеческая рубрика, eval-only, отсоединён; heuristic quality-gate — вечный fail

`ExtensionQualityScoring.cs`:
- `:9-15` 4 обязательных слоя (`prompt_to_task`, `orchestration_reasoning`,
  `skill_selection`, `final_output_usefulness`);
- `:45-48` `missing_human_reviewer_note` — требует ЧЕЛОВЕЧЕСКУЮ заметку ревьюера
  на каждый слой (ручной процесс, не авто-guard);
- `:66-78` `QualityRegressed` — булев ВХОД от вызывающего, не измеряется скорером;
- `:122-125` в результате `QualityResultClaimed:false`, `SemanticQualityClaimed:false`.
- Callers (git grep): `ExtensionEvalOnlyQualityRunner.cs`,
  `43.SmartSkillsDomain.Tools/Quality02EvalOnlyRunner/Program.cs`, Tests — все
  eval-only/тесты; в рантайм Cursor не подключён.

`TokenTraceAccounting.cs`:
- `:87-88` `CreateHeuristicTrace` всегда `QualityScore:null`, `QualityRegression:null`;
- `:179-201` `EvaluateQualityGate` при null-полях всегда добавляет
  `token_delta_alone_cannot_pass_quality` и
  `quality_regression_requires_approved_quality_rubric` ⇒ `CanPass=false` всегда.

Значит автоматического guard-а non-regression на токен-паре нет: рубрика требует
ручных оценок слоёв + утверждённого rubric (не подключён), а heuristic-gate —
вечный fail.

## Документальное подтверждение (не claim)

`docs/CURSOR-DOCS-FINAL-COVERAGE-TOKEN-QUALITY-NONHOST.md`:
- `:28` `token_savings_claimed=false`, `:30` «trace != savings»;
- `:44` `quality_improvement_claimed=false`, `:46` «Quality-03 остаётся закрытым»;
- Token accounting boundary = PARTIAL, Quality non-regression = PARTIAL;
- будущий token-PASS требует same-pair: same host/prompt/scenario/method/
  evidence-class/tokenizer-version + baseline/adaptive refs + non-regression —
  НИЧЕГО из этого не доказано.

## Таблица НОВЫХ дыр

| Component | Real-status | Evidence file:line | Gap | Closure |
|---|---|---|---|---|
| Cursor row12 движок учёта токенов (TokenTraceAccounting) | BUILT_NOT_PROVEN / ARCH DISCONNECT | `TokenTraceAccounting.cs:60-62,101-127`; callers=Tests+`measurement-20260625.../Program.cs` only; `XiigenExtensionApiService.cs` token-count grep=0, token_mode grep=0; `extension.ts`/`core-api.ps1` grep=0 | Движок считает эвристические слова/пунктуацию (не токены модели/хоста, `trace != savings`) и не вызывается на поставляемом row12/TOKEN-03 host-пути — production-замер = 0 чисел токенов | Подключить реальный токенизатор хоста/модели к учёту в поставляемой Cursor-команде; эмитить числа токенов через границу; либо честно пометить heuristic-eval-only |
| Cursor row12 baseline↔adaptive paired runner | STUB EXPERIMENT / no independent variable | `measurement-...Program.cs:66-67,247-299` (обе ветви — один и тот же промпт/метод/скиллы, различие только `HostFacts["token_mode"]:269`); ядро token_mode игнорирует; `token03-extensionHostFullPackQA.js:5` executeCommand получает `{prompt,method}` без mode; delta/`token_reduction_observed:false` | Нет независимой переменной: baseline и adaptive шлют идентичный вход ⇒ token_delta структурно 0, adaptive-нога без эффекта (`adaptive_effect_claimed:false`) — редукцию невозможно показать | Задать реальную разницу контекста (baseline=сырой промпт; adaptive=xiigen-сжатый контекст) так, чтобы входы отличались; измерить настоящую дельту; доказать, что adaptive меняет вход/выход |
| Cursor row12 quality guard (14 assertions) | STUB-COUPLED-TO-NOTREADY-CORE / executed=NEEDS_FIX | `measurement-...Program.cs:546-562` (ассерты требуют marker/scenario/task_id/три шага/verification gate/SelectedSkills>0); ядро=`NO_AVAILABLE_SKILLS_PROVIDED`/`loaded_state_no_matching_learned_pattern`/`SelectedSkills:[]`; отчёт `:210-215`; `:97-104` `MEASUREMENT_NEEDS_FIX`; `:217-222` «do not count row12» | Совместный проход «редукция+non-regression» невозможен, пока ядро — not-ready-стаб без структурного task/plan/skills; guard стоит вниз от стаба | Пройти 14-assert guard можно только после реального structured-инференса ядра (P02/P10); перегнать после; держать row12 blocked до этого |
| Extension quality non-regression scorer (ExtensionQualityScoring / EvaluateQualityGate) | HUMAN-RUBRIC / EVAL-ONLY / DEAD-GATE | `ExtensionQualityScoring.cs:45-48` (нужна человеческая заметка на слой), `:66-78` `QualityRegressed`=вход, не измеряется; callers=`ExtensionEvalOnlyQualityRunner.cs`+`Quality02EvalOnlyRunner`+Tests; `TokenTraceAccounting.cs:87-88,179-201` null QualityScore/Regression ⇒ EvaluateQualityGate всегда fail | Авто-guard-а non-regression на токен-паре нет: рубрика ручная + rubric не подключён; heuristic quality-gate — вечный fail (null-поля) | Реализовать автоматический скорер качества пары (или подключить рубрику с утверждённым rubric + заполненными QualityScore/QualityRegression), связать с рантаймом Cursor, доказать, что EvaluateQualityGate может дать CanPass=true |
