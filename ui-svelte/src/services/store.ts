import { get, readonly, writable } from 'svelte/store'
import { type Account } from '../models/account'
import { type Category } from '../models/category'
import { type Currency } from '../models/currency'
import { TransactionService } from './transaction-service'

const LocalStorage = typeof window !== 'undefined' ? localStorage : null

const loadFromLocalStorage = (key: string, additionalActionOnValue: any = null) => {
    if(!LocalStorage) return null

    const localStorageString = LocalStorage.getItem(key)

    if (localStorageString === null) return null 

    let value = JSON.parse(localStorageString)
    if(additionalActionOnValue !== null)
        value = additionalActionOnValue(value)

    return value
}
  
const saveToLocalStorage = (key: string, value: any) => {
    if(!LocalStorage) return

    LocalStorage.setItem(key, JSON.stringify(value));
}
  
const removeFromLocalStorage = (key: string) => {
    if(!LocalStorage) return

    LocalStorage.removeItem(key);
}

const getLastInitTime = () => { 
    return loadFromLocalStorage('last-init', (dateStr: string) => { return new Date(dateStr) }) 
}

const accountStore = writable<Array<Account>>(loadFromLocalStorage('accounts') ?? {})
const categoryStore = writable<Array<Category>>(loadFromLocalStorage('categories') ?? {})
const currencyStore = writable<Array<Currency>>(loadFromLocalStorage('currencies') ?? {})
const lastInitStore = writable<Date>(getLastInitTime() ?? new Date(2000, 1, 1))

export const accountStoreReadOnly = readonly(accountStore)
export const categoryStoreReadOnly = readonly(categoryStore)
export const currencyStoreReadOnly = readonly(currencyStore)

accountStore.subscribe((v) => { saveToLocalStorage('accounts', v) })
categoryStore.subscribe((v) => { saveToLocalStorage('categories', v) })
currencyStore.subscribe((v) => { saveToLocalStorage('currencies', v) })
lastInitStore.subscribe((v) => { saveToLocalStorage('last-init', v) })

export const initDataStore = async (ownerId: number, forceReload?: boolean) => {
    let lastInit = getLastInitTime()
    let now = new Date()
    let timePastLastInitInMin = (+now - +lastInit) / 1000 / 60
  
    if(timePastLastInitInMin < 15 && !forceReload){
        return await Promise.resolve()
    }
  
    const accountsCall = TransactionService.SearchAccounts({ownerId: ownerId, active: true})
    const categoryCall = TransactionService.SearchCategories({ownerId: ownerId, active: true})
    const currencyCall = TransactionService.SearchCurrencies({active: true})
  
    try{
      const [accountsResponse, categoriesResponse, currencyResponse] = await Promise.all([accountsCall, categoryCall, currencyCall])
      accountStore.set(Object.assign({}, ...accountsResponse.map((x: Account) => ({[x.id]: x}))))
      categoryStore.set(Object.assign({}, ...categoriesResponse.map((x: Category) => ({[x.id]: x}))))
      currencyStore.set(Object.assign({}, ...currencyResponse.map((x: Currency) => ({[x.id]: x}))))

      lastInitStore.set(new Date())
    }
    catch(error){
        console.log('There was an error loading data to the store!')
    }
}

export const reloadAccount = async (ownerId: number, accountId: number) => {
    try{
        await TransactionService.CalculateAccountsAmount({ownerId: ownerId, accounts: [accountId]})
        const accountsResponse = await TransactionService.SearchAccounts({ownerId: ownerId, id: accountId})
        await accountStore.update(data => { 
            data[accountId] = accountsResponse[0]
            return data
        })
    } 
    catch(error){
        console.log('There was an error loading account data!')
    }
}

export const accountCurrencyMap = () => {
    let accounts = get(accountStoreReadOnly)
    let accountToCurrencyMap = {}
    let currencyDictionary = get(currencyStoreReadOnly)
    Object.keys(accounts).forEach(accKey => { accountToCurrencyMap[accKey] = currencyDictionary[accounts[accKey].currencyId].code })
    return accountToCurrencyMap
}

