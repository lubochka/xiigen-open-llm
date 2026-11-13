# Независимая проверка двух PromptToTask гипотез

19 октября 2026. Reviewer: code-review subagent. **Общий результат: обе причинные цепочки подтверждаются исходником; ниже уточнения обязательной области действия.** Чужие source/probe/report файлы не изменялись. Native C#, продуктовый training и внешний judge не запускались.

## Что проверено

Прочитаны относящиеся к выводу record contracts, весь механизм nearest/signals, `ContinueWithAcceptedPattern`, `Translate`, fallback, загрузка и handoff обычного API. Для трёх исходников через GitHub повторно получены blob SHA на core `44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76`; они совпали с байтами в parent probe receipt:

| Файл | Git blob SHA |
|---|---|
| `PromptToTaskTaskPatternUnit.cs` | `f87546449cd403f29409b73e1464998972b177eb` |
| `PromptToTaskMachine.cs` | `ffaeefef52d0456295c225554b581acda1529983` |
| `XiigenExtensionApiService.cs` | `a59cdcdd830c4acb70c30ba82626155ee322a4df` |

Parent script повторно исполнен без изменений, output записан отдельно: [prompt_pattern_independent_rerun.json](prompt_pattern_independent_rerun.json). Получены duplicate margin0, `ready→notReady`, для шести ASCII alias fixtures A2/6 и B6/6. Это **повтор расчёта того же source proxy**, не независимый новый эксперимент обучения и не новая статистическая выборка.

## 1. Повтор положительного semantic pattern снижает готовность

**Подтверждено.** [Continue, 263–301][continue] проверяет наличие pattern, acceptedRefId и текущего loaded state, затем без content-dedup добавляет pattern через `Concat`. `_importedLearnedState` становится новым текущим состоянием. Этот код не использует C# record equality: утверждение не зависит от того, совпадают ли ссылки на списки.

[Decide, 174–206][margin] сравнивает top1 и top2 **по отдельным pattern records**, а не по различающимся результатам задачи. У двух записей с одинаковыми normalized signal sets и Confidence scores равны. При обычном положительном `MinimumTopTwoMargin=0.01` одна запись имеет margin0.9, две — margin0. Это нормальные допустимые данные; разные PatternId не устраняют tie. [Scoring, 507–533][score] не читает TaskType, requirements или forbidden actions при вычислении score, поэтому одинаковый semantic output второй записи не признаётся дополнительной поддержкой первого.

**Точность семантической эквивалентности.** Для native минимального примера достаточно `p2 = p1 with { PatternId = "p2" }` и отдельного acceptedRefId. Значения всех остальных полей тогда сохраняются. В предлагаемом объединении нельзя ограничиваться TaskType или одинаковым signal set. Семантический payload включает ExplicitUserRequirements, Constraints, Deliverables, ForbiddenActions, RequiredHosts, EvidenceNeeds; SplitId также входит в выдаваемый контракт — [record, 23–34][pattern-record], [выдача, 183–215][emit]. Идентификаторы/accepted provenance необходимо сохранить отдельно. Без equality всех существенных полей реальные противоречия могут быть ошибочно объединены. Parent proxy это оговаривает через composite `action`, но не строит настоящий C# record: его результат нужно так и описать.

**Контроль неоднозначности корректен.** Parent counterfactual сохраняет два конфликтующих action payload при одинаковых сигналах; gate остаётся notReady. Он не предлагает глобально обнулить margin threshold. Также не следует называть повтор `accepted` доказательством внешнего semantic approval: в source это API argument и reference, а не выполненный в этой сессии judge.

**Последующий эффект.** `Translate` использует тот же загруженный state и при notReady вызывает fallback — [117–180][translate]. При request без API boundary keys вернётся `loaded_state_no_matching_learned_pattern`; при штатных boundary keys выдаётся `baseline_internal_universal_task`. Это замена ранее выданных learned requirements/hosts на общий baseline contract, а не удаление patterns из памяти и не прекращение всякого API ответа. Fallback явно отмечен `BASELINE_INTERNAL_FALLBACK_USED`, `SEMANTIC_QUALITY_CLAIMED_FALSE` — [461–527][fallback]. Называть его скрытой заявкой на measured semantic success было бы неверно.

## 2. Подстрока gate создаёт missing_gates, включая completed/delegated контекст

**Подтверждено.** [Alias, 462–464][aliases] отдельно добавляет `missing_gates` как для `missing gate`, так и для просто `gate`. Helper использует `normalizedPrompt.Contains(phrase, Ordinal)`, без границ слова и без проверки отрицания/состояния — [497–504][alias-helper]. Поэтому `The gate is complete.` получает признак отсутствующего gate, а `The delegated review is complete.` получает его даже без отдельного слова gate. Выбор `repair_missing_gate` доказан в parent constructed state с confidence0.95 против0.9; для произвольного shipped learned state такой выбор **не установлен**, потому что scoring зависит от его полного набора сигналов и порогов.

**Формулировки примеров.** Исполненные строки содержат `complete`, а не отдельный токен `completed`. Сам факт ложного `missing_gates` верен и для текста с completed gate, но рассчитанные A2/6→B6/6 относятся только к шести сохранённым точным строкам. Нельзя незаметно менять формулировки examples и переносить на них числа.

