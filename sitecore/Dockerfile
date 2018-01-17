# escape=`

# Stage 0: prepare files
FROM microsoft/aspnet:4.7.1-windowsservercore-1709 AS prepare

ARG COMMERCE_SIF_PACKAGE
ARG COMMERCE_CONNECT_PACKAGE
ARG COMMERCE_DEPLOYMENT_HELPER_PACKAGE

SHELL ["powershell", "-NoProfile", "-Command", "$ErrorActionPreference = 'Stop';"]

ADD files/ /Files/

RUN Expand-Archive -Path "/Files/$Env:COMMERCE_SIF_PACKAGE" -DestinationPath /Files/CommerceSIF -Force
RUN Expand-Archive -Path "/Files/$Env:COMMERCE_CONNECT_PACKAGE" -DestinationPath /Files/SitecoreCommerceConnectCore -Force
RUN Expand-Archive -Path "/Files/$Env:COMMERCE_DEPLOYMENT_HELPER_PACKAGE" -DestinationPath /Files/Sitecore.Commerce.Deployment.HelperTools -Force


# Stage 1: perform actual build
FROM sitecoredocker_sitecore

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

ARG COMMERCE_CERT_PATH

COPY --from=prepare /Files/CommerceSIF /Files/CommerceSIF/
COPY --from=prepare /Files/SitecoreCommerceConnectCore /Files/SitecoreCommerceConnectCore
COPY --from=prepare /Files/Sitecore.Commerce.Deployment.HelperTools /Files/Sitecore.Commerce.Deployment.HelperTools

ADD sitecore/InstallCommercePackages.ps1 /Scripts/
ADD files/$COMMERCE_CERT_PATH /Files/
ADD scripts/Import-Certificate.ps1 /Scripts/

# Trust Self signed certificates
RUN /Scripts/Import-Certificate.ps1 -certificateFile /Files/$Env:COMMERCE_CERT_PATH -secret 'secret' -storeName 'Root' -storeLocation 'LocalMachine'

# Import XConnect certificate
RUN /Scripts/Import-Certificate.ps1 -certificateFile /Files/$Env:COMMERCE_CERT_PATH -secret 'secret' -storeName 'My' -storeLocation 'LocalMachine'
