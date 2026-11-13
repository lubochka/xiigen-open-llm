# PLAN: XIIGen Domain Licensing — Stripe + PayPal + Private Repo Distribution
# Version: 1.0 — 2026-05-15
# Status: REFERENCE.
#
# Free tier:  .NET domain (42.ProjectWikiDomain) — shipped in the public repo
# Paid tiers: Angular, AngularJS, Java, SQL domains — private repo
#             https://github.com/lubochka/xiigen_smart_domains

---

## Architecture Overview

```
Developer machine (plugin)
  wiki-context-hook.exe
    → LicenseManager.CheckDomain("angular")
        → reads ~/.xiigen/license.json
        → if valid: download DomainDll from CDN using signed URL
        → load DLL via DomainLoader
        → cache DLL in ~/.xiigen/domains/angular/{version}/
    → IWikiOrchestrator for that domain

License server (xiigen.com/api/license)
  POST /checkout/stripe      ← creates Stripe Checkout session
  POST /checkout/paypal      ← creates PayPal order
  POST /webhook/stripe       ← Stripe payment confirmed → generate license
  POST /webhook/paypal       ← PayPal payment confirmed → generate license
  GET  /license/validate     ← plugin calls to re-validate online
  GET  /domain/download-url  ← returns signed CDN URL for the domain DLL

Private GitHub repo
  https://github.com/lubochka/xiigen_smart_domains
  Used as the source of truth for domain DLL releases.
  GitHub Releases serve the compiled DLL artifacts.
  Plugin never touches GitHub directly — goes through license server.

CDN / signed URL layer
  License server generates time-limited signed URLs.
  Plugin downloads from CDN URL, not from GitHub directly.
  This prevents sharing download links.
```

---

## What a License Contains

```json
{
  "license_id": "lic_a3f8b2c1",
  "customer_id": "cus_stripe_abc123",
  "customer_email": "dev@example.com",
  "domains": ["angular", "java"],
  "plan": "pro_multi",
  "issued_at": "2026-05-15T10:00:00Z",
  "expires_at": "2027-05-15T10:00:00Z",
  "machine_ids": ["sha256(mac_address+hostname)", "sha256(...)"],
  "max_machines": 3,
  "signature": "HMAC-SHA256(license_id+customer_id+domains+expires_at, SERVER_SECRET)"
}
```

The `signature` field is signed with a server secret. The plugin verifies the
signature locally — it can validate a cached license without an internet call
for up to 30 days. After 30 days it must re-validate online.

---

## Pricing Tiers

Define these before building. Example:

| Plan | Domains | Price | Period |
|---|---|---|---|
| Free | .NET only | $0 | Forever |
| Starter | 1 extra domain (Angular OR Java OR SQL) | $9/mo or $79/yr | Monthly/Annual |
| Pro | All domains (Angular, AngularJS, Java, SQL) | $19/mo or $149/yr | Monthly/Annual |
| Team | All domains, 5 seats | $49/mo or $399/yr | Monthly/Annual |
| Enterprise | All domains, unlimited seats, private support | Contact | Annual |

---

## Part 1 — License Server

### Technology

NestJS (existing XIIGen stack). Deployed on the same infrastructure.

```
xiigen-license-server/
  src/
    checkout/
      checkout.controller.ts     ← POST /checkout/stripe + /checkout/paypal
      stripe.service.ts
      paypal.service.ts
    webhooks/
      stripe-webhook.controller.ts
      paypal-webhook.controller.ts
    license/
      license.service.ts         ← generate, sign, validate
      license.controller.ts      ← GET /license/validate, /domain/download-url
    domain/
      domain.service.ts          ← GitHub Release asset URLs → signed CDN URLs
    database/
      license.entity.ts
      customer.entity.ts
```

### Stripe Integration

