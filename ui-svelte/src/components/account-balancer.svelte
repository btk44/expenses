<script lang="ts">
	import { type Account } from "../models/account";

    export let account: Account
    let realAmount = 0
    let cash = 0
    $: missingAmount = Math.round((+account.amount - (+realAmount + +cash)) *100) / 100
</script>

<div class="account-balancer">
    <div class="control-group">
        <label for="calculatedAmount">stan w aplikacji:</label>
        <input type="text" name="calculatedAmount" readonly bind:value={account.amount}/>
    </div>
    <div class="control-group">
        <label for="realAmount">fizycznie na koncie:</label>
        <input type="text" name="realAmount" bind:value={realAmount}/>
    </div>
    <div class="control-group">
        <label for="cashAmount">gotówka:</label>
        <input type="text" name="cashAmount" bind:value={cash}/>
    </div>
    <div class="control-group">
        <label for="missingAmount">manko:</label>
        <input type="text" name="missingAmount" readonly bind:value={missingAmount}/>
    </div>
    <div>
        <input type="text" />
        <button class="button-outlined">OK</button>
    </div>
</div>

<style lang="scss">
    @import '../styles/app.scss';

    .account-balancer { 
        display: flex;  flex-direction: column; gap: 10px; justify-content: space-between;
        border: 3px solid $primary-color;
        padding: 10px;
        .control-group { 
            display: flex; justify-content: start; flex-direction: column;
            label { margin: 0 5px; }
        }
    }
</style>