#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8000
EXPOSE 8001

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Bilbo/Bilbo.csproj", "Bilbo/"]
RUN dotnet restore "Bilbo/Bilbo.csproj"
COPY . .
WORKDIR "/src/Bilbo"
RUN dotnet build "Bilbo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Bilbo.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bilbo.dll"]
