# TransactionService - database (postgres)

## How to build and run docker container:

* Run docker compose in the other docker
    ```
    sudo docker compose up -d
    ```

## How to backup and restore database

* Make database backup
    ```
    sudo docker exec -t expenses-db pg_dump -U postgres TransactionService > path/to/file/transaction-service-db_`date +%Y-%m-%d"_"%H_%M_%S`.sql
    ```

* Restore database from backup 
    ```
    sudo cat path/to/file/transaction-service-db_[FILL DATE] | sudo docker exec -i expenses-db psql -U postgres TransactionService
    ```

