# Documentation: askapediaweb::ReCaptchaClass.Validate

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\BL\ReCaptchaClass.cs
- Symbol: askapediaweb::ReCaptchaClass.Validate
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ var client = new System.Net.WebClient(); string PrivateKey = "6Ld2wQsUAAAAANZtYiUB5b0446sCs7QDJAzbGoqC"; var GoogleReply = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", PrivateKey, EncodedResponse)); var captchaResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ReCaptchaClass>(GoogleReply); return captchaResponse.Success; }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
