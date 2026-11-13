# Необученное начальное состояние и один контроль Student

Actual retained diagnostic завершился 14.10.2026 естественно: 84 prompt tokens,
256 generated tokens, `LENGTH_LIMIT`, generation error отсутствует; 35,59 секунды
по native summary. Output UTF-8 SHA-256:
`a6515a506f8bd46db1374c486df963c92316c1ff0024205135105bbc43f16e42`.
786 символов — реальный необученный output, качество не оценивалось. Прежние
config/tokenizer/snapshot/base/LoRA A/B/RNG hashes совпали до/после. 36 resource
samples: peak working set 729,247,744 bytes, CPU delta 33,515625 seconds.
Это наблюдаемая стоимость одного вызова, не новый лимит либо доказанный минимум.

Действующее указание пользователя 14.10.2026 отменило автоматический local Student
wall-clock timeout. `--repeat-retained-control --cold-start-reason <прежняя причина>`
разрешён для одного retained diagnostic: reader проверяет те же config/tokenizer,
owner START воспроизводит прежний snapshot hash до prediction; snapshot не
перезаписывается. Отмена только Ctrl+C/явный stop/process shutdown, resource samples
и heartbeat продолжаются до фактического завершения. Marker `repeat-preregistration.json`
запрещает автоматический следующий repeat. Старый 30s результат ниже — история.

`--verify-configuration --configuration-sha256 <pinned hash>` только читает реальный
reader и проверяет обычный Denied START, без model inference/artifact write. Публичный
reader wrapper возвращает `OperationResult`, внутренний server/bootstrap API сохранён.
Причина cold start ограничивается 512 символами без control chars до file effects.

Initial и retained control теперь используют initial-only discovery внутри START.
Любой checkpoint candidate даёт `REPOSITORY_OWNED_MODEL_INITIAL_CONTROL_CHECKPOINT_PRESENT`
до custody preparation и открытия manifest/DPO/weights. Snapshot открывается только
после clean initial admission. Обычный Generate сохраняет прежнюю загрузку accepted
checkpoint. Успешный diagnostic выше выполнен до этого guard repair; новая версия
проверена отдельным guard probe без повторного Student prediction.

Инструмент создаёт живые `config/code-documentation/generation-v1.json` и
`tokenizer.json`, затем вызывает production reader, repository-owned START и
`CodeDocumentationStudentGeneration.Generate`. Поддерживается только
`--initialize-control --cold-start-reason <действующая причина>`.
Config, snapshot и preregistration открываются для новой записи без перезаписи.
Пустой private каталог, оставшийся от первой pre-START ACL ошибки этого же tool,
допускает ограниченное восстановление DACL. Global security/ACL не меняются.

Student использует `ModelConfig.Small`: hidden 512, четыре слоя, 8/2 attention
heads, FFN 1408. Контекст 2048 и actual BPE vocabulary объявляются до prediction.
Canonical BPE обучается только на exact owner-input четырёх допущенных TRAIN семей;
Gold/rubric/DEV/SEALED bodies не читаются. Это подготовка tokenizer, не Student
update, DPO admission или платное происхождение эталона.

Обычный reader сохраняет `Denied`. Отдельный offline control копирует разрешённую
рабочим заданием cold-start policy с реальной причиной. Optional
`baseInitializationSeed` задаёт scoped initialization базы; null сохраняет legacy
global RNG. LoRA seed authority независима. Второе owner состояние создаётся до
prediction для проверки точного full-state воспроизведения, без второго inference.
Owner сохраняет используемый snapshot только в фиксированный
`.kiro/xiigen-slices/work/results/RP01.03A-initial-control-binding-20261014/private/initial.weights.bin`
через существующую custody boundary и `CreateNew`; caller не получает Model/handle.

Decoder: 256 новых токенов, temperature 0, top-k 0, top-p 1, seed 20261014,
максимум 4096 символов, verbatim owner-input без prefix. Preparation budget исходной
tokenizer initialization остаётся 600 секунд; Student Generate не получает automatic
deadline. Раз в секунду записываются процесс/весь PC/CPU time. VRAM, денежная стоимость
и полный disk-I/O не измеряются.

14.10.2026: actual BPE vocabulary 629; четыре TRAIN prompt lengths 84/100/103/107
с exact round-trip и резервом 256 помещаются в контекст. Полный actual checkpoint
47,899,519 bytes воспроизведён с hash
`a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911`.
Один diagnostic достиг deadline и вернул `CODE_DOCUMENTATION_GENERATION_CANCELLED`;
Text и complete token receipt отсутствуют. До/после full weights/base/A/B/RNG совпали.
Эта первая попытка не повторялась автоматически; позднее прямое указание пользователя
отменило лимит и отдельно разрешило один retained diagnostic.

```powershell
dotnet build tools/Rp01StudentControls/Rp01StudentControls.csproj -c Release -m:1 /nodeReuse:false
# Только новый явно разрешённый control; текущий snapshot после timeout не повторять.
dotnet tools/Rp01StudentControls/bin/Release/net9.0/Rp01StudentControls.dll --initialize-control --cold-start-reason "<реальная причина>"
```

Actual receipts и неизменённая preregistration находятся в указанном results
каталоге. Отменённый TRAIN diagnostic не является независимым экзаменом, P на старом
коде, useful learning point, accepted publication или ordinary user adoption.
Teacher/evaluator/CODEX, optimizer, DPO reserve/consume и publication не выполняются.
Полный RP01 и закрытые consumer/provenance bindings остаются открытыми.
