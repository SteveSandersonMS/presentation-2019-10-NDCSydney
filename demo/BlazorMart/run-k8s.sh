./build-images.sh
kubectl delete -f resource-manifests
kubectl apply -f resource-manifests/filter.yaml # May need to run first
kubectl apply -f resource-manifests

export INGRESS_PORT=$(kubectl -n istio-system get service istio-ingressgateway -o jsonpath='{.spec.ports[?(@.name=="http2")].nodePort}')
export SECURE_INGRESS_PORT=$(kubectl -n istio-system get service istio-ingressgateway -o jsonpath='{.spec.ports[?(@.name=="https")].nodePort}')
echo ---
echo Ingress URL: http://$MINIKUBE_IP:$INGRESS_PORT
echo Ingress URL: https://$MINIKUBE_IP:$SECURE_INGRESS_PORT
