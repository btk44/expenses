export interface AccountSearchFilters {
    ownerId?: number
    currencies?: Array<number>
    name?: string
    amountFrom?: number
    amountTo?: number
    id?: number
    active?: boolean
}