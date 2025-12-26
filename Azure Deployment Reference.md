az staticwebapp list --resource-group EnsateBlogRG --query "[].{name:name, defaultHostname:defaultHostname, customDomains:customDomains}"
 --output json

Quick deployments: deploy-staticwebapp.ps1
Full stack: deploy-apps-only.ps1
Backend only: .[deploy-apps-only.ps1](http://_vscodecontentref_/2) -SkipFrontend