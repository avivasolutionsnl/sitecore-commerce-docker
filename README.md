> THIS REPO IS NO LONGER MAINTAINED.
>
> The Sitecore XC Docker configuration is now available in [sitecore-docker](https://github.com/avivasolutionsnl/sitecore-docker).

Run Sitecore Commerce 9 using Docker and Windows containers.

# Disclaimer
This repository contains experimental code that we use in development setups. We do not consider the current code in this repository ready for production.
Hopefully this will help you to get up and running with Sitecore and Docker. By no means we consider ourselves Docker experts and thus expect these images to still contain a lot of bugs. Great help for creating this setup was provided by the [sitecoreops](https://github.com/sitecoreops/sitecore-images) and [sitecore-nine-docker](https://github.com/pbering/sitecore-nine-docker) repos. Please feel free to provide feedback by creating an issue, PR, etc. 

# Requirements
- Windows 10 update 1709 (with Hyper-V enabled)
- Docker for Windows (version 1712 or better): https://docs.docker.com/docker-for-windows/
- Visual Studio 15.5.3
- Sitecore Commerce 9 installation files
- [Nuke.build](https://nuke.build)


# Build
As Sitecore does not distribute Docker images, the first step is to build the required Docker images.

## Pre-build steps
For this you need the Sitecore installation files and a Sitecore license file. Plumber is installed to inspect Commerce pipelines, download it [here](https://github.com/ewerkman/plumber-sc/releases) and save it as `files/plumber.zip`. What files to use are set in the [build configuration](./build/Build.cs).

As this Sitecore Commerce Docker build relies on Sitecore Docker, first build the Sitecore Docker images: https://github.com/avivasolutionsnl/sitecore-docker
From the Sitecore Docker `files` directory copy all `.pfx` certificate files to the `files/` directory.

The Commerce setup requires by default SSL between the services, for this we need (more) self signed certificates. You can generate these by running the `./Generate-Certificates.ps1` script (note that this requires an Administrator elevated powershell environment and you may need to set the correct execution policy, e.g. `PS> powershell.exe -ExecutionPolicy Unrestricted`).

## Build step
Build all images using:
```
PS> nuke 
```

The build results in the following Docker images:
- commerce: ASP.NET
- mssql: MS SQL + Sitecore databases
- sitecore: IIS + ASP.NET + Sitecore
- solr: Apache Solr

and three SXA images:
- sitecore-sxa
- solr-sxa
- mssql-sxa

### Push images
Push the Docker images to your repository, e.g:
```
PS> nuke push
```


# Run
Docker compose is used to start up all required services.

Place the Sitecore source files in the `.\wwwroot\sitecore` directory and Commerce source files in `.\wwwroot\commerce`.

Create the log directories which are mounted in the Docker compose file:
```
PS> ./CreateLogDirs.ps1
```

To start Sitecore;
```
PS> docker-compose up
```

or to start Sitecore with SXA:
```
PS> docker-compose -f docker-compose.yml -f docker-compose.sxa.yml up
```

Run-time parameters can be modified using the `.env` file:

| Field                     | Description                                      |
| ------------------------- | ------------------------------------------------ |
| SQL_SA_PASSWORD           | The password to use for the SQL sa user          |
| SITECORE_SITE_NAME        | Host name of the Sitecore site                   |
| IMAGE_PREFIX              | The Docker image prefix to use                   |
| TAG                       | The version to tag the Docker images with        |


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
