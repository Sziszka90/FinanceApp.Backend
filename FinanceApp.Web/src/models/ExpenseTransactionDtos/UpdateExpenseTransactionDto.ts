import { Money } from "../Money/Money";


export interface UpdateExpenseTransactionDto {
    id: string,
    name: string,
    description: string,
    value: Money,
    dueDate: Date,
    transactionGroupId: string
}
