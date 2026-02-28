FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY PanOpticon.sln .
COPY PanOpticon/PanOpticon.csproj PanOpticon/
RUN dotnet restore PanOpticon/PanOpticon.csproj

COPY PanOpticon/ PanOpticon/
RUN dotnet build PanOpticon/PanOpticon.csproj -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/build .
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "PanOpticon.dll"]