```typescript
// checkout/stripe.service.ts
import Stripe from 'stripe';

@Injectable()
export class StripeService {
  private stripe = new Stripe(process.env.STRIPE_SECRET_KEY);

  async createCheckoutSession(
    planId: string,
    customerEmail: string,
    machineId: string
  ): Promise<string> {
    const priceId = PLAN_TO_STRIPE_PRICE[planId]; // e.g. 'price_abc123'

    const session = await this.stripe.checkout.sessions.create({
      mode: 'subscription',          // use 'payment' for one-time
      customer_email: customerEmail,
      line_items: [{ price: priceId, quantity: 1 }],
      success_url: `https://xiigen.com/license/success?session={CHECKOUT_SESSION_ID}`,
      cancel_url:  `https://xiigen.com/pricing`,
      metadata: {
        machine_id: machineId,       // passed from the plugin at purchase time
        plan_id:    planId
      },
      subscription_data: {
        metadata: { machine_id: machineId, plan_id: planId }
      }
    });

    return session.url;  // redirect developer to this URL
  }
}
```

```typescript
// webhooks/stripe-webhook.controller.ts
@Post('webhook/stripe')
@HttpCode(200)
async handleStripeWebhook(
  @Req() req: RawBodyRequest<Request>,
  @Headers('stripe-signature') sig: string
): Promise<void> {
  const event = this.stripe.webhooks.constructEvent(
    req.rawBody,
    sig,
    process.env.STRIPE_WEBHOOK_SECRET  // from Stripe Dashboard
  );

  switch (event.type) {
    case 'checkout.session.completed':
      await this.onPaymentSuccess(event.data.object);
      break;
    case 'invoice.paid':
      await this.onRenewal(event.data.object);
      break;
    case 'customer.subscription.deleted':
      await this.onCancellation(event.data.object);
      break;
  }
}

private async onPaymentSuccess(session: Stripe.Checkout.Session) {
  const planId    = session.metadata.plan_id;
  const machineId = session.metadata.machine_id;
  const email     = session.customer_email;

  const domains = PLAN_TO_DOMAINS[planId];  // ['angular', 'java']
  await this.licenseService.issueLicense({
    customerId: session.customer as string,
    customerEmail: email,
    domains,
    planId,
    machineIds: [machineId],
    expiresAt: addMonths(new Date(), 1),    // or 12 for annual
  });
}
```

### PayPal Integration

```typescript
// checkout/paypal.service.ts
import { PayPalHttpClient, OrdersCreateRequest } from '@paypal/checkout-server-sdk';

@Injectable()
export class PaypalService {
  private client: PayPalHttpClient;

  constructor() {
    const env = process.env.NODE_ENV === 'production'
      ? new PayPal.core.LiveEnvironment(process.env.PAYPAL_CLIENT_ID, process.env.PAYPAL_SECRET)
      : new PayPal.core.SandboxEnvironment(process.env.PAYPAL_CLIENT_ID, process.env.PAYPAL_SECRET);
    this.client = new PayPalHttpClient(env);
  }

  async createOrder(planId: string, machineId: string): Promise<string> {
    const price = PLAN_TO_PRICE_USD[planId];    // e.g. '9.00'
    const planName = PLAN_DISPLAY_NAMES[planId]; // e.g. 'XIIGen Starter — Monthly'

    const request = new OrdersCreateRequest();
    request.requestBody({
      intent: 'CAPTURE',
      purchase_units: [{
        amount: { currency_code: 'USD', value: price },
        description: planName,
        custom_id: `${planId}:${machineId}`,     // machine_id passed through
      }],
      application_context: {
        return_url: `https://xiigen.com/license/success/paypal`,
        cancel_url: `https://xiigen.com/pricing`,
      }
    });

    const order = await this.client.execute(request);
    const approveLink = order.result.links.find(l => l.rel === 'approve');
    return approveLink.href;   // redirect developer here
  }
}
```

```typescript
// webhooks/paypal-webhook.controller.ts
@Post('webhook/paypal')
@HttpCode(200)
async handlePaypalWebhook(@Body() body: any, @Headers() headers: any) {
  // Verify PayPal webhook signature
  const isValid = await this.paypalService.verifyWebhook(body, headers);
  if (!isValid) throw new ForbiddenException('Invalid PayPal webhook');

  if (body.event_type === 'PAYMENT.CAPTURE.COMPLETED') {
    const customId = body.resource.custom_id;  // 'planId:machineId'
    const [planId, machineId] = customId.split(':');

    const domains = PLAN_TO_DOMAINS[planId];
    await this.licenseService.issueLicense({
      customerId: body.resource.id,
      customerEmail: body.resource.payer?.email_address,
      domains,
      planId,
      machineIds: [machineId],
      expiresAt: addMonths(new Date(), 1),
    });
  }
}
```

### License Generation and Signing

```typescript
// license/license.service.ts
import * as crypto from 'crypto';

