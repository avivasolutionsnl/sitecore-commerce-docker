[Environment]::SetEnvironmentVariable('PSModulePath', $env:PSModulePath + ';/Files/CommerceSIF/Modules');

Copy-Item -Path /Files/CommerceSIF/SiteUtilityPages -Destination c:\\inetpub\\wwwroot\\sitecore\\SiteUtilityPages -Force -Recurse

Install-SitecoreConfiguration -Path '/Files/CommerceSIF/Configuration/Commerce/Connect/Connect.json' `
    -ModuleFullPath '/Files/SitecoreCommerceConnectCore/package.zip' `
    -ModulesDirDst c:\\inetpub\wwwroot\\sitecore\\App_Data\\packages `
    -BaseUrl 'http://sitecore/SiteUtilityPages'
   
Install-SitecoreConfiguration -Path '/Files/CommerceSIF/Configuration/Commerce/CEConnect/CEConnect.json' `
    -PackageFullPath /Files/Sitecore.Commerce.Engine.Connect.2.0.835.update `
    -PackagesDirDst c:\\inetpub\wwwroot\\sitecore\\sitecore\\admin\\Packages `
    -BaseUrl 'http://sitecore/SiteUtilityPages' `
    -MergeTool '/Files/Microsoft.Web.XmlTransform.dll' `
    -InputFile c:\\inetpub\\wwwroot\\sitecore\\MergeFiles\\Sitecore.Commerce.Engine.Connectors.Merge.Config `
    -WebConfig c:\\inetpub\\wwwroot\\sitecore\\web.config

# Modify the commerce engine connection
$engineConnectIncludeDir = 'c:\\inetpub\\wwwroot\\sitecore\\App_Config\\Include\\Y.Commerce.Engine'; `
$cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2; `
$cert.Import('c:\\Files\\commerce.pfx', 'secret', 'MachineKeySet'); `
$pathToConfig = $(Join-Path -Path $engineConnectIncludeDir -ChildPath "\Sitecore.Commerce.Engine.Connect.config"); `
$xml = [xml](Get-Content $pathToConfig); `
$node = $xml.configuration.sitecore.commerceEngineConfiguration; `
$node.certificateThumbprint = $cert.Thumbprint; `
$node.shopsServiceUrl = 'http://commerce:5000/api/'; `
$node.commerceOpsServiceUrl = 'http://commerce:5000/commerceops/'; `
$node.defaultEnvironment = 'MercuryFoodAuthoring'; `
$node.defaultShopName = 'MercuryFood'; `
$xml.Save($pathToConfig);