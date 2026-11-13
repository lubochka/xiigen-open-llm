# Internal Train Ready DPO Run — BPE tokenizer

- Run id:        `ru-en-l1-batch006-bs10`
- Source:        `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-003\training-pairs-train-ready.jsonl`
- Source SHA256: `09EA82F717AF911CB1FAFB3FEBC03595BBBACAFB38E9A77C8999BD3715C560DC`
- Pairs:         50
- Batches:       5
- Batch size:    10
- Context len:   128
- BPE vocab:     2048
- Result root:   `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-006\dpo-run\ru-en-l1-batch006-bs10`
- Total elapsed: 25163.4 ms
- Avg loss:      0.685809
- Final loss:    0.691433
- Avg accuracy:  0.620
- Final accuracy:0.400
- Avg margin:    +0.015389
- Final margin:  +0.003532

## Per-batch metrics
| Batch | Pairs | Loss | Accuracy | Margin | ms | First | Last |
|---:|---:|---:|---:|---:|---:|---|---|
| 1 | 10 | 0.685324 | 0.700 | +0.016636 | 5751.5 | ru-en-g-l1-b003-train-0001 | ru-en-g-l1-b003-train-0010 |
| 2 | 10 | 0.673567 | 0.600 | +0.040516 | 5128.0 | ru-en-g-l1-b003-train-0011 | ru-en-g-l1-b003-train-0020 |
| 3 | 10 | 0.681328 | 0.900 | +0.024161 | 4180.5 | ru-en-g-l1-b003-train-0021 | ru-en-g-l1-b003-train-0030 |
| 4 | 10 | 0.697396 | 0.500 | -0.007901 | 4902.2 | ru-en-g-l1-b003-train-0031 | ru-en-g-l1-b003-train-0040 |
| 5 | 10 | 0.691433 | 0.400 | +0.003532 | 4600.9 | ru-en-g-l1-b003-train-0041 | ru-en-g-l1-b003-train-0050 |
