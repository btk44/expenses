<script lang="ts">
	import { GetEmptyTransaction, type Transaction }  from '../models/transaction'
	import { createEventDispatcher } from 'svelte';
    import { accountStoreReadOnly, categoryStoreReadOnly, reloadAccount } from '../services/store';
	import { TransactionService } from '../services/transaction-service';

    const inputId = 'transaction-input'

    export const initTransaction = (initTransaction: Transaction) => {  
        transaction = initTransaction
        if(transaction.id > 0){
            currentInput = `${initTransaction.accountId} ${initTransaction.categoryId} ${initTransaction.amount} ${initTransaction.comment ?? ''}`
        }
        else 
            currentInput =  ''
    }

    //const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

    let transaction: Transaction = GetEmptyTransaction()
    let currentInput: string = ''

    const dispatch = createEventDispatcher();
    const isNumber = (input: string): boolean => { return !isNaN(+input) }
    const resetErrorFlags = () => { accountError = categoryError = amountError = false }
    const accounts = $accountStoreReadOnly 
    const categories = $categoryStoreReadOnly

    const defaultAccountText = 'konto*'
    const defaultCategoryText = 'kategoria*'
    const defaultAmountText = 'kwota*'
    const defaultCommentText = 'komentarz'

    $: amountText = transaction.amount === 0 ? defaultAmountText : transaction.amount
    $: accountText = transaction.accountId <= 0 ? defaultAccountText : accounts[transaction.accountId].name
    $: categoryText = transaction.categoryId <= 0 ? defaultCategoryText : categories[transaction.categoryId].name

    let accountError = false
    let categoryError = false
    let amountError = false
    $: hasError = categoryError || accountError || amountError
    $: isDataMissing = !(transaction.accountId > 0 && transaction.categoryId > 0)

    let saving = false

    const processAccountInput = (inputValue: string | undefined | null): void => {
        if(!inputValue){
            transaction.accountId = 0
            return
        }
        
        let accountId: number | null = null
        if(isNumber(inputValue)) accountId = +inputValue
        if(accountId != null && accounts[accountId]) { 
            transaction.accountId = accountId
            accountError = false
            accountText = accounts[accountId].name
        }
        else {
            transaction.accountId = 0
            accountError = true
        }
    }

    const processCategoryInput = (inputValue: string | undefined | null): void => {
        if(!inputValue){
            transaction.categoryId = 0
            return
        }

        let categoryId: number | null = null
        if(isNumber(inputValue)) categoryId = +inputValue
        if(categoryId != null && categories[categoryId]) { 
            transaction.categoryId = categoryId
            categoryError = false
            categoryText = categories[categoryId].name
        }
        else{
            transaction.categoryId = 0
            categoryError = true
        } 
    }
    
    const processAmountInput = (inputValue: string | undefined | null): void => {
        if(!inputValue){
            transaction.amount = 0
            return
        }

        transaction.amount = 0
        if(isNumber(inputValue)){
            amountError = false
            transaction.amount = +inputValue
            amountText = inputValue
        } 
        else {
            amountError = true
        } 
    }

    const processInput = (event: any) => {
        resetErrorFlags()
        transaction.accountId = 0
        transaction.categoryId = 0
        transaction.amount = 0

        let currentInput = event.target.value

        if (event.key === "Escape") inputCancel()

        if(currentInput && currentInput.trim() !== ''){
            const inputValues = currentInput.match(/\s{0,}[^\s]{1,}/g)

            if(inputValues){
                processAccountInput(inputValues[0])
                processCategoryInput(inputValues[1])
                processAmountInput(inputValues[2])

                transaction.comment = currentInput.replace(inputValues[0], '')
                                                  .replace(inputValues[1], '')
                                                  .replace(inputValues[2], '').trim()

                if(event.key === 'Enter') inputSubmit()
            }
        }
    }

    const inputSubmit = async () => {
        if(!hasError && !isDataMissing){
            try{
                saving = true
                //await sleep(1000)
                transaction.id =  (await TransactionService.SaveTransactions([transaction]))[0].id
                await reloadAccount(1, transaction.accountId)
                dispatch('transactionSubmit', { transaction })
                transaction = GetEmptyTransaction()
                currentInput = ''
            }
            catch { 
                alert('server error')
            } finally {
                saving = false
            }
        }
    }

    const inputCancel = () => {
        currentInput = ''
        resetErrorFlags()
        dispatch('transactionCancel')
        transaction = GetEmptyTransaction()
    }
</script>
<div class="transaction-input">
    {#if saving}
    <div class="mask"><div class="loader"></div></div>
    {/if}
    <div class="input-group">
        <input type="text" name={inputId} id={inputId} placeholder="0 0 -0.00 xxxx" 
                on:keyup={processInput} 
                bind:value={currentInput}
                autocomplete="off"/>
        <button class="button-outlined" on:click={inputSubmit} disabled={hasError || isDataMissing}>&#x2713;</button>
        <button class="button-outlined" on:click={inputCancel}>&#x2715;</button>
    </div>
    <label for={inputId}>
        <span class={accountError ? 'error-text': ''}>{accountText}</span>\
        <span class={categoryError ? 'error-text': ''}>{categoryText}</span>\
        <span class={amountError ? 'error-text': ''}>{amountText}</span>\
        <span>{transaction.comment.length > 0 ? transaction.comment : defaultCommentText}</span>
    </label>
</div>


<style lang="scss">
    @import '../styles/app.scss';

    .transaction-input {  
        position: relative;
        display: flex; gap: 5px; flex-direction: column; justify-content: center;
        .input-group { display: flex; flex-direction: row; gap: 10px; }
    }

    input { width: 400px; }
    label { width: 400px; margin: 0 5px; display: flex; flex-direction: row; gap: 10px; 
            span { text-overflow: ellipsis; overflow: hidden; white-space: nowrap; display: inline-block; }  
          }
</style>