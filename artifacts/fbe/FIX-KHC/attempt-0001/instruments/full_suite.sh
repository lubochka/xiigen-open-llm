#!/bin/bash
# Полный прогон проекта проверок без пересборки: библиотека уже собрана этапом.
# Проверка занятости стоит УСЛОВИЕМ и останавливает прогон.
# Параметр: имя подкаталога улик.
set -o pipefail
cd /c/Projects/llm_mvp_core
STAGE="$1"
OUT="artifacts/fbe/FIX-KHC/attempt-0001/$STAGE"
mkdir -p "$OUT/tests"

busy() { powershell -NoProfile -ExecutionPolicy Bypass -File /c/Users/lubao/AppData/Local/Temp/busy_check.ps1 2>/dev/null | tr -d ' \r\n'; }

n=$(busy)
{ echo "=== $STAGE: занятость перед полным прогоном, $(date '+%Y-%m-%d %H:%M:%S') ==="
  echo "чужих процессов на общем каталоге (сборки+прогоны): $n"; } | tee -a "$OUT/occupancy.log"
if [ "$n" != "0" ]; then echo "ОСТАНОВ: каталог занят ($n)"; exit 20; fi

echo "=== отпечатки библиотек перед полным прогоном ===" >> "$OUT/assembly-stamp.txt"
for dll in Tests/bin/Release/net9.0/SemanticCompiler.dll Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll; do
  { echo "объект: файл $(pwd)/$dll"
    echo "  размер в байтах: $(stat -c %s "$dll")"
    echo "  git hash-object: $(git hash-object "$dll")"
    echo "  sha256sum: $(sha256sum "$dll" | cut -d' ' -f1)"; } >> "$OUT/assembly-stamp.txt"
done

echo "=== $STAGE: ПОЛНЫЙ прогон проекта проверок, $(date '+%H:%M:%S') ==="
dotnet test Tests/LlmFromScratch.Tests.csproj -c Release --no-build \
  --logger "trx;LogFileName=full-$STAGE.trx" --results-directory "$OUT/tests" > "$OUT/full-run.log" 2>&1
echo "код возврата полного прогона: $?" > "$OUT/full-exit.txt"
tail -4 "$OUT/full-run.log"
echo "=== $STAGE полный прогон готов, $(date '+%H:%M:%S') ==="
exit 0
