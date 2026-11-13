# N41-T1 — четыре старых переводческих Unit против шести новых: несовместимость cold restore

<!-- POST117-CURRENT-2026-11-03:START -->
## После P117: восстановление должно читать фактически выгруженную память

[текущая сверка после P117](codex-2026-11-03-POST117-TRANSFER-AND-EXPERIMENT-AUDIT.md) сохраняет узкий P117 reload: actual executor maps и 3 ответа после import совпали. Но KNN-файл построен из ExpectedKnnWrites и проверен только поCount; это не полная проверка live store и не product TranslationMachine migration4→6. Нельзя объявлять этот исторический контракт закрытым из трёх ответов копии.

Для отдельно назначенной приёмки экспортируется реальный store, manifest связывает direction/schema/все записи, порядок, кратности и target/source. Хороший контроль сохраняет существующие четыре компетенции и честно пустые две новые; same-count подмена target/signature или неправильное направление отвергается. Новый обычный reader должен загрузиться из тех же байтов без нового Train. В source-linked host P117 internal методы доступны, поэтому повторное обучение ради восстановления не объясняется их недоступностью.

При конфликте публикации выбранный ремонт обязан сохранить именно подготовленный кандидат: BuildCheckpoint с его version/runtime и атомарный writer готового payload возвращают проверяемый locator. Вызов обычного SaveCheckpointAsync после конфликта читает CurrentVersion победителя и не сохраняет проигравший кандидат. Persistence failure называется отдельно и оставляет приёмку незакрытой; сообщение о потере не заменяет сохранность. Artifact hash проверяет записанные bytes, state parity не зависит от нового ExportedAtUtc. Native migration и interleaving здесь не исполнялись. Ниже историческое исследование 18.10 с его исходными статусами.
<!-- POST117-CURRENT-2026-11-03:END -->


18.10.2026. **NEW independent cross-branch source finding / NATIVE NOT_RUN / neural RP01 causal status UNKNOWN.** Это **другая** ошибка, чем уже описанный в каталоге разрыв orchestration guide-checkpoint v1→v2. Объект здесь: именно TranslationMachine / прежнее успешное состояние перевода EN→RU, RU→EN, HE→EN, EN→HE. ЗАЩИЩЁННЫЙ core research — только источник.

## 1. Источник доказательства (два pinned branch blobs)

