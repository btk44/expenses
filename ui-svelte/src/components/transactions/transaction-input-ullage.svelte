<script lang="ts">
	import { GetEmptyTransaction }  from '../../models/transaction'
    import { accountStoreReadOnly, categoryStoreReadOnly } from '../../services/store'
	import { processAccountInput, processAmountInput, processCategoryInput } from './transaction-input-helper'

    //setup values
    const accounts = $accountStoreReadOnly 
    const categories = $categoryStoreReadOnly
    const inputValuesRegex = /\s{0,}[^\s]{1,}/g
    const inputId = 'transaction-input'

    const defaultAccountText = 'konto*'
    const defaultCategoryText = 'kategoria*'
    const defaultCashText = 'gotówka'
    const defaultRealAmountText = 'na koncie'

    //props
    let { onTransactionSubmit } = $props()

    //states
    let transaction = $state(GetEmptyTransaction())
    let ullageInput = $state('')
    let realAmount = $state(0)
    let cash = $state(0)

    let errors = $state({
        accountError: false,
        categoryError: false,
        realAmountError: false,
        cashError: false
    })

    const resetErrorFlags = () => { 
        errors.realAmountError = errors.cashError = errors.categoryError = errors.accountError = false }

    const hasError = $derived(errors.accountError || errors.categoryError || errors.realAmountError || errors.cashError)
    const isDataMissing = $derived(!(transaction.accountId > 0 && transaction.categoryId > 0))
    const accountText = $derived(transaction.accountId <= 0 ? defaultAccountText : accounts[transaction.accountId].name)
    const categoryText = $derived(transaction.categoryId <= 0 ? defaultCategoryText : categories[transaction.categoryId].name)
    const realAmountText = $derived(realAmount === 0 ? defaultRealAmountText : realAmount.toString())
    const cashText = $derived(cash === 0 ? defaultCashText : cash.toString())
    const ullageText = $derived(hasError || isDataMissing ? '??' : (-1 * transaction.amount).toString())

    const processUllageInput = (event: any) => {       
        resetErrorFlags()
        realAmount = 0
        cash = 0

        let ullageInput = event.target.value

        if (event.key === "Escape") ullageCancel()

        if(ullageInput && ullageInput.trim() !== ''){
            const inputValues = ullageInput.match(inputValuesRegex)

            if(inputValues){
                const accountOutput = processAccountInput(inputValues[0], accounts)
                const categoryOutput = processCategoryInput(inputValues[1], categories)
                const realAmountOutput = processAmountInput(inputValues[2])
                const cashOutput = processAmountInput(inputValues[3])

                transaction.accountId = accountOutput.value
                errors.accountError = accountOutput.isError
                transaction.categoryId = categoryOutput.value
                errors.categoryError = categoryOutput.isError
                
                if (!errors.accountError)
                    transaction.amount = -1 * accounts[transaction.accountId].amount

                errors.realAmountError = realAmountOutput.isError
                realAmount = errors.realAmountError ? 0 : realAmountOutput.value

                if (!errors.realAmountError)
                    transaction.amount += realAmount

                errors.cashError = cashOutput.isError
                cash = errors.cashError ? 0 : cashOutput.value

                if (!errors.cashError)
                    transaction.amount += cash

                transaction.amount =  Math.round(transaction.amount *100) / 100
                transaction.comment = 'manko - wyrównanie stanu konta'

                if(event.key === 'Enter') ullageSubmit()
            }
        }
    }

    const ullageSubmit = () => {
        if(!hasError && !isDataMissing){
            onTransactionSubmit(transaction)
            transaction = GetEmptyTransaction()
            ullageInput = ''
            realAmount = cash = 0
        }
    }

    const ullageCancel = () => {
        transaction = GetEmptyTransaction()
        ullageInput = ''
        realAmount = cash = 0
        resetErrorFlags()
        // to do : this does not update labels
    }

</script>
<div class="transaction-input">
    <div class="input-group">
        <input type="text" name={inputId+'-ullage'} id={inputId+'-ullage'} placeholder="0 0 0.00 0.00" 
                onkeyup={processUllageInput} 
                bind:value={ullageInput}
                autocomplete="off"/>
        <button class="button-outlined" onclick={ullageSubmit} disabled={hasError || isDataMissing}>&#x2713;</button>
        <button class="button-outlined" onclick={ullageCancel}>&#x2715;</button>
    </div>
    <label for={inputId+'-ullage'}>
        <span class={errors.accountError ? 'error-text': ''}>{accountText}</span>\
        <span class={errors.categoryError ? 'error-text': ''}>{categoryText}</span>\
        <span class={errors.realAmountError ? 'error-text': ''}>{realAmountText}</span>\
        <span class={errors.cashError ? 'error-text': ''}>{cashText}</span>=
        <span>{ullageText}</span>
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

    input { width: 400px; background-color: $primary-color-dark; color: $text-color;}
    label { width: 400px; margin: 0 5px; display: flex; flex-direction: row; gap: 10px; 
            span { text-overflow: ellipsis; overflow: hidden; white-space: nowrap; display: inline-block; }  
          }
</style>