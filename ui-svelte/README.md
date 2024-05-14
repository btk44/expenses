# TransactionService - user interface (svelte)

## How to build and run docker container:


* Build docker container
    ```
    sudo docker build -t expenses-ui -f Dockerfile .
    ```

* save image outside docker to copy it to other machine
    ```
    sudo docker save -o transaction-service-ui.tar expenses-ui
    ```

* change permissions
    ```
    sudo chmod a+rwx transaction-service-ui.tar
    ```
* now copy it to other machine 

* load image to other docker
    ```
    sudo docker load -i path/to/copied/file/transaction-service-ui.tar
    ```

* now run docker compose in the other docker
    ```
    sudo docker compose up -d
    ```
