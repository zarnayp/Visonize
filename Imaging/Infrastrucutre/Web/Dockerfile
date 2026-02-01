# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
# For more information, please see https://aka.ms/containercompat



# This stage is used to build the service project
FROM mcr.microsoft.com/windows:1809
#FROM mcr.microsoft.com/windows/servercore:ltsc2022

EXPOSE 8080
EXPOSE 8081

# Install .NET 8 SDK
RUN powershell.exe -Command \
    Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force; \
    Invoke-WebRequest -Uri https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.ps1 -OutFile dotnet-install.ps1; \
    powershell.exe -File dotnet-install.ps1 -InstallDir 'C:\Program Files\dotnet' -Channel 8.0 -Architecture x64; \
    Remove-Item -Force dotnet-install.ps1

    # Set the PATH environment variable to include the .NET installation directory
RUN setx PATH "%PATH%;C:\Program Files\dotnet"

RUN dotnet --info


ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["UltrasoundImaging/Infrastrucutre/Web/DupploPulse.UsImaging.Infrastructure.Web.csproj", "UltrasoundImaging/Infrastrucutre/Web/"]
COPY ["UltrasoundImaging/Domain/DupploPulse.UsImaging.Domain.csproj", "UltrasoundImaging/Domain/"]
COPY ["PragmaticScene/Scene/PragmaticScene.csproj", "PragmaticScene/Scene/"]
COPY ["UltrasoundImaging/Infrastrucutre/CompositionRoot/DupploPulse.UsImaging.Infrastracture.CompositionRoot.csproj", "UltrasoundImaging/Infrastrucutre/CompositionRoot/"]
COPY ["UltrasoundImaging/Infrastrucutre/Renderer/DupploPulse.UsImaging.Infrastracture.Renderer.csproj", "UltrasoundImaging/Infrastrucutre/Renderer/"]
COPY ["PragmaticScene/Renderer/PragmaticScene.Renderer.csproj", "PragmaticScene/Renderer/"]
COPY ["UltrasoundImaging/Infrastrucutre/ImageSource/DupploPulse.UsImaging.Infrastracture.ImageSource.csproj", "UltrasoundImaging/Infrastrucutre/ImageSource/"]
RUN dotnet restore "./UltrasoundImaging/Infrastrucutre/Web/DupploPulse.UsImaging.Infrastructure.Web.csproj"
COPY . .
WORKDIR "/src/UltrasoundImaging/Infrastrucutre/Web"
RUN dotnet build "./DupploPulse.UsImaging.Infrastructure.Web.csproj" -c %BUILD_CONFIGURATION% -o /app/build -p:Platform=x64

# This stage is used to publish the service project to be copied to the final stage
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./DupploPulse.UsImaging.Infrastructure.Web.csproj" -c %BUILD_CONFIGURATION% -o /app/publish /p:UseAppHost=false -p:Platform=x64



# This stage is used when running from VS in fast mode (Default for Debug configuration)
#FROM mcr.microsoft.com/windows:1809 AS base
#USER ContainerUser

# Install .NET 8 SDK  
# todo: maybe just runtime is enough
#RUN powershell.exe -Command \
 #   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force; \
#    Invoke-WebRequest -Uri https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.ps1 -OutFile dotnet-install.ps1; \
 #   powershell.exe -File dotnet-install.ps1 -InstallDir C:\dotnet -Channel 8.0 -Architecture x64; \
#    Remove-Item -Force dotnet-install.ps1

    # Set the PATH environment variable to include the .NET installation directory
#RUN setx PATH "%PATH%;C:\dotnet"

RUN powershell.exe "Get-ItemProperty -Path "HKLM:\Software\Microsoft\DirectX" | Select-Object -Property Version"

WORKDIR /app

COPY ["native/GraphicsEngineD3D12.dll", ""]
                                 #       C:\app\bin\x64\Debug\net8.0\System.Drawing.Common.dll
#COPY --from=publish /app/publish .

CMD ["dotnet", "DupploPulse.UsImaging.Infrastructure.Web.dll"]

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
