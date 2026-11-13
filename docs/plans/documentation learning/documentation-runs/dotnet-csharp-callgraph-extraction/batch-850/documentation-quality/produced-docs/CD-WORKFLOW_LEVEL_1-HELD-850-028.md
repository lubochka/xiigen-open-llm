# Documentation: askapediaweb::QuestionsController.AddNewQuestion

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\Controllers\QuestionsController.cs
- Symbol: askapediaweb::QuestionsController.AddNewQuestion
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ string answersDE = Request.Content.ReadAsStringAsync().Result.ToString(); //HttpUtility.UrlDecode(answers); string questionDE = HttpUtility.UrlDecode(question); List<BL.Answer> myanswers = JsonConvert.DeserializeObject<List<BL.Answer>>(answersDE); AskUser userT = BL.User.GetUserByToken(Token); string resultT = OK; object dataT = null; if (userT != null) { int count = 0; foreach (var answer in myanswers) { if (answer.value != "") { count++; } } if (count > 1) { AskQuestion Aquestion = Question.addQuestion(questionDE, userT.id); int countQ = 1; if (Aquestion != null) { foreach (var answer in myanswers) { if (answer.value != "") { Data.AskAnswer aAnswer = Question.addAnswer(Aquestion.Id, answer.value, countQ); } countQ++; } } dataT = Aquestion.Id; } else { resultT = ERROR; //EN: if (BL.User.mainLanguage == "en") { dataT = "not enough possible answers"; } if (BL.User.mainLanguage == "he") 

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
