<script lang="ts">
	import { onMount } from 'svelte';
    import AccountTable from '../components/account-table.svelte';
    import Card from '../components/card.svelte';
	import CategoryTable from '../components/category-table.svelte';
	import { initDataStore } from '../services/store';
	import TransactionTable from '../components/transaction-table.svelte';

    let dataLoaded = false

    onMount(async () => { 
        try{
            await initDataStore(1,true)
            dataLoaded = true
        }
        catch(error){
            dataLoaded = false
        }
    })
    // LOADING animation!

</script>

<div class="test">
    {#if dataLoaded}
    <div class="transactions">
        <Card>
            <TransactionTable></TransactionTable>
        </Card>
    </div>
    <div class="accounts">
        <Card>
            <AccountTable></AccountTable>
        </Card>
    </div>
    <div class="categories">
        <Card>
            <CategoryTable></CategoryTable>
        </Card>
    </div>
    {/if}
</div>

<style lang="scss">
    @import "../styles/app.scss";

    //reset all
    //:global(body) { margin: 0; padding: 0; font-family: "Source Code Pro", monospace; font-optical-sizing: auto; font-weight: 500; font-style: normal; }
    :global(body) { margin: 0; padding: 0; font-family: "Reddit Mono", monospace; font-optical-sizing: auto; font-weight: 500; font-style: normal; }
    
	:global(*) { box-sizing: border-box; background-color: transparent;	margin: 0; padding: 0;  
                 font-family: inherit; font-optical-sizing: inherit; font-weight: inherit; font-style: inherit; }

    //set global styles
    :global(html) { margin: 0; height: 100vh; background-color: $background-color; }
    :global(body) { margin: 0; display: flex; place-items: center; min-width: 320px; height: 100vh; background-color: $background-color; } 

    // main page styles
    $header-size: 60px;
    $side-size: 200px;
    $grid-row-size: calc(50vh - 8px - 16px); // recalculate it again! (to do)

    .test { 
            background-color: lightgray;
            display: grid; 
            grid-template-columns: 2.5fr 1fr;
            grid-template-rows: $grid-row-size $grid-row-size;
            grid-template-areas: 
            "transactions accounts"
            "transactions categories";
            gap: 16px;
            position: relative;
            margin: 0;
            padding: 16px;
            width: calc(100vw - 0px);
            height: calc(100vh - 0px);

            .transactions{ grid-area: transactions; }
            .transaction-input { grid-area: trans-input; }
            .accounts { grid-area: accounts }
            .categories { grid-area: categories }
    }

    // @media (width <= $mobile-width) {
    //     .test { 
    //         grid-template-columns: 1fr;
    //         grid-template-rows: calc(50vh - 8px - 16px) calc(50vh - 8px - 16px) ;
    //         grid-template-areas: 
    //         "transactions"
    //         "transactions";

    //         .accounts { display: none;}
    //         .categories { display: none; }
    //     }
    // }    

    .main-page {
        // * {
        //     -webkit-transition: all 0.1s ease-in-out;
        //     -moz-transition: all 0.1s ease-in-out;
        //     -o-transition: all 0.1s ease-in-out;
        //     transition: all 0.1s ease-in-out;
        // }
    }
</style>

