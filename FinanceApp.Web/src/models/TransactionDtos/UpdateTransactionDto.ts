import { TransactionTypeEnum } from "../Enums/TransactionType.enum";
import { Money } from "../Money/Money";

export interface UpdateTransactionDto {
    id: string,
    name: string,
    description?: string,
    value: Money,
    transactionType: TransactionTypeEnum,
    transactionDate?: Date,
    transactionGroupId: string
}
