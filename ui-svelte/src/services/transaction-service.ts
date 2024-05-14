import { type AccountAmountCalculateParameters } from '../models/account-amount-calculation-parameters'
import { type AccountSearchFilters } from '../models/account-search-filters'
import { type CategorySearchFilters } from '../models/cartegory-search-filters'
import { type CurrencySearchFilters } from '../models/currency-search-filters'
import { type Transaction } from '../models/transaction'
import { type TransactionSearchFilters } from '../models/transaction-search-filters'

export const TransactionService = {
    apiUrl: 'http://localhost:5000/api', // windows
    apiUrl1: 'https://localhost:5001/api', // linux
    transactionUrl: 'Transaction',
    accountUrl: 'Account',
    categoryUrl: 'Category',
    currencyUrl: 'Currency',

    SearchTransactions: async function(filters?: TransactionSearchFilters) {    
        try{
            const response = await this.Post(`${this.apiUrl}/${this.transactionUrl}/search`, { ...(filters || {}) })
            return response.map((t: Transaction) => { return { ...t, date: new Date(t.date)} })
        } catch(e) {
            console.log('Transaction search failed')
            throw e
        }
    },

    SearchTransactionsCount: async function(filters?: TransactionSearchFilters) {    
        try{
            return await this.Post(`${this.apiUrl}/${this.transactionUrl}/search-count`, { ...(filters || {}) })
        } catch(e) {
            console.log('Transaction search count failed')
            throw e
        }
    },

    SearchTransactionsSum: async function(filters?: TransactionSearchFilters) {    
        try{
            return await this.Post(`${this.apiUrl}/${this.transactionUrl}/search-sum`, { ...(filters || {}) })
        } catch(e) {
            console.log('Transaction search sum failed')
            throw e
        }
    },

    SaveTransactions: async function(transactions: Array<Transaction>) {    
        try{
            const processInput = { transactions: transactions }
            const response = await fetch(`${this.apiUrl}/${this.transactionUrl}/process`, 
                                        { method: 'POST', body: JSON.stringify(processInput), headers: this.GetHeaders() })
            return await response.json()
        } catch(e) {
            console.log('Transaction save failed')
            throw e
        }
    },

    SearchAccounts: async function(filters?: AccountSearchFilters) {    
        try{
            return await this.Post(`${this.apiUrl}/${this.accountUrl}/search`, { ...(filters || {}) })
        } catch(e) {
            console.log('Account search failed')
            throw e
        }
    },

    CalculateAccountsAmount: async function(filters?: AccountAmountCalculateParameters) {    
        try{
            return await this.Post(`${this.apiUrl}/${this.accountUrl}/calculate-amount`, { ...(filters || {}) })
        } catch(e) {
            console.log('Account amount calculation failed')
            throw e
        }
    },

    SearchCategories: async function(filters?: CategorySearchFilters) {    
        try{
            return await this.Post(`${this.apiUrl}/${this.categoryUrl}/search`, { ...(filters || {}) })
        } catch(e) {
            console.log('Category search failed')
            throw e
        }
    },

    SearchCurrencies: async function(filters?: CurrencySearchFilters) {    
        try{
            return await this.Post(`${this.apiUrl}/${this.currencyUrl}/search`, { ...(filters || {}) })
        } catch(e) {
            console.log('Currency search failed')
            throw e
        }
    },

    Post: async function(url: string, filters: any) {
        const response = await fetch(`${url}`, { method: 'POST', body: JSON.stringify(filters), headers: this.GetHeaders() })
        return await response.json()
    },

    Get: async function(url: string) {
        const response = await fetch(`${url}`, { method: 'GET', headers: this.GetHeaders() })
        return await response.json()
    },

    GetHeaders: function(): Headers {
        const headers = new Headers()
        headers.append('Authorization', 'Bearer to.ke.n')
        headers.append('Access-Control-Allow-Origin', '*')
        headers.append('Content-Type', 'application/json')
        headers.append('accept', 'text/plain')
        
        return headers;
    }
}