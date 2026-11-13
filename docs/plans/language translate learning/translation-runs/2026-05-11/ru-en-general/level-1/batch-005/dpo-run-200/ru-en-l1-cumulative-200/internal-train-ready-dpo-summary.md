# Internal Train Ready DPO Run — BPE tokenizer

- Run id:        `ru-en-l1-cumulative-200`
- Source:        `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-005\training-pairs-combined-200.jsonl`
- Source SHA256: `A769643CDE8C35D32FBFBD45613F0668739FF15F8993D03825F81A73F8CA68C5`
- Pairs:         200
- Batches:       4
- Batch size:    50
- Context len:   128
- BPE vocab:     2048
- Result root:   `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-005\dpo-run-200\ru-en-l1-cumulative-200`
- Total elapsed: 80061.8 ms
- Avg loss:      0.700862
- Final loss:    0.703100
- Avg accuracy:  0.290
- Final accuracy:0.200
- Avg margin:    -0.015213
- Final margin:  -0.019652

## Per-batch metrics
| Batch | Pairs | Loss | Accuracy | Margin | ms | First | Last |
|---:|---:|---:|---:|---:|---:|---|---|
| 1 | 50 | 0.698611 | 0.360 | -0.010775 | 15980.7 | TR-RU_EN-L1-T01-0001 | TR-RU_EN-L1-T01-0050 |
| 2 | 50 | 0.701602 | 0.280 | -0.016638 | 14270.5 | ru-en-g-l1-b003-train-0001 | ru-en-g-l1-b003-train-0050 |
| 3 | 50 | 0.700133 | 0.320 | -0.013788 | 27353.2 | ru-en-g-l1-b004-train-0001 | ru-en-g-l1-b004-train-0050 |
| 4 | 50 | 0.703100 | 0.200 | -0.019652 | 22151.5 | ru-en-g-l1-b005-train-0001 | ru-en-g-l1-b005-train-0050 |
