# Internal Train Ready DPO Run — BPE tokenizer

- Run id:        `ru-en-l1-cumulative-150`
- Source:        `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-005\training-pairs-combined-150.jsonl`
- Source SHA256: `30238DB9A60F16358CAC3A698E3DEEEF13028F2D71E6DD8B3F2DC6ACF9E93F58`
- Pairs:         150
- Batches:       3
- Batch size:    50
- Context len:   128
- BPE vocab:     2048
- Result root:   `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-005\dpo-run-150\ru-en-l1-cumulative-150`
- Total elapsed: 52552.8 ms
- Avg loss:      0.691802
- Final loss:    0.695524
- Avg accuracy:  0.507
- Final accuracy:0.460
- Avg margin:    +0.002829
- Final margin:  -0.004669

## Per-batch metrics
| Batch | Pairs | Loss | Accuracy | Margin | ms | First | Last |
|---:|---:|---:|---:|---:|---:|---|---|
| 1 | 50 | 0.690458 | 0.580 | +0.005456 | 18564.4 | TR-RU_EN-L1-T01-0001 | TR-RU_EN-L1-T01-0050 |
| 2 | 50 | 0.689423 | 0.480 | +0.007701 | 18296.1 | ru-en-g-l1-b003-train-0001 | ru-en-g-l1-b003-train-0050 |
| 3 | 50 | 0.695524 | 0.460 | -0.004669 | 15419.6 | ru-en-g-l1-b005-train-0001 | ru-en-g-l1-b005-train-0050 |
