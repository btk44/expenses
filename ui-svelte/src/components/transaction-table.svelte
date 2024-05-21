<script lang="ts">
	import { GetEmptyTransaction, type Transaction }  from '../models/transaction'
	import { accountStoreReadOnly, categoryStoreReadOnly, accountCurrencyMap } from '../services/store';
	import { onMount } from 'svelte';
	import { TransactionService } from '../services/transaction-service';
	import { type TransactionSearchFilters } from '../models/transaction-search-filters';
	import TransactionInput from './transaction-input.svelte';

    const headers = ['data', 'konto', 'kategoria', 'kwota', '-', 'komentarz'] 
    const accounts = $accountStoreReadOnly 
    const categories = $categoryStoreReadOnly
    const accountCurrency = accountCurrencyMap()
    const pageSize = 20

    let displayTransactions: Array<Transaction> = []
    let dataLoaded = false
    let page = 0
    let pageCount = 0
    $: isLastPage = page + 1 === pageCount
    $: emptyRowsCount = pageSize - displayTransactions.length > 0 ? pageSize - displayTransactions.length : 0
    // filters: 
    let dateFilterText = ''
    let accountFilterText = ''
    let categoryFilterText = ''
    let amountFilterText = ''
    let commentFilterText = ''

    let filtersVisible = false
    let selectedRow = -1

    let initTransactionInput: any

    //const sleep = (ms: number) => new Promise((r) => setTimeout(r, ms));

    onMount(async () => { 
        loadTransactionPage(0)
    })

    const getFilters = (): TransactionSearchFilters => {
        const getDate = (dateString: string) => {
            return new Date((new Date()).setTime(new Date(dateString).getTime()+12*60*60*1000))
        } 

        const searchFilters: TransactionSearchFilters = {ownerId: 1}

        if(dateFilterText.length){
            const dateStrings = dateFilterText.split(',')
            //this is all wrong - welcome fucking timezones and time formats!
            if (dateStrings.length > 0 && !isNaN(Date.parse(dateStrings[0]))) { searchFilters.dateFrom = getDate(dateStrings[0])}
            if (dateStrings.length > 1 && !isNaN(Date.parse(dateStrings[1]))) { searchFilters.dateTo = getDate(dateStrings[1]) }
        }

        if (accountFilterText.length){
            const accountIds = accountFilterText.split(',').filter(x => x.length && !isNaN(+x)).map(x => parseInt(x) )
            if (accountIds.length) { searchFilters.accounts = accountIds }
        }

        if (categoryFilterText.length){
            const categoryIds = categoryFilterText.split(',').filter(x => x.length && !isNaN(+x)).map(x => parseInt(x) )
            if (categoryIds.length) { searchFilters.categories = categoryIds }
        }

        if (amountFilterText.length){
            const amountStrings = amountFilterText.split(',')
            if (amountStrings.length > 0 && !isNaN(+amountStrings[0])) { searchFilters.amountFrom = +amountStrings[0] }
            if (amountStrings.length > 1 && !isNaN(+amountStrings[1])) { searchFilters.amountTo = +amountStrings[1] }
        }

        if (commentFilterText.length){ searchFilters.comment = commentFilterText }
        
        return searchFilters
    }

    const loadTransactionPage = async (pageChange: number) => {
        let nextPage = page + pageChange
        if(nextPage < 0) return
        
        dataLoaded = false
        //await sleep(500)       
        try{
            const transactionCountResult = await TransactionService.SearchTransactionsCount(getFilters())
            pageCount = Math.max(Math.ceil(transactionCountResult / pageSize), 1)

            if(pageChange === 0){
                nextPage = pageCount -1 // initial load -> go to last page
            }

            const transactionSearchResult = await TransactionService.SearchTransactions({...getFilters(), take: pageSize, offset: nextPage*pageSize})

            if(transactionSearchResult.length > 0){
                page = nextPage
                displayTransactions = transactionSearchResult
            }
            else {
                page = 0
                displayTransactions = []
            }

            scrollTable()
        }
        catch{ alert('server error') }
        finally{ dataLoaded = true }
    }

    const formatDate = (date: Date): string => { 
        const addLeadingZero = (number: number): string =>  number < 10 ? `0${number}` : number.toString()

        return `${addLeadingZero(date.getDate())}/${addLeadingZero(date.getMonth()+1)}/${date.getFullYear()}`
    } 

    const formatAmount = (amount: number): string => { 
        return amount < 0 ? amount.toString() : `+${amount}` 
    }

    const rowDoubleClick = (transaction: Transaction, index: number) => { 
        if(selectedRow === index) {
            selectedRow = -1
            initTransactionInput(GetEmptyTransaction())
            return
        }
        
        selectedRow = index
        initTransactionInput(transaction)
    }

    const filtersClick = () => { filtersVisible = !filtersVisible }
    const filtersConfirmed = () => { filtersVisible = false; loadTransactionPage(0) }
    const filtersConfirmedWithEnter = (event: any) => { if(event.key === 'Enter') filtersConfirmed() }
    const transactionInputSubmit = (event: any) => {
        selectedRow = -1
        const transactionIndex = displayTransactions.indexOf(event.detail.transaction)
        if(transactionIndex > -1){
            displayTransactions[transactionIndex] = event.detail.transaction
        }
        else  {
            if(!isLastPage || displayTransactions.length === pageSize) 
                loadTransactionPage(0)
            else {
                displayTransactions[displayTransactions.length] = event.detail.transaction
                scrollTable()
            }
        }
    }

    const scrollTable = () => {
        const objDiv = document.getElementById('transaction-table-data')
        
        if(objDiv){
            const singleRowHeight = objDiv.scrollHeight/pageSize
            setTimeout(() => { objDiv.scrollTop = singleRowHeight*(displayTransactions.length-1) }, 40) // this is a hack :(
        }
    }

    const transactionInputCancel = () => {
        selectedRow = -1
        //do nothing?
    }
