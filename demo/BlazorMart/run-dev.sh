# Before this will work, you need to create a dev cert at ~/.aspnet/https/aspnetapp.pfx
# with password 'mycertpassword'
# For example, run:
#    dotnet dev-certs https -ep ${HOME}/.aspnet/https/aspnetapp.pfx -p mycertpassword
# or run the equivalent for Windows then copy the .pfx file to ~/.aspnet/https/
# More info at https://docs.microsoft.com/en-us/aspnet/core/security/docker-https?view=aspnetcore-3.0
# Unfortunately it's not enough to run without HTTPS because gRPC only allows insecure connections
# if you only enable HTTP2 protocol. We need HTTP1 as well for gRPC-Web.

# Build images
cp ./BlazorMart.Server/Protos/*.proto ./BlazorMart.Client/
docker build -t blazormart-client ./BlazorMart.Client
docker build -t blazormart-server ./BlazorMart.Server

# Kill any existing instances
docker kill blazormart-client-instance
docker kill blazormart-server-instance

# Start client
docker rm blazormart-client-instance
docker run --rm -d -p 8000:80 --name blazormart-client-instance blazormart-client

# Start server
docker rm blazormart-server-instance
docker create --rm -p 8001:443 \
    -e ASPNETCORE_URLS="https://+;http://+" \
    -e ASPNETCORE_Kestrel__Certificates__Default__Password="mycertpassword" \
    -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx \
    --name blazormart-server-instance blazormart-server
docker cp ~/.aspnet/https blazormart-server-instance:/https/
docker start -i blazormart-server-instance
docker kill blazormart-client-instance
