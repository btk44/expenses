<script lang="ts">
	import { onMount } from "svelte";
	import { CopyTransaction, GetEmptyTransaction, type Transaction } from "../../models/transaction"
	import TransactionTable from "./transaction-table.svelte"
	import { TransactionService } from "../../services/transaction-service"
	import type { TransactionSearchFilters } from "../../models/transaction-search-filters"
	import TransactionInput from "./transaction-input.svelte"
	import { reloadAccount } from "../../services/store"
	import TransactionInputUllage from "./transaction-input-ullage.svelte"

    // setup values
    const ullageText = 'manko'
    const pageSize = 16
    const page = 0
    const defaultFilters: TransactionSearchFilters = {ownerId: 1, active: true}
    const getDefaultTransactionSelection = () => { return { index: -1, transaction: GetEmptyTransaction() } }

    // states
    let data = $state({
        page: page,
        filters: defaultFilters,
        loaded: false,
        pageCount: 0,
        displayTransactions: new Array<Transaction>()
    })

    let saving = $state(false)
    let selectedTransaction = $state(getDefaultTransactionSelection())
    let standardInputMode = $state(true)

    // events
    onMount(async () => { 
        loadTransactionPage(0)
    })

    const loadTransactionPage = async (pageChange: number) => {
        let nextPage = data.page + pageChange
        if(nextPage < 0) return
        
        data.loaded = false   
        try{
            const transactionCountResult = await TransactionService.SearchTransactionsCount(data.filters)
            data.pageCount = Math.max(Math.ceil(transactionCountResult / pageSize), 1)

            if(pageChange === 0){
                nextPage = data.pageCount -1 // initial load -> go to last page
            }

            const transactionSearchResult = await TransactionService.SearchTransactions({...data.filters, take: pageSize, offset: nextPage*pageSize})

            if(transactionSearchResult.length > 0){
                data.page = nextPage
                data.displayTransactions = transactionSearchResult
            }
            else {
                data.page = 0
                data.displayTransactions = []
            }
        }
        catch{ alert('server error') }
        finally{ data.loaded = true }
    }

    const onFilterData = (filters: TransactionSearchFilters) => { data.filters = filters; loadTransactionPage(0) }
    const onRowSelected = (index: number, transaction: Transaction) => { 
        if (index === -1)
            selectedTransaction = getDefaultTransactionSelection()
        else
            selectedTransaction = { index: index, transaction: transaction }
    }
    const onPageChange = (pageChange: number) => {
        onTransactionEditCancel()
        loadTransactionPage(pageChange)
    }

    const onTransactionSubmit = async (submittedTransaction: Transaction) => {
        try{
            saving = true
            const transaction = CopyTransaction(submittedTransaction)
            transaction.id =  (await TransactionService.SaveTransactions([transaction]))[0].id
            await reloadAccount(1, transaction.accountId)
            
            const transactionIndex = data.displayTransactions.map(x => x.id).indexOf(transaction.id)
            if(transactionIndex > -1)
                data.displayTransactions[transactionIndex] = transaction
            else {
                if(!(data.page + 1 === data.pageCount) || data.displayTransactions.length === pageSize) 
                    loadTransactionPage(0)
                else
                    data.displayTransactions[data.displayTransactions.length] = transaction
            }

            selectedTransaction = getDefaultTransactionSelection()
        }
        catch { alert('server error') }
        finally{ saving = false }
    }
    const onTransactionEditCancel = () => { selectedTransaction = getDefaultTransactionSelection() }
</script>

<div class="transactions">
    {#if !data.loaded || saving}
    <div class="mask"><div class="loader"></div></div>
    {/if}
    <div class="table">
        <TransactionTable displayTransactions = {data.displayTransactions}
                        page = {data.page}
                        pageCount = {data.pageCount}
                        pageSize = {pageSize}
                        selectedRow = {selectedTransaction.index}
                        {onFilterData}
                        {onRowSelected}
                        {onPageChange}></TransactionTable>
                    </div>
    <div class="inputs">
        {#if standardInputMode}
        <div>
            <TransactionInput inputTransaction = {selectedTransaction.transaction}
                            {onTransactionSubmit}
                            {onTransactionEditCancel}></TransactionInput>
        </div>
        {:else}
        <div>
            <TransactionInputUllage {onTransactionSubmit}></TransactionInputUllage> 
        </div>
        {/if}
        <button class={standardInputMode ? 'button-outlined' : 'button-outlined-toggled'}
                onclick={() => {standardInputMode = !standardInputMode}}>{ullageText}</button>
    </div>

</div>

<style lang="scss">
    @import '../../styles/app.scss';

    .transactions {
        display: flex;
        flex-direction: column;
        justify-content: space-between;
        height: 100%;

        .table{
            height: calc(100% - $control-min-height*2);
        }

        .inputs {
            display: flex;
            flex-direction: row;
            justify-content: flex-start;
            height: $control-min-height*2;

            button { height: $control-min-height; margin: 5px 5px 5px 0px; 
                writing-mode: sideways-rl; -webkit-writing-mode: vertical-rl; 
                font-size: xx-small; text-transform: lowercase; padding: 0;}
        }
    }
</style>