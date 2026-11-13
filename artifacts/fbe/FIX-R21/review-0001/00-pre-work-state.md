# Состояние до начала проверки (независимый проверяющий, review-0001)

Смысл файла: зафиксировать вершину ветки и отпечатки всех файлов, которые я собираюсь читать и которые по условиям наряда не должны измениться. Этот файл записан ДО чтения проверяемого кода и улик. Если в конце проверки отпечатки совпадут с записанными здесь — я ничего не менял.

## Вершина ветки

- Рабочий каталог: `C:\Projects\llm_mvp_core`
- Ветка: `codex/dpo-implementation`
- Вершина (HEAD): `2f710d2b445a79ffc7d51dcd22c1b8956a852496`
- Совпадает с коммитом, названным в наряде: да.
- Рабочее дерево чисто: `git status --porcelain` вернул 0 строк.

Доказательство: `git rev-parse HEAD` -> `2f710d2b445a79ffc7d51dcd22c1b8956a852496`; `git rev-parse --abbrev-ref HEAD` -> `codex/dpo-implementation`; `git status --porcelain | wc -l` -> `0`.

## Состав проверяемого коммита

Автор: Luba Karpenko; дата: Tue Nov 10 07:22:45 2026 +0200; 35 файлов, 1427 вставок, 0 удалений (только новые файлы).

Доказательство: `git show --stat 2f710d2b445...` -> `35 files changed, 1427 insertions(+)`; список: 1 файл проверок `Tests/ExperimentFixes/MethodKnnTieReachedThroughFacadeTests.cs` (338 строк) + 34 файла улик под `artifacts/fbe/FIX-R21/attempt-0001/`.

Вывод из состава: в коммите НЕТ изменений механизма продукта, фасада, ранее принятого файла проверок, реестра, карточек, навыков, файлов агента, `.gitignore`, `.gitattributes`. Это подтверждается самим списком файлов коммита (только добавления, 35 путей). Отдельная проверка истории — в файле `05-untouched-files.md`.

## Отпечатки (SHA-256) файлов, которые я буду читать и которые не должны меняться

| Файл | SHA-256 рабочего дерева |
|---|---|
| Tests/ExperimentFixes/MethodKnnTieReachedThroughFacadeTests.cs | 3bb7998dc2b6e2e981c934fde5ab8a7c58ddcd12f9305043b01c88dfc239fa98 |
| 12.SemanticCompiler/Prediction/MethodKnn.cs | 2bb6349423309ad3845e0e3131f120d7724f1e5e16f782c6968067744525a69a |
| 12.SemanticCompiler/SemanticCompilerFacade.cs | 54b8b6d8abe92b21df32cbf230e6d19ff46f43061af4af145d21db1075d4e184 |
| Tests/ExperimentFixes/MethodKnnTieIsResolvedByDistanceTests.cs | fad1b402672f00809ebbff3cd24ac958bb2fd387e5d45a1d041d6a88810f3ec3 |
| 12.SemanticCompiler/SemanticCompiler.csproj | 8633f619eb2b9d249b687993346e76908d2319945ca16a1f8bccce164e470ab9 |
| Tests/LlmFromScratch.Tests.csproj | fc687de159b473c71e3976e3d207d4c115a0628bd94479bf93b206905db3d62c |
| docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md | c6e8eb65265de613da4370ed4470e2a673c462455ea47bd3112ec4c8983e6144 |
| .gitignore | 86545cb001e5e0e4178f7b2c5bf7515a048ce4e63e39f32df2b67921018fe1e3 |
| .gitattributes | c6b337aa4d926985804f34e03acd66cca38921f11980ba51e693df714433dafd |

Доказательство: `sha256sum <пути>` в корне хранилища, вывод выше дословно.

## Отпечатки 34 файлов улик исполнителя (SHA-256, размер в байтах, путь относительно attempt-0001)

