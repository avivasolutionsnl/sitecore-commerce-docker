using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.ProjectModel;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Docker.DockerTasks;
using Nuke.Docker;
using Nuke.Common.Tooling;

class Build : NukeBuild
{
    public static int Main () => Execute<Build>(x => x.All);

    // Tools
    [PathExecutable(name: "docker-compose")] readonly Tool DockerCompose;

    [PathExecutable] readonly Tool Powershell;

    // Docker image naming
    [Parameter("Docker image prefix for XC")]
    readonly string XcImagePrefix = "sitecore-commerce-docker_";

    [Parameter("Docker image prefix for XP")]
    readonly string XpImagePrefix = "sitecore-docker_";

    [Parameter("Docker image version tag for XC")]
    readonly string XcVersion = "9.0.3";

    [Parameter("Docker image version tag for XP")]
    readonly string XpVersion = "9.0.2";

    // Packages
    [Parameter("Sitecore Identity server package")]
    readonly string SITECORE_IDENTITY_PACKAGE = "Sitecore.IdentityServer.1.4.2.zip";

    [Parameter("Sitecore BizFx package")]
    readonly string SITECORE_BIZFX_PACKAGE = "Sitecore.BizFX.1.4.1.zip";

    [Parameter("Commerce Engine package")]
    readonly string COMMERCE_ENGINE_PACKAGE = "Sitecore.Commerce.Engine.2.4.63.zip";

    [Parameter("Commerce Connect package")]
    readonly string COMMERCE_CONNECT_PACKAGE = "Sitecore Commerce Connect Core 11.4.15.zip";

    [Parameter("Commerce Connect Engine package")]
    readonly string COMMERCE_CONNECT_ENGINE_PACKAGE = "Sitecore.Commerce.Engine.Connect.2.4.32.update";

    [Parameter("Commerce Marketing Automation package")]
    readonly string COMMERCE_MA_PACKAGE = "Sitecore Commerce Marketing Automation Core 11.4.15.zip";

    [Parameter("Commerce Marketing Automation for AutomationEngine package")]
    readonly string COMMERCE_MA_FOR_AUTOMATION_ENGINE_PACKAGE = "Sitecore Commerce Marketing Automation for AutomationEngine 11.4.15.zip";

    [Parameter("Commerce SIF package")]
    readonly string COMMERCE_SIF_PACKAGE = "SIF.Sitecore.Commerce.1.4.7.zip";

    [Parameter("Commerce SDK package")]
    readonly string COMMERCE_SDK_PACKAGE = "Sitecore.Commerce.Engine.SDK.2.4.43.zip";

    [Parameter("Commerce XP Core package")]
    readonly string COMMERCE_XPROFILES_PACKAGE = "Sitecore Commerce ExperienceProfile Core 11.4.15.zip";

    [Parameter("Commerce XP Analytics Core package")]
    readonly string COMMERCE_XANALYTICS_PACKAGE = "Sitecore Commerce ExperienceAnalytics Core 11.4.15.zip";

    [Parameter("Powershell Extension package")]
    readonly string PSE_PACKAGE = "Sitecore PowerShell Extensions-5.0.zip";

    [Parameter("SXA package")]
    readonly string SXA_PACKAGE = "Sitecore Experience Accelerator 1.8 rev. 181112 for 9.0.zip";

    [Parameter("SXA Commerce package")]
    readonly string SCXA_PACKAGE = "Sitecore Commerce Experience Accelerator 1.4.150.zip";

    [Parameter("Web transform tool")]
    readonly string WEB_TRANSFORM_TOOL = "Microsoft.Web.XmlTransform.dll";

    [Parameter("Plumber package")]
    readonly string PLUMBER_FILE_NAME = "plumber.zip";

    // Certificates
    [Parameter("Commerce certificate file")]
    readonly string COMMERCE_CERT_PATH = "commerce.pfx";

