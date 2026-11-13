# Pass P27 — Реальность «экономии токенов» (token savings reality)

Фокус прохода: доказать/опровергнуть РЕАЛЬНУЮ экономию токенов у трёх расширений
как СИСТЕМУ: (a) accepted paired traces (baseline vs adaptive), (b) измеренная
редукция, (c) guard качества (non-regression), связывающий редукцию с качеством.
Оценка ведётся от ЖИВОГО кода `C:\Projects\llm_mvp_core`, не от копий пакета.

## Что прочитано в этом проходе (bounded)

- `43.SmartSkillsDomain/TokenTraceAccounting.cs` (целиком, 211 строк) — движок
  трейса/учёта токенов, паросверка, quality-gate.
- `43.SmartSkillsDomain/ExtensionQualityScoring.cs` (целиком, 191 строка) —
  рубричный скорер качества + token-comparison ветка.
- `43.SmartSkillsDomain/XiigenExtensionApiService.cs:620-699` — общий боундари
  всех трёх хостов; блок `non_claims` / `token_savings_claimed=false`.
- `43.SmartSkillsDomain/ExtensionEvalOnlyQualityRunner.cs:339-399` — единственный
  НЕ-тестовый вызыватель `ExtensionQualityScoring.Score`.
- Грепы вызывателей по всему репо: `CreateHeuristicTrace`, `ExtensionQualityScoring.Score`,
  `EvaluateQualityGate`, `TokenTraceAccounting`; грепы `token_savings|reduction|delta`
  по `43.SmartSkillsDomain` и по `XiigenExtensionApiService.cs`.
- Пакет `extension development`: грепы token-savings/paired evidence-папок
  (все — `*cursor-row12*`, Claude/Codex token-папок нет).

## Дедуп (что уже в мастере — НЕ повторяю)

- P01 фаза 13 (STUB/NEEDS_FIX «prior Cursor row12 evidence did not prove reduction»).
- P15 (Cursor-specific): `TokenTraceAccounting` эвристический и не вызван на
  поставляемом Cursor row12/TOKEN-03 пути; baseline↔adaptive runner без независимой
  переменной (Program.cs); 14-assert row12 guard связан с not-ready ядром; scorer =
  человеческая рубрика / dead-gate (null QualityScore/Regression).
- P06 (пакетный `measurement.json` MD5-identical / harness BLOCKED).

P27 отличается: смотрю на СИСТЕМУ token-savings как кросс-хостовую (Claude+Codex,
не только Cursor row12) и на КОРНЕВЫЕ дефекты СХЕМЫ/КОНТРАКТА в самом ядре, из-за
которых «accepted paired trace с редукцией + guard» структурно не может собраться
и никогда не исполняется вне тестов.

## Ключевые НОВЫЕ находки (с file:line)

### 1. Кросс-хостовый корень: общий core-сервис не эмитит НИ ОДНОГО токен-трейса ни для одного хоста
`XiigenExtensionApiService` — единственный общий боундари для Claude/Cursor/Codex —
НЕ вызывает ни `TokenTraceAccounting.CreateHeuristicTrace`, ни
`ExtensionQualityScoring.Score`. В сервисе только строковые НЕ-заявления:
`XiigenExtensionApiService.cs:632` `["token_savings_claimed"]=false`,
`:691` `"token_savings_claimed=false"`, `:664/:792/:816` текст non-claim.
Грепы вызывателей: `CreateHeuristicTrace` → только `Tests/ExtensionTokenTraceAccountingTests.cs`
+ одноразовая `docs/plans/evidence/.../measurement-20260625T004435/Program.cs:415`;
`ExtensionQualityScoring.Score` → только `ExtensionEvalOnlyQualityRunner.cs:371` + Tests.
Следствие: Claude и Codex-пути (идущие через тот же core) вообще не имеют учёта
токенов — не только Cursor row12 (это уточняет P15, который был Cursor-scoped).

### 2. В trace-подсистеме нет арифметики редукции, а схема НАВСЕГДА запрещает savings-claim
`TokenTraceAccounting` хранит `TokenCountInput/Output/Total`, но НИГДЕ не считает
дельту `baseline.TokenCountTotal - adaptive.TokenCountTotal` и не выводит
ratio/percent. `ValidateBaselineAdaptivePair` (`:135-177`) и `EvaluateQualityGate`
(`:179-201`) сверяют равенство полей и null-качество, но никогда не сравнивают счёт.
`TokenSavingsClaimed` захардкожен `false` (`:97`), а ЛЮБОЕ `true` — это ошибка
валидации `token_savings_claim_forbidden_in_trace_schema_phase` (`:161-164`).
То есть «trace schema phase» по построению не может выдать принятое число экономии —
пути ACCEPTANCE не существует. (P15 упоминал поле `TokenSavingsClaimed=false`; ново:
нет вычисления дельты вообще И валидатор активно запрещает сам claim.)