@Injectable()
export class LicenseService {
  private readonly SECRET = process.env.LICENSE_SIGNING_SECRET; // 256-bit secret, never rotated

  async issueLicense(dto: IssueLicenseDto): Promise<LicenseEntity> {
    const licenseId = `lic_${crypto.randomBytes(8).toString('hex')}`;

    const payload = {
      license_id:     licenseId,
      customer_id:    dto.customerId,
      customer_email: dto.customerEmail,
      domains:        dto.domains,         // ['angular', 'java']
      plan:           dto.planId,
      issued_at:      new Date().toISOString(),
      expires_at:     dto.expiresAt.toISOString(),
      machine_ids:    dto.machineIds,
      max_machines:   dto.maxMachines ?? 3,
    };

    const signature = this.sign(payload);
    const license = { ...payload, signature };

    // Save to DB
    await this.licenseRepo.save({ ...license, rawJson: JSON.stringify(license) });

    // Email to customer
    await this.emailService.sendLicense(dto.customerEmail, license);

    return license;
  }

  private sign(payload: object): string {
    const data = [
      payload['license_id'],
      payload['customer_id'],
      payload['domains'].join(','),
      payload['expires_at'],
    ].join('|');

    return crypto.createHmac('sha256', this.SECRET)
      .update(data)
      .digest('hex');
  }

  validateSignature(license: LicenseJson): boolean {
    const expected = this.sign(license);
    // Constant-time comparison to prevent timing attacks
    return crypto.timingSafeEqual(
      Buffer.from(expected),
      Buffer.from(license.signature)
    );
  }
}
```

### Signed Download URL (CDN)

The private GitHub repo releases are mirrored to S3 or CloudFront.
The license server generates time-limited signed URLs — never exposes
the permanent GitHub asset URL.

```typescript
// domain/domain.service.ts
import { S3Client, GetObjectCommand } from '@aws-sdk/client-s3';
import { getSignedUrl } from '@aws-sdk/s3-request-presigner';

@Injectable()
export class DomainService {
  private s3 = new S3Client({ region: process.env.AWS_REGION });

  async getSignedDownloadUrl(
    domain: string,   // 'angular' | 'java' | 'sql' | 'angularjs'
    version: string,  // '1.0.0'
    rid: string,      // 'win-x64' | 'linux-x64' | 'osx-arm64'
    licenseId: string
  ): Promise<string> {
    const key = `domains/${domain}/${version}/${rid}/XiiGen.Domain.${capitalize(domain)}.dll`;

    const command = new GetObjectCommand({
      Bucket: process.env.S3_DOMAIN_BUCKET,
      Key: key,
    });

    // Signed URL valid for 15 minutes — enough to download, not to share
    const url = await getSignedUrl(this.s3, command, { expiresIn: 900 });

    // Log download for audit
    await this.auditLog.record(licenseId, domain, version, rid);

    return url;
  }
}
```

---

## Part 2 — Plugin Side: LicenseManager

```
42.ProjectWikiDomain/
  Licensing/
    ILicenseManager.cs
    LicenseManager.cs          ← validates + caches license locally
    DomainLoader.cs            ← downloads + loads paid domain DLL
    MachineId.cs               ← generates stable anonymous machine ID
    LicenseStore.cs            ← reads/writes ~/.xiigen/license.json
```

### ILicenseManager

```csharp
// Licensing/ILicenseManager.cs
public interface ILicenseManager
{
    /// <summary>
    /// Check if a domain is licensed for this machine.
    /// Returns true for "dotnet" always (free).
    /// For paid domains: checks local cache first, validates online if stale.
    /// </summary>
    Task<DomainAccessResult> CheckDomainAccessAsync(
        string domain,
        CancellationToken ct = default);

