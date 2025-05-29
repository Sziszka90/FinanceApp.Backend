import { Money } from "../Money/Money";

export interface CreateTransactionDto {
    name: string,
    description: string,
    value: Money,
    transactionType: TransactionTypeEnum,
    transactionDate: Date,
    transactionGroupId: string
}