    [Parameter("Root certificate file")]
    readonly string ROOT_CERT_PATH = "root.pfx";

    [Parameter("Sitecore certificate file")]
    readonly string SITECORE_CERT_PATH = "sitecore.pfx";

    [Parameter("Solr certificate file")]
    readonly string SOLR_CERT_PATH = "solr.pfx";

    [Parameter("Xconnect certificate file")]
    readonly string XCONNECT_CERT_PATH = "xConnect.pfx";

    // Build configuration parameters
    [Parameter("SQL password")]
    readonly string SQL_SA_PASSWORD = "my_Sup3rSecret!!";

    [Parameter("SQL db prefix")]
    readonly string SQL_DB_PREFIX = "Sitecore";

    [Parameter("Solr hostname")]
    readonly string SOLR_HOST_NAME = "solr";

    [Parameter("Solr port")]
    readonly string SOLR_PORT = "8983";

    [Parameter("Solr service name")]
    readonly string SOLR_SERVICE_NAME = "Solr-6";

    [Parameter("Xconnect site name")]
    readonly string XCONNECT_SITE_NAME = "xconnect";

    [Parameter("Xconnect Solr core prefix")]
    readonly string XCONNECT_SOLR_CORE_PREFIX = "xp0";

    [Parameter("Sitecore site name")]
    readonly string SITECORE_SITE_NAME = "sitecore";

    [Parameter("Sitecore Solr core prefix")]
    readonly string SITECORE_SOLR_CORE_PREFIX = "Sitecore";

    [Parameter("Commerce shop name")]
    readonly string SHOP_NAME = "CommerceEngineDefaultStorefront";

    [Parameter("Commerce environment name")]
    readonly string ENVIRONMENT_NAME = "HabitatAuthoring";
    
    private string XcFullImageName(string name) => $"{XcImagePrefix}{name}:{XcVersion}";

    private string XpFullImageName(string name) => $"{XpImagePrefix}{name}:{XpVersion}";

    Target Commerce => _ => _
        .Executes(() =>
        {
            DockerBuild(x => x
                .SetPath(".")
                .SetFile("commerce/Dockerfile")
                .SetTag(XcFullImageName("commerce"))
                .SetBuildArg(new string[] {
                    $"SQL_SA_PASSWORD={SQL_SA_PASSWORD}",
                    $"SQL_DB_PREFIX={SQL_DB_PREFIX}",
                    $"SOLR_PORT={SOLR_PORT}",  
                    $"SHOP_NAME={SHOP_NAME}",
                    $"ENVIRONMENT_NAME={ENVIRONMENT_NAME}",
                    $"COMMERCE_SIF_PACKAGE={COMMERCE_SIF_PACKAGE}",
                    $"COMMERCE_SDK_PACKAGE={COMMERCE_SDK_PACKAGE}",
                    $"SITECORE_BIZFX_PACKAGE={SITECORE_BIZFX_PACKAGE}",
                    $"SITECORE_IDENTITY_PACKAGE={SITECORE_IDENTITY_PACKAGE}",
                    $"COMMERCE_ENGINE_PACKAGE={COMMERCE_ENGINE_PACKAGE}",
                    $"COMMERCE_CERT_PATH={COMMERCE_CERT_PATH}",
                    $"ROOT_CERT_PATH={ROOT_CERT_PATH}",
                    $"SITECORE_CERT_PATH={SITECORE_CERT_PATH}",
                    $"SOLR_CERT_PATH={SOLR_CERT_PATH}",
                    $"XCONNECT_CERT_PATH={XCONNECT_CERT_PATH}",
                    $"PLUMBER_FILE_NAME={PLUMBER_FILE_NAME}"
                })
            );
        });

