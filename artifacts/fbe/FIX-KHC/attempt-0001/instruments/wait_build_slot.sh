#!/bin/bash
# Ожидание свободного общего каталога сборки: освобождением считаются ДВА нуля
# подряд с интервалом 30 секунд — так отличается настоящая свобода от зазора
# между концом чужой сборки и началом чужого прогона. Предел — десять минут.
cd /c/Projects/llm_mvp_core
LOG=artifacts/fbe/FIX-GB/attempt-0001/build-queue-and-occupancy.log
echo "--- очередь по правилу двух нулей подряд, предел 10 минут, старт $(date '+%H:%M:%S') ---" >> "$LOG"
zeros=0
for i in $(seq 1 20); do
  n=$(powershell -NoProfile -ExecutionPolicy Bypass -File /c/Users/lubao/AppData/Local/Temp/busy_check.ps1 2>/dev/null | tr -d ' \r\n')
  if [ "$n" = "0" ]; then zeros=$((zeros+1)); else zeros=0; fi
  echo "попытка $i из 20, $(date '+%H:%M:%S'): чужих процессов на общем каталоге = $n; нулей подряд = $zeros" >> "$LOG"
  if [ "$zeros" -ge 2 ]; then
    echo "каталог свободен по правилу двух нулей подряд, попытка $i, $(date '+%H:%M:%S')" >> "$LOG"
    exit 0
  fi
  sleep 30
done
echo "после десяти минут общий каталог всё ещё занят, $(date '+%H:%M:%S')" >> "$LOG"
exit 2
