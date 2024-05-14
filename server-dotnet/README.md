# TransactionService - server (.net 8)

## How to build and run docker container:


* Change name of the solution to avoid conflicts while building docker container (error that there are more than 1 project)
    ```
    mv transaction-service.sln transaction-service.xxx
    ```
* Build docker container
    ```
    sudo docker build -t expenses-backend -f Dockerfile .
    ```
* Rename solution to what is was
    ```
    mv transaction-service.xxx transaction-service.sln
    ```

* save image outside docker to copy it to other machine
    ```
    sudo docker save -o transaction-service.tar expenses-backend
    ```

* change permissions
    ```
    sudo chmod a+rwx transaction-service.tar
    ```
* now copy it to other machine 

* load image to other docker
    ```
    sudo docker load -i docker/samba/tmp/transaction-service.tar
    ```

### now run docker compose in the other docker
services:
  db:
    image: expenses-backend
    container_name: expenses-backend
    network_mode: bridge
    restart: always
    ports:
      - 5001:8080