**Сфера authority.** Alias применяется к AuthorityTexts, а не безусловно ко всякому архивному тексту RawPrompt: [BuildQuery/ExtractPromptSignals, 327–365][authority-query]; [ExtractAuthorityText, 368–410][authority]. Evidence-only blocks могут быть удалены. Для эксперимента запрос должен быть обычным текущим неподкавыченным instruction либо корректным `current_unquoted_user_instruction`. Это не пример prompt injection и не доказательство, что процитированный старый план становится authority.

**Предел исправления.** Удаление только broad `gate→missing_gates` — честная однофакторная диагностическая интервенция. Она сохраняет положительный контроль `Repair the missing gate.` благодаря более точному alias. Однако не является доказанно полной заменой semantic extraction: `gate is absent` может потерять useful recall, а `no missing gate` всё ещё содержит literal `missing gate`. Эти два случая следует включить в будущий domain exam как counterevidence. Не добавлять все новые aliases в одно плечо и не называть source proxy универсальным улучшением понимания английского/планирования.

## 3. Уточнение API reachability и loaded state

Для факта внутри PromptToTask нужен **загруженный PTT state** с рассмотренными паттернами. При `learnedState is null` Machine возвращает `not_trained_no_loaded_state` до `Decide` — [117–130][translate]. Последующий API опыт должен доказать exact selected checkpoint/manifest и реальный экземпляр машины, потому что `TargetLocator` может загрузить другой user-adaptive state.

Но **`AllLoaded=true` не является обязательным guard до PromptToTask в прочитанном коде**. Без TargetLocator [ResolveRequestLoadState, 284–288][resolve] возвращает `_commonAssetLoadState` без такого условия. [ProcessHostExtensionRequest, 121–136][api-call] сначала вызывает `PromptToTaskMachine.Translate`, затем `OrchestrationReasoningMachine.Plan`. `LoadCommonAssets` записывает отдельные ошибки загрузки и общий AllLoaded, сохраняя независимо успешно загруженную PTT машину — [1408–1444][load]. Поэтому отказ соседнего OrchestrationReasoning v1 может испортить дальнейший end-to-end/aggregate status, но сам по себе не доказывает, что PTT.Decide вообще не вызвался.

Штатный `BuildPromptContextFields` добавляет `xiigen_method` и `raw_prompt_redacted` — [1096–1104][api-context]. Эти ключи достаточны для `extension_api_boundary_present` и других generic signals — [530–560][generic]. При загруженном PTT, прошедшем request validation и рассмотренном tie, предсказание внутреннего fallback обосновано исходником. Это ещё не native receipt default installation: актуальное сочетание assets/loader/request должно быть показано отдельно.

Безопасный порядок протокола: (1) вывести selected PTT state/checkpoint IDs; (2) провести same-request direct Unit/Machine до и после одного accepted append; (3) подтвердить прохождение обычного API тем же state и показать поля structured task; (4) отдельно объяснить возможные ошибки соседних learners/aggregate status. Нельзя исправлять N52 provenance/schema admission лишь ради обхода precondition этого опыта.

## 4. Новизна

Проверены parent corpus из 661 Markdown и дополнительно 12 refresh Markdown. Exact terms `PromptToTask`, `missing_gates`, `ContinueWithAcceptedPattern`, `TopTwoMargin` не обнаружили этих конкретных механизмов в исходных 661. В refresh четыре упоминания PromptToTask относятся к соседнему asset/index и aggregate availability, а не к duplicate agreement или gate alias. Сходные общие темы tie/nearest/abstention существуют в Wiki и KNN; заявлять открытие общего принципа нельзя. Два новых owner-specific механизма относительно просмотренных reports поддерживаются.

**Рекомендация parent:** публиковать как две новые source-backed гипотезы с вышеописанными пределами и `SOURCE_BOUND_PYTHON_CONTRACT_PROBE`, не как фактическое восстановление external_domains, результат native learning или доказанную default-production регрессию. Конкретный callable PTT путь сильнее абстрактного API counterexample, но требуется показать реальный loaded state.

[continue]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskMachine.cs#L263-L301
[pattern-record]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskMachine.cs#L23-L34
[margin]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs#L174-L206
[score]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs#L507-L533
[emit]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskMachine.cs#L183-L215
[translate]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskMachine.cs#L117-L180
[fallback]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskMachine.cs#L461-L527
[generic]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskMachine.cs#L530-L560
[aliases]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs#L462-L464
[alias-helper]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs#L497-L504
[authority-query]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs#L327-L365
[authority]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/PromptToTaskTaskPatternUnit.cs#L368-L410
[api-call]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/XiigenExtensionApiService.cs#L121-L136
[api-context]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/XiigenExtensionApiService.cs#L1096-L1104
[resolve]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/XiigenExtensionApiService.cs#L284-L288
[load]: https://github.com/lubochka/llm_mvp_core/blob/44507823fc5aaeab4c3f0fe4e9da9ce924c6ec76/43.SmartSkillsDomain/XiigenExtensionApiService.cs#L1408-L1444
