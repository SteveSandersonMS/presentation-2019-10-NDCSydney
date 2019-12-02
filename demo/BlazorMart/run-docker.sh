./build-images.sh

# Start client
docker run --rm -d -p 8000:80 --name blazormart-client-instance blazormart-client

# Start server
docker create --rm -p 8001:443 --name blazormart-server-instance blazormart-server
docker start -i blazormart-server-instance
docker kill blazormart-client-instance
