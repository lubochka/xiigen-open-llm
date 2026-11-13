# Кандидат: использовать сохранённое обученное состояние в малых проверках

**Статус: полезный измеренный результат принят в список кандидатов; production не менялся.** Это документирование уже сохранённого `RESTORE-MINIMAL-SET`, не новый запуск и не утверждение о неисправности native restore.

## Что произошло

На двух TRAIN строках C# модель получила1000 SFT updates, затем сохранила weights и AdamW m/v/step. Новый объект с другой исходной базой загрузил state и продолжил200updates. В raw `weightsRestoredBitExact=true`, `optimizerImported=true`, step1000→1000→1200. Поля обеих5строчных панелей после prior и после restore совпадают при прочтении raw; код флага `panelIdenticalAfterRestore` проверяет только две агрегированные величины, поэтому сам этот флаг не подменяет полную сверку. Источник: [Save→fresh Load→Import→continue](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs#L73-L130) и [raw](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/restore-minimal-set.json#L69-L363). Без непрерывного контрольного плеча это не доказательство эквивалентности continued/uninterrupted обучения.

| Показатель | Восстановленный step1000 | После200 новых updates | Изменение |
|---|---:|---:|---:|
|Правильные TRAIN response tokens|3/40|4/40|+1 token;7.5%→10.0%.|
|TRAIN response mean NLL|4.766696420187372|4.711598363502413|−0.055098056684959.|
|TRAIN mean gold rank|51.825|50.05|−1.775.|
|Полностью точные TRAIN ответы|0/2|0/2|Полный ответ не освоен.|
|Правильные EXAM response tokens|0/43|0/43|Ненулевая EXAM точность не показана.|
|EXAM Three response NLL|5.527751288639532|5.4992039045038466|−0.0285473841356854.|
|EXAM Four response NLL|5.769119468000703|5.677866738414823|−0.09125272958588.|
|CONTROL Pos response NLL|5.8079633717111525|5.857194149707198|**+0.0492307779960455 — ухудшение**.|
|CONTROL Pos gold rank|95.85714285714286|100.5|**+4.64285714285714 — ухудшение**.|

Любое измеренное полезное улучшение считается успехом своей метрики и домена. Здесь улучшение TRAIN реально есть при ненулевых правильных tokens до и после; ухудшение CONTROL не скрыто. Владение C#, полные ответы, free generation, EXAM-generalization, скорость и причинный эффект восстановления этим не доказаны. Это маленькая частично обученная исследовательская модель; не «почти готовый» продуктовый Student.

## Что сохранено и пригодно для рассмотрения

**Донор находится на step1000, не1200.** [Save](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs#L94-L97) расположен до цикла продолжения; после него повторного Save нет.

| Артефакт | Точный путь и published object |
|---|---|
|Weights|[trained-weights.bin](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/trained-weights.bin); LFS SHA256 `ba8ec8c20250bc431da4bf9993ff22c508422cd7224b5c57cec33ce7b98c6e0e`,43500bytes.|
|Optimizer|[trained-optimizer.bin](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/trained-optimizer.bin); LFS SHA256 `603cf13423f7c6ae5af073a4ea824f49009175b145ce551f6c1b5cf59b66842a`,1112bytes.|
|Измерения|[restore-minimal-set.json](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/results/restore-minimal-set.json); Git blob `d66acf8882b781336c5a131c4e781055d4fe4b1e`, SHA256 `bda100b0109e9024b2ca6ee3ddaf563ea81ddc16258910c1a7316f2d10e56e2b`.|

В этом аудите проверены Git LFS pointers и raw; сами бинарные LFS objects не скачивались и новый load не выполнялся. Доступность этих objects будущему исполнителю должна проверяться адресно при материализации, без переобучения1000steps как скрытого fallback.

Топология: vocab256, context64, hidden16,1layer,2heads,1KVhead, FFN32; q/v LoRA rank2/alpha4/dropout0; AdamW LR0.0003, weightDecay0; byte BPE без merges; данные и framing `// file: <name>`+LF+source — [точные настройки](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs#L36-L71). Нельзя загрузить этот state в saved profile629/512/4layers или объявить донором другого домена. Tokenizer в данном инструменте пересоздаётся из fixed byte configuration; отдельного tokenizer checkpoint здесь не сохранено.

## Кандидат изменения процесса экспериментов

Повторно чинить `LlmModel.SaveCheckpoint/LoadCheckpoint` не требуется по этому результату: здесь native механизм сработал. Кандидат — **переиспользовать точный исследовательский donor и его optimizer для будущих совместимых малых опытов**, вместо нового random B0 на каждом запуске. Исходный№08 остаётся сохранённым; повтор не назначен.

Если отдельно будет выбрана версия host с warm donor, точка изменения — экспериментальная копия [FrozenFactory](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/N63I/Program.cs#L358-L378): сейчас она Load bare base → ApplySeeded → new AdamW, то есть предназначена для холодного состояния. Для warm state нужен порядок из уже работающего restore-инструмента: создать ту же конфигурацию → установить q/v topology → Load full weights → импортировать portable m/v/step на новые named bindings. Не обнулять optimizer, не переустанавливать adapter послеLoad, не грузить state в production canonical registry. Послеloader общий источник в A/B одинаков; admission остаётся единственным A/B фактором. Изменение порядка загрузки проверяется в новой экспериментальной копии, не в original коде.

Эта кандидатная запись не задаёт новых runtime-команд или бюджета. Для разрешённого будущего ID автор обязан назначить один host/entry/result, actual LFS materialization, положительные TRAIN anchors и оплаченные readouts до начала исполнения. Доступного точного binary нет — `PREPARATION_REQUIRED`, без замены модели, полного CI или запуска1000trainingsteps. Изменять EXAM для получения успеха запрещено.

## Роли, отчёт и немедленный переход

Исполнитель — default модель его сессии. CODEX/Astra6ultracode выполняет выбор и качество DPO; внешний открытый китайский teacher генерирует DPO. В этом сохранённом SFT инструменте внешних DPO calls нет. Русский отчёт показывает source→learning→state→consumer, до/после, полезные оси и регрессии; контроль ухудшения обязателен. Любое прямое «заканчивай и переходи» прекращает оформление прошлого; «остановить всё» прекращает всю работу. Это правило нельзя обходить hashes, review, Git или повтором ради красивого отчёта.
