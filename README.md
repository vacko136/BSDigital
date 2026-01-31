# BSDigital
MetaExchange - Internal Project Summary

1. Overview
-----------
MetaExchange is a simple meta-exchange system designed to calculate the best execution plan
for buying or selling Bitcoin (BTC) across multiple crypto exchanges, considering
both the order books and exchange balances.

It consists of three main components:
1. MetaExcange.Console - Console application for quick testing and demonstration.
2. MetaExchange.Api - ASP.NET Core Web API exposing the best execution functionality.
3. MetaExchange.Tests - Unit tests verifying the logic and edge cases.

2. Core Logic - BestExecutionService
------------------------------------
The heart of the system is the BestExecutionService. Its main responsibilities are:

- Aggregating liquidity:
  It reads order books from multiple exchanges and compiles all bid or ask levels
  depending on whether the user wants to buy or sell.

- Sorting for optimal price:
  - Buy orders: sorted ascending (lowest ask prices first)
  - Sell orders: sorted descending (highest bid prices first)

- Respecting balances:
  The service ensures that no order is placed beyond the available EUR or BTC
  on each exchange.

- Partial execution:
  If the requested BTC amount cannot be fully executed on one exchange,
  the service spreads the order across multiple exchanges to achieve the best price.

- Exception handling:
  If the requested BTC cannot be fully executed due to insufficient market liquidity,
  an exception is thrown with a clear message:
    - "Insufficient market liquidity to Buy X BTC"
    - "Insufficient BTC balance to Sell X BTC"

3. Console App
--------------
- Reads JSON files for order books and exchange balances from the Data/ folder.
- Calls BestExecutionService with a hard-coded order request.
- Prints the execution plan and total EUR to the console.

4. API
------
- Exposes the BestExecutionService via a POST endpoint: /api/Execution
- Validates requests using Data Annotations ([Required], [Range]).
- Returns JSON containing:
    - List of execution orders (Exchange, Side, Price, AmountBtc)
    - Total EUR spent or received
- Swagger UI is available for testing the API.

5. Unit Tests
-------------
- Tests include single and multiple exchange scenarios, balance limits,
  partial execution, and insufficient liquidity exceptions.
- Ensures that the algorithm consistently returns the **best execution**.

6. Data
-------
- orderbooks.json: Contains order book snapshots for all exchanges.
- balances.json: Contains current EUR and BTC balances per exchange.

7. Usage Notes
--------------
- The system respects exchange balances and never exceeds them.
- Partial fills are allowed; exceptions only occur when total requested BTC
  cannot be matched by available liquidity across all exchanges.
- Console and API read data from JSON files, making it easy to modify for tests.

