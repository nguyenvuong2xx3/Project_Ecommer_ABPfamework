FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

RUN dotnet tool install -g Microsoft.Web.LibraryManager.Cli
RUN ln -s ~/.dotnet/tools/libman /usr/local/bin/libman

WORKDIR /src
COPY ["src/Acme.SimpleTaskApp.Web.Mvc/Acme.SimpleTaskApp.Web.Mvc.csproj", "src/Acme.SimpleTaskApp.Web.Mvc/"]
COPY ["src/Acme.SimpleTaskApp.Web.Core/Acme.SimpleTaskApp.Web.Core.csproj", "src/Acme.SimpleTaskApp.Web.Core/"]
COPY ["src/Acme.SimpleTaskApp.Application/Acme.SimpleTaskApp.Application.csproj", "src/Acme.SimpleTaskApp.Application/"]
COPY ["src/Acme.SimpleTaskApp.Core/Acme.SimpleTaskApp.Core.csproj", "src/Acme.SimpleTaskApp.Core/"]
COPY ["src/Acme.SimpleTaskApp.EntityFrameworkCore/Acme.SimpleTaskApp.EntityFrameworkCore.csproj", "src/Acme.SimpleTaskApp.EntityFrameworkCore/"]
WORKDIR "/src/src/Acme.SimpleTaskApp.Web.Mvc"
RUN dotnet restore 

WORKDIR /src
COPY ["src/Acme.SimpleTaskApp.Web.Mvc", "src/Acme.SimpleTaskApp.Web.Mvc"]
COPY ["src/Acme.SimpleTaskApp.Web.Core", "src/Acme.SimpleTaskApp.Web.Core"]
COPY ["src/Acme.SimpleTaskApp.Application", "src/Acme.SimpleTaskApp.Application"]
COPY ["src/Acme.SimpleTaskApp.Core", "src/Acme.SimpleTaskApp.Core"]
COPY ["src/Acme.SimpleTaskApp.EntityFrameworkCore", "src/Acme.SimpleTaskApp.EntityFrameworkCore"]
WORKDIR "/src/src/Acme.SimpleTaskApp.Web.Mvc"
RUN libman restore
RUN dotnet publish -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 80
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "Acme.SimpleTaskApp.Web.Mvc.dll"]