    /// <summary>
    /// Get a signed download URL for a domain DLL.
    /// Requires valid license. Only callable when CheckDomainAccessAsync returns true.
    /// </summary>
    Task<string?> GetDomainDownloadUrlAsync(
        string domain,
        string version,
        string rid,
        CancellationToken ct = default);

    /// <summary>
    /// Returns the URL to purchase a license for a domain.
    /// Opens in browser.
    /// </summary>
    string GetPurchaseUrl(string domain);
}

public record DomainAccessResult(
    bool IsLicensed,
    DomainAccessReason Reason,
    DateTimeOffset? ExpiresAt,
    string? PurchaseUrl);

public enum DomainAccessReason
{
    Free,               // .NET domain — always allowed
    ValidLicense,       // license found and valid
    LicenseExpired,     // had a license, now expired
    LicenseNotFound,    // no license file
    DomainNotLicensed,  // license exists but this domain not in it
    ValidationFailed,   // online validation returned invalid
    OfflineCached,      // validated within 30 days, no internet
}
```

### LicenseManager Implementation

```csharp
// Licensing/LicenseManager.cs
public sealed class LicenseManager : ILicenseManager
{
    private const string FreeDomain = "dotnet";
    private const string LicenseServerBase = "https://xiigen.com/api";
    private const int OfflineGraceDays = 30;

    private readonly LicenseStore _store;
    private readonly HttpClient _http;

    public async Task<DomainAccessResult> CheckDomainAccessAsync(
        string domain, CancellationToken ct)
    {
        // .NET is always free
        if (domain.Equals(FreeDomain, StringComparison.OrdinalIgnoreCase))
            return new(true, DomainAccessReason.Free, null, null);

        // Load cached license
        var license = _store.Load();
        if (license is null)
            return new(false, DomainAccessReason.LicenseNotFound,
                null, GetPurchaseUrl(domain));

        // Check domain is in license
        if (!license.Domains.Contains(domain, StringComparer.OrdinalIgnoreCase))
            return new(false, DomainAccessReason.DomainNotLicensed,
                null, GetPurchaseUrl(domain));

        // Verify signature locally (offline)
        if (!VerifySignatureLocally(license))
            return new(false, DomainAccessReason.ValidationFailed,
                null, GetPurchaseUrl(domain));

        // Check expiry
        if (license.ExpiresAt < DateTimeOffset.UtcNow)
            return new(false, DomainAccessReason.LicenseExpired,
                license.ExpiresAt, GetPurchaseUrl(domain));

        // Check machine ID
        var myMachineId = MachineId.Get();
        if (!license.MachineIds.Contains(myMachineId))
        {
            // Machine not registered — try to register online
            var registered = await TryRegisterMachineAsync(license, myMachineId, ct);
            if (!registered)
                return new(false, DomainAccessReason.ValidationFailed,
                    null, GetPurchaseUrl(domain));
        }

        // If last validated > 30 days ago: validate online
        if (license.LastValidatedAt < DateTimeOffset.UtcNow.AddDays(-OfflineGraceDays))
        {
            var onlineValid = await ValidateOnlineAsync(license, ct);
            if (!onlineValid)
                return new(false, DomainAccessReason.ValidationFailed,
                    null, GetPurchaseUrl(domain));
        }

        return new(true, license.LastValidatedAt < DateTimeOffset.UtcNow.AddDays(-OfflineGraceDays)
            ? DomainAccessReason.OfflineCached
            : DomainAccessReason.ValidLicense,
            license.ExpiresAt, null);
    }

