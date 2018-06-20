# escape=`
FROM sitecore-docker_xconnect

SHELL ["powershell", "-Command", "$ErrorActionPreference = 'Stop'; $ProgressPreference = 'SilentlyContinue';"]

# Fix models conflict for SC9 update 1: https://blog.ryanbailey.co.nz/2018/03/sitecore-experience-commerce-one-or.html
COPY xconnect/Sitecore.Commerce.Connect.XConnect.Models.json /Files/
RUN cp /Files/Sitecore.Commerce.Connect.XConnect.Models.json C:\inetpub\wwwroot\xconnect\App_data\jobs\continuous\IndexWorker\App_data\Models\; `
    cp /Files/Sitecore.Commerce.Connect.XConnect.Models.json C:\inetpub\wwwroot\xconnect\App_data\Models\
