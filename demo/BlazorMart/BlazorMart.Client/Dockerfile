FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /app-build
COPY *.csproj ./
RUN dotnet restore
COPY ./ ./
RUN dotnet publish -c Release -o out

FROM nginx:stable as server
COPY --from=build /app-build/out/BlazorMart.Client/dist/ /var/www
COPY ./nginx.conf /etc/nginx/conf.d/default.conf
CMD ["nginx", "-g", "daemon off;"]