    private bool VerifySignatureLocally(LicenseJson license)
    {
        // The public verification key is embedded in the binary at build time.
        // Uses HMAC-SHA256 — server secret never leaves the server.
        // Binary embeds a separate verification-only key (can verify, cannot sign).
        var data = $"{license.LicenseId}|{license.CustomerId}|" +
                   $"{string.Join(',', license.Domains)}|{license.ExpiresAt:O}";

        var expected = ComputeHmac(data, EmbeddedVerificationKey);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(expected),
            Convert.FromHexString(license.Signature));
    }

    private async Task<bool> ValidateOnlineAsync(LicenseJson license, CancellationToken ct)
    {
        try
        {
            var response = await _http.GetAsync(
                $"{LicenseServerBase}/license/validate" +
                $"?license_id={license.LicenseId}&machine_id={MachineId.Get()}", ct);

            if (!response.IsSuccessStatusCode) return false;

            var result = await response.Content.ReadFromJsonAsync<ValidationResult>(ct);
            if (result?.Valid != true) return false;

            // Update last validated timestamp in local cache
            license.LastValidatedAt = DateTimeOffset.UtcNow;
            _store.Save(license);
            return true;
        }
        catch
        {
            // Network error — allow offline grace period
            return true;  // grace: already checked locally above
        }
    }

    public string GetPurchaseUrl(string domain)
        => $"https://xiigen.com/pricing?domain={domain}" +
           $"&machine_id={MachineId.Get()}";
}
```

### MachineId — Anonymous Stable Identifier

```csharp
// Licensing/MachineId.cs
public static class MachineId
{
    private static string? _cached;

    public static string Get()
    {
        if (_cached is not null) return _cached;

        // Combine stable machine properties — never contains PII
        var components = new[]
        {
            Environment.MachineName,
            Environment.UserName,
            // On Windows: processor ID from registry
            GetProcessorId(),
        };

        var raw = string.Join("|", components.Where(c => !string.IsNullOrEmpty(c)));
        _cached = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(raw)))[..16];

        return _cached;
    }

    private static string? GetProcessorId()
    {
        if (!OperatingSystem.IsWindows()) return null;
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine
                .OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            return key?.GetValue("ProcessorNameString")?.ToString();
        }
        catch { return null; }
    }
}
```

### LicenseStore — Local File

```csharp
// Licensing/LicenseStore.cs
public sealed class LicenseStore
{
    private static string LicensePath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".xiigen", "license.json");

    public LicenseJson? Load()
    {
        if (!File.Exists(LicensePath)) return null;
        try
        {
            var json = File.ReadAllText(LicensePath);
            return JsonSerializer.Deserialize<LicenseJson>(
                json, WikiJsonContext.Default.LicenseJson);
        }
        catch { return null; }
    }

    public void Save(LicenseJson license)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(LicensePath)!);
        var json = JsonSerializer.Serialize(license,
            WikiJsonContext.Default.LicenseJson);
        // Write to temp file then rename — prevents corruption on crash
        var tmp = LicensePath + ".tmp";
        File.WriteAllText(tmp, json);
        File.Move(tmp, LicensePath, overwrite: true);
    }
}
```

### DomainLoader — Downloads and Loads the DLL

```csharp
// Licensing/DomainLoader.cs
public sealed class DomainLoader
{
    private static readonly string CacheDir =
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".xiigen", "domains");

    public async Task<IWikiOrchestrator?> LoadDomainAsync(
        string domain,
        string version,
        string rid,
        ILicenseManager licenseManager,
        CancellationToken ct)
    {
        // 1. Check license
        var access = await licenseManager.CheckDomainAccessAsync(domain, ct);
        if (!access.IsLicensed)
        {
            Console.Error.WriteLine(
                $"XIIGen: {domain} domain requires a license. " +
                $"Purchase at: {access.PurchaseUrl}");
            return null;
        }

        // 2. Check local cache
        var dllPath = Path.Combine(CacheDir, domain, version, rid,
            $"XiiGen.Domain.{Capitalize(domain)}.dll");

        if (!File.Exists(dllPath))
        {
            // 3. Get signed download URL from license server
            var url = await licenseManager.GetDomainDownloadUrlAsync(
                domain, version, rid, ct);
            if (url is null) return null;

            // 4. Download
            await DownloadDllAsync(url, dllPath, ct);
        }

        // 5. Load and instantiate
        return LoadFromDll(dllPath);
    }

    private async Task DownloadDllAsync(string url, string dllPath, CancellationToken ct)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dllPath)!);
        using var http = new HttpClient();
        using var stream = await http.GetStreamAsync(url, ct);
        using var file = File.Create(dllPath + ".tmp");
        await stream.CopyToAsync(file, ct);
        file.Close();
        File.Move(dllPath + ".tmp", dllPath, overwrite: true);
    }

    private IWikiOrchestrator? LoadFromDll(string dllPath)
    {
        try
        {
            var context = new WikiDomainLoadContext(dllPath);
            var asm = context.LoadFromAssemblyPath(dllPath);

            // Convention: every domain DLL exports a type called DomainEntryPoint
            var entryType = asm.GetType("XiiGen.Domain.DomainEntryPoint");
            var createMethod = entryType?.GetMethod("CreateOrchestrator");
            return createMethod?.Invoke(null, null) as IWikiOrchestrator;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"XIIGen: Failed to load domain: {ex.Message}");
            return null;
        }
    }
}

