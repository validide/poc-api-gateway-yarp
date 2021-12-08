# poc-api-gateway-yarp
YARP based API Gateway Implementation Proof Of Concept

## To start testing

```
./ngrok.exe http https://localhost:12080

curl --location --request GET 'https://4777-92-81-12-236.ngrok.io/apis/aaa/bbb'


https://4777-92-81-12-236.ngrok.io/id5-cookie/sssssss
https://4777-92-81-12-236.ngrok.io/id4-cookie/sssssss


curl --location --request POST 'https://demo.identityserver.io/connect/token' \
--header 'Content-Type: application/x-www-form-urlencoded' \
--data-urlencode 'client_id=m2m' \
--data-urlencode 'client_secret=secret' \
--data-urlencode 'grant_type=client_credentials'

curl --location --request GET 'https://4777-92-81-12-236.ngrok.io/id4-bearer/aaa/bbb' \
--header 'Authorization: Bearer your_token_here'




curl --location --request POST 'https://demo.duendesoftware.com/connect/token' \
--header 'Content-Type: application/x-www-form-urlencoded' \
--data-urlencode 'client_id=m2m' \
--data-urlencode 'client_secret=secret' \
--data-urlencode 'grant_type=client_credentials'


curl --location --request GET 'https://4777-92-81-12-236.ngrok.io/id5-bearer/aaa/bbb' \
--header 'Authorization: Bearer your_token_here'
```



## To demo
- Add authentication and authorization
  * cookie
  * JWT
  * propagate user as header
- Add rate limiting
- Add request delay
