# Пакет1212: принятие карточек и разрешение конфликтов

21 октября2026. Текущая работа — документы, без build/test/Train/Forward и без изменений старых результатов. Вход: [локальный пакет](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/2026-10-21-learning-delta-26332500/README.md>), planning commit `1212d2d6a8d6cf9d9755a83e0e53f7768dd62e44`. Прочитаны bytes всех26файлов:18Markdown и8JSON; JSON разобраны целиком, решения приняты по карточкам, промптам, source manifest, поправкам и адресным разделам доказательств. [Пофайловый учёт](source-intake.json) различает импорт и evidence. Это не полный повторный смысловой аудит174исторических/новых карточек и не воспроизведение результатов.

## Что применено

- Три карточки и три промпта теперь в действующих `experimets/cards` и `experimets/prompts`: [51HCL](../experimets/cards/HIERARCHICAL-LOCAL-HISTORY-01.md) → [52SVD](../experimets/cards/SVD-DIRECTED-TARGET-01.md) → [53CS01](../experimets/cards/ATTN-CAUSAL-SCORES-01.md). Index, JSON и output contract согласуются; старые позиции1–50 сохранены.53ID=14сохранённых/частичных attempts+39будущих проектов, не53готовых опытов.
- HCL/SVD: отменены `--run-id auto`, подбор следующего каталога и автоматический новый suffix после сбоя. Задан единственный `results/attempt-0001`, CreateNew claim и общий контракт30. Набор, native fits, query counts и одиночный фактор сохранены.
- CS01: документная копия [36source inputs](CS01-SOURCE-COPY-INPUTS.json); при чтении SHA256 всех36текущих native files совпали. Это проверка bytes, не build. Общий trained-start переносится в контракт33; consumer не повторяет acquisition.
- GRAD-ACCUM-COPY-01 и FROZEN-BROADCAST-MAP-01: один launcher `pwsh -NoProfile -File '<назначенный Run-With-Learning-Signal.ps1>'` во всех действующих владельцах. Требуется Windows x64 / PowerShell Core≥7, одна actualversion для обоих arms; до Train фиксируется actualexecutable/version. Get-Command обнаружил pwsh в этой документной сессии, совместимость будущегоhost этим не доказана. При отсутствии не устанавливать и не подменять shell.
- WIKI-FEATURE-KEYS-01 и WIKI-FEATURES-ONCE-01: оркестрацияD07, маршрутыWiki. Labels остаются классами одной задачи; данные иbudget не меняются.

## Новые указания Любы выше старого пакета

[Ожидание31](../experimets/31-tool-discovery-and-wait.md): не заканчивать ход на живом handle, не заменять ожидание новым waiter/monitor/polling и не придумывать summary scripts/manifests/агентов «пока ждём». Проверяются исходный handle+PID/start+exit, не имя dotnet.exe. Отсутствие log/notification не доказывает состояние или нулевой расход.

[Общий сохранённый старт33](../experimets/33-shared-learned-starts.md): фиксированные совместимые семейства, точные producer/consumer paths, наборы и полное состояние. B0 untrained не означает отсутствия всех trained donors; PA01 — частное принятие H16, не canonical promotion в training-artifacts. Текущее выполнение LC не прерывать/не перезапускать; сначала сохранить и проверить его eventualbundle, не назначать ещё1000шагов из-за этой редакции.

## Что сохранено как свидетельство, а не новый результат

N60: оба arms завершены, поздняя summaryfailure не отменяет Train; false-reject1→0 рядом с false-accept0→1. WM:2/4=constant-class, discriminationOPEN; WTvsWM не causal datasetexperiment. N63C/R: частичные положительные tokenрезультаты рядом с регрессиями и source/binaryограничением; fullresponseOPEN. H16donor1000 —3/40tokens и0/2fullresponse, не освоенный общий навык.

HCL/SVD — исторические реальные учащиеся алгоритмы; RP01consumer для них не установлен. CS01 — цена nativeSFT, ещё не измеренный выигрыш. E25/E43положительные исторические результаты сохранены; E09differentdenominators и E26BuildDefault ограничивают вывод. StackAndVersionself-oracle остаётся evidence без нового учебного опыта. Ни гипотеза, ни наличие файла не объявлены успешно выполненным экспериментом.

Приоритет: обучение и восстановление → конфликты → эти обновления → периферия. Входные26файлов не являются41Gitobjects из transportlog. Исходные источники не изменены.
