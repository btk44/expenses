<script lang="ts">
	import { CopyTransaction, GetEmptyTransaction, type Transaction }  from '../../models/transaction'
    import { accountStoreReadOnly, categoryStoreReadOnly } from '../../services/store'
	import { processAccountInput, processAmountInput, processCategoryInput } from './transaction-input-helper'

    //setup values
    const accounts = $accountStoreReadOnly 
    const categories = $categoryStoreReadOnly
    const inputValuesRegex = /\s{0,}[^\s]{1,}/g
    const inputId = 'transaction-input'

    const defaultAccountText = 'konto*'
    const defaultCategoryText = 'kategoria*'
    const defaultAmountText = 'kwota*'
    const defaultCommentText = 'komentarz'

    //props
    let { inputTransaction, onTransactionSubmit, onTransactionEditCancel } = $props()

    //states
    let currentInput: string = $state('')
    let transaction: Transaction = $state(GetEmptyTransaction())
    let lastAccountId: number = $state(0)

    const amountText = $derived(transaction.amount === 0 ? defaultAmountText : transaction.amount)
    const accountText = $derived(transaction.accountId <= 0 ? defaultAccountText : accounts[transaction.accountId].name)
    const categoryText = $derived(transaction.categoryId <= 0 ? defaultCategoryText : categories[transaction.categoryId].name)

    let errors = $state({
        accountError: false,
        categoryError: false,
        amountError: false
    })

    const hasError = $derived(errors.categoryError || errors.accountError || errors.amountError)
    const isDataMissing = $derived(!(transaction.accountId > 0 && transaction.categoryId > 0))

    $effect(() => {
        transaction = {...CopyTransaction(inputTransaction), accountId: lastAccountId }

        if (inputTransaction.id > 0)
            currentInput = `${inputTransaction.accountId} ${inputTransaction.categoryId} ${inputTransaction.amount} ${inputTransaction.comment ?? ''}`
        else if (lastAccountId !== 0) {
            currentInput = `${lastAccountId} `
        } 
        else
            currentInput = ''
    })

    //error methods
    const resetErrorFlags = () => { errors.accountError = errors.categoryError = errors.amountError = false }

    const processInput = (event: any) => {
        resetErrorFlags()
        transaction.accountId = 0
        transaction.categoryId = 0
        transaction.amount = 0

        let currentInput = event.target.value

        if (event.key === "Escape") inputCancel()

        if(currentInput && currentInput.trim() !== ''){
            const inputValues = currentInput.match(inputValuesRegex)

            if(inputValues){
                const accountOutput = processAccountInput(inputValues[0], accounts)
                const categoryOutput = processCategoryInput(inputValues[1], categories)
                const amountOutput = processAmountInput(inputValues[2])

                transaction.accountId = accountOutput.value
                errors.accountError = accountOutput.isError
                transaction.categoryId = categoryOutput.value
                errors.categoryError = categoryOutput.isError
                transaction.amount = amountOutput.value
                errors.amountError = amountOutput.isError

                transaction.comment = currentInput.replace(inputValues[0], '')
                                                  .replace(inputValues[1], '')
                                                  .replace(inputValues[2], '').trim()

                if(event.key === 'Enter') inputSubmit()
            }
        }
    }

    //events
    const inputSubmit = async () => {
        if(!hasError && !isDataMissing){
            lastAccountId = transaction.accountId
            onTransactionSubmit(transaction)
        }
    }

    const inputCancel = () => {
        currentInput = ''
        lastAccountId = 0
        resetErrorFlags()
        onTransactionEditCancel()
    }

</script>
<div class="transaction-input">
    <div class="input-group">
        <input type="text" name={inputId} id={inputId} placeholder="0 0 -0.00 xxxx" 
                onkeyup={processInput} 
                bind:value={currentInput}
                autocomplete="off"/>
        <button class="button-outlined" onclick={inputSubmit} disabled={hasError || isDataMissing}>&#x2713;</button>
        <button class="button-outlined" onclick={inputCancel}>&#x2715;</button>
    </div>
    <label for={inputId}>
        <span class={errors.accountError ? 'error-text': ''}>{accountText}</span>\
        <span class={errors.categoryError ? 'error-text': ''}>{categoryText}</span>\
        <span class={errors.amountError ? 'error-text': ''}>{amountText}</span>\
        <span>{transaction.comment.length > 0 ? transaction.comment : defaultCommentText}</span>
    </label>
</div>


<style lang="scss">
    @import '../../styles/app.scss';

    .transaction-input {  
        position: relative;
        margin: 5px;
        display: flex; gap: 5px; flex-direction: column; justify-content: center;
        .input-group { 
            display: flex; flex-direction: row; gap: 5px; 
        }
    }

    input { width: 400px; }
    label { width: 400px; margin: 0 5px; display: flex; flex-direction: row; gap: 10px; 
            span { text-overflow: ellipsis; overflow: hidden; white-space: nowrap; display: inline-block; }  
          }
</style>