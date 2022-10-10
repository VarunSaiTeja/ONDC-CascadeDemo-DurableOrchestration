# ONDC Cascaded transaction demo with Azure Durable Orchestration Function

![image](https://user-images.githubusercontent.com/35555010/194807814-16fb9969-03e5-4403-959a-146d878c1690.png)


I made a select api as http trigger which invokes select orchestrator function.

The orch function makes logistic search call & waits for completion of any one task below:

1. External event(occurs only once if minimum 3 on_search responses received from  logistic on_search endpoint)

2. Timer of 5 sec

The select orch func continues execution if any of above task is completed and cancels/ignores the other task going forward.

If task 1 completed within 5 sec then the logistic on_search http endpoint will raise an external event with the best quote to select orch func.

If task 2 gets completed first then it will stop considering further logistic on_search responses.

Finally the select orch func checks for best on_search responses received so far. If there's any logistic on_search response available then the select orch func will make retail on_select response with delivery quote & delivery state serviceable else on_select response without delivery quote & delivery state non-serviceable.


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
