istioctl manifest apply --set profile=demo
istioctl manifest apply --set values.kiali.enabled=true
kubectl label namespace default istio-injection=enabled
