export interface Transaction {
    ownerId: number,
    id: number,
    date: Date,
    accountId: number,
    categoryId: number,
    amount: number,
    comment: string,
    gid: string // this is an extra field to mark multiple connected transactions (like transfers)
    active: boolean
}

export function GetEmptyTransaction() : Transaction {
    return {
        ownerId: 1,
        id: 0,
        date: new Date(),
        accountId: 0,
        amount: 0,
        categoryId: 0,
        comment: '',
        gid: '',
        active: true
    }
}