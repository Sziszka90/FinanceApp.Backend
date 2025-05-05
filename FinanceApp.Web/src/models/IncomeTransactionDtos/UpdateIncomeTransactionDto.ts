import { Money } from "./Money";

export interface UpdateIncomeTransactionDto {
    id: string,
    name: string,
    description: string,
    value: Money,
    dueDate: Date,
    transactionGroupId: string
}
