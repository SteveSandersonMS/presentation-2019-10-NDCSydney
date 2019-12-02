./build-images.sh
kubectl delete -f resource-manifests/kube
kubectl delete -f resource-manifests/istio
kubectl apply -f resource-manifests/kube
kubectl apply -f resource-manifests/istio
