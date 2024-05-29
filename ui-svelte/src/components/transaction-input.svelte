<script lang="ts">
	import { GetEmptyTransaction, type Transaction }  from '../models/transaction'
	import { createEventDispatcher } from 'svelte';
    import { accountStoreReadOnly, categoryStoreReadOnly, reloadAccount } from '../services/store';
	import { TransactionService } from '../services/transaction-service';

    interface processOutput {
        value: number,
        isError: boolean
    }

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
    let remove = false

    const processAccountInput = (inputValue: string | undefined | null): processOutput => {
        if(!inputValue)
            return {isError: false, value: 0}
        
        let accountId: number | null = null
        if(isNumber(inputValue)) accountId = +inputValue
        if(accountId != null && accounts[accountId])
            return {isError: false, value: accountId}

        return {isError: true, value: 0}
    }

    const processCategoryInput = (inputValue: string | undefined | null): processOutput  => { 
        if(!inputValue)
            return {isError: false, value: 0}

        let categoryId: number | null = null
        if(isNumber(inputValue)) categoryId = +inputValue
        if(categoryId != null && categories[categoryId])
            return {isError: false, value: categoryId}

            return {isError: true, value: 0}
    }
    
    const processAmountInput = (inputValue: string | undefined | null): processOutput  => { 
        if(!inputValue)
            return {isError: false, value: 0}

        if(isNumber(inputValue))
            return {isError: false, value: +inputValue}
        
            return {isError: true, value: 0}
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
                const accountOutput = processAccountInput(inputValues[0])
                const categoryOutput = processCategoryInput(inputValues[1])
                const amountOutput = processAmountInput(inputValues[2])

                transaction.accountId = accountOutput.value
                accountError = accountOutput.isError
                transaction.categoryId = categoryOutput.value
                categoryError = categoryOutput.isError
                transaction.amount = amountOutput.value
                amountError = amountOutput.isError

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
                const lastAccountId = transaction.accountId
                //await sleep(1000)
                transaction.id =  (await TransactionService.SaveTransactions([transaction]))[0].id
                await reloadAccount(1, transaction.accountId)
                dispatch('transactionSubmit', { transaction })
                transaction = GetEmptyTransaction()
                transaction.accountId = lastAccountId
                currentInput = `${transaction.accountId} `
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

    // ullage mode
    let standardMode = true

    let ullageInput = ''
    const resetUllageErrorFlags = () => { ullageAccountError = ullageCategoryError = realAmountError = cashError = false }
    let ullageAccountError = false
    let ullageCategoryError = false
    let realAmountError = false
    let cashError = false
    let realAmount = 0
    let cash = 0
    let ullageAccountId = 0
    let ullageCategoryId = 0

    const defaultCashText = 'gotówka'
    const defaultRealAmountText = 'na koncie'

    $: ullageAccountText = ullageAccountId <= 0 ? defaultAccountText : accounts[ullageAccountId].name
    $: ullageCategoryText = ullageCategoryId <= 0 ? defaultCategoryText : categories[ullageCategoryId].name
    $: realAmountText = realAmountError ? defaultRealAmountText : realAmount.toString()
    $: cashText = cashError ? defaultCashText : cash.toString()
    $: ullageText = ullageHasError || ullageDataMissing ? '0' : Math.round((+accounts[ullageAccountId].amount - (+realAmount + +cash)) *100) / 100

    $: ullageHasError = ullageAccountError || ullageCategoryError || realAmountError || cashError
    $: ullageDataMissing = !(ullageAccountId > 0 && ullageCategoryId > 0)

    const processUllageInput = (event: any) => {
        resetUllageErrorFlags()
        ullageAccountId = 0
        ullageCategoryId = 0
        realAmount = 0
        cash = 0

        let ullageInput = event.target.value

        if (event.key === "Escape") ullageCancel()

        if(ullageInput && ullageInput.trim() !== ''){
            const inputValues = ullageInput.match(/\s{0,}[^\s]{1,}/g)

            if(inputValues){
                const accountOutput = processAccountInput(inputValues[0])
                const categoryOutput = processCategoryInput(inputValues[1])
                const amountOutput = processAmountInput(inputValues[2])
                const cashOutput = processAmountInput(inputValues[3])

                ullageAccountError = accountOutput.isError
                ullageAccountId = ullageAccountError ? 0 : accountOutput.value
                ullageCategoryError = categoryOutput.isError
                ullageCategoryId = ullageCategoryError ? 0 : categoryOutput.value        
                realAmountError = amountOutput.isError
                realAmount = realAmountError ? 0 : amountOutput.value
                cashError = cashOutput.isError
                cash = cashError ? 0 : cashOutput.value

                //Math.round((+account.amount - (+realAmount + +cash)) *100) / 100

                if(event.key === 'Enter') ullageSubmit()
            }
        }
    }

    const ullageSubmit = async () => {

    }

    const ullageCancel = () => {
        ullageInput = ''
        resetUllageErrorFlags()
        ullageAccountId = ullageCategoryId = realAmount = cash = 0 // this does not clear texts when Esc pressed
    }

</script>
<div class="transaction-input">
    {#if saving}
    <div class="mask"><div class="loader"></div></div>
    {/if}
    {#if standardMode}
    <div class="input-group">
        <input type="text" name={inputId} id={inputId} placeholder="0 0 -0.00 xxxx" 
                on:keyup={processInput} 
                bind:value={currentInput}
                autocomplete="off"/>
        <button class="button-outlined" on:click={inputSubmit} disabled={hasError || isDataMissing}>&#x2713;</button>
        <button class="button-outlined" on:click={inputCancel}>&#x2715;</button>
        <div class="small-buttons">
            <button class={"button-outlined" + (standardMode ? '' : '-toggled' )} on:click={() => { standardMode = !standardMode }}>M</button>
            <button class={"button-outlined" + (transaction.active ? '' : '-toggled' )} on:click={() => { transaction.active = !transaction.active }}>R</button>
        </div>
    </div>
    <label for={inputId}>
        <span class={accountError ? 'error-text': ''}>{accountText}</span>\
        <span class={categoryError ? 'error-text': ''}>{categoryText}</span>\
        <span class={amountError ? 'error-text': ''}>{amountText}</span>\
        <span>{transaction.comment.length > 0 ? transaction.comment : defaultCommentText}</span>
    </label>
    {:else}
    <div class="input-group">
        <input type="text" name={inputId+'-ullage'} id={inputId+'-ullage'} placeholder="0 0 0.00 0.00" 
                on:keyup={processUllageInput} 
                bind:value={ullageInput}
                autocomplete="off"/>
        <button class="button-outlined" on:click={ullageSubmit} disabled={ullageHasError || ullageDataMissing}>&#x2713;</button>
        <button class="button-outlined" on:click={ullageCancel}>&#x2715;</button>
        <div class="small-buttons">
            <button class={"button-outlined" + (standardMode ? '' : '-toggled' )} on:click={() => { standardMode = !standardMode }}>M</button>
        </div>
    </div>
    <label for={inputId+'-ullage'}>
        <span class={ullageAccountError ? 'error-text': ''}>{ullageAccountText}</span>\
        <span class={ullageCategoryError ? 'error-text': ''}>{ullageCategoryText}</span>\
        <span class={realAmountError ? 'error-text': ''}>{realAmountText}</span>\
        <span class={cashError ? 'error-text': ''}>{cashText}</span>&#8594;
        <span class={cashError ? 'error-text': ''}>{ullageText}</span>
    </label>
    {/if}
</div>


<style lang="scss">
    @import '../styles/app.scss';

    .transaction-input {  
        position: relative;
        display: flex; gap: 5px; flex-direction: column; justify-content: center;
        .input-group { 
            display: flex; flex-direction: row; gap: 5px; 
            .small-buttons { 
                display: flex; flex-direction: column; gap: 1px;
                button { min-height: 0; height: calc($control-min-height/2 - 1px); padding: 2px; font-size: xx-small; } 
            }
        }
    }

    input { width: 400px; }
    label { width: 400px; margin: 0 5px; display: flex; flex-direction: row; gap: 10px; 
            span { text-overflow: ellipsis; overflow: hidden; white-space: nowrap; display: inline-block; }  
          }
</style>