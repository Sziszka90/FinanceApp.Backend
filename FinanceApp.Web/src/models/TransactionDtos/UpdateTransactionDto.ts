import { Money } from "../Money/Money";

export interface UpdateTransactionDto {
    id: string,
    name: string,
    description?: string,
    value: Money,
    transactionDate: Date,
    transactionGroupId: string
}
