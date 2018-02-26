Run Sitecore Commerce 9 using Docker and Windows containers.

# Disclaimer
This repository contains experimental code that we use in development setups. We do not consider the current code in this repository ready for production.
Hopefully this will help you to get up and running with Sitecore and Docker. By no means we consider ourselves Docker experts and thus expect these images to still contain a lot of bugs. Great help for creating this setup was provided by the [sitecoreops](https://github.com/sitecoreops/sitecore-images) and [sitecore-nine-docker](https://github.com/pbering/sitecore-nine-docker) repos. Please feel free to provide feedback by creating an issue, PR, etc. 

# Requirements
- Windows 10 update 1709 (with Hyper-V enabled)
- Docker for Windows (version 1712 or better): https://docs.docker.com/docker-for-windows/
- Visual Studio 15.5.3
- Sitecore Commerce 9 installation files

# Build
As Sitecore does not distribute Docker images, the first step is to build the required Docker images.
For this you need the Sitecore installation files and a Sitecore license file. What files to use are set by environment variables (interpreted by docker-compose); download all the packages that are defined by variables in the `.env.` file.

As this Sitecore Commerce Docker build relies on Sitecore Docker, first build the Sitecore Docker images: https://github.com/avivasolutionsnl/sitecore-docker

The xp0 Sitecore topology requires SSL between the services, for this we need self signed certificates for the 
xConnect and SOLR roles. You can generate these by running the './Generate-Certificates.ps1' script. 

Next, modify the .env file and change the build parameters if needed:

| Field                     | Description                                      |
| ------------------------- | ------------------------------------------------ |
| SQL_SA_PASSWORD           | The password to use for the SQL sa user          |
| SQL_DB_PREFIX             | Prefix to use for all DB names                   |
| SOLR_HOST_NAME            | Host name to use for the SOLR instance           |
| SOLR_PORT                 | Port to use for the SOLR instance                |
| SOLR_SERVICE_NAME         | Name of the SOLR Windows service                 |
| SITECORE_SITE_NAME        | Host name of the Sitecore site                   |
| SITECORE_SOLR_CORE_PREFIX | Prefix to use for the Sitecore SOLR cores        |
| WEB_TRANSFORM_TOOL        | Reference to the XML transform tool (Microsoft.Web.XmlTransform.dll). Needs to be placed in the files folder and can be found in the Visual Studio program files folder |

The build results in the following Docker images:
- commerce: ASP.NET
- mssql: MS SQL + Sitecore databases
- sitecore: IIS + ASP.NET + Sitecore
- solr: Apache Solr 

# Run
Docker compose is used to start up all required services.

Place the Sitecore source files in the `.\wwwroot\sitecore` directory.

Create the log directories which are mounted in the Docker compose file:
```
PS> ./CreateLogDirs.ps1
```

Create a webroot directory:
```
PS> mkdir -p wwwroot/sitecore
```

To start Sitecore:
```
PS> docker-compose up
```

For the first run an initialization step is required in the `sitecore` container (retry when it fails). This is needed because not all installation steps can be run isolated. For example, to install sitecore packages, sitecore and its dependencies need to be running. The script will install the commerce connect packages, initialize a default environment and enable the Sitecore commerce data provider. 

The script takes the following parameters, which have default values:

| Parameter                 | Description                                      |
| ------------------------- | ------------------------------------------------ |
| certificateFile           | The certficate file that contains the thumbprint used to authenticate against commerce server  |
| shopsServiceUrl           | The url of the commerce server shops service     |
| commerceOpsServiceUrl     | The url of the commerce server ops service       |
| identityServerUrl         | The url of the identity server                   |
| defaultEnvironment        | Name of the default environment                  |
| defaultShopName           | Name of the default shop                         |
| sitecoreUserName          | Sitecore user name                               |
| sitecorePassword          | Sitecore password                                |

```
PS> docker exec sitecore powershell -Command "C:\Scripts\InstallCommercePackages.ps1"
```

The business roles are not created well, so in Sitecore create a role named Commerce Business Users make it a member of the admin group.

After this final installation step commit all changes to the Docker images:
```
PS> docker commit
```

## DNS
The containers have fixed IP addresses in the docker compose file. The easiest way to access the containers from the host is by adding the following to your hosts file:

``` Hosts
172.16.238.10	solr
172.16.238.11	mssql
172.16.238.12	xconnect
172.16.238.13	sitecore
172.16.238.14	commerce
```

## Log files
Logging is set up to log on the host under the logs folder of this repository. 

# Known issues
- We have quite a lot of custom powershell scripts for trivial installation tasks. This is because the commerce SIF scripts contain hardcoded values. For example, it is not possible to use hostnames other than localhost. We should be able to remove this custom code when those scripts get fixed.
- During the installation of the commerce server instances, it tries to set permissions on the log folder. For some reason, this results in an exception saying the access control list is not in canonical form. This can be ignored, because the log folders are mounted on the host. However, it does cause an annoying delay in the installation. 
- Docker for Windows can be unstable at times, some troubleshooting tips are listed below.

## Containers not reachable by domain name
Sometimes the internal Docker DNS is malfunctioning and containers (e.g. mssql) cannot be reached by domain name. To solve this restart the Docker daemon.

### Clean up network hosting
In case it's no longer possible to create networks and docker network commands don't work give this a try: https://github.com/MicrosoftDocs/Virtualization-Documentation/tree/live/windows-server-container-tools/CleanupContainerHostNetworking

## Clean Docker install
In case nothing else helps, perform a clean Docker install using the following steps:
- Uninstall Docker

- Check that no Windows Containers are running (https://docs.microsoft.com/en-us/powershell/module/hostcomputeservice/get-computeprocess?view=win10-ps):
```
PS> Get-ComputeProcess
```
and if so, stop them using `Stop-ComputeProcess`.

- Remove the `C:\ProgramData\Docker` directory (and Windows Containers) using the [docker-ci-zap](https://github.com/jhowardmsft/docker-ci-zap) tool as administrator in `cmd`:
```
PS> docker-ci-zap.exe -folder "c:\ProgramData\Docker"
```

- Install Docker
