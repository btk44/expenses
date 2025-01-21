import type { Account } from '../../models/account'
import type { Category } from '../../models/category'

export interface processOutput {
    value: number,
    isError: boolean
}

export const isNumber = (input: string): boolean => { return !isNaN(+input) }

export const processAccountInput = (inputValue: string | undefined | null, accounts: Array<Account>): processOutput => {
    if(!inputValue)
        return {isError: false, value: 0}
    
    let accountId: number | null = null
    if(isNumber(inputValue)) accountId = +inputValue
    if(accountId != null && accounts[accountId])
        return {isError: false, value: accountId}

    return {isError: true, value: 0}
}

export const processCategoryInput = (inputValue: string | undefined | null, categories: Array<Category>): processOutput  => { 
    if(!inputValue)
        return {isError: false, value: 0}

    let categoryId: number | null = null
    if(isNumber(inputValue)) categoryId = +inputValue
    if(categoryId != null && categories[categoryId])
        return {isError: false, value: categoryId}

        return {isError: true, value: 0}
}

export const processAmountInput = (inputValue: string | undefined | null): processOutput  => { 
    if(!inputValue)
        return {isError: false, value: 0}

    if(isNumber(inputValue))
        return {isError: false, value: +inputValue}
    
        return {isError: true, value: 0}
}