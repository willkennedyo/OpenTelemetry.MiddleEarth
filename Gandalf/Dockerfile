#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8090
EXPOSE 44428

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Gandalf/Gandalf.csproj", "Gandalf/"]
RUN dotnet restore "Gandalf/Gandalf.csproj"
COPY . .
WORKDIR "/src/Gandalf"
RUN dotnet build "Gandalf.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Gandalf.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gandalf.dll"]
