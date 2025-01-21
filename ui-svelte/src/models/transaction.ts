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

export function CopyTransaction(sourceTransaction: Transaction): Transaction {
    return {
        ownerId: sourceTransaction.ownerId,
        id: sourceTransaction.id,
        date: structuredClone(sourceTransaction.date),
        accountId: sourceTransaction.accountId,
        amount: sourceTransaction.amount,
        categoryId: sourceTransaction.categoryId,
        comment: sourceTransaction.comment,
        gid: sourceTransaction.gid,
        active: sourceTransaction.active
    }
}