</script>

<div class="transactions">
    {#if !dataLoaded}
    <div class="mask"><div class="loader"></div></div>
    {/if}
    <div id="transaction-table-data" class="data">
        <table>
            <tr>
                {#each headers as header}
                <th class="aln-l">{header}</th>
                {/each}
            </tr>
            {#each displayTransactions as transaction, index}
            <tr class={selectedRow === index ? 'selected' : ''} on:dblclick={() => { rowDoubleClick(transaction, index) }}>
                <td class="aln-l w-20pc">{formatDate(transaction.date)}</td>
                <td class="aln-l w-20pc">{accounts[transaction.accountId]?.name ?? ''}</td>
                <td class="aln-l w-20pc">{categories[transaction.categoryId]?.name ?? ''}</td>
                <td class="aln-r w-10pc"><span class={transaction.amount > 0 ? 'green-text' : 'red-text'}>{formatAmount(transaction.amount)}</span></td>
                <td class="aln-l w-10pc">{accountCurrency[transaction.accountId] ?? ''}</td>
                <td class="aln-l w-20pc">{transaction.comment}</td>
            </tr>
            {/each}
            {#each {length: emptyRowsCount} as _ }
            <tr>
                <td class="aln-l w-20pc"></td>
                <td class="aln-l w-20pc"></td>
                <td class="aln-l w-20pc"></td>
                <td class="aln-l w-10pc"></td>
                <td class="aln-l w-10pc"></td>
                <td class="aln-l w-20pc"></td>
            </tr>
            {/each}
        </table>
    </div>
    <div class="other">
        <table class={filtersVisible ? '' : 'hidden'}>
            <tr>
                <td class="aln-l w-20pc"><input type="text" on:keypress={filtersConfirmedWithEnter} bind:value={dateFilterText} placeholder="data: od, do"/></td>
                <td class="aln-l w-20pc"><input type="text" on:keypress={filtersConfirmedWithEnter} bind:value={accountFilterText} placeholder="konto: id, id"/></td>
                <td class="aln-l w-20pc"><input type="text" on:keypress={filtersConfirmedWithEnter} bind:value={categoryFilterText} placeholder="kategoria: id, id"/></td>
                <td class="aln-l w-20pc"><input type="text" on:keypress={filtersConfirmedWithEnter} bind:value={amountFilterText} placeholder="kwota: od, do"/></td>
                <td class="aln-l w-20pc"><input type="text" on:keypress={filtersConfirmedWithEnter} bind:value={commentFilterText} placeholder="komentarz"/></td>
            </tr>
        </table>
        <div class="actions">
            <TransactionInput bind:initTransaction={initTransactionInput}
                                on:transactionSubmit={transactionInputSubmit}
                                on:transactionCancel={transactionInputCancel}></TransactionInput>
            <div class="buttons">
                {#if filtersVisible}
                <button class="button-outlined" on:click={() => { filtersConfirmed() }}>&#x2699;</button>
                {/if}
                <button class="button-outlined" on:click={() => { filtersClick() }}>&#x2637;</button>
                <button class="button-outlined" disabled={page === 0} on:click={() => { loadTransactionPage(-1) }}>&#x276E;</button>
                <div>{`${page + 1} / ${pageCount}`}</div>
                <button class="button-outlined" disabled={isLastPage} on:click={() => { loadTransactionPage(1) }}>&#x276F;</button>
            </div>
        </div>
    </div>
</div>

<style lang="scss">
    @import '../styles/app.scss';
    .transactions {
        display: flex;
        flex-direction: column;
        justify-content: space-between;
        height: 100%;

        .red-text { color: $red-text-color; }
        .green-text { color: $green-text-color; }

        .data{ overflow-y: auto; height: 100%; } 
        .actions { margin: 5px; display: flex; align-items: flex-start; justify-content: space-between;
            .buttons { display: flex; align-items: flex-start; gap: 10px; justify-content: end; div { line-height: $control-min-height; } }
        }
        .selected { background-color: $accent-color-light;}
        input { width: 100%;}
        .hidden { visibility: hidden;}
    }
</style>