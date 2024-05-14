export interface TransactionSearchFilters {
    dateFrom?: Date
    dateTo?: Date
    id?: number
    ownerId?: number
    amountFrom?: number
    amountTo?: number
    categories?: Array<number>
    comment?: string  
    active?: boolean
    accounts?: Array<number>
    take?: number
    offset?: number
  }