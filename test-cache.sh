#!/bin/bash

# Configuration
API_URL="http://localhost:5116"
TOKEN=$(cat test-token.txt)
AUTH_HEADER="Authorization: Bearer $TOKEN"

echo "=== E-Commerce Cache Verification Script ==="

echo -e "\n1. Fetching first product from list (Cache Miss expected)..."
PRODUCTS_JSON=$(curl -s -X GET "$API_URL/api/products" -H "$AUTH_HEADER")
FIRST_PRODUCT_ID=$(echo $PRODUCTS_JSON | jq -r '.items[0].id')
FIRST_PRODUCT_PRICE=$(echo $PRODUCTS_JSON | jq -r '.items[0].price')
echo "Fetched Product ID: $FIRST_PRODUCT_ID, Price: $FIRST_PRODUCT_PRICE"

echo -e "\n2. Fetching same product individually (Should partially hit DB/Cache based on TTLs)..."
PRODUCT_DETAILS=$(curl -s -X GET "$API_URL/api/products/$FIRST_PRODUCT_ID" -H "$AUTH_HEADER")
echo $PRODUCT_DETAILS | jq .

echo -e "\n3. Changing price in DB (Via Update API)..."
NEW_PRICE="1299.99"
UPDATE_PAYLOAD=$(cat <<EOF
{
  "price": $NEW_PRICE
}
EOF
)
curl -s -X PATCH "$API_URL/api/products/$FIRST_PRODUCT_ID/price" \
     -H "Content-Type: application/json" \
     -H "$AUTH_HEADER" \
     -d "$UPDATE_PAYLOAD" -w "\nHTTP Status: %{http_code}\n"

echo -e "\n4. Fetching product via GetById (Should reflect NEW price immediately due to valid invalidation)..."
curl -s -X GET "$API_URL/api/products/$FIRST_PRODUCT_ID" -H "$AUTH_HEADER" | jq .

echo -e "\n5. Fetching product via List (Should reflect NEW price immediately)..."
curl -s -X GET "$API_URL/api/products" -H "$AUTH_HEADER" | jq -r ".items[] | select(.id==\"$FIRST_PRODUCT_ID\") | {name, price}"

echo -e "\n6. Checking Redis keys directly to ensure separated cache exists..."
sudo docker exec -i ecommerce-redis redis-cli --scan --pattern "ECommerce:products*"

echo -e "\n=== Test Completed ==="
