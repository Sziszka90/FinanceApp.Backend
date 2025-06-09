import { TransactionTypeEnum } from "../Enums/TransactionType.enum";
import { Money } from "../Money/Money";

export interface CreateTransactionDto {
    name: string,
    description: string,
    value: Money,
    transactionDate: Date,
    transactionType: TransactionTypeEnum,
    transactionGroupId?: string
}