```text
cb9c00a90dc95ff82fd70d1caf1c5b2be0cb486cf056d7c9ca9882a824dbe5ae  22425  ./acceptance-record.md
583156d17debad4e348153262edd489ab4100cfe3cb5452803b3668561171d85  17526  ./expectation-before-run.md
8c0969f2e34f33ba959d0169643127c1b291fb3bfa7fd481a12d2b7bebf09eb8  3255  ./pre-work-fingerprints.md
3b1549cf2dfc60bb70f11399e1253a6572cfa25f1f9df0e240e84fd87bde9e78  2505  ./run-baseline.sh
79cdeb286d702badc4df348b39bd9e15c14b2a9c9080239d9bb61817bf5c0400  5524  ./run-mutation.sh
cbc96ba2e44242e4fdc9b0d5cbe498e5bcc2b0184dd192f39e8ca82ce788c297  12957  ./runs/after-restore/after-restore-both-families.trx
8e44930405658d608f701ceebdc2b7068a33276853ed02a7aedbbe8cdf4aebc6  142  ./runs/after-restore/commands.txt
ea7080f60f03888884f2cafee4df2915b92aa6e57dd6d2ada1be3b658ee49ef5  4266  ./runs/after-restore/r1-build-product.log
9236c19deb2ed4e7d8be781b8433d783befd243f73b8a123fcbd81d1044f2536  146783  ./runs/after-restore/r2-build-tests.log
08d324c76bc4e3df9565b13bc5726f73be3ed73dd4609003ac8d90895b4c2fe1  425  ./runs/after-restore/r3-both-families.log
fd2532e198fe00231ca40844c35d1894da7bb8a2e4ef22cc521650de9ddfd7af  8625  ./runs/baseline/baseline.trx
bc9096084ca4a4ddb4599096a96503198ecc1578467b83605288d69f356aab0a  335  ./runs/baseline/commands.txt
60f4933e319640c9cc79d95150f2dad4e189ea9fe8f23b209eef50713e2178e0  382  ./runs/baseline/step1b-build-product-corrected.log
b396d78fea04806c1863d4741b44314e41b86ec432d7b65ad82ede55e20fec3a  111  ./runs/baseline/step1-build-product.log
8027e070dd65c196365488473de94dae2aa848cf9f4a590384c0ba762dbcf79e  151355  ./runs/baseline/step2-build-tests.log
55f3b459e68138cf6abe54feacd03daa58c3b02c0a700ffe54fff4e734821c02  786  ./runs/baseline/step3-list-tests.log
7006c629893e64829e8f79f73da49a01afe02d8834af9cc296b774431839ef23  401  ./runs/baseline/step4-test-run.log
0ecc88e51343d9938e0a438301b27396af0f38e83ce708dae6ff350f15ab5313  4266  ./runs/mutation/m1-build-product.log
9ae2242d37942e15d37dd145c1b03d7d43ac478ab49ea5ef8b507db2bfcbc7cd  146783  ./runs/mutation/m2-build-tests.log
2816f70e0acda38a77c2a4906cefd44f58811575e81a8de40cbdca91775368d0  1362  ./runs/mutation/m3-new-tests-under-mutation.log
bb9bfcf76543948026ce9fa7b916211d2f9127736c6e7427d9cfcf9a00fe88a6  3235  ./runs/mutation/m4-existing-tests-under-mutation.log
ce2150a3cb0c022adc7054ff21779a2b1fc291fec43a1cb9a657867c1891f351  14619  ./runs/mutation/mutant-existing-tests.trx
90834480bf804a6289ec0781a206d6a92f89014ba5ba59ef79727200e5ab1f1f  7976  ./runs/mutation/mutant-new-tests.trx
9e1718e8fb9d6812a43028f52824c3d51a1b87f547ecd0379fbf05d7e365cae2  1052  ./runs/mutation/mutation-journal.txt
3a8514e0b69cf4d20a7334928df4283c2e5e3702834bb533a60afdf9b15b2e63  207  ./runs/new-tests/commands.txt
e48e8d83be3b9d686ff7c0024eb68c195fcba207e767b32d2850c830df0cdd9f  11265  ./runs/new-tests/new-tests-after-fixture-repair.trx
dc8db016663b12f8a1bf2c1a684be24c08c23809d36d0c84b9555d9784bc58d5  5945  ./runs/new-tests/new-tests-green.trx
237ebd09ed5c819f0fad0049f8582854839c5b6c7cca9fa79b46cc2f38dfc5ef  100771  ./runs/new-tests/step1-build-tests.log
d640a6a43b706f68c95bd94b1213afd35929ad71e199532b07c9c2aa8d194e75  574  ./runs/new-tests/step2-list-tests.log
2185d7946ea93bc1fc393b59b81bd1d8ff73ce998004341a64a481432e03c0e5  3679  ./runs/new-tests/step3-test-run.log
e8bd65deb0aa76b389a86d3a312af1677fc9421143d827962a2675836c7582f5  100771  ./runs/new-tests/step4-build-after-fixture-repair.log
6d1ca6259aa37aea40fadd8045a270d73a14ae877e8900f2d0dd9d021684ba23  3086  ./runs/new-tests/step5-test-run-after-repair.log
a0f3def04a9931e4a23f7a3721984d3b2affcfc4ed81da62753541bca41838aa  100771  ./runs/new-tests/step6-build-after-marker-repair.log
6a66aa9a324e663cd49b0c2fc792c0f3aade14c0754fe28af2912c963deaf457  540  ./runs/new-tests/step7-test-run-green.log
```

Доказательство: `find . -type f | sort | while read f; do sha256sum; wc -c; done` в каталоге `artifacts/fbe/FIX-R21/attempt-0001`; 34 строки, что совпадает с `find ... -type f | wc -l` -> `34`.

Наблюдение сразу при фиксации (без вывода, только факт): три журнала сборки тестов `step1-build-tests.log`, `step4-build-after-fixture-repair.log`, `step6-build-after-marker-repair.log` имеют одинаковый размер 100771 байт, но разные отпечатки. Это нормально для трёх последовательных сборок одного и того же проекта (разные временные метки внутри), но будет проверено в `04-evidence-integrity.md`.

## Идентификаторы объектов git (blob) этих файлов в коммите

Записаны для сверки «рабочее дерево = коммит»: `git ls-tree -r 2f710d2b445...` вернул 35 blob-идентификаторов (1 файл проверок + 34 улики). Полный список я не дублирую здесь, он воспроизводим одной читающей командой; ключевой: файл проверок -> blob `8af569f5ea5cacdee584bfe9b8cdcc7d5d5e8b06`.

## Атрибуты git для улик

`git check-attr -a` по `.trx` -> `diff: set`, `text: unset`, `whitespace: cr-at-eol`; по `.log` и `commands.txt` -> `diff: unset`, `text: unset` (поэтому в `--stat` они показаны как `Bin`). LFS-фильтра нет: ни один из проверенных путей не имеет атрибута `filter`. Значит байты улик лежат прямо в хранилище.

## Что я ещё не знаю на момент записи

- Имя файла извлекателя признаков методов (нужен для вопроса 6) — будет определено чтением фасада и механизма, отпечаток будет дописан в `02-production-path.md`.
- Времена создания/изменения файлов улик — будут сняты отдельно (PowerShell `Get-ChildItem` с `CreationTime`/`LastWriteTime`) и записаны в `04-evidence-integrity.md`.

## Что я не делал

- Не запускал git-команд записи. Не менял файлов вне `artifacts/fbe/FIX-R21/review-0001/`. На момент записи этого файла в каталоге `review-0001` других файлов нет (каталог создан этой записью).