    Target Mssql => _ => _
        .Executes(() =>
        {
            var baseImage = XpFullImageName("mssql");

            DockerBuild(x => x
                .SetPath(".")
                .SetFile("mssql/Dockerfile")
                .SetTag(XcFullImageName("mssql"))
                .SetBuildArg(new string[] {
                    $"BASE_IMAGE={baseImage}",
                    $"DB_PREFIX={SQL_DB_PREFIX}",
                    $"COMMERCE_SDK_PACKAGE={COMMERCE_SDK_PACKAGE}",
                    $"COMMERCE_SIF_PACKAGE={COMMERCE_SIF_PACKAGE}"
                })
            );
        });

    Target SitecoreBase => _ => _
        .Executes(() =>
        {
            var baseImage = XpFullImageName("sitecore");

            DockerBuild(x => x
                .SetPath(".")
                .SetFile("sitecore/Dockerfile")
                .SetTag(XcFullImageName("sitecore"))
                .SetBuildArg(new string[] {
                    $"BASE_IMAGE={baseImage}",
                    $"COMMERCE_CERT_PATH={COMMERCE_CERT_PATH}",
                    $"COMMERCE_CONNECT_PACKAGE={COMMERCE_CONNECT_PACKAGE}",
                    $"WEB_TRANSFORM_TOOL={WEB_TRANSFORM_TOOL}",
                    $"COMMERCE_CONNECT_ENGINE_PACKAGE={COMMERCE_CONNECT_ENGINE_PACKAGE}",
                    $"COMMERCE_SIF_PACKAGE={COMMERCE_SIF_PACKAGE}",
                    $"COMMERCE_MA_PACKAGE={COMMERCE_MA_PACKAGE}",
                    $"COMMERCE_MA_FOR_AUTOMATION_ENGINE_PACKAGE={COMMERCE_MA_FOR_AUTOMATION_ENGINE_PACKAGE}",
                    $"COMMERCE_XPROFILES_PACKAGE={COMMERCE_XPROFILES_PACKAGE}",
                    $"COMMERCE_XANALYTICS_PACKAGE={COMMERCE_XANALYTICS_PACKAGE}",
                    $"ROOT_CERT_PATH={ROOT_CERT_PATH}"
                })
            );
        });

    Target Sitecore => _ => _
        .DependsOn(Commerce, Mssql, SitecoreBase, Solr, Xconnect, SetupDirectories)
        .Executes(() => {
            DockerCompose(@"up -d");

            // Install Commerce Connect package
            DockerExec(x => x
                .SetContainer($"{XcImagePrefix}sitecore_1")
                .SetCommand("powershell")
                .SetArgs(@"C:\Scripts\InstallCommercePackages.ps1")
                .SetInteractive(true)
                .SetTty(true)
            );

            DockerCompose("stop");

            // Commit changes
            DockerCommit(x => x
                .SetContainer($"{XcImagePrefix}mssql_1")
                .SetRepository(XcFullImageName("mssql"))
            );

            DockerCommit(x => x
                .SetContainer($"{XcImagePrefix}sitecore_1")
                .SetRepository(XcFullImageName("sitecore"))
            );

            // Remove build artefacts
            DockerCompose("down");
        });
    
    Target Solr => _ => _
        .Executes(() =>
        {
            var baseImage = XpFullImageName("solr");

            DockerBuild(x => x
                .SetPath(".")
                .SetFile("solr/Dockerfile")
                .SetTag(XcFullImageName("solr"))
                .SetBuildArg(new string[] {
                    $"BASE_IMAGE={baseImage}",
                    $"HOST_NAME={SOLR_HOST_NAME}",
                    $"PORT={SOLR_PORT}",
                    $"SERVICE_NAME={SOLR_SERVICE_NAME}",
                    $"SITECORE_CORE_PREFIX={SITECORE_SOLR_CORE_PREFIX}",
                    $"COMMERCE_SIF_PACKAGE={COMMERCE_SIF_PACKAGE}"
                })
            );
        });

