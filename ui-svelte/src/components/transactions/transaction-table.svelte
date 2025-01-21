<script lang="ts">
	import { type Transaction }  from '../../models/transaction'
	import { accountStoreReadOnly, categoryStoreReadOnly, accountCurrencyMap } from '../../services/store'
	import { type TransactionSearchFilters } from '../../models/transaction-search-filters'

    // setup values
    const headers = ['data', 'konto', 'kategoria', 'kwota', '-', 'komentarz'] 
    const accounts = $accountStoreReadOnly 
    const categories = $categoryStoreReadOnly
    const accountCurrency = accountCurrencyMap()

    // props
    let { 
        page = 0, pageCount = 0, pageSize = 0, displayTransactions = new Array<Transaction>(), selectedRow = -1,
        onFilterData, onRowSelected, onPageChange
     } = $props()

    // states
    const isLastPage = $derived(page + 1 === pageCount)
    const emptyRowsCount = $derived(pageSize - displayTransactions.length > 0 ? pageSize - displayTransactions.length : 0)

    let filters = $state({ 
        filtersVisible: false,
        dateFilterText: '', 
        accountFilterText: '', 
        categoryFilterText: '', 
        amountFilterText: '', 
        commentFilterText: ''})

    // helpers
    const getFilters = (): TransactionSearchFilters => {
        const getDate = (dateString: string) => {
            return new Date((new Date()).setTime(new Date(dateString).getTime()+12*60*60*1000))
        } 

        const searchFilters: TransactionSearchFilters = {ownerId: 1, active: true}

        if(filters.dateFilterText.length){
            const dateStrings = filters.dateFilterText.split(',')
            //this is all wrong - welcome fucking timezones and time formats!
            if (dateStrings.length > 0 && !isNaN(Date.parse(dateStrings[0]))) { searchFilters.dateFrom = getDate(dateStrings[0])}
            if (dateStrings.length > 1 && !isNaN(Date.parse(dateStrings[1]))) { searchFilters.dateTo = getDate(dateStrings[1]) }
        }

        if (filters.accountFilterText.length){
            const accountIds = filters.accountFilterText.split(',').filter(x => x.length && !isNaN(+x)).map(x => parseInt(x) )
            if (accountIds.length) { searchFilters.accounts = accountIds }
        }

        if (filters.categoryFilterText.length){
            const categoryIds = filters.categoryFilterText.split(',').filter(x => x.length && !isNaN(+x)).map(x => parseInt(x) )
            if (categoryIds.length) { searchFilters.categories = categoryIds }
        }

        if (filters.amountFilterText.length){
            const amountStrings = filters.amountFilterText.split(',')
            if (amountStrings.length > 0 && !isNaN(+amountStrings[0])) { searchFilters.amountFrom = +amountStrings[0] }
            if (amountStrings.length > 1 && !isNaN(+amountStrings[1])) { searchFilters.amountTo = +amountStrings[1] }
        }

        if (filters.commentFilterText.length){ searchFilters.comment = filters.commentFilterText }
        
        return searchFilters
    }

    const formatDate = (date: Date): string => { 
        const addLeadingZero = (number: number): string =>  number < 10 ? `0${number}` : number.toString()

        return `${addLeadingZero(date.getDate())}/${addLeadingZero(date.getMonth()+1)}/${date.getFullYear()}`
    } 

    const formatAmount = (amount: number): string => { 
        return amount < 0 ? amount.toString() : `+${amount}` 
    }

    // events
    const onRowDoubleClick = (transaction: Transaction, index: number) => { 
        if(selectedRow === index) 
            selectedRow = -1
        else
            selectedRow = index

        onRowSelected(selectedRow, transaction)
    }

    const onFiltersClick = () => { filters.filtersVisible = !filters.filtersVisible }
    const onFiltersChange = (event: any) => { 
        if(event.key !== 'Enter') return

        filters.filtersVisible = false
        onFilterData(getFilters()) 
    }
    const onChangePage = (changeValue: number) => { onPageChange(changeValue) }
</script>

<div class="transaction-table">
    <div id="transaction-table-data" class="data">
        <table>
            <tbody>
                <tr>
                    {#each headers as header}
                    <th class="aln-l">
                        {header}
                        <button class="button-plain" onclick={() => { onFiltersClick() }}>&#x2637;</button>
                    </th>
                    {/each}
                </tr>
                {#if filters.filtersVisible}
                <tr class="test">
                    <td class="aln-l w-20pc"><input type="text" onkeypress={onFiltersChange} bind:value={filters.dateFilterText} placeholder="data: od, do"/></td>
                    <td class="aln-l w-20pc"><input type="text" onkeypress={onFiltersChange} bind:value={filters.accountFilterText} placeholder="konto: id, id"/></td>
                    <td class="aln-l w-20pc"><input type="text" onkeypress={onFiltersChange} bind:value={filters.categoryFilterText} placeholder="kategoria: id, id"/></td>
                    <td class="aln-l w-10pc"><input type="text" onkeypress={onFiltersChange} bind:value={filters.amountFilterText} placeholder="kwota: od, do"/></td>
                    <td class="aln-l w-10pc"><input type="text" readonly placeholder="--" /></td>
                    <td class="aln-l w-20pc"><input type="text" onkeypress={onFiltersChange} bind:value={filters.commentFilterText} placeholder="komentarz"/></td>
                </tr>
                {/if}
                {#each displayTransactions as transaction, index}
                <tr class={selectedRow === index ? 'selected' : ''} ondblclick={() => { onRowDoubleClick(transaction, index) }}>
                    <td class="aln-l w-20pc">{formatDate(transaction.date)}</td>
                    <td class="aln-l w-20pc" title={transaction.accountId.toString()}>{accounts[transaction.accountId]?.name ?? ''}</td>
                    <td class="aln-l w-20pc" title={transaction.categoryId.toString()}>{categories[transaction.categoryId]?.name ?? ''}</td>
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
            </tbody>
        </table>
    </div>
    <div class="actions">
        <div class="buttons">
            <button class="button-outlined" disabled={page === 0} onclick={() => { onChangePage(-1) }}>&#x276E;</button>
            <div>{`${page + 1} / ${pageCount}`}</div>
            <button class="button-outlined" disabled={isLastPage} onclick={() => { onChangePage(1) }}>&#x276F;</button>
        </div>
    </div>
</div>


<style lang="scss">
    @import '../../styles/app.scss';
    
    .transaction-table{
        display: flex;
        flex-direction: column;
        justify-content: space-between;
        height: 100%;

        input { width: 100%;}
        th {
            position: relative;
            button { position: absolute; right: 15px; color: $text-color;}
        }
        
        .red-text { color: $red-text-color; }
        .green-text { color: $green-text-color; }

        .data{ overflow-y: auto; height: 100%; } 
        .actions { margin: 5px; display: flex; align-items: flex-start; justify-content: right;
            .buttons { display: flex; align-items: flex-start; gap: 10px; justify-content: end; div { line-height: $control-min-height; } }
        }
        .selected { background-color: $accent-color-light;}
    }   
</style>