FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /app-build
COPY *.csproj ./
RUN dotnet restore
COPY ./ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS runtime
WORKDIR /app
COPY --from=build /app-build/out ./
ENTRYPOINT ["dotnet", "BlazorMart.Server.dll"]
