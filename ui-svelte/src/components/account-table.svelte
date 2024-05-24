<script lang="ts">
	import { type Account } from '../models/account';
    import { accountStoreReadOnly, accountCurrencyMap, reloadAccount } from '../services/store'
	import AccountBalancer from './account-balancer.svelte';
  
    const headers = ['id', 'nazwa', 'kwota', '-'] 
    const accountCurrency = accountCurrencyMap()

    let selectedRow = -1
    let accounts = $accountStoreReadOnly
    accountStoreReadOnly.subscribe((data) => {accounts = data})

    const formatAmount = (amount: number): string => { 
        return amount < 0 ? amount.toString() : `${amount}` 
    }

    const recalculateAccountAmounts = async (accountId: number) => { 
        try { await reloadAccount(1, accountId) }
        catch { alert('server error') }
        finally { selectedRow = -1 }
    }


    let accountToBalance: Account = null
    const openAccountBalancer = (account: Account) => {
        accountToBalance = account
    }
</script>

<div class="accounts">
    {#if accountToBalance == null}
    <div class="data">
        <table>
            <tr>
                {#each headers as header}
                <th class="aln-l">{header}</th>
                {/each}
            </tr>
            {#each Object.entries(accounts) as [accountId, account], index}
            {#if account.active}
            <!-- <tr class={selectedRow === index ? 'selected' : ''} on:dblclick={() => { selectedRow = index; recalculateAccountAmounts(+accountId) }}> -->
            <tr class={selectedRow === index ? 'selected' : ''} on:dblclick={() => { openAccountBalancer(account) }}>
                <td class="aln-c w-10pc">{accountId}</td>
                <td class="aln-l w-50pc">{account.name}</td>
                <td class="aln-r w-30pc">{formatAmount(account.amount)}</td>
                <td class="aln-l w-10pc">{accountCurrency[accountId] ?? ''}</td>
            </tr>
            {/if}
            {/each}
        </table>
    </div>
    {/if}
    {#if accountToBalance != null}
    <div class="account-balancer">
        <AccountBalancer account={accountToBalance}></AccountBalancer>
    </div>
    {/if}
</div>

<style lang="scss">
    @import '../styles/app.scss';

    .accounts{
        .data{ 
            overflow-y: auto; height: 100%;
            .selected { background-color: $accent-color-light;}        
        }
        .account-balancer { width: 100%; height: 100%; background-color: white; }
    }

</style>