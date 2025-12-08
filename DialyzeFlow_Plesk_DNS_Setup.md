# Configuring dev.dialyzeflow.com in Plesk (HostingRaja)

## Step-by-Step Guide to Map Subdomain to Azure Static Web App

Your Azure Static Web App URL: `lively-pond-08e4f7c00.3.azurestaticapps.net`
Target subdomain: `dev.dialyzeflow.com`

---

## Part 1: Get Validation Code from Azure Portal

**Before configuring Plesk, first get the validation code from Azure:**

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to your Static Web App: **lively-pond-08e4f7c00**
3. In the left sidebar, click **Custom domains**
4. Click **+ Add**
5. Select **Custom domain on other DNS**
6. Enter: `dev.dialyzeflow.com`
7. Click **Next**
8. **Copy the validation code** shown (looks like: `asuid.dev.dialyzeflow.com` → `abc123xyz...`)
9. Keep this page open - you'll come back to it

---

## Part 2: Configure DNS in Plesk (HostingRaja)

### Step 1: Login to Plesk

1. Go to your HostingRaja Plesk login URL (typically: `https://your-server.hostingraja.in:8443` or via HostingRaja client area)
2. Enter your username and password
3. Click **Log In**

### Step 2: Navigate to DNS Settings

1. Click on **Websites & Domains** tab
2. Find **dialyzeflow.com** in your domain list
3. Click on **DNS Settings** (or **DNS** icon)

```
┌─────────────────────────────────────────────────────────────┐
│  Websites & Domains                                          │
├─────────────────────────────────────────────────────────────┤
│  dialyzeflow.com                                             │
│  ┌──────┐ ┌──────┐ ┌────────────┐ ┌──────┐ ┌──────────┐    │
│  │ PHP  │ │ SSL  │ │ DNS Settings│ │ FTP  │ │ Databases │    │
│  └──────┘ └──────┘ └────────────┘ └──────┘ └──────────┘    │
│                          ▲                                   │
│                          │                                   │
│                    Click here                                │
└─────────────────────────────────────────────────────────────┘
```

### Step 3: Add TXT Record (Domain Validation)

1. Click **Add Record**
2. Configure as follows:

| Field | Value |
|-------|-------|
| **Record type** | `TXT` |
| **Domain name** | `_dnsauth.dev` |
| **TXT record** | *(Paste the validation code from Azure)* |
| **TTL** | `3600` (or leave default) |

3. Click **OK**

```
┌─────────────────────────────────────────────────────────────┐
│  Add DNS Record                                              │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Record type:    [ TXT                          ▼ ]         │
│                                                              │
│  Domain name:    [ _dnsauth.dev                   ]         │
│                  .dialyzeflow.com                            │
│                                                              │
│  TXT record:     [ paste-azure-validation-code-here ]       │
│                                                              │
│  TTL:            [ 3600                           ]         │
│                                                              │
│                        [ OK ]  [ Cancel ]                   │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### Step 4: Add CNAME Record (Actual Mapping)

1. Click **Add Record** again
2. Configure as follows:

| Field | Value |
|-------|-------|
| **Record type** | `CNAME` |
| **Domain name** | `dev` |
| **Canonical name** | `lively-pond-08e4f7c00.3.azurestaticapps.net.` |
| **TTL** | `3600` (or leave default) |

> ⚠️ **Important Notes:**
> - For "Domain name", enter ONLY `dev` (Plesk adds `.dialyzeflow.com` automatically)
> - For "Canonical name", enter the Azure URL WITHOUT `https://`
> - Some Plesk versions require a trailing dot (.) at the end of the canonical name

3. Click **OK**

