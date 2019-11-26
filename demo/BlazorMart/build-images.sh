cp ./BlazorMart.Server/Protos/*.proto ./BlazorMart.Client/
docker build -t blazormart-client ./BlazorMart.Client
docker build -t blazormart-server ./BlazorMart.Server