// Isolated load context — prevents version conflicts between domains
internal sealed class WikiDomainLoadContext : AssemblyLoadContext
{
    private readonly string _dllPath;
    private readonly AssemblyDependencyResolver _resolver;

    public WikiDomainLoadContext(string dllPath) : base(isCollectible: true)
    {
        _dllPath = dllPath;
        _resolver = new AssemblyDependencyResolver(dllPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        return path is not null ? LoadFromAssemblyPath(path) : null;
    }
}
```

---

## Part 3 — WikiOrchestratorFactory Update

```csharp
// 42.ProjectWikiDomain/WikiOrchestratorFactory.cs (updated)
public static class WikiOrchestratorFactory
{
    public static IWikiOrchestrator Create(string projectRoot)
    {
        // .NET is always free — no license check
        var manifest = ReadManifest(projectRoot);
        if (manifest?.Status == "PROMOTED")
            return new ProjectLocalWikiOrchestrator(projectRoot);
        return new MockWikiOrchestrator(projectRoot);
    }

    public static async Task<IWikiOrchestrator?> CreateForDomainAsync(
        string domain,
        string projectRoot,
        ILicenseManager licenseManager,
        CancellationToken ct = default)
    {
        // Free domain — no license check
        if (domain.Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            return Create(projectRoot);

        // Paid domain — check license and load DLL
        var rid = GetCurrentRid();  // win-x64, linux-x64, osx-arm64
        var loader = new DomainLoader();
        var orchestrator = await loader.LoadDomainAsync(
            domain, DomainVersions.Current[domain], rid, licenseManager, ct);

        if (orchestrator is null)
            return null;  // caller handles: show purchase URL

        return orchestrator;
    }

    private static string GetCurrentRid()
    {
        if (OperatingSystem.IsWindows()) return "win-x64";
        if (OperatingSystem.IsMacOS())  return Environment.Is64BitProcess
            ? (IsAppleSilicon() ? "osx-arm64" : "osx-x64")
            : "osx-x64";
        return "linux-x64";
    }

    private static bool IsAppleSilicon()
    {
        try
        {
            var output = System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo("sysctl", "-n hw.optional.arm64")
                    { RedirectStandardOutput = true })
                ?.StandardOutput.ReadToEnd().Trim();
            return output == "1";
        }
        catch { return false; }
    }
}
```

---

## Part 4 — Private Repo Structure (xiigen_smart_domains)

```
https://github.com/lubochka/xiigen_smart_domains/
  angular/
    1.0.0/
      src/                        ← source code (private, never distributed)
      XiiGen.Domain.Angular.csproj
    publish/
      win-x64/XiiGen.Domain.Angular.dll
      linux-x64/XiiGen.Domain.Angular.dll
      osx-arm64/XiiGen.Domain.Angular.dll
  angularjs/
    1.0.0/
      src/
      publish/...
  java/
    1.0.0/
      src/
      publish/...
  sql/
    1.0.0/
      src/
      publish/...
  scripts/
    publish-release.ps1           ← builds + uploads to S3 + creates GitHub Release
    verify-signature.ps1
  .github/
    workflows/
      build-domains.yml
```

### Each Domain DLL Exports DomainEntryPoint

```csharp
// angular/1.0.0/src/DomainEntryPoint.cs (in private repo)
// This class is the ONLY public API the host needs to know about.
namespace XiiGen.Domain;

public static class DomainEntryPoint
{
    public static IWikiOrchestrator CreateOrchestrator()
        => new AngularWikiOrchestrator();
}
```

### Build and Release Script

```powershell
# scripts/publish-release.ps1
param(
    [string]$Domain,    # angular | angularjs | java | sql
    [string]$Version    # 1.0.0
)

$rids = @("win-x64", "linux-x64", "osx-arm64")

foreach ($rid in $rids) {
    dotnet publish "$Domain/$Version/XiiGen.Domain.${Domain}.csproj" `
        -c Release -r $rid `
        --self-contained false `     # DLL, not exe — loaded into existing process
        -p:PublishTrimmed=true `
        -p:TrimMode=partial `        # partial trim — safer for reflection-heavy domain code
        -o "$Domain/$Version/publish/$rid/"
}

# Upload to S3
foreach ($rid in $rids) {
    $key = "domains/$Domain/$Version/$rid/XiiGen.Domain.${Domain}.dll"
    aws s3 cp "$Domain/$Version/publish/$rid/XiiGen.Domain.${Domain}.dll" `
              "s3://$env:S3_DOMAIN_BUCKET/$key" `
              --sse aws:kms
}

# Create GitHub Release with artifacts (source stays private; only DLLs in release)
gh release create "v$Version-$Domain" `
    --repo lubochka/xiigen_smart_domains `
    --title "$Domain domain v$Version" `
    --notes "Compiled domain DLL for licensed customers." `
    "$Domain/$Version/publish/win-x64/XiiGen.Domain.${Domain}.dll#win-x64" `
    "$Domain/$Version/publish/linux-x64/XiiGen.Domain.${Domain}.dll#linux-x64" `
    "$Domain/$Version/publish/osx-arm64/XiiGen.Domain.${Domain}.dll#osx-arm64"
```

---

## Part 5 — User Experience In The Plugin

### When domain is not licensed (VS Code sidebar)

```
XIIGen Code Wiki

Status: .NET wiki — PROMOTED · 247 entries

Angular wiki: not licensed
  To enable Angular question answering:
  https://xiigen.com/pricing?domain=angular&machine_id=a3f8b2c1

[Open Pricing Page]
```

### When domain is being downloaded

```
XIIGen Code Wiki

Downloading Angular domain (1.2 MB)...
████████████░░░░  75%
```

### After successful license activation

```bash
# Developer installs license from email or downloads from xiigen.com/dashboard
# Option A: CLI
wiki-context-hook --activate <license-key>

# Option B: VS Code command
# Cmd+Shift+P → "XIIGen: Activate License"
# Paste license key → validated → stored → domain DLL downloaded
```

```csharp
// wiki-context-hook --activate <key> implementation
static async Task<int> RunActivateAsync(string[] args)
{
    if (args.Length < 2)
    {
        Console.Error.WriteLine("Usage: wiki-context-hook --activate <license-key>");
        return 1;
    }

    var key = args[1];
    var response = await http.PostAsJsonAsync(
        $"{LicenseServerBase}/license/activate",
        new { license_key = key, machine_id = MachineId.Get() });

    if (!response.IsSuccessStatusCode)
    {
        Console.Error.WriteLine("License activation failed. Check the key and try again.");
        return 1;
    }

    var license = await response.Content.ReadFromJsonAsync<LicenseJson>();
    new LicenseStore().Save(license);

    Console.WriteLine($"License activated for: {string.Join(", ", license.Domains)}");
    Console.WriteLine($"Valid until: {license.ExpiresAt:D}");
    Console.WriteLine($"Machine registered: {MachineId.Get()}");
    return 0;
}
```

---

## Security Checklist

```
□ Stripe webhook signature verified (Stripe-Signature header)
□ PayPal webhook identity verified (PayPal webhook ID validation)
□ License signature uses HMAC-SHA256 with server secret — secret never in binary
□ Download URLs are time-limited (15 min) signed S3 presigned URLs
□ MachineId is anonymous (hashed, no PII) — GDPR compliant
□ License file stored in ~/.xiigen/ — not in project directory (never committed)
□ DLL loaded in isolated AssemblyLoadContext — cannot affect host process
□ Domain DLL source code never leaves private repo — only compiled DLL distributed
□ Stripe/PayPal webhooks validate origin — raw body used for signature (not parsed body)
□ License validation uses constant-time comparison (CryptographicOperations.FixedTimeEquals)
□ Offline grace: 30 days without internet still works — developer not blocked
□ Failed validation: graceful degradation — falls back to .NET free domain
```
