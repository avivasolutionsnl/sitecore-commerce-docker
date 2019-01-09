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

## Pre-build steps
For this you need the Sitecore installation files and a Sitecore license file. Plumber is installed to inspect Commerce pipelines, download it [here](https://github.com/ewerkman/plumber-sc/releases) and save it as `files/plumber.zip`. What files to use are set by environment variables (interpreted by docker-compose); download all the packages that are defined by variables in the `.env.` file.

As this Sitecore Commerce Docker build relies on Sitecore Docker, first build the Sitecore Docker images: https://github.com/avivasolutionsnl/sitecore-docker
From the Sitecore Docker `files` directory copy all `.pfx` certificate files to the `files/` directory.

The Commerce setup requires by default SSL between the services, for this we need (more) self signed certificates. You can generate these by running the `./Generate-Certificates.ps1` script (note that this requires an Administrator elevated powershell environment and you may need to set the correct execution policy, e.g. `PS> powershell.exe -ExecutionPolicy Unrestricted`).

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
| TAG                       | The version to tag the Docker images with        |

## Build step
Now perform the Docker build step:
```
PS> docker-compose build
```

> Optionally use an overlay `docker-compose.yml` file to give the images custom tags, e.g. `docker-compose -f docker-compose.yml -f docker-compose.aviva.yml build`

The build results in the following Docker images:
- commerce: ASP.NET
- mssql: MS SQL + Sitecore databases
- sitecore: IIS + ASP.NET + Sitecore
- solr: Apache Solr

## Post-build steps
Post-build steps require a running system. 

Create the log directories which are mounted in the Docker compose file:
```
PS> ./CreateLogDirs.ps1
```

Create a webroot directory:
```
PS> mkdir -p wwwroot/sitecore
PS> mkdir -p wwwroot/commerce
```

To start Sitecore:
```
PS> docker-compose up
```

### Install Commerce package
Install the Commerce Connect packages, initialize a default environment and enable the Sitecore commerce data provider. 

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

NB. the `InstallCommercePackages.ps1` script requires (by default) the Commerce container to be reachable by DNS at e.g. https://commerce:5000.

```
PS> docker exec sitecore-commerce-docker_sitecore_1 powershell -Command "C:\Scripts\InstallCommercePackages.ps1"
```

Stop the containers and store the changes, e.g:
```
PS> docker commit sitecore-commerce-docker_sitecore_1 sxc-sitecore:9.0.3
PS> docker commit sitecore-commerce-docker_mssql_1 sxc-mssql:9.0.3
```

Optionally correctly tag the stored images, e.g:
```
PS> docker tag sxc-sitecore:9.0.3 avivasolutionsnl.azurecr.io/sxc-sitecore:9.0.3
PS> docker tag sxc-mssql:9.0.3 avivasolutionsnl.azurecr.io/sxc-mssql:9.0.3
```

### (Optionally) Install Sitecore SXA packages
Copy the `PSE_PACKAGE`, `SXA_PACKAGE`, and `SCXA_PACKAGE` (defined in the `.env` file) into the files directory.

Install SXA Solr cores:
```
PS> docker-compose -f docker-compose.install-sxa.yml build solr
```

Install SXA using: 
```
PS> docker-compose -f docker-compose.yml -f docker-compose.install-sxa.yml up
PS> docker exec -ti sitecore-commerce-docker_sitecore_1 powershell -Command "C:\sxa\InstallSXA.ps1"
```

Store the changes, e.g:
```
PS> docker commit sitecore-commerce-docker_sitecore_1 sxc-sitecore-sxa:9.0.3
PS> docker commit sitecore-commerce-docker_mssql_1 sxc-mssql-sxa:9.0.3
```

Optionally tag the stored images, e.g:
```
PS> docker tag sxc-sitecore-sxa:9.0.3 avivasolutionsnl.azurecr.io/sxc-sitecore-sxa:9.0.3
PS> docker tag sxc-mssql-sxa:9.0.3 avivasolutionsnl.azurecr.io/sxc-mssql-sxa:9.0.3
PS> docker tag sxc-solr-sxa:9.0.3 avivasolutionsnl.azurecr.io/sxc-solr-sxa:9.0.3
```

### (Optionally) Obtain database files from images
Obtain mountable files from the stopped containers:
- Copy SQL databases to `databases`: `PS> ./CopyDatabases.ps1`
- Copy Solr cores to `cores`: `PS> ./CopyCores.ps1`

### Push images
Push the Docker images to your repository, e.g:
```
PS> docker-compose -f docker-compose.yml -f docker-compose.aviva.yml push
```

# Run
Docker compose is used to start up all required services.

Place the Sitecore source files in the `.\wwwroot\sitecore` directory.

## Plumber
Plumber is available at: http://commerce:4000

## DNS
To set the Docker container service names as DNS names on your host edit your `hosts` file. 
A convenient tool to automatically do this is [whales-names](https://github.com/gregolsky/whales-names).

## Log files
Logging is set up to log on the host under the logs folder of this repository. 

# Known issues
Docker for Windows can be unstable at times, some troubleshooting tips are listed below and [here](https://github.com/avivasolutionsnl/sitecore-docker)

## Commerce setup
- We have quite a lot of custom powershell scripts for trivial installation tasks. This is because the commerce SIF scripts contain hardcoded values. For example, it is not possible to use hostnames other than localhost. We should be able to remove this custom code when those scripts get fixed.
- During the installation of the commerce server instances, it tries to set permissions on the log folder. For some reason, this results in an exception saying the access control list is not in canonical form. This can be ignored, because the log folders are mounted on the host. However, it does cause an annoying delay in the installation. 
