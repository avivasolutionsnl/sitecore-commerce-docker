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

# Stage 1: perform actual build
FROM sitecore-docker_sitecore

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

ARG COMMERCE_CERT_PATH
ARG ROOT_CERT_PATH
ARG WEB_TRANSFORM_TOOL
ARG COMMERCE_CONNECT_ENGINE_PACKAGE

COPY --from=prepare /Files/CommerceSIF /Files/CommerceSIF/
COPY --from=prepare /Files/SitecoreCommerceConnectCore /Files/SitecoreCommerceConnectCore
COPY --from=prepare /Files/${WEB_TRANSFORM_TOOL} /Files/Microsoft.Web.XmlTransform.dll
COPY --from=prepare /Files/${COMMERCE_CONNECT_ENGINE_PACKAGE} /Files/Sitecore.Commerce.Engine.Connect.update
COPY --from=prepare /Files/${ROOT_CERT_PATH} /Files/${ROOT_CERT_PATH}

ADD sitecore/InstallCommercePackages.ps1 /Scripts/
ADD files/$COMMERCE_CERT_PATH /Files/
ADD scripts/Import-Certificate.ps1 /Scripts/

# Trust Self signed certificates
RUN /Scripts/Import-Certificate.ps1 -certificateFile /Files/$Env:COMMERCE_CERT_PATH -secret 'secret' -storeName 'Root' -storeLocation 'LocalMachine'
RUN /Scripts/Import-Certificate.ps1 -certificateFile /Files/$Env:ROOT_CERT_PATH -secret 'secret' -storeName 'Root' -storeLocation 'LocalMachine'

# Import XConnect certificate
RUN /Scripts/Import-Certificate.ps1 -certificateFile /Files/$Env:COMMERCE_CERT_PATH -secret 'secret' -storeName 'My' -storeLocation 'LocalMachine'
RUN /Scripts/Import-Certificate.ps1 -certificateFile /Files/$Env:ROOT_CERT_PATH -secret 'secret' -storeName 'My' -storeLocation 'LocalMachine'

ENTRYPOINT /Scripts/Watch-Directory.ps1 -Path C:\Workspace -Destination c:\inetpub\wwwroot\sitecore
