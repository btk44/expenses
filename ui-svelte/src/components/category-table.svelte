<script lang="ts">
	import { type Category } from "../models/category";
	import { categoryStoreReadOnly } from "../services/store";
    
    const headers = ['id', 'nazwa'] 
    const categories = (() => { 
        const originalCategories = $categoryStoreReadOnly
        const originalCategoriesList = Object.entries(originalCategories).map(([_,v])=> v)
        let result: Array<Category> = []
        originalCategoriesList.filter(x => x.parentId === 0).forEach(x => {
            result.push(x)
            result = result.concat(originalCategoriesList.filter(y => y.parentId === x.id))
        })
        
        return result
    })()
</script>

<div class="data">
    <table>
        <tr>
            <th colspan="2" class="aln-l">{headers[0]}</th>
            <th class="aln-l">{headers[1]}</th>
        </tr>
        {#each categories as category}
        <tr>
            <td class="aln-c w-15pc">{@html category.parentId > 0 ? '.' : category.id}</td>
            <td class="aln-c w-15pc">{@html category.parentId > 0 ? category.id : '.'}</td>
            <td class="aln-l w-70pc">{category.name}</td>
        </tr>
        {/each}
    </table>
</div>

<style lang="scss">
    @import '../styles/app.scss';
    
    .data{ overflow-y: auto; height: 100%; } 
</style>