./build-images.sh
kubectl delete -f resource-manifests

kubectl apply -f resource-manifests/filter.yaml
kubectl apply -f <(istioctl kube-inject -f resource-manifests/backend.yaml)
kubectl apply -f resource-manifests/frontend.yaml
kubectl apply -f resource-manifests/gateway.yaml