    Target Xconnect => _ => _
        .Executes(() =>
        {
            var baseImage = XpFullImageName("xconnect");

            DockerBuild(x => x
                .SetPath(".")
                .SetFile("xconnect/Dockerfile")
                .SetTag(XcFullImageName("xconnect"))
                .SetBuildArg(new string[] {
                    $"BASE_IMAGE={baseImage}",
                    $"COMMERCE_MA_FOR_AUTOMATION_ENGINE_PACKAGE={COMMERCE_MA_FOR_AUTOMATION_ENGINE_PACKAGE}"
                })
            );
        });

    Target SetupDirectories => _ => _
        .Executes(() => {
            // Setup
            System.IO.Directory.CreateDirectory(@"wwwroot/commerce");
            System.IO.Directory.CreateDirectory(@"wwwroot/sitecore");
            Powershell("./CreateLogDirs.ps1");
        });
    
    Target SitecoreSxa => _ => _
        .DependsOn(Sitecore, SolrSxa, SetupDirectories)
        .Executes(() => {
            // Set env variables for docker-compose
            Environment.SetEnvironmentVariable("PSE_PACKAGE", $"{PSE_PACKAGE}", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("SXA_PACKAGE", $"{SXA_PACKAGE}", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("SCXA_PACKAGE", $"{SCXA_PACKAGE}", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("IMAGE_PREFIX", $"{XcImagePrefix}", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("TAG", $"{XcVersion}", EnvironmentVariableTarget.Process);

            DockerCompose(@"-f docker-compose.yml -f docker-compose.build-sxa.yml up -d");

            // Install SXA package
            DockerExec(x => x
                .SetContainer($"{XcImagePrefix}sitecore_1")
                .SetCommand("powershell")
                .SetArgs(@"C:\sxa\InstallSXA.ps1")
                .SetInteractive(true)
                .SetTty(true)
            );

            DockerCompose("stop");

            // Commit changes
            DockerCommit(x => x
                .SetContainer($"{XcImagePrefix}mssql_1")
                .SetRepository(XcFullImageName("mssql-sxa"))
            );

            DockerCommit(x => x
                .SetContainer($"{XcImagePrefix}sitecore_1")
                .SetRepository(XcFullImageName("sitecore-sxa"))
            );

            // Remove build artefacts
            DockerCompose("down");
        });

    Target SolrSxa => _ => _
        .DependsOn(Solr)
        .Executes(() => {
            var baseImage = XpFullImageName("solr");

            DockerBuild(x => x
                .SetPath("solr/sxa")
                .SetTag(XcFullImageName("solr-sxa"))
                .SetBuildArg(new string[] {
                    $"BASE_IMAGE={baseImage}"
                })
            );
        });

    Target Xc => _ => _
        .DependsOn(Commerce, Mssql, Sitecore, Solr, Xconnect);

    Target XcSxa => _ => _
        .DependsOn(Xc, SitecoreSxa, SolrSxa);

    Target All => _ => _
        .DependsOn(Xc, XcSxa);

    Target PushBase => _ => _
        .DependsOn(Xc)
        .Executes(() => {
            DockerPush(x => x.SetName(XcFullImageName("commerce")));
            DockerPush(x => x.SetName(XcFullImageName("mssql")));
            DockerPush(x => x.SetName(XcFullImageName("sitecore")));
            DockerPush(x => x.SetName(XcFullImageName("solr")));
            DockerPush(x => x.SetName(XcFullImageName("xconnect")));
        });
    
    Target PushSxa => _ => _
        .DependsOn(XcSxa)
        .Executes(() => {
            DockerPush(x => x.SetName(XcFullImageName("mssql-sxa")));
            DockerPush(x => x.SetName(XcFullImageName("sitecore-sxa")));
            DockerPush(x => x.SetName(XcFullImageName("solr-sxa")));
        });

    Target Push => _ => _
        .DependsOn(PushBase)
        .DependsOn(PushSxa);
}