| Контракт | PrepareToGPL | codex/dpo-implementation |
| --- | --- | --- |
| Создание переводческих исполнителей | [TranslationMachine.cs L557–562](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/27.TranslationDomain/TranslationMachine.cs#L557-L562): 4 направления RU_EN, EN_RU, HE_EN, EN_HE | [TranslationMachine.cs L716–723](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L716-L723): добавлены ES_EN, EN_ES; 6 исполнителей |
| Запись snapshot | [L281–296](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/27.TranslationDomain/TranslationMachine.cs#L281-L296): Executors берутся из _runtime.Executors.Values | [current L290–305](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L290-L305): та же семантика, но runtime содержит 6 направлений |
| Приём snapshot | [old Import L299–350](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/27.TranslationDomain/TranslationMachine.cs#L299-L350): проверка наличия направлений старого runtime | [new ValidateCheckpointStructure L405–432](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L405-L432): вычисляет missingDirections = runtime.Executors.Keys.Except(checkpoint.Executors...) и при непустом результате возвращает **EXECUTOR_DIRECTIONS_INCOMPLETE** |
| Порядок проверки | Старый Import начинает частичную мутацию раньше полного проверки; отдельный риск | [current Import L333–350](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationMachine.cs#L333-L350) сначала вызывает ValidateCheckpointStructure; это улучшение атомарности, его нельзя удалять ради совместимости |
| Реальный обучаемый Executor | [old TranslationTrainedUnits.cs](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L455) | [current тот же код](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L455), совпадает полный blob SHA **1742abfc9fb617dfc66e4d618e0c169da01a6ccc** |

**Новая причинная гипотеза.** Snapshot старой TranslationMachine, легально обученный на EN→RU, может быть отвергнут новой TranslationMachine не из-за потери памяти, а потому что в старом checkpoint **не могли присутствовать** новые ES_EN / EN_ES executors. При ручной подмене пустым состоянием обычный consumer будет отвечать хуже или bootstrap. **Только первая часть (разный contract и проверка) подтверждена кодом; факт исполнения данного old checkpoint в RP01 неизвестен.** Не путать с другим good model orchestration-reasoning v1 в [40-PREPARETOGPL chapter](40-PREPARETOGPL-RECOVERY-PRIORITY-CHAPTER-2026-10-18.md), где иные owner, schema и правила provenance.

## 2. Минимальный эксперимент — MIG-TRANSLATE-01

**Домен:** перевод. **Профили:** EN→RU (первичный), RU→EN, HE→EN; контроль для новых ES→EN/EN→ES. **Получатель:** persistent TranslationMachine Executor memory; не Student LoRA, не KNN model manifest.

Baseline A — на **PrepareToGPL** построить свежую TranslationMachine; через настоящий TrainStructuredBatchAsync дать 2–8 разрешённых pairs с явно заданными phrase mappings, затем сохранить v1 JSON обычным SaveCheckpointAsync и записать 2–4 обычных ответа. Это только small private fixture, не старые неаттестованные источники.

B — импортировать **те же bytes** A в свежий нынешний TranslationMachine и записать **фактический** IsSuccess/ErrorCode, список missingDirections; при ошибке не выдавать fallback за сохранённый ответ. C — current-native шестинаправленный checkpoint, обученный **теми же входами**, загрузить cold в current и убедиться, что importer и ordinary EN→RU Flow работают. D — только если B подтвердил несовместимость, исследовать версионированный read-only migration adapter: старые четыре состояния сохранять 1:1, два новых явно создавать как пустые и отмечать MIGRATED_EMPTY, оставляя текущий preflight validation; если нет разрешения менять runtime, D — DESIGN_ONLY.

**Изолирующее вмешательство:** сравнить B и D при **идентичном историческом checkpoint**, а не одновременно менять словарь/phrase parser. Снимок learned EN→RU по полям exact, phrase, word, KNN должен быть побайтно сопоставим после учёта timestamp. Ошибка unknown direction, duplicate direction, null map и tampered bytes обязана по-прежнему отклоняться, **без частичной мутации**.

**Первичные outcome:** load_success, error_code, recovered #units, learned entries hash per direction, post-cold correct/all, wrong-accepted/all, coverage, score on exact/near/composition/negation. Время load/answer ms, process peak RSS, checkpoint bytes, Unit/Flow route/depth. Neural layers unchanged/not applicable. Для старых 4 направлений никакое correctness ухудшение недопустимо в D при равных inputs; для новых 2 допустима честная пустая память (не выдумывать прежнюю компетенцию).

**Критерий причинного подтверждения:** A и C корректно отвечают; B реально возвращает EXECUTOR_DIRECTIONS_INCOMPLETE; D проходит импорт и тот же cold ordinary Flow без потери 4 старых trained states. При другой ошибке — гипотеза данного seam фальсифицирована/неполна; investigate actual import order and asset locator. Если old checkpoint вообще не используется в продукте, это **compatibility debt**, а не объяснение RP01.

## 3. Почему это не готовая правка и не повтор прежних карточек

Соседние existing P25 холодный restore, P117 shared direction namespace и P53 batch atomicity исследуют **другие** условия; здесь триада **старое количество направлений → новый strict importer → потеря доступа к прежней памяти**. Никаких production changes не вносилось; это проект одного native A/B/C/D эксперимента. Не ослаблять проверку целостности, не давать checkpoint с запрещёнными примерами обходить source admission, не изменять архивные результаты.
