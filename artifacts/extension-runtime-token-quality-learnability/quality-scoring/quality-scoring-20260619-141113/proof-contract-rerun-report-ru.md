# QUALITY-03 proof contract runner rerun report

Всего: 63 blocking proof refs по 9 QUALITY-03 строкам и 7 proof categories.

Сделано: входы проверены локально. Evidence pack `quality-scoring-20260619-120659` содержит 9 строк в каждом required JSONL. Proof contract pack `quality-scoring-20260619-122536` содержит 9 строк в каждом runner-expected JSONL. Authorized second-rater packet `quality-scoring-20260619-135510` содержит 9/9 pass-like inter-rater rows and no external AI/model/training mutation flags.

Сейчас: runner не запускался, потому что текущий CLI не принимает путь к authorized second-rater packet. Запуск без такого параметра использовал бы старый `row-specific-independent-second-rater-adjudication.jsonl` из proof-contract pack, где inter-rater pass-like rows = 0/9, и не проверил бы packet 135510. Поэтому официальный proof PASS остается 0/63, runner-ready rows 0/9, quality-pass rows 0/9.

Осталось: выполнить bounded code-fix work order `QUALITY-03-PROOF-CONTRACT-RUNNER-SECOND-RATER-PACKET-CLI-FIX-09K`, добавить явный `--second-rater-packet <dir>`, затем заново запустить proof contract runner на тех же трех input packs. Ожидаемый честный результат после фикса, если остальные шесть категорий не изменятся: inter-rater может стать 9/9, общий proof PASS вероятно 9/63, runner-ready 0/9, quality-pass 0/9.

Детерминированные shortcuts не скрыты: использованы только inspection CLI/source contract, JSON/JSONL parse и row counts. External AI/model calls не выполнялись. Training/DPO/checkpoint/model/locator mutation не выполнялись. Git stage/commit/push/branch не выполнялись.
