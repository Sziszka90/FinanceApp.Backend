import { Money } from "./Money";

export interface CreateIncomeTransactionDto {
    name: string,
    description: string,
    value: Money,
    dueDate: Date,
    transactionGroupId: string
}
