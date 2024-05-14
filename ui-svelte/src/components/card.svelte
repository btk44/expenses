<div class="card">
    <slot/>
</div>

<style lang="scss">
    @import '../styles/app.scss';

    .card {
        background-color: $background-color;
        padding: 5px 5px 20px 5px;
        box-shadow: $control-box-shadow;
        display: flex;
        flex-direction: column;
        gap: 10px;
        width: 100%;
        height: 100%;
        position: relative;
    }
</style>