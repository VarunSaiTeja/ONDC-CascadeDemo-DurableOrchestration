# CascadeDemo

Make Retail Select API Call

`curl --location --request POST 'http://localhost:7287/api/Select'`


Make Logistic OnSearch API Call

`curl --location --request POST 'http://localhost:7287/api/OnSearch' \
--header 'Content-Type: application/json' \
--data-raw '{
    "provider":"Dunzo",
    "price":20,
    "messageId":"720d4bd3-b998-4d3a-a817-b0e2487219fa",
    "transactionId":"c6931f3a-3674-4e49-9fe7-db70231c8276"
}'`
