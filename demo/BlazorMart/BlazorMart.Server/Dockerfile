FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /app-build
COPY *.csproj ./
RUN dotnet restore
COPY ./ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS runtime
WORKDIR /app
COPY --from=build /app-build/out ./
COPY ./mycert.pfx /cert/
ENV ASPNETCORE_URLS https://+;http://+
ENV ASPNETCORE_Kestrel__Certificates__Default__Path /cert/mycert.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password mycertpassword
ENTRYPOINT ["dotnet", "BlazorMart.Server.dll"]