### 3. Валидатор пары требует ИДЕНТИЧНЫЙ `prompt_hash` → структурно запрещает input/context-reduction
`ValidateBaselineAdaptivePair:146` `RequireEqual(errors,"prompt_hash",baseline.PromptHash,adaptive.PromptHash)`.
Реальная экономия токенов — это сокращение/сжатие ВХОДА (контекста), отправляемого
модели; в adaptive-ноге вход обязан ОТЛИЧАТЬСЯ → другой `prompt_hash` → провал
`prompt_hash_must_match_between_baseline_and_adaptive`. Схема допускает различие
только на ВЫХОДЕ, тогда как экономия — про ВХОД/контекст. Это корень под
наблюдением P15 «нет независимой переменной»: даже корректный runner был бы
отвергнут ядром.

### 4. Учёт считает НЕ ТУ величину: user-prompt + видимый ответ XIIGen, а не токены к хостовой LLM
`ExtensionTokenTraceInput` содержит лишь `PromptText`/`VisibleAnswerText` (`:13-14`);
трейс считает `CountHeuristicTokens(PromptText)` и `CountHeuristicTokens(VisibleAnswerText)`
(`:68-69`). Нет понятия «токены контекста модели» / «размер downstream-запроса к
Claude/Cursor/Codex». Экономия = меньше токенов в ПЛАТНУЮ модель; здесь же меряется
длина пользовательского промпта и собственного текста XIIGen. Вместе с правилом
идентичного prompt_hash (находка 3) вход по счёту равен по построению, варьирует
лишь длина ответа — что не есть «savings». (Отдельно от P15 «эвристика вместо
токенизатора модели»: там — точность счётчика; тут — КАКОЙ текст вообще считается /
несовпадение целевой величины.)

### 5. Две подсистемы token+quality не сведены и никогда не соединяются в рантайме
Trace-продюсер всегда ставит `QualityRegression: null`
(`TokenTraceAccounting.cs:88`, `:36-37`); единственный НЕ-тестовый вызыватель
скорера передаёт `TokenComparison: null`
(`ExtensionEvalOnlyQualityRunner.cs:377`). Итог: «редукция + guard качества вместе»
не исполняется нигде вне тестов — ветка token-comparison скорера
(`ExtensionQualityScoring.cs:66-78`) МЁРТВА у каждого реального вызывателя.
Нет ни одного компонента, который на ОДНОМ прогоне: (a) считает токены,
(b) вычисляет редукцию, (c) меряет регресс качества.

### 6. Pass-путь скорера НЕ требует редукции и пропустил бы РОСТ токенов
Вердикт `ExtensionQualityScoring.Score` (`:95-110`) зависит только от layer-скоров и
`reasons`; `TokenComparison`/`tokenReduction` НИКОГДА не является условием Pass —
срабатывают лишь два защитных повода: `token_reduction_without_quality_scores_cannot_pass`
(только при `tokenReduction>0 && layerScores.Length==0`, `:69-72`) и
`token_reduction_cannot_pass_when_quality_regresses` (при `QualityRegressed`, `:74-77`).
Пакет с `AdaptiveTokenTotal >= BaselineTokenTotal` (нулевая/отрицательная экономия)
при полном наборе слоёв ≥4 и `QualityRegressed=false` возвращает **Pass**. То есть у
«accepted paired traces + reduction» нет позитивного гейта, что редукция реально
достигнута. (P15 gap-4 — про dead-null gate; тут — про отсутствие позитивного
требования редукции и терпимость к росту токенов.)

## Вывод P27

Реальной СИСТЕМЫ экономии токенов нет ни на одном из трёх хостов. Есть два
несведённых полу-механизма в ядре (heuristic-trace без арифметики редукции и без
пути claim; рубричный скорер, чья token-ветка мертва у всех реальных вызывателей),
общий сервис вообще не эмитит трейсов, а схема пары ядра структурно запрещает
input-reduction и запрещает savings-claim. «Accepted paired traces + reduction +
quality guard» как связная возможность не исполняется вне тестов.

Уровни зрелости: находки 1-6 — BUILT_NOT_PROVEN/STUB/MISSING; ни одна не PROVEN.
Все имеют artifact/file:line. Claude/Codex token-savings evidence — MISSING
(not found: в пакете только `*cursor-row12*` token-папки).