```
┌─────────────────────────────────────────────────────────────┐
│  Add DNS Record                                              │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Record type:    [ CNAME                        ▼ ]         │
│                                                              │
│  Domain name:    [ dev                            ]         │
│                  .dialyzeflow.com                            │
│                                                              │
│  Canonical name: [ lively-pond-08e4f7c00.3.azurestaticapps.net. ] │
│                                                              │
│  TTL:            [ 3600                           ]         │
│                                                              │
│                        [ OK ]  [ Cancel ]                   │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### Step 5: Apply Changes

1. After adding both records, you should see them in the DNS records list
2. Click **Update** or **Apply** if Plesk shows this option
3. The records are now being propagated

Your DNS settings should now look like this:

```
┌─────────────────────────────────────────────────────────────┐
│  DNS Records for dialyzeflow.com                             │
├─────────────────────────────────────────────────────────────┤
│  Type    │ Host                      │ Value                 │
├──────────┼──────────────────────────┼───────────────────────┤
│  A       │ dialyzeflow.com          │ [your-server-IP]      │
│  A       │ www.dialyzeflow.com      │ [your-server-IP]      │
│  CNAME   │ dev.dialyzeflow.com      │ lively-pond-08e...    │ ← NEW
│  TXT     │ _dnsauth.dev.dialyze...  │ [validation-code]     │ ← NEW
│  MX      │ dialyzeflow.com          │ mail.dialyzeflow.com  │
│  ...     │ ...                      │ ...                   │
└─────────────────────────────────────────────────────────────┘
```

---

## Part 3: Complete Validation in Azure

### Step 1: Wait for DNS Propagation

DNS changes can take 5 minutes to 48 hours to propagate globally. Typically:
- **Within same country**: 5-30 minutes
- **Globally**: 1-4 hours
- **Maximum**: 48 hours

### Step 2: Verify DNS Records

You can check if your records are propagating:

**Option A: Using command line (Windows)**
```cmd
nslookup -type=TXT _dnsauth.dev.dialyzeflow.com
nslookup -type=CNAME dev.dialyzeflow.com
```

**Option B: Using command line (Mac/Linux)**
```bash
dig TXT _dnsauth.dev.dialyzeflow.com
dig CNAME dev.dialyzeflow.com
```

**Option C: Using online tools**
- https://dnschecker.org
- https://mxtoolbox.com/DNSLookup.aspx

### Step 3: Complete Azure Validation

1. Return to Azure Portal → Custom domains page
2. Your domain `dev.dialyzeflow.com` should show as "Pending" or ready to validate
3. Click **Validate** or **Add**
4. Wait for Azure to verify the TXT record
5. Once validated, Azure will:
   - Issue a free SSL certificate
   - Configure the custom domain
   - Show status as "Ready" ✓

---

## Part 4: Test Your Setup

Once Azure shows the domain as validated:

1. Open a new browser tab
2. Go to: `https://dev.dialyzeflow.com`
3. Your DialyzeFlow app should load with a valid SSL certificate 🎉

---

## Troubleshooting

### Issue: "CNAME Record is invalid" in Azure

**Cause**: DNS hasn't propagated yet or incorrect CNAME value

**Solution**:
1. Wait 15-30 minutes and try again
2. Verify CNAME value is exactly: `lively-pond-08e4f7c00.3.azurestaticapps.net`
3. Check there's no `https://` prefix
4. Check there are no extra spaces

### Issue: "This record will conflict with the glue record" in Plesk

**Cause**: You created a subdomain in Plesk's "Websites & Domains" section

**Solution**:
1. Go to Websites & Domains
2. If you see `dev.dialyzeflow.com` listed as a subdomain, **delete it**
3. Only use DNS records to point the subdomain to Azure

### Issue: TXT record not found

**Cause**: Wrong format for the TXT host

**Solution**:
- Host should be: `_dnsauth.dev` (NOT `_dnsauth.dev.dialyzeflow.com`)
- Plesk automatically appends the domain

### Issue: SSL Certificate Error

**Cause**: Azure hasn't finished provisioning SSL

**Solution**:
1. Wait 10-15 minutes after validation completes
2. Azure auto-provisions free SSL certificates
3. Clear browser cache and try again

---

## Summary: Records to Add in Plesk

| # | Type | Domain name (Host) | Value | Purpose |
|---|------|-------------------|-------|---------|
| 1 | TXT | `_dnsauth.dev` | `[Azure validation code]` | Domain ownership verification |
| 2 | CNAME | `dev` | `lively-pond-08e4f7c00.3.azurestaticapps.net.` | Point subdomain to Azure |

---

## Future Subdomains

When you're ready for production, add these records for other environments:

### For staging.dialyzeflow.com
```
TXT   _dnsauth.staging   [Azure validation code for staging]
CNAME staging            [staging-app].azurestaticapps.net.
```

### For app.dialyzeflow.com (Production)
```
TXT   _dnsauth.app       [Azure validation code for production]
CNAME app                [production-app].azurestaticapps.net.
```

### For Hospital Subdomains (Wildcard)
If you want all hospital subdomains to work:
```
CNAME *                  [your-main-app].azurestaticapps.net.
```
> Note: Wildcard DNS works, but each subdomain still needs to be added in Azure Portal for SSL.

---

## Need Help?

- **HostingRaja Support**: Contact via their support portal
- **Azure Support**: [Azure Static Web Apps Documentation](https://learn.microsoft.com/en-us/azure/static-web-apps/custom-domain-external)
- **DNS Propagation Check**: https://dnschecker.org

---

*Guide created for DialyzeFlow deployment - December 2024*
