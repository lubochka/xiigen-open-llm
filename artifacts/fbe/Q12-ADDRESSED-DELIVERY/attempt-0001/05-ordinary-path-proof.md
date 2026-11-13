# Доказательство: выдача шла ОБЫЧНЫМ путём, а не прямым вызовом координатора

## 1. Проверка вызывает поток, а не координатор

Все упоминания координатора адресного исправления в файле проверки:
```
229:        var coordinator = new KnnAddressedCorrectionCoordinator<string>(directory, writer, journalRoot);
```

Ни одного вызова ApplyAsync у координатора в проверке нет (счёт совпадений):
```
coordinator.ApplyAsync -> 0
```

Выдача ответа идёт вызовом потока:
```
251:            var created = factory.Create(new LogicalFlowFactoryRequest(
260:            var flow = Assert.IsType<GeneratedCapabilityFlow>(created.Value!);
264:            var served = await flow.ExecuteAsync(new LogicalFlowRequest<string>(
```

## 2. Запись выдачи физически живёт в продукте, в точке выдачи

Признак answered_delivery_recorded может поставить только точка обычной выдачи:
```
18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs:7693:            metrics["answered_delivery_recorded"] = recorded.IsSuccess.ToString();
18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs:7697:                metrics["answered_delivery_owner_id"] = recorded.Value.Owner.UnitId;
Tests/ExperimentFixes/Q12AnsweredDeliveryCorrectionTests.cs:274:            Assert.Equal(bool.TrueString, served.Value!.Metrics["answered_delivery_recorded"]);
Tests/ExperimentFixes/Q12AnsweredDeliveryCorrectionTests.cs:277:            Assert.Equal("U2", served.Value.Metrics["answered_delivery_owner_id"]);
Tests/ExperimentFixes/Q12AnsweredDeliveryCorrectionTests.cs:325:                selectedOwnerId: served.Value.Metrics["answered_delivery_owner_id"],
```
