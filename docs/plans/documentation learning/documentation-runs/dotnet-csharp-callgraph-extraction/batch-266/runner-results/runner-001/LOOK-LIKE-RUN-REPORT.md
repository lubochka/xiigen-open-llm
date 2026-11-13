# Code Documentation Batch Run

Status: PASS
Metric mode: runner_verified

| Metric | Value |
|---|---:|
| train rows | 50 |
| held-out rows | 50 |
| routing accuracy | 1.000 |
| source gate accuracy | 1.000 |
| version gate accuracy | 1.000 |
| transit gate accuracy | 1.000 |
| bottom-up order accuracy | 0.740 |
| callee-summary transit accuracy | 0.740 |
| local-vs-delegated accuracy | 0.740 |
| CPU ms | 406.25 |
| peak RAM MB | 37.133 |
| elapsed ms | 438.937 |

This run verifies the trainable documentation router path. It does not claim a full LoRA checkpoint unless a separate DPO model runner is attached